using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Shardion.Terrabreak.Features.RoleRecoloring;

public class RoleRecoloringModule(RoleRecoloringOptions options) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("recolor", "Recolor your role, if any.", Contexts = [InteractionContextType.Guild])]
    public async Task Recolor(string color)
    {
        Debug.Assert(Context.Guild is not null);

        if (!options.UserRecolorableRoles.TryGetValue(Context.User.Id, out ulong roleId))
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("You don't have a recolorable role!")
                .WithFlags(MessageFlags.Ephemeral)));
            return;
        }

        if (ParseColor(color) is not Color parsedColor)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent(@"
                    Could not parse color. Accepted forms:
                    - <hex code>
                    - `#`<hex code>
                    - <red>`,`<green>`,`<blue>
                ")
                .WithFlags(MessageFlags.Ephemeral)));
            return;
        }

        await Task.WhenAll(
            Context.Guild.ModifyRoleAsync(roleId, role =>
            {
                role.Colors = new(parsedColor);
            }),
            RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Role recolored: <@&{roleId}>")
                .WithAllowedMentions(AllowedMentionsProperties.None)
                .WithFlags(MessageFlags.Ephemeral)))
        );
    }

    private static Color? ParseColor(string colorString)
    {
        string trimmedColorString = colorString.Trim();
        if (trimmedColorString.Length == 7 && trimmedColorString[0] == '#')
        {
            return ParseHexColor(trimmedColorString[1..]);
        }
        else if (trimmedColorString.Contains(','))
        {
            return ParseDecimalColorComponents(trimmedColorString.Split(',',
                    StringSplitOptions.TrimEntries
                    | StringSplitOptions.RemoveEmptyEntries
            ));
        }
        else if (trimmedColorString.Length == 6)
        {
            return ParseHexColor(trimmedColorString);
        }
        return null;
    }

    private static Color? ParseHexColor(string hexColorString)
    {
        if (int.TryParse(hexColorString, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int colorInt))
        {
            return new Color(colorInt);
        }
        return null;
    }

    private static Color? ParseDecimalColorComponents(string[] decimalColorString)
    {
        if (decimalColorString.Length != 3)
        {
            return null;
        }
        if (!byte.TryParse(decimalColorString[0], CultureInfo.InvariantCulture, out byte red))
        {
            return null;
        }
        if (!byte.TryParse(decimalColorString[0], CultureInfo.InvariantCulture, out byte green))
        {
            return null;
        }
        if (!byte.TryParse(decimalColorString[0], CultureInfo.InvariantCulture, out byte blue))
        {
            return null;
        }
        return new Color(red, green, blue);
    }
}
