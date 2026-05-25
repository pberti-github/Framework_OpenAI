using System.Threading;
using System.Threading.Tasks;
using Itecnous.AI.OpenAI.Models.Responses;

namespace Itecnous.AI.OpenAI.Abstractions;

public interface IResponsesClient
{
	Task<ResponseCreateResult> CreateAsync(ResponseCreateRequest request, CancellationToken ct = default(CancellationToken));

	Task<ResponseCreateResult> CreateWithFileAsync(string model, string instructions, string userText, string fileId, double? temperature = null, double? topP = null, int? maxOutputTokens = null, string? responseFormatType = null, CancellationToken ct = default(CancellationToken));
}
