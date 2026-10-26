using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;
using PeterO.Numbers;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace Rasm.Imaging.Filtering;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Reduction {
    public static Fin<Unit> Quantized(PixelFrame frame, IQuantizer quantizer, IProgress<int> progress) =>
        frame.Mutate(image => image.Mutate(context => context.Quantize(quantizer))).Map(_ => {
            progress.Report(frame.Size.Height);
            return unit;
        });

    public static Fin<Action<Span<Vector4>, long, long>> Posterized(int levels, float strength, float retainHue, ERational cell, int height) =>
        levels >= 2 && cell.Sign > 0
            ? Rows(levels, strength, retainHue, cell, height)
            : Error.New(string.Create(CultureInfo.InvariantCulture, $"Posterize needs at least 2 levels and a positive dither cell, has {levels} levels and cell {cell}"));

    private static Action<Span<Vector4>, long, long> Rows(int levels, float strength, float retainHue, ERational cell, int height) =>
        (row, column, line) => {
            int v = Ordered(ERational.FromInt64((2L * (height - line)) - 1L), cell);
            for (int x = 0; x < row.Length; x++) {
                int b = Ordered(ERational.FromInt64((2L * (column + x)) + 1L), cell) ^ v;
                int rank = ((b & 1) << 5) | ((v & 1) << 4) | ((b & 2) << 2) | ((v & 2) << 1) | ((b & 4) >> 1) | ((v & 4) >> 2);
                row[x] = Leveled(row[x], levels - 1, strength * (((rank + 0.5d) / 64d) - 0.5d), retainHue);
            }
        };

    private static int Ordered(ERational doubled, ERational cell) => WrapMode.Periodic.Fold(doubled.Divide(2).Divide(cell), 8).Point.ToEInteger().ToInt32Checked();

    private static Vector4 Leveled(Vector4 pixel, double steps, double offset, float retainHue) =>
        (Vector256.Floor((Vector256.Clamp(Vector256.Create(pixel.X, pixel.Y, pixel.Z, 0d), Vector256<double>.Zero, Vector256.Create(1d)) * steps) + Vector256.Create(0.5d + offset)) / steps) switch {
            var codes => new Vector4((float)codes.GetElement(0), (float)codes.GetElement(1), (float)codes.GetElement(2), pixel.W) switch {
                var level when retainHue > 0f => new Vector4(Vector3.Lerp(level.AsVector3(), (Hsy.From(level) with { Hue = Hsy.From(pixel).Hue }).ToRgb(pixel.W).AsVector3(), retainHue), pixel.W),
                var level => level,
            },
        };
}
