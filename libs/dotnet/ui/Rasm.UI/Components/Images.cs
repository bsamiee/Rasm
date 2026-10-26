using System.Globalization;
using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<Invalid>]
public sealed partial class ImageRegion {
    public static ImageRegion Whole { get; } = Create(new SKRect(0f, 0f, 1f, 1f));

    public SKRect Edges { get; }

    public Vector2 At(Vector2 anchor) => Vector2.Lerp(new Vector2(Edges.Left, Edges.Top), new Vector2(Edges.Right, Edges.Bottom), anchor);

    public Option<ImageRegion> Moved(Vector2 anchor, Vector2 to, Option<float> aspect) {
        Vector2 at = Vector2.Clamp(to, Vector2.Zero, Vector2.One);
        SKRect moved = anchor == new Vector2(0.5f)
            ? SKRect.Create(float.Clamp(at.X - (Edges.Width / 2f), 0f, 1f - Edges.Width), float.Clamp(at.Y - (Edges.Height / 2f), 0f, 1f - Edges.Height), Edges.Width, Edges.Height)
            : new SKRect(anchor.X is 0f ? at.X : Edges.Left, anchor.Y is 0f ? at.Y : Edges.Top, anchor.X is 1f ? at.X : Edges.Right, anchor.Y is 1f ? at.Y : Edges.Bottom);
        SKRect held = aspect.Match(
            Some: wide => anchor switch {
                { X: not 0.5f } => anchor.Y is 0f ? moved with { Top = moved.Bottom - (moved.Width / wide) } : moved with { Bottom = moved.Top + (moved.Width / wide) },
                { Y: not 0.5f } => moved with { Right = moved.Left + (moved.Height * wide) },
                _ => moved,
            },
            None: () => moved);
        return Validated(held.Standardized).ToOption();
    }

    public static Fin<ImageRegion> Validated(SKRect edges) => TryCreate(edges, out ImageRegion? region, out Invalid? fault) ? region : fault;

    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref SKRect edges) =>
        validationError = edges is { Left: >= 0f, Top: >= 0f, Right: <= 1f, Bottom: <= 1f } && edges.Left < edges.Right && edges.Top < edges.Bottom
            ? null
            : new Invalid(string.Create(CultureInfo.InvariantCulture, $"image region ({edges.Left}, {edges.Top}, {edges.Right}, {edges.Bottom}) is empty or leaves the unit square"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ImageTool {
    private static readonly Seq<Vector2> Anchors = toSeq(from y in Seq(0f, 0.5f, 1f) from x in Seq(0f, 0.5f, 1f) select new Vector2(x, y));
    private static readonly Key Picked = new Key.Named("pick");

    internal abstract Node Drawn(PlotPlane view, Vector2 extent);

    public sealed record Region(ImageRegion Value, Option<float> Aspect, Func<Edit<ImageRegion>, IO<Unit>> Edited) : ImageTool {
        internal override Node Drawn(PlotPlane view, Vector2 extent) =>
            new Handles<ImageRegion>(Value, Points<ImageRegion>.Fixed(Anchors.Map(anchor => Lens<ImageRegion, Vector2>.New(
                    held => held.At(anchor) * extent,
                    to => held => held.Moved(anchor, to / extent, Aspect.Map(wide => wide * extent.Y / extent.X)).IfNone(held)))) with { Placed = static (_, _, _) => None, },
                Edited).Drawn(view, Ringed(extent).ToSeq());

        private Option<Node> Ringed(Vector2 extent) =>
            (AxisRange.Validated(Value.Edges.Left * extent.X, Value.Edges.Right * extent.X), AxisRange.Validated(Value.Edges.Top * extent.Y, Value.Edges.Bottom * extent.Y))
                .Apply(static (across, down) => (Node)new Node.Mark(new Figure.Region(Some(across), Some(down))) { Role = Role.Ring }).As()
                .ToOption();
    }

    public sealed record Pick(Option<Vector2> At, Func<Edit<Option<Vector2>>, IO<Unit>> Edited) : ImageTool {
        internal override Node Drawn(PlotPlane view, Vector2 extent) =>
            new Handles<Option<Vector2>>(At, new Points<Option<Vector2>>(
                    static at => at.Map(static held => Seq((Picked, held))).IfNone([]),
                    static (at, targets, _) => (targets.Last.Map(static target => target.To) | at, targets.Map(static target => target.Key))) {
                Placed = static (_, at, _) => Some((Some(at), Picked)),
            },
                Edited) { GroundCursor = CursorShape.Crosshair }.Drawn(view, []);
    }
}

public sealed record ImagePlot(Content<SKImage> Image, SKSizeI Pixels, PlotPlane View, Func<Edit<PlotPlane>, IO<Unit>> Navigated) {
    public Option<string> Caption { get; init; }
    public Seq<Node> Overlays { get; init; }
    public Option<ImageTool> Tool { get; init; }
    public Func<Option<Vector2>, Option<IO<Unit>>> Hovered { get; init; } = static _ => None;

    public Option<PlotPlane> Fit =>
        (AxisRange.Validated(0f, Pixels.Width), AxisRange.Validated(Pixels.Height, 0f))
            .Apply(static (across, down) => (PlotPlane)new PlotPlane.Cartesian(new Axis.Linear(across), new Axis.Linear(down))).As()
            .ToOption();

    public Node Drawn =>
        new Node.Overlay(
            new Node.Plot(View, Image.Held.Map(image => (Node)Covering(new Node.Image(image) { Rendering = ImageRendering.Pixelated })).ToSeq() + Overlays + Tool.Map(tool => tool.Drawn(View, Extent)).ToSeq()) {
                Part = Fit.Map(rest => (Part)new Part.Viewport(rest, Navigated)),
                Width = Sizing.Grow.Unit,
                Aspect = Factor.Validated((float)Pixels.Width / Pixels.Height).ToOption(),
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

    internal Option<IO<Unit>> Navigation(PlotPlane plane) => plane == View ? None : Some(Navigated(Edit.Commit(plane)));

    private Vector2 Extent => new(Pixels.Width, Pixels.Height);
}

[SmartEnum<string>]
public sealed partial class ImageOperation {
    public static readonly ImageOperation Fit = new("fit", static (view, _) => view.Fit.Bind(view.Navigation));
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
