# [PY_GEOMETRY_MESH_SERVE]

`GeometryServe` is the geometry-side owner of the generated `ComputeService` and complete generated `ArtifactService`. It returns tessellation receipts by reference, serves stored bodies through `Fetch`, and accepts verified bodies through `Put`. The runtime host seats `BodyAdmission(SERVER)` on both generated applications, while runtime `transport/artifact` alone owns cross-frame identity, extent, hashing, temporary-file custody, and framing.

The servicer has no ring, byte cache, frame state machine, or second store. `TessellationDaemon.repository` is the one required repository used by tessellation input resolution, output publication, replay, `Fetch`, and `Put`.

## [01]-[INDEX]

- [02]-[SERVE]: Complete Compute and Artifact service composition over one daemon-owned repository.

## [02]-[SERVE]

- Owner: `GeometryServe.tessellate`, `GeometryServe.fetch`, and `GeometryServe.put` implement every generated method on `ComputeService` and `ArtifactService`; `served()` is the one application roster consumed by both `mount` and `companion`.
- Law: `Tessellate` passes the admitted generated request whole into the persistent daemon and returns `TessellateResponse.artifact` from its published `ArtifactRef`. No flat `artifact_hash`, raw body, or geometry-authored wire carrier remains.
- Law: `Fetch` opens the requested `sha256` through `daemon.repository.opened`, whose streamed arm holds custody across every yield and lets repository staging prove the stored body, and yields only `rasm.runtime.transport.artifact.fetch_responses`. `Put` unwraps generated request envelopes through `put_frames`, receives through `rasm.runtime.transport.artifact.receive`, persists its helper-owned path by atomic overwrite inside the helper context, and returns the same generated reference.
- Law: `BodyAdmission(SERVER)` in `ServerHost` validates every unary and streamed message once. Serve adds only aggregate artifact proof and never repeats descriptor validation or field rules.
- Law: one `Resource.REQUEST` charge settles each handled route, including refusals. Decode-time contract refusals never reach a handler and therefore carry no handler charge.
- Law: the caller deadline admitted by `ServerHost` rides the daemon kernel. Artifact helper contexts close on success, refusal, cancellation, and downstream stream termination.
- Entry: generated overrides collapse runtime rails through `ServerHost.settle`; `mount` proves geometry graduation before registering either application.
- Receipt: serve emits no parallel receipt family. `@receipted` harvests the daemon once after each drive.
- Packages: generated compute/artifact protocols, runtime `transport/artifact`, the daemon/graduation owners, and runtime host/journal custody.
- Growth: a new artifact integrity rule lands in the contracts helper; a new generated RPC adds one override and route charge here. Neither change adds a local frame dialect or storage port.
- Boundary: runtime owns bind, Connect codecs, body admission, health, typed fault egress, and lifecycle. Geometry contributes generated application rows and one servicer instance.

```python signature
from collections.abc import AsyncIterator
from functools import partial
from typing import TYPE_CHECKING, Any, Final, assert_never, override

from connectrpc.request import RequestContext
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block
from rasm.runtime.transport.artifact import ArtifactError, ArtifactRefusal, fetch_responses, put_frames, receive, rendered
from rasm.contracts.rasm.contracts.artifact.artifact_connect import ArtifactService, ArtifactServiceASGIApplication
from rasm.contracts.rasm.contracts.artifact.artifact_pb import (
    ArtifactRef,
    FetchRequest,
    FetchResponse,
    PutRequest,
    PutResponse,
)
from rasm.contracts.rasm.contracts.compute.compute_connect import ComputeService, ComputeServiceASGIApplication
from rasm.contracts.rasm.contracts.compute.compute_pb import TessellateRequest, TessellateResponse

from rasm.data.tabular.columnar import DatasetRef
from rasm.data.tabular.journal import FactJournal
from rasm.geometry.graduation import EvidenceScope, GeometryLeg, bench_seam, bench_subject, evidence_run, registered
from rasm.geometry.mesh.daemon import TessellationDaemon, TessellationResult
from rasm.runtime.admission import RuntimeContext, SecretBoundary
from rasm.runtime.faults import TERMINAL, BoundaryFault, FaultRow, RuntimeRail, rostered
from rasm.runtime.journal import Custody, Journal, Ledger, MeterFact, Resource
from rasm.runtime.profiles import BenchmarkReceipt
from rasm.runtime.receipts import DEFAULT_SCOPE, OPEN, ScopeKey, receipted
from rasm.runtime.roots import StoreFault
from rasm.runtime.serve import Served, ServerHost, companion_app

if TYPE_CHECKING:
    from cyclopts import App


TESSELLATE_ROUTE: Final[str] = "tessellate"
FETCH_ROUTE: Final[str] = "fetch"
PUT_ROUTE: Final[str] = "put"

SERVE_EMPTY: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.SERVE,
    point="tessellate.result",
    arm="boundary",
    defect="empty-tessellation",
    retriability=TERMINAL,
)
SERVE_ABSENT: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.SERVE,
    point="fetch.artifact",
    arm="config",
    defect="artifact-absent",
    retriability=TERMINAL,
    slots=("artifact",),
)
SERVE_ARTIFACT: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.SERVE,
    point="artifact.transfer",
    arm="boundary",
    defect="integrity-refused",
    retriability=TERMINAL,
    slots=("proof",),
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([SERVE_EMPTY, SERVE_ABSENT, SERVE_ARTIFACT]))


async def _served[T](route: str, composition: ScopeKey, settled: RuntimeRail[T]) -> RuntimeRail[T]:
    return (await Journal.record(MeterFact(resource=Resource.REQUEST, quantity=1, surface=route), scope=composition)).bind(
        lambda _landed: settled
    )


def _ledger(evidence: Option[tuple[DatasetRef, DatasetRef, SecretBoundary]]) -> RuntimeRail[Option[tuple[Ledger, Custody]]]:
    match evidence:
        case Option(tag="some", some=(facts, custody, boundary)):
            return FactJournal.of(facts, custody).map(
                lambda ledger: Some((ledger, Custody.vault(boundary, EvidenceScope.MESH_SERVE.value)))
            )
        case _:
            return Ok(Nothing)


def _receipt(result: TessellationResult) -> TessellateResponse:
    return TessellateResponse(
        content_key=result.content_key.wire_bytes,
        element_count=result.element_count,
        triangle_count=result.triangle_count,
        semantic=result.semantic,
        spill=result.spill,
        artifact=result.artifact,
    )


def _transfer(refusal: ArtifactRefusal) -> BoundaryFault:
    # `rendered` is the library's own law token for the refusal — the retired `.proof.value` read was a member
    # `ArtifactError` never carried.
    return SERVE_ARTIFACT.raised(rendered(refusal))


class GeometryServe(ComputeService, ArtifactService):
    def __init__(
        self,
        daemon: TessellationDaemon,
        *,
        composition: ScopeKey = DEFAULT_SCOPE,
    ) -> None:
        self._daemon = daemon
        self._composition = composition

    def served(self) -> Block[Served[Any]]:
        return Block.of_seq([(ComputeServiceASGIApplication, self), (ArtifactServiceASGIApplication, self)])

    def mount(self, host: ServerHost) -> RuntimeRail[int]:
        return registered(self._composition).bind(lambda _install: host.mount(self.served()))

    @override
    async def tessellate(
        self,
        request: TessellateRequest,
        ctx: RequestContext[TessellateRequest, TessellateResponse],
        /,
    ) -> TessellateResponse:
        # `ComputeService.Tessellate` binds its payload DIRECTLY: nothing unwraps here and the request reaches the daemon as Connect decoded it.
        _ = ctx
        return ServerHost.settle(await self._tessellate(request, ServerHost.admitted()))

    @override
    async def fetch(
        self,
        request: FetchRequest,
        ctx: RequestContext[FetchRequest, FetchResponse],
        /,
    ) -> AsyncIterator[FetchResponse]:
        _ = ctx
        # `FetchRequest` carries the bare `sha256` coordinate; the single-field envelope it once nested is gone.
        ServerHost.settle(await _served(FETCH_ROUTE, self._composition, Ok(None)))
        try:
            async for response in self._daemon.repository.opened(request.sha256, fetch_responses):
                yield response
        except ArtifactError as refused:
            ServerHost.settle(Error(_transfer(refused.refusal)))
        except BoundaryFault as refused:
            match refused:
                case BoundaryFault(tag="domain", domain=(_, StoreFault(tag="missing"))):
                    ServerHost.settle(Error(SERVE_ABSENT.raised(request.sha256.hex())))
                case _:
                    ServerHost.settle(Error(refused))

    @override
    async def put(
        self,
        request: AsyncIterator[PutRequest],
        ctx: RequestContext[PutRequest, PutResponse],
        /,
    ) -> PutResponse:
        _ = ctx
        return PutResponse(artifact=ServerHost.settle(await self._put(request)))

    def companion(
        self,
        evidence: Option[tuple[DatasetRef, DatasetRef, SecretBoundary]] = Nothing,
    ) -> RuntimeRail[App]:
        return _ledger(evidence).map(
            lambda bound: companion_app(self.served(), ledger=bound, composition=self._composition)
        )

    async def _tessellate(
        self,
        request: TessellateRequest,
        context: RuntimeContext,
    ) -> RuntimeRail[TessellateResponse]:
        rail = await evidence_run(
            EvidenceScope.MESH_SERVE,
            TESSELLATE_ROUTE,
            partial(self._drive, request, context.budget),
            composition=self._composition,
        )
        return await _served(
            TESSELLATE_ROUTE,
            self._composition,
            rail.bind(
                lambda results: results.try_head()
                .map(lambda head: Ok(_receipt(head)))
                .default_value(Error(SERVE_EMPTY.raised()))
            ),
        )

    async def _drive(
        self,
        request: TessellateRequest,
        budget: Option[float],
    ) -> RuntimeRail[Block[TessellationResult]]:
        rail = await self._daemon.tessellate(request, budget=budget)
        self._harvest(self._daemon)
        return rail

    @receipted(OPEN)
    def _harvest(self, daemon: TessellationDaemon) -> TessellationDaemon:
        return daemon

    async def _put(self, request: AsyncIterator[PutRequest]) -> RuntimeRail[ArtifactRef]:
        try:
            # `receive` yields a `Result` — the frame law's refusal rides the value, never a raise — so the seal
            # settles onto the rail here and only the stream's own `ArtifactError` crosses as an exception.
            async with receive(put_frames(request)) as sealed:
                match sealed:
                    case Result(tag="ok", ok=owned):
                        published = await self._daemon.repository.put(owned)
                    case Result(tag="error", error=refusal):
                        published = Error(_transfer(refusal))
                    case _ as unreachable:
                        assert_never(unreachable)
                return await _served(PUT_ROUTE, self._composition, published)
        except ArtifactError as refused:
            return await _served(PUT_ROUTE, self._composition, Error(_transfer(refused.refusal)))
        except BoundaryFault as refused:
            return await _served(PUT_ROUTE, self._composition, Error(refused))

    def bench(
        self,
        request: TessellateRequest,
        context: RuntimeContext,
        *,
        rounds: int = 32,
        warmup: int = 4,
    ) -> RuntimeRail[BenchmarkReceipt]:
        return bench_seam(
            bench_subject(EvidenceScope.MESH_SERVE, TESSELLATE_ROUTE),
            partial(self._tessellate, request, context),
            rounds=rounds,
            warmup=warmup,
            composition=self._composition,
        )
```

## [03]-[RESEARCH]

(none)
