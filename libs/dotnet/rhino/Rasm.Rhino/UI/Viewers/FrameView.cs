using System.Globalization;
using System.Numerics;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Rhino.UI.Controls;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.UI.Viewers;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct FrameZoom : IMinMaxValue<FrameZoom> {
    public static FrameZoom MinValue { get; } = new(float.BitIncrement(0f));
    public static FrameZoom MaxValue { get; } = new(float.MaxValue);
    public static FrameZoom Actual { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct OverlayOpacity : IMinMaxValue<OverlayOpacity> {
    public static OverlayOpacity MinValue { get; } = new(0f);
    public static OverlayOpacity MaxValue { get; } = new(1f);
    public static OverlayOpacity Half { get; } = new(0.5f);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
[ObjectFactory<string>]
public sealed partial class FrameRegion : IConvertible<string> {
    public AxisFraction Left { get; }
    public AxisFraction Top { get; }
    public AxisFraction Right { get; }
    public AxisFraction Bottom { get; }

    public static FrameRegion Whole { get; } = new(AxisFraction.MinValue, AxisFraction.MinValue, AxisFraction.MaxValue, AxisFraction.MaxValue);

    public RectangleF Edges => RectangleF.FromSides(Left, Top, Right, Bottom);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref AxisFraction left, ref AxisFraction top, ref AxisFraction right, ref AxisFraction bottom) =>
        validationError = left < right && top < bottom ? null : new InvalidRhinoValue();

    internal static InvalidRhinoValue? Validate(RectangleF edges, out FrameRegion? item) {
        item = null;
        return AxisFraction.Validate(edges.Left, CultureInfo.InvariantCulture, out AxisFraction left) is null
            && AxisFraction.Validate(edges.Top, CultureInfo.InvariantCulture, out AxisFraction top) is null
            && AxisFraction.Validate(edges.Right, CultureInfo.InvariantCulture, out AxisFraction right) is null
            && AxisFraction.Validate(edges.Bottom, CultureInfo.InvariantCulture, out AxisFraction bottom) is null
                ? Validate(left, top, right, bottom, out item)
                : new InvalidRhinoValue();
    }

    public static InvalidRhinoValue? Validate(string? value, IFormatProvider? provider, out FrameRegion? item) {
        item = null;
        return value is null
            ? null
            : toSeq(value.Split(','))
                .Traverse(static field => Callbacks.Found(float.TryParse(field, NumberStyles.Float, CultureInfo.InvariantCulture, out float number), number))
                .As()
                .Case switch {
                    Seq<float> and [var left, var top, var right, var bottom] => Validate(RectangleF.FromSides(left, top, right, bottom), out item),
                    _ => new InvalidRhinoValue(),
                };
    }

    public string ToValue() => string.Create(CultureInfo.InvariantCulture, $"{(float)Left},{(float)Top},{(float)Right},{(float)Bottom}");
}

[Union]
public abstract partial record FrameTransform {
    public sealed record Fitted : FrameTransform;
    public sealed record Placed(FrameZoom Zoom, Vector2 Center) : FrameTransform;
}

public sealed record FrameFeed(IO<Option<PixelFrame>> Read, Transfer Encoding, Option<Nits> Peak, ImageInterpolation Reduction);

public sealed record FrameSide(FrameFeed Feed, string Caption);

public sealed record FrameShown(FrameSide Side, Option<PixelFrame> Frame);

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class CompareMode {
    public static readonly CompareMode VerticalWipe = new(Some(Orientation.Vertical), Keys.Q, GlyphRole.VerticalWipe, "Vertical Wipe");
    public static readonly CompareMode HorizontalWipe = new(Some(Orientation.Horizontal), Keys.W, GlyphRole.HorizontalWipe, "Horizontal Wipe");
    public static readonly CompareMode SideBySide = new(Option<Orientation>.None, Keys.E, GlyphRole.SideBySide, "Side by Side");

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

public readonly record struct QuadCorners(Vector2 TopLeft, Vector2 TopRight, Vector2 BottomRight, Vector2 BottomLeft) {
    public Option<FrameQuad> Quad => FrameQuad.Validate(TopLeft, TopRight, BottomRight, BottomLeft, out FrameQuad? quad) is null ? Optional(quad) : None;
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class QuadCorner {
    public static readonly QuadCorner TopLeft = new(Lens<QuadCorners, Vector2>.New(static held => held.TopLeft, static point => held => held with { TopLeft = point }));
    public static readonly QuadCorner TopRight = new(Lens<QuadCorners, Vector2>.New(static held => held.TopRight, static point => held => held with { TopRight = point }));
    public static readonly QuadCorner BottomRight = new(Lens<QuadCorners, Vector2>.New(static held => held.BottomRight, static point => held => held with { BottomRight = point }));
    public static readonly QuadCorner BottomLeft = new(Lens<QuadCorners, Vector2>.New(static held => held.BottomLeft, static point => held => held with { BottomLeft = point }));

    public Lens<QuadCorners, Vector2> Point { get; }
}

public sealed record QuadPatch(QuadCorners Corners, Vector3 Fill, Vector3 Outline);

public sealed record FrameMask(Coverage Coverage, MarkStyle Style);

public sealed record FrameGuide(System.Drawing.Rectangle Frame, MarkStyle.Stroke Style);

[Union]
public abstract partial record FrameOverlay {
    public sealed record AreaPick(Option<PixelExtent> Aspect, Func<FrameRegion, string> Readout) : FrameOverlay;
    public sealed record MaskSet(Seq<FrameMask> Masks) : FrameOverlay;
    public sealed record GuideSet(Seq<FrameGuide> Guides) : FrameOverlay;
    public sealed record QuadCage(Func<FrameQuad, Seq<QuadPatch>> Patches, Gamut Working, OverlayOpacity Opacity) : FrameOverlay;
}

[Union]
public abstract partial record FrameEdit {
    public sealed record AreaEdited(FrameRegion Area) : FrameEdit;
    public sealed record QuadEdited(FrameQuad Quad) : FrameEdit;
    public sealed record PixelPicked(System.Drawing.Point Pixel) : FrameEdit;
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class PickKind {
    public static readonly PickKind Value = new(static sink => Icons.PickCursor(sink).Map(static cursor => (cursor, Some<IDisposable>(cursor))));
    public static readonly PickKind Position = new(static _ => IO.pure((Cursors.Crosshair, Option<IDisposable>.None)));

    [UseDelegateFromConstructor]
    public partial IO<(Cursor Shape, Option<IDisposable> Lease)> Cursor(IPlugInSink sink);
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
    Option<FrameOverlay> Overlay, FrameRegion Area, Option<FrameQuad> Quad, Option<PickKind> Picking, Option<FrameDrag> Drag);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class FrameView : ComponentControl<FrameViewState, FramePart, FrameEdit> {
    // --- [STATE]
    private readonly Seq<(Command Command, Func<FrameViewState, Transition<FrameViewState, FrameEdit>> Act)> bindings;
    private Cache cache = Cache.Empty;

    public FrameView(IPlugInSink sink, FrameSide current, IO<Option<FrameSide>> reference)
        : base(sink, Initial(current), Some(Sampled(sink, current, reference))) {
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

    public IO<Unit> Pick(PickKind kind) =>
        from lease in kind.Cursor(Sink)
        from released in Unpicked
        from held in IO.lift(() => {
            cache = cache with { Pick = Some(lease) };
            Focus();
        })
        from picked in Advance(state => new(state with { Picking = Some(kind), Drag = None }, None))
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
        from released in when(before.Picking.IsSome && after.Picking.IsNone, Unpicked).As()
        select released;

    protected override void OnUnLoad(EventArgs e) {
        base.OnUnLoad(e);
        _ = Callbacks.Answer(Emptied, static () => unit, Site());
    }

    private static FrameViewState Initial(FrameSide current) =>
        new(new FrameShown(current, None), None, Nits.ReferenceWhite, new FrameTransform.Fitted(), Comparing: false, CompareMode.VerticalWipe, AxisFraction.Half, Swapped: false,
            None, FrameRegion.Whole, None, None, None);

    private static Func<Control, IO<Func<FrameViewState, FrameViewState>>> Sampled(IPlugInSink sink, FrameSide current, IO<Option<FrameSide>> reference) =>
        view =>
            from frame in current.Feed.Read
            from side in reference
            from other in side.Traverse(static held => held.Feed.Read.Map(read => new FrameShown(held, read))).As()
            from target in Target(sink, view)
            select fun((FrameViewState held) => held with {
                Current = new FrameShown(current, frame),
                Reference = other,
                Target = target,
                Transform = frame.Map(static shown => shown.Size) == held.Current.Frame.Map(static shown => shown.Size) ? held.Transform : new FrameTransform.Fitted(),
            });

    private IO<Unit> Unpicked =>
        from held in IO.lift(() => cache.Pick)
        from cleared in IO.lift(() => {
            cache = cache with { Pick = None };
        })
        from released in DisposalOps.Release(held.Bind(static pick => pick.Lease).ToSeq())
        select released;

    private IO<Unit> Emptied =>
        from held in IO.lift(() => cache)
        from cleared in IO.lift(() => {
            cache = Cache.Empty;
        })
        from released in DisposalOps.Release(held.Owned)
        select released;

    private IO<Bitmap> Cached<TKey>(Lens<Cache, HashMap<TKey, Bitmap>> slot, TKey key, Func<IO<Bitmap>> build) =>
        IO.lift(() => slot.Get(cache).Find(key)).Bind(found => found.Match(
            Some: static image => IO.pure(image),
            None: () =>
                from image in build()
                from stored in IO.lift(() => {
                    cache = slot.Update(held => held.Add(key, image), cache);
                })
                select image));

    private IO<Unit> Retained<TKey>(Lens<Cache, HashMap<TKey, Bitmap>> slot, LanguageExt.HashSet<TKey> keep) =>
        from gone in IO.lift(() => slot.Get(cache).Filter((key, _) => !keep.Contains(key)))
        from kept in IO.lift(() => {
            cache = slot.Update(held => held.Filter((key, _) => keep.Contains(key)), cache);
        })
        from released in DisposalOps.Release(toSeq(gone.Values))
        select released;

    private sealed record Cache(
        HashMap<(PixelFrame Frame, Nits Target), Bitmap> Frames,
        HashMap<(PixelFrame Frame, FrameMask Mask, Color Color), Bitmap> Tints,
        Option<(Cursor Shape, Option<IDisposable> Lease)> Pick) {
        public static Cache Empty { get; } = new(HashMap<(PixelFrame Frame, Nits Target), Bitmap>(), HashMap<(PixelFrame Frame, FrameMask Mask, Color Color), Bitmap>(), None);

        public static Lens<Cache, HashMap<(PixelFrame Frame, Nits Target), Bitmap>> FrameSlot { get; } =
            Lens<Cache, HashMap<(PixelFrame Frame, Nits Target), Bitmap>>.New(static held => held.Frames, static frames => held => held with { Frames = frames });

        public static Lens<Cache, HashMap<(PixelFrame Frame, FrameMask Mask, Color Color), Bitmap>> TintSlot { get; } =
            Lens<Cache, HashMap<(PixelFrame Frame, FrameMask Mask, Color Color), Bitmap>>.New(static held => held.Tints, static tints => held => held with { Tints = tints });

        public Seq<IDisposable> Owned =>
            toSeq(Frames.Values).Map<IDisposable>(static image => image) + toSeq(Tints.Values).Map<IDisposable>(static image => image) + Pick.Bind(static pick => pick.Lease).ToSeq();
    }

    private readonly record struct Placement(RectangleF Pane, float Scale, Vector2 Extent, float Fit, float Zoom, Vector2 Center) {
        private Vector2 Middle => new(Pane.X + (Pane.Width / 2f), Pane.Y + (Pane.Height / 2f));

        public RectangleF Frame => Mapped(new RectangleF(0f, 0f, Extent.X, Extent.Y));
        public RectangleF Seen => RectangleF.Intersect(Frame, Pane);
        public RectangleF Visible => (Point(Pane.TopLeft), Point(Pane.BottomRight)) switch {
            var (from, to) => RectangleF.FromSides(from.X, from.Y, to.X, to.Y),
        };

        public PointF View(Vector2 point) => (((point - Center) * (Zoom / Scale)) + Middle) switch {
            var view => new PointF(view.X, view.Y),
        };

        public Vector2 Point(PointF view) => ((new Vector2(view.X, view.Y) - Middle) * (Scale / Zoom)) + Center;

        public Vector2 Fraction(PointF view) => Vector2.Clamp(Point(view) / Extent, Vector2.Zero, Vector2.One);

        public Vector2 Holding(PointF view, float zoom) => Point(view) - ((new Vector2(view.X, view.Y) - Middle) * (Scale / zoom));

        public RectangleF Mapped(RectangleF frame) => (View(new Vector2(frame.Left, frame.Top)), View(new Vector2(frame.Right, frame.Bottom))) switch {
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

    private static string Flipping(Orientation axis) => axis == Orientation.Horizontal ? "Flip Horizontal" : "Flip Vertical";

    private static Func<FrameViewState, Transition<FrameViewState, FrameEdit>> Selected(CompareMode mode) =>
        state => Viewed(state with { Comparing = !(state.Comparing && state.Mode == mode), Mode = mode });

    private static Transition<FrameViewState, FrameEdit> Flip(FrameViewState state, Orientation axis) =>
        state.Quad.Map(quad => axis == Orientation.Horizontal ? quad.FlippedHorizontal : quad.FlippedVertical).Match(
            Some: flipped => new Transition<FrameViewState, FrameEdit>(
                state with { Quad = Some(flipped) }, Some<Edit<FrameEdit>>(new Edit<FrameEdit>.Commit(new FrameEdit.QuadEdited(flipped)))),
            None: () => Viewed(state));

    // --- [READS]
    public IO<Option<FrameZoom>> Zoom =>
        IO.lift(() => Placed(State, Size, ParentWindow.LogicalPixelSize).Bind(static at => Conversions.Validated<FrameZoom, float, InvalidRhinoValue>(at.Zoom).ToOption()));

    public IO<Option<System.Drawing.Point>> Hovered =>
        IO.lift(() =>
            from pointer in Pointer
            from at in At(State, Size, ParentWindow.LogicalPixelSize, pointer)
            from pixel in Pixel(at, pointer)
            select pixel);

    public IO<Option<RectangleF>> VisibleFrame => IO.lift(() => Placed(State, Size, ParentWindow.LogicalPixelSize).Map(static at => at.Visible));

    private static Option<System.Drawing.Point> Pixel(Placement at, PointF view) =>
        at.Point(view) switch {
            var point when point.X >= 0f && point.Y >= 0f && point.X < at.Extent.X && point.Y < at.Extent.Y =>
                Some(new System.Drawing.Point((int)MathF.Floor(point.X), (int)MathF.Floor(point.Y))),
            _ => None,
        };

    private static Vector2 Extent(PixelFrame frame) => new(frame.Size.Width, frame.Size.Height);

    private static Vector2 Centre(FrameTransform transform, Vector2 extent) =>
        transform.Switch(extent, fitted: static (held, _) => held / 2f, placed: static (_, placed) => placed.Center);

    private static Option<Placement> Placed(FrameViewState state, RectangleF pane, float scale) =>
        from frame in state.Current.Frame
        let extent = Extent(frame)
        let fit = float.Min(pane.Width * scale / extent.X, pane.Height * scale / extent.Y)
        select new Placement(pane, scale, extent, fit, state.Transform.Switch(fit, fitted: static (held, _) => held, placed: static (_, placed) => (float)placed.Zoom), Centre(state.Transform, extent));

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
        state.Comparing && state.Picking.IsNone ? state.Reference.Map(reference => state.Swapped ? (state.Current, reference) : (reference, state.Current)) : None;

    private static Option<(Orientation Along, float Position, RectangleF Bounds, FrameShown Left, FrameShown Right)> Divided(FrameViewState state, SizeF size, float scale) =>
        from pair in Compared(state)
        from at in Placed(state, size, scale)
        select state.Mode.Rule.Match(
            Some: rule => (rule, Ruled(at, rule, state.Split), at.Seen, pair.Left, pair.Right),
            None: () => (Orientation.Vertical, at.Pane.Right, at.Seen, pair.Left, pair.Right));

    private static float Ruled(Placement at, Orientation rule, AxisFraction split) =>
        at.View(at.Extent * (float)split) switch {
            var view => rule == Orientation.Vertical ? view.X : view.Y,
        };

    private static Option<FrameOverlay> Overlaid(FrameViewState state) =>
        state.Picking.IsNone && !state.Comparing ? state.Overlay : None;

    private static Option<FrameOverlay.AreaPick> Picked(FrameViewState state) =>
        Overlaid(state).Bind(static overlay => overlay.Switch<Option<FrameOverlay.AreaPick>>(
            areaPick: static pick => pick, maskSet: static _ => None, guideSet: static _ => None, quadCage: static _ => None));

    private static Seq<FrameMask> Masks(FrameViewState state) =>
        Overlaid(state).ToSeq().Bind(static overlay => overlay.Switch(
            areaPick: static _ => [], maskSet: static set => set.Masks, guideSet: static _ => [], quadCage: static _ => []));

    private static Seq<FrameGuide> Guides(FrameViewState state) =>
        Overlaid(state).ToSeq().Bind(static overlay => overlay.Switch(
            areaPick: static _ => [], maskSet: static _ => [], guideSet: static set => set.Guides, quadCage: static _ => []));

    private static Option<(FrameOverlay.QuadCage Cage, FrameQuad Quad)> Caged(FrameViewState state) =>
        from cage in Overlaid(state).Bind(static overlay => overlay.Switch<Option<FrameOverlay.QuadCage>>(
            areaPick: static _ => None, maskSet: static _ => None, guideSet: static _ => None, quadCage: static cage => cage))
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
                    new MarkShape.Band(at.Mapped(new RectangleF(guide.Frame.X, guide.Frame.Y, guide.Frame.Width, guide.Frame.Height))), Some<MarkStyle>(guide.Style), None))
                + Divided(state, size, scale).Filter(_ => state.Mode.Rule.IsSome).ToSeq().Map(static divided => new PlotMark<FramePart>(
                    Ruler(divided.Along, divided.Position, divided.Bounds), Some<MarkStyle>(Wipe), Some(new MarkPart<FramePart>(new FramePart.Rule(), Plots.Reach))))
                + Picked(state).ToSeq().Bind(_ => Areas(at, state.Area))
                + Caged(state).ToSeq().Bind(held => Cage(at, held.Cage, held.Quad))));

    protected override PartFacet Facet(FrameViewState state, FramePart key) =>
        key.Switch(
            (State: state, Ground: Grounded(state)),
            ground: static (held, _) => new PartFacet(PartRole.Image, held.Ground, RowText.Localize("Frame").Local, Measured(held.State), None),
            rule: static (held, _) => new PartFacet(PartRole.Image, held.Ground, RowText.Localize("Wipe").Local, Some(RowText.Localize("{0:P0}", None, (float)held.State.Split).Local), None),
            edge: static (held, edge) => new PartFacet(PartRole.Image, edge.Handle.Shape, RowText.Localize("Area").Local, Some(RowText.Localize(
                "{0:P0}, {1:P0}, {2:P0}, {3:P0}", None, (float)held.State.Area.Left, (float)held.State.Area.Top, (float)held.State.Area.Right, (float)held.State.Area.Bottom).Local), None),
            flip: static (_, flip) => new PartFacet(PartRole.Button, Cursors.Pointer, RowText.Localize(Flipping(flip.Axis)).Local, None, None),
            corner: static (held, corner) => new PartFacet(PartRole.Image, Cursors.Move, RowText.Localize("Corner").Local, held.State.Quad
                .Map(quad => corner.Handle.Point.Get(QuadMapper.ToCorners(quad)))
                .Map(static point => RowText.Localize("{0:0.#}, {1:0.#}", None, point.X, point.Y).Local), None));

    protected override Option<Transition<FrameViewState, FrameEdit>> Pressed(FrameViewState state, SizeF size, float scale, Option<FramePart> part, MouseEventArgs e) =>
        At(state, size, scale, e.Location).Map(at => (state.Picking.IsSome, e.Buttons) switch {
            (true, MouseButtons.Primary) => Pixel(at, e.Location).Match(
                Some: pixel => new Transition<FrameViewState, FrameEdit>(
                    state with { Picking = None }, Some<Edit<FrameEdit>>(new Edit<FrameEdit>.Commit(new FrameEdit.PixelPicked(pixel)))),
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
        state.Picking.IsSome && e.Buttons != MouseButtons.Primary && state.Drag.Exists(drag => Still(state.Transform, drag))
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
        e.KeyData == Keys.Escape && state.Picking.IsSome
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
        new(state with { Picking = None, Drag = None }, Some<Edit<FrameEdit>>(new Edit<FrameEdit>.Cancel()));

    private Cursor Grounded(FrameViewState state) =>
        (state.Picking.Bind(_ => cache.Pick).Map(static pick => pick.Shape)
         | Compared(state).Bind(_ => state.Mode.Rule).Map(static rule => rule == Orientation.Vertical ? Cursors.VerticalSplit : Cursors.HorizontalSplit)
         | Picked(state).Map(static _ => Cursors.Crosshair))
        .IfNone(Cursors.Default);

    private static Option<string> Measured(FrameViewState state) =>
        state.Current.Frame.Map(frame => state.Transform.Switch(
            frame,
            fitted: static (shown, _) => RowText.Localize("{0} × {1}", None, shown.Size.Width, shown.Size.Height).Local,
            placed: static (shown, placed) => RowText.Localize("{0} × {1}, {2:P0}", None, shown.Size.Width, shown.Size.Height, (float)placed.Zoom).Local));

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
                from split in Conversions.Validated<AxisFraction, float, InvalidGrade>(rule == Orientation.Vertical ? fraction.X : fraction.Y).ToOption()
                select state with { Split = split })
            .IfNone(state));

    private static Transition<FrameViewState, FrameEdit> Panned(FrameViewState state, Placement at, Vector2 start, PointF moved) =>
        Viewed(Placing(at, at.Zoom, start - (new Vector2(moved.X, moved.Y) * (at.Scale / at.Zoom))).Map(transform => state with { Transform = transform }).IfNone(state));

    private static Option<FrameTransform> Placing(Placement at, float zoom, Vector2 center) =>
        Conversions.Validated<FrameZoom, float, InvalidRhinoValue>(zoom).ToOption()
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
            var ratio when anchor.X != 0.5f => (edges.Width * extent.X / ratio / extent.Y) switch {
                var height => anchor.Y == 0f
                    ? RectangleF.FromSides(edges.Left, edges.Bottom - height, edges.Right, edges.Bottom)
                    : RectangleF.FromSides(edges.Left, edges.Top, edges.Right, edges.Top + height),
            },
            var ratio when anchor.Y != 0.5f => RectangleF.FromSides(edges.Left, edges.Top, edges.Left + (edges.Height * extent.Y * ratio / extent.X), edges.Bottom),
            _ => edges,
        };

    private static Transition<FrameViewState, FrameEdit> Cornered(FrameViewState state, Placement at, FrameDrag.Cornering corner, PointF previous, MouseEventArgs e) =>
        (e.Modifiers.HasFlag(Keys.Alt), e.Modifiers.HasFlag(Keys.Shift), Scale(e.Modifiers)) switch {
            (true, true, var fine) => Bent(
                state, corner, Matrix3x2.CreateRotation((e.Location.X - previous.X) * at.Scale * Turn * fine, corner.Handle.Point.Get(corner.Raw)), other => other != corner.Handle),
            (var every, _, var fine) => Bent(
                state, corner, Matrix3x2.CreateTranslation((at.Point(e.Location) - at.Point(previous)) * fine), other => every || other == corner.Handle),
        };

    private static Option<Transition<FrameViewState, FrameEdit>> Spreaded(FrameViewState state, FrameDrag drag, float factor) =>
        drag.Switch<(FrameViewState State, float Factor), Option<Transition<FrameViewState, FrameEdit>>>(
            (state, factor),
            panning: static (_, _) => None,
            wiping: static (_, _) => None,
            sizing: static (_, _) => None,
            cornering: static (held, corner) => Bent(held.State, corner, Matrix3x2.CreateScale(held.Factor, corner.Handle.Point.Get(corner.Raw)), other => other != corner.Handle));

    private static Transition<FrameViewState, FrameEdit> Bent(FrameViewState state, FrameDrag.Cornering drag, Matrix3x2 motion, Func<QuadCorner, bool> moves) =>
        toSeq(QuadCorner.Items).Filter(moves).Fold(drag.Raw, (raw, corner) => corner.Point.Update(point => Vector2.Transform(point, motion), raw)) switch {
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
        float glyph = IconSlot.PanelButton.Master.Points;
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
        along == Orientation.Vertical
            ? new MarkShape.Rule(new PointF(position, bounds.Top), new PointF(position, bounds.Bottom))
            : new MarkShape.Rule(new PointF(bounds.Left, position), new PointF(bounds.Right, position));

    // --- [PAINT]
    private const float GripSide = 4f;

    private static readonly MarkStyle Outline = new MarkStyle.Stroke(new MarkColor.Themed(PaintSlot.Highlight), 1f, 1f);
    private static readonly MarkStyle Knob = new MarkStyle.Fill(new MarkColor.Themed(PaintSlot.Highlight), 1f);
    private static readonly MarkStyle.Stroke Wipe = new(new MarkColor.Fixed(Colors.White), 1f, 1f);
    private static readonly MarkStyle Letter = new MarkStyle.Fill(new MarkColor.Fixed(Colors.White), 1f);

    private static float CaptionInset => RhinoLayout.Padding(RhinoLayout.PaddingType.RhinoPanel).Top;

    public static IO<Bitmap> Raster(IPlugInSink sink, Control view, PixelFrame frame, Transfer encoding) =>
        IO.lift(static () => Optional(Platform.Instance.Find<Func<IPlugInSink, Control, PixelFrame, Transfer, IO<Bitmap>>>()))
            .Bind(found => found.Match(
                Some: create => create()(sink, view, frame, encoding),
                None: () => IO.lift(() => Plots.Pixels(frame.Size, encoding.Displayed(frame)))));

    protected override IO<Unit> Draw(PlotCanvas canvas, RectangleF bounds, FrameViewState state, Seq<PlotMark<FramePart>> marks, Interaction<FramePart> interaction) =>
        from well in Plots.Well(canvas, bounds)
        let layers = Layers(state, bounds.Size, canvas.Scale)
        let tints =
            from at in Placed(state, bounds.Size, canvas.Scale).ToSeq()
            from frame in state.Current.Frame.ToSeq()
            from mask in Masks(state)
            select (At: at, Frame: frame, Mask: mask, Color: Plots.Resolve(canvas, mask.Style))
        from framesKept in Retained(Cache.FrameSlot, toHashSet(layers.Choose(layer => layer.Side.Frame.Map(frame => (frame, state.Target)))))
        from tintsKept in Retained(Cache.TintSlot, toHashSet(tints.Map(static tint => (tint.Frame, tint.Mask, tint.Color))))
        from framed in layers.TraverseM(layer => Framed(canvas, layer, state.Target)).As()
        from masked in tints.TraverseM(tint => Tinted(canvas, tint, state.Current.Side.Feed.Reduction)).As()
        from captioned in Captioned(canvas, state, bounds.Size)
        from painted in Plots.Paint(canvas, marks)
        from glyphs in Glyphed(canvas, state, bounds.Size)
        from readout in Readout(canvas, state, bounds.Size)
        select unit;

    private static IO<Nits> Target(IPlugInSink sink, Control view) =>
        IO.lift(static () => Optional(Platform.Instance.Find<Func<IPlugInSink, Control, IO<float>>>()))
            .Bind(found => found.Match(
                Some: create => create()(sink, view),
                None: static () => IO.pure(1f)))
            .Bind(static headroom => IO.lift(Conversions.Validated<Nits, float, InvalidColor>(headroom * (float)Nits.ReferenceWhite)));

    private static IO<Bitmap> Mapped(IPlugInSink sink, Control view, FrameFeed feed, PixelFrame frame, Nits target) =>
        feed.Peak.Bind(peak => new PeakMapping(target, DisplayBlack.Floor, KneeOffset.Bt2390).Pass(Nits.ReferenceWhite, peak)).Match(
            Some: pass =>
                from light in IO.lift(() => feed.Encoding.Light(frame))
                from mapped in IO.lift(() => pass.Run(light, new Progress<int>()))
                from image in Raster(sink, view, light, TransferCurve.Linear)
                select image,
            None: () => Raster(sink, view, frame, feed.Encoding));

    private static Seq<(Placement At, FrameShown Side, RectangleF Clip)> Layers(FrameViewState state, SizeF size, float scale) =>
        Divided(state, size, scale).Match(
            Some: divided => state.Mode.Rule.IsSome
                ? Placed(state, size, scale).ToSeq().Bind(at => Seq((at, divided.Right, at.Pane), (at, divided.Left, Before(at.Pane, divided.Along, divided.Position))))
                : from lane in Panes(state, size).Zip(Seq(divided.Left, divided.Right))
                  from at in Placed(state, lane.First, scale).ToSeq()
                  select (at, lane.Second, at.Pane),
            None: () => Placed(state, size, scale).ToSeq().Map(at => (at, state.Current, at.Pane)));

    private static RectangleF Before(RectangleF pane, Orientation along, float position) =>
        along == Orientation.Vertical
            ? RectangleF.FromSides(pane.Left, pane.Top, position, pane.Bottom)
            : RectangleF.FromSides(pane.Left, pane.Top, pane.Right, position);

    private IO<Unit> Framed(PlotCanvas canvas, (Placement At, FrameShown Side, RectangleF Clip) layer, Nits target) =>
        layer.Side.Frame.Match(
            Some: frame => Clipped(canvas, layer.Clip, Pictured(canvas, layer.At.Fitted(frame.Size), frame, layer.Side.Side.Feed, target)),
            None: static () => IO.pure(unit));

    private IO<Unit> Pictured(PlotCanvas canvas, RectangleF bounds, PixelFrame frame, FrameFeed feed, Nits target) =>
        from board in Thumbnails.Checker(canvas, bounds, Checkerboard.Frame)
        from image in Cached(Cache.FrameSlot, (frame, target), () => Mapped(Sink, this, feed, frame, target))
        from drawn in Imaged(canvas, image, bounds, frame.Size, feed.Reduction)
        select drawn;

    private IO<Unit> Tinted(PlotCanvas canvas, (Placement At, PixelFrame Frame, FrameMask Mask, Color Color) tint, ImageInterpolation reduction) =>
        from image in Cached(Cache.TintSlot, (tint.Frame, tint.Mask, tint.Color), () => IO.lift(() => Folded(tint.Frame, tint.Mask.Coverage, tint.Color)))
        from drawn in Imaged(canvas, image, tint.At.Frame, tint.Frame.Size, reduction)
        select drawn;

    private static IO<Unit> Clipped(PlotCanvas canvas, RectangleF clip, IO<Unit> draw) =>
        IO.lift(() => canvas.Graphics.SetClip(clip)).Bracket(Use: _ => draw, Fin: _ => IO.lift(canvas.Graphics.ResetClip));

    private static IO<Unit> Imaged(PlotCanvas canvas, Image image, RectangleF target, PixelExtent extent, ImageInterpolation reduction) =>
        IO.lift(() => {
            canvas.Graphics.ImageInterpolation = target.Width * canvas.Scale >= extent.Width ? ImageInterpolation.None : reduction;
            canvas.Graphics.DrawImage(image, target);
        });

    private static Bitmap Folded(PixelFrame frame, Coverage coverage, Color color) {
        int[] words = GC.AllocateUninitializedArray<int>(frame.Size.Width * frame.Size.Height);
        float[] weights = new float[frame.Size.Width];
        for (int index = 0; index < frame.Size.Height; index++) {
            coverage.Weights(frame.View.Span.GetRowSpan(index), frame.Origin.X, frame.Line(index), frame.Extent, weights);
            for (int column = 0; column < weights.Length; column++)
                words[(index * weights.Length) + column] = new Color(color, color.A * weights[column]).ToArgb();
        }
        return Plots.Pixels(frame.Size, words);
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
            along == Orientation.Vertical
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
                location + (along == Orientation.Vertical ? new PointF(Wipe.Width, 0f) : new PointF(0f, Wipe.Width)),
                text);
            canvas.Graphics.DrawText(canvas.Font, Plots.Resolve(canvas, Letter), location, text);
        });

    private static float Shade(PlotCanvas canvas) => canvas.Accessibility.ReduceTransparency ? 1f : 0.5f;

    private IO<Unit> Glyphed(PlotCanvas canvas, FrameViewState state, SizeF size) =>
        (from caged in Caged(state)
         from at in Placed(state, size, canvas.Scale)
         select Flips(at, QuadMapper.ToCorners(caged.Quad))).Match(
            Some: flips => Icons.Frames(Sink, GlyphRole.Flip, IconSlot.PanelButton).Bind(icon => flips.TraverseM(flip => Turned(canvas, icon, flip)).As()).Map(static _ => unit),
            None: static () => IO.pure(unit));

    private static IO<Unit> Turned(PlotCanvas canvas, Image icon, (Orientation Axis, RectangleF Band, float Degrees) flip) =>
        IO.lift(() => {
            canvas.Graphics.SaveTransform();
            canvas.Graphics.TranslateTransform(flip.Band.Center);
            canvas.Graphics.RotateTransform(flip.Degrees);
        }).Bracket(Use: _ => Stamped(canvas, icon, flip.Band.Size), Fin: _ => IO.lift(canvas.Graphics.RestoreTransform));

    private static IO<Unit> Stamped(PlotCanvas canvas, Image icon, SizeF size) =>
        IO.lift(() => canvas.Graphics.DrawImage(icon, RectangleF.FromCenter(PointF.Empty, size)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class QuadMapper {
    internal static partial QuadCorners ToCorners(FrameQuad quad);
}
