namespace Lab.T01;

[SmartEnum<string>]
internal sealed partial class ShippingMethod {
    public static readonly ShippingMethod Standard = new("STANDARD", basePrice: 5.99m, weightMultiplier: 0.5m);
    public static readonly ShippingMethod Express = new("EXPRESS", basePrice: 15.99m, weightMultiplier: 0.75m);

    private readonly decimal _basePrice;
    private readonly decimal _weightMultiplier;

    public decimal Price(decimal orderWeight) => _basePrice + (orderWeight * _weightMultiplier);
}

[SmartEnum]
internal sealed partial class MoneyRoundingStrategy {
    public static readonly MoneyRoundingStrategy Nearest = new("Nearest", static d => decimal.Round(d, 2, MidpointRounding.ToEven));
    public static readonly MoneyRoundingStrategy Up = new("Up", static d => decimal.Round(d, 2, MidpointRounding.ToPositiveInfinity));

    public string Name { get; }

    [UseDelegateFromConstructor]
    public partial decimal Round(decimal value);

    public override string ToString() => Name;
}

[SmartEnum<string>]
internal sealed partial class OrderStatus {
    public static readonly OrderStatus Pending = new("Pending", new(PendingNext));
    public static readonly OrderStatus Shipped = new("Shipped", new(ShippedNext));
    public static readonly OrderStatus Delivered = new("Delivered", new(NoNext));

    private readonly Lazy<IReadOnlyList<OrderStatus>> _nextStates;

    public bool CanTransitionTo(OrderStatus next) => _nextStates.Value.Contains(next);

    private static IReadOnlyList<OrderStatus> PendingNext() => [Shipped];
    private static IReadOnlyList<OrderStatus> ShippedNext() => [Delivered];
    private static IReadOnlyList<OrderStatus> NoNext() => [];
}
