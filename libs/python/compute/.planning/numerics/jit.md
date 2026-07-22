# [PY_COMPUTE_JIT]

One polymorphic JIT owner collapses the numba LLVM loop-kernel compiler and the jax XLA array-transform compiler into a single backend-discriminated route table: `JitBackend` discriminates the compile route, `_JIT_ROUTES` carries each route's compile-and-capture closure as data, and `JitEvidence` parameterizes the captured lowered-IR output the way `JitBackend` parameterizes the input. Both compilers are admitted as study-kernel accelerators on one owner — distinct from the Array-API dispatch in `numerics/array.md#PAYLOAD`, where jax rides `array_namespace` as a backend rather than a wrap — and the `none` passthrough is the unconditional runtime floor, so a run without the gated packages returns `Host` evidence rather than `Error(Import)`.

This owner mints the `LoweredSpec` vocabulary of the symbolic-to-jit-to-consumer lowering chain: `analysis/symbolic.md#DERIVATION` emits it off its `_lower` fold, and `experiments/study.md#STUDY` and `solvers/quadrature.md#QUADRATURE` compile through `JitBackend.compile` — DAG-lawful because a symbolic-derived spec crosses as a value and no consumer imports symbolic. Its `Cfunc` row compiles the C-ABI callback the quadax/scipy `LowLevelCallable` consumers bind.

## [01]-[INDEX]

- [01]-[JIT]: numba and jax compile routes on one `JitBackend` owner over the `_JIT_ROUTES` table, evidence discriminated over `JitEvidence`, and the jit-minted `LoweredSpec` bridge vocabulary.

## [02]-[JIT]

- Owner: `JitBackend` — each case carries its route's option payload, and the bare `_capture_*` function IS the `_JIT_ROUTES` row, so `compile` indexes one row rather than fanning the shared decorate/warm-probe/read-IR pattern across match arms; the gated `numba`/`jax` imports stay inside each capture body, so the table is an eager import-free module constant.
- Cases: `Specimen` is the one typed warm-probe carrier every route consumes — numba forces one dispatcher specialization against it, jax traces one `make_jaxpr` over it, and the empty `Specimen()` is the unarmed probe a route ignores — so no route reads a positional `probe[0]` off an erased varargs tuple.
- Output: `JitEvidence` gives each route its own case with a total `facts()` projection of native scalars, so an LLVM specialization never smuggles jax fields and the receipt spreads only the matched case's slots; `diagnostics_lines` is the realized parallel-region evidence, distinct from the requested `parallel` flag. `EngineProfile` is the engine-native profile band the `llvm` case carries and `solvers/receipt.md#RECEIPT` mounts as its optional `profile` slot — specialization count beside the `inspect_llvm`/`inspect_asm`/`inspect_types` extents and the realized `parallel_diagnostics` tally, harvested off the dispatcher the capture already holds, so a slow compile or solve explains itself from the receipt with no profiler attach; the `xla` case carries its captured-jaxpr statistics as its own profile evidence.
- Packages: the numba dispatcher and jax trace handles are typed through `TYPE_CHECKING` `Protocol`s so every capture reads a named member rather than a phantom off `object`; `Specimen` and `Jitted` stay GC-tracked because each holds a container field — `gc=False` is reserved for container-free leaves.
- Receipt: `compile` runs under the hub weave as `evidence_run(EvidenceScope.JIT, f"compile.{self.tag}", rail, facts=...)` — LLVM/XLA lowering is the canonical measured surface, the span carries the backend, kernel, and armed discriminants, and the weave harvest emits the `Jitted` receipts on the clean exit, so `contribute()` needs no page-local emit call.
- Growth: a new compiler is one `JitBackend` case, one `_JIT_ROUTES` row, and its `JitEvidence` case — the `Cfunc` row is exactly that path realized; a new option is one column absorbed by the existing decorator call; a new lowering producer emits `LoweredSpec` values and adds zero surface here; a new profile statistic is one `EngineProfile` field and one `_profiled` read, reaching the solve receipt's mount with zero receipt edits.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
import io
from collections.abc import Callable, Iterable
from contextlib import redirect_stdout
from typing import TYPE_CHECKING, Final, Literal, Protocol, assert_never

from beartype.door import is_bearable
from expression import Error, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct, structs

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

lazy import jax
lazy import numba

if TYPE_CHECKING:
    # Capture handles read named members through protocols rather than phantom attributes on `object`.
    class Dispatcher(Protocol):  # the numba `CPUDispatcher` a decorated kernel becomes
        signatures: list[object]

        def __call__(self, *args: object) -> object: ...
        def inspect_asm(self) -> dict[object, str]: ...
        def inspect_llvm(self) -> dict[object, str]: ...
        def inspect_types(self) -> None: ...
        def parallel_diagnostics(self, signature: object = ..., level: int = ...) -> None: ...

    class CFunc(Protocol):
        address: int
        ctypes: Callable[..., object]

    class Jaxpr(Protocol):  # the traced jax program `make_jaxpr` returns; `repr` is the IR text
        def __repr__(self) -> str: ...

    class ShapeDtypeStruct(Protocol):  # a `return_shape=True` out-spec leaf; `.shape`/`.dtype` are the native facts
        shape: tuple[int, ...]
        dtype: object


# --- [TYPES] -------------------------------------------------------------------------------

type Tag = Literal["njit", "vectorize", "cfunc", "jax_jit", "none"]
type Kernel = Callable[..., object]  # the study kernel; numba reads numpy arrays, jax reads its own `Array`
# one route row: (kernel, specimen, backend) -> (compiled callable, captured IR); the bare closure IS the
# row, so the `_JIT_ROUTES` table keys `Tag -> Capture` with no single-field wrapper struct between.
type Capture = Callable[[Kernel, "Specimen", "JitBackend"], tuple[Kernel, "JitEvidence"]]


# --- [MODELS] ------------------------------------------------------------------------------


class Specimen(Struct, frozen=True):
    # GC-tracked (`args` is a container field). `is_armed` gates the warm call so an unspecialized compile reads `<unspecialized>`
    # rather than raising on an empty trace.
    args: tuple[object, ...] = ()

    @staticmethod
    def of(*args: object) -> "Specimen":
        return Specimen(args)

    @property
    def is_armed(self) -> bool:
        return len(self.args) > 0


class EngineProfile(Struct, frozen=True, gc=False):
    # engine-native profile band — what the compiler already measured, harvested off the held dispatcher:
    # specialization count beside the lowered-IR, native-asm, typed-source, and parallel-diagnostics extents.
    # Shared outward: `solvers/receipt.md#RECEIPT` mounts it as the optional per-case `profile` slot.
    specializations: int
    ir_lines: int
    asm_lines: int
    typed_lines: int
    diagnostics_lines: int

    def facts(self, prefix: str = "") -> dict[str, int]:
        # one derived projection; `prefix` parameterizes the mount — bare on the jit receipt, `profile.`-namespaced on the solve receipt.
        return {f"{prefix}{name}": value for name, value in structs.asdict(self).items()}


@tagged_union(frozen=True)
class JitEvidence:
    tag: Literal["llvm", "ufunc", "cabi", "xla", "host"] = tag()
    llvm: tuple[str, bool, bool, bool, EngineProfile] = case()  # signature, parallel, fastmath, cached, profile
    ufunc: tuple[str, str, str] = case()  # signature, layout, target
    cabi: tuple[str, int] = case()  # signature, address
    xla: tuple[tuple[int, ...], tuple[int, ...], str, int] = case()  # static_argnums, out_shape, out_dtype, jaxpr_lines
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
    def Xla(static_argnums: tuple[int, ...], out_shape: tuple[int, ...], out_dtype: str, jaxpr_lines: int) -> "JitEvidence":
        return JitEvidence(xla=(static_argnums, out_shape, out_dtype, jaxpr_lines))

    @staticmethod
    def Host() -> "JitEvidence":
        return JitEvidence(host=())

    def facts(self) -> dict[str, object]:
        match self:
            case JitEvidence(tag="llvm", llvm=(signature, parallel, fastmath, cached, profile)):
                return {"mode": "llvm", "signature": signature, "parallel": parallel, "fastmath": fastmath, "cached": cached, **profile.facts()}
            case JitEvidence(tag="ufunc", ufunc=(signature, layout, target)):
                return {"mode": "gufunc" if layout else "ufunc", "signature": signature, "layout": layout or "<elementwise>", "target": target}
            case JitEvidence(tag="cabi", cabi=(signature, address)):
                return {"mode": "cabi", "signature": signature, "address": address}
            case JitEvidence(tag="xla", xla=(static_argnums, out_shape, out_dtype, jaxpr_lines)):
                return {"mode": "xla", "static_argnums": static_argnums, "out_shape": out_shape, "out_dtype": out_dtype, "jaxpr_lines": jaxpr_lines}
            case JitEvidence(tag="host", host=()):
                return {"mode": "none"}
            case _ as unreachable:
                assert_never(unreachable)


class Jitted(Struct, frozen=True):  # GC-tracked: carries the `fn` callable and the `backend`/`evidence` union containers
    fn: Kernel
    backend: "JitBackend"
    content_key: ContentKey
    evidence: JitEvidence

    def contribute(self) -> Iterable[Receipt]:
        facts = {"backend": self.backend.tag, "content_key": self.content_key.project("hex"), **self.evidence.facts()}
        yield Receipt.of(EvidenceScope.JIT.value, ("emitted", self.backend.tag, facts))


@tagged_union(frozen=True)
class JitBackend:
    tag: Tag = tag()
    njit: tuple[bool, bool, bool, bool, bool] = case()  # parallel, fastmath, cache, boundscheck, nogil
    vectorize: tuple[tuple[str, ...], Literal["cpu", "parallel"], str] = case()  # signatures, target, layout
    cfunc: tuple[str] = case()  # the numba C signature string, e.g. "float64(float64, voidptr)"
    jax_jit: tuple[tuple[int, ...], tuple[int, ...]] = case()  # static_argnums, donate_argnums
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
    def JaxJit(*, static_argnums: tuple[int, ...] = (), donate_argnums: tuple[int, ...] = ()) -> "JitBackend":
        return JitBackend(jax_jit=(static_argnums, donate_argnums))

    @staticmethod
    def Passthrough() -> "JitBackend":
        return JitBackend(none=())

    def compile(self, kernel: Kernel, specimen: "Specimen" = Specimen()) -> "RuntimeRail[Jitted]":
        # non-callable guard is a pure domain reject before the gated import — never a thunk raising purely so `boundary` can
        # re-catch it; the railed key threads `.bind` so a canonical-encode fault rides the one rail, and the whole compile runs
        # under the `compute.jit` span with the weave harvest emitting the `Jitted` receipts on the clean exit.
        if not is_bearable(kernel, Kernel):
            return Error(BoundaryFault(boundary=(f"jit.{self.tag}", "kernel-not-callable")))

        def rail() -> "RuntimeRail[Jitted]":
            return ContentIdentity.of(f"jit.{self.tag}", self.identity_buffer(kernel, specimen)).bind(
                lambda key: boundary(f"jit.{self.tag}", lambda: self._compiled(kernel, specimen, key))
            )

        facts = {"backend": self.tag, "kernel": getattr(kernel, "__qualname__", repr(kernel)), "armed": specimen.is_armed}
        return evidence_run(EvidenceScope.JIT, f"compile.{self.tag}", rail, facts=facts)

    def identity_buffer(self, kernel: Kernel, specimen: "Specimen") -> bytes:
        # closure source is not byte-stable across runs — tag + qualname + probe signature + option row is the stable buffer, so one
        # kernel compiled under two option payloads never shares a `ContentKey`.
        row: tuple[object, ...]
        match self:
            case JitBackend(tag="njit", njit=options):
                row = options
            case JitBackend(tag="vectorize", vectorize=(signatures, target, layout)):
                row = (repr(signatures), target, layout)
            case JitBackend(tag="cfunc", cfunc=(signature,)):
                row = (signature,)
            case JitBackend(tag="jax_jit", jax_jit=(static_argnums, donate_argnums)):
                row = (repr(static_argnums), repr(donate_argnums))
            case JitBackend(tag="none", none=()):
                row = ()
            case _ as unreachable:
                assert_never(unreachable)
        probe = "|".join(f"{type(a).__name__}:{getattr(a, 'shape', ())}:{getattr(a, 'dtype', '')}" for a in specimen.args)
        cells = "|".join(str(cell) for cell in row)
        return f"{self.tag}|{getattr(kernel, '__qualname__', repr(kernel))}|{probe}|{cells}".encode()

    def _compiled(self, kernel: Kernel, specimen: "Specimen", key: ContentKey) -> "Jitted":
        fn, evidence = _JIT_ROUTES[self.tag](kernel, specimen, self)
        return Jitted(fn, self, key, evidence)


# --- [OPERATIONS] --------------------------------------------------------------------------


def _capture_njit(kernel: Kernel, specimen: "Specimen", backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    parallel, fastmath, cache, boundscheck, nogil = backend.njit
    fn: "Dispatcher" = numba.njit(cache=cache, parallel=parallel, fastmath=fastmath, boundscheck=boundscheck, nogil=nogil)(kernel)
    if specimen.is_armed:
        fn(*specimen.args)  # forces one `CPUDispatcher` specialization so `signatures`/`inspect_*` are non-empty
    signature = ", ".join(str(s) for s in fn.signatures) or "<unspecialized>"
    return fn, JitEvidence.Llvm(signature, parallel=parallel, fastmath=fastmath, cached=cache, profile=_profiled(fn, parallel))


def _capture_vectorize(kernel: Kernel, _specimen: "Specimen", backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    signatures, target, layout = backend.vectorize
    fn = numba.guvectorize(list(signatures), layout, target=target)(kernel) if layout else numba.vectorize(list(signatures), target=target)(kernel)
    return fn, JitEvidence.Ufunc(" | ".join(signatures), layout, target)


def _capture_jax(kernel: Kernel, specimen: "Specimen", backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    static_argnums, donate_argnums = backend.jax_jit
    fn = jax.jit(kernel, static_argnums=static_argnums, donate_argnums=donate_argnums)
    # `make_jaxpr(return_shape=True)` stages the jaxpr AND returns the out-structure pytree in one trace — no separate `eval_shape`
    # pass; `tree_leaves` reads the leading out-spec leaf so a multi-output pytree never raises `AttributeError` on `.shape`.
    if specimen.is_armed:
        jaxpr: "Jaxpr"
        jaxpr, out_tree = jax.make_jaxpr(kernel, static_argnums=static_argnums, return_shape=True)(*specimen.args)
        out: "ShapeDtypeStruct" = jax.tree_util.tree_leaves(out_tree)[0]
        return fn, JitEvidence.Xla(static_argnums, tuple(out.shape), str(out.dtype), _text_lines(repr(jaxpr)))
    return fn, JitEvidence.Xla(static_argnums, (), "<unspecialized>", 0)


def _capture_cfunc(kernel: Kernel, _specimen: "Specimen", backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    # `.address` is the callback pointer a quadax/scipy `LowLevelCallable` consumer binds; the compiled object stays
    # Python-callable through `.ctypes`, which is the uniform callable returned to consumers.
    (signature,) = backend.cfunc
    fn: "CFunc" = numba.cfunc(signature)(kernel)
    return fn.ctypes, JitEvidence.Cabi(signature, int(fn.address))


def _capture_host(kernel: Kernel, _specimen: "Specimen", _backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    return kernel, JitEvidence.Host()


def _profiled(fn: "Dispatcher", parallel: bool) -> EngineProfile:
    # profile harvest off the held dispatcher — everything the compiler already measured, no profiler attach:
    # `inspect_llvm`/`inspect_asm` return per-signature dicts whose values all join the extent through the one
    # non-blank tally, while `inspect_types` and `parallel_diagnostics` print their reports and ride the stdout form.
    armed = bool(fn.signatures)
    return EngineProfile(
        specializations=len(fn.signatures),
        ir_lines=sum(_text_lines(text) for text in fn.inspect_llvm().values()) if armed else 0,
        asm_lines=sum(_text_lines(text) for text in fn.inspect_asm().values()) if armed else 0,
        typed_lines=_printed_lines(fn.inspect_types) if armed else 0,
        diagnostics_lines=_printed_lines(lambda: fn.parallel_diagnostics(level=1)) if parallel and armed else 0,
    )


def _text_lines(text: str) -> int:
    # one report-extent rule for every profile tally: non-blank lines via splitlines, so a report without a trailing
    # newline counts whole and blank padding never inflates the extent — a `count("\n")` separator tally is the
    # rejected form on every LLVM, ASM, and jaxpr report.
    return sum(1 for line in text.splitlines() if line.strip())


def _printed_lines(emit: Callable[[], object]) -> int:
    # `inspect_types` and `parallel_diagnostics(level=)` write their reports to stdout rather than returning them; the
    # non-blank line tally is present-vs-absent extent evidence that never couples to a specific report phrase.
    sink = io.StringIO()
    with redirect_stdout(sink):
        emit()
    return _text_lines(sink.getvalue())


# --- [TABLES] ------------------------------------------------------------------------------

# each `_capture_*` already holds the full `Capture` arity, so the row is the bare function reference — no wrapper struct and no
# forwarding lambda; the table anchors after the captures because the module-level `Map` resolves them at load.
_JIT_ROUTES: Final[Map[Tag, Capture]] = Map.of_seq([
    ("njit", _capture_njit),
    ("vectorize", _capture_vectorize),
    ("cfunc", _capture_cfunc),
    ("jax_jit", _capture_jax),
    ("none", _capture_host),
])


# --- [EXPORTS] -----------------------------------------------------------------------------


class LoweredSpec(Struct, frozen=True):
    # bridge value of the symbolic-to-jit-to-consumer chain: crosses as a VALUE, so no consumer imports symbolic and the DAG
    # carries no back-edge.
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
