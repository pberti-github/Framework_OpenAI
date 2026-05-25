using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Itecnous.AI.OpenAI.Common;

public class OpenAISettings
{
	public OpenAIProvider Provider { get; set; }

	[Required]
	public string ApiKey { get; set; } = string.Empty;


	public string? OrganizationId { get; set; }

	public string? ProjectId { get; set; }

	public string? AzureEndpoint { get; set; }

	public string AzureApiVersion { get; set; } = "2024-02-15-preview";


	public Dictionary<string, string> DeploymentMappings { get; set; } = new Dictionary<string, string>();


	public EmbeddingSettings Embeddings { get; set; } = new EmbeddingSettings();


	public Dictionary<string, ModelPricing> Pricing { get; set; } = new Dictionary<string, ModelPricing>();

}
