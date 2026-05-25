using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Responses;

public class ResponsesUsage
{
	[JsonProperty("total_tokens")]
	public int? TotalTokens { get; set; }

	[JsonProperty("prompt_tokens")]
	public int? PromptTokens { get; set; }

	[JsonProperty("completion_tokens")]
	public int? CompletionTokens { get; set; }
}
