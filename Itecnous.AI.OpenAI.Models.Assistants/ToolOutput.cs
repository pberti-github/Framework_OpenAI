using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Assistants;

public class ToolOutput
{
	[JsonProperty("tool_call_id")]
	public string ToolCallId { get; set; } = string.Empty;


	[JsonProperty("output")]
	public string Output { get; set; } = string.Empty;

}
