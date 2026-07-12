# [RASM_RHINO]

`Rasm.Rhino` is the single RhinoCommon, Rhino UI, Eto, and macOS host boundary above the `Rasm` kernel; it references no sibling package. `DocumentSession`, `HostThread`, and `UiThread` bound host access by affinity and capability, while `Lease<T>` and the boundary capsules retain every native resource only across its valid extent.

## [01]-[ROUTER]

[DOCUMENT]:
- [01]-[SESSION](.planning/Document/session.md): `DocumentSession.Of` admits borrowed or owned document sources, `Demand` serializes fresh handle and capability evidence, and `Adjust` commits receipted model/page regime changes against a refreshed kernel `Context`.
- [02]-[GEOMETRY](.planning/Document/geometry.md): `GeometryCrossing.Cross` admits native and kernel forms into `GeometryHandle`; the handle owns custody, inspection, comparison, tags, kernel-built motion, bounds, clipping, and release evidence.
- [03]-[TABLES](.planning/Document/tables.md): `TableTarget` and `TableOp` close document-table addressing and mutation; `Tables.Commit` executes a shaped `TableTransaction` through `UndoBracket` and returns the additive `TableReceipt` fact stream.
- [04]-[EVENTS](.planning/Document/events.md): `EventFamily` rows bind host events into detached facts, while `DocumentStream.Observe` transactionally owns host or file attachment, delivery policy, bounded evidence, and symmetric release.

[COMMANDS]:
- [05]-[COMMAND](.planning/Commands/command.md): `Stage<TModel>` and `CommandFlow<TState>.Drive` own the bounded immutable command algebra; `RasmCommand<TSelf,TState>` adapts host lifecycle, and `Scripting.Run` owns sanctioned macro and registered-command execution.
- [06]-[ACQUISITION](.planning/Commands/acquisition.md): `InputKind` rows and one `Acquire` value describe every getter modality; `Acquisition.Get` owns the option loop, capability window, native getter lifetime, and typed `AcquiredReceipt` terminal.
- [07]-[OPTIONS](.planning/Commands/options.md): `OptionValue` and `OptionRow` make command-line options data; `OptionSet.Bind` mints an `OptionLease` that owns native carriers, selection decode, live snapshots, and deterministic release.
- [08]-[SELECTION](.planning/Commands/selection.md): `Picks` owns eager `ObjRef` capture, `PartKind` projection, geometry retention, policy-driven execution, and measured kernel re-entry without retaining a native reference.

[BLOCKS]:
- [09]-[MODEL](.planning/Blocks/model.md): `BlockRef` owns definition identity, `BlockSnapshot` captures whole live state, and the policy rows close reference, conflict, deletion, explode, placement, link, and preview decisions.
- [10]-[GRAPH](.planning/Blocks/graph.md): `GraphSource` folds live and archived definitions into one transient QuikGraph topology; `BlockGraph.Ask` owns topology questions, and `ArchiveClosure.Closure` owns linked-archive traversal evidence.
- [11]-[LIFECYCLE](.planning/Blocks/lifecycle.md): `BlockLifecycle` composes document observation, versioned `PreviewGrant` custody, policy-driven invalidation, linked-file refresh, document eviction, and exact native disposal on one spine.
- [12]-[OPERATIONS](.planning/Blocks/operations.md): `Blocks.Ask` closes block reads, while `Blocks.Commit` executes `BlockTransaction` programs through plan-derived grants, geometry leases, undo/redraw bracketing, and the additive `BlockReceipt` stream.

[VIEWPORT]:
- [13]-[CAMERA](.planning/Viewport/camera.md): `ViewportTarget` resolves through `ViewportLease`, `CameraPose` composes the kernel frame and intent owners, and `CameraSnapshot` projects disposable native state into detached values.
- [14]-[OPERATIONS](.planning/Viewport/operations.md): `Cameras.Apply` folds one `CameraOp` across scalar or broadcast targets under one UI-thread and redraw policy, returning `CameraReceipt` evidence without exporting a live viewport.
- [15]-[CAPTURE](.planning/Viewport/capture.md): `CapturePlan` and `CaptureRequest` own settings-driven scalar and printer delivery, `TransparentCaptureSpec` owns the facade path, and `Captures.Run` shares one reverse-released preparation rail with publication staging.
- [16]-[MOTION](.planning/Viewport/motion.md): `FrameClock` owns portable and display-linked pacing, `RedrawTarget` owns frame landing, and `MotionPump.Drive` samples kernel motion into a leased `MotionDrive` under accessibility and display facts.

[DISPLAY]:
- [17]-[CONDUIT](.planning/Display/conduit.md): `ConduitProgram` and `Conduits.Mount` own filtered phase participation and balanced render state; the same owner carries display-mode, visual-analysis, retained-overlay, pen, and iso-effect rows.
- [18]-[DRAW](.planning/Display/draw.md): `Marks.Render` dispatches one `Mark` vocabulary over pipeline and Eto `Canvas` cases; retained paths, style rows, text measurement, hit testing, and `SpriteSheet` custody remain on the same draw boundary.
- [19]-[INTERACTION](.planning/Display/interaction.md): `ViewportPointer`, `GumballRig`, and `WidgetHost` own pointer, manipulator, and in-viewport widget lifecycles while emitting detached facts and retaining no callback-scoped host handles.
- [20]-[RENDER](.planning/Display/render.md): `RenderJob.Open` owns batch pipeline and window custody, `RealtimeEngine` owns interactive framebuffer participation, and `PostEffectGate` plus `TextureBake` close post-processing and texture evaluation.

[EXCHANGE]:
- [21]-[FORMATS](.planning/Exchange/formats.md): `FileCodec` rows are the interchange authority from detection through direct engine dispatch; `Codecs` owns internal read/write execution, and `CodecPort` projects the same rows into host file-dialog registration.
- [22]-[ARCHIVE](.planning/Exchange/archive.md): `Archives.Apply` executes one `ArchiveOp` over an owned `File3dm` lease; `ArchiveProgram`, `ArchiveReceipt`, and shared `ExchangeEvidence` retain ordered steps, detached yields, exact-byte identity, and mutation residue.
- [23]-[OPERATIONS](.planning/Exchange/operations.md): `Exchanges.Run` owns document-bound `ExchangeOp` programs and complete terminal evidence; `Exchanges.Convert` alone admits `IoLane` concurrency across independently acquired headless sessions.
- [24]-[SHEETS](.planning/Exchange/sheets.md): `SheetOp` unifies page and detail selection, desired state, arrangement, scale, veils, clips, audit, and batching; `Sheets.Preview` projects plans, and `Sheets.Commit` owns undo/redraw settlement and `SheetReceipt` evidence.
- [25]-[PUBLISH](.planning/Exchange/publish.md): `PageFrame` and `PageSource` derive captured or blank pages for one `PublishTarget`; `Publishing.Run` composes capture leases and atomically lands content-keyed artifacts or typed printer evidence.

[ETO]:
- [26]-[PLATFORM](.planning/Eto/platform.md): `HostPlatform` owns ambient backend discovery, `NativeMount` and `NativeAttachment` own both native-control crossings, and `ThemeSeam` plus `ThemeCatalog` own tracked style rebroadcast and named color resolution.
- [27]-[RUNTIME](.planning/Eto/runtime.md): `UiThread` owns control-tree affinity, `Pulse` owns timer lifetime, and the runtime rows close displays, input, transfer, drag/drop, notifications, tray presence, and taskbar progress.
- [28]-[ELEMENTS](.planning/Eto/elements.md): `Element.Realize` folds the typed control tree through `ElementSpec`, family rows, grids, and arrangements; each control retains its bindings, and `UiFault` is the sub-domain failure vocabulary.
- [29]-[BINDING](.planning/Eto/binding.md): `Bind.Rig` wires typed state, context, or seed sources; the realized control owns each `BindReceipt`, while `Bind.Owned`, `Refresh`, and `Release` expose its complete lifecycle and evidence ledger.
- [30]-[CANVAS](.planning/Eto/canvas.md): `Scene` renders and hit-tests one retained `Mark` vocabulary, `Surface` owns drawable mounting and invalidation, and `PixelLease` bounds locked bitmap access under the single perceptual-color projection.
- [31]-[CHROME](.planning/Eto/chrome.md): `IntentTable` projects one verb vocabulary into menus, toolbars, and commands; `ShellPlan`, `Prompt<TResult>`, and `PrintPlan` own window, typed modal, and paginated scene lifecycles.

[HOSTUI]:
- [32]-[SHELL](.planning/HostUi/shell.md): `HostThread` owns Rhino command-thread affinity; `StatusDelta`, `PromptWatch`, `Progress.Use`, `ShellWindows`, and `ShellTheme` own application status, observation, progress, window adoption, and theme synchronization.
- [33]-[PANELS](.planning/HostUi/panels.md): `HostPanel` and `PanelHost` own panel content, lifecycle, registration, placement, visibility, icon, and subscriptions; `Rui.Apply` and `Sections.Mount` close toolbar-file and collapsible-section composition.
- [34]-[PAGES](.planning/HostUi/pages.md): `PagePlan` and `PageKind` realize one `HostPage` lifecycle behind the host base classes; `PageNav` owns stacked mutation, and `PageMount.Land` owns registration collection shape.
- [35]-[DIALOGS](.planning/HostUi/dialogs.md): `Inquiries.Ask` folds native dialog intent into typed answers under the session dialog grant, while `HostResources` and `Previews.Render` own host resource and preview projection.

## [02]-[DOMAIN_PACKAGES]

The boundary compiles against assemblies projected from the installed Rhino WIP application; `.api/` catalogues own the admitted member surfaces.

[RHINO_HOST]:
- `RhinoCommon` — document, command, input, geometry, block, viewport, display, render, archive, exchange, and plug-in file surfaces
- `Rhino.UI` — Eto host bridging, panels, pages, native dialogs, status, RUI chrome, interaction, and resource projection
- `System.Drawing.Common` — compile-time GDI carriers crossing host bitmap, icon, printer, and screen seams

[ETO_HOST]:
- `Eto` — controls, layouts, binding, drawing, runtime dispatch, transfer, windows, dialogs, menus, printing, and system presence
- `Eto.macOS` — the AppKit backend, native control hosting, and platform conversions

[PLATFORM_NATIVE]:
- `Microsoft.macOS` — AppKit, CoreAnimation, Foundation, and Objective-C bindings behind the gated motion and display owners

## [03]-[SUBSTRATE_PACKAGES]

The package consumes the universal C# functional substrate and its graph-algorithm row; shared contracts live in the C# substrate registry and `.api/` catalogues.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[GRAPH_ALGORITHM]:
- `QuikGraph` — transient dependency topology, reachability, strongly connected components, and source-first ordering for block graphs
