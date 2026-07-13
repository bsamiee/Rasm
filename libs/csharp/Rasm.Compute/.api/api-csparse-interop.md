# [RASM_COMPUTE_API_CSPARSE_INTEROP]

`csparse-interop` is the native-solver interop layer extending `CSparse.NET` — the SAME author (`wo80`) as the admitted managed `CSparse`, so it is the canonical extension of that rail, never a parallel one. It binds the industry-standard native sparse eigensolver ARPACK (shift-invert Lanczos for the generalized symmetric eigenproblem `K·φ = λ·M·φ`) and the native direct factorizations CHOLMOD (SuiteSparse supernodal Cholesky), SuperLU, and UMFPACK, plus the modern Spectra eigensolver, to `CSparse`'s `SparseMatrix`. It is the sparse-eigen + native-direct tier: one sparse-eigen row on the `Solver/contract` `SolveMethod` axis and native-direct rows on `Tensor/factor`'s `FactorKind`, mirroring `blas.md`'s managed↔native substrate pattern — the managed CSparse `SparseLU`/`SparseCholesky` stays the cold-start terminal, the native row is claim-gated by RID exactly as `LinearProvider` is. It lifts the self-flagged dense-EVD eigensolver ceiling (`contract.md:104,733`) so building-scale `fea-modal`/`fea-buckling` and the seismic response-spectrum route solve at 10⁴–10⁶ DOF where dense O(n³) is infeasible.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `csparse-interop`
- package: `csparse-interop` (SOURCE-VENDORED from the live `wo80/csparse-interop`; NOT on public NuGet)
- license: BSD-3-Clause (same as `CSparse`); the wrapped native libraries carry their own permissive licenses (ARPACK BSD, SuiteSparse/CHOLMOD LGPL-or-permissive per module, SuperLU BSD)
- assembly: `CSparse.Interop` (managed wrapper over the native solver libraries)
- namespace: `CSparse.Interop.ARPACK`, `CSparse.Interop.Spectra`, `CSparse.Interop.SuiteSparse.Cholmod`, `CSparse.Interop.SuiteSparse.Umfpack`, `CSparse.Interop.SuperLU`
- asset: managed interop wrapper P/Invoking the native ARPACK / SuiteSparse / SuperLU libraries; osx-arm64 runs the SuiteSparse/ARPACK leg (MKL is x64-only). The native libraries are Forge-provisioned — the activation gate, NOT this campaign's design blocker; a solve without the provisioned native faults at native init
- verification: authored-at-admission against the `wo80/csparse-interop` repo surface; exact member signatures VERIFY against the vendored tree at the leg-2 first-compose (the vendored-source verification law), never asserted green before the tree lands
- rail: sparse-solver

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: sparse eigensolvers
- namespace: `CSparse.Interop.ARPACK`, `CSparse.Interop.Spectra`
- rail: sparse-solver
- note: every driver operates over a `CSparse` `SparseMatrix`; the `Spectrum` selector is `SmallestMagnitude`/`LargestMagnitude`/`SmallestAlgebraic`/`LargestAlgebraic`/`BothEnds`.

| [INDEX] | [SYMBOL]             | [CAPABILITY]                                                                                             |
| :-----: | :------------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Arpack`             | ARPACK shift-invert Lanczos/Arnoldi; standard + generalized, shift selects interior/smallest eigenvalues |
|  [02]   | `IEigenSolverResult` | eigen-result — `EigenValues`, `EigenVectors` (dense, optional), converged count, iterations, error code  |
|  [03]   | `Spectra`            | modern C++ Spectra eigensolver alternative to ARPACK — the fallback/comparison engine                    |
|  [04]   | `Job` / `Spectrum`   | which-eigenvalues `Spectrum` selector driving the shift-invert mode                                      |

[PUBLIC_TYPE_SCOPE]: native direct factorizations
- namespace: `CSparse.Interop.SuiteSparse.Cholmod`, `CSparse.Interop.SuperLU`, `CSparse.Interop.SuiteSparse.Umfpack`
- rail: sparse-solver
- note: each native factor is `Factorize()` then `Solve(b)`, `IDisposable` over a native handle.

| [INDEX] | [SYMBOL]  | [CAPABILITY]                                                                                                                |
| :-----: | :-------- | :-------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Cholmod` | SuiteSparse supernodal Cholesky over an SPD `SparseMatrix` — native counterpart of managed `SparseCholesky`                 |
|  [02]   | `SuperLU` | SuperLU native LU for general (unsymmetric) sparse systems — native counterpart of managed `SparseLU`                       |
|  [03]   | `Umfpack` | UMFPACK native multifrontal LU for general sparse systems — an alternate native-direct factor on the same `FactorKind` axis |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: generalized sparse eigensolve — `Arpack`
- namespace: `CSparse.Interop.ARPACK`
- rail: sparse-solver
- composition law: the `Solver/contract` FE lane assembles `K` and `M` as `CSparse` `SparseMatrix` (`B^T·D·B` and the lumped/consistent mass), constructs the `Arpack` driver over `(K, M)`, requests `k` eigenpairs with a shift near the frequency band of interest, and projects `(φ, λ)` onto the typed modal receipt; the 90% modal-mass-participation floor is a typed `(Solve, Numeric)` shortfall naming the achieved fraction.
- call: ctors take `SparseMatrix` args; the solve methods take `(int k, [double shift,] Spectrum which)` and return `IEigenSolverResult`.

| [INDEX] | [SURFACE]                                                                           | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `new Arpack(K, M[, symmetric])` / `new Arpack(A)`                                   | generalized / standard eigen driver                |
|  [02]   | `.Tolerance` / `.ArnoldiCount` / `.ComputeEigenVectors` / `.ShiftInvert` / `.Shift` | tolerance, Krylov, eigenvector, shift-invert knobs |
|  [03]   | `.SolveGeneralized` / `.SolveStandard`                                              | `k` eigenpairs nearest shift / spectrum end        |
|  [04]   | `.EigenValues` / `.EigenVectors` / `.ConvergedEigenValues` / `.IterationsTaken`     | eigenvalues, eigenvectors, converged, iterations   |

[ENTRYPOINT_SCOPE]: native direct factor — `Cholmod` / `SuperLU` / `Umfpack`
- namespace: `CSparse.Interop.SuiteSparse.Cholmod`, `CSparse.Interop.SuperLU`
- rail: sparse-solver
- call: ctors take one `SparseMatrix A`; `Solve` takes `(double[] b, double[] x)` or `(DenseMatrix B, DenseMatrix X)`.

| [INDEX] | [SURFACE]                                              | [CAPABILITY]                                                      |
| :-----: | :----------------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `new Cholmod(A)` / `new SuperLU(A)` / `new Umfpack(A)` | bind the native factor to an assembled `SparseMatrix`             |
|  [02]   | `factor.Factorize()`                                   | native symbolic+numeric factorization, once                       |
|  [03]   | `factor.Solve(b, x)` / `Solve(B, X)`                   | back/forward-substitute one or many RHS against the cached factor |
|  [04]   | `factor.Dispose()`                                     | release the native factor handle                                  |

## [04]-[IMPLEMENTATION_LAW]

[NATIVE_TIER]:
- the managed CSparse `SparseLU`/`SparseCholesky` (`api-csparse.md`) is the COLD-START terminal — always available, pure-managed; the native ARPACK/CHOLMOD/SuperLU row is claim-gated by RID exactly as `MathNet.Numerics.Providers.MKL`'s `LinearProvider` is, so osx-arm64 resolves the SuiteSparse/ARPACK leg and an x64 deployment can claim MKL PARDISO. A claimed row whose native asset is missing FAULTS at native init (`NativeClaim` on `Tensor/factor` — a probe `Factorize` at claim time, the typed `ComputeFault` rail carrying the miss); the managed terminal is a ROUTING decision made before the claim, never a silent post-claim fallback.
- the native factor and eigensolver are `IDisposable` over native handles; a factorization is performed ONCE and reused across many right-hand sides / spectrum shifts, never re-factored per solve.

[STACKING]:
- `Solver/contract#SOLVE_METHOD`: the sparse-eigen row composes the `Arpack` driver over Compute's OWN CSparse-assembled `K`/`M` — never a reach into the landed kernel `Numerics/matrix.md` LOBPCG (that is kernel-interior spectral machinery; the kernel ledger declares no matrix-plane seam toward Compute). `fea-modal`/`fea-buckling`/seismic response-spectrum solve here.
- `Tensor/factor#FACTOR_KIND`: the native-direct rows (`Cholmod`/`SuperLU`/`Umfpack`) sit beside the managed `SparseCholesky`/`SparseLU` on the one `FactorKind` axis, mirroring `Tensor/blas`'s managed↔native `LinearProvider` pattern — one factor fold discriminates on the row, never a parallel native-solver owner.
- `CSparse` (`api-csparse.md`): the shared `SparseMatrix`/`CompressedColumnStorage` types the native drivers factor — csparse-interop is the native EXTENSION of that rail (same author), so its native-factor delta co-locates in the `api-csparse.md` reconcile, never a duplicate matrix type.

[RAIL_LAW]:
- Package: `csparse-interop` (BSD-3, source-vendored from `wo80/csparse-interop`; native libraries Forge-provisioned)
- Owns: the native sparse eigensolver (ARPACK shift-invert Lanczos, Spectra) and native direct factors (CHOLMOD/SuperLU/UMFPACK) extending `CSparse` — the sparse-eigen + native-direct tier
- Accept: a building-scale generalized symmetric eigensolve (`fea-modal`/`fea-buckling`/seismic) and a native direct factor over an assembled `CSparse` `SparseMatrix`, RID-claim-gated with the managed CSparse terminal as the cold-start fallback
- Reject: a reach into the kernel `Numerics/matrix.md` LOBPCG (kernel-interior, no seam); a parallel native-solver rail beside the one `SolveMethod`/`FactorKind` axis; a native-required assumption absent the Forge provisioning; a member asserted before the vendored-tree first-compose verification
