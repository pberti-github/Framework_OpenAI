using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Responses;

public class ResponsesToolResources
{
	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public ResponsesFileSearchResources? FileSearch { get; set; }
}
