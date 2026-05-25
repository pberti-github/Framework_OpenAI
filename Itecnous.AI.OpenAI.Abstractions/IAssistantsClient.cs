using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Itecnous.AI.OpenAI.Models.Assistants;

namespace Itecnous.AI.OpenAI.Abstractions;

/// <summary>
/// Contrato de bajo nivel para la API de Assistants.
/// </summary>
public interface IAssistantsClient
{
	/// <summary>
	/// Crea un thread nuevo en Assistants.
	/// </summary>
	Task<ThreadCreateResponse> CreateThreadAsync(CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Agrega un mensaje a un thread existente.
	/// </summary>
	Task<MessageResponse> AddMessageAsync(string threadId, string role, string content, IEnumerable<MessageAttachment>? attachments = null, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Crea un run para un assistant concreto.
	/// </summary>
	Task<RunResponse> CreateRunAsync(string threadId, string assistantId, string? additionalInstructions = null, ResponseFormat? responseFormat = null, ToolResources? toolResources = null, double? temperature = null, int? maxOutputTokens = null, double? topP = null, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Obtiene el texto del ultimo mensaje de un thread.
	/// </summary>
	Task<string?> GetLastMessageTextAsync(string threadId, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Consulta el estado de un run.
	/// </summary>
	Task<RunResponse> GetRunAsync(string threadId, string runId, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Obtiene los pasos ejecutados por un run.
	/// </summary>
	Task<RunStepsResponse> GetRunStepsAsync(string threadId, string runId, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Envía los tool outputs pendientes al run.
	/// </summary>
	Task<RunResponse> SubmitToolOutputsAsync(string threadId, string runId, IEnumerable<ToolOutput> outputs, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Ejecuta un flujo completo de Assistants hasta completarlo o agotar el timeout.
	/// </summary>
	Task<RunCompletionResult> RunToCompletionAsync(string threadId, string assistantId, string role, string content, IEnumerable<MessageAttachment>? attachments = null, string? additionalInstructions = null, ResponseFormat? responseFormat = null, ToolResources? toolResources = null, TimeSpan? pollingDelay = null, TimeSpan? overallTimeout = null, CancellationToken ct = default(CancellationToken));
}
