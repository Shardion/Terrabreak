using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using System;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using Shardion.Terrabreak.Services.Options;
using System.Net.Http.Json;
using System.Text.Json;
using Serilog;
using System.Collections.Frozen;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Utilities;

namespace Shardion.Terrabreak.Features.CobaltDownload;

public class CobaltDownloadModule(OptionsManager optionsManager)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [MessageCommand("Download Media",
        Contexts =
        [
            InteractionContextType.BotDMChannel, InteractionContextType.DMChannel, InteractionContextType.Guild
        ])]
    public async Task DownloadMedia(RestMessage message)
    {
        CobaltDownloadOptions options = optionsManager.Get<CobaltDownloadOptions>();
        IdentityOptions identity = optionsManager.Get<IdentityOptions>();
        if (options.CobaltAPIUrl is null)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"The {identity.BotName} administration has not set up Download Media!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        ReadOnlyCollection<string> foundLinks = TextParsingUtil.FindLinks(message.Content);
        List<string> reverseFixedLinks = [];
        foreach (string link in foundLinks)
            if (TextParsingUtil.RedirectToOriginalService(link) is string reverseFixedLink)
                reverseFixedLinks.Add(reverseFixedLink);
            else
                reverseFixedLinks.Add(link);

        if (reverseFixedLinks.Count <= 0)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("No links found in this message!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        // todo: bad direct usage of httpclient, replace with factory in feature class
        using HttpClient http = new();
        http.DefaultRequestHeaders.Add("Accept", "application/json");
        http.DefaultRequestHeaders.Add("User-Agent", "Project Terrabreak");
        if (options.CobaltAPIKey is not null)
            http.DefaultRequestHeaders.Add("Authorization", $"api-key {options.CobaltAPIKey}");

        Task defer = RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        ConcurrentBag<string> messages = [];
        await Parallel.ForEachAsync(reverseFixedLinks, async (link, cancellationToken) =>
        {
            HttpResponseMessage serializedResponse =
                await http.PostAsJsonAsync(options.CobaltAPIUrl, new CobaltRequest(link), cancellationToken);
            if (await serializedResponse.Content.ReadFromJsonAsync<CobaltResponse>(JsonSerializerOptions.Web,
                    cancellationToken) is CobaltResponse response)
            {
                if (response.Url is not null)
                    messages.Add(response.Url);
                else if (response.Error is not null)
                    messages.Add(response.Error.Code);
                else
                    messages.Add("(parsing error)");
            }
            else
            {
                messages.Add("(request error)");
            }
        });

        _ = await ModifyResponseAsync(response =>
            response
                .WithContent(string.Join('\n', messages))
                .WithFlags(MessageFlags.Ephemeral)
        );
    }
}
