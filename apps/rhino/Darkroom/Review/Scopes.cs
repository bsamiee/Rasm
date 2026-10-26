using System.Numerics;
using System.Numerics.Tensors;
using Darkroom.Chain;
using MathNet.Numerics.LinearAlgebra.Double;
using Rasm.Imaging.Color;
using Rasm.Imaging.Color.Calibration;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;

namespace Darkroom.Review;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class TraceMix {
    public static readonly TraceMix RgbLuma = new("rgb-luma", [TracePlane.Red, TracePlane.Green, TracePlane.Blue, TracePlane.Luma]);
    public static readonly TraceMix Rgb = new("rgb", [TracePlane.Red, TracePlane.Green, TracePlane.Blue]);
    public static readonly TraceMix Luma = new("luma", [TracePlane.Luma]);
    public static readonly TraceMix Red = new("red", [TracePlane.Red]);
    public static readonly TraceMix Green = new("green", [TracePlane.Green]);
    public static readonly TraceMix Blue = new("blue", [TracePlane.Blue]);

    public Seq<TracePlane> Planes { get; }
}

[SmartEnum<string>]
public sealed partial class ScopeKind {
    public static readonly ScopeKind Histogram = new("histogram", static (state, frames) =>
        new ScopeRequest.Histogram(frames.Encoding.Gamut, frames.Levels.Map(static levels => levels.Clipping), frames.Input.Map(input => (input, frames.Working)))
            .Read(frames.Frame) switch {
                var reading => new ScopeChart.Counts(
                    frames.Encoding,
                    state.Mix.Planes.Map(plane => (plane, plane.Bins(reading.Bins).Counts)),
                    reading.Input.Map(static input => TracePlane.Luma.Bins(input).Counts),
                    reading.Clipped),
            });
    public static readonly ScopeKind WaveformLuma = new("waveform-luma", static (_, frames) => Waveform(frames, [[TracePlane.Luma]]));
    public static readonly ScopeKind WaveformRgb = new("waveform-rgb", static (_, frames) => Waveform(frames, [[TracePlane.Red, TracePlane.Green, TracePlane.Blue]]));
    public static readonly ScopeKind Parade = new("parade", static (_, frames) => Waveform(frames, [[TracePlane.Red], [TracePlane.Green], [TracePlane.Blue]]));
    public static readonly ScopeKind Vectorscope = new("vectorscope", static (state, frames) =>
        new ScopeRequest.Vectorscope(frames.Encoding.Gamut, PixelExtent.Analysis, state.Magnification).Read(frames.Frame) switch {
            var reading => new ScopeChart.Chroma(frames.Encoding, reading.Bins, reading.Request.Reach),
        });
    public static readonly ScopeKind Chromaticity = new("chromaticity", static (_, frames) =>
        new ScopeRequest.Chromaticity(frames.Encoding.Gamut, frames.Encoding.Transfer, PixelExtent.Analysis).Read(frames.Frame) switch {
            var reading => new ScopeChart.Chromaticities(frames.Encoding, reading.Bins, frames.Working),
        });

    [UseDelegateFromConstructor]
    public partial ScopeChart Read(ScopeState state, ScopeFrames frames);

    private static ScopeChart.Densities Waveform(ScopeFrames frames, Seq<Seq<TracePlane>> columns) =>
        new ScopeRequest.Waveform(frames.Encoding.Gamut, PixelExtent.Analysis).Read(frames.Frame) switch {
            var reading => new(frames.Encoding, columns.Map(planes => planes.Map(plane => (plane, plane.Bins(reading.Bins))))),
        };
}

public sealed record ScopeFrames(PixelFrame Frame, ColorEncoding Encoding, Option<Quantizer> Levels, Gamut Working, Option<PixelFrame> Input) {
    public static ScopeFrames Of(RenderRun run, Option<OutputDepth> depth) =>
        new(run.Formed, DisplayFormation.Display.Encoding, depth.Bind(static held => held.Levels), DisplayFormation.Working, Some(run.Input));
}

public sealed record ScopeState(ScopeKind Kind, TraceMix Mix, float Magnification) {
    public static ScopeState Default { get; } = new(ScopeKind.Histogram, TraceMix.RgbLuma, Magnification: 1f);

    public ScopeChart Read(ScopeFrames frames) => Kind.Read(this, frames);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScopeChart {
    private ScopeChart(ColorEncoding encoding) => Encoding = encoding;

    public ColorEncoding Encoding { get; }

    public abstract Seq<ReadOnlyMemory<int>> Bins { get; }

    public int Peak => Bins.Fold(1, static (peak, bins) => int.Max(peak, TensorPrimitives.Max(bins.Span)));

    public abstract Option<Vector2> Guide(Vector4 sample);

    private double Luma(Vector4 sample) => LuminanceSamples.Luminance(sample.X, sample.Y, sample.Z, Encoding.Gamut.Luminance);

    public sealed record Counts(
        ColorEncoding Encoding, Seq<(TracePlane Plane, ReadOnlyMemory<int> Counts)> Planes, Option<ReadOnlyMemory<int>> Input, Option<Clips> Clipped) : ScopeChart(Encoding) {
        public override Seq<ReadOnlyMemory<int>> Bins => Planes.Map(static plane => plane.Counts) + Input.ToSeq();

        public override Option<Vector2> Guide(Vector4 sample) => Some(new Vector2(HistogramAxis.Bin(Luma(sample)) + 0.5f, 0.5f));
    }

    public sealed record Densities(ColorEncoding Encoding, Seq<Seq<(TracePlane Plane, BinGrid Grid)>> Columns) : ScopeChart(Encoding) {
        public override Seq<ReadOnlyMemory<int>> Bins => Columns.Bind(static column => column.Map(static plane => plane.Grid.Counts));

        public override Option<Vector2> Guide(Vector4 sample) => Some(new Vector2(0.5f, (float)Luma(sample)));
    }

    public sealed record Chroma(ColorEncoding Encoding, BinGrid Grid, float Reach) : ScopeChart(Encoding) {
        private const float BarAmplitude = 0.75f;

        public Seq<Vector2> Bars => toSeq(Range(1, 6)).Map(corner => Polar(new Vector3(corner & 1, (corner >> 1) & 1, (corner >> 2) & 1) * BarAmplitude));

        public Option<Vector2> Skin =>
            ColorChart.ColorCheckerClassic.References(Encoding.Gamut).Find(ChartPatch.LightSkin).Map(skin => Polar(new Vector3((float)skin.R, (float)skin.G, (float)skin.B)));

        public override Seq<ReadOnlyMemory<int>> Bins => [Grid.Counts];

        public override Option<Vector2> Guide(Vector4 sample) => Some(Polar(sample.AsVector3()));

        private Vector2 Polar(Vector3 rgb) =>
            Gamut.ToYpbpr(Encoding.Gamut.Ybr).Multiply(new DenseVector([rgb.X, rgb.Y, rgb.Z])) switch {
                var ypbpr => new Vector2((float)double.Atan2(ypbpr[2], ypbpr[1]), (float)double.Hypot(ypbpr[1], ypbpr[2])),
            };
    }

    public sealed record Chromaticities(ColorEncoding Encoding, BinGrid Grid, Gamut Working) : ScopeChart(Encoding) {
        public Seq<(Gamut Gamut, Seq<Vector2> Corners, Vector2 White)> Gamuts =>
            Seq(Working, Encoding.Gamut).Map(static gamut => (
                gamut,
                Seq(gamut.Rgb.ChromaticityR, gamut.Rgb.ChromaticityG, gamut.Rgb.ChromaticityB).Map(static corner => new Vector2((float)corner.X, (float)corner.Y)),
                new Vector2((float)gamut.Rgb.WhitePoint.Chromaticity.X, (float)gamut.Rgb.WhitePoint.Chromaticity.Y)));

        public override Seq<ReadOnlyMemory<int>> Bins => [Grid.Counts];

        public override Option<Vector2> Guide(Vector4 sample) {
            Vector4 light = sample;
            Encoding.Transfer.Decode(new Span<Vector4>(ref light));
            return Encoding.Gamut.ToXyz.Multiply(new DenseVector([light.X, light.Y, light.Z])) switch {
                var xyz when xyz.Sum() is > 0d and var sum => Some(new Vector2((float)(xyz[0] / sum), (float)(xyz[1] / sum))),
                _ => None,
            };
        }
    }
}
