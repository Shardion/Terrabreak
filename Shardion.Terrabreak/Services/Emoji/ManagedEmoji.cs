using System.Globalization;
using NetCord;

namespace Shardion.Terrabreak.Services.Emoji;

public record ManagedEmoji(string Name, ulong Id)
{
    public EmojiProperties ToProperties()
    {
        return EmojiProperties.Custom(Id).WithName(Name);
    }

    public override string ToString()
    {
        return $"<:{Name}:{Id.ToString(CultureInfo.InvariantCulture)}>";
    }
}
