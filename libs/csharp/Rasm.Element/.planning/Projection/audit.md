# [ELEMENT_AUDIT]

The model-completeness grader: ONE fold over a frozen `ElementGraph` yielding a typed `ModelAudit` receipt carrying per-discipline coverage ratios, a graded integrity finding stream, and the threshold policy a delivery gate reads as a single predicate. The scope split is absolute — `Rasm.Element` `ModelAudit` owns NEUTRAL structural integrity and coverage over the one graph, while the `Rasm.Bim` `Review` `ModelHealth` owns IFC template and IDS semantics; the two COMPOSE on one model and never overlap, because a dangling bag reference is a graph fact any consumer can name and a missing `Pset_WallCommon` row is an IFC claim only the schema owner can make.

The audit holds ZERO authority: it mutates nothing, mints no node, and rails no fault of its own — every verdict is receipt data, so a model too broken to assemble still grades. It reuses the settled grades and keys rather than minting parallel ones: `Projection/projection#PROJECTION_CONTRACT` `ConstraintSeverity` is the verdict vocabulary (structural faults `Blocking`, coverage shortfalls `Warning`), `Projection/address#CONTENT_ADDRESS` `ContentAddress.OfGraph` the snapshot identity a receipt pins to and `Verify` the tamper census folded in, `Projection/fault#FAULT_BAND` `[DETAIL_GRAMMAR]` the frozen token every finding detail spells, and the `Graph/element#ELEMENT_GRAPH` accessor family plus the memoized `Bake` the only reads — a coverage ratio never opens a second graph.

## [01]-[INDEX]

- [02]-[COVERAGE_CENSUS]: the `CoverageRatio` population/covered pair with its derived share, the `DisciplineCoverage` assessed-versus-substantiated split, the `CoverageCensus` model-wide row set keyed per `Discipline`, and the ONE occurrence fold that carries every counter and every bake verdict in a single pass.
- [03]-[INTEGRITY_SWEEP]: the `AuditCategory` graded taxonomy, the `AuditFinding` census row, and the seven sweeps — orphan nodes off the incidence closure, dangling bag references, the `Compose` acyclicity proof over the filtered tagged view, empty occurrence representations, source-bound `Connect` interfaces, non-current assessments, and the `ContentAddress.Verify` drift census.
- [04]-[AUDIT_FOLD]: the `AuditThresholds` delivery-policy row, the `ModelAudit` receipt with its `Blocking`/`Tallies`/`Clears` derived reads, and the `ModelAudit.Of` entry that composes the census and every sweep into one graded value.

## [02]-[COVERAGE_CENSUS]

- Owner: `CoverageRatio` the `(Population, Covered)` pair whose share DERIVES; `DisciplineCoverage` the per-discipline assessed-and-substantiated pair; `CoverageCensus` the model-wide census over the occurrence population, holding the four structural shares beside the discipline-keyed rows.
- Law: coverage reads the BAKED element, never a raw incidence scan. `Bake` applies the `Assign.TypeDefinition` inheritance and the `InheritanceMode` bag merge, so a takeoff bound to a Type COVERS every occurrence of that type — the raw scan under-reports exactly the modelling practice the IFC `QTO_TYPEDRIVENOVERRIDE` rule exists to license, and it would read a correctly-authored catalogue model as un-quantified.
- Law: the four structural shares carry NO discipline. An `Graph/element#NODE_MODEL` `Object` declares no discipline column — the `Classification/classification#DISCIPLINE_AXIS` axis keys assessments and material property sets, never objects — so a per-discipline classified or quantified share would partition a population the graph does not partition. The discipline axis lands exactly where a discipline is spellable: the assessment read and the material-property read, one row per `Discipline`.
- Entry: the census is folded, never constructed — `[04]` `ModelAudit.Of` runs the one pass and the receipt carries the result.
- Auto: the population is the OCCURRENCE roster (`ObjectKind.Occurrence`), because a Type is a catalogue definition and grading it beside its instances double-counts the shared data it exists to share. Every `Discipline` row seeds at zero population, so a discipline no occurrence touches reads a vacuous full share rather than an absent key a gate must special-case. A `Bake` that rails contributes its verdict to the finding stream and no counter — an element that cannot be read cannot be graded.
- Receipt: `CoverageCensus` is the completeness half of the `ModelAudit` receipt; a report reads the counts, a gate reads the shares, and neither re-walks the graph.
- Packages: LanguageExt.Core (`Seq`/`HashMap`/`Option`/`Fin` + the `Fold` state thread), `Graph/element#ELEMENT_GRAPH` (`ElementGraph.ObjectNodes`/`Bake` and the baked `Element` flat reads), `Classification/classification#DISCIPLINE_AXIS` (`Discipline`), `Composition/material#MATERIAL_PROPERTY` (`MaterialPropertySet.Discipline`), `Assessment/assessment#ASSESSMENT_NODE` (`AssessmentPayload.Discipline`/`Outcome` with the `Usable` column).
- Growth: a new structural share is one `CoverageRatio` column plus one predicate in the fold; a new per-discipline axis is one `DisciplineCoverage` column; a new discipline is one `Discipline` row the seed absorbs with no edit here. A second fold over the same population is the deleted form — a new counter joins the one pass or it does not exist.
- Boundary: a vacuous population reads FULLY covered. An empty model must not trip a gate built to catch a half-classified one, and `0/0` admits no other honest answer; a gate that means "and the model is non-empty" states that as its own blocking-count ceiling.
- Boundary: `Quantified` and `Propertied` require a NON-EMPTY bag, not a bound one — an `Assign.PropertyDefinition` edge onto an empty bag is a binding, not evidence, and counting it would read the scaffolding as the content.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Element.Assessment;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element.Projection;

// --- [MODELS] -----------------------------------------------------------------------------
// The share DERIVES — a stored ratio beside its two counts is the double-store defect, and the two audiences want
// different halves: a report names "37 of 214 occurrences", a gate compares 0.173 against a floor.
public readonly record struct CoverageRatio(int Population, int Covered) {
    public double Share => Population == 0 ? 1.0 : (double)Covered / Population;

    // The fold step: one occurrence, one verdict. Population advances unconditionally, so a covered count can never
    // drift past the population that produced it.
    public CoverageRatio Fold(bool covered) => new(Population + 1, Covered + (covered ? 1 : 0));
}

// TWO ratios, never one blended share: Assessed answers "has the analysis run" and Substantiated "could it run"
// (the bound material carries this discipline's property set). A project missing thermal properties and a project
// missing thermal RESULTS need different work, and a single number hides which one is in front of you.
public readonly record struct DisciplineCoverage(CoverageRatio Assessed, CoverageRatio Substantiated);

public sealed record CoverageCensus(
    CoverageRatio Classified,
    CoverageRatio MaterialBound,
    CoverageRatio Quantified,
    CoverageRatio Propertied,
    HashMap<Discipline, DisciplineCoverage> ByDiscipline) {

    // Every Discipline row seeds present at zero population, so ByDiscipline is TOTAL over the closed roster and a
    // threshold naming an untouched discipline reads a vacuous full share instead of an absent key.
    public static CoverageCensus Empty =>
        new(default, default, default, default,
            toSeq(Discipline.Items).Fold(
                HashMap<Discipline, DisciplineCoverage>(),
                static (held, discipline) => held.Add(discipline, default)));

    // ONE occurrence, EVERY counter. The baked element already carries materials, bags, assessments, and the type
    // inheritance flat, so each predicate is a read rather than a traversal and the whole census costs one Bake.
    public CoverageCensus Fold(Element element) =>
        this with {
            Classified = Classified.Fold(element.Classification != default || !element.Classifications.IsEmpty),
            MaterialBound = MaterialBound.Fold(!element.Materials.IsEmpty),
            Quantified = Quantified.Fold(element.Quantities.Exists(static bag => !bag.Values.IsEmpty)),
            Propertied = Propertied.Fold(element.Properties.Exists(static bag => !bag.Values.IsEmpty)),
            ByDiscipline = ByDiscipline.Map((discipline, row) => new DisciplineCoverage(
                // Usable is the outcome column, not a state roster: a Computed and a Stale receipt both carry a
                // readable result, and a Failed or Queued one does not, so coverage reads the column.
                row.Assessed.Fold(element.Assessments.Exists(a => a.Discipline == discipline && a.Outcome.Usable)),
                row.Substantiated.Fold(element.Materials.Exists(m => m.Material.Properties.Exists(p => p.Discipline == discipline))))),
        };
}
```

## [03]-[INTEGRITY_SWEEP]

- Owner: `AuditCategory` the `[SmartEnum<string>]` sweep taxonomy carrying its `Grade` column; `AuditFinding` the graded census row (category, severity, optional subject node, frozen detail token); `AuditTally` the `(category, severity, count)` bucket the observe rail's fact carries.
- Law: `Grade` is ROW DATA, not a call-site argument. A structural fault is `Blocking` wherever it is found and a coverage shortfall `Warning`, so a sweep never re-decides its own class and a report cannot show one category under two grades. The vocabulary is `ConstraintSeverity` itself — a parallel audit-severity enum would make `blocking` mean two things on one dashboard.
- Law: every `Detail` is a frozen `<kind:colon-args>` token under `Projection/fault#FAULT_BAND` `[DETAIL_GRAMMAR]`, so a finding dedups and pins across runs on the same terms a `ConstraintFinding` waiver does. A sweep folding a fault's own message forwards that fault's token verbatim rather than re-wording it.
- Entry: the sweeps are internal to the `[04]` fold; a consumer reads the graded findings off the receipt and never runs one directly.
- Auto: ORPHANS read the incidence closure and exempt `Object` nodes; DANGLING reads the property bags alone; COMPOSE proves acyclicity over the `Compose`-filtered tagged view; REPRESENTATIONS censuses occurrences; INTERFACES censuses `Connect` edges carrying a blob key; ASSESSMENTS censuses the non-current lifecycle states; DRIFT unpacks the `ContentAddress.Verify` accumulation.
- Receipt: a `Seq<AuditFinding>` is the integrity half of the `ModelAudit`; `AuditTally` is its metric-plane projection, folded once at `[04]` so an instrument arm writes buckets instead of re-walking the sequence per fire.
- Packages: QuikGraph (`AlgorithmExtensions.IsDirectedAcyclicGraph` the acyclicity proof over the `Graph/element#ELEMENT_GRAPH` `ElementGraph.TopologyOf` predicate view), LanguageExt.Core (`Seq`/`Option` + `Choose`/`Filter`/`Bind` and the `ManyErrors` unpack), `Projection/address#CONTENT_ADDRESS` (`ContentAddress.Verify`), `Relations/relation#EDGE_ALGEBRA` (`Relationship.Compose`/`Connect`), `Assessment/assessment#ASSESSMENT_NODE` (`AssessmentOutcome`).
- Growth: a new integrity class is one `AuditCategory` row carrying its grade plus one sweep composed into the `[04]` concatenation; a new lifecycle state the assessment sweep should catch is one outcome in that sweep's filter, never a second sweep.
- Boundary: ORPHANS read `EdgesAt`, the `Relationship.Members` closure, and NEVER the topology view's `IsolatedVertices` — the view is built from `DirectedPairs` and a node reachable only as a buried `PropertyValue.Reference` contributes no directed leg, so that read would report a live, cascade-tracked node as an orphan. `EdgesAt` is the closure the `DropNode` cascade itself keys on, which is exactly what an orphan claim means.
- Boundary: a bag's buried `PropertyValue.Reference` is the ONE dangling class the graph admits — every edge member is guarded at `WorkingGraph.Apply` and re-guarded at `ElementGraph.Apply`, so no edge can name an absent node, and a `QuantityBag` carries `MeasureValue` rows that bury no `NodeId` at all. The sweep therefore reads `PropertySet` nodes and nothing else.
- Boundary: the `Compose` acyclicity proof is WHOLE-GRAPH where `Bake`'s ancestry guard is per-root: a cycle in a subtree no consumer bakes still corrupts a federation merge and an egress walk, and `ElementGraph.TopologyOf` proves the property over the graph's own built-once view — a filtered-view generic re-spelled here would mint a second kind-scoping owner beside the one that declares it.
- Boundary: an empty representation on an occurrence is a WARNING, and on a TYPE is not a finding at all — a Component's shape legitimately rides its occurrences, so grading the catalogue definition would report the modelling convention as a defect.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The sweep taxonomy. Grade is the DEFAULT severity every finding of the class carries, so the class and its
// verdict travel together and a sweep has no grade argument to get wrong.
[SmartEnum<string>]
public sealed partial class AuditCategory {
    public static readonly AuditCategory Orphan = new("orphan", grade: ConstraintSeverity.Blocking);
    public static readonly AuditCategory DanglingReference = new("dangling-reference", grade: ConstraintSeverity.Blocking);
    public static readonly AuditCategory ComposeCycle = new("compose-cycle", grade: ConstraintSeverity.Blocking);
    public static readonly AuditCategory AddressDrift = new("address-drift", grade: ConstraintSeverity.Blocking);
    public static readonly AuditCategory BakeRejected = new("bake-rejected", grade: ConstraintSeverity.Blocking);
    public static readonly AuditCategory EmptyRepresentation = new("empty-representation", grade: ConstraintSeverity.Warning);
    public static readonly AuditCategory SourceBoundInterface = new("source-bound-interface", grade: ConstraintSeverity.Warning);
    public static readonly AuditCategory AssessmentStale = new("assessment-stale", grade: ConstraintSeverity.Warning);
    public static readonly AuditCategory CoverageShortfall = new("coverage-shortfall", grade: ConstraintSeverity.Warning);

    public ConstraintSeverity Grade { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
// Subject is OPTIONAL because the sweeps split by scope: a drifted node or an orphan names one id, while a
// Compose cycle and a coverage shortfall are whole-graph properties no single node owns. A fabricated subject on
// a graph-scoped finding would send a repair to an arbitrary participant.
public readonly record struct AuditFinding(AuditCategory Category, ConstraintSeverity Severity, Option<NodeId> Subject, string Detail) {
    public static AuditFinding Of(AuditCategory category, string detail, Option<NodeId> subject = default) =>
        new(category, category.Grade, subject, detail);
}

// The metric-plane bucket the Projection/observe#HOOK_RAIL Audited fact carries. It seats HERE, with the taxonomy
// and the grade it pairs, and the fact composes it — the audit owns the counts, the rail owns the event.
public readonly record struct AuditTally(AuditCategory Category, ConstraintSeverity Severity, int Count);
```

## [04]-[AUDIT_FOLD]

- Owner: `AuditThresholds` the delivery-gate policy row (four structural share floors, a per-`Discipline` assessed-share map, and a blocking-count ceiling); `ModelAudit` the graded receipt over one frozen snapshot, carrying the census, the finding stream, and the policy it was graded under.
- Law: the audit holds ZERO authority — it mutates nothing, mints no node, and produces no rail failure of its own. A blocking finding is RECEIPT DATA, never a `Fin.Fail`: an audit that refused to report on a broken model would fail exactly where it is needed, and a consumer that wants a hard stop reads `Clears` and decides. The `Fin` return exists so the entry sits on the seam's one rail like every other entrypoint and the `Projection/observe#HOOK_RAIL` timed decoration binds it unchanged.
- Law: the receipt carries its own `AuditThresholds`, so a stored audit is re-readable against the policy it was graded under. A gate re-evaluating a persisted receipt against today's floors would silently re-verdict yesterday's delivery.
- Entry: `ModelAudit.Of(ElementGraph graph, Op key, Option<AuditThresholds> thresholds = default)` runs the whole grade — one occurrence fold, seven sweeps, and the shortfall projection — defaulting to `AuditThresholds.Structural`; `Blocking` filters the unwaivable rows, `Tallies` folds the metric buckets, and `Clears` is the ONE delivery predicate.
- Auto: the coverage fold and the bake verdicts ride ONE pass over the occurrence population; the sweeps then run over the already-frozen node, edge, and topology structures the snapshot built once. Coverage shortfalls project into the SAME finding stream as the integrity rows, so a report reads one shape and a dashboard bands one series. The snapshot address is `ContentAddress.OfGraph`, so a receipt pins to exactly the content it graded.
- Receipt: `ModelAudit` is the model-quality evidence a delivery gate, a QA report, and the audit instrument series all read — the graded snapshot address, the coverage census, the finding stream, and the policy, with `Clears` the single predicate a gate evaluates.
- Packages: LanguageExt.Core (`Seq`/`HashMap`/`Option`/`Fin` + the `Fold` state thread and the `ManyErrors` unpack), `Rasm` (the kernel `Op` op-key), `Projection/projection#PROJECTION_CONTRACT` (`ConstraintSeverity` the shared grade), `Projection/address#CONTENT_ADDRESS` (`ContentAddress.OfGraph` the snapshot identity, `Verify` the drift census), `Graph/element#ELEMENT_GRAPH` (the accessor family and the memoized `Bake`).
- Growth: a new threshold axis is one `AuditThresholds` column plus one clause in `Clears` and one row in the shortfall projection; a new sweep is one term in the finding concatenation. A second entry point, a mutating repair verb, or an audit-owned fault case is the deleted form — the audit reports and the owner repairs.
- Boundary: the default policy is STRUCTURAL only — every coverage floor zero, the blocking ceiling zero — so an undeclared project claims no coverage it has not committed to, and a gate that passes by default passes only a model with no structural fault.
- Boundary: `Clears` reads the RECEIPT alone. A gate that re-folds the graph to answer a question the receipt already carries is the deleted form, and it is what makes the receipt persistable beside the model in the first place.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// Every column is a SHARE floor on [0,1] or a count ceiling, so the gate is arithmetic over the receipt rather than
// a policy language. Assessed is keyed per Discipline because a project commits to thermal coverage without
// committing to acoustic coverage, and one blended floor cannot express that.
public readonly record struct AuditThresholds(
    double Classified, double MaterialBound, double Quantified, double Propertied,
    HashMap<Discipline, double> Assessed, int BlockingCeiling) {

    // The default row: structural integrity alone. An undeclared project claims no coverage, so the gate passes a
    // sound-but-sparse model and stops a broken one — the opposite of a default that fails every early model.
    public static readonly AuditThresholds Structural =
        new(0.0, 0.0, 0.0, 0.0, HashMap<Discipline, double>(), 0);
}

public sealed record ModelAudit {
    private ModelAudit(ContentAddress snapshot, CoverageCensus coverage, Seq<AuditFinding> findings, AuditThresholds thresholds) =>
        (Snapshot, Coverage, Findings, Thresholds) = (snapshot, coverage, findings, thresholds);

    public ContentAddress Snapshot { get; }
    public CoverageCensus Coverage { get; }
    public Seq<AuditFinding> Findings { get; }
    public AuditThresholds Thresholds { get; }

    public Seq<AuditFinding> Blocking => Findings.Filter(static finding => finding.Severity.Blocks);

    // The metric buckets, folded ONCE: the observe rail fires one fact per audit and the instrument arm writes one
    // row per bucket, where a per-fire re-walk of the finding sequence would pay the fold at every subscriber.
    // The bucket walk re-enters through AsIterable: a two-parameter HashMap declares no fold of its own, so its
    // carrier-generic Fold carries the VALUE alone and the key-bearing pair run is that projection.
    public Seq<AuditTally> Tallies =>
        Findings
            .Fold(HashMap<(AuditCategory Category, ConstraintSeverity Severity), int>(),
                  static (held, finding) => held.AddOrUpdate((finding.Category, finding.Severity), static count => count + 1, 1))
            .AsIterable()
            .Fold(Seq<AuditTally>(), static (rows, bucket) => rows.Add(new AuditTally(bucket.Key.Category, bucket.Key.Severity, bucket.Value)));

    // The ONE delivery predicate, over the receipt alone. A discipline the policy names but the census never
    // populated reads a vacuous full share, so a floor on an untouched discipline passes rather than blocking a
    // model that was never asked to carry it.
    public bool Clears =>
        Blocking.Count <= Thresholds.BlockingCeiling
        && Coverage.Classified.Share >= Thresholds.Classified
        && Coverage.MaterialBound.Share >= Thresholds.MaterialBound
        && Coverage.Quantified.Share >= Thresholds.Quantified
        && Coverage.Propertied.Share >= Thresholds.Propertied
        && Thresholds.Assessed.ForAll((discipline, floor) => AssessedShare(discipline) >= floor);

    double AssessedShare(Discipline discipline) =>
        Coverage.ByDiscipline.Find(discipline).Map(static row => row.Assessed.Share).IfNone(1.0);

    // The whole grade: one occurrence pass, seven sweeps over the already-frozen structures, and the shortfall
    // projection that lands the census in the SAME finding stream. Fin is the seam's uniform rail, not a failure
    // channel — this fold has no fault to mint, and a model too broken to assemble still grades.
    public static Fin<ModelAudit> Of(ElementGraph graph, Op key, Option<AuditThresholds> thresholds = default) =>
        (Policy: thresholds.IfNone(AuditThresholds.Structural), Run: Population(graph, key)) switch {
            var grade => Fin.Succ(new ModelAudit(
                ContentAddress.OfGraph(graph),
                grade.Run.Census,
                grade.Run.Verdicts
                    + Orphans(graph) + Dangling(graph) + ComposeCycles(graph) + Representations(graph)
                    + Interfaces(graph) + Assessments(graph) + Drift(graph, key)
                    + Shortfalls(grade.Run.Census, grade.Policy),
                grade.Policy)),
        };

    // --- [OCCURRENCE_PASS]
    // ONE fold carries the census AND the bake verdicts: a Bake that rails is itself evidence (an absent root, a
    // cyclic Compose ancestry), so collecting it in a second pass would re-walk the subgraph the first pass reached.
    // The failed element contributes NO counter — an element that cannot be read cannot be graded.
    static (CoverageCensus Census, Seq<AuditFinding> Verdicts) Population(ElementGraph graph, Op key) =>
        toSeq(graph.ObjectNodes)
            .Filter(static o => o.Kind == ObjectKind.Occurrence)
            .Fold((Census: CoverageCensus.Empty, Verdicts: Seq<AuditFinding>()), (state, occurrence) =>
                graph.Bake(occurrence.Id, key).Match(
                    Succ: element => (state.Census.Fold(element), state.Verdicts),
                    // The fault's own frozen token forwards verbatim — a re-worded detail would re-key the finding.
                    Fail: error => (state.Census, state.Verdicts.Add(
                        AuditFinding.Of(AuditCategory.BakeRejected, error.Message, Some(occurrence.Id))))));

    // --- [STRUCTURAL_SWEEPS]
    // EdgesAt is the Members closure, so a node reachable ONLY as a buried attribute reference is NOT an orphan.
    // Object nodes are exempt: an element root carrying no relationship yet is an authoring state, not a defect.
    static Seq<AuditFinding> Orphans(ElementGraph graph) =>
        toSeq(graph.Nodes.Values)
            .Filter(node => node is not Node.Object && graph.EdgesAt(node.Id).IsEmpty)
            .Map(static node => AuditFinding.Of(AuditCategory.Orphan, $"<orphan-node:{node.Id.Value}>", Some(node.Id)));

    // Property bags alone: a QuantityBag holds MeasureValue rows that bury no NodeId, and every edge member is
    // already guarded at admission and replay, so this is the one dangling class the graph admits.
    static Seq<AuditFinding> Dangling(ElementGraph graph) =>
        toSeq(graph.Nodes.Values)
            .Choose(static node => node is Node.PropertySet bag ? Some(bag) : None)
            .Bind(bag => toSeq(bag.Bag.Values.Values)
                .Bind(static value => value.References())
                .Filter(target => !graph.Nodes.ContainsKey(target))
                .Map(target => AuditFinding.Of(
                    AuditCategory.DanglingReference, $"<dangling-reference:{bag.Id.Value}:{target.Value}>", Some(bag.Id))));

    // The kind-scoped walk is the graph's OWN TopologyOf predicate view over the one built topology, so this sweep
    // materializes nothing and spells no filtered-view generic beside the owner that declares it. Whole-graph where
    // Bake's ancestry guard is per-root.
    static Seq<AuditFinding> ComposeCycles(ElementGraph graph) =>
        graph.TopologyOf(static edge => edge is Relationship.Compose).IsDirectedAcyclicGraph()
            ? Seq<AuditFinding>()
            : Seq(AuditFinding.Of(AuditCategory.ComposeCycle, "<compose-cycle>"));

    // Occurrence-scoped: a Type's shape legitimately rides its occurrences, so grading a catalogue definition would
    // report the modelling convention as a defect.
    static Seq<AuditFinding> Representations(ElementGraph graph) =>
        toSeq(graph.ObjectNodes)
            .Filter(static o => o.Kind == ObjectKind.Occurrence && o.Representations.ByIdentifier.IsEmpty)
            .Map(static o => AuditFinding.Of(
                AuditCategory.EmptyRepresentation, $"<representations-empty:{o.Id.Value}>", Some(o.Id)));

    // A SOURCE-BOUND census, not a fault: the Interface key names blob geometry the producing end alone rehydrates,
    // so a crossing that leaves that end is incomplete without the store travelling with it.
    static Seq<AuditFinding> Interfaces(ElementGraph graph) =>
        toSeq(graph.Edges)
            .Choose(static edge => edge is Relationship.Connect { Interface.IsSome: true } connect ? Some(connect) : None)
            .Map(static connect => AuditFinding.Of(
                AuditCategory.SourceBoundInterface, $"<connect-interface:{connect.From.Value}:{connect.To.Value}>"));

    // ONE sweep over three non-current lifecycle states, the OUTCOME token riding the detail — a fourth state joins
    // the filter rather than minting a fourth sweep and a fourth category.
    static Seq<AuditFinding> Assessments(ElementGraph graph) =>
        toSeq(graph.Nodes.Values)
            .Choose(static node => node is Node.Assessment assessment ? Some(assessment) : None)
            .Filter(static a => a.Payload.Outcome == AssessmentOutcome.Stale
                             || a.Payload.Outcome == AssessmentOutcome.Superseded
                             || a.Payload.Outcome == AssessmentOutcome.Failed)
            .Map(static a => AuditFinding.Of(
                AuditCategory.AssessmentStale, $"<assessment-{a.Payload.Outcome.Key}:{a.Id.Value}>", Some(a.Id)));

    // The TAMPER census folds Verify's accumulating Validation — the sweep is already complete because independent
    // node checks accumulate, so this unpacks ManyErrors rather than re-verifying node by node.
    static Seq<AuditFinding> Drift(ElementGraph graph, Op key) =>
        ContentAddress.Verify(graph, key).Match(
            Succ: static _ => Seq<AuditFinding>(),
            Fail: static error => (error is ManyErrors many ? many.Errors : Seq(error))
                .Map(static drift => AuditFinding.Of(AuditCategory.AddressDrift, drift.Message)));

    // --- [COVERAGE_SHORTFALL]
    // A missed floor is a Warning FINDING, so integrity and coverage ride ONE stream and a report needs no second
    // shape. Clears stays the verdict; these rows are what it would name.
    static Seq<AuditFinding> Shortfalls(CoverageCensus census, AuditThresholds policy) =>
        Seq<(string Axis, double Share, double Floor)>(
            ("classified", census.Classified.Share, policy.Classified),
            ("material-bound", census.MaterialBound.Share, policy.MaterialBound),
            ("quantified", census.Quantified.Share, policy.Quantified),
            ("propertied", census.Propertied.Share, policy.Propertied))
        .Filter(static row => row.Share < row.Floor)
        .Map(static row => AuditFinding.Of(AuditCategory.CoverageShortfall, $"<coverage-shortfall:{row.Axis}>"))
        // The per-discipline floors walk through AsIterable for the same reason the tally fold does — the key is
        // load-bearing in the detail token, and a two-parameter HashMap's own fold carries the value alone.
        + policy.Assessed.AsIterable().Fold(Seq<AuditFinding>(), (rows, declared) =>
            census.ByDiscipline.Find(declared.Key).Map(static row => row.Assessed.Share).IfNone(1.0) < declared.Value
                ? rows.Add(AuditFinding.Of(AuditCategory.CoverageShortfall, $"<coverage-shortfall:assessed:{declared.Key.Key}>"))
                : rows);
}
```

## [05]-[RESEARCH]

(none)
