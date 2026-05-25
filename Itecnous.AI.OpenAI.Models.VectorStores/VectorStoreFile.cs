using System.Collections.Generic;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.VectorStores;

public class VectorStoreFile
{
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;


	[JsonProperty("vector_store_id")]
	public string VectorStoreId { get; set; } = string.Empty;


	[JsonProperty("status")]
	public string Status { get; set; } = string.Empty;


	[JsonProperty("attributes")]
	public Dictionary<string, object>? Attributes { get; set; }

	[JsonProperty("usage_bytes")]
	public long? UsageBytes { get; set; }

	[JsonProperty("created_at")]
	public long? CreatedAt { get; set; }

	[JsonProperty("last_error")]
	public object? LastError { get; set; }

	[JsonProperty("chunking_strategy")]
	public object? ChunkingStrategy { get; set; }
}
