# [RASM_FABRICATION_TELEMETRY]

`FabricationFact` is the package's one fact vocabulary for measured production: every operational metric is a projection of a settled domain receipt flattened onto this union, and the instrument roster, the contributor port, the projection arms, the span band, and the classification rows all derive from it — a metric minted beside the fan is a second truth. Domain kernels stay pure; facts fire only where receipts settle on the run spine through `FabricationRuntime`'s one `FabricationTap` port.

Settled composition draws every mechanism from the kernel signal capsule — the causal frame with its receipt sink, the hook capsule, the instrument capsule with its bucket advice and level cells, the trace band, and the SLO algebra; each fence prelude names the exact rows it binds. Composition mints the meter, merges this arm table onto the AppHost receipt fan under each message envelope's own `TenantContext.Stamp` bracket, mounts hook points through `HookRegistry.Mount`, admits the solver scopes into its `SpanBand`, and holds no OpenTelemetry reference, exporter, or provider. Instrument names run dotted `rasm.fabrication.<domain>.<measure>` with UCUM units under the `TelemetrySource.Fabrication` scope the composing app admits by name, every job row carrying the kernel `rasm.tenant` partition.

## [01]-[INDEX]

- [02]-[FACT_UNION]: `FabricationFact` kind-keyed union, the solver, phase, and sustainability-quantity vocabularies, per-receipt `Of` projections, wire context, tap port, and sink-bound emission.
- [03]-[INSTRUMENT_ROSTER]: `rasm.fabrication.*` `InstrumentSpec` roster, bucket advice, level rows, and the contributor port.
- [04]-[FACT_PROJECTION]: Kind-keyed projection arms from the receipt message envelope onto mounted instruments.
- [05]-[CLASSIFICATION]: Suite-taxonomy attribute rows for the classified receipt members.
- [06]-[SPANS]: `FabricationTrace` rosters the solver scopes the composition admits into the kernel span band and owns the lane bracket and mark.
- [07]-[HOOK_RAIL]: `FabricationPoint` closes the `rasm.fabrication.<domain>.<point>` vocabulary, `FabricationHookFact` closes the spine payloads over it, and `FabricationHooks` mints the one kernel rail.
- [08]-[BOARD_PACK]: `FabricationDescriptors` binds the kernel pack over that roster.

## [02]-[FACT_UNION]

- Owner: `FabricationFact` — the closed fact union; `FabricationEngine` and `EnginePhase` — the solver-lane and lane-point vocabularies every engine row, span scope, and span mark keys on; `SustainabilityQuantity` — the UCUM-unit axis every sealed passport measure resolves its instrument through; `FabricationWireContext` — the Strict serializer context whose polymorphism metadata is the one kind registry; `FabricationTap` — the runtime emission port; `FabricationSurface` — the sink-bound emission seam.
- Cases: tool-wear · tool-refresh · cutting-fit · probe · capability · removal · cycle · estimate · fleet-match · run · quality-seal · traveler · delivery · engine.
- Law: `FactKind` is the ONE kind roster and `FactField` the ONE wire-field roster. The polymorphic registration here and the arm table at `[04]` both key off `FactKind`, and every wire-arm read names a `FactField` const, so a kind cannot be serialized under one literal and mounted under another and a renamed property cannot leave an arm reading a field that no longer exists; a typed arm reads the case's own properties, the compiler holding for it what the const roster holds for the wire arms. Two rosters over one vocabulary is the named defect.
- Entry: `FabricationTap.Fire(FabricationFact fact)` — the sole in-package emission verb; `FabricationSurface.Emit(CorrelationId correlation, FabricationFact fact)` binds sink and serializer once at composition and the app root wires the tap onto it, so `FabricationTap.Silent` keeps a headless kernel run emitting into unit with zero branching.
- Auto: each `Of` projection flattens its receipt to measures and bounded dimensions at the fact boundary — a smart-enum spine value crosses as its key scalar, a NodaTime span as seconds — so the wire context serializes primitives only; wire kind derives from the polymorphic metadata pinned on the union under one declared discriminator const, and ambient `TenantContext.Current` threads into `Send` so the message envelope's tenant field partitions evidence; `ToolWear.Of` yields `None` on a receipt without a critical state and `ToolRefresh.Of` on a provider-digest source, so non-measured admissions project nothing rather than fabricate zeros, and `QualitySeal.Of` holds the same law across the whole passport evidence union — each sealed measure crosses as one row keyed by its own case name under the `SustainabilityQuantity` its unit selects, and an unsealed measure crosses as no row rather than a zero reading; every engine row derives its owning solver from its phase row, so the two vocabularies cannot drift and a hand-spelled solver string has no construction path.
- Receipt: none — the union projects settled receipts. `Probe` mints at the datum-result fold because its pre-egress report is file-scoped there; every other case mints through its `Of` row here. `Removal.Of` consumes the public verification result, `Delivery.Of` consumes the settled program-delivery receipt, and each `Engine.Of` row consumes its solver receipt.
- Packages: Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new measured concern is one case row, one `[JsonDerivedType]` registration, one `Of` projection, one roster row at `[03]`, one projection arm at `[04]`, and ONE `Fire` site at the fold where its receipt settles — a case complete on the first five and absent from the sixth declares an instrument nothing ever writes, which is the named defect and the shape a roster audit hunts for; a new solver lane is one `FabricationEngine` row, gaining `EnginePhase` rows and an `Of` overload only where the lane counts internal steps; a new lane milestone is one uncounted `EnginePhase` row; a new sealed measure is one arm on the passport fold, and a measure in a unit no row carries is one `SustainabilityQuantity` row that mints its roster entry, its arm route, and its board panel together; a case whose receipt gains a measure widens that case, never a sibling.
- Boundary: fact cases carry no `ContentKey`, no personnel or heat identity, and no free-text detail — the receipt rail owns identity and the classification rows at `[05]` bar the classified members structurally; the `[JsonDerivedType]` kind column is the canonical spelling the message envelope carries to the sink rail, so a kind outside this roster is receipt-only by declaration; a tap subscriber fault parks as `IsolatedFault` on the port's own cell and never re-enters the emitting fold, so a swallowed emission is still evidence a support bundle reads.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Compliance.Classification;
using NodaTime;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Fabrication.Additive;
using Rasm.Fabrication.Documentation;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Forming;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Nesting;
using Rasm.Fabrication.Posting;
using Rasm.Fabrication.Spec;
using Rasm.Fabrication.Toolpath;
using Rasm.Fabrication.Tooling;
using Rasm.Fabrication.Verify;
using Rasm.Processing;
using Thinktecture;
using Thinktecture.Text.Json.Serialization;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Process;

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

    private static readonly Lazy<FrozenDictionary<SustainabilityQuantity, string>> Names = new(
        static () => Items.ToFrozenDictionary(static row => row, static row => $"rasm.fabrication.sustainability.{row.Key}"),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public string Instrument => Names.Value[this];
}

// --- [MODELS] --------------------------------------------------------------------------
public static class FactKind {
    public const string ToolWear = "tool-wear";
    public const string ToolRefresh = "tool-refresh";
    public const string CuttingFit = "cutting-fit";
    public const string Probe = "probe";
    public const string Capability = "capability";
    public const string Removal = "removal";
    public const string Cycle = "cycle";
    public const string Estimate = "estimate";
    public const string FleetMatch = "fleet-match";
    public const string Run = "run";
    public const string QualitySeal = "quality-seal";
    public const string Traveler = "traveler";
    public const string Delivery = "delivery";
    public const string Engine = "engine";
}

public static class FactField {
    public const string Action = "action";
    public const string Amendments = "amendments";
    public const string AgeSeconds = "ageSeconds";
    public const string AirCutRatio = "airCutRatio";
    public const string Basis = "basis";
    public const string CarbonKg = "carbonKg";
    public const string ClockSeconds = "clockSeconds";
    public const string Conforming = "conforming";
    public const string Controller = "controller";
    public const string Count = "count";
    public const string Currency = "currency";
    public const string Determination = "determination";
    public const string DistanceMm = "distanceMm";
    public const string Effectiveness = "effectiveness";
    public const string EnergyKwh = "energyKwh";
    public const string Features = "features";
    public const string FitResidual = "fitResidual";
    public const string FractionRemaining = "fractionRemaining";
    public const string Gouges = "gouges";
    public const string Kinds = "kinds";
    public const string Measure = "measure";
    public const string Measured = "measured";
    public const string Metric = "metric";
    public const string Model = "model";
    public const string Money = "money";
    public const string OvercutMm3 = "overcutMm3";
    public const string Phase = "phase";
    public const string Process = "process";
    public const string ProgramKind = "programKind";
    public const string Quantity = "quantity";
    public const string Residual = "residual";
    public const string Rows = "rows";
    public const string Scope = "scope";
    public const string Seconds = "seconds";
    public const string SimulationBacked = "simulationBacked";
    public const string Sustainability = "sustainability";
    public const string UncutMm3 = "uncutMm3";
    public const string Utilization = "utilization";
    public const string Value = "value";
    public const string Verified = "verified";
    public const string Verification = "verification";
    public const string Violations = "violations";
    public const string Warnings = "warnings";
    public const string WorstDeviationMm = "worstDeviationMm";
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = FabricationFact.KindProperty)]
[JsonDerivedType(typeof(ToolWear), FactKind.ToolWear)]
[JsonDerivedType(typeof(ToolRefresh), FactKind.ToolRefresh)]
[JsonDerivedType(typeof(CuttingFit), FactKind.CuttingFit)]
[JsonDerivedType(typeof(Probe), FactKind.Probe)]
[JsonDerivedType(typeof(Capability), FactKind.Capability)]
[JsonDerivedType(typeof(Removal), FactKind.Removal)]
[JsonDerivedType(typeof(Cycle), FactKind.Cycle)]
[JsonDerivedType(typeof(Estimate), FactKind.Estimate)]
[JsonDerivedType(typeof(FleetMatch), FactKind.FleetMatch)]
[JsonDerivedType(typeof(Run), FactKind.Run)]
[JsonDerivedType(typeof(QualitySeal), FactKind.QualitySeal)]
[JsonDerivedType(typeof(Traveler), FactKind.Traveler)]
[JsonDerivedType(typeof(Delivery), FactKind.Delivery)]
[JsonDerivedType(typeof(Engine), FactKind.Engine)]
public abstract partial record FabricationFact {
    public const string KindProperty = "kind";

    private FabricationFact() { }

    public sealed record ToolWear(string Basis, double FractionRemaining, double ConservativeRemaining, string Action,
        Option<double> FitResidual) : FabricationFact {
        public static Option<ToolWear> Of(WearReceipt receipt) =>
            receipt.Critical.Map(critical => new ToolWear(
                critical.Basis.Key,
                critical.FractionRemaining,
                critical.ConservativeRemaining,
                receipt.Action.Disposition.Key,
                receipt.Diagnostics.Map(static row => row.RootMeanSquareResidual).Max()));
    }

    public sealed record ToolRefresh(double AgeSeconds) : FabricationFact {
        public static Option<ToolRefresh> Of(CatalogReceipt receipt) =>
            receipt.Source is CatalogSource.Telemetry telemetry
                ? Some(new ToolRefresh((receipt.ObservedAt - telemetry.Previous).TotalSeconds))
                : None;
    }

    public sealed record CuttingFit(string Model, double Residual, double Determination) : FabricationFact {
        public static CuttingFit Of(string model, PowerLaw fit) => new(model, fit.RootMeanSquareResidual, fit.RSquared);
    }

    public sealed record Probe(int Features, int Conforming, double WorstDeviationMm) : FabricationFact;

    public sealed record CapabilityFactRow(string Metric, double Value, double Demanded, bool Pass);

    public sealed record Capability(Seq<CapabilityFactRow> Rows, int Violations) : FabricationFact {
        public static Capability Of(CapabilityReport report) =>
            new(report.Rows.Map(static row => new CapabilityFactRow(row.Metric.Key, row.Value, row.Demanded, row.Pass)),
                report.Violations.Count);
    }

    public sealed record Removal(int Gouges, double UncutMm3, double OvercutMm3, double AirCutRatio) : FabricationFact {
        public static Removal Of(FabricationResult.VerificationResult result) =>
            new(result.Gouges.Count, result.UncutVolume, result.OvercutVolume, result.AirCutRatio);
    }

    public sealed record Cycle(double Seconds, double EnergyKwh, double DistanceMm) : FabricationFact {
        public static Cycle Of(SimulationLedger ledger) => new(ledger.Cycle.TotalSeconds, ledger.EnergyKwh, ledger.DistanceMm);
    }

    public sealed record Estimate(string Scope, string Currency, double Money, double CarbonKg, double ClockSeconds, bool SimulationBacked) : FabricationFact {
        public static Estimate Of(EstimateReceipt receipt) => receipt.Switch(
            unit: static value => new Estimate(
                "unit", value.Receipt.Evidence.Currency.ToString(), (double)value.Receipt.Evidence.MoneyTotal,
                value.Receipt.Evidence.CarbonTotalKgCo2e,
                value.Receipt.Evidence.MachineTime.TotalSeconds, value.Receipt.Evidence.SimulationBacked),
            lot: static value => new Estimate(
                "lot", value.Ledger.Unit.Evidence.Currency.ToString(), (double)value.Ledger.QuotedTotal,
                value.Ledger.CarbonTotalKgCo2e,
                value.Ledger.Unit.Evidence.MachineTime.TotalSeconds, value.Ledger.Unit.Evidence.SimulationBacked));
    }

    public sealed record FleetMatch(string Process, double Utilization, double Score, double Effectiveness, bool Measured) : FabricationFact {
        public static FleetMatch Of(MachineMatch match, bool measured) =>
            new(match.Process.Key, match.Utilization, match.Score, match.Effectiveness, measured);
    }

    public sealed record Run(string Process, double Seconds, Seq<string> Kinds, int Produced, int Warnings, string Verification) : FabricationFact {
        public static Run Of(RunEvidence evidence, Duration elapsed) =>
            new(evidence.Process.Key,
                elapsed.TotalSeconds,
                evidence.Produced.Map(static key => key.Kind.Key),
                evidence.Produced.Count,
                evidence.Warnings.Count,
                evidence.Verified.Match(Some: static pass => pass ? "verified" : "failed", None: static () => "unverified"));
    }

    public sealed record SustainabilityRow(string Quantity, string Measure, double Value);

    public sealed record QualitySeal(int Declarations, Seq<SustainabilityRow> Sustainability) : FabricationFact {
        public static QualitySeal Of(PassportEvidence passport) =>
            new(passport.Declarations.Count, passport.Sustainability.Map(Row).Strict());

        static SustainabilityRow Row(SustainabilityEvidence evidence) => evidence.Switch(
            energyUse: static value => Measured(value, SustainabilityQuantity.Energy, value.Value.Joules),
            carbon: static value => Measured(value, SustainabilityQuantity.Mass, value.Value.Kilograms),
            waste: static value => Measured(value, SustainabilityQuantity.Mass, value.Value.Kilograms),
            recycledContent: static value => Measured(value, SustainabilityQuantity.Fraction, value.Value.DecimalFractions),
            waterUse: static value => Measured(value, SustainabilityQuantity.Volume, value.Value.Liters),
            renewableEnergy: static value => Measured(value, SustainabilityQuantity.Fraction, value.Value.DecimalFractions),
            recyclableMass: static value => Measured(value, SustainabilityQuantity.Mass, value.Value.Kilograms),
            hazardousSubstance: static value => Measured(value, SustainabilityQuantity.Mass, value.Value.Kilograms),
            repairability: static value => Measured(value, SustainabilityQuantity.Fraction, value.Value.DecimalFractions),
            durability: static value => Measured(value, SustainabilityQuantity.Lifetime, value.Value.TotalSeconds));

        static SustainabilityRow Measured(SustainabilityEvidence evidence, SustainabilityQuantity quantity, double value) =>
            new(quantity.Key, evidence.Measure, value);
    }

    public sealed record Traveler(int Amendments, int Produced) : FabricationFact {
        public static Traveler Of(TravelerArtifact artifact) => new(artifact.Amendments.Count, artifact.Produced.Count);
    }

    public sealed record Delivery(string ProgramKind, bool Verified, string Controller, string Acknowledged, int Records) : FabricationFact {
        public static Delivery Of(ProgramDelivery delivery) => new(
            delivery.Image.Kind.Key,
            delivery.Verified,
            delivery.Controller,
            delivery.Acknowledged.Key,
            delivery.Records);
    }

    public sealed record Engine(EnginePhase Phase, long Count) : FabricationFact {
        public static Seq<Engine> Of(NestEvidence evidence) => Rows(
            (EnginePhase.Candidates, evidence.Candidates),
            (EnginePhase.Evaluated, evidence.Evaluated),
            (EnginePhase.CandidatesRejected, evidence.Rejected),
            (EnginePhase.MemoHits, evidence.MemoHits),
            (EnginePhase.MemoMisses, evidence.MemoMisses),
            (EnginePhase.Moulds, evidence.Moulds),
            (EnginePhase.ChiralFloor, evidence.ChiralFloor));

        public static Seq<Engine> Of(SkeletonWalk receipt) => Rows(
            (EnginePhase.Nodes, receipt.NodeCount),
            (EnginePhase.Arcs, receipt.ArcCount),
            (EnginePhase.Passes, receipt.Passes.Count));

        public static Seq<Engine> Of(SetupSchedule schedule) => Rows((EnginePhase.Decisions, schedule.Decisions.Count));

        public static Seq<Engine> Of(ScanEvidence evidence) => Rows(
            (EnginePhase.Exposures, evidence.Exposures),
            (EnginePhase.Jumps, evidence.Jumps),
            (EnginePhase.Remelts, evidence.Remelts),
            (EnginePhase.Stitches, evidence.Stitches));

        public static Seq<Engine> Of(AlignmentReceipt receipt) => Rows((EnginePhase.IcpIterations, receipt.Iterations));

        public static Seq<Engine> Of(BendPlan plan) => Rows(
            (EnginePhase.Expansions, plan.Expansions),
            (EnginePhase.ExpansionsRejected, plan.Rejected));

        static Seq<Engine> Rows(params ReadOnlySpan<(EnginePhase Phase, long Count)> counts) =>
            toSeq(counts.ToArray()).Map(static row => new Engine(row.Phase, row.Count)).Strict();
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true,
    Converters = [typeof(ThinktectureJsonConverterFactory), typeof(LanguageExtJsonConverterFactory)])]
[JsonSerializable(typeof(FabricationFact))]
public partial class FabricationWireContext : JsonSerializerContext;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record FabricationTap(Func<FabricationFact, Unit> Send, FaultCell Faults) {
    private static readonly HookId Emission = HookId.Create("rasm.fabrication.tap.emit");
    private static readonly Op Raise = Op.Of();

    public static readonly FabricationTap Silent =
        new(static _ => unit, new FaultCell(cap: Dimension.Create(value: 1), clock: TimeProvider.System));

    public Unit Fire(FabricationFact fact) =>
        Raise.Catch(() => Fin.Succ(Send(fact))).Match(
            Succ: static _ => unit,
            Fail: error => ignore(Faults.Park(point: Emission, cause: error)));
}

public sealed class FabricationSurface(ReceiptSinkPort sink, FabricationWireContext wire) {
    private static readonly Op Serialize = Op.Of();

    public IO<ReceiptEnvelope> Emit(CorrelationId correlation, FabricationFact fact) =>
        IO.lift(() => Serialize.Catch(() => Fin.Succ(JsonSerializer.SerializeToElement(fact, wire.FabricationFact))))
            .Bind(payload => payload.TryGetProperty(FabricationFact.KindProperty, out JsonElement kind) && kind.GetString() is { } key
                ? sink.Send(correlation, TenantContext.Current, TelemetrySource.Fabrication.Key, key, payload)
                : IO.fail<ReceiptEnvelope>(new KernelFault.InvalidValue(
                    Label: nameof(FabricationFact),
                    Requirement: "a polymorphic kind discriminator on the serialized fact")));
}
```

## [03]-[INSTRUMENT_ROSTER]

- Owner: `FabricationInstruments` — the Fabrication `InstrumentSpec` roster and the `TelemetryContributorPort` mint; the roster is composition-free data, so one declaration binds against any meter and any cells.
- Entry: `FabricationInstruments.Telemetry(string version)` — the one contributor port (scope `TelemetrySource.Fabrication`) carrying the domain row set and the `[08]` board pack over those same rows into the composing root; the mint stamps the kernel schema coordinate as `MeterOptions.TelemetrySchemaUrl`, so every `rasm.fabrication.*` scope reads with pinned semantics and the shop's board proves against the roster the root just bound.
- Auto: every advised histogram row ships its named kernel `Buckets` boundaries at creation, the fallback a backend without exponential histograms reads — the default aggregation stays base2 exponential at the provider and no bound array is spelled here; registry mount de-duplicates by name, so a duplicate row is a composition fault, never a forked stream; the projection arms at `[04]-[FACT_PROJECTION]` write the level cells, so each pulled row reads a current level, never a re-derived scan; every share indicator partitions ONE mounted population on the outcome dimension its own row declares, so a good half is a tag value the same arm already stamps and no roster row exists to carry a numerator; every outcome axis reads its values off this owner's consts, so an arm and a board row never spell one value twice; the sustainability family generates from `SustainabilityQuantity`, so the roster carries one row per UCUM unit rather than one per measure and both the projection arm and the board panels resolve off that one axis.
- Packages: Rasm, LanguageExt.Core, BCL inbox.
- Growth: one measured concern is one `InstrumentSpec` here and one projection arm at `[04]-[FACT_PROJECTION]`; a per-kind family derives from its owning vocabulary, never hand-enumerated rows; a new level is one `cells.Level` write at its producing arm beside one `Level` row, or one keyed write beside one `Levels` row where the shop holds a reading per basis, process, or machine.
- Boundary: instrument names are dotted `rasm.fabrication.<domain>.<measure>` with UCUM units, never pre-baked `_total` or unit suffixes; the port's `Scope` is the version-stamped package id the composing root admits by name; facts are event-shaped and ride counters and histograms while level-shaped measures ride pulled rows reading the composition's cells at collection cadence; every dimension key is a declared slot const on its own row's `Dimensions` column, so the governance leg derives view tag keys from the mounted roster and no second roster restates them; tenancy is the kernel `TenantContext` projection every job row declares, so this page holds no tenant key, no baggage read, and no zero sentinel; a scalar pulled level carries no call-site tag, because a tag whose value flips between collections strands the previous value's series live forever — a level holding one reading per basis or process is a keyed `Levels` family whose tag IS its cell key, and provenance beyond that key rides the event-shaped rows that carry it.

```csharp signature
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

    private static readonly Seq<(string Field, string Value)> ResidueColumns = Seq(
        (FactField.UncutMm3, "uncut"), (FactField.OvercutMm3, "overcut"));

    private const string TaylorModel = "taylor";

    public const string ToolAssessments = "rasm.fabrication.tool.assessments";
    public const string ToolWear = "rasm.fabrication.tool.wear";
    public const string ToolRefreshAge = "rasm.fabrication.tool.refresh.age";
    public const string FitResidual = "rasm.fabrication.fit.residual";
    public const string FitQuality = "rasm.fabrication.fit.quality";
    public const string ProbeFeatures = "rasm.fabrication.probe.features";
    public const string ProbeDeviation = "rasm.fabrication.probe.deviation";
    public const string CapabilityStudies = "rasm.fabrication.capability.studies";
    public const string CapabilityIndex = "rasm.fabrication.capability.index";
    public const string CapabilityViolations = "rasm.fabrication.capability.violations";
    public const string RemovalVerifications = "rasm.fabrication.removal.verifications";
    public const string RemovalDefects = "rasm.fabrication.removal.defects";
    public const string RemovalResidual = "rasm.fabrication.removal.residual";
    public const string RemovalAirCut = "rasm.fabrication.removal.aircut";
    public const string CycleDuration = "rasm.fabrication.cycle.duration";
    public const string CycleEnergy = "rasm.fabrication.cycle.energy";
    public const string CycleDistance = "rasm.fabrication.cycle.distance";
    public const string EstimateMoney = "rasm.fabrication.estimate.money";
    public const string EstimateCarbon = "rasm.fabrication.estimate.carbon";
    public const string EstimateClock = "rasm.fabrication.estimate.clock";
    public const string FleetMatches = "rasm.fabrication.fleet.matches";
    public const string FleetUtilization = "rasm.fabrication.fleet.utilization";
    public const string FleetEffectiveness = "rasm.fabrication.fleet.effectiveness";
    public const string FleetLoad = "rasm.fabrication.fleet.load";
    public const string ToolFloor = "rasm.fabrication.tool.floor";
    public const string RunDuration = "rasm.fabrication.run.duration";
    public const string RunArtifacts = "rasm.fabrication.run.artifacts";
    public const string RunWarnings = "rasm.fabrication.run.warnings";
    public const string TravelerAmendments = "rasm.fabrication.traveler.amendments";
    public const string DeliveryPrograms = "rasm.fabrication.delivery.programs";
    public const string EngineSteps = "rasm.fabrication.engine.steps";

    public static readonly Seq<InstrumentSpec> Rows = Seq(
        InstrumentSpec.Create(ToolAssessments, InstrumentKind.Count, MeasureForm.Whole, "{assessment}",
            "wear assessments settled at a critical state by basis and maintenance disposition", Seq(TenantContext.TenantSlot, BasisSlot, ActionSlot), None, None, None),
        InstrumentSpec.Create(ToolWear, InstrumentKind.Distribution, MeasureForm.Real, "1",
            "remaining-life fraction at the critical wear state", Seq(TenantContext.TenantSlot, BasisSlot, ActionSlot), Some(Buckets.Fractions), None, None),
        InstrumentSpec.Create(ToolRefreshAge, InstrumentKind.Distribution, MeasureForm.Real, "s",
            "interval between successive telemetry catalog refreshes", Seq<string>(), Some(Buckets.RefreshSeconds), None, None),
        InstrumentSpec.Create(FitResidual, InstrumentKind.Distribution, MeasureForm.Real, "1",
            "RMS residual of the wear and machinability power-law fits", Seq(TenantContext.TenantSlot, ModelSlot), None, None, None),
        InstrumentSpec.Create(FitQuality, InstrumentKind.Distribution, MeasureForm.Real, "1",
            "coefficient of determination of the machinability fit", Seq(TenantContext.TenantSlot, ModelSlot), None, None, None),
        InstrumentSpec.Create(ProbeFeatures, InstrumentKind.Count, MeasureForm.Whole, "{feature}",
            "inspected features by conformance verdict", Seq(TenantContext.TenantSlot, VerdictSlot), None, None, None),
        InstrumentSpec.Create(ProbeDeviation, InstrumentKind.Distribution, MeasureForm.Real, "mm",
            "worst absolute measured deviation per inspection", Seq(TenantContext.TenantSlot), Some(Buckets.Millimeters), None, None),
        InstrumentSpec.Create(CapabilityStudies, InstrumentKind.Count, MeasureForm.Whole, "{study}",
            "capability studies settled by SPC conformance verdict", Seq(TenantContext.TenantSlot, VerdictSlot), None, None, None),
        InstrumentSpec.Create(CapabilityIndex, InstrumentKind.Distribution, MeasureForm.Real, "1",
            "capability and performance index values by metric row", Seq(TenantContext.TenantSlot, MetricSlot), None, None, None),
        InstrumentSpec.Create(CapabilityViolations, InstrumentKind.Count, MeasureForm.Whole, "{violation}",
            "SPC rule violations per study", Seq(TenantContext.TenantSlot), None, None, None),
        InstrumentSpec.Create(RemovalVerifications, InstrumentKind.Count, MeasureForm.Whole, "{verification}",
            "material-removal verifications settled by gouge-finding verdict", Seq(TenantContext.TenantSlot, VerdictSlot), None, None, None),
        InstrumentSpec.Create(RemovalDefects, InstrumentKind.Count, MeasureForm.Whole, "{finding}",
            "gouge findings per material-removal verification", Seq(TenantContext.TenantSlot), None, None, None),
        InstrumentSpec.Create(RemovalResidual, InstrumentKind.Distribution, MeasureForm.Real, "mm3",
            "uncut and overcut voxel volume per verification", Seq(TenantContext.TenantSlot, ResidueSlot), None, None, None),
        InstrumentSpec.Create(RemovalAirCut, InstrumentKind.Distribution, MeasureForm.Real, "1",
            "air-cut fraction of swept program motion per verification", Seq(TenantContext.TenantSlot), Some(Buckets.Fractions), None, None),
        InstrumentSpec.Create(CycleDuration, InstrumentKind.Distribution, MeasureForm.Real, "s",
            "simulated modal cycle time per program", Seq(TenantContext.TenantSlot), Some(Buckets.CycleSeconds), None, None),
        InstrumentSpec.Create(CycleEnergy, InstrumentKind.Distribution, MeasureForm.Real, "kW.h",
            "simulated machine energy per program", Seq(TenantContext.TenantSlot), None, None, None),
        InstrumentSpec.Create(CycleDistance, InstrumentKind.Distribution, MeasureForm.Real, "mm",
            "simulated cutting-motion path length per program", Seq(TenantContext.TenantSlot), None, None, None),
        InstrumentSpec.Create(EstimateMoney, InstrumentKind.Distribution, MeasureForm.Real, "{money}",
            "signed money ledger total in receipt currency", Seq(TenantContext.TenantSlot, ScopeSlot, CurrencySlot), None, None, None),
        InstrumentSpec.Create(EstimateCarbon, InstrumentKind.Distribution, MeasureForm.Real, "kg",
            "carbon ledger total as kilograms CO2-equivalent", Seq(TenantContext.TenantSlot, ScopeSlot), None, None, None),
        InstrumentSpec.Create(EstimateClock, InstrumentKind.Distribution, MeasureForm.Real, "s",
            "estimated machine clock per subject", Seq(TenantContext.TenantSlot, BackedSlot), Some(Buckets.CycleSeconds), None, None),
        InstrumentSpec.Create(FleetMatches, InstrumentKind.Count, MeasureForm.Whole, "{match}",
            "machine matches assessed by process and ranking evidence", Seq(TenantContext.TenantSlot, ProcessSlot, EvidenceSlot), None, None, None),
        InstrumentSpec.Create(FleetUtilization, InstrumentKind.Distribution, MeasureForm.Real, "1",
            "machine load factor at match assessment", Seq(TenantContext.TenantSlot, ProcessSlot), Some(Buckets.Fractions), None, None),
        InstrumentSpec.Create(FleetEffectiveness, InstrumentKind.Distribution, MeasureForm.Real, "1",
            "machine effectiveness fraction at match assessment", Seq(TenantContext.TenantSlot, ProcessSlot), Some(Buckets.Fractions), None, None),
        InstrumentSpec.Create(FleetLoad, InstrumentKind.Levels, MeasureForm.Real, "1",
            "latest machine load factor at match assessment by process", Seq<string>(), None, Some(ProcessSlot), None),
        InstrumentSpec.Create(ToolFloor, InstrumentKind.Levels, MeasureForm.Real, "1",
            "latest remaining-life fraction at the critical wear state by basis", Seq<string>(), None, Some(BasisSlot), None),
        InstrumentSpec.Create(RunDuration, InstrumentKind.Distribution, MeasureForm.Real, "s",
            "fabrication run wall duration", Seq(TenantContext.TenantSlot, ProcessSlot, VerificationSlot), Some(Buckets.CycleSeconds), None, None),
        InstrumentSpec.Create(RunArtifacts, InstrumentKind.Count, MeasureForm.Whole, "{artifact}",
            "content-keyed artifacts produced by egress kind", Seq(TenantContext.TenantSlot, KindSlot), None, None, None),
        InstrumentSpec.Create(RunWarnings, InstrumentKind.Count, MeasureForm.Whole, "{warning}",
            "run warnings accumulated on the evidence receipt", Seq(TenantContext.TenantSlot), None, None, None),
        InstrumentSpec.Create(TravelerAmendments, InstrumentKind.Distribution, MeasureForm.Whole, "{amendment}",
            "as-run amendment chain length at traveler seal", Seq(TenantContext.TenantSlot), None, None, None),
        InstrumentSpec.Create(DeliveryPrograms, InstrumentKind.Count, MeasureForm.Whole, "{program}",
            "posted programs delivered to controllers by custody verdict", Seq(TenantContext.TenantSlot, KindSlot, VerdictSlot, ControllerSlot), None, None, None),
        InstrumentSpec.Create(EngineSteps, InstrumentKind.Count, MeasureForm.Whole, "{step}",
            "solver-internal step counts by solver and phase", Seq(TenantContext.TenantSlot, SolverSlot, PhaseSlot), None, None, None))
        + toSeq(SustainabilityQuantity.Items).Map(static row => InstrumentSpec.Create(
            row.Instrument, InstrumentKind.Distribution, MeasureForm.Real, row.Unit,
            "sealed passport sustainability evidence by measure", Seq(TenantContext.TenantSlot, MeasureSlot),
            None, None, None));

    public static TelemetryContributorPort Telemetry(string version) =>
        new(Scope: TelemetrySource.Fabrication.Key, Version: version, Instruments: Rows,
            Planes: toSeq(FabricationTrace.Scopes), Classifications: FabricationClassified.Values,
            Board: FabricationDescriptors.Pack);
}
```

## [04]-[FACT_PROJECTION]

- Owner: `FabricationInstruments.Arms` — the contributed kind-arm table over the Fabrication kind registry, one of the tables the composition root hands `InstrumentFan.Mount` on its `contributed` argument; `FabricationInstruments.Facts` — the typed-fact twin table the same fan mounts beside it.
- Entry: `Arms` enters `ReceiptFan.Of` as one contributed table beside the Persistence `StoreInstruments.Arms` precedent and merges onto the fan's frozen arm map, so `ReceiptFan.Project` folds every message envelope the sink emits into instrument writes with zero call-site metering; a duplicate kind across any two tables faults at the frozen merge; `Facts` enters the same `Of`'s typed side through `ReceiptFan.Arm`, covering the kinds whose emission and projection share the suite process — an in-process round reads its fields off the typed case the emitter just built, and every uncovered kind rides the typed dispatch's own wire fallthrough.
- Auto: dimension values ride the payload's own key-scalar fields, so tag vocabularies stay bounded by the union's admission; every tag set materializes through the kernel `InstrumentSet.Tags` entry, which folds the ambient `TenantContext` partition in beside the arm's own slots, so a partitioned shop attributes every job row and a single-tenant one mints no dimension at all, while a hand-spelled `KeyValuePair` array beside it re-mints the one materialization the capsule owns and drops the partition on the arm that forgets it; a kind without a table row stays wire-only by declaration, and a fact field without an arm write stays wire evidence — `ConservativeRemaining` carries basis-keyed units one UCUM histogram cannot hold, `Score` is objective-relative, and the `Produced` and `Declarations` counts derive in the message envelope store; an arm re-admitting a wire key through its vocabulary's generated `Validate` derives the dimension from that one roster rather than a second wire field, so an unadmitted key refuses instead of writing a value no partition selects and no roster row mounts; an outcome-partitioned population stamps its verdict on the one write that counts it, so a share's good half can never miss an occurrence its denominator recorded.
- Packages: LanguageExt.Core, Rasm, BCL inbox.
- Growth: a new projected kind is one table row here and its instrument row at `[03]-[INSTRUMENT_ROSTER]`.
- Boundary: arm bodies are the one place fact wire names meet instrument writes — the platform-forced statement seam — and an arm re-reads only the one key its own tag vocabulary owns, never the payload fields its typed fact already admitted; arm execution rides the receipt-tap subscription the AppHost fan mounts on its hook rail, so a fan failure is that rail's shielded fault and never re-enters the emitting fold, and that fan brackets each message envelope in its OWN `TenantContext.Stamp` before projecting; a refused write short-circuits its own arm and rides `InstrumentArm`'s `Fin<Unit>` out through `ReceiptFan.Project` to that fan's rail-shaped `Observe`, which parks it point-attributed beside every other tap fault — no arm reaches a discard site and no folder mints a refusal cell of its own.

```csharp signature
public static partial class FabricationInstruments {
    public static readonly FrozenDictionary<string, InstrumentArm> Arms =
        new Dictionary<string, InstrumentArm> {
            [FactKind.ToolWear] = static (set, payload) =>
                from edge in payload.GetProperty(FactField.Basis).GetString() is { } key
                    ? Fin.Succ(key)
                    : Fin.Fail<string>(new KernelFault.InvalidValue(Label: BasisSlot, Requirement: "a basis key on every wear assessment"))
                from disposition in MaintenanceDisposition.Validate(payload.GetProperty(FactField.Action).GetString(), null, out MaintenanceDisposition? row) is null
                    ? Fin.Succ(row!)
                    : Fin.Fail<MaintenanceDisposition>(new KernelFault.InvalidValue(Label: ActionSlot, Requirement: "an admitted maintenance disposition key"))
                from done in set.Enabled(ToolAssessments, ToolWear, FitResidual, ToolFloor)
                    ? from assessment in Fin.Succ(InstrumentSet.Tags(TenantContext.Current, (BasisSlot, edge), (ActionSlot, disposition.Key)))
                      from remaining in Fin.Succ(payload.GetProperty(FactField.FractionRemaining).GetDouble())
                      from _ in set.Write(ToolAssessments, 1L, assessment)
                      from _wear in set.Write(ToolWear, remaining, assessment)
                      from _fit in payload.TryGetProperty(FactField.FitResidual, out JsonElement residual)
                          ? set.Write(FitResidual, residual.GetDouble(), InstrumentSet.Tags(TenantContext.Current, (ModelSlot, TaylorModel)))
                          : Fin.Succ(unit)
                      from floor in set.Level(ToolFloor, remaining, Some(edge))
                      select floor
                    : Fin.Succ(unit)
                select done,
            [FactKind.ToolRefresh] = static (set, payload) =>
                set.Write(ToolRefreshAge, payload.GetProperty(FactField.AgeSeconds).GetDouble()),
            [FactKind.CuttingFit] = static (set, payload) =>
                from model in Fin.Succ(InstrumentSet.Tags(TenantContext.Current, (ModelSlot, payload.GetProperty(FactField.Model).GetString())))
                from _ in set.Write(FitResidual, payload.GetProperty(FactField.Residual).GetDouble(), model)
                from done in set.Write(FitQuality, payload.GetProperty(FactField.Determination).GetDouble(), model)
                select done,
            [FactKind.Probe] = static (set, payload) =>
                from features in Fin.Succ(payload.GetProperty(FactField.Features).GetInt64())
                from conforming in Fin.Succ(payload.GetProperty(FactField.Conforming).GetInt64())
                from partition in Fin.Succ(InstrumentSet.Tags(TenantContext.Current))
                from _ in set.Write(ProbeFeatures, conforming, [.. partition, new(VerdictSlot, Pass)])
                from _failed in set.Write(ProbeFeatures, features - conforming, [.. partition, new(VerdictSlot, Fail)])
                from done in set.Write(ProbeDeviation, payload.GetProperty(FactField.WorstDeviationMm).GetDouble(), partition)
                select done,
            [FactKind.Capability] = static (set, payload) =>
                from violations in Fin.Succ(payload.GetProperty(FactField.Violations).GetInt64())
                from partition in Fin.Succ(InstrumentSet.Tags(TenantContext.Current))
                from _ in toSeq(payload.GetProperty(FactField.Rows).EnumerateArray()).TraverseM(row => set.Write(
                    CapabilityIndex, row.GetProperty(FactField.Value).GetDouble(),
                    [.. partition, new(MetricSlot, row.GetProperty(FactField.Metric).GetString())])).As()
                from _studies in set.Write(CapabilityStudies, 1L, [.. partition, new(VerdictSlot, violations == 0L ? Pass : Fail)])
                from done in set.Write(CapabilityViolations, violations, partition)
                select done,
            [FactKind.Removal] = static (set, payload) =>
                from gouges in Fin.Succ(payload.GetProperty(FactField.Gouges).GetInt64())
                from partition in Fin.Succ(InstrumentSet.Tags(TenantContext.Current))
                from _ in set.Write(RemovalVerifications, 1L, [.. partition, new(VerdictSlot, gouges == 0L ? Pass : Fail)])
                from _defects in set.Write(RemovalDefects, gouges, partition)
                from _residual in ResidueColumns.TraverseM(row => set.Write(
                    RemovalResidual, payload.GetProperty(row.Field).GetDouble(),
                    [.. partition, new(ResidueSlot, row.Value)])).As()
                from done in set.Write(RemovalAirCut, payload.GetProperty(FactField.AirCutRatio).GetDouble(), partition)
                select done,
            [FactKind.Cycle] = static (set, payload) =>
                Fin.Succ(InstrumentSet.Tags(TenantContext.Current)).Bind(partition =>
                    set.Write(CycleDuration, payload.GetProperty(FactField.Seconds).GetDouble(), partition)
                        .Bind(_ => set.Write(CycleEnergy, payload.GetProperty(FactField.EnergyKwh).GetDouble(), partition))
                        .Bind(_ => set.Write(CycleDistance, payload.GetProperty(FactField.DistanceMm).GetDouble(), partition))),
            [FactKind.Estimate] = static (set, payload) =>
                from scope in Fin.Succ(InstrumentSet.Tags(TenantContext.Current, (ScopeSlot, payload.GetProperty(FactField.Scope).GetString())))
                from _ in set.Write(EstimateMoney, payload.GetProperty(FactField.Money).GetDouble(),
                    [.. scope, new(CurrencySlot, payload.GetProperty(FactField.Currency).GetString())])
                from _carbon in set.Write(EstimateCarbon, payload.GetProperty(FactField.CarbonKg).GetDouble(), scope)
                from done in set.Write(EstimateClock, payload.GetProperty(FactField.ClockSeconds).GetDouble(),
                    InstrumentSet.Tags(TenantContext.Current, (BackedSlot, payload.GetProperty(FactField.SimulationBacked).GetBoolean() ? Simulation : Fallback)))
                select done,
            [FactKind.FleetMatch] = static (set, payload) =>
                from machine in payload.GetProperty(FactField.Process).GetString() is { } key
                    ? Fin.Succ(key)
                    : Fin.Fail<string>(new KernelFault.InvalidValue(Label: ProcessSlot, Requirement: "a process key on every fleet match"))
                from process in Fin.Succ(InstrumentSet.Tags(TenantContext.Current, (ProcessSlot, machine)))
                from utilization in Fin.Succ(payload.GetProperty(FactField.Utilization).GetDouble())
                from _ in set.Write(FleetMatches, 1L,
                    [.. process, new(EvidenceSlot, payload.GetProperty(FactField.Measured).GetBoolean() ? Measured : Declared)])
                from _utilization in set.Write(FleetUtilization, utilization, process)
                from _effect in set.Write(FleetEffectiveness, payload.GetProperty(FactField.Effectiveness).GetDouble(), process)
                from done in set.Level(FleetLoad, utilization, Some(machine))
                select done,
            [FactKind.Run] = static (set, payload) =>
                from partition in Fin.Succ(InstrumentSet.Tags(TenantContext.Current))
                from _ in set.Write(RunDuration, payload.GetProperty(FactField.Seconds).GetDouble(), [
                    .. partition,
                    new(ProcessSlot, payload.GetProperty(FactField.Process).GetString()),
                    new(VerificationSlot, payload.GetProperty(FactField.Verification).GetString())])
                from _warnings in set.Write(RunWarnings, payload.GetProperty(FactField.Warnings).GetInt64(), partition)
                from done in toSeq(payload.GetProperty(FactField.Kinds).EnumerateArray()).TraverseM(kind => set.Write(
                    RunArtifacts, 1L, [.. partition, new(KindSlot, kind.GetString())])).As()
                select unit,
            [FactKind.QualitySeal] = static (set, payload) =>
                toSeq(payload.GetProperty(FactField.Sustainability).EnumerateArray()).TraverseM(row =>
                    SustainabilityQuantity.Validate(row.GetProperty(FactField.Quantity).GetString(), null, out SustainabilityQuantity? quantity) is null
                        ? set.Write(quantity!.Instrument, row.GetProperty(FactField.Value).GetDouble(),
                            InstrumentSet.Tags(TenantContext.Current, (MeasureSlot, row.GetProperty(FactField.Measure).GetString())))
                        : Fin.Fail<Unit>(new KernelFault.InvalidValue(
                            Label: nameof(SustainabilityQuantity), Requirement: "an admitted sustainability quantity key"))).As()
                    .Map(static _ => unit),
            [FactKind.Traveler] = static (set, payload) =>
                set.Write(TravelerAmendments, payload.GetProperty(FactField.Amendments).GetInt64(), InstrumentSet.Tags(TenantContext.Current)),
            [FactKind.Delivery] = static (set, payload) =>
                set.Write(DeliveryPrograms, 1L, InstrumentSet.Tags(TenantContext.Current,
                    (KindSlot, payload.GetProperty(FactField.ProgramKind).GetString()),
                    (VerdictSlot, payload.GetProperty(FactField.Verified).GetBoolean() ? Verified : Unverified),
                    (ControllerSlot, payload.GetProperty(FactField.Controller).GetString()))),
            [FactKind.Engine] = static (set, payload) =>
                EnginePhase.Validate(payload.GetProperty(FactField.Phase).GetString(), null, out EnginePhase? phase) is not null
                    ? Fin.Fail<Unit>(new KernelFault.InvalidValue(Label: EngineSteps, Requirement: "an admitted engine phase key"))
                    : set.Enabled(EngineSteps)
                    ? set.Write(EngineSteps, payload.GetProperty(FactField.Count).GetInt64(), InstrumentSet.Tags(TenantContext.Current,
                        (SolverSlot, phase!.Solver.Key),
                        (PhaseSlot, phase.Key)))
                    : Fin.Succ(unit),
        }.ToFrozenDictionary(StringComparer.Ordinal);

    public static readonly FrozenDictionary<Type, InstrumentArm<object>> Facts =
        new[] {
            ReceiptFan.Arm<FabricationFact.Engine>(static (set, fact) =>
                set.Enabled(EngineSteps)
                    ? set.Write(EngineSteps, fact.Count, InstrumentSet.Tags(TenantContext.Current,
                        (SolverSlot, fact.Phase.Solver.Key),
                        (PhaseSlot, fact.Phase.Key)))
                    : Fin.Succ(unit)),
            ReceiptFan.Arm<FabricationFact.CuttingFit>(static (set, fact) =>
                from model in Fin.Succ(InstrumentSet.Tags(TenantContext.Current, (ModelSlot, fact.Model)))
                from _ in set.Write(FitResidual, fact.Residual, model)
                from done in set.Write(FitQuality, fact.Determination, model)
                select done),
            ReceiptFan.Arm<FabricationFact.Cycle>(static (set, fact) =>
                Fin.Succ(InstrumentSet.Tags(TenantContext.Current)).Bind(partition =>
                    set.Write(CycleDuration, fact.Seconds, partition)
                        .Bind(_ => set.Write(CycleEnergy, fact.EnergyKwh, partition))
                        .Bind(_ => set.Write(CycleDistance, fact.DistanceMm, partition)))),
        }.ToFrozenDictionary();
}
```

## [05]-[CLASSIFICATION]

- Owner: `FabricationClassified` — the sealed attribute rows binding this folder's classified receipt members to the suite taxonomy by value, and the `Values` roster contributing those same texts upward on the `[03]-[INSTRUMENT_ROSTER]` port.
- Cases: personal · confidential · credential.
- Auto: an annotated member redacts wherever a log or export seam expands it — HMAC for personnel and heat identity so cross-record correlation survives, erase for credential material — and sealed artifact bytes never redact: canonical documents are domain truth, classification governs telemetry egress alone.
- Packages: Microsoft.Extensions.Compliance.Redaction, Rasm (`ClassifiedValue`), LanguageExt.Core, BCL inbox.
- Growth: a newly classified member family is one attribute row binding an existing taxonomy key plus its `ClassifiedValue` row on `Values`, both off one const; a new sensitivity class is a suite-taxonomy decision, never a folder mint.
- Boundary: taxonomy name and row keys are value federation to the suite `DataClassification` vocabulary — the attribute rows carry `(taxonomy, value)` string pairs and no type reference crosses the package boundary, and `Values` STRENGTHENS that law rather than qualifying it: the contribution rides the existing `TelemetryContributorPort` seam as the identical text, so the suite's redaction owner proves this folder's values against its rostered set at boot and an unrostered value refuses at composition instead of reaching the erasing fallback at egress, where a deleted dimension raises nothing and is noticed only when someone misses it; annotated owners are `AttestationPayload.Signer` and `.Credential`, `HeatNumber`, `WelderQualification.Welder`, `TravelerAmendment.Actor`, and `ProgramDelivery.Operator`, each carrying its attribute at the declaring fence; `DataClassificationTypeConverter` string round-tripping stays under its `EXTEXP0002` gate as a declared policy value when a classification ever binds from configuration.

```csharp signature
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

## [06]-[SPANS]

- Owner: `FabricationTrace` — the solver scope roster the composing root admits into the kernel `SpanBand`, the engine-keyed bracket every solver lane opens, and the lane-milestone mark; the band, the listener gate, the typed status verdict, and the source lifetime are the capsule's.
- Entry: `FabricationTrace.Scopes` rides this package's contributor port into the composing root's `SpanBand`; a solver takes the band as a trailing `SpanBand? band = null` parameter beside its `FabricationTap? tap`, brackets its own fold as `band.Traced(engine, key, body)` in the rail shape it already returns, and marks a milestone inside that fold as `FabricationTrace.Mark(span, phase)` — so the bracket is one kernel call and no folder-local activity source, wrapper, or second band owner exists.
- Auto: the scope key derives from the `FabricationEngine` row, so the span source name and the `[04]` solver dimension resolve to one vocabulary and a new solver lane arms its span with no edit here; a milestone spells an `EnginePhase` row, so a span mark and a step counter read the same lane-point roster and no free-text phase string exists; the kernel gates every bracket on `HasListeners`, so an unlistened solve pays one null test and a mark on its null span costs one more.
- Packages: Rasm, LanguageExt.Core, BCL inbox (`System.Diagnostics`).
- Growth: a new traced lane is one `FabricationEngine` row; a new milestone is one `EnginePhase` row.
- Boundary: the band is composition-entered and reaches a solver through the run spine, never a process-static source a domain page reads ambiently — the nullable receiver is what makes a lane holding no band run untraced rather than mint one, so a headless kernel run needs no ambient source and no branch at the call site; `FabricationFact.Engine` stays the receipt truth and the span carries no counter — a metric read off a span is the sampling-dependent duplicate the fan already owns; an unadmitted scope refuses on the kernel rail, so a composition that omits this roster fails at the first bracket rather than silently dropping every solver span; trace-based exemplars join the `[03]` histograms to these spans at the provider, and the meter scope stays `TelemetrySource.Fabrication` — a `TraceScope` and a meter scope are distinct grammars and neither derives from the other.

```csharp signature
public static class FabricationTrace {
    public static readonly ImmutableArray<TraceScope> Scopes =
        [.. FabricationEngine.Items.Select(static row => row.Trace)];

    extension(SpanBand? band) {
        public Fin<T> Traced<T>(FabricationEngine engine, Op key, Func<Activity?, Fin<T>> body) =>
            band is null ? body(null) : band.Traced(engine.Trace, key, body);

        public IO<T> Traced<T>(FabricationEngine engine, Op key, Func<Activity?, IO<T>> body) =>
            band is null ? body(null) : band.Traced(engine.Trace, key, body);
    }

    public static Unit Mark(Activity? span, EnginePhase phase) =>
        ignore(span?.AddEvent(new ActivityEvent(phase.Key)));
}
```

## [07]-[HOOK_RAIL]

- Owner: `FabricationPoint` — the `[SmartEnum<string>]` point vocabulary keyed `rasm.fabrication.<domain>.<point>`, realizing the kernel `IHookRoster<FabricationPoint>` floor with a `CapabilitySet<HookModality>` column and a plane answer; `FabricationHookFact` — the closed spine-payload union realizing `IHookFact<FabricationPoint>`, its `At` column the primary case-to-row correspondence; `FabricationHooks` — the composition entry minting the ONE kernel `HookRail<FabricationPoint, FabricationHookFact, TelemetrySource>`. Seats, veto folding, bounded replay, fork-shielded isolation, detach custody, owner-scoped release, and the bounded `FaultCell` all ride that rail, so this folder mints zero rail mechanism.
- Cases: `rasm.fabrication.run.admission` veto over `FabricationInput` · `rasm.fabrication.derive.stage` observe over `PlannedStep` · `rasm.fabrication.egress.mint` veto over `ContentKey` · `rasm.fabrication.verify.verdict` replay over `FabricationResult.VerificationResult` · `rasm.fabrication.delivery.handoff` observe over `RunEvidence`. Every row admits `HookModality.Observe` beside whatever else it holds, so a plugin watching admission or egress attaches on the same point its veto governs rather than demanding a shadow seat.
- Entry: `FabricationHooks.Live(key, gates, taps, cell)` mints the rail once at composition against the evidence cell the composing app hands it; `rail.Fire(fact.At, fact, key)` is the spine's raise and the guarded arity `rail.Fire(fact.At, fact, key, body)` the one unwrap a veto site takes; `rail.Points` is the census `HookRegistry.Mount` freezes at the app root beside the AppHost rail and the receipt-tap observe row; `rail.Release(TelemetrySource.Fabrication, key)` drops this package's subscriptions alone.
- Auto: `At` is the primary correspondence and its generated total `Map` breaks at compile time on a case with no row, so `Seats` derives rather than mirrors and no spine site names a point; ids derive from each row's own key through one `Items`-built index, so a seat re-spells neither id nor modality; the run spine fires every point — admission before dispatch, egress mint per produced key, stage and verdict off the settled result, hand-off after evidence — so any app observes, vetoes, or replays a run with zero emit calls in domain kernels.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new point is one `FabricationPoint` row, one `FabricationHookFact` case with its `At` arm, and one fire site on the run spine; delivery semantics are the kernel modality rows.
- Law: `FabricationHookFact` and `[02]`'s `FabricationFact` survive as TWO unions because their REGIMES differ, never their subject — a hook fact hands a whole domain aggregate in process to a subscriber that may refuse it, while a metric fact flattens to primitives and crosses the wire under a kind discriminator. Merging them serializes `FabricationInput` onto the receipt rail and hands a veto gate a projection it refuses nothing with.
- Law: `EgressMint` is REFUSAL-ONLY. `ContentKey` addresses its own bytes, so a gate rewriting the admitted key forges an identity nothing produced — the same forgery `[06]-[RUN_DISPATCH]` already rails a lineage cycle as — and the spine proves it at the site by refusing an admitted key differing from the one it fired. `Admission` carries no such law: rewriting the request is what an admission veto exists for, and its site threads the admitted input onward.
- Law: NAMED LOSS from composing the kernel rail — the per-point FACT TYPE. Each named `HookPoint<TFact>` field refused every sibling payload at compile time; under one rail every point shares `FabricationHookFact` and subscribers discriminate on the case. What survives is stronger: `At` fixes the case-to-row pairing at compile time and the kernel gates `Seats` TWICE per fire — at entry and on the veto fold's product — so per-point narrowing moved off five field declarations onto one generated map. WITNESS — the five `HookPoint<TFact>` columns, the five-line `Live`, the five-entry `Points` census, and the private `Seat<TFact>` mint all delete onto `FabricationRail.Of`.
- Boundary: hook scope rides the `FabricationRuntime` instance, so two apps composing the library never share a mutable registry or shadow each other's subscribers; ids obey the four-segment `rasm.<pkg>.<domain>.<point>` grammar `HookId` admission enforces; a subscriber fault parks as `IsolatedFault` on the composition's own bounded cell and the emitter is untouched, the ring shedding oldest-first rather than growing for process lifetime; a veto refusal returns on the run's own rail as the subscriber's typed fault. Spans are absent by design — admitted band scopes are the solver lanes at `[06]`, so `Plane` is `None` on every row, no `TraceScope` derives off these ids, and `Live` binds no `IHookSpan`.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using FabricationGate = Rasm.Domain.HookGate<Rasm.Fabrication.Process.FabricationPoint, Rasm.Fabrication.Process.FabricationHookFact, Rasm.Domain.TelemetrySource>;
using FabricationObserver = Rasm.Domain.HookTap<Rasm.Fabrication.Process.FabricationPoint, Rasm.Fabrication.Process.FabricationHookFact, Rasm.Domain.TelemetrySource>;
using FabricationRail = Rasm.Domain.HookRail<Rasm.Fabrication.Process.FabricationPoint, Rasm.Fabrication.Process.FabricationHookFact, Rasm.Domain.TelemetrySource>;

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
    public static Fin<FabricationRail> Live(
        Op key, Seq<FabricationGate> gates = default, Seq<FabricationObserver> taps = default,
        Option<FaultCell> cell = default) =>
        FabricationRail.Of(key, gates, taps, Option<IHookSpan>.None, cell);
}
```

## [08]-[BOARD_PACK]

- Owner: `FabricationDescriptors` — the shop's one kernel `BoardPack` value binding the panel rows and burn-rate objectives over the `[03]` roster.
- Cases: in-service disposition share · conforming-study share · gouge-free verification share · measured-ranking share · cycle-time ceiling · machine-load headroom · remaining-life headroom.
- Entry: `FabricationDescriptors.Pack` is the whole descriptor surface the AppHost alert rail and the deploy-plane board compile decode — `Panels` and `Objectives` are its columns, `Alerts` derives one `AlertSpec` per objective per burn row through the kernel fold, and `Pack.Admit(roster)` proves every panel instrument, every break key, every widget resolution, and every indicator series against the declaring port's own roster of the required kind; the pack rides `[03]`'s contributor port outward, so the mounting root runs that proof and this folder exposes no second admission entry.
- Auto: a panel naming an instrument alone reads the kernel widget projection for that row's measurement shape, so only a deliberate reading spells a `PanelKind`; the AppHost alert rail and the deploy-plane dashboard compile consume the same pack, so a roster change re-derives verdicts, alerts, and panels in one diff and a hand-authored panel or rule beside it is the drift defect; burn windows, factors, severities, hold, tone, and the budget share all derive from the kernel burn table, and every objective omits its compliance window so kernel admission canonicalizes the one estate default — no threshold, lane, window pair, or calendar literal lands here.
- Packages: Rasm, LanguageExt.Core, NodaTime.
- Growth: a new shop objective is one `Objective` row over an existing indicator shape, and a share over an already-fanned population needs no roster edit at all; a new board panel is one `PanelSpec` on the pack, and a whole passport panel family is one `SustainabilityQuantity` row; a new indicator shape is a kernel `Sli` case breaking every compile leg at once.
- Boundary: indicator, severity, panel, descriptor-row, and burn vocabularies are the kernel capsule's and cross the language boundary as values, never types; a success share is a partition over the ONE counter its outcome dimension already fans — a good-half twin doubles the series the roster mounts and strands its denominator on the next arm edit — while `Ratio` stays reserved for genuinely independent counters and a saturation indicator names one pulled level against a bound, because a load or life reading forms no counter pair; a partition's good set is derived where a vocabulary owns the verdict and named off the axis const where the population itself owns it, never spelled as a value literal; every named series is written by an arm at `[04]` on every occurrence, so no denominator depends on a veto path; the pack's boards and alerts stay descriptor data — query dialect, datasource binding, provisioning, and delivery routing are the deploy plane's.

```csharp signature
public static class FabricationDescriptors {
    public static readonly BoardPack Pack = new(
        Wire: "fabrication.slo",
        Panels: Seq(
            PanelSpec.Of("Tool wear floor", FabricationInstruments.ToolFloor, FabricationInstruments.BasisSlot),
            PanelSpec.Of("Remaining life by basis", FabricationInstruments.ToolWear, FabricationInstruments.BasisSlot),
            PanelSpec.Of("Maintenance actions", FabricationInstruments.ToolAssessments, PanelKind.Table, FabricationInstruments.BasisSlot, FabricationInstruments.ActionSlot),
            PanelSpec.Of("Catalog refresh age", FabricationInstruments.ToolRefreshAge),
            PanelSpec.Of("Fit residual by model", FabricationInstruments.FitResidual, FabricationInstruments.ModelSlot),
            PanelSpec.Of("Inspection verdicts", FabricationInstruments.ProbeFeatures, FabricationInstruments.VerdictSlot),
            PanelSpec.Of("Worst probe deviation", FabricationInstruments.ProbeDeviation),
            PanelSpec.Of("Capability index", FabricationInstruments.CapabilityIndex, FabricationInstruments.MetricSlot),
            PanelSpec.Of("SPC violations", FabricationInstruments.CapabilityViolations),
            PanelSpec.Of("Study conformance", FabricationInstruments.CapabilityStudies, FabricationInstruments.VerdictSlot),
            PanelSpec.Of("Gouge findings", FabricationInstruments.RemovalDefects),
            PanelSpec.Of("Verification verdicts", FabricationInstruments.RemovalVerifications, FabricationInstruments.VerdictSlot),
            PanelSpec.Of("Residual volume", FabricationInstruments.RemovalResidual, FabricationInstruments.ResidueSlot),
            PanelSpec.Of("Air-cut fraction", FabricationInstruments.RemovalAirCut),
            PanelSpec.Of("Cycle time", FabricationInstruments.CycleDuration),
            PanelSpec.Of("Cycle energy", FabricationInstruments.CycleEnergy, PanelKind.Timeseries),
            PanelSpec.Of("Estimate ledger", FabricationInstruments.EstimateMoney, PanelKind.Table, FabricationInstruments.ScopeSlot, FabricationInstruments.CurrencySlot),
            PanelSpec.Of("Fleet load", FabricationInstruments.FleetLoad, FabricationInstruments.ProcessSlot),
            PanelSpec.Of("Match utilization", FabricationInstruments.FleetUtilization, FabricationInstruments.ProcessSlot),
            PanelSpec.Of("Match ranking evidence", FabricationInstruments.FleetMatches, FabricationInstruments.ProcessSlot, FabricationInstruments.EvidenceSlot),
            PanelSpec.Of("Run duration", FabricationInstruments.RunDuration, FabricationInstruments.ProcessSlot, FabricationInstruments.VerificationSlot),
            PanelSpec.Of("Artifacts by egress kind", FabricationInstruments.RunArtifacts, FabricationInstruments.KindSlot),
            PanelSpec.Of("Run warnings", FabricationInstruments.RunWarnings),
            PanelSpec.Of("Delivery custody", FabricationInstruments.DeliveryPrograms, PanelKind.Table, FabricationInstruments.VerdictSlot, FabricationInstruments.ControllerSlot),
            PanelSpec.Of("Solver steps", FabricationInstruments.EngineSteps, FabricationInstruments.SolverSlot, FabricationInstruments.PhaseSlot))
            + toSeq(SustainabilityQuantity.Items).Map(static row =>
                PanelSpec.Of($"Passport {row.Key}", row.Instrument, PanelKind.Table, FabricationInstruments.MeasureSlot)),
        Objectives: Seq(
            Objective.Create(
                name: "fabrication.wear.serviceable",
                sli: new Sli.Partition(
                    Metric: FabricationInstruments.ToolAssessments,
                    By: FabricationInstruments.ActionSlot,
                    Good: MaintenanceDisposition.ServiceableKeys),
                target: 0.99d,
                window: default),
            Objective.Create(
                name: "fabrication.capability.capable",
                sli: new Sli.Partition(
                    Metric: FabricationInstruments.CapabilityStudies,
                    By: FabricationInstruments.VerdictSlot,
                    Good: Seq(FabricationInstruments.Pass)),
                target: 0.995d,
                window: default),
            Objective.Create(
                name: "fabrication.removal.clean",
                sli: new Sli.Partition(
                    Metric: FabricationInstruments.RemovalVerifications,
                    By: FabricationInstruments.VerdictSlot,
                    Good: Seq(FabricationInstruments.Pass)),
                target: 0.999d,
                window: default),
            Objective.Create(
                name: "fabrication.fleet.measured",
                sli: new Sli.Partition(
                    Metric: FabricationInstruments.FleetMatches,
                    By: FabricationInstruments.EvidenceSlot,
                    Good: Seq(FabricationInstruments.Measured)),
                target: 0.95d,
                window: default),
            Objective.Create(
                name: "fabrication.cycle.envelope",
                sli: new Sli.Latency(Metric: FabricationInstruments.CycleDuration, Ceiling: Duration.FromHours(4), Quantile: 0.95d),
                target: 0.95d,
                window: default),
            Objective.Create(
                name: "fabrication.fleet.headroom",
                sli: new Sli.Saturation(Metric: FabricationInstruments.FleetLoad, Bound: 0.85d, Breach: LevelBreach.Ceiling),
                target: 0.9d,
                window: default),
            Objective.Create(
                name: "fabrication.tool.headroom",
                sli: new Sli.Saturation(Metric: FabricationInstruments.ToolFloor, Bound: 0.15d, Breach: LevelBreach.Floor),
                target: 0.95d,
                window: default)));
}
```

## [09]-[RESEARCH]

(none)
