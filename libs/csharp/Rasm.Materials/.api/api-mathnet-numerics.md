# [RASM_MATERIALS_API_MATHNET_NUMERICS]

Full surface and stacking: `libs/csharp/.api/api-mathnet-numerics.md` (shared-tier canonical owner).

## [01]-[IMPLEMENTATION_LAW]

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
