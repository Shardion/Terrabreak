using System.Collections.Generic;
using Shardion.Terrabreak.Services.Database;

namespace Shardion.Terrabreak.Features.Bags
{
    public class BagCollectionManager : AbstractCollectionManager<Bag>
    {
        protected override string CollectionName => "bag";

        public BagCollectionManager(DatabaseManager database) : base(database)
        {
            Collection.EnsureIndex(bag => bag.Name);
            Collection.EnsureIndex(bag => bag.OwnerId);
        }

        public Bag CreateBag(ulong userId, string name)
        {
            Bag bag = new Bag()
            {
                Name = name,
                OwnerId = userId,
                Entries = [],
            };
            Collection.Insert(bag);
            return bag;
        }

        public Bag? GetBag(ulong userId, string name)
        {
            return Collection.FindOne(bag => bag.Name == name && bag.OwnerId == userId);
        }

        public bool DeleteBag(ulong userId, string name)
        {
            if (GetBag(userId, name) is not Bag bag)
            {
                return false;
            }
            return DeleteBag(bag);
        }

        public bool DeleteBag(Bag bag)
        {
            return Collection.Delete(bag.Id);
        }

        public bool AddToBag(ulong userId, string name, string entry)
        {
            if (GetBag(userId, name) is not Bag bag)
            {
                return false;
            }
            return AddToBag(bag, entry);
        }

        public bool AddToBag(Bag bag, string entry)
        {
            bag.Entries.Add(entry);
            return Collection.Update(bag);
        }

        public bool RemoveFromBag(ulong userId, string name, string entry)
        {
            if (GetBag(userId, name) is not Bag bag)
            {
                return false;
            }
            return RemoveFromBag(bag, entry);
        }

        public bool RemoveFromBag(Bag bag, string entry)
        {
            if (!bag.Entries.Remove(entry))
            {
                return false;
            }
            return Collection.Update(bag);
        }

        public IEnumerable<Bag> FindBags(ulong userId)
        {
            return Collection.Find(bag => bag.OwnerId == userId);
        }
    }
}
