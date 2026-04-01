using System.Collections.Frozen;
using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Machina;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Nature;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Rodent;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Spirit;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics.GeraExperimental;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorCore;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorUltra;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics.PresidentZannotFinest;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics.TricksOfTheTrade;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics.UndauntedJourney;

namespace Shardion.Terrabreak.Features.ZannotGoodenough;

public static class Registries
{
    public static Registry<IRelic<RelicState>> Relics { get; } = new([
        new Placebo(),
        new GamerFood(),
        new ShadowWizardMoney(),
        new FocusHarder(),
        new WeavingThread(),
        new CelestialDust(),
        new BrilliantGeode(),
        new OverpoweringBlizzard(),
        new AllTheRules(),
        new MountainViews(),
        new TheArtSyndicate(),
        new TheGarbageSortingSquad(),
        new TheSaxophoners(),
        new TheZombies(),
        new BroccoliToken(),
        new StopTheRangers(),
        new CutthroatRanks(),
        new BlurayBlastingBomb(),
        new GuardianPresence(),
        new LetsGoToTheMoon(),
        new GotDemons(),
        new SpokenVeil(),
        new IntensiveNuggetCare(),
        new LinebreakerBladeReplica(),
        new MissingChildIncident(),
        new LineBreakerRotaryCannonModule(),
        new RocksCollection(),
        new SuperNitrogen(),
        new SowTheWind(),
        new BlessedRelic(),
        new LawsOfReality(),
        new OoPartFinest(),
        new AParticularFlavourOfDerangement(),
    ]);

    public static Registry<IRelicSeries> RelicSeries { get; } = new([
        new InvestorCore(),
        new PresidentZannotFinest(),
        new UndauntedJourney(),
        new InvestorUltra(),
        new GeraExperimental(),
        new TricksOfTheTrade(),
    ]);

    public static Registry<IMonster<MonsterState>> Monsters { get; } = new([
        new BlueEyesWhiteDragon(),
        new RetRat(),
        new RazRat(),
        new BroccoliMan(),
        new CowDemon(),
        new MisterBones(),
        new Nanite(),
        new LineBreaker(),
        new PlagueMaster(),
        new Staua(),
        new StationaryMonster(),
    ]);

    public static FrozenSet<string> BattleStartQuotes { get; } = new List<string>([
        "Let's make some noise!",
        "Tell me, Doctor Zannot",
        "For great justice!",
        "Let's learn the basics of investing",
        "Zan plays everything in our sights",
        "A party for monsters",
        "Investing in progress...",
        "We bring the sound that shakes the ground",
        "Are you ready?",
        "You'll never shut it out",
        "Long live the investment king",
        "Let's light it up",
        "Let's go investing!!",
        "The architect and the builder",
        "アタリハズレ", // "atari hazure", or "gambling"
        "A simple numbers game",
        "Using Project Shapeshifter algorithmic technology",
        "It's all or nothing",
    ]).ToFrozenSet();

    public static FrozenSet<string> StadiumContestantNames { get; } = new List<string>([
        "シュソ", // "shuso"
        "'Twinespinner'",
        "'The Singer'",
        "'Parting'",
        "'René'",
        "Wood Man",
        "Zarry",
        "The Biblioclast",
        "Contrail",
        "Snow Roomba",
        "Plantterror",
        "DN",
        "Damong Nus",
        "Richard Nixon",
        "Uni (cat)",
        "Uni (cat?)",
        "The Brick",
        "The Horse",
        "BOX GOD",
        "ICE GOD",
        "PIG GOD",
        "Cerveca Cristal Marketer",
        "Discord User ffmpeg",
        "Kasane Teto",
        "Sponge",
        "Polterghast",
        "C22",
        "O22",
        "Link fixer bot EX",
        "The Giant Rat",
        "Kobar",
        "Glomprus", // Suggested by Geckronome
        "Hatixhe", // Suggested by Gabe\\\
        "Thunderclast", // Suggested by Optimisto
        "Kiryu", // Suggested by zan
        "Majima", // Suggested by zan
        "The Blue One", // Suggested by zan
        "Obyn Greenfoot", // Suggested by Planterror
        "Amelia the Amazing", // Suggested by Planterror
        "Patch", // Suggested by Planterror
        "Ocram", // Suggested by Planterror
        "Redigit", // Suggested by TechTato
        "Xenmas", // Suggested by iris
    ]).ToFrozenSet();
}
