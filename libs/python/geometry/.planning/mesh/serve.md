# [PY_GEOMETRY_MESH_SERVE]

The geometry-side wire owner — the servicer that puts the daemon's flagship output behind the C# `ComputeService`/`ArtifactSync` contract. `GeometryServe` registers the tessellation servicer in the runtime `ServerHost` lifecycle as `Route` rows (descriptor id + wire-registry codec-pair names + railed handler), decodes the C#-minted `TessellationRequest` by symbol from `rasm.runtime.shapes`, drives the `mesh/daemon` `TessellationDaemon` for the per-element GLB, and answers the `TessellationReceipt` field floor — content key, element/triangle tally, `SemanticHeader` schema/project, whole-artifact XxHash128, and the `replay` provenance phase so the C# artifact index distinguishes by-reference replay from fresh iteration. The GLB streams back over the `ArtifactSync` leg as 64 KiB `FrameEdge`-framed `ArtifactFrame` rows, each carrying its per-frame Crc32 and offset, the whole-artifact hash the seed-zero (`Some(0)`) `XxHash128` wire key that equals the C# `RepresentationContentHash` entry byte-for-byte. Registration direction is boundary law: serve owns the geometry-side composition root — it registers the servicer rows and composes the runtime `Entrypoint` install → admit → serve → drain fold from the geometry side; runtime never imports geometry.

Geometry authors NO wire vocabulary: `TessellationRequest`/`TessellationReceipt`/`ArtifactFrame` import by symbol from the runtime `transport/shapes` registry (the C# mints are `Rasm.Bim/Exchange/tessellation` + `Rasm.Compute/Runtime/codecs`; the registry rows `"tessellate"`/`"tessellation_receipt"`/`"artifact_frame"` are LANDED with geometry named the consumer), and no proto, stub, or codegen surface exists here — the `grpcio`/`grpcio-tools`/`protobuf` substrate is consumed only through the runtime transport owners. The daemon/serve boundary: the daemon tessellates, caches, and keys — serve registers, frames, and streams, never tessellating, re-keying the policy-folded cache key, or reaching past the daemon's returned `RuntimeRail[Block[TessellationResult]]`; the one hash serve derives is the seed-zero wire key through the runtime identity owner's buffer modality — the reproduce-under-parity mint the identity page pins to this consumer, distinct from the daemon's cache key by the two-key discipline.

## [01]-[INDEX]

- [01]-[SERVE]: the geometry servicer composition root — the `ROUTES` row builder over the runtime `Route`/`ServerHost.register` surface, the `TessellationRequest` decode (source modality, source bytes, the `TessellationPolicy` echo), the daemon drive, the `TessellationReceipt` field-floor answer, the 64 KiB `FrameEdge` `ArtifactFrame` fold with per-frame Crc32, and the `@receipted` harvest of the daemon's accumulated contributor stream.

## [02]-[SERVE]

- Owner: `GeometryServe` — the geometry-side composition root holding the `TessellationDaemon` and its lane, exposing `mount(host)` (the one registration fold handing the `ROUTES`-built `Block[Route]` to `ServerHost.register`, whose wire-registry lookup resolves each row's request/response codec pair by name under `Disposition.ACCUMULATE`) and `sync(key)` (the `ArtifactSync` frame producer folding a served artifact into its 64 KiB framed rows); `_tessellate` the one railed handler decoding the request, driving the daemon, harvesting the daemon's receipts under the page's `@receipted` aspect, and answering the receipt floor; `_frames` the pure frame fold chunking the GLB at the `FRAME_EDGE` 64 KiB boundary into `ArtifactFrame` rows carrying `artifact_id` (the wire-hash bytes), `artifact_bytes` (the whole length), per-frame `offset`, the `zlib.crc32` per-frame `frame_crc`, and the `payload` chunk; `_policy` the request-echo decode lifting the C#-echoed `policy` map into the geometry-owned `TessellationPolicy` (deflection/angular knobs, defaults on absence — never a raw dict threaded into the daemon); `_wire_hash` the seed-zero derivation through the runtime identity owner. Serve holds NO cache, NO kernel, and NO wire shape of its own.
- Cases: one servicer row today — `("rasm.compute.v1.ComputeService", "Tessellate", "tessellate", "tessellate", "tessellation_receipt", _tessellate)` — plus the `ArtifactSync` frame leg the `sync` producer feeds; a new geometry-served method is one `ROUTES` row binding an existing registry codec pair to a railed handler, never a hand-wired servicer beside the `register` fold and never a new wire shape (a new field floor is the runtime registry's one row-pair growth, C#-minted).
- Entry: `mount(host)` returns `RuntimeRail[int]` — the registered-row count — by handing the built rows to `ServerHost.register`, which resolves both codec names per row through the wire registry and accumulates every unresolvable row into one fault (never first-miss); the geometry daemon process composes the runtime `Entrypoint` install → admit → serve → drain fold with `mount` as its install step, so lifecycle (bind, credentials, health, graceful drain) stays runtime-owned and geometry contributes only rows. `_tessellate(request, context)` is the railed handler — the runtime `RailHandler` shape, request `Struct` first, `RuntimeContext` second: it matches `request.source_modality` onto the daemon's `TessellationSource` union (`"ifc"` → `ifc(source)`, `"step"`/`"iges"` → `cad(source, BridgeFormat(modality))` — the modality vocabulary is the source ADT's, an unknown modality a typed `wire` fault naming the value), decodes the `policy` echo through `_policy`, awaits `daemon.tessellate`, harvests the daemon's accumulated `contribute` stream once under the page's `@receipted` `_harvest` aspect (receipts stay on the daemon; serve is the harvest point), and maps the head result onto the `TessellationReceipt` floor — `content_key` the daemon's policy-folded cache key hex, the tallies, `semantic_header` the packed schema/project, `artifact_hash` the seed-zero wire key hex, `replay_phase` the result's provenance — every floor field decoded by symbol, a response the consumer cannot dedupe or attribute being a chain break, never a doc gap. `sync(key)` folds the served GLB (fetched from the daemon's returned results held by the serve session, never re-tessellated) through `_frames` into the `Block[ArtifactFrame]` the runtime `ArtifactSync` bidi leg streams.
- Auto: `_frames` is one pure fold — `Block.of_seq(range(0, len(glb), FRAME_EDGE))` maps each offset to its `ArtifactFrame(artifact_id=wire_hash_bytes, artifact_bytes=len(glb), offset=off, frame_crc=zlib.crc32(chunk), payload=chunk)` row, so framing is data over one constant, never a hand-rolled message loop; the per-frame Crc32 is the frame field's producer obligation (the wire shape carries `frame_crc: WireU64`), and the whole-artifact integrity is the seed-zero XxHash128 the receipt's `artifact_hash` carries — computed ONCE per artifact through `_wire_hash`, never per frame. `_wire_hash` composes the runtime identity owner's buffer modality with `seed=Some(0)` — the bare C# `XxHash128.HashToUInt128(span)` parity path the identity page pins to this consumer and the landed `rasm.runtime.reproduction` `name="glb-by-key"` design-pin proves; the daemon's policy-folded cache key never rides the wire as the artifact hash (the two-key discipline). `_policy` reads the echoed `deflection`/`angle_tolerance` map entries into `TessellationPolicy`, absent entries falling to `CANONICAL_TESSELLATION`'s fields, so the C# request echo and the geometry mint agree on one knob vocabulary.
- Receipt: serve emits nothing of its own — the `@receipted(_REDACTION)` `_harvest` aspect harvests the DAEMON's structurally-conforming `contribute` stream once per drive (the daemon accumulates per-source `fact` and `rejected` rows; serve is the one harvest point after the drain fold), so the receipt chain carries every tessellation fact exactly once and serve adds no parallel receipt rail. Serve mints no graduation subject — the daemon's product is wire geometry, not evidence, and the downstream repair/reconstruction owners graduate the conditioned solid.
- Packages: geometry (`mesh/daemon` `TessellationDaemon`/`TessellationSource`/`TessellationResult` the served engine, `mesh/cad` `BridgeFormat`/`TessellationPolicy`/`CANONICAL_TESSELLATION` the modality and knob vocabulary), runtime `transport/shapes` (`TessellationRequest`/`TessellationReceipt`/`ArtifactFrame` by symbol — the C#-minted wire rows, zero geometry-authored wire vocabulary), runtime `transport/serve` (`Route`/`ServerHost` the registration surface, the `Entrypoint` install → admit → serve → drain fold the daemon process composes), runtime (`RuntimeRail`/`BoundaryFault`, `ContentIdentity`/`Some(0)` the seed-zero wire-key derivation through `rasm.runtime.identity`, `Receipt`/`Redaction`/`receipted` the harvest aspect, `LanePolicy` threaded to the daemon), `msgspec` (`to_builtins` the `SemanticHeader`-to-`Packed` projection), `zlib` (`crc32` the per-frame integrity field's producer obligation — stdlib at the framing boundary), `expression` (`Block`/`Map`/`Ok`/`Error`).
- Growth: a new served method is one `ROUTES` row over an existing registry codec pair; a new framed artifact class (the next geometry/data streamed artifact) is the runtime registry's one row pair plus one `sync`-style producer here; a per-element streaming fan (one `ArtifactFrame` stream per element GLB) is one `create_memory_object_stream` composition over the same `_frames` fold when the viewer demands element-grain delivery; serve latency/pool metrics are one graduation-weave composition, never a page-local instrument mint; zero new surface.
- Boundary: serve never tessellates, never re-keys the cache key, never reaches past the daemon's returned results, and never authors a wire shape, proto, stub, or codegen surface; runtime never imports geometry (serve registers INTO the runtime host — the registration direction is the law); the daemon owns the cache and the kernel; the C# `Rasm.Compute/Runtime` owns the `ComputeService`/`ArtifactSync` proto contract both ends compile. The deleted forms: a hand-wired `grpc.Server`/servicer beside the `ServerHost.register` fold; a geometry-minted `TessellationRequestWire` or any parallel shape beside the one registry row; a policy-folded seed on the artifact hash (the wire key is seed-zero, the cache key policy-folded — mixing them is the named drift defect); a re-tessellation inside `sync` where the served results are held; a per-frame whole-artifact hash where the artifact hashes once and the frames carry Crc32; a raw `dict` policy threaded into the daemon where `_policy` lifts the echo into `TessellationPolicy`; a serve-local receipt rail where the daemon's `contribute` stream is the one evidence source; and an unregistered streaming loop where the `ArtifactSync` registry row and the runtime host own the leg.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import zlib
from typing import Final

from expression import Error, Ok, Some
from expression.collections import Block, Map
from msgspec import to_builtins

from rasm.geometry.mesh.cad import CANONICAL_TESSELLATION, BridgeFormat, TessellationPolicy
from rasm.geometry.mesh.daemon import TessellationDaemon, TessellationResult, TessellationSource
from rasm.runtime.admission import RuntimeContext
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt, Redaction, receipted
from rasm.runtime.serve import Route, ServerHost
from rasm.runtime.shapes import ArtifactFrame, TessellationReceipt, TessellationRequest

# --- [CONSTANTS] ------------------------------------------------------------------------

# the ArtifactSync frame boundary — one constant, the C# Runtime/transport ARTIFACT_FRAMES FrameEdge
# both ends hold; framing is data over this edge, never a hand-rolled message loop.
FRAME_EDGE: Final[int] = 64 * 1024
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # tessellation facts carry no secret field

# --- [OPERATIONS] -----------------------------------------------------------------------


def _policy(request: TessellationRequest) -> TessellationPolicy:
    # the C#-echoed knob map lifts into the geometry-owned TessellationPolicy — one knob vocabulary
    # both ends hold; an absent entry falls to the canonical default, never a raw dict into the daemon.
    echo = request.policy
    return TessellationPolicy(
        deflection=float(echo.get("deflection", CANONICAL_TESSELLATION.deflection)),
        angle_tolerance=float(echo.get("angle_tolerance", CANONICAL_TESSELLATION.angle_tolerance)),
    )


def _source(request: TessellationRequest) -> "RuntimeRail[TessellationSource]":
    # the modality vocabulary is the daemon's source ADT; an unknown modality is a typed wire fault
    # naming the value, never a silent default arm.
    match request.source_modality:
        case "ifc":
            return Ok(TessellationSource(ifc=request.source))
        case "step" | "iges" as fmt:
            return Ok(TessellationSource(cad=(request.source, BridgeFormat(fmt))))
        case unknown:
            return Error(BoundaryFault(wire=(f"serve.tessellate.{unknown}", 0)))


def _wire_hash(glb: bytes) -> ContentKey:
    # the seed-zero (Some(0)) XxHash128 wire key — the bare C# XxHash128.HashToUInt128(span) parity
    # path equal to the RepresentationContentHash entry byte-for-byte, proven against the landed
    # rasm.runtime.reproduction "glb-by-key" pin; DISTINCT from the daemon's policy-folded cache key.
    return ContentIdentity.key("glb", glb, seed=Some(0))


def _frames(wire_key: ContentKey, glb: bytes) -> Block[ArtifactFrame]:
    # one pure fold over the 64 KiB edge: each frame carries the artifact id (the wire-hash bytes),
    # the whole length, its offset, the per-frame zlib.crc32 (the frame_crc producer obligation the
    # wire shape carries), and the payload chunk — the whole-artifact hash rides the receipt once.
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
    # the consumer field floor decoded by symbol: content key, tally, semantic header, whole-artifact
    # hash, replay provenance — a response the consumer cannot dedupe or attribute is a chain break.
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
    # the geometry-side composition root: it holds the daemon, registers the servicer rows into the
    # runtime host, and keeps the served results for the ArtifactSync frame leg — no cache, no
    # kernel, no wire shape of its own.
    def __init__(self, daemon: TessellationDaemon, lane: LanePolicy) -> None:
        self._daemon = daemon
        self._lane = lane  # the lane a non-canonical policy echo mints its per-request daemon over
        self._served: Map[str, TessellationResult] = Map.empty()  # wire-hash hex -> served result, the sync leg's source

    def mount(self, host: ServerHost) -> "RuntimeRail[int]":
        # the one registration fold: rows in, count out — ServerHost resolves both codec names per
        # row through the wire registry under ACCUMULATE and owns bind/credential/health/drain; the
        # daemon process composes the runtime Entrypoint install -> admit -> serve -> drain fold
        # with this mount as its install step. Runtime never imports geometry.
        return host.register(
            Block.singleton(
                Route(
                    service="rasm.compute.v1.ComputeService",
                    method="Tessellate",
                    descriptor="tessellate",
                    request="tessellate",
                    response="tessellation_receipt",
                    handler=self._tessellate,
                )
            )
        )

    async def _tessellate(self, request: TessellationRequest, context: RuntimeContext) -> "RuntimeRail[TessellationReceipt]":
        # decode -> drive -> harvest -> answer: the daemon returns the results (the flagship egress),
        # the page's @receipted aspect harvests the daemon's accumulated contribute stream once, and
        # the head result answers the receipt floor while every result parks for the sync leg.
        match _source(request):
            case Ok(source):
                rail = await self._drive(source, _policy(request))
                return rail.bind(
                    lambda results: results.try_head()
                    .map(lambda head: Ok(_receipt(head)))
                    .default_value(Error(BoundaryFault(wire=("serve.tessellate.empty", 0))))
                )
            case Error(fault):
                return Error(fault)

    async def _drive(self, source: TessellationSource, mesher: TessellationPolicy) -> "RuntimeRail[Block[TessellationResult]]":
        # a canonical policy echo drives the held daemon; a sharpened echo mints a per-request daemon
        # over the same lane so the cache keys stay policy-distinct — never a mutated shared daemon.
        daemon = self._daemon if mesher == CANONICAL_TESSELLATION else TessellationDaemon(self._lane, mesher)
        rail = await daemon.tessellate(source)
        self._harvest(daemon)
        return rail.map(self._park)

    @receipted(_REDACTION)
    def _harvest(self, daemon: TessellationDaemon) -> TessellationDaemon:
        # the one harvest point: the aspect reads the daemon's structurally-conforming contribute
        # stream on exit — receipts stay on the daemon, serve adds no parallel receipt rail.
        return daemon

    def _park(self, results: Block[TessellationResult]) -> Block[TessellationResult]:
        for result in results:  # Exemption: the served-results index is the host's one mutating seam.
            self._served = self._served.add(_wire_hash(result.glb).hex, result)
        return results

    def sync(self, artifact_hash: str) -> "RuntimeRail[Block[ArtifactFrame]]":
        # the ArtifactSync frame producer: the served GLB fetched by its wire hash (never
        # re-tessellated), folded through the one 64 KiB frame fold the runtime bidi leg streams.
        return (
            self._served.try_find(artifact_hash)
            .map(lambda result: Ok(_frames(_wire_hash(result.glb), result.glb)))
            .default_value(Error(BoundaryFault(wire=(f"serve.sync.{artifact_hash}", 0))))
        )
```
