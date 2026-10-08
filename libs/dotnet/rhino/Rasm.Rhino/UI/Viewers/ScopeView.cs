using System.Numerics;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Wacton.Unicolour;

namespace Rasm.Rhino.UI.Viewers;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct PlotAspect : IMinMaxValue<PlotAspect> {
    public static PlotAspect MinValue { get; } = new(float.BitIncrement(0f));
    public static PlotAspect MaxValue { get; } = new(float.MaxValue);
    public static PlotAspect Proportional { get; } = new(0.4f);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct TraceGain : IMinMaxValue<TraceGain> {
    public static TraceGain MinValue { get; } = new(1f);
    public static TraceGain MaxValue { get; } = new(10f);
    public static TraceGain Unity => MinValue;

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

public sealed record ScopeFrames(PixelFrame Frame, Gamut Gamut, Transfer Encoding, Option<Quantizer> Levels, Option<(PixelFrame Frame, Gamut Gamut)> Scene);

[SmartEnum]
public sealed partial class WaveformLayout {
    public static readonly WaveformLayout Luma = new(static plot => IterableNE.singleton((TraceChannel.Luma, plot)));
    public static readonly WaveformLayout Overlay = new(static plot => IterableNE.create((TraceChannel.Red, plot), (TraceChannel.Green, plot), (TraceChannel.Blue, plot)));
    public static readonly WaveformLayout Parade = new(static plot => (plot.Width / 3f) switch {
        var third => IterableNE.create(
            (TraceChannel.Red, plot with { Width = third }),
            (TraceChannel.Green, plot with { X = plot.X + third, Width = third }),
            (TraceChannel.Blue, plot with { X = plot.X + (2f * third), Width = third })),
    });

    [UseDelegateFromConstructor]
    public partial IterableNE<(TraceChannel Channel, RectangleF Column)> Columns(RectangleF plot);

    public Fin<ScopePlan> Plan(ScopeFrames frames, RectangleF plot, float scale) =>
        Plots.Device(ScopeKind.Unit(Columns(plot).Head.Column), scale).Map(extent =>
            (ScopePlan)new ScopePlan.Waveform(frames.Frame, new ScopeRequest.Waveform(frames.Gamut, extent), this, frames.Levels.Map(static levels => levels.MaxCode)));
}

[Union]
public abstract partial record ScopeTraces {
    public sealed record Histogram(ScopeReading.Histogram Shown, Option<ScopeReading.Histogram> Scene) : ScopeTraces;
    public sealed record Waveform(ScopeReading.Waveform Reading, WaveformLayout Layout, Option<float> MaxCode) : ScopeTraces;
    public sealed record Vectorscope(ScopeReading.Vectorscope Reading) : ScopeTraces;

    public sealed record Chromaticity(ScopeReading.Chromaticity Reading, Option<Gamut> Working) : ScopeTraces {
        internal static readonly Seq<Vector2> Locus = toSeq(Cmf.RequiredWavelengths)
            .Map(static nm => new Unicolour(Configuration.Default, new Spd(nm, 1, 1.0)).Chromaticity)
            .Map(static xy => new Vector2((float)xy.X, (float)xy.Y))
            .Strict();

        public static Vector2 Fit => Locus.Fold(Vector2.Zero, Vector2.Max) switch {
            var top => new(MathF.Ceiling(10f * top.X) / 10f, MathF.Ceiling(10f * top.Y) / 10f),
        };

        public static RectangleF Square(RectangleF plot) => new(plot.X, plot.Bottom - (plot.Height / Fit.Y), plot.Width / Fit.X, plot.Height / Fit.Y);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScopePlan {
    public abstract ScopeTraces Read();

    public sealed record Histogram(PixelFrame Frame, ScopeRequest.Histogram Request, Option<(PixelFrame Frame, ScopeRequest.Histogram Request)> Scene) : ScopePlan {
        public override ScopeTraces Read() => new ScopeTraces.Histogram(Request.Read(Frame), Scene.Map(static scene => scene.Request.Read(scene.Frame)));
    }

    public sealed record Waveform(PixelFrame Frame, ScopeRequest.Waveform Request, WaveformLayout Layout, Option<float> MaxCode) : ScopePlan {
        public override ScopeTraces Read() => new ScopeTraces.Waveform(Request.Read(Frame), Layout, MaxCode);
    }

    public sealed record Vectorscope(PixelFrame Frame, ScopeRequest.Vectorscope Request) : ScopePlan {
        public override ScopeTraces Read() => new ScopeTraces.Vectorscope(Request.Read(Frame));
    }

    public sealed record Chromaticity(PixelFrame Frame, ScopeRequest.Chromaticity Request, Option<Gamut> Working) : ScopePlan {
        public override ScopeTraces Read() => new ScopeTraces.Chromaticity(Request.Read(Frame), Working);
    }
}

[SmartEnum]
public sealed partial class WellShape {
    public const float Floor = 100f;

    public static readonly WellShape Banded = new(resizes: true, static (width, aspect) => float.Max(aspect * width, Floor));
    public static readonly WellShape Square = new(resizes: false, static (width, _) => width);
    public static readonly WellShape Diagram = new(resizes: false, static (width, _) =>
        ((width - (2f * Plots.Inset)) * ScopeTraces.Chromaticity.Fit.Y / ScopeTraces.Chromaticity.Fit.X) + (2f * Plots.Inset));

    public bool Resizes { get; }

    [UseDelegateFromConstructor]
    public partial float Height(float width, PlotAspect aspect);
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScopeKind {
    public static readonly ScopeKind Histogram = new("histogram", WellShape.Banded, static (frames, _, _, _) =>
        new ScopePlan.Histogram(
            frames.Frame, new ScopeRequest.Histogram(frames.Gamut, frames.Levels.Map(static levels => levels.Clipping)),
            frames.Scene.Map(static scene => (scene.Frame, new ScopeRequest.Histogram(scene.Gamut, None)))));
    public static readonly ScopeKind WaveformLuma = new("waveform-luma", WellShape.Banded, static (frames, plot, scale, _) => WaveformLayout.Luma.Plan(frames, plot, scale));
    public static readonly ScopeKind WaveformRgb = new("waveform-rgb", WellShape.Banded, static (frames, plot, scale, _) => WaveformLayout.Overlay.Plan(frames, plot, scale));
    public static readonly ScopeKind Parade = new("parade", WellShape.Banded, static (frames, plot, scale, _) => WaveformLayout.Parade.Plan(frames, plot, scale));
    public static readonly ScopeKind Vectorscope = new("vectorscope", WellShape.Square, static (frames, plot, scale, zoom) =>
        Plots.Device(Unit(plot), scale).Map(extent => (ScopePlan)new ScopePlan.Vectorscope(frames.Frame, new ScopeRequest.Vectorscope(frames.Gamut, extent, zoom))));
    public static readonly ScopeKind Chromaticity = new("chromaticity", WellShape.Diagram, static (frames, plot, scale, _) =>
        Plots.Device(Unit(ScopeTraces.Chromaticity.Square(plot)), scale).Map(extent => (ScopePlan)new ScopePlan.Chromaticity(
            frames.Frame, new ScopeRequest.Chromaticity(frames.Gamut, frames.Encoding, extent), frames.Scene.Map(static scene => scene.Gamut))));

    public WellShape Shape { get; }

    [UseDelegateFromConstructor]
    public partial Fin<ScopePlan> Plan(ScopeFrames frames, RectangleF plot, float scale, ChromaZoom zoom);

    internal static PlotPlane.Cartesian Unit(RectangleF rectangle) =>
        new(rectangle, new Axis.Linear(AxisRange.Unit), new Axis.Linear(AxisRange.Unit));
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CountScale {
    public static readonly CountScale Linear = new("linear", static range => new Axis.Linear(range));
    public static readonly CountScale Logarithmic = new("logarithmic", static range => new Axis.Logarithmic(range));

    [UseDelegateFromConstructor]
    public partial Axis Spanning(AxisRange range);
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HistogramRange {
    public static readonly HistogramRange Full = new("full", HistogramAxis.Bins);
    public static readonly HistogramRange Sdr = new("sdr", HistogramAxis.Bin(1f));

    public int Bins { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TraceMix {
    public static readonly TraceMix RgbLuma = new("rgb-luma", Seq(TraceChannel.Luma, TraceChannel.Red, TraceChannel.Green, TraceChannel.Blue));
    public static readonly TraceMix Rgb = new("rgb", Seq(TraceChannel.Red, TraceChannel.Green, TraceChannel.Blue));
    public static readonly TraceMix Luma = new("luma", Seq(TraceChannel.Luma));
    public static readonly TraceMix Red = new("red", Seq(TraceChannel.Red));
    public static readonly TraceMix Green = new("green", Seq(TraceChannel.Green));
    public static readonly TraceMix Blue = new("blue", Seq(TraceChannel.Blue));

    public Seq<TraceChannel> Channels { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TraceBrightness {
    public static readonly TraceBrightness Bright = new("bright", 125);
    public static readonly TraceBrightness Normal = new("normal", 100);
    public static readonly TraceBrightness Dimmed = new("dimmed", 50);

    public int Step { get; }
}

public sealed record ScopeOptions(ScopeKind Kind, CountScale Counts, HistogramRange Range, TraceMix Channels, TraceBrightness Brightness) {
    public static ScopeOptions Default { get; } = new(ScopeKind.Histogram, CountScale.Linear, HistogramRange.Full, TraceMix.RgbLuma, TraceBrightness.Normal);
}

[SmartEnum]
public sealed partial class VectorTarget {
    public static readonly VectorTarget Red = new(Color.FromArgb(0xFF, 0x00, 0x00));
    public static readonly VectorTarget Magenta = new(Color.FromArgb(0xFF, 0x00, 0xFF));
    public static readonly VectorTarget Blue = new(Color.FromArgb(0x00, 0x00, 0xFF));
    public static readonly VectorTarget Cyan = new(Color.FromArgb(0x00, 0xFF, 0xFF));
    public static readonly VectorTarget Green = new(Color.FromArgb(0x00, 0xFF, 0x00));
    public static readonly VectorTarget Yellow = new(Color.FromArgb(0xFF, 0xFF, 0x00));

    public Color Paint { get; }

    public Vector3 Drive => 0.75f * new Vector3(Paint.R, Paint.G, Paint.B);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScopeEdit {
    public ScopeOptions Chosen => Switch(chart: static chart => chart.Options, points: static _ => ScopeOptions.Default);
    public PlotAspect WellAspect => Switch(chart: static chart => chart.Aspect, points: static _ => PlotAspect.Proportional);
    public Option<Levels> Pair => Switch(chart: static _ => Option<Levels>.None, points: static points => Some(points.Value));

    public sealed record Chart(ScopeOptions Options, PlotAspect Aspect) : ScopeEdit;
    public sealed record Points(Levels Value) : ScopeEdit;
}

[Union]
public abstract partial record ScopePart {
    public sealed record Plot(RectangleF Bounds) : ScopePart;
    public sealed record Clip(ClipEnd End) : ScopePart;
    public sealed record Grip(RectangleF Well) : ScopePart;
    public sealed record Strip(RectangleF Bounds) : ScopePart;
    public sealed record Level(LevelsParameter Handle, RectangleF Bounds) : ScopePart;
}

public sealed record ScopeMarks(Option<(int Bin, ScopeReading.Histogram Reading)> Hovered, Option<BinRange> Selection, Option<ClipEnd> Clips);

public sealed record ScopeViewState(
    ScopeEdit Value, Option<ScopeTraces> Traces, TraceGain Gain, ChromaZoom Zoom,
    Option<BinRange> Selection, Option<ClipEnd> Clips, Option<LevelsParameter> Held);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ScopeView : ComponentControl<ScopeViewState, ScopePart, ScopeEdit> {
    // --- [STATE]
    public const float GripHeight = 5f;
    public const float StripHeight = 10f;

    private readonly string caption;
    private readonly IO<Option<ScopeFrames>> frames;
    private readonly Atom<Option<Run>> run;

    public ScopeView(IPlugInSink sink, string caption, ScopeEdit value, IO<Option<ScopeFrames>> frames)
        : this(sink, caption, value, frames, Atom(Option<Run>.None)) { }

    private ScopeView(IPlugInSink sink, string caption, ScopeEdit value, IO<Option<ScopeFrames>> frames, Atom<Option<Run>> run)
        : base(sink, new ScopeViewState(value, None, TraceGain.Unity, ChromaZoom.Normal, None, None, None), Some(Sampled(run))) =>
        (this.caption, this.frames, this.run) = (caption, frames, run);

    public IO<ScopeMarks> Marks =>
        IO.lift(() => new ScopeMarks(
            from at in Pointer
            from window in Optional(ParentWindow)
            from part in Plots.Hit(Layout(State, Size, window.LogicalPixelSize), at)
            from bin in Bin(State, part, at)
            from shown in HistogramOf(State)
            select (Bin: bin, Reading: shown.Shown),
            State.Selection,
            State.Clips));

    protected override ScopeViewState Received(ScopeViewState state, Option<ScopeEdit> value) =>
        value.Map(shown => shown.Chosen.Kind == state.Value.Chosen.Kind
                ? state with { Value = shown }
                : state with { Value = shown, Traces = None, Selection = None, Clips = None, Gain = TraceGain.Unity })
            .IfNone(state);

    protected override Option<float> Fitted(ScopeViewState state, float width) =>
        Placed(state, width) switch {
            var (well, plot) => well.Height + Footer(state, well, plot).Map(static footer => footer.Bounds.Height).IfNone(0f),
        };

    // --- [RUN]
    private sealed record Run(ScopeKind Kind, ScopePlan Requested, Option<ScopeTraces> Shown);

    private static Func<Control, IO<Func<ScopeViewState, ScopeViewState>>> Sampled(Atom<Option<Run>> run) =>
        _ => run.ValueIO.Map(static held => (Func<ScopeViewState, ScopeViewState>)(state =>
            state with { Traces = held.Filter(answered => answered.Kind == state.Value.Chosen.Kind).Bind(static answered => answered.Shown) }));

    protected override IO<Unit> Reconcile(ScopeViewState before, ScopeViewState after, Interaction<ScopePart> was, Interaction<ScopePart> now) =>
        from scale in IO.lift(() => Optional(ParentWindow).Map(static window => window.LogicalPixelSize))
        from source in frames
        from width in IO.lift(() => (float)Width)
        from requested in (from held in scale
                           from shown in source
                           from plan in after.Value.Chosen.Kind.Plan(shown, Placed(after, width).Plot, held, after.Zoom).ToOption()
                           where !run.Value.Exists(current => current.Requested == plan)
                           select Requested(after.Value.Chosen.Kind, plan)).IfNone(IO.pure(unit))
        select unit;

    private IO<Unit> Requested(ScopeKind kind, ScopePlan plan) =>
        from requested in run.SwapIO(held => Some(new Run(kind, plan, held.Filter(current => current.Kind == kind).Bind(static current => current.Shown))))
        from forked in IO.lift(() => Callbacks.Succeeded(Answered(plan), new CallbackSite(Sink, typeof(ScopeView), nameof(ScopePlan.Read)))).Fork()
        select unit;

    private IO<Unit> Answered(ScopePlan plan) =>
        from traces in IO.lift(plan.Read)
        from answered in IO.lift(() => run.SwapMaybe(held => held.Filter(current => current.Requested == plan).Map(current => Some(current with { Shown = Some(traces) }))))
        select unit;

    // --- [LAYOUT]
    private const float ClipSide = 9f;
    private const float ClipDimmed = 0.7f;
    private static readonly SizeF HandleSize = new(10f, 5f);
    private static readonly MarkStyle HandleFill = new MarkStyle.Fill(new MarkColor.Themed(PaintSlot.ControlText), 1f);
    private static readonly MarkStyle Outlined = new MarkStyle.Stroke(new MarkColor.Themed(PaintSlot.ControlText), 1f, 1f);

    protected override Seq<PlotMark<ScopePart>> Layout(ScopeViewState state, SizeF size, float scale) =>
        Placed(state, size.Width) switch {
            var (well, plot) => Seq(Ground(well, new ScopePart.Plot(plot)))
                .Concat(Footer(state, well, plot).Map(static footer => Ground(footer.Bounds, footer.Part)).ToSeq())
                .Concat(ClipMarks(state, plot))
                .Concat(Handles(state, well, plot)),
        };

    protected override PartFacet Facet(ScopeViewState state, ScopePart key) =>
        key.Switch(
            (State: state, Caption: caption),
            plot: static (held, _) => new PartFacet(
                PartRole.Image,
                held.State.Value is ScopeEdit.Chart && HistogramOf(held.State).IsSome ? Cursors.Crosshair : Cursors.Default,
                held.Caption,
                Some(Caption(held.State.Value.Chosen.Kind)),
                None),
            clip: static (held, clip) => new PartFacet(
                PartRole.Button,
                Cursors.Pointer,
                RowText.Localize(clip.End.Below ? "Low Clip" : "High Clip").Local,
                Some(RowText.Localize(held.State.Clips.Exists(end => Covers(end, clip.End)) ? "Shown on Frame" : "Hidden").Local),
                None),
            grip: static (held, _) => new PartFacet(
                PartRole.Handle,
                Cursors.HorizontalSplit,
                RowText.Localize("Plot Height").Local,
                Some(RowText.Localize("{0:P0}", arguments: [(float)held.State.Value.WellAspect]).Local),
                None),
            strip: static (_, _) => new PartFacet(PartRole.Image, Cursors.Default, RowText.Localize("Levels").Local, None, None),
            level: static (held, level) => new PartFacet(
                PartRole.Handle,
                Cursors.VerticalSplit,
                RowText.Localize(level.Handle.Map(black: "Black Level", white: "White Level")).Local,
                held.State.Value.Pair.Map(pair => level.Handle.Map(
                    black: FieldTexts.Number(LevelsParameter.BlackPoint.Presentation).Shown(pair.Black),
                    white: FieldTexts.Number(LevelsParameter.WhitePoint.Presentation).Shown(pair.White))),
                None));

    private static (RectangleF Well, RectangleF Plot) Placed(ScopeViewState state, float width) =>
        new RectangleF(0f, 0f, width, state.Value.Chosen.Kind.Shape.Height(width, state.Value.WellAspect)) switch {
            var well => (well, Plots.Plot(well)),
        };

    private static Option<(RectangleF Bounds, ScopePart Part)> Footer(ScopeViewState state, RectangleF well, RectangleF plot) =>
        state.Value.Switch(
            (Well: well, Plot: plot, state.Value.Chosen.Kind.Shape),
            chart: static (held, _) => held.Shape.Resizes
                ? Some((new RectangleF(held.Well.Left, held.Well.Bottom, held.Well.Width, GripHeight), (ScopePart)new ScopePart.Grip(held.Well)))
                : None,
            points: static (held, _) => Some((new RectangleF(held.Plot.Left, held.Well.Bottom, held.Plot.Width, StripHeight), (ScopePart)new ScopePart.Strip(held.Plot))));

    private static PlotMark<ScopePart> Ground(RectangleF bounds, ScopePart part) =>
        new(new MarkShape.Band(bounds), None, new MarkPart<ScopePart>(part, 0f));

    private static Seq<PlotMark<ScopePart>> ClipMarks(ScopeViewState state, RectangleF plot) =>
        from clipped in HistogramOf(state).Bind(static histogram => histogram.Shown.Clipped).ToSeq()
        from mark in Seq(
            (End: ClipEnd.Low, Counts: clipped.Low, Box: new RectangleF(plot.Left, plot.Top, ClipSide, ClipSide)),
            (End: ClipEnd.High, Counts: clipped.High, Box: new RectangleF(plot.Right - ClipSide, plot.Top, ClipSide, ClipSide)))
        select new PlotMark<ScopePart>(
            Triangle(mark.Box),
            Lit(mark.Counts, state.Clips.Exists(held => Covers(held, mark.End))),
            new MarkPart<ScopePart>(new ScopePart.Clip(mark.End), Plots.Reach));

    private static Seq<PlotMark<ScopePart>> Handles(ScopeViewState state, RectangleF well, RectangleF plot) =>
        from pair in state.Value.Pair.ToSeq()
        from handle in toSeq(LevelsParameter.Items)
        select new PlotMark<ScopePart>(
            Triangle(new RectangleF(LevelX(state, plot, Level(handle, pair)) - (HandleSize.Width / 2f), well.Bottom, HandleSize.Width, HandleSize.Height)),
            handle.Map(black: HandleFill, white: Outlined),
            new MarkPart<ScopePart>(new ScopePart.Level(handle, plot), Plots.Reach));

    private static MarkShape.Polygon Triangle(RectangleF box) =>
        new([new PointF(box.Left, box.Bottom), new PointF(box.Left + (box.Width / 2f), box.Top), new PointF(box.Right, box.Bottom)]);

    private static MarkStyle.Fill Lit(ChannelCounts counts, bool shown) =>
        new Vector3(int.Sign(counts.Red), int.Sign(counts.Green), int.Sign(counts.Blue)) switch {
            var lit when lit == Vector3.Zero => new MarkStyle.Fill(new MarkColor.Themed(PaintSlot.DisabledText), ClipDimmed),
            var lit => new MarkStyle.Fill(new MarkColor.Fixed(Shade(lit)), shown ? 1f : ClipDimmed),
        };

    private static Option<ScopeTraces.Histogram> HistogramOf(ScopeViewState state) =>
        state.Traces.Bind(static traces => traces is ScopeTraces.Histogram histogram ? Some(histogram) : None);

    private static bool Covers(ClipEnd held, ClipEnd end) => (held.Below && end.Below) || (held.Above && end.Above);

    private static Option<int> Bin(ScopeViewState state, ScopePart part, PointF at) =>
        part.Switch(
            (State: state, At: at),
            plot: static (held, plot) => HistogramOf(held.State).Map(_ => Binned(held.State, plot.Bounds, held.At.X)),
            clip: static (_, _) => Option<int>.None,
            grip: static (_, _) => Option<int>.None,
            strip: static (held, strip) => Some(Binned(held.State, strip.Bounds, held.At.X)),
            level: static (held, level) => Some(Binned(held.State, level.Bounds, held.At.X)));

    private static int Binned(ScopeViewState state, RectangleF plot, float x) =>
        state.Value.Chosen.Range.Bins switch {
            var bins => int.Clamp((int)MathF.Floor((x - plot.Left) / plot.Width * bins), 0, bins - 1),
        };

    private static int Located(ScopeViewState state, float level) => int.Min(HistogramAxis.Bin(level), state.Value.Chosen.Range.Bins - 1);

    private static float LevelX(ScopeViewState state, RectangleF plot, float level) =>
        plot.Left + (Located(state, level) * plot.Width / state.Value.Chosen.Range.Bins);

    private static float Level(LevelsParameter handle, Levels pair) => handle.Map(black: pair.Black, white: pair.Black + float.Exp2(pair.White));

    // --- [TRANSITIONS]
    private const float GainStep = 120f * 0.00125f;
    private static readonly Func<ScopeEdit, Edit<ScopeEdit>> AsPreview = static value => new Edit<ScopeEdit>.Preview(value);
    private static readonly Func<ScopeEdit, Edit<ScopeEdit>> AsStep = static value => new Edit<ScopeEdit>.Step(value);

    protected override Option<Transition<ScopeViewState, ScopeEdit>> Pressed(ScopeViewState state, SizeF size, float scale, Option<ScopePart> part, MouseEventArgs e) =>
        part.Bind(key => key.Switch(
            (State: state, Key: key, At: e.Location, Both: Snaps(e.Modifiers)),
            plot: static (held, _) =>
                from chart in Some(held.State.Value).Filter(static value => value is ScopeEdit.Chart)
                from bin in Bin(held.State, held.Key, held.At)
                from range in Spanned(bin, bin)
                select new Transition<ScopeViewState, ScopeEdit>(held.State with { Selection = range }, None),
            clip: static (held, clip) => Some(new Transition<ScopeViewState, ScopeEdit>(held.State with { Clips = Toggled(held.State.Clips, held.Both ? ClipEnd.Both : clip.End) }, None)),
            grip: static (held, _) => Some(new Transition<ScopeViewState, ScopeEdit>(held.State, None)),
            strip: static (held, strip) =>
                from pair in held.State.Value.Pair
                from bin in Bin(held.State, held.Key, held.At)
                from handle in Nearer(held.State, strip.Bounds, held.At.X, pair)
                select Moved(held.State with { Held = handle }, handle, bin, AsPreview),
            level: static (held, level) => Some(new Transition<ScopeViewState, ScopeEdit>(held.State with { Held = level.Handle }, None))));

    protected override Transition<ScopeViewState, ScopeEdit> Dragged(ScopeViewState state, SizeF size, float scale, Option<ScopePart> part, PointF origin, PointF previous, MouseEventArgs e) =>
        part.Map(key => key.Switch(
                (State: state, Key: key, Origin: origin, At: e.Location),
                plot: static (held, _) => new Transition<ScopeViewState, ScopeEdit>(
                    held.State with {
                        Selection = (from first in Bin(held.State, held.Key, held.Origin)
                                     from last in Bin(held.State, held.Key, held.At)
                                     from range in Spanned(first, last)
                                     select range) | held.State.Selection,
                    },
                    None),
                clip: static (held, _) => new Transition<ScopeViewState, ScopeEdit>(held.State, None),
                grip: static (held, grip) => Edited(held.State, Resized(held.State, grip.Well, held.At.Y - held.Origin.Y), AsPreview),
                strip: static (held, _) => Dragging(held.State, held.Key, held.At),
                level: static (held, _) => Dragging(held.State, held.Key, held.At)))
            .IfNone(new Transition<ScopeViewState, ScopeEdit>(state, None));

    protected override Transition<ScopeViewState, ScopeEdit> Released(ScopeViewState state, SizeF size, float scale, Option<ScopePart> part, MouseEventArgs e) =>
        new(state with { Held = None }, None);

    protected override Option<Transition<ScopeViewState, ScopeEdit>> DoubleClicked(ScopeViewState state, SizeF size, float scale, Option<ScopePart> part, MouseEventArgs e) =>
        part.Filter(static key => key is ScopePart.Plot).Bind(_ => state.Value.Chosen.Kind.Shape.Switch(
            state,
            banded: static held => Some(new Transition<ScopeViewState, ScopeEdit>(held with { Gain = TraceGain.Unity }, None)),
            square: static held => Some(new Transition<ScopeViewState, ScopeEdit>(held with { Zoom = held.Zoom.Map(normal: ChromaZoom.Doubled, doubled: ChromaZoom.Normal) }, None)),
            diagram: static _ => Option<Transition<ScopeViewState, ScopeEdit>>.None));

    protected override Option<Transition<ScopeViewState, ScopeEdit>> Scrolled(ScopeViewState state, SizeF size, float scale, Option<ScopePart> part, MouseEventArgs e) =>
        part.Filter(static key => key is ScopePart.Plot).Bind(_ => state.Value.Chosen.Kind.Shape.Switch(
            (State: state, Notches: e.Delta.Height),
            banded: static held => Some(new Transition<ScopeViewState, ScopeEdit>(
                held.State with { Gain = Conversions.Validated<TraceGain, float, InvalidRhinoValue>(held.State.Gain + (GainStep * held.Notches)).IfFail(held.State.Gain) },
                None)),
            square: static _ => Option<Transition<ScopeViewState, ScopeEdit>>.None,
            diagram: static _ => Option<Transition<ScopeViewState, ScopeEdit>>.None));

    protected override Option<Transition<ScopeViewState, ScopeEdit>> KeyPressed(ScopeViewState state, SizeF size, float scale, Option<ScopePart> part, KeyEventArgs e) =>
        e.Key == Keys.Escape && (state.Selection.IsSome || state.Clips.IsSome)
            ? Some(new Transition<ScopeViewState, ScopeEdit>(state with { Selection = None, Clips = None }, None))
            : None;

    protected override Option<Transition<ScopeViewState, ScopeEdit>> Stepped(ScopeViewState state, SizeF size, float scale, ScopePart part, int steps) =>
        part.Switch(
            (State: state, Steps: steps),
            plot: static (_, _) => Option<Transition<ScopeViewState, ScopeEdit>>.None,
            clip: static (_, _) => Option<Transition<ScopeViewState, ScopeEdit>>.None,
            grip: static (held, grip) => Some(Edited(held.State, Resized(held.State, grip.Well, held.Steps * GripHeight), AsStep)),
            strip: static (_, _) => Option<Transition<ScopeViewState, ScopeEdit>>.None,
            level: static (held, level) => held.State.Value.Pair.Map(pair => Moved(
                held.State,
                level.Handle,
                int.Clamp(Located(held.State, Level(level.Handle, pair)) + held.Steps, 0, held.State.Value.Chosen.Range.Bins - 1),
                AsStep)));

    protected override Option<Transition<ScopeViewState, ScopeEdit>> Activated(ScopeViewState state, SizeF size, float scale, ScopePart part) =>
        part is ScopePart.Clip clip ? Some(new Transition<ScopeViewState, ScopeEdit>(state with { Clips = Toggled(state.Clips, clip.End) }, None)) : None;

    private static Option<ClipEnd> Toggled(Option<ClipEnd> held, ClipEnd end) =>
        (Below: held.Exists(static shown => shown.Below) != end.Below, Above: held.Exists(static shown => shown.Above) != end.Above) switch {
            var ends => toSeq(ClipEnd.Items).Find(item => item.Below == ends.Below && item.Above == ends.Above),
        };

    private static Option<LevelsParameter> Nearer(ScopeViewState state, RectangleF plot, float x, Levels pair) =>
        toSeq(toSeq(LevelsParameter.Items).OrderBy(handle => MathF.Abs(LevelX(state, plot, Level(handle, pair)) - x))).Head;

    private static Fin<Levels> Shifted(LevelsParameter handle, Levels pair, float level) =>
        handle.Switch(
            (Pair: pair, Level: level),
            black: static held =>
                from black in Conversions.Validated<BlackLevel, float, InvalidGrade>(held.Level)
                from white in Conversions.Validated<Exposure, float, InvalidToneValue>(float.Log2(Level(LevelsParameter.White, held.Pair) - held.Level))
                select new Levels(black, white),
            white: static held =>
                Conversions.Validated<Exposure, float, InvalidToneValue>(float.Log2(held.Level - held.Pair.Black)).Map(white => held.Pair with { White = white }));

    private static Transition<ScopeViewState, ScopeEdit> Moved(ScopeViewState state, LevelsParameter handle, int bin, Func<ScopeEdit, Edit<ScopeEdit>> edit) =>
        Edited(
            state,
            from pair in state.Value.Pair
            from moved in Shifted(handle, pair, HistogramAxis.LowerBound(bin)).ToOption()
            select (ScopeEdit)new ScopeEdit.Points(moved),
            edit);

    private static Transition<ScopeViewState, ScopeEdit> Dragging(ScopeViewState state, ScopePart part, PointF at) =>
        (from handle in state.Held
         from bin in Bin(state, part, at)
         select Moved(state, handle, bin, AsPreview))
            .IfNone(new Transition<ScopeViewState, ScopeEdit>(state, None));

    private static Option<BinRange> Spanned(int first, int last) =>
        Callbacks.Found(BinRange.Validate(int.Min(first, last), int.Max(first, last), out BinRange range) is null, range);

    private static Option<ScopeEdit> Resized(ScopeViewState state, RectangleF well, float rise) =>
        Conversions.Validated<PlotAspect, float, InvalidRhinoValue>(float.Max(well.Height + rise, WellShape.Floor) / well.Width)
            .ToOption()
            .Map(aspect => (ScopeEdit)new ScopeEdit.Chart(state.Value.Chosen, aspect));

    private static new Transition<ScopeViewState, ScopeEdit> Edited(ScopeViewState state, Option<ScopeEdit> value, Func<ScopeEdit, Edit<ScopeEdit>> edit) =>
        value.Map(shown => new Transition<ScopeViewState, ScopeEdit>(state with { Value = shown }, Some(edit(shown))))
            .IfNone(new Transition<ScopeViewState, ScopeEdit>(state, None));

    // --- [TEXT]
    public static string Caption(ScopeKind kind) =>
        RowText.Localize(kind.Map(
            histogram: "Histogram",
            waveformLuma: "Waveform Luma",
            waveformRgb: "Waveform RGB",
            parade: "Parade",
            vectorscope: "Vectorscope",
            chromaticity: "Chromaticity")).Local;

    // --- [PAINT]
    private const int Divisions = 10;
    private const float ChannelFill = 0.75f;
    private const float Intensity = 2.56f / 256f;
    private const float SkinDegrees = 123f;
    private const float TargetDegrees = 2.5f;
    private const float TargetChroma = 2.5f / 200f;
    private const float TargetAlpha = 126f / 255f;
    private const float WhiteShare = 0.015f;
    private const float DisplayAlpha = 0.5f;
    private const float WorkingAlpha = 0.7f;
    private static readonly SizeF LabelOffset = new(13f, 16f);
    private static readonly (float Least, float Most) WhiteSide = (3f, 6f);
    private static readonly Seq<float> Quarters = toSeq(Range(1, 4)).Map(static quarter => quarter / 4f);
    private static readonly MarkStyle Split = new MarkStyle.Stroke(new MarkColor.Themed(PaintSlot.ControlText), 0.5f, 1f);

    protected override IO<Unit> Draw(PlotCanvas canvas, RectangleF bounds, ScopeViewState state, Seq<PlotMark<ScopePart>> marks, Interaction<ScopePart> interaction) =>
        Placed(state, bounds.Width) switch {
            var (well, plot) =>
                from ground in Plots.Well(canvas, well)
                from traced in state.Traces.Match(Some: traces => Traced(canvas, state, plot, traces), None: static () => IO.pure(unit))
                from marked in Plots.Paint(canvas, marks.Concat(Cue(marks, interaction)))
                select unit,
        };

    private static Seq<PlotMark<ScopePart>> Cue(Seq<PlotMark<ScopePart>> marks, Interaction<ScopePart> interaction) =>
        (from grip in marks.Choose(static mark => mark.Part.Bind(static part => part.Key is ScopePart.Grip held ? Some(held) : None)).Head
         from cued in interaction.Pressed | interaction.Hovered
         let foot = grip.Well.Bottom + GripHeight - (GripHeight * 3f / 4f)
         select new PlotMark<ScopePart>(
             new MarkShape.Rule(new PointF(grip.Well.Left + (grip.Well.Width * 3f / 8f), foot), new PointF(grip.Well.Left + (grip.Well.Width * 5f / 8f), foot)),
             new MarkStyle.Stroke(new MarkColor.Themed(PaintSlot.ContentTextEnabled), cued is ScopePart.Grip ? 1f : 0.5f, GripHeight / 2f),
             None)).ToSeq();

    private static IO<Unit> Traced(PlotCanvas canvas, ScopeViewState state, RectangleF plot, ScopeTraces traces) =>
        traces.Switch(
            (Canvas: canvas, State: state, Plot: plot),
            histogram: static (held, histogram) => HistogramPaint(held.Canvas, held.State, held.Plot, histogram),
            waveform: static (held, waveform) => WaveformPaint(held.Canvas, held.State, held.Plot, waveform),
            vectorscope: static (held, vectorscope) => VectorscopePaint(held.Canvas, held.State, held.Plot, vectorscope),
            chromaticity: static (held, chromaticity) => ChromaticityPaint(held.Canvas, held.Plot, chromaticity));

    private static IO<Unit> HistogramPaint(PlotCanvas canvas, ScopeViewState state, RectangleF plot, ScopeTraces.Histogram traces) =>
        state.Value.Chosen switch {
            var options =>
                from range in IO.lift(Reaching(options.Range.Bins))
                let ground = new PlotPlane.Cartesian(plot, new Axis.Linear(range), new Axis.Linear(AxisRange.Unit))
                from extent in IO.lift(Plots.Device(ground, canvas.Scale))
                let shown = options.Channels.Channels.Map(channel => (Channel: channel, Counts: channel.Bins(traces.Shown.Bins).Counts[..options.Range.Bins]))
                let scene = traces.Scene.Map(reading => TraceChannel.Luma.Bins(reading.Bins).Counts[..options.Range.Bins])
                let counted = Reaching(shown.Map(static held => held.Counts).Concat(scene.ToSeq()).Map(Peak).Fold(0, int.Max) / state.Gain).ToOption().Map(options.Counts.Spanning)
                let fills = from axis in counted.ToSeq()
                            from held in shown
                            select (
                                held.Channel,
                                Ink: Ink(canvas, new MarkColor.Trace(held.Channel)) * (held.Channel.Lane.IsSome ? ChannelFill : 1f - ChannelFill),
                                Heights: Heights(held.Counts, extent.Width, axis))
                let outline = from axis in counted
                              from held in scene
                              select Heights(held, extent.Width, axis)
                from filled in Rastered(canvas, plot, extent, point => fills.Fold(Vector3.Zero, (sum, fill) =>
                    (plot.Bottom - point.Y) / plot.Height < fill.Heights[(int)((point.X - plot.Left) / plot.Width * fill.Heights.Length)] ? sum + fill.Ink : sum))
                from marked in Plots.Paint(canvas, Selected(state, ground).Concat(Graticule(ground, options.Range.Bins)).Concat(Edges(plot, fills, outline)))
                from labelled in Plots.Labels(canvas, ground, LabelSide.Bottom, Ticks(options.Range.Bins)
                    .Filter(tick => options.Range.Bins <= HistogramAxis.Bin(1f) || tick.Level >= 1f)
                    .Map(static tick => new AxisLabel(HistogramAxis.Bin(tick.Level), tick.Level.ToString(tick.Format, RowText.Culture))))
                select unit,
        };

    private static Seq<PlotMark<ScopePart>> Selected(ScopeViewState state, PlotPlane.Cartesian ground) =>
        state.Selection.Map(range => new PlotMark<ScopePart>(
            new MarkShape.Band(RectangleF.FromSides(ground.Point(range.First, 0f).X, ground.Plot.Top, ground.Point(range.Last + 1, 0f).X, ground.Plot.Bottom)),
            MarkStyle.Selection,
            None)).ToSeq();

    private static Seq<PlotMark<ScopePart>> Graticule(PlotPlane.Cartesian ground, int bins) =>
        Ticks(bins) switch {
            var ticks => Plots.Rules(ground, Orientation.Vertical, ticks.Map(static tick => (float)HistogramAxis.Bin(tick.Level)))
                .Zip(ticks, static (shape, tick) => new PlotMark<ScopePart>(shape, tick.Level == 1f ? Split : MarkStyle.Grid, None))
                .Concat(Plots.Rules(ground, Orientation.Horizontal, Quarters).Map(static shape => new PlotMark<ScopePart>(shape, MarkStyle.Grid, None))),
        };

    private static Seq<(float Level, string Format)> Ticks(int bins) =>
        Quarters.Map(static level => (Level: level, Format: "F2"))
            .Concat(toSeq(Range(1, HistogramAxis.TopStop)).Map(static stop => (Level: float.Exp2(stop), Format: "0")))
            .Filter(tick => HistogramAxis.Bin(tick.Level) <= bins);

    private static Seq<PlotMark<ScopePart>> Edges(RectangleF plot, Seq<(TraceChannel Channel, Vector3 Ink, float[] Heights)> fills, Option<float[]> scene) =>
        fills.Filter(static fill => fill.Channel.Lane.IsSome)
            .Map(fill => new PlotMark<ScopePart>(Outline(plot, fill.Heights), new MarkStyle.Stroke(new MarkColor.Trace(fill.Channel), 1f, 1f), None))
            .Concat(scene.Map(heights => new PlotMark<ScopePart>(Outline(plot, heights), Outlined, None)).ToSeq());

    private static MarkShape.Polyline Outline(RectangleF plot, float[] heights) =>
        new([.. heights.Select((height, column) => new PointF(plot.Left + ((column + 0.5f) * plot.Width / heights.Length), plot.Bottom - (height * plot.Height)))]);

    private static float[] Heights(ReadOnlyMemory<int> counts, int columns, Axis axis) =>
        [.. Enumerable.Range(0, columns).Select(column => (column * counts.Length / columns) switch {
            var first => float.Clamp(axis.Unit(Peak(counts[first..int.Max(first + 1, (column + 1) * counts.Length / columns)])), 0f, 1f),
        })];

    private static IO<Unit> WaveformPaint(PlotCanvas canvas, ScopeViewState state, RectangleF plot, ScopeTraces.Waveform traces) =>
        from range in IO.lift(Reaching(1f / state.Gain))
        let whole = new PlotPlane.Cartesian(plot, new Axis.Linear(AxisRange.Unit), new Axis.Linear(range))
        let columns = toSeq(traces.Layout.Columns(plot)).Map(column => (
            Plane: new PlotPlane.Cartesian(column.Column, new Axis.Linear(AxisRange.Unit), new Axis.Linear(range)),
            Ink: Ink(canvas, new MarkColor.Trace(column.Channel)),
            Grid: column.Channel.Bins(traces.Reading.Bins)))
        let peak = columns.Map(static column => Peak(column.Grid.Counts)).Fold(0, int.Max)
        let tenths = Tenths(range.High)
        from extent in IO.lift(Plots.Device(whole, canvas.Scale))
        from rastered in Rastered(canvas, plot, extent, point => columns.Fold(Vector3.Zero, (sum, column) => column.Plane.Value(point) switch {
            var at when at.First is >= 0f and < 1f => sum + (column.Ink * Weight(Count(column.Grid, at), peak, state.Value.Chosen.Brightness)),
            _ => sum,
        }))
        from ruled in Plots.Paint(canvas, Plots.Rules(whole, Orientation.Horizontal, tenths.Map(static tenth => (float)tenth / Divisions)).Map(static shape => new PlotMark<ScopePart>(shape, MarkStyle.Grid, None)))
        from percent in Plots.Labels(canvas, whole, LabelSide.Left, tenths.Map(static tenth => new AxisLabel((float)tenth / Divisions, (tenth * 100 / Divisions).ToString(RowText.Culture))))
        from coded in traces.MaxCode.Match(
            Some: code => Plots.Labels(canvas, whole, LabelSide.Right, tenths.Map(tenth =>
                new AxisLabel((float)tenth / Divisions, MathF.Round(code * tenth / Divisions, MidpointRounding.ToEven).ToString(RowText.Culture)))),
            None: static () => IO.pure(unit))
        select unit;

    private static Seq<int> Tenths(float limit) =>
        toSeq(Range(0, Divisions + 1)).Filter(tenth => (float)tenth / Divisions <= limit);

    private static IO<Unit> VectorscopePaint(PlotCanvas canvas, ScopeViewState state, RectangleF plot, ScopeTraces.Vectorscope traces) =>
        (Axes: new ChromaAxes(traces.Reading.Request.Gamut.Luminance), Peak: Peak(traces.Reading.Bins.Counts), Square: ScopeKind.Unit(plot)) switch {
            var held =>
                from radius in IO.lift(Reaching(traces.Reading.Request.Zoom.Reach))
                let polar = new PlotPlane.Polar(plot, new Axis.Periodic(AxisRange.Radians), new Axis.Linear(radius))
                from extent in IO.lift(Plots.Device(held.Square, canvas.Scale))
                from rastered in Rastered(canvas, plot, extent, point => Scoped(canvas, held.Axes, traces, held.Peak, state.Value.Chosen.Brightness, held.Square.Value(point)))
                from ringed in Plots.Paint(canvas, Rings(polar, radius.High))
                from zoomed in traces.Reading.Request.Zoom.Switch(
                    (Canvas: canvas, Polar: polar, held.Axes),
                    normal: static marked => Targets(marked.Canvas, marked.Polar, marked.Axes),
                    doubled: static marked => IO.lift(() => marked.Canvas.Graphics.DrawText(
                        marked.Canvas.Font, Plots.Resolve(marked.Canvas, MarkStyle.Label), marked.Polar.Plot.Location, RowText.Localize("2x Zoom").Local)))
                select unit,
        };

    private static Vector3 Scoped(PlotCanvas canvas, ChromaAxes axes, ScopeTraces.Vectorscope traces, int peak, TraceBrightness brightness, (float First, float Second) at) =>
        (Centred: new Vector2(at.First, at.Second) - new Vector2(0.5f), Count: Count(traces.Reading.Bins, at)) switch {
            var held when held.Centred.Length() <= 0.5f && held.Count > 0 =>
                Ink(canvas, Hue(axes, 2f * held.Centred * traces.Reading.Request.Zoom.Reach)) * Weight(held.Count, peak, brightness),
            _ => Vector3.Zero,
        };

    private static MarkColor Hue(ChromaAxes axes, Vector2 chroma) =>
        Vector4.Transform(new Vector4(0.5f, chroma.X, chroma.Y, 1f), axes.Inverse).AsVector3() switch {
            var rgb => (Low: float.Min(rgb.X, float.Min(rgb.Y, rgb.Z)), High: float.Max(rgb.X, float.Max(rgb.Y, rgb.Z))) switch {
                var (low, high) when high > low => new MarkColor.Fixed(Shade((rgb - new Vector3(low)) / (high - low))),
                _ => new MarkColor.Themed(PaintSlot.ControlText),
            },
        };

    private static Seq<PlotMark<ScopePart>> Rings(PlotPlane.Polar polar, float reach) =>
        (East: polar.Point(0f, reach), North: polar.Point(MathF.PI / 2f, reach), West: polar.Point(MathF.PI, reach), South: polar.Point(3f * MathF.PI / 2f, reach)) switch {
            var held => Seq<MarkShape>(
                    new MarkShape.Disc(polar.Plot.Center, PointF.Distance(held.East, held.West)),
                    new MarkShape.Rule(held.West, held.East),
                    new MarkShape.Rule(held.South, held.North),
                    new MarkShape.Rule(polar.Plot.Center, polar.Point(float.DegreesToRadians(SkinDegrees), reach)))
                .Map(static shape => new PlotMark<ScopePart>(shape, MarkStyle.Grid, None)),
        };

    private static IO<Unit> Targets(PlotCanvas canvas, PlotPlane.Polar polar, ChromaAxes axes) =>
        Aimed(polar, axes) switch {
            var targets =>
                from boxed in Plots.Paint(canvas, targets.Map(static held => new PlotMark<ScopePart>(
                    held.Box, new MarkStyle.Stroke(new MarkColor.Fixed(held.Target.Paint), TargetAlpha, 1f), None)))
                from lettered in IO.lift(() => targets.Iter(held => canvas.Graphics.DrawText(
                    canvas.Font,
                    Plots.Resolve(canvas, MarkStyle.Label),
                    held.At + LabelOffset - new SizeF(0f, canvas.Font.Ascent),
                    RowText.Localize(held.Target.Map(red: "R", magenta: "Mg", blue: "B", cyan: "Cy", green: "G", yellow: "Yl")).Local)))
                select unit,
        };

    private static Seq<(VectorTarget Target, PointF At, MarkShape.Polygon Box)> Aimed(PlotPlane.Polar polar, ChromaAxes axes) =>
        toSeq(VectorTarget.Items).Map(target => Vector4.Transform(new Vector4(target.Drive, 1f), axes.Forward) switch {
            var ycc => (Angle: MathF.Atan2(ycc.Z, ycc.Y), Chroma: new Vector2(ycc.Y, ycc.Z).Length()) switch {
                var (angle, chroma) => (Target: target, At: polar.Point(angle, chroma), Box: Box(polar, angle, chroma)),
            },
        });

    private static MarkShape.Polygon Box(PlotPlane.Polar polar, float angle, float chroma) =>
        float.DegreesToRadians(TargetDegrees) switch {
            var spread => new MarkShape.Polygon([
                polar.Point(angle - spread, chroma - TargetChroma), polar.Point(angle + spread, chroma - TargetChroma),
                polar.Point(angle + spread, chroma + TargetChroma), polar.Point(angle - spread, chroma + TargetChroma)]),
        };

    private static IO<Unit> ChromaticityPaint(PlotCanvas canvas, RectangleF plot, ScopeTraces.Chromaticity traces) =>
        IO.lift(() => canvas.Graphics.SetClip(plot)).Bracket(
            Use: _ => ChromaticityPlot(canvas, plot, traces),
            Fin: _ => IO.lift(canvas.Graphics.ResetClip));

    private static IO<Unit> ChromaticityPlot(PlotCanvas canvas, RectangleF plot, ScopeTraces.Chromaticity traces) =>
        (Square: ScopeKind.Unit(ScopeTraces.Chromaticity.Square(plot)), Ink: Ink(canvas, new MarkColor.Themed(PaintSlot.ControlText)), Peak: Peak(traces.Reading.Bins.Counts),
          ScopeTraces.Chromaticity.Fit, Roles: Roles(traces), Side: float.Clamp(plot.Width * WhiteShare, WhiteSide.Least, WhiteSide.Most)) switch {
              var held =>
                  from extent in IO.lift(Plots.Device(held.Square, canvas.Scale))
                  from rastered in Rastered(canvas, held.Square.Plot, extent, point => held.Ink * Weight(Count(traces.Reading.Bins, held.Square.Value(point)), held.Peak, TraceBrightness.Normal))
                  from ruled in Plots.Paint(canvas, Lattice(held.Square, held.Fit).Concat(Gamuts(held.Square, held.Side, held.Roles)))
                  from based in Plots.Labels(canvas, held.Square, LabelSide.Bottom, Evens(held.Fit.X))
                  from sided in Plots.Labels(canvas, held.Square, LabelSide.Left, Evens(held.Fit.Y))
                  from listed in Legend(canvas, plot, held.Roles)
                  select unit,
          };

    private static Seq<PlotMark<ScopePart>> Lattice(PlotPlane.Cartesian square, Vector2 fit) =>
        Plots.Rules(square, Orientation.Vertical, Tenths(fit.X).Map(static tenth => (float)tenth / Divisions))
            .Concat(Plots.Rules(square, Orientation.Horizontal, Tenths(fit.Y).Map(static tenth => (float)tenth / Divisions)))
            .Add(new MarkShape.Polygon([.. ScopeTraces.Chromaticity.Locus.Map(point => square.Point(point.X, point.Y))]))
            .Map(static shape => new PlotMark<ScopePart>(shape, MarkStyle.Grid, None));

    private static Seq<AxisLabel> Evens(float limit) =>
        Tenths(limit).Filter(static tenth => tenth % 2 == 0).Map(static tenth => ((float)tenth / Divisions) switch {
            var value => new AxisLabel(value, value.ToString("F1", RowText.Culture)),
        });

    private static Seq<(string Role, Gamut Gamut, float Alpha)> Roles(ScopeTraces.Chromaticity traces) =>
        traces.Working.Map(static gamut => (Role: RowText.Localize("Working").Local, Gamut: gamut, Alpha: WorkingAlpha)).ToSeq()
            .Add((RowText.Localize("Display").Local, traces.Reading.Request.Gamut, DisplayAlpha));

    private static Seq<PlotMark<ScopePart>> Gamuts(PlotPlane.Cartesian square, float side, Seq<(string Role, Gamut Gamut, float Alpha)> roles) =>
        from role in roles
        let columns = role.Gamut.XyzColumns
        let white = Xy(square, columns.Red + columns.Green + columns.Blue)
        from mark in Seq(
            new PlotMark<ScopePart>(
                new MarkShape.Polygon([Xy(square, columns.Red), Xy(square, columns.Green), Xy(square, columns.Blue)]),
                new MarkStyle.Stroke(new MarkColor.Themed(PaintSlot.ControlText), role.Alpha, 1f),
                None),
            new PlotMark<ScopePart>(
                new MarkShape.Band(RectangleF.FromCenter(white, new SizeF(2f * side, 2f * side))),
                new MarkStyle.Fill(new MarkColor.Themed(PaintSlot.ControlText), role.Alpha),
                None),
            new PlotMark<ScopePart>(
                new MarkShape.Band(RectangleF.FromCenter(white, new SizeF(side, side))),
                new MarkStyle.Fill(new MarkColor.Themed(PaintSlot.ControlBackground), 1f),
                None))
        select mark;

    private static PointF Xy(PlotPlane.Cartesian square, Vector3 xyz) =>
        (xyz.X + xyz.Y + xyz.Z) switch {
            var sum => square.Point(xyz.X / sum, xyz.Y / sum),
        };

    private static IO<Unit> Legend(PlotCanvas canvas, RectangleF plot, Seq<(string Role, Gamut Gamut, float Alpha)> roles) =>
        IO.lift(() => roles
            .Map(static (role, block) => Lines(role.Role, role.Gamut).Map(text => (Text: text, role.Alpha, Block: block)))
            .Flatten()
            .Map((line, index) => (line.Text, line.Alpha, Top: plot.Top + (index * canvas.Font.LineHeight) + (line.Block * canvas.Font.LineHeight / 2f)))
            .Iter(line => canvas.Graphics.DrawText(
                canvas.Font,
                Plots.Resolve(canvas, new MarkStyle.Fill(new MarkColor.Themed(PaintSlot.ControlText), line.Alpha)),
                new PointF(plot.Right - canvas.Graphics.MeasureString(canvas.Font, line.Text).Width, line.Top),
                line.Text)));

    private static Seq<string> Lines(string role, Gamut gamut) =>
        role.Cons(Seq(
                (Format: "R {0:F4} {1:F4}", Point: gamut.Configuration.Rgb.ChromaticityR),
                (Format: "G {0:F4} {1:F4}", Point: gamut.Configuration.Rgb.ChromaticityG),
                (Format: "B {0:F4} {1:F4}", Point: gamut.Configuration.Rgb.ChromaticityB),
                (Format: "W {0:F4} {1:F4}", Point: gamut.Configuration.Rgb.WhitePoint.Chromaticity))
            .Map(static line => RowText.Localize(line.Format, arguments: [line.Point.X, line.Point.Y]).Local));

    private static IO<Unit> Rastered(PlotCanvas canvas, RectangleF target, PixelExtent extent, Func<PointF, Vector3> trace) =>
        Rgb(canvas.Slots[PaintSlot.ControlBackground]) switch {
            var well => use(() => Plots.Pixels(extent, Enumerable.Range(0, extent.Width * extent.Height).Select(index =>
                    Shade(Vector3.Min(well + trace(new PointF(
                        target.Left + (((index % extent.Width) + 0.5f) * target.Width / extent.Width),
                        target.Top + (((index / extent.Width) + 0.5f) * target.Height / extent.Height))), Vector3.One)).ToArgb())))
                .Bind(bitmap => Plots.Raster(canvas, target, bitmap))
                .Bracket(),
        };

    private static Vector3 Ink(PlotCanvas canvas, MarkColor paint) => Rgb(Plots.Resolve(canvas, new MarkStyle.Fill(paint, 1f)));

    private static Vector3 Rgb(Color color) => new(color.R, color.G, color.B);

    private static Color Shade(Vector3 rgb) => new(rgb.X, rgb.Y, rgb.Z);

    private static float Weight(int count, int peak, TraceBrightness brightness) =>
        count > 0 ? float.Min(1f, (float)count / peak * brightness.Step * Intensity) : 0f;

    private static int Count(BinGrid grid, (float First, float Second) at) => grid.Row((int)(at.Second * grid.Rows))[(int)(at.First * grid.Columns)];

    private static int Peak(ReadOnlyMemory<int> counts) => MemoryMarshal.ToEnumerable(counts).Max();

    private static Fin<AxisRange> Reaching(float high) => AxisRange.Validate(0f, high, out AxisRange range) is { } error ? error : range;
}
