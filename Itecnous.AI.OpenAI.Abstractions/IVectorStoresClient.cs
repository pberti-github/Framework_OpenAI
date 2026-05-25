using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Itecnous.AI.OpenAI.Models.VectorStores;

namespace Itecnous.AI.OpenAI.Abstractions;

/// <summary>
/// Contrato de bajo nivel para la API de Vector Stores.
/// </summary>
public interface IVectorStoresClient
{
	/// <summary>
	/// Crea un vector store nuevo.
	/// </summary>
	Task<VectorStore> CreateAsync(string name, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Lista los vector stores disponibles.
	/// </summary>
	Task<VectorStoresList> ListAsync(CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Recupera un vector store por identificador.
	/// </summary>
	Task<VectorStore> RetrieveAsync(string id, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Elimina un vector store.
	/// </summary>
	Task<bool> DeleteAsync(string vectorStoreId, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Agrega un archivo a un vector store.
	/// </summary>
	Task<VectorStoreFile> AddFileAsync(string vectorStoreId, string fileId, object? attributes = null, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Recupera un archivo asociado a un vector store.
	/// </summary>
	Task<VectorStoreFile> RetrieveFileAsync(string vectorStoreId, string fileId, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Actualiza los atributos de un archivo asociado a un vector store.
	/// </summary>
	Task<bool> UpdateFileAttributesAsync(string vectorStoreId, string fileId, Dictionary<string, object> attributes, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Lista los archivos de un vector store.
	/// </summary>
	Task<VectorStoreFilesList> ListFilesAsync(string vectorStoreId, CancellationToken ct = default(CancellationToken));

	/// <summary>
	/// Desasocia un archivo de un vector store.
	/// </summary>
	Task<bool> RemoveFileAsync(string vectorStoreId, string fileId, CancellationToken ct = default(CancellationToken));
}
