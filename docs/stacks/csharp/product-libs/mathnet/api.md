# [MATHNET_API]

MathNet owns numeric algorithms, solvers, statistics, and symbolic operations. Treat supporting package closure as load-context evidence, not as public API guidance.

## [1][OWNERSHIP]

| [INDEX] | [SURFACE]                        | [OWNS]                                                 | [NOT_OWN]                                |
| :-----: | -------------------------------- | ------------------------------------------------------ | ---------------------------------------- |
|   [1]   | `MathNet.Numerics.LinearAlgebra` | Matrices, vectors, decompositions, solvers.            | Host geometry identity.                  |
|   [2]   | `MathNet.Numerics.Optimization`  | Minimizers, objective functions, convergence.          | Domain failure rails.                    |
|   [3]   | `MathNet.Numerics.Integration`   | Numerical integration rules.                           | Host curve/surface semantic integration. |
|   [4]   | `MathNet.Numerics.Interpolation` | Interpolation schemes.                                 | Host tree/path structure.                |
|   [5]   | `MathNet.Numerics.Statistics`    | Estimators and distributions.                          | Domain result vocabulary.                |
|   [6]   | `MathNet.Symbolics`              | Formula parse, transform, calculus, evaluate, compile. | Runtime load safety.                     |

**Consumer adoption:** local production use belongs in `rasm.md` or package-state docs. Keep this page to MathNet API ownership.

Host geometry and data-access boundaries: project usage owner, maintained host metadata, and nested host instruction owner.

## [2][CONSTRUCTION]

Use `Matrix<T>.Build`, `Vector<T>.Build`, `MatrixBuilder<T>`, `VectorBuilder<T>`, `CreateMatrix`, and `CreateVector`. Do not document non-public builder implementation types. MathNet storage and indexers stay inside algorithm execution; public domain outputs carry dimensions, ordering, convergence, tolerance, and diagnostics.

## [3][CONTROL]

Control surfaces include `MathNet.Numerics.Control`, `MathNet.Numerics.Providers.LinearAlgebra.LinearAlgebraControl`, `MathNet.Numerics.Providers.FourierTransform.FourierTransformControl`, and `MathNet.Numerics.Providers.SparseSolver.SparseSolverControl`. Package-backed acceleration or interchange paths become project guidance only after the package graph and host load owner record an active consumer.

## [4][RULES]

- Use direct algorithm objects when convergence, iterations, residuals, or status matter.
- Use static facades only when no diagnostic policy is needed.
- Make random/distribution flows seed-explicit for GH-visible output.
- Convert exceptions, non-convergence, non-finite values, and unsupported result shapes into LanguageExt rails at the boundary.
