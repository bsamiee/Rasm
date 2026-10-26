using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rasm.UI;
using UnitsNet;
using UnitsNet.Units;

namespace Darkroom.Review;

// --- [TYPES] ---------------------------------------------------------------------------
public enum Verdict {
    Pass,
    Advisory,
    Unmeasured,
    Refuse,
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Bar(Magnitude Advisory, Magnitude Refuse) {
    public Verdict Graded(Func<double, bool> holds) => !holds(Refuse) ? Verdict.Refuse : !holds(Advisory) ? Verdict.Advisory : Verdict.Pass;
}

public sealed record ComparisonBars(Bar Psnr, Bar Nrmse) : IStateRecord<ComparisonBars, ComparisonBarsParameter> {
    public static ComparisonBars Default { get; } = new(new(Magnitude.Create(35f), Magnitude.Create(30f)), new(Magnitude.Create(0.05f), Magnitude.Create(0.1f)));

    public Verdict Graded(FrameComparison measured) =>
        (Psnr.Graded(level => measured.Psnr >= level), measured.Nrmse.Match(Some: nrmse => Nrmse.Graded(level => nrmse <= level), None: static () => Verdict.Unmeasured)) switch {
            var (psnr, nrmse) => psnr > nrmse ? psnr : nrmse,
        };
}

[SmartEnum<string>]
public sealed partial class ComparisonBarsParameter : IStateParameter<ComparisonBars> {
    private static readonly Presentation<float> Decibels = new() { Unit = Quantity.GetUnitInfo(LevelUnit.Decibel), Decimals = 1 };
    private static readonly Presentation<float> Normalized = new() { Decimals = 2 };
    private static readonly Lens<ComparisonBars, Bar> PsnrBar = Lens<ComparisonBars, Bar>.New(static bars => bars.Psnr, static psnr => bars => bars with { Psnr = psnr });
    private static readonly Lens<ComparisonBars, Bar> NrmseBar = Lens<ComparisonBars, Bar>.New(static bars => bars.Nrmse, static nrmse => bars => bars with { Nrmse = nrmse });
    private static readonly Lens<Bar, Magnitude> AdvisoryLevel = Lens<Bar, Magnitude>.New(static bar => bar.Advisory, static level => bar => bar with { Advisory = level });
    private static readonly Lens<Bar, Magnitude> RefuseLevel = Lens<Bar, Magnitude>.New(static bar => bar.Refuse, static level => bar => bar with { Refuse = level });

    public static readonly ComparisonBarsParameter PsnrAdvisory = new("psnr-advisory", StateParameter.Bounded<ComparisonBars, Magnitude, float>(lens(PsnrBar, AdvisoryLevel), Decibels),
        new("PSNR advisory", "PSNR in decibels below which compared renders grade advisory"));
    public static readonly ComparisonBarsParameter PsnrRefuse = new("psnr-refuse", StateParameter.Bounded<ComparisonBars, Magnitude, float>(lens(PsnrBar, RefuseLevel), Decibels),
        new("PSNR refuse", "PSNR in decibels below which compared renders are refused"));
    public static readonly ComparisonBarsParameter NrmseAdvisory = new("nrmse-advisory", StateParameter.Bounded<ComparisonBars, Magnitude, float>(lens(NrmseBar, AdvisoryLevel), Normalized),
        new("NRMSE advisory", "Normalized error above which compared renders grade advisory"));
    public static readonly ComparisonBarsParameter NrmseRefuse = new("nrmse-refuse", StateParameter.Bounded<ComparisonBars, Magnitude, float>(lens(NrmseBar, RefuseLevel), Normalized),
        new("NRMSE refuse", "Normalized error above which compared renders are refused"));

    public StateParameter<ComparisonBars> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
public sealed partial class CompareMode : ICaptioned {
    public static readonly CompareMode VerticalWipe = new("vertical-wipe", Some(Orientation.Vertical));
    public static readonly CompareMode HorizontalWipe = new("horizontal-wipe", Some(Orientation.Horizontal));
    public static readonly CompareMode SideBySide = new("side-by-side", Option<Orientation>.None);

    public Option<Orientation> Rule { get; }
}

public sealed record CompareState(CompareMode Mode, Fraction Split, bool Swapped) {
    public static CompareState Default { get; } = new(CompareMode.VerticalWipe, Fraction.Create(0.5f), Swapped: false);

    public Option<TileRequest> Reference { get; init; }
}

public sealed record ComparisonReading(FrameComparison Measured, Verdict Verdict) {
    public Coverage Difference(Gamut gamut) => new Coverage.Matte(MatteChannel.Lightness, Measured.Difference, gamut, Inverted: false);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RenderingComparison : IDisposable {
    private readonly Ref<Held> held = Ref(new Held(CompareState.Default));
    private readonly Derivations derivations = new();

    private RenderingComparison() => Changed = Subscriptions.Changed(held).Choose(static _ => Some(unit));

    private sealed record Held(CompareState State) {
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

    public IO<CompareState> State => held.ValueIO.Map(static shown => shown.State);

    public IO<Option<PixelFrame>> Reference => held.ValueIO.Map(static shown => shown.Reference);

    public IO<Option<ComparisonReading>> Measured(PixelFrame current, ComparisonBars bars) =>
        from reference in Reference
        from measured in reference.Traverse(formed => IO.lift(() => FrameComparison.Of(formed, current))).As()
        select measured.Map(comparison => new ComparisonReading(comparison, bars.Graded(comparison)));

    // --- [WRITES]
    public IO<Unit> Shown(CompareState next) =>
        from moved in atomicIO(() => (Before: held.Value, After: held.Value = held.Value.State.Reference == next.Reference ? held.Value with { State = next } : new Held(next)))
        from formed in when(moved.Before.State.Reference != next.Reference, next.Reference.Traverse(Landed).As().Map(static _ => unit)).As()
        select unit;

    private IO<Unit> Landed(TileRequest request) =>
        from formed in DisposalOps.OnFailure(request.Formed(derivations), Withdrawn(request))
        from landed in atomicIO(() => held.Swap(prior => prior.State.Reference == Some(request) ? prior with { Reference = Some(formed) } : prior))
        select unit;

    private IO<Unit> Withdrawn(TileRequest request) =>
        atomicIO(() => held.Swap(prior => prior.State.Reference == Some(request) ? new Held(prior.State with { Reference = None }) : prior)).Map(static _ => unit);
}
