# [RASM_FABRICATION_TRAVELER]

The traveler owner is the terminal forward shop-document model for `Run(Document{results})`: it assembles completed fabrication evidence into canonical section rows, keys the document with `ContentKey.Of(EgressKind.Traveler, bytes)`, and returns only `FabricationResult.TravelerDocument(ContentKey Key, Seq<ContentKey> Composed)`. The composer is the widest fan-in node in the folder, so every upstream owner that contributes to shop execution exposes a typed receipt before it enters this model; projection views, magazine tool lists, setup plans, program facts, tolerance frames, capability rows, DfM reports, and WPS qualification receipts are carried forward as typed rows, never re-derived from raw geometry, raw program text, or plane-internal state. The traveler carries the shop-floor half beside the engineering half: work-order identity (order, part, revision, quantity, heat lot, serials), per-operation hold points naming the buy-off authority, and work instructions — the accountability axes a traveler exists to be audited FOR.

The traveler is forward execution truth. `QualityReport` owns as-built quality records and sealed inspection evidence; `TravelerDocument` owns the pre-work and in-work shop packet. Rendering, annotation, sheets, PDFs, and artifact layouts ride the artifacts-plane seam after this page emits the keyed document model; Persistence owns the `traveler` artifact-index enrollment row and reads only the content key spine.

## [01]-[INDEX]

- [01]-[TRAVELER_DOCUMENT]: owns `TravelerSectionKind`, the nine typed section row families, `TravelerReceiptCorpus`, the one-pass `Harvest` result fold, `TravelerDocumentBody` with its STABLE canonical writer, `TravelerDocumentModel`, and the internal `Traveler.Assemble` case body for `Run(Document{Seq<FabricationResult>})`.

## [02]-[TRAVELER_DOCUMENT]

- Owner: `TravelerSectionKind` closes the nine-section vocabulary; `TravelerHeaderRow`, `TravelerIdentityRow`, `TravelerSetupRow`/`TravelerHold`/`TravelerOperationRow`, `TravelerToolRow`, `TravelerProgramRow`, `TravelerSpecRow`, `TravelerProcedureRow`, `TravelerViewRow`, and `TravelerRecordRow` carry the typed document rows; `TravelerReceiptCorpus` is the fan-in pack (its `Records` lane carries the report page's `SealedRecord` receipts, its `Dfm` lane the manufacturability reports, its `Procedures` lane the WPS qualification receipts, its `Identity`/`Holds`/`Instructions` lanes the shop-floor axes); `TravelerDocumentBody` is the canonical byte source; `TravelerDocumentModel` is the keyed receipt.
- Cases: `TravelerSectionKind` rows 9 — `header`, `identity`, `operation`, `tool`, `program`, `spec`, `procedure`, `view`, `record`; `TravelerSection` union cases 9, one per row family, its canonical writer a generated TOTAL `Switch` — a tenth section case breaks the build at the writer until its arm lands, never a runtime catch-all; the result fan-in is ONE `Harvest` fold through the generated total `FabricationResult.Switch` — ten arms in one pass, never five parallel language switches; `Traveler.Assemble` remains an internal case body invoked by owner#run and never a second public fold.
- Entry: owner#run dispatches `Document(Seq<FabricationResult> Results)` into `internal static Fin<FabricationResult> Traveler.Assemble(FabricationPolicy.Document policy, FabricationInput input)`; the receipt-enriched 4-arg form threads a populated `TravelerReceiptCorpus` and an explicit `Instant`; the outer public entry remains `Fabrication.Run`. The corpus is unreachable from the bare `Document(Seq<FabricationResult>)` case — the corpus rides the policy case when owner#atoms lands the `Document(Results, Corpus)` widening; until then the 2-arg arm composes `TravelerReceiptCorpus.Empty` and only result-derived sections populate through `Run`.
- Auto: `Assemble` folds the result set ONCE through the generated `FabricationResult.Switch` into the `Harvest` accumulator (program sections, view sections, part ids, plan steps, and the composed key spine in a single pass), projects the corpus receipts to document-altitude rows (`Setup` lowers to `TravelerSetupRow(Datum, ReachableOps, Clamps)` — WCS datum lineage and reach facts, never fixture geometry; `ToolAssembly` lowers to its `ContentHash.Of` `Identity` digest), orders sections by `TravelerSectionKind.Order`, writes STABLE canonical bytes — every row field spelled explicitly under the invariant culture, free text netstring-framed (`{length}:{text}` — injective against delimiter collision), smart-enum keys, `R`-format scalars, ISO instants, and content-key digests; NO `GetHashCode` reaches the byte source, because record hash codes are process-randomized and a content key must be stable across processes — then mints `ContentKey.Of(EgressKind.Traveler, bytes)` and returns `TravelerDocument(Key, Composed)`.
- Receipt: `TravelerDocument(ContentKey Key, Seq<ContentKey> Composed)` is the only owner#atoms result case. `Composed` is the provenance spine over every upstream content key in the completed result set: placement, additive artifacts, verification residual and setup snapshots, posted programs, plan artifacts, formed outputs, sealed quality-record keys from the corpus `Records` lane, and — for a prior traveler — the prior DOCUMENT key plus its composed spine.
- Packages: owner#atoms (`EgressKind.Traveler`, `ContentKey`, `FabricationPolicy.Document`, `FabricationResult`, `FabricationInput`, `PlannedStep`, `StockSnapshot`, `CapabilityVerdict`, `ProjectionDir`, `Edge3`), `Documentation/projection` (`HiddenLineResult` via owner result), `Tooling/magazine` (`ToolChange`, `ToolAssembly.Identity`), `Fixturing/setups` (`SetupSchedule`, `Setup`, `WcsAssignment`, `WcsDatum`, `WcsSlot`), `Spec/tolerance` (`FeatureFrameReceipt`), `Spec/capability` (`CapabilityReport`, `CapabilityRow`, `SpcLimitRow`), `Spec/manufacturability` (`DfmReport`, `DfmVerdict`, `DfmLocus`, `RoutingRow`), `Joining/procedure` (`ProcedureReceipt`, `ComplianceRow`), NodaTime (`Instant`, `InstantPattern.ExtendedIso`, `SystemClock.Instance.GetCurrentInstant` — the ONE boundary clock read in the 2-arg arm), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`CultureInfo`); cross-package: Persistence artifact index owns the `traveler` enrollment row.
- Growth: a new traveler section is one `TravelerSectionKind` row, one `TravelerSection` case, one row record, and one writer `Switch` arm — the generated dispatch breaks the build until the arm lands; a new upstream receipt contributes through `TravelerReceiptCorpus`; a new result contribution is one `Harvest` arm the generated result `Switch` already demands; a new output medium remains an artifacts-plane consumer; a new as-built record remains a `QualityRecord` case.
- Boundary: Documentation has no local fault arms, and upstream owners validate their receipts before this terminal compose. Traveler mints no GD&T frames, no tool-life measurements, no WCS roster, no program AST, no setup graph (the operation section carries the datum/reach PROJECTION of the schedule, never a re-minted `SetupSchedule`), no quality-record CONTENT (sealed records enter as keys and typed receipts through the corpus lane), no sheet annotation, and no artifact layout. Missing typed receipt exposure is an upstream corpus defect, not a local reconstruction path; `ContentKey.Of` is the only key mint, raw hashing is outside the page, and a `GetHashCode` fold in the canonical writer is the named process-randomization defect. `PostedProgram` carries no dialect column, so the program row stamps the ONE `input.Dialect` — the per-program dialect column is an owner#atoms widening, recorded there.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Globalization;
using System.Text;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using NodaTime.Text;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Joining;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Spec;
using Rasm.Fabrication.Tooling;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Documentation;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class TravelerSectionKind {
    public static readonly TravelerSectionKind Header = new("header", order: 0);
    public static readonly TravelerSectionKind Identity = new("identity", order: 1);
    public static readonly TravelerSectionKind Operation = new("operation", order: 2);
    public static readonly TravelerSectionKind Tool = new("tool", order: 3);
    public static readonly TravelerSectionKind Program = new("program", order: 4);
    public static readonly TravelerSectionKind Spec = new("spec", order: 5);
    public static readonly TravelerSectionKind Procedure = new("procedure", order: 6);
    public static readonly TravelerSectionKind View = new("view", order: 7);
    public static readonly TravelerSectionKind Record = new("record", order: 8);

    public int Order { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record TravelerHeaderRow(
    ProcessKind Process,
    Machine Machine,
    ProjectionDir View,
    Seq<int> PartIds,
    Instant StampedAt);

// The accountability identity: which lot of which material becomes which serialized parts under which work order.
public sealed record TravelerIdentityRow(
    string WorkOrder,
    string Part,
    string Revision,
    int Quantity,
    Option<string> HeatLot,
    Seq<string> Serials);

// Document-altitude setup projection: datum lineage, WCS binding, reach facts, clamp count — never fixture geometry.
public sealed record TravelerSetupRow(WcsDatum Datum, Arr<int> ReachableOps, int Clamps);

// A hold point: work stops after the named step until the named authority buys off.
public sealed record TravelerHold(int AfterStep, string Authority);

public sealed record TravelerOperationRow(
    Seq<TravelerSetupRow> Setups,
    Seq<WcsAssignment> Wcs,
    Seq<(int Before, int After)> Precedence,
    Seq<PlannedStep> Steps,
    Seq<StockSnapshot> Stock,
    Seq<TravelerHold> Holds,
    Seq<string> Instructions);

// Assemblies ride as their ContentHash.Of identities — the magazine owns the assembly interior.
public sealed record TravelerToolRow(Seq<ToolChange> Changes, Seq<UInt128> Assemblies);

public sealed record TravelerProgramRow(
    Option<PostDialect> Dialect,
    int BlockCount,
    ContentKey Key);

public sealed record TravelerSpecRow(
    Seq<FeatureFrameReceipt> Frames,
    Seq<CapabilityRow> Capability,
    Seq<SpcLimitRow> Limits,
    Option<CapabilityVerdict> Verdict,
    Seq<DfmReport> Dfm);

// The WPS-per-joint fan-in Joining/procedure declares: the traveler renders the qualification receipt rows.
public sealed record TravelerProcedureRow(Seq<ProcedureReceipt> Procedures);

public sealed record TravelerViewRow(
    Seq<Edge3> Visible,
    Seq<Edge3> Hidden,
    Seq<Edge3> Silhouette);

// The sealed quality-record fan-in lane: report.md's SealedRecord receipts enter the traveler HERE as a typed
// section, their content keys riding the Composed spine — as-built records are traveler-composed, never re-built.
public sealed record TravelerRecordRow(Seq<SealedRecord> Records);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TravelerSection {
    private TravelerSection() { }

    public abstract TravelerSectionKind Kind { get; }

    public sealed record Header(TravelerHeaderRow Row) : TravelerSection {
        public override TravelerSectionKind Kind => TravelerSectionKind.Header;
    }

    public sealed record Identity(TravelerIdentityRow Row) : TravelerSection {
        public override TravelerSectionKind Kind => TravelerSectionKind.Identity;
    }

    public sealed record Operation(TravelerOperationRow Row) : TravelerSection {
        public override TravelerSectionKind Kind => TravelerSectionKind.Operation;
    }

    public sealed record Tool(TravelerToolRow Row) : TravelerSection {
        public override TravelerSectionKind Kind => TravelerSectionKind.Tool;
    }

    public sealed record Program(TravelerProgramRow Row) : TravelerSection {
        public override TravelerSectionKind Kind => TravelerSectionKind.Program;
    }

    public sealed record Spec(TravelerSpecRow Row) : TravelerSection {
        public override TravelerSectionKind Kind => TravelerSectionKind.Spec;
    }

    public sealed record Procedure(TravelerProcedureRow Row) : TravelerSection {
        public override TravelerSectionKind Kind => TravelerSectionKind.Procedure;
    }

    public sealed record View(TravelerViewRow Row) : TravelerSection {
        public override TravelerSectionKind Kind => TravelerSectionKind.View;
    }

    public sealed record Record(TravelerRecordRow Row) : TravelerSection {
        public override TravelerSectionKind Kind => TravelerSectionKind.Record;
    }
}

public sealed record TravelerReceiptCorpus(
    Seq<ToolChange> ToolChanges,
    Seq<ToolAssembly> ToolAssemblies,
    Seq<SetupSchedule> Setups,
    Seq<FeatureFrameReceipt> Frames,
    Seq<CapabilityReport> Capabilities,
    Seq<DfmReport> Dfm,
    Seq<ProcedureReceipt> Procedures,
    Seq<SealedRecord> Records,
    Option<TravelerIdentityRow> Identity,
    Seq<TravelerHold> Holds,
    Seq<string> Instructions) {
    public static readonly TravelerReceiptCorpus Empty = new(
        Seq<ToolChange>(), Seq<ToolAssembly>(), Seq<SetupSchedule>(), Seq<FeatureFrameReceipt>(), Seq<CapabilityReport>(),
        Seq<DfmReport>(), Seq<ProcedureReceipt>(), Seq<SealedRecord>(), Option<TravelerIdentityRow>.None, Seq<TravelerHold>(), Seq<string>());
}

// ONE pass over the result set: the generated total Switch makes a new FabricationResult case a compile error here
// until its harvest arm lands — five parallel language switches were the collapsed form.
sealed record Harvest(Seq<TravelerSection> Programs, Seq<TravelerSection> Views, Seq<int> Parts, Seq<PlannedStep> Steps, Seq<ContentKey> Composed) {
    public static readonly Harvest Empty = new(Seq<TravelerSection>(), Seq<TravelerSection>(), Seq<int>(), Seq<PlannedStep>(), Seq<ContentKey>());
}

public sealed record TravelerDocumentBody(
    Instant StampedAt,
    Seq<TravelerSection> Sections,
    Seq<ContentKey> Composed) {
    public TravelerDocumentModel Seal() =>
        new(
            ContentKey.Of(EgressKind.Traveler, CanonicalBytes(this)),
            Composed,
            StampedAt,
            Sections);

    public static TravelerDocumentBody Of(
        Seq<FabricationResult> results,
        FabricationInput input,
        TravelerReceiptCorpus corpus,
        Instant stampedAt) {
        Harvest harvest = results.Fold(Harvest.Empty, (h, result) => Gather(h, result, input.Dialect));
        Seq<TravelerSection> sections =
            (Seq<TravelerSection>(
                new TravelerSection.Header(new TravelerHeaderRow(input.Process, input.Machine, input.View, harvest.Parts.Distinct().ToSeq(), stampedAt)),
                new TravelerSection.Operation(Operation(corpus, harvest, input)),
                new TravelerSection.Tool(new TravelerToolRow(corpus.ToolChanges, corpus.ToolAssemblies.Map(static a => a.Identity))))
            + corpus.Identity.Map(static row => (TravelerSection)new TravelerSection.Identity(row)).ToSeq()
            + harvest.Programs
            + Specs(corpus, input)
            + (corpus.Procedures.IsEmpty ? Seq<TravelerSection>() : Seq1((TravelerSection)new TravelerSection.Procedure(new TravelerProcedureRow(corpus.Procedures))))
            + harvest.Views
            + (corpus.Records.IsEmpty ? Seq<TravelerSection>() : Seq1((TravelerSection)new TravelerSection.Record(new TravelerRecordRow(corpus.Records)))))
            .OrderBy(static section => section.Kind.Order).ToSeq();
        return new TravelerDocumentBody(stampedAt, sections, (harvest.Composed + corpus.Records.Map(static r => r.Key)).Distinct().ToSeq());
    }

    static Harvest Gather(Harvest h, FabricationResult result, Option<PostDialect> dialect) =>
        result.Switch(
            hiddenLineResult: view => h with { Views = h.Views.Add(new TravelerSection.View(new TravelerViewRow(view.Visible, view.Hidden, view.Silhouette))) },
            motion: _ => h,
            placement: placement => h with { Parts = h.Parts + placement.Parts.Map(static p => p.PartId), Composed = h.Composed.Add(placement.Key) },
            additiveResult: additive => h with { Composed = h.Composed + additive.Artifacts },
            verificationResult: verified => h with { Composed = h.Composed + verified.Snapshots.Map(static s => s.Key) + Seq1(verified.Residual.Key) },
            inspectionResult: _ => h,
            postedProgram: program => h with {
                Programs = h.Programs.Add(new TravelerSection.Program(new TravelerProgramRow(dialect, program.Blocks.Count, program.Key))),
                Composed = h.Composed.Add(program.Key),
            },
            // A prior TravelerDocument contributes its OWN key plus its composed spine — document lineage never drops.
            travelerDocument: prior => h with { Composed = (h.Composed + Seq1(prior.Key)) + prior.Composed },
            fabricationPlan: plan => h with { Steps = h.Steps + plan.Steps, Composed = h.Composed + plan.Artifacts.Add(plan.Key) },
            formedResult: formed => h with { Composed = h.Composed.Add(formed.Key) });

    // The schedule PROJECTS to document altitude — datum, reach, clamp count — never a re-minted SetupSchedule.
    static TravelerOperationRow Operation(TravelerReceiptCorpus corpus, Harvest harvest, FabricationInput input) =>
        new(
            corpus.Setups.Bind(static schedule => schedule.Setups.ToSeq().Map(static s => new TravelerSetupRow(s.Datum, s.ReachableOps, s.Fixture.Clamps.Count))),
            corpus.Setups.Bind(static schedule => schedule.Wcs),
            corpus.Setups.Bind(static schedule => schedule.Precedence),
            harvest.Steps,
            input.Snapshots,
            corpus.Holds,
            corpus.Instructions);

    static Seq<TravelerSection> Specs(TravelerReceiptCorpus corpus, FabricationInput input) {
        if (!corpus.Capabilities.IsEmpty)
            return corpus.Capabilities
                .Map(report => (TravelerSection)new TravelerSection.Spec(new TravelerSpecRow(corpus.Frames, report.Rows, report.Limits, Some(report.Verdict), corpus.Dfm)));
        return input.Capability.Match(
            Some: verdict => Seq1((TravelerSection)new TravelerSection.Spec(new TravelerSpecRow(corpus.Frames, Seq<CapabilityRow>(), Seq<SpcLimitRow>(), Some(verdict), corpus.Dfm))),
            None: () => corpus.Frames.IsEmpty && corpus.Dfm.IsEmpty
                ? Seq<TravelerSection>()
                : Seq1((TravelerSection)new TravelerSection.Spec(new TravelerSpecRow(corpus.Frames, Seq<CapabilityRow>(), Seq<SpcLimitRow>(), None, corpus.Dfm))));
    }

    // --- [BOUNDARIES] -------------------------------------------------------------------------------------------------------------------------------
    // STABLE canonical bytes (K9): every carried field of every section row contributes explicitly — invariant-culture
    // R-format scalars, smart-enum keys, ISO instants, content-key digests, netstring-framed free text (injective
    // against delimiter collision), typed locus writers through the generated union dispatch. GetHashCode never
    // reaches the byte source: record hash codes are process-randomized and the content key IS the document identity.
    static byte[] CanonicalBytes(TravelerDocumentBody body) =>
        Encoding.UTF8.GetBytes(string.Join(
            "\n",
            body.Sections.Map(SectionLine)
                .Prepend(InstantPattern.ExtendedIso.Format(body.StampedAt))
                .Concat(body.Composed.Map(static key => $"composed|{key.Digest:x32}"))));

    // The generated TOTAL Switch: a tenth section case fails the build here until its writer arm lands.
    static string SectionLine(TravelerSection section) =>
        section.Switch(
            header: static h =>
                $"header|{h.Row.Process.Key}|{h.Row.Machine.Key}|view:{V(h.Row.View.Forward)};{V(h.Row.View.ScreenU)};{V(h.Row.View.ScreenV)}"
                + $"|parts:{string.Join(',', h.Row.PartIds)}|{InstantPattern.ExtendedIso.Format(h.Row.StampedAt)}",
            identity: static i =>
                $"identity|{S(i.Row.WorkOrder)}|{S(i.Row.Part)}|{S(i.Row.Revision)}|{i.Row.Quantity}"
                + $"|lot:{i.Row.HeatLot.Map(S).IfNone("-")}|serials:{string.Join(',', i.Row.Serials.Map(S))}",
            operation: static o =>
                $"operation|setups:{string.Join(',', o.Row.Setups.Map(static s => $"{s.Datum.Setup}:{s.Datum.Slot.Family.Key}{s.Datum.Slot.Ordinal}:{s.Datum.AnchorOperation}:l[{string.Join(';', s.Datum.Lineage)}]:r[{string.Join(';', s.ReachableOps)}]:c{s.Clamps}"))}"
                + $"|wcs:{string.Join(',', o.Row.Wcs.Map(static w => $"{w.Setup}:{w.Slot.Family.Key}{w.Slot.Ordinal}"))}"
                + $"|prec:{string.Join(',', o.Row.Precedence.Map(static p => $"{p.Before}>{p.After}"))}"
                + $"|steps:{string.Join(',', o.Row.Steps.Map(static s => $"{s.Order}:{s.Process.Key}:{s.Machine.Key}:{s.Setup}:{s.Program.Map(static k => $"{k.Digest:x32}").IfNone("-")}"))}"
                + $"|stock:{string.Join(',', o.Row.Stock.Map(static s => $"{s.Key.Digest:x32}"))}"
                + $"|holds:{string.Join(',', o.Row.Holds.Map(static x => $"{x.AfterStep}:{S(x.Authority)}"))}"
                + $"|inst:{string.Join(',', o.Row.Instructions.Map(S))}",
            tool: static t =>
                $"tool|changes:{string.Join(',', t.Row.Changes.Map(static c => $"{c.Slot}:{c.ProgramTool}:{N(c.LengthOffset)}:{N(c.Retract)}:{c.MidJob}:{c.ManualConfirm}"))}"
                + $"|assemblies:{string.Join(',', t.Row.Assemblies.Map(static a => $"{a:x32}"))}",
            program: static p =>
                $"program|{p.Row.Dialect.Map(static d => d.Key).IfNone("-")}|blocks:{p.Row.BlockCount}|key:{p.Row.Key.Digest:x32}",
            spec: static s =>
                $"spec|frames:{string.Join(',', s.Row.Frames.Map(FrameLine))}"
                + $"|capability:{string.Join(',', s.Row.Capability.Map(static c => $"{c.Metric.Key}:{N(c.Value)}:{N(c.Demanded)}:{c.Pass}"))}"
                + $"|limits:{string.Join(',', s.Row.Limits.Map(static l => $"{l.Chart.Key}:{InstantPattern.ExtendedIso.Format(l.At)}:{N(l.Center)}:{N(l.Lower)}:{N(l.Upper)}:{l.Violations}"))}"
                + $"|verdict:{s.Row.Verdict.Map(static v => $"{v.Pass}:{N(v.Cpk)}:{v.DemandedItGrade}").IfNone("-")}"
                + $"|dfm:{string.Join(',', s.Row.Dfm.Map(DfmLine))}",
            procedure: static q =>
                $"procedure|{string.Join(',', q.Row.Procedures.Map(static r => $"{S(r.WpsId)}:{r.Qualified}:{InstantPattern.ExtendedIso.Format(r.At)}:[{string.Join(';', r.Rows.Map(ComplianceLine))}]"))}",
            view: static v =>
                $"view|visible:{string.Join(',', v.Row.Visible.Map(E))}|hidden:{string.Join(',', v.Row.Hidden.Map(E))}|silhouette:{string.Join(',', v.Row.Silhouette.Map(E))}",
            record: static r =>
                $"record|{string.Join(',', r.Row.Records.Map(static x => $"{x.Key.Digest:x32}:{x.Rows}:{x.Measured}:{x.Conforming}:{x.Contradictions}"))}");

    static string FrameLine(FeatureFrameReceipt f) =>
        $"{f.Qif.Key}:{f.Characteristic.Key}:{f.Kind.Key}:{N(f.WidthMm)}"
        + $":m[{string.Join(';', f.Modifiers.Map(static m => m.Key))}]"
        + $":d[{string.Join(';', f.Datums.Map(static d => $"{S(d.Label)}:{d.Precedence.Key}:{d.Material.Key}"))}]:{f.Material.Key}";

    static string DfmLine(DfmReport report) =>
        $"{report.ComponentKey:x32}"
        + $":v[{string.Join(';', report.Verdicts.Map(static v => $"{v.Check.Key}:{v.Severity.Key}:{Locus(v.Locus)}:{N(v.Measured)}:{N(v.Bound)}"))}]"
        + $":r[{string.Join(';', report.Rows.Map(static r => $"{r.Process.Key}:{r.Viable}:b[{string.Join('/', r.Blockers.Map(static b => b.Key))}]:{r.Friction}"))}]"
        + $":{report.StackupPrecheck}";

    // Typed locus and compliance writers ride the generated union dispatch — total, never a catch-all.
    static string Locus(DfmLocus locus) =>
        locus.Switch(
            atPoint: static p => $"p{P(p.Point)}",
            atEdge: static e => $"e{E(e.Edge)}",
            atFace: static f => $"f{f.Face}",
            atJoint: static j => $"j{j.Joint}",
            global: static _ => "g");

    static string ComplianceLine(ComplianceRow row) =>
        row.Switch(
            numeric: static n => $"n{n.Ordinal}:{n.Variable.Key}:{N(n.Demanded)}:{N(n.QualifiedLow)}:{N(n.QualifiedHigh)}:{n.Pass}",
            categorical: static c => $"c{c.Ordinal}:{c.Variable.Key}:{S(c.Demanded)}:{S(c.Qualified)}:{c.Pass}");

    static string S(string raw) => $"{raw.Length}:{raw}";
    static string N(double value) => value.ToString("R", CultureInfo.InvariantCulture);
    static string V(Vector3d v) => $"{N(v.X)},{N(v.Y)},{N(v.Z)}";
    static string P(Point3d p) => $"{N(p.X)},{N(p.Y)},{N(p.Z)}";
    static string E(Edge3 e) => $"{P(e.A)};{P(e.B)}";
}

public sealed record TravelerDocumentModel(
    ContentKey Key,
    Seq<ContentKey> Composed,
    Instant StampedAt,
    Seq<TravelerSection> Sections) {
    public FabricationResult ToResult() =>
        new FabricationResult.TravelerDocument(Key, Composed);
}

internal static class Traveler {
    // The owner#run Document arm: results-only compose at the boundary clock; a receipt-enriched corpus
    // threads the 4-arg form — one fold, never a second public entry.
    public static Fin<FabricationResult> Assemble(FabricationPolicy.Document policy, FabricationInput input) =>
        Assemble(policy, input, TravelerReceiptCorpus.Empty, SystemClock.Instance.GetCurrentInstant());

    public static Fin<FabricationResult> Assemble(
        FabricationPolicy.Document policy,
        FabricationInput input,
        TravelerReceiptCorpus corpus,
        Instant stampedAt) =>
        Fin.Succ(TravelerDocumentBody.Of(policy.Results, input, corpus, stampedAt).Seal().ToResult());
}
```

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    mainBkg: "#44475A"
    nodeBorder: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Traveler fan-in seams
    accDescr: Completed results harvest in one fold while corpus receipts project to document rows; the sealed body mints the content key that crosses to persistence and the artifacts plane.
    Run["owner#run Document(results)"] -->|"Traveler.Assemble"| Traveler["one Harvest fold + corpus projection"]
    Projection["HiddenLineResult views"] -->|"result"| Traveler
    Magazine["ToolChange + ToolAssembly.Identity"] -->|"corpus"| Traveler
    Setup["SetupSchedule → TravelerSetupRow projection"] -->|"corpus"| Traveler
    Program["PostedProgram blocks + key"] -->|"result"| Traveler
    Spec["FeatureFrameReceipt + CapabilityReport + DfmReport"] -->|"corpus"| Traveler
    Procedure["ProcedureReceipt WPS rows"] -->|"corpus"| Traveler
    Shop["identity + holds + instructions"] -->|"corpus"| Traveler
    Traveler -->|"stable canonical bytes"| Key["ContentKey.Of(EgressKind.Traveler, bytes)"]
    Key -->|"receipt"| Result["TravelerDocument(Key, Composed)"]
    Result -->|"enrollment"| Persistence["Persistence traveler artifact index"]
    Result -->|"rendering seam"| Artifacts["artifacts-plane rendering"]
    Quality["QualityReport sealed records"] -.->|"keys + typed rows"| Traveler
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    class Run primary
    class Traveler,Key boundary
    class Projection,Magazine,Setup,Program,Spec,Procedure,Shop,Quality data
    class Result,Persistence,Artifacts annotation
    linkStyle 12 stroke:#6272A4,color:#F8F8F2,stroke-dasharray:4 6
```
