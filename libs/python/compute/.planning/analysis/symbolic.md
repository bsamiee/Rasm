# [PY_COMPUTE_SYMBOLIC]

`SymbolicDerivation` owns classical computer algebra, left-folding a `Block[SymbolicOp]` over an `ExprForm`: every staging op rewrites the carried expression and the terminal op returns its exact provider product with the derivation key. `ExprForm` discriminates a `str` spelling, a `MatrixForm`, or a constructed `Expr` through one `derive` entry.

This is the core solver route with a gating law per backend: `sympy` is pure-Python and imports on the runtime, so calculus, rewrite, solve, matrix algebra, assumption logic, number theory, heuristic numeric evaluation, and the source-printer family run as live core; `python-flint`'s exact kernels and the certified-ball `Evaluate` precision row gate on the worker lane; `Lower(jax)` reads `jax` at usage on the jaxlib floor; `Lower(native)`'s `autowrap`/`ufuncify` rows gate on a host C/Fortran toolchain. Derivations key through the runtime `ContentIdentity` over the canonical `SymbolicPayload`, so a repeated derivation at identical `(form, spec, ops)` is a cache hit by reference; a `source` derivation graduates outward on the symbolic `HandoffAxis` case once stable and reproducible.

## [01]-[INDEX]

- [02]-[OP]: the `SymbolicOp` bounded vocabulary — staging expression-to-expression rows composing ahead of one terminal artifact row, with the `GroundDomain` and `Precision` accelerator axes.
- [03]-[DERIVATION]: `SymbolicDerivation.derive` left-folding a `Block[SymbolicOp]` over an `ExprForm` to the terminal provider value and its content key.

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

- `Calculus`: unevaluated `Derivative`/`Integral`/`Sum` nodes force through `.doit()`, and `series` trims its `Order` term in the same row, never a second method.
- `Substitute`: map keys and values are spellings resolved against the live `SymbolSpec`, never raw strings escaping the boundary.
- `Refine`: assumptions are derivation inputs the `SymbolSpec` declares, never a post-hoc filter.
- `LinAlg`: `GroundDomain.FLINT` accelerates only the `_FLINT_MATRIX_ROUTES` exact-over-rationals subset, and `MINPOLY` is FLINT-only — sympy `Matrix` owns no minimal-polynomial kernel, so the sympy ground rails a fenced typed fault.
- `NumberTheory`: GCD/LCM read a polynomial with its derivative; constant inputs refuse because the unary request carries no second operand.
- `Evaluate`: `CERTIFIED` returns a `python-flint` `arb` ball whose `rad()` supplies the stability bound; `HEURISTIC` returns SymPy's native numeric value and clears no certified ceiling.
- `Codegen`: a new target is one `CodeTarget` value and one `_CODE_PRINTER` row, never a parallel emitter.

## [03]-[DERIVATION]

`SymbolicDerivation` threads an assumption-carrying `SymbolSpec` over an `ExprForm` and left-folds a `Block[SymbolicOp]` pipeline to the terminal provider product from one shared `cse` lowering.

- Cases: `ExprForm` is the polymorphic input — a `str` spelling, a `MatrixForm` of cell spellings, or a constructed `Expr` — discriminated by one `derive` entry rather than `derive`/`derive_matrix`/`derive_expr` siblings.
- Entry: `derive(form, spec, *ops)` is the one railed entrypoint riding the hub weave as `evidence_run(EvidenceScope.SYMBOLIC, f"derive.{terminal}", rail, facts=...)`; the span carries terminal, op-count, and symbol discriminants.
- Auto: the runtime content owner mints the derivation key over the canonical `SymbolicPayload`, never a hand-rolled canonical encode; two derivations differing in assumption context, op pipeline, or terminal route key distinctly.
- Output: `derive` retains the exact terminal provider value beside its content key; `graduates` reads certification from the actual `arb` product.
- Growth: a new calculus transform is one `CalculusKind` row and one `_CALCULUS` entry; a new rewrite pass is one `RewritePass` row; a new solve route, matrix extraction, or number-theoretic query is one row on its existing case; a new lowering backend is one `LowerBackend` row and one `_LOWER_ROUTE` row; a new code target is one `CodeTarget` row and one `_CODE_PRINTER` entry.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never, cast

from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, Graduation, HandoffAxis, evidence_run
from rasm.compute.numerics.jit import JitBackend, LoweredSpec
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey

lazy import flint
lazy import sympy
lazy from flint import arb, fmpq, fmpq_mat, fmpq_poly, fmpz
lazy from sympy import srepr
lazy from sympy.matrices.exceptions import MatrixError
lazy from sympy.polys.polyerrors import BasePolynomialError
lazy from sympy.utilities.autowrap import autowrap, ufuncify
lazy from sympy.utilities.codegen import codegen

if TYPE_CHECKING:
    from flint import acb, arb, fmpq, fmpq_mat, fmpq_poly, fmpz
    from sympy import Basic, Expr, MatrixBase, Set

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


class GroundDomain(StrEnum):
    SYMPY = "sympy"
    FLINT = "flint"


class Precision(StrEnum):
    HEURISTIC = "heuristic"
    CERTIFIED = "certified"


class LowerBackend(StrEnum):
    NUMPY = "numpy"
    JAX = "jax"
    UFUNC = "ufunc"
    NATIVE = "native"


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


type MatrixForm = tuple[tuple[str, ...], ...]
type ExprForm = str | MatrixForm | Expr | MatrixBase
type AlgebraValue = (
    Basic
    | MatrixBase
    | Set
    | fmpz
    | fmpq
    | fmpq_mat
    | fmpq_poly
    | arb
    | acb
    | LoweredSpec
    | str
    | bool
    | int
    | list[Basic]
    | list[int]
    | list[tuple[Basic, ...]]
    | list[dict[Basic, Basic]]
    | list[tuple[Basic, int]]
    | list[tuple[Basic, int, list[MatrixBase]]]
    | list[tuple[fmpz, int]]
    | list[tuple[arb | acb, int]]
    | dict[Basic, int]
    | tuple[Basic, list[tuple[Basic, int]]]
    | tuple[fmpq, list[tuple[fmpq_poly, int]]]
    | tuple[MatrixBase, ...]
    | tuple[MatrixBase, MatrixBase]
    | tuple[MatrixBase, MatrixBase, list[tuple[int, int]]]
    | tuple[MatrixBase, tuple[int, ...]]
    | tuple[fmpq_mat, int]
)

# --- [MODELS] ---------------------------------------------------------------------------


class SymbolSpec(Struct, frozen=True):
    """Assumption-carrying free-variable vocabulary; assumptions are derivation inputs."""

    names: tuple[str, ...]
    assume: Map[str, AssumptionPredicate] = Map.empty()

    def symbols(self, sym: object) -> tuple["Expr", ...]:
        return tuple(
            self.assume.try_find(name).map(lambda p: sym.Symbol(name, **{p.value: True})).default_with(lambda: sym.Symbol(name))
            for name in self.names
        )


class SymbolicPayload(Struct, frozen=True, gc=False):
    form: str
    assume: tuple[tuple[str, str], ...]
    ops: tuple[str, ...]

    @staticmethod
    def of(form: ExprForm, spec: SymbolSpec, ops: tuple["SymbolicOp", ...]) -> "SymbolicPayload":
        return SymbolicPayload(
            form=_form_spelling(form),
            assume=tuple(sorted((name, pred.value) for name, pred in spec.assume.items())),
            ops=tuple(f"{op.tag}:{op.signature()}" for op in ops),
        )


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
                return value.value
            case _ as unreachable:
                assert_never(unreachable)

    def apply(
        self, sym: object, expr: "Expr | MatrixBase", free: tuple["Expr", ...]
    ) -> AlgebraValue:
        match self:
            case SymbolicOp(tag="calculus", calculus=(kind, order)):
                return _CALCULUS[kind](sym, expr, _primary(free, self.tag), order)
            case SymbolicOp(tag="rewrite", rewrite=pass_):
                args = (expr, _primary(free, self.tag)) if pass_ is RewritePass.COLLECT else (expr,)
                return getattr(sym, pass_.value)(*args)
            case SymbolicOp(tag="substitute", substitute=(SubstituteMode.SUBS, mapping)):
                local = {s.name: s for s in free}
                return expr.subs({sym.sympify(k, locals=local): sym.sympify(v, locals=local) for k, v in mapping.items()})
            case SymbolicOp(tag="substitute", substitute=(SubstituteMode.REPLACE, mapping)):
                [(pattern, target)] = mapping.items()
                return expr.replace(sym.Wild(pattern), sym.sympify(target, locals={s.name: s for s in free}))
            case SymbolicOp(tag="substitute", substitute=(SubstituteMode.REWRITE, mapping)):
                [(_, basis)] = mapping.items()
                return expr.rewrite(getattr(sym, basis))
            case SymbolicOp(tag="refine", refine=predicate):
                return sym.refine(expr, getattr(sym.Q, predicate.value)(_primary(free, self.tag)))
            case SymbolicOp(tag="solve", solve=(route, domain, ground)):
                return _solve(sym, expr, free, route, domain, ground)
            case SymbolicOp(tag="linalg", linalg=(route, ground)):
                return _linalg(sym, expr, route, ground)
            case SymbolicOp(tag="number", number=(route, ground)):
                return _number(sym, expr, route, ground)
            case SymbolicOp(tag="evaluate", evaluate=(digits, precision)):
                return _evaluate(sym, expr, free, digits, precision)
            case SymbolicOp(tag="lower", lower=backend):
                fn = _lower(sym, expr, free, backend)
                spec = LoweredSpec(
                    kernel=fn,
                    name=f"symbolic.{backend.value}",
                    arity=len(free),
                    signature=", ".join(str(s) for s in free),
                    route=_LOWER_ROUTE[backend],
                )
                return spec
            case SymbolicOp(tag="codegen", codegen=(target, name)):
                return _emit(sym, expr, target, name)
            case _ as unreachable:
                assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------

_CALCULUS: Final[Map[CalculusKind, Callable[[object, "Expr", "Expr", int], "Expr"]]] = Map.of_seq([
    (CalculusKind.DIFF, lambda s, e, x, n: s.diff(e, x, n).doit()),
    (CalculusKind.INTEGRATE, lambda s, e, x, _: s.integrate(e, x).doit()),
    (CalculusKind.LIMIT, lambda s, e, x, _: s.limit(e, x, 0)),
    (CalculusKind.SERIES, lambda s, e, x, n: s.series(e, x, 0, n).removeO()),
    (CalculusKind.SUMMATION, lambda s, e, x, n: s.summation(e, (x, 0, n)).doit()),
])

_FLINT_MATRIX_ROUTES: Final[frozenset[MatrixRoute]] = frozenset({
    MatrixRoute.DETERMINANT,
    MatrixRoute.RANK,
    MatrixRoute.CHARPOLY,
    MatrixRoute.MINPOLY,
    MatrixRoute.INVERSE,
    MatrixRoute.RREF,
    MatrixRoute.NULLSPACE,
})

_CODE_PRINTER: Final[Map[CodeTarget, Callable[[object, "Expr"], str]]] = Map.of_seq([
    (CodeTarget.C, lambda s, e: s.ccode(e, standard="c99")),
    (CodeTarget.CXX, lambda s, e: s.cxxcode(e, standard="c++17")),
    (CodeTarget.FORTRAN, lambda s, e: s.fcode(e, standard=95)),
    (CodeTarget.RUST, lambda s, e: s.rust_code(e)),
    (CodeTarget.JULIA, lambda s, e: s.julia_code(e)),
    (CodeTarget.OCTAVE, lambda s, e: s.octave_code(e)),
])

# --- [OPERATIONS] -----------------------------------------------------------------------


def _solve(sym: object, expr: "Expr", free: tuple["Expr", ...], route: SolveRoute, domain: SolveDomain, ground: GroundDomain) -> AlgebraValue:
    match route:
        case SolveRoute.SOLVE:
            return sym.solve(expr, *free)
        case SolveRoute.SOLVESET:
            return sym.solveset(expr, _primary(free, route.value), domain=getattr(sym, domain.value))
        case SolveRoute.LINSOLVE | SolveRoute.NONLINSOLVE:
            return getattr(sym, route.value)((expr,), *free)
        case SolveRoute.NSOLVE:
            return sym.nsolve(expr, _primary(free, route.value), 0.0)
        case SolveRoute.DSOLVE | SolveRoute.PDSOLVE:
            return getattr(sym, route.value)(expr)
        case SolveRoute.ROOTS:
            return sym.roots(sym.Poly(expr, _primary(free, route.value)))
        case _:
            return _poly_route(sym, expr, _primary(free, route.value), route, ground)


def _poly_route(sym: object, expr: "Expr", primary: "Expr", route: SolveRoute, ground: GroundDomain) -> AlgebraValue:
    poly = sym.Poly(expr, primary)
    if ground is GroundDomain.FLINT:
        return _flint_poly(sym, poly, route)
    match route:
        case SolveRoute.REAL_ROOTS:
            return poly.real_roots()
        case SolveRoute.NROOTS:
            return poly.nroots()
        case SolveRoute.FACTOR_LIST:
            return poly.factor_list()
        case _:
            return sym.resultant(poly.as_expr(), poly.diff(primary).as_expr(), primary)


def _flint_poly(sym: object, poly: object, route: SolveRoute) -> AlgebraValue:
    fp = fmpq_poly([_as_fmpq(sym, c) for c in reversed(poly.all_coeffs())])
    match route:
        case SolveRoute.REAL_ROOTS:
            return fp.real_roots()
        case SolveRoute.NROOTS:
            return fp.complex_roots()
        case SolveRoute.FACTOR_LIST:
            return fp.factor()
        case _:
            return fp.resultant(fp.derivative())


def _linalg(sym: object, expr: "Expr", route: MatrixRoute, ground: GroundDomain) -> AlgebraValue:
    matrix = expr if hasattr(expr, "rref") else sym.Matrix(expr)
    if ground is GroundDomain.FLINT and route in _FLINT_MATRIX_ROUTES:
        return _flint_matrix(sym, matrix, route)
    match route:
        case MatrixRoute.DETERMINANT:
            return matrix.det()
        case MatrixRoute.RANK:
            return matrix.rank()
        case MatrixRoute.EIGENVALS:
            return matrix.eigenvals()
        case MatrixRoute.EIGENVECTS:
            return matrix.eigenvects()
        case MatrixRoute.SINGULAR:
            return matrix.singular_values()
        case MatrixRoute.CHARPOLY:
            return matrix.charpoly()
        case MatrixRoute.MINPOLY:
            raise ValueError("minimal polynomial requires GroundDomain.FLINT; sympy Matrix owns no exact minpoly kernel")
        case MatrixRoute.NULLSPACE:
            return matrix.nullspace()
        case MatrixRoute.INVERSE | MatrixRoute.PINV:
            return matrix.inv() if route is MatrixRoute.INVERSE else matrix.pinv()
        case _:
            return getattr(matrix, route.value)()


def _flint_matrix(sym: object, matrix: object, route: MatrixRoute) -> AlgebraValue:
    fm = fmpq_mat([[_as_fmpq(sym, matrix[i, j]) for j in range(matrix.shape[1])] for i in range(matrix.shape[0])])
    match route:
        case MatrixRoute.DETERMINANT:
            return fm.det()
        case MatrixRoute.RANK:
            return fm.rank()
        case MatrixRoute.CHARPOLY | MatrixRoute.MINPOLY:
            return fm.charpoly() if route is MatrixRoute.CHARPOLY else fm.minpoly()
        case MatrixRoute.INVERSE:
            return fm.inv()
        case _:
            return getattr(fm, route.value)()


def _number(sym: object, expr: "Expr", route: NumberRoute, ground: GroundDomain) -> AlgebraValue:
    if ground is GroundDomain.FLINT and route in {NumberRoute.FACTORINT, NumberRoute.ISPRIME} and expr.is_integer:
        return _flint_number(int(expr), route)
    match route:
        case NumberRoute.FACTORINT:
            return sym.factorint(expr)
        case NumberRoute.PRIMERANGE:
            return list(sym.primerange(2, int(expr) + 1))
        case NumberRoute.ISPRIME:
            return bool(sym.isprime(expr))
        case _:
            free = tuple(expr.free_symbols)
            if not free:
                raise ValueError(f"{route.value} requires a polynomial free symbol")
            return getattr(sym, route.value)(expr, sym.diff(expr, free[0]))


def _flint_number(n: int, route: NumberRoute) -> AlgebraValue:
    z = fmpz(n)
    match route:
        case NumberRoute.FACTORINT:
            return z.factor()
        case NumberRoute.ISPRIME:
            return bool(z.is_prime())
        case _:
            raise ValueError(f"unsupported FLINT integer route {route.value}")


def _evaluate(sym: object, expr: "Expr", free: tuple["Expr", ...], digits: int, precision: Precision) -> AlgebraValue:
    scalar = expr if expr.is_number else sym.Poly(expr, _primary(free, "evaluate")).all_coeffs()[0]
    if precision is Precision.HEURISTIC:
        return sym.N(scalar, digits)
    return _certified(scalar, digits)


def _certified(scalar: "Expr", digits: int) -> "arb":
    with flint.ctx.workdps(digits):
        return flint.good(lambda: arb(str(scalar.evalf(flint.ctx.dps + 2))))


def _lower(sym: object, expr: "Expr", free: tuple["Expr", ...], backend: LowerBackend) -> object:
    match backend:
        case LowerBackend.UFUNC:
            return ufuncify(free, expr)
        case LowerBackend.NATIVE:
            return autowrap(expr, args=free)
        case _:
            return sym.lambdify(free, expr, modules=backend.value, cse=True)


def _emit(sym: object, expr: "Expr", target: CodeTarget, name: str) -> str:
    if target is CodeTarget.CXX:
        return _CODE_PRINTER[target](sym, expr)
    language = "c99" if target is CodeTarget.C else target.value
    [(_, source), *_] = codegen((name, expr), language=language, header=False, empty=False)
    return source


def _primary(free: tuple["Expr", ...], tag: str) -> "Expr":
    if not free:
        raise ValueError(f"symbolic op {tag} needs a declared free symbol; SymbolSpec.names is empty")
    return free[0]


def _as_fmpq(sym: object, cell: object) -> object:
    rational = cell if hasattr(cell, "q") else sym.Rational(cell)
    return fmpq(int(rational.p), int(rational.q))


# --- [TABLES] ---------------------------------------------------------------------------

SYMBOLIC_DERIVE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.SYMBOLIC, point="derive", arm="boundary", defect="derivation-refused", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([SYMBOLIC_DERIVE]))


# --- [COMPOSITION] ----------------------------------------------------------------------


class SymbolicDerivation:
    @staticmethod
    def derive(
        form: ExprForm, spec: SymbolSpec, *ops: SymbolicOp, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[tuple[AlgebraValue, ContentKey]]":
        terminal = ops[-1].tag if ops else "noop"

        def rail() -> "RuntimeRail[tuple[AlgebraValue, ContentKey]]":
            return ContentIdentity.of("symbolic", SymbolicPayload.of(form, spec, ops)).bind(
                lambda key: boundary(
                    SYMBOLIC_DERIVE,
                    lambda: _derive(form, spec, ops, key),
                    catch=(BasePolynomialError, MatrixError, ZeroDivisionError, ValueError, TypeError, NameError),
                )
            )

        facts = {"terminal": terminal, "op_count": len(ops), "symbols": ",".join(spec.names)}
        return evidence_run(EvidenceScope.SYMBOLIC, f"derive.{terminal}", rail, facts=facts, composition=composition)


def graduates(
    terminal: SymbolicOp, result: tuple[AlgebraValue, ContentKey], *, composition: ScopeKey = DEFAULT_SCOPE
) -> "RuntimeRail[Graduation]":
    value, key = result
    match terminal:
        case SymbolicOp(tag="evaluate", evaluate=(_digits, Precision.CERTIFIED)):
            ledger, ceiling = ({"radius": float(cast("arb", value).rad()), "unstable": 0.0}, {"radius": 1e-12, "unstable": 0.0})
        case SymbolicOp(tag="evaluate"):
            ledger, ceiling = ({"unstable": 1.0}, {"unstable": 0.0})
        case _:
            ledger, ceiling = ({"unstable": 0.0}, {"unstable": 0.0})
    return Graduation.graduates(
        EvidenceScope.SYMBOLIC.value, HandoffAxis(symbolic=terminal.tag), key, ledger, ceiling, composition=composition
    )


def _derive(form: ExprForm, spec: SymbolSpec, ops: tuple[SymbolicOp, ...], key: ContentKey) -> tuple[AlgebraValue, ContentKey]:
    if not ops:
        raise ValueError("symbolic derivation needs at least one terminal op")
    free = spec.symbols(sympy)
    staged = _sympify_form(sympy, form, free)
    *stages, terminal = ops
    folded = Block.of_seq(stages).fold(lambda acc, op: _stage(op, sympy, acc, free), staged)
    if terminal.tag in {"calculus", "rewrite", "substitute", "refine"}:
        raise ValueError(f"symbolic pipeline terminal {terminal.tag} yields a staging op, not an artifact")
    return terminal.apply(sympy, folded, free), key


def _form_spelling(form: ExprForm) -> str:
    match form:
        case str() as source:
            return source
        case tuple() as rows:
            return repr(rows)
        case _:
            return srepr(form)


def _sympify_form(sym: object, form: ExprForm, free: tuple["Expr", ...]) -> "Expr | MatrixBase":
    local = {s.name: s for s in free}
    match form:
        case str() as source:
            return sym.sympify(source, locals=local)
        case tuple() as rows if rows and isinstance(rows[0], tuple):
            return sym.Matrix([[sym.sympify(cell, locals=local) for cell in row] for row in rows])
        case _:
            return form


def _stage(op: SymbolicOp, sym: object, expr: "Expr | MatrixBase", free: tuple["Expr", ...]) -> "Expr | MatrixBase":
    if op.tag not in {"calculus", "rewrite", "substitute", "refine"}:
        raise ValueError(f"non-terminal op {op.tag} must be a calculus/rewrite/substitute/refine staging op")
    return cast("Expr | MatrixBase", op.apply(sym, expr, free))
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
