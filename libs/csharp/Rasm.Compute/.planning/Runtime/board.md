# [COMPUTE_BOARD]

Rasm.Compute contributes ONE dashboard-and-reliability pack: panels derive from the `Runtime/receipts#RECEIPT_UNION` instrument roster, objectives bind a mounted counter pair to a typed receipt-case sampler, and both travel to the composing root as data rows on the kernel contributor port. Indicator shapes, the burn table, severity routing, the verdict fold, the alert spec, and both descriptor carriers are the kernel SLO algebra composed whole — this owner mints no second panel truth, no second burn table, and no rendered JSON.

The hook rail lives here too, because a hook point is an evidence surface a board reads: five typed points on the kernel hook capsule, and the isolation cell whose parked subscriber faults publish as a pulled level beside every pushed row.

## [01]-[INDEX]

- [02]-[FACT_SELECTION]: `FactSelector` — the typed case-and-predicate sampler over the fact stream.
- [03]-[OBJECTIVES]: `ComputeObjective` — one objective binding a kernel indicator to a population/breach selector pair minted off ONE type argument.
- [04]-[PANEL_PROJECTION]: `PanelRow`, `ComputeDescriptors` — the pack derived from the instrument roster and the objective roster, and its wire projection.
- [05]-[HOOK_POINTS]: `ConvergenceMark`, `ComputeHookRail` — the five-point compute hook roster on the kernel capsule, its fire sites, and its isolation-cell reading.
- [06]-[TS_PROJECTION]: the pack crosses as a generated message the corpus mints, never a hand TS mirror.

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
    // Exemption: the registry read is a construction-time PROOF whose value nothing consumes — the frozen lookup
    // throws on an unregistered case, so the statement seam is what makes the proof precede the mint.
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
// Compute's evidence plane is its own fact stream, so the sampler is a selector pair while the burn windows,
// factors, severity routing, budget share, and spec derivation stay the kernel's single discipline.
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
- Boundary: descriptor rows emit during the descriptor build under the suite schema hash beside `ReceiptSurface.Kinds` and cross only as the generated message `[06]-[TS_PROJECTION]` names; the ts-iac compile leg (`typescript:iac` `[0014]`) owns turning rows into Foundation-SDK dashboards and rule groups — Compute owns no IaC surface and renders nothing.
- Boundary: `BoardPack.Admit` carries every claim this pack owes — panel widgets and break keys, indicator series and partition keys, and objective-name distinctness across the alert namespace — so an alert can never name a series the meter never mounts, a panel can never break on a key its row never declares, and a folder-local probe restating any of them is the deleted form. Two further proofs are STRUCTURAL and probing them tests nothing: `FactSelector.Of` resolves its kind through the frozen registry so an objective naming an unregistered case has no construction path, and `ComputeObjective.Of` mints the population-and-breach pair off ONE type argument behind a private constructor so a pair spanning two cases has none either.
- Boundary: omitting the window canonicalizes it at the kernel to the estate compliance default, so no calendar literal lands in a descriptor row and a shortened window still refuses below the longest burn row; a hand-typed window, factor, or severity beside the kernel table is the forked form that silently diverges from every sibling descriptor plane on the next tuning.

```csharp signature
// Wire projection of one pack panel: the kernel `PanelSpec` carries the policy half — which instrument, broken
// on which keys, under which widget — and these columns carry the instrument facts a renderer needs beside it,
// so the deploy plane renders from one row and resolves nothing against a meter it cannot reach.
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

    // One derivation from the spec roster: the kernel policy row and its wire projection are two reads of the
    // same pair, so a panel cannot sit on the pack and be missing from the wire or carry a different widget on
    // each side. Every panel breaks on its declaring row's own `Dimensions`, so the break vocabulary IS the
    // declaration and a hand-kept break list beside it has nothing to hold.
    static readonly Seq<(PanelSpec Panel, InstrumentSpec Row)> Descriptors =
        ComputeInstrument.Rows.Map(static row => (PanelSpec.Of(row.Description, row.Name, [.. row.Dimensions]), row)).Strict();

    // Panels and objectives travel as one kernel pack, so a roster change re-derives panels, alerts, and the
    // whole admission proof in one diff and no descriptor plane re-mints a panel carrier.
    public static readonly BoardPack Board = new(
        Wire: "compute.receipt", // the provenance key the deploy tuple admits this projection under; pack and key are one value
        Panels: Descriptors.Map(static entry => entry.Panel).Strict(),
        Objectives: Objectives.Map(static row => row.Objective).Strict());

    public static Seq<PanelRow> Panels =>
        Descriptors.Map(static entry => new PanelRow(
            entry.Panel.Title, entry.Panel.Instrument, entry.Row.Unit, entry.Row.Kind,
            entry.Panel.Widget.IfNone(PanelKind.For(entry.Row.Kind)), entry.Panel.By, entry.Row.Bounds)).Strict();

    // The scored case is the ONE type argument every row states, so the objective's two selectors cannot name two
    // cases; the window stays `default` so the kernel canonicalizes it to the estate compliance default.
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

    // The isolation cell's ONE reader: a pulled probe over the cell publishes the parked census without a push at
    // any fire site, and retiring the returned scope drops exactly this registration.
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

- Law: the board pack crosses to the deploy plane ONLY as a generated message the corpus mints — no hand TS interface, literal union, or method shape lives on this page, and the hand indicator, alert, and panel mirror that once stood here is retired with its arm-for-arm census. The corpus carries no `rasm.contracts.board` family today, so `ComputeDescriptors.Board` leaves the process as the kernel `BoardPack` value on the contributor port alone, and the descriptor-to-dashboard compile leg (`typescript:iac/operate/observe#BOARD_APPLY`) reads the generated schema the day the family lands — IDEAS `BOARD_PACK_FAMILY` names that corpus ripple. NAMED LOSS: no cross-language board wire exists until the family mints. Witness: the retired mirror had zero TS readers (`typescript:core/observe/board#PACKS` encodes packs in-process and names no wire type), so nothing decoded it.

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
