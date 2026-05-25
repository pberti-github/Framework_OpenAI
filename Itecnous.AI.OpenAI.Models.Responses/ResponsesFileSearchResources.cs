using System.Collections.Generic;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Responses;

public class ResponsesFileSearchResources
{
	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public IList<string>? VectorStoreIds { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public FileSearchFilters? Filters { get; set; }
}
