using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Errors;

internal class OpenAIError
{
	[JsonProperty("message")]
	public string? Message { get; set; }

	[JsonProperty("type")]
	public string? Type { get; set; }

	[JsonProperty("code")]
	public string? Code { get; set; }
}
