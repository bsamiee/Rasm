# [STACK_CORE] — ultra-stacking research, libs/typescript/core

Scope: the wave-0 `core` folder (22 pages, value/state/interchange/observe). Every spelling below is verified against the on-disk catalogs, the installed `node_modules` `.d.ts`, or the pre-staged prefetch dossiers (machines/pubsub `[A]`,`[C]`; the OTel `[D]` layer is runtime-folder, not core). VERDICT UP FRONT: this folder is already near-ceiling — the catalogs are exploited to real depth (Subscribable/SubscriptionRef live handles in `fold`, ExecutionPlan+Budget failover in `invoke`, full typeclass merge algebra in `merge`/`evidence`/`presence`, protobuf-es/cbor-x/msgpack engines in `format`). The genuine stacking surface is concentrated in **one flagship (statechart generalization of `machine`)**, **three concrete collapses/exposures**, and **two weigh-candidates**. Most pages have NO stacking gap and are listed as such so the improver does not manufacture churn.

Admitted-at-branch but host-free and thus core-legal: `@effect/printer` (0.49.0), `@effect/experimental/VariantSchema` (0.60.0). Admitted but OUT OF SCOPE for core by the boundary law (transport/durable/host-bound): `@effect/rpc`, `@effect/cluster`, `@effect/workflow`, `@effect/sql*`, `@effect/ai*`, all `@opentelemetry/sdk-*`/exporter packages. Do NOT reach for them in core — see `[C.OOS]`.

---

## [A] — UNDERUTILIZED MEMBERS (exact spelling → owning page)

### [A1] `@effect/experimental` Machine (`core/.api` via branch `effect-experimental.md`) → `state/machine.md`

`machine.md` uses only `Machine.makeSerializable`, `Machine.serializable.make/.add`, `Machine.boot`, `Machine.restore`, `Machine.snapshot`, `Machine.retry`, and the context members `forkReplace`/`send`/`state`/`request`. The following admitted surface is unused and the folder concept (task emphasis 4: research-paper-depth machines) demands it:

| [SYMBOL] | [VERIFIED] | [STACKING USE] |
| :------- | :--------- | :------------- |
| `Actor<M> extends Subscribable.Subscribable<Machine.State<M>>` | `Machine.d.ts:153` | `Transition.Actor` surfaces only `feed`/`phase`/`freeze`. The actor IS a `Subscribable` of state — expose `state: Subscribable.Subscribable<P>` = `Subscribable.map(actor, ...)`. The page prose ("the live phase binds to view atoms through the actor's subscribable state") already asserts this but the `Actor` shape omits the member. |
| `SerializableActor<M>.sendUnknown(u): Effect<ExitEncoded, ParseError>` | `Machine.d.ts:164` | The wire-arriving request lane — a phase/signal decoded from a socket frame — has no surface; `feed` only takes a typed `S`. A `Transition.Actor.sendUnknown` closes the untrusted-signal admission. |
| context `forkOne(id)` / `forkReplaceWith` / `forkWith` / `fork` | prefetch `[A4]` | `forkOne(id)` IS `spawnChild`/`stopChild` (id-addressed child fibers); the statechart `invoke`/`spawn` generalization is these + `Machine.boot` of a child machine in the fork. Only `forkReplace` (single watchdog) is used today. |
| context `unsafeSend` / `sendAwait` / `unsafeSendAwait` | prefetch `[A4]` | `unsafeSend` is the child-completion self-signal (`done.invoke.<id>`) and the eventless-transition raise the macrostep fold drains. |
| `Machine.withTracingEnabled` / `currentTracingEnabled` | `Machine.d.ts:238` | actor-level span tracing is never wired; a machine emits no telemetry today. |

CORRECTION (prefetch `[A4]`, not a member add): `Machine.serializable.add(schema, handler)` is a **pipeable dual** (data-last returning `(self)=>self'`); `Machine.procedures.add` is a differently-shaped curried type-application `add<Req>()(...)`. The page's `.pipe(Machine.serializable.add(Feed, handler))` form is correct; keep the note so a rebuild does not "fix" it toward the wrong namespace shape.

### [A2] `effect` core → `state/machine.md`, `state/fold.md`

- `Stream.mapAccum` → `machine.md`: the page's own prose says "the exact step a stream lifts unchanged through `Stream.mapAccum`", but the owner exposes only `Transition.step` (Mealy) and `Transition.drive` (`Array.mapAccum`). There is **no stream driver member**. Add `Transition.trace = (rows) => (ops: Stream<S>) => Stream<V>` via `Stream.mapAccum` — exact symmetry with `fold.md`'s `Fold.trace`. This is the vehicle the macrostep fold rides (prefetch `[A3]` item 6).
- `Data.taggedEnum().$is` guards → `board.md`/`slo.md`/`invoke.md` use `$match` exhaustively (correct) but never `$is`; only relevant if a partial predicate arm is genuinely needed — not a gap, noted for completeness.

### [A3] `@effect/platform` (branch `effect-platform.md`) → `interchange/invoke.md`, `interchange/frame.md`

| [SYMBOL] | [VERIFIED] | [STACKING USE] |
| :------- | :--------- | :------------- |
| `MsgPack.duplexSchema({inputSchema, outputSchema})` | `MsgPack.d.ts:67` | `Gateway.duplex` composes `bytes.pipe(MsgPack.duplex())` THEN `.pipe(ChannelSchema.duplexUnknown({inputSchema, outputSchema}))` — two stages. `duplexSchema` fuses both into one call with the asymmetric CommandPayload-in/outcome-out schemas. See `[B1]`. |
| `Ndjson.duplexSchema` / `Ndjson.duplexSchemaString` | `Ndjson.d.ts:131/153` | same fusion for the ndjson arm; `duplexSchemaString` is the JSON-safe **text** lane the page's own msgpack-vs-ndjson precision law describes (the bigint-Hlc-forbidden text frame). |
| `Transferable.schema(schema)` | `effect-platform.md [03]` | `frame.md` GeometryFrame's zero-copy worker crossing ("the moment the buffer transfers … the view is dead") is the exact `Transferable` concern. BOUNDARY: the transfer marshal is the consumer's (ui/runtime) declaration, so this is likely a note not a core edit — flag for the improver, default DEFER. |

### [A4] `hash-wasm` (`core/.api/hash-wasm.md`) → `value/contentKey.md`, `interchange/frame.md`

- `IHasher` streaming (`createXXHash128(0,0)` once → `init().update(chunk).digest()`) is documented in the catalog `[04]` as the chunked-payload rail but `contentKey`'s `Digest` exposes only whole-buffer mint. `frame.md`'s `ArtifactFrame` does a **single-allocation `_joined` THEN `Parity.verified` (a second full pass)** — an incremental `IHasher.update(band)` per frame inside `_gathered` verifies without the double pass. See cross-stack `[C1]`.
- Documented-but-unused: `createBLAKE3`, `createCRC32`/`createHMAC` rows. `blake3`/`crc32` are catalog-completeness rows, not a demanded core capability — do NOT admit a second content-address notion (the mint is `xxhash128` seed-0 only, catalog `[06]` reject-law). No action.

### [A5] `@electric-sql/d2mini`/`d2ts` (`core/.api`) → `state/fold.md`

`fold.md` binds `map`/`reduce`/`consolidate`/`orderByWithFractionalIndexBTree`/`innerJoin`/`groupBy`/`iterate`/`Index.reconstructAt`/`compact`. Unused, but ALREADY flagged as growth rows in the page's `DATAFLOW_VERBS` prose ("semijoin via `filterBy`, anti-join, top-k board over a group"):
- d2mini `filterBy(other)` (semijoin), `join(..., 'anti')` (antiJoin), `topKWithIndex`, `groupByOperators.median`/`mode`. These are one-handle-row-each additions on the existing verb family — realize only if a consumer demands them; not a standalone stacking defect.
- d2ts `./electric` `electricStreamToD2Input`/`outputElectricMessages` — replication bridge, correctly fenced to the data-branch node altitude by the page boundary law. Leave.

### [A6] Fully-exploited catalogs (NO underutilization — do not churn)

`@opentelemetry/semantic-conventions` (`convention.md`: stable+incubating+value families all mined), `@effect/typeclass` (`merge.md`/`evidence.md`/`presence.md`: struct/min/max/optional/first/last/Monoid/Bounded all bound; the higher law-combinators are correctly testkit-side), `@bufbuild/protobuf` (`format.md`/`codec.md`: fromBinary/toBinary/isMessage/sizeDelimited*/createRegistry), `@msgpack/msgpack` (`format.md`: Decoder/Encoder/ExtensionCodec/decodeMultiStream + Hlc ext), `cbor-x` (`format.md`: Decoder/setSizeLimits/decodeMultiple + the augmentation), `rfc6902` (`format.md`: applyPatch/createPatch/createTests/isDestructive/VoidableDiff), `@connectrpc/connect(-web)` (`invoke.md`: Code/ConnectError/createClient/Interceptor/createConnectTransport/createGrpcWebTransport). VERIFY-TARGET (not read line-by-line): `contract.md`'s `DescriptorGate` should use protobuf-es `createFileRegistry(fileDescriptorSet)` reflection, not a hand diff — improver confirms against `bufbuild-protobuf.md`.

---

## [B] — NEVER-USED ADMITTED CAPABILITY THE FOLDER CONCEPT DEMANDS

### [B0] FLAGSHIP — statechart generalization of `state/machine.md` (ZERO new package)

Prefetch `[A3]` is a closed ruling: the current flat Mealy `Transition.Table` is the **degenerate case** — a depth-1 tree with a singleton configuration. The full statechart (task emphasis 4: "hierarchical/parallel regions, guarded transitions, timers, history") is **table algebra + already-admitted `@effect/experimental` Machine / fiber / `Schedule` primitives**, no external package (xstate is mine-design-only, admissions.md reject). The 13 capability rows and their encoding are enumerated in prefetch `[A3]`; the load-bearing ones for a rebuild:

- Config-as-state (active-leaf set, not one phase); static tree precompute at table construction (`documentOrder`/`entryOrder`/`exitOrder`/LCCA) — the row keys gain tree paths, row order becomes load-bearing (SCXML document-order determinism).
- Guarded transitions: pure row predicate `(extendedState, signal) => boolean` — preserves Mealy character + serializability.
- Internal-vs-external: one per-row domain flag (source vs LCCA).
- Entry/exit ordered-action emit: the `emit` channel generalizes from one verdict to an ordered action program; the Effect interpreter executes it.
- Eventless/macrostep: `drive`/`trace` become the macrostep fold (drain eventless+internal to stability per external signal, bounded-microstep fuel row) — rides `Stream.mapAccum` (`[A2]`).
- History (shallow/deep): `historyValue` lives INSIDE machine state → `Machine.snapshot`/`restore` carries it durably for free, exceeding xstate's default (prefetch `[A3]` item 8).
- `invoke`/`spawn`: child `Machine.boot` in `forkOne(id)`, exit-fold interrupt, `unsafeSend` completion self-signal (`[A1]` context members).

This is the single largest never-used-capability play in core and the direct realization of task emphasis (4). It re-homes durable-execution/replay/compensation to the runtime workflow altitude unchanged (the page's existing ALTITUDE RULING already draws that line).

### [B1] Machine inspection fact-stream (`state/machine.md`, admitted primitives)

The `Actor` Subscribable (`[A1]`) plus a microstep-fact `Stream` mirroring the `@xstate.actor`/`@xstate.event`/`@xstate.snapshot`/`@xstate.microstep` taxonomy (prefetch `[A3]` item 13) is the machine's telemetry HOOK — task emphasis (3): "telemetry and diagnostics grow consumer HOOKS a project plugs into." A consumer taps the fact stream; the machine forks nothing. Pure admitted `Subscribable`/`Stream`.

---

## [C] — CROSS-STACKING PLAYS (package × package the corpus never attempts)

### [C1] hash-wasm `IHasher` × `frame.md` reassembly — streaming verify

`ArtifactFrame._gathered` accumulates bands, `_joined` allocates the summed buffer, `Parity.verified` hashes the whole join (a second pass). STACK: `value/contentKey` `Digest` exposes a streaming verify (`createXXHash128(0,0)` memoized module-singleton → `IHasher.init().update(band)` fed per frame in `_gathered` → `digest()` compared at completion). Eliminates the post-join full re-hash on large multi-frame artifacts. Requires: one new streaming-verify member on the `Digest` owner (`contentKey.md`) that `frame.md` composes. Efficiency play; weigh against the current code's single-verify-site clarity (the page names `Parity.verified` as "the interchange's one content-mint delegation site" — the streaming member must preserve that single-site law).

### [C2] Machine `Actor` Subscribable × `state/fold` — machine-state-as-fold-input

`machine.md` and `fold.md` are fully decoupled today. A booted actor is a `Subscribable<State>`; `Stream.mapAccum` on its state changes (`[A2]`) yields an op stream a `Fold.Plan`/`Replay.memory` folds, and its `state` Subscribable binds a view. The seam "a machine drives a fold, a fold's change wave feeds a machine signal" is unattempted. Candidate only if a real consumer (ui/runtime) needs it — note as an available composition, not a mandated edit.

### [C3] `MsgPack.duplexSchema` × `ChannelSchema` fusion — `invoke.md` collapse

Already spelled in `[A3]`: `Gateway.duplex` collapses its two-stage `MsgPack.duplex()`+`ChannelSchema.duplexUnknown` to a single `MsgPack.duplexSchema({inputSchema: outcome, outputSchema: CommandPayload})` (and the ndjson arm to `Ndjson.duplexSchema`/`duplexSchemaString`). Fewer stages, same asymmetric-schema seam. This is a real collapse the doctrine's "use the ecosystem's native abstraction at its intended power" mandate favors.

### [C.OOS] Explicit out-of-scope (prevent the improver reaching for admitted branch packages)

`@effect/rpc` client-derivation mapped-type mirrors `invoke.md`'s `Capability.Sdk<T>` — but it is TS↔TS RPC, NOT the C#-minted connectrpc/protobuf-es seam core owns; connectrpc is correct here. `@effect/cluster`/`@effect/workflow` are the durable-execution/sharding altitude the machine ALTITUDE RULING already routes to the runtime branch. `@effect/sql*` (EventJournal/PersistedQueue durable backings), `@effect/ai*`, `@opentelemetry/sdk-*`+exporters are all later-wave/host-bound and forbidden by core's boundary law. None belong in core.

---

## [D] — GAP CAPABILITIES (package + integration shape; improver weighs)

### [D1] `@effect/printer` `Doc` for `board.md` `Query.render` — LOW confidence

`Query.render` hand-assembles PromQL via `_selector`/`_span`/`_render` string concatenation. `@effect/printer` (`Doc.text`/`Doc.encloseSep`/`Doc.group`, verified `Doc.d.ts:382/2538/1846`) is the ecosystem document-assembly primitive; `Doc.encloseSep` is exactly the `{selector,label,...}` list joiner. WEIGH: PromQL is a single-line dialect where the rendered string is byte-load-bearing (content-key-adjacent determinism, quoted UTF-8 selector contract); printer's layout/`group` reflow is a risk and buys little on a flat dialect. HONEST verdict: probably reject — the current fold is arguably the correct altitude — but the doctrine's "never hand-roll a lower-level reimplementation" mandate makes it a legitimate candidate the improver should consciously rule on, not skip.

### [D2] `@effect/experimental/VariantSchema` — LOW confidence, likely reject

`VariantSchema` declares one schema with named field-set variants (the engine behind `@effect/sql` `Model`). Core's wire twins (`FromBytes`/`FromWire` on `ReceiptEnvelope`/`CommandPayload`/etc.) are byte-**transforms**, not field-set variants, so VariantSchema is a poor structural fit for the dominant pattern. The only plausible site is a shape genuinely needing distinct decoded/snapshot/wire **field sets** (none clearly present). Listed for completeness because the task names experimental at operator depth; default verdict REJECT unless the improver finds a real field-set-variant shape.

---

## [E] — PER-PAGE INTEGRATION MAP

| [PAGE] | [ACTION] | [PLAY] | [CONFIDENCE] |
| :----- | :------- | :----- | :----------- |
| `state/machine.md` | Generalize `Transition` flat-Mealy → statechart (config/hierarchy/parallel/guards/internal-external/ordered-emit/macrostep/history/invoke/spawn) | `[B0]` prefetch `[A3]` | HIGH — flagship |
| `state/machine.md` | Surface `Actor.state: Subscribable<P>`; add `Transition.trace` (`Stream.mapAccum` driver); add inspection fact-stream; realize `forkOne`/`unsafeSend` for invoke/spawn; wire `withTracingEnabled`; keep `serializable.add`-dual note | `[A1]`,`[A2]`,`[B1]` | HIGH |
| `interchange/invoke.md` | Collapse `Gateway.duplex` to `MsgPack.duplexSchema`/`Ndjson.duplexSchema`(+`duplexSchemaString` text lane) | `[A3]`,`[C3]` | HIGH |
| `interchange/frame.md` | Streaming `IHasher` verify in `ArtifactFrame` reassembly (via a new `Digest` streaming member) | `[A4]`,`[C1]` | MED — weigh vs single-verify-site clarity |
| `interchange/frame.md` | `Transferable.schema` for GeometryFrame worker crossing | `[A3]` | LOW — likely DEFER to consumer marshal |
| `value/contentKey.md` | Expose a streaming-verify member on `Digest` enabling `[C1]` (preserve the single content-mint-site law) | `[A4]` | MED (paired with frame) |
| `observe/board.md` | Weigh `@effect/printer` `Doc` for `Query.render` | `[D1]` | LOW — conscious rule, likely reject |
| `state/fold.md` | Realize `filterBy`/anti-join/`topKWithIndex` verb rows only on consumer demand (already growth-flagged) | `[A5]` | LOW — demand-driven |
| `interchange/contract.md` | Verify `DescriptorGate` uses protobuf-es `createFileRegistry` reflection | `[A6]` | VERIFY |
| `observe/convention.md`, `observe/slo.md`, `state/evidence.md`, `state/feed.md`, `state/presence.md`, `state/merge.md`, `state/causal.md`, `state/commit.md`, `interchange/format.md`, `interchange/codec.md`, `value/schema.md`, `value/identity.md`, `value/clock.md`, `value/quantity.md`, `value/fault.md` | NO stacking gap — catalogs fully exploited; do not churn | `[A6]` | — |
