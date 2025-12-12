using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTranslate;
using GTranslate.Results;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Shardion.Terrabreak.Features.MachineTranslation;

public class MachineTranslationModule(MachineTranslationManager mtlManager)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    private static readonly FrozenDictionary<string, string> Aliases = new Dictionary<string, string>
    {
        ["jp"] = "ja",
    }.ToFrozenDictionary();

    [MessageCommand("Translate",
        Contexts =
        [
            InteractionContextType.BotDMChannel, InteractionContextType.DMChannel, InteractionContextType.Guild
        ])]
    public async Task TranslateMessage(RestMessage message)
    {
        Task<InteractionMessageProperties> resultTask = Translate(message.Content, "en", null);
        Task deferralTask = RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        InteractionMessageProperties result = await resultTask;

        await deferralTask;
        await ModifyResponseAsync(response => response
            .WithComponents(result.Components)
            .WithFlags(result.Flags | MessageFlags.Ephemeral)
        );
    }

    [SlashCommand("translate", "Translates the provided text with a machine translation service.",
        Contexts =
        [
            InteractionContextType.BotDMChannel, InteractionContextType.DMChannel, InteractionContextType.Guild
        ])]
    public async Task TranslateSlash(
        [SlashCommandParameter(Description = "The text to translate.")]
        string text,
        [SlashCommandParameter(Description = "The language that the text is in.")]
        string? fromLang = null,
        [SlashCommandParameter(Description = "The language to translate the text into.")]
        string toLang = "en"
    )
    {
        Task<InteractionMessageProperties> resultTask = Translate(text, toLang, fromLang);
        Task deferralTask = RespondAsync(InteractionCallback.DeferredMessage());

        InteractionMessageProperties result = await resultTask;

        await deferralTask;
        await ModifyResponseAsync(message => message
            .WithComponents(result.Components)
            .WithFlags(result.Flags)
        );
    }

    public async Task<InteractionMessageProperties> Translate(string text, string toLang, string? fromLang)
    {
        if (text.IsWhiteSpace())
        {
            return new InteractionMessageProperties()
                .WithContent("Cannot translate nothing!")
                .WithFlags(MessageFlags.Ephemeral);
        }

        string dereferencedToLang = Aliases.GetValueOrDefault(toLang, toLang);
        string? dereferencedFromLang = fromLang is not null ? Aliases.GetValueOrDefault(fromLang, fromLang) : null;

        ITranslationResult result;

        try
        {
            result = await mtlManager.Translator.TranslateAsync(text, dereferencedToLang, dereferencedFromLang);
        }
        catch (TranslatorException e)
        {
            return new InteractionMessageProperties()
                .WithComponents([
                    new TextDisplayProperties("Error while translating."),
                    new TextDisplayProperties(e.Message)
                ])
                .WithFlags(MessageFlags.IsComponentsV2 | MessageFlags.Ephemeral);
        }

        return new InteractionMessageProperties()
            .WithComponents([
                new TextDisplayProperties($">>> {result.Translation}"),
                new TextDisplayProperties(
                    $"-# **{result.SourceLanguage.Name}** → **{result.TargetLanguage.Name}** using `{result.Service}`")
            ])
            .WithFlags(MessageFlags.IsComponentsV2);
    }
}
