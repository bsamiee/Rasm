using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Imaging;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CompareMode : ICaptioned {
    public static readonly CompareMode VerticalWipe = new("vertical-wipe", Some(Orientation.Vertical));
    public static readonly CompareMode HorizontalWipe = new("horizontal-wipe", Some(Orientation.Horizontal));
    public static readonly CompareMode SideBySide = new("side-by-side", Option<Orientation>.None);

    public Option<Orientation> Rule { get; }
}

[ComplexValueObject]
[ObjectFactory<string>]
public sealed partial class FrameRegion : IConvertible<string> {
    public AxisFraction Left { get; }
    public AxisFraction Top { get; }
    public AxisFraction Right { get; }
    public AxisFraction Bottom { get; }

    public static FrameRegion Whole { get; } = new(AxisFraction.MinValue, AxisFraction.MinValue, AxisFraction.MaxValue, AxisFraction.MaxValue);

    public SKRect Edges => new(Left, Top, Right, Bottom);

    public static Fin<FrameRegion> Spanning(SKRect edges) =>
        (AxisFraction.Validated(edges.Left), AxisFraction.Validated(edges.Top), AxisFraction.Validated(edges.Right), AxisFraction.Validated(edges.Bottom))
            .Apply(static (left, top, right, bottom) => (Left: left, Top: top, Right: right, Bottom: bottom)).As()
            .Bind(static sides => ValidationResults.ToFin(Validate(sides.Left, sides.Top, sides.Right, sides.Bottom, out FrameRegion? region), region));

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out FrameRegion? item) {
        (item, ValidationError? fault) = value is null
            ? (null, null)
            : (toSeq(value.Split(',')).Traverse(static field => Invariant.Number<float>(field)).As().Case is Seq<float> and [var left, var top, var right, var bottom]
                    ? Spanning(new SKRect(left, top, right, bottom))
                    : Error.New($"frame region '{value}' is no four comma-separated fractions"))
                .Match(Succ: static region => ((FrameRegion?)region, (ValidationError?)null), Fail: static error => (null, new ValidationError(error.Message)));
        return fault;
    }

    public string ToValue() => string.Create(CultureInfo.InvariantCulture, $"{(float)Left},{(float)Top},{(float)Right},{(float)Bottom}");

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref AxisFraction left, ref AxisFraction top, ref AxisFraction right, ref AxisFraction bottom) =>
        validationError = left < right && top < bottom ? null : new ValidationError("frame region needs left < right and top < bottom");
}

public sealed record FrameSide(Display Display, string Caption);

public sealed record FrameShown(FrameSide Side, Content<SKImage> Image, PixelExtent Extent);

public sealed record FrameState(FrameShown Current, Option<FrameShown> Reference, PlotPlane.Cartesian View, CompareMode Mode, Fraction Split, bool Swapped) {
    public static PlotPlane.Cartesian Fitted(PixelExtent extent) =>
        new(new Axis.Linear(AxisRange.Create(0f, extent.Width)), new Axis.Linear(AxisRange.Create(extent.Height, 0f)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FrameTool {
    public sealed record Region(FrameRegion Value, Option<PixelExtent> Aspect, Func<Edit<FrameRegion>, IO<Unit>> Edited) : FrameTool;

    public sealed record Cage(FrameQuad Value, Func<FrameQuad, Seq<Node>> Patches, Func<Edit<FrameQuad>, IO<Unit>> Edited) : FrameTool;

    public sealed record Pick(Func<Vector2, IO<Unit>> Picked) : FrameTool;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Frames {
    // --- [IMAGE]
    public static IO<SKImage> Image(PixelFrame frame, Display display, float headroom) =>
        from target in IO.lift(Nits.Validated(headroom * (float)Nits.ReferenceWhite))
        from image in IO.lift(() => (PeakMapping.Default with { TargetPeak = target }).Pass(Nits.ReferenceWhite, display.Peak).Match(
            Some: pass => Linear(display.Encoding.Transfer.Light(frame), display.Encoding, held => pass.Run(held, new Progress<int>()).Map(_ => held)),
            None: () => Space(display.Encoding).Match(
                Some: space => Wrapped(frame, space),
                None: () => Linear(display.Encoding.Transfer.Light(frame), display.Encoding, Fin.Succ))))
        select image;

    public static IO<SKImage> Mask(PixelFrame frame, Coverage coverage) =>
        IO.lift(() => {
            float[] weights = new float[frame.Size.Width * frame.Size.Height];
            for (int row = 0; row < frame.Size.Height; row++)
                coverage.Weights(frame.Row(frame.Line(row)), frame.Origin.X, frame.Line(row), weights.AsSpan(row * frame.Size.Width, frame.Size.Width));
            Half[] halves = new Half[weights.Length];
            TensorPrimitives.ConvertToHalf(weights, halves);
            return SKImage.FromPixelCopy(new SKImageInfo(frame.Size.Width, frame.Size.Height, SKColorType.AlphaF16, SKAlphaType.Premul), MemoryMarshal.AsBytes(halves.AsSpan()));
        });

    private static Fin<SKImage> Linear(PixelFrame light, ColorEncoding encoding, Func<PixelFrame, Fin<PixelFrame>> mapped) =>
        (mapped(light), Space(encoding with { Transfer = TransferCurve.Linear }).ToFin(Error.New($"linear {encoding.Gamut.Key} light has neither a CICP code nor an ICC profile")))
            .Apply(Wrapped).As();

    private static Option<SKColorSpace> Space(ColorEncoding encoding) =>
        encoding.Cicp.Bind(static cicp => Optional(SKColorSpace.CreateCicp((SKColorspacePrimariesCicp)(int)cicp.ColorPrimaries, (SKColorspaceTransferFnCicp)(int)cicp.TransferCharacteristics)))
        | encoding.Profile(DateTimeOffset.UnixEpoch).Bind(static icc => Optional(SKColorSpace.CreateIcc(icc)));

    private static SKImage Wrapped(PixelFrame frame, SKColorSpace space) {
        SKImageInfo info = new(frame.Size.Width, frame.Size.Height, SKColorType.RgbaF32, SKAlphaType.Unpremul, space);
        using SKPixmap pixels = new(info, frame.Address, info.RowBytes);
        return SKImage.FromPixels(pixels, static (_, held) => GC.KeepAlive(held), frame);
    }

    // --- [VIEW]
    private static readonly Sizing Grown = new Sizing.Grow(Factor.Create(1f));
    private static readonly Seq<Vector2> Anchors = toSeq(from x in Seq(0f, 0.5f, 1f) from y in Seq(0f, 0.5f, 1f) select new Vector2(x, y));
    private static readonly Key Picked = new Key.Named("pick");

    public static Node View(
        FrameState state, Seq<(FrameMask Mask, Content<SKImage> Plane)> masks, Option<FrameTool> tool,
        Func<Edit<FrameState>, IO<Unit>> edited, Func<Option<Vector2>, IO<Unit>> pointer) {
        Part.Viewport viewport = new(FrameState.Fitted(state.Current.Extent), edit => edited(edit.Map(view => view is PlotPlane.Cartesian cartesian ? state with { View = cartesian } : state).As()));
        Node tooled = Tooled(state, tool, pointer);
        Seq<Node> shaded = masks.Choose(mask => mask.Plane.Held.Map(plane => (Node)Spanned(state.Current.Extent, new Node.Image(plane) { Role = mask.Mask.Role })));
        return state.Reference.Match(
            Some: reference => (state.Swapped ? (First: reference, Second: state.Current) : (First: state.Current, Second: reference)) switch {
                var sides => state.Mode.Rule.Match<Node>(
                    Some: rule => Framed(state, sides.First, shaded + Seq(Wiped(state, sides.Second, rule.Cross), Split(state, rule, edited), tooled), viewport),
                    None: () => new Node.Flex(Orientation.Horizontal, Seq<Node>(Framed(state, sides.First, shaded.Add(tooled), viewport), Framed(state, sides.Second, [], viewport))) { Width = Grown }),
            },
            None: () => Framed(state, state.Current, shaded.Add(tooled), viewport));
    }

    private static Node.Overlay Framed(FrameState state, FrameShown shown, Seq<Node> above, Part.Viewport viewport) =>
        new(
            new Node.Plot(state.View, shown.Image.Held.Map(image => (Node)Spanned(shown.Extent, new Node.Image(image) { Rendering = ImageRendering.Pixelated })).ToSeq() + above) {
                Part = viewport,
                Width = Grown,
                Aspect = Factor.Create((float)shown.Extent.Width / shown.Extent.Height),
                Conditions = shown.Image is Content<SKImage>.Pending ? [Condition.Busy] : [],
            },
            [new Node.Text([shown.Side.Caption]) { Role = ImagingRoles.OverlayCaption, Align = UnitPoint.Create(0f, 0f) }]) { Width = Grown };

    private static Node.Pin Spanned(PixelExtent extent, Node content) => new(Vector2.Zero, content) { Span = new Vector2(extent.Width, extent.Height) };

    private static Node.Pin Wiped(FrameState state, FrameShown reference, Orientation along) {
        float split = along.Map(horizontal: state.View.X, vertical: state.View.Y).Value(state.Split);
        (Axis x, Axis y) = along.Map(
            horizontal: (state.View.X with { Range = AxisRange.Create(split, state.View.X.Range.To) }, state.View.Y),
            vertical: (state.View.X, state.View.Y with { Range = AxisRange.Create(split, state.View.Y.Range.To) }));
        return new Node.Pin(new Vector2(x.Range.From, y.Range.From), new Node.Plot(new PlotPlane.Cartesian(x, y),
            reference.Image.Held.Map(image => (Node)Spanned(reference.Extent, new Node.Image(image) { Rendering = ImageRendering.Pixelated })).ToSeq())) {
            Span = new Vector2(x.Range.Span, y.Range.Span),
        };
    }

    private static Node Split(FrameState state, Orientation rule, Func<Edit<FrameState>, IO<Unit>> edited) =>
        new PlotPlane.Track(rule.Cross, new Axis.Linear(AxisRange.Create(0f, 1f))) switch {
            var track => new Handles<FrameState>(state, Points<FrameState>.Scalar(
                    track,
                    Lens<FrameState, float>.New(static held => held.Split, static split => held => held with { Split = Fraction.Create(float.Clamp(split, 0f, 1f)) }),
                    static split => split.ToString("P0", CultureInfo.InvariantCulture)) with { Placed = None, },
                edited).Drawn(track, [], Some<Func<Key, Vector2, Node>>((_, at) => new Node.Pin(at, rule.Map(
                    horizontal: new Node.Box { Role = ImagingRoles.Needle, Width = Grown },
                    vertical: new Node.Box { Role = ImagingRoles.Needle, Height = Grown })) { Align = UnitPoint.Center })),
        };

    private static Node Tooled(FrameState state, Option<FrameTool> tool, Func<Option<Vector2>, IO<Unit>> pointer) =>
        tool.Match(
            Some: held => held.Switch(
                (State: state, Pointer: pointer),
                region: static (view, region) => Region(view.State, region, view.Pointer),
                cage: static (view, cage) => Cage(view.State, cage, view.Pointer),
                pick: static (view, pick) => new Handles<Option<Vector2>>(None, new Points<Option<Vector2>>(
                        static at => at.Map(static held => Seq((Picked, held))).IfNone([]),
                        static (at, targets, _) => (targets.Last.Map(static target => target.To) | at, targets.Map(static target => target.Key)),
                        Spoken) {
                    Placed = Some<Func<Option<Vector2>, Vector2, Option<Key>, Option<(Option<Vector2> Value, Key Key)>>>(static (_, at, _) => Some((Some(at), Picked))),
                },
                    edit => edit.Settled.Bind(static at => at).Match(Some: pick.Picked, None: static () => IO.pure(unit))) {
                    Hover = Some(view.Pointer),
                    GroundCursor = Cursor.Crosshair,
                }.Drawn(view.State.View, [])),
            None: () => new Handles<Unit>(unit, new Points<Unit>(static _ => [], static (held, targets, _) => (held, targets.Map(static target => target.Key)), Spoken), static _ => IO.pure(unit)) {
                Hover = Some(pointer),
            }.Drawn(state.View, []));

    private static Node Region(FrameState state, FrameTool.Region region, Func<Option<Vector2>, IO<Unit>> pointer) {
        Vector2 extent = new(state.Current.Extent.Width, state.Current.Extent.Height);
        return new Handles<FrameRegion>(region.Value, new Points<FrameRegion>(
                held => Anchors.Map((anchor, index) => ((Key)new Key.Ordinal((uint)index), Vector2.Lerp(new Vector2(held.Left, held.Top), new Vector2(held.Right, held.Bottom), anchor) * extent)),
                (held, targets, _) => (targets.Fold(held, (area, target) => (from index in Index(target.Key)
                                                                             from anchor in Anchors.At(index)
                                                                             from moved in Resized(area, anchor, target.To / extent, region.Aspect, extent).ToOption()
                                                                             select moved).IfNone(area)), targets.Map(static target => target.Key)),
                Spoken),
            region.Edited) {
            Hover = Some(pointer),
        }.Drawn(state.View, [new Node.Mark(new Figure.Region(
            Some(AxisRange.Create(region.Value.Left * extent.X, region.Value.Right * extent.X)),
            Some(AxisRange.Create(region.Value.Top * extent.Y, region.Value.Bottom * extent.Y)))) { Role = ImagingRoles.Ring }]);
    }

    private static Fin<FrameRegion> Resized(FrameRegion area, Vector2 anchor, Vector2 to, Option<PixelExtent> aspect, Vector2 extent) {
        SKRect start = area.Edges;
        SKRect moved = (anchor == new Vector2(0.5f)) switch {
            true => SKRect.Create(
                float.Clamp(to.X - (start.Width / 2f), 0f, 1f - start.Width), float.Clamp(to.Y - (start.Height / 2f), 0f, 1f - start.Height), start.Width, start.Height),
            false => new SKRect(
                anchor.X is 0f ? to.X : start.Left, anchor.Y is 0f ? to.Y : start.Top,
                anchor.X is 1f ? to.X : start.Right, anchor.Y is 1f ? to.Y : start.Bottom),
        };
        return FrameRegion.Spanning(aspect.Match(
            Some: ratio => ((float)ratio.Width / ratio.Height) switch {
                var wide when anchor.X is not 0.5f => (moved.Width * extent.X / wide / extent.Y) switch {
                    var tall => anchor.Y is 0f ? moved with { Top = moved.Bottom - tall } : moved with { Bottom = moved.Top + tall },
                },
                var wide when anchor.Y is not 0.5f => moved with { Right = moved.Left + (moved.Height * extent.Y * wide / extent.X) },
                _ => moved,
            },
            None: () => moved));
    }

    private static Node Cage(FrameState state, FrameTool.Cage cage, Func<Option<Vector2>, IO<Unit>> pointer) =>
        new Handles<FrameQuad>(cage.Value, new Points<FrameQuad>(
                static quad => Corners(quad).Map(static (corner, index) => ((Key)new Key.Ordinal((uint)index), corner)),
                static (quad, targets, _) => (Corners(quad).Map((corner, index) => targets.Find(target => Index(target.Key) == Some(index)).Map(static target => target.To).IfNone(corner)) switch {
                    [var topLeft, var topRight, var bottomRight, var bottomLeft] => ValidationResults.ToFin(FrameQuad.Validate(Point(topLeft), Point(topRight), Point(bottomRight), Point(bottomLeft), out FrameQuad? bent), bent).IfFail(quad),
                    _ => quad,
                }, targets.Map(static target => target.Key)),
                Spoken),
            cage.Edited) {
            Hover = Some(pointer),
        }.Drawn(state.View, cage.Patches(cage.Value).Add(new Node.Mark(new Figure.Polygon(Corners(cage.Value))) { Role = ImagingRoles.Ring }));

    private static Seq<Vector2> Corners(FrameQuad quad) =>
        Seq(quad.TopLeft, quad.TopRight, quad.BottomRight, quad.BottomLeft).Map(static corner => new Vector2((float)corner.X, (float)corner.Y));

    private static (double X, double Y) Point(Vector2 at) => (at.X, at.Y);

    private static Option<int> Index(Key key) => key is Key.Ordinal ordinal ? Some((int)ordinal.Index) : None;

    private static string Spoken(Vector2 at) => string.Create(CultureInfo.InvariantCulture, $"{at.X:F1}, {at.Y:F1}");
}
