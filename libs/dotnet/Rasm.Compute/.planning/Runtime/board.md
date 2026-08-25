# [COMPUTE_BOARD]

Rasm.Compute contributes ONE dashboard-and-reliability pack: panels derive from the `Runtime/receipts#RECEIPT_UNION` instrument roster, objectives bind a mounted counter pair to a typed receipt-case sampler, and both travel to the composing root as data rows on the kernel contributor port. Indicator shapes, the burn table, severity routing, the verdict fold, the alert spec, and both descriptor carriers are the kernel SLO algebra composed whole — this owner mints no second panel truth, no second burn table, and no rendered JSON.

The hook rail lives here too, because a hook point is an evidence surface a board reads: five typed points on the kernel hook capsule, and the isolation cell whose parked subscriber faults publish as a pulled level beside every pushed row.

## [01]-[INDEX]

- [02]-[FACT_SELECTION]: `FactSelector` — the typed case-and-predicate sampler over the fact stream.
- [03]-[OBJECTIVES]: `ComputeObjective` — one objective binding a kernel indicator to a population/breach selector pair minted off ONE type argument.
- [04]-[PANEL_PROJECTION]: `PanelRow`, `ComputeDescriptors` — the pack derived from the instrument roster and the objective roster, and its wire projection.
- [05]-[HOOK_POINTS]: `ConvergenceMark`, `ComputeHookRail` — the five-point compute hook roster on the kernel capsule, its fire sites, and its isolation-cell reading.
- [06]-[TS_PROJECTION]: `BoardWireMap` — the generated `rasm.contracts.board` binding the pack crosses as, and its one `ComputeDescriptors` door.

## [02]-[FACT_SELECTION]

- Owner: `FactSelector` — one erased predicate over `ComputeReceipt`, minted from a typed case predicate.
- Entry: `FactSelector.Of<TCase>(Func<TCase, bool>? holds = null)` mints the selector; `Count(Seq<ComputeReceipt>)` answers the population size.
- Auto: typed case selection replaces a stringly field matcher — the population IS a receipt case and the predicate reads that case's own fields, so a renamed payload field breaks at compile time instead of silently selecting nothing, and selectors never cross the wire.
- Receipt: none.
- Packages: LanguageExt.Core, BCL inbox
- Growth: a new sampler is one `Of<TCase>` call at its objective row; zero new surface.
- Law: registry membership is proved AT CONSTRUCTION and travels as nothing. `ReceiptSurface.KindOf` resolves the case against the frozen `[JsonDerivedType]` roster and throws on a case that roster never declared, so an objective naming an unregistered case has no construction path. NAMED LOSS: the `Kind` column is retired — its stated purpose was a boot probe, no probe on any page ever read it, and the construction-time resolution it was derived from is the same proof carried out one step earlier. `Runtime/receipts#FOLD_PROJECTIONS` `ReceiptFolds.Cases<TCase>` is this correspondence's un-erased twin, taken where the predicate need not survive as a value.
- Boundary: an unregistered case fails where the selector is CONSTRUCTED rather than at a boot probe restating it, so the failure names the objective row rather than a roster count.

```csharp signature
public sealed record FactSelector(Func<ComputeReceipt, bool> Holds) {
    public static FactSelector Of<TCase>(Func<TCase, bool>? holds = null) where TCase : ComputeReceipt {
        ignore(ReceiptSurface.KindOf(typeof(TCase)));
        return new(fact => fact is TCase held && (holds is null || holds(held)));
    }

    public long Count(Seq<ComputeReceipt> facts) => facts.Filter(Holds).Count;
}
```

## [03]-[OBJECTIVES]

- Owner: `ComputeObjective` — one kernel `Objective` bound to a population-and-breach selector pair over ONE receipt case.
- Entry: `ComputeObjective.Of<TCase>(Objective objective, Func<TCase,bool> breached, Func<TCase,bool>? within = null)` mints the pair behind a private constructor; `Sample(Seq<ComputeReceipt> window)` answers one `SloSample`; `Verdict(Func<Duration, Seq<ComputeReceipt>> window)` folds the kernel burn table over the long and short windows.
- Auto: each objective binds a mounted counter pair as its ratio indicator and a typed selector pair as its in-process sampler, so the store-side burn rule and the live verdict divide the same evidence; the four multiwindow burn rows, both severities, the budget share, and every annotation derive from the kernel table, so a factor change moves verdict, alert, and dashboard in one edit at one owner.
- Receipt: none — an objective is a projection of the fact vocabulary.
- Packages: LanguageExt.Core, NodaTime, Rasm (kernel signal capsule — `Objective`, `Sli`, `BurnRow`, `BurnReading`, `SloSample`, `SloVerdict`, `Slo`), BCL inbox
- Growth: a new objective is one `Bound<TCase>` row naming its scored receipt case, indicator series, target, and breach predicate; a fifth indicator shape is one kernel `Sli` case breaking every consumer at compile time; zero new surface.
- Law: ONE type argument mints BOTH views, so population and breach are two predicates over one receipt case BY CONSTRUCTION. A pair naming two cases samples a breach against a population it can never intersect and reports a permanent zero — a green indicator over an unmeasured objective, the worst reading this plane can produce — and the private constructor leaves this the only path, so the mismatch has no construction site rather than a runtime refusal restating it. `within` narrows the population when an objective scores a slice of its case.
- Law: `Breaching` filters the ALREADY-filtered population, so the sample's `Breaching <= Total` claim holds by construction at the one seam that mints it and no consumer re-proves it.
- Boundary: `Slo.Specs` is the kernel's compilation-ready projection and this owner forwards nothing to it — a consumer holding the pack reads `BoardPack.Alerts` in one hop, so the two zero-caller forwarding members that stood between them are retired.

```csharp signature
public sealed record ComputeObjective {
    private ComputeObjective(Objective objective, FactSelector population, FactSelector breach) =>
        (Objective, Population, Breach) = (objective, population, breach);

    public Objective Objective { get; }

    public FactSelector Population { get; }

    public FactSelector Breach { get; }

    public static ComputeObjective Of<TCase>(
        Objective objective, Func<TCase, bool> breached, Func<TCase, bool>? within = null)
        where TCase : ComputeReceipt =>
        new(objective, FactSelector.Of<TCase>(within), FactSelector.Of<TCase>(breached));

    public SloSample Sample(Seq<ComputeReceipt> window) {
        Seq<ComputeReceipt> total = window.Filter(Population.Holds);
        return new SloSample(Breaching: Breach.Count(total), Total: total.Count);
    }

    public SloVerdict Verdict(Func<Duration, Seq<ComputeReceipt>> window) =>
        Slo.Evaluate(Objective, row => new BurnReading(Long: Sample(window(row.Long)), Short: Sample(window(row.Short))));
}
```

## [04]-[PANEL_PROJECTION]

- Owner: `PanelRow` the wire projection of one pack panel; `ComputeDescriptors` the ONE derivation from the instrument roster and the objective roster into the kernel `BoardPack`.
- Entry: `ComputeDescriptors.Board` — the kernel `BoardPack` carrying one `PanelSpec` per `Runtime/receipts#RECEIPT_UNION` `ComputeInstrument` row beside every objective; `ComputeDescriptors.Panels` — the wire projection of that pack, each row carrying its title, break keys, widget, unit, and bucket ladder; `ComputeDescriptors.Board.Alerts` — the pack's compilation-ready specs. Admission is the kernel pack's whole and reaches the composing root on the `Runtime/receipts#RECEIPT_UNION` contributor port, so this owner exposes no probe entry.
- Auto: panels derive from `ComputeInstrument.Rows`, so descriptor truth structurally cannot drift from the mounted roster — a new instrument row is a new panel with zero descriptor edit — and each panel's break keys are its declaring row's `Dimensions`, so the tag vocabulary a writer spells is the vocabulary the board splits on.
- Receipt: none — the descriptor is a projection of the spec roster and the fact vocabulary; a hand-authored board beside it is the drift the projection deletes.
- Packages: LanguageExt.Core, NodaTime, Rasm (kernel signal capsule — `PanelSpec`, `PanelKind`, `BoardPack`, `AlertSpec`, `Buckets`, `InstrumentKind`), BCL inbox
- Growth: a new panel is the instrument row it derives from; a panel wanting a non-default widget or a narrower break set overrides on its own `PanelSpec` row; zero new surface.
- Law: a ladder crosses WITH its unit. `Buckets` carries the UCUM unit its boundaries measure and the kernel proves that unit against the declaring row's own, so the panel row carries the ladder ROW rather than a bare boundary array a renderer would have to guess the quantity of.
- Law: `Objective.Create` throws on a malformed row, and every row here is a `static readonly` roster entry — so a bad name, target, or window breaks at TYPE INITIALIZATION, which is the compile-adjacent proof a rail-typed factory over a fixed roster could not give.
- Boundary: descriptor rows emit during the descriptor build under the suite schema hash beside `ReceiptSurface.Kinds` and cross only as the generated message `[06]-[TS_PROJECTION]` names; the ts-iac compile leg (`typescript:iac/operate/observe#BOARD_APPLY`) owns turning rows into Foundation-SDK dashboards and rule groups — Compute owns no IaC surface and renders nothing.
- Boundary: `BoardPack.Admit` carries every claim this pack owes — panel widgets and break keys, indicator series and partition keys, and objective-name distinctness across the alert namespace — so an alert can never name a series the meter never mounts, a panel can never break on a key its row never declares, and a folder-local probe restating any of them is the deleted form. Two further proofs are STRUCTURAL and probing them tests nothing: `FactSelector.Of` resolves its kind through the frozen registry so an objective naming an unregistered case has no construction path, and `ComputeObjective.Of` mints the population-and-breach pair off ONE type argument behind a private constructor so a pair spanning two cases has none either.
- Boundary: omitting the window canonicalizes it at the kernel to the estate compliance default, so no calendar literal lands in a descriptor row and a shortened window still refuses below the longest burn row; a hand-typed window, factor, or severity beside the kernel table is the forked form that silently diverges from every sibling descriptor plane on the next tuning.

```csharp signature
public sealed record PanelRow(
    string Title, string Instrument, string Unit, InstrumentKind Measure, PanelKind Panel,
    Seq<string> By, Option<Buckets> Ladder);

public static class ComputeDescriptors {
    public static readonly Seq<ComputeObjective> Objectives = Seq(
        Bound<ComputeReceipt.Solve>("compute.solve-convergence",
            new Sli.Ratio(ComputeInstrument.SolveConverged.Key, ComputeInstrument.SolveRuns.Key), 0.99d,
            static solve => !solve.Converged),
        Bound<ComputeReceipt.RemoteCall>("compute.remote-call",
            new Sli.Ratio(ComputeInstrument.RemoteOk.Key, ComputeInstrument.RemoteCalls.Key), 0.999d,
            static call => !StringComparer.Ordinal.Equals(call.Status, ReceiptSurface.OkStatus)),
        Bound<ComputeReceipt.Backpressure>("compute.backpressure",
            new Sli.Ratio(ComputeInstrument.BackpressureAdmitted.Key, ComputeInstrument.BackpressureVerdicts.Key), 0.999d,
            static queued => queued.Verdict is BackpressureVerdict.Shed),
        Bound<ComputeReceipt.Twin>("compute.twin-anomaly",
            new Sli.Ratio(ComputeInstrument.TwinNominal.Key, ComputeInstrument.TwinVerdicts.Key), 0.95d,
            static twin => twin.Anomaly),
        Bound<ComputeReceipt.Trajectory>("compute.trajectory-resolution",
            new Sli.Ratio(ComputeInstrument.TrajectoryResolved.Key, ComputeInstrument.TrajectoryRuns.Key), 0.99d,
            static run => !run.Resolved));

    static readonly Seq<(PanelSpec Panel, InstrumentSpec Row)> Descriptors =
        ComputeInstrument.Rows.Map(static row => (PanelSpec.Of(row.Description, row.Name, [.. row.Dimensions]), row)).Strict();

    public static readonly BoardPack Board = new(
        Wire: "compute.receipt",
        Panels: Descriptors.Map(static entry => entry.Panel).Strict(),
        Objectives: Objectives.Map(static row => row.Objective).Strict());

    public static Seq<PanelRow> Panels =>
        Descriptors.Map(static entry => new PanelRow(
            entry.Panel.Title, entry.Panel.Instrument, entry.Row.Unit, entry.Row.Kind,
            entry.Panel.Widget.IfNone(PanelKind.For(entry.Row.Kind)), entry.Panel.By, entry.Row.Bounds)).Strict();

    public static BoardPackWire Wire => BoardWireMap.ToWire(Board, Panels);

    static ComputeObjective Bound<TCase>(
        string name, Sli sli, double target, Func<TCase, bool> breached, Func<TCase, bool>? within = null)
        where TCase : ComputeReceipt =>
        ComputeObjective.Of(Objective.Create(name: name, sli: sli, target: target, window: default), breached, within);
}
```

## [05]-[HOOK_POINTS]

- Owner: `ComputeHookRail` — the five-point compute hook roster on the kernel hook capsule, one typed `HookPoint<TFact>` per point, declared once and mounted into the one `HookRegistry` at composition; `ConvergenceMark` — the solve-iteration evidence struct the replay point buffers.
- Cases: `Admit` `rasm.compute.runtime.admit` (Veto over `AdmittedIntent` — policy transform-or-reject before `Plan`) · `Dispatch` `rasm.compute.runtime.dispatch` (Observe over `SelectionReceipt` — substrate-keyed tap beside the `Runtime/receipts#TELEMETRY_PROJECTION` dispatch span) · `Iteration` `rasm.compute.solve.iteration` (Replay over `ConvergenceMark`, depth 256 — a late UI subscriber drains the recent convergence window) · `Writeback` `rasm.compute.assessment.writeback` (Veto over `GraphDelta` — gate before the caller applies the assessment delta) · `Control` `rasm.compute.twin.control` (Veto over `TwinVerdict` — gate before the control suggestion crosses to the AppHost write-back as `ExternalValue`).
- Entry: `ComputeHookRail.Live()` mints the roster; `HookRegistry.Mount` at the composition root folds these points beside the AppHost `HookRail` rows into one frozen table, so a duplicate id dies structurally at composition and subscription reaches a point only through its declared rail field — a name-resolved lookup surface never exists. `Isolated(InstrumentSet)` registers the parked-fault reading and returns the scope that retires exactly that registration.
- Auto: domain code fires evidence and observability subscribes — `Planned` runs the admit veto fold on the emitter's own rail before `SubstrateSelection.Plan` so the first refusing gate short-circuits with its typed fault and a transform threads forward; `Ran` fires the dispatch tap with the `SelectionReceipt` before `DispatchTable.Run` so the tap observes the identical evidence the dispatch span tags, and the run's own `Fin<ComputeReceipt>` verdict threads out unerased; `Marked` folds a `ConvergenceMark` into the bounded replay buffer through the same cadence gate the `rasm.compute.progress.cadence` law meters, so a hot solver never floods the buffer; `Applied` and `Suggested` run the writeback and control veto folds where the delta and the verdict leave the package.
- Receipt: none — a hook fire is the evidence event itself; the emitter's own receipt already carries the fact, and an instrument write for hook evidence subscribes as an observe tap on the mounted fan, never an emit call added in domain code.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm (kernel signal capsule — `HookPoint<TFact>`, `HookId`, `HookModality`, `HookRegistry`, `IsolatedFault`, `InstrumentSet`, `Op`), Rasm.Element (project — `GraphDelta`), BCL inbox
- Growth: a new compute hook is one `HookPoint<TFact>` field on the rail record and one `Mount` row, its id admitted through the `HookId.Validate` four-segment grammar; a new subscriber is one `Observe`/`Veto` call at composition; zero new surface.
- Law: the isolation cell HAS a reader. A parked subscriber fault is evidence no emit path carries, so the cell publishes as the `rasm.compute.hook.isolated` PULLED level off the cell itself rather than a push every fire site would have to remember — and because `[04]-[PANEL_PROJECTION]` derives one panel per instrument row, the parked-fault census reaches the board with zero descriptor edit. Before that binding the cell was a declared capability nothing read.
- Boundary: subscriber-fault isolation is the kernel capsule law composed whole — every observe delivery runs fork-shielded, so a throwing or failing tap parks on the roster's evidence cell and never touches the emitter's `Fire` result; a faulting UI subscriber structurally cannot fail a solve. A veto refusal is the point's contract, returned on the emitter's rail as the veto's own typed fault.
- Boundary: payload types close at declaration — every `TFact` is a typed record already settled on its owning page, so a stringly payload cannot enter the rail; the rail adds no second emit path — `ReceiptSurface.Emit` stays the one sink leg and the `Runtime/receipts#TELEMETRY_PROJECTION` fold stays the one instrument projection, hook taps observing evidence beside them; a tap that must never lose an event is a durable outbox consumer, never a hook subscriber; ids are registry-enforced unique, so two apps compose disjoint hook sets without collision.

```csharp signature
public readonly record struct ConvergenceMark(CorrelationId Correlation, string Physics, int Iteration, double Residual);

public sealed record ComputeHookRail(
    HookPoint<AdmittedIntent> Admit,
    HookPoint<SelectionReceipt> Dispatch,
    HookPoint<ConvergenceMark> Iteration,
    HookPoint<GraphDelta> Writeback,
    HookPoint<TwinVerdict> Control,
    Atom<Seq<IsolatedFault>> Faults) {
    public static ComputeHookRail Live() {
        var faults = Atom(Seq<IsolatedFault>());
        return new(
            new(HookId.Create("rasm.compute.runtime.admit"), HookModality.Veto, faults),
            new(HookId.Create("rasm.compute.runtime.dispatch"), HookModality.Observe, faults),
            new(HookId.Create("rasm.compute.solve.iteration"), HookModality.Replay, faults, depth: 256),
            new(HookId.Create("rasm.compute.assessment.writeback"), HookModality.Veto, faults),
            new(HookId.Create("rasm.compute.twin.control"), HookModality.Veto, faults),
            faults);
    }

    public Seq<IHookPoint> Points => Seq<IHookPoint>(Admit, Dispatch, Iteration, Writeback, Control);

    public Fin<IDisposable> Isolated(InstrumentSet set) =>
        set.Bind(ComputeInstrument.HookIsolated.Row, () => Faults.Value.Count, Op.Of(name: "compute.hook.isolated"));

    public Fin<Seq<SelectionReceipt>> Planned(AdmittedIntent admitted, SelectionContext context) =>
        Admit.Fire(admitted).Bind(gated => SubstrateSelection.Plan(gated, context));

    public IO<Fin<ComputeReceipt>> Ran(DispatchTable table, SelectionReceipt selection, AdmittedIntent admitted) =>
        IO.lift(() => ignore(Dispatch.Fire(selection))).Bind(_ => table.Run(selection, admitted));

    public Unit Marked(ConvergenceMark mark) => ignore(Iteration.Fire(mark));

    public Fin<GraphDelta> Applied(GraphDelta delta) => Writeback.Fire(delta);

    public Fin<TwinVerdict> Suggested(TwinVerdict verdict) => Control.Fire(verdict);
}
```

## [06]-[TS_PROJECTION]

- Owner: `BoardWireMap` — the ONE `[Mapper]` seam projecting the pack onto the generated `rasm.contracts.board.BoardPackWire`; `ComputeDescriptors.Wire` is the one door that composes it, and no hand TS interface, literal union, or JSON shape lives anywhere on this page.
- Entry: `BoardWireMap.ToWire(BoardPack pack, Seq<PanelRow> panels)` takes BOTH halves because the kernel `PanelSpec` on the pack carries the policy half alone — which instrument, broken on which keys, under which widget — while the wire panel carries the instrument facts a renderer resolves nothing for; `ComputeDescriptors.Wire` is the composition of the two reads the descriptor fold already mints.
- Auto: each row-to-enum fold IS the kernel row's generated total `Switch`, so a kernel row landed without its proto enum value breaks THIS mapper at compile time and a hand `(key, enum)` table has nowhere to live; the mapper runs `RequiredMappingStrategy.Both`, so a wire column no source member fills and a source member no column reads each break the build rather than crossing empty.
- Law: the projection is the LAST step. `BoardPack.Admit` proves panel widgets, break keys, indicator series, partition keys, and objective-name distinctness against the roster BEFORE the pack reaches this owner, so the mapper never re-proves a claim and never refuses — an unadmitted pack has no path to the wire.
- Law: the pack crosses as ProtoJSON and this owner formats nothing — the composing root writes `WireJson.Write(ComputeDescriptors.Wire, sink)` at the one AppHost codec door, so the descriptor build emits one document under the suite schema hash and no second serializer stands beside it.
- Law: factor, windows, and hold do NOT cross. `burn` and `severity` ride the generated enums, and each consumer reads factor, long window, short window, dwell, and routing posture off ITS OWN burn and severity rows keyed by that enum — a tuned kernel factor moves every plane with no wire edit, where crossing the derived numbers would freeze one branch's tuning into the other's rules.
- Law: alerts derive from `BoardPack.Alerts` — the pack's own `Objectives.Bind(Slo.Specs)` — and the objective each names rides `Slo.ObjectiveSlot` on the spec's own annotations, so the wire's `objective` column reads the kernel annotation rather than splitting a slug; a spec reaching here without that row carries its slug instead, whose colon fails the column's own pattern at admission.
- Packages: Rasm.Contracts (project — generated `board` family), Riok.Mapperly, Google.Protobuf, NodaTime.Serialization.Protobuf, Rasm.AppHost (project — `WireJson`), Rasm (kernel signal capsule), LanguageExt.Core
- Growth: a sixth indicator shape is one kernel `Sli` case, one proto oneof arm, and one `Switch` arm here; a ninth panel row or a fifth burn row is one enum value beside its kernel row, and every consumer re-derives.
- Boundary: the consumer fence is `typescript:core/observe/board#PACK_WIRE`, which lands `BoardPackWire` through the branch's one ProtoJSON arm and folds it into the value `typescript:iac/operate/observe#BOARD_APPLY` ingests; `compute.receipt` is the pack's FIRST column at both ends, so the provenance key and the pack are one value and no consumer tier originates a key its producer cannot stamp.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using NodaTime.Serialization.Protobuf;
using Riok.Mapperly.Abstractions;
using AlertSeverityWire = Rasm.Contracts.Board.AlertSeverity;
using AlertWire = Rasm.Contracts.Board.AlertWire;
using BoardPackWire = Rasm.Contracts.Board.BoardPackWire;
using BucketsWire = Rasm.Contracts.Board.BucketsWire;
using BurnRowWire = Rasm.Contracts.Board.BurnRow;
using DurationWire = Google.Protobuf.WellKnownTypes.Duration;
using InstrumentKindWire = Rasm.Contracts.Board.InstrumentKind;
using LevelBreachWire = Rasm.Contracts.Board.LevelBreach;
using ObjectiveWire = Rasm.Contracts.Board.ObjectiveWire;
using PanelKindWire = Rasm.Contracts.Board.PanelKind;
using PanelWire = Rasm.Contracts.Board.PanelWire;
using SliWire = Rasm.Contracts.Board.SliWire;

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
public static partial class BoardWireMap {
    public static BoardPackWire ToWire(BoardPack pack, Seq<PanelRow> panels) {
        BoardPackWire wire = new() { Wire = pack.Wire };
        wire.Panels.AddRange(panels.Map(Panel));
        wire.Objectives.AddRange(pack.Objectives.Map(Objective));
        wire.Alerts.AddRange(pack.Alerts.Map(static spec => Alert(spec, Named(spec))));
        return wire;
    }

    [MapProperty(nameof(PanelRow.Panel), nameof(PanelWire.Widget))]
    private static partial PanelWire Panel(PanelRow row);

    [MapperIgnoreSource(nameof(Objective.Budget))]
    private static partial ObjectiveWire Objective(Objective objective);

    [MapperIgnoreSource(nameof(AlertSpec.Hold))]
    [MapperIgnoreSource(nameof(AlertSpec.Sli))]
    [MapperIgnoreSource(nameof(AlertSpec.Target))]
    [MapperIgnoreSource(nameof(AlertSpec.Annotations))]
    private static partial AlertWire Alert(AlertSpec spec, string objective);

    [UserMapping]
    private static SliWire Sli(Sli sli) => sli.Switch(
        ratio: static row => new SliWire { Ratio = new SliWire.Types.Ratio { Good = row.Good, Total = row.Total } },
        partition: static row => new SliWire {
            Partition = new SliWire.Types.Partition { Metric = row.Metric, By = row.By, Good = { row.Good } },
        },
        latency: static row => new SliWire {
            Latency = new SliWire.Types.Latency {
                Metric = row.Metric, Ceiling = row.Ceiling.ToProtobufDuration(), Quantile = row.Quantile,
            },
        },
        saturation: static row => new SliWire {
            Saturation = new SliWire.Types.Saturation { Metric = row.Metric, Bound = row.Bound, Breach = Breach(row.Breach) },
        },
        freshness: static row => new SliWire {
            Freshness = new SliWire.Types.Freshness { Metric = row.Metric, Horizon = row.Horizon.ToProtobufDuration() },
        });

    [UserMapping]
    private static BucketsWire? Ladder(Option<Buckets> ladder) => ladder.Match(
        Some: static row => new BucketsWire { Unit = row.Unit, Bounds = { row.Bounds } },
        None: static () => (BucketsWire?)null);

    [UserMapping]
    private static DurationWire Span(Duration span) => span.ToProtobufDuration();

    [UserMapping]
    private static PanelKindWire Widget(PanelKind panel) => panel.Switch(
        timeseries: static () => PanelKindWire.Timeseries,
        stat: static () => PanelKindWire.Stat,
        gauge: static () => PanelKindWire.Gauge,
        heatmap: static () => PanelKindWire.Heatmap,
        logs: static () => PanelKindWire.Logs,
        table: static () => PanelKindWire.Table,
        geomap: static () => PanelKindWire.Geomap,
        nodes: static () => PanelKindWire.Nodes);

    [UserMapping]
    private static InstrumentKindWire Measure(InstrumentKind kind) => kind.Switch(
        count: static () => InstrumentKindWire.Count,
        delta: static () => InstrumentKindWire.Delta,
        distribution: static () => InstrumentKindWire.Distribution,
        reading: static () => InstrumentKindWire.Reading,
        total: static () => InstrumentKindWire.Total,
        balance: static () => InstrumentKindWire.Balance,
        level: static () => InstrumentKindWire.Level,
        levels: static () => InstrumentKindWire.Levels);

    [UserMapping]
    private static AlertSeverityWire Severity(AlertSeverity severity) => severity.Switch(
        ticket: static () => AlertSeverityWire.Ticket,
        page: static () => AlertSeverityWire.Page);

    [UserMapping]
    private static BurnRowWire Burn(BurnRow row) => row.Switch(
        pageFast: static () => BurnRowWire.PageFast,
        pageSlow: static () => BurnRowWire.PageSlow,
        ticketFast: static () => BurnRowWire.TicketFast,
        ticketSlow: static () => BurnRowWire.TicketSlow);

    [UserMapping]
    private static LevelBreachWire Breach(LevelBreach breach) => breach.Switch(
        ceiling: static () => LevelBreachWire.Ceiling,
        floor: static () => LevelBreachWire.Floor);

    private static string Named(AlertSpec spec) =>
        spec.Annotations.Find(static row => string.Equals(row.Key, Slo.ObjectiveSlot, StringComparison.Ordinal))
            .Map(static row => row.Value).IfNone(spec.Slug);
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
