using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Itecnous.AI.OpenAI.Models.Embeddings;

namespace Itecnous.AI.OpenAI.Abstractions;

public interface IEmbeddingsClient
{
	Task<EmbeddingResponse> CreateAsync(string input, string? model = null, int? dimensions = null, CancellationToken ct = default(CancellationToken));

	Task<EmbeddingResponse> CreateBatchAsync(IEnumerable<string> inputs, string? model = null, int? dimensions = null, CancellationToken ct = default(CancellationToken));
}
