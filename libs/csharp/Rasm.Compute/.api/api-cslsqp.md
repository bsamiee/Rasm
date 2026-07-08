# [RASM_COMPUTE_API_CSLSQP]

`cslsqp` is the pure-C# span-based SLSQP (sequential least-squares quadratic programming) solver — the smooth constrained-NLP engine behind the `Solver/optimizer#OPTIMIZER_LANE` `OptimizerKind.slsqp` row for sizing/calibration problems that are neither topology-adjoint nor MILP. It is the matched Oberbichler pair with `HyperJet`: the row's exact gradient arrives through the `SensitivityLaw` hyper-dual leg (`DesignProblem.Smooth`), the honest central-difference fall standing only for black-box oracles.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cslsqp`
- package: `cslsqp` (SOURCE-VENDORED from the live `oberbichler/cslsqp`; feed-verified ABSENT from public NuGet — vendored source is the ruled admission form until its release lands)
- license: ISC
- assembly: vendored source compiled in-tree
- namespace: `CsLsqp`
- rail: optimizer
- verification: authored-at-admission against the repo surface; exact member signatures VERIFY against the vendored tree at the first compose (the vendored-source law), never asserted green before the tree lands

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]      | [RAIL]    | [CAPABILITY]                                                                 |
| :-----: | :------------ | :-------- | :--------------------------------------------------------------------------- |
|  [01]   | `SlsqpSolver` | optimizer | the span-based SQP driver: bound + inequality-constrained smooth NLP minimization over caller-supplied objective and gradient delegates; iteration cap and accuracy knobs |

## [03]-[IMPLEMENTATION_LAW]

[STACKING]:
- `Solver/optimizer#OPTIMIZER_LANE`: the `slsqp` `OptimizerKind` row — continuous `DesignVariable` rows only (a mixed-integer problem stays on the cp-sat/milp rows), the gradient from the `DesignProblem.Smooth` hyperdual objective through `SensitivityLaw.Gradient` where authored, the central-difference fall for black-box oracles, the result harvested onto the one `ParetoFront`/`KernelRun` shape every row shares.

[RAIL_LAW]:
- Owns: the smooth constrained-NLP (SQP) kernel — one row on the one `OptimizerKind` axis
- Accept: a continuous sizing/calibration NLP with bounds and inequality constraints and a smooth hyperdual-authored objective where the objective owner supplies sensitivities
- Reject: a second gradient mechanism beside the `Sensitivity` family; a mixed-integer lowering (the exact rows own it); a NuGet pin before the upstream release lands (the vendored tree is the source of truth)
