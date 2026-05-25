using System.Collections.Generic;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Responses;

public class ResponseCreateResult
{
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;


	[JsonIgnore]
	public string? ResponseId { get; set; }

	[JsonIgnore]
	public string? ConversationId { get; set; }

	[JsonProperty("output_text")]
	public string? OutputText { get; set; }

	[JsonProperty("usage")]
	public ResponsesUsage? Usage { get; set; }

	[JsonProperty("model")]
	public string? Model { get; set; }

	[JsonIgnore]
	public IList<ResponseCitation>? Citations { get; set; }
}
