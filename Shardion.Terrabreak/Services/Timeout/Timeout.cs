using System;
using System.ComponentModel.DataAnnotations;

namespace Shardion.Terrabreak.Services.Timeout
{
    public class Timeout
    {
        [Key]
        public Guid Id { get; set; }

        public required string Identifier { get; set; }
        public required byte[] Data { get; set; }

        public required DateTimeOffset ExpirationDate { get; set; }
        public bool ExpiryProcessed { get; set; }

        public bool IsNear()
        {
            TimeSpan timeBetweenNowAndExpiry = DateTimeOffset.UtcNow - ExpirationDate;
            return timeBetweenNowAndExpiry.TotalMinutes <= 30;
        }
    }
}
