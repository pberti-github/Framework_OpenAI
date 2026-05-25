using System.Collections.Generic;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Assistants;

public class MessageAddRequest
{
	[JsonProperty("role")]
	public string Role { get; set; } = "user";


	[JsonProperty("content")]
	public string Content { get; set; } = string.Empty;


	[JsonProperty("attachments")]
	public IEnumerable<MessageAttachment>? Attachments { get; set; }
}
