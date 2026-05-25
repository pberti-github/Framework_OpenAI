using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Assistants;

public class RequiredAction
{
	[JsonProperty("type")]
	public string Type { get; set; } = string.Empty;


	[JsonProperty("submit_tool_outputs")]
	public SubmitToolOutputs? SubmitToolOutputs { get; set; }
}
