using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Responses;

public class TextOptions
{
	[JsonProperty("format")]
	public TextFormatOptions? Format { get; set; }

	[JsonProperty("verbosity")]
	public string? Verbosity { get; set; }
}
