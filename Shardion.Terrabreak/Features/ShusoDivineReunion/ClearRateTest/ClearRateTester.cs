using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.ClearRateTest;

public class ClearRateTester
{
    private readonly HashSet<BattleEngine> _battleEngines;
    public bool UseHeal { get; set; }

    public ClearRateTester(IEnemy enemy, int playerNum, IPlayer playerBase, int battles, bool useHeal)
    {
        UseHeal = useHeal;
        _battleEngines = [];
        IPlayer[] players = playerNum switch
        {
            > 3 => [playerBase, ClonePlayer(playerBase), ClonePlayer(playerBase), ClonePlayer(playerBase)],
            > 2 => [playerBase, ClonePlayer(playerBase), ClonePlayer(playerBase)],
            > 1 => [playerBase, ClonePlayer(playerBase)],
            _ => [playerBase]
        };
        for (int battle = 0; battle < battles; battle++)
        {
            _battleEngines.Add(new BattleEngine(enemy, players));
        }
    }

    private IPlayer ClonePlayer(IPlayer player)
    {
        return new HelperPlayer
        {
            Name = player.Name,
            Credits = player.Credits,
            Weapon = player.Weapon,
            Shield = player.Shield,
            Heal = player.Heal,
            Cure = player.Cure,
        };
    }

    public async Task<IReadOnlyCollection<BattleResult>> RunTestAsync(int maxIterations = 100)
    {
        return await Task.Run(() => RunTest(maxIterations));
    }

    public IReadOnlyCollection<BattleResult> RunTest(int maxIterations = 100)
    {
        List<BattleResult> results = [];
        foreach (BattleEngine engine in _battleEngines)
        {
            engine.Start();
        }
        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            foreach (BattleEngine engine in _battleEngines)
            {
                if (engine.Turn() is BattleResult result)
                {
                    results.Add(result);
                    _battleEngines.Remove(engine);
                }
            }
        }

        return results;
    }
}
