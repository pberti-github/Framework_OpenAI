namespace Itecnous.AI.OpenAI.Models.Assistants;

public class RunCompletionResult
{
	public string RunId { get; set; } = string.Empty;


	public string? MessageId { get; set; }

	public string? Model { get; set; }

	public int? PromptTokens { get; set; }

	public int? CompletionTokens { get; set; }

	public int? TotalTokens { get; set; }

	public string Status { get; set; } = string.Empty;

}
