# [FABRICATION_TELEMETRY]

`FabricationFact` is the package's one fact vocabulary for measured production: every operational metric is a projection of a settled domain receipt flattened onto this union, and the instrument roster, the contributor port, the projection arms, and the classification rows all derive from it — a metric minted beside the fan is a second truth. Domain kernels stay pure; facts fire only where receipts settle on the run spine, through the one `FabricationTap` port `Process/owner#RUN_FOLD`'s `FabricationRuntime` carries.

Settled composition: `ReceiptSinkPort`, `ReceiptEnvelope`, and `TenantContext` arrive from the AppHost port vocabulary, and `TelemetrySource.Fabrication`, `InstrumentRow`, `TelemetryContributorPort`, and `InstrumentSet` from the observability spine — instruments mount through the AppHost meter mint, and the folder holds no OpenTelemetry reference, exporter, or provider.

## [01]-[INDEX]

- [02]-[FACT_UNION]: `FabricationFact` kind-keyed union, per-receipt `Of` projections, wire context, tap port, and sink-bound emission.
- [03]-[INSTRUMENT_ROSTER]: `rasm.fabrication.*` `InstrumentRow` roster, bucket advice, and the contributor port.
- [04]-[FACT_PROJECTION]: Kind-keyed projection arms from the receipt envelope onto mounted instruments.
- [05]-[CLASSIFICATION]: Suite-taxonomy attribute rows for the classified receipt members.

## [02]-[FACT_UNION]

- Owner: `FabricationFact` — the closed fact union; `FabricationWireContext` — the Strict serializer context whose polymorphism metadata is the one kind registry; `FabricationTap` — the runtime emission port; `FabricationSurface` — the sink-bound emission seam.
- Cases: tool-wear · tool-refresh · cutting-fit · probe · capability · removal · cycle · estimate · fleet-match · run · quality-seal · traveler.
- Entry: `FabricationTap.Fire(FabricationFact fact)` — the sole in-package emission verb; `FabricationSurface.Emit(CorrelationId correlation, FabricationFact fact)` binds sink and serializer once at composition and the app root wires the tap onto it, so `FabricationTap.Silent` keeps a headless kernel run emitting into unit with zero branching; `Fire` collapses a subscriber failure through `Try`, so a tap fault never re-enters the emitting fold.
- Auto: each `Of` projection flattens its receipt to measures and bounded dimensions at the fact boundary — a smart-enum spine value crosses as its key scalar, a NodaTime span as seconds — so the wire context serializes primitives only; wire kind derives from the polymorphic metadata pinned on the union, and ambient `TenantContext.Current` threads into `Send` so the envelope tenant field partitions evidence; `ToolWear.Of` yields `None` on a receipt without a critical state and `ToolRefresh.Of` on a provider-digest source, so non-measured admissions project nothing rather than fabricate zeros.
- Receipt: none — the union is the projection of receipts; `Probe` mints at its owning fold (`Verify/probing#DATUM_AND_RESULT`) because the pre-egress report is file-scoped there, and every other case mints through its `Of` row here — `Verify/removal#STOCK_FOLD` fires `Removal.Of` over the public `FabricationResult.VerificationResult`.
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
        public static Run Of(RunEvidence evidence, Duration elapsed) =>
            new(evidence.Process.Key,
                elapsed.TotalSeconds,
                toSeq(evidence.Request.Kinds).Map(static kind => kind.Key),
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

- Owner: `FabricationInstruments` — the Fabrication `InstrumentRow` roster and the `TelemetryContributorPort` mint; `FabricationBuckets` — the explicit-bucket advice policy rows.
- Entry: `FabricationInstruments.Telemetry(string version)` — the one contributor port carrying the domain row set into the AppHost registry at composition.
- Auto: every histogram row ships its `InstrumentAdvice<double>` boundaries at creation, the fallback a backend without exponential histograms reads — the default aggregation stays base2 exponential at the provider; registry mount de-duplicates by name, so a duplicate row is a composition fault, never a forked stream.
- Packages: LanguageExt.Core, BCL inbox.
- Growth: one measured concern is one `InstrumentRow` here and one projection arm at `[04]-[FACT_PROJECTION]`; a per-kind family derives from its owning vocabulary, never hand-enumerated rows.
- Boundary: instrument names are dotted `rasm.fabrication.<domain>.<measure>` with UCUM units, never pre-baked `_total` or unit suffixes; every row binds `TelemetrySource.Fabrication`, so scope id equals the version-stamped package id; all Fabrication facts are event-shaped, so the roster carries counters and histograms only, and a level-shaped measure entering the folder lands as an observable-gauge row with its cell owner; tag keys per instrument are the closed set the projection arms write (`basis`, `action`, `model`, `verdict`, `metric`, `kind`, `scope`, `currency`, `backed`, `process`, `verification`), and cardinality caps ride the app root's view rows, never call-site gating.

```csharp signature
public static class FabricationBuckets {
    public static readonly ImmutableArray<double> Fractions = [0.01, 0.05, 0.1, 0.25, 0.5, 0.75, 0.9, 1.0];
    public static readonly ImmutableArray<double> Millimeters = [0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 1.0];
    public static readonly ImmutableArray<double> CycleSeconds = [1, 10, 60, 300, 900, 3600, 14400, 86400];
    public static readonly ImmutableArray<double> RefreshSeconds = [60, 300, 900, 3600, 14400, 86400, 604800];

    public static Histogram<double> Advised(Meter meter, string name, string unit, string text, ImmutableArray<double> bounds) =>
        meter.CreateHistogram<double>(name, unit, text, tags: null, advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = bounds });
}

public static class FabricationInstruments {
    public static readonly Seq<InstrumentRow> Rows = Seq(
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.tool.wear", "1", "conservative remaining-life fraction at the critical wear state",
            static (meter, name, unit, text) => FabricationBuckets.Advised(meter, name, unit, text, FabricationBuckets.Fractions)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.tool.refresh.age", "s", "interval between successive telemetry catalog refreshes",
            static (meter, name, unit, text) => FabricationBuckets.Advised(meter, name, unit, text, FabricationBuckets.RefreshSeconds)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.fit.residual", "1", "RMS residual of the wear and machinability power-law fits",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.fit.quality", "1", "coefficient of determination of the machinability fit",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.probe.features", "{feature}", "inspected features by conformance verdict",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.probe.deviation", "mm", "worst absolute measured deviation per inspection",
            static (meter, name, unit, text) => FabricationBuckets.Advised(meter, name, unit, text, FabricationBuckets.Millimeters)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.capability.index", "1", "capability and performance index values by metric row",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.capability.violations", "{violation}", "SPC rule violations per study",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.removal.defects", "{finding}", "gouge findings per material-removal verification",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.removal.residual", "mm3", "uncut and overcut voxel volume per verification",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.removal.aircut", "1", "air-cut fraction of swept program motion per verification",
            static (meter, name, unit, text) => FabricationBuckets.Advised(meter, name, unit, text, FabricationBuckets.Fractions)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.cycle.duration", "s", "simulated modal cycle time per program",
            static (meter, name, unit, text) => FabricationBuckets.Advised(meter, name, unit, text, FabricationBuckets.CycleSeconds)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.cycle.energy", "kW.h", "simulated machine energy per program",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.cycle.distance", "mm", "simulated cutting-motion path length per program",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.estimate.money", "1", "signed money ledger total in receipt currency",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.estimate.carbon", "kg", "carbon ledger total as kilograms CO2-equivalent",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.estimate.clock", "s", "estimated machine clock per subject",
            static (meter, name, unit, text) => FabricationBuckets.Advised(meter, name, unit, text, FabricationBuckets.CycleSeconds)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.fleet.utilization", "1", "machine load factor at match assessment",
            static (meter, name, unit, text) => FabricationBuckets.Advised(meter, name, unit, text, FabricationBuckets.Fractions)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.fleet.effectiveness", "1", "machine effectiveness fraction at match assessment",
            static (meter, name, unit, text) => FabricationBuckets.Advised(meter, name, unit, text, FabricationBuckets.Fractions)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.fleet.stale", "{match}", "matches ranked on nameplate after the freshness fallback",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.run.duration", "s", "fabrication run wall duration",
            static (meter, name, unit, text) => FabricationBuckets.Advised(meter, name, unit, text, FabricationBuckets.CycleSeconds)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.run.artifacts", "{artifact}", "content-keyed artifacts produced by egress kind",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.run.warnings", "{warning}", "run warnings accumulated on the evidence receipt",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.sustainability.energy", "J", "sealed passport energy-use evidence",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.sustainability.carbon", "kg", "sealed passport embodied-carbon evidence",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.sustainability.waste", "kg", "sealed passport waste-mass evidence",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.sustainability.water", "L", "sealed passport water-use evidence",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Fabrication, "rasm.fabrication.traveler.amendments", "{amendment}", "as-run amendment chain length at traveler seal",
            static (meter, name, unit, text) => meter.CreateHistogram<long>(name, unit, text)));

    public static TelemetryContributorPort Telemetry(string version) => new(TelemetrySource.Fabrication, version, Rows);
}
```

## [04]-[FACT_PROJECTION]

- Owner: `FabricationFan` — the one envelope-to-instrument projection over the Fabrication kind registry.
- Entry: `FabricationFan.Project(InstrumentSet set, ReceiptEnvelope envelope)` folds one envelope into instrument writes through the kind-keyed table; the fan registers as one observe row on the AppHost hook-registry receipt tap at composition, so every envelope the sink emits projects with zero call-site metering.
- Auto: dimension values ride the payload's own key-scalar fields, so tag vocabularies stay bounded by the union's admission; a kind without a table row stays wire-only by declaration, and a fact field without an arm write stays wire evidence — `ConservativeRemaining` carries basis-keyed units one UCUM histogram cannot hold, `Score` is objective-relative, and the `Produced` and `Declarations` counts derive in the envelope store.
- Packages: LanguageExt.Core, BCL inbox.
- Growth: a new projected kind is one table row here and its instrument row at `[03]-[INSTRUMENT_ROSTER]`.
- Boundary: arm bodies are the one place fact wire names meet instrument writes — the platform-forced statement seam — and an arm never re-validates the payload its typed fact already admitted; subscriber isolation, replay, and veto modalities live on the AppHost hook registry hosting the tap, so a fan failure is that registry's typed fault and never re-enters the emitting fold.

```csharp signature
public static class FabricationFan {
    static readonly FrozenDictionary<string, Action<InstrumentSet, JsonElement>> Table =
        new Dictionary<string, Action<InstrumentSet, JsonElement>> {
            ["tool-wear"] = static (set, payload) => {
                var tags = (KeyValuePair<string, object?>[])[new("basis", payload.GetProperty("basis").GetString()), new("action", payload.GetProperty("action").GetString())];
                ignore(set.Record("rasm.fabrication.tool.wear", payload.GetProperty("fractionRemaining").GetDouble(), tags));
                ignore(set.Record("rasm.fabrication.fit.residual", payload.GetProperty("fitResidual").GetDouble(), new KeyValuePair<string, object?>("model", "taylor")));
            },
            ["tool-refresh"] = static (set, payload) =>
                ignore(set.Record("rasm.fabrication.tool.refresh.age", payload.GetProperty("ageSeconds").GetDouble())),
            ["cutting-fit"] = static (set, payload) => {
                var tags = (KeyValuePair<string, object?>[])[new("model", payload.GetProperty("model").GetString())];
                ignore(set.Record("rasm.fabrication.fit.residual", payload.GetProperty("residual").GetDouble(), tags));
                ignore(set.Record("rasm.fabrication.fit.quality", payload.GetProperty("determination").GetDouble(), tags));
            },
            ["probe"] = static (set, payload) => {
                long features = payload.GetProperty("features").GetInt64();
                long conforming = payload.GetProperty("conforming").GetInt64();
                ignore(set.Count("rasm.fabrication.probe.features", conforming, new KeyValuePair<string, object?>("verdict", "pass")));
                ignore(set.Count("rasm.fabrication.probe.features", features - conforming, new KeyValuePair<string, object?>("verdict", "fail")));
                ignore(set.Record("rasm.fabrication.probe.deviation", payload.GetProperty("worstDeviationMm").GetDouble()));
            },
            ["capability"] = static (set, payload) => {
                foreach (var row in payload.GetProperty("rows").EnumerateArray()) {
                    ignore(set.Record("rasm.fabrication.capability.index", row.GetProperty("value").GetDouble(),
                        new KeyValuePair<string, object?>("metric", row.GetProperty("metric").GetString())));
                }
                ignore(set.Count("rasm.fabrication.capability.violations", payload.GetProperty("violations").GetInt64()));
            },
            ["removal"] = static (set, payload) => {
                ignore(set.Count("rasm.fabrication.removal.defects", payload.GetProperty("gouges").GetInt64(), new KeyValuePair<string, object?>("kind", "gouge")));
                ignore(set.Record("rasm.fabrication.removal.residual", payload.GetProperty("uncutMm3").GetDouble(), new KeyValuePair<string, object?>("kind", "uncut")));
                ignore(set.Record("rasm.fabrication.removal.residual", payload.GetProperty("overcutMm3").GetDouble(), new KeyValuePair<string, object?>("kind", "overcut")));
                ignore(set.Record("rasm.fabrication.removal.aircut", payload.GetProperty("airCutRatio").GetDouble()));
            },
            ["cycle"] = static (set, payload) => {
                ignore(set.Record("rasm.fabrication.cycle.duration", payload.GetProperty("seconds").GetDouble()));
                ignore(set.Record("rasm.fabrication.cycle.energy", payload.GetProperty("energyKwh").GetDouble()));
                ignore(set.Record("rasm.fabrication.cycle.distance", payload.GetProperty("distanceMm").GetDouble()));
            },
            ["estimate"] = static (set, payload) => {
                var scope = new KeyValuePair<string, object?>("scope", payload.GetProperty("scope").GetString());
                ignore(set.Record("rasm.fabrication.estimate.money", payload.GetProperty("money").GetDouble(), scope,
                    new KeyValuePair<string, object?>("currency", payload.GetProperty("currency").GetString())));
                ignore(set.Record("rasm.fabrication.estimate.carbon", payload.GetProperty("carbonKg").GetDouble(), scope));
                ignore(set.Record("rasm.fabrication.estimate.clock", payload.GetProperty("clockSeconds").GetDouble(),
                    new KeyValuePair<string, object?>("backed", payload.GetProperty("simulationBacked").GetBoolean() ? "simulation" : "fallback")));
            },
            ["fleet-match"] = static (set, payload) => {
                ignore(set.Record("rasm.fabrication.fleet.utilization", payload.GetProperty("utilization").GetDouble(),
                    new KeyValuePair<string, object?>("process", payload.GetProperty("process").GetString())));
                ignore(set.Record("rasm.fabrication.fleet.effectiveness", payload.GetProperty("effectiveness").GetDouble(),
                    new KeyValuePair<string, object?>("process", payload.GetProperty("process").GetString())));
                if (!payload.GetProperty("measured").GetBoolean()) ignore(set.Count("rasm.fabrication.fleet.stale", 1L));
            },
            ["run"] = static (set, payload) => {
                ignore(set.Record("rasm.fabrication.run.duration", payload.GetProperty("seconds").GetDouble(),
                    new KeyValuePair<string, object?>("process", payload.GetProperty("process").GetString()),
                    new KeyValuePair<string, object?>("verification", payload.GetProperty("verification").GetString())));
                foreach (var kind in payload.GetProperty("kinds").EnumerateArray()) {
                    ignore(set.Count("rasm.fabrication.run.artifacts", 1L, new KeyValuePair<string, object?>("kind", kind.GetString())));
                }
                ignore(set.Count("rasm.fabrication.run.warnings", payload.GetProperty("warnings").GetInt64()));
            },
            ["quality-seal"] = static (set, payload) => {
                ignore(set.Record("rasm.fabrication.sustainability.energy", payload.GetProperty("energyJoules").GetDouble()));
                ignore(set.Record("rasm.fabrication.sustainability.carbon", payload.GetProperty("carbonKg").GetDouble()));
                ignore(set.Record("rasm.fabrication.sustainability.waste", payload.GetProperty("wasteKg").GetDouble()));
                ignore(set.Record("rasm.fabrication.sustainability.water", payload.GetProperty("waterLiters").GetDouble()));
            },
            ["traveler"] = static (set, payload) =>
                ignore(set.Record("rasm.fabrication.traveler.amendments", payload.GetProperty("amendments").GetInt64())),
        }.ToFrozenDictionary(StringComparer.Ordinal);

    public static Unit Project(InstrumentSet set, ReceiptEnvelope envelope) =>
        Table.TryGetValue(envelope.Kind, out var arm) ? fun(() => arm(set, envelope.Payload))() : unit;
}
```

## [05]-[CLASSIFICATION]

- Owner: `FabricationClassified` — the sealed attribute rows binding this folder's classified receipt members to the suite taxonomy by value.
- Cases: personal · confidential · credential.
- Auto: an annotated member redacts wherever a log or export seam expands it — HMAC for personnel and heat identity so cross-record correlation survives, erase for credential material — and sealed artifact bytes never redact: canonical documents are domain truth, classification governs telemetry egress alone.
- Packages: Microsoft.Extensions.Compliance.Redaction, BCL inbox.
- Growth: a newly classified member family is one attribute row binding an existing taxonomy key; a new sensitivity class is a suite-taxonomy decision, never a folder mint.
- Boundary: taxonomy name and row keys are value federation to the suite `DataClassification` vocabulary — the attribute rows carry `(taxonomy, value)` string pairs and no type reference crosses the package boundary; annotated owners are `AttestationPayload.Signer` and `.Credential`, `HeatNumber`, `WelderQualification.Welder`, and `TravelerAmendment.Actor`, each carrying its attribute at the declaring fence; `DataClassificationTypeConverter` string round-tripping stays under its `EXTEXP0002` gate as a declared policy value when a classification ever binds from configuration.

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

## [06]-[RESEARCH]

- [SUSTAINABILITY_UNITS]-[OPEN]: exact UnitsNet scalar-projection spellings `Mass.Kilograms` and `Volume.Liters` consumed by `QualitySeal.Of`; verify via `tools.assay api query` over UnitsNet and extend `.api/api-unitsnet.md` with the mass and volume rows.
- [MAINTENANCE_ACTION_SWITCH]-[OPEN]: generated `Switch` arm-parameter spelling for the keyword-colliding `Continue` case (`@continue`) on the `MaintenanceAction` `[Union]` consumed by `ToolWear.ActionKey`; confirm against the Thinktecture generated dispatch surface.
