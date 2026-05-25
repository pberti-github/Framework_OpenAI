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

public class ResponsesClient : IResponsesClient
{
	private readonly HttpClient _httpClient;

	public ResponsesClient(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

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
		ResponseCreateResult responseCreateResult = new ResponseCreateResult
		{
			Id = (((object)val2["id"])?.ToString() ?? string.Empty),
			ResponseId = ((object)val2["id"])?.ToString(),
			Usage = usage,
			OutputText = outputText,
			Model = ((object)val2["model"])?.ToString()
		};
		if (response.Headers.TryGetValues("openai-conversation-id", out IEnumerable<string> values))
		{
			responseCreateResult.ConversationId = values.FirstOrDefault();
		}
		return responseCreateResult;
	}

	private static string ExtraerTextoSalida(JObject root)
	{
		string texto = root.Value<string>("output_text");
		if (!string.IsNullOrWhiteSpace(texto))
		{
			return texto.Trim();
		}
		JToken output = root["output"];
		if (output != null)
		{
			foreach (JToken item in output.Children())
			{
				if (string.Equals(item.Value<string>("type"), "message", StringComparison.OrdinalIgnoreCase))
				{
					JToken content = item["content"];
					if (content != null)
					{
						foreach (JToken contentItem in content.Children())
						{
							if (string.Equals(contentItem.Value<string>("type"), "output_text", StringComparison.OrdinalIgnoreCase))
							{
								string text = contentItem.Value<string>("text");
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
			JToken token = root.SelectToken(path);
			if (token != null && int.TryParse(token.ToString(), out int valor))
			{
				return valor;
			}
		}
		return null;
	}

	public async Task<ResponseCreateResult> CreateWithFileAsync(string model, string instructions, string userText, string fileId, double? temperature = null, double? topP = null, int? maxOutputTokens = null, string? responseFormatType = null, CancellationToken ct = default(CancellationToken))
	{
		if (!string.IsNullOrWhiteSpace(responseFormatType) && (responseFormatType.Equals("json", StringComparison.OrdinalIgnoreCase) || responseFormatType.Equals("json_object", StringComparison.OrdinalIgnoreCase)))
		{
			string text = userText ?? string.Empty;
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
		JToken obj = val["output"];
		object obj2;
		if (obj == null)
		{
			obj2 = null;
		}
		else
		{
			JToken? obj3 = ((IEnumerable<JToken>)obj).FirstOrDefault((Func<JToken, bool>)((JToken t) => string.Equals((string)((t != null) ? t[(object)"type"] : null), "message", StringComparison.OrdinalIgnoreCase)));
			if (obj3 == null)
			{
				obj2 = null;
			}
			else
			{
				JToken obj4 = obj3[(object)"content"];
				if (obj4 == null)
				{
					obj2 = null;
				}
				else
				{
					JToken? obj5 = ((IEnumerable<JToken>)obj4).FirstOrDefault((Func<JToken, bool>)((JToken t) => string.Equals((string)((t != null) ? t[(object)"type"] : null), "output_text", StringComparison.OrdinalIgnoreCase)));
					obj2 = ((obj5 == null) ? null : ((object)obj5[(object)"text"])?.ToString());
				}
			}
		}
		if (obj2 == null)
		{
			obj2 = string.Empty;
		}
		string outputText = (string)obj2;
		ResponsesUsage responsesUsage = new ResponsesUsage();
		JToken obj6 = val["usage"];
		int? promptTokens;
		if (obj6 == null)
		{
			promptTokens = null;
		}
		else
		{
			JToken obj7 = obj6[(object)"input_tokens"];
			promptTokens = obj7?.Value<int?>();
		}
		responsesUsage.PromptTokens = promptTokens;
		JToken obj8 = val["usage"];
		int? completionTokens;
		if (obj8 == null)
		{
			completionTokens = null;
		}
		else
		{
			JToken obj9 = obj8[(object)"output_tokens"];
			completionTokens = obj9?.Value<int?>();
		}
		responsesUsage.CompletionTokens = completionTokens;
		JToken obj10 = val["usage"];
		int? totalTokens;
		if (obj10 == null)
		{
			totalTokens = null;
		}
		else
		{
			JToken obj11 = obj10[(object)"total_tokens"];
			totalTokens = obj11?.Value<int?>();
		}
		responsesUsage.TotalTokens = totalTokens;
		ResponsesUsage usage = responsesUsage;
		return new ResponseCreateResult
		{
			Id = (((object)val["id"])?.ToString() ?? string.Empty),
			ResponseId = ((object)val["id"])?.ToString(),
			OutputText = outputText,
			Model = ((object)val["model"])?.ToString(),
			Usage = usage
		};
	}
}
