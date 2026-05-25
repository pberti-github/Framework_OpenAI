using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Itecnous.AI.OpenAI.Models.Embeddings;

namespace Itecnous.AI.OpenAI.Abstractions;

/// <summary>
/// Contrato de bajo nivel para la API de Embeddings.
/// </summary>
public interface IEmbeddingsClient
{
	/// <summary>
	/// Genera un embedding para un texto de entrada.
	/// </summary>
	Task<EmbeddingResponse> CreateAsync(string input, string? model = null, int? dimensions = null, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Genera embeddings para una coleccion de textos.
	/// </summary>
	Task<EmbeddingResponse> CreateBatchAsync(IEnumerable<string> inputs, string? model = null, int? dimensions = null, CancellationToken ct = default(CancellationToken));
}
