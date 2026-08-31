namespace Lab.T04;

[SmartEnum<int>]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All, UseForModelBinding = true)]
internal sealed partial class ShippingMethod {
    public static readonly ShippingMethod Standard = new(1, "standard");
    public static readonly ShippingMethod Express = new(2, "express");

    public string Slug { get; }

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out ShippingMethod? item) {
        if (value is null) {
            item = null;
            return null;
        }
        item = value switch {
            "standard" => Standard,
            "express" => Express,
            _ => null,
        };
        return item is null ? new ValidationError($"Unknown shipping method '{value}'") : null;
    }

    public string ToValue() => Slug;
}
