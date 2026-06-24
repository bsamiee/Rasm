# [COMPUTE_TASKLOG]

The open and closed work for measured execution, distilled from `IDEAS.md`. Each open task carries a status marker and the capability-to-build, packages, integration points/boundaries, and key considerations; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup and the residual live-host probes whose owner shape is complete.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[NATIVE-BLAS]-[BLOCKED]: activate the native-OpenBLAS/MKL dense and sparse execution row.
- Capability: `LinearProvider.Select` activates the native dense and sparse provider row only when the RID resolves the admitted OpenBLAS or MKL native asset and the MathNet `Control.TryUseNativeOpenBLAS`/`TryUseNativeMKL` probe succeeds.
- Shape: the `Tensor/blas#DENSE_ALGEBRA` and `Tensor/factor#SPARSE_SOLVE` lanes stay shape-complete as one RID-keyed provider axis, with the managed terminal proved end-to-end on osx-arm64 and the native row entering as provider data rather than a second solve surface.
- Unlocks: win-x64 and linux-x64 hosts can prove the same dense factorization, sparse factorization, residual witness, and receipt path against native acceleration without changing call sites.
- Anchors: `Tensor/blas#DENSE_ALGEBRA`, `Tensor/factor#SPARSE_SOLVE`, `MathNet.Numerics.Providers.MKL`, `MathNet.Numerics.Providers.OpenBLAS`, `MathNet.Numerics.MKL.Win-x64`, and `MathNet.Numerics.MKL.Linux-x64`.
- Tension: blocked on a host RID that resolves the native asset; the managed fallback is the correct cold start until then.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[ADMIT_SILK_WEBGPU]-[COMPLETE]: `Silk.NET.WebGPU` 2.23.0 (already manifest-pinned) registered in Compute README `[TENSOR_NUMERIC]` and `.api/api-silk-webgpu.md` authored for the shared branch `ONE_WGPU_DEVICE`; compute-shader lowering rides `DEEPEN_DEVICE_GEMM_SPMV`.
[ADMIT_ORTOOLS]-[COMPLETE]: `Google.OrTools` 9.15.6755 pinned + referenced (per-RID natives incl. osx-arm64 auto-resolve); README `[OPTIMIZATION]` group and `.api/api-ortools.md` authored.
[COMPUTE_QUANTITY_SPECTRAL_BRDF_PORT]-[COMPLETE]: three Compute-internal owners realized — `Symbolic/units` `QuantityFamily.Illuminance`, `Tensor/blas` `LevenbergMarquardt`/thin-QR, `Model/inference` ONNX spectral run; strata-corrected (Materials admits in-folder, no AEC->app-platform reference); Ripple: csharp:Rasm.Materials `[SPECTRAL_REFLECTANCE_GROUNDING]`/`[BRDF_FIT_COMPUTE_SEAM]`.
[COMPUTE_BSDD_PORT_TRANSPORT]-[COMPLETE]: `Runtime/channels` `BsddTransport.Fetch<TResponse>` issues `GET /api/Class/v1` under the hop deadline; Compute owns the channel, Bim owns the `BsddPort` interface + response projection + `LocalShape` degrade, app root closes `Fetch<BsddClassResponse>`; Ripple: csharp:Rasm.Bim `[PROPERTY_TEMPLATE_RESOLUTION]`/`[T-BSDD-TRANSPORT]`.
[COMPUTE_IFCTESTER_ORACLE_RPC]-[COMPLETE]: `Runtime/codecs` `IdsAuditRequest`/`IdsVerdict` adds one ifctester invocation beside `IfcConvert` over the settled TWO_HOP companion rpc, projecting the per-spec GlobalId-plus-facet verdict back into the Bim `IdsAudit`; Ripple: csharp:Rasm.Bim `[T-IDS-IFCTESTER-COMPANION]`.
[COMPUTE_UNITS_CUTPARAM_INGRESS_CONTRACT]-[DROPPED]: the cross-strata peer-export it was authored for is void (AEC->app-platform downward edge); the Compute-internal `CutParameterIngress` canonicalization is already stated on `Symbolic/units` with no further note needed, and Fabrication resolved in-folder with no counterpart card.
[STRUCTURE_PROHIBITIONS_BLOCK]-[COMPLETE]: `ARCHITECTURE.md` `[04]-[SEAM_PROHIBITIONS]` + `[05]-[PROHIBITIONS]` blocks promote the per-page rejected-forms into spine law, citing the real next-free `ComputeFault` 2213 (`CacheCorrupt` 2212) and the row-or-case-on-existing-owner invariant.
[DEEPEN_DEVICE_GEMM_SPMV]-[COMPLETE]: `Tensor/dispatch#DEVICE_KERNELS` `DeviceKernels` registry lowers MatMul/Conv*/pool/SpMV to WGSL `ComputePipeline` dispatch behind the residency gate and a winning `BenchmarkRow`, mirroring `KernelLowering`'s table shape (never the CPU delegate tables).
[COLLAPSE_SUBSTRATE_RESIDENCY_LATTICE]-[COMPLETE]: device-ness is the one `Substrate.DeviceWgpu` row (fallback-successor `cpu-tensor`, sheddable, `OrtResidency.DeviceResident` discriminant) on the existing `SubstrateSelection` fold + `ShedVerdict` seam — no `DeviceSelection`/second `SelectionReceipt`; Ripple: csharp:Rasm.AppHost `[ONE_DEGRADATION_SHED_VERDICT]`.
[DEEPEN_SPARSE_SPMM_CONTRACT]-[COMPLETE]: `Tensor/factor#SPARSE_ALGEBRA` `SparseTensorOpFamily` spmv/spmm/add/scale/transpose/kronecker/contract rows over MathNet `SparseMatrix` and CSparse `CoordinateStorage`, one fold on the one CSR storage.
[DEEPEN_EINSUM_PLANNER]-[COMPLETE]: `Tensor/factor#SPARSE_ALGEBRA` `EinsumPlan.Of` parses the subscript spec, derives a greedy minimum-intermediate pairwise order, and lowers each binary step to one MatMul (dense) or Contract (sparse) row under `Contract`.
[DEEPEN_CSPARSE_QR_LEAST_SQUARES]-[COMPLETE]: `Tensor/factor` `FactoredOp.Solve` routes a rectangular `Qr` operator to the SparseQR least-squares `min‖Ax−b‖` over the `SolutionDim`-sized buffer, witnessing the true residual against the ORIGINAL rectangular A.
[DEEPEN_FORWARD_JVP_COVERAGE]-[COMPLETE]: `Tensor/dispatch#EQUIVALENCE_INTEROP` `Forward` owner binds the MatMul two-tangent pushforward, the SoftMax `J·t`, and the geometry forward `Apply`, so every `DifferentiableOp.Jvp` is `Some` and `AdjointMode.Forward` is total — `<no-forward-jvp>` unreachable on a bound row.
[DEEPEN_HESSIAN_VECTOR_PRODUCT]-[COMPLETE]: `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.Hvp(tape, primalSeed, vector) = Chain(tape, Pushforward(tape, vector))` is the matrix-free forward-over-reverse Hv the optimizer Newton-CG and the FEM constitutive/contact tangents consume — no second tape.
[DEEPEN_SPARSE_JACOBIAN_COLORING]-[COMPLETE]: `Tensor/dispatch#EQUIVALENCE_INTEROP` `JacobianColoring` greedy distance-1 degree-ordered coloring recovers the Jacobian in (#colors) directional sweeps into CSR `CoordinateStorage`, falling through to per-column AD below the threshold.
[DEEPEN_PLASTICITY_HYPERELASTIC_STRESS_UPDATE]-[COMPLETE]: `Solver/contract#CONSTITUTIVE` `ConstitutiveModel` plastic(return-map)/hyperelastic/viscoelastic/damage cases each carry their strain-energy tape; `StressUpdate` reads stress via reverse VJP and the consistent tangent via the AD HVP — no hand-coded D-matrix.
[DEEPEN_FRICTIONAL_CONTACT_ENFORCEMENT]-[COMPLETE]: `Solver/contract#CONSTITUTIVE` `ContactConstraint` NodeToSurface/Mortar reuse the optimizer `ConstraintHandling.AugmentedLagrangian` `λ←λ+ρ·g` update, contact stiffness from the HVP of the regularized gap potential, broad-phase pairs from the Clash lane.
[DEEPEN_GLM_REGRESSION_ESTIMATORS]-[COMPLETE]: `Stats/estimator` `Estimator.Regression` ols/ridge (QR least-squares) / lasso (coordinate descent) / GLM (IRLS over `LinkFunction`) on one Fit/Predict contract and one `EstimatorModel` union.
[DEEPEN_CLUSTERING_DIMREDUCTION]-[COMPLETE]: `Stats/estimator` `Reduction`/`Cluster`/`Classify` pca(SVD)/kernel-pca/nmf/kmeans/gmm/dbscan/hierarchical/knn/svm/naive-bayes rows on the same Fit/Predict contract, factorized rows on the dense SVD, iterative/density rows on their kernels.
[DEEPEN_HYPOTHESIS_TIMESERIES]-[COMPLETE]: `Stats/estimator` `StatisticalTest` t/welch/anova/chi-square/ks/mann-whitney reading the matching `Distributions` CDF, and `TimeSeriesModel` ar(QR)/arma(MLE)/exponential-smoothing/state-space on the Estimator contract.
[DEEPEN_SIGNAL_SPECTRAL]-[COMPLETE]: `Stats/signal` `SpectralTransform` fft/ifft/rfft/stft/spectrogram/welch-psd/dwt over MathNet `IntegralTransforms.Fourier`+`Window`, and `FilterDesign` FIR(Conv1D)/IIR(direct-form-II + filtfilt) with two application mechanisms on one design axis.
[FILL_UNCERTAINTY_UQ]-[COMPLETE]: `Solver/uncertainty` realizes each `UncertaintyMethod` kernel — LHS stratification, subset conditional levels, FORM analytic β, Saltelli A/B/AB Sobol, Morris μ*, Wiener-Askey PCE closed-form moments/Sobol, SORM Breitung — one `Propagate` fold over the `LowDiscrepancy` design, never a per-call `System.Random`.
[DEEPEN_MIP_CPSAT]-[COMPLETE]: `Solver/optimizer` `cp-sat`/`milp`/`multi-start-global`/`robust-minimax` rows on the `Optimizer.Steps` fold — OR-Tools `CpModel`/`CpSolver`/`LinearSolver.Solver` via the typed model-builder API, robust over the UQ scenario set with the surrogate-gated inner reliability.
[DATA_GEOARROW_GLB]-[COMPLETE]: `Runtime/codecs` `GeoArrowBuffer.ToImportedGeometry` decodes the data-companion column-major coordinate/offset buffers into the GLB tessellation wire vertex/index spans on the existing companion channel, no second spatial codec; Ripple: python:data `[GEOSPATIAL_INGRESS_DEEPEN]`.
