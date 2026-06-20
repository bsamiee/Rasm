# [COMPUTE_TASKLOG]

The open and closed work for measured execution, distilled from `IDEAS.md`. Each open task carries a status marker and the capability-to-build, packages, integration points/boundaries, and key considerations; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup and the residual live-host probes whose owner shape is complete.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
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

(none)
