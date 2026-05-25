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
using Itecnous.AI.OpenAI.Models.VectorStores;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Clients;

public class VectorStoresClient : IVectorStoresClient
{
	private readonly OpenAISettings _settings;

	private static readonly string[] _betaHeaders = new string[1] { "assistants=v2" };

	public VectorStoresClient(OpenAISettings settings)
	{
		_settings = settings;
	}

	public async Task<VectorStore> CreateAsync(string name, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			var value = new { name };
			HttpResponseMessage resp = await http.PostAsJsonAsync("vector_stores", value, ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(text2 ?? "Error al crear vector store.", resp.StatusCode, openAIErrorCode, text);
			}
			return JsonConvert.DeserializeObject<VectorStore>(text);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al crear vector store.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	public async Task<VectorStoresList> ListAsync(CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			HttpResponseMessage resp = await http.GetAsync("vector_stores", ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(text2 ?? "Error al listar vector stores.", resp.StatusCode, openAIErrorCode, text);
			}
			return JsonConvert.DeserializeObject<VectorStoresList>(text);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al listar vector stores.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	public async Task<VectorStore> RetrieveAsync(string id, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			HttpResponseMessage resp = await http.GetAsync("vector_stores/" + id, ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(text2 ?? "Error al recuperar vector store.", resp.StatusCode, openAIErrorCode, text);
			}
			return JsonConvert.DeserializeObject<VectorStore>(text);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al recuperar vector store.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	public async Task<VectorStoreFile> AddFileAsync(string vectorStoreId, string fileId, object? attributes = null, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			object value = ((attributes != null) ? ((object)new
			{
				file_id = fileId,
				attributes = attributes
			}) : ((object)new
			{
				file_id = fileId
			}));
			HttpResponseMessage resp = await http.PostAsJsonAsync("vector_stores/" + vectorStoreId + "/files", value, ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(text2 ?? "Error al agregar archivo al vector store.", resp.StatusCode, openAIErrorCode, text);
			}
			return JsonConvert.DeserializeObject<VectorStoreFile>(text);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al agregar archivo al vector store.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	public async Task<VectorStoreFile> RetrieveFileAsync(string vectorStoreId, string fileId, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			HttpResponseMessage resp = await http.GetAsync("vector_stores/" + vectorStoreId + "/files/" + fileId, ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(text2 ?? "Error al recuperar archivo del vector store.", resp.StatusCode, openAIErrorCode, text);
			}
			return JsonConvert.DeserializeObject<VectorStoreFile>(text);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al recuperar archivo del vector store.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	public async Task<bool> UpdateFileAttributesAsync(string vectorStoreId, string fileId, Dictionary<string, object> attributes, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			string requestUri = "vector_stores/" + vectorStoreId + "/files/" + fileId;
			var inputValue = new { attributes };
			HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
			{
				Content = JsonContent.Create(inputValue)
			};
			HttpResponseMessage resp = await http.SendAsync(request, ct);
			if (!resp.IsSuccessStatusCode)
			{
				string text = await resp.Content.ReadAsStringAsync(ct);
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(text2 ?? "Error al actualizar atributos del archivo del vector store.", resp.StatusCode, openAIErrorCode, text);
			}
			return true;
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al actualizar atributos del archivo del vector store.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	public async Task<bool> RemoveFileAsync(string vectorStoreId, string fileId, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			HttpResponseMessage resp = await http.DeleteAsync("vector_stores/" + vectorStoreId + "/files/" + fileId, ct);
			if (!resp.IsSuccessStatusCode)
			{
				string text = await resp.Content.ReadAsStringAsync(ct);
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(text2 ?? "Error al desadjuntar archivo del vector store.", resp.StatusCode, openAIErrorCode, text);
			}
			return true;
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al desadjuntar archivo del vector store.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	public async Task<VectorStoreFilesList> ListFilesAsync(string vectorStoreId, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			HttpResponseMessage resp = await http.GetAsync("vector_stores/" + vectorStoreId + "/files", ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(text2 ?? "Error al listar archivos del vector store.", resp.StatusCode, openAIErrorCode, text);
			}
			return JsonConvert.DeserializeObject<VectorStoreFilesList>(text);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al listar archivos del vector store.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	public async Task<bool> DeleteAsync(string vectorStoreId, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			HttpResponseMessage resp = await http.DeleteAsync("vector_stores/" + vectorStoreId, ct);
			if (!resp.IsSuccessStatusCode)
			{
				string text = await resp.Content.ReadAsStringAsync(ct);
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(text2 ?? "Error al eliminar vector store.", resp.StatusCode, openAIErrorCode, text);
			}
			return true;
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al eliminar vector store.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}
}
