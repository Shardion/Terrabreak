using System.Collections.Frozen;
using System.Collections.Generic;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion;

public static class SdrRegistries
{
    public static Registry<IWeapon> Weapons { get; } = new([
        new WeaponWooden(),
        new WeaponUnion(),
        new WeaponNeo(),
        new WeaponBoxcutter(),
    ]);

    public static Registry<IShield> Shields { get; } = new([
        new ShieldCardboard(),
        new ShieldSoldier(),
        new ShieldHoly(),
        new ShieldMantle()
    ]);

    public static Registry<IHeal> Heals { get; } = new([
        new HealSandwich(),
        new HealPotion(),
        new HealSeltzer(),
        new HealPastry(),
    ]);

    public static Registry<ICure> Cures { get; } = new([
        new CureBread(),
        new CureMegaBread(),
        new CureGigaBread(),
        new CureTeraBread(),
    ]);

    public static Registry<IEnemy> Enemies { get; } = new([
        new EnemyRetRat(),
        new EnemyRazRat(),
        new EnemyBroccoliMan(),
        new EnemyCowDemon(),
        new EnemyMisterBones(),
        new EnemyNanite(),
        new EnemyStationaryMonster(),
        new EnemyLegendaryWarrior(),
        new EnemyPlagueMaster(),
        new EnemyWeregamer(),
        new EnemyLineBreaker(),
        new EnemyBlenderman(),
        new EnemyBrimbeast(),
        new EnemyExtinctionBall(),
        new EnemySilkseeker(),
        new EnemyLifeBloom(),
        new EnemyFormless(),
        new EnemyTheSinger(),
        new EnemyTwinespinner(),
        new EnemyParting(),
        new EnemyBoxGod(),
    ]);

    public static IReadOnlyList<string> RibbonResponses { get; } = new List<string>([
        "What is the square root of a fish?",
        "You only lit three desks on fire. Your performance is lacking.",
        "You are **0%** the high dragun from the video game \"enter the gungeon\" published in april 2016 by dodge roll games.",
        "You are **0%** <:22:773603249633755186>.",
        "Hey, there's not enough bass in here!",
        "(Realite profanity)!",
        "Your name is probably Kevin.",
        "You little Fker You made a shit of piece with your trash fight it's fKing Bad this trash takeover I will become back my channel I hope you will in your next time a cow on a trash farm you sucker",
        "ts pmo icl sybau",
        "You have been banned from free ham sandwich day.",
        "You Have Not Win!",
        "Your chicken is on fire.",
        "You probably can't get past Napstablock.",
        "Bro <:GDNormal:1433218750961946745> Your so Bad!! <:GDHarder:1433218749011464313> at Hexagon <:GDDemon:1433218738877890681> Force <:GDEasy:1433218746637352980>",
        "**LEBRON JAMES** REPORTEDLY **FOUGHT THE LIBERATORS**",
        "[`that sound.flac`](https://anomaly.tail354c3.ts.net/that_sound.flac)",
        "Java is probably in your lightbulb.",
        "https://www.youtube.com/watch?v=MKgaonkIN2E",
        "Geeettttttt pic.twitter.com!!!",
        "Just like in Among Us!",
        "Connection terminated. I'm sorry to interrupt you,",
        "RUN BOXGOD RUN BOXGOD RUN BOXGOD RUN BOXGOD RUN B",
        "Do not takeover here. Not takeover area here.",
        "**Nope!**",
        "missingno",
        "You have seen the white light.",
        "Welcome to tModLoader. You have successfully installed tModLoader.",
    ]).AsReadOnly();

    public static IReadOnlyList<string> Passages { get; } = new List<string>([
        "BOX GOD is an old friend. He can usually be found working on various constructions and machines.",
        "Where did BOX GOD come from? A different side of Real's Domain.",
        "There lives a bird who draws power from the spokes behind their back. Each one represents a specific capability.",
        "\"The Mantle\" is a grand machine made of hexagonal \"mirrors.\" It can grant immense power to its user, but nobody's figured out who's responsible for it, yet.",
        "What does the shopkeeper use all the Credits for? To buy more inventory. What else...?",
        "BOX GOD's combat machines are built with the purpose of finding the Holy Grail. I think he's still looking...",
        "The shopkeeper owns a factory. Maybe that's where they get all the equipment...?",
        "Why is BOX GOD here? Maybe he was just lonely?",
        "Why does the shopkeeper wear that robe? It's a disguise.",
        "BOX GOD is capable of flight, and boasts special plate armor. Don't underestimate him!",
        "If you arrange a bunch of Credits into lines, they'll resonate, and produce some interesting sounds. I believe it's a consequence of how they're made.",
        "BOX GOD is different, you say? Everyone changes.",
        "What's BOX GOD's favorite food? Belt spaghetti. It's a great source of fiber.",
        "Does a machine deserve humanity?",
        "The shopkeeper doesn't work alone.",
        "What's the shopkeeper's favorite food? I don't think they ever told me... cheeseburgers, perhaps?",
        "Legends are told of a sword, forged of light, used by a lone soul to defend their home from countless machines.",
        "Anomaly-class entities create black crystals that are capable of incredible feats.",
        "All of the best restaurants serve pasta.",
        "Never look a Brimbeast in the eyes. Even if you're keeping it as a pet, it could be the last thing you see.",
        "The Singer can use the stars in the sky to tell your future.",
        "<:22:773603249633755186>",
        "Not that many people know the Singer. Consider yourself lucky!",
        "Between you and me, the Singer's real name is Eleanor.",
        "Twinespinner's outlook changes with the seasons.",
        "Nothing compares to the knits and weaves of Twinespinner.",
        "Whenever Twinespinner works, loose threads end up strewn all over the place. It's hard to say if the tripping hazard is intentional.",
        "Parting is a skilled painter.",
        "Beware Parting's hammer. It's said that getting hit by it will change your future... and it really hurts!",
        "Parting's favorite activity is smashing open geodes from the caverns.",
        "Who am I? The antithesis of what is not.",
    ]).AsReadOnly();
}
