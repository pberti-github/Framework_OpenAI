using System.Collections.Generic;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Assistants;

public class SubmitToolOutputs
{
	[JsonProperty("tool_calls")]
	public List<ToolCall> ToolCalls { get; set; } = new List<ToolCall>();

}
