using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Assistants;

public record MessageAttachment([property: JsonProperty("file_id")] string FileId, [property: JsonProperty("tools")] object[]? Tools = null);
