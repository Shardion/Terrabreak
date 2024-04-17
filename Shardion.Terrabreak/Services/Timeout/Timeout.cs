using System;
using LiteDB;

namespace Shardion.Terrabreak.Services.Timeout
{
    public class Timeout
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();

        public required string Identifier { get; set; }
        public required BsonDocument Data { get; set; }

        public required DateTimeOffset ExpirationDate { get; set; }
        public bool ExpiryProcessed { get; set; }

        public bool IsNear()
        {
            TimeSpan timeBetweenNowAndExpiry = DateTimeOffset.UtcNow - ExpirationDate;
            return timeBetweenNowAndExpiry.TotalMinutes <= 30;
        }
    }
}
