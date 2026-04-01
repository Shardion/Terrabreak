using System.Collections.Generic;
using Serilog;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public record LikesAbilityDirective(DirectiveSource Source, LikesAbilityInvocation<MonsterState> Invocation) : IBattleDirective
{
    public string? LogLine { get; private set; }

    public IEnumerable<IBattleDirective>? FireHooks()
    {
        return [];
    }

    public IEnumerable<IBattleDirective>? FireInterceptors()
    {
        List<IBattleDirective> directives = [];
        foreach (BattleRelic friendlyRelic in Invocation.FriendlyPlayer.GetRelicEnumerator())
        {
            if (friendlyRelic.Relic.InterceptLikesAbility(Invocation, friendlyRelic, friendlyRelic.Relic.CastState(friendlyRelic.State)) is IEnumerable<IBattleDirective> hookDirectives)
            {
                directives.AddRange(hookDirectives);
            }
        }
        foreach (BattleRelic enemyRelic in Invocation.EnemyPlayer.GetRelicEnumerator())
        {
            if (enemyRelic.Relic.InterceptLikesAbility(Invocation, enemyRelic, enemyRelic.Relic.CastState(enemyRelic.State)) is IEnumerable<IBattleDirective> hookDirectives)
            {
                directives.AddRange(hookDirectives);
            }
        }
        return directives;
    }

    public IEnumerable<IBattleDirective>? Execute()
    {
        LikesCostResult result = Battle.RunRules(new LikesCostRule(
            Source,
            new(
                Invocation.Battlefield,
                Invocation.FriendlyPlayer,
                Invocation.EnemyPlayer,
                Invocation.User,
                Invocation.UserState,
                Invocation.User.Monster.LikesAbility.BaseLikesCost,
                Invocation.User.State.LikesCostStaticModifier,
                Invocation.User.State.LikesCostPercentageModifier
            )));
        if (Invocation.FriendlyPlayer.Likes < result.FinalLikesCost)
        {
            Log.Debug(
                "{player} could not pay for {monster}'s Likes ability, costing {cost} Likes, but only holding {held} Likes!",
                Invocation.FriendlyPlayer.Player.Name, Invocation.User.Monster.Name, result.FinalLikesCost, Invocation.FriendlyPlayer.Likes);
            return null;
        }
        Log.Debug(
            "{player} paid {cost} Likes for {monster}'s Likes ability.",
            Invocation.FriendlyPlayer.Player.Name, result.FinalLikesCost, Invocation.User.Monster.Name);
        Invocation.FriendlyPlayer.Likes -= result.FinalLikesCost;
        LogLine = $"{Invocation.FriendlyPlayer.Player.Name}'s {Invocation.User.Monster.Name} uses {Invocation.User.Monster.LikesAbility.Name}!";
        return Invocation.User.Monster.LikesAbility.ExecuteCastingState(Invocation);
    }
}
