# [RASM_GRASSHOPPER]

`Rasm.Grasshopper` is the single Grasshopper 2 host boundary: the GH2, Eto, Rhino UI, and macOS native surface captured as typed, leased capability. Every sub-domain folds through one owner, with `GhSession`, the kernel `UiThread`, and `MacGate` bounding live host access and `Lease<T>` carrying every retained resource and its inverse lifecycle. Its bar is native-fidelity product capability: components, canvas interaction, and motion land at the grade of GH2's own built-in surfaces, vsync-paced, undo-sealed, wide-color-aware, composed as typed rails rather than raw host calls.

## [01]-[ROUTER]

[CANVAS]:
- [01]-[CANVAS](.planning/Canvas/canvas.md): `CanvasOperator.Apply` closes commands and settles result-typed canvas queries on the host surface.
- [02]-[INTERACTION](.planning/Canvas/interaction.md): `GhResponder` projects the kernel responder spec onto GH2 `Responses` under leased mounts.
- [03]-[LAYOUT](.planning/Canvas/layout.md): `CanvasLayout` folds arrangements into snap and stretch solvers, sealing move and undo as one mutation.
- [04]-[MOTION](.planning/Canvas/motion.md): `CanvasPacer` leases canvas pacing; clock and display-link drives consume one `MotionDrive.Step` fold.
- [05]-[PAINT](.planning/Canvas/paint.md): `GhMark` wraps the kernel mark band; `GhPaint` batches kernel runs through the scene's paint fences.
- [06]-[WIRES](.planning/Canvas/wires.md): `WireRoute` capsules host wire geometry; `WirePick` and `WirePass` own picking and the mark plan.

[COMPONENTS]:
- [07]-[ATTRIBUTES](.planning/Components/attributes.md): `ComponentChrome` owns the chrome policy spine over host `ResizableAttributes<T>` shells.
- [08]-[COMPONENT](.planning/Components/component.md): `ComponentSpec` owns self-typed declaration and iteration policy, sealing one run receipt.
- [09]-[DATA](.planning/Components/data.md): `GardenData` owns typed data-access transfer, tree algebra, cast-or-convert, host-tolerance projection.
- [10]-[OBJECTS](.planning/Components/objects.md): `NativeObject` owns native-object families, persisted read and assign, and GH1 import admission.
- [11]-[PORTS](.planning/Components/ports.md): `PortRow` carries the data-driven pin catalogue — carrier, semantic, axis columns — side-aware.

[DOCUMENT]:
- [12]-[DOCUMENT](.planning/Document/document.md): `DocumentScope` owns tiers, lifecycle, persistence, keyed shelves, and undo-sealed mutation gate.
- [13]-[GRAPH](.planning/Document/graph.md): `GraphScope` projects object and connectivity reads, sealing wire and membership changes into the ledger.
- [14]-[HISTORY](.planning/Document/history.md): `HistoryLedger` seals actions into the branching tree: stride, re-root, replay, autosave, reconcile.
- [15]-[SOLUTION](.planning/Document/solution.md): `SolutionControl` closes launch, halt, cancel, and deferred expiry over the leased run lifecycle.

[ETO]:
- [16]-[RUNTIME](.planning/Eto/runtime.md): `EtoTimer` leases the kernel clock's UITimer; `FrameTune` seats the pace — the one Eto concern here.

[PLATFORM]:
- [17]-[CAPTURE](.planning/Platform/capture.md): `SessionCapture` publishes ScreenCaptureKit frames into the kernel drain, proving paint claims.
- [18]-[COMPOSITION](.planning/Platform/composition.md): `PlatformRoot` mints plugin identity, the session clock, brokers, and the mount roster.
- [19]-[HANDLERS](.planning/Platform/handlers.md): `IMacViewHandler` and `IMacWindow` census registered AppKit contracts and refused members.
- [20]-[LAYERS](.planning/Platform/layers.md): `Compose` holds CoreAnimation graph custody, transaction fences, and display-link motion.
- [21]-[NATIVE](.planning/Platform/native.md): `MacGate` gates macOS AppKit touch: monitor and gesture leases, pressure restore, conversion, pacing.

[SHELL]:
- [22]-[CHROME](.planning/Shell/chrome.md): `Chrome` applies toolbar, input-panel, tooltip, and button intent onto GH2 hosts; `Mount` seats chrome.
- [23]-[EDITOR](.planning/Shell/editor.md): `EditorShell` projects chrome-pane slots, swings toggle capabilities, settles `GateReceipt<ShellFacts>`.
- [24]-[EVENTS](.planning/Shell/events.md): `GhFact` closes the folder fact band; GH2 source rows ride kernel `UiEvents` subscription and drains.
- [25]-[HOOKS](.planning/Shell/hooks.md): `GrasshopperPoint` realizes the kernel `HookRail` roster, every row naming its live fire site.
- [26]-[ICONS](.planning/Shell/icons.md): `IconOwner.Mint` materializes kernel asset origins into `IIcon` values; `IconCatalog` is the inventory.
- [27]-[JOURNAL](.planning/Shell/journal.md): `SessionJournal` folds drained facts into stamped per-document partitions with export.
- [28]-[SESSION](.planning/Shell/session.md): `GhSession` closes session work and repaint receipts over the injected session clock.
- [29]-[TELEMETRY](.planning/Shell/telemetry.md): `GhTelemetry` admits factories and projects receipts into attributed instruments.

## [02]-[DOMAIN_PACKAGES]

Host assemblies admitted here bind as `Directory.Build.props` references off the installed Rhino bundle, corroborated by this folder's `.api/`.

[MANAGED_HOST]:
- `Grasshopper2` — Component, canvas, document, and solution host surface.
- `GrasshopperIO` — `IReader`/`IWriter` host-document persistence.
- `RhinoCommon` — Rhino document and geometry carriers with the getter and dialog handoff.
- `Rhino.UI` — Rhino styling and the native UI bridge.

[NATIVE_UI]:
- `Eto` — Cross-platform UI toolkit: forms, drawing, binding, dispatch, controls, windows, and input.
- `Eto.macOS` — AppKit backing and the `IMacControlHandler` view roles.
- `Microsoft.macOS` — AppKit, CoreAnimation, CoreGraphics, CoreImage, and Foundation bindings behind the gated native owners.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry, whose charters own the full contracts; `libs/csharp/.api/` holds the shared API evidence.

[CORE_SUBSTRATE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Generator.Equals`
- `JetBrains.Annotations`
- `Riok.Mapperly` — Compile-time generation of every projection seam this folder mints.

[OBSERVABILITY]:
- `Microsoft.Extensions.Logging.Abstractions` — App-neutral logger admission for the `GhTelemetry` capsule; the metric surface ships BCL in-box.
- `Microsoft.Extensions.Compliance.Abstractions` — `DataClassificationAttribute` grammar the kernel `Sensitivity` rows attach through.

[DEPENDENCY_FLOORS]:
- `System.Drawing.Common` — Compile-time GDI carrier interop at the GH1 icon boundary.
