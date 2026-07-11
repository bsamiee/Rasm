# [RASM_GRASSHOPPER_DOCUMENT_HISTORY]

The undo ledger of the GH2 document boundary — ONE ledger owner (`HistoryLedger`) over the host's branching `History` tree: sealing a filled `ActionList` into the tree under one `VerbNoun`, striding the tree back and forward, re-rooting a branch, replaying a banked `Record` against a document, attaching per-object undo records, and requesting autosave — plus the branch-reconciliation read (`FindCommonAncestor`/`FindShortestPath`) and the node-topology projection over `PrimaryChild`/`SecondaryChildren`. The census-era `DocumentHistory` command family collapses here: every undo verb is a case of one `HistoryOp` union settled by one `Commit` gate, direction is a two-row `LedgerStride` family whose rows carry BOTH the tree stride and the record replay as delegate columns, and `Seal` is the one cross-page spelling `Document/document.md`'s `Transact` and `Document/graph.md`'s `Mutate` compose so mutation and undo are structurally one act everywhere in the folder. GH2's undo is a branching `Node` tree, never a linear stack — `PromoteChild` re-roots, reconciliation walks the tree — and this page is the only surface in the folder that touches it. Every fallible step rides an `Op`-keyed `Fin<T>` rail inside one UI marshal; every receipt proves itself through `ValidityClaim.All`.

## [01]-[INDEX]

- [02]-[LEDGER]: `LedgerStride` + `HistoryOp` + `LedgerReceipt` + `HistoryLedger` — the command union, the stride/replay row family, the one `Commit` gate, and the cross-page `Seal`/`Bank` seams.
- [03]-[BRANCHES]: `BranchPath` + `BranchCrown` — branch reconciliation and node-topology projection over the undo tree.

## [02]-[LEDGER]

- Owner: `LedgerStride` `[SmartEnum<int>]` — the direction vocabulary with two delegate columns: `Back` (key 0, `History.Undo` / `Record.Undo(Document)`), `Forward` (key 1, `History.Redo` / `Record.Redo(Document)`) — one row family serves both the live tree stride and the banked-record replay, so direction is data on both surfaces. `HistoryOp` `[Union]` `[GenerateUnionOps]` closes the command family: `SealCase(VerbNoun, ActionList)` (`History.Do` — seal a filled action buffer into the tree), `StrideCase(LedgerStride)` (step the tree), `BranchCase(Node, Node)` (`Node.PromoteChild` — promote a secondary child to the primary line), `ReplayCase(LedgerStride, Record)` (replay a banked record against the document), `AttachCase(IDocumentObject, VerbNoun, UndoAction)` (`IDocumentObject.AddUndoRecord` — object-scoped undo), `AutoSaveCase(IDocumentObject, AutoSaveReason)` (`IDocumentObject.RequestAutoSave`). `LedgerReceipt` is the settlement evidence — the raising `Op`, the settled case name, and the marshal latency — implementing `IValidityEvidence`.
- Entry: `HistoryLedger.Commit(HistoryOp op, Option<HostDocument> graph = default, Op? key = null)` → `Fin<LedgerReceipt>` — the command gate; `HistoryLedger.Seal(History ledger, ActionList actions, VerbNoun label, Op key)` → `Fin<LedgerReceipt>` — the one-expression seal the folder's mutation gates compose inside their own marshal windows; `HistoryLedger.Bank(ActionList actions, VerbNoun label, Op? key = null)` → `Fin<Record>` — `ActionList.ToRecord`, minting a replayable record without touching the tree.
- Law: `Seal` is the folder's only `History.Do` spelling — `Document/document.md`'s `Transact` arms and `Document/graph.md`'s `Mutate` arms call it with the `ActionList` their host verb filled, so no mutation in the folder exists without its undo record and no second seal path exists to diverge; `Seal` runs on the caller's marshal and never opens its own.
- Law: a `Record` is banked evidence, not tree state — `Bank` seals an action buffer into a detached `Record` whose `ReplayCase` replays it against any document; the tree stride and the record replay share the `LedgerStride` rows, so a new direction semantics is impossible to add to one surface and forget on the other.
- Law: object-scoped undo rides the object — `AttachCase` binds a `VerbNoun`-labelled `UndoAction` to one `IDocumentObject`, and autosave intent is a per-object request with its typed `AutoSaveReason`; neither touches the document tree, both settle through the same gate.
- RESEARCH: `VerbNoun`'s mint spelling (constructor or factory) is catalog-unstated — every gate on this page accepts an already-minted label, and the factory lands here as one boundary adapter when the decompile fixes it; `ActionList`'s parameterless construction (assumed by the folder's mutation gates), `History.Undo`/`Redo` return shapes, and `Record.State`'s payload re-verify at the same pass.
- Boundary: undo lifecycle observation — the `Undone`/`NodeMoved` streams — is `Shell/events.md`'s `UiSource.HistoryUndone`/`HistoryNodeMoved` rows anchored on `EventAnchor.HistoryCase(History)`; a ledger consumer needing settled-undo notification composes those rows, and this page publishes nothing of its own.
- Packages: Grasshopper2 (`History.Do`/`Undo`/`Redo`, `ActionList.ToRecord`, `Record.Undo`/`Redo`, `Node.PromoteChild`, `VerbNoun`, `IDocumentObject.AddUndoRecord`/`RequestAutoSave`, `AutoSaveReason`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new undo verb is one `HistoryOp` case breaking the gate's total `Switch` loudly; a new direction semantics is one `LedgerStride` row carrying both columns — zero new entrypoints.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Grasshopper2.Doc;
using Grasshopper2.Undo;
using Rasm.Csp;
using HostDocument = Grasshopper2.Doc.Document;
using UndoAction = Grasshopper2.Undo.Action;

namespace Rasm.Grasshopper.Document;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class LedgerStride {
    public static readonly LedgerStride Back = new(
        key: 0,
        stride: static ledger => Op.Side(action: ledger.Undo),
        replay: static (record, document) => Op.Side(action: () => record.Undo(document)));
    public static readonly LedgerStride Forward = new(
        key: 1,
        stride: static ledger => Op.Side(action: ledger.Redo),
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

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct LedgerReceipt(Op Operation, string Verb, TimeSpan Latency) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(holds: !string.IsNullOrWhiteSpace(value: Verb)),
        ValidityClaim.Nonnegative(value: Latency.TotalSeconds));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[BoundaryAdapter]
public static partial class HistoryLedger {
    public static Fin<LedgerReceipt> Seal(History ledger, ActionList actions, VerbNoun label, Op key) {
        long entered = Environment.TickCount64;
        return from live in Optional(ledger).ToFin(key.MissingContext())
               from filled in Optional(actions).ToFin(key.InvalidInput())
               from _ in key.Catch(body: () => Fin.Succ(Op.Side(action: () => live.Do(label, filled))))
               select new LedgerReceipt(
                   Operation: key,
                   Verb: nameof(HistoryOp.SealCase),
                   Latency: TimeSpan.FromMilliseconds(value: Environment.TickCount64 - entered));
    }

    public static Fin<Record> Bank(ActionList actions, VerbNoun label, Op? key = null) {
        Op active = key.OrDefault();
        return Optional(actions).ToFin(active.InvalidInput())
            .Bind(filled => active.Catch(body: () => Fin.Succ(filled.ToRecord(label))));
    }

    public static Fin<LedgerReceipt> Commit(HistoryOp op, Option<HostDocument> graph = default, Op? key = null) {
        Op active = key.OrDefault();
        long entered = Environment.TickCount64;
        return Optional(op).ToFin(active.InvalidInput())
            .Bind(valid => DocumentScope.Resolve(graph: graph, key: active, body: document => valid.Switch(
                state: (Key: active, Graph: document),
                sealCase: static (frame, c) =>
                    Seal(ledger: frame.Graph.Undo, actions: c.Actions, label: c.Label, key: frame.Key).Map(static receipt => receipt.Verb),
                strideCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ((c.Stride.Stride(ledger: frame.Graph.Undo), nameof(HistoryOp.StrideCase)).Item2)),
                branchCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ((Op.Side(action: () => c.Parent.PromoteChild(c.Child)), nameof(HistoryOp.BranchCase)).Item2)),
                replayCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ((c.Stride.Replay(record: c.Record, document: frame.Graph), nameof(HistoryOp.ReplayCase)).Item2)),
                attachCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ((Op.Side(action: () => c.Subject.AddUndoRecord(c.Label, c.Action)), nameof(HistoryOp.AttachCase)).Item2)),
                autoSaveCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ((Op.Side(action: () => c.Subject.RequestAutoSave(c.Reason)), nameof(HistoryOp.AutoSaveCase)).Item2)))))
            .Map(verb => new LedgerReceipt(
                Operation: active,
                Verb: verb,
                Latency: TimeSpan.FromMilliseconds(value: Environment.TickCount64 - entered)));
    }
}
```

## [03]-[BRANCHES]

- Owner: `BranchPath` — the reconciliation receipt: the common ancestor of two undo-tree nodes and the shortest node path between them, the stride count deriving from the path, so a consumer replays from one branch tip to another without walking the tree itself. `BranchCrown` — the topology projection of one node: its primary child as `Option` (a tree tip has none) and its secondary children as a detached `Seq`, the material `BranchCase` promotion decides over; the crown's validity claims secondaries exist only beside a primary, because the host tree fills the primary line first.
- Entry: `HistoryLedger.Reconcile(Node from, Node to, Option<HostDocument> graph = default, Op? key = null)` → `Fin<BranchPath>` — `History.FindCommonAncestor` + `History.FindShortestPath` fused into one evidence value; `HistoryLedger.Crown(Node root, Op? key = null)` → `Fin<BranchCrown>` — the marshalled child-roster read.
- Law: reconciliation is a read — `Reconcile` mutates nothing; the consumer inspects the path, then commits `BranchCase`/`StrideCase` operations to move the tree, so branch navigation decomposes into one read plus N sealed-gate commands, never a hidden multi-step mutation.
- Law: the undo tree is host truth — no local mirror, cache, or shadow tree of `Node` topology exists in the folder; `Crown` and `Reconcile` re-read the live tree per call, and staleness is structurally impossible because nothing is retained.
- RESEARCH: `Node.PrimaryChild`/`SecondaryChildren` member spellings and `FindShortestPath`'s path carrier are catalog-unstated — the fences assume the natural child accessors and enumerable `Node` sequences and re-verify at decompile; `FindCommonAncestor`'s `Node` return is the settled natural reading.
- Boundary: undo-tree visualization — drawing the branch structure, hover, and picking — is `Canvas/*` territory over these projections; the `NodeMoved` stream a tree-view would redraw on is `Shell/events.md`'s `HistoryNodeMoved` row.
- Packages: Grasshopper2 (`History.FindCommonAncestor`/`FindShortestPath`, `Node.PrimaryChild`/`SecondaryChildren`), LanguageExt.Core, `Rasm.Domain`.
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
    public int Strides => Path.Count;
    public bool IsValid => ValidityClaim.Of(holds: Ancestor is not null);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct BranchCrown(Option<Node> Primary, Seq<Node> Secondary) : IValidityEvidence {
    public bool IsValid => ValidityClaim.Of(holds: Primary.IsSome || Secondary.IsEmpty);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static partial class HistoryLedger {
    public static Fin<BranchPath> Reconcile(Node from, Node to, Option<HostDocument> graph = default, Op? key = null) {
        Op active = key.OrDefault();
        return from head in Optional(from).ToFin(active.InvalidInput())
               from tail in Optional(to).ToFin(active.InvalidInput())
               from path in DocumentScope.Resolve(graph: graph, key: active, body: document => active.Catch(body: () => {
                   History ledger = document.Undo;
                   Node ancestor = ledger.FindCommonAncestor(head, tail);
                   Seq<Node> route = toSeq(ledger.FindShortestPath(head, tail));
                   return Fin.Succ(new BranchPath(Ancestor: ancestor, Path: route));
               }))
               select path;
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

| [INDEX] | [CONCERN]             | [OWNER]                        | [KIND]                                               | [RAIL]                          | [CASES] |
| :-----: | :-------------------- | :------------------------------ | :------------------------------------------------------ | :--------------------------------- | :-----: |
|  [01]   | stride direction      | `LedgerStride`                 | `[SmartEnum<int>]` stride + replay columns           | `Stride`/`Replay` (internal)    |    2    |
|  [02]   | undo commands         | `HistoryOp` + `LedgerReceipt`  | `[GenerateUnionOps]` `[Union]` + evidence receipt    | `Commit → Fin<LedgerReceipt>`   |    6    |
|  [03]   | the one seal          | `HistoryLedger.Seal`           | cross-page composed seam, caller-marshal             | `Seal → Fin<LedgerReceipt>`     |    1    |
|  [04]   | record banking        | `HistoryLedger.Bank`           | `ActionList.ToRecord` mint                           | `Bank → Fin<Record>`            |    1    |
|  [05]   | branch reconciliation | `BranchPath` + `BranchCrown`   | evidence receipts over the live tree                 | `Reconcile`/`Crown` → `Fin<T>`  |  2 + 2  |

`DocumentScope.Resolve`, `EtoDispatch`, `Op`, `Fault`, and `ValidityClaim` are composed upstream owners. The census `DocumentHistory` command family has no successor shape — its capabilities land as the cases and rows above, and the folder's mutation gates reach the tree only through `Seal`.
