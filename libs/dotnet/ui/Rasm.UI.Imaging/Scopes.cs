using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Color.Calibration;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using Rasm.UI.Styling;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Imaging;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class ImagingRoles {
    public static readonly Role Well = Role.Create("well");
    public static readonly Role Series = Role.Create("series");
    public static readonly Role Trace = Role.Create("trace");
    public static readonly Role Guide = Role.Create("guide");
    public static readonly Role Major = Role.Create("major");
    public static readonly Role GridFine = Role.Create("grid-fine");
    public static readonly Role Highlight = Role.Create("highlight");
    public static readonly Role ClipOver = Role.Create("clip-over");
    public static readonly Role ClipUnder = Role.Create("clip-under");
    public static readonly Role Checker = Role.Create("checker");
    public static readonly Role OverlayCaption = Role.Create("overlay-caption");
    public static readonly Role Readout = Role.Create("readout");
    public static readonly Role Knob = Role.Create("knob");
    public static readonly Role Puck = Role.Create("puck");
    public static readonly Role Needle = Role.Create("needle");
    public static readonly Role Stop = Role.Create("stop");
    public static readonly Role Ring = Role.Create("ring");
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScopeKind : ICaptioned {
    public static readonly ScopeKind Histogram = new("histogram", "Histogram");
    public static readonly ScopeKind WaveformLuma = new("waveform-luma", "Waveform luma");
    public static readonly ScopeKind WaveformRgb = new("waveform-rgb", "Waveform RGB");
    public static readonly ScopeKind Parade = new("parade", "Parade");
    public static readonly ScopeKind Vectorscope = new("vectorscope", "Vectorscope");
    public static readonly ScopeKind Chromaticity = new("chromaticity", "Chromaticity");

    public string Caption { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TraceMix : ICaptioned {
    public static readonly TraceMix RgbLuma = new("rgb-luma", "RGB and luma", Seq(TracePlane.Red, TracePlane.Green, TracePlane.Blue, TracePlane.Luma));
    public static readonly TraceMix Rgb = new("rgb", "RGB", Seq(TracePlane.Red, TracePlane.Green, TracePlane.Blue));
    public static readonly TraceMix Luma = new("luma", "Luma", Seq(TracePlane.Luma));
    public static readonly TraceMix Red = new("red", "Red", Seq(TracePlane.Red));
    public static readonly TraceMix Green = new("green", "Green", Seq(TracePlane.Green));
    public static readonly TraceMix Blue = new("blue", "Blue", Seq(TracePlane.Blue));

    public string Caption { get; }
    public Seq<TracePlane> Planes { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Brightness : ICaptioned {
    public static readonly Brightness Bright = new("bright", 1.25f);
    public static readonly Brightness Normal = new("normal", 1f);
    public static readonly Brightness Dimmed = new("dimmed", 0.5f);

    public float Gain { get; }
    public Fraction Opacity => Fraction.Create(Gain / Bright.Gain);
}

public sealed record ScopeState(
    ScopeKind Kind, bool Logarithmic, bool Sdr, TraceMix Channels, Brightness Traces, Brightness Graticule,
    PositiveMagnitude Aspect, PositiveMagnitude Magnification, SampleSquare Sample) : IStateRecord<ScopeState, ScopeParameter> {
    public static ScopeState Default { get; } = new(
        ScopeKind.Histogram, Logarithmic: false, Sdr: false, TraceMix.RgbLuma, Brightness.Normal, Brightness.Normal, PositiveMagnitude.Create(0.4f), PositiveMagnitude.Unit, SampleSquare.One);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScopeParameter : IStateParameter<ScopeState> {
    private static readonly Seq<ScopeKind> Banded = Seq(ScopeKind.Histogram, ScopeKind.WaveformLuma, ScopeKind.WaveformRgb, ScopeKind.Parade);
    private static readonly Seq<ScopeKind> Dense = Seq(ScopeKind.WaveformLuma, ScopeKind.WaveformRgb, ScopeKind.Parade, ScopeKind.Vectorscope, ScopeKind.Chromaticity);
    private static readonly Presentation<float> Typed = new() { Form = NumberForm.Field };

    public static readonly ScopeParameter Scope = new("kind",
        new StateParameter<ScopeState>.Choice<ScopeKind>(Lens<ScopeState, ScopeKind>.New(static state => state.Kind, static kind => state => state with { Kind = kind })),
        toSeq(ScopeKind.Items), new("Scope", "Chart the scope draws"));
    public static readonly ScopeParameter Logarithmic = new("logarithmic",
        new StateParameter<ScopeState>.Toggle(Lens<ScopeState, bool>.New(static state => state.Logarithmic, static logarithmic => state => state with { Logarithmic = logarithmic })),
        Seq(ScopeKind.Histogram), new("Logarithmic counts", "Bin counts on a logarithmic scale"));
    public static readonly ScopeParameter Sdr = new("sdr",
        new StateParameter<ScopeState>.Toggle(Lens<ScopeState, bool>.New(static state => state.Sdr, static sdr => state => state with { Sdr = sdr })),
        Seq(ScopeKind.Histogram), new("SDR only", "Bins span codes up to reference white"));
    public static readonly ScopeParameter Channels = new("channels",
        new StateParameter<ScopeState>.Choice<TraceMix>(Lens<ScopeState, TraceMix>.New(static state => state.Channels, static channels => state => state with { Channels = channels })),
        Seq(ScopeKind.Histogram), new("Channels", "Planes the histogram traces"));
    public static readonly ScopeParameter Traces = new("traces",
        new StateParameter<ScopeState>.Choice<Brightness>(Lens<ScopeState, Brightness>.New(static state => state.Traces, static traces => state => state with { Traces = traces })),
        Dense, new("Trace brightness", "Opacity of density traces"));
    public static readonly ScopeParameter Graticule = new("graticule",
        new StateParameter<ScopeState>.Choice<Brightness>(Lens<ScopeState, Brightness>.New(static state => state.Graticule, static graticule => state => state with { Graticule = graticule })),
        toSeq(ScopeKind.Items), new("Graticule brightness", "Opacity of rules, rings, and targets"));
    public static readonly ScopeParameter Aspect = new("aspect",
        StateParameter.Bounded(Lens<ScopeState, PositiveMagnitude>.New(static state => state.Aspect, static aspect => state => state with { Aspect = aspect }), Typed),
        Banded, new("Aspect", "Chart height against width"));
    public static readonly ScopeParameter Magnification = new("magnification",
        StateParameter.Bounded(Lens<ScopeState, PositiveMagnitude>.New(static state => state.Magnification, static magnification => state => state with { Magnification = magnification }), Typed),
        Seq(ScopeKind.Vectorscope), new("Magnification", "Chroma zoom about neutral"));
    public static readonly ScopeParameter Sample = new("sample",
        new StateParameter<ScopeState>.Keyed<SampleSquare, int>(Lens<ScopeState, SampleSquare>.New(static state => state.Sample, static sample => state => state with { Sample = sample })),
        toSeq(ScopeKind.Items), new("Sample", "Pixels per side of square each pointer reading averages"));

    public StateParameter<ScopeState> Kind { get; }
    public Seq<ScopeKind> Listed { get; }
    public ParameterText Text { get; }
}

public sealed record ScopeFrames(PixelFrame Frame, ColorEncoding Encoding, Option<Quantizer> Levels, Gamut Working, Option<PixelFrame> Input);

public sealed record ScopeMarks(Option<BinRange> Selection, LanguageExt.HashSet<ClipEnd> Clips);

public sealed record FrameMask(Coverage Coverage, Role Role);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record ScopeTraces(ScopeReading Reading, Seq<(Option<TracePlane> Plane, SKImage Density)> Images) : IDisposable {
    public void Dispose() => Images.Iter(static image => image.Density.Dispose());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Scopes {
    // --- [READ]
    private const string HueSource = """
        uniform shader density;
        uniform float3 luma;
        uniform float3 blue;
        uniform float3 red;
        uniform float reach;
        uniform float2 size;
        half4 main(float2 p) {
            float2 chroma = ((float2(p.x, size.y - p.y) / size * 2.0) - 1.0) * reach;
            float3 rgb = (0.5 * luma) + (chroma.x * blue) + (chroma.y * red);
            float low = min(rgb.r, min(rgb.g, rgb.b));
            float high = max(rgb.r, max(rgb.g, rgb.b));
            half3 hue = high > low ? half3((rgb - low) / (high - low)) : half3(1.0);
            half weight = density.eval(p).a;
            return half4(hue * weight, weight);
        }
        """;

    private static readonly SKRuntimeEffect Hue = Compiled(HueSource);

    public static ScopeRequest Request(ScopeState state, ScopeFrames frames) =>
        state.Kind.Switch(
            (State: state, Frames: frames, frames.Encoding.Gamut),
            histogram: static held => (ScopeRequest)new ScopeRequest.Histogram(
                held.Gamut, held.Frames.Levels.Map(static levels => levels.Clipping), held.Frames.Input.Map(input => (input, held.Frames.Working))),
            waveformLuma: static held => new ScopeRequest.Waveform(held.Gamut, PixelExtent.Analysis),
            waveformRgb: static held => new ScopeRequest.Waveform(held.Gamut, PixelExtent.Analysis),
            parade: static held => new ScopeRequest.Waveform(held.Gamut, PixelExtent.Analysis),
            vectorscope: static held => new ScopeRequest.Vectorscope(held.Gamut, PixelExtent.Analysis, held.State.Magnification),
            chromaticity: static held => new ScopeRequest.Chromaticity(held.Gamut, held.Frames.Encoding.Transfer, PixelExtent.Analysis));

    public static IO<ScopeTraces> Read(ScopeRequest request, PixelFrame frame) =>
        IO.lift(() => request.Read(frame) switch {
            var reading => new ScopeTraces(reading, reading.Switch(
                histogram: static _ => Seq<(Option<TracePlane> Plane, SKImage Density)>(),
                waveform: static waveform => Waveformed(waveform.Bins),
                vectorscope: static vectorscope => Seq((Plane: Option<TracePlane>.None, Density: Hued(vectorscope))),
                chromaticity: static chromaticity => Seq((Plane: Option<TracePlane>.None, Density: Density(chromaticity.Bins, Peak(chromaticity.Bins)))))),
        });

    public static Seq<FrameMask> Masks(ScopeFrames frames, ScopeMarks marks) =>
        marks.Selection.Map(range => new FrameMask(new Coverage.Luma(frames.Encoding.Gamut, range), Role.Selection)).ToSeq()
        + (from levels in frames.Levels.ToSeq()
           from end in toSeq(ClipEnd.Items).Filter(marks.Clips.Contains)
           select new FrameMask(new Coverage.Clipped(levels.Clipping, end), Clipping(end)));

    private static Seq<(Option<TracePlane> Plane, SKImage Density)> Waveformed(Traces traces) {
        Seq<(TracePlane Plane, BinGrid Grid)> planes = toSeq(TracePlane.Items).Map(plane => (Plane: plane, Grid: plane.Bins(traces)));
        int peak = planes.Map(static held => Peak(held.Grid)).Fold(1, int.Max);
        return planes.Map(held => (Plane: Some(held.Plane), Density: Density(held.Grid, peak)));
    }

    private static int Peak(BinGrid grid) => int.Max(1, TensorPrimitives.Max(grid.Counts.Span));

    private static SKImage Density(BinGrid grid, int peak) {
        float[] weights = new float[grid.Columns * grid.Rows];
        for (int row = 0; row < grid.Rows; row++)
            TensorPrimitives.ConvertTruncating(grid.Row(row), weights.AsSpan((grid.Rows - 1 - row) * grid.Columns, grid.Columns));
        TensorPrimitives.Divide(weights, peak, weights);
        Half[] halves = new Half[weights.Length];
        TensorPrimitives.ConvertToHalf(weights, halves);
        return SKImage.FromPixelCopy(new SKImageInfo(grid.Columns, grid.Rows, SKColorType.AlphaF16, SKAlphaType.Premul), MemoryMarshal.AsBytes(halves.AsSpan()));
    }

    private static SKImage Hued(ScopeReading.Vectorscope reading) {
        Matrix<double> toRgb = Gamut.FromYpbpr(reading.Request.Gamut.Ybr);
        SKImageInfo info = new(reading.Bins.Columns, reading.Bins.Rows, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
        using SKImage density = Density(reading.Bins, Peak(reading.Bins));
        using SKShader sampled = density.ToShader();
        using SKRuntimeEffectUniforms uniforms = new(Hue) {
            { "luma", Column(toRgb, 0) },
            { "blue", Column(toRgb, 1) },
            { "red", Column(toRgb, 2) },
            { "reach", reading.Request.Reach },
            { "size", new SKSize(info.Width, info.Height) },
        };
        using SKRuntimeEffectChildren children = new(Hue) { { "density", sampled } };
        using SKShader shader = Hue.ToShader(uniforms, children);
        using SKSurface surface = SKSurface.Create(info);
        using SKPaint paint = new() { Shader = shader };
        surface.Canvas.DrawRect(info.Rect, paint);
        return surface.Snapshot();
    }

    private static SKRuntimeEffect Compiled(string source) =>
        SKRuntimeEffect.CreateShader(source, out string errors) switch {
            var effect when errors is null => effect,
            _ => throw new InvalidOperationException(errors),
        };

    private static float[] Column(Matrix<double> matrix, int column) => [(float)matrix[0, column], (float)matrix[1, column], (float)matrix[2, column]];

    // --- [CHART]
    private const float BarLevel = 0.75f;
    private const int Divisions = 10;
    private static readonly Sizing Grown = new Sizing.Grow(Factor.Create(1f));
    private static readonly Key First = new Key.Ordinal(0);
    private static readonly Key Last = new Key.Ordinal(1);

    public static Node Chart(
        ScopeState state, ScopeFrames frames, Content<ScopeTraces> traces, ScopeMarks marks, Option<Vector2> pointer,
        Func<Edit<ScopeState>, IO<Unit>> edited, Func<Edit<ScopeMarks>, IO<Unit>> marked) =>
        (Shown: traces.Held, Pixel: pointer.Bind(at => state.Sample.Mean(frames.Frame, (at.X, at.Y)))) switch {
            var held => state.Kind.Switch(
                (State: state, Frames: frames, held.Shown, held.Pixel, Marks: marks, Edited: edited, Marked: marked),
                histogram: static chart => Histogram(chart.State, chart.Frames, chart.Shown, chart.Marks, chart.Pixel, chart.Marked),
                waveformLuma: static chart => Waveform(chart.State, chart.Frames, chart.Shown, chart.Pixel, [[TracePlane.Luma]]),
                waveformRgb: static chart => Waveform(chart.State, chart.Frames, chart.Shown, chart.Pixel, [[TracePlane.Red, TracePlane.Green, TracePlane.Blue]]),
                parade: static chart => Waveform(chart.State, chart.Frames, chart.Shown, chart.Pixel, [[TracePlane.Red], [TracePlane.Green], [TracePlane.Blue]]),
                vectorscope: static chart => Vectorscope(chart.State, chart.Frames, chart.Shown, chart.Pixel, chart.Edited),
                chromaticity: static chart => Chromaticity(chart.State, chart.Frames, chart.Shown, chart.Pixel)) with {
                Role = ImagingRoles.Well,
                Width = Grown,
                Conditions = traces.Switch(
                    pending: static _ => LanguageExt.HashSet<Condition>.Empty.Add(Condition.Busy),
                    ready: static _ => [],
                    empty: static _ => [],
                    failed: static _ => LanguageExt.HashSet<Condition>.Empty.Add(Condition.Faulted)),
            },
        };

    public static Node Levels(Levels value, Content<ScopeTraces> traces, Func<Edit<Levels>, IO<Unit>> edited) =>
        new PlotPlane.Track(Orientation.Horizontal, new Axis.Linear(AxisRange.Create(0f, HistogramAxis.Bins))) switch {
            var plane => new Handles<Levels>(value, new Points<Levels>(
                    static levels => toSeq(LevelsParameter.Items).Map((parameter, index) => ((Key)new Key.Ordinal((uint)index), new Vector2(HistogramAxis.Bin(levels.Level(parameter)) + 0.5f, 0.5f))),
                    static (levels, targets, _) => (targets.Fold(levels, static (held, target) => (from key in Some(target.Key).Bind(static key => key is Key.Ordinal ordinal ? Some((int)ordinal.Index) : None)
                                                                                                   from parameter in toSeq(LevelsParameter.Items).At(key)
                                                                                                   from leveled in held.Leveled(parameter, HistogramAxis.LowerBound(Binned(target.To.X, HistogramAxis.Bins))).ToOption()
                                                                                                   select leveled).IfNone(held)), targets.Map(static target => target.Key)),
                    static at => Spoken(HistogramAxis.LowerBound(Binned(at.X, HistogramAxis.Bins)))), edited)
                .Drawn(plane, traces.Held.Bind(static shown => Histogrammed(shown.Reading)).Map(static reading => (Node)Area(TracePlane.Luma.Bins(reading.Bins).Row(0), new Axis.Linear(AxisRange.Create(0f, Peak(TracePlane.Luma.Bins(reading.Bins))))) with {
                    Role = ImagingRoles.Trace,
                }).ToSeq(), Some<Func<Key, Vector2, Node>>(static (_, at) => new Node.Pin(at, new Node.Box { Role = ImagingRoles.Knob }) { Align = UnitPoint.Center })) with {
                Role = ImagingRoles.Well,
                Width = Grown,
            },
        };

    private static Node Histogram(ScopeState state, ScopeFrames frames, Option<ScopeTraces> shown, ScopeMarks marks, Option<Vector4> pixel, Func<Edit<ScopeMarks>, IO<Unit>> marked) {
        int bins = state.Sdr ? HistogramAxis.Bin(1d) : HistogramAxis.Bins;
        PlotPlane.Track plane = new(Orientation.Horizontal, new Axis.Linear(AxisRange.Create(0f, bins)));
        Option<ScopeReading.Histogram> reading = shown.Bind(static held => Histogrammed(held.Reading));
        Seq<(TracePlane Plane, ReadOnlyMemory<int> Counts)> planes =
            from held in reading.ToSeq()
            from mixed in state.Channels.Planes
            select (Plane: mixed, Counts: mixed.Bins(held.Bins).Counts[..bins]);
        Option<ReadOnlyMemory<int>> input = reading.Bind(static held => held.Input).Map(luma => TracePlane.Luma.Bins(luma).Counts[..bins]);
        AxisRange span = AxisRange.Create(0f, int.Max(1, (planes.Map(static held => held.Counts) + input.ToSeq()).Map(static counts => TensorPrimitives.Max(counts.Span)).Fold(1, int.Max)));
        Axis counts = state.Logarithmic ? new Axis.Logarithmic(span) : new Axis.Linear(span);
        Seq<float> ticks = HistogramAxis.Ticks.Filter(tick => HistogramAxis.Bin(tick) < bins);
        Handles<Option<BinRange>> selection = new(marks.Selection, new Points<Option<BinRange>>(
                static held => held.Map(static range => Seq<(Key Key, Vector2 At)>((First, new Vector2(range.First + 0.5f, 0.5f)), (Last, new Vector2(range.Last + 0.5f, 0.5f)))).IfNone([]),
                (held, targets, _) => Ends(held, targets, bins),
                at => Spoken(HistogramAxis.LowerBound(Binned(at.X, bins)))) {
            Placed = Some<Func<Option<BinRange>, Vector2, Option<Key>, Option<(Option<BinRange> Value, Key Key)>>>((_, at, _) =>
                Binned(at.X, bins) switch {
                    var bin => Spanned(bin, bin).Map(static range => (Some(range), Last)),
                }),
            Removed = Some<Func<Option<BinRange>, Seq<Key>, Option<Option<BinRange>>>>(static (_, _) => Some(Option<BinRange>.None)),
        },
            edit => marked(edit.Map(held => marks with { Selection = held }).As())) {
            GroundCursor = Cursor.Crosshair,
        };
        Seq<Node> graticule = ticks.Map(static tick => (Node)new Node.Mark(new Figure.Rule(new Vector2(HistogramAxis.Bin(tick), 0f), new Vector2(HistogramAxis.Bin(tick), 1f))) {
            Role = tick.CompareTo(1f) is 0 ? ImagingRoles.Major : Role.Grid,
        })
            .Add(new Node.Mark(new Figure.Grid(Orientation.Vertical)) { Role = ImagingRoles.GridFine });
        Seq<Node> traced = planes.Map(held => (Node)(Area(held.Counts.Span, counts) with { Role = ImagingRoles.Series, Series = Seriesed(held.Plane) }))
            + input.Map(luma => (Node)new Node.Mark(new Figure.Curve(Centered(luma.Length), Fractions(luma.Span, counts))) { Role = ImagingRoles.Trace }).ToSeq();
        Seq<Node> clips =
            from clipped in reading.Bind(static held => held.Clipped).ToSeq()
            from end in Seq(
                (End: ClipEnd.Low, Lit: clipped.Low, At: new Vector2(0f, 1f), Align: UnitPoint.Create(0f, 0f)),
                (End: ClipEnd.High, Lit: clipped.High, At: new Vector2(bins, 1f), Align: UnitPoint.Create(1f, 0f)))
            select (Node)new Node.Pin(end.At, new Part.Press(_ => marked(new Edit<ScopeMarks>.Commit(marks with { Clips = marks.Clips.Contains(end.End) ? marks.Clips.Remove(end.End) : marks.Clips.Add(end.End) })))
                .Drawn(new Node.Box {
                    Role = Clipping(end.End),
                    Conditions = toHashSet(Seq(
                        Some(Condition.Selected).Filter(_ => marks.Clips.Contains(end.End)),
                        Some(Condition.Disabled).Filter(_ => end.Lit.Red + end.Lit.Green + end.Lit.Blue is 0)).Somes()),
                })) { Align = end.Align };
        Seq<Node> guide = pixel.Map(sample => (Node)Ruled(HistogramAxis.Bin(Luminance(sample, frames.Encoding.Gamut)) + 0.5f, Orientation.Vertical) with { Role = ImagingRoles.Guide }).ToSeq();
        return selection.Drawn(plane,
            Seq<Node>(Layer(plane, state.Graticule.Opacity, graticule))
            + marks.Selection.Map(static range => (Node)new Node.Mark(new Figure.Region(Some(AxisRange.Create(range.First, range.Last + 1)), None)) { Role = Role.Selection }).ToSeq()
            + Seq<Node>(Layer(plane, Fraction.MaxValue, traced))
            + guide
            + clips) with {
            Aspect = Factor.Create(1f / state.Aspect),
        };
    }

    private static Node.Flex Waveform(ScopeState state, ScopeFrames frames, Option<ScopeTraces> shown, Option<Vector4> pixel, Seq<Seq<TracePlane>> columns) {
        PlotPlane.Track plane = new(Orientation.Vertical, new Axis.Linear(AxisRange.Create(0f, 1f)));
        Seq<float> tenths = toSeq(Range(0, Divisions + 1)).Map(static tenth => (float)tenth / Divisions);
        (double low, double high) = YbrConfiguration.Rec709.RangeY;
        double full = YbrConfiguration.Jpeg.RangeY.max;
        Seq<Node> graticule = Seq<Node>(new Node.Mark(new Figure.Grid(Orientation.Vertical)) { Role = Role.Grid })
            + Seq(low, high).Map(level => (Node)Ruled((float)(level / full), Orientation.Horizontal) with { Role = ImagingRoles.Major });
        Seq<Node.Pin> labels = tenths.Map(static tenth => new Node.Pin(new Vector2(0f, tenth), Labelled(tenth * 100f, "0")) { Align = UnitPoint.Create(0f, 0.5f) })
            + (from levels in frames.Levels.ToSeq()
               from tenth in tenths
               select new Node.Pin(new Vector2(1f, tenth), Labelled(MathF.Round(levels.MaxCode * tenth, MidpointRounding.ToEven), "0")) { Align = UnitPoint.Create(1f, 0.5f) });
        Seq<Node> guide = pixel.Map(sample => (Node)Ruled((float)Luminance(sample, frames.Encoding.Gamut), Orientation.Horizontal) with { Role = ImagingRoles.Guide }).ToSeq();
        return new Node.Flex(Orientation.Horizontal, columns.Map(planes => (Node)new Node.Plot(plane,
            Seq<Node>(Layer(plane, state.Graticule.Opacity, graticule), Layer(plane, state.Traces.Opacity, shown.ToSeq()
                .Bind(static held => held.Images)
                .Choose(image => image.Plane.Filter(trace => planes.Exists(held => held == trace)).Map(trace => (Node)new Node.Image(image.Density) { Role = ImagingRoles.Series, Series = Seriesed(trace), Width = Grown, Height = Grown }))))
            + guide) {
            Width = Grown,
            Height = Grown,
            Labels = labels,
        })) { Aspect = Factor.Create(1f / state.Aspect) };
    }

    private static Node.Plot Vectorscope(ScopeState state, ScopeFrames frames, Option<ScopeTraces> shown, Option<Vector4> pixel, Func<Edit<ScopeState>, IO<Unit>> edited) {
        Gamut gamut = frames.Encoding.Gamut;
        Matrix<double> toYpbpr = Gamut.ToYpbpr(gamut.Ybr);
        float reach = 0.5f / state.Magnification;
        PlotPlane.Polar polar = new(new Axis.Periodic(AxisRange.Create(0f, MathF.Tau), 1, 0), Some<Axis>(new Axis.Linear(AxisRange.Create(0f, reach))));
        PlotPlane.Track rest = new(Orientation.Vertical, new Axis.Linear(AxisRange.Create(0f, 0.5f)));
        Seq<Vector2> bars = toSeq(Range(1, 6)).Map(static corner => new Vector3(corner & 1, (corner >> 1) & 1, (corner >> 2) & 1) * BarLevel).Map(rgb => Chroma(toYpbpr, rgb));
        Seq<Node> graticule = Seq<Node>(
                new Node.Mark(new Figure.Grid(Orientation.Vertical)) { Role = ImagingRoles.GridFine },
                new Node.Mark(new Figure.Rule(new Vector2(0f, reach), new Vector2(MathF.PI, reach))) { Role = ImagingRoles.Major },
                new Node.Mark(new Figure.Rule(new Vector2(MathF.PI / 2f, reach), new Vector2(3f * MathF.PI / 2f, reach))) { Role = ImagingRoles.Major })
            + ColorChart.ColorCheckerClassic.References(gamut).Find(ChartPatch.LightSkin).Map(skin => Chroma(toYpbpr, new Vector3((float)skin.R, (float)skin.G, (float)skin.B))).Map(skin =>
                (Node)new Node.Mark(new Figure.Rule(new Vector2(skin.X, 0f), new Vector2(skin.X, reach))) { Role = ImagingRoles.Highlight }).ToSeq()
            + bars.Filter(bar => bar.Y <= reach).Map(static bar => (Node)new Node.Pin(bar, new Node.Box { Role = ImagingRoles.Ring }) { Align = UnitPoint.Center });
        Seq<Node> guide = pixel.Map(sample => Chroma(toYpbpr, new Vector3(sample.X, sample.Y, sample.Z))).Filter(at => at.Y <= reach)
            .Map(static at => (Node)new Node.Pin(at, new Node.Box { Role = ImagingRoles.Needle }) { Align = UnitPoint.Center }).ToSeq();
        return new Node.Plot(new PlotPlane.Track(Orientation.Vertical, new Axis.Linear(AxisRange.Create(0f, reach))), [
            new Node.Plot(polar, Seq<Node>(Layer(polar, state.Graticule.Opacity, graticule), Layer(polar, state.Traces.Opacity, shown.ToSeq()
                    .Bind(static held => held.Images)
                    .Map(static image => (Node)new Node.Image(image.Density) { Role = ImagingRoles.Trace, Fit = ImageFit.Contain, Width = Grown, Height = Grown })))
                + guide) { Width = Grown, Height = Grown },
        ]) {
            Aspect = Factor.Create(1f),
            Part = new Part.Viewport(rest, edit => edited(edit.Map(plane => plane.Dimensions.Head
                .Bind(static held => PositiveMagnitude.Validated(0.5f / MathF.Abs(held.Axis.Range.Span)).ToOption())
                .Map(magnification => state with { Magnification = magnification })
                .IfNone(state)).As())),
        };
    }

    private static Node.Plot Chromaticity(ScopeState state, ScopeFrames frames, Option<ScopeTraces> shown, Option<Vector4> pixel) {
        Vector2 fit = StandardObserver.Locus.Fold(Vector2.Zero, Vector2.Max) switch {
            var top => new Vector2(MathF.Ceiling(Divisions * top.X), MathF.Ceiling(Divisions * top.Y)) / Divisions,
        };
        Seq<(Gamut Gamut, Role Role)> gamuts = Seq((frames.Working, ImagingRoles.Guide), (frames.Encoding.Gamut, ImagingRoles.Major));
        Seq<Node> graticule = Seq<Node>(
                new Node.Mark(new Figure.Grid(Orientation.Horizontal)) { Role = ImagingRoles.GridFine },
                new Node.Mark(new Figure.Grid(Orientation.Vertical)) { Role = ImagingRoles.GridFine },
                new Node.Mark(new Figure.Polygon(toSeq(StandardObserver.Locus))) { Role = Role.Grid })
            + gamuts.Bind(static held => Seq<Node>(
                new Node.Mark(new Figure.Polygon(Seq(held.Gamut.Configuration.Rgb.ChromaticityR, held.Gamut.Configuration.Rgb.ChromaticityG, held.Gamut.Configuration.Rgb.ChromaticityB)
                    .Map(static xy => new Vector2((float)xy.X, (float)xy.Y)))) { Role = held.Role },
                new Node.Pin(new Vector2((float)held.Gamut.Configuration.Rgb.WhitePoint.Chromaticity.X, (float)held.Gamut.Configuration.Rgb.WhitePoint.Chromaticity.Y),
                    new Node.Box { Role = ImagingRoles.Ring }) { Align = UnitPoint.Center }));
        Seq<Node> guide = pixel.Bind(sample => Xy(frames.Encoding, sample)).Map(static at => (Node)new Node.Pin(at, new Node.Box { Role = ImagingRoles.Needle }) { Align = UnitPoint.Center }).ToSeq();
        Seq<float> tenths = toSeq(Range(0, Divisions + 1)).Filter(static tenth => tenth % 2 is 0).Map(static tenth => (float)tenth / Divisions);
        PlotPlane.Cartesian plane = new(new Axis.Linear(AxisRange.Create(0f, fit.X)), new Axis.Linear(AxisRange.Create(0f, fit.Y)));
        return new Node.Plot(plane,
            Seq<Node>(Layer(plane, state.Graticule.Opacity, graticule), Layer(plane, state.Traces.Opacity, shown.ToSeq()
                .Bind(static held => held.Images)
                .Map(static image => (Node)new Node.Pin(Vector2.Zero, new Node.Image(image.Density) { Role = ImagingRoles.Trace }) { Span = Vector2.One })))
            + guide) {
            Aspect = Factor.Create(fit.X / fit.Y),
            Labels = tenths.Filter(tenth => tenth <= fit.X).Map(static tenth => new Node.Pin(new Vector2(tenth, 0f), Labelled(tenth, "F1")) { Align = UnitPoint.Create(0.5f, 1f) })
                + tenths.Filter(tenth => tenth <= fit.Y && tenth > 0f).Map(static tenth => new Node.Pin(new Vector2(0f, tenth), Labelled(tenth, "F1")) { Align = UnitPoint.Create(0f, 0.5f) }),
        };
    }

    private static Node.Styled Layer(PlotPlane plane, Fraction opacity, Seq<Node> children) =>
        new(new Node.Plot(plane, children) { Width = Grown, Height = Grown }) { Opacity = opacity };

    private static Node.Mark Area(ReadOnlySpan<int> counts, Axis axis) => new(new Figure.Area(Centered(counts.Length), Fractions(counts, axis)));

    private static Node.Mark Ruled(float at, Orientation held) =>
        new(held.Map(horizontal: new Figure.Rule(new Vector2(0f, at), new Vector2(1f, at)), vertical: new Figure.Rule(new Vector2(at, 0f), new Vector2(at, 1f))));

    private static Node.Text Labelled(float value, string format) =>
        new([value.ToString(format, CultureInfo.InvariantCulture)]) { Role = Role.Label };

    private static AxisRange Centered(int bins) => AxisRange.Create(0.5f, bins - 0.5f);

    private static Seq<float> Fractions(ReadOnlySpan<int> counts, Axis axis) {
        float[] fractions = new float[counts.Length];
        for (int bin = 0; bin < counts.Length; bin++)
            fractions[bin] = float.Clamp(axis.Fraction(counts[bin]), 0f, 1f);
        return toSeq(fractions);
    }

    internal static Series Seriesed(TracePlane plane) => new((uint)TracePlane.Items.TakeWhile(item => item != plane).Count()) { Color = new SKColorF(plane.Color.X, plane.Color.Y, plane.Color.Z) };

    private static Role Clipping(ClipEnd end) => end.Map(low: ImagingRoles.ClipUnder, high: ImagingRoles.ClipOver);

    private static Option<ScopeReading.Histogram> Histogrammed(ScopeReading reading) =>
        reading is ScopeReading.Histogram histogram ? Some(histogram) : None;

    private static int Binned(float at, int bins) => int.Clamp((int)MathF.Floor(at), 0, bins - 1);

    private static Option<BinRange> Spanned(int first, int last) =>
        ValidationResults.ToFin(BinRange.Validate(int.Min(first, last), int.Max(first, last), out BinRange range), range).ToOption();

    private static (Option<BinRange> Value, Seq<Key> Keys) Ends(Option<BinRange> held, Seq<(Key Key, Vector2 To)> targets, int bins) =>
        held.Map(range => targets.Fold((range.First, range.Last), (ends, target) => Binned(target.To.X, bins) switch {
            var bin => target.Key == First ? (bin, ends.Last) : (ends.First, bin),
        }))
            .Map(ends => (Value: Spanned(ends.First, ends.Last) | held, Keys: ends.First > ends.Last ? targets.Map(static target => target.Key == First ? Last : First) : targets.Map(static target => target.Key)))
            .IfNone((held, targets.Map(static target => target.Key)));

    private static double Luminance(Vector4 sample, Gamut gamut) => LuminanceSamples.Luminance(sample.X, sample.Y, sample.Z, gamut.Luminance);

    private static Vector2 Chroma(Matrix<double> toYpbpr, Vector3 rgb) =>
        (Blue: (toYpbpr[1, 0] * rgb.X) + (toYpbpr[1, 1] * rgb.Y) + (toYpbpr[1, 2] * rgb.Z), Red: (toYpbpr[2, 0] * rgb.X) + (toYpbpr[2, 1] * rgb.Y) + (toYpbpr[2, 2] * rgb.Z)) switch {
            var (blue, red) => new Vector2((float)double.Atan2(red, blue), (float)double.Hypot(blue, red)),
        };

    private static Option<Vector2> Xy(ColorEncoding encoding, Vector4 sample) {
        Vector4[] light = [sample];
        encoding.Decode(light);
        Matrix<double> toXyz = encoding.Gamut.ToXyz;
        (double x, double y, double z) = (
            (toXyz[0, 0] * light[0].X) + (toXyz[0, 1] * light[0].Y) + (toXyz[0, 2] * light[0].Z),
            (toXyz[1, 0] * light[0].X) + (toXyz[1, 1] * light[0].Y) + (toXyz[1, 2] * light[0].Z),
            (toXyz[2, 0] * light[0].X) + (toXyz[2, 1] * light[0].Y) + (toXyz[2, 2] * light[0].Z));
        return x + y + z > 0d ? Some(new Vector2((float)(x / (x + y + z)), (float)(y / (x + y + z)))) : None;
    }

    private static string Spoken(float level) => level.ToString("F3", CultureInfo.InvariantCulture);
}
