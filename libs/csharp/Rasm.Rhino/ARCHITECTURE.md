# [RASM_RHINO_ARCHITECTURE]

The domain map of `Rasm.Rhino` — the Rhino 9 host boundary that captures the RhinoCommon document, command, block, viewport, display, and exchange surfaces, owns the native Eto UI sub-domain and the `Rhino.UI` shell over it, and composes the `Rasm` kernel for every host-neutral computation. The folder references only the kernel; kernel composition is a dependency fact, not a seam. Namespace mirrors folder path (`[03]`).

Each codemap node is the eventual source file its `.planning/` design page becomes, named in PascalCase folder and file casing. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Rhino/
├── Document/            # The host-document substrate every host surface sits on
│   ├── Session.cs       # DocumentSession identity, admission gates, capability-scoped access, and the live model/page regime
│   ├── Geometry.cs      # Native GeometryBase crossing: custody modes, transient identity, kernel transforms, handle-scoped bounds
│   ├── Tables.cs        # TableKind/TableTarget/TableOp mutation folded by Tables.Commit with undo sealing and redraw compensation
│   └── Events.cs        # EventFamily observation roster and DocumentStream transactional attach, delivery scheduling, and detach
├── Commands/            # Native command lifecycle, input acquisition, and picked-reference projection
│   ├── Command.cs       # Stage<TModel> command algebra over one immutable model, the Command host adapter, CommandVerdict onto Result
│   ├── Acquisition.cs   # InputKind acquisition matrix, one parameterized Acquire request, Acquisition.Get into AcquiredReceipt
│   ├── Options.cs       # OptionValue/OptionRow command-line vocabulary as data, OptionLease minting native carriers per getter window
│   └── Selection.cs     # ObjRef PartKind projection matrix onto the Picked union, eager PickCapture evidence, frozen Analyze re-entry
├── Blocks/              # The instance-definition domain over the kernel
│   ├── Model.cs         # BlockRef address union, whole-state BlockSnapshot, reference/conflict/deletion/explode/preview policy rows
│   ├── Graph.cs         # GraphSource union folding InstanceDefinitionTable and File3dm archives into one QuikGraph topology
│   ├── Lifecycle.cs     # One spine: host event ingress, versioned preview leasing, deferred refresh, eviction, disposable release
│   └── Operations.cs    # BlockOp mutation union through Blocks.Commit under demand-gated grants, undo bracketing, the BlockReceipt stream
├── Viewport/            # The camera model, operation rail, capture spec, and motion pacing
│   ├── Camera.cs        # CameraPose/ViewportTarget/CameraSnapshot altitudes over kernel VectorFrame/VectorIntent behind a ViewportLease
│   ├── Operations.cs    # CameraOp union executed by Cameras.Apply over the ViewportLease with UI-thread gating and the CameraReceipt
│   ├── Capture.cs       # Resolved CaptureSpec render axes, Captures.Run building ViewCaptureSettings once, dispatching CaptureSink cases
│   └── Motion.cs        # Host motion-pacing adapter: RedrawTarget landing, FrameClock drivers, MotionGate accessibility, MotionPump
├── Display/             # The display-pipeline participation and renderer boundary
│   ├── Conduit.cs       # ConduitPhase pipeline algebra deriving one OverlayConduit, plus display-mode and visual-analysis participation
│   ├── Draw.cs          # Two-backend Mark union dispatched by Marks.Render over the Canvas union (pipeline vs Eto Graphics)
│   ├── Interaction.cs   # ViewportPointer hook, GumballRig manipulator, and WidgetHost widgets folded onto kernel-neutral fact streams
│   └── Render.cs        # RenderJob batch session and RealtimeEngine participant over Rhino.Render with explicit channel rows
├── Exchange/            # The document interchange and publication surface
│   ├── Formats.cs       # FileCodec [SmartEnum<string>] matrix projecting detection, dialog filters, plug-in registration, dispatch
│   ├── Archive.cs       # File3dm transaction union through Archives.Apply over one Lease<File3dm> ownership seam, detached values
│   ├── Operations.cs    # Document-bound exchange union through Exchanges.Run: capability proof, undo bracketing, IoLane concurrency
│   ├── Sheets.cs        # Sheet transaction union with SheetSelect/DetailSelect resolution and the declarative DetailState commit
│   └── Publish.cs       # Publication pipeline deriving every target egress from one CaptureSpec, token/stamp vocabularies, artifacts
├── Eto/                 # The native Eto UI framework sub-domain
│   ├── Platform.cs      # Ambient Platform binding as Option-railed feature rows, NativeMount host bridging, ThemeSeam/ThemeCatalog grid
│   ├── Runtime.cs       # Ambient runtime rails: UiThread dispatch, UITimer pulse, display/input projection, transfer, system presence
│   ├── Elements.cs      # Closed Element control tree, the Realize minting fold, the Arrangement layout algebra, the UiFault vocabulary
│   ├── Binding.cs       # Typed Bind.Rig rows over the host *Binding surface, one kernel-composing admission gate, BindReceipt lifecycle
│   ├── Canvas.cs        # Retained Mark scene folded into the host Graphics stream, path hit-testing, one PerceptualColor-to-Color mint
│   └── Chrome.cs        # IntentRow verb table projected into menus/toolbars/commands, ShellPlan windows, Prompt<TResult> dialogs
└── HostUi/              # The Rhino.UI shell composed over the Eto sub-domain
    ├── Shell.cs         # Host-runtime surface: HostThread marshalling, the status monoid, the leased progress meter, window adoption
    ├── Panels.cs        # HostPanel IPanel owner, registration/placement/visibility, the icon family, and the RuiOp toolbar-file fold
    ├── Pages.cs         # PageKind host-page discriminant, a complete PagePlan value, one realization dispatch behind two sealed leaves
    └── Dialogs.cs       # Inquiry union over the Rhino.UI.Dialogs roster folded by Inquiries.Ask, plus DrawingUtilities resources
```

## [02]-[SEAMS]

```text seams
Viewport/Camera.cs     ←  csharp:Rasm/Processing  # [BOUNDARY]: VectorIntent + VectorFrame + MotionInterpolation frozen-name contract
Commands/Selection.cs  ←  csharp:Rasm/Analysis    # [BOUNDARY]: Analyze/AnalysisQuery/Env frozen-name contract
```

## [03]-[NAMESPACES]

Namespace mirrors folder path — `.editorconfig` `dotnet_style_namespace_match_folder = true:error`: every fence under `Rasm.Rhino/<Folder>/` declares `namespace Rasm.Rhino.<Folder>;`, giving the eight roots `Rasm.Rhino.Blocks`, `Rasm.Rhino.Commands`, `Rasm.Rhino.Display`, `Rasm.Rhino.Document`, `Rasm.Rhino.Eto`, `Rasm.Rhino.Exchange`, `Rasm.Rhino.HostUi`, `Rasm.Rhino.Viewport`.

The boundary compiles as ONE assembly — the single `Rasm.Rhino.csproj` — so internal members cross the eight namespaces with no build edge, and the project references only `Rasm.csproj`. Kernel-neutral values (`VectorFrame`, `VectorIntent`, `PerceptualColor`, `Context`, `ContentHash`, `Fin`) compose from the kernel; a live host handle, a native carrier, or a `System.Drawing` screen struct never crosses out of the sub-domain that leases it.
