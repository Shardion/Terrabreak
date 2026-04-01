using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.UndauntedJourney;

public class IntensiveNuggetCare : IRelic<RelicState>
{
    public string Name => "Intensive Nugget Care";
    public string Description => "Fail!";
    public IRelicSeries Series => Registries.RelicSeries.Forward["UndauntedJourney"];
    public string EffectDescription =>
        "Healing applied to all Machina-class monsters in play is instead inflicted as damage.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Attack, 1.0),
    ];
}
