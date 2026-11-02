# [TODO]

## [01]-[LANGUAGE_REBUILDS]

Rebuild the Rust and TypeScript libraries and their consumers the way the C# and Python code was rebuilt. Libraries hold general capability any future app composes, and each app holds its own content. The work reduces surface and raises capability: fewer constants, types, records and items, more derived and parameterized logic, every package used fully.

### What a run produces

- **Depth in little surface:** the full capability the domain needs under real, advanced use, in the fewest exported and internal names, types, constants and helpers.
- **Value:** a change improves code only when its result does more with less than its base. Every declaration earns its place through an operation a caller performs, never through a rule, a row cell or the old code alone.
- **Both ends:** a library and its consumers are rebuilt together and neither shapes the other. Each consumer calls the library directly with no adapter, wrapper or reshaping.
- **Measured:** each run measures lines, declarations and exports per file at base and after (`git archive HEAD`, `loc`, `ast-grep outline`) and removes growth no capability requires before it ends.
- **Parameters over declarations:** one parameterized declaration serves every variant, and a consumer reaches a variant through an argument.
- **Derived values:** each value derives from the type, member or host query that owns it.
- **Packages used whole:** Effect 4, rmcp and the catalog crates do their work through their own members, never a wrapper or hand logic.
- **One concept per file:** no `index.ts`, barrel, `main`, `lib` or `mod` file collects a package's logic, and no folder holds a single file.
- **Crate roots:** a Rust crate root holds the crate's code beside its modules.
- **Dependencies:** a package depends only on the packages whose members it calls, with no cycle.
- **Delete and rebuild:** logic built poorly is deleted whole and rebuilt in its best form, never kept beside its replacement, wrapped, aliased or bridged.
- **No shuffling:** a concept sits where the architecture places it. A misplaced concept is rebuilt in its new place, and relocating the same code is no improvement.
- **Errors:** TypeScript keeps one `Schema.TaggedError` per package with a `reason` literal, a `subject` and an optional `cause`.
  - Its error covers only failures no host error or `Unmatched` states.
  - `Unmatched`, `ReconcileError`, `InspectError` and `PreconditionFailed` are rebuilt to that form, and no near-copy enters.
  - Rust follows the error forms of its skill.
- **No anchoring:** a run designs from the row, the skill and the package sources, never from the shape of the current code.
- **Base check:** after landing, the Check phase compares each rebuilt file with its version at the run's start and restores what that version did better.

### Running the workflows

- **Scripts:** `.claude/workflows/{rust,typescript}.js`, with rules and target rows in the sibling `RUST.md` and `TYPESCRIPT.md`.
- **Skills:** each run loads `Skill(<language>-coding)` and its companions first, and fixes a skill where a run finds a stronger form.
- **Rows:** one run per `[03]` row, with `args.key` set to its `[KEY]`, started after the rows its `[AFTER]` cell names have landed.
- **Concurrency:** TypeScript rows on folders that share no files overlap, started at least 20 minutes apart, and commit together. Up to five Rust row runs and five `depth` runs overlap, on folders that share no logic and no depth run under a row that builds on it, started at least 10 minutes apart, and overlapping runs commit together.
- **Base:** `HEAD` when a run starts. A line citation in the rules applies to its code wherever that code now sits.
- **Watching:** follow a run in `/workflows`, read `.artifacts/workflows/<language>/<key>/PLAN.md` once the Plan phase writes it, and read the diff.

### Order and watch points

- **Later Rust rows:** each starts once its `[AFTER]` rows have landed.
- **MCP:** Rust `services/mcp` and TypeScript `ai/mcp` are the capability every agent-facing app builds on.
  - Each states sessions, routing, progress, resources, elicitation, tasks, client and proxy in full, on rmcp and `effect/ai`.
  - Neither takes its shape from the current servers.
- **TypeScript `web/hosting`:** runs alone, since it shares `vite.config.ts` with media and two portfolio files with ui.

### Review between runs

- **Each finished run:** the diff against the run's base read file by file, lines, declarations and exports measured with the same commands, regressions sent back to an agent, then a commit of the run's own paths by `git commit -- <paths>`, then its tune launch (`rust-tune` or `typescript-tune` with `args.commit`), so the next run never waits on mining.
- **Run phases:** a parallel survey brief, an author, an in-place challenger, landing packages, per file group an adversary then a depth adversary that rebuilds each file for advanced use, and a check agent. Mining the checks and skills is its own launch after the commit.
- **Pushes:** each commit is pushed at once. When a mining pass ends, its skills and `tools/` changes are committed and pushed before the next launch, so no finished mining stays in the tree.
- **Hosts:** every agent runs desktop hosts in the background through Atelier's MCP tools and watch-apps. Nothing activates an app or takes the owner's focus, and scripts prove headless with their UI shadowed in scope.
- **Shared tree:** another session may edit beside these runs. Files are split by owner before either commits, and neither touches the other's files.

### Folder guidance

`.claude/workflows/guidance.js` writes and rebuilds the folder CLAUDE.md files once Atelier and the running rows have landed. A standard first (what a nested file earns, from the Claude Code documentation, with a line-by-line rubric), then one reader per area over its code and its whole history, inferring the higher-order principles that would have prevented the repeated fixes. One judge picks the highest folder level that covers each group and deletes files a level above covers; writers keep each file within 125 lines with an AGENTS.md link; adversaries cut whatever repeats the root, README, a skill or a level above, states the obvious, or freezes today's shape.

### Depth passes

Depth runs follow behind the rows, so no library stays surface level: each committed folder gets one, up to three at once, on folders that share no logic with a running run.
- **Script:** `.claude/workflows/depth.js` with `language` (rust, typescript or python), `key` (the folder under `libs/<language>/`) and `base` (the commit it starts from); then its commit and that language's tune. Rust goes first in RUST.md row order, `services/telemetry`, `cli/command` and `cli/output` leading.
- **Phases:** five readers in parallel, one lens each (capability, dependencies, reduction, correctness, shape), report only what changes code. One adversarial lander rejects weak, regressive or surface-growing findings with a reason and lands the rest without relitigating them. Three reviewers read the diff against `base` (additions, regressions, remaining reduction). One agent fixes what holds, the rule or skill lines that steered a defect, and every check, and writes `surface.md`.
- **Growth:** an item enters only for an operation a consumer could not perform before, stated per item in `surface.md`. No re-export, alias, convenience overload or variant a parameter already serves.

### State on 2026-11-02

- **TypeScript:** `data/search`, `ai/mcp`, `creative-cloud/uxp`, `graphics/typography`, `creative-cloud/automation`, `creative-cloud/indesign`, `data/database`, `web/media` and `web/ui` landed. The `atelier` workflow is in its check: it rebuilds Atelier, its Swift host and the Creative Cloud libraries with every app as a plugin whose tools show only while the app runs (InDesign and Photoshop as UXP plugins, Illustrator and After Effects as CEP extensions through `@rasm/cep`, Acrobat through `command`), one typed scripting path and one capture concept, then reinstalls Atelier and checks it live.
- **Rust:** 15 rows landed: `services/telemetry`, `cli/command`, `cli/output`, `cli/screen`, `services/daemon`, `services/mcp`, `geometry/mesh`, `geometry/proximity`, `geospatial/site`, `graphs/dependency`, `graphs/network`, `numerics/optimize`, `numerics/stats`, `data/tables`, `drawing/linework`. Depth runs landed on telemetry, command, mesh, stats and linework. `geospatial/solar` and `documents/typeset` are running as rows and `services/daemon` as a depth run.
- **Quality bar for every language:** thread safety and performance hold as firmly as surface and depth; a gap a review finds (a lowered lint, a dropped `Send`, a starved future) is fixed at its root and the rule or skill line that let it through is corrected.

### Queue

- **Rust rows:** `numerics/spectra`, `interchange/ifc`, `drawing/annotation`, `numerics/analysis`, `graphs/layout`, then the rest in RUST.md order, up to three at once.
- **Rust depth:** `documents/typeset` first (a density near 1e6 px/pt passes the pixmap bound and aborts on allocation: the density takes a validated bound from page size and pixel budget inside the crate), `cli/output`, `cli/screen`, `services/mcp`, `geometry/proximity`, `geospatial/site`, `graphs/dependency`, `numerics/optimize`, `graphs/network`, `data/tables`, then each row as it lands, up to three at once.
- **Rust tune:** after each group of commits, before the next landing.
- **TypeScript:** after Atelier commits, `typescript-tune` for `web/media` then `web/ui` (with knip), then `web/hosting`. Once the harness run has also committed and nothing else edits TypeScript, `typescript-sweep` rebuilds every TypeScript project in place: one export per concept with operations as statics, near-duplicate schemas and types across `creative-cloud` stated once at their owner, stringly forms derived, Effect 4 at full depth, agent-facing prose cleaned, and no code moved or extracted without becoming stronger. Its tune follows.
- **Folder guidance:** the `guidance` workflow once Atelier and the running rows have landed.

### Defects to land

- **TypeScript exports:** one export list per module with inline `type` markers is now checked (`style/useExportType` inline, ast-grep `no-split-export-list`); the four `tools/harnesses` files it flags are fixed by the harness run's check. Exports no other module reads are checked next: the `typescript-tune` after Atelier adds knip with each package's real entry points, since a bare run reports entry-imported names as unused.
- **Split concepts:** modules export a model beside its operations or views under separate names (`{ Change, change }` and every `@rasm/indesign` request, `{ Launch, launch }` in `tools/harnesses/instance.ts`, `{ Destination, destination }` in the portfolio uploader, `FontView`, `PreflightProcessView`). Once Atelier and the harness run commit, one agent reads every TypeScript project (libs, apps, tools, tests), folds each concept into one name with its operations, and moves every consumer.
- **Rhino harness crash:** the Rhino lander read every settings getter by reflection and crashed Rhino Beta in `CRhinoZebraAnalysisSettings` (report `Rhinoceros-2026-11-02-063036.ips`), raising the crash dialog although `node` started it. Once the harness run commits, one agent rebuilds the settings snapshot from the settings whose owner is loaded or from Rhino's persisted settings as data, launches each session hidden from its start into an untitled document with no start window, and fixes the C# analyzer findings waiting under `tools/harnesses/rhino`.
- **Opening hosts:** hosts opened by agents and tools raise alert sounds, Dock bounces, crash dialogs, extra instances and start windows, and linger. A read-only investigation compares macOS launch behavior with every launch path (watch-apps, `libs/swift/macos`, Atelier, harnesses, skills); then one implementer, after Atelier commits, puts launch behavior in one place (hidden from launch, no activation, untitled or requested document in the running instance, no splash, finished when work ends), removes scattered logic and guidance, and lands the command-hook rewrites. The Illustrator harness run (`wf_86d76a37-613`, its script corrected) and the stopped ScribeDOOR-for-Illustrator proofs (`wf_667d5b14-a86`) resume after it.
- **Landing order:** each step lands its findings whole in one pass, never as a stop-gap. Done: Atelier, the launch path, the harness repair, the Ghidra skill, function hooks, the TypeScript config and Nx consolidation.
  1. Now: deployment (`DEPLOY.md`), the proof move onto `tools/harnesses/indesign` with the runner deletion and the InDesign host-scripting deny, the `rasm-dependency` refusal fix with its dylint fix, the Illustrator harness run.
  2. `rust-tune` returns: the snippet lander over `.artifacts/workflows/findings/rust-snippets.md`, then one clean-prose agent over the changed skill lines, then commit.
  3. Deployment returns: `typescript-tune` on the Atelier commit, then on `web/media` and `web/ui`, then the `web/hosting` row and its tune, the last [01] TypeScript row.
  4. Illustrator harness returns: the paused ScribeDOOR-for-Illustrator proofs, then Acrobat and Photoshop harnesses, then `harness-depth` per host.
  5. Every TypeScript run has landed: `typescript-sweep`, then its tune.
  6. Decompile runs finish: the Ghidra product migration, then the extension phases in TODO [02] order.
- **Findings ledger:** every investigation's full report sits in `.artifacts/workflows/findings/` and lands whole, each item fixed or refuted with a reason:
  - `typescript-processes.md` and `typescript-config.md`: an agent is landing them now; the `tools/harnesses` tsconfig rows land once the harness run commits.
  - `ghidra-scripts.md` and `ghidra-usage.md`: the skill rebuild is in progress; the product migration (`.artifacts/workflows/ghidra/MIGRATION.md`) lands once the decompile runs finish, including a native `BitmapsOverlap` hook for InPage kerning frames (budgets up to 15,625 s from 16 emulated pixel tests per second).
  - `launch.md`: one launch path in `libs/swift/macos` `Launch.swift` once Atelier commits.
  - Rebuilt-module gap: the harness's first live InDesign batch found the stroke family's 4 pt bottom-edge case reading 4 in InDesign against 1 from the rebuilt module; the product's next proof round through the harness fixes the rebuild.
  - Product proof runners: 78 `run-26.applescript`/`run-27.applescript` copies under `docs/research/applications/adobe/extensions/*/proof/harness` drive InDesign directly; they set the interaction level off now and are deleted once every proof runs through `tools/harnesses/indesign` (after Atelier commits and the harness state format is fixed).
  - Command mining for function hooks: lands with the miner's report, rewriting raw `tsc`, `--skip-nx-cache`, `timeout`, literal `analyzeHeadless` and raw app launches into their repository forms.
  - C# analyzer gaps (`Canvases.cs` and other conditionally compiled sources): an agent is landing them, the `tools/harnesses/rhino` findings once the harness run commits.
- **Harness state format:** `tools/harnesses` fails on Atelier's new automation state (`SchemaError: Missing key …state.running.status`); it is fixed once Atelier commits, before any further InDesign proof.
- **Grid Calculator host proof:** zero functions proven in InDesign; the demo is licensed and the stale agent documents are closed, so its next proof round runs through the harness.
- **ScribeDOOR deadline:** about 88 functions proven and none in InDesign, with the trial ending around 8 November; its InDesign-facing families go first, and its welcome dialog needs the owner's click per launch.
- **Stray commit:** six Atelier files landed inside `387a9b607`; the rest commit with Atelier.
- **Host crashes:** InDesign 2027 Beta crashed under the old-style script proofs; every proof moves onto the harness-owned, launch-hidden host.

### Done

- Zero findings from every compiler, checker and test the Check phase runs.
- Rows over existing code end at or below their base in lines, files and public items, except growth for capability the base lacked or held shallowly, stated per item, and every file objectively better.

## [02]-[TYPESCRIPT_EXTENSION]

Runs beside [01] and extends the TypeScript libraries in depth the way the Rust architecture pass designed the Rust crates; its rows start once the [01] TypeScript rows have landed.

### Method

- **Decompile in full:** every acquired plugin, extension and script, free, trial or bought, is decompiled completely and truthfully, never surveyed from strings, names or readmes. Native code is named and typed in Ghidra, scripts are decoded and named, every algorithm is rebuilt as runnable TypeScript, and every rebuild is proven against the binary (emulation) and the host (InDesign 2027 Beta, or 2026 where a plugin loads only there). A Windows-only build runs on the Mac where its form allows (scripts, CEP and UXP); native Windows code is proven through emulation.
- **Rebuild, never port:** each capability is rebuilt in our own shape and depth as programmatic library capability, never as a port or a shippable script, the way Sidekick became the MCP layer, the Creative Cloud libraries and Atelier. `distilled/` is read to understand what each product does and how deep it goes, never copied: the rebuild states that capability in the repository code bar, its adversarial passes collapsing the products' structs, types and shapes into derived values, algebraic data and parameters and folding logic in where a consumer would otherwise compose it. Where products overlap, the rebuild takes the strongest form and the nuance of each, and nothing genuine any product holds is left out.
- **Levels:** general capability with no host type (grids, margins, leading, color, text and right-to-left math), Creative Cloud capability shared across Adobe hosts, InDesign capability (host reads and writes), and Atelier with its Galley extension composing them. Each capability sits once at the lowest level that holds it whole, never split across levels or scattered into small packages. Nothing pre-existing binds the design: current code is rebuilt where the design needs it.
- **Hosts:** every line of code targets the newest host releases (the 2027 Betas and their successors). InDesign 2026 and Illustrator 2026 are installed only to load and prove products built for them; where an older host differs from the newest, the proof records both and the code follows the newest.
- **Packages:** the newest well-made package owns each domain fact (typesetting, grids, arithmetic, right-to-left text, fonts, color); an older or weaker package is replaced, and nothing is hand-rolled that a package states.
- **Quality:** the [01] bar holds or rises: depth for real, advanced use, the smallest internal and external surface, values derived and parameterized with no stringly type or arbitrary constant, no wrapper, alias, forwarder or shim, current Effect 4 forms (`SchemaTransformation`, `SchemaGetter` and their kin), and fast code with no indirection.

### Phases

1. **Acquire:** done but for the owner's choices below. Assets sit under `docs/research/applications/adobe/extensions/<vendor>-<product>/`, color engines under `docs/research/typescript-extension/sources/`, fonts under `docs/research/typescript-extension/fonts/` and the owner's Paid Premium Fonts drive folder.
2. **Decompile:** `plugin-decompile` (Grid Calculator; ScribeDOOR's InDesign and Illustrator builds, compared so the core both hosts share and each host's part are known; the InPage plugin), `script-decompile` and its second run, and `indesign-internals`. Host proofs run through `tools/harnesses/indesign`, one batch per product, documents without windows, interaction off, and a facts index so no host question is asked twice; a sibling harness per host follows the same shape. Open gaps close before Distill: the four right-to-left products left unproven and the runnable grid functions no case reached.
- **Harnesses:** once `tools/harnesses/indesign` lands, one run per host (Acrobat, Illustrator, Photoshop, Blender, Rhino) builds its sibling to the same depth, and an adversary rebuilds the InDesign one. Each host gets readers over its scripting model (dictionary, object model types, the skills that drive it, docs/research and decompiled host bridges where they settle a fact), over the crashes and dialogs past probes caused and why each probe was naive, then a builder, then adversaries that cut surface, exports, constants and stringly forms and remove every path that can crash the host or raise a dialog, then a live check in the background. The language is the one each host requires or that serves it best, held to the same standards.
- **Harness depth:** `.claude/workflows/harness-depth.js` hardens each harness once it lands and again after each decompile round: miners over past proofs, workarounds, crashes and dialogs; a battery of case authors per need run serially through the harness; one architect grouping findings by cause and one lander landing the general fix; then two unanchored depth passes and a live battery check.
3. **Distill:** `decompile-distill` per proven product, pipelined: `rebuilt/` rewritten into `distilled/` in the repository code shape, 20 to 50 percent fewer lines, proven identical by an A/B harness over every vector and seeded inputs, then reduced again by an adversary with the harness green. `rebuilt/` stays as the address-cited link to the binary; everything after reads `distilled/`.
- **InPage distill:** the value of the paid plug-in is its typesetting logic, not its fonts: nuqta and aerab position adjustment, letter adjustment, the spacing tuner, both kasheeda types with auto kasheeda, kerning from glyph-bitmap overlap, text carving, symbols, the Quran glossary and its word shaping. The distill and the libraries built from it carry that logic for Persian and Arabic over any installed font, reading what each font states (OpenType features, anchors, metrics) and naming no font set; Urdu-only forms stay out. `font-rebuild` research (`docs/research/typescript-extension/fonts/inpage/`: real word spacing, the `salt` spacing alternate, mark anchoring) informs that logic, never couples it to a face.
4. **Clean:** `docs/research` is organized and raised in quality so the agents that rebuild the companion files and run the rows find each capability, its depth and its limits directly, not a summary. Content stays; provenance, meta framing, citation and sourcing language, hedging, licensing language and noise go, duplicated notes merge into the one that owns the topic, and every path a note names resolves.
- **Write-ups:** products stay under `docs/research/applications/adobe/extensions/`. Studies and domain notes name only the product files, functions and proof cases a claim rests on, and carry the capability itself (algorithm, parameters, strongest form, limits), never bare pointers or link lists. The architecture pass reads them as a guide and checks each claim against the distilled code before relying on it.
5. **Packages:** a research fan, one agent per domain (shaping and typesetting, right-to-left and bidirectional text, fonts, color, grids and layout math, units and numerics, document and image formats, Adobe host types), each reading registries and package sources for the newest well-made package that states each fact the distilled code computes by hand, then one adversary comparing the candidates. The chosen catalog rows land at their newest release, prereleases included, and install before Architecture.
6. **Architecture:** a fan of domain readers over every `distilled/`, study, package finding and current library builds one capability matrix: each capability, the products that hold it and their strongest form, the package that states it, and its level. Levels: general (no host type), Creative Cloud (shared across Adobe hosts: text, color, documents, the scripting transport), one folder per app (InDesign, Illustrator, Photoshop, Acrobat, as the Atelier run lays them out), then Atelier and Galley composing them. A capability two hosts share, such as ScribeDOOR's core, sits at Creative Cloud level or below, with each host's part in its app folder. Two adversarial architects then rewrite ARCHITECTURE.md, WORKFLOWS.md and the TYPESCRIPT.md rows in place, each cutting surface, wrappers and restated package facts.
7. **Rows:** each row on `typescript.js`, then `depth.js` on its folder, reviewed against its base, committed, then `typescript-tune`. Rows on disjoint folders run in pairs, 20 minutes apart, and commit together.
- **No bottleneck:** every phase fans out by product or domain, with one judging or landing agent after the fan and adversaries that rewrite in place; no phase gives one agent the whole corpus.

### State

- **Decompile:** `plugin-decompile` holds Grid Calculator (374 of 484 rebuilt functions proven against the binary, none in the host) and ScribeDOOR for InDesign (about 88 proven, none in the host). ScribeDOOR for Illustrator proves on Illustrator 2026. The InPage plugin is binary-proven only, since it is Windows-only. `indesign-internals`: preferences 334 binary and 38 host, with 73 dialog cases waiting on a launch-hidden InDesign; right-to-left 148 of 274. `script-decompile` proved grids, right-to-left (four products left), footnotes and tables; styles and pages run now. Its second run (seven paid hasaninscripts, V-Justify, Kashida, `wf_82142624-10d`) resumes with `prove: true` once the first returns, then the right-to-left gap (four products) and the 14 Table Stroke Adjuster Pro cases.
- **Distill:** the 15 grids products are distilled with A/B at zero mismatches; products whose distilled exports exceed the rebuild's rerun the capped adversary with the next batch.
- **Harnesses:** InDesign, Blender and Rhino harnesses are built; an agent is repairing them (Atelier's state format, Rhino settings, launches through `watch-apps launch`) and proving one live batch per host. The Illustrator harness run and the paused ScribeDOOR-for-Illustrator proofs resume after it; Acrobat and Photoshop follow; `harness-depth` then hardens each.
- **Deployment:** Atelier is rebuilt with every Adobe app as a plugin; an agent is landing `.artifacts/workflows/atelier/DEPLOY.md` (a `.ccx` per UXP host, a `.zxp` per CEP host signed with ZXPSignCmd under Rosetta, the app signed for this Mac, one link endpoint naming each plugin by its application), installed by the standard installers with our `eng/` targets fully automatic.
- **Fonts:** `font-rebuild` rebuilt all 87 fonts from `original/` and is in visual review.
- **Font install:** once `font-rebuild` and the unity pass finish, every font in the owner's drive folders `01.Fonts/00.Paid Premium Fonts` and `01.Fonts/01.Premium Fonts` (the rebuilt InPage set among them) is added through Typeface (`/Applications/Typeface-beta.app`, computer use where it has no scripting), so every app, Adobe ones included, sees them; Adobe Fonts activates only its own catalog, so local fonts reach Creative Cloud apps through the system font set Typeface manages.
- **Font set unity:** after `font-rebuild` passes review, one pass checks the set as a whole, in research and the drive alike: one family per design with its real weights, no face duplicated across families or folders, no symbol, engine-fragment or ornament font mixed into the text families (those stay under `research-only/` or a folder of their own), and no font shipped that covers Persian and Arabic partly; a font that cannot be completed from its original goes to `research-only/` with the reason in the README.
- **Research:** `grids.md` still cites `docs/research/creative-cloud-sources/scratchpad/design/gui-indesign.md`; its facts fold into `grids.md` and the citation goes.
- **Planning:** the first plan (map, six domain studies, ARCHITECTURE.md with nine rows, WORKFLOWS.md) is the base phase 6 rewrites.
- **Owner's choices:** the Quranic Publishing System 4.1 + Plugin is bought and unpacked (plugin 2.1.3 for InDesign 2025, its fonts in research and the owner's drive); InPage was asked on 2026-11-02 for plugin 2.2 for InDesign 2026 and a macOS build, which join the decompile when they arrive. The seven paid hasaninscripts scripts join the second script run. No Apple Developer Program for now. Release Photoshop 2026 is installed beside the Beta.

### Scope

- **InDesign interface:** the TypeScript libraries gain a full interface layer, as `libs/python/blender/bpy/ui` does for Blender and Rasm.UI with Rasm.Rhino.UI do for Rhino: panels, dialogs, preference pages and controls declared as data, bound to every setting and preference by the host's own categories, never hand lists. `indesign-internals` grounds it: the preferences, right-to-left and color modules show how InDesign lays out its panels, binds controls to settings, validates and applies them. It serves Atelier and Galley panels, the rebuilt plug-in capability (Grid Calculator, ScribeDOOR, InPage) as real panels, and agents reading and setting any preference by category. Other Adobe apps follow the same shape later.
- **Typography:** shaping, metrics and typesetting logic, with Persian and Arabic right-to-left support through the packages that own it.
- **Color:** its own package of color spaces (sRGB, CMYK, Lab, Adobe RGB and others), one conversion path between them, and palettes.
- **Grids:** grid systems, grid types and their construction, rule lines, and margin and ruling logic, drawn from graphic design grid theory, usable in InDesign and coupled to no host.
- **Depth:** MCP logic, automation and scripting, data and stores, search kinds and pagination, wherever the current code is shallow.

## [03]-[DARKROOM_WINDOW]

Later work. Investigate first, then plan, then build.

### Goal

Darkroom gets its own window in Rhino, built from the ground up, that replaces the native Rendering panel for anyone who installs it. The window is called Darkroom.
- **What it holds:** every native rendering capability is reachable from it, in one cleaner, unified place. That covers the Rendering panel settings, the render window, post effects, tone mapping, output and the related commands and containers.
- **What it is:** a real window with Darkroom's own buttons, panels and sidebars, in the spirit of Jifto. It is not one more docked container on the right.
- **The host:** Rhino keeps its own Rendering panel. Darkroom stops relying on it, and a Darkroom user uses Darkroom instead.

### Boundary rules (owner, binding)

- **Libraries:** every host capability the work derives is built in `libs/` as a general capability any future app can use. Examples: hosting a window, reading and writing render settings, driving the render pipeline, reaching native render commands.
- **Darkroom:** only Darkroom-specific content lives in `apps/rhino/Darkroom`, meaning its layout, its effect chain, and its choice of what to show.
- **UI libraries:** building a full panel, window or GUI is a capability of both Rasm.UI (host-neutral, Skia) and Rasm.Rhino.UI (Rhino-native, Eto and Rhino.UI). Either one can build it on its own.
- **No coupling:** Rasm.UI and Rasm.Rhino.UI never reference each other. Rasm.Rhino, Rasm.Grasshopper and Rasm.UI never reference each other.
- **No moves:** no code moves between libraries and apps to suit one app.

### Already established (read before starting)

- **Host facts:** `docs/research/darkroom/window/GUI-INVESTIGATION.md` §1.1, "What a plug-in can and cannot extend". It covers Rhino 9 Mac with decompile citations:
  - **Render window:** its command toolbar and status bar are closed to plug-ins.
  - **Stage tabs and side panes:** extra tabs and side panes need a RenderPlugIn (`RenderTabs.RegisterTab`, `RenderPanels.RegisterPanel`).
  - **Rendering panel sections:** these belong to the current renderer alone.
  - **Native containers:** no API changes them.
  - **What a plug-in can add:** its own panel, Options page, Document Properties page, Snapshots client and View properties page.
- **Darkroom inventory and extension points:** `docs/research/darkroom/window/DARKROOM-PLAN.md` §2, the old Darkroom surfaces, and "Rhino rendering extension points".
- **UI research:** `docs/research/darkroom/window/ui-research/` (pro image apps, Rhino renderer UIs, modern Rhino plug-in UIs, Rhino UI techniques).
- **Current state:** Darkroom has no UI, and only restoring a stored chain creates chain edits. Rasm.Rhino's `UI/` folder (namespace Rasm.Rhino.UI) holds windows, panels, containers, toolbars, rows, inputs and Skia surfaces. Nothing in the tree hosts a Rasm.UI scene in Rhino yet.

### Questions to answer

1. **Window kind:** is it a modeless Eto window owned by Rhino's main window, a docked panel that fills a container, or a full document-window-style surface? Settle docking, restore on startup, multiple monitors and macOS 27 window behaviour, from the host's own window classes.
2. **Replacing the Rendering panel:**
   - Can Darkroom register as a RenderPlugIn, or as something else that owns the render settings surface?
   - Does becoming the current renderer give Darkroom the Rendering panel's sections and the render window's tabs and panes?
   - What does Darkroom lose if it is not the renderer, and what does Rhino's own renderer lose if Darkroom is?
3. **Native capability coverage:** list every native rendering command, panel section, render-window control and setting. For each, record how Darkroom reaches it:
   - a RhinoCommon API
   - a command or script call
   - document settings (`RenderSettings`, `PostEffect` and its state, `RenderWindow`)
   - not reachable
   For each unreachable one, decide whether to drop it or link out to the native surface.
4. **Hiding the native surfaces:** can an install hide or replace the Rendering panel, the Render Tools tab and render toolbar entries? Use RUI declarations and the containers file. Prior research says no API changes existing containers, so confirm what an RUI and the plug-in's own containers can do.
5. **Rasm.UI in Rhino without coupling:** how does a Rasm.UI scene (`Hosting/SceneHost.cs`) draw and take input inside a Rhino window while neither library references the other?
   - **Candidate A:** Rasm.Rhino.UI exposes a host surface that speaks only Skia and plain input events, and the app wires a SceneHost to it.
   - **Candidate B:** Rasm.UI hosts itself on Eto. Eto is cross-platform and not Rhino, so this keeps Rasm.UI host-neutral.
   - Decide which glue is general library capability and which is app composition.
6. **Rasm.Rhino.UI or Rasm.UI for Darkroom:** which one fits a dense image-editing window best? Weigh native look and accessibility, custom drawing (scopes, curves, wheels), theming, and Jifto-level polish. Mixing per region is allowed if the glue question above is settled cleanly.
7. **Library gaps:** list what the window needs that neither library has today. Examples: window chrome, sidebars, splitters, tool rails, viewport-linked previews, render progress, history browsing. Build each gap as a general library component, never in Darkroom.
8. **Chain editing:** the window becomes the place where the chain is edited. Confirm the chain-edit API (`Chain/`) carries every edit the window needs: undo through Rhino, document persistence and live viewport update.

### Method when picked up

- **Facts:** take them from Rhino 9 itself, its decompiles in `docs/research/darkroom/window/gui-inv/` and `docs/research/darkroom/window/ui-research/rc/`, RhinoCommon docs, the forum and live probes through the use-rhino and watch-apps skills. Do not guess.
- **Order:** answer questions 1 to 5 before any design, because they decide what is possible. Then design the window. Then land the library capabilities first and Darkroom's content after.
- **Done means:** a live check in Rhino with screenshots, every native rendering capability reachable from Darkroom, and zero findings.
