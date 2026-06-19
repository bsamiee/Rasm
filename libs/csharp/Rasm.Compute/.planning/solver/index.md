# [COMPUTE_SOLVER]

The solver lane sub-domain index: the physics-solve concern fans out across five owners, each a separate design page so a reader navigates the sub-domain without scanning one god page. Discretization owns the volumetric mesher and the element/quadrature vocabulary; the solve contract owns the physics×BC×element assembly fold and the multi-physics coupling; the optimizer owns the design-space search axis with its reduced-order surrogate duality; the sweep governor owns the N-dim DOE orchestration with the frame-budget early-stop; and the clash compute owns the acceleration-structure collision fold with the ROM digital-twin loop. The five pages compose the `numeric#DENSE_ALGEBRA`/`numeric#SPARSE_SOLVE` factorization machinery, the `tensors#EQUIVALENCE_INTEROP` gradient-adjoint tape, the `WorkLane`/`LaneRuntime` scheduler, the `ComputeReceipt` rail, and the Persistence artifact and vector indexes as settled vocabulary.

## [1]-[PAGES]

| [INDEX] | [PAGE]                                  | [OWNS]                                                                         |
| :-----: | :-------------------------------------- | :---------------------------------------------------------------------------- |
|   [1]   | [discretization](discretization.md)     | Volumetric mesher; tet/hex/boundary-layer; shape-function/quadrature; metric  |
|   [2]   | [solve-contract](solve-contract.md)     | Physics×BC×element solve axis; transient/nonlinear; multi-physics; recovery   |
|   [3]   | [optimizer](optimizer.md)               | Design-var/link/conditional search; constraint axis; ROM/GP surrogate duality |
|   [4]   | [sweep](sweep.md)                       | N-dim DOE sweep grid; frame-budgeted early-stop; Morris/Sobol sensitivity     |
|   [5]   | [clash](clash.md)                       | Acceleration-structure collision compute; Kalman-banded ROM digital-twin loop |
|   [6]   | `uncertainty` (planned)                 | Forward-UQ propagation; PCE/MC/reliability; Sobol variance; failure-probability β |

## [2]-[SPINE]

```mermaid
flowchart LR
    MeshSpace -->|Discretize| Discretization
    Discretization -->|DiscreteMesh / FieldSpace| SolveContract
    SolveContract -->|SolveResult / adjoint tape| Optimizer
    Optimizer -->|evaluate oracle| Sweep
    SolveContract -->|field artifact| Sweep
    Optimizer -->|Surrogate baseline| Clash
    Discretization -.->|Fin fail| ComputeFault
    SolveContract -.->|Fin fail| ComputeFault
    Optimizer -.->|Fin fail| ComputeFault
```

Discretization emits the `DiscreteMesh` and `FieldSpace` the solve contract assembles over; the solve contract emits the `SolveResult` field and the `tensors#EQUIVALENCE_INTEROP` adjoint tape the optimizer gradient-adjoint row reads; the optimizer's `evaluate` oracle drives the sweep's DOE fan-out and the optimizer's `Surrogate` is the clash digital-twin baseline; every owner aborts on the `ComputeFault` rail.
