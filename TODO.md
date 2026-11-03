# [TODO]

## [01]-[LANGUAGE_REBUILDS]

Rust and TypeScript libraries and their consumers are rebuilt as general capability any future app composes, each app holding its own content: fewer types, constants and items, more derived and parameterized logic, every package used through its own members.

### Bar

- **Depth in little surface:** the domain's full capability under advanced use in the fewest names.
- **Reduction:** a struct, type, enum, error variant, constant or validation exists only where a caller acts on it differently; the rest is deleted.
- **Derivation:** values derive from the type, member or host query that owns them, and one parameterized item serves every variant.
- **Both ends:** a library and its consumers are rebuilt together, each consumer calling it directly.
- **Rebuild, never move:** poor logic is deleted whole and rebuilt in its best form; relocated code is no change.
- **Recorded findings:** RUST.md, TYPESCRIPT.md, `.artifacts/workflows/` and `docs/research` are acted on, never rediscovered.
- **Checks:** run once when a run's work is done, findings fixed once, then run once more.

### Workflows

- **Rows:** `.claude/workflows/{rust,typescript}.js`, one run per `[03]` row of the sibling `RUST.md` or `TYPESCRIPT.md` once its `[AFTER]` rows land.
- **Depth:** `.claude/workflows/depth.js` with `language`, `key` and `base` on each landed folder.
- **Shape:** readers in parallel, one lander that pushes back only on surface growth or regressions, adversaries fixing in place in parallel, one check.
- **Tune:** `rust-tune.js` over a range of Rust commits, between groups of Rust runs; `typescript-tune.js` over a range of TypeScript commits. A fresh clean-prose agent per changed skill closes each.
- **Sweep:** `typescript-sweep.js` rebuilds every TypeScript project in place.
- **Folder guidance:** `guidance.js` once the tree settles.
- **Commits:** each landed run is committed by its own paths and pushed at once.

### State

- **TypeScript:** all fourteen `[03]` rows landed.
- **Rust rows landed:** telemetry, command, output, screen, daemon, mcp, mesh, proximity, planar, implicit, site, solar, dependency, network, layout, optimize, stats, analysis, spectra, tables, linework, annotation, typeset, ifc.
- **Rust rows stopped, to resume:** `numerics/dynamics`, `drawing/views`, `documents/pdf`, `geospatial/weather`.
- **Rust rows left:** form, sheets, workbooks, cad, facets, scene, pointcloud, terrain, features, energy, daylight, query, lake, fields.
- **Depth stopped, to resume:** `cli` over its three crates.

### Queue

1. **Rust:** six runs at once, three rows in RUST.md order and three depth runs, each landing freeing its slot; `rust-tune` from `24ede74fc` after 2 to 3 land, and again every few landings.
2. **InPage:** depth pass, clean-prose agents, `typescript-tune`, then one manual audit (every page of the InPage manual against our logic) with its fixes and a tune if justified.
3. **Then:** one focused agent per open defect, and `typescript-sweep` once no other run edits TypeScript. Depth order: `interchange/ifc`, `geospatial/solar` with `geospatial/site` as one run, `documents/typeset`, then each landed row not yet deepened.
4. **InPage depth rerun:** once the sweep and all other TypeScript work land (it may overlap the extension planning), rerun the depth pass from `.artifacts/workflows/typescript-extension/inpage/depth-prompt.md` (one Fable agent), its prompt extended with every finding and naivety fix recorded since, the matching `docs/research` notes, and deeper fontkit, HarfBuzz and catalog package use only where it replaces hand logic or adds real capability.
5. **Folder guidance:** once the sweep and the Rust queue settle.

### Open defects

- **Split concepts:** modules export a model beside its operations under separate names (`{ Change, change }`, the `@rasm/indesign` requests, `{ Destination, destination }` in the portfolio uploader, `FontView`, `PreflightProcessView`); each folds into one name with its operations.
- **Unread exports:** knip as a root check with each package's real entry points.
- **Stroke gap:** InDesign reads the stroke family's 4 pt bottom-edge case as 4 against 1 from the rebuilt module.
- **Ghidra migration:** `.artifacts/workflows/ghidra/MIGRATION.md` once the decompile runs finish.

## [02]-[TYPESCRIPT_EXTENSION]

Extends the TypeScript libraries in depth from what the most capable InDesign plugins and scripts do.

### Method

- **Decompile in full:** every acquired product is decompiled from its code, every algorithm rebuilt as runnable TypeScript and proven against the binary (emulation) and the host (InDesign 2027 Beta, or 2026 where a product loads only there).
- **Rebuild, never port:** `distilled/` shows what each product does; the libraries state that capability in the repository's own shape, the strongest form where products overlap.
- **Host mechanism, not product shortcut:** a product's fixed name, width or style becomes the host's real mechanism for it (a numbering style built on InDesign's GREP styling, not a hardcoded `HS-DynamicTableNumbering`), and related quantities live in one derived relation any of whose values a caller can solve for.
- **Floor:** the InPage depth pass (`f9bfccb21`) sets the bar every landing meets: typography fell from 18 files and 1141 lines to 16 and 986 while gaining capability. What produced it: a lead reading the plan, the distilled source and these principles whole; separate agents arguing relations that make existing functions uses of one function; packages read from source at their pinned release; deletions landed first and measured per package; adversaries on any growth; a behavior oracle freeing aggressive collapse.
- **Higher-order capability:** the mined products together reveal capability none of them states (alignment as one general relation behind many products' commands); the architecture names each one at its level (universal, Creative Cloud or InDesign) and builds the products' functions on it.
- **Levels:** general (no host type), Creative Cloud (shared across Adobe hosts), one folder per app, then Atelier and Galley composing them; each capability once, at the lowest level that holds it whole.
- **Hosts:** code targets the newest host releases; older releases load only products built for them.
- **Packages:** the newest well-made package owns each domain fact.

### Phases

1. **Acquire:** done. Products sit under `docs/research/applications/<host>/products/<domain>/<key>/`, their distilled code under `docs/research/applications/<host>/distilled/<domain>/<key>/`.
2. **Decompile:** binary proofs through Ghidra's emulator, host proofs through `tools/harnesses/indesign`.
3. **Distill:** `rebuilt/` rewritten into `distilled/` by one distiller per algorithm family, each held identical by its own A/B harness, then adversaries per family and across the product.
4. **Clean and companion file:** 2 to 3 Fable agents read the code bar, the workflow and every relevant `docs/research` file, rebuild the companion file where justified, and clean the typesetting and InDesign notes: provenance and stale lines go, duplicates merge, depth rises.
5. **Packages:** `extension-research` writes each domain note (color, typography, composition, grids, perso-arabic, print) at university depth and `packages.md`, which the architecture reads.
6. **Architecture:** domain readers over every `distilled/` and the domain notes (`color.md`, `typography.md`, `composition.md`, `grids.md`, `perso-arabic.md`, `print.md`, `packages.md`) build one capability matrix, each corrected fact (Bouma spacing, TeX page cost, the Didot unit, kashida degree intervals) placed at its level, then ARCHITECTURE.md, WORKFLOWS.md and the TYPESCRIPT.md rows are rewritten and adversarially confirmed. Then 3 to 4 Opus agents per category write one focused plan file per row or phase: its distilled, research and study content in focused lines, its functionality and higher-order capability, and its packages with imports; the guidance file points to each and keeps the code bar.
7. **Rows:** each row on `typescript.js`, then `depth.js` on its folder, tearing out naive surface from earlier landings.

### State

- **Running:** `internals-finish` (preferences, right-to-left, composers, color, guides), `font-foundry`, and the adversarial review of the InPage landing.
- **InPage landing:** library code committed (`3c293ca22`), plan at `.artifacts/workflows/typescript-extension/inpage/PLAN.md`; its Atelier section stands as uncommitted drafts until the harness session commits.
- **InPage depth pass, on the owner's signal:** after the review, one agent reads the plan, the distilled logic and the [02] method for missed capability, higher-order capability that ripples through the code (4 to 5 agents ideating), and fuller use of Effect 4 and each package; it cuts surface, stringly forms, nesting and hand logic, adversaries read any growth, then clean-prose agents rename files, members and messages over everything modified. It delivers real line and surface reduction: poor logic and indirection are deleted and files removed, never shuffled; additions stand only where justified.
- **Harnesses:** rebuilt on one launch path with no automatic quit, kill, press, or relaunch; a dialog, hang, or crash ends a batch, pid in `failed`.
- **Next proofs:** host cases written and not run (second script decompile, Grid Calculator, ScribeDOOR, script domains) and ScribeDOOR for Illustrator run on it; ScribeDOOR's trial ends around 8 November.
- **InPage:** 164 functions binary-proven over 1494 cases; the plug-in is Windows-only, so host proof is impossible. Its distill carries the typesetting logic (nuqta and aerab position, letter adjustment, spacing tuner, kasheeda, overlap kerning, text carving, symbols, the Quran glossary) for Persian and Arabic over any installed font, reading what each font states; Urdu-only forms stay out.
- **Distilled:** 61 products; pages (23 left), styles (20), right-to-left (8) and tables (3) wait on host proofs.
- **Fonts:** `font-foundry` studies the InPage set through Perso-Arabic typography and font engineering, tosses South Asian and junk faces, rebuilds each kept family to foundry grade and publishes to research and the drive, tossed families into `Rejected/`. Then every font in the drive's `01.Fonts/00.Paid Premium Fonts` and `01.Fonts/01.Premium Fonts` is added through Typeface (`/Applications/Typeface-beta.app`).
- **Owner's choices:** InPage plug-in 2.2 for InDesign 2026 and a macOS build join the decompile when they arrive. No Apple Developer Program for now.

### Scope

- **InDesign interface:** panels, dialogs, preference pages and controls declared as data and bound to every setting by the host's own categories, grounded in `indesign-internals`; it serves Atelier and Galley panels and the rebuilt plug-in capability.
- **Typography:** shaping, metrics and typesetting with Persian and Arabic right-to-left support through the packages that own it.
- **Color:** color spaces (sRGB, CMYK, Lab, Adobe RGB and others), one conversion path between them, and palettes.
- **Grids:** grid systems, rule lines, margins and ruling from grid theory, coupled to no host; any print or digital format is input, house values are caller data.
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
