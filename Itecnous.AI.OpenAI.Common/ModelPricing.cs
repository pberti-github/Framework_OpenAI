namespace Itecnous.AI.OpenAI.Common;

public class ModelPricing
{
	public decimal InputCostPer1M { get; set; }

	public decimal OutputCostPer1M { get; set; }

	public decimal? AudioCostPerMinute { get; set; }
}
