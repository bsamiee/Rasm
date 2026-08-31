namespace Lab.T03;

[Union<string, int, bool>(SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
internal sealed partial class TextNumberOrFlag;

internal static class PartialMatching {
    public static string Label(TextNumberOrFlag union) => union.MapPartially(@default: "other", @string: "text");
    public static int Length(TextNumberOrFlag union) => union.SwitchPartially(@default: static _ => 0, @string: static text => text.Length);
}
