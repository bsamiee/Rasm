using System.Globalization;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [TYPES] ---------------------------------------------------------------------------
public enum AccessRole {
    Group,
    Text,
    Image,
    Button,
    MenuButton,
    PopUpButton,
    CheckBox,
    Radio,
    Tab,
    TabList,
    List,
    Tree,
    Row,
    Cell,
    Slider,
    Handle,
    Splitter,
    Disclosure,
    TextField,
    ScrollArea,
}

public enum Live {
    Polite,
    Assertive,
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AccessValue {
    public sealed record Text(string Value) : AccessValue;

    public sealed record Toggled(bool On) : AccessValue;

    public sealed record Range(double Value, double Min, double Max, string Shown) : AccessValue;
}

public sealed record Accessible(AccessRole Role, Option<string> Label) {
    public Seq<Act> Actions { get; init; }
    public Option<AccessValue> Value { get; init; }
    public Option<string> Help { get; init; }
    public Option<Live> Live { get; init; }
    public Option<bool> Expanded { get; init; }

    internal static Accessible Declared(AccessRole role, Option<Accessible> declared) =>
        new(role, declared.Bind(static held => held.Label)) { Help = declared.Bind(static held => held.Help) };
}

public sealed record Element(Seq<Key> Id, Accessible Accessible, LanguageExt.HashSet<Condition> Conditions, SKRect Frame, Seq<Element> Children);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Access {
    // --- [PROJECT]
    public static Seq<Element> Project(Scene scene) => Members(scene, scene.Layout, None);

    internal static HashMap<Seq<Key>, Element> Indexed(Seq<Element> elements) => toHashMap(elements.Bind(Flattened).Map(static element => (element.Id, element)));

    private static Seq<Element> Flattened(Element element) => element.Cons(element.Children.Bind(Flattened));

    private static Seq<Element> Members(Scene scene, Laid laid, Option<LanguageExt.HashSet<Seq<Key>>> claimed) {
        Seq<(Seq<Key> Path, Accessible Accessible)> own = laid.Node.Part.Map(part => part.Access(laid)).IfNone([]);
        Seq<Element> inner = laid.Children.Bind(child => Members(scene, child, laid.Node.Part.IsSome ? Some(toHashSet(own.Map(static pair => pair.Path))) : claimed));
        return laid.Node.Part.IsSome
            ? Nested(own.Choose(pair => Placed(scene, pair.Path, pair.Path, pair.Accessible, [])) + inner)
            : claimed.Exists(held => held.Contains(laid.Path))
                ? inner
                : (claimed.IsNone ? Implied(scene, laid, inner) : None)
                    .IfNone(() => laid.Node.Accessible.Bind(accessible => Placed(scene, laid.Path, laid.Path, accessible, inner)).Match(Some: static element => [element], None: () => inner));
    }

    private static Option<Seq<Element>> Implied(Scene scene, Laid laid, Seq<Element> inner) =>
        laid.Node is Node.Split split
            ? Placed(scene, laid.Path, laid.Children[1].Path, Splitter(laid, split), []).Map(splitter => splitter.Cons(inner))
            : laid.Node is Node.Scroll scroll
                ? Placed(scene, laid.Path, laid.Path, Accessible.Declared(AccessRole.ScrollArea, laid.Node.Accessible) with {
                    Actions = [new Act.Step(scroll.Axis, 1), new Act.Step(scroll.Axis, -1), new Act.Page(scroll.Axis, 1), new Act.Page(scroll.Axis, -1)],
                }, inner).Map(static element => Seq(element))
                : laid.Node is Node.Text text
                    ? Placed(scene, laid.Path, laid.Path, laid.Node.Accessible.IfNone(() => new Accessible(AccessRole.Text, text.Content)), inner).Map(static element => Seq(element))
                    : None;

    private static Accessible Splitter(Laid laid, Node.Split split) =>
        (Extent: Layouts.Pane(laid, split),
         Travel: Layouts.Travel(split, float.Max(0f, split.Axis.Of(Layouts.Content(laid).Size) - split.Axis.Of(laid.Children[1].Frame.Size)))) switch {
             var (extent, travel) => Accessible.Declared(AccessRole.Splitter, laid.Node.Accessible) with {
                 Value = new AccessValue.Range(extent, travel.Least, travel.Most, string.Create(CultureInfo.InvariantCulture, $"{(extent - travel.Least) / float.Max(travel.Most - travel.Least, float.Epsilon):P0}")),
                 Actions = [new Act.Step(split.Axis, 1), new Act.Step(split.Axis, -1)],
             },
         };

    private static Option<Element> Placed(Scene scene, Seq<Key> id, Seq<Key> at, Accessible accessible, Seq<Element> children) =>
        from frame in scene.Frame(at)
        from laid in scene.Index.Find(id)
        select new Element(id, accessible, laid.Node.Conditions, frame, children);

    private static Seq<Element> Nested(Seq<Element> flat) =>
        flat.Filter(element => !flat.Exists(outer => Within(outer.Id, element.Id)))
            .Map(root => root with { Children = root.Children + Nested(flat.Filter(element => Within(root.Id, element.Id))) });

    private static bool Within(Seq<Key> outer, Seq<Key> inner) => inner.Count > outer.Count && inner.Take(outer.Count) == outer;

    // --- [CHANGES]
    public static Seq<Notice> Changes(HashMap<Seq<Key>, Element> was, HashMap<Seq<Key>, Element> now) =>
        now.Values.ToSeq().Choose(element => was.Find(element.Id).Filter(prior => prior.Accessible.Value != element.Accessible.Value).Map(_ => (Notice)new Notice.ValueChanged(element.Id)))
            .Concat(toHashSet(was.Keys) != toHashSet(now.Keys) || now.Values.Exists(element => was.Find(element.Id).Exists(prior => prior.Frame != element.Frame))
                ? Seq<Notice>(new Notice.LayoutChanged())
                : [])
            .Concat(now.Values.ToSeq().Choose(element =>
                from live in element.Accessible.Live
                where was.Find(element.Id).ForAll(prior => prior.Accessible.Label != element.Accessible.Label || prior.Accessible.Value != element.Accessible.Value)
                select (Notice)new Notice.Announced(Spoken(element.Accessible), live)));

    private static string Spoken(Accessible accessible) =>
        string.Join(' ', Seq(accessible.Label, accessible.Value.Bind(static value => value.Switch(text: static text => Some(text.Value), toggled: static _ => Option<string>.None, range: static range => Some(range.Shown)))).Somes());
}
