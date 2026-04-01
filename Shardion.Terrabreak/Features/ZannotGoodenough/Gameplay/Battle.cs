using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Player;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;

public class Battle
{
    public required BattleLoadout Player1 { get; init; }
    public required BattleLoadout Player2 { get; init; }
    public PlayerLikesAbilityIntent? Player1Intent { get; set; }
    public PlayerLikesAbilityIntent? Player2Intent { get; set; }
    public PlayerLikesAbilityIntent? Cpu1Intent { get; set; }
    public PlayerLikesAbilityIntent? Cpu2Intent { get; set; }

    public List<string> Lines { get; } = [];

    public BattleResolution? Start()
    {
        BattleResolution? resolution = RunDirectives([
            new BattleStartDirective(
                new(this, null, null),
                new(this, Player1, Player2)
            )
        ]);
        return resolution;
    }

    public BattleResolution? Turn()
    {
        Lines.Clear();
        BattleResolution? resolution = RunDirectives([
            new TurnStartDirective(
                new(this, null, null),
                new(this, Player1, Player2)
            )
        ]);
        if (resolution is not null)
        {
            Log.Debug("Battle ended at the beginning of a turn.");
            return resolution;
        }
        BattleResolution? resolution1 = PlayerTurn(Player1, Player2);
        if (resolution1 is not null)
        {
            Log.Debug("Battle ended after Player 1's turn.");
            return resolution1;
        }
        BattleResolution? resolution2 = PlayerTurn(Player2, Player1);
        if (resolution2 is not null)
        {
            Log.Debug("Battle ended after Player 2's turn.");
            return resolution2;
        }
        return null;
    }

    public BattleResolution? PlayerTurn(BattleLoadout attacker, BattleLoadout defender)
    {
        if (Player1.Player is ComputerPlayer)
        {
            if ((Cpu1Intent is null || BattleRules.CheckKnockout(Cpu1Intent.MonsterUsingAbility)) && Player1.GetMonsterEnumerator().Shuffle().FirstOrDefault() is BattleMonster randomMonster)
            {
                Log.Debug("CPU #1 intent set to {intent}.", randomMonster.Monster.Name);
                Cpu1Intent = new(randomMonster);
            }
        }
        if (Player2.Player is ComputerPlayer)
        {
            if ((Cpu2Intent is null || BattleRules.CheckKnockout(Cpu2Intent.MonsterUsingAbility)) && Player2.GetMonsterEnumerator().Shuffle().FirstOrDefault() is BattleMonster randomMonster)
            {
                Log.Debug("CPU #2 intent set to {intent}.", randomMonster.Monster.Name);
                Cpu2Intent = new(randomMonster);
            }
        }

        foreach (BattleMonster attackingMonster in attacker.GetMonsterEnumerator())
        {
            if (BattleRules.CheckKnockout(attackingMonster))
            {
                continue;
            }

            if (Player1Intent is PlayerLikesAbilityIntent player1Intent && player1Intent.MonsterUsingAbility == attackingMonster)
            {
                Log.Debug("Player #1 intent received to use {ability}.", attackingMonster.Monster.LikesAbility.Name);
                Player1Intent = null;
                if (UseLikesAbility(attacker, defender, attackingMonster) is BattleResolution resolution)
                {
                    return resolution;
                }
            }
            if (Player2Intent is PlayerLikesAbilityIntent player2Intent && player2Intent.MonsterUsingAbility == attackingMonster)
            {
                Log.Debug("Player #2 intent received to use {ability}.", attackingMonster.Monster.LikesAbility.Name);
                Player2Intent = null;
                if (UseLikesAbility(attacker, defender, attackingMonster) is BattleResolution resolution)
                {
                    return resolution;
                }
            }
            if (Cpu1Intent is PlayerLikesAbilityIntent cpu1Intent && cpu1Intent.MonsterUsingAbility == attackingMonster)
            {
                bool coinFlipIfTwoMonsters = true;
                if (Player1.GetMonsterEnumerator().Count() < 3)
                {
                    coinFlipIfTwoMonsters = Random.Shared.Next(2) == 1;
                }
                if (coinFlipIfTwoMonsters && attackingMonster.Monster.LikesAbility.BaseLikesCost <= Player1.Likes)
                {
                    Log.Error("CPU #1 intent received to use {ability}.", attackingMonster.Monster.LikesAbility.Name);
                    Cpu1Intent = null;
                    if (UseLikesAbility(attacker, defender, attackingMonster) is BattleResolution resolution)
                    {
                        return resolution;
                    }
                }
            }
            if (Cpu2Intent is PlayerLikesAbilityIntent cpu2Intent && cpu2Intent.MonsterUsingAbility == attackingMonster)
            {
                bool coinFlipIfTwoMonsters = true;
                if (Player2.GetMonsterEnumerator().Count() < 3)
                {
                    coinFlipIfTwoMonsters = Random.Shared.Next(2) == 1;
                }
                if (coinFlipIfTwoMonsters && attackingMonster.Monster.LikesAbility.BaseLikesCost <= Player2.Likes)
                {
                    Log.Debug("CPU #2 intent received to use {ability}.", attackingMonster.Monster.LikesAbility.Name);
                    Cpu2Intent = null;
                    if (UseLikesAbility(attacker, defender, attackingMonster) is BattleResolution resolution)
                    {
                        return resolution;
                    }
                }
            }

            TargetSelectionRule targetSelectionRule = new(
                new(this, null, attackingMonster),
                new(this, attacker, defender, attackingMonster));
            TargetSelectionResult targetSelectionResult = RunRules(targetSelectionRule);
            if (targetSelectionResult.Target is not BattleMonster target)
            {
                continue;
            }
            Log.Debug("Basic attack target for {attacker} set to {defender}.", attackingMonster.Monster.Name, target.Monster.Name);

            BasicAttackDirective basicAttackDirective = new(
                new(this, null, attackingMonster),
                new(this, attacker, defender, attackingMonster, target)
            );

            if (RunDirectives([basicAttackDirective]) is BattleResolution directiveResolution)
            {
                return directiveResolution;
            }
        }

        return CheckSignals();
    }

    private BattleResolution? RunDirectives(IEnumerable<IBattleDirective> directives)
    {
        foreach (IBattleDirective directive in directives)
        {
            List<IBattleDirective> chainedDirectives = [];
            IBattleDirective pendingDirective = directive;
            if (directive.FireHooks() is IEnumerable<IBattleDirective> hookDirectives)
            {
                Log.Debug("Adding chained directives for `{directive}`.", pendingDirective.GetType().Name);
                chainedDirectives.AddRange(hookDirectives);
            }

            if (directive.FireInterceptors() is IEnumerable<IBattleDirective> interceptorDirectives)
            {
                // TODO: Any form of conflict resolution...?
                if (interceptorDirectives.LastOrDefault() is IBattleDirective lastInterceptorDirective)
                {
                    Log.Debug("Intercepting directive `{directive}`.", pendingDirective.GetType().Name);
                    pendingDirective = lastInterceptorDirective;
                }
            }

            Log.Debug("Executing directive `{directive}`.", pendingDirective.GetType().Name);
            if (pendingDirective.Execute() is IEnumerable<IBattleDirective> postExecutionDirectives)
            {
                chainedDirectives.AddRange(postExecutionDirectives);
            }

            if (pendingDirective.LogLine is not null)
            {
                Log.Debug("Added line `{line}`.", pendingDirective.LogLine);
                Lines.Add(pendingDirective.LogLine);
            }

            if (CheckSignals() is BattleResolution resolution)
            {
                Log.Debug("Battle ended after directive resolution.");
                return resolution;
            }
            RunDirectives(chainedDirectives);
        }

        return null;
    }

    public static TRuleResult RunRules<TRuleResult, TInvocation>(IBattleRule<TRuleResult, TInvocation> rule) where TRuleResult : IRuleResult
    {
        IBattleRule<TRuleResult, TInvocation> pendingRule = rule;
        if (rule.FireHooks() is IEnumerable<object> hookInvocations)
        {
            foreach (object hookInvocation in hookInvocations)
            {
                Log.Debug("Modifying invocation for `{rule}`.", pendingRule.GetType().Name);
                pendingRule = pendingRule.Modify(hookInvocation);
            }
        }
        if (rule.FireInterceptors() is IEnumerable<TRuleResult> interceptorRules)
        {
            // TODO: Any form of conflict resolution...?
            if (interceptorRules.LastOrDefault() is TRuleResult lastInterceptorRule)
            {
                Log.Debug("Intercepting result for `{rule}`.", pendingRule.GetType().Name);
                return lastInterceptorRule;
            }
        }

        Log.Debug("Executing rule `{rule}`.", pendingRule.GetType().Name);
        return pendingRule.Execute();
    }

    private BattleResolution? UseLikesAbility(BattleLoadout attacker, BattleLoadout defender, BattleMonster attackingMonster)
    {
        LikesCostResult result = Battle.RunRules(new LikesCostRule(
            new(this, null, attackingMonster),
            new(
                this,
                attacker,
                defender,
                attackingMonster,
                attackingMonster.State,
                attackingMonster.Monster.LikesAbility.BaseLikesCost,
                attackingMonster.State.LikesCostStaticModifier,
                attackingMonster.State.LikesCostPercentageModifier
            )));
        // Exploit prevention. Likes abilities already cannot be selected if they cannot be paid for
        if (result.FinalLikesCost <= attacker.Likes)
        {
            LikesAbilityDirective likesAbilityDirective = new(
                new(this, null, attackingMonster),
                new(this, attacker, defender, attackingMonster, attackingMonster.State)
            );
            if (RunDirectives([likesAbilityDirective]) is BattleResolution likesResolution)
            {
                return likesResolution;
            }
        }

        return null;
    }

    private BattleResolution? CheckSignals()
    {
        if (BattleRules.CheckPlayerLoss(Player1))
        {
            return new(Player2);
        }
        if (BattleRules.CheckPlayerLoss(Player2))
        {
            return new(Player1);
        }

        return null;
    }
}
