using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.UndauntedJourney;

public class LinebreakerBladeReplica : IRelic<RelicState>
{
    public string Name => "Linebreaker Blade Replica";

    public string Description =>
        "A mere replica of the legendary weapon, this object lacks the capabilities of the original, but inspires all the same confidence.";
    public IRelicSeries Series => Registries.RelicSeries.Forward["UndauntedJourney"];
    public string EffectDescription => "Friendly Piercing-character monsters have +2 DEF.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Defense, 0.5),
        new(RelicCategory.PiercingCharacter, 0.5),
    ];
}
