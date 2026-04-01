using System.Collections.Generic;
using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Services.Emoji;

public sealed class EmojiOptions : IDynamicOptions
{
    public string SectionName => "Emoji";
    public OptionsPermissions Permissions => new()
    {
        Bot = OptionsAccessibility.ReadWrite,
        Servers = OptionsAccessibility.None,
        Users = OptionsAccessibility.None
    };

    public Dictionary<string, ulong> EmojiList { get; set; } = [];
}
