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
- **Concurrency:** one TypeScript run at a time. Up to three Rust row runs and three `rust-depth` runs overlap, on crates that share no logic, started at least 20 minutes apart, and overlapping runs commit together.
- **Base:** `HEAD` when a run starts. A line citation in the rules applies to its code wherever that code now sits.
- **Watching:** follow a run in `/workflows`, read `.artifacts/workflows/<language>/<key>/PLAN.md` once the Plan phase writes it, and read the diff.

### Order and watch points

- **TypeScript:** `data/search` first, then rows that need it alone, and `creative-cloud/indesign` after `uxp`, `automation` and `typography`.
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

### Rust depth passes

Depth runs follow behind the rows, so no crate stays surface level: each committed row gets one, up to three at once, on crates that share no logic with a running row.
- **Script:** `.claude/workflows/rust-depth.js`, with rust.js's Basis, Sources and Check, one run per crate in RUST.md row order, staggered like the rows, then its commit and `rust-tune`. `services/telemetry`, `cli/command` and `cli/output` go first, since they landed before the depth adversary existed.
- **Passes:** three adversaries in sequence, each rewriting the crate and its consumers in place. The first adds the capability its domain and its dependencies' full member sets state that the crate lacks or exposes shallowly. The second replaces hand logic, wrappers, forwarders and restated types with dependency members and derives each value from its owner. The third rebuilds the whole crate from the ground up in its strongest form.
- **Growth:** an export enters only for an operation a consumer could not perform before, stated per item in `surface.md`. No re-export, alias, convenience overload or variant a parameter already serves.

### State on 2026-11-02

- **TypeScript:** `data/search`, `ai/mcp`, `creative-cloud/uxp`, `graphics/typography`, `creative-cloud/automation` and `creative-cloud/indesign` landed. `data/database` is running, then `web/hosting`, `web/media` and `web/ui`, one at a time, each committed and then tuned. Atelier is rebuilt onto the new `@rasm/indesign` but the installed app predates it: it is reinstalled and checked live in InDesign.
- **Effect 4 mining pass:** the SchemaTransformation pass landed. One more pass mines the other Effect 4 members the TypeScript code restates by hand, where a member states the form, once the lane opens.
- **Quality bar for every language:** thread safety and performance hold as firmly as surface and depth; a gap a review finds (a lowered lint, a dropped `Send`, a starved future) is fixed at its root and the rule or skill line that let it through is corrected.
- **Rust:** `services/telemetry`, `cli/command`, `cli/output`, `cli/screen`, `services/daemon`, `geometry/mesh`, `drawing/linework`, `numerics/stats` and `services/mcp` landed. `geometry/proximity`, `geospatial/site` and `graphs/dependency` are running. The other 30 rows launch as slots free, each disjoint row 20 minutes after the last, honoring [AFTER]. Depth runs follow on committed crates, up to three at once.

### Done

- Zero findings from every compiler, checker and test the Check phase runs.
- Rows over existing code end at or below their base in lines, files and public items, except growth for capability the base lacked or held shallowly, stated per item, and every file objectively better.

## [02]-[TYPESCRIPT_EXTENSION]

Starts once every [01] run has landed. It extends the TypeScript libraries in depth the way the Rust architecture pass designed the Rust crates.

### Method

- **Decompile in full:** every acquired plugin, extension and script, free, trial or bought, is decompiled completely and truthfully, never surveyed from strings, names or readmes. Native code is named and typed in Ghidra, scripts are decoded and named, every algorithm is rebuilt as runnable TypeScript, and every rebuild is proven against the binary (emulation) and the host (InDesign 2027 Beta, or 2026 where a plugin loads only there). A Windows-only build runs on the Mac where its form allows (scripts, CEP and UXP); native Windows code is proven through emulation.
- **Rebuild, never port:** each capability is rebuilt in our own shape and depth as programmatic library capability, never as a port or a shippable script, the way Sidekick became the MCP layer, the Creative Cloud libraries and Atelier. Where products overlap, the rebuild takes the strongest form and the nuance of each, and nothing genuine any product holds is left out.
- **Levels:** general capability with no host type (grids, margins, leading, color, text and right-to-left math), Creative Cloud capability shared across Adobe hosts, InDesign capability (host reads and writes), and Atelier with its Galley extension composing them. Nothing pre-existing binds the design: current code is rebuilt where the design needs it.
- **Packages:** the newest well-made package owns each domain fact (typesetting, grids, arithmetic, right-to-left text, fonts, color); an older or weaker package is replaced, and nothing is hand-rolled that a package states.
- **Quality:** the [01] bar holds or rises: depth for real, advanced use, the smallest internal and external surface, values derived and parameterized with no stringly type or arbitrary constant, no wrapper, alias, forwarder or shim, current Effect 4 forms (`SchemaTransformation`, `SchemaGetter` and their kin), and fast code with no indirection.

### Phases

1. **Acquire:** done but for the owner's choices below. Assets sit under `docs/research/applications/adobe/extensions/<vendor>-<product>/`, color engines under `docs/research/typescript-extension/sources/`, fonts under `docs/research/typescript-extension/fonts/` and the owner's Paid Premium Fonts drive folder.
2. **Decompile:** `plugin-decompile` (native and single products: `acquired/`, `unpacked/`, `binaries/`, `ghidra/`, `recovered/`, `rebuilt/`, `proof/`, a study per product) and `script-decompile` (scripts decoded in batches, then rebuilt, proven and studied per domain). Then the same method over any product still bought and InDesign's own bundles (ARCHITECTURE.md Study [01] to [05]).
3. **Distill:** each proven product's `rebuilt/` is rewritten into `distilled/` in the repository code shape (parameterized, derived, no stringly types, the fewest exports, 20 to 50 percent fewer lines), proven identical by an A/B harness over every vector and seeded inputs, then reduced again by an adversary with the harness green. `rebuilt/` stays as the address-cited link to the binary, and studies, the architecture pass and the rows read `distilled/`.
4. **Clean:** `docs/research` holds facts someone can act on. Meta framing, citation and sourcing language, defensive hedging, licensing language and anything else that is neither a relevant fact nor actionable are removed, and notes left with nothing else are deleted. Every path a study names resolves.
5. **Packages:** one research pass over the newest packages per domain, then the chosen catalog rows land at their newest release and install before any row runs.
6. **Architecture:** after phases 2 to 5, reading each product's `distilled/`, one integration pass rewrites ARCHITECTURE.md and WORKFLOWS.md in place from every study: a capability matrix across all products, each capability placed at its level, then two adversarial architects rewriting in place.
7. **Rows:** once the last [01] TypeScript row lands, the one TYPESCRIPT.md edit, then each extension row on typescript.js (adversary, depth adversary, check), reviewed against its base, committed, then `typescript-tune` on its commit.

### State

- **Decompile:** `plugin-decompile` holds Grid Calculator (named and typed, rebuilt, proving against the binary) and ScribeDOOR (typing rounds). The InPage plugin 2.1.3 and InDesign's own bundles have their own runs. `script-decompile` proves its six domains one at a time through a headless harness, and a second script run holds the seven paid hasaninscripts, V-Justify and Kashida, whose proofs follow the first run's. The ScribeDOOR trial is seven days from 2026-11-01. InPage plugin 2.2 joins when InPage replies.
- **Fonts:** `font-rebuild` researches InPage's spacing and marks and rebuilds the set from `original/` for Persian and Arabic, with visual review, replacing the earlier repair in research and the drive.
- **Planning:** the first plan (map, six domain studies, ARCHITECTURE.md with nine rows, WORKFLOWS.md) is the base phase 6 rewrites.
- **Owner's choices:** InPage's InDesign plugin ships only inside Windows desktop bundles ($225, $325 with Kasheeda, $650 with the full InPage Naskh) and its Mac build is not on sale; the Quranic Publishing System 4.1 + Plugin is bought and unpacked (plugin 2.1.3 for InDesign 2025, its fonts placed in docs/research and the owner's drive); InPage was asked by email on 2026-11-01 for the plugin 2.2 build for InDesign 2026, which joins the decompile when it arrives. manzar.in is dropped as unsupported after the merger. The owner bought the seven paid hasaninscripts scripts; they join the script decompile after its first run.

### Scope

- **InDesign:** the full interface, settings and preferences, derived from the host's own categories, never hand lists. Other Adobe apps follow later.
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
