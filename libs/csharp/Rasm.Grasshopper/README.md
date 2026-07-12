# [RASM_GRASSHOPPER]

`Rasm.Grasshopper` is the single Grasshopper 2, Eto, Rhino UI, and macOS host boundary above the `Rasm` kernel; it references no sibling package. Six folder-mirrored namespaces separate canvas behavior, component authoring, document control, Eto UI, native platform work, and shell orchestration inside one assembly. `GhSession`, `EtoDispatch`, and `MacGate` bound live host access, while `Lease<T>` carries every retained resource and inverse lifecycle.

## [01]-[ROUTER]

[CANVAS]:

- [01]-[CANVAS](.planning/Canvas/canvas.md): `CanvasOperator.Apply` closes `CanvasOp` commands, `Read` settles `CanvasQuery` into detached `CanvasProjection` values, and `FlexPulse` serves non-canvas redraw; monotonic receipts and owned raster leases cross the boundary.
- [02]-[INTERACTION](.planning/Canvas/interaction.md): `Dispatch` mounts declarative `ResponderSpec` targets through one `InteractionMount`; the same lease spine owns focus, drag capture, context-menu population, and the `DragSession`/`EdgeResize` gesture capsules.
- [03]-[LAYOUT](.planning/Canvas/layout.md): `CanvasLayout` folds each `Arrangement` into host snapping and stretch solvers plus residual pivot deltas, then seals the move and its `PivotAction` undo rows as one document mutation.
- [04]-[MOTION](.planning/Canvas/motion.md): Host `Animated<T>` tweens, `FlexDrive`, and `GlyphPath` compose beside lease-owned `CanvasPacer`; both clock pacing and native display-link pacing consume the shared `MotionDrive.Step` fold.
- [05]-[PAINT](.planning/Canvas/paint.md): `PaintAnchor` owns phase attachment and callback outcomes; declarative `PaintFrame` planners emit `Mark` values, raw painters receive event-scoped `PaintScene`, and `PaintPlan` owns culling, stock custody, temporary leases, and monotonic receipts.
- [06]-[WIRES](.planning/Canvas/wires.md): `WireShape` owns route geometry and `ShapeType` installation, public canvas queries own point picking, `WindowSelection` owns marquee selection, and `WireSkin` owns the pen pass.

[COMPONENTS]:

- [07]-[ATTRIBUTES](.planning/Components/attributes.md): `ChromeEvent` and `ChromeDecision` close the callback algebra; `ComponentChrome`, `ChromeTrace`, and `ChromeDispatch` feed both host shells, while `ResizableAttributes<T>` retains resize, snap, cursor, persistence, and undo ownership.
- [08]-[COMPONENT](.planning/Components/component.md): `ComponentSpec` is the constructor-safe declaration consumed by `SpecComponent<TSelf>`; `Execution` owns topology, `IterationPolicy` owns iteration arrays, and every process path seals emission and lifecycle evidence.
- [09]-[DATA](.planning/Components/data.md): `GardenData` owns typed `IDataAccess` transfer across item, pear, twig, and tree topology; `Coerce` ranks broker and conversion-server evidence, and `HostUnits` projects host tolerance into the kernel context.
- [10]-[OBJECTS](.planning/Components/objects.md): `NativeKind` catalogs the public special, routing, `Cluster`, `Chain`, and `ScribbleObject` families; `PersistedValue` closes public state, `NativeObject` owns polymorphic operations, and `Gh1Import` is the sole GH1 admission.
- [11]-[PORTS](.planning/Components/ports.md): `PortRow` couples semantic family, carrier, capability axes, and side-aware `PortBinding`; `PinPlan` and complete `PinTrim` carry writable policy, while `Ports` admits, declares, and realizes every pin.

[DOCUMENT]:

- [12]-[DOCUMENT](.planning/Document/document.md): `DocumentScope` owns document tiers, lifecycle, persistence, keyed shelves, and facet reads; `Transact` pairs every `GraphTransact` host mutation with its undo seal and causal event evidence.
- [13]-[GRAPH](.planning/Document/graph.md): `GraphScope.Ask` projects `ObjectList` and `Connectivity` reads, while `Mutate` settles wire and membership changes through `Connections`, `ObjectList`, and `SplitWire` under one undo seal.
- [14]-[HISTORY](.planning/Document/history.md): `HistoryLedger` seals `ActionList` records into the branching `History` tree and owns stride, re-root, replay, object records, autosave, and branch reconciliation.
- [15]-[SOLUTION](.planning/Document/solution.md): `SolutionControl.Drive` closes launch, await, halt, cancel, deferred expiry, and explicit expiry; `Watch` and `Trace` turn the leased six-row solution lifecycle into ordered run evidence.

[ETO]:

- [16]-[BINDING](.planning/Eto/binding.md): `BindingRail` fuses control and model through the installed binding machinery, `ValueGate` admits typed conversion, `DataScope` owns context assignment, and `StoreRail` owns collection carriers.
- [17]-[CONTROLS](.planning/Eto/controls.md): `ControlForge.Realize` folds the recursive `ControlSpec` tree into a tagged `ControlPlant`; role rows absorb modality, field values stay typed, and interactive view verbs share one mutation surface.
- [18]-[RUNTIME](.planning/Eto/runtime.md): `EtoDispatch` owns blocking, awaitable, queued, and pump lanes; `UiClock` owns monotonic timer beats, `Transfer` closes clipboard/data-object and drag payloads, and `Display`/`InputState`/`NoticeSurface` project ambient host facts and leased notification resources.
- [19]-[WINDOWS](.planning/Eto/windows.md): `CommandDeck` mints command rows, `MenuNode` folds lease-owned menus, `WindowSpec` and `WindowVerb` own live windows, and `DialogSpec<TResult>` plus `PickerSpec` close typed presentation.

[PLATFORM]:

- [20]-[COMPOSITION](.planning/Platform/composition.md): `LayerNode` materializes into a leased `LayerMount` under `Compose.Mutate`; `MotionAttachment` consumes shared kernel drives, while `Glides`, `Effects`, and `WideColor` own animation, native effects, and Display-P3 projection.
- [21]-[HANDLERS](.planning/Platform/handlers.md): `PlatformSeam` owns capability demand, `Handlers` owns widget-to-handler identity and minting, `Styler` freezes style rows, and `Bridge` owns `NativeControlHost` embedding and the Eto conversion boundary.
- [22]-[NATIVE](.planning/Platform/native.md): `MacGate` and `MacAnchor` own dual admission and explicit `IMacControlHandler` view roles; `NativeSeam` owns ABI-typed monitors, gesture and pressure leases, point conversion, accessibility posture, and anchor-screen pacing observation.

[SHELL]:

- [23]-[CHROME](.planning/Shell/chrome.md): `Chrome.Apply` folds toolbar, input-panel, tooltip, and floating-button intent onto the mintable GH2 chrome hosts through one receipted gate.
- [24]-[EDITOR](.planning/Shell/editor.md): `EditorShell` projects pane slots through `ShellSlot`, mutates boolean posture through `ShellToggle`, returns `ShellState`, and owns the single `BeginRhinoGetter` handoff.
- [25]-[EVENTS](.planning/Shell/events.md): `UiSource` rows attach every admitted GH2 and Eto stream to typed `UiFact` cases; `UiEvents.Observe` transactionally mints one leased `UiSubscription` over an `EventAnchor` and stamps each `UiEvent` in sink order.
- [26]-[ICONS](.planning/Shell/icons.md): `IconOwner` admits the host icon origins, keyed poses, filters, and render modalities; `IconCatalog` freezes plugin inventory, and owned raster products compose session recency.
- [27]-[SESSION](.planning/Shell/session.md): `GhSession.Apply` closes command-shaped `SessionOp` work and acknowledgement receipts, `Run<TOut>` bounds detached projections to one marshal, and `SessionCache` keys `HybridCache` entries and document tags from stable `DocumentToken` identity.

## [02]-[DOMAIN_PACKAGES]

The host boundary compiles against assemblies projected from the installed Rhino WIP application; `.api/` catalogues own the admitted member surfaces.

[GRASSHOPPER_HOST]:

- `Grasshopper2` — component authoring, data transfer, document graph and history, solution execution, canvas behavior, native objects, and editor chrome
- `GrasshopperIO` — `IReader`/`IWriter` persistence for host documents, objects, and plug-ins

[ETO_HOST]:

- `Eto` — `Eto.Forms`, `Eto.Drawing`, binding, runtime dispatch, controls, windows, dialogs, menus, input, transfer, and screen metrics
- `Eto.macOS` — the AppKit backend, `IMacControlHandler` view roles, native hosting, and `MacConversions`/`CGConversions`

[RHINO_HOST]:

- `RhinoCommon` — Rhino document and geometry carriers plus the getter and dialog handoff payloads
- `Rhino.UI` — Rhino styling and native UI bridge used by session and presentation owners
- `System.Drawing.Common` — compile-time GDI carrier interop at the GH1 icon boundary; runtime ownership remains with the host

[PLATFORM_NATIVE]:

- `Microsoft.macOS` — AppKit, CoreAnimation, CoreGraphics, CoreImage, Foundation, and Objective-C bindings behind the gated native owners

## [03]-[SUBSTRATE_PACKAGES]

The package consumes the universal C# functional substrate and its one folder-specific recency row; shared contracts live in the C# substrate registry and `.api/` catalogues.

[FUNCTIONAL_CORE]:

- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[RECENCY_CACHE]:

- `Microsoft.Extensions.Caching.Hybrid` — tagged L1/L2 recency and stampede control for document-scoped `SessionCache` values
