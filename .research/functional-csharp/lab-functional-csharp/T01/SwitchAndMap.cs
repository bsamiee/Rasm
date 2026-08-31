namespace Lab.T01;

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
internal sealed partial class ProductType {
    public static readonly ProductType Groceries = new("Groceries");
    public static readonly ProductType Housewares = new("Housewares");
}

internal static class Matching {
    public static string Label(ProductType type, decimal weight) =>
        type.Switch(
            weight,
            groceries: static w => string.Create(CultureInfo.InvariantCulture, $"cold chain, {w} kg"),
            housewares: static w => string.Create(CultureInfo.InvariantCulture, $"fragile, {w} kg"));

    public static string Handling(ProductType type) => type.MapPartially(@default: "standard", groceries: "cold chain");
    public static void RecordColdChain(ProductType type, ICollection<string> log) => type.SwitchPartially(log, groceries: static l => l.Add("cold chain"));
    public static void RecordAll(ProductType type, ICollection<string> log) =>
        type.SwitchPartially(log, @default: static (l, item) => l.Add(item.Key), groceries: static l => l.Add("cold chain"));
}
