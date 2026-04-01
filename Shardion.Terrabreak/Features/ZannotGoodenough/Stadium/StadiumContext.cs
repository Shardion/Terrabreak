using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Rodent;
using Shardion.Terrabreak.Features.ZannotGoodenough.Player;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Stadium;

public record StadiumContext(int TotalRounds, int CurrentRound)
{
    private static readonly HashSet<string> Round1AllowedMonsters = [
        "RetRat",
        "RazRat",
        "BroccoliMan",
    ];

    private static readonly HashSet<string> AfterRound4Monsters = [
        "LineBreaker",
        "Silkseeker",
        "Malwind",
        "ZShambler",
        "SmelterGolem",
        "PlagueMaster",
    ];

    private static readonly HashSet<string> BeforeRound6Monsters = [
        "RetRat",
        "RazRat",
        "CowDemon",
        "MisterBones",
    ];

    public BattleLoadout GenerateOpponent()
    {
        int points;
        double strategy;
        if (CurrentRound >= 9)
        {
            // Actually round 10...
            points = 4000;
            strategy = 1.0;
        }
        else
        {
            // +125 points per round
            points = 125 * (CurrentRound + 1);
            strategy = 0.1 * (CurrentRound + 1);
        }

        string name = CurrentRound >= 9
            ? "President Zannot"
            : Registries.StadiumContestantNames.Shuffle().First();
        BattleLoadout opponent = new()
        {
            Player = new ComputerPlayer
            {
                Name = name,
            },
        };

        opponent.Monster1 = new(SelectMonster(opponent));
        opponent.Monster2 = new(SelectMonster(opponent));
        if (CurrentRound > 1)
        {
            opponent.Monster3 = new(SelectMonster(opponent));
        }

        // Relics are selected in reverse order, so the most powerful ones take effect last
        if (SelectRelic(opponent, points, strategy) is IRelic<RelicState> relic4)
        {
            opponent.Relic4 = new(relic4);
            points -= relic4.Series.StadiumPointsCost;
        }
        if (SelectRelic(opponent, points, strategy) is IRelic<RelicState> relic3)
        {
            opponent.Relic3 = new(relic3);
            points -= relic3.Series.StadiumPointsCost;
        }
        if (SelectRelic(opponent, points, strategy) is IRelic<RelicState> relic2)
        {
            opponent.Relic2 = new(relic2);
            points -= relic2.Series.StadiumPointsCost;
        }
        if (SelectRelic(opponent, points, strategy) is IRelic<RelicState> relic1)
        {
            opponent.Relic1 = new(relic1);
        }

        Log.Debug("Final loadout: {relic1}, {relic2}, {relic3}, {relic4}.", opponent.Relic1?.Relic.Name,opponent.Relic2?.Relic.Name,opponent.Relic3?.Relic.Name,opponent.Relic4?.Relic.Name);

        return opponent;
    }

    private IMonster<MonsterState> SelectMonster(BattleLoadout opponent)
    {
        if (CurrentRound == 0)
        {
            // Special set of monsters for round 1 only
            return Round1AllowedMonsters.Select(id => Registries.Monsters.Forward[id]).Shuffle().First();
        }

        IEnumerable<IMonster<MonsterState>> validMonsters = Registries.Monsters.Contents
            .Where(monster => !monster.Hidden);
        if (CurrentRound < 4)
        {
            // Ban some monsters if we are on round 4 or lower
            validMonsters = validMonsters
                .Where(monster => !AfterRound4Monsters.Contains(monster.InternalName));
        }
        if (CurrentRound >= 6)
        {
            // Ban some monsters if we are on round 6 or higher
            validMonsters = validMonsters
                .Where(monster => !BeforeRound6Monsters.Contains(monster.InternalName));

            if (opponent.GetMonsterEnumerator().Any(monster => monster.Monster.InternalName == "BroccoliMan"))
            {
                // Don't use Broccoli Man if it's already in the loadout
                validMonsters = validMonsters
                    .Where(monster => monster.InternalName != "BroccoliMan");
            }
        }

        return validMonsters.Shuffle().First();
    }

    private IRelic<RelicState>? SelectRelic(BattleLoadout loadout, int points, double strategy)
    {
        if (points < 100)
        {
            // Fail fast if we are out of points (the cheapest relics cost 100)
            Log.Debug("Out of points.");
            return null;
        }

        List<string> existingRelics = [];
        HashSet<RelicCategory> existingConflicts = [];
        foreach (BattleRelic existingRelic in loadout.GetRelicEnumerator())
        {
            existingRelics.Add(existingRelic.Relic.InternalName);
            foreach (RelicCategory existingRelicConflict in existingRelic.Relic.Conflicts)
            {
                existingConflicts.Add(existingRelicConflict);
            }
        }

        List<IRelic<RelicState>> allUsableRelics = Registries.Relics.Contents
            .Where(relic => relic.Series.StadiumPointsCost <= points
                            && !existingRelics.Contains(relic.InternalName)
                            && !existingConflicts.Any(conflict => relic.Conflicts.Contains(conflict))
            )
            .ToList();

        Dictionary<RelicCategory, double> domain = [];
        // Pre-load domain with some prominences for each monster in play
        foreach (BattleMonster monster in loadout.GetMonsterEnumerator())
        {
            RelicCategory classificationCategory = monster.Monster.Classification switch
            {
                MonsterClassification.Rodent => RelicCategory.RodentMonster,
                MonsterClassification.Nature => RelicCategory.NatureMonster,
                MonsterClassification.Machina => RelicCategory.MachinaMonster,
                MonsterClassification.Spirit => RelicCategory.SpiritMonster,
                _ => throw new ArgumentOutOfRangeException()
            };
            if (domain.TryGetValue(classificationCategory, out double monsterClassificationCategoryProminence))
            {
                domain[classificationCategory] = monsterClassificationCategoryProminence + 1.0;
            }
            else
            {
                domain[classificationCategory] = 1.0;
            }

            RelicCategory characteristicCategory = monster.Monster.Characteristic switch
            {
                MonsterCharacteristic.Blunt => RelicCategory.BluntCharacter,
                MonsterCharacteristic.Piercing => RelicCategory.PiercingCharacter,
                MonsterCharacteristic.Blasting => RelicCategory.BlastingCharacter,
                MonsterCharacteristic.Haunting => RelicCategory.HauntingCharacter,
                _ => throw new ArgumentOutOfRangeException()
            };
            if (domain.TryGetValue(characteristicCategory, out double monsterCharacteristicCategoryProminence))
            {
                domain[characteristicCategory] = monsterCharacteristicCategoryProminence + 1.0;
            }
            else
            {
                domain[characteristicCategory] = 1.0;
            }
        }

        // If all non-monster categories are empty, nudge to a random one
        List<RelicCategory> randomCategories = [RelicCategory.Attack, RelicCategory.Defense, RelicCategory.Likes];
        RelicCategory? maybeRandomCategory = randomCategories
            .Where(category => !domain.ContainsKey(category))
            .Shuffle()
            .FirstOrDefault();
        if (maybeRandomCategory is RelicCategory randomCategory)
        {
            if (domain.TryGetValue(randomCategory, out double randomCategoryProminence))
            {
                domain[randomCategory] = randomCategoryProminence + 1.0;
            }
            else
            {
                domain[randomCategory] = 1.0;
            }
        }

        foreach (BattleRelic relic in loadout.GetRelicEnumerator())
        {
            foreach (RelicDomainPart relicDomainPart in relic.Relic.Domain)
            {
                if (domain.TryGetValue(relicDomainPart.Category, out double currentProminence))
                {
                    domain[relicDomainPart.Category] = currentProminence + relicDomainPart.Prominence;
                }
                else
                {
                    domain[relicDomainPart.Category] = relicDomainPart.Prominence;
                }
            }
        }

        double totalDomainProminence = domain.Sum(pair => pair.Value);
        if (totalDomainProminence <= 0.0)
        {
            Log.Debug("No domain prominence, skipping.");
            return allUsableRelics.Shuffle().FirstOrDefault();
        }

        StringBuilder dimensionsBuilder = new("Dimensions:\n");
        foreach (KeyValuePair<RelicCategory, double> dimension in domain)
        {
            dimensionsBuilder.AppendLine($"- {dimension.Key.ToString()}@{dimension.Value}");
        }
        Log.Debug("{dim}", dimensionsBuilder.ToString());

        List<StadiumRelic> choices = [];
        foreach (IRelic<RelicState> relic in allUsableRelics)
        {
            int matchingCategories = 0;
            double totalMatchingProminence = 0.0;
            foreach (RelicDomainPart relicDomainPart in relic.Domain)
            {
                if (domain.TryGetValue(relicDomainPart.Category, out double existingValue) && existingValue > 0.0)
                {
                    matchingCategories += 1;
                    totalMatchingProminence += relicDomainPart.Prominence;
                }
            }
            choices.Add(new(relic,
                Math.Clamp(
                    (totalMatchingProminence / totalDomainProminence)
                    * (matchingCategories * 0.5)
                    * strategy,
                    0.0, 0.80)));
        }

        double rng = Random.Shared.NextDouble();
        List<StadiumRelic> validStrategicChoices = choices
            .Where(choice => choice.StrategicValue > 0.0)
            .OrderBy(choice => choice.StrategicValue)
            .ToList();
        StadiumRelic? lowestChoice = choices.MinBy(choice => choice.StrategicValue);
        StadiumRelic? highestChoice = choices.MaxBy(choice => choice.StrategicValue);
        Log.Debug("RNG: {rng}, strategy input: {strategy}, lowest strategic value: {lowestname} with {lowestvalue}, highest strategic value: {highestname} with {highestvalue}.",
            rng,
            strategy,
            lowestChoice?.Relic.Name,
            lowestChoice?.StrategicValue,
            highestChoice?.Relic.Name,
            highestChoice?.StrategicValue
        );
        if (validStrategicChoices.Count > 0 && rng <= strategy)
        {
            StadiumRelic? bestRelic = validStrategicChoices.MaxBy(relic => relic.StrategicValue);
            Log.Debug("Selecting {RelicName}.", bestRelic?.Relic.Name ?? validStrategicChoices.First().Relic.Name);
            return bestRelic?.Relic ?? validStrategicChoices.First().Relic;
        }

        // No strategic choices, give up and throw a random relic
        Log.Debug("No strategic relics or the RNG did not allow for one, throwing a random relic.");
        return allUsableRelics.Shuffle().FirstOrDefault();
    }
}
