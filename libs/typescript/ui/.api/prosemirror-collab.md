# [TS_UI_API_PROSEMIRROR_COLLAB]

`prosemirror-collab` owns client-side collaborative editing against a central authority: the plugin tracks a linear version number and the unconfirmed local steps, `sendableSteps(state)` hands the outbound batch to any transport, and `receiveTransaction(state, steps, clientIDs)` rebases the local work over what the authority accepted. Convergence comes from the authority's total order over steps, not from a commutative merge.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the plugin configuration and the outbound batch shape — both structural, neither exported as a named type.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :--------------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `collab` config parameter    | interface     | `{version?, clientID?}`; `clientID` is `number \| string` |
|  [02]   | `sendableSteps` return       | interface     | `{version, steps, clientID, origins} \| null`             |
|  [03]   | `receiveTransaction` options | interface     | `{mapSelectionBackward?}`                                 |

- `CollabConfig` is declared but never exported; annotate a config object structurally or inline it at the `collab(...)` call.
- `clientID` defaults to a random 32-bit number per plugin instance and must stay stable for the life of one editor, so a caller that persists identity supplies it.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the four declared surfaces — one plugin, one outbound read, one inbound fold, one version read.

| [INDEX] | [SURFACE]                                                             | [SHAPE] | [CAPABILITY]                                           |
| :-----: | :-------------------------------------------------------------------- | :------ | :----------------------------------------------------- |
|  [01]   | `collab({version, clientID}) -> Plugin`                               | static  | tracks the confirmed version and the unconfirmed steps |
|  [02]   | `sendableSteps(state) -> {version, steps, clientID, origins} \| null` | static  | outbound batch, `null` when nothing is unconfirmed     |
|  [03]   | `receiveTransaction(state, steps, clientIDs, options) -> Transaction` | static  | folds authority steps, rebasing local work over them   |
|  [04]   | `getVersion(state) -> number`                                         | static  | reports the version this client synced to              |

- `sendableSteps` returns `readonly Step[]`; serialize each with `Step.toJSON()` and rehydrate on arrival with `Step.fromJSON(schema, json)`.
- `origins` holds the original `Transaction` objects that produced each step, carrying timestamps and metadata; the steps themselves may have been rebased away from those transactions.
- `receiveTransaction` requires `steps.length === clientIDs.length` with the arrays index-aligned; the client's own id in that list marks a confirmation rather than a foreign change.
- `options.mapSelectionBackward` maps a text selection's sides with negative bias so content inserted at the cursor lands after it — off by default.
- `rebaseSteps` ships as a runtime export with no type declaration and takes an unexported `Rebaseable[]`; the rebase it performs is what `receiveTransaction` already runs.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One central authority owns the total order: it holds a monotonically increasing version and an append-only step log, accepts a batch only when its `version` matches the authority's current version, and rejects a stale batch outright. Every client converges by replaying that one order.
- `collab` closes the client loop and drives it: `sendableSteps(state)` reports unconfirmed work, the transport ships `{version, steps: steps.map(s => s.toJSON()), clientID}`, and every authority response — the client's own accepted batch included — comes back through `receiveTransaction` as `(steps, clientIDs)`. Rejection resolves by pulling the missing steps and calling `receiveTransaction`, after which `sendableSteps` reports the rebased batch.
- Rebasing is invert-apply-remap, run inside `receiveTransaction`: local unconfirmed steps invert in reverse order, the authority's steps apply, and each local step maps through the accumulated mapping and re-applies where it survives. `receiveTransaction` drops a local step whose target the remote change deleted, the intended conflict resolution.
- This is a central-authority protocol, not a CRDT: steps carry no causal metadata, do not commute, and converge only through the authority's order. Peer-to-peer exchange, offline divergence beyond the retained step log, and merging two independently advanced histories are all outside what the plugin resolves.
- `getVersion(state)` is the resume coordinate: a client reconnecting requests every step after that version, feeds them through `receiveTransaction`, and continues; a client booting fresh calls `collab({version})` with the version the document snapshot was taken at.
- Steps are the persisted record, not documents: the authority stores the step log and periodic snapshots, so history, presence, and any audit derive from the same append-only sequence the wire already carries.

[STACKING]:
- `prosemirror-transform`(`.api/prosemirror-transform.md`): the `Step` values `sendableSteps` returns serialize with `Step.toJSON()` and rehydrate with `Step.fromJSON(schema, json)`; `receiveTransaction` rebases through `Step.map(mapping)` and `Transform.maybeStep` over a `Mapping.slice`. Every custom step registers with `Step.jsonID` on both client and authority before any batch carries it.
- `prosemirror-state`(`.api/prosemirror-state.md`): `collab()` is a `Plugin` with its own `StateField` holding the version and the unconfirmed steps, and `receiveTransaction` returns a `Transaction` the caller dispatches like any other.
- `prosemirror-history`(`.api/prosemirror-history.md`): the history plugin rebases its own branches over remote steps and never undoes another client's change, so the two plugins compose with no coordination; both are folded from the same transaction stream.
- `core/interchange/codec`: `Wire` owns the closed wire vocabulary — an editor batch lands as one family row carrying `{version, steps, clientID}`, inheriting fault classification, bounded quarantine, and parity obligation instead of minting a private message shape.
- `core/state/commit`: `Commit` owns the branch's content-keyed commit graph and Merkle anti-entropy; the collab step log is the editor-local linear log under one authority and projects into a commit at snapshot grain rather than replacing that owner.
- `core/state/presence`: `Presence` owns the ephemeral cursor, selection, and view axes as a CRDT; collaborative caret rendering rides that fold, keeping this package to document convergence alone.
- `core/state/merge`: `Merge.Instance` is the branch's commutative merge law; a collab step carries no such law, so a document reached through this plugin never enters a `Merge` fold as if it commuted.
- within-lib `view/content`: the editor page mounts `collab({version, clientID})` per session, drives the send loop off `sendableSteps` on each dispatch, and routes every authority frame through `receiveTransaction` before any other plugin sees it.

[LOCAL_ADMISSION]:
- Route every authority response — including the confirmation of this client's own batch — through `receiveTransaction`; applying a local batch as confirmed by hand desynchronizes the version.
- Ship steps as `Step.toJSON()` payloads over the branch's own wire family and rehydrate with `Step.fromJSON(schema, json)`; the plugin owns no transport.
- Hold `clientID` stable for the life of an editor session and derive it from the branch's identity owner rather than the random default where a durable actor identity exists.
- Keep document convergence here and presence axes on the presence fold; a cursor position sent as a document step is the rejected shape.
