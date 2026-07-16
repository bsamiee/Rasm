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

[STATS_MODEL_SELECTION]-[QUEUED]: Model-selection surface — information criteria, hyper-parameter paths, and a candidate chooser over the estimator lane's landed `Validate` axis.
- Capability: selection beyond held-out scoring — information criteria over fitted likelihoods, hyper-parameter path evaluation (penalty strength, kernel width, cluster count), and a chooser folding `Validate` scores across candidate `EstimatorPolicy` rows into one ranked verdict.
- Shape: a selection fold on `Stats/estimator` `EstimatorFold` beside `Fit`/`Predict`/`Validate` — candidates as policy rows, never a sibling trainer or a grid-search service.
- Unlocks: defensible-by-default classical fits; the C# half of the graduation-evidence contract gains the selection discipline the Python companion carries.
- Anchors: `Stats/estimator#ESTIMATOR_LANE` `Validate` (k-fold and forward-chain scoring landed), `EstimatorPolicy` admitted ranges, the graduation-evidence axis demanding quantified generalization.

[SOLVER_ELEMENT_QUANTIFIED_RULES]-[QUEUED]: Graph-exhaustive rule grounding — `ComplianceRule` templates ground over every `ElementGraph` node-class member with a coverage proof.
- Capability: a node-class selector deriving `RuleGrounding` populations from the concrete graph plus a population fold proving every matching member instantiated, verdict/witness/unsat-core keyed per element.
- Shape: a grounding derivation on `Solver/satisfy#RULE_SATISFACTION` consuming the graph the assessment spine already routes — the template quantifies, the selector proves exhaustiveness, caller-supplied rows remain the manual lane.
- Unlocks: satisfy upgrades from caller-assembled populations to a whole-building code audit whose unsat core names the exact failing elements ("every egress door", "each lateral-system member").
- Anchors: `ComplianceRule`/`RuleGrounding` template quantification with name@element tracking literals (landed), assessment-spine per-node fact routing, `Analysis` runners reading the concrete `Rasm.Element` `ElementGraph`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[SOLVER_GEOTECHNICAL_CONSTITUTIVE]-[COMPLETE]: pressure-dependent frictional soil laws landed collapsed — `Solver/constitutive` `PlasticPotential` parameterizes `DruckerPrager`/`SmoothedMohrCoulomb`/`ModifiedCamClay` as seed data over one invariant generator, and `MaterialState` carries volumetric hardening, preconsolidation pressure, and pore pressure.

[SOLVER_ARC_LENGTH_CONTINUATION]-[COMPLETE]: `Solver/contract` `SolveMethod.ArcLength` plus `ArcLengthPolicy` enforce the Crisfield displacement/load constraint through predictor-corrector iterations across limit points on the landed Newton internal-force machinery.

[ST-FDD]-[COMPLETE]: `Stats/signal` `Transform.Modal` runs the N-channel frequency-domain decomposition over Welch cross-PSD matrices, returning `ModalEstimate`/`MeasuredMode` with the full singular spectrum; `MeasuredMode` crosses to `Solver/clash#CLASH_AND_TWIN` as the FE-updating measured end.
