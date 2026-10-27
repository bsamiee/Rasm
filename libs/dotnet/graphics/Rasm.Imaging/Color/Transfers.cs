using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Pixels;
using TinyEXR;
using UnitsNet;
using Wacton.Unicolour;

namespace Rasm.Imaging.Color;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
internal abstract partial record LogPiece {
    public abstract double Encode(double linear);
    public abstract double Decode(double code);

    public sealed record Logarithm(double Base, double LogSideSlope, double LogSideOffset, double LinSideSlope, double LinSideOffset) : LogPiece {
        public override double Encode(double linear) => (LogSideSlope * double.Log((LinSideSlope * linear) + LinSideOffset, Base)) + LogSideOffset;
        public override double Decode(double code) => (double.Pow(Base, (code - LogSideOffset) / LogSideSlope) - LinSideOffset) / LinSideSlope;
    }

    public sealed record Line(double Slope, double Offset) : LogPiece {
        public override double Encode(double linear) => (Slope * linear) + Offset;
        public override double Decode(double code) => (code - Offset) / Slope;
    }

    public sealed record Power(double Scale, double Shift, double Exponent) : LogPiece {
        public override double Encode(double linear) => Scale * double.Pow(double.Max(linear + Shift, 0d), Exponent);
        public override double Decode(double code) => double.Pow(double.Max(code / Scale, 0d), 1d / Exponent) - Shift;
    }
}

[SmartEnum<string>]
public sealed partial class CameraLog {
    public static readonly CameraLog ArriLogC3 = LogCamera("arri-logc3", new(10d, 0.247189638318671, 0.385536998692443, 5.55555555555556, 0.0522722750251688), 0.0105909904954696);
    public static readonly CameraLog ArriLogC4 = LogCamera("arri-logc4", new(2d, 0.0647954196341293, -0.295908392682586, 2231.82630906769, 64d), -0.0180569961199113);
    public static readonly CameraLog SonySLog2 = ((940d - 64d) / 1023d) switch {
        var legal => LogCamera("sony-slog2", new(10d, 0.432699 * legal, (0.646596 * legal) + (64d / 1023d), 155d / (219d * 0.9), 0.037584), 0d),
    };
    public static readonly CameraLog SonySLog3 = LogCamera(
        "sony-slog3", new(10d, 261.5 / 1023d, 420d / 1023d, 1d / (0.18 + 0.01), 0.01 / (0.18 + 0.01)), 0.01125, Some((171.2102946929 - 95d) / 0.01125 / 1023d));
    public static readonly CameraLog CanonLog2 = new(
        "canon-log2",
        new LogPiece.Logarithm(10d, -0.24136077, 0.092864125, -87.099375 / 0.9, 1d),
        [(0d, new LogPiece.Logarithm(10d, 0.24136077, 0.092864125, 87.099375 / 0.9, 1d))]);
    public static readonly CameraLog CanonLog3 = new(
        "canon-log3",
        new LogPiece.Logarithm(10d, -0.36726845, 0.12783901, -14.98325 / 0.9, 1d),
        [(-0.014 * 0.9, new LogPiece.Line(1.9754798 / 0.9, 0.12512219)), (0.014 * 0.9, new LogPiece.Logarithm(10d, 0.36726845, 0.12240537, 14.98325 / 0.9, 1d))]);
    public static readonly CameraLog RedLog3G10 = LogCamera("red-log3g10", new(10d, 0.224282, 0d, 155.975327, (0.01 * 155.975327) + 1d), -0.01);
    public static readonly CameraLog PanasonicVLog = LogCamera("panasonic-vlog", new(10d, 0.241514, 0.598206, 1d, 0.00873), 0.01);
    public static readonly CameraLog FujifilmFLog = LogCamera("fujifilm-flog", new(10d, 0.344676, 0.790453, 0.555556, 0.009468), 0.00089, Some(8.735631));
    public static readonly CameraLog FujifilmFLog2 = LogCamera("fujifilm-flog2", new(10d, 0.245281, 0.384316, 5.555556, 0.064829), 0.000889, Some(8.799461));
    public static readonly CameraLog DjiDLog = LogCamera("dji-dlog", new(10d, 0.256662970719888, 0.58455504907396, 0.9892, 0.0108), 0.00758078675);
    public static readonly CameraLog GoProProtune = new("gopro-protune", new LogPiece.Logarithm(113d, 1d, 0d, 112d, 1d), []);
    public static readonly CameraLog NikonNLog = new(
        "nikon-nlog", new LogPiece.Power(650d / 1023d, 0.0075, 1d / 3d), [(0.328, new LogPiece.Logarithm(double.E, 150d / 1023d, 619d / 1023d, 1d, 0d))]);
    public static readonly CameraLog AppleLog = new(
        "apple-log", new LogPiece.Power(47.28711236, 0.05641088, 2d), [(0.01, new LogPiece.Logarithm(2d, 0.08550479, 0.69336945, 1d, 0.00964052))]);
    public static readonly CameraLog BlackmagicFilmGen5 = LogCamera(
        "blackmagic-film-gen5", new(double.E, 0.0869287606549122, 0.530013339229194, 1d, 0.00549407243225781), 0.005);
    public static readonly CameraLog DaVinciIntermediate = LogCamera("davinci-intermediate", new(2d, 0.07329248, 0.07329248 * 7d, 1d, 0.0075), 0.00262409, Some(10.44426855));
    public static readonly CameraLog FilmLightTLog = ((1d - 0.075) / (16d * (0.7107 + (1.2359 * double.Log(128d * 16d))))) switch {
        var linSideOffset => ((1d - 0.075) / double.Log(1d + (128d / linSideOffset))) switch {
            var logSideSlope => LogCamera("filmlight-tlog", new(double.E, logSideSlope, 1d - (logSideSlope * double.Log(128d + linSideOffset)), 1d, linSideOffset), 0d),
        },
    };

    private readonly LogPiece first;
    private readonly Seq<(double Break, LogPiece Piece)> joints;

    public double Encode(double linear) => Encoded(joints.AsSpan(), linear);

    public double Decode(double code) => Decoded(joints.AsSpan(), code);

    private double Encoded(ReadOnlySpan<(double Break, LogPiece Piece)> above, double linear) =>
        above.IsEmpty ? first.Encode(linear)
        : linear >= above[^1].Break ? above[^1].Piece.Encode(linear)
        : Encoded(above[..^1], linear);

    private double Decoded(ReadOnlySpan<(double Break, LogPiece Piece)> above, double code) =>
        above.IsEmpty ? first.Decode(code)
        : above[^1].Piece.Decode(code) is var linear && linear >= above[^1].Break ? linear
        : Decoded(above[..^1], code);

    private static CameraLog LogCamera(string key, LogPiece.Logarithm log, double linSideBreak, Option<double> linearSlope = default) =>
        linearSlope.IfNone(() => log.LogSideSlope * log.LinSideSlope / (((log.LinSideSlope * linSideBreak) + log.LinSideOffset) * double.Log(log.Base))) switch {
            var slope => new(key, new LogPiece.Line(slope, log.Encode(linSideBreak) - (slope * linSideBreak)), [(linSideBreak, log)]),
        };
}

[SmartEnum<string>]
internal sealed partial class TransferForm {
    public static readonly TransferForm Gamma = new("gamma", static level => Transfer.Gamma.Of((float)level).ToOption().Map(static gamma => (Transfer)gamma));
    public static readonly TransferForm Pq = new("pq", static level => Transfer.Pq.Of(Luminance.FromNits(level)).ToOption().Map(static pq => (Transfer)pq));
    public static readonly TransferForm Hlg = new("hlg", static level => Transfer.Hlg.Of(Luminance.FromNits(level)).ToOption().Map(static hlg => (Transfer)hlg));

    [UseDelegateFromConstructor]
    public partial Option<Transfer> At(double level);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
[ValidationError<ImagingFault>]
public abstract partial record Transfer : IConvertible<string> {
    private static readonly Luminance ReferenceWhite = Luminance.FromNits(DynamicRange.Standard.WhiteLuminance);
    private static readonly FrozenDictionary<string, Transfer> Fixed =
        Seq<Transfer>(new Linear(), new Srgb(), new AcesCct(), new AcesCc()).Concat(toSeq(CameraLog.Items).Map(static log => (Transfer)new Camera(log))).ToFrozenDictionary(static transfer => transfer.ToValue(), StringComparer.Ordinal);

    private interface IChannelCurve { internal double Encode(double value); internal double Decode(double value); }

    public virtual Luminance White => ReferenceWhite;
    internal virtual Option<(int Function, Seq<double> Parameters)> IccCurve => None;

    public abstract void Encode(Span<Vector4> row);
    public abstract void Decode(Span<Vector4> row);

    public PixelFrame Light(PixelFrame frame) =>
        new(frame.Origin, frame.Size, frame.Extent, block => {
            frame.Block.CopyTo(block);
            Decode(MemoryMarshal.Cast<float, Vector4>(block.AsSpan()));
        });

    public string ToValue() => Switch(
        linear: static _ => "linear",
        srgb: static _ => "srgb",
        pq: static pq => string.Create(CultureInfo.InvariantCulture, $"{TransferForm.Pq.Key}:{pq.White.Nits.ToDouble()}"),
        hlg: static hlg => string.Create(CultureInfo.InvariantCulture, $"{TransferForm.Hlg.Key}:{hlg.White.Nits.ToDouble()}"),
        acesCct: static _ => "acescct",
        acesCc: static _ => "acescc",
        gamma: static gamma => string.Create(CultureInfo.InvariantCulture, $"{TransferForm.Gamma.Key}:{gamma.Exponent}"),
        camera: static camera => camera.Log.Key);

    public static Fin<Transfer> Read(string value) => Named(value).ToFin(Unnamed(value));

    public static ImagingFault? Validate(string? value, IFormatProvider? provider, out Transfer? item) {
        item = value is null ? null : Named(value).ValueUnsafe();
        return value is not null && item is null ? Unnamed(value) : null;
    }

    private static Option<Transfer> Named(string value) => Optional(Fixed.GetValueOrDefault(value)) | Parametric(value);

    private static ImagingFault Unnamed(string value) =>
        new($"transfer '{value}' beside curve and camera log names, gamma:<exponent>, pq:<nits>, and hlg:<nits>", ImagingReason.UnknownFormat);

    private static Option<Transfer> Parametric(string value) =>
        from parts in value.Split(':', 2) is [var form, var number] ? Some((Form: form, Number: number)) : None
        from family in TransferForm.TryGet(parts.Form, out TransferForm? named) ? Some(named) : None
        from level in double.TryParse(parts.Number, NumberStyles.Float, CultureInfo.InvariantCulture, out double read) ? Some(read) : None
        from transfer in family.At(level)
        select transfer;

    private static Fin<Luminance> Lit(Luminance white) =>
        double.IsFinite(white.Nits.ToDouble()) && white.Nits.ToDouble() > 0d ? white : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"transfer white {white}"), ImagingReason.Range);

    private static void PerChannel<TCurve>(Span<Vector4> row, TCurve curve, bool encode) where TCurve : struct, IChannelCurve {
        for (int index = 0; index < row.Length; index++)
            row[index] = new Vector4((float)At(row[index].X), (float)At(row[index].Y), (float)At(row[index].Z), row[index].W);

        double At(double value) => encode ? curve.Encode(value) : curve.Decode(value);
    }

    private static void Scaled(Span<Vector4> row, float scale) {
        for (int index = 0; index < row.Length; index++)
            row[index] = new Vector4(row[index].AsVector3() * scale, row[index].W);
    }

    private static void Coded(Span<Vector4> row, TransferFunction curve, Action<ReadOnlySpan<float>, Span<float>, TransferFunction> code) {
        Span2D<float> samples = MemoryMarshal.Cast<Vector4, float>(row).AsSpan2D(row.Length, 4);
        using SpanOwner<float> alpha = SpanOwner<float>.Allocate(row.Length);
        samples.GetColumn(3).CopyTo(alpha.Span);
        code(MemoryMarshal.Cast<Vector4, float>(row), MemoryMarshal.Cast<Vector4, float>(row), curve);
        alpha.Span.CopyTo(samples.GetColumn(3));
    }

    public sealed record Linear : Transfer {
        internal override Option<(int Function, Seq<double> Parameters)> IccCurve => (0, Seq(1d));
        public override void Encode(Span<Vector4> row) { }
        public override void Decode(Span<Vector4> row) { }
    }

    public sealed record Srgb : Transfer {
        internal override Option<(int Function, Seq<double> Parameters)> IccCurve => (3, Seq(2.4, 1d / 1.055, 1d - (1d / 1.055), 1d / 12.92, 0.04045));
        public override void Encode(Span<Vector4> row) => Coded(row, TransferFunction.Srgb, ImageProcessing.EncodeTransfer);
        public override void Decode(Span<Vector4> row) => Coded(row, TransferFunction.Srgb, ImageProcessing.DecodeTransfer);
    }

    public sealed record Pq : Transfer {
        private static readonly Luminance Peak = Luminance.FromNits(10_000d);

        private Pq(Luminance white) => White = white;

        public static Pq Standard { get; } = new(ReferenceWhite);
        public static Pq High { get; } = new(Luminance.FromNits(DynamicRange.High.WhiteLuminance));

        public override Luminance White { get; }

        public static Fin<Pq> Of(Luminance white) => Lit(white).Map(static lit => new Pq(lit));

        public override void Encode(Span<Vector4> row) {
            Scaled(row, (float)(White / Peak).ToDouble());
            Coded(row, TransferFunction.Pq, ImageProcessing.EncodeTransfer);
        }

        public override void Decode(Span<Vector4> row) {
            Coded(row, TransferFunction.Pq, ImageProcessing.DecodeTransfer);
            Scaled(row, (float)(Peak / White).ToDouble());
        }
    }

    public sealed record Hlg : Transfer {
        private static readonly Luminance Peak = Luminance.FromNits(DynamicRange.High.MaxLuminance);
        private static readonly (Vector256<double> Weights, double Gamma, double SceneFloor, double DisplayFloor) Ootf =
            (Gamut.Rec2020.Luminance, 1.2d + (0.42d * double.Log10(Peak.Nits.ToDouble() / 1_000d))) switch {
                var (weights, gamma) => (weights, gamma, double.Pow(1e-4d, 1d / gamma) / 3d, 1e-4d / double.Pow(3d, gamma)),
            };

        private Hlg(Luminance white) => White = white;

        public static Hlg Standard { get; } = new(ReferenceWhite);
        public static Hlg High { get; } = new(Luminance.FromNits(DynamicRange.High.WhiteLuminance));

        public override Luminance White { get; }

        public static Fin<Hlg> Of(Luminance white) => Lit(white).Map(static lit => new Hlg(lit));

        public override void Encode(Span<Vector4> row) {
            double scale = (White / Peak).ToDouble();
            foreach (ref Vector4 pixel in row) {
                Vector256<double> display = Vector256.Create(pixel.X, pixel.Y, pixel.Z, 0d) * scale;
                double luminance = Vector256.Dot(display, Ootf.Weights);
                Vector256<double> scene = display * double.Pow(double.Max(luminance, Ootf.DisplayFloor), (1d - Ootf.Gamma) / Ootf.Gamma);
                pixel = new Vector4((float)scene[0], (float)scene[1], (float)scene[2], pixel.W);
            }
            Coded(row, TransferFunction.Hlg, ImageProcessing.EncodeTransfer);
        }

        public override void Decode(Span<Vector4> row) {
            double scale = (Peak / White).ToDouble();
            Coded(row, TransferFunction.Hlg, ImageProcessing.DecodeTransfer);
            foreach (ref Vector4 pixel in row) {
                Vector256<double> scene = Vector256.Create(pixel.X, pixel.Y, pixel.Z, 0d);
                double luminance = Vector256.Dot(scene, Ootf.Weights);
                Vector256<double> display = scene * (double.Pow(double.Max(luminance, Ootf.SceneFloor), Ootf.Gamma - 1d) * scale);
                pixel = new Vector4((float)display[0], (float)display[1], (float)display[2], pixel.W);
            }
        }
    }

    public sealed record AcesCct : Transfer {
        public override void Encode(Span<Vector4> row) => PerChannel(row, default(Coding), encode: true);
        public override void Decode(Span<Vector4> row) => PerChannel(row, default(Coding), encode: false);
        private readonly struct Coding : IChannelCurve { double IChannelCurve.Encode(double value) => RgbModels.Acescct.FromLinear(value); double IChannelCurve.Decode(double value) => RgbModels.Acescct.ToLinear(value); }
    }

    public sealed record AcesCc : Transfer {
        public override void Encode(Span<Vector4> row) => PerChannel(row, default(Coding), encode: true);
        public override void Decode(Span<Vector4> row) => PerChannel(row, default(Coding), encode: false);
        private readonly struct Coding : IChannelCurve { double IChannelCurve.Encode(double value) => RgbModels.Acescc.FromLinear(value); double IChannelCurve.Decode(double value) => RgbModels.Acescc.ToLinear(value); }
    }

    public sealed record Gamma : Transfer {
        private Gamma(float exponent) => Exponent = exponent;

        public static Gamma Gamma22 { get; } = new(2.2f);
        public static Gamma Rec1886 { get; } = new(2.4f);

        public float Exponent { get; }
        internal override Option<(int Function, Seq<double> Parameters)> IccCurve => (0, Seq<double>(Exponent));

        public static Fin<Gamma> Of(float exponent) =>
            float.IsFinite(exponent) && exponent > 0f ? new Gamma(exponent) : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"gamma exponent {exponent}"), ImagingReason.Range);

        public override void Encode(Span<Vector4> row) => Code(row, encode: true);
        public override void Decode(Span<Vector4> row) => Code(row, encode: false);

        private void Code(Span<Vector4> row, bool encode) {
            if (Curve.ToSpan() is [var curve]) Coded(row, curve, encode ? ImageProcessing.EncodeTransfer : ImageProcessing.DecodeTransfer);
            else PerChannel(row, new Raised(Exponent), encode);
        }

        private readonly struct Raised(double exponent) : IChannelCurve { double IChannelCurve.Encode(double value) => double.Pow(double.Max(value, 0d), 1d / exponent); double IChannelCurve.Decode(double value) => double.Pow(double.Max(value, 0d), exponent); }

        private Option<TransferFunction> Curve => Exponent switch {
            2.2f => TransferFunction.Gamma22,
            2.4f => TransferFunction.Gamma24,
            _ => None,
        };
    }

    public sealed record Camera(CameraLog Log) : Transfer {
        public override void Encode(Span<Vector4> row) => PerChannel(row, new Coding(Log), encode: true);
        public override void Decode(Span<Vector4> row) => PerChannel(row, new Coding(Log), encode: false);
        private readonly struct Coding(CameraLog log) : IChannelCurve { double IChannelCurve.Encode(double value) => log.Encode(value); double IChannelCurve.Decode(double value) => log.Decode(value); }
    }
}

public sealed record ColorEncoding(Gamut Gamut, Transfer Transfer) {
    public Seq<Action<Span<Vector4>>> To(ColorEncoding target) =>
        this == target
            ? Seq<Action<Span<Vector4>>>()
            : (Transfer is Transfer.Linear ? Seq<Action<Span<Vector4>>>() : Seq(Transfer.Decode))
              + LinearTo(target).Map<Action<Span<Vector4>>>(static table => table.Apply)
              + (target.Transfer is Transfer.Linear ? Seq<Action<Span<Vector4>>>() : Seq(target.Transfer.Encode));

    internal Seq<LutTable> LinearTo(ColorEncoding target) =>
        Gamut == target.Gamut && Transfer.White == target.Transfer.White ? [] : [new LutTable.Affine(Gamut.MatrixTo(target.Gamut).Multiply((Transfer.White / target.Transfer.White).ToDouble()))];
}
