using System;
using System.Threading.Tasks;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Shardion.Terrabreak.Services.Database;

namespace Shardion.Terrabreak.Features.Bags;

public class BagComponentsModule(TerrabreakDatabaseContext db) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("bag-remove")]
    public async Task RemoveBag(string entryId)
    {
        if (db.GetEntry(Guid.Parse(entryId)) is not BagEntry entry)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"This entry is no longer in the bag!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        db.Remove(entry);
        await Task.WhenAll(
            db.SaveChangesAsync(),
            RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Removed entry from bag **`{entry.Bag.Name}`.**")
                .WithFlags(MessageFlags.Ephemeral)
        )));
    }
}
