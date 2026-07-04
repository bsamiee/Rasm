# [GEOMETRY_CAMPAIGN_RUN_LEDGER]

Run ledger for the RASM-CS-GEOMETRY rebuild legs. Resume is launching-session-only; a brief edit means a fresh run, never a resume.

## Leg 1 — FLOOR + CONTRACTS — CLOSED

- Run `wf_e5cec0fa-836`: 14 agents, 0 errors; 9 pages (1 new, 5 rebuild, 3 improve). Landed at commit `7c0c9d30`.
- Its 3 `hard_residual` items were forward-leg respell advisories (self-keyed W2/W3/W4 → DECISION rows 7-9 / 11-16 / 17-28) naming only leg-2/3/4 targets — SKIPPED per policy; each leg's Discover re-derives the drift from disk. Full text: the run journal return (`subagents/workflows/wf_e5cec0fa-836/journal.jsonl`).

## Leg 2 — MESHING LATTICE — CLOSING VIA SALVAGE

- Run `wf_219d9ec6-cc4` DIED at redteam on credit exhaustion (never resume it): Plan/Discover/Implement (all 5 pages) + Critique (both batches) + redteam on `repair.md` COMPLETED; the 4-page meshing redteam died mid-write on `delaunay.md`.
- SALVAGE (user-ruled): ONE direct fable redteam agent over the 4 meshing pages, engine semantics, kernel-wide fix authority; RECONCILE SKIPPED by operator ruling. On its return: `git add -A` + commit = leg 2 CLOSED.
- Leg 3 launches ONLY on user signal (auto-chain suspended for this handoff).

## Leg 3 — PROCESSING + SOLVING — CLOSED

- Run `wf_ce8d239c-260`: 10 agents, 0 errors; `Solving/` minted (solver + fit new-with-absorb, both `Processing/` twins deleted after verified capture), receipts/repair/decimate/flatten rebuilt. First reconcile-free leg — ripples repaired in-pass. Committed at leg-close save point.

## Leg 4 — RUNNING (DECISION `[10]` row 4) — REDESIGNED ENGINE

- Run ID: `wf_4668d811-66c` (task `wx387w77l`). First launch (`wf_9cd7ee6c-35a`) no-opped: the targetless `{brief, leg}` plan read the leg clause as a FILTER over an empty target expansion — fixed by making the leg rows the page-set SOURCE (legRead + planPrompt empty-TARGETS clause; parse-verified). Dead run never resumed.
- Invocation: `Workflow({scriptPath: ".claude/workflows/rebuild.js", args: {brief: "RASM-CS-GEOMETRY-DECISION.md", leg: 4}})` — targets derive from the leg row itself; kinds re-derived from disk (9 new / 3 rebuild — the DECISION census was corrected on disk).
- The new engine executes leg 4's full rider layer natively: GShark props/csproj drop + `api-gshark.md` deletion in the serialized tail, `guardPage`-gated on `Parametric/nurbs.md` landing; FAB counterpart edits + AppHost signature-lock verify receipt-forced on their owning pages; the four `[05]` acceptance dry-runs in the read-only Close agent; `nurbs` batched before its consumers by dependency order.
- DECISION-side handoff defects (engine never edits briefs; fix before or after leg 4 by operator call): stale `[10]:262` reconcile sentence, row-4 kind census 8/4 vs disk 9/3, stale `Rasm.csproj:18` anchor.

## POST-LEG-4, PRE-LEG-5 — NAMESPACE ALIGNMENT FIX (user-ruled, error-grade)

- `.editorconfig:24` = `dotnet_style_namespace_match_folder = true:error`; `Rasm.Rhino` source proves the convention (`namespace Rasm.Rhino.Blocks;`). The kernel ARCHITECTURE `[110]` namespace matrix (`Rasm.Vectors` et al. spanning foreign folders) and the DECISION's `Rasm.Geometry.*` vocabulary are BOTH non-compilable fictions — no doc can legalize them.
- RULING: namespace = folder path. Kernel pages declare `Rasm.<Folder>` (Numerics/Spatial/Meshing/Parametric/Processing/Solving/Drawing/Analysis/Domain). `Rasm.Vectors` + `Rasm.Geometry.*` die everywhere, including cross-package consumers (Bim/Compute/Element/Fabrication/Materials cite them).
- EXECUTION (the moment leg 4 returns, BEFORE leg 5): ONE fable fixer consuming the four vectors-probe censuses (`.claude/scratch/vectors-probe/`) — re-declare every kernel page namespace to folder truth, re-point every `using` kernel-wide + cross-package, DISSOLVE the ARCHITECTURE namespace matrix into one line citing the editorconfig rule, strip all namespace-mapping prose litter from ARCHITECTURE/README/`.api` catalogs. Leg-5 finalize cold-checks it.
- The probes' "legitimate per matrix" verdicts are OVERRIDDEN — the error-level analyzer rule outranks the matrix; only their mention censuses are consumed.
- CENSUS COMPLETE (4 dossiers in `.claude/scratch/vectors-probe/`): 102 mentions → 83 settled-legitimate (per the old matrix), 14 provisional (leg-4 in-flight: `curve.md` ×12, `view.md`, `pack.md`), ZERO dead types, ZERO signature drift across ~15 verified consumer signatures. Anchored defects for the fixer beyond the rename itself: (1) `Numerics/predicates.md:21,31` unused `using Rasm.Vectors;` + `Point3d` misattributed (it is `Rhino.Geometry`) — strike or wire the `AtomProjection` owner; (2) `Domain/validation.md:172` retired names `SpatialHit`/`SpatialPair` → live `NeighborHit`/`NeighborPair`; (3) the 5 Spatial pages never declare their namespace in a fence (folder-truth rename resolves by construction). Watch-items on leg-4 output: `curve.md:32` using may touch only Rhino-carried atoms; `view.md:18` Packages bullet misgroups `MeshEdit` (owner: `Meshing/edit.md`). Timeline fact: the old SOURCE `Rasm/Vectors/` was folder-true and compilable; the kernel conversion (`741ea2c7f`) kept the namespace while scattering pages — that is when the doctrine became non-compilable fiction.

## Leg 5 — FINALIZE (our own, post leg-4 + namespace fix)

- `Workflow(finalize.js, {targets: ["libs/csharp/Rasm/.planning/Analysis", "libs/csharp/Rasm/.planning/Domain", "libs/csharp/Rasm/.planning/Meshing", "libs/csharp/Rasm/.planning/Numerics", "libs/csharp/Rasm/.planning/Parametric", "libs/csharp/Rasm/.planning/Processing", "libs/csharp/Rasm/.planning/Spatial"]})` — the user-named 7 folders; `Drawing/`, `Solving/` (exists after leg 3), and `Vectors/` are addable at launch by widening the targets array (user call).
- Shape: strata-derived folder order → per-page opus discovery (page + related + csproj/README + both `.api` tiers; underutilized/hand-rolled/phantom/split-brain census) → ONE fable fixer per folder (docs/stacks/csharp ROOT pages only) → downstream residual routing → terminal sweep. Corrections/closure, not rebuild.
- After leg 5: cold re-runs of legs across sessions until a pass finds nothing → geometry pair retires to `.archive/`.
