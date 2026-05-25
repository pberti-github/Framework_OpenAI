using System.Collections.Generic;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Responses;

public class FileSearchFilters
{
	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public IDictionary<string, object>? Attributes { get; set; }
}
