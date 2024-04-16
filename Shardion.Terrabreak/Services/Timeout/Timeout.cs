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
    }
}
