using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;

public class SecretBossMenu(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, MenuManager menuManager, Guild server, TakeoverManager takeoverManager, LiberationFeature liberationFeature) : TerrabreakMenu
{
    public override Task<MenuMessage> BuildMessage()
    {
        return Task.FromResult(new MenuMessage([
            new ComponentContainerProperties([
                new TextDisplayProperties("**Make your choice.**"),
                new ComponentSectionProperties(new ButtonProperties($"menu:{MenuGuid}:the-singer", EmojiProperties.Custom(1426440268193857567), ButtonStyle.Primary), [new("- \"The Singer\"")]),
                new ComponentSectionProperties(new ButtonProperties($"menu:{MenuGuid}:twinespinner", EmojiProperties.Custom(1426440268193857567), ButtonStyle.Success), [new("- \"Twinespinner\"")]),
                new ComponentSectionProperties(new ButtonProperties($"menu:{MenuGuid}:parting", EmojiProperties.Custom(1426440268193857567), ButtonStyle.Danger), [new("- \"Parting\"")]),
            ])
        ]));
    }

    public override async Task OnButton(ButtonInteractionContext context)
    {
        string captor;
        if (context.Interaction.Data.CustomId.EndsWith("the-singer"))
        {
            captor = "TheSinger";
        }
        else if (context.Interaction.Data.CustomId.EndsWith("twinespinner"))
        {
            captor = "Twinespinner";
        }
        else
        {
            captor = "Parting";
        }

        SdrChannel fakeChannel = new()
        {
            CaptorId = captor,
            OriginalName = "22",
            TakenOver = false,
            ChannelId = context.Interaction.Channel.Id,
            ServerId = server.Id,
        };

        await ReplaceMenuAsync(context, menuManager, new LobbyMenu(dbContextFactory, menuManager, fakeChannel, takeoverManager, liberationFeature));
    }
}
