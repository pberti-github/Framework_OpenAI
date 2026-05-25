using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Errors;

internal class OpenAIErrorEnvelope
{
	[JsonProperty("error")]
	public OpenAIError? Error { get; set; }
}
