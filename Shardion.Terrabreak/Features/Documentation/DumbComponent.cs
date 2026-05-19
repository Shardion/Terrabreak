using System.Text.Json;
using System.Text.Json.Serialization;
using NetCord;
using NetCord.JsonConverters;
using NetCord.JsonModels;
using NetCord.Rest;

namespace Shardion.Terrabreak.Features.Documentation;

// FIXME: This class pretends to be a new type of Components V2 component, but
// is actually only a wrapper for a JSON element, and serializes into it
public class DumbComponent(JsonElement baseElement) : IMessageComponentProperties
{
    public void WriteTo(Utf8JsonWriter writer)
    {
        baseElement.WriteTo(writer);
    }

    // This may be mildly illegal...
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("id")]
    public int? Id
    {
        get => baseElement.GetProperty("value"u8).GetInt32();
        set { }
    }

    [JsonPropertyName("type")] public ComponentType ComponentType => (ComponentType)baseElement.GetProperty("type"u8).GetInt32();
}
