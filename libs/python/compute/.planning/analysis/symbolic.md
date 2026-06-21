# [PY_COMPUTE_SYMBOLIC]

The one classical computer-algebra owner. A derivation is a `Block[SymbolicOp]` left-folded over an `ExprForm` to one typed `SymbolicReceipt`: every non-terminal op rewrites the carried expression (calculus transform, rewrite pass, substitution, assumption refinement) and the terminal op produces the artifact (solution set, matrix-invariant spectrum, number-theoretic structure, certified-or-heuristic numeric reconstruction, vectorized/compiled callable, generated source module), so a `Refine -> simplify -> diff -> Lower(numpy)` derivation is one fold, never a `pre`/`terminal` split. The terminal artifact is one discriminated `Outcome` the `SymbolicReceipt` carries whole — the same evidence-on-the-receipt collapse `analysis/signal.md#DSP` holds over `SignalEvidence`, `analysis/transform.md#TRANSFORM` over `TransformEvidence`, and `analysis/spatial.md#SPATIAL` over `SpatialEvidence` — and the input is parameterized as tightly as the output: `ExprForm` discriminates a `str` spelling, a `MatrixForm`, or a constructed `Expr` through one `derive` entry.

This is the cp315-clean solver route. `sympy` is pure-Python and imports on the cp315 beta, so calculus, rewrite, solve, matrix algebra, assumption logic, number theory, heuristic numeric evaluation, and the source-printer family run as live core; `python-flint`'s `fmpz_poly`/`fmpq_mat`/`arb` exact kernels and the certified-ball `Evaluate` precision row gate on the `python_version<'3.15'` companion band, `Lower(jax)` reads `jax` at usage on the jaxlib floor, and `Lower(native)`'s `autowrap`/`ufuncify` rows gate on a host C/Fortran toolchain. The derivation keys its `SymbolicReceipt` through the runtime `ContentIdentity` over a canonical `SymbolicPayload`, so a repeated derivation at identical `(form, spec, ops)` is a cache hit by reference. No learned or generative symbolic search enters this owner; a `source` derivation graduates outward on the symbolic `HandoffAxis` case once stable and reproducible.

## [01]-[INDEX]

- [01]-[OP]: `SymbolicOp` discriminated union over every symbolic op kind — calculus / rewrite / substitute / refine / solve / linalg / number-theory / evaluate / lower(numpy|jax|ufunc|native) / codegen — each folded by total `match` against an op dispatch table, the calculus/rewrite/substitute/refine rows expression-to-expression staging and the solve/linalg/number-theory/evaluate/lower/codegen rows expression-to-artifact; the polynomial solve/linalg routes carry a `GroundDomain` axis selecting the `sympy` symbolic kernel or the `python-flint` exact-arithmetic accelerator, the `Evaluate` row a `Precision` axis selecting the heuristic `mpmath` decimal or the certified `arb`-ball reconstruction.
- [02]-[DERIVATION]: one `sympy` `SymbolicDerivation` owner left-folding a `SymbolicOp` pipeline over an `ExprForm` to the typed `SymbolicReceipt`, the terminal `Outcome` case carrying the artifact the receipt spreads — solution set, matrix-invariant spectrum, number-theoretic structure, numeric reconstruction with certified error bound, vectorized/differentiable/compiled callable, or generated source — under a shared `cse` lowering, one assumption-carrying `SymbolSpec` vocabulary, and one `ContentIdentity`-keyed `content_key`.

## [02]-[OP]

`SymbolicOp` is the bounded vocabulary of what one fold step does to an expression. The rows collapse what would otherwise be sibling entrypoints (`differentiate`, `simplify`, `subs`, `refine`, `solve`, `eigenvals`, `factorint`, `evalf`, `lambdify`, `ufuncify`, `ccode`, `rust_code`) into one discriminant the derivation folds. Staging rows are expression-to-expression and compose in any number ahead of one terminal artifact row; terminal rows are expression-to-artifact. A new calculus transform, rewrite pass, solve route, matrix extraction, number-theoretic query, lowering backend, or code target is a new sub-vocabulary value or dispatch-table row, never a parallel entrypoint or a parallel emitter.

| [INDEX] | [ROW]          | [KIND]   | [CASE_DATA]                          | [SYMPY_SURFACE]                                                                             |
| :-----: | :------------- | :------- | :----------------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `Calculus`     | staging  | `(CalculusKind, int)` kind+order     | `diff` / `integrate` / `limit` / `series` / `summation`, forcing `Derivative`/`Integral`/`Sum` via `.doit()`, trimming `Order` via `.removeO()` |
|  [02]   | `Rewrite`      | staging  | `RewritePass`                        | `simplify` / `factor` / `expand` / `collect` / `cancel` / `trigsimp` / `radsimp` / `ratsimp` / `apart` / `together` / `powsimp` / `logcombine` / `nsimplify` |
|  [03]   | `Substitute`   | staging  | `(SubstituteMode, Map[str, str])`    | `Expr.subs` simultaneous map / `Expr.replace` over a `Wild` pattern / `Expr.rewrite` into a functional basis |
|  [04]   | `Refine`       | staging  | `AssumptionPredicate`                | `refine(expr, Q.<pred>(sym))` simplification under the `SymbolSpec` assumption context |
|  [05]   | `Solve`        | terminal | `(SolveRoute, SolveDomain, GroundDomain)` | `solve` / `solveset(domain=)` / `linsolve` / `nonlinsolve` / `nsolve` / `dsolve` / `pdsolve` / `roots`, plus `Poly.real_roots`/`nroots`/`factor_list`/`discriminant`/`resultant`, the polynomial routes lowering to `fmpz_poly`/`fmpq_poly` on the `FLINT` ground domain |
|  [06]   | `LinAlg`       | terminal | `(MatrixRoute, GroundDomain)`        | `Matrix.eigenvals` / `eigenvects` / `det` / `charpoly` / `minpoly` / `rref` / `nullspace` / `rank` / `inv` / `pinv` / `LUdecomposition` / `QRdecomposition` / `cholesky` / `diagonalize` / `jordan_form` / `singular_values` over a `MatrixSymbol`, the exact routes lowering to `fmpz_mat`/`fmpq_mat` on the `FLINT` ground domain |
|  [07]   | `NumberTheory` | terminal | `NumberRoute`                        | `factorint` / `primerange` / `isprime` / `gcd` / `lcm`, the integer-domain structure beside the `FLINT` `fmpz.factor`/`is_prime` accelerator |
|  [08]   | `Evaluate`     | terminal | `(int, Precision)` digits+precision  | `N(expr, digits)` heuristic exact-to-decimal through the `mpmath` context, or `flint.good` certified `arb`-ball reconstruction carrying `rad()`, falling to `Poly(expr, primary).all_coeffs()` lead-coefficient magnitude on a still-symbolic polynomial |
|  [09]   | `Lower`        | terminal | `LowerBackend`                       | `lambdify(modules=numpy\|jax, cse=True)` vectorized/differentiable callable, `ufuncify` broadcasting ufunc, `autowrap` compiled native callable |
|  [10]   | `Codegen`      | terminal | `(CodeTarget, str)` target+name      | `utilities.codegen.codegen` named module or the per-target printer `ccode`/`cxxcode`/`fcode`/`rust_code`/`julia_code`/`octave_code`, one printer surface keyed by `CodeTarget` |

- Calculus rows force the unevaluated `Derivative`/`Integral`/`Sum` node through `.doit()` and trim the `series` asymptotic `Order` term through `.removeO()` in the same row, never a second method.
- Substitution rows discriminate `SubstituteMode` over `subs`/`replace`/`rewrite`: `subs` threads a simultaneous structural map, `replace` matches a `Wild` pattern, `rewrite` retargets a functional basis (`exp`, `Piecewise`). The map keys/values are symbol/expression spellings resolved against the live `SymbolSpec`, never raw strings escaping the boundary.
- `Refine` consumes the assumptions the `SymbolSpec` already declares: `refine(expr, Q.positive(sym))` rather than re-deriving assumptions post hoc, satisfying the assumption-as-input law. `ask(Q.<pred>(sym))` gates the refinement when the predicate is decidable.
- `Solve` yields a closed-form, set-valued (`solveset` over a `SolveDomain` of `Reals`/`Complexes`/`Integers`/`Naturals`), numeric-root, polynomial-root-multiset, exact-factorization, or resultant result rather than a callable. The polynomial routes read `Poly.real_roots`/`nroots`/`factor_list`/`discriminant` for isolated roots, factor structure, root count, and discriminant evidence, and on the `FLINT` `GroundDomain` lower the carried `Poly` to an `fmpz_poly`/`fmpq_poly` whose `factor`/`roots`/`resultant`/`complex_roots` own the fast exact ground-domain arithmetic; the `solution` evidence carries `(route, cardinality, metric)` where `metric` is the discriminant magnitude, the resultant magnitude, or the `nsolve` residual per route, never a dead `0.0`.
- `LinAlg` builds a `MatrixSymbol`-backed dense `Matrix` from the carried expression rows and extracts the exact spectrum, eigenvectors, determinant, characteristic or minimal polynomial, reduced row-echelon form, null-space basis, rank, inverse, pseudo-inverse, or canonical decomposition (`LU`/`QR`/`cholesky`/`diagonalize`/`jordan_form`/`singular_values`) — the matrix-algebra `[ENTRYPOINT_SCOPE]` surface the owner owns rather than deferring to a numeric kernel before graduation; the `FLINT` `GroundDomain` lowers the matrix to an `fmpz_mat`/`fmpq_mat` whose `det`/`rank`/`charpoly`/`minpoly`/`inv`/`hnf`/`snf`/`lll` own the exact lattice and linear algebra.
- `NumberTheory` is the integer-domain terminal: `factorint` returns the prime-factor multiset, `primerange`/`isprime` the prime structure, `gcd`/`lcm` the polynomial/integer divisor algebra, beside the `FLINT` `fmpz.factor`/`is_prime` accelerator on the companion band; the `arithmetic` evidence carries `(route, cardinality, magnitude)` so a factorization names its distinct-prime count and largest-prime magnitude rather than smuggling the result through the `solution` slots.
- `Evaluate` carries a `Precision` axis: `HEURISTIC` lifts `N(expr, digits)` through the bundled `mpmath` context (no error bound), `CERTIFIED` re-evaluates the closed form through a `python-flint` `arb`/`acb` ball under `flint.good` to the requested `dps`, reading `mid()` as the value and `rad()` as the certified error bound the heuristic path lacks; both fall to `Poly(expr, primary).all_coeffs()[0]` lead-coefficient magnitude on a still-symbolic polynomial, both into one `numeric` carrying `(digits, magnitude, radius)`.
- `Lower` is the callable lowering discriminated by `LowerBackend` over `numpy`/`jax`/`ufunc`/`native`: `lambdify(free, expr, modules=..., cse=True)` for the vectorized `numpy` study closure (unconditional on cp315) and the differentiable `jax` callable the `solvers/sensitivity.md#SENSITIVITY` adjoint family consumes through `jax.grad`/`jax.vjp`; `ufuncify(free, expr)` for a broadcasting numpy ufunc and `autowrap(expr)` for a compiled native extension when the hot path warrants it. The `jax` value reads `jax` and gates on the jaxlib floor; the `native` value gates on a host C/Fortran toolchain; `numpy`/`ufunc` stay cp315-clean. The terminal materializes the callable as the witness that lowering compiles and records `(backend, arity)` as the `callable_` evidence; the live callable is not stored on the frozen serializable `SymbolicReceipt` (a callable is not `msgspec`-encodable). The `sensitivity` consumer retrieves the live `fn` through the lowering seam keyed by the derivation's `content_key`, never off the receipt struct.
- `Codegen` is the one source-emission surface keyed by `CodeTarget` over `c`/`cxx`/`fortran`/`rust`/`julia`/`octave`: a `_CODE_PRINTER` dispatch row selects `ccode`/`cxxcode`/`fcode`/`rust_code`/`julia_code`/`octave_code`, and `utilities.codegen.codegen(language=...)` wraps a named, header-stripped module where the target supports it. A new code target is one `CodeTarget` value plus one `_CODE_PRINTER` row, never a parallel emitter — the catalog's single-polymorphic-codegen law.

`SymbolicOp.apply(sym, expr, free)` is a total `match` over the union tag returning `Outcome`, a `tagged_union` of `staged` (the rewritten `Expr` for calculus/rewrite/substitute/refine), `solution`, `spectrum`, `arithmetic`, `numeric`, `callable_`, or `source`, each minted through a named `Outcome` static factory so construction reads `Outcome.Solution(route, n, metric)` rather than a raw keyword. The match closes with `assert_never`; adding a case without a match arm is a type error, not a runtime fallthrough. The fold requires every staging op to yield `staged` (enforced by `_stage`) and the terminal op to yield a non-`staged` `Outcome` (enforced inside the `derive` boundary); a pipeline ending on a staging row is a boundary fault, not a silent identity.

## [03]-[DERIVATION]

`SymbolicDerivation` threads an assumption-carrying `SymbolSpec` over an `ExprForm` and left-folds a `Block[SymbolicOp]` pipeline to one typed `SymbolicReceipt`. A single derivation yields the numpy study callable, the jax differentiable callable, the compiled native callable, the matrix-invariant spectrum, the number-theoretic structure, the certified-or-heuristic numeric reconstruction, or the generated source handoff from one shared `cse` lowering rather than parallel entries; the terminal op's `Outcome` case is the receipt's evidence carrier, not a tag selecting one of seven sibling factories.

- Entry: `SymbolicDerivation.derive(form, spec, *ops)` is the one railed entrypoint returning `RuntimeRail[SymbolicReceipt]`. It keys the derivation through the railed `ContentIdentity.of("symbolic", SymbolicPayload.of(form, spec, ops), IdentityPolicy())` and `bind`s the resolved `content_key` into one `boundary(f"symbolic.{ops[-1].tag if ops else 'noop'}", run)`, mirroring the `numerics/array.md#PAYLOAD` admit-then-bind weave so a repeated derivation at identical `(form, spec, ops)` is a cache hit by reference and a canonical-encode fault rails before any `sympy` work. Inside `run` it mints the assumption-carrying free symbols from `spec.symbols(sympy)` (each name a `sympy.Symbol` carrying its declared `real`/`positive`/`integer` assumption), `sympify`s the `ExprForm` against those exact symbol objects through a `locals` binding, left-folds the staging ops with `Block.fold` (each rewriting the carried `Expr`), applies the terminal op, and carries the resulting `Outcome` into one `SymbolicReceipt`. The `sympy` import, the empty-pipeline gate, the fold, and the receipt construction all live inside `run` so every raise — a `sympy`/`flint` step, an empty `*ops`, or a `staged` terminal — converts to one `BoundaryFault` rather than escaping; the subject reads a stable label, never `ops[-1]` at construction time where an empty pipeline would escape the rail as `IndexError`. There is no second un-railed constructor. The graduation gate reads the `source` evidence as the kernel-handoff artifact through the symbolic `HandoffAxis` case.
- `ExprForm` is the polymorphic input carrier: a `str` source spelling `sympify`s directly, a `MatrixForm` (a row-major `tuple[tuple[str, ...], ...]` of cell spellings) builds the `Matrix` the `LinAlg` terminal extracts from, and an already-constructed `Expr` passes through. One `derive` entry discriminates the input shape rather than `derive`/`derive_matrix`/`derive_expr` siblings — input polymorphism mirroring the `Outcome` output polymorphism.
- Identity: `SymbolicPayload` is the canonical `msgspec.Struct(frozen=True, gc=False)` the `canonical` `IdentitySource` modality folds through the one cached deterministic encoder — the `repr` of the `form` (a `str` spelling, the `MatrixForm` tuple, or `srepr(expr)` for a constructed `Expr`), the sorted `(name, predicate)` assumption pairs, and the ordered op-tag-and-case-data tuple — so the runtime content owner mints the key rather than a hand-rolled `msgspec.json.encode` plus `b"\x00".join` reinvention of the canonical encode. Two derivations with the same expression but a different assumption context, op pipeline, or terminal route key distinctly.
- Receipt: `SymbolicReceipt` is one `Struct(frozen=True)` of `(op, symbols, content_key, outcome)`, where `outcome` is the terminal `Outcome` case and owns the `facts()` total projection — collapsing what would be a multi-field default-zero result bag into one discriminated carrier the way `analysis/signal.md#DSP` collapses `Spectral`/`Multiresolution`/`Scale`/`Packet` and `analysis/spatial.md#SPATIAL` collapses `Proximity`/`Complex`/`Boundary`. `contribute` yields the `Iterable[Receipt]` the `ReceiptContributor` port declares — one `Receipt.of("compute.symbolic", ("emitted", op_tag, facts))` row over the `(Phase, subject, facts)` `Evidence` triple, never the four-positional `Receipt.of("emitted", owner, subject, facts)` the factory rejects — whose `facts` spreads the `op`/`symbols`/`content_key.project("hex")` render plus only the slots the matched `Outcome` carries (`route`/`cardinality`/`metric` for `solution`, `route`/`dimension`/`invariant` for `spectrum`, `route`/`cardinality`/`magnitude` for `arithmetic`, `digits`/`magnitude`/`radius` for `numeric`, `backend`/`arity` for `callable_`, `target`/`name`/`byte_count` for `source`). `derive` is the `RuntimeRail[SymbolicReceipt]` boundary owner; the resolved `SymbolicReceipt` is the contributor the study spine harvests through the `runtime/observability/receipts#RECEIPT` `@receipted` aspect on the `Ok` arm, never an inline emit, the same convention `analysis/signal.md#DSP` and `analysis/spatial.md#SPATIAL` hold. A `source` derivation graduates outward through `graduation/handoff.md#GRADUATION` on the `HandoffAxis(symbolic=...)` case once stable and reproducible.
- Growth: a new calculus transform is one `CalculusKind` row plus one `_CALCULUS` entry; a new rewrite pass is one `RewritePass` row; a new solve route is one `SolveRoute` row on the existing `Solve` case; a new matrix extraction is one `MatrixRoute` row; a new number-theoretic query is one `NumberRoute` row; an exact-arithmetic accelerator is the existing `GroundDomain.FLINT` value on the polynomial route; a new lowering backend is one `LowerBackend` row on the `_lower` match; a new code target is one `CodeTarget` row plus one `_CODE_PRINTER` entry; a new artifact shape is one `Outcome` case plus its `facts()` arm and one terminal `apply` arm; zero new owner surface, never a parallel entrypoint or emitter.
- Packages: `sympy` (`symbols`, `sympify`, `srepr`, `Symbol`, `Wild`, `Matrix`, `MatrixSymbol`, `Q`, `refine`, `ask`, `Expr.evalf`, `Expr.subs`, `Expr.replace`, `Expr.rewrite`, `Poly.all_coeffs`/`real_roots`/`nroots`/`factor_list`/`discriminant`/`resultant`/`LC`, `Matrix.inv`/`pinv`/`jordan_form`/`cholesky`/`eigenvects`/`minimal_polynomial`, `factorint`/`primerange`/`isprime`/`gcd`/`lcm`, `lambdify`, `cse`, `utilities.codegen.codegen`, `utilities.autowrap.autowrap`/`ufuncify`, `printing.c.ccode`, `printing.cxx.cxxcode`, `printing.fortran.fcode`, `printing.rust.rust_code`, `printing.julia.julia_code`, `printing.octave.octave_code`, `diff`, `integrate`, `limit`, `series`, `summation`, `solve`, `solveset`, `linsolve`, `nonlinsolve`, `nsolve`, `dsolve`, `pdsolve`, `roots`, `Reals`/`Complexes`/`Integers`/`Naturals`, `simplify`, `factor`, `expand`, `collect`, `cancel`, `trigsimp`, `radsimp`, `ratsimp`, `apart`, `together`, `powsimp`, `logcombine`, `nsimplify`), `python-flint` (`fmpz`/`fmpq`, `fmpz_poly`/`fmpq_poly` `factor`/`roots`/`resultant`/`complex_roots`, `fmpz_mat`/`fmpq_mat` `det`/`rank`/`charpoly`/`minpoly`/`inv`/`hnf`/`snf`/`lll`, `arb`/`acb`, `flint.good` the adaptive certified-precision driver, `flint.ctx.dps` the precision context — the exact-arithmetic and certified-ball accelerator on the `python_version<'3.15'` companion band), `mpmath` (the bundled `N`/`evalf` heuristic precision context), `jax` (`grad`/`vjp`, consumed by `solvers/sensitivity.md#SENSITIVITY` over the `jax` `Lower` callable), `numpy` (the `numpy`/`ufunc` lowering target module), `expression` (`Block` folding the pipeline, `Map` carrying the substitution and dispatch tables, `tagged_union`/`tag`/`case`), `msgspec` (`Struct(frozen=True)` the `SymbolicReceipt`/`SymbolSpec`, `Struct(frozen=True, gc=False)` the `SymbolicPayload`), runtime (`RuntimeRail`, `boundary`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`/`ReceiptContributor`, the `@receipted` study-spine emit aspect).
- Boundary: classical CAS only — calculus, rewriting, substitution, assumption refinement, solving, symbolic linear algebra, integer number theory, exact-to-numeric evaluation (heuristic and certified), numpy/jax/ufunc/native lowering, and multi-language source emission are in-scope; no learned or generative symbolic search, no numeric kernels `scipy`/`numpy` own at runtime, no production algebra a C# owner owns after graduation. `sympy` is pure-Python and imports on cp315, so every row except the `FLINT` `GroundDomain` accelerator, the `CERTIFIED` `Precision` row, the `Lower(JAX)`, and the `Lower(NATIVE)` backends is live core, reflected rather than deploy-gated; the `FLINT` and `CERTIFIED` rows read `python-flint` on the `python_version<'3.15'` companion band, the `jax` row reads `jax` at usage on the jaxlib floor, and the `native` row gates on a host C/Fortran toolchain. `sympy` owns the symbolic algebra, `python-flint` owns the fast exact ground-domain arithmetic and the certified ball, and the two meet only through the shared `SolveRoute`/`MatrixRoute`/`Precision` vocabulary keyed by `GroundDomain`, never a parallel solver surface. The deleted forms are a fat default-zero `SymbolicReceipt`, a receipt with no `content_key` where the sibling owners key by `ContentIdentity`, a `solution` slot carrying a dead `discriminant=0.0` where the `metric` slot reads a real per-route fact, a bare-`Receipt` `contribute` against the `Iterable[Receipt]` port, a four-positional `Receipt.of`, a parallel `factorint`/`gcd` entrypoint where the `NumberTheory` case folds them, a hand-rolled exact-factorization or lattice reduction where the `FLINT` ground domain owns it, a `Outcome(solution=...)` raw-keyword construction where the named factory reads, a bare-`ContentKey` assignment off the railed `ContentIdentity.of`, and per-artifact factories.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
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


# the exact-arithmetic axis: SYMPY runs the pure-Python `Poly`/`Matrix` kernel, FLINT lowers
# the carried polynomial/matrix to an `fmpz_poly`/`fmpq_mat` whose C-level factor/roots/det/
# charpoly/HNF own the fast exact ground domain — one row on the polynomial solve/linalg case,
# never a parallel FLINT solver surface. FLINT gates on the `python_version<'3.15'` band.
class GroundDomain(StrEnum):
    SYMPY = "sympy"
    FLINT = "flint"


# the numeric-evaluation precision axis: HEURISTIC lifts `N(expr, digits)` through the bundled
# `mpmath` context with no error bound, CERTIFIED re-evaluates the closed form through a
# `python-flint` `arb`/`acb` ball under `flint.good` carrying the certified `rad()` bound the
# heuristic path lacks. CERTIFIED gates on the `python_version<'3.15'` band.
class Precision(StrEnum):
    HEURISTIC = "heuristic"
    CERTIFIED = "certified"


class LowerBackend(StrEnum):
    NUMPY = "numpy"  # cp315-clean lambdify
    JAX = "jax"  # jaxlib python_version<'3.15' floor
    UFUNC = "ufunc"  # ufuncify broadcasting ufunc
    NATIVE = "native"  # autowrap compiled extension, host-toolchain gated


class CodeTarget(StrEnum):
    C = "c"
    CXX = "cxx"
    FORTRAN = "fortran"
    RUST = "rust"
    JULIA = "julia"
    OCTAVE = "octave"


# `MatrixForm` rows are cell spellings sympified against the live `SymbolSpec`; an `ExprForm`
# is the polymorphic `derive` input discriminated by shape, not parallel entries.
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
            self.assume.try_find(name)
            .map(lambda p: sym.Symbol(name, **{p.value: True}))
            .default_with(lambda: sym.Symbol(name))
            for name in self.names
        )


class SymbolicPayload(Struct, frozen=True, gc=False):
    # the canonical content-identity source the `canonical` IdentitySource modality folds
    # through the one cached deterministic encoder: the form repr, the sorted assumption pairs,
    # and the ordered op signature, so two derivations differing in assumption context, op
    # pipeline, or terminal route key distinctly. `gc=False` drops this container-free leaf
    # from the tracked GC set on the high-allocation derivation path.
    form: str
    assume: tuple[tuple[str, str], ...]
    ops: tuple[str, ...]

    @staticmethod
    def of(form: ExprForm, spec: SymbolSpec, ops: tuple["SymbolicOp", ...]) -> "SymbolicPayload":
        # `str` is its own spelling and a `MatrixForm` tuple renders deterministically through
        # `repr`; a constructed `Expr` MUST render through `srepr` (`Pow(Symbol('x'), Integer(2))`)
        # not `repr`/`str` (`x**2`) so assumption-carrying vs assumption-free symbols and exact vs
        # auto-simplified nodes key distinctly — the round-trippable canonical form `sympify` reads.
        return SymbolicPayload(
            form=_form_spelling(form),
            assume=tuple(sorted((name, pred.value) for name, pred in spec.assume.items())),
            ops=tuple(f"{op.tag}:{op.signature()}" for op in ops),
        )


@tagged_union(frozen=True)
class Outcome:
    # The terminal artifact is parameterized as tightly as the op vocabulary: one discriminated
    # case per artifact shape minted through a named factory, never one struct of default-zero
    # field groups. `facts()` is the total projection the receipt spreads, so each case names
    # only its slots; the `solution` `metric` and the `numeric` `radius` carry a real per-route
    # fact (discriminant/resultant/residual, certified ball radius) rather than a dead `0.0`.
    tag: Literal["staged", "solution", "spectrum", "arithmetic", "numeric", "callable_", "source"] = tag()
    staged: "Expr" = case()
    solution: tuple[SolveRoute, int, float] = case()
    spectrum: tuple[MatrixRoute, int, float] = case()
    arithmetic: tuple[NumberRoute, int, float] = case()
    numeric: tuple[int, float, float] = case()
    callable_: tuple[LowerBackend, int] = case()
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
    def Callable(backend: LowerBackend, arity: int) -> "Outcome":
        return Outcome(callable_=(backend, arity))

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
            case Outcome(tag="callable_", callable_=(backend, arity)):
                return {"backend": backend.value, "arity": arity}
            case Outcome(tag="source", source=(target, name, source)):
                return {"target": target.value, "name": name, "byte_count": len(source)}
            case Outcome(tag="staged"):
                # Unreachable: `derive` faults a staging terminal at the boundary before any
                # receipt mints, so a `staged` `Outcome` never reaches `facts()`. The arm keeps
                # the match total without a domain raise the rail already owns one layer up.
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
    number: NumberRoute = case()
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
    def Solve(route: SolveRoute = SolveRoute.SOLVE, domain: SolveDomain = SolveDomain.COMPLEXES, ground: GroundDomain = GroundDomain.SYMPY) -> "SymbolicOp":
        return SymbolicOp(solve=(route, domain, ground))

    @staticmethod
    def LinAlg(route: MatrixRoute = MatrixRoute.EIGENVALS, ground: GroundDomain = GroundDomain.SYMPY) -> "SymbolicOp":
        return SymbolicOp(linalg=(route, ground))

    @staticmethod
    def Number(route: NumberRoute = NumberRoute.FACTORINT) -> "SymbolicOp":
        return SymbolicOp(number=route)

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
        # the case-data spelling the `SymbolicPayload` canonical key folds, so a `Solve(SOLVE,
        # REALS, FLINT)` and a `Solve(SOLVE, COMPLEXES, SYMPY)` key distinctly; the substitution
        # map renders its sorted items rather than its identity so identical maps key identically.
        match self:
            case SymbolicOp(tag="calculus", calculus=(kind, order)):
                return f"{kind.value}/{order}"
            case SymbolicOp(tag="substitute", substitute=(mode, mapping)):
                return f"{mode.value}/{sorted(mapping.items())}"
            case SymbolicOp(tag="solve", solve=(route, domain, ground)):
                return f"{route.value}/{domain.value}/{ground.value}"
            case SymbolicOp(tag="linalg", linalg=(route, ground)):
                return f"{route.value}/{ground.value}"
            case SymbolicOp(tag="evaluate", evaluate=(digits, precision)):
                return f"{digits}/{precision.value}"
            case SymbolicOp(tag="codegen", codegen=(target, name)):
                return f"{target.value}/{name}"
            case _:
                # the single-field cases (rewrite/refine/number/lower) carry one enum value.
                return str(getattr(self, self.tag))

    def apply(self, sym: object, expr: "Expr", free: tuple["Expr", ...]) -> Outcome:
        primary, local = free[0], {s.name: s for s in free}
        match self:
            case SymbolicOp(tag="calculus", calculus=(kind, order)):
                return Outcome.Staged(_CALCULUS[kind](sym, expr, primary, order))
            case SymbolicOp(tag="rewrite", rewrite=pass_):
                args = (expr, primary) if pass_ is RewritePass.COLLECT else (expr,)
                return Outcome.Staged(getattr(sym, pass_.value)(*args))
            case SymbolicOp(tag="substitute", substitute=(SubstituteMode.SUBS, mapping)):
                return Outcome.Staged(expr.subs({sym.sympify(k, locals=local): sym.sympify(v, locals=local) for k, v in mapping.items()}))
            case SymbolicOp(tag="substitute", substitute=(SubstituteMode.REPLACE, mapping)):
                [(pattern, target)] = mapping.items()
                return Outcome.Staged(expr.replace(sym.Wild(pattern), sym.sympify(target, locals=local)))
            case SymbolicOp(tag="substitute", substitute=(SubstituteMode.REWRITE, mapping)):
                [(_, basis)] = mapping.items()
                return Outcome.Staged(expr.rewrite(getattr(sym, basis)))
            case SymbolicOp(tag="refine", refine=predicate):
                return Outcome.Staged(sym.refine(expr, getattr(sym.Q, predicate.value)(primary)))
            case SymbolicOp(tag="solve", solve=(route, domain, ground)):
                return _solve(sym, expr, free, route, domain, ground)
            case SymbolicOp(tag="linalg", linalg=(route, ground)):
                return _linalg(sym, expr, route, ground)
            case SymbolicOp(tag="number", number=route):
                return _number(sym, expr, route)
            case SymbolicOp(tag="evaluate", evaluate=(digits, precision)):
                return _evaluate(sym, expr, primary, digits, precision)
            case SymbolicOp(tag="lower", lower=backend):
                # Materialize the callable as the witness that lowering compiles; the receipt
                # carries `(backend, arity)` evidence, the live `fn` rides the cross-file
                # lowering seam since a callable is not encodable on the frozen receipt.
                _lower(sym, expr, free, backend)
                return Outcome.Callable(backend, len(free))
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

# Source-printer dispatch keyed by target: one polymorphic codegen surface selecting the
# per-language printer, never a parallel emitter per language.
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
    # `solve`/`solveset`/`nsolve` take an equation against the free symbols; `linsolve`/
    # `nonlinsolve` take a one-equation *system* `(expr,)`, so the carried scalar `expr` lifts
    # to a singleton system rather than passing as a bare expression the set solvers reject.
    # The polynomial routes carry a `metric` — the discriminant/resultant magnitude or the
    # `nsolve` residual — rather than a dead `0.0`, and on `GroundDomain.FLINT` lower to a
    # `fmpz_poly`/`fmpq_poly` whose C-level `factor`/`roots`/`resultant` own the exact arithmetic.
    primary = free[0]
    match route:
        case SolveRoute.SOLVE:
            return Outcome.Solution(route, _cardinality(sym.solve(expr, *free)))
        case SolveRoute.SOLVESET:
            return Outcome.Solution(route, _cardinality(sym.solveset(expr, primary, domain=getattr(sym, domain.value))))
        case SolveRoute.LINSOLVE | SolveRoute.NONLINSOLVE:
            return Outcome.Solution(route, _cardinality(getattr(sym, route.value)((expr,), *free)))
        case SolveRoute.NSOLVE:
            # one numeric root of the scalar equation in the primary unknown; the `metric` is the
            # substituted residual `|f(root)|` at the found root, the convergence witness `solve`
            # routes lack, keyed by the assumption-carrying `primary` Symbol object, never its name.
            root = sym.nsolve(expr, primary, 0.0)
            return Outcome.Solution(route, 1, abs(float(sym.N(expr.subs(primary, root)))))
        case SolveRoute.DSOLVE | SolveRoute.PDSOLVE:
            return Outcome.Solution(route, _cardinality(getattr(sym, route.value)(expr)))
        case SolveRoute.ROOTS:
            return Outcome.Solution(route, len(sym.roots(sym.Poly(expr, primary))))
        case _:
            return _poly_route(sym, expr, primary, route, ground)


def _poly_route(sym: object, expr: "Expr", primary: "Expr", route: SolveRoute, ground: GroundDomain) -> Outcome:
    # the polynomial sub-routes share one body keyed on `(route, ground)`: SYMPY reads
    # `Poly.real_roots`/`nroots`/`factor_list`/`discriminant`, FLINT lowers the integer-domain
    # `Poly` to an `fmpz_poly` whose `roots`/`complex_roots`/`factor` own the fast exact kernel.
    poly = sym.Poly(expr, primary)
    if ground is GroundDomain.FLINT:
        return _flint_poly(poly, route)
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


def _flint_poly(poly: object, route: SolveRoute) -> Outcome:
    from flint import fmpz_poly

    # the sympy integer `Poly` lowers to an `fmpz_poly` over its ascending coefficient vector;
    # the FLINT C kernel owns factorization, real/complex root isolation, and the resultant.
    fp = fmpz_poly([int(c) for c in reversed(poly.all_coeffs())])
    match route:
        case SolveRoute.REAL_ROOTS:
            roots = fp.complex_roots()
            return Outcome.Solution(route, sum(1 for r, _ in roots if abs(complex(r).imag) < 1e-12), 0.0)
        case SolveRoute.NROOTS:
            return Outcome.Solution(route, len(fp.complex_roots()), 0.0)
        case SolveRoute.FACTOR_LIST:
            _, factors = fp.factor()
            return Outcome.Solution(route, len(factors), 0.0)
        case _:  # RESULTANT against the formal derivative through the FLINT exact kernel
            return Outcome.Solution(route, fp.degree(), abs(float(fp.resultant(fp.derivative()))))


def _linalg(sym: object, expr: "Expr", route: MatrixRoute, ground: GroundDomain) -> Outcome:
    matrix = expr if hasattr(expr, "rref") else sym.Matrix(expr)
    if ground is GroundDomain.FLINT:
        return _flint_matrix(matrix, route)
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
            values = matrix.singular_values()
            return Outcome.Spectrum(route, len(values), abs(float(sym.N(values[0]))) if values else 0.0)
        case MatrixRoute.CHARPOLY | MatrixRoute.MINPOLY:
            poly = matrix.charpoly() if route is MatrixRoute.CHARPOLY else matrix.minimal_polynomial()
            return Outcome.Spectrum(route, poly.degree())
        case MatrixRoute.NULLSPACE:
            return Outcome.Spectrum(route, len(matrix.nullspace()))
        case MatrixRoute.INVERSE | MatrixRoute.PINV:
            inverse = matrix.inv() if route is MatrixRoute.INVERSE else matrix.pinv()
            return Outcome.Spectrum(route, inverse.shape[0], abs(float(matrix.det())) if route is MatrixRoute.INVERSE else 0.0)
        case _:  # LU/QR/cholesky/diagonalize/jordan_form — the decomposition leading dimension
            extracted = getattr(matrix, route.value)()
            head = extracted[0] if isinstance(extracted, tuple) else extracted
            return Outcome.Spectrum(route, head.shape[0])


def _flint_matrix(matrix: object, route: MatrixRoute) -> Outcome:
    from flint import fmpq_mat

    # the sympy `Matrix` lowers to an exact `fmpq_mat`; the FLINT C kernel owns the exact
    # determinant, rank, characteristic/minimal polynomial, inverse, and the HNF/SNF/LLL
    # lattice routes, the spectrum read off the certified exact arithmetic.
    fm = fmpq_mat([[_as_fmpq(matrix[i, j]) for j in range(matrix.shape[1])] for i in range(matrix.shape[0])])
    dimension = fm.nrows()
    match route:
        case MatrixRoute.DETERMINANT:
            return Outcome.Spectrum(route, dimension, abs(float(fm.det())))
        case MatrixRoute.RANK:
            return Outcome.Spectrum(route, fm.rank())
        case MatrixRoute.CHARPOLY | MatrixRoute.MINPOLY:
            return Outcome.Spectrum(route, (fm.charpoly() if route is MatrixRoute.CHARPOLY else fm.minpoly()).degree())
        case MatrixRoute.INVERSE:
            return Outcome.Spectrum(route, fm.inv().nrows(), abs(float(fm.det())))
        case _:  # RREF/NULLSPACE and the lattice routes fold to the leading dimension
            extracted = getattr(fm, route.value)()
            head = extracted[0] if isinstance(extracted, tuple) else extracted
            return Outcome.Spectrum(route, head.nrows())


def _number(sym: object, expr: "Expr", route: NumberRoute) -> Outcome:
    # the integer-domain terminal: a factorization names its distinct-prime count and
    # largest-prime magnitude, a prime test/range its cardinality, a gcd/lcm its scalar value.
    match route:
        case NumberRoute.FACTORINT:
            factors = sym.factorint(expr)
            return Outcome.Arithmetic(route, len(factors), float(max(factors, default=0)))
        case NumberRoute.PRIMERANGE:
            primes = tuple(sym.primerange(2, int(expr) + 1))
            return Outcome.Arithmetic(route, len(primes), float(primes[-1]) if primes else 0.0)
        case NumberRoute.ISPRIME:
            return Outcome.Arithmetic(route, int(bool(sym.isprime(expr))), float(expr))
        case _:  # GCD/LCM of the carried expression against its formal derivative — the
            # squarefree-part divisor algebra over the single folded `Expr`, never an
            # `expr.args` unpack that degenerates to `expr` on a leaf integer. The `cardinality`
            # is the divisor's polynomial degree (scalar divisor -> 1), the `magnitude` its
            # constant value when the divisor is a number, the lead coefficient otherwise.
            free = tuple(expr.free_symbols)
            value = getattr(sym, route.value)(expr, sym.diff(expr, free[0]) if free else expr)
            if value.is_number:
                return Outcome.Arithmetic(route, 1, abs(float(sym.N(value))))
            poly = sym.Poly(value, *free)
            return Outcome.Arithmetic(route, poly.degree(), abs(float(sym.N(poly.LC()))))


def _evaluate(sym: object, expr: "Expr", primary: "Expr", digits: int, precision: Precision) -> Outcome:
    # A numeric `expr` evaluates directly; a still-symbolic polynomial falls to its leading-
    # coefficient magnitude at the same precision. HEURISTIC reads the `mpmath` `N` decimal with
    # no bound; CERTIFIED re-evaluates through a `python-flint` `arb` ball under `flint.good`,
    # reading `mid()` as the value and `rad()` as the certified error the heuristic path lacks.
    scalar = expr if expr.is_number else sym.Poly(expr, primary).all_coeffs()[0]
    if precision is Precision.HEURISTIC:
        return Outcome.Numeric(digits, abs(float(sym.N(scalar, digits))))
    return _certified(scalar, digits)


def _certified(scalar: "Expr", digits: int) -> Outcome:
    import flint
    from flint import arb

    # `flint.good` re-runs the thunk at escalating `ctx.prec` until the ball pins `ctx.dps`
    # digits; the thunk re-renders the exact closed form to the live working precision through
    # sympy's own `mpmath`-backed `evalf` (so `sqrt(2)` lowers to a decimal `arb` parses, never
    # the unparseable `str(sqrt(2))` spelling) and lifts that decimal to a midpoint ball. The
    # certified `(mid, rad)` is the bound the heuristic `mpmath` `N` value lacks.
    flint.ctx.dps = digits
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
    # The printer family is the one polymorphic surface covering all six targets; for the
    # module-supported languages (C/Fortran/Octave/Julia/Rust) `codegen` wraps the printer
    # output in a named, signature-bearing module, while CXX renders bare via `cxxcode`.
    if target is CodeTarget.CXX:
        return _CODE_PRINTER[target](sym, expr)
    from sympy.utilities.codegen import codegen

    language = "c99" if target is CodeTarget.C else target.value
    [(_, source), *_] = codegen((name, expr), language=language, header=False, empty=False)
    return source


def _cardinality(solution: object) -> int:
    return len(tuple(solution)) if hasattr(solution, "__len__") else 1


def _as_fmpq(cell: object) -> object:
    from flint import fmpq

    # an exact sympy `Rational`/`Integer` cell lowers to an `fmpq` over its numerator/denominator
    # so the FLINT matrix carries exact rationals rather than a lossy float coerce.
    return fmpq(int(cell.p), int(cell.q)) if hasattr(cell, "q") else fmpq(int(cell))


# --- [COMPOSITION] ----------------------------------------------------------------------


class SymbolicDerivation:
    @staticmethod
    def derive(form: ExprForm, spec: SymbolSpec, *ops: SymbolicOp) -> "RuntimeRail[SymbolicReceipt]":
        # the derivation keys through the railed `ContentIdentity.of` over the canonical
        # `SymbolicPayload` and binds the resolved `content_key` into the `boundary` fence, the
        # admit-then-bind weave `numerics/array.md#PAYLOAD` holds, so a repeated derivation at
        # identical `(form, spec, ops)` is a cache hit and a canonical-encode fault rails first.
        return ContentIdentity.of("symbolic", SymbolicPayload.of(form, spec, ops), IdentityPolicy()).bind(
            lambda key: boundary(f"symbolic.{ops[-1].tag if ops else 'noop'}", lambda: _derive(form, spec, ops, key))
        )


def _derive(form: ExprForm, spec: SymbolSpec, ops: tuple[SymbolicOp, ...], key: ContentKey) -> SymbolicReceipt:
    # the subject reads a stable label, never `ops[-1].tag` at construction time: an empty
    # pipeline reads `ops[-1]` outside the thunk and escapes the rail as `IndexError`, so the
    # empty gate lives inside the fence where `boundary` converts it to a `BoundaryFault`.
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
    # the canonical-key spelling of the polymorphic input, the inverse of `_sympify_form`: a
    # `str` is its own spelling, a `MatrixForm` tuple renders deterministically through `repr`,
    # and a constructed `Expr` renders through `srepr` for round-trippable canonical bytes.
    match form:
        case str() as source:
            return source
        case tuple() as rows:
            return repr(rows)
        case _:
            from sympy import srepr

            return srepr(form)


def _sympify_form(sym: object, form: ExprForm, free: tuple["Expr", ...]) -> "Expr":
    # `local` binds every name to its assumption-carrying `Symbol` so the parsed expression
    # shares the exact symbol objects `free` holds; a plain `sympify` would mint distinct
    # assumption-free symbols that `diff`/`solveset` over `free[0]` would not recognize.
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

- [SYMPY_CORE]: `sympy` is pure-Python and cp315-clean; the `symbols`/`sympify`/`srepr`/`Symbol`/`Wild`/`lambdify`/`cse`/`refine`/`ask`/`Q` spellings, the calculus surface (`diff`/`integrate`/`limit`/`series`/`summation`), the rewrite surface (`simplify`/`factor`/`expand`/`collect`/`cancel`/`trigsimp`/`radsimp`/`ratsimp`/`apart`/`together`/`powsimp`/`logcombine`/`nsimplify`), the substitution surface (`Expr.subs`/`replace`/`rewrite`), the number-theory surface (`factorint`/`primerange`/`isprime`/`gcd`/`lcm`), and the `Expr.evalf`/`Poly.all_coeffs` members verify against the `compute/.api/sympy.md` `[ENTRYPOINT_SCOPE]` tables. The calculus rows force unevaluated `Derivative`/`Integral`/`Sum` nodes through `.doit()` and trim the `series` asymptotic `Order` term through `.removeO()`, both confirmed against the calculus-node table; `_CALCULUS` is the dispatch table folding the five node constructions into one row map. Every row except the `FLINT` `GroundDomain` accelerator, the `CERTIFIED` `Precision` row, and the `Lower(JAX)`/`Lower(NATIVE)` backends is live core on the cp315 beta.
- [GROUND_DOMAIN]: the `GroundDomain` axis is the `sympy`↔`python-flint` exact-arithmetic seam the `compute/.api/sympy.md` `[INTEGRATION_STACK]` and `compute/.api/python-flint.md` `[ARITHMETIC_STACKING]` `sympy ↔ flint` rows mandate — `sympy` owns the symbolic algebra, `python-flint` owns the fast exact ground-domain arithmetic, and the two meet only through the shared `SolveRoute`/`MatrixRoute` vocabulary. `GroundDomain.FLINT` lowers the carried integer `Poly` to an `fmpz_poly` over its ascending coefficient vector (the `Poly.all_coeffs` reversal) whose `factor`/`complex_roots`/`resultant`/`derivative` own the C-level factorization, root isolation, and resultant (`compute/.api/python-flint.md` `fmpz_poly`/`fmpq_poly` ENTRYPOINTS [01]-[03]), and the `Matrix` to an exact `fmpq_mat` (each cell an `fmpq` over its numerator/denominator) whose `det`/`rank`/`charpoly`/`minpoly`/`inv`/`rref`/`nullspace`/`hnf`/`snf`/`lll` own the exact linear-algebra and lattice routes (matrix ENTRYPOINTS [01]-[04]); `GroundDomain.SYMPY` keeps the pure-Python `Poly`/`Matrix` kernel. `python-flint` is the `python_version<'3.15'` companion band (LGPL Cython/C extension, no cp315 wheel), so the FLINT routes are authored against the documented API on the gated band while the sympy routes stay cp315-clean — one axis row, never a parallel FLINT solver surface.
- [SOLVE_DOMAIN]: the `Solve` route carries a `SolveDomain` (`Reals`/`Complexes`/`Integers`/`Naturals`) so `solveset(expr, primary, domain=...)` returns the set-valued solution over the declared domain rather than the unbounded `Complexes` default; `_solve` is a total `match` over the twelve-member `SolveRoute` closed by `assert_never`, dispatching each route to its catalogued call shape rather than a `getattr(sym, route.value)(expr, *free)` catch-all: `solve(f, *symbols)` and `solveset(f, symbol, domain=)` take the scalar equation against the free symbols, `linsolve`/`nonlinsolve` take a one-equation *system* so the carried `expr` lifts to the singleton `(expr,)` the set solvers require (catalog rows [01]-[04]), `dsolve`/`pdsolve` take the differential equation (rows [06]-[07]), and the polynomial routes (`REAL_ROOTS`/`NROOTS`/`FACTOR_LIST`/`RESULTANT`) read `Poly.real_roots`/`nroots`/`factor_list`/`discriminant`/`resultant` (rows [05]-[06] of the numeric-evaluation table) for isolated roots, root count, factor structure, and the squarefree resultant against the formal derivative. The `solution` `metric` slot carries a real per-route fact — the discriminant magnitude for the root/factor routes, the resultant magnitude for `RESULTANT`, the substituted-residual magnitude for `nsolve` — replacing the prior dead `discriminant=0.0` the `solution` slot carried on every non-polynomial route.
- [MATRIX_ALGEBRA]: the `LinAlg` terminal owns the matrix `[ENTRYPOINT_SCOPE]` surface — `Matrix.eigenvals`/`eigenvects`/`det`/`charpoly`/`minimal_polynomial`/`rref`/`nullspace`/`rank`/`inv`/`pinv`/`LUdecomposition`/`QRdecomposition`/`cholesky`/`diagonalize`/`jordan_form`/`singular_values` over a `MatrixSymbol`-backed dense `Matrix` (catalog matrix rows [03]-[10]) — extracting the exact spectral radius (max `abs(N(eigenvalue))`), determinant magnitude, characteristic/minimal-polynomial degree, leading singular value, null-space dimension, inverse/pseudo-inverse dimension, or decomposition leading dimension into one `Outcome.spectrum`. The `eigenvects` route reads the `(value, multiplicity, basis)` triples folding the value→multiplicity map the eigenvalue radius reads; `minpoly` reads `Matrix.minimal_polynomial`. The `MatrixForm` cell spellings `sympify` against the live `SymbolSpec` so a symbolic matrix carries the same free variables the calculus and solve rows do, and the `FLINT` ground domain lowers the cells to exact `fmpq` entries so the exact spectrum reads off certified arithmetic rather than a float coerce.
- [NUMBER_THEORY]: the `NumberTheory` terminal owns the integer-domain `[ENTRYPOINT_SCOPE]` surface (`compute/.api/sympy.md` assumption/number-theory rows [05]-[06]) — `factorint(n)` the prime-factor multiset, `primerange(a, b)` the prime generator, `isprime(n)` the primality test, and `gcd`/`lcm` the polynomial/integer divisor algebra — folding each into one `Outcome.arithmetic` carrying `(route, cardinality, magnitude)` so a factorization names its distinct-prime count and largest-prime magnitude rather than overloading the `solution` slots, the `python-flint` `fmpz.factor`/`is_prime`/`gcd`/`lcm` (`compute/.api/python-flint.md` `fmpz` ENTRYPOINTS [02]-[04]) the exact accelerator beside it on the companion band reached through the same vocabulary when the integer is hot.
- [NUMERIC_BRIDGE]: the `Evaluate` row carries a `Precision` axis spanning the heuristic and certified bridges. `HEURISTIC` is the `.api` `[GRADUATION_PATH]` numeric bridge through the bundled `mpmath` context — `N(expr, digits)` lifts a numeric expression to a fixed-precision decimal, and on a still-symbolic polynomial `Poly(expr, primary).all_coeffs()[0]` reads the leading coefficient whose magnitude evaluates at the same precision — with `radius=0.0` because `mpmath` carries no error bound. `CERTIFIED` is the `compute/.api/python-flint.md` `[ARITHMETIC_STACKING]` `mpmath ↔ flint` promotion: the exact scalar re-evaluates through a `python-flint` `arb` ball under the adaptive `flint.good(func)` driver (`compute/.api/python-flint.md` certified-evaluation ENTRYPOINTS [01]) that re-runs the thunk at rising working precision until the ball is accurate to the `flint.ctx.dps` target the row sets; the thunk lowers the exact closed form to a parseable decimal through sympy's own `Expr.evalf(ctx.dps + 2)` before `arb(str(...))` (so `sqrt(2)` lifts as a decimal midpoint rather than the unparseable `str(sqrt(2))` symbol spelling), reading `mid()` as the value and `rad()` as the certified error bound the heuristic path lacks — so the precision claim and the certified bound enter the same `SymbolicReceipt` the codegen and lower rows feed, rather than a separate evaluation surface. `cse=True` precedes every `lambdify` lowering to dedupe shared subexpressions as the `.api` law requires.
- [LOWER_BACKEND]: `LowerBackend` collapses the lowering family into one terminal — `lambdify(free, expr, modules="numpy"|"jax", cse=True)` for the vectorized/differentiable callable (catalog row [03][12]), `utilities.autowrap.ufuncify(free, expr)` for the broadcasting numpy ufunc (row [11]), and `utilities.autowrap.autowrap(expr, args=free)` for the compiled native extension (row [10]). The `Lower(LowerBackend.JAX)` value bridges this owner and the JAX autodiff family in `solvers/sensitivity.md#SENSITIVITY`, where `jax.grad`/`jax.vjp` consume the callable; the live callable is materialized as the lowering witness but not stored on the frozen `SymbolicReceipt` (a callable is not `msgspec`-encodable), so the consumer retrieves the live `fn` through the lowering seam keyed by the derivation's `content_key`. The `jax` value gates on the jaxlib `python_version<'3.15'` floor and the `native` value on a host C/Fortran toolchain, while `numpy`/`ufunc` stay cp315-clean because only the `jax` value passes `modules="jax"` and a `numpy`/`ufunc` lowering never imports `jax`.
- [CODEGEN_PRINTER]: `Codegen` is the single polymorphic source-emission surface the `[GRADUATION_PATH]` law mandates — `_CODE_PRINTER` keys the per-target printer (`ccode`/`cxxcode`/`fcode`/`rust_code`/`julia_code`/`octave_code`, catalog rows [07][08]) by `CodeTarget`, and `utilities.codegen.codegen(language=...)` (row [09]) wraps a named, header-stripped module where the target supports it, falling to the printer for a bare expression render. A new code target is one `CodeTarget` value plus one `_CODE_PRINTER` row, never a parallel emitter. `cse` precedes codegen to dedupe shared subexpressions, and the emitted source becomes the `Rasm.Compute` C# numeric-owner graduation candidate the symbolic `HandoffAxis` reads.
- [CONTENT_IDENTITY]: `SymbolicDerivation.derive` keys the derivation through the railed `ContentIdentity.of("symbolic", SymbolicPayload.of(form, spec, ops), IdentityPolicy())` from `runtime/evidence/identity#IDENTITY` and `bind`s the resolved `content_key` into the `boundary` fence — the `ContentIdentity.of(fmt, source, policy, *, view="value") -> RuntimeRail[ContentKey]` contract and the admit-then-bind weave `numerics/array.md#PAYLOAD` holds and the sibling `experiments/inference.md#BAYESIAN`/`experiments/model.md#ASSET` thread, so a repeated derivation at identical `(form, spec, ops)` is a cache hit by reference. `SymbolicPayload` is the canonical `msgspec.Struct(frozen=True, gc=False)` the `canonical` `IdentitySource` modality folds through the one cached deterministic encoder (the form repr, the sorted assumption pairs, the ordered op signature), so the runtime content owner mints the key rather than a hand-rolled `msgspec.json.encode` plus `b"\x00".join` reinvention. `SymbolicReceipt` carries the resolved `content_key` and renders it through `ContentKey.project("hex")` (the `{value:032x}:{fmt}` form the C# `InterchangeIdentity.Key` contract reads) on every `contribute` row beside the `op`/`symbols` facts — the prior receipt carrying no content identity is the deleted form. A bare-`ContentKey` assignment off the railed `ContentIdentity.of` is the deleted form the in-fence `bind` replaces.
- [RECEIPT_SHAPE]: `SymbolicReceipt.contribute` yields the `Iterable[Receipt]` the `runtime/observability/receipts#RECEIPT` `ReceiptContributor` Protocol declares (`contribute(self) -> Iterable[Receipt]`), and the row is `Receipt.of("compute.symbolic", ("emitted", op_tag, facts))` — the two-argument shape-polymorphic factory over the `(Phase, subject, facts)` `Evidence` triple, never the four-positional `Receipt.of("emitted", owner, subject, facts)` the factory does not admit, and never a single bare `Receipt` against the `Iterable[Receipt]` port, the same `yield`-one-row form `analysis/spatial.md#SPATIAL` and `analysis/transform.md#TRANSFORM` hold. `Outcome` parameterizes the terminal artifact and owns its own `facts()` projection, so the receipt spreads only the slots the matched case carries rather than a multi-field default-zero struct. `derive` is the `RuntimeRail[SymbolicReceipt]` boundary owner (the error arm carries no contributor), so emission is not an `@receipted` decorator on `derive` but the study spine harvesting the resolved `SymbolicReceipt` contributor through the `@receipted` aspect on the `Ok` arm — the same convention `analysis/signal.md#DSP` and `analysis/spatial.md#SPATIAL` hold.
