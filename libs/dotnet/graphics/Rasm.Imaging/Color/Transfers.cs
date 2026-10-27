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
            (Gamut.Rec2020.LuminanceLanes, 1.2d + (0.42d * double.Log10(Peak.Nits.ToDouble() / 1_000d))) switch {
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

        public override void Encode(Span<Vector4> row) {
            if (Curve.ToSpan() is [var curve]) Coded(row, curve, ImageProcessing.EncodeTransfer);
            else PerChannel(row, new Raised(Exponent), encode: true);
        }

        public override void Decode(Span<Vector4> row) {
            if (Curve.ToSpan() is [var curve]) Coded(row, curve, ImageProcessing.DecodeTransfer);
            else PerChannel(row, new Raised(Exponent), encode: false);
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
