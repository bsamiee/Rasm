# [RASM_GRASSHOPPER]

`Rasm.Grasshopper` is the Grasshopper2 and Eto host boundary on the C# app strata: a planning-scoped package that captures the whole GH2 SDK and native Eto UI surface and references the `Rasm` kernel and no other sibling. Its design corpus lives under one `.planning/` root in six sub-domain folders — `Canvas` (the paint, wire, layout, motion, and interaction owners over the live canvas), `Components` (component authoring, the pin catalog, Garden data transfer, attribute chrome, and the native object catalog), `Document` (the graph transaction spine, query/wire operator, branching undo ledger, and solution controller), `Eto` (native control construction, two-way binding, the UI-thread runtime floor, and the window/dialog/menu spine), `Platform` (the Eto handler seam, the gated macOS-native AppKit touch, and the CoreAnimation compositing owner), and `Shell` (the session spine, the UI event algebra, editor-shell control, chrome intent, and the vector-icon owner). Namespace mirrors folder path: every fence under `<Folder>/` declares `namespace Rasm.Grasshopper.<Folder>;` (`ARCHITECTURE.md` `[03]`). Host-agnostic math — easing, spring, interpolation, perceptual colour blending — composes the kernel motion and colour surfaces, never a second in-folder derivation; every host member is decompile-verified against the installed Grasshopper2 and Eto assemblies. This README routes the design pages and registers every host assembly and package the folder uses.

## [01]-[ROUTER]

[CANVAS]:
- [01]-[CANVAS](.planning/Canvas/canvas.md): The canvas operator — `CanvasOperator.Apply` command gate and `.Read` projection gate over the live GH2 `Canvas`, folding viewport navigation, projection writes, marquee lifecycle, sparkle overlays, and window-select gating into one `CanvasOp` union with pick resolution, coordinate mapping, and rasterization as typed `CanvasLens` reads.
- [02]-[INTERACTION](.planning/Canvas/interaction.md): The canvas interaction spine — `ResponderSpec` declaring a hit-testable input target as data over one `Verdict` vocabulary, the focus stack, object dragging, dwell and context-menu policy, and edge resizing mounted onto the host flex dispatch through one lease-owned adapter.
- [03]-[LAYOUT](.planning/Canvas/layout.md): The layout owner — programmatic arrangement as one `Arrangement` case folded to per-object pivot deltas and sealed as one undo-recorded document mutation over the host `SnappingAction`/`SnapSpace`/`StretchLayoutSolver` surfaces, beside interactive snap solving.
- [04]-[MOTION](.planning/Canvas/motion.md): The GH2 pacing adapter — `Animated<T>` composition, the `Animators` factories, the `IFlexControl` animation drive, and the `AnimatedPath` feedback glyphs, re-paced off the kernel motion rows so a host tween and a kernel curve meet at one consumer.
- [05]-[PAINT](.planning/Canvas/paint.md): The declarative paint owner — one `Mark` intent union, one `PaintStock` executor owning every Eto pen, brush, and font lifetime against spec identity, and the `PaintPhase` row vocabulary over the eight ordered host paint events, with perceptual colour crossing one `Pigment` pair onto the kernel blend rows.
- [06]-[WIRES](.planning/Canvas/wires.md): The wire-visual owner — route geometry over the host `WireShape` family, custom-route installation through the published `RouteStyle` seam, wire picking through the public pick map, marquee wire selection through `WindowSelection`, and the `WireSkin` pen pass.

[COMPONENTS]:
- [07]-[ATTRIBUTES](.planning/Components/attributes.md): `ComponentChrome` — every host attribute callback (layout, pivot, paint, menu, pointer, keyboard, focus, tooltip, resize, cursor) as one `ChromeEvent` case answered by one `Respond` fold to a right-biased `ChromeDecision`, sealed into one `ChromeReceipt` stream over one `ChromeDispatch` spine.
- [08]-[COMPONENT](.planning/Components/component.md): `ComponentSpec` — the one component declaration carrying identity, pin roster, `Execution` input-shape discrimination, lifecycle, threading, bake, and canvas chrome on one record, with the `SpecComponent` `ModularComponent` projection and `PluginSpec` registration rows.
- [09]-[DATA](.planning/Components/data.md): `GardenData` — the GH2 `IDataAccess` transfer policy across item, pear, twig, and tree topology, the `Garden` tree algebra, the merit-scored `ConversionServer` cast-or-convert fold, and the closed `GhFault` family and `Hosted` funnel every host crossing reports through.
- [10]-[OBJECTS](.planning/Components/objects.md): `NativeObject` — the interactive special, routing (`Shout`/`Listen`/`Relay`), and composite-iterative (`Cluster`/`Chain`/`Loop`) families as `NativeKind` catalog rows with `PersistedValue` cases, plus the one explicit GH1 interop import boundary returning a typed receipt.
- [11]-[PORTS](.planning/Components/ports.md): `PortRow` — the data-driven pin catalog over the modular adders carrying carrier type, side and hidden-side capability, and both registration bindings, with pin depth, presence, and visibility as generated vocabularies and `PinTrim` property policy, folded by `Ports`.

[DOCUMENT]:
- [12]-[DOCUMENT](.planning/Document/document.md): `DocumentScope` — document minting across the inert, inactive, and active tiers, lifecycle and persistence settlement, typed keyed-value shelves, and the one `Transact` graph gate pairing every `DocumentMethods` mutation verb with its undo seal in one act.
- [13]-[GRAPH](.planning/Document/graph.md): `GraphScope` — `Ask` settling read intent over the host `ObjectList`/`Connectivity` surfaces and `Mutate` settling wire and membership mutation over `Connections` and `SplitWire`, each sealed into the undo ledger, with direction, reach, and facet as `[SmartEnum<int>]` rows.
- [14]-[HISTORY](.planning/Document/history.md): `HistoryLedger` — sealing a filled `ActionList` into the host branching `History` tree under one `VerbNoun`, tree stride, re-root, and replay, branch reconciliation, and the `Seal` spelling `Document` and `Graph` compose so mutation and undo are one act.
- [15]-[SOLUTION](.planning/Document/solution.md): `SolutionControl` — launching a run in every posture the host `SolutionServer` admits, halting, cancelling an in-flight `Solution`, driving the deferred-expiry protocol, and the `Watch`/`Trace` fold over the six-event solution lifecycle.

[ETO]:
- [16]-[BINDING](.planning/Eto/binding.md): `BindingRail` — two-way control-to-model fusion over the host `IndirectBinding`/`BindableBinding`/`DualBinding` machinery, the `ValueGate` conversion admission for value-object and smart-enum fields, `DataScope` context marshalling, and the `StoreRow` collection carriers.
- [17]-[CONTROLS](.planning/Eto/controls.md): `ControlForge.Realize` — one recursive `ControlSpec` union over the installed `Eto.Forms` construction surface, the `FieldTag`/`FieldValue`/`FieldGuard` capture vocabulary, role-row modality, and the tagged-harvest registry.
- [18]-[RUNTIME](.planning/Eto/runtime.md): The Eto runtime floor — `EtoDispatch` UI-thread marshal over `Application.Instance`, the `UiClock` tick over the kernel `Lease<T>` rail, the `Transfer` clipboard and data-object algebra, and the `Display`/`InputState`/`NoticeSurface` host-fact projections.
- [19]-[WINDOWS](.planning/Eto/windows.md): The window, dialog, menu, and command spine — the `CommandDeck` row family, the recursive `MenuNode` fold, the `WindowSpec`/`WindowChrome` family with the `WindowVerb` live-mutation gate, and the typed-result `DialogSpec<TResult>` and `PickerSpec` present gate.

[PLATFORM]:
- [20]-[COMPOSITION](.planning/Platform/composition.md): The CoreAnimation owner — the `CALayer` graph inside `CATransaction` batches, `CADisplayLink` vsync pacing against the live screen ceiling, kernel-sampled per-beat drives written to layer state, and the CoreImage filter, haptic, and vibrancy seams behind `MacGate`.
- [21]-[HANDLERS](.planning/Platform/handlers.md): The Eto platform seam — `PlatformSeam` capability demand over the active `Platform`, the `Handlers` widget-to-handler substrate, the `Styler` frozen style rows, and the `Bridge` `NativeControlHost` embedding and managed-to-AppKit contract.
- [22]-[NATIVE](.planning/Platform/native.md): `NativeSeam` — every gated AppKit touch (`NSView` extraction, `NSEvent` local monitors, gesture and pressure attachment, `NSWorkspace` observation, point conversion) demanding `MacGate` with a typed `Fault.Unsupported` off-platform branch.

[SHELL]:
- [23]-[CHROME](.planning/Shell/chrome.md): `Chrome.Apply` — toolbar, input-panel, tooltip, and floating-button demand folded onto the mintable GH2 chrome hosts (`Toolbar.Bar`, `InputPanel`, the static `Tooltip.Frame`, the flex float collection) through one gate returning typed receipts.
- [24]-[EDITOR](.planning/Shell/editor.md): `EditorShell` — the GH2 `Editor` singleton's chrome-pane slots as `ShellSlot` rows, the boolean shell toggles as `ShellToggle` rows, the projected `ShellState` receipt, and the one `BeginRhinoGetter` Rhino handoff.
- [25]-[EVENTS](.planning/Shell/events.md): The one UI event algebra — the `UiSource` source-row vocabulary over every catalog-verified GH2 and Eto event stream, the `UiFact` payload union, the `EventAnchor` union, and the `UiSubscription` owner riding the kernel `Lease<T>` rail with transactional batch attach.
- [26]-[ICONS](.planning/Shell/icons.md): The stateful vector-icon owner — the five `AbstractIcon` origins with `CodeDiagnostic` capture, the keyed-state pose machine, the `IconContext` filter chain, the render gate, and the frozen `IconCatalog`, with tint math over the kernel blend rows.
- [27]-[SESSION](.planning/Shell/session.md): `GhSession` — typed scope acquisition over the live editor, canvas, and document chain, UI-thread execution through the runtime dispatch rail, lifecycle, teardown, and repaint receipts, and `SessionCache` document-scoped recency over `HybridCache`.

## [02]-[DOMAIN_PACKAGES]

The Grasshopper2 and Eto host assemblies this boundary captures outside the C# substrate registry, plus the folder additions its consumers compose; the host assemblies resolve from the installed Grasshopper2 SDK, `Wacton.Unicolour` and `Microsoft.Extensions.Caching.Hybrid` centralize in the one C# manifest, and the folder `.api/` catalogs fix the verified member surface each concern composes.

[HOST_GRAPH]:
- `Grasshopper2` — the visual-programming host: component authoring, the document graph and undo tree, `SolutionServer` execution, and the `Editor` shell.
- `GrasshopperIO` — the `IReader`/`IWriter` persistence pair every native object and plugin serializes through.

[NATIVE_UI]:
- `Eto.Forms` — the native control, window, dialog, menu, and command framework realized into hosted panels and shell chrome; the boundary's one UI framework.
- `Eto.Drawing` — the host-neutral 2D paint surface every canvas painter, wire renderer, and icon projector draws through.
- `Eto.Platform` — the handler and style substrate resolving each widget onto its native backing, plus the `NativeControlHost` and `Eto.Mac` AppKit bridge.

[PLATFORM_NATIVE]:
- `Microsoft.macOS` — the platform-gated AppKit/CoreAnimation seam beneath GH2 canvas: `NSView` extraction, the `CALayer` graph, and `CADisplayLink` pacing.

[GEOMETRY_HOST]:
- `Rhino.Geometry` — the RhinoCommon value and geometry carrier types native pins transport, and the Rhino edit and getter handoff surface.

[RECENCY_CACHE]:
- `Microsoft.Extensions.Caching.Hybrid` — the `HybridCache` substrate `SessionCache` composes for document-tagged recency and L1/L2 policy.

## [03]-[SUBSTRATE_PACKAGES]

The C# substrate registry cards this folder consumes; the full registry and substrate contracts live in `libs/csharp/.planning/README.md`, with shared API evidence in `libs/csharp/.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[COLOR_SCIENCE]:
- `Wacton.Unicolour` — the perceptual colour-space owner the `Pigment` seam converts through onto the kernel `PerceptualBlend` rows
