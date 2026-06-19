# [IMPLEMENT_WORKFLOW]

`<TARGET>` = ...

`libs/.planning/campaign-method.md` is general workflow information that is superceded by content within this prompt, it exaplins the general workflows, but the sole focus of our session is `<TARGET>`, edits made to any other file within `libs/.planning/planning-targets.md` is strictly for alignment, updating steams, ports/boundaries/wires/tasks/ideas etc if they become misaligned or stale due to changes made in our `<TARGET>`.

[BRIEF] You must be an orchestrator, not a writer, unless the task is focused, and small, otherwise, delegate all tasks. We are in plan mode, and MUST maximize it exhaustively, we cannot and will not leave plan mode until it is extensively researched, investigated, and detailed; all the actions that will be done once we leave plan mode must be predetermined with great detail in plan mode. Ensure the plan is high signal, nothing is left to ambiguity and is declarative and instructional, not explanatory prose, nor giving you lee-way for re-inventening approach after already established in plan, treat plan as law and truth, focused, and structured properly, no vagueness or ambiguity in it, providing enough detail to immediatly jump into workflows once we leave plan mode. Prior to presenting the plan to user, review the entire "finalized" plan line by line, and identify the appropriate series of workflows that will be needed, plan them NOW to have agents also review/critique it compared to plan, to ensure we have well made phase structure, without any gaps, each phase is high impact/value, and is sequenced properly, and we leverage parallel work within a workflow, as well as parallel workflows where we can. Never commit, do not worry about commiting, focus on implementing all the work, user will commit themselves at the end.

ASK THE USER AS MANY QUESTIONS AS POSSIBLE, EARLY DURING PLAN MODE, DURING PLAN MODE, MIDDLE OF PLAN MODE, AND WELL BEFORE FINAL PRESENTATION TO ENSURE PLAN IS DONE WITHOUT MISUNDERSTANDING. THE PLAN MUST BE EXTREMELY DETAILED, 

[READ]: 
- Session Target(s): `<TARGET>`
- Planning Projects: `libs/.planning/planning-targets.md`
- Mandated Approach: `libs/.planning/campaign-method.md`
- Planning Infrastructure: `libs/.planning/README.md`
- Planning Topology: `libs/.planning/architecture.md`
- MUST READ: `CLAUDE.md` + `AGENTS.md` + `README.md` + `tools/assay/README.md`

[CODE_DOCTRINE] - Universal, regardless of language, all other languages must meet these same standards of density, complexity, advancement, no flat code.
- Foundation: `docs/stacks/csharp/README.md`
- Core (read all ROOT files in dir): `docs/stacks/csharp`
- Specialized (read all files in dir): `docs/stacks/csharp/domain`

Note: `docs/stacks/<lang>/` for the tier's language is the governing code doctrine, read in full at every stage; `docs/stacks/csharp/` is the universal density bar every branch matches whatever its language. A fence is held to C# bar FIRST its own language doctrine SECOND.

[API_SOURCES]
- The branch and folder targets named by the session, each target's own `<pkg>/.api/` catalogues, and the admitted packages themselves.
- These MUST be utilized and understood whenever working on design docs, idea or task creation and implementation, we treat external libs/packages as FIRST CLASS and PREFERRED over stdlib.

[LIVE_PROVE]
- If a task or idea requires live probing, launch Rhino WIP (not rhino 8) locally on machine to run it, it is available, between assay, rhino-bridge, and our rhino-mcp skill there is nothing we can properly diagnose/extract/identify/understand regarding rhino behavior for any functionality need.
- We have: `tools/rhino-bridge/README.md` available as well, and pairs with `tools/assay`
- We have: `.claude/skills/rhino-mcp` as well if needed

## [GOALS]

[SCOPE]:
- ALL `IDEAS.md` file(s) open items in `<TARGET>`
- ALL `TASKLOG.MD` file(s) open items in `<TARGET>`
- All `<USER_TASK_REQUESTS>`

Identify all open items in both file types, and understand the topology of lib-wide: `libs/.planning`, language-wide: `libs/csharp/.planning`, `libs/python/.planning`, and `libs/typescript/.planning`, and folder-specific as per `libs/.planning/planning-targets.md`.

`<TARGET>` has an `IDEAS.md` + `TASKLOG.MD` within it's broader folder, so for example: `libs/csharp/Rasm.Apphost/IDEAS.md`, `libs/csharp/Rasm.Apphost/TASKLOG.md`

Ideas are meant to be larger conceptual functionalities, and based on topology, the splash-radius/touch-points needed. Tasks are concrete/targeted items of work. Both ideas and tasks vary in scope/scale, from small, medium, and large. They are meant to be outlines of work, not the full detail, and it is REQUIRED that while in PLAN mode (which we are), we identify all tasks, ideas, and all the high-quality work we need to do to properly implement them. 

Idea ambition increases as topology level increases, with lib-wide being the largest, and as such, demands the most care when attempting to tackle. Which leads to EXCEPTIONS.

`<USER_TASK_REQUESTS>`:
- ...
- ...
- ...
- ...
- ...

[IMPORTANT]: Our focus is `<TARGET>` but understanding the broader planning targets/folders we have and their relation and topology is critical when working within `<TARGET>` so that we can update other folders if needed for alignment, or keep our changes within `<TARGET>` properly bounded, and within our seperation of concer, ensuring no overlap/duplication/cycles, etc

[BLOCKED_ITEMS]:
- ANYTHING MARKED [BLOCKED] will be investigated/researched NOW, for py related blockers, read pyproject.toml + our `tools/assay/README.md` UNDERSTAND that we have a full provisioning tool to confirm/check things, during plan mode especially. Our Parametric_Forge project also provides py substrates for us on packages that we couldn't add due to our hard requirement of being on py 315, though we can version floor other tools as needed, READ:
- `/Users/bardiasamiee/Documents/99.Github/Parametric_Forge/modules/home/programs/languages/python-tools.nix`
- `/Users/bardiasamiee/Documents/99.Github/Parametric_Forge/modules/home/programs/languages/scientific-tools.nix`

## [METHOD_AND_STANDARDS]

[METHOD] The campaign method, stated inline so the task is self-contained on entry; the routes below carry the full law and the complete doctrine.
- Surface: work ONLY on the `.planning/` design corpus — design pages and the transcription-complete code FENCES inside them — plus the four index docs per tier, the central manifests, the per-folder `.api/` catalogues, and the Nix toolchain. A FENCE is a markdown fenced block, the work product itself, never a `.cs`/`.py`/`.ts` source file.
- Principles: one unified body of functionality — a new capability is a row, case, or column on an existing axis, never a parallel surface; full modality completeness from the root; everything parameterized; plug-in-play composition aligned only at the wire/companion seams, never coupled to a sibling's interior.
- Tiering + stages: run as fanned workflows across three altitudes — per-folder, then a full per-language pass, then the cross-`libs/` master, run BOTTOM-UP so each higher tier deepens a settled lower one. Each higher tier is a FULL pass at broader scope (the same produce → critique → red-team depth), never a thin reconcile: it deepens and aligns the settled lower tier, adds the fences and tasks that only emerge at that altitude, and applies the boundary-integrity check — a concern owned twice within a runtime, a folder mixing unrelated concerns, a concern scattered across folders, or a cross-folder/cross-language touchpoint coupled to a sibling's interior is a defect it fixes or records. Producer → critique → red-team, every stage applying its own fixes; a single un-adversaried author is never the final word.
- IMPLEMENT realizes open tasks and ideas into deep code fences, mines every admitted package to its full capability, and crushes surface sprawl into fewer, richer, optimized owners with zero functionality loss.

[BAR] A high-value IMPLEMENT turn leaves every owner capturing the full capability of every package it admits, every sprawl collapsed into one denser owner with no capability lost, and every fence transcription-complete against the verified `.api`. The critique pass guards capability conservation and density; the red-team pass attacks every fence for a surface that could still collapse, a thin wrapper, a silent functionality drop during a refactor, a missed package capability, or a framework violation, and fixes each in place.

[HUNT] At implement, critique, and red-team alike, from multiple facets:
- Under-captured capability: an admitted package whose `.api` and code expose capability no owner exploits is a named gap, closed by deepening an existing fence or adding one.
- New concepts: a capability the package or the domain admits that the corpus has not conceived becomes a new fence within the framework; when it is a bigger concept it is recorded as a CLOSED idea only if its fences land this turn, otherwise it is left as an open idea or task for IDEATE rather than ideated top-down here.
- Surface sprawl: parallel types, enums, methods, or near-duplicate shapes collapse into one parameterized owner in the language's own collapse vocabulary — a tagged/discriminated union, fold algebra, dispatch table, or the language doctrine's generated-owner form — that replaces them with no functionality removed; fewer objects, richer ones.
- Rail unification: one entrypoint family per rail, one fault family per package, total dispatch, unified in the fences without bleeding a rail/axis/lane term into an ARCHITECTURE sub-domain name.
- Optimization: correctness first, then performance and sophistication — allocation, span/SoA layout, dispatch shape, algorithmic complexity — not only line-count reduction.
- New work surfaced: api gaps, newer or stronger packages, and tasks the implementation exposes are realized or recorded the same turn. Adopting a replacement or admitting a new package extends the canonical owner first — never a parallel surface — updates the README registry and the central language manifest, adds the folder's `.api/<package>.md` catalogue, and introduces no per-folder manifest.

## [PLAN_MODE_DISCIPLINE]

Plan mode is left only when the plan is exhaustive. Before exit, every gate, fence, page, sub-domain, admission, catalogue, manifest edit, and refinement the campaign will touch is identified in decision-complete detail — no residual research deferred to an execution turn, no entry-gate assumption left unresolved, no finding, idea, gap, or suggestion truncated or tossed. Each finding lands at its right tier and target; a finding without a home is a defect, not a discard. Every addition — fence, card, page, admission, refinement — is critiqued and red-teamed as it lands, and the whole plan and corpus are critiqued and red-teamed as one body before the plan finalizes; a single un-adversaried author is never the final word. A red-team or verify stage applies its fixes rather than only reporting them: it verifies before flipping any status — a spike or gap is marked finalized only against a cited `.api` line or harness output — fixes true positives in place, refines false positives rather than deleting the finding, and never marks work done without evidence. The plan carries the full scope a 4+ page blueprint demands; a thin plan is an un-exhausted one.

## [PLAN_EXIT_PROTOCOL]

On leaving plan mode, before any execution turn, the orchestrating session runs one handshake: re-read the entire plan and every plan-mode artifact end to end; derive the complete, exhaustive task and phase list — every gate, fence, page, sub-domain, admission, catalogue, manifest edit, and refinement — as the execution ledger it alone writes; read all of `docs/stacks/csharp/` (the floor for every branch) and `docs/stacks/<lang>/` for each touched language. Every dispatched workflow and sub-agent then reads its plan phase, the route-owned `docs/stacks/<lang>/`, and the three form standards (`docs/standards/information-structure.md`, `formatting.md`, `style-guide.md`) before authoring any fence, page, idea, or task. THE PLANNING MODE CANNOT BE LEFT UNLESS IT IS EXHAUSTIVE, DETAILED, ADVANCED, COMPLEX/RICH, NO FLUFF/CHAFF, ALL CONSIDERATIONS/TASKS FULLY INVESTIGATED, RESOLVED, CRITIQUED/REFINED + RED-TEAMED

[IMPORTANT]: The plan must be created in a structured, detailed way, ALL work that we do should be done with workflows WHILE WE ARE IN PLANNING MODE, AND ONCE WE LEAVE IT, you are an orchestrator as much as possible, doing personal work only where surgical/needed. At the start of leaving the planning mode, you MUST re-read the entire plan + all artifacts we made, if any, create a proper task list. ALL agents/workflows/teams must re-read the plan for their phase work, fully, and they MUST read `docs/stacks/csharp` regardless of language target, understand these files, our `docs/stacks/csharp/README.md` for code doctrine, and other files in root of `docs/stacks/csharp` the code standard here is the FLOOR, ts/py code if we touch/consider it is far far weaker, and always needs refining/hardening to reduce surface, reduce object/shape/types with ultra-advanced/modern/bleeding-edge replacements to reduce LOC + surface + objects without losing functionality. 

[CRITICAL]: ALL CRITIQUE, AND REDTEAM AGENTS IMPLEMENT CORRECTIONS/REFINEMENTS AS THEY FIND IT, NOT LEDGER NOTES FOR YOU TO DO, WORKFLOWS SHOULD BE SELF-CONTAINED, CREATE THE WORKFLOWS WITH ALL PHASES YOU BELIEVE ARE BEST, JUST ADD THESE MANDATORY PASSES; PRIOR TO FINALIZING THE PLAN FOR PRESENTATION, USE MULTIPLE SUB-AGENTS (OPUS) TO RE-READ ALL PLAN, TO OPTIMIZE IT, ENSURE NO GAPS/MISTAKES, STRUCTURE OPTIMIZATION, INCREASE SIGNAL/RESOLUTION FOR VALUE, AND HELP WITH REFINING ALL WORKFLOW SCRIPTS IF MADE DURING PLAN MODE, TO ENSURE ALL CRITIQUE/REDTEAM ARE LOOKING AT ACTUAL/IMPORTANT ITEMS NOT NONSENSE/HOUSEKEEPING, BUT HIGH VALUE IMPROVEMENTS/CORRECTIONS/REFINEMENT/ALIGNMENT NEEDS. They must find gaps in implementation, ensure tasklog/ideas are updated if we had items from them done, they need to check for missing external lib api usage, unnecessary handrolling, deviation from `docs/stacks/csharp/README.md` code doctrine (universal), indirection, flat-code, tacked-on code patterns, enforce ground-up/root-up refactoring/adding of content, even when making a new file, proper integration into existing code logic/bodies, etc...