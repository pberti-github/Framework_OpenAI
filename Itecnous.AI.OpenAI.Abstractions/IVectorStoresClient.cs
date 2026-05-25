using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Itecnous.AI.OpenAI.Models.VectorStores;

namespace Itecnous.AI.OpenAI.Abstractions;

public interface IVectorStoresClient
{
	Task<VectorStore> CreateAsync(string name, CancellationToken ct = default(CancellationToken));

	Task<VectorStoresList> ListAsync(CancellationToken ct = default(CancellationToken));

	Task<VectorStore> RetrieveAsync(string id, CancellationToken ct = default(CancellationToken));

	Task<bool> DeleteAsync(string vectorStoreId, CancellationToken ct = default(CancellationToken));

	Task<VectorStoreFile> AddFileAsync(string vectorStoreId, string fileId, object? attributes = null, CancellationToken ct = default(CancellationToken));

	Task<VectorStoreFile> RetrieveFileAsync(string vectorStoreId, string fileId, CancellationToken ct = default(CancellationToken));

	Task<bool> UpdateFileAttributesAsync(string vectorStoreId, string fileId, Dictionary<string, object> attributes, CancellationToken ct = default(CancellationToken));

	Task<VectorStoreFilesList> ListFilesAsync(string vectorStoreId, CancellationToken ct = default(CancellationToken));

	Task<bool> RemoveFileAsync(string vectorStoreId, string fileId, CancellationToken ct = default(CancellationToken));
}
