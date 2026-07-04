# [GEOMETRY_CAMPAIGN_RUN_LEDGER]

Run ledger for the RASM-CS-GEOMETRY rebuild legs. Resume is launching-session-only; a brief edit means a fresh run, never a resume.

## Leg 1 — FLOOR + CONTRACTS — CLOSED

- Run `wf_e5cec0fa-836`: 14 agents, 0 errors; 9 pages (1 new, 5 rebuild, 3 improve). Landed at commit `7c0c9d30`.
- Its 3 `hard_residual` items were forward-leg respell advisories (self-keyed W2/W3/W4 → DECISION rows 7-9 / 11-16 / 17-28) naming only leg-2/3/4 targets — SKIPPED per policy; each leg's Discover re-derives the drift from disk. Full text: the run journal return (`subagents/workflows/wf_e5cec0fa-836/journal.jsonl`).

## Leg 2 — MESHING LATTICE — CLOSING VIA SALVAGE

- Run `wf_219d9ec6-cc4` DIED at redteam on credit exhaustion (never resume it): Plan/Discover/Implement (all 5 pages) + Critique (both batches) + redteam on `repair.md` COMPLETED; the 4-page meshing redteam died mid-write on `delaunay.md`.
- SALVAGE (user-ruled): ONE direct fable redteam agent over the 4 meshing pages, engine semantics, kernel-wide fix authority; RECONCILE SKIPPED by operator ruling. On its return: `git add -A` + commit = leg 2 CLOSED.
- Leg 3 launches ONLY on user signal (auto-chain suspended for this handoff).

## Leg 3 — PROCESSING + SOLVING — RUNNING

- Run ID: `wf_ce8d239c-260` (task `wi2kc0ii1`), RECONCILE-FREE engine (every pass repairs its own cross-file ripples; no residual machinery exists)
- Args: `{targets: [Solving/solver.md, Solving/fit.md, Processing/receipts.md, Processing/repair.md, Processing/decimate.md, Processing/flatten.md] (libs/csharp/Rasm/.planning/-prefixed), brief: RASM-CS-GEOMETRY-DECISION.md}`
- Expected kinds: new ×2 with absorb {Solving/solver ← Processing/solver}, {Solving/fit ← Processing/fit} + deletePages [Processing/solver.md, Processing/fit.md]; rebuild ×4
- Resume: `Workflow({scriptPath: ".claude/workflows/rebuild.js", resumeFromRunId: "wf_ce8d239c-260"})`
- On return: commit → leg 4 on user signal.

## Leg 4 — queued (DECISION `[10]` row 4) — RUNS ON THE REDESIGNED ENGINE

- Invocation: `Workflow({scriptPath: ".claude/workflows/rebuild.js", args: {brief: "RASM-CS-GEOMETRY-DECISION.md", leg: 4}})` — targets derive from the leg row itself; kinds re-derived from disk (9 new / 3 rebuild; the row's "8/4" census is a recorded DECISION defect).
- The new engine executes leg 4's full rider layer natively: GShark props/csproj drop + `api-gshark.md` deletion in the serialized tail, `guardPage`-gated on `Parametric/nurbs.md` landing; FAB counterpart edits + AppHost signature-lock verify receipt-forced on their owning pages; the four `[05]` acceptance dry-runs in the read-only Close agent; `nurbs` batched before its consumers by dependency order.
- DECISION-side handoff defects (engine never edits briefs; fix before or after leg 4 by operator call): stale `[10]:262` reconcile sentence, row-4 kind census 8/4 vs disk 9/3, stale `Rasm.csproj:18` anchor.

## Leg 5 — FINALIZE (our own, post leg-4)

- `Workflow(finalize.js, {targets: ["libs/csharp/Rasm/.planning/Analysis", "libs/csharp/Rasm/.planning/Domain", "libs/csharp/Rasm/.planning/Meshing", "libs/csharp/Rasm/.planning/Numerics", "libs/csharp/Rasm/.planning/Parametric", "libs/csharp/Rasm/.planning/Processing", "libs/csharp/Rasm/.planning/Spatial"]})` — the user-named 7 folders; `Drawing/`, `Solving/` (exists after leg 3), and `Vectors/` are addable at launch by widening the targets array (user call).
- Shape: strata-derived folder order → per-page opus discovery (page + related + csproj/README + both `.api` tiers; underutilized/hand-rolled/phantom/split-brain census) → ONE fable fixer per folder (docs/stacks/csharp ROOT pages only) → downstream residual routing → terminal sweep. Corrections/closure, not rebuild.
- After leg 5: cold re-runs of legs across sessions until a pass finds nothing → geometry pair retires to `.archive/`.
