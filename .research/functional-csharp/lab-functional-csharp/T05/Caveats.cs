namespace Lab.T05;

[ValueObject<int>]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All)]
internal sealed partial class Percentage {
    public static ValidationError? Validate(string? value, IFormatProvider? provider, out Percentage? item) {
        if (int.TryParse(value.AsSpan().TrimEnd('%'), NumberStyles.Integer, provider, out int number))
            return Validate(number, provider, out item);
        item = null;
        return new ValidationError("A percentage ends with '%'.");
    }

    public string ToValue() => string.Create(CultureInfo.InvariantCulture, $"{_value}%");
}

internal static class Caveats {
    public static void Log(Logger logger) {
        logger.Information("value object with object factory: {@Value}", Percentage.Create(42));
        logger.Information("default struct union: {@Value}", Uninitialized<StatusOrText>());
        logger.Information("smart enum without @: {Value}", OrderStatus.Paid);
        logger.Information("value object without @: {Value}", Amount.Create(99.95m));
        logger.Information("record without @: {Value}", new Order(OrderStatus.Paid, Amount.Create(99.95m)));
    }

    private static T Uninitialized<T>() where T : struct => default;
}
