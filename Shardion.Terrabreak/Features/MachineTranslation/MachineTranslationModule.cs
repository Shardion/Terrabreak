using System;
using System.Threading.Tasks;
using GTranslate.Results;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Shardion.Terrabreak.Features.MachineTranslation;

public class MachineTranslationModule(MachineTranslationManager mtlManager)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [MessageCommand("Translate",
        Contexts =
        [
            InteractionContextType.BotDMChannel, InteractionContextType.DMChannel, InteractionContextType.Guild
        ])]
    public Task TranslateMessage(RestMessage message)
    {
        return Translate(message.Content);
    }

    [SlashCommand("translate", "Translates the provided text with a machine translation service.",
        Contexts =
        [
            InteractionContextType.BotDMChannel, InteractionContextType.DMChannel, InteractionContextType.Guild
        ])]
    public async Task Translate(
        [SlashCommandParameter(Description = "The text to translate.")]
        string text
    )
    {
        Task<ITranslationResult> resultTask = mtlManager.Translator.TranslateAsync(text, "en");
        Task deferralTask = RespondAsync(InteractionCallback.DeferredMessage());

        ITranslationResult result = await resultTask;

        string[] translationLines = result.Translation.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        for (int lineIndex = 0; lineIndex < translationLines.Length; lineIndex++)
            translationLines[lineIndex] = $"> {translationLines[lineIndex]}";
        string finalTranslation = string.Join('\n', translationLines);

        await deferralTask;
        await FollowupAsync($"Translated from {result.SourceLanguage.Name} to English.\n{finalTranslation}");
    }
}
