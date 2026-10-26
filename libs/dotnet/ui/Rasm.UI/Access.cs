using System.Globalization;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class RoleCategory {
    public static readonly RoleCategory Widget = new();
    public static readonly RoleCategory Composite = new();
    public static readonly RoleCategory Structure = new();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NameFrom {
    public static readonly NameFrom Contents = new("contents");
    public static readonly NameFrom Author = new("author");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Check {
    public static readonly Check Off = new("false");
    public static readonly Check On = new("true");
    public static readonly Check Mixed = new("mixed");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Live {
    public static readonly Live Polite = new("polite");
    public static readonly Live Assertive = new("assertive");
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AccessRole {
    public static readonly AccessRole Text = new("text", RoleCategory.Structure, NameFrom.Contents, None, Checked);
    public static readonly AccessRole Image = new("img", RoleCategory.Structure, NameFrom.Author, None, Checked);
    public static readonly AccessRole Heading = new("heading", RoleCategory.Structure, NameFrom.Contents, None, Checked);
    public static readonly AccessRole Button = new("button", RoleCategory.Widget, NameFrom.Contents, None, Checked);
    public static readonly AccessRole MenuButton = new("menubutton", RoleCategory.Widget, NameFrom.Contents, None, Checked);
    public static readonly AccessRole PopUpButton = new("popupbutton", RoleCategory.Widget, NameFrom.Contents, None, Checked);
    public static readonly AccessRole Disclosure = new("disclosure", RoleCategory.Widget, NameFrom.Contents, None, Expanded);
    public static readonly AccessRole CheckBox = new("checkbox", RoleCategory.Widget, NameFrom.Contents, None, Checked);
    public static readonly AccessRole Switch = new("switch", RoleCategory.Widget, NameFrom.Contents, None, Checked);
    public static readonly AccessRole Radio = new("radio", RoleCategory.Widget, NameFrom.Contents, None, Checked);
    public static readonly AccessRole Tab = new("tab", RoleCategory.Widget, NameFrom.Contents, None, Selected);
    public static readonly AccessRole ListItem = new("listitem", RoleCategory.Widget, NameFrom.Contents, None, Selected);
    public static readonly AccessRole TreeItem = new("treeitem", RoleCategory.Widget, NameFrom.Contents, None, Selected);
    public static readonly AccessRole GridCell = new("gridcell", RoleCategory.Widget, NameFrom.Contents, None, Selected);
    public static readonly AccessRole MenuItem = new("menuitem", RoleCategory.Widget, NameFrom.Contents, None, Checked);
    public static readonly AccessRole Slider = new("slider", RoleCategory.Widget, NameFrom.Author, None, Checked);
    public static readonly AccessRole Handle = new("handle", RoleCategory.Widget, NameFrom.Author, None, Checked);
    public static readonly AccessRole Separator = new("separator", RoleCategory.Widget, NameFrom.Author, None, Checked);
    public static readonly AccessRole TextBox = new("textbox", RoleCategory.Widget, NameFrom.Author, None, Checked);
    public static readonly AccessRole ProgressBar = new("progressbar", RoleCategory.Widget, NameFrom.Author, None, Checked);
    public static readonly AccessRole Group = new("group", RoleCategory.Structure, NameFrom.Author, None, Checked);
    public static readonly AccessRole ScrollArea = new("scrollarea", RoleCategory.Structure, NameFrom.Author, None, Checked);
    public static readonly AccessRole Status = new("status", RoleCategory.Structure, NameFrom.Author, None, Checked);
    public static readonly AccessRole Dialog = new("dialog", RoleCategory.Structure, NameFrom.Author, None, Checked);
    public static readonly AccessRole Toolbar = new("toolbar", RoleCategory.Composite, NameFrom.Author, None, Checked);
    public static readonly AccessRole RadioGroup = new("radiogroup", RoleCategory.Composite, NameFrom.Author, Some(Radio), Checked);
    public static readonly AccessRole TabList = new("tablist", RoleCategory.Composite, NameFrom.Author, Some(Tab), Checked);
    public static readonly AccessRole List = new("list", RoleCategory.Composite, NameFrom.Author, Some(ListItem), Checked);
    public static readonly AccessRole Tree = new("tree", RoleCategory.Composite, NameFrom.Author, Some(TreeItem), Checked);
    public static readonly AccessRole Grid = new("grid", RoleCategory.Composite, NameFrom.Author, Some(GridCell), Checked);
    public static readonly AccessRole Menu = new("menu", RoleCategory.Composite, NameFrom.Author, Some(MenuItem), Checked);

    public RoleCategory Category { get; }
    public NameFrom NameFrom { get; }
    public Option<AccessRole> Owns { get; }

    [UseDelegateFromConstructor]
    public partial Accessible Marked(Accessible accessible, Check mark);

    private static Accessible Checked(Accessible accessible, Check mark) => accessible with { Value = new AccessValue.Checked(mark) };

    private static Accessible Expanded(Accessible accessible, Check mark) => accessible with { Expanded = mark != Check.Off };

    private static Accessible Selected(Accessible accessible, Check _) => accessible;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AccessValue {
    public sealed record Text(string Value) : AccessValue;

    public sealed record Checked(Check Mark) : AccessValue;

    public sealed record Ranged(double Value, double Min, double Max) : AccessValue {
        public Option<string> Description { get; init; }
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
    public Option<Orientation> Orientation { get; init; }
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

public sealed record Element(Seq<Key> Id, AccessRole Role, Accessible Accessible, Seq<Act> Actions, LanguageExt.HashSet<Condition> Conditions, SKRect Frame, Seq<Element> Children);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Access {
    // --- [PROJECT]
    private sealed record Claim(Accessible Derived, Seq<Act> Actions);

    private sealed record Owner(Laid Laid, Part Part, SKRect Bounds);

    internal static Seq<Element> Project(Scene scene, CultureInfo culture) => Projected(scene, culture, scene.Layout, [], None, Accessible.Empty);

    internal static HashMap<Seq<Key>, Element> Indexed(Seq<Element> elements) => toHashMap(elements.Bind(Flattened).Map(static element => (element.Id, element)));

    internal static Seq<Element> Flattened(Element element) => element.Cons(element.Children.Bind(Flattened));

    private static Seq<Element> Projected(Scene scene, CultureInfo culture, Laid laid, HashMap<Seq<Key>, Claim> claimed, Option<Owner> owner, Accessible lent) {
        Option<Owner> owning = (from part in laid.Node.Part from bounds in scene.Frame(laid.Path) select new Owner(laid, part, bounds)) | owner;
        HashMap<Seq<Key>, Claim> claims = claimed.AddOrUpdateRange(Claims(scene, laid, owner.IsSome));
        Claim claim = claims.Find(laid.Path).IfNone(() => new Claim(owning.IsSome ? Accessible.Empty : Implied(scene, laid), []));
        Accessible accessible = laid.Node.Accessible.Combine(claim.Derived);
        Accessible lends = accessible.Role.IsSome ? Accessible.Empty : new Accessible { Label = accessible.Label, Help = accessible.Help, Shortcut = accessible.Shortcut }.Combine(lent);
        Seq<Element> children = laid.Children.Bind(child => Projected(scene, culture, child, claims, owning, lends));
        return (from role in accessible.Role
                from frame in scene.Frame(laid.Path)
                select new Element(laid.Path, role, Described(Named(accessible, role, laid, lent), culture), claim.Actions + owning.Filter(held => held.Part.Offers(held.Laid, laid.Path)).Map(held => Offered(scene, held, laid.Path)).IfNone([]),
                    laid.Style.Conditions, frame, children))
            .Match(Some: static element => [element], None: () => children);
    }

    private static Seq<(Seq<Key> Path, Claim Claim)> Claims(Scene scene, Laid laid, bool owned) =>
        owned
            ? []
            : (from adjuster in scene.Adjuster(laid.Path)
               from split in adjuster.Switch(scroll: static _ => Option<Adjuster.Split>.None, split: static split => Some(split))
               from handle in Layouts.Handle(laid)
               select (handle.Path, new Claim(Adjusted(AccessRole.Separator, split), Stepping))).ToSeq();

    private static Accessible Implied(Scene scene, Laid laid) =>
        laid.Node.SwitchPartially(
            (Scene: scene, Laid: laid),
            @default: static (_, _) => Accessible.Empty,
            scroll: static (held, _) => held.Scene.Adjuster(held.Laid.Path).Map(static adjuster => Adjusted(AccessRole.ScrollArea, adjuster)).IfNone(Accessible.Empty),
            text: static (_, text) => new Accessible { Role = AccessRole.Text, Label = text.Content },
            glyph: static (_, glyph) => Pictured(glyph),
            image: static (_, image) => Pictured(image));

    private static Accessible Pictured(Node node) => node.Accessible.Label.Map(static _ => new Accessible { Role = AccessRole.Image }).IfNone(Accessible.Empty);

    private static Accessible Adjusted(AccessRole role, Adjuster adjuster) =>
        new() { Role = role, Value = new AccessValue.Ranged(adjuster.Value, adjuster.Min, adjuster.Max), Orientation = adjuster.Axis };

    private static Accessible Named(Accessible accessible, AccessRole role, Laid laid, Accessible lent) =>
        accessible with {
            Label = accessible.Label | (role.NameFrom == NameFrom.Contents ? Contents(laid) : None) | lent.Label,
            Help = accessible.Help | lent.Help,
            Shortcut = accessible.Shortcut | lent.Shortcut,
        };

    private static Option<string> Contents(Laid laid) => Texts(laid) switch { { IsEmpty: true } => None, var texts => Some(string.Join(' ', texts)) };

    private static Seq<string> Texts(Laid laid) => laid.Texted.Map(static text => text.Source.Content).ToSeq() + laid.Children.Bind(Texts);

    private static Accessible Described(Accessible accessible, CultureInfo culture) =>
        accessible with {
            Value = accessible.Value.Map(value => value.Switch(
                culture,
                text: static (_, text) => (AccessValue)text,
                @checked: static (_, mark) => mark,
                ranged: static (culture, ranged) => ranged with {
                    Description = ranged.Description.IfNone(() => string.Create(culture, $"{(ranged.Value - ranged.Min) / double.Max(ranged.Max - ranged.Min, double.Epsilon):P0}")),
                })),
        };

    // --- [ACTIONS]
    private static readonly Seq<Act> Stepping = Seq<Act>(new Act.Increment(), new Act.Decrement());

    private static readonly Seq<Act> Standard = Seq<Act>(new Act.Press(), new Act.Confirm(), new Act.Cancel(), new Act.ShowMenu(), new Act.Delete(None)) + Stepping;

    private static Seq<Act> Offered(Scene scene, Owner owner, Seq<Key> path) =>
        Standard.Filter(act => owner.Part.Performed(new Gesture.Perform(act), new Aim(scene, owner.Laid, owner.Bounds, path)).Switch(pass: static _ => false, take: static _ => true));

    // --- [CHANGES]
    internal static Seq<Notice> Changes(HashMap<Seq<Key>, Element> was, HashMap<Seq<Key>, Element> now) {
        Seq<(Element Prior, Element Next)> kept = now.Values.ToSeq().Choose(element => was.Find(element.Id).Map(prior => (Prior: prior, Next: element)));
        return kept.Filter(static held => held.Prior.Accessible.Value != held.Next.Accessible.Value || held.Prior.Accessible.Expanded != held.Next.Accessible.Expanded || held.Prior.Conditions != held.Next.Conditions)
                .Map(static held => (Notice)new Notice.ValueChanged(held.Next.Id))
            .Concat(kept.Filter(static held => Selected(held.Prior) != Selected(held.Next)).Map(static held => (Notice)new Notice.SelectionChanged(held.Next.Id)))
            .Concat(toHashSet(was.Keys) != toHashSet(now.Keys) || kept.Exists(static held => held.Prior.Frame != held.Next.Frame) ? Seq<Notice>(new Notice.LayoutChanged()) : [])
            .Concat(now.Values.ToSeq().Choose(element =>
                from live in element.Accessible.Live
                where was.Find(element.Id).ForAll(prior => prior.Accessible.Label != element.Accessible.Label || prior.Accessible.Value != element.Accessible.Value)
                select (Notice)new Notice.Announced(Spoken(element.Accessible), live)));
    }

    private static LanguageExt.HashSet<Seq<Key>> Selected(Element element) =>
        toHashSet(element.Children.Filter(static child => child.Conditions.Contains(Condition.Selected)).Map(static child => child.Id));

    private static string Spoken(Accessible accessible) =>
        string.Join(' ', Seq(accessible.Label, accessible.Value.Bind(static value => value.Switch(
            text: static text => Some(text.Value),
            @checked: static held => Some(held.Mark.Map(off: "not checked", on: "checked", mixed: "mixed")),
            ranged: static ranged => ranged.Description))).Somes());
}
