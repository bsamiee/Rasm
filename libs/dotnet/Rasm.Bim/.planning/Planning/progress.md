# [BIM_PROGRESS]

`ProgressVerification` owns scan-derived physical progress: one `Compare` fold joins a reality-capture `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` of `Exchange/reconstruct#RECONSTRUCTION`-authored occurrences to the as-designed graph and the `Planning/schedule#SCHEDULE` `ScheduleNetwork`, producing one `ProgressReport` — a `ProgressEvidence` row per `ConstructionTask` carrying the MEASURED `Observed` completion fraction beside the schedule-derived `Expected` fraction at the capture instant, their signed `Variance`, the matched-against-expected element counts, the below-confidence `Uncertain` count, and the capture identity — beside the `Unmatched` residue of reconstructed occurrences no task's assignment claims. Progress here is MEASURED, never authored: `Observed` divides a task's captured elements by its assigned ones, so a task whose `IfcTaskTime.Completion` claims ninety percent against a capture resolving half its assigned set reads a negative `Variance` a 4D report ranks as a dispute-grade finding rather than as a self-reported percent.

`Compare` reads two graph snapshots and one activity network as a VIEW, never a second element, schedule, or capture store. `Node.Object.ExternalId` joins observed to as-designed — the `Review/diff#MODEL_DIFF` federation idiom over the Bim-stored `Rasm.Element/Graph/element#NODE_MODEL` IFC `GlobalId`, the ONE cross-party stable identity, never the neutral kernel `NodeId` a re-ingest re-mints — and a reconstructed occurrence carries the DETERMINISTIC `ExternalId` the reconstruction hashes from its `recon:`-prefixed `ReconstructionKey` preimage, so a re-run at identical fit parameters re-keys identically and a capture joins the same counterparts across passes. Every set the fold reads composes the `Model/query#ELEMENT_SET` `BimTerm` algebra alone — capture occurrences through `ByKind(ObjectKind.Occurrence)`, the below-floor set through `ByProperty` over the `Pset_Reconstruction` `NeedsReview` row, each assigned set through `ByAttribute(ObjectAttribute.GlobalId, ValueMatch.OneOf(…))` — so no second selection surface and no direct bag read exists here. `Expected` composes the ONE `Planning/cost#EARNED_VALUE` `CostPerformance.Fraction` clamped interval-fraction law, so an expected completion and an EVM planned value never disagree at one instant. Rejections lower onto `Model/faults#FAULT_BAND` `BimFault` (band 2600, `Fault`-derived), the typed case lifting BARE onto the `Fin<T>` rail with no `.ToError()` hop: `ScheduleNetwork.BindAssignments` gates a task assigning a `GlobalId` the as-designed graph never declares, so the rejection stays that owner's `BimFault.Refused` with `BimReason.DanglingReference` under its own `task-assigns-absent-element` token — one owner, one refusal, never a progress-local restatement.

## [01]-[INDEX]

- [02]-[PROGRESS_EVIDENCE]: the `ProgressEvidence` per-task measured-against-planned row (`Observed`/`Expected`/`Variance`, the matched-against-expected element counts, the below-confidence `Uncertain` column, the `CaptureKey` content key, the capture `Instant`), the `ProgressReport` carrying that row set beside the `Unmatched` reconstructed-occurrence residue, and the `ProgressVerification.Compare` fold joining one capture graph to the as-designed graph and the activity network.

## [02]-[PROGRESS_EVIDENCE]

- Owner: `ProgressEvidence` the per-task verification row promoting one `Planning/schedule#SCHEDULE` `ConstructionTask` into a measured-progress fact carrying its `TaskGlobalId`, the `Option<double> Observed` resolved-element fraction (absent where the task assigns no geometry to measure), the `Expected` planned fraction at the capture instant, the derived `Option<double> Variance` gap propagating that absence, the `MatchedElements`/`ExpectedElements` counts the fraction divides, the `Uncertain` below-confidence count carried BESIDE the fraction rather than inside it, the `CaptureKey` content key joining the row to the exact capture, and the capture `At` instant; `ProgressReport` the report carrying the `Seq<ProgressEvidence>` row set, the `Unmatched` reconstructed-occurrence `GlobalId` residue no task claims, and the same capture identity and instant every row repeats so a dashboard reads one join key off the report head; `ProgressVerification` the static fold over one capture graph, one as-designed graph, and one activity network.
- Entry: `ProgressVerification.Compare(ScheduleNetwork network, ElementGraph asDesigned, ElementGraph observed, Instant captureAt, Op key)` folds the three inputs into one `Fin<ProgressReport>` — resolving the network's assignments against the as-designed graph through the settled `ScheduleNetwork.BindAssignments` gate FIRST (so a task naming an absent element aborts at the schedule owner's own law rather than silently scoring zero progress against geometry the model never declared), selecting the capture's occurrence set and its below-confidence subset through the `Model/query#ELEMENT_SET` predicate algebra, folding each task's assigned `GlobalId` set against the VERIFIED capture ids for `Observed`, reading `Expected` off the task's scheduled `Interval` through `Planning/cost#EARNED_VALUE` `CostPerformance.Fraction`, deriving the `CaptureKey` over the capture's own `Pset_Reconstruction` `SourceCloud` rows, and collecting every capture occurrence outside the union of the assignment sets as the `Unmatched` residue; the fold is TOTAL past that one gate — a task with no assignment MEASURES NOTHING (`Observed` absent, `Variance` absent with it) rather than faulting or reading zero, because an unassigned activity claims no geometry a capture confirms and a zero there is a false measurement a report ranks as the worst variance on the board.
- Auto: `Compare` reads the capture graph as an occurrence set (`ByKind(ObjectKind.Occurrence)`) and splits it once — the `ByProperty` restriction over the `Pset_Reconstruction` `NeedsReview` row (the flag the `Exchange/reconstruct#RECONSTRUCTION` fold stamps when a fit falls under its `ConfidenceFloor`) partitions the below-floor occurrences off through the same-graph `ElementQuery.Except`, so the verified set and the flagged set are two projections of one selection and no occurrence lands in both; each task's `Observed` is `MatchedElements / ExpectedElements` over the VERIFIED ids alone while the flagged intersection rides `Uncertain`, so a low-confidence fit never inflates a completion fraction and never disappears from the report either; `Expected` is the clamped elapsed fraction of the task's scheduled `Interval` at `captureAt` — the schedule's planned window, never the actual one, because the comparison the report exists to make is measured-against-PLAN; the `CaptureKey` folds the ordinal-sorted DISTINCT `SourceCloud` lineage texts the capture occurrences publish through the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress` over the kernel `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter` — length-prefixed per row under its `Ordinal` count — so a single-scan verification keys off that one lineage, a federated multi-scan verification off its whole set, and a dashboard re-reads a report only when the capture behind it genuinely moved; the `Unmatched` residue is the capture's `GlobalIds` minus the union of every assignment set, the as-built evidence a coordination read treats as unplanned work or as an unmodelled existing condition.
- Output: the `ProgressReport` is the typed physical-progress evidence — `Planning/cost#EARNED_VALUE` reads the PRESENT per-task `Observed` fractions as the `Map<string, double> observed` the `CostSchedule.EarnedValue` actual-percent election consumes at its HIGHEST tier (evidence outranking the authored `IfcTaskTime.Completion`, an absent measurement falling through to the next tier rather than entering as a zero), and the `Rasm.AppUi/Charts` progress dashboard reads the row set by the `CaptureKey` so a re-render costs nothing on an unchanged capture; `Variance` is the per-task finding a 4D report ranks where a measurement exists, `Uncertain` the data-coverage verdict a reviewer reads before trusting any fraction, and `Unmatched` the as-built residue the coordination board turns into an issue — each carried on the one report, never a second progress store.
- Packages: Rasm, Rasm.Element, LanguageExt.Core, NodaTime
- Growth: a new evidence axis (a per-task volumetric coverage, a capture-density measure) is one column on `ProgressEvidence` the same fold fills and one write on the same `CanonicalWriter` preimage; a new confidence band is one `Pset_Reconstruction` row the existing `ByProperty` restriction re-keys with no new arm; a new capture modality (a photogrammetry capture beside a LAS scan) is `Exchange/reconstruct#RECONSTRUCTION`'s concern entirely — this page joins seam OCCURRENCES and never point clouds, so a new upstream source reaches it as the same `ElementGraph` with zero edits here; a new consumer of the observed fractions reads the existing `ProgressEvidence` row; never a per-task progress record, never a parallel progress-side element or schedule store, never a second capture identity scheme, and never a proximity or geometry matcher minted here.
- Boundary: `Observed` is MEASURED evidence carried as an `Option` — an unmeasurable task reads absent and a fabricated zero is the deleted form — and re-deriving it from the schedule's authored `PercentComplete` is the named seam violation — the schedule owns the authored claim, this page owns the physical one, and the whole value of the pairing is that the two can disagree; a below-confidence occurrence is EXCLUDED from `Observed` and counted on `Uncertain`, so folding a flagged fit into a completion fraction (reading an unverified element as built) and dropping it silently (erasing the coverage question a reviewer asks first) are both deleted forms; the observed-to-as-designed correspondence is the deterministic `Node.Object.ExternalId` the reconstruction mints, and a spatial-proximity or bounding-box matcher minted here is the deleted form — the correspondence is upstream evidence, never a heuristic this fold invents; every set read is the `Model/query#ELEMENT_SET` `BimTerm` algebra and a direct `Pset_Reconstruction` bag read, a raw `graph.ObjectNodes` scan, or a `Func<Node.Object, bool>` filter beside it is the no-second-selection-surface reject, while the bag and row names compose the `Exchange/reconstruct#RECONSTRUCTION` `ReconstructionRows` statics and a page-local const restating them is the deleted form; `Expected` is the ONE `Planning/cost#EARNED_VALUE` `CostPerformance.Fraction` law and a second clamped elapsed-fraction fold beside it is the deleted form — a progress report and an EVM planned value disagreeing at one instant is the drift that law forecloses; the `CaptureKey` is a typed seam `ContentAddress` minted over the `CanonicalWriter` preimage — the one content-key type the `Review/diff#MODEL_DIFF` `ElementFingerprint`, the `Review/versioning#VERSION_GRAPH` `CommitKey`, and the `Review/validation#IDS_FACETS` `FacetKey` are stated in, so a raw `UInt128` field here erases that type at the exact edge a dashboard joins on — and re-minting an `Exchange/reconstruct#LAS_INGEST` `CaptureLineage` over the lineage SET is the deleted form the reconstruction's own one-value-type-two-key-spaces ruling already closed — that value object addresses ONE capture's source bytes; the assignment gate is the settled `ScheduleNetwork.BindAssignments` fold and a progress-local dangling-reference check with its own detail token is the deleted form; a verification rejection lifts the typed `BimFault` case BARE onto the `Fin<T>` rail and a `.ToError()` lowering hop or a one-arg ctor bypassing the kernel `Op` context is the named seam defect.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Bim.Model;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Query;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;
using BimTerm = Rasm.Element.Query.Predicate<Rasm.Bim.Model.BimLeaf>;
using IdSet = LanguageExt.HashSet<string>;

namespace Rasm.Bim.Planning;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ProgressEvidence(
    string TaskGlobalId,
    Option<double> Observed,
    double Expected,
    int MatchedElements,
    int ExpectedElements,
    int Uncertain,
    ContentAddress CaptureKey,
    Instant At) {
    public Option<double> Variance => Observed.Map(observed => observed - Expected);
}

public sealed record ProgressReport(
    Seq<ProgressEvidence> Tasks,
    Seq<string> Unmatched,
    ContentAddress CaptureKey,
    Instant At);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ProgressVerification {
    static readonly BimTerm Flagged = BimLeaf.Of(new ElementLeaf.ByProperty(
        new ValueMatch.Exact(new PropertyValue.Text(ReconstructionRows.Set)),
        new ValueMatch.Exact(new PropertyValue.Text(ReconstructionRows.NeedsReview.Value)),
        new ValueMatch.Exact(new PropertyValue.Boolean(true))));

    static readonly ValueSource Lineage = new ValueSource.Property(ReconstructionRows.Set, ReconstructionRows.SourceCloud.Value);

    public static Fin<ProgressReport> Compare(ScheduleNetwork network, ElementGraph asDesigned, ElementGraph observed, Instant captureAt, Op key) =>
        network.BindAssignments(asDesigned, key).Map(bound => {
            ElementQuery capture = ElementQuery.Query(observed, BimLeaf.Of(new ElementLeaf.ByKind(ObjectKind.Occurrence)));
            ElementQuery uncertain = capture.Where(Flagged);
            IdSet verified = toHashSet(capture.Except(uncertain).GlobalIds);
            IdSet flagged = toHashSet(uncertain.GlobalIds);
            var assigned = bound.Assignments.Fold(Map<string, Seq<string>>(), static (rows, row) =>
                rows.AddOrUpdate(row.TaskGlobalId, held => held + row.ElementGlobalIds, row.ElementGlobalIds));
            IdSet claimed = toHashSet(bound.Assignments.Bind(static row => row.ElementGlobalIds));
            ContentAddress captureKey = CaptureKeyOf(observed, capture);
            return new ProgressReport(
                bound.Tasks.Map(task => Evidence(
                    task, assigned.Find(task.GlobalId).IfNone(Seq<string>()), verified, flagged, captureAt, captureKey)),
                capture.GlobalIds.Filter(id => !claimed.Contains(id)),
                captureKey,
                captureAt);
        });

    static ProgressEvidence Evidence(
        ConstructionTask task, Seq<string> assigned, IdSet verified, IdSet flagged,
        Instant captureAt, ContentAddress captureKey) {
        int matched = assigned.Filter(id => verified.Contains(id)).Count;
        return new ProgressEvidence(
            task.GlobalId,
            assigned.IsEmpty ? Option<double>.None : Some((double)matched / assigned.Count),
            CostPerformance.Fraction(task.Scheduled, captureAt),
            matched,
            assigned.Count,
            assigned.Filter(id => flagged.Contains(id)).Count,
            captureKey,
            captureAt);
    }

    static ContentAddress CaptureKeyOf(ElementGraph observed, ElementQuery capture) {
        Seq<string> lineages = toSeq(capture.Objects
            .Bind(obj => ElementQuery.ValuesOf(observed, obj, Lineage))
            .Map(static value => value.Render())
            .Distinct()
            .OrderBy(static text => text, StringComparer.Ordinal));
        return ContentAddress.Of(lineages, 0.0, static (rows, writer) =>
            rows.Fold(writer.Ordinal(rows.Count), static (w, text) => w.String(text)));
    }
}
```

## [03]-[RESEARCH]

(none)
