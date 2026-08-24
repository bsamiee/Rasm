# [ELEMENT_AUDIT]

`ModelAudit` grades model completeness in ONE fold over a frozen `ElementGraph`: per-discipline coverage ratios, a graded integrity finding stream, and the threshold policy a delivery gate reads as a single predicate. `ModelAudit` owns NEUTRAL structural integrity and coverage over the one graph while the `Rasm.Bim` `Review` `ModelHealth` owns IFC template and IDS semantics — the two COMPOSE on one model and never overlap, because a dangling bag reference is a graph fact any consumer names and a missing `Pset_WallCommon` row is an IFC claim only the schema owner makes.

`ModelAudit` holds ZERO authority: it mutates nothing, mints no node, and rails no fault of its own — every verdict is receipt data, so a model too broken to assemble still grades.

`ConstraintSeverity` grades each finding, `ContentAddress.OfGraph` pins the audited snapshot, and each finding retains self-contained evidence from its producing sweep.

## [01]-[INDEX]

- [02]-[COVERAGE_CENSUS]: `CoverageRatio` the population/covered pair with its derived share, `DisciplineCoverage` the assessed-versus-substantiated split, `CoverageCensus` the model-wide row set keyed per `Discipline`, and the ONE occurrence fold that carries every counter and every bake verdict in a single pass.
- [03]-[INTEGRITY_SWEEP]: `AuditCategory` the graded taxonomy, `AuditEvidence` the semantic-detail/exact-refusal evidence union, `AuditFinding` the census row, and the seven sweeps — orphan nodes off the incidence closure, dangling bag references, the `Compose` acyclicity proof over the filtered tagged view, empty occurrence representations, source-bound `Connect` interfaces, non-current assessments, and the `ContentAddress.Verify` drift census.
- [04]-[AUDIT_FOLD]: `AuditThresholds` the delivery-policy row, `ModelAudit` the receipt with its `Blocking`/`Tallies`/`Clears` derived reads, and `ModelAudit.Of` the entry that composes the census and every sweep into one graded value.

## [02]-[COVERAGE_CENSUS]

- Owner: `CoverageRatio` the `(Population, Covered)` pair whose share DERIVES; `DisciplineCoverage` the per-discipline assessed-and-substantiated pair; `CoverageCensus` the model-wide census over the occurrence population, holding the four structural shares beside the discipline-keyed rows.
- Law: coverage reads the BAKED element, never a raw incidence scan. `Bake` applies the `Assign.TypeDefinition` inheritance and the `InheritanceMode` bag merge, so a takeoff bound to a Type COVERS every occurrence of that type — the raw scan under-reports exactly the modelling practice the IFC `QTO_TYPEDRIVENOVERRIDE` rule exists to license, and it reads a correctly-authored catalogue model as un-quantified.
- Law: the four structural shares carry NO discipline. `Graph/element#NODE_MODEL` `Object` declares no discipline column — the `Classification/classification#DISCIPLINE_AXIS` axis keys assessments and material property sets, never objects — so a per-discipline classified or quantified share partitions a population the graph does not partition. Discipline lands exactly where it is spellable: the assessment read and the material-property read, one row per `Discipline`.
- Entry: the census is folded, never constructed — `[04]` `ModelAudit.Of` runs the one pass and the receipt carries the result.
- Auto: the population is the OCCURRENCE roster (`ObjectKind.Occurrence`), because a Type is a catalogue definition and grading it beside its instances double-counts the shared data it exists to share. Every `Discipline` row seeds at zero population, so a discipline no occurrence touches reads a vacuous full share rather than an absent key a gate must special-case. `Bake` railing on an element contributes its verdict to the finding stream and no counter — an element that cannot be read cannot be graded.
- Receipt: `CoverageCensus` is the completeness half of the `ModelAudit` receipt; a report reads the counts, a gate reads the shares, and neither re-walks the graph.
- Packages: LanguageExt.Core (`Seq`/`HashMap`/`Option`/`Fin` + the `Fold` state thread), `Graph/element#ELEMENT_GRAPH` (`ElementGraph.ObjectNodes`/`Bake` and the baked `Element` flat reads), `Classification/classification#DISCIPLINE_AXIS` (`Discipline`), `Composition/material#MATERIAL_PROPERTY` (`MaterialPropertySet.Discipline`), `Assessment/assessment#ASSESSMENT_NODE` (`AssessmentPayload.Discipline`/`Outcome` with the `Usable` column).
- Growth: a new structural share is one `CoverageRatio` column with one predicate in the fold; a new per-discipline axis is one `DisciplineCoverage` column; a new discipline is one `Discipline` row the seed absorbs with no edit here. `ModelAudit.Of` runs the ONE pass, so a second fold over the same population is the deleted form — a new counter joins that pass or it does not exist.
- Boundary: a vacuous population reads FULLY covered. Empty models never trip a gate built to catch a half-classified one, and `0/0` admits no other honest answer; a gate that means "and the model is non-empty" states that as its own blocking-count ceiling.
- Boundary: `Quantified` and `Propertied` require a NON-EMPTY bag, not a bound one — an `Assign.PropertyDefinition` edge onto an empty bag is a binding, not evidence, and counting it reads the scaffolding as the content.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Element.Assessment;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Generator.Equals;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Projection;

// --- [MODELS] -----------------------------------------------------------------------------
// Share DERIVES — a stored ratio beside its two counts is the double-store defect, and the two audiences want
// different halves: a report names "37 of 214 occurrences", a gate compares 0.173 against a floor.
public readonly record struct CoverageRatio(int Population, int Covered) {
    // The ONE vacuous-truth value: an empty population claims full share, and every absent-key read returns THIS
    // rather than a bare 1.0 literal re-deciding vacuity per site.
    public static readonly CoverageRatio Vacuous = new(0, 0);

    public double Share => Population == 0 ? 1.0 : (double)Covered / Population;

    // Fold steps one occurrence to one verdict. Population advances unconditionally, so a covered count can never
    // drift past the population that produced it.
    public CoverageRatio Fold(bool covered) => new(Population + 1, Covered + (covered ? 1 : 0));
}

// TWO ratios, never one blended share: Assessed answers "has the analysis run" and Substantiated "could it run"
// (the bound material carries this discipline's property set). A project missing thermal properties and a project
// missing thermal RESULTS need different work, and a single number hides which one is in front of you.
public readonly record struct DisciplineCoverage(CoverageRatio Assessed, CoverageRatio Substantiated);

// CoverageAxis owns each STRUCTURAL coverage axis as one row: the per-element Covered predicate the census fold
// reads and the Floor projection the gate and the shortfall projection read off the policy — the four ratios were
// previously spelled FOUR times (census columns, threshold columns, Clears clauses, Shortfalls literals); every
// site now folds Items, so a fifth axis is one row and zero fold edits.
[SmartEnum<string>]
public sealed partial class CoverageAxis {
    public static readonly CoverageAxis Classified = new("classified",
        covered: static element => element.Classification.Admitted.IsSome || !element.Classifications.IsEmpty,
        floor: static policy => policy.Classified);
    public static readonly CoverageAxis MaterialBound = new("material-bound",
        covered: static element => !element.Materials.IsEmpty,
        floor: static policy => policy.MaterialBound);
    public static readonly CoverageAxis Quantified = new("quantified",
        covered: static element => element.Quantities.Exists(static bag => !bag.Values.IsEmpty),
        floor: static policy => policy.Quantified);
    public static readonly CoverageAxis Propertied = new("propertied",
        covered: static element => element.Properties.Exists(static bag => !bag.Values.IsEmpty),
        floor: static policy => policy.Propertied);

    [UseDelegateFromConstructor] public partial bool Covered(Element element);
    [UseDelegateFromConstructor] public partial double Floor(AuditThresholds policy);
}

// Structural keys the closed CoverageAxis roster and ByDiscipline the closed Discipline roster — both TOTAL at
// Empty (every row seeded at zero population), so a policy naming an untouched axis or discipline reads a vacuous
// full share instead of an absent key. [Equatable]: the census is STORED on the Bim ModelHealth receipt, so its
// equality must be structural, never the reference identity two HashMap carriers report.
[Equatable]
public sealed partial record CoverageCensus(
    [property: UnorderedEquality] HashMap<CoverageAxis, CoverageRatio> Structural,
    [property: UnorderedEquality] HashMap<Discipline, DisciplineCoverage> ByDiscipline) {

    public static CoverageCensus Empty =>
        new(toSeq(CoverageAxis.Items).Fold(
                HashMap<CoverageAxis, CoverageRatio>(),
                static (held, axis) => held.Add(axis, CoverageRatio.Vacuous)),
            toSeq(Discipline.Items).Fold(
                HashMap<Discipline, DisciplineCoverage>(),
                static (held, discipline) => held.Add(discipline, default)));

    public CoverageRatio At(CoverageAxis axis) => Structural.Find(axis).IfNone(CoverageRatio.Vacuous);

    // ONE occurrence, EVERY counter — each axis reads its own row predicate, so the fold body never re-spells one.
    public CoverageCensus Fold(Element element) =>
        this with {
            Structural = Structural.Map((axis, ratio) => ratio.Fold(axis.Covered(element))),
            ByDiscipline = ByDiscipline.Map((discipline, row) => new DisciplineCoverage(
                // Consumable is the outcome capability, not a state roster: Computed and Stale both carry a
                // readable result, Failed and Queued do not, so coverage reads the capability.
                row.Assessed.Fold(element.Assessments.Exists(a => a.Discipline == discipline && a.Outcome.Capabilities.Admits(OutcomeCapability.Consumable))),
                row.Substantiated.Fold(element.Materials.Exists(m => m.Material.Properties.Exists(p => p.Discipline == discipline))))),
        };
}
```

## [03]-[INTEGRITY_SWEEP]

- Owner: `AuditCategory` carries the sweep, grade, and origin; `AuditEvidence` distinguishes a semantic detail from an exact railed refusal; `AuditFinding` carries category, severity, optional subject, and that evidence; `AuditTally` carries each category-severity count.
- Law: `Grade` is ROW DATA, never a call-site argument. Structural faults grade `Blocking` wherever found and coverage shortfalls `Warning`, so a sweep never re-decides its own class and a report cannot show one category under two grades. `ConstraintSeverity` IS the vocabulary — a parallel audit-severity enum makes `blocking` mean two things on one dashboard.
- Law: each sweep retains the producing fault's evidence verbatim and uses numeric fault identity wherever routing or aggregation requires identity.
- Entry: the sweeps are internal to the `[04]` fold; a consumer reads the graded findings off the receipt and never runs one directly.
- Auto: ORPHANS read the incidence closure and exempt `Object` nodes; DANGLING reads the property bags alone; COMPOSE proves acyclicity over the keyed Composition view; REPRESENTATIONS censuses occurrences; INTERFACES censuses `Connect` edges carrying a blob key; ASSESSMENTS reads the outcome's own `Reportable` capability (Failed/Cancelled/Stale/Superseded — the roster can never silently outgrow the filter); SHORTFALLS folds the `CoverageAxis` roster against the policy; DRIFT flattens the `ContentAddress.Verify` accumulation through `AdmissionSlots.Unpack`.
- Receipt: a `Seq<AuditFinding>` is the integrity half of the `ModelAudit`; semantic findings retain their detail and failed folds retain their exact `Error` through `AuditEvidence`; `AuditTally` is its metric-plane projection, folded once at `[04]` so an instrument arm writes buckets instead of re-walking the sequence per fire.
- Packages: QuikGraph (`AlgorithmExtensions.IsDirectedAcyclicGraph` the acyclicity proof over the `Graph/element#ELEMENT_GRAPH` keyed `ElementGraph.View` scope), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[Union]`), LanguageExt.Core (`Seq`/`Option`/`Error` + `Choose`/`Filter`/`Bind` and the `ManyErrors` unpack), `Projection/address#CONTENT_ADDRESS` (`ContentAddress.Verify`), `Relations/relation#EDGE_ALGEBRA` (`Relationship.Compose`/`Connect`), `Assessment/assessment#ASSESSMENT_NODE` (`AssessmentOutcome`).
- Growth: a new integrity class is one `AuditCategory` row carrying its grade AND its sweep — the `[04]` fold over `Items` absorbs it with zero edits; a new lifecycle state the assessment sweep catches is one `Reportable`-carrying outcome row at its owner, never a filter edit here.
- Boundary: ORPHANS read `EdgesAt`, the `Relationship.Members` closure, and NEVER the topology view's `IsolatedVertices` — the view is built from `DirectedPairs` and a node reachable only as a buried `PropertyValue.Reference` contributes no directed leg, so that read reports a live, cascade-tracked node as an orphan. `EdgesAt` is the closure the `DropNode` cascade itself keys on, which is exactly what an orphan claim means.
- Boundary: a bag's buried `PropertyValue.Reference` is the ONE dangling class the graph admits — every edge member is guarded at `WorkingGraph.Apply` and re-guarded at `ElementGraph.Apply`, so no edge can name an absent node, and a `QuantityBag` carries `MeasureValue` rows that bury no `NodeId` at all, so DANGLING reads `PropertySet` nodes and nothing else.
- Boundary: the `Compose` acyclicity proof is WHOLE-GRAPH where `Bake`'s ancestry guard is per-root: a cycle in a subtree no consumer bakes still corrupts a federation merge and an egress walk, and `ElementGraph.View(EdgeFilter.Composition, EdgeOrientation.Forward)` proves the property over the graph's own memoized scope — a filtered-view generic re-spelled here mints a second kind-scoping owner beside the one that declares it.
- Boundary: an empty representation on an occurrence is a WARNING, and on a TYPE is not a finding at all — a Component's shape legitimately rides its occurrences, so grading the catalogue definition reports the modelling convention as a defect.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// SweepOrigin names WHERE a category's findings mint: Sweep rows own their fold as the Sweep column below, and the
// one Population row's findings mint inside the occurrence pass (a bake refusal is evidence only that fold sees),
// so the column states the exception instead of a reader inferring it from an empty delegate.
[SmartEnum<string>]
public sealed partial class SweepOrigin {
    public static readonly SweepOrigin Sweep = new("sweep");
    public static readonly SweepOrigin Population = new("population");
}

// AuditRun is the ONE state record every sweep takes — graph, key, the folded census, and the policy — so the
// category roster folds as `Items.Bind(category => category.Sweep(run))` and a new integrity class is one row
// carrying its own fold, never an eighth term in a hand concatenation.
public readonly record struct AuditRun(ElementGraph Graph, Op Key, CoverageCensus Census, AuditThresholds Policy);

// AuditCategory closes the sweep taxonomy. Grade is the DEFAULT severity every finding of the class carries, and
// Sweep IS the class's fold — class, verdict, and detection travel together, so a sweep has no grade argument to
// get wrong and the [04] entry has no roster to forget a term of.
[SmartEnum<string>]
public sealed partial class AuditCategory {
    public static readonly AuditCategory Orphan = new("orphan", grade: ConstraintSeverity.Blocking,
        origin: SweepOrigin.Sweep, sweep: static run => ModelAudit.Orphans(run.Graph));
    public static readonly AuditCategory DanglingReference = new("dangling-reference", grade: ConstraintSeverity.Blocking,
        origin: SweepOrigin.Sweep, sweep: static run => ModelAudit.Dangling(run.Graph));
    public static readonly AuditCategory ComposeCycle = new("compose-cycle", grade: ConstraintSeverity.Blocking,
        origin: SweepOrigin.Sweep, sweep: static run => ModelAudit.ComposeCycles(run.Graph));
    public static readonly AuditCategory AddressDrift = new("address-drift", grade: ConstraintSeverity.Blocking,
        origin: SweepOrigin.Sweep, sweep: static run => ModelAudit.Drift(run.Graph, run.Key));
    public static readonly AuditCategory BakeRejected = new("bake-rejected", grade: ConstraintSeverity.Blocking,
        origin: SweepOrigin.Population, sweep: static _ => Seq<AuditFinding>());
    public static readonly AuditCategory EmptyRepresentation = new("empty-representation", grade: ConstraintSeverity.Warning,
        origin: SweepOrigin.Sweep, sweep: static run => ModelAudit.Representations(run.Graph));
    public static readonly AuditCategory SourceBoundInterface = new("source-bound-interface", grade: ConstraintSeverity.Warning,
        origin: SweepOrigin.Sweep, sweep: static run => ModelAudit.Interfaces(run.Graph));
    public static readonly AuditCategory AssessmentStale = new("assessment-stale", grade: ConstraintSeverity.Warning,
        origin: SweepOrigin.Sweep, sweep: static run => ModelAudit.Assessments(run.Graph));
    public static readonly AuditCategory CoverageShortfall = new("coverage-shortfall", grade: ConstraintSeverity.Warning,
        origin: SweepOrigin.Sweep, sweep: static run => ModelAudit.Shortfalls(run.Census, run.Policy));

    public ConstraintSeverity Grade { get; }
    public SweepOrigin Origin { get; }

    [UseDelegateFromConstructor] public partial Seq<AuditFinding> Sweep(AuditRun run);
}

// --- [MODELS] -----------------------------------------------------------------------------
// Semantic audit observations and railed refusals are distinct evidence shapes. A Bake/Verify failure retains the
// exact Error; rendering its Message into the detail case would erase typed identity and exception cause custody.
[Union]
public abstract partial record AuditEvidence {
    private AuditEvidence() { }
    public sealed record Detail(string Value) : AuditEvidence;
    public sealed record Refusal(Error Cause) : AuditEvidence;
}

// Subject is OPTIONAL because the sweeps split by scope: a drifted node or an orphan names one id, while a
// Compose cycle and a coverage shortfall are whole-graph properties no single node owns. A fabricated subject on
// a graph-scoped finding would send a repair to an arbitrary participant.
public readonly record struct AuditFinding(AuditCategory Category, ConstraintSeverity Severity, Option<NodeId> Subject, AuditEvidence Evidence) {
    public static AuditFinding Of(AuditCategory category, string detail, Option<NodeId> subject = default) =>
        new(category, category.Grade, subject, new AuditEvidence.Detail(detail));
    public static AuditFinding Of(AuditCategory category, Error cause, Option<NodeId> subject = default) =>
        new(category, category.Grade, subject, new AuditEvidence.Refusal(cause));
}

// AuditTally is the metric-plane bucket the Projection/observe#HOOK_RAIL Audited fact carries. It seats HERE beside
// its taxonomy and grade, and the fact composes it — the audit owns the counts, the rail owns the event.
public readonly record struct AuditTally(AuditCategory Category, ConstraintSeverity Severity, int Count);
```

## [04]-[AUDIT_FOLD]

- Owner: `AuditThresholds` the delivery-gate policy row (the `CoverageAxis`-projected structural floors, a per-`Discipline` assessed-share map, and a blocking-count ceiling); `ModelAudit` the `[Equatable]` graded receipt over one frozen snapshot (STORED on the Bim `ModelHealth`), carrying the census, the ordered finding stream, and the policy it was graded under.
- Law: the audit holds ZERO authority — it mutates nothing, mints no node, and produces no rail failure of its own. Blocking findings are RECEIPT DATA, never a `Fin.Fail`: an audit refusing to report on a broken model fails exactly where it is needed, and a consumer wanting a hard stop reads `Clears` and decides. `Fin` is the return so the entry sits on the seam's one rail like every other entrypoint and the `Projection/observe#HOOK_RAIL` timed decoration binds it unchanged.
- Law: the receipt carries its own `AuditThresholds`, so a stored audit is re-readable against the policy it was graded under; re-evaluating a persisted receipt against today's floors silently re-verdicts yesterday's delivery.
- Entry: `ModelAudit.Of(ElementGraph graph, Op key, Option<AuditThresholds> thresholds = default)` runs the whole grade — one occurrence fold, then ONE fold over the `AuditCategory` roster where each row runs its own `Sweep(run)` — defaulting to `AuditThresholds.Structural`; `Blocking` and `Tallies` derive from one memoized pass, and `Clears` is the ONE delivery predicate folding the same `CoverageAxis` roster the census and the shortfall projection read.
- Auto: the coverage fold reads each `CoverageAxis` row's own `Covered` predicate and rides ONE pass over the occurrence population with the bake verdicts (the `BakeRejected` row's `Origin` names that fold); the sweeps then run over the already-frozen node, edge, and view structures the snapshot built once. Coverage shortfalls project into the SAME finding stream as the integrity rows, so a report reads one shape and a dashboard bands one series. `ContentAddress.OfGraph` supplies the snapshot address, so a receipt pins to exactly the content it graded.
- Receipt: `ModelAudit` is the model-quality evidence a delivery gate, a QA report, and the audit instrument series all read — the graded snapshot address, the coverage census, the finding stream, and the policy, with `Clears` the single predicate a gate evaluates.
- Packages: LanguageExt.Core (`Seq`/`HashMap`/`Option`/`Fin` + the `Fold` state thread and the `ManyErrors` unpack), `Rasm` (the kernel `Op` op-key), `Projection/projection#PROJECTION_CONTRACT` (`ConstraintSeverity` the shared grade), `Projection/address#CONTENT_ADDRESS` (`ContentAddress.OfGraph` the snapshot identity, `Verify` the drift census), `Graph/element#ELEMENT_GRAPH` (the accessor family and the memoized `Bake`).
- Growth: a new threshold axis is one `AuditThresholds` column with one clause in `Clears` and one row in the shortfall projection; a new sweep is one term in the finding concatenation. `ModelAudit.Of` is the ONE entry that reports while the owner repairs, so a second entry point, a mutating repair verb, or an audit-owned fault case is the deleted form.
- Boundary: the default policy is STRUCTURAL only — every coverage floor zero, the blocking ceiling zero — so an undeclared project claims no coverage it has not committed to, and a gate that passes by default passes only a model with no structural fault.
- Boundary: `Clears` reads the RECEIPT alone, which is what makes the receipt persistable beside the model; re-folding the graph to answer a question the receipt already carries is the deleted form.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// Every column is a SHARE floor on [0,1] or a count ceiling, so the gate is arithmetic over the receipt rather than
// a policy language. Assessed is keyed per Discipline because a project commits to thermal coverage without
// committing to acoustic coverage, and one blended floor cannot express that.
public readonly record struct AuditThresholds(
    double Classified, double MaterialBound, double Quantified, double Propertied,
    HashMap<Discipline, double> Assessed, int BlockingCeiling) {

    // Structural is the default row, grading integrity alone: an undeclared project claims no coverage, so the gate
    // passes a sound-but-sparse model and stops a broken one, never failing every early model by default.
    public static readonly AuditThresholds Structural =
        new(0.0, 0.0, 0.0, 0.0, HashMap<Discipline, double>(), 0);
}

// [Equatable]: the audit is STORED on the Bim ModelHealth receipt, so equality is structural and the finding run
// ordered — a reference-compared receipt reads every re-load as a change.
[Equatable]
public sealed partial record ModelAudit {
    private ModelAudit(ContentAddress snapshot, CoverageCensus coverage, Seq<AuditFinding> findings, AuditThresholds thresholds) =>
        (Snapshot, Coverage, Findings, Thresholds) = (snapshot, coverage, findings, thresholds);

    public ContentAddress Snapshot { get; }
    public CoverageCensus Coverage { get; }
    [OrderedEquality] public Seq<AuditFinding> Findings { get; }
    public AuditThresholds Thresholds { get; }

    // Blocking and Tallies derive from ONE pass over the finding run, memoized per instance — the observe rail
    // fires one fact per audit and the gate reads Blocking beside it, so neither read re-walks the sequence.
    [IgnoreEquality] Lazy<(Seq<AuditFinding> Blocking, Seq<AuditTally> Tallies)>? ledger;

    (Seq<AuditFinding> Blocking, Seq<AuditTally> Tallies) Ledger => (ledger ??= new(() => {
        var folded = Findings.Fold(
            (Blocking: Seq<AuditFinding>(), Buckets: HashMap<(AuditCategory Category, ConstraintSeverity Severity), int>()),
            static (state, finding) => (
                finding.Severity.Blocks ? state.Blocking.Add(finding) : state.Blocking,
                state.Buckets.AddOrUpdate((finding.Category, finding.Severity), static count => count + 1, 1)));
        // The bucket walk re-enters through AsIterable: a two-parameter HashMap declares no fold of its own, so its
        // carrier-generic Fold carries the VALUE alone and the key-bearing pair run is that projection.
        return (folded.Blocking, folded.Buckets.AsIterable()
            .Fold(Seq<AuditTally>(), static (rows, bucket) => rows.Add(new AuditTally(bucket.Key.Category, bucket.Key.Severity, bucket.Value))));
    })).Value;

    public Seq<AuditFinding> Blocking => Ledger.Blocking;

    public Seq<AuditTally> Tallies => Ledger.Tallies;

    // Clears is the ONE delivery predicate, over the receipt alone — the structural floors fold the SAME axis
    // roster the census and the shortfall projection read, so a fifth axis reaches all three with zero edits here.
    // A discipline the policy names but the census never populated reads the vacuous share, so a floor on an
    // untouched discipline passes rather than blocking a model never asked to carry it.
    public bool Clears =>
        Blocking.Count <= Thresholds.BlockingCeiling
        && toSeq(CoverageAxis.Items).ForAll(axis => Coverage.At(axis).Share >= axis.Floor(Thresholds))
        && Thresholds.Assessed.ForAll((discipline, floor) => AssessedShare(discipline) >= floor);

    double AssessedShare(Discipline discipline) =>
        Coverage.ByDiscipline.Find(discipline).Map(static row => row.Assessed).IfNone(CoverageRatio.Vacuous).Share;

    // Of runs the whole grade: one occurrence pass, seven sweeps over the already-frozen structures, and the
    // shortfall projection that lands the census in the SAME finding stream. Fin is the seam's uniform rail, never a
    // failure channel — this fold has no fault to mint, and a model too broken to assemble still grades.
    public static Fin<ModelAudit> Of(ElementGraph graph, Op key, Option<AuditThresholds> thresholds = default) =>
        (Policy: thresholds.IfNone(AuditThresholds.Structural), Pass: Population(graph, key)) switch {
            var grade => new AuditRun(graph, key, grade.Pass.Census, grade.Policy) switch {
                // ONE fold over the category roster — each row runs its OWN sweep, the population verdicts riding
                // in front (their row's Origin names that fold), so an eighth class is a row, never a ninth term.
                var run => Fin.Succ(new ModelAudit(
                    ContentAddress.OfGraph(graph),
                    run.Census,
                    grade.Pass.Verdicts + toSeq(AuditCategory.Items).Bind(category => category.Sweep(run)),
                    grade.Policy)),
            },
        };

    // --- [OCCURRENCE_PASS]
    // ONE fold carries the census AND the bake verdicts: a Bake that rails is itself evidence (an absent root, a
    // cyclic Compose ancestry), so collecting it in a second pass would re-walk the subgraph the first pass reached.
    // Population gives a failed element NO counter — an element that cannot be read cannot be graded.
    static (CoverageCensus Census, Seq<AuditFinding> Verdicts) Population(ElementGraph graph, Op key) =>
        toSeq(graph.ObjectNodes)
            .Filter(static o => o.Kind == ObjectKind.Occurrence)
            .Fold((Census: CoverageCensus.Empty, Verdicts: Seq<AuditFinding>()), (state, occurrence) =>
                graph.Bake(occurrence.Id, key).Match(
                    Succ: element => (state.Census.Fold(element), state.Verdicts),
                    // Preserve the exact Error; presentation belongs outside the audit receipt.
                    Fail: error => (state.Census, state.Verdicts.Add(
                        AuditFinding.Of(AuditCategory.BakeRejected, error, Some(occurrence.Id))))));

    // --- [STRUCTURAL_SWEEPS]
    // EdgesAt is the Members closure, so a node reachable ONLY as a buried attribute reference is NOT an orphan.
    // Object nodes are exempt: an element root carrying no relationship yet is an authoring state, not a defect.
    internal static Seq<AuditFinding> Orphans(ElementGraph graph) =>
        toSeq(graph.Nodes.Values)
            .Filter(node => node is not Node.Object && graph.EdgesAt(node.Id).IsEmpty)
            .Map(static node => AuditFinding.Of(AuditCategory.Orphan, $"<orphan-node:{node.Id.Value}>", Some(node.Id)));

    // Property bags alone: a QuantityBag holds MeasureValue rows that bury no NodeId, and every edge member is
    // already guarded at admission and replay, so this is the one dangling class the graph admits.
    internal static Seq<AuditFinding> Dangling(ElementGraph graph) =>
        toSeq(graph.Nodes.Values)
            .Choose(static node => node is Node.PropertySet bag ? Some(bag) : None)
            .Bind(bag => toSeq(bag.Bag.Values.Values)
                .Bind(static value => value.References())
                .Filter(target => !graph.Nodes.ContainsKey(target))
                .Map(target => AuditFinding.Of(
                    AuditCategory.DanglingReference, $"<dangling-reference:{bag.Id.Value}:{target.Value}>", Some(bag.Id))));

    // ComposeCycles walks the graph's OWN keyed Composition view, so this sweep
    // materializes nothing and spells no filtered-view generic beside the owner that declares it. Whole-graph where
    // Bake's ancestry guard is per-root.
    internal static Seq<AuditFinding> ComposeCycles(ElementGraph graph) =>
        graph.View(EdgeFilter.Composition, EdgeOrientation.Forward).IsDirectedAcyclicGraph()
            ? Seq<AuditFinding>()
            : Seq(AuditFinding.Of(AuditCategory.ComposeCycle, "<compose-cycle>"));

    // Occurrence-scoped: a Type's shape legitimately rides its occurrences, so grading a catalogue definition would
    // report the modelling convention as a defect.
    internal static Seq<AuditFinding> Representations(ElementGraph graph) =>
        toSeq(graph.ObjectNodes)
            .Filter(static o => o.Kind == ObjectKind.Occurrence && o.Representations.ByIdentifier.IsEmpty)
            .Map(static o => AuditFinding.Of(
                AuditCategory.EmptyRepresentation, $"<representations-empty:{o.Id.Value}>", Some(o.Id)));

    // Interfaces censuses SOURCE-BOUND edges, never faults: the Interface key names blob geometry the producing end
    // alone rehydrates, so a crossing that leaves that end is incomplete without the store travelling with it.
    internal static Seq<AuditFinding> Interfaces(ElementGraph graph) =>
        toSeq(graph.Edges)
            .Choose(static edge => edge is Relationship.Connect { Interface.IsSome: true } connect ? Some(connect) : None)
            .Map(static connect => AuditFinding.Of(
                AuditCategory.SourceBoundInterface, $"<connect-interface:{connect.From.Value}:{connect.To.Value}>"));

    // Assessments reads the outcome's OWN Reportable capability — Failed, Cancelled, Stale, and Superseded all
    // carry it at the owner, so the sweep can never silently drop a state the roster grew (the Cancelled drop this
    // filter previously had: same settled-not-consumable columns as Failed, absent from the hand roster, so a
    // cancelled analysis on a delivered model graded CLEAN). The OUTCOME token rides the detail.
    internal static Seq<AuditFinding> Assessments(ElementGraph graph) =>
        toSeq(graph.Nodes.Values)
            .Choose(static node => node is Node.Assessment assessment ? Some(assessment) : None)
            .Filter(static a => a.Payload.Outcome.Capabilities.Admits(OutcomeCapability.Reportable))
            .Map(static a => AuditFinding.Of(
                AuditCategory.AssessmentStale, $"<assessment-{a.Payload.Outcome.Key}:{a.Id.Value}>", Some(a.Id)));

    // Drift folds Verify's accumulating Validation as the TAMPER census — that sweep is already complete because
    // independent node checks accumulate, so this unpacks ManyErrors rather than re-verifying node by node.
    internal static Seq<AuditFinding> Drift(ElementGraph graph, Op key) =>
        ContentAddress.Verify(graph, key).Match(
            Succ: static _ => Seq<AuditFinding>(),
            // Unpack is the branch's ONE ManyErrors flattener — never a local `is ManyErrors` twin per sweep.
            Fail: static error => Unpack(error)
                .Map(static drift => AuditFinding.Of(AuditCategory.AddressDrift, drift)));

    // --- [COVERAGE_SHORTFALL]
    // Shortfalls lands a missed floor as a Warning FINDING, so integrity and coverage ride ONE stream and a report
    // needs no second shape. Clears stays the verdict; these rows name what it counted.
    internal static Seq<AuditFinding> Shortfalls(CoverageCensus census, AuditThresholds policy) =>
        toSeq(CoverageAxis.Items)
        .Filter(axis => census.At(axis).Share < axis.Floor(policy))
        .Map(axis => AuditFinding.Of(AuditCategory.CoverageShortfall, $"<coverage-shortfall:{axis.Key}>"))
        // Per-discipline floors walk through AsIterable for the same reason the tally fold does — the key is
        // load-bearing in the detail token, and a two-parameter HashMap's own fold carries the value alone.
        + policy.Assessed.AsIterable().Fold(Seq<AuditFinding>(), (rows, declared) =>
            census.ByDiscipline.Find(declared.Key).Map(static row => row.Assessed).IfNone(CoverageRatio.Vacuous).Share < declared.Value
                ? rows.Add(AuditFinding.Of(AuditCategory.CoverageShortfall, $"<coverage-shortfall:assessed:{declared.Key.Key}>"))
                : rows);
}
```

## [05]-[RESEARCH]

(none)
