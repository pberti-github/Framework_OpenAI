using System.Collections.Generic;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Embeddings;

public class EmbeddingResponse
{
	[JsonProperty("object")]
	public string Object { get; set; } = "list";


	[JsonProperty("data")]
	public List<EmbeddingData> Data { get; set; } = new List<EmbeddingData>();


	[JsonProperty("model")]
	public string Model { get; set; } = string.Empty;


	[JsonProperty("usage")]
	public EmbeddingUsage Usage { get; set; } = new EmbeddingUsage();

}
