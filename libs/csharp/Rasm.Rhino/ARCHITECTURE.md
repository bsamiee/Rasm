# [RASM_RHINO_ARCHITECTURE]

The domain map of `Rasm.Rhino` — the Rhino 9 host boundary that captures the RhinoCommon document, command, block, viewport, display, and exchange surfaces, owns the native Eto UI sub-domain and the `Rhino.UI` shell over it, and composes the `Rasm` kernel for every host-neutral computation. The folder references only the kernel; the seam map names only cross-owner contracts that constrain the host boundary. Namespace mirrors folder path (`[03]`).

Each codemap node is the eventual source file its `.planning/` design page becomes, named in PascalCase folder and file casing. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Rhino/
├── Document/            # The host-document substrate every host surface sits on
│   ├── Session.cs       # DocKey/SessionSource acquisition, capability-scoped DocumentSession demands, ModelUnit-backed UnitRegime adjustment
│   ├── Geometry.cs      # Native GeometryBase crossing: custody modes, transient identity, kernel transforms, handle-scoped bounds
│   ├── Tables.cs        # TableOp/TableTransaction mutation through Tables.Commit; the shared UndoBracket, redraw compensation, fact receipts
│   └── Events.cs        # EventFamily observation roster and DocumentStream transactional attach, delivery scheduling, and detach
├── Commands/            # Native command lifecycle, input acquisition, and picked-reference projection
│   ├── Command.cs       # Stage<TModel> command algebra over one immutable model, the Command host adapter, CommandVerdict onto Result
│   ├── Acquisition.cs   # InputKind acquisition matrix, one parameterized Acquire request, Acquisition.Get into AcquiredReceipt
│   ├── Options.cs       # OptionValue/OptionRow command-line vocabulary as data, OptionLease minting native carriers per getter window
│   └── Selection.cs     # ObjRef PartKind projection matrix onto the Picked union, eager PickCapture evidence, frozen Analyze re-entry
├── Blocks/              # The instance-definition domain over the kernel
│   ├── Model.cs         # BlockRef address union, whole-state BlockSnapshot, reference/conflict/deletion/explode/preview policy rows
│   ├── Graph.cs         # Live/loaded/stored GraphSource topology, BlockGraphAsk queries, and linked ArchiveClosure unit/path evidence
│   ├── Lifecycle.cs     # DocumentStream ingress, versioned PreviewGrant vault, deferred refresh, linked-file watch, eviction, release
│   └── Operations.cs    # BlockOp/BlockAsk through Commit/Ask, GeometryIntake custody, UndoBracket settlement, BlockReceipt facts
├── Viewport/            # The camera model, operation rail, capture spec, and motion pacing
│   ├── Camera.cs        # CameraPose/ViewportTarget/CameraSnapshot altitudes over kernel VectorFrame/VectorIntent behind a ViewportLease
│   ├── Operations.cs    # CameraOp union executed by Cameras.Apply over the ViewportLease with UI-thread gating and the CameraReceipt
│   ├── Capture.cs       # Sink-free CapturePlan, cardinality-admitted CaptureRequest/CaptureSink delivery, facade capture, leased preparation
│   └── Motion.cs        # Host motion-pacing adapter: RedrawTarget landing, FrameClock drivers, MotionGate accessibility, MotionPump
├── Display/             # The display-pipeline participation and renderer boundary
│   ├── Conduit.cs       # ConduitPhase pipeline algebra deriving one OverlayConduit, plus display-mode and visual-analysis participation
│   ├── Draw.cs          # Two-backend Mark union dispatched by Marks.Render over the Canvas union (pipeline vs Eto Graphics)
│   ├── Interaction.cs   # ViewportPointer hook, GumballRig manipulator, and WidgetHost widgets folded onto kernel-neutral fact streams
│   └── Render.cs        # RenderJob batch session and RealtimeEngine participant over Rhino.Render with explicit channel rows
├── Exchange/            # The document interchange and publication surface
│   ├── Formats.cs       # FileCodec [SmartEnum<string>] matrix projecting detection, dialog filters, plug-in registration, dispatch
│   ├── Archive.cs       # Standalone ArchiveOp programs through Archives.Apply, one File3dm lease, detached graph/evidence/content-key receipts
│   ├── Operations.cs    # ExchangeOp through Run, ordered program evidence, UndoBracket truth; Convert alone fans independent sessions
│   ├── Sheets.cs        # SheetOp plans through Commit/Preview, live selectors, declarative DetailState, scale/veil/clip/group evidence
│   └── Publish.cs       # PageFrame/PageSource target dispatch through capture, atomic content-keyed file landing, typed page evidence
├── Eto/                 # The native Eto UI framework sub-domain
│   ├── Platform.cs      # Ambient Platform binding as Option-railed feature rows, NativeMount host bridging, ThemeSeam/ThemeCatalog grid
│   ├── Runtime.cs       # Ambient runtime rails: UiThread dispatch, UITimer pulse, display/input projection, transfer, system presence
│   ├── Elements.cs      # Closed Element control tree, the Realize minting fold, the Arrangement layout algebra, the UiFault vocabulary
│   ├── Binding.cs       # StateCell/BindSource through Bind.Rig attachments, control-owned BindReceipt ledger, refresh/release
│   ├── Canvas.cs        # Retained Mark scene folded into the host Graphics stream, path hit-testing, one PerceptualColor-to-Color mint
│   └── Chrome.cs        # IntentRow verb table projected into menus/toolbars/commands, ShellPlan windows, Prompt<TResult> dialogs
└── HostUi/              # The Rhino.UI shell composed over the Eto sub-domain
    ├── Shell.cs         # HostThread/OnSession, status and prompt facts, progress lease, window adoption/discovery, host-theme edge
    ├── Panels.cs        # HostPanel fact stream, PanelHost placement, RuiOp state fold, menu links, collapsible SectionSpec mounting
    ├── Pages.cs         # PageKind/PagePlan realization, PageSignal spine, PageNav fold, kind-safe PageBasket mounting and reveal
    └── Dialogs.cs       # Capability-gated Inquiry rail, HostResources loaders, and PreviewOp image/stroke projection
```

## [02]-[SEAMS]

```text seams
Document/Session.cs     ←  Document/Tables.cs        # [TRANSACTION]: regime adjustment composes the receipt-generic UndoBracket
Document/Tables.cs      ←  Document/Session.cs       # [LIFETIME]: every table transaction executes inside one fresh capability demand
Blocks/Graph.cs         ←  Exchange/Archive.cs       # [SHAPE]: loaded File3dm custody and stored archive evidence feed offline topology
Blocks/Lifecycle.cs     ←  Document/Events.cs        # [WIRE]: definition/document facts drive deferred invalidation and teardown
Blocks/Lifecycle.cs     ←  Blocks/Operations.cs      # [PROJECTION]: preview and linked refresh requests ride the BlockAsk/BlockOp rail
Blocks/Operations.cs    ←  Document/Session.cs       # [LIFETIME]: one plan-derived demand retains every live definition handle
Blocks/Operations.cs    ←  Document/Tables.cs        # [TRANSACTION]: Blocks.Commit seals through the shared UndoBracket
Viewport/Capture.cs     ←  HostUi/Shell.cs           # [LIFETIME]: every request resolves and delivers inside HostThread.OnSession
Exchange/Operations.cs  ←  Document/Session.cs       # [LIFETIME]: Run serializes one document; Convert owns each headless session
Exchange/Operations.cs  ←  Document/Tables.cs        # [TRANSACTION]: each attempted live mutation composes the shared UndoBracket
Exchange/Sheets.cs      ←  Document/Session.cs       # [LIFETIME]: selectors and desired-state writers resolve inside one demand
Exchange/Sheets.cs      ←  Document/Tables.cs        # [TRANSACTION]: Sheets.Commit seals every mutating plan through UndoBracket
Exchange/Publish.cs     ←  Exchange/Sheets.cs        # [PROJECTION]: sheet/detail resolution supplies the ordered publication pages
Exchange/Publish.cs     ←  Viewport/Capture.cs       # [DELIVERY]: PageFrame projects plans, requests, sinks, and leased capture staging
Eto/Elements.cs         ←  Eto/Binding.cs            # [LIFETIME]: ElementSpec carries BindAttachment custody into realized controls
HostUi/Shell.cs         ←  Document/Session.cs       # [LIFETIME]: OnSession is the sole UI-result crossing over document demand
HostUi/Shell.cs         ←  Eto/Platform.cs           # [THEME]: ShellTheme projects the Rhino edge through ThemeSeam
HostUi/Panels.cs        ←  Eto/Elements.cs           # [PROJECTION]: HostPanel and SectionSpec realize one native-styled element tree
HostUi/Panels.cs        ←  Document/Events.cs        # [WIRE]: registry-wide panel observation composes DocumentStream
HostUi/Panels.cs        ←  HostUi/Shell.cs           # [LIFETIME]: document-scoped panel operations ride HostThread.OnSession
HostUi/Pages.cs         ←  Eto/Elements.cs           # [PROJECTION]: PagePlan content realizes once behind the host leaf
HostUi/Dialogs.cs       ←  HostUi/Shell.cs           # [LIFETIME]: inquiries and previews cross one capability-gated host marshal
Document/Session.cs     ←  csharp:Rasm/Domain        # [REGIME]: ModelUnit and Context are canonical unit/tolerance evidence
Viewport/Camera.cs      ←  csharp:Rasm/Processing    # [BOUNDARY]: VectorIntent + VectorFrame + MotionInterpolation frozen-name contract
Commands/Selection.cs   ←  csharp:Rasm/Analysis      # [BOUNDARY]: Analyze/AnalysisQuery/Env frozen-name contract
Viewport/Motion.cs      ←  csharp:Rasm/Parametric    # [BOUNDARY]: easing, cycle, spring, and branded monotonic timing
Eto/Canvas.cs           ←  csharp:Rasm/Numerics      # [BOUNDARY]: PerceptualColor and BlendPath projected once to Eto color
```

## [03]-[NAMESPACES]

Namespace mirrors folder path — `.editorconfig` `dotnet_style_namespace_match_folder = true:error`: every fence under `Rasm.Rhino/<Folder>/` declares `namespace Rasm.Rhino.<Folder>;`, giving the eight roots `Rasm.Rhino.Blocks`, `Rasm.Rhino.Commands`, `Rasm.Rhino.Display`, `Rasm.Rhino.Document`, `Rasm.Rhino.Eto`, `Rasm.Rhino.Exchange`, `Rasm.Rhino.HostUi`, `Rasm.Rhino.Viewport`.

The boundary compiles as ONE assembly — the single `Rasm.Rhino.csproj` — so internal members cross the eight namespaces with no build edge, and the project references only `Rasm.csproj`. Kernel-neutral values (`VectorFrame`, `VectorIntent`, `PerceptualColor`, `Context`, `ContentHash`, `Fin`) compose from the kernel; a live host handle, a native carrier, or a `System.Drawing` screen struct never crosses out of the sub-domain that leases it.

Host-name resolution is one law: inside `Rasm.Rhino.*` the first identifier of a partial qualification re-resolves against the boundary's own namespaces (`Rhino.UI.X` binds `Rasm.Rhino` through the member `Rhino` of `Rasm`, `Eto.Forms.X` binds `Rasm.Rhino.Eto`, `Display.X` binds `Rasm.Rhino.Display`), so fences name host members BARE through the project-level global usings; a host type no global using reaches spells `global::` in full (`global::Rhino.UI.MouseCallback`), and a simple-name collision between two host namespaces resolves through one project-level `<Using Include="..." Alias="..." />` row in `Rasm.Rhino.csproj` — never a per-fence `using` alias, never a partial qualification.
