using System.Text.Json;
using System.Text.Json.Serialization;
using NetCord;
using NetCord.JsonModels;
using NetCord.Rest;

namespace Shardion.Terrabreak.Features.Documentation;

// Absolute mad hack: this class pretends to be a new type of Components V2 component,
// but is actually only a wrapper for a JSON component, and serializes into it,
public class DumbComponent(JsonComponent baseComponent) : IMessageComponentProperties
{
    public void WriteTo(Utf8JsonWriter writer)
    {
        JsonSerializer.Serialize(writer, baseComponent);
    }

    // This may be mildly illegal...
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("id")]
    public int? Id
    {
        get => baseComponent.Id;
        set { }
    }

    [JsonPropertyName("type")] public ComponentType ComponentType => baseComponent.Type;
}
