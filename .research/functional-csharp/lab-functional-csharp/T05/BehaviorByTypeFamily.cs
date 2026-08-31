namespace Lab.T05;

[SmartEnum<string>]
internal sealed partial class OrderStatus {
    public static readonly OrderStatus Pending = new("Pending");
    public static readonly OrderStatus Paid = new("Paid");
    public static readonly OrderStatus Shipped = new("Shipped");
}

[ValueObject<decimal>]
internal readonly partial struct Amount;

[ComplexValueObject]
internal sealed partial class Boundary {
    public decimal Lower { get; }
    public decimal Upper { get; }
}

[Union<Amount, string>]
internal sealed partial class AmountOrText;

[Union<Boundary, int>]
internal sealed partial class BoundaryOrNumber;

[Union<OrderStatus, string>]
internal readonly partial struct StatusOrText;

[SmartEnum]
internal sealed partial class Channel {
    public static readonly Channel Email = new("email");
    public static readonly Channel Sms = new("sms");

    public string Name { get; }

    public override string ToString() => Name;
}

[Union]
internal abstract partial record Shape {
    internal sealed record Circle(double Radius) : Shape;
}

internal sealed record Order(OrderStatus Status, Amount Total);

internal static class Families {
    public static void Log(Logger logger) {
        Amount amount = Amount.Create(99.95m);
        Boundary boundary = Boundary.Create(1m, 10m);
        logger.Information("keyed smart enum: {@Value}", OrderStatus.Paid);
        logger.Information("simple value object: {@Value}", amount);
        logger.Information("union holding string: {@Value}", (AmountOrText)"pending");
        logger.Information("union holding value object: {@Value}", (AmountOrText)amount);
        logger.Information("union holding smart enum: {@Value}", (StatusOrText)OrderStatus.Paid);
        logger.Information("union holding complex value object: {@Value}", (BoundaryOrNumber)boundary);
        logger.Information("record with members: {@Value}", new Order(OrderStatus.Paid, amount));
        logger.Information("complex value object: {@Value}", boundary);
        logger.Information("keyless smart enum: {@Value}", Channel.Email);
        logger.Information("regular union: {@Value}", new Shape.Circle(2.5));
    }
}
