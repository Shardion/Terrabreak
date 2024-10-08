using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;
using Discord;
using Discord.Interactions;
using System.Linq;
using System;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using Shardion.Terrabreak.Services.Options;
using System.Net.Http.Json;
using System.Text.Json;
using Serilog;
using System.Collections.Frozen;

namespace Shardion.Terrabreak.Features.CobaltDownload
{
    [CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
    [IntegrationType(ApplicationIntegrationType.UserInstall, ApplicationIntegrationType.GuildInstall)]
    public class CobaltDownloadModule : InteractionModuleBase
    {
        private readonly OptionsManager _optionsManager;
        public CobaltDownloadModule(OptionsManager optionsManager)
        {
            _optionsManager = optionsManager;
        }

        [MessageCommand("Download Media")]
        public async Task DownloadMedia(IMessage message)
        {
            CobaltDownloadOptions options = _optionsManager.Get<CobaltDownloadOptions>();
            if (options.CobaltAPIUrl is null)
            {
                await RespondAsync("Feature disabled, Terrabreak instance owner has not configured a Cobalt API URL", ephemeral: true);
                return;
            }
            ReadOnlyCollection<string> foundLinks = FindLinks(message.Content);

            if (foundLinks.Count <= 0)
            {
                await RespondAsync("No links found in this message!", ephemeral: true);
                return;
            }

            // todo: bad direct usage of httpclient, replace with factory in feature class
            using HttpClient http = new();
            http.DefaultRequestHeaders.Add("Accept", "application/json");
            http.DefaultRequestHeaders.Add("User-Agent", "Project Terrabreak");
            if (options.CobaltAPIKey is not null)
            {
                http.DefaultRequestHeaders.Add("Authorization", $"api-key {options.CobaltAPIKey}");
            }

            await DeferAsync(ephemeral: true);

            ConcurrentBag<string> messages = [];

            await Parallel.ForEachAsync(foundLinks, async (link, cancellationToken) =>
            {
                HttpResponseMessage serializedResponse = await http.PostAsJsonAsync(options.CobaltAPIUrl, new CobaltRequest(link), cancellationToken);
                if (await serializedResponse.Content.ReadFromJsonAsync<CobaltResponse>(JsonSerializerOptions.Web, cancellationToken) is CobaltResponse response)
                {
                    if (response.Url is not null)
                    {
                        messages.Add(response.Url);
                    }
                    else if (response.Error is not null)
                    {
                        messages.Add(response.Error.Code);
                    }
                    else
                    {
                        messages.Add("parsing error");
                    }
                }
                else
                {
                    messages.Add("request error");
                }
            });

            _ = await FollowupAsync(string.Join('\n', messages), ephemeral: true);
        }

        private static ReadOnlyCollection<string> FindLinks(string content)
        {
            List<string> foundLinks = [];
            string[] httpsProtocol = ["h", "t", "t", "p", "s", ":", "/", "/"];

            bool parsingLinkCompletely = false;
            List<string> currentLinkContents = [];
            int currentPositionInProtocol = 0;
            TextElementEnumerator contentElements = StringInfo.GetTextElementEnumerator(content);
            contentElements.Reset(); // unsure if this is needed, but the docs do this
            // horrible manual enumeration because I can't find a GetEnumerator() that returns TEE

            while (contentElements.MoveNext())
            {
                string currentTextElement = contentElements.GetTextElement();

                if (!parsingLinkCompletely)
                {
                    if (currentTextElement.SequenceEqual(httpsProtocol[currentPositionInProtocol]))
                    {
                        currentPositionInProtocol++;
                        if (currentPositionInProtocol >= httpsProtocol.Length)
                        {
                            currentPositionInProtocol = 0;
                            parsingLinkCompletely = true;
                            currentLinkContents.AddRange(httpsProtocol);
                        }
                    }
                    else
                    {
                        currentPositionInProtocol = 0;
                        parsingLinkCompletely = false;
                        currentLinkContents.Clear();
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(currentTextElement))
                    {
                        foundLinks.Add(string.Join(null, currentLinkContents));
                        currentLinkContents.Clear();
                        parsingLinkCompletely = false;
                        currentPositionInProtocol = 0;
                    }
                    else
                    {
                        currentLinkContents.Add(currentTextElement);
                    }
                }
            }

            // reached the end of the message content, do one last check
            // hits messages which only consist of a link
            if (!string.IsNullOrWhiteSpace(currentLinkContents.LastOrDefault()))
            {
                foundLinks.Add(string.Join(null, currentLinkContents));
            }

            // replace fxtwitter, vxtwitter, fixupx, fixvx with twitter
            // yes, this is quadratic
            string[] replaceWithTwitter = ["https://fxtwitter.com/", "https://vxtwitter.com/", "https://fixupx.com/", "https://fixvx.com/"];

            List<string> correctedLinks = [];
            foreach (string link in foundLinks)
            {
                bool added = false;
                foreach (string replaceableSite in replaceWithTwitter)
                {
                    if (link.StartsWith(replaceableSite, true, CultureInfo.InvariantCulture))
                    {
                        correctedLinks.Add(link.Replace(replaceableSite, "https://twitter.com/"));
                        added = true;
                        break;
                    }
                }
                if (!added)
                {
                    correctedLinks.Add(link);
                }
            }

            return correctedLinks.AsReadOnly();
        }
    }
}
