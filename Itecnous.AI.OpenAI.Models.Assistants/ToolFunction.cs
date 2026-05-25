using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Assistants;

public class ToolFunction
{
	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;


	[JsonProperty("arguments")]
	public string Arguments { get; set; } = "{}";

}
