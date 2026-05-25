using System;
using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Assistants;

public class RunStepsResponse
{
	[JsonProperty("data")]
	public RunStep[] Data { get; set; } = Array.Empty<RunStep>();

}
