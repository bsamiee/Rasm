using System.Globalization;
using System.Numerics;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Styling;
using Rhino.UI.Controls;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Render.UI;

// --- [TYPES] ---------------------------------------------------------------------------
internal delegate IO<Bitmap> FrameRaster(Control view, PixelFrame frame, Transfer encoding, Action<Bitmap> retain);

internal delegate IO<float> DisplayHeadroom(Control view);

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FrameZoom : IMinMaxValue<FrameZoom> {
    public static FrameZoom MinValue { get; } = new(float.BitIncrement(0f));
    public static FrameZoom MaxValue { get; } = new(float.MaxValue);
    public static FrameZoom Actual { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct OverlayOpacity : IMinMaxValue<OverlayOpacity> {
    public static OverlayOpacity MinValue { get; } = new(0f);
    public static OverlayOpacity MaxValue { get; } = new(1f);
    public static OverlayOpacity Half { get; } = new(0.5f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ComplexValueObject]
[ObjectFactory<string>]
public sealed partial class FrameRegion : IConvertible<string> {
    public AxisFraction Left { get; }
    public AxisFraction Top { get; }
    public AxisFraction Right { get; }
    public AxisFraction Bottom { get; }

    public static FrameRegion Whole { get; } = new(AxisFraction.MinValue, AxisFraction.MinValue, AxisFraction.MaxValue, AxisFraction.MaxValue);

    public RectangleF Edges => RectangleF.FromSides(Left, Top, Right, Bottom);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref AxisFraction left, ref AxisFraction top, ref AxisFraction right, ref AxisFraction bottom) =>
        validationError = left < right && top < bottom ? null : new ValidationError("frame left lies at or right of its right, or top at or below its bottom");

    internal static ValidationError? Validate(RectangleF edges, out FrameRegion? item) {
        item = null;
        return AxisFraction.Validate(edges.Left, CultureInfo.InvariantCulture, out AxisFraction left) is null
            && AxisFraction.Validate(edges.Top, CultureInfo.InvariantCulture, out AxisFraction top) is null
            && AxisFraction.Validate(edges.Right, CultureInfo.InvariantCulture, out AxisFraction right) is null
            && AxisFraction.Validate(edges.Bottom, CultureInfo.InvariantCulture, out AxisFraction bottom) is null
                ? Validate(left, top, right, bottom, out item)
                : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"frame edges {edges} hold a fraction outside [{AxisFraction.MinValue}, {AxisFraction.MaxValue}]"));
    }

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out FrameRegion? item) {
        item = null;
        return value is null
            ? null
            : toSeq(value.Split(','))
                .Traverse(static field => Invariant.Number<float>(field))
                .As()
                .Case switch {
                    Seq<float> and [var left, var top, var right, var bottom] => Validate(RectangleF.FromSides(left, top, right, bottom), out item),
                    _ => new ValidationError($"{value} is no four comma-separated fractions"),
                };
    }

    public string ToValue() => string.Create(CultureInfo.InvariantCulture, $"{(float)Left},{(float)Top},{(float)Right},{(float)Bottom}");
}

[Union]
public abstract partial record FrameTransform {
    public sealed record Fitted : FrameTransform;
    public sealed record Placed(FrameZoom Zoom, Vector2 Center) : FrameTransform;
}

public sealed record FrameFeed(Transfer Encoding, Option<Nits> Peak, ImageInterpolation Reduction);

public sealed record FrameSide(FrameFeed Feed, string Caption);

public sealed record FrameShown(FrameSide Side, Option<PixelFrame> Frame);

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class CompareMode {
    public static readonly CompareMode VerticalWipe = new(Some(Orientation.Vertical), Keys.Q, GlyphRole.VerticalWipe, "Vertical wipe");
    public static readonly CompareMode HorizontalWipe = new(Some(Orientation.Horizontal), Keys.W, GlyphRole.HorizontalWipe, "Horizontal wipe");
    public static readonly CompareMode SideBySide = new(Option<Orientation>.None, Keys.E, GlyphRole.SideBySide, "Side by side");

    public Option<Orientation> Rule { get; }
    public Keys Shortcut { get; }
    public GlyphRole Glyph { get; }
    public string Caption { get; }
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class RegionHandle {
    public static readonly RegionHandle TopLeft = new(new Vector2(0f, 0f), Cursors.SizeTopLeft);
    public static readonly RegionHandle Top = new(new Vector2(0.5f, 0f), Cursors.HorizontalSplit);
    public static readonly RegionHandle TopRight = new(new Vector2(1f, 0f), Cursors.SizeTopRight);
    public static readonly RegionHandle Right = new(new Vector2(1f, 0.5f), Cursors.VerticalSplit);
    public static readonly RegionHandle BottomRight = new(new Vector2(1f, 1f), Cursors.SizeBottomRight);
    public static readonly RegionHandle Bottom = new(new Vector2(0.5f, 1f), Cursors.HorizontalSplit);
    public static readonly RegionHandle BottomLeft = new(new Vector2(0f, 1f), Cursors.SizeBottomLeft);
    public static readonly RegionHandle Left = new(new Vector2(0f, 0.5f), Cursors.VerticalSplit);
    public static readonly RegionHandle Inside = new(new Vector2(0.5f, 0.5f), Cursors.Move);

    public Vector2 Anchor { get; }
    public Cursor Shape { get; }
}

public readonly record struct QuadCorners((double X, double Y) TopLeft, (double X, double Y) TopRight, (double X, double Y) BottomRight, (double X, double Y) BottomLeft) {
    public Option<FrameQuad> Quad => FrameQuad.Validate(TopLeft, TopRight, BottomRight, BottomLeft, out FrameQuad? quad) is null ? Optional(quad) : None;
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class QuadCorner {
    public static readonly QuadCorner TopLeft = new(Lens<QuadCorners, (double X, double Y)>.New(static held => held.TopLeft, static point => held => held with { TopLeft = point }));
    public static readonly QuadCorner TopRight = new(Lens<QuadCorners, (double X, double Y)>.New(static held => held.TopRight, static point => held => held with { TopRight = point }));
    public static readonly QuadCorner BottomRight = new(Lens<QuadCorners, (double X, double Y)>.New(static held => held.BottomRight, static point => held => held with { BottomRight = point }));
    public static readonly QuadCorner BottomLeft = new(Lens<QuadCorners, (double X, double Y)>.New(static held => held.BottomLeft, static point => held => held with { BottomLeft = point }));

    public Lens<QuadCorners, (double X, double Y)> Point { get; }
}

public sealed record QuadPatch(QuadCorners Corners, Vector3 Fill, Vector3 Outline);

public sealed record FrameMask(Coverage Coverage, MarkStyle Style);

public sealed record FrameSampling(IO<(FrameShown Current, Option<FrameShown> Reference, Seq<FrameMask> Masks)> Read, IO<Unit> Stop);

public sealed record FrameGuide((long Left, long Top, long Right, long Bottom) Frame, MarkStyle.Stroke Style);

[Union]
public abstract partial record FrameOverlay {
    public sealed record AreaPick(Option<PixelExtent> Aspect, Func<FrameRegion, string> Readout) : FrameOverlay;
    public sealed record GuideSet(Seq<FrameGuide> Guides) : FrameOverlay;
    public sealed record QuadCage(Func<FrameQuad, Seq<QuadPatch>> Patches, Gamut Working, OverlayOpacity Opacity) : FrameOverlay;
}

[Union]
public abstract partial record FrameEdit {
    public sealed record AreaEdited(FrameRegion Area) : FrameEdit;
    public sealed record QuadEdited(FrameQuad Quad) : FrameEdit;
    public sealed record PixelPicked((double X, double Y) Pixel) : FrameEdit;
}

[Union]
public abstract partial record FramePart {
    public sealed record Ground : FramePart;
    public sealed record Rule : FramePart;
    public sealed record Edge(RegionHandle Handle) : FramePart;
    public sealed record Flip(Orientation Axis) : FramePart;
    public sealed record Corner(QuadCorner Handle) : FramePart;
}

[Union]
public abstract partial record FrameDrag {
    public sealed record Panning(Vector2 Start) : FrameDrag;
    public sealed record Wiping : FrameDrag;
    public sealed record Sizing(RegionHandle Handle, RectangleF Start) : FrameDrag;
    public sealed record Cornering(QuadCorner Handle, QuadCorners Raw) : FrameDrag;
}

public sealed record FrameViewState(
    FrameShown Current, Option<FrameShown> Reference, Nits Target, FrameTransform Transform, bool Comparing, CompareMode Mode, AxisFraction Split, bool Swapped,
    Seq<FrameMask> Masks, Option<FrameOverlay> Overlay, FrameRegion Area, Option<FrameQuad> Quad, bool Picking, Option<FrameDrag> Drag);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class FrameView : ComponentControl<FrameViewState, FramePart, FrameEdit> {
    // --- [STATE]
    private readonly Seq<(Command Command, Func<FrameViewState, Transition<FrameViewState, FrameEdit>> Act)> bindings;
    private Cache cache = Cache.Empty;
    private Option<IDisposable> appearance;

    public FrameView(IPlugInSink sink, (FrameShown Current, Option<FrameShown> Reference, Seq<FrameMask> Masks) initial, FrameSampling sampling)
        : base(sink, Initial(initial), Some(Sampled(sampling))) {
        Compare = Titled(new CheckCommand(), "Compare");
        Modes = toSeq(CompareMode.Items).Map(static mode => (mode, Titled(new RadioCommand { Shortcut = mode.Shortcut }, mode.Caption))).Strict();
        Swap = Titled(new Command { Shortcut = Keys.Alt | Keys.W }, "Swap");
        Fit = Titled(new Command { Shortcut = Keys.Shift | Keys.Z }, "Fit");
        ActualSize = Titled(new Command { Shortcut = Keys.Alt | Keys.Shift | Keys.Z }, "Actual Size");
        FlipHorizontal = Titled(new Command(), Flipping(Orientation.Horizontal));
        FlipVertical = Titled(new Command(), Flipping(Orientation.Vertical));
        bindings = Seq<(Command Command, Func<FrameViewState, Transition<FrameViewState, FrameEdit>> Act)>(
                (Compare, static state => Viewed(state with { Comparing = !state.Comparing })),
                (Swap, static state => Viewed(state with { Swapped = !state.Swapped })),
                (Fit, static state => Viewed(state with { Transform = new FrameTransform.Fitted() })),
                (ActualSize, static state => Viewed(state.Current.Frame
                    .Map(frame => state with { Transform = new FrameTransform.Placed(FrameZoom.Actual, Centre(state.Transform, Extent(frame))) })
                    .IfNone(state))),
                (FlipHorizontal, static state => Flip(state, Orientation.Horizontal)),
                (FlipVertical, static state => Flip(state, Orientation.Vertical)))
            + Modes.Map(static pair => (Command: (Command)pair.Command, Act: Selected(pair.Mode)));
        _ = bindings.Iter(binding => binding.Command.Executed += Callbacks.Handler<EventArgs>(_ => Advance(binding.Act), Site(nameof(Command.Executed))));
        _ = Commanded(State);
    }

    public IO<Unit> Overlay(Option<FrameOverlay> overlay) => Advance(state => new(state with { Overlay = overlay, Drag = None }, None));

    public IO<Unit> Pick =>
        from held in PickCursor
        from focused in IO.lift(Focus)
        from picked in Advance(static state => new(state with { Picking = true, Drag = None }, None))
        select picked;

    protected override FrameViewState Received(FrameViewState state, Option<FrameEdit> value) =>
        value.Match(
            Some: edit => edit.Switch(
                state,
                areaEdited: static (held, area) => held with { Area = area.Area, Drag = None },
                quadEdited: static (held, quad) => held with { Quad = Some(quad.Quad), Drag = None },
                pixelPicked: static (held, _) => held),
            None: () => state);

    protected override IO<Unit> Reconcile(FrameViewState before, FrameViewState after, Interaction<FramePart> was, Interaction<FramePart> now) =>
        from commanded in IO.lift(() => Commanded(after))
        from released in when(before.Picking && !after.Picking, Unpicked).As()
        select released;

    protected override void OnLoad(EventArgs e) {
        base.OnLoad(e);
        Callbacks.Run(DisposalOps.Acquire(owned => HostTheme.In(this).Inline(_ =>
            from cursor in when(State.Picking, PickCursor).As()
            from refreshed in Advance(static state => new(state, None))
            from invalidated in IO.lift(() => Invalidate())
            select unit, Sink, owned), (_, lease) => appearance = Some(lease)), Site());
    }

    protected override void OnUnLoad(EventArgs e) =>
        Callbacks.Run(Callbacks.Each(Seq(
            Unwatched, IO.lift(() => base.OnUnLoad(e)), Advance(static state => new(state with { Picking = false, Drag = None }, None)), Emptied))
            .Uninterruptible(), Site());

    protected override void Dispose(bool disposing) =>
        Callbacks.Run(Callbacks.Each(
            (disposing
                ? Seq(Unwatched)
                : Seq<IO<Unit>>()).Add(IO.lift(() => base.Dispose(disposing)).Bind(_ =>
                    when(disposing, IO.lift(() => {
                        Cache held = cache;
                        cache = Cache.Empty;
                        return held;
                    }).Bind(static held => DisposalOps.Release(held.Owned))).As())))
            .Uninterruptible(), Site());

    private static FrameViewState Initial((FrameShown Current, Option<FrameShown> Reference, Seq<FrameMask> Masks) initial) =>
        new(initial.Current, initial.Reference, Nits.ReferenceWhite, new FrameTransform.Fitted(), Comparing: initial.Reference.IsSome, CompareMode.VerticalWipe, AxisFraction.Half, Swapped: false,
            initial.Masks, None, FrameRegion.Whole, None, Picking: false, None);

    private static Func<Control, IO<Func<FrameViewState, FrameViewState>>> Sampled(FrameSampling sampling) =>
        view =>
            from sampled in sampling.Read
            from target in Target(view)
            select fun((FrameViewState held) => held with {
                Current = sampled.Current,
                Reference = sampled.Reference,
                Comparing = sampled.Reference.IsSome && (held.Comparing || sampled.Reference.Map(static shown => shown.Side) != held.Reference.Map(static shown => shown.Side)),
                Target = target,
                Masks = sampled.Masks,
                Transform = sampled.Current.Frame.Map(static shown => shown.Size) == held.Current.Frame.Map(static shown => shown.Size) ? held.Transform : new FrameTransform.Fitted(),
            });

    private IO<Unit> Unpicked =>
        IO.lift(() => {
            Option<(Option<Cursor> Shape, Seq<(Cursor Shape, IDisposable Lease)> Owned)> held = cache.Pick;
            cache = cache with { Pick = held.Map(static pick => (Shape: Option<Cursor>.None, pick.Owned)) };
            return held;
        }).Bind(held => (from pick in held from shape in pick.Shape select pick.Owned).Match(
            Some: owned => IO.lift(() => {
                Cursor selected = Cursor;
                if (owned.Exists(pick => ReferenceEquals(selected, pick.Shape))) Cursor = Cursors.Default;
                cache = cache with { Pick = None };
            }).Bind(_ => DisposalOps.Release(owned.Map(static pick => pick.Lease))),
            None: static () => IO.pure(unit))).Uninterruptible();

    private IO<Unit> PickCursor =>
        DisposalOps.Replace(Icons.PickCursor, (shape, lease) => {
            Option<(Option<Cursor> Shape, Seq<(Cursor Shape, IDisposable Lease)> Owned)> prior = cache.Pick;
            Cursor selected = Cursor;
            Seq<(Cursor Shape, IDisposable Lease)> reserved = prior.ToSeq().Bind(static pick => pick.Owned).Add((shape, lease));
            cache = cache with { Pick = Some((prior.Map(static pick => pick.Shape).IfNone(Some(shape)), reserved)) };
            return DisposalOps.OnFailure(
                IO.lift(() => {
                    if (prior.Exists(pick => pick.Shape.IsNone || pick.Owned.Exists(owned => ReferenceEquals(selected, owned.Shape)))) Cursor = shape;
                }),
                IO.lift(() => { Cursor = selected; })
                    .Bind(_ => IO.lift(() => { cache = cache with { Pick = prior }; }))
                    .Bind(_ => DisposalOps.Release(lease)))
                .Bind(_ => IO.lift(() => { cache = cache with { Pick = Some((Some(shape), Seq((shape, lease)))) }; }))
                .Bind(_ => DisposalOps.Release(prior.ToSeq().Bind(static pick => pick.Owned).Map(static owned => owned.Lease)));
        }).Map(static _ => unit);

    private IO<Unit> Unwatched =>
        IO.lift(() => {
            Option<IDisposable> held = appearance;
            appearance = None;
            return held;
        }).Bind(static held => DisposalOps.Release(held.ToSeq()));

    private IO<Unit> Emptied =>
        Callbacks.Each(Seq(Unpicked, IO.lift(() => {
            Cache held = cache;
            cache = Cache.Empty with { Pick = held.Pick };
            return held with { Pick = None };
        }).Bind(static held => DisposalOps.Release(held.Owned))))
            .Map(unit).Uninterruptible();

    private IO<Bitmap> Cached<TKey>(Lens<Cache, HashMap<TKey, Bitmap>> slot, TKey key, Func<Action<Bitmap>, IO<Bitmap>> build) =>
        IO.lift(() => slot.Get(cache).Find(key)).Bind(found => found.Match(
            Some: static image => IO.pure(image),
            None: () => build(image => cache = slot.Update(held => held.Add(key, image), cache))));

    private IO<Unit> Retained<TKey>(Lens<Cache, HashMap<TKey, Bitmap>> slot, LanguageExt.HashSet<TKey> keep) =>
        (from gone in IO.lift(() => {
            HashMap<TKey, Bitmap> gone = slot.Get(cache).Filter((key, _) => !keep.Contains(key));
            cache = slot.Update(held => held.Filter((key, _) => keep.Contains(key)), cache);
            return gone;
        })
         from released in DisposalOps.Release(toSeq(gone.Values))
         select released).Uninterruptible();

    private sealed record Cache(
        HashMap<(PixelFrame Frame, FrameFeed Feed, Nits Target), Bitmap> Frames,
        HashMap<(PixelFrame Frame, FrameMask Mask, Color Color), Bitmap> Tints,
        Option<(Option<Cursor> Shape, Seq<(Cursor Shape, IDisposable Lease)> Owned)> Pick) {
        public static Cache Empty { get; } = new(HashMap<(PixelFrame Frame, FrameFeed Feed, Nits Target), Bitmap>(), HashMap<(PixelFrame Frame, FrameMask Mask, Color Color), Bitmap>(), None);

        public static Lens<Cache, HashMap<(PixelFrame Frame, FrameFeed Feed, Nits Target), Bitmap>> FrameSlot { get; } =
            Lens<Cache, HashMap<(PixelFrame Frame, FrameFeed Feed, Nits Target), Bitmap>>.New(static held => held.Frames, static frames => held => held with { Frames = frames });

        public static Lens<Cache, HashMap<(PixelFrame Frame, FrameMask Mask, Color Color), Bitmap>> TintSlot { get; } =
            Lens<Cache, HashMap<(PixelFrame Frame, FrameMask Mask, Color Color), Bitmap>>.New(static held => held.Tints, static tints => held => held with { Tints = tints });

        public Seq<IDisposable> Owned =>
            toSeq(Frames.Values).Map<IDisposable>(static image => image) + toSeq(Tints.Values).Map<IDisposable>(static image => image)
            + Pick.ToSeq().Bind(static pick => pick.Owned).Map(static owned => owned.Lease);
    }

    private readonly record struct Placement(RectangleF Pane, float Scale, Vector2 Extent, System.Drawing.Point Origin, float Fit, float Zoom, Vector2 Center) {
        private Vector2 Middle => new(Pane.MiddleX, Pane.MiddleY);

        public RectangleF Frame => Mapped(new RectangleF(0f, 0f, Extent.X, Extent.Y));
        public RectangleF Seen => RectangleF.Intersect(Frame, Pane);
        public RectangleF Visible => (Point(Pane.TopLeft), Point(Pane.BottomRight)) switch {
            var (from, to) => RectangleF.FromSides(from.X, from.Y, to.X, to.Y),
        };

        public PointF View(Vector2 point) => (((point - Center) * (Zoom / Scale)) + Middle) switch {
            var view => new PointF(view.X, view.Y),
        };

        public PointF View((double X, double Y) point) => View(new Vector2((float)(point.X - Origin.X), (float)(point.Y - Origin.Y)));

        public Vector2 Point(PointF view) => ((new Vector2(view.X, view.Y) - Middle) * (Scale / Zoom)) + Center;

        public Vector2 Fraction(PointF view) => Vector2.Clamp(Point(view) / Extent, Vector2.Zero, Vector2.One);

        public Vector2 Holding(PointF view, float zoom) => Point(view) - ((new Vector2(view.X, view.Y) - Middle) * (Scale / zoom));

        public RectangleF Mapped(RectangleF frame) => (View(new Vector2(frame.Left, frame.Top)), View(new Vector2(frame.Right, frame.Bottom))) switch {
            var (from, to) => RectangleF.FromSides(from.X, from.Y, to.X, to.Y),
        };

        public RectangleF Mapped((long Left, long Top, long Right, long Bottom) frame) =>
            (View((frame.Left, frame.Top)), View((frame.Right, frame.Bottom))) switch {
                var (from, to) => RectangleF.FromSides(from.X, from.Y, to.X, to.Y),
            };

        public RectangleF Region(RectangleF fractions) =>
            Mapped(new RectangleF(fractions.X * Extent.X, fractions.Y * Extent.Y, fractions.Width * Extent.X, fractions.Height * Extent.Y));

        public RectangleF Fitted(PixelExtent extent) => Frame switch {
            var frame => float.Min(frame.Width / extent.Width, frame.Height / extent.Height) switch {
                var fit => RectangleF.FromCenter(frame.Center, new SizeF(extent.Width * fit, extent.Height * fit)),
            },
        };
    }

    // --- [COMMANDS]
    public CheckCommand Compare { get; }
    public Seq<(CompareMode Mode, RadioCommand Command)> Modes { get; }
    public Command Swap { get; }
    public Command Fit { get; }
    public Command ActualSize { get; }
    public Command FlipHorizontal { get; }
    public Command FlipVertical { get; }

    private Unit Commanded(FrameViewState state) {
        Compare.Checked = state.Comparing;
        _ = Modes.Iter(pair => pair.Command.Checked = pair.Mode == state.Mode);
        Swap.Enabled = state.Reference.IsSome;
        FlipHorizontal.Enabled = Caged(state).IsSome;
        FlipVertical.Enabled = Caged(state).IsSome;
        return unit;
    }

    private static TCommand Titled<TCommand>(TCommand command, string english) where TCommand : Command {
        string caption = RowText.Localize(english).Local;
        command.MenuText = caption;
        command.ToolTip = caption;
        return command;
    }

    private static string Flipping(Orientation axis) => axis is Orientation.Horizontal ? "Flip Horizontal" : "Flip Vertical";

    private static Func<FrameViewState, Transition<FrameViewState, FrameEdit>> Selected(CompareMode mode) =>
        state => Viewed(state with { Comparing = !(state.Comparing && state.Mode == mode), Mode = mode });

    private static Transition<FrameViewState, FrameEdit> Flip(FrameViewState state, Orientation axis) =>
        state.Quad.Map(quad => axis is Orientation.Horizontal ? quad.FlippedHorizontal : quad.FlippedVertical).Match(
            Some: flipped => new Transition<FrameViewState, FrameEdit>(
                state with { Quad = Some(flipped) }, Some<Edit<FrameEdit>>(new Edit<FrameEdit>.Commit(new FrameEdit.QuadEdited(flipped)))),
            None: () => Viewed(state));

    // --- [READS]
    public IO<Option<FrameZoom>> Zoom =>
        IO.lift(() => Placed(State, Size, ParentWindow.LogicalPixelSize).Bind(static at => FrameZoom.Validated(at.Zoom).ToOption()));

    public IO<Option<(double X, double Y)>> Hovered =>
        IO.lift(() =>
            from pointer in Pointer
            from at in At(State, Size, ParentWindow.LogicalPixelSize, pointer)
            from pixel in Pixel(at, pointer)
            select pixel);

    public IO<Option<RectangleF>> VisibleFrame => IO.lift(() => Placed(State, Size, ParentWindow.LogicalPixelSize).Map(static at => at.Visible));

    private static Option<(double X, double Y)> Pixel(Placement at, PointF view) =>
        at.Point(view) switch {
            var point when point.X >= 0f && point.Y >= 0f && point.X < at.Extent.X && point.Y < at.Extent.Y =>
                Some((at.Origin.X + (double)MathF.Floor(point.X) + 0.5d, at.Origin.Y + (double)MathF.Floor(point.Y) + 0.5d)),
            _ => None,
        };

    private static Vector2 Extent(PixelFrame frame) => new(frame.Size.Width, frame.Size.Height);

    private static Vector2 Centre(FrameTransform transform, Vector2 extent) =>
        transform.Switch(extent, fitted: static (held, _) => held / 2f, placed: static (_, placed) => placed.Center);

    private static Option<Placement> Placed(FrameViewState state, RectangleF pane, float scale) =>
        from frame in state.Current.Frame
        let extent = Extent(frame)
        let fit = float.Min(pane.Width * scale / extent.X, pane.Height * scale / extent.Y)
        select new Placement(pane, scale, extent, frame.Origin, fit, state.Transform.Switch(fit, fitted: static (held, _) => held, placed: static (_, placed) => (float)placed.Zoom), Centre(state.Transform, extent));

    private static Option<Placement> Placed(FrameViewState state, SizeF size, float scale) =>
        Panes(state, size).Head.Bind(pane => Placed(state, pane, scale));

    private static Option<Placement> At(FrameViewState state, SizeF size, float scale, PointF point) =>
        Panes(state, size) switch {
            var panes => (panes.Find(pane => pane.Contains(point)) | panes.Head).Bind(pane => Placed(state, pane, scale)),
        };

    private static Seq<RectangleF> Panes(FrameViewState state, SizeF size) =>
        Compared(state).IsSome && state.Mode.Rule.IsNone
            ? Seq(new RectangleF(0f, 0f, size.Width / 2f, size.Height), new RectangleF(size.Width / 2f, 0f, size.Width / 2f, size.Height))
            : Seq(new RectangleF(size));

    private static Option<(FrameShown Left, FrameShown Right)> Compared(FrameViewState state) =>
        state.Comparing && !state.Picking ? state.Reference.Map(reference => state.Swapped ? (state.Current, reference) : (reference, state.Current)) : None;

    private static Option<(Orientation Along, float Position, RectangleF Bounds, FrameShown Left, FrameShown Right)> Divided(FrameViewState state, SizeF size, float scale) =>
        from pair in Compared(state)
        from at in Placed(state, size, scale)
        select state.Mode.Rule.Match(
            Some: rule => (rule, Ruled(at, rule, state.Split), at.Seen, pair.Left, pair.Right),
            None: () => (Orientation.Vertical, at.Pane.Right, at.Seen, pair.Left, pair.Right));

    private static float Ruled(Placement at, Orientation rule, AxisFraction split) =>
        at.View(at.Extent * (float)split) switch {
            var view => rule is Orientation.Vertical ? view.X : view.Y,
        };

    private static Option<FrameOverlay> Overlaid(FrameViewState state) =>
        !state.Picking && !state.Comparing ? state.Overlay : None;

    private static Option<FrameOverlay.AreaPick> Picked(FrameViewState state) =>
        Overlaid(state).Bind(static overlay => overlay.Switch<Option<FrameOverlay.AreaPick>>(
            areaPick: static pick => pick, guideSet: static _ => None, quadCage: static _ => None));

    private static Seq<FrameGuide> Guides(FrameViewState state) =>
        Overlaid(state).ToSeq().Bind(static overlay => overlay.Switch(
            areaPick: static _ => [], guideSet: static set => set.Guides, quadCage: static _ => []));

    private static Option<(FrameOverlay.QuadCage Cage, FrameQuad Quad)> Caged(FrameViewState state) =>
        from cage in Overlaid(state).Bind(static overlay => overlay.Switch<Option<FrameOverlay.QuadCage>>(
            areaPick: static _ => None, guideSet: static _ => None, quadCage: static cage => cage))
        from quad in state.Quad
        select (cage, quad);

    // --- [TRANSITIONS]
    private const float Notch = 6f / 5f;
    private const float Nearest = 1f / 16f;
    private const float Farthest = 3f;
    private const float MinimumSide = 5f;
    private const float Turn = 0.01f;
    private const float Spread = 1.05f;

    protected override Seq<PlotMark<FramePart>> Layout(FrameViewState state, SizeF size, float scale) =>
        new PlotMark<FramePart>(new MarkShape.Band(new RectangleF(size)), None, Some(new MarkPart<FramePart>(new FramePart.Ground(), 0f))).Cons(
            Placed(state, size, scale).ToSeq().Bind(at =>
                Guides(state).Map(guide => new PlotMark<FramePart>(
                    new MarkShape.Band(at.Mapped(guide.Frame)), Some<MarkStyle>(guide.Style), None))
                + Divided(state, size, scale).Filter(_ => state.Mode.Rule.IsSome).ToSeq().Map(static divided => new PlotMark<FramePart>(
                    Ruler(divided.Along, divided.Position, divided.Bounds), Some<MarkStyle>(Wipe), Some(new MarkPart<FramePart>(new FramePart.Rule(), Plots.Reach))))
                + Picked(state).ToSeq().Bind(_ => Areas(at, state.Area))
                + Caged(state).ToSeq().Bind(held => Cage(at, held.Cage, held.Quad))));

    protected override PartFacet Facet(FrameViewState state, FramePart key) =>
        key.Switch(
            (State: state, Ground: Grounded(state)),
            ground: static (held, _) => new PartFacet(PartRole.Image, held.Ground, RowText.Localize("Frame").Local, Measured(held.State), None),
            rule: static (held, _) => new PartFacet(PartRole.Image, held.Ground, RowText.Localize("Wipe").Local, Some(RowText.Localize("{0:P0}", arguments: [(float)held.State.Split]).Local), None),
            edge: static (held, edge) => new PartFacet(PartRole.Image, edge.Handle.Shape, RowText.Localize("Area").Local, Some(RowText.Localize(
                "{0:P0}, {1:P0}, {2:P0}, {3:P0}", arguments: [(float)held.State.Area.Left, (float)held.State.Area.Top, (float)held.State.Area.Right, (float)held.State.Area.Bottom]).Local), None),
            flip: static (_, flip) => new PartFacet(PartRole.Button, Cursors.Pointer, RowText.Localize(Flipping(flip.Axis)).Local, None, None),
            corner: static (held, corner) => new PartFacet(PartRole.Image, Cursors.Move, RowText.Localize("Corner").Local, held.State.Quad
                .Map(quad => corner.Handle.Point.Get(QuadMapper.ToCorners(quad)))
                .Map(static point => RowText.Localize("{0:0.#}, {1:0.#}", arguments: [point.X, point.Y]).Local), None));

    protected override Option<Transition<FrameViewState, FrameEdit>> Pressed(FrameViewState state, SizeF size, float scale, Option<FramePart> part, MouseEventArgs e) =>
        At(state, size, scale, e.Location).Map(at => (state.Picking, e.Buttons) switch {
            (true, MouseButtons.Primary) => Pixel(at, e.Location).Match(
                Some: pixel => new Transition<FrameViewState, FrameEdit>(
                    state with { Picking = false }, Some<Edit<FrameEdit>>(new Edit<FrameEdit>.Commit(new FrameEdit.PixelPicked(pixel)))),
                None: () => Viewed(state)),
            (false, MouseButtons.Primary) => Gripped(state, at, part, e.Location),
            _ => Viewed(state with { Drag = Some<FrameDrag>(new FrameDrag.Panning(at.Center)) }),
        });

    protected override Transition<FrameViewState, FrameEdit> Dragged(FrameViewState state, SizeF size, float scale, Option<FramePart> part, PointF origin, PointF previous, MouseEventArgs e) =>
        (from drag in state.Drag
         from at in At(state, size, scale, origin)
         select drag.Switch(
             (State: state, At: at, Origin: origin, Previous: previous, Event: e),
             panning: static (held, pan) => Panned(held.State, held.At, pan.Start, held.Event.Location - held.Origin),
             wiping: static (held, _) => Wiped(held.State, held.At, held.Event.Location),
             sizing: static (held, sizing) => Sized(held.State, held.At, sizing, (held.At.Point(held.Event.Location) - held.At.Point(held.Origin)) / held.At.Extent),
             cornering: static (held, corner) => Cornered(held.State, held.At, corner, held.Previous, held.Event)))
        .IfNone(() => Viewed(state));

    protected override Transition<FrameViewState, FrameEdit> Released(FrameViewState state, SizeF size, float scale, Option<FramePart> part, MouseEventArgs e) =>
        state.Picking && e.Buttons is not MouseButtons.Primary && state.Drag.Exists(drag => Still(state.Transform, drag))
            ? Cancelled(state)
            : Viewed(state with { Drag = None });

    protected override Option<Transition<FrameViewState, FrameEdit>> DoubleClicked(FrameViewState state, SizeF size, float scale, Option<FramePart> part, MouseEventArgs e) =>
        part.Bind(key => key.Switch(
            state,
            ground: static (_, _) => None,
            rule: static (held, _) => Viewed(held with { Split = AxisFraction.Half, Drag = None }),
            edge: static (held, edge) => edge.Handle == RegionHandle.Inside
                ? Some(new Transition<FrameViewState, FrameEdit>(
                    held with { Area = FrameRegion.Whole, Drag = None }, Some<Edit<FrameEdit>>(new Edit<FrameEdit>.Commit(new FrameEdit.AreaEdited(FrameRegion.Whole)))))
                : None,
            flip: static (_, _) => None,
            corner: static (_, _) => None));

    protected override Option<Transition<FrameViewState, FrameEdit>> Scrolled(FrameViewState state, SizeF size, float scale, Option<FramePart> part, MouseEventArgs e) =>
        from at in At(state, size, scale, e.Location)
        select state.Drag.Bind(drag => Spreaded(state, drag, MathF.Pow(Spread, e.Delta.Height))).IfNone(() => Zoomed(state, at, e.Location, MathF.Pow(Notch, e.Delta.Height)));

    protected override Option<Transition<FrameViewState, FrameEdit>> KeyPressed(FrameViewState state, SizeF size, float scale, Option<FramePart> part, KeyEventArgs e) =>
        e.KeyData is Keys.Escape && state.Picking
            ? Some(Cancelled(state))
            : bindings.Find(binding => binding.Command.Enabled && binding.Command.Shortcut == e.KeyData).Map(binding => binding.Act(state));

    protected override Option<Transition<FrameViewState, FrameEdit>> Magnified(FrameViewState state, SizeF size, float scale, Option<FramePart> part, Option<PointF> pointer, float change) =>
        pointer.IfNone(new PointF(size.Width / 2f, size.Height / 2f)) switch {
            var anchor => At(state, size, scale, anchor).Map(at => Zoomed(state, at, anchor, 1f + change)),
        };

    protected override Option<Transition<FrameViewState, FrameEdit>> Activated(FrameViewState state, SizeF size, float scale, FramePart part) =>
        part.Switch<FrameViewState, Option<Transition<FrameViewState, FrameEdit>>>(
            state,
            ground: static (_, _) => None,
            rule: static (_, _) => None,
            edge: static (_, _) => None,
            flip: static (held, flip) => Flip(held, flip.Axis),
            corner: static (_, _) => None);

    private static Transition<FrameViewState, FrameEdit> Viewed(FrameViewState state) => new(state, None);

    private static Transition<FrameViewState, FrameEdit> Cancelled(FrameViewState state) =>
        new(state with { Picking = false, Drag = None }, Some<Edit<FrameEdit>>(new Edit<FrameEdit>.Cancel()));

    private Cursor Grounded(FrameViewState state) =>
        ((state.Picking ? cache.Pick : None).Bind(static pick => pick.Shape)
         | Compared(state).Bind(_ => state.Mode.Rule).Map(static rule => rule is Orientation.Vertical ? Cursors.VerticalSplit : Cursors.HorizontalSplit)
         | Picked(state).Map(static _ => Cursors.Crosshair))
        .IfNone(Cursors.Default);

    private static Option<string> Measured(FrameViewState state) =>
        state.Current.Frame.Map(frame => state.Transform.Switch(
            frame,
            fitted: static (shown, _) => RowText.Localize("{0} × {1}", arguments: [shown.Size.Width, shown.Size.Height]).Local,
            placed: static (shown, placed) => RowText.Localize("{0} × {1}, {2:P0}", arguments: [shown.Size.Width, shown.Size.Height, (float)placed.Zoom]).Local));

    private static Transition<FrameViewState, FrameEdit> Gripped(FrameViewState state, Placement at, Option<FramePart> part, PointF pointer) =>
        Compared(state).IsSome && state.Mode.Rule.IsSome
            ? Wiped(state with { Drag = Some<FrameDrag>(new FrameDrag.Wiping()) }, at, pointer)
            : (part.Bind(key => Claimed(state, key)) | Picked(state).Map(_ => Drawn(state, at.Fraction(pointer))))
                .IfNone(() => Viewed(state with { Drag = Some<FrameDrag>(new FrameDrag.Panning(at.Center)) }));

    private static Option<Transition<FrameViewState, FrameEdit>> Claimed(FrameViewState state, FramePart key) =>
        key.Switch(
            state,
            ground: static (_, _) => None,
            rule: static (_, _) => None,
            edge: static (held, edge) => Viewed(held with { Drag = Some<FrameDrag>(new FrameDrag.Sizing(edge.Handle, held.Area.Edges)) }),
            flip: static (held, flip) => Flip(held, flip.Axis),
            corner: static (held, corner) => held.Quad.Map(quad => Viewed(held with { Drag = Some<FrameDrag>(new FrameDrag.Cornering(corner.Handle, QuadMapper.ToCorners(quad))) })));

    private static Transition<FrameViewState, FrameEdit> Drawn(FrameViewState state, Vector2 start) =>
        Viewed(state with { Drag = Some<FrameDrag>(new FrameDrag.Sizing(RegionHandle.BottomRight, new RectangleF(start.X, start.Y, 0f, 0f))) });

    private static Transition<FrameViewState, FrameEdit> Wiped(FrameViewState state, Placement at, PointF pointer) =>
        Viewed((from rule in state.Mode.Rule
                let fraction = at.Fraction(pointer)
                from split in AxisFraction.Validated(rule is Orientation.Vertical ? fraction.X : fraction.Y).ToOption()
                select state with { Split = split })
            .IfNone(state));

    private static Transition<FrameViewState, FrameEdit> Panned(FrameViewState state, Placement at, Vector2 start, PointF moved) =>
        Viewed(Placing(at, at.Zoom, start - (new Vector2(moved.X, moved.Y) * (at.Scale / at.Zoom))).Map(transform => state with { Transform = transform }).IfNone(state));

    private static Option<FrameTransform> Placing(Placement at, float zoom, Vector2 center) =>
        FrameZoom.Validated(zoom).ToOption()
            .Map(valid => (FrameTransform)new FrameTransform.Placed(valid, Vector2.Clamp(center, Vector2.Zero, at.Extent)));

    private static Transition<FrameViewState, FrameEdit> Zoomed(FrameViewState state, Placement at, PointF pointer, float factor) =>
        float.Clamp(at.Zoom * factor, float.Min(Nearest, at.Fit), float.Max(Farthest, at.Fit)) switch {
            var zoom => Viewed(Placing(at, zoom, at.Holding(pointer, zoom)).Map(transform => state with { Transform = transform }).IfNone(state)),
        };

    private static bool Still(FrameTransform transform, FrameDrag drag) =>
        drag.Switch(
            transform,
            panning: static (held, pan) => held.Switch(pan.Start, fitted: static (_, _) => true, placed: static (start, placed) => placed.Center == start),
            wiping: static (_, _) => false,
            sizing: static (_, _) => false,
            cornering: static (_, _) => false);

    private static Transition<FrameViewState, FrameEdit> Sized(FrameViewState state, Placement at, FrameDrag.Sizing sizing, Vector2 delta) {
        RectangleF stretched = sizing.Handle == RegionHandle.Inside
            ? new RectangleF(
                sizing.Start.X + float.Clamp(delta.X, -sizing.Start.Left, 1f - sizing.Start.Right),
                sizing.Start.Y + float.Clamp(delta.Y, -sizing.Start.Top, 1f - sizing.Start.Bottom),
                sizing.Start.Width,
                sizing.Start.Height)
            : (Edges(sizing.Start.Left, sizing.Start.Right, sizing.Handle.Anchor.X, delta.X, MinimumSide * at.Scale / at.Zoom / at.Extent.X),
               Edges(sizing.Start.Top, sizing.Start.Bottom, sizing.Handle.Anchor.Y, delta.Y, MinimumSide * at.Scale / at.Zoom / at.Extent.Y)) switch {
                   var (across, down) => RectangleF.FromSides(across.Low, down.Low, across.High, down.High),
               };
        RectangleF edges = Picked(state).Bind(static pick => pick.Aspect).Match(Some: aspect => Aspected(stretched, sizing.Handle.Anchor, aspect, at.Extent), None: () => stretched);
        return (FrameRegion.Validate(edges, out FrameRegion? region) is null ? Optional(region) : None).Match(
            Some: area => new Transition<FrameViewState, FrameEdit>(state with { Area = area }, Some<Edit<FrameEdit>>(new Edit<FrameEdit>.Preview(new FrameEdit.AreaEdited(area)))),
            None: () => Viewed(state));
    }

    private static (float Low, float High) Edges(float low, float high, float anchor, float delta, float least) =>
        anchor switch {
            0f => (float.Min(float.Clamp(low + delta, 0f, 1f), high - least), high),
            1f => (low, float.Max(float.Clamp(high + delta, 0f, 1f), low + least)),
            _ => (low, high),
        };

    private static RectangleF Aspected(RectangleF edges, Vector2 anchor, PixelExtent aspect, Vector2 extent) =>
        ((float)aspect.Width / aspect.Height) switch {
            var ratio when anchor.X is not 0.5f => (edges.Width * extent.X / ratio / extent.Y) switch {
                var height => anchor.Y is 0f
                    ? RectangleF.FromSides(edges.Left, edges.Bottom - height, edges.Right, edges.Bottom)
                    : RectangleF.FromSides(edges.Left, edges.Top, edges.Right, edges.Top + height),
            },
            var ratio when anchor.Y is not 0.5f => RectangleF.FromSides(edges.Left, edges.Top, edges.Left + (edges.Height * extent.Y * ratio / extent.X), edges.Bottom),
            _ => edges,
        };

    private static Transition<FrameViewState, FrameEdit> Cornered(FrameViewState state, Placement at, FrameDrag.Cornering corner, PointF previous, MouseEventArgs e) =>
        (e.Modifiers.HasFlag(Keys.Alt), e.Modifiers.HasFlag(Keys.Shift), Scale(e.Modifiers)) switch {
            (true, true, var fine) =>
                (Math.SinCos((e.Location.X - previous.X) * at.Scale * Turn * fine), corner.Handle.Point.Get(corner.Raw)) switch {
                    var ((sin, cos), pivot) => Bent(state, corner,
                        point => (point.X - pivot.X, point.Y - pivot.Y) switch {
                            var (x, y) => (pivot.X + (x * cos) - (y * sin), pivot.Y + (x * sin) + (y * cos)),
                        }, other => other != corner.Handle),
                },
            (var every, _, var fine) => ((at.Point(e.Location) - at.Point(previous)) * fine) switch {
                var moved => Bent(state, corner, point => (point.X + moved.X, point.Y + moved.Y), other => every || other == corner.Handle),
            },
        };

    private static Option<Transition<FrameViewState, FrameEdit>> Spreaded(FrameViewState state, FrameDrag drag, float factor) =>
        drag.Switch<(FrameViewState State, float Factor), Option<Transition<FrameViewState, FrameEdit>>>(
            (state, factor),
            panning: static (_, _) => None,
            wiping: static (_, _) => None,
            sizing: static (_, _) => None,
            cornering: static (held, corner) => corner.Handle.Point.Get(corner.Raw) switch {
                var pivot => Bent(held.State, corner, point => (pivot.X + ((point.X - pivot.X) * held.Factor), pivot.Y + ((point.Y - pivot.Y) * held.Factor)), other => other != corner.Handle),
            });

    private static Transition<FrameViewState, FrameEdit> Bent(FrameViewState state, FrameDrag.Cornering drag, Func<(double X, double Y), (double X, double Y)> motion, Func<QuadCorner, bool> moves) =>
        toSeq(QuadCorner.Items).Filter(moves).Fold(drag.Raw, (raw, corner) => corner.Point.Update(motion, raw)) switch {
            var raw => raw.Quad.Match(
                Some: quad => new Transition<FrameViewState, FrameEdit>(
                    state with { Quad = Some(quad), Drag = Some<FrameDrag>(drag with { Raw = raw }) },
                    Some<Edit<FrameEdit>>(new Edit<FrameEdit>.Preview(new FrameEdit.QuadEdited(quad)))),
                None: () => Viewed(state with { Drag = Some<FrameDrag>(drag with { Raw = raw }) })),
        };

    private static Seq<PlotMark<FramePart>> Areas(Placement at, FrameRegion area) =>
        at.Region(area.Edges) switch {
            var bounds => Seq(
                    new PlotMark<FramePart>(new MarkShape.Band(bounds), Some(Outline), None),
                    new PlotMark<FramePart>(new MarkShape.Band(bounds), None, Some(new MarkPart<FramePart>(new FramePart.Edge(RegionHandle.Inside), 0f))))
                + toSeq(RegionHandle.Items)
                    .Filter(static handle => handle != RegionHandle.Inside)
                    .Map(handle => Grip(new PointF(bounds.Left + (handle.Anchor.X * bounds.Width), bounds.Top + (handle.Anchor.Y * bounds.Height)), new FramePart.Edge(handle))),
        };

    private static Seq<PlotMark<FramePart>> Cage(Placement at, FrameOverlay.QuadCage cage, FrameQuad quad) =>
        QuadMapper.ToCorners(quad) switch {
            var corners => cage.Patches(quad).Bind(patch => Patched(at, cage, patch))
                + Seq(new PlotMark<FramePart>(new MarkShape.Polygon(Outlined(at, corners)), Some(Outline), None))
                + Flips(at, corners).Map(static flip => new PlotMark<FramePart>(new MarkShape.Band(flip.Band), None, Some(new MarkPart<FramePart>(new FramePart.Flip(flip.Axis), 0f))))
                + toSeq(QuadCorner.Items).Map(corner => Grip(at.View(corner.Point.Get(corners)), new FramePart.Corner(corner))),
        };

    private static Seq<PlotMark<FramePart>> Patched(Placement at, FrameOverlay.QuadCage cage, QuadPatch patch) =>
        Outlined(at, patch.Corners) switch {
            var outline => Seq(
                new PlotMark<FramePart>(
                    new MarkShape.Polygon(outline), Some<MarkStyle>(new MarkStyle.Fill(new MarkColor.Fixed(Plots.Encoded(cage.Working, patch.Fill)), cage.Opacity)), None),
                new PlotMark<FramePart>(
                    new MarkShape.Polygon(outline), Some<MarkStyle>(new MarkStyle.Stroke(new MarkColor.Fixed(Plots.Encoded(cage.Working, patch.Outline)), 1f, 1f)), None)),
        };

    private static IReadOnlyList<PointF> Outlined(Placement at, QuadCorners corners) =>
        [.. toSeq(QuadCorner.Items).Map(corner => at.View(corner.Point.Get(corners)))];

    private static Seq<(Orientation Axis, RectangleF Band, float Degrees)> Flips(Placement at, QuadCorners corners) {
        PointF centre = toSeq(QuadCorner.Items).Fold(PointF.Empty, (sum, corner) => sum + at.View(corner.Point.Get(corners))) / QuadCorner.Items.Count;
        float glyph = IconSlot.PanelButton.Master;
        return Seq((Axis: Orientation.Horizontal, From: corners.TopLeft, To: corners.TopRight), (Axis: Orientation.Vertical, From: corners.BottomLeft, To: corners.TopLeft))
            .Map(edge => Flipped(edge.Axis, at.View(edge.From), at.View(edge.To), centre, glyph));
    }

    private static (Orientation Axis, RectangleF Band, float Degrees) Flipped(Orientation axis, PointF from, PointF to, PointF centre, float glyph) =>
        ((to - from).Normal / (to - from).Length, (from + to) / 2f) switch {
            var (normal, middle) => (
                axis,
                RectangleF.FromCenter(middle + ((PointF.DotProduct(normal, middle - centre) < 0f ? -normal : normal) * glyph), new SizeF(glyph, glyph)),
                (to - from).Angle),
        };

    private static PlotMark<FramePart> Grip(PointF centre, FramePart part) =>
        new(new MarkShape.Band(RectangleF.FromCenter(centre, new SizeF(GripSide, GripSide))), Some(Knob), Some(new MarkPart<FramePart>(part, Plots.Reach)));

    private static MarkShape.Rule Ruler(Orientation along, float position, RectangleF bounds) =>
        along is Orientation.Vertical
            ? new MarkShape.Rule(new PointF(position, bounds.Top), new PointF(position, bounds.Bottom))
            : new MarkShape.Rule(new PointF(bounds.Left, position), new PointF(bounds.Right, position));

    // --- [PAINT]
    private const float GripSide = 4f;

    private static readonly MarkStyle Outline = new MarkStyle.Stroke(new MarkColor.Themed(PaintSlot.Highlight), 1f, 1f);
    private static readonly MarkStyle Knob = new MarkStyle.Fill(new MarkColor.Themed(PaintSlot.Highlight), 1f);
    private static readonly MarkStyle.Stroke Wipe = new(new MarkColor.Fixed(Colors.White), 1f, 1f);
    private static readonly MarkStyle Letter = new MarkStyle.Fill(new MarkColor.Fixed(Colors.White), 1f);

    private static float CaptionInset => RhinoLayout.Padding(RhinoLayout.PaddingType.RhinoPanel).Top;

    public static IO<Bitmap> Raster(Control view, PixelFrame frame, Transfer encoding, Action<Bitmap> retain) =>
        IO.lift(static () => Optional(Platform.Instance.Find<FrameRaster>()))
            .Bind(found => found.Match(
                Some: create => create()(view, frame, encoding, retain),
                None: () => DisposalOps.Acquire(IO.lift(() => ImagePlots.Pixels(frame.Size, encoding.Displayed(frame))), retain)));

    protected override IO<Unit> Draw(PlotCanvas canvas, RectangleF bounds, FrameViewState state, Seq<PlotMark<FramePart>> marks, Interaction<FramePart> interaction) =>
        from well in Plots.Well(canvas, bounds)
        let layers = Layers(state, bounds.Size, canvas.Scale)
        let tints =
            from layer in layers
            where ReferenceEquals(layer.Side, state.Current)
            from frame in layer.Side.Frame.ToSeq()
            from mask in state.Masks
            select (layer.At, layer.Clip, Frame: frame, Mask: mask, Color: Plots.Resolve(canvas, mask.Style))
        from framesKept in Retained(Cache.FrameSlot, toHashSet(layers.Choose(layer => layer.Side.Frame.Map(frame => (frame, layer.Side.Side.Feed, state.Target)))))
        from tintsKept in Retained(Cache.TintSlot, toHashSet(tints.Map(static tint => (tint.Frame, tint.Mask, tint.Color))))
        from framed in layers.TraverseM(layer => Framed(canvas, layer, state.Target)).As()
        from masked in tints.TraverseM(tint => Clipped(canvas, tint.Clip, Tinted(canvas, (tint.At, tint.Frame, tint.Mask, tint.Color), state.Current.Side.Feed.Reduction))).As()
        from captioned in Captioned(canvas, state, bounds.Size)
        from painted in Plots.Paint(canvas, marks)
        from glyphs in Glyphed(canvas, state, bounds.Size)
        from readout in Readout(canvas, state, bounds.Size)
        select unit;

    private static IO<Nits> Target(Control view) =>
        IO.lift(static () => Optional(Platform.Instance.Find<DisplayHeadroom>()))
            .Bind(found => found.Match(
                Some: create => create()(view),
                None: static () => IO.pure(1f)))
            .Bind(static headroom => IO.lift(Nits.Validated(headroom * (float)Nits.ReferenceWhite)));

    private static IO<Bitmap> Mapped(Control view, FrameFeed feed, PixelFrame frame, Nits target, Action<Bitmap> retain) =>
        feed.Peak.Bind(peak => new PeakMapping(target, AxisFraction.MinValue, KneeOffset.Bt2390).Pass(Nits.ReferenceWhite, peak)).Match(
            Some: pass =>
                from light in IO.lift(() => feed.Encoding.Light(frame))
                from mapped in IO.lift(() => pass.Run(light, new Progress<int>()))
                from image in Raster(view, light, TransferCurve.Linear, retain)
                select image,
            None: () => Raster(view, frame, feed.Encoding, retain));

    private static Seq<(Placement At, FrameShown Side, RectangleF Clip)> Layers(FrameViewState state, SizeF size, float scale) =>
        Divided(state, size, scale).Match(
            Some: divided => state.Mode.Rule.IsSome
                ? Placed(state, size, scale).ToSeq().Bind(at => Seq(
                    (at, divided.Right, Portion(at, divided.Along, divided.Position, before: false)),
                    (at, divided.Left, Portion(at, divided.Along, divided.Position, before: true))))
                : from lane in Panes(state, size).Zip(Seq(divided.Left, divided.Right))
                  from at in Placed(state, lane.First, scale).ToSeq()
                  select (at, lane.Second, at.Pane),
            None: () => Placed(state, size, scale).ToSeq().Map(at => (at, state.Current, at.Pane)));

    private static RectangleF Portion(Placement at, Orientation along, float position, bool before) =>
        RectangleF.Intersect(at.Pane, along is Orientation.Vertical
            ? RectangleF.FromSides(before ? at.Frame.Left : position, at.Frame.Top, before ? position : at.Frame.Right, at.Frame.Bottom)
            : RectangleF.FromSides(at.Frame.Left, before ? at.Frame.Top : position, at.Frame.Right, before ? position : at.Frame.Bottom));

    private IO<Unit> Framed(PlotCanvas canvas, (Placement At, FrameShown Side, RectangleF Clip) layer, Nits target) =>
        layer.Side.Frame.Match(
            Some: frame => Clipped(canvas, layer.Clip, Pictured(canvas, layer.At.Fitted(frame.Size), frame, layer.Side.Side.Feed, target)),
            None: static () => IO.pure(unit));

    private IO<Unit> Pictured(PlotCanvas canvas, RectangleF bounds, PixelFrame frame, FrameFeed feed, Nits target) =>
        from board in Thumbnails.Checker(canvas, bounds, Checkerboard.Frame)
        from image in Cached(Cache.FrameSlot, (frame, feed, target), retain => Mapped(this, feed, frame, target, retain))
        from drawn in Imaged(canvas, image, bounds, frame.Size, feed.Reduction)
        select drawn;

    private IO<Unit> Tinted(PlotCanvas canvas, (Placement At, PixelFrame Frame, FrameMask Mask, Color Color) tint, ImageInterpolation reduction) =>
        from image in Cached(Cache.TintSlot, (tint.Frame, tint.Mask, tint.Color), retain => DisposalOps.Acquire(IO.lift(() => Folded(tint.Frame, tint.Mask.Coverage, tint.Color)), retain))
        from drawn in Imaged(canvas, image, tint.At.Frame, tint.Frame.Size, reduction)
        select drawn;

    private static IO<Unit> Clipped(PlotCanvas canvas, RectangleF clip, IO<Unit> draw) =>
        DisposalOps.Within(
            () => { canvas.Graphics.SetClip(clip); return unit; },
            _ => draw,
            _ => IO.lift(canvas.Graphics.ResetClip));

    private static IO<Unit> Imaged(PlotCanvas canvas, Image image, RectangleF target, PixelExtent extent, ImageInterpolation reduction) =>
        IO.lift(() => {
            canvas.Graphics.ImageInterpolation = target.Width * canvas.Scale >= extent.Width ? ImageInterpolation.None : reduction;
            canvas.Graphics.DrawImage(image, target);
        });

    private static Bitmap Folded(PixelFrame frame, Coverage coverage, Color color) {
        int[] words = GC.AllocateUninitializedArray<int>(frame.Size.Width * frame.Size.Height);
        float[] weights = new float[frame.Size.Width];
        for (int index = 0; index < frame.Size.Height; index++) {
            coverage.Weights(frame.View.Span.GetRowSpan(index), frame.Origin.X, frame.Line(index), weights);
            for (int column = 0; column < weights.Length; column++)
                words[(index * weights.Length) + column] = new Color(color, color.A * weights[column]).ToArgb();
        }
        return ImagePlots.Pixels(frame.Size, words);
    }

    private static IO<Unit> Captioned(PlotCanvas canvas, FrameViewState state, SizeF size) =>
        Divided(state, size, canvas.Scale).Match(
            Some: divided =>
                from shadow in Plots.Paint(canvas, state.Mode.Rule.ToSeq().Map(_ => new PlotMark<FramePart>(
                    Ruler(divided.Along, divided.Position + Wipe.Width, divided.Bounds),
                    Some<MarkStyle>(new MarkStyle.Stroke(new MarkColor.Fixed(Colors.Black), Shade(canvas), Wipe.Width)),
                    None)))
                from before in Caption(canvas, divided.Left.Side.Caption, divided.Along, divided.Position, divided.Bounds, before: true)
                from after in Caption(canvas, divided.Right.Side.Caption, divided.Along, divided.Position, divided.Bounds, before: false)
                select unit,
            None: static () => IO.pure(unit));

    private static IO<Unit> Caption(PlotCanvas canvas, string text, Orientation along, float position, RectangleF bounds, bool before) =>
        IO.lift(() => (Extent: canvas.Graphics.MeasureString(canvas.Font, text), Inset: CaptionInset)).Bind(held => Lettered(
            canvas,
            text,
            along is Orientation.Vertical
                ? new PointF(before ? position - held.Inset - held.Extent.Width : position + held.Inset, bounds.Top + held.Inset)
                : new PointF(bounds.Left + held.Inset, before ? position - held.Inset - held.Extent.Height : position + held.Inset),
            along));

    private static IO<Unit> Readout(PlotCanvas canvas, FrameViewState state, SizeF size) =>
        (from pick in Picked(state)
         from at in Placed(state, size, canvas.Scale)
         select (Text: pick.Readout(state.Area), Bounds: at.Region(state.Area.Edges))).Match(
            Some: shown => IO.lift(() => (Extent: canvas.Graphics.MeasureString(canvas.Font, shown.Text), Inset: CaptionInset)).Bind(held =>
                Lettered(canvas, shown.Text, new PointF(shown.Bounds.Left, shown.Bounds.Top - held.Inset - held.Extent.Height), Orientation.Vertical)),
            None: static () => IO.pure(unit));

    private static IO<Unit> Lettered(PlotCanvas canvas, string text, PointF location, Orientation along) =>
        IO.lift(() => {
            canvas.Graphics.DrawText(
                canvas.Font,
                Plots.Resolve(canvas, new MarkStyle.Fill(new MarkColor.Fixed(Colors.Black), Shade(canvas))),
                location + (along is Orientation.Vertical ? new PointF(Wipe.Width, 0f) : new PointF(0f, Wipe.Width)),
                text);
            canvas.Graphics.DrawText(canvas.Font, Plots.Resolve(canvas, Letter), location, text);
        });

    private static float Shade(PlotCanvas canvas) => canvas.Accessibility.ReduceTransparency ? 1f : 0.5f;

    private static IO<Unit> Glyphed(PlotCanvas canvas, FrameViewState state, SizeF size) =>
        (from caged in Caged(state)
         from at in Placed(state, size, canvas.Scale)
         select Flips(at, QuadMapper.ToCorners(caged.Quad))).Match(
            Some: flips => Icons.Frames((GlyphRole.Flip, IconSlot.PanelButton.Master), IconSlot.PanelButton).Bind(icon => flips.TraverseM(flip => Turned(canvas, icon, flip)).As()).Map(unit),
            None: static () => IO.pure(unit));

    private static IO<Unit> Turned(PlotCanvas canvas, Image icon, (Orientation Axis, RectangleF Band, float Degrees) flip) =>
        DisposalOps.Within(canvas.Graphics.SaveTransformState, _ => IO.lift(() => {
            canvas.Graphics.TranslateTransform(flip.Band.Center);
            canvas.Graphics.RotateTransform(flip.Degrees);
        }).Bind(_ => Stamped(canvas, icon, flip.Band.Size)));

    private static IO<Unit> Stamped(PlotCanvas canvas, Image icon, SizeF size) =>
        IO.lift(() => canvas.Graphics.DrawImage(icon, RectangleF.FromCenter(PointF.Empty, size)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class QuadMapper {
    internal static partial QuadCorners ToCorners(FrameQuad quad);
}
