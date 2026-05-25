using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Itecnous.AI.OpenAI.Models.Assistants;

namespace Itecnous.AI.OpenAI.Abstractions;

public interface IAssistantsClient
{
	Task<ThreadCreateResponse> CreateThreadAsync(CancellationToken ct = default(CancellationToken));

	Task<MessageResponse> AddMessageAsync(string threadId, string role, string content, IEnumerable<MessageAttachment>? attachments = null, CancellationToken ct = default(CancellationToken));

	Task<RunResponse> CreateRunAsync(string threadId, string assistantId, string? additionalInstructions = null, ResponseFormat? responseFormat = null, ToolResources? toolResources = null, double? temperature = null, int? maxOutputTokens = null, double? topP = null, CancellationToken ct = default(CancellationToken));

	Task<string?> GetLastMessageTextAsync(string threadId, CancellationToken ct = default(CancellationToken));

	Task<RunResponse> GetRunAsync(string threadId, string runId, CancellationToken ct = default(CancellationToken));

	Task<RunStepsResponse> GetRunStepsAsync(string threadId, string runId, CancellationToken ct = default(CancellationToken));

	Task<RunResponse> SubmitToolOutputsAsync(string threadId, string runId, IEnumerable<ToolOutput> outputs, CancellationToken ct = default(CancellationToken));

	Task<RunCompletionResult> RunToCompletionAsync(string threadId, string assistantId, string role, string content, IEnumerable<MessageAttachment>? attachments = null, string? additionalInstructions = null, ResponseFormat? responseFormat = null, ToolResources? toolResources = null, TimeSpan? pollingDelay = null, TimeSpan? overallTimeout = null, CancellationToken ct = default(CancellationToken));
}
