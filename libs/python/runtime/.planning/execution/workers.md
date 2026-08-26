# [PY_RUNTIME_WORKERS]

One closed `WorkerKind` family carries every worker the branch runs — thread, subinterpreter, process, device-pinned process, daemon, remote, and sandboxed guest — each kind a `KIND_POLICY` row binding its fidelity obligation and, where something actuates it, its restart class. `Kernel` is the single kernel-crossing owner: every callable that leaves the event loop crosses as one frozen value whose `KernelTrait` row derives isolation, worker-death retry, deadline enforcement, and shipping, so no consumer re-derives a modality, pairs a retry class by convention, or hand-builds a name-crossing gate. `WorkerPool` owns the warm reusable process pools and the fleet-host remote arm with spawn/warm/roll/drain/retire lifecycle, and `Supervisor` is the restart actuator — psutil probes answering typed ceiling evidence, a windowed restart budget, and the health projection the serve owner advertises.

Composition is settled: the thread and subinterpreter crossing arms stay `execution/lanes#LANE` owners the trait table projects onto, while both process arms ride `WorkerPool` under the `WORKER_BAND` this page mints; worker-death backoff rides `reliability/resilience#RESILIENCE` rows (`OCCT` for the anyio subinterpreter arm, `WORKER` for the pool executors, `SPAWN` for a daemon child); every pool fault converts through the `reliability/faults#FAULT` lift, whose pool-death row lands executor deaths on the `resource` case; pool and supervision evidence writes its own line through `structlog` at the site. `cloudpickle` ships a closure or local callable by value across the pickle boundaries stdlib pickle refuses, `tblib` re-raises a worker fault parent-side with its worker frames, `loky` owns the crash-respawning warm process pool every cooperative `PROCESS` offload rides, `pebble` owns the terminal wall-clock kill, PEP 734 `anyio.to_interpreter` owns the subinterpreter substrate with zero package cost, `asyncssh` carries the sealed kernel to a fleet host over the one `transport/roots#RESOURCE` `RemoteEndpoint` channel under the `SSH` restart row, `wasmtime` runs a sandboxed guest module in-process under a shared epoch pacer, and the device arms pin an accelerator through loky's `env=` and pebble's initializer at spawn — placement rides the existing pools, zero new package. Worker floors are parented emitters: every process arm boots the parent-captured telemetry install post-spawn through one `_worker_boot` initializer, the kernel span opens under the carried W3C parent, the profiler attaches where the cycles burn, and the two-read `Cost` bracket prices every crossing to the tenant that ran it.

## [01]-[INDEX]

- [02]-[CROSSING]: `Kernel` owns every isolation crossing — the `WorkerKind` family and its policy rows, the `KernelTrait` isolation classifier, shipping, wire, deadline, and enforcement as fields, the host-side `admitted` gate, the shared-memory span channel, the parented-emitter worker gate (kernel span, profiler phase, and cost bracket over the stitch-and-resolve pair), the remote-floor entry, the guest sandbox arm with its epoch pacer, and the tblib fidelity latch.
- [03]-[POOL]: the warm reusable `WorkerPool` capsule — loky, pebble, and per-device arms under one lifecycle vocabulary, the `WorkerBoot` install boundary and its exit-owned flush law, band-bounded settle, in-band worker-death retry, and the asyncssh remote arm crossing the sealed kernel to a fleet host.
- [04]-[LEASE]: one fenced claim, heartbeat, settlement, and drain algebra over an admitted backend generation.
- [05]-[SUPERVISION]: kind-scoped psutil verdicts as typed `Weighed` evidence, restart budget and escalation, serving health, and bundle verdict projection.

## [02]-[CROSSING]

- Owner: `WorkerKind` closes the worker family; a kind's standing obligations are its `KIND_POLICY` row — the fidelity latch and the restart class an actuator re-drives it under, empty on the kinds nothing actuates — so a new worker kind is one member and one row, never a sibling vocabulary. `Kernel` unifies the four crossing spellings the branch grew — a name-string gate, a by-reference module kernel, a page-local `_run_kernel`, a per-family `_dispatch` — into one frozen value: `Kernel.of` classifies the target once, and every downstream hop reads isolation, retry, deadline, enforcement, and wire off the value.
- Cases: `KernelTrait` answers the one isolation question per kernel family — `INLINE` runs on the loop (sub-quantum pure body), `PURE` wants its own GIL in-process (subinterpreter), `RELEASING` releases the GIL or blocks on a syscall (thread), `HOSTILE` holds process-global native state or a GIL-hostile extension (process), `SANDBOXED` runs a foreign guest module inside the in-process wasmtime sandbox (the wasm-bytes target shape answers it, never a declaration) — and `TRAIT_ROW` projects each trait onto its `WorkerKind` and default worker-death `RetryClass`. A consumer declares the trait per kernel family as domain knowledge; isolation, band, retry, and crossing mechanics are this owner's.
- Entry: `Kernel.of(target, trait, *, deadline, enforcement, wire, idempotent, retry)` is the one constructor, polymorphic on the target shape — the trait's boundary classifies first, so a loop or thread kernel ships `LIVE` with its callable carried whole at zero serialization, a pickle-boundary kernel classifies by picklability (a module-qualified callable ships `REFERENCE`; a closure, `<locals>` callable, or bound method ships `VALUE` as cloudpickle bytes, since a by-name walk loses the instance a `__self__` carries), a `(module, name)` pair is the native-gated form: the loop floor names a worker-floor kernel it must never import, `REFERENCE` by construction, and a wasm module crosses as its own bytes — `GUEST` by construction, the digest label its log and fault subject. `deadline` is the per-offload budget the lane folds against its own — the tighter bound wins — and `enforcement` selects the deadline arm: `COOPERATIVE` cancels the awaiting scope and leaves the worker to the pool's reaper, `TERMINAL` routes the hop through the pebble pool so the deadline kills the worker mid-kernel and reclaims the slot, the only bound a hung native call obeys.
- Auto: `fidelity()` latches `tblib.pickling_support.install()` once per interpreter — every exception crossing a pickle boundary thereafter carries its worker-side frames, so the faults lift classifies the true cause instead of a flattened marker; the latch runs in every pool initializer whose `KIND_POLICY` row obliges it and inside `shipped`, so a spawned interpreter re-latches cold. This latch runs default-off locals capture, and the explicit `Traceback` carrier stays out — the pickle path is the crossing's whole tblib surface, log lines carrying facts, never frames.
- Law: a worker-death retry re-runs the kernel whole, so the retry row binds only under the kernel's `idempotent` declaration — content-keyed inputs, run-scoped outputs, no external state — and `Kernel.of` drops EVERY retry binding for a kernel declaring `idempotent=False`, the caller's explicit class exactly as the trait default, because a body that cannot re-run overrules a call site that asked for a re-run; the declaration gates the two live dispatch sites — `WorkerPool.submit` for the pooled kinds and the `execution/lanes#LANE` offload hop for the anyio arms — each reading `kernel.retry`, never a convention the call site remembers.
- Law: `Wire` closes the payload-crossing axis — `PICKLE` copies arguments across the boundary, `SHARED_MEMORY` exports every top-level ndarray argument once into a named `multiprocessing.shared_memory` block and crosses it as a `ShmSpan` the worker re-views through `numpy.frombuffer` — so a heavy-buffer kernel upgrades its crossing by one field with the call site untouched. Block custody stays loop-side and BRACKETS the whole offload hop: `execution/lanes#LANE` is that bracket's one owner — `exported` copies and names before the hop, `released` closes and unlinks after it settles — so `WorkerPool.submit` receives arguments already spanned and re-exports nothing, and a consumer submitting to an arm directly owns the same bracket around its own call. The worker view is ingress-only: a kernel consumes it inside its body and returns owned material, because the worker handle closes when the kernel returns. A buffer wrapped inside a struct stays `PICKLE`; only a bare ndarray argument rides the span channel. Named blocks are the chosen out-of-band channel because cloudpickle's protocol-5 `buffer_callback` collects buffers the executor transports have no side channel to carry, where a named block crosses at zero payload bytes.
- Law: a native-gated worker module splits at the parse floor — the vocabulary module parses and imports on both interpreter floors while the worker-body module holds its eager native providers and loads only worker-side — and `REFERENCE` shipping resolves the kernel by qualified name through the one `shipped` gate; `covered(module, names)` is the worker-floor witness that same module runs at its own import, proving every dispatchable name resolves through the identical walk so a misspelled roster fails at worker import, never mid-offload. Crossing law homes at `libs/python/.planning/RULINGS.md`; `artifacts` scene rendering is the standing proof instance. `cloudpickle.register_pickle_by_value` stays out: a worker floor that must import its module from disk is the stronger contract than shipping a module by value into an interpreter that drifts from it.
- Law: `traced_kernel` is the parented-emitter gate every crossing resolves through — the propagator extracts the carried W3C parent and the observe-owned `attach` brackets it, the `worker.<name>` span opens under it so worker-interior evidence joins the one trace, the profiler `phase` window tags the flame by kernel subject and shipping form, and the two-read `Cost` bracket records the kernel's own process spend onto the `rasm.cost.<measure>` rows under the attached context, the promoted `rasm.tenant` entry pricing the kernel to the tenant that ran it; an uninstalled floor resolves no-op providers and a null profiler window, so the gate never conditions on install state and costs two process reads. The cost rows key on `Kernel.subject`, not `name`: a code kernel's qualname is bounded by the code base while a GUEST digest is minted from caller-controlled bytes, and only the tenant axis carries a value budget, so a per-digest attribute would accrete a series axis nothing bounds — the digest stays the span, line, and fault subject, where it costs no series.
- Law: `remote_floor` is the fleet mirror of `shipped` — the far interpreter's module entry reads one sealed blob on stdin, resolves it through the same `sealed_kernel` gate, and writes one pickled `("value", T) | ("raise", BaseException)` verdict on stdout — so every `Shipping` form is total across the SSH channel (the seal cloudpickles the whole `Kernel`, a `LIVE` callable crossing by value, `REFERENCE` re-importing from the remote install, the worker-floor contract at fleet scale) and a kernel raise crosses home frame-whole under the latch `shipped` re-arms.
- Law: a typed band token crosses as DATA and never as a pickled exception. `Exception.__reduce__` hands `(cls, self.args, self.__dict__)`, and a kwarg-only `@tagged_union` has EMPTY `args` with its case in `__dict__`, so reconstruction re-enters the union's one-case guard and dies on EVERY arm — `StopIteration` under stdlib pickle and cloudpickle, `TypeError: One and only one case can be specified` once the `tblib` latch is armed, which is every pooled and remote crossing. Both directions break identically: a token RAISED worker-side and a result RETURNING `BoundaryFault.domain` carrying one. `shipped` is the one floor gate every arm resolves through, so `crossed` lowers both there onto `CrossedFault` — positional args and a plain `__dict__`, the inverse shape of the guard that refuses — and `homed`/`_homing` re-mint the producer's own case parent-side, ahead of the fence that seats it. A producer keeps raising its typed case and edits nothing.
- Law: the re-mint is EXACT or it is absent. `convert` restores each case's DECLARED type off the union's own field, so a `tuple` slot arrives a tuple rather than the list its encoding decodes to; a payload no encoder lowers crosses as its own render and a class the parent cannot import keeps the carrier, which is faithfully `Tagged` and publishes the producer's case and evidence through `facts()` regardless. `mesh/daemon#DAEMON`'s `raise RuntimeError(str(token))` is that render hand-rolled at one call site, and it retires onto this boundary.
- Law: `admitted` is the host-side crossing gate — every offload passes it before an arm sees the kernel, and `GUEST` is the one form that does work there, `wasmtime.Module.validate` refusing malformed wasm and a non-bytes payload parent-side onto a typed `config` fault at parse cost with no compile, no store, and no instance. Caller-controlled bytes are the reason: a guest defect reaching the worker floor surfaces one hop and one epoch budget later as an instantiation trap the catch-all `boundary` case cannot tell from a genuine guest trap. Every other shipping form yields `Nothing` at one branch, so the gate stays one call on the offload path and never a wasm arm inside the lane's isolation table.
- Law: `_guest` is `GUEST` shipping's worker-floor arm — zero-import instantiation (no WASI, no ambient capability), a fresh `Store` per call so guest state never leaks across kernels, `GUEST_MEMORY` bounding linear memory, and request/reply crossing as bytes over the `GUEST_ABI` exports, which is also why the arm's result type IS bytes rather than the crossing's free `T`; the module compiles once per digest per interpreter, so the per-call cost is instantiation alone. Engine and module resolve as a PAIR under one gate because `SANDBOXED` kernels ride the thread arm: two first guests land concurrently as the ordinary case, a memo that re-enters its body on a concurrent miss lets the loser's engine escape with its own pacer thread, and a store on one engine instantiating a module compiled on another refuses. One `WasmtimeError` fence spans compile, instantiate, and call, so no arm of the guest crossing escapes `shipped` outside the elapsed-budget discrimination.
- Law: the guest deadline is the engine's epoch — one daemon pacer heartbeats the engine-global epoch every `EPOCH_TICK` while each store carries its own relative tick budget, so concurrent guests never kill each other and a guest dies mid-kernel at wall clock IN-PROCESS, the enforcement no thread or interpreter arm owns. `WasmtimeError` exposes no addressable trap code, so the arm discriminates by elapsed budget: an epoch kill re-raises `TimeoutError` onto the faults `deadline` row, and a genuine trap crosses whole into the catch-all `boundary` case with its trap message.
- Law: every refusal across the three regions resolves ONE `reliability/faults#FAULT` `RAISES` anchor under `RuntimeLeg.WORKERS` and derives its subject from that leg — the kernel name, the pool phase, the lease verb, and the daemon charge all ride NAMED row slots. The three deadline re-stamps compose the owner's `expired` fold, so the budget-unknown floor is DECLARED once rather than spelled `default_value(0.0)` at each fence. Two fences keep a catch-all and state why: a crossing re-raises the caller's OWN kernel exception through `tblib`, and a settlement's evidence projection is caller-supplied — neither raise surface a runtime can roster, and a leak at either strands a line or a live lease.
- Growth: a new band family crosses with ZERO edits here — `CrossedFault` keys on the union grammar, never on a roster of families; a new worker kind is one `WorkerKind` member with one `KIND_POLICY` row; a new isolation answer is one `KernelTrait` member with one `TRAIT_ROW` row and every call site untouched; a new shipping form is one `Shipping` member with one `shipped` arm and, where its payload arrives unjudged, one `admitted` arm; a new enforcement arm is one `Enforcement` member with one offload projection row; a new payload crossing is one `Wire` member with one `exported` arm; a new cost measure is one `Cost` field at the observe owner with one `INSTRUMENTS` row at the metrics owner, reaching this bracket through `measures` with zero gate edits.
- Boundary: trait declaration stays consumer domain knowledge — this owner never inspects a callable for GIL behavior; picklability is the one property `Kernel.of` classifies itself. Thread and subinterpreter crossing arms and the offload hop stay `execution/lanes#LANE`'s; this page mints the vocabulary the hop consumes, the process bands, and the process pools. `execution/admission#CONTEXT` admits the `isolation` axis upstream and refuses an unbound crossing there, so `KernelTrait` selects the worker kind INSIDE a value the profile already serves — `INLINE` under `in-proc`, `PURE` and `RELEASING` under `thread`, `HOSTILE` under `process`, `SANDBOXED` under `wasm`, the `WorkerKind.REMOTE` fleet arm under `remote` — and a kernel reaching a crossing the profile never admitted is unrepresentable, never a runtime downgrade this owner absorbs.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
import hashlib
import math
import sys
import threading
import dataclasses
import time
from collections.abc import Awaitable, Callable, Iterable
from enum import StrEnum
from functools import cache, lru_cache, reduce
from importlib import import_module
from multiprocessing.shared_memory import SharedMemory
from typing import Final, assert_never

import cloudpickle
import msgspec
from expression import Error, Nothing, Option, Result, Some
from expression.collections import Map
from msgspec import Struct, convert, to_builtins
from opentelemetry import propagate, trace
from tblib import pickling_support

from rasm.runtime.faults import SCOPES, WORKERS_COVERED, WORKERS_GUEST, BoundaryFault, RuntimeResult, Scope, Tagged, boundary, scoped
from rasm.runtime.metrics import Metrics
from rasm.runtime.observe import Cost, Facts, attach, logger
from rasm.runtime.profiles import Profiles
from rasm.runtime.resilience import RetryClass

lazy import numpy
lazy import wasmtime

_TRACER: Final[trace.Tracer] = scoped(trace.get_tracer, SCOPES[Scope.WORKERS])

# --- [TYPES] ----------------------------------------------------------------------------


class WorkerKind(StrEnum):
    THREAD = "thread"
    INTERPRETER = "interpreter"
    PROCESS = "process"
    GPU = "gpu"
    WASM = "wasm"
    DAEMON = "daemon"
    REMOTE = "remote"


class KernelTrait(StrEnum):
    INLINE = "inline"
    PURE = "pure"
    RELEASING = "releasing"
    HOSTILE = "hostile"
    SANDBOXED = "sandboxed"


class Shipping(StrEnum):
    LIVE = "live"
    REFERENCE = "reference"
    VALUE = "value"
    GUEST = "guest"


class Wire(StrEnum):
    PICKLE = "pickle"
    SHARED_MEMORY = "shared-memory"


class Enforcement(StrEnum):
    COOPERATIVE = "cooperative"
    TERMINAL = "terminal"


type KernelTarget[U] = Callable[..., U] | tuple[str, str] | bytes | bytearray

# --- [CONSTANTS] ------------------------------------------------------------------------

GUEST_ABI: Final[tuple[str, str, str]] = ("memory", "alloc", "run")
GUEST_MEMORY: Final[int] = 1 << 28
GUEST_MODULES: Final[int] = 8
EPOCH_TICK: Final[float] = 0.1
UNBOUNDED_TICKS: Final[int] = 1 << 62
VERDICT_FRAME: Final[int] = 1 << 30

# --- [MODELS] ---------------------------------------------------------------------------


class TraitRow(Struct, frozen=True):
    kind: Option[WorkerKind]
    retry: Option[RetryClass]


class KindPolicy(Struct, frozen=True, gc=False):
    fidelity: bool
    restart: Option[RetryClass]


class ShmSpan(Struct, frozen=True, gc=False):
    name: str
    dtype: str
    shape: tuple[int, ...]


class Kernel[T](Struct, frozen=True):
    trait: KernelTrait
    name: str
    module: str
    shipping: Shipping
    payload: bytes = b""
    live: Callable[..., T] | None = None
    deadline: Option[float] = Nothing
    enforcement: Enforcement = Enforcement.COOPERATIVE
    wire: Wire = Wire.PICKLE
    idempotent: bool = True
    retry: Option[RetryClass] = Nothing

    @staticmethod
    def of[U](
        target: KernelTarget[U],
        trait: KernelTrait = KernelTrait.PURE,
        *,
        deadline: Option[float] = Nothing,
        enforcement: Enforcement = Enforcement.COOPERATIVE,
        wire: Wire = Wire.PICKLE,
        idempotent: bool = True,
        retry: Option[RetryClass] = Nothing,
    ) -> "Kernel[U]":
        trait = KernelTrait.SANDBOXED if isinstance(target, bytes | bytearray) else trait
        if trait is KernelTrait.SANDBOXED and not isinstance(target, bytes | bytearray):
            raise TypeError(f"workers.kernel.{getattr(target, '__qualname__', target)}: sandboxed-trait-requires-wasm-bytes")
        row = TRAIT_ROW[trait]
        boundary = row.kind.map(lambda kind: KIND_POLICY[kind].fidelity).default_value(False)
        match target:
            case bytes() | bytearray() as guest:
                name, module = hashlib.sha256(guest).hexdigest()[:16], ""
                shipping, payload, live = Shipping.GUEST, bytes(guest), None
            case (str() as module, str() as name):
                shipping, payload, live = Shipping.REFERENCE, b"", None
            case fn if not boundary:
                name = getattr(fn, "__qualname__", "<lambda>")
                module = getattr(fn, "__module__", "")
                shipping, payload, live = Shipping.LIVE, b"", fn
            case fn:
                name = getattr(fn, "__qualname__", "<lambda>")
                module = getattr(fn, "__module__", "")
                importable = "<lambda>" not in name and "<locals>" not in name and getattr(fn, "__self__", None) is None
                shipping, payload, live = (Shipping.REFERENCE, b"", None) if importable else (Shipping.VALUE, cloudpickle.dumps(fn), None)
        return Kernel(
            trait=trait,
            name=name,
            module=module,
            shipping=shipping,
            payload=payload,
            live=live,
            deadline=deadline,
            enforcement=enforcement,
            wire=wire,
            idempotent=idempotent,
            retry=retry.or_else(row.retry) if idempotent else Nothing,
        )

    @property
    def row(self) -> TraitRow:
        return TRAIT_ROW[self.trait]

    @property
    def subject(self) -> str:
        return self.shipping.value if self.shipping is Shipping.GUEST else self.name


# --- [OPERATIONS] -----------------------------------------------------------------------


@cache
def fidelity() -> bool:
    pickling_support.install()
    return True


def admitted[T](kernel: Kernel[T]) -> Option[BoundaryFault]:
    if kernel.shipping is not Shipping.GUEST:
        return Nothing
    match boundary(WORKERS_GUEST, lambda: wasmtime.Module.validate(wasmtime.Engine(), kernel.payload), catch=(wasmtime.WasmtimeError, TypeError)):
        case Result(tag="error"):
            return Some(WORKERS_GUEST.raised(kernel.name))
        case _:
            return Nothing


class CrossedFault(Exception):
    def __init__(self, module: str, name: str, case_: str, payload: object) -> None:
        super().__init__(module, name, case_, payload)
        self.tag = case_
        setattr(self, case_, payload)

    @staticmethod
    def of(token: Tagged) -> "CrossedFault":
        kind, carried = type(token), getattr(token, token.tag)
        try:
            lowered = to_builtins(carried)
        except (TypeError, NotImplementedError):
            lowered = repr(carried)
        return CrossedFault(kind.__module__, kind.__qualname__, token.tag, lowered)

    def homed(self) -> Option[BaseException]:
        module, name, active, payload = self.args
        try:
            kind = reduce(getattr, name.split("."), import_module(module))
            slot = next(field for field in dataclasses.fields(kind) if field.name == active)
            return Some(kind(**{active: convert(payload, type=slot.type)}))
        except (ImportError, AttributeError, StopIteration, msgspec.ValidationError):
            return Nothing

    def __str__(self) -> str:
        return f"{self.args[1]}.{self.tag}:{self.args[3]}"


def crossed[T](value: T) -> T:
    match value:
        case Result(tag="error", error=BoundaryFault(tag="domain", domain=(subject, Tagged() as token))):
            return Error(BoundaryFault(domain=(subject, CrossedFault.of(token))))
        case _:
            return value


def homed[T](value: T) -> T:
    match value:
        case Result(tag="error", error=BoundaryFault(tag="domain", domain=(subject, CrossedFault() as carrier))):
            return Error(BoundaryFault(domain=(subject, carrier.homed().default_value(carrier))))
        case _:
            return value


async def _homing[T](run: Awaitable[T]) -> T:
    try:
        return homed(await run)
    except CrossedFault as carried:
        raise carried.homed().default_value(carried) from None


def shipped[T](kernel: Kernel[T], *args: object) -> T:
    fidelity()
    match kernel.shipping:
        case Shipping.LIVE:
            fn = kernel.live if kernel.live is not None else reduce(getattr, kernel.name.split("."), import_module(kernel.module))
        case Shipping.REFERENCE:
            fn = reduce(getattr, kernel.name.split("."), import_module(kernel.module))
        case Shipping.VALUE:
            fn = cloudpickle.loads(kernel.payload)
        case Shipping.GUEST:
            fn = _guest(kernel)
        case _ as unreachable:
            assert_never(unreachable)
    views: list[object] = []
    handles: list[SharedMemory] = []
    try:
        for value in args:
            view, handle = _attached(value)
            views.append(view)
            if handle is not None:
                handles.append(handle)
        try:
            return crossed(_detached(fn(*views), views) if handles else fn(*views))
        except BaseException as raised:
            if not isinstance(raised, Tagged):
                raise
            raise CrossedFault.of(raised) from None
    finally:
        views.clear()
        for handle in handles:
            handle.close()


def traced_kernel[T](carrier: dict[str, str], kernel: Kernel[T], *args: object) -> T:
    with attach(propagate.extract(carrier)):
        span = _TRACER.start_span(f"worker.{kernel.name}")
        with trace.use_span(span, end_on_exit=True), Profiles.phase({"kernel": kernel.name, "shipping": kernel.shipping.value}):
            before = Cost.own()
            try:
                return shipped(kernel, *args)
            finally:
                Cost.spent(Cost.own(), before).map(lambda spend: Metrics.record(spend.measures(), domain="cost", kind=kernel.subject))


def sealed_kernel[T](blob: bytes) -> T:
    boot, carrier, kernel, args = cloudpickle.loads(blob)
    if boot is not None:
        _worker_boot(boot)
    return traced_kernel(carrier, kernel, *args)


def remote_floor() -> int:
    channel, sys.stdout = sys.stdout, sys.stderr
    try:
        verdict: tuple[str, object] = ("value", sealed_kernel(sys.stdin.buffer.read()))
    except BaseException as raised:
        verdict = ("raise", raised)
    payload = cloudpickle.dumps(verdict)
    channel.buffer.write(len(payload).to_bytes(8, "big") + payload)
    channel.buffer.flush()
    return 0


def covered(module: str, names: Iterable[str]) -> RuntimeResult[int]:
    return boundary(
        WORKERS_COVERED, lambda: sum(1 for name in names if reduce(getattr, name.split("."), import_module(module)) is not None),
        catch=(ImportError, AttributeError),
    )


def exported(wire: Wire, args: tuple[object, ...]) -> tuple[tuple[object, ...], tuple[SharedMemory, ...]]:
    if wire is Wire.PICKLE:
        return args, ()
    crossed: list[object] = []
    blocks: list[SharedMemory] = []
    try:
        for value in args:
            view, block = _spanned(value)
            crossed.append(view)
            if block is not None:
                blocks.append(block)
    except BaseException:
        released(tuple(blocks))
        raise
    return tuple(crossed), tuple(blocks)


def released(blocks: tuple[SharedMemory, ...]) -> None:
    failures: list[Exception] = []
    for block in blocks:
        for step in (block.close, block.unlink):
            try:
                step()
            except (OSError, BufferError) as refused:
                failures.append(refused)
    if failures:
        raise ExceptionGroup("workers.released", failures)


def _spanned(value: object) -> tuple[object, SharedMemory | None]:
    match value:
        case numpy.ndarray() as array if array.nbytes and not array.dtype.hasobject:
            block = SharedMemory(create=True, size=array.nbytes)
            try:
                numpy.frombuffer(block.buf, dtype=array.dtype)[:] = array.reshape(-1)
            except BaseException:
                block.close()
                block.unlink()
                raise
            return ShmSpan(name=block.name, dtype=str(array.dtype), shape=tuple(array.shape)), block
        case passthrough:
            return passthrough, None


def _attached(value: object) -> tuple[object, SharedMemory | None]:
    match value:
        case ShmSpan(name=name, dtype=dtype, shape=shape):
            block = SharedMemory(name=name, track=False)
            return numpy.frombuffer(block.buf, dtype=dtype).reshape(shape), block
        case passthrough:
            return passthrough, None


def _detached(result: object, views: list[object]) -> object:
    match result:
        case numpy.ndarray() as array if any(isinstance(view, numpy.ndarray) and numpy.may_share_memory(array, view) for view in views):
            return array.copy()
        case passthrough:
            return passthrough


# --- [GUEST_SANDBOX]

_GUEST_GATE: Final[threading.Lock] = threading.Lock()


def _paced(engine: "wasmtime.Engine") -> None:
    while True:
        time.sleep(EPOCH_TICK)
        engine.increment_epoch()


@cache
def _guest_engine() -> "wasmtime.Engine":
    config = wasmtime.Config()
    config.epoch_interruption = True
    engine = wasmtime.Engine(config)
    threading.Thread(target=_paced, args=(engine,), daemon=True, name="rasm-guest-pacer").start()
    return engine


@lru_cache(maxsize=GUEST_MODULES)
def _guest_module(payload: bytes) -> "wasmtime.Module":
    return wasmtime.Module(_guest_engine(), payload)


def _guest[T](kernel: Kernel[T]) -> Callable[..., bytes]:
    def run(request: bytes = b"") -> bytes:
        started = time.monotonic()
        try:
            with _GUEST_GATE:
                engine, module = _guest_engine(), _guest_module(kernel.payload)
            store = wasmtime.Store(engine)
            store.set_limits(memory_size=GUEST_MEMORY)
            store.set_epoch_deadline(kernel.deadline.map(lambda budget: max(1, math.ceil(budget / EPOCH_TICK))).default_value(UNBOUNDED_TICKS))
            exports = wasmtime.Instance(store, module, []).exports(store)
            memory, alloc, entry = (exports[name] for name in GUEST_ABI)
            pointer = alloc(store, len(request))
            memory.write(store, request, pointer)
            packed = entry(store, pointer, len(request))
        except wasmtime.WasmtimeError as trapped:
            if kernel.deadline.map(lambda budget: time.monotonic() - started >= budget).default_value(False):
                raise TimeoutError(f"guest.{kernel.name}:epoch-kill") from trapped
            raise
        head, span = packed >> 32, packed & 0xFFFFFFFF
        return bytes(memory.read(store, head, head + span))

    return run


# --- [TABLES] ---------------------------------------------------------------------------

TRAIT_ROW: Final[Map[KernelTrait, TraitRow]] = Map.of_seq([
    (KernelTrait.INLINE, TraitRow(kind=Nothing, retry=Nothing)),
    (KernelTrait.PURE, TraitRow(kind=Some(WorkerKind.INTERPRETER), retry=Some(RetryClass.OCCT))),
    (KernelTrait.RELEASING, TraitRow(kind=Some(WorkerKind.THREAD), retry=Nothing)),
    (KernelTrait.HOSTILE, TraitRow(kind=Some(WorkerKind.PROCESS), retry=Some(RetryClass.WORKER))),
    (KernelTrait.SANDBOXED, TraitRow(kind=Some(WorkerKind.WASM), retry=Nothing)),
])

KIND_POLICY: Final[Map[WorkerKind, KindPolicy]] = Map.of_seq([
    (WorkerKind.THREAD, KindPolicy(fidelity=False, restart=Nothing)),
    (WorkerKind.INTERPRETER, KindPolicy(fidelity=True, restart=Nothing)),
    (WorkerKind.PROCESS, KindPolicy(fidelity=True, restart=Some(RetryClass.WORKER))),
    (WorkerKind.GPU, KindPolicy(fidelity=True, restart=Some(RetryClass.WORKER))),
    (WorkerKind.WASM, KindPolicy(fidelity=False, restart=Nothing)),
    (WorkerKind.DAEMON, KindPolicy(fidelity=False, restart=Some(RetryClass.SPAWN))),
    (WorkerKind.REMOTE, KindPolicy(fidelity=True, restart=Some(RetryClass.SSH))),
])
```

## [03]-[POOL]

- Owner: `WorkerPool` is the warm reusable pool capsule — one polymorphic surface over the process-executor, device, and fleet remote arms: `loky.get_reusable_executor` for `COOPERATIVE` (process-global warm pool, crash-respawning, cloudpickle payloads, idle reap on `timeout`), `pebble.ProcessPool` for `TERMINAL` (`schedule(timeout=)` kills a running worker at wall-clock and reclaims the slot; `max_tasks` recycles a worker after N tasks bounding RSS creep), one `asyncssh` channel per `WorkerKind.REMOTE` arm (a memoized `SSHClientConnection` off `transport/roots#RESOURCE` `RemoteEndpoint.dialed`, per-submit `create_process` sessions running `remote_floor`, a per-arm session limiter bounding fleet in-flight), and one instance-owned loky executor per `WorkerKind.GPU` device arm — the process-global singleton cannot hold per-device state, so device custody is pebble-style instance ownership, `env=` pinning the device before any worker module loads on the cooperative arm and the `_worker_boot` initializer pinning it on the terminal arm. Custody follows each substrate's own topology: the pebble pool is instance-owned, the loky singleton arm holds acquisition arguments and re-acquires through the factory per use because loky is process-global and a broken instance is replaced only by re-acquisition — a field pinning the executor pins the corpse — the remote arm re-dials a closed channel per use under the same law, and a broken device instance drops its corpse at the submit boundary so the next acquisition mints fresh. Arm selection is `(WorkerKind, Enforcement, placement key)` — one derived key off the `Placement` shape, endpoint key, device key, or `""` local, never a caller-facing executor knob — and the subinterpreter and sandbox kinds ride no pool: the anyio arms are already own-GIL or in-process-killable substrates a package pool duplicates without adding respawn or kill capability.
- Entry: `WorkerPool.acquire(kind, enforcement, placement)` memoizes one live pool per arm key behind a membership guard — an effectful `setdefault` mint is the deleted form — so every call site shares the warm workers; `live` reads that registry without minting under the SAME `(kind, enforcement, placement)` triple every other verb takes, `alive` reads the arm's own liveness (pebble `active`; the loky arms self-heal per acquisition; the remote arm reads `is_closed` off its dialed channel), and `pids` is total — every arm names its own live worker set off its substrate's map, so an arm-scoped probe weighs exactly its workers and never a process-tree complement two unnamed arms would share. Both teardowns drop the memo under one ownership gate — a drained arm is re-mintable rather than a registered key whose submit fence refuses forever — and the last drop retires the `WORKER_BAND` occupancy probe the first mint registered, so the band's level series lives exactly as long as something bounds it; the fleet arm's per-arm session limiter carries its own `session` band over its own capsule lifetime, so a saturated channel and a saturated pool read apart instead of summing into one number. `submit(kernel, *args)` injects the trace carrier, drives the arm's executor with `traced_kernel`, and settles the future on a `WORKER_BAND`-bounded thread with `abandon_on_cancel=True` — the band token bounds pool in-flight and settle threads in one acquisition, and a cooperative cancel abandons the settle while the worker runs to completion under the pool's reaper, exactly the `COOPERATIVE` law; on the terminal arm the same cancel instead escalates through `ProcessFuture.cancel`, pebble terminating the RUNNING task so the killable slot reclaims immediately rather than holding to the wall-clock kill. In-band worker-death retry reads `kernel.retry` before the terminal lift, and each attempt re-acquires the executor, so a `TerminatedWorkerError` re-submission genuinely lands on the respawned pool and a non-idempotent kernel never re-runs.
- Law: the remote crossing seals `(boot, carrier, kernel, args)` as the one cloudpickle blob the TERMINAL arm already ships — stdin in, one pickled verdict out — and the hop honours `kernel.deadline` itself since no lane wraps a direct fleet submit: the deadline scope sits OUTSIDE the retry and results the typed `deadline` fault when tripped, the in-flight session's cancellation cleanup escalating on the way out — `TERMINAL` kills the far process mid-kernel, `COOPERATIVE` terminates the session so the channel reaps the floor — while a torn or empty verdict raises `ConnectionError` into the `SSH` retry band. `boot` is the parent-captured worker floor and may be `None` only when the pebble initializer already installed it. In-band retry re-keys to `KIND_POLICY[REMOTE].restart` under the kernel's `idempotent` declaration — executor-death names are meaningless across a channel — and `Wire.SHARED_MEMORY` is refused as a typed `config` fault because a span name never resolves across hosts; silently downgrading a declared zero-copy crossing to a copy betrays the declaration.
- Auto: every process arm pins an explicit spawn-family start method — `get_context("spawn")` on pebble, the loky spawn-based `"loky"` method on both loky arms — so crossing semantics never fork by platform default; every process arm's initializer is the one `_worker_boot` — device visibility pins first, the fidelity latch re-arms per its `KIND_POLICY` row, and the parent-captured `WorkerBoot` installs telemetry, instruments, and the profiler post-spawn under `WORKER_SIGNAL_PROFILE` with the exit drain registered — while the capsule latches parent-side at construction, so a worker raise re-crosses with frames intact and the settle-side unpickle resolves them; every lifecycle transition writes its `PoolStatus` as one `pool` line at the transition — the phase methods and the SPAWNED mint at `acquire`'s membership guard alike — so pool chronology rides the one log path. A worker-local cache or native handle is a module-level `@cache` in the worker-body module, so the warm prime pays it once per spawned worker and a reaped worker's respawn re-pays it. Elasticity is the substrate's own pair — the idle reap shrinks a quiet pool and a later submit respawns to the cap — so a sizing knob never lands. Worker-death evidence is typed per arm — loky raises `TerminatedWorkerError`/`BrokenProcessPool` on the pending future, pebble raises `ProcessExpired` (pid, exitcode) or `TimeoutError` on the deadline kill — and the faults pool-death row lands each on the `resource` case while the deadline kill converts to the `deadline` case, never retried because the `WORKER` row's target excludes it; loky's exit-code forensics never land — re-acquisition replaces the corpse before a forensic read, so death evidence is the typed raise and pebble's pid/exitcode.
- Law: the worker flush law is exit-owned — `_worker_boot` registers the telemetry drain then the profiler stop through `atexit`, so a graceful settle (`shutdown(wait=True)`, pebble `close`/`join`, the remote floor's process exit) drains every worker's buffered tail, a roll's double-buffer drains the stale arm's workers the same way, and only the kill paths (`kill_workers`, pebble `stop`, the remote `abort`) forfeit at most one `WORKER_SIGNAL_PROFILE` export window. The boot is captured parent-side as data off `Telemetry.installed`/`Profiles.installed`, so a silent parent spawns silent workers, no endpoint knob rides the pool surface, and the worker geometry keeps the HTTP transport the fork fence requires — the gRPC egress row stays structurally refused on every spawned floor.
- Output: `PoolStatus` carries `phase`, `kind`, `enforcement`, and `workers` — lifecycle evidence, never task outcomes; task outcomes stay the lane's `Drained`.
- Growth: a new executor arm is one constructor match arm keyed by `(WorkerKind, Enforcement)`; a new fleet host is one `RemoteEndpoint` value and a new accelerator one `Device` value, each acquiring its own arm at zero new surface; a new lifecycle phase is one `PoolPhase` member; a new warm-state obligation is one initializer fold; a new worker-boot fact is one `WorkerBoot` field the one initializer reads.
- Boundary: pools serve the lanes offload hop, the daemon drain fold, the fleet and device consumers, and the supervisor — a consumer never imports an executor class, holds a future, or sizes a pool; sizing derives from `loky.cpu_count(only_physical_cores=True)`, which already folds the `LOKY_MAX_CPU_COUNT` deploy override and the cgroup budget, so a cgroup-capped batch arm is deploy placement with zero new surface — the capped daemon's workers inherit its cgroup and every pool self-sizes to the quota, scheduling class and affinity riding the same deploy custody, never a kind — and `WORKER_BAND` bounds in-flight admission above the pool, refusing burst past physical cores. `REMOTE` and `GPU` are caller placement, never trait-derived — the lanes offload never routes to them, and a consumer acquires the arm with its `RemoteEndpoint` or `Device` exactly as trait declaration is consumer domain knowledge on the crossing; priority is the same placement axis — a latency class acquires its own arm key, never a queue-discipline knob. Fan-out modality stays the lane's `drain` — the pools expose `submit` alone, never a second `map`, stream, or priority surface. `Kernel.of` is the whole payload-classification surface and `ShmSpan` the one out-of-band buffer channel, so no per-object wrap, pickler swap, or reducer registration exists beside them; the span bracket itself is the lane's, so `submit` takes arguments already exported and a direct-submit consumer brackets its own call with `exported`/`released` rather than expecting the pool to read `kernel.wire`. Host-side admission sits at that same boundary under the same law — the lane's `offload` runs `admitted` once for every crossing it drives, and a direct-submit consumer runs it itself, so the gate is never paid twice on the terminal guest route.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
import atexit
import os
from concurrent.futures import Future
from contextlib import ExitStack
from functools import partial
from multiprocessing import get_context
from pickle import PicklingError
from typing import Final, Literal, assert_never
from uuid import uuid4

import anyio
import anyio.to_thread
import asyncssh
import loky
import loky.backend.context
import pebble
from anyio import CapacityLimiter, move_on_after
from expression import Error, Nothing, Option, Result
from opentelemetry.exporter.otlp.proto.http import Compression
from opentelemetry.sdk.resources import SERVICE_INSTANCE_ID, SERVICE_NAME, SERVICE_NAMESPACE, Resource

from rasm.runtime.admission import RuntimeContext, RuntimeProfile
from rasm.runtime.faults import (
    SCHEMA_URL,
    WORKERS_CROSSING,
    WORKERS_ENDPOINT,
    WORKERS_PHASE,
    WORKERS_REMOTE,
    WORKERS_SEAL,
    WORKERS_SHM,
    Catch,
    RuntimeResult,
    async_boundary,
    expired,
)
from rasm.runtime.resilience import guard
from rasm.runtime.roots import RemoteEndpoint
from rasm.runtime.telemetry import NAMESPACE, SignalProfile, Telemetry


# --- [TYPES] ----------------------------------------------------------------------------


class PoolPhase(StrEnum):
    SPAWNED = "spawned"
    WARM = "warm"
    DRAINING = "draining"
    RETIRED = "retired"


type PoolKind = Literal[WorkerKind.PROCESS, WorkerKind.GPU, WorkerKind.REMOTE]
type ArmKey = tuple[PoolKind, Enforcement, str]
type Placement = RemoteEndpoint | Device | None

# --- [CONSTANTS] ------------------------------------------------------------------------

WORKER_BAND: Final[CapacityLimiter] = CapacityLimiter(loky.cpu_count(only_physical_cores=True))

REMOTE_FLOOR: Final[str] = "-m rasm.runtime.workers"

_SEAL_RAISES: Final[Catch] = (PicklingError, TypeError, AttributeError)
_CROSSING_RAISES: Final[Catch] = Exception

WORKER_SIGNAL_PROFILE: Final[SignalProfile] = SignalProfile(
    export_interval_ms=5000, schedule_delay_ms=1000, max_queue_size=512, max_export_batch_size=128, compression=Compression.Gzip
)

# --- [MODELS] ---------------------------------------------------------------------------


class Device(Struct, frozen=True, gc=False):
    index: int
    selector: str = "CUDA_VISIBLE_DEVICES"

    @property
    def key(self) -> str:
        return f"{self.selector}:{self.index}"

    @property
    def env(self) -> dict[str, str]:
        return {self.selector: str(self.index)}


class WorkerBoot(Struct, frozen=True):
    kind: WorkerKind
    profile: RuntimeProfile
    otel: str | None = None
    pyroscope: str | None = None
    tenant: str | None = None
    device: Device | None = None

    @staticmethod
    def captured(kind: WorkerKind, device: Device | None = None) -> "WorkerBoot":
        telemetry = Telemetry.installed()
        profiles = Profiles.installed()
        return WorkerBoot(
            kind=kind,
            profile=telemetry.map(lambda held: held.profile).default_value(RuntimeProfile.PACKAGE),
            otel=telemetry.map(lambda held: held.endpoint).to_optional(),
            pyroscope=profiles.map(lambda held: held.endpoint).to_optional(),
            tenant=profiles.bind(lambda held: Option.of_optional(held.tenant)).to_optional(),
            device=device,
        )

    @property
    def env(self) -> dict[str, str]:
        return {**({"OTEL_EXPORTER_OTLP_ENDPOINT": self.otel} if self.otel else {}), **(self.device.env if self.device is not None else {})}


class PoolStatus(Struct, frozen=True, gc=False):
    phase: PoolPhase
    kind: WorkerKind
    enforcement: Enforcement
    workers: int

    def facts(self) -> Facts:
        return {"phase": self.phase.value, "kind": self.kind.value, "enforcement": self.enforcement.value, "workers": self.workers}


# --- [OPERATIONS] -----------------------------------------------------------------------


def _placed(placement: Placement) -> str:
    return placement.key if placement is not None else ""


def _reported(status: PoolStatus) -> PoolStatus:
    logger().info("pool", **status.facts())
    return status


def worker_resource(kind: WorkerKind) -> Resource:
    return Resource.create(
        {
            SERVICE_NAMESPACE: NAMESPACE,
            SERVICE_NAME: SCOPES[Scope.SERVICE],
            SERVICE_INSTANCE_ID: uuid4().hex,
            "worker.kind": kind.value,
            "worker.pid": os.getpid(),
        },
        schema_url=SCHEMA_URL,
    )


def _worker_boot(boot: WorkerBoot) -> None:
    if boot.device is not None:
        os.environ.update(boot.device.env)
    if KIND_POLICY[boot.kind].fidelity:
        fidelity()
    ctx = RuntimeContext.admit(boot.profile)
    if boot.otel is not None:
        atexit.register(Telemetry.shutdown)
        installed = Telemetry.install(ctx, boot.otel, resource=worker_resource(boot.kind), signal_profile=WORKER_SIGNAL_PROFILE)
        Metrics.install(budget=installed.signal_profile.cardinality_budget)
    if boot.pyroscope is not None:
        atexit.register(Profiles.shutdown)
        Profiles.install(ctx, boot.pyroscope, tags={"worker.kind": boot.kind.value}, tenant=boot.tenant)


# --- [SERVICES] -------------------------------------------------------------------------


class WorkerPool:
    _live: Final[dict[ArmKey, "WorkerPool"]] = {}
    _probes: Final[ExitStack] = ExitStack()

    def __init__(self, kind: PoolKind, enforcement: Enforcement, workers: int, placement: Placement = None) -> None:
        self._kind, self._enforcement, self._workers, self._placement = kind, enforcement, workers, placement
        self._key: ArmKey = (kind, enforcement, _placed(placement))
        self._phase = PoolPhase.SPAWNED
        self._boot = WorkerBoot.captured(kind, placement if isinstance(placement, Device) else None)
        if KIND_POLICY[kind].fidelity:
            fidelity()
        match kind, enforcement, placement:
            case (WorkerKind.GPU, _, placed) if not isinstance(placed, Device):
                raise TypeError(f"workers.pool.{kind.value}: gpu-requires-device-placement")
            case (WorkerKind.REMOTE, _, placed) if not isinstance(placed, RemoteEndpoint):
                raise TypeError(f"workers.pool.{kind.value}: remote-requires-endpoint-placement")
            case (WorkerKind.REMOTE, _, _):
                self._conn: asyncssh.SSHClientConnection | None = None
                self._dial = anyio.Lock()
                self._sessions = CapacityLimiter(workers)
                self._occupancy = ExitStack()
                self._occupancy.enter_context(Metrics.occupied(lambda: self._sessions.borrowed_tokens, band="session"))
            case (WorkerKind.GPU, Enforcement.COOPERATIVE, Device()):
                self._loky: loky.ProcessPoolExecutor | None = None
                self._executor()
            case (_, Enforcement.TERMINAL, _):
                self._pebble = pebble.ProcessPool(
                    max_workers=workers, max_tasks=64, initializer=partial(_worker_boot, self._boot), context=get_context("spawn")
                )
            case (_, Enforcement.COOPERATIVE, _):
                self._held: loky.ProcessPoolExecutor | None = None
                self._executor()
            case _ as unmatched:
                assert_never(unmatched)

    def _executor(self, kill_workers: bool = False) -> loky.ProcessPoolExecutor:
        loky.backend.resource_tracker._resource_tracker._use_simple_format = True
        if isinstance(self._placement, Device):
            if kill_workers and self._loky is not None:
                self._loky.shutdown(wait=False, kill_workers=True)
                self._loky = None
            if self._loky is None:
                self._loky = loky.ProcessPoolExecutor(
                    max_workers=self._workers, timeout=120, context=loky.backend.context.get_context("loky"),
                    initializer=partial(_worker_boot, self._boot), env=self._placement.env,
                )
            return self._loky
        self._held = loky.get_reusable_executor(
            max_workers=self._workers, context="loky", timeout=120, kill_workers=kill_workers, reuse="auto",
            initializer=partial(_worker_boot, self._boot),
        )
        return self._held

    @classmethod
    def acquire(cls, kind: PoolKind, enforcement: Enforcement = Enforcement.COOPERATIVE, placement: Placement = None) -> "WorkerPool":
        key: ArmKey = (kind, enforcement, _placed(placement))
        if key not in cls._live:
            if not cls._live:
                cls._probes.enter_context(Metrics.occupied(lambda: WORKER_BAND.borrowed_tokens, band="worker"))
            cls._live[key] = cls(kind, enforcement, loky.cpu_count(only_physical_cores=True), placement)
            _reported(PoolStatus(phase=PoolPhase.SPAWNED, kind=kind, enforcement=enforcement, workers=0))
        return cls._live[key]

    @classmethod
    def live(cls, kind: PoolKind, enforcement: Enforcement = Enforcement.COOPERATIVE, placement: Placement = None) -> "Option[WorkerPool]":
        return Option.of_optional(cls._live.get((kind, enforcement, _placed(placement))))

    def alive(self) -> bool:
        if self._kind is WorkerKind.REMOTE:
            return self._conn is None or not self._conn.is_closed()
        return self._pebble.active if self._enforcement is Enforcement.TERMINAL else True

    def pids(self) -> frozenset[int]:
        match self._kind, self._enforcement:
            case (WorkerKind.REMOTE, _):
                return frozenset()
            case (_, Enforcement.TERMINAL):
                return frozenset(self._pebble._pool_manager.worker_manager.workers)
            case (_, Enforcement.COOPERATIVE):
                held = self._loky if isinstance(self._placement, Device) else self._held
                return frozenset(held._processes) if held is not None else frozenset()
            case _ as unmatched:
                assert_never(unmatched)

    @classmethod
    async def roll(cls, kind: PoolKind, enforcement: Enforcement = Enforcement.COOPERATIVE, placement: Placement = None) -> "PoolStatus":
        stale = cls._live.pop((kind, enforcement, _placed(placement)), None)
        if stale is not None and enforcement is Enforcement.COOPERATIVE and kind is WorkerKind.PROCESS:
            await anyio.to_thread.run_sync(lambda: stale._executor(kill_workers=True), abandon_on_cancel=True, limiter=WORKER_BAND)
        warmed = await cls.acquire(kind, enforcement, placement).warm()
        if stale is not None and (enforcement is Enforcement.TERMINAL or kind in (WorkerKind.GPU, WorkerKind.REMOTE)):
            await stale.drain()
        return warmed

    async def _connection(self, endpoint: RemoteEndpoint) -> asyncssh.SSHClientConnection:
        async with self._dial:
            if self._conn is None or self._conn.is_closed():
                self._conn = await endpoint.dialed()
            return self._conn

    async def _remote[T](self, carrier: dict[str, str], kernel: Kernel[T], args: tuple[object, ...]) -> RuntimeResult[T]:
        match self._placement, kernel.wire:
            case (RemoteEndpoint(), Wire.SHARED_MEMORY):
                return Error(WORKERS_SHM.raised(kernel.name))
            case (RemoteEndpoint() as endpoint, _):
                pass
            case _:
                return Error(WORKERS_ENDPOINT.raised(kernel.name))

        async def crossing(blob: bytes) -> T:
            conn = await self._connection(endpoint)
            async with self._sessions:
                async with await conn.create_process(f"{endpoint.python} {REMOTE_FLOOR}", encoding=None, stderr=asyncssh.DEVNULL) as process:
                    try:
                        process.stdin.write(blob)
                        process.stdin.write_eof()
                        span = int.from_bytes(await process.stdout.readexactly(8), "big")
                        if not 0 < span <= VERDICT_FRAME:
                            raise ConnectionError(f"workers.remote.{kernel.name}:frame:{span}")
                        payload = await process.stdout.readexactly(span)
                    except anyio.get_cancelled_exc_class():
                        (process.kill if kernel.enforcement is Enforcement.TERMINAL else process.terminate)()
                        raise
                    except EOFError as torn:
                        raise ConnectionError(f"workers.remote.{kernel.name}:exit={process.exit_status}") from torn
            match cloudpickle.loads(payload):
                case ("value", value):
                    return value
                case ("raise", CrossedFault() as carried):
                    raise carried.homed().default_value(carried)
                case ("raise", BaseException() as raised):
                    raise raised
                case _:
                    raise ConnectionError(f"workers.remote.{kernel.name}:torn-verdict")

        keyed = KIND_POLICY[WorkerKind.REMOTE].restart if kernel.idempotent else Nothing
        with move_on_after(kernel.deadline.default_value(float("inf"))):
            sealed = await async_boundary(
                WORKERS_SEAL,
                lambda: anyio.to_thread.run_sync(
                    lambda: cloudpickle.dumps((self._boot, carrier, kernel, args)), abandon_on_cancel=True, limiter=WORKER_BAND
                ),
                catch=_SEAL_RAISES,
            )
            match sealed:
                case Result(tag="error") as refused:
                    return refused
                case Result(tag="ok", ok=blob):
                    run = partial(crossing, blob)
                    return await async_boundary(
                        WORKERS_REMOTE, lambda: _homing(keyed.map(lambda cls: guard(cls)(run)).default_with(run)), catch=_CROSSING_RAISES
                    )
                case _ as unreachable:
                    assert_never(unreachable)
        return Error(expired(WORKERS_REMOTE, kernel.deadline, f"remote-kill:{kernel.name}"))

    async def submit[T](self, kernel: Kernel[T], *args: object) -> RuntimeResult[T]:
        if self._phase in (PoolPhase.DRAINING, PoolPhase.RETIRED):
            return Error(WORKERS_PHASE.raised(self._kind.value, kernel.name, self._phase.value))
        carrier: dict[str, str] = {}
        propagate.inject(carrier)
        if self._kind is WorkerKind.REMOTE:
            return await self._remote(carrier, kernel, args)

        async def crossing() -> T:
            if self._enforcement is Enforcement.TERMINAL:
                sealed = await anyio.to_thread.run_sync(
                    lambda: cloudpickle.dumps((None, carrier, kernel, args)), abandon_on_cancel=True, limiter=WORKER_BAND
                )
                pending: Future[T] = self._pebble.schedule(sealed_kernel, args=(sealed,), timeout=kernel.deadline.to_optional())
                try:
                    return await anyio.to_thread.run_sync(pending.result, abandon_on_cancel=True, limiter=WORKER_BAND)
                except anyio.get_cancelled_exc_class():
                    pending.cancel()
                    raise

            def settled() -> T:
                try:
                    return self._executor().submit(traced_kernel, carrier, kernel, *args).result()
                except loky.BrokenProcessPool:
                    if self._kind is WorkerKind.GPU:
                        self._loky = None
                    raise

            return await anyio.to_thread.run_sync(settled, abandon_on_cancel=True, limiter=WORKER_BAND)

        return await async_boundary(
            WORKERS_CROSSING,
            lambda: _homing(kernel.retry.map(lambda cls: guard(cls)(crossing)).default_with(crossing)),
            catch=_CROSSING_RAISES,
        )

    async def warm(self, count: int | None = None) -> "PoolStatus":
        primed = self._workers if count is None else max(0, min(count, self._workers))
        async with anyio.create_task_group() as group:
            handles = tuple(group.start_soon(self.submit, Kernel.of(fidelity, KernelTrait.HOSTILE)) for _ in range(primed))
        live = sum(1 for handle in handles if handle.return_value.is_ok())
        self._phase = PoolPhase.WARM if live else self._phase
        return _reported(PoolStatus(phase=self._phase, kind=self._kind, enforcement=self._enforcement, workers=live))

    async def drain(self, grace: float = 30.0) -> "PoolStatus":
        self._phase = PoolPhase.DRAINING
        match self._kind, self._enforcement:
            case (WorkerKind.REMOTE, _):
                with move_on_after(grace):
                    while self._sessions.statistics().borrowed_tokens:
                        await anyio.sleep(0.05)
                if self._conn is not None:
                    self._conn.close()
                    await self._conn.wait_closed()
                self._occupancy.close()
            case (_, Enforcement.TERMINAL):
                await anyio.to_thread.run_sync(lambda: (self._pebble.close(), self._pebble.join()), abandon_on_cancel=True, limiter=WORKER_BAND)
            case (WorkerKind.GPU, Enforcement.COOPERATIVE):
                if self._loky is not None:
                    await anyio.to_thread.run_sync(lambda: self._loky.shutdown(wait=True), abandon_on_cancel=True, limiter=WORKER_BAND)
            case (_, Enforcement.COOPERATIVE):
                if WorkerPool._live.get(self._key) is self:
                    await anyio.to_thread.run_sync(lambda: self._executor().shutdown(wait=True), abandon_on_cancel=True, limiter=WORKER_BAND)
            case _ as unmatched:
                assert_never(unmatched)
        if WorkerPool._live.get(self._key) is self:
            WorkerPool._live.pop(self._key)
            self._retired()
        return _reported(PoolStatus(phase=self._phase, kind=self._kind, enforcement=self._enforcement, workers=self._workers))

    @staticmethod
    def _retired() -> None:
        if not WorkerPool._live:
            WorkerPool._probes.close()

    def retire(self) -> "PoolStatus":
        match self._kind, self._enforcement:
            case (WorkerKind.REMOTE, _):
                if self._conn is not None:
                    self._conn.abort()
                self._occupancy.close()
            case (_, Enforcement.TERMINAL):
                self._pebble.stop()
                self._pebble.join()
            case (WorkerKind.GPU, Enforcement.COOPERATIVE):
                if self._loky is not None:
                    self._loky.shutdown(wait=False, kill_workers=True)
            case (_, Enforcement.COOPERATIVE):
                if WorkerPool._live.get(self._key) is self:
                    self._executor().shutdown(wait=False, kill_workers=True)
            case _ as unmatched:
                assert_never(unmatched)
        self._phase = PoolPhase.RETIRED
        if WorkerPool._live.get(self._key) is self:
            WorkerPool._live.pop(self._key)
            self._retired()
        return _reported(PoolStatus(phase=self._phase, kind=self._kind, enforcement=self._enforcement, workers=0))
```

## [04]-[LEASE]

- Owner: `LeaseSession` composes one `LeasePort` with an admitted `BackendGeneration` and one existing `WorkerPool`, so a distributed work claim reuses the whole crossing this page already owns — pool, kernel value, band, and retry row — and adds only the fence. It mints no worker, no executor, and no second band.
- Cases: `LeaseOp` closes claim, heartbeat, settle, and drain behind one provider-polymorphic `apply`, and `LeaseVerdict` closes what a provider may answer, so every arm this owner does not expect is a typed refusal naming the verb rather than an unhandled shape.
- Entry: `LeaseSession.run(kernel, evidence)` is the one work entry — claim, race, settle in one call — and `drain` the one retirement. A caller hands the kernel it would have submitted to the pool directly plus the projection that turns its result into settlement bytes, so adopting the lease costs no change to the kernel and no second submit surface stands beside `WorkerPool.submit`.
- Auto: the heartbeat and the work run as siblings under ONE task group whose first message cancels the other, so a lost lease preempts the crossing under the kernel's own declared enforcement instead of letting a worker finish against a fence it no longer holds; the stream is depth-1 because exactly one message decides the race, and the loser's send is cancelled with its scope rather than buffered into a slot nobody reads. Renewal cadence derives from the fence the provider actually granted — `WorkLease.ttl` times the policy fraction — so a heartbeat can never silently outrun the lease it defends the way a free absolute interval does.
- Law: `LeaseDemand` carries generation, worker identity, and capability evidence; `WorkLease` returns one fenced token, its payload, and the provider's own fence lifetime. Generation crosses as the admission owner's `Digest128`, so the claim, the returned fence, and the drift compare read one 32-hex domain.
- Law: settlement is TOTAL over every terminal — success carries a kernel content identity under the seed-zero parity contract, a kernel fault and a raised evidence projection each carry their typed fault tag — so the provider's token fence retires on every path and no terminal leaves a live lease waiting out its own lapse; the fence is what keeps a settlement from landing against a lease another worker now holds.
- Law: provider admission closes before pool drain, so no claim enters after local execution begins settling.
- Packages: AnyIO structured concurrency, `expression` carriers, `msgspec` unions, and runtime `ContentIdentity`.
- Growth: a provider implements one port; a lease transition is one `LeaseOp` case with its `LeaseVerdict` answer and one `_verdict` row; worker execution and supervision remain unchanged.
- Boundary: adapters own PostgreSQL, broker, or service calls; this owner carries no SQL, polling protocol, or provider transaction.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Callable
from typing import Annotated, Literal, Protocol, assert_never

import anyio
from expression import Error, Ok, Result, Some, case, tag, tagged_union
from msgspec import Meta, Struct

from rasm.runtime.admission import BackendGeneration, Digest128
from rasm.runtime.faults import LEASE_DRIFT, LEASE_EVIDENCE, LEASE_LOST, LEASE_VERDICT, RuntimeResult, boundary
from rasm.runtime.identity import U128, ContentIdentity


# --- [TYPES] ----------------------------------------------------------------------------


type LeaseRace[T] = tuple[Literal["work"], RuntimeResult[T]] | tuple[Literal["lease"], BoundaryFault]

# --- [MODELS] ---------------------------------------------------------------------------


class LeaseDemand(Struct, frozen=True):
    generation: Digest128
    worker: str
    capabilities: frozenset[str]


class LeasePolicy(Struct, frozen=True, gc=False):
    renew: Annotated[float, Meta(gt=0.0, lt=1.0)] = 0.5


class WorkLease(Struct, frozen=True):
    token: str
    generation: Digest128
    payload: bytes
    ttl: Annotated[float, Meta(gt=0.0)]


@tagged_union(frozen=True)
class LeaseSettlement:
    tag: Literal["succeeded", "failed"] = tag()
    succeeded: U128 = case()
    failed: str = case()


@tagged_union(frozen=True)
class LeaseOp:
    tag: Literal["claim", "heartbeat", "settle", "drain"] = tag()
    claim: LeaseDemand = case()
    heartbeat: WorkLease = case()
    settle: tuple[WorkLease, LeaseSettlement] = case()
    drain: str = case()


@tagged_union(frozen=True)
class LeaseVerdict:
    tag: Literal["claimed", "renewed", "settled", "drained", "empty", "lost"] = tag()
    claimed: WorkLease = case()
    renewed: str = case()
    settled: str = case()
    drained: str = case()
    empty: str = case()
    lost: str = case()


class LeasePort(Protocol):
    async def apply(self, operation: LeaseOp, /) -> RuntimeResult[LeaseVerdict]: ...


# --- [SERVICES] -------------------------------------------------------------------------


class LeaseSession(Struct, frozen=True):
    port: LeasePort
    pool: WorkerPool
    generation: BackendGeneration
    worker: str
    policy: LeasePolicy

    async def run[T](self, kernel: Kernel[T], evidence: Callable[[T], bytes], /) -> RuntimeResult[T]:
        demand = LeaseDemand(generation=self.generation.generation, worker=self.worker, capabilities=self.generation.observed.capabilities)
        claimed = _verdict("claim", await self.port.apply(LeaseOp(claim=demand)), "claimed").bind(
            lambda verdict: Ok(verdict.claimed)
            if verdict.claimed.generation == demand.generation
            else Error(LEASE_DRIFT.raised())
        )
        match claimed:
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=lease):
                return await self._settled(lease, evidence, await self._raced(kernel, lease))
            case _ as unreachable:
                assert_never(unreachable)

    async def _raced[T](self, kernel: Kernel[T], lease: WorkLease) -> LeaseRace[T]:
        send, receive = anyio.create_memory_object_stream[LeaseRace[T]](1)

        async def worked() -> None:
            await send.send(("work", await self.pool.submit(kernel, lease.payload)))

        async def pulsed() -> None:
            while True:
                await anyio.sleep(lease.ttl * self.policy.renew)
                match _verdict("heartbeat", await self.port.apply(LeaseOp(heartbeat=lease)), "renewed"):
                    case Result(tag="error", error=fault):
                        await send.send(("lease", fault))
                        return
                    case _:
                        continue

        async with send, receive, anyio.create_task_group() as group:
            group.start_soon(worked)
            group.start_soon(pulsed)
            raced = await receive.receive()
            group.cancel_scope.cancel()
        return raced

    async def _settled[T](self, lease: WorkLease, evidence: Callable[[T], bytes], raced: LeaseRace[T]) -> RuntimeResult[T]:
        match raced:
            case ("lease", fault):
                return Error(fault)
            case ("work", outcome):
                return await self._reported(lease, outcome, self._sealed(outcome, evidence))
            case _ as unreachable:
                assert_never(unreachable)

    def _sealed[T](self, outcome: RuntimeResult[T], evidence: Callable[[T], bytes]) -> LeaseSettlement:
        match outcome:
            case Result(tag="error", error=fault):
                return LeaseSettlement(failed=fault.tag)
            case Result(tag="ok", ok=value):
                return boundary(
                    LEASE_EVIDENCE,
                    lambda: LeaseSettlement(succeeded=ContentIdentity.key("worker-settlement", evidence(value), seed=Some(0)).value),
                    catch=Exception,
                ).default_with(lambda refused: LeaseSettlement(failed=refused.tag))
            case _ as unreachable:
                assert_never(unreachable)

    async def _reported[T](self, lease: WorkLease, outcome: RuntimeResult[T], settlement: LeaseSettlement) -> RuntimeResult[T]:
        return _verdict("settle", await self.port.apply(LeaseOp(settle=(lease, settlement))), "settled").bind(lambda _settled: outcome)

    async def drain(self) -> RuntimeResult[PoolStatus]:
        match _verdict("drain", await self.port.apply(LeaseOp(drain=self.worker)), "drained"):
            case Result(tag="error") as refused:
                return refused
            case _:
                return Ok(await self.pool.drain())


# --- [OPERATIONS] -----------------------------------------------------------------------


def _verdict(verb: str, answered: RuntimeResult[LeaseVerdict], expected: str) -> RuntimeResult[LeaseVerdict]:
    match answered:
        case Result(tag="error") as refused:
            return refused
        case Result(tag="ok", ok=verdict) if verdict.tag == expected:
            return Ok(verdict)
        case Result(tag="ok", ok=LeaseVerdict(tag="lost", lost=reason)):
            return Error(LEASE_LOST.raised(reason))
        case Result(tag="ok", ok=LeaseVerdict(tag="empty")):
            return Error(LEASE_LOST.raised("empty"))
        case Result(tag="ok", ok=verdict):
            return Error(LEASE_VERDICT.raised(verb, verdict.tag))
        case _ as unreachable:
            assert_never(unreachable)
```

## [05]-[SUPERVISION]

- Owner: `Supervisor` binds the probe evidence, the restart rows, and the health projection into the one actuator loop the branch lacked: every ingredient existed — psutil probes, retry backoff, health status, worker-death markers — and this owner closes them. A `SupervisionPolicy` row per supervised subject carries the probe ceilings and the windowed restart budget; verdicts are data the loop folds, never inline judgment.
- Cases: `Verdict` closes the probe outcomes — `LIVE`, `UNMEASURED` (every column refused its read), `DEGRADED` (a `Breach` row tripped: rss over budget, involuntary context-switch storm, socket-table growth), `DEAD` (child gone, pool retired, channel closed) — and `_actuate` maps each onto its actuation: `LIVE` re-arms the probe, `UNMEASURED` emits its evidence and actuates nothing, `DEGRADED` rolls the arm, `DEAD` flips the subject down and re-spawns under the kind's restart row with stamina backoff. Budgets are windowed: `restarts` actuations inside `window` seconds park the subject `NOT_SERVING` until the window drains, so a crash storm holds down instead of thrashing.
- Law: a probe answers `Weighed` — the verdict plus the columns that produced it — never a collapsed bool, and every ceiling column is optional because a `PROCESS_FAULTS` refusal is a measurement nobody took: a zero there reads as a healthy worker and yields exactly the false `LIVE` this actuator exists to catch, so an all-refused read is `UNMEASURED` and actuates nothing rather than advertising health or spending the restart budget on evidence about the probe. Fences are per COLUMN, so a denied socket table never darkens an rss the same subject measured, and the arm-level reading is the PEAK of each column across the arm's workers folded through the one `_judged` law. The actuation line carries those columns beside its own fact — `parked` on a budget hold, `restarted` on a granted one — so an operator reads which ceiling moved, by how much, and what the actuator did in one shape.
- Entry: `Supervisor.watch(group)` starts one probe loop per supervised subject inside the caller's task group — the daemon composition root's supervision group, never a private loop — each cycle fenced so a probe or actuation raise writes one warning and the rhythm survives, and the whole weighing crosses to a thread under this owner's OWN band so a slow psutil read never parks the loop and never borrows the pool's width. Probes are arm-scoped: a `DAEMON` charge reads its own child handle — the two `oneshot`-batched ceilings in one collection and the socket count on its own syscall, since that member is not in the batch — and a pool charge reads capsule presence and the arm's own `alive()` before weighing exactly the pids that arm names, so a daemon child, a sibling arm's worker, and an unrelated grandchild all stay outside this subject's verdict by construction rather than by a complement two unnamed arms would share; a `REMOTE` charge reads channel liveness alone and carries `held`/`alive` as its columns, resource ceilings belonging to the far host's own supervisor and fleet saturation to the arm's own `session` band, while a `GPU` charge weighs host RSS alone — device-memory evidence stays the kernel's own cost bracket, unobservable through a psutil scan. The supervision band publishes its own occupancy for exactly the watched lifetime, so a probe rhythm queueing behind slow syscalls reads as saturation rather than as silence. Verdicts project onto the serve owner's per-service state — the injected awaited `ServerHost.status` coroutine — so any `DEAD` subject flips its service `NOT_SERVING` and recovery flips it back for the next generated `Health.Check` poll. The `verdicts` accessor publishes the same last-verdict state as data for the bundle capsule, so no second verdict surface exists beside the flip and the projection.
- Daemon kind: a `DAEMON` worker is a supervised long-lived child — `psutil.Popen` fuses the subprocess handle with the probe surface, the spawn environment forwards the parent's effective OTLP endpoint through `WorkerBoot.env` so the child's own composition root installs against the same collector, `terminate()` then `kill()` after the grace window is the stop escalation, restart rides the `SPAWN` row targeting spawn transients, and an empty spawn command is a config refusal that parks the subject down; a daemon's readiness is its next `LIVE` verdict, never a sleep. Every reap runs `psutil.Popen.wait(timeout=)` inside the banded thread rather than an enclosing cancel scope, so the one `_reaped` helper serves the restart escalation and the stop sweep alike. `Supervisor.stop()` runs that same escalation over every surviving child as the serve drain fold's daemon row, so a child never outlives the daemon that spawned it.
- Growth: a new probe dimension is one `SupervisionPolicy` ceiling field, one `Breach` member, and one `_judged` row — the arm fold, the line columns, and the peak projection each name themselves from it; a new actuation is one `_actuate` arm; a new supervised subject is one `Supervisor.watch` registration.
- Boundary: the supervisor actuates pooled arms — device arms included — remote channels, and daemon children only — `ChargeKind` seals that subject set by construction — and it never restarts the serve host, never owns the signal boundary (`transport/serve#ENTRY`'s), and never emits health protocol wire (the serve owner's `ServerHost`, the generated `grpc.health.v1` `Health` servicer, is the sole advertiser). Probe evidence writes its `supervise` line at the actuation site.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
import time
from collections.abc import Awaitable, Callable
from contextlib import suppress
from enum import StrEnum
from typing import Literal, assert_never

import anyio
import anyio.to_thread
import psutil
from anyio.abc import TaskGroup
from expression import Result
from expression.collections import Block

from rasm.runtime.faults import SUPERVISE_CYCLE, WORKERS_COMMAND, async_boundary
from rasm.runtime.observe import PROCESS_FAULTS
from rasm.runtime.resilience import guard


# --- [TYPES] ----------------------------------------------------------------------------


class Verdict(StrEnum):
    LIVE = "live"
    UNMEASURED = "unmeasured"
    DEGRADED = "degraded"
    DEAD = "dead"


class Breach(StrEnum):
    RSS = "rss"
    SWITCHES = "switches"
    SOCKETS = "sockets"


type Flip = Callable[[str, bool], Awaitable[None]]
type ChargeKind = Literal[WorkerKind.PROCESS, WorkerKind.GPU, WorkerKind.DAEMON, WorkerKind.REMOTE]

# --- [MODELS] ---------------------------------------------------------------------------


class SupervisionPolicy(Struct, frozen=True, gc=False):
    subject: str
    interval: float = 5.0
    rss_ceiling: int = 2_147_483_648
    switch_ceiling: int = 100_000
    socket_ceiling: int = 1_024
    grace: float = 5.0
    restarts: int = 3
    window: float = 300.0


class Weighed(Struct, frozen=True, gc=False):
    verdict: Verdict
    rss: int | None = None
    switches: int | None = None
    sockets: int | None = None
    held: bool | None = None
    alive: bool | None = None
    breached: frozenset[Breach] = frozenset()

    def facts(self) -> Facts:
        columns: dict[str, object | None] = {
            "rss": self.rss,
            "switches": self.switches,
            "sockets": self.sockets,
            "held": self.held,
            "alive": self.alive,
            "breached": sorted(breach.value for breach in self.breached) if self.breached else None,
        }
        return {"verdict": self.verdict.value, **{key: value for key, value in columns.items() if value is not None}}


class Charge(Struct, frozen=True):
    policy: SupervisionPolicy
    kind: ChargeKind
    enforcement: Enforcement = Enforcement.COOPERATIVE
    command: tuple[str, ...] = ()
    placement: Placement = None


# --- [OPERATIONS] -----------------------------------------------------------------------


def _reading[T](read: Callable[[], T]) -> T | None:
    with suppress(*PROCESS_FAULTS):
        return read()
    return None


def _batched(proc: psutil.Process) -> tuple[int, int]:
    with proc.oneshot():
        return proc.memory_info().rss, proc.num_ctx_switches().involuntary


def _judged(rss: int | None, switches: int | None, sockets: int | None, policy: SupervisionPolicy) -> Weighed:
    breached = frozenset(
        breach
        for breach, measured, ceiling in (
            (Breach.RSS, rss, policy.rss_ceiling),
            (Breach.SWITCHES, switches, policy.switch_ceiling),
            (Breach.SOCKETS, sockets, policy.socket_ceiling),
        )
        if measured is not None and measured > ceiling
    )
    graded = Verdict.DEGRADED if breached else Verdict.LIVE
    return Weighed(
        verdict=graded if any(column is not None for column in (rss, switches, sockets)) else Verdict.UNMEASURED,
        rss=rss,
        switches=switches,
        sockets=sockets,
        breached=breached,
    )


def _peak(readings: Block[Weighed], column: Callable[[Weighed], int | None]) -> int | None:
    measured = tuple(value for value in readings.map(column) if value is not None)
    return max(measured) if measured else None


def _folded(readings: Block[Weighed], policy: SupervisionPolicy) -> Weighed:
    return _judged(
        _peak(readings, lambda held: held.rss),
        _peak(readings, lambda held: held.switches),
        _peak(readings, lambda held: held.sockets),
        policy,
    )


# --- [SERVICES] -------------------------------------------------------------------------


class Supervisor:
    def __init__(self, charges: Block[Charge], flip: Flip) -> None:
        self._charges, self._flip = charges, flip
        self._verdicts: dict[str, Verdict] = {}
        self._served: dict[str, bool] = {}
        self._children: dict[str, psutil.Popen] = {}
        self._stamps: dict[str, tuple[float, ...]] = {}
        self._band = CapacityLimiter(max(1, len(charges)))
        self._probes = ExitStack()

    def _weighed(self, subject: psutil.Process | int, policy: SupervisionPolicy) -> Weighed:
        proc = _reading(lambda: psutil.Process(subject) if isinstance(subject, int) else subject)
        if proc is None:
            return _judged(None, None, None, policy)
        batched = _reading(lambda: _batched(proc))
        rss, switches = batched if batched is not None else (None, None)
        return _judged(rss, switches, _reading(lambda: len(proc.net_connections())), policy)

    def _probe(self, charge: Charge) -> Weighed:
        match charge.kind:
            case WorkerKind.DAEMON:
                handle = self._children.get(charge.policy.subject)
                if handle is None or not handle.is_running():
                    return Weighed(verdict=Verdict.DEAD, held=handle is not None, alive=False)
                return self._weighed(handle, charge.policy)
            case WorkerKind.REMOTE:
                arm = WorkerPool.live(charge.kind, charge.enforcement, charge.placement)
                alive = arm.map(lambda pool: pool.alive()).default_value(False)
                return Weighed(verdict=Verdict.LIVE if alive else Verdict.DEAD, held=arm.is_some(), alive=alive)
            case kind:
                arm = WorkerPool.live(kind, charge.enforcement, charge.placement)
                if arm.is_none() or not arm.value.alive():
                    return Weighed(verdict=Verdict.DEAD, held=arm.is_some(), alive=False)
                return _folded(Block.of_seq(self._weighed(pid, charge.policy) for pid in arm.value.pids()), charge.policy)

    def _budgeted(self, policy: SupervisionPolicy) -> bool:
        now = time.monotonic()
        fresh = tuple(stamp for stamp in self._stamps.get(policy.subject, ()) if now - stamp < policy.window)
        granted = len(fresh) < policy.restarts
        self._stamps[policy.subject] = (*fresh, now) if granted else fresh
        return granted

    async def _advertise(self, subject: str, serving: bool) -> None:
        if self._served.get(subject) is not serving:
            self._served[subject] = serving
            await self._flip(subject, serving)

    async def _actuate(self, charge: Charge, weighed: Weighed) -> None:
        subject = charge.policy.subject
        self._verdicts[subject] = weighed.verdict
        match weighed.verdict:
            case Verdict.LIVE:
                await self._advertise(subject, True)
            case Verdict.UNMEASURED:
                logger().info("supervise", subject=subject, **weighed.facts())
            case Verdict.DEGRADED | Verdict.DEAD:
                if not self._budgeted(charge.policy):
                    await self._advertise(subject, False)
                    logger().info("supervise", subject=subject, parked=True, **weighed.facts())
                    return
                if weighed.verdict is Verdict.DEAD:
                    await self._advertise(subject, False)
                respawn = KIND_POLICY[charge.kind].restart.map(lambda cls: guard(cls)(self._respawn, charge))
                restarted = await respawn.default_with(lambda: self._respawn(charge))
                logger().info("supervise", subject=subject, restarted=restarted, **weighed.facts())
            case _ as unreachable:
                assert_never(unreachable)

    async def _reaped(self, child: psutil.Popen, grace: float) -> None:
        with suppress(psutil.TimeoutExpired):
            await anyio.to_thread.run_sync(partial(child.wait, timeout=grace), abandon_on_cancel=True, limiter=self._band)

    async def _respawn(self, charge: Charge) -> bool:
        subject = charge.policy.subject
        match charge.kind:
            case WorkerKind.DAEMON:
                if not charge.command:
                    await self._advertise(subject, False)
                    logger().warning("supervise", subject=subject, **WORKERS_COMMAND.raised(subject).facts())
                    return False
                stale = self._children.get(subject)
                if stale is not None and stale.is_running():
                    stale.terminate()
                    await self._reaped(stale, charge.policy.grace)
                    if stale.is_running():
                        stale.kill()
                        await self._reaped(stale, charge.policy.grace)
                self._children[subject] = psutil.Popen(list(charge.command), env={**os.environ, **WorkerBoot.captured(WorkerKind.DAEMON).env})
                return True
            case kind:
                rolled = await WorkerPool.roll(kind, charge.enforcement, charge.placement)
                return rolled.phase is PoolPhase.WARM

    async def _cycle(self, charge: Charge) -> None:
        async def cycled() -> None:
            await self._actuate(charge, await anyio.to_thread.run_sync(self._probe, charge, abandon_on_cancel=True, limiter=self._band))

        while True:
            (await async_boundary(SUPERVISE_CYCLE, cycled, catch=Exception)).swap().map(
                lambda fault: logger().warning("supervise.cycle", **fault.facts())
            )
            await anyio.sleep(charge.policy.interval)

    def verdicts(self) -> Map[str, str]:
        return Map.of_seq((subject, verdict.value) for subject, verdict in self._verdicts.items())

    def watch(self, group: TaskGroup) -> None:
        self._probes.enter_context(Metrics.occupied(lambda: self._band.borrowed_tokens, band="supervision"))
        for charge in self._charges:
            group.start_soon(self._cycle, charge)

    async def stop(self) -> int:
        stopped = 0
        for charge in self._charges.filter(lambda held: held.kind is WorkerKind.DAEMON):
            child = self._children.pop(charge.policy.subject, None)
            if child is None or not child.is_running():
                continue
            child.terminate()
            await self._reaped(child, charge.policy.grace)
            if child.is_running():
                child.kill()
                await self._reaped(child, charge.policy.grace)
            stopped += 1
        self._probes.close()
        return stopped


# --- [ENTRY] ----------------------------------------------------------------------------

if __name__ == "__main__":
    sys.exit(remote_floor())
```

## [06]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
