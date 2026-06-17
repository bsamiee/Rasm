# [PY_COMPUTE_DERIVATION]

The one classical computer-algebra derivation and code-generation owner. `SymbolicDerivation` lambdifies a sympy expression to a numpy-callable closure and generates the C source the graduation gate consumes as the kernel-handoff artifact. This is the one ungated solver route because sympy is pure-Python and imports on cp315; symbolic differentiation and algebra are in-scope, and no learned or generative symbolic search enters this owner. The derivation graduates outward on the symbolic `HandoffAxis` case.

## [1]-[INDEX]

[DERIVATION]: sympy lambdify and codegen producing the numpy/C handoff artifact on one `SymbolicDerivation` owner.

## [2]-[DERIVATION]

- Owner: `SymbolicDerivation` — the ungated `sympy.lambdify` plus `sympy.utilities.codegen.codegen` surface producing the C# handoff artifact; `callable_` is the numpy-callable closure and `c_source` is the C emission the C# graduation gate consumes. `SymbolicOp` discriminates `Lambdify(expr, symbols)` (the numpy-callable derivation) and `Codegen(expr, name)` (the C-source emission); the two share one derivation and emit one receipt.
- Entry: `SymbolicDerivation.of` builds the free symbols with `sympy.symbols`, lambdifies the expression over the numpy module, and runs `codegen` for the C source; `derive` returns `RuntimeRail[SymbolicDerivation]`. The graduation gate reads the C source as the kernel-handoff artifact through the symbolic `HandoffAxis` case.
- Receipt: `SymbolicDerivation.contribute` emits one `Receipt.Emitted` row carrying the symbol list and the C-source byte count; the derivation graduates outward through `graduation/receipt.md#GRADUATION` on the symbolic axis once the derivation is stable and reproducible.
- Packages: `sympy` (`symbols`, `lambdify`, `utilities.codegen.codegen`, `diff`, `simplify`), `numpy` (the lambdify target module), runtime (`RuntimeRail`, `boundary`, `ReceiptContributor`).
- Growth: a new code target is one `Codegen` language argument; a new derivation op is one `SymbolicOp` case; zero new surface.
- Boundary: classical CAS only — symbolic differentiation, simplification, and codegen are in-scope; no learned or generative symbolic search. `sympy` is pure-Python and imports on cp315, so this is the one ungated solver route, reflected rather than deploy-gated.

```python signature
from collections.abc import Callable
from typing import Literal

from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.observability.receipts import Receipt
from rasm.runtime.faults import RuntimeRail, boundary


# --- [TYPES] -------------------------------------------------------------------------------
@tagged_union(frozen=True)
class SymbolicOp:
    tag: Literal["lambdify", "codegen"] = tag()
    lambdify: tuple[object, tuple[str, ...]] = case()
    codegen: tuple[object, str] = case()

    @staticmethod
    def Lambdify(expr: object, symbols: tuple[str, ...]) -> "SymbolicOp":
        return SymbolicOp(lambdify=(expr, symbols))

    @staticmethod
    def Codegen(expr: object, name: str) -> "SymbolicOp":
        return SymbolicOp(codegen=(expr, name))


# --- [MODELS] ------------------------------------------------------------------------------
class SymbolicDerivation(Struct, frozen=True):
    symbols: tuple[str, ...]
    callable_: Callable[..., float]
    c_source: str

    @staticmethod
    def of(expr: object, symbols: tuple[str, ...], *, name: str = "kernel") -> "SymbolicDerivation":
        import sympy
        from sympy.utilities.codegen import codegen

        free = sympy.symbols(symbols)
        fn = sympy.lambdify(free, expr, modules="numpy")
        ([(_, source), *_]) = codegen((name, expr), language="C", header=False, empty=False)
        return SymbolicDerivation(symbols, fn, source)

    def contribute(self) -> Receipt:  # ReceiptContributor
        facts = {"symbols": ",".join(self.symbols), "c_bytes": str(len(self.c_source))}
        return Receipt.Emitted("compute.symbolic", "derivation", facts)


def derive(expr: object, symbols: tuple[str, ...], *, name: str = "kernel") -> "RuntimeRail[SymbolicDerivation]":
    return boundary("symbolic.derive", lambda: SymbolicDerivation.of(expr, symbols, name=name))
```

## [3]-[RESEARCH]

- [SYMPY_CODEGEN]: `sympy` is pure-Python and cp315-clean; the `symbols`/`lambdify`/`utilities.codegen.codegen`/`diff`/`simplify` spellings are reflected and verify against the branch `.api` catalogue. This is the one ungated solver route.
- [JAX_LAMBDIFY]: `sympy.lambdify(modules="jax")` emits a differentiable JAX callable, a bridge between this owner and the JAX autodiff family in `differentiation/sensitivity.md#SENSITIVITY`; the bridge gates on the jaxlib `python_version<'3.15'` floor and verifies against the branch `.api` catalogue once jaxlib resolves.
