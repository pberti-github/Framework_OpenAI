using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Assistants;

public class ToolResources
{
	[JsonProperty("file_search")]
	public object? FileSearch { get; set; }
}
