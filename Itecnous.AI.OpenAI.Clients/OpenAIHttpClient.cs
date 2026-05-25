using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Itecnous.AI.OpenAI.Common;

namespace Itecnous.AI.OpenAI.Clients;

internal static class OpenAIHttpClient
{
	public static HttpClient Create(OpenAISettings settings, IEnumerable<string>? betaHeaders = null, string? modelForAzure = null)
	{
		bool num = settings.Provider == OpenAIProvider.Azure;
		HttpClient httpClient = new HttpClient
		{
			Timeout = TimeSpan.FromSeconds(120.0)
		};
		if (num)
		{
			if (string.IsNullOrWhiteSpace(settings.AzureEndpoint))
			{
				throw new ArgumentNullException("AzureEndpoint", "AzureEndpoint es requerido cuando Provider es Azure.");
			}
			string text = settings.AzureEndpoint.TrimEnd('/');
			string uriString;
			if (!string.IsNullOrEmpty(modelForAzure))
			{
				if (!settings.DeploymentMappings.TryGetValue(modelForAzure, out string value))
				{
					value = modelForAzure;
				}
				uriString = text + "/openai/deployments/" + value + "/";
			}
			else
			{
				uriString = text + "/openai/";
			}
			httpClient.BaseAddress = new Uri(uriString);
			httpClient.DefaultRequestHeaders.Add("api-key", settings.ApiKey);
		}
		else
		{
			httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
			if (!string.IsNullOrWhiteSpace(settings.OrganizationId))
			{
				httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", settings.OrganizationId);
			}
			if (!string.IsNullOrWhiteSpace(settings.ProjectId))
			{
				httpClient.DefaultRequestHeaders.Add("OpenAI-Project", settings.ProjectId);
			}
		}
		if (betaHeaders != null)
		{
			foreach (string betaHeader in betaHeaders)
			{
				if (!string.IsNullOrWhiteSpace(betaHeader))
				{
					httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", betaHeader.Trim());
				}
			}
		}
		return httpClient;
	}
}
