using System.Collections.Generic;
using System.Linq;
using Serilog;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.PresidentZannotFinest;

public class AllTheRules : IRelic<RelicState>
{
    public string Name => "All The Rules";
    public string Description =>
        "Let's see what kind of trouble we can get ourselves into...";
    public IRelicSeries Series => Registries.RelicSeries.Forward["PresidentZannotFinest"];
    public string EffectDescription =>
        "Friendly Rodent-class monsters and Blunt-character monsters have a 25% chance to dodge attacks, and +1 DEF.";

    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.RodentMonster, 0.33),
        new(RelicCategory.BluntCharacter, 0.33),
        new(RelicCategory.Defense, 0.33),
    ];

    public IEnumerable<IBattleDirective>? HookBattleStart(BattleStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        foreach (BattleLoadout player in invocation.GetPlayerEnumerator())
        {
            if (!player.GetRelicEnumerator().Contains(thisRelic))
            {
                continue;
            }
            Log.Debug("Running All The Rules for {player}.", player.Player.Name);
            IEnumerable<BattleMonster> applicableMonsters = player.GetMonsterEnumerator()
                .Where(monster => monster.Monster.Characteristic == MonsterCharacteristic.Blunt ||
                                  monster.Monster.Classification == MonsterClassification.Rodent);
            foreach (BattleMonster monster in applicableMonsters)
            {
                double before = monster.State.DodgeChance;
                monster.State.DodgeChance += 0.25;
                monster.State.DefenseStaticModifier += 1;
                Log.Debug("All The Rules increased {monster}'s dodge chance from {before} to {after}.", monster, before, monster.State.DodgeChance);
            }
        }

        return null;
    }
}
