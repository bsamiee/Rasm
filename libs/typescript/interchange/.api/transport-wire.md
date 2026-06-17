# [API_TRANSPORT_WIRE]

Dependency catalogue for the `transport-wire` package set. Surfaces are grounded from published `.d.ts` declarations. Owner-symbol consumers: `WireTransport`, `DecodeRail`, `GeometryRail`, `FaultDetailRail`, `QuarantineFold` (wire-contracts); `SnapshotFeed` (state-stores); `CompositionRoot` (runtime-host).

---

## [@connectrpc/connect]

### Transport

```typescript
interface Transport {
  unary<I extends DescMessage, O extends DescMessage>(
    method: DescMethodUnary<I, O>,
    signal: AbortSignal | undefined,
    timeoutMs: number | undefined,
    header: HeadersInit | undefined,
    input: MessageInitShape<I>,
    contextValues?: ContextValues,
  ): Promise<UnaryResponse<I, O>>;

  stream<I extends DescMessage, O extends DescMessage>(
    method: DescMethodStreaming<I, O>,
    signal: AbortSignal | undefined,
    timeoutMs: number | undefined,
    header: HeadersInit | undefined,
    input: AsyncIterable<MessageInitShape<I>>,
    contextValues?: ContextValues,
  ): Promise<StreamResponse<I, O>>;
}
```

Single shared interface for both unary and streaming RPC legs; `WireTransport` holds one instance.

### Interceptor

```typescript
type Interceptor = (next: AnyFn) => AnyFn;
```

Middleware wrapping each transport call. The correlation-stamp interceptor on `WireTransport` stamps `rasm-correlation` and `traceparent` headers here.

### createClient

```typescript
function createClient<T extends DescService>(
  service: T,
  transport: Transport,
): Client<T>;
```

Builds one typed client per browser-dialable service over the shared `WireTransport`. Return type `Client<T>` maps each method to its call shape: `unary` → `Promise`, `server_streaming` → `AsyncIterable`.

### CallOptions

```typescript
interface CallOptions {
  timeoutMs?: number;
  headers?: HeadersInit;
  signal?: AbortSignal;        // threads Effect interruption into transport cancellation
  onHeader?: (headers: Headers) => void;
  onTrailer?: (trailers: Headers) => void;
  contextValues?: ContextValues;
}
```

Per-call configuration. `signal` is the load-bearing field for Effect interruption threading.

### UnaryRequest / UnaryResponse

```typescript
interface UnaryRequest<I extends DescMessage = DescMessage, O extends DescMessage = DescMessage>
  extends RequestCommon {
  readonly stream: false;
  readonly message: MessageShape<I>;
  readonly method: DescMethodUnary<I, O>;
}

interface UnaryResponse<I extends DescMessage = DescMessage, O extends DescMessage = DescMessage>
  extends ResponseCommon {
  readonly stream: false;
  readonly message: MessageShape<O>;
  readonly method: DescMethodUnary<I, O>;
}
```

### StreamRequest / StreamResponse

```typescript
interface StreamRequest<I extends DescMessage = DescMessage, O extends DescMessage = DescMessage>
  extends RequestCommon {
  readonly stream: true;
  readonly message: AsyncIterable<MessageShape<I>>;
  readonly method: DescMethodStreaming<I, O>;
}

interface StreamResponse<I extends DescMessage = DescMessage, O extends DescMessage = DescMessage>
  extends ResponseCommon {
  readonly stream: true;
  readonly message: AsyncIterable<MessageShape<O>>;
  readonly method: DescMethodStreaming<I, O>;
}
```

`StreamResponse.message` is the `AsyncIterable` consumed as `for await` in server-stream calls. `StreamRequest.message` exists structurally but carries no browser client (client-stream and bidi are excluded on the browser row).

### ConnectError

```typescript
class ConnectError extends Error {
  readonly code: Code;
  readonly metadata: Headers;
  details: (OutgoingDetail | IncomingDetail)[];
  readonly rawMessage: string;
  cause: unknown;

  constructor(
    message: string,
    code?: Code,
    metadata?: HeadersInit,
    outgoingDetails?: OutgoingDetail[],
    cause?: unknown,
  );

  static from(reason: unknown, code?: Code): ConnectError;

  findDetails<Desc extends DescMessage>(desc: Desc): MessageShape<Desc>[];
  findDetails(registry: Registry): Message[];
}
```

`findDetails` with the generated `FaultDetail` descriptor is the `FaultDetailRail` entry point — it extracts `google.rpc.Status` details riding the `grpc-status-details-bin` trailer.

---

## [@connectrpc/connect-web]

### createGrpcWebTransport

```typescript
function createGrpcWebTransport(options: GrpcWebTransportOptions): Transport;
```

Sole factory for `WireTransport`. Returns a `Transport` for same-origin gRPC-Web calls over `fetch`.

### GrpcWebTransportOptions

```typescript
interface GrpcWebTransportOptions {
  baseUrl: string;             // same-origin under co-hosted topology
  useBinaryFormat?: boolean;   // default true — binary wire, never text mode
  interceptors?: Interceptor[];
  fetch?: typeof globalThis.fetch;
  defaultTimeoutMs?: number;
  jsonOptions?: Partial<JsonReadOptions & JsonWriteOptions>;
  binaryOptions?: Partial<BinaryReadOptions & BinaryWriteOptions>;
}
```

`baseUrl` is the co-hosted same-origin; no CORS credential row is needed day-one. `interceptors` carries the correlation-stamp interceptor. `useBinaryFormat` stays `true` (default) — the text mode never enters.

---

## [@bufbuild/protobuf]

### create

```typescript
function create<Desc extends DescMessage>(
  schema: Desc,
  init?: MessageInitShape<Desc>,
): MessageShape<Desc>;
```

Canonical message constructor. Generated messages are never constructed as object literals.

### toBinary / fromBinary

```typescript
function toBinary<Desc extends DescMessage>(
  schema: Desc,
  message: MessageShape<Desc>,
  options?: Partial<BinaryWriteOptions>,
): Uint8Array<ArrayBuffer>;

function fromBinary<Desc extends DescMessage>(
  schema: Desc,
  bytes: Uint8Array,
  options?: Partial<BinaryReadOptions>,
): MessageShape<Desc>;
```

Schema-anchored binary serialisation. Used by `DecodeRail` for proto-framed payloads; unknown fields are retained by default.

### DescMessage / DescService / DescMethod

```typescript
interface DescMessage {
  readonly kind: "message";
  readonly typeName: string;
  readonly name: string;
  readonly file: DescFile;
  readonly fields: DescField[];
  readonly field: Record<string, DescField>;
  readonly oneofs: DescOneof[];
  readonly members: (DescField | DescOneof)[];
  readonly nestedEnums: DescEnum[];
  readonly nestedMessages: DescMessage[];
  readonly deprecated: boolean;
  toString(): string;
}

interface DescService {
  readonly kind: "service";
  readonly typeName: string;
  readonly name: string;
  readonly file: DescFile;
  readonly methods: DescMethod[];
  readonly method: Record<string, DescMethod>;
  readonly deprecated: boolean;
  toString(): string;
}

interface DescMethod {
  readonly kind: "rpc";
  readonly name: string;
  readonly localName: string;
  readonly parent: DescService;
  readonly methodKind: "unary" | "server_streaming" | "client_streaming" | "bidi_streaming";
  readonly input: DescMessage;
  readonly output: DescMessage;
  readonly deprecated: boolean;
  toString(): string;
}
```

### MessageShape / MessageInitShape

```typescript
// Inferred from generated descriptor Desc; resolves to the runtime message instance type.
type MessageShape<Desc extends DescMessage> = /* runtime shape from Desc */;

// Accepted by create(); all fields optional except $typeName and $unknown.
type MessageInitShape<Desc extends DescMessage> = MessageInit<MessageShape<Desc>>;
```

These are the generic type-level connectives threading descriptor types through `create`, `toBinary`, `fromBinary`, and `createClient`.

### Registry / createRegistry / createFileRegistry

```typescript
interface Registry {
  readonly kind: "registry";
  [Symbol.iterator](): Iterator<DescMessage | DescEnum | DescExtension | DescService>;
  get(typeName: string): DescMessage | DescEnum | DescExtension | DescService | undefined;
  getMessage(typeName: string): DescMessage | undefined;
  getEnum(typeName: string): DescEnum | undefined;
  getExtension(typeName: string): DescExtension | undefined;
  getService(typeName: string): DescService | undefined;
}

function createRegistry(...inputs: (DescMessage | DescEnum | DescExtension | DescService | DescFile | Registry)[]): Registry;

// Overloads — builds a file-aware registry from a FileDescriptorSet message or FileDescriptorProto.
function createFileRegistry(fileDescriptorSet: FileDescriptorSet): FileRegistry;
function createFileRegistry(file: FileDescriptorProto, resolve: (protoFileName: string) => FileDescriptorProto | undefined): FileRegistry;
function createFileRegistry(...registries: FileRegistry[]): FileRegistry;
```

`createFileRegistry` is the production path from the app-root-emitted `FileDescriptorSet` to the `Registry` passed to `ConnectError.findDetails` in `FaultDetailRail`.

---

## [@bufbuild/buf — codegen pipeline]

The buf CLI is a build-time toolchain surface, not a runtime import. The load-bearing configuration shape:

```yaml
# buf.gen.yaml (v2)
version: v2
plugins:
  - local: protoc-gen-es        # single plugin — messages + service descriptors together
    out: src/gen
    include_imports: true       # embed all imported descriptors in generated output
    opt: target=ts              # emit .ts, not .js/.d.ts split
```

`@connectrpc/protoc-gen-connect-es` is the rejected separate-plugin form; `protoc-gen-es` alone emits `*_pb.ts` with one `GenService` per service carrying `methodKind`, `input`, and `output` on every rpc. Codegen input is the app-root-emitted descriptor set published beside the discovery manifest by `ContractGuard`; hand-copied `.proto` files are the deleted form.

---

## [msgpackr]

```typescript
class Unpackr {
  constructor(options?: Options);
  unpack(messagePack: Buffer | Uint8Array, options?: UnpackOptions): any;
  decode(messagePack: Buffer | Uint8Array, options?: UnpackOptions): any;
  unpackMultiple(messagePack: Buffer | Uint8Array): any[];
}

class Decoder extends Unpackr {}   // alias — canonical name for DecodeRail instances

interface Options {
  int64AsType?: 'bigint' | 'number' | 'string';  // 'bigint' for snapshot 64-bit fields
  useBigInt64?: boolean;           // deprecated alias; prefer int64AsType: 'bigint'
  moreTypes?: boolean;
  useRecords?: boolean | ((value: any) => boolean);
  // ... structural-sharing and codegen options omitted — not used on the snapshot rail
}

interface Extension {
  type: number;
  pack?(value: any): Buffer | Uint8Array;
  unpack?(messagePack: Buffer | Uint8Array): any;
  read?(datum: any): any;
  write?(instance: any): any;
}

function addExtension(extension: Extension): void;
// Extension registration path — set is empty by contract (SnapshotExtensionRows is `never`)

type UnpackOptions = { start?: number; end?: number; lazy?: boolean } | number;
```

One reused `Decoder` instance per rail with `int64AsType: 'bigint'` aligns 64-bit sequence and logical fields with .NET. `addExtension` is the registration path for custom extension bytes; zero registrations because `SnapshotExtensionRows` is declared `never` on `snapshot-codecs.md#TS_PROJECTION`.
