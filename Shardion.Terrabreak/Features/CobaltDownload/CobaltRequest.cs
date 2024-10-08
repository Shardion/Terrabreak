namespace Shardion.Terrabreak.Features.CobaltDownload
{
    public sealed class CobaltRequest
    {
        public CobaltRequest(string url)
        {
            Url = url;
        }

        public string Url { get; init; }
    }
}
