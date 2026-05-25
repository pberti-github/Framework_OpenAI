using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gestion.EF;
using Itecnous.AI.OpenAI.Abstractions;
using Itecnous.AI.OpenAI.Errors;
using Itecnous.AI.OpenAI.Models.Assistants;
using Itecnous.AI.OpenAI.Models.Embeddings;
using Itecnous.AI.OpenAI.Models.Files;
using Itecnous.AI.OpenAI.Models.Responses;
using Itecnous.AI.OpenAI.Models.VectorStores;

namespace Itecnous.AI.OpenAI.Services;

public class OpenAIResultService
{
	private readonly IAssistantsClient _assistants;

	private readonly IResponsesClient _responses;

	private readonly IFilesClient _files;

	private readonly IVectorStoresClient _vectors;

	private readonly IAudioClient _audio;

	private readonly IEmbeddingsClient _embeddings;

	private readonly TokenCounterService _tokenCounter;

	private readonly PricingService _pricing;

	public OpenAIResultService(IAssistantsClient assistants, IResponsesClient responses, IFilesClient files, IVectorStoresClient vectors, IAudioClient audio, IEmbeddingsClient embeddings, TokenCounterService tokenCounter, PricingService pricing)
	{
		_assistants = assistants;
		_responses = responses;
		_files = files;
		_vectors = vectors;
		_audio = audio;
		_embeddings = embeddings;
		_tokenCounter = tokenCounter;
		_pricing = pricing;
	}

	public async Task<ReturnData> Assistants_AskAgentAsync(string assistantId, string userContent, string? systemInstructions = null, ResponseFormat? responseFormat = null, double? temperature = null, int? maxOutputTokens = null, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			ThreadCreateResponse thread = await _assistants.CreateThreadAsync(ct);
			await _assistants.AddMessageAsync(thread.Id, "user", userContent, null, ct);
			RunResponse run = await _assistants.CreateRunAsync(thread.Id, assistantId, systemInstructions, responseFormat, null, temperature, maxOutputTokens, null, ct);
			DateTime start = DateTime.UtcNow;
			TimeSpan timeout = TimeSpan.FromMinutes(2.0);
			TimeSpan delay = TimeSpan.FromMilliseconds(800.0);
			string text = null;
			int? promptTokens = 0;
			int? completionTokens = 0;
			RunResponse lastRun = null;
			while (DateTime.UtcNow - start < timeout)
			{
				RunResponse runResponse = await _assistants.GetRunAsync(thread.Id, run.Id, ct);
				if (runResponse.Status == "completed")
				{
					RunStep runStep = (await _assistants.GetRunStepsAsync(thread.Id, run.Id, ct)).Data.FirstOrDefault();
					promptTokens = runStep?.Usage?.PromptTokens;
					completionTokens = runStep?.Usage?.CompletionTokens;
					text = await _assistants.GetLastMessageTextAsync(thread.Id, ct);
					break;
				}
				bool flag;
				switch (runResponse.Status)
				{
				case "failed":
				case "cancelled":
				case "expired":
					flag = true;
					break;
				default:
					flag = false;
					break;
				}
				if (flag)
				{
					break;
				}
				await Task.Delay(delay, ct);
			}
			var anon = new
			{
				Model = (lastRun?.Model ?? "gpt-4o"),
				PromptTokens = promptTokens.GetValueOrDefault(),
				CompletionTokens = completionTokens.GetValueOrDefault(),
				TotalTokens = promptTokens.GetValueOrDefault() + completionTokens.GetValueOrDefault()
			};
			if (string.IsNullOrWhiteSpace(text))
			{
				r.isError = true;
				r.data1 = "Assistant run completed pero no se pudo leer el texto final.";
				r.data2 = 200;
				return r;
			}
			r.isError = false;
			r.data = text;
			r.data1 = anon;
			r.data4 = _pricing.CalculateCost(anon.Model, anon.PromptTokens, anon.CompletionTokens);
			return r;
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
			return r;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
			return r;
		}
	}

	public async Task<ReturnData> Assistants_RunToCompletionAsync(string threadId, string assistantId, string role, string content, ResponseFormat? responseFormat = null, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			RunCompletionResult runCompletionResult = await _assistants.RunToCompletionAsync(threadId, assistantId, role, content, null, null, responseFormat, null, null, null, ct);
			r.isError = false;
			r.data = runCompletionResult;
			r.data1 = "OK";
			r.data4 = _pricing.CalculateCost(runCompletionResult.Model ?? "gpt-4o", runCompletionResult.PromptTokens.GetValueOrDefault(), runCompletionResult.CompletionTokens.GetValueOrDefault());
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> Responses_CreateAsync(ResponseCreateRequest request, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			ResponseCreateResult responseCreateResult = await _responses.CreateAsync(request, ct);
			r.isError = false;
			r.data = responseCreateResult;
			r.data1 = "OK";
			r.data4 = _pricing.CalculateCost(responseCreateResult.Model, responseCreateResult.Usage?.PromptTokens, responseCreateResult.Usage?.CompletionTokens);
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> Responses_CreateWithFileAsync(string filePath, string model, string instructions, string userText, string purpose = "assistants", double? temperature = null, double? topP = null, int? maxOutputTokens = null, string? responseFormatType = null, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			FileUploadResponse upload = await _files.UploadAsync(filePath, purpose, null, 3600, "created_at", ct);
			if (string.IsNullOrWhiteSpace(upload.Id))
			{
				r.isError = true;
				r.data1 = "Falló la subida del archivo a OpenAI.";
				return r;
			}
			ResponseCreateResult responseCreateResult = await _responses.CreateWithFileAsync(model, instructions, userText, upload.Id, null, null, maxOutputTokens, responseFormatType, ct);
			r.isError = false;
			r.data = responseCreateResult;
			r.data1 = "OK";
			r.data2 = upload.Id;
			r.data4 = _pricing.CalculateCost(responseCreateResult.Model, responseCreateResult.Usage?.PromptTokens, responseCreateResult.Usage?.CompletionTokens);
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> Assistants_CreateThreadAsync(CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			ThreadCreateResponse data = await _assistants.CreateThreadAsync(ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> Assistants_AddMessageAsync(string threadId, string role, string content, IEnumerable<MessageAttachment>? attachments = null, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			MessageResponse data = await _assistants.AddMessageAsync(threadId, role, content, attachments, ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> Assistants_CreateRunAsync(string threadId, string assistantId, string? additionalInstructions = null, ResponseFormat? responseFormat = null, ToolResources? toolResources = null, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			RunResponse data = await _assistants.CreateRunAsync(threadId, assistantId, additionalInstructions, responseFormat, toolResources, null, null, null, ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> Assistants_GetRunAsync(string threadId, string runId, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			RunResponse data = await _assistants.GetRunAsync(threadId, runId, ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> Assistants_GetRunStepsAsync(string threadId, string runId, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			RunStepsResponse data = await _assistants.GetRunStepsAsync(threadId, runId, ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> Assistants_SubmitToolOutputsAsync(string threadId, string runId, IEnumerable<ToolOutput> outputs, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			RunResponse data = await _assistants.SubmitToolOutputsAsync(threadId, runId, outputs, ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> Files_UploadAsync(string filePath, string purpose, Dictionary<string, string>? metadata = null, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			FileUploadResponse data = await _files.UploadAsync(filePath, purpose, metadata, null, "created_at", ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> Files_GetAsync(string fileId, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			FileData data = await _files.GetAsync(fileId, ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> Files_ListAsync(CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			FilesList data = await _files.ListAsync(ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> Files_DeleteAsync(string fileId, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			bool flag = await _files.DeleteAsync(fileId, ct);
			r.isError = !flag;
			r.data = flag;
			r.data1 = (flag ? "OK" : "No se pudo eliminar el archivo");
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> Files_DownloadContentAsync(string fileId, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			byte[] inArray = await _files.DownloadContentAsync(fileId, ct);
			r.isError = false;
			r.data = Convert.ToBase64String(inArray);
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> VectorStores_CreateAsync(string name, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			VectorStore data = await _vectors.CreateAsync(name, ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> VectorStores_ListAsync(CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			VectorStoresList data = await _vectors.ListAsync(ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> VectorStores_RetrieveAsync(string id, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			VectorStore data = await _vectors.RetrieveAsync(id, ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> VectorStores_AddFileAsync(string vectorStoreId, string fileId, object? attributes = null, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			VectorStoreFile data = await _vectors.AddFileAsync(vectorStoreId, fileId, attributes, ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> VectorStores_RetrieveFileAsync(string vectorStoreId, string fileId, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			VectorStoreFile data = await _vectors.RetrieveFileAsync(vectorStoreId, fileId, ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> VectorStores_UpdateFileAttributesAsync(string vectorStoreId, string fileId, Dictionary<string, object> attributes, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			bool flag = await _vectors.UpdateFileAttributesAsync(vectorStoreId, fileId, attributes, ct);
			r.isError = !flag;
			r.data = flag;
			r.data1 = (flag ? "OK" : "No se pudieron actualizar los atributos");
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> VectorStores_ListFilesAsync(string vectorStoreId, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			VectorStoreFilesList data = await _vectors.ListFilesAsync(vectorStoreId, ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> VectorStores_DeleteAsync(string vectorStoreId, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			bool flag = await _vectors.DeleteAsync(vectorStoreId, ct);
			r.isError = !flag;
			r.data = flag;
			r.data1 = (flag ? "OK" : "No se pudo eliminar el Vector Store");
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> VectorStores_UploadAndAttachAsync(string filePath, string vectorStoreId, object? attributes = null, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			FileUploadResponse uploadRes = await _files.UploadAsync(filePath, "assistants", null, null, "created_at", ct);
			if (string.IsNullOrEmpty(uploadRes.Id))
			{
				r.isError = true;
				r.data1 = "Falló la subida del archivo a OpenAI.";
				return r;
			}
			VectorStoreFile data = await _vectors.AddFileAsync(vectorStoreId, uploadRes.Id, attributes, ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
			r.data2 = uploadRes.Id;
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> Audio_TranscribeAsync(string filePath, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			string data = await _audio.TranscribeAsync(filePath, ct);
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
			r.data4 = _pricing.CalculateAudioCost(0.0);
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public ReturnData Utils_CountTokensAsync(string text)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		ReturnData val = new ReturnData();
		try
		{
			int num = _tokenCounter.CountTokens(text);
			val.isError = false;
			val.data = num;
			val.data1 = "OK";
		}
		catch (Exception ex)
		{
			val.isError = true;
			val.data1 = ex.Message;
		}
		return val;
	}

	public async Task<ReturnData> Embeddings_CreateAsync(string input, string? model = null, int? dimensions = null, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			EmbeddingResponse embeddingResponse = await _embeddings.CreateAsync(input, model, dimensions, ct);
			r.isError = false;
			r.data = embeddingResponse;
			r.data1 = "OK";
			r.data4 = _pricing.CalculateCost(embeddingResponse.Model, embeddingResponse.Usage?.PromptTokens, 0);
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}

	public async Task<ReturnData> Embeddings_CreateBatchAsync(IEnumerable<string> inputs, string? model = null, int? dimensions = null, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			EmbeddingResponse embeddingResponse = await _embeddings.CreateBatchAsync(inputs, model, dimensions, ct);
			r.isError = false;
			r.data = embeddingResponse;
			r.data1 = "OK";
			r.data4 = _pricing.CalculateCost(embeddingResponse.Model, embeddingResponse.Usage?.PromptTokens, 0);
		}
		catch (OpenAIClientException ex)
		{
			r.isError = true;
			r.data1 = ex.Message;
			r.data2 = (int)ex.StatusCode;
			r.data3 = ex.OpenAIErrorCode;
			r.data4 = ex.RawBody;
		}
		catch (Exception ex2)
		{
			r.isError = true;
			r.data1 = ex2.Message;
		}
		return r;
	}
}
