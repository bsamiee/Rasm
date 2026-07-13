# [RASM_RHINO]

`Rasm.Rhino` owns the single host boundary over RhinoCommon, Rhino UI, Eto, and the macOS native surface above the `Rasm` kernel, folding every host access through thread-affinity and capability gates that retain each native resource only across its leased extent. It references no sibling package — every alignment travels down the one kernel seam.

## [01]-[ROUTER]

[DOCUMENT]:
- [01]-[SESSION](.planning/Document/session.md): `DocumentSession.Of` admits borrowed or owned sources behind fresh handle and capability evidence.
- [02]-[GEOMETRY](.planning/Document/geometry.md): `GeometryHandle` owns geometry-custody crossing — inspection, motion, bounds, clipping, release.
- [03]-[TABLES](.planning/Document/tables.md): `Tables.Commit` executes a shaped table transaction through the undo bracket and redraw compensation.
- [04]-[EVENTS](.planning/Document/events.md): `DocumentStream.Observe` binds events into detached facts under scoped attach and symmetric release.

[COMMANDS]:
- [05]-[COMMAND](.planning/Commands/command.md): `CommandFlow<TState>.Drive` owns the bounded immutable command algebra behind the host lifecycle.
- [06]-[ACQUISITION](.planning/Commands/acquisition.md): `Acquisition.Get` owns each getter modality, native getter lifetime, and typed receipt.
- [07]-[OPTIONS](.planning/Commands/options.md): `OptionSet.Bind` makes command-line options data, leasing native carriers under scoped release.
- [08]-[SELECTION](.planning/Commands/selection.md): `Picks` owns eager picked-reference capture, geometry retention, and measured kernel re-entry.

[BLOCKS]:
- [09]-[MODEL](.planning/Blocks/model.md): `BlockRef` owns instance-definition identity and the whole-state snapshot policy rows.
- [10]-[GRAPH](.planning/Blocks/graph.md): `BlockGraph.Ask` folds live and archived definitions into one transient topology and its closure evidence.
- [11]-[LIFECYCLE](.planning/Blocks/lifecycle.md): `BlockLifecycle` composes ingress, preview vault, deferred refresh, eviction, and native disposal.
- [12]-[OPERATIONS](.planning/Blocks/operations.md): `Blocks.Commit` runs read and transaction rails through plan grants and additive receipts.

[VIEWPORT]:
- [13]-[CAMERA](.planning/Viewport/camera.md): `CameraPose` composes the kernel vector frame and intent owners behind the viewport lease.
- [14]-[OPERATIONS](.planning/Viewport/operations.md): `Cameras.Apply` folds camera operations across scalar or broadcast under one UI-thread policy.
- [15]-[CAPTURE](.planning/Viewport/capture.md): `Captures.Run` owns settings-driven scalar and printer delivery over one reverse-released prep rail.
- [16]-[MOTION](.planning/Viewport/motion.md): `MotionPump.Drive` samples kernel motion into a leased drive under accessibility and display facts.

[DISPLAY]:
- [17]-[CONDUIT](.planning/Display/conduit.md): `Conduits.Mount` owns filtered display-pipeline participation and balanced render state.
- [18]-[DRAW](.planning/Display/draw.md): `Marks.Render` dispatches one mark vocabulary over the pipeline and Eto canvas backends.
- [19]-[INTERACTION](.planning/Display/interaction.md): `WidgetHost` owns pointer, gumball, and widget lifecycles while emitting detached facts.
- [20]-[RENDER](.planning/Display/render.md): `RenderJob.Open` owns batch pipeline and window custody beside the realtime framebuffer participant.

[EXCHANGE]:
- [21]-[FORMATS](.planning/Exchange/formats.md): `FileCodec` is interchange authority from detection through engine dispatch and dialog registration.
- [22]-[ARCHIVE](.planning/Exchange/archive.md): `Archives.Apply` runs archive programs over one owned lease with exact-byte identity evidence.
- [23]-[OPERATIONS](.planning/Exchange/operations.md): `Exchanges.Run` owns document-bound programs, convert alone admitting headless concurrency.
- [24]-[SHEETS](.planning/Exchange/sheets.md): `Sheets.Commit` unifies page and detail selection, arrangement, and undo/redraw settlement in one op.
- [25]-[PUBLISH](.planning/Exchange/publish.md): `Publishing.Run` lands captured or blank pages as content-keyed artifacts or typed printer evidence.

[ETO]:
- [26]-[PLATFORM](.planning/Eto/platform.md): `HostPlatform` owns ambient backend discovery, native-control crossings, and tracked theme rebroadcast.
- [27]-[RUNTIME](.planning/Eto/runtime.md): `UiThread` owns control-tree affinity over displays, input, transfer, drag/drop, and system presence.
- [28]-[ELEMENTS](.planning/Eto/elements.md): `Element.Realize` folds the typed control tree through specs, family rows, grids, and arrangements.
- [29]-[BINDING](.planning/Eto/binding.md): `Bind.Rig` wires typed state, context, or seed sources into a realized control's receipt ledger.
- [30]-[CANVAS](.planning/Eto/canvas.md): `Scene` renders and hit-tests one retained mark vocabulary under the perceptual-color projection.
- [31]-[CHROME](.planning/Eto/chrome.md): `IntentTable` projects one verb vocabulary into menus, toolbars, windows, and typed modals.

[HOSTUI]:
- [32]-[SHELL](.planning/HostUi/shell.md): `HostThread` owns Rhino command-thread affinity, application status, progress, and window adoption.
- [33]-[PANELS](.planning/HostUi/panels.md): `PanelHost` owns panel content, lifecycle, registration, placement, and toolbar-file composition.
- [34]-[PAGES](.planning/HostUi/pages.md): `HostPage` owns page realization and kind-safe mounting behind the host base classes.
- [35]-[DIALOGS](.planning/HostUi/dialogs.md): `Inquiries.Ask` folds native dialog intent into typed answers under the session dialog grant.

## [02]-[DOMAIN_PACKAGES]

Host assemblies admitted by this folder; versions centralize in the C# manifest and corroborate against this folder's `.api/`.

[RHINO_HOST]:
- `RhinoCommon` — core Rhino host object model behind every document, command, geometry, and exchange surface.
- `Rhino.UI` — Rhino shell bridge for Eto hosting, panels, pages, dialogs, and RUI chrome.
- `System.Drawing.Common` — compile-time GDI carriers crossing host bitmap, icon, printer, and screen seams.

[NATIVE_UI]:
- `Eto` — cross-platform control, layout, binding, drawing, and window framework.
- `Eto.macOS` — AppKit backend, native control hosting, and platform conversions.
- `Microsoft.macOS` — AppKit, CoreAnimation, and Foundation bindings behind the gated motion and display owners.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry; the registry and its charters own the full contracts, and `libs/csharp/.api/` holds the shared API evidence.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[GRAPH_ALGORITHM]:
- `QuikGraph` — transient block-graph topology, reachability, and source-first ordering.
