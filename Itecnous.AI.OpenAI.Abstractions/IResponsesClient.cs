using System.Threading;
using System.Threading.Tasks;
using Itecnous.AI.OpenAI.Models.Responses;

namespace Itecnous.AI.OpenAI.Abstractions;

/// <summary>
/// Contrato de bajo nivel para la API de Responses.
/// </summary>
public interface IResponsesClient
{
	/// <summary>
	/// Crea una respuesta usando la API Responses.
	/// </summary>
	Task<ResponseCreateResult> CreateAsync(ResponseCreateRequest request, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Crea una respuesta asociando un archivo a la entrada.
	/// </summary>
	Task<ResponseCreateResult> CreateWithFileAsync(string model, string instructions, string userText, string fileId, double? temperature = null, double? topP = null, int? maxOutputTokens = null, string? responseFormatType = null, CancellationToken ct = default(CancellationToken));
}
