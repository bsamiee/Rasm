namespace Lab.T05;

[ValueObject<int>(SkipToString = true)]
internal readonly partial struct Quantity;

internal static class Rendering {
    public static void Log(Logger logger) {
        logger.Information("keyed smart enum: {@Value}", OrderStatus.Paid);
        logger.Information("simple value object: {@Value}", Amount.Create(99.95m));
        logger.Information("union holding value object: {@Value}", (AmountOrText)Amount.Create(99.95m));
        logger.Information("union holding complex value object: {@Value}", (BoundaryOrNumber)Boundary.Create(1m, 10m));
        logger.Information("value object with SkipToString: {@Value}", Quantity.Create(3));
    }
}
