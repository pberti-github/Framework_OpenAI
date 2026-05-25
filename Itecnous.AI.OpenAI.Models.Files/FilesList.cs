using System;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Files;

public class FilesList
{
	[JsonProperty("data")]
	public FileData[] Data { get; set; } = Array.Empty<FileData>();

}
