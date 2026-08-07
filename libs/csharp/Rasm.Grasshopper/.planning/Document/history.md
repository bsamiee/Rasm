# [RASM_GRASSHOPPER_DOCUMENT_HISTORY]

`HistoryLedger` is the undo ledger of the GH2 document boundary — ONE ledger owner over the host's branching `History` tree: sealing a filled `ActionList` into the tree under one `VerbNoun`, striding the tree back and forward, re-rooting a branch, replaying a banked `Record` against a document, attaching per-object undo records, requesting autosave, the branch-reconciliation read over the public `Node` topology, and the node-topology projection over `PrimaryChild`/`SecondaryChildren`.

Every undo verb is a case of one `HistoryOp` union settled by one `Commit` gate; direction is a two-row `LedgerStride` family whose rows carry BOTH the tree stride and the record replay as delegate columns, and `Seal` is the one cross-page spelling `Document/document.md`'s `Transact` and `Document/graph.md`'s `Mutate` compose so mutation and undo are structurally one act everywhere in the folder. GH2's undo is a branching `Node` tree, never a linear stack — `PromoteChild` re-roots, reconciliation walks the tree — and this page is the only surface in the folder that touches it.

## [01]-[INDEX]

- [02]-[LEDGER]: `LedgerStride` + `HistoryOp` + `HistoryLedger` — the command union, the stride/replay row family, the one `Commit` gate, and the cross-page `Seal`/`Bank` seams.
- [03]-[BRANCHES]: `BranchPath` + `BranchCrown` — branch reconciliation and node-topology projection over the undo tree.

## [02]-[LEDGER]

- Owner: `LedgerStride` `[SmartEnum<int>]` — the direction vocabulary with two delegate columns: `Back` (key 0, `History.Undo` / `Record.Undo(Document)`), `Forward` (key 1, `History.Redo` / `Record.Redo(Document)`) — one row family serves both the live tree stride and the banked-record replay, so direction is data on both surfaces. `HistoryOp` `[Union]` `[GenerateUnionOps]` closes the command family: `SealCase(VerbNoun, ActionList)` (`History.Do` — seal a filled action buffer into the tree), `StrideCase(LedgerStride)` (step the tree), `BranchCase(Node, Node)` (`Node.PromoteChild` — promote a secondary child to the primary line), `ReplayCase(LedgerStride, Record)` (replay a banked record against the document), `AttachCase(IDocumentObject, VerbNoun, UndoAction)` (`IDocumentObject.AddUndoRecord` — object-scoped undo), `AutoSaveCase(IDocumentObject, AutoSaveReason)` (`IDocumentObject.RequestAutoSave`). Settlement evidence is `Document/document.md`'s `GateReceipt`, composed rather than re-declared: `SealCase` and the cross-page `Seal` seam carry `Seal: Some(label)` because they mint the undo record, and every other command settles with `Seal: None` because it strides or replays one already banked.
- Entry: `HistoryLedger.Commit(HistoryOp op, Option<HostDocument> graph = default, MonotonicTimeline? clock = null, Op? key = null)` → `Fin<GateReceipt>` — the command gate; `HistoryLedger.Seal(History ledger, ActionList actions, VerbNoun label, Op key)` → `Fin<Unit>` — the one-expression seal the folder's mutation gates compose inside their own marshal windows; `HistoryLedger.Bank(ActionList actions, VerbNoun label, Op? key = null)` → `Fin<Record>` — `ActionList.ToRecord`, minting a replayable record without touching the tree.
- Law: `Seal` is the folder's only `History.Do` spelling — `Document/document.md`'s `Transact` arms and `Document/graph.md`'s `Mutate` arms call it with the `ActionList` their host verb filled, so no mutation in the folder exists without its undo record and no second seal path exists to diverge; `Seal` runs on the caller's marshal and never opens its own.
- Law: a `Record` is banked evidence, not tree state — `Bank` seals an action buffer into a detached `Record` whose `ReplayCase` replays it against any document; the tree stride and the record replay share the `LedgerStride` rows, so a new direction semantics is impossible to add to one surface and forget on the other.
- Law: object-scoped undo rides the object — `AttachCase` binds a `VerbNoun`-labelled `UndoAction` to one `IDocumentObject`, and autosave intent is a per-object request with its typed `AutoSaveReason`; neither touches the document tree, both settle through the same gate.
- Law: `VerbNoun` mints as `new VerbNoun(verb, noun)` and every gate accepts the already-minted label; `ActionList`'s `params Action[]` constructor admits the empty call the folder's mutation gates make; `History.Undo`/`Redo` return `void` in both their bare and `Node`-targeted arities, so `SettledCase` IS the host's whole answer for a stride and the settled receipt over it is honest, never a dropped return.
- Boundary: undo lifecycle observation — the `Undone`/`NodeMoved` streams — is `Shell/events.md`'s `UiSource.HistoryUndone`/`HistoryNodeMoved` rows anchored on `EventAnchor.HistoryCase(History)`; a ledger consumer needing settled-undo notification composes those rows, and this page publishes nothing of its own.
- Packages: Grasshopper2 (`History.Do`/`Undo`/`Redo`, `ActionList.ToRecord`, `Record.Undo`/`Redo`, `Node.PromoteChild`, `VerbNoun`, `IDocumentObject.AddUndoRecord`/`RequestAutoSave`, `AutoSaveReason`), LanguageExt.Core, `Rasm.Domain`, `Rasm.Parametric` (`MonotonicTimeline`, `MonotonicStamp`).
- Growth: a new undo verb is one `HistoryOp` case breaking the gate's total `Switch` loudly; a new direction semantics is one `LedgerStride` row carrying both columns — zero new entrypoints.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Grasshopper2.Doc;
using Grasshopper2.Undo;
using Rasm.Csp;
using Rasm.Parametric;
using HostDocument = Grasshopper2.Doc.Document;
using UndoAction = Grasshopper2.Undo.Action;

namespace Rasm.Grasshopper.Document;

// --- [TYPES] --------------------------------------------------------------------------------
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
[GenerateUnionOps]
public abstract partial record HistoryOp {
    private HistoryOp() { }
    public sealed record SealCase(VerbNoun Label, ActionList Actions) : HistoryOp;
    public sealed record StrideCase(LedgerStride Stride) : HistoryOp;
    public sealed record BranchCase(Node Parent, Node Child) : HistoryOp;
    public sealed record ReplayCase(LedgerStride Stride, Record Record) : HistoryOp;
    public sealed record AttachCase(IDocumentObject Subject, VerbNoun Label, UndoAction Action) : HistoryOp;
    public sealed record AutoSaveCase(IDocumentObject Subject, AutoSaveReason Reason) : HistoryOp;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[BoundaryAdapter]
public static partial class HistoryLedger {
    // Seal is a seam, not a gate — the CALLING gate owns the receipt, its stamps, and its timeline, so this
    // spelling answers only whether the ledger accepted the record; a second receipt minted here was evidence
    // every caller discarded.
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

    public static Fin<GateReceipt> Commit(HistoryOp op, Option<HostDocument> graph = default, MonotonicTimeline? clock = null, Op? key = null) {
        Op active = key.OrDefault();
        return from valid in Optional(op).ToFin(active.InvalidInput())
               from timeline in clock is { } shared ? Fin.Succ(shared) : MonotonicTimeline.Of(provider: TimeProvider.System, key: active)
               from entered in timeline.Capture(key: active)
               from answer in DocumentScope.Resolve(graph: graph, key: active, body: document => valid.Switch(
                state: (Key: active, Graph: document),
                sealCase: static (frame, c) =>
                    Seal(ledger: frame.Graph.Undo, actions: c.Actions, label: c.Label, key: frame.Key)
                        .Map(_ => (Verb: nameof(HistoryOp.SealCase), Seal: Some(c.Label))),
                strideCase: static (frame, c) => Free(frame.Key, nameof(HistoryOp.StrideCase),
                    () => c.Stride.Stride(ledger: frame.Graph.Undo)),
                branchCase: static (frame, c) => Free(frame.Key, nameof(HistoryOp.BranchCase),
                    () => Op.Side(action: () => c.Parent.PromoteChild(c.Child))),
                replayCase: static (frame, c) => Free(frame.Key, nameof(HistoryOp.ReplayCase),
                    () => c.Stride.Replay(record: c.Record, document: frame.Graph)),
                attachCase: static (frame, c) => Free(frame.Key, nameof(HistoryOp.AttachCase),
                    () => Op.Side(action: () => c.Subject.AddUndoRecord(c.Label, c.Action))),
                autoSaveCase: static (frame, c) => Free(frame.Key, nameof(HistoryOp.AutoSaveCase),
                    () => Op.Side(action: () => c.Subject.RequestAutoSave(c.Reason)))))
               from settled in timeline.Capture(key: active)
               from latency in timeline.Elapsed(start: entered, end: settled, key: active)
               select new GateReceipt(
                   Operation: active, Verb: answer.Verb, Seal: answer.Seal, Outcome: new GateOutcome.SettledCase(),
                   Entered: entered, Settled: settled, Latency: latency);
    }

    private static Fin<(string Verb, Option<VerbNoun> Seal)> Free(Op key, string verb, Func<Unit> act) =>
        key.Catch(body: () => Fin.Succ((act(), (Verb: verb, Seal: Option<VerbNoun>.None)).Item2));
}
```

## [03]-[BRANCHES]

- Owner: `BranchPath` — the reconciliation receipt: the common ancestor of two undo-tree nodes and the shortest node path between them, the stride count deriving from the path, so a consumer replays from one branch tip to another without walking the tree itself. `BranchCrown` — the topology projection of one node: its primary child as `Option` (a tree tip has none) and its secondary children as a detached `Seq`, the material `BranchCase` promotion decides over; the crown's validity claims secondaries exist only beside a primary, because the host tree fills the primary line first.
- Entry: `HistoryLedger.Reconcile(Node from, Node to, Op? key = null)` → `Fin<BranchPath>` — common ancestor and shortest path fused into one evidence value; `HistoryLedger.Crown(Node root, Op? key = null)` → `Fin<BranchCrown>` — the marshalled child-roster read. Both are pure `Node`-topology reads and marshal through `EtoDispatch` alone, needing no document.
- Law: reconciliation walks the PUBLIC `Node` topology — `Parent`/`ParentIfNotRoot`/`Depth` carry the whole ancestor derivation, because the host's own `History.FindCommonAncestor`/`FindShortestPath` are internal and a fence naming an uncallable member is fiction; the depth-aligned two-pointer walk is the standard LCA over a parent-linked tree, and the path is the up-leg from `from` joined to the reversed up-leg from `to`.
- Law: reconciliation is a read — `Reconcile` mutates nothing; the consumer inspects the path, then commits `BranchCase`/`StrideCase` operations to move the tree, so branch navigation decomposes into one read and N sealed-gate commands, never a hidden multi-step mutation.
- Law: the undo tree is host truth — no local mirror, cache, or shadow tree of `Node` topology exists in the folder; `Crown` and `Reconcile` re-read the live tree per call, and staleness is structurally impossible because nothing is retained.
- Law: `BranchPath.Strides` counts path EDGES — the node path includes both endpoints, so the stride count is `Path.Count - 1` clamped at zero, and an endpoint-inclusive count read as strides overshoots every replay by one.
- Boundary: undo-tree visualization — drawing the branch structure, hover, and picking — is `Canvas/*` territory over these projections; the `NodeMoved` stream a tree-view redraws on is `Shell/events.md`'s `HistoryNodeMoved` row.
- Packages: Grasshopper2 (`Node.Parent`/`ParentIfNotRoot`/`Depth`/`PrimaryChild`/`SecondaryChildren`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new tree read is one projection member beside `Crown` returning its own evidence value; the reconciliation shape never widens.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Grasshopper2.Undo;
using Rasm.Csp;
using Rasm.Grasshopper.Eto;
using HostDocument = Grasshopper2.Doc.Document;

namespace Rasm.Grasshopper.Document;

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct BranchPath(Node Ancestor, Seq<Node> Path) : IValidityEvidence {
    public int Strides => int.Max(Path.Count - 1, 0);
    public bool IsValid => ValidityClaim.Of(holds: Ancestor is not null);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct BranchCrown(Option<Node> Primary, Seq<Node> Secondary) : IValidityEvidence {
    public bool IsValid => ValidityClaim.Of(holds: Primary.IsSome || Secondary.IsEmpty);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static partial class HistoryLedger {
    public static Fin<BranchPath> Reconcile(Node from, Node to, Op? key = null) {
        Op active = key.OrDefault();
        return from head in Optional(from).ToFin(active.InvalidInput())
               from tail in Optional(to).ToFin(active.InvalidInput())
               from path in EtoDispatch.Run(body: () => active.Catch(body: () => {
                   // Depth-aligned two-pointer LCA over the parent-linked tree; the loops are the named
                   // statement kernel for a walk whose step count is the tree depth.
                   Node a = head;
                   Node b = tail;
                   for (; a.Depth > b.Depth; a = a.Parent) { }
                   for (; b.Depth > a.Depth; b = b.Parent) { }
                   for (; !ReferenceEquals(a, b); a = a.Parent, b = b.Parent) { }
                   Seq<Node> up = toSeq(Climb(head, a));
                   Seq<Node> down = toSeq(Climb(tail, a)).Rev().Tail;
                   return Fin.Succ(new BranchPath(Ancestor: a, Path: up + down));
               }), key: active)
               select path;

        static IEnumerable<Node> Climb(Node tip, Node ancestor) {
            for (Node held = tip; ; held = held.Parent) {
                yield return held;
                if (ReferenceEquals(held, ancestor)) { yield break; }
            }
        }
    }

    public static Fin<BranchCrown> Crown(Node root, Op? key = null) {
        Op active = key.OrDefault();
        return Optional(root).ToFin(active.InvalidInput())
            .Bind(node => EtoDispatch.Run(body: () => active.Catch(body: () =>
                Fin.Succ(new BranchCrown(
                    Primary: Optional(node.PrimaryChild),
                    Secondary: toSeq(node.SecondaryChildren)))), key: active));
    }
}
```

## [04]-[DENSITY_BAR]

| [INDEX] | [CONCERN]             | [OWNER]                      | [RAIL]                         | [CASES] |
| :-----: | :-------------------- | :--------------------------- | :----------------------------- | :-----: |
|  [01]   | stride direction      | `LedgerStride`               | `Stride`/`Replay` (internal)   |    2    |
|  [02]   | undo commands         | `HistoryOp`                  | `Commit → Fin<GateReceipt>`    |    6    |
|  [03]   | the one seal          | `HistoryLedger.Seal`         | `Seal → Fin<Unit>`             |    1    |
|  [04]   | record banking        | `HistoryLedger.Bank`         | `Bank → Fin<Record>`           |    1    |
|  [05]   | branch reconciliation | `BranchPath` + `BranchCrown` | `Reconcile`/`Crown` → `Fin<T>` |  2 + 2  |

- [01]-[STRIDE_DIRECTION]: `[SmartEnum<int>]` stride + replay columns.
- [02]-[UNDO_COMMANDS]: `[GenerateUnionOps]` `[Union]` over the folder's one receipt.
- [03]-[THE_ONE_SEAL]: cross-page composed seam, caller-marshal.
- [04]-[RECORD_BANKING]: `ActionList.ToRecord` mint.
- [05]-[BRANCH_RECONCILIATION]: evidence receipts over the live tree.

`DocumentScope.Resolve`, `GateReceipt`, `GateOutcome`, `EtoDispatch`, `Op`, `Fault`, and `ValidityClaim` are composed upstream owners; every undo capability lands as the cases and rows above, and the folder's mutation gates reach the tree only through `Seal`.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
