using System.Collections.Generic;
using System.Linq;
using Serilog;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorCore;

public class MountainViews : IRelic<RelicState>
{
    public string Name => "Mountain Views";
    public string Description => "Call my name three times on a sunlit, cloudy afternoon.";
    public IRelicSeries Series => Registries.RelicSeries.Forward["InvestorCore"];
    public string EffectDescription => "Friendly Nature-class monsters have a 25% chance to dodge attacks.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Defense, 0.5),
        new(RelicCategory.NatureMonster, 0.5),
    ];

    public IEnumerable<IBattleDirective>? HookBattleStart(BattleStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        foreach (BattleLoadout player in invocation.GetPlayerEnumerator())
        {
            if (!player.GetRelicEnumerator().Contains(thisRelic))
            {
                continue;
            }
            Log.Debug("Running Mountain Views for {player}.", player.Player.Name);

            IEnumerable<BattleMonster> applicableMonsters = player.GetMonsterEnumerator()
                .Where(monster => monster.Monster.Classification == MonsterClassification.Nature);
            foreach (BattleMonster monster in applicableMonsters)
            {
                double before = monster.State.DodgeChance;
                monster.State.DodgeChance += 0.25;
                Log.Debug("Mountain Views increased {monster}'s dodge chance from {before} to {after}.", monster, before, monster.State.DodgeChance);
            }
        }

        return null;
    }
}
