using System.Numerics;
using Rasm.UI.Motion;
using Rasm.UI.Automation;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Input;
using Rasm.UI.Layout;
using Rasm.UI.Media;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SuperLinq;
using Topten.RichTextKit;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Key {
    private const char Prefix = '#';

    public string Identifier =>
        Switch(
            named: static named => named.Name.StartsWith(Prefix) ? $"{Prefix}{named.Name}" : named.Name,
            ordinal: static ordinal => $"{Prefix}{ordinal.Index}",
            handle: static _ => $"{Prefix}handle");

    public sealed record Named(string Name) : Key;

    public sealed record Ordinal(uint Index) : Key;

    public sealed record Handle : Key;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record View {
    private View(Option<Painting> painting = default) => Painting = painting.IfNone(Painting.Frame);

    public Option<Key> Key { get; init; }
    public Option<Key> Role { get; init; }
    public HashSet<Condition> Conditions { get; init; }
    public StyleSheet Sheet { get; init; } = StyleSheet.Empty;
    public Fraction Opacity { get; init; } = Fraction.MaxValue;
    public Option<DataSeries> Series { get; init; }
    public Option<Sizing> Width { get; init; }
    public Option<Sizing> Height { get; init; }
    public Option<Factor> Aspect { get; init; }
    public UnitPoint Align { get; init; } = UnitPoint.Start;
    public Option<Matrix4x4> Transform { get; init; }
    public Option<PhysicalAxis> Sticks { get; init; }
    public Option<CellSpan> Spanning { get; init; }
    public Option<Part> Part { get; init; }
    public Option<Lift> Lift { get; init; }
    public Option<Drop> Drop { get; init; }
    public Option<Uri> Link { get; init; }
    public Option<Expiry> Expires { get; init; }
    public Accessible Accessible { get; init; } = Accessible.Empty;

    public View Conditioned(Condition condition) => this with { Conditions = Conditions.TryAdd(condition) };

    public View Grown => this with { Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit };

    public View Stretched(PhysicalAxis along) =>
        along.Map(horizontal: this with { Width = Sizing.Grow.Unit, Height = Sizing.Fit.Content }, vertical: this with { Width = Sizing.Fit.Content, Height = Sizing.Grow.Unit });

    internal static View Divider(PhysicalAxis container) => new Box { Role = Roles.Divider, Accessible = new Accessible { Role = AccessRole.Separator } }.Stretched(container.Cross);

    internal Painting Painting { get; }

    internal virtual Option<View> Inner => None;

    internal virtual Option<PhysicalAxis> MainAxis => None;

    internal virtual Option<PlotPlane> Plotted => None;

    internal virtual Option<Figure> Traced => None;

    internal Option<Pin> Pinned => SwitchPartially(@default: static node => node.Inner.Bind(static inner => inner.Pinned), pin: static pin => Some(pin));

    internal virtual bool Clips(View child) => false;

    public sealed record Box : View;

    public sealed record Flex(PhysicalAxis Axis, Seq<View> Children) : View {
        public Option<Baseline> Baseline { get; init; }
        public bool Wraps { get; init; }

        internal override Option<PhysicalAxis> MainAxis => Some(Axis);
    }

    public sealed record Grid(Tracks Columns, Seq<Seq<View>> Rows) : View;

    public sealed record Lazy(PhysicalAxis Axis, Tracks Across, Seq<Key> Items) : View {
        public Option<float> Extent { get; init; }
        public Key Rows { get; init; } = Roles.ListRow;
        public HashMap<Key, View> Realized { get; init; }

        internal override Option<PhysicalAxis> MainAxis => Some(Axis);
    }

    public sealed record Overlay(View Base, Seq<View> Above) : View {
        internal override Option<View> Inner => Some(Base);
    }

    public sealed record Layer(View Content) : View {
        public Option<Seq<Key>> Anchor { get; init; } = Some(Seq<Key>());
        public UnitPoint Side { get; init; } = UnitPoint.Bottom;
        public float Gap { get; init; }
        public bool Flips { get; init; } = true;
        public Option<IO<Unit>> Dismissed { get; init; }
        public bool Modal { get; init; }
        public Option<Trigger> Trigger { get; init; }

        internal override Option<View> Inner => Some(Content);
    }

    public sealed record Choice(PhysicalAxis Axis, Seq<View> Alternatives) : View {
        internal override Option<PhysicalAxis> MainAxis => Some(Axis);
    }

    public sealed record Scroll(PhysicalAxis Axis, View Content) : View {
        public Option<UnitPoint> Snap { get; init; }
        public Option<Seq<Key>> Follows { get; init; }
        public bool KeepsEnd { get; init; }

        internal override Option<PhysicalAxis> MainAxis => Some(Axis);

        internal override bool Clips(View child) => true;
    }

    public sealed record Plot(PlotPlane Plane, Seq<View> Children) : View {
        public Seq<Guide> Guides { get; init; }
        public Option<Func<Option<Vector2>, IO<Unit>>> Hovered { get; init; }
        public Option<Func<Option<Nearest>, IO<Unit>>> Pointed { get; init; }
        public Option<Func<Nearest, View>> Crosshair { get; init; }

        internal override Option<PlotPlane> Plotted => Some(Plane);

        internal override bool Clips(View child) => child.Pinned.IsNone;
    }

    public sealed record Pin(Vector2 At, View Content) : View {
        public Option<Vector2> Span { get; init; }

        internal override Option<View> Inner => Some(Content);
    }

    public sealed record Mark(Figure Figure) : View(Painting.Figure) {
        internal override Option<Figure> Traced => Some(Figure);
    }

    public sealed record Text(Seq<Span> Spans) : View(Painting.Foreground) {
        public string Content => string.Concat(Spans.Map(static span => span.Text));
        public Option<int> MaxLines { get; init; }
        public Option<float> MaxHeight { get; init; }
        public Fraction Truncation { get; init; } = Fraction.MaxValue;
        public TextAlignment Alignment { get; init; } = TextAlignment.Auto;
        public Option<TextRange> Selected { get; init; }
        public Option<Func<Edit<string>, IO<Unit>>> Edited { get; init; }
        public Option<string> Placeholder { get; init; }
    }

    public sealed record Glyph(GlyphSource Art) : View(Painting.Foreground);

    public sealed record Image(ImageSource Source) : View(Painting.Figure) {
        public ImageFit Fit { get; init; } = ImageFit.Stretch;
        public Factor Density { get; init; } = Factor.Unit;
        public ImageRendering Rendering { get; init; } = ImageRendering.Auto;
        public Option<ColorRamp> Scale { get; init; }
    }

    public sealed record Slot() : View(Painting.Hosted);
}

[SmartEnum]
internal sealed partial class Painting {
    public static readonly Painting Frame = new(paints: false, acceptsBackground: true, inherits: false);
    public static readonly Painting Figure = new(paints: true, acceptsBackground: false, inherits: false);
    public static readonly Painting Foreground = new(paints: true, acceptsBackground: true, inherits: true);
    public static readonly Painting Hosted = new(paints: true, acceptsBackground: true, inherits: false);

    internal bool Paints { get; }
    internal bool AcceptsBackground { get; }
    internal bool Inherits { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Trigger {
    private Trigger(Seq<Act> acts, bool atPointer, Option<AccessRole> anchorRole = default) => (Acts, AtPointer, AnchorRole) = (acts, atPointer, anchorRole);

    internal Seq<Act> Acts { get; }
    internal bool AtPointer { get; }
    internal Option<AccessRole> AnchorRole { get; }
    internal virtual Option<Duration> Hovered => None;

    public sealed record Hover(Duration Delay) : Trigger([], atPointer: false) {
        internal override Option<Duration> Hovered => Delay;
    }

    public sealed record Press() : Trigger([Act.Press, Act.Confirm], atPointer: false, AccessRole.PopUpButton) {
        public Option<Duration> Delay { get; init; }

        internal override Option<Duration> Hovered => Delay;
    }

    public sealed record Secondary() : Trigger([Act.ShowMenu], atPointer: true);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class KeyExtensions {
    extension(Seq<Key> path) {
        internal Option<int> Position(Key key) => Some(path.IndexOf(key)).Filter(static index => index >= 0);
    }
}
