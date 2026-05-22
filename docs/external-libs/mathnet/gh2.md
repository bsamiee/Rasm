# [H1][MATHNET_GH2]
>**Dictum:** *GH2 owns data access, trees, coverage, diagnostics, and user-visible numerics.*

<br>

[IMPORTANT] MathNet may compute values for GH2 components; it does not replace GH2 data model semantics.

---
## [1][DATA_ACCESS]
>**Dictum:** *`IDataAccess` is the component boundary.*

<br>

Use local GH2 XML/decompile to verify `GetPear`, `GetTwig`, `GetTree`, `SetPear`, `SetTwig`, `SetTree`, `CoverageIn`, `CoverageOut`, diagnostics, progress, tolerance, unit, transform, quaternion, null, and meta access. Keep reads and writes inside the GH2 boundary rail.

---
## [2][TREES]
>**Dictum:** *Tree paths are native structure, not list decoration.*

<br>

`CoverageOut(index)` returns coverage. Derive output prefix from `CoverageOut(slot).TwigIndex`, then apply `WithPathPrefix` on the tree before writing. Use GH2 `Garden`, `Tree`, `Twig`, `Pear`, `Leaf`, `Site`, `Path`, and `Coverage` as the native tree substrate. Do not hand-build path logic beside GH2 owners.

---
## [3][NUMERICS]
>**Dictum:** *GH2 numeric inputs carry host policy.*

<br>

Respect GH2 tolerance, unit system, scaling, numeric filters, and parameter access (`Item`, `Twig`, `Tree`). Validate formula text, scalar values, matrices, and result shapes before MathNet execution. Report parse, arity, convergence, unsupported result, and non-finite failures through GH2 diagnostics.

---
## [4][OUTPUT]
>**Dictum:** *User-visible output preserves computation evidence.*

<br>

- Include operation name and input nickname in diagnostics.
- Preserve convergence/status/tolerance/residual when algorithms expose them.
- Keep symbolic objects private unless output contract explicitly asks for infix, LaTeX, MathML, or formula text.
- Never expose MathNet matrices/vectors directly as public GH2 data identity without a Rasm-owned shape.
