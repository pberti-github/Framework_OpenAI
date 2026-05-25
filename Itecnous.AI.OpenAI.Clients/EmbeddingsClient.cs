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

public class EmbeddingsClient : IEmbeddingsClient
{
	private readonly OpenAISettings _settings;

	public EmbeddingsClient(OpenAISettings settings)
	{
		_settings = settings;
	}

	public async Task<EmbeddingResponse> CreateAsync(string input, string? model = null, int? dimensions = null, CancellationToken ct = default(CancellationToken))
	{
		return await ExecuteInternalAsync(input, model, dimensions, ct);
	}

	public async Task<EmbeddingResponse> CreateBatchAsync(IEnumerable<string> inputs, string? model = null, int? dimensions = null, CancellationToken ct = default(CancellationToken))
	{
		return await ExecuteInternalAsync(inputs, model, dimensions, ct);
	}

	private async Task<EmbeddingResponse> ExecuteInternalAsync(object input, string? model = null, int? dimensions = null, CancellationToken ct = default(CancellationToken))
	{
		string text = model ?? _settings.Embeddings.DefaultModel;
		int? dimensions2 = dimensions ?? _settings.Embeddings.DefaultDimensions;
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
				throw new OpenAIClientException(text3 ?? "Error al generar embedding.", resp.StatusCode, openAIErrorCode, text2);
			}
			EmbeddingResponse embeddingResponse = JsonConvert.DeserializeObject<EmbeddingResponse>(text2);
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
