using System;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion;

public static class PowerLevel
{
    public static int Average(Tier weapon, Tier shield, Tier healing, Tier cure)
    {
        return ((int)weapon * 100 + (int)shield * 100 + (int)healing * 100 + (int)cure * 100) / 4;
    }

    public static int Average(IPlayer player)
    {
        int weaponTier = (int)player.Weapon.Tier * 100;
        int shieldTier = (int)player.Shield.Tier * 100;
        int healTier = (int)(player.Heal?.Tier ?? Tier.Zero) * 100;
        int cureTier = (int)(player.Cure?.Tier ?? Tier.Zero) * 100;
        return weaponTier + shieldTier + healTier + cureTier / 4;
    }

    public static int Compare(IPlayer player, IEnemy enemy)
    {
        int playerPowerLevel = Average(player);
        int enemyPowerLevel = Convert.ToInt32(enemy.TargetTotalPowerLevel * 100);
        return playerPowerLevel.CompareTo(enemyPowerLevel);
    }
}

public enum Tier
{
    Zero =  0,
    One =   1,
    Two =   2,
    Three = 3,
    Four =  4,
}
