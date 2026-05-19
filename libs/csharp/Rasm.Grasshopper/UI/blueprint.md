# [H1][GRASSHOPPER_UI_BLUEPRINT]
>**Dictum:** *One GH2-native rail owns editor, canvas, document, and component UI capability.*

<br>

[IMPORTANT] `Rasm.Grasshopper` owns GH2 UI. Do not import `Rasm.Rhino` to reuse UI. Replicate its request/scope/`Use<T>`/`Fin<T>` pattern, and keep direct imports to GH2, Eto, RhinoCommon, and `RhinoApp` UI-thread dispatch.

---
## [1][BOUNDARY_DECISION]
>**Dictum:** *GH2 canvas semantics and Rhino host UI semantics are adjacent, not identical.*

<br>

`Rasm.Grasshopper` keeps direct GH2 ownership. It already receives `Grasshopper2`, `GrasshopperIO`, `Eto`, and `RhinoCommon` through `Directory.Build.props`. It does not receive `Rhino.UI`, and it does not reference `Rasm.Rhino`.

`Rasm.Rhino.UI` remains a Rhino command/document UI boundary. Use its architectural pattern only: request object, host scope, `Use<T>`, `Fin<T>`, UI-thread marshal, protected native call, and native-result snapshot. Do not import `RhinoUi`, `UiIntent`, `PanelOp`, `RasmPanel`, command context, gumball, viewport overlay, or Rhino status/progress rails into `Rasm.Grasshopper` unless a later feature deliberately becomes Rhino-UI-aware.

Direct `Rhino.UI.*` use in `Rasm.Grasshopper` requires an explicit build classification change. Do not hide that dependency behind a `Rasm.Rhino` project reference.

---
## [2][PURPOSE]
>**Dictum:** *Downstream GH2 developers need one dense capability surface, not wrapper fragments.*

<br>

`libs/csharp/Rasm.Grasshopper/UI` provides one high-value OOP boundary over GH2 editor/canvas/document APIs. It exposes native capability through compact typed intents and snapshots. It adds value by unifying scope, UI-thread execution, error rails, document-object selection, snapping, layout, component attributes, and Eto drawing primitives.

The folder is not a component-widget folder. Component UI is one facet under the rail. Canvas, document, object graph, wires, snapping, input, painting, and host interop converge through the same boundary.

[CRITICAL]:
- [NEVER] Add thin wrappers that only rename GH2 calls.
- [NEVER] Create parallel rails for canvas, component, document, and wires.
- [ALWAYS] Add capability by extending one request algebra and one scope model.

---
## [3][CURRENT_FILES]
>**Dictum:** *Current files split one concept into mismatched roots.*

<br>

| [INDEX] | [FILE] | [REAL VALUE] | [FUTURE LOCATION] |
| :-----: | ------ | ------------ | ----------------- |
| **[1]** | `UI/CanvasUi.cs` | Active `Editor`/`Canvas` acquisition, `RhinoApp` UI-thread dispatch, `Fin<T>` protected execution, snapshot, invalidate, pick, instantiate popup, bitmap capture. | Remove. Replace with `UI/Ui.cs` as `GrasshopperUi`, `GrasshopperUiIntent<T>`, and `GrasshopperUi.Scope`. |
| **[2]** | `UI/ComponentUi.cs` | `ComponentSpec.Ui` declaration path, phase rail, context, decision merge, terminal response, GH-safe fallback defaults, `ComponentAttributes` subclass, input-panel/menu/cursor/hover/mouse/key hooks. | Remove from `UI`. Move into root `Component.cs` near `ComponentSpec` and `Component<TSelf>` because component attributes have one owning call site. |

`ComponentUi` stays as capability, not as file identity. Preserve its value as component attribute policy available to component definitions. Collapse implementation into component infrastructure so `UI` owns global GH2 canvas/document capability only.

---
## [4][COMPONENT_UI_MIGRATION]
>**Dictum:** *Component customization belongs beside component definition and creation.*

<br>

Preserve these capabilities from `ComponentUi.cs`:

- Declarative `ComponentSpec.Ui` entry for downstream components.
- Composable UI operations over layout, draw, input panel, menu, cursor, hover, mouse, and key phases.
- Decision algebra for bounds, cursor, hover invalidation, response capture/release/handled, and terminal stop.
- Internal `ComponentAttributes` subclass that translates GH2 callbacks into the Rasm rail.
- Failure policy: native callback failures return GH-safe defaults and do not escape into GH2.

Move plan:

1. Keep `ComponentSpec.Ui` as public component declaration surface.
2. Move `ComponentUi`, phase/context/decision values, and `RasmComponentAttributes` into `Component.cs`.
3. Keep all GH2 callback plumbing internal except the compact declaration type used by component authors.
4. Replace catch-all optional context growth with a compact phase payload model during implementation.
5. Keep persistent UI state on owner document objects, not attributes.

---
## [5][TARGET_ARCHITECTURE]
>**Dictum:** *One rail owns scope; facet files own dense native capability groups.*

<br>

| [INDEX] | [FILE] | [PURPOSE] |
| :-----: | ------ | --------- |
| **[1]** | `UI/Ui.cs` | Singular `GrasshopperUi` boundary. Owns active editor/canvas/document scope, `Use<T>`, UI-thread dispatch, protected native execution, and intent execution. |
| **[2]** | `UI/Document.cs` | Document aggregate rail. Owns document lifecycle snapshots, `Document.Methods` mutations, object-list queries, selection snapshots, undo-aware operation policy. |
| **[3]** | `UI/Canvas.cs` | Canvas control rail. Owns invalidate, bitmap, pick map, paint hooks, window selection flags, object instantiation, active-canvas policy, redraw scheduling, viewport dragging switch. |
| **[4]** | `UI/Layout.cs` | Layout and arrange rail. Owns `IAttributes` bounds/pivots/move, `OCD.AlignObjects`, snapping settings, snapping constraints, resolved snap actions, wire-straightening inputs. |
| **[5]** | `UI/Wires.cs` | Wire rail. Owns `Connections`, `WireEnds`, selected/all wires, wire picking via `Canvas.ResolvePick`, split-wire flow, public wire snapshots. |
| **[6]** | `UI/Input.cs` | Eto/GH2 input rail. Owns `InputPanel`, toolbar bars/items, context menus, keyboard modifier helpers, selection mode mapping, command/option semantics. |
| **[7]** | `UI/Paint.cs` | Drawing rail. Owns paint-layer events, `Skin`, `Shape`, `Shade`, `Capsule`, `Context`, icon/vector drawing, safe disposable Eto resources. |
| **[8]** | `api.md` | Full local GH2/Eto/Rhino API ledger and publicness notes. No architecture prose beyond capability classification. |

Each file contributes one facet to `GrasshopperUi`. Public entry remains one OOP boundary. Files group native capability so implementation stays navigable without creating independent services.

---
## [6][UNIFIED_RAIL]
>**Dictum:** *Capability fans out internally; callers see one polymorphic surface.*

<br>

`GrasshopperUi` owns:

- `Scope`: active `Editor`, `Canvas`, `Document`, object list, document methods, current skin/projection when available.
- `Intent<T>`: one request algebra for read, mutate, draw, pick, layout, wire, input, and component UI actions.
- `Snapshot`: stable ids, counts, bounds, points, wire endpoints, document state, and encoded bitmap bytes crossing out of GH2.
- `Policy`: open editor, require canvas, require document, allow active-canvas mutation, owned-canvas mutation, undo behavior, repaint behavior.
- `Host`: optional Rhino/Eto host hooks through direct native APIs or later explicit Rhino-UI-aware classification.

Callers feed typed intents into `GrasshopperUi.Use<T>()`. Native GH2 objects stay inside the rail unless caller explicitly owns custom canvas/control lifetime.

---
## [7][GH2_CAPABILITY_BASIS]
>**Dictum:** *Installed GH2 XML defines the foundation surface.*

<br>

GH2 provides enough native API for a real advanced canvas/UI layer on macOS: editor singleton access, visible editor repair, active canvas/document access, paint phases, bitmap and pick-map rendering, object and wire picking, document mutation methods, object-list queries, selection state, wire endpoints, component attributes, input panels, toolbars, context menus, Eto drawing, native skinning, snapping settings, snapping constraints, snapping feedback, and narrow object alignment utilities.

Treat GH2 as the base platform. Do not model old GH1 WinForms/WPF assumptions. Use Eto and GH2-native UI types. Mac command/option semantics come from `KeyboardExtensionMethods`, not local platform guesses.

---
## [8][PUBLICNESS_RULES]
>**Dictum:** *XML presence is not enough; publicness defines integration.*

<br>

- `Canvas.AllowedActions` is mutable but only safe on canvases Rasm creates.
- `WireRepository` and `Canvas.WireDrawCache` are internal display caches; use public wire snapshots and `Canvas.ResolvePick`.
- `Canvas.ShowInstantiationPopup` is real; `ShowSearchPopup` decompiles empty and must not be presented as full search API.
- `Document.Methods` is the document mutation facade; construct through `Document.Methods`.
- `DocumentMethods.MoveSelection` is public but decompile evidence shows no useful movement; prefer `IAttributes.Move`, native drag interaction, or explicit layout actions until live-proven.
- `IResponsive` and `IInteraction` docs direct implementers toward `AbstractResponsive` and `AbstractInteraction`.

---
## [9][SOURCE_TRUTH]
>**Dictum:** *Local installed XML and decompile evidence own API claims.*

<br>

Primary sources:

- `scripts/rhino.sh api doctor`.
- `/Applications/RhinoWIP.app/.../Grasshopper2Plugin.rhp/Grasshopper2.xml`.
- `/Applications/RhinoWIP.app/.../Grasshopper2Plugin.rhp/GrasshopperIO.xml`.
- `/Applications/RhinoWIP.app/.../Resources/RhinoCommon.xml`.
- `.cache/nuget/packages/rhinocommon/.../Eto.xml`.
- `ilspycmd` decompile for publicness and stub checks.

`Rhino.UI.dll` exists locally, but `Rhino.UI.xml` is missing. Keep Rhino.UI out of this rail unless build classification changes explicitly.
