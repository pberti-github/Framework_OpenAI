using Newtonsoft.Json;

namespace Itecnous.AI.OpenAI.Models.Assistants;

public record ThreadCreateResponse([property: JsonProperty("id")] string Id, [property: JsonProperty("object")] string Object, [property: JsonProperty("created_at")] long CreatedAt);
