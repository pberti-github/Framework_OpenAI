using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Assistants;

public class RunStep
{
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;


	[JsonProperty("status")]
	public string Status { get; set; } = string.Empty;


	[JsonProperty("usage")]
	public Usage? Usage { get; set; }
}
