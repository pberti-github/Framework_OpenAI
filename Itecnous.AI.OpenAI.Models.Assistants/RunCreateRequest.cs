using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Assistants;

public class RunCreateRequest
{
	[JsonProperty("assistant_id")]
	public string AssistantId { get; set; } = string.Empty;


	[JsonProperty("additional_instructions")]
	public string? AdditionalInstructions { get; set; }

	[JsonProperty("response_format")]
	public ResponseFormat? ResponseFormat { get; set; }

	[JsonProperty("tool_resources")]
	public ToolResources? ToolResources { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public double? Temperature { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public double? TopP { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public int? MaxOutputTokens { get; set; }
}
