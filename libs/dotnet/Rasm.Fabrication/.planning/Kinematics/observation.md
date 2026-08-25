# [RASM_FABRICATION_MACHINE_OBSERVATION]

`MachineObservation` is the one typed vocabulary for decoded spindle, feed, temperature, execution, condition, and tool-state telemetry. Rows admit once from `MachineObservationIngress`; every measured wear, fleet, and load-ceiling consumer reads this slice instead of a transport shape. `MachineObservations` is the machine-scoped ordered window deriving span, latest value, producing fraction, fault episodes, and load-ceiling evidence once.

Machine telemetry enters through the AppHost decode lane; this page admits provider-neutral ingress rows, and no transport, agent, or stream reference crosses the package boundary. `ToolCatalog.Admit` keeps the cutting-tool asset slice on `Tooling/magazine.md` — assets are ISO-13399 lifecycle documents, observations are streamed process state, and the two admissions never share a vocabulary.

## [01]-[INDEX]

- [02]-[MACHINE_OBSERVATION]: `MachineObservation` decoded-telemetry union, `ExecutionState` and `ConditionSeverity` federated vocabularies, the one `Admit` decode seam, and the `MachineObservations` window with its `LoadWindow` ceiling evidence.

## [02]-[MACHINE_OBSERVATION]

- Owner: `MachineObservation` — the closed decoded-telemetry union with the observation instant threaded through the base; `ExecutionState` and `ConditionSeverity` — the federated controller-state and condition-level vocabularies; `MachineObservations` — the window scoped to one `MachineInstanceKey`; `LoadWindow` — the measured-load evidence the engagement fold consumes.
- Law: the window keys on the S0 `MachineInstanceKey` the fleet registry, `PlannedStep.Instance`, and the schedule all key on. Telemetry answers about a physical STATION, so a bare identifier here forks the one key space every consumer joins on.
- Cases: spindle-load · spindle-speed · path-feed · temperature · execution · alarm · tool-in-spindle — one case per decoded stream the shop consumers read; a stream without a consumer stays wire-only by omission, never a speculative case.
- Entry: `MachineObservation.Admit(MachineObservationIngress source)` — the one package admission: unavailable and unsupported rows project `None`, admitted scalar rows validate their units and ranges, and malformed values fail typed — no provider type or sentinel survives the seam.
- Auto: `MachineObservations` orders rows at admission and derives `Span`, `LatestLoad`, `MeanLoad`, `ActiveFraction`, `FaultEpisodes`, `ToolNumber`, and `LoadCeiling` as pure folds over the one row sequence — a consumer re-scanning rows for a derivable projection is the deleted form.
- Law: every scalar predicate is a `ValidityClaim` row and this page owns only the refusal SHAPE — one `Held` guard turns a claim into the typed `PolicyInadmissible` naming the decoded stream, where the kernel guard raises its own scalar-range fault. Hand `Finite`/`Nonnegative`/`Fraction` rows also spelled finiteness as `double.IsFinite`, admitting the host unset sentinel the kernel arm screens; `Text` survives because it PRODUCES a trimmed value no claim row answers over.
- Receipt: `LoadWindow` carries observed fraction, target fraction, reference radial depth, observation instant, and expiry; it declares `IValidityEvidence` over `ValidityClaim.All`, so its coherence is a fold the acceptance oracle reads and `Ceiling` states only what a fold cannot — whether the read instant falls inside the sampled span. `Kinematics/fleet.md` `MachinePerformance.Of` folds the same window into refreshed OEE, reliability, and observed-power rows under `FleetPolicy.PerformanceHorizon`, and `Tooling/wear.md` `ConditionSignal.Of` lowers the thermal case into the wear channel family.
- Packages: `Rasm.Domain` owns `ValidityClaim` and `IValidityEvidence`; NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new decoded stream is one union case, one dispatch row in `Admit`, and one window projection where a consumer folds it; a new consumer rebinds onto the window with zero decode edits.
- Boundary: AppHost owns provider timestamp and enum conversion into `MachineObservationIngress`; consumers hold only admitted union cases. Condition severity keeps the normal edge because a fault episode closes on it — dropping normals leaves every episode open-ended.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using Rasm.Fabrication.Process;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Kinematics;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ExecutionState {
    public static readonly ExecutionState Ready = new("ready", producing: false);
    public static readonly ExecutionState Active = new("active", producing: true);
    public static readonly ExecutionState Interrupted = new("interrupted", producing: false);
    public static readonly ExecutionState FeedHold = new("feed-hold", producing: false);
    public static readonly ExecutionState Stopped = new("stopped", producing: false);
    public static readonly ExecutionState OptionalStop = new("optional-stop", producing: false);
    public static readonly ExecutionState ProgramStopped = new("program-stopped", producing: false);
    public static readonly ExecutionState ProgramCompleted = new("program-completed", producing: false);
    public static readonly ExecutionState Wait = new("wait", producing: false);
    public static readonly ExecutionState ProgramOptionalStop = new("program-optional-stop", producing: false);

    public bool Producing { get; }

}

[SmartEnum<string>]
public sealed partial class ConditionSeverity {
    public static readonly ConditionSeverity Normal = new("normal", faulted: false);
    public static readonly ConditionSeverity Warning = new("warning", faulted: false);
    public static readonly ConditionSeverity Fault = new("fault", faulted: true);

    public bool Faulted { get; }

}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MachineObservationIngress {
    private MachineObservationIngress() { }

    public sealed record SpindleLoad(Instant At, double Fraction) : MachineObservationIngress;
    public sealed record SpindleSpeed(Instant At, double Rpm) : MachineObservationIngress;
    public sealed record PathFeed(Instant At, double MillimetersPerSecond) : MachineObservationIngress;
    public sealed record Temperature(Instant At, double Celsius, string Locus) : MachineObservationIngress;
    public sealed record Execution(Instant At, ExecutionState State) : MachineObservationIngress;
    public sealed record Alarm(Instant At, ConditionSeverity Severity, string ConditionId, string NativeCode) : MachineObservationIngress;
    public sealed record ToolInSpindle(Instant At, string ToolNumber) : MachineObservationIngress;
    public sealed record Unavailable : MachineObservationIngress;
    public sealed record Unsupported : MachineObservationIngress;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MachineObservation {
    private MachineObservation(Instant at) => At = at;

    public Instant At { get; }

    public sealed record SpindleLoad(Instant At, double Fraction) : MachineObservation(At);
    public sealed record SpindleSpeed(Instant At, double Rpm) : MachineObservation(At);
    public sealed record PathFeed(Instant At, double MillimetersPerSecond) : MachineObservation(At);
    public sealed record Temperature(Instant At, double Celsius, string Locus) : MachineObservation(At);
    public sealed record Execution(Instant At, ExecutionState State) : MachineObservation(At);
    public sealed record Alarm(Instant At, ConditionSeverity Severity, string ConditionId, string NativeCode) : MachineObservation(At);
    public sealed record ToolInSpindle(Instant At, string ToolNumber) : MachineObservation(At);

    public static Fin<Option<MachineObservation>> Admit(MachineObservationIngress source) =>
        Optional(source).ToFin(new KernelFault.InvalidValue("observation", "observation:null"))
            .Bind(row => row.Switch(
                spindleLoad: static value => Held(ValidityClaim.UnitInterval(value.Fraction), value.Fraction, "observation:load")
                    .Map(fraction => Some((MachineObservation)new SpindleLoad(value.At, fraction))),
                spindleSpeed: static value => Held(ValidityClaim.Nonnegative(value.Rpm), value.Rpm, "observation:rotary-velocity")
                    .Map(rpm => Some((MachineObservation)new SpindleSpeed(value.At, rpm))),
                pathFeed: static value => Held(
                        ValidityClaim.Nonnegative(value.MillimetersPerSecond), value.MillimetersPerSecond, "observation:path-feedrate")
                    .Map(feed => Some((MachineObservation)new PathFeed(value.At, feed))),
                temperature: static value => Held(ValidityClaim.Finite(value.Celsius), value.Celsius, "observation:temperature")
                    .Bind(celsius => Text(value.Locus, "observation:temperature-locus")
                        .Map(locus => Some((MachineObservation)new Temperature(value.At, celsius, locus)))),
                execution: static value => Optional(value.State)
                    .ToFin(new KernelFault.InvalidValue("observation", "observation:execution"))
                    .Map(state => Some((MachineObservation)new Execution(value.At, state))),
                alarm: static value => Optional(value.Severity)
                    .ToFin(new KernelFault.InvalidValue("observation", "observation:alarm-severity"))
                    .Bind(severity => Text(value.ConditionId, "observation:alarm-condition")
                        .Map(condition => Some((MachineObservation)new Alarm(value.At, severity, condition, value.NativeCode?.Trim() ?? string.Empty)))),
                toolInSpindle: static value => Text(value.ToolNumber, "observation:tool-number")
                    .Map(tool => Some((MachineObservation)new ToolInSpindle(value.At, tool))),
                unavailable: static _ => Fin.Succ(Option<MachineObservation>.None),
                unsupported: static _ => Fin.Succ(Option<MachineObservation>.None)));

    private static Fin<T> Held<T>(ValidityClaim claim, T value, string locus) =>
        claim
            ? Fin.Succ(value)
            : Fin.Fail<T>(new KernelFault.InvalidValue("observation", locus));

    private static Fin<string> Text(string value, string locus) =>
        Optional(value).Map(static row => row.Trim()).Filter(static row => row.Length > 0)
            .ToFin(new KernelFault.InvalidValue("observation", locus));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct LoadWindow(
    double ObservedFraction,
    double TargetFraction,
    double ReferenceRadialMm,
    Instant ObservedAt,
    Instant ValidUntil) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(ObservedFraction),
        ValidityClaim.Positive(TargetFraction),
        ValidityClaim.Positive(ReferenceRadialMm),
        ObservedAt <= ValidUntil);

    public Option<double> Ceiling(Instant at) =>
        IsValid && ObservedAt <= at && at <= ValidUntil
            ? Some(ReferenceRadialMm * TargetFraction / ObservedFraction)
            : None;
}

[ComplexValueObject]
public sealed partial class MachineObservations {
    public MachineInstanceKey Instance { get; }
    public Seq<MachineObservation> Rows { get; }
    public Instant WindowEnd { get; }

    public NodaTime.Interval Span => new(Rows.Head.Map(static row => row.At).IfNone(Instant.MinValue),
        WindowEnd);

    private Option<SpindleLoad> Latest =>
        Rows.Rev().Choose(static row => row is SpindleLoad load ? Some(load) : Option<SpindleLoad>.None).Head;

    public Option<double> LatestLoad => Latest.Map(static load => load.Fraction);

    public Option<double> MeanLoad =>
        Rows.Choose(static row => row is SpindleLoad load ? Some(load.Fraction) : Option<double>.None) is { IsEmpty: false } loads
            ? Some(loads.Fold(0.0, static (sum, value) => sum + value) / loads.Count)
            : None;

    public Option<string> ToolNumber => Rows.Rev()
        .Choose(static row => row is ToolInSpindle tool ? Some(tool.ToolNumber) : Option<string>.None).Head;

    public double ActiveFraction =>
        Rows.Choose(static row => row is Execution execution ? Some(execution) : Option<Execution>.None) is { IsEmpty: false } edges
            && (Span.End - Span.Start).TotalSeconds is > 0.0 and var total
            ? edges.Zip(edges.Skip(1).Map(static edge => edge.At).Add(Span.End),
                static (edge, until) => edge.State.Producing ? (until - edge.At).TotalSeconds : 0.0)
                .Fold(0.0, static (sum, value) => sum + value) / total
            : 0.0;

    public Seq<NodaTime.Interval> FaultEpisodes {
        get {
            (HashMap<string, Instant> Open, Seq<NodaTime.Interval> Closed) state = Rows
                .Choose(static row => row is Alarm alarm ? Some(alarm) : Option<Alarm>.None)
                .Fold((Open: HashMap<string, Instant>(), Closed: Seq<NodaTime.Interval>()), (held, alarm) =>
                    alarm.Severity.Faulted
                        ? (held.Open.TryAdd(alarm.ConditionId, alarm.At), held.Closed)
                        : held.Open.Find(alarm.ConditionId).Match(
                            Some: opened => (held.Open.Remove(alarm.ConditionId), held.Closed.Add(new NodaTime.Interval(opened, alarm.At))),
                            None: () => held));
            return state.Closed + state.Open.Values.ToSeq().Map(opened => new NodaTime.Interval(opened, Span.End));
        }
    }

    public Option<LoadWindow> LoadCeiling(
        Instant at,
        Duration freshness,
        double targetFraction,
        double referenceRadialMm) =>
        ValidityClaim.All(
            freshness > Duration.Zero,
            ValidityClaim.Positive(targetFraction),
            ValidityClaim.Positive(referenceRadialMm))
            ? Latest
                .Filter(load => load.At <= at && at - load.At <= freshness)
                .Map(load => new LoadWindow(load.Fraction, targetFraction, referenceRadialMm, load.At, load.At + freshness))
                .Filter(window => window.Ceiling(at).IsSome)
            : None;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref MachineInstanceKey instance,
        ref Seq<MachineObservation> rows,
        ref Instant windowEnd) {
        rows = toSeq(rows.OrderBy(static row => row.At));
        if (rows.IsEmpty || !rows.Last.Exists(row => windowEnd >= row.At))
            validationError = new ValidationError("machine-observations:window");
    }

    public static Fin<MachineObservations> Admit(
        MachineInstanceKey instance, Seq<MachineObservation> rows, Instant windowEnd) =>
        Validate(instance, rows, windowEnd, out MachineObservations window).Admitted(window);
}
```

## [03]-[RESEARCH]

(none)
