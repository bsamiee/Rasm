namespace Lab.T06;

internal static class Samples {
    public static Fin<Unit> Run() {
        const string expected = "E42";
        string label = ShippingMethod.Express.Switch(standard: static () => "S", express: static () => "E");
        IdOrName value = new(42);
        string text = value.Switch(int32: static i => i.ToString(CultureInfo.InvariantCulture), @string: static s => s);
        return string.Equals(label + text, expected, StringComparison.Ordinal) ? unit : Error.New("T06 switch mismatch");
    }
}
