using System.Globalization;
using System.Numerics;
using System.Reflection;
using Rasm.UI.Styling;
using SkiaSharp;
using Topten.RichTextKit;
using Wacton.Unicolour;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<int>(SkipIComparable = true)]
public sealed partial class PhysicalAxis {
    public static readonly PhysicalAxis Horizontal = new(0);
    public static readonly PhysicalAxis Vertical = new(1);

    public PhysicalAxis Cross => Map(horizontal: Vertical, vertical: Horizontal);

    public float Of(Vector2 value) => value[Key];

    public float Of(SKSize size) => Of(size.ToPoint());

    public Vector2 With(Vector2 value, float along) => value.WithElement(Key, along);

    public SKRect Spanned(SKRect frame, float start, float extent) => SKRect.Create(With(frame.Location, start), new SKSize(With(frame.Size.ToPoint(), extent)));
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<Invalid>]
public sealed partial class Fraction {
    public static Fraction MinValue { get; } = new(0f);
    public static Fraction Half { get; } = new(0.5f);
    public static Fraction MaxValue { get; } = new(1f);

    public Fraction Complement => new(1f - _value);

    public Fraction Midpoint(Fraction other) => new((_value + other._value) / 2f);

    public static Fin<Fraction> Validated(float value) => TryCreate(value, out Fraction? fraction, out Invalid? fault) ? fraction : fault;

    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref float value) =>
        validationError = value is >= 0f and <= 1f ? null : new Invalid(string.Create(CultureInfo.InvariantCulture, $"fraction {value} lies outside [0, 1]"));
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<Invalid>]
public sealed partial class Factor {
    public static Factor Unit { get; } = new(1f);

    public static Fin<Factor> Validated(float value) => TryCreate(value, out Factor? factor, out Invalid? fault) ? factor : fault;

    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref float value) =>
        validationError = float.IsFinite(value) && value > 0f ? null : new Invalid(string.Create(CultureInfo.InvariantCulture, $"factor {value} is not finite and positive"));
}

public sealed record UnitPoint(Fraction X, Fraction Y) {
    public static UnitPoint Start { get; } = new(Fraction.MinValue, Fraction.MinValue);
    public static UnitPoint Center { get; } = new(Fraction.Half, Fraction.Half);

    public float Of(PhysicalAxis along) => along.Of(new Vector2((float)X, (float)Y));
}

[ComplexValueObject]
[ValidationError<Invalid>]
public sealed partial class Limits {
    public static Limits Open { get; } = Create(0f, float.PositiveInfinity);

    public float Min { get; }
    public float Max { get; }

    public float Clamped(float value) => float.Clamp(value, Min, Max);

    public static Fin<Limits> Validated(float min, float max) => TryCreate(min, max, out Limits? limits, out Invalid? fault) ? limits : fault;

    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref float min, ref float max) =>
        validationError = float.IsFinite(min) && min >= 0f && max >= min
            ? null
            : new Invalid(string.Create(CultureInfo.InvariantCulture, $"limits ({min}, {max}) are not a non-negative ordered span"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Sizing {
    public Limits Limits { get; init; } = Limits.Open;

    internal float Growth => SwitchPartially(@default: static _ => 0f, grow: static grow => (float)grow.Weight);
    internal Option<int> Yields => Map(@fixed: Option<int>.None, fit: Some(1), grow: Some(0), percent: Some(1));
    internal bool Stretches => Growth > 0f;
    internal Option<float> Definite => SwitchPartially(@default: static _ => Option<float>.None, @fixed: static held => Some(held.Limits.Clamped(held.Extent)));

    public static Seq<float> Stretched(Seq<Fixed> items, float space) =>
        Layouts.Distribute(items.Map(static item => ((Sizing)(Grow.Unit with { Limits = item.Limits }), item.Limits.Clamped(item.Extent), item.Limits.Min)), space);

    internal float Floor(float content) => SwitchPartially(content, @default: static (_, held) => held.Limits.Min, fit: static (least, held) => float.Max(held.Limits.Min, least));

    public sealed record Fixed(float Extent) : Sizing;

    public sealed record Fit : Sizing {
        public static Fit Content { get; } = new();
    }

    public sealed record Grow(Factor Weight) : Sizing {
        public static Grow Unit { get; } = new(Factor.Unit);
    }

    public sealed record Percent(Fraction Fraction) : Sizing;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Tracks {
    public sealed record Explicit(Seq<Sizing> Sizes) : Tracks;

    public sealed record AutoFill(Sizing Size) : Tracks;
}

[SmartEnum]
public sealed partial class ImageFit {
    public static readonly ImageFit Stretch = new(static (content, _) => content);
    public static readonly ImageFit Contain = new(static (content, natural) => content.AspectFit(natural));
    public static readonly ImageFit Cover = new(static (content, natural) => content.AspectFill(natural));

    [UseDelegateFromConstructor]
    public partial SKRect Fitted(SKRect content, SKSize natural);
}

[SmartEnum<string>]
public sealed partial class ImageRendering {
    private static readonly SKSamplingOptions Smooth = new(SKFilterMode.Linear, SKMipmapMode.Linear);
    private static readonly SKSamplingOptions Nearest = new(SKFilterMode.Nearest, SKMipmapMode.None);

    public static readonly ImageRendering Auto = new("auto", Smooth, Smooth);
    public static readonly ImageRendering HighQuality = new("high-quality", new SKSamplingOptions(SKCubicResampler.Mitchell), Smooth);
    public static readonly ImageRendering CrispEdges = new("crisp-edges", Nearest, Nearest);
    public static readonly ImageRendering Pixelated = new("pixelated", Nearest, Smooth);

    private readonly SKSamplingOptions magnified;
    private readonly SKSamplingOptions minified;

    internal SKSamplingOptions Sampling(float density) => density > 1f ? magnified : minified;
}

[SmartEnum]
internal sealed partial class Painting {
    public static readonly Painting Frame = new(paints: false, acceptsBackground: true, inherits: false);
    public static readonly Painting Figure = new(paints: true, acceptsBackground: false, inherits: false);
    public static readonly Painting Ink = new(paints: true, acceptsBackground: true, inherits: true);
    public static readonly Painting Hosted = new(paints: true, acceptsBackground: true, inherits: false);

    internal bool Paints { get; }
    internal bool AcceptsBackground { get; }
    internal bool Inherits { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GlyphSource {
    internal abstract (Assembly Assembly, string Resource, bool Tinted) Shown(SKColorF under);

    public sealed record Tinted(Assembly Assembly, string Resource) : GlyphSource {
        internal override (Assembly Assembly, string Resource, bool Tinted) Shown(SKColorF under) => (Assembly, Resource, true);
    }

    public sealed record Paired(Assembly Assembly, string Light, string Dark) : GlyphSource {
        private static readonly Unicolour White = new(ColourSpace.Rgb, 1.0, 1.0, 1.0);
        private static readonly Unicolour Black = new(ColourSpace.Rgb, 0.0, 0.0, 0.0);

        public string Resource(SKColorF under) => under.ToUnicolour() switch { var held => held.Contrast(White) > held.Contrast(Black) ? Dark : Light };

        internal override (Assembly Assembly, string Resource, bool Tinted) Shown(SKColorF under) => (Assembly, Resource(under), false);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Key {
    private const char Mark = '#';

    public string Identifier =>
        Switch(
            named: static named => named.Name.StartsWith(Mark) ? $"{Mark}{named.Name}" : named.Name,
            ordinal: static ordinal => $"{Mark}{ordinal.Index}",
            handle: static _ => $"{Mark}handle");

    public sealed record Named(string Name) : Key;

    public sealed record Ordinal(uint Index) : Key;

    public sealed record Handle : Key;
}

public sealed record Span(string Text) {
    public Option<Role> Role { get; init; }
    public LanguageExt.HashSet<Condition> Conditions { get; init; }

    public static implicit operator Span(string text) => FromString(text);

    public static Span FromString(string text) => new(text);
}

[SmartEnum]
public sealed partial class EditPhase {
    public static readonly EditPhase Preview = new(settles: false);
    public static readonly EditPhase Step = new(settles: true);
    public static readonly EditPhase Commit = new(settles: true);
    public static readonly EditPhase Cancel = new(settles: false);

    public bool Settles { get; }
}

public sealed record Edit<TValue>(EditPhase Phase, Option<TValue> Value) : K<Edit, TValue> {
    public Option<TValue> Applied => Value.Filter(_ => Phase.Settles);
}

public sealed class Edit : Traversable<Edit> {
    public static Edit<T> Preview<T>(T value) => new(EditPhase.Preview, Some(value));

    public static Edit<T> Step<T>(T value) => new(EditPhase.Step, Some(value));

    public static Edit<T> Commit<T>(T value) => new(EditPhase.Commit, Some(value));

    public static Edit<T> Cancel<T>() => new(EditPhase.Cancel, None);

    public static K<Edit, B> Map<A, B>(Func<A, B> f, K<Edit, A> ma) => ma.As() switch { var edit => new Edit<B>(edit.Phase, edit.Value.Map(f)) };

    public static K<F, K<Edit, B>> Traverse<F, A, B>(Func<A, K<F, B>> f, K<Edit, A> ta) where F : Applicative<F> =>
        ta.As() switch {
            var edit => edit.Value.Match(Some: value => f(value).Map(held => (K<Edit, B>)new Edit<B>(edit.Phase, Some(held))), None: () => F.Pure<K<Edit, B>>(new Edit<B>(edit.Phase, None))),
        };

    public static Fold<A, S> FoldStep<A, S>(K<Edit, A> ta, S initialState) =>
        ta.As().Value.Match(Some: value => Fold.Loop(initialState, value, Fold.Done<A, S>), None: () => Fold.Done<A, S>(initialState));

    public static Fold<A, S> FoldStepBack<A, S>(K<Edit, A> ta, S initialState) => FoldStep(ta, initialState);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Selection {
    public Option<Key> Lead => Switch(one: static one => one.Picked, many: static many => many.Focus);

    public bool Holds(Key key) => Switch(key, one: static (held, one) => one.Picked == Some(held), many: static (held, many) => many.Picked.Contains(held));

    public Selection Raised(Seq<Key> ordered, Key hit, KeyModifiers modifiers) =>
        Switch(
            (Ordered: ordered, Hit: hit, Modifiers: modifiers),
            one: static (held, _) => (Selection)new One(Some(held.Hit)),
            many: static (held, many) => (held.Modifiers.Common, held.Modifiers.Shift) switch {
                (true, _) => new Many(many.Picked.SymmetricExcept([held.Hit]), Some(held.Hit), Some(held.Hit)),
                (_, true) => Spanned(held.Ordered, many.Anchor, held.Hit),
                _ when many.Picked.Contains(held.Hit) => new Many(many.Picked, Some(held.Hit), Some(held.Hit)),
                _ => Only(held.Hit),
            });

    public Selection Led(Seq<Key> ordered, Key lead, bool extend) =>
        Switch(
            (Ordered: ordered, Lead: lead, Extend: extend),
            one: static (held, _) => (Selection)new One(Some(held.Lead)),
            many: static (held, many) => held.Extend ? Spanned(held.Ordered, many.Anchor | many.Focus, held.Lead) : Only(held.Lead));

    public Selection All(Seq<Key> ordered) => Switch(ordered, one: static (_, one) => (Selection)one, many: static (keys, many) => new Many(toHashSet(keys), many.Anchor, many.Focus));

    public Selection Swept(Seq<Key> ordered, LanguageExt.HashSet<Key> covered, KeyModifiers modifiers) =>
        Switch(
            (Ordered: ordered, Covered: covered, Modifiers: modifiers),
            one: static (held, _) => (Selection)new One(held.Ordered.Find(held.Covered.Contains)),
            many: static (held, many) => (held.Modifiers.Common, held.Modifiers.Shift) switch {
                (true, _) => many.Picked.SymmetricExcept(held.Covered),
                (_, true) => many.Picked.Union(held.Covered),
                _ => held.Covered,
            } switch {
                var picked => held.Ordered.Filter(held.Covered.Contains) switch {
                    var swept => new Many(picked, swept.Head | many.Anchor, swept.Last | many.Focus),
                },
            });

    internal Selection Renamed(HashMap<Key, Key> renames) =>
        Switch(
            renames,
            one: static (held, one) => (Selection)new One(one.Picked.Map(key => Renamed(held, key))),
            many: static (held, many) => new Many(many.Picked.Map(key => Renamed(held, key)), many.Anchor.Map(key => Renamed(held, key)), many.Focus.Map(key => Renamed(held, key))));

    internal static Option<int> Position(Seq<Key> ordered, Key key) =>
        ordered.Map(static (held, index) => (Key: held, Index: index)).Find(entry => entry.Key == key).Map(static entry => entry.Index);

    private static Key Renamed(HashMap<Key, Key> renames, Key key) => renames.Find(key).IfNone(key);

    private static Many Only(Key key) => new([key], Some(key), Some(key));

    private static Many Spanned(Seq<Key> ordered, Option<Key> anchor, Key target) =>
        (from end in Position(ordered, target)
         let found = anchor.Bind(held => Position(ordered, held))
         let start = found.IfNone(end)
         select new Many(toHashSet(ordered.Skip(int.Min(start, end)).Take(int.Abs(start - end) + 1)), found.IsSome ? anchor : Some(target), Some(target)))
        .IfNone(() => Only(target));

    public sealed record One(Option<Key> Picked) : Selection;

    public sealed record Many(LanguageExt.HashSet<Key> Picked, Option<Key> Anchor, Option<Key> Focus) : Selection;
}

public sealed record Expiry(Duration Lifetime, IO<Unit> Elapsed);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SlotKind {
    public sealed record Text(string Value, Option<int> MaxLines, Func<Edit<string>, IO<Unit>> Edited) : SlotKind;

    public sealed record Native : SlotKind;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Node {
    private static readonly Func<Option<Vector2>, Option<IO<Unit>>> Unhovered = static _ => None;

    public Option<Key> Key { get; init; }
    public Option<Role> Role { get; init; }
    public LanguageExt.HashSet<Condition> Conditions { get; init; }
    public Option<Sizing> Width { get; init; }
    public Option<Sizing> Height { get; init; }
    public Option<Factor> Aspect { get; init; }
    public UnitPoint Align { get; init; } = UnitPoint.Start;
    public Option<Part> Part { get; init; }
    public Option<Expiry> Expires { get; init; }
    public Accessible Accessible { get; init; } = Accessible.Empty;

    internal Option<Node> Inner => SwitchPartially(@default: static _ => Option<Node>.None, overlay: static overlay => Some(overlay.Base), pin: static pin => Some(pin.Content), styled: static styled => Some(styled.Content));

    internal Option<PlotPlane> Plotted => SwitchPartially(@default: static _ => Option<PlotPlane>.None, plot: static plot => Some(plot.Plane));

    internal Func<Option<Vector2>, Option<IO<Unit>>> Hovering => SwitchPartially(@default: static _ => Unhovered, plot: static plot => plot.Hovered);

    internal bool Hovers => Hovering(None).IsSome;

    internal Option<Lazy> Windowed => SwitchPartially(@default: static _ => Option<Lazy>.None, lazy: static lazy => Some(lazy));

    internal Option<Pin> Pinned => SwitchPartially(@default: static node => node.Inner.Bind(static inner => inner.Pinned), pin: static pin => Some(pin));

    internal Option<Figure> Traced => SwitchPartially(@default: static _ => Option<Figure>.None, mark: static mark => Some(mark.Figure));

    internal Option<GlyphSource> Glyphed => SwitchPartially(@default: static _ => Option<GlyphSource>.None, glyph: static glyph => Some(glyph.Art));

    internal Option<Styled> Restyled => SwitchPartially(@default: static _ => Option<Styled>.None, styled: static styled => Some(styled));

    internal float Alpha => Restyled.Map(static styled => (float)styled.Opacity).IfNone(1f);

    internal abstract Painting Painting { get; }

    internal bool Clips(Node child) => SwitchPartially(child, @default: static (_, _) => false, scroll: static (_, _) => true, plot: static (held, _) => held.Pinned.IsNone);

    internal Node Conditioned(Condition condition) =>
        Restyled.Match(Some: styled => styled with { Content = styled.Content.Conditioned(condition) }, None: () => this with { Conditions = Conditions.TryAdd(condition) });

    public sealed record Box : Node {
        internal override Painting Painting => Painting.Frame;
    }

    public sealed record Flex(PhysicalAxis Axis, Seq<Node> Children) : Node {
        public bool Baseline { get; init; }

        internal override Painting Painting => Painting.Frame;
    }

    public sealed record Grid(Tracks Columns, Seq<Seq<Node>> Rows) : Node {
        internal override Painting Painting => Painting.Frame;
    }

    public sealed record Lazy(PhysicalAxis Axis, Tracks Across, float Extent, Seq<Key> Items) : Node {
        public HashMap<Key, Node> Realized { get; init; }

        internal override Painting Painting => Painting.Frame;
    }

    public sealed record Overlay(Node Base, Seq<Node> Above) : Node {
        internal override Painting Painting => Painting.Frame;
    }

    public sealed record Choice(PhysicalAxis Axis, Seq<Node> Alternatives) : Node {
        internal override Painting Painting => Painting.Frame;
    }

    public sealed record Scroll(PhysicalAxis Axis, Node Content) : Node {
        internal override Painting Painting => Painting.Frame;
    }

    public sealed record Plot(PlotPlane Plane, Seq<Node> Children) : Node {
        public Seq<Pin> Labels { get; init; }
        public Func<Option<Vector2>, Option<IO<Unit>>> Hovered { get; init; } = Unhovered;

        internal override Painting Painting => Painting.Frame;
    }

    public sealed record Pin(Vector2 At, Node Content) : Node {
        public Option<Vector2> Span { get; init; }

        internal override Painting Painting => Painting.Frame;
    }

    public sealed record Mark(Figure Figure) : Node {
        public Option<Series> Series { get; init; }

        internal override Painting Painting => Painting.Figure;
    }

    public sealed record Text(Seq<Span> Spans) : Node {
        public string Content => string.Concat(Spans.Map(static span => span.Text));
        public Option<int> MaxLines { get; init; }
        public TextAlignment Alignment { get; init; } = TextAlignment.Auto;

        internal override Painting Painting => Painting.Ink;
    }

    public sealed record Glyph(GlyphSource Art) : Node {
        internal override Painting Painting => Painting.Ink;
    }

    public sealed record Image(SKImage Source) : Node {
        public ImageFit Fit { get; init; } = ImageFit.Stretch;
        public float Density { get; init; } = 1f;
        public ImageRendering Rendering { get; init; } = ImageRendering.Auto;
        public Option<Series> Series { get; init; }

        internal override Painting Painting => Painting.Figure;
    }

    public sealed record Slot(SlotKind Kind) : Node {
        internal override Painting Painting => Painting.Hosted;
    }

    public sealed record Styled(Node Content) : Node {
        public StyleSheet Sheet { get; init; } = StyleSheet.Empty;
        public Fraction Opacity { get; init; } = Fraction.MaxValue;

        internal override Painting Painting => Painting.Frame;
    }
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record Invalid(string Message) : Expected(Message, 0), IValidationError<Invalid> {
    public static Invalid Create(string message) => new(message);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EditExtensions {
    extension<TValue>(K<Edit, TValue> edit) {
        public Edit<TValue> As() => (Edit<TValue>)edit;
    }
}
