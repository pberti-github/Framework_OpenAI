using Itecnous.AI.OpenAI.Abstractions;
using Itecnous.AI.OpenAI.Clients;
using Itecnous.AI.OpenAI.Common;
using Itecnous.AI.OpenAI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itecnous.AI.OpenAI.Extensions;

/// <summary>
/// Extensiones para registrar la integracion de OpenAI en el contenedor de servicios.
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registra la configuracion, clientes y servicios de OpenAI en el contenedor DI.
	/// </summary>
	/// <param name="services">Contenedor de servicios donde se registran las dependencias.</param>
	/// <param name="configuration">Configuracion raiz de la aplicacion.</param>
	/// <param name="sectionName">Nombre de la seccion con los parametros de OpenAI.</param>
	/// <returns>El mismo contenedor de servicios para permitir encadenamiento.</returns>
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
