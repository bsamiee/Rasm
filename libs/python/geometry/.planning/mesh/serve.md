# [PY_GEOMETRY_MESH_SERVE]

Geometry's wire owner: the servicer putting the daemon's flagship output behind the C# `ComputeService`/`ArtifactSync` contract — it registers `Route` rows into the runtime `ServerHost`, decodes the C#-minted `TessellationRequest`, drives the `mesh/daemon` `TessellationDaemon`, answers the `TessellationReceipt` field floor, and streams the GLB back as 64 KiB `FrameEdge`-framed `ArtifactFrame` rows. Registration direction is boundary law: serve owns the geometry-side composition root and registers INTO the runtime host; runtime never imports geometry.

Geometry authors NO wire vocabulary: `TessellationRequest`/`TessellationReceipt`/`ArtifactFrame` import by symbol from the runtime `transport/shapes` registry (C#-minted, geometry the named consumer), and the `grpcio`/`protobuf` substrate is consumed only through the runtime transport owners — no proto, stub, or codegen surface exists here. Serve derives exactly one hash — the seed-zero (`Some(0)`) `XxHash128` wire key equal to the C# `RepresentationContentHash` byte-for-byte; the daemon's policy-folded cache key never rides the wire (the two-key discipline). Daemon and serve split by law: the daemon tessellates, caches, and keys; serve registers, frames, and streams — never tessellating, re-keying, or reaching past the daemon's returned results.

## [01]-[INDEX]

- [01]-[SERVE]: geometry servicer composition root — route registration, request decode, daemon drive, receipt-floor answer, and the 64 KiB `ArtifactFrame` fold.

## [02]-[SERVE]

- Owner: `GeometryServe` — the composition root holding the daemon and its lane; serve holds NO cache, NO kernel, and NO wire shape of its own.
- Cases: one route row per served method — `Tessellate` answers the receipt floor, the `ArtifactSync` `Sync` leg folds a parked GLB back as its framed rows; a new geometry-served method is one route row binding an existing registry codec pair to a railed handler — a new field floor is the runtime registry's one C#-minted row-pair growth, never a geometry-authored shape.
- Entry: `mount` is the runtime `Entrypoint` fold's install step, so lifecycle — bind, credentials, health, graceful drain — stays runtime-owned and geometry contributes only rows; `_tessellate` returns through the graduation weave seeded `EvidenceScope.MESH_SERVE`, its span nested INTERNAL under the host interceptor's SERVER span, so serve latency is the geometry evidence-duration row and pool depth stays the lane spine's own gauges.
- Receipt: serve emits nothing of its own — the `@receipted` harvest reads the daemon's accumulated `contribute` stream once per drive, so the chain carries every tessellation fact exactly once; serve mints no graduation subject, since the daemon's product is wire geometry, not evidence.
- Packages: the daemon, cad, and graduation-weave vocabulary from geometry, the wire shapes and serve surface from runtime, `msgspec.to_builtins`, `zlib.crc32`, and `expression` per the fence imports.
- Growth: a new framed artifact class is the runtime registry's one row pair and one `sync`-style producer here; a per-element streaming fan is one `create_memory_object_stream` composition over the same `_frames` fold.
- Boundary: the C# `Rasm.Compute/Runtime` owns the `ComputeService`/`ArtifactSync` proto contract both ends compile; the daemon owns the cache and the kernel.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import zlib
from functools import partial
from typing import Final

from expression import Error, Ok, Result, Some
from expression.collections import Block, Map
from msgspec import to_builtins

from rasm.geometry.graduation import EvidenceScope, evidence_run
from rasm.geometry.mesh.cad import CANONICAL_TESSELLATION, BridgeFormat, TessellationPolicy
from rasm.geometry.mesh.daemon import TessellationDaemon, TessellationResult, TessellationSource
from rasm.runtime.admission import RuntimeContext
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt, Redaction, receipted
from rasm.runtime.serve import Route, RouteArity, ServerHost
from rasm.runtime.shapes import ArtifactFrame, TessellationReceipt, TessellationRequest

# --- [CONSTANTS] ------------------------------------------------------------------------

# C#'s ARTIFACT_FRAMES FrameEdge both ends hold; framing is data over this edge, never a hand-rolled message loop.
FRAME_EDGE: Final[int] = 64 * 1024
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # tessellation facts carry no secret field

# --- [OPERATIONS] -----------------------------------------------------------------------


def _policy(request: TessellationRequest) -> TessellationPolicy:
    # an absent entry falls to the canonical default — one knob vocabulary both ends hold, never a raw dict into the daemon.
    echo = request.policy
    return TessellationPolicy(
        deflection=float(echo.get("deflection", CANONICAL_TESSELLATION.deflection)),
        angle_tolerance=float(echo.get("angle_tolerance", CANONICAL_TESSELLATION.angle_tolerance)),
    )


def _source(request: TessellationRequest) -> "RuntimeRail[TessellationSource]":
    # an unknown modality is a typed wire fault naming the value, never a silent default arm.
    match request.source_modality:
        case "ifc":
            return Ok(TessellationSource(ifc=request.source))
        case "step" | "iges" as fmt:
            return Ok(TessellationSource(cad=(request.source, BridgeFormat(fmt))))
        case unknown:
            return Error(BoundaryFault(wire=(f"serve.tessellate.{unknown}", 0)))


def _wire_hash(glb: bytes) -> ContentKey:
    # bare C# XxHash128.HashToUInt128(span) parity path, proven by the "glb-by-key" reproduction pin;
    # DISTINCT from the daemon's policy-folded cache key.
    return ContentIdentity.key("glb", glb, seed=Some(0))


def _frames(wire_key: ContentKey, glb: bytes) -> Block[ArtifactFrame]:
    # whole-artifact hash rides the receipt once; each frame carries only its per-frame crc32 producer obligation.
    return Block.of_seq(range(0, len(glb), FRAME_EDGE)).map(
        lambda off: ArtifactFrame(
            artifact_id=wire_key.memory,  # the ContentKey 16-byte little-endian projection
            artifact_bytes=len(glb),
            offset=off,
            frame_crc=zlib.crc32(glb[off : off + FRAME_EDGE]),
            payload=glb[off : off + FRAME_EDGE],
        )
    )


def _receipt(result: TessellationResult) -> TessellationReceipt:
    # a response the consumer cannot dedupe or attribute is a chain break.
    return TessellationReceipt(
        content_key=result.content_key.hex,
        element_count=result.element_count,
        triangle_count=result.triangle_count,
        semantic_header=to_builtins(result.semantic),
        artifact_hash=_wire_hash(result.glb).hex,
        replay_phase=result.replay,
    )


# --- [SERVICES] -------------------------------------------------------------------------


class GeometryServe:
    def __init__(self, daemon: TessellationDaemon, lane: LanePolicy) -> None:
        self._daemon = daemon
        self._lane = lane  # the lane a non-canonical policy echo mints its per-request daemon over
        self._served: Map[bytes, TessellationResult] = Map.empty()  # wire-key memory bytes -> served result, the sync leg's source

    def mount(self, host: ServerHost) -> "RuntimeRail[int]":
        # rows in, count out; ServerHost resolves both codec names per row under ACCUMULATE, never first-miss.
        return host.register(
            Block.of_seq([
                Route(
                    service="rasm.compute.v1.ComputeService",
                    method="Tessellate",
                    descriptor="tessellate",
                    request="tessellate",
                    response="tessellation_receipt",
                    handler=self._tessellate,
                ),
                Route(
                    service="rasm.compute.v1.ArtifactSync",
                    method="Sync",
                    descriptor="artifact_sync",
                    request="artifact_frame",
                    response="artifact_frame",
                    handler=self._sync,
                    arity=RouteArity.BIDI,
                ),
            ])
        )

    async def _tessellate(self, request: TessellationRequest, context: RuntimeContext) -> "RuntimeRail[TessellationReceipt]":
        # decode -> drive -> harvest -> answer; the head result answers the floor while every result parks for the sync leg.
        # `evidence_run` seeds MESH_SERVE: the weave span nests INTERNAL under the ServerHost interceptor's SERVER span,
        # and serve latency lands as the geometry evidence-duration row keyed by scope.
        match _source(request):
            case Result(tag="ok", ok=source):
                rail = await evidence_run(EvidenceScope.MESH_SERVE, "tessellate", partial(self._drive, source, _policy(request)))
                return rail.bind(
                    lambda results: results.try_head()
                    .map(lambda head: Ok(_receipt(head)))
                    .default_value(Error(BoundaryFault(wire=("serve.tessellate.empty", 0))))
                )
            case Result(tag="error") as refused:
                return refused

    async def _drive(self, source: TessellationSource, mesher: TessellationPolicy) -> "RuntimeRail[Block[TessellationResult]]":
        # a sharpened echo mints a per-request daemon over the same lane so the cache keys stay policy-distinct —
        # never a mutated shared daemon.
        daemon = self._daemon if mesher == CANONICAL_TESSELLATION else TessellationDaemon(self._lane, mesher)
        rail = await daemon.tessellate(source)
        self._harvest(daemon)
        return rail.map(self._park)

    @receipted(_REDACTION)
    def _harvest(self, daemon: TessellationDaemon) -> TessellationDaemon:
        # one harvest point — receipts stay on the daemon, serve adds no parallel receipt rail.
        return daemon

    def _park(self, results: Block[TessellationResult]) -> Block[TessellationResult]:
        for result in results:  # Exemption: the served-results index is the host's one mutating seam.
            self._served = self._served.add(_wire_hash(result.glb).memory, result)
        return results

    async def _sync(self, request: ArtifactFrame, context: RuntimeContext) -> "RuntimeRail[Block[ArtifactFrame]]":
        # bidi leg: each inbound frame names the artifact_id it wants; the parked GLB folds back as its framed rows.
        return self.sync(request.artifact_id)

    def sync(self, artifact_id: bytes) -> "RuntimeRail[Block[ArtifactFrame]]":
        # served GLB fetched by its wire-key memory bytes — the exact artifact_id every frame carries — never re-tessellated.
        return (
            self._served.try_find(artifact_id)
            .map(lambda result: Ok(_frames(_wire_hash(result.glb), result.glb)))
            .default_value(Error(BoundaryFault(wire=(f"serve.sync.{artifact_id.hex()}", 0))))
        )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
