using Itecnous.AI.OpenAI.Common;

namespace Itecnous.AI.OpenAI.Services;

public class PricingService
{
	private readonly OpenAISettings _settings;

	public PricingService(OpenAISettings settings)
	{
		_settings = settings;
	}

	public decimal CalculateCost(string? model, int? inputTokens, int? outputTokens = 0)
	{
		if (string.IsNullOrEmpty(model))
		{
			return 0m;
		}
		int valueOrDefault = inputTokens.GetValueOrDefault();
		int valueOrDefault2 = outputTokens.GetValueOrDefault();
		if (_settings.Pricing != null && _settings.Pricing.TryGetValue(model, out ModelPricing value))
		{
			return Calculate(valueOrDefault, valueOrDefault2, value.InputCostPer1M, value.OutputCostPer1M);
		}
		string text = model.ToLower();
		if (text.Contains("gpt-5.1-nano") || text.Contains("gpt-5-nano"))
		{
			return Calculate(valueOrDefault, valueOrDefault2, 0.05m, 0.40m);
		}
		string text2 = text;
		if (text2.Contains("gpt-5.1-mini") || text2.Contains("gpt-5-mini"))
		{
			return Calculate(valueOrDefault, valueOrDefault2, 0.25m, 2.00m);
		}
		string text3 = text;
		if (text3 == "gpt-5" || text3.Contains("gpt-5 ") || text3.Contains("gpt-5-"))
		{
			return Calculate(valueOrDefault, valueOrDefault2, 1.25m, 10.00m);
		}
		if (text.Contains("gpt-4.1-nano"))
		{
			return Calculate(valueOrDefault, valueOrDefault2, 0.10m, 0.40m);
		}
		if (text.Contains("gpt-4.1-mini"))
		{
			return Calculate(valueOrDefault, valueOrDefault2, 0.40m, 1.60m);
		}
		if (text.Contains("gpt-4.1"))
		{
			return Calculate(valueOrDefault, valueOrDefault2, 2.00m, 8.00m);
		}
		if (text.Contains("gpt-4o-mini"))
		{
			return Calculate(valueOrDefault, valueOrDefault2, 0.15m, 0.60m);
		}
		if (text.Contains("gpt-4o"))
		{
			return Calculate(valueOrDefault, valueOrDefault2, 2.50m, 10.00m);
		}
		if (text.Contains("embedding-3-small"))
		{
			return Calculate(valueOrDefault, 0, 0.02m, 0m);
		}
		if (text.Contains("embedding-3-large"))
		{
			return Calculate(valueOrDefault, 0, 0.13m, 0m);
		}
		if (text.Contains("ada-002"))
		{
			return Calculate(valueOrDefault, 0, 0.10m, 0m);
		}
		return 0m;
	}

	public decimal CalculateAudioCost(double seconds, string model = "whisper-1")
	{
		decimal num = (decimal)(seconds / 60.0);
		if (_settings.Pricing != null && _settings.Pricing.TryGetValue(model, out ModelPricing value) && value.AudioCostPerMinute.HasValue)
		{
			return num * value.AudioCostPerMinute.Value;
		}
		return num * 0.006m;
	}

	private decimal Calculate(int input, int output, decimal input1M, decimal output1M)
	{
		return (decimal)input * input1M / 1000000m + (decimal)output * output1M / 1000000m;
	}
}
