# [RASM_GRASSHOPPER_DOCUMENT_DOCUMENT]

`DocumentScope` is the document spine of the GH2 graph boundary — ONE scope operator owning document minting across the inert/inactive/active tiers, the closed inert-facet read, and ONE settlement gate absorbing the whole `DocumentMethods` verb surface AND the lifecycle/persistence/shelf commands: the former `Apply`/`Transact` arity twins are one `Transact` over one `DocumentAct` union, the case deciding whether it seals, observes, or neither. This page also declares the folder's shared settlement machinery — `GateLane`, `GateReceipt`, `GateOutcome` with its payload records, and the `DocumentGate.Run` spine every `Document/*` gate composes — so the five gates that each re-spelled the open-timeline/resolve/stamp/marshal preamble now spell one line.

Every gate takes the session's injected `MonotonicTimeline` REQUIRED (folder RULINGS `[02]`), crosses the marshal through the kernel's SYNCHRONOUS `UiThread.Run` arity, and settles a `GaugedSpan<GateLane>` — entry, settlement, latency, and the budget verdict all derive from the kernel gauge, never a stored stamp pair. Mutation gate fires the `document.mutate` veto row on the hook rail before committing, which is that row's fire-site producer.

## [01]-[INDEX]

- [02]-[LIFECYCLE]: `DocumentTier` + `MarkPosture` + `DocumentFacet` + `DocumentAnswer` + `GateLane` + `GateOutcome` + `GateReceipt` + `DocumentGate` — mint and archive-load rows, the closed facet read, the folder's settlement machinery, and the shared gate spine.
- [03]-[TRANSACT]: `SelectionTarget` + `SelectionSweep` + `SelectionPosture` + `DeleteDepth` + `IsolationAxis` + `DocumentAct` + `DocumentScope` — the one settlement union over the whole verb surface with its selection-target discriminant, the wrap preflight, the seal law, and the causal-delta window.

## [02]-[LIFECYCLE]

- Owner: `DocumentTier` `[SmartEnum<int>]` — 3 mint rows over one `Mint()` column (`NewInertDocument`/`NewInactiveDocument`/`NewActiveDocument`); tier is data, so headless pipelines, background parsing, and canvas-bound editing mint through one gate. `MarkPosture` `[SmartEnum<int>]` — the dirty flag as two rows over one `Stamp` column, so the polarity is a named row, never a boolean payload. `DocumentFacet` `[SmartEnum<int>]` — the closed inert-read vocabulary over one `Project(HostDocument) -> DocumentAnswer` column; `DocumentAnswer` `[Union]` the closed result, one case per evidence shape. Mapperly is REFUTED for this page's projections — the facets hand out host accessor OBJECTS, not field correspondences.
- Owner: `GateLane` `[SmartEnum<int>]` realizing `IGaugeLane<GateLane>` — the folder's gauge vocabulary: `Lifecycle`, `Mutate`, `Undo`, `Solve` (reads settle answers, not receipts, so no read lane exists to go unread); each row's bound DERIVES from the kernel dispatch lane it crosses on, so the four gates that measured with no declared bound now measure against one.
- Owner: `GateReceipt<TFacts>` — the folder's ONE settlement evidence, generic over its FACTS case (E-G47): the settled case's generated `SelfOp` identity, the `Option<VerbNoun>` seal (present exactly when the act minted an undo record), the `GateOutcome` the host verb answered, the `GaugedSpan<GateLane>`, and the gate's own facts — for every document gate the causal `Seq<UiEvent<GhFact>>` delta window (empty for unobserved gates), for `Shell/editor.md`'s gate its settled shell posture. `GateOutcome` `[Union]` closes what a host verb hands back — `SettledCase`, `CountCase`, `ClearedCase` (typed apart so cleared never reads as removed), `ChangedCase`, `MintedCase`, `RefusedCase` (the host's own why-not from a preflighted wrap, settled with no seal), `RemapCase`, `WirelessCase(WirelessPair)`, `RepairCase(Seq<PinRepairRow>)`, `RunCase(RunPulse)`. Three payload records live HERE beside the spine that carries them (`Document/graph.md` composes them — a spine importing its payload page was the inverted layering), and the former `ObservedCase` wrapper deletes: deltas are a RECEIPT column, not an outcome nesting.
- Owner: `DocumentGate` — the shared gate SPINE: resolve the target document (supplied or session-acquired), cross the marshal synchronously, run the body, gauge the whole window on the lane, and mint the receipt. Five `Document/*` gates compose it and none re-spells the preamble.
- Entry: `DocumentScope.Mint(tier, key)` → `Fin<HostDocument>`; `Load(IReader, key)` — the archive mint; `ReadProbe<T>(reader, name, key)` → `Fin<Option<T>>` — probe-gates-accessor (`HasItemOrNode` before `Storable<T>`); `Read(facet, graph, key)` → `Fin<DocumentAnswer>`; `Recall<T>(name, graph, key)` → `Fin<Option<T>>` — ABSENCE rides the option; the caller-supplied fallback is deleted, because a fabricated default past the boundary spelled absence as a value; `Roster(key)` — the live `AllDocuments` sweep.
- Law: absence of a target document is a modality, never an overload — `Option<HostDocument>` discriminates the supplied graph from the session-active document, and the absent branch resolves through `GhSession.Run(ScopeTarget.DocumentHost, …)`.
- Law: every gate settles inside one marshal window through the kernel SYNC arity — `UiThread.Run(new UiDispatch<T>.Blocking(body), DispatchLane.Interactive, key)` composes inside the gate's own `Fin` query; the live `HostDocument` never crosses back out, because `Read` takes a facet ROW and returns the row's own answer case — a caller lambda over the live document voids that law from the signature down.
- Law: `Read` owns the INERT facets alone — `Objects` is `Document/graph.md`'s, `Undo` is `Document/history.md`'s, `Solution` is `Document/solution.md`'s, `Methods` is `[03]`'s gate.
- Law: keyed state is one shelf — `Document.CustomValues` is the host's only `KeyedValues` facet; a shelf selector over a one-inhabitant vocabulary carries no information.
- Law: the archive is symmetric on ONE axis — `Store(IWriter[, FileContents])` writes and the `Document(IReader)` constructor reads, so the load posture is a MINT beside `DocumentTier`, never a gate case.
- Boundary: autosave requests are per-object and ride `Document/history.md`; file-compare and editor reveal are `Shell/editor.md`'s; the retired session-cache eviction claim is gone with the cache estate.
- Packages: Grasshopper2 (the document surface), GrasshopperIO (`IWriter`, `IReader`, `IStorable`), `Rasm.Interaction` (`UiThread`, `UiDispatch<T>`, `DispatchLane`), `Rasm.Parametric` (`MonotonicTimeline`, `GaugedSpan`, `IGaugeLane`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new mint posture is one `DocumentTier` row; a new inert read is one facet row with its answer case; a new host answer shape is one `GateOutcome` case; a new gauged gate is one `GateLane` row.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Grasshopper2.Doc;
using Grasshopper2.Parameters.Special;
using Grasshopper2.Undo;
using GrasshopperIO;
using Rasm.Domain;
using Rasm.Grasshopper.Shell;
using Rasm.Interaction;
using Rasm.Parametric;
using HostDocument = Grasshopper2.Doc.Document;

namespace Rasm.Grasshopper.Document;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class DocumentTier {
    public static readonly DocumentTier Inert = new(key: 0, mint: static () => HostDocument.NewInertDocument());
    public static readonly DocumentTier Inactive = new(key: 1, mint: static () => HostDocument.NewInactiveDocument());
    public static readonly DocumentTier Active = new(key: 2, mint: static () => HostDocument.NewActiveDocument());
    [UseDelegateFromConstructor] internal partial HostDocument Mint();
}

[SmartEnum<int>]
public sealed partial class MarkPosture {
    public static readonly MarkPosture Dirty = new(key: 0, stamp: static document => Op.Side(action: document.Modify));
    public static readonly MarkPosture Clean = new(key: 1, stamp: static document => Op.Side(action: document.Unmodify));
    [UseDelegateFromConstructor] internal partial Unit Stamp(HostDocument document);
}

// Bounds DERIVE from the kernel dispatch lanes the gates cross on — the four gates that measured with no
// declared bound now measure against one owner and no millisecond literal exists here.
[SmartEnum<int>]
public sealed partial class GateLane : IGaugeLane<GateLane> {
    public static readonly GateLane Lifecycle = new(key: 0, lane: static () => DispatchLane.Interactive);
    public static readonly GateLane Mutate = new(key: 1, lane: static () => DispatchLane.Interactive);
    public static readonly GateLane Undo = new(key: 2, lane: static () => DispatchLane.Interactive);
    public static readonly GateLane Solve = new(key: 3, lane: static () => DispatchLane.Deferred);

    [UseDelegateFromConstructor] internal partial DispatchLane Lane();
    public TimeSpan Bound => Lane().Bound;
}

[SmartEnum<int>]
public sealed partial class DocumentFacet {
    public static readonly DocumentFacet Identity = new(key: 0,
        project: static document => new DocumentAnswer.IdentityCase(Value: document.Identity));
    public static readonly DocumentFacet Hash = new(key: 1,
        project: static document => new DocumentAnswer.HashCase(Value: document.Hash));
    public static readonly DocumentFacet Notes = new(key: 2,
        project: static document => new DocumentAnswer.NotesCase(Value: document.Notes));
    public static readonly DocumentFacet Condition = new(key: 3,
        project: static document => new DocumentAnswer.ConditionCase(
            State: document.State, Modifications: document.Modifications, Empty: document.IsEmpty));
    public static readonly DocumentFacet File = new(key: 4,
        project: static document => new DocumentAnswer.FileCase(Accessor: document.File));
    public static readonly DocumentFacet Display = new(key: 5,
        project: static document => new DocumentAnswer.DisplayCase(Chrome: document.Display));
    public static readonly DocumentFacet Dependencies = new(key: 6,
        project: static document => new DocumentAnswer.DependenciesCase(Graph: document.Dependencies));
    public static readonly DocumentFacet NamedViews = new(key: 7,
        project: static document => new DocumentAnswer.ViewsCase(Views: document.NamedViews));
    public static readonly DocumentFacet Parent = new(key: 8,
        project: static document => new DocumentAnswer.ParentCase(Host: Optional(document.Parent)));
    public static readonly DocumentFacet Globals = new(key: 9,
        project: static document => new DocumentAnswer.GlobalsCase(Server: document.Globals));
    public static readonly DocumentFacet Projection = new(key: 10,
        project: static document => new DocumentAnswer.ProjectionCase(
            Centre: document.Projection.centre, Zoom: document.Projection.zoom));
    [UseDelegateFromConstructor] internal partial DocumentAnswer Project(HostDocument document);
}

[Union]
public abstract partial record DocumentAnswer {
    private DocumentAnswer() { }
    public sealed record IdentityCase(Guid Value) : DocumentAnswer;
    public sealed record HashCase(Guid Value) : DocumentAnswer;
    public sealed record NotesCase(string Value) : DocumentAnswer;
    public sealed record ConditionCase(DocumentState State, int Modifications, bool Empty) : DocumentAnswer {
        public bool Modified => Modifications > 0;
    }
    public sealed record FileCase(FileUtility Accessor) : DocumentAnswer;
    public sealed record DisplayCase(DocumentDisplay Chrome) : DocumentAnswer;
    public sealed record DependenciesCase(DocumentDependencies Graph) : DocumentAnswer;
    public sealed record ViewsCase(NamedViews Views) : DocumentAnswer;
    public sealed record ParentCase(Option<IDocumentParent> Host) : DocumentAnswer;
    public sealed record GlobalsCase(GlobalServer Server) : DocumentAnswer;
    public sealed record ProjectionCase(PointF Centre, float Zoom) : DocumentAnswer;
}

// --- [MODELS] -------------------------------------------------------------------------------
// Three graph-verb payloads live BESIDE the spine that carries them; `Document/graph.md` and
// `Document/solution.md` compose them and their member shapes are the host verbs' own return shapes.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct WirelessPair(Guid Shout, Guid Listen) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Shout != Guid.Empty,
        Listen != Guid.Empty);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PinRepairRow(PinRepair Method, Guid Pin, Guid Cushion) : IValidityEvidence {
    public bool IsValid => Pin != Guid.Empty;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct RunPulse(
    SolutionId Id, SolutionPhase Phase, SolutionMode Mode,
    int Computable, int Invalid, int Progress, TimeSpan Age) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Computable, floor: 0),
        ValidityClaim.CountAtLeast(count: Invalid, floor: 0),
        ValidityClaim.CountAtLeast(count: Progress, floor: 0),
        ValidityClaim.Nonnegative(value: Age.TotalSeconds));
}

[Union]
public abstract partial record GateOutcome {
    private GateOutcome() { }
    public sealed record SettledCase : GateOutcome;
    public sealed record CountCase(int Touched) : GateOutcome;
    public sealed record ClearedCase(int Cleared) : GateOutcome;
    public sealed record ChangedCase(bool Changed) : GateOutcome;
    public sealed record MintedCase(Guid Instance) : GateOutcome;
    public sealed record RefusedCase(string WhyNot) : GateOutcome;
    public sealed record RemapCase(HashMap<Guid, Guid> Correspondence) : GateOutcome;
    public sealed record WirelessCase(WirelessPair Pair) : GateOutcome;
    public sealed record RepairCase(Seq<PinRepairRow> Rows) : GateOutcome;
    public sealed record RunCase(RunPulse Pulse) : GateOutcome;
}

// Kernel gauge IS the temporal evidence: identity, seal, outcome, span, and the causal deltas — a stored
// stamp pair or an outcome-nested observation wrapper has no seat.
// Generic over its FACTS case (E-G47): the document gates' facts are the causal delta window; the editor
// gate's facts are its settled shell posture — one receipt family, per-gate facts, zero receipt siblings.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct GateReceipt<TFacts>(
    Op Verb, Option<VerbNoun> Seal, GateOutcome Outcome,
    GaugedSpan<GateLane> Span, TFacts Facts) : IValidityEvidence {
    public TimeSpan Latency => Span.Elapsed;
    public bool Breached => Span.Breached;
    public bool IsValid => Span.IsValid;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
// SPINE the folder's five gates compose: resolve, marshal synchronously, gauge, receipt — one line per gate
// where five ~10-line preambles stood (the folder's own ruled collapse).
[BoundaryAdapter]
public static class DocumentGate {
    public static Fin<GateReceipt<Seq<UiEvent<GhFact>>>> Run(
        GateLane lane,
        MonotonicTimeline clock,
        Option<HostDocument> graph,
        Func<HostDocument, Fin<(Op Verb, Option<VerbNoun> Seal, GateOutcome Outcome, Seq<UiEvent<GhFact>> Deltas)>> body,
        Op key) =>
        from gauged in clock.Gauged<(Op, Option<VerbNoun>, GateOutcome, Seq<UiEvent<GhFact>>), GateLane>(
            lane: lane,
            work: key,
            body: () => Resolve(graph: graph, key: key, body: body),
            key: key)
        from settled in gauged.Value
        select new GateReceipt<Seq<UiEvent<GhFact>>>(
            Verb: settled.Item1, Seal: settled.Item2, Outcome: settled.Item3, Span: gauged.Span, Facts: settled.Item4);

    internal static Fin<TOut> Resolve<TOut>(Option<HostDocument> graph, Op key, Func<HostDocument, Fin<TOut>> body) =>
        graph.Match(
            Some: chosen => UiThread.Run(
                new UiDispatch<TOut>.Blocking(() => body(chosen)), DispatchLane.Interactive, key),
            None: () => GhSession.Run(
                target: ScopeTarget.DocumentHost,
                project: scope => scope.Document.ToFin(key.MissingContext()).Bind(body),
                key: key));
}

public static partial class DocumentScope {
    public static Fin<HostDocument> Mint(DocumentTier tier, Op? key = null);
    public static Fin<HostDocument> Load(IReader reader, Op? key = null);

    // Probe gates the accessor: a missing archive entry is None, a malformed one a typed fault.
    public static Fin<Option<T>> ReadProbe<T>(IReader reader, string name, Op? key = null) where T : IStorable;

    public static Fin<DocumentAnswer> Read(DocumentFacet facet, Option<HostDocument> graph = default, Op? key = null);

    // ABSENCE rides the option — the caller-supplied fallback fabricated a value for a key nothing wrote.
    public static Fin<Option<T>> Recall<T>(string name, Option<HostDocument> graph = default, Op? key = null);

    public static Fin<Seq<HostDocument>> Roster(Op? key = null);
}
```

## [03]-[TRANSACT]

- Owner: `SelectionTarget` `[Union]` — selection-scope discriminant, read by every scoped verb: `Selected` names the live selection and `Explicit(Seq<IDocumentObject>)` a computed set riding the host's `*Objects` twin. GAIN over the deleted `Seq`-emptiness convention: a computed EMPTY set no longer silently means "the whole selection" — it is an explicit set that touches nothing. `SelectionSweep` names the 8-row sweep family; `SelectionPosture` the 9-row posture family whose explicit twin is an `Option` column (the four pin-side reveals carry `None` and the gate refuses an explicit target on them typed); `DeleteDepth` the 2-row depth family; `IsolationAxis` `[SmartEnum<string>]` realizing `ICapability` — three axes the host names by what each KEEPS reachable, carried as `CapabilitySet<IsolationAxis>` and read per named position at the one host call (the `[Flags]` enum with its `HasFlag` reads deletes; that change also lands `Document/graph.md`'s two flag enums).
- Owner: `DocumentAct` `[Union]` `[GenerateUnionOps]` — THE one settlement union: the five lifecycle/shelf cases (`CloseCase`, `StoreCase`, `MarkCase`, `StashCase`, `ForgetCase` — the former `DocumentGate` union, merged: two gates over one receipt were arity twins) beside the seventeen graph cases (`SweepCase`, the clipboard three, the wrapper three, `DeleteCase(DeleteDepth, SelectionTarget, Seq<WireEnds>)`, `DropCase`, `SnippetCase`, `NudgeCase`, `PostureCase(SelectionPosture, SelectionTarget)`, `DressCase(PerceptualColor, SelectionTarget)`, `IsolateCase(IDocumentObject, CapabilitySet<IsolationAxis>)`, `MigrateCase`, `DependencyCase`, `RevealDependenciesCase`). Host discriminants (`ClipboardKind`, `PasteBehaviour`, `OpenColor.Family`) ride case payloads unchanged because this package IS the seam; the colour override crosses as the KERNEL `PerceptualColor` and projects to the host colour at its one write arm.
- Entry: `DocumentScope.Transact(Option<VerbNoun> label, DocumentAct op, MonotonicTimeline clock, Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail = default, Option<HostDocument> graph = default, Op? key = null)` → `Fin<GateReceipt<Seq<UiEvent<GhFact>>>>` — the ONE gate; a sealing case with an absent label refuses typed, a non-sealing case ignores it, and the case BAND selects the gauge lane (`GateLane.Lifecycle` for the shelf/lifecycle band, `GateLane.Mutate` for the graph band).
- Law: the `document.mutate` veto fires HERE — a mutating case fires `rail.Fire(at: GrasshopperPoint.DocumentMutate, fact: new HookSignal.IntentCase(Operation: op, DocumentId: Some(document.Identity)), key: key)` before its verb, and a `Fail` verdict refuses the transaction with nothing mutated; an absent rail dispatches ungoverned (the test-host arm). This is the fire-site producer the hook census names.
- Law: mutation and undo are one act — every mutating arm mints one `ActionList`, runs its host verb into it, and seals through `HistoryLedger.Seal` under the caller's `VerbNoun`, carried as `Seal: Some(label)`; non-mutating arms settle `Seal: None`. `DocumentMethods` call outside this gate is the deleted form.
- Law: the receipt reports what the HOST answered — each arm folds the host's own return into its `GateOutcome` case, so a settled receipt over an unmeasured act is unrepresentable; chain and cluster wraps preflight through the host's own `CanCreate*` verdict on the same roster the mint consumes, a refusal settling `RefusedCase(whyNot)` with no seal.
- Law: a `Data`-depth delete carrying a wire span refuses at admission — `DeleteObjectData` takes no wire span by host design.
- Law: the outcome is CAUSAL — the mutation window opens a window-local kernel `EvidenceDrain<GhFact>`, observes the document's own `GhSource` rows (`graph.object-added`, `graph.object-removed`, `graph.selection`, `document.modified`) under `Atomicity.AllOrNothing`, runs the verb, completes the drain, and folds the ordered `UiEvent<GhFact>` envelopes into the receipt's `Deltas` — a consumer reads what the mutation did as typed evidence, never by re-diffing the graph; attach, verb, seal, and fold share one marshal, so no concurrent delta interleaves.
- Boundary: wire mutation, id remapping, pins, and window selection are `Document/graph.md`'s operator; canvas room-making is `Canvas/layout.md`'s; repaint intent after a transaction is `Shell/session.md`'s `RepaintCase`, composed by the consumer.
- Packages: Grasshopper2 (`DocumentMethods`, `CanCreateChain`/`CanCreateCluster`, `ObjectList.SelectedObjects`, `ClipboardKind`, `PasteBehaviour`, `Snippet`, `WireEnds`, `OpenColor.Family`, `ActionList`), `Shell/events.md` (`GhSource`, `GhFact`), `Rasm.Interaction` (`UiEvents`, `EvidenceDrain`, `Atomicity`, `PaintColor`), `Shell/hooks.md` (`GrasshopperPoint`, `HookSignal`, `HookScope`), `Document/history.md` (`HistoryLedger.Seal`), `Rasm.Numerics` (`PerceptualColor`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new document verb is one `DocumentAct` case whose `Switch` arm breaks the gate loudly; a new sweep, posture, or depth verb is one row on its owning family; a new causal stream is one `GhSource` row added to the observation set.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Grasshopper2.Doc;
using Grasshopper2.Framework;
using Grasshopper2.Undo;
using Rasm.Domain;
using Rasm.Grasshopper.Shell;
using Rasm.Interaction;
using Rasm.Numerics;
using Rasm.Parametric;
using HostDocument = Grasshopper2.Doc.Document;

namespace Rasm.Grasshopper.Document;

// --- [TYPES] --------------------------------------------------------------------------------
// Scope discriminant every scoped verb reads: a computed EMPTY set is an explicit set touching nothing,
// never a silent whole-selection sweep — the gain the Seq-emptiness convention could not spell.
[Union]
public abstract partial record SelectionTarget {
    private SelectionTarget() { }
    public sealed record Selected : SelectionTarget;
    public sealed record Explicit(Seq<IDocumentObject> Objects) : SelectionTarget;
}

[SmartEnum<int>]
public sealed partial class SelectionSweep {
    public static readonly SelectionSweep All = new(key: 0, sweep: static verbs => verbs.SelectAll());
    public static readonly SelectionSweep None = new(key: 1, sweep: static verbs => verbs.DeselectAll());
    public static readonly SelectionSweep Invert = new(key: 2, sweep: static verbs => verbs.InvertSelection());
    public static readonly SelectionSweep ShiftUp = new(key: 3, sweep: static verbs => verbs.ShiftSelection(upstream: true));
    public static readonly SelectionSweep ShiftDown = new(key: 4, sweep: static verbs => verbs.ShiftSelection(upstream: false));
    public static readonly SelectionSweep GrowUp = new(key: 5, sweep: static verbs => verbs.GrowSelection(upstream: true, downstream: false));
    public static readonly SelectionSweep GrowDown = new(key: 6, sweep: static verbs => verbs.GrowSelection(upstream: false, downstream: true));
    public static readonly SelectionSweep GrowBoth = new(key: 7, sweep: static verbs => verbs.GrowSelection(upstream: true, downstream: true));
    [UseDelegateFromConstructor] internal partial int Sweep(DocumentMethods verbs);
}

// Six posture verbs publish explicit-set twins; the four pin-side reveals do not — the twin is an OPTION column
// and the gate refuses an explicit target on an absent one with a typed refusal naming the axis.
[SmartEnum<int>]
public sealed partial class SelectionPosture {
    public static readonly SelectionPosture Enabled = new(key: 0,
        apply: static (verbs, actions) => verbs.EnableSelected(actions),
        explicitArm: Some<Func<DocumentMethods, IDocumentObject[], ActionList, int>>(
            static (verbs, objects, actions) => verbs.EnableObjects(objects, actions)));
    public static readonly SelectionPosture Disabled = new(key: 1,
        apply: static (verbs, actions) => verbs.DisableSelected(actions),
        explicitArm: Some<Func<DocumentMethods, IDocumentObject[], ActionList, int>>(
            static (verbs, objects, actions) => verbs.DisableObjects(objects, actions)));
    public static readonly SelectionPosture Shown = new(key: 2,
        apply: static (verbs, actions) => verbs.ShowSelected(actions),
        explicitArm: Some<Func<DocumentMethods, IDocumentObject[], ActionList, int>>(
            static (verbs, objects, actions) => verbs.ShowObjects(objects, actions)));
    public static readonly SelectionPosture Hidden = new(key: 3,
        apply: static (verbs, actions) => verbs.HideSelected(actions),
        explicitArm: Some<Func<DocumentMethods, IDocumentObject[], ActionList, int>>(
            static (verbs, objects, actions) => verbs.HideObjects(objects, actions)));
    public static readonly SelectionPosture Toggled = new(key: 4,
        apply: static (verbs, actions) => verbs.ToggleDisplaySelected(actions),
        explicitArm: Some<Func<DocumentMethods, IDocumentObject[], ActionList, int>>(
            static (verbs, objects, actions) => verbs.ToggleDisplayObjects(objects, actions)));
    public static readonly SelectionPosture InputsShown = new(key: 5,
        apply: static (verbs, actions) => verbs.ShowSelectedInputs(actions), explicitArm: None);
    public static readonly SelectionPosture InputsHidden = new(key: 6,
        apply: static (verbs, actions) => verbs.HideSelectedInputs(actions), explicitArm: None);
    public static readonly SelectionPosture OutputsShown = new(key: 7,
        apply: static (verbs, actions) => verbs.ShowSelectedOutputs(actions), explicitArm: None);
    public static readonly SelectionPosture OutputsHidden = new(key: 8,
        apply: static (verbs, actions) => verbs.HideSelectedOutputs(actions), explicitArm: None);

    internal Option<Func<DocumentMethods, IDocumentObject[], ActionList, int>> ExplicitArm { get; }
    [UseDelegateFromConstructor] internal partial int Apply(DocumentMethods verbs, ActionList actions);
}

[SmartEnum<int>]
public sealed partial class DeleteDepth {
    public static readonly DeleteDepth Graph = new(key: 0,
        selected: static (verbs, actions) => verbs.DeleteSelection(actions),
        explicitArm: static (verbs, objects, wires, actions) => verbs.DeleteObjects(objects, wires, actions),
        outcome: static touched => new GateOutcome.CountCase(Touched: touched));
    public static readonly DeleteDepth Data = new(key: 1,
        selected: static (verbs, actions) => verbs.DeleteSelectionData(actions),
        explicitArm: static (verbs, objects, _, actions) => verbs.DeleteObjectData(objects, actions),
        outcome: static cleared => new GateOutcome.ClearedCase(Cleared: cleared));
    [UseDelegateFromConstructor] internal partial int Selected(DocumentMethods verbs, ActionList actions);
    [UseDelegateFromConstructor] internal partial int ExplicitArm(DocumentMethods verbs, IDocumentObject[] objects, WireEnds[] wires, ActionList actions);
    [UseDelegateFromConstructor] internal partial GateOutcome Outcome(int touched);
}

// Host names the axes by what each KEEPS reachable and takes them positionally; the set carries membership
// and the one host call reads `Admits` per named position — `HasFlag` ladders and a `[Flags]` enum delete.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IsolationAxis : ICapability<IsolationAxis> {
    public static readonly IsolationAxis Pins = new(key: "pins");
    public static readonly IsolationAxis Inputs = new(key: "inputs");
    public static readonly IsolationAxis Outputs = new(key: "outputs");
    public static CapabilityLaw<IsolationAxis> Law => CapabilityLaw<IsolationAxis>.Open;
}

// ONE settlement union: the five lifecycle/shelf cases beside the seventeen graph cases — the case decides
// whether the act seals, observes, or neither, and the former two-gate split was an arity twin.
[Union]
[GenerateUnionOps]
public abstract partial record DocumentAct {
    private DocumentAct() { }
    // --- lifecycle and shelf (no seal, no observation — the host mints no undo for these by design)
    public sealed record CloseCase : DocumentAct;
    public sealed record StoreCase(IWriter Writer, FileContents Contents) : DocumentAct;
    public sealed record MarkCase(MarkPosture Posture) : DocumentAct;
    public sealed record StashCase(string Name, IStorable Value) : DocumentAct;
    public sealed record ForgetCase(string Name) : DocumentAct;
    // --- graph verbs (sealed and observed)
    public sealed record SweepCase(SelectionSweep Sweep) : DocumentAct;
    public sealed record CopyCase(ClipboardKind Kind) : DocumentAct;
    public sealed record CutCase(ClipboardKind Kind) : DocumentAct;
    public sealed record PasteCase(ClipboardKind Kind, PasteBehaviour Behaviour) : DocumentAct;
    public sealed record GroupCase(Option<string> Name, Option<OpenColor.Family> Colour, SelectionTarget Target) : DocumentAct;
    public sealed record ChainCase(SelectionTarget Target) : DocumentAct;
    public sealed record ClusterCase(SelectionTarget Target) : DocumentAct;
    public sealed record DeleteCase(DeleteDepth Depth, SelectionTarget Target, Seq<WireEnds> Wires) : DocumentAct;
    public sealed record DropCase(IDocumentObject Subject, PointF At) : DocumentAct;
    public sealed record SnippetCase(Snippet Payload, PointF At) : DocumentAct;
    public sealed record NudgeCase(int X, int Y) : DocumentAct;
    public sealed record PostureCase(SelectionPosture Posture, SelectionTarget Target) : DocumentAct;
    // Override crosses as the KERNEL colour and projects to the host colour at its one write arm.
    public sealed record DressCase(PerceptualColor Override, SelectionTarget Target) : DocumentAct;
    public sealed record IsolateCase(IDocumentObject Subject, CapabilitySet<IsolationAxis> Reach) : DocumentAct;
    public sealed record MigrateCase(Seq<IDocumentObject> Objects, PointF At) : DocumentAct;
    public sealed record DependencyCase(PointF At) : DocumentAct;
    public sealed record RevealDependenciesCase : DocumentAct;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static partial class DocumentScope {
    // ONE gate over the whole union: the veto fires before a mutating verb, the causal window brackets it, the
    // seal law reads the case, and the spine gauges the crossing — a sealing case with no label refuses typed.
    public static Fin<GateReceipt<Seq<UiEvent<GhFact>>>> Transact(
        Option<VerbNoun> label,
        DocumentAct op,
        MonotonicTimeline clock,
        Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail = default,
        Option<HostDocument> graph = default,
        Op? key = null);

    // Causal window: a window-local kernel drain, the four document GhSource rows, AllOrNothing seating,
    // verb, complete, fold — deltas are ordered kernel envelopes on the receipt. Internal: `Document/graph.md`'s
    // mutation gate brackets its verbs with the SAME window rather than minting a sibling.
    internal static Fin<(Op Verb, Option<VerbNoun> Seal, GateOutcome Outcome, Seq<UiEvent<GhFact>> Deltas)> Observed(
        HostDocument document, MonotonicTimeline clock, Func<Fin<(Op, Option<VerbNoun>, GateOutcome)>> verb, Op key);

    // Preflight-gated wrap: the host's own feasibility verdict runs on the same roster the mint consumes; a
    // refusal settles RefusedCase(whyNot) with NO seal — nothing mutated.
    private static Fin<(Op Verb, Option<VerbNoun> Seal, GateOutcome Outcome)> Wrap(
        HostDocument document, Op caseOp, SelectionTarget target,
        Func<DocumentMethods, IDocumentObject[], (bool Can, string WhyNot)> preflight,
        Func<DocumentMethods, IDocumentObject[], ActionList, Guid> mint,
        VerbNoun label, Op key);
}
```

## [04]-[DENSITY_BAR]

| [INDEX] | [CONCERN]           | [OWNER]                            | [RAIL]                                                     | [CASES] |
| :-----: | :------------------ | :--------------------------------- | :--------------------------------------------------------- | :-----: |
|  [01]   | document minting    | `DocumentTier`                     | `Mint → Fin<HostDocument>`                                 |    3    |
|  [02]   | inert facet read    | `DocumentFacet` + `DocumentAnswer` | `Read → Fin<DocumentAnswer>`                               | 11 + 11 |
|  [03]   | gate spine          | `DocumentGate.Run` + `GateLane`    | one gauged preamble for five gates                         |    5    |
|  [04]   | settlement evidence | `GateReceipt` + `GateOutcome`      | kernel span + deltas, payloads co-homed                    |   10    |
|  [05]   | selection scope     | `SelectionTarget`                  | Selected / Explicit — empty set honest                     |    2    |
|  [06]   | verb families       | `SelectionSweep`/`Posture`/`Depth` | delegate rows, Option explicit twins                       |  8+9+2  |
|  [07]   | settlement union    | `DocumentAct`                      | `Transact → Fin<GateReceipt<Seq<UiEvent<GhFact>>>>` + veto |   22    |

`Apply`/`Transact` twin gates, the `ObservedCase` nesting, the stored stamp pair, the fallback-fabricating `Recall`, the `[Flags]` enum, the `Seq`-emptiness selection convention, the nullable explicit arm, and the per-call clock mint are all deleted; the graph-verb payload records moved home beside their spine (E-G45).

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
