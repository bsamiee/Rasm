# [MATERIALS_OBSERVABILITY]

THE MATERIALS SIGNAL TAP. `MaterialsTap` decorators fire typed `MaterialsFact` cases, so domain owners emit nothing: `MaterialsHookRail` declares the `rasm.materials.<domain>.<point>` points the one AppHost `HookRegistry.Mount` freezes, `MaterialsInstrumentFan` folds facts onto `IMeterFactory`-minted UCUM instruments, `MaterialsLog` bands faults onto `ILogger`, `MaterialsLatency` checkpoints the eager constructions, and `MaterialsDescriptors` mints the panel and alert rows the IaC compile leg decodes. No OpenTelemetry type is reachable from this page — provider composition is the app root's.

Settled composition: `HookPoint<TFact>`, `IHookPoint`, `HookId`, `HookModality`, `InstrumentRow`, `InstrumentSet`, `TelemetryContributorPort`, and `TelemetrySource.Materials` arrive from the `Rasm.AppHost` observability spine; the fact payloads compose the Component, Appearance, Properties, and seam owners as found. Instrument names run dotted `rasm.materials.<domain>.<measure>` with UCUM units, and every histogram row ships `InstrumentAdvice<double>` explicit-bucket boundaries as the fallback a backend without base2-exponential histograms reads.

## [01]-[INDEX]

- [02]-[FACT_FAMILY]: the closed `MaterialsFact` union and the `MaterialsTap` composition-root decorators.
- [03]-[HOOK_ROSTER]: the `MaterialsHookRail` seven-point roster under the AppHost `HookId` grammar.
- [04]-[INSTRUMENT_FAN]: `MaterialsBuckets`, `MaterialsLevelCells`, and the `MaterialsInstrumentFan` fact-to-instrument projection.
- [05]-[FAULT_LOG]: `MaterialsLog` banded `[LoggerMessage]` records and the `MaterialsLatency` checkpoint ledger.
- [06]-[DESCRIPTOR_ROWS]: `MaterialsDescriptors` panel and alert rows over the instrument roster.

## [02]-[FACT_FAMILY]

- Owner: `MaterialsFact` — the closed evidence union every tap fires and every projection folds; `MaterialsTap` — the composition-root decorators that fire facts around the owning entrypoints, so domain owners emit nothing.
- Cases: `CatalogueAdmit` (the row a veto gate may transform or refuse pre-freeze), `SectionSolve` (profile case, solved section, wall duration), `CapacityCheck` (the lifted `CapacityReceipt`, the `Utilisation` verdict, wall duration), `GraphCompile` (material, ordered node count, wall duration), `AcquisitionFit` (the measured `Provenance` receipt, wall duration), `WireMint` (material, `WireProvenance` receipt), `ProjectionGate` (the `GraphDelta` a veto may refuse pre-merge).
- Entry: each `MaterialsTap` member wraps one owning entrypoint as a delegate the composition root binds — `Admit` the catalogue veto, `Solve` the section solve, `Checked` the capacity fold, `Compiled` the graph compile, `Fitted` the acquisition import, `Minted` the wire mint, `Gated` the projection veto — so the fire site is a decoration, never an edit to the owning page.
- Auto: `Timed` stamps wall duration once through `TimeProvider.GetTimestamp`/`GetElapsedTime`, so every duration column derives from one injected clock; a veto refusal returns on the emitter's own rail as the gate's typed fault, and an observe tap failure converts at the AppHost shield without touching the emitter.
- Packages: Rasm.AppHost (project), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new evidence shape is one `MaterialsFact` case and one `MaterialsTap` member, breaking the fan's total `Switch` at compile time.
- Boundary: facts carry receipts the owning pages already mint — `CapacityReceipt`, `Provenance`, `WireProvenance`, `ComputedSection` — never re-derived scalars, so the fact stream is a projection of typed truth; the `TimeProvider` is injected at composition and defaults to `TimeProvider.System`, never an ambient static read inside a transform.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialsFact {
    private MaterialsFact() { }

    public sealed record CatalogueAdmit(ComponentRow Row) : MaterialsFact;
    public sealed record SectionSolve(Op Key, string Profile, ComputedSection Section, Duration Elapsed) : MaterialsFact;
    public sealed record CapacityCheck(Op Key, CapacityReceipt Receipt, Utilisation Verdict, Duration Elapsed) : MaterialsFact;
    public sealed record GraphCompile(Op Key, MaterialId Material, int Nodes, Duration Elapsed) : MaterialsFact;
    public sealed record AcquisitionFit(Op Key, Provenance Receipt, Duration Elapsed) : MaterialsFact;
    public sealed record WireMint(Op Key, MaterialId Material, WireProvenance Receipt) : MaterialsFact;
    public sealed record ProjectionGate(GraphDelta Delta) : MaterialsFact;
}

public static class MaterialsTap {
    public static Func<ComponentRow, Fin<ComponentRow>> Admit(MaterialsHookRail rail) =>
        row => rail.CatalogueAdmit.Fire(new MaterialsFact.CatalogueAdmit(row)).Map(static fact => fact.Row);

    public static Func<SectionProfile, Op, Fin<ComputedSection>> Solve(MaterialsHookRail rail, TimeProvider clock) =>
        (profile, key) => Timed(clock, () => SectionSolver.Solve(profile, key),
            (section, elapsed) => rail.SectionSolve.Fire(new MaterialsFact.SectionSolve(key, profile.GetType().Name, section, elapsed)));

    public static Fin<Utilisation> Checked(MaterialsHookRail rail, TimeProvider clock, CapacityReceipt receipt, Op key, Func<Fin<Utilisation>> check) =>
        Timed(clock, check, (verdict, elapsed) => rail.CapacityCheck.Fire(new MaterialsFact.CapacityCheck(key, receipt, verdict, elapsed)));

    public static Fin<CompiledGraph> Compiled(MaterialsHookRail rail, TimeProvider clock, MaterialId material, Op key, Func<Fin<CompiledGraph>> compile) =>
        Timed(clock, compile, (graph, elapsed) => rail.GraphCompile.Fire(new MaterialsFact.GraphCompile(key, material, graph.Order.Count, elapsed)));

    public static Fin<T> Fitted<T>(MaterialsHookRail rail, TimeProvider clock, Op key, Func<T, Provenance> receipt, Func<Fin<T>> import) =>
        Timed(clock, import, (value, elapsed) => rail.AcquisitionFit.Fire(new MaterialsFact.AcquisitionFit(key, receipt(value), elapsed)));

    public static Fin<T> Minted<T>(MaterialsHookRail rail, Op key, MaterialId material, Func<T, WireProvenance> receipt, Func<Fin<T>> mint) =>
        mint().Map(value => (ignore(rail.WireMint.Fire(new MaterialsFact.WireMint(key, material, receipt(value)))), value).Item2);

    public static Fin<GraphDelta> Gated(MaterialsHookRail rail, Fin<GraphDelta> projected) =>
        projected.Bind(delta => rail.ProjectionGate.Fire(new MaterialsFact.ProjectionGate(delta)).Map(static fact => fact.Delta));

    static Fin<T> Timed<T, TFact>(TimeProvider clock, Func<Fin<T>> run, Func<T, Duration, Fin<TFact>> fire) {
        long stamp = clock.GetTimestamp();
        return run().Map(value => (ignore(fire(value, Duration.FromTimeSpan(clock.GetElapsedTime(stamp)))), value).Item2);
    }
}
```

## [03]-[HOOK_ROSTER]

- Owner: `MaterialsHookRail` — the typed point roster this folder declares under the AppHost `HookId` grammar and contributes whole to the one `HookRegistry.Mount` at composition.
- Cases: `rasm.materials.catalogue.admit` Veto, `rasm.materials.section.solve` Observe, `rasm.materials.capacity.check` Observe, `rasm.materials.graph.compile` Observe, `rasm.materials.acquisition.fit` Replay (late subscribers drain the bounded recent-fit window), `rasm.materials.wire.mint` Observe, `rasm.materials.projection.project` Veto.
- Entry: `MaterialsHookRail.Live()` mints the seven points once; `Points` is the `Seq<IHookPoint>` the composition root spreads into the one AppHost registry mount, whose frozen-table duplicate-id throw owns cross-package collision.
- Auto: subscription reaches a point through its declared rail field, so a name-resolved lookup surface never exists; each point binds one closed `MaterialsFact` case, so a stringly payload cannot enter the rail.
- Packages: Rasm.AppHost (project), LanguageExt.Core.
- Growth: a new point is one rail field, one `Live` row, and one `Points` element.
- Boundary: the AppHost registry stays the one mount and the one uniqueness law — this rail declares points and never carries a second registry, queue, or scheduler; subscriber-fault isolation is the AppHost shield, so a throwing Materials tap lands on the hook fault rail and the emitter's `Fire` result is untouched.

```csharp signature
public sealed record MaterialsHookRail(
    HookPoint<MaterialsFact.CatalogueAdmit> CatalogueAdmit,
    HookPoint<MaterialsFact.SectionSolve> SectionSolve,
    HookPoint<MaterialsFact.CapacityCheck> CapacityCheck,
    HookPoint<MaterialsFact.GraphCompile> GraphCompile,
    HookPoint<MaterialsFact.AcquisitionFit> AcquisitionFit,
    HookPoint<MaterialsFact.WireMint> WireMint,
    HookPoint<MaterialsFact.ProjectionGate> ProjectionGate) {

    public static MaterialsHookRail Live() => new(
        new HookPoint<MaterialsFact.CatalogueAdmit>(HookId.Create("rasm.materials.catalogue.admit"), HookModality.Veto),
        new HookPoint<MaterialsFact.SectionSolve>(HookId.Create("rasm.materials.section.solve"), HookModality.Observe),
        new HookPoint<MaterialsFact.CapacityCheck>(HookId.Create("rasm.materials.capacity.check"), HookModality.Observe),
        new HookPoint<MaterialsFact.GraphCompile>(HookId.Create("rasm.materials.graph.compile"), HookModality.Observe),
        new HookPoint<MaterialsFact.AcquisitionFit>(HookId.Create("rasm.materials.acquisition.fit"), HookModality.Replay),
        new HookPoint<MaterialsFact.WireMint>(HookId.Create("rasm.materials.wire.mint"), HookModality.Observe),
        new HookPoint<MaterialsFact.ProjectionGate>(HookId.Create("rasm.materials.projection.project"), HookModality.Veto));

    public Seq<IHookPoint> Points =>
        Seq<IHookPoint>(CatalogueAdmit, SectionSolve, CapacityCheck, GraphCompile, AcquisitionFit, WireMint, ProjectionGate);
}
```

## [04]-[INSTRUMENT_FAN]

- Owner: `MaterialsBuckets` — explicit-bucket advice rows; `MaterialsLevelCells` — the level atoms observable gauges read at collection cadence; `MaterialsInstrumentFan` — the one fact-to-instrument projection over the closed union.
- Entry: `MaterialsInstrumentFan.Telemetry(string version, string schemaUrl)` is the `TelemetryContributorPort` the app root mounts beside every sibling contribution, the `SchemaUrl` coordinate stamping the semconv schema pin `TelemetryIdentity.Mint` reads; `Mount(Meter meter)` materializes the roster once over the minted `TelemetrySource.Materials` meter into the AppHost `InstrumentSet` capsule; `Project(InstrumentSet set, MaterialsFact fact)` folds one fact through the generated total `Switch`; `Tap(MaterialsHookRail rail, InstrumentSet set)` mounts the seven observe subscriptions at composition.
- Auto: every projection rides the hook rail with zero call-site metering; the catalogue and library gauges read the `MaterialsLevelCells.Live` atoms the catalogue and library builds fold at composition, so a level read is a current cell, never a re-derived scan; `KindOf` derives the capacity tag from the receipt's own generated dispatch, so a new receipt case breaks the tag projection at compile time.
- Packages: Rasm.AppHost (project), LanguageExt.Core, BCL inbox.
- Growth: a new projected fact is one roster row and one `Switch` arm; a level-shaped fact is one `MaterialsLevelCells` atom and one observable-gauge row.
- Boundary: instruments stay curated aggregates — capacity tags carry kind, governing action, and adequacy; acquisition tags carry the capture method key; wire tags carry the material key bounded by the library roster and the `WireProvenance.Method` instrument key — and every tag fan rides the AppHost tenant cardinality cap; per-app meter scope rides `IMeterFactory` through the AppHost mint, so two hosts never share a meter.

```csharp signature
public static class MaterialsBuckets {
    public static readonly ImmutableArray<double> SolveSeconds = [0.00001, 0.0001, 0.001, 0.01, 0.1, 0.5, 1, 5];
    public static readonly ImmutableArray<double> CompileSeconds = [0.0001, 0.001, 0.01, 0.05, 0.1, 0.5, 1, 5];
    public static readonly ImmutableArray<double> FitSeconds = [0.01, 0.05, 0.1, 0.5, 1, 5, 15, 60];
    public static readonly ImmutableArray<double> Utilisation = [0.25, 0.5, 0.75, 0.9, 1, 1.1, 1.25, 1.5, 2, 4];
    public static readonly ImmutableArray<double> ResidualDecades = [0.000001, 0.00001, 0.0001, 0.001, 0.01, 0.1, 1];

    public static Histogram<double> Advised(Meter meter, string name, string unit, string text, ImmutableArray<double> bounds) =>
        meter.CreateHistogram<double>(name, unit, text, tags: null, advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = bounds });
}

public sealed record MaterialsLevelCells(Atom<long> CatalogueRows, Atom<long> LibraryRows) {
    public static readonly MaterialsLevelCells Live = new(Atom(0L), Atom(0L));
}

public static class MaterialsInstrumentFan {
    public static readonly Seq<InstrumentRow> Rows = Seq(
        new InstrumentRow(TelemetrySource.Materials, "rasm.materials.catalogue.rows", "{row}", "component rows admitted into the frozen catalogue",
            static (meter, name, unit, text) => meter.CreateObservableGauge(name, static () => MaterialsLevelCells.Live.CatalogueRows.Value, unit, text)),
        new InstrumentRow(TelemetrySource.Materials, "rasm.materials.library.rows", "{row}", "material rows registered in the appearance library",
            static (meter, name, unit, text) => meter.CreateObservableGauge(name, static () => MaterialsLevelCells.Live.LibraryRows.Value, unit, text)),
        new InstrumentRow(TelemetrySource.Materials, "rasm.materials.catalogue.admissions", "{row}", "catalogue rows crossing the admission veto",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Materials, "rasm.materials.section.solves", "{solve}", "section solves by profile case",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Materials, "rasm.materials.section.duration", "s", "section solve wall duration",
            static (meter, name, unit, text) => MaterialsBuckets.Advised(meter, name, unit, text, MaterialsBuckets.SolveSeconds)),
        new InstrumentRow(TelemetrySource.Materials, "rasm.materials.capacity.checks", "{check}", "capacity checks by receipt kind, governing action, and adequacy",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Materials, "rasm.materials.capacity.utilisation", "1", "bounded utilisation verdicts per check",
            static (meter, name, unit, text) => MaterialsBuckets.Advised(meter, name, unit, text, MaterialsBuckets.Utilisation)),
        new InstrumentRow(TelemetrySource.Materials, "rasm.materials.graph.compiles", "{compile}", "material graph compiles by material",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Materials, "rasm.materials.graph.duration", "s", "graph compile wall duration",
            static (meter, name, unit, text) => MaterialsBuckets.Advised(meter, name, unit, text, MaterialsBuckets.CompileSeconds)),
        new InstrumentRow(TelemetrySource.Materials, "rasm.materials.acquisition.fits", "{fit}", "acquisition imports by capture method",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Materials, "rasm.materials.acquisition.duration", "s", "acquisition fit wall duration",
            static (meter, name, unit, text) => MaterialsBuckets.Advised(meter, name, unit, text, MaterialsBuckets.FitSeconds)),
        new InstrumentRow(TelemetrySource.Materials, "rasm.materials.acquisition.residual", "1", "witnessed fit residual per import",
            static (meter, name, unit, text) => MaterialsBuckets.Advised(meter, name, unit, text, MaterialsBuckets.ResidualDecades)),
        new InstrumentRow(TelemetrySource.Materials, "rasm.materials.wire.mints", "{wire}", "material wires minted by material key",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Materials, "rasm.materials.projection.gates", "{delta}", "graph deltas crossing the projection veto",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Materials, "rasm.materials.faults", "{fault}", "banded faults by band and category",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)));

    public static TelemetryContributorPort Telemetry(string version, string schemaUrl) => new(TelemetrySource.Materials, version, schemaUrl, Rows);

    public static InstrumentSet Mount(Meter meter) =>
        new(Rows.ToFrozenDictionary(static row => row.Name, row => row.Bind(meter, row.Name, row.Unit, row.Description), StringComparer.Ordinal));

    public static Seq<IDisposable> Tap(MaterialsHookRail rail, InstrumentSet set) => Seq(
        rail.CatalogueAdmit.Observe(fact => IO.lift(() => Project(set, fact))),
        rail.SectionSolve.Observe(fact => IO.lift(() => Project(set, fact))),
        rail.CapacityCheck.Observe(fact => IO.lift(() => Project(set, fact))),
        rail.GraphCompile.Observe(fact => IO.lift(() => Project(set, fact))),
        rail.AcquisitionFit.Observe(fact => IO.lift(() => Project(set, fact))),
        rail.WireMint.Observe(fact => IO.lift(() => Project(set, fact))),
        rail.ProjectionGate.Observe(fact => IO.lift(() => Project(set, fact))));

    public static Unit Project(InstrumentSet set, MaterialsFact fact) => fact.Switch(
        state: set,
        catalogueAdmit: static (s, _) => s.Count("rasm.materials.catalogue.admissions", 1L),
        sectionSolve: static (s, f) => Solved(s, f),
        capacityCheck: static (s, f) => Checked(s, f),
        graphCompile: static (s, f) => Compiled(s, f),
        acquisitionFit: static (s, f) => Fitted(s, f),
        wireMint: static (s, f) => s.Count("rasm.materials.wire.mints", 1L,
            new KeyValuePair<string, object?>("material", f.Material.Value),
            new KeyValuePair<string, object?>("method", f.Receipt.Method)),
        projectionGate: static (s, _) => s.Count("rasm.materials.projection.gates", 1L));

    // Expected = the kernel Rasm.Domain.Expected whose virtual Category feeds the tag, never the LanguageExt.Common global-using twin.
    public static Unit Faulted(InstrumentSet set, Expected fault) =>
        set.Count("rasm.materials.faults", 1L,
            new KeyValuePair<string, object?>("band", fault.Code),
            new KeyValuePair<string, object?>("category", fault.Category));

    public static string KindOf(CapacityReceipt receipt) => receipt.Switch(
        steel: static _ => nameof(CapacityReceipt.Steel),
        timber: static _ => nameof(CapacityReceipt.Timber),
        masonry: static _ => nameof(CapacityReceipt.Masonry),
        reinforcedMasonry: static _ => nameof(CapacityReceipt.ReinforcedMasonry),
        glass: static _ => nameof(CapacityReceipt.Glass),
        weld: static _ => nameof(CapacityReceipt.Weld),
        adhesive: static _ => nameof(CapacityReceipt.Adhesive),
        stud: static _ => nameof(CapacityReceipt.Stud),
        connector: static _ => nameof(CapacityReceipt.Connector));

    static Unit Solved(InstrumentSet set, MaterialsFact.SectionSolve fact) {
        KeyValuePair<string, object?>[] tags = [new("profile", fact.Profile)];
        ignore(set.Count("rasm.materials.section.solves", 1L, tags));
        return set.Record("rasm.materials.section.duration", fact.Elapsed.TotalSeconds, tags);
    }

    static Unit Checked(InstrumentSet set, MaterialsFact.CapacityCheck fact) {
        KeyValuePair<string, object?>[] tags =
            [new("kind", KindOf(fact.Receipt)), new("governing", fact.Verdict.Governing.Key), new("adequate", fact.Verdict.Adequate)];
        ignore(set.Count("rasm.materials.capacity.checks", 1L, tags));
        return fact.Verdict is Utilisation.Bounded bounded
            ? set.Record("rasm.materials.capacity.utilisation", bounded.Value, tags)
            : unit;
    }

    static Unit Compiled(InstrumentSet set, MaterialsFact.GraphCompile fact) {
        KeyValuePair<string, object?>[] tags = [new("material", fact.Material.Value)];
        ignore(set.Count("rasm.materials.graph.compiles", 1L, tags));
        return set.Record("rasm.materials.graph.duration", fact.Elapsed.TotalSeconds, tags);
    }

    static Unit Fitted(InstrumentSet set, MaterialsFact.AcquisitionFit fact) {
        KeyValuePair<string, object?>[] tags = [new("method", fact.Receipt.Method.Key)];
        ignore(set.Count("rasm.materials.acquisition.fits", 1L, tags));
        ignore(set.Record("rasm.materials.acquisition.duration", fact.Elapsed.TotalSeconds, tags));
        return set.Record("rasm.materials.acquisition.residual", fact.Receipt.FitResidual, tags);
    }
}
```

## [05]-[FAULT_LOG]

- Owner: `MaterialsLog` — the generated `[LoggerMessage]` partials banding every fault this folder rails; `MaterialsLatency` — the checkpoint vocabulary and the measured bracket over the eager constructions.
- Entry: `MaterialsLog.Banded(ILogger logger, InstrumentSet set, Expected fault)` routes one fault to its banded record and its fault-counter write in one fold; `MaterialsLatency.Measured(ILatencyContext ledger, CheckpointToken started, CheckpointToken settled, Func<Fin<T>> body)` brackets one eager construction between two checkpoints.
- Auto: band routing reads the fault's own `Code` against the `FaultBand` registry rows, so a fault logs under its owning band with no per-site branch; the checkpoint names register once at the app root through `RegisterCheckpointNames` and tokens resolve once through `ILatencyContextTokenIssuer.GetCheckpointToken`, so an unregistered name is a composition-time refusal.
- Packages: Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Telemetry.Abstractions, Rasm.AppHost (project), LanguageExt.Core.
- Growth: a new band is one generated partial and one routing arm; a new eager construction is one checkpoint pair in `Checkpoints`.
- Boundary: libraries reference the two contract assemblies alone and take `ILogger` and the latency ledger by injection with `NullLogger.Instance` the unbound default; `Expected` binds the kernel `Rasm.Domain.Expected` whose virtual `Category` the banding reads — the `LanguageExt.Common` global-using twin carries none; the latency ledger is cheaper than child spans and free of sampling coupling, so span wrapping stays the AppHost trace spine and never doubles here.

```csharp signature
public static partial class MaterialsLog {
    // EventId literals mirror the FaultBand registry rows — the attribute slot admits only a constant.
    [LoggerMessage(EventId = 2300, Level = LogLevel.Warning, Message = "component fault {Category}: {Detail}")]
    public static partial void ComponentFaulted(ILogger logger, string category, string detail);

    [LoggerMessage(EventId = 2450, Level = LogLevel.Warning, Message = "material fault {Category}: {Detail}")]
    public static partial void MaterialFaulted(ILogger logger, string category, string detail);

    [LoggerMessage(EventId = 2470, Level = LogLevel.Warning, Message = "projection fault {Category}: {Detail}")]
    public static partial void ProjectionFaulted(ILogger logger, string category, string detail);

    [LoggerMessage(EventId = 2500, Level = LogLevel.Warning, Message = "element fault {Category}: {Detail}")]
    public static partial void ElementFaulted(ILogger logger, string category, string detail);

    public static Unit Banded(ILogger logger, InstrumentSet set, Expected fault) {
        ignore(MaterialsInstrumentFan.Faulted(set, fault));
        return fault.Code switch {
            var code when code == FaultBand.Component => fun(() => ComponentFaulted(logger, fault.Category, fault.Message))(),
            var code when code == FaultBand.Material => fun(() => MaterialFaulted(logger, fault.Category, fault.Message))(),
            var code when code == FaultBand.Projection => fun(() => ProjectionFaulted(logger, fault.Category, fault.Message))(),
            _ => fun(() => ElementFaulted(logger, fault.Category, fault.Message))(),
        };
    }
}

public static class MaterialsLatency {
    public const string CatalogueBuildStarted = "rasm.materials.catalogue.build.started";
    public const string CatalogueBuildSettled = "rasm.materials.catalogue.build.settled";
    public const string InteractionSolveStarted = "rasm.materials.interaction.solve.started";
    public const string InteractionSolveSettled = "rasm.materials.interaction.solve.settled";

    // RegisterCheckpointNames rows the app root registers before any token resolves.
    public static readonly Seq<string> Checkpoints =
        Seq(CatalogueBuildStarted, CatalogueBuildSettled, InteractionSolveStarted, InteractionSolveSettled);

    public static Fin<T> Measured<T>(ILatencyContext ledger, CheckpointToken started, CheckpointToken settled, Func<Fin<T>> body) {
        ledger.AddCheckpoint(started);
        return body().Map(value => (fun(() => ledger.AddCheckpoint(settled))(), value).Item2);
    }
}
```

## [06]-[DESCRIPTOR_ROWS]

- Owner: `PanelKind` and `AlertSeverity` — the bounded descriptor vocabularies; `PanelRow` and `AlertRow` — the typed descriptor shapes; `MaterialsDescriptors` — the panel and alert roster over the instrument names.
- Entry: `MaterialsDescriptors.Panels` and `Alerts` are the data the IaC compile leg decodes into boards and burn-rate rules, so board truth derives from the mounted roster and the compile leg authors nothing.
- Auto: every descriptor names an instrument the `[04]` roster mints, so a renamed instrument breaks its descriptors in one page; an alert threshold is a burn-fraction hint the SLO algebra consumes, never a provisioned rule body.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new board panel or alert is one row; a new descriptor shape is one field on the owning row record.
- Boundary: dashboards, alert provisioning, tenancy, and the burn-rate algebra are the IaC plane's — this page mints descriptor data behind the same `rasm.materials.*` names the instruments carry and never a query string, board JSON, or provider type.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelKind {
    public static readonly PanelKind Timeseries = new("timeseries");
    public static readonly PanelKind Stat = new("stat");
    public static readonly PanelKind Heatmap = new("heatmap");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AlertSeverity {
    public static readonly AlertSeverity Page = new("page");
    public static readonly AlertSeverity Warn = new("warn");
}

public sealed record PanelRow(string Title, string Instrument, PanelKind Kind);

public sealed record AlertRow(string Name, string Instrument, double BurnFraction, Duration Window, AlertSeverity Severity);

public static class MaterialsDescriptors {
    public static readonly Seq<PanelRow> Panels = Seq(
        new PanelRow("Section solve rate", "rasm.materials.section.solves", PanelKind.Timeseries),
        new PanelRow("Section solve latency", "rasm.materials.section.duration", PanelKind.Heatmap),
        new PanelRow("Capacity verdicts", "rasm.materials.capacity.checks", PanelKind.Timeseries),
        new PanelRow("Capacity utilisation", "rasm.materials.capacity.utilisation", PanelKind.Heatmap),
        new PanelRow("Graph compile latency", "rasm.materials.graph.duration", PanelKind.Heatmap),
        new PanelRow("Acquisition fit residual", "rasm.materials.acquisition.residual", PanelKind.Heatmap),
        new PanelRow("Catalogue rows", "rasm.materials.catalogue.rows", PanelKind.Stat),
        new PanelRow("Library rows", "rasm.materials.library.rows", PanelKind.Stat),
        new PanelRow("Fault rate", "rasm.materials.faults", PanelKind.Timeseries));

    public static readonly Seq<AlertRow> Alerts = Seq(
        new AlertRow("materials-fault-burn", "rasm.materials.faults", 0.02d, Duration.FromMinutes(30), AlertSeverity.Page),
        new AlertRow("materials-inadequate-capacity", "rasm.materials.capacity.checks", 0.10d, Duration.FromHours(2), AlertSeverity.Warn),
        new AlertRow("materials-fit-residual", "rasm.materials.acquisition.residual", 0.05d, Duration.FromHours(6), AlertSeverity.Warn));
}
```

## [07]-[RESEARCH]

- [FOREIGN_POINT_MOUNT]-[OPEN]: `MaterialsHookRail.Points` spreads into the one AppHost `HookRegistry.Mount`, whose `params ReadOnlySpan<IHookPoint>` entry admits foreign-declared points structurally, and the Compute `ComputeHookRail` already mounts its points beside the AppHost rows — yet the AppHost hook-registry growth prose still spells the subscribers-only clause; confirm the package-declared-point amendment lands; route: the AppHost `Observability/hooks.md` `[04]-[HOOK_REGISTRY]` growth law.
- [SIBLING_FAN_ROW]-[OPEN]: `TelemetrySource.Materials` — the `("Rasm.Materials", minted: true)` identity row — waits on the AppHost telemetry vocabulary; provider `AddMeter`/`AddSource` rosters derive from `TelemetrySource.Items`, so the row lands once with zero roster edits; route: the AppHost `Observability/telemetry.md` `TelemetrySource` vocabulary fence.
