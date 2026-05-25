using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Itecnous.AI.OpenAI.Abstractions;
using Itecnous.AI.OpenAI.Common;
using Itecnous.AI.OpenAI.Errors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Itecnous.AI.OpenAI.Clients;

/// <summary>
/// Cliente de bajo nivel para la API de audio.
/// </summary>
public class AudioClient : IAudioClient
{
	private readonly OpenAISettings _settings;

	public AudioClient(OpenAISettings settings)
	{
		_settings = settings;
	}

	/// <summary>
	/// Transcribe un archivo de audio con Whisper.
	/// </summary>
	public async Task<string> TranscribeAsync(string filePath, CancellationToken ct = default(CancellationToken))
	{
		_ = 2;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings);
			using MultipartFormDataContent form = new MultipartFormDataContent();
			form.Add(new StringContent("whisper-1"), "model");
			ByteArrayContent byteArrayContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath, ct));
			byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
			form.Add(byteArrayContent, "file", Path.GetFileName(filePath));
			HttpResponseMessage resp = await http.PostAsync("audio/transcriptions", form, ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				if (string.IsNullOrWhiteSpace(text2))
				{
					text2 = "Error al transcribir audio.";
				}
				throw new OpenAIClientException(text2, resp.StatusCode, openAIErrorCode, text);
			}
			JObject val = JObject.Parse(text);
			JToken? token = val["text"];
			if (token == null)
			{
				throw new OpenAIClientException("Respuesta invalida al transcribir audio.", HttpStatusCode.InternalServerError, null, text);
			}
			string value = token.ToString();
			if (string.IsNullOrWhiteSpace(value))
			{
				throw new OpenAIClientException("Respuesta invalida al transcribir audio.", HttpStatusCode.InternalServerError, null, text);
			}
			return value;
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al transcribir audio.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}
}
