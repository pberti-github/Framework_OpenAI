using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Responses;

public class TextFormatOptions
{
	[JsonProperty("type")]
	public string Type { get; set; } = "text";

}
