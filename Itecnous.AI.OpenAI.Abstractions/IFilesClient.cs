using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Itecnous.AI.OpenAI.Models.Files;

namespace Itecnous.AI.OpenAI.Abstractions;

/// <summary>
/// Contrato de bajo nivel para la API de Files.
/// </summary>
public interface IFilesClient
{
	/// <summary>
	/// Sube un archivo a OpenAI.
	/// </summary>
	Task<FileUploadResponse> UploadAsync(string filePath, string purpose, Dictionary<string, string>? metadata = null, int? expiresAfterSeconds = null, string expiresAfterAnchor = "created_at", CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Obtiene la metadata de un archivo.
	/// </summary>
	Task<FileData> GetAsync(string fileId, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Lista los archivos disponibles.
	/// </summary>
	Task<FilesList> ListAsync(CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Elimina un archivo por su identificador.
	/// </summary>
	Task<bool> DeleteAsync(string fileId, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Descarga el contenido binario de un archivo.
	/// </summary>
	Task<byte[]> DownloadContentAsync(string fileId, CancellationToken ct = default(CancellationToken));
}
