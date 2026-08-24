# [PY_CAD_PROVIDER]

`CadProvider` implements the generated `CadService` protocol over the isolated OCCT process: two idempotent unary rpcs that admit a request, resolve its artifact references onto call-owned paths, drive one native fold on the cancellable process lane, publish the sealed output, and answer a reference-only body. One spine serves both, and one policy row per rpc carries every axis they differ by, so a third rpc is one row beside its generated override.

`faults#ROWS` supplies every refusal and `faults#PROJECTION` the one detail fold, so this page holds the package's single outbound raise: one `ConnectError(code, message, details=[...])` built from `refused(fault, stamp)`. `spool#POLICY` owns the admitted budgets and the compression roster, `spool#BUDGET` the single deadline read, `spool#SPOOL` the source and output custody, and `lane#LANE` the pickle-seam call with its marshalled evidence. `BodyAdmission(SERVER)` owns every field rule.

## [01]-[INDEX]

- [02]-[PROVIDER]: Per-rpc policy row, one served spine over it, and the terminal collapse into `ConnectError`.
- [03]-[COMPOSITION]: Generated ASGI application, its one mount policy, and the boot-census obligation it inherits.

## [02]-[PROVIDER]

- Owner: `CadProvider.execute` and `CadProvider.tessellate` are the two generated overrides satisfying `CadService`; `_served` is their one spine and `_EXECUTE` and `_TESSELLATE` are the two rows every difference between the rpcs now lives in.
- Law: `execute` and `tessellate` were ONE method written twice — twenty-two of thirty-two lines identical, including all sixteen lines of an eight-arm `except` cascade, varying only by native fold, one extra argument, reply class, and one coordinate literal; the collapse tables those five axes as `Rpc` columns and leaves each override two lines long.
- Cases: `Rpc` carries the refusal coordinate, the sink suffix, the source-arity resolver, the native kernel, and the reply builder — the extra-argument axis is gone because BOTH kernels now take one `NativeCall` and both enforce its admitted output `ceiling` at their write seam, which also closes the unbounded STEP write the asymmetry hid.
- Law: `execute` takes `ExecuteRequest` and answers `ExecuteResponse`, `tessellate` takes `TessellateRequest` and answers `TessellateResponse`, and both are `(request, ctx, /)` POSITIONAL-ONLY under `@override` against the generated protocol, so a renamed parameter cannot silently diverge from the interface Connect dispatches through.
- Law: `ctx.timeout_ms` is read ONCE per call, in the override, and nowhere else — it is a property recomputing time REMAINING off a monotonic deadline and it goes negative past that deadline with no clamp, so a second read inside one method is a different number by construction.
- Law: `except Exception` over an owned family has NO seat — the rail carries every domain refusal, each foreign raise is converted at the seam that owns it, and an unclassified raise propagates as a defect rather than being flattened into `INTERNAL` beside a coordinate that names nothing.
- Law: `TimeoutError` from `fail_after` is the ONE raise this spine catches, and it converts to `CALL_DEADLINE` after the sink context has already retired its operation directory under the shield `spool#SPOOL` states.
- Law: the collapse is ONE `ConnectError` built from `refused(fault, stamp)`, and a refused stamp crosses DETAIL-LESS under its own row's code rather than borrowing another call's correlation, because absence is answerable on ingress and a forged verdict is not.
- Law: `request.to_binary()` re-encodes a body Connect already decoded and body admission already passed, so the proto-plane encode fence — which guards a HAND-BUILT message against a wrong-typed slot — has no seat on this path.
- Entry: `_driven` threads three expressions and no accumulator: sources resolve and fold into one `NativeCall`, the lane returns marshalled evidence, and the published `ArtifactRef` meets that evidence in the row's own reply builder.
- Growth: a new rpc is one `Rpc` row beside its two-line generated override; a new refusal is one `faults#ROWS` row; a new reply field is one proto edit and regeneration.
- Boundary: mount, bind, credentials, process supervision, and the `FaultStamp` source stay at the app root; this page constructs one application value, raises one `ConnectError`, and mints no listener.

```python signature
from collections.abc import Callable
from contextlib import AsyncExitStack
from typing import Final, override

from anyio import fail_after
from connectrpc.compression import Compression
from connectrpc.errors import ConnectError
from connectrpc.interceptor import Interceptor
from connectrpc.request import RequestContext
from expression import Error, Result
from msgspec import Struct
from protobuf import Message
from rasm.contracts import AdmissionSide, BodyAdmission
from rasm.runtime.transport.artifact import ArtifactSink, ArtifactTransfer, output
from rasm.contracts.rasm.contracts.artifact.artifact_pb import ArtifactRef
from rasm.contracts.rasm.contracts.cad.operations_pb import ExecuteRequest, ExecuteResponse
from rasm.contracts.rasm.contracts.cad.service_connect import CadService, CadServiceASGIApplication
from rasm.contracts.rasm.contracts.cad.service_pb import TessellateRequest
from rasm.contracts.rasm.contracts.cad.types_pb import (
    BrepKernelReceipt,
    SealedStep,
    StepProtocol,
    TessellateResponse,
)

from rasm.cad.faults import CALL_DEADLINE, CadFault, CadRail, FaultStamp, refused
from rasm.cad.service.lane import (
    BrepMarshal,
    MeshMarshal,
    NativeCall,
    OneSource,
    SourceRow,
    SourceRows,
    Sources,
    brep_kernel,
    mesh_kernel,
    native,
)
from rasm.cad.service.spool import COMPRESSIONS, ArtifactPort, ProviderPolicy, bound, budget, published, sources

# --- [MODELS] ---------------------------------------------------------------------------


class Rpc[Q: Message, S: Message, E, C: Sources](Struct, frozen=True, kw_only=True):
    # five axes the two hand-written methods duplicated twenty-two lines to express. `C` names the source shape
    # this rpc admits and `E` the marshalled evidence its kernel answers, so the spine stays generic over both
    # and names neither native leg; a sixth axis lands as a sixth column, never a branch inside `_served`.
    coordinate: str
    suffix: str
    resolve: Callable[[tuple[SourceRow, ...]], CadRail[C]]
    kernel: Callable[[NativeCall[C]], CadRail[E]]
    reply: Callable[[E, ArtifactRef], S]


# --- [TABLES] ---------------------------------------------------------------------------

_EXECUTE: Final[Rpc[ExecuteRequest, ExecuteResponse, BrepMarshal, SourceRows]] = Rpc(
    coordinate="cad.execute",
    suffix=".step",
    resolve=SourceRows.of,
    kernel=brep_kernel,
    reply=lambda marshal, artifact: ExecuteResponse(
        receipt=BrepKernelReceipt.from_binary(marshal.receipt),
        step=SealedStep(protocol=StepProtocol(marshal.protocol), artifact=artifact),
    ),
)

_TESSELLATE: Final[Rpc[TessellateRequest, TessellateResponse, MeshMarshal, OneSource]] = Rpc(
    coordinate="cad.tessellate",
    suffix=".glb",
    resolve=OneSource.of,
    kernel=mesh_kernel,
    reply=lambda marshal, artifact: TessellateResponse(
        element_count=marshal.element_count,
        triangle_count=marshal.triangle_count,
        kernel=BrepKernelReceipt.from_binary(marshal.kernel),
        artifact=artifact,
    ),
)


# --- [SERVICES] -------------------------------------------------------------------------


class CadProvider(CadService):
    def __init__(
        self,
        policy: ProviderPolicy,
        artifacts: ArtifactPort,
        stamp: Callable[[], CadRail[FaultStamp]],
        /,
    ) -> None:
        self._policy = policy
        self._artifacts = ArtifactTransfer(artifacts)
        self._stamp = stamp

    @override
    async def execute(
        self,
        request: ExecuteRequest,
        ctx: RequestContext[ExecuteRequest, ExecuteResponse],
        /,
    ) -> ExecuteResponse:
        # this is the call's ONE read of `ctx.timeout_ms`: it recomputes time REMAINING on every read and goes
        # negative past the deadline, so every inner window derives from the scope this one read opens.
        return self._settled(await self._served(_EXECUTE, request, ctx.timeout_ms))

    @override
    async def tessellate(
        self,
        request: TessellateRequest,
        ctx: RequestContext[TessellateRequest, TessellateResponse],
        /,
    ) -> TessellateResponse:
        return self._settled(await self._served(_TESSELLATE, request, ctx.timeout_ms))

    async def _served[Q: Message, S: Message, E, C: Sources](
        self,
        row: Rpc[Q, S, E, C],
        request: Q,
        timeout_ms: float | None,
        /,
    ) -> CadRail[S]:
        try:
            with fail_after(budget(timeout_ms, self._policy.call_seconds)):
                async with AsyncExitStack() as stack, output(suffix=row.suffix) as sink:
                    return await self._driven(row, request, stack, sink)
        except TimeoutError:
            # `fail_after` bounds the whole call, so by the time this arm runs the sink context has already retired
            # its operation directory under a shielded scope and no fetched input path survives the elapsed budget.
            return Error(CALL_DEADLINE.at(f"{row.coordinate}.deadline"))

    async def _driven[Q: Message, S: Message, E, C: Sources](
        self,
        row: Rpc[Q, S, E, C],
        request: Q,
        stack: AsyncExitStack,
        sink: ArtifactSink,
        /,
    ) -> CadRail[S]:
        prepared = (await sources(request, self._artifacts, stack, self._policy)).bind(row.resolve).map(
            lambda shape: NativeCall(
                # Connect decoded this body and `BodyAdmission(SERVER)` admitted it, so the re-encode that carries
                # it across the pickle seam is total and needs no separate proto-plane encode fence.
                payload=request.to_binary(),
                sources=shape,
                target=str(sink.path),
                ceiling=self._policy.artifact_bytes,
            )
        )
        marshalled = await bound(prepared, lambda call: native(row.kernel, call, saturation=self._policy.call_seconds))
        return await bound(marshalled, lambda evidence: self._replied(row, evidence, sink))

    async def _replied[Q: Message, S: Message, E, C: Sources](
        self,
        row: Rpc[Q, S, E, C],
        evidence: E,
        sink: ArtifactSink,
        /,
    ) -> CadRail[S]:
        return (await published(sink, self._artifacts)).map(lambda artifact: row.reply(evidence, artifact))

    def _settled[S](self, rail: CadRail[S], /) -> S:
        match rail:
            case Result(tag="ok", ok=reply):
                return reply
            case Result(tag="error", error=fault):
                raise self._connect(fault)

    def _connect(self, fault: CadFault, /) -> ConnectError:
        # `_settled` raises here and nowhere else — the package's ONE outbound raise site, and the only seat a
        # foreign caller's demand for an exception has. A refused stamp crosses DETAIL-LESS under its own row's
        # code rather than borrowing another call's correlation: absence answers, and absence forges no verdict.
        match self._stamp():
            case Result(tag="ok", ok=stamp):
                code, message, details = refused(fault, stamp)
                return ConnectError(code, message, details=details)
            case Result(tag="error", error=unstamped):
                return ConnectError(unstamped.row.code, f"{unstamped.row.leg}:{unstamped.coordinate}", details=())
```

## [03]-[COMPOSITION]

- Owner: `application` mints the package's one composition value — a `CadServiceASGIApplication` over `CadProvider` under a single mount policy.
- Law: `BodyAdmission(SERVER)` validates every request and response element and refuses each as a `ConnectError` ITSELF, so a request violation never reaches a handler body and no handler prologue re-spells a field rule the corpus already declares.
- Law: a response violation refuses as `INTERNAL` after this spine has returned, which is precisely why a reply this page builds is never re-checked here — the interceptor is the response's own fence.
- Law: interceptors wrap in declaration order with the first declared OUTERMOST, so the app root's metadata pair leads and body admission sits innermost against the body it validates; this signature admits that pair by parameter rather than by an edit here.
- Law: `read_max_bytes` bounds the decompressed request body and `compressions` negotiates per call with identity always surviving, and both read the one `spool#POLICY` row rather than a literal seated at the mount.
- Law: zstd is OPT-IN — an unset roster negotiates gzip and identity alone, so the declared `zstandard` distribution reaches the wire only because `COMPRESSIONS` is passed here and to every dial on the same profile.
- Law: unset `codecs` seats the proto binary and proto JSON pair, which is the whole codec surface this service answers; a request naming an unseated codec draws a 415 before any interceptor runs.
- Law: the app root's boot census compares this application's `path` against the corpus method roster in BOTH directions and runs BEFORE any install claims process ownership, so a corpus rpc with no handler and a handler no rpc backs each refuse at boot rather than at a peer's first dial.
- Law: the census reads GENERATED surfaces — the application's own `path` and the descriptor's methods — never a transcribed route regex the next generator release breaks.
- Growth: a new interceptor is one element the app root passes; a new compression is one `COMPRESSIONS` value; a second served service is its own application value at the app root, never a second mount here.
- Boundary: hypercorn, the dispatcher, TLS material, and the ambient process lifecycle stay at the app root; this page hands it one ASGI callable carrying its own mount path.

```python signature
# --- [COMPOSITION] ----------------------------------------------------------------------


def application(
    policy: ProviderPolicy,
    artifacts: ArtifactPort,
    stamp: Callable[[], CadRail[FaultStamp]],
    /,
    *,
    interceptors: tuple[Interceptor, ...] = (),
    compressions: tuple[Compression, ...] = COMPRESSIONS,
) -> CadServiceASGIApplication:
    # ONE mount policy for the whole service: the app root's metadata interceptors lead so they wrap outermost,
    # body admission sits innermost against the body, and the body ceiling and codec roster read the admitted
    # policy row rather than literals, so a profile change moves one row instead of every mount and dial.
    return CadServiceASGIApplication(
        CadProvider(policy, artifacts, stamp),
        interceptors=(*interceptors, BodyAdmission(AdmissionSide.SERVER)),
        read_max_bytes=policy.read_max_bytes,
        compressions=compressions,
    )
```

## [04]-[RESEARCH]

(none)
