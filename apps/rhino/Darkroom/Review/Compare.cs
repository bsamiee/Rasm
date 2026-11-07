using Rasm.Imaging.Pixels;

namespace Darkroom.Review;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum]
public sealed partial class Verdict {
    public static readonly Verdict Pass = new(0);
    public static readonly Verdict Advisory = new(1);
    public static readonly Verdict Unmeasured = new(2);
    public static readonly Verdict Refuse = new(3);

    private readonly int severity;

    public Verdict Worse(Verdict other) => other.severity > severity ? other : this;
}

public sealed record Bar(double Advisory, double Refuse) {
    public Verdict Graded(Func<double, bool> holds) => !holds(Refuse) ? Verdict.Refuse : !holds(Advisory) ? Verdict.Advisory : Verdict.Pass;
}

public sealed record ComparisonBars(Bar Psnr, Bar Nrmse) {
    public static ComparisonBars Default { get; } = new(new(Advisory: 35d, Refuse: 30d), new(Advisory: 0.05d, Refuse: 0.1d));

    public Verdict Graded(FrameComparison measured) =>
        Psnr.Graded(level => measured.Psnr >= level).Worse(measured.Nrmse.Match(Some: nrmse => Nrmse.Graded(level => nrmse <= level), None: static () => Verdict.Unmeasured));
}

public sealed record ComparisonReading(FrameComparison Measured, Verdict Verdict) {
    public static IO<ComparisonReading> Of(Rendering reference, Rendering current, ComparisonBars bars) =>
        from frames in (reference.Frame, current.Frame).Apply(static (expected, actual) => (Expected: expected, Actual: actual)).As()
        from measured in IO.lift(FrameComparison.Of(frames.Expected, frames.Actual))
        select new ComparisonReading(measured, bars.Graded(measured));
}
