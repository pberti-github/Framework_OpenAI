using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Errors;

internal static class OpenAIErrorParser
{
	public static (string? message, string? code) TryParse(string body)
	{
		try
		{
			OpenAIErrorEnvelope openAIErrorEnvelope = JsonConvert.DeserializeObject<OpenAIErrorEnvelope>(body);
			if (openAIErrorEnvelope?.Error == null)
			{
				return (message: null, code: null);
			}
			string item = (string.IsNullOrWhiteSpace(openAIErrorEnvelope.Error.Code) ? openAIErrorEnvelope.Error.Type : openAIErrorEnvelope.Error.Code);
			return (message: openAIErrorEnvelope.Error.Message, code: item);
		}
		catch
		{
			return (message: null, code: null);
		}
	}
}
