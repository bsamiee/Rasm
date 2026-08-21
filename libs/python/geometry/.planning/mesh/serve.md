# [PY_GEOMETRY_MESH_SERVE]

Geometry's wire owner: the servicer putting the daemon's flagship output behind the C# `ComputeService`/`ArtifactSyncService` contract — it registers `Route` rows into the runtime `ServerHost`, decodes the registry `TessellationRequest`, drives the `mesh/daemon` `TessellationDaemon`, answers the `TessellationReceipt` field floor, and streams the GLB back as 64 KiB `FrameEdge`-framed `ArtifactFrame` rows. Registration direction is boundary law: serve owns the geometry-side composition root and registers INTO the runtime host; runtime never imports geometry.

Geometry authors NO wire vocabulary: `TessellationRequest`/`TessellationReceipt`/`ArtifactFrame` import by symbol from the runtime `transport/shapes` registry (contract-minted, geometry the named tessellation producer), and the `grpcio`/`protobuf` substrate is consumed only through the runtime transport owners — no proto, stub, or codegen surface exists here. Serve derives NO hash at all: the seed-zero (`Some(0)`) `XxHash128` wire key equal to the C# `RepresentationContentHash` arrives on the `mesh/cad#BRIDGE` `GlbArtifact` its encoding kernel minted, so the servicer addresses payload bytes it never re-hashes and the two-key discipline holds at one mint site rather than two. Daemon and serve split by law: the daemon tessellates, caches, and cache-keys; serve registers, frames, streams, and holds the bounded ring the bidi sync leg answers from — never tessellating, keying, or reaching past the daemon's returned results.

## [01]-[INDEX]

- [02]-[SERVE]: geometry servicer composition root — the graduation install gate, one route roster serving both `mount` and the companion entry, request decode, daemon drive, receipt-floor answer, the bounded parked ring with its durable read-through, the 64 KiB `ArtifactFrame` fold, the per-route request charge, and the daemon-entry `(Ledger, Custody)` binding.

## [02]-[SERVE]

- Owner: `GeometryServe` — the composition root holding the daemon, its lane, the durable artifact tier, and the one bounded parked ring the `Sync` leg reads; serve holds NO tessellation cache, NO kernel, NO hash, and NO wire shape of its own, and the ring is a bounded ring rather than a cache because it never short-circuits work — a miss falls through to the durable tier and then re-drives the daemon, whose own content cache owns replay.
- Cases: one route row per served method — `Tessellate` answers the receipt floor, the `ArtifactSyncService` `Sync` leg folds a parked or durably-held GLB back as its framed rows; a new geometry-served method is one route row binding an existing registry codec pair to a railed handler — a new field floor is the runtime registry's one contract row-pair growth, never a geometry-authored shape. `routes()` is the one roster both `mount` and `companion` read, since two builders would let a daemon composition serve rows the mounted composition does not and the divergence would surface only as a method the C# rail cannot find.
- Law: `companion` is the daemon-entry composition root and the durable evidence plane's ONE binding site. Runtime owns every lifecycle stage the entry drives — bind, credentials, health, the sd-notify handshake, supervision, the ordered drain — and takes this folder's contribution as DATA, so geometry hands a route roster and a `(Ledger, Custody)` PAIR and imports no CLI surface. The pair is inseparable because a journal that lands rows it cannot shred is not a lawful plane, and it binds here because this is the only site that knows which datasets and which KEK boundary a deployment owns; `vault` is the posture rather than `local`, so the KEK resolves per call and a rotation reaches the next wrap with no rebind. An unbound `Nothing` is the honest unjournalled daemon the runtime boot installs no plane for — never a default ledger this folder would have to invent a store, a retention, and a key custody for.
- Law: the `Sync` leg answers ring first, durable tier second, typed absence fault last — a key past the ring horizon is NOT necessarily an unknown artifact, since a warm-restarted process and a fleet peer each hold no ring at all, so the read-through answers an unchanged model before a consumer is told to re-request a tessellation nobody needs to re-run. Serve still derives NO hash: the object is addressed by the exact `artifact_id` the frame carries, through the daemon's own `spill_path`, so the address IS the identity and a re-hash here would mint the second key this page exists without. A refused, absent, or unbound tier all answer the ONE unknown-artifact fault, because all three mean one thing to the consumer and surfacing a store transport fault would hand the C# rail a refusal it has no arm and no useful retry for.
- Law: the parked ring is BOUNDED by one `SERVED_DEPTH` policy value on the folder's only process-lifetime servicer — an unbounded index grows its resident set monotonically with every distinct model ever tessellated, since a serve process outlives every drain — so `_park` folds admissions and evictions in one pass over an insertion-ordered log and the ring holds the most recent `SERVED_DEPTH` artifacts, a `Sync` past the horizon answering the same typed absence fault an unknown id answers.
- Law: the caller-dialed deadline is SPENT here. The host admits it off the call's remaining time onto `RuntimeContext`, and `_tessellate` threads `context.budget` into the drive so the daemon carries it onto each unit's kernel and the lane folds it against its own standing bound — an abandonment on the calling side stops the tessellation instead of paying it out, and the lane's contained trip still answers the partial as a `rejected` receipt. A handler reading the request alone is the unbounded leg the admitted budget exists to close, and a serve-side cancel scope is the ruled-out shape because it strips the drain of the receipt the lane owes.
- Law: `mount` proves the graduation install BEFORE it registers a route — `registered(composition)` runs the charter census and mounts the pulse points under the servicer's own composition key, so a divergent charter row or a colliding pulse id refuses at admission with typed evidence rather than killing the first record of a live served call; the install rail binds the route registration, never runs beside it.
- Law: both served routes charge one `Resource.REQUEST` `MeterFact` at their handler settle, surfaced by the route they answered — the billing evidence that resource exists for, and the reason a route surface is spelled once and read by the weave operation, the bench subject, and the meter alike. The charge lands on the refusal too, since a fault still consumed the served slot a consumer's call occupied and billing successes alone hands every consumer free retries; a modality refused before the route is served occupies no slot and carries none. The meter rail BINDS into the verdict: an armed plane that cannot record a request means this deployment cannot account for what it just served, so the route refuses rather than answering unbilled. `_ledger`'s `Nothing` is the lawful unjournalled daemon — the runtime boot installs no plane and the three-state intake law makes every `Journal.record` on this page a no-op costing one map read, which is why the unjournalled composition needs no second code path anywhere in this folder.
- Law: `bench` rides the graduation `bench_seam` fold over the whole `_tessellate` entry — decode, daemon drive, receipt floor, the real tessellation seam the C# rail pays — under subject `rasm.geometry.mesh.serve.tessellate`; latency and throughput rows land beside the per-call evidence-duration histogram with zero instrument rows, and graduation's `bench_terminal` wraps the fold in the runtime `JobRun.bounded` envelope for a process-terminal run.
- Entry: `mount` is the runtime `Entrypoint` fold's install step, so lifecycle — bind, credentials, health, graceful drain — stays runtime-owned and geometry contributes only rows; `_tessellate` returns through the graduation weave seeded `EvidenceScope.MESH_SERVE`, its span nested INTERNAL under the host interceptor's SERVER span, so serve latency is the geometry evidence-duration row and pool depth stays the lane spine's own gauges. Every weave, install, bench, request-meter, per-request daemon, and companion-entry call threads the servicer's composition `ScopeKey`, so an embedded host's evidence, charter series, capability registry, and capsule captures all partition from the process root's on one key.
- Receipt: serve emits nothing of its own — the `@receipted` harvest reads the daemon's accumulated `contribute` stream once per drive, so the chain carries every tessellation fact exactly once; serve mints no graduation subject, since the daemon's product is wire geometry, not evidence.
- Packages: the daemon, cad, and graduation-weave vocabulary from geometry, the wire shapes, serve entry, store lane, and journal custody from runtime, the `FactJournal` `Ledger` implementer and its `DatasetRef` from data, `msgspec.to_builtins`, `zlib.crc32`, and `expression` per the fence imports.
- Growth: a new served method is one route surface constant, one `routes()` row, and one `_served` charge at its handler settle; a new framed artifact class is the runtime registry's one row pair and one `sync`-style producer here; a per-element streaming fan is one `create_memory_object_stream` composition over the same `_frames` fold; a deeper hold is one `SERVED_DEPTH` value, never a second index; a new served method is one `routes()` row both entries inherit; a second custody posture is one `Custody` instance at `_ledger`, zero entry edits.
- Boundary: the C# `Rasm.Compute/Runtime` owns the `ComputeService`/`ArtifactSyncService` proto contract both ends compile; the daemon owns the tessellation cache, the kernel, the durable spill, and the `spill_path` layout this leg reads; `mesh/cad#BRIDGE` owns the `GlbArtifact` carrier and its wire-key mint; the runtime entry owns daemon lifecycle whole, so no drain stage, supervision charge, or CLI command is authored here.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import zlib
from functools import partial
from typing import TYPE_CHECKING, Final

from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block, Map
from msgspec import to_builtins

from rasm.data.tabular.columnar import DatasetRef
from rasm.data.tabular.journal import FactJournal
from rasm.geometry.graduation import EvidenceScope, GeometryLeg, bench_seam, bench_subject, evidence_run, registered
from rasm.geometry.mesh.cad import CANONICAL_TESSELLATION, BridgeFormat, GlbArtifact, TessellationPolicy
from rasm.geometry.mesh.daemon import SpillKind, TessellationDaemon, TessellationResult, TessellationSource, spill_path
from rasm.runtime.admission import RuntimeContext, SecretBoundary
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, rostered
from rasm.runtime.journal import Custody, Journal, Ledger, MeterFact, Resource
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.profiles import BenchmarkReceipt
from rasm.runtime.receipts import DEFAULT_SCOPE, OPEN, Receipt, ScopeKey, receipted
from rasm.runtime.roots import ObjectStoreLane, StoreOp
from rasm.runtime.serve import Route, RouteArity, ServerHost, companion_app
from rasm.runtime.shapes import ArtifactFrame, TessellationReceipt, TessellationRequest, WireMethod, WireService

if TYPE_CHECKING:  # the cyclopts `App` is the runtime entry's own return shape; geometry annotates it and imports no CLI surface
    from cyclopts import App

# --- [CONSTANTS] ------------------------------------------------------------------------

# C#'s ARTIFACT_FRAMES FrameEdge both ends hold; framing is data over this edge, never a hand-rolled message loop.
FRAME_EDGE: Final[int] = 64 * 1024

# parked-ring depth on the folder's one process-lifetime servicer: the resident set is capped by this bound rather
# than by tessellation history, and a `Sync` past the horizon answers the unknown-artifact absence fault.
SERVED_DEPTH: Final[int] = 64

# the two served route surfaces, spelled ONCE and read by the weave operation, the bench subject, and the request
# meter alike, so a billing row and the method it charges for can never drift into two names for one route.
TESSELLATE_ROUTE: Final[str] = "tessellate"
SYNC_ROUTE: Final[str] = "sync"

# --- [TABLES] ---------------------------------------------------------------------------

# this module's whole raise roster: every servicer refusal anchors one row, so no answer spells a subject and
# the `rostered` door seats every row on the branch census, proving `geometry.mesh.serve` against a real module at import. All three retired the
# `wire` arm
# they rode with a hardcoded `0` code: `wire` is reserved for a REAL protocol code, and a zero standing in for one
# published a code the peer never issued while claiming the discriminant `wire` exists to carry. The two admission
# refusals now grade `config` — the deployment serves no such modality, and the artifact must be re-produced before
# any read succeeds, both FAILED_PRECONDITION at the gRPC edge rather than the INVALID_ARGUMENT a forged code bought
# — while an empty kernel result stays engine-side and keeps its INVALID_ARGUMENT grade. All TERMINAL: re-issuing the
# identical request refuses identically, so none joins the server's own re-drive set.
SERVE_MODALITY: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.SERVE, point="tessellate.modality", arm="config", defect="unserved-modality", retriability=TERMINAL, slots=("modality",)
)
SERVE_EMPTY: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.SERVE, point="tessellate.result", arm="boundary", defect="empty-tessellation", retriability=TERMINAL
)
SERVE_ABSENT: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.SERVE, point="sync.artifact", arm="config", defect="artifact-absent", retriability=TERMINAL, slots=("artifact",)
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([SERVE_MODALITY, SERVE_EMPTY, SERVE_ABSENT]))

# --- [OPERATIONS] -----------------------------------------------------------------------


async def _served[T](route: str, composition: ScopeKey, settled: "RuntimeRail[T]") -> "RuntimeRail[T]":
    # the billing evidence `Resource.REQUEST` exists for: ONE unit per served call at the handler's settle, surfaced
    # by the route it answered. The charge lands on the refusal too — a fault still consumed the served slot the
    # consumer's call occupied, and billing successes alone hands every consumer free retries on every fault. The
    # meter rail BINDS into the verdict: an armed plane that cannot record the request means this deployment cannot
    # account for what it just served, so the route refuses rather than answering unbilled. `_ledger`'s `Nothing`
    # installs no plane at all, and the three-state intake law makes every record here a lawful no-op costing one
    # map read, which is why the unjournalled companion needs no second code path.
    return (await Journal.record(MeterFact(resource=Resource.REQUEST, quantity=1, surface=route), scope=composition)).bind(
        lambda _landed: settled
    )


def _policy(request: TessellationRequest) -> TessellationPolicy:
    # an absent entry falls to the canonical default — one knob vocabulary both ends hold, never a raw dict into the daemon.
    echo = request.policy
    return TessellationPolicy(
        deflection=float(echo.get("deflection", CANONICAL_TESSELLATION.deflection)),
        angle_tolerance=float(echo.get("angle_tolerance", CANONICAL_TESSELLATION.angle_tolerance)),
    )


def _source(request: TessellationRequest) -> "RuntimeRail[TessellationSource]":
    # an unknown modality is a rostered refusal naming the value, never a silent default arm.
    match request.source_modality:
        case "ifc":
            return Ok(TessellationSource(ifc=request.source))
        case "step" | "iges" as fmt:
            return Ok(TessellationSource(cad=(request.source, BridgeFormat(fmt))))
        case unknown:
            return Error(SERVE_MODALITY.raised(unknown))


def _frames(artifact_id: bytes, glb: bytes) -> Block[ArtifactFrame]:
    # ONE framing fold over both answer paths — the parked carrier and the durable read-through — taking the id and
    # the octets rather than a carrier, because the store leg holds octets addressed BY that id and rebuilding a
    # `GlbArtifact` there would make serve assert a producer literal nothing on this page recorded. Whole-artifact
    # identity rides the receipt off the carrier's own key; each frame carries only its per-frame crc32 producer
    # obligation, and serve re-derives no identity the encoding kernel already minted.
    return Block.of_seq(range(0, len(glb), FRAME_EDGE)).map(
        lambda off: ArtifactFrame(
            artifact_id=artifact_id,  # the ContentKey 16-byte little-endian projection
            artifact_bytes=len(glb),
            offset=off,
            frame_crc=zlib.crc32(glb[off : off + FRAME_EDGE]),
            payload=glb[off : off + FRAME_EDGE],
        )
    )


def _unknown(artifact_id: bytes) -> "RuntimeRail[Block[ArtifactFrame]]":
    # ONE unknown-artifact answer both misses reach — a key past the ring horizon the durable tier does not hold,
    # and a composition that bound no tier at all — so a consumer re-requests the tessellation rather than reading
    # two refusal spellings for one absence or receiving a truncated stream.
    return Error(SERVE_ABSENT.raised(artifact_id.hex()))


def _ledger(evidence: "Option[tuple[DatasetRef, DatasetRef, SecretBoundary]]") -> "RuntimeRail[Option[tuple[Ledger, Custody]]]":
    # the durable evidence plane binds as a PAIR because a journal that lands rows it cannot shred is not a lawful
    # plane: the `FactJournal` implementer and the KEK custody posture arrive together or not at all. `vault` is the
    # posture rather than `local`, so the KEK resolves per call through the one credential reader and a rotation
    # reaches the next wrap with no rebind. `Nothing` is the honest UNJOURNALLED composition — the runtime boot then
    # installs no plane and every producer runs unjournalled — never a default ledger this folder would have to
    # invent a store, a retention, and a key custody for.
    match evidence:
        case Option(tag="some", some=(facts, custody, boundary)):
            return FactJournal.of(facts, custody).map(lambda ledger: Some((ledger, Custody.vault(boundary, EvidenceScope.MESH_SERVE.value))))
        case _:
            return Ok(Nothing)


def _receipt(result: TessellationResult) -> TessellationReceipt:
    # a response the consumer cannot dedupe or attribute is a chain break; both keys read off the result — the
    # daemon's policy-folded cache key and the artifact's own seed-zero wire key — with no second hash here.
    return TessellationReceipt(
        content_key=result.content_key.hex,
        element_count=result.element_count,
        triangle_count=result.triangle_count,
        semantic_header=to_builtins(result.semantic),
        artifact_hash=result.glb.wire_key.hex,
        replay_phase=result.replay,
    )


# --- [SERVICES] -------------------------------------------------------------------------


class GeometryServe:
    def __init__(
        self,
        daemon: TessellationDaemon,
        lane: LanePolicy,
        *,
        store: "Option[ObjectStoreLane]" = Nothing,
        composition: ScopeKey = DEFAULT_SCOPE,
    ) -> None:
        self._daemon = daemon
        self._lane = lane  # the lane a non-canonical policy echo mints its per-request daemon over
        self._store = store  # the durable artifact tier the read-through reads and every per-request daemon inherits
        self._composition = composition  # the custody key the install gate, weave, and bench fold all stamp
        self._served: Map[bytes, GlbArtifact] = Map.empty()  # wire-key memory bytes -> artifact, the sync leg's source
        self._order: Block[bytes] = Block.empty()  # admission-ordered ring log the bound evicts from the head

    def routes(self) -> Block[Route]:
        # the servicer's route roster as a VALUE: `mount` registers it into a host a caller already built, and
        # `companion` hands the same rows to the runtime entry that builds one. Two builders would let a daemon
        # composition serve a roster the mounted composition does not, and the divergence would only surface as a
        # method the C# rail cannot find.
        return Block.of_seq([
            Route(
                service=WireService.COMPUTE,
                method=WireMethod.TESSELLATE,
                descriptor="tessellate",
                request="tessellate",
                response="tessellation_receipt",
                handler=self._tessellate,
            ),
            Route(
                service=WireService.ARTIFACT_SYNC,
                method=WireMethod.SYNC,
                descriptor="artifact_sync",
                request="artifact_frame",
                response="artifact_frame",
                handler=self._sync,
                arity=RouteArity.BIDI,
            ),
        ])

    def mount(self, host: ServerHost) -> "RuntimeRail[int]":
        # install gate FIRST, then rows in and count out: `registered` proves every charter row against the runtime
        # census and mounts the geometry pulse points under this servicer's composition before a single route can
        # answer, so a divergent descriptor refuses here instead of killing the first record of a live call.
        # ServerHost resolves both codec names per row under ACCUMULATE, never first-miss.
        return registered(self._composition).bind(lambda _install: host.register(self.routes()))

    def companion(self, evidence: "Option[tuple[DatasetRef, DatasetRef, SecretBoundary]]" = Nothing) -> "RuntimeRail[App]":
        # the geometry companion daemon's composition root. Runtime owns the whole lifecycle — bind, credentials,
        # health, sd-notify handshake, supervision, ordered drain — and takes this folder's contribution as DATA, so
        # geometry hands a route roster and a `(Ledger, Custody)` pair and imports no CLI surface. The durable
        # evidence plane is the one capability a default cannot supply: the runtime entry's `Nothing` runs the daemon
        # unjournalled, and a companion that means to journal binds the pair HERE, which is the only place that
        # knows which datasets and which KEK boundary this deployment owns. Drains and charges stay empty because
        # the lane owns its own pool teardown and this folder supervises no child of its own — a row asserting one
        # would name a drain stage nothing drains. The servicer's own `composition` threads in, so the entry's
        # capability-invoke registry and capsule captures partition on the SAME key the ledger binding, the install
        # gate, the weave, and every request meter already stamp — one scope across every custody surface, where a
        # default here would strand an embedded host's daemon evidence in the process root's.
        return _ledger(evidence).map(lambda bound: companion_app(self.routes(), ledger=bound, composition=self._composition))

    async def _tessellate(self, request: TessellationRequest, context: RuntimeContext) -> "RuntimeRail[TessellationReceipt]":
        # decode -> drive -> harvest -> answer; the head result answers the floor while every result parks for the sync leg.
        # `evidence_run` seeds MESH_SERVE: the weave span nests INTERNAL under the ServerHost interceptor's SERVER span,
        # and serve latency lands as the geometry evidence-duration row keyed by scope.
        match _source(request):
            case Result(tag="ok", ok=source):
                rail = await evidence_run(
                    EvidenceScope.MESH_SERVE,
                    TESSELLATE_ROUTE,
                    partial(self._drive, source, _policy(request), context.budget),
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
            case Result(tag="error") as refused:
                # a modality the registry never declared is refused BEFORE the route is served, so it occupies no
                # served slot and carries no charge.
                return refused

    async def _drive(
        self, source: TessellationSource, mesher: TessellationPolicy, budget: "Option[float]"
    ) -> "RuntimeRail[Block[TessellationResult]]":
        # `budget` is the caller-dialed deadline the host lifted off `time_remaining()` into the admitted context, and
        # this is the site that spends it: the daemon threads it onto each unit's kernel, where the lane folds it
        # against its own standing bound. Dropped here, the whole tessellation runs under the composition-time
        # deadline alone and an abandoned call keeps paying for a GLB nobody will read — the admitted budget would be
        # a value the rail carries and no leg reads. A serve-side `move_on_after` is the ruled-out shape: it cancels
        # the drain out from under its own receipt, where the lane's contained trip still answers the partial.
        # a sharpened echo mints a per-request daemon over the same lane so the cache keys stay policy-distinct —
        # never a mutated shared daemon — inheriting the SAME durable tier, since a spill under a tier the
        # request-scoped daemon never saw would leave that policy's artifacts undurable.
        daemon = (
            self._daemon
            if mesher == CANONICAL_TESSELLATION
            else TessellationDaemon(self._lane, mesher, store=self._store, composition=self._composition)
        )
        rail = await daemon.tessellate(source, budget=budget)
        self._harvest(daemon)
        return rail.map(self._park)

    @receipted(OPEN)  # tessellation facts carry no secret field, so the runtime keep-all policy binds
    def _harvest(self, daemon: TessellationDaemon) -> TessellationDaemon:
        # one harvest point — receipts stay on the daemon, serve adds no parallel receipt rail.
        return daemon

    def _park(self, results: Block[TessellationResult]) -> Block[TessellationResult]:
        # Exemption: the parked ring is the host's one mutating seam. Admission and eviction fold in ONE pass over
        # the insertion-ordered log — a re-park of a key the ring already holds appends no second log entry, so the
        # horizon never evicts a live artifact, and the head slice past the bound drops from both the log and the map.
        fresh = results.map(lambda r: r.glb).filter(lambda artifact: artifact.wire_key.memory not in self._served)
        keys = self._order.append(fresh.map(lambda artifact: artifact.wire_key.memory))
        held = fresh.fold(lambda ring, artifact: ring.add(artifact.wire_key.memory, artifact), self._served)
        evicted = keys.take(max(len(keys) - SERVED_DEPTH, 0))
        self._order = keys.skip(len(evicted))
        self._served = evicted.fold(lambda ring, key: ring.remove(key), held)
        return results

    async def _sync(self, request: ArtifactFrame, context: RuntimeContext) -> "RuntimeRail[Block[ArtifactFrame]]":
        # bidi leg: each inbound frame names the artifact_id it wants; the parked GLB folds back as its framed rows.
        # The charge seats HERE rather than on `sync`, so an in-process caller reaching the ring directly is not
        # billed for a request no consumer made — a served slot is what `Resource.REQUEST` prices.
        return await _served(SYNC_ROUTE, self._composition, await self.sync(request.artifact_id))

    async def sync(self, artifact_id: bytes) -> "RuntimeRail[Block[ArtifactFrame]]":
        # ring first, durable tier second, typed absence fault last. A key past the ring horizon is NOT necessarily an
        # unknown artifact — a warm-restarted process and a fleet peer each hold no ring at all — so the read-through
        # answers an unchanged model from the store before a consumer is told to re-request a tessellation nobody
        # needs to re-run. Serve still derives no hash: the object is addressed by the exact `artifact_id` the frame
        # carries, through the daemon's own `spill_path`, so the address IS the identity and re-hashing here would
        # mint the second key mint this page exists without.
        match self._served.try_find(artifact_id):
            case Option(tag="some", some=artifact):
                return Ok(_frames(artifact_id, artifact.bytes))
            case _:
                return await self._read_through(artifact_id)

    async def _read_through(self, artifact_id: bytes) -> "RuntimeRail[Block[ArtifactFrame]]":
        # a refused or absent object answers the SAME unknown-artifact fault an unbound tier answers, because both
        # mean one thing to the consumer: re-request the tessellation. Surfacing the store's own transport fault
        # here would hand the C# rail a refusal it has no arm for and no retry that helps.
        match self._store:
            case Option(tag="some", some=lane):
                fetched = await lane.run_async(StoreOp.Get(spill_path(SpillKind.ARTIFACT, artifact_id)))
                return fetched.map(lambda outcome: _frames(artifact_id, bytes(outcome.source))).or_else_with(lambda _absent: _unknown(artifact_id))
            case _:
                return _unknown(artifact_id)

    def bench(self, request: TessellationRequest, context: RuntimeContext, *, rounds: int = 32, warmup: int = 4) -> "RuntimeRail[BenchmarkReceipt]":
        # macro-bench over the real tessellation entry — _tessellate whole: decode, daemon drive, receipt floor — the
        # same seam the C# rail pays; the canonical daemon stays warm across rounds, so the cache tier prices in.
        return bench_seam(
            bench_subject(EvidenceScope.MESH_SERVE, TESSELLATE_ROUTE),
            partial(self._tessellate, request, context),
            rounds=rounds,
            warmup=warmup,
            composition=self._composition,
        )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
