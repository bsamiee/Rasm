namespace Lab.T04;

[SmartEnum<int>]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.Json, UseForModelBinding = true)]
[ObjectFactory<char>(UseForSerialization = SerializationFrameworks.MessagePack)]
internal sealed partial class Dual {
    public static readonly Dual One = new(1);
    public static readonly Dual Two = new(2);

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out Dual? item) {
        if (value is null) {
            item = null;
            return null;
        }
        item = value switch {
            "one" => One,
            "two" => Two,
            _ => null,
        };
        return item is null ? new ValidationError($"Unknown value '{value}'") : null;
    }

    public static ValidationError? Validate(char value, IFormatProvider? provider, out Dual? item) {
        item = value switch {
            '1' => One,
            '2' => Two,
            _ => null,
        };
        return item is null ? new ValidationError($"Unknown value '{value}'") : null;
    }

    public string ToValue() => Key == 1 ? "one" : "two";

    char IConvertible<char>.ToValue() => Key == 1 ? '1' : '2';
}
