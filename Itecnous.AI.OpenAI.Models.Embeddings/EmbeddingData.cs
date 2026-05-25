using System;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Embeddings;

public class EmbeddingData
{
	[JsonProperty("object")]
	public string Object { get; set; } = "embedding";


	[JsonProperty("index")]
	public int Index { get; set; }

	[JsonProperty("embedding")]
	public float[] Embedding { get; set; } = Array.Empty<float>();

}
