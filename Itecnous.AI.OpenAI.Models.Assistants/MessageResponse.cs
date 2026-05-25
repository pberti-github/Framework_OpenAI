using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Assistants;

public class MessageResponse
{
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;


	[JsonProperty("thread_id")]
	public string ThreadId { get; set; } = string.Empty;


	[JsonProperty("role")]
	public string Role { get; set; } = string.Empty;


	[JsonProperty("content")]
	public object? Content { get; set; }
}
