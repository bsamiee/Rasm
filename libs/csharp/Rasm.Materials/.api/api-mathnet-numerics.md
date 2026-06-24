# [RASM_MATERIALS_API_MATHNET_NUMERICS]

`MathNet.Numerics` supplies the dense linear-algebra carrier (`Matrix<double>`/`Vector<double>`), the matrix factorization family (`QR`/`Svd`/`Cholesky`/`LU`), and the managed-provider selection façade the `Appearance/acquisition#CAPTURE_SOURCE` `MeasuredBrdf` fit consumes for its overdetermined GGX/Smith least-squares solve. The fit is a thin-QR Gauss-Newton iteration over the angular-reflectance design matrix — the `algorithms-doc#ROUTE_SPINE` dense-overdetermined route — admitted as a DIRECT Materials dependency (the AEC-domain folder's own pin), never a `Rasm.Compute` project reference, which the acyclic strata forbids the AEC→app-platform edge.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MathNet.Numerics`
- package: `MathNet.Numerics`
- assembly: `MathNet.Numerics`
- namespace: `MathNet.Numerics`, `MathNet.Numerics.LinearAlgebra`, `MathNet.Numerics.LinearAlgebra.Double`, `MathNet.Numerics.LinearAlgebra.Factorization`, `MathNet.Numerics.Providers.LinearAlgebra`
- asset: runtime library (managed; native MKL/OpenBLAS providers are x64-only, osx-arm64 falls back to `UseManaged`)
- rail: numeric

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dense algebra
- rail: numeric

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE] | [CAPABILITY]                                        |
| :-----: | :--------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `Matrix<double>` (`Double.Matrix`) | dense matrix   | the numeric-lane dense carrier (column-major)       |
|  [02]   | `Vector<double>` (`Double.Vector`) | dense vector   | the dense right-hand-side / solution carrier        |
|  [03]   | `QR<double>`                       | factorization  | thin/full QR decomposition + least-squares solve    |
|  [04]   | `Svd<double>`                      | factorization  | SVD; `Rank`/`ConditionNumber`/pseudo-inverse solve  |
|  [05]   | `Cholesky<double>`                 | factorization  | SPD decomposition + solve (normal-equations path)   |
|  [06]   | `ISolver<double>`                  | solver seam    | the shared `Solve` seam every factorization exposes |
|  [07]   | `QRMethod`                         | enum           | `Full` / `Thin` factorization mode                  |

[PUBLIC_TYPE_SCOPE]: provider selection
- rail: numeric

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE] | [CAPABILITY]                         |
| :-----: | :----------------------- | :------------- | :----------------------------------- |
|  [01]   | `Control`                | static façade  | selects + probes the active provider |
|  [02]   | `LinearAlgebraControl`   | static façade  | provider-level direct selection API  |
|  [03]   | `ILinearAlgebraProvider` | provider seam  | the active provider handle           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: dense build + factorization + solve
- rail: numeric

| [INDEX] | [SURFACE]                                 | [CALL_SHAPE]       | [CAPABILITY]                                           |
| :-----: | :---------------------------------------- | :----------------- | :----------------------------------------------------- |
|  [01]   | `Matrix<double>.Build.DenseOfArray`       | factory call       | builds a dense `m×n` matrix from a `double[,]`         |
|  [02]   | `Matrix<double>.Build.Dense(m, n, init)`  | factory call       | builds a dense matrix by shape/initializer             |
|  [03]   | `Vector<double>.Build.Dense(n)`           | factory call       | builds a dense vector by length                        |
|  [04]   | `Vector<double>.Build.DenseOfArray`       | factory call       | builds a dense vector from a `double[]`                |
|  [05]   | `Matrix<double>.QR()`                     | matrix call        | builds `QR<double>` (full Householder)                 |
|  [06]   | `Matrix<double>.QR(QRMethod.Thin)`        | matrix call        | builds the thin `QR<double>` for `m > n`               |
|  [07]   | `Matrix<double>.Svd(computeVectors)`      | matrix call        | builds `Svd<double>`; `true` for the solve path        |
|  [08]   | `QR<double>.Solve(Vector<double>)`        | factorization call | least-squares solve for an overdetermined `m×n` system |
|  [09]   | `QR<double>.Solve(Matrix<double>)`        | factorization call | least-squares solve over multiple right-hand sides     |
|  [10]   | `Svd<double>.Solve(Vector<double>)`       | factorization call | truncated pseudo-inverse solve (rank-deficient path)   |
|  [11]   | `Svd<double>.Rank`                        | factorization prop | numerical rank (`σ_max·ε·max(m,n)` threshold)          |
|  [12]   | `Svd<double>.ConditionNumber`             | factorization prop | `σ_max/σ_min` (`+Inf` for rank-deficient)              |
|  [13]   | `Matrix<double>.Multiply(Vector, result)` | matrix call        | in-place GEMV (the residual recompute against `A`)     |
|  [14]   | `Vector<double>.L2Norm()`                 | vector call        | Euclidean norm (the residual magnitude)                |

[ENTRYPOINT_SCOPE]: provider selection
- rail: numeric

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]  | [CAPABILITY]                                      |
| :-----: | :------------------------------ | :------------ | :------------------------------------------------ |
|  [01]   | `Control.UseManaged()`          | static `void` | selects the pure-managed provider (osx-arm64)     |
|  [02]   | `Control.TryUseNativeMKL()`     | static `bool` | selects MKL; `false` on missing native (x64 only) |
|  [03]   | `LinearAlgebraControl.Provider` | static prop   | gets/sets the active provider handle              |

## [04]-[IMPLEMENTATION_LAW]

[DENSE_FACTOR]:
- carrier: `Matrix<double>`/`Vector<double>` composed directly — never a package-local matrix wrapper.
- overdetermined route: `Matrix<double>.QR(QRMethod.Thin)` then `QR<double>.Solve(b)` returns the least-squares solution `argmin‖Ax−b‖₂` for `m > n`; the conditioning fallback is `Svd(true)` truncated pseudo-inverse (`algorithms-doc#ROUTE_SPINE` row [03]).
- normal-equations alternative: `(AᵀA)` symmetrized then `Cholesky().Solve(Aᵀb)` squares the condition number; prefer thin-QR when the design matrix conditioning exceeds the inverse cap.
- residual witness: recompute `‖A·x − b‖₂ / ‖b‖₂` against the ORIGINAL operator in working precision (`Matrix.Multiply(x, scratch)` + `L2Norm`), never against the reconstructed factors — the one correctness signal the fit reports on the `Provenance.FitResidual` receipt.

[ADMISSION]:
- The library refuses its own gates: a near-zero column norm divides through and fills `Q`/`R` with `NaN` while `IsFullRank` returns `true`; admit by gating the design matrix all-finite before factoring and probing the factor buffers all-finite after.
- `Control.UseManaged()` is selected ONCE at composition; a per-call-site `Control.TryUseNativeMKL()` is the named defect, and the native MKL/OpenBLAS assets are x64-only so osx-arm64 always rides the managed path.

[RAIL_LAW]:
- Package: `MathNet.Numerics` (dense linear-algebra subset; the distribution/integration/root-finding surface is not consumed by this folder)
- Owns: dense matrix factorization (`QR`/`Svd`/`Cholesky`), the least-squares solve, managed-provider selection
- Accept: `Matrix<double>`/`Vector<double>` dense work, overdetermined least-squares, residual witness against the original operator
- Reject: a hand-rolled Levenberg-Marquardt or normal-equations loop, a Gram-plus-ridge that squares `κ` when thin-QR avoids it, a `Rasm.Compute` project reference to obtain MathNet transitively (the acyclic strata forbids it)
