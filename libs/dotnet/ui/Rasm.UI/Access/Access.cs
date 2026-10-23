using System.Globalization;
using Rasm.UI.Layout;
using Rasm.UI.Scenes;
using SkiaSharp;

namespace Rasm.UI.Access;

// --- [TYPES] ---------------------------------------------------------------------------
public enum AccessAction {
    Press,
    Increment,
    Decrement,
    Confirm,
    Cancel,
    ShowMenu,
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class AccessRole {
    public static readonly AccessRole Group = new("group", []);
    public static readonly AccessRole Text = new("text", []);
    public static readonly AccessRole Image = new("image", []);
    public static readonly AccessRole Button = new("button", [AccessAction.Press]);
    public static readonly AccessRole MenuButton = new("menu-button", [AccessAction.Press, AccessAction.ShowMenu]);
    public static readonly AccessRole Cell = new("cell", [AccessAction.Press]);
    public static readonly AccessRole Handle = new("handle", [AccessAction.Increment, AccessAction.Decrement]);
    public static readonly AccessRole Slider = new("slider", [AccessAction.Increment, AccessAction.Decrement]);
    public static readonly AccessRole Field = new("field", [AccessAction.Confirm, AccessAction.Cancel]);

    public ImmutableArray<AccessAction> Actions { get; }
}

public sealed record Facet(AccessRole Role, string Label) {
    public Option<string> Value { get; init; }
    public Option<string> Help { get; init; }
}

public sealed record Accessible(string Id, Facet Facet, SKRect Bounds, Seq<Accessible> Children);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Accessibility {
    public static Seq<Accessible> Tree(Laid laid) =>
        (laid.Node.Facet | Implicit(laid.Node)).Match(
            Some: facet => Seq(new Accessible(laid.Id, facet, laid.Frame, Members(laid))),
            None: () => Members(laid));

    private static Seq<Accessible> Members(Laid laid) =>
        laid.Node is Node.Widget widget
            ? widget.Component.Facets(laid.Content).Map((part, index) => new Accessible(string.Create(CultureInfo.InvariantCulture, $"{laid.Id}#{index}"), part.Facet, part.Bounds, []))
            : laid.Children.Bind(Tree);

    private static Option<Facet> Implicit(Node node) => node is Node.Text text ? Some(new Facet(AccessRole.Text, text.Content)) : None;
}
