using System;

namespace Shardion.Terrabreak.Utilities;

public abstract record FixEmbedDecision
{
    public abstract string Fix(string domain, string url);
}
