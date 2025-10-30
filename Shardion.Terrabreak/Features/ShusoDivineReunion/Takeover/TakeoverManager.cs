using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.JsonModels;
using NetCord.Rest;
using Quartz;
using Serilog;
using Shardion.Terrabreak.Features.Documentation;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Utilities;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;

public class TakeoverManager(RestClient discord, IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, ISchedulerFactory schedulerFactory, DocumentationManager documentationManager) : ITerrabreakFeature
{
    public static readonly FrozenDictionary<ulong, ulong> TakeoverAnnouncements = new Dictionary<ulong, ulong>
    {
        // NBR testing
        [1005478985070817410] = 1133252094921560114,
        // Ench Table
        [712393733995495464] = 712396995725230135,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<ulong, ulong> TakeoverChangelogs = new Dictionary<ulong, ulong>
    {
        // NBR testing
        [1005478985070817410] = 1427066816768774265,
        // Ench Table
        [712393733995495464] = 736334186431905903,
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<ulong, FrozenDictionary<ulong, string>> TakeoverMap =
        new Dictionary<ulong, FrozenDictionary<ulong, string>>
        {
            // NBR testing
            [1005478985070817410] = new Dictionary<ulong, string>
            {
                // ret-rat
                [1431454891242688542] = "RetRat",
                // raz-rat
                [1431454908946710660] = "RazRat",
                // broccoli-man
                [1431454937723703428] = "BroccoliMan",
                // cow-demon
                [1431455006116151369] = "CowDemon",
                // mister-bones
                [1431455023187103835] = "MisterBones",
                // bgn-7-nanite
                [1432245442049216584] = "Nanite",
                // stationary-monster
                [1432259454107123904] = "StationaryMonster",
                // the-legendary-warrior
                [1432259471052116120] = "LegendaryWarrior",
                // plague-master
                [1432986988918870028] = "PlagueMaster",
                // weregamer
                [1432986997986689045] = "Weregamer",
                // bgn-4-line-breaker
                [1433184814844084224] = "LineBreaker",
                // blenderman
                [1433198943151587430] = "Blenderman",
                // brimbeast
                [1433198954908225737] = "Brimbeast",
                // extinction-ball
                [1433201762059423954] = "ExtinctionBall",
                // silkseeker
                [1433241023026565170] = "Silkseeker",
                // life-bloom
                [1433245426361831484] = "LifeBloom",
                // formless
                [1433255354787168276] = "Formless",
                // box-god
                [1433271696676094032] = "BOXGOD",
            }.ToFrozenDictionary(),
            // Ench Table
            [712393733995495464] = new Dictionary<ulong, string>()
            {
                // general
                [741037240276484116] = "RetRat",
                // botspam-and-shit-and-ech
                [712427632632791050] = "RazRat",
                // multimemer
                [744222288739565709] = "BroccoliMan",
                // arknights-arknights-arknights-arknights-arknights-arknights-arknights
                [1247892880392060948] = "CowDemon",
                // rogue-adjacents
                [1247899455231885364] = "MisterBones",
                // no-mic
                [712402880816218212] = "Nanite",
                // wcropolix-not-verified
                [1247890703745552454] = "StationaryMonster",
                // mrbeast-monkey
                [1247896113470640158] = "LegendaryWarrior",
                // stack-o-smore-hexecontetessarasmingot
                [1387555394674491423] = "PlagueMaster",
                // golf-mod-and-pokemon-news
                [753295694550401083] = "Weregamer",
                // ozone-october
                [1247892169482833922] = "LineBreaker",
                // ive-come-to-make-an-announcement
                [736334186431905903] = "Blenderman",
                // isaac-and-pvz-moment
                [838923978742956044] = "Brimbeast",
                // modd
                [753295672433967125] = "ExtinctionBall",
                // 22⅔-hornet-squad
                [1413084673416106055] = "Silkseeker",
                // starboard
                [799754171568881704] = "LifeBloom",
                // brims-announcements-3
                [712397834174988368] = "Formless",
                // important-shit
                [712396995725230135] = "BOXGOD",

            }.ToFrozenDictionary(),
        }.ToFrozenDictionary();

    public DateTimeOffset TakeoverTimestamp { get; set; } = new(new(2025, 10, 31), new(0, 0), TimeSpan.Zero);
    public byte[]? ProfilePictureBytes { get; set; }

    public async Task StartAsync()
    {
        _ = await LoadProfilePictureAsync();
        // TODO: Ideally would never touch the DB, but could be made better by only adding the jobs if it doesn't exist
        IScheduler scheduler = await schedulerFactory.GetScheduler();
        await scheduler.DeleteJob(new JobKey("takeoverServerJob", "shusoDivineReunion"));
        IJobDetail job = JobBuilder.Create<TakeoverServerJob>()
            .WithIdentity("takeoverServerJob", "shusoDivineReunion")
            .Build();
        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("takeoverServerTrigger", "shusoDivineReunion")
            .StartAt(TakeoverTimestamp)
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }

    public async Task TakeoverServerAsync(ulong serverId, bool force = false)
    {
        if (!TakeoverMap.TryGetValue(serverId, out FrozenDictionary<ulong, string>? maybeTakeoverMap) || maybeTakeoverMap is not FrozenDictionary<ulong, string> takeoverMap)
        {
            Log.Error("Tried to take over a server that doesn't have a takeover map, skipping. This is a bug.");
            return;
        }

        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        SdrServer server = await dbContext.GetOrCreateServerAsync(serverId);
        if (server.TakenOver && !force)
        {
            Log.Error("Without force, tried to take over a server that's already been taken over, skipping. This is a bug.");
            return;
        }

        foreach (KeyValuePair<ulong, string> pair in takeoverMap)
        {
            await Task.WhenAny(TakeoverChannelImmediatelyAsync(pair.Key, pair.Value), Task.Delay(TimeSpan.FromSeconds(60)));
        }

        if (!TakeoverAnnouncements.TryGetValue(serverId, out ulong announcementsChannelId))
        {
            Log.Error("Tried to take over a server that doesn't have a takeover announcements channel. This is a bug.");
        }
        else
        {
            byte[]? maybeProfilePictureBytes = await LoadProfilePictureAsync();
            ImageProperties? profilePicture = null;
            if (maybeProfilePictureBytes is byte[] profilePictureBytes)
            {
                profilePicture = new ImageProperties(ImageFormat.WebP, profilePictureBytes);
            }
            IncomingWebhook webhook = await discord.CreateWebhookAsync(announcementsChannelId, new("BOX GOD")
            {
                Avatar = profilePicture,
            });
            await webhook.ExecuteAsync(new()
            {
                Components = [
                    new ComponentContainerProperties(
                        [
                            new TextDisplayProperties("# BOX GOD has taken over!"),
                            new TextDisplayProperties("He and his subjects have invaded Ench Table's channels!"),
                            new TextDisplayProperties("Go to <#741037240276484116>, drive them out with `/liberate`, upgrade your gear at `/shop`, then find more invaders with `/next`!"),
                            new TextDisplayProperties("Learn more with `/overview`!")
                        ])
                ],
                Flags = MessageFlags.IsComponentsV2,
            });
        }

        if (!TakeoverChangelogs.TryGetValue(serverId, out ulong changelogsChannelId))
        {
            Log.Error("Tried to take over a server that doesn't have a takeover changelogs channel. This is a bug.");
        }
        else
        {
            if (!documentationManager.Changelogs.TryGetValue("4.1", out JsonComponent? maybeChangelog) || maybeChangelog is not JsonComponent changelog)
            {
                Log.Error("Tried to take over a server that doesn't have a takeover changelogs channel. This is a bug.");
            }
            else
            {
                await discord.SendMessageAsync(changelogsChannelId, new()
                {
                    Components = [new DumbComponent(changelog)],
                    Flags = MessageFlags.IsComponentsV2,
                });
            }
        }

        server.TakenOver = true;
        dbContext.Update(server);
        await dbContext.SaveChangesAsync();
    }

    public async Task TakeoverChannelImmediatelyAsync(ulong channelId, string enemyId)
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        SdrChannel? maybeSdrChannel = dbContext.GetChannel(channelId);
        if (maybeSdrChannel is not null)
        {
            Log.Warning("Tried to take over a channel that's already been taken over, skipping. This is a bug.");
            return;
        }
        Channel maybeChannel = await discord.GetChannelAsync(channelId);
        if (maybeChannel is not IGuildChannel guildChannel)
        {
            Log.Warning("Tried to take over a non-guild channel, skipping. This is a bug.");
            return;
        }
        Task renameTask = discord.ModifyGuildChannelAsync(channelId, options =>
        {
            // This returns the number of Char (type) used to encode the string
            // Char encodes a single UTF-16 code unit
            // Discord counts channel name length in UTF-16 code units, so we do the same here
            if (guildChannel.Name.Length > 96)
            {
                options.Name = "box-" + TextParsingUtil.TrimToCodeUnitLength(guildChannel.Name, 96);
            }
            else
            {
                options.Name = "box-" + guildChannel.Name;
            }
        });
        dbContext.Add(new SdrChannel
        {
            ChannelId = channelId,
            ServerId = guildChannel.GuildId,
            OriginalName = guildChannel.Name,
            CaptorId = enemyId,
            TakenOver = true,
        });
        Task saveTask = dbContext.SaveChangesAsync();
        await Task.WhenAll(renameTask, saveTask);
    }

    public async Task RelinquishChannelImmediatelyAsync(ulong channelId)
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        if (dbContext.GetChannel(channelId) is not SdrChannel { TakenOver: true } channel)
        {
            Log.Warning("Tried to relinquish a channel that has not been taken over, skipping. This is a bug.");
            return;
        }
        Task renameTask = discord.ModifyGuildChannelAsync(channelId, options =>
        {
            options.Name = channel.OriginalName;
        });
        channel.TakenOver = false;
        dbContext.Update(channel);
        Task saveTask = dbContext.SaveChangesAsync();
        await Task.WhenAll(renameTask, saveTask);
    }

    public async Task RelinquishChannelAsync(TerrabreakDatabaseContext dbContext, ulong channelId)
    {
        if (dbContext.GetChannel(channelId) is not SdrChannel { TakenOver: true } channel)
        {
            Log.Warning("Tried to relinquish a channel that has not been taken over, skipping. This is a bug.");
            return;
        }
        Task renameTask = discord.ModifyGuildChannelAsync(channelId, options =>
        {
            options.Name = channel.OriginalName;
        });
        channel.TakenOver = false;
        dbContext.Update(channel);
        await renameTask;
    }

    public async Task<byte[]?> LoadProfilePictureAsync()
    {
        if (ProfilePictureBytes is byte[] existingProfilePictureBytes)
        {
            return existingProfilePictureBytes;
        }

        string profilePictureFilename =
            Path.Join(Entrypoint.GetConfigurationDirectoryPath(), "profile-picture.webp");
        byte[] profilePictureBytes;
        try
        {
            profilePictureBytes = await File.ReadAllBytesAsync(profilePictureFilename);
        }
        catch (FileNotFoundException)
        {
            Log.Information(
                $"Profile picture file expected at `{profilePictureFilename}`, but it does not exist. Not loading profile picture.");
            return null;
        }
        catch (DirectoryNotFoundException)
        {
            Log.Information(
                $"Configuration directory not found while trying to read `{profilePictureFilename}`. Not loading profile picture.");
            return null;
        }
        catch (PathTooLongException)
        {
            Log.Error("Profile picture file path is too long. Not loading profile picture.");
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            Log.Error(
                $"Profile picture file expected at `{profilePictureFilename}`, but path refers to a directory. Not loading profile picture.");
            return null;
        }

        ProfilePictureBytes = profilePictureBytes;
        return profilePictureBytes;
    }
}
