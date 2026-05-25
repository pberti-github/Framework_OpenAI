using System.Threading;
using System.Threading.Tasks;

namespace Itecnous.AI.OpenAI.Abstractions;

public interface IAudioClient
{
	Task<string> TranscribeAsync(string filePath, CancellationToken ct = default(CancellationToken));
}
