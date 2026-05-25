using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Itecnous.AI.OpenAI.Abstractions;
using Itecnous.AI.OpenAI.Common;
using Itecnous.AI.OpenAI.Errors;
using Itecnous.AI.OpenAI.Models.Embeddings;
using Itecnous.AI.OpenAI.Utils;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Clients;

/// <summary>
/// Cliente de bajo nivel para la API de Embeddings.
/// </summary>
public class EmbeddingsClient : IEmbeddingsClient
{
	private readonly OpenAISettings _settings;

	public EmbeddingsClient(OpenAISettings settings)
	{
		_settings = settings;
	}

	private static string ObtenerMensajeError(string? mensaje, string mensajePorDefecto)
	{
		if (!string.IsNullOrWhiteSpace(mensaje))
		{
			return mensaje;
		}
		return mensajePorDefecto;
	}

	private static T DeserializarRequerido<T>(string texto, HttpStatusCode statusCode, string mensajePorDefecto, string cuerpo) where T : class
	{
		T? data = JsonConvert.DeserializeObject<T>(texto);
		if (data == null)
		{
			throw new OpenAIClientException(mensajePorDefecto, statusCode, null, cuerpo);
		}
		return data;
	}

	/// <summary>
	/// Genera un embedding para un texto de entrada.
	/// </summary>
	public async Task<EmbeddingResponse> CreateAsync(string input, string? model = null, int? dimensions = null, CancellationToken ct = default(CancellationToken))
	{
		return await ExecuteInternalAsync(input, model, dimensions, ct);
	}

	/// <summary>
	/// Genera embeddings para una coleccion de textos.
	/// </summary>
	public async Task<EmbeddingResponse> CreateBatchAsync(IEnumerable<string> inputs, string? model = null, int? dimensions = null, CancellationToken ct = default(CancellationToken))
	{
		return await ExecuteInternalAsync(inputs, model, dimensions, ct);
	}

	private async Task<EmbeddingResponse> ExecuteInternalAsync(object input, string? model = null, int? dimensions = null, CancellationToken ct = default(CancellationToken))
	{
		string? text = model;
		if (string.IsNullOrWhiteSpace(text))
		{
			text = _settings.Embeddings.DefaultModel;
		}
		int? dimensions2 = dimensions;
		if (!dimensions2.HasValue)
		{
			dimensions2 = _settings.Embeddings.DefaultDimensions;
		}
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, null, text);
			EmbeddingRequest value = new EmbeddingRequest
			{
				Input = input,
				Model = text,
				Dimensions = dimensions2
			};
			string requestUri = ((_settings.Provider == OpenAIProvider.Azure) ? ("embeddings?api-version=" + _settings.AzureApiVersion) : "embeddings");
			HttpResponseMessage resp = await http.PostAsJsonAsync(requestUri, value, ct);
			string text2 = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text3, openAIErrorCode) = OpenAIErrorParser.TryParse(text2);
				throw new OpenAIClientException(ObtenerMensajeError(text3, "Error al generar embedding."), resp.StatusCode, openAIErrorCode, text2);
			}
			EmbeddingResponse embeddingResponse = DeserializarRequerido<EmbeddingResponse>(text2, resp.StatusCode, "Respuesta invalida.", text2);
			if (_settings.Embeddings.NormalizarVectores && embeddingResponse.Data != null)
			{
				foreach (EmbeddingData datum in embeddingResponse.Data)
				{
					datum.Embedding = VectorUtils.NormalizeL2(datum.Embedding);
				}
			}
			return embeddingResponse;
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al generar embedding.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}
}
