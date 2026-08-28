# [RASM_FABRICATION_TELEMETRY]

`Process/telemetry` is the package's observation seat: the `rasm.fabrication.*` instrument roster, the site-write entries every producing lane measures through, the solver span bracket, the classification rows, the hook set every spine fact fires on, and the board pack. Results are the truth — a lane writes its own instrument from its typed result at the site where that result settles, brackets its solve in its engine span, and fires spine facts on the kernel hook set, so no fact union, projection fan, kind roster, or emission port stands between a result and the instrument that observes it.

Composition draws every mechanism from the kernel signal capsule — `InstrumentSet` mounted over this roster, `SpanBand` admitting the engine scopes, `HookRegistry.Mount` freezing the points, the SLO algebra over the pack — and holds no OpenTelemetry reference, exporter, or provider. Instrument names run dotted `rasm.fabrication.<domain>.<measure>` with UCUM units under the `TelemetrySource.Fabrication` scope the composing app admits by name, and `InstrumentSet.Tags` folds the kernel `rasm.tenant` partition into every write. Durable facts leave as CloudEvents the app root publishes from its observe subscription over `FabricationHookFact`, never from an emit inside a fold.

## [01]-[INDEX]

- [02]-[INSTRUMENT_ROSTER]: `SustainabilityQuantity`, the `rasm.fabrication.*` `InstrumentSpec` rows with their slot and verdict consts, and the contributor port.
- [03]-[OBSERVE]: `FabricationEngine` and `EnginePhase` solver vocabularies, `FabricationTrace` scopes with the `Traced` bracket and `Mark`, and the `Write`, `Level`, and `Steps` site entries over the mounted set.
- [04]-[CLASSIFICATION]: Suite-taxonomy attribute rows for the classified members.
- [05]-[HOOKS]: `FabricationPoint` closes the `rasm.fabrication.<domain>.<point>` vocabulary, `FabricationHookFact` closes the spine payloads over it, and `FabricationHooks` mints the one kernel hook set.
- [06]-[BOARD_PACK]: `FabricationDescriptors` binds the kernel pack over that roster.

## [02]-[INSTRUMENT_ROSTER]

- Owner: `FabricationInstruments` — the Fabrication `InstrumentSpec` roster and the `TelemetryContributorPort` mint; `SustainabilityQuantity` — the UCUM-unit axis every sealed passport measure resolves its instrument through. The roster is composition-free data, so one declaration binds against any meter and any cells.
- Entry: `FabricationInstruments.Telemetry(string version)` — the one contributor port (scope `TelemetrySource.Fabrication`) carrying the domain row set, the `[03]` trace planes, the `[04]` classifications, and the `[06]` board pack into the composing root; the mint stamps the kernel schema coordinate as `MeterOptions.TelemetrySchemaUrl`, so every `rasm.fabrication.*` scope reads with pinned semantics and the shop's board proves against the roster the root just bound.
- Auto: every advised histogram row ships its named kernel `Buckets` boundaries at creation, the fallback a backend without exponential histograms reads — the default aggregation stays base2 exponential at the provider and no bound array is spelled here; registry mount de-duplicates by name, so a duplicate row is a composition fault, never a forked stream; every share indicator partitions ONE population on the outcome dimension its own row declares, so a good half is a tag value the producing write already stamps and no roster row carries a numerator; every outcome axis reads its values off this owner's consts, so a producing site and a board row never spell one value twice; the sustainability family generates from `SustainabilityQuantity`, so the roster carries one row per UCUM unit rather than one per measure, and the passport seal and the board panels resolve off that one axis.
- Packages: Rasm, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: one measured concern is one `InstrumentSpec` row here and one `Write` at the site where its result settles; a per-kind family derives from its owning vocabulary, never hand-enumerated rows; a new level is one `Level` write at its producing site beside one `Level` row, or one keyed write beside one `Levels` row where the shop holds a reading per basis, process, or machine; a measure in a unit no row carries is one `SustainabilityQuantity` row that mints its roster entry and its board panel together.
- Boundary: instrument names are dotted `rasm.fabrication.<domain>.<measure>` with UCUM units, never pre-baked `_total` or unit suffixes; the port's `Scope` is the version-stamped package id the composing root admits by name; event-shaped facts ride counters and histograms while level-shaped measures ride pulled rows reading the composition's cells at collection cadence; every dimension key is a declared slot const on its own row's `Dimensions` column, so the governance leg derives view tag keys from the mounted roster and no second roster restates them; tenancy is the kernel `TenantContext` projection every job row declares, so this page holds no tenant key, no baggage read, and no zero sentinel; a scalar pulled level carries no call-site tag, because a tag whose value flips between collections strands the previous value's series live forever — a level holding one reading per basis or process is a keyed `Levels` family whose tag IS its cell key.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Compliance.Classification;
using NodaTime;
using Rasm.Domain;
using Rasm.Fabrication.Spec;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Process;

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SustainabilityQuantity {
    public static readonly SustainabilityQuantity Energy = new("energy", unit: "J");
    public static readonly SustainabilityQuantity Mass = new("mass", unit: "kg");
    public static readonly SustainabilityQuantity Fraction = new("fraction", unit: "1");
    public static readonly SustainabilityQuantity Volume = new("volume", unit: "L");
    public static readonly SustainabilityQuantity Lifetime = new("lifetime", unit: "s");

    public string Unit { get; }

    private static readonly Lazy<FrozenDictionary<SustainabilityQuantity, InstrumentSpec>> Rows = new(
        static () => Items.ToFrozenDictionary(static row => row, static row => InstrumentSpec.Create(
            $"rasm.fabrication.sustainability.{row.Key}", InstrumentKind.Distribution, MeasureForm.Real, row.Unit,
            "sealed passport sustainability evidence by measure", Seq(TenantContext.TenantSlot, FabricationInstruments.MeasureSlot),
            None, None, None)),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public InstrumentSpec Instrument => Rows.Value[this];
}

public static partial class FabricationInstruments {
    public const string BasisSlot = "rasm.fabrication.basis";
    public const string ActionSlot = "rasm.fabrication.action";
    public const string ModelSlot = "rasm.fabrication.model";
    public const string VerdictSlot = "rasm.fabrication.verdict";
    public const string MetricSlot = "rasm.fabrication.metric";
    public const string ResidueSlot = "rasm.fabrication.residue";
    public const string MeasureSlot = "rasm.fabrication.measure";
    public const string ScopeSlot = "rasm.fabrication.scope";
    public const string CurrencySlot = "rasm.fabrication.currency";
    public const string BackedSlot = "rasm.fabrication.backed";
    public const string EvidenceSlot = "rasm.fabrication.evidence";
    public const string ProcessSlot = "rasm.fabrication.process";
    public const string KindSlot = "rasm.fabrication.egress.kind";
    public const string VerificationSlot = "rasm.fabrication.verification";
    public const string SolverSlot = "rasm.fabrication.solver";
    public const string PhaseSlot = "rasm.fabrication.phase";
    public const string ControllerSlot = "rasm.fabrication.controller";

    public const string Pass = "pass";
    public const string Fail = "fail";
    public const string Verified = "verified";
    public const string Unverified = "unverified";
    public const string Measured = "measured";
    public const string Declared = "declared";
    public const string Simulation = "simulation";
    public const string Fallback = "fallback";
    public const string Taylor = "taylor";
    public const string Uncut = "uncut";
    public const string Overcut = "overcut";
    public const string Unit = "unit";
    public const string Lot = "lot";

    public static readonly InstrumentSpec ToolAssessments = InstrumentSpec.Create("rasm.fabrication.tool.assessments", InstrumentKind.Count, MeasureForm.Whole, "{assessment}",
        "wear assessments settled at a critical state by basis and maintenance disposition", Seq(TenantContext.TenantSlot, BasisSlot, ActionSlot), None, None, None);
    public static readonly InstrumentSpec ToolWear = InstrumentSpec.Create("rasm.fabrication.tool.wear", InstrumentKind.Distribution, MeasureForm.Real, "1",
        "remaining-life fraction at the critical wear state", Seq(TenantContext.TenantSlot, BasisSlot, ActionSlot), Some(Buckets.Fractions), None, None);
    public static readonly InstrumentSpec ToolRefreshAge = InstrumentSpec.Create("rasm.fabrication.tool.refresh.age", InstrumentKind.Distribution, MeasureForm.Real, "s",
        "interval between successive telemetry catalog refreshes", Seq<string>(), Some(Buckets.RefreshSeconds), None, None);
    public static readonly InstrumentSpec FitResidual = InstrumentSpec.Create("rasm.fabrication.fit.residual", InstrumentKind.Distribution, MeasureForm.Real, "1",
        "RMS residual of the wear and machinability power-law fits", Seq(TenantContext.TenantSlot, ModelSlot), None, None, None);
    public static readonly InstrumentSpec FitQuality = InstrumentSpec.Create("rasm.fabrication.fit.quality", InstrumentKind.Distribution, MeasureForm.Real, "1",
        "coefficient of determination of the machinability fit", Seq(TenantContext.TenantSlot, ModelSlot), None, None, None);
    public static readonly InstrumentSpec ProbeFeatures = InstrumentSpec.Create("rasm.fabrication.probe.features", InstrumentKind.Count, MeasureForm.Whole, "{feature}",
        "inspected features by conformance verdict", Seq(TenantContext.TenantSlot, VerdictSlot), None, None, None);
    public static readonly InstrumentSpec ProbeDeviation = InstrumentSpec.Create("rasm.fabrication.probe.deviation", InstrumentKind.Distribution, MeasureForm.Real, "mm",
        "worst absolute measured deviation per inspection", Seq(TenantContext.TenantSlot), Some(Buckets.Millimeters), None, None);
    public static readonly InstrumentSpec CapabilityStudies = InstrumentSpec.Create("rasm.fabrication.capability.studies", InstrumentKind.Count, MeasureForm.Whole, "{study}",
        "capability studies settled by SPC conformance verdict", Seq(TenantContext.TenantSlot, VerdictSlot), None, None, None);
    public static readonly InstrumentSpec CapabilityIndex = InstrumentSpec.Create("rasm.fabrication.capability.index", InstrumentKind.Distribution, MeasureForm.Real, "1",
        "capability and performance index values by metric row", Seq(TenantContext.TenantSlot, MetricSlot), None, None, None);
    public static readonly InstrumentSpec CapabilityViolations = InstrumentSpec.Create("rasm.fabrication.capability.violations", InstrumentKind.Count, MeasureForm.Whole, "{violation}",
        "SPC rule violations per study", Seq(TenantContext.TenantSlot), None, None, None);
    public static readonly InstrumentSpec RemovalVerifications = InstrumentSpec.Create("rasm.fabrication.removal.verifications", InstrumentKind.Count, MeasureForm.Whole, "{verification}",
        "material-removal verifications settled by gouge-finding verdict", Seq(TenantContext.TenantSlot, VerdictSlot), None, None, None);
    public static readonly InstrumentSpec RemovalDefects = InstrumentSpec.Create("rasm.fabrication.removal.defects", InstrumentKind.Count, MeasureForm.Whole, "{finding}",
        "gouge findings per material-removal verification", Seq(TenantContext.TenantSlot), None, None, None);
    public static readonly InstrumentSpec RemovalResidual = InstrumentSpec.Create("rasm.fabrication.removal.residual", InstrumentKind.Distribution, MeasureForm.Real, "mm3",
        "uncut and overcut voxel volume per verification", Seq(TenantContext.TenantSlot, ResidueSlot), None, None, None);
    public static readonly InstrumentSpec RemovalAirCut = InstrumentSpec.Create("rasm.fabrication.removal.aircut", InstrumentKind.Distribution, MeasureForm.Real, "1",
        "air-cut fraction of swept program motion per verification", Seq(TenantContext.TenantSlot), Some(Buckets.Fractions), None, None);
    public static readonly InstrumentSpec CycleDuration = InstrumentSpec.Create("rasm.fabrication.cycle.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
        "simulated modal cycle time per program", Seq(TenantContext.TenantSlot), Some(Buckets.CycleSeconds), None, None);
    public static readonly InstrumentSpec CycleEnergy = InstrumentSpec.Create("rasm.fabrication.cycle.energy", InstrumentKind.Distribution, MeasureForm.Real, "kW.h",
        "simulated machine energy per program", Seq(TenantContext.TenantSlot), None, None, None);
    public static readonly InstrumentSpec CycleDistance = InstrumentSpec.Create("rasm.fabrication.cycle.distance", InstrumentKind.Distribution, MeasureForm.Real, "mm",
        "simulated cutting-motion path length per program", Seq(TenantContext.TenantSlot), None, None, None);
    public static readonly InstrumentSpec EstimateMoney = InstrumentSpec.Create("rasm.fabrication.estimate.money", InstrumentKind.Distribution, MeasureForm.Real, "{money}",
        "signed money ledger total in the estimate currency", Seq(TenantContext.TenantSlot, ScopeSlot, CurrencySlot), None, None, None);
    public static readonly InstrumentSpec EstimateCarbon = InstrumentSpec.Create("rasm.fabrication.estimate.carbon", InstrumentKind.Distribution, MeasureForm.Real, "kg",
        "carbon ledger total as kilograms CO2-equivalent", Seq(TenantContext.TenantSlot, ScopeSlot), None, None, None);
    public static readonly InstrumentSpec EstimateClock = InstrumentSpec.Create("rasm.fabrication.estimate.clock", InstrumentKind.Distribution, MeasureForm.Real, "s",
        "estimated machine clock per subject", Seq(TenantContext.TenantSlot, BackedSlot), Some(Buckets.CycleSeconds), None, None);
    public static readonly InstrumentSpec FleetMatches = InstrumentSpec.Create("rasm.fabrication.fleet.matches", InstrumentKind.Count, MeasureForm.Whole, "{match}",
        "machine matches assessed by process and ranking evidence", Seq(TenantContext.TenantSlot, ProcessSlot, EvidenceSlot), None, None, None);
    public static readonly InstrumentSpec FleetUtilization = InstrumentSpec.Create("rasm.fabrication.fleet.utilization", InstrumentKind.Distribution, MeasureForm.Real, "1",
        "machine load factor at match assessment", Seq(TenantContext.TenantSlot, ProcessSlot), Some(Buckets.Fractions), None, None);
    public static readonly InstrumentSpec FleetEffectiveness = InstrumentSpec.Create("rasm.fabrication.fleet.effectiveness", InstrumentKind.Distribution, MeasureForm.Real, "1",
        "machine effectiveness fraction at match assessment", Seq(TenantContext.TenantSlot, ProcessSlot), Some(Buckets.Fractions), None, None);
    public static readonly InstrumentSpec FleetLoad = InstrumentSpec.Create("rasm.fabrication.fleet.load", InstrumentKind.Levels, MeasureForm.Real, "1",
        "latest machine load factor at match assessment by process", Seq<string>(), None, Some(ProcessSlot), None);
    public static readonly InstrumentSpec ToolFloor = InstrumentSpec.Create("rasm.fabrication.tool.floor", InstrumentKind.Levels, MeasureForm.Real, "1",
        "latest remaining-life fraction at the critical wear state by basis", Seq<string>(), None, Some(BasisSlot), None);
    public static readonly InstrumentSpec RunDuration = InstrumentSpec.Create("rasm.fabrication.run.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
        "fabrication run wall duration", Seq(TenantContext.TenantSlot, ProcessSlot, VerificationSlot), Some(Buckets.CycleSeconds), None, None);
    public static readonly InstrumentSpec RunArtifacts = InstrumentSpec.Create("rasm.fabrication.run.artifacts", InstrumentKind.Count, MeasureForm.Whole, "{artifact}",
        "content-keyed artifacts produced by egress kind", Seq(TenantContext.TenantSlot, KindSlot), None, None, None);
    public static readonly InstrumentSpec RunWarnings = InstrumentSpec.Create("rasm.fabrication.run.warnings", InstrumentKind.Count, MeasureForm.Whole, "{warning}",
        "run warnings accumulated on the sealed run", Seq(TenantContext.TenantSlot), None, None, None);
    public static readonly InstrumentSpec TravelerAmendments = InstrumentSpec.Create("rasm.fabrication.traveler.amendments", InstrumentKind.Distribution, MeasureForm.Whole, "{amendment}",
        "as-run amendment chain length at traveler seal", Seq(TenantContext.TenantSlot), None, None, None);
    public static readonly InstrumentSpec DeliveryPrograms = InstrumentSpec.Create("rasm.fabrication.delivery.programs", InstrumentKind.Count, MeasureForm.Whole, "{program}",
        "posted programs delivered to controllers by custody verdict", Seq(TenantContext.TenantSlot, KindSlot, VerdictSlot, ControllerSlot), None, None, None);
    public static readonly InstrumentSpec EngineSteps = InstrumentSpec.Create("rasm.fabrication.engine.steps", InstrumentKind.Count, MeasureForm.Whole, "{step}",
        "solver-internal step counts by solver and phase", Seq(TenantContext.TenantSlot, SolverSlot, PhaseSlot), None, None, None);

    public static readonly Seq<InstrumentSpec> Rows = Seq(
        ToolAssessments, ToolWear, ToolRefreshAge, FitResidual, FitQuality, ProbeFeatures, ProbeDeviation,
        CapabilityStudies, CapabilityIndex, CapabilityViolations, RemovalVerifications, RemovalDefects, RemovalResidual, RemovalAirCut,
        CycleDuration, CycleEnergy, CycleDistance, EstimateMoney, EstimateCarbon, EstimateClock,
        FleetMatches, FleetUtilization, FleetEffectiveness, FleetLoad, ToolFloor,
        RunDuration, RunArtifacts, RunWarnings, TravelerAmendments, DeliveryPrograms, EngineSteps)
        + toSeq(SustainabilityQuantity.Items).Map(static row => row.Instrument);

    public static TelemetryContributorPort Telemetry(string version) =>
        new(Scope: TelemetrySource.Fabrication, Version: version, Instruments: Rows,
            Planes: toSeq(FabricationTrace.Scopes), Classifications: FabricationClassified.Values,
            Rosters: [FeatureControlWire.Proof],
            Board: Some(FabricationDescriptors.Pack));
}
```

## [03]-[OBSERVE]

- Owner: `FabricationEngine` and `EnginePhase` — the solver-lane and lane-point vocabularies every span scope, span mark, and step count keys on; `FabricationTrace` — the scope roster the composing root admits into the kernel `SpanBand`, the engine-keyed bracket every solver lane opens, and the lane-milestone mark; `FabricationInstruments`'s site entries — `Write`, `Level`, and `Steps` over the mounted set a lane receives.
- Entry: a lane takes `Option<InstrumentSet> set = default, Option<SpanBand> band = default` as trailing columns, or reads `runtime.Instruments`/`runtime.Band` when it enters through `Fabrication.Run`; it measures as `set.Write(FabricationInstruments.<Row>, measurement, (<Slot>, value), …)`, holds a level as `set.Level(<Row>, value)`, counts solver steps as `set.Steps((EnginePhase.<Row>, count), …)`, brackets its fold as `band.Traced(FabricationEngine.<Row>, span => …)`, and marks a milestone as `FabricationTrace.Mark(span, EnginePhase.<Row>)` — every entry one kernel call on the `Fin` result.
- Law: the write sits INSIDE the producing fold on the settled typed result — one site per measured concern, its dimensions read off the result's own smart-enum keys and the roster consts — so a measurement can neither precede the fact it observes nor survive a refusal of it; `None` on either column writes and traces nothing, so a headless caller passes nothing and branches nowhere, while an unmounted row under a present set refuses on the error channel rather than vanishing.
- Auto: `Write` folds the ambient `TenantContext` partition through `InstrumentSet.Tags`, so a partitioned shop attributes every row and a single-tenant one mints no dimension, and no lane spells a tenant key; `Steps` derives the solver dimension from the phase row, so the two vocabularies cannot drift and a hand-spelled solver string has no construction path; the scope key derives from the `FabricationEngine` row, so the span source name and the `SolverSlot` value resolve to one vocabulary; the kernel gates every bracket on `HasListeners`, so an unlistened solve pays one null test and a mark on its null span one more.
- Packages: Rasm, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox (`System.Diagnostics`).
- Growth: a new traced lane is one `FabricationEngine` row; a new milestone or counted step is one `EnginePhase` row and its `Mark` or `Steps` site.
- Boundary: the band and the set are composition-entered and reach a lane through the run spine or its own trailing columns, never a process-static source a domain page reads ambiently; a span carries no counter — a metric read off a span is the sampling-dependent duplicate the roster row already owns — and the meter scope stays `TelemetrySource.Fabrication`, a `TraceScope` and a meter scope being distinct grammars neither derives from; an unadmitted scope refuses on the kernel result, so a composition that omits this roster fails at the first bracket rather than silently dropping every solver span; trace-based exemplars join the `[02]` histograms to these spans at the provider.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FabricationEngine {
    public static readonly FabricationEngine Nest = new("nest");
    public static readonly FabricationEngine Skeleton = new("skeleton");
    public static readonly FabricationEngine Setup = new("setup");
    public static readonly FabricationEngine Scan = new("scan");
    public static readonly FabricationEngine Probe = new("probe");
    public static readonly FabricationEngine Form = new("form");
    public static readonly FabricationEngine Simulate = new("simulate");

    private static readonly Lazy<FrozenDictionary<FabricationEngine, TraceScope>> Scopes = new(
        static () => Items.ToFrozenDictionary(static row => row, static row => TraceScope.Create(value: $"rasm.fabrication.{row.Key}")),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public TraceScope Trace => Scopes.Value[this];
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EnginePhase {
    public static readonly EnginePhase Candidates = new("candidates", FabricationEngine.Nest);
    public static readonly EnginePhase Evaluated = new("evaluated", FabricationEngine.Nest);
    public static readonly EnginePhase CandidatesRejected = new("candidates-rejected", FabricationEngine.Nest);
    public static readonly EnginePhase MemoHits = new("memo-hits", FabricationEngine.Nest);
    public static readonly EnginePhase MemoMisses = new("memo-misses", FabricationEngine.Nest);
    public static readonly EnginePhase Moulds = new("moulds", FabricationEngine.Nest);
    public static readonly EnginePhase ChiralFloor = new("chiral-floor", FabricationEngine.Nest);
    public static readonly EnginePhase Nodes = new("nodes", FabricationEngine.Skeleton);
    public static readonly EnginePhase Arcs = new("arcs", FabricationEngine.Skeleton);
    public static readonly EnginePhase Passes = new("passes", FabricationEngine.Skeleton);
    public static readonly EnginePhase Decisions = new("decisions", FabricationEngine.Setup);
    public static readonly EnginePhase Exposures = new("exposures", FabricationEngine.Scan);
    public static readonly EnginePhase Jumps = new("jumps", FabricationEngine.Scan);
    public static readonly EnginePhase Remelts = new("remelts", FabricationEngine.Scan);
    public static readonly EnginePhase Stitches = new("stitches", FabricationEngine.Scan);
    public static readonly EnginePhase IcpIterations = new("icp-iterations", FabricationEngine.Probe);
    public static readonly EnginePhase DatumRegistered = new("datum-registered", FabricationEngine.Probe);
    public static readonly EnginePhase FeaturesFitted = new("features-fitted", FabricationEngine.Probe);
    public static readonly EnginePhase Expansions = new("expansions", FabricationEngine.Form);
    public static readonly EnginePhase ExpansionsRejected = new("expansions-rejected", FabricationEngine.Form);

    public FabricationEngine Solver { get; }
}

public static class FabricationTrace {
    public static readonly ImmutableArray<TraceScope> Scopes =
        [.. FabricationEngine.Items.Select(static row => row.Trace)];

    extension(Option<SpanBand> band) {
        public Fin<T> Traced<T>(FabricationEngine engine, Func<Activity?, Fin<T>> body) =>
            band.Match(Some: admitted => admitted.Traced(engine.Trace, body), None: () => body(null));

        public IO<T> Traced<T>(FabricationEngine engine, Func<Activity?, IO<T>> body) =>
            band.Match(Some: admitted => admitted.Traced(engine.Trace, body), None: () => body(null));
    }

    public static Unit Mark(Activity? span, EnginePhase phase) =>
        ignore(span?.AddEvent(new ActivityEvent(phase.Key)));
}

public static partial class FabricationInstruments {
    extension(Option<InstrumentSet> set) {
        public Fin<Unit> Write(InstrumentSpec row, double measurement, params ReadOnlySpan<(string Slot, object? Value)> facts) {
            TagList tags = InstrumentSet.Tags(TenantContext.Current, facts);
            return set.Match(Some: mounted => mounted.Write(row, measurement, in tags), None: static () => Fin.Succ(unit));
        }

        public Fin<Unit> Level(InstrumentSpec row, double value, Option<string> key = default) =>
            set.Match(Some: mounted => mounted.Level(row, value, key), None: static () => Fin.Succ(unit));

        public Fin<Unit> Steps(params ReadOnlySpan<(EnginePhase Phase, long Count)> counts) =>
            toSeq(counts.ToArray())
                .TraverseM(row => set.Write(EngineSteps, row.Count, (SolverSlot, row.Phase.Solver.Key), (PhaseSlot, row.Phase.Key)))
                .As()
                .Map(static _ => unit);
    }
}
```

## [04]-[CLASSIFICATION]

- Owner: `FabricationClassified` — the sealed attribute rows binding this folder's classified members to the suite taxonomy by value, and the `Values` roster contributing those same texts upward on the `[02]-[INSTRUMENT_ROSTER]` port.
- Cases: personal · confidential · credential.
- Auto: an annotated member redacts wherever a log or export boundary expands it — HMAC for personnel and heat identity so cross-record correlation survives, erase for credential material — and sealed artifact bytes never redact: canonical documents are domain truth, classification governs telemetry egress alone.
- Packages: Microsoft.Extensions.Compliance.Redaction, Rasm (`ClassifiedValue`), LanguageExt.Core, BCL inbox.
- Growth: a newly classified member family is one attribute row binding an existing taxonomy key plus its `ClassifiedValue` row on `Values`, both off one const; a new sensitivity class is a suite-taxonomy decision, never a folder mint.
- Boundary: taxonomy name and row keys are value federation to the suite `DataClassification` vocabulary — the attribute rows carry `(taxonomy, value)` string pairs and no type reference crosses the package boundary, and `Values` STRENGTHENS that law rather than qualifying it: the contribution rides the existing `TelemetryContributorPort` interface as the identical text, so the suite's redaction owner proves this folder's values against its rostered set at boot and an unrostered value refuses at composition instead of reaching the erasing fallback at egress, where a deleted dimension raises nothing and is noticed only when someone misses it; annotated owners are `AttestationPayload.Signer` and `.Credential`, `HeatNumber`, `WelderQualification.Welder`, `TravelerAmendment.Actor`, and `ProgramDelivery.Operator`, each carrying its attribute at the declaring fence; `DataClassificationTypeConverter` string round-tripping stays under its `EXTEXP0002` gate as a declared policy value when a classification ever binds from configuration.

```csharp
public static class FabricationClassified {
    const string SuiteTaxonomy = "DataClassification";
    const string PersonalValue = "personal";
    const string ConfidentialValue = "confidential";
    const string CredentialValue = "credential";

    public static readonly DataClassification Personal = new(SuiteTaxonomy, PersonalValue);
    public static readonly DataClassification Confidential = new(SuiteTaxonomy, ConfidentialValue);
    public static readonly DataClassification Credential = new(SuiteTaxonomy, CredentialValue);

    public static readonly Seq<ClassifiedValue> Values = Seq(
        new ClassifiedValue(SuiteTaxonomy, PersonalValue),
        new ClassifiedValue(SuiteTaxonomy, ConfidentialValue),
        new ClassifiedValue(SuiteTaxonomy, CredentialValue));
}

public sealed class PersonalDataAttribute() : DataClassificationAttribute(FabricationClassified.Personal);

public sealed class ConfidentialDataAttribute() : DataClassificationAttribute(FabricationClassified.Confidential);

public sealed class CredentialDataAttribute() : DataClassificationAttribute(FabricationClassified.Credential);
```

## [05]-[HOOKS]

- Owner: `FabricationPoint` — the `[SmartEnum<string>]` point vocabulary keyed `rasm.fabrication.<domain>.<point>`, realizing the kernel `IHookRoster<FabricationPoint>` floor with a `CapabilitySet<HookModality>` column and a plane answer; `FabricationHookFact` — the closed spine-payload union realizing `IHookFact<FabricationPoint>`, its `At` column the primary case-to-row correspondence; `FabricationHooks` — the composition entry minting the ONE kernel `HookSet<FabricationPoint, FabricationHookFact, TelemetrySource>`. Seats, veto folding, bounded replay, fork-shielded isolation, detach custody, owner-scoped release, and the bounded `FaultCell` all ride that hook set, so this folder mints zero hook mechanism.
- Cases: `rasm.fabrication.run.admission` veto over `FabricationInput` · `rasm.fabrication.derive.stage` observe over `PlannedStep` · `rasm.fabrication.egress.mint` veto over `ContentKey` · `rasm.fabrication.verify.verdict` replay over `FabricationResult.VerificationResult` · `rasm.fabrication.delivery.handoff` observe over `RunEvidence`. Every row admits `HookModality.Observe` beside whatever else it holds, so a plugin watching admission or egress attaches on the same point its veto governs rather than demanding a shadow seat.
- Entry: `FabricationHooks.Live(gates, taps, cell)` mints the hook set once at composition against the evidence cell the composing app hands it; `hooks.Fire(fact.At, fact)` is the spine's raise and the guarded arity `hooks.Fire(fact.At, fact, body)` the one unwrap a veto site takes; `hooks.Points` is the census `HookRegistry.Mount` freezes at the app root beside the AppHost hook set and the app root's CloudEvent observe row; `hooks.Release(TelemetrySource.Fabrication)` drops this package's subscriptions alone.
- Auto: `At` is the primary correspondence and its generated total `Map` breaks at compile time on a case with no row, so `Seats` derives rather than mirrors and no caller names a point; ids derive from each row's own key through one `Items`-built index, so a seat re-spells neither id nor modality. `Run` fires admission before dispatch, egress mint per produced key, stage and verdict from the canonical domain value, and hand-off after evidence.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new point is one `FabricationPoint` row, one `FabricationHookFact` case with its `At` arm, and one fire site on the run spine; delivery semantics are the kernel modality rows.
- Law: `FabricationHookFact` hands a WHOLE domain aggregate in process to a subscriber that may refuse, replay, or publish it — the app root's observe subscription is the one CloudEvent emitter, projecting a fired fact as the event `data` under the solution `[HOOK_ORDER]` join, so no fold in this package mints an envelope and no flattened wire twin of a hook payload exists.
- Law: `EgressMint` is REFUSAL-ONLY. `ContentKey` addresses its own bytes, so a gate rewriting the admitted key forges an identity nothing produced — the same forgery `owner#RUN_DISPATCH` already fails a lineage cycle as — and the spine proves it at the site by refusing an admitted key differing from the one it fired. `Admission` carries no such law: rewriting the request is what an admission veto exists for, and its site threads the admitted input onward.
- Law: NAMED LOSS from composing the kernel hook set — the per-point FACT TYPE. Each named `HookPoint<TFact>` field refused every sibling payload at compile time; under one hook set every point shares `FabricationHookFact` and subscribers discriminate on the case. What survives is stronger: `At` fixes the case-to-row pairing at compile time and the kernel gates `Seats` TWICE per fire — at entry and on the veto fold's product — so per-point narrowing moved off five field declarations onto one generated map. WITNESS — the five `HookPoint<TFact>` columns, the five-line `Live`, the five-entry `Points` census, and the private `Seat<TFact>` mint all delete onto `FabricationHooks.Of`.
- Boundary: hook scope rides the `FabricationRuntime` instance, so two apps composing the library never share a mutable registry or shadow each other's subscribers; ids obey the four-segment `rasm.<pkg>.<domain>.<point>` grammar `HookId` admission enforces; a subscriber fault parks as `IsolatedFault` on the composition's own bounded cell and the emitter is untouched, the ring shedding oldest-first rather than growing for process lifetime; a veto refusal returns on the run's own result as the subscriber's typed fault. Spans are absent by design — admitted band scopes are the solver lanes at `[03]`, so `Plane` is `None` on every row, no `TraceScope` derives off these ids, and `Live` binds no `IHookSpan`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using FabricationGate = Rasm.Domain.HookGate<Rasm.Fabrication.Process.FabricationPoint, Rasm.Fabrication.Process.FabricationHookFact, Rasm.Domain.TelemetrySource>;
using FabricationObserver = Rasm.Domain.HookTap<Rasm.Fabrication.Process.FabricationPoint, Rasm.Fabrication.Process.FabricationHookFact, Rasm.Domain.TelemetrySource>;
using FabricationHooks = Rasm.Domain.HookSet<Rasm.Fabrication.Process.FabricationPoint, Rasm.Fabrication.Process.FabricationHookFact, Rasm.Domain.TelemetrySource>;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FabricationPoint : IHookRoster<FabricationPoint> {
    public static readonly FabricationPoint Admission =
        new("rasm.fabrication.run.admission", CapabilitySet<HookModality>.Of(HookModality.Veto, HookModality.Observe));
    public static readonly FabricationPoint StageAdvance =
        new("rasm.fabrication.derive.stage", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly FabricationPoint EgressMint =
        new("rasm.fabrication.egress.mint", CapabilitySet<HookModality>.Of(HookModality.Veto, HookModality.Observe));
    public static readonly FabricationPoint VerifyVerdict =
        new("rasm.fabrication.verify.verdict", CapabilitySet<HookModality>.Of(HookModality.Replay, HookModality.Observe));
    public static readonly FabricationPoint Delivery =
        new("rasm.fabrication.delivery.handoff", CapabilitySet<HookModality>.Of(HookModality.Observe));

    public CapabilitySet<HookModality> Modalities { get; }

    public HookId Id => Ids.Value[this];

    public Option<TraceScope> Plane => Option<TraceScope>.None;

    private static readonly Lazy<FrozenDictionary<FabricationPoint, HookId>> Ids = new(
        static () => Items.ToFrozenDictionary(static row => row, static row => HookId.Create(value: row.Key)),
        LazyThreadSafetyMode.ExecutionAndPublication);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FabricationHookFact : IHookFact<FabricationPoint> {
    private FabricationHookFact() { }

    public sealed record Admission(FabricationInput Input) : FabricationHookFact;
    public sealed record StageAdvance(PlannedStep Step) : FabricationHookFact;
    public sealed record EgressMint(ContentKey Produced) : FabricationHookFact;
    public sealed record VerifyVerdict(FabricationResult.VerificationResult Verdict) : FabricationHookFact;
    public sealed record Delivery(RunEvidence Evidence) : FabricationHookFact;

    public bool Seats(FabricationPoint at) => at == At;

    public FabricationPoint At => Map(
        admission:     FabricationPoint.Admission,
        stageAdvance:  FabricationPoint.StageAdvance,
        egressMint:    FabricationPoint.EgressMint,
        verifyVerdict: FabricationPoint.VerifyVerdict,
        delivery:      FabricationPoint.Delivery);
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class FabricationHooks {
    public static Fin<FabricationHooks> Live(Seq<FabricationGate> gates = default, Seq<FabricationObserver> taps = default,
        Option<FaultCell> cell = default) =>
        FabricationHooks.Of(gates, taps, Option<IHookSpan>.None, cell);
}
```

## [06]-[BOARD_PACK]

- Owner: `FabricationDescriptors` — the shop's one kernel `BoardPack` value binding the panel rows and burn-rate objectives over the `[02]` roster.
- Cases: in-service disposition share · conforming-study share · gouge-free verification share · measured-ranking share · cycle-time ceiling · machine-load headroom · remaining-life headroom.
- Entry: `FabricationDescriptors.Pack` is the whole descriptor surface the AppHost alert pipeline and the deploy-plane board compile decode — `Panels` and `Objectives` are its columns, `Alerts` derives one `AlertSpec` per objective per burn row through the kernel fold, and `Pack.Admit(roster)` proves every panel instrument, every break key, every widget resolution, and every indicator series against the declaring port's own roster of the required kind; the pack rides `[02]`'s contributor port outward, so the mounting root runs that proof and this folder exposes no second admission entry.
- Auto: a panel naming an instrument alone reads the kernel widget projection for that row's measurement shape, so only a deliberate reading spells a `PanelKind`; the AppHost alert pipeline and the deploy-plane dashboard compile consume the same pack, so a roster change re-derives verdicts, alerts, and panels in one diff and a hand-authored panel or rule beside it is the drift defect; burn windows, factors, severities, hold, tone, and the budget share all derive from the kernel burn table, and every objective omits its compliance window so kernel admission canonicalizes the one solution default — no threshold, lane, window pair, or calendar literal lands here.
- Packages: Rasm, LanguageExt.Core, NodaTime.
- Growth: a new shop objective is one `Objective` row over an existing indicator shape, and a share over an already-fanned population needs no roster edit at all; a new board panel is one `PanelSpec` on the pack, and a whole passport panel family is one `SustainabilityQuantity` row; a new indicator shape is a kernel `Sli` case breaking every compile leg at once.
- Boundary: indicator, severity, panel, descriptor-row, and burn vocabularies are the kernel capsule's and cross the language boundary as values, never types; a success share is a partition over the ONE counter its outcome dimension already fans — a good-half twin doubles the series the roster mounts and strands its denominator on the next site edit — while `Ratio` stays reserved for genuinely independent counters and a saturation indicator names one pulled level against a bound, because a load or life reading forms no counter pair; a partition's good set is derived where a vocabulary owns the verdict and named off the axis const where the population itself owns it, never spelled as a value literal; every named series is written at its producing site on every occurrence, so no denominator depends on a veto path; the pack's boards and alerts stay descriptor data — query dialect, datasource binding, provisioning, and delivery routing are the deploy plane's.

```csharp
public static class FabricationDescriptors {
    public static readonly BoardPack Pack = new(
        Wire: "fabrication.slo",
        Panels: Seq(
            PanelSpec.Of("Tool wear floor", FabricationInstruments.ToolFloor.Name, FabricationInstruments.BasisSlot),
            PanelSpec.Of("Remaining life by basis", FabricationInstruments.ToolWear.Name, FabricationInstruments.BasisSlot),
            PanelSpec.Of("Maintenance actions", FabricationInstruments.ToolAssessments.Name, PanelKind.Table, FabricationInstruments.BasisSlot, FabricationInstruments.ActionSlot),
            PanelSpec.Of("Catalog refresh age", FabricationInstruments.ToolRefreshAge.Name),
            PanelSpec.Of("Fit residual by model", FabricationInstruments.FitResidual.Name, FabricationInstruments.ModelSlot),
            PanelSpec.Of("Inspection verdicts", FabricationInstruments.ProbeFeatures.Name, FabricationInstruments.VerdictSlot),
            PanelSpec.Of("Worst probe deviation", FabricationInstruments.ProbeDeviation.Name),
            PanelSpec.Of("Capability index", FabricationInstruments.CapabilityIndex.Name, FabricationInstruments.MetricSlot),
            PanelSpec.Of("SPC violations", FabricationInstruments.CapabilityViolations.Name),
            PanelSpec.Of("Study conformance", FabricationInstruments.CapabilityStudies.Name, FabricationInstruments.VerdictSlot),
            PanelSpec.Of("Gouge findings", FabricationInstruments.RemovalDefects.Name),
            PanelSpec.Of("Verification verdicts", FabricationInstruments.RemovalVerifications.Name, FabricationInstruments.VerdictSlot),
            PanelSpec.Of("Residual volume", FabricationInstruments.RemovalResidual.Name, FabricationInstruments.ResidueSlot),
            PanelSpec.Of("Air-cut fraction", FabricationInstruments.RemovalAirCut.Name),
            PanelSpec.Of("Cycle time", FabricationInstruments.CycleDuration.Name),
            PanelSpec.Of("Cycle energy", FabricationInstruments.CycleEnergy.Name, PanelKind.Timeseries),
            PanelSpec.Of("Estimate ledger", FabricationInstruments.EstimateMoney.Name, PanelKind.Table, FabricationInstruments.ScopeSlot, FabricationInstruments.CurrencySlot),
            PanelSpec.Of("Fleet load", FabricationInstruments.FleetLoad.Name, FabricationInstruments.ProcessSlot),
            PanelSpec.Of("Match utilization", FabricationInstruments.FleetUtilization.Name, FabricationInstruments.ProcessSlot),
            PanelSpec.Of("Match ranking evidence", FabricationInstruments.FleetMatches.Name, FabricationInstruments.ProcessSlot, FabricationInstruments.EvidenceSlot),
            PanelSpec.Of("Run duration", FabricationInstruments.RunDuration.Name, FabricationInstruments.ProcessSlot, FabricationInstruments.VerificationSlot),
            PanelSpec.Of("Artifacts by egress kind", FabricationInstruments.RunArtifacts.Name, FabricationInstruments.KindSlot),
            PanelSpec.Of("Run warnings", FabricationInstruments.RunWarnings.Name),
            PanelSpec.Of("Delivery custody", FabricationInstruments.DeliveryPrograms.Name, PanelKind.Table, FabricationInstruments.VerdictSlot, FabricationInstruments.ControllerSlot),
            PanelSpec.Of("Solver steps", FabricationInstruments.EngineSteps.Name, FabricationInstruments.SolverSlot, FabricationInstruments.PhaseSlot))
            + toSeq(SustainabilityQuantity.Items).Map(static row =>
                PanelSpec.Of($"Passport {row.Key}", row.Instrument.Name, PanelKind.Table, FabricationInstruments.MeasureSlot)),
        Objectives: Seq(
            Objective.Create(
                name: "fabrication.wear.serviceable",
                sli: new Sli.Partition(
                    Metric: FabricationInstruments.ToolAssessments.Name,
                    By: FabricationInstruments.ActionSlot,
                    Good: MaintenanceDisposition.ServiceableKeys),
                target: 0.99d,
                window: default),
            Objective.Create(
                name: "fabrication.capability.capable",
                sli: new Sli.Partition(
                    Metric: FabricationInstruments.CapabilityStudies.Name,
                    By: FabricationInstruments.VerdictSlot,
                    Good: Seq(FabricationInstruments.Pass)),
                target: 0.995d,
                window: default),
            Objective.Create(
                name: "fabrication.removal.clean",
                sli: new Sli.Partition(
                    Metric: FabricationInstruments.RemovalVerifications.Name,
                    By: FabricationInstruments.VerdictSlot,
                    Good: Seq(FabricationInstruments.Pass)),
                target: 0.999d,
                window: default),
            Objective.Create(
                name: "fabrication.fleet.measured",
                sli: new Sli.Partition(
                    Metric: FabricationInstruments.FleetMatches.Name,
                    By: FabricationInstruments.EvidenceSlot,
                    Good: Seq(FabricationInstruments.Measured)),
                target: 0.95d,
                window: default),
            Objective.Create(
                name: "fabrication.cycle.envelope",
                sli: new Sli.Latency(Metric: FabricationInstruments.CycleDuration.Name, Ceiling: Duration.FromHours(4), Quantile: 0.95d),
                target: 0.95d,
                window: default),
            Objective.Create(
                name: "fabrication.fleet.headroom",
                sli: new Sli.Saturation(Metric: FabricationInstruments.FleetLoad.Name, Bound: 0.85d, Breach: LevelBreach.Ceiling),
                target: 0.9d,
                window: default),
            Objective.Create(
                name: "fabrication.tool.headroom",
                sli: new Sli.Saturation(Metric: FabricationInstruments.ToolFloor.Name, Bound: 0.15d, Breach: LevelBreach.Floor),
                target: 0.95d,
                window: default)));
}
```

## [07]-[RESEARCH]

(none)
