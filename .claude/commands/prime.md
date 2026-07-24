---
description: Ground a session for planning-corpus work — topology, scaffold, planning law — then take or resume the objective
argument-hint: [target folder path(s) or a language (csharp|python|typescript); empty = branch-level grounding]
---

# [PRIME]

Ground this session for planning-corpus work, then take the objective. TARGET: $ARGUMENTS — folder paths deepen those folders at prime; a bare language word deepens that branch; empty grounds at branch level and defers folder deepening to the objective. Grounding is read-only and plan-mode-compatible; no work, no proposals, no workflow launch before the objective arrives — a session already carrying its objective grounds, then continues straight into it.

Frame: `libs/csharp`, `libs/python`, `libs/typescript` — building advanced parameterized, polymorphic lib-grade foundations, and the entire operating surface is DESIGN DOCS: every capability is a transcription-complete code fence inside a `.planning/` design page, rebuilt root-up without hesitation; no session lands source files.

Spend only on what the target or objective names; the whole prime stays under 100k tokens. Batch every multi-file read through one `tail -n +1` command, never per-file reads.

## [01]-[TOPOLOGY]

Run all four in one parallel block:
1. `tree -L 3 libs` — maps every branch and package with per-file size and modified-age columns: all docs as relative page weight in one view.
2. `fd -H -t d -d 3 '^\.(planning|api)$' libs` — censuses the scaffold: which folders carry a `.planning/` and which carry an `.api/` catalog tier.
3. `fd -d 1 -t f .` — root files, names only. Root file — central manifests, lockfiles, workspace/solution files, tool config — an owner to know exists.
4. `ls .claude/workflows/` — lists the workflow roster, names only. Each file's `meta` block states its own contract; a roster entry is read when the arrived objective names it, never at prime.

## [02]-[PLANNING_LAW]

READ 100%, one batch (`==> path <==` headers delimit files):

```bash copy-safe
fd -H -t f -e md -E 'IDEAS.md' -E 'TASKLOG.md' . libs/.planning libs/csharp/.planning libs/python/.planning libs/typescript/.planning docs/laws -X tail -n +1
```

This is the complete Tier-0 + branch law: `campaign-method.md` (the loop, the bar, the agent-role law, the two naivety axes, collapse-floor freedom), `ARCHITECTURE.md` (strata, dependency direction, wire seams), `README.md` (the authoring standard), `planning-targets.md` (the target index), and each branch router + `ARCHITECTURE.md`. `IDEAS.md`/`TASKLOG.md` never open at prime — card pools enter context only when a dispatched rail works them. `RULINGS.md` is law, never a card pool: the branch and cross-libs decision registries ride this batch and ground every session.

`docs/laws/` rides the same batch — `topology.md` binds counterpart obligations on every multi-surface edit, `patterns.md` and `scars.md` carry the cross-branch and regression law.

## [03]-[TARGET_DEEPENING]

Spent only on what TARGET or the arrived objective names — folder cores are never read wholesale; the branch law already maps every folder:
- One folder: READ `<pkg>/README.md` + `<pkg>/ARCHITECTURE.md` + `<pkg>/RULINGS.md` (where minted), then `loc <pkg>/.planning` (the page inventory with the thinness/complexity signal a bare `ls` hides) and `ls <pkg>/.api/`.

Bare-language TARGET: batch the branch's folder cores in one command:

```bash template
fd -H -t f --max-depth 3 -E '_tmp' -E '.planning' '^(README|ARCHITECTURE|RULINGS)\.md$' libs/<lang> -X tail -n +1
```

## [04]-[CLOSE]

State in a few lines what grounded: the branches and packages mapped, the planning law read, and whatever TARGET deepened. Then ask for the session objective, scope, and constraints — unless the objective already stands, in which case continue straight into it. Priming ends there — no findings, no proposals, no rail selection, no restated law.
