using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Itecnous.AI.OpenAI.Models.Files;

namespace Itecnous.AI.OpenAI.Abstractions;

public interface IFilesClient
{
	Task<FileUploadResponse> UploadAsync(string filePath, string purpose, Dictionary<string, string>? metadata = null, int? expiresAfterSeconds = null, string expiresAfterAnchor = "created_at", CancellationToken ct = default(CancellationToken));

	Task<FileData> GetAsync(string fileId, CancellationToken ct = default(CancellationToken));

	Task<FilesList> ListAsync(CancellationToken ct = default(CancellationToken));

	Task<bool> DeleteAsync(string fileId, CancellationToken ct = default(CancellationToken));

	Task<byte[]> DownloadContentAsync(string fileId, CancellationToken ct = default(CancellationToken));
}
