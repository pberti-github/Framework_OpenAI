using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

/// <summary>
/// Fachada de alto nivel para consumir OpenAI y Azure OpenAI devolviendo <see cref="ReturnData"/>.
/// </summary>
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

	/// <summary>
	/// Crea una nueva instancia de la fachada de resultados de OpenAI.
	/// </summary>
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

	private static ReturnData CrearErrorDeFase(string codigo, string mensaje, ReturnData? origen = null)
	{
		ReturnData r = new ReturnData();
		r.isError = true;
		r.data = codigo + " - " + mensaje;
		if (origen != null)
		{
			if (origen.data != null)
			{
				r.data1 = origen.data;
			}
			if (origen.data1 != null)
			{
				r.data2 = origen.data1;
			}
			if (origen.data2 != null)
			{
				r.data3 = origen.data2;
			}
			if (origen.data3 != null)
			{
				r.data4 = origen.data3;
			}
			if (origen.data4 != null)
			{
				r.data5 = origen.data4;
			}
			if (origen.data5 != null)
			{
				r.data6 = origen.data5;
			}
		}
		return r;
	}

	private static ReturnData CrearErrorTecnico(string mensaje, Exception ex, OpenAIClientException? errorCliente = null, ReturnData? origen = null, [CallerMemberName] string? miembro = null)
	{
		ReturnData r = new ReturnData();
		r.isError = true;
		r.data = ObtenerCodigoErrorTecnico(miembro) + " - " + mensaje;
		r.data1 = ex.Message;
		if (errorCliente != null)
		{
			r.data2 = (int)errorCliente.StatusCode;
			r.data3 = errorCliente.OpenAIErrorCode;
			r.data4 = errorCliente.RawBody;
		}
		if (origen != null)
		{
			if (origen.data != null)
			{
				r.data5 = origen.data;
			}
			if (origen.data1 != null)
			{
				r.data6 = origen.data1;
			}
		}
		return r;
	}

	private static string ObtenerCodigoErrorTecnico(string? miembro)
	{
		if (string.IsNullOrWhiteSpace(miembro))
		{
			return "OPENAI_ERR099";
		}
		if (miembro.StartsWith("Assistants_", StringComparison.OrdinalIgnoreCase))
		{
			return "ASA_ERR099";
		}
		if (miembro.StartsWith("Responses_", StringComparison.OrdinalIgnoreCase))
		{
			return "RSP_ERR099";
		}
		if (miembro.StartsWith("Files_", StringComparison.OrdinalIgnoreCase))
		{
			return "FIL_ERR099";
		}
		if (miembro.StartsWith("VectorStores_", StringComparison.OrdinalIgnoreCase))
		{
			return "VST_ERR099";
		}
		if (miembro.StartsWith("Embeddings_", StringComparison.OrdinalIgnoreCase))
		{
			return "EMB_ERR099";
		}
		if (miembro.StartsWith("Audio_", StringComparison.OrdinalIgnoreCase))
		{
			return "AUD_ERR099";
		}
		if (miembro.StartsWith("Utils_", StringComparison.OrdinalIgnoreCase))
		{
			return "UTL_ERR099";
		}
		return "OPENAI_ERR099";
	}

	private static bool TryObtener<T>(object? value, out T? typed) where T : class
	{
		if (value is T result)
		{
			typed = result;
			return true;
		}
		typed = default;
		return false;
	}

	#region Assistants

	/// <summary>
	/// Ejecuta un flujo completo de Assistants creando thread, mensaje y run hasta obtener el ultimo texto.
	/// </summary>
	public async Task<ReturnData> Assistants_AskAgentAsync(string assistantId, string userContent, string? systemInstructions = null, ResponseFormat? responseFormat = null, double? temperature = null, int? maxOutputTokens = null, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			#region 1 - Crear thread
			ReturnData threadResult = await Assistants_CreateThreadAsync(ct);
			if (threadResult.isError)
			{
				return CrearErrorDeFase("ASA_ERR010", "No se pudo crear el thread.", threadResult);
			}
			if (!TryObtener<ThreadCreateResponse>(threadResult.data, out ThreadCreateResponse? thread) || thread == null || string.IsNullOrWhiteSpace(thread.Id))
			{
				return CrearErrorDeFase("ASA_ERR011", "La respuesta del thread es invalida.");
			}
			#endregion

			#region 2 - Agregar mensaje inicial
			ReturnData messageResult = await Assistants_AddMessageAsync(thread.Id, "user", userContent, null, ct);
			if (messageResult.isError)
			{
				return CrearErrorDeFase("ASA_ERR020", "No se pudo agregar el mensaje inicial.", messageResult);
			}
			#endregion

			#region 3 - Crear run
			ReturnData runResult = await Assistants_CreateRunAsync(thread.Id, assistantId, systemInstructions, responseFormat, null, ct);
			if (runResult.isError)
			{
				return CrearErrorDeFase("ASA_ERR030", "No se pudo crear el run.", runResult);
			}
			if (!TryObtener<RunResponse>(runResult.data, out RunResponse? run) || run == null || string.IsNullOrWhiteSpace(run.Id))
			{
				return CrearErrorDeFase("ASA_ERR031", "La respuesta del run es invalida.");
			}
			#endregion

			DateTime start = DateTime.UtcNow;
			TimeSpan timeout = TimeSpan.FromMinutes(2.0);
			TimeSpan delay = TimeSpan.FromMilliseconds(800.0);
			string? text = null;
			int? promptTokens = 0;
			int? completionTokens = 0;
			string? model = run.Model;
			if (string.IsNullOrWhiteSpace(model))
			{
				model = "gpt-4o";
			}

			#region 4 - Esperar cierre del run
			while (DateTime.UtcNow - start < timeout)
			{
				ReturnData runPollResult = await Assistants_GetRunAsync(thread.Id, run.Id, ct);
				if (runPollResult.isError)
				{
					return CrearErrorDeFase("ASA_ERR040", "No se pudo consultar el estado del run.", runPollResult);
				}
				if (!TryObtener<RunResponse>(runPollResult.data, out RunResponse? runResponse) || runResponse == null)
				{
					return CrearErrorDeFase("ASA_ERR041", "La respuesta del estado del run es invalida.");
				}
				if (!string.IsNullOrWhiteSpace(runResponse.Model))
				{
					model = runResponse.Model;
				}
				if (runResponse.Status == "completed")
				{
					ReturnData stepsResult = await Assistants_GetRunStepsAsync(thread.Id, run.Id, ct);
					if (stepsResult.isError)
					{
						return CrearErrorDeFase("ASA_ERR050", "No se pudieron obtener los pasos del run.", stepsResult);
					}
					if (TryObtener<RunStepsResponse>(stepsResult.data, out RunStepsResponse? steps) && steps != null && steps.Data != null)
					{
						RunStep? runStep = steps.Data.FirstOrDefault();
						promptTokens = runStep?.Usage?.PromptTokens;
						completionTokens = runStep?.Usage?.CompletionTokens;
					}
					text = await _assistants.GetLastMessageTextAsync(thread.Id, ct);
					break;
				}
				if (runResponse.Status == "failed" || runResponse.Status == "cancelled" || runResponse.Status == "expired")
				{
					return CrearErrorDeFase("ASA_ERR060", "El run termino con un estado no valido: " + runResponse.Status + ".");
				}
				await Task.Delay(delay, ct);
			}
			#endregion

			if (string.IsNullOrWhiteSpace(text))
			{
				return CrearErrorDeFase("ASA_ERR070", "El run finalizo pero no se pudo leer el texto final.");
			}

			var anon = new
			{
				Model = model,
				PromptTokens = promptTokens.GetValueOrDefault(),
				CompletionTokens = completionTokens.GetValueOrDefault(),
				TotalTokens = promptTokens.GetValueOrDefault() + completionTokens.GetValueOrDefault()
			};
			r.isError = false;
			r.data = text;
			r.data1 = anon;
			r.data4 = _pricing.CalculateCost(model, anon.PromptTokens, anon.CompletionTokens);
			return r;
		}
		catch (OpenAIClientException ex)
		{
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
	}

	/// <summary>
	/// Ejecuta un flujo completo de Assistant hasta su finalizacion o timeout.
	/// </summary>
	public async Task<ReturnData> Assistants_RunToCompletionAsync(string threadId, string assistantId, string role, string content, ResponseFormat? responseFormat = null, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			RunCompletionResult runCompletionResult = await _assistants.RunToCompletionAsync(threadId, assistantId, role, content, null, null, responseFormat, null, null, null, ct);
			r.isError = false;
			r.data = runCompletionResult;
			r.data1 = "OK";
			string? model = runCompletionResult.Model;
			if (string.IsNullOrWhiteSpace(model))
			{
				model = "gpt-4o";
			}
			r.data4 = _pricing.CalculateCost(model, runCompletionResult.PromptTokens.GetValueOrDefault(), runCompletionResult.CompletionTokens.GetValueOrDefault());
		}
		catch (OpenAIClientException ex)
		{
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Crea un thread en Assistants.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Agrega un mensaje a un thread de Assistants.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Crea un run en Assistants.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Obtiene el estado actual de un run.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Obtiene los pasos ejecutados por un run.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Envía tool outputs a un run en curso.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	#endregion

	#region Responses

	/// <summary>
	/// Crea una respuesta usando la API Responses.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Sube un archivo y crea una respuesta usando ese archivo como entrada.
	/// </summary>
	public async Task<ReturnData> Responses_CreateWithFileAsync(string filePath, string model, string instructions, string userText, string purpose = "assistants", double? temperature = null, double? topP = null, int? maxOutputTokens = null, string? responseFormatType = null, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			ReturnData uploadResult = await Files_UploadAsync(filePath, purpose, null, ct);
			if (uploadResult.isError)
			{
				return CrearErrorDeFase("RCF_ERR010", "No se pudo subir el archivo a OpenAI.", uploadResult);
			}
			if (!TryObtener<FileUploadResponse>(uploadResult.data, out FileUploadResponse? upload) || upload == null || string.IsNullOrWhiteSpace(upload.Id))
			{
				return CrearErrorDeFase("RCF_ERR011", "La respuesta de subida del archivo es invalida.");
			}
			ResponseCreateResult responseCreateResult = await _responses.CreateWithFileAsync(model, instructions, userText, upload.Id, temperature, topP, maxOutputTokens, responseFormatType, ct);
			r.isError = false;
			r.data = responseCreateResult;
			r.data1 = "OK";
			r.data2 = upload.Id;
			r.data4 = _pricing.CalculateCost(responseCreateResult.Model, responseCreateResult.Usage?.PromptTokens, responseCreateResult.Usage?.CompletionTokens);
		}
		catch (OpenAIClientException ex)
		{
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	#endregion

	#region Files

	/// <summary>
	/// Sube un archivo a OpenAI.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Obtiene la metadata de un archivo.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Lista los archivos disponibles en OpenAI.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Elimina un archivo de OpenAI.
	/// </summary>
	public async Task<ReturnData> Files_DeleteAsync(string fileId, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			bool flag = await _files.DeleteAsync(fileId, ct);
			r.isError = !flag;
			if (flag)
			{
				r.data = flag;
				r.data1 = "OK";
			}
			else
			{
				r.data = "FIL_ERR020 - No se pudo eliminar el archivo.";
				r.data1 = flag;
			}
		}
		catch (OpenAIClientException ex)
		{
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Descarga el contenido de un archivo y lo devuelve en Base64.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	#endregion

	#region Vector Stores

	/// <summary>
	/// Crea un vector store.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Lista los vector stores disponibles.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Recupera un vector store por su identificador.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Agrega un archivo a un vector store.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Recupera un archivo de un vector store.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Actualiza atributos de un archivo asociado a un vector store.
	/// </summary>
	public async Task<ReturnData> VectorStores_UpdateFileAttributesAsync(string vectorStoreId, string fileId, Dictionary<string, object> attributes, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			bool flag = await _vectors.UpdateFileAttributesAsync(vectorStoreId, fileId, attributes, ct);
			r.isError = !flag;
			if (flag)
			{
				r.data = flag;
				r.data1 = "OK";
			}
			else
			{
				r.data = "VST_ERR020 - No se pudieron actualizar los atributos.";
				r.data1 = flag;
			}
		}
		catch (OpenAIClientException ex)
		{
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Lista los archivos de un vector store.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Elimina un vector store.
	/// </summary>
	public async Task<ReturnData> VectorStores_DeleteAsync(string vectorStoreId, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			bool flag = await _vectors.DeleteAsync(vectorStoreId, ct);
			r.isError = !flag;
			if (flag)
			{
				r.data = flag;
				r.data1 = "OK";
			}
			else
			{
				r.data = "VST_ERR020 - No se pudo eliminar el Vector Store.";
				r.data1 = flag;
			}
		}
		catch (OpenAIClientException ex)
		{
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Sube un archivo y lo adjunta a un vector store validando cada paso.
	/// </summary>
	public async Task<ReturnData> VectorStores_UploadAndAttachAsync(string filePath, string vectorStoreId, object? attributes = null, CancellationToken ct = default(CancellationToken))
	{
		ReturnData r = new ReturnData();
		try
		{
			ReturnData uploadResult = await Files_UploadAsync(filePath, "assistants", null, ct);
			if (uploadResult.isError)
			{
				return CrearErrorDeFase("VUA_ERR010", "No se pudo subir el archivo para adjuntarlo al vector store.", uploadResult);
			}
			if (!TryObtener<FileUploadResponse>(uploadResult.data, out FileUploadResponse? uploadRes) || uploadRes == null || string.IsNullOrWhiteSpace(uploadRes.Id))
			{
				return CrearErrorDeFase("VUA_ERR011", "La respuesta de subida del archivo es invalida.");
			}
			ReturnData attachResult = await VectorStores_AddFileAsync(vectorStoreId, uploadRes.Id, attributes, ct);
			if (attachResult.isError)
			{
				return CrearErrorDeFase("VUA_ERR020", "No se pudo adjuntar el archivo al vector store.", attachResult);
			}
			if (!TryObtener<VectorStoreFile>(attachResult.data, out VectorStoreFile? data) || data == null)
			{
				return CrearErrorDeFase("VUA_ERR021", "La respuesta de adjuntar el archivo es invalida.");
			}
			r.isError = false;
			r.data = data;
			r.data1 = "OK";
			r.data2 = uploadRes.Id;
		}
		catch (OpenAIClientException ex)
		{
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	#endregion

	#region Audio y utilidades

	/// <summary>
	/// Transcribe un archivo de audio usando Whisper.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Cuenta tokens para un texto de entrada.
	/// </summary>
	public ReturnData Utils_CountTokensAsync(string text)
	{
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
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex);
		}
		return val;
	}

	/// <summary>
	/// Genera embeddings para un texto.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	/// <summary>
	/// Genera embeddings para una coleccion de textos.
	/// </summary>
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
			return CrearErrorTecnico("Excepcion al ejecutar la operacion.", ex, ex);
		}
		catch (Exception ex2)
		{
			return CrearErrorTecnico("Excepcion no controlada al ejecutar la operacion.", ex2);
		}
		return r;
	}

	#endregion
}
