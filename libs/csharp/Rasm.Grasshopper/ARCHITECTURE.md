# [RASM_GRASSHOPPER_ARCHITECTURE]

The domain map of `Rasm.Grasshopper` — the Grasshopper2 and Eto host boundary on the C# app strata, a planning-scoped package whose whole design corpus lives under one `.planning/` root in six sub-domain folders. It references the `Rasm` kernel and no other sibling; host-agnostic math composes the kernel motion and colour surfaces rather than a second in-folder derivation. Namespace mirrors folder path (`[03]`).

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Grasshopper/
├── Canvas/                 # Paint, wire, layout, motion, and interaction owners over the live GH2 canvas
│   ├── Canvas.cs           # CanvasOperator.Apply/.Read over the live Canvas: navigation, projection, marquee, sparkle, pick, rasterize
│   ├── Interaction.cs      # ResponderSpec hit-test targets, focus stack, drag, dwell/menu policy, edge resize over host flex dispatch
│   ├── Layout.cs           # Arrangement folded to pivot deltas sealed as one undo mutation; host snapping/stretch solvers; snap solving
│   ├── Motion.cs           # GH2 pacing adapter: Animated<T>, Animators, IFlexControl drive, AnimatedPath glyphs, re-paced off kernel rows
│   ├── Paint.cs            # Declarative Mark union, PaintStock Eto-resource executor, PaintPhase rows over the eight host paint events
│   └── Wires.cs            # Wire route geometry over host WireShape, RouteStyle custom routes, wire pick, marquee select, WireSkin pen pass
├── Components/             # Component authoring, pin catalog, Garden data transfer, attribute chrome, native object catalog
│   ├── Attributes.cs       # ComponentChrome: every host callback as one ChromeEvent case, Respond fold, ChromeReceipt over one dispatch spine
│   ├── Component.cs        # ComponentSpec: the one declaration — identity, pins, Execution shape, lifecycle, chrome; SpecComponent projection
│   ├── Data.cs             # GardenData: IDataAccess transfer, Garden tree algebra, ConversionServer cast-or-convert, the closed GhFault family
│   ├── Objects.cs          # NativeObject: special/routing/composite families as NativeKind rows, PersistedValue cases, GH1 interop boundary
│   └── Ports.cs            # PortRow: data-driven pin catalog over the modular adders, generated side/depth vocabularies, PinTrim policy
├── Document/               # The graph transaction spine, query/wire operator, undo ledger, solution controller
│   ├── Document.cs         # DocumentScope: minting across inert/inactive/active tiers, keyed-value shelves, the one Transact graph gate
│   ├── Graph.cs            # GraphScope: Ask over ObjectList/Connectivity, Mutate over Connections/SplitWire, each sealed into the ledger
│   ├── History.cs          # HistoryLedger: ActionList sealed into the branching History tree, stride/re-root/replay, the shared Seal spelling
│   └── Solution.cs         # SolutionControl: SolutionServer launch/halt/cancel, deferred-expiry protocol, Watch/Trace lifecycle fold
├── Eto/                    # Native control construction, two-way binding, the UI-thread runtime floor, window/dialog/menu spine
│   ├── Binding.cs          # BindingRail: IndirectBinding/BindableBinding/DualBinding fusion, ValueGate admission, DataScope, StoreRow carriers
│   ├── Controls.cs         # ControlForge.Realize: recursive ControlSpec over the Eto.Forms surface, FieldTag/FieldValue capture, role rows
│   ├── Runtime.cs          # EtoDispatch UI-thread marshal, UiClock over Lease<T>, the Transfer clipboard algebra, Display/InputState facts
│   └── Windows.cs          # CommandDeck, recursive MenuNode fold, WindowSpec/WindowChrome + WindowVerb, DialogSpec<TResult>/PickerSpec gate
├── Platform/               # The Eto handler seam, the gated macOS-native AppKit touch, the CoreAnimation compositing owner
│   ├── Composition.cs      # CoreAnimation: CALayer graph in CATransaction batches, CADisplayLink pacing, kernel-sampled drives, filter seams
│   ├── Handlers.cs         # PlatformSeam capability demand, Handlers widget-to-handler substrate, Styler frozen style rows, Bridge embedding
│   └── Native.cs           # NativeSeam: gated AppKit — NSView extraction, NSEvent monitors, gestures, pressure — MacGate, Fault.Unsupported
└── Shell/                  # The session spine, UI event algebra, editor-shell control, chrome intent, vector-icon owner
    ├── Chrome.cs           # Chrome.Apply: toolbar, input-panel, tooltip, floating-button demand onto the mintable GH2 chrome hosts, one gate
    ├── Editor.cs           # EditorShell: Editor singleton chrome-pane ShellSlot rows, ShellToggle rows, ShellState receipt, BeginRhinoGetter
    ├── Events.cs           # UiSource vocabulary over every GH2/Eto stream, UiFact payload union, EventAnchor, UiSubscription over Lease<T>
    ├── Icons.cs            # Vector-icon owner: five AbstractIcon origins, keyed-state pose machine, IconContext filter chain, IconCatalog
    └── Session.cs          # GhSession: scope acquisition over editor/canvas/document, UI-thread Run, lifecycle receipts, SessionCache recency
```

## [02]-[SEAMS]

`Rasm.Grasshopper` composes the `Rasm` kernel downward and references no other package, so the codemap charters carry every kernel dependency and no bidirectional seam crosses this folder. Kernel motion, colour, identity, and validity surfaces — `Easing`, `PerceptualBlend`, `Lease<T>`, the `Op`-keyed `Fin<T>` rail, `ValidityClaim` — compose inside the owning source file and record on its charter, never elevated to a seam. The boundary exposes no wire to a sibling package and no cross-language contract; every host member it captures is decompile-verified against the installed Grasshopper2 and Eto assemblies, fixed in the folder `.api/` catalogs.

## [03]-[NAMESPACES]

Namespace mirrors folder path — `.editorconfig` `dotnet_style_namespace_match_folder = true:error`: every fence under `Rasm.Grasshopper/<Folder>/` declares `namespace Rasm.Grasshopper.<Folder>;`, giving the six roots `Rasm.Grasshopper.Canvas`, `Rasm.Grasshopper.Components`, `Rasm.Grasshopper.Document`, `Rasm.Grasshopper.Eto`, `Rasm.Grasshopper.Platform`, and `Rasm.Grasshopper.Shell`.

The boundary compiles as ONE assembly — the single `Rasm.Grasshopper.csproj` — so members cross the six namespaces with no build edge. `Eto.Forms`, `Eto.Drawing`, `Rasm.Domain`, and the `Grasshopper2.*` roots arrive as project-level global usings, so fences name host members bare.
