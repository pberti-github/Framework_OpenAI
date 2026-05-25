using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Assistants;

public class ResponseFormat
{
	[JsonProperty("type")]
	public string Type { get; set; } = "text";

}
