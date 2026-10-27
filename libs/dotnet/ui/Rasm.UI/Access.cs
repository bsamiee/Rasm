using System.Globalization;
using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class Check {
    public static readonly Check Off = new("not checked", None);
    public static readonly Check On = new("checked", Some(Condition.Selected));
    public static readonly Check Mixed = new("mixed", Some(Condition.Mixed));

    public string Spoken { get; }
    public Option<Condition> Conditioned { get; }
}

[SmartEnum]
public sealed partial class Live {
    public static readonly Live Polite = new(interrupts: false);
    public static readonly Live Assertive = new(interrupts: true);

    public bool Interrupts { get; }
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class AccessRole {
    public static readonly AccessRole Text = new(composite: false, namedByContents: true);
    public static readonly AccessRole Image = new(composite: false, namedByContents: false);
    public static readonly AccessRole Heading = new(composite: false, namedByContents: true);
    public static readonly AccessRole Button = new(composite: false, namedByContents: true);
    public static readonly AccessRole MenuButton = new(composite: false, namedByContents: true);
    public static readonly AccessRole PopUpButton = new(composite: false, namedByContents: true);
    public static readonly AccessRole Disclosure = new(composite: false, namedByContents: true, None, Expanded);
    public static readonly AccessRole CheckBox = new(composite: false, namedByContents: true);
    public static readonly AccessRole Switch = new(composite: false, namedByContents: true);
    public static readonly AccessRole Radio = new(composite: false, namedByContents: true);
    public static readonly AccessRole Tab = new(composite: false, namedByContents: true, None, Selected);
    public static readonly AccessRole ListItem = new(composite: false, namedByContents: true, None, Selected);
    public static readonly AccessRole TreeItem = new(composite: false, namedByContents: true, None, Selected);
    public static readonly AccessRole GridCell = new(composite: false, namedByContents: true, None, Selected);
    public static readonly AccessRole MenuItem = new(composite: false, namedByContents: true);
    public static readonly AccessRole Slider = new(composite: false, namedByContents: false);
    public static readonly AccessRole Handle = new(composite: false, namedByContents: false);
    public static readonly AccessRole Separator = new(composite: false, namedByContents: false);
    public static readonly AccessRole TextBox = new(composite: false, namedByContents: false);
    public static readonly AccessRole ProgressBar = new(composite: false, namedByContents: false);
    public static readonly AccessRole Group = new(composite: false, namedByContents: false);
    public static readonly AccessRole ScrollArea = new(composite: false, namedByContents: false);
    public static readonly AccessRole Status = new(composite: false, namedByContents: false);
    public static readonly AccessRole Dialog = new(composite: false, namedByContents: false);
    public static readonly AccessRole Toolbar = new(composite: true, namedByContents: false);
    public static readonly AccessRole RadioGroup = new(composite: true, namedByContents: false, Some(Radio), Checked);
    public static readonly AccessRole TabList = new(composite: true, namedByContents: false, Some(Tab), Checked);
    public static readonly AccessRole List = new(composite: true, namedByContents: false, Some(ListItem), Checked);
    public static readonly AccessRole Tree = new(composite: true, namedByContents: false, Some(TreeItem), Checked);
    public static readonly AccessRole Grid = new(composite: true, namedByContents: false, Some(GridCell), Checked);
    public static readonly AccessRole Menu = new(composite: true, namedByContents: false, Some(MenuItem), Checked);

    private AccessRole(bool composite, bool namedByContents) : this(composite, namedByContents, None, Checked) { }

    public bool Composite { get; }
    public bool NamedByContents { get; }
    public Option<AccessRole> Owns { get; }

    [UseDelegateFromConstructor]
    public partial Accessible WithCheck(Accessible accessible, Check mark);

    private static Accessible Checked(Accessible accessible, Check mark) => accessible with { Value = new AccessValue.Checked(mark) };

    private static Accessible Expanded(Accessible accessible, Check mark) => accessible with { Expanded = mark != Check.Off };

    private static Accessible Selected(Accessible accessible, Check _) => accessible;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AccessValue {
    public sealed record Text(string Value) : AccessValue;

    public sealed record Checked(Check Mark) : AccessValue;

    public sealed record Ranged(double Value, double Min, double Max) : AccessValue {
        public Option<int> Decimals { get; init; }
        public Option<string> Unit { get; init; }
        public Option<string> Description { get; internal init; }
    }

    public sealed record Point(PlotPlane Plane, Vector2 At) : AccessValue {
        public Option<string> Description { get; internal init; }
    }
}

public sealed record Accessible : Monoid<Accessible> {
    public static Accessible Empty { get; } = new();

    public Option<AccessRole> Role { get; init; }
    public Option<string> Label { get; init; }
    public Option<string> Help { get; init; }
    public Option<string> Shortcut { get; init; }
    public Option<AccessValue> Value { get; init; }
    public Option<bool> Expanded { get; init; }
    public Option<PhysicalAxis> Orientation { get; init; }
    public Option<Live> Live { get; init; }

    public Accessible Combine(Accessible rhs) =>
        new() {
            Role = Role | rhs.Role,
            Label = Label | rhs.Label,
            Help = Help | rhs.Help,
            Shortcut = Shortcut | rhs.Shortcut,
            Value = Value | rhs.Value,
            Expanded = Expanded | rhs.Expanded,
            Orientation = Orientation | rhs.Orientation,
            Live = Live | rhs.Live,
        };
}

public sealed record Element(Seq<Key> Id, AccessRole Role, Accessible Accessible, Seq<Act> Actions, HashSet<Condition> Conditions, SKRect Frame, Seq<Element> Children);

[SmartEnum]
public sealed partial class ElementChange {
    public static readonly ElementChange Value = new(static (prior, next) =>
        prior.Accessible.Value != next.Accessible.Value || prior.Accessible.Expanded != next.Accessible.Expanded || prior.Conditions != next.Conditions);
    public static readonly ElementChange Selection = new(static (prior, next) => Selected(prior) != Selected(next));

    [UseDelegateFromConstructor]
    internal partial bool Differs(Element prior, Element next);

    private static HashSet<Seq<Key>> Selected(Element element) =>
        toHashSet(element.Children.Filter(static child => child.Conditions.Contains(Condition.Selected)).Map(static child => child.Id));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AccessibilityNotice {
    public sealed record Announced(string Text, Live Live) : AccessibilityNotice;

    public sealed record Changed(Seq<Key> Element, ElementChange Change) : AccessibilityNotice;

    public sealed record FocusChanged(Seq<Key> Element) : AccessibilityNotice;

    public sealed record LayoutChanged : AccessibilityNotice;

    public sealed record Detent : AccessibilityNotice;
}

public sealed record Output(Seq<SKRect> Damage, Seq<AccessibilityNotice> Notices, Option<IO<Unit>> Effects);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Access {
    // --- [PROJECT]
    internal static Seq<Element> Project(Scene scene, CultureInfo culture) => Projected(scene, culture, scene.Layout, None, Accessible.Empty);

    public static Seq<Element> Flattened(Seq<Element> tree) => tree.Bind(static element => element.Cons(Flattened(element.Children)));

    internal static HashMap<Seq<Key>, Element> Indexed(Seq<Element> elements) => toHashMap(Flattened(elements).Map(static element => (element.Id, element)));

    private static Seq<Element> Projected(Scene scene, CultureInfo culture, LayoutNode layout, Option<PartOwner> owner, Accessible lent) {
        Option<PartOwner> owning = scene.PartAt(layout.Path) | owner;
        Accessible accessible = layout.Node.Accessible.Combine(owning.IsSome ? Accessible.Empty : Implied(scene, layout));
        Accessible lends = accessible.Role.IsSome ? Accessible.Empty : new Accessible { Label = accessible.Label, Help = accessible.Help, Shortcut = accessible.Shortcut }.Combine(lent);
        Seq<Element> children = layout.Children.Bind(child => Projected(scene, culture, child, owning, lends));
        return (from role in accessible.Role
                from frame in scene.Frame(layout.Path)
                select new Element(layout.Path, role, Described(Named(accessible, role, layout, lent), culture), owning.Filter(held => held.Part.Offers(held.Layout, layout.Path)).Map(held => Offered(scene, held, layout.Path)).IfNone([]),
                    layout.Style.Conditions, frame, children))
            .Match(Some: static element => [element], None: () => children);
    }

    private static Accessible Implied(Scene scene, LayoutNode layout) =>
        layout.Node.SwitchPartially(
            (Scene: scene, Layout: layout),
            @default: static (_, _) => Accessible.Empty,
            scroll: static (held, _) => held.Scene.Adjustment(held.Layout.Path)
                .Map(static adjuster => new Accessible { Role = AccessRole.ScrollArea, Value = new AccessValue.Ranged(adjuster.Value, adjuster.Min, adjuster.Max), Orientation = adjuster.Axis })
                .IfNone(Accessible.Empty),
            text: static (_, text) => new Accessible { Role = AccessRole.Text, Label = text.Content },
            glyph: static (_, glyph) => Pictured(glyph),
            image: static (_, image) => Pictured(image));

    private static Accessible Pictured(Node node) => node.Accessible.Label.Map(static _ => new Accessible { Role = AccessRole.Image }).IfNone(Accessible.Empty);

    private static Accessible Named(Accessible accessible, AccessRole role, LayoutNode layout, Accessible lent) =>
        accessible with {
            Label = accessible.Label | (role.NamedByContents ? Contents(layout) : None) | lent.Label,
            Help = accessible.Help | lent.Help,
            Shortcut = accessible.Shortcut | lent.Shortcut,
        };

    private static Option<string> Contents(LayoutNode layout) => Texts(layout) switch { { IsEmpty: true } => None, var texts => Some(string.Join(' ', texts)) };

    private static Seq<string> Texts(LayoutNode layout) => layout.TextNode.Map(static text => text.Source.Content).ToSeq() + layout.Children.Bind(Texts);

    private static Accessible Described(Accessible accessible, CultureInfo culture) =>
        accessible with {
            Value = accessible.Value.Map(value => value.Switch(
                culture,
                text: static (_, text) => (AccessValue)text,
                @checked: static (_, mark) => mark,
                ranged: static (culture, ranged) => ranged with {
                    Description = ranged.Decimals.Match(
                        Some: digits => {
                            string number = Math.Round(ranged.Value, digits, MidpointRounding.ToEven).ToString(culture);
                            return ranged.Unit.Match(Some: suffix => $"{number} {suffix}", None: () => number);
                        },
                        None: () => string.Create(culture, $"{(ranged.Value - ranged.Min) / double.Max(ranged.Max - ranged.Min, double.Epsilon):P0}")),
                },
                point: static (culture, point) => point with { Description = point.Plane.Spoken(point.At, culture) })),
        };

    // --- [ACTIONS]
    private static readonly Seq<Act> Standard = Seq<Act>(new Act.Press(), new Act.Confirm(), new Act.Cancel(), new Act.ShowMenu(), new Act.Delete(), new Act.Increment(), new Act.Decrement());

    private static Seq<Act> Offered(Scene scene, PartOwner owner, Seq<Key> path) =>
        Standard.Filter(act => owner.Part.Performed(new Gesture.Perform(act), scene.HitTarget(owner, path)).Switch(ignored: static _ => false, accepted: static _ => true));

    // --- [HELP]
    public static Option<string> Help(Mount mount) =>
        mount.Hover.Head.Bind(path => {
            Seq<Element> above = Scene.AncestorsAndSelf(path).Choose(mount.Elements.Find);
            return Seq(
                    above.Choose(static element => element.Accessible.Help).Head
                    | mount.Scene.Index.Find(path).Bind(static layout => Scene.Preorder(layout).Choose(static held => held.TextNode.Filter(static text => text.Typeset.Truncated).Map(static text => text.Source.Content)).Head),
                    above.Choose(static element => element.Accessible.Shortcut).Head)
                .Somes() switch {
                    { IsEmpty: true } => Option<string>.None,
                    var shown => Some(string.Join(' ', shown)),
                };
        });

    // --- [CHANGES]
    internal static Seq<AccessibilityNotice> Changes(HashMap<Seq<Key>, Element> was, HashMap<Seq<Key>, Element> now) {
        Seq<(Element Prior, Element Next)> kept = now.Values.ToSeq().Choose(element => was.Find(element.Id).Map(prior => (Prior: prior, Next: element)));
        return (from held in kept
                from change in toSeq(ElementChange.Items)
                where change.Differs(held.Prior, held.Next)
                select (AccessibilityNotice)new AccessibilityNotice.Changed(held.Next.Id, change))
            .Concat(toHashSet(was.Keys) != toHashSet(now.Keys) || kept.Exists(static held => held.Prior.Frame != held.Next.Frame) ? Seq<AccessibilityNotice>(new AccessibilityNotice.LayoutChanged()) : [])
            .Concat(now.Values.ToSeq().Choose(element =>
                from live in element.Accessible.Live
                where was.Find(element.Id).ForAll(prior => prior.Accessible.Label != element.Accessible.Label || prior.Accessible.Value != element.Accessible.Value)
                select (AccessibilityNotice)new AccessibilityNotice.Announced(Spoken(element.Accessible), live)));
    }

    private static string Spoken(Accessible accessible) =>
        string.Join(' ', Seq(accessible.Label, accessible.Value.Bind(static value => value.Switch(
            text: static text => Some(text.Value),
            @checked: static held => Some(held.Mark.Spoken),
            ranged: static ranged => ranged.Description,
            point: static point => point.Description))).Somes());
}
