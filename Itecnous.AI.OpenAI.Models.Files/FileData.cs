using System.Collections.Generic;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Files;

public class FileData
{
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;


	[JsonProperty("filename")]
	public string FileName { get; set; } = string.Empty;


	[JsonProperty("status")]
	public string Status { get; set; } = string.Empty;


	[JsonProperty("purpose")]
	public string Purpose { get; set; } = string.Empty;


	[JsonProperty("bytes")]
	public long? Bytes { get; set; }

	[JsonProperty("created_at")]
	public long? CreatedAt { get; set; }

	[JsonProperty("metadata")]
	public Dictionary<string, string>? Metadata { get; set; }
}
