using System.Collections.Generic;
using SharpToken;

namespace Itecnous.AI.OpenAI.Services;

public class TokenCounterService
{
	private readonly GptEncoding _encoding;

	public TokenCounterService()
	{
		_encoding = GptEncoding.GetEncoding("cl100k_base");
	}

	public int CountTokens(string? text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return 0;
		}
		return _encoding.Encode(text).Count;
	}
}
