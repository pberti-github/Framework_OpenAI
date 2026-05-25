using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Embeddings;

public class EmbeddingRequest
{
	[JsonProperty("input")]
	public object Input { get; set; } = string.Empty;


	[JsonProperty("model")]
	public string Model { get; set; } = string.Empty;


	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public int? Dimensions { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public string? User { get; set; }
}
