using System.Numerics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;

namespace Rasm.Imaging.Pixels;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FrameComparison(double Psnr, Option<double> Nrmse, PixelFrame Difference) {
    public static Fin<FrameComparison> Of(PixelFrame reference, PixelFrame test) =>
        reference.Size == test.Size ? Measured(reference, test) : new ImagingFault($"reference size {reference.Size} against test size {test.Size}", ImagingReason.Mismatch);

    private static FrameComparison Measured(PixelFrame reference, PixelFrame test) {
        using MemoryOwner<Measure> rows = MemoryOwner<Measure>.Allocate(test.Size.Height);
        PixelFrame difference = new(test.Origin, test.Size, test.Extent);
        ParallelHelper.For(0, test.Size.Height, new DifferenceAction(reference.View, test.View, difference.View, rows.Memory));
        Measure total = new(0d, 0d, Vector3.One);
        foreach (Measure row in rows.Span) total += row;
        double peak = float.Max(total.Peak.X, float.Max(total.Peak.Y, total.Peak.Z));
        return new(
            10d * double.Log10(peak * peak * 3d * test.Size.Width * test.Size.Height / total.Error),
            total.Signal > 0d ? Some(double.Sqrt(total.Error / total.Signal)) : None,
            difference);
    }
}

file sealed record Measure(double Error, double Signal, Vector3 Peak) {
    public static Measure operator +(Measure left, Measure right) =>
        new(left.Error + right.Error, left.Signal + right.Signal, Vector3.Max(left.Peak, right.Peak));
}

file readonly struct DifferenceAction(Memory2D<Vector4> reference, Memory2D<Vector4> test, Memory2D<Vector4> difference, Memory<Measure> rows) : IAction {
    public void Invoke(int i) {
        ReadOnlySpan<Vector4> a = reference.Span.GetRowSpan(i);
        ReadOnlySpan<Vector4> b = test.Span.GetRowSpan(i);
        Span<Vector4> d = difference.Span.GetRowSpan(i);
        (double error, double signal, Vector3 peak) = (0d, 0d, Vector3.Zero);
        for (int x = 0; x < d.Length; x++) {
            (Vector3 r, Vector3 t) = (a[x].AsVector3(), b[x].AsVector3());
            d[x] = new(Vector3.Abs(r - t), 1f);
            (error, signal, peak) = (error + Vector3.DistanceSquared(r, t), signal + r.LengthSquared(), Vector3.Max(peak, Vector3.Max(r, t)));
        }
        rows.Span[i] = new Measure(error, signal, peak);
    }
}
