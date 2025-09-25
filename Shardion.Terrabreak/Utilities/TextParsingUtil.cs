using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Flurl;
using NetCord;

namespace Shardion.Terrabreak.Utilities;

public class TextParsingUtil
{
    public static FrozenDictionary<string, FixEmbedDecision> ForwardFixEmbedDictionary { get; } =
        new Dictionary<string, FixEmbedDecision>
        {
            ["twitter.com"] = new DictionaryFixEmbedDecision("vxtwitter.com"),
            ["x.com"] = new DictionaryFixEmbedDecision("vxtwitter.com"),
            // xcancel's embeds are technically broken as well, as they lack video support
            ["xcancel.com"] = new DictionaryFixEmbedDecision("vxtwitter.com"),
            ["pixiv.net"] = new DictionaryFixEmbedDecision("phixiv.net"),
            ["www.bilibili.com"] = new DictionaryFixEmbedDecision("www.vxbilibili.com"),
            ["b23.tv"] = new DictionaryFixEmbedDecision("vxb23.tv"),
            ["reddit.com"] = new DictionaryFixEmbedDecision("rxddit.com"),
            ["old.reddit.com"] = new DictionaryFixEmbedDecision("old.rxddit.com"),
            ["www.reddit.com"] = new DictionaryFixEmbedDecision("www.rxddit.com"),
            ["www.tiktok.com"] = new DictionaryFixEmbedDecision("www.tnktok.com"),
            ["www.facebook.com"] = new DictionaryFixEmbedDecision("www.facebed.com"),
            ["www.youtube.com"] = new ShortenYoutubeFixEmbedDecision()
        }.ToFrozenDictionary();

    public static FrozenDictionary<string, string> ReverseFixEmbedDictionary { get; } = new Dictionary<string, string>
    {
        ["vxtwitter.com"] = "twitter.com",
        ["fxtwitter.com"] = "twitter.com",
        ["fixupx.com"] = "twitter.com",
        ["fixvx.com"] = "twitter.com",
        ["girlcockx.com"] = "twitter.com",
        ["stupidpenisx.com"] = "twitter.com",
        ["xcancel.com"] = "twitter.com",
        ["x.com"] = "twitter.com",
        ["phixiv.net"] = "pixiv.net",
        ["www.vxbilibili.com"] = "www.bilibili.com",
        ["vxb23.tv"] = "b23.tv",
        ["www.tnktok.com"] = "www.tiktok.com",
        // Unlikely that people will discover mode combining naturally, and nobody
        // goes to the fxtiktok github page, so this should be OK for now
        ["a.tnktok.com"] = "www.tiktok.com",
        ["d.tnktok.com"] = "www.tiktok.com",
        ["hq.tnktok.com"] = "www.tiktok.com",
        ["rxddit.com"] = "reddit.com",
        ["www.rxddit.com"] = "www.reddit.com",
        ["old.rxddit.com"] = "old.reddit.com",
        ["facebed.com"] = "www.facebook.com",
        ["www.facebed.com"] = "www.facebed.com"
    }.ToFrozenDictionary();

    public static string? RedirectToFixerService(string url)
    {
        Url parsedUrl = Url.Parse(url);
        if (ForwardFixEmbedDictionary.TryGetValue(parsedUrl.Authority, out FixEmbedDecision? decision))
            return decision.Fix(parsedUrl.Authority, url);
        return null;
    }

    public static string? RedirectToOriginalService(string url)
    {
        Url parsedUrl = Url.Parse(url);
        return ReverseFixEmbedDictionary.GetValueOrDefault(parsedUrl.Authority);
    }

    public static ReadOnlyCollection<string> FindLinks(string content)
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
            foundLinks.Add(string.Join(null, currentLinkContents));

        return foundLinks.AsReadOnly();
    }
}
