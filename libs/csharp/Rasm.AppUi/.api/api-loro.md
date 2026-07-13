# [RASM_APPUI_API_LORO]

`LoroCs` is the UniFFI-generated C# binding over the Rust `loro` CRDT engine — the Eg-walker/Fugue high-performance sequence + map + text + movable-list + tree + counter CRDT runtime backing the Shell/Editing notebook, annotation, table, and live-data collaboration op-log, presence, and time-travel. One `LoroDoc` owns a forest of nested containers, exports/imports a binary op-log (full snapshot, shallow snapshot, or delta updates), checks out / forks / reverts to any historical `Frontiers`, and streams subscriber diffs as typed `Diff` records. It retires the bespoke `NotebookCrdt` LWW algebra: the document IS the merge authority, `Cursor` is the position that survives concurrent edits, `EphemeralStore`/`Awareness` are the presence channels, and `UndoManager` is the local-only undo that respects remote ops. The whole surface is `IDisposable` — every container, cursor, frontiers, and version-vector handle wraps a Rust pointer freed on dispose.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `LoroCs`
- package: `LoroCs`
- assembly: `LoroCs` (single shipped managed assembly)
- namespace: `LoroCs` (one flat namespace — containers, value/diff unions, FFI plumbing, and exceptions all live here)
- license: MIT (`<license type="expression">MIT</license>`)
- build-floor: ships only `lib/netstandard2.0` (no `net8.0`+ asset); the `net10.0` consumer binds `netstandard2.0` forward — the documented surface
- native asset: the loro Rust core ships as `runtimes/osx-arm64/native/loro.dylib` (UniFFI P/Invoke `_UniFFILib` over the native lib); outside-Rhino / companion only — the native dylib firebreaks it out of any in-Rhino plugin ALC, the same posture as the other native AppUi rows
- xml-doc: none shipped (no `.xml` beside the assembly; member intent is the UniFFI-generated signature)
- dependencies: zero managed NuGet deps (`netstandard2.0` group is empty; the binding self-contains its FFI marshalling)
- rail: collaboration

## [02]-[PUBLIC_TYPES]

[DOCUMENT_AND_CONTAINERS]: the document root and its six container kinds
- rail: collaboration
- every container is `class Loro* : ILoro*, IDisposable`; bind to the interface, hold the handle, dispose on detach. A container is created by `LoroDoc.Get<Kind>(ContainerIdLike)` (attach-or-create) or nested via a parent's `Insert*Container`/`GetOrCreate*Container`/`EnsureMergeable*`.

| [INDEX] | [SYMBOL]                                          | [ROLE]             |
| :-----: | :------------------------------------------------ | :----------------- |
|  [01]   | `LoroDoc : ILoroDoc, IDisposable`                 | merge authority    |
|  [02]   | `LoroText : ILoroText, IDisposable`               | rich text          |
|  [03]   | `LoroMap : ILoroMap, IDisposable`                 | LWW map            |
|  [04]   | `LoroList : ILoroList, IDisposable`               | Fugue sequence     |
|  [05]   | `LoroMovableList : ILoroMovableList, IDisposable` | stable reorder     |
|  [06]   | `LoroTree : ILoroTree, IDisposable`               | hierarchy          |
|  [07]   | `LoroCounter : ILoroCounter, IDisposable`         | PN-counter         |
|  [08]   | `LoroUnknown : ILoroUnknown, IDisposable`         | opaque future kind |

- [01]-[DOCUMENT]: `LoroDoc` owns the container forest, import/export, checkout/fork/revert, and subscriptions.
- [02]-[TEXT]: `LoroText` owns insert/delete/splice, marks, cursors, and deltas.
- [03]-[MAP]: `LoroMap` maps keys to LWW values or containers.
- [04]-[LIST]: `LoroList` orders values and containers as a Fugue sequence.
- [05]-[MOVABLE_LIST]: `LoroMovableList` applies stable-id `Mov` and `Set` operations without delete-and-insert replacement.
- [06]-[TREE]: `LoroTree` owns `Create`, `Mov`, `MovBefore`, `MovAfter`, fractional-index ordering, and per-node `GetMeta` maps.
- [07]-[COUNTER]: `LoroCounter` applies conflict-free `Increment` and `Decrement` operations.
- [08]-[UNKNOWN]: `LoroUnknown` round-trips a container kind newer than the binding opaquely.

[VALUE_AND_DIFF_UNIONS]: the closed `record`-hierarchy discriminated unions (pattern-match the leaf)
- rail: collaboration
- these are sealed-by-construction `abstract record` roots with one leaf `record` per case — the dispatch surface the editing/diff rails fold over, never a parallel enum

| [INDEX] | [UNION]            | [CONSUMER]             |
| :-----: | :----------------- | :--------------------- |
|  [01]   | `LoroValue`        | container leaf value   |
|  [02]   | `Diff`             | subscriber payload     |
|  [03]   | `ExportMode`       | `Export` selector      |
|  [04]   | `ContainerType`    | container discriminant |
|  [05]   | `ContainerId`      | container identity     |
|  [06]   | `TreeParentId`     | tree parent slot       |
|  [07]   | `Index`            | path hop               |
|  [08]   | `TextDelta`        | rich-text delta        |
|  [09]   | `ListDiffItem`     | list change            |
|  [10]   | `TreeExternalDiff` | tree change            |

- [01]-[LORO_VALUE]: `Null`, `Bool(bool)`, `Double(double)`, `I64(long)`, `Binary(byte[])`, `String(string)`, `List(LoroValue[])`, `Map(Dictionary<string,LoroValue>)`, and `Container(ContainerId)` carry every container leaf value.
- [02]-[DIFF]: `Text(TextDelta[])`, `List(ListDiffItem[])`, `Map(MapDelta)`, `Tree(TreeDiff)`, `Counter(double)`, and `Unknown` discriminate per-container subscriber changes.
- [03]-[EXPORT_MODE]: `Snapshot`, `Updates(VersionVector)`, `UpdatesInRange(IdSpan[])`, `ShallowSnapshot(Frontiers)`, `StateOnly(Frontiers?)`, and `SnapshotAt(Frontiers)` select `Export` output.
- [04]-[CONTAINER_TYPE]: `Text`, `Map`, `List`, `MovableList`, `Tree`, `Counter`, and `Unknown(byte Kind)` discriminate the kind inside `ContainerId`.
- [05]-[CONTAINER_ID]: `Root(string Name, ContainerType)` identifies named roots, and `Normal(ulong Peer, int Counter, ContainerType)` identifies op-created containers.
- [06]-[TREE_PARENT_ID]: `Root`, `Node(TreeId)`, `Deleted`, and `Unexist` represent a tree node's parent slot.
- [07]-[INDEX]: `Key(string)`, `Seq(uint)`, and `Node(TreeId)` represent one `GetByPath(Index[])` hop.
- [08]-[TEXT_DELTA]: `Insert(string, Dictionary<string,LoroValue>? Attributes)`, `Delete(uint)`, and `Retain(uint, Dictionary<string,LoroValue>? Attributes)` form Quill-style rich-text delta operations.
- [09]-[LIST_DIFF_ITEM]: `Insert(ValueOrContainer[], bool IsMove)`, `Delete(uint)`, and `Retain(uint)` form list-change operations.
- [10]-[TREE_EXTERNAL_DIFF]: `Create(TreeParentId, uint, string)`, `Move(...)`, and `Delete(TreeParentId, uint)` form tree-change operations inside `TreeDiff`.

[VALUE_CARRIERS_AND_HELPERS]: the version/identity/option value records and the collaboration helpers
- rail: collaboration

| [INDEX] | [SYMBOL]                                              | [ROLE]          |
| :-----: | :---------------------------------------------------- | :-------------- |
|  [01]   | `VersionVector : IVersionVector, IDisposable`         | version base    |
|  [02]   | `Frontiers : IFrontiers, IDisposable`                 | DAG cut         |
|  [03]   | `Cursor : ICursor, IDisposable`                       | stable position |
|  [04]   | `ValueOrContainer : IValueOrContainer, IDisposable`   | result union    |
|  [05]   | `Subscription : ISubscription, IDisposable`           | event lifetime  |
|  [06]   | `UndoManager : IUndoManager, IDisposable`             | local undo      |
|  [07]   | `EphemeralStore : IEphemeralStore, IDisposable`       | presence state  |
|  [08]   | `Awareness : IAwareness, IDisposable`                 | peer state      |
|  [09]   | `Configure : IConfigure, IDisposable`                 | configuration   |
|  [10]   | `LoroValueLike` / `LoroValueLikeImpl : LoroValueLike` | write coercion  |
|  [11]   | `ChangeMeta` and collaboration option/value records   | data carriers   |

- [01]-[VERSION_VECTOR]: `VersionVector` carries the per-peer op frontier and forms the `Export(Updates(vv))` delta base.
- [02]-[FRONTIERS]: `Frontiers` carries a DAG cut of op ids for `Checkout`, `Fork`, and `Revert`.
- [03]-[CURSOR]: `Cursor` carries a text or list position across concurrent edits through `GetCursor` and `GetCursorPos`.
- [04]-[VALUE_OR_CONTAINER]: A `Get` returns either a `LoroValue` leaf or a live nested-container handle.
- [05]-[SUBSCRIPTION]: `Subscription` holds a live subscription and detaches it on disposal.
- [06]-[UNDO_MANAGER]: `new UndoManager(LoroDoc)` owns local undo and redo while skipping remote operations.
- [07]-[EPHEMERAL_STORE]: `new EphemeralStore(long timeoutMs)` holds TTL-expiring cursor and selection state.
- [08]-[AWARENESS]: `new Awareness(ulong peer, long timeoutMs)` holds per-peer user and color state.
- [09]-[CONFIGURE]: `Configure` owns merge-interval, record-timestamp, and text-style configuration.
- [10]-[LORO_VALUE_LIKE]: `LoroValueLike` and `LoroValueLikeImpl` carry write values accepted by `Insert`, `Set`, and `Mark`.
- [11]-[RECORD_CARRIERS]: `ChangeMeta`, `CommitOptions`, `UpdateOptions`, `ImportStatus`, `StyleConfig`, `IdSpan`, `TreeId`, `CounterSpan`, and `AbsolutePosition` carry commit, import, update, identifier, and span data.

[ENUMS]: the bounded vocabularies
- rail: collaboration

| [INDEX] | [SYMBOL]                | [CASES]                           |
| :-----: | :---------------------- | :-------------------------------- |
|  [01]   | `Side`                  | `Left`, `Right`                   |
|  [02]   | `PosType`               | `Bytes`, `Unicode`, `Utf16`       |
|  [03]   | `ExpandType`            | `Before`, `After`, `Both`, `None` |
|  [04]   | `EventTriggerKind`      | `Local`, `Import`, `Checkout`     |
|  [05]   | `UndoOrRedo`            | `OnPop`, `OnPush`                 |
|  [06]   | `EphemeralEventTrigger` | `Local`, `Import`, `Timeout`      |
|  [07]   | `Ordering`              | `Less`, `Equal`, `Greater`        |

- [01]-[SIDE]: Controls cursor stickiness.
- [02]-[POS_TYPE]: Selects the byte, Unicode, or UTF-16 text index space.
- [03]-[EXPAND_TYPE]: Controls rich-text mark expansion on an edge insertion.
- [04]-[EVENT_TRIGGER_KIND]: Identifies whether a subscriber diff came from a local edit, import, or checkout.
- [05]-[UNDO_OR_REDO]: Discriminates the `OnPop` and `OnPush` directions.
- [06]-[EPHEMERAL_EVENT_TRIGGER]: Identifies local, imported, and timed-out ephemeral-store changes.
- [07]-[ORDERING]: Carries the `CmpWithFrontiers` result.

## [03]-[ENTRYPOINTS]

[DOC_LIFECYCLE]: import / export / commit / time-travel on `LoroDoc`
- rail: collaboration

| [INDEX] | [SURFACE]                                                                              | [ROLE]          |
| :-----: | :------------------------------------------------------------------------------------- | :-------------- |
|  [01]   | `new LoroDoc()`                                                                        | empty document  |
|  [02]   | `byte[] Export(ExportMode mode)`                                                       | export          |
|  [03]   | `ImportStatus Import(byte[])` / `ImportBatch(byte[][])` / `ImportWith(byte[], origin)` | merge op log    |
|  [04]   | `void Commit()` / `CommitWith(CommitOptions)`                                          | seal change     |
|  [05]   | `void Checkout(Frontiers)` / `CheckoutToLatest()`                                      | time travel     |
|  [06]   | `LoroDoc Fork()` / `ForkAt(Frontiers)`                                                 | branch          |
|  [07]   | `void RevertTo(Frontiers)`                                                             | inverse ops     |
|  [08]   | `DiffBatch Diff(Frontiers a, Frontiers b)`                                             | change set      |
|  [09]   | `Frontiers OplogFrontiers()` / `VersionVector OplogVv()`                               | op-log version  |
|  [10]   | `Frontiers StateFrontiers()` / `VersionVector StateVv()`                               | state version   |
|  [11]   | `ImportJsonUpdates(string)` / `ExportJsonUpdates(VersionVector, VersionVector)`        | JSON exchange   |
|  [12]   | `ExportShallowSnapshot(Frontiers)` / `IsShallow()` / `ShallowSinceVv()`                | shallow history |
|  [13]   | `SetPeerId(ulong)` / `PeerId()`                                                        | peer identity   |
|  [14]   | `SetRecordTimestamp(bool)` / `SetChangeMergeInterval(long)`                            | change policy   |

- [01]-[CREATE]: `new LoroDoc()` creates an empty document with auto-commit enabled.
- [02]-[EXPORT]: `ExportMode` selects snapshot, shallow snapshot, updates, or state-only output from the polymorphic `Export` surface.
- [03]-[IMPORT]: `Import`, `ImportBatch`, and `ImportWith` merge a remote op log, and `ImportStatus` carries success and pending spans.
- [04]-[COMMIT]: `CommitWith(CommitOptions)` seals origin, timestamp, and message metadata with the pending transaction.
- [05]-[CHECKOUT]: `Checkout` selects a historical cut, and `CheckoutToLatest` restores the latest state.
- [06]-[FORK]: `Fork` branches from the current cut, and `ForkAt` branches from a historical cut.
- [07]-[REVERT]: `RevertTo` appends inverse operations that return state to the selected version while preserving history.
- [08]-[DIFF]: `Diff` returns the change set between two cuts for custom replay.
- [09]-[JSON]: `ImportJsonUpdates` and `ExportJsonUpdates` exchange human-readable operations for debugging and cross-implementation use.
- [10]-[SHALLOW]: `ExportShallowSnapshot` drops operations before a cut, `IsShallow` identifies the trimmed form, and `ShallowSinceVv` returns its base.

[CONTAINER_ACCESS]: the polymorphic container accessors on `LoroDoc`
- rail: collaboration
- one semantic per kind, two arities: `Get<Kind>(ContainerIdLike)` attaches-or-creates (throws on type mismatch); `TryGet<Kind>(ContainerIdLike)` returns null when absent/mismatched. `ContainerIdLike` accepts a root name string or a `ContainerId`.

| [INDEX] | [SURFACE]                                                 | [RESULT]           |
| :-----: | :-------------------------------------------------------- | :----------------- |
|  [01]   | `Get<Kind>(ContainerIdLike)`                              | attached container |
|  [02]   | `TryGet<Kind>(ContainerIdLike)`                           | nullable container |
|  [03]   | `GetByPath(Index[])` / `GetByStrPath(string)`             | nested value       |
|  [04]   | `Jsonpath(string)` / `SubscribeJsonpath(string, ...)`     | query or feed      |
|  [05]   | `GetContainer(ContainerId)` / `HasContainer(ContainerId)` | handle or probe    |
|  [06]   | `GetValue()` / `GetDeepValue()` / `GetDeepValueWithId()`  | document snapshot  |

- [01]-[GET]: `GetText`, `GetMap`, `GetList`, `GetMovableList`, `GetTree`, and `GetCounter` attach or create a root container of the named kind.
- [02]-[TRY_GET]: `TryGetText`, `TryGetMap`, `TryGetList`, `TryGetMovableList`, `TryGetTree`, and `TryGetCounter` return null when the container is absent or mismatched.
- [03]-[PATH]: `GetByPath(Index[])` and `GetByStrPath(string)` resolve a nested container or value.
- [04]-[JSONPATH]: `Jsonpath(string)` returns a value array, and `SubscribeJsonpath(string, ...)` returns its live subscription.
- [05]-[CONTAINER_ID]: `GetContainer(ContainerId)` resolves a live handle, and `HasContainer(ContainerId)` probes its presence.
- [06]-[SNAPSHOT]: `GetValue`, `GetDeepValue`, and `GetDeepValueWithId` snapshot the document tree as `LoroValue`.

[SUBSCRIPTIONS]: the diff and commit event streams on `LoroDoc`
- rail: collaboration
- every `Subscribe*` returns a `Subscription` (`IDisposable`); the callback delegate receives the typed event. Hold the subscription for its lifetime, dispose to detach.

| [INDEX] | [SURFACE]                                   | [FIRES_ON]         |
| :-----: | :------------------------------------------ | :----------------- |
|  [01]   | `SubscribeRoot(Subscriber)`                 | any container      |
|  [02]   | `Subscribe(ContainerId, Subscriber)`        | one container      |
|  [03]   | `SubscribeLocalUpdate(LocalUpdateCallback)` | local op-log delta |
|  [04]   | `SubscribePreCommit(PreCommitCallback)`     | pre-commit         |
|  [05]   | `SubscribeFirstCommitFromPeer(...)`         | new peer commit    |

`SubscribeLocalUpdate` emits the bytes broadcast to peers, while every other callback receives its typed event. Every container also carries its own `Subscription? Subscribe(Subscriber)` — a per-container diff feed equivalent to `Subscribe(container.Id(), subscriber)` on the document, null when the container is detached.

[CONTAINER_OPS]: the per-kind editing surface (the dense vocabularies the editing rail composes)
- rail: collaboration

| [INDEX] | [CONTAINER]       | [EDIT_MODEL]       |
| :-----: | :---------------- | :----------------- |
|  [01]   | `LoroText`        | rich-text sequence |
|  [02]   | `LoroMap`         | LWW key map        |
|  [03]   | `LoroList`        | ordered sequence   |
|  [04]   | `LoroMovableList` | stable sequence    |
|  [05]   | `LoroTree`        | movable hierarchy  |
|  [06]   | `LoroCounter`     | PN-counter         |

[LORO_TEXT]:
- Mutation: `Insert(pos, s)`, `Delete(pos, len)`, `Splice(pos, len, s)`, and `Update(s, UpdateOptions)` apply direct and whole-document diff updates.
- Marks: `Mark(from, to, key, value)` and `Unmark` edit rich-text marks.
- Position: `GetCursor(pos, Side)` returns a stable cursor.
- Delta: `ToDelta()` returns `TextDelta[]`, and `ApplyDelta` applies that representation.
- Encoding: Unicode, `*Utf8`, and `*Utf16` forms compose with `ConvertPos(index, PosType from, PosType to)`.

[LORO_MAP]:
- Values: `Insert(key, v)`, `Get(key)`, `Delete(key)`, `Keys()`, and `Values()` own key access.
- Containers: `Ensure*Mergeable(key)` creates nested containers idempotently, while `Insert*Container` and `GetOrCreate*Container(key, child)` attach children.
- Provenance: `GetLastEditor(key)` returns `ulong?` editor identity.

[LORO_LIST]:
- Values: `Insert(pos, v)`, `Push(v)`, `Pop()`, `Delete(pos, len)`, `Get(index)`, and `ToVec()` own sequence access.
- Containers: `Insert*Container(pos, child)` attaches a nested container.
- Position: `GetCursor(pos, Side)` returns a stable cursor, and `GetIdAt(pos)` returns `Id?` identity.

[LORO_MOVABLE_LIST]:
- Base: Every `LoroList` operation remains available.
- Reorder: `Mov(from, to)` preserves stable identity, while `Set(pos, value)` and `Set*Container(pos, child)` replace values in place.
- Provenance: `GetCreatorAt`, `GetLastEditorAt`, and `GetLastMoverAt(pos)` expose edit identity.

[LORO_TREE]:
- Lifecycle: `Create(parent)`, `CreateAt(parent, index)`, and `Delete(target)` own node lifetime.
- Movement: `Mov`, `MovBefore`, `MovAfter`, and `MovTo(target, parent, index)` own hierarchy changes.
- Traversal: `Children(parent)`, `Roots()`, `Nodes()`, and `Parent(target)` expose structure.
- Metadata: `GetMeta(target)` returns the per-node `LoroMap` attribute map.
- Ordering: `EnableFractionalIndex(jitter)` configures fractional ordering, and `FractionalIndex(target)` reads the node position.

[LORO_COUNTER]:
- Value: `Increment(double)`, `Decrement(double)`, and `GetValue()` own the counter.

[COLLABORATION_HELPERS]: undo, presence, and ephemeral state
- rail: collaboration

| [INDEX] | [HELPER]         | [ROLE]            |
| :-----: | :--------------- | :---------------- |
|  [01]   | `UndoManager`    | local undo        |
|  [02]   | `EphemeralStore` | expiring presence |
|  [03]   | `Awareness`      | peer awareness    |

[UNDO_MANAGER]:
- Stack: `UndoManager(LoroDoc)` exposes `Undo()`, `Redo()`, `CanUndo()`, and `CanRedo()` while respecting remote operations.
- Grouping: `GroupStart` and `GroupEnd` group transactions.
- Policy: `SetOnPush(OnPush)`, `SetOnPop(OnPop)`, `SetMaxUndoSteps(uint)`, and `AddExcludeOriginPrefix(string)` bind hooks, depth, and origin exclusion.

[EPHEMERAL_STORE]:
- State: `EphemeralStore(long).Set(key, value)`, `Get(key)`, `Encode(key)`, `EncodeAll()`, and `Apply(byte[])` own cursor and selection presence.
- Expiry: `RemoveOutdated()` evicts expired state.
- Feed: `Subscribe(EphemeralSubscriber)` and `SubscribeLocalUpdate(...)` emit presence changes whose encoded bytes broadcast to peers.

[AWARENESS]:
- State: `Awareness(ulong peer, long).SetLocalState(value)` owns per-peer user and color state on a separate channel.
- Wire: `Encode(peers)` and `Apply(byte[])` exchange state as `AwarenessPeerUpdate`.

## [04]-[ERROR_TAXONOMY]

[BOUNDARY_FAULTS]: the `LoroException` hierarchy lifted at the collaboration edge
- rail: collaboration
- all throw types derive from `LoroException` (with `LoroEncodeException`, `CannotFindRelativePosition`, `ChangeTravelException`, `JsonPathException`, `UpdateTimeoutException` as intermediate roots); the boundary folds them to the editing rail's typed failures.

| [INDEX] | [FAULT]     | [CAUSE]                |
| :-----: | :---------- | :--------------------- |
|  [01]   | decode      | corrupt import         |
|  [02]   | encoding    | incompatible history   |
|  [03]   | attachment  | detached use           |
|  [04]   | range       | invalid text index     |
|  [05]   | cursor      | missing anchor         |
|  [06]   | travel      | missing history target |
|  [07]   | JSONPath    | invalid expression     |
|  [08]   | transaction | invalid state          |
|  [09]   | update      | timeout                |

- [01]-[DECODE]: `DecodeException`, `DecodeChecksumMismatchException`, `DecodeDataCorruptionException`, and `DecodeVersionVectorException` reject corrupt or incompatible imported op-log bytes.
- [02]-[ENCODING]: `IncompatibleFutureEncodingException`, `ImportUnsupportedEncodingMode`, and `ImportUpdatesThatDependsOnOutdatedVersion` reject newer encodings, unsupported modes, and missing dependencies.
- [03]-[ATTACHMENT]: `MisuseDetachedContainer`, `ReattachAttachedContainer`, and `EditWhenDetached` reject use after detach, reattachment, and edits during time travel.
- [04]-[RANGE]: `OutOfBound`, `EndIndexLessThanStartIndex`, `Utf8InUnicodeCodePoint`, and `Utf16InUnicodeCodePoint` reject invalid ranges and `PosType` encoding boundaries.
- [05]-[CURSOR]: `CannotFindRelativePosition` branches to `IdNotFound`, `ContainerDeleted`, and `HistoryCleared` when a cursor anchor disappears.
- [06]-[TRAVEL]: `ChangeTravelException` branches to `TargetIdNotFound` and `TargetVersionNotIncluded` when `TravelChangeAncestors` or checkout targets absent history.
- [07]-[JSONPATH]: `JsonPathException` branches to `InvalidJsonPath` and `EvaluationException` for malformed or unevaluable `Jsonpath` expressions.
- [08]-[TRANSACTION]: `LockException`, `ImportWhenInTxn`, `AutoCommitNotStarted`, and `TransactionException` reject concurrent access and transaction-state misuse.
- [09]-[UPDATE]: `UpdateTimeoutException` branches to `Timeout` when `Update` or `UpdateByLine` exceeds `UpdateOptions.TimeoutMs`.

## [05]-[STACKING_AND_RAIL]

[STACKING]:

[LIVE_SESSION]:
- Authority: `LoroCs` merges each local `SubscribeLocalUpdate` delta broadcast by the AppHost transport and imported by live peers, so `Collab/` carries no custom merge algebra.
- Replacement: The document retires the bespoke `NotebookCrdt` LWW algebra.
- Boundary: The Loro-native wire remains session-ephemeral, while the Persistence bit-parity law owns durable truth.
- Catch-up: `ExportMode.Updates(VersionVector)` synchronizes a reconnecting active epoch, and a cold start seeds its epoch from the ledger replay window.

[DURABLE_TRUTH]:
- Stream: Persistence owns the typed edit-intent stream projected from AppUi domain operations onto `OpLogEntry` and `SyncOpKind` rows through the `Version/ledger` changefeed, generalizing the `RevertibleOp` to `SyncOpKind` discipline.
- Accelerator: `LoroDoc.Export(Snapshot)` persists only as a derivable, deletable, content-keyed cold-start blob composed through `ContentHash.Of`, reconstructible from the op log, and never authoritative.
- Hashing: A direct `System.IO.Hashing` `XxHash128` mint over snapshot bytes remains outside the design.
- Load: A cold load decodes the ledger replay window into a fresh `LoroDoc` in log order, while `ExportShallowSnapshot(Frontiers)` bounds retained history.

[PRESENCE]:
- Cursor: `EphemeralStore` publishes TTL-expiring `Cursor` state, and `GetCursorPos(cursor)` returns `PosQueryResult` for remote caret and selection rendering.
- Awareness: `Awareness` carries per-peer user and color identity on its own ephemeral topic.
- Wire: Both channels encode to `byte[]` over the AppHost transport without entering the durable op log.

[UNDO]:
- Local: `UndoManager(doc)` binds local Shell/Editing/history undo, and `AddExcludeOriginPrefix` excludes programmatic `CommitOptions.Origin` values so remote operations remain intact.
- Revert: `RevertTo(Frontiers)` appends forward inverse operations for an audited, history-preserving revert.

[RICH_TEXT]:
- Delta: `LoroText.ToDelta()` and `ApplyDelta(TextDelta[])` bind the Quill-shaped `AvaloniaEdit` document representation.
- Marks: `Mark(from, to, key, value)` carries inline style spans.
- Encoding: `ConvertPos` maps Avalonia UTF-16 offsets to Loro Unicode indices across the `Bytes`, `Unicode`, and `Utf16` `PosType` cases.

[STRUCTURED_EDITING]:
- Graph: `LoroTree` and its per-node `GetMeta` map bind the NodeEditor canvas through one bidirectional projection with echo suppression.
- Events: `EventTriggerKind.Local`, `Import`, and `Checkout` route local tree commits and remote ReactiveUI updates without re-emission.
- Inspector: The same tree and map projection backs inspector and outline surfaces.
- Table: `LoroMovableList.Mov` reorders DataGrid rows without delete-and-insert identity loss.
- Aggregate: `LoroCounter` carries conflict-free aggregate tiles.

[RAIL_LAW]:
- Packages: `LoroCs` (zero managed deps; native `loro.dylib`, outside-Rhino only)
- Owns: the Shell/Editing collaboration op-log, presence, time-travel, and local undo — the document merge authority for notebook/annotation/table/live-data
- Accept: one long-lived `LoroDoc` per collaborative document; `Get<Kind>(name)` to attach named root containers; `SubscribeLocalUpdate` → broadcast / `Import` → merge as the only sync path; `Export(Snapshot)` as the persisted content-keyed blob; `Cursor` via `EphemeralStore`/`Awareness` for presence; `UndoManager` with origin-exclusion for local undo; the `LoroValue`/`Diff`/`ExportMode` unions pattern-matched at their leaf
- Reject: a hand-rolled LWW/merge algebra beside the document (the CRDT IS the merge); a presence cursor stored in the durable op-log (use the ephemeral channel); an und/redo stack that ignores remote-op origins; treating any `Loro*`/`Cursor`/`Frontiers`/`VersionVector` handle as managed-GC'd (all are `IDisposable` Rust-pointer wrappers); referencing `LoroCs` from an in-Rhino plugin assembly (the native dylib is companion-only)
