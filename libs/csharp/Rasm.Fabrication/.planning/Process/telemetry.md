# [FABRICATION_TELEMETRY]

`FabricationFact` is the package's one fact vocabulary for measured production: every operational metric is a projection of a settled domain receipt flattened onto this union, and the instrument roster, the contributor port, the projection arms, and the classification rows all derive from it — a metric minted beside the fan is a second truth. Domain kernels stay pure; facts fire only where receipts settle on the run spine through `FabricationRuntime`'s one `FabricationTap` port.

Settled composition: `ReceiptSinkPort`, `ReceiptEnvelope`, and `TenantContext` arrive from the AppHost port vocabulary; `HookPoint<TFact>`, `HookId`, `HookModality`, `IHookPoint`, `HookRegistry`, `IsolatedFault`, `InstrumentRow`, `InstrumentSet`, `InstrumentArm`, `LevelCells`, `Buckets`, and `TelemetryContributorPort` from the kernel signal capsule — instruments mount through the composing root's meter mint, the contributed arm table merges onto the AppHost receipt fan at the same root, hook points mount through `HookRegistry.Mount` beside them, and the folder holds no OpenTelemetry reference, exporter, or provider.

## [01]-[INDEX]

- [02]-[FACT_UNION]: `FabricationFact` kind-keyed union, per-receipt `Of` projections, wire context, tap port, and sink-bound emission.
- [03]-[INSTRUMENT_ROSTER]: `rasm.fabrication.*` `InstrumentRow` roster, bucket advice, level cells, and the contributor port.
- [04]-[FACT_PROJECTION]: Kind-keyed projection arms from the receipt envelope onto mounted instruments.
- [05]-[CLASSIFICATION]: Suite-taxonomy attribute rows for the classified receipt members.
- [06]-[SPANS]: blocked solve-span route pending exact diagnostics catalog rows.
- [07]-[HOOK_ROSTER]: `rasm.fabrication.<domain>.<point>` hook points the run spine fires.
- [08]-[SLO_ROWS]: Burn-rate objectives derived from the instrument roster.

## [02]-[FACT_UNION]

- Owner: `FabricationFact` — the closed fact union; `FabricationWireContext` — the Strict serializer context whose polymorphism metadata is the one kind registry; `FabricationTap` — the runtime emission port; `FabricationSurface` — the sink-bound emission seam.
- Cases: tool-wear · tool-refresh · cutting-fit · probe · capability · removal · cycle · estimate · fleet-match · run · quality-seal · traveler · delivery · engine.
- Entry: `FabricationTap.Fire(FabricationFact fact)` — the sole in-package emission verb; `FabricationSurface.Emit(CorrelationId correlation, FabricationFact fact)` binds sink and serializer once at composition and the app root wires the tap onto it, so `FabricationTap.Silent` keeps a headless kernel run emitting into unit with zero branching; `Fire` collapses a subscriber failure through `Try`, so a tap fault never re-enters the emitting fold.
- Auto: each `Of` projection flattens its receipt to measures and bounded dimensions at the fact boundary — a smart-enum spine value crosses as its key scalar, a NodaTime span as seconds — so the wire context serializes primitives only; wire kind derives from the polymorphic metadata pinned on the union, and ambient `TenantContext.Current` threads into `Send` so the envelope tenant field partitions evidence; `ToolWear.Of` yields `None` on a receipt without a critical state and `ToolRefresh.Of` on a provider-digest source, so non-measured admissions project nothing rather than fabricate zeros.
- Receipt: none — the union projects settled receipts. `Probe` mints at the datum-result fold because its pre-egress report is file-scoped there; every other case mints through its `Of` row here. `Removal.Of` consumes the public verification result, `Delivery.Of` consumes the settled program-delivery receipt, and each `Engine.Of` row consumes its solver receipt.
- Packages: Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new measured concern is one case row, one `[JsonDerivedType]` registration, one `Of` projection, one roster row at `[03]`, and one projection arm at `[04]` — zero new surface; a case whose receipt gains a measure widens that case, never a sibling.
- Boundary: fact cases carry no `ContentKey`, no personnel or heat identity, and no free-text detail — the receipt rail owns identity and the classification rows at `[05]` bar the classified members structurally; the `[JsonDerivedType]` kind column is the canonical spelling the envelope carries to the sink rail, so a kind outside this roster is receipt-only by declaration.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ToolWear), "tool-wear")]
[JsonDerivedType(typeof(ToolRefresh), "tool-refresh")]
[JsonDerivedType(typeof(CuttingFit), "cutting-fit")]
[JsonDerivedType(typeof(Probe), "probe")]
[JsonDerivedType(typeof(Capability), "capability")]
[JsonDerivedType(typeof(Removal), "removal")]
[JsonDerivedType(typeof(Cycle), "cycle")]
[JsonDerivedType(typeof(Estimate), "estimate")]
[JsonDerivedType(typeof(FleetMatch), "fleet-match")]
[JsonDerivedType(typeof(Run), "run")]
[JsonDerivedType(typeof(QualitySeal), "quality-seal")]
[JsonDerivedType(typeof(Traveler), "traveler")]
[JsonDerivedType(typeof(Delivery), "delivery")]
[JsonDerivedType(typeof(Engine), "engine")]
public abstract partial record FabricationFact {
    private FabricationFact() { }

    public sealed record ToolWear(string Basis, double FractionRemaining, double ConservativeRemaining, string Action, double FitResidual) : FabricationFact {
        public static Option<ToolWear> Of(WearReceipt receipt) =>
            receipt.Critical.Map(critical => new ToolWear(
                critical.Basis.Key,
                critical.FractionRemaining,
                critical.ConservativeRemaining,
                ActionKey(receipt.Action),
                receipt.Diagnostics.Map(static row => row.RootMeanSquareResidual).Fold(0.0, Math.Max)));

        static string ActionKey(MaintenanceAction action) => action.Switch(
            @continue: static _ => "continue",
            monitor: static _ => "monitor",
            inspect: static _ => "inspect",
            rotate: static _ => "rotate",
            replace: static _ => "replace",
            recondition: static _ => "recondition",
            retire: static _ => "retire",
            notApplicable: static _ => "not-applicable");
    }

    public sealed record ToolRefresh(double AgeSeconds) : FabricationFact {
        public static Option<ToolRefresh> Of(CatalogReceipt receipt) =>
            receipt.Source is CatalogSource.Telemetry telemetry
                ? Some(new ToolRefresh((receipt.ObservedAt - telemetry.Previous).TotalSeconds))
                : None;
    }

    public sealed record CuttingFit(string Model, double Residual, double Determination) : FabricationFact {
        public static CuttingFit Of(string model, PowerLawReceipt fit) => new(model, fit.RootMeanSquareResidual, fit.RSquared);
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
        public static Cycle Of(SimulationReceipt receipt) => new(receipt.Cycle.TotalSeconds, receipt.EnergyKwh, receipt.DistanceMm);
    }

    public sealed record Estimate(string Scope, string Currency, double Money, double CarbonKg, double ClockSeconds, bool SimulationBacked) : FabricationFact {
        public static Estimate Of(EstimateReceipt receipt) => receipt.Switch(
            unit: static value => new Estimate(
                "unit", value.Receipt.Currency.ToString(), (double)value.Receipt.MoneyTotal,
                value.Receipt.CarbonTotalKgCo2e,
                value.Receipt.MachineTime.TotalSeconds, value.Receipt.SimulationBacked),
            lot: static value => new Estimate(
                "lot", value.Receipt.Unit.Currency.ToString(), (double)value.Receipt.QuotedTotal,
                value.Receipt.CarbonTotalKgCo2e,
                value.Receipt.Unit.MachineTime.TotalSeconds, value.Receipt.Unit.SimulationBacked));
    }

    public sealed record FleetMatch(string Process, double Utilization, double Score, double Effectiveness, bool Measured) : FabricationFact {
        public static FleetMatch Of(MachineMatch match, bool measured) =>
            new(match.Process.Key, match.Utilization, match.Score, match.Effectiveness, measured);
    }

    public sealed record Run(string Process, double Seconds, Seq<string> Kinds, int Produced, int Warnings, string Verification) : FabricationFact {
        // Kinds and Produced both derive from the produced content keys, so the artifact counter and the
        // roster row's produced-by-egress-kind declaration read one vocabulary; requested kinds stay
        // receipt evidence on `RunEvidence.Request`.
        public static Run Of(RunEvidence evidence, Duration elapsed) =>
            new(evidence.Process.Key,
                elapsed.TotalSeconds,
                evidence.Produced.Map(static key => key.Kind.Key),
                evidence.Produced.Count,
                evidence.Warnings.Count,
                evidence.Verified.Match(Some: static pass => pass ? "verified" : "failed", None: static () => "unverified"));
    }

    public sealed record QualitySeal(int Declarations, double EnergyJoules, double CarbonKg, double WasteKg, double WaterLiters) : FabricationFact {
        public static QualitySeal Of(PassportEvidence passport) =>
            new(passport.Declarations.Count,
                passport.Sustainability.Choose(static row => row is SustainabilityEvidence.EnergyUse energy ? Some(energy.Value.Joules) : None).Sum(),
                passport.Sustainability.Choose(static row => row is SustainabilityEvidence.Carbon carbon ? Some(carbon.Value.Kilograms) : None).Sum(),
                passport.Sustainability.Choose(static row => row is SustainabilityEvidence.Waste waste ? Some(waste.Value.Kilograms) : None).Sum(),
                passport.Sustainability.Choose(static row => row is SustainabilityEvidence.WaterUse water ? Some(water.Value.Liters) : None).Sum());
    }

    public sealed record Traveler(int Amendments, int Produced) : FabricationFact {
        public static Traveler Of(TravelerArtifact artifact) => new(artifact.Amendments.Count, artifact.Produced.Count);
    }

    // Operator attestation stays on the receipt under its classification attribute; the fact carries the
    // chain-of-custody verdict and controller identity alone.
    public sealed record Delivery(string ProgramKind, bool Verified, string Controller, string Acknowledged, int Records) : FabricationFact {
        public static Delivery Of(ProgramDelivery delivery) => new(
            delivery.Image.Kind.Key,
            delivery.Verified,
            delivery.Controller,
            delivery.Acknowledged.Key,
            delivery.Records);
    }

    // One case owns every solver-internal counter; input shape selects the projection, and a new solver
    // lane is one `Of` row over the receipt evidence its kernel already accumulates — never a
    // per-iteration write inside an allocation-free fold.
    public sealed record Engine(string Solver, string Phase, long Count) : FabricationFact {
        public static Seq<Engine> Of(NestEvidence evidence) => Seq(
            new Engine("nest", "candidates", evidence.Candidates),
            new Engine("nest", "evaluated", evidence.Evaluated),
            new Engine("nest", "rejected", evidence.Rejected),
            new Engine("nest", "memo-hits", evidence.MemoHits),
            new Engine("nest", "memo-misses", evidence.MemoMisses));

        public static Seq<Engine> Of(SkeletonReceipt receipt) => Seq(
            new Engine("skeleton", "nodes", receipt.NodeCount),
            new Engine("skeleton", "arcs", receipt.ArcCount),
            new Engine("skeleton", "passes", receipt.Passes.Count));

        public static Seq<Engine> Of(SetupSchedule schedule) =>
            Seq(new Engine("setup", "decisions", schedule.Decisions.Count));

        public static Seq<Engine> Of(ScanReceipt receipt) => Seq(
            new Engine("scan", "exposures", receipt.Exposures),
            new Engine("scan", "jumps", receipt.Jumps),
            new Engine("scan", "remelts", receipt.Remelts),
            new Engine("scan", "stitches", receipt.Stitches));

        public static Seq<Engine> Of(AlignmentReceipt receipt) =>
            Seq(new Engine("probe", "icp-iterations", receipt.Iterations));

        public static Seq<Engine> Of(BendSequenceReceipt receipt) => Seq(
            new Engine("form", "expansions", receipt.Expansions),
            new Engine("form", "rejections", receipt.Rejected));
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true,
    Converters = [typeof(ThinktectureJsonConverterFactory)])]
[JsonSerializable(typeof(FabricationFact))]
public partial class FabricationWireContext : JsonSerializerContext;

public sealed record FabricationTap(Func<FabricationFact, Unit> Send) {
    public static readonly FabricationTap Silent = new(static _ => unit);

    public Unit Fire(FabricationFact fact) => Try.lift(() => Send(fact)).Run().IfFail(static _ => unit);
}

public sealed class FabricationSurface(ReceiptSinkPort sink, FabricationWireContext wire) {
    public IO<ReceiptEnvelope> Emit(CorrelationId correlation, FabricationFact fact) =>
        IO.lift(() => JsonSerializer.SerializeToElement(fact, wire.FabricationFact))
            .Bind(payload => sink.Send(correlation, TenantContext.Current, TelemetrySource.Fabrication.Key, payload.GetProperty("kind").GetString()!, payload));
}
```

## [03]-[INSTRUMENT_ROSTER]

- Owner: `FabricationInstruments` — the Fabrication `InstrumentRow` roster and the `TelemetryContributorPort` mint; level rows read the composition's `LevelCells`.
- Entry: `FabricationInstruments.Telemetry(LevelCells cells, string version, string schemaUrl)` — the one contributor port (scope `Rasm.Fabrication`) carrying the domain row set and its semconv schema coordinate into the composing root at composition; the mint stamps the coordinate as `MeterOptions.TelemetrySchemaUrl`, so every `rasm.fabrication.*` scope reads with pinned semantics.
- Auto: every histogram row ships its advice boundaries at creation through the kernel `Buckets` rows (`Fractions`, `Millimeters`, `CycleSeconds`, `RefreshSeconds`), the fallback a backend without exponential histograms reads — the default aggregation stays base2 exponential at the provider; registry mount de-duplicates by name, so a duplicate row is a composition fault, never a forked stream; the projection arms at `[04]-[FACT_PROJECTION]` write the level cells, so each gauge reads a current level, never a re-derived scan.
- Packages: Rasm, LanguageExt.Core, BCL inbox.
- Growth: one measured concern is one `InstrumentRow` here and one projection arm at `[04]-[FACT_PROJECTION]`; a per-kind family derives from its owning vocabulary, never hand-enumerated rows; a new level is one `cells.Level` write at its producing arm and one `Reader`-bound gauge row.
- Boundary: instrument names are dotted `rasm.fabrication.<domain>.<measure>` with UCUM units, never pre-baked `_total` or unit suffixes; the port's `Scope` string is the version-stamped package id the composing root admits by name; facts are event-shaped and ride counters and histograms, while the two level-shaped measures ride observable-gauge rows reading the composition's cells at collection cadence; tag keys per instrument are the closed set the projection arms write (`basis`, `action`, `model`, `verdict`, `metric`, `kind`, `scope`, `currency`, `backed`, `process`, `verification`, `solver`, `phase`, `controller`, `measured`), and cardinality caps ride the app root's view rows, never call-site gating.

```csharp signature
public static partial class FabricationInstruments {
    public const string Scope = "Rasm.Fabrication";

    public static Seq<InstrumentRow> Rows(LevelCells cells) => Seq(
        new InstrumentRow("rasm.fabrication.tool.wear", "1", "remaining-life fraction at the critical wear state",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.Fractions)),
        new InstrumentRow("rasm.fabrication.tool.refresh.age", "s", "interval between successive telemetry catalog refreshes",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.RefreshSeconds)),
        new InstrumentRow("rasm.fabrication.fit.residual", "1", "RMS residual of the wear and machinability power-law fits",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.fit.quality", "1", "coefficient of determination of the machinability fit",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.probe.features", "{feature}", "inspected features by conformance verdict",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.probe.deviation", "mm", "worst absolute measured deviation per inspection",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.Millimeters)),
        new InstrumentRow("rasm.fabrication.capability.index", "1", "capability and performance index values by metric row",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.capability.violations", "{violation}", "SPC rule violations per study",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.removal.defects", "{finding}", "gouge findings per material-removal verification",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.removal.residual", "mm3", "uncut and overcut voxel volume per verification",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.removal.aircut", "1", "air-cut fraction of swept program motion per verification",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.Fractions)),
        new InstrumentRow("rasm.fabrication.cycle.duration", "s", "simulated modal cycle time per program",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.CycleSeconds)),
        new InstrumentRow("rasm.fabrication.cycle.energy", "kW.h", "simulated machine energy per program",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.cycle.distance", "mm", "simulated cutting-motion path length per program",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.estimate.money", "1", "signed money ledger total in receipt currency",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.estimate.carbon", "kg", "carbon ledger total as kilograms CO2-equivalent",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.estimate.clock", "s", "estimated machine clock per subject",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.CycleSeconds)),
        new InstrumentRow("rasm.fabrication.fleet.utilization", "1", "machine load factor at match assessment",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.Fractions)),
        new InstrumentRow("rasm.fabrication.fleet.effectiveness", "1", "machine effectiveness fraction at match assessment",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.Fractions)),
        new InstrumentRow("rasm.fabrication.fleet.stale", "{match}", "matches ranked on nameplate after the freshness fallback",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.run.duration", "s", "fabrication run wall duration",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.CycleSeconds)),
        new InstrumentRow("rasm.fabrication.run.artifacts", "{artifact}", "content-keyed artifacts produced by egress kind",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.run.warnings", "{warning}", "run warnings accumulated on the evidence receipt",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.sustainability.energy", "J", "sealed passport energy-use evidence",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.sustainability.carbon", "kg", "sealed passport embodied-carbon evidence",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.sustainability.waste", "kg", "sealed passport waste-mass evidence",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.sustainability.water", "L", "sealed passport water-use evidence",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.traveler.amendments", "{amendment}", "as-run amendment chain length at traveler seal",
            static (meter, name, unit, text) => meter.CreateHistogram<long>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.delivery.programs", "{program}", "posted programs delivered to controllers by custody verdict",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.engine.steps", "{step}", "solver-internal step counts by solver and phase",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.fabrication.fleet.load", "1", "latest machine load factor at match assessment",
            (meter, name, unit, text) => meter.CreateObservableGauge(
                name,
                () => new System.Diagnostics.Metrics.Measurement<double>(
                    cells.Reader(name)(),
                    new KeyValuePair<string, object?>("measured", cells.Reader($"{name}.measured")() > 0.0)),
                unit,
                text)),
        new InstrumentRow("rasm.fabrication.wear.floor", "1", "latest remaining-life fraction observed at the critical wear state",
            (meter, name, unit, text) => meter.CreateObservableGauge(name, cells.Reader(name), unit, text)));

    public static TelemetryContributorPort Telemetry(LevelCells cells, string version, string schemaUrl) =>
        new(Scope, version, schemaUrl, Rows(cells));
}
```

## [04]-[FACT_PROJECTION]

- Owner: `FabricationInstruments.Arms` — the contributed kind-arm table over the Fabrication kind registry, the roster name the AppHost `[CONTRIBUTED_ARMS]` contributor table mounts.
- Entry: `Arms` enters `InstrumentFan.Mount` as one contributed element beside the Persistence `StoreInstruments.Arms` precedent and merges onto `InstrumentSet.Arms`, so `InstrumentFan.Project` folds every envelope the sink emits into instrument writes with zero call-site metering; a duplicate kind across any two tables faults at the frozen merge.
- Auto: dimension values ride the payload's own key-scalar fields, so tag vocabularies stay bounded by the union's admission; a kind without a table row stays wire-only by declaration, and a fact field without an arm write stays wire evidence — `ConservativeRemaining` carries basis-keyed units one UCUM histogram cannot hold, `Score` is objective-relative, and the `Produced` and `Declarations` counts derive in the envelope store; the tool-wear and fleet-match arms also write the composition level cells the `[03]` gauges bind, so a level is current at every collection.
- Packages: LanguageExt.Core, BCL inbox.
- Growth: a new projected kind is one table row here and its instrument row at `[03]-[INSTRUMENT_ROSTER]`.
- Boundary: arm bodies are the one place fact wire names meet instrument writes — the platform-forced statement seam — and an arm never re-validates the payload its typed fact already admitted; arm execution rides the receipt-tap subscription `InstrumentFan.Tap` mounts on the AppHost hook rail, so a fan failure is that rail's shielded fault and never re-enters the emitting fold.

```csharp signature
public static partial class FabricationInstruments {
    public static readonly FrozenDictionary<string, InstrumentArm> Arms =
        new Dictionary<string, InstrumentArm> {
            ["tool-wear"] = static (set, cells, payload) => {
                var tags = (KeyValuePair<string, object?>[])[new("basis", payload.GetProperty("basis").GetString()), new("action", payload.GetProperty("action").GetString())];
                ignore(set.Record("rasm.fabrication.tool.wear", payload.GetProperty("fractionRemaining").GetDouble(), tags));
                ignore(set.Record("rasm.fabrication.fit.residual", payload.GetProperty("fitResidual").GetDouble(), new KeyValuePair<string, object?>("model", "taylor")));
                ignore(cells.Level("rasm.fabrication.wear.floor", payload.GetProperty("fractionRemaining").GetDouble()));
            },
            ["tool-refresh"] = static (set, _, payload) =>
                ignore(set.Record("rasm.fabrication.tool.refresh.age", payload.GetProperty("ageSeconds").GetDouble())),
            ["cutting-fit"] = static (set, _, payload) => {
                var tags = (KeyValuePair<string, object?>[])[new("model", payload.GetProperty("model").GetString())];
                ignore(set.Record("rasm.fabrication.fit.residual", payload.GetProperty("residual").GetDouble(), tags));
                ignore(set.Record("rasm.fabrication.fit.quality", payload.GetProperty("determination").GetDouble(), tags));
            },
            ["probe"] = static (set, _, payload) => {
                long features = payload.GetProperty("features").GetInt64();
                long conforming = payload.GetProperty("conforming").GetInt64();
                ignore(set.Count("rasm.fabrication.probe.features", conforming, new KeyValuePair<string, object?>("verdict", "pass")));
                ignore(set.Count("rasm.fabrication.probe.features", features - conforming, new KeyValuePair<string, object?>("verdict", "fail")));
                ignore(set.Record("rasm.fabrication.probe.deviation", payload.GetProperty("worstDeviationMm").GetDouble()));
            },
            ["capability"] = static (set, _, payload) => {
                foreach (var row in payload.GetProperty("rows").EnumerateArray()) {
                    ignore(set.Record("rasm.fabrication.capability.index", row.GetProperty("value").GetDouble(),
                        new KeyValuePair<string, object?>("metric", row.GetProperty("metric").GetString())));
                }
                ignore(set.Count("rasm.fabrication.capability.violations", payload.GetProperty("violations").GetInt64()));
            },
            ["removal"] = static (set, _, payload) => {
                ignore(set.Count("rasm.fabrication.removal.defects", payload.GetProperty("gouges").GetInt64(), new KeyValuePair<string, object?>("kind", "gouge")));
                ignore(set.Record("rasm.fabrication.removal.residual", payload.GetProperty("uncutMm3").GetDouble(), new KeyValuePair<string, object?>("kind", "uncut")));
                ignore(set.Record("rasm.fabrication.removal.residual", payload.GetProperty("overcutMm3").GetDouble(), new KeyValuePair<string, object?>("kind", "overcut")));
                ignore(set.Record("rasm.fabrication.removal.aircut", payload.GetProperty("airCutRatio").GetDouble()));
            },
            ["cycle"] = static (set, _, payload) => {
                ignore(set.Record("rasm.fabrication.cycle.duration", payload.GetProperty("seconds").GetDouble()));
                ignore(set.Record("rasm.fabrication.cycle.energy", payload.GetProperty("energyKwh").GetDouble()));
                ignore(set.Record("rasm.fabrication.cycle.distance", payload.GetProperty("distanceMm").GetDouble()));
            },
            ["estimate"] = static (set, _, payload) => {
                var scope = new KeyValuePair<string, object?>("scope", payload.GetProperty("scope").GetString());
                ignore(set.Record("rasm.fabrication.estimate.money", payload.GetProperty("money").GetDouble(), scope,
                    new KeyValuePair<string, object?>("currency", payload.GetProperty("currency").GetString())));
                ignore(set.Record("rasm.fabrication.estimate.carbon", payload.GetProperty("carbonKg").GetDouble(), scope));
                ignore(set.Record("rasm.fabrication.estimate.clock", payload.GetProperty("clockSeconds").GetDouble(),
                    new KeyValuePair<string, object?>("backed", payload.GetProperty("simulationBacked").GetBoolean() ? "simulation" : "fallback")));
            },
            ["fleet-match"] = static (set, cells, payload) => {
                bool measured = payload.GetProperty("measured").GetBoolean();
                var process = new KeyValuePair<string, object?>("process", payload.GetProperty("process").GetString());
                var source = new KeyValuePair<string, object?>("measured", measured);
                ignore(set.Record("rasm.fabrication.fleet.utilization", payload.GetProperty("utilization").GetDouble(),
                    process, source));
                ignore(set.Record("rasm.fabrication.fleet.effectiveness", payload.GetProperty("effectiveness").GetDouble(),
                    process, source));
                ignore(cells.Level("rasm.fabrication.fleet.load", payload.GetProperty("utilization").GetDouble()));
                ignore(cells.Level("rasm.fabrication.fleet.load.measured", measured ? 1.0 : 0.0));
                if (!measured)
                    ignore(set.Count("rasm.fabrication.fleet.stale", 1L, new KeyValuePair<string, object?>("measured", false)));
            },
            ["run"] = static (set, _, payload) => {
                ignore(set.Record("rasm.fabrication.run.duration", payload.GetProperty("seconds").GetDouble(),
                    new KeyValuePair<string, object?>("process", payload.GetProperty("process").GetString()),
                    new KeyValuePair<string, object?>("verification", payload.GetProperty("verification").GetString())));
                foreach (var kind in payload.GetProperty("kinds").EnumerateArray()) {
                    ignore(set.Count("rasm.fabrication.run.artifacts", 1L, new KeyValuePair<string, object?>("kind", kind.GetString())));
                }
                ignore(set.Count("rasm.fabrication.run.warnings", payload.GetProperty("warnings").GetInt64()));
            },
            ["quality-seal"] = static (set, _, payload) => {
                ignore(set.Record("rasm.fabrication.sustainability.energy", payload.GetProperty("energyJoules").GetDouble()));
                ignore(set.Record("rasm.fabrication.sustainability.carbon", payload.GetProperty("carbonKg").GetDouble()));
                ignore(set.Record("rasm.fabrication.sustainability.waste", payload.GetProperty("wasteKg").GetDouble()));
                ignore(set.Record("rasm.fabrication.sustainability.water", payload.GetProperty("waterLiters").GetDouble()));
            },
            ["traveler"] = static (set, _, payload) =>
                ignore(set.Record("rasm.fabrication.traveler.amendments", payload.GetProperty("amendments").GetInt64())),
            ["delivery"] = static (set, _, payload) =>
                ignore(set.Count("rasm.fabrication.delivery.programs", 1L,
                    new KeyValuePair<string, object?>("kind", payload.GetProperty("programKind").GetString()),
                    new KeyValuePair<string, object?>("verdict", payload.GetProperty("verified").GetBoolean() ? "verified" : "unverified"),
                    new KeyValuePair<string, object?>("controller", payload.GetProperty("controller").GetString()))),
            ["engine"] = static (set, _, payload) =>
                ignore(set.Count("rasm.fabrication.engine.steps", payload.GetProperty("count").GetInt64(),
                    new KeyValuePair<string, object?>("solver", payload.GetProperty("solver").GetString()),
                    new KeyValuePair<string, object?>("phase", payload.GetProperty("phase").GetString()))),
        }.ToFrozenDictionary(StringComparer.Ordinal);
}
```

## [05]-[CLASSIFICATION]

- Owner: `FabricationClassified` — the sealed attribute rows binding this folder's classified receipt members to the suite taxonomy by value.
- Cases: personal · confidential · credential.
- Auto: an annotated member redacts wherever a log or export seam expands it — HMAC for personnel and heat identity so cross-record correlation survives, erase for credential material — and sealed artifact bytes never redact: canonical documents are domain truth, classification governs telemetry egress alone.
- Packages: Microsoft.Extensions.Compliance.Redaction, BCL inbox.
- Growth: a newly classified member family is one attribute row binding an existing taxonomy key; a new sensitivity class is a suite-taxonomy decision, never a folder mint.
- Boundary: taxonomy name and row keys are value federation to the suite `DataClassification` vocabulary — the attribute rows carry `(taxonomy, value)` string pairs and no type reference crosses the package boundary; annotated owners are `AttestationPayload.Signer` and `.Credential`, `HeatNumber`, `WelderQualification.Welder`, `TravelerAmendment.Actor`, and `ProgramDelivery.Operator`, each carrying its attribute at the declaring fence; `DataClassificationTypeConverter` string round-tripping stays under its `EXTEXP0002` gate as a declared policy value when a classification ever binds from configuration.

```csharp signature
public static class FabricationClassified {
    const string SuiteTaxonomy = "DataClassification";

    public static readonly DataClassification Personal = new(SuiteTaxonomy, "personal");
    public static readonly DataClassification Confidential = new(SuiteTaxonomy, "confidential");
    public static readonly DataClassification Credential = new(SuiteTaxonomy, "credential");
}

public sealed class PersonalDataAttribute() : DataClassificationAttribute(FabricationClassified.Personal);

public sealed class ConfidentialDataAttribute() : DataClassificationAttribute(FabricationClassified.Confidential);

public sealed class CredentialDataAttribute() : DataClassificationAttribute(FabricationClassified.Credential);
```

## [06]-[SPANS]

Solve spans remain outside settled fence code until the language-root diagnostics catalog admits every member the rail requires. `FabricationFact.Engine` remains the receipt truth, and no no-op bracket may claim tracing while that adapter is blocked.

## [07]-[HOOK_ROSTER]

- Owner: `FabricationHooks` — the typed hook-point roster over the run spine, one `HookPoint<TFact>` per point with its modality and payload closed at declaration.
- Cases: `rasm.fabrication.run.admission` (veto over `FabricationInput`) · `rasm.fabrication.derive.stage` (observe over `PlannedStep`) · `rasm.fabrication.egress.mint` (veto over `ContentKey`) · `rasm.fabrication.verify.verdict` (replay over `FabricationResult.VerificationResult`) · `rasm.fabrication.delivery.handoff` (observe over `RunEvidence`).
- Entry: `FabricationHooks.Live()` mints the roster; `Points` hands the point set to `HookRegistry.Mount` at the app root beside the AppHost rail and the receipt-tap observe row.
- Auto: every point fires from the run spine — admission before dispatch, egress mint per produced key, stage and verdict off the settled result, hand-off after evidence — so any app observes, vetoes, or replays a run with zero emit calls in domain kernels; subscriber isolation, veto short-circuit, and the bounded replay buffer are the kernel capsule law, faults parked on the roster cell.
- Packages: LanguageExt.Core, BCL inbox.
- Growth: a new point is one roster field, one `Live` row, and one fire site on the run spine.
- Boundary: hook scope rides the `FabricationRuntime` instance, so two apps composing the library never share a mutable registry or shadow each other's subscribers; ids obey the four-segment `rasm.<pkg>.<domain>.<point>` grammar the `HookId` admission enforces, and a veto refusal returns on the run's own rail as the subscriber's typed fault.

```csharp signature
public sealed record FabricationHooks(
    HookPoint<FabricationInput> Admission,
    HookPoint<PlannedStep> StageAdvance,
    HookPoint<ContentKey> EgressMint,
    HookPoint<FabricationResult.VerificationResult> VerifyVerdict,
    HookPoint<RunEvidence> Delivery,
    Atom<Seq<IsolatedFault>> Faults) {
    public static FabricationHooks Live() {
        var faults = Atom(Seq<IsolatedFault>());
        return new(
            new(HookId.Create("rasm.fabrication.run.admission"), HookModality.Veto, faults),
            new(HookId.Create("rasm.fabrication.derive.stage"), HookModality.Observe, faults),
            new(HookId.Create("rasm.fabrication.egress.mint"), HookModality.Veto, faults),
            new(HookId.Create("rasm.fabrication.verify.verdict"), HookModality.Replay, faults),
            new(HookId.Create("rasm.fabrication.delivery.handoff"), HookModality.Observe, faults),
            faults);
    }

    public Seq<IHookPoint> Points => Seq<IHookPoint>(Admission, StageAdvance, EgressMint, VerifyVerdict, Delivery);
}
```

## [08]-[SLO_ROWS]

- Owner: `FabricationSlo` — one burn-rate objective row naming its instrument, breach selector, target, window, and burn lane; `FabricationSlos` — the row set beside the instrument roster; `SliKind` and `BurnLane` — the objective-kind and burn-severity vocabularies.
- Cases: wear-critical rate · capability-violation budget · gouge budget · fleet stale-match ratio · cycle-time envelope.
- Auto: the AppHost alert rail and the deploy-plane dashboard compile consume the same rows, so a roster change re-derives verdicts, alerts, and panels in one diff and a hand-authored panel or rule beside the rows is the drift defect.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new shop objective is one row; a new objective shape is one `SliKind` row breaking the compile legs that dispatch on it.
- Boundary: kind and lane keys are value federation to the core observe vocabulary (`ratio`/`latency`/`saturation`/`freshness`; `page`/`ticket`) — no type crosses the language boundary; burn thresholds and window pairs stay the core closed multi-window burn table and are never re-decided per row, so a row carries its lane and every threshold derives; windows never undercut the longest 72-hour burn row.

```csharp signature
[SmartEnum<string>]
public sealed partial class SliKind {
    public static readonly SliKind Ratio = new("ratio");
    public static readonly SliKind Latency = new("latency");
    public static readonly SliKind Saturation = new("saturation");
    public static readonly SliKind Freshness = new("freshness");
}

[SmartEnum<string>]
public sealed partial class BurnLane {
    public static readonly BurnLane Page = new("page");
    public static readonly BurnLane Ticket = new("ticket");
}

public sealed record FabricationSlo(
    string Name,
    SliKind Kind,
    string Instrument,
    string Breach,
    Option<string> Total,
    double Target,
    Duration Window,
    BurnLane Burn);

public static class FabricationSlos {
    public static readonly Seq<FabricationSlo> Rows = Seq(
        new FabricationSlo("fabrication.wear.critical", SliKind.Ratio, "rasm.fabrication.tool.wear",
            "action=replace,retire", None, 0.99, Duration.FromDays(30), BurnLane.Page),
        new FabricationSlo("fabrication.capability.violations", SliKind.Ratio, "rasm.fabrication.capability.violations",
            "count>0", Some("rasm.fabrication.capability.index"), 0.995, Duration.FromDays(30), BurnLane.Ticket),
        new FabricationSlo("fabrication.removal.gouges", SliKind.Ratio, "rasm.fabrication.removal.defects",
            "kind=gouge", Some("rasm.fabrication.run.artifacts"), 0.999, Duration.FromDays(30), BurnLane.Page),
        new FabricationSlo("fabrication.fleet.stale", SliKind.Ratio, "rasm.fabrication.fleet.stale",
            "measured=false", Some("rasm.fabrication.fleet.utilization"), 0.95, Duration.FromDays(7), BurnLane.Ticket),
        new FabricationSlo("fabrication.cycle.envelope", SliKind.Latency, "rasm.fabrication.cycle.duration",
            "le=14400", None, 0.95, Duration.FromDays(7), BurnLane.Ticket));
}
```

## [09]-[RESEARCH]

| [QUESTION] | [ROUTE] |
| :--------- | :------ |
| Which exact diagnostics members complete the activity bracket? | `CATALOG → ADAPTER` |

`CATALOG` = `libs/csharp/.api/api-diagnostics-metrics.md`; `ADAPTER` = `libs/csharp/Rasm.AppHost`.
