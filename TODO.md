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
- **Concurrency:** TypeScript rows on folders that share no files overlap, started at least 20 minutes apart, and commit together. Up to three Rust row runs and three `depth` runs overlap, on folders that share no logic, started at least 20 minutes apart, and overlapping runs commit together.
- **Base:** `HEAD` when a run starts. A line citation in the rules applies to its code wherever that code now sits.
- **Watching:** follow a run in `/workflows`, read `.artifacts/workflows/<language>/<key>/PLAN.md` once the Plan phase writes it, and read the diff.

### Order and watch points

- **TypeScript:** `data/search`, `ai/mcp`, `creative-cloud/uxp`, `graphics/typography`, `creative-cloud/automation` and `creative-cloud/indesign` landed. `data/database` is running; it is committed and tuned, then one pass gives a type and const naming one concept one shared name across the TypeScript code, where that case exists, and the skill and `TYPESCRIPT.md` are confirmed to state it. `web/media` and `web/ui` then run together, 20 minutes apart, since they import neither each other nor any shared portfolio file; they commit together and each is tuned. `web/hosting` follows alone, since it shares `vite.config.ts` with media and two portfolio files with ui. Atelier is rebuilt onto the new `@rasm/indesign` but the installed app predates it: it is reinstalled and checked live in InDesign.
- **Rust:** nine rows landed: `services/telemetry`, `cli/command`, `cli/output`, `cli/screen`, `services/daemon`, `services/mcp`, `geometry/mesh`, `drawing/linework`, `numerics/stats`. `geometry/proximity`, `geospatial/site` and `graphs/dependency` are running, and 30 rows have not started. Depth runs start with `services/telemetry`, `drawing/linework` and `numerics/stats`, once `rust-tune` on 16b941624 is committed.
- **Later Rust rows:** each starts once its `[AFTER]` rows have landed.
- **Rust `services/telemetry`:** owns all of `libs/rust` for the move into categories. Read its plan before its Land phase goes far.
- **MCP:** Rust `services/mcp` and TypeScript `ai/mcp` are the capability every agent-facing app builds on.
  - Each states sessions, routing, progress, resources, elicitation, tasks, client and proxy in full, on rmcp and `effect/ai`.
  - Neither takes its shape from the current servers.
- **TypeScript `web/ui`:** its plan justifies each component as general capability, never a port of one portfolio screen.

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

### State

- **Decompile:** `plugin-decompile` holds Grid Calculator (374 of 484 rebuilt functions proven against the binary, 0 in the host; the vendor installer now licenses the demo in InDesign 2026) and ScribeDOOR for InDesign (about 88 proven, none in the host; its trial ends around 8 November and its welcome dialog needs one click per launch). ScribeDOOR for Illustrator decompiles beside it on Illustrator 2026. The InPage plugin is binary-proven only (Windows-only). `indesign-internals`: preferences 334 binary, 38 host, 73 dialog cases pending a launch-hidden InDesign; right-to-left 148 of 274. `script-decompile` proved grids, right-to-left (four products left), footnotes and tables; styles, then pages run next; its second run (seven paid hasaninscripts, V-Justify, Kashida) waits for proofs with `prove: true`.
- **Distill:** the 15 grids products are distilled with A/B at zero mismatches; products whose distilled exports exceed the rebuild's rerun the capped adversary with the next batch.
- **Harnesses:** `tools/harnesses/indesign` landed; the `harnesses` workflow rebuilds it and builds Blender and Rhino under the no-crash rules; Acrobat, Illustrator and Photoshop follow the Atelier landing; `harness-depth` then hardens each.
- **Deployment:** `.artifacts/workflows/atelier/DEPLOY.md` plans the artifacts (.ccx per UXP host, signed .zxp per CEP host signed with ZXPSignCmd under Rosetta, the app signed for this Mac) installed by the standard installers, with our eng/ targets fully automatic. It lands after the atelier workflow.
- **Fonts:** `font-rebuild` rebuilt all 87 fonts from `original/` and is in visual review; then the unity pass.
- **Owner's choices:** InPage plugin 2.2 for InDesign 2026 is requested from InPage (reply sent 2026-11-02). No Apple Developer Program for now. Release Photoshop 2026 is being installed beside the Beta.

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
