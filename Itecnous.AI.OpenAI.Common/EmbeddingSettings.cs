namespace Itecnous.AI.OpenAI.Common;

public class EmbeddingSettings
{
	public string DefaultModel { get; set; } = "text-embedding-3-small";


	public int? DefaultDimensions { get; set; }

	public bool NormalizarVectores { get; set; } = true;

}
