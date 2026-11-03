# [TODO]

## [01]-[LANGUAGE_REBUILDS]

Libraries in every language and their consumers are rebuilt as general capability any future app composes, each app holding its own content.

### Bar

`.claude/workflows/BAR.md` holds the code bar, the hunk judgment and the agent rules every workflow prompt names once.

### Workflows

- Build: `deepen.js` with `mode` build, `language`, `key` and `base`, per `[03]` row of `RUST.md` or `TYPESCRIPT.md` after its `[AFTER]` rows commit
- Deepen and reduce: `deepen.js` with `mode` deepen or reduce, `language`, `base`, one of `path`, `paths` or `key`, `exclude` another run's folders
- Stages: map, readers per file group and root, per group an editor, adversaries in sequence and a verdict, cross-group sets, a lead, one check
- Tune: `tune.js` with `language`, `base` and `exclude` mines commits since `base` into that language's skills and checks
- Rows: `languages.js` holds each language's skills, sources, release, companion, catalog and checkers for `deepen.js` and `tune.js`
- Extension research: `extension-research.js` with `key` one domain note, or `architecture` for the capability matrix and `TYPESCRIPT.md` rows
- Harness depth: `harness-depth.js` with `key`, `base`, `name`, `apps`, `skills` and `products`
- Folder guidance: `guidance.js` once the tree is committed after a round of rebuilds
- Commits: tree committed and pushed before the InPage rerun, after its work closes, and before the extension phases

### State

- **TypeScript:** all fourteen `[03]` rows built.
- **Rust paused:** no new run starts until the TypeScript extension closes; the runs below finish and count as built.
- **Rust rows built:** telemetry, command, output, screen, daemon, mcp, mesh, proximity, planar, implicit, site, solar, dependency, network, layout, optimize, stats, analysis, spectra, dynamics, form, tables, linework, views, annotation, sheets, typeset, ifc, weather, pdf, workbooks, cad.
- **Rust rows left:** facets, scene, pointcloud, terrain, features, energy, daylight, query, lake, fields.
- **Deepened:** cli, typeset, ifc, analysis, layout, implicit, dependency, annotation, mesh, site, solar, weather.
- **Deepen left:** telemetry, daemon, mcp, network, stats, optimize, spectra, dynamics, proximity, planar, form, tables, linework, views, sheets, pdf, workbooks, cad, and each row above once it is built.
- **Checks owed:** `rasm-sheets` against the deepened annotation.

### Queue

1. **TypeScript close:** the TypeScript audit is committed (`5febba4b7`); the workflow review (five reviewers and the lead's findings in `.artifacts/workflows/review/`) rebuilt the workflows into `deepen.js` and `tune.js`; then `tune.js` over TypeScript from `2a7c018e2`.
2. **Push**, then the TypeScript extension phases 4 to 7 in [02].
3. **Harnesses:** run logging is committed (`1193208fe`); the depth pass in `.artifacts/workflows/harnesses/depth-brief.md` and the app launch route pass in `.artifacts/workflows/launch/depth-brief.md` (both running), which also routes waits on Claude Code task output to the completion notice and leaves Swift at zero findings; then the open host proofs.
4. **Font library:** `libs/python/typography` and the `foundry` CLI at `tools/foundry` (running) from `.artifacts/workflows/fonts/DECISION.md` and `ARCHITECTURE.md`, 2,650 to 4,000 lines; a general tool for building, refining and verifying any font, Perso-Arabic one domain of it, each change rebuilding only what it reaches, with no flag a question does not need; each round pushed; then 2 to 3 `deepen.js` rounds over both paths with `.artifacts/workflows/fonts/deepen-brief.md` as input, each from the round before, the first collapsing the type, class, constant and string spam the build left into fewer, richer objects with higher-order operations for fonts in general and agent-facing CLI use; then `tune.js` over Python, which can run beside InPage.
5. **Font files:** `assets/fonts` rebuilt through the library: one folder per family holding its faces, quality raised with agents reading rendered glyphs for weight, proportion, joining and mark placement against Perso-Arabic principles, and each file stating its metrics, features and behavior completely; it follows the library rounds and blocks nothing.
6. **Rust, after the TypeScript extension:** three build runs in RUST.md order and three deepen runs at once, each commit freeing its slot; `tune.js` over Rust every few commits.
7. **Folder guidance:** once the TypeScript extension and the Rust queue are committed.

### Open defects

- **Stroke gap:** InDesign reads the stroke family's 4 pt bottom-edge case as 4 against 1 from the rebuilt module.
- **Ghidra migration:** `.artifacts/workflows/ghidra/MIGRATION.md` once the decompile runs finish.

## [02]-[TYPESCRIPT_EXTENSION]

Extends the TypeScript libraries in depth from what the most capable InDesign plugins and scripts do.

### Method

- **Decompile in full:** every acquired product is decompiled from its code, every algorithm rebuilt as runnable TypeScript and proven against the binary (emulation) and the host (InDesign 2027 Beta, or 2026 where a product loads only there).
- **Rebuild:** use `.claude/workflows/TYPESCRIPT.md` for the method every extension run's agents read.

### Phases

1. **Acquire:** done. Products sit under `docs/research/applications/<host>/products/<domain>/<key>/`, their distilled code under `docs/research/applications/<host>/distilled/<domain>/<key>/`.
2. **Decompile:** binary proofs through Ghidra's emulator, host proofs through `tools/harnesses/indesign`.
3. **Distill:** `rebuilt/` rewritten into `distilled/` by one distiller per algorithm family, each held identical by its own A/B harness, then adversaries per family and across the product.
4. **InPage:** `deepen` with `.artifacts/workflows/typescript-extension/inpage/args.json` and `base` the commit pushed before launch, then `tune` with `language` typescript from that base.
5. **Research:** one `extension-research` run per domain note at a time, in the order typography, perso-arabic, composition, grids, indesign, data, mcp, print, color, each pushed before the next.
6. **Architecture:** `extension-research` with `key` architecture writes the capability matrix into `ARCHITECTURE.md` and the `TYPESCRIPT.md` [03] rows, and runs again over a family once its distill closes.
7. **Rows:** one `deepen` run per `TYPESCRIPT.md` [03] row once its `[AFTER]` rows are committed and its gates answered: `mode` build with `key`, `base` the commit pushed before it, `inputs` the evidence its matrix rows name, `oracle` a product proof covering it and `exclude` a nested row's folder, the typography row in `mode` deepen over the InPage result; then `tune` with `language` typescript from the row's base, pushed before the next row.

### State

- **Running:** `internals-finish` on color alone with one binary round (preferences, right-to-left, composers and guides distilled).
- **InPage:** library and Atelier committed; plan at `.artifacts/workflows/typescript-extension/inpage/PLAN.md`; oracle `proof/ab.ts` in `docs/research/applications/indesign/products/typesetting/inpage-typesetting-tool`.
- **Research notes:** cleaned to principles at their owners, pre-clean copies in `.artifacts/workflows/typescript-extension/base/`.
- **Harnesses:** one launch path for every host, each run logged under `.artifacts/logs/harnesses/` with its summary in the CLI result; agents launch, drive and quit apps through watch-apps, and the hook routes every other form there.
- **Next proofs:** host cases written and not run (second script decompile, Grid Calculator, ScribeDOOR, script domains) and ScribeDOOR for Illustrator run on it; ScribeDOOR's trial ends around 8 November.
- **InPage proofs:** 164 functions binary-proven over 1494 cases; the plug-in is Windows-only, so host proof is impossible. Its distill carries the typesetting logic (nuqta and aerab position, letter adjustment, spacing tuner, kasheeda, overlap kerning, text carving, symbols, the Quran glossary) for Persian and Arabic over any installed font, reading what each font states; Urdu-only forms stay out.
- **Distilled:** 61 products; pages (23 left), styles (20), right-to-left (8) and tables (3) wait on host proofs.
- **Owner's choices:** InPage plug-in 2.2 for InDesign 2026 and a macOS build join the decompile when they arrive. No Apple Developer Program for now.

### Scope

- **InDesign interface:** panels, dialogs, preference pages and controls declared as data and bound to every setting by the host's own categories, grounded in `indesign-internals`; it serves Atelier and Galley panels and the rebuilt plug-in capability.
- **Typography:** shaping, metrics and typesetting with Persian and Arabic right-to-left support through the packages that own it.
- **Color:** color spaces (sRGB, CMYK, Lab, Adobe RGB and others), one conversion path between them, and palettes.
- **Grids:** grid systems, rule lines, margins and ruling from grid theory, coupled to no host; any print or digital format is input, house values are caller data.
- **Document structure:** pages, styles, tables and footnotes as host objects the distilled scripts drive, rebuilt on the host's own mechanisms.
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
