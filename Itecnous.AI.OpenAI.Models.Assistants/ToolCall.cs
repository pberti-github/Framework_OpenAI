using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Assistants;

public class ToolCall
{
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;


	[JsonProperty("type")]
	public string Type { get; set; } = "function";


	[JsonProperty("function")]
	public ToolFunction Function { get; set; } = new ToolFunction();

}
