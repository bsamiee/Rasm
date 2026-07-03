# [TS_BOUNDARIES]

The edge is the only place the system meets material it did not mint: wire payloads, foreign codecs, platform capability, sibling threads and peers. This page fixes where each crossing converts and who owns it — admission placement, contracts as data, codec engines, runtime selection, and marshal — so that no module past a seam names a provider shape, re-validates an admitted value, or resolves a runtime. Every seam is a declaration site: what crosses, in which direction, with which fault, is recoverable from the owner alone.

## [01]-[EDGE_CHOOSER]

This table selects the owner for a foreign signal; when a signal matches several rows, the most specific wins, and identity rows are read before transport rows.

| [INDEX] | [FOREIGN_SIGNAL]                     | [SEAM_OWNER]                                | [INTERIOR_FORM]                          | [REJECT]                          |
| :-----: | :----------------------------------- | :------------------------------------------ | :--------------------------------------- | :-------------------------------- |
|  [01]   | raw payload — body, file, message    | Schema owner at first sight                 | decoded value on the rail                | interior re-validation            |
|  [02]   | signed or hashed octets              | opaque byte band held at admission          | digest receipt + content projection      | parse-then-reserialize            |
|  [03]   | HTTP, RPC, or CLI surface            | contribution family — endpoint/group as data | derived server, client, and spec        | hand-rolled route table           |
|  [04]   | foreign binary or text format        | codec engine behind `Schema.transformOrFail` | `ParseError` on the one decode rail     | second codec fault family         |
|  [05]   | platform capability — fs, net, exec  | abstract `Context.Tag`                      | requirement in the `R` channel           | direct `node:*`/`fetch` import    |
|  [06]   | environment or config read           | `Config` through the provider chain         | validated value at construction          | scattered `process.env` reads     |
|  [07]   | process start                        | `runMain` in one boot module                | drained fibers and finalizers            | top-level `Effect.runPromise`     |
|  [08]   | off-thread or socket handoff         | Schema-typed marshal frame                  | tagged request/response family           | untyped `postMessage` payload     |

## [02]-[ADMISSION]

[DECODE_PLACEMENT]:
- Use: every ingress — request bodies, headers, query params, file content, message frames, storage reads.
- Law: the decode sits on the first line that sees the foreign value, and that seam fixes three decisions at once: the owner (`Schema.decodeUnknown(Owner)`), the accumulation posture (`errors: "all"` for reportable admission, first-error for guard seams), and the drift posture (`onExcessProperty: "error"` where an unknown member is evidence of contract skew, the default where extension bands are tolerated). The configured decode is a module-scope value — one admission policy per seam, never per call.
- Law: HTTP seams use the fused accessors — `HttpServerRequest.schemaBodyJson`, `schemaHeaders`, `schemaSearchParams` inbound; `HttpClientResponse.schemaJson`, `schemaBodyJson`, `matchStatus` on egress replies — where read, decode, and error lift are one combinator; a raw body read followed by a separate validate is the fused seam decomposed by hand.
- Law: a foreign value that resurfaces past the seam — a nested `unknown` band, a lazy provider default, a second read from the same source — is a second admission site, not a leaked first one; it decodes where it appears, because the first decode never covered it and interior totality holds only over what was admitted.
- Law: egress is the mirror seam — `Schema.encode` at the explicit projection point, never field-by-field serialization; the encoded twin derives from the same owner, so the wire shape cannot skew from the interior shape.
- Exemption: the `TextDecoder` byte-to-text crossing inside the admission kernel is the named platform-forced seam.
- Reject: a decoded value re-checked downstream; a decode inside a loop body where the seam owns the collection; `ParseResult.ArrayFormatter`/`TreeFormatter` rendering anywhere but the terminal reporting edge; an `as` cast standing where a decode produces evidence.

[BYTE_IDENTITY]:
- Use: signatures, content keys, idempotency tokens, checksum verification, byte-stable forwarding.
- Law: a sub-band that must round-trip byte-identically is held opaque at admission — `Schema.Uint8ArrayFromSelf` in memory, `Schema.Uint8ArrayFromBase64` across a text wire — and the digest computes over those held octets before any content parse; identity and content are two projections of one admission, never two reads of the source.
- Law: parse-then-reserialize is rejected for signed material — a re-encode respells float forms, key order, and escapes — so forwarding emits the held octets verbatim, and `Schema.encode` of the envelope re-emits the same band bytes by construction.
- Boundary: the receipt carries the coordinate and the digest, never the octets; the digest function is fixed at composition and arrives as a parameter or service, never chosen per site.

```typescript
import { Effect, Option, type ParseResult, Schema } from "effect"

// --- [MODELS] ---------------------------------------------------------------------------

class Passport extends Schema.Class<Passport>("Passport")({
  kind: Schema.Literal("<kind-a>", "<kind-b>"),
  extent: Schema.Number.pipe(Schema.positive(), Schema.brand("Extent")),
  note: Schema.optionalWith(Schema.String, { as: "Option" }),
}) {}

class Envelope extends Schema.Class<Envelope>("Envelope")({
  coordinate: Schema.String,
  band: Schema.Uint8ArrayFromBase64,
}) {}

// --- [OPERATIONS] -----------------------------------------------------------------------

const _opened = Schema.decodeUnknown(Envelope, { errors: "all", onExcessProperty: "error" })
const _content = Schema.decodeUnknown(Schema.parseJson(Passport), { errors: "all" })

const emitted: (envelope: Envelope) => Effect.Effect<typeof Envelope.Encoded, ParseResult.ParseError> =
  Schema.encode(Envelope)

const admitted: (
  raw: unknown,
  digest: (octets: Uint8Array) => string,
) => Effect.Effect<
  { readonly key: string; readonly coordinate: string; readonly label: string; readonly passport: Passport },
  ParseResult.ParseError
> = Effect.fn("admitted")(function* (raw: unknown, digest: (octets: Uint8Array) => string) {
  const envelope = yield* _opened(raw)
  const passport = yield* _content(new TextDecoder().decode(envelope.band))
  return {
    key: digest(envelope.band),
    coordinate: envelope.coordinate,
    label: Option.getOrElse(passport.note, () => "<value-a>"),
    passport,
  } as const
})

// --- [EXPORTS] --------------------------------------------------------------------------

export { admitted, emitted, Envelope, Passport }
```

## [03]-[CONTRACT_FAMILY]

[CONTRIBUTION_FAMILY]:
- Use: every HTTP, RPC, and CLI entry surface.
- Law: the contract is data — `HttpApiEndpoint` carries path, payload, success, and error Schemas as one declaration (`HttpApiEndpoint.get(name, path)` with `.setPath`/`.setPayload`/`.addSuccess`/`.addError`, or the `HttpApiSchema.param` template form); `HttpApiGroup.make(name).add(endpoint)` is the owning module's contribution; exactly one `HttpApi.make(id).add(group)` assembles at the composition root. The god contract has no lib-side existence — structurally impossible, not disciplined against.
- Law: the same assembly law spans transports — `RpcGroup.make(...Rpc.make(tag, { payload, success, error, stream }))` is the procedure family whose protocol and serialization cross as two orthogonal Layer axes at the root (`RpcServer.layerProtocolHttp`/`layerProtocolWebsocket`/`layerProtocolWorkerRunner`/`layerProtocolStdio` against `RpcSerialization.layerJson`/`layerNdjson`/`layerMsgPack`), and the CLI command tree contributes verbs under the identical shape. A definition names no transport, no port, no engine; the root selects every one.
- Law: handler exhaustiveness is compiler-checked — `HttpApiBuilder.group` demands `.handle` for every declared endpoint and `RpcGroup.toLayer` demands the full handler record; a missing or mistyped handler is a compile error, never a 404 discovered at runtime.
- Law: group-scoped concerns ride the declaration — `.middleware(Tag)` on the group, `.addError` for group-wide faults, `.prefix` for mount points — so a cross-cutting obligation is recoverable from the contract value, never from handler bodies.
- Reject: a hand-rolled router table where the declarative family fits; a route registered beside the contract; transport or codec baked into a handler; a second contract authored for a client variant.

[DERIVED_SURFACES]:
- Law: one declaration derives every consumer surface — `HttpApiBuilder.api` plus `HttpApiBuilder.serve` derive the server, `HttpApiClient.make` derives the fully typed client, `OpenApi.fromApi` derives the spec, `HttpApiBuilder.toWebHandler` derives the fetch-shaped handler for hostless runtimes; server, client, and spec cannot drift because they are projections of one value. `RpcClient.make` against the same group is the identical law on the RPC axis, and `RpcTest.makeClient` short-circuits transport in specs — the production and test callers share the contract.
- Law: endpoint faults are declared — `Schema.TaggedError` classes on `.addError` with their status — so the caller reconstructs the exact tagged family the handler failed with, and one error vocabulary spans the wire; the family's design is `rails-and-effects.md`'s.
- Reject: a hand-written fetch client beside a contract; an API document authored by hand; a client-side error type parallel to the declared fault; a spec regenerated into source and committed as a second truth.

```typescript
import { HttpApi, HttpApiBuilder, HttpApiClient, HttpApiEndpoint, HttpApiGroup, OpenApi } from "@effect/platform"
import { Effect, Layer, Schema } from "effect"

// --- [MODELS] ---------------------------------------------------------------------------

class Row extends Schema.Class<Row>("Row")({
  key: Schema.Number,
  label: Schema.String,
}) {}

class Missing extends Schema.TaggedError<Missing>()("Missing", { key: Schema.Number }) {}

// --- [CONTRACT] -------------------------------------------------------------------------

const _rows = HttpApiGroup.make("rows")
  .add(
    HttpApiEndpoint.get("one", "/rows/:key")
      .setPath(Schema.Struct({ key: Schema.NumberFromString }))
      .addSuccess(Row)
      .addError(Missing, { status: 404 }),
  )
  .add(HttpApiEndpoint.post("grow", "/rows").setPayload(Schema.Struct({ label: Schema.String })).addSuccess(Row))

const Contract = HttpApi.make("contract").add(_rows)

// --- [COMPOSITION] ----------------------------------------------------------------------

const _RowsLive = HttpApiBuilder.group(Contract, "rows", (handlers) =>
  handlers
    .handle("one", ({ path }) =>
      path.key > 0
        ? Effect.succeed(new Row({ key: path.key, label: "<value-a>" }))
        : Effect.fail(new Missing({ key: path.key })))
    .handle("grow", ({ payload }) => Effect.succeed(new Row({ key: 1, label: payload.label }))))

const ContractLive = HttpApiBuilder.api(Contract).pipe(Layer.provide(_RowsLive))

// --- [OPERATIONS] -----------------------------------------------------------------------

const specification = OpenApi.fromApi(Contract)

const probed = Effect.fn("probed")(function* (key: number) {
  const client = yield* HttpApiClient.make(Contract, { baseUrl: "<origin>" })
  return yield* client.rows.one({ path: { key } })
})

// --- [EXPORTS] --------------------------------------------------------------------------

export { Contract, ContractLive, Missing, probed, Row, specification }
```

## [04]-[CODEC_ENGINE]

[ENGINE_FOLD]:
- Use: every foreign codec — binary formats, text formats, compression, crypto envelopes — whose decode/encode pair the platform does not own.
- Law: the engine is a pure function pair behind one `Schema.transformOrFail` from the byte schema to `Schema.Unknown`, composed onto the owned shape with `Schema.compose(shape, { strict: false })`; the engine's throw is caught inside the transform — `Either.try` folding the defect to `new ParseResult.Type(ast, actual, message)` — so codec faults join the same `ParseError` rail every admission rides. A codec fault family beside the decode rail is the rejected second vocabulary: the caller already discriminates admission failure, and the engine's failure is admission failure.
- Law: the engine configures once at the owner per policy — instance options, untrusted-input ceilings, tag and extension registries are module-init facts — and the configured decode is the exported surface; the interior receives the owned shape and never a raw engine value, so replacing the engine is an edit to one module.
- Law: the transform is total both directions — the encode arm folds the engine's serializer through the same `Either.try` — so the owner round-trips and the wire twin derives; a decode-only crossing that hand-writes its egress is half an owner.
- Reject: engine output touched by domain code before the Schema owner; a throwing engine call outside the transform; per-call engine construction; a decode ceiling checked after the decode it was meant to bound.

[QUIRK_CAPTURE]:
- Law: provider type-surface drift — runtime-real members the shipped declarations omit, mislabeled declarations, phantom re-exports — reconciles at one `declare module "<package>"` augmentation co-located with the engine owner; the augmentation declares only verified runtime truth, the mislabeled member is never called, and downstream composes the corrected surface without re-discovering the mismatch.
- Law: behavioral quirks — a default option that engages a proprietary dialect, a shared global registry, a native accelerator probe — are internalized as the owner's configuration facts; the owner's construction encodes the correct posture, so no consumer can reach the quirk path.
- Reject: an `as` bridge at call sites where the augmentation owns the truth; the augmentation in a global ambient dump far from the engine; a wrapper whose only job is smuggling a corrected type.

```typescript
import { Either, ParseResult, Schema } from "effect"

// --- [TYPES] ----------------------------------------------------------------------------

type Engine = {
  readonly decode: (octets: Uint8Array) => unknown
  readonly encode: (value: unknown) => Uint8Array
}

// --- [MODELS] ---------------------------------------------------------------------------

class Snapshot extends Schema.Class<Snapshot>("Snapshot")({
  key: Schema.String,
  extent: Schema.Number,
  held: Schema.Uint8ArrayFromSelf,
}) {}

// --- [OPERATIONS] -----------------------------------------------------------------------

const engineSchema = <A, I, R>(engine: Engine, shape: Schema.Schema<A, I, R>): Schema.Schema<A, Uint8Array, R> =>
  Schema.transformOrFail(Schema.Uint8ArrayFromSelf, Schema.Unknown, {
    strict: true,
    decode: (octets, _options, ast) =>
      Either.try({
        try: () => engine.decode(octets),
        catch: (defect) => new ParseResult.Type(ast, octets, String(defect)),
      }),
    encode: (value, _options, ast) =>
      Either.try({
        try: () => engine.encode(value),
        catch: (defect) => new ParseResult.Type(ast, value, String(defect)),
      }),
  }).pipe(Schema.compose(shape, { strict: false }))

const admitted = (engine: Engine) => Schema.decodeUnknown(engineSchema(engine, Snapshot))

// --- [EXPORTS] --------------------------------------------------------------------------

export { admitted, engineSchema, Snapshot }
export type { Engine }
```

## [05]-[RUNTIME_SELECTION]

[CAPABILITY_TAG]:
- Use: filesystem, HTTP egress, subprocess, tty, socket, worker spawn, key-value storage, path algebra.
- Law: capability is the abstract Tag — `FileSystem.FileSystem`, `HttpClient.HttpClient`, `Command`/`CommandExecutor`, `Terminal.Terminal`, `Socket.Socket`, `Worker.WorkerManager`, `KeyValueStore.KeyValueStore`, `Path.Path` — yielded on the rail; the runtime binding — `NodeContext.layer` plus `NodeHttpClient.layerUndici` and `NodeHttpServer.layer`, `BunContext.layer` plus `BunHttpServer.layer`, `FetchHttpClient.layer`, `BrowserHttpClient.layerXMLHttpRequest` — is provided once at the root, so a runtime change is a Layer row, never a fork; the wiring algebra is `services-and-layers.md`'s.
- Law: a direct `node:*`, `fetch`, or socket-library import in domain flow bypasses tracing, the typed error rail, pooling policy, and portability in one stroke; those specifiers survive only inside the binding packages and the named FFI seam.
- Law: egress policy attaches to the one shared client as composed transformers — `HttpClient.retryTransient`, `filterStatusOk`, `mapRequest`, `withTracerPropagation` — so retry, admission, auth, and propagation are recoverable from the client's declaration, never re-stated per call.
- Boundary: config ingress is the same shape — the provider chain (`PlatformConfigProvider.layerDotEnv`, `layerFileTree`) satisfies `Config` reads at the root; a `process.env` read in domain flow is the same defect as a `node:fs` import.

[BOOT_EDGE]:
- Law: one `runMain` per process is the entire imperative surface — `NodeRuntime.runMain`, `BunRuntime.runMain`, and `BrowserRuntime.runMain` inhabit the single `RunMain` shape that forks the root fiber, installs interrupt handling, sets the exit code from the `Exit`, and drains finalizers on signal; its parameter pins `R` to `never`, so the boot line is where an unwired Tag becomes a compile error rather than a runtime absence.
- Law: `Effect.runPromise` heading a long-lived process is the rejected boot — no signal draining, finalizers lost on interrupt; a second `runMain` in one process is the named defect.
- Law: the boot module is the only module that names a runtime, and it exports nothing — the empty exports surface is the structural proof it is terminal; a worker entry is a boot module under the same law, `WorkerRunner.launch` run beneath the runner binding.
- Law: importing a binding pins a module to that runtime lane; domain modules import zero bindings and therefore sit in every lane — the per-runtime subpath gate enforces the fence, and this page owns its consequence: a browser bundle cannot resolve a server binding because the module graph never reaches one.
- Exemption: the top-level `runMain` call is the named platform-forced statement seam.
- Reject: a runtime binding imported for one capability where the aggregate context Layer already carries it; runtime detection branching inside domain flow; a library module that calls any `run*`.

```typescript
import { Command, FetchHttpClient, FileSystem, HttpClient, Path } from "@effect/platform"
import { NodeContext, NodeRuntime } from "@effect/platform-node"
import { Effect, Layer } from "effect"

// --- [OPERATIONS] -----------------------------------------------------------------------

const staged = Effect.fn("staged")(function* (source: string, root: string) {
  const client = yield* HttpClient.HttpClient
  const fs = yield* FileSystem.FileSystem
  const path = yield* Path.Path
  const body = yield* client.get(source).pipe(
    Effect.flatMap((response) => response.text),
    Effect.scoped,
  )
  const target = path.join(root, "<file>")
  yield* fs.writeFileString(target, body)
  const code = yield* Command.make("<command>", target).pipe(Command.exitCode)
  return { target, code } as const
})

// --- [COMPOSITION] ----------------------------------------------------------------------

const Root = Layer.mergeAll(NodeContext.layer, FetchHttpClient.layer)

// --- [ENTRY] ----------------------------------------------------------------------------

NodeRuntime.runMain(staged("<url>", "<root>").pipe(Effect.provide(Root)))

// --- [EXPORTS] --------------------------------------------------------------------------

export {}
```

## [06]-[MARSHAL]

[WORKER_PROTOCOL]:
- Use: off-main-thread compute, worker pools, process-boundary request/response.
- Law: the message vocabulary is a closed family of `Schema.TaggedRequest` classes — payload, success, and failure Schemas in one declaration — collected by `Schema.Union` into one protocol; the pool executes decoded requests (`Worker.makePoolSerializedLayer(Tag, { size, concurrency })` — `executeEffect` one-shot, `execute` streaming, `broadcast` fan-out; the request's declared nature discriminates the modality, never a parallel pool), and the worker implements the same protocol through `WorkerRunner.layerSerialized(protocol, handlers)` — an `Effect` handler answers once, a `Stream` handler streams — with the handler record compiler-checked against the union.
- Law: failure crosses as the request's failure Schema and reconstructs as the same tagged class on the caller, so one fault vocabulary spans the thread boundary; a stringified error message crossing a worker seam destroys the discriminant every downstream recovery dispatches on.
- Law: zero-copy crossings are declared at the schema — `Transferable.Uint8Array`, `Transferable.MessagePort`, `Transferable.ImageData`, or `Transferable.schema(shape, project)` for a composite — so the marshal plan is recoverable from the message declaration, never from an argument list at a call site.
- Boundary: the pool's spawner and the runner's binding are runtime rows at the boot edge; the protocol module names neither; the RPC protocol row `RpcServer.layerProtocolWorkerRunner` serves an `RpcGroup` over this same runner law.
- Reject: an untyped `postMessage` payload; a hand-rolled `onmessage` switch; a string-keyed message map with casts; per-message transferable lists at call sites; a second pool where a request tag discriminates.

[FRAME_CHANNEL]:
- Use: socket duplex, WebSocket sessions, any byte-stream peer.
- Law: the socket is a byte `Channel` — `Socket.toChannelWith<E>()` — and the frame codec layers over it as `MsgPack.duplexSchema` or `Ndjson.duplexSchema({ inputSchema, outputSchema })`, yielding typed messages in both directions with backpressure; framing, chunk reassembly, and codec faults (`MsgPackError`, `ParseError`) ride the channel's own error rail, and the pipeline geometry above the frame is `streams.md`'s.
- Law: the socket's construction is capability, not code — `Socket.makeWebSocket(url)` against the `Socket.WebSocketConstructor` Tag the runtime row satisfies — so one framed transport definition serves every runtime lane.
- Reject: raw socket event listeners; a hand-written length-prefix parser; `JSON.stringify` written to a socket where the duplex codec owns the frame.

```typescript
import { MsgPack, Socket, Transferable, Worker, WorkerRunner } from "@effect/platform"
import { Context, Effect, ParseResult, Schema, Stream } from "effect"

// --- [MODELS] ---------------------------------------------------------------------------

class MarshalFault extends Schema.TaggedError<MarshalFault>()("MarshalFault", {
  reason: Schema.Literal("<reason-a>", "<reason-b>"),
}) {}

class Grade extends Schema.TaggedRequest<Grade>()("Grade", {
  payload: { octets: Transferable.Uint8Array },
  success: Schema.Struct({ key: Schema.String, extent: Schema.Number }),
  failure: MarshalFault,
}) {}

class Sweep extends Schema.TaggedRequest<Sweep>()("Sweep", {
  payload: { keys: Schema.Array(Schema.String) },
  success: Schema.String,
  failure: MarshalFault,
}) {}

const Protocol: Schema.Union<[typeof Grade, typeof Sweep]> = Schema.Union(Grade, Sweep)

// --- [SERVICES] -------------------------------------------------------------------------

class Bench extends Context.Tag("Bench")<Bench, Worker.SerializedWorkerPool<Grade | Sweep>>() {}

// --- [COMPOSITION] ----------------------------------------------------------------------

const BenchLive = Worker.makePoolSerializedLayer(Bench, { size: 4, concurrency: 2 })

const RunnerLive = WorkerRunner.layerSerialized(Protocol, {
  Grade: ({ octets }) => Effect.succeed({ key: "<value-a>", extent: octets.byteLength }),
  Sweep: ({ keys }) => Stream.fromIterable(keys),
})

// --- [OPERATIONS] -----------------------------------------------------------------------

const graded = (octets: Uint8Array) => Effect.flatMap(Bench, (pool) => pool.executeEffect(new Grade({ octets })))

const swept = (keys: ReadonlyArray<string>) => Stream.unwrap(Effect.map(Bench, (pool) => pool.execute(new Sweep({ keys }))))

const framed = (socket: Socket.Socket) =>
  Socket.toChannelWith<MsgPack.MsgPackError | ParseResult.ParseError>()(socket).pipe(
    MsgPack.duplexSchema({ inputSchema: Protocol, outputSchema: Protocol }),
  )

// --- [EXPORTS] --------------------------------------------------------------------------

export { Bench, BenchLive, framed, graded, Grade, MarshalFault, Protocol, RunnerLive, swept, Sweep }
```
