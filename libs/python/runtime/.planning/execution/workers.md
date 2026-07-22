# [PY_RUNTIME_WORKERS]

One closed `WorkerKind` family carries every worker the branch runs — thread, subinterpreter, process, device-pinned process, daemon, remote, and sandboxed guest — each kind a `KIND_POLICY` row binding its fidelity obligation and restart class. `Kernel` is the single kernel-crossing owner: every callable that leaves the event loop crosses as one frozen value whose `KernelTrait` row derives isolation, worker-death retry, deadline enforcement, and shipping, so no consumer re-derives a modality, pairs a retry class by convention, or hand-builds a name-crossing gate. `WorkerPool` owns the warm reusable process pools and the fleet-host remote arm with spawn/warm/roll/drain/retire lifecycle, and `Supervisor` is the restart actuator — psutil probes, a windowed restart budget, and the health projection the serve owner advertises.

Composition is settled: the thread and subinterpreter crossing arms stay `execution/lanes#LANE` owners the trait table projects onto, while both process arms ride `WorkerPool` under the `WORKER_BAND` this page mints; worker-death backoff rides `reliability/resilience#RESILIENCE` rows (`OCCT` for the anyio subinterpreter arm, `WORKER` for the pool executors, `SPAWN` for a daemon child); every pool fault converts through the `reliability/faults#FAULT` lift, whose pool-death row lands executor deaths on the `resource` case; pool and supervision evidence streams through the `observability/receipts#RECEIPT` contributor port. `cloudpickle` ships a closure or local callable by value across the pickle seams stdlib pickle refuses, `tblib` re-raises a worker fault parent-side with its worker frames, `loky` owns the crash-respawning warm process pool every cooperative `PROCESS` offload rides, `pebble` owns the terminal wall-clock kill, PEP 734 `anyio.to_interpreter` owns the subinterpreter substrate with zero package cost, `asyncssh` carries the sealed kernel to a fleet host over the one `transport/roots#RESOURCE` `RemoteEndpoint` channel under the `SSH` restart row, `wasmtime` runs a sandboxed guest module in-process under a shared epoch pacer, and the device arms pin an accelerator through loky's `env=` and pebble's initializer at spawn — placement rides the existing pools, zero new package. Worker floors are parented emitters: every process arm boots the parent-captured telemetry install post-spawn through one `_worker_boot` initializer, the kernel span opens under the carried W3C parent, the profiler attaches where the cycles burn, and the two-read `Cost` bracket prices every crossing to the tenant that ran it.

## [01]-[INDEX]

- [01]-[FABRIC]: the `WorkerKind` family and its policy rows, the `KernelTrait` isolation classifier, the one `Kernel` crossing owner with shipping, wire, deadline, and enforcement as fields, the shared-memory span channel, the parented-emitter worker gate — kernel span, profiler phase, and cost bracket over the stitch-and-resolve pair — the remote-floor entry, the guest sandbox arm with its epoch pacer, and the tblib fidelity latch.
- [02]-[POOL]: the warm reusable `WorkerPool` capsule — loky, pebble, and per-device arms under one lifecycle vocabulary, the `WorkerBoot` install seam and its exit-owned flush law, band-bounded settle, in-band worker-death retry, and the asyncssh remote arm crossing the sealed kernel to a fleet host.
- [03]-[SUPERVISION]: the restart actuator — kind-scoped psutil probes as data verdicts, the windowed restart budget, escalation, the serve-facing health projection, and the bundle-facing verdict projection.

## [02]-[FABRIC]

- Owner: `WorkerKind` closes the worker family; a kind's standing obligations are its `KIND_POLICY` row — fidelity latch and restart class — so a new worker kind is one member and one row, never a sibling vocabulary. `Kernel` unifies the four crossing spellings the branch grew — a name-string gate, a by-reference module kernel, a page-local `_run_kernel`, a per-family `_dispatch` — into one frozen value: `Kernel.of` classifies the target once, and every downstream hop reads isolation, retry, deadline, enforcement, and wire off the value.
- Cases: `KernelTrait` answers the one isolation question per kernel family — `INLINE` runs on the loop (sub-quantum pure body), `PURE` wants its own GIL in-process (subinterpreter), `RELEASING` releases the GIL or blocks on a syscall (thread), `HOSTILE` holds process-global native state or a GIL-hostile extension (process), `SANDBOXED` runs a foreign guest module inside the in-process wasmtime sandbox (the wasm-bytes target shape answers it, never a declaration) — and `TRAIT_ROW` projects each trait onto its `WorkerKind` and default worker-death `RetryClass`. A consumer declares the trait per kernel family as domain knowledge; isolation, band, retry, and crossing mechanics are this owner's.
- Entry: `Kernel.of(target, trait, *, deadline, enforcement, wire, idempotent, retry)` is the one constructor, polymorphic on the target shape — the trait's seam classifies first, so a loop or thread kernel ships `LIVE` with its callable carried whole at zero serialization, a pickle-seam kernel classifies by picklability (a module-qualified callable ships `REFERENCE`; a closure, `<locals>` callable, or bound method ships `VALUE` as cloudpickle bytes, since a by-name walk loses the instance a `__self__` carries), a `(module, name)` pair is the native-gated form: the loop floor names a worker-floor kernel it must never import, `REFERENCE` by construction, and a wasm module crosses as its own bytes — `GUEST` by construction, the digest label its receipt and fault subject. `deadline` is the per-offload budget the lane folds against its own — the tighter bound wins — and `enforcement` selects the deadline arm: `COOPERATIVE` cancels the awaiting scope and leaves the worker to the pool's reaper, `TERMINAL` routes the hop through the pebble pool so the deadline kills the worker mid-kernel and reclaims the slot, the only bound a hung native call obeys.
- Auto: `fidelity()` latches `tblib.pickling_support.install()` once per interpreter — every exception crossing a pickle seam thereafter carries its worker-side frames, so the faults lift classifies the true cause instead of a flattened marker; the latch runs in every pool initializer whose `KIND_POLICY` row obliges it and inside `shipped`, so a spawned interpreter re-latches cold. This latch runs default-off locals capture, and the explicit `Traceback` carrier stays out — the pickle rail is the fabric's whole tblib surface, receipts carrying facts, never frames.
- Law: a worker-death retry re-runs the kernel whole, so the retry row binds only under the kernel's `idempotent` declaration — content-keyed inputs, run-scoped outputs, no external state — and `Kernel.of` drops the trait's retry default for a kernel declaring `idempotent=False`; the declaration gates the two live dispatch sites — `WorkerPool.submit` for the pooled kinds and the `execution/lanes#LANE` offload hop for the anyio arms — each reading `kernel.retry`, never a convention the call site remembers.
- Law: `Wire` closes the payload-crossing axis — `PICKLE` copies arguments across the seam, `SHARED_MEMORY` exports every top-level ndarray argument once into a named `multiprocessing.shared_memory` block and crosses it as a `ShmSpan` the worker re-views through `numpy.frombuffer` — so a heavy-buffer kernel upgrades its crossing by one field with the call site untouched. Block custody stays loop-side: `exported` copies and names, `released` closes and unlinks after the offload settles, and the worker view is ingress-only — a kernel consumes the view inside its body and returns owned material, because the worker handle closes when the kernel returns. A buffer wrapped inside a struct stays `PICKLE`; only a bare ndarray argument rides the span channel. Named blocks are the chosen out-of-band channel because cloudpickle's protocol-5 `buffer_callback` collects buffers the executor transports have no side channel to carry, where a named block crosses at zero payload bytes.
- Law: a native-gated worker module splits at the parse floor — the vocabulary module parses and imports on both interpreter floors while the worker-body module holds its eager native providers and loads only worker-side — and `REFERENCE` shipping resolves the kernel by qualified name through the one `shipped` gate; `covered(module, names)` is the worker-floor witness that same module runs at its own import, proving every dispatchable name resolves through the identical walk so a misspelled roster fails at worker import, never mid-offload. This gate is the canonical crossing law every native-gated consumer composes — `artifacts` scene rendering is the standing proof instance — never a page-local `getattr` gate re-spelled beside it. `cloudpickle.register_pickle_by_value` stays out: a worker floor that must import its module from disk is the stronger contract than shipping a module by value into an interpreter that drifts from it.
- Law: `traced_kernel` is the parented-emitter gate every crossing resolves through — the receipts pair resolves the carried W3C parent, the `worker.<name>` span opens under it so worker-interior evidence joins the one trace, the profiler `phase` window tags the flame by kernel subject and shipping form, and the two-read `Cost` bracket records the kernel's own process spend onto the `rasm.cost.<measure>` rows under the attached context, the promoted `rasm.tenant` entry pricing the kernel to the tenant that ran it; an uninstalled floor resolves no-op providers and a null profiler window, so the gate never conditions on install state and costs two process reads.
- Law: `remote_floor` is the fleet mirror of `shipped` — the far interpreter's module entry reads one sealed blob on stdin, resolves it through the same `sealed_kernel` gate, and writes one pickled `("value", T) | ("raise", BaseException)` verdict on stdout — so every `Shipping` form is total across the SSH channel (the seal cloudpickles the whole `Kernel`, a `LIVE` callable crossing by value, `REFERENCE` re-importing from the remote install, the worker-floor contract at fleet scale) and a kernel raise crosses home frame-whole under the latch `shipped` re-arms.
- Law: `_guest` is `GUEST` shipping's worker-floor arm — zero-import instantiation (no WASI, no ambient capability), a fresh `Store` per call so guest state never leaks across kernels, `GUEST_MEMORY` bounding linear memory, and request/reply crossing as bytes over the `GUEST_ABI` exports; the module compiles once per digest per interpreter, so the per-call cost is instantiation alone.
- Law: the guest deadline is the engine's epoch — one daemon pacer heartbeats the engine-global epoch every `EPOCH_TICK` while each store carries its own relative tick budget, so concurrent guests never kill each other and a guest dies mid-kernel at wall clock IN-PROCESS, the enforcement no thread or interpreter arm owns. `WasmtimeError` exposes no addressable trap code, so the arm discriminates by elapsed budget: an epoch kill re-raises `TimeoutError` onto the faults `deadline` row, and a genuine trap crosses whole into the catch-all `boundary` case with its trap message.
- Growth: a new worker kind is one `WorkerKind` member with one `KIND_POLICY` row; a new isolation answer is one `KernelTrait` member with one `TRAIT_ROW` row and every call site untouched; a new shipping form is one `Shipping` member with one `shipped` arm; a new enforcement arm is one `Enforcement` member with one offload projection row; a new payload crossing is one `Wire` member with one `exported` arm; a new cost measure is one `Cost` field at the receipts owner with one `INSTRUMENTS` row at the metrics owner, reaching this bracket through `measures` with zero gate edits.
- Boundary: trait declaration stays consumer domain knowledge — this owner never inspects a callable for GIL behavior; picklability is the one property `Kernel.of` classifies itself. Thread and subinterpreter crossing arms and the offload hop stay `execution/lanes#LANE`'s; this page mints the vocabulary the hop consumes, the process bands, and the process pools.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import hashlib
import math
import sys
import threading
import time
from collections.abc import Callable, Iterable
from enum import StrEnum
from functools import cache, lru_cache, reduce
from importlib import import_module
from multiprocessing.shared_memory import SharedMemory
from typing import Final, assert_never

import cloudpickle
from expression import Nothing, Option, Some
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace
from tblib import pickling_support

from rasm.runtime.faults import SCOPES, RuntimeRail, Scope, boundary
from rasm.runtime.metrics import Metrics
from rasm.runtime.profiles import Profiles
from rasm.runtime.receipts import Cost, Signals
from rasm.runtime.resilience import RetryClass

lazy import numpy  # shared-memory reconstruction alone touches it; the wire stays dark for PICKLE-only processes
lazy import wasmtime  # the guest arm alone touches it; an interpreter running no sandboxed kernel stays dark

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

GUEST_ABI: Final[tuple[str, str, str]] = ("memory", "alloc", "run")  # canonical guest exports: linear memory, request allocator, entry point
GUEST_MEMORY: Final[int] = 1 << 28  # per-call guest linear-memory ceiling; Store.set_limits refuses growth past it
GUEST_MODULES: Final[int] = 8  # compiled-module cache bound: caller-controlled wasm payloads evict LRU, never accrete per digest forever
EPOCH_TICK: Final[float] = 0.1  # pacer quantum and guest-deadline resolution; a budget rounds up to whole ticks
UNBOUNDED_TICKS: Final[int] = 1 << 62  # a deadline-free guest still carries a tick bound, so the shared pacer never kills it
VERDICT_FRAME: Final[int] = 1 << 30  # remote verdict payload ceiling; the 8-byte length header validates against it BEFORE any buffering

# --- [MODELS] ---------------------------------------------------------------------------


class TraitRow(Struct, frozen=True):
    # `Nothing` kind runs on the loop; `retry` is the worker-death default the crossing binds when the caller supplies none.
    kind: Option[WorkerKind]
    retry: Option[RetryClass]


class KindPolicy(Struct, frozen=True, gc=False):
    fidelity: bool  # pickle seam: the tblib latch rides the pool initializer where True; THREAD shares memory, DAEMON is spawned not called
    restart: RetryClass


class ShmSpan(Struct, frozen=True, gc=False):
    # one crossed ndarray: named block plus the dtype/shape rebuild recipe; the exporter owns unlink, the worker view is ingress-only.
    name: str
    dtype: str
    shape: tuple[int, ...]


class Kernel[T](Struct, frozen=True):
    # one crossing value: `live` carries LIVE shipping's callable (loop/thread arms, no pickle seam), `name` resolves
    # REFERENCE shipping worker-side, `payload` carries VALUE shipping bytes or the GUEST wasm module.
    trait: KernelTrait
    name: str
    module: str
    shipping: Shipping
    payload: bytes = b""
    live: Callable[..., T] | None = None
    deadline: Option[float] = Nothing
    enforcement: Enforcement = Enforcement.COOPERATIVE
    wire: Wire = Wire.PICKLE
    idempotent: bool = True  # a worker-death retry re-runs the kernel whole ONLY under this declaration
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
        trait = KernelTrait.SANDBOXED if isinstance(target, bytes | bytearray) else trait  # the wasm-bytes target shape owns the sandbox answer
        if trait is KernelTrait.SANDBOXED and not isinstance(target, bytes | bytearray):
            # target shape owns the answer both ways: a SANDBOXED declaration over a callable or name pair is a mis-declared
            # trait refused at construction, never a kernel the non-seam arm ships LIVE onto the wasm band.
            raise TypeError(f"workers.kernel.{getattr(target, '__qualname__', target)}: sandboxed-trait-requires-wasm-bytes")
        row = TRAIT_ROW[trait]
        seam = row.kind.map(lambda kind: KIND_POLICY[kind].fidelity).default_value(False)  # fidelity marks exactly the pickle-seam kinds
        match target:
            case bytes() | bytearray() as guest:
                # GUEST form: the wasm module IS the payload; the digest label is the subject receipts and faults carry for it.
                name, module = hashlib.sha256(guest).hexdigest()[:16], ""
                shipping, payload, live = Shipping.GUEST, bytes(guest), None
            case (str() as module, str() as name):
                # native-gated form: the parse floor names a worker-floor kernel it never imports; `covered` proves the roster worker-side.
                shipping, payload, live = Shipping.REFERENCE, b"", None
            case fn if not seam:
                # loop and thread arms cross no pickle seam: the callable rides LIVE at zero serialization, and a TERMINAL
                # re-route stays total because the pebble arm cloudpickle-seals the whole payload.
                name = getattr(fn, "__qualname__", "<lambda>")
                module = getattr(fn, "__module__", "")
                shipping, payload, live = Shipping.LIVE, b"", fn
            case fn:
                # `<lambda>`/`<locals>` qualnames and bound methods (`__self__` set) mark the callables a by-name resolution
                # loses or mis-resolves; cloudpickle ships each by value.
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


# --- [OPERATIONS] -----------------------------------------------------------------------


@cache
def fidelity() -> bool:
    # process-global one-shot: every exception pickled after this carries its traceback; idempotent under @cache per interpreter.
    pickling_support.install()
    return True


def shipped[T](kernel: Kernel[T], *args: object) -> T:
    # worker-floor rehydration gate — the ONE spelling every crossing resolves through; runs on the far interpreter,
    # so the latch re-arms cold and REFERENCE resolves by import exactly as pickle-by-reference re-imports a module.
    fidelity()
    match kernel.shipping:
        case Shipping.LIVE:
            # a LIVE slot stripped by an unexpected seam self-heals through the same by-name walk REFERENCE runs.
            fn = kernel.live if kernel.live is not None else reduce(getattr, kernel.name.split("."), import_module(kernel.module))
        case Shipping.REFERENCE:
            # dotted-qualname walk resolves nested owners (a classmethod kernel) the flat getattr cannot.
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
        for value in args:  # Exemption: incremental attach keeps a mid-attach raise from orphaning the earlier span handles.
            view, handle = _attached(value)
            views.append(view)
            if handle is not None:
                handles.append(handle)
        # egress copy-fence: the ingress-only contract is enforced, never trusted — a bare ndarray result aliasing a span
        # block detaches into owned material before the handles below close its backing buffer; nested containment
        # stays the kernel's idempotent-material contract, mirroring _spanned's bare-ndarray wire law.
        return _detached(fn(*views), views) if handles else fn(*views)
    finally:
        views.clear()  # Exemption: span views drop before close, so the buffer release finds no exported pointer to refuse on.
        for handle in handles:  # Exemption: the worker-side span handles close when the kernel returns.
            handle.close()


def traced_kernel[T](carrier: dict[str, str], kernel: Kernel[T], *args: object) -> T:
    # worker-side half of the offload stitch, the parented-emitter gate: the receipts pair — pure extract, token-paired
    # attach scope — resolves the carried W3C parent, the kernel span opens under it so worker-interior evidence joins
    # the one trace, the profiler `phase` window tags the flame by kernel subject, and the two-read `Cost` bracket
    # records the kernel's own process spend onto the `rasm.cost.<measure>` rows under the attached context — the
    # `rasm.tenant` baggage the carrier promotes prices the kernel to the tenant that ran it. An uninstalled floor
    # resolves no-op providers and a null profiler window, so the gate costs exactly two process reads.
    with Signals.attach(Signals.continue_inbound(carrier)):
        span = trace.get_tracer(SCOPES[Scope.SERVICE]).start_span(f"worker.{kernel.name}")
        with trace.use_span(span, end_on_exit=True), Profiles.phase({"kernel": kernel.name, "shipping": kernel.shipping.value}):
            before = Cost.own()
            try:
                return shipped(kernel, *args)
            finally:  # Exemption: the bracket's terminal read pairs with the entry read — two process reads per crossing, fault arm included.
                Metrics.record(Cost.own().delta(before).measures(), domain="cost", kind=kernel.name)


def sealed_kernel[T](blob: bytes) -> T:
    # pebble's stdlib pickler sees one bytes argument: the cloudpickle seal makes the TERMINAL arm total over
    # closure-bearing arguments at one extra serialization pass, so no payload shape is unspellable under a kill
    # deadline. A carried boot lands first — the remote floor's whole install rides the seal — while a pooled seal
    # carries None because the pool initializer already booted the worker.
    boot, carrier, kernel, args = cloudpickle.loads(blob)
    if boot is not None:
        _worker_boot(boot)
    return traced_kernel(carrier, kernel, *args)


def remote_floor() -> int:
    # fleet worker-floor entry the remote arm's session command runs: one sealed blob on stdin, one FRAMED pickled
    # verdict on stdout — an 8-byte big-endian length header ahead of the payload, so the parent validates the frame
    # against VERDICT_FRAME before buffering a byte; `shipped` re-arms the tblib latch before the kernel body, so the
    # raise arm crosses home frame-whole. stdout is the verdict channel ALONE — the kernel runs with stdout re-pointed
    # at stderr so a stray print inside a shipped body never corrupts the binary frame. The seal-carried boot installs
    # the floor's telemetry, and interpreter exit runs the boot-registered atexit drain AFTER the verdict frame lands,
    # so the short-lived floor exports completely without delaying the parent's read.
    channel, sys.stdout = sys.stdout, sys.stderr
    try:
        verdict: tuple[str, object] = ("value", sealed_kernel(sys.stdin.buffer.read()))
    except BaseException as raised:  # Exemption: the floor's terminal fence — every raise crosses home as the pickled verdict, never a lost exit code.
        verdict = ("raise", raised)
    payload = cloudpickle.dumps(verdict)
    channel.buffer.write(len(payload).to_bytes(8, "big") + payload)
    return 0


def covered(module: str, names: Iterable[str]) -> RuntimeRail[int]:
    # worker-floor import-time witness: every dispatchable name resolves through the same walk `shipped` runs,
    # so a misspelled roster fails at worker import, never mid-offload.
    return boundary(module, lambda: sum(1 for name in names if reduce(getattr, name.split("."), import_module(module)) is not None))


def exported(wire: Wire, args: tuple[object, ...]) -> tuple[tuple[object, ...], tuple[SharedMemory, ...]]:
    # loop-side span export: each top-level ndarray copies once into a named block and travels as its ShmSpan; every other
    # argument passes through, so the two wires share one call shape and PICKLE pays no probe.
    if wire is Wire.PICKLE:
        return args, ()
    crossed: list[object] = []
    blocks: list[SharedMemory] = []
    try:
        for value in args:  # Exemption: the allocation loop is the export seam; a mid-export raise releases the partial set below.
            view, block = _spanned(value)
            crossed.append(view)
            if block is not None:
                blocks.append(block)
    except BaseException:
        released(tuple(blocks))
        raise
    return tuple(crossed), tuple(blocks)


def released(blocks: tuple[SharedMemory, ...]) -> None:
    # exporter-owned unlink — safe under an abandoned settle because an already-attached mapping outlives the name; a worker
    # still queued when a cancelled offload unlinks attaches a dead name and raises, confined to the abandoned job no one reads.
    failures: list[Exception] = []
    for block in blocks:  # Exemption: the unlink walk is the exporter's teardown seam; one refused step never orphans the sibling blocks.
        for step in (block.close, block.unlink):
            try:
                step()
            except (OSError, BufferError) as refused:
                # BufferError joins OSError: close() refuses while a stray exported view still pins the mapping, and
                # that refusal must not strand the sibling blocks' unlink — both collect into the aggregate.
                failures.append(refused)
    if failures:
        raise ExceptionGroup("workers.released", failures)


def _spanned(value: object) -> tuple[object, SharedMemory | None]:
    match value:
        # a zero-byte array pickles through — SharedMemory(create=True, size=0) refuses — and an object-dtype array pickles
        # through too: its buffer holds process-local pointers no foreign mapping can honor.
        case numpy.ndarray() as array if array.nbytes and not array.dtype.hasobject:
            block = SharedMemory(create=True, size=array.nbytes)
            try:
                numpy.frombuffer(block.buf, dtype=array.dtype)[:] = array.reshape(-1)
            except BaseException:
                # a mid-copy raise (an unmappable dtype form, a layout refusal) lands BEFORE the caller holds the
                # block, so `exported`'s partial-set release can never reach it — this seam closes and unlinks its
                # own just-minted block, then the raise crosses whole.
                block.close()
                block.unlink()
                raise
            return ShmSpan(name=block.name, dtype=str(array.dtype), shape=tuple(array.shape)), block
        case passthrough:
            return passthrough, None


def _attached(value: object) -> tuple[object, SharedMemory | None]:
    match value:
        case ShmSpan(name=name, dtype=dtype, shape=shape):
            # track=False keeps the worker-side resource tracker off a block the exporter alone unlinks.
            block = SharedMemory(name=name, track=False)
            return numpy.frombuffer(block.buf, dtype=dtype).reshape(shape), block
        case passthrough:
            return passthrough, None


def _detached(result: object, views: list[object]) -> object:
    match result:
        # a kernel returning a view into an attached span crosses a buffer whose handle closes at return; the copy
        # detaches it into caller-owned storage, and an owned array passes through untouched.
        case numpy.ndarray() as array if any(isinstance(view, numpy.ndarray) and numpy.may_share_memory(array, view) for view in views):
            return array.copy()
        case passthrough:
            return passthrough


def _paced(engine: "wasmtime.Engine") -> None:
    while True:  # Exemption: the epoch pacer is the guest fabric's standing heartbeat, a daemon thread ending with the interpreter.
        time.sleep(EPOCH_TICK)
        engine.increment_epoch()


@cache
def _guest_engine() -> "wasmtime.Engine":
    # one engine per interpreter: epoch interruption arms once, and the daemon pacer increments the engine-global epoch
    # every EPOCH_TICK — each store's RELATIVE tick deadline therefore isolates per call under one shared heartbeat.
    config = wasmtime.Config()
    config.epoch_interruption = True
    engine = wasmtime.Engine(config)
    threading.Thread(target=_paced, args=(engine,), daemon=True, name="rasm-guest-pacer").start()
    return engine


@lru_cache(maxsize=GUEST_MODULES)
def _guest_module(payload: bytes) -> "wasmtime.Module":
    # compile once per module bytes per interpreter, bounded: the payload is caller-controlled, so an unbounded memo
    # would retain every distinct wasm blob (and its compiled machine code) for the interpreter's life — the LRU
    # keeps the hot working set's compilation reuse and evicts the cold tail; instantiation stays per call, so guest
    # state never leaks across kernels.
    return wasmtime.Module(_guest_engine(), payload)


def _guest[T](kernel: Kernel[T]) -> Callable[..., T]:
    # GUEST shipping's worker-floor arm: zero-import instantiation — no WASI, no ambient capability — a fresh Store per
    # call, GUEST_MEMORY bounding linear memory, and the store's relative epoch budget as the in-process wall-clock kill.
    def run(request: bytes = b"") -> T:
        engine, started = _guest_engine(), time.monotonic()
        store = wasmtime.Store(engine)
        store.set_limits(memory_size=GUEST_MEMORY)
        store.set_epoch_deadline(kernel.deadline.map(lambda budget: max(1, math.ceil(budget / EPOCH_TICK))).default_value(UNBOUNDED_TICKS))
        exports = wasmtime.Instance(store, _guest_module(kernel.payload), []).exports(store)
        memory, alloc, entry = (exports[name] for name in GUEST_ABI)
        pointer = alloc(store, len(request))
        memory.write(store, request, pointer)
        try:
            packed = entry(store, pointer, len(request))
        except wasmtime.WasmtimeError as trapped:
            # WasmtimeError exposes no addressable trap code, so elapsed budget discriminates: an epoch kill re-raises
            # TimeoutError onto the deadline row, and a genuine trap crosses whole with its message.
            if kernel.deadline.map(lambda budget: time.monotonic() - started >= budget).default_value(False):
                raise TimeoutError(f"guest.{kernel.name}:epoch-kill") from trapped
            raise
        head, span = packed >> 32, packed & 0xFFFFFFFF  # entry packs (ptr << 32) | len; reply copies out before the store drops
        return bytes(memory.read(store, head, head + span))

    return run


# --- [TABLES] ---------------------------------------------------------------------------

# trait -> (worker kind, worker-death retry default): the one place the isolation question is answered — PURE rides the anyio
# subinterpreter arm (OCCT: the anyio death pair), HOSTILE rides the pooled process arms (WORKER: the loky/pebble death names),
# SANDBOXED rides the thread band with the guest's own epoch kill, its retry Nothing because a trap is deterministic.
TRAIT_ROW: Final[Map[KernelTrait, TraitRow]] = Map.of_seq([
    (KernelTrait.INLINE, TraitRow(kind=Nothing, retry=Nothing)),
    (KernelTrait.PURE, TraitRow(kind=Some(WorkerKind.INTERPRETER), retry=Some(RetryClass.OCCT))),
    (KernelTrait.RELEASING, TraitRow(kind=Some(WorkerKind.THREAD), retry=Nothing)),
    (KernelTrait.HOSTILE, TraitRow(kind=Some(WorkerKind.PROCESS), retry=Some(RetryClass.WORKER))),
    (KernelTrait.SANDBOXED, TraitRow(kind=Some(WorkerKind.WASM), retry=Nothing)),
])

# per-kind standing obligations: THREAD shares the address space so fidelity is structural; GPU mirrors PROCESS — same pickle
# seam, same pool-death names; WASM crosses no pickle seam and holds no supervisable subject, its restart row the in-process
# transient band; DAEMON is spawned, not called, and its restart row targets spawn transients rather than pool deaths;
# REMOTE's fidelity marks the SSH pickle seam and its restart row targets channel transients — the channel, not an
# executor, is what dies at fleet scale.
KIND_POLICY: Final[Map[WorkerKind, KindPolicy]] = Map.of_seq([
    (WorkerKind.THREAD, KindPolicy(fidelity=False, restart=RetryClass.OCCT)),
    (WorkerKind.INTERPRETER, KindPolicy(fidelity=True, restart=RetryClass.OCCT)),
    (WorkerKind.PROCESS, KindPolicy(fidelity=True, restart=RetryClass.WORKER)),
    (WorkerKind.GPU, KindPolicy(fidelity=True, restart=RetryClass.WORKER)),
    (WorkerKind.WASM, KindPolicy(fidelity=False, restart=RetryClass.OCC_NATIVE)),
    (WorkerKind.DAEMON, KindPolicy(fidelity=False, restart=RetryClass.SPAWN)),
    (WorkerKind.REMOTE, KindPolicy(fidelity=True, restart=RetryClass.SSH)),
])
```

## [03]-[POOL]

- Owner: `WorkerPool` is the warm reusable pool capsule — one polymorphic surface over the process-executor, device, and fleet remote arms: `loky.get_reusable_executor` for `COOPERATIVE` (process-global warm pool, crash-respawning, cloudpickle payloads, idle reap on `timeout`), `pebble.ProcessPool` for `TERMINAL` (`schedule(timeout=)` kills a running worker at wall-clock and reclaims the slot; `max_tasks` recycles a worker after N tasks bounding RSS creep), one `asyncssh` channel per `WorkerKind.REMOTE` arm (a memoized `SSHClientConnection` off `transport/roots#RESOURCE` `RemoteEndpoint.dialed`, per-submit `create_process` sessions running `remote_floor`, a per-arm session limiter bounding fleet in-flight), and one instance-owned loky executor per `WorkerKind.GPU` device arm — the process-global singleton cannot hold per-device state, so device custody is pebble-style instance ownership, `env=` pinning the device before any worker module loads on the cooperative arm and the `_worker_boot` initializer pinning it on the terminal arm. Custody follows each substrate's own topology: the pebble pool is instance-owned, the loky singleton arm holds acquisition arguments and re-acquires through the factory per use because loky is process-global and a broken instance is replaced only by re-acquisition — a field pinning the executor pins the corpse — the remote arm re-dials a closed channel per use under the same law, and a broken device instance drops its corpse at the submit seam so the next acquisition mints fresh. Arm selection is `(WorkerKind, Enforcement, placement key)` — one derived key off the `Placement` shape, endpoint key, device key, or `""` local, never a caller-facing executor knob — and the subinterpreter and sandbox kinds ride no pool: the anyio arms are already own-GIL or in-process-killable substrates a package pool duplicates without adding respawn or kill capability.
- Entry: `WorkerPool.acquire(kind, enforcement, placement)` memoizes one live pool per arm key behind a membership guard — an effectful `setdefault` mint is the deleted form — so every call site shares the warm workers; `live` is the read-only accessor the supervisor and the drain fold consult without minting, `alive` reads the arm's own liveness (pebble `active`; the loky arms self-heal per acquisition; the remote arm reads `is_closed` off its dialed channel), and `pids` names a loky arm's live worker set for the arm-scoped probe. `submit(kernel, *args)` injects the trace carrier, drives the arm's executor with `traced_kernel`, and settles the future on a `WORKER_BAND`-bounded thread with `abandon_on_cancel=True` — the band token bounds pool in-flight and settle threads in one acquisition, and a cooperative cancel abandons the settle while the worker runs to completion under the pool's reaper, exactly the `COOPERATIVE` law; on the terminal arm the same cancel instead escalates through `ProcessFuture.cancel`, pebble terminating the RUNNING task so the killable slot reclaims immediately rather than holding to the wall-clock kill. In-band worker-death retry reads `kernel.retry` before the terminal lift, and each attempt re-acquires the executor, so a `TerminatedWorkerError` re-submission genuinely lands on the respawned pool and a non-idempotent kernel never re-runs.
- Law: the remote crossing seals `(boot, carrier, kernel, args)` as the one cloudpickle blob the TERMINAL arm already ships — stdin in, one pickled verdict out — and the hop honours `kernel.deadline` itself since no lane wraps a direct fleet submit: the deadline scope sits OUTSIDE the retry and rails the typed `deadline` fault when tripped, the in-flight session's cancellation cleanup escalating on the way out — `TERMINAL` kills the far process mid-kernel, `COOPERATIVE` terminates the session so the channel reaps the floor — while a torn or empty verdict raises `ConnectionError` into the `SSH` retry band. `boot` is the parent-captured worker floor and may be `None` only when the pebble initializer already installed it. In-band retry re-keys to `KIND_POLICY[REMOTE].restart` under the kernel's `idempotent` declaration — executor-death names are meaningless across a channel — and `Wire.SHARED_MEMORY` is refused as a typed `config` fault because a span name never resolves across hosts; silently downgrading a declared zero-copy crossing to a copy betrays the declaration.
- Auto: every process arm pins an explicit spawn-family start method — `get_context("spawn")` on pebble, the loky spawn-based `"loky"` method on both loky arms — so crossing semantics never fork by platform default; every process arm's initializer is the one `_worker_boot` — device visibility pins first, the fidelity latch re-arms per its `KIND_POLICY` row, and the parent-captured `WorkerBoot` installs telemetry, instruments, and the profiler post-spawn under `WORKER_SIGNAL_PROFILE` with the exit drain registered — while the capsule latches parent-side at construction, so a worker raise re-crosses with frames intact and the settle-side unpickle resolves them; every lifecycle transition self-emits its `PoolReceipt` — the phase methods through the `@receipted` harvest, the SPAWNED mint at `acquire`'s membership guard — so pool chronology rides the one receipt rail. A worker-local cache or native handle is a module-level `@cache` in the worker-body module, so the warm prime pays it once per spawned worker and a reaped worker's respawn re-pays it. Elasticity is the substrate's own pair — the idle reap shrinks a quiet pool and a later submit respawns to the cap — so a sizing knob never lands. Worker-death evidence is typed per arm — loky raises `TerminatedWorkerError`/`BrokenProcessPool` on the pending future, pebble raises `ProcessExpired` (pid, exitcode) or `TimeoutError` on the deadline kill — and the faults pool-death row lands each on the `resource` case while the deadline kill converts to the `deadline` case, never retried because the `WORKER` row's target excludes it; loky's exit-code forensics never land — re-acquisition replaces the corpse before a forensic read, so death evidence is the typed raise and pebble's pid/exitcode.
- Law: the worker flush law is exit-owned — `_worker_boot` registers the telemetry drain then the profiler stop through `atexit`, so a graceful settle (`shutdown(wait=True)`, pebble `close`/`join`, the remote floor's process exit) drains every worker's buffered tail, a roll's double-buffer drains the stale arm's workers the same way, and only the kill paths (`kill_workers`, pebble `stop`, the remote `abort`) forfeit at most one `WORKER_SIGNAL_PROFILE` export window. The boot is captured parent-side as data off `Telemetry.receipt`/`Profiles.receipt`, so a silent parent spawns silent workers, no endpoint knob rides the pool surface, and the worker geometry keeps the HTTP transport the fork fence requires — the gRPC egress row stays structurally refused on every spawned floor.
- Receipt: `PoolReceipt` carries `phase`, `kind`, `enforcement`, and `workers` — lifecycle evidence, never task outcomes; task outcomes stay the lane's `DrainReceipt`.
- Growth: a new executor arm is one constructor match arm keyed by `(WorkerKind, Enforcement)`; a new fleet host is one `RemoteEndpoint` value and a new accelerator one `Device` value, each acquiring its own arm at zero new surface; a new lifecycle phase is one `PoolPhase` member; a new warm-state obligation is one initializer fold; a new worker-boot fact is one `WorkerBoot` field the one initializer reads.
- Boundary: pools serve the lanes offload hop, the daemon drain fold, the fleet and device consumers, and the supervisor — a consumer never imports an executor class, holds a future, or sizes a pool; sizing derives from `loky.cpu_count(only_physical_cores=True)`, which already folds the `LOKY_MAX_CPU_COUNT` deploy override and the cgroup budget, so a cgroup-capped batch arm is deploy placement with zero new surface — the capped daemon's workers inherit its cgroup and every pool self-sizes to the quota, scheduling class and affinity riding the same deploy custody, never a kind — and `WORKER_BAND` bounds in-flight admission above the pool, refusing burst past physical cores. `REMOTE` and `GPU` are caller placement, never trait-derived — the lanes offload never routes to them, and a consumer acquires the arm with its `RemoteEndpoint` or `Device` exactly as trait declaration is consumer domain knowledge on the fabric; priority is the same placement axis — a latency class acquires its own arm key, never a queue-discipline knob. Fan-out modality stays the lane's `drain` — the pools expose `submit` alone, never a second `map`, stream, or priority surface. `Kernel.of` is the whole payload-classification surface and `ShmSpan` the one out-of-band buffer channel, so no per-object wrap, pickler swap, or reducer registration exists beside them.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import atexit
import os
from collections.abc import Iterable
from concurrent.futures import Future
from functools import partial
from multiprocessing import get_context
from typing import Final, Literal, assert_never
from uuid import uuid4

import anyio
import anyio.to_thread
import asyncssh
import loky
import loky.backend.context
import pebble
from anyio import CapacityLimiter, move_on_after
from expression import Error, Nothing, Option, Result, Some
from opentelemetry import propagate
from opentelemetry.exporter.otlp.proto.http import Compression
from opentelemetry.sdk.resources import SERVICE_INSTANCE_ID, SERVICE_NAME, SERVICE_NAMESPACE, Resource

from rasm.runtime.admission import RuntimeProfile
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.receipts import OPEN, Receipt, Signals, receipted
from rasm.runtime.resilience import guard
from rasm.runtime.roots import RemoteEndpoint
from rasm.runtime.telemetry import NAMESPACE, SCHEMA_URL, SignalProfile, Telemetry

# every FABRIC-region owner — the kind/trait/shipping/wire vocabulary, `Kernel`, the policy tables, the crossing gates, and their
# imports (`cloudpickle`, `StrEnum`, `Struct` included) — resolves in this same module; one module, three regions.

# --- [TYPES] ----------------------------------------------------------------------------


class PoolPhase(StrEnum):
    SPAWNED = "spawned"
    WARM = "warm"
    DRAINING = "draining"
    RETIRED = "retired"


# pool arms exist for the process, device, and remote kinds alone: THREAD, INTERPRETER, and WASM ride the anyio arms, DAEMON
# rides supervised children; the arm key's third part is the placement key — endpoint, device, or "" on plain local arms.
type PoolKind = Literal[WorkerKind.PROCESS, WorkerKind.GPU, WorkerKind.REMOTE]
type ArmKey = tuple[PoolKind, Enforcement, str]
type Placement = RemoteEndpoint | Device | None

# --- [CONSTANTS] ------------------------------------------------------------------------

# process-wide worker band: bounds every pooled submission AND its settle thread through the one to_thread acquisition,
# so concurrent process crossings never oversubscribe the host against each package's internal thread pool.
WORKER_BAND: Final[CapacityLimiter] = CapacityLimiter(loky.cpu_count(only_physical_cores=True))

# fleet floor entry the remote arm's session command appends to the endpoint's interpreter; the far install owns the
# module — the REFERENCE worker-floor contract at fleet scale.
REMOTE_FLOOR: Final[str] = "-m rasm.runtime.workers"

# worker-shaped egress geometry: small queues and a short interval so kernel-grain evidence exports continuously and
# the atexit drain carries only a tail window; the HTTP transport default IS the fork fence — the gRPC row never
# rides a spawned or forked floor.
WORKER_SIGNAL_PROFILE: Final[SignalProfile] = SignalProfile(
    export_interval_ms=5000, schedule_delay_ms=1000, max_queue_size=512, max_export_batch_size=128, compression=Compression.Gzip
)

# --- [MODELS] ---------------------------------------------------------------------------


class Device(Struct, frozen=True, gc=False):
    # one accelerator placement: `selector` names the runtime's device-visibility variable, `index` the ordinal it pins;
    # device binding crosses at spawn, so a worker's device is fixed before any native runtime reads the variable.
    index: int
    selector: str = "CUDA_VISIBLE_DEVICES"

    @property
    def key(self) -> str:
        return f"{self.selector}:{self.index}"

    @property
    def env(self) -> dict[str, str]:
        return {self.selector: str(self.index)}


class WorkerBoot(Struct, frozen=True):
    # the parent's EFFECTIVE install captured as data off `Telemetry.receipt`/`Profiles.receipt`: a silent parent
    # captures no endpoint and spawns silent workers, so no emission knob rides the pool surface and the worker's own
    # install re-evaluates the same profile gate the parent passed. `device` folds accelerator pinning into the one
    # initializer, so visibility lands before any native runtime import reads it.
    kind: WorkerKind
    profile: RuntimeProfile
    otel: str | None = None
    pyroscope: str | None = None
    tenant: str | None = None
    device: Device | None = None

    @staticmethod
    def captured(kind: WorkerKind, device: Device | None = None) -> "WorkerBoot":
        telemetry = Telemetry.receipt()
        profiles = Profiles.receipt()
        return WorkerBoot(
            kind=kind,
            profile=telemetry.map(lambda held: held.profile).default_value(RuntimeProfile.PACKAGE),
            otel=telemetry.map(lambda held: held.endpoint).to_optional(),
            pyroscope=profiles.map(lambda held: held.endpoint).to_optional(),
            tenant=profiles.bind(lambda held: Option.of_optional(held.tenant)).to_optional(),
            device=device,
        )

    def env(self) -> dict[str, str]:
        # daemon-spawn seam: a supervised child reads the standard SDK variable at its own admission, so the parent's
        # effective endpoint crosses by environment beside the device visibility, never a re-plumbed setting.
        return {**({"OTEL_EXPORTER_OTLP_ENDPOINT": self.otel} if self.otel else {}), **(self.device.env if self.device is not None else {})}


class PoolReceipt(Struct, frozen=True, gc=False):
    phase: PoolPhase
    kind: WorkerKind
    enforcement: Enforcement
    workers: int

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = {"kind": self.kind.value, "enforcement": self.enforcement.value, "workers": self.workers}
        return (Receipt.of("workers", ("emitted", self.phase.value, facts)),)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _placed(placement: Placement) -> str:
    return placement.key if placement is not None else ""


def worker_resource(kind: WorkerKind) -> Resource:
    # hand-built worker identity: a spawned worker is its own emitter, so a per-process instance id keys every worker
    # distinctly and the worker axes ride beside the service triple — no detector carries worker semantics.
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
    # the ONE initializer every process arm runs post-spawn: device visibility pins first (pebble ships no env= slot,
    # and a native runtime reads the variable at its own first initialization), the tblib latch re-arms per the kind's
    # KIND_POLICY row, and the parent-captured install lands fresh — a spawned or forked worker inherits no live batch
    # thread, so the pipeline mints in-process under WORKER_SIGNAL_PROFILE with the exit drain registered LIFO:
    # interpreter exit stops the profiler push first, then drains telemetry last, mirroring the daemon drain order.
    if boot.device is not None:
        os.environ.update(boot.device.env)
    if KIND_POLICY[boot.kind].fidelity:
        fidelity()
    if boot.otel is not None:
        atexit.register(Telemetry.shutdown)
        Telemetry.install(boot.profile, boot.otel, resource=worker_resource(boot.kind), signal_profile=WORKER_SIGNAL_PROFILE)
        Metrics.install()
    if boot.pyroscope is not None:
        atexit.register(Profiles.shutdown)
        Profiles.install(boot.profile, boot.pyroscope, tags={"worker.kind": boot.kind.value}, tenant=boot.tenant)


# --- [SERVICES] -------------------------------------------------------------------------


class WorkerPool:
    # one live capsule per arm key, module-registry memoized; `_live` is the operation-local mutable registry this owner reads and retires.
    _live: Final[dict[ArmKey, "WorkerPool"]] = {}

    def __init__(self, kind: PoolKind, enforcement: Enforcement, workers: int, placement: Placement = None) -> None:
        self._kind, self._enforcement, self._workers, self._placement = kind, enforcement, workers, placement
        self._key: ArmKey = (kind, enforcement, _placed(placement))
        self._phase = PoolPhase.SPAWNED
        # the one initializer payload: parent-effective install + device pinning + fidelity obligation captured as data.
        self._boot = WorkerBoot.captured(kind, placement if isinstance(placement, Device) else None)
        if KIND_POLICY[kind].fidelity:
            fidelity()  # parent-side latch: the settle-side unpickle resolves tblib reducers before the first worker raise crosses back
        match kind, enforcement, placement:
            case (WorkerKind.GPU, _, placed) if not isinstance(placed, Device):
                # placement admission: a GPU arm without a Device would fall through to the generic process arms —
                # keyed "" beside the plain local arm, pinning no device — so the mis-placement refuses at
                # construction exactly as Kernel.of refuses a mis-declared trait.
                raise TypeError(f"workers.pool.{kind.value}: gpu-requires-device-placement")
            case (WorkerKind.REMOTE, _, placed) if not isinstance(placed, RemoteEndpoint):
                # symmetric admission for the fleet arm: an endpoint-less REMOTE capsule could memoize, dial
                # nothing, and rail only at first submit — refused here instead, before it ever registers.
                raise TypeError(f"workers.pool.{kind.value}: remote-requires-endpoint-placement")
            case (WorkerKind.REMOTE, _, _):
                # remote arm: no executor — one lazily dialed channel behind a dial lock, per-submit sessions, a per-arm
                # limiter bounding fleet in-flight.
                self._conn: asyncssh.SSHClientConnection | None = None
                self._dial = anyio.Lock()
                self._sessions = CapacityLimiter(workers)
            case (WorkerKind.GPU, Enforcement.COOPERATIVE, Device()):
                self._loky: loky.ProcessPoolExecutor | None = None
                self._executor()  # instance mints here; env= applies the device binding before any worker module loads
            case (_, Enforcement.TERMINAL, _):
                self._pebble = pebble.ProcessPool(
                    max_workers=workers, max_tasks=64, initializer=partial(_worker_boot, self._boot), context=get_context("spawn")
                )
            case (_, Enforcement.COOPERATIVE, _):
                self._held: loky.ProcessPoolExecutor | None = None  # observation snapshot for pids(); never a submit target
                self._executor()  # loky is process-global: the capsule holds acquisition ARGS, never the instance — see _executor
            case _ as unmatched:
                assert_never(unmatched)

    def _executor(self, kill_workers: bool = False) -> loky.ProcessPoolExecutor:
        # per-call acquisition is the respawn seam: reuse='auto' returns the healthy process-global singleton and replaces a
        # broken one, so the in-band worker-death retry lands on a fresh pool instead of the dead instance a field would pin;
        # timeout reaps idle workers, the 'loky' context pins spawn semantics so crossing behavior never forks by platform
        # default. The device arm is the instance mirror: the singleton cannot hold per-device env, so the capsule owns one
        # standalone executor whose corpse `submit` drops on a break — kill_workers or a dropped corpse re-mints fresh here.
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
        if key not in cls._live:  # membership guard precedes the mint — an effectful setdefault spawns an executor per call
            cls._live[key] = cls(kind, enforcement, loky.cpu_count(only_physical_cores=True), placement)
            Signals.emit(PoolReceipt(phase=PoolPhase.SPAWNED, kind=kind, enforcement=enforcement, workers=0), OPEN)
        return cls._live[key]

    @classmethod
    def live(cls, kind: PoolKind, enforcement: Enforcement = Enforcement.COOPERATIVE, placement: str = "") -> "Option[WorkerPool]":
        return Option.of_optional(cls._live.get((kind, enforcement, placement)))

    def alive(self) -> bool:
        # pebble publishes `active`; the loky arms self-heal per acquisition, so their capsules are live while registered; the
        # remote arm reads channel state — un-dialed is live, a closed channel reads DEAD so the supervisor rolls a fresh dial.
        if self._kind is WorkerKind.REMOTE:
            return self._conn is None or not self._conn.is_closed()
        return self._pebble.active if self._enforcement is Enforcement.TERMINAL else True

    def pids(self) -> frozenset[int] | None:
        # both loky arms publish the live {pid: worker} map; pebble exposes none, so the terminal arm probes by complement;
        # remote workers are far-host processes no local pid names. The read is observation-only: _executor() is the
        # acquisition seam that mints or replaces a pool as a side effect, so the probe reads the instance the last
        # acquisition landed and an unbuilt arm names no workers.
        cooperative_local = self._enforcement is Enforcement.COOPERATIVE and self._kind is not WorkerKind.REMOTE
        held = (self._loky if isinstance(self._placement, Device) else self._held) if cooperative_local else None
        return frozenset(held._processes) if held is not None else None

    @classmethod
    async def roll(cls, kind: PoolKind, enforcement: Enforcement = Enforcement.COOPERATIVE, placement: Placement = None) -> "PoolReceipt":
        # arm-aware roll: loky's singleton roll is one kill_workers re-acquisition swapping the process-global instance in
        # place; pebble, the device arms, and the remote arm genuinely double-buffer — the fresh arm warms before the stale
        # one retires, so capacity never gaps and in-flight remote sessions finish on the stale channel.
        stale = cls._live.pop((kind, enforcement, _placed(placement)), None)
        if stale is not None and enforcement is Enforcement.COOPERATIVE and kind is WorkerKind.PROCESS:
            await anyio.to_thread.run_sync(lambda: stale._executor(kill_workers=True), abandon_on_cancel=True, limiter=WORKER_BAND)
        receipt = await cls.acquire(kind, enforcement, placement).warm()
        if stale is not None and (enforcement is Enforcement.TERMINAL or kind in (WorkerKind.GPU, WorkerKind.REMOTE)):
            await stale.drain()  # graceful double-buffer: in-flight work settles on the stale arm while the fresh arm already serves
        return receipt

    async def _connection(self, endpoint: RemoteEndpoint) -> asyncssh.SSHClientConnection:
        # per-use re-dial is the remote respawn seam — the loky re-acquisition law over a channel: a closed connection is
        # replaced on the next submit, never a pinned corpse, and the SSH retry row re-drives a dial lost mid-flight; the
        # dial lock single-flights the mint, so a concurrent warm fan opens ONE channel, never N leaked siblings.
        async with self._dial:
            if self._conn is None or self._conn.is_closed():
                self._conn = await endpoint.dialed()
            return self._conn

    async def _remote[T](self, carrier: dict[str, str], kernel: Kernel[T], args: tuple[object, ...]) -> RuntimeRail[T]:
        match self._placement, kernel.wire:
            case (RemoteEndpoint(), Wire.SHARED_MEMORY):
                # a span name never resolves across hosts — the shm channel is host-local by construction, refused loudly
                # rather than silently downgrading a declared zero-copy crossing to a copy.
                return Error(BoundaryFault(config=(f"workers.remote.{kernel.name}", "shared-memory-wire-is-host-local")))
            case (RemoteEndpoint() as endpoint, _):
                pass
            case _:
                return Error(BoundaryFault(config=(f"workers.remote.{kernel.name}", "remote-arm-without-endpoint")))

        async def crossing(blob: bytes) -> T:
            conn = await self._connection(endpoint)
            async with self._sessions:
                # stderr discards at the session: the floor re-points kernel stdout onto stderr, so a chatty kernel would
                # otherwise fill the unread stderr pipe, block the far write, and starve the verdict read below.
                async with await conn.create_process(f"{endpoint.python} {REMOTE_FLOOR}", encoding=None, stderr=asyncssh.DEVNULL) as process:
                    try:
                        process.stdin.write(blob)
                        process.stdin.write_eof()
                        # framed verdict read: the 8-byte header validates against VERDICT_FRAME BEFORE any payload
                        # buffering, so a torn, hostile, or runaway far stream can never balloon parent memory, and
                        # readexactly's EOFError (a dead floor, an empty stream) converts to the channel fault below.
                        span = int.from_bytes(await process.stdout.readexactly(8), "big")
                        if not 0 < span <= VERDICT_FRAME:
                            raise ConnectionError(f"workers.remote.{kernel.name}:frame:{span}")
                        payload = await process.stdout.readexactly(span)
                    except anyio.get_cancelled_exc_class():
                        # deadline trip: TERMINAL kills the far process mid-kernel, COOPERATIVE ends the session so the
                        # channel HUP reaps the floor — then the cancellation re-raises, so the outer scope, never the
                        # SSH retry band, owns the deadline verdict.
                        (process.kill if kernel.enforcement is Enforcement.TERMINAL else process.terminate)()
                        raise
                    except EOFError as torn:  # asyncssh readexactly signals a short stream as IncompleteReadError(EOFError)
                        raise ConnectionError(f"workers.remote.{kernel.name}:exit={process.exit_status}") from torn
            match cloudpickle.loads(payload):
                case ("value", value):
                    return value
                case ("raise", BaseException() as raised):
                    raise raised  # frame-whole under the floor-side tblib latch — the faults lift classifies the true cause
                case _:
                    raise ConnectionError(f"workers.remote.{kernel.name}:torn-verdict")

        # channel transients, not executor deaths, are the remote in-band retry — re-keyed to the kind's restart row and
        # gated on the kernel's idempotent declaration exactly as the local arms gate on kernel.retry; the deadline scope
        # sits OUTSIDE the retry, so a tripped budget cancels the attempt chain and rails typed instead of re-arming it.
        keyed = Some(KIND_POLICY[WorkerKind.REMOTE].restart) if kernel.idempotent else Nothing
        with move_on_after(kernel.deadline.default_value(float("inf"))):
            # seal crosses the worker band INSIDE the deadline scope and its own boundary fence: an unpicklable
            # argument rails the typed fault instead of raising raw out of submit, an oversized payload's dumps never
            # blocks the loop, and sealing ONCE ahead of the retry keeps a re-driven attempt from re-paying it.
            sealed = await async_boundary(
                "workers.remote",
                lambda: anyio.to_thread.run_sync(
                    # the remote seal carries the boot: the floor is a fresh process per submit, so its whole install
                    # rides the blob and the atexit drain flushes the short-lived floor after the verdict lands.
                    lambda: cloudpickle.dumps((self._boot, carrier, kernel, args)), abandon_on_cancel=True, limiter=WORKER_BAND
                ),
            )
            match sealed:
                case Result(tag="error") as refused:
                    return refused
                case Result(tag="ok", ok=blob):
                    run = partial(crossing, blob)
                    return await async_boundary("workers.remote", lambda: keyed.map(lambda cls: guard(cls)(run)).default_with(run))
                case _ as unreachable:
                    assert_never(unreachable)
        return Error(BoundaryFault(deadline=(f"workers.remote.{kernel.name}", kernel.deadline.default_value(0.0), "remote-kill")))

    async def submit[T](self, kernel: Kernel[T], *args: object) -> RuntimeRail[T]:
        if self._phase in (PoolPhase.DRAINING, PoolPhase.RETIRED):
            # admission fence: a draining or retired arm refuses new work, so the remote re-dial never resurrects a closed channel
            # and a post-drain submission rails typed instead of racing the teardown.
            return Error(BoundaryFault(config=(f"workers.{self._kind.value}.{kernel.name}", f"pool-{self._phase.value}")))
        carrier: dict[str, str] = {}
        propagate.inject(carrier)
        if self._kind is WorkerKind.REMOTE:
            return await self._remote(carrier, kernel, args)

        async def crossing() -> T:
            # abandon_on_cancel everywhere: a cooperative cancel abandons the settle thread and leaves a loky worker to the
            # pool's reaper; the terminal arm escalates instead — its future mints loop-side (schedule is non-blocking and
            # thread-safe), so the cancel path terminates the RUNNING task through ProcessFuture.cancel and the killable
            # slot reclaims immediately rather than holding to the wall-clock kill.
            if self._enforcement is Enforcement.TERMINAL:
                # TERMINAL deadline rides pebble's schedule(timeout=) kill, its payload cloudpickle-sealed so a
                # closure-bearing argument survives pebble's stdlib pickler; the seal itself crosses the worker band —
                # a large closure/array payload's dumps otherwise stalls the event loop for its whole duration, while
                # schedule stays non-blocking and thread-safe once the inert bytes exist.
                sealed = await anyio.to_thread.run_sync(
                    # a pooled seal carries no boot — the pebble initializer already booted the worker it lands on.
                    lambda: cloudpickle.dumps((None, carrier, kernel, args)), abandon_on_cancel=True, limiter=WORKER_BAND
                )
                pending: Future[T] = self._pebble.schedule(sealed_kernel, args=(sealed,), timeout=kernel.deadline.to_optional())
                try:
                    return await anyio.to_thread.run_sync(pending.result, abandon_on_cancel=True, limiter=WORKER_BAND)
                except anyio.get_cancelled_exc_class():
                    pending.cancel()
                    raise

            def settled() -> T:
                # executor submission starts only once the band token is held, so WORKER_BAND bounds pool in-flight and the
                # settle thread in one acquisition; a broken device instance drops its corpse before the raise, so the
                # in-band retry re-mints and genuinely lands on a fresh pool.
                try:
                    return self._executor().submit(traced_kernel, carrier, kernel, *args).result()
                except loky.BrokenProcessPool:  # Exemption: the corpse-drop seam — the raise still crosses to the retry band whole.
                    if self._kind is WorkerKind.GPU:
                        self._loky = None
                    raise

            return await anyio.to_thread.run_sync(settled, abandon_on_cancel=True, limiter=WORKER_BAND)

        # in-band worker-death retry: `kernel.retry` is Nothing for a non-idempotent kernel, so the gate is the declaration.
        return await async_boundary(f"workers.{self._kind.value}", lambda: kernel.retry.map(lambda cls: guard(cls)(crossing)).default_with(crossing))

    @receipted(OPEN)
    async def warm(self, count: int | None = None) -> "PoolReceipt":
        # concurrent priming spawns the full worker set — a sequential await would keep one warm worker busy N times —
        # and each spawned worker runs the `_worker_boot` initializer (device pin, fidelity latch, telemetry and
        # profiler install) before the first no-op kernel lands; the
        # remote arm's same fold proves the channel and session capacity, its floor latching inside `shipped`. Handles
        # keep each priming rail, so a refused prime subtracts from the advertised count and an all-refused warm never flips WARM.
        primed = self._workers if count is None else max(0, min(count, self._workers))  # explicit 0 primes nothing; the arm cap bounds the request
        async with anyio.create_task_group() as group:  # Exemption: task-group registration is the one imperative spawn seam.
            handles = tuple(group.start_soon(self.submit, Kernel.of(fidelity, KernelTrait.HOSTILE)) for _ in range(primed))
        live = sum(1 for handle in handles if handle.return_value.is_ok())
        self._phase = PoolPhase.WARM if live else self._phase
        return PoolReceipt(phase=self._phase, kind=self._kind, enforcement=self._enforcement, workers=live)

    @receipted(OPEN)
    async def drain(self, grace: float = 30.0) -> "PoolReceipt":
        # graceful teardown: the submit fence refuses new work the moment DRAINING lands, in-flight work settles inside
        # `grace`, and the blocking joins ride the worker band so a drain never parks the loop.
        self._phase = PoolPhase.DRAINING
        match self._kind, self._enforcement:
            case (WorkerKind.REMOTE, _):
                with move_on_after(grace):
                    while self._sessions.statistics().borrowed_tokens:  # Exemption: the session-settle poll is the drain's grace wait.
                        await anyio.sleep(0.05)
                if self._conn is not None:
                    self._conn.close()  # channel close after the session wait; a survivor past grace ends with the channel EOF
                    await self._conn.wait_closed()
            case (_, Enforcement.TERMINAL):
                await anyio.to_thread.run_sync(lambda: (self._pebble.close(), self._pebble.join()), abandon_on_cancel=True, limiter=WORKER_BAND)
            case (WorkerKind.GPU, Enforcement.COOPERATIVE):
                if self._loky is not None:  # a dropped corpse leaves nothing to drain; a live instance settles in-flight work
                    await anyio.to_thread.run_sync(lambda: self._loky.shutdown(wait=True), abandon_on_cancel=True, limiter=WORKER_BAND)
            case (_, Enforcement.COOPERATIVE):
                # ownership gate: the loky singleton is process-global, so only the REGISTERED capsule may settle it —
                # a rolled-out stale capsule draining here would park the fresh arm's just-warmed workers.
                if WorkerPool._live.get(self._key) is self:
                    await anyio.to_thread.run_sync(lambda: self._executor().shutdown(wait=True), abandon_on_cancel=True, limiter=WORKER_BAND)
            case _ as unmatched:
                assert_never(unmatched)
        return PoolReceipt(phase=self._phase, kind=self._kind, enforcement=self._enforcement, workers=self._workers)

    @receipted(OPEN)
    def retire(self) -> "PoolReceipt":
        # terminal teardown: the memo drops so the next acquire re-spawns a fresh arm; kill_workers reclaims a stuck loky
        # pool, and abort tears the remote channel without the close handshake a wedged far host never answers.
        match self._kind, self._enforcement:
            case (WorkerKind.REMOTE, _):
                if self._conn is not None:
                    self._conn.abort()
            case (_, Enforcement.TERMINAL):
                self._pebble.stop()
                self._pebble.join()
            case (WorkerKind.GPU, Enforcement.COOPERATIVE):
                if self._loky is not None:  # a dropped corpse already died; a live instance is killed, never re-minted to retire
                    self._loky.shutdown(wait=False, kill_workers=True)
            case (_, Enforcement.COOPERATIVE):
                # ownership gate: `_executor()` re-acquires the process-global singleton, so an unguarded retire on a
                # rolled-out stale capsule would mint-or-seize the FRESH arm's pool and kill it — only the capsule
                # still registered under its own key may shut the singleton down; a superseded capsule retires memo-only.
                if WorkerPool._live.get(self._key) is self:
                    self._executor().shutdown(wait=False, kill_workers=True)
            case _ as unmatched:
                assert_never(unmatched)
        self._phase = PoolPhase.RETIRED
        if WorkerPool._live.get(self._key) is self:  # a rolling restart's fresh arm never drops with the stale
            WorkerPool._live.pop(self._key)
        return PoolReceipt(phase=self._phase, kind=self._kind, enforcement=self._enforcement, workers=0)
```

## [04]-[SUPERVISION]

- Owner: `Supervisor` binds the probe evidence, the restart rows, and the health projection into the one actuator loop the branch lacked: every ingredient existed — psutil probes, retry backoff, health status, worker-death markers — and this owner closes them. A `SupervisionPolicy` row per supervised subject carries the probe ceilings and the windowed restart budget; verdicts are data the loop folds, never inline judgment.
- Cases: `Verdict` closes the probe outcomes — `LIVE`, `DEGRADED` (a ceiling breached: rss over budget, involuntary context-switch storm), `DEAD` (child gone, pool retired) — and `_actuate` maps each onto its actuation: `LIVE` re-arms the probe, `DEGRADED` rolls the arm, `DEAD` flips the subject down and re-spawns under the kind's restart row with stamina backoff. Budgets are windowed: `restarts` actuations inside `window` seconds park the subject `NOT_SERVING` until the window drains, so a crash storm holds down instead of thrashing.
- Entry: `Supervisor.watch(group)` starts one probe loop per supervised subject inside the caller's task group — the daemon composition root's supervision group, never a private loop — each cycle fenced so a probe or actuation raise emits one rejected receipt and the rhythm survives. Probes are arm-scoped: a `DAEMON` charge reads its own child handle under one `oneshot` with the listing-to-reading race fenced, and a pool charge reads capsule presence and the arm's own `alive()` before weighing exactly its workers — the loky arms (device arms included) by their published pid maps, the pebble arm by complement over the children that are neither daemons nor a sibling arm's named workers — so one heavy child never marks an unrelated subject degraded; a `REMOTE` charge reads channel liveness alone, resource ceilings belonging to the far host's own supervisor, and a `GPU` charge weighs host RSS alone — device-memory evidence stays the kernel's own receipt, unobservable through a psutil scan. Verdicts project onto the serve owner's per-service flip — the injected awaited `ServerHost.status` coroutine — so any `DEAD` subject flips its service `NOT_SERVING` and recovery flips it back, the health poller the estate shipped only the server half of; the `verdicts` accessor publishes the same last-verdict state as data for the bundle capsule, so no second verdict surface exists beside the flip and the projection.
- Daemon kind: a `DAEMON` worker is a supervised long-lived child — `psutil.Popen` fuses the subprocess handle with the probe surface, the spawn environment forwards the parent's effective OTLP endpoint through `WorkerBoot.env` so the child's own composition root installs against the same collector, `terminate()` then `kill()` after the grace window is the stop escalation, restart rides the `SPAWN` row targeting spawn transients, and an empty spawn command is a config refusal that parks the subject down; a daemon's readiness is its next `LIVE` verdict, never a sleep. `Supervisor.stop()` runs that same escalation over every surviving child as the serve drain fold's daemon row, so a child never outlives the daemon that spawned it.
- Growth: a new probe dimension is one `SupervisionPolicy` ceiling field with one `_weighed` term; a new actuation is one `_actuate` arm; a new supervised subject is one `Supervisor.watch` registration.
- Boundary: the supervisor actuates pooled arms — device arms included — remote channels, and daemon children only — `ChargeKind` seals that subject set by construction — and it never restarts the serve host, never owns the signal seam (`transport/serve#ENTRY`'s), and never emits health protocol wire (the serve owner's `HealthServicer` is the sole advertiser). Probe evidence emits through the contributor port under the `OPEN` policy.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import time
from collections.abc import Awaitable, Callable
from enum import StrEnum
from typing import Literal, assert_never

import anyio
import anyio.to_thread
import psutil
from anyio.abc import TaskGroup
from expression import Result
from expression.collections import Block

from rasm.runtime.faults import BoundaryFault, async_boundary
from rasm.runtime.receipts import OPEN, PROCESS_FAULTS, Receipt, Signals
from rasm.runtime.resilience import guard

# every FABRIC- and POOL-region owner the supervision code names resolves in this same module's earlier regions — no cross-module import.

# --- [TYPES] ----------------------------------------------------------------------------


class Verdict(StrEnum):
    LIVE = "live"
    DEGRADED = "degraded"
    DEAD = "dead"


type Flip = Callable[[str, bool], Awaitable[None]]
# supervisable subjects are the pooled, spawned, and dialed kinds alone; a THREAD, INTERPRETER, or WASM charge is
# unspellable because the anyio arms hold no pool, child, or channel a probe could weigh.
type ChargeKind = Literal[WorkerKind.PROCESS, WorkerKind.GPU, WorkerKind.DAEMON, WorkerKind.REMOTE]

# --- [MODELS] ---------------------------------------------------------------------------


class SupervisionPolicy(Struct, frozen=True, gc=False):
    subject: str
    interval: float = 5.0
    rss_ceiling: int = 2_147_483_648
    switch_ceiling: int = 100_000  # involuntary context-switch storm marks a thrashing worker DEGRADED before rss does
    grace: float = 5.0
    restarts: int = 3
    window: float = 300.0  # rolling budget window: `restarts` actuations inside it park the subject down until it drains


class Charge(Struct, frozen=True):
    # one supervised subject: a pooled arm, a daemon child, or a remote channel; `command` spawns the DAEMON kind and stays
    # empty on pool charges, `placement` names the REMOTE endpoint or GPU device and stays None on plain local charges.
    # `policy.subject` is the health key the flip advertises, so it spells the registered gRPC service name exactly —
    # a drifted subject flips a phantom key.
    policy: SupervisionPolicy
    kind: ChargeKind
    enforcement: Enforcement = Enforcement.COOPERATIVE
    command: tuple[str, ...] = ()
    placement: Placement = None


# --- [SERVICES] -------------------------------------------------------------------------


class Supervisor:
    def __init__(self, charges: Block[Charge], flip: Flip) -> None:
        self._charges, self._flip = charges, flip
        self._verdicts: dict[str, Verdict] = {}
        self._children: dict[str, psutil.Popen] = {}  # DAEMON handles: subprocess control fused with the probe surface
        self._stamps: dict[str, tuple[float, ...]] = {}  # per-subject actuation stamps the windowed budget folds

    def _weighed(self, proc: psutil.Process, policy: SupervisionPolicy) -> bool:
        # one oneshot batches both ceiling reads; the listing-to-reading race is fenced — a vanished process weighs nothing.
        try:
            with proc.oneshot():
                return proc.memory_info().rss > policy.rss_ceiling or proc.num_ctx_switches().involuntary > policy.switch_ceiling
        except PROCESS_FAULTS:
            return False

    def _probe(self, charge: Charge) -> Verdict:
        match charge.kind:
            case WorkerKind.DAEMON:
                handle = self._children.get(charge.policy.subject)
                if handle is None or not handle.is_running():
                    return Verdict.DEAD
                return Verdict.DEGRADED if self._weighed(handle, charge.policy) else Verdict.LIVE
            case WorkerKind.REMOTE:
                # channel liveness is the whole remote probe: rss/switch ceilings belong to the far host's own supervisor,
                # unobservable through a local psutil scan.
                held = WorkerPool.live(charge.kind, charge.enforcement, _placed(charge.placement))
                return Verdict.DEAD if held.is_none() or not held.value.alive() else Verdict.LIVE
            case kind:
                held = WorkerPool.live(kind, charge.enforcement, _placed(charge.placement))
                if held.is_none() or not held.value.alive():
                    return Verdict.DEAD
                # arm-scoped weighing: the loky arm names its worker pids; the pebble arm probes the complement — every child
                # that is neither a daemon nor a sibling arm's named worker — so one heavy child never marks an unrelated subject.
                pool = held.value
                named = pool.pids()
                excluded = frozenset(child.pid for child in self._children.values()) | frozenset(
                    pid for other in WorkerPool._live.values() if other is not pool for pid in (other.pids() or ())
                )
                scoped = (
                    child
                    for child in psutil.Process().children(recursive=True)
                    if child.pid not in excluded and (named is None or child.pid in named)
                )
                return Verdict.DEGRADED if any(self._weighed(child, charge.policy) for child in scoped) else Verdict.LIVE

    def _budgeted(self, policy: SupervisionPolicy) -> bool:
        # windowed restart budget: stale stamps drop and ONLY a granted actuation stamps in, so a parked subject's
        # window drains on its own instead of self-refreshing on every parked probe cycle.
        now = time.monotonic()
        fresh = tuple(stamp for stamp in self._stamps.get(policy.subject, ()) if now - stamp < policy.window)
        granted = len(fresh) < policy.restarts
        self._stamps[policy.subject] = (*fresh, now) if granted else fresh
        return granted

    async def _actuate(self, charge: Charge, verdict: Verdict) -> None:
        subject = charge.policy.subject
        self._verdicts[subject] = verdict
        match verdict:
            case Verdict.LIVE:
                await self._flip(subject, True)
            case Verdict.DEGRADED | Verdict.DEAD:
                if not self._budgeted(charge.policy):
                    await self._flip(subject, False)
                    Signals.emit(Receipt.of("workers", ("emitted", f"supervise.{subject}", {"verdict": verdict.value, "held": True})), OPEN)
                    return
                if verdict is Verdict.DEAD:
                    await self._flip(subject, False)
                restarted = await guard(KIND_POLICY[charge.kind].restart)(self._respawn, charge)
                Signals.emit(Receipt.of("workers", ("emitted", f"supervise.{subject}", {"verdict": verdict.value, "restarted": restarted})), OPEN)
            case _ as unreachable:
                assert_never(unreachable)

    async def _respawn(self, charge: Charge) -> bool:
        subject = charge.policy.subject
        match charge.kind:
            case WorkerKind.DAEMON:
                if not charge.command:  # a DAEMON charge without a spawn command is a config refusal, parked down
                    await self._flip(subject, False)
                    Signals.emit(Receipt.of("workers", BoundaryFault(config=(subject, "daemon-charge-without-command"))), OPEN)
                    return False
                # terminate-then-kill escalation on any stale handle, then a fresh child; readiness is the next LIVE verdict,
                # so the subject stays down until _actuate observes the respawn live — never a flip the spawn itself asserts.
                stale = self._children.get(subject)
                if stale is not None and stale.is_running():
                    stale.terminate()
                    with anyio.move_on_after(charge.policy.grace):
                        await anyio.to_thread.run_sync(stale.wait, abandon_on_cancel=True, limiter=WORKER_BAND)
                    if stale.is_running():
                        stale.kill()
                        # reap the SIGKILLed child before its handle drops: an unwaited kill leaves a zombie whose
                        # is_running() still answers True, and the replacement below would strand it unreaped.
                        with anyio.move_on_after(charge.policy.grace):
                            await anyio.to_thread.run_sync(stale.wait, abandon_on_cancel=True, limiter=WORKER_BAND)
                # spawn environment forwards the parent's effective OTLP endpoint beside the inherited environment, so
                # the child's own composition root installs against the same collector — the daemon row of the worker
                # install seam, its telemetry owned by the child's boot, never a parent-side patch.
                self._children[subject] = psutil.Popen(list(charge.command), env={**os.environ, **WorkerBoot.captured(WorkerKind.DAEMON).env()})
                return True
            case kind:
                # roll receipt is the respawn verdict: a fresh arm that never flips WARM is a failed restart, held down
                # under the same next-LIVE law rather than advertised on the actuator's own optimism.
                receipt = await WorkerPool.roll(kind, charge.enforcement, charge.placement)
                return receipt.phase is PoolPhase.WARM

    async def _cycle(self, charge: Charge) -> None:
        while True:  # Exemption: the supervision loop is the daemon's standing probe rhythm, cancelled by its owning task group.
            (await async_boundary(f"supervise.{charge.policy.subject}", lambda: self._actuate(charge, self._probe(charge)))).swap().map(
                lambda fault: Signals.emit(Receipt.of("workers", fault), OPEN)
            )  # the rhythm survives a probe or actuation raise
            await anyio.sleep(charge.policy.interval)

    def verdicts(self) -> Map[str, str]:
        # bundle-facing projection: the last per-subject verdict as data, never the live mutable dict — the diagnostic
        # capsule reads supervision state through this one accessor.
        return Map.of_seq((subject, verdict.value) for subject, verdict in self._verdicts.items())

    def watch(self, group: TaskGroup) -> None:
        for charge in self._charges:  # Exemption: task-group registration is the one imperative spawn seam.
            group.start_soon(self._cycle, charge)

    async def stop(self) -> int:
        # shutdown escalation for the DAEMON children the pool drain never touches: terminate, await under the charge's own
        # grace, kill a survivor — the supervision group is already cancelled, so this is the one live-handle sweep left.
        stopped = 0
        for charge in self._charges.filter(lambda held: held.kind is WorkerKind.DAEMON):  # Exemption: the stop walk is the teardown seam.
            child = self._children.pop(charge.policy.subject, None)
            if child is None or not child.is_running():
                continue
            child.terminate()
            with anyio.move_on_after(charge.policy.grace):
                await anyio.to_thread.run_sync(child.wait, abandon_on_cancel=True, limiter=WORKER_BAND)
            if child.is_running():
                child.kill()
                # same wait-after-kill law as the restart path: the popped handle is the last reference, so the
                # kill reaps here or the child zombifies past the supervisor's own teardown.
                with anyio.move_on_after(charge.policy.grace):
                    await anyio.to_thread.run_sync(child.wait, abandon_on_cancel=True, limiter=WORKER_BAND)
            stopped += 1
        return stopped


# --- [ENTRY] ------------------------------------------------------------------------------

if __name__ == "__main__":  # fleet floor: the remote arm's session command lands here on the far interpreter
    sys.exit(remote_floor())
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [LOKY_RESOURCE_TRACKER]-[OPEN]: does the admitted loky release spawn cleanly under the final CPython 3.15 interpreter, or does the beta-observed `resource_tracker` `ValueError: unknown resource type ... semlock` noise persist and demand an upstream pin or patch; `uv run python -c` live `get_reusable_executor` submit probe on the released interpreter.
