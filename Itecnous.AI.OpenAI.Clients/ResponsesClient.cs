using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Itecnous.AI.OpenAI.Abstractions;
using Itecnous.AI.OpenAI.Models.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Itecnous.AI.OpenAI.Clients;

/// <summary>
/// Cliente de bajo nivel para la API de Responses.
/// </summary>
public class ResponsesClient : IResponsesClient
{
	private readonly HttpClient _httpClient;

	public ResponsesClient(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	private static string LeerTexto(JObject root, string propiedad)
	{
		JToken? token = root[propiedad];
		if (token == null)
		{
			return string.Empty;
		}
		return token.ToString();
	}

	private static string LeerTexto(JToken? token)
	{
		if (token == null)
		{
			return string.Empty;
		}
		return token.ToString();
	}

	private static int? LeerEntero(JToken? token)
	{
		if (token == null)
		{
			return null;
		}
		if (int.TryParse(token.ToString(), out int valor))
		{
			return valor;
		}
		return null;
	}

	/// <summary>
	/// Crea una respuesta usando la API Responses.
	/// </summary>
	public async Task<ResponseCreateResult> CreateAsync(ResponseCreateRequest request, CancellationToken ct = default(CancellationToken))
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>
		{
			["model"] = request.Model,
			["input"] = request.Messages
		};
		if (!string.IsNullOrWhiteSpace(request.PreviousResponseId))
		{
			dictionary["previous_response_id"] = request.PreviousResponseId;
		}
		if (request.Temperature.HasValue)
		{
			dictionary["temperature"] = request.Temperature.Value;
		}
		if (request.TopP.HasValue)
		{
			dictionary["top_p"] = request.TopP.Value;
		}
		if (request.MaxOutputTokens.HasValue)
		{
			dictionary["max_output_tokens"] = request.MaxOutputTokens.Value;
		}
		if (request.Tools != null && request.Tools.Length != 0)
		{
			dictionary["tools"] = request.Tools;
		}
		if (request.ToolResources != null)
		{
			dictionary["tool_resources"] = request.ToolResources;
		}
		if (request.ToolChoice != null)
		{
			dictionary["tool_choice"] = request.ToolChoice;
		}
		if (!string.IsNullOrWhiteSpace(request.Conversation))
		{
			dictionary["conversation"] = request.Conversation;
		}
		if (request.Text != null)
		{
			dictionary["text"] = request.Text;
		}
		else if (request.ResponseFormat != null)
		{
			try
			{
				JToken val = JToken.FromObject(request.ResponseFormat);
				dictionary["text"] = (object)new JObject { ["format"] = val };
			}
			catch
			{
			}
		}
		StringContent content = new StringContent(JsonConvert.SerializeObject((object)dictionary, new JsonSerializerSettings
		{
			NullValueHandling = (NullValueHandling)1
		}), Encoding.UTF8, "application/json");
		HttpResponseMessage response = await _httpClient.PostAsync("responses", content, ct);
		string responseBody = await response.Content.ReadAsStringAsync(ct);
		if (!response.IsSuccessStatusCode)
		{
			throw new HttpRequestException($"OpenAI API Error ({response.StatusCode}): {responseBody}");
		}
		JObject val2 = JObject.Parse(responseBody);
		string outputText = ExtraerTextoSalida(val2);
		ResponsesUsage usage = new ResponsesUsage
		{
			PromptTokens = LeerEnteroNullable(val2, "usage.input_tokens", "usage.inputTokens", "usage.prompt_tokens", "usage.promptTokens"),
			CompletionTokens = LeerEnteroNullable(val2, "usage.output_tokens", "usage.outputTokens", "usage.completion_tokens", "usage.completionTokens"),
			TotalTokens = LeerEnteroNullable(val2, "usage.total_tokens", "usage.totalTokens")
		};
		string id = LeerTexto(val2, "id");
		ResponseCreateResult responseCreateResult = new ResponseCreateResult
		{
			Id = id,
			ResponseId = id,
			Usage = usage,
			OutputText = outputText,
			Model = LeerTexto(val2, "model")
		};
		if (response.Headers.TryGetValues("openai-conversation-id", out IEnumerable<string>? values))
		{
			foreach (string value in values)
			{
				if (!string.IsNullOrWhiteSpace(value))
				{
					responseCreateResult.ConversationId = value;
					break;
				}
			}
		}
		return responseCreateResult;
	}

	private static string ExtraerTextoSalida(JObject root)
	{
		string texto = LeerTexto(root, "output_text");
		if (!string.IsNullOrWhiteSpace(texto))
		{
			return texto.Trim();
		}
		JToken? output = root["output"];
		if (output != null)
		{
			foreach (JToken item in output.Children())
			{
				if (string.Equals(item.Value<string>("type"), "message", StringComparison.OrdinalIgnoreCase))
				{
					JToken? content = item["content"];
					if (content != null)
					{
						foreach (JToken contentItem in content.Children())
						{
							if (string.Equals(contentItem.Value<string>("type"), "output_text", StringComparison.OrdinalIgnoreCase))
							{
								string text = LeerTexto(contentItem["text"]);
								if (!string.IsNullOrWhiteSpace(text))
								{
									return text.Trim();
								}
							}
						}
					}
				}
			}
		}
		return string.Empty;
	}

	private static int? LeerEnteroNullable(JObject root, params string[] paths)
	{
		foreach (string path in paths)
		{
			JToken? token = root.SelectToken(path);
			int? valor = LeerEntero(token);
			if (valor.HasValue)
			{
				return valor.Value;
			}
		}
		return null;
	}

	/// <summary>
	/// Crea una respuesta asociando un archivo a la entrada.
	/// </summary>
	public async Task<ResponseCreateResult> CreateWithFileAsync(string model, string instructions, string userText, string fileId, double? temperature = null, double? topP = null, int? maxOutputTokens = null, string? responseFormatType = null, CancellationToken ct = default(CancellationToken))
	{
		if (!string.IsNullOrWhiteSpace(responseFormatType) && (responseFormatType.Equals("json", StringComparison.OrdinalIgnoreCase) || responseFormatType.Equals("json_object", StringComparison.OrdinalIgnoreCase)))
		{
			string text = userText;
			if (string.IsNullOrWhiteSpace(text))
			{
				text = string.Empty;
			}
			if (!text.Contains("json", StringComparison.OrdinalIgnoreCase))
			{
				text = (text + " Devuelve la salida en json válido.").Trim();
			}
			userText = text;
		}
		StringContent content = new StringContent(JsonConvert.SerializeObject((object)new
		{
			model = model,
			instructions = instructions,
			input = new object[1]
			{
				new
				{
					role = "user",
					content = new object[2]
					{
						new
						{
							type = "input_text",
							text = userText
						},
						new
						{
							type = "input_file",
							file_id = fileId
						}
					}
				}
			},
			temperature = temperature,
			top_p = topP,
			max_output_tokens = maxOutputTokens,
			text = (string.IsNullOrWhiteSpace(responseFormatType) ? null : new
			{
				format = new
				{
					type = ((responseFormatType.ToLowerInvariant() == "json") ? "json_object" : "text")
				}
			})
		}, new JsonSerializerSettings
		{
			NullValueHandling = (NullValueHandling)1
		}), Encoding.UTF8, "application/json");
		HttpResponseMessage response = await _httpClient.PostAsync("responses", content, ct);
		if (!response.IsSuccessStatusCode)
		{
			string value = await response.Content.ReadAsStringAsync(ct);
			throw new HttpRequestException($"OpenAI API Error ({response.StatusCode}): {value}");
		}
		JObject val = JObject.Parse(await response.Content.ReadAsStringAsync(ct));
		string outputText = string.Empty;
		JToken? output = val["output"];
		if (output != null)
		{
			foreach (JToken item in output.Children())
			{
				if (!string.Equals(item.Value<string>("type"), "message", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				JToken? contentNode = item["content"];
				if (contentNode == null)
				{
					continue;
				}
				foreach (JToken contentItem in contentNode.Children())
				{
					if (!string.Equals(contentItem.Value<string>("type"), "output_text", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					string text = LeerTexto(contentItem["text"]);
					if (!string.IsNullOrWhiteSpace(text))
					{
						outputText = text;
						break;
					}
				}
				if (!string.IsNullOrWhiteSpace(outputText))
				{
					break;
				}
			}
		}
		ResponsesUsage responsesUsage = new ResponsesUsage();
		JToken? usage = val["usage"];
		if (usage != null)
		{
			responsesUsage.PromptTokens = LeerEntero(usage["input_tokens"]);
			responsesUsage.CompletionTokens = LeerEntero(usage["output_tokens"]);
			responsesUsage.TotalTokens = LeerEntero(usage["total_tokens"]);
		}
		return new ResponseCreateResult
		{
			Id = LeerTexto(val, "id"),
			ResponseId = LeerTexto(val, "id"),
			OutputText = outputText,
			Model = LeerTexto(val, "model"),
			Usage = responsesUsage
		};
	}
}
