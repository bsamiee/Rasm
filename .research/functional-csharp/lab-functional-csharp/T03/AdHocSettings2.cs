namespace Lab.T03;

[Union<string, int>(T1Name = "Text", T2Name = "Number")]
internal sealed partial class NamedTextOrNumber {
    static partial void NormalizeText(ref string text) => text = text?.Trim().ToLowerInvariant() ?? "";
}
