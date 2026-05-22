# [H1][MATHNET_GH2_NUMERICS_BOUNDARIES]
>**Dictum:** *GH2 sees stable Rasm outputs, not MathNet implementation objects.*

<br>

[IMPORTANT] MathNet APIs are a `Rasm` implementation detail. GH2 ports should expose Rhino/GH-native data, scalars, Rasm records, or tree/list structures already owned by the boundary.

---
## [1][OUTPUT_POLICY]
>**Dictum:** *Numerical output shape must be predictable in Grasshopper trees.*

<br>

| [INDEX] | [NUMERICAL_RESULT] | [GH2_OUTPUT] | [RULE] |
| :-----: | ------------------ | ------------ | ------ |
| [1] | Scalar | GH number or Rasm scalar value object. | Validate finite value before output. |
| [2] | Vector length 3 | Rhino `Vector3d` or `Point3d` by semantic role. | Do not output raw MathNet vector. |
| [3] | Matrix | Rasm `Matrix`, row records, or explicit tree rows. | Preserve row order and dimensions. |
| [4] | Eigenpairs | Tree of result records with eigenvalue, eigenvector, residual, and source index. | Sort deterministically and normalize vector sign/phase when identity matters. |
| [5] | Statistics | Named Rasm statistic records. | Preserve estimator semantics in output names. |
| [6] | Symbolic expression | Rasm expression record or formatted string payload. | Do not expose raw `SymbolicExpression` as GH2 contract. |

---
## [2][DETERMINISM]
>**Dictum:** *Parametric graphs must recompute reproducibly.*

<br>

| [INDEX] | [SOURCE] | [POLICY] |
| :-----: | -------- | -------- |
| [1] | `MathNet.Numerics.Random` | Require explicit seed for graph-visible randomness. |
| [2] | Distributions | Treat distribution parameters as typed input; sample only through seeded source. |
| [3] | Iterative solvers | Expose tolerance and iteration count through Rasm operation policy. |
| [4] | Eigenvectors | Normalize sign/phase when output identity matters. |
| [5] | Floating reductions | Use deterministic ordered folds for exposed results. |
| [6] | Symbolic parse/format | Keep parser culture and format style explicit for stable graph recomputation. |
| [7] | Fourier transforms | Fix `FourierOptions`, scaling, and frequency ordering in operation policy. |
| [8] | ODE solvers | Expose fixed step size, step count, and integration interval. |
| [9] | Optimizers | Emit `ExitCondition`, residual, iterations, and tolerance policy. |
| [10] | MCMC and distributions | Validate distribution parameters and expose burn/sample counts. |

---
## [3][FAILURE_MESSAGES]
>**Dictum:** *Solver failures become GH diagnostics through existing rails.*

<br>

| [INDEX] | [FAILURE] | [RASM_RAIL] | [GH2_BEHAVIOR] |
| :-----: | --------- | ----------- | -------------- |
| [1] | Invalid dimensions | `Op.InvalidInput` | Component reports input shape error. |
| [2] | Non-convergence | `Op.InvalidResult` or typed caution when added. | Component reports solver failure with operation name. |
| [3] | Non-finite numeric value | `Op.InvalidInput` or `Op.InvalidResult` by boundary. | Component rejects value before output. |
| [4] | Unsupported output type | `Op.Unsupported` | Port/output shape remains explicit. |
| [5] | Missing context | `Op.MissingContext` | Component reports missing Rhino model context. |
| [6] | Symbolic parse error | Future expression parse fault or `Op.InvalidInput` | Component reports invalid expression text. |

---
## [4][BOUNDARY_RULES]
>**Dictum:** *MathNet stays behind the component contract.*

<br>

- Keep MathNet package references in `Rasm`, not in GH2 app plugins.
- Keep GH2 project references explicit and thin.
- Do not add MathNet global usings to `Rasm.Grasshopper` unless a GH2 boundary file directly owns that API.
- Convert MathNet values into Rhino or Rasm values before `Output` projection.
- Route all failures through `Fin<T>` and existing GH2 diagnostic collapse.
- Keep Symbolics behind Rasm expression ownership; GH2 may render infix, LaTeX, MathML, or evaluated numeric output by explicit port type.

---
## [5][DATA_ACCESS_TREE_POLICY]
>**Dictum:** *GH2 tree/list output is a native data-access contract, not an array dump.*

<br>

| [INDEX] | [ACCESS] | [READ_RULE] | [WRITE_RULE] |
| :-----: | -------- | ----------- | ------------ |
| [1] | `Access.Item` | Read via `GetPear`/`GetPears` when null or metadata matters. | Write exactly one value with `SetPear`; report error for multi-value numerical output. |
| [2] | `Access.Twig` | Read via `GetTwig` and preserve pear metadata. | Write with `Garden.TwigFromPears` and stable item order. |
| [3] | `Access.Tree` | Read via `GetTree` and preserve `Site` path/item identity. | Write with `Garden.TreeFromLeaves` when source sites exist; apply `WithPathPrefix` from `CoverageOut`. |
| [4] | Matrix rows | Treat each row as a twig or result record, not a raw MathNet matrix. | Preserve row index, column index, and dimensions in output shape. |
| [5] | Solver diagnostics | Emit convergence status, residual, iterations, and warnings as Rasm records or GH remarks. | Do not hide non-convergence behind empty trees. |
