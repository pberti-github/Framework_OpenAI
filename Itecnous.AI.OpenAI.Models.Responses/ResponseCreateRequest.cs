using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Responses;

public class ResponseCreateRequest
{
	[JsonProperty("model")]
	public string Model { get; set; } = string.Empty;


	[JsonProperty("messages")]
	public object Messages { get; set; } = string.Empty;


	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public string? Conversation { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public string? PreviousResponseId { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public object? ResponseFormat { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public double? Temperature { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public double? TopP { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public int? MaxOutputTokens { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public TextOptions? Text { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public object[]? Tools { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public ResponsesToolResources? ToolResources { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public object? ToolChoice { get; set; }
}
