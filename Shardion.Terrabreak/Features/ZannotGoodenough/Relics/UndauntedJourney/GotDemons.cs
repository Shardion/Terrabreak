using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.UndauntedJourney;

public class GotDemons : IRelic<RelicState>
{
    public string Name => "Got Demons?";
    public string InternalName => "GotDemons";
    public string Description => "Which ones, you ask? Please refer to the chart.";
    public IRelicSeries Series => Registries.RelicSeries.Forward["UndauntedJourney"];
    public string EffectDescription => "Friendly Spirit-class monsters have a 50% chance to dodge attacks.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.SpiritMonster, 1.0),
        new(RelicCategory.Defense, 0.5),
    ];
}
