using System;
using System.Net;

namespace Itecnous.AI.OpenAI.Errors;

public class OpenAIClientException : Exception
{
	public HttpStatusCode StatusCode { get; }

	public string? OpenAIErrorCode { get; }

	public string? RawBody { get; }

	public OpenAIClientException(string message, HttpStatusCode statusCode, string? openAIErrorCode = null, string? rawBody = null, Exception? inner = null)
		: base(message, inner)
	{
		StatusCode = statusCode;
		OpenAIErrorCode = openAIErrorCode;
		RawBody = rawBody;
	}
}
