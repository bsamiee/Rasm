namespace Lab.T04;

internal static class ShippingMethods {
    public static string Wire(ShippingMethod method) => System.Text.Json.JsonSerializer.Serialize(method);
    public static ShippingMethod? Read(string json) => System.Text.Json.JsonSerializer.Deserialize<ShippingMethod>(json);
    public static bool Accepts(string slug) => ShippingMethod.TryParse(slug, provider: null, out _);
    public static ShippingMethod Parsed(string slug) => ShippingMethod.Parse(slug, provider: null);
}
