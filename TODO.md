# [TODO]

## [01]-[LANGUAGE_REBUILDS]

Rebuild the Rust, Swift and TypeScript libraries and their consumers the way the C# and Python code was rebuilt. Libraries hold general capability any future app composes, and each app holds its own content. The work reduces surface and raises capability: fewer constants, types, records and items, more derived and parameterized logic, every package used fully.

### What a run produces

- **Less surface:** the fewest exported and internal names, types, constants and helpers the row allows.
- **Value:** a change improves code only when its result does more with less than its base. Every declaration earns its place through an operation a caller performs, never through a rule, a row cell or the old code alone.
- **Both ends:** a library and its consumers are rebuilt together and neither shapes the other. Each consumer calls the library directly with no adapter, wrapper or reshaping.
- **Measured:** each run measures lines, declarations and exports per file at base and after (`git archive HEAD`, `loc`, `ast-grep outline`) and removes growth no capability requires before it ends.
- **Parameters over declarations:** one parameterized declaration serves every variant, and a consumer reaches a variant through an argument.
- **Derived values:** each value derives from the type, member or host query that owns it.
- **Packages used whole:** Effect 4, rmcp, the catalog crates and the macOS SDK do their work through their own members, never a wrapper or hand logic.
- **One concept per file:** no `index.ts`, barrel, `main`, `lib` or `mod` file collects a package's logic, and no folder holds a single file.
- **Crate roots:** a Rust crate root holds the crate's code beside its modules.
- **Dependencies:** a package depends only on the packages whose members it calls, with no cycle.
- **Delete and rebuild:** logic built poorly is deleted whole and rebuilt in its best form, never kept beside its replacement, wrapped, aliased or bridged.
- **No shuffling:** a concept sits where the architecture places it. A misplaced concept is rebuilt in its new place, and relocating the same code is no improvement.
- **Errors:** TypeScript keeps one `Schema.TaggedError` per package with a `reason` literal, a `subject` and an optional `cause`.
  - Its error covers only failures no host error or `Unmatched` states.
  - `Unmatched`, `ReconcileError`, `InspectError` and `PreconditionFailed` are rebuilt to that form, and no near-copy enters.
  - Rust and Swift follow the error forms of their skills.
- **No anchoring:** a run designs from the row, the skill and the package sources, never from the shape of the current code.
- **Base check:** after landing, the Check phase compares each rebuilt file with its version at the run's start and restores what that version did better.

### Running the workflows

- **Scripts:** `.claude/workflows/{rust,swift,typescript}.js`, with rules and target rows in the sibling `RUST.md`, `SWIFT.md` and `TYPESCRIPT.md`.
- **Skills:** each run loads `Skill(<language>-coding)` and its companions first, and fixes a skill where a run finds a stronger form.
- **Rows:** one run per `[03]` row, with `args.key` set to its `[KEY]`, started after the rows its `[AFTER]` cell names have landed.
- **Concurrency:** one TypeScript run at a time. Rust runs on rows that share no dependency overlap when started at least 20 minutes apart, and overlapping runs commit together.
- **Base:** `HEAD` when a run starts. A line citation in the rules applies to its code wherever that code now sits.
- **Watching:** follow a run in `/workflows`, read `.artifacts/workflows/<language>/<key>/PLAN.md` once the Plan phase writes it, and read the diff.

### Order and watch points

- **Swift:** `core`, then `macos/process`, then `macos/applications`.
- **TypeScript:** `data/search` once Swift `core` lands, then rows that need it alone, `creative-cloud/automation` once Swift `macos/applications` lands, and `creative-cloud/indesign` after `uxp`, `automation` and `typography`.
- **Rust:** `services/telemetry`, `cli/command`, `services/daemon`, `services/mcp`, with `cli/output` and `cli/screen` after `cli/command`.
- **Later Rust rows:** each starts once its `[AFTER]` rows have landed.
- **Rust `services/telemetry`:** owns all of `libs/rust` for the move into categories. Read its plan before its Land phase goes far.
- **MCP:** Rust `services/mcp` and TypeScript `ai/mcp` are the capability every agent-facing app builds on.
  - Each states sessions, routing, progress, resources, elicitation, tasks, client and proxy in full, on rmcp and `effect/ai`.
  - Neither takes its shape from the current servers.
- **TypeScript `web/ui`:** its plan justifies each component as general capability, never a port of one portfolio screen.

### Done

- Zero findings from every compiler, checker and test the Check phase runs.
- Rows over existing code end with fewer lines, files and public items than their base, and every file objectively better.

## [02]-[DARKROOM_WINDOW]

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
