# rebuild.js — implementation notes (blueprint transcription deltas)

The engine at `.claude/workflows/rebuild.js` (566 LOC) realizes `blueprint.md` in full. Nine minimal deltas, none re-litigating a ruling:

1. **Targetless language derivation** — `{brief, leg}` with no `targets` derives `LANG_KEY` from the `RASM-(CS|PY|TS)-` brief-name token; explicit targets always win. The blueprint mandates the targetless form but names no language source.
2. **Acceptance page binding** — `acceptancePrompt(ACCEPT, PAGES)` embeds the landed page list; the blueprint's `acceptancePrompt(ACCEPT)` gave the agent no way to resolve "the pages landed this run".
3. **`MODE.folder.conceptSource`** drops its trailing "and its nearest sibling pages" — the discover task already appends "plus its nearest sibling pages"; verbatim binding duplicated the clause.
4. **LANG `illusion` rows** drop their trailing "(a phantom)" — STANCE wraps the binding as `a phantom (<illusion>)`; the tail doubled the word.
5. **No `?.`/`??`** — the blueprint sketch uses optional chaining; realized with explicit guards, the sandbox-proven style of every existing workflow.
6. **Launcher-row normalization** — `args.riders`/`args.acceptance` bypass schema validation, so wave defaults to `0` and `needs` to `[]` at merge.
7. **Wave rows carry `deleted`** — delete-executor evidence stays in the run result (the old engine reported it; the blueprint's return shape dropped it).
8. **Unified batching** — discover rides the `IMPL_BATCH` chunking so dossier paths align per batch; `DISCOVER_BATCH` stays declared as the `[10]` policy constant.
9. **Plan prompt pins waves** — no-brief runs with `args.waves` get an `ARGS-PINNED WAVES` line; `normalizePages` remains the authoritative engine-side override.

Leg-4 invocation: `Workflow({ scriptPath: '.claude/workflows/rebuild.js', args: { brief: 'RASM-CS-GEOMETRY-DECISION.md', leg: 4 } })` — targets derive from the leg rows; a hand-copied target array also still works.
