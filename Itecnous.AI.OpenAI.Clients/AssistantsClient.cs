using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Itecnous.AI.OpenAI.Abstractions;
using Itecnous.AI.OpenAI.Common;
using Itecnous.AI.OpenAI.Errors;
using Itecnous.AI.OpenAI.Models.Assistants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Itecnous.AI.OpenAI.Clients;

/// <summary>
/// Cliente de bajo nivel para la API de Assistants.
/// </summary>
public class AssistantsClient : IAssistantsClient
{
	private readonly OpenAISettings _settings;

	private static readonly string[] _betaHeaders = new string[1] { "assistants=v2" };

	public AssistantsClient(OpenAISettings settings)
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
	/// Crea un thread nuevo en Assistants.
	/// </summary>
	public async Task<ThreadCreateResponse> CreateThreadAsync(CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			HttpResponseMessage resp = await http.PostAsync("threads", new StringContent("{}", Encoding.UTF8, "application/json"), ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(ObtenerMensajeError(text2, "Error al crear thread."), resp.StatusCode, openAIErrorCode, text);
			}
			return DeserializarRequerido<ThreadCreateResponse>(text, resp.StatusCode, "Respuesta invalida.", text);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al crear thread.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	/// <summary>
	/// Agrega un mensaje a un thread existente.
	/// </summary>
	public async Task<MessageResponse> AddMessageAsync(string threadId, string role, string content, IEnumerable<MessageAttachment>? attachments = null, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			MessageAddRequest value = new MessageAddRequest
			{
				Role = role,
				Content = content,
				Attachments = attachments
			};
			HttpResponseMessage resp = await http.PostAsJsonAsync("threads/" + threadId + "/messages", value, ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(ObtenerMensajeError(text2, "Error al agregar mensaje."), resp.StatusCode, openAIErrorCode, text);
			}
			return DeserializarRequerido<MessageResponse>(text, resp.StatusCode, "Respuesta invalida.", text);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al agregar mensaje.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	/// <summary>
	/// Obtiene el texto del ultimo mensaje de un thread.
	/// </summary>
	public async Task<string?> GetLastMessageTextAsync(string threadId, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			HttpResponseMessage resp = await http.GetAsync("threads/" + threadId + "/messages?limit=1&order=desc", ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(ObtenerMensajeError(text2, "Error al listar mensajes."), resp.StatusCode, openAIErrorCode, text);
			}
			JToken? data = JObject.Parse(text)["data"];
			JArray? messages = data as JArray;
			if (messages != null)
			{
				foreach (JObject item in messages.OfType<JObject>())
				{
					if (string.Equals(item.Value<string>("type"), "text", StringComparison.OrdinalIgnoreCase))
					{
						JToken? textNode = item["text"];
						JObject? textObject = textNode as JObject;
						string? text3 = null;
						if (textObject != null)
						{
							text3 = textObject.Value<string>("value");
						}
						if (!string.IsNullOrEmpty(text3))
						{
							return text3;
						}
					}
				}
			}
			return null;
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al obtener último mensaje.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	/// <summary>
	/// Crea un run para un assistant concreto.
	/// </summary>
	public async Task<RunResponse> CreateRunAsync(string threadId, string assistantId, string? additionalInstructions = null, ResponseFormat? responseFormat = null, ToolResources? toolResources = null, double? temperature = null, int? maxOutputTokens = null, double? topP = null, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			StringContent content = new StringContent(JsonConvert.SerializeObject((object)new RunCreateRequest
			{
				AssistantId = assistantId,
				AdditionalInstructions = additionalInstructions,
				ResponseFormat = responseFormat,
				ToolResources = toolResources,
				Temperature = temperature,
				MaxOutputTokens = maxOutputTokens,
				TopP = topP
			}, new JsonSerializerSettings
			{
				NullValueHandling = (NullValueHandling)1
			}), Encoding.UTF8, "application/json");
			HttpResponseMessage resp = await http.PostAsync("threads/" + threadId + "/runs", content, ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(ObtenerMensajeError(text2, "Error al crear run."), resp.StatusCode, openAIErrorCode, text);
			}
			return DeserializarRequerido<RunResponse>(text, resp.StatusCode, "Respuesta invalida.", text);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al crear run.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	/// <summary>
	/// Consulta el estado de un run.
	/// </summary>
	public async Task<RunResponse> GetRunAsync(string threadId, string runId, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			HttpResponseMessage resp = await http.GetAsync("threads/" + threadId + "/runs/" + runId, ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(ObtenerMensajeError(text2, "Error al obtener run."), resp.StatusCode, openAIErrorCode, text);
			}
			return DeserializarRequerido<RunResponse>(text, resp.StatusCode, "Respuesta invalida.", text);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al obtener run.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	/// <summary>
	/// Obtiene los pasos ejecutados por un run.
	/// </summary>
	public async Task<RunStepsResponse> GetRunStepsAsync(string threadId, string runId, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			HttpResponseMessage resp = await http.GetAsync($"threads/{threadId}/runs/{runId}/steps", ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(ObtenerMensajeError(text2, "Error al obtener steps."), resp.StatusCode, openAIErrorCode, text);
			}
			return DeserializarRequerido<RunStepsResponse>(text, resp.StatusCode, "Respuesta invalida.", text);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al obtener steps.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	/// <summary>
	/// Envía los tool outputs pendientes al run.
	/// </summary>
	public async Task<RunResponse> SubmitToolOutputsAsync(string threadId, string runId, IEnumerable<ToolOutput> outputs, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using HttpClient http = OpenAIHttpClient.Create(_settings, _betaHeaders);
			var value = new
			{
				tool_outputs = outputs
			};
			HttpResponseMessage resp = await http.PostAsJsonAsync($"threads/{threadId}/runs/{runId}/submit_tool_outputs", value, ct);
			string text = await resp.Content.ReadAsStringAsync(ct);
			if (!resp.IsSuccessStatusCode)
			{
				var (text2, openAIErrorCode) = OpenAIErrorParser.TryParse(text);
				throw new OpenAIClientException(ObtenerMensajeError(text2, "Error al enviar tool outputs."), resp.StatusCode, openAIErrorCode, text);
			}
			return DeserializarRequerido<RunResponse>(text, resp.StatusCode, "Respuesta invalida.", text);
		}
		catch (OpenAIClientException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new OpenAIClientException("Excepción al enviar tool outputs.", HttpStatusCode.InternalServerError, null, null, inner);
		}
	}

	/// <summary>
	/// Ejecuta un flujo completo de Assistants hasta completarlo o agotar el timeout.
	/// </summary>
	public async Task<RunCompletionResult> RunToCompletionAsync(string threadId, string assistantId, string role, string content, IEnumerable<MessageAttachment>? attachments = null, string? additionalInstructions = null, ResponseFormat? responseFormat = null, ToolResources? toolResources = null, TimeSpan? pollingDelay = null, TimeSpan? overallTimeout = null, CancellationToken ct = default(CancellationToken))
	{
		pollingDelay.GetValueOrDefault();
		if (!pollingDelay.HasValue)
		{
			TimeSpan value = TimeSpan.FromMilliseconds(800.0);
			pollingDelay = value;
		}
		overallTimeout.GetValueOrDefault();
		if (!overallTimeout.HasValue)
		{
			TimeSpan value = TimeSpan.FromMinutes(2.0);
			overallTimeout = value;
		}
		using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
		cts.CancelAfter(overallTimeout.Value);
		try
		{
			await AddMessageAsync(threadId, role, content, attachments, cts.Token);
			RunResponse run = await CreateRunAsync(threadId, assistantId, additionalInstructions, responseFormat, toolResources, null, null, null, cts.Token);
			while (!cts.IsCancellationRequested)
			{
				RunResponse runResponse = await GetRunAsync(threadId, run.Id, cts.Token);
				if (runResponse.Status == "completed")
				{
					string? model = runResponse.Model;
					if (string.IsNullOrWhiteSpace(model))
					{
						model = run.Model;
					}
					return new RunCompletionResult
					{
						RunId = run.Id,
						Status = runResponse.Status,
						Model = model,
						PromptTokens = runResponse.Usage?.PromptTokens,
						CompletionTokens = runResponse.Usage?.CompletionTokens,
						TotalTokens = runResponse.Usage?.TotalTokens
					};
				}
				if (runResponse.Status == "requires_action")
				{
					return new RunCompletionResult
					{
						RunId = run.Id,
						Status = runResponse.Status
					};
				}
				if (runResponse.Status == "failed" || runResponse.Status == "cancelled" || runResponse.Status == "expired")
				{
					return new RunCompletionResult
					{
						RunId = run.Id,
						Status = runResponse.Status
					};
				}
				await Task.Delay(pollingDelay.Value, cts.Token);
			}
			return new RunCompletionResult
			{
				RunId = run.Id,
				Status = "timeout"
			};
		}
		catch (OperationCanceledException)
		{
			return new RunCompletionResult
			{
				Status = "timeout"
			};
		}
	}
}
