using System.Globalization;
using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class Check {
    public static readonly Check Off = new(None);
    public static readonly Check On = new(Some(Condition.Selected));
    public static readonly Check Mixed = new(Some(Condition.Mixed));

    internal Option<Condition> Conditioned { get; }
}

[SmartEnum]
public sealed partial class Live {
    public static readonly Live Polite = new(interrupts: false);
    public static readonly Live Assertive = new(interrupts: true);

    public bool Interrupts { get; }
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class AccessRole {
    public static readonly AccessRole Text = new(namedByContents: true);
    public static readonly AccessRole Image = new(namedByContents: false);
    public static readonly AccessRole Heading = new(namedByContents: true);
    public static readonly AccessRole Link = new(namedByContents: true);
    public static readonly AccessRole Button = new(namedByContents: true);
    public static readonly AccessRole MenuButton = new(namedByContents: true);
    public static readonly AccessRole PopUpButton = new(namedByContents: true);
    public static readonly AccessRole Disclosure = new(namedByContents: true);
    public static readonly AccessRole CheckBox = new(namedByContents: true);
    public static readonly AccessRole Switch = new(namedByContents: true);
    public static readonly AccessRole Radio = new(namedByContents: true);
    public static readonly AccessRole Tab = new(namedByContents: true);
    public static readonly AccessRole ListItem = new(namedByContents: true);
    public static readonly AccessRole TreeItem = new(namedByContents: true);
    public static readonly AccessRole GridCell = new(namedByContents: true);
    public static readonly AccessRole MenuItem = new(namedByContents: true);
    public static readonly AccessRole Slider = new(namedByContents: false);
    public static readonly AccessRole Handle = new(namedByContents: false);
    public static readonly AccessRole Separator = new(namedByContents: false);
    public static readonly AccessRole TextBox = new(namedByContents: false);
    public static readonly AccessRole ProgressBar = new(namedByContents: false);
    public static readonly AccessRole Group = new(namedByContents: false);
    public static readonly AccessRole ScrollArea = new(namedByContents: false);
    public static readonly AccessRole Status = new(namedByContents: false);
    public static readonly AccessRole Dialog = new(namedByContents: false);
    public static readonly AccessRole Popover = new(namedByContents: false);
    public static readonly AccessRole Tooltip = new(namedByContents: true);
    public static readonly AccessRole Toolbar = new(composite: true, namedByContents: false, None);
    public static readonly AccessRole RadioGroup = new(Radio);
    public static readonly AccessRole TabList = new(Tab);
    public static readonly AccessRole List = new(ListItem);
    public static readonly AccessRole Tree = new(TreeItem);
    public static readonly AccessRole Grid = new(GridCell);
    public static readonly AccessRole Menu = new(MenuItem);

    private AccessRole(bool namedByContents) : this(composite: false, namedByContents, None) { }

    private AccessRole(AccessRole owns) : this(composite: true, namedByContents: false, Some(owns)) { }

    internal bool Composite { get; }
    internal bool NamedByContents { get; }
    internal Option<AccessRole> Owns { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AccessValue {
    public abstract string Spoken(CultureInfo culture);

    public sealed record Text(string Value) : AccessValue {
        public override string Spoken(CultureInfo culture) => Value;
    }

    public sealed record Ranged(double Value, double Min, double Max) : AccessValue {
        public Option<Axis> Axis { get; init; }

        public override string Spoken(CultureInfo culture) =>
            Axis.Match(Some: axis => axis.Spoken((float)Value, culture), None: () => string.Create(culture, $"{(Value - Min) / double.Max(Max - Min, double.Epsilon):P0}"));
    }

    public sealed record Point(PlotPlane Plane, Vector2 At) : AccessValue {
        public override string Spoken(CultureInfo culture) => Plane.Spoken(At, culture);
    }
}

public sealed record AxisDescription(Option<string> Title, Axis Axis) {
    public Seq<float> Gridlines { get; init; }
}

public sealed record SeriesDescription(string Label, Figure Figure);

public sealed record ChartDescription(string Title, Seq<AxisDescription> Axes, Seq<SeriesDescription> Series) {
    public Option<string> Summary { get; init; }
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
    public Option<ChartDescription> Chart { get; init; }

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
            Chart = Chart | rhs.Chart,
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

public sealed record Lifted(IO<Transfer> Transfer, SKPoint At);

public sealed record Output(Seq<SKRect> Damage, Seq<AccessibilityNotice> Notices, Option<IO<Unit>> Effects) {
    public Option<Lifted> Lifted { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Access {
    // --- [PROJECT]
    internal static Seq<Element> Project(Scene scene) => scene.Layout.Cons(scene.Layers).Bind(layout => Projected(scene, layout, None, Accessible.Empty));

    internal static Seq<Element> Flattened(Seq<Element> tree) => tree.Bind(static root => toSeq(SuperEnumerable.TraverseDepthFirst(root, static element => element.Children)));

    internal static HashMap<Seq<Key>, Element> Indexed(Seq<Element> elements) => toHashMap(Flattened(elements).Map(static element => (element.Id, element)));

    private static Seq<Element> Projected(Scene scene, LayoutNode layout, Option<PartOwner> owner, Accessible lent) =>
        scene.Inert.Contains(layout.Path) ? [] : Admitted(scene, layout, scene.PartAt(layout.Path) | owner, lent);

    private static Seq<Element> Admitted(Scene scene, LayoutNode layout, Option<PartOwner> owning, Accessible lent) {
        Accessible accessible = Charted(layout.Node.Accessible.Combine(owning.IsSome ? Accessible.Empty : Implied(scene, layout)), layout);
        Accessible lends = accessible.Role.IsSome ? Accessible.Empty : new Accessible { Label = accessible.Label, Help = accessible.Help, Shortcut = accessible.Shortcut }.Combine(lent);
        Seq<Element> children = layout.Children.Bind(child => Projected(scene, child, owning, lends));
        return (from role in accessible.Role
                from frame in scene.Frame(layout.Path)
                select new Element(layout.Path, role, accessible.Combine(new Accessible { Label = role.NamedByContents ? Joined(Texts(layout)) : None }).Combine(lent),
                    owning.Filter(held => held.Part.Offers(held.Layout, layout.Path)).Map(held => Offered(scene, held, layout.Path)).IfNone([]), layout.Style.Conditions, frame, children))
            .Match(Some: static element => [element], None: () => children);
    }

    private static Accessible Implied(Scene scene, LayoutNode layout) =>
        layout.Node.Link.IsSome ? new Accessible { Role = AccessRole.Link } : layout.Node.SwitchPartially(
            (Scene: scene, Layout: layout),
            @default: static (_, _) => Accessible.Empty,
            layer: static (_, layer) => new Accessible { Role = layer.Modal ? AccessRole.Dialog : layer.Hover.IsSome ? AccessRole.Tooltip : AccessRole.Popover },
            scroll: static (held, _) => held.Scene.Scroller(held.Layout.Path)
                .Map(static scroller => new Accessible { Role = AccessRole.ScrollArea, Value = new AccessValue.Ranged(scroller.Value, 0f, scroller.Overflow.Max), Orientation = scroller.Axis })
                .IfNone(Accessible.Empty),
            text: static (_, text) => new Accessible { Role = AccessRole.Text, Label = text.Content },
            glyph: static (_, glyph) => Pictured(glyph),
            image: static (_, image) => Pictured(image));

    private static Accessible Charted(Accessible accessible, LayoutNode layout) =>
        (from chart in accessible.Chart
         from frame in Layouts.Plotted(layout)
         let spacing = Styles.Resolve(layout.Inner, Role.Tick, layout.Style.Conditions).Metrics[Metric.Spacing]
         select accessible with {
             Chart = chart with {
                 Axes = chart.Axes.Map(described => frame.Plane.Dimensions.Find(held => held.Axis == described.Axis)
                     .Map(held => described with { Gridlines = held.Axis.Ticks(spacing / frame.Pixels(held.Along)).Filter(static tick => tick.Major).Map(static tick => tick.Value) })
                     .IfNone(described)),
             },
         })
        .IfNone(accessible);

    private static Accessible Pictured(Node node) => node.Accessible.Label.Map(static _ => new Accessible { Role = AccessRole.Image }).IfNone(Accessible.Empty);

    private static Option<string> Joined(Seq<string> parts) => parts.IsEmpty ? None : Some(string.Join(' ', parts));

    private static Seq<string> Texts(LayoutNode layout) => layout.TextNode.Map(static text => text.Typeset.Text.Content).ToSeq() + layout.Children.Bind(Texts);

    // --- [ACTIONS]
    private static readonly Seq<Act> Standard = Seq(Act.Press, Act.Confirm, Act.Cancel, Act.ShowMenu, Act.Delete, Act.Increment, Act.Decrement, Act.Reset);

    private static Seq<Act> Offered(Scene scene, PartOwner owner, Seq<Key> path) =>
        Standard.Filter(act => owner.Part.Performed(new Gesture.Perform(act), scene.HitTarget(owner, path)).IsSome);

    // --- [HELP]
    public static Option<string> Help(Mount mount) =>
        mount.Hover.Head.Bind(path => Scene.AncestorsAndSelf(path).Choose(mount.Elements.Find) switch {
            var above => Joined(Seq(
                    above.Choose(static element => element.Accessible.Help).Head
                    | mount.Scene.Index.Find(path).Bind(static layout => Scene.Preorder(layout).Choose(static held => held.TextNode.Filter(static text => text.Typeset.Truncated).Map(static text => text.Typeset.Text.Content)).Head),
                    above.Choose(static element => element.Accessible.Shortcut).Head)
                .Somes()),
        });

    // --- [CHANGES]
    internal static Seq<AccessibilityNotice> Changes(HashMap<Seq<Key>, Element> was, HashMap<Seq<Key>, Element> now, CultureInfo culture) {
        Seq<Change<Element>> changes = toSeq(toTrackingHashMap(was.AsIterable()).Snapshot().RemoveRange(was.Keys).AddOrUpdateRange(now.AsIterable()).Changes.Values);
        Seq<EntryMapped<Element, Element>> kept = changes.Choose(static change => change is EntryMapped<Element, Element> mapped ? Some(mapped) : None);
        return (from held in kept
                from change in toSeq(ElementChange.Items)
                where change.Differs(held.From, held.To)
                select (AccessibilityNotice)new AccessibilityNotice.Changed(held.To.Id, change))
            .Concat(changes.Exists(static change => change.HasAdded || change.HasRemoved) || kept.Exists(static held => held.From.Frame != held.To.Frame) ? Seq<AccessibilityNotice>(new AccessibilityNotice.LayoutChanged()) : [])
            .Concat(changes.Choose(static change => change switch {
                EntryAdded<Element> added => Some(added.Value),
                EntryMapped<Element, Element> mapped when mapped.From.Accessible.Label != mapped.To.Accessible.Label || mapped.From.Accessible.Value != mapped.To.Accessible.Value => Some(mapped.To),
                _ => Option<Element>.None,
            })
                .Choose(element => element.Accessible.Live.Map(live => (AccessibilityNotice)new AccessibilityNotice.Announced(
                    string.Join(' ', Seq(element.Accessible.Label, element.Accessible.Value.Map(value => value.Spoken(culture))).Somes()), live))));
    }
}
