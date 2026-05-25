using System;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.VectorStores;

public class VectorStoreFilesList
{
	[JsonProperty("data")]
	public VectorStoreFile[] Data { get; set; } = Array.Empty<VectorStoreFile>();

}
