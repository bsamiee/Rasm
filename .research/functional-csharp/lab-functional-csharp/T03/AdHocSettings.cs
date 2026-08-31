namespace Lab.T03;

[Union<string, int>(T1Name = "Text", T2Name = "Number", ConversionFromValue = ConversionOperatorsGeneration.None, ConstructorAccessModifier = UnionConstructorAccessModifier.Private)]
internal sealed partial class LabeledTextOrNumber {
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public LabeledTextOrNumber(string text, string label) : this(text) => Label = label;
    public required string Label { get; init; }
}
