# [PY_COMPUTE_JIT]

The one polymorphic JIT owner collapsing the numba LLVM loop-kernel compiler and the jax XLA array-transform compiler into a single backend-discriminated route table. `JitBackend` is one `@tagged_union` whose `Literal` tag selects the compile route — numba `njit`, numba `vectorize`/`guvectorize`, or jax `jit` — and one `_JIT_ROUTES` `Map[Tag, Capture]` carries the per-route compile-and-capture closure as data — the bare `_capture_*` function IS the row, never a single-field wrapper struct over it — so `compile` indexes one row and folds one `Jitted` rather than a four-arm `match` fanning the shared decorate -> warm-probe -> read-lowered-IR pattern across parallel bodies. The captured lowered-IR evidence rides the `JitEvidence` `@tagged_union` that parameterizes the output the way `JitBackend` parameterizes the input: `Llvm`/`Ufunc`/`Xla`/`Host` each name only their own facts through a total `facts()` projection, never one `dict[str, str]` of default-empty slots restating every route's fields. The `none` passthrough is the unconditional runtime floor reachable for every kernel, so a runtime run without the gated packages returns the host callable as `Host` evidence rather than `Error(Import)`. numba is a loop-kernel compiler and jax is an XLA array-transform compiler; both are admitted here as study-kernel accelerators on one owner, distinct from the Array-API backend dispatch in `numerics/array.md#PAYLOAD` where jax rides `array_namespace` as a backend rather than a wrap. This owner additionally MINTS the lowered-spec vocabulary of the symbolic->jit->consumer lowering chain: `LoweredSpec` is the value `analysis/symbolic.md#DERIVATION` emits off its `_lower` fold (the lambdify/codegen callable plus its arity, dtype signature, and recommended route) and `experiments/study.md#STUDY` and `solvers/quadrature.md#QUADRATURE` compile through `JitBackend.compile` — DAG-lawful by construction: symbolic imports jit downward to TYPE its emission, the consumers import jit ONLY, and a symbolic-derived spec reaches them as a value, never a symbolic import. The `Cfunc` route row compiles a C-ABI callback (`numba.cfunc`) beside the standing `Njit`/`Vectorize`/`JaxJit` rows for the quadax/scipy callback consumers.

## [01]-[INDEX]

- [01]-[JIT]: numba `njit`/`vectorize`/`guvectorize`/`cfunc` and jax `jit` collapsed into one backend-discriminated `JitBackend` owner over one `_JIT_ROUTES` compile-and-capture table, folding the `JitEvidence` discriminated lowered-IR carrier through one railed `ContentIdentity`-keyed `Jitted` receipt, plus the jit-minted `LoweredSpec` vocabulary of the symbolic->jit->consumer lowering bridge — no five-arm dispatch, no parallel receipt union, no stringly evidence map.

## [02]-[JIT]

- Owner: `JitBackend` — the numba-and-jax JIT cases on the one accelerator; `Njit(parallel, fastmath, cache, boundscheck, nogil)`, `Vectorize(signatures, target, layout)`, `Cfunc(signature)`, `JaxJit(static_argnums, donate_argnums)`, and the `none` passthrough each discriminate the compile route, and the `Capture` closure is the one row carrying the route's compile body (decorate the kernel, run the typed `Specimen` warm probe, read the lowered IR into the matching `JitEvidence` case) as data — the bare `_capture_*` function keyed directly in `_JIT_ROUTES`, no single-field row struct between. The numba `njit` row runs `numba.njit(cache=, parallel=, fastmath=, boundscheck=, nogil=)`; the numba `vectorize` row runs `numba.vectorize(signatures, target=)` (or `numba.guvectorize(signatures, layout, target=)` when the `layout` column is present); the `cfunc` row runs `numba.cfunc(signature)` capturing the C-ABI `address` beside the compiled callback; the jax row runs `jax.jit(static_argnums=, donate_argnums=)`. The five routes are five `_JIT_ROUTES` rows over one fold — the decorator, its options, the warm probe, and the lowered-IR read are the row, each row a bare `_capture_*` function reference carrying the full `Capture` arity (the `numba`/`jax` import scoped inside each body, so the table is an eager import-free module constant) — never four `match` arms, never a per-decorator method family, never parallel wrap modules, never a second union restating the same tag, never a forwarding lambda where the capture already holds the row signature.
- Request axis: `Specimen` is the ONE typed warm-probe carrier the routes consume rather than untyped `*probe: object` varargs — `Specimen.of(*args)` folds the concrete argument tuple a route forces one specialization against, and the empty `Specimen()` is the unarmed probe that leaves a route unspecialized, gated by `Specimen.is_armed`. The numba `njit` route calls the dispatcher on the specimen to compile one signature; the jax row threads the same specimen through one `make_jaxpr(return_shape=True)` trace. One carrier parameterizes the input across every route the way `JitEvidence` parameterizes the output, so a route never reads a positional `probe[0]` off an erased tuple and the `none`/`vectorize` routes ignore the specimen total.
- Output axis: `JitEvidence` is the `@tagged_union` lowered-IR carrier discriminating the captured evidence by route — `Llvm(signature, parallel, fastmath, cached, ir_lines, diagnostics_lines)` for the numba njit specialization (`diagnostics_lines` the realized parallel-region report-line count captured off `parallel_diagnostics`, distinct from the requested `parallel` flag), `Ufunc(signature, layout, target)` for the vectorize/guvectorize lift, `Xla(static_argnums, out_shape, out_dtype, jaxpr_lines)` for the traced jax transform, and `Host()` for the passthrough — each owning a total `facts()` projection of native scalars. The XLA case mirrors the input precision: `static_argnums` rides as the native `tuple[int, ...]` the `JaxJit` request carries and `out_shape` as the native `tuple[int, ...]` read off the traced `ShapeDtypeStruct.shape` (with `out_dtype` its `str`-named dtype), never a `repr(static_argnums)`/`repr(out_shape)` stringified slot the `enc_hook=repr` receipt renderer would double-quote — the same native-scalar discipline the `Llvm` case holds for its `bool`/`int` fields. The receipt spreads only the slots the matched case carries, so an LLVM specialization names `signature`/`ir_lines` and an XLA trace names `out_shape`/`jaxpr_lines` without a `Llvm(..., out_shape=(), jaxpr_lines=0)` overload smuggling jax fields onto the numba case. This is the same discriminated-output collapse `analysis/signal.md#DSP` applies to `SignalEvidence` and `analysis/spatial.md#SPATIAL` to its query evidence, never one `dict[str, str]` of every route's union of slots.
- Receipt: `Jitted` is the typed compile-evidence carrier — the compiled callable, the originating `JitBackend` case, the keying `ContentKey`, and the `JitEvidence` lowered-IR union folded off the route. `Jitted.contribute` yields into the `Iterable[Receipt]` the runtime `ReceiptContributor` port declares — one `Receipt.of("compute.jit", ("emitted", self.backend.tag, facts))` row against the two-argument `of(owner, evidence)` contract over the `(Phase, subject, facts)` triple, the `facts` map spreading the route tag, the `content_key.project("hex")` render, and only the slots the matched `JitEvidence` carries through its own `facts()`. `compile` returns the resolved `Jitted` whole on the `Ok` arm rather than threading an inline `Signals.emit` through the compile body, so the consuming solve/study spine harvests the one `contribute` stream where it composes the `Jitted` — the `analysis/signal.md#DSP` `SignalReceipt`-on-the-rail convention, never re-deriving the compile facts downstream.
- Packages: `numba` (`njit`, `vectorize`, `guvectorize`, `cfunc` the C-ABI callback route, the `CPUDispatcher.signatures`/`inspect_llvm`/`parallel_diagnostics` evidence-capture surface read off the `TYPE_CHECKING` `Dispatcher` `Protocol`), `jax` (`jit`, `make_jaxpr(return_shape=True)` returning the jaxpr and the output-structure pytree of `ShapeDtypeStruct` leaves in one trace, `tree_util.tree_leaves` reading the leading out-spec leaf so a multi-output kernel never raises on a `tuple`/`dict` pytree, the leaf `ShapeDtypeStruct.shape`/`.dtype` read off the `TYPE_CHECKING` `ShapeDtypeStruct` `Protocol` into the native `Xla` shape tuple and dtype name), `beartype` (`door.is_bearable` the kernel-callability `TypeIs` guard rejecting a non-callable before the gated import), `expression.collections` (`Map` the `_JIT_ROUTES` route table — the folder's one dispatch rail), `expression` (`tag`/`case`/`tagged_union` the `JitBackend`/`JitEvidence` unions, `Error` the pure non-callable reject arm), `msgspec` (`Struct` the `Jitted`/`Specimen` carriers, both GC-tracked because each holds a container field — `Specimen` the `args` tuple, `Jitted` the `fn` callable and the `backend`/`evidence` unions — never the `gc=False`-on-a-container-carrier deleted form; the route row is the bare `Capture` closure keyed in `_JIT_ROUTES`, not a struct), stdlib (`io.StringIO`/`contextlib.redirect_stdout` capturing the `parallel_diagnostics` stdout report into the `diagnostics_lines` count), runtime (`RuntimeRail`/`boundary`/`BoundaryFault` from `runtime/faults`, `ContentIdentity`/`ContentKey` from `runtime/identity`, `Receipt`/`ReceiptContributor` from `runtime/receipts`).
- Growth: a new loop-kernel compiler is one `JitBackend` case plus one `_JIT_ROUTES` row carrying its compile closure and the matching `JitEvidence` case (the `Cfunc` C-ABI row is exactly that path realized); a new lowering producer emits `LoweredSpec` values and adds zero surface here; a new numba option is one column on the `Njit`/`Vectorize` tuple absorbed by the existing decorator call; a generalized ufunc is the `layout` column already on `Vectorize`; a new captured-IR fact is one slot on the route's `JitEvidence` case plus its `facts()` arm; zero new surface, never a per-backend accelerator owner, never parallel numba-wrap and jax-wrap modules, never a per-mode receipt union mirroring the backend tag, never a four-arm dispatch fold parallel to the route table.

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
    # and never import at runtime; the `_capture_*` bodies annotate the dispatcher/jaxpr handle through
    # these `Protocol`s so a route reads a named capture method rather than a phantom `object` — the
    # sibling `numerics/statistics.md#STATISTICS` typed-result-`Protocol` discipline over `scipy.stats`.
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
    # GC-tracked: `args` is a `tuple` container field, so the leaf-only `gc=False` opt-out the sibling
    # `numerics/statistics.md#STATISTICS` `Reading` and `experiments/study.md#STUDY` `ParamAxis` reserve
    # for container-free leaves never applies here. The typed warm-probe carrier the routes specialize
    # against, replacing untyped `*probe: object` varargs: numba forces one `CPUDispatcher` specialization
    # by calling the dispatcher on `args`, and jax traces one `make_jaxpr(return_shape=True)` over the same
    # `args`. The empty `Specimen()` is the unarmed probe a route ignores; `is_armed` gates the warm call so
    # an unspecialized compile reads `<unspecialized>` rather than raising on an empty trace.
    args: tuple[object, ...] = ()

    @staticmethod
    def of(*args: object) -> "Specimen":
        return Specimen(args)

    @property
    def is_armed(self) -> bool:
        return len(self.args) > 0


@tagged_union(frozen=True)
class JitEvidence:
    # the output is parameterized as tightly as the input: one discriminated carrier per captured-IR
    # shape, not one `dict[str, str]` of every route's slots. `facts()` is the total projection the
    # receipt spreads, so each case names only its own native-scalar slots. `Xla` is its own case
    # rather than an `Llvm(..., out_shape=(), jaxpr_lines=0)` overload smuggling jax fields onto numba.
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
    # the compiled callable beside the route case, the keying `ContentKey`, and the discriminated
    # lowered-IR evidence folded off the route — the typed accelerator-capture receipt, never a
    # generic reported value and never a bare timing.
    fn: Kernel
    backend: "JitBackend"
    content_key: ContentKey
    evidence: JitEvidence

    def contribute(self) -> Iterable[Receipt]:
        # the runtime two-argument `Receipt.of(owner, evidence)` contract over the `(Phase, subject,
        # facts)` triple; the `JitEvidence` `facts()` spreads only the matched case's native scalars
        # beside the route tag and the canonical key render, which the `enc_hook=repr` renderer
        # serializes without a `str()`/`f""` coerce.
        facts = {"backend": self.backend.tag, "content_key": self.content_key.project("hex"), **self.evidence.facts()}
        yield Receipt.of("compute.jit", ("emitted", self.backend.tag, facts))


@tagged_union(frozen=True)
class JitBackend:
    # the compile-route discriminant; each case carries its per-route option payload, dispatched
    # through the `_JIT_ROUTES` table rather than a `match` arm. `tag` is the route key the entry
    # indexes and the `boundary`/receipt subject names.
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
        # the non-callable guard is a pure domain reject — a direct `Error(BoundaryFault(boundary=...))`
        # the way `graduation/handoff.md#GRADUATION` `_clear` rejects a ceiling breach, never a thunk that
        # `raise`s a `TypeError` purely so `boundary` can re-catch it. `is_bearable` narrows the kernel
        # before the gated import so the numba/jax decorate never sees a non-callable. The railed
        # `ContentIdentity.of` key (the `CANONICAL_POLICY` default policy) threads through `.bind` so a
        # canonical-encode fault rides the one rail; `boundary` fences the compile/trace and the gated
        # `ImportError`. `compile` is the `RuntimeRail[Jitted]` boundary owner returning the resolved
        # `Jitted` contributor whole on the `Ok` arm, so the consuming solve/study spine harvests its
        # `contribute` stream where it composes the `Jitted`, never an inline `emit` here — the
        # `analysis/signal.md#DSP` `SignalReceipt`-on-the-rail convention.
        if not is_bearable(kernel, Kernel):
            return Error(BoundaryFault(boundary=(f"jit.{self.tag}", "kernel-not-callable")))
        return ContentIdentity.of(f"jit.{self.tag}", self.identity_buffer(kernel, specimen)).bind(
            lambda key: boundary(f"jit.{self.tag}", lambda: self._compiled(kernel, specimen, key))
        )

    def identity_buffer(self, kernel: Kernel, specimen: "Specimen") -> bytes:
        # the FULL compile identity, owned by the route: every option column that changes the compiled
        # artifact (the numba flag quintet, the vectorize signatures/target/layout, the C signature, the
        # jax static/donate argnum tuples) beside the kernel's qualified name and the specimen
        # dtype/shape signature — so one kernel compiled under two option payloads never shares a
        # `ContentKey`. The closure source is not byte-stable across runs; tag + qualname + probe
        # signature + option row is the stable accelerator-capture buffer.
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
        # the route reads its option payload off `self`; the table key is the tag and the payload is
        # the case tuple, never a monkey-patched attribute or a parallel option-passing channel.
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
    # the real XLA trace boundary in one pass: `make_jaxpr(return_shape=True)` stages the kernel to a
    # jaxpr AND returns the output-structure pytree of `ShapeDtypeStruct` leaves without executing, so the
    # jaxpr text and the out `(shape, dtype)` come from one trace rather than a `make_jaxpr` + separate
    # `eval_shape` double pass. `tree_leaves` reads the leading out-spec leaf, so a multi-output kernel
    # (a `(primal, aux)` or gradient tuple) projects its leading output rather than raising `AttributeError`
    # on a `tuple`/`dict` pytree that has no `.shape`. The native out shape tuple and the dtype name ride
    # the evidence directly — the `enc_hook=repr` receipt renderer serializes the `tuple[int, ...]` without
    # a `repr()` pre-coerce, the `Llvm`-case symmetry.
    if specimen.is_armed:
        jaxpr: "Jaxpr"
        jaxpr, out_tree = jax.make_jaxpr(kernel, static_argnums=static_argnums, return_shape=True)(*specimen.args)
        out: "ShapeDtypeStruct" = jax.tree_util.tree_leaves(out_tree)[0]
        return fn, JitEvidence.Xla(static_argnums, tuple(out.shape), str(out.dtype), repr(jaxpr).count("\n"))
    return fn, JitEvidence.Xla(static_argnums, (), "<unspecialized>", 0)


def _capture_cfunc(kernel: Kernel, _specimen: "Specimen", backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    import numba

    # the C-ABI callback route: `numba.cfunc(signature)` compiles ahead-of-call, and the `.address`
    # integer is the callback pointer a quadax/scipy `LowLevelCallable` consumer binds; the compiled
    # object stays Python-callable through `.ctypes`, so the returned kernel is uniformly invocable.
    (signature,) = backend.cfunc
    fn = numba.cfunc(signature)(kernel)
    return fn, JitEvidence.Cabi(signature, int(fn.address))


def _capture_host(kernel: Kernel, _specimen: "Specimen", _backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    return kernel, JitEvidence.Host()


def _diagnostics_lines(fn: "Dispatcher") -> int:
    # `parallel_diagnostics(level=)` is the canonical numba parallel-evidence source: it writes the
    # loop-fusion/parallel-region report to stdout rather than returning it, so the capture redirects the
    # stream and tallies the emitted report lines. The non-empty report-line count is the realized
    # parallelization evidence — a present-vs-absent fact that does not couple to a specific report
    # phrase — distinct from the `parallel=True` *request* flag the `Llvm` case already carries.
    sink = io.StringIO()
    with redirect_stdout(sink):
        fn.parallel_diagnostics(level=1)
    return sum(1 for line in sink.getvalue().splitlines() if line.strip())


# --- [TABLES] ------------------------------------------------------------------------------

# one row per route carrying the compile-and-read closure as data: decorate the kernel through the
# route's `numba`/`jax` entrypoint, force the `Specimen` specialization, and read the lowered IR into
# the matching `JitEvidence` case. Each `_capture_*` already holds the full `Capture` arity, so the row
# is the bare function reference — no single-field wrapper struct and no forwarding lambda intercedes;
# the gated `numba`/`jax` import stays inside each body, so the table is an eager import-free module
# constant the dispatch indexes directly, anchored after the captures the module-level `Map`
# resolves at load. `_compiled` indexes one row rather than a four-arm `match` fanning the shared
# decorate -> probe -> read pattern across parallel bodies. The `none` row ignores the specimen total.
_JIT_ROUTES: Final[Map[Tag, Capture]] = Map.of_seq([
    ("njit", _capture_njit),
    ("vectorize", _capture_vectorize),
    ("cfunc", _capture_cfunc),
    ("jax_jit", _capture_jax),
    ("none", _capture_host),
])


# --- [EXPORTS] -------------------------------------------------------------------------------


class LoweredSpec(Struct, frozen=True):
    # the jit-MINTED lowered-callable spec vocabulary of the symbolic->jit->consumer bridge:
    # `analysis/symbolic.md#DERIVATION` emits it off its `_lower` fold (the lambdify/codegen callable
    # plus arity, dtype signature, and the recommended route row), and `experiments/study.md#STUDY` /
    # `solvers/quadrature.md#QUADRATURE` compile it — a symbolic-derived spec crosses as a VALUE, so
    # no consumer imports symbolic and the DAG carries no back-edge.
    kernel: Kernel
    name: str
    arity: int
    signature: str = ""
    route: JitBackend = JitBackend.Passthrough()

    def compiled(self, specimen: Specimen = Specimen()) -> "RuntimeRail[Jitted]":
        return self.route.compile(self.kernel, specimen)
```
