# [PY_COMPUTE_JIT]

The one polymorphic JIT owner collapsing the numba LLVM loop-kernel compiler and the jax XLA array-transform compiler into a single backend-discriminated route table. `JitBackend` is one `@tagged_union` whose `Literal` tag selects the compile route — numba `njit`, numba `vectorize`/`guvectorize`, or jax `jit` — and one `_JIT_ROUTES` `FrozenDict[Tag, JitRoute]` carries the per-route compile-and-capture closure as data, so `compile` indexes one row and folds one `Jitted` rather than a four-arm `match` fanning the shared decorate -> warm-probe -> read-lowered-IR pattern across parallel bodies. The captured lowered-IR evidence rides the `JitEvidence` `@tagged_union` that parameterizes the output the way `JitBackend` parameterizes the input: `Llvm`/`Ufunc`/`Xla`/`Host` each name only their own facts through a total `facts()` projection, never one `dict[str, str]` of default-empty slots restating every route's fields. The `none` passthrough is the unconditional cp315 floor reachable for every kernel, so a cp315 run without the gated wheels returns the host callable as `Host` evidence rather than `Error(Import)`. numba is a loop-kernel compiler and jax is an XLA array-transform compiler; both are admitted here as study-kernel accelerators on one owner, distinct from the Array-API backend dispatch in `numerics/array.md#PAYLOAD` where jax rides `array_namespace` as a backend rather than a wrap.

## [01]-[INDEX]

- [01]-[JIT]: numba `njit`/`vectorize`/`guvectorize` and jax `jit` collapsed into one backend-discriminated `JitBackend` owner over one `_JIT_ROUTES` compile-and-capture table, folding the `JitEvidence` discriminated lowered-IR carrier through one railed `ContentIdentity`-keyed `Jitted` receipt — no four-arm dispatch, no parallel receipt union, no stringly evidence map.

## [02]-[JIT]

- Owner: `JitBackend` — the numba-and-jax JIT cases on the one accelerator; `Njit(parallel, fastmath, cache, boundscheck, nogil)`, `Vectorize(signatures, target, layout)`, `JaxJit(static_argnums, donate_argnums)`, and the `none` passthrough each discriminate the compile route, and `JitRoute` is the one row carrying the route's `compile` closure (decorate the kernel, run the typed `Specimen` warm probe, read the lowered IR into the matching `JitEvidence` case) as data. The numba `njit` row runs `numba.njit(cache=, parallel=, fastmath=, boundscheck=, nogil=)`; the numba `vectorize` row runs `numba.vectorize(signatures, target=)` (or `numba.guvectorize(signatures, layout, target=)` when the `layout` column is present); the jax row runs `jax.jit(static_argnums=, donate_argnums=)`. The four routes are four `_JIT_ROUTES` rows over one fold — the decorator, its options, the warm probe, and the lowered-IR read are the row, each row a bare `_capture_*` function reference carrying the full `Capture` arity (the `numba`/`jax` import scoped inside each body, so the table is an eager import-free module constant) — never four `match` arms, never a per-decorator method family, never parallel wrap modules, never a second union restating the same tag, never a forwarding lambda where the capture already holds the row signature.
- Request axis: `Specimen` is the ONE typed warm-probe carrier the routes consume rather than untyped `*probe: object` varargs — `Specimen.of(*args)` folds the concrete argument tuple a route forces one specialization against, and the empty `Specimen()` is the unarmed probe that leaves a route unspecialized, gated by `Specimen.is_armed`. The numba `njit` route calls the dispatcher on the specimen to compile one signature; the jax row threads the same specimen through one `make_jaxpr(return_shape=True)` trace. One carrier parameterizes the input across every route the way `JitEvidence` parameterizes the output, so a route never reads a positional `probe[0]` off an erased tuple and the `none`/`vectorize` routes ignore the specimen total.
- Output axis: `JitEvidence` is the `@tagged_union` lowered-IR carrier discriminating the captured evidence by route — `Llvm(signature, parallel, fastmath, cached, ir_lines, diagnostics_lines)` for the numba njit specialization (`diagnostics_lines` the realized parallel-region report-line count captured off `parallel_diagnostics`, distinct from the requested `parallel` flag), `Ufunc(signature, layout, target)` for the vectorize/guvectorize lift, `Xla(static_argnums, out_shape, jaxpr_lines)` for the traced jax transform, and `Host()` for the passthrough — each owning a total `facts()` projection of native scalars. The receipt spreads only the slots the matched case carries, so an LLVM specialization names `signature`/`ir_lines` and an XLA trace names `out_shape`/`jaxpr_lines` without a `Llvm(..., out_shape="", jaxpr_lines=0)` overload smuggling jax fields onto the numba case. This is the same discriminated-output collapse `analysis/signal.md#SIGNAL` applies to `SignalEvidence` and `analysis/spatial.md#SPATIAL` to its query evidence, never one `dict[str, str]` of every route's union of slots.
- Entry: `JitBackend.compile(kernel, specimen)` rejects a non-callable kernel as a pure `Error(BoundaryFault(boundary=(f"jit.{self.tag}", "kernel-not-callable")))` — the `is_bearable(kernel, Kernel)` `TypeIs` guard narrowing before the gated import, the `graduation/handoff.md#GRADUATION` `_clear` direct-reject shape, never a thunk that `raise`s a `TypeError` purely so `boundary` re-catches it. It then threads the railed `ContentIdentity.of` `ContentKey` over the kernel-source-plus-specimen buffer through `.bind` (the `CANONICAL_POLICY` default policy, never a fresh `IdentityPolicy()` allocation), then enters one `boundary(f"jit.{self.tag}", ...)` so a numba `TypingError`/`UnsupportedError`, a jax trace fault, or the gated `ImportError` converts to a `BoundaryFault` exactly once and the digest fault rides the same rail rather than collapsing to a bare `ContentKey` — the sibling `numerics/statistics.md#STATISTICS` join shape. The body indexes `_JIT_ROUTES[self.tag]`, runs the row's `_capture_*` function into a `(callable, JitEvidence)` pair, and folds one `Jitted`. The numba njit row warms the `Specimen` specialization, then reads `CPUDispatcher.signatures` for the resolved signature, `inspect_llvm` for the lowered-IR line count, and — only under a `parallel=True` request — captures the `parallel_diagnostics` report (the `.api` IMPLEMENTATION_LAW's canonical parallelization-evidence source) into the realized `diagnostics_lines` report-line count, folding `Llvm`. The numba vectorize row records the ufunc layout signatures and target as `Ufunc`. The jax row traces the specimen once through `make_jaxpr(return_shape=True)`, reading the jaxpr text and the output `ShapeDtypeStruct` from one trace — the real XLA trace boundary, not a hardcoded `"traced"` literal nor a `make_jaxpr` + separate `eval_shape` double pass — folding `Xla`. The `none` passthrough returns the host kernel as `Host` so a cp315 study composes the same `Jitted` whether or not the wheel resolved.
- Receipt: `Jitted` is the typed compile-evidence carrier — the compiled callable, the originating `JitBackend` case, the keying `ContentKey`, and the `JitEvidence` lowered-IR union folded off the route. `Jitted.contribute` yields into the `Iterable[Receipt]` the runtime `ReceiptContributor` port declares — one `Receipt.of("compute.jit", ("emitted", self.backend.tag, facts))` row against the two-argument `of(owner, evidence)` contract over the `(Phase, subject, facts)` triple, the `facts` map spreading the route tag, the `content_key.project("hex")` render, and only the slots the matched `JitEvidence` carries through its own `facts()`. Emission rides the `runtime/observability/receipts#RECEIPT` `@receipted` aspect on the `Ok` arm, never an inline `Signals.emit` threaded through the compile body. A graduated kernel routes its receipt through the solve/study spine, never re-deriving the compile facts downstream.
- Packages: `numba` (`njit`, `vectorize`, `guvectorize`, the `CPUDispatcher.signatures`/`inspect_llvm`/`parallel_diagnostics` evidence-capture surface read off the `TYPE_CHECKING` `Dispatcher` `Protocol`), `jax` (`jit`, `make_jaxpr(return_shape=True)` returning the jaxpr and the output `ShapeDtypeStruct` in one trace), `beartype` (`door.is_bearable` the kernel-callability `TypeIs` guard rejecting a non-callable before the gated import, `FrozenDict` the `_JIT_ROUTES` route table), `expression` (`tag`/`case`/`tagged_union` the `JitBackend`/`JitEvidence` unions, `Error` the pure non-callable reject arm), `msgspec` (`Struct` the `Jitted`/`JitRoute`/`Specimen` carriers, all GC-tracked because each holds a container or closure field — `Specimen` the `args` tuple, `JitRoute` the `capture` closure, `Jitted` the `fn` callable and the `backend`/`evidence` unions — never the `gc=False`-on-a-container-carrier deleted form), stdlib (`io.StringIO`/`contextlib.redirect_stdout` capturing the `parallel_diagnostics` stdout report into the `diagnostics_lines` count), runtime (`RuntimeRail`/`boundary`/`BoundaryFault` from `runtime/faults`, `ContentIdentity`/`ContentKey` from `runtime/content_identity`, `Receipt`/`ReceiptContributor` from `runtime/receipts`).
- Growth: a new loop-kernel compiler is one `JitBackend` case plus one `_JIT_ROUTES` row carrying its compile closure and the matching `JitEvidence` case; a new numba option is one column on the `Njit`/`Vectorize` tuple absorbed by the existing decorator call; a generalized ufunc is the `layout` column already on `Vectorize`; a new captured-IR fact is one slot on the route's `JitEvidence` case plus its `facts()` arm; zero new surface, never a per-backend accelerator owner, never parallel numba-wrap and jax-wrap modules, never a per-mode receipt union mirroring the backend tag, never a four-arm dispatch fold parallel to the route table.
- Boundary: the `none` passthrough runs unconditionally on cp315; `numba`/`llvmlite` and `jax`/`jaxlib` carry no cp315 wheel, so the numba and jax routes are authored against the documented API behind one gated import each with the passthrough as the reachable floor for every kernel. The deleted forms: a separate numba accelerator owner beside a jax JIT owner; a per-decorator method family; parallel wrap bodies; a four-arm `match` dispatch where `_JIT_ROUTES` indexes the route; a second `JitReceipt` union restating the compile tag; a generic `IReceipt` over the compile evidence; one `dict[str, str]` evidence map of every route's slots where `JitEvidence` discriminates the output; a `Receipt.of("emitted", owner, subject, facts)` four-positional call against the two-argument contract; a `contribute` returning one bare `Receipt` against the `Iterable[Receipt]` port; an `f""`-pre-formatted facts map where the renderer carries native scalars; untyped `*probe: object` varargs where `Specimen` types the warm probe; a `gc=False` `Specimen` opting the `args`-`tuple`-carrying struct out of GC tracking against the container-free-leaf-only opt-out (the `numerics/statistics.md#STATISTICS` `Reading` and `experiments/study.md#STUDY` `ParamAxis` rule); a `_reject` thunk that `raise`s a `TypeError` inside `boundary` purely so the fence re-catches it where the non-callable guard is a pure `Error(BoundaryFault(boundary=...))` direct reject the `graduation/handoff.md#GRADUATION` `_clear` shape holds; a fresh `IdentityPolicy()` allocation passed to `ContentIdentity.of` where the `CANONICAL_POLICY` default already keys the canonical path; a bare `ContentKey` use dropping the railed digest; a `Callable[..., np.ndarray]` kernel signature falsely pinning the jax `Array` return where `Kernel = Callable[..., object]` admits both substrates and a dead `import numpy as np` the weak return position never references is dropped; a forwarding `lambda kernel, specimen, backend: _capture_*(...)` route row where the `_capture_*` function already carries the `Capture` arity as a bare reference; a `make_jaxpr` + separate `eval_shape` double trace where `make_jaxpr(return_shape=True)` reads the jaxpr and the out shape in one pass; an `Llvm` case that emits only the requested `parallel` flag as if it were the realized parallel-fusion decision where `parallel_diagnostics` carries the captured `diagnostics_lines` count; and a jax-as-array-backend arm here (that rides `array_namespace` at array admission, not a kernel wrap).

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
import io
from collections.abc import Callable, Iterable
from contextlib import redirect_stdout
from typing import TYPE_CHECKING, Literal, Protocol, assert_never

from beartype import FrozenDict
from beartype.door import is_bearable
from expression import Error, case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    # the gated `numba`/`jax` artifacts, typed structurally because the wheels carry no cp315 build
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


# --- [TYPES] -------------------------------------------------------------------------------

type Tag = Literal["njit", "vectorize", "jax_jit", "none"]
type Kernel = Callable[..., object]  # the study kernel; numba reads numpy arrays, jax reads its own `Array`
# one route's compile-and-read closure: (kernel, specimen, backend) -> (compiled callable, captured IR)
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
    # rather than an `Llvm(..., out_shape="", jaxpr_lines=0)` overload smuggling jax fields onto numba.
    tag: Literal["llvm", "ufunc", "xla", "host"] = tag()
    llvm: tuple[str, bool, bool, bool, int, int] = case()  # signature, parallel, fastmath, cached, ir_lines, diagnostics_lines
    ufunc: tuple[str, str, str] = case()                   # signature, layout, target
    xla: tuple[str, str, int] = case()                     # static_argnums, out_shape, jaxpr_lines
    host: tuple[()] = case()

    @staticmethod
    def Llvm(signature: str, *, parallel: bool, fastmath: bool, cached: bool, ir_lines: int, diagnostics_lines: int) -> "JitEvidence":
        return JitEvidence(llvm=(signature, parallel, fastmath, cached, ir_lines, diagnostics_lines))

    @staticmethod
    def Ufunc(signature: str, layout: str, target: str) -> "JitEvidence":
        return JitEvidence(ufunc=(signature, layout, target))

    @staticmethod
    def Xla(static_argnums: str, out_shape: str, jaxpr_lines: int) -> "JitEvidence":
        return JitEvidence(xla=(static_argnums, out_shape, jaxpr_lines))

    @staticmethod
    def Host() -> "JitEvidence":
        return JitEvidence(host=())

    def facts(self) -> dict[str, object]:
        match self:
            case JitEvidence(tag="llvm", llvm=(signature, parallel, fastmath, cached, ir_lines, diagnostics_lines)):
                return {"mode": "llvm", "signature": signature, "parallel": parallel, "fastmath": fastmath, "cached": cached, "ir_lines": ir_lines, "diagnostics_lines": diagnostics_lines}
            case JitEvidence(tag="ufunc", ufunc=(signature, layout, target)):
                return {"mode": "gufunc" if layout else "ufunc", "signature": signature, "layout": layout, "target": target}
            case JitEvidence(tag="xla", xla=(static_argnums, out_shape, jaxpr_lines)):
                return {"mode": "xla", "static_argnums": static_argnums, "out_shape": out_shape, "jaxpr_lines": jaxpr_lines}
            case JitEvidence(tag="host", host=()):
                return {"mode": "none"}
            case unreachable:
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


class JitRoute(Struct, frozen=True):  # GC-tracked: carries the `capture` closure
    capture: Capture  # decorate the kernel, run the `Specimen` probe, read the lowered IR into a `JitEvidence` case


@tagged_union(frozen=True)
class JitBackend:
    # the compile-route discriminant; each case carries its per-route option payload, dispatched
    # through the `_JIT_ROUTES` table rather than a `match` arm. `tag` is the route key the entry
    # indexes and the `boundary`/receipt subject names.
    tag: Tag = tag()
    njit: tuple[bool, bool, bool, bool, bool] = case()              # parallel, fastmath, cache, boundscheck, nogil
    vectorize: tuple[tuple[str, ...], Literal["cpu", "parallel"], str] = case()  # signatures, target, layout
    jax_jit: tuple[tuple[int, ...], tuple[int, ...]] = case()       # static_argnums, donate_argnums
    none: tuple[()] = case()

    @staticmethod
    def Njit(*, parallel: bool = False, fastmath: bool = False, cache: bool = True, boundscheck: bool = False, nogil: bool = False) -> "JitBackend":
        return JitBackend(njit=(parallel, fastmath, cache, boundscheck, nogil))

    @staticmethod
    def Vectorize(signatures: tuple[str, ...], *, target: Literal["cpu", "parallel"] = "cpu", layout: str = "") -> "JitBackend":
        return JitBackend(vectorize=(signatures, target, layout))

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
        # `ImportError`. `compile` is the `RuntimeRail[Jitted]` boundary owner — the study spine harvests
        # the resolved `Jitted` contributor through the `@receipted` aspect on the `Ok` arm, never an
        # inline `emit` here, the convention `analysis/signal.md#SIGNAL` holds.
        if not is_bearable(kernel, Kernel):
            return Error(BoundaryFault(boundary=(f"jit.{self.tag}", "kernel-not-callable")))
        buffer = _identity_buffer(kernel, specimen)
        return ContentIdentity.of(f"jit.{self.tag}", buffer).bind(
            lambda key: boundary(f"jit.{self.tag}", lambda: self._compiled(kernel, specimen, key))
        )

    def _compiled(self, kernel: Kernel, specimen: "Specimen", key: ContentKey) -> "Jitted":
        # the route reads its option payload off `self`; the table key is the tag and the payload is
        # the case tuple, never a monkey-patched attribute or a parallel option-passing channel.
        fn, evidence = _JIT_ROUTES[self.tag].capture(kernel, specimen, self)
        return Jitted(fn, self, key, evidence)


# --- [OPERATIONS] --------------------------------------------------------------------------

def _identity_buffer(kernel: Kernel, specimen: "Specimen") -> bytes:
    # the canonical content-identity buffer the `whole` modality keys (named `buffer`, not `seed`, to
    # stay distinct from the `ContentIdentity.of` `seed: Option[U64]` override): the kernel's qualified
    # name joined with the specimen shape signature, so two compiles of the same kernel against the same
    # probe key identically and a changed probe re-keys. The closure source is not byte-stable across
    # runs; the qualname plus the probe dtype/shape is the stable accelerator-capture buffer.
    probe = "|".join(f"{type(a).__name__}:{getattr(a, 'shape', ())}:{getattr(a, 'dtype', '')}" for a in specimen.args)
    return f"{getattr(kernel, '__qualname__', repr(kernel))}|{probe}".encode()


def _capture_njit(kernel: Kernel, specimen: "Specimen", backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    import numba

    parallel, fastmath, cache, boundscheck, nogil = backend.njit
    fn: "Dispatcher" = numba.njit(cache=cache, parallel=parallel, fastmath=fastmath, boundscheck=boundscheck, nogil=nogil)(kernel)
    if specimen.is_armed:
        fn(*specimen.args)  # forces one `CPUDispatcher` specialization so `signatures`/`inspect_llvm` are non-empty
    signature = ", ".join(str(s) for s in fn.signatures) or "<unspecialized>"
    llvm = next(iter(fn.inspect_llvm().values()), "") if fn.signatures else ""
    diagnostics = _diagnostics_lines(fn) if parallel and fn.signatures else 0
    return fn, JitEvidence.Llvm(signature, parallel=parallel, fastmath=fastmath, cached=cache, ir_lines=llvm.count("\n"), diagnostics_lines=diagnostics)


def _capture_vectorize(kernel: Kernel, _specimen: "Specimen", backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    import numba

    signatures, target, layout = backend.vectorize
    fn = (
        numba.guvectorize(list(signatures), layout, target=target)(kernel)
        if layout
        else numba.vectorize(list(signatures), target=target)(kernel)
    )
    return fn, JitEvidence.Ufunc(" | ".join(signatures), layout or "<elementwise>", target)


def _capture_jax(kernel: Kernel, specimen: "Specimen", backend: "JitBackend") -> tuple[Kernel, JitEvidence]:
    import jax

    static_argnums, donate_argnums = backend.jax_jit
    fn = jax.jit(kernel, static_argnums=static_argnums, donate_argnums=donate_argnums)
    # the real XLA trace boundary in one pass: `make_jaxpr(return_shape=True)` stages the kernel to a
    # jaxpr AND returns the output `ShapeDtypeStruct` without executing, so the jaxpr text and the out
    # shape come from one trace rather than a `make_jaxpr` + separate `eval_shape` double pass.
    if specimen.is_armed:
        jaxpr: "Jaxpr"
        jaxpr, out_shape = jax.make_jaxpr(kernel, static_argnums=static_argnums, return_shape=True)(*specimen.args)
        return fn, JitEvidence.Xla(repr(static_argnums), repr(out_shape), repr(jaxpr).count("\n"))
    return fn, JitEvidence.Xla(repr(static_argnums), "<unknown-shape>", 0)


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
# is the bare function reference rather than a forwarding lambda — the gated `numba`/`jax` import stays
# inside each body, so the table is an eager import-free module constant the dispatch indexes directly,
# anchored after the captures it references because the module-level `FrozenDict` resolves them at load.
# `_compiled` indexes one row rather than a four-arm `match` fanning the shared decorate -> probe ->
# read pattern across parallel bodies. The `none` row ignores the specimen and reads `Host` total.
_JIT_ROUTES: FrozenDict[Tag, JitRoute] = FrozenDict({
    "njit": JitRoute(_capture_njit),
    "vectorize": JitRoute(_capture_vectorize),
    "jax_jit": JitRoute(_capture_jax),
    "none": JitRoute(_capture_host),
})
```

## [03]-[RESEARCH]

- [NUMBA_JIT]: `numba`/`llvmlite` carry the `python_version<'3.15'` marker; the `njit(cache=, parallel=, fastmath=, boundscheck=, nogil=)`, `vectorize(signatures, target=)`, `guvectorize(signatures, layout, target=)`, and the `CPUDispatcher.signatures`/`parallel_diagnostics`/`inspect_llvm` evidence-capture spellings verify against `compute/.api/numba.md` (ENTRYPOINTS [01]/[04]/[05]; IMPLEMENTATION_LAW DISPATCHER_TOPOLOGY names `inspect_llvm`/`parallel_diagnostics` as the IR/parallel-decision capture surface, and `nogil=True` releases the GIL for the compiled region) once the numba wheel resolves on the gated band. `njit` is `jit(nopython=True)`; a decorated kernel becomes a `CPUDispatcher` that specializes one compiled signature per argument-type tuple, so `signatures` reads `<unspecialized>` and `inspect_llvm` is empty until a warm call — the typed `Specimen` warm probe forces that one specialization so the lowered-IR evidence is non-empty at capture, and the `Dispatcher`/`Jaxpr` `Protocol`s under `TYPE_CHECKING` annotate the dispatcher and jaxpr handles without importing the gated wheel, the sibling `numerics/statistics.md#STATISTICS` typed-result-`Protocol` discipline. `parallel_diagnostics(level=)` writes its loop-fusion/parallel-region report to stdout rather than returning a value, so a `parallel=True` njit capture redirects it through `io.StringIO`/`contextlib.redirect_stdout` and tallies the non-empty report lines into the realized `diagnostics_lines` slot — the canonical parallelization evidence captured as a present-vs-absent fact rather than the requested `parallel` flag restated, and not coupled to a specific report phrase. The `none` passthrough runs unconditionally on cp315.
- [JAX_JIT]: `jax`/`jaxlib` carry the `python_version<'3.15'` marker; the `jit(fun, static_argnums=, donate_argnums=)` and `make_jaxpr(fun, static_argnums=, return_shape=True)` spellings verify against `compute/.api/jax.md` (ENTRYPOINTS compile-and-differentiation [01], vectorization-and-staging [05] `make_jaxpr` whose `return_shape=False` default flips to `True` to also return the output `ShapeDtypeStruct` PUBLIC_TYPES [02]) on the gated band. `static_argnums` marks compile-time-constant arguments whose change retraces; `make_jaxpr(return_shape=True)` traces the kernel once without executing and returns the jaxpr plus the output `ShapeDtypeStruct` in one pass, so the XLA evidence is the real traced jaxpr text and output shape rather than a hardcoded literal and rather than a `make_jaxpr` + separate `eval_shape` double trace. The transform requires a pure kernel, the same purity the solver and study kernels already hold. jax rides the Array-API namespace as a backend at `numerics/array.md#PAYLOAD`; here it is admitted only as the XLA kernel-transform arm, never duplicating the array admission.
- [ROUTE_TABLE]: `_JIT_ROUTES` is the `beartype.FrozenDict[Tag, JitRoute]` the dispatch resolves by `JitBackend.tag`, the same callable-row table `numerics/statistics.md#STATISTICS` builds for `_STAT_ROUTES` and `analysis/signal.md#SIGNAL` for its wavelet routes — each row a `JitRoute` carrying the route's `_capture_*` function as a bare reference (every capture holds the full `Capture` arity, so no forwarding lambda intercedes), so a new accelerator is one `JitBackend` case plus one row rather than a fifth `match` arm. The `numba`/`jax` import stays boundary-scoped inside each `_capture_*` body per the manifest import policy, so the table is an eager import-free module constant anchored after the captures it references; the route reads its option payload off the bound `JitBackend` the `compile` entry threads, so the table key is the tag and the payload is the case tuple, never a parallel option-passing channel. The `JitEvidence` output union and the `Specimen` input carrier are the input-and-output parameterization the `transport/roots#RESOURCE` `@overload` ladder and the `analysis/signal.md#SIGNAL` `SignalEvidence` collapse share — the compile owner parameterized over both the warm probe it consumes and the lowered-IR shape it emits.
- [RECEIPT_SHAPE]: `Jitted.contribute` yields into the `Iterable[Receipt]` the `runtime/observability/receipts#RECEIPT` `ReceiptContributor` Protocol declares (`contribute(self) -> Iterable[Receipt]`), and the row is `Receipt.of("compute.jit", ("emitted", backend_tag, facts))` — the two-argument shape-polymorphic factory over the `(Phase, subject, facts)` `Evidence` triple, never the four-positional `Receipt.of("emitted", owner, subject, facts)` the factory does not admit and never a bare `Receipt` return against the `Iterable[Receipt]` port. `JitEvidence` parameterizes the output shape and owns its own `facts()` projection, so the receipt spreads only the slots the matched case carries beside the `backend` tag and the `content_key.project("hex")` render, the dedicated `Xla` case keeping a jaxpr capture off the LLVM slots. `compile` is the `RuntimeRail[Jitted]` boundary owner (the error arm carries no contributor), so emission is the `@receipted` aspect harvesting the resolved `Jitted` contributor on the `Ok` arm — the same convention `analysis/signal.md#SIGNAL` and `numerics/statistics.md#STATISTICS` hold, the receipt the contributor and the rail the boundary form. The `ContentIdentity.of` key threads through `.bind` per the `evidence/identity#IDENTITY` `ContentIdentity.of(fmt, source, policy=CANONICAL_POLICY, *, view="value", seed=Nothing) -> RuntimeRail[ContentKey]` contract and the sibling `numerics/array.md#PAYLOAD` threading idiom, resting on the `CANONICAL_POLICY` default rather than a fresh `IdentityPolicy()` allocation, so two compiles of the same kernel against the same `Specimen` key identically and a bare-`ContentKey` assignment dropping the canonical-encode rail is the deleted form.
