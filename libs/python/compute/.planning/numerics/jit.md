# [PY_COMPUTE_JIT]

The one polymorphic JIT owner collapsing the numba LLVM loop-kernel compiler and the jax XLA array-transform compiler into a single backend-discriminated route table: `JitBackend` discriminates the compile route, `_JIT_ROUTES` carries each route's compile-and-capture closure as data, and `JitEvidence` parameterizes the captured lowered-IR output the way `JitBackend` parameterizes the input. Both compilers are admitted as study-kernel accelerators on one owner — distinct from the Array-API dispatch in `numerics/array.md#PAYLOAD`, where jax rides `array_namespace` as a backend rather than a wrap — and the `none` passthrough is the unconditional runtime floor, so a run without the gated packages returns `Host` evidence rather than `Error(Import)`.

This owner mints the `LoweredSpec` vocabulary of the symbolic-to-jit-to-consumer lowering chain: `analysis/symbolic.md#DERIVATION` emits it off its `_lower` fold, and `experiments/study.md#STUDY` and `solvers/quadrature.md#QUADRATURE` compile through `JitBackend.compile` — DAG-lawful because a symbolic-derived spec crosses as a value and no consumer imports symbolic. The `Cfunc` row compiles the C-ABI callback the quadax/scipy `LowLevelCallable` consumers bind.

## [01]-[INDEX]

- [01]-[JIT]: numba and jax compile routes on one `JitBackend` owner over the `_JIT_ROUTES` table, evidence discriminated over `JitEvidence`, plus the jit-minted `LoweredSpec` bridge vocabulary.

## [02]-[JIT]

- Owner: `JitBackend` — each case carries its route's option payload, and the bare `_capture_*` function IS the `_JIT_ROUTES` row, so `compile` indexes one row rather than fanning the shared decorate/warm-probe/read-IR pattern across match arms; the gated `numba`/`jax` imports stay inside each capture body, so the table is an eager import-free module constant.
- Cases: `Specimen` is the one typed warm-probe carrier every route consumes — numba forces one dispatcher specialization against it, jax traces one `make_jaxpr` over it, and the empty `Specimen()` is the unarmed probe a route ignores — so no route reads a positional `probe[0]` off an erased varargs tuple.
- Output: `JitEvidence` gives each route its own case with a total `facts()` projection of native scalars, so an LLVM specialization never smuggles jax fields and the receipt spreads only the matched case's slots; `diagnostics_lines` is the realized parallel-region evidence, distinct from the requested `parallel` flag.
- Packages: the numba dispatcher and jax trace handles are typed through `TYPE_CHECKING` `Protocol`s so every capture reads a named member rather than a phantom off `object`; `Specimen` and `Jitted` stay GC-tracked because each holds a container field — `gc=False` is reserved for container-free leaves.
- Growth: a new compiler is one `JitBackend` case plus one `_JIT_ROUTES` row plus its `JitEvidence` case — the `Cfunc` row is exactly that path realized; a new option is one column absorbed by the existing decorator call; a new lowering producer emits `LoweredSpec` values and adds zero surface here.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
import io
from collections.abc import Callable, Iterable
from contextlib import redirect_stdout
from typing import TYPE_CHECKING, Final, Literal, Protocol, assert_never

from beartype.door import is_bearable
from expression import Error, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    # `numba`/`jax` never import at runtime; the `_capture_*` bodies annotate their handles through these `Protocol`s so a route
    # reads a named capture member rather than a phantom off a bare `object`.
    class Dispatcher(Protocol):  # the numba `CPUDispatcher` a decorated kernel becomes
        signatures: list[object]

        def __call__(self, *args: object) -> object: ...
        def inspect_llvm(self) -> dict[object, str]: ...
        def parallel_diagnostics(self, signature: object = ..., level: int = ...) -> None: ...

    class Jaxpr(Protocol):  # the traced jax program `make_jaxpr` returns; `repr` is the IR text
        def __repr__(self) -> str: ...

    class ShapeDtypeStruct(Protocol):  # a `return_shape=True` out-spec leaf; `.shape`/`.dtype` are the native facts
        shape: tuple[int, ...]
        dtype: object


# --- [TYPES] -------------------------------------------------------------------------------

type Tag = Literal["njit", "vectorize", "cfunc", "jax_jit", "none"]
type Kernel = Callable[..., object]  # the study kernel; numba reads numpy arrays, jax reads its own `Array`
# one route row: (kernel, specimen, backend) -> (compiled callable, captured IR); the bare closure IS
# the row, so the `_JIT_ROUTES` table keys `Tag -> Capture` with no single-field wrapper struct between.
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


@tagged_union(frozen=True)
class JitEvidence:
    tag: Literal["llvm", "ufunc", "cabi", "xla", "host"] = tag()
    llvm: tuple[str, bool, bool, bool, int, int] = case()  # signature, parallel, fastmath, cached, ir_lines, diagnostics_lines
    ufunc: tuple[str, str, str] = case()  # signature, layout, target
    cabi: tuple[str, int] = case()  # signature, address
    xla: tuple[tuple[int, ...], tuple[int, ...], str, int] = case()  # static_argnums, out_shape, out_dtype, jaxpr_lines
    host: tuple[()] = case()

    @staticmethod
    def Llvm(signature: str, *, parallel: bool, fastmath: bool, cached: bool, ir_lines: int, diagnostics_lines: int) -> "JitEvidence":
        return JitEvidence(llvm=(signature, parallel, fastmath, cached, ir_lines, diagnostics_lines))

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
            case JitEvidence(tag="llvm", llvm=(signature, parallel, fastmath, cached, ir_lines, diagnostics_lines)):
                return {
                    "mode": "llvm",
                    "signature": signature,
                    "parallel": parallel,
                    "fastmath": fastmath,
                    "cached": cached,
                    "ir_lines": ir_lines,
                    "diagnostics_lines": diagnostics_lines,
                }
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
        yield Receipt.of("compute.jit", ("emitted", self.backend.tag, facts))


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
        # the non-callable guard is a pure domain reject before the gated import — never a thunk that raises purely so `boundary`
        # can re-catch it; the railed key threads `.bind` so a canonical-encode fault rides the one rail.
        if not is_bearable(kernel, Kernel):
            return Error(BoundaryFault(boundary=(f"jit.{self.tag}", "kernel-not-callable")))
        return ContentIdentity.of(f"jit.{self.tag}", self.identity_buffer(kernel, specimen)).bind(
            lambda key: boundary(f"jit.{self.tag}", lambda: self._compiled(kernel, specimen, key))
        )

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
    import numba

    parallel, fastmath, cache, boundscheck, nogil = backend.njit
    fn: "Dispatcher" = numba.njit(cache=cache, parallel=parallel, fastmath=fastmath, boundscheck=boundscheck, nogil=nogil)(kernel)
    if specimen.is_armed:
        fn(*specimen.args)  # forces one `CPUDispatcher` specialization so `signatures`/`inspect_llvm` are non-empty
    signature = ", ".join(str(s) for s in fn.signatures) or "<unspecialized>"
    llvm = next(iter(fn.inspect_llvm().values()), "") if fn.signatures else ""
    diagnostics = _diagnostics_lines(fn) if parallel and fn.signatures else 0
    return fn, JitEvidence.Llvm(
        signature, parallel=parallel, fastmath=fastmath, cached=cache, ir_lines=llvm.count("\n"), diagnostics_lines=diagnostics
    )


def _capture_vectorize(kernel: Kernel, _specimen: "Specimen", backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    import numba

    signatures, target, layout = backend.vectorize
    fn = numba.guvectorize(list(signatures), layout, target=target)(kernel) if layout else numba.vectorize(list(signatures), target=target)(kernel)
    return fn, JitEvidence.Ufunc(" | ".join(signatures), layout, target)


def _capture_jax(kernel: Kernel, specimen: "Specimen", backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    import jax

    static_argnums, donate_argnums = backend.jax_jit
    fn = jax.jit(kernel, static_argnums=static_argnums, donate_argnums=donate_argnums)
    # `make_jaxpr(return_shape=True)` stages the jaxpr AND returns the out-structure pytree in one trace — no separate `eval_shape`
    # pass; `tree_leaves` reads the leading out-spec leaf so a multi-output pytree never raises `AttributeError` on `.shape`.
    if specimen.is_armed:
        jaxpr: "Jaxpr"
        jaxpr, out_tree = jax.make_jaxpr(kernel, static_argnums=static_argnums, return_shape=True)(*specimen.args)
        out: "ShapeDtypeStruct" = jax.tree_util.tree_leaves(out_tree)[0]
        return fn, JitEvidence.Xla(static_argnums, tuple(out.shape), str(out.dtype), repr(jaxpr).count("\n"))
    return fn, JitEvidence.Xla(static_argnums, (), "<unspecialized>", 0)


def _capture_cfunc(kernel: Kernel, _specimen: "Specimen", backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    import numba

    # `.address` is the callback pointer a quadax/scipy `LowLevelCallable` consumer binds; the compiled object stays
    # Python-callable through `.ctypes`, so the returned kernel is uniformly invocable.
    (signature,) = backend.cfunc
    fn = numba.cfunc(signature)(kernel)
    return fn, JitEvidence.Cabi(signature, int(fn.address))


def _capture_host(kernel: Kernel, _specimen: "Specimen", _backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    return kernel, JitEvidence.Host()


def _diagnostics_lines(fn: "Dispatcher") -> int:
    # `parallel_diagnostics(level=)` writes its report to stdout rather than returning it; the line tally is present-vs-absent
    # evidence that never couples to a specific report phrase.
    sink = io.StringIO()
    with redirect_stdout(sink):
        fn.parallel_diagnostics(level=1)
    return sum(1 for line in sink.getvalue().splitlines() if line.strip())


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
    # the bridge value of the symbolic-to-jit-to-consumer chain: crosses as a VALUE, so no consumer imports symbolic and the DAG
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
