# [RASM_RHINO]

`Rasm.Rhino` is the Rhino 9 host boundary for the C# strata: a planning-scoped package that captures the RhinoCommon document, command, block, viewport, display, and exchange surfaces, composes the native Eto UI framework and the `Rhino.UI` shell over it, and reaches the `Rasm` kernel for every host-neutral computation. The folder references only the kernel — never a sibling package — so a Rhino or Grasshopper2 app composes host capability as declared rows instead of re-deriving host plumbing; namespace mirrors folder path (`ARCHITECTURE.md` `[03]`). This README routes the design pages and registers the host assemblies and the one folder graph addition.

## [01]-[ROUTER]

[DOCUMENT]:
- [01]-[SESSION](.planning/Document/session.md): `DocumentSession` — document identity, admission gates, capability-scoped access, and the live model/page regime with kernel `Context` refresh.
- [02]-[GEOMETRY](.planning/Document/geometry.md): The native `GeometryBase` crossing — custody modes, transient identity, kernel-built transforms, handle-scoped bounds, and receipted clipping.
- [03]-[TABLES](.planning/Document/tables.md): `TableKind`/`TableTarget`/`TableOp` document-table mutation folded by `Tables.Commit` inside one capability window with undo sealing and redraw compensation.
- [04]-[EVENTS](.planning/Document/events.md): The `EventFamily` observation roster and `DocumentStream` transactional attach, delivery scheduling, and symmetric detach into detached facts.

[COMMANDS]:
- [05]-[COMMAND](.planning/Commands/command.md): The `Stage<TModel>` command algebra over one immutable model, the thin `Command` host adapter, and `CommandVerdict` mapped onto the native `Result`.
- [06]-[ACQUISITION](.planning/Commands/acquisition.md): The `InputKind` acquisition matrix, one parameterized `Acquire` request, and `Acquisition.Get` proving the session grant into an `AcquiredReceipt`.
- [07]-[OPTIONS](.planning/Commands/options.md): The `OptionValue`/`OptionRow` command-line vocabulary as data, with the boundary-owned `OptionLease` minting and releasing every native carrier per getter window.
- [08]-[SELECTION](.planning/Commands/selection.md): The `ObjRef` `PartKind` projection matrix onto the `Picked` union with eager `PickCapture` evidence and the frozen `Analyze` kernel re-entry.

[BLOCKS]:
- [09]-[MODEL](.planning/Blocks/model.md): The `BlockRef` address union, the whole-state `BlockSnapshot`, and the reference-scope, conflict, deletion, explode, and preview policy rows.
- [10]-[GRAPH](.planning/Blocks/graph.md): The `GraphSource` union folding the live `InstanceDefinitionTable` and offline `File3dm` archives into one QuikGraph topology answered by `BlockGraph.Ask`.
- [11]-[LIFECYCLE](.planning/Blocks/lifecycle.md): One spine for host event ingress, versioned preview leasing, deferred refresh, document eviction, and native disposable release.
- [12]-[OPERATIONS](.planning/Blocks/operations.md): The `BlockOp` mutation union committed through `Blocks.Commit` under demand-gated grants, undo bracketing, and the `BlockReceipt` fact stream.

[VIEWPORT]:
- [13]-[CAMERA](.planning/Viewport/camera.md): The `CameraPose`/`ViewportTarget`/`CameraSnapshot` altitudes composing kernel `VectorFrame`/`VectorIntent` over a session-gated `ViewportLease`.
- [14]-[OPERATIONS](.planning/Viewport/operations.md): The `CameraOp` union executed by `Cameras.Apply` over the `ViewportLease` with UI-thread gating, batched redraw, and the `CameraReceipt`.
- [15]-[CAPTURE](.planning/Viewport/capture.md): The resolved `CaptureSpec` render axes and `Captures.Run` constructing `ViewCaptureSettings` once, then dispatching the `CaptureSink` cases.
- [16]-[MOTION](.planning/Viewport/motion.md): The host motion-pacing adapter — `RedrawTarget` landing, `FrameClock` drivers, `MotionGate` accessibility, and the kernel-sampled `MotionPump`.

[DISPLAY]:
- [17]-[CONDUIT](.planning/Display/conduit.md): The `ConduitPhase` pipeline algebra deriving one `OverlayConduit`, plus display-mode, visual-analysis, and `CustomDisplay` participation.
- [18]-[DRAW](.planning/Display/draw.md): The two-backend `Mark` union dispatched by `Marks.Render` over the `Canvas` union — `DisplayPipeline` and `Eto.Drawing.Graphics` — with data-driven style.
- [19]-[INTERACTION](.planning/Display/interaction.md): Three in-viewport tiers — `ViewportPointer` hook, `GumballRig` manipulator, `WidgetHost` widgets — folded onto kernel-neutral fact streams.
- [20]-[RENDER](.planning/Display/render.md): The `RenderJob` batch session and `RealtimeEngine` interactive participant over `Rhino.Render` with explicit channel and post-effect rows.

[EXCHANGE]:
- [21]-[FORMATS](.planning/Exchange/formats.md): The `FileCodec` `[SmartEnum<string>]` interchange matrix projecting detection, dialog filters, plug-in registration, and read/write dispatch.
- [22]-[ARCHIVE](.planning/Exchange/archive.md): The `File3dm` transaction union through `Archives.Apply` over one `Lease<File3dm>` ownership seam returning detached values.
- [23]-[OPERATIONS](.planning/Exchange/operations.md): The document-bound exchange union through `Exchanges.Run` with capability proof, undo bracketing, collision policy, and `IoLane` concurrency.
- [24]-[SHEETS](.planning/Exchange/sheets.md): The sheet transaction union with `SheetSelect`/`DetailSelect` resolution and the declarative `DetailState` field-row commit correspondence.
- [25]-[PUBLISH](.planning/Exchange/publish.md): The publication pipeline deriving every target egress from one `CaptureSpec`, token and stamp vocabularies, and content-keyed artifact evidence.

[ETO]:
- [26]-[PLATFORM](.planning/Eto/platform.md): The ambient `Platform` binding as `Option`-railed feature rows, `NativeMount` host bridging, and the `ThemeSeam`/`ThemeCatalog` color grid.
- [27]-[RUNTIME](.planning/Eto/runtime.md): The ambient runtime rails — `UiThread` dispatch, the `UITimer` pulse, display and input projection, typed transfer, and system presence.
- [28]-[ELEMENTS](.planning/Eto/elements.md): The closed `Element` control tree, the `Realize` minting fold, the `Arrangement` layout algebra, and the `UiFault` failure vocabulary.
- [29]-[BINDING](.planning/Eto/binding.md): Typed `Bind.Rig` rows over the host `*Binding` surface with one kernel-composing admission gate and the `BindReceipt` lifecycle.
- [30]-[CANVAS](.planning/Eto/canvas.md): The retained `Mark` scene folded into the host `Graphics` stream, path-derived hit-testing, and the one `PerceptualColor`-to-`Color` mint.
- [31]-[CHROME](.planning/Eto/chrome.md): The `IntentRow` verb table projected into menus, toolbars, and commands, `ShellPlan` windows, `Prompt<TResult>` dialogs, and `PrintPlan` output.

[HOSTUI]:
- [32]-[SHELL](.planning/HostUi/shell.md): The host-runtime surface — `HostThread` marshalling, the status monoid, the leased progress meter, window adoption, and the theme edge.
- [33]-[PANELS](.planning/HostUi/panels.md): The `HostPanel` `IPanel` owner, registration and placement, the icon family driving both register overloads, and the `RuiOp` toolbar-file fold.
- [34]-[PAGES](.planning/HostUi/pages.md): The `PageKind` host-page discriminant, a complete `PagePlan` value, and one realization dispatch behind two sealed override leaves.
- [35]-[DIALOGS](.planning/HostUi/dialogs.md): The `Inquiry` union over the `Rhino.UI.Dialogs` roster folded by `Inquiries.Ask`, plus the `DrawingUtilities` resource and preview families.

## [02]-[DOMAIN_PACKAGES]

The Rhino 9 WIP host assemblies this boundary compiles against, resolved by `HintPath` from the loaded RhinoWIP framework, and the one graph package the block domain adds; versions centralize in the one C# manifest, corroborated by `.api/`.

[RHINO_HOST]:
- `RhinoCommon` — the host surface for document, tables, commands, input, blocks, display, render, file IO, and plug-in file-type registration
- `Rhino.UI` — native chrome: panel and page registration, the built-in dialog roster, the gumball and in-viewport widget families, status bar and RUI state, and the Eto host bridge
- `System.Drawing.Common` — the host bitmap and screen-carrier interop struct set the boundary reads and detaches before any value crosses inward

[NATIVE_UI]:
- `Eto` — the embedded cross-platform toolkit driving every native surface: `Eto.Forms` construction and `Eto.Drawing` immediate-2D painting
- `Eto.macOS` — the macOS backend handler the ambient `Platform` resolves for the loaded `Eto.dll`

[PLATFORM_NATIVE]:
- `Microsoft.macOS` — the AppKit and CoreAnimation binding behind display-link frame pacing, reduce-motion gating, and screen-parameter observation, platform-gated to the macOS target

[GRAPH_ALGORITHM]:
- `QuikGraph` — the block definition-graph engine: reachability, strongly connected components, and topological order over a transient `BidirectionalGraph`

## [03]-[SUBSTRATE_PACKAGES]

The C# substrate registry this folder consumes directly; the full registry and substrate contracts live in `libs/csharp/.planning/README.md`, with shared API evidence in `libs/csharp/.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`
