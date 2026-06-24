# [IMPLEMENT_PASS]

`<TARGET>` = `libs/csharp/Rasm.AppHost` + `libs/csharp/Rasm.AppUi` + `libs/csharp/Rasm.Compute` + `libs/csharp/Rasm.Persistence`

[GOAL]:
- Realize every open `IDEAS` and `TASKLOG` card under `<TARGET>` into deep design-page code FENCES. `<TARGET>` is a [FOLDER] (ex: `libs/python/geometry`), a [PACKAGE] (`libs/csharp/Rasm.Bim`), a [LANGUAGE_ROOT] (`libs/python`), `libs` for all three, or several such paths.

1. List all files in: `libs/.planning` and read them all
2. `libs/.planning/planning-targets.md` contains the full list of possible `<TARGET>` values (all "planning" folders), note `libs/csharp/Rasm/Geometry/.planning` is the only one nested in a mature folder, the content of that folder all pertain to `libs/csharp/Rasm/Geometry/.planning`, the `.api/`, `README.md`, etc...
3. Understand the STRATA for csharp `libs/.planning/architecture.md`
4. READ: `README.md` + `CLAUDE.md` + `AGENTS.md`
5. READ: `tools/assay/README.md` + `tools/rhino-bridge/README.md` + `.claude/skills/rhino-mcp`, we have Rhino WIP on local machine, and ALL PROBING MUST BE DONE, AND FULLY PERMITTED, we have all the tooling we can possible need to test/confirm anything, from those three sources, as well as the provisoning functionality grated by our `Parametric_Forge` project (epsecially for db's/containers/py tooling), understand the provisoning feature of
6. Use `tree` alias with <arg> being a path to show all folder structure in `libs` for full topology/understanding
7. use `tree` on all `<TARGET>` individually, identify and understand all topology
8. Read all `IDEAS.md` + `TASKLOG.md` of `<TARGET>`, identify all files within it's `.api/` folder and design docs within `.planing/` to understand the scope, and what cards are like, and the `Ripple:` field
9. UNDERSTAND what a "critique" is vs a "redteam" neither are simple reviews, as seen in `.claude/workflows/rebuild-python.js` example, it's py focused, but it shows that we are not interested in simple adjustments, but world-class 12/10, bleeding-edge, ultra-advanced, complex/dense/rich code (fences) and code logic, WE ALWAYS BUILD DESIGN DOCS ROOT/GROUND-UP TO ADD FEATURES AS IF THEY WERE THERE FROM THE START; THAT IS WHY WE DO DESIGN DOCS NOT CODE FILES; WE NEVER MAKE A CODE FILE, JUST DESIGN-DOCS
10. List all files in `docs/stacks/csharp/` this is the FLOOR and best "stacks" folder we have, ts code must NOT use `docs/stacks/typescript/` or `coding-ts`, they are both terrible, py code MUST follow the ultra-high pattern of code quality established already in `.planning/` files within `libs/python/`, understand the `docs/stacks/csharp` READ THE `README.MD` FULLY, UNDERSTAND OUR CODE DOCTRINE, AND READ: `docs/stacks/csharp/shapes.md` + `docs/stacks/csharp/surfaces-and-dispatch.md` + `docs/stacks/csharp/rails-and-effects.md`, these are nearly universal in how we approach all code regardless of language.
11. [WORKFLOW_MAIN_TASK]: Read all of `.claude/workflows/implement.js` then `Workflow({ name: 'implement', args: '<TARGET>' })`

`<TARGET>` has an `IDEAS.md` + `TASKLOG.MD` within it's broader folder, so for example: `libs/csharp/Rasm.Apphost/IDEAS.md`, `libs/csharp/Rasm.Apphost/TASKLOG.md`

Ideas are meant to be larger conceptual functionalities, and based on topology, the splash-radius/touch-points needed. Tasks are concrete/targeted items of work. Both ideas and tasks vary in scope/scale, from small, medium, and large. They are meant to be outlines of work, not the full detail, and it is REQUIRED that while in PLAN mode (which we are), we identify all tasks, ideas, and all the high-quality work we need to do to properly implement them. Idea ambition increases as topology level increases, with lib-wide being the largest, and as such, demands the most care when attempting to tackle. Which leads to EXCEPTIONS.

The workflow auto-pulls every ripple-reached folder into scope to closure and realizes ONLY the ripple-counterpart cards there, so a narrow `<TARGET>` may still touch sibling and cross-language folders. On return, read `pulled_in`, `closed`, `admitted`, `weak`, `realize_failed`, `blocked_unresolved`, `ripple_out_of_scope`, and `hard_residual`. `pulled_in` names the out-of-scope folders the closure realized (ripple cards only). `weak` and `realize_failed` are cards left open (under-realized, or whose folder realize agent died) — re-run `<TARGET>` to finish them. A non-empty `ripple_out_of_scope` is the genuinely-unrealizable remainder — ripple targets with no design-page surface (a language-level/central scope or an unrecognized package), informational only, not a re-run trigger. A non-empty `hard_residual` is for the human-in-the-loop `resolve-residuals` workflow: paste those items into its `DEFAULT_RESIDUALS` (or pass `args: { residuals }`) and run it.
