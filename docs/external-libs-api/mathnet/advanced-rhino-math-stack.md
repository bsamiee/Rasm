# [H1][MATHNET_ADVANCED_RHINO_MATH_STACK]
>**Dictum:** *Rasm composes symbolic, numerical, and Rhino geometry kernels directly.*

<br>

[IMPORTANT] Rasm targets Rhino 9/WIP, GH2, and `net10.0`. Math features use direct MathNet, RhinoCommon, LanguageExt, and Thinktecture APIs. External libraries own hard algorithms.

[IMPORTANT] Baseline: MathNet packages are centrally pinned. RhinoCommon, GH2, and Eto assemblies come from RhinoWIP. BCL tensors and hashing stay out of the graph until first consumer.

---
## [1][STACK]
>**Dictum:** *Each library owns a different level of mathematical truth.*

<br>

| [INDEX] | [OWNER] | [ROLE] | [RASM_USE] |
| :-----: | ------- | ------ | -------- |
| [1] | RhinoCommon | Geometry, tolerances, transforms, model units, validity. | Admit and emit spatial values. |
| [2] | MathNet.Numerics | Numeric execution, linear algebra, optimization, stats, integration, interpolation, distributions. | Execute dense/sparse/scalar algorithms. |
| [3] | MathNet.Symbolics | Exact expression algebra, parsing, formatting, calculus, transforms. | Own formulas before numerical sampling. |
| [4] | LanguageExt | `Fin<T>`, `Validation`, `Eff`, immutable collections, effect-polymorphic composition. | Collapse failure and execution rails. |
| [5] | Thinktecture | Value objects, smart enums, unions, generated dispatch. | Encode bounded vocabularies and operation modes. |

---
## [2][FEATURE_COMPOSITION]
>**Dictum:** *The strongest features stack symbolic intent with numeric execution.*

<br>

| [INDEX] | [FEATURE] | [SYMBOLIC_LAYER] | [NUMERIC_LAYER] | [RHINO_GH2_LAYER] |
| :-----: | --------- | ---------------- | --------------- | ----------------- |
| [1] | Formula fields | Parse and differentiate scalar expression. | Compile/evaluate over sample grids, integrate, optimize. | Emit contours, vectors, colors, and GH2 trees. |
| [2] | Analytic frames | Differentiate expression and collect variables. | Normalize gradients, fit local Hessians, eigensolve curvature. | Emit `Plane`, `Vector3d`, and preview geometry. |
| [3] | Registration | Symbolic objective names and constraints. | SVD, least squares, BFGS, Levenberg-Marquardt. | Emit `Transform` and residual diagnostics. |
| [4] | Mesh spectra | Symbolic operator labels. | Sparse storage, matvec, Rasm eigen kernels, dense projected solves. | Emit spectral descriptors and GH2 branch data. |
| [5] | Stochastic design | Symbolic parameter domains. | Distributions, seeded random sources, MCMC, kernel density. | Emit reproducible parameter sets and previews. |
| [6] | Simulation traces | Symbolic vector fields. | ODE solvers, interpolation, integration, root finding. | Emit curves, frames, events, and state trees. |

---
## [3][RASM_CONCEPT_BOUNDARIES]
>**Dictum:** *Rasm types encode intent; MathNet types execute algorithms.*

<br>

| [INDEX] | [TYPE] | [BACKING] | [RULE] |
| :-----: | ------ | --------- | ------ |
| [1] | Formula values | `SymbolicExpression` | Store expression intent and canonical text; expose transform/evaluate operations. |
| [2] | Symbol names | String value object | Bound accepted variables and prevent empty symbols. |
| [3] | Numeric kernels | Smart enum or union | Select MathNet family: factorization, minimizer, interpolation, integration, distribution, transform, derivative, or ODE stepper. |
| [4] | `Matrix` / `SparseMatrix` | MathNet matrices internally | Keep public storage Rasm-owned and execution MathNet-owned. |
| [5] | Fields | Formula plus compiled evaluator or sampled grid | Let exact formulas and sampled values coexist under one operation vocabulary. |
| [6] | Solver results | MathNet result plus Rasm diagnostics | Preserve convergence, residuals, iterations, and output values. |

---
## [4][IMPLEMENTATION_RULES]
>**Dictum:** *Library power deletes local ceremony.*

<br>

- Use `SymbolicExpression` for C# formula work; use lower-level `Expression` only for structural inspection.
- Use MathNet builders, factorization objects, minimizers, distributions, and interpolation types directly inside Rasm kernels.
- Use Numerics 6 managed providers by default; native providers require macOS RhinoWIP load proof and BenchmarkDotNet evidence.
- Use Thinktecture generated dispatch for solver modes, transform modes, distributions, and output shapes.
- Use `Fin<T>` and `Validation` rails at parse, admission, solver, evaluation, and Rhino conversion boundaries.
- Use `Eff<RT,T>` when formulas or numeric kernels need model context, cancellation, progress, or runtime services.
- Collapse single-use adapters into owning operations; keep MathNet implementation details private to Rasm concern files.

---
## [5][OUTPUT_POLICY]
>**Dictum:** *GH2 receives typed results, not library implementation artifacts.*

<br>

| [INDEX] | [RESULT] | [OUTPUT] |
| :-----: | -------- | -------- |
| [1] | Numeric scalar | GH number or Rasm scalar value. |
| [2] | Formula | Rasm formula value plus optional infix, LaTeX, or MathML text. |
| [3] | Matrix/vector | Rasm value, Rhino vector/point, or deterministic tree rows. |
| [4] | Solver result | Result record with residual, iterations, status, and values. |
| [5] | Geometry projection | RhinoCommon geometry validated by Rhino tolerance and units. |
