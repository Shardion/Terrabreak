using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;

public class SdrChannelTplComparer : Comparer<SdrChannel>
{
    public override int Compare(SdrChannel? x, SdrChannel? y)
    {
        if (x is not null && y is not null)
        {
            return x.Captor.TargetTotalPowerLevel.CompareTo(y.Captor.TargetTotalPowerLevel);
        }

        if (x is null && y is not null)
        {
            return -1;
        }

        if (y is null && x is not null)
        {
            return 1;
        }

        return 0;
    }
}
