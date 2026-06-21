# [PY_COMPUTE_SYMBOLIC]

The one classical computer-algebra owner. `SymbolicOp` is a discriminated union over the symbolic op kind — a calculus transform, an algebraic solve, a canonicalization rewrite, an exact-to-numeric evaluation, a numpy/jax callable lowering, or a C-source emission — and `SymbolicDerivation` left-folds a pipeline of ops over an expression to one typed `SymbolicReceipt`. The pipeline is the whole story: a derivation is a `Sequence[SymbolicOp]` where every non-terminal op rewrites the expression (a calculus transform, a rewrite pass) and the terminal op produces the artifact (a solution set, a numeric reconstruction, a callable, a C kernel), so a `simplify -> diff -> NumpyLambdify` derivation is one fold, not a `pre`/`terminal` split. This is the one cp315-clean solver route: sympy is pure-Python and imports on the cp315 beta, so calculus, solve, rewrite, numeric evaluation, and C-codegen run as live core; only the `Lambdify(Jax)` row reads jax at usage and gates on the jaxlib `python_version<'3.15'` floor. No learned or generative symbolic search enters this owner; the derivation graduates outward on the symbolic `HandoffAxis` case.

## [01]-[INDEX]

- [01]-[OP]: `SymbolicOp` discriminated union over every symbolic op kind — calculus / solve / rewrite / numeric-evaluate / lambdify(numpy|jax) / C-codegen — each row folded by total match, the calculus/rewrite rows expression-to-expression and the solve/evaluate/lambdify/codegen rows expression-to-artifact.
- [02]-[DERIVATION]: one sympy `SymbolicDerivation` owner left-folding a `SymbolicOp` pipeline over an expression to the typed `SymbolicReceipt`, the terminal row selecting which artifact — solution set, numeric reconstruction, vectorized callable, differentiable callable, or C handoff — the receipt carries under a shared `cse` lowering.

## [02]-[OP]

- Union: `SymbolicOp` is the bounded vocabulary of what one fold step does to an expression. The rows collapse what would otherwise be sibling entries (`differentiate`, `solve`, `simplify`, `evalf`, `lambdify_numpy`, `lambdify_jax`, `codegen_c`) into one discriminant the derivation folds; the calculus/rewrite rows are expression-to-expression staging steps any number of which compose ahead of one terminal artifact row, and the terminal rows are expression-to-artifact. A new calculus transform, solve route, rewrite pass, lambdify backend, or code target is a new case or row value, never a parallel entrypoint.
  - `Calculus(kind, order)`: a `diff`/`integrate`/`limit`/`series`/`summation` transform; `kind` is the `CalculusKind` sub-vocabulary and `order` the differentiation/series order. The unevaluated `Derivative`/`Integral`/`Sum` node is forced by the same row, never a second method.
  - `Solve(route)`: an algebraic solve over `solve`/`solveset`/`linsolve`/`nonlinsolve`/`nsolve`/`dsolve`/`roots` discriminated by `SolveRoute`, yielding a closed-form, set-valued, numeric-root, or polynomial-root-multiset result rather than a callable.
  - `Rewrite(pass_)`: a canonicalization rewrite over `simplify`/`factor`/`expand`/`collect`/`cancel`/`trigsimp`/`apart`/`together`/`powsimp`/`logcombine`/`nsimplify` discriminated by `RewritePass`, producing a canonical expression that feeds a downstream terminal.
  - `Evaluate(digits)`: the exact-to-numeric bridge — `N(expr, digits)` lifting a numeric expression to an arbitrary-precision decimal, falling to `Poly(expr, primary).all_coeffs()` on a still-symbolic polynomial to read the leading-coefficient magnitude at the same precision — producing the precision-claimed numeric reconstruction the `.api` graduation path names under one `numeric` outcome.
  - `Lambdify(backend)`: the callable lowering — `lambdify(free, expr, modules=backend.value, cse=True)` — discriminated by `LambdifyBackend` over `numpy`/`jax`, producing the vectorized study closure (numpy, unconditional on cp315) or the differentiable jax callable the `solvers/sensitivity.md#SENSITIVITY` adjoint family consumes through `jax.grad`/`jax.vjp`; the `jax` row is the one case that reads jax and gates on the jaxlib `python_version<'3.15'` floor.
  - `CCodegen(name, standard)`: the C handoff emission — `utilities.codegen.codegen((name, expr), language="C", ...)` over the C99 `standard`, or the standalone `printing.c.ccode` expression render — producing the source the C# graduation gate consumes.
- Fold: `SymbolicOp.apply(sym, expr, free)` is a total match over the union tag returning `Outcome` — a `tagged_union` of `staged` (the rewritten expression for calculus/rewrite), `solution`, `numeric`, `callable_`, or `c_source`. The match closes with `assert_never`; adding a row without a match arm is a type error, not a runtime fallthrough. The pipeline fold requires the last op to yield a non-`staged` `Outcome` and every earlier op to yield `staged`; a pipeline ending on a staging row is a boundary fault, not a silent identity.
- Growth: a new code target (`Fortran`, `Rust`, `Octave`) is one `CCodegen`-sibling `language` row; a new lambdify backend is one `LambdifyBackend` value; a new calculus transform is one `CalculusKind`; a new solve route is one `SolveRoute`; a new rewrite pass is one `RewritePass`. Zero new owner surface, never a parallel per-artifact entry.

## [03]-[DERIVATION]

- Owner: `SymbolicDerivation` — the sympy derivation that threads `symbols` over an expression and left-folds a `SymbolicOp` pipeline to the typed `SymbolicReceipt`. One derivation of an expression yields the numpy study callable, the jax differentiable callable, the precision-claimed numeric reconstruction, or the C kernel handoff from one shared common-subexpression lowering rather than parallel entries; the terminal op's `Outcome` tag selects which `SymbolicReceipt` factory the fold lands on.
- Entry: `SymbolicDerivation.derive(expr, symbols, *ops)` is the one railed entrypoint — it builds the free symbols with `sympy.symbols`, left-folds the variadic `ops` pipeline (each staging op rewriting the carried expression, the terminal op producing the `Outcome`), and folds the `Outcome` to a `SymbolicReceipt`, all inside one `boundary(f"symbolic.{ops[-1].tag}", ...)` returning `RuntimeRail[SymbolicReceipt]`. There is no second un-railed `of` constructor; the sympy import, the fold, and the receipt construction live behind the single boundary so a sympy raise on any step is the boundary fault, never an escaped exception. The graduation gate reads the `c_source` evidence as the kernel-handoff artifact through the symbolic `HandoffAxis` case.
- Receipt: `SymbolicReceipt` is one typed `Struct` with semantic named fields and `Outcome`-keyed factories — `Solved(route, cardinality)`, `Numeric(digits, magnitude)`, `Lowered(backend, arity)`, `Emitted(name, c_bytes)` — collapsing what would be a four-nullable result bag into one receipt the way `analysis/spatial.md#SPATIAL` collapses `Proximity`/`Complex`/`Boundary`. `contribute` emits one `Receipt.of("emitted", ...)` row carrying the terminal-op tag, the symbol list, and the typed evidence (the result-set `cardinality` and `route` for `Solve`, the `digits`/`magnitude` precision claim for `Evaluate`, the callable `arity` and `backend` for `Lambdify`, the C-source `c_bytes` and `name` for `CCodegen`); the derivation graduates outward through `graduation/handoff.md#GRADUATION` on the symbolic axis once stable and reproducible.
- Packages: `sympy` (`symbols`, `Expr.evalf`, `Poly.all_coeffs`, `lambdify`, `cse`, `utilities.codegen.codegen`, `printing.c.ccode`, `diff`, `integrate`, `limit`, `series`, `summation`, `solve`, `solveset`, `linsolve`, `nonlinsolve`, `nsolve`, `dsolve`, `roots`, `simplify`, `factor`, `expand`, `collect`, `cancel`, `trigsimp`, `apart`, `together`, `powsimp`, `logcombine`, `nsimplify`, `Wild`), `jax` (`grad`, `vjp` — consumed by the sensitivity owner over the `jax` lambdify callable), `numpy` (the `numpy` lambdify target module), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor`).
- Boundary: classical CAS only — calculus, solving, simplification, exact-to-numeric evaluation, numpy/jax/C lowering are in-scope; no learned or generative symbolic search, no numeric kernels scipy/numpy own, no production algebra a C# owner owns after graduation. sympy is pure-Python and imports on cp315, so every row except the `jax` lambdify backend is live core, reflected rather than deploy-gated; the `jax` row reads jax at usage and is authored against the documented API on the jaxlib `python_version<'3.15'` floor, the lone gated row beside the cp315-clean band.

```python signature
from collections.abc import Sequence
from enum import StrEnum
from functools import reduce
from typing import TYPE_CHECKING, Literal, assert_never

from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    from sympy import Expr


class CalculusKind(StrEnum):
    DIFF = "diff"
    INTEGRATE = "integrate"
    LIMIT = "limit"
    SERIES = "series"
    SUMMATION = "summation"


class SolveRoute(StrEnum):
    SOLVE = "solve"
    SOLVESET = "solveset"
    LINSOLVE = "linsolve"
    NONLINSOLVE = "nonlinsolve"
    NSOLVE = "nsolve"
    DSOLVE = "dsolve"
    ROOTS = "roots"


class RewritePass(StrEnum):
    SIMPLIFY = "simplify"
    FACTOR = "factor"
    EXPAND = "expand"
    COLLECT = "collect"
    CANCEL = "cancel"
    TRIGSIMP = "trigsimp"
    APART = "apart"
    TOGETHER = "together"
    POWSIMP = "powsimp"
    LOGCOMBINE = "logcombine"
    NSIMPLIFY = "nsimplify"


class LambdifyBackend(StrEnum):
    NUMPY = "numpy"
    JAX = "jax"


@tagged_union(frozen=True)
class Outcome:
    tag: Literal["staged", "solution", "numeric", "callable_", "c_source"] = tag()
    staged: "Expr" = case()
    solution: tuple[SolveRoute, int] = case()
    numeric: tuple[int, float] = case()
    callable_: tuple[LambdifyBackend, int] = case()
    c_source: tuple[str, str] = case()


@tagged_union(frozen=True)
class SymbolicOp:
    tag: Literal["calculus", "solve", "rewrite", "evaluate", "lambdify", "c_codegen"] = tag()
    calculus: tuple[CalculusKind, int] = case()
    solve: SolveRoute = case()
    rewrite: RewritePass = case()
    evaluate: int = case()
    lambdify: LambdifyBackend = case()
    c_codegen: tuple[str, str] = case()

    @staticmethod
    def Calculus(kind: CalculusKind, order: int = 1) -> "SymbolicOp":
        return SymbolicOp(calculus=(kind, order))

    @staticmethod
    def Solve(route: SolveRoute = SolveRoute.SOLVE) -> "SymbolicOp":
        return SymbolicOp(solve=route)

    @staticmethod
    def Rewrite(pass_: RewritePass = RewritePass.SIMPLIFY) -> "SymbolicOp":
        return SymbolicOp(rewrite=pass_)

    @staticmethod
    def Evaluate(digits: int = 15) -> "SymbolicOp":
        return SymbolicOp(evaluate=digits)

    @staticmethod
    def Lambdify(backend: LambdifyBackend = LambdifyBackend.NUMPY) -> "SymbolicOp":
        return SymbolicOp(lambdify=backend)

    @staticmethod
    def CCodegen(name: str = "kernel", standard: str = "c99") -> "SymbolicOp":
        return SymbolicOp(c_codegen=(name, standard))

    def apply(self, sym: object, expr: "Expr", free: Sequence["Expr"]) -> Outcome:
        primary = free[0]
        match self:
            case SymbolicOp(tag="calculus", calculus=(CalculusKind.DIFF, order)):
                return Outcome(staged=sym.diff(expr, primary, order).doit())
            case SymbolicOp(tag="calculus", calculus=(CalculusKind.INTEGRATE, _)):
                return Outcome(staged=sym.integrate(expr, primary).doit())
            case SymbolicOp(tag="calculus", calculus=(CalculusKind.LIMIT, _)):
                return Outcome(staged=sym.limit(expr, primary, 0))
            case SymbolicOp(tag="calculus", calculus=(CalculusKind.SERIES, order)):
                return Outcome(staged=sym.series(expr, primary, 0, order).removeO())
            case SymbolicOp(tag="calculus", calculus=(CalculusKind.SUMMATION, order)):
                return Outcome(staged=sym.summation(expr, (primary, 0, order)).doit())
            case SymbolicOp(tag="rewrite", rewrite=pass_):
                fn = sym.collect if pass_ is RewritePass.COLLECT else getattr(sym, pass_.value)
                return Outcome(staged=fn(expr, primary) if pass_ is RewritePass.COLLECT else fn(expr))
            case SymbolicOp(tag="solve", solve=SolveRoute.SOLVESET):
                solution = sym.solveset(expr, primary)
                return Outcome(solution=(SolveRoute.SOLVESET, _cardinality(solution)))
            case SymbolicOp(tag="solve", solve=SolveRoute.NSOLVE):
                solution = sym.nsolve(expr, free, [0.0] * len(free))
                return Outcome(solution=(SolveRoute.NSOLVE, _cardinality(solution)))
            case SymbolicOp(tag="solve", solve=SolveRoute.DSOLVE):
                solution = sym.dsolve(expr)
                return Outcome(solution=(SolveRoute.DSOLVE, _cardinality(solution)))
            case SymbolicOp(tag="solve", solve=SolveRoute.ROOTS):
                solution = sym.roots(sym.Poly(expr, primary))
                return Outcome(solution=(SolveRoute.ROOTS, len(solution)))
            case SymbolicOp(tag="solve", solve=route):
                solution = getattr(sym, route.value)(expr, *free)
                return Outcome(solution=(route, _cardinality(solution)))
            case SymbolicOp(tag="evaluate", evaluate=digits):
                value = sym.N(expr, digits)
                if value.is_number:
                    return Outcome(numeric=(digits, abs(float(value))))
                coeffs = sym.Poly(expr, primary).all_coeffs()
                return Outcome(numeric=(digits, abs(float(sym.N(coeffs[0], digits)))))
            case SymbolicOp(tag="lambdify", lambdify=backend):
                sym.lambdify(free, expr, modules=backend.value, cse=True)
                return Outcome(callable_=(backend, len(free)))
            case SymbolicOp(tag="c_codegen", c_codegen=(name, standard)):
                from sympy.utilities.codegen import codegen

                [(_, source), *_] = codegen((name, expr), language="C", standard=standard, header=False, empty=False)
                return Outcome(c_source=(name, source))
            case unreachable:
                assert_never(unreachable)


class SymbolicReceipt(Struct, frozen=True):
    op: str
    symbols: tuple[str, ...]
    route: str = ""
    cardinality: int = 0
    digits: int = 0
    magnitude: float = 0.0
    backend: str = ""
    arity: int = 0
    name: str = ""
    c_bytes: int = 0

    @staticmethod
    def of(op: SymbolicOp, symbols: tuple[str, ...], outcome: Outcome) -> "SymbolicReceipt":
        match outcome:
            case Outcome(tag="solution", solution=(route, cardinality)):
                return SymbolicReceipt(op.tag, symbols, route=route.value, cardinality=cardinality)
            case Outcome(tag="numeric", numeric=(digits, magnitude)):
                return SymbolicReceipt(op.tag, symbols, digits=digits, magnitude=magnitude)
            case Outcome(tag="callable_", callable_=(backend, arity)):
                return SymbolicReceipt(op.tag, symbols, backend=backend.value, arity=arity)
            case Outcome(tag="c_source", c_source=(name, source)):
                return SymbolicReceipt(op.tag, symbols, name=name, c_bytes=len(source))
            case Outcome(tag="staged"):
                raise ValueError("symbolic pipeline ends on a staging op, not a terminal artifact")
            case unreachable:
                assert_never(unreachable)

    def contribute(self) -> Receipt:
        facts = {"op": self.op, "symbols": ",".join(self.symbols)}
        facts |= {"route": self.route, "cardinality": str(self.cardinality)} if self.route else {}
        facts |= {"digits": str(self.digits), "magnitude": f"{self.magnitude:.6g}"} if self.digits else {}
        facts |= {"backend": self.backend, "arity": str(self.arity)} if self.backend else {}
        facts |= {"name": self.name, "c_bytes": str(self.c_bytes)} if self.c_bytes else {}
        return Receipt.of("emitted", "compute.symbolic", self.op, facts)


def _cardinality(solution: object) -> int:
    return len(tuple(solution)) if hasattr(solution, "__len__") else 1


class SymbolicDerivation:
    @staticmethod
    def derive(expr: str | object, symbols: tuple[str, ...], *ops: SymbolicOp) -> "RuntimeRail[SymbolicReceipt]":
        def run() -> SymbolicReceipt:
            import sympy

            free = sym_free if isinstance(sym_free := sympy.symbols(symbols), tuple) else (sym_free,)
            staged = sympy.sympify(expr)
            *stages, terminal = ops
            staged = reduce(lambda acc, op: _stage(op, sympy, acc, free), stages, staged)
            return SymbolicReceipt.of(terminal, symbols, terminal.apply(sympy, staged, free))

        return boundary(f"symbolic.{ops[-1].tag}", run)


def _stage(op: SymbolicOp, sym: object, expr: "Expr", free: Sequence["Expr"]) -> "Expr":
    match op.apply(sym, expr, free):
        case Outcome(tag="staged", staged=rewritten):
            return rewritten
        case _:
            raise ValueError(f"non-terminal op {op.tag} must be a calculus/rewrite staging op")
```

## [04]-[RESEARCH]

- [SYMPY_CORE]: sympy is pure-Python and cp315-clean; the `symbols`/`sympify`/`lambdify`/`cse`/`utilities.codegen.codegen`/`printing.c.ccode`/`diff`/`integrate`/`limit`/`series`/`summation`/`solve`/`solveset`/`linsolve`/`nonlinsolve`/`nsolve`/`dsolve`/`roots`/`simplify`/`factor`/`expand`/`collect`/`cancel`/`trigsimp`/`apart`/`together`/`powsimp`/`logcombine`/`nsimplify`/`Wild` spellings and the `Expr.evalf`/`Poly.all_coeffs` members verify against the `compute/.api/sympy.md` `[ENTRYPOINT_SCOPE]` tables. The calculus rows force unevaluated `Derivative`/`Integral`/`Sum` nodes through `.doit()` and trim the `series` asymptotic `Order` term through `.removeO()`, both confirmed against the calculus-node table. Every row except the `jax` lambdify backend is live core on the cp315 beta.
- [NUMERIC_BRIDGE]: the `Evaluate` row owns the `.api` `[GRADUATION_PATH]` numeric bridge — `N(expr, digits)` lifts a numeric expression to a fixed-precision decimal, and on a still-symbolic polynomial `Poly(expr, primary).all_coeffs()` reads the leading coefficient whose magnitude evaluates at the same precision — so the precision claim and the leading-magnitude reconstruction enter the same `SymbolicReceipt` the C-codegen and lambdify rows feed, rather than a separate evaluation surface. `cse=True` precedes every codegen and lambdify lowering to dedupe shared subexpressions as the `.api` law requires.
- [JAX_LAMBDIFY]: `sympy.lambdify(free, expr, modules="jax", cse=True)` emits a differentiable jax callable; the `Lambdify(LambdifyBackend.JAX)` row bridges this owner and the JAX autodiff family in `solvers/sensitivity.md#SENSITIVITY`, where `jax.grad`/`jax.vjp` consume the callable. The `jax` backend gates on the jaxlib `python_version<'3.15'` floor and is authored against the documented `jax` `.api` until the jaxlib cp315 wheel resolves; the surrounding owner stays cp315-clean because only the `jax` `LambdifyBackend` value passes `modules="jax"` and a `numpy` lowering never imports jax.
