# [RASM_RHINO_AGENTS]

Scope: `libs/csharp/Rasm.Rhino/` only. Root policy and `libs/csharp/AGENTS.md` own universal C# and library-family rules; this file adds RhinoWIP boundary deltas.

## [1][SCOPE]

`Rasm.Rhino` is the canonical RhinoWIP boundary library for command, UI, camera, viewport, document, exchange, block, construction, and future Rhino concern categories.

Each category folder owns one full Rhino concern. Capture native API capability deeply, then expose a smaller, stronger, value-adding boundary with FP/ROP internals. Downstream code should read like product logic, not RhinoCommon sequencing, option plumbing, or ceremony.

## [2][READ_ORDER]

- When editing a folder, read existing folder files and extend canonical owners before creating new files or types.
- When naming risky Rhino behavior, verify against local RhinoWIP XML, decompile evidence when XML is absent, and the repo API rail.
- When changing `System.*`, package/reference, raster, filesystem, or host-provided assembly decisions, read `docs/system-api-map`.
- When adding numerical or symbolic algorithms, read `docs/external-libs/mathnet`.
- When authoring runtime scenarios, read bridge guidance first.

## [3][EXTENSION_GRAMMAR]

- New Rhino concern: create or extend a category owner, not a wrapper facade.
- New document, view, object, or panel watch: route through root `Events.cs` and `WatchBus`.
- New capture setup: route camera, UI, and Exchange capture setup through root `Capture.cs` and `CaptureRecipe`.
- New command input, option, prompt, transform, selection, point event, gumball, or text mode: extend the case-row rail.
- New mutation behavior: apply redraw, commit, disposal, and UI-thread protection at the boundary edge.
- New native return conversion: map nullable to `Option` or `Fin`, bool failure to typed failure, resource lifetime to scoped projection, and document mutation to the existing receipt vocabulary.

## [4][ROUTING]

| [INDEX] | [CONCERN]    | [OWNER]                                               |
| :-----: | :----------- | :---------------------------------------------------- |
|   [1]   | Watches      | root `Events.cs` and `WatchBus`                       |
|   [2]   | Capture      | root `Capture.cs` and `CaptureRecipe`                 |
|   [3]   | Commands     | `Commands/` staged execution and document mutation    |
|   [4]   | UI           | `UI/` Rhino/Eto integration and UI-thread protection  |
|   [5]   | Camera       | `Camera/` viewport, named-view, projection, capture   |
|   [6]   | Blocks       | `Blocks/` definitions, instances, graphs, previews    |
|   [7]   | Exchange     | `Exchange/` import/export, sheet/detail, publish      |
|   [8]   | Construction | `Construction/ROADMAP.md` until production code lands |

## [5][UI_RULES]

- `UI/` owns Rhino/Eto dispatch, dialogs, panels, overlays, callbacks, retained canvas state, motion clocks, and drawing-resource lifetimes.
- Keep the Rhino UI executor as the sole dispatch edge; route callbacks and user delegates through protected native boundaries.
- Model native UI events as typed unions with native identities.
- Treat process-lifetime native registrations, retained overlay state, motion algebra, and per-paint resource disposal as owner-rail concerns.

## [6][REJECTIONS]

- No wrapper-only RhinoCommon renames.
- No public parameter mirror of every native knob.
- No split ownership across managers, helpers, compatibility shims, or parallel rails.
- No hardcoded invisible project policy; use named policies, native defaults, explicit default records, or caller input.
- No functionality removal merely to reduce LOC.
- No stale public names, old union cases, old parameter names, or compatibility nouns after algebra collapse.
- No native-member claim based only on member existence; verify semantics when the replacement value matters.

## [7][STOP_RULES]

If static managed gates cannot prove native Rhino behavior, route to bridge scenarios. If host API semantics are unverified, use XML, decompile, API rail, or an explicit proof gap before publishing the claim.
