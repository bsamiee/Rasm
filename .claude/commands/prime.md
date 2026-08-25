---
description: Ground a session for planning-corpus work — topology, scaffold, planning law — then take or resume the objective
argument-hint: [target folder path(s) or a language (dotnet|python|typescript); empty = branch-level grounding]
disable-model-invocation: true
---

# [PRIME]

Branch law and target-owned surfaces ground planning-corpus context before the objective. `TARGET: $ARGUMENTS` deepens each folder path or named language branch; empty input grounds branch law and leaves folder selection to the objective. Grounding is read-only and plan-mode-compatible. Without an objective, grounding stops; otherwise work continues into the objective.

`libs/dotnet`, `libs/python`, and `libs/typescript` are design-only foundations. Transcription-complete fences on `.planning/` pages carry every parameterized, polymorphic library capability; rebuild them root-up and leave source files untouched.

Grounding stays below 100k tokens and reads only target- or objective-owned surfaces; every multi-file read runs as one `tail -n +1` batch.

## [01]-[TOPOLOGY]

Run in one parallel block:
1. `tree -a -L 3 libs` — Maps every branch and package with per-file size and modified-age columns: all docs as relative page weight in one view.
2. `fd -t d -d 3 '^\.(planning|api)$' libs` — censuses the scaffold: which folders carry a `.planning/` and which carry an `.api/` catalog tier.
3. `fd -d 1 -t f .` — Root files, names only. Root file — central manifests, lockfiles, workspace/solution files, tool config — an owner to know exists.

## [02]-[PLANNING_LAW]

READ 100%, one batch (`==> path <==` headers delimit files):

```bash
fd -t f -e md . libs/.planning libs/dotnet/.planning libs/python/.planning libs/typescript/.planning -X tail -n +1
```

- Batch grounds all Tier-0 and branch law, including every `RULINGS.md`.

## [03]-[TARGET_DEEPENING]

Spent only on what TARGET or the arrived objective names — folder cores are never read wholesale; the branch law already maps every folder:
- FOLDER CORE: read `<pkg>/README.md`, `<pkg>/ARCHITECTURE.md`, and `<pkg>/RULINGS.md` when present.
- FOLDER SURFACE: run `loc <pkg>/.planning` for page inventory, LOC, and complexity, then `ls <pkg>/.api/`.

Bare-language TARGET: batch the branch's folder cores in one command:

```bash
fd -t f --max-depth 3 -E '_tmp' -E '.planning' '^(README|ARCHITECTURE|RULINGS)\.md$' libs/<lang> -X tail -n +1
```

## [04]-[CLOSE]

1. Report grounded branches/packages, law, and target deepening in a few lines.
2. Continue into any actionable argument or objective already in context; otherwise request the objective, scope, and constraints, then stop.
