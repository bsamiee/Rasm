namespace Lab.T04;

[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
internal sealed partial class Slug {
    private Slug(string value) => Value = value;

    public string Value { get; }

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out Slug? item) {
        item = null;
        if (value is null) return null;
        if (value.Length == 0 || value.Contains(' ', StringComparison.Ordinal)) return new ValidationError("A slug has no spaces and is not empty");
        item = new Slug(value);
        return null;
    }

    public string ToValue() => Value;
}
