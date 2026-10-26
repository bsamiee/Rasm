using System.Globalization;
using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<Invalid>]
public sealed partial class ImageRegion {
    public static ImageRegion Whole { get; } = Create(Fraction.MinValue, Fraction.MinValue, Fraction.MaxValue, Fraction.MaxValue);

    public Fraction Left { get; }
    public Fraction Top { get; }
    public Fraction Right { get; }
    public Fraction Bottom { get; }

    public Vector2 At(Vector2 anchor) => Vector2.Lerp(new Vector2((float)Left, (float)Top), new Vector2((float)Right, (float)Bottom), anchor);

    public Option<ImageRegion> Moved(Vector2 anchor, Vector2 to, Option<float> aspect) {
        Vector2 at = Vector2.Clamp(to, Vector2.Zero, Vector2.One);
        SKRect start = new((float)Left, (float)Top, (float)Right, (float)Bottom);
        SKRect moved = anchor == new Vector2(0.5f)
            ? SKRect.Create(float.Clamp(at.X - (start.Width / 2f), 0f, 1f - start.Width), float.Clamp(at.Y - (start.Height / 2f), 0f, 1f - start.Height), start.Width, start.Height)
            : new SKRect(anchor.X is 0f ? at.X : start.Left, anchor.Y is 0f ? at.Y : start.Top, anchor.X is 1f ? at.X : start.Right, anchor.Y is 1f ? at.Y : start.Bottom);
        SKRect held = aspect.Match(
            Some: wide => anchor switch {
                { X: not 0.5f } => anchor.Y is 0f ? moved with { Top = moved.Bottom - (moved.Width / wide) } : moved with { Bottom = moved.Top + (moved.Width / wide) },
                { Y: not 0.5f } => moved with { Right = moved.Left + (moved.Height * wide) },
                _ => moved,
            },
            None: () => moved);
        return Fraction.TryCreate(held.Left, out Fraction? left) && Fraction.TryCreate(held.Top, out Fraction? top) && Fraction.TryCreate(held.Right, out Fraction? right)
            && Fraction.TryCreate(held.Bottom, out Fraction? bottom) && TryCreate(left, top, right, bottom, out ImageRegion? region)
            ? Some(region)
            : None;
    }

    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref Fraction left, ref Fraction top, ref Fraction right, ref Fraction bottom) =>
        validationError = left < right && top < bottom
            ? null
            : new Invalid(string.Create(CultureInfo.InvariantCulture, $"image region ({(float)left}, {(float)top}, {(float)right}, {(float)bottom}) is empty"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ImageTool {
    private static readonly Seq<Vector2> Anchors = toSeq(from y in Seq(0f, 0.5f, 1f) from x in Seq(0f, 0.5f, 1f) select new Vector2(x, y));
    private static readonly Key Picked = new Key.Named("pick");

    internal abstract Node Drawn(PlotPlane view, Vector2 extent);

    private static Option<int> Index(Key key) => key is Key.Ordinal ordinal ? Some((int)ordinal.Index) : None;

    private static string Spoken(Vector2 at) => string.Create(CultureInfo.InvariantCulture, $"{at.X:0}, {at.Y:0}");

    public sealed record Region(ImageRegion Value, Option<float> Aspect, Func<Edit<ImageRegion>, IO<Unit>> Edited) : ImageTool {
        internal override Node Drawn(PlotPlane view, Vector2 extent) =>
            new Handles<ImageRegion>(Value, new Points<ImageRegion>(
                    held => Anchors.Map((anchor, index) => ((Key)new Key.Ordinal((uint)index), held.At(anchor) * extent)),
                    (held, targets, _) => (targets.Fold(held, (area, target) => (from index in Index(target.Key)
                                                                                 from anchor in Anchors.At(index)
                                                                                 from moved in area.Moved(anchor, target.To / extent, Aspect.Map(wide => wide * extent.Y / extent.X))
                                                                                 select moved).IfNone(area)), targets.Map(static target => target.Key)),
                    Spoken),
                Edited).Drawn(view, Ringed(extent).ToSeq());

        private Option<Node> Ringed(Vector2 extent) =>
            AxisRange.TryCreate((float)Value.Left * extent.X, (float)Value.Right * extent.X, out AxisRange? across)
            && AxisRange.TryCreate((float)Value.Top * extent.Y, (float)Value.Bottom * extent.Y, out AxisRange? down)
                ? Some<Node>(new Node.Mark(new Figure.Region(Some(across), Some(down))) { Role = Role.Ring })
                : None;
    }

    public sealed record Pick(Option<Vector2> At, Func<Edit<Option<Vector2>>, IO<Unit>> Edited) : ImageTool {
        internal override Node Drawn(PlotPlane view, Vector2 extent) =>
            new Handles<Option<Vector2>>(At, new Points<Option<Vector2>>(
                    static at => at.Map(static held => Seq((Picked, held))).IfNone([]),
                    static (at, targets, _) => (targets.Last.Map(static target => target.To) | at, targets.Map(static target => target.Key)),
                    Spoken) {
                Placed = Some<Func<Option<Vector2>, Vector2, Option<Key>, Option<(Option<Vector2> Value, Key Key)>>>(static (_, at, _) => Some((Some(at), Picked))),
            },
                Edited) { GroundCursor = CursorShape.Crosshair }.Drawn(view, []);
    }
}

public sealed record ImagePlot(Content<SKImage> Image, SKSizeI Pixels, PlotPlane View, Func<Edit<PlotPlane>, IO<Unit>> Navigated) {
    public SKSizeI Pixels { get; } = Pixels is { Width: > 0, Height: > 0 } ? Pixels : throw new ArgumentOutOfRangeException(nameof(Pixels), Pixels, "an image plot spans a positive pixel size");

    public Option<string> Caption { get; init; }
    public Seq<Node> Overlays { get; init; }
    public Option<ImageTool> Tool { get; init; }
    public Option<Func<Option<Vector2>, IO<Unit>>> Hovered { get; init; }

    public PlotPlane Fit => new PlotPlane.Cartesian(new Axis.Linear(AxisRange.Create(0f, Pixels.Width)), new Axis.Linear(AxisRange.Create(Pixels.Height, 0f)));

    public Node Drawn =>
        new Node.Overlay(
            new Node.Plot(View, Image.Held.Map(image => (Node)Covering(new Node.Image(image) { Rendering = ImageRendering.Pixelated })).ToSeq() + Overlays + Tool.Map(tool => tool.Drawn(View, Extent)).ToSeq()) {
                Part = new Part.Viewport(Fit, Navigated),
                Width = Sizing.Grow.Unit,
                Aspect = Factor.Create((float)Pixels.Width / Pixels.Height),
                Conditions = ImageTile.Conditioned(Image),
                Hovered = Hovered,
                Accessible = new Accessible { Role = AccessRole.Image, Label = Caption },
            },
            Caption.Map(static caption => (Node)new Node.Text([caption]) { Role = Role.OverlayCaption }).ToSeq()) {
            Width = Sizing.Grow.Unit,
        };

    public Node.Pin Covering(Node content) => new(Vector2.Zero, content) { Span = Extent };

    public Option<PlotPlane> ActualSize(float across) =>
        from axis in View.Dimension(PhysicalAxis.Horizontal)
        from plane in View.Scaled(View.Dimensions.Fold(Vector2.Zero, static (center, held) => held.Along.With(center, (held.Axis.Domain.From + held.Axis.Domain.To) / 2f)), across / MathF.Abs(axis.Domain.Span))
        select plane;

    public Option<string> Zoom(float across, CultureInfo culture) => View.Dimension(PhysicalAxis.Horizontal).Map(axis => (across / MathF.Abs(axis.Domain.Span)).ToString("P0", culture));

    internal Option<IO<Unit>> Navigation(PlotPlane plane) => plane == View ? None : Some(Navigated(new Edit<PlotPlane>.Commit(plane)));

    private Vector2 Extent => new(Pixels.Width, Pixels.Height);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ImageOperation {
    public static readonly ImageOperation Fit = new("fit", static (view, _) => view.Navigation(view.Fit));
    public static readonly ImageOperation ActualSize = new("actual-size", static (view, across) => view.ActualSize(across).Bind(view.Navigation));

    [UseDelegateFromConstructor]
    public partial Option<IO<Unit>> Applied(ImagePlot view, float across);

    public Command<TGate, TInvoke>.Run Commanded<TGate, TInvoke>(string caption, Func<ImageOperation, TGate> enabled, Func<ImageOperation, TInvoke> invoke) where TGate : notnull where TInvoke : notnull =>
        new(new CommandFace(Key, caption, None, None), Some(enabled(this)), invoke(this));
}

public sealed record ImageTile(Content<SKImage> Image) {
    public Node Drawn =>
        new Node.Overlay(
            new Node.Box { Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit },
            Image.Held.Map(static image => (Node)new Node.Image(image) { Fit = ImageFit.Contain, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }).ToSeq()) {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
            Conditions = Conditioned(Image),
            Accessible = new Accessible {
                Role = AccessRole.Image,
                Help = Image.Switch(
                    pending: static _ => Option<string>.None,
                    ready: static _ => Option<string>.None,
                    empty: static empty => Some(empty.Reason),
                    failed: static failed => failed.Error.IsExpected ? Some(failed.Error.Message) : None),
            },
        };

    internal static LanguageExt.HashSet<Condition> Conditioned(Content<SKImage> image) =>
        image.Map<LanguageExt.HashSet<Condition>>(pending: [Condition.Busy], ready: [], empty: [], failed: [Condition.Faulted]);
}
