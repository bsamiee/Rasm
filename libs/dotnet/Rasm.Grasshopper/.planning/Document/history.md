# [RASM_GRASSHOPPER_DOCUMENT_HISTORY]

`HistoryLedger` is the undo ledger of the GH2 document boundary — ONE ledger owner over the host's branching `History` tree: sealing a filled `ActionList` into the tree under one `VerbNoun`, striding the tree back and forward, re-rooting a branch, replaying a banked `Record` against a document, the object-scoped undo verbs, the branch-reconciliation read over the public `Node` topology, and the node-topology projection over `PrimaryChild`/`SecondaryChildren`.

Every undo verb is a case of one `HistoryOp` union handled by one `Commit` gate on `Document/document.md`'s shared `DocumentGate.Run` spine. Direction is a two-row `LedgerStride` family whose rows carry BOTH the tree stride and the record replay as delegate columns, and `Seal` is the one cross-page spelling `Document/document.md`'s `Transact` and `Document/graph.md`'s `Mutate` compose so mutation and undo are structurally one act everywhere in the folder. GH2's undo is a branching `Node` tree, never a linear stack — `PromoteChild` re-roots, reconciliation walks the tree — and this page is the only surface in the folder that touches it.

## [01]-[INDEX]

- [02]-[LEDGER]: `LedgerStride` + `ObjectUndoVerb` + `HistoryOp` + `HistoryLedger` — the command union, the stride/replay row family, the one `Commit` gate, and the cross-page `Seal`/`Bank` seams.
- [03]-[BRANCHES]: `BranchPath` + `BranchCrown` — branch reconciliation and node-topology projection over the undo tree.

## [02]-[LEDGER]

- Owner: `LedgerStride` `[SmartEnum<int>]` — the direction vocabulary with two delegate columns: `Back` (key 0, `History.Undo` / `Record.Undo(Document)`), `Forward` (key 1, `History.Redo` / `Record.Redo(Document)`) — one row family serves both the live tree stride and the banked-record replay, so direction is data on both surfaces. `ObjectUndoVerb` `[Union]` — the object-scoped verb pair: `AttachCase(VerbNoun, UndoAction)` (`IDocumentObject.AddUndoRecord`) and `AutoSaveCase(AutoSaveReason)` (`IDocumentObject.RequestAutoSave`); both ride ONE `HistoryOp.SubjectCase(IDocumentObject, ObjectUndoVerb)` because the subject shape is shared and the verb is the discriminant — two sibling top-level cases re-spelled the same custody. `HistoryOp` `[Union]` `[GenerateUnionOps]` closes the command family: `SealCase(VerbNoun, ActionList)` (`History.Do` — seal a filled action buffer into the tree), `StrideCase(LedgerStride)` (step the tree), `BranchCase(Node, Node)` (`Node.PromoteChild` — promote a secondary child to the primary line), `ReplayCase(LedgerStride, Record)` (replay a banked record against the document), `SubjectCase(IDocumentObject, ObjectUndoVerb)`.
- Entry: `HistoryLedger.Commit(HistoryOp op, Option<HostDocument> graph = default, Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail = default, Op? key = null)` → `Fin<GateOutcome>` — the command gate; `HistoryLedger.Seal(History ledger, ActionList actions, VerbNoun label, Op key)` → `Fin<Unit>` — the one-expression seal the folder's mutation gates compose inside their own marshal windows; `HistoryLedger.Bank(ActionList actions, VerbNoun label, Op? key = null)` → `Fin<Record>` — `ActionList.ToRecord`, minting a replayable record without touching the tree.
- Law: `Seal` is the folder's only `History.Do` spelling — `Document/document.md`'s `Transact` arms and `Document/graph.md`'s `Mutate` arms call it with the `ActionList` their host verb filled, so no mutation in the folder exists without its undo record and no second seal path exists to diverge; `Seal` runs on the caller's marshal and never opens its own.
- Law: `StrideCase` and `ReplayCase` are the `history.replay` fire site — both re-run sealed actions against the live document, so each fires `GrasshopperPoint.HistoryReplay` (`Replay` modality) on the injected rail with the case's own op as the intent signal before the host verb runs; an absent rail replays unobserved, and `SealCase`/`BranchCase`/`SubjectCase` fire nothing because nothing re-runs.
- Law: a `Record` is banked evidence, not tree state — `Bank` seals an action buffer into a detached `Record` whose `ReplayCase` replays it against any document; the tree stride and the record replay share the `LedgerStride` rows, so a new direction semantics is impossible to add to one surface and forget on the other.
- Law: object-scoped undo rides the object — `SubjectCase` binds its `ObjectUndoVerb` to one `IDocumentObject`; neither verb touches the document tree, both settle through the same gate.
- Law: `VerbNoun` mints as `new VerbNoun(verb, noun)` and every gate accepts the already-minted label; `ActionList`'s `params Action[]` constructor admits the empty call the folder's mutation gates make; `History.Undo`/`Redo` return `void` in both their bare and `Node`-targeted arities, so `SettledCase` IS the host's whole answer for a stride, never a dropped return.
- Boundary: undo lifecycle observation — the `Undone`/`NodeMoved` streams — is `Shell/events.md`'s subject-closed `Of(History)` source rows; a ledger consumer needing settled-undo notification composes those rows, and this page publishes nothing of its own.
- Packages: Grasshopper2 (`History.Do`/`Undo`/`Redo`, `ActionList.ToRecord`, `Record.Undo`/`Redo`, `Node.PromoteChild`, `VerbNoun`, `IDocumentObject.AddUndoRecord`/`RequestAutoSave`, `AutoSaveReason`), `Shell/hooks.md` (`GrasshopperPoint`, `HookSignal`, `HookScope`), `Document/document.md` (`DocumentGate`, `GateOutcome`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new undo verb is one `HistoryOp` case breaking the gate's total `Switch` loudly; a new object-scoped verb is one `ObjectUndoVerb` case; a new direction semantics is one `LedgerStride` row carrying both columns — zero new entrypoints.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Grasshopper2.Doc;
using Grasshopper2.Undo;
using Rasm.Domain;
using Rasm.Grasshopper.Shell;
using HostDocument = Grasshopper2.Doc.Document;
using UndoAction = Grasshopper2.Undo.Action;

namespace Rasm.Grasshopper.Document;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class LedgerStride {
    public static readonly LedgerStride Back = new(
        key: 0,
        stride: static ledger => Op.Side(action: () => ledger.Undo()),
        replay: static (record, document) => Op.Side(action: () => record.Undo(document)));
    public static readonly LedgerStride Forward = new(
        key: 1,
        stride: static ledger => Op.Side(action: () => ledger.Redo()),
        replay: static (record, document) => Op.Side(action: () => record.Redo(document)));
    [UseDelegateFromConstructor] internal partial Unit Stride(History ledger);
    [UseDelegateFromConstructor] internal partial Unit Replay(Record record, HostDocument document);
}

[Union]
public abstract partial record ObjectUndoVerb {
    private ObjectUndoVerb() { }
    public sealed record AttachCase(VerbNoun Label, UndoAction Action) : ObjectUndoVerb;
    public sealed record AutoSaveCase(AutoSaveReason Reason) : ObjectUndoVerb;
}

[Union]
[GenerateUnionOps]
public abstract partial record HistoryOp {
    private HistoryOp() { }
    public sealed record SealCase(VerbNoun Label, ActionList Actions) : HistoryOp;
    public sealed record StrideCase(LedgerStride Stride) : HistoryOp;
    public sealed record BranchCase(Node Parent, Node Child) : HistoryOp;
    public sealed record ReplayCase(LedgerStride Stride, Record Record) : HistoryOp;
    public sealed record SubjectCase(IDocumentObject Subject, ObjectUndoVerb Verb) : HistoryOp;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[BoundaryAdapter]
public static partial class HistoryLedger {
    public static Fin<Unit> Seal(History ledger, ActionList actions, VerbNoun label, Op key) =>
        from live in Optional(ledger).ToFin(key.MissingContext())
        from filled in Optional(actions).ToFin(key.InvalidInput())
        from _ in key.Catch(body: () => Fin.Succ(Op.Side(action: () => live.Do(label, filled))))
        select unit;

    public static Fin<Record> Bank(ActionList actions, VerbNoun label, Op? key = null) {
        Op active = key.OrDefault();
        return Optional(actions).ToFin(active.InvalidInput())
            .Bind(filled => active.Catch(body: () => Fin.Succ(filled.ToRecord(label))));
    }

    public static Fin<GateOutcome> Commit(
        HistoryOp op,
        Option<HostDocument> graph = default,
        Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail = default,
        Op? key = null) {
        Op active = key.OrDefault();
        return Optional(op).ToFin(active.InvalidInput())
            .Bind(valid => DocumentGate.Run(
                graph: graph, key: active,
                body: document => valid.Switch(
                        state: (Key: active, Graph: document, Rail: rail),
                        sealCase: static (frame, c) =>
                            Seal(ledger: frame.Graph.Undo, actions: c.Actions, label: c.Label, key: frame.Key)
                                .Map(_ => (GateOutcome)new GateOutcome.SettledCase()),
                        strideCase: static (frame, c) =>
                            Replayed(rail: frame.Rail, op: c.SelfOp, document: frame.Graph, key: frame.Key)
                                .Bind(_ => Free(frame.Key, () => c.Stride.Stride(ledger: frame.Graph.Undo))),
                        branchCase: static (frame, c) => Free(frame.Key,
                            () => Op.Side(action: () => c.Parent.PromoteChild(c.Child))),
                        replayCase: static (frame, c) =>
                            Replayed(rail: frame.Rail, op: c.SelfOp, document: frame.Graph, key: frame.Key)
                                .Bind(_ => Free(frame.Key, () => c.Stride.Replay(record: c.Record, document: frame.Graph))),
                        subjectCase: static (frame, c) => Free(frame.Key, () => c.Verb.Switch(
                            state: c.Subject,
                            attachCase: static (subject, v) => Op.Side(action: () => subject.AddUndoRecord(v.Label, v.Action)),
                            autoSaveCase: static (subject, v) => Op.Side(action: () => subject.RequestAutoSave(v.Reason))))));
    }

    private static Fin<Unit> Replayed(
        Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail, Op op, HostDocument document, Op key) =>
        rail.Match(
            Some: live => live.Fire(
                    at: GrasshopperPoint.HistoryReplay,
                    fact: new HookSignal.IntentCase(Operation: op, DocumentId: Some(document.Identity)),
                    key: key)
                .Map(static _ => unit),
            None: () => Fin.Succ(unit));

    private static Fin<GateOutcome> Free(Op key, Func<Unit> act) =>
        key.Catch(body: () => Fin.Succ((act(), (GateOutcome)new GateOutcome.SettledCase()).Item2));
}
```

## [03]-[BRANCHES]

- Owner: `BranchPath` — the reconciliation: the common ancestor of two undo-tree nodes and the shortest node path between them, the stride count deriving from the path, so a consumer replays from one branch tip to another without walking the tree itself. `BranchCrown` — the topology projection of one node: its primary child as `Option` (a tree tip has none) and its secondary children as a detached `Seq`, the material `BranchCase` promotion decides over; the crown's validity claims secondaries exist only beside a primary, because the host tree fills the primary line first. Both are `[Equatable]` — the path and secondary rosters compare ordered, the host nodes by reference, so a settled projection is a comparable value.
- Entry: `HistoryLedger.Reconcile(Node from, Node to, Op? key = null)` → `Fin<BranchPath>` — common ancestor and shortest path fused into one evidence value; `HistoryLedger.Crown(Node root, Op? key = null)` → `Fin<BranchCrown>` — the marshalled child-roster read. Both are pure `Node`-topology reads and marshal through the kernel's synchronous `UiThread.Run` arity alone, needing no document.
- Law: reconciliation walks the PUBLIC `Node` topology — `Parent`/`ParentIfNotRoot`/`Depth` carry the whole ancestor derivation, because the host's own `History.FindCommonAncestor`/`FindShortestPath` are internal and a fence naming an uncallable member is fiction; the depth-aligned two-pointer walk is the standard LCA over a parent-linked tree, and the path is the up-leg from `from` joined to the reversed up-leg from `to` as two `List.unfold` climbs.
- Law: reconciliation is a read — `Reconcile` mutates nothing; the consumer inspects the path, then commits `BranchCase`/`StrideCase` operations to move the tree, so branch navigation decomposes into one read and N sealed-gate commands, never a hidden multi-step mutation.
- Law: the undo tree is host truth — no local mirror, cache, or shadow tree of `Node` topology exists in the folder; `Crown` and `Reconcile` re-read the live tree per call, and staleness is structurally impossible because nothing is retained.
- Law: `BranchPath.Strides` counts path EDGES — the node path includes both endpoints, so the stride count is `Path.Count - 1` clamped at zero, and an endpoint-inclusive count read as strides overshoots every replay by one.
- Boundary: undo-tree visualization — drawing the branch structure, hover, and picking — is `Canvas/*` territory over these projections; the node-moved stream a tree-view redraws on is `Shell/events.md`'s `Of(History)` row set.
- Packages: Grasshopper2 (`Node.Parent`/`ParentIfNotRoot`/`Depth`/`PrimaryChild`/`SecondaryChildren`), `Rasm.Interaction` (`UiThread`, `UiDispatch`, `DispatchLane`), Generator.Equals, LanguageExt.Core, `Rasm.Domain`.
- Growth: a new tree read is one projection member beside `Crown` returning its own evidence value; the reconciliation shape never widens.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Grasshopper2.Undo;
using Rasm.Domain;
using Rasm.Interaction;

namespace Rasm.Grasshopper.Document;

// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly partial record struct BranchPath(
    [property: ReferenceEquality] Node Ancestor,
    [property: OrderedEquality] Seq<Node> Path) : IValidityEvidence {
    public int Strides => int.Max(Path.Count - 1, 0);
    public bool IsValid => Ancestor is not null;
}

[Equatable]
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly partial record struct BranchCrown(
    Option<Node> Primary,
    [property: OrderedEquality] Seq<Node> Secondary) : IValidityEvidence {
    public bool IsValid => Primary.IsSome || Secondary.IsEmpty;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class HistoryLedger {
    public static Fin<BranchPath> Reconcile(Node from, Node to, Op? key = null) {
        Op active = key.OrDefault();
        return from head in Optional(from).ToFin(active.InvalidInput())
               from tail in Optional(to).ToFin(active.InvalidInput())
               from path in UiThread.Run(new UiDispatch<BranchPath>.Blocking(() => active.Catch(body: () => {
                   Node a = head;
                   Node b = tail;
                   for (; a.Depth > b.Depth; a = a.Parent) { }
                   for (; b.Depth > a.Depth; b = b.Parent) { }
                   for (; !ReferenceEquals(a, b); a = a.Parent, b = b.Parent) { }
                   Seq<Node> up = Climb(tip: head, ancestor: a);
                   Seq<Node> down = Climb(tip: tail, ancestor: a).Rev().Tail;
                   return Fin.Succ(new BranchPath(Ancestor: a, Path: up + down));
               })), DispatchLane.Interactive, active)
               select path;

        static Seq<Node> Climb(Node tip, Node ancestor) => toSeq(List.unfold(Some(tip), held => held.Map(node =>
            (node, ReferenceEquals(node, ancestor) ? Option<Node>.None : Some(node.Parent)))));
    }

    public static Fin<BranchCrown> Crown(Node root, Op? key = null) {
        Op active = key.OrDefault();
        return Optional(root).ToFin(active.InvalidInput())
            .Bind(node => UiThread.Run(new UiDispatch<BranchCrown>.Blocking(() => active.Catch(body: () =>
                Fin.Succ(new BranchCrown(
                    Primary: Optional(node.PrimaryChild),
                    Secondary: toSeq(node.SecondaryChildren))))), DispatchLane.Interactive, active));
    }
}
```

## [04]-[DENSITY_BAR]

| [INDEX] | [CONCERN]             | [OWNER]                      | [RAIL]                         | [CASES] |
| :-----: | :-------------------- | :--------------------------- | :----------------------------- | :-----: |
|  [01]   | stride direction      | `LedgerStride`               | `Stride`/`Replay` (internal)   |    2    |
|  [02]   | object-scoped verbs   | `ObjectUndoVerb`             | cases inside `SubjectCase`     |    2    |
|  [03]   | undo commands         | `HistoryOp`                  | `Commit → Fin<GateOutcome>`    |    5    |
|  [04]   | the one seal          | `HistoryLedger.Seal`         | `Seal → Fin<Unit>`             |    1    |
|  [05]   | record banking        | `HistoryLedger.Bank`         | `Bank → Fin<Record>`           |    1    |
|  [06]   | branch reconciliation | `BranchPath` + `BranchCrown` | `Reconcile`/`Crown` → `Fin<T>` |  2 + 2  |

- [01]-[STRIDE_DIRECTION]: `[SmartEnum<int>]` stride + replay columns.
- [02]-[OBJECT_SCOPED_VERBS]: `[Union]` verb pair under one subject custody.
- [03]-[UNDO_COMMANDS]: `[GenerateUnionOps]` `[Union]` on the shared gate spine, replay rows firing `history.replay`.
- [04]-[THE_ONE_SEAL]: cross-page composed seam, caller-marshal.
- [05]-[RECORD_BANKING]: `ActionList.ToRecord` mint.
- [06]-[BRANCH_RECONCILIATION]: `[Equatable]` projections over the live tree, climbs as `List.unfold`.

`DocumentGate.Run`, `GateOutcome`, kernel `UiThread`, `Op`, `Fault`, and `ValidityClaim` are composed upstream owners; the `nameof` verb strings, the two sibling object-scoped cases, and the hand `yield` climb are all deleted, and the folder's mutation gates reach the tree only through `Seal`.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
