using System.Numerics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;

namespace Rasm.Imaging.Pixels;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct BarLevel : IMinMaxValue<BarLevel> {
    public static BarLevel MinValue { get; } = new(0d);
    public static BarLevel MaxValue { get; } = new(double.MaxValue);
    public static BarLevel PsnrAdvisory { get; } = new(35d);
    public static BarLevel PsnrRefuse { get; } = new(30d);
    public static BarLevel NrmseAdvisory { get; } = new(0.05d);
    public static BarLevel NrmseRefuse { get; } = new(0.10d);

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

[SmartEnum<int>]
[ValidationError<InvalidPixelValue>]
public sealed partial class Verdict {
    public static readonly Verdict Pass = new(0);
    public static readonly Verdict Advisory = new(1);
    public static readonly Verdict Unmeasured = new(2);
    public static readonly Verdict Refuse = new(3);

    public Verdict Max(Verdict other) => other > this ? other : this;
}

public sealed record Bar(BarLevel Advisory, BarLevel Refuse) {
    public Verdict Graded(Func<double, bool> holds) =>
        !holds(Refuse) ? Verdict.Refuse : !holds(Advisory) ? Verdict.Advisory : Verdict.Pass;
}

public sealed record ComparisonBars(Bar Psnr, Bar Nrmse);

public sealed class FrameComparison {
    private FrameComparison(double psnr, Option<double> nrmse, PixelFrame difference) => (Psnr, Nrmse, Difference) = (psnr, nrmse, difference);

    public double Psnr { get; }
    public Option<double> Nrmse { get; }
    public PixelFrame Difference { get; }

    public static Fin<FrameComparison> Of(PixelFrame reference, PixelFrame test) =>
        reference.Size == test.Size ? Measured(reference, test) : new UnequalFrames(reference.Size, test.Size);

    public Verdict Graded(ComparisonBars bars) =>
        bars.Psnr.Graded(level => Psnr >= level)
            .Max(Nrmse.Match(Some: nrmse => bars.Nrmse.Graded(level => nrmse <= level), None: static () => Verdict.Unmeasured));

    private static FrameComparison Measured(PixelFrame reference, PixelFrame test) {
        using MemoryOwner<Measure> rows = MemoryOwner<Measure>.Allocate(test.Size.Height);
        PixelFrame difference = new(test.Origin, test.Size, test.Extent, block =>
            ParallelHelper.For(0, test.Size.Height, new DifferenceAction(
                reference.View, test.View, block.AsMemory().Cast<float, Vector4>().AsMemory2D(test.Size.Height, test.Size.Width), rows.Memory)));
        Measure total = new(0d, 0d, Vector3.One);
        foreach (Measure row in rows.Span)
            total += row;
        double peak = float.Max(total.Peak.X, float.Max(total.Peak.Y, total.Peak.Z));
        return new(
            10d * double.Log10(peak * peak * 3d * test.Size.Width * test.Size.Height / total.Error),
            total.Signal > 0d ? Some(double.Sqrt(total.Error / total.Signal)) : None,
            difference);
    }
}

file readonly record struct Measure(double Error, double Signal, Vector3 Peak) {
    public static Measure operator +(Measure left, Measure right) =>
        new(left.Error + right.Error, left.Signal + right.Signal, Vector3.Max(left.Peak, right.Peak));
}

file readonly struct DifferenceAction(Memory2D<Vector4> reference, Memory2D<Vector4> test, Memory2D<Vector4> difference, Memory<Measure> rows) : IAction {
    public void Invoke(int i) {
        ReadOnlySpan<Vector4> a = reference.Span.GetRowSpan(i);
        ReadOnlySpan<Vector4> b = test.Span.GetRowSpan(i);
        Span<Vector4> d = difference.Span.GetRowSpan(i);
        Measure measure = default;
        for (int x = 0; x < d.Length; x++) {
            (Vector3 r, Vector3 t) = (a[x].AsVector3(), b[x].AsVector3());
            d[x] = new(Vector3.Abs(r - t), 1f);
            measure += new Measure(Vector3.DistanceSquared(r, t), r.LengthSquared(), Vector3.Max(r, t));
        }
        rows.Span[i] = measure;
    }
}
