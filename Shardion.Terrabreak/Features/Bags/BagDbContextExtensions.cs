using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shardion.Terrabreak.Services.Database;

namespace Shardion.Terrabreak.Features.Bags;

public static class BagDbContextExtensions
{
    public static Bag CreateBag(this DbContext context, ulong userId, string name)
    {
        Bag bag = new()
        {
            Name = name,
            OwnerId = userId
        };
        context.Add(bag);
        return bag;
    }

    public static async Task<Bag> CreateBagAsync(this DbContext context, ulong userId, string name,
        CancellationToken token = default)
    {
        Bag bag = new()
        {
            Name = name,
            OwnerId = userId
        };
        await context.AddAsync(bag, token);
        return bag;
    }

    public static Bag? GetBag(this DbContext context, ulong userId, string name)
    {
        return context.Set<Bag>().FirstOrDefault(bag => bag.Name == name && bag.OwnerId == userId);
    }

    public static bool DeleteBag(this DbContext context, ulong userId, string name)
    {
        if (GetBag(context, userId, name) is not Bag bag) return false;
        bool successful = DeleteBag(context, bag);
        return successful;
    }

    public static bool DeleteBag(this DbContext context, Bag bag)
    {
        context.Remove(bag);
        return true;
    }

    public static bool AddToBag(this DbContext context, ulong userId, string name, string entry)
    {
        if (GetBag(context, userId, name) is not Bag bag) return false;
        bool successful = AddToBag(context, bag, entry);
        return successful;
    }

    public static bool AddToBag(this DbContext context, Bag bag, string entry)
    {
        bag.Entries.Add(new BagEntry
            {
                Text = entry,
                Bag = bag
            }
        );
        context.Update(bag);
        return true;
    }

    public static bool AddToBag(this DbContext context, Bag bag, BagEntry entry)
    {
        bag.Entries.Add(entry);
        context.Update(bag);
        return true;
    }

    public static bool RemoveFromBag(this DbContext context, ulong userId, string name, BagEntry entry)
    {
        if (GetBag(context, userId, name) is not { } bag) return false;
        bool successful = RemoveFromBag(context, bag, entry);
        return successful;
    }

    public static bool RemoveFromBag(this DbContext context, Bag bag, BagEntry entry)
    {
        if (!bag.Entries.Remove(entry)) return false;
        context.Update(bag);
        return true;
    }

    public static BagEntry? GetEntry(this DbContext context, Guid entryId)
    {
        return context.Set<BagEntry>().FirstOrDefault(entry => entry.Id == entryId);
    }
}
