# [PY_COMPUTE_SYMBOLIC]

The one classical computer-algebra owner. A derivation is a `Block[SymbolicOp]` left-folded over an `ExprForm` to one typed `SymbolicReceipt`: every non-terminal op rewrites the carried expression and the terminal op produces the artifact, so a `Refine -> simplify -> diff -> Lower(numpy)` derivation is one fold, never a `pre`/`terminal` split. The terminal artifact is one discriminated `Outcome` the receipt carries whole, and the input is parameterized as tightly as the output — `ExprForm` discriminates a `str` spelling, a `MatrixForm`, or a constructed `Expr` through one `derive` entry. No learned or generative symbolic search enters this owner.

This is the core solver route with a gating law per backend: `sympy` is pure-Python and imports on the runtime, so calculus, rewrite, solve, matrix algebra, assumption logic, number theory, heuristic numeric evaluation, and the source-printer family run as live core; `python-flint`'s exact kernels and the certified-ball `Evaluate` precision row gate on the worker lane; `Lower(jax)` reads `jax` at usage on the jaxlib floor; `Lower(native)`'s `autowrap`/`ufuncify` rows gate on a host C/Fortran toolchain. The derivation keys through the runtime `ContentIdentity` over the canonical `SymbolicPayload`, so a repeated derivation at identical `(form, spec, ops)` is a cache hit by reference; a `source` derivation graduates outward on the symbolic `HandoffAxis` case once stable and reproducible.

## [01]-[INDEX]

- [01]-[OP]: the `SymbolicOp` bounded vocabulary — staging expression-to-expression rows composing ahead of one terminal artifact row, with the `GroundDomain` and `Precision` accelerator axes.
- [02]-[DERIVATION]: `SymbolicDerivation.derive` left-folding a `Block[SymbolicOp]` over an `ExprForm` to one content-keyed `SymbolicReceipt`.

## [02]-[OP]

`SymbolicOp` is the bounded vocabulary of what one fold step does to an expression: the rows collapse the sibling entrypoints into one discriminant the derivation folds, staging rows composing in any number ahead of one terminal artifact row. Each row's exhaustive member roster is its `[CASE_DATA]` `StrEnum`; the `[SYMPY_SURFACE]` column names the anchoring sympy call.

| [INDEX] | [ROW]          | [KIND]   | [CASE_DATA]                               | [SYMPY_SURFACE]                                              |
| :-----: | :------------- | :------- | :---------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `Calculus`     | staging  | `(CalculusKind, int)`                     | `diff`/`integrate`/`limit`/`series`/`summation`              |
|  [02]   | `Rewrite`      | staging  | `RewritePass`                             | `simplify`/`factor`/`expand`/`collect`/`trigsimp`/`apart`    |
|  [03]   | `Substitute`   | staging  | `(SubstituteMode, Map[str, str])`         | `Expr.subs`/`Expr.replace` `Wild`/`Expr.rewrite` basis       |
|  [04]   | `Refine`       | staging  | `AssumptionPredicate`                     | `refine(expr, Q.<pred>(sym))` under `SymbolSpec`             |
|  [05]   | `Solve`        | terminal | `(SolveRoute, SolveDomain, GroundDomain)` | `solve`/`solveset(domain=)`/`nsolve`/`dsolve` + `Poly` roots |
|  [06]   | `LinAlg`       | terminal | `(MatrixRoute, GroundDomain)`             | dense `Matrix`: `eigenvals`/`det`/`charpoly`/`inv` + decomps |
|  [07]   | `NumberTheory` | terminal | `(NumberRoute, GroundDomain)`             | `factorint`/`primerange`/`isprime`/`gcd`/`lcm`               |
|  [08]   | `Evaluate`     | terminal | `(int, Precision)`                        | `N(expr, digits)` or `flint.good` `arb`-ball `rad()`         |
|  [09]   | `Lower`        | terminal | `LowerBackend`                            | `lambdify(numpy\|jax)`/`ufuncify`/`autowrap`                 |
|  [10]   | `Codegen`      | terminal | `(CodeTarget, str)`                       | per-`CodeTarget` printer `ccode`/`fcode`/`rust_code`         |

- `Calculus`: the unevaluated `Derivative`/`Integral`/`Sum` node forces through `.doit()` and `series` trims its `Order` term in the same row, never a second method.
- `Substitute`: map keys and values are spellings resolved against the live `SymbolSpec`, never raw strings escaping the boundary.
- `Refine`: assumptions are derivation inputs the `SymbolSpec` declares, never a post-hoc filter.
- `Solve`: every polynomial route carries a real `metric` — discriminant magnitude, resultant magnitude, or `nsolve` residual — never a dead `0.0`.
- `LinAlg`: `GroundDomain.FLINT` accelerates only the `_FLINT_MATRIX_ROUTES` exact-over-rationals subset, and `MINPOLY` is FLINT-only — sympy `Matrix` owns no minimal-polynomial kernel, so the sympy ground rails a fenced typed fault.
- `NumberTheory`: GCD/LCM reinterpret unary by operand shape — a polynomial reads `gcd(p, p')`, a leaf integer its genuine divisor-lattice structure, never the vacuous `gcd(n, n) == n` tautology.
- `Evaluate`: `CERTIFIED` re-evaluates through a `python-flint` `arb` ball whose `rad()` is the certified error bound `HEURISTIC` lacks.
- `Codegen`: a new target is one `CodeTarget` value plus one `_CODE_PRINTER` row, never a parallel emitter.

## [03]-[DERIVATION]

`SymbolicDerivation` threads an assumption-carrying `SymbolSpec` over an `ExprForm` and left-folds a `Block[SymbolicOp]` pipeline to one typed `SymbolicReceipt` from one shared `cse` lowering, never parallel entries per artifact.

- Entry: `derive(form, spec, *ops)` is the one railed entrypoint — there is no second un-railed constructor; the graduation gate reads the `source` evidence through the symbolic `HandoffAxis` case.
- Cases: `ExprForm` is the polymorphic input — a `str` spelling, a `MatrixForm` of cell spellings, or a constructed `Expr` — discriminated by one `derive` entry rather than `derive`/`derive_matrix`/`derive_expr` siblings.
- Auto: the runtime content owner mints the derivation key over the canonical `SymbolicPayload`, never a hand-rolled canonical encode; two derivations differing in assumption context, op pipeline, or terminal route key distinctly.
- Receipt: `outcome` is the terminal `Outcome` case owning its `facts()` projection, and the carried `LoweredSpec` is a VALUE consumers project off the outcome, never a receipt fact; a derivation graduates through the self-wired `graduates` producer — `Precision.CERTIFIED` ships its arb radius, a reproducibly `HEURISTIC` run ships zero instability — against the `_CEILING` family row.
- Growth: a new calculus transform is one `CalculusKind` row plus one `_CALCULUS` entry; a new rewrite pass is one `RewritePass` row; a new solve route, matrix extraction, or number-theoretic query is one row on its existing case; a new lowering backend is one `LowerBackend` row plus one `_LOWER_ROUTE` row; a new code target is one `CodeTarget` row plus one `_CODE_PRINTER` entry; a new artifact shape is one `Outcome` case plus its `facts()` arm and one terminal `apply` arm.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.compute.graduation.handoff import EvidenceScope, GraduationReceipt, HandoffAxis, evidence_run
from rasm.compute.numerics.jit import JitBackend, LoweredSpec
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    from sympy import Expr

# --- [TYPES] ----------------------------------------------------------------------------


class CalculusKind(StrEnum):
    DIFF = "diff"
    INTEGRATE = "integrate"
    LIMIT = "limit"
    SERIES = "series"
    SUMMATION = "summation"


class RewritePass(StrEnum):
    SIMPLIFY = "simplify"
    FACTOR = "factor"
    EXPAND = "expand"
    COLLECT = "collect"
    CANCEL = "cancel"
    TRIGSIMP = "trigsimp"
    RADSIMP = "radsimp"
    RATSIMP = "ratsimp"
    APART = "apart"
    TOGETHER = "together"
    POWSIMP = "powsimp"
    LOGCOMBINE = "logcombine"
    NSIMPLIFY = "nsimplify"


class SubstituteMode(StrEnum):
    SUBS = "subs"
    REPLACE = "replace"
    REWRITE = "rewrite"


class AssumptionPredicate(StrEnum):
    REAL = "real"
    POSITIVE = "positive"
    NEGATIVE = "negative"
    INTEGER = "integer"
    NONNEGATIVE = "nonnegative"


class SolveRoute(StrEnum):
    SOLVE = "solve"
    SOLVESET = "solveset"
    LINSOLVE = "linsolve"
    NONLINSOLVE = "nonlinsolve"
    NSOLVE = "nsolve"
    DSOLVE = "dsolve"
    PDSOLVE = "pdsolve"
    ROOTS = "roots"
    REAL_ROOTS = "real_roots"
    NROOTS = "nroots"
    FACTOR_LIST = "factor_list"
    RESULTANT = "resultant"


class SolveDomain(StrEnum):
    COMPLEXES = "Complexes"
    REALS = "Reals"
    INTEGERS = "Integers"
    NATURALS = "Naturals"


class MatrixRoute(StrEnum):
    EIGENVALS = "eigenvals"
    EIGENVECTS = "eigenvects"
    DETERMINANT = "det"
    CHARPOLY = "charpoly"
    MINPOLY = "minpoly"
    RREF = "rref"
    NULLSPACE = "nullspace"
    RANK = "rank"
    INVERSE = "inv"
    PINV = "pinv"
    LU = "LUdecomposition"
    QR = "QRdecomposition"
    CHOLESKY = "cholesky"
    DIAGONALIZE = "diagonalize"
    JORDAN = "jordan_form"
    SINGULAR = "singular_values"


class NumberRoute(StrEnum):
    FACTORINT = "factorint"
    PRIMERANGE = "primerange"
    ISPRIME = "isprime"
    GCD = "gcd"
    LCM = "lcm"


# the exact-arithmetic axis: SYMPY runs the pure-Python `Poly`/`Matrix` kernel, FLINT lowers the carried polynomial/matrix to an
# `fmpq_poly`/`fmpq_mat` C-level exact kernel — one row on the solve/linalg/number case, never a parallel FLINT surface.
class GroundDomain(StrEnum):
    SYMPY = "sympy"
    FLINT = "flint"


# HEURISTIC lifts `N(expr, digits)` through the bundled `mpmath` context with no error bound; CERTIFIED re-evaluates through a
# `python-flint` `arb`/`acb` ball under `flint.good`, carrying the certified `rad()` bound the heuristic path lacks.
class Precision(StrEnum):
    HEURISTIC = "heuristic"
    CERTIFIED = "certified"


class LowerBackend(StrEnum):
    NUMPY = "numpy"  # core lambdify
    JAX = "jax"  # jaxlib worker floor
    UFUNC = "ufunc"  # ufuncify broadcasting ufunc
    NATIVE = "native"  # autowrap compiled extension, host-toolchain gated


# the recommended jit route per backend: numpy/ufunc/native lowerings are already vectorized or compiled (Passthrough), jax
# recommends the XLA route — consumers compile through `JitBackend.compile`, never by importing this page.
_LOWER_ROUTE: Final[Map[LowerBackend, JitBackend]] = Map.of_seq([
    (LowerBackend.NUMPY, JitBackend.Passthrough()),
    (LowerBackend.JAX, JitBackend.JaxJit()),
    (LowerBackend.UFUNC, JitBackend.Passthrough()),
    (LowerBackend.NATIVE, JitBackend.Passthrough()),
])


class CodeTarget(StrEnum):
    C = "c"
    CXX = "cxx"
    FORTRAN = "fortran"
    RUST = "rust"
    JULIA = "julia"
    OCTAVE = "octave"


# `MatrixForm` rows are cell spellings sympified against the live `SymbolSpec`.
type MatrixForm = tuple[tuple[str, ...], ...]
type ExprForm = str | MatrixForm | "Expr"

# --- [MODELS] ---------------------------------------------------------------------------


class SymbolSpec(Struct, frozen=True):
    """Assumption-carrying free-variable vocabulary; assumptions are derivation inputs."""

    names: tuple[str, ...]
    assume: Map[str, AssumptionPredicate] = Map.empty()

    def symbols(self, sym: object) -> tuple["Expr", ...]:
        # Each free variable carries its own declared assumption as a `Symbol` kwarg, so
        # `Refine`/`solveset` reason under the same context rather than a post-hoc filter.
        return tuple(
            self.assume.try_find(name).map(lambda p: sym.Symbol(name, **{p.value: True})).default_with(lambda: sym.Symbol(name))
            for name in self.names
        )


class SymbolicPayload(Struct, frozen=True, gc=False):
    # the form repr, sorted assumption pairs, and ordered op signature feed the one cached deterministic encoder; `gc=False` drops
    # this container-free leaf from the tracked GC set on the high-allocation derivation path.
    form: str
    assume: tuple[tuple[str, str], ...]
    ops: tuple[str, ...]

    @staticmethod
    def of(form: ExprForm, spec: SymbolSpec, ops: tuple["SymbolicOp", ...]) -> "SymbolicPayload":
        # a constructed `Expr` MUST render through `srepr` (`Pow(Symbol('x'), Integer(2))`), not `repr`/`str` (`x**2`), so
        # assumption-carrying vs assumption-free symbols and exact vs auto-simplified nodes key distinctly.
        return SymbolicPayload(
            form=_form_spelling(form),
            assume=tuple(sorted((name, pred.value) for name, pred in spec.assume.items())),
            ops=tuple(f"{op.tag}:{op.signature()}" for op in ops),
        )


@tagged_union(frozen=True)
class Outcome:
    tag: Literal["staged", "solution", "spectrum", "arithmetic", "numeric", "callable_", "source"] = tag()
    staged: "Expr" = case()
    solution: tuple[SolveRoute, int, float] = case()
    spectrum: tuple[MatrixRoute, int, float] = case()
    arithmetic: tuple[NumberRoute, int, float] = case()
    numeric: tuple[int, float, float] = case()
    callable_: tuple[LowerBackend, int, LoweredSpec] = case()  # the jit-minted spec the consumers compile — the lowered callable is never discarded
    source: tuple[CodeTarget, str, str] = case()

    @staticmethod
    def Staged(expr: "Expr") -> "Outcome":
        return Outcome(staged=expr)

    @staticmethod
    def Solution(route: SolveRoute, cardinality: int, metric: float = 0.0) -> "Outcome":
        return Outcome(solution=(route, cardinality, metric))

    @staticmethod
    def Spectrum(route: MatrixRoute, dimension: int, invariant: float = 0.0) -> "Outcome":
        return Outcome(spectrum=(route, dimension, invariant))

    @staticmethod
    def Arithmetic(route: NumberRoute, cardinality: int, magnitude: float) -> "Outcome":
        return Outcome(arithmetic=(route, cardinality, magnitude))

    @staticmethod
    def Numeric(digits: int, magnitude: float, radius: float = 0.0) -> "Outcome":
        return Outcome(numeric=(digits, magnitude, radius))

    @staticmethod
    def Callable(backend: LowerBackend, arity: int, spec: LoweredSpec) -> "Outcome":
        return Outcome(callable_=(backend, arity, spec))

    @staticmethod
    def Source(target: CodeTarget, name: str, source: str) -> "Outcome":
        return Outcome(source=(target, name, source))

    def facts(self) -> dict[str, object]:
        match self:
            case Outcome(tag="solution", solution=(route, cardinality, metric)):
                return {"route": route.value, "cardinality": cardinality, "metric": f"{metric:.6g}"}
            case Outcome(tag="spectrum", spectrum=(route, dimension, invariant)):
                return {"route": route.value, "dimension": dimension, "invariant": f"{invariant:.6g}"}
            case Outcome(tag="arithmetic", arithmetic=(route, cardinality, magnitude)):
                return {"route": route.value, "cardinality": cardinality, "magnitude": f"{magnitude:.6g}"}
            case Outcome(tag="numeric", numeric=(digits, magnitude, radius)):
                return {"digits": digits, "magnitude": f"{magnitude:.6g}", "radius": f"{radius:.3e}"}
            case Outcome(tag="callable_", callable_=(backend, arity, _spec)):
                return {"backend": backend.value, "arity": arity}
            case Outcome(tag="source", source=(target, name, source)):
                return {"target": target.value, "name": name, "byte_count": len(source)}
            case Outcome(tag="staged"):
                # `derive` faults a staging terminal before any receipt mints, so this arm only
                # keeps the match total — the rail owns the real rejection one layer up.
                return {}
            case _ as unreachable:
                assert_never(unreachable)


class SymbolicReceipt(Struct, frozen=True):
    op: str
    symbols: tuple[str, ...]
    content_key: ContentKey
    outcome: Outcome

    @staticmethod
    def of(op: str, symbols: tuple[str, ...], key: ContentKey, outcome: Outcome) -> "SymbolicReceipt":
        return SymbolicReceipt(op, symbols, key, outcome)

    def contribute(self) -> Iterable[Receipt]:
        facts = {"op": self.op, "symbols": ",".join(self.symbols), "content_key": self.content_key.project("hex"), **self.outcome.facts()}
        yield Receipt.of("compute.symbolic", ("emitted", self.op, facts))


@tagged_union(frozen=True)
class SymbolicOp:
    tag: Literal["calculus", "rewrite", "substitute", "refine", "solve", "linalg", "number", "evaluate", "lower", "codegen"] = tag()
    calculus: tuple[CalculusKind, int] = case()
    rewrite: RewritePass = case()
    substitute: tuple[SubstituteMode, Map[str, str]] = case()
    refine: AssumptionPredicate = case()
    solve: tuple[SolveRoute, SolveDomain, GroundDomain] = case()
    linalg: tuple[MatrixRoute, GroundDomain] = case()
    number: tuple[NumberRoute, GroundDomain] = case()
    evaluate: tuple[int, Precision] = case()
    lower: LowerBackend = case()
    codegen: tuple[CodeTarget, str] = case()

    @staticmethod
    def Calculus(kind: CalculusKind, order: int = 1) -> "SymbolicOp":
        return SymbolicOp(calculus=(kind, order))

    @staticmethod
    def Rewrite(pass_: RewritePass = RewritePass.SIMPLIFY) -> "SymbolicOp":
        return SymbolicOp(rewrite=pass_)

    @staticmethod
    def Substitute(mode: SubstituteMode, mapping: Map[str, str]) -> "SymbolicOp":
        return SymbolicOp(substitute=(mode, mapping))

    @staticmethod
    def Refine(predicate: AssumptionPredicate = AssumptionPredicate.REAL) -> "SymbolicOp":
        return SymbolicOp(refine=predicate)

    @staticmethod
    def Solve(
        route: SolveRoute = SolveRoute.SOLVE, domain: SolveDomain = SolveDomain.COMPLEXES, ground: GroundDomain = GroundDomain.SYMPY
    ) -> "SymbolicOp":
        return SymbolicOp(solve=(route, domain, ground))

    @staticmethod
    def LinAlg(route: MatrixRoute = MatrixRoute.EIGENVALS, ground: GroundDomain = GroundDomain.SYMPY) -> "SymbolicOp":
        return SymbolicOp(linalg=(route, ground))

    @staticmethod
    def Number(route: NumberRoute = NumberRoute.FACTORINT, ground: GroundDomain = GroundDomain.SYMPY) -> "SymbolicOp":
        return SymbolicOp(number=(route, ground))

    @staticmethod
    def Evaluate(digits: int = 15, precision: Precision = Precision.HEURISTIC) -> "SymbolicOp":
        return SymbolicOp(evaluate=(digits, precision))

    @staticmethod
    def Lower(backend: LowerBackend = LowerBackend.NUMPY) -> "SymbolicOp":
        return SymbolicOp(lower=backend)

    @staticmethod
    def Codegen(target: CodeTarget = CodeTarget.C, name: str = "kernel") -> "SymbolicOp":
        return SymbolicOp(codegen=(target, name))

    def signature(self) -> str:
        # the case-data spelling the canonical key folds; the substitution map renders its sorted items rather than its identity
        # so identical maps key identically.
        match self:
            case SymbolicOp(tag="calculus", calculus=(kind, order)):
                return f"{kind.value}/{order}"
            case SymbolicOp(tag="substitute", substitute=(mode, mapping)):
                return f"{mode.value}/{sorted(mapping.items())}"
            case SymbolicOp(tag="solve", solve=(route, domain, ground)):
                return f"{route.value}/{domain.value}/{ground.value}"
            case SymbolicOp(tag="linalg", linalg=(route, ground)) | SymbolicOp(tag="number", number=(route, ground)):
                return f"{route.value}/{ground.value}"
            case SymbolicOp(tag="evaluate", evaluate=(digits, precision)):
                return f"{digits}/{precision.value}"
            case SymbolicOp(tag="codegen", codegen=(target, name)):
                return f"{target.value}/{name}"
            case SymbolicOp(tag="rewrite", rewrite=value) | SymbolicOp(tag="refine", refine=value) | SymbolicOp(tag="lower", lower=value):
                # the or-pattern binds the lone `StrEnum` payload, never a `getattr` reflection escaping the exhaustive match.
                return value.value
            case _ as unreachable:
                assert_never(unreachable)

    def apply(self, sym: object, expr: "Expr", free: tuple["Expr", ...]) -> Outcome:
        # the free-symbol read is lazy per arm: a symbol-free `NumberTheory`/numeric-`Evaluate` derivation declares no symbols, so
        # an eager `free[0]` here IndexErrors before its arm runs; `_primary` faults a symbol-needing op on an empty `SymbolSpec`.
        match self:
            case SymbolicOp(tag="calculus", calculus=(kind, order)):
                return Outcome.Staged(_CALCULUS[kind](sym, expr, _primary(free, self.tag), order))
            case SymbolicOp(tag="rewrite", rewrite=pass_):
                args = (expr, _primary(free, self.tag)) if pass_ is RewritePass.COLLECT else (expr,)
                return Outcome.Staged(getattr(sym, pass_.value)(*args))
            case SymbolicOp(tag="substitute", substitute=(SubstituteMode.SUBS, mapping)):
                local = {s.name: s for s in free}
                return Outcome.Staged(expr.subs({sym.sympify(k, locals=local): sym.sympify(v, locals=local) for k, v in mapping.items()}))
            case SymbolicOp(tag="substitute", substitute=(SubstituteMode.REPLACE, mapping)):
                [(pattern, target)] = mapping.items()
                return Outcome.Staged(expr.replace(sym.Wild(pattern), sym.sympify(target, locals={s.name: s for s in free})))
            case SymbolicOp(tag="substitute", substitute=(SubstituteMode.REWRITE, mapping)):
                [(_, basis)] = mapping.items()
                return Outcome.Staged(expr.rewrite(getattr(sym, basis)))
            case SymbolicOp(tag="refine", refine=predicate):
                return Outcome.Staged(sym.refine(expr, getattr(sym.Q, predicate.value)(_primary(free, self.tag))))
            case SymbolicOp(tag="solve", solve=(route, domain, ground)):
                return _solve(sym, expr, free, route, domain, ground)
            case SymbolicOp(tag="linalg", linalg=(route, ground)):
                return _linalg(sym, expr, route, ground)
            case SymbolicOp(tag="number", number=(route, ground)):
                return _number(sym, expr, route, ground)
            case SymbolicOp(tag="evaluate", evaluate=(digits, precision)):
                return _evaluate(sym, expr, free, digits, precision)
            case SymbolicOp(tag="lower", lower=backend):
                # the materialized callable rides the jit-minted `LoweredSpec` VALUE consumers compile through `JitBackend.compile`
                # — the callable is never discarded.
                fn = _lower(sym, expr, free, backend)
                spec = LoweredSpec(
                    kernel=fn,
                    name=f"symbolic.{backend.value}",
                    arity=len(free),
                    signature=", ".join(str(s) for s in free),
                    route=_LOWER_ROUTE[backend],
                )
                return Outcome.Callable(backend, len(free), spec)
            case SymbolicOp(tag="codegen", codegen=(target, name)):
                return Outcome.Source(target, name, _emit(sym, expr, target, name))
            case _ as unreachable:
                assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------

# Calculus node construction keyed by kind: each forces its unevaluated node and trims the
# series Order term in one row, collapsing five near-identical match arms to a data table.
_CALCULUS: Final[Map[CalculusKind, Callable[[object, "Expr", "Expr", int], "Expr"]]] = Map.of_seq([
    (CalculusKind.DIFF, lambda s, e, x, n: s.diff(e, x, n).doit()),
    (CalculusKind.INTEGRATE, lambda s, e, x, _: s.integrate(e, x).doit()),
    (CalculusKind.LIMIT, lambda s, e, x, _: s.limit(e, x, 0)),
    (CalculusKind.SERIES, lambda s, e, x, n: s.series(e, x, 0, n).removeO()),
    (CalculusKind.SUMMATION, lambda s, e, x, n: s.summation(e, (x, 0, n)).doit()),
])

# the `MatrixRoute` subset `fmpq_mat` owns an exact-over-ℚ kernel for; the routes outside it have no exact rational analogue and
# stay on the symbolic kernel.
_FLINT_MATRIX_ROUTES: Final[frozenset[MatrixRoute]] = frozenset({
    MatrixRoute.DETERMINANT,
    MatrixRoute.RANK,
    MatrixRoute.CHARPOLY,
    MatrixRoute.MINPOLY,
    MatrixRoute.INVERSE,
    MatrixRoute.RREF,
    MatrixRoute.NULLSPACE,
})

# source-printer dispatch keyed by target: one polymorphic codegen surface, never a parallel emitter per language.
_CODE_PRINTER: Final[Map[CodeTarget, Callable[[object, "Expr"], str]]] = Map.of_seq([
    (CodeTarget.C, lambda s, e: s.ccode(e, standard="c99")),
    (CodeTarget.CXX, lambda s, e: s.cxxcode(e, standard="c++17")),
    (CodeTarget.FORTRAN, lambda s, e: s.fcode(e, standard=95)),
    (CodeTarget.RUST, lambda s, e: s.rust_code(e)),
    (CodeTarget.JULIA, lambda s, e: s.julia_code(e)),
    (CodeTarget.OCTAVE, lambda s, e: s.octave_code(e)),
])

# --- [OPERATIONS] -----------------------------------------------------------------------


def _solve(sym: object, expr: "Expr", free: tuple["Expr", ...], route: SolveRoute, domain: SolveDomain, ground: GroundDomain) -> Outcome:
    # `linsolve`/`nonlinsolve` take a one-equation SYSTEM `(expr,)`, so the carried scalar lifts to a singleton system rather than
    # a bare expression the set solvers reject. `_primary` resolves lazily per arm — `dsolve`/`pdsolve` carry their `Function`
    # unknown in `expr` and read no plain `Symbol`, so an eager read here faults a valid symbol-free solve.
    match route:
        case SolveRoute.SOLVE:
            return Outcome.Solution(route, _cardinality(sym.solve(expr, *free)))
        case SolveRoute.SOLVESET:
            return Outcome.Solution(route, _cardinality(sym.solveset(expr, _primary(free, route.value), domain=getattr(sym, domain.value))))
        case SolveRoute.LINSOLVE | SolveRoute.NONLINSOLVE:
            return Outcome.Solution(route, _cardinality(getattr(sym, route.value)((expr,), *free)))
        case SolveRoute.NSOLVE:
            # the `metric` is the substituted residual `|f(root)|` — the convergence witness the closed-form routes lack — keyed by
            # the assumption-carrying `primary` Symbol object, never its name.
            primary = _primary(free, route.value)
            root = sym.nsolve(expr, primary, 0.0)
            return Outcome.Solution(route, 1, abs(float(sym.N(expr.subs(primary, root)))))
        case SolveRoute.DSOLVE | SolveRoute.PDSOLVE:
            return Outcome.Solution(route, _cardinality(getattr(sym, route.value)(expr)))
        case SolveRoute.ROOTS:
            return Outcome.Solution(route, len(sym.roots(sym.Poly(expr, _primary(free, route.value)))))
        case _:
            return _poly_route(sym, expr, _primary(free, route.value), route, ground)


def _poly_route(sym: object, expr: "Expr", primary: "Expr", route: SolveRoute, ground: GroundDomain) -> Outcome:
    poly = sym.Poly(expr, primary)
    if ground is GroundDomain.FLINT:
        return _flint_poly(sym, poly, route)
    match route:
        case SolveRoute.REAL_ROOTS:
            return Outcome.Solution(route, len(poly.real_roots()), abs(float(poly.discriminant())))
        case SolveRoute.NROOTS:
            return Outcome.Solution(route, len(poly.nroots()), abs(float(poly.discriminant())))
        case SolveRoute.FACTOR_LIST:
            _, factors = poly.factor_list()
            return Outcome.Solution(route, len(factors), abs(float(poly.discriminant())))
        case _:  # RESULTANT against the formal derivative — the squarefree discriminant kernel
            return Outcome.Solution(route, poly.degree(), abs(float(sym.resultant(poly.as_expr(), poly.diff(primary).as_expr(), primary))))


def _flint_poly(sym: object, poly: object, route: SolveRoute) -> Outcome:
    from flint import fmpq_poly

    # `fmpq_poly` (not `fmpz_poly`) so a rational-coefficient poly lowers exactly through `_as_fmpq` rather than an `int(c)` coerce
    # that TypeErrors on a non-integer. The `metric` is `|res(p, p')|` — the discriminant up to lead-coefficient/sign normalization,
    # the same squarefree fact the SYMPY ground reads off `Poly.discriminant`; a symbolic-coefficient poly faults at `_as_fmpq`.
    fp = fmpq_poly([_as_fmpq(sym, c) for c in reversed(poly.all_coeffs())])
    disc = abs(float(fp.resultant(fp.derivative())))
    match route:
        case SolveRoute.REAL_ROOTS:
            roots = fp.complex_roots()
            return Outcome.Solution(route, sum(1 for r, _ in roots if abs(complex(r).imag) < 1e-12), disc)
        case SolveRoute.NROOTS:
            return Outcome.Solution(route, len(fp.complex_roots()), disc)
        case SolveRoute.FACTOR_LIST:
            _, factors = fp.factor()
            return Outcome.Solution(route, len(factors), disc)
        case _:  # RESULTANT against the formal derivative through the FLINT exact kernel
            return Outcome.Solution(route, fp.degree(), disc)


def _linalg(sym: object, expr: "Expr", route: MatrixRoute, ground: GroundDomain) -> Outcome:
    # a route outside `_FLINT_MATRIX_ROUTES` stays on the sympy kernel regardless of the requested ground — `fmpq_mat` exposes no
    # such method, so honoring FLINT there calls a phantom `getattr`.
    matrix = expr if hasattr(expr, "rref") else sym.Matrix(expr)
    if ground is GroundDomain.FLINT and route in _FLINT_MATRIX_ROUTES:
        return _flint_matrix(sym, matrix, route)
    dimension = matrix.shape[0]
    match route:
        case MatrixRoute.DETERMINANT:
            return Outcome.Spectrum(route, dimension, abs(float(matrix.det())))
        case MatrixRoute.RANK:
            return Outcome.Spectrum(route, matrix.rank())
        case MatrixRoute.EIGENVALS | MatrixRoute.EIGENVECTS:
            spectral = matrix.eigenvals() if route is MatrixRoute.EIGENVALS else {v: m for v, m, _ in matrix.eigenvects()}
            radius = max((abs(complex(sym.N(v))) for v in spectral), default=0.0)
            return Outcome.Spectrum(route, len(spectral), radius)
        case MatrixRoute.SINGULAR:
            # the spectral 2-norm reads order-independently through `max`, never `values[0]` resting on one method's ordering contract.
            values = matrix.singular_values()
            return Outcome.Spectrum(route, len(values), max((abs(float(sym.N(v))) for v in values), default=0.0))
        case MatrixRoute.CHARPOLY:
            # degree alone restates the dimension; the charpoly's own `discriminant()` is the invariant — zero exactly on a
            # repeated-eigenvalue spectrum.
            poly = matrix.charpoly()
            return Outcome.Spectrum(route, poly.degree(), abs(float(sym.N(poly.discriminant()))))
        case MatrixRoute.MINPOLY:
            # `charpoly` is not a substitute — the minimal polynomial divides it; `fmpq_mat.minpoly` is the one admitted owner.
            raise ValueError("minimal polynomial requires GroundDomain.FLINT; sympy Matrix owns no exact minpoly kernel")
        case MatrixRoute.NULLSPACE:
            return Outcome.Spectrum(route, len(matrix.nullspace()))
        case MatrixRoute.INVERSE | MatrixRoute.PINV:
            # INVERSE carries the determinant magnitude; PINV has no determinant and reads the Frobenius `norm()` of the
            # pseudo-inverse rather than a dead `0.0`.
            inverse = matrix.inv() if route is MatrixRoute.INVERSE else matrix.pinv()
            magnitude = abs(float(matrix.det())) if route is MatrixRoute.INVERSE else abs(float(sym.N(inverse.norm())))
            return Outcome.Spectrum(route, inverse.shape[0], magnitude)
        case _:  # LU/QR/cholesky/diagonalize/jordan_form — the decomposition leading dimension
            extracted = getattr(matrix, route.value)()
            head = extracted[0] if isinstance(extracted, tuple) else extracted
            return Outcome.Spectrum(route, head.shape[0])


def _flint_matrix(sym: object, matrix: object, route: MatrixRoute) -> Outcome:
    from flint import fmpq_mat

    fm = fmpq_mat([[_as_fmpq(sym, matrix[i, j]) for j in range(matrix.shape[1])] for i in range(matrix.shape[0])])
    dimension = fm.nrows()
    match route:
        case MatrixRoute.DETERMINANT:
            return Outcome.Spectrum(route, dimension, abs(float(fm.det())))
        case MatrixRoute.RANK:
            return Outcome.Spectrum(route, fm.rank())
        case MatrixRoute.CHARPOLY | MatrixRoute.MINPOLY:
            # the monic polynomial's discriminant magnitude — `resultant` against the formal derivative — is the invariant, zero
            # exactly on a repeated-root spectrum.
            poly = fm.charpoly() if route is MatrixRoute.CHARPOLY else fm.minpoly()
            return Outcome.Spectrum(route, poly.degree(), abs(float(poly.resultant(poly.derivative()))))
        case MatrixRoute.INVERSE:
            return Outcome.Spectrum(route, fm.inv().nrows(), abs(float(fm.det())))
        case _:
            # RREF/NULLSPACE — `rref` returns `(matrix, pivots)` and `nullspace` `(matrix, rank)`; the head is the matrix whose `nrows()` leads.
            extracted = getattr(fm, route.value)()
            head = extracted[0] if isinstance(extracted, tuple) else extracted
            return Outcome.Spectrum(route, head.nrows())


def _number(sym: object, expr: "Expr", route: NumberRoute, ground: GroundDomain) -> Outcome:
    # `PRIMERANGE` (a range generator, not an `fmpz` query) and a still-symbolic operand stay on the sympy ground by construction;
    # only the single-integer routes lower to the FLINT `fmpz` kernel.
    if ground is GroundDomain.FLINT and route is not NumberRoute.PRIMERANGE and expr.is_integer:
        return _flint_number(int(expr), route)
    match route:
        case NumberRoute.FACTORINT:
            factors = sym.factorint(expr)
            return Outcome.Arithmetic(route, len(factors), float(max(factors, default=0)))
        case NumberRoute.PRIMERANGE:
            primes = tuple(sym.primerange(2, int(expr) + 1))
            return Outcome.Arithmetic(route, len(primes), float(primes[-1]) if primes else 0.0)
        case NumberRoute.ISPRIME:
            return Outcome.Arithmetic(route, int(bool(sym.isprime(expr))), float(expr))
        case _:
            # GCD/LCM against the formal derivative — the squarefree-part kernel; `cardinality` is the divisor's degree, `magnitude`
            # its constant value or lead coefficient. A leaf integer carries no free symbol and falls to its `factorint` divisor
            # structure — the FLINT ground owns the totient read.
            free = tuple(expr.free_symbols)
            if not free:
                factors = sym.factorint(expr)
                return Outcome.Arithmetic(route, len(factors), float(max(factors, default=0)))
            value = getattr(sym, route.value)(expr, sym.diff(expr, free[0]))
            if value.is_number:
                return Outcome.Arithmetic(route, 1, abs(float(sym.N(value))))
            poly = sym.Poly(value, *free)
            return Outcome.Arithmetic(route, poly.degree(), abs(float(sym.N(poly.LC()))))


def _flint_number(n: int, route: NumberRoute) -> Outcome:
    from flint import fmpz

    # GCD reads the divisor-lattice bottom through `divisor_sigma(0)` count and `divisor_sigma(1)` sum; LCM reads the lattice top
    # through the `euler_phi` totient — the genuine unary structure of a leaf integer, never the vacuous `z.gcd(z) == n`.
    z = fmpz(n)
    match route:
        case NumberRoute.FACTORINT:
            factors = z.factor()
            return Outcome.Arithmetic(route, len(factors), float(max((int(p) for p, _ in factors), default=0)))
        case NumberRoute.ISPRIME:
            return Outcome.Arithmetic(route, int(bool(z.is_prime())), float(n))
        case NumberRoute.GCD:
            return Outcome.Arithmetic(route, int(z.divisor_sigma(0)), abs(float(z.divisor_sigma(1))))
        case _:  # LCM — the divisor-lattice join read as the Euler totient over the leaf integer
            return Outcome.Arithmetic(route, int(z.euler_phi()), float(n))


def _evaluate(sym: object, expr: "Expr", free: tuple["Expr", ...], digits: int, precision: Precision) -> Outcome:
    # a still-symbolic polynomial falls to its lead-coefficient magnitude, resolving the primary unknown only on that branch so a
    # closed numeric form evaluates without a declared symbol.
    scalar = expr if expr.is_number else sym.Poly(expr, _primary(free, "evaluate")).all_coeffs()[0]
    if precision is Precision.HEURISTIC:
        return Outcome.Numeric(digits, abs(float(sym.N(scalar, digits))))
    return _certified(scalar, digits)


def _certified(scalar: "Expr", digits: int) -> Outcome:
    import flint
    from flint import arb

    # `flint.good` re-runs the thunk at escalating precision until the ball pins `ctx.dps` digits; the thunk re-renders through
    # sympy's `mpmath`-backed `evalf` so `sqrt(2)` lowers to a decimal `arb` parses, never the unparseable `str(sqrt(2))` spelling.
    # `workdps` restores the prior session `ctx.dps` on exit, never the leaking `ctx.dps = digits` global mutation.
    with flint.ctx.workdps(digits):
        ball = flint.good(lambda: arb(str(scalar.evalf(flint.ctx.dps + 2))))
    return Outcome.Numeric(digits, abs(float(ball.mid())), float(ball.rad()))


def _lower(sym: object, expr: "Expr", free: tuple["Expr", ...], backend: LowerBackend) -> object:
    match backend:
        case LowerBackend.UFUNC:
            from sympy.utilities.autowrap import ufuncify

            return ufuncify(free, expr)
        case LowerBackend.NATIVE:
            from sympy.utilities.autowrap import autowrap

            return autowrap(expr, args=free)
        case _:
            return sym.lambdify(free, expr, modules=backend.value, cse=True)


def _emit(sym: object, expr: "Expr", target: CodeTarget, name: str) -> str:
    # `codegen` wraps a named, signature-bearing module where the language supports it; CXX renders bare via `cxxcode`.
    if target is CodeTarget.CXX:
        return _CODE_PRINTER[target](sym, expr)
    from sympy.utilities.codegen import codegen

    language = "c99" if target is CodeTarget.C else target.value
    [(_, source), *_] = codegen((name, expr), language=language, header=False, empty=False)
    return source


def _primary(free: tuple["Expr", ...], tag: str) -> "Expr":
    # an empty `SymbolSpec` faults as a `BoundaryFault` naming the offending op, never a bare `IndexError` from `free[0]`.
    if not free:
        raise ValueError(f"symbolic op {tag} needs a declared free symbol; SymbolSpec.names is empty")
    return free[0]


def _cardinality(solution: object) -> int:
    return len(tuple(solution)) if hasattr(solution, "__len__") else 1


def _as_fmpq(sym: object, cell: object) -> object:
    from flint import fmpq

    # a `Float` or other numeric node coerces through `sym.Rational` first, so the FLINT exact ground carries no lossy float and no
    # `int(cell)` that TypeErrors on a non-integer; a symbolic cell has no rational form and faults in the fence.
    rational = cell if hasattr(cell, "q") else sym.Rational(cell)
    return fmpq(int(rational.p), int(rational.q))


# --- [COMPOSITION] ----------------------------------------------------------------------


class SymbolicDerivation:
    @staticmethod
    def derive(form: ExprForm, spec: SymbolSpec, *ops: SymbolicOp) -> "RuntimeRail[SymbolicReceipt]":
        # admit-then-bind: the canonical key resolves before the `boundary` fence, so a canonical-encode fault rails first.
        return ContentIdentity.of("symbolic", SymbolicPayload.of(form, spec, ops)).bind(
            lambda key: boundary(f"symbolic.{ops[-1].tag if ops else 'noop'}", lambda: _derive(form, spec, ops, key))
        )


# the symbolic family's DEFAULT graduation ceiling — the stability law as data, caller-overridable.
_CEILING: Final[Map[str, float]] = Map.of_seq([("radius", 1e-12), ("unstable", 0.0)])


def graduates(receipt: SymbolicReceipt, *, certified: bool, radius: float = 0.0) -> "RuntimeRail[GraduationReceipt]":
    # either the ledger clears the `_CEILING` family row or the hub rejects the crossing.
    ledger = {"radius": radius, "unstable": 0.0 if certified else 1.0}
    return GraduationReceipt.graduates("compute.symbolic", HandoffAxis(symbolic=receipt.op), receipt.content_key, ledger, dict(_CEILING.items()))


def _derive(form: ExprForm, spec: SymbolSpec, ops: tuple[SymbolicOp, ...], key: ContentKey) -> SymbolicReceipt:
    # the empty-pipeline gate lives inside the fence where `boundary` converts it — an `ops[-1]` read outside the thunk escapes
    # the rail as a bare `IndexError`.
    import sympy

    if not ops:
        raise ValueError("symbolic derivation needs at least one terminal op")
    free = spec.symbols(sympy)
    staged = _sympify_form(sympy, form, free)
    *stages, terminal = ops
    folded = Block.of_seq(stages).fold(lambda acc, op: _stage(op, sympy, acc, free), staged)
    outcome = terminal.apply(sympy, folded, free)
    if outcome.tag == "staged":
        raise ValueError(f"symbolic pipeline terminal {terminal.tag} yields a staging op, not an artifact")
    return SymbolicReceipt.of(terminal.tag, spec.names, key, outcome)


def _form_spelling(form: ExprForm) -> str:
    # the canonical-key spelling, the inverse of `_sympify_form`; the `srepr` requirement is `SymbolicPayload.of`'s law.
    match form:
        case str() as source:
            return source
        case tuple() as rows:
            return repr(rows)
        case _:
            from sympy import srepr

            return srepr(form)


def _sympify_form(sym: object, form: ExprForm, free: tuple["Expr", ...]) -> "Expr":
    # `local` binds every name to its assumption-carrying `Symbol` so the parsed expression shares the exact objects `free` holds;
    # a plain `sympify` mints distinct assumption-free symbols `diff`/`solveset` over `free[0]` never recognize.
    local = {s.name: s for s in free}
    match form:
        case str() as source:
            return sym.sympify(source, locals=local)
        case tuple() as rows if rows and isinstance(rows[0], tuple):
            return sym.Matrix([[sym.sympify(cell, locals=local) for cell in row] for row in rows])
        case _:
            return form


def _stage(op: SymbolicOp, sym: object, expr: "Expr", free: tuple["Expr", ...]) -> "Expr":
    match op.apply(sym, expr, free):
        case Outcome(tag="staged", staged=rewritten):
            return rewritten
        case _:
            raise ValueError(f"non-terminal op {op.tag} must be a calculus/rewrite/substitute/refine staging op")
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
