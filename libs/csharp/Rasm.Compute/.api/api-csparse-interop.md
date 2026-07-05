# [RASM_COMPUTE_API_CSPARSE_INTEROP]

`csparse-interop` is the native-solver interop layer extending `CSparse.NET` — the SAME author (`wo80`) as the admitted managed `CSparse`, so it is the canonical extension of that rail, never a parallel one. It binds the industry-standard native sparse eigensolver ARPACK (shift-invert Lanczos for the generalized symmetric eigenproblem `K·φ = λ·M·φ`) and the native direct factorizations CHOLMOD (SuiteSparse supernodal Cholesky), SuperLU, and UMFPACK, plus the modern Spectra eigensolver, to `CSparse`'s `SparseMatrix`. It is the `[V9]` sparse-eigen + native-direct tier: one sparse-eigen row on the `Solver/contract` `SolveMethod` axis and native-direct rows on `Tensor/factor`'s `FactorKind`, mirroring `blas.md`'s managed↔native substrate pattern — the managed CSparse `SparseLU`/`SparseCholesky` stays the cold-start terminal, the native row is claim-gated by RID exactly as `LinearProvider` is. It lifts the self-flagged dense-EVD eigensolver ceiling (`contract.md:104,733`) so building-scale `fea-modal`/`fea-buckling` and the seismic response-spectrum route solve at 10⁴–10⁶ DOF where dense O(n³) is infeasible.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `csparse-interop`
- package: `csparse-interop` (SOURCE-VENDORED from the live `wo80/csparse-interop`; NOT on public NuGet)
- version: vendored at the admitted `wo80/csparse-interop` HEAD (tracked as a vendored-source revision, not a pin)
- license: BSD-3-Clause (same as `CSparse`); the wrapped native libraries carry their own permissive licenses (ARPACK BSD, SuiteSparse/CHOLMOD LGPL-or-permissive per module, SuperLU BSD)
- assembly: `CSparse.Interop` (managed wrapper over the native solver libraries)
- namespace: `CSparse.Interop.ARPACK`, `CSparse.Interop.Spectra`, `CSparse.Interop.SuiteSparse.Cholmod`, `CSparse.Interop.SuiteSparse.Umfpack`, `CSparse.Interop.SuperLU`
- asset: managed interop wrapper P/Invoking the native ARPACK / SuiteSparse / SuperLU libraries; osx-arm64 runs the SuiteSparse/ARPACK leg (MKL is x64-only). The native libraries are **Forge-provisioned** — the activation gate, NOT this campaign's design blocker; a solve without the provisioned native faults at native init
- verification: authored-at-admission against the `wo80/csparse-interop` repo surface; exact member signatures VERIFY against the vendored tree at the leg-2 first-compose (the vendored-source verification law), never asserted green before the tree lands
- rail: sparse-solver

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: sparse eigensolvers
- namespace: `CSparse.Interop.ARPACK`, `CSparse.Interop.Spectra`
- rail: sparse-solver

| [INDEX] | [SYMBOL]              | [RAIL]         | [CAPABILITY]                                                                                              |
| :-----: | :-------------------- | :------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Arpack`              | sparse-solver  | the ARPACK shift-invert Lanczos/Arnoldi driver over a `CSparse` `SparseMatrix`; standard `A·x = λ·x` and generalized `A·x = λ·B·x` eigenproblems with a shift for interior/smallest eigenvalues — the `fea-modal`/`fea-buckling` `(φ, λ)` engine |
|  [02]   | `IEigenSolverResult`  | sparse-solver  | the eigen-result carrier — `EigenValues` (`Complex[]`), `EigenVectors` (a dense matrix, when requested), converged count, iterations, and an error/convergence code |
|  [03]   | `Spectra`             | sparse-solver  | the modern C++ Spectra eigensolver alternative to ARPACK over the same `SparseMatrix` — the fallback/comparison engine on the one eigen rail |
|  [04]   | `Job` / `Spectrum`    | sparse-solver  | the which-eigenvalues selector (`SmallestMagnitude`/`LargestMagnitude`/`SmallestAlgebraic`/`LargestAlgebraic`/`BothEnds`) driving the shift-invert mode |

[PUBLIC_TYPE_SCOPE]: native direct factorizations
- namespace: `CSparse.Interop.SuiteSparse.Cholmod`, `CSparse.Interop.SuperLU`, `CSparse.Interop.SuiteSparse.Umfpack`
- rail: sparse-solver

| [INDEX] | [SYMBOL]   | [RAIL]        | [CAPABILITY]                                                                                              |
| :-----: | :--------- | :------------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Cholmod`  | sparse-solver | the SuiteSparse supernodal Cholesky native factor over an SPD `SparseMatrix` — `Factorize()` then `Solve(b)`, `IDisposable` native factor; the native counterpart of the managed `SparseCholesky` for large SPD stiffness systems |
|  [02]   | `SuperLU`  | sparse-solver | the SuperLU native LU factor for general (unsymmetric) sparse systems — `Factorize()`/`Solve(b)`, `IDisposable`; the native counterpart of the managed `SparseLU` |
|  [03]   | `Umfpack`  | sparse-solver | the UMFPACK native multifrontal LU for general sparse systems — an alternate native-direct factor on the same `FactorKind` axis |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: generalized sparse eigensolve — `Arpack`
- namespace: `CSparse.Interop.ARPACK`
- rail: sparse-solver
- composition law: the `Solver/contract` FE lane assembles `K` and `M` as `CSparse` `SparseMatrix` (`B^T·D·B` and the lumped/consistent mass), constructs the `Arpack` driver over `(K, M)`, requests `k` eigenpairs with a shift near the frequency band of interest, and projects `(φ, λ)` onto the typed modal receipt; the 90% modal-mass-participation floor is a typed `(Solve, Numeric)` shortfall naming the achieved fraction.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `new Arpack(K, M)` / `new Arpack(A)` | `(SparseMatrix K, SparseMatrix M[, bool symmetric])` / `(SparseMatrix A)`            | the generalized `K·φ = λ·M·φ` / standard eigen driver        |
|  [02]   | `arpack.Tolerance` / `ArnoldiCount` / `ComputeEigenVectors` / `ShiftInvert` / `Shift` | properties      | convergence tolerance, Krylov subspace size, eigenvector request, shift-invert mode + shift value |
|  [03]   | `arpack.SolveGeneralized` / `SolveStandard` | `(int k, [double shift,] Spectrum which)` → `IEigenSolverResult`               | compute the `k` eigenpairs nearest the shift / at the spectrum end |
|  [04]   | `result.EigenValues` / `EigenVectors` / `ConvergedEigenValues` / `IterationsTaken` | reads                     | the eigenvalue vector, eigenvector matrix, converged count, iteration count |

[ENTRYPOINT_SCOPE]: native direct factor — `Cholmod` / `SuperLU` / `Umfpack`
- namespace: `CSparse.Interop.SuiteSparse.Cholmod`, `CSparse.Interop.SuperLU`
- rail: sparse-solver

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `new Cholmod(A)` / `new SuperLU(A)` | `(SparseMatrix A)`                                                                    | bind the native factor to an assembled sparse system         |
|  [02]   | `factor.Factorize`              | `()`                                                                                     | perform the native symbolic+numeric factorization once       |
|  [03]   | `factor.Solve`                  | `(double[] b, double[] x)` / `(DenseMatrix B, DenseMatrix X)`                             | back/forward-substitute for one or many right-hand sides against the cached factor |
|  [04]   | `factor.Dispose`                | `()`                                                                                     | release the native factor handle                             |

## [04]-[IMPLEMENTATION_LAW]

[NATIVE_TIER]:
- the managed CSparse `SparseLU`/`SparseCholesky` (`api-csparse.md`) is the COLD-START terminal — always available, pure-managed; the native ARPACK/CHOLMOD/SuperLU row is claim-gated by RID exactly as `MathNet.Numerics.Providers.MKL`'s `LinearProvider` is, so osx-arm64 resolves the SuiteSparse/ARPACK leg and an x64 deployment can claim MKL PARDISO. A missing native asset falls back to the managed terminal, never faults the design.
- the native factor and eigensolver are `IDisposable` over native handles; a factorization is performed ONCE and reused across many right-hand sides / spectrum shifts, never re-factored per solve.

[STACKING]:
- `Solver/contract#SOLVE_METHOD` (`[V9]`): the sparse-eigen row composes the `Arpack` driver over Compute's OWN CSparse-assembled `K`/`M` — never a reach into the landed kernel `Numerics/matrix.md` LOBPCG (that is kernel-interior spectral machinery; the kernel ledger declares no matrix-plane seam toward Compute). `fea-modal`/`fea-buckling`/seismic response-spectrum solve here.
- `Tensor/factor#FACTOR_KIND`: the native-direct rows (`Cholmod`/`SuperLU`/`Umfpack`) sit beside the managed `SparseCholesky`/`SparseLU` on the one `FactorKind` axis, mirroring `Tensor/blas`'s managed↔native `LinearProvider` pattern — one factor fold discriminates on the row, never a parallel native-solver owner.
- `CSparse` (`api-csparse.md`): the shared `SparseMatrix`/`CompressedColumnStorage` types the native drivers factor — csparse-interop is the native EXTENSION of that rail (same author), so its native-factor delta co-locates in the `api-csparse.md` reconcile, never a duplicate matrix type.

[RAIL_LAW]:
- Package: `csparse-interop` (BSD-3, source-vendored from `wo80/csparse-interop`; native libraries Forge-provisioned)
- Owns: the native sparse eigensolver (ARPACK shift-invert Lanczos, Spectra) and native direct factors (CHOLMOD/SuperLU/UMFPACK) extending `CSparse` — the `[V9]` sparse-eigen + native-direct tier
- Accept: a building-scale generalized symmetric eigensolve (`fea-modal`/`fea-buckling`/seismic) and a native direct factor over an assembled `CSparse` `SparseMatrix`, RID-claim-gated with the managed CSparse terminal as the cold-start fallback
- Reject: a reach into the kernel `Numerics/matrix.md` LOBPCG (kernel-interior, no seam); a parallel native-solver rail beside the one `SolveMethod`/`FactorKind` axis; a native-required assumption absent the Forge provisioning; a member asserted before the vendored-tree first-compose verification
