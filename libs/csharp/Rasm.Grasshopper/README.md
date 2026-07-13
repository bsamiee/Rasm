# [RASM_GRASSHOPPER]

`Rasm.Grasshopper` owns the single Grasshopper 2, Eto, Rhino UI, and macOS host boundary above the `Rasm` kernel: `GhSession`, `EtoDispatch`, and `MacGate` bound live host access, and `Lease<T>` carries every retained resource and its inverse lifecycle. It references no sibling package, admitting the kernel only as a boundary contract.

## [01]-[ROUTER]

[CANVAS]:
- [01]-[CANVAS](.planning/Canvas/canvas.md): `CanvasOperator.Apply` closes commands and settles detached projections over the live host surface.
- [02]-[INTERACTION](.planning/Canvas/interaction.md): `InteractionMount` folds focus, drag capture, menus, and gesture capsules onto the lease spine.
- [03]-[LAYOUT](.planning/Canvas/layout.md): `CanvasLayout` folds arrangements into snap and stretch solvers, sealing move and undo as one mutation.
- [04]-[MOTION](.planning/Canvas/motion.md): Clock and native display-link pacing consume the one shared `MotionDrive.Step` fold.
- [05]-[PAINT](.planning/Canvas/paint.md): `PaintPlan` owns culling, stock custody, temporary leases, and monotonic pigment-egress receipts.
- [06]-[WIRES](.planning/Canvas/wires.md): `WireShape` owns wire route geometry, custom routes, point picking, marquee select, and skin pen pass.

[COMPONENTS]:
- [07]-[ATTRIBUTES](.planning/Components/attributes.md): `ResizableAttributes<T>` owns the chrome spine: callback algebra, resize, snap, cursor, undo.
- [08]-[COMPONENT](.planning/Components/component.md): `SpecComponent<TSelf>` owns self-typed declaration, topology, iteration policy, run ledger.
- [09]-[DATA](.planning/Components/data.md): `GardenData` owns typed data-access transfer, tree algebra, cast-or-convert, host-tolerance projection.
- [10]-[OBJECTS](.planning/Components/objects.md): `NativeObject` owns native-object families, persisted read and assign, and GH1 import admission.
- [11]-[PORTS](.planning/Components/ports.md): `Ports` admits, declares, realizes pin over the carrier/semantic/axis catalog with side-aware binding.

[DOCUMENT]:
- [12]-[DOCUMENT](.planning/Document/document.md): `DocumentScope` owns tiers, lifecycle, persistence, keyed shelves, and undo-sealed mutation gate.
- [13]-[GRAPH](.planning/Document/graph.md): `GraphScope` projects object and connectivity reads and settles wire and membership changes into ledger.
- [14]-[HISTORY](.planning/Document/history.md): `HistoryLedger` seals actions into the branching tree: stride, re-root, replay, autosave, reconcile.
- [15]-[SOLUTION](.planning/Document/solution.md): `SolutionControl` closes launch, halt, cancel, and deferred expiry over the leased run lifecycle.

[ETO]:
- [16]-[BINDING](.planning/Eto/binding.md): `BindingRail` fuses control and model through binding machinery with value-gate admission and store rows.
- [17]-[CONTROLS](.planning/Eto/controls.md): `ControlForge` folds the control-spec tree into tagged plant on Eto, absorbing modality into role rows.
- [18]-[RUNTIME](.planning/Eto/runtime.md): `EtoDispatch` owns the UI-thread floor: blocking, awaitable, queued, pump lanes, clock, transfer algebra.
- [19]-[WINDOWS](.planning/Eto/windows.md): `CommandDeck` mints command rows and folds lease-owned menus, closing window, dialog, picker construction.

[PLATFORM]:
- [20]-[COMPOSITION](.planning/Platform/composition.md): `Compose` materializes layers into leased mounts: drives, effects, Display-P3 wide-colour.
- [21]-[HANDLERS](.planning/Platform/handlers.md): `Handlers` owns Eto handler seam: demand, widget-to-handler mint, frozen stylers, native embedding.
- [22]-[NATIVE](.planning/Platform/native.md): `MacGate` gates macOS AppKit touch: monitor and gesture leases, pressure restore, conversion, pacing.

[SHELL]:
- [23]-[CHROME](.planning/Shell/chrome.md): `Chrome` folds toolbar, input-panel, tooltip, floating-button intent onto GH2 chrome hosts through gate.
- [24]-[EDITOR](.planning/Shell/editor.md): `EditorShell` projects chrome-pane slots, mutates boolean posture, returns state, owns Rhino getter.
- [25]-[EVENTS](.planning/Shell/events.md): `UiEvents` binds GH2 and Eto streams to typed fact cases, minting one leased subscription in sink order.
- [26]-[ICONS](.planning/Shell/icons.md): `IconOwner` admits host icon origins, keyed poses, filter chain, modalities, composing rasters into recency.
- [27]-[SESSION](.planning/Shell/session.md): `GhSession` closes session work and repaint receipts, bounding projections and keying session cache.

## [02]-[DOMAIN_PACKAGES]

Host assemblies projected from the installed Rhino host application; this folder's `.api/` catalogues own the admitted member surfaces.

[MANAGED_HOST]:
- `Grasshopper2` — the component, canvas, document, and solution host surface
- `GrasshopperIO` — `IReader`/`IWriter` host-document persistence
- `RhinoCommon` — Rhino document and geometry carriers plus the getter and dialog handoff
- `Rhino.UI` — Rhino styling and the native UI bridge
- `System.Drawing.Common` — compile-time GDI carrier interop at the GH1 icon boundary

[UI_TOOLKIT]:
- `Eto` — the cross-platform UI toolkit: forms, drawing, binding, dispatch, controls, windows, and input
- `Eto.macOS` — the AppKit backend and `IMacControlHandler` view roles

[PLATFORM_NATIVE]:
- `Microsoft.macOS` — AppKit, CoreAnimation, CoreGraphics, CoreImage, and Foundation bindings behind the gated native owners

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry, plus the folder-specific recency cache; the registry and `libs/csharp/.api/` own the full contracts.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[RECENCY_CACHE]:
- `Microsoft.Extensions.Caching.Hybrid` — tagged L1/L2 recency and stampede control for document-scoped `SessionCache` values
