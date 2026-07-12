# [RASM_GRASSHOPPER_ARCHITECTURE]

The domain map of `Rasm.Grasshopper` — the Grasshopper2 and Eto host boundary on the C# app strata, a planning-scoped package whose whole design corpus lives under one `.planning/` root in six sub-domain folders. It references the `Rasm` kernel and no other sibling; host-agnostic math composes the kernel motion and colour surfaces rather than a second in-folder derivation. Namespace mirrors folder path (`[03]`).

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Grasshopper/
├── Canvas/                 # Paint, wire, layout, motion, and interaction owners over the live GH2 canvas
│   ├── Canvas.cs           # CanvasOp commands plus closed CanvasQuery/CanvasProjection reads, typed picks, monotonic receipts, owned rasters
│   ├── Interaction.cs      # ResponderSpec dispatch, InteractionMount registration/focus/menu leases, DragSession and EdgeResize capsules
│   ├── Layout.cs           # Arrangement folded to pivot deltas sealed as one undo mutation; host snapping/stretch solvers; snap solving
│   ├── Motion.cs           # Span/Pace rows, Tween/FlexDrive, animated glyphs, and a CanvasPacer lease sharing MotionDrive.Step
│   ├── Paint.cs            # PaintAnchor event leases, scoped PaintScene, PaintFrame/Mark plans, PaintStock custody, culling, Pigment egress
│   └── Wires.cs            # Wire route geometry over host WireShape, RouteStyle custom routes, wire pick, marquee select, WireSkin pen pass
├── Components/             # Component authoring, pin catalog, Garden data transfer, attribute chrome, native object catalog
│   ├── Attributes.cs       # ChromeEvent/ChromeDecision policy, bounded ChromeTrace, and one ChromeHost/ResizableChromeHost projection spine
│   ├── Component.cs        # Self-typed static declaration, constructor-safe SpecComponent<TSelf>, topology/iteration policies, run ledger
│   ├── Data.cs             # GardenData: IDataAccess transfer, Garden tree algebra, ConversionServer cast-or-convert, the closed GhFault family
│   ├── Objects.cs          # NativeKind public factories, PersistedValue read/assign, timer targets, cluster maps, and the GH1 host boundary
│   └── Ports.cs            # PortRow carrier/semantic/axis catalog, side-aware PortBinding, PinPlan/PinTrim admission, and declaration fold
├── Document/               # The graph transaction spine, query/wire operator, undo ledger, solution controller
│   ├── Document.cs         # DocumentScope: minting across inert/inactive/active tiers, keyed-value shelves, the one Transact graph gate
│   ├── Graph.cs            # GraphScope: Ask over ObjectList/Connectivity, Mutate over Connections/SplitWire, each sealed into the ledger
│   ├── History.cs          # HistoryLedger: ActionList sealed into the branching History tree, stride/re-root/replay, the shared Seal spelling
│   └── Solution.cs         # SolutionControl: SolutionServer launch/halt/cancel, deferred-expiry protocol, Watch/Trace lifecycle fold
├── Eto/                    # Native control construction, two-way binding, the UI-thread runtime floor, window/dialog/menu spine
│   ├── Binding.cs          # BindingRail: IndirectBinding/BindableBinding/DualBinding fusion, ValueGate admission, DataScope, StoreRow carriers
│   ├── Controls.cs         # ControlForge.Realize: recursive ControlSpec over the Eto.Forms surface, FieldTag/FieldValue capture, role rows
│   ├── Runtime.cs          # EtoDispatch lanes/echoes, leased UiClock, Transfer algebra, Display/InputState facts, notice and tray custody
│   └── Windows.cs          # CommandDeck, recursive MenuNode fold, WindowSpec/WindowChrome + WindowVerb, DialogSpec<TResult>/PickerSpec gate
├── Platform/               # The Eto handler seam, the gated macOS-native AppKit touch, the CoreAnimation compositing owner
│   ├── Composition.cs      # LayerMount custody, Compose transactions, MotionDrive/Attachment, glides, effects, and Display-P3 WideColor
│   ├── Handlers.cs         # PlatformSeam capability demand, Handlers widget-to-handler substrate, Styler frozen style rows, Bridge embedding
│   └── Native.cs           # MacGate/MacAnchor roles, ABI-typed monitor and gesture leases, pressure restore, WorkspaceWatch screen pacing
└── Shell/                  # The session spine, UI event algebra, editor-shell control, chrome intent, vector-icon owner
    ├── Chrome.cs           # Chrome.Apply: toolbar, input-panel, tooltip, floating-button demand onto the mintable GH2 chrome hosts, one gate
    ├── Editor.cs           # EditorShell: Editor singleton chrome-pane ShellSlot rows, ShellToggle rows, ShellState receipt, BeginRhinoGetter
    ├── Events.cs           # UiFact/UiEvent evidence, EventAnchor/UiSource rows, and transactional UiSubscription ownership on Lease<T>
    ├── Icons.cs            # Vector-icon owner: five AbstractIcon origins, keyed-state pose machine, IconContext filter chain, IconCatalog
    └── Session.cs          # ScopeTarget/GhScope acquisition, Apply/Run gates, repaint and monotonic receipts, DocumentToken/SessionCache
```

## [02]-[SEAMS]

```text seams
Canvas/Wires.cs           ←  Canvas/Canvas.cs             # [PROJECTION]: typed point picks through CanvasQuery/CanvasProjection
Canvas/Interaction.cs     ←  Canvas/Canvas.cs             # [PROJECTION]: dwell and gesture reads through detached canvas state
Canvas/Interaction.cs     ←  Shell/Session.cs             # [LIFETIME]: GhSession resolves the live canvas before an owned mount or capsule escapes
Canvas/Motion.cs          ←  Eto/Runtime.cs               # [LIFETIME]: CanvasPacer owns one UiClock lease through terminal stop
Canvas/Motion.cs          ←  Platform/Composition.cs      # [SHAPE]: one MotionDrive.Step/DriveFrame sampling contract
Canvas/Paint.cs           ←  Shell/Session.cs             # [LIFETIME]: paint hooks acquire the live canvas inside GhSession
Canvas/Wires.cs           ←  Canvas/Paint.cs              # [LIFETIME]: wire rendering consumes event-scoped PaintScene only
Components/Component.cs   ←  Components/Attributes.cs     # [POLICY]: ComponentSpec.Chrome projects through one attribute spine
Components/Component.cs   ←  Components/Ports.cs          # [DECLARATION]: PinPlan rows drive constructor-time and maintenance ports
Components/Objects.cs     ←  Components/Data.cs           # [RAIL]: NativeObject host calls cross Hosted/GhFault
Shell/Events.cs           ←  Eto/Runtime.cs               # [WIRE]: ClockBeat and dispatch evidence enter UiFact rows
Shell/Events.cs           ←  Shell/Session.cs             # [IDENTITY]: document facts carry DocumentToken, never a live Document
Shell/Session.cs          ←  Eto/Runtime.cs               # [BOUNDARY]: Apply/Run settle through EtoDispatch lanes
Platform/Native.cs        ←  Eto/Runtime.cs               # [LIFETIME]: native attachment and teardown marshal through EtoDispatch
Platform/Composition.cs   ←  Platform/Native.cs           # [LIFETIME]: MacAnchor and WorkspaceWatch bound layer and display-link custody
Canvas/Canvas.cs          ←  csharp:Rasm/Parametric       # [RECEIPT]: monotonic command settlement
Canvas/Canvas.cs          ←  csharp:Rasm/Numerics         # [EVIDENCE]: finite unit-interval canvas state
Canvas/Motion.cs          ←  csharp:Rasm/Parametric       # [BOUNDARY]: easing, cycle, spring, and branded monotonic timing
Canvas/Motion.cs          ←  csharp:Rasm/Numerics         # [BOUNDARY]: perceptual interpolation and unit-interval admission
Canvas/Paint.cs           ←  csharp:Rasm/Parametric       # [RECEIPT]: monotonic paint execution settlement
Canvas/Paint.cs           ←  csharp:Rasm/Numerics         # [BOUNDARY]: perceptual colour and mapped sRGB Eto egress
Eto/Runtime.cs            ←  csharp:Rasm/Parametric       # [BOUNDARY]: MonotonicTimeline stamps and beats
Shell/Events.cs           ←  csharp:Rasm/Parametric       # [EVIDENCE]: ClockBeat carries MonotonicBeat identity
Shell/Session.cs          ←  csharp:Rasm/Parametric       # [RECEIPT]: monotonic command acknowledgement
Shell/Icons.cs            ←  csharp:Rasm/Numerics         # [BOUNDARY]: perceptual icon tint and BlendPath policy
Platform/Composition.cs   ←  csharp:Rasm/Parametric       # [BOUNDARY]: sampled drive state and monotonic beats
Platform/Composition.cs   ←  csharp:Rasm/Numerics         # [BOUNDARY]: perceptual drives and mapped Display-P3 AppKit egress
```

## [03]-[NAMESPACES]

Namespace mirrors folder path — `.editorconfig` `dotnet_style_namespace_match_folder = true:error`: every fence under `Rasm.Grasshopper/<Folder>/` declares `namespace Rasm.Grasshopper.<Folder>;`, giving the six roots `Rasm.Grasshopper.Canvas`, `Rasm.Grasshopper.Components`, `Rasm.Grasshopper.Document`, `Rasm.Grasshopper.Eto`, `Rasm.Grasshopper.Platform`, and `Rasm.Grasshopper.Shell`.

The boundary compiles as ONE assembly — the single `Rasm.Grasshopper.csproj` — so members cross the six namespaces with no build edge. `Eto.Forms`, `Eto.Drawing`, `Rasm.Domain`, and the `Grasshopper2.*` roots arrive as project-level global usings, so fences name host members bare.

Host-name resolution is one law: inside `Rasm.Grasshopper.*` the first identifier of a partial qualification re-resolves against the boundary's own namespaces (`Eto.Forms.X` binds `Rasm.Grasshopper.Eto`, `Canvas.X`/`Document.X`/`Shell.X`/`Platform.X`/`Components.X` bind the sibling sub-namespaces), so fences name host members BARE through the global usings; a host type no global using reaches spells `global::` in full, and a simple-name collision between two host namespaces resolves through one project-level `<Using Include="..." Alias="..." />` row in `Rasm.Grasshopper.csproj` — never a per-fence `using` alias, never a partial qualification. Fully-qualified `Grasshopper2.*` spellings stay valid because no boundary namespace shadows that root.
