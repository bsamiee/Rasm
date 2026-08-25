# [PY_COMPUTE_JIT]

One polymorphic JIT owner collapses the numba LLVM loop-kernel compiler and the jax XLA array-transform compiler onto one backend-discriminated route table: `JitBackend` discriminates the compile route, `_JIT_ROUTES` carries each route's compile-and-capture closure AND its own provider raise set as data, and `JitEvidence` parameterizes the captured output the way `JitBackend` parameterizes the input. `numerics/array#PAYLOAD` owns the separate concern of jax as an `array_namespace` backend, and `none` floors every run, so absent gated packages return `Host` evidence rather than `Error(Import)`.

This owner mints the `LoweredSpec` vocabulary of the symbolic-to-jit-to-consumer lowering chain: `analysis/symbolic#DERIVATION` emits it off its `_lower` fold, and `experiments/study#STUDY` and `solvers/quadrature#QUADRATURE` compile through `JitBackend.compile` — DAG-lawful because a symbolic-derived spec crosses as a value and no consumer imports symbolic. Its `Cfunc` row compiles the C-ABI callback the quadax/scipy `LowLevelCallable` consumers bind.

## [01]-[INDEX]

- [02]-[JIT]: numba and jax compile routes on one `JitBackend` owner over the `_JIT_ROUTES` table, evidence discriminated over `JitEvidence`, and the jit-minted `LoweredSpec` bridge vocabulary.

## [02]-[JIT]

- Owner: `JitBackend` — each case carries its route's option payload, and the `_capture_*` function beside its narrowed `catch` set IS the `_JIT_ROUTES` row, so `compile` indexes one row rather than fanning the shared decorate/warm-probe/read-IR pattern across match arms; the gated `numba`/`jax` names bind once as module-scope `lazy` imports whose proxies reify in the capture body that fires, so the table stays an eager import-free module constant and `_capture_jax` — the one jax door — owns this page's x64 config seam.
- Cases: `Specimen` is the one typed warm-probe carrier every route consumes — numba forces one dispatcher specialization against it, jax traces one `make_jaxpr` over it, and the empty `Specimen()` is the unarmed probe a route ignores — so no route reads a positional `probe[0]` off an erased varargs tuple.
- Output: `JitEvidence` gives each route its own case with a total `facts()` projection of native scalars, so an LLVM specialization never smuggles jax fields and `Jitted.attributes` spreads only the matched case's slots; `diagnostics_lines` is the realized parallel-region evidence, distinct from the requested `parallel` flag. `EngineProfile` is the engine-neutral compile-extent band BOTH compiled cases carry, `JitEvidence.profile` the one outward read every mount takes rather than destructuring a case payload by offset, and `solvers/solve#SOLVE` mounts it as the optional `profile` slot the `solvers/quadrature#QUADRATURE` lowering bridge fills — specialization count beside the engine-IR, target-code, typed-source, and diagnostics extents, each column answered from what the engine already measured, so a slow compile or solve explains itself from the `Jitted` value with no profiler attach. `llvm` fills it off the held dispatcher's `inspect_llvm`/`inspect_asm`/`inspect_types`/`parallel_diagnostics` reports, `xla` fills the identical columns off the staging ladder — `Lowered.as_text` StableHLO, `Compiled.as_text` optimized HLO, the captured jaxpr, and the `cost_analysis` entry tally — so one profile shape spans both engines and a comparison reads one profile. `TraceEvidence` rides the `xla` case alone as its caller-armed device-timeline band.
- Output: `compile` runs under the hub weave as `evidence_run(EvidenceScope.JIT, f"compile.{self.tag}", rail, facts=...)` — LLVM/XLA lowering is the canonical measured surface, the span carries the backend, kernel, and armed discriminants at open, and the `Jitted` mint stamps its `attributes` on that same span, so no page-local emit call exists.
- Packages: the numba dispatcher, the jax trace handle, the `Wrapped`/`Lowered`/`Compiled` staging rungs, and the four-tier profile reader are typed through `TYPE_CHECKING` `Protocol`s so every capture reads a named member rather than a phantom off `object`; `Specimen` and `Jitted` stay GC-tracked because each holds a container field — `gc=False` is reserved for container-free leaves like the two profile bands.
- Growth: a new compiler is one `JitBackend` case, one `_JIT_ROUTES` row carrying its capture and its own raise set, and its `JitEvidence` case — the `Cfunc` row is exactly that path realized; a new option is one column absorbed by the existing decorator call; a new lowering producer emits `LoweredSpec` values and adds zero surface here; a new compile statistic is one `EngineProfile` column every compiled route answers from its own engine, reaching the `Solve` mount with zero edits there, while a statistic only one engine can measure lands on that case's own band — `TraceEvidence` being that path realized, since a host-compiled kernel has no device timeline to answer a device column with anything but a zero.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Callable, Iterable, Mapping, Sequence
from contextlib import redirect_stdout
from threading import Lock
from typing import TYPE_CHECKING, Final, Literal, Protocol, assert_never

from beartype.door import is_bearable
from expression import Error, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs
from upath import UPath

from opentelemetry import trace

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, evidence_run
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.faults import TERMINAL, TRANSIENT, Catch, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey

lazy import jax
lazy import numba

if TYPE_CHECKING:
    class Dispatcher(Protocol):
        signatures: list[object]

        def __call__(self, *args: object) -> object: ...
        def inspect_asm(self) -> dict[object, str]: ...
        def inspect_llvm(self) -> dict[object, str]: ...
        def inspect_types(self) -> None: ...
        def parallel_diagnostics(self, signature: object = ..., level: int = ...) -> None: ...

    class CFunc(Protocol):
        address: int
        ctypes: Callable[..., object]

    class Jaxpr(Protocol):
        def __repr__(self) -> str: ...

    class ShapeDtypeStruct(Protocol):
        shape: tuple[int, ...]
        dtype: object

    class Compiled(Protocol):
        def as_text(self) -> str | None: ...
        def cost_analysis(self) -> object: ...

    class Lowered(Protocol):
        def as_text(self) -> str: ...
        def compile(self) -> "Compiled": ...
        def cost_analysis(self) -> object: ...

    class Wrapped(Protocol):
        def __call__(self, *args: object) -> object: ...
        def lower(self, *args: object) -> "Lowered": ...

    class ProfileEvent(Protocol):
        duration_ns: int

    class ProfileLine(Protocol):
        events: Iterable["ProfileEvent"]

    class ProfilePlane(Protocol):
        lines: Iterable["ProfileLine"]

    class ProfileData(Protocol):
        planes: Iterable["ProfilePlane"]


# --- [TYPES] ----------------------------------------------------------------------------

type Tag = Literal["njit", "vectorize", "cfunc", "jax_jit", "none"]
type Kernel = Callable[..., object]
type Capture = Callable[[Kernel, "Specimen", "JitBackend"], tuple[Kernel, "JitEvidence"]]


# --- [MODELS] ---------------------------------------------------------------------------


class Specimen(Struct, frozen=True):
    args: tuple[object, ...] = ()

    @staticmethod
    def of(*args: object) -> "Specimen":
        return Specimen(args)

    @property
    def is_armed(self) -> bool:
        return len(self.args) > 0


class EngineProfile(Struct, frozen=True, gc=False):
    specializations: int
    ir_lines: int
    asm_lines: int
    typed_lines: int
    diagnostics_lines: int

    def facts(self, prefix: str = "") -> dict[str, int]:
        return {f"{prefix}{name}": value for name, value in structs.asdict(self).items()}


class TraceEvidence(Struct, frozen=True, gc=False):
    planes: int
    events: int
    device_ns: int
    heap_bytes: int

    def facts(self, prefix: str = "") -> dict[str, int]:
        return {f"{prefix}{name}": value for name, value in structs.asdict(self).items()}


@tagged_union(frozen=True)
class JitEvidence:
    tag: Literal["llvm", "ufunc", "cabi", "xla", "host"] = tag()
    llvm: tuple[str, bool, bool, bool, EngineProfile] = case()
    ufunc: tuple[str, str, str] = case()
    cabi: tuple[str, int] = case()
    xla: tuple[tuple[int, ...], tuple[int, ...], str, EngineProfile, TraceEvidence | None] = case()
    host: tuple[()] = case()

    @staticmethod
    def Llvm(signature: str, *, parallel: bool, fastmath: bool, cached: bool, profile: EngineProfile) -> "JitEvidence":
        return JitEvidence(llvm=(signature, parallel, fastmath, cached, profile))

    @staticmethod
    def Ufunc(signature: str, layout: str, target: str) -> "JitEvidence":
        return JitEvidence(ufunc=(signature, layout, target))

    @staticmethod
    def Cabi(signature: str, address: int) -> "JitEvidence":
        return JitEvidence(cabi=(signature, address))

    @staticmethod
    def Xla(
        static_argnums: tuple[int, ...], out_shape: tuple[int, ...], out_dtype: str, profile: EngineProfile, trace: TraceEvidence | None = None
    ) -> "JitEvidence":
        return JitEvidence(xla=(static_argnums, out_shape, out_dtype, profile, trace))

    @staticmethod
    def Host() -> "JitEvidence":
        return JitEvidence(host=())

    @property
    def profile(self) -> EngineProfile | None:
        match self:
            case JitEvidence(tag="llvm", llvm=(_, _, _, _, profile)) | JitEvidence(tag="xla", xla=(_, _, _, profile, _)):
                return profile
            case JitEvidence(tag="ufunc") | JitEvidence(tag="cabi") | JitEvidence(tag="host"):
                return None
            case _ as unreachable:
                assert_never(unreachable)

    def facts(self) -> dict[str, object]:
        match self:
            case JitEvidence(tag="llvm", llvm=(signature, parallel, fastmath, cached, profile)):
                return {"mode": "llvm", "signature": signature, "parallel": parallel, "fastmath": fastmath, "cached": cached, **profile.facts()}
            case JitEvidence(tag="ufunc", ufunc=(signature, layout, target)):
                return {"mode": "gufunc" if layout else "ufunc", "signature": signature, "layout": layout or "<elementwise>", "target": target}
            case JitEvidence(tag="cabi", cabi=(signature, address)):
                return {"mode": "cabi", "signature": signature, "address": address}
            case JitEvidence(tag="xla", xla=(static_argnums, out_shape, out_dtype, profile, trace)):
                return {
                    "mode": "xla", "static_argnums": static_argnums, "out_shape": out_shape, "out_dtype": out_dtype,
                    **profile.facts(), **(trace.facts("trace.") if trace else {}),
                }
            case JitEvidence(tag="host", host=()):
                return {"mode": "none"}
            case _ as unreachable:
                assert_never(unreachable)


class Jitted(Struct, frozen=True):
    fn: Kernel
    backend: "JitBackend"
    content_key: ContentKey
    evidence: JitEvidence

    @property
    def attributes(self) -> dict[str, str | bool | int | float]:
        scalars = {name: value for name, value in self.evidence.facts().items() if isinstance(value, str | bool | int | float)}
        return {"backend": self.backend.tag, "key": self.content_key.hex, **scalars}

    def _noted(self) -> "Jitted":
        trace.get_current_span().set_attributes(self.attributes)
        return self


@tagged_union(frozen=True)
class JitBackend:
    tag: Tag = tag()
    njit: tuple[bool, bool, bool, bool, bool] = case()
    vectorize: tuple[tuple[str, ...], Literal["cpu", "parallel"], str] = case()
    cfunc: tuple[str] = case()
    jax_jit: tuple[tuple[int, ...], tuple[int, ...], str] = case()
    none: tuple[()] = case()

    @staticmethod
    def Njit(*, parallel: bool = False, fastmath: bool = False, cache: bool = True, boundscheck: bool = False, nogil: bool = False) -> "JitBackend":
        return JitBackend(njit=(parallel, fastmath, cache, boundscheck, nogil))

    @staticmethod
    def Vectorize(signatures: tuple[str, ...], *, target: Literal["cpu", "parallel"] = "cpu", layout: str = "") -> "JitBackend":
        return JitBackend(vectorize=(signatures, target, layout))

    @staticmethod
    def Cfunc(signature: str) -> "JitBackend":
        return JitBackend(cfunc=(signature,))

    @staticmethod
    def JaxJit(*, static_argnums: tuple[int, ...] = (), donate_argnums: tuple[int, ...] = (), trace_dir: str = "") -> "JitBackend":
        return JitBackend(jax_jit=(static_argnums, donate_argnums, trace_dir))

    @staticmethod
    def Passthrough() -> "JitBackend":
        return JitBackend(none=())

    def compile(self, kernel: Kernel, specimen: "Specimen" = Specimen(), *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[Jitted]":
        if not is_bearable(kernel, Kernel):
            return Error(KERNEL.raised(self.tag))

        def rail() -> "RuntimeRail[Jitted]":
            route = _JIT_ROUTES[self.tag]
            return ContentIdentity.of(f"jit.{self.tag}", self.identity_source(kernel, specimen)).bind(
                lambda key: boundary(COMPILE, lambda: self._compiled(kernel, specimen, key), catch=route.catch)
            )

        facts = {"backend": self.tag, "kernel": getattr(kernel, "__qualname__", repr(kernel)), "armed": specimen.is_armed}
        return evidence_run(EvidenceScope.JIT, f"compile.{self.tag}", rail, facts=facts, composition=composition)

    def identity_source(self, kernel: Kernel, specimen: "Specimen") -> IdentitySource:
        row: tuple[object, ...]
        match self:
            case JitBackend(tag="njit", njit=options):
                row = options
            case JitBackend(tag="vectorize", vectorize=(signatures, target, layout)):
                row = (repr(signatures), target, layout)
            case JitBackend(tag="cfunc", cfunc=(signature,)):
                row = (signature,)
            case JitBackend(tag="jax_jit", jax_jit=(static_argnums, donate_argnums, _trace_dir)):
                row = (repr(static_argnums), repr(donate_argnums))
            case JitBackend(tag="none", none=()):
                row = ()
            case _ as unreachable:
                assert_never(unreachable)
        probes = tuple(f"{type(a).__name__}:{getattr(a, 'shape', ())}:{getattr(a, 'dtype', '')}".encode() for a in specimen.args)
        return IdentitySource(parts=(
            self.tag.encode(),
            getattr(kernel, "__qualname__", repr(kernel)).encode(),
            str(len(probes)).encode(),
            *probes,
            *(str(cell).encode() for cell in row),
        ))

    def _compiled(self, kernel: Kernel, specimen: "Specimen", key: ContentKey) -> "Jitted":
        fn, evidence = _JIT_ROUTES[self.tag](kernel, specimen, self)
        return Jitted(fn, self, key, evidence)._noted()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _capture_njit(kernel: Kernel, specimen: "Specimen", backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    parallel, fastmath, cache, boundscheck, nogil = backend.njit
    fn: "Dispatcher" = numba.njit(cache=cache, parallel=parallel, fastmath=fastmath, boundscheck=boundscheck, nogil=nogil)(kernel)
    if specimen.is_armed:
        fn(*specimen.args)
    signature = ", ".join(str(s) for s in fn.signatures) or "<unspecialized>"
    return fn, JitEvidence.Llvm(signature, parallel=parallel, fastmath=fastmath, cached=cache, profile=_profiled(fn, parallel))


def _capture_vectorize(kernel: Kernel, _specimen: "Specimen", backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    signatures, target, layout = backend.vectorize
    fn = numba.guvectorize(list(signatures), layout, target=target)(kernel) if layout else numba.vectorize(list(signatures), target=target)(kernel)
    return fn, JitEvidence.Ufunc(" | ".join(signatures), layout, target)


def _capture_jax(kernel: Kernel, specimen: "Specimen", backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    jax.config.update("jax_enable_x64", True)
    static_argnums, donate_argnums, trace_dir = backend.jax_jit
    fn: "Wrapped" = jax.jit(kernel, static_argnums=static_argnums, donate_argnums=donate_argnums)
    if specimen.is_armed:
        jaxpr: "Jaxpr"
        jaxpr, out_tree = jax.make_jaxpr(kernel, static_argnums=static_argnums, return_shape=True)(*specimen.args)
        out: "ShapeDtypeStruct" = jax.tree_util.tree_leaves(out_tree)[0]
        return fn, JitEvidence.Xla(
            static_argnums, tuple(out.shape), str(out.dtype),
            _xla_profiled(fn.lower(*specimen.args), repr(jaxpr)),
            _traced(fn, specimen, trace_dir) if trace_dir else None,
        )
    return fn, JitEvidence.Xla(static_argnums, (), "<unspecialized>", EngineProfile(0, 0, 0, 0, 0))


def _capture_cfunc(kernel: Kernel, _specimen: "Specimen", backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    (signature,) = backend.cfunc
    fn: "CFunc" = numba.cfunc(signature)(kernel)
    return fn.ctypes, JitEvidence.Cabi(signature, int(fn.address))


def _capture_host(kernel: Kernel, _specimen: "Specimen", _backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    return kernel, JitEvidence.Host()


def _profiled(fn: "Dispatcher", parallel: bool) -> EngineProfile:
    armed = bool(fn.signatures)
    return EngineProfile(
        specializations=len(fn.signatures),
        ir_lines=sum(_text_lines(text) for text in fn.inspect_llvm().values()) if armed else 0,
        asm_lines=sum(_text_lines(text) for text in fn.inspect_asm().values()) if armed else 0,
        typed_lines=_printed_lines(fn.inspect_types) if armed else 0,
        diagnostics_lines=_printed_lines(lambda: fn.parallel_diagnostics(level=1)) if parallel and armed else 0,
    )


def _xla_profiled(lowered: "Lowered", jaxpr_text: str) -> EngineProfile:
    compiled = lowered.compile()
    return EngineProfile(
        specializations=1,
        ir_lines=_text_lines(lowered.as_text()),
        asm_lines=_text_lines(compiled.as_text() or ""),
        typed_lines=_text_lines(jaxpr_text),
        diagnostics_lines=_entry_count(lowered.cost_analysis()),
    )


def _entry_count(report: object) -> int:
    match report:
        case None:
            return 0
        case Mapping():
            return len(report)
        case str() | bytes():
            return 1
        case Sequence():
            return sum(_entry_count(row) for row in report)
        case _:
            return 1


_TRACE_GATE: Final[Lock] = Lock()


def _traced(fn: "Wrapped", specimen: "Specimen", log_dir: str) -> TraceEvidence:
    root = UPath(log_dir)
    with _TRACE_GATE:
        with jax.profiler.trace(str(root)):
            jax.block_until_ready(fn(*specimen.args))
        planes = tuple(_opened(root).planes)
    events = tuple(event for plane in planes for line in plane.lines for event in line.events)
    return TraceEvidence(
        planes=len(planes),
        events=len(events),
        device_ns=sum(int(event.duration_ns) for event in events),
        heap_bytes=len(jax.profiler.device_memory_profile()),
    )


def _opened(root: UPath) -> "ProfileData":
    xplane = max(root.glob("plugins/profile/*/*.xplane.pb"), key=lambda path: path.parent.name)
    return jax.profiler.ProfileData.from_file(str(xplane))


def _text_lines(text: str) -> int:
    return sum(1 for line in text.splitlines() if line.strip())


def _printed_lines(emit: Callable[[], object]) -> int:
    sink = io.StringIO()
    with redirect_stdout(sink):
        emit()
    return _text_lines(sink.getvalue())


# --- [TABLES] ---------------------------------------------------------------------------

class JitRoute(Struct, frozen=True, gc=False):
    capture: Capture
    catch: Catch


_JIT_ROUTES: Final[Map[Tag, JitRoute]] = Map.of_seq([
    ("njit", JitRoute(_capture_njit, numba.NumbaError)),
    ("vectorize", JitRoute(_capture_vectorize, numba.NumbaError)),
    ("cfunc", JitRoute(_capture_cfunc, numba.NumbaError)),
    ("jax_jit", JitRoute(_capture_jax, (TypeError, ValueError))),
    ("none", JitRoute(_capture_host, ())),
])

KERNEL: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.JIT, point="kernel", arm="config", defect="kernel-not-callable", retriability=TERMINAL, slots=("backend",)
)
COMPILE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.JIT, point="compile", arm="boundary", defect="compile", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([KERNEL, COMPILE]))


# --- [EXPORTS] --------------------------------------------------------------------------


class LoweredSpec(Struct, frozen=True):
    kernel: Kernel
    name: str
    arity: int
    signature: str = ""
    route: JitBackend = JitBackend.Passthrough()

    def compiled(self, specimen: Specimen = Specimen()) -> "RuntimeRail[Jitted]":
        return self.route.compile(self.kernel, specimen)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
