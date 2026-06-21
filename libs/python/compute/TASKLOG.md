# [PY_COMPUTE_TASKLOG]

The open and closed work distilled from `IDEAS.md`. `[1]-[OPEN]` carries task cards whose leader holds a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — and three to four scoped bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. `[2]-[CLOSED]` carries `[COMPLETE]` and `[DROPPED]` items. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

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

[DATA_STUDY_INPUT]-[QUEUED]: compute/experiments + numerics consume the two settled `data` study-input seams in-pass; the multimodal and gated-terrain edges stay blocked on their `data`-side QUEUED counterparts.
- Capability: `experiments/study.md#STUDY` reads the Hypothesis-checked DOE frame from the now-`COMPLETE` `data/tabular/contract` `[CONTRACT_GATE_FOLD]`, and the `cubed.Array` plan plus `tensorstore` store from the now-`COMPLETE` `data/gridded/store` `[CUBED_LINALG_DEEPEN]` — both ready seams authored this pass as `[SHAPE]`/`[PORT]` rows on the study-input and `numerics/array` operands; the Lance multimodal vector index and the `xrspatial` numba/scipy <3.15 terrain band remain blocked behind their open `data`-side cards.
- Shape: the compute/experiments DOE study-input owner and the `numerics/array.md#PAYLOAD` array seam consume `python:data/tabular/contract` and `python:data/gridded/store` now (lazy `cubed`/`tensorstore` plan preserved across the seam); the `python:data/tabular/multimodal` and `python:data/spatial/geospatial` rows stay declared-but-blocked until `data` `[MULTIMODAL]` and `[GEOSPATIAL_TERRAIN_GATED]` land.
- Anchors: `experiments/study.md#STUDY` DOE study input, the `numerics/array.md#PAYLOAD` seam, the `cubed.Array` plan, the `tensorstore` store, `data/tabular/contract`, `data/gridded/store`.
- Considerations: the two settled edges mirror their `data`-side counterparts with the matching glyph this pass; the cubed/tensorstore plan stays lazy across the seam, and the scipy <3.15 pin on the blocked terrain edge tracks the `xrspatial` numba band rather than a compute-local constraint.
- Ripple: `python:data` `[CONTRACT_GATE_FOLD]` (settled — primary study-input seam) and `[CUBED_LINALG_DEEPEN]` (settled — cubed plan); the still-blocked edges pair with `python:data` `[MULTIMODAL]` (Lance vector index) and `[GEOSPATIAL_TERRAIN_GATED]` (numba/scipy terrain band).

[RANDOM_SEED_OWNER]-[QUEUED]: one numerics-owned randomness-provenance reconciler over the numpy `Generator` and the jax `PRNGKey` seed algebras.
- Capability: a `numerics`-owned seed/key reconciliation surface that derives both a `numpy.random.Generator` and a `jax.random.PRNGKey` from one content-keyed seed, so a DOE sampler, a stochastic SDE solve, a Bayesian draw, and an active-learning batch share one reproducible randomness provenance recorded on the receipt rather than each owner seeding independently.
- Shape: a randomness-provenance value object keyed through `ContentIdentity.of` over the seed material, projecting a `default_rng(seed)` for the numpy floor consumers and a `jax.random.key`/`split` lineage for the JAX consumers, folding the seed digest into the receipt so two runs from the same seed key identically.
- Anchors: `numpy` (`random.default_rng`, `random.Generator`, `SeedSequence`), `jax` (`random.key`, `random.split`, `random.PRNGKey`), `experiments/study.md#STUDY` (the `qmc`/`default_rng` consumer), `solvers/differential.md` (the SDE Brownian-path seed), `experiments/inference.md#BAYESIAN` (the sampler seed), runtime `ContentIdentity`.
- Considerations: the numpy `Generator` floor runs on cp315 unconditionally; the jax-key leg rides the gated `python_version<'3.15'` band, so the reconciler resolves the numpy seed always and the jax key only where jaxlib resolves; the seed digest is the reproducibility provenance the receipt carries, never a hidden global RNG state.

[ARRAY_LAYOUT_GRADUATION]-[QUEUED]: a deferred cross-backend bit-identity proof that an array round-trips through the numerics layout admission unchanged across backends.
- Capability: a graduation-gated proof that `numerics/array.md#PAYLOAD`'s namespace-dispatched admission produces a bit-identical layout across the Array-API backends — a numpy array, its dask/jax/sparse peer, and the round-tripped result share one `ContentIdentity` digest, so a layout graduation admits only when the cross-backend digest matches; this is the array-side feed `graduation/handoff.md#GRADUATION` already names as the fourth `_admit` rejection clause (an array layout that fails bit-identical reproduction) but `array.md` does not yet emit.
- Shape: a layout-parity verdict folded into the array admission's graduation path — `ContentIdentity.of("array", host.tobytes(), IdentityPolicy())` over the canonical contiguous buffer, computed on each admitted backend and compared, the parity verdict riding the same residual-over-ceiling `GraduationReceipt.graduates`/`_admit` fold the `solvers/receipt`, `optimization/convex`, and `experiments/inference` owners feed on their own `HandoffAxis` cases.
- Anchors: `numerics/array.md#PAYLOAD` (the namespace-dispatched `ArrayPayload.admit`, the dtype/shape/`FinitePolicy`/`ContentIdentity` checks), `array-api-compat` (`array_namespace`/`device`/`to_device`, the cross-backend namespace), `numpy`/`dask`/`jax`/`sparse` (the admitted backend peers `array_namespace` resolves), `graduation/handoff.md#GRADUATION` (the `array_layout` `HandoffAxis` case and the `GraduationReceipt._admit` parity fold), runtime `ContentIdentity`.
- Considerations: the numpy floor and dask admission run on cp315; the jax and pydata-`sparse` cross-backend legs ride the gated `python_version<'3.15'` band, so the full bit-identity proof is deferred until those backend wheels resolve — the numpy-floor leg proves self-round-trip parity now, the cross-backend digest match graduates later.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[INTERPAX_QUADAX_USAGE]-[DROPPED]: redundant — the `quadax` adaptive/sampled quadrature family and the `interpax` differentiable interpolants are already landed transcription-complete on `solvers/quadrature.md#QUADRATURE` (the `QuadKind`/`_QUADAX_ENTRY` rule table and the `InterpKind` `interpax` interpolant floor); the card also mis-filed the interpax interpolation half onto `solvers/field.md`, the FEM DOF-vector interpolate/project owner that owns no 1-D interpolation. No new fence to author.
[CONVEX_PARAM_SWEEP]-[DROPPED]: redundant — the DPP `cp.Parameter` warm-re-solve sweep is already landed on `optimization/convex.md#CONVEX` (the `ParamBind` type, the `_NO_BIND` anchor, the `params: ParamBind` factory parameter on every case, the `warm_start=True` Clarabel re-solve loop, the per-bind `ConvexReceipt` fold, and the `ContentIdentity.of` bind keying). The card was a usage/campaign exercise of the existing fence, not a fence diff.
[BLACKJAX_RAW_ALGEBRA]-[DROPPED]: out of charter — `experiments/inference.md#BAYESIAN` is bounded at conjugate/hierarchical/GLM gradient MCMC and explicitly names the raw `blackjax` kernel algebra and variational inference among its deleted forms ("variational, normalizing-flow, and neural-posterior estimation never enter it"; the three accelerated engines route through PyMC's own `nuts_sampler` dispatch, never a re-driven kernel). The SMC/SGMCMC/VI route is a boundary decision the inference page owns, not an open compute task; reopen only as an admitted distinct sub-domain, never a second sampler surface beside the PyMC owner.
