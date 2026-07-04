# [GEOMETRY_CAMPAIGN_RUN_LEDGER]

Run ledger for the RASM-CS-GEOMETRY rebuild legs. Resume is launching-session-only; a brief edit means a fresh run, never a resume.

## Leg 1 — FLOOR + CONTRACTS

- Run ID: `wf_e5cec0fa-836` (task `w0738e589`, cancelled at Discover; RESUMED as task `wnn6jcp72` — plan replayed from cache, discover onward live)
- Script: `.claude/workflows/rebuild.js` (engine state as of commit `a89e8aa5`)
- Brief: `RASM-CS-GEOMETRY-DECISION.md` (pre-leg `[09]` roster motion EXECUTED at `a89e8aa5`; legs verify, never re-apply)
- Args: `{targets: [Numerics/predicates.md, Numerics/faults.md, Spatial/index.md, Spatial/naming.md, Spatial/reconciliation.md, Meshing/edit.md, Processing/repair.md, Drawing/pack.md, Processing/solver.md] (libs/csharp/Rasm/.planning/-prefixed), brief: RASM-CS-GEOMETRY-DECISION.md}`
- Expected kinds: rebuild ×5, new ×1 (edit), improve ×3 (repair `MeshEdit` donor excision ONLY; pack + solver phantom kills ONLY)
- Resume: `Workflow({scriptPath: ".claude/workflows/rebuild.js", resumeFromRunId: "wf_e5cec0fa-836"})`
- On return: SKIP the `hard_residual` list by default — the in-run reconcile fixed the real ones; only a genuinely real OUT-OF-SCOPE item gets ONE fable fix agent, launched only when its files cannot collide with a running workflow (collision risk = don't launch, forget the residuals; never chase noise).
- Then: empty `libs/csharp/Rasm/IDEAS.md` + `TASKLOG.md` to the `(none)` pattern → once the energy-exchange implementer has ALSO returned, `git add -A` + commit everything (user-authorized gate commit) → read `RASM-CS-GEOMETRY-BRIEF.md` → launch leg 2 fresh per DECISION `[10]` row 2 (repo-relative `libs/csharp/Rasm/.planning/...` targets; current self-contained engine, no residual step).

## Legs 2-4 — queued (DECISION `[10]` rows)

- Leg 2 MESHING LATTICE: delaunay, intersect, arrangement, offset + repair improve (`BooleanOp` excision)
- Leg 3 PROCESSING + SOLVING: Solving/solver + Solving/fit (new-with-absorb from Processing twins), receipts, repair, decimate, flatten
- Leg 4 NEW OWNERS + PARAMETRIC + DRAWING: slice, skeleton, remesh, nurbs, curve, surface, subdivide, develop, panelize, patternmap, view, pack — then the four acceptance dry-runs close the campaign (no settling workflows; legs re-run cold until a pass finds nothing)
