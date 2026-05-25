using System.Collections.Generic;
using System.Linq;
using Itecnous.AI.OpenAI.Models.Responses;

namespace Itecnous.AI.OpenAI.Utils;

public static class OpenAIResponseBuilders
{
	public static ResponseCreateRequest BuildRouterRequest(string model, string system, string user, double? temperature = null, double? topP = null, int? maxOutputTokens = null)
	{
		ResponseCreateRequest responseCreateRequest = new ResponseCreateRequest();
		responseCreateRequest.Model = model;
		responseCreateRequest.Messages = new object[2]
		{
			new
			{
				role = "system",
				content = system
			},
			new
			{
				role = "user",
				content = user
			}
		};
		responseCreateRequest.Text = new TextOptions
		{
			Format = new TextFormatOptions
			{
				Type = "json"
			}
		};
		responseCreateRequest.Temperature = temperature;
		responseCreateRequest.TopP = topP;
		responseCreateRequest.MaxOutputTokens = maxOutputTokens;
		return responseCreateRequest;
	}

	public static ResponseCreateRequest BuildResponderRequest(string model, string system, string user, double? temperature = null, double? topP = null, int? maxOutputTokens = null, IEnumerable<string>? vectorStoreIds = null, IDictionary<string, object>? attributeFilters = null)
	{
		ResponseCreateRequest responseCreateRequest = new ResponseCreateRequest();
		responseCreateRequest.Model = model;
		responseCreateRequest.Messages = new object[2]
		{
			new
			{
				role = "system",
				content = system
			},
			new
			{
				role = "user",
				content = user
			}
		};
		responseCreateRequest.Text = new TextOptions
		{
			Format = new TextFormatOptions
			{
				Type = "text"
			}
		};
		responseCreateRequest.Temperature = temperature;
		responseCreateRequest.TopP = topP;
		responseCreateRequest.MaxOutputTokens = maxOutputTokens;
		ResponseCreateRequest responseCreateRequest2 = responseCreateRequest;
		if (vectorStoreIds != null)
		{
			List<string> list = (from s in vectorStoreIds
				where !string.IsNullOrWhiteSpace(s)
				select s.Trim()).ToList();
			if (list.Count > 0)
			{
				if (attributeFilters != null && attributeFilters.Count > 0)
				{
					var filters = Enumerable.Select(attributeFilters, (KeyValuePair<string, object> kv) => new
					{
						type = "eq",
						key = kv.Key,
						value = kv.Value
					}).ToArray();
					var filters2 = new
					{
						type = "and",
						filters = filters
					};
					responseCreateRequest2.Tools = new object[1]
					{
						new
						{
							type = "file_search",
							vector_store_ids = list,
							filters = filters2
						}
					};
				}
				else
				{
					responseCreateRequest2.Tools = new object[1]
					{
						new
						{
							type = "file_search",
							vector_store_ids = list
						}
					};
				}
			}
		}
		return responseCreateRequest2;
	}
}
