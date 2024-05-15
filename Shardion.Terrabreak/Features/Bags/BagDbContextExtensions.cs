using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Services.Database;

namespace Shardion.Terrabreak.Features.Bags
{
    public static class BagDbContextExtensions
    {
        public static Bag CreateBag(this TerrabreakDatabaseContext context, ulong userId, string name, bool save = true)
        {
            Bag bag = new()
            {
                Name = name,
                OwnerId = userId,
                Entries = [],
            };
            context.Add(bag);
            if (save)
            {
                context.SaveChanges();
            }
            return bag;
        }

        public static Bag? GetBag(this TerrabreakDatabaseContext context, ulong userId, string name)
        {
            return context.Set<Bag>().Where(bag => bag.Name == name && bag.OwnerId == userId).FirstOrDefault();
        }

        public static bool DeleteBag(this TerrabreakDatabaseContext context, ulong userId, string name, bool save = true)
        {
            if (GetBag(context, userId, name) is not Bag bag)
            {
                return false;
            }
            bool successful = DeleteBag(context, bag, save: false);
            if (save)
            {
                context.SaveChanges();
            }
            return successful;
        }

        public static bool DeleteBag(this TerrabreakDatabaseContext context, Bag bag, bool save = true)
        {
            context.Remove(bag);
            if (save)
            {
                context.SaveChanges();
            }
            return true;
        }

        public static bool AddToBag(this TerrabreakDatabaseContext context, ulong userId, string name, string entry, bool save = true)
        {
            if (GetBag(context, userId, name) is not Bag bag)
            {
                return false;
            }
            bool successful = AddToBag(context, bag, entry, save: false);
            if (save)
            {
                context.SaveChanges();
            }
            return successful;
        }

        public static bool AddToBag(this TerrabreakDatabaseContext context, Bag bag, string entry, bool save = true)
        {
            bag.Entries.Add(entry);
            context.Update(bag);
            if (save)
            {
                context.SaveChanges();
            }
            return true;
        }

        public static bool RemoveFromBag(this TerrabreakDatabaseContext context, ulong userId, string name, string entry, bool save = true)
        {
            if (GetBag(context, userId, name) is not Bag bag)
            {
                return false;
            }
            bool successful = RemoveFromBag(context, bag, entry, save: false);
            if (save)
            {
                context.SaveChanges();
            }
            return successful;
        }

        public static bool RemoveFromBag(this TerrabreakDatabaseContext context, Bag bag, string entry, bool save = true)
        {
            if (!bag.Entries.Remove(entry))
            {
                return false;
            }
            context.Update(bag);
            if (save)
            {
                context.SaveChanges();
            }
            return true;
        }

        public static IEnumerable<Bag> FindBags(this TerrabreakDatabaseContext context, ulong userId)
        {
            return context.Set<Bag>().Where(bag => bag.OwnerId == userId);
        }
    }
}
