using Itecnous.AI.OpenAI.Abstractions;
using Itecnous.AI.OpenAI.Clients;
using Itecnous.AI.OpenAI.Common;
using Itecnous.AI.OpenAI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itecnous.AI.OpenAI.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOpenAI(this IServiceCollection services, IConfiguration configuration, string sectionName = "OpenAI")
	{
		OpenAISettings openAISettings = new OpenAISettings();
		configuration.GetSection(sectionName).Bind(openAISettings);
		services.AddSingleton(openAISettings);
		services.AddSingleton<IAssistantsClient, AssistantsClient>();
		services.AddSingleton<IResponsesClient, ResponsesClient>();
		services.AddSingleton<IFilesClient, FilesClient>();
		services.AddSingleton<IVectorStoresClient, VectorStoresClient>();
		services.AddSingleton<IAudioClient, AudioClient>();
		services.AddSingleton<IEmbeddingsClient, EmbeddingsClient>();
		services.AddSingleton<PricingService>();
		services.AddSingleton<OpenAIResultService>();
		services.AddSingleton<TokenCounterService>();
		return services;
	}
}
