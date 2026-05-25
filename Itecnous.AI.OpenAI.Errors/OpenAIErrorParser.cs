using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Errors;

internal static class OpenAIErrorParser
{
	public static (string? message, string? code) TryParse(string body)
	{
		try
		{
			OpenAIErrorEnvelope? openAIErrorEnvelope = JsonConvert.DeserializeObject<OpenAIErrorEnvelope>(body);
			if (openAIErrorEnvelope?.Error == null)
			{
				return (message: null, code: null);
			}
			string? item = openAIErrorEnvelope.Error.Code;
			if (string.IsNullOrWhiteSpace(item))
			{
				item = openAIErrorEnvelope.Error.Type;
			}
			return (message: openAIErrorEnvelope.Error.Message, code: item);
		}
		catch
		{
			return (message: null, code: null);
		}
	}
}
