# [INTERCHANGE_TRANSPORT]

One page owns the outbound transport edge of the platform-neutral wire boundary — the single shared grpc-web transport, one generated client per browser-dialable service, the transport-capability shape that gates which method kinds the grpc-web row admits, and the buf descriptor pipeline that is the rail's build-time input edge. The owning C# `#TS_PROJECTION` fence is the authoritative wire shape; this page names which client derives from which service descriptor and fixes the codegen tooling as the transport owner's input, never re-authoring a shape. The descriptor pipeline is a cluster on this page, not a fourth file: its output is `src/gen/*_pb.ts`, the values the transport composes.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]             | [OWNS]                                                                  |
| :-----: | :-------------------- | :---------------------------------------------------------------------- |
|   [1]   | TRANSPORT_AND_CLIENTS | one shared transport, one client per service, the capability shape      |
|   [2]   | CODEGEN_TOOLING       | the committed-descriptor buf pipeline as the transport input            |
|   [3]   | TS_PROJECTION         | the proto service shapes and transport-capability the transport derives |

## [2]-[TRANSPORT_AND_CLIENTS]

- Owner: `WireTransport`, the single shared transport `Effect.Service`, plus `WireClients`, one generated `Client<typeof GenService>` per browser-dialable service built over it; clients construct only through the descriptor generator the CODEGEN_TOOLING cluster fixes, never as hand-shaped message literals. `TransportCapabilityWire` is the two-key method-kind-tuple axis (transcribed verbatim from the C# wire) fixing which method kinds each transport admits — the grpcWeb tuple lists the two browser-carried kinds, the http2 tuple all four.
- Cases: the four browser-dialable generated services over one transport; a unary call resolves by await, a server-stream consumes by async iteration; the client-stream capture lane (`DocumentServiceShape.captureEvents`) and the bidirectional artifact-sync lane (`ArtifactSyncShape.sync`) are structurally excluded on grpcWeb, mirrored from `remote-lane.md#TS_PROJECTION` where the transport-capability tuple admits only `unary` and `serverStream` on the grpcWeb row while the http2 row admits all four — the exclusion is read from the wire, never invented branch-side. The connect-es `Client<typeof DocumentService>` STILL surfaces `captureEvents` on its generated type (connect-es projects every service method onto the client face regardless of stream kind), so the clientStream method is PRESENT on the client type but FAULTED by the capability gate at dial time, never absent from the generated client — the structural exclusion is a runtime capability-row gate, not a type-level method removal.
- Entry: outbound calls cross one transport whose interceptor stamp axis is polymorphic over the correlation identifier, the trace parent, and the bearer credential, mirroring the `CallSpine.CorrelationKey`/`TraceparentKey` constants named on `remote-lane.md#TS_PROJECTION`; the interceptor is a connect-es async interceptor over a captured `Effect.runtime` snapshot taken once at service construction, never a generator — a single `Runtime.runPromise` per call resolves the live token producer (`AuthSession.tokenHeader`, the `Option<string>` full `Bearer` header value) and the active span in one effect, so a token cached past expiry never ships and the W3C `traceparent` is authored from the runtime-resolved span context, never a per-call double promise round-trip; the credential row is designed-only growth activating with the cross-origin deployment exactly as the C# `CredentialPolicy.Bearer` row gates the per-call mint; per-call cancellation threads interruption into transport cancellation through the call signal; one interceptor stamps all three header rows in one pass, never three parallel interceptors.
- Packages: `@connectrpc/connect` for `createClient`, `@connectrpc/connect-web` for `createGrpcWebTransport`, `@bufbuild/protobuf` for the descriptor runtime, and `effect` for the transport-as-`Effect.Service` composition.
- Frames: the `ArtifactFrameStreaming` row is the transport-level chunked framing fold over the same shared transport — NOT the content-addressed reassembly (`codec-rails.md#CODEC_RAILS` `ArtifactFrameRail` owns the Crc32-verify-and-stitch). Down: the server-stream artifact-delivery path is the `serverStream` method set the C# wire actually declares — `ComputeServiceShape.generate` (`serverStream`→`TokenChunk`) and `ComputeServiceShape.subtreeFetch` (`serverStream`→`GraphChunk`) — whose chunk frames are lifted into an `Effect`-native `Stream.Stream<ArtifactFrameWire, FaultDetail>` through `Stream.fromAsyncIterable` over the connect-es server-stream async iterator, with backpressure flowing from the consumer through the iterator's natural pull (each `next()` is one network read, never pre-buffered) and a `Stream.buffer({ capacity, strategy: "suspend" })` bound holding at most one 64-KiB frame window in flight so a slow `DecodeWorkerPool` consumer suspends the wire read instead of growing an unbounded queue. `solve` is `unary`→`SolveResponse` on the wire and carries no server-stream frame leg — the branch never invents an `ArtifactFrame` server-stream contract the C# method kinds do not declare. Up: the `DocumentServiceShape.captureEvents` clientStream is structurally excluded on grpcWeb (the capability row gates it), so the unary-chunked UPLOAD leg frames a large outbound artifact as a bounded `Stream` of `ArtifactFrameWire` chunks driven through repeated UNARY calls — `Stream.runForEach` over a `Stream.fromChunks` of fixed 64-KiB-offset frames, each one unary `await`, with `Effect.zipRight` sequencing so an upload chunk N+1 never ships before chunk N's unary reply, the backpressure being the per-call await itself. The framing fold is one polymorphic surface keyed by direction, never two parallel framers; the frame TYPE is `ArtifactFrameWire` decoded verbatim from `remote-lane.md#TS_PROJECTION`, never re-authored.
- Growth: a new browser-dialable service lands as one generated client row over the same transport; a new remote verb lands as one generated method row; a new chunked direction lands as one `FrameDirection` case on the existing framing fold; a cross-origin deployment lands the credential and CORS rows as designed-only growth mirrored from the C# boundary.
- Boundary: the transport is same-origin under the co-hosted topology and configures no cross-origin header; the excluded client-stream and bidi lanes carry no browser path because the C# boundary excludes them, never because the branch invents an exclusion; `AuthSession` is consumed as a per-call token producer from `platform` and never owned here, so `interchange` declares no OIDC dependency; the chunked-framing fold produces the `Stream<ArtifactFrameWire, FaultDetail>` the codec rail consumes and never itself reassembles, verifies Crc32, or derives a content key — that is `ArtifactFrameRail`'s owned surface, never duplicated branch-side.

```ts contract
type StreamKind = "unary" | "serverStream" | "clientStream" | "bidi";

// transcribed verbatim from remote-lane.md#TS_PROJECTION line 476: a two-key object keyed by transport name
// whose value is the method-kind tuple that transport admits — NOT a row-per-protocol record. The grpcWeb tuple
// lists exactly the two kinds the binary grpc-web frame carries; the http2 tuple lists all four. The structural
// clientStream/bidi exclusion on the browser is read from this tuple, never invented branch-side.
const TransportCapabilityWire = Schema.Struct({
  http2: Schema.Tuple(Schema.Literal("unary"), Schema.Literal("serverStream"), Schema.Literal("clientStream"), Schema.Literal("bidi")),
  grpcWeb: Schema.Tuple(Schema.Literal("unary"), Schema.Literal("serverStream")),
});
type TransportCapabilityWire = Schema.Schema.Type<typeof TransportCapabilityWire>;

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
    const runtime = yield* Effect.runtime<never>();
    // the stamp resolves the LIVE per-call span AND the live token in one effect, run per call — Effect.currentSpan
    // is read inside the call-time fiber (not captured at construction), so traceparent reflects the request's own
    // span context rather than a constant construction-time span, and a token cached past expiry never ships.
    const stampEffect = Effect.all({ token: session.tokenHeader, span: Effect.currentSpan.pipe(Effect.option) });
    const interceptor: Interceptor = (next) => async (req) => {
      if (!req.header.has("rasm-correlation")) req.header.set("rasm-correlation", crypto.randomUUID());
      const { token, span } = await Runtime.runPromise(runtime)(stampEffect);
      Option.match(span, { onNone: () => {}, onSome: (s) => req.header.set("traceparent", `00-${s.traceId}-${s.spanId}-01`) });
      Option.match(token, { onNone: () => {}, onSome: (t) => req.header.set("authorization", t) });
      return next(req);
    };
    const transport = createGrpcWebTransport({ baseUrl, interceptors: [interceptor] });
    return { transport, interceptor } satisfies WireTransport;
  }),
}) {}
```

The chunked-framing fold rides this same transport. It is one polymorphic surface over a `FrameDirection` `Data.TaggedEnum` — the server-stream DOWN leg lifts the connect-es async iterator into a backpressured `Stream<ArtifactFrameWire, FaultDetail>`, the unary-chunked UP leg drives a bounded outbound `Stream` of 64-KiB frames through repeated unary calls. The reassembly, Crc32 verification, and content-key derivation are NOT here — they are `codec-rails.md#CODEC_RAILS` `ArtifactFrameRail`; this fold owns only the transport-level chunk boundary and backpressure.

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

// fault projection and CRC are OWNED on codec-rails and consumed here as the concrete exported values — fold
// connect-es failures through `faultDetailRail.fromConnect` (its one polymorphic cause→FaultDetail entry) and
// compute the per-frame frameCrc through the `crc32` rail, never a free crc32Of that re-owns CRC the
// ArtifactFrameRail/Crc32 owner already holds. The rails are pure values, so no service injection is needed.
class ArtifactFrameStreamingLive extends Effect.Service<ArtifactFrameStreamingLive>()("@rasm/ts/interchange/ArtifactFrameStreaming", {
  sync: () => {
    const frame = (direction: FrameDirection): Stream.Stream<ArtifactFrameWire, FaultDetail> =>
      FrameDirection.$match(direction, {
        // down: one network read per pull; suspend the wire when the worker pool lags, never pre-buffer
        Down: ({ call }) =>
          Stream.fromAsyncIterable(call(), faultDetailRail.fromConnect).pipe(
            Stream.buffer({ capacity: 1, strategy: "suspend" }),
          ),
        // up: re-chunk the source bytes at FRAME_BYTES and ship each as one unary call, sequenced; the await is the backpressure
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
// splitFrames sources the per-frame CRC from the codec-rails Crc32 owner (passed in), never a free crc32Of.
// state carries [remaining-buffer, absolute-offset]; artifactBytes is the FULL original length on every frame,
// offset advances by the bytes consumed this step.
const splitFrames = (crc: Crc32, artifactId: string, bytes: Uint8Array): Chunk.Chunk<ArtifactFrameWire> =>
  Chunk.unfold([bytes, 0] as const, ([buf, offset]) =>
    buf.length === 0
      ? Option.none()
      : Option.some([
          { artifactId, artifactBytes: bytes.byteLength, offset, frameCrc: crc.of(buf.subarray(0, FRAME_BYTES)), payload: buf.subarray(0, FRAME_BYTES) },
          [buf.subarray(FRAME_BYTES), offset + Math.min(FRAME_BYTES, buf.length)] as const,
        ]),
  );
```

## [3]-[CODEGEN_TOOLING]

- Owner: the descriptor pipeline as the transport owner's build-time input edge — the committed app-root `FileDescriptorSet` input, the single-plugin `buf.gen.yaml` config, and the `src/gen/*_pb.ts` output the transport composes. There is no `codegen.ts` runtime module; the output is generated code, so the pipeline is the input edge of `transport.ts`.
- Cases: descriptor ingestion reads the app-root-emitted `FileDescriptorSet` published beside the discovery manifest by the C# `ContractGuard`, so the branch consumes the same descriptor set the .NET side emits and never re-authors a `.proto`; the generation pass runs the single `protoc-gen-es` v2 plugin with `target=ts` and `include_imports: true`; the descriptor runtime composes the generated values — message construction through `create`, binary through `fromBinary`/`toBinary`, and the file-aware registry from the emitted `FileDescriptorSet` the fault rail passes to `findDetails`; client derivation runs `createClient(service, transport)` per browser-dialable service descriptor.
- Entry: `buf generate` is the build-time driver — a tool surface, never a runtime import — and the generated `*_pb.ts` modules are the runtime values `WireClients` derives from; the `MethodShape` aliases in the inventory mirror the descriptor `methodKind` one-for-one, so a method-shape row and its generated descriptor never diverge; unknown fields are retained by the descriptor runtime by default, the additive-drift tolerance the versioning law relies on.
- Packages: `@bufbuild/protoc-gen-es` for the single-plugin codegen, `@bufbuild/buf` (`allowBuilds: true`) for the build-time CLI driver, and `@bufbuild/protobuf` for the runtime descriptor values.
- Growth: a new wire service lands one generated descriptor and one `createClient` row; a descriptor-set refresh regenerates `src/gen` and is the only edit to it; `@connectrpc/protoc-gen-connect-es` is the rejected separate-plugin form and a hand-edited generated module is the deleted form; the capability-descriptor SDK codegen leg lands as a SECOND plugin row on the same single-config pipeline gated on the C# `capability-registry#SDK_CODEGEN` descriptor source — never a parallel `buf.gen.yaml`, never a hand-maintained capability client.
- Boundary: the pipeline emits `src/gen/*_pb.ts` and never a hand-copied `.proto` tree; `buf.gen.yaml` carries the proto-message plugin plus the capability-SDK plugin and never the rejected message-plus-service split; the descriptor registry resolves the full type graph from one module because `include_imports` embeds the transitive descriptors; the capability SDK leg reads ONLY the C#-emitted `CapabilityDescriptorWire[]` catalog descriptor — the TS branch never re-authors a capability shape, never hand-writes a command method, and never reaches a C# interior, consuming the generated descriptor exactly as the proto leg consumes the `FileDescriptorSet`.

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

The capability-SDK leg is the SECOND plugin on the SAME pipeline, not a runtime module: it consumes the C# `capability-registry#SDK_CODEGEN` `DiscoveryResultWire[]` catalog descriptor (the `capability-registry#TS_PROJECTION` `CapabilityDescriptorWire`/`CapabilityCommandReceiptWire`/`DiscoveryResultWire` shapes the C# `SdkTarget.Typescript` renderer emits) and generates `src/gen/capabilities_pb.ts` — a typed-per-descriptor effect-classed command surface plus one MCP client leg, both DERIVED, never hand-shaped. The generated descriptor row is transcribed below as the SHAPE the TS branch consumes; the branch reads the descriptor, never re-authors the catalog.

```ts contract
// --- [TYPES] -------------------------------------------------------------------------
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

The generated `capabilities_pb.ts` SDK surface the leg emits is a typed command face over the `CommandService` wire verb — ONE polymorphic `invoke` method keyed by the descriptor id, never a sibling method per descriptor and never a hand-written client; the MCP client leg is the same surface re-projected as the MCP tool descriptor list the host advertises. The branch consumes this generated surface; it is authored by the plugin off the C# catalog.

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

class CapabilitySdkLive extends Effect.Service<CapabilitySdkLive>()("@rasm/ts/interchange/CapabilitySdk", {
  effect: Effect.gen(function* () {
    // the generated capabilities_pb.ts CommandService client over the same shared transport — derived, never hand-maintained
    const command = yield* CapabilityClient;
    // the codec-rails `faultDetailRail` concrete value owns the cause→FaultDetail fold (its one polymorphic
    // fromConnect); the fold-to-FaultDetail symbol is never a static namespace and never re-authored per call site.
    const decodeCatalog = Schema.decodeUnknown(Schema.Array(DiscoveryResultWire));
    const decodeReceipt = Schema.decodeUnknown(CapabilityCommandReceiptWire);
    const catalog = Effect.tryPromise({ try: () => command.discover({}), catch: faultDetailRail.fromConnect }).pipe(
      Effect.flatMap((reply) => decodeCatalog(reply.descriptors).pipe(Effect.mapError(faultDetailRail.fromConnect))),
    );
    const invoke = (descriptor: string, args: Record<string, unknown>) =>
      Effect.tryPromise({ try: () => command.invoke({ descriptor, arguments: toStruct(args) }), catch: faultDetailRail.fromConnect }).pipe(
        Effect.flatMap((reply) => decodeReceipt(reply.receipt).pipe(Effect.mapError(faultDetailRail.fromConnect))),
      );
    // inputSchema is the descriptor's REAL generated argument schema read from the generated SDK — capabilities_pb.ts
    // emits per-descriptor the JSON Schema the C# capability-registry#SDK_CODEGEN JsonSchemaExporter produces
    // (CommandArguments resolved through SuiteContracts.Schema, one schema digest per descriptor across all three
    // targets), never a hand-built {type:"object"} stub that degrades the wire shape. The catalog row carries no
    // schema field (DiscoveryResultWire is the C# catalog shape verbatim); the schema rides the generated method.
    const mcpTools = catalog.pipe(Effect.map(Array.map((d) => ({
      name: d.descriptor,
      description: `${d.surface} (effect=${d.effect}, idempotency=${d.idempotency}, scope=${d.scopeHash})`,
      effect: d.effect,
      idempotency: d.idempotency,
      inputSchema: command.argumentSchema(d.descriptor),
    } satisfies McpToolWire))));
    return { catalog, invoke, mcpTools } satisfies CapabilitySdk;
  }),
  dependencies: [WireTransportLive.Default],
}) {}
```

`CapabilityClient` is the generated `Client<typeof CommandService>` the capability-SDK codegen leg emits into `capabilities_pb.ts` — one more `createClient(CommandService, transport)` row over the SAME shared `WireTransport`, never a second transport and never a hand-written client; `command.discover`/`command.invoke` are the two generated CommandService verbs the C# `capability-registry#SDK_CODEGEN` descriptor source projects, and `command.argumentSchema(descriptor)` is the generated per-descriptor JSON-Schema accessor the codegen leg emits beside them (the `JsonSchemaExporter` schema the C# renderer binds one-per-descriptor, identical digest across all three SDK targets), so `mcpTools` projects the descriptor's real argument contract; the TS branch reads all three from the generated module exactly as `WireClients` reads its four service clients.

## [4]-[TS_PROJECTION]

- Owner: the proto service-shape and transport-capability projection the transport derives — `MethodShape<K extends StreamKind, I extends string, O extends string>` keying every browser-dialable verb inside its service-shape alias, and the `TransportCapabilityWire` gating the grpcWeb method set.
- Entry: every proto rpc is one `MethodShape` row inside its `ComputeServiceShape`/`DocumentServiceShape`/`ControlServiceShape`/`HealthShape` alias; `ArtifactSyncShape.sync` (bidi) and `DocumentServiceShape.captureEvents` (clientStream) are the two structurally-excluded browser methods the capability row gates; the excluded `sync` method is distinct from the `ArtifactFrameWire` frame TYPE, which is browser-reachable over the server-stream artifact-delivery path and lands on `codec-rails.md#CODEC_RAILS` `ArtifactFrameRail`.
- Packages: `@bufbuild/protobuf` and `@connectrpc/connect` for the descriptor and method-shape surface.
- Growth: a new proto rpc lands as one `MethodShape` row; a new transport row lands as one `TransportCapabilityWire` literal.
- Boundary: the projection transcribes the C# `remote-lane.md#TS_PROJECTION` fence verbatim; the branch authors no service shape and no capability literal absent from that fence.

```ts contract
// transcribed verbatim from remote-lane.md#TS_PROJECTION lines 474-488 — member names request/response (NOT
// input/output), the five service method-shape aliases field-for-field, and TransportFramingWire's two-key
// frame-mode object. The branch authors no output type name and drops no method row; the wire is the authority.
type MethodShape<K extends StreamKind, I extends string, O extends string> = {
  readonly kind: K;
  readonly request: I;
  readonly response: O;
};

interface TransportFramingWire {
  readonly http2: { readonly mode: "binary"; readonly carries: ["unary", "serverStream", "clientStream", "bidi"] };
  readonly grpcWeb: { readonly mode: "binary"; readonly mediaType: "application/grpc-web"; readonly carries: ["unary", "serverStream"] };
}

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
