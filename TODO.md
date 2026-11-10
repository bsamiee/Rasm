# [TODO]

## [01]-[WORK]

Libraries in every language and their consumers are rebuilt as general capability any future app, tool or plugin composes, each app holding its own content. Fresh sessions read this section, CLAUDE.md and README.md, then drive the queue to its end.

### [01.1]-[OPERATING]

- Leads are Opus agents with focused briefs (goal, inputs) free of pass numbers, narrowed scope, validation language and negative framing
- Fan-out follows a run's units (plan rows, findings, files, domains, hosts): disjoint at once, overlapping in order, agents reading unit evidence
- Adversaries read diff and files whole, no earlier agent's report; later passes lean to collapse; each pass deepens capability as hard as it reduces
- Agents run commands in foreground and checks once at the end, editing only their own files; messages to agents inside a running workflow go nowhere
- Desktop apps run through watch-apps, Ghidra through use-ghidra; function-hooks rewrites or refuses the rest
- Defects are fixed at cause in the pass finding them, in every layer holding them (tool, hook, skill, library); nothing found is deferred or unacted
- Distilled products evidence logic, not scripts to copy; findings in `~/.claude/rasm/workflows/` and `docs/research` are acted on, not rediscovered
- Prose (skills, rules, messages, guidance) changes through clean-prose by replacing the line owning a fact, with no appended clause
- Owner reports hold results and decisions in few words; runs stopped by a usage limit resume, workflows with `args.finished` built from their journal
- Each change commits locally; pushes come before each InPage package run and the first extension run, and after each font library round

### [01.2]-[WORKFLOWS]

- Deepen: `deepen.js` with `language`, `mode` build/deepen/reduce, `base`, one of `path`/`paths`/`key`, optional `exclude`/`inputs`/`oracle`/`plan`
  - With `plan`, its rows for the paths group into units by owned files and slice into what one editor writes; without it, one reader per file group
  - Each unit: editors per slice, adversaries (deepen, then collapse), a verdict; then cross-group sets, a lead, one check with the oracle
- Tune: `tune.js` with `language`, `base` and `exclude` maps the diff since `base` into improvements and regressions, each to its owner:
  - Ownership order from `languages.js`: Biome/tsconfig, Ruff/ty/mypy/refurb, clippy/dylint, `.editorconfig`/globalconfig/Roslyn, swiftlint, ast-grep
  - One writer per checker, per skill a writer, adversary and clean-prose pass, one close
- Extension research: `extension-research.js` with `key` one domain note, or `mode` plan and `key` one domain for its build plan, one run at a time
- Folder guidance: `guidance.js` once the tree is committed after a round of rebuilds

### [01.3]-[RESEARCH]

- `docs/research/applications/<host>/`: `products/`, per product `decompiled/`, `internals/` with proofs, `distilled/` runnable TypeScript, `facts/`
- `docs/research/typescript-extension/`: domain notes, `studies/`, `packages.md` alone choosing packages, `ARCHITECTURE.md` capability matrix, levels
- `~/.claude/rasm/workflows/typescript-extension/`: `inpage/PLAN.md` rows citing distilled sources, `base/` pre-clean notes, `review/` plan reviews
- `~/.claude/rasm/workflows/fonts/`: font library's `DECISION.md`, `ARCHITECTURE.md`, `INTERFACES.md` and `deepen-brief.md`
- `docs/research` is ignored by git: runs rewriting a note snapshot it first and judge its diff against the snapshot

### [01.4]-[STATE]

- TypeScript: 14 `TYPESCRIPT.md` [03] rows built, sweep, audit, project reductions, one-name-per-concept fold and skill and check tune (`46ab9d28c`)
- Workflows: `deepen.js`, `tune.js` and `languages.js` replace the per-language scripts; fan-out from units (`bf444ecef`, `cf438234f`)
- Desktop apps: watch-apps, its library and function-hooks route, observe and clean up every launch (`3297a5770`)
- Ghidra: one project per binary shared through `Serve.java` (`cc48d8bfa`)
- Rust waits for a later session's launch:
  - Deepened: cli, typst, ifc, bcf, numerical-analysis, regression, graph-layout, implicit-modeling, dependency-graph, drafting annotation, mesh, site, solar-geometry, weather
  - Deepen left, in order:
    - Services and graphs: `rasm-observability` `process`, daemon, mcp, network-analysis
    - Numerics and geometry: stats, optimization, signal-processing, dynamic-simulation, proximity, planar-geometry, form-finding
    - Data, drawing, documents and interchange: arrow, drawing, drawing-views, drafting `sheets`, pdf, workbooks, dwg
  - Checks owed: `rasm-drafting` `sheets` against the deepened annotation

### [01.5]-[QUEUE]

Numbers run in order; a number with a letter runs beside the number it shares.

- 0: Workflows first: fix `deepen.js` and every other workflow before any run below
  - Typography run (base `0305d5c82`): 117 agents, 12.4M tokens, 2 hours, for 18 plan rows over 15 files
  - Cross-group sets ran a second round over the same core files, each set a full editor, an adversary pair and a verdict
  - Duplicate findings spawned their own sets (one `Settings.Marks` fix reported 3 times); empty sets still ran their whole chain
  - Lead and check never ran, so no agent closed the run; spanning findings outside its paths were left as text
  - Fix: one cross-group pass over the union of spanning findings, deduplicated by file and change, chained once
  - Fix: a set with no change skips its adversaries; consumers outside the paths move in the editor that changes their API
  - Fix: the lead and the check run before anything optional; each run is measured and reports its cost
  - Then read `tune.js`, `extension-research.js` and `guidance.js` for the same fan-out and closure faults
- 1: InPage, one `deepen` run per package with `plan` `~/.claude/rasm/workflows/typescript-extension/inpage/PLAN.md`, its inputs and its oracle:
  - Finish typography: its plan rows, then its lead review and check with the oracle
  - Then search, then indesign, then `apps/creative-cloud/atelier`
  - Plan rows for `@rasm/grid` wait for the grid extension row
  - Then `tune` over TypeScript from `0305d5c82`, then push
- 1a: Rust `deepen` runs beside 1, launched by a later session:
  - Builds per `RUST.md` [03] row, `[AFTER]` order: facets, pointcloud, terrain, energy, daylight, features, query, lake, fields, scene after facets
  - Deepens per crate in Deepen left order
  - Cadence: one build and one deepen at launch, another pair every 30 minutes, at most 3 of each running, each commit freeing its slot
  - Once deepen targets run out, deepen runs over the geospatial group, cli group or chosen crates; `tune` over Rust every few commits
- 1c: Font library `libs/python/graphics/typography` and its `foundry` command, building, refining and verifying any font, Perso-Arabic one domain in depth:
  - Round one committed (3,049 lines in seven packages, `foundry ACTION DECLARATION [NAME] [DESTINATION]`), so push it first
  - Then 2 or 3 `deepen` rounds over both paths with `deepen-brief.md` input, each a fresh run from the prior round
  - `rasm-bpy:typecheck:mypy` spends 7.5 s on `rasm_bpy/ui/_resources/icons.py`'s literal-union `case _`; fix it at its cause
  - First round folds type, class, constant and string spam into richer objects with higher-order operations for fonts in general
  - Each change rebuilds only what it reaches; CLI takes no flag a question does not need; nothing is tied to old tooling or one font
  - Then `tune` over Python
- 1d: Font files after 1c, every face in `assets/fonts` rebuilt through the library to world-class quality:
  - One folder per family under its writing style or construction in `assets/fonts/perso-arabic/`, with `rasm.toml` and `corpus.toml` at the set root
  - Agents judge glyph renders (weight, proportion, joining, mark placement, spacing) by universal Perso-Arabic principles, not Urdu or Desi forms
  - Every face complete, none outline-only or partial: metrics, features and behavior stated in full, metadata with the set's own names and notices
- 2: Extension after 1, in [02] order: research per note, then a plan per domain, then a build run per `TYPESCRIPT.md` row, tuned over its commits
- 3: Folder guidance after 2 and 1a
- 4: Drain: every item closes; [03] alone stays

## [02]-[TYPESCRIPT_EXTENSION]

Extends TypeScript libraries in depth from what the most capable InDesign plugins and scripts do.

### [02.1]-[STANDARD]

Extension is the repository's central deliverable, held to one standard:
- Research notes reach university depth from primary sources
- Each phase and row finds more than the one before
- Every chosen package is used deeply through its own members
- Higher-order capability is realized in code, never named alone
- Architecture follows level: general, Creative Cloud, one app
- Packages with many concepts group them in multi-concept domain folders; no single-file folder, flat directory of unrelated files or coined term
- Prose follows clean-prose throughout
- Naive notes and plans rerun their `extension-research` run before a row reads them; naive code takes a `deepen` run over its folder

### [02.2]-[METHOD]

- Decompile in full: every algorithm of each acquired product rebuilt as runnable TypeScript, proven against binary and host
- Rebuild: use `.claude/workflows/TYPESCRIPT.md` for the method every extension run's agents read

### [02.3]-[PHASES]

1. Acquire: done, products in `docs/research/applications/<host>/products/<domain>/<key>/`, distilled code in `<host>/distilled/<domain>/<key>/`
2. Decompile: binary proofs through Ghidra's emulator
   - Host proofs run in InDesign 2027 Beta, or 2026 where a product loads only there
3. Distill: one distiller per algorithm family rewrites `rebuilt/` into `distilled/`, held identical by its A/B harness
   - Adversaries follow per family and across the product
4. InPage: one `deepen` run per package over the plan rows in [01] Queue 1 order, then `tune` over TypeScript
5. Research: one `extension-research` run per domain note at a time, each pushed before the next, in order:
   - Typography, perso-arabic, composition, grids, indesign, data, mcp, print, color
6. Plans: after the InPage run and its tune commit, one `extension-research` `mode` plan run per domain at a time, in order:
   - Typography, grids, color, indesign, data, mcp
   - Each run writes `~/.claude/rasm/workflows/typescript-extension/<domain>/PLAN.md` from its distilled products, rebuilt code and research
   - Each run writes its domain's `ARCHITECTURE.md` rows and `TYPESCRIPT.md` [03] rows
7. Rows: one `deepen` run per `TYPESCRIPT.md` [03] row once its `[AFTER]` rows commit and its gates are answered:
   - `mode` build with `key`, and `base` the commit pushed before it
   - `inputs` its domain plan and the evidence the plan names
   - `oracle` the product proof covering it where one exists
   - `exclude` a nested row's folder
   - Typography row in `mode` deepen over the InPage result
   - Then `tune` with `language` typescript from the row's base, pushed before the next row

### [02.4]-[STATE]

- InPage: `~/.claude/rasm/workflows/typescript-extension/inpage/PLAN.md` holds 31 open rows grouped by owning package, each citing distilled sources
  - Goal: universal Perso-Arabic typesetting on any installed font, real InDesign logic shaped by neither Atelier nor MCP, packages fully used
  - Oracle `proof/ab.ts` in `docs/research/applications/indesign/products/typesetting/inpage-typesetting-tool`, exiting 0 against current libraries
- InPage proofs: 164 functions binary-proven over 1494 cases, host proof impossible for the Windows-only plug-in
- InPage distill: Persian and Arabic typesetting over any installed font, read from what each font states, Urdu-only forms out:
  - Nuqta and aerab position, letter adjustment, spacing tuner, kasheeda, overlap kerning, text carving, symbols, Quran glossary
- Color: no further distill, evidence in the color research and in `docs/research/applications/indesign/`:
  - `distilled/internals/adobe-indesign-color`
  - `internals/adobe-indesign-color` README locating each rebuilt family
- Research notes: cleaned to principles at their owners, pre-clean copies in `~/.claude/rasm/workflows/typescript-extension/base/`
- Fonts: proofs, probes and research examples set text in the faces under `assets/fonts`; library code reads what any face states and names none
  - Agents open, drive and quit apps through watch-apps, the hook routing every other form there
- Next proofs: host cases written and unrun (second script decompile, Grid Calculator, ScribeDOOR, script domains)
  - ScribeDOOR for Illustrator runs on the launch path, its trial ending around 8 November
- Distilled: 61 products, pages (23 left), styles (20), right-to-left (8) and tables (3) waiting on host proofs
- Owner's choices: InPage plug-in 2.2 for InDesign 2026 and a macOS build join the decompile on arrival, no Apple Developer Program for now

### [02.5]-[SCOPE]

- InDesign interface: library interface layer as complete as Rhino's and Blender's, from the distilled app, native beside InDesign's own:
  - Panels, panes, dialogs, preference pages, lists, buttons, swatches, color wheels and hue controls, native in look and behavior
  - Every application preference set with no document open, every document setting with one open (sizes, layouts, visual elements)
  - Each setting declared as data bound to the host's own categories
  - Atelier, Galley and the rebuilt plug-in capability compose the layer
- Typography: shaping, metrics and typesetting with Persian and Arabic right-to-left support through the packages that own it
- Color: color spaces (sRGB, CMYK, Lab, Adobe RGB and others), one conversion path between them, and palettes
- Grids: grid systems, rule lines, margins and ruling from grid theory over any print or digital format, coupled to no host, house values caller data
- Document structure: pages, styles, tables and footnotes as host objects the distilled scripts drive, rebuilt on the host's own mechanisms
- Depth: MCP logic, automation and scripting, data and stores, search kinds and pagination, wherever code is shallow

## [03]-[DARKROOM_WINDOW]

Later work. Investigate first, then plan, then build.

### [03.1]-[GOAL]

Darkroom gets its own window in Rhino, built from the ground up, that replaces the native Rendering panel for anyone who installs it. The window is called Darkroom.
- What it holds: every native rendering capability is reachable from it, in one cleaner, unified place. That covers the Rendering panel settings, the render window, post effects, tone mapping, output and the related commands and containers
- What it is: a real window with Darkroom's own buttons, panels and sidebars, in the spirit of Jifto. It is not one more docked container on the right
- The host: Rhino keeps its own Rendering panel. Darkroom stops relying on it, and a Darkroom user uses Darkroom instead

### [03.2]-[BOUNDARY_RULES_OWNER_BINDING]

- Libraries: every host capability the work derives is built in `libs/` as a general capability any future app can use. Examples: hosting a window, reading and writing render settings, driving the render pipeline, reaching native render commands
- Darkroom: only Darkroom-specific content lives in `apps/rhino/Darkroom`, meaning its layout, its effect chain, and its choice of what to show
- UI libraries: building a full panel, window or GUI is a capability of both Rasm.UI (host-neutral, Skia) and Rasm.Rhino.UI (Rhino-native, Eto and Rhino.UI). Either one can build it on its own
- No coupling: Rasm.UI and Rasm.Rhino.UI never reference each other. Rasm.Rhino, Rasm.Grasshopper and Rasm.UI never reference each other
- No moves: no code moves between libraries and apps to suit one app

### [03.3]-[ALREADY_ESTABLISHED_READ_BEFORE_STARTING]

- Host facts: `docs/research/darkroom/window/GUI-INVESTIGATION.md` §1.1, "What a plug-in can and cannot extend". It covers Rhino 9 Mac with decompile citations:
  - Render window: its command toolbar and status bar are closed to plug-ins
  - Stage tabs and side panes: extra tabs and side panes need a RenderPlugIn (`RenderTabs.RegisterTab`, `RenderPanels.RegisterPanel`)
  - Rendering panel sections: these belong to the current renderer alone
  - Native containers: no API changes them
  - What a plug-in can add: its own panel, Options page, Document Properties page, Snapshots client and View properties page
- Darkroom inventory and extension points: `docs/research/darkroom/window/DARKROOM-PLAN.md` §2, the old Darkroom surfaces, and "Rhino rendering extension points"
- UI research: `docs/research/darkroom/window/ui-research/` (pro image apps, Rhino renderer UIs, modern Rhino plug-in UIs, Rhino UI techniques)
- Current state: Darkroom has no UI, and only restoring a stored chain creates chain edits. Rasm.Rhino's `UI/` folder (namespace Rasm.Rhino.UI) holds windows, panels, containers, toolbars, rows, inputs and Skia surfaces. Nothing in the tree hosts a Rasm.UI scene in Rhino yet

### [03.4]-[QUESTIONS_TO_ANSWER]

1. Window kind: is it a modeless Eto window owned by Rhino's main window, a docked panel that fills a container, or a full document-window-style surface? Settle docking, restore on startup, multiple monitors and macOS 27 window behaviour, from the host's own window classes
2. Replacing the Rendering panel:
   - Can Darkroom register as a RenderPlugIn, or as something else that owns the render settings surface
   - Does becoming the current renderer give Darkroom the Rendering panel's sections and the render window's tabs and panes
   - What does Darkroom lose if it is not the renderer, and what does Rhino's own renderer lose if Darkroom is
3. Native capability coverage: list every native rendering command, panel section, render-window control and setting. For each, record how Darkroom reaches it:
   - a RhinoCommon API
   - a command or script call
   - document settings (`RenderSettings`, `PostEffect` and its state, `RenderWindow`)
   - not reachable For each unreachable one, decide whether to drop it or link out to the native surface
4. Hiding the native surfaces: can an install hide or replace the Rendering panel, the Render Tools tab and render toolbar entries? Use RUI declarations and the containers file. Prior research says no API changes existing containers, so confirm what an RUI and the plug-in's own containers can do
5. Rasm.UI in Rhino without coupling: how does a Rasm.UI scene (`Hosting/SceneHost.cs`) draw and take input inside a Rhino window while neither library references the other
   - Candidate A: Rasm.Rhino.UI exposes a host surface that speaks only Skia and plain input events, and the app wires a SceneHost to it
   - Candidate B: Rasm.UI hosts itself on Eto. Eto is cross-platform and not Rhino, so this keeps Rasm.UI host-neutral
   - Decide which glue is general library capability and which is app composition
6. Rasm.Rhino.UI or Rasm.UI for Darkroom: which one fits a dense image-editing window best? Weigh native look and accessibility, custom drawing (scopes, curves, wheels), theming, and Jifto-level polish. Mixing per region is allowed if the glue question above is settled cleanly
7. Library gaps: list what the window needs that neither library has today. Examples: window chrome, sidebars, splitters, tool rails, viewport-linked previews, render progress, history browsing. Build each gap as a general library component, never in Darkroom
8. Chain editing: the window becomes the place where the chain is edited. Confirm the chain-edit API (`Chain/`) carries every edit the window needs: undo through Rhino, document persistence and live viewport update

### [03.5]-[METHOD_WHEN_PICKED_UP]

- Facts: take them from Rhino 9 itself, its decompiles in `docs/research/darkroom/window/gui-inv/` and `docs/research/darkroom/window/ui-research/rc/`, RhinoCommon docs, the forum and live probes through the use-rhino and watch-apps skills. Do not guess
- Order: answer questions 1 to 5 before any design, because they decide what is possible. Then design the window. Then land the library capabilities first and Darkroom's content after
- Done means: a live check in Rhino with screenshots, every native rendering capability reachable from Darkroom, and zero findings
