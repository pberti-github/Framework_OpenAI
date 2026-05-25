using System;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.VectorStores;

public class VectorStoresList
{
	[JsonProperty("data")]
	public VectorStore[] Data { get; set; } = Array.Empty<VectorStore>();

}
