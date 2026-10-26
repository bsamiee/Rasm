using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
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
    private static readonly HashMap<string, Transfer> Fixed =
        toHashMap(Seq<Transfer>(new Linear(), new Srgb(), new AcesCct(), new AcesCc()).Concat(toSeq(CameraLog.Items).Map(static log => (Transfer)new Camera(log)))
            .Map(static transfer => (transfer.ToValue(), transfer)));

    public virtual Luminance White => ReferenceWhite;

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

    private static Option<Transfer> Named(string value) => Fixed.Find(value) | Parametric(value);

    private static ImagingFault Unnamed(string value) =>
        new($"transfer '{value}' beside curve and camera log names, gamma:<exponent>, pq:<nits>, and hlg:<nits>", ImagingReason.Unnamed);

    private static Option<Transfer> Parametric(string value) =>
        from parts in value.Split(':', 2) is [var form, var number] ? Some((Form: form, Number: number)) : None
        from family in TransferForm.TryGet(parts.Form, out TransferForm? named) ? Some(named) : None
        from level in double.TryParse(parts.Number, NumberStyles.Float, CultureInfo.InvariantCulture, out double read) ? Some(read) : None
        from transfer in family.At(level)
        select transfer;

    private static Fin<Luminance> Lit(Luminance white) =>
        double.IsFinite(white.Nits.ToDouble()) && white.Nits.ToDouble() > 0d ? white : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"transfer white {white}"), ImagingReason.Range);

    private static void PerChannel(Span<Vector4> row, Func<double, double> curve) {
        for (int index = 0; index < row.Length; index++)
            row[index] = new Vector4((float)curve(row[index].X), (float)curve(row[index].Y), (float)curve(row[index].Z), row[index].W);
    }

    private static void Scaled(Span<Vector4> row, float scale) {
        for (int index = 0; index < row.Length; index++)
            row[index] = new Vector4(row[index].AsVector3() * scale, row[index].W);
    }

    private static Span<float> Components(ref Vector4 pixel) => MemoryMarshal.CreateSpan(ref pixel.X, 3);

    private static void Encoded(Span<Vector4> row, TransferFunction curve) {
        Span2D<float> pixels = MemoryMarshal.Cast<Vector4, float>(row).AsSpan2D(row.Length, 4).Slice(0, 0, row.Length, 3);
        for (int pixel = 0; pixel < pixels.Height; pixel++)
            ImageProcessing.EncodeTransfer(pixels.GetRowSpan(pixel), pixels.GetRowSpan(pixel), curve);
    }

    private static void Decoded(Span<Vector4> row, TransferFunction curve) {
        Span2D<float> pixels = MemoryMarshal.Cast<Vector4, float>(row).AsSpan2D(row.Length, 4).Slice(0, 0, row.Length, 3);
        for (int pixel = 0; pixel < pixels.Height; pixel++)
            ImageProcessing.DecodeTransfer(pixels.GetRowSpan(pixel), pixels.GetRowSpan(pixel), curve);
    }

    public sealed record Linear : Transfer {
        public override void Encode(Span<Vector4> row) { }
        public override void Decode(Span<Vector4> row) { }
    }

    public sealed record Srgb : Transfer {
        public override void Encode(Span<Vector4> row) => Encoded(row, TransferFunction.Srgb);
        public override void Decode(Span<Vector4> row) => Decoded(row, TransferFunction.Srgb);
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
            Encoded(row, TransferFunction.Pq);
        }

        public override void Decode(Span<Vector4> row) {
            Decoded(row, TransferFunction.Pq);
            Scaled(row, (float)(Peak / White).ToDouble());
        }
    }

    public sealed record Hlg : Transfer {
        private static readonly Luminance Peak = Luminance.FromNits(DynamicRange.High.MaxLuminance);
        private static readonly (Vector256<double> Weights, double Gamma, double SceneFloor, double DisplayFloor) Ootf =
            (Gamut.Rec2020.Luminance, 1.2d + (0.42d * double.Log10(Peak.Nits.ToDouble() / 1_000d))) switch {
                var ((r, g, b), gamma) => (Vector256.Create(r, g, b, 0d), gamma, double.Pow(1e-4d, 1d / gamma) / 3d, 1e-4d / double.Pow(3d, gamma)),
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
                ImageProcessing.EncodeTransfer(Components(ref pixel), Components(ref pixel), TransferFunction.Hlg);
            }
        }

        public override void Decode(Span<Vector4> row) {
            double scale = (Peak / White).ToDouble();
            foreach (ref Vector4 pixel in row) {
                ImageProcessing.DecodeTransfer(Components(ref pixel), Components(ref pixel), TransferFunction.Hlg);
                Vector256<double> scene = Vector256.Create(pixel.X, pixel.Y, pixel.Z, 0d);
                double luminance = Vector256.Dot(scene, Ootf.Weights);
                Vector256<double> display = scene * (double.Pow(double.Max(luminance, Ootf.SceneFloor), Ootf.Gamma - 1d) * scale);
                pixel = new Vector4((float)display[0], (float)display[1], (float)display[2], pixel.W);
            }
        }
    }

    public sealed record AcesCct : Transfer {
        public override void Encode(Span<Vector4> row) => PerChannel(row, RgbModels.Acescct.FromLinear);
        public override void Decode(Span<Vector4> row) => PerChannel(row, RgbModels.Acescct.ToLinear);
    }

    public sealed record AcesCc : Transfer {
        public override void Encode(Span<Vector4> row) => PerChannel(row, RgbModels.Acescc.FromLinear);
        public override void Decode(Span<Vector4> row) => PerChannel(row, RgbModels.Acescc.ToLinear);
    }

    public sealed record Gamma : Transfer {
        private Gamma(float exponent) => Exponent = exponent;

        public static Gamma Gamma22 { get; } = new(2.2f);
        public static Gamma Rec1886 { get; } = new(2.4f);

        public float Exponent { get; }

        public static Fin<Gamma> Of(float exponent) =>
            float.IsFinite(exponent) && exponent > 0f ? new Gamma(exponent) : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"gamma exponent {exponent}"), ImagingReason.Range);

        public override void Encode(Span<Vector4> row) {
            if (Curve.ToSpan() is [var curve]) Encoded(row, curve);
            else PerChannel(row, linear => double.Pow(double.Max(linear, 0d), 1d / Exponent));
        }

        public override void Decode(Span<Vector4> row) {
            if (Curve.ToSpan() is [var curve]) Decoded(row, curve);
            else PerChannel(row, signal => double.Pow(double.Max(signal, 0d), Exponent));
        }

        private Option<TransferFunction> Curve => Exponent switch {
            2.2f => TransferFunction.Gamma22,
            2.4f => TransferFunction.Gamma24,
            _ => None,
        };
    }

    public sealed record Camera(CameraLog Log) : Transfer {
        public override void Encode(Span<Vector4> row) => PerChannel(row, Log.Encode);
        public override void Decode(Span<Vector4> row) => PerChannel(row, Log.Decode);
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
        (Transfer.White / target.Transfer.White).ToDouble() switch {
            1d when Gamut == target.Gamut => [],
            var white => [new LutTable.Affine(Gamut.MatrixTo(target.Gamut).Multiply(white))],
        };
}
