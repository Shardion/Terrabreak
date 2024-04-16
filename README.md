# Project Terrabreak

A heavily-reorganized port of Project Achromatic to Discord.NET.
Often called simply "Terrabreak".

## Structure

The foundation of Terrabreak is its services; these implement `ITerrabreakService` and live
inside `Shardion.Terrabreak/Services/`.

These services implement core functionality of the bot, like providing its branding to features,
supporting a database, and providing access to a Discord client.

The structure placed on top of the metaphorical service foundation is features; that is, anything in
`Shardion.Terrabreak/Features/`. Services ultimately exist to support features, and features are what
is visible to the end users.

Features come in many shapes and forms, but they all interface with Discord in some way, typically
by adding application commands or handling gateway events.

The third element of Terrabreak, options, comprise its configuration system.
There are two types of options: static, and dynamic. While their interfaces are (currently)
the same, they imply two very different things.

Static options are permanent—they can never be viewed or modified by users, and bot code is
not supposed to modify their properties, either. They are the core of configuration and the absolute
truth of the bot.

Dynamic options are a lot more complicated. They are built to be modifiable at run-time, and viewed
or even modified by users, and can exist on per-server or per-user levels. They are intended to be
used for anything that is not a core truth of the bot, like the number of reactions required to
mute someone with the Votemute feature, or the color used by default in embeds.
