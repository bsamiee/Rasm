using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;

namespace Darkroom.Review;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class Verdict {
    public static readonly Verdict Pass = new(0);
    public static readonly Verdict Advisory = new(1);
    public static readonly Verdict Unmeasured = new(2);
    public static readonly Verdict Refuse = new(3);
}

public sealed record Bar(double Advisory, double Refuse) {
    public Verdict Graded(Func<double, bool> holds) => !holds(Refuse) ? Verdict.Refuse : !holds(Advisory) ? Verdict.Advisory : Verdict.Pass;
}

public sealed record ComparisonBars(Bar Psnr, Bar Nrmse) {
    public static ComparisonBars Default { get; } = new(new(Advisory: 35d, Refuse: 30d), new(Advisory: 0.05d, Refuse: 0.1d));

    public Verdict Graded(FrameComparison measured) =>
        (Psnr.Graded(level => measured.Psnr >= level), measured.Nrmse.Match(Some: nrmse => Nrmse.Graded(level => nrmse <= level), None: static () => Verdict.Unmeasured)) switch {
            var (psnr, nrmse) => psnr.Key > nrmse.Key ? psnr : nrmse,
        };
}

public sealed record ComparisonReading(FrameComparison Measured, Verdict Verdict) {
    public Coverage Difference(Gamut gamut) => new Coverage.Matte(MatteChannel.Lightness, Measured.Difference, gamut, Inverted: false);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RenderingComparison : IDisposable {
    private readonly Ref<Held> held = Ref(new Held());
    private readonly Derivations derivations = new();

    private RenderingComparison() => Changed = Subscriptions.Changed(held).Choose(static _ => Some(unit));

    private sealed record Held {
        public Option<TileRequest> Request { get; init; }
        public Option<PixelFrame> Reference { get; init; }
    }

    // --- [ACQUISITION]
    public static PlugInSlot<RenderingComparison> Slot { get; } = new();

    public static Func<IPlugInViews, Action<IDisposable>, IO<Unit>> Register { get; } = static (views, retain) =>
        from comparison in DisposalOps.Acquire(IO.lift(static () => new RenderingComparison()), retain)
        from published in Slot.Publish(views, comparison)
        select unit;

    public void Dispose() => derivations.Dispose();

    // --- [READS]
    public HostEvent<Unit> Changed { get; }

    public IO<Option<TileRequest>> Request => held.ValueIO.Map(static shown => shown.Request);

    public IO<Option<PixelFrame>> Reference => held.ValueIO.Map(static shown => shown.Reference);

    public IO<Option<ComparisonReading>> Measured(PixelFrame current, ComparisonBars bars) =>
        from reference in Reference
        from measured in reference.Traverse(formed => IO.lift(() => FrameComparison.Of(formed, current))).As()
        select measured.Map(comparison => new ComparisonReading(comparison, bars.Graded(comparison)));

    // --- [WRITES]
    public IO<Unit> Shown(Option<TileRequest> next) =>
        from moved in atomicIO(() => (Before: held.Value, After: held.Value = held.Value.Request == next ? held.Value : new Held { Request = next }))
        from formed in when(moved.Before.Request != next, next.Traverse(Landed).As().Map(static _ => unit)).As()
        select unit;

    private IO<Unit> Landed(TileRequest request) =>
        from formed in DisposalOps.OnFailure(request.Formed(derivations), Withdrawn(request))
        from landed in atomicIO(() => held.Swap(prior => prior.Request == Some(request) ? prior with { Reference = Some(formed) } : prior))
        select unit;

    private IO<Unit> Withdrawn(TileRequest request) =>
        atomicIO(() => held.Swap(prior => prior.Request == Some(request) ? new Held() : prior)).Map(static _ => unit);
}
