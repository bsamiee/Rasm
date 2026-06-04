# [H1][MATHNET_API]

[IMPORTANT] Rasm directly consumes `MathNet.Numerics` and `MathNet.Symbolics` at the versions pinned in `Directory.Packages.props`. Treat supporting MathNet/F# package closure as load-context evidence, not an API surface for Rasm C# guidance.

## [1][SOURCE_TRUTH]

| [INDEX] | [SOURCE]                                                           | [USE]                                               |
| :-----: | ------------------------------------------------------------------ | --------------------------------------------------- |
|   [1]   | `Directory.Packages.props`                                         | Central package truth.                              |
|   [2]   | `uv run python -m tools.quality api resolve MathNet.Numerics all`  | Numerics API asset truth.                           |
|   [3]   | `uv run python -m tools.quality api resolve MathNet.Symbolics all` | Symbolics API asset truth.                          |
|   [4]   | `CSparse` / `CSparse.xml`                                          | Sparse direct factorization — detail in `sparse.md` |
|   [5]   | Package nuspec/DLL reflection                                      | Dependency and public-type proof.                   |
|   [6]   | Public MathNet docs                                                | Topic map only; verify signatures locally.          |

## [2][OWNERSHIP]

| [INDEX] | [SURFACE]                        | [OWNS]                                                 | [NOT_OWN]                                 |
| :-----: | -------------------------------- | ------------------------------------------------------ | ----------------------------------------- |
|   [1]   | `MathNet.Numerics.LinearAlgebra` | Matrices, vectors, decompositions, solvers.            | Rhino geometry identity.                  |
|   [2]   | `MathNet.Numerics.Optimization`  | Minimizers, objective functions, convergence.          | Domain failure rails.                     |
|   [3]   | `MathNet.Numerics.Integration`   | Numerical integration rules.                           | Rhino curve/surface semantic integration. |
|   [4]   | `MathNet.Numerics.Interpolation` | Interpolation schemes.                                 | GH2 tree/path structure.                  |
|   [5]   | `MathNet.Numerics.Statistics`    | Estimators and distributions.                          | Rasm result vocabulary.                   |
|   [6]   | `MathNet.Symbolics`              | Formula parse, transform, calculus, evaluate, compile. | Runtime load safety.                      |

**Adoption in production `libs/`:** Numerics via `Vectors/Matrix.cs`, `Cloud.cs`, and `Mesh.cs` (CSparse hybrid); detail in `rasm.md`. Symbolics, Optimization, Integration, and Interpolation are pinned but **not referenced** in production `.cs` files.

Host geometry and GH2 data-access boundaries: `../../usage.md` §1, local RhinoWIP/GH2 XML, and nested host `AGENTS.md`.

## [3][CONSTRUCTION]

Use `Matrix<T>.Build`, `Vector<T>.Build`, `MatrixBuilder<T>`, `VectorBuilder<T>`, `CreateMatrix`, and `CreateVector`. Do not document non-public builder implementation types. MathNet storage and indexers stay inside algorithm execution; public Rasm outputs carry dimensions, ordering, convergence, tolerance, and diagnostics.

## [4][CONTROL]

Verified control surfaces include `MathNet.Numerics.Control`, `MathNet.Numerics.Providers.LinearAlgebra.LinearAlgebraControl`, `MathNet.Numerics.Providers.FourierTransform.FourierTransformControl`, and `MathNet.Numerics.Providers.SparseSolver.SparseSolverControl`. Treat package-backed acceleration or interchange paths as out of graph until central package truth and host load proof exist.

## [5][RULES]

- Use direct algorithm objects when convergence, iterations, residuals, or status matter.
- Use static facades only when no diagnostic policy is needed.
- Make random/distribution flows seed-explicit for GH-visible output.
- Convert exceptions, non-convergence, non-finite values, and unsupported result shapes into LanguageExt rails at the boundary.
