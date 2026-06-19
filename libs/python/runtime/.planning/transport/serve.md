# [PY_RUNTIME_SERVE]

The inbound companion server-runtime, credential axis, and private daemon entrypoint. `ServerHost` owns the `grpc.aio` server lifecycle (bind, request lifecycle, graceful drain) under the `anyio` runner, hosting the geometry companion daemon that speaks the EXISTING C# `ComputeService`/`DocumentService`/`ControlService`/`ArtifactSync` gRPC contract over the UDS leg. `Credential` is the Python half of the C# `CredentialPolicy` axis, decoded against the five producer rows and resolved to the one credential the companion UDS serve leg admits — `insecure_loopback`, peer identity read by the kernel at accept, never a PEM bundle. `WireProtoCodec` transcodes the canonical `msgspec` `Struct` shapes to the generated protobuf `Message` the gRPC wire carries; `CrdtOpDecode` decodes the MessagePack op-log delta the C# `Rasm.Persistence/Version/commits#CRDT_WIRE` owner mints, never a re-mint; `CapabilityInvoke` consumes the C#-generated capability-descriptor SDK, deriving the companion command surface from the descriptor catalog. `Entrypoint` is the type-hint-driven `cyclopts` command grammar that launches the daemon — a PRIVATE entry only. The package owns the companion's inbound serve and the decode of every C#-owned wire shape; it mints no wire vocabulary.

## [1]-[INDEX]

- [1]-[SERVE]: the inbound `grpc.aio` server lifecycle, the decoded credential axis, the OTel span interceptor.
- [2]-[PROTO_TRANSCODE]: the one msgspec-canonical to protobuf-wire codec at the gRPC seam.
- [3]-[CRDT_DECODE]: the MessagePack op-log CRDT-op decode (the durability codec, NOT protobuf).
- [4]-[CAPABILITY_INVOKE]: the descriptor-driven polymorphic invoke decoding the C# capability SDK.
- [5]-[ENTRY]: the private companion entrypoint grammar.

## [2]-[SERVE]

- Owner: `ServerHost` — the boundary capsule over one `grpc.aio` server hosting servicers generated from the C# proto descriptors; `Credential` the tagged union decoding the C# `CredentialPolicy` five-row axis to the one credential the companion UDS serve leg admits.
- Cases: the C# `CredentialPolicy` mints five rows — `insecure-loopback`, `tls`, `mtls`, `bearer`, `composed` — but the companion serves the local control plane over the UDS leg (`RemoteTransport.UnixDomainSocket`, whose `Credentials` column is `Seq(CredentialPolicy.InsecureLoopback)` exactly), so the decode admits one case: `Credential` cases `insecure_loopback=True` is the only constructible serve-side credential, and peer identity is the kernel-reported `(pid, uid)` the C# `AppHost/companion-sidecar#PEER_ADMISSION` reads at accept through `SO_PEERCRED`/`LOCAL_PEERCRED`, never a wire-carried PEM; `tls`/`mtls`/`bearer`/`composed` are the OUTBOUND client legs the C# `Rasm.AppHost` dials and carry PEM/token material the companion never serves, so the decode names them as the deleted serve-side forms rather than constructing a server credential from them.
- Entry: `ServerHost.serve` binds the UDS socket, starts the server, and awaits termination under the `anyio` runner; `ServerHost.drain` calls `server.stop(grace)` participating in the host drain choreography so in-flight calls complete within the grace period; `Credential.server_credentials` folds the loopback case to `grpc.local_server_credentials(grpc.LocalConnectionType.UDS)` so the UDS peer authenticates by socket locality, never a key-chain pair.
- Auto: outcomes map to `grpc.StatusCode` and a domain `Error(BoundaryFault)` from `reliability/faults#FAULT` converts through `ServicerContext.abort`, never a bare exception across the wire; the generated stubs arrive from `grpcio-tools` compiling the C# `.proto` descriptors — the runtime implements servicers over them and converts to canonical shapes at the seam; the credential decode is total over the five producer rows by `match`/`assert_never` so a producer row the UDS serve leg cannot admit is a typed construction failure, never a phantom PEM bundle; the `LocalConnectionType.UDS` server credential is the grpc-native loopback authenticator the C# `ChannelCredentials.Insecure` UDS dial pairs with at the same locality boundary.
- Packages: `grpcio` (`grpc.aio.server`/`grpc.local_server_credentials`/`grpc.LocalConnectionType`/`grpc.StatusCode`/`grpc.aio.ServicerContext.abort`/`grpc.aio.ServicerContext.invocation_metadata`/`grpc.Compression`), `grpcio-tools`, `protobuf`, `opentelemetry-instrumentation-grpc` (`aio_server_interceptor`/`filters.health_check`/`filters.negate`), `opentelemetry-api` (`trace.get_current_span`/`Span.set_attribute` for the SERVER-span enrichment), `anyio`.
- Auto: the serve leg admits one `aio_server_interceptor(filter_=filters.negate(filters.health_check()))` into `grpc.aio.server(interceptors=[...])` so every RPC emits a `SERVER` span without a hand-rolled interceptor, the filter suppressing the `grpc.health.v1.Health` liveness-RPC trace noise; the interceptor is wired once at server construction, never per request, and the `TracerProvider` arrives installed at `observability/telemetry#TELEMETRY`. The inbound trace-context extract rides the servicer entry: the servicer reads the carrier off `grpc.aio.ServicerContext.invocation_metadata()` projected to `dict[str, str]` and calls `observability/receipts#RECEIPT` `Signals.continue_inbound` + `Signals.attach` BEFORE the servicer body opens its measured span, so the companion span seeds from the C#-minted parent rather than a fresh root. The SERVER-span enrichment is a span-attribute enricher composed AFTER the catalogue `aio_server_interceptor` opens the SERVER span — `ServerHost.enrich` calls `trace.get_current_span().set_attribute(rasm.descriptor/rasm.tenant/rasm.fault_case)` inside the active span scope (NOT a hook param on `aio_server_interceptor`, which takes none, and NOT a hand-rolled tracing interceptor, which the catalogue Reject row forbids), so a flame graph shows the descriptor and `Tenant` per RPC, not a bare method name.
- Growth: a new servicer is one generated stub implementation; a new wire message is one `WireProtoCodec` type pair at `[3]-[PROTO_TRANSCODE]`; a new op kind is one `CrdtOpDecode` union arm at `[4]-[CRDT_DECODE]`; a new capability is one descriptor row the `[5]-[CAPABILITY_INVOKE]` catalog already discriminates; a new enriched server-span dimension is one `set_attribute` key in `enrich`; zero new surface, no second RPC server.
- Boundary: the wire contract is the existing C# `ComputeService`/`DocumentService`/`ControlService`/`ArtifactSync` proto — the runtime mints no transport, no channel, and no second wire vocabulary; a hand-rolled message loop, a divergent message shape, a bare exception across the wire, a key-chain server credential on the UDS leg, a second RPC server, a fresh-root span where the C# parent is on the inbound carrier, and a request_hook/response_hook on the SERVER interceptor (the admitted `aio_server_interceptor(tracer_provider, filter_)` takes no hook params — server enrichment is `set_attribute`, the hook law scopes hooks to CLIENT spans) are the deleted forms; the C# host lifecycle, global health, and product telemetry export stay AppHost-owned. The serve runtime rides the cp315 core floor through `grpcio`/`protobuf`; only `grpcio-tools` stub regeneration rides the Forge companion lane (`python_version<'3.13'`), so server boot never depends on the companion environment.

```python signature
from typing import Literal, assert_never

import anyio
import grpc
from opentelemetry import trace
from opentelemetry.instrumentation.grpc import aio_server_interceptor, filters
from expression import case, tag, tagged_union

from rasm.runtime.admission import CausalFrame, RuntimeContext, RuntimeProfile, Tenant
from rasm.runtime.receipts import Signals


@tagged_union(frozen=True)
class Credential:
    tag: Literal["insecure_loopback", "tls", "mtls", "bearer", "composed"] = tag()
    insecure_loopback: bool = case()
    tls: str = case()
    mtls: tuple[str, str] = case()
    bearer: str = case()
    composed: tuple[str, ...] = case()

    def server_credentials(self) -> grpc.ServerCredentials:
        match self:
            case Credential(tag="insecure_loopback"):
                return grpc.local_server_credentials(grpc.LocalConnectionType.UDS)
            case Credential(tag="tls") | Credential(tag="mtls") | Credential(tag="bearer") | Credential(tag="composed"):
                raise ValueError(f"{self.tag} is an outbound C# client credential; the companion serves insecure_loopback over UDS only")
            case _ as unreachable:
                assert_never(unreachable)


class ServerHost:
    def __init__(self, bind: str, credential: Credential = Credential(insecure_loopback=True), grace: float = 5.0) -> None:
        self._bind, self._credential, self._grace = bind, credential, grace
        interceptor = aio_server_interceptor(filter_=filters.negate(filters.health_check()))
        self._server: grpc.aio.Server = grpc.aio.server(interceptors=[interceptor], compression=grpc.Compression.Gzip)

    def _listen(self) -> None:
        self._server.add_secure_port(self._bind, self._credential.server_credentials())

    @staticmethod
    def inbound(servicer_context: grpc.aio.ServicerContext) -> tuple[object, RuntimeContext]:
        carrier = {key: value for key, value in servicer_context.invocation_metadata()}
        frame = CausalFrame.of(
            int(carrier.get("rasm-hlc-physical", "0")), int(carrier.get("rasm-hlc-logical", "0")), carrier.get("rasm-tenant", "default")
        )
        context = Signals.continue_inbound({k: v for k, v in carrier.items() if k in ("traceparent", "tracestate")})
        return context, RuntimeContext.admit(RuntimeProfile.SIDECAR, causal=frame)

    @staticmethod
    def enrich(descriptor: str, tenant: Tenant, fault_case: str) -> None:
        span = trace.get_current_span()
        span.set_attribute("rasm.descriptor", descriptor)
        span.set_attribute("rasm.tenant", str(tenant))
        span.set_attribute("rasm.fault_case", fault_case)

    async def serve(self) -> None:
        self._listen()
        await self._server.start()
        async with anyio.create_task_group() as group:
            group.start_soon(self._server.wait_for_termination)

    async def drain(self) -> None:
        await self._server.stop(self._grace)
```

## [3]-[PROTO_TRANSCODE]

- Owner: `WireProtoCodec` — the one boundary codec transcoding the canonical `msgspec` `Struct` shapes interior code holds into the generated protobuf `Message` the C# gRPC services carry, and back; it mints no second wire vocabulary, owning only the projection across the gRPC seam so interior code never touches a `Message` and the wire never sees a `Struct`.
- Entry: `WireProtoCodec.encode` projects a canonical `Struct` to wire bytes — `msgspec.to_builtins` lowers the struct to a JSON-compatible mapping, `json_format.ParseDict` fills the generated `Message` (snake_case field names preserved against the proto schema), and `Message.SerializeToString` emits the canonical protobuf wire encoding; `WireProtoCodec.decode` reverses it — `proto.parse` decodes the wire bytes into a fresh `Message` instance, `json_format.MessageToDict` projects it to a mapping with `preserving_proto_field_name=True`, and `msgspec.convert` validates that mapping into the typed canonical `Struct`, the `msgspec.ValidationError` or protobuf `DecodeError` crossing the one `reliability/faults#FAULT` `boundary` conversion as the `wire` case.
- Auto: the codec is generic over the canonical `Struct` type and its paired generated `Message` class — one polymorphic `encode`/`decode` pair discriminating on the type arguments, never a per-message hand client; `to_builtins`/`convert` are the `msgspec` lowering/raising pair that keep the canonical shape the single source of truth, and `ParseDict`/`MessageToDict` with `preserving_proto_field_name=True` hold the snake_case field parity the proto schema and the `Struct` share; the generated `_pb2` `Message` classes arrive from `grpcio-tools` compiling the C# `.proto` descriptors and the codec reads them, never authoring a message shape. The concrete pairs decode the producer messages field-for-field — the canonical wire shapes are `TransactionRequest`/`TransactionReceipt` (the flagship `DocumentService.ExecuteTransaction` parity seam), `QueryRequest`/`QueryResponse` (the field-mask read), `InferRequest`/`InferResponse`, `SolveRequest`/`SolveResponse`, `GenerateRequest`/`TokenChunk`, `GraphDiffRequest`/`GraphDiffResponse`, `SubtreeFetchRequest`/`GraphChunk`, `ArtifactFrame`, and `FaultDetail` — the C# `Rasm.Compute/Runtime/channels#PROTO_VOCABULARY` owner's eighteen-rpc/seventeen-message family.
- Packages: `protobuf` (`json_format.ParseDict`/`json_format.MessageToDict`/`proto.parse`/`Message.SerializeToString`/`DecodeError`), `msgspec` (`to_builtins`/`convert`/`ValidationError`/`Struct`).
- Growth: a new wire message is one `(Struct, Message)` type pair passed to the existing `encode`/`decode`; zero new surface, no second codec.
- Boundary: the codec transcodes only — no second wire vocabulary, no message shape authored here, no JSON-as-wire-format on the production path (protobuf binary is the gRPC wire); a per-message hand client, a `Message` leaking into interior code, and a `Struct` crossing the wire unprojected are the deleted forms. The `FaultDetail` decode is the inbound complement of the C# `WireFault.Decode(FaultDetail)` arm — the companion reads `(package, code, case, message, evidence, correlation, hlc_physical, hlc_logical)` off the receipt slot or the `grpc-status-details-bin` trailer onto a canonical `Struct`, the typed conflict the retry owner already decodes, never a re-derived status string; the `hlc_physical`/`hlc_logical` two-half stamp and the wire `tenant` partition the inbound slot carries are no longer dropped — `ServerHost.inbound` (`[2]-[SERVE]`) decodes them into the `execution/admission#CONTEXT` `CausalFrame` threaded onto `RuntimeContext.causal`, so lanes, receipts, and metrics attribute to one host-minted `(Hlc, Tenant)` frame (the `csharp:Rasm.AppHost/Runtime/ports#PORT_RECORDS` single mint), the HLC two-64-bit-half order riding the same little-endian parity the `evidence/identity#SEED_REPRODUCTION` content seed proves.

```python signature
from google.protobuf import json_format, proto
from google.protobuf.message import Message
import msgspec
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary


class WireProtoCodec[S: Struct, M: Message]:
    def __init__(self, struct: type[S], message: type[M]) -> None:
        self._struct, self._message = struct, message

    def encode(self, value: S) -> RuntimeRail[bytes]:
        return boundary("wire", lambda: json_format.ParseDict(msgspec.to_builtins(value), self._message()).SerializeToString())

    def decode(self, payload: bytes) -> RuntimeRail[S]:
        def project() -> S:
            message = proto.parse(self._message, payload)
            mapping = json_format.MessageToDict(message, preserving_proto_field_name=True)
            return msgspec.convert(mapping, self._struct)

        return boundary("wire", project)
```

## [4]-[CRDT_DECODE]

- Owner: `CrdtOp` — the canonical op-log delta union the companion decodes from the C# `Rasm.Persistence/Version/commits#CRDT_WIRE` `CrdtOpWire` `[MessagePack.Union]` owner, ten arms tag 0-9 dense and append-only; `Hlc` the hybrid-logical-clock cell every arm carries; `CrdtOpDecode` the one decode surface reading the MessagePack delta the `OpLogEntry.Payload` carries for `column-family=crdt` rows. The op-log crosses as MessagePack under `Lz4BlockArray`, NOT protobuf — the durability codec the C# snapshot ladder and the wire seam share, distinct from the gRPC proto wire at `[3]`.
- Cases: ten op arms mirroring the producer union tag-for-tag — `set`(0), `write`(1), `add`(2), `remove`(3), `increment`(4), `insert_after`(5), `delete`(6), `maintain`(7), `beat`(8), `leave`(9); LWW survives only as the `set` arm reconstructing the `LwwRegister`, the `beat`/`leave` arms carry the `EphemeralMap` presence delta the late-joining companion reconstructs from the op-log prefix. Each arm's field positions decode the producer `[Key]` order exactly — `set` is `(field, value, physical_ticks, logical, origin)` at keys 0-4, `write` adds the `(origin, seq)[]` version-vector context at key 2, `add`/`remove` carry the `(guid, ulong)` element tags, `insert_after` carries the predecessor/id `(origin, logical)` pairs, `beat` carries `(field, origin, state, physical_ticks, logical)`, `leave` carries `(field, origin, physical_ticks, logical)`.
- Entry: `CrdtOpDecode.decode` reads the LZ4-decompressed MessagePack payload through `msgspec.msgpack.Decoder(CrdtArm)` (the `CrdtArm` union of the ten `[MessagePack.Union]` wire structs) into the tagged union, then `CrdtOpDecode.lift` projects the wire arm onto the canonical `CrdtOp` reconstructing the `Hlc` from the `(physical_ticks, logical)` pair and the `ElementId` from the `(origin, logical)` pairs — one total `match` over the ten tags, `assert_never` on the exhausted union so a producer op the companion does not decode is a typed build failure, never a silent drop; the `decompress: Callable[[bytes], bytes]` injected at the seam chooses WHAT decompresses (identity over a `MessagePackCompression.None` companion lane, or the published `Lz4BlockArray`-envelope reader once the producer framing lands) without a body rewrite; a decode that fails validation returns `Error(BoundaryFault(wire=...))` through the one `reliability/faults#FAULT` `boundary`, never a partial op.
- Auto: the wire is MessagePack — `msgspec.msgpack` decodes the producer's `[MessagePack.Union]` tag-keyed envelope where the C# `[MessagePack.Union(n, typeof(Case))]` integer `n` is the msgpack union tag and each `[property: Key(k)]` is the array position; the companion authors no op kind the wire does not carry and re-mints no op vocabulary, the C# owner being the sole mint per the single-mint invariant; `physical_ticks` is the C# `Instant.ToUnixTimeTicks()` 100-ns count the companion raises back to a NodaTime-equivalent instant by `(seconds, nanos)` split, and the `(origin, logical)` `ElementId` is the HLC-stable peer-local identity the RGA and OR-set address by, never positional.
- Packages: `msgspec` (`msgpack.Decoder`/`msgpack.Ext`/`Struct`/`ValidationError`), `lz4` (`block` LZ4 block decode — the `Lz4BlockArray` envelope leg, companion `python_version<'3.15'` band only).
- Growth: a new op kind is one `CrdtOpWire` tagged-union arm plus one `CrdtOp` arm plus one `lift` match arm — the producer adds the wire tag, the companion adds the decode arm, never ahead of the wire; zero new surface, no second op vocabulary.
- Boundary: the op-log delta is the C#-owned MessagePack wire — a protobuf framing of the op-log (the prior mismodel), a typeless payload, a JSON-array delta, and a re-minted op union are the deleted forms; the decode is read-only, the companion never encoding an op the C# owner did not mint; the `set` arm is the LWW `Adjudicate` survivor and the ten-arm union is the strict superset the C# `Crdt.Merge` join-semilattice converges, so a companion that decodes the prefix reconstructs the identical materialized state the producer holds. The `Lz4BlockArray` envelope is the MessagePack-csharp block-array framing (a msgpack `ext` envelope wrapping independently-LZ4-block-compressed chunks), so the LZ4 decompression leg gates on the cp315 `lz4` wheel and the MessagePack-block-array reframing decision the producer owns — see `[7]-[RESEARCH]`.

```python signature
from collections.abc import Callable
from typing import assert_never

import msgspec
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary


class Hlc(Struct, frozen=True):
    physical_ticks: int
    logical: int


class ElementId(Struct, frozen=True):
    origin: bytes
    logical: int


class CrdtOp(Struct, frozen=True, tag_field="op"):
    pass


class SetOp(CrdtOp, tag="set"):
    field: str
    value: bytes
    cell: Hlc
    origin: bytes


class WriteOp(CrdtOp, tag="write"):
    field: str
    value: bytes
    context: list[tuple[bytes, int]]
    cell: Hlc
    origin: bytes


class AddOp(CrdtOp, tag="add"):
    field: str
    element: bytes
    tag_id: ElementId


class RemoveOp(CrdtOp, tag="remove"):
    field: str
    element: bytes
    observed_tags: list[ElementId]


class IncrementOp(CrdtOp, tag="increment"):
    field: str
    origin: bytes
    delta: int


class InsertAfterOp(CrdtOp, tag="insert_after"):
    field: str
    predecessor: ElementId
    id: ElementId
    value: bytes


class DeleteOp(CrdtOp, tag="delete"):
    field: str
    id: ElementId


class MaintainOp(CrdtOp, tag="maintain"):
    field: str
    quiescent: list[tuple[bytes, int]]


class BeatOp(CrdtOp, tag="beat"):
    field: str
    origin: bytes
    state: bytes
    cell: Hlc


class LeaveOp(CrdtOp, tag="leave"):
    field: str
    origin: bytes
    cell: Hlc


type CrdtOpArm = SetOp | WriteOp | AddOp | RemoveOp | IncrementOp | InsertAfterOp | DeleteOp | MaintainOp | BeatOp | LeaveOp


class CrdtOpWire(Struct, frozen=True, tag_field="tag", array_like=True):
    pass


class SetWire(CrdtOpWire, tag=0):
    field: str
    value: bytes
    physical_ticks: int
    logical: int
    origin: bytes


class WriteWire(CrdtOpWire, tag=1):
    field: str
    value: bytes
    context: list[tuple[bytes, int]]
    physical_ticks: int
    logical: int
    origin: bytes


class AddWire(CrdtOpWire, tag=2):
    field: str
    element: bytes
    tag_origin: bytes
    tag_logical: int


class RemoveWire(CrdtOpWire, tag=3):
    field: str
    element: bytes
    observed_tags: list[tuple[bytes, int]]


class IncrementWire(CrdtOpWire, tag=4):
    field: str
    origin: bytes
    delta: int


class InsertAfterWire(CrdtOpWire, tag=5):
    field: str
    pred_origin: bytes
    pred_logical: int
    id_origin: bytes
    id_logical: int
    value: bytes


class DeleteWire(CrdtOpWire, tag=6):
    field: str
    id_origin: bytes
    id_logical: int


class MaintainWire(CrdtOpWire, tag=7):
    field: str
    quiescent: list[tuple[bytes, int]]


class BeatWire(CrdtOpWire, tag=8):
    field: str
    origin: bytes
    state: bytes
    physical_ticks: int
    logical: int


class LeaveWire(CrdtOpWire, tag=9):
    field: str
    origin: bytes
    physical_ticks: int
    logical: int


type CrdtArm = SetWire | WriteWire | AddWire | RemoveWire | IncrementWire | InsertAfterWire | DeleteWire | MaintainWire | BeatWire | LeaveWire


class CrdtOpDecode:
    _decoder: msgspec.msgpack.Decoder[CrdtArm] = msgspec.msgpack.Decoder(CrdtArm)

    @staticmethod
    def lift(wire: CrdtArm) -> CrdtOpArm:
        match wire:
            case SetWire(field, value, physical_ticks, logical, origin):
                return SetOp(field, value, Hlc(physical_ticks, logical), origin)
            case WriteWire(field, value, context, physical_ticks, logical, origin):
                return WriteOp(field, value, context, Hlc(physical_ticks, logical), origin)
            case AddWire(field, element, tag_origin, tag_logical):
                return AddOp(field, element, ElementId(tag_origin, tag_logical))
            case RemoveWire(field, element, observed_tags):
                return RemoveOp(field, element, [ElementId(o, n) for o, n in observed_tags])
            case IncrementWire(field, origin, delta):
                return IncrementOp(field, origin, delta)
            case InsertAfterWire(field, pred_origin, pred_logical, id_origin, id_logical, value):
                return InsertAfterOp(field, ElementId(pred_origin, pred_logical), ElementId(id_origin, id_logical), value)
            case DeleteWire(field, id_origin, id_logical):
                return DeleteOp(field, ElementId(id_origin, id_logical))
            case MaintainWire(field, quiescent):
                return MaintainOp(field, quiescent)
            case BeatWire(field, origin, state, physical_ticks, logical):
                return BeatOp(field, origin, state, Hlc(physical_ticks, logical))
            case LeaveWire(field, origin, physical_ticks, logical):
                return LeaveOp(field, origin, Hlc(physical_ticks, logical))
            case _ as unreachable:
                assert_never(unreachable)

    @classmethod
    def decode(cls, lz4_payload: bytes, decompress: Callable[[bytes], bytes]) -> RuntimeRail[CrdtOpArm]:
        return boundary("wire", lambda: cls.lift(cls._decoder.decode(decompress(lz4_payload))))
```

## [5]-[CAPABILITY_INVOKE]

- Owner: `CapabilityInvoke` — the descriptor-driven polymorphic invoke decoding the C# `Rasm.AppHost/Agent/capability#SDK_CODEGEN` Python target; `DiscoveryResult` the descriptor catalog row the companion ingests from the C# `DiscoveryResultWire[]` projection; `CommandReceipt` the per-command evidence the companion reads back through the C# `ReceiptEnvelopeWire<CapabilityCommandReceiptWire>`. One invoke keyed by descriptor id discriminates every capability, never a per-service hand client. The OUTBOUND dispatch wraps its channel with `aio_client_interceptors(tracer_provider, filter_, request_hook, response_hook)` so the call propagates the active span through the `observability/telemetry#TELEMETRY`-installed composite and the client hooks enrich the CLIENT span with the descriptor id, the `CAUSAL_TENANT_FRAME` `Tenant`, and the fault case.
- Cases: the C# SDK Python target emits one method per descriptor — `def {surface}_{op}(self, args: CommandArguments) -> CommandReceipt: return self._run("{surface}.{op}", args)` — so the companion decodes the descriptor catalog into one `_run(descriptor_id, args)` dispatch where `descriptor_id` is the `{surface}.{op}` join the C# `CapabilityDescriptor.Of` mints; the per-descriptor `input_schema` is the `JsonSchemaExporter` JSON Schema the C# `SuiteContracts.Schema` derives from the descriptor's `CommandArguments`, so the companion validates the argument payload against the generated schema, never a hand-mirrored shape; the command-txn disposition decodes as the literal-discriminated union the C# `CommandTxn` mints — `committed` · `rolled_back` · `compensated` · `refused` — over the `CapabilityCommandReceiptWire.txn` kind.
- Entry: `CapabilityInvoke.run` keys the descriptor by id, validates the `CommandArguments` payload against the descriptor's generated `input_schema` through `msgspec.json.decode`, projects the argument shape onto the `WireProtoCodec`-paired request message, dispatches over the gRPC stub, and decodes the `CommandReceipt` back; `CapabilityInvoke.discover` reads the descriptor catalog the C# `CapabilityRegistry.Discover(DiscoveryQuery.All)` emits so the companion command surface is generated from the descriptor rows, never hand-listed; `CapabilityInvoke.interceptors` folds the catalogue `aio_client_interceptors` factory with the descriptor-id/`Tenant` `request_hook` and the fault-case `response_hook` so the outbound channel composer wraps its stub once — the `request_hook` receives `(span, request)` and the `response_hook` receives `(span, details)`, the gRPC status-details string the interceptor awaits off the unary call (`""` on OK, the server's detail message on a non-OK status), both CLIENT-side per the catalogue hook law.
- Auto: the descriptor catalog crosses the wire as the `DiscoveryResultWire[]` array the C# `[7]-[TS_PROJECTION]` projects — `(descriptor, surface, effect, idempotency, estimated, scope_hash)` — and the companion decodes it through `msgspec.json.Decoder(list[DiscoveryResult])`; the `effect`/`idempotency`/`cost-unit` keys decode as the C# smart-enum string keys (`pure`/`read`/`write`/`external`/`irreversible`, `idempotent`/`keyed`/`single-shot`/`non-idempotent`, `cpu-millis`/`wall-millis`/`bytes-egress`/`model-tokens`/`calls`); the cost vector decodes as the unit-keyed mapping the C# `CostVectorWire` emits; a new descriptor row regenerates the companion command surface with no per-capability hand edit because the catalog is the single descriptor source; the client hooks set `rasm.descriptor`/`rasm.tenant`/`rasm.fault_case` on the CLIENT span through `Span.set_attribute` inside the active span scope, never blocking, and the factory's `tracer_provider` defaults to `None`, resolving the global provider the `observability/telemetry#TELEMETRY` `set_tracer_provider` install registers (the catalogue provider law), so no second tracer mints.
- Packages: `msgspec` (`json.Decoder`/`json.decode`/`Struct`/`ValidationError`), `protobuf` (the `WireProtoCodec`-paired command request/receipt messages), `opentelemetry-instrumentation-grpc` (`aio_client_interceptors(tracer_provider, filter_, request_hook, response_hook)` interceptor factories [3], `filters` predicate algebra), `opentelemetry-api` (`trace.get_current_span`/`Span.set_attribute` for the hook enrichment).
- Growth: a new capability is one descriptor row the C# registry already folds — the companion reads it through the existing `discover`/`run` pair, never a new method; a new effect/idempotency/cost key is one literal the decode union already discriminates; a new enriched span dimension is one `set_attribute` key in the existing hook, never a second interceptor; zero new surface, no per-service hand client.
- Boundary: the descriptor is the suite's only op-metadata owner and the C# registry is its sole mint — a per-capability hand client, a re-authored descriptor shape, a hand-mirrored argument schema, a second cost table, a hand-rolled tracing interceptor (the catalogue Reject row), and a second tracer are the deleted forms; the companion reads the descriptor, re-authoring no capability shape; the command dispatch lands on the gRPC stub through `WireProtoCodec`, so the capability invoke rides the existing proto wire and mints no fourth wire shape; the cross-language shape-identity is the C# `SuiteContracts.Schema` JSON Schema the C#, TS, and Python SDKs all bind, so an additive descriptor field admits on every consumer and a breaking reshape rejects on all three through the one `ContractGuard.AdditiveOnly` classifier.

```python signature
from collections.abc import Awaitable, Callable
from typing import Literal

import msgspec
from expression import Error, Ok
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.instrumentation.grpc import aio_client_interceptors, filters

from rasm.runtime.admission import Tenant
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary, boundary


type CostUnitKey = Literal["cpu-millis", "wall-millis", "bytes-egress", "model-tokens", "calls"]
type CostVector = dict[CostUnitKey, int]
type CommandTxnKind = Literal["committed", "rolled_back", "compensated", "refused"]


class DiscoveryResult(Struct, frozen=True, rename="camel"):
    descriptor: str
    surface: str
    effect: Literal["pure", "read", "write", "external", "irreversible"]
    idempotency: Literal["idempotent", "keyed", "single-shot", "non-idempotent"]
    estimated: CostVector
    scope_hash: str


class CommandReceipt(Struct, frozen=True, rename="camel"):
    descriptor: str
    txn: dict[str, object]
    charged: CostVector
    elapsed: str
    correlation: str


class CommandArguments(Struct, frozen=True):
    payload: bytes
    tenant: Tenant
    correlation: str


class CapabilityInvoke:
    _catalog: msgspec.json.Decoder[list[DiscoveryResult]] = msgspec.json.Decoder(list[DiscoveryResult])
    _receipt: msgspec.json.Decoder[CommandReceipt] = msgspec.json.Decoder(CommandReceipt)

    def __init__(self, catalog: dict[str, DiscoveryResult], dispatch: Callable[[str, bytes], Awaitable[bytes]]) -> None:
        self._catalog_by_id, self._dispatch = catalog, dispatch

    @classmethod
    def discover(cls, payload: bytes) -> RuntimeRail[list[DiscoveryResult]]:
        return boundary("wire", lambda: cls._catalog.decode(payload))

    @staticmethod
    def interceptors(descriptor_id: str, tenant: Tenant) -> list[object]:
        def request_hook(span: trace.Span, _request: object) -> None:
            span.set_attribute("rasm.descriptor", descriptor_id)
            span.set_attribute("rasm.tenant", str(tenant))

        def response_hook(span: trace.Span, details: str) -> None:
            span.set_attribute("rasm.fault_case", details or "ok")

        return aio_client_interceptors(filter_=filters.negate(filters.health_check()), request_hook=request_hook, response_hook=response_hook)

    async def run(self, descriptor_id: str, args: CommandArguments) -> RuntimeRail[CommandReceipt]:
        if descriptor_id not in self._catalog_by_id:
            return Error(BoundaryFault(wire=(descriptor_id, 0)))
        validated = boundary("wire", lambda: msgspec.json.decode(args.payload, type=dict))
        if validated.is_error():
            return Error(validated.error)
        dispatched = await async_boundary("wire", lambda: self._dispatch(descriptor_id, msgspec.json.encode(args)))
        return dispatched.bind(lambda raw: boundary("wire", lambda: self._receipt.decode(raw)))
```

## [6]-[ENTRY]

- Owner: `Entrypoint` — the type-hint-driven `cyclopts` command axis backing the companion daemon's PRIVATE entry and package-internal entrypoints only; co-located with `ServerHost` because `serve` composes the host it launches.
- Entry: `companion_app` returns the `cyclopts.App` whose commands bind to the companion serve and drain; arguments bind from type hints, never from a hand-parsed `argv`.
- Packages: `cyclopts` (`App`/`Parameter`).
- Growth: a new private command is one `@app.command` method on the existing app; zero new surface.
- Boundary: never a new public command surface — public commands are reserved to the suite Assay command surface; a public-facing CLI axis and a hand-parsed argument loop are the deleted forms.

```python signature
from cyclopts import App


def companion_app() -> App:
    app = App(name="rasm-companion", help="private companion daemon entry")

    @app.command
    async def serve(bind: str, *, grace: float = 5.0) -> None:
        host = ServerHost(bind, Credential(insecure_loopback=True), grace)
        await host.serve()

    return app
```

## [7]-[RESEARCH]

- [CREDENTIAL_PEM]: [COMPLETE] — decoded against the C# `Rasm.Compute/Runtime/channels#CALL_POLICY` `CredentialPolicy` five-row axis. The companion serves the local control plane over `RemoteTransport.UnixDomainSocket`, whose `Credentials` column is `Seq(CredentialPolicy.InsecureLoopback)` exactly, so the serve-side credential is `insecure_loopback` and peer identity is the kernel-reported `(pid, uid)` the C# `AppHost/companion-sidecar#PEER_ADMISSION` reads at accept (`SO_PEERCRED` on Linux, `LOCAL_PEERCRED`+`LOCAL_PEERPID` on macOS), never a wire PEM. The serve credential is `grpc.local_server_credentials(grpc.LocalConnectionType.UDS)` (reflection-confirmed: `grpc.LocalConnectionType.UDS` and `grpc.local_server_credentials` are the loopback authenticator pair). The `tls`/`mtls`/`bearer`/`composed` rows are the OUTBOUND C# client legs (`ChannelCredentials.SecureSsl`/`CallCredentials.FromInterceptor`/`CallCredentials.Compose`) carrying PEM/token material the companion never serves — the prior `\n--SEP--\n` PEM-split placeholder was a fabricated serve-side PEM bundle and is the deleted form. The `Credential` decode is total over the five producer rows by `match`/`assert_never`.
- [PROTO_TRANSCODE]: [COMPLETE] — reflection-confirmed. `msgspec.to_builtins`/`msgspec.convert` are the canonical lowering/raising pair, and `json_format.ParseDict`/`json_format.MessageToDict(preserving_proto_field_name=True)`/`proto.parse`/`Message.SerializeToString` are the protobuf seam projection (branch `libs/python/.api/protobuf.md` rows). The codec is settled; the concrete `(Struct, Message)` pairs decode the eighteen-rpc/seventeen-message C# `Rasm.Compute/Runtime/channels#PROTO_VOCABULARY` family and await only `grpcio-tools` compiling the landed `.proto` descriptors to `_pb2`.
- [CRDT_DECODE_MSGPACK]: [COMPLETE] (cp315-clean leg) — the op-log crosses as MessagePack (`CrdtOpWire` `[MessagePack.Union]` tags 0-9, the C# `Rasm.Persistence/Version/commits#CRDT_WIRE` owner), decoded through `msgspec.msgpack.Decoder` (reflection-confirmed on the cp315 core: `msgspec.msgpack` exposes `Decoder`/`Encoder`/`Ext`/`decode`/`encode`). The `msgspec.Struct` tagged-union with `tag_field`/`array_like=True` and integer `tag=n` decodes the producer's tag-keyed array envelope position-for-position against the `[property: Key(k)]` order, validated arm-for-arm: `Set`/`Beat`/`Leave` split the `(physical_ticks, logical)` pair the `CrdtWire.Map` Hlc reconstruction reads, `Add`/`Remove`/`InsertAfter`/`Delete` carry the `(origin, logical)` `ElementId` pairs, and the ten `[Key]` orders match field-for-field. `CrdtOpDecode.lift` is the total `match`/`assert_never` projection onto the canonical `CrdtOp` union mirroring the C# `CrdtWire.Map` switch — `decode` runs `lift ∘ Decoder.decode`, so an unmapped wire arm is a typed build failure, never a raw-wire leak. The prior model — a protobuf `WireCodec (Struct, Message)` pair — was a decode DEFECT: the op-log is the durability codec, NOT the gRPC proto wire. The MessagePack decode and the canonical lift are settled on the cp315 core.
- [CRDT_DECODE_LZ4]: [UPSTREAM-BLOCKED] — the C# producer compresses the op-log payload under `MessagePackCompression.Lz4BlockArray`, the MessagePack-csharp block-array framing (a msgpack `ext` type wrapping independently-LZ4-block-compressed chunks), not a raw LZ4 frame. CONSUMER-SIDE FRAMING DECISION (this campaign authors it): the `CrdtOpDecode.decode(cls, lz4_payload, decompress)` seam (the `decompress: Callable[[bytes], bytes]` injected at the fence) already exists, so the resolution is purely WHICH callable fills it, never a body rewrite. The producer-owned choice is one of two lanes — (a) a `MessagePackCompression.None` companion lane: the cp315 decode fills `decompress` with the identity function and reads the uncompressed delta the settled `msgspec.msgpack.Decoder(CrdtArm)` leg already decodes (`[CRDT_DECODE_MSGPACK]` [COMPLETE]), so no LZ4 dependency is needed on the core; or (b) a published `Lz4BlockArray` envelope spec: `decompress` becomes the msgpack-ext-wrapping-LZ4-blocks reader the three runtimes share, gated on the cp315 `lz4` wheel. This unit OWNS the consumer-side note; the producer-side mint is the cross-agent `CRDT_OPLOG_WIRE_AMENDMENT` delta (`csharp:Rasm.Persistence/Version/commits#CRDT_WIRE`, where `CrdtWire.Encode` under `Lz4BlockArray` is the producer and emits the `MessagePackCompression.None` companion lane for cross-runtime consumers — LZ4 stays C#-internal). Two genuinely-upstream gates remain: (1) `lz4` is manifest-declared `lz4; python_version<'3.15'` with NO cp315 wheel synced, so the LZ4 decompression rides the `python_version<'3.15'` companion band exactly like `xxhash`; (2) the producer `Lz4BlockArray`-envelope framing publication. The `msgspec.msgpack` decode of the uncompressed delta is proven on the cp315 core; the compressed-envelope link stays gated, never fabricated as present.
- [CAPABILITY_INVOKE]: [COMPLETE] — decoded against the C# `Rasm.AppHost/Agent/capability#SDK_CODEGEN` Python target (`def {method}(self, args: CommandArguments) -> CommandReceipt: return self._run("{descriptor}", args)`) and the `#TS_PROJECTION` `DiscoveryResultWire`/`CapabilityCommandReceiptWire` shapes. `CapabilityInvoke.discover` decodes the catalog through `msgspec.json.Decoder(list[DiscoveryResult])` and `CapabilityInvoke.run` is the one polymorphic invoke — descriptor-id lookup against the frozen catalog (an absent id is a typed `wire` fault), `msgspec.json.decode` schema validation of the `CommandArguments` payload, `async_boundary` dispatch over the injected stub coroutine, and `_receipt.decode` of the `CommandReceipt` back, all on the `RuntimeRail`. `DiscoveryResult` matches the producer `DiscoveryResult` (`descriptor, surface, effect, idempotency, estimated, scope_hash`) field-for-field and the `committed`/`rolled_back`/`compensated`/`refused` `CommandTxnKind` mirrors the producer `CommandTxn` `Committed`/`RolledBack`/`Compensated`/`Refused` arms; the `txn` arm-payload carrier stays the producer-owned `CapabilityCommandReceiptWire.txn` mapping, never a re-minted discriminant. The companion re-authors no descriptor shape. Settled; awaits only the live descriptor catalog emission at SDK-bootstrap.
- [GRPC_CLIENT_AND_HOOKS]: [COMPLETE] (design) — reflection-confirmed against `.api/opentelemetry-instrumentation-grpc.md`. CLIENT enrichment rides `aio_client_interceptors(tracer_provider, filter_, request_hook, response_hook)` (interceptor factories [3], the only factory carrying hook params) on the `[5]-[CAPABILITY_INVOKE]` outbound dispatch and any host call-back channel — the `request_hook` reads `(span, request)` and sets `rasm.descriptor`/`rasm.tenant`, the `response_hook` reads `(span, details)` (the unary `call.details()` status-details string, empty on OK) and sets `rasm.fault_case`, both CLIENT-side per the hook law. SERVER enrichment is a span-attribute enricher: `ServerHost.enrich` calls `trace.get_current_span().set_attribute(...)` (NOT a server-interceptor hook — `aio_server_interceptor(tracer_provider=None, filter_=None)` (interceptor factories [4]) takes NO hook params, and the catalogue Reject row forbids a hand-rolled tracing interceptor) AFTER the catalogue `aio_server_interceptor` opens the SERVER span, inside the active span scope, never blocking. `tracer_provider` is the `observability/telemetry#TELEMETRY`-installed provider so no second tracer mints; the outbound interceptors compose the same `Propagators.DefaultTextMapPropagator` the inbound `observability/receipts#RECEIPT` `continue_inbound` extract reads, so inbound and outbound legs share one trace (the mechanical reason this sequences after `TRACE_INBOUND_EXTRACT`). Realizes the cross-`libs/` `ONE_DISTRIBUTED_TRACE` outbound Python leg. Spellings settled.
- [KEYRING_CATALOGUE]: [DROPPED] (serve leg) — the prior `keyring.get_password` resolution was the serve-side PEM-bundle path the `[CREDENTIAL_PEM]` decode deletes; the companion UDS serve leg reads no OS keyring (peer identity is the kernel accept-time credential), so the `keyring` package leaves the SERVE owner. `keyring` is NOT folder-orphaned — it now lands the one `execution/admission#SETTINGS` `SecretBoundary` reading OS-keystore credentials for the OUTBOUND transport legs (`transport/roots#RESOURCE` `ssh` passphrase, `http` bearer), profile-gated and lazy on the outbound leg; the serve leg admits no keyring lookup.
