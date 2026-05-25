using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Assistants;

public class RunResponse
{
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;


	[JsonProperty("status")]
	public string Status { get; set; } = string.Empty;


	[JsonProperty("model")]
	public string? Model { get; set; }

	[JsonProperty("thread_id")]
	public string ThreadId { get; set; } = string.Empty;


	[JsonProperty("required_action")]
	public RequiredAction? RequiredAction { get; set; }

	[JsonProperty("usage")]
	public Usage? Usage { get; set; }
}
