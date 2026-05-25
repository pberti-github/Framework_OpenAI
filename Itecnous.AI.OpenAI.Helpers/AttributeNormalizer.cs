using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Itecnous.AI.OpenAI.Helpers;

public static class AttributeNormalizer
{
	public const int MaxKeyLength = 64;

	public const int MaxStringLength = 512;

	public const int MaxArrayItems = 5;

	public const int MaxArrayItemLength = 64;

	public static Dictionary<string, object> NormalizeFromPairs(IEnumerable<KeyValuePair<string, object?>> pairs)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		foreach (KeyValuePair<string, object?> pair in pairs)
		{
			string text = SanitizeKey(pair.Key);
			if (!string.IsNullOrWhiteSpace(text))
			{
				object? obj = NormalizeValue(pair.Value);
				if (obj != null)
				{
					dictionary[text] = obj;
				}
			}
		}
		return dictionary;
	}

	private static string SanitizeKey(string raw)
	{
		if (string.IsNullOrWhiteSpace(raw))
		{
			return string.Empty;
		}
		string text = raw.Trim().ToLowerInvariant();
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		bool flag = false;
		string text2 = text;
		foreach (char c in text2)
		{
			char c2 = (((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_') ? c : '_');
			if (c2 == '_')
			{
				if (flag)
				{
					continue;
				}
				flag = true;
			}
			else
			{
				flag = false;
			}
			stringBuilder.Append(c2);
			if (stringBuilder.Length >= 64)
			{
				break;
			}
		}
		return stringBuilder.ToString().Trim('_');
	}

	private static object? NormalizeValue(object? value)
	{
		if (value == null)
		{
			return null;
		}
		if (value is string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return null;
			}
			return Truncate(text.Trim(), 512);
		}
		if (value is IEnumerable<string> enumerable)
		{
			List<string> list = new List<string>();
			foreach (string item in enumerable)
			{
				if (list.Count >= 5)
				{
					break;
				}
				if (!string.IsNullOrWhiteSpace(item))
				{
					string clean = Truncate(item.Trim(), 64);
					if (!list.Any((string x) => x.Equals(clean, StringComparison.OrdinalIgnoreCase)))
					{
						list.Add(clean);
					}
				}
			}
			if (list.Count <= 0)
			{
				return null;
			}
			return list;
		}
		if (value is bool || value is int || value is long || value is double || value is float || value is decimal)
		{
			return value;
		}
		string? text2 = value.ToString();
		if (string.IsNullOrWhiteSpace(text2))
		{
			return null;
		}
		return Truncate(text2.Trim(), 512);
	}

	private static string Truncate(string input, int max)
	{
		if (input.Length <= max)
		{
			return input;
		}
		return input.Substring(0, max);
	}
}
