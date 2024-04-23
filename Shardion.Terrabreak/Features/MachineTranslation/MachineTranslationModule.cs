using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using GTranslate.Results;

namespace Shardion.Terrabreak.Features.MachineTranslation
{
    [CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
    [IntegrationType(ApplicationIntegrationType.UserInstall, ApplicationIntegrationType.GuildInstall)]
    public class MachineTranslationModule : InteractionModuleBase
    {
        private readonly MachineTranslationFeature _machineTranslation;

        public MachineTranslationModule(MachineTranslationFeature machineTranslation)
        {
            _machineTranslation = machineTranslation;
        }

        [MessageCommand("Translate")]
        public Task TranslateMessage(IMessage message)
        {
            return Translate(message.CleanContent);
        }

        [SlashCommand("translate", "Translates the provided text with a machine translation service.")]
        public async Task Translate(
            [Summary(description: "The text to translate.")] string text
        )
        {
            Task<ITranslationResult> resultTask = _machineTranslation.Translator.TranslateAsync(text, "en");
            Task deferralTask = DeferAsync();

            ITranslationResult result = await resultTask;

            string[] translationLines = result.Translation.Split('\n', options: StringSplitOptions.RemoveEmptyEntries);
            for (int lineIndex = 0; lineIndex < translationLines.Length; lineIndex++)
            {
                translationLines[lineIndex] = $"> {translationLines[lineIndex]}";
            }
            string finalTranslation = string.Join('\n', translationLines);

            await deferralTask;
            await FollowupAsync($"Translated from {result.SourceLanguage.Name} to English.\n{finalTranslation}");
        }
    }
}
