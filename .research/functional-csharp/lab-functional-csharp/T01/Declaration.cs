namespace Lab.T01;

internal abstract class IsoCoded(int numericCode) {
    public int NumericCode { get; } = numericCode;
}

[SmartEnum<string>]
internal sealed partial class Country : IsoCoded {
    public static readonly Country Germany = new("de", "Germany", 276);
    public static readonly Country France = new("fr", "France", 250);

    public string Name { get; }

    static partial void ValidateConstructorArguments(ref string key, ref string name, ref int numericCode) {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key must not be empty.", nameof(key));
        key = key.Trim().ToUpperInvariant();
    }
}
