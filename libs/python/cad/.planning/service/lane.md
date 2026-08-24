# [PY_CAD_LANE]

`NATIVE_LANE` is the one crossing every OCCT fold makes into the isolated worker process, and `regime` the process-wide exchange configuration that fold runs behind. This owner admits one native call at a time, refuses a third caller with a measured retry window instead of queueing it behind two whole calls, converts a worker death into the rail, and declares every value the pickle seam carries in each direction.

`faults#ROWS` supplies `NATIVE_INIT`, `NATIVE_WORKER`, `LANE_SATURATED`, and `SOURCE_SHAPE`, and a `CadFault` minted inside the worker rides home as a VALUE because `msgspec.Struct` pickles by reference. `exchange/identity#PINS` rules which `Interface_Static` pins the artifact's byte-stability contract requires and why; this page applies that roster and re-argues none of it. `brep/operation#OPERATION` and `tessellation/mesh#MESH` run INSIDE the worker and never see the lane.

## [01]-[INDEX]

- [02]-[REGIME]: Cached controller-and-pin fold configuring one process's OCCT exchange session.
- [03]-[LANE]: One-slot cancellable process lane, its saturation admission, and every value the pickle seam carries.

## [02]-[REGIME]

- Owner: `regime` folds every exchange-controller start and every `Interface_Static` pin into one `CadRail[None]`, cached per process, and it is the only fence in the package that configures OCCT process state.
- Law: `@cache` keys the empty argument tuple, so the fold runs ONCE PER PROCESS — the worker holds its own cache, and a respawned worker re-pins before its first fold without any parent-side coordination.
- Law: a module-global `_configured = False` flag is the DELETED form, and its defect was residency rather than style: the ladder ran inside the `to_process` worker, so the parent's copy never flipped, every respawn silently re-ran a probe nothing read, and the one boolean any reader consulted lived in the process that never touched it.
- Law: three bare `RuntimeError` raises are the other half of that deleted form — a refusal that cannot name its leg, cannot carry its Connect code, and cannot cross the pickle seam as a value is not a refusal, and `NATIVE_INIT` states all three from one row.
- Law: every pin WRITES and then READS BACK, because OCCT accepts a set that never takes; a write-only probe certifies a unit the writer does not hold, which is exactly how a metre receipt silently becomes a millimetre one.
- Cases: `Controller` rows start the STEP and IGES exchange controllers; `Pin` rows carry a coordinate, the value it must hold, and the typed `Interface_Static` write-and-read pair for that value's kind, so the string pins and the integer pin fold through one arm instead of two statement ladders differing only by which member they call.
- Growth: a new process-wide OCCT setting is one `Pin` row, a new exchange controller one `Controller` row, and neither touches the fold.
- Boundary: readers, writers, meshers, and property folds compose an already-pinned process and never re-probe one; the roster's membership is `exchange/identity#PINS`'s ruling, applied here.

```python signature
from collections.abc import Callable
from functools import cache
from pathlib import Path
from typing import Final

from anyio import BrokenWorkerProcess, CapacityLimiter
from anyio.to_process import run_sync
from expression import Error, Ok
from expression.collections import Block
from expression.extra.result import traverse
from msgspec import Struct
from OCP.IGESControl import IGESControl_Controller
from OCP.Interface import Interface_Static
from OCP.STEPControl import STEPControl_Controller
from rasm.contracts.rasm.contracts.cad.operations_pb import ExecuteRequest
from rasm.contracts.rasm.contracts.cad.service_pb import TessellateRequest

from rasm.cad.brep.operation import execute as execute_brep
from rasm.cad.exchange.identity import SCHEMA, UNIT
from rasm.cad.faults import LANE_SATURATED, NATIVE_INIT, NATIVE_WORKER, SOURCE_SHAPE, CadRail
from rasm.cad.tessellation.mesh import tessellate as tessellate_cad

# --- [TYPES] ----------------------------------------------------------------------------

# one admitted reference projected onto the path its call owns: the digest identifies it, the path reaches it
type SourceRow = tuple[bytes, str]


# --- [MODELS] ---------------------------------------------------------------------------


class Controller(Struct, frozen=True):
    # each exchange controller registers its own reader and writer family with OCCT's process-wide session, and
    # neither family resolves until its controller has started, whatever unit the session later carries.
    coordinate: str
    start: Callable[[], bool]


class Pin[V](Struct, frozen=True):
    # `write` and `read` are the typed `Interface_Static` pair for THIS value's kind, so the value's type picks
    # its member pair instead of a per-kind statement arm re-spelling the same write-then-verify shape.
    coordinate: str
    wanted: V
    write: Callable[[str, V], bool]
    read: Callable[[str], V]


# --- [TABLES] ---------------------------------------------------------------------------

_CONTROLLERS: Final[Block[Controller]] = Block.of_seq((
    Controller(coordinate="step", start=STEPControl_Controller.Init_s),
    Controller(coordinate="iges", start=IGESControl_Controller.Init_s),
))

# membership and rationale are `exchange/identity#PINS`'s ruling; this table is where that ruling executes, and
# the wanted values IMPORT from that owner so the executed regime cannot drift from the declared one
_PINS: Final[Block[Pin[str] | Pin[int]]] = Block.of_seq((
    Pin(coordinate="xstep.cascade.unit", wanted=UNIT, write=Interface_Static.SetCVal_s, read=Interface_Static.CVal_s),
    Pin(coordinate="write.step.unit", wanted=UNIT, write=Interface_Static.SetCVal_s, read=Interface_Static.CVal_s),
    Pin(coordinate="write.step.schema", wanted=SCHEMA, write=Interface_Static.SetIVal_s, read=Interface_Static.IVal_s),
))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _started(controller: Controller, /) -> CadRail[str]:
    return Ok(controller.coordinate) if controller.start() else Error(NATIVE_INIT.at(f"occt.init.{controller.coordinate}"))


def _pinned[V](pin: Pin[V], /) -> CadRail[str]:
    # OCCT accepts a set that never takes, so the READ-BACK is the proof and the write alone certifies nothing
    return (
        Ok(pin.coordinate)
        if pin.write(pin.coordinate, pin.wanted) and pin.read(pin.coordinate) == pin.wanted
        else Error(NATIVE_INIT.at(f"occt.pin.{pin.coordinate}"))
    )


@cache
def regime() -> CadRail[None]:
    # `@cache` keys the empty argument tuple, so this runs once per PROCESS: the worker holds its own cache and
    # a respawned worker re-pins before its first fold, which a parent-side boolean could never observe.
    return traverse(_started, _CONTROLLERS).bind(lambda _ready: traverse(_pinned, _PINS)).map(lambda _held: None)
```

## [03]-[LANE]

- Owner: `native` is the package's ONE `to_process` call site, and the lane it holds is one slot deep because OCCT's mesher takes whole-process custody when its parallel switch is on.
- Law: `cancellable=True` is the verified spelling on `to_process.run_sync`; `abandon_on_cancel=` is `to_thread.run_sync`'s rename alone and raises `TypeError` here, so the two offload arms never share a keyword.
- Law: cancellation is NOT a refusal — `cancellable=True` terminates the worker and the cancelled exception re-raises past the narrow `BrokenWorkerProcess` arm untouched, so the scope that tripped keeps its structured-cancellation contract and nothing rails a cancellation into a value.
- Law: saturation is READ before the wait, off the limiter's own `statistics()` snapshot, and its window is the admitted `call_seconds` ceiling times the occupancy the caller queues behind — `LANE_SATURATED.windowed` therefore projects a measured worst case into the arm's own `RetryInfo` rather than a literal no measurement backs, and this is the one arm that makes `Recovery`'s third case reachable.
- Law: the read races by one — a caller admitted between the snapshot and the acquire still serializes on the limiter, so the snapshot is an admission bound and never a lock, and the cost of losing the race is one extra waiter rather than a second concurrent native fold.
- Law: `_LANE_DEPTH` admits exactly one waiter, so a caller arriving mid-fold queues and the caller after it is refused with a stated window instead of waiting behind two whole calls with nothing said about the delay.
- Law: every value crossing the pickle seam parses and imports on BOTH sides of it, kernels cross by QUALIFIED NAME through `_regimed`, and a `CadFault` crosses home as a VALUE because `msgspec.Struct` pickles by reference — a custom exception transporting an inner fault across that seam is the rejected inversion.
- Law: generated messages and artifact bodies never cross; the request crosses as BINARY, the receipt returns as binary, and the output leaves through the call-owned path rather than through the seam.
- Law: `OneSource.of` resolves the single source path on the SERVE floor, where `references` already ran and body admission already passed, so the worker receives a path instead of a roster and an unguarded index into it — the double lookup `_path_rows(sources)[references(request)[0].sha256]` performed on the unvalidated side is the deleted form.
- Law: an unpicklable argument raises out of `run_sync` and PROPAGATES as a defect, because a value this owner cannot marshal is a construction fault of the fence that built it, never a caller's refusal.
- Cases: `Sources` is `OneSource` for the rpc admitting exactly one reference and `SourceRows` for the rpc admitting any admitted count including none; the arity discriminant is the source shape itself, so no kernel re-guards a count the serve floor already settled.
- Output: `BrepMarshal` and `MeshMarshal` are each native leg's own non-artifact half of its reply, and they are instantiations of the spine's `E` parameter rather than branches inside it — `provider#PROVIDER` never names either.
- Growth: a new native leg is one kernel and one marshal struct; a new marshalled field is one struct member.
- Boundary: this page spells no outbound raise — the inbound `BrokenWorkerProcess` is the seam's only exception and one arm converts it, and the package's single outbound raise is `provider#PROVIDER`'s `ConnectError`.

```python signature
# --- [CONSTANTS] ------------------------------------------------------------------------

# `BRepMesh_IncrementalMesh` takes its parallel switch as a per-call boolean with no thread count, so enabling it
# is only safe under whole-lane custody; one slot is that custody, and one waiter absorbs a mid-fold arrival.
NATIVE_LANE: Final[CapacityLimiter] = CapacityLimiter(1)
_LANE_DEPTH: Final[int] = 1


# --- [MODELS] ---------------------------------------------------------------------------


class OneSource(Struct, frozen=True, gc=False):
    path: str

    @staticmethod
    def of(rows: tuple[SourceRow, ...], /) -> CadRail["OneSource"]:
        # resolved on the SERVE floor, where `references` already ran and body admission already passed; the
        # worker receives a path, and the sequence pattern makes a miscounted roster unrepresentable downstream.
        match rows:
            case ((_digest, path),):
                return Ok(OneSource(path=path))
            case _:
                return Error(SOURCE_SHAPE.at(f"cad.sources.one:{len(rows)}"))


class SourceRows(Struct, frozen=True):
    rows: tuple[SourceRow, ...]

    @staticmethod
    def of(rows: tuple[SourceRow, ...], /) -> CadRail["SourceRows"]:
        # TOTAL by construction: a primitive operation embeds no reference at all, so an empty roster admits and
        # `spool#SPOOL`'s budget gate stays the only count law either arm answers to.
        return Ok(SourceRows(rows=rows))

    def paths(self) -> dict[bytes, Path]:
        return {digest: Path(path) for digest, path in self.rows}


type Sources = OneSource | SourceRows


class NativeCall[C: Sources](Struct, frozen=True, kw_only=True):
    # everything the worker receives: request BINARY, the resolved source shape, the call-owned output path, and
    # one admitted output ceiling BOTH native legs enforce at their own write seam.
    payload: bytes
    sources: C
    target: str
    ceiling: int


class BrepMarshal(Struct, frozen=True, gc=False):
    receipt: bytes
    protocol: int


class MeshMarshal(Struct, frozen=True, gc=False):
    element_count: int
    triangle_count: int
    kernel: bytes


# --- [OPERATIONS] -----------------------------------------------------------------------


def brep_kernel(call: NativeCall[SourceRows], /) -> CadRail[BrepMarshal]:
    # runs INSIDE the worker: the request decodes here, the sealed STEP writes to `target` under `ceiling`, and
    # its receipt leaves as binary because a generated message crosses the pickle seam in neither direction.
    return execute_brep(
        ExecuteRequest.from_binary(call.payload), call.sources.paths(), Path(call.target), call.ceiling
    ).map(lambda evidence: BrepMarshal(receipt=evidence.receipt.to_binary(), protocol=int(evidence.protocol)))


def mesh_kernel(call: NativeCall[OneSource], /) -> CadRail[MeshMarshal]:
    return tessellate_cad(
        TessellateRequest.from_binary(call.payload), Path(call.sources.path), Path(call.target), call.ceiling
    ).map(
        lambda evidence: MeshMarshal(
            element_count=evidence.element_count,
            triangle_count=evidence.triangle_count,
            kernel=evidence.kernel.to_binary(),
        )
    )


def _regimed[C: Sources, E](kernel: Callable[[NativeCall[C]], CadRail[E]], call: NativeCall[C], /) -> CadRail[E]:
    # `_regimed` crosses by QUALIFIED NAME and `kernel` and `call` ride behind it as values, so this module and
    # every kernel's module parse and import on the worker floor with no runtime-only import chain.
    return regime().bind(lambda _pinned: kernel(call))


async def native[C: Sources, E](
    kernel: Callable[[NativeCall[C]], CadRail[E]],
    call: NativeCall[C],
    /,
    *,
    saturation: float,
) -> CadRail[E]:
    held = NATIVE_LANE.statistics()
    queued = held.borrowed_tokens + held.tasks_waiting
    if queued > _LANE_DEPTH:
        # `saturation` is the admitted `call_seconds` ceiling and NOT a forwarded timeout: it states the window a
        # queued caller actually faces, so the projected `RetryInfo` delay is measured off live occupancy.
        return Error(LANE_SATURATED.windowed(saturation * queued).at(f"cad.native.lane:{queued}"))
    try:
        return await run_sync(_regimed, kernel, call, cancellable=True, limiter=NATIVE_LANE)
    except BrokenWorkerProcess:
        # pickle seam's ONE inbound raise: the worker died without returning, so no rail crossed and the refusal
        # mints on this floor. Cancellation never reaches this arm, and an unpicklable value raises straight past.
        return Error(NATIVE_WORKER.at("cad.native.worker"))
```

## [04]-[RESEARCH]

- [OUTPUT_CEILING]-[OPEN]: do `brep/operation` and `tessellation/emission` both admit `NativeCall.ceiling` at their write seam; verify at each owner's fence.
