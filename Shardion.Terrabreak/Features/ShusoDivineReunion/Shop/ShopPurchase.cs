namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Shop;

public record ShopPurchase
{
    public required INamedEntity? Entity { get; init; }
    public required PurchaseType Kind { get; init; }
    public required Tier? Tier { get; init; }
    public required int Cost { get; init; }
}
