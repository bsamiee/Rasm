using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino;
using SkiaSharp;
using UnitsNet;

namespace Darkroom.Review;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FrameImages {
    private static readonly IProgress<int> Unreported = new Progress<int>();

    private static readonly Func<(SKColorSpaceTransferFn Transfer, Gamut Gamut), Option<SKColorSpace>> Spaces =
        memoUnsafe(static ((SKColorSpaceTransferFn Transfer, Gamut Gamut) space) => Optional(SKColorSpace.CreateRgb(space.Transfer, new SKColorSpacePrimaries(
            (float)space.Gamut.Rgb.ChromaticityR.X, (float)space.Gamut.Rgb.ChromaticityR.Y,
            (float)space.Gamut.Rgb.ChromaticityG.X, (float)space.Gamut.Rgb.ChromaticityG.Y,
            (float)space.Gamut.Rgb.ChromaticityB.X, (float)space.Gamut.Rgb.ChromaticityB.Y,
            (float)space.Gamut.Rgb.WhitePoint.Chromaticity.X, (float)space.Gamut.Rgb.WhitePoint.Chromaticity.Y).ToColorSpaceXyz())));

    extension(PixelFrame frame) {
        public Fin<SKImage> Displayed(DisplayDevice display, Luminance peak) =>
            from signal in Transfer.Pq.Of(display.Encoding.Transfer.White)
            let rolled = Peak.Rolled(signal, display.Peak, peak, 0f, Peak.Bt2390)
            from image in Coded(display.Encoding).Filter(_ => rolled.IsNone).Match(
                Some: space => Read(frame, space),
                None: () => Lit(display.Encoding.Transfer.Light(frame), display.Encoding.Gamut, rolled))
            select image;

        public Fin<SKImage> Masked(Coverage coverage) {
            Half[] halves = GC.AllocateUninitializedArray<Half>(frame.Size.Width * frame.Size.Height);
            Span2D<Half> plane = new(halves, frame.Size.Height, frame.Size.Width);
            Span2D<Vector4> rows = frame.View.Span;
            float[] weights = new float[frame.Size.Width];
            for (int row = 0; row < frame.Size.Height; row++) {
                coverage.Weights(rows.GetRowSpan(row), frame.Origin.X, frame.Line(row), weights);
                TensorPrimitives.ConvertToHalf(weights, plane.GetRowSpan(row));
            }
            return Refused.UnlessFound(
                SKImage.FromPixelCopy(new SKImageInfo(frame.Size.Width, frame.Size.Height, SKColorType.AlphaF16, SKAlphaType.Premul), MemoryMarshal.AsBytes(halves.AsSpan())),
                nameof(SKImage.FromPixelCopy));
        }
    }

    private static Option<SKColorSpace> Coded(ColorEncoding encoding) =>
        encoding.Transfer.Switch(
            linear: static _ => Some(SKColorSpaceTransferFn.Linear),
            srgb: static _ => Some(SKColorSpaceTransferFn.Srgb),
            pq: static _ => Option<SKColorSpaceTransferFn>.None,
            hlg: static _ => Option<SKColorSpaceTransferFn>.None,
            acesCct: static _ => Option<SKColorSpaceTransferFn>.None,
            acesCc: static _ => Option<SKColorSpaceTransferFn>.None,
            gamma: static gamma => Some(new SKColorSpaceTransferFn(gamma.Exponent, 1f, 0f, 0f, 0f, 0f, 0f)),
            camera: static _ => Option<SKColorSpaceTransferFn>.None)
            .Bind(transfer => Spaces((transfer, encoding.Gamut)));

    private static Fin<SKImage> Lit(PixelFrame light, Gamut gamut, Option<Action<Span<Vector4>>> rolled) =>
        from mapped in rolled.Traverse(roll => new PixelPass.Color(roll).Run(light, Unreported)).As()
        from space in Spaces((SKColorSpaceTransferFn.Linear, gamut)).ToFin(new Refused(nameof(SKColorSpace.CreateRgb), Refused.Absent))
        from image in Read(light, space)
        select image;

    private static Fin<SKImage> Read(PixelFrame frame, SKColorSpace space) {
        SKImageInfo source = new(frame.Size.Width, frame.Size.Height, SKColorType.RgbaF32, SKAlphaType.Unpremul, space);
        using SKPixmap pixels = new(source, frame.Address, source.RowBytes);
        using SKBitmap target = new(source.WithColorType(SKColorType.RgbaF16).WithAlphaType(SKAlphaType.Premul));
        using SKPixmap half = target.PeekPixels();
        if (!pixels.ReadPixels(half))
            return new Refused(nameof(SKPixmap.ReadPixels), Refused.Declined);
        target.SetImmutable();
        return Refused.UnlessFound(SKImage.FromBitmap(target), nameof(SKImage.FromBitmap));
    }
}
