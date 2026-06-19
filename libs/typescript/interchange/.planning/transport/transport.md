# [INTERCHANGE_TRANSPORT]

The outbound transport edge of the host-free wire boundary: one polymorphic browser transport over a protocol-selection axis, one generated browser-dialable client per service over that one transport, the capability tuple gating which method kinds each protocol admits, the chunked-framing fold that rides the same transport, and the buf descriptor pipeline that is the rail's build-time input edge. The owning C# `#TS_PROJECTION` fence is the authoritative wire shape; this page names which client derives from which descriptor and fixes codegen as the transport input, never re-authoring a shape. The descriptor pipeline and the capability-SDK leg are clusters on this page, not separate files: their output is `src/gen/*_pb.ts`, the values the transport composes.

## [1]-[INDEX]

- [1]-[TRANSPORT_AND_CLIENTS]: the protocol-selection transport, one client per service, the capability tuple, the framing fold.
- [2]-[CODEGEN_TOOLING]: the committed-descriptor buf pipeline and the capability-SDK leg as the transport input.
- [3]-[TS_PROJECTION]: the proto service shapes and transport-capability the transport derives.


## [2]-[TRANSPORT_AND_CLIENTS]

- Owner: `WireTransport`, the single transport `Effect.Service` over a protocol-selection axis, plus `WireClients`, one `Client<typeof GenService>` per browser-dialable service built over it. The `TransportProtocol` axis routes a long-lived server-stream leg over the Connect protocol (`createConnectTransport`, standard HTTP, trailer-free, no roughly-60s server-stream cap) and the binary-frame leg over gRPC-Web (`createGrpcWebTransport`) where the backend dictates, and carries a third forward-only `webTransport` case the connect-es v2 surface ships no factory for. `TransportCapabilityWire` is the per-protocol row carrying each protocol's binary framing mode, its admitted method-kind tuple (transcribed verbatim from the C# wire), and the `available` dial-gate bit — the `grpcWeb` row carries its `application/grpc-web` media type and the two browser-carried kinds, the `connect` row the four, and the `webTransport` row the full four with `available: false` because connect-es v2 exposes no `createWebTransport`; framing, capability, and availability are the one row, never a parallel framing shape beside the capability tuple. The dial-time availability gate reads the same `available` bit `Transport/gateway.md` reads, so an unavailable protocol selection faults at construction through `faultDetailRail` rather than dialing a non-existent factory.
- Lifetime: ONE `Transport` per authority is resolved once at service construction and held app-lifetime over the captured `Effect.runtime` snapshot; the per-call transport mint is the deleted form. The connect-web factory exposes no retry knob (no `RetryPolicy` analogue to the C# channel's `MethodConfig`), so wire retry is composition-time policy, not a transport option: the `retryUnary` boundary rail wraps the unary `Effect.tryPromise` leg through the `Effect.retry(self, { schedule: retrySchedule, while: retryableWire })` options arm — `retrySchedule` is `Schedule.exponential("100 millis", 2)` `Schedule.jittered`, `Schedule.union`-bounded by `Schedule.recurs(3)`, and the `while` predicate keys the typed `FaultDetail` error channel directly so only the `retryableWire` `HopFault`/`reason: "wire"`/`Code.Unavailable` leg (the C# `RetryableStatusCodes` set) recurs while a `ComputeFault`/`StoreFault`/`Quarantine` short-circuits, faulting onto the already-stamped `FaultDetail` after the schedule exhausts; a server-stream leg carries no retry (a partially-drained stream is not idempotent), mirroring the C# law that retry is a per-row owner column, never both channel and seam. The `while`-by-error-input arm is verified against the `effect` `Retry.Options<E> { while?, until?, times?, schedule? }` surface — the prior `Schedule.whileInput`/`Schedule.intersect` composition is the deleted heavier form; the options arm keys the fault input one-pass without the tuple-output `intersect` carries.
- Cases: the four browser-dialable generated services over one transport; a unary call resolves by await, a server-stream consumes by async iteration. The client-stream capture leg (`DocumentServiceShape.captureEvents`) and the bidirectional artifact-sync leg (`ArtifactSyncShape.sync`) are structurally excluded on `grpcWeb`, mirrored from the upstream wire where the capability tuple admits only `unary` and `serverStream` on the `grpcWeb` row while the `connect` row admits all four — the exclusion is read from the wire, never invented branch-side. The connect-es `Client<typeof DocumentService>` STILL surfaces `captureEvents` on its generated type (connect-es projects every service method onto the client face regardless of stream kind), so the clientStream method is present on the client type but faulted by the capability gate at dial time, never absent from the generated client — the structural exclusion is a runtime capability-row gate, not a type-level removal.
- Entry: outbound calls cross one transport whose interceptor stamp axis is polymorphic over the correlation identifier, the trace parent, and the bearer credential, mirroring the `CallSpine.CorrelationKey`/`TraceparentKey` constants on the upstream wire. The interceptor is a connect-es async interceptor over a captured `Effect.runtime` snapshot taken once at service construction. A single `Runtime.runPromise` per call resolves the live token producer (`AuthSession.tokenHeader`, the `Option<string>` full `Bearer` header value) and the active span in one effect, so a token cached past expiry never ships and the W3C `traceparent` is authored from the runtime-resolved span context, never a per-call double promise round-trip. Per-call cancellation threads interruption into transport cancellation through the call signal; one interceptor stamps all three header rows in one pass, never three parallel interceptors. `ConnectError.from` normalizes a fetch `AbortError` and `TimeoutError` alike to `Code.Canceled`; a server-enforced deadline arrives as a distinct `Code.DeadlineExceeded` on the `ConnectError`, so `faultDetailRail.fromConnect` keys the no-trailer landing by the connect `Code` and a deadline leg lands distinctly from a client-side abort.
- Packages: `@connectrpc/connect` for `createClient`, `@connectrpc/connect-web` for `createConnectTransport` and `createGrpcWebTransport`, `@bufbuild/protobuf` for the descriptor runtime, and `effect` for the transport-as-`Effect.Service` composition.
- Frames: the framing fold is the transport-level chunked boundary over the same shared transport — NOT the content-addressed reassembly (`Codec/frame.md` owns the Crc32-verify-and-stitch). The server-stream artifact-delivery path is the `serverStream` method set the C# wire declares — `ComputeServiceShape.generate` (`serverStream`→`TokenChunk`) and `ComputeServiceShape.subtreeFetch` (`serverStream`→`GraphChunk`) — whose chunk frames lift into a `Stream.Stream<ArtifactFrameWire, FaultDetail>` through `Stream.fromAsyncIterable` over the connect-es server-stream async iterator, backpressure flowing from the consumer through the iterator's natural pull (each `next()` is one network read, never pre-buffered) and a `Stream.buffer({ capacity, strategy: "suspend" })` bound holding at most one 64-KiB frame window in flight so a slow `DecodeWorkerPool` consumer suspends the wire read instead of growing an unbounded queue. `solve` is `unary`→`SolveResponse` and carries no server-stream frame leg — the branch never invents an `ArtifactFrame` server-stream contract the C# method kinds do not declare. The `captureEvents` clientStream is structurally excluded on `grpcWeb`, so the UPLOAD leg frames a large outbound artifact as a bounded `Stream` of `ArtifactFrameWire` chunks driven through repeated UNARY calls — `Stream.mapEffect` over a `Stream.fromChunks` of fixed 64-KiB-offset frames, each one unary `await`, `concurrency: 1` sequencing so chunk N+1 never ships before chunk N's unary reply, the backpressure being the per-call await itself. The framing fold is one polymorphic surface keyed by direction, never two parallel framers; the frame TYPE is `ArtifactFrameWire` decoded verbatim from the upstream wire, never re-authored.
- Growth: a new browser-dialable service lands as one generated client row over the same transport; a new remote verb lands as one generated method row; a new chunked direction lands as one `FrameDirection` case on the framing fold; the `webTransport` HTTP/3 raw-byte leg activates by flipping its row's `available` bit to `true` and binding the connect-es WebTransport factory the moment v2 ships it (RESEARCH: connect-es exposes no `createWebTransport`), routing the frame stream over the same transferable boundary the `Codec/frame.md` worker seam already consumes; a cross-origin deployment lands the credential and CORS rows as designed-only growth mirrored from the C# boundary.
- Boundary: the transport is same-origin under the co-hosted topology and configures no cross-origin header; the excluded client-stream and bidi legs carry no browser path because the C# boundary excludes them, never because the branch invents an exclusion; `AuthSession` is consumed as a per-call token producer from `platform` and never owned here, so `interchange` declares no OIDC dependency; the chunked-framing fold produces the `Stream<ArtifactFrameWire, FaultDetail>` the artifact rail consumes and never itself reassembles, verifies Crc32, or derives a content key — that is `Codec/frame.md`'s owned surface, never duplicated branch-side.

```ts contract
type StreamKind = "unary" | "serverStream" | "clientStream" | "bidi";
type TransportProtocol = "connect" | "grpcWeb" | "webTransport";

const TransportCapabilityWire = Schema.Struct({
  connect: Schema.Struct({ mode: Schema.Literal("binary"), available: Schema.Literal(true), carries: Schema.Tuple(Schema.Literal("unary"), Schema.Literal("serverStream"), Schema.Literal("clientStream"), Schema.Literal("bidi")) }),
  grpcWeb: Schema.Struct({ mode: Schema.Literal("binary"), available: Schema.Literal(true), mediaType: Schema.Literal("application/grpc-web"), carries: Schema.Tuple(Schema.Literal("unary"), Schema.Literal("serverStream")) }),
  webTransport: Schema.Struct({ mode: Schema.Literal("binary"), available: Schema.Literal(false), carries: Schema.Tuple(Schema.Literal("unary"), Schema.Literal("serverStream"), Schema.Literal("clientStream"), Schema.Literal("bidi")) }),
});
type TransportCapabilityWire = Schema.Schema.Type<typeof TransportCapabilityWire>;

const retryableWire = (fault: FaultDetail): boolean =>
  FaultDetail.$is("HopFault")(fault) && fault.reason === "wire" && fault.evidence.code === String(Code.Unavailable);

const retrySchedule = Schedule.exponential("100 millis", 2).pipe(Schedule.jittered, Schedule.union(Schedule.recurs(3)));

const retryUnary = <A>(call: Effect.Effect<A, FaultDetail>): Effect.Effect<A, FaultDetail> =>
  Effect.retry(call, { schedule: retrySchedule, while: retryableWire });

interface WireTransport {
  readonly transport: Transport;
  readonly interceptor: Interceptor;
}

interface WireClients {
  readonly compute: Client<typeof ComputeService>;
  readonly document: Client<typeof DocumentService>;
  readonly control: Client<typeof ControlService>;
  readonly health: Client<typeof HealthService>;
}

class WireTransportLive extends Effect.Service<WireTransportLive>()("@rasm/ts/interchange/WireTransport", {
  effect: Effect.gen(function* () {
    const config = yield* RuntimeConfig;
    const session = yield* AuthSession;
    const baseUrl = yield* config.apiBaseUrl;
    const protocol = yield* config.transportProtocol;
    const runtime = yield* Effect.runtime<never>();
    const stampEffect = Effect.all({ token: session.tokenHeader, span: Effect.currentSpan.pipe(Effect.option) });
    const interceptor: Interceptor = (next) => async (req) => {
      if (!req.header.has("rasm-correlation")) req.header.set("rasm-correlation", crypto.randomUUID());
      const { token, span } = await Runtime.runPromise(runtime)(stampEffect);
      Option.match(span, { onNone: () => {}, onSome: (s) => req.header.set("traceparent", `00-${s.traceId}-${s.spanId}-01`) });
      Option.match(token, { onNone: () => {}, onSome: (t) => req.header.set("authorization", t) });
      return next(req);
    };
    const transport = yield* Match.value(protocol).pipe(
      Match.when("connect", () => Effect.succeed(createConnectTransport({ baseUrl, useBinaryFormat: true, interceptors: [interceptor] }))),
      Match.when("grpcWeb", () => Effect.succeed(createGrpcWebTransport({ baseUrl, useBinaryFormat: true, interceptors: [interceptor] }))),
      Match.when("webTransport", () => Effect.fail(FaultDetail.HopFault({ reason: "command-disabled", evidence: { protocol: "webTransport" } }))),
      Match.exhaustive,
    );
    return { transport, interceptor } satisfies WireTransport;
  }),
}) {}
```

The chunked-framing fold rides this same transport as one polymorphic surface over a `FrameDirection` `Data.TaggedEnum` — the server-stream DOWN leg lifts the connect-es async iterator into a backpressured `Stream`, the unary-chunked UP leg drives a bounded outbound `Stream` of 64-KiB frames through repeated unary calls. Reassembly, Crc32 verification, and content-key derivation are owned by `Codec/frame.md`; this fold owns only the transport-level chunk boundary and backpressure.

```ts contract
// --- [TYPES] -------------------------------------------------------------------------
type FrameDirection = Data.TaggedEnum<{
  readonly Down: { readonly call: () => AsyncIterable<ArtifactFrameWire> };
  readonly Up: { readonly source: Stream.Stream<Uint8Array, FaultDetail>; readonly artifactId: string; readonly send: (frame: ArtifactFrameWire) => Promise<void> };
}>;
const FrameDirection = Data.taggedEnum<FrameDirection>();

// --- [CONSTANTS] ---------------------------------------------------------------------
const FRAME_BYTES = 65_536;

// --- [SERVICES] ----------------------------------------------------------------------
interface ArtifactFrameStreaming {
  readonly frame: (direction: FrameDirection) => Stream.Stream<ArtifactFrameWire, FaultDetail>;
}

class ArtifactFrameStreamingLive extends Effect.Service<ArtifactFrameStreamingLive>()("@rasm/ts/interchange/ArtifactFrameStreaming", {
  sync: () => {
    const frame = (direction: FrameDirection): Stream.Stream<ArtifactFrameWire, FaultDetail> =>
      FrameDirection.$match(direction, {
        Down: ({ call }) =>
          Stream.fromAsyncIterable(call(), faultDetailRail.fromConnect).pipe(
            Stream.buffer({ capacity: 1, strategy: "suspend" }),
          ),
        Up: ({ source, artifactId, send }) =>
          source.pipe(
            Stream.mapConcat((bytes) => splitFrames(crc32, artifactId, bytes)),
            Stream.mapEffect((f) =>
              Effect.tryPromise({ try: () => send(f), catch: faultDetailRail.fromConnect }).pipe(Effect.as(f)),
              { concurrency: 1 },
            ),
          ),
      });
    return { frame } satisfies ArtifactFrameStreaming;
  },
}) {}

// --- [OPERATIONS] --------------------------------------------------------------------
const splitFrames = (crc: Crc32, artifactId: string, bytes: Uint8Array): ReadonlyArray<ArtifactFrameWire> =>
  Array.makeBy(Math.max(1, Math.ceil(bytes.byteLength / FRAME_BYTES)), (i) => {
    const offset = i * FRAME_BYTES;
    const payload = bytes.subarray(offset, offset + FRAME_BYTES);
    return { artifactId, artifactBytes: bytes.byteLength, offset, frameCrc: crc.of(payload), payload };
  });
```

## [3]-[CODEGEN_TOOLING]

- Owner: the descriptor pipeline as the transport owner's build-time input edge — the committed app-root `FileDescriptorSet` input, the `buf.gen.yaml` config carrying the message-and-service plugin plus the capability-SDK plugin, and the `src/gen/*_pb.ts` output the transport composes. There is no `codegen.ts` runtime module; the output is generated code, so the pipeline is the input edge of `transport.ts`.
- Cases: descriptor ingestion reads the app-root-emitted `FileDescriptorSet` published beside the discovery manifest by the C# `ContractGuard`, so the branch consumes the same descriptor set the .NET side emits and never re-authors a `.proto`; the generation pass runs the single `protoc-gen-es` v2 plugin with `target=ts` and `include_imports: true`; the descriptor runtime composes the generated values — message construction through `create`, binary through `fromBinary`/`toBinary`, and the file-aware registry from the emitted `FileDescriptorSet` the fault rail passes to `findDetails`; client derivation runs `createClient(service, transport)` per browser-dialable service descriptor.
- Entry: `buf generate` is the build-time driver — a tool surface, never a runtime import — and the generated `*_pb.ts` modules are the runtime values `WireClients` derives from; the `MethodShape` aliases in the inventory mirror the descriptor `methodKind` one-for-one, so a method-shape row and its generated descriptor never diverge; unknown fields are retained by the descriptor runtime by default, the additive-drift tolerance the versioning law relies on.
- Packages: `@bufbuild/protoc-gen-es` for the message-and-service codegen, `@bufbuild/buf` (`allowBuilds: true`) for the build-time CLI driver, and `@bufbuild/protobuf` for the runtime descriptor values.
- Growth: a new wire service lands one generated descriptor and one `createClient` row; a descriptor-set refresh regenerates `src/gen` and is the only edit to it; `@connectrpc/protoc-gen-connect-es` is the rejected separate-plugin form and a hand-edited generated module is the deleted form; the capability-descriptor SDK codegen leg is a SECOND plugin row on the same single-config pipeline gated on the C# `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN` descriptor source — never a parallel `buf.gen.yaml`, never a hand-maintained capability client.
- Boundary: the pipeline emits `src/gen/*_pb.ts` and never a hand-copied `.proto` tree; `buf.gen.yaml` carries the proto-message plugin plus the capability-SDK plugin and never the rejected message-plus-service split; the descriptor registry resolves the full type graph from one module because `include_imports` embeds the transitive descriptors; the capability SDK leg reads ONLY the C#-emitted `CapabilityDescriptorWire[]` catalog descriptor — the TS branch never re-authors a capability shape, never hand-writes a command method, and never reaches a C# interior.

```yaml
version: v2
inputs:
  - directory: gen/descriptors
plugins:
  - local: protoc-gen-es
    out: src/gen
    include_imports: true
    opt: target=ts
  - local: protoc-gen-capability-es
    out: src/gen
    opt:
      - target=ts
      - emit_mcp_client=true
```

The capability-SDK leg is the SECOND plugin on the SAME pipeline, not a runtime module: it consumes the C# `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN` `DiscoveryResultWire[]` catalog descriptor (the `CapabilityDescriptorWire`/`CapabilityCommandReceiptWire`/`DiscoveryResultWire` shapes the C# `SdkTarget.Typescript` renderer emits) and generates `src/gen/capabilities_pb.ts` — a typed-per-descriptor effect-classed command surface plus one MCP client leg, both derived, never hand-shaped. The generated descriptor shapes are transcribed below as the SHAPE the TS branch consumes; the branch reads the descriptor, never re-authors the catalog.

The `EffectClassKey`/`IdempotencyKey`/`CostUnitKey` literal vocabularies and the `txn` union literals below are transcribed VERBATIM from the `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN` enum source (the C# `EffectClass`/`Idempotency`/`CostUnit` smart-enums and the `CommandTxn` union), never re-authored — a new C# effect class, idempotency mode, cost unit, or txn kind lands as one added literal folding an unknown to the `Ingress/quarantine.md` `Additive` case, never a silent branch-side drift. This is the named cross-language drift seam: the literal sets are owned by C# and mirrored here under the same verbatim-transcription law `Contract/inventory#WIRE_LAW` fixes, not invented branch vocabulary.

```ts contract
// --- [TYPES] -------------------------------------------------------------------------
// sourced verbatim from `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN`
type EffectClassKey = "pure" | "read" | "write" | "external" | "irreversible";
type IdempotencyKey = "idempotent" | "keyed" | "single-shot" | "non-idempotent";
type CostUnitKey = "cpu-millis" | "wall-millis" | "bytes-egress" | "model-tokens" | "calls";

// --- [MODELS] ------------------------------------------------------------------------
const CostVectorWire = Schema.Record({ key: Schema.Literal("cpu-millis", "wall-millis", "bytes-egress", "model-tokens", "calls"), value: Schema.Number });

class DiscoveryResultWire extends Schema.Class<DiscoveryResultWire>("@rasm/ts/interchange/DiscoveryResultWire")({
  descriptor: Schema.String,
  surface: Schema.String,
  effect: Schema.Literal("pure", "read", "write", "external", "irreversible"),
  idempotency: Schema.Literal("idempotent", "keyed", "single-shot", "non-idempotent"),
  estimated: CostVectorWire,
  scopeHash: Schema.String,
}) {}

class CapabilityCommandReceiptWire extends Schema.Class<CapabilityCommandReceiptWire>("@rasm/ts/interchange/CapabilityCommandReceiptWire")({
  descriptor: Schema.String,
  txn: Schema.Union(
    Schema.Struct({ kind: Schema.Literal("committed"), dispatch: Schema.String }),
    Schema.Struct({ kind: Schema.Literal("rolled-back"), reason: Schema.String }),
    Schema.Struct({ kind: Schema.Literal("compensated"), forward: Schema.String, compensation: Schema.String }),
    Schema.Struct({ kind: Schema.Literal("refused"), fault: Schema.String }),
  ),
  charged: CostVectorWire,
  elapsed: Schema.String,
  correlation: Schema.String,
}) {}
```

The generated `capabilities_pb.ts` SDK surface is a typed command face over the `CommandService` wire verb — ONE polymorphic `invoke` method keyed by the descriptor id, never a sibling method per descriptor and never a hand-written client; the MCP client leg is the same surface re-projected as the MCP tool descriptor list the host advertises. The `CapabilitySdk` SERVICE CONTRACT below is the consumed shape; its `*Live` fold stays BLOCKED on the upstream `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN` descriptor — `CapabilityClient` and its generated `discover`/`invoke`/`argumentSchema` members are not `.d.ts`-confirmed until the plugin emits `capabilities_pb.ts`, so no live fold calls them here. The contract fixes the obligation the plugin must satisfy; `CAPABILITY_SDK_CODEGEN` carries the realization.

```ts contract
// --- [SERVICES] ----------------------------------------------------------------------
interface CapabilitySdk {
  readonly catalog: Effect.Effect<ReadonlyArray<DiscoveryResultWire>, FaultDetail>;
  readonly invoke: (descriptor: string, args: Record<string, unknown>) => Effect.Effect<CapabilityCommandReceiptWire, FaultDetail>;
  readonly mcpTools: Effect.Effect<ReadonlyArray<McpToolWire>, FaultDetail>;
}

interface McpToolWire {
  readonly name: string;
  readonly description: string;
  readonly effect: EffectClassKey;
  readonly idempotency: IdempotencyKey;
  readonly inputSchema: unknown;
}
```

The `CapabilitySdkLive` fold lands with `CAPABILITY_SDK_CODEGEN`. The codegen plugin must emit `CapabilityClient` as one `createClient(CommandService, transport)` row over the SAME shared `WireTransport` (never a second transport, never a hand-written client) carrying three generated members: `discover()` returning the `DiscoveryResultWire[]` catalog, `invoke(descriptor, arguments)` returning the `CapabilityCommandReceiptWire`, and a per-descriptor argument-schema accessor binding the C# `JsonSchemaExporter` schema one-per-descriptor (identical digest across all three SDK targets) so `mcpTools.inputSchema` is the descriptor's real generated argument contract, never a hand-built `{type:"object"}` stub. `catalog`/`invoke` decode the generated reply through `Schema.decodeUnknown(DiscoveryResultWire)`/`(CapabilityCommandReceiptWire)` mapping the `ParseError` through `faultDetailRail.fromConnect`; `mcpTools` projects the catalog rows with no schema field on `DiscoveryResultWire` because that is the C# catalog shape verbatim. The member spellings (`discover`, `invoke`, the schema accessor) resolve against the emitted `capabilities_pb.ts` `.d.ts` before the fold is transcribed, never asserted from this page.

The `CapabilitySdkLive` `Effect.Service` design fence below is the realization SHAPE the obligation admits — the `invoke`-by-descriptor polymorphic dispatch, the `catalog`/`mcpTools` decode-and-project folds, and the one `createClient` row over the shared transport. It is GATED on the absent `protoc-gen-capability-es` plugin: the three `CapabilityClient` member spellings (`discover`, `invoke`, `argumentSchema`) carry the obligated names this page fixes and resolve against the emitted `capabilities_pb.ts` `.d.ts` the runtime-action plugin emits post-approval, captured then in `.api/protoc-gen-capability-es.md`. No live call here asserts an unverified generated member; the fence is the obligation the plugin satisfies, not a confirmed binding.

```ts contract
// --- [SERVICES] ----------------------------------------------------------------------
class CapabilitySdkLive extends Effect.Service<CapabilitySdkLive>()("@rasm/ts/interchange/CapabilitySdk", {
  effect: Effect.gen(function* () {
    const { transport } = yield* WireTransportLive;
    // CommandService + CapabilityClient land from the generated `capabilities_pb.ts`;
    // member spellings resolve against the emitted `.d.ts` (see `.api/protoc-gen-capability-es.md`).
    const client = createClient(CommandService, transport);
    const catalog = Effect.tryPromise({ try: () => client.discover(), catch: faultDetailRail.fromConnect }).pipe(
      Effect.flatMap((rows) => Effect.forEach(rows, (row) => Schema.decodeUnknown(DiscoveryResultWire)(row))),
      Effect.mapError(faultDetailRail.fromConnect),
    );
    const invoke = (descriptor: string, args: Record<string, unknown>): Effect.Effect<CapabilityCommandReceiptWire, FaultDetail> =>
      Effect.tryPromise({ try: () => client.invoke(descriptor, args), catch: faultDetailRail.fromConnect }).pipe(
        Effect.flatMap((reply) => Schema.decodeUnknown(CapabilityCommandReceiptWire)(reply)),
        Effect.mapError(faultDetailRail.fromConnect),
      );
    const mcpTools = catalog.pipe(
      Effect.map((rows) =>
        rows.map((row): McpToolWire => ({
          name: row.descriptor,
          description: row.surface,
          effect: row.effect,
          idempotency: row.idempotency,
          inputSchema: client.argumentSchema(row.descriptor),
        })),
      ),
    );
    return { catalog, invoke, mcpTools } satisfies CapabilitySdk;
  }),
}) {}
```

## [4]-[TS_PROJECTION]

- Owner: the proto service-shape and transport-capability projection the transport derives — `MethodShape<K extends StreamKind, I extends string, O extends string>` keying every browser-dialable verb inside its service-shape alias, and the `TransportCapabilityWire` gating the per-protocol method set.
- Entry: every proto rpc is one `MethodShape` row inside its `ComputeServiceShape`/`DocumentServiceShape`/`ControlServiceShape`/`HealthShape` alias; `ArtifactSyncShape.sync` (bidi) and `DocumentServiceShape.captureEvents` (clientStream) are the two structurally-excluded browser methods the `grpcWeb` capability row gates; the excluded `sync` method is distinct from the `ArtifactFrameWire` frame TYPE, which is browser-reachable over the server-stream artifact-delivery path and lands on `Codec/frame.md`.
- Packages: `@bufbuild/protobuf` and `@connectrpc/connect` for the descriptor and method-shape surface.
- Growth: a new proto rpc lands as one `MethodShape` row; a new transport row lands as one `TransportCapabilityWire` literal.

```ts contract
type MethodShape<K extends StreamKind, I extends string, O extends string> = {
  readonly kind: K;
  readonly request: I;
  readonly response: O;
};

type ComputeServiceShape = {
  readonly infer: MethodShape<"unary", "InferRequest", "InferResponse">;
  readonly progress: MethodShape<"serverStream", "ProgressRequest", "ProgressUpdate">;
  readonly capabilities: MethodShape<"unary", "Empty", "ComputeCapabilities">;
  readonly solve: MethodShape<"unary", "SolveRequest", "SolveResponse">;
  readonly generate: MethodShape<"serverStream", "GenerateRequest", "TokenChunk">;
  readonly graphDiff: MethodShape<"unary", "GraphDiffRequest", "GraphDiffResponse">;
  readonly subtreeFetch: MethodShape<"serverStream", "SubtreeFetchRequest", "GraphChunk">;
};

type DocumentServiceShape = {
  readonly capabilities: MethodShape<"unary", "Empty", "DocumentCapabilities">;
  readonly documentEvents: MethodShape<"serverStream", "WatchRequest", "DocumentEvent">;
  readonly executeTransaction: MethodShape<"unary", "TransactionRequest", "TransactionReceipt">;
  readonly query: MethodShape<"unary", "QueryRequest", "QueryResponse">;
  readonly captureEvents: MethodShape<"clientStream", "CaptureFrame", "CaptureSummary">;
};

type ControlServiceShape = {
  readonly captureSupport: MethodShape<"unary", "Empty", "CaptureSupportReply">;
  readonly setDegradation: MethodShape<"unary", "SetDegradationRequest", "DegradationReply">;
  readonly reloadOptions: MethodShape<"unary", "Empty", "ReloadReply">;
};

type ArtifactSyncShape = {
  readonly sync: MethodShape<"bidi", "ArtifactFrame", "ArtifactFrame">;
};

type HealthShape = {
  readonly check: MethodShape<"unary", "HealthCheckRequest", "HealthCheckResponse">;
  readonly watch: MethodShape<"serverStream", "HealthCheckRequest", "HealthCheckResponse">;
};
```
