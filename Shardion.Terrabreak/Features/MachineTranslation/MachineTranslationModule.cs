using System;
using System.Threading.Tasks;
using GTranslate.Results;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Shardion.Terrabreak.Features.MachineTranslation;

public class MachineTranslationModule(MachineTranslationManager mtlManager)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [MessageCommand("Translate")]
    public Task TranslateMessage(RestMessage message)
    {
        return Translate(message.Content);
    }

    [SlashCommand("translate", "Translates the provided text with a machine translation service.")]
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
