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

[DATA_STUDY_INPUT]-[QUEUED]: compute/experiments + numerics consume the `data` tabular/gridded/spatial study inputs, mirroring the four study seams on both endpoints.
- Capability: compute/experiments reads the Hypothesis-checked DOE frame from `data/tabular/contract`, the `cubed.Array` plan plus `tensorstore` store from `data/gridded/store`, the Lance vector index over embedding columns from `data/tabular/multimodal`, and the numerics array seam aligns the `xrspatial` numba/scipy <3.15 band from `data/spatial/geospatial`.
- Shape: compute/experiments DOE study-input owner plus a compute numerics array seam, with `[SHAPE]`/`[PORT]` seam rows consuming `python:data/tabular/contract`, `python:data/gridded/store`, `python:data/tabular/multimodal`, and `python:data/spatial/geospatial`.
- Anchors: compute/experiments DOE study input, the compute numerics array seam, the `cubed.Array` plan, the Lance vector index, and the numba/scipy <3.15 band.
- Considerations: each of the four edges mirrors its `data`-side counterpart with the matching glyph; the cubed/tensorstore plan stays lazy across the seam, and the scipy <3.15 pin tracks the `xrspatial` numba band rather than a compute-local constraint.
- Ripple: `python:data` `[CONTRACT_GATE_FOLD]` — the Hypothesis-checked DOE frame is the primary study-input seam; the same task also mirrors `[CUBED_LINALG_DEEPEN]` (cubed plan), `[MULTIMODAL]` (Lance vector index), and `[GEOSPATIAL_TERRAIN_GATED]` (numba/scipy band).

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
