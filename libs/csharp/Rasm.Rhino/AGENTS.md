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
- When adding construction-output behavior, extend the production `Construction` owner where present; otherwise read `Construction/ROADMAP.md`, route through `RhinoConstruction.Project<TOut>` / `ConstructionOp`, and stop before exposing a public rail that the roadmap cannot prove.
- When authoring runtime scenarios, read `tests/csharp/AGENTS.md`, `tests/csharp/libs/AGENTS.md`, and `tools/rhino-bridge/AGENTS.md` first.

## [3][EXTENSION_GRAMMAR]

- New Rhino concern: create or extend a category owner, not a wrapper facade.
- New document, view, object, or panel watch: route through root `Events.cs` and `WatchBus`.
- New capture setup: route camera, UI, and Exchange capture setup through root `Capture.cs` and `CaptureRecipe`.
- New command input, option, prompt, transform, selection, point event, gumball, or text mode: extend `CommandInputPolicy`, `CommandInputRequest<T>`, `CommandPointEventPhase`, `CommandOption`, and the staged input rail in `Commands/Input.cs`.
- New document mutation: extend `DocumentOp`, `DocumentTransaction`, `DocumentEdit.Commit`, and `DocumentReceiptSlot` in `Commands/Document.cs`.
- New block definition, instance, linked archive, attribute, graph, preview, or cache behavior: extend `BlockOp`, `BlockInstanceTask`, `LinkLifecycle`, `BlockAttributeTask`, `MutationReceipt`, and existing snapshot/cache owners before adding another facade.
- New UI behavior: extend `UiIntent<T>`, `UiViewportPreview`, `UiViewportInteraction<TState>`, `MotionSpec` in `UI/Motion.cs`, or `RasmPanel` / `UiChromeOp<T>` in `UI/Panel.cs` before adding a parallel executor.
- New native return conversion: map nullable to `Option` or `Fin`, bool failure to typed failure, resource lifetime to scoped projection, and document mutation to the existing receipt vocabulary.

## [4][ROUTING]

| [INDEX] | [CONCERN]    | [OWNER]                                                                                        |
| :-----: | :----------- | :--------------------------------------------------------------------------------------------- |
|   [1]   | Watches      | root `Events.cs` and `WatchBus`                                                                |
|   [2]   | Capture      | root `Capture.cs` and `CaptureRecipe`                                                          |
|   [3]   | Commands     | `Commands/` input rails, `DocumentOp`, transactions                                            |
|   [4]   | UI           | `UI/` intents, dispatch, overlays, motion, callbacks                                           |
|   [5]   | Camera       | `Camera/` viewport, named-view, projection, capture                                            |
|   [6]   | Blocks       | `Blocks/` ops, lifecycle, archive/link, graph, preview                                         |
|   [7]   | Exchange     | `Exchange/` import/export, sheet/detail, publish                                               |
|   [8]   | Construction | production `Construction` owner where present; otherwise `Construction/ROADMAP.md` proof route |

## [5][UI_RULES]

- `UI/` owns `UiIntent<T>`, Rhino/Eto dispatch, dialogs, panels, overlays, callbacks, retained canvas state, motion clocks, and drawing-resource lifetimes.
- Keep the Rhino UI executor as the sole dispatch edge; route callbacks, user delegates, native event identity, and scoped disposal through protected native boundaries.
- Model native UI events as typed unions with native identities.
- Treat process-lifetime native registrations, retained overlay state, motion algebra, and per-paint resource disposal as owner-rail concerns.

## [6][REJECTIONS]

- No wrapper-only RhinoCommon renames.
- No public parameter mirror of every native knob.
- No split ownership across managers, helpers, compatibility shims, or parallel rails.
- No hardcoded invisible project policy; use named policies, native defaults, explicit default records, or caller input.
- No functionality removal merely to reduce LOC.
- No stale public names, old union cases, old parameter names, or compatibility nouns after algebra collapse.
- No obsolete native value exposed as compatibility API; project it only inside the owning boundary conversion when live Rhino documents can still emit it, then expose canonical Rasm names.
- No native-member claim based only on member existence; verify semantics when the replacement value matters.

## [7][STOP_RULES]

If RhinoWIP XML, decompile evidence, or the API rail cannot prove semantics, stop before naming a public rail or replacement value; route runtime behavior to source-owned bridge scenarios.
