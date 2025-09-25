namespace Shardion.Terrabreak.Utilities;

public record DictionaryFixEmbedDecision(string Transform) : FixEmbedDecision
{
    public override string Fix(string domain, string url)
    {
        return url.Replace(domain, Transform);
    }
}
