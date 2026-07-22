# [MATERIALS_OBSERVABILITY]

MATERIALS signal evidence starts with the closed `MaterialsFact` family: `MaterialsHooks` composes the kernel signal capsule into the folder's seven-point rail, `MaterialsInstruments` projects the fact stream onto `rasm.materials.<domain>.<measure>` instruments as a rail subscriber, `MaterialsLatency` brackets eager constructions, and `MaterialsDescriptors` mints the panel and alert rows the IaC compile leg decodes.

Settled composition: `HookId`, `HookModality`, `HookPoint<TFact>`, `IsolatedFault`, `InstrumentRow`, `InstrumentSet`, `LevelCells`, `Buckets`, `TelemetryContributorPort`, and `TelemetryIdentity.Mint` arrive from the kernel signal capsule; fact payloads compose Component, Appearance, Properties, and seam receipts. Instrument names run dotted `rasm.materials.<domain>.<measure>` with UCUM units; the composing app admits the `Rasm.Materials` scope by name.

## [01]-[INDEX]

- [02]-[FACT_FAMILY]: the closed `MaterialsFact` union.
- [03]-[HOOK_RAIL]: the seven-point `MaterialsHooks` composition over the kernel capsule.
- [04]-[INSTRUMENT_TAP]: the instrument roster, level cells, contributor port, and the rail-subscribed projection.
- [05]-[FAULT_LOG]: the blocked fixed-severity log projection and the `MaterialsLatency` checkpoint ledger.
- [06]-[DESCRIPTOR_ROWS]: `MaterialsDescriptors` panel and alert rows over the instrument roster.

## [02]-[FACT_FAMILY]

- Owner: `MaterialsFact` — the closed evidence union every tap fires and every projection folds.
- Cases: `CatalogueAdmit` (the row a veto gate may transform or refuse pre-freeze), `SectionSolve` (profile case, solved section, wall duration), `CapacityCheck` (the lifted `CapacityReceipt`, the `Utilisation` verdict, wall duration), `GraphCompile` (material, ordered node count, wall duration), `AcquisitionFit` (the measured `Provenance` receipt, wall duration), `WireMint` (material, `WireProvenance` receipt), `ProjectionGate` (the `GraphDelta` a veto may refuse pre-merge).
- Entry: each composition-root decorator fires one case after the owning entrypoint settles; veto cases fire before catalogue freeze or graph merge.
- Auto: elapsed columns derive from one injected clock at the decorator boundary.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new evidence shape is one `MaterialsFact` case, one point row at `[03]`, and one projection arm at `[04]`.
- Boundary: facts carry receipts the owning pages already mint — `CapacityReceipt`, `Provenance`, `WireProvenance`, `ComputedSection` — and never re-derive their scalars.

```csharp signature
[Union]
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
```

## [03]-[HOOK_RAIL]

- Owner: `MaterialsHooks` — the per-composition point roster composing the kernel capsule: one `HookPoint<TFact>` field per point, one shared `IsolatedFault` evidence cell, no process-global registry — Materials holds no plugin-identity grant custody, so the kernel's composition-frozen point mount is the whole registry.
- Cases: `rasm.materials.catalogue.admit` veto (`CatalogueAdmit`), `rasm.materials.section.solve` observe (`SectionSolve`), `rasm.materials.capacity.check` observe (`CapacityCheck`), `rasm.materials.graph.compile` observe (`GraphCompile`), `rasm.materials.acquisition.fit` replay (`AcquisitionFit`), `rasm.materials.wire.mint` observe (`WireMint`), `rasm.materials.projection.project` veto (`ProjectionGate`).
- Entry: `MaterialsHooks.Live()` mints the roster once at composition; a decorator fires its declared point value, so a name-resolved lookup surface never exists; the capsule's `Veto`/`Observe`/`Drain` are the subscriber entries.
- Packages: Rasm, LanguageExt.Core.
- Growth: a new point is one field, one `Live()` row, and one `MaterialsFact` case.
- Boundary: fire order, veto folding, bounded replay, and fork-shielded isolation are the capsule's — a subscriber fault parks as `IsolatedFault` on the composition's cell and the emitter is untouched; one synchronous `Fire`, so an effect-composed decorator lifts at its own seam.

```csharp signature
public sealed record MaterialsHooks(
    HookPoint<MaterialsFact.CatalogueAdmit> CatalogueAdmit,
    HookPoint<MaterialsFact.SectionSolve> SectionSolve,
    HookPoint<MaterialsFact.CapacityCheck> CapacityCheck,
    HookPoint<MaterialsFact.GraphCompile> GraphCompile,
    HookPoint<MaterialsFact.AcquisitionFit> AcquisitionFit,
    HookPoint<MaterialsFact.WireMint> WireMint,
    HookPoint<MaterialsFact.ProjectionGate> ProjectionGate,
    Atom<Seq<IsolatedFault>> Faults) {
    public static MaterialsHooks Live() {
        var faults = Atom(Seq<IsolatedFault>());
        return new(
            new(HookId.Create("rasm.materials.catalogue.admit"), HookModality.Veto, faults),
            new(HookId.Create("rasm.materials.section.solve"), HookModality.Observe, faults),
            new(HookId.Create("rasm.materials.capacity.check"), HookModality.Observe, faults),
            new(HookId.Create("rasm.materials.graph.compile"), HookModality.Observe, faults),
            new(HookId.Create("rasm.materials.acquisition.fit"), HookModality.Replay, faults),
            new(HookId.Create("rasm.materials.wire.mint"), HookModality.Observe, faults),
            new(HookId.Create("rasm.materials.projection.project"), HookModality.Veto, faults),
            faults);
    }
}
```

## [04]-[INSTRUMENT_TAP]

- Owner: `MaterialsInstruments` — the `rasm.materials.*` roster, the contributor port, and the rail-subscribed projection; level rows read the composition's `LevelCells`.
- Cases: solve counts and duration off `SectionSolve`; capacity verdict counts and utilisation off `CapacityCheck`; compile duration off `GraphCompile`; fit residual off `AcquisitionFit`; catalogue and library row levels off the freeze fold; fault counts off the rail's `IsolatedFault` cell banded by kernel category.
- Entry: `MaterialsInstruments.Telemetry(LevelCells cells, string version, string schemaUrl)` — the one contributor port (scope `Rasm.Materials`); `MaterialsInstruments.Tap(MaterialsHooks hooks, InstrumentSet set, LevelCells cells)` mounts the observe subscriptions at composition, so create and write calls live only inside this spine.
- Auto: histogram rows bind the kernel `Buckets` rows (`SolveSeconds`, `CompileSeconds`, `ProfileSeconds`, `GoverningRatio`, `ResidualDecades`); capacity tags carry kind, governing action, and adequacy; acquisition tags carry capture method; wire tags carry the bounded material and method keys.
- Packages: Rasm, LanguageExt.Core, BCL inbox (`System.Diagnostics.Metrics`).
- Growth: a histogram policy change is one kernel `Buckets` row reference; a new instrument is one roster row and one tap arm.
- Boundary: level cells are composition-owned and threaded, so app-scoped isolation holds by construction; live facts and replay envelopes remain mutually exclusive evidence paths at the composition root; instrument custody is one-per-composition — either the composing app materializes the port through its fan or this package's spine mounts the rows, never both.

```csharp signature
public static class MaterialsInstruments {
    public const string Scope = "Rasm.Materials";

    public static Seq<InstrumentRow> Rows(LevelCells cells) => Seq(
        new InstrumentRow("rasm.materials.section.solves", "{solve}", "profile section solves by profile case",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.materials.section.duration", "s", "profile section solve wall duration",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.SolveSeconds)),
        new InstrumentRow("rasm.materials.capacity.checks", "{check}", "capacity checks by kind, governing action, and adequacy",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.materials.capacity.utilisation", "1", "governing utilisation ratio per capacity check",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.GoverningRatio)),
        new InstrumentRow("rasm.materials.graph.duration", "s", "material graph compile wall duration",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.CompileSeconds)),
        new InstrumentRow("rasm.materials.acquisition.residual", "1", "acquisition fit RMS residual",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.ResidualDecades)),
        new InstrumentRow("rasm.materials.catalogue.rows", "{row}", "frozen catalogue row population",
            (meter, name, unit, text) => meter.CreateObservableGauge(name, cells.Reader(name), unit, text)),
        new InstrumentRow("rasm.materials.library.rows", "{row}", "admitted library row population",
            (meter, name, unit, text) => meter.CreateObservableGauge(name, cells.Reader(name), unit, text)),
        new InstrumentRow("rasm.materials.faults", "{fault}", "veto refusals and isolated tap faults by category",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)));

    public static TelemetryContributorPort Telemetry(LevelCells cells, string version, string schemaUrl) =>
        new(Scope, version, schemaUrl, Rows(cells));

    public static Seq<IDisposable> Tap(MaterialsHooks hooks, InstrumentSet set, LevelCells cells) {
        AtomChangedEvent<Seq<IsolatedFault>> rejected = held => held.Last.Iter(fault =>
            ignore(set.Count("rasm.materials.faults", 1L, new KeyValuePair<string, object?>("category", fault.Cause.Category()))));
        hooks.Faults.Change += rejected;
        return Seq<IDisposable>(
            hooks.SectionSolve.Observe(fact => IO.lift(() => {
                ignore(set.Count("rasm.materials.section.solves", 1L, new KeyValuePair<string, object?>("profile", fact.Profile)));
                return set.Record("rasm.materials.section.duration", fact.Elapsed.TotalSeconds);
            })),
            hooks.CapacityCheck.Observe(fact => IO.lift(() => {
                KeyValuePair<string, object?>[] tags = [
                    new("kind", CapacityKindOf(fact.Receipt)),
                    new("governing", fact.Verdict.Governing.Key),
                    new("adequate", fact.Verdict.Adequate),
                ];
                ignore(set.Count("rasm.materials.capacity.checks", 1L, tags));
                return fact.Verdict.Switch(
                    bounded: verdict => set.Record("rasm.materials.capacity.utilisation", verdict.Value, tags),
                    requiresMemberCheck: verdict => set.Record("rasm.materials.capacity.utilisation", verdict.Value, tags),
                    overcapacity: static _ => unit);
            })),
            hooks.GraphCompile.Observe(fact => IO.lift(() =>
                set.Record("rasm.materials.graph.duration", fact.Elapsed.TotalSeconds))),
            hooks.AcquisitionFit.Observe(fact => IO.lift(() =>
                set.Record("rasm.materials.acquisition.residual", fact.Receipt.FitResidual,
                    new KeyValuePair<string, object?>("method", fact.Receipt.Method.Key)))),
            new HookDetacher(() => hooks.Faults.Change -= rejected));
    }

    static string CapacityKindOf(CapacityReceipt receipt) => receipt.Switch(
        steel: static _ => nameof(CapacityReceipt.Steel),
        timber: static _ => nameof(CapacityReceipt.Timber),
        masonry: static _ => nameof(CapacityReceipt.Masonry),
        reinforcedMasonry: static _ => nameof(CapacityReceipt.ReinforcedMasonry),
        glass: static _ => nameof(CapacityReceipt.Glass),
        weld: static _ => nameof(CapacityReceipt.Weld),
        adhesive: static _ => nameof(CapacityReceipt.Adhesive),
        stud: static _ => nameof(CapacityReceipt.Stud),
        connector: static _ => nameof(CapacityReceipt.Connector));
}
```

## [05]-[FAULT_LOG]

- Owner: `MaterialsLatency` — the checkpoint vocabulary and measured bracket over eager constructions; the logging contract owns fixed declaration severity and remains blocked by `[LOGLEVEL_WARNING_MEMBER]`.
- Entry: `MaterialsLatency.Measured(ILatencyContext ledger, CheckpointToken started, CheckpointToken settled, Func<Fin<T>> body)` brackets one eager construction between two checkpoints.
- Auto: checkpoint names register once at the app root through `RegisterCheckpointNames` and tokens resolve once through `ILatencyContextTokenIssuer.GetCheckpointToken`, so an unregistered name is a composition-time refusal.
- Packages: Microsoft.Extensions.Telemetry.Abstractions, LanguageExt.Core.
- Growth: a new eager construction is one checkpoint pair in `Checkpoints`.
- Boundary: libraries take the latency ledger by injection. Settlement records in `finally`, so failed or throwing constructions close the same bracket as successful constructions.

```csharp signature
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
        try { return body(); }
        finally { ledger.AddCheckpoint(settled); }
    }
}
```

## [06]-[DESCRIPTOR_ROWS]

- Owner: `PanelKind` and `AlertSeverity` — the bounded descriptor vocabularies; `PanelRow` and `AlertRow` — the typed descriptor shapes; `MaterialsDescriptors` — the panel and alert roster over the instrument names.
- Entry: `MaterialsDescriptors.Panels` and `Alerts` are the data the IaC compile leg decodes into boards and burn-rate rules, so board truth derives from the mounted roster and the compile leg authors nothing.
- Auto: every descriptor names an instrument on the `[04]` roster; an alert threshold is a burn-fraction hint the SLO algebra consumes, never a provisioned rule body.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new board panel or alert is one row; a new descriptor shape is one field on the owning row record.
- Boundary: dashboards, alert provisioning, tenancy, and the burn-rate algebra are the IaC plane's — this page mints descriptor data behind the same `rasm.materials.*` names the instruments carry and never a query string, board JSON, or provider type.

```csharp signature
[SmartEnum<string>]
public sealed partial class PanelKind {
    public static readonly PanelKind Timeseries = new("timeseries");
    public static readonly PanelKind Stat = new("stat");
    public static readonly PanelKind Heatmap = new("heatmap");
}

[SmartEnum<string>]
public sealed partial class AlertSeverity {
    public static readonly AlertSeverity Page = new("page");
    public static readonly AlertSeverity Warn = new("warn");
}

public sealed record PanelRow(string Title, string Instrument, PanelKind Kind);

public sealed record AlertRow(string Name, string Instrument, double BurnFraction, long WindowSeconds, AlertSeverity Severity);

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
        new AlertRow("materials-fault-burn", "rasm.materials.faults", 0.02d, WindowSeconds: 1_800, AlertSeverity.Page),
        new AlertRow("materials-inadequate-capacity", "rasm.materials.capacity.checks", 0.10d, WindowSeconds: 7_200, AlertSeverity.Warn),
        new AlertRow("materials-fit-residual", "rasm.materials.acquisition.residual", 0.05d, WindowSeconds: 21_600, AlertSeverity.Warn));
}
```

## [07]-[RESEARCH]

- [LOGLEVEL_WARNING_MEMBER]-[BLOCKED]: which exact `LogLevel` member declares the fixed warning severity on the four banded `[LoggerMessage]` partials; `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Materials/.api/api-logging-abstractions.md`, then `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/.api/api-logging-abstractions.md`.
