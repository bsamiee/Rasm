using System.Globalization;
using Rasm.UI.Input;
using Rasm.UI.Layout;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;
using Topten.RichTextKit.Utils;

namespace Rasm.UI.Automation;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record AutomationElement(Seq<Key> Id, AccessRole Role, Accessible Accessible, Seq<Act> Actions, HashSet<Condition> Conditions, SKRect Frame, Seq<AutomationElement> Children) {

    // --- [PROJECTION]

    internal static Seq<AutomationElement> Projected(Scene scene) => Projected(scene, scene.Layout.Cons(scene.Layers), None, Accessible.Empty, Trampoline.Pure).Run();

    internal static Seq<AutomationElement> Flattened(Seq<AutomationElement> tree) => tree.Bind(static root => toSeq(SuperEnumerable.TraverseDepthFirst(root, static element => element.Children)));

    internal static HashMap<Seq<Key>, AutomationElement> Indexed(Seq<AutomationElement> elements) => toHashMap(Flattened(elements).Map(static element => (element.Id, element)));

    internal static Option<string> Joined(Seq<string> parts) => parts.IsEmpty ? None : Some(string.Join(' ', parts));

    private static Trampoline<Seq<AutomationElement>> Projected(Scene scene, Seq<LayoutNode> layouts, Option<PartOwner> owner, Accessible inherited, Func<Seq<AutomationElement>, Trampoline<Seq<AutomationElement>>> next) =>
        layouts.AsEnumerable().AggregateRight(next, (layout, remaining) => elements => scene.Inert.Contains(layout.Path)
            ? Trampoline.More(() => remaining(elements))
            : Trampoline.More(() => Described(scene, layout, scene.PartAt(layout.Path) || owner, inherited, described => remaining(elements + described))))([]);

    private static Trampoline<Seq<AutomationElement>> Described(Scene scene, LayoutNode layout, Option<PartOwner> owning, Accessible inherited, Func<Seq<AutomationElement>, Trampoline<Seq<AutomationElement>>> complete) {
        Seq<PlacedLayer> anchoring = scene.Anchoring(layout.Path);
        Accessible anchored = anchoring.Choose(held => held.Layer.Trigger.Bind(static trigger => trigger.AnchorRole).Map(role => new Accessible { Role = role, Expanded = scene.Open.Contains(held.Layout.Path) })).Fold(Accessible.Empty, static (held, next) => held.Combine(next));
        Accessible accessible = Charted(layout.View.Accessible.Combine(anchored).Combine(owning.IsSome ? Accessible.Empty : Implied(scene, layout)), layout);
        Accessible passed = accessible.Role.IsSome ? Accessible.Empty : new Accessible { Label = accessible.Label, Help = accessible.Help, Shortcut = accessible.Shortcut }.Combine(inherited);
        return Projected(scene, layout.Children, owning, passed, nested => Trampoline.More(() => complete((nested + Spanned(scene, layout)) switch {
            var children => (from role in accessible.Role
                             from frame in scene.Frame(layout.Path)
                             select new AutomationElement(layout.Path, role, accessible.Combine(new Accessible { Label = role.NamedByContents ? Joined(Texts(layout)) : None }).Combine(inherited),
                                 toSeq((owning.Filter(held => held.Part.Offers(held.Layout, layout.Path)).Map(held => Offered(scene, held, layout.Path)).IfNone([]) + anchoring.Bind(static held => held.Acts)).Distinct()),
                                 layout.Style.Conditions, frame, children))
                .Match(Some: static element => [element], None: () => children),
        })));
    }

    private static Seq<AutomationElement> Spanned(Scene scene, LayoutNode layout) =>
        layout.TextNode.Map(text => toSeq(text.Typeset.Parts.AsIterable()).Choose(laid =>
            from owner in scene.PartAt(layout.Path.Add(new Key.Ordinal((uint)laid.Key)))
            let span = text.Typeset.Text.Spans[laid.Key]
            let role = span.Role.Exists(static held => held == Roles.Link) ? AccessRole.Link : AccessRole.Button
            select new AutomationElement(owner.Path, role, new Accessible { Role = role, Label = string.Concat(laid.Value.Map(static run => Utf32Utils.FromUtf32(run.Text))).Trim() },
                Offered(scene, owner, owner.Path), layout.Style.Conditions.Union(span.Conditions), owner.Bounds, []))).IfNone([]);

    private static Accessible Implied(Scene scene, LayoutNode layout) =>
        layout.View.Link.IsSome ? new Accessible { Role = AccessRole.Link } : layout.View.SwitchPartially(
            (Scene: scene, Layout: layout),
            @default: static (_, _) => Accessible.Empty,
            layer: static (_, layer) => new Accessible { Role = layer.Modal ? AccessRole.Dialog : layer.Trigger.Exists(static trigger => trigger.Acts.IsEmpty) ? AccessRole.Tooltip : AccessRole.Popover },
            scroll: static (held, _) => held.Scene.Scroller(held.Layout.Path)
                .Map(static scroller => new Accessible {
                    Role = AccessRole.ScrollArea,
                    Value = Some(scroller.Overflow.Max).Filter(static extent => extent > 0f).Map<AccessValue>(extent => new AccessValue.Ranged(scroller.Value, 0f, extent)),
                    Orientation = scroller.Axis,
                })
                .IfNone(Accessible.Empty),
            text: static (_, text) => new Accessible { Role = AccessRole.Text, Label = text.Content },
            glyph: static (_, glyph) => Pictured(glyph),
            image: static (_, image) => Pictured(image));

    private static Accessible Charted(Accessible accessible, LayoutNode layout) =>
        (from chart in accessible.Chart
         from frame in layout.Plotted(SKPoint.Empty)
         let spacing = layout.Within(Roles.Tick).Metrics[Metric.Spacing]
         select accessible with {
             Chart = chart with {
                 Axes = chart.Axes.Map(described => frame.Plane.Dimensions.Find(held => held.Axis == described.Axis)
                     .Map(held => described with { Gridlines = held.Axis.Ticks(spacing / frame.Pixels(held.Along)).Filter(static tick => tick.Major).Map(static tick => tick.Value) })
                     .IfNone(described)),
             },
         })
        .IfNone(accessible);

    private static Accessible Pictured(View node) => node.Accessible.Label.Map(static _ => new Accessible { Role = AccessRole.Image }).IfNone(Accessible.Empty);

    private static Seq<string> Texts(LayoutNode layout) =>
        toSeq(SuperEnumerable.TraverseDepthFirst(layout, static node => node.Children)).Choose(static node => node.TextNode.Map(static text => text.Typeset.Text.Content));

    // --- [ACTIONS]

    private static Seq<Act> Offered(Scene scene, PartOwner owner, Seq<Key> path) =>
        toSeq(Act.Items).Filter(act => act.Offered && owner.Part.Performed(new Gesture.Perform(act), new HitTarget(scene, owner, path)).IsSome);
}

[SmartEnum]
public sealed partial class ElementChange {
    public static readonly ElementChange Value = new(static (prior, next) =>
        prior.Accessible.Value != next.Accessible.Value || prior.Accessible.Expanded != next.Accessible.Expanded || prior.Conditions != next.Conditions);
    public static readonly ElementChange Selection = new(static (prior, next) => Selected(prior) != Selected(next));

    [UseDelegateFromConstructor]
    internal partial bool Differs(AutomationElement prior, AutomationElement next);

    private static HashSet<Seq<Key>> Selected(AutomationElement element) =>
        toHashSet(element.Children.Filter(static child => child.Conditions.Contains(Condition.Selected)).Map(static child => child.Id));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AccessibilityNotice {
    internal static Seq<AccessibilityNotice> Changes(HashMap<Seq<Key>, AutomationElement> was, HashMap<Seq<Key>, AutomationElement> now, CultureInfo culture) {
        Seq<Change<AutomationElement>> changes = toSeq(toTrackingHashMap(was.AsIterable()).Snapshot().RemoveRange(was.Keys).AddOrUpdateRange(now.AsIterable()).Changes.Values);
        Seq<EntryMapped<AutomationElement, AutomationElement>> kept = changes.Choose(static change => change is EntryMapped<AutomationElement, AutomationElement> mapped ? Some(mapped) : None);
        return (from held in kept
                from change in toSeq(ElementChange.Items)
                where change.Differs(held.From, held.To)
                select (AccessibilityNotice)new Changed(held.To.Id, change))
            .Concat(changes.Exists(static change => change.HasAdded || change.HasRemoved) || kept.Exists(static held => held.From.Frame != held.To.Frame) ? Seq<AccessibilityNotice>(new LayoutChanged()) : [])
            .Concat(changes.Choose(static change => change switch {
                EntryAdded<AutomationElement> added => Some(added.Value),
                EntryMapped<AutomationElement, AutomationElement> mapped when mapped.From.Accessible.Label != mapped.To.Accessible.Label || mapped.From.Accessible.Value != mapped.To.Accessible.Value => Some(mapped.To),
                _ => Option<AutomationElement>.None,
            })
                .Choose(element =>
                    from interrupts in element.Accessible.Interrupts
                    from text in element.Accessible.Spoken(culture)
                    select (AccessibilityNotice)new Announced(text, interrupts)));
    }

    public sealed record Announced(string Text, bool Interrupts) : AccessibilityNotice;

    public sealed record Changed(Seq<Key> Element, ElementChange Change) : AccessibilityNotice;

    public sealed record FocusChanged(Seq<Key> Element) : AccessibilityNotice;

    public sealed record LayoutChanged : AccessibilityNotice;

    public sealed record Detent : AccessibilityNotice;
}
