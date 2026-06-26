# [RASM_APPUI_API_LORO]

`LoroCs` is the UniFFI-generated C# binding over the Rust `loro` CRDT engine — the Eg-walker/Fugue high-performance sequence + map + text + movable-list + tree + counter CRDT runtime backing the Shell/Editing notebook, annotation, table, and live-data collaboration op-log, presence, and time-travel. One `LoroDoc` owns a forest of nested containers, exports/imports a binary op-log (full snapshot, shallow snapshot, or delta updates), checks out / forks / reverts to any historical `Frontiers`, and streams subscriber diffs as typed `Diff` records. It retires the bespoke `NotebookCrdt` LWW algebra: the document IS the merge authority, `Cursor` is the position that survives concurrent edits, `EphemeralStore`/`Awareness` are the presence channels, and `UndoManager` is the local-only undo that respects remote ops. The whole surface is `IDisposable` — every container, cursor, frontiers, and version-vector handle wraps a Rust pointer freed on dispose.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `LoroCs`
- package: `LoroCs`
- version: `1.13.6`
- assembly: `LoroCs` (single shipped managed assembly)
- namespace: `LoroCs` (one flat namespace — containers, value/diff unions, FFI plumbing, and exceptions all live here)
- license: MIT (`<license type="expression">MIT</license>`)
- build-floor: ships only `lib/netstandard2.0` (no `net8.0`+ asset); the `net10.0` consumer binds `netstandard2.0` forward — the documented surface
- native asset: the loro Rust core ships as `runtimes/osx-arm64/native/loro.dylib` (UniFFI P/Invoke `_UniFFILib` over the native lib); **outside-Rhino / companion only** — the native dylib firebreaks it out of any in-Rhino plugin ALC, the same posture as the other native AppUi rows
- xml-doc: none shipped (no `.xml` beside the assembly; member intent is the UniFFI-generated signature)
- dependencies: zero managed NuGet deps (`netstandard2.0` group is empty; the binding self-contains its FFI marshalling)
- rail: collaboration

## [02]-[PUBLIC_TYPES]

[DOCUMENT_AND_CONTAINERS]: the document root and its six container kinds
- rail: collaboration
- every container is `class Loro* : ILoro*, IDisposable`; bind to the interface, hold the handle, dispose on detach. A container is created by `LoroDoc.Get<Kind>(ContainerIdLike)` (attach-or-create) or nested via a parent's `Insert*Container`/`GetOrCreate*Container`/`EnsureMergeable*`.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]   | [CAPABILITY]                                                  |
| :-----: | :-------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `LoroDoc : ILoroDoc, IDisposable`   | document        | the merge authority — container forest, import/export, checkout/fork/revert, subscribe |
|  [02]   | `LoroText : ILoroText, IDisposable` | text container  | collaborative rich-text — insert/delete/splice, marks, cursors, delta |
|  [03]   | `LoroMap : ILoroMap, IDisposable`   | map container   | LWW key→value/container map                                  |
|  [04]   | `LoroList : ILoroList, IDisposable` | list container  | ordered Fugue sequence of values/containers                  |
|  [05]   | `LoroMovableList : ILoroMovableList, IDisposable` | movable list | list with stable-id `Mov`/`Set` — reorder without delete+insert |
|  [06]   | `LoroTree : ILoroTree, IDisposable` | tree container  | movable hierarchy — `Create`/`Mov`/`MovBefore`/`MovAfter`, fractional-index ordering, per-node `GetMeta` map |
|  [07]   | `LoroCounter : ILoroCounter, IDisposable` | counter container | conflict-free PN-counter — `Increment`/`Decrement` |
|  [08]   | `LoroUnknown : ILoroUnknown, IDisposable` | forward-compat | a container kind newer than this binding (round-trips opaquely) |

[VALUE_AND_DIFF_UNIONS]: the closed `record`-hierarchy discriminated unions (pattern-match the leaf)
- rail: collaboration
- these are sealed-by-construction `abstract record` roots with one leaf `record` per case — the dispatch surface the editing/diff rails fold over, never a parallel enum

| [INDEX] | [UNION (`abstract record`)] | [LEAF CASES]                                                 | [CONSUMER]                          |
| :-----: | :-------------------------- | :----------------------------------------------------------- | :---------------------------------- |
|  [01]   | `LoroValue`                 | `Null`/`Bool(bool)`/`Double(double)`/`I64(long)`/`Binary(byte[])`/`String(string)`/`List(LoroValue[])`/`Map(Dictionary<string,LoroValue>)`/`Container(ContainerId)` | the leaf value carried by every container |
|  [02]   | `Diff`                      | `Text(TextDelta[])`/`List(ListDiffItem[])`/`Map(MapDelta)`/`Tree(TreeDiff)`/`Counter(double)`/`Unknown` | the subscriber event payload (per-container change) |
|  [03]   | `ExportMode`                | `Snapshot`/`Updates(VersionVector)`/`UpdatesInRange(IdSpan[])`/`ShallowSnapshot(Frontiers)`/`StateOnly(Frontiers?)`/`SnapshotAt(Frontiers)` | the `Export(ExportMode)` selector  |
|  [04]   | `ContainerType`             | `Text`/`Map`/`List`/`MovableList`/`Tree`/`Counter`/`Unknown(byte Kind)` | the container-kind discriminant inside `ContainerId` |
|  [05]   | `ContainerId`               | `Root(string Name, ContainerType)`/`Normal(ulong Peer, int Counter, ContainerType)` | container identity (named root vs op-created) |
|  [06]   | `TreeParentId`              | `Root`/`Node(TreeId)`/`Deleted`/`Unexist`                    | a tree node's parent slot           |
|  [07]   | `Index`                     | `Key(string)`/`Seq(uint)`/`Node(TreeId)`                     | one hop in a `GetByPath(Index[])` path |
|  [08]   | `TextDelta`                 | `Insert(string, Dictionary<string,LoroValue>? Attributes)`/`Delete(uint)`/`Retain(uint, Dictionary<string,LoroValue>? Attributes)` | Quill-style rich-text delta op       |
|  [09]   | `ListDiffItem`              | `Insert(ValueOrContainer[], bool IsMove)`/`Delete(uint)`/`Retain(uint)` | list change op                      |
|  [10]   | `TreeExternalDiff`          | `Create(TreeParentId, uint, string)`/`Move(...)`/`Delete(TreeParentId, uint)` | tree change op (inside `TreeDiff`)  |

[VALUE_CARRIERS_AND_HELPERS]: the version/identity/option value records and the collaboration helpers
- rail: collaboration

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]   | [CAPABILITY]                                                  |
| :-----: | :-------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `VersionVector : IVersionVector, IDisposable` | version | per-peer op frontier; the `Export(Updates(vv))` delta base   |
|  [02]   | `Frontiers : IFrontiers, IDisposable` | version     | a DAG cut (set of op-ids); the `Checkout`/`Fork`/`Revert` target |
|  [03]   | `Cursor : ICursor, IDisposable`   | position        | a stable text/list position surviving concurrent edits (`GetCursor`→`GetCursorPos`) |
|  [04]   | `ValueOrContainer : IValueOrContainer, IDisposable` | value/handle | a `Get` result — either a `LoroValue` leaf or a live nested container handle |
|  [05]   | `Subscription : ISubscription, IDisposable` | lifetime  | a live subscription handle; dispose to unsubscribe           |
|  [06]   | `UndoManager : IUndoManager, IDisposable` | undo      | local-only undo/redo that skips remote ops; `new UndoManager(LoroDoc)` |
|  [07]   | `EphemeralStore : IEphemeralStore, IDisposable` | presence | TTL-expiring ephemeral key→value state (cursors, selections); `new EphemeralStore(long timeoutMs)` |
|  [08]   | `Awareness : IAwareness, IDisposable` | presence    | per-peer awareness state (user, color); `new Awareness(ulong peer, long timeoutMs)` |
|  [09]   | `Configure : IConfigure, IDisposable` | config      | merge-interval / record-timestamp / text-style config        |
|  [10]   | `LoroValueLike` (interface) / `LoroValueLikeImpl : LoroValueLike` | input coercion | the write-side value the `Insert`/`Set`/`Mark` setters accept |
|  [11]   | `ChangeMeta` / `CommitOptions` / `UpdateOptions` / `ImportStatus` / `StyleConfig` / `IdSpan` / `TreeId` / `CounterSpan` / `AbsolutePosition` | record carriers | commit/import/update metadata and id/span value records |

[ENUMS]: the bounded vocabularies
- rail: collaboration

| [INDEX] | [SYMBOL]                | [CASES / CAPABILITY]                                          |
| :-----: | :---------------------- | :----------------------------------------------------------- |
|  [01]   | `Side`                  | `Left`/`Right` — cursor stickiness side                      |
|  [02]   | `PosType`               | `Bytes`/`Unicode`/`Utf16` — text index space (the tri-encoding axis) |
|  [03]   | `ExpandType`            | rich-text mark expand behavior on edge insert (`Before`/`After`/`Both`/`None`) |
|  [04]   | `EventTriggerKind`      | `Local`/`Import`/`Checkout` — what fired a subscriber diff    |
|  [05]   | `UndoOrRedo`            | the `OnPop`/`OnPush` direction discriminant                  |
|  [06]   | `EphemeralEventTrigger` | `Local`/`Import`/`Timeout` — ephemeral-store change cause     |
|  [07]   | `Ordering`              | `Less`/`Equal`/`Greater` — `CmpWithFrontiers` result         |

## [03]-[ENTRYPOINTS]

[DOC_LIFECYCLE]: import / export / commit / time-travel on `LoroDoc`
- rail: collaboration

| [INDEX] | [SURFACE]                                  | [SHAPE / CAPABILITY]                                          |
| :-----: | :----------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `new LoroDoc()`                            | a fresh empty document (auto-commit on)                      |
|  [02]   | `byte[] Export(ExportMode mode)`           | the one polymorphic export — snapshot / shallow / updates / state-only by case |
|  [03]   | `ImportStatus Import(byte[])` / `ImportBatch(byte[][])` / `ImportWith(byte[], origin)` | merge a remote op-log; `ImportStatus` carries success + pending spans |
|  [04]   | `void Commit()` / `CommitWith(CommitOptions)` | seal the pending transaction into a change (origin/timestamp/message) |
|  [05]   | `void Checkout(Frontiers)` / `CheckoutToLatest()` | time-travel the document state to a historical cut          |
|  [06]   | `LoroDoc Fork()` / `ForkAt(Frontiers)`     | branch a new independent document from now / a historical cut |
|  [07]   | `void RevertTo(Frontiers)`                 | append inverse ops returning state to `version` (history-preserving undo) |
|  [08]   | `DiffBatch Diff(Frontiers a, Frontiers b)` | the change-set between two cuts (drives a custom replay)     |
|  [09]   | `Frontiers OplogFrontiers()` / `VersionVector OplogVv()` / `StateFrontiers()` / `StateVv()` | the current op-log vs state version reads |
|  [10]   | `ImportStatus ImportJsonUpdates(string)` / `string ExportJsonUpdates(VersionVector, VersionVector)` | the human-readable JSON op interchange (debug / cross-impl) |
|  [11]   | `byte[] ExportShallowSnapshot(Frontiers)` / `bool IsShallow()` / `VersionVector ShallowSinceVv()` | gc-trimmed shallow history (drop ops before a cut)           |
|  [12]   | `void SetPeerId(ulong)` / `ulong PeerId()` / `void SetRecordTimestamp(bool)` / `SetChangeMergeInterval(long)` | peer identity + change-recording knobs |

[CONTAINER_ACCESS]: the polymorphic container accessors on `LoroDoc`
- rail: collaboration
- one semantic per kind, two arities: `Get<Kind>(ContainerIdLike)` attaches-or-creates (throws on type mismatch); `TryGet<Kind>(ContainerIdLike)` returns null when absent/mismatched. `ContainerIdLike` accepts a root name string or a `ContainerId`.

| [INDEX] | [SURFACE]                                  | [CAPABILITY]                                                  |
| :-----: | :----------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `GetText/GetMap/GetList/GetMovableList/GetTree/GetCounter(ContainerIdLike)` | attach-or-create a named root container of that kind         |
|  [02]   | `TryGetText/TryGetMap/.../TryGetCounter(ContainerIdLike)` | the null-returning probe form                                |
|  [03]   | `ValueOrContainer? GetByPath(Index[])` / `GetByStrPath(string)` | resolve a nested container/value by path                     |
|  [04]   | `ValueOrContainer[] Jsonpath(string)` / `Subscription SubscribeJsonpath(string, ...)` | JSONPath query + live JSONPath subscription                  |
|  [05]   | `ValueOrContainer? GetContainer(ContainerId)` / `bool HasContainer(ContainerId)` | resolve / probe by full id                                   |
|  [06]   | `LoroValue GetValue()` / `GetDeepValue()` / `GetDeepValueWithId()` | snapshot the whole document tree as a plain `LoroValue`      |

[SUBSCRIPTIONS]: the diff and commit event streams on `LoroDoc`
- rail: collaboration
- every `Subscribe*` returns a `Subscription` (`IDisposable`); the callback delegate receives the typed event. Hold the subscription for its lifetime, dispose to detach.

| [INDEX] | [SURFACE]                                          | [FIRES ON]                                                   |
| :-----: | :------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `Subscription SubscribeRoot(Subscriber)`           | any change to any container (the whole-document diff feed)  |
|  [02]   | `Subscription Subscribe(ContainerId, Subscriber)`  | changes to one container only                               |
|  [03]   | `Subscription SubscribeLocalUpdate(LocalUpdateCallback)` | each local op-log delta (the bytes to broadcast to peers)   |
|  [04]   | `Subscription SubscribePreCommit(PreCommitCallback)` / `SubscribeFirstCommitFromPeer(...)` | the pre-commit hook / first-op-from-a-new-peer hook         |

[CONTAINER_OPS]: the per-kind editing surface (the dense vocabularies the editing rail composes)
- rail: collaboration

| [INDEX] | [CONTAINER]       | [KEY OPS]                                                     |
| :-----: | :---------------- | :----------------------------------------------------------- |
|  [01]   | `LoroText`        | `Insert(pos, s)`/`Delete(pos, len)`/`Splice(pos, len, s)`/`Update(s, UpdateOptions)` (whole-doc diff-update); `Mark(from, to, key, value)`/`Unmark` rich-text marks; `Cursor? GetCursor(pos, Side)`; `TextDelta[] ToDelta()`/`ApplyDelta`; tri-encoding `*Utf8`/`*Utf16`/unicode forms + `ConvertPos(index, PosType from, PosType to)` |
|  [02]   | `LoroMap`         | `Insert(key, v)`/`Get(key)`/`Delete(key)`/`Keys()`/`Values()`; `Ensure*Mergeable(key)` (idempotent nested-container create); `Insert*Container`/`GetOrCreate*Container(key, child)`; `ulong? GetLastEditor(key)` |
|  [03]   | `LoroList`        | `Insert(pos, v)`/`Push(v)`/`Pop()`/`Delete(pos, len)`/`Get(index)`/`ToVec()`; `Insert*Container(pos, child)`; `Cursor? GetCursor(pos, Side)`; `Id? GetIdAt(pos)` |
|  [04]   | `LoroMovableList` | all `LoroList` ops plus `Mov(from, to)` (stable-id reorder) / `Set(pos, value)` / `Set*Container(pos, child)`; provenance `GetCreatorAt`/`GetLastEditorAt`/`GetLastMoverAt(pos)` |
|  [05]   | `LoroTree`        | `Create(parent)`/`CreateAt(parent, index)`/`Delete(target)`; `Mov`/`MovBefore`/`MovAfter`/`MovTo(target, parent, index)`; `Children(parent)`/`Roots()`/`Nodes()`/`Parent(target)`; `LoroMap GetMeta(target)` (per-node attribute map); fractional-index `EnableFractionalIndex(jitter)`/`FractionalIndex(target)` |
|  [06]   | `LoroCounter`     | `Increment(double)`/`Decrement(double)`/`GetValue()`         |

[COLLABORATION_HELPERS]: undo, presence, and ephemeral state
- rail: collaboration

| [INDEX] | [SURFACE]                                          | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `UndoManager(LoroDoc)` → `Undo()`/`Redo()`/`CanUndo()`/`CanRedo()` | local-only undo respecting remote ops; `GroupStart`/`GroupEnd` transaction grouping |
|  [02]   | `UndoManager.SetOnPush(OnPush)`/`SetOnPop(OnPop)`/`SetMaxUndoSteps(uint)`/`AddExcludeOriginPrefix(string)` | undo-stack hooks + bounded depth + origin exclusion         |
|  [03]   | `EphemeralStore(long).Set(key, value)`/`Get(key)`/`Encode(key)`/`EncodeAll()`/`Apply(byte[])` | cursor/selection presence channel; `RemoveOutdated()` TTL eviction |
|  [04]   | `EphemeralStore.Subscribe(EphemeralSubscriber)`/`SubscribeLocalUpdate(...)` | the presence change feed (broadcast `Encode` to peers)      |
|  [05]   | `Awareness(ulong peer, long).SetLocalState(value)`/`Encode(peers)`/`Apply(byte[])` → `AwarenessPeerUpdate` | per-peer awareness (user identity/color) over a separate channel |

## [04]-[ERROR_TAXONOMY]

[BOUNDARY_FAULTS]: the `LoroException` hierarchy lifted at the collaboration edge
- rail: collaboration
- all throw types derive from `LoroException` (with `LoroEncodeException`, `CannotFindRelativePosition`, `ChangeTravelException`, `JsonPathException`, `UpdateTimeoutException` as intermediate roots); the boundary folds them to the editing rail's typed failures.

| [INDEX] | [THROWN ROOT / LEAF]                                         | [DISCRIMINANT / CAUSE]                                       |
| :-----: | :---------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `DecodeException` / `DecodeChecksumMismatchException` / `DecodeDataCorruptionException` / `DecodeVersionVectorException` | a corrupt / incompatible imported op-log byte stream         |
|  [02]   | `IncompatibleFutureEncodingException` / `ImportUnsupportedEncodingMode` / `ImportUpdatesThatDependsOnOutdatedVersion` | an op-log from a newer/incompatible loro version or a missing-dependency import |
|  [03]   | `MisuseDetachedContainer` / `ReattachAttachedContainer` / `EditWhenDetached` | a container used after detach, or an edit while time-traveled |
|  [04]   | `OutOfBound` / `EndIndexLessThanStartIndex` / `Utf8InUnicodeCodePoint` / `Utf16InUnicodeCodePoint` | an index/range fault (often a `PosType` encoding mismatch)    |
|  [05]   | `CannotFindRelativePosition` (→ `IdNotFound`/`ContainerDeleted`/`HistoryCleared`) | a `Cursor` whose anchor was deleted / gc'd                   |
|  [06]   | `ChangeTravelException` (→ `TargetIdNotFound`/`TargetVersionNotIncluded`) | a `TravelChangeAncestors` / checkout target not in history    |
|  [07]   | `JsonPathException` (→ `InvalidJsonPath`/`EvaluationException`) | a malformed / unevaluable `Jsonpath` expression              |
|  [08]   | `LockException` / `ImportWhenInTxn` / `AutoCommitNotStarted` / `TransactionException` | a concurrent-access / transaction-state misuse               |
|  [09]   | `UpdateTimeoutException` (→ `Timeout`)                       | `Update`/`UpdateByLine` exceeded `UpdateOptions.TimeoutMs`    |

## [05]-[STACKING_AND_RAIL]

[STACKING]:
- op-log spine over the AppHost wire: `SubscribeLocalUpdate` yields each local delta `byte[]`; the AppHost transport broadcasts it and a peer `Import`s it — the document is the merge authority, so the Shell/Editing collaboration rail holds NO custom merge logic, retiring the bespoke `NotebookCrdt` LWW algebra. `ExportMode.Updates(VersionVector)` is the catch-up delta a reconnecting client requests; `ExportMode.Snapshot` is the cold-load.
- content-addressed durability: `LoroDoc.Export(Snapshot)` is the persisted blob the Persistence object-store spine keys by its `System.IO.Hashing` `XxHash128` — the same content-key discipline `api-objectstore`/`api-minio` state; `ExportShallowSnapshot(Frontiers)` is the gc-trimmed variant for bounded history.
- presence beside data: `Cursor` (a position that survives concurrent edits) is published through `EphemeralStore` (TTL-expiring) — a remote caret/selection renders from `GetCursorPos(cursor)` → `PosQueryResult`, never mixed into the durable op-log. `Awareness` carries the per-peer user/color identity on its own channel. Both encode to `byte[]` riding the same AppHost transport as the data updates but on a separate ephemeral topic.
- undo that respects remote ops: `UndoManager(doc)` is the local-only undo the Shell/Editing/history rail binds — `AddExcludeOriginPrefix` excludes programmatic origins (set via `CommitWith(CommitOptions{Origin})`) so a user's Ctrl-Z never reverts a peer's concurrent edit; `RevertTo(Frontiers)` is the alternative history-preserving (forward-op) revert for an audited timeline.
- rich-text into the editor: `LoroText.ToDelta()`/`ApplyDelta(TextDelta[])` is the Quill-shaped delta the `AvaloniaEdit` document binds; `Mark(from, to, key, value)` carries inline style spans, and the `PosType` tri-encoding (`Bytes`/`Unicode`/`Utf16`) lets the binding map Avalonia's UTF-16 offsets onto loro's unicode indices via `ConvertPos`.
- tree/map into the inspector and tables: `LoroTree` (movable hierarchy with per-node `GetMeta` map) backs the Shell/Editing parametric/dependency-graph and outline surfaces; `LoroMovableList.Mov` is the stable-id row-reorder the `Avalonia.Controls.DataGrid` table edit needs (reorder without delete+insert losing identity); `LoroCounter` is the conflict-free aggregate tile.

[RAIL_LAW]:
- Packages: `LoroCs` (zero managed deps; native `loro.dylib`, outside-Rhino only)
- Owns: the Shell/Editing collaboration op-log, presence, time-travel, and local undo — the document merge authority for notebook/annotation/table/live-data
- Accept: one long-lived `LoroDoc` per collaborative document; `Get<Kind>(name)` to attach named root containers; `SubscribeLocalUpdate` → broadcast / `Import` → merge as the only sync path; `Export(Snapshot)` as the persisted content-keyed blob; `Cursor` via `EphemeralStore`/`Awareness` for presence; `UndoManager` with origin-exclusion for local undo; the `LoroValue`/`Diff`/`ExportMode` unions pattern-matched at their leaf
- Reject: a hand-rolled LWW/merge algebra beside the document (the CRDT IS the merge); a presence cursor stored in the durable op-log (use the ephemeral channel); an und/redo stack that ignores remote-op origins; treating any `Loro*`/`Cursor`/`Frontiers`/`VersionVector` handle as managed-GC'd (all are `IDisposable` Rust-pointer wrappers); referencing `LoroCs` from an in-Rhino plugin assembly (the native dylib is companion-only)
