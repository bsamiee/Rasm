# [PY_COMPUTE_SYMBOLIC]

The one classical computer-algebra derivation and code-generation owner. `SymbolicDerivation` lambdifies a sympy expression to a numpy-callable closure and generates the C source the graduation gate consumes as the kernel-handoff artifact. This is the one ungated solver route because sympy is pure-Python and imports on cp315; symbolic differentiation and algebra are in-scope, and no learned or generative symbolic search enters this owner. The derivation graduates outward on the symbolic `HandoffAxis` case.

## [01]-[INDEX]

- [01]-[DERIVATION]: one sympy lambdify-and-codegen derivation producing the numpy callable and the C handoff artifact on one `SymbolicDerivation` owner.

## [02]-[DERIVATION]

- Owner: `SymbolicDerivation` — the ungated `sympy.lambdify` plus `sympy.utilities.codegen.codegen` surface producing the C# handoff artifact; `callable_` is the numpy-callable closure and `c_source` is the C emission the C# graduation gate consumes. One derivation of an expression yields both artifacts from one common-subexpression pass, so the numpy study callable and the C kernel handoff share one lowering rather than two parallel entries.
- Entry: `SymbolicDerivation.of` builds the free symbols with `sympy.symbols`, lambdifies the expression over the numpy module under `cse=True`, and runs `codegen` for the C source; `derive` returns `RuntimeRail[SymbolicDerivation]`. The graduation gate reads the C source as the kernel-handoff artifact through the symbolic `HandoffAxis` case.
- Receipt: `SymbolicDerivation.contribute` emits one `Receipt.of("emitted", ...)` row carrying the symbol list and the C-source byte count; the derivation graduates outward through `graduation/handoff.md#GRADUATION` on the symbolic axis once the derivation is stable and reproducible.
- Packages: `sympy` (`symbols`, `lambdify`, `utilities.codegen.codegen`, `diff`, `simplify`, `cse`), `numpy` (the lambdify target module), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor`).
- Growth: a new code target is one `language` argument on `of`; a new calculus transform composes `sympy.diff`/`simplify` on the expression before lowering; zero new surface, never a parallel per-artifact entry beside the one derivation.
- Boundary: classical CAS only — symbolic differentiation, simplification, and codegen are in-scope; no learned or generative symbolic search. `sympy` is pure-Python and imports on cp315, so this is the one ungated solver route, reflected rather than deploy-gated.

```python signature
from collections.abc import Callable

from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


class SymbolicDerivation(Struct, frozen=True):
    symbols: tuple[str, ...]
    callable_: Callable[..., float]
    c_source: str

    @staticmethod
    def of(expr: object, symbols: tuple[str, ...], *, name: str = "kernel", language: str = "C") -> "SymbolicDerivation":
        import sympy
        from sympy.utilities.codegen import codegen

        free = sympy.symbols(symbols)
        fn = sympy.lambdify(free, expr, modules="numpy", cse=True)
        ([(_, source), *_]) = codegen((name, expr), language=language, header=False, empty=False)
        return SymbolicDerivation(symbols, fn, source)

    def contribute(self) -> Receipt:
        facts = {"symbols": ",".join(self.symbols), "c_bytes": str(len(self.c_source))}
        return Receipt.of("emitted", "compute.symbolic", "derivation", facts)


def derive(expr: object, symbols: tuple[str, ...], *, name: str = "kernel", language: str = "C") -> "RuntimeRail[SymbolicDerivation]":
    return boundary("symbolic.derive", lambda: SymbolicDerivation.of(expr, symbols, name=name, language=language))
```

## [03]-[RESEARCH]

- [SYMPY_CODEGEN]: `sympy` is pure-Python and cp315-clean; the `symbols`/`lambdify`/`utilities.codegen.codegen`/`cse`/`diff`/`simplify` spellings are reflected and verify against the `.api` catalogue. This is the one ungated solver route.
- [JAX_LAMBDIFY]: `sympy.lambdify(modules="jax")` emits a differentiable JAX callable, a bridge between this owner and the JAX autodiff family in `solvers/sensitivity.md#SENSITIVITY`; the bridge gates on the jaxlib `python_version<'3.15'` floor and verifies against the `.api` catalogue once jaxlib resolves.
