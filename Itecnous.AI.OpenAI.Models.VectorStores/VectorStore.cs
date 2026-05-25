using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.VectorStores;

public class VectorStore
{
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;


	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;


	[JsonProperty("status")]
	public string Status { get; set; } = string.Empty;

}
