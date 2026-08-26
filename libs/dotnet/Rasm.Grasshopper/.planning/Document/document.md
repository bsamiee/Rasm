# [RASM_GRASSHOPPER_DOCUMENT_DOCUMENT]

`DocumentScope` is the document spine of the GH2 graph boundary — ONE scope operator owning document minting across the inert/inactive/active tiers, the closed inert-facet read, and ONE command gate absorbing the whole `DocumentMethods` verb surface AND the lifecycle/persistence/shelf commands: the former `Apply`/`Transact` arity twins are one `Transact` over one `DocumentAct` union. This page also declares `GateOutcome` with its payload records and the `DocumentGate.Run` marshal spine every `Document/*` command composes.

Every gate crosses the marshal through the kernel's synchronous `UiThread.Run` arity and returns the host's `GateOutcome`. Mutation fires the `document.mutate` veto row on the hook dispatch before committing, which is that row's fire-site producer.

## [01]-[INDEX]

- [02]-[LIFECYCLE]: `DocumentTier` + `MarkPosture` + `DocumentFacet` + `DocumentAnswer` + `GateOutcome` + `DocumentGate` — mint and archive-load rows, the closed facet read, the result family, and the shared marshal spine.
- [03]-[TRANSACT]: `SelectionTarget` + `SelectionSweep` + `SelectionPosture` + `DeleteDepth` + `IsolationAxis` + `DocumentAct` + `DocumentScope` — the one command union over the whole verb surface with its selection-target discriminant, wrap preflight, and seal law.

## [02]-[LIFECYCLE]

- Owner: `DocumentTier` `[SmartEnum<int>]` — 3 mint rows over one `Mint()` column (`NewInertDocument`/`NewInactiveDocument`/`NewActiveDocument`); tier is data, so headless pipelines, background parsing, and canvas-bound editing mint through one gate. `MarkPosture` `[SmartEnum<int>]` — the dirty flag as two rows over one `Stamp` column, so the polarity is a named row, never a boolean payload. `DocumentFacet` `[SmartEnum<int>]` — the closed inert-read vocabulary over one `Project(HostDocument) -> DocumentAnswer` column; `DocumentAnswer` `[Union]` the closed result, one case per evidence shape. Mapperly is REFUTED for this page's projections — the facets hand out host accessor OBJECTS, not field correspondences.
- Owner: `GateOutcome` `[Union]` closes what a host verb hands back — `SettledCase`, `CountCase`, `ClearedCase` (typed apart so cleared never reads as removed), `ChangedCase`, `MintedCase`, `RefusedCase` (the host's own why-not from a preflighted wrap), `RemapCase`, `WirelessCase(WirelessPair)`, `RepairCase(Seq<PinRepairRow>)`, `RunCase(RunPulse)`. Three payload records live HERE beside the spine that carries them (`Document/graph.md` composes them — a spine importing its payload page was the inverted layering).
- Owner: `DocumentGate` — the shared gate spine: resolve the target document (supplied or session-acquired), cross the marshal synchronously, run the body, and return its `GateOutcome`. Every `Document/*` command composes it and none re-spells the preamble.
- Entry: `DocumentScope.Mint(tier, key)` → `Fin<HostDocument>`; `Load(IReader, key)` — the archive mint; `ReadProbe<T>(reader, name, key)` → `Fin<Option<T>>` — probe-gates-accessor (`HasItemOrNode` before `Storable<T>`); `Read(facet, graph, key)` → `Fin<DocumentAnswer>`; `Recall<T>(name, graph, key)` → `Fin<Option<T>>` — ABSENCE rides the option; the caller-supplied fallback is deleted, because a fabricated default past the boundary spelled absence as a value; `Roster(key)` — the live `AllDocuments` sweep.
- Law: absence of a target document is a modality, never an overload — `Option<HostDocument>` discriminates the supplied graph from the session-active document, and the absent branch resolves through `GhSession.Run(ScopeTarget.DocumentHost, …)`.
- Law: every gate settles inside one marshal window through the kernel SYNC arity — `UiThread.Run(new UiDispatch<T>.Blocking(body), DispatchLane.Interactive, key)` composes inside the gate's own `Fin` query; the live `HostDocument` never crosses back out, because `Read` takes a facet ROW and returns the row's own answer case — a caller lambda over the live document voids that law from the signature down.
- Law: `Read` owns the INERT facets alone — `Objects` is `Document/graph.md`'s, `Undo` is `Document/history.md`'s, `Solution` is `Document/solution.md`'s, `Methods` is `[03]`'s gate.
- Law: keyed state is one shelf — `Document.CustomValues` is the host's only `KeyedValues` facet; a shelf selector over a one-inhabitant vocabulary carries no information.
- Law: the archive is symmetric on ONE axis — `Store(IWriter[, FileContents])` writes and the `Document(IReader)` constructor reads, so the load posture is a MINT beside `DocumentTier`, never a gate case.
- Boundary: autosave requests are per-object and ride `Document/history.md`; file-compare and editor reveal are `Shell/editor.md`'s; the retired session-cache eviction claim is gone with the cache module.
- Packages: Grasshopper2 (the document surface), GrasshopperIO (`IWriter`, `IReader`, `IStorable`), `Rasm.Interaction` (`UiThread`, `UiDispatch<T>`, `DispatchLane`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new mint posture is one `DocumentTier` row; a new inert read is one facet row with its answer case; a new host answer shape is one `GateOutcome` case.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Grasshopper2.Doc;
using Grasshopper2.Parameters.Special;
using Grasshopper2.Undo;
using GrasshopperIO;
using Rasm.Domain;
using Rasm.Grasshopper.Shell;
using Rasm.Interaction;
using HostDocument = Grasshopper2.Doc.Document;

namespace Rasm.Grasshopper.Document;

// --- [TYPES] ---------------------------------------------------------------------------
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

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct WirelessPair(Guid Shout, Guid Listen) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Shout != Guid.Empty,
        Listen != Guid.Empty);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PinRepairRow(PinRepair Method, Guid Pin, Guid Cushion) : IValidityEvidence {
    public bool IsValid => Pin != Guid.Empty;
}

[StructLayout(LayoutKind.Auto)]
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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DocumentGate {
    public static Fin<GateOutcome> Run(
        Option<HostDocument> graph,
        Func<HostDocument, Fin<GateOutcome>> body,
        Op key) => Resolve(graph: graph, key: key, body: body);

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

    public static Fin<Option<T>> ReadProbe<T>(IReader reader, string name, Op? key = null) where T : IStorable;

    public static Fin<DocumentAnswer> Read(DocumentFacet facet, Option<HostDocument> graph = default, Op? key = null);

    public static Fin<Option<T>> Recall<T>(string name, Option<HostDocument> graph = default, Op? key = null);

    public static Fin<Seq<HostDocument>> Roster(Op? key = null);
}
```

## [03]-[TRANSACT]

- Owner: `SelectionTarget` `[Union]` — selection-scope discriminant, read by every scoped verb: `Selected` names the live selection and `Explicit(Seq<IDocumentObject>)` a computed set riding the host's `*Objects` twin. GAIN over the deleted `Seq`-emptiness convention: a computed EMPTY set no longer silently means "the whole selection" — it is an explicit set that touches nothing. `SelectionSweep` names the 8-row sweep family; `SelectionPosture` the 9-row posture family whose explicit twin is an `Option` column (the four pin-side reveals carry `None` and the gate refuses an explicit target on them typed); `DeleteDepth` the 2-row depth family; `IsolationAxis` `[SmartEnum<string>]` realizing `ICapability` — three axes the host names by what each KEEPS reachable, carried as `CapabilitySet<IsolationAxis>` and read per named position at the one host call (the `[Flags]` enum with its `HasFlag` reads deletes; that change also lands `Document/graph.md`'s two flag enums).
- Owner: `DocumentAct` `[Union]` `[GenerateUnionOps]` — THE one command union: the five lifecycle/shelf cases (`CloseCase`, `StoreCase`, `MarkCase`, `StashCase`, `ForgetCase` — the former `DocumentGate` union, merged: two gates over one result were arity twins) beside the seventeen graph cases (`SweepCase`, the clipboard three, the wrapper three, `DeleteCase(DeleteDepth, SelectionTarget, Seq<WireEnds>)`, `DropCase`, `SnippetCase`, `NudgeCase`, `PostureCase(SelectionPosture, SelectionTarget)`, `DressCase(PerceptualColor, SelectionTarget)`, `IsolateCase(IDocumentObject, CapabilitySet<IsolationAxis>)`, `MigrateCase`, `DependencyCase`, `RevealDependenciesCase`). Host discriminants (`ClipboardKind`, `PasteBehaviour`, `OpenColor.Family`) ride case payloads unchanged because this package IS the boundary; the colour override crosses as the KERNEL `PerceptualColor` and projects to the host colour at its one write arm.
- Entry: `DocumentScope.Transact(Option<VerbNoun> label, DocumentAct op, Option<HookSet<GrasshopperPoint, HookSignal, HookScope>> hooks = default, Option<HostDocument> graph = default, Op? key = null)` → `Fin<GateOutcome>` — the ONE gate; a sealing case with an absent label refuses typed, and a non-sealing case ignores it.
- Law: the `document.mutate` veto fires HERE — a mutating case fires `hooks.Fire(at: GrasshopperPoint.DocumentMutate, fact: new HookSignal.IntentCase(Operation: op, DocumentId: Some(document.Identity)), key: key)` before its verb, and a `Fail` verdict refuses the transaction with nothing mutated; absent hooks dispatch ungoverned (the test-host arm). This is the fire-site producer the hook census names.
- Law: mutation and undo are one act — every mutating arm mints one `ActionList`, runs its host verb into it, and seals through `HistoryLedger.Seal` under the caller's `VerbNoun`. `DocumentMethods` call outside this gate is the deleted form.
- Law: each arm returns what the HOST answered through its `GateOutcome` case; chain and cluster wraps preflight through the host's own `CanCreate*` verdict on the same roster the mint consumes, with refusal returning `RefusedCase(whyNot)`.
- Law: a `Data`-depth delete carrying a wire span refuses at admission — `DeleteObjectData` takes no wire span by host design.
- Boundary: wire mutation, id remapping, pins, and window selection are `Document/graph.md`'s operator; canvas room-making is `Canvas/layout.md`'s; repaint intent after a transaction is `Shell/session.md`'s `RepaintCase`, composed by the consumer.
- Packages: Grasshopper2 (`DocumentMethods`, `CanCreateChain`/`CanCreateCluster`, `ObjectList.SelectedObjects`, `ClipboardKind`, `PasteBehaviour`, `Snippet`, `WireEnds`, `OpenColor.Family`, `ActionList`), `Rasm.Interaction` (`PaintColor`), `Shell/hooks.md` (`GrasshopperPoint`, `HookSignal`, `HookScope`), `Document/history.md` (`HistoryLedger.Seal`), `Rasm.Numerics` (`PerceptualColor`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new document verb is one `DocumentAct` case whose `Switch` arm breaks the gate loudly; a new sweep, posture, or depth verb is one row on its owning family; lifecycle observation grows only through `Shell/events.md`'s `GhSource` rows.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Grasshopper2.Doc;
using Grasshopper2.Framework;
using Grasshopper2.Undo;
using Rasm.Domain;
using Rasm.Grasshopper.Shell;
using Rasm.Interaction;
using Rasm.Numerics;
using HostDocument = Grasshopper2.Doc.Document;

namespace Rasm.Grasshopper.Document;

// --- [TYPES] ---------------------------------------------------------------------------
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IsolationAxis : ICapability<IsolationAxis> {
    public static readonly IsolationAxis Pins = new(key: "pins");
    public static readonly IsolationAxis Inputs = new(key: "inputs");
    public static readonly IsolationAxis Outputs = new(key: "outputs");
    public static CapabilityLaw<IsolationAxis> Law => CapabilityLaw<IsolationAxis>.Open;
}

[Union]
[GenerateUnionOps]
public abstract partial record DocumentAct {
    private DocumentAct() { }
    public sealed record CloseCase : DocumentAct;
    public sealed record StoreCase(IWriter Writer, FileContents Contents) : DocumentAct;
    public sealed record MarkCase(MarkPosture Posture) : DocumentAct;
    public sealed record StashCase(string Name, IStorable Value) : DocumentAct;
    public sealed record ForgetCase(string Name) : DocumentAct;
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
    public sealed record DressCase(PerceptualColor Override, SelectionTarget Target) : DocumentAct;
    public sealed record IsolateCase(IDocumentObject Subject, CapabilitySet<IsolationAxis> Reach) : DocumentAct;
    public sealed record MigrateCase(Seq<IDocumentObject> Objects, PointF At) : DocumentAct;
    public sealed record DependencyCase(PointF At) : DocumentAct;
    public sealed record RevealDependenciesCase : DocumentAct;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class DocumentScope {
    public static Fin<GateOutcome> Transact(
        Option<VerbNoun> label,
        DocumentAct op,
        Option<HookSet<GrasshopperPoint, HookSignal, HookScope>> hooks = default,
        Option<HostDocument> graph = default,
        Op? key = null);

    private static Fin<GateOutcome> Wrap(
        HostDocument document, Op caseOp, SelectionTarget target,
        Func<DocumentMethods, IDocumentObject[], (bool Can, string WhyNot)> preflight,
        Func<DocumentMethods, IDocumentObject[], ActionList, Guid> mint,
        VerbNoun label, Op key);
}
```

## [04]-[DENSITY_BAR]

| [INDEX] | [CONCERN]        | [OWNER]                            | [RESULT]                                   | [CASES] |
| :-----: | :--------------- | :--------------------------------- | :----------------------------------------- | :-----: |
|  [01]   | document minting | `DocumentTier`                     | `Mint → Fin<HostDocument>`                 |    3    |
|  [02]   | inert facet read | `DocumentFacet` + `DocumentAnswer` | `Read → Fin<DocumentAnswer>`               | 11 + 11 |
|  [03]   | gate spine       | `DocumentGate.Run`                 | one marshal preamble for document commands |    1    |
|  [04]   | command result   | `GateOutcome`                      | host return shapes                         |   10    |
|  [05]   | selection scope  | `SelectionTarget`                  | Selected / Explicit — empty set honest     |    2    |
|  [06]   | verb families    | `SelectionSweep`/`Posture`/`Depth` | delegate rows, Option explicit twins       |  8+9+2  |
|  [07]   | command union    | `DocumentAct`                      | `Transact → Fin<GateOutcome>` + veto       |   22    |

`Apply`/`Transact` twin gates, the `ObservedCase` nesting, the stored stamp pair, the fallback-fabricating `Recall`, the `[Flags]` enum, the `Seq`-emptiness selection convention, the nullable explicit arm, and the per-call clock mint are all deleted; the graph-verb payload records moved home beside their spine (E-G45).

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
