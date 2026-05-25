using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Itecnous.AI.OpenAI.Abstractions;
using Itecnous.AI.OpenAI.Common;
using Itecnous.AI.OpenAI.Errors;
using Itecnous.AI.OpenAI.Models.Files;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Clients;

public class FilesClient : IFilesClient
{
	private readonly OpenAISettings _settings;

	public FilesClient(OpenAISettings settings)
	{
		_settings = settings;
	}

	public async Task<FileUploadResponse> UploadAsync(string filePath, string purpose, Dictionary<string, string>? metadata = null, int? expiresAfterSeconds = null, string expiresAfterAnchor = "created_at", CancellationToken ct = default(CancellationToken))
	{
		_ = 2;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings);
			using MultipartFormDataContent form = new MultipartFormDataContent();
			form.Add(new StringContent(purpose), "purpose");
			if (expiresAfterSeconds.HasValue && expiresAfterSeconds.Value > 0)
			{
				form.Add(new StringContent(expiresAfterAnchor), "expires_after[anchor]");
				form.Add(new StringContent(expiresAfterSeconds.Value.ToString()), "expires_after[seconds]");
			}
			ByteArrayContent content = new ByteArrayContent(await File.ReadAllBytesAsync(filePath, ct));
			form.Add(content, "file", Path.GetFileName(filePath));
			if (metadata != null)
			{
				foreach (KeyValuePair<string, string> metadatum in metadata)
				{
					form.Add(new StringContent(metadatum.Value), "metadata[" + metadatum.Key + "]");
				}
			}
			HttpResponseMessage resp = await http.PostAsync("files", form, ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(text2 ?? "Error al subir archivo.", resp.StatusCode, openAIErrorCode, text);
			}
			return JsonConvert.DeserializeObject<FileUploadResponse>(text);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al subir archivo.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	public async Task<FileData> GetAsync(string fileId, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings);
			HttpResponseMessage resp = await http.GetAsync("files/" + fileId, ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(text2 ?? "Error al obtener archivo.", resp.StatusCode, openAIErrorCode, text);
			}
			return JsonConvert.DeserializeObject<FileData>(text);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al obtener archivo.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	public async Task<FilesList> ListAsync(CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings);
			HttpResponseMessage resp = await http.GetAsync("files", ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(text2 ?? "Error al listar archivos.", resp.StatusCode, openAIErrorCode, text);
			}
			return JsonConvert.DeserializeObject<FilesList>(text);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al listar archivos.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	public async Task<bool> DeleteAsync(string fileId, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings);
			HttpResponseMessage resp = await http.DeleteAsync("files/" + fileId, ct);
			if (!resp.IsSuccessStatusCode)
			{
				string text = await resp.Content.ReadAsStringAsync(ct);
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(text2 ?? "Error al eliminar archivo.", resp.StatusCode, openAIErrorCode, text);
			}
			return true;
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al eliminar archivo.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	public async Task<byte[]> DownloadContentAsync(string fileId, CancellationToken ct = default(CancellationToken))
	{
		_ = 2;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings);
			HttpResponseMessage resp = await http.GetAsync("files/" + fileId + "/content", ct);
			if (!resp.IsSuccessStatusCode)
			{
				string text = await resp.Content.ReadAsStringAsync(ct);
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(text2 ?? "Error al descargar contenido.", resp.StatusCode, openAIErrorCode, text);
			}
			return await resp.Content.ReadAsByteArrayAsync(ct);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al descargar contenido.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}
}
