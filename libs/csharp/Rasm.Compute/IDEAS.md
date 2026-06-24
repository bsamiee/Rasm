# [COMPUTE_IDEAS]

The forward pool of higher-order concepts for measured execution, each grounded in the folder's domain and current platform capability — some deepen a thin owner into a fuller axis, others bind a concrete technique to a settled abstract surface. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

(none)

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[DEVICE_COMPUTE_SUBSTRATE]-[COMPLETE]: device-resident GPGPU substrate realized as `Tensor/dispatch#DEVICE_KERNELS` (`DeviceKernels` WGSL `ComputePipeline` registry + `DeviceDispatch` over the shared `ONE_WGPU_DEVICE`, `OrtResidency.DeviceResident` ingress/egress bridge, device `DeterminismTag`) and the `Runtime/admission` `Substrate.DeviceWgpu` row — a column on the existing axis, never a parallel device-tensor type; Ripple: csharp:Rasm.AppUi/Render `[CSG_SILHOUETTE]` shared device seam.
[SPARSE_TENSOR_ALGEBRA]-[COMPLETE]: full sparse-tensor algebra realized as `Tensor/factor#SPARSE_ALGEBRA` (`SparseTensorOpFamily` 8-row axis · `SparseTensorOps` fold over MathNet `SparseMatrix`/CSparse `CoordinateStorage` · `EinsumPlan` greedy pairwise contraction lowering to dense GEMM/sparse contract) over the one CSR storage, nnz-growth `AllocationClass` stamped.
[AUTODIFF_DUAL_MODE_ENGINE]-[COMPLETE]: dual-mode AD realized as `Tensor/dispatch#EQUIVALENCE_INTEROP` (`Forward` JVP owner total over MatMul/SoftMax/geometry, `SensitivityLaw.Hvp` forward-over-reverse, `JacobianColoring` greedy distance-1 into CSR) and `Solver/contract#CONSTITUTIVE` (`ConstitutiveModel` plasticity/hyperelastic/viscoelastic/damage + `ContactConstraint` whose consistent tangent is the AD HVP).
[STATISTICS_AND_LEARNING_LANE]-[COMPLETE]: Stats lane realized as `Stats/estimator` (one `Estimator`/`EstimatorModel` union over regression/GLM/PCA/clustering/classification + `StatisticalTest`/`TimeSeriesModel`) and `Stats/signal` (`SpectralTransform` FFT/STFT/PSD/wavelet + `FilterDesign` FIR/IIR), factoring through the dense `Factorization`/`IntegralTransforms` surface.
[UQ_METHOD_BREADTH]-[COMPLETE]: full UQ breadth realized on the `Solver/uncertainty` `UncertaintyMethod` 8-row axis — Saltelli A/B/AB Sobol, Morris μ* screening, Wiener-Askey PCE (Hermite/Legendre/Laguerre/Jacobi) closed-form moments, SORM Breitung curvature over the AD HVP — one `Uncertainty.Propagate` fold over the shared `evaluate` oracle.
[OPTIMIZATION_DEPTH]-[COMPLETE]: optimization category completed on the `Solver/optimizer` `Optimizer.Steps` fold — `cp-sat`/`milp` lower the typed `DesignProblem` to OR-Tools `CpModel`/`CpSolver`/`LinearSolver.Solver` via the model-builder API, `multi-start-global` LowDiscrepancy basin restart, `robust-minimax` over the UQ scenario set — one `Optimize` entry, no parallel MIP owner.
