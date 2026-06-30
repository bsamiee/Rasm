# [RASM_MATERIALS_API_MATHNET_NUMERICS]

`MathNet.Numerics` supplies the dense linear-algebra carrier (`Matrix<double>`/`Vector<double>`), the matrix factorization family (`QR`/`Svd`/`Cholesky`/`LU`), and the managed-provider selection façade the `Appearance/acquisition#CAPTURE_SOURCE` `MeasuredBrdf` fit consumes for its overdetermined GGX/Smith least-squares solve. The fit is a thin-QR Gauss-Newton iteration over the angular-reflectance design matrix — the `algorithms-doc#ROUTE_SPINE` dense-overdetermined route — admitted as a DIRECT Materials dependency (the AEC-domain folder's own pin), never a `Rasm.Compute` project reference, which the acyclic strata forbids the AEC→app-platform edge.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MathNet.Numerics`
- package: `MathNet.Numerics`
- version: `6.0.0-beta2` (pre-release; central-pinned in `Directory.Packages.props`, removable when 6.0.0 stable lands)
- license: MIT
- assembly: `MathNet.Numerics`
- namespace: `MathNet.Numerics`, `MathNet.Numerics.LinearAlgebra`, `MathNet.Numerics.LinearAlgebra.Double`, `MathNet.Numerics.LinearAlgebra.Factorization`, `MathNet.Numerics.Providers.LinearAlgebra`
- asset: runtime library (multi-TFM `net48` / `net6.0` / `net8.0` / `netstandard2.0`; the consumer `net10.0` binds the `net8.0` asset). Pure-managed core; the native MKL/OpenBLAS/CUDA providers are SEPARATE companion packages (`MathNet.Numerics.Providers.MKL`/`.OpenBLAS`, x64-only) — osx-arm64 has no native asset and rides `Control.UseManaged()`.
- rail: numeric

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dense algebra
- rail: numeric

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE] | [CAPABILITY]                                                       |
| :-----: | :---------------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `Matrix<double>` (`Double.Matrix`)  | dense matrix   | the numeric-lane dense carrier; GEMV/GEMM + transpose-multiply family |
|  [02]   | `Vector<double>` (`Double.Vector`)  | dense vector   | the dense right-hand-side / solution carrier; full norm family    |
|  [03]   | `MatrixBuilder<double>` (`.Build`)  | factory        | the 17-member `DenseOf*` build family (design-matrix construction) |
|  [04]   | `VectorBuilder<double>` (`.Build`)  | factory        | the dense vector build family (residual/RHS construction)         |
|  [05]   | `QR<double>`                        | factorization  | thin/full QR decomposition + least-squares solve; `Q`/`R`/`IsFullRank` |
|  [06]   | `Svd<double>`                       | factorization  | SVD; `S`/`U`/`VT`/`W`/`Rank`/`ConditionNumber`/`L2Norm` + pseudo-inverse solve |
|  [07]   | `Cholesky<double>`                  | factorization  | SPD decomposition + solve; `Determinant`/`DeterminantLn` (normal-equations path) |
|  [08]   | `ISolver<double>`                   | solver seam    | the shared `Solve(input)` / `Solve(input, result)` seam every factorization exposes |
|  [09]   | `QRMethod`                          | enum           | `Full` / `Thin` factorization mode                                |

[PUBLIC_TYPE_SCOPE]: provider selection
- rail: numeric

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE] | [CAPABILITY]                                                          |
| :-----: | :----------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `Control`                | static façade  | global provider selection (`UseManaged`/`UseBest`/`TryUseNative*`/`FreeResources`), threading, and `Describe` |
|  [02]   | `LinearAlgebraControl`   | static façade  | provider-level direct selection (`Provider` get/set, `UseBest`/`TryUse`, `HintPath`) |
|  [03]   | `ILinearAlgebraProvider` | provider seam  | the active provider handle (`ManagedLinearAlgebraProvider.Instance` on osx-arm64) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: dense build + factorization + solve
- rail: numeric

| [INDEX] | [SURFACE]                                              | [CALL_SHAPE]       | [CAPABILITY]                                                                  |
| :-----: | :----------------------------------------------------- | :----------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Matrix<double>.Build.DenseOfArray(double[,])`         | factory call       | builds a dense `m×n` matrix from a `double[,]`                                |
|  [02]   | `Matrix<double>.Build.Dense(m, n, init)`               | factory call       | builds a dense matrix by shape + `(i,j)=>` initializer (the Jacobian build)   |
|  [03]   | `Matrix<double>.Build.DenseOfRowArrays` / `DenseOfColumnMajor` / `DenseOfIndexed` | factory call | the 17-member `DenseOf*` family — build the design matrix from rows, column-major buffer, or sparse `(i,j,v)` triples without a `double[,]` copy |
|  [04]   | `Vector<double>.Build.Dense(n)` / `Dense(n, init)`     | factory call       | builds a dense vector by length or by `i=>` initializer (the residual build)  |
|  [05]   | `Vector<double>.Build.DenseOfArray(double[])`          | factory call       | builds a dense vector from a `double[]`                                       |
|  [06]   | `Matrix<double>.QR()` / `QR(QRMethod.Full)`            | matrix call        | builds the full Householder `QR<double>`                                      |
|  [07]   | `Matrix<double>.QR(QRMethod.Thin)`                     | matrix call        | builds the thin `QR<double>` for `m > n` (the overdetermined route)           |
|  [08]   | `Matrix<double>.Svd(computeVectors)`                   | matrix call        | builds `Svd<double>`; `true` computes `U`/`VT` for the solve path             |
|  [09]   | `Matrix<double>.Cholesky()`                            | matrix call        | builds `Cholesky<double>` for an SPD matrix (the normal-equations path)       |
|  [10]   | `QR<double>.Solve(Vector)` / `Solve(Vector, result)`   | factorization call | least-squares solve for an overdetermined system; in-place mirror writes into `result` |
|  [11]   | `QR<double>.Solve(Matrix)`                             | factorization call | least-squares solve over multiple right-hand sides                           |
|  [12]   | `QR<double>.Q` / `.R` / `.IsFullRank` / `.Determinant` | factorization prop | the factor buffers (probe all-finite after factoring) + rank/determinant readouts |
|  [13]   | `Svd<double>.Solve(Vector)`                            | factorization call | truncated pseudo-inverse solve (rank-deficient conditioning fallback)         |
|  [14]   | `Svd<double>.S` / `.U` / `.VT` / `.W`                  | factorization prop | singular-value vector `S`, left/right vectors `U`/`VT`, diagonal `W`          |
|  [15]   | `Svd<double>.Rank`                                     | factorization prop | numerical rank (`σ_max·ε·max(m,n)` threshold)                                |
|  [16]   | `Svd<double>.ConditionNumber` / `.L2Norm` / `.Determinant` | factorization prop | `σ_max/σ_min` (`+Inf` for rank-deficient), spectral 2-norm `σ_max`, det      |
|  [17]   | `Cholesky<double>.Solve(Vector)` / `.Determinant` / `.DeterminantLn` | factorization call | SPD solve of `(AᵀA)x = Aᵀb`; log-determinant for ill-conditioning detection |
|  [18]   | `Matrix<double>.TransposeThisAndMultiply(Matrix)` / `(Vector)` | matrix call | forms `AᵀA` and `Aᵀb` for the normal-equations Gram WITHOUT an explicit transpose allocation |
|  [19]   | `Matrix<double>.Multiply(Vector, result)`             | matrix call        | in-place GEMV `A·x → result` (the residual recompute against the ORIGINAL `A`) |
|  [20]   | `Vector<double>.L2Norm()` / `L1Norm()` / `InfinityNorm()` / `Norm(p)` | vector call | Euclidean (residual magnitude), L1, ∞, and general p-norm                     |

[ENTRYPOINT_SCOPE]: provider selection
- rail: numeric

| [INDEX] | [SURFACE]                                        | [CALL_SHAPE]  | [CAPABILITY]                                                                |
| :-----: | :----------------------------------------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `Control.UseManaged()`                           | static `void` | selects the pure-managed provider (the osx-arm64 composition-root call)     |
|  [02]   | `Control.UseDefaultProviders()` / `UseBestProviders()` | static `void` | selects the default or best-available (native-if-present) provider set |
|  [03]   | `Control.TryUseNative()` / `TryUseNativeMKL()` / `TryUseNativeOpenBLAS()` / `TryUseNativeCUDA()` | static `bool` | probes a native provider; `false` when the companion native package/asset is absent (always `false` on osx-arm64) |
|  [04]   | `Control.UseSingleThread()` / `UseMultiThreading()` | static `void` | sets the managed-provider threading mode                                |
|  [05]   | `Control.FreeResources()` / `Describe()`         | static        | releases native handles; returns the active-provider description string    |
|  [06]   | `LinearAlgebraControl.Provider`                  | static prop   | gets/sets the active `ILinearAlgebraProvider` handle directly               |
|  [07]   | `LinearAlgebraControl.UseManaged()` / `UseBest()` / `UseDefault()` / `TryUse(provider)` | static | provider-level direct selection mirroring `Control`, plus `TryUse` of an explicit handle |
|  [08]   | `LinearAlgebraControl.HintPath`                  | static prop   | optional native-binary probe path (unused on the managed osx-arm64 path)    |

## [04]-[IMPLEMENTATION_LAW]

[DENSE_FACTOR]:
- carrier: `Matrix<double>`/`Vector<double>` composed directly — never a package-local matrix wrapper.
- overdetermined route: `Matrix<double>.QR(QRMethod.Thin)` then `QR<double>.Solve(b)` returns the least-squares solution `argmin‖Ax−b‖₂` for `m > n`; the conditioning fallback is `Svd(true)` truncated pseudo-inverse (`algorithms-doc#ROUTE_SPINE` row [03]).
- normal-equations alternative: `A.TransposeThisAndMultiply(A)` forms the SPD Gram `AᵀA` and `A.TransposeThisAndMultiply(b)` the `Aᵀb` RHS WITHOUT an explicit transpose allocation, then `Cholesky().Solve(Aᵀb)` — but this squares the condition number, so prefer thin-QR when the design-matrix conditioning approaches the inverse cap; the Gram path is the SPD-only fallback.
- residual witness: recompute `‖A·x − b‖₂ / ‖b‖₂` against the ORIGINAL operator in working precision (`Matrix.Multiply(x, scratch)` + `Vector.L2Norm`), never against the reconstructed factors — the one correctness signal the fit reports on the `Provenance.FitResidual` receipt.

[ADMISSION]:
- The library refuses its own gates: a near-zero column norm divides through and fills `Q`/`R` with `NaN` while `IsFullRank` returns `true`; admit by gating the design matrix all-finite (`Enumerate().All(double.IsFinite)`) before factoring and probing the `Q`/`R` (or the `Svd.Solve` output) all-finite after — the `acquisition#ACQUISITION` fit switches to `Svd(true).Solve` exactly on a non-finite thin-QR step.
- `Control.UseManaged()` is selected ONCE at composition; a per-call-site `Control.TryUseNativeMKL()` is the named defect, and the native MKL/OpenBLAS/CUDA providers are SEPARATE x64-only companion packages so osx-arm64 always rides `ManagedLinearAlgebraProvider.Instance`.

[STACK]:
- acquisition seam: `acquisition#ACQUISITION` `SolveGgx` runs the thin-QR Gauss-Newton step `Δp = Matrix.Build.Dense(m, 2, Jacobian).QR(QRMethod.Thin).Solve(−r)` over the log-residual `Vector.Build.Dense(m, …)`, switches to `Svd(true).Solve` on a non-finite step, and witnesses `‖r‖/‖logMeasured‖` via `Vector.L2Norm` — the `bsdf#MICROFACET_KERNEL` GGX/Smith/Fresnel `D·G·F/(4·cosθi·cosθo)` is the forward model, MathNet owns only the dense solve.
- receipt seam: the numeric rail and the `Wacton.Unicolour` colour rail MEET at the row — the fitted `FitResidual` (this package) and the spectral-grounded scene-linear base colour (the colour package) pair on ONE `Provenance` receipt the `bsdf#WHITE_FURNACE_HARNESS` gates and the `interchange#MATERIAL_WIRE` `WireProvenance` carries; there is no fused colour-plus-numeric kernel.
- strata seam: MathNet is a DIRECT Materials NuGet pin (the AEC-domain folder's own dependency), never a `Rasm.Compute` project reference — the acyclic strata forbids the AEC→app-platform edge, and the `csharp:Rasm.Compute/blas#DENSE_ALGEBRA` thin-QR is a DOCTRINE shape the fit follows, not a realized project edge.

[RAIL_LAW]:
- Package: `MathNet.Numerics` (dense linear-algebra subset; the distribution/integration/root-finding surface is not consumed by this folder)
- Owns: dense matrix factorization (`QR`/`Svd`/`Cholesky`), the least-squares solve, managed-provider selection
- Accept: `Matrix<double>`/`Vector<double>` dense work, overdetermined least-squares, residual witness against the original operator
- Reject: a hand-rolled Levenberg-Marquardt or normal-equations loop, a Gram-plus-ridge that squares `κ` when thin-QR avoids it, a `Rasm.Compute` project reference to obtain MathNet transitively (the acyclic strata forbids it)
