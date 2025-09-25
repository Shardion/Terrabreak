using Flurl;

namespace Shardion.Terrabreak.Utilities;

public record ShortenYoutubeFixEmbedDecision : FixEmbedDecision
{
    public override string Fix(string domain, string url)
    {
        Url parsedUrl = Url.Parse(url);
        if (parsedUrl.QueryParams.TryGetFirst("v", out object value) && value is string videoId)
        {
            parsedUrl.Host = "youtu.be";
            parsedUrl.Path = videoId;
            parsedUrl.QueryParams.Clear();
        }

        return parsedUrl;
    }
}
