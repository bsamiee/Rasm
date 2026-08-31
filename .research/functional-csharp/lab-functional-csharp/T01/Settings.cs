namespace Lab.T01;

[SmartEnum<int>(ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads, EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
internal sealed partial class Priority {
    public static readonly Priority Low = new(1);
    public static readonly Priority High = new(3);
}
