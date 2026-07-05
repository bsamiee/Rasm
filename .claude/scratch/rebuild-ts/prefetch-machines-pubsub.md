# [PREFETCH] — MACHINES / PUBSUB / STREAMING / TELEMETRY-HOOKS

Research dossier for the TS platform-library rebuild. All member spellings verified against published `.d.ts` / source / primary specs; UNVERIFIED items flagged inline. Baseline = the admitted pnpm catalog (`effect` 3.21.4, `@effect/platform` 0.96.2, `@effect/experimental` 0.60.0, `@effect/cluster` 0.59.0, `@effect/workflow` 0.18.2, `@nats-io/{nats-core,jetstream}` 3.4.0, OTel stable 2.8.0 / experimental 0.219.0, `@effect/opentelemetry` 0.63.0).

## [A]-[MACHINES]

### [A1] Statechart design space — the semantics worth encoding

Normative core: W3C SCXML REC (REC-scxml-20150901). Two layers — the normative set-theoretic definitions (§3.13) and the informative Appendix D imperative algorithm. Encode against the normative sets; the sets are fold-shaped (static-tree algebra), so a `Transition.Table` generalization honors them without transliterating pseudocode.

- Configuration model: live state is a configuration — an ordered set of active states (atomic leaves + all ancestors), never one phase. Compound `<state>` = OR (exactly one active child); `<parallel>` = AND (all children active). The current flat Mealy table is the degenerate case: depth-1 tree, singleton configuration. Static tree facts precompute at table construction: `documentOrder`, `entryOrder` (ancestors first), `exitOrder` (reverse document order), ancestor chains (`getProperAncestors`), `isDescendant`.
- Transition selection (§3.13 normative): *optimally enabled* = enabled by E in atomic state S, no earlier-document-order sibling transition enabled in the source, no descendant transition enabled (inner-first, child preempts parent). *Conflict* = non-null exit-set intersection (targetless transitions have empty exit sets, never conflict). *Priority* = descendant-source wins, else document order of the selecting atomic state. *Optimal transition set* = largest conflict-free set of optimally enabled transitions with no excluded higher-priority member; one microstep executes exactly one such set. Appendix D realization: `selectTransitions(event)` / `selectEventlessTransitions()` -> `removeConflictingTransitions`, conflict tested via `computeExitSet` intersection.
- Microstep/macrostep loop (run-to-completion): `microstep` = `exitStates` -> `executeTransitionContent` -> `enterStates`, lock-step. No named `macrostep()` exists — it is the inner `while not macrostepDone` loop in `mainEventLoop()`: drain eventless transitions; only when none, dequeue one internal event; macrostep ends when eventless selection empty AND internal queue empty. Eventless transitions preempt internal-queue drain. Then: fire deferred `<invoke>`s for states entered-and-kept (`statesToInvoke`, `entryOrder`); re-drain if invokes raised internal events; only then blocking-dequeue ONE external event (`externalQueue` blocking, `internalQueue` plain) — one macrostep per external event. Pre-selection: `applyFinalize` per matching `invokeid`, forward to every `autoforward` child. Termination law: a microstep always terminates, a macrostep may not — a production encoding adds a bounded-microstep fuel row (XState-class runtimes do the same).
- Exit/entry tree algebra: `getTransitionDomain(t)` — targetless -> null; `type="internal"` with compound source and all targets inside the source -> the source; else `findLCCA([source] ++ targets)`. THE ENTIRE internal-vs-external distinction is this one flag flipping domain from LCCA to source (shrinks exit/entry sets, skips the source's own exit/entry actions). `findLCCA` = least common COMPOUND ancestor (filtered to compound/`<scxml>`), guaranteed to exist, never a member of its input. `computeExitSet` = every configuration member descending from the domain. `computeEntrySet` = `addDescendantStatesToEnter` (dereferences history, fills compound defaults + parallel children) + `addAncestorStatesToEnter` (ancestors up to, excluding, the domain; expands uncovered parallel children). Ordering: exits innermost-first (`exitOrder`), entries outermost-first (`entryOrder`); `onexit`/`onentry` within a state run in document order. XState v5 matches action ordering but INVERTS the default: transitions internal by default, `reenter: true` opts into external semantics — a design decision to make explicitly.
- History: recorded in `exitStates` BEFORE `onexit` runs — deep = active atomic-descendant set of the exited compound; shallow = active immediate children; stored in `historyValue` keyed by history-state id. Re-entry through the history pseudostate restores the stored configuration; never-recorded falls to the history state's default transition (`defaultHistoryContent`). Load-bearing for this rebuild: history is pure data inside machine state — `Machine.snapshot`/`restore` carries it across process restarts for free, which XState only gets via a separate persisted-snapshot mechanism.
- Final states / done events / invoke: entering nested `<final>` enqueues `done.state.<parentId>` (with `<donedata>`) on the INTERNAL queue; parallel grandparent with every region `isInFinalState` additionally enqueues `done.state.<grandparentId>`; top-level final sets `running = false`. `isInFinalState`: compound = some child final active; parallel = every child final. `<invoke>`: child started at macrostep boundary for entered-and-kept states, cancelled on exit of the owning state, completion posts `done.invoke.<id>` to the parent's EXTERNAL queue, `autoforward` copies external events down, `<finalize>` massages child events pre-selection. XState v5 mirror: `invoke: { id, src, input, onDone, onError }`, entry-start/exit-stop, skips starting when entered-and-exited within one microstep chain.
- Determinism axis: Harel/STATEMATE contributes AND/OR decomposition, entry/exit actions, history, guards, and synchronous broadcast with genuine nondeterminism. SCXML deliberately removes both: broadcast replaced by the serialized internal queue (raised event visible NEXT microstep), nondeterminism removed by document-order priority + inner-first preemption (`<parallel>` MUST NOT introduce concurrency nondeterminism). Ecosystem split worth a policy row: SCXML orders parallel firing by document order; Sismic orders by decreasing source depth and flags nondeterminism; ConStaBL forbids conflicting transitions. Ruling: adopt SCXML document-order determinism — row order is load-bearing. STATEMATE step-vs-superstep timing specifics: UNVERIFIED (no primary source retrieved).
- Modern formal work (2023-2026, live-source): ICTAC 2023 Event-B formalization of SCXML RTC (verifies the external-consumed-only-when-internal-empty queue law); Context2Theory ABZ 2024 (Event-B Theory datatype encodings); ConStaBL arXiv 2307.03790v2 (concurrent statecharts + the ordering-split documentation, fuzzing); LLFSMs->TLA+ MODELSWARD 2025; Coq SOS + refinement for UML-Statecharts (FITEE 2024); Sandia Q framework (Coq->C via Frama-C/VST); Isabelle AFP hierarchical-automata formalization.

### [A2] Build-vs-admit verdict

| npm | stable | license | maintenance | verdict | justification |
| --- | --- | --- | --- | --- | --- |
| `xstate` | 5.32.4 (2026-07-02); v6 alphas daily (6.0.0-alpha.17) | MIT | very active — still landing parallel/history correctness fixes | mine-design-only | Own event loop, mutable-assign context, untyped error channel, JSON snapshots — irreconcilable with Schema-typed state, typed rails, fiber/Scope lifecycles; every needed semantic lands as (a)/(b) below; v5->v6 churn makes admission timing worst-case |
| `@effect/experimental` (Machine) | 0.60.0 | MIT | active on effect 3.x | admit-substrate (KEEP) | Still the current actor-primitive home for effect 3.x; all baseline spellings confirmed, one call-shape correction ([A4]); effect 4.0.0-beta.93 core exports NO machine/actor module — Machine's effect-4 home is a watch item, not a blocker |
| `@statelyai/inspect` | 0.7.2 (2026-06-14) | MIT | active | mine-design-only | The inspection taxonomy to mirror as a `Subscribable`/`Stream` fact plane (`inspector.actor/event/snapshot`); package couples to Stately tooling |
| `@xstate/inspect` | 0.8.0 (2023-03) | MIT | stale — superseded | reject | v4-era; replaced by `@statelyai/inspect` |
| `@xstate/graph` | 3.0.4 | MIT | maintained | reject | Path generation only over xstate machine objects; inadmissible without admitting `xstate` |

### [A3] Capability delta — flat Mealy table -> full statechart

Tags: (a) more-table = table-structure generalization over the current owner; (b) effect-primitive = already-admitted Machine/fiber/Schedule capability; (c) requires-xstate.

| # | Missing capability | Tag | Encoding |
| --- | --- | --- | --- |
| 1 | Hierarchical/compound states | (a) | Phase becomes a path in a static tree; document order = row order; ancestor chains, `entryOrder`/`exitOrder`, LCCA precompute at table construction |
| 2 | Parallel/orthogonal regions | (a) | State = configuration (active-leaf set); per-region selection + conflict removal via exit-set intersection; SCXML document-order determinism fixed as policy |
| 3 | Guarded transitions | (a) | Row predicate `(extendedState, signal) => boolean`, pure by law (SCXML `cond` side-effect-free; XState guards = named refs + `params`) — purity preserves Mealy character and serializability |
| 4 | Internal vs external transitions | (a) | One per-row flag selecting domain: source (internal) vs LCCA (external); XState v5 default is internal + `reenter: true` |
| 5 | Entry/exit action ordering | (a) | Emit channel generalizes from one verdict to an ordered action program: exits reverse-document-order, transition content, entries document-order — still pure Mealy output; the Effect interpreter executes it |
| 6 | Eventless/always transitions + internal queue | (a) | `drive` becomes the macrostep fold: drain eventless + internal (`raise`/done) to stability per external signal, bounded-microstep fuel row; expressible in `Stream.mapAccum` |
| 7 | Final states / onDone | (a) | `isInFinalState` fold + generated `done.state.<id>` internal signals into the macrostep accumulator |
| 8 | History (shallow/deep) | (a)+(b) | `historyValue` recorded in the exit fold, living inside machine state — `Machine.snapshot`/`restore` carries it durably for free, exceeding XState's default |
| 9 | Delayed/after transitions | (b) | Already proven by phase-keyed `watch` rows: `forkReplace` with state-scoped ids; `Schedule`/Clock/TestClock beats XState delays on testability |
| 10 | Actor invocation (`invoke`) | (b) | Child `Machine.boot` in a fiber via `fork`/`forkOne`/`forkReplace`, scoped to state entry, interrupted in the exit fold (= `cancelInvoke`); completion self-signals `done.invoke.<id>` via `unsafeSend`; autoforward = send-forwarding row; cross-process sharding stays at the ruled `@effect/workflow`+`@effect/cluster` altitude |
| 11 | Spawn (dynamic children) | (b) | Keyed `forkOne` registry (id-addressed fibers) IS `spawnChild`/`stopChild`; refs live in machine state |
| 12 | Broadcast | (a) | Reject Harel synchronous broadcast; adopt SCXML queued-internal-events — it is item 6's internal queue |
| 13 | Inspection protocol | (b), taxonomy mined | `Actor` already extends `Subscribable<State>`; add a microstep-fact stream mirroring `@xstate.actor` / `@xstate.event` / `@xstate.snapshot` / `@xstate.microstep` |

NOTHING lands in (c). XState-exclusive assets are cloud tooling (Stately editor/inspector) and `@xstate/graph` path generation — tooling, not runtime semantics. VERDICT: the statechart layer is a generalization of the existing `core/state/machine` owner — rows gain tree-path keys, guard predicates, a domain flag, ordered-action emit; the driver gains the macrostep fold; hierarchy/parallel/history/invoke reduce to table algebra + admitted Machine/fiber primitives. No external package is justified.

### [A4] Verified `@effect/experimental` 0.60.0 Machine spellings (+ corrections)

- Top-level: `make`, `makeWith`, `makeSerializable({ state, input? }, initialize)`, `boot`, `snapshot`, `restore`, `retry(schedule)`, `withTracingEnabled`, `currentTracingEnabled`; namespaces `Machine.procedures`, `Machine.serializable`.
- `Machine.serializable.make(initialState, options?: { identifier? })` confirmed. CORRECTION to the current design page: `Machine.serializable.add(RequestSchema, handler)` is a PIPEABLE DUAL — data-last `add(schema, handler)` returning `(self) => self'`, or data-first `add(self, schema, handler)`; `addPrivate` same dual shape. `Machine.procedures.add` is differently shaped: curried type-application `add<Req>()(...)` plus `addProcedure`/`addProcedurePrivate` — the two namespaces do not share a call shape.
- Handler context (`Procedure.Context`): `request`, `state`, `send` (Effect<void>, own-mailbox enqueue), `sendAwait`, `unsafeSend`, `unsafeSendAwait`, `fork`, `forkOne(id)`, `forkReplace(id)`, `forkWith`, `forkOneWith`, `forkReplaceWith` (the `*With` variants pair fork with a state update).
- Actor: `Actor<M> extends Subscribable<Machine.State<M>>` with `send(request) => Effect<Success, Error>`; `SerializableActor` adds `sendUnknown(unknown) => Effect<ExitEncoded, ParseError>`.
- XState v5 spellings (design reference, all verified): `setup({ types, actions, actors, guards, delays }).createMachine(...)`, `createActor(machine, { inspect })`, `guard: { type, params }`, `after: { <ms>: ... }`, `always: { guard, target }`, `reenter: true`, `{ type: 'history', history: 'deep' | 'shallow', target }`, `invoke: { id, src, input, onDone, onError }`, `spawnChild(logic, { id })`, `spawn` inside `assign`, `stopChild(id)`, `fromPromise`/`fromEventObservable`; inspection events `@xstate.actor` / `@xstate.event` / `@xstate.snapshot` / `@xstate.microstep`; `createBrowserInspector()` from `@statelyai/inspect`.

## [B]-[PUBSUB]

### [B1] Package verdicts — the `@nats-io` v3 family + browser lanes

Whole family lock-stepped at 3.4.0 stable (Apache-2.0, 2026-05-08); `3.4.1-0` is a `next` prerelease — do not admit.

| npm | stable | license | maintenance | verdict | justification |
| --- | --- | --- | --- | --- | --- |
| `@nats-io/nats-core` | 3.4.0 | Apache-2.0 | active | admit-substrate (KEEP) | Already admitted; `NatsConnection`/`wsconnect` transport core, current |
| `@nats-io/jetstream` | 3.4.0 | Apache-2.0 | active | admit-substrate (KEEP) | Already admitted; fanout/replay engine; hard-deps `nats-core@3.4.0` |
| `@nats-io/kv` | 3.4.0 | Apache-2.0 | active | admit-folder (ADD) | KV topology row: revision-CAS distributed state; SEPARATE package, not a jetstream sub-export; deps jetstream+nats-core (present) |
| `@nats-io/obj` | 3.4.0 | Apache-2.0 | active | admit-folder (ADD) | ObjectStore topology row: chunked large-binary; separate package; pulls `js-sha256@^0.11.1` |
| `@nats-io/services` | 3.4.0 | Apache-2.0 | active | reject (mine-design-only) | Request-reply microservice framework — RPC, not fanout; overlaps `@effect/rpc`/`@effect/cluster` |
| `@nats-io/transport-node` | 3.4.0 | Apache-2.0 | active | mine-design-only (growth) | TCP `connect()`; design rides runtime-neutral `wsconnect` — TCP is a latency-case manifest decision |
| `@nats-io/transport-deno` | not on npm (JSR only) | — | — | n/a | — |
| `@nats-io/msgpack` | does not exist | — | — | n/a | "msgpack" is an `@effect/cluster` serialization option, not a NATS package |
| `@nats-io/nkeys` / `@nats-io/nuid` | 2.0.3 / 3.0.0 | Apache-2.0 | — | transitive-only | Pulled by nats-core; never admitted directly |
| `nats` / `nats.ws` monoliths | 2.29.3 / 1.30.3 | Apache-2.0 | DEPRECATED | reject | Registry deprecation points to the scoped family |
| BroadcastChannel | platform, no package | — | Baseline widely-available 2022-03 | admit engine row | Browser cross-tab fanout transport |
| Web Locks (`navigator.locks`) | platform, no package | — | Baseline widely-available 2022-03 | admit coordination row | Cross-tab leader election / mutex — a SIBLING coordination port, not a `Fanout` row ([B5]) |

`kv`/`obj` ride the same `wsconnect` connection the jetstream engine holds behind the interior `Nats` Tag — one connection capability fans into fanout + revision-state + blob-store.

### [B2] JetStream API-correctness — confirmations + design defects

Confirmed correct (source-verified against `nats-io/nats.js@main`): `wsconnect`, `NatsConnection`; `jetstream(nc)`/`jetstreamManager(nc)`; `jsm.streams.add({ name, subjects, max_age, duplicate_window })`; `js.publish(subject, payload?, { msgID })`; `PubAck.seq/.duplicate/.stream`; `expect: { lastMsgID, lastSequence, lastSubjectSequence, streamName }`; `msg.ack(): void`, `msg.ackAck(opts?: Partial<{ timeout: number }>): Promise<boolean>`, `msg.nak(millis?): void`, `msg.term(reason?): void`, `msg.working(): void`; `DeliverPolicy.StartSequence`/`StartTime`; `opt_start_seq`/`opt_start_time`; `js.consumers.get(stream, nameOrOptions?)`; `consumer.consume()/fetch()/next()`; `ConsumerMessages.close()`; `msg.headers?.get("Nats-Msg-Id")`.

DEFECTS in the current `runtime/.planning/net/pubsub.md`:

1. HEADLINE: the at-least-once `consume` lane rides an ordered consumer, which CANNOT ack. `js.consumers.get(topic, optionsObject)` mints a nameless ordered consumer with hardcoded `ack_policy: AckPolicy.None` (`jetstream/src/consumer.ts:1019`). Every `ack()`/`nak()`/`ackAck()` in the consume lane is a no-op: no redelivery, no double-ack, no at-least-once. Fix: durable consumer — `jsm.consumers.add(stream, { durable_name, ack_policy: AckPolicy.Explicit, ack_wait, max_deliver, deliver_policy, opt_start_seq/opt_start_time })` then `js.consumers.get(stream, durable_name)`. The `drained()` kernel must split: ordered (subscribe/replay, no ack) vs durable-explicit (consume, real ack algebra).
2. `AckPolicy`/`ReplayPolicy` absent from design + `.api` catalog. `AckPolicy` = `None:"none"`, `All:"all"`, `Explicit:"explicit"`, `FlowControl:"flow_control"` (server 2.14+), `NotSet:""`; `ReplayPolicy` = `Instant:"instant"`, `Original:"original"`. Both are `const` objects, not TS enums.
3. `.api` catalog `DeliverPolicy` row missing `LastPerSubject` (`"last_per_subject"`); full set `All/Last/New/StartSequence/StartTime/LastPerSubject`.
4. `subscribe` Window semantics wrong: `_start(Window)` returning `{}` defaults the ordered consumer to delivery-from-stream-start, not the "new deliveries" the PORT_SHAPE law claims. Fix: `{ deliver_policy: DeliverPolicy.New }` for the Window anchor of live fanout.
5. Real-but-unused growth rows: `working()` (ack-wait heartbeat for long handlers), `term(reason?)` for poison (design only naks), `next()`/`fetch()` bounded pulls. `OrderedConsumerOptions` = `{ name_prefix, filter_subjects, deliver_policy, opt_start_seq, opt_start_time, replay_policy, inactive_threshold, headers_only }`.

### [B3] `@nats-io/kv` capability row (verified spellings)

`Kvm` (`new Kvm(nc)`): `create(name, opts?)`, `open(name, opts?)`, `list()`. `KV`: `create(k, data, markerTTL?): Promise<number>` (create-if-absent); `put(k, data, opts?): Promise<number>`; `update(k, data, version: number, timeout?): Promise<number>` — the revision-CAS OCC member; `get(k, opts?: { revision: number }): Promise<KvEntry | null>`; `delete(k, opts?)`; `purge(k, opts?)`; `watch(opts?)`; `keys(filter?): Promise<QueuedIterator<string>>`; `history(opts)`; `status()`; `destroy()`. `KvEntry` = `{ bucket, key, value: Uint8Array, created, revision, delta?, operation: "PUT"|"DEL"|"PURGE" }` + `.string()`/`.json()`. No entry-level `open(key)` — retrieval is `get`.

### [B4] `@nats-io/obj` capability row (verified spellings)

`Objm` (`new Objm(nc)`): `create/open/list`. `ObjectStore`: `put(meta, rs: ReadableStream<Uint8Array>): Promise<ObjectInfo>` (chunked, `meta.options.max_chunk_size`); `putBlob(meta, data: Uint8Array)`; `get(name): Promise<ObjectResult | null>` (`.data: ReadableStream`, `.error: Promise`); `getBlob(name): Promise<Uint8Array | null>`; `info(name)`; `list()`; `delete(name): Promise<PurgeResponse>`; `link(name, meta: ObjectInfo)`; `linkStore(name, bucket: ObjectStore)`; `watch(opts?)`; `seal()`; `status(opts?)`; `destroy()`. Digests via `js-sha256`. Distinct engine from the iac S3-style object store — an engine row, not a replacement.

### [B5] Browser lanes

- BroadcastChannel — WHATWG HTML; Baseline widely-available since 2022-03. NOT a Node global: exported only from `node:worker_threads`, same-process cross-thread only, never cross-OS-process. `new BroadcastChannel(name)`, `.postMessage(msg)`, `.close()`, `.name`, `message`/`messageerror` events; Node adds `.ref()`/`.unref()`; structured clone; sender excluded; same-origin. Role: browser cross-tab fanout engine row (best-effort, no ack — a degradation-table row like the local engine).
- Web Locks — W3C, secure-context; Baseline widely-available since 2022-03. In Node since v24.5.0: `locks` (`LockManager`) from `node:worker_threads` (cross-thread, not a global). `navigator.locks.request(name, callback)` / `request(name, { mode: 'exclusive'|'shared', ifAvailable, steal, signal }, callback)`; `navigator.locks.query(): Promise<{ held, pending }>` of `LockInfo{ name, mode, clientId }`; `Lock{ name, mode }`. Held-forever exclusive lock = the MDN-documented leader-election pattern; browser auto-releases on tab close and grants to the next queued. UNVERIFIED: the WebIDL name `LockManagerSnapshot` (spec-only; use `held`/`pending`/`LockInfo`).
- Scope ruling: BroadcastChannel maps cleanly to a browser `Fanout` engine row. Web Locks is a DIFFERENT concern — leader election/mutex — modeled as a sibling coordination port with a Web-Locks row (browser) and a KV `update` revision-CAS row (NATS), never forced into `Fanout`.

### [B6] One-owner-many-topologies precedent

Engine-as-`Layer`-row-behind-one-`Context.Tag` is the canonical Effect idiom:
- `@effect/cluster` 0.59.0 (strongest precedent): `MessageStorage` Tag (`SqlMessageStorage.layer` vs memory/noop), `Runners` Tag (noop vs `SocketRunner`/`HttpRunner`), `RunnerStorage` (`layerMemory` vs SQL), `RunnerHealth` (`layerNoop`/`layerK8s`), serialization as policy row (`"msgpack" | "ndjson"`), root selection via `SingleRunner.layer({ runnerStorage: "memory"|"sql" })`. STALE-CLAIM CORRECTION: `ShardManager` was removed in Effect 3.19 — runner state lives in `RunnerStorage`; experimental `HashRing` handles key distribution.
- `@effect/experimental` 0.60.0: `Persistence`/`BackingPersistence` Tag (memory | LMDB | Redis) is the cleanest single-Tag/many-engine example; `EventJournal`/`EventLog` are append/replay, not pub/sub; NO dedicated fanout port module exists — a multi-engine fanout port is bespoke but squarely on precedent.
- Naming to mirror: `KeyValueStore.layerMemory/layerFileSystem`, `@effect/sql` per-driver layers — prefer `layer<Engine>` naming for engine rows.

## [C]-[STREAMING]

### [C1] `Sse.Retry` / `Sse.Event` resolution (closes the open repo research item)

Verified against published `@effect/experimental@0.60.0` `dist/dts/Sse.d.ts` and `main`:
- `Sse.Event` — interface, exactly: `_tag: "Event"`, `event: string`, `id: string | undefined`, `data: string`. The event-name field is `event` (not `type`/`name`).
- `Sse.Retry` — `class Retry extends Data.TaggedClass("Retry")<{ readonly duration: Duration.Duration; readonly lastEventId: string | undefined }>`.
- The current design's premise is WRONG: no millisecond field exists (`retry`/`millis`/`delay` do not exist). The interval arrives as `duration: Duration.Duration` — the `_hinted` projection collapses to reading `frame.duration` directly, no conversion. The encoder serializes back via `Duration.toMillis(event.duration)`. `Retry.lastEventId` is currently ignored by the design and can thread into the cursor fold. `makeChannel<IE, Done>(options?: { bufferSize?: number })` and `encoder: Encoder` confirmed.

### [C2] Bidirectional transport — verified `Socket`/`Channel` surface (`@effect/platform` 0.96.2)

- `Socket` interface: `run(handler: (_: Uint8Array) => Effect | void, { onOpen? })`, `runRaw(handler: (_: string | Uint8Array) => ...)`, `writer: Effect<(chunk: Uint8Array | string | CloseEvent) => Effect<void, SocketError>, never, Scope>`.
- `makeWebSocket(url: string | Effect<string>, options?: { closeCodeIsError?, openTimeout?: DurationInput, protocols? }): Effect<Socket, never, WebSocketConstructor>`. `WebSocketConstructor` Tag = `(url, protocols?) => globalThis.WebSocket`; platform-node/-bun/-browser provide the constructor layer.
- `toChannelWith<IE = never>()(self: Socket): Channel<Chunk<Uint8Array>, Chunk<Uint8Array | string | CloseEvent>, SocketError | IE, IE, void, unknown>`. Also: `fromWebSocket`, `toChannel`, `toChannelString`, `toChannelMap`, `makeChannel`, `fromTransformStream`, `defaultCloseCodeIsError`, `CloseEvent` class. `SocketError = SocketGenericError("Write"|"Read"|"Open"|"OpenTimeout") | SocketCloseError("Close")`. `InputError` is NOT exported from `Socket`.
- Serving side EXISTS: `SocketServer` module — `SocketServer` Tag `{ address, run: (handler: (socket: Socket.Socket) => Effect<_,E,R>) => Effect<never, SocketServerError, R> }`, `Address = UnixAddress | TcpAddress`, `SocketServerError` reason `"Open"|"Unknown"`; impls `NodeSocketServer` (platform-node), Bun in platform-bun.
- Frame codecs: `Ndjson.duplexSchema<IA,II,IR,OA,OI,OR>(options: Partial<NdjsonOptions> & { inputSchema; outputSchema })` returns a Channel transformer, `NdjsonError` reason `"Pack"|"Unpack"`; `MsgPack.duplexSchema` is the dual-overload `(self, options)` / `(options) => (self)` form producing `Channel<Chunk<OA>, Chunk<IA>, MsgPackError | ParseError | OutErr, InErr, ...>`; plus `MsgPack.schema(...)`.
- Duplex multiplexing: sandwich the codec transformer around `Socket.toChannelWith<E>()(socket)` to lift the byte duplex into a typed message duplex; `Stream.pipeThroughChannel` drives the outbound request Stream through and yields the inbound Stream. Request/response multiplexing rides ONE physical socket: correlation is a schema concern — tag frames with a request id in `inputSchema`/`outputSchema`, demux the single inbound Channel by id (`Channel.mergeWith` / fiber-keyed `Ref` map). The transport stays one duplex Channel; multiplexing is a router above the codec.

### [C3] WebTransport verdict — DEFER

| Surface | Status 2026-07 |
| --- | --- |
| Baseline | Newly Available since 2026-03-24, in Interop 2026; Widely Available projected 2028-09 |
| Chrome/Edge | Shipped (97+/98+) |
| Firefox | Shipped 114+ |
| Safari desktop + iOS | Shipped 26.4 — the historic holdout is closed |
| Model | HTTP/3 over QUIC; reliable bidi/uni streams + unreliable datagrams; `sendOrder`, `serverCertificateHashes`, some BYOB still baseline-false |
| Node server | NO native core API; `node:quic` experimental/flag-gated; only `@fails-components/webtransport` (libquiche native addon) |
| Bun server | No native server |
| Effect binding | None exists, none planned — `@effect/platform` 0.96.2 dts has zero `WebTransport` references; `Socket` is WebSocket-only |

RULING: defer the dedicated lane. The browser gap closed; the decisive gap moved server-side — no native Node/Bun server, no Effect binding, and the sole server path is a native addon its own author calls duct-tape, W3C-flagged not-production-ready, with Node 24 build fragility. Design consequence: keep codec + Channel planes transport-agnostic (already true — `Socket.toChannelWith` is one boundary adapter producing a byte duplex Channel); WebTransport later slots in as a sibling boundary adapter (bidi stream -> `Channel<Chunk<Uint8Array>, Chunk<Uint8Array>>`, datagrams -> unreliable Stream) with zero churn above. Mine `@fails-components/webtransport` for the stream/datagram shape only.

### [C4] WHATWG streams — Node 24+/26

All Stable (index 2) globals since Node 18, no import (API non-experimental as of v21): `ReadableStream`, `WritableStream`, `TransformStream`, `ByteLengthQueuingStrategy`, `CountQueuingStrategy`, `ReadableStreamBYOBReader` (+ `ReadableStreamBYOBRequest`, `ReadableStreamDefaultReader`), `TextEncoderStream`, `TextDecoderStream`, `CompressionStream`, `DecompressionStream`; BYOB / `type: "bytes"` supported. Node<->Web bridges on `node:stream`: `Readable.toWeb`/`fromWeb`, `Writable.toWeb`/`fromWeb`, `Duplex.toWeb` (-> TransformStream) / `Duplex.fromWeb`.

Effect bridges live in CORE `effect` `Stream` (not `@effect/platform`): `Stream.fromReadableStream(evaluate | { evaluate, onError })`, `Stream.fromReadableStreamByob({ evaluate, onError, bufferSize? })` (zero-copy byte ingest), `Stream.toReadableStream(options?: { strategy? })`, `Stream.toReadableStreamEffect(...)`, `Stream.toReadableStreamRuntime(runtime, ...)` (non-default runtime at the browser seam). Browser-seam law: pipe the typed duplex out through a Stream, `toReadableStream`/`toReadableStreamRuntime` to hand a native ReadableStream to Web consumers; ingest with `fromReadableStreamByob`.

### [C5] CDC-wasm delta

`fastcdc-rs` (crate `fastcdc`) now 4.0.1 (2026-04, MIT, Rust 2024 edition; `v2020` module = 64-bit, fastest, recommended), up from 3.2.1. No maintained wasm/npm build exists — wasm packaging remains DIY (`wasm-pack build` the v2020 chunker). No 2026 CDC contender displaces FastCDC.

| npm / crate | version | license | maintenance | verdict | justification |
| --- | --- | --- | --- | --- | --- |
| `@fails-components/webtransport` | 1.6.4 (2026-06) | BSD-3-Clause | active but self-described duct-tape, W3C-flagged not-production-ready, Node 24 build fragility | mine-design-only (defer) | Sole Node WT server; native libquiche addon, not Effect-shaped — study stream/datagram shape only |
| `fastcdc` crate (fastcdc-rs) | 4.0.1 | MIT | active | admit-design-only | Reference CDC; compile v2020 to wasm via wasm-pack — no maintained npm to admit |
| `@dstanesc/wasm-chunking-fastcdc-node` | 0.1.1 (2022-09) | MIT OR Apache-2.0 | abandoned | reject | Stale wrapper over old fastcdc-rs |
| `fastcdc` npm (N-API) | 1.0.1 (2021-10) | ISC | stale | reject | Native addon, not wasm, unmaintained |

## [D]-[TELEMETRY_HOOKS]

### [D1] Version deltas (repo -> current; all peer-compatible with `@effect/opentelemetry` 0.63.0)

- Stable SDK 2.8.0 -> 2.9.0 (`sdk-trace-base`, `sdk-metrics`, `sdk-trace-node`, `sdk-trace-web`, `resources`, `core`). Experimental 0.219.0 -> 0.220.0 (`sdk-logs`, `exporter-*-otlp-http`). `semantic-conventions` 1.41.1 current; `@effect/opentelemetry` 0.63.0 current.
- Structural delta in 2.9.0: the tracing SDK moved to a new `@opentelemetry/sdk-trace` package; `sdk-trace-base` 2.9.0 is a thin re-export shim. Keep importing from `sdk-trace-base`; do not add `sdk-trace` directly.

### [D2] Hook-surface catalog (verified spellings; corrections to the current design flagged)

TRACE (`sdk-trace-base` 2.9.0):
- `SpanProcessor`: `onStart(span: Span, parentContext: Context)`, `onEnding?(span: Span)`, `onEnd(span: ReadableSpan)`, `forceFlush(): Promise<void>`, `shutdown(): Promise<void>`. `onEnding` is present but JSDoc-flagged `@experimental` — the `Redaction.processor` scrub-in-`onEnding` design is correct (`onEnding` hands a mutable `Span`; `onEnd` only `ReadableSpan`) but rides an experimental method; record that on the lane card.
- Multiple processors: `TracerProviderOptions.spanProcessors?: SpanProcessor[]` — fan-out in array order. `MultiSpanProcessor` is internal/unexported. The imperative `provider.addSpanProcessor()` is removed in 2.x.
- `Sampler` (lives in the trace SDK, NOT `@opentelemetry/api`): `shouldSample(context, traceId, spanName, spanKind, attributes: Attributes, links: Link[]): SamplingResult` + `toString()`. CORRECTION: the param type is `Attributes`, not `SpanAttributes`. `SamplingResult = { decision: SamplingDecision; attributes?: Readonly<Attributes>; traceState?: TraceState }`; `SamplingDecision` = `NOT_RECORD=0, RECORD=1, RECORD_AND_SAMPLED=2`. Samplers: `ParentBasedSampler({ root, remoteParentSampled?, remoteParentNotSampled?, localParentSampled?, localParentNotSampled? })`, `TraceIdRatioBasedSampler(ratio)`, `AlwaysOnSampler`, `AlwaysOffSampler`, new `createAlwaysRecordSampler(delegate)`.
- `SpanExporter`: `export(spans: ReadableSpan[], resultCallback: (result: ExportResult) => void)`, `shutdown()`, `forceFlush?()`. Provider rename: `TracerProvider` (new) / `BasicTracerProvider` (shim).

METRICS (`sdk-metrics` 2.9.0 — fully declarative now; biggest corrections):
- `MeterProviderOptions`: `resource?`, `views?: ViewOptions[]`, `readers?: IMetricReader[]`.
- Views are `ViewOptions` OBJECT LITERALS (the `View` class exists but the provider takes option objects). Stream fields: `name?`, `description?`, `attributesProcessors?: IAttributesProcessor[]`, `aggregation?: AggregationOption`, `aggregationCardinalityLimit?`. Selection fields: `instrumentType?`, `instrumentName?` (wildcard `*`), `instrumentUnit?`, `meterName?`, `meterVersion?`, `meterSchemaUrl?`.
- Aggregation is declarative: `AggregationType` enum (`DEFAULT=0, DROP=1, SUM=2, LAST_VALUE=3, EXPLICIT_BUCKET_HISTOGRAM=4, EXPONENTIAL_HISTOGRAM=5`) + `AggregationOption` union, e.g. `{ type: AggregationType.EXPLICIT_BUCKET_HISTOGRAM, options?: { boundaries, recordMinMax? } }`. The old `Aggregation`/`ExplicitBucketHistogramAggregation` classes are internal/deprecated.
- Attribute filtering: `IAttributesProcessor { process(incoming: Attributes, context?: Context): Attributes }`; factories `createAllowListAttributesProcessor(keys: string[])` / `createDenyListAttributesProcessor(keys: string[])` (JSDoc examples showing `createAllowListProcessor` are wrong).
- `MetricReader` abstract (custom = extend + `onShutdown()`/`onForceFlush()`/`onInitialized()?`); `IMetricReader`: `setMetricProducer`, `selectAggregation`, `selectAggregationTemporality`, `selectCardinalityLimit`, `collect`, `shutdown`, `forceFlush`. `PeriodicExportingMetricReaderOptions`: `exporter`, `exportIntervalMillis?`, `exportTimeoutMillis?`, `metricProducers?`, `cardinalityLimits?: { counter?; gauge?; histogram?; upDownCounter?; observableCounter?; observableGauge?; observableUpDownCounter?; default? }`, `maxExportBatchSize?` (exp, new). `CardinalitySelector = (instrumentType: InstrumentType) => number`.
- `ExemplarFilter` is NOT a public hook — filters exist internally, unexported, not configurable via `MeterProviderOptions` in 2.9.0; treat as unavailable.
- `MetricProducer` (`collect(options?): Promise<CollectionResult>`) IS public — the contribute-external-aggregated-data hook, fed to a reader via `metricProducers`.

LOGS (`sdk-logs` 0.220.0):
- `LogRecordProcessor`: `onEmit(logRecord: SdkLogRecord, context?: Context)`, `forceFlush()`, `shutdown()`, new `enabled?(options: { context; instrumentationScope; severityNumber?; eventName? }): boolean`. Record type renamed to `SdkLogRecord`.
- Multiple processors: `LoggerProviderOptions.processors?: LogRecordProcessor[]`, plus `loggerConfigurator`. `LogRecordExporter`: `export(logs: ReadableLogRecord[], resultCallback)`, `shutdown()`, `forceFlush()`. `BatchLogRecordProcessor`, `SimpleLogRecordProcessor` present. New declarative per-logger config (logs analog of Views): `LoggerConfig { disabled?; minimumSeverity?: SeverityNumber; traceBased? }`, `LoggerConfigurator`, `createLoggerConfigurator`, `LoggerPattern`.

RESOURCE (`resources` 2.9.0 — API CHANGED, major delta from the current design's usage):
- `ResourceDetector`: `detect(config?: ResourceDetectionConfig): DetectedResource` — SYNCHRONOUS return of `DetectedResource = { attributes?: DetectedResourceAttributes }` where `DetectedResourceAttributes = Record<string, MaybePromise<AttributeValue | undefined>>`. Async is per-attribute promises, not `Promise<Resource>`; a custom detector is `{ detect: () => ({ attributes: { key: valueOrPromise } }) }`.
- `detectResources(config?): Resource` returns synchronously; async attrs settle via `resource.waitForAsyncAttributes()`. `Resource` is not user-implementable — mint via `resourceFromAttributes(attributes, options?)` (replaces `new Resource(...)`), `emptyResource()`, `defaultResource()`, `detectResources()`. Members: `attributes`, `schemaUrl?`, `asyncAttributesPending?`, `waitForAsyncAttributes?()`, `merge(other)`, `getRawAttributes()`.
- Built-in detectors confirmed (current design is correct): `envDetector`, `hostDetector`, `osDetector`, `processDetector`, `serviceInstanceIdDetector`; also `defaultServiceName`.

### [D3] `@effect/opentelemetry` 0.63.0 — what reaches through

`NodeSdk.Configuration` / `WebSdk.Configuration` (identical shape; `WebSdk.resource` required):
- `spanProcessor?: SpanProcessor | ReadonlyArray<SpanProcessor>` — ARRAY reaches through.
- `tracerConfig?: Omit<TracerConfig, "resource">` — carries `sampler?`, `spanLimits?`, `generalLimits?`, `idGenerator?`, `spanProcessors?`, `forceFlushTimeoutMillis?`. Sampler reaches through.
- `metricReader?: MetricReader | ReadonlyArray<MetricReader>` — array reaches through.
- `logRecordProcessor?: LogRecordProcessor | ReadonlyArray<LogRecordProcessor>` — array reaches through.
- `loggerProviderConfig?: Omit<LoggerProviderConfig, "resource">`; `resource?: { serviceName; serviceVersion?; attributes? }`; `shutdownTimeout?` (NodeSdk only).
- `layer` accepts `LazyArg<Configuration>` OR `Effect<Configuration, E, R>` — the config can be COMPUTED at Layer build (load-bearing for the registry pattern in [D4]). Lower-level: `NodeSdk.layerTracerProvider(processor | processors[], config?)`, `NodeSdk.layerEmpty` (new). Injection Tags: `Tracer.OtelTracerProvider`, `Resource.Resource`, `Metrics.makeProducer` / `Metrics.registerProducer(producer, () => reader|readers[])`, `Metrics.layer(() => reader|readers[], { shutdownTimeout? })`, `Resource.layer({ serviceName, ... })`, `Resource.layerFromEnv(additionalAttributes?)`, `Resource.layerEmpty`. `Tracer.makeExternalSpan` / `Tracer.withSpanContext` confirmed.

FORK-PRESSURE POINTS (do NOT reach through the facade):
1. Metric `views` — unreachable. No `views` slot on `Configuration`; `Metrics.layer`/`MeterProviderOptions.views` never surfaced. Per-tenant metric cardinality via Views forces a hand-built `MeterProvider({ views, readers })` bridged in through the Tags. Biggest fork-pressure point.
2. Custom `ResourceDetector[]` — not injectable via `NodeSdk.layer.resource` (serviceName/version/attributes only). Escape hatch: `Layer.succeed(Resource.Resource, detectResources({ detectors: [...] }))` or `Resource.layerFromEnv`.
3. `Otlp` lane opaque — `Otlp.layer/layerJson/layerProtobuf({ baseUrl, resource?, headers?, maxBatchSize?, replaceLogger?, tracerContext?, loggerExportInterval?, loggerExcludeLogSpans?, metricsExportInterval?, tracerExportInterval?, shutdownTimeout? })` — no processor/sampler/reader/views/log-processor slot; `tracerContext` is a context carrier, not an attribute hook. Confirms the design's `[R3]` note: boundary span scrub requires an SDK lane.

### [D4] Library-hook pattern (the shape the rebuild exposes)

OTel library law: a library uses only `@opentelemetry/api`, emits nothing until an app installs the SDK, never registers a global provider/processor; multiple SDK providers per process are warned against (context/propagator leakage). `@effect/opentelemetry` ships NO pipeline-registry Tag — the platform library defines it:
- One `Context.Tag` pipeline registry carrying accumulating collections: `Ref<Chunk<SpanProcessor>>` plus parallel `MetricReader[]`, `LogRecordProcessor[]`, `ViewOptions[]`, `ResourceDetector[]`.
- Each app/feature contributes via a `Layer` (`Layer.effectDiscard`) that APPENDS to the registry — order-independent merges, zero global effects. Contributor layers only add; they never call `NodeSdk.layer` themselves.
- Exactly ONE SDK-composition root reads the accumulated registry inside the `NodeSdk.layer(Effect.gen(...))` config Effect (the facade accepts `Effect<Configuration>`), draining into `spanProcessor: [...]`, `metricReader: [...]`, `logRecordProcessor: [...]`. For the unreachable pieces (Views, detectors) the root bypasses the facade: hand-built `MeterProvider({ views, readers })` and direct `Tracer.OtelTracerProvider` / `Resource.Resource` Tag provision. Contribute-then-collect, never fork.

### [D5] Per-tenant isolation (many apps, one telemetry plane)

- One provider per process, tenant as data — provider-per-tenant is discouraged (global-state leakage).
- Resource scoping (per app instance): `resourceFromAttributes({ [ATTR_SERVICE_NAME], [ATTR_SERVICE_NAMESPACE], [ATTR_SERVICE_INSTANCE_ID], [ATTR_SERVICE_VERSION] })`; `service.namespace` = tenant/team group; the `(namespace, name, instance.id)` triplet is globally unique. Fits per-tenant PROCESSES, not many tenants in one process.
- Per-request tenant tagging (many tenants, one process): a `SpanProcessor.onStart` baggage->`tenant.id`-attribute bridge — the same processor slot the design already owns.
- Metric cardinality budget (core lever): per-view `ViewOptions.aggregationCardinalityLimit` (default 2000) + `attributesProcessors: [createAllowListAttributesProcessor([...])]` (allow-list primary, limit as circuit-breaker); per-reader `cardinalityLimits` (per-instrument-type) or `cardinalitySelector` — set the reader breaker ABOVE the max per-view limit. Overflow aggregates under a single overflow data point (bounded memory under tenant explosion/DoS).
- Trace partitioning: per-tenant `Sampler` (`ParentBasedSampler({ root: TraceIdRatioBasedSampler(perTenantRatio) })`) or a routing `SpanProcessor` fanning to tenant exporters — both ride slots that reach through the facade.
- Caveat: Views-based cardinality does not reach through the facade (fork-pressure #1) — per-tenant metric cardinality forces the SDK lane.

### [D6] Package verdicts

| npm | stable | license | maintenance | verdict | justification |
| --- | --- | --- | --- | --- | --- |
| `@opentelemetry/api-logs` | 0.220.0 | Apache-2.0 | active (core) | admit-substrate | API tier for the logs signal (`SdkLogRecord`, `SeverityNumber`); likely transitively present — make explicit |
| `@opentelemetry/exporter-logs-otlp-http` | 0.220.0 | Apache-2.0 | active | admit-substrate | `OTLPLogExporter` — the OTLP-HTTP log egress the design lacks (trace+metric only today); feeds `BatchLogRecordProcessor` on the SDK lane |
| `@opentelemetry/sdk-trace` | 2.9.0 | Apache-2.0 | active | mine-design-only (do NOT add) | New tracing-SDK home; `sdk-trace-base` 2.9.0 re-exports it — adding it directly duplicates the surface |
| `@opentelemetry/sdk-node` | 0.220.0 | Apache-2.0 | active | reject | All-in-one bootstrap; `NodeSdk.layer` already owns Effect-native composition — admitting it forks the root |
| `@opentelemetry/host-metrics` | 0.39.0 | Apache-2.0 | active (contrib) | admit-folder (Node host only) | `HostMetrics({ meterProvider })` CPU/mem/process gauges; pulls `systeminformation` — never in web |
| `@opentelemetry/resource-detector-container` | 0.8.11 | Apache-2.0 | active (0.x) | mine-design-only | Trivial to author as a custom sync `detect(): DetectedResource` instead |
| `@opentelemetry/resource-detector-aws` | 2.20.0 | Apache-2.0 | active (contrib) | mine-design-only / admit-folder (AWS host only) | Only if a consuming app runs on AWS; not substrate |
| `@opentelemetry/exporter-prometheus` | 0.220.0 | Apache-2.0 | active | reject (mine-design-only) | Pull `MetricReader`; OTLP push already covers egress — admit only on a hard scrape-endpoint requirement |

## [E]-[MANIFEST_AND_PAGE_ACTIONS] (consolidated)

- `pnpm-workspace.yaml` catalog: ADD `@nats-io/kv: 3.4.0`, `@nats-io/obj: 3.4.0`, `@opentelemetry/api-logs: 0.220.0`, `@opentelemetry/exporter-logs-otlp-http: 0.220.0`; BUMP OTel stable 2.8.0 -> 2.9.0 and experimental 0.219.0 -> 0.220.0 as one wave; `@opentelemetry/host-metrics: 0.39.0` optional Node-host folder admission.
- `runtime/.planning/net/pubsub.md`: fix the ordered-consumer/ack no-op defect (durable `AckPolicy.Explicit` consumer for the consume lane), the Window-anchor `DeliverPolicy.New` default, and add `AckPolicy`/`ReplayPolicy`/`term`/`working` rows; `runtime/.api/nats-io-jetstream.md`: add `LastPerSubject`, `AckPolicy`, `ReplayPolicy`, the ordered-cannot-ack law.
- `runtime/.planning/net/channel.md`: replace the `_hinted` UNVERIFIED note — `Sse.Retry.duration: Duration.Duration` read directly; thread `Retry.lastEventId` into the cursor fold.
- `core/.planning/state/machine.md`: correct `Machine.serializable.add` call-shape note; the statechart generalization is table algebra + admitted primitives, no package admission.
- `runtime/.planning/otel/emit.md`: `resources` 2.9.0 detector API change (`DetectedResource`, `resourceFromAttributes`), `ViewOptions`/`AggregationType` declarative metrics shape, `onEnding` experimental flag on the redaction lane card, and the [D4] registry hook as the consumer-extension surface.
