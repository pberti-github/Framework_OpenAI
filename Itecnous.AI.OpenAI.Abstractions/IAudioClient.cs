using System.Threading;
using System.Threading.Tasks;

namespace Itecnous.AI.OpenAI.Abstractions;

/// <summary>
/// Contrato de bajo nivel para la API de audio.
/// </summary>
public interface IAudioClient
{
	/// <summary>
	/// Transcribe un archivo de audio con Whisper.
	/// </summary>
	Task<string> TranscribeAsync(string filePath, CancellationToken ct = default(CancellationToken));
}
