# [INTERCHANGE_TRANSPORT]

One page owns the outbound transport edge of the platform-neutral wire boundary — the single shared grpc-web transport, one generated client per browser-dialable service, the transport-capability shape that gates which method kinds the grpc-web row admits, and the buf descriptor pipeline that is the rail's build-time input edge. The owning C# `#TS_PROJECTION` fence is the authoritative wire shape; this page names which client derives from which service descriptor and fixes the codegen tooling as the transport owner's input, never re-authoring a shape. The descriptor pipeline is a cluster on this page, not a fourth file: its output is `src/gen/*_pb.ts`, the values the transport composes.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]             | [OWNS]                                                          |
| :-----: | :-------------------- | :-------------------------------------------------------------- |
|   [1]   | TRANSPORT_AND_CLIENTS | one shared transport, one client per service, the capability shape |
|   [2]   | CODEGEN_TOOLING       | the committed-descriptor buf pipeline as the transport input    |
|   [3]   | TS_PROJECTION         | the proto service shapes and transport-capability the transport derives |

## [2]-[TRANSPORT_AND_CLIENTS]

- Owner: `WireTransport`, the single shared transport `Effect.Service`, plus `WireClients`, one generated `Client<typeof GenService>` per browser-dialable service built over it; clients construct only through the descriptor generator the CODEGEN_TOOLING cluster fixes, never as hand-shaped message literals. `TransportCapabilityWire` is the `Schema.Literal` axis fixing which method kinds each transport admits.
- Cases: the four browser-dialable generated services over one transport; a unary call resolves by await, a server-stream consumes by async iteration; the client-stream capture lane (`DocumentServiceShape.captureEvents`) and the bidirectional artifact-sync lane (`ArtifactSyncShape.sync`) carry no browser client, mirrored from `remote-lane.md#TS_PROJECTION` where the transport-capability shape admits only `unary` and `serverStream` on the grpcWeb row while the http2 row admits all four — the structural exclusion is read from the wire, never invented branch-side.
- Entry: outbound calls cross one transport whose interceptor stamp axis is polymorphic over the correlation identifier, the trace parent, and the bearer credential, mirroring the `CallSpine.CorrelationKey`/`TraceparentKey` constants named on `remote-lane.md#TS_PROJECTION`; the interceptor is a connect-es async interceptor over a captured `Effect.runtime` snapshot taken once at service construction, never a generator — a single `Runtime.runPromise` per call resolves the live token producer (`AuthSession.tokenHeader`, the `Option<string>` full `Bearer` header value) and the active span in one effect, so a token cached past expiry never ships and the W3C `traceparent` is authored from the runtime-resolved span context, never a per-call double promise round-trip; the credential row is designed-only growth activating with the cross-origin deployment exactly as the C# `CredentialPolicy.Bearer` row gates the per-call mint; per-call cancellation threads interruption into transport cancellation through the call signal; one interceptor stamps all three header rows in one pass, never three parallel interceptors.
- Packages: `@connectrpc/connect` for `createClient`, `@connectrpc/connect-web` for `createGrpcWebTransport`, `@bufbuild/protobuf` for the descriptor runtime, and `effect` for the transport-as-`Effect.Service` composition.
- Growth: a new browser-dialable service lands as one generated client row over the same transport; a new remote verb lands as one generated method row; a cross-origin deployment lands the credential and CORS rows as designed-only growth mirrored from the C# boundary.
- Boundary: the transport is same-origin under the co-hosted topology and configures no cross-origin header; the excluded client-stream and bidi lanes carry no browser path because the C# boundary excludes them, never because the branch invents an exclusion; `AuthSession` is consumed as a per-call token producer from `@rasm/web` and never owned here, so `@rasm/interchange` declares no OIDC dependency.

```ts contract
type StreamKind = "unary" | "serverStream" | "clientStream" | "bidi";

const TransportCapabilityWire = Schema.Struct({
  protocol: Schema.Literal("http2", "grpcWeb"),
  admits: Schema.Array(Schema.Literal("unary", "serverStream", "clientStream", "bidi")),
});
type TransportCapabilityWire = Schema.Schema.Type<typeof TransportCapabilityWire>;

const grpcWebCapability: TransportCapabilityWire = { protocol: "grpcWeb", admits: ["unary", "serverStream"] };

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

class WireTransportLive extends Effect.Service<WireTransportLive>()("@rasm/interchange/WireTransport", {
  effect: Effect.gen(function* () {
    const config = yield* RuntimeConfig;
    const session = yield* AuthSession;
    const baseUrl = yield* config.apiBaseUrl;
    const runtime = yield* Effect.runtime<never>();
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

## [3]-[CODEGEN_TOOLING]

- Owner: the descriptor pipeline as the transport owner's build-time input edge — the committed app-root `FileDescriptorSet` input, the single-plugin `buf.gen.yaml` config, and the `src/gen/*_pb.ts` output the transport composes. There is no `codegen.ts` runtime module; the output is generated code, so the pipeline is the input edge of `transport.ts`.
- Cases: descriptor ingestion reads the app-root-emitted `FileDescriptorSet` published beside the discovery manifest by the C# `ContractGuard`, so the branch consumes the same descriptor set the .NET side emits and never re-authors a `.proto`; the generation pass runs the single `protoc-gen-es` v2 plugin with `target=ts` and `include_imports: true`; the descriptor runtime composes the generated values — message construction through `create`, binary through `fromBinary`/`toBinary`, and the file-aware registry from the emitted `FileDescriptorSet` the fault rail passes to `findDetails`; client derivation runs `createClient(service, transport)` per browser-dialable service descriptor.
- Entry: `buf generate` is the build-time driver — a tool surface, never a runtime import — and the generated `*_pb.ts` modules are the runtime values `WireClients` derives from; the `MethodShape` aliases in the inventory mirror the descriptor `methodKind` one-for-one, so a method-shape row and its generated descriptor never diverge; unknown fields are retained by the descriptor runtime by default, the additive-drift tolerance the versioning law relies on.
- Packages: `@bufbuild/protoc-gen-es` for the single-plugin codegen, `@bufbuild/buf` (`allowBuilds: true`) for the build-time CLI driver, and `@bufbuild/protobuf` for the runtime descriptor values.
- Growth: a new wire service lands one generated descriptor and one `createClient` row; a descriptor-set refresh regenerates `src/gen` and is the only edit to it; `@connectrpc/protoc-gen-connect-es` is the rejected separate-plugin form and a hand-edited generated module is the deleted form; the capability-descriptor codegen stage is a designed-only growth row gated on the C# `capability-registry#CAPABILITY_CATALOG` descriptor.
- Boundary: the pipeline emits `src/gen/*_pb.ts` and never a hand-copied `.proto` tree; `buf.gen.yaml` carries one plugin and never the rejected message-plus-service split; the descriptor registry resolves the full type graph from one module because `include_imports` embeds the transitive descriptors.

```yaml
version: v2
inputs:
  - directory: gen/descriptors
plugins:
  - local: protoc-gen-es
    out: src/gen
    include_imports: true
    opt: target=ts
```

## [4]-[TS_PROJECTION]

- Owner: the proto service-shape and transport-capability projection the transport derives — `MethodShape<K extends StreamKind, I extends string, O extends string>` keying every browser-dialable verb inside its service-shape alias, and the `TransportCapabilityWire` gating the grpcWeb method set.
- Entry: every proto rpc is one `MethodShape` row inside its `ComputeServiceShape`/`DocumentServiceShape`/`ControlServiceShape`/`HealthShape` alias; `ArtifactSyncShape.sync` (bidi) and `DocumentServiceShape.captureEvents` (clientStream) are the two structurally-excluded browser methods the capability row gates; the excluded `sync` method is distinct from the `ArtifactFrameWire` frame TYPE, which is browser-reachable over the server-stream artifact-delivery path and lands on `codec-rails.md#CODEC_RAILS` `ArtifactFrameRail`.
- Packages: `@bufbuild/protobuf` and `@connectrpc/connect` for the descriptor and method-shape surface.
- Growth: a new proto rpc lands as one `MethodShape` row; a new transport row lands as one `TransportCapabilityWire` literal.
- Boundary: the projection transcribes the C# `remote-lane.md#TS_PROJECTION` fence verbatim; the branch authors no service shape and no capability literal absent from that fence.

```ts contract
type MethodShape<K extends StreamKind, I extends string, O extends string> = {
  readonly kind: K;
  readonly input: I;
  readonly output: O;
};

interface ComputeServiceShape {
  readonly infer: MethodShape<"unary", "InferRequest", "InferReply">;
  readonly progress: MethodShape<"serverStream", "ProgressRequest", "ProgressMark">;
  readonly capabilities: MethodShape<"unary", "CapabilityRequest", "CapabilityReply">;
  readonly solve: MethodShape<"serverStream", "SolveRequest", "ArtifactFrame">;
  readonly generate: MethodShape<"serverStream", "GenerateRequest", "ArtifactFrame">;
}

interface ControlServiceShape {
  readonly captureSupport: MethodShape<"unary", "CaptureSupportRequest", "SupportReceipt">;
  readonly setDegradation: MethodShape<"unary", "DegradationRequest", "DegradationWire">;
  readonly reloadOptions: MethodShape<"unary", "ReloadRequest", "ReloadReceipt">;
}
```
