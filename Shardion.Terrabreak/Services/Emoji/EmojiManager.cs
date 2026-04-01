using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetCord;

namespace Shardion.Terrabreak.Services.Emoji;

public class EmojiManager(EmojiOptions options) : ITerrabreakService
{
    public ManagedEmoji? GetEmojiOrDefault(string name, ManagedEmoji? defaultValue = null)
    {
        ulong? nullableId = options.EmojiList.GetValueOrDefault(name);
        if (nullableId is not ulong id)
        {
            return defaultValue;
        }
        return new ManagedEmoji(name, id);
    }

    public ManagedEmoji GetEmoji(string name)
    {
        if (GetEmojiOrDefault(name) is not ManagedEmoji emoji)
        {
            throw new NullReferenceException($"Emoji `{name}` is not present in configuration.");
        }
        return emoji;
    }

    public Task StartAsync()
    {
        return Task.CompletedTask;
    }
}
