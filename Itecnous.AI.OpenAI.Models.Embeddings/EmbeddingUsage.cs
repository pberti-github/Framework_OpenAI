using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Embeddings;

public class EmbeddingUsage
{
	[JsonProperty("prompt_tokens")]
	public int PromptTokens { get; set; }

	[JsonProperty("total_tokens")]
	public int TotalTokens { get; set; }
}
