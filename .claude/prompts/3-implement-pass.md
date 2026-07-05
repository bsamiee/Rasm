# [IMPLEMENT_PASS]

`<TARGET>` = `<target folder path(s)>`

[GOAL]:
- Realize every open `IDEAS` and `TASKLOG` card under `<TARGET>` into deep design-page code FENCES. `<TARGET>` is a [FOLDER] (ex: `libs/python/geometry`), a [PACKAGE] (`libs/csharp/Rasm.Bim`), a [LANGUAGE_ROOT] (`libs/python`), `libs` for all three, or several such paths.

1. List all files in: `libs/.planning` and read them all
2. `libs/.planning/planning-targets.md` contains the full list of possible `<TARGET>` values (all "planning" folders)
3. Understand the STRATA for csharp `libs/.planning/architecture.md`
4. READ: `README.md` + `CLAUDE.md` + `AGENTS.md`
5. READ: `tools/assay/README.md` + `tools/rhino-bridge/README.md` + `.claude/skills/rhino-mcp`. Rhino WIP is on the local machine; ALL PROBING IS PERMITTED AND EXPECTED — those three sources plus the `uv run python -m tools.assay provision` rail (databases, containers, Python tooling) can test or confirm anything.
6. Use `tree` alias with <arg> being a path to show all folder structure in `libs` for full topology/understanding
7. use `tree` on all `<TARGET>` individually, identify and understand all topology
8. Read all `IDEAS.md` + `TASKLOG.md` of `<TARGET>`, identify all files within its `.api/` folder and design docs within `.planning/` to understand the scope, and what cards are like, and the `Ripple:` field
9. UNDERSTAND what a "critique" is vs a "redteam" — neither is a simple review; `.claude/workflows/rebuild.js` shows the role law: not simple adjustments, but world-class 12/10, bleeding-edge, ultra-advanced, complex/dense/rich code (fences) and code logic. WE ALWAYS BUILD DESIGN DOCS ROOT/GROUND-UP TO ADD FEATURES AS IF THEY WERE THERE FROM THE START; THAT IS WHY WE DO DESIGN DOCS, NOT CODE FILES; WE NEVER MAKE A CODE FILE, JUST DESIGN DOCS.
10. Read the FULL root of `docs/stacks/<lang>/` for each language in `<TARGET>` — it is the governing doctrine for that language, and sibling stacks are the floor, never the ceiling. For C#, `docs/stacks/csharp/shapes.md` + `surfaces-and-dispatch.md` + `rails-and-effects.md` carry the dispatch/rail approach that generalizes across languages.
11. [WORKFLOW_MAIN_TASK]: Read all of the matching `.claude/workflows/implement-<cs|py|ts>.js` for `<TARGET>`'s language, then `Workflow({ name: 'implement-<cs|py|ts>', args: '<TARGET>' })`

`<TARGET>` has an `IDEAS.md` + `TASKLOG.md` within its broader folder, so for example: `libs/csharp/Rasm.AppHost/IDEAS.md`, `libs/csharp/Rasm.AppHost/TASKLOG.md`

Ideas are meant to be larger conceptual functionalities, and based on topology, the splash-radius/touch-points needed. Tasks are concrete/targeted items of work. Both ideas and tasks vary in scope/scale, from small, medium, and large. They are meant to be outlines of work, not the full detail, and it is REQUIRED that while in PLAN mode (which we are), we identify all tasks, ideas, and all the high-quality work we need to do to properly implement them. Idea ambition increases as topology level increases, with lib-wide being the largest, and as such, demands the most care when attempting to tackle. Which leads to EXCEPTIONS.

The workflow resolves ripples to closure — in-scope seams aligned, 1-hop out-of-scope counterparts realized — so a narrow `<TARGET>` may still touch sibling and cross-language folders. On return, read the workflow's typed result: cards it closed, cards left open or deferred with reasons, and unrealizable ripple remainders. Re-run `<TARGET>` for any card left open or under-realized; a ripple target with no design-page surface is informational only, never a re-run trigger.
