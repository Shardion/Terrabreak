using Shardion.Terrabreak.Services.Timeout;
using Shardion.Terrabreak.Services.Discord;
using Serilog;
using System.Threading.Tasks;
using System;
using System.Text.Json;
using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Features.Sudo
{
    public class SudoFeature : ITerrabreakFeature
    {
        private readonly DiscordManager _discordManager;
        private readonly TimeoutManager _timeoutManager;
        private readonly OptionsManager _options;

        public SudoFeature(DiscordManager discordManager, TimeoutManager timeoutManager, OptionsManager options)
        {
            _discordManager = discordManager;
            _timeoutManager = timeoutManager;
            _options = options;
            _timeoutManager.TimeoutExpired += async (timeout) =>
            {
                try
                {
                    if (timeout.Identifier != "sudo")
                    {
                        return;
                    }

                    using JsonDocument document = JsonDocument.Parse(timeout.Data);

                    if (document.RootElement.ValueKind != JsonValueKind.Object)
                    {
                        throw new JsonException("JSON is so badly malformed that I have no idea what to do with it");
                    }

                    ulong uid;
                    if (!document.RootElement.TryGetProperty("UserId"u8, out JsonElement unparsedUid) || unparsedUid.ValueKind != JsonValueKind.String || !ulong.TryParse(unparsedUid.ToString(), out ulong parsedUid))
                    {
                        throw new JsonException("Failed to parse user ID");
                    }
                    else
                    {
                        uid = parsedUid;
                    }

                    ulong gid;
                    if (!document.RootElement.TryGetProperty("GuildId"u8, out JsonElement unparsedGid) || unparsedGid.ValueKind != JsonValueKind.String || !ulong.TryParse(unparsedGid.GetString(), out ulong parsedGid))
                    {
                        throw new JsonException("Failed to parse guild ID");
                    }
                    else
                    {
                        gid = parsedGid;
                    }

                    SudoOptions sudoOptions = _options.Get<SudoOptions>(userId: null, serverId: gid);

                    if (sudoOptions.SudoRoleId is not ulong sudoRoleId)
                    {
                        return;
                    }

                    await _discordManager.RestClient.RemoveRoleAsync(gid, uid, sudoRoleId);
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            };
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
