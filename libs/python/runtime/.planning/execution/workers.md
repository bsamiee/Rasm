# [PY_RUNTIME_WORKERS]

One closed `WorkerKind` family carries every worker the branch runs — thread, subinterpreter, process, device-pinned process, daemon, remote, and sandboxed guest — each kind a `KIND_POLICY` row binding its fidelity obligation and, where something actuates it, its restart class. `Kernel` is the single kernel-crossing owner: every callable that leaves the event loop crosses as one frozen value whose `KernelTrait` row derives isolation, worker-death retry, deadline enforcement, and shipping, so no consumer re-derives a modality, pairs a retry class by convention, or hand-builds a name-crossing gate. `WorkerPool` owns the warm reusable process pools and the fleet-host remote arm with spawn/warm/roll/drain/retire lifecycle, and `Supervisor` is the restart actuator — psutil probes answering typed ceiling evidence, a windowed restart budget, and the health projection the serve owner advertises.

Composition is settled: the thread and subinterpreter crossing arms stay `execution/lanes#LANE` owners the trait table projects onto, while both process arms ride `WorkerPool` under the `WORKER_BAND` this page mints; worker-death backoff rides `reliability/resilience#RESILIENCE` rows (`OCCT` for the anyio subinterpreter arm, `WORKER` for the pool executors, `SPAWN` for a daemon child); every pool fault converts through the `reliability/faults#FAULT` lift, whose pool-death row lands executor deaths on the `resource` case; pool and supervision evidence streams through the `observability/receipts#RECEIPT` contributor port. `cloudpickle` ships a closure or local callable by value across the pickle seams stdlib pickle refuses, `tblib` re-raises a worker fault parent-side with its worker frames, `loky` owns the crash-respawning warm process pool every cooperative `PROCESS` offload rides, `pebble` owns the terminal wall-clock kill, PEP 734 `anyio.to_interpreter` owns the subinterpreter substrate with zero package cost, `asyncssh` carries the sealed kernel to a fleet host over the one `transport/roots#RESOURCE` `RemoteEndpoint` channel under the `SSH` restart row, `wasmtime` runs a sandboxed guest module in-process under a shared epoch pacer, and the device arms pin an accelerator through loky's `env=` and pebble's initializer at spawn — placement rides the existing pools, zero new package. Worker floors are parented emitters: every process arm boots the parent-captured telemetry install post-spawn through one `_worker_boot` initializer, the kernel span opens under the carried W3C parent, the profiler attaches where the cycles burn, and the two-read `Cost` bracket prices every crossing to the tenant that ran it.

## [01]-[INDEX]

- [02]-[CROSSING]: `Kernel` owns every isolation crossing — the `WorkerKind` family and its policy rows, the `KernelTrait` isolation classifier, shipping, wire, deadline, and enforcement as fields, the host-side `admitted` gate, the shared-memory span channel, the parented-emitter worker gate (kernel span, profiler phase, and cost bracket over the stitch-and-resolve pair), the remote-floor entry, the guest sandbox arm with its epoch pacer, and the tblib fidelity latch.
- [03]-[POOL]: the warm reusable `WorkerPool` capsule — loky, pebble, and per-device arms under one lifecycle vocabulary, the `WorkerBoot` install seam and its exit-owned flush law, band-bounded settle, in-band worker-death retry, and the asyncssh remote arm crossing the sealed kernel to a fleet host.
- [04]-[LEASE]: one fenced claim, heartbeat, settlement, and drain algebra over an admitted backend generation.
- [05]-[SUPERVISION]: kind-scoped psutil verdicts as typed `Weighed` evidence, restart budget and escalation, serving health, and bundle verdict projection.

## [02]-[CROSSING]

- Owner: `WorkerKind` closes the worker family; a kind's standing obligations are its `KIND_POLICY` row — the fidelity latch and the restart class an actuator re-drives it under, empty on the kinds nothing actuates — so a new worker kind is one member and one row, never a sibling vocabulary. `Kernel` unifies the four crossing spellings the branch grew — a name-string gate, a by-reference module kernel, a page-local `_run_kernel`, a per-family `_dispatch` — into one frozen value: `Kernel.of` classifies the target once, and every downstream hop reads isolation, retry, deadline, enforcement, and wire off the value.
- Cases: `KernelTrait` answers the one isolation question per kernel family — `INLINE` runs on the loop (sub-quantum pure body), `PURE` wants its own GIL in-process (subinterpreter), `RELEASING` releases the GIL or blocks on a syscall (thread), `HOSTILE` holds process-global native state or a GIL-hostile extension (process), `SANDBOXED` runs a foreign guest module inside the in-process wasmtime sandbox (the wasm-bytes target shape answers it, never a declaration) — and `TRAIT_ROW` projects each trait onto its `WorkerKind` and default worker-death `RetryClass`. A consumer declares the trait per kernel family as domain knowledge; isolation, band, retry, and crossing mechanics are this owner's.
- Entry: `Kernel.of(target, trait, *, deadline, enforcement, wire, idempotent, retry)` is the one constructor, polymorphic on the target shape — the trait's seam classifies first, so a loop or thread kernel ships `LIVE` with its callable carried whole at zero serialization, a pickle-seam kernel classifies by picklability (a module-qualified callable ships `REFERENCE`; a closure, `<locals>` callable, or bound method ships `VALUE` as cloudpickle bytes, since a by-name walk loses the instance a `__self__` carries), a `(module, name)` pair is the native-gated form: the loop floor names a worker-floor kernel it must never import, `REFERENCE` by construction, and a wasm module crosses as its own bytes — `GUEST` by construction, the digest label its receipt and fault subject. `deadline` is the per-offload budget the lane folds against its own — the tighter bound wins — and `enforcement` selects the deadline arm: `COOPERATIVE` cancels the awaiting scope and leaves the worker to the pool's reaper, `TERMINAL` routes the hop through the pebble pool so the deadline kills the worker mid-kernel and reclaims the slot, the only bound a hung native call obeys.
- Auto: `fidelity()` latches `tblib.pickling_support.install()` once per interpreter — every exception crossing a pickle seam thereafter carries its worker-side frames, so the faults lift classifies the true cause instead of a flattened marker; the latch runs in every pool initializer whose `KIND_POLICY` row obliges it and inside `shipped`, so a spawned interpreter re-latches cold. This latch runs default-off locals capture, and the explicit `Traceback` carrier stays out — the pickle rail is the crossing's whole tblib surface, receipts carrying facts, never frames.
- Law: a worker-death retry re-runs the kernel whole, so the retry row binds only under the kernel's `idempotent` declaration — content-keyed inputs, run-scoped outputs, no external state — and `Kernel.of` drops EVERY retry binding for a kernel declaring `idempotent=False`, the caller's explicit class exactly as the trait default, because a body that cannot re-run overrules a call site that asked for a re-run; the declaration gates the two live dispatch sites — `WorkerPool.submit` for the pooled kinds and the `execution/lanes#LANE` offload hop for the anyio arms — each reading `kernel.retry`, never a convention the call site remembers.
- Law: `Wire` closes the payload-crossing axis — `PICKLE` copies arguments across the seam, `SHARED_MEMORY` exports every top-level ndarray argument once into a named `multiprocessing.shared_memory` block and crosses it as a `ShmSpan` the worker re-views through `numpy.frombuffer` — so a heavy-buffer kernel upgrades its crossing by one field with the call site untouched. Block custody stays loop-side and BRACKETS the whole offload hop: `execution/lanes#LANE` is that bracket's one owner — `exported` copies and names before the hop, `released` closes and unlinks after it settles — so `WorkerPool.submit` receives arguments already spanned and re-exports nothing, and a consumer submitting to an arm directly owns the same bracket around its own call. The worker view is ingress-only: a kernel consumes it inside its body and returns owned material, because the worker handle closes when the kernel returns. A buffer wrapped inside a struct stays `PICKLE`; only a bare ndarray argument rides the span channel. Named blocks are the chosen out-of-band channel because cloudpickle's protocol-5 `buffer_callback` collects buffers the executor transports have no side channel to carry, where a named block crosses at zero payload bytes.
- Law: a native-gated worker module splits at the parse floor — the vocabulary module parses and imports on both interpreter floors while the worker-body module holds its eager native providers and loads only worker-side — and `REFERENCE` shipping resolves the kernel by qualified name through the one `shipped` gate; `covered(module, names)` is the worker-floor witness that same module runs at its own import, proving every dispatchable name resolves through the identical walk so a misspelled roster fails at worker import, never mid-offload. Crossing law homes at `libs/python/.planning/RULINGS.md`; `artifacts` scene rendering is the standing proof instance. `cloudpickle.register_pickle_by_value` stays out: a worker floor that must import its module from disk is the stronger contract than shipping a module by value into an interpreter that drifts from it.
- Law: `traced_kernel` is the parented-emitter gate every crossing resolves through — the receipts pair resolves the carried W3C parent, the `worker.<name>` span opens under it so worker-interior evidence joins the one trace, the profiler `phase` window tags the flame by kernel subject and shipping form, and the two-read `Cost` bracket records the kernel's own process spend onto the `rasm.cost.<measure>` rows under the attached context, the promoted `rasm.tenant` entry pricing the kernel to the tenant that ran it; an uninstalled floor resolves no-op providers and a null profiler window, so the gate never conditions on install state and costs two process reads. The cost rows key on `Kernel.subject`, not `name`: a code kernel's qualname is bounded by the code base while a GUEST digest is minted from caller-controlled bytes, and only the tenant axis carries a value budget, so a per-digest attribute would accrete a series axis nothing bounds — the digest stays the span, receipt, and fault subject, where it costs no series.
- Law: `remote_floor` is the fleet mirror of `shipped` — the far interpreter's module entry reads one sealed blob on stdin, resolves it through the same `sealed_kernel` gate, and writes one pickled `("value", T) | ("raise", BaseException)` verdict on stdout — so every `Shipping` form is total across the SSH channel (the seal cloudpickles the whole `Kernel`, a `LIVE` callable crossing by value, `REFERENCE` re-importing from the remote install, the worker-floor contract at fleet scale) and a kernel raise crosses home frame-whole under the latch `shipped` re-arms.
- Law: `admitted` is the host-side crossing gate — every offload passes it before an arm sees the kernel, and `GUEST` is the one form that does work there, `wasmtime.Module.validate` refusing malformed wasm and a non-bytes payload parent-side onto a typed `config` fault at parse cost with no compile, no store, and no instance. Caller-controlled bytes are the reason: a guest defect reaching the worker floor surfaces one hop and one epoch budget later as an instantiation trap the catch-all `boundary` case cannot tell from a genuine guest trap. Every other shipping form yields `Nothing` at one branch, so the gate stays one call on the offload path and never a wasm arm inside the lane's isolation table.
- Law: `_guest` is `GUEST` shipping's worker-floor arm — zero-import instantiation (no WASI, no ambient capability), a fresh `Store` per call so guest state never leaks across kernels, `GUEST_MEMORY` bounding linear memory, and request/reply crossing as bytes over the `GUEST_ABI` exports, which is also why the arm's result type IS bytes rather than the crossing's free `T`; the module compiles once per digest per interpreter, so the per-call cost is instantiation alone. Engine and module resolve as a PAIR under one gate because `SANDBOXED` kernels ride the thread arm: two first guests land concurrently as the ordinary case, a memo that re-enters its body on a concurrent miss lets the loser's engine escape with its own pacer thread, and a store on one engine instantiating a module compiled on another refuses. One `WasmtimeError` fence spans compile, instantiate, and call, so no arm of the guest crossing escapes `shipped` outside the elapsed-budget discrimination.
- Law: the guest deadline is the engine's epoch — one daemon pacer heartbeats the engine-global epoch every `EPOCH_TICK` while each store carries its own relative tick budget, so concurrent guests never kill each other and a guest dies mid-kernel at wall clock IN-PROCESS, the enforcement no thread or interpreter arm owns. `WasmtimeError` exposes no addressable trap code, so the arm discriminates by elapsed budget: an epoch kill re-raises `TimeoutError` onto the faults `deadline` row, and a genuine trap crosses whole into the catch-all `boundary` case with its trap message.
- Growth: a new worker kind is one `WorkerKind` member with one `KIND_POLICY` row; a new isolation answer is one `KernelTrait` member with one `TRAIT_ROW` row and every call site untouched; a new shipping form is one `Shipping` member with one `shipped` arm and, where its payload arrives unjudged, one `admitted` arm; a new enforcement arm is one `Enforcement` member with one offload projection row; a new payload crossing is one `Wire` member with one `exported` arm; a new cost measure is one `Cost` field at the receipts owner with one `INSTRUMENTS` row at the metrics owner, reaching this bracket through `measures` with zero gate edits.
- Boundary: trait declaration stays consumer domain knowledge — this owner never inspects a callable for GIL behavior; picklability is the one property `Kernel.of` classifies itself. Thread and subinterpreter crossing arms and the offload hop stay `execution/lanes#LANE`'s; this page mints the vocabulary the hop consumes, the process bands, and the process pools. `execution/admission#CONTEXT` admits the `isolation` axis upstream and refuses an unbound crossing there, so `KernelTrait` selects the worker kind INSIDE a value the profile already serves — `INLINE` under `in-proc`, `PURE` and `RELEASING` under `thread`, `HOSTILE` under `process`, `SANDBOXED` under `wasm`, the `WorkerKind.REMOTE` fleet arm under `remote` — and a kernel reaching a crossing the profile never admitted is unrepresentable, never a runtime downgrade this owner absorbs.

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
from expression import Nothing, Option, Result, Some
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace
from tblib import pickling_support

from rasm.runtime.faults import SCOPES, BoundaryFault, RuntimeRail, Scope, boundary, scoped
from rasm.runtime.metrics import Metrics
from rasm.runtime.profiles import Profiles
from rasm.runtime.receipts import Cost, Signals
from rasm.runtime.resilience import RetryClass

lazy import numpy  # shared-memory reconstruction alone touches it; the wire stays dark for PICKLE-only processes
lazy import wasmtime  # the guest admission and worker arms alone touch it; an interpreter running no sandboxed kernel stays dark

# crossing tracer minted once per interpreter — the worker floor included: the API mints a fresh handle per
# `get_tracer` call and caches none, so a per-crossing mint allocates on the offload path, while the pre-install
# proxy this module-scope handle resolves re-reads the global at every `start_span` and upgrades with no invalidation.
# The scope names THIS emitting library, not the served host: a backend joining on scope separates worker-crossing
# spans from the serve host's own, where the shared `SERVICE` row left four independent planes indistinguishable.
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
    # `restart` is the SUPERVISED restart class an actuator re-drives the subject under, so it seats only on the kinds
    # something actuates — the pooled, spawned, and dialed set `ChargeKind` seals. A kind whose only death answer is the
    # crossing's own `TRAIT_ROW.retry` carries `Nothing` rather than a second class no reader consults and no probe can
    # keep honest: a THREAD row spelling a restart the trait row leaves empty is exactly the divergence this slot closes.
    fidelity: bool  # pickle seam: the tblib latch rides the pool initializer where True; THREAD shares memory, DAEMON is spawned not called
    restart: Option[RetryClass]


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
            # trait refused at construction, never a kernel the non-seam arm ships LIVE onto the wasm band. The raise is the
            # honest form here where `admitted` rails: `of` returns a VALUE every call site reads fields off, so a rail would
            # oblige every construction to unwrap a gate that cannot otherwise fail, and a trait contradicting its own target
            # is a caller code defect no runtime fault channel repairs — exactly as the pool refuses a mis-placed arm.
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

    @property
    def subject(self) -> str:
        # the SERIES discriminant, distinct from `name`: a code kernel's qualname is bounded by the code base, while a
        # GUEST digest is minted from caller-controlled bytes, so a per-digest metric attribute accretes an unbounded
        # KIND axis the tenant budget never reaches — the same accretion `GUEST_MODULES` bounds on the compile side.
        # The digest stays the span name, receipt, and fault subject, where it costs no time series.
        return self.shipping.value if self.shipping is Shipping.GUEST else self.name


# --- [OPERATIONS] -----------------------------------------------------------------------


@cache
def fidelity() -> bool:
    # process-global one-shot: every exception pickled after this carries its traceback; idempotent under @cache per interpreter.
    pickling_support.install()
    return True


def admitted[T](kernel: Kernel[T]) -> Option[BoundaryFault]:
    # host-side crossing admission — the one gate a crossing passes before any arm sees the kernel, answering the
    # REFUSAL alone so every total shipping form costs one branch. GUEST is the sole form whose payload is
    # caller-controlled bytes no earlier seam judged: `Module.validate` refuses malformed wasm (`WasmtimeError`) and
    # a non-bytes payload (`TypeError`) parent-side at parse cost — no compile, no store, no instance — where the
    # same defect otherwise surfaces worker-side one hop later as an instantiation trap the catch-all `boundary`
    # case cannot tell from a genuine guest trap. The throwaway `Engine` is deliberate: validation reads the module
    # and arms no epoch, so the gate never touches the memoized engine the guests themselves run on.
    if kernel.shipping is not Shipping.GUEST:
        return Nothing
    match boundary(f"workers.wasm.{kernel.name}", lambda: wasmtime.Module.validate(wasmtime.Engine(), kernel.payload)):
        case Result(tag="error"):
            return Some(BoundaryFault(config=(f"workers.wasm.{kernel.name}", "guest-validation-refused")))
        case _:
            return Nothing


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
        span = _TRACER.start_span(f"worker.{kernel.name}")
        with trace.use_span(span, end_on_exit=True), Profiles.phase({"kernel": kernel.name, "shipping": kernel.shipping.value}):
            before = Cost.own()
            try:
                return shipped(kernel, *args)
            finally:  # Exemption: the bracket's terminal read pairs with the entry read — two process reads per crossing, fault arm included.
                # a bracket missing either read records NOTHING: the counters are cumulative, so a spend derived
                # from one live read and one absent one prices this kernel at the whole process's history.
                Cost.spent(Cost.own(), before).map(lambda spend: Metrics.record(spend.measures(), domain="cost", kind=kernel.subject))


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
    channel.buffer.flush()  # the frame reaches the parent HERE; an unflushed buffer holds it behind the atexit drain the parent then waits out
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


# --- [GUEST_SANDBOX]

# guest memo gate, homed with the memos it serializes: the engine and the compiled-module cache resolve as a PAIR under
# it, so concurrent first guests on the thread arm can never split an engine from a module compiled against another one.
_GUEST_GATE: Final[threading.Lock] = threading.Lock()


def _paced(engine: "wasmtime.Engine") -> None:
    while True:  # Exemption: the epoch pacer is the guest crossing's standing heartbeat, a daemon thread ending with the interpreter.
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


def _guest[T](kernel: Kernel[T]) -> Callable[..., bytes]:
    # GUEST shipping's worker-floor arm: zero-import instantiation — no WASI, no ambient capability — a fresh Store per
    # call, GUEST_MEMORY bounding linear memory, and the store's relative epoch budget as the in-process wall-clock kill.
    # The arm spells `bytes` rather than borrowing the crossing's free `T`: the GUEST_ABI byte exchange IS the guest's
    # whole result contract, so a SANDBOXED kernel's `T` is bytes by construction and an annotation claiming otherwise
    # promises a shape no export can return.
    def run(request: bytes = b"") -> bytes:
        started = time.monotonic()
        try:
            with _GUEST_GATE:
                # SANDBOXED kernels ride the thread arm, so two first guests land concurrently as the ordinary case, and
                # `@cache` re-enters its body on a concurrent miss: the loser's engine escapes the memo with its own pacer
                # thread, and a store on one engine instantiating a module compiled on the other refuses. One gate over the
                # memo PAIR makes engine and module co-resolve, folds a duplicate compile of the same bytes into one, and
                # holds nothing across the call — the guest body itself runs outside it.
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
            # the fence spans COMPILE, instantiate, and call, because `WasmtimeError` is total across all three and a
            # compile raised outside it escapes `shipped` raw, skipping the one arm that owns this package's raises.
            # The error exposes no addressable trap code, so elapsed budget discriminates: an epoch kill re-raises
            # TimeoutError onto the deadline row, and every other trap — a compile refusal the host gate could not
            # reach, an instantiation failure, a genuine guest trap — crosses whole with its message onto `boundary`.
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
# seam, same pool-death names; DAEMON is spawned, not called, and its restart row targets spawn transients rather than pool
# deaths; REMOTE's fidelity marks the SSH pickle seam and its restart row targets channel transients — the channel, not an
# executor, is what dies at fleet scale. THREAD, INTERPRETER, and WASM restart to `Nothing`: nothing actuates them, so their
# whole death answer is the crossing's own `TRAIT_ROW.retry`, and a second class here would be a row no reader opens.
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

- Owner: `WorkerPool` is the warm reusable pool capsule — one polymorphic surface over the process-executor, device, and fleet remote arms: `loky.get_reusable_executor` for `COOPERATIVE` (process-global warm pool, crash-respawning, cloudpickle payloads, idle reap on `timeout`), `pebble.ProcessPool` for `TERMINAL` (`schedule(timeout=)` kills a running worker at wall-clock and reclaims the slot; `max_tasks` recycles a worker after N tasks bounding RSS creep), one `asyncssh` channel per `WorkerKind.REMOTE` arm (a memoized `SSHClientConnection` off `transport/roots#RESOURCE` `RemoteEndpoint.dialed`, per-submit `create_process` sessions running `remote_floor`, a per-arm session limiter bounding fleet in-flight), and one instance-owned loky executor per `WorkerKind.GPU` device arm — the process-global singleton cannot hold per-device state, so device custody is pebble-style instance ownership, `env=` pinning the device before any worker module loads on the cooperative arm and the `_worker_boot` initializer pinning it on the terminal arm. Custody follows each substrate's own topology: the pebble pool is instance-owned, the loky singleton arm holds acquisition arguments and re-acquires through the factory per use because loky is process-global and a broken instance is replaced only by re-acquisition — a field pinning the executor pins the corpse — the remote arm re-dials a closed channel per use under the same law, and a broken device instance drops its corpse at the submit seam so the next acquisition mints fresh. Arm selection is `(WorkerKind, Enforcement, placement key)` — one derived key off the `Placement` shape, endpoint key, device key, or `""` local, never a caller-facing executor knob — and the subinterpreter and sandbox kinds ride no pool: the anyio arms are already own-GIL or in-process-killable substrates a package pool duplicates without adding respawn or kill capability.
- Entry: `WorkerPool.acquire(kind, enforcement, placement)` memoizes one live pool per arm key behind a membership guard — an effectful `setdefault` mint is the deleted form — so every call site shares the warm workers; `live` reads that registry without minting under the SAME `(kind, enforcement, placement)` triple every other verb takes, `alive` reads the arm's own liveness (pebble `active`; the loky arms self-heal per acquisition; the remote arm reads `is_closed` off its dialed channel), and `pids` is total — every arm names its own live worker set off its substrate's map, so an arm-scoped probe weighs exactly its workers and never a process-tree complement two unnamed arms would share. Both teardowns drop the memo under one ownership gate — a drained arm is re-mintable rather than a registered key whose submit fence refuses forever — and the last drop retires the `WORKER_BAND` occupancy probe the first mint registered, so the band's level series lives exactly as long as something bounds it; the fleet arm's per-arm session limiter carries its own `session` band over its own capsule lifetime, so a saturated channel and a saturated pool read apart instead of summing into one number. `submit(kernel, *args)` injects the trace carrier, drives the arm's executor with `traced_kernel`, and settles the future on a `WORKER_BAND`-bounded thread with `abandon_on_cancel=True` — the band token bounds pool in-flight and settle threads in one acquisition, and a cooperative cancel abandons the settle while the worker runs to completion under the pool's reaper, exactly the `COOPERATIVE` law; on the terminal arm the same cancel instead escalates through `ProcessFuture.cancel`, pebble terminating the RUNNING task so the killable slot reclaims immediately rather than holding to the wall-clock kill. In-band worker-death retry reads `kernel.retry` before the terminal lift, and each attempt re-acquires the executor, so a `TerminatedWorkerError` re-submission genuinely lands on the respawned pool and a non-idempotent kernel never re-runs.
- Law: the remote crossing seals `(boot, carrier, kernel, args)` as the one cloudpickle blob the TERMINAL arm already ships — stdin in, one pickled verdict out — and the hop honours `kernel.deadline` itself since no lane wraps a direct fleet submit: the deadline scope sits OUTSIDE the retry and rails the typed `deadline` fault when tripped, the in-flight session's cancellation cleanup escalating on the way out — `TERMINAL` kills the far process mid-kernel, `COOPERATIVE` terminates the session so the channel reaps the floor — while a torn or empty verdict raises `ConnectionError` into the `SSH` retry band. `boot` is the parent-captured worker floor and may be `None` only when the pebble initializer already installed it. In-band retry re-keys to `KIND_POLICY[REMOTE].restart` under the kernel's `idempotent` declaration — executor-death names are meaningless across a channel — and `Wire.SHARED_MEMORY` is refused as a typed `config` fault because a span name never resolves across hosts; silently downgrading a declared zero-copy crossing to a copy betrays the declaration.
- Auto: every process arm pins an explicit spawn-family start method — `get_context("spawn")` on pebble, the loky spawn-based `"loky"` method on both loky arms — so crossing semantics never fork by platform default; every process arm's initializer is the one `_worker_boot` — device visibility pins first, the fidelity latch re-arms per its `KIND_POLICY` row, and the parent-captured `WorkerBoot` installs telemetry, instruments, and the profiler post-spawn under `WORKER_SIGNAL_PROFILE` with the exit drain registered — while the capsule latches parent-side at construction, so a worker raise re-crosses with frames intact and the settle-side unpickle resolves them; every lifecycle transition self-emits its `PoolReceipt` — the phase methods through the `@receipted` harvest, the SPAWNED mint at `acquire`'s membership guard — so pool chronology rides the one receipt rail. A worker-local cache or native handle is a module-level `@cache` in the worker-body module, so the warm prime pays it once per spawned worker and a reaped worker's respawn re-pays it. Elasticity is the substrate's own pair — the idle reap shrinks a quiet pool and a later submit respawns to the cap — so a sizing knob never lands. Worker-death evidence is typed per arm — loky raises `TerminatedWorkerError`/`BrokenProcessPool` on the pending future, pebble raises `ProcessExpired` (pid, exitcode) or `TimeoutError` on the deadline kill — and the faults pool-death row lands each on the `resource` case while the deadline kill converts to the `deadline` case, never retried because the `WORKER` row's target excludes it; loky's exit-code forensics never land — re-acquisition replaces the corpse before a forensic read, so death evidence is the typed raise and pebble's pid/exitcode.
- Law: the worker flush law is exit-owned — `_worker_boot` registers the telemetry drain then the profiler stop through `atexit`, so a graceful settle (`shutdown(wait=True)`, pebble `close`/`join`, the remote floor's process exit) drains every worker's buffered tail, a roll's double-buffer drains the stale arm's workers the same way, and only the kill paths (`kill_workers`, pebble `stop`, the remote `abort`) forfeit at most one `WORKER_SIGNAL_PROFILE` export window. The boot is captured parent-side as data off `Telemetry.receipt`/`Profiles.receipt`, so a silent parent spawns silent workers, no endpoint knob rides the pool surface, and the worker geometry keeps the HTTP transport the fork fence requires — the gRPC egress row stays structurally refused on every spawned floor.
- Receipt: `PoolReceipt` carries `phase`, `kind`, `enforcement`, and `workers` — lifecycle evidence, never task outcomes; task outcomes stay the lane's `DrainReceipt`.
- Growth: a new executor arm is one constructor match arm keyed by `(WorkerKind, Enforcement)`; a new fleet host is one `RemoteEndpoint` value and a new accelerator one `Device` value, each acquiring its own arm at zero new surface; a new lifecycle phase is one `PoolPhase` member; a new warm-state obligation is one initializer fold; a new worker-boot fact is one `WorkerBoot` field the one initializer reads.
- Boundary: pools serve the lanes offload hop, the daemon drain fold, the fleet and device consumers, and the supervisor — a consumer never imports an executor class, holds a future, or sizes a pool; sizing derives from `loky.cpu_count(only_physical_cores=True)`, which already folds the `LOKY_MAX_CPU_COUNT` deploy override and the cgroup budget, so a cgroup-capped batch arm is deploy placement with zero new surface — the capped daemon's workers inherit its cgroup and every pool self-sizes to the quota, scheduling class and affinity riding the same deploy custody, never a kind — and `WORKER_BAND` bounds in-flight admission above the pool, refusing burst past physical cores. `REMOTE` and `GPU` are caller placement, never trait-derived — the lanes offload never routes to them, and a consumer acquires the arm with its `RemoteEndpoint` or `Device` exactly as trait declaration is consumer domain knowledge on the crossing; priority is the same placement axis — a latency class acquires its own arm key, never a queue-discipline knob. Fan-out modality stays the lane's `drain` — the pools expose `submit` alone, never a second `map`, stream, or priority surface. `Kernel.of` is the whole payload-classification surface and `ShmSpan` the one out-of-band buffer channel, so no per-object wrap, pickler swap, or reducer registration exists beside them; the span bracket itself is the lane's, so `submit` takes arguments already exported and a direct-submit consumer brackets its own call with `exported`/`released` rather than expecting the pool to read `kernel.wire`. Host-side admission sits at that same seam under the same law — the lane's `offload` runs `admitted` once for every crossing it drives, and a direct-submit consumer runs it itself, so the gate is never paid twice on the terminal guest route.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import atexit
import os
from collections.abc import Iterable
from concurrent.futures import Future
from contextlib import ExitStack
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
from expression import Error, Nothing, Option, Result
from opentelemetry import propagate
from opentelemetry.exporter.otlp.proto.http import Compression
from opentelemetry.sdk.resources import SERVICE_INSTANCE_ID, SERVICE_NAME, SERVICE_NAMESPACE, Resource

from rasm.runtime.admission import RuntimeContext, RuntimeProfile
from rasm.runtime.faults import SCHEMA_URL, BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.receipts import OPEN, Receipt, Signals, receipted
from rasm.runtime.resilience import guard
from rasm.runtime.roots import RemoteEndpoint
from rasm.runtime.telemetry import NAMESPACE, SignalProfile, Telemetry

# every CROSSING-region owner — the kind/trait/shipping/wire vocabulary, `Kernel`, the policy tables, the crossing gates, and their
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

    @property
    def env(self) -> dict[str, str]:
        # daemon-spawn seam, spelled as `Device.env` is because it answers the same question: a supervised child reads
        # the standard SDK variable at its own admission, so the parent's effective endpoint crosses by environment
        # beside the device visibility, never a re-plumbed setting.
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
    # Readmission rebuilds the axis row both emission gates read from the preset NAME the boot capture carried, so no
    # context object crosses the pickle seam and the telemetry and profile installs share one boot correlation.
    ctx = RuntimeContext.admit(boot.profile)
    if boot.otel is not None:
        atexit.register(Telemetry.shutdown)
        # budget threads off the install receipt's EFFECTIVE geometry, never the requested row, so the worker enrolls
        # against the ceiling its own pipeline fixed; an unthreaded `install()` discards the profile's own budget.
        installed = Telemetry.install(ctx, boot.otel, resource=worker_resource(boot.kind), signal_profile=WORKER_SIGNAL_PROFILE)
        Metrics.install(budget=installed.signal_profile.cardinality_budget)
    if boot.pyroscope is not None:
        atexit.register(Profiles.shutdown)
        Profiles.install(ctx, boot.pyroscope, tags={"worker.kind": boot.kind.value}, tenant=boot.tenant)


# --- [SERVICES] -------------------------------------------------------------------------


class WorkerPool:
    # one live capsule per arm key, module-registry memoized; `_live` is the operation-local mutable registry this owner reads and retires.
    _live: Final[dict[ArmKey, "WorkerPool"]] = {}
    # band-occupancy custody, lifetime-bound at BOTH ends exactly as the metrics owner rules: the process-global
    # `WORKER_BAND` mints its `rasm.band.in_flight` series when the first arm registers and leaves the probe map when
    # the last one retires, so a level nobody holds publishes no point. One registration for the one band — a per-arm
    # probe would sum the SAME borrowed count once per arm and report a saturation no limiter is carrying.
    _probes: Final[ExitStack] = ExitStack()

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
                # limiter bounding fleet in-flight. That limiter is a bound of its own, so it publishes its own `session`
                # band: fleet saturation and pool saturation answer different operator questions, and each endpoint arm
                # holds a DISTINCT limiter, so concurrent arms legitimately sum into the one band exactly as concurrent
                # lanes do — the double-count law binds one limiter read twice, never two limiters read once each.
                # Custody is this capsule's own lifetime, closed on both teardown paths so a rolled-out arm leaves none.
                self._conn: asyncssh.SSHClientConnection | None = None
                self._dial = anyio.Lock()
                self._sessions = CapacityLimiter(workers)
                self._occupancy = ExitStack()
                self._occupancy.enter_context(Metrics.occupied(lambda: self._sessions.borrowed_tokens, band="session"))
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
        # Tracker-format seat: loky subclasses the stdlib `multiprocessing.resource_tracker` writer while its own
        # tracker-process `main` colon-splits ASCII lines, and that writer emits JSON records by default — unpinned, every
        # register/unregister raises `ValueError: unknown resource type` inside the tracker child, leaked semlocks and
        # temp folders stop reclaiming, and a long-lived daemon floods child stderr (probe: 36 raises and an unreclaimed
        # folder unpinned; zero stderr and clean reclaim pinned). The pin rides the WRITER because the tracker child is
        # spawned from `main.__module__` across an exec seam no parent-side rebind crosses; loky's rostered names
        # (semlock, folder, file) never carry the newline that falls back to JSON. The seat retires whole when an
        # upstream release ships a JSON-reading tracker (joblib/loky#624).
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
        if key not in cls._live:  # membership guard precedes the mint — an effectful setdefault spawns an executor per call
            if not cls._live:  # first arm of the interpreter opens the band's series; the last one to retire closes it
                cls._probes.enter_context(Metrics.occupied(lambda: WORKER_BAND.borrowed_tokens, band="worker"))
            cls._live[key] = cls(kind, enforcement, loky.cpu_count(only_physical_cores=True), placement)
            Signals.emit(PoolReceipt(phase=PoolPhase.SPAWNED, kind=kind, enforcement=enforcement, workers=0), OPEN)
        return cls._live[key]

    @classmethod
    def live(cls, kind: PoolKind, enforcement: Enforcement = Enforcement.COOPERATIVE, placement: Placement = None) -> "Option[WorkerPool]":
        # same `(kind, enforcement, placement)` triple every other entry takes, keyed through the one `_placed` fold —
        # a second placement spelling here would leave one surface asking callers which of two shapes each verb wants.
        return Option.of_optional(cls._live.get((kind, enforcement, _placed(placement))))

    def alive(self) -> bool:
        # pebble publishes `active`; the loky arms self-heal per acquisition, so their capsules are live while registered; the
        # remote arm reads channel state — un-dialed is live, a closed channel reads DEAD so the supervisor rolls a fresh dial.
        if self._kind is WorkerKind.REMOTE:
            return self._conn is None or not self._conn.is_closed()
        return self._pebble.active if self._enforcement is Enforcement.TERMINAL else True

    def pids(self) -> frozenset[int]:
        # TOTAL across every arm — each substrate publishes its own live {pid: worker} map (both loky arms `_processes`,
        # pebble's pool manager `worker_manager.workers`), so a supervisor weighs exactly the workers an arm owns and
        # never a complement over the process tree, which folds every UNNAMED arm's workers into every other unnamed
        # arm's verdict the moment two terminal arms coexist. Remote workers are far-host processes no local pid names,
        # so the fleet arm names an empty set. The read is observation-only: `_executor()` is the acquisition seam that
        # mints or replaces a pool as a side effect, so the probe reads the instance the last acquisition landed.
        match self._kind, self._enforcement:
            case (WorkerKind.REMOTE, _):
                return frozenset()
            case (_, Enforcement.TERMINAL):
                # double-private reach, kept deliberately: pebble publishes no worker roster at all, and the alternative
                # — a process-tree complement — is the exact unscoped read this probe exists to replace, so the private
                # walk buys arm scoping nothing public can. It breaks the moment pebble renames `_pool_manager` or moves
                # its worker map off `worker_manager`, which surfaces as an AttributeError on the FIRST probe cycle after
                # a bump, never as a silent mis-verdict; the repair is a public roster row at the pebble catalogue.
                return frozenset(self._pebble._pool_manager.worker_manager.workers)
            case (_, Enforcement.COOPERATIVE):
                held = self._loky if isinstance(self._placement, Device) else self._held
                return frozenset(held._processes) if held is not None else frozenset()
            case _ as unmatched:
                assert_never(unmatched)

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
        keyed = KIND_POLICY[WorkerKind.REMOTE].restart if kernel.idempotent else Nothing
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
                self._occupancy.close()  # the session band retires with the arm that bounded it; a second close unwinds nothing
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
        # the memo drops with the drained executor: a DRAINING capsule left registered answers every later `acquire`
        # with an arm whose submit fence refuses forever, so the key would brick on the first graceful teardown. A
        # rolled-out stale capsule is already unregistered, so the gate skips it and the fresh arm keeps its key.
        if WorkerPool._live.get(self._key) is self:
            WorkerPool._live.pop(self._key)
            self._retired()
        return PoolReceipt(phase=self._phase, kind=self._kind, enforcement=self._enforcement, workers=self._workers)

    @staticmethod
    def _retired() -> None:
        # the band's series retires with its LAST holder — a zero-seeded level nobody bounds reads identically to a live
        # limiter sitting empty, which is exactly the distinction the occupancy series exists to answer.
        if not WorkerPool._live:
            WorkerPool._probes.close()

    @receipted(OPEN)
    def retire(self) -> "PoolReceipt":
        # terminal teardown: the memo drops so the next acquire re-spawns a fresh arm; kill_workers reclaims a stuck loky
        # pool, and abort tears the remote channel without the close handshake a wedged far host never answers.
        match self._kind, self._enforcement:
            case (WorkerKind.REMOTE, _):
                if self._conn is not None:
                    self._conn.abort()
                self._occupancy.close()
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
            self._retired()
        return PoolReceipt(phase=self._phase, kind=self._kind, enforcement=self._enforcement, workers=0)
```

## [04]-[LEASE]

- Owner: `LeaseSession` composes one `LeasePort` with an admitted `BackendGeneration` and one existing `WorkerPool`, so a distributed work claim reuses the whole crossing this page already owns — pool, kernel value, band, and retry row — and adds only the fence. It mints no worker, no executor, and no second band.
- Cases: `LeaseOp` closes claim, heartbeat, settle, and drain behind one provider-polymorphic `apply`, and `LeaseVerdict` closes what a provider may answer, so every arm this owner does not expect is a typed refusal naming the verb rather than an unhandled shape.
- Entry: `LeaseSession.run(kernel, evidence)` is the one work entry — claim, race, settle in one call — and `drain` the one retirement. A caller hands the kernel it would have submitted to the pool directly plus the projection that turns its result into settlement bytes, so adopting the lease costs no change to the kernel and no second submit surface stands beside `WorkerPool.submit`.
- Auto: the heartbeat and the work run as siblings under ONE task group whose first message cancels the other, so a lost lease preempts the crossing under the kernel's own declared enforcement instead of letting a worker finish against a fence it no longer holds; the stream is depth-1 because exactly one message decides the race, and the loser's send is cancelled with its scope rather than buffered into a slot nobody reads. Renewal cadence derives from the fence the provider actually granted — `WorkLease.ttl` times the policy fraction — so a heartbeat can never silently outrun the lease it defends the way a free absolute interval does.
- Law: `LeaseDemand` carries generation, worker identity, and capability evidence; `WorkLease` returns one fenced token, its payload, and the provider's own fence lifetime. Generation crosses as the admission owner's `Digest128`, so the claim, the returned fence, and the drift compare read one 32-hex domain.
- Law: settlement is TOTAL over every terminal — success carries a kernel content identity under the seed-zero parity contract, a kernel fault and a raised evidence projection each carry their typed fault tag — so the provider's token fence retires on every path and no terminal leaves a live lease waiting out its own lapse; the fence is what keeps a settlement from landing against a lease another worker now holds.
- Law: provider admission closes before pool drain, so no claim enters after local execution begins settling.
- Packages: AnyIO structured concurrency, `expression` rails, `msgspec` unions, and runtime `ContentIdentity`.
- Growth: a provider implements one port; a lease transition is one `LeaseOp` case with its `LeaseVerdict` answer and one `_verdict` row; worker execution and supervision remain unchanged.
- Boundary: adapters own PostgreSQL, broker, or service calls; this owner carries no SQL, polling protocol, or provider transaction.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from typing import Annotated, Literal, Protocol, assert_never

import anyio
from expression import Error, Ok, Result, Some, case, tag, tagged_union
from msgspec import Meta, Struct

from rasm.runtime.admission import BackendGeneration, Digest128
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.identity import U128, ContentIdentity

# `Kernel`, `WorkerPool`, and `PoolReceipt` are the [02]-[CROSSING] and [03]-[POOL] owners of this same module.

# --- [TYPES] ----------------------------------------------------------------------------


# the race carrier: exactly one of the two siblings ever sends, so the tag names which one decided and the payload is
# already the shape that arm terminates on — a shared nullable slot would leave the reader re-deriving the winner.
type LeaseRace[T] = tuple[Literal["work"], RuntimeRail[T]] | tuple[Literal["lease"], BoundaryFault]

# --- [MODELS] ---------------------------------------------------------------------------


class LeaseDemand(Struct, frozen=True):
    # Generation crosses as the admission owner's own 32-hex domain, so a fence token and a demand read one spelling
    # and a truncated or re-based digest fails at construction rather than at the provider's key compare.
    generation: Digest128
    worker: str
    capabilities: frozenset[str]


class LeasePolicy(Struct, frozen=True, gc=False):
    # renewal cadence as a FRACTION of the provider's own fence lifetime, never a free absolute: a heartbeat spelled
    # independently of the TTL it defends can silently outrun it, and a lapsed fence is the exact state the race exists
    # to preempt. Bounded strictly inside (0, 1), so a renewal always lands while the fence this session holds is live.
    renew: Annotated[float, Meta(gt=0.0, lt=1.0)] = 0.5


class WorkLease(Struct, frozen=True):
    # the provider states its OWN fence lifetime beside the token, so the session derives its cadence instead of
    # carrying a second number nothing reconciles against the fence it is meant to hold open.
    token: str
    generation: Digest128
    payload: bytes
    ttl: Annotated[float, Meta(gt=0.0)]


@tagged_union(frozen=True)
class LeaseSettlement:
    # the success arm carries the identity owner's OWN width — a bare `int` erases the unsigned 128-bit domain a
    # `ContentKey.value` occupies, and this settlement crosses a provider wire where an unfloored integer decodes
    # anything the transport hands it.
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
    async def apply(self, operation: LeaseOp, /) -> RuntimeRail[LeaseVerdict]: ...


# --- [SERVICES] ---------------------------------------------------------------------------


class LeaseSession(Struct, frozen=True):
    # `Struct(frozen=True)` like every other record on this page: the session is a bound composition value a caller
    # holds, and a second record kind here would leave one page carrying two owner spellings for one concept.
    port: LeasePort
    pool: WorkerPool
    generation: BackendGeneration
    worker: str
    policy: LeasePolicy

    async def run[T](self, kernel: Kernel[T], evidence: Callable[[T], bytes], /) -> RuntimeRail[T]:
        # claim, race, settle as one bind chain: every provider answer routes through `_verdict`, so the four call
        # sites share one refusal law and none re-spells which verdicts a verb admits.
        demand = LeaseDemand(generation=self.generation.generation, worker=self.worker, capabilities=self.generation.observed.capabilities)
        claimed = _verdict("claim", await self.port.apply(LeaseOp(claim=demand)), "claimed").bind(
            lambda verdict: Ok(verdict.claimed)
            if verdict.claimed.generation == demand.generation
            else Error(BoundaryFault(config=("workers.lease", "generation-drift")))
        )
        match claimed:
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=lease):
                return await self._settled(lease, evidence, await self._raced(kernel, lease))
            case _ as unreachable:
                assert_never(unreachable)

    async def _raced[T](self, kernel: Kernel[T], lease: WorkLease) -> LeaseRace[T]:
        # ONE task group whose first message decides: the work sibling and the heartbeat sibling race, the winner's
        # send lands, and the group cancel preempts the loser mid-flight — a lost lease therefore stops the crossing
        # under the kernel's declared enforcement instead of letting a worker finish against a fence it no longer
        # holds. Depth 1 because exactly one message is ever read; the loser's send dies with its scope.
        send, receive = anyio.create_memory_object_stream[LeaseRace[T]](1)

        async def worked() -> None:
            await send.send(("work", await self.pool.submit(kernel, lease.payload)))

        async def pulsed() -> None:
            while True:  # Exemption: the heartbeat is a standing rhythm its owning group cancels; no expression form awaits a cadence.
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

    async def _settled[T](self, lease: WorkLease, evidence: Callable[[T], bytes], raced: LeaseRace[T]) -> RuntimeRail[T]:
        # settlement is the terminal for BOTH work arms — a failure settles its typed fault tag exactly as a success
        # settles its content identity, so the provider's fence retires either way and a crashed kernel never leaves a
        # lease live until its heartbeat lapses. Only the lease arm skips it: there is no fence left to settle against.
        match raced:
            case ("lease", fault):
                return Error(fault)
            case ("work", outcome):
                return await self._reported(lease, outcome, self._sealed(outcome, evidence))
            case _ as unreachable:
                assert_never(unreachable)

    def _sealed[T](self, outcome: RuntimeRail[T], evidence: Callable[[T], bytes]) -> LeaseSettlement:
        # TOTAL over every terminal, the raised evidence projection included: the success arm keys the projected bytes
        # under the seed-zero parity contract, so two workers settling one deterministic kernel report the identical
        # identity and a provider dedups a redelivery by value, while a projection that raises settles its own fault tag
        # on the `failed` arm. Railing that raise instead short-circuits the report and leaves precisely the state the
        # fence exists to prevent — a live lease no worker will ever settle, held until it lapses.
        match outcome:
            case Result(tag="error", error=fault):
                return LeaseSettlement(failed=fault.tag)
            case Result(tag="ok", ok=value):
                return boundary(
                    "workers.lease.evidence",
                    lambda: LeaseSettlement(succeeded=ContentIdentity.key("worker-settlement", evidence(value), seed=Some(0)).value),
                ).default_with(lambda refused: LeaseSettlement(failed=refused.tag))
            case _ as unreachable:
                assert_never(unreachable)

    async def _reported[T](self, lease: WorkLease, outcome: RuntimeRail[T], settlement: LeaseSettlement) -> RuntimeRail[T]:
        # one settle hop, no second refusal ladder: `_sealed` is total, so the only rail left here is the provider's own.
        return _verdict("settle", await self.port.apply(LeaseOp(settle=(lease, settlement))), "settled").bind(lambda _settled: outcome)

    async def drain(self) -> RuntimeRail[PoolReceipt]:
        # provider admission closes BEFORE the pool drains, so no claim enters while local execution settles.
        match _verdict("drain", await self.port.apply(LeaseOp(drain=self.worker)), "drained"):
            case Result(tag="error") as refused:
                return refused
            case _:
                return Ok(await self.pool.drain())


# --- [OPERATIONS] -----------------------------------------------------------------------


def _verdict(verb: str, answered: RuntimeRail[LeaseVerdict], expected: str) -> RuntimeRail[LeaseVerdict]:
    # ONE verdict law every lease verb reads: the expected arm passes, `lost` and `empty` are the provider's own
    # resource facts, and any other arm is a protocol refusal naming the verb that received it. Spelling this per call
    # site left four near-identical match ladders whose `Result(tag="ok")` catch-all silently absorbed a verdict a
    # later provider case would add — the one class this closed answer set exists to make unspellable.
    match answered:
        case Result(tag="error") as refused:
            return refused
        case Result(tag="ok", ok=verdict) if verdict.tag == expected:
            return Ok(verdict)
        case Result(tag="ok", ok=LeaseVerdict(tag="lost", lost=reason)):
            return Error(BoundaryFault(resource=("workers.lease", reason)))
        case Result(tag="ok", ok=LeaseVerdict(tag="empty")):
            return Error(BoundaryFault(resource=("workers.lease", "empty")))
        case Result(tag="ok", ok=verdict):
            return Error(BoundaryFault(config=("workers.lease", f"{verb}-verdict:{verdict.tag}")))
        case _ as unreachable:
            assert_never(unreachable)
```

## [05]-[SUPERVISION]

- Owner: `Supervisor` binds the probe evidence, the restart rows, and the health projection into the one actuator loop the branch lacked: every ingredient existed — psutil probes, retry backoff, health status, worker-death markers — and this owner closes them. A `SupervisionPolicy` row per supervised subject carries the probe ceilings and the windowed restart budget; verdicts are data the loop folds, never inline judgment.
- Cases: `Verdict` closes the probe outcomes — `LIVE`, `UNMEASURED` (every column refused its read), `DEGRADED` (a `Breach` row tripped: rss over budget, involuntary context-switch storm, socket-table growth), `DEAD` (child gone, pool retired, channel closed) — and `_actuate` maps each onto its actuation: `LIVE` re-arms the probe, `UNMEASURED` emits its evidence and actuates nothing, `DEGRADED` rolls the arm, `DEAD` flips the subject down and re-spawns under the kind's restart row with stamina backoff. Budgets are windowed: `restarts` actuations inside `window` seconds park the subject `NOT_SERVING` until the window drains, so a crash storm holds down instead of thrashing.
- Law: a probe answers `Weighed` — the verdict plus the columns that produced it — never a collapsed bool, and every ceiling column is optional because a `PROCESS_FAULTS` refusal is a measurement nobody took: a zero there reads as a healthy worker and yields exactly the false `LIVE` this actuator exists to catch, so an all-refused read is `UNMEASURED` and actuates nothing rather than advertising health or spending the restart budget on evidence about the probe. Fences are per COLUMN, so a denied socket table never darkens an rss the same subject measured, and the arm-level reading is the PEAK of each column across the arm's workers folded through the one `_judged` law. The actuation receipt carries those columns beside its own fact — `parked` on a budget hold, `restarted` on a granted one — so an operator reads which ceiling moved, by how much, and what the actuator did in one shape.
- Entry: `Supervisor.watch(group)` starts one probe loop per supervised subject inside the caller's task group — the daemon composition root's supervision group, never a private loop — each cycle fenced so a probe or actuation raise emits one rejected receipt and the rhythm survives, and the whole weighing crosses to a thread under this owner's OWN band so a slow psutil read never parks the loop and never borrows the pool's width. Probes are arm-scoped: a `DAEMON` charge reads its own child handle — the two `oneshot`-batched ceilings in one collection and the socket count on its own syscall, since that member is not in the batch — and a pool charge reads capsule presence and the arm's own `alive()` before weighing exactly the pids that arm names, so a daemon child, a sibling arm's worker, and an unrelated grandchild all stay outside this subject's verdict by construction rather than by a complement two unnamed arms would share; a `REMOTE` charge reads channel liveness alone and carries `held`/`alive` as its columns, resource ceilings belonging to the far host's own supervisor and fleet saturation to the arm's own `session` band, while a `GPU` charge weighs host RSS alone — device-memory evidence stays the kernel's own receipt, unobservable through a psutil scan. The supervision band publishes its own occupancy for exactly the watched lifetime, so a probe rhythm queueing behind slow syscalls reads as saturation rather than as silence. Verdicts project onto the serve owner's per-service flip — the injected awaited `ServerHost.status` coroutine — so any `DEAD` subject flips its service `NOT_SERVING` and recovery flips it back, the health poller the estate shipped only the server half of; the flip writes on a CHANGE in the advertised state alone, because the servicer pushes to every live watcher on each set and a standing verdict re-asserted per interval is a watcher storm carrying no news. The `verdicts` accessor publishes the same last-verdict state as data for the bundle capsule, so no second verdict surface exists beside the flip and the projection.
- Daemon kind: a `DAEMON` worker is a supervised long-lived child — `psutil.Popen` fuses the subprocess handle with the probe surface, the spawn environment forwards the parent's effective OTLP endpoint through `WorkerBoot.env` so the child's own composition root installs against the same collector, `terminate()` then `kill()` after the grace window is the stop escalation, restart rides the `SPAWN` row targeting spawn transients, and an empty spawn command is a config refusal that parks the subject down; a daemon's readiness is its next `LIVE` verdict, never a sleep. Every reap runs `psutil.Popen.wait(timeout=)` inside the banded thread rather than an enclosing cancel scope, because the scope form bounds the AWAIT while the abandoned thread stands in the syscall and holds its band token for the process's life; the timeout form always returns the thread, so the one `_reaped` helper serves the restart escalation and the stop sweep alike. `Supervisor.stop()` runs that same escalation over every surviving child as the serve drain fold's daemon row, so a child never outlives the daemon that spawned it.
- Growth: a new probe dimension is one `SupervisionPolicy` ceiling field, one `Breach` member, and one `_judged` row — the arm fold, the receipt columns, and the peak projection each name themselves from it; a new actuation is one `_actuate` arm; a new supervised subject is one `Supervisor.watch` registration.
- Boundary: the supervisor actuates pooled arms — device arms included — remote channels, and daemon children only — `ChargeKind` seals that subject set by construction — and it never restarts the serve host, never owns the signal seam (`transport/serve#ENTRY`'s), and never emits health protocol wire (the serve owner's `HealthServicer` is the sole advertiser). Probe evidence emits through the contributor port under the `OPEN` policy.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
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

from rasm.runtime.faults import BoundaryFault, async_boundary
from rasm.runtime.receipts import OPEN, PROCESS_FAULTS, Receipt, Signals
from rasm.runtime.resilience import guard

# every CROSSING- and POOL-region owner the supervision code names resolves in this same module's earlier regions — no cross-module import.

# --- [TYPES] ----------------------------------------------------------------------------


class Verdict(StrEnum):
    # ascending severity; UNMEASURED sits between health and harm because a refused read is a fact about the PROBE,
    # so it must never advertise serving and must never spend the restart budget either.
    LIVE = "live"
    UNMEASURED = "unmeasured"
    DEGRADED = "degraded"
    DEAD = "dead"


class Breach(StrEnum):
    RSS = "rss"
    SWITCHES = "switches"
    SOCKETS = "sockets"


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
    socket_ceiling: int = 1_024  # open-socket growth marks a leaking long-lived child whose rss and switch columns still read clean
    grace: float = 5.0
    restarts: int = 3
    window: float = 300.0  # rolling budget window: `restarts` actuations inside it park the subject down until it drains


class Weighed(Struct, frozen=True, gc=False):
    # one probe reading carrying the evidence that produced it. Every ceiling column is `int | None` because a
    # refused read is a measurement NOBODY TOOK — an AccessDenied socket table, a pid that exited between naming and
    # reading — and a zero there reads as a healthy worker, which is precisely the false LIVE this actuator exists
    # to catch. `breached` names WHICH ceilings tripped, so the receipt says why rather than that something did, and
    # `held`/`alive` carry the fleet arm's channel columns, whose subject owns no local process to weigh.
    verdict: Verdict
    rss: int | None = None
    switches: int | None = None
    sockets: int | None = None
    held: bool | None = None
    alive: bool | None = None
    breached: frozenset[Breach] = frozenset()

    def facts(self) -> dict[str, object]:
        # receipt columns spell absence by OMITTING the key exactly as the branch's optional-dimension law rules: an
        # unmeasured ceiling and a measured floor are different facts, and a reader that cannot separate them reads
        # the wrong incident. `verdict` is the one column always present — every reading answers it.
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
    # one supervised subject: a pooled arm, a daemon child, or a remote channel; `command` spawns the DAEMON kind and stays
    # empty on pool charges, `placement` names the REMOTE endpoint or GPU device and stays None on plain local charges.
    # `policy.subject` is the health key the flip advertises, so it spells the registered gRPC service name exactly —
    # a drifted subject flips a phantom key.
    policy: SupervisionPolicy
    kind: ChargeKind
    enforcement: Enforcement = Enforcement.COOPERATIVE
    command: tuple[str, ...] = ()
    placement: Placement = None


# --- [OPERATIONS] -----------------------------------------------------------------------


def _reading[T](read: Callable[[], T]) -> T | None:
    # one fenced native read per COLUMN, never per probe: a `PROCESS_FAULTS` refusal spells UNMEASURED for its own
    # column alone, so a denied socket table never darkens an rss the same subject measured cleanly, and the pid lift
    # rides this same fence, closing the naming-to-reading race end to end.
    with suppress(*PROCESS_FAULTS):
        return read()
    return None


def _batched(proc: psutil.Process) -> tuple[int, int]:
    # exactly the two ceilings `oneshot` genuinely batches — both resolve off the one cached task/kinfo collection, so
    # the pair costs one collection. `net_connections` is NOT in that cache and takes its own socket-table syscall, so
    # folding it into this block would read as batched while paying full price on every probe cycle of every worker.
    with proc.oneshot():
        return proc.memory_info().rss, proc.num_ctx_switches().involuntary


def _judged(rss: int | None, switches: int | None, sockets: int | None, policy: SupervisionPolicy) -> Weighed:
    # ceilings as ROWS: each weighs only a column that measured, so a new probe dimension is one policy field and one
    # row here. A subject whose every column refused stays UNMEASURED rather than passing as LIVE — the verdict a
    # zero-filled reading forges, and the one an operator would act on as health.
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
    # arm-level projection over its workers: each column carries the PEAK measured value, so the arm reads as its
    # worst worker rather than an average that buries the one against its ceiling, and the SAME `_judged` fold then
    # decides the arm — one ceiling law, never a second verdict rank beside it. An arm whose every worker refused its
    # read, or whose worker set emptied between naming and reading, measures nothing and stays UNMEASURED.
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
        self._served: dict[str, bool] = {}  # last ADVERTISED health per subject; the flip writes only where this moves
        self._children: dict[str, psutil.Popen] = {}  # DAEMON handles: subprocess control fused with the probe surface
        self._stamps: dict[str, tuple[float, ...]] = {}  # per-subject actuation stamps the windowed budget folds
        # supervision's own named band, sized by its owner exactly as the branch rules: every charge may probe or reap
        # concurrently and none queues, while the pool's `WORKER_BAND` keeps its whole width for crossings — a daemon
        # `wait` parked for its grace window on the pool band would silently shrink pool in-flight for that whole span.
        self._band = CapacityLimiter(max(1, len(charges)))
        # this band's occupancy custody: opened with the probe rhythm at `watch` and retired at `stop`, so a supervisor
        # holding no cycles publishes no level and a saturated supervision band reads apart from a saturated pool.
        self._probes = ExitStack()

    def _weighed(self, subject: psutil.Process | int, policy: SupervisionPolicy) -> Weighed:
        # one process reading as typed evidence over either subject shape — a live handle (the DAEMON child) or a pid
        # an arm named. The socket count takes its own syscall and its own fence outside the batch, because
        # AccessDenied on the socket table is the ordinary answer for a process this one does not own, and that
        # refusal must cost only its own column.
        proc = _reading(lambda: psutil.Process(subject) if isinstance(subject, int) else subject)
        if proc is None:
            return _judged(None, None, None, policy)
        batched = _reading(lambda: _batched(proc))
        rss, switches = batched if batched is not None else (None, None)
        return _judged(rss, switches, _reading(lambda: len(proc.net_connections())), policy)

    def _probe(self, charge: Charge) -> Weighed:
        # blocking by construction — psutil ceiling reads are syscalls — so `_cycle` runs this off the loop under the
        # supervision band; only the actuation, which awaits the flip and the roll, belongs on it.
        match charge.kind:
            case WorkerKind.DAEMON:
                handle = self._children.get(charge.policy.subject)
                if handle is None or not handle.is_running():
                    return Weighed(verdict=Verdict.DEAD, held=handle is not None, alive=False)
                return self._weighed(handle, charge.policy)
            case WorkerKind.REMOTE:
                # channel liveness is the whole remote probe — rss, switch, and socket ceilings belong to the far host's
                # own supervisor, unobservable through a local psutil scan — and the two channel columns ride the
                # verdict, so an arm that never registered and a channel that closed read apart at the receipt instead
                # of collapsing into one DEAD. Fleet saturation is the arm's own `session` band, never a ceiling here.
                arm = WorkerPool.live(charge.kind, charge.enforcement, charge.placement)
                alive = arm.map(lambda pool: pool.alive()).default_value(False)
                return Weighed(verdict=Verdict.LIVE if alive else Verdict.DEAD, held=arm.is_some(), alive=alive)
            case kind:
                arm = WorkerPool.live(kind, charge.enforcement, charge.placement)
                if arm.is_none() or not arm.value.alive():
                    return Weighed(verdict=Verdict.DEAD, held=arm.is_some(), alive=False)
                # arm-scoped weighing off the arm's OWN named pids — no process-tree walk, no exclusion set, and no
                # complement, so a daemon child, a sibling arm's worker, and an unrelated grandchild can none of them
                # reach this subject's verdict, and the read costs the arm's worker count rather than the whole tree.
                return _folded(Block.of_seq(self._weighed(pid, charge.policy) for pid in arm.value.pids()), charge.policy)

    def _budgeted(self, policy: SupervisionPolicy) -> bool:
        # windowed restart budget: stale stamps drop and ONLY a granted actuation stamps in, so a parked subject's
        # window drains on its own instead of self-refreshing on every parked probe cycle.
        now = time.monotonic()
        fresh = tuple(stamp for stamp in self._stamps.get(policy.subject, ()) if now - stamp < policy.window)
        granted = len(fresh) < policy.restarts
        self._stamps[policy.subject] = (*fresh, now) if granted else fresh
        return granted

    async def _advertise(self, subject: str, serving: bool) -> None:
        # the flip writes only where the ADVERTISED state moves: the health servicer pushes a response to every live
        # watcher on each `set`, so re-asserting a standing verdict once per probe interval per subject is a watcher
        # storm carrying no news, and a recovery or a park is exactly the transition a watcher is waiting on.
        if self._served.get(subject) is not serving:
            self._served[subject] = serving
            await self._flip(subject, serving)

    async def _actuate(self, charge: Charge, weighed: Weighed) -> None:
        # every arm emits the SAME evidence columns beside its own actuation fact, so an operator reads which ceiling
        # moved, by how much, and what the actuator did about it from one receipt shape.
        subject = charge.policy.subject
        self._verdicts[subject] = weighed.verdict
        match weighed.verdict:
            case Verdict.LIVE:
                await self._advertise(subject, True)
            case Verdict.UNMEASURED:
                # nothing measured, so nothing is claimed: no advertise, no restart, no budget spend. A reading no
                # probe took is evidence about the PROBE — a privilege refusal, a worker set that vanished mid-read —
                # and restarting on it thrashes a healthy arm while advertising on it publishes health nothing backs.
                # The empty columns reach the receipt, so a standing UNMEASURED reads as the defect it is.
                Signals.emit(Receipt.of("workers", ("emitted", f"supervise.{subject}", weighed.facts())), OPEN)
            case Verdict.DEGRADED | Verdict.DEAD:
                if not self._budgeted(charge.policy):
                    await self._advertise(subject, False)
                    Signals.emit(Receipt.of("workers", ("emitted", f"supervise.{subject}", {**weighed.facts(), "parked": True})), OPEN)
                    return
                if weighed.verdict is Verdict.DEAD:
                    await self._advertise(subject, False)
                # every supervisable kind carries a restart class, so the fold is total by construction; reading it as
                # the Option the row spells keeps the unsupervised kinds' empty rows unspellable here rather than
                # obliging a class the actuator would never re-drive them under.
                respawn = KIND_POLICY[charge.kind].restart.map(lambda cls: guard(cls)(self._respawn, charge))
                restarted = await respawn.default_with(lambda: self._respawn(charge))
                Signals.emit(Receipt.of("workers", ("emitted", f"supervise.{subject}", {**weighed.facts(), "restarted": restarted})), OPEN)
            case _ as unreachable:
                assert_never(unreachable)

    async def _reaped(self, child: psutil.Popen, grace: float) -> None:
        # the reap bound rides psutil's OWN `wait(timeout=)` rather than an enclosing `move_on_after`: the timeout
        # lives INSIDE the blocking call, so the thread always returns and always releases its band token, where an
        # abandoned `wait()` on a child that never exits parks a supervision slot for the life of the process. The
        # scope form bounded only the await, never the syscall the abandoned thread went on standing in. Expiry is
        # the bound's own answer, so the caller re-reads liveness instead of reading a raise.
        with suppress(psutil.TimeoutExpired):
            await anyio.to_thread.run_sync(partial(child.wait, timeout=grace), abandon_on_cancel=True, limiter=self._band)

    async def _respawn(self, charge: Charge) -> bool:
        subject = charge.policy.subject
        match charge.kind:
            case WorkerKind.DAEMON:
                if not charge.command:  # a DAEMON charge without a spawn command is a config refusal, parked down
                    await self._advertise(subject, False)
                    Signals.emit(Receipt.of("workers", BoundaryFault(config=(subject, "daemon-charge-without-command"))), OPEN)
                    return False
                # terminate-then-kill escalation on any stale handle, then a fresh child; readiness is the next LIVE verdict,
                # so the subject stays down until _actuate observes the respawn live — never a flip the spawn itself asserts.
                stale = self._children.get(subject)
                if stale is not None and stale.is_running():
                    stale.terminate()
                    await self._reaped(stale, charge.policy.grace)
                    if stale.is_running():
                        stale.kill()
                        # reap the SIGKILLed child before its handle drops: an unwaited kill leaves a zombie whose
                        # is_running() still answers True, and the replacement below would strand it unreaped.
                        await self._reaped(stale, charge.policy.grace)
                # spawn environment forwards the parent's effective OTLP endpoint beside the inherited environment, so
                # the child's own composition root installs against the same collector — the daemon row of the worker
                # install seam, its telemetry owned by the child's boot, never a parent-side patch.
                self._children[subject] = psutil.Popen(list(charge.command), env={**os.environ, **WorkerBoot.captured(WorkerKind.DAEMON).env})
                return True
            case kind:
                # roll receipt is the respawn verdict: a fresh arm that never flips WARM is a failed restart, held down
                # under the same next-LIVE law rather than advertised on the actuator's own optimism.
                receipt = await WorkerPool.roll(kind, charge.enforcement, charge.placement)
                return receipt.phase is PoolPhase.WARM

    async def _cycle(self, charge: Charge) -> None:
        async def cycled() -> None:
            # the weighing is a blocking native body, so it crosses to a banded thread and the loop keeps serving while
            # a slow or wedged psutil read runs; the actuation stays on the loop, where the flip and the roll it awaits
            # belong. `abandon_on_cancel` lets a cancelled supervision group drop a probe thread instead of joining it.
            await self._actuate(charge, await anyio.to_thread.run_sync(self._probe, charge, abandon_on_cancel=True, limiter=self._band))

        while True:  # Exemption: the supervision loop is the daemon's standing probe rhythm, cancelled by its owning task group.
            (await async_boundary(f"supervise.{charge.policy.subject}", cycled)).swap().map(
                lambda fault: Signals.emit(Receipt.of("workers", fault), OPEN)
            )  # the rhythm survives a probe or actuation raise
            await anyio.sleep(charge.policy.interval)

    def verdicts(self) -> Map[str, str]:
        # bundle-facing projection: the last per-subject verdict as data, never the live mutable dict — the diagnostic
        # capsule reads supervision state through this one accessor.
        return Map.of_seq((subject, verdict.value) for subject, verdict in self._verdicts.items())

    def watch(self, group: TaskGroup) -> None:
        # the band's series opens WITH the probe rhythm rather than at construction: a supervisor that never watches
        # bounds nothing, and a level nobody holds publishes no point.
        self._probes.enter_context(Metrics.occupied(lambda: self._band.borrowed_tokens, band="supervision"))
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
            await self._reaped(child, charge.policy.grace)
            if child.is_running():
                child.kill()
                # same wait-after-kill law as the restart path: the popped handle is the last reference, so the
                # kill reaps here or the child zombifies past the supervisor's own teardown.
                await self._reaped(child, charge.policy.grace)
            stopped += 1
        self._probes.close()  # the band retires AFTER the reaps that borrow it, so the last held slot still reports
        return stopped


# --- [ENTRY] ------------------------------------------------------------------------------

if __name__ == "__main__":  # fleet floor: the remote arm's session command lands here on the far interpreter
    sys.exit(remote_floor())
```

## [06]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
