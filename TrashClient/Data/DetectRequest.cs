using Newtonsoft.Json;

namespace TrashClient.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public record DetectRequest
    {
        [JsonProperty("key")]
        public string Key { get; init; }
    }
}