export const meta = {
    name: 'convert-host-finish',
    description:
        'Continuation of the convert-host campaign (run wf_f27cf61a-18a, stopped at codex-quota exhaustion): every completed stage — survey, catalogs, architects, and six landed unit implements — is baked in as journal-reconstructed data, and every remaining lane runs NATIVE Claude (fable implement/critique/redteam/law, opus docs/acceptance) with zero codex calls. Remaining work: six unit implements, all twelve critique+redteam chains with fold-forward, per-folder index docs, the acceptance fan, and the terminal law fixer.',
    whenToUse: 'Launch once to finish the stopped convert-host run natively. Ephemeral - delete with convert-host.js after the campaign lands.',
    phases: [
        {
            title: 'Build',
            detail: 'per unit: fable ground-up implement (six units baked from the stopped run), native fable critique (fixlog to disk, receipt on the wire), fable terminal red-team folding the critique rows; opus index docs per folder',
        },
        {
            title: 'Close',
            detail: 'native opus acceptance fan per folder (full disposition + symbol/boundary/kernel/law audit, products to disk) then ONE terminal fable law fixer: manifests, index rows, backlog drain, orphaned critique fixlogs, acceptance findings re-verified as signals, law-doc sweep',
        },
    ],
};

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 14; // runtime concurrency clamp is min(16, cores-2) = 14 on this machine
const STAGGER_MS = 1500;
const STALL = 300000;
const SOL_STALL = 2400000; // sol ultra critique holds one blocking MCP call up to the 1800s tier ceiling; stall detection must outlast it
const MAX_UNITS = 6;
const UNIT_PAGES = 8; // pages per unit ceiling; editing fidelity degrades past ~8 dense pages per writer
const CAT_AUTHORS = 5; // catalog-author fan ceiling per folder; the planner packs the ruled roster into at most this many disjoint write groups
const CODEX = false; // continuation: codex quota exhausted — every lane runs native (fable critique, opus acceptance)
const CODEX_DIR = '.claude/scratch/convert-host'; // per-run MCP reports and unit seam ledgers

// Kernel mining slices: the Rasm planning corpus is too large for one honest full-read lane.
const KERNEL_SLICES = [
    { key: 'core', dirs: 'Domain/, Numerics/, Spatial/' },
    { key: 'motion', dirs: 'Parametric/, Processing/, Solving/' },
    { key: 'visual', dirs: 'Analysis/, Drawing/, Meshing/' },
];

const FOLDERS = [
    {
        key: 'rhino',
        root: 'libs/csharp/Rasm.Rhino',
        name: 'Rasm.Rhino',
        host: 'RhinoCommon (Rhino 9 WIP) + Eto.Forms/Eto.Drawing',
        census: [
            { key: 'a', paths: 'Blocks/ and Camera/' },
            { key: 'b', paths: 'Commands/, Capture.cs, and Events.cs' },
            { key: 'c', paths: 'Exchange/' },
            { key: 'd', paths: 'UI/' },
        ],
        mining: [
            {
                key: 'rcdoc',
                catalogs: 'api-rhinocommon-document.md, api-rhinocommon-commands.md, api-rhinocommon-blocks.md, api-rhinocommon-fileio.md',
                charge:
                    'RhinoCommon document + command substrate on Rhino 9 WIP: RhinoDoc lifecycle and events, the object tables ' +
                    '(objects/layers/materials/groups/views/named views/instance definitions), RhinoApp/Command infrastructure, the ' +
                    'full Get* interactive families, transaction/undo records, units and tolerance regimes, block/instance-definition ' +
                    'surfaces, file IO incl. the format-engine roster and FilePdf, and every Rhino-9-new capability those surfaces carry.',
            },
            {
                key: 'rcvis',
                catalogs: 'api-rhinocommon-display.md, api-rhinocommon-geometry.md, api-rhino-ui.md',
                charge:
                    'RhinoCommon display + UI on Rhino 9 WIP: the geometry-to-display pipeline, DisplayConduit and custom draw, ' +
                    'ViewCapture, render pipeline hooks, display modes and materials, viewport/camera control, and Rhino.UI ' +
                    'panels/dialogs/pages/gumball/toolbars - plus every Rhino-9-new display/UI capability the current assemblies carry.',
            },
            {
                key: 'eto',
                catalogs: 'api-eto-forms.md, api-eto-runtime.md, api-eto-drawing.md, api-macos-native.md',
                charge:
                    'Eto.Forms + Eto.Drawing as shipped with Rhino 9: the FULL control roster, layout containers (dynamic/table/' +
                    'pixel/stack), data binding (IndirectBinding/BindableBinding, MVVM patterns), dialogs and semi-modal, custom drawing ' +
                    '(Drawable, Graphics, paths/brushes/pens/text), styles and platform handlers, Rhino UI integration surfaces ' +
                    '(Panels.RegisterPanel, RhinoEtoApp, EtoExtensions), toolbars, clipboard/drag-drop/notifications, plus the ' +
                    'platform-gated AppKit/CoreAnimation vsync-and-cosmetics seam - the capability set a generator-shaped UI layer ' +
                    'should own so any native Rhino UI element is a row, never hand assembly.',
            },
        ],
    },
    {
        key: 'gh',
        root: 'libs/csharp/Rasm.Grasshopper',
        name: 'Rasm.Grasshopper',
        host: 'Grasshopper2 SDK (GH2 on Rhino 9 WIP) + Eto',
        census: [
            { key: 'a', paths: 'Components/' },
            { key: 'b', paths: 'UI/Canvas.cs, UI/Ui.cs, UI/Document.cs, and UI/Editor.cs' },
            { key: 'c', paths: 'UI/Events.cs, UI/Input.cs, UI/Interaction.cs, and UI/Layout.cs' },
            { key: 'd', paths: 'UI/Motion.cs, UI/Paint.cs, and UI/Wire.cs' },
        ],
        mining: [
            {
                key: 'gh2',
                catalogs: 'api-gh2-components.md, api-gh2-document.md',
                charge:
                    'The Grasshopper2 SDK as shipped with Rhino 9 WIP: the component model (Component base, construction-time ' +
                    'inputs/outputs, Access levels, the pin/parameter families), the document/solver model (Document, DocumentMethods, ' +
                    'ObjectList, Connectivity/Connections, SolutionServer lifecycle, expiry), data model (Garden trees/paths/pears), ' +
                    'special objects, undo actions, plugin registration, and every GH2-vs-GH1 paradigm break - GH1 idioms ' +
                    '(GH_Component, IGH_*, RegisterInputParams, SolveInstance signatures) are LEGACY POISON: naming one as current is a ' +
                    'defect. GH2 documentation is sparse; the decompile IS the truth.',
            },
            {
                key: 'gh2ui',
                catalogs: 'api-gh2-canvas.md, api-gh2-editor.md, api-gh2-flex.md, api-gh2-interaction.md',
                charge:
                    'GH2 canvas + chrome UI: canvas paint phases and picking, skinning, wire shapes, snapping/alignment, the ' +
                    'flex/animation substrate (Motion.Pacer/Spring, RepaintRequest, Subscription, MotionEquations), editor/toolbar/' +
                    'input-panel/tooltip chrome, and floating buttons - the capability set a higher-order layer should own so custom ' +
                    'component UI and canvas overlays are rows. GH2 documentation is sparse; the decompile IS the truth.',
            },
            {
                key: 'etogh',
                catalogs: 'api-eto-forms.md, api-eto-runtime.md, api-eto-drawing.md, api-macos-native.md',
                charge:
                    'Eto.Forms + Eto.Drawing as hosted inside the GH2 process: the FULL control roster, layout containers ' +
                    '(dynamic/table/pixel/stack), data binding (IndirectBinding/BindableBinding, MVVM patterns), dialogs and ' +
                    'semi-modal, custom drawing (Drawable, Graphics, paths/brushes/pens/text), styles and platform handlers, ' +
                    'Eto-hosted panels/dialogs inside GH2, clipboard/drag-drop/notifications, plus the platform-gated AppKit/' +
                    'CoreAnimation gesture/vsync/cosmetics seam - the capability set so any GH2-native panel, dialog, or overlay ' +
                    'chrome is a row, never hand assembly.',
            },
        ],
    },
];

// Baked completed-work state, journal-reconstructed from run wf_f27cf61a-18a; `done` maps unit key -> landed implement fixlog.
const STATE = {
    folders: [
        {
            key: 'rhino',
            name: 'Rasm.Rhino',
            root: 'libs/csharp/Rasm.Rhino',
            arch: {
                preSwap: 'd08e0f0c645ac199b51429080cd7737e7eed0f12',
                units: [
                    {
                        key: 'document',
                        pages: [
                            'libs/csharp/Rasm.Rhino/.planning/Document/session.md',
                            'libs/csharp/Rasm.Rhino/.planning/Document/tables.md',
                            'libs/csharp/Rasm.Rhino/.planning/Document/events.md',
                            'libs/csharp/Rasm.Rhino/.planning/Document/geometry.md',
                        ],
                        owns:
                            'Document session/scope/capability lease vocabulary (live+headless RhinoDoc identity, readiness, run-mode, ' +
                            'units/tolerances), the one component-table transaction rail and its receipt vocabulary, the one document ' +
                            'observation stream (event families, delivery policy, scheduling), and the host GeometryBase crossing carriers.',
                        charter:
                            'Foundation unit; every other unit composes its vocabulary. session.md: one document capability owner over ' +
                            'RhinoDoc live/headless identity, lifecycle flags, run-mode, model/page unit and tolerance regimes, custom ' +
                            'units, TimeoutActiveGet - absorbing census Commands/Context.cs (kill the eager all-subsystem facade; ' +
                            'capability demand is declared, not constructed) and the capability-context half of Exchange/RhinoFiles.cs ' +
                            '(kill nullable ambient host fields and Live/Offline factory names; one admitted capability context proves ' +
                            'operation eligibility). tables.md: one typed table-transaction rail over the FULL Rhino 9 component-table ' +
                            'roster per api-rhinocommon-document.md (Objects/Layers/Materials/Groups/Views/NamedViews/InstanceDefinitions/' +
                            'NamedConstructionPlanes/NamedPositions/NamedLayerStates/Snapshots/SectionStyles/Markups/PageViewGroups/' +
                            'Render* tables), object mutation with history, selection/visibility/locking, undo records and custom undo ' +
                            'events, deferred redraw - absorbing census Commands/Document.cs DocumentEdit (kill object-typed geometry ' +
                            'ingress: conversion parameterizes through the kernel Normalization coercion lattice Kind.Of/CoerceTo; kill ' +
                            'opaque resource-mutation delegates: table resources are typed transaction rows). events.md: one ' +
                            'observation-stream owner rebuilt from census Events.cs WatchBus - typed per-family payload projection (kill ' +
                            'the lossy common payload), explicit delivery policy for per-frame draw channels (kill the all-phases ' +
                            'default), Rhino 9 additions WorksessionFileChanged/SectionStyleTableEvent/MarkupTableEvent/' +
                            'PageViewGroupTableEvent, one scheduling/coalescing/back-pressure policy composing kernel Domain rails; the ' +
                            'generic dispatcher/idle-queue duplication collapses into this one owner. geometry.md: host-bound GeometryBase ' +
                            'crossing per api-rhinocommon-geometry.md - shallow-duplicate/document-control identity, user strings, ' +
                            'DataCRC, transform decomposition (DecomposeSimilarity/Rigid/Affine), bounding-box derivation, ' +
                            'ClippingPlaneSurface clip participation. Kernel compose rows: Op/Fault/Requirement/OpAcceptance rails, ' +
                            'Context.Of(RhinoDoc), ContentHash.Of for any content identity. Catalogs: api-rhinocommon-document.md, ' +
                            'api-rhinocommon-geometry.md.',
                        after: [],
                    },
                    {
                        key: 'eto',
                        pages: [
                            'libs/csharp/Rasm.Rhino/.planning/Eto/elements.md',
                            'libs/csharp/Rasm.Rhino/.planning/Eto/binding.md',
                            'libs/csharp/Rasm.Rhino/.planning/Eto/chrome.md',
                            'libs/csharp/Rasm.Rhino/.planning/Eto/canvas.md',
                            'libs/csharp/Rasm.Rhino/.planning/Eto/runtime.md',
                            'libs/csharp/Rasm.Rhino/.planning/Eto/platform.md',
                        ],
                        owns:
                            'The full native Eto UI construction algebra: the element/control generator-row vocabulary and layout algebra, ' +
                            'the data-binding surface, window/dialog/menu/toolbar/command chrome, the Eto.Drawing immediate-2D owner, the ' +
                            'ambient Eto runtime (dispatch/timers/input/clipboard/drag-drop/notification/tray/screen), and the ' +
                            'platform-handler/native-host/theme/print seam.',
                        charter:
                            'The MANDATORY Eto sub-domain: full native UI construction as generator rows, referencing only the Rasm kernel ' +
                            'and host assemblies - Eto is THE UI framework, no Rasm.AppUi coupling. elements.md: one typed ' +
                            'element-realization algebra rebuilt from census UI/Elements.cs - the FULL current control roster per ' +
                            'api-eto-forms.md (Calendar, CheckBoxList, ComboBox, FontPicker, ImageView, LinkButton, ListBox, PasswordBox, ' +
                            'ProgressBar, PropertyGrid, RadioButton, RichTextArea, SearchBox, SegmentedButton, Spinner, TabControl, ' +
                            'TreeView, WebView, GridView/TreeGridView cell+item families, and the census-era set) as rows on one ' +
                            'realization dispatch (kill the five-control runtime-type event switch and per-control public factories), ' +
                            'DynamicLayout/TableLayout/PixelLayout/StackLayout construction absorbed as layout rows. binding.md: the ' +
                            'IndirectBinding/DirectBinding/BindableBinding/DualBinding/DataContext surface (census absence: zero binding ' +
                            'usage despite hand-rolled state sync) - typed binding rows with conversion, child paths, delayed propagation, ' +
                            'validation cadence; this replaces the census-era manual TextChanged/CheckedChanged plumbing. chrome.md: ' +
                            'windows, Eto dialogs (ShowModal/ShowModalAsync), MenuBar/ContextMenu/ToolBar, Command/CheckCommand/' +
                            'RadioCommand state synchronization (absorbing the Eto-construction half of census UI/Panel.cs UiChromeOp/' +
                            'UiAction - kill the parallel menu/toolbar/command APIs), plus the PrintDocument/PrintDialog/' +
                            'PrintPreviewDialog/PageSettings/PrintSettings document-output flow per api-eto-printing.md. canvas.md: the ' +
                            'Eto.Drawing owner per api-eto-drawing.md - Graphics primitives, GraphicsPath construction and fill/stroke ' +
                            'hit-testing (kills census Paint.cs hand-rolled hit testing per LIBRARY_DEPTH), FormattedText/MeasureString ' +
                            '(kills hard-coded font metrics), Matrix composition, gradient/texture brushes, Bitmap.Lock pixel access, ' +
                            'Color conversion at the boundary (perceptual blend composes the kernel color extension), SystemFonts, ' +
                            'Drawable lifecycle. runtime.md: Application Invoke/AsyncInvoke/InvokeAsync/RunIteration, UITimer, Screen/' +
                            'Mouse/Keyboard/Cursors, Clipboard/DataObject typed transfer, drag effects, Notification, TrayIndicator, ' +
                            'Taskbar progress per api-eto-runtime.md. platform.md: Eto.Platform backend selection and Supports/Create ' +
                            'feature discovery, NativeControlHost lifecycle, AttachNative/DetachNative, TriggerStyleChanged, and the ' +
                            'theme-transition seam including EtoExtensions.Get per api-eto-platform.md. Kernel compose rows: UI state/' +
                            'validation composes Domain rails; color math composes the kernel perceptual color extension (Numerics/' +
                            'atoms.md [02]-[SCALAR_FLOOR]); no second state, easing, or color derivation in this sub-domain. Catalogs: ' +
                            'api-eto-forms.md, api-eto-drawing.md, api-eto-runtime.md, api-eto-platform.md, api-eto-printing.md.',
                        after: [],
                    },
                    {
                        key: 'interaction',
                        pages: [
                            'libs/csharp/Rasm.Rhino/.planning/Commands/command.md',
                            'libs/csharp/Rasm.Rhino/.planning/Commands/acquisition.md',
                            'libs/csharp/Rasm.Rhino/.planning/Commands/options.md',
                            'libs/csharp/Rasm.Rhino/.planning/Commands/selection.md',
                            'libs/csharp/Rasm.Rhino/.planning/Blocks/model.md',
                            'libs/csharp/Rasm.Rhino/.planning/Blocks/operations.md',
                            'libs/csharp/Rasm.Rhino/.planning/Blocks/graph.md',
                            'libs/csharp/Rasm.Rhino/.planning/Blocks/lifecycle.md',
                        ],
                        owns:
                            'The native command lifecycle and stage algebra, the declarative input-acquisition matrix, the command-line ' +
                            'option vocabulary, the ObjRef/pick projection owner, and the whole instance-definition domain: block state ' +
                            'model, the one polymorphic block operation rail, the QuikGraph-composed definition graph, and block lifecycle/' +
                            'preview leasing.',
                        charter:
                            'Two document-adjacent interaction sub-domains, both composing the document unit vocabulary. Commands/' +
                            'command.md: RasmCommand native adaptation rebuilt from census Command.cs - ONE total transition algebra for ' +
                            'effect/branch/prompt/commit/cancel/exit (kill the parallel EffectCase/BranchCase/CommitCase class roster and ' +
                            'the PromptStage virtual escape hatch), Rhino lifecycle ingress/egress only at the Command adapter, replay/' +
                            'scripted-token/history as policy rows, plus command registration, BeginCommand/EndCommand/UndoRedo events, ' +
                            'GetMostRecentCommands/GetCommandStack/RunProxyCommand (Rhino 9 additions), and RhinoApp script execution as ' +
                            'the ONE sanctioned script row (RunScript elsewhere is killed). acquisition.md: ONE declarative input-kind ' +
                            'matrix rebuilt from census Input.cs - each row carries native getter, result projection, scripted decode, ' +
                            'policy admission, lifetime, receipt (kill the typeof(T) registry and per-getter factory branches); full ' +
                            'GetBaseClass/GetObject/GetPoint/GetTransform surface plus RhinoGet modal family including Rhino 9 GetDistance/' +
                            'GetViewports per api-rhinocommon-commands.md; point constraints, snap/construction points, dynamic draw and ' +
                            'gumball feedback as policy dimensions. options.md: ONE option vocabulary row family from census Options.cs - ' +
                            'native binder, localization, scripted decode, current-value projection, validation, lifetime per row (kill ' +
                            'the CLR-type switch factory and the dynamic enum escape hatch); AddOptionString and the enum-list members are ' +
                            'verified rows. selection.md: ONE typed ObjRef projection owner from census Selection.cs - each admissible ' +
                            'projection (Brep/Face/Edge/Trim/SubD parts/Curve/Surface/Mesh/annotations/grips/InstanceDefinitionPart, ' +
                            'selection method/point/view evidence) is a declared capability contract (kill the FrozenDictionary Type-keyed ' +
                            'extractor roster); PickContext configuration is a host policy dimension. Blocks/model.md: compact block state ' +
                            'vocabulary from census State.cs - one typed identity+policy vocabulary with native enum adaptation at the ' +
                            'edge (kill per-native-enum wrapper types, public System.Drawing.Bitmap ownership, local Fnv64 - ' +
                            'ContentHash.Of is the hasher). operations.md: ONE polymorphic instance-definition operation rail from census ' +
                            'Operations.cs (kill the 19-case BlockOp plus BlockAttributeTask/BlockInstanceTask/LinkLifecycle/TableMutation ' +
                            'parallel unions and the RunScript path) - authoring, Modify/ModifyGeometry, linked-source transitions ' +
                            '(ModifySourceArchive/RefreshLinkedBlock/UpdateLinkedInstanceDefinition/DestroySourceArchive/' +
                            'MakeSourcePathRelative), Delete/Undelete/Purge/PurgeUnused/Compact/Export (Rhino 9), instance placement/' +
                            'replacement/explode (AddExplodedInstancePieces, TransformWithHistory), preview bitmaps (both ' +
                            'CreatePreviewBitmap overloads), TextFields attribute extraction per api-rhinocommon-blocks.md - one admission/' +
                            'transaction/redraw/receipt path. graph.md: ONE definition-graph rail over live InstanceDefinitionTable and ' +
                            'offline File3dm sources from census Archive.cs - reference topology (GetReferences/GetContainers/' +
                            'UsesDefinition/UseCount overloads), linked-archive closure, cycle grouping, dependency ordering COMPOSED from ' +
                            'QuikGraph (kill hand-rolled reachability/SCC/depth passes and the hard-coded closure depth); unit conversion ' +
                            'composes kernel Context, never a second policy. lifecycle.md: one block lifecycle rail from census Blocks.cs ' +
                            '- InstanceDefinitionTableEvent ingress composing the Document observation stream, versioned preview leases, ' +
                            'idle-deferred refresh, document eviction, native disposable release (kill parallel vaults and the cache-mode ' +
                            'enum). Kernel compose rows: Domain rails/Context everywhere; RhinoMath.UnitScale only inside kernel Context ' +
                            'composition. Catalogs: api-rhinocommon-commands.md, api-rhinocommon-blocks.md, api-rhinocommon-document.md.',
                        after: ['document'],
                    },
                    {
                        key: 'visual',
                        pages: [
                            'libs/csharp/Rasm.Rhino/.planning/Viewport/camera.md',
                            'libs/csharp/Rasm.Rhino/.planning/Viewport/operations.md',
                            'libs/csharp/Rasm.Rhino/.planning/Viewport/capture.md',
                            'libs/csharp/Rasm.Rhino/.planning/Viewport/motion.md',
                            'libs/csharp/Rasm.Rhino/.planning/Display/conduit.md',
                            'libs/csharp/Rasm.Rhino/.planning/Display/draw.md',
                            'libs/csharp/Rasm.Rhino/.planning/Display/interaction.md',
                            'libs/csharp/Rasm.Rhino/.planning/Display/render.md',
                        ],
                        owns:
                            'The camera scope/model and the one camera operation rail, the ViewCapture render specification, the host ' +
                            'motion-pacing adapter (redraw targets, frame clocks, display-link drivers), the display-conduit phase algebra ' +
                            'with display modes and visual-analysis participation, the two-backend mark/draw renderer, in-viewport ' +
                            'interaction (mouse/gumball/UI objects), and the Rhino.Render renderer lifecycle.',
                        charter:
                            'Viewport/camera.md: one camera model from census Camera/State.cs separating kernel pose/frustum/intent ' +
                            '(VectorFrame, VectorIntent - composed, never re-derived) from a typed Rhino scope lease and native ' +
                            'ViewportInfo snapshot adapter (kill the second camera-frame algebra, public retention of disposable ' +
                            'snapshots, System.Drawing screen carriers, live-host objects as domain values); main views, page details, ' +
                            'depth/DoF, construction planes, screen transforms as typed host rows. operations.md: ONE polymorphic camera ' +
                            'operation request from census Camera/Operations.cs + Camera.cs (kill the per-modality gesture/projection/lock/' +
                            'path union family and the separate single/broadcast/rig execution surfaces) - keyboard/mouse gestures, ' +
                            'projection changes incl. Rhino 9 IsometricCamera SetProjection and CameraAngle, view/cplane stacks, ' +
                            'ZoomBoundingBox, NamedViewTable full family incl. RestoreWithAspectRatio/RestoreAnimatedConstantTime, ' +
                            'clipping-plane participation, detail commits, ViewSettings as scoped policy rows, motion-compile lowering ' +
                            'kernel MotionInterpolation output into camera edits; execution absorbs scope resolution, UI-thread dispatch, ' +
                            'redraw reduction with typed receipts. RULED KILL executed here: Camera/Architecture.cs recipe catalog dies; ' +
                            'architectural two-point/plan/axon/section/RCP conventions re-land as kernel VectorIntent policy rows (kernel ' +
                            'extension row: Rasm/.planning/Processing/intent.md) lowered through this rail. capture.md: one resolved ' +
                            'capture specification from census Capture.cs - target, media, layout, scale, decoration, output, lifetime as ' +
                            'explicit data (kill order-dependent scale precedence and hidden Policy state); Bitmap, SVG, transparent, and ' +
                            'SendToPrinter delivery as modality rows per api-rhinocommon-display.md; ViewCaptureSettings full surface ' +
                            'incl. CreatePreviewSettings/SetWindowRect. motion.md: the host pacing adapter from census UI/Motion.cs and ' +
                            'Camera/Motion.cs boundary halves - RedrawTarget rows (view/document/overlay/Eto canvas), frame clock ' +
                            'selection over CADisplayLink (NSScreen.GetDisplayLink - the NSView spelling is a phantom, silently deleted), ' +
                            'Eto UITimer fallback, RhinoApp.Idle, reduced-motion and screen-parameter policy rows per api-macos-native.md; ' +
                            'ALL easing/spring/interpolation/Oklab/cycle mathematics COMPOSES the kernel extensions (Parametric/' +
                            'projections.md [03]-[MOTION] easing+cycle; Numerics/integrate.md [04]-[STEPPER] spring/decay modules; ' +
                            'Numerics/atoms.md [02]-[SCALAR_FLOOR] perceptual color) - zero UI-local numerics. Display/conduit.md: one ' +
                            'display-conduit algebra from census Overlay.cs conduit half - phase participation (ObjectCulling through ' +
                            'DrawOverlay, viewport Bind/ExclusiveBind/UnbindAll - Rhino 9), pipeline state stacks (depth/clip/cull/' +
                            'projection/model-transform), selection/object-id filters, plus DisplayModeDescription management, ' +
                            'VisualAnalysisMode, CustomDisplay, IsoDrawEffect/DisplayPen rows per the WIP delta (kill the sibling breach: ' +
                            'viewport projection reads host members directly, never Camera package types; selection previews take ' +
                            'kernel-neutral values, never CommandSelection). draw.md: ONE mark/draw algebra from census Paint.cs - typed ' +
                            'mark rows dispatched over two backends (DisplayPipeline geometry/sprite/text/annotation draws incl. Rhino 9 ' +
                            'DrawInstanceDefinitionShaded/DrawSurfaceDirectionIndicators/DrawCurvaturePreview; Eto Graphics via the eto ' +
                            'canvas owner), gradients composing the kernel color extension, hit-testing and text metrics composing Eto ' +
                            'GraphicsPath/FormattedText capability (kill hand-rolled samplers/metrics/HUD arithmetic). interaction.md: ' +
                            'in-viewport interaction from census Overlay.cs interaction half + WIP delta - MouseCallback with paired end ' +
                            'events, MouseState, gumball lifecycle (GumballObject Set* family, GumballDisplayConduit PickGumball/' +
                            'UpdateGumball), and the Rhino 9 UserInterfaceObjectBase/Grip/Direction/Rotation/TextDot/UserInterfaceSlider ' +
                            'families as typed rows. render.md: NEW territory per api-rhinocommon-render.md - RenderPipeline render/pause/' +
                            'resume, RealtimeDisplayMode framebuffer/middleground hooks and pass state, RenderWindow channels, post-effect ' +
                            'execution control, RenderTexture evaluation composing the display pipeline. Catalogs: ' +
                            'api-rhinocommon-display.md, api-rhinocommon-render.md, api-rhino-ui.md, api-macos-native.md, ' +
                            'api-eto-drawing.md.',
                        after: ['document'],
                    },
                    {
                        key: 'exchange',
                        pages: [
                            'libs/csharp/Rasm.Rhino/.planning/Exchange/archive.md',
                            'libs/csharp/Rasm.Rhino/.planning/Exchange/formats.md',
                            'libs/csharp/Rasm.Rhino/.planning/Exchange/operations.md',
                            'libs/csharp/Rasm.Rhino/.planning/Exchange/publish.md',
                            'libs/csharp/Rasm.Rhino/.planning/Exchange/sheets.md',
                        ],
                        owns:
                            'The File3dm archive transaction family, the generated format-codec capability matrix with file-type plug-in ' +
                            'registration, the one host exchange transaction rail (open/import/export/save, named tables, geolocation, ' +
                            'batch), the publication pipeline (PDF/print/raster/SVG), and the sheet/layout/detail transaction family.',
                        charter:
                            'archive.md: ONE total archive request/result family from census Exchange/Archive.cs (kill projection-specific ' +
                            'entrypoints and parallel graph/metadata representations) - path+byte ingress, filtered/metadata-only reads ' +
                            '(Read with TableTypeFilter/ObjectTypeFilter, ReadNotes/ReadArchiveVersion/ReadRevisionHistory/' +
                            'ReadEarthAnchorPoint/ReadApplicationData/ReadPageViews/ReadDimensionStyles - Rhino 9 ReadPageViews admitted), ' +
                            'full table roster incl. render content and embedded files, mutation, byte serialization, IsValid/Audit/' +
                            'Polish, preview images, one resource graph with derived projections; resource identity composes ' +
                            'ContentHash.Of. formats.md: ONE codec capability matrix from census Formats.cs (kill the handwritten BuiltIn ' +
                            'roster and per-format option-builder families) - each row carries extensions, phase capability, host engine ' +
                            'adapter (the full direct-engine roster incl. Rhino 9 FileNwd), typed option projection, receipt behavior; ' +
                            'derived import/write/export/prompt filtering from one correspondence; PLUS the Rhino.PlugIns FileImportPlugIn/' +
                            'FileExportPlugIn/FileTypeList registration territory per api-rhinocommon-file-plugins.md as capability rows ' +
                            'binding formats into the host file-dialog dispatch. operations.md: ONE host exchange transaction family from ' +
                            'census Operations.cs + the runtime half of RhinoFiles.cs (kill parallel FileOp entrypoints, the separate ' +
                            'FileNativeTable union, consumer-visible live-vs-headless orchestration, and RunScript) - document mode, ' +
                            'ingress, operation, lifecycle policy, receipt projection on one rail; NamedLayerStates/NamedPositions full ' +
                            'families, EarthAnchorPoint/Sun geolocation with kernel-composed transform policy, batch policy, headless ' +
                            'document lifetime composing the Document session lease; the exchange vocabulary of census State.cs ' +
                            'distributes into the owning pages - kernel-grade file/collision/policy algebra composes Domain owners, host ' +
                            'option adapters stay boundary-local (no vocabulary shell page). publish.md: ONE publication request/result ' +
                            'family from census Publish.cs (kill target-specific WritePdf/WritePrinter/WriteRaster/WriteSvg pipelines and ' +
                            'per-target capture callbacks) - source, render intent, output carrier, page stream, policy, artifacts, ' +
                            'evidence; FilePdf full surface (AddPage both forms, Draw* family, PreWrite, GetCustomPages/SetCustomPages, ' +
                            'LayersAsOptionalContentGroups), printer/raster/vector egress derived from one capture correspondence ' +
                            'composing the Viewport capture specification; token interpolation and stamping compose kernel algebra. ' +
                            'sheets.md: ONE sheet transaction family from census Sheets.cs (kill the 21-case FileSheetEdit verb roster and ' +
                            'per-case private mutators) - page selection, detail selection, desired state, mutation policy, conflict ' +
                            'evidence, receipts on one state correspondence; AddPageView/ImportPageView/ReorderPageViews/PageViewCount, ' +
                            'detail creation/commit (AddDetailView, CommitViewportChanges, TryGetPaperLength/ModelLength), Rhino 9 ' +
                            'page-view group membership (AddToPageViewGroup/GetPageViewGroupList/IsInPageViewGroup/' +
                            'RemoveFromPageViewGroup), per-viewport layer overrides, clipping planes, scale audit; frame/grid/anchor/' +
                            'numbering arithmetic composes kernel numeric owners. Catalogs: api-rhinocommon-fileio.md, ' +
                            'api-rhinocommon-file-plugins.md, api-rhinocommon-document.md, api-rhinocommon-display.md.',
                        after: ['document', 'visual'],
                    },
                    {
                        key: 'hostui',
                        pages: [
                            'libs/csharp/Rasm.Rhino/.planning/HostUi/panels.md',
                            'libs/csharp/Rasm.Rhino/.planning/HostUi/pages.md',
                            'libs/csharp/Rasm.Rhino/.planning/HostUi/dialogs.md',
                            'libs/csharp/Rasm.Rhino/.planning/HostUi/shell.md',
                        ],
                        owns:
                            'Rhino.UI host integration: panel registration/lifecycle/docking with RUI toolbar-file state, the polymorphic ' +
                            'options/document-properties/object-properties page adapter, the native dialog intent rail with file prompts ' +
                            'and resource/preview loading, and the host shell (UI-thread dispatch, status/prompt/toast/progress, ' +
                            'RhinoEtoApp window ownership, EtoExtensions styling).',
                        charter:
                            'panels.md: one panel-and-chrome owner from census Panel.cs (kill the WatchBus coupling - panels own their ' +
                            'host callbacks via Panels.Show/Closed and IPanel PanelShown/PanelHidden/PanelClosing directly; kill parallel ' +
                            'placement/state/menu-sync APIs) - RegisterPanel both overloads, GetPanel/GetPanels with document instances, ' +
                            'OpenPanel/OpenPanelAsSibling/FloatPanel/ClosePanel/IsPanelVisible with selected-tab evidence, dock-bar state, ' +
                            'ChangePanelIcon, RuiUpdateUi menu registration, ToolbarFileCollection/ToolbarFile/Toolbar full mutation ' +
                            'surface, EtoCollapsibleSection holders; Eto menu/toolbar construction composes the eto chrome vocabulary. ' +
                            'pages.md: ONE polymorphic host-page adapter from census Pages.cs (kill the per-base wrapper-class pair and ' +
                            'registration-overload dispatch) - options, document-properties, application-preferences, object-properties, ' +
                            'stacked-child rows on one typed page discriminant; all override callbacks owned once (OnActivate/OnApply/' +
                            'OnCancel/RunScript/OnDefaults/OnHelp/OnCreateParent/OnSizeParent, ShouldDisplay/UpdatePage/ModifyPage/' +
                            'GetSelectedObjects), navigation styling, RhinoEtoApp DocumentPropertiesWindowForPage/' +
                            'ApplicationPreferencesWindowForPage. dialogs.md: ONE typed native-intent discriminant from census Intent.cs ' +
                            'with the Exchange breach dissolved (kill using Rasm.Rhino.Exchange; kill one-factory-per-dialog) - the full ' +
                            'Rhino.UI.Dialogs roster (ShowMessage, text/list/multi-list/check-list/property-list/context-menu/edit/number, ' +
                            'layer/linetype/print-width/sun/color dialogs), Eto and Rhino file prompts (SaveFileDialog/OpenFileDialog/' +
                            'SelectFolderDialog) returning kernel-neutral request/result values, DrawingUtilities resource and preview ' +
                            'family (BitmapFromSvg with dark-mode, CreateMeshPreviewImage, CreateLinetypePreviewGeometryEx, icon/bitmap ' +
                            'scale-down loaders), semi-modal presentation composing shell ownership. shell.md: ONE host-runtime operation ' +
                            'surface from census Ui.cs (kill separate static helpers per effect) - RhinoApp UI-thread affinity ' +
                            '(IsOnMainThread/InvokeOnUiThread/InvokeAndWait), command prompt state incl. Rhino 9 CommandPromptChanged, ' +
                            'StatusBar panes and the document-scoped progress meter with created/refused/foreign ownership evidence, ' +
                            'RhinoView.ShowToast, RhinoEtoApp.MainWindowForDocument and EtoExtensions window discovery/styling/position ' +
                            'persistence (UseRhinoStyle/Show/ShowSemiModal/SavePosition/RestorePosition/LocalizeAndRestore/' +
                            'WindowsFromDocument); status aggregation composes Domain rails. Catalogs: api-rhino-ui.md, api-eto-forms.md, ' +
                            'api-rhinocommon-display.md.',
                        after: ['document', 'eto'],
                    },
                ],
                packageDeltas: [
                    'No Directory.Packages.props motion required: QuikGraph is already centrally pinned (2.5.0) and ' +
                        'LanguageExt.Core/Thinktecture.Runtime.Extensions flow via the UseWorkspaceLibraries condition; the Blocks ' +
                        'graph owner consumes QuikGraph through the new folder-local PackageReference row applied to libs/csharp/' +
                        'Rasm.Rhino/Rasm.Rhino.csproj (folder edit, not a central one). Host assemblies (RhinoCommon, Rhino.UI, Eto, ' +
                        'Eto.macOS, Microsoft.macOS, System.Drawing.Common) remain owned by the Directory.Build.props ' +
                        'IsRhinoCommonAwareProject/IsRhinoUiAwareProject classification rows - no change.',
                ],
            },
            done: {
                document: {
                    files: [
                        'libs/csharp/Rasm.Rhino/.planning/Document/session.md',
                        'libs/csharp/Rasm.Rhino/.planning/Document/tables.md',
                        'libs/csharp/Rasm.Rhino/.planning/Document/events.md',
                        'libs/csharp/Rasm.Rhino/.planning/Document/geometry.md',
                        '.claude/scratch/convert-host/rhino-document-seams.md',
                    ],
                    verdict: 'authored',
                    summary:
                        'Ground-up authored the four Rasm.Rhino Document foundation pages (1,900 LOC total, fence mass 400-700+ per ' +
                        'page). session.md: one DocumentSession capability owner \u2014 DocKey [ValueObject<uint>] over ' +
                        'RuntimeSerialNumber, SessionPhase fold over the five lifecycle flags plus ReadOnly/Locked/Headless/' +
                        'Engaged(InGetPoint) snapshot, SessionMode lanes, SessionNeed capability rows with Admits delegate columns ' +
                        '(kills the census eager six-subsystem facade and the RhinoFiles nullable-ambient-field Live/Offline pair), ' +
                        'one polymorphic Of over SessionSource with document lifetime on kernel Lease<RhinoDoc> (Borrowed live / Owned ' +
                        'headless), Context.Of(RhinoDoc) threading, TimeoutActiveGet interrupt, and DocumentSpace rows collapsing the ' +
                        'Model*/Page* unit-tolerance member family with the UnitChange adjustment rail and custom units. tables.md: ' +
                        'one TableKind vocabulary (17 catalog tables + 3 event-only rows with Sweep reclamation delegate columns), one ' +
                        'TableTarget addressing union with derived query factories, one TableOp verb union (add/replace/delete/' +
                        'move-with-history/amend/state/selection/flash/revive/expunge/ordered-cloud/rebind/reclaim/import-page/' +
                        'named-view-restore/undo-stack roll) committed through Tables.Commit(session, plan) with UndoBracket, redraw ' +
                        'suppression, deferred Views.Redraw, custom undo, and one TableReceipt fact stream (TableSlot + FactBody ' +
                        'payload union); geometry ingress parameterizes through kernel Kind.Of/Requirement.ForKind/' +
                        'CoerceTo<GeometryBase> onto Lease<GeometryBase> \u2014 the 13-arm GeometrySource roster and the opaque ' +
                        'Func<RhinoDoc,Op,Fin<Unit>> resource delegate are dead. events.md: one EventFamily roster (banded keys, ~40 ' +
                        'rows including Rhino 9 ViewSettled/WorksessionFileChanged/SectionStyleTableEvent/MarkupTableEvent/' +
                        'PageViewGroupTableEvent) with Cadence columns and typed per-family bind delegates, one EventPayload union ' +
                        "preserving each family's native evidence (kills the lossy common payload), explicit Delivery policy (Inline/" +
                        'Deferred/Paced over StreamLane channel rows with receipted StreamLoss) adjudicated by the cadence row so ' +
                        'PerFrame draw channels require a dropping lane and the all-phases default is dead, one DocumentStream spine ' +
                        'absorbing the census WatchBus/EventDispatcher/WatchIdle/channel-wrapper quadruplication, with kernel Op.Catch ' +
                        'guarding every sink (RhinoUi.Protect coupling killed) plus the debounced file-observation modality. ' +
                        'geometry.md: GeometryCrossing custody on Lease<GeometryBase> decided by IsDocumentControlled, chained ' +
                        'GeometryCrc over DataCRC (federation identity stays kernel ContentHash), TagOp user-string rail with ' +
                        'absence-projected reads, Motion mutation union, DecomposeAs rows with tolerance columns onto the ' +
                        'Decomposition result union plus Option-al inverse, BoundsRequest three-shape derivation onto GeometryBounds, ' +
                        'and the ClipScope participation union with forward and inverse on one owner. Every host member is catalog- or ' +
                        'census-verified; unverified argument surfaces are held as RESEARCH items and excluded from fences. Seam ' +
                        'ledger written with five SEAM_CHANGED rows.',
                    deltas: [
                        {
                            symbol: 'DocKey',
                            change:
                                'minted [ValueObject<uint>] document key over RuntimeSerialNumber with Resolve(Op), Of(RhinoDoc,Op), ' +
                                'Census(includeHeadless); zero serial dies at the factory; host uint never travels bare',
                        },
                        {
                            symbol: 'SessionPhase / SessionSnapshot',
                            change:
                                'minted lifecycle vocabulary (Ready/Opening/Closing/Initializing/Creating/Unavailable) + one flag-product fold ' +
                                'carrying ReadOnly/Locked/Headless/Engaged; replaces RhinoDocState.IsReady bool',
                        },
                        {
                            symbol: 'SessionMode',
                            change: 'minted run lanes Interactive/Scripted/Headless with Live/Prompting columns; OfRunMode re-closes host RunMode at the seam',
                        },
                        {
                            symbol: 'SessionNeed',
                            change:
                                'minted keyless capability rows (Read/Observe/Mutate/Undo/Redraw/Prompt/Export) each a ' +
                                '[UseDelegateFromConstructor] Admits(SessionSnapshot,SessionMode) column',
                        },
                        {
                            symbol: 'SessionSource / HeadlessSource',
                            change:
                                'minted admission unions Live/Keyed/Opened/Headless(Template|Archive|Configured) with Acquire returning ' +
                                'Lease<RhinoDoc> (Borrowed live, Owned headless)',
                        },
                        {
                            symbol: 'DocumentSession',
                            change:
                                'minted the one capability owner: Of(source,mode,params needs) admission, Demand(need) use-time re-proof, ' +
                                'Interrupt() via TimeoutActiveGet, Context.Of(RhinoDoc) threading, IDisposable lease release; replaces ' +
                                'RhinoCommandContext and the FileRuntime capability half',
                        },
                        {
                            symbol: 'DocumentSpace / UnitRegime / CustomUnit / UnitChange / SessionUnits',
                            change:
                                'minted the Model/Page axis rows collapsing the Model*/Page* host member family, the live-read regime bundle, ' +
                                'custom units, and the AdjustLength/AdjustSystem/SetCustom adjustment rail with per-request geometry-scaling ' +
                                'payload',
                        },
                        {
                            symbol: 'TableKind',
                            change:
                                'minted 20-row component-table vocabulary (17 catalog + 3 event-only) with ModelComponentType, Accessible, and ' +
                                'Sweep reclamation delegate columns; Reclaim lifts the sweep onto Fin',
                        },
                        {
                            symbol: 'TableTarget',
                            change:
                                'minted Ids/Filter/Where addressing union with derived factories Of/Query/OnLayer/Tagged/Painted/Within/' +
                                'Clipping; Resolve and Runtime folds; no pick/CommandSelection case \u2014 interaction hands id sets',
                        },
                        {
                            symbol: 'TableOp',
                            change:
                                'minted the one 16-case transaction verb union (Add/Replace/Delete/Move/Amend/State/ClearSelection/Flash/' +
                                'Revive/Expunge/Cloud/Rebind/Reclaim/ImportPage/RestoreView/Roll) with total generated dispatch; replaces ' +
                                'DocumentOp + opaque Resource delegate',
                        },
                        {
                            symbol: 'GeometryIntake',
                            change:
                                'minted kernel-lattice ingress: GeometryBase short-circuits Borrowed, everything else Kind.Of -> ' +
                                'Requirement.ForKind -> CoerceTo<GeometryBase> onto Lease<GeometryBase>.Owned; kills the 13-arm GeometrySource ' +
                                'roster',
                        },
                        {
                            symbol: 'NamedRestore / HistoryRoll / SelectionPolicy / TableCustomUndo / RedrawPolicy / TableTransaction',
                            change:
                                'minted the named-view restore union (Proportional/Animated), the Undo/Redo/Clear stack-navigation union ' +
                                '(unrecorded sole-op law), the five-axis selection value, the custom-undo registration, the deferred-redraw ' +
                                'policy, and the commit plan with Batch/Navigate factories',
                        },
                        {
                            symbol: 'Tables / UndoBracket',
                            change:
                                'minted the one commit spine Tables.Commit(DocumentSession, TableTransaction) \u2014 session-demand gating ' +
                                '(Mutate/Undo/Redraw), undo bracket sealed on every exit, redraw suppression/restore, deferred Views.Redraw, ' +
                                'receipt fold; replaces DocumentEdit.Commit',
                        },
                        {
                            symbol: 'TableSlot / FactBody / TableFact / TableReceipt',
                            change:
                                'minted the one consequence fact stream: 15 slots, Object/Component/Record/Named payload union, + monoid, ' +
                                'slot-keyed projections, before/after selection delta; replaces DocumentReceipt/DocumentReceiptSlot',
                        },
                        {
                            symbol: 'Cadence',
                            change:
                                'minted keyless Discrete/Burst/PerFrame rows each owning its delivery law as an Admits(Delivery) delegate ' +
                                'column \u2014 PerFrame admits only a dropping paced lane',
                        },
                        {
                            symbol: 'EventFamily',
                            change:
                                'minted the banded ~40-row observation roster (lifecycle/object/selection/table/view/page/display/draw/panel) ' +
                                'with typed bind delegates, per-(viewport,doc) change-counter conflation on ProjectionChanged, ' +
                                'LayerTableEventArgs NewState admission law, and the Rhino 9 additions ViewSettled/WorksessionFile/' +
                                'SectionStyleTable/MarkupTable/PageViewGroupTable; replaces WatchPhase with no all-phases default',
                        },
                        {
                            symbol: 'EventPayload / EventEnvelope / DocEvent',
                            change:
                                'minted typed per-family payload union (Opened/Saved/UnitsScaled/UserString/Objects/Replaced/Attributes/' +
                                'Selection/LayerDelta/TableDelta/ViewDelta/PageDelta/Projection/DisplayMode/Frame/Panel...) and the ' +
                                'DocEvent(Family, Option<DocKey>, Payload) carrier \u2014 no RhinoDoc handle rides the event; kills the lossy ' +
                                'WatchPayload compression',
                        },
                        {
                            symbol: 'StreamLane / StreamLoss / Delivery',
                            change:
                                'minted channel rows Mailbox/Shed/Ordered/Firehose with receipted drop loss and the Inline/Deferred/Paced ' +
                                'delivery union; loss facts key (lane, family) into DocumentStream.Losses',
                        },
                        {
                            symbol: 'EventScope / Observation / FileObservation / Watch / DocumentStream',
                            change:
                                'minted the one observation spine: whole-request admission before any attach, Paced channel hand-off returning ' +
                                'the reader, Deferred idle drain via whole-queue CAS, kernel Op.Catch sink guarding, debounced ' +
                                'FileSystemWatcher modality; collapses WatchBus + EventDispatcher + WatchIdle + SubscribeChannel',
                        },
                        {
                            symbol: 'Subscription / Reentrancy / IdlePump',
                            change:
                                're-minted the detacher algebra (delegate-identity attach, | merge, idempotent Dispose) guarded by kernel ' +
                                'Op.Catch instead of RhinoUi.Protect, plus per-thread reentrancy suppression and the RhinoApp.Idle pump',
                        },
                        {
                            symbol: 'GeometryCrossing / GeometryCrc / TagOp / TagResult',
                            change:
                                'minted custody (Detach onto Lease<GeometryBase> decided by IsDocumentControlled, explicit Shallow with ' +
                                'SharesNative fact), the chained DataCRC change probe (federation identity stays kernel ContentHash.Of), and ' +
                                'the user-string verb union with absence-projected reads landing HashMap<string,string>',
                        },
                        {
                            symbol: 'Motion / Frame / DecomposeAs / Decomposition / Placement',
                            change:
                                'minted the in-place mutation union (Rigid/Translate/Spin/Grow), the verified Transform factory surface, ' +
                                'tolerance-columned decomposition request rows dispatched through generated Switch onto typed Similarity/Rigid/' +
                                'Affine results, Option-al TryGetInverse, and TransformBoundingBox',
                        },
                        {
                            symbol: 'BoundsRequest / GeometryBounds / Bounds',
                            change: 'minted the World/Under/InPlane derivation union onto one typed result with oriented-box option and copy-based Inflated projection',
                        },
                        {
                            symbol: 'ClipScope / ClipState / ClipParticipation',
                            change:
                                'minted the Everything/Only/Except participation union (enabled-flag x exclusion-bool product collapsed), ' +
                                'Option<double> depth verb, viewport clip pair \u2014 read and write on one owner',
                        },
                    ],
                    deferred: [
                        {
                            files: [
                                'libs/csharp/Rasm.Rhino/.planning/Exchange/operations.md',
                                'libs/csharp/Rasm.Rhino/.planning/Exchange/archive.md',
                            ],
                            claim:
                                'Exchange unit composes DocumentSession in place of the killed FileRuntime capability half and the Live/' +
                                "Offline factory pair; document lifetime arrives as the session's Lease<RhinoDoc>, eligibility as SessionNeed " +
                                'rows (Export/Mutate), and IoScheduler policy stays exchange-owned \u2014 no nullable ambient host field ' +
                                're-mints there',
                        },
                        {
                            files: ['libs/csharp/Rasm.Rhino/.planning/Commands/command.md', 'libs/csharp/Rasm.Rhino/.planning/Commands/selection.md'],
                            claim:
                                'Interaction unit composes DocumentSession (RhinoCommandContext is dead: capability demand declares ' +
                                'SessionNeed rows) and hands pick/selection products to the tables rail as Guid sets through TableTarget.Of ' +
                                '\u2014 TableTarget deliberately carries no pick or CommandSelection case',
                        },
                        {
                            files: ['libs/csharp/Rasm.Rhino/.planning/Display/conduit.md', 'libs/csharp/Rasm.Rhino/.planning/Viewport/motion.md'],
                            claim:
                                'Visual unit observes DrawForeground/DrawOverlay only through DocumentStream with Delivery.Paced over a ' +
                                'dropping StreamLane (Cadence.PerFrame admission law); the conduit page owns drawing INTO the pipeline while ' +
                                'the document stream owns observing it \u2014 no second draw-event subscription path',
                        },
                        {
                            files: ['libs/csharp/Rasm.Rhino/.planning/HostUi/panels.md'],
                            claim:
                                'HostUi panel lifecycle composes EventFamily.PanelShown/PanelHidden/PanelClosed from the document stream ' +
                                'rather than re-subscribing Rhino.UI.Panels statics',
                        },
                        {
                            files: ['libs/csharp/Rasm.Rhino/.planning/Blocks/operations.md'],
                            claim:
                                'Blocks unit composes TableOp.Rebind as the one instance-replace path (ObjectTable.ReplaceInstanceObject) ' +
                                'instead of minting a second replace site on its operation rail',
                        },
                        {
                            files: ['libs/csharp/Rasm.Rhino/.planning/Document/events.md', 'libs/csharp/Rasm.Rhino/.planning/Document/tables.md'],
                            claim:
                                'RESEARCH verification leg: assay decompile of InstanceDefinitionTableEventArgs / SectionStyleTableEventArgs / ' +
                                'MarkupTableEventArgs / PageViewGroupTableEventArgs / WorksessionFileChangedEventArgs / ' +
                                'RhinoTransformObjectsEventArgs+RhinoAfterTransformObjectsEventArgs member surfaces and the RhinoDoc.Linetypes/' +
                                'Lights/DimensionStyles accessors; on verification, key the four unkeyed event families with their typed ' +
                                'payloads and flip the three TableKind.Accessible rows with their mutation arms',
                        },
                    ],
                    indexRows: [
                        {
                            doc: 'libs/csharp/Rasm.Rhino/README.md',
                            row:
                                '[DOCUMENT] router: [01]-[SESSION](.planning/Document/session.md): the one document capability owner \u2014 ' +
                                'DocKey identity, SessionPhase readiness, SessionMode lanes, SessionNeed capability rows, Lease<RhinoDoc> ' +
                                'lifetime, DocumentSpace unit/tolerance regimes, TimeoutActiveGet interrupt',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Rhino/README.md',
                            row:
                                '[DOCUMENT] router: [02]-[TABLES](.planning/Document/tables.md): the component-table transaction rail \u2014 ' +
                                'TableKind roster, TableTarget addressing, TableOp verb union, kernel-lattice geometry intake, Tables.Commit ' +
                                'undo/redraw spine, TableReceipt fact stream',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Rhino/README.md',
                            row:
                                '[DOCUMENT] router: [03]-[EVENTS](.planning/Document/events.md): the one document observation stream \u2014 ' +
                                'EventFamily roster with cadence columns, typed EventPayload projection, explicit Inline/Deferred/Paced ' +
                                'delivery over StreamLane rows with receipted loss, DocumentStream spine',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Rhino/README.md',
                            row:
                                '[DOCUMENT] router: [04]-[GEOMETRY](.planning/Document/geometry.md): the host GeometryBase crossing \u2014 ' +
                                'Lease-carried custody, GeometryCrc change probe, Motion/Decomposition placement, BoundsRequest derivation, ' +
                                'TagOp user strings, ClipScope participation',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Rhino/ARCHITECTURE.md',
                            row:
                                'codemap: Document/Session.cs \u2014 DocKey/SessionPhase/SessionMode/SessionNeed capability owner, ' +
                                'SessionSource admission onto Lease<RhinoDoc>, DocumentSpace unit regimes',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Rhino/ARCHITECTURE.md',
                            row:
                                'codemap: Document/Tables.cs \u2014 TableKind/TableTarget/TableOp transaction rail, Tables.Commit undo-bracket ' +
                                'spine, TableSlot/TableFact/TableReceipt fact stream',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Rhino/ARCHITECTURE.md',
                            row:
                                'codemap: Document/Events.cs \u2014 Cadence/EventFamily/EventPayload observation roster, StreamLane/Delivery ' +
                                'policy, DocumentStream spine, Subscription algebra',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Rhino/ARCHITECTURE.md',
                            row:
                                'codemap: Document/Geometry.cs \u2014 GeometryCrossing custody/identity, Motion/DecomposeAs/Decomposition ' +
                                'placement, BoundsRequest/GeometryBounds, ClipScope participation',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Rhino/ARCHITECTURE.md',
                            row:
                                'seams: Document/Session.cs \u2190 csharp:Rasm/Domain/context # [BOUNDARY]: Context.Of(RhinoDoc) the one ' +
                                'doc-coupled tolerance ingress threaded by DocumentSession',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Rhino/ARCHITECTURE.md',
                            row:
                                'seams: Document/Tables.cs \u2190 csharp:Rasm/Domain/normalization # [BOUNDARY]: Kind.Of/' +
                                'CoerceTo<GeometryBase> coercion lattice onto Lease<GeometryBase> is the only geometry ingress',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Rhino/ARCHITECTURE.md',
                            row:
                                'seams: Document/Geometry.cs \u2190 csharp:Rasm/Domain/identity # [CONTENT_KEY]: federation identity composes ' +
                                'ContentHash.Of over kernel canonical bytes; GeometryCrc DataCRC stays in-process',
                        },
                    ],
                    phantoms: [
                        'InstanceDefinitionTableEventArgs / SectionStyleTableEventArgs / MarkupTableEventArgs / ' +
                            'PageViewGroupTableEventArgs argument members (Document/Index/EventType) \u2014 never written as settled fence ' +
                            'code; those families deliver unkeyed under AnyDocument scope with a RESEARCH row',
                        'WorksessionFileChangedEventArgs payload members \u2014 excluded; the family projects an empty Worksession payload pending verification',
                        'RhinoTransformObjectsEventArgs / RhinoAfterTransformObjectsEventArgs payload members \u2014 excluded; ' +
                            'Transforming payload carries no unverified evidence',
                        'RhinoDoc.Linetypes / RhinoDoc.Lights / RhinoDoc.DimensionStyles table accessors \u2014 excluded from the ' +
                            'mutation rail; the TableKind rows exist Accessible:false for their verified events only',
                        'RhinoDoc.ActiveDoc \u2014 never referenced; document resolution rides FromRuntimeSerialNumber and OpenDocuments exclusively',
                    ],
                },
                eto: {
                    files: [
                        'libs/csharp/Rasm.Rhino/.planning/Eto/elements.md',
                        'libs/csharp/Rasm.Rhino/.planning/Eto/binding.md',
                        'libs/csharp/Rasm.Rhino/.planning/Eto/chrome.md',
                        'libs/csharp/Rasm.Rhino/.planning/Eto/canvas.md',
                        'libs/csharp/Rasm.Rhino/.planning/Eto/runtime.md',
                        'libs/csharp/Rasm.Rhino/.planning/Eto/platform.md',
                        'libs/csharp/Rasm/.planning/Numerics/atoms.md',
                        'libs/csharp/Rasm.Rhino/.api/api-eto-printing.md',
                        '.claude/scratch/convert-host/rhino-eto-seams.md',
                    ],
                    verdict: 'authored',
                    summary:
                        'Ground-up authored all six Rasm.Rhino Eto unit pages (2061 lines, fences at full transcription depth) plus ' +
                        'the mandated kernel expand-form extension. Every host member in every fence is decompile-verified against the ' +
                        "installed Rhino 9 WIP Eto.dll via `uv run python -m tools.assay api query --key eto` (the mining lanes' " +
                        'blocked-assay gap is now closed \u2014 Slider/ProgressBar/Calendar/DateTimePicker/ColorPicker/FontPicker/' +
                        'SegmentedButton/RadioButton/ListBox/CheckBoxList/RichTextArea/Notification/TrayIndicator/Style.Add/' +
                        'Binding.Delegate/BindableBinding.Bind/DualBinding/Screen/StackLayout/Splitter/Window/TableLayout/' +
                        'DocumentControl/PrintDocument-events/GridColumn/CustomCell/TreeGridItemCollection/Color.FromArgb/SystemFonts ' +
                        'methods/GraphicsPath.AddPath and every enum roster are verified spellings, zero guesses, zero RESEARCH ' +
                        'leftovers except the one Rhino.UI notifier shape). elements.md: one recursive `Element` [Union] (15 cases) ' +
                        'over seven `[SmartEnum]` kind-row vocabularies covering the FULL current control roster, one total ' +
                        '`Realize(Op?)` dispatch (kills the census five-control event switch and per-control factories), the full grid/' +
                        'tree-grid/tree cell family as one `CellRow` union, and the four host layouts as one ' +
                        '`Arrangement`+`FlowRegion` algebra that structurally deletes the DynamicLayout Begin/End pairing hazard; also ' +
                        'mints the unit-wide `UiFault : Expected` family. binding.md: `BindPlan`/`Bind.Rig` rows over the verified ' +
                        'host `*Binding` properties with an admission `Convert` gate composing Domain rails, `StateCell<TState>` ' +
                        'bridging kernel `Atom` into `DirectBinding` via `Binding.Delegate`, `FlowMode`/`Cadence` rows (debounce = ' +
                        'host `AfterDelay`, commit = `LostFocus`), and the `BindLedger` rejection stream \u2014 replaces the census ' +
                        'hand-rolled TextChanged plumbing. chrome.md: ONE `IntentRow` table projecting `Command`/`CheckCommand`/' +
                        '`RadioCommand` into MenuBar/ContextMenu/ToolBar via `CreateMenuItem`/`CreateToolItem` (kills the parallel ' +
                        'UiChromeOp/UiAction APIs), `ShellPlan` windows, `Prompt<TResult>` modal rail (dismissal = typed fault, never ' +
                        'default sentinel), and `PrintPlan` whose pages are canvas Scenes routed Silent/Chooser/Preview. canvas.md: ' +
                        '`Mark`/`Scene` retained draw tree with ONE render fold and hit-testing off the SAME `PathSpec` via ' +
                        'FillContains/StrokeContains (kills census hand-rolled hit testing), `TypeRole`+`GlyphBlock` measurement via ' +
                        'FormattedText.Measure (kills hard-coded metrics), color quantized at exactly one `Pigment` site, `Surface` ' +
                        'Drawable lifecycle, and `PixelLease` riding kernel `Lease<T>` over `Bitmap.Lock`. runtime.md: `UiThread` ' +
                        'dispatch owner, `Pulse` leased UITimer, `Displays`/`PointerState`/`CursorRow`, `Mime`-keyed `PayloadSlot`/' +
                        '`TransferTarget`/`Transfer` typed clipboard+drag contract, `DragPlan`/`Drop`, `Toast`/`TrayLease`/' +
                        '`TaskbarPulse`. platform.md: `Backend` rows + `HostPlatform` Supports/Create/Find discovery, `NativeMount`/' +
                        '`NativeAttachment` both native-seam directions, `ThemeSeam` (Style.Add rows, weak tracked restyle set, ' +
                        'TriggerStyleChanged rebroadcast), and `ThemeCatalog` \u2014 a freeze-total (role \u00d7 variant) grid of ' +
                        'kernel `PerceptualColor` cells with WCAG contrast gates and atomic diff-emitting swap. KERNEL UNIFICATION: ' +
                        '`Numerics/atoms.md` [02]-[SCALAR_FLOOR] extended in expand-form with `PerceptualColor` ([ComplexValueObject] ' +
                        'OKLab+alpha composing Wacton.Unicolour: Mix/Ramp/Contrast/gamut-clipped ToRgb) and `BlendPath` (5 mix-path ' +
                        'rows) \u2014 canvas and platform compose it, no second color derivation exists; the DENSITY_BAR table gained ' +
                        'the color row and was repaired to gate-clean width. Prose gate passes zero FAIL across all touched docs; seam ' +
                        'ledger written.',
                    deltas: [
                        {
                            symbol: 'Rasm.Numerics.PerceptualColor',
                            change:
                                'minted in expand-form on atoms.md [02]-[SCALAR_FLOOR]: [ComplexValueObject] OKLab Lightness/OpponentA/' +
                                'OpponentB + Alpha, Of/OfRgb admission, Mix(other, UnitInterval, BlendPath?), Ramp(to, Dimension, BlendPath?), ' +
                                'Contrast, InRgbGamut, ToRgb() byte egress over Wacton.Unicolour',
                        },
                        {
                            symbol: 'Rasm.Numerics.BlendPath',
                            change: 'minted: [SmartEnum<int>] 5 rows (Oklab, OklchShorter/Longer/Increasing/Decreasing) carrying ColourSpace + HueSpan columns',
                        },
                        {
                            symbol: 'Rasm.Rhino.Eto.UiFault',
                            change:
                                'minted (elements.md): Expected-derived [Union] \u2014 Dismissed(Op), Unavailable(Op,Capability), ' +
                                'OffThread(Op), Rejected(Op,Field,Reason), AbsentPayload(Mime), HostRejected(Op,Detail); the unit-wide failure ' +
                                'vocabulary',
                        },
                        {
                            symbol: 'Rasm.Rhino.Eto.Element',
                            change:
                                'minted: recursive closed [Union], 15 cases (Text/Choice/Scalar/Pick/Press/Static/Boxed/Tabs/Split/Tabular/' +
                                'Laid/Painted/Embedded/Web/Inspector) with ONE Realize(Op?) -> Fin<Control> total dispatch',
                        },
                        {
                            symbol: 'Rasm.Rhino.Eto kind rows',
                            change:
                                'minted: TextKind(5)/ChoiceKind(6)/ScalarKind(4)/PickKind(5)/PressKind(2)/StaticKind(2)/BoxKind(4) ' +
                                '[SmartEnum<int>] vocabularies with [UseDelegateFromConstructor] Mint columns + TextPolicy/ChoicePolicy/' +
                                'ScalarPolicy/PickPolicy/StaticContent/BoxDress policy records',
                        },
                        {
                            symbol: 'Rasm.Rhino.Eto grid family',
                            change:
                                'minted: CellRow [Union] (8 cases over the full host cell roster), ColumnPlan, RowSeed, BranchSeed, ' +
                                'GridChrome, GridPlan [Union] (Flat/Branched/Outline over GridView/TreeGridView/TreeView)',
                        },
                        {
                            symbol: 'Rasm.Rhino.Eto.Arrangement + FlowRegion',
                            change:
                                'minted: layout algebra [Union]s \u2014 Flow/GridTable/Run/Absolute over DynamicLayout Add*/TableRow+TableCell/' +
                                'StackLayoutItem/PixelLayout; Begin*/End* scope protocol structurally bypassed',
                        },
                        {
                            symbol: 'Rasm.Rhino.Eto.ElementKey / ElementSpec / BindAttachment / BindReceipt',
                            change:
                                'minted: element identity VO, the uniform Apply spine (Enabled/Visible/ToolTip/Style + bind traversal), and ' +
                                'the erased elements<->binding wire seam',
                        },
                        {
                            symbol: 'Rasm.Rhino.Eto.Bind + BindPlan + BindSource + FlowMode + Cadence + StateCell + Lens + BindLedger + ContextScope',
                            change:
                                'minted (binding.md): Rig entry over host *Binding selectors, Convert-gated admission composing Domain rails, ' +
                                'Atom->DirectBinding bridge via Binding.Delegate, DualBindingMode seam rows, edit/debounced/commit cadence, ' +
                                'rejection ledger, DataContext scope',
                        },
                        {
                            symbol: 'Rasm.Rhino.Eto.IntentKey / CommandKind / IntentRow / PlacementSlot / IntentTable / IntentReceipt',
                            change:
                                'minted (chrome.md): the one verb table \u2014 Act/Toggle/Pick command kinds, gesture-conflict rejection at ' +
                                'Materialize, availability sweep, receipt stream, MenuOf/PopupOf/BarOf projections via CreateMenuItem/' +
                                'CreateToolItem',
                        },
                        {
                            symbol: 'Rasm.Rhino.Eto.ShellKind / ShellPlan / Prompt<TResult>',
                            change:
                                'minted: Form/FloatingForm window plans with table-bound Menu/ToolBar; typed modal rail over Dialog<T> \u2014 ' +
                                'affirm rows Close(result)+DefaultButton, dismissal folds to UiFault.Dismissed on ShowModal and ShowModalAsync',
                        },
                        {
                            symbol: 'Rasm.Rhino.Eto.PrintSpec / PrintRoute / PrintPlan / PrintReceipt',
                            change:
                                'minted: PrintSettings projection, Silent/Chooser/Preview routing over PrintDialog/PrintPreviewDialog, pages ' +
                                'as Seq<Scene> rendered via PrintPage events, taskbar progress projection, PageCount derived from the page seq',
                        },
                        {
                            symbol: 'Rasm.Rhino.Eto.Pigment / FillSpec / StrokeSpec / DashRow / Figure / PathSpec / Pose / TypeRole / GlyphBlock / ScenePolicy / Mark / Scene',
                            change:
                                'minted (canvas.md): the one PerceptualColor->Color mint, brush/pen/dash rows, path-atom union with Build fold ' +
                                '+ FillContains/StrokeContains hit tests, Matrix-factory pose union, SystemFonts role rows, FormattedText ' +
                                'measurement, and the retained Mark/Scene render+hit-test folds',
                        },
                        {
                            symbol: 'Rasm.Rhino.Eto.Redraw / SurfaceSpec / Surface / PixelLease',
                            change:
                                'minted: Drawable lifecycle owner \u2014 Atom<Scene> swap + Whole/Region/Immediate invalidation, ' +
                                'SupportsCreateGraphics-gated Lease<Graphics> acquire, IME verbs, Bitmap.Lock under Lease<BitmapData>',
                        },
                        {
                            symbol: 'Rasm.Rhino.Eto.UiThread / Pulse / PulseBeat / PulseLease / Displays / DisplayFacts / PointerState / ModifierState / CursorRow',
                            change:
                                'minted (runtime.md): the one dispatch boundary (On/Post/OnAsync/Guard/Pump over Application.Instance), leased ' +
                                'UITimer clock with beat evidence, Screen projection with railed capture, live input reads, cursor roster rows',
                        },
                        {
                            symbol: 'Rasm.Rhino.Eto.Mime / PayloadSlot / TransferTarget / Transfer / DragPlan / Drop / Toast / TrayLease / TaskbarPulse / PulseState',
                            change:
                                'minted: validated MIME VO, four-shape payload union, Board/Bundle target union with one write fold and ' +
                                'Option-railed reads, DoDragDrop plan + DragEventArgs admission record, Notification/TrayIndicator/Taskbar ' +
                                'owners with UnitInterval progress',
                        },
                        {
                            symbol:
                                'Rasm.Rhino.Eto.Backend / HostPlatform / NativeMount / NativeAttachment / StyleKey / ThemeSeam / ThemeVariant /' +
                                ' PaletteRole / ResolvedTheme / ThemeCatalog',
                            change:
                                'minted (platform.md): backend classification rows, Supports/Create/Find discovery with ThreadStart lease, ' +
                                'NativeControlHost eager/deferred + AttachNative lease, Style.Add row registry + weak restyle set + ' +
                                'TriggerStyleChanged rebroadcast, and the freeze-total 12-role x 2-variant PerceptualColor grid with WCAG ' +
                                'gates and diff-emitting Swap',
                        },
                    ],
                    deferred: [
                        {
                            files: ['libs/csharp/Rasm/.planning/Parametric/projections.md'],
                            claim:
                                'Kernel motion extension site [03]-[MOTION] (easing families, damped-spring, repeat/yoyo timeline arithmetic) ' +
                                'remains unextended \u2014 the kernel recon named it and the census Motion.cs demands it, but it serves the ' +
                                "visual unit's host motion adapter (Viewport/motion.md); the Eto unit ships zero temporal math, so the " +
                                'extension lands with the visual unit or the law tail, not raced here.',
                        },
                        {
                            files: ['libs/csharp/Rasm.Rhino/.planning/Eto/platform.md', 'libs/csharp/Rasm.Rhino/.api/api-rhino-ui.md'],
                            claim:
                                'The Rhino theme notifier returned by Rhino.UI.EtoExtensions.Get(Control) has an unverified event-member ' +
                                'spelling (assay decompile over Rhino.UI.dll needed); platform.md carries it as an explicit Research line and ' +
                                'ThemeSeam.OnHostThemeChanged(ThemeVariant) is the entry the HostUi shell wires the live transition into ' +
                                '\u2014 hostui unit owns that wiring.',
                        },
                        {
                            files: ['libs/csharp/Rasm.Rhino/.planning/HostUi/shell.md'],
                            claim:
                                'Rhino window styling (EtoExtensions.UseRhinoStyle/RestorePosition/SavePosition, census Elements.cs:360) is ' +
                                "deliberately absent from chrome.md ShellPlan/Prompt \u2014 the hostui unit's shell page owns applying Rhino " +
                                'style and position persistence to Eto windows; its shell page composes ShellPlan.Realize output, never ' +
                                're-mints window construction.',
                        },
                        {
                            files: ['libs/csharp/Rasm.Rhino/.planning/Eto/elements.md', 'libs/csharp/Rasm.Rhino/.planning/HostUi/dialogs.md'],
                            claim:
                                'Eto file/color/font chooser dialogs (OpenFileDialog/SaveFileDialog/SelectFolderDialog/ColorDialog/FontDialog ' +
                                'per api-eto-forms.md) are not realized in the Eto unit \u2014 the hostui dialog intent rail owns file prompts ' +
                                'per its charter; if hostui rules them out of scope, they land as one more chrome.md prompt family.',
                        },
                    ],
                    indexRows: [
                        {
                            doc: 'libs/csharp/Rasm/README.md',
                            row:
                                '[03]-[SUBSTRATE_PACKAGES] / [FUNCTIONAL_CORE]-adjacent concern gains: `Wacton.Unicolour` \u2014 the ' +
                                'perceptual color model behind the `Numerics/atoms` `PerceptualColor` owner (OKLab mix/ramp/contrast/gamut ' +
                                'egress); centrally pinned in Directory.Packages.props (already present at 8.0.0), consumed by the kernel and ' +
                                'composed by every host UI layer.',
                        },
                        {
                            doc: 'libs/csharp/Rasm/Rasm.csproj',
                            row:
                                'PackageReference row for `Wacton.Unicolour` (version centralized) \u2014 the kernel Numerics/atoms perceptual ' +
                                'color extension now composes it.',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Rhino/README.md',
                            row:
                                'Router gains the Eto sub-domain: .planning/Eto/elements.md (element-realization algebra + UiFault + layout ' +
                                'algebra), binding.md (bind rows over IndirectBinding/DirectBinding/BindableBinding/DualBinding), chrome.md ' +
                                '(intent table, windows, modal rail, print flow), canvas.md (Eto.Drawing Mark/Scene owner), runtime.md ' +
                                '(dispatch/clock/stage/transfer/drag/presence), platform.md (backend/native-host/style/theme grid); ' +
                                '[2]-[DOMAIN_PACKAGES] notes Eto is host-provided from the Rhino-loaded Eto.dll \u2014 no NuGet admission.',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj',
                            row:
                                'No new PackageReference for the Eto unit (Eto binds from the host assembly set); the only project edge ' +
                                'remains the Rasm kernel reference the color/lease/rails composition requires.',
                        },
                    ],
                    phantoms: [
                        'api-eto-printing.md TaskbarProgressState roster listed a `Normal` member that does not exist in the host ' +
                            'assembly \u2014 the decompiled enum is None/Progress/Indeterminate/Error/Paused; the catalog row is corrected ' +
                            "in place and runtime.md's PulseState rows bind TaskbarProgressState.Progress.",
                        'The census-era five-control runtime-type event switch (UI/Elements.cs BindState over TextBox/TextArea/' +
                            'CheckBox/NumericStepper/DropDown) is not carried forward in any form \u2014 value channels ride the host ' +
                            '*Binding properties through binding.md rows, so the switch has no successor surface.',
                    ],
                },
            },
        },
        {
            key: 'gh',
            name: 'Rasm.Grasshopper',
            root: 'libs/csharp/Rasm.Grasshopper',
            arch: {
                preSwap: 'd08e0f0c645ac199b51429080cd7737e7eed0f12',
                units: [
                    {
                        key: 'gh-runtime',
                        pages: [
                            'libs/csharp/Rasm.Grasshopper/.planning/Shell/session.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Shell/events.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Eto/runtime.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Eto/binding.md',
                        ],
                        owns:
                            'GhSession (host scope acquisition, Eto UI-thread execution, lifecycle/teardown/repaint receipts), UiEvents ' +
                            '(one source-row event algebra over GH2 canvas/document/solution/undo + Eto control/window/timer streams), ' +
                            'EtoDispatch/UiClock (Application sync/async/queued dispatch, UITimer), Transfer (typed clipboard/drag-drop ' +
                            'payload rows, notifications, screen metrics), BindingRail (IndirectBinding/BindableBinding/DataContext ' +
                            'two-way projections). Also authors the kernel expand-form extension: easing-family rows, analytic ' +
                            'damped-spring state, repeat/yoyo cycle arithmetic, and OKLab/OKLCH perceptual interpolation rows on libs/' +
                            'csharp/Rasm/.planning/Parametric/projections.md [03]-[MOTION].',
                        charter:
                            'Foundation unit \u2014 mints the session spine every other unit composes. Absorbs census GrasshopperUi ' +
                            '(Ui.cs): RepaintRequest/Teardown/Subscription/intent roster collapses into one host-session operator with ' +
                            'typed scope, dispatch, lifecycle, and fault settlement; BoundedCache is killed \u2014 recency caching ' +
                            'composes the admitted hybrid-cache substrate (libs/csharp/.api/api-hybrid-cache.md), never a local generic ' +
                            'cache. Absorbs census UiEvent (Events.cs): the thirteen provider-oriented cases and per-family handler ' +
                            'products collapse into one source-row algebra whose lifecycle/ownership receipts compose the Rasm kernel ' +
                            'Domain rails lease and validity vocabulary (libs/csharp/Rasm/.planning/Domain/rails.md) \u2014 no second ' +
                            'in-folder lifecycle algebra; AppKit monitor attachment rows are projected but their platform gating is owned ' +
                            "by gh-shell-chrome's Platform/native.md. Host capability targets: mining-gh-etogh api-eto-runtime rows (sync/" +
                            'async/generic/queued dispatch, timer lifecycle, typed binary/stream/object/HTML/URI clipboard formats, ' +
                            'notification activation, tray, keyboard lock, display density) and the api-eto-binding NEW catalog (no ' +
                            'current source touches binding \u2014 pure HOST/CONSUMER gap buildout). Kernel rows: every fallible operation ' +
                            'rides Op-keyed Fin rails and Context.Of(RhinoDoc?) tolerance/units; the kernel [03]-[MOTION] extension is ' +
                            'ADDITIVE expand-form only (rows beside the existing quaternion Linear/Slerp, spring stepping composing ' +
                            'Numerics/integrate [04]-[STEPPER], color floors composing the Unicolour substrate catalog) \u2014 re-read the ' +
                            'kernel page immediately before editing, the Rasm.Rhino architect runs concurrently; any rename/collapse of a ' +
                            'kernel surface is recorded deferred, never raced. Phantom kills (silent): Motion.Pacer, PacerOption, Spring, ' +
                            'Subscription, RepaintRequest, Paint.Hook, CanvasLease, UiRail, MotionClock, MotionVector, SpringConfig, ' +
                            'SpringHandle<T> \u2014 none exists in the installed Grasshopper2.xml catalog.',
                        after: [],
                    },
                    {
                        key: 'gh-eto',
                        pages: [
                            'libs/csharp/Rasm.Grasshopper/.planning/Eto/controls.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Eto/windows.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Platform/handlers.md',
                        ],
                        owns:
                            'ControlSpec (the full Eto control/layout generator row family: text/number/check/choice/slider/grid/tree/list/' +
                            'property-grid/tab/rich-text/browser/segmented/masked/progress/group/image/expander/native-host plus dynamic/' +
                            'table/pixel/stack layout), WindowSpec/DialogSpec (windows, modal/attached/file/folder dialogs), MenuSpec/' +
                            'CommandSpec (menus, commands, context menus), StyleRow/PlatformSeam (Style/StyleWidgetHandler/StyleHandler, ' +
                            'Platform/Platforms, WidgetHandler families, NativeControlHost, Eto.Mac managed-to-AppKit bridge).',
                        charter:
                            'The mandatory Eto sub-domain core: full native UI construction as generator rows \u2014 one polymorphic ' +
                            'ControlSpec row family the host adapter renders, never a hand-written control roster. Absorbs census Input ' +
                            '(Input.cs) construction half: the nine-shape FormField roster and object-valued capture are killed \u2014 ' +
                            'every Eto control becomes a typed generator row with semantic field, validation, and capture policy (COVERAGE ' +
                            'buildout from api-eto-forms: the census composed only text/number/checkbox/choice/colour/slider/file-dialog ' +
                            'while the installed surface carries the grid/tree/list/property-grid/tab/rich-text/browser/segmented/masked/' +
                            'progress/group/image/expander families \u2014 all admitted as rows, HOST gap). Dialogs/menus/commands land in ' +
                            'windows.md as spec rows with Rhino-styled presentation policy (Rhino.UI.EtoExtensions.UseRhinoStyle) ' +
                            'internalized. Platform/handlers.md realizes the api-eto-platform NEW catalog (zero current source references ' +
                            'Eto.Style/Platform/NativeControlHost \u2014 pure HOST gap buildout): scoped styling, platform capability ' +
                            'checks, handler identity, and native control extraction as one seam owner. Show/run projections compose ' +
                            "gh-runtime's GhSession dispatch receipts; construction rows themselves stay dispatch-free.",
                        after: ['gh-runtime'],
                    },
                    {
                        key: 'gh-components',
                        pages: [
                            'libs/csharp/Rasm.Grasshopper/.planning/Components/component.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Components/ports.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Components/data.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Components/attributes.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Components/objects.md',
                        ],
                        owns:
                            'ComponentSpec (one declaration owning identity/Nomen/IoId, pins, Process(IDataAccess) execution with single/' +
                            'batch/aggregate/heterogeneous input discrimination, BeforeProcess/PreProcess/PostProcess/PostProcessTree ' +
                            'lifecycle, ThreadingState, cancellation, variable-parameter hooks, bake capability, FleetingCustomData ' +
                            'persistence, and Plugin/PluginServer registration rows), PortRow (the data-driven pin catalog), GardenData ' +
                            '(tree algebra + transfer/conversion owner), ComponentChrome (component attribute interaction policy), ' +
                            'NativeObject (the standard + special object catalog).',
                        charter:
                            'Component authoring model. component.md absorbs census ComponentSpec (Component.cs) AND census OutputBinding ' +
                            '(Binding.cs): SpecBuilder method families, reflection Definition recovery, and the private read/run/release/' +
                            'empty delegate quartet all collapse into one declaration whose ModularComponent projection is the single ' +
                            'boundary adapter; plugin catalogue validation consumes the same declaration registration consumes (census ' +
                            'acceptance honored). ports.md absorbs census PortKind (Port.cs): the hand-enumerated ~110-member static Add* ' +
                            'roster demotes to seed DATA \u2014 one PortRow carries CLR type, input/output capability, Access/Requirement, ' +
                            'wire representation, parameter-property policy (VectorParameter/AngleParameter/IntegerParameter/' +
                            'CurveParameter/SurfaceParameter projections \u2014 full family, not five), presets, persistent data, and ' +
                            'visibility; namespace truth per mining seam: IDataAccess/InputAdder/OutputAdder/ComponentParameters are ' +
                            'Grasshopper2.Components, brokers are Grasshopper2.Parameters.Standard. data.md absorbs census Bridge ' +
                            '(Bridge.cs): process-global broker accumulation dies \u2014 broker participation becomes document/' +
                            'plugin-scoped rows with deterministic precedence; one transfer policy carries Item/Pear/Twig/Tree topology, ' +
                            'Garden algebra (TreeFromList/Pears/Leaves/Twigs, PairWiseOp, PearWiseOp, Twig.Convert/Apply), metadata/site ' +
                            'retention, ConversionServer/CastOrConvert brokers, unit/tolerance context (GetTolerance/GetUnitSystem/' +
                            'GetUnitScaling), diagnostics, and disposal. attributes.md absorbs census ComponentUi (Attributes.cs): fifteen ' +
                            'static Phase rows and callback subclasses collapse into one interaction receipt over layout/bounds/pivot/' +
                            'paint/panel/menu/pointer/keyboard/tooltip/resize/snapping/undo; the global CanvasSnapToObjects mutation gains ' +
                            'a boundary-scoped restoration receipt. objects.md realizes the two NEW catalogs (api-gh2-standard-components, ' +
                            'api-gh2-special-objects \u2014 HOST gap): Accumulation/Chain/Cluster/ClusterBoundary/Loop family and the ' +
                            '21-object Parameters.Special interactive family as one object-row catalog with persistent values and canvas ' +
                            'attribute state; GH1 interop is an explicit import boundary row. Kernel rows: execution rails compose ' +
                            'Rasm.Domain rails/Op keys; geometry ingress composes Domain normalization/evaluation; no session dependence ' +
                            '\u2014 this unit composes kernel + host only.',
                        after: [],
                    },
                    {
                        key: 'gh-document',
                        pages: [
                            'libs/csharp/Rasm.Grasshopper/.planning/Document/document.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Document/graph.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Document/history.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Document/solution.md',
                        ],
                        owns:
                            'DocumentScope (document lifecycle/IO/state: New{Inert,Inactive,Active}Document, Close, Store, Modified, ' +
                            'Globals, CustomValues/KeyedValues, NamedViews, Notes, Dependencies, Hash + the one graph transaction operator ' +
                            'absorbing the DocumentMethods verb surface: selection, clipboard, grouping, chaining, clustering, isolation, ' +
                            'migration, drop, colour/display overrides), GraphQuery/GraphMutation (ObjectList query/find/window-select/' +
                            'transfer/ID-remap/pins, Connectivity topology, Connections wire mutation, WireEnds, Shout/Listen wireless, ' +
                            'SplitWire), HistoryLedger (undo History/Node/Record branching, ActionList receipts), SolutionControl ' +
                            '(SolutionServer lifecycle, solution event family, delayed expiry, SolutionRecord phases).',
                        charter:
                            'The GH2 document graph. Absorbs census DocumentOp (Document.cs): the eighteen parallel unions/smart-enums ' +
                            '(ObjectScope, VisibilityChange, SelectionOp, ClipboardOp, ComposeOp, FindStrategy, DocumentQuery, GroupOp, ' +
                            'WirelessOp, DocumentHistory, DocumentMark, DocumentTargetOp, DocumentMutation, DocumentOp, ...) collapse into ' +
                            'one host-graph operator discriminating read/observe/transact/solve/persist intent through typed rows, and the ' +
                            'census-flagged ABSENCE closes: transaction receipts carry causal object/wire/history/solution/selection ' +
                            'deltas fed by SolutionServer + ObjectList + Document event subscriptions (composed via gh-runtime UiEvents ' +
                            'rows). Absorbs the document half of census WireOp (Wire.cs): local graph traversal, cycle detection, and ' +
                            'bounded path enumeration are KILLED by host absorption \u2014 Connectivity.FindImmediate/All{Inputs,Outputs}, ' +
                            'FindConnections, SubsetTopology, IsLinear, SortCausally, WithoutRelays and ObjectList.SearchUpstream/' +
                            'Downstream own the graph queries (catalog-verified); wire mutation rides Grasshopper2.Parameters.Connections ' +
                            '(namespace corrected from Doc per mining seam) with ActionList/undo receipts. HOST gap buildout from mining: ' +
                            'document IO and dependency surfaces, global pins (AddGlobalPin/RepairPins), object Transfer and ChangeAllIds/' +
                            'ApplyIdMap remapping, SelectedWires state, undo branching (Node.PromoteChild, FindCommonAncestor, ' +
                            'FindShortestPath), SolutionRecord phases/culmination, KeyedValues. UI-thread transaction execution and ' +
                            "repaint intent compose gh-runtime's GhSession receipts.",
                        after: ['gh-runtime'],
                    },
                    {
                        key: 'gh-canvas',
                        pages: [
                            'libs/csharp/Rasm.Grasshopper/.planning/Canvas/canvas.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Canvas/paint.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Canvas/wires.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Canvas/interaction.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Canvas/layout.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Canvas/motion.md',
                        ],
                        owns:
                            'CanvasOperator (projection/navigation via IFlexControl.Map/Navigate, pick-map resolution, redraw scheduling, ' +
                            'DrawToBitmap, window selection, inline/value editors, CanvasActions policy, sparkle overlays), PaintPlan ' +
                            '(declarative paint intent + one boundary executor owning Eto resource lifetime, the eight ordered paint ' +
                            'events, skin projection/interpolation, capsule/icon rendering), WireVisual (WireShape spline/elbow family, ' +
                            'WireSkin resolution, WireDrawCache observation, wire picking), CanvasInteraction (IResponsive/Responses ' +
                            'dispatch, focus stack, ObjectDragInteraction, dwell/context menu, ResizingFrame), CanvasLayout (alignment/' +
                            'distribution/grid/nudge over host snap solvers), MotionAdapter (Animated<T>/Motion/Duration/MotionEquations/' +
                            'Animators/AnimatedPath pacing over kernel motion rows).',
                        charter:
                            'The canvas rendering and interaction surface. canvas.md absorbs census CanvasOp (Canvas.cs): the ' +
                            'nineteen-case command union with parallel request/result/snapshot families collapses into one operator over ' +
                            'typed rows; boundary-owned numeric validation, interpolation, and cache mechanics are killed \u2014 kernel ' +
                            'composition only. paint.md absorbs census DrawMark/Paint (Paint.cs): the 20-case union separating into ' +
                            'declarative paint intent (geometry/text/image/icon/capsule/wire/clip/transform modalities) executed by one ' +
                            'owner holding brush/font/path lifetime, graphics-state restoration, and culling; the phantom CanvasPaintPhase ' +
                            'type dies \u2014 the eight catalog-verified Canvas events (Before/' +
                            'AfterPaint{Background,Groups,Wires,Objects}) plus CanvasBackgroundPaintEventArgs.OverrideDefaultPainting are ' +
                            'the paint seam. wires.md takes the canvas half of census WireOp (Wire.cs): reflection rails are KILLED \u2014 ' +
                            'Canvas.WireDrawCache and Canvas.WireAt are public catalog-verified members; the local orthogonal ' +
                            'WireRouteSolver is a ruled kill absorbed by the host WireShape family (WireShapeDefault.CreateSpline, ' +
                            'WireShapeElbow, ShapeType, Project/DistanceTo/Intersects/IsCoincident) plus ' +
                            'SnappingAction.CreateStraightenWireAction; simple-name type reflection in WireStyle dies. interaction.md ' +
                            'absorbs the canvas rows of census CanvasChromeOp (Interaction.cs): focus/IResponsive registration and ' +
                            'Response dispatch are UI.Flex members (namespace corrected per mining), ObjectDragInteraction and ' +
                            'ResizingFrame edge state land at full catalog depth; the Tooltip.Layout reflection path is a phantom kill ' +
                            '(Frame is verified, Layout is not). layout.md absorbs census LayoutArrangement (Layout.cs): local SnapAngle/' +
                            'ArcLines/RepelPair/EquidistanceGuides/GeometryActions math is killed by host absorption \u2014 SnappingAction ' +
                            'factories (align/gap/straighten), SnapSpace.CreateOrthogonal/Snap/Merge, SnappingConstraints, and ' +
                            'StretchLayoutSolver own the solving (all catalog-verified), residual angle/vector arithmetic composes kernel ' +
                            'Numerics atoms; every arrangement is a document mutation with gh-document transaction receipts and undo ' +
                            'ownership. motion.md resolves the census Motion blocker (Motion.cs): the host adapter composes the kernel ' +
                            '[03]-[MOTION] rows authored by gh-runtime (easing/spring/cycle/perceptual-color \u2014 never a second ' +
                            'host-local derivation) and owns only GH2 pacing: Animated<T> Create/Chain/Evaluate, Animators, Duration/' +
                            'State, IFlexControl.Animate/ScheduleRedraw/AnimatedZoomFactor, and the AnimatedPath feedback-path factories ' +
                            "(HOST gap \u2014 absent from every stub); macOS CoreAnimation cosmetics route to gh-shell-chrome's Platform/" +
                            'composition.md.',
                        after: ['gh-runtime', 'gh-document'],
                    },
                    {
                        key: 'gh-shell-chrome',
                        pages: [
                            'libs/csharp/Rasm.Grasshopper/.planning/Shell/editor.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Shell/chrome.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Shell/icons.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Platform/native.md',
                            'libs/csharp/Rasm.Grasshopper/.planning/Platform/composition.md',
                        ],
                        owns:
                            'EditorShell (Editor lifecycle/visibility/EnsureVisible/ThisOrRhino, tabs, breadcrumbs, status bar, ' +
                            'DefinedLayouts/InitialLayout, recent documents, notes, BeginRhinoGetter handoff, file-compare), ChromeSpec ' +
                            '(Toolbar.Bar item rows with state/layout/render, InputPanel category mutation, Tooltip.Frame + shortcut/' +
                            'text-and-icon painters, FloatingButton/Collection/Layout with occlusion), IconOwner (IIcon/AbstractIcon ' +
                            'FromResource/FromCode with diagnostics, MoveState animation, IconContext filters, DrawToBitmap), NativeSeam ' +
                            '(AppKit local monitors, gesture recognizers, pressure configuration, NSWorkspace observation \u2014 all ' +
                            'platform-gated), LayerComposition (CoreAnimation layer graphs, basic/spring/keyframe animations, ' +
                            'CADisplayLink vsync pacing, CoreImage filters, haptics, vibrancy).',
                        charter:
                            'Editor shell, chrome, and the macOS-native seam. editor.md absorbs census EditorOp (Editor.cs): the four ' +
                            'provider cases and direct singleton exposure collapse into one shell operator whose receipts project editor/' +
                            'canvas/document/shell state without singleton interrogation; the census-flagged uncaptured chrome closes here ' +
                            'at catalog depth (Tabs, BreadCrumbs, StatusBar, ThisOrRhino, EnsureVisible \u2014 HOST gap). chrome.md ' +
                            "absorbs the chrome rows of census CanvasChromeOp and census Input's toolbar/panel projections: FloatingButton/" +
                            'Collection/Layout are Grasshopper2.UI.Flex (namespace corrected from Canvas per mining), tooltip/' +
                            'floating-button/toolbar/input-panel capability composes through one chrome intent surface with typed ' +
                            'state-change events, numeric values, anchored placement, and occlusion (mining capability entry \u2014 richer ' +
                            'than the census inventory). icons.md owns the stateful vector icon family as a first-class authoring surface ' +
                            '(FromCode with CodeDiagnostic capture, state machine via MoveState with Duration/Motion, context filters) ' +
                            '\u2014 CONSUMER gap: every future plugin needs icon rows. Platform/native.md absorbs the AppKit rows of ' +
                            'census CanvasChromeOp/UiEvent/Motion and fixes the census-flagged missing platform admission: every AppKit/' +
                            'CoreAnimation/CoreImage touch is an explicitly admitted platform-bound row behind an ' +
                            'OperatingSystem.IsMacOS-gated capability policy with a typed non-native outcome. Platform/composition.md ' +
                            'takes the macOS half of the census Motion blocker: CALayer/CAShapeLayer/CAGradientLayer/CATextLayer/' +
                            'CAReplicatorLayer/CAEmitterLayer graphs, CABasicAnimation/CASpringAnimation/CAKeyFrameAnimation, ' +
                            'CAMediaTimingFunction, CADisplayLink/CAFrameRateRange pacing, CIFilter color controls, ' +
                            'NSHapticFeedbackManager, NSVisualEffectView, reduce-motion accessibility \u2014 timing policy composes the ' +
                            "kernel motion rows, layer mechanics stay host-bound. Session scope and event attachment compose gh-runtime's " +
                            'GhSession/UiEvents vocabulary.',
                        after: ['gh-runtime'],
                    },
                ],
                packageDeltas: [],
            },
            done: {
                'gh-runtime': {
                    files: [
                        'libs/csharp/Rasm.Grasshopper/.planning/Shell/session.md',
                        'libs/csharp/Rasm.Grasshopper/.planning/Shell/events.md',
                        'libs/csharp/Rasm.Grasshopper/.planning/Eto/runtime.md',
                        'libs/csharp/Rasm.Grasshopper/.planning/Eto/binding.md',
                        'libs/csharp/Rasm/.planning/Parametric/projections.md',
                        '.claude/scratch/convert-host/grasshopper-gh-runtime-seams.md',
                    ],
                    verdict: 'authored',
                    summary:
                        'Ground-up authored the gh-runtime foundation unit: four Rasm.Grasshopper planning pages (session spine, event ' +
                        'algebra, Eto runtime floor, binding rail) plus the additive kernel [03]-[MOTION] expand-form extension. Shell/' +
                        'session.md collapses the census GrasshopperUi roster into one GhSession operator \u2014 ScopeTarget ' +
                        'acquisition rows draining into a GhScope union, a six-case SessionOp command union settled by one Apply gate ' +
                        'returning IValidityEvidence receipts, one generic Run<TOut> projection gate holding acquisition and work in a ' +
                        'single EtoDispatch marshal, RepaintRow policy rows over Canvas.Invalidate/ScheduleRedraw, teardown as one ' +
                        'ReleaseCase over Lease<Form>, and BoundedCache killed in favor of SessionCache over the HybridCache substrate ' +
                        'with gh-doc tags and document-close eviction. Shell/events.md collapses the thirteen provider-oriented ' +
                        'UiEvent cases and per-family handler products into one UiSource [SmartEnum<string>] row vocabulary (40+ ' +
                        'catalog-verified GH2 canvas/document/object-list/solution/undo + Eto control/window/app/keyboard/timer rows ' +
                        'built by eleven factory folds over one Attach column), one UiFact payload union (18 evidence cases over six ' +
                        'signal vocabularies), one EventAnchor union, and one transactional Observe gate returning ' +
                        'Fin<Lease<UiSubscription>> \u2014 lifecycle rides the kernel lease, no second algebra; AppKit monitor rows ' +
                        'enter through the Platform/native.md gated adapter seam. Eto/runtime.md mints EtoDispatch (Run/RunAsync/Post/' +
                        'Pump over Application.Instance with on-thread short-circuit and the Op.Catch funnel), DispatchLane rows, ' +
                        'UiClock (UiCadence value object, ClockBeat evidence, FaultPosture policy, Lease-owned UITimer), the Transfer ' +
                        'algebra (TransferSurface collapsing Clipboard+DataObject, TransferPayload/PayloadShape families, five-verb ' +
                        'TransferOp behind one Apply gate including DoDragDrop), and the Display/InputState/NoticeSurface host-fact ' +
                        'projections. Eto/binding.md is pure HOST/CONSUMER buildout over the api-eto-binding catalog: BindMode rows ' +
                        'over DualBindingMode, ModelLens union, ValueGate Fin-gated Convert admission with GatePolicy refusal postures ' +
                        'and CatchException fault re-landing, BindingRail.Fuse/FuseGated onto DualBinding receipts, marshalled ' +
                        'DataScope.Assign, and StoreRail over IDataStore carriers. The kernel extension adds Easing (28 rows generated ' +
                        'by 3 polarity folds x 9 family kernels), CyclePlan/CyclePhase repeat-yoyo arithmetic with terminal clamp, ' +
                        'SpringShape/SpringState analytic three-regime damped-spring plus the one spring IntegrationModule with Step ' +
                        'composing FieldIntegrator, and PerceptualBlend (5 Oklab/Oklch HueSpan rows over Unicolour Mix/Palette) \u2014 ' +
                        'MotionInterpolation untouched, lead/index/diagram/density-bar/research updated in place. All twelve charter ' +
                        'phantoms silently absent; seam ledger written.',
                    deltas: [
                        {
                            symbol: 'GhSession.Apply(SessionOp, Op?) -> Fin<SessionReceipt>',
                            change:
                                'minted \u2014 the one host-session command gate; census GrasshopperUi/GhUi/GrasshopperUiIntent/' +
                                'GrasshopperUiPolicy roster absorbed as SessionOp cases (Reveal/Execute/Repaint/Style/Focus/Release)',
                        },
                        {
                            symbol: 'GhSession.Run<TOut>(ScopeTarget, Func<GhScope, Fin<TOut>>, Op?) -> Fin<TOut>',
                            change: 'minted \u2014 the one value-projection gate; scope acquisition + consumer projection in a single EtoDispatch marshal window',
                        },
                        {
                            symbol: 'ScopeTarget / GhScope',
                            change:
                                'minted \u2014 3 acquisition rows (EditorHost/CanvasHost/DocumentHost over Editor.Instance -> Editor.Canvas -> ' +
                                'Canvas.Document, every hop MissingContext-gated) draining into the 3-case GhScope union with Option-projected ' +
                                'chain accessors',
                        },
                        {
                            symbol: 'RepaintRow',
                            change:
                                'minted \u2014 [SmartEnum<int>] Immediate/Scheduled/Deferred over Canvas.Invalidate / ScheduleRedraw() / ' +
                                'ScheduleRedraw(TimeSpan); replaces census RepaintRequest',
                        },
                        {
                            symbol: 'SessionOp.ReleaseCase(Lease<Form>)',
                            change: 'minted \u2014 the one teardown spelling (Close inside the lease window, disposal by the Owned fold); replaces census Teardown union',
                        },
                        {
                            symbol: 'SessionCache',
                            change:
                                'minted \u2014 HybridCache profile with interpolated-handler gh:{doc:N}:{name} keys, gh-doc document tags, ' +
                                'Evict(documentId) via RemoveByTagAsync; census BoundedCache killed',
                        },
                        {
                            symbol: 'UiSource',
                            change:
                                'minted \u2014 [SmartEnum<string>] source-row vocabulary, 40+ rows over one Attach(EventAnchor, ' +
                                'Action<UiEvent>, Op) -> Fin<IDisposable> column, 11 per-anchor factory folds (Canvas/Document/Graph/Solution/' +
                                'History/Control/Window/Ambient + Pointer/Keystroke/Drag/Stage sub-folds); replaces the 13-case census UiEvent ' +
                                'provider union',
                        },
                        {
                            symbol: 'UiFact / UiEvent',
                            change:
                                'minted \u2014 one 18-case payload union over six signal vocabularies (CanvasSignal 6, DocumentSignal 3, ' +
                                'GraphSignal 3, SolutionSignal 6 in lifecycle order, UndoSignal 2, LifecycleStage 10) plus the stamped UiEvent ' +
                                'envelope; per-family handler products and sparse snapshot records deleted',
                        },
                        {
                            symbol: 'EventAnchor',
                            change:
                                'minted \u2014 8-case attachment-target union (Canvas/Document/Solution/History/Control/Window/Ambient/Clock); ' +
                                'anchor agreement is admission at the attach column',
                        },
                        {
                            symbol: 'UiEvents.Observe(anchor, publish, Op?, params UiSource[]) -> Fin<Lease<UiSubscription>>',
                            change:
                                'minted \u2014 transactional multi-row attach in one marshal with rollback on any refused row; subscription ' +
                                'lifecycle = kernel Lease<T>, census Subscription owner absorbed',
                        },
                        {
                            symbol: 'EtoDispatch',
                            change:
                                'minted \u2014 Run<T>/RunAsync<T>/Post/Pump over Application.Instance (Invoke/InvokeAsync/AsyncInvoke/' +
                                'RunIteration) with IsUIThread short-circuit, MissingContext gate, and the Op.Catch exception funnel',
                        },
                        {
                            symbol: 'DispatchLane',
                            change:
                                'minted \u2014 [SmartEnum<int>] Blocking/Queued marshal-policy rows over one Marshal(Func<Fin<Unit>>, Op) ' +
                                'column; carried as data on SessionOp.ExecuteCase',
                        },
                        {
                            symbol: 'UiClock / UiCadence / ClockBeat / FaultPosture',
                            change:
                                'minted \u2014 Lease-owned UITimer lifecycle: validated cadence [ValueObject<double>], typed beat evidence ' +
                                '(Index/Elapsed/Delta, IValidityEvidence), Halt/Continue fault posture, per-tick Op.Catch',
                        },
                        {
                            symbol: 'Transfer.Apply(TransferOp, Op?) -> Fin<TransferResult>',
                            change:
                                'minted \u2014 one gate over TransferSurface (ClipboardCase|PayloadCase collapsing the mirrored accessor ' +
                                'family), TransferPayload/PayloadShape 8-case families (intrinsic + DataFormats-keyed), verbs Write/Read/Probe/' +
                                'Clear/Drag incl. Control.DoDragDrop',
                        },
                        {
                            symbol: 'Display.Resolve / InputState.Snapshot / InputState.LockProbe / NoticeSurface.Post / NoticeSurface.Tray',
                            change:
                                'minted \u2014 DisplayQuery union onto DisplayMetrics evidence (Screen density facts), ambient ' +
                                'PointerSnapshot, SupportedLockKeys-gated lock probe, Notification/TrayIndicator alert seam',
                        },
                        {
                            symbol: 'BindingRail.Fuse / BindingRail.FuseGated -> Fin<BindingReceipt<T>>',
                            change:
                                'minted \u2014 BindDataContext fuse under BindMode rows (DualBindingMode column); FuseGated composes ValueGate ' +
                                'admission through Convert + CatchException with fault atoms; pure HOST/CONSUMER gap buildout, no census ' +
                                'predecessor',
                        },
                        {
                            symbol: 'ModelLens<TValue> / ValueGate<TRaw,TModel> / GatePolicy<TRaw>',
                            change:
                                'minted \u2014 model-side binding family (PropertyBinding via nameof law, Child drill), Fin-gated render/admit ' +
                                'pair for [ValueObject]/[SmartEnum] fields, Hold/Fallback refusal postures',
                        },
                        {
                            symbol: 'DataScope.Assign(IBindable, object, Op?)',
                            change: 'minted \u2014 the one DataContext assignment, marshalled through EtoDispatch per the host UI-thread law',
                        },
                        {
                            symbol: 'StoreRail.Mount over StoreRow<T> / StoreSink / StoreItemLens',
                            change: 'minted \u2014 Eager/Virtual IDataStore carriers, Grid/List/Tree sinks, ItemTextBinding/ItemKeyBinding as mount data',
                        },
                        {
                            symbol: 'Easing (Rasm.Parametric)',
                            change:
                                'kernel expand-form \u2014 [SmartEnum<int>] 28 rows generated by In/Out/InOut polarity folds over 9 family ' +
                                'kernels (Power 2/3/5, Sine, Expo, Circ, Back, Elastic, Bounce) + Linear; Evaluate(UnitInterval) -> double, ' +
                                'unclamped by decision; THE one scalar easing owner',
                        },
                        {
                            symbol: 'CyclePlan / CyclePhase (Rasm.Parametric)',
                            change:
                                'kernel expand-form \u2014 repeat/yoyo phase arithmetic: Of(Option<int>, yoyo) admission, Phase(elapsed, ' +
                                'period, key) -> Fin<CyclePhase> with mirrored-local UnitInterval, parity-preserving terminal clamp on bounded ' +
                                'completion',
                        },
                        {
                            symbol: 'SpringShape / SpringState / SpringShape.Module (Rasm.Parametric)',
                            change:
                                'kernel expand-form \u2014 analytic three-regime damped-spring (underdamped/critical at SqrtEpsilon/' +
                                'overdamped) via Evaluate; driven stepping via Step composing Numerics/integrate FieldIntegrator over the one ' +
                                'consumer-declared IntegrationModule<SpringState, SpringState>',
                        },
                        {
                            symbol: 'PerceptualBlend (Rasm.Parametric)',
                            change:
                                'kernel expand-form \u2014 [SmartEnum<int>] 5 rows with (ColourSpace, HueSpan) columns; Mix -> Fin<Unicolour> ' +
                                'and Ramp -> Fin<Seq<Unicolour>> over Unicolour.Mix/Palette in Oklab/Oklch; host colour conversion stays at ' +
                                'the consuming boundary',
                        },
                        {
                            symbol: 'projections.md lead / [01]-[INDEX] / mermaid / [04]-[DENSITY_BAR] / [05]-[RESEARCH]',
                            change:
                                'updated in place additively \u2014 six-vocabulary lead, four single-owner laws, timing-algebra boundary law ' +
                                'on the [03]-[MOTION] card, rows [06]-[09] on the density bar, new [ONE_TIMING] research entry; ' +
                                'MotionInterpolation and all selector surfaces untouched',
                        },
                    ],
                    deferred: [
                        {
                            files: ['libs/csharp/Rasm.Grasshopper/.planning/Platform/native.md'],
                            claim:
                                'gh-shell-chrome counterpart obligation: publish the gated AppKit local-monitor attachment adapter ' +
                                '(NSEventMask vocabulary, monitor lifetime, macOS gate) that registers native-monitor rows into the Shell/' +
                                'events.md UiSource attach contract (Attach(EventAnchor, Action<UiEvent>, Op) -> Fin<IDisposable>); events.md ' +
                                'carries only the row seam and no AppKit spelling.',
                        },
                        {
                            files: ['libs/csharp/Rasm.Grasshopper/.planning/Shell/events.md'],
                            claim:
                                'RESEARCH resolution: decompile the GH2 event delegate argument types (Canvas.DocumentChanged family, ' +
                                'Document.ModifiedChanged family, ObjectList.ObjectAdded family, the SolutionServer six, History.Undone/' +
                                'NodeMoved) and the Eto event-arg member spellings (MouseEventArgs.Location/Buttons/Modifiers, ' +
                                'KeyEventArgs.Key, TextInputEventArgs.Text); wire lambdas currently assume the EventHandler family \u2014 ' +
                                'resolution also enriches UiFact DocumentCase/GraphCase/SolutionCase with real subject Guids where the args ' +
                                'carry them.',
                        },
                        {
                            files: ['libs/csharp/Rasm.Grasshopper/.planning/Shell/session.md'],
                            claim:
                                'RESEARCH resolution: fix Editor.ShowEditor(System.Boolean, System.String) argument semantics via decompile, ' +
                                'then extend SessionOp.RevealCase to compose it beside EnsureVisible (reveal-with-layout modality).',
                        },
                        {
                            files: ['libs/csharp/Rasm.Grasshopper/.planning/Eto/binding.md'],
                            claim:
                                'RESEARCH resolution: verify DelegateBinding<TObject,TValue> and ObjectBinding constructor spellings, then ' +
                                'land the delegate-lens case on ModelLens<TValue> (closed family, zero gate impact).',
                        },
                        {
                            files: ['libs/csharp/Rasm.Grasshopper/.planning/Eto/windows.md'],
                            claim:
                                'gh-eto counterpart note: Shell/events.md publishes window Closing/Closed as LifeCase evidence only \u2014 the ' +
                                'closing-veto channel (CancelEventArgs.Cancel policy) is WindowSpec territory; windows.md owns the veto row so ' +
                                'no consumer reaches for the raw event.',
                        },
                    ],
                    indexRows: [
                        {
                            doc: 'libs/csharp/Rasm.Grasshopper/README.md',
                            row:
                                '[SHELL] router rows: .planning/Shell/session.md \u2014 GhSession host-session operator (ScopeTarget/GhScope ' +
                                'acquisition, SessionOp command gate, RepaintRow policy, SessionCache over HybridCache); .planning/Shell/' +
                                'events.md \u2014 UiSource/UiFact/EventAnchor one-row event algebra with UiEvents.Observe onto ' +
                                'Lease<UiSubscription>',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Grasshopper/README.md',
                            row:
                                '[ETO] router rows: .planning/Eto/runtime.md \u2014 EtoDispatch/DispatchLane UI-thread marshal, UiClock over ' +
                                'UITimer, Transfer clipboard/drag algebra, Display/InputState/NoticeSurface host facts; .planning/Eto/' +
                                'binding.md \u2014 BindingRail/ModelLens/ValueGate/DataScope/StoreRail two-way data rail',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Grasshopper/README.md',
                            row:
                                '[3]-[SUBSTRATE_PACKAGES] row: Microsoft.Extensions.Caching.Hybrid \u2014 SessionCache recency seam ' +
                                '(document-tagged GetOrCreateAsync/RemoveByTagAsync); catalogue at libs/csharp/.api/api-hybrid-cache.md',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Grasshopper/Rasm.Grasshopper.csproj',
                            row:
                                '<ItemGroup Label="Package References"><PackageReference Include="Microsoft.Extensions.Caching.Hybrid" /></' +
                                'ItemGroup> \u2014 consumed by Shell/session.md SessionCache; version stays central in ' +
                                'Directory.Packages.props (10.7.0 row exists)',
                        },
                        {
                            doc: 'libs/csharp/Rasm/README.md',
                            row:
                                '[03]-[SUBSTRATE_PACKAGES] row: Wacton.Unicolour \u2014 the Parametric/projections.md [03]-[MOTION] ' +
                                'PerceptualBlend rows (Oklab/Oklch Mix/Palette with HueSpan policy); catalogue at libs/csharp/.api/' +
                                'api-unicolour.md; Directory.Packages.props already pins 8.0.0',
                        },
                        {
                            doc: 'libs/csharp/Rasm/Rasm.csproj',
                            row:
                                '<PackageReference Include="Wacton.Unicolour" /> \u2014 kernel PerceptualBlend consumer; central pin exists, ' +
                                'project reference row is the only addition',
                        },
                    ],
                    phantoms: [
                        'Motion.Pacer',
                        'PacerOption',
                        'Spring (census host type \u2014 the kernel SpringShape is a new host-neutral owner, not this phantom)',
                        'Subscription',
                        'RepaintRequest',
                        'Paint.Hook',
                        'CanvasLease',
                        'UiRail',
                        'MotionClock',
                        'MotionVector',
                        'SpringConfig',
                        'SpringHandle<T>',
                    ],
                },
                'gh-components': {
                    files: [
                        'libs/csharp/Rasm.Grasshopper/.planning/Components/component.md',
                        'libs/csharp/Rasm.Grasshopper/.planning/Components/ports.md',
                        'libs/csharp/Rasm.Grasshopper/.planning/Components/data.md',
                        'libs/csharp/Rasm.Grasshopper/.planning/Components/attributes.md',
                        'libs/csharp/Rasm.Grasshopper/.planning/Components/objects.md',
                        '.claude/scratch/convert-host/grasshopper-gh-components-seams.md',
                    ],
                    verdict: 'authored',
                    summary:
                        'Ground-up authored the five gh-components pages of Rasm.Grasshopper/.planning/Components against the census, ' +
                        'the GH2 mining truths, and the folder .api catalogs; every host member transcribed is census- or ' +
                        'mining-verified (compile-proven old-source usage or XML-sidecar catalog), every unverifiable shape is a ' +
                        'tracked RESEARCH item, and the docgen prose gate passes clean on all five. component.md collapses census ' +
                        'ComponentSpec+OutputBinding into one ComponentSpec record whose Execution union discriminates single/batch/' +
                        'aggregate/heterogeneous by step payload shape with admission-proven depth coherence, one SpecComponent : ' +
                        'ModularComponent boundary adapter, and PluginSpec/Catalogue consuming the same rows registration consumes. ' +
                        'ports.md demotes the ~110-member Add* roster to 53 PortRow seed rows with delegate columns, generated side/' +
                        'depth/presence vocabularies over host Side/Access/Requirement, the PinTrim full parameter-property family, ' +
                        'and one accumulating Declare/Realize fold. data.md mints the GhFault 4600-band closed fault family + Hosted ' +
                        'funnel + Severity/Notice diagnostics, the Transfer<T> topology union over Garden algebra, revocable document/' +
                        'plugin-scoped BrokerLedger rows with deterministic precedence (killing the process-global Atom registry), the ' +
                        'Coerce merit-receipted conversion fold with typed CurveShape/SurfaceShape families, and HostUnits context ' +
                        'capture. attributes.md collapses fifteen Phase rows and callback subclasses into the ChromeEvent union + ' +
                        'right-biased ChromeDecision monoid + ChromeReceipt stream, mints the SnapWindow restoration capsule over ' +
                        'CanvasSnapToObjects (census defect fixed), and projects one ChromeDispatch spine through the two ' +
                        'platform-forced attribute bases. objects.md realizes the HOST-gap catalogs as 32 NativeKind rows + the ' +
                        'PersistedValue union + loop/boundary/accumulation vocabularies over the host smart enums, one NativeObject ' +
                        'operation surface (mint/value/assign/retarget/cluster/chain/loop-drive/sample), and the Gh1Import ' +
                        'receipt-bearing boundary row as the sole GH1 surface. Kernel composition stays Rasm.Domain rails (Op keys, ' +
                        'Expected) with geometry ingress routed through Normalization; no session dependence and no sibling-package ' +
                        'reference anywhere. Seam ledger written with six SEAM_CHANGED mint rows; sibling ledgers (gh-runtime, ' +
                        'rhino-document) read \u2014 no file intersection.',
                    deltas: [
                        {
                            symbol: 'ComponentSpec',
                            change:
                                'minted: one declaration record (Identity/IoId/Inputs/Outputs/Execution/Emissions/Lifecycle/Threading/Variable/' +
                                'Bake/Fleeting/Icon/Panel/Chrome) with accumulating Admit() proving side capability, emission range, ' +
                                'execution-depth coherence, and io-id parseability',
                        },
                        {
                            symbol: 'Execution',
                            change:
                                'minted: [Union] Single(Seq<IPear> step) | Batch(Seq<ITwig>) | Aggregate(Seq<ITree>) | Heterogeneous(scope ' +
                                'step) \u2014 input-shape discrimination by step payload; Uniform projects the depth each case demands; ' +
                                'absorbs census OutputBinding read/run/release/empty quartet',
                        },
                        {
                            symbol: 'ProcessScope / ProcessReceipt / Emission',
                            change:
                                'minted: the one Process seam value (typed reads, receipted writes, notices, progress, HostUnits, ' +
                                'Solution.Token cancellation, kernel Op key) and the sealed written/missing-required receipt fleeting rows ' +
                                'persist',
                        },
                        {
                            symbol: 'SpecComponent',
                            change:
                                'minted: abstract ModularComponent adapter \u2014 every host virtual (AddInputs/AddOutputs/Process/' +
                                'Before-Pre-Post-ProcessTree/Can-DoCreate-RemoveParameter/VariableParameterMaintenance/BakeCapable/BakeShapes/' +
                                'CreateAttributes/IconInternal/AppendToInputPanel/Threading) is one dispatch into the declaration',
                        },
                        {
                            symbol: 'PluginSpec / SpecPlugin / Catalogue',
                            change:
                                'minted: registration rows + Plugin projection + Audit/Load/Harvest/OwnerOf over PluginServer lifted onto Fin/' +
                                'Validation; audit consumes the same rows registration consumes',
                        },
                        {
                            symbol: 'PortRow',
                            change:
                                'minted: [SmartEnum<string>] 53 seed rows (census Add* roster demoted to data) with Option<Type> carrier, ' +
                                'PortSides capability, BindInput/BindOutput delegate columns, ForCarrier frozen index; AddTopological row ' +
                                'carries its verified 3-arg shape',
                        },
                        {
                            symbol: 'PinSide / PinAccess / PinPresence / PortSides / PinVisibility',
                            change: 'minted: generated vocabularies over host Side/Access/Requirement with host columns and PinSide.Of reverse projection',
                        },
                        {
                            symbol: 'PinTrim / PinPlan',
                            change:
                                'minted: closed parameter-property union (Vector/Angle/Integer/Curve/Surface \u2014 all eleven verified ' +
                                'Standard properties) applied by one joint pattern; PinPlan the per-pin declaration with Realize ' +
                                'post-registration fold',
                        },
                        {
                            symbol: 'Ports',
                            change:
                                'minted: Declare (adder-type discriminated, Validation-accumulating, side-gated), Realize (ComponentParameters ' +
                                'index walk), DeclareEnum<T>, DeclareHidden (the two verified hidden emissions)',
                        },
                        {
                            symbol: 'GhFault',
                            change:
                                'minted: closed host-boundary fault [Union] : Expected, band 4600-4699 (Text/Absent/Refused/Conversion/Host/' +
                                'Registration/Aggregate) with Semigroup Combine; cancellation excluded by law',
                        },
                        {
                            symbol: 'Hosted / Severity / Notice',
                            change:
                                'minted: the one Try-lift host exception funnel (Func/Action absorbed by argument shape) and the document ' +
                                'diagnostic vocabulary over AddRemark/AddWarning/AddError delegate rows',
                        },
                        {
                            symbol: 'Transfer<T> / Retention / GardenData',
                            change:
                                'minted: topology union Item/OfPear/OfTwig/OfTree with Read (depth-dispatched)/Write (case-dispatched)/AsTree ' +
                                'promotion/Zip (PairWiseOp)/Amend (PearWiseOp)/ConvertTwig folds; census Bridge Channel union absorbed',
                        },
                        {
                            symbol: 'BrokerLedger / BrokerRow / BrokerScope / Enrolment',
                            change:
                                'minted: document/plugin-scoped broker rows with (scope precedence, rank, ordinal) deterministic ordering and ' +
                                'IDisposable revocation \u2014 census process-global Atom broker accumulation killed',
                        },
                        {
                            symbol: 'Coerce / ConversionReceipt / CurveShape / SurfaceShape',
                            change:
                                'minted: one conversion fold (direct -> scoped brokers -> merit-scored ConversionServer) with route+Merit ' +
                                'receipt; typed cast-or-convert families over the verified CurveBroker/SurfaceBroker out-lattices with IsValid/' +
                                'null probes',
                        },
                        {
                            symbol: 'HostUnits',
                            change:
                                'minted: one tolerance/angle/unit-system capture per Process pass with ScalingTo(Rhino.UnitSystem) projection; ' +
                                'kernel Context remains the tolerance policy owner',
                        },
                        {
                            symbol: 'ChromeEvent / ChromeDecision / ChromeState / ChromeReceipt',
                            change:
                                'minted: 10-case interaction union (Layout/Pivot/Paint/Panel/Menu/Pointer/Key/Tooltip/Resize/Cursor + ' +
                                'PointerKind/KeyPhase/ResizeStage), right-biased | merge monoid (Verdict/Redraw/Bounds/Pointer/Tip/Undo), and ' +
                                'the per-interaction receipt row \u2014 census fifteen Phase rows and callback subclasses collapsed',
                        },
                        {
                            symbol: 'SnapWindow / ResizePolicy / ResizeSession',
                            change:
                                'minted: boundary-scoped CanvasSnapToObjects restoration capsule (census global-mutation defect fixed) and the ' +
                                'resize capsule over verified ResizingFrame(RectangleF, SizeF, SizeF, SnappingConstraints, SnappingSettings) ' +
                                'with Begin(PointF, Padding)/Continue/CursorAt',
                        },
                        {
                            symbol: 'ComponentChrome / ChromeDispatch / ChromeHost / ResizableChromeHost / ChromeWiring',
                            change:
                                'minted: one policy record (Respond fold + Option<ResizePolicy>), one dispatch spine with LatestByKind/' +
                                'RedrawCount projections, and the two platform-forced attribute bases (ComponentAttributes, ' +
                                'ResizableAttributes<Component>) both routing through it; Mount selects the base from the policy',
                        },
                        {
                            symbol: 'NativeKind / ObjectFamily / PersistedValue',
                            change:
                                'minted: 32-row object catalog (21 Parameters.Special + ValueList/DataPanel/DataRecorder/TreeViewer + Shout/' +
                                'Listen/Relay + Cluster/Chain/Loop + GH1) with family/host-type/mintable columns and ForHost frozen index; ' +
                                '8-case persistent-value union',
                        },
                        {
                            symbol: 'BreakTiming / StepAction / RepeatBound / AccumulationMode / BoundaryRole',
                            change:
                                'minted: generated vocabularies over LoopContinuation/LoopingAction/LoopRepeats/Accumulation/ClusterBoundary ' +
                                'with host columns, Halts behavior column, and Of reverse projection',
                        },
                        {
                            symbol: 'NativeObject / LoopPolicy / LoopReceipt',
                            change:
                                'minted: one operation surface \u2014 Mint/ValueOf/Assign joint patterns over verified members, Retarget timer ' +
                                'reconciliation, Clustered/Boundary/Disentangle, Chained (ValidateChain -> Validation), Drive (push/' +
                                'continuation/flush loop kernel sealing steps+cyclical+proper receipt), Sample',
                        },
                        {
                            symbol: 'Gh1Import / Gh1Receipt',
                            change:
                                'minted: the one GH1 boundary row \u2014 IGH_Component wraps to GH1InteropComponent on Fin, returning the ' +
                                'component beside a receipt carrying Grasshopper1Id/Name/Xml',
                        },
                        {
                            symbol: 'Eto.Forms.Cursor / Grasshopper2.Doc.IAttributes',
                            change:
                                'wire correction: census-era Eto.Drawing.Cursor(s) respelled to mining-verified Eto.Forms.Cursors.Default; ' +
                                'IAttributes homed at Grasshopper2.Doc per mining, ComponentAttributes/ResizableAttributes/IContextMenuAware/' +
                                'ICursorAwareAttributes kept at Grasshopper2.Doc.Attributes per census',
                        },
                    ],
                    deferred: [
                        {
                            files: [
                                'libs/csharp/Rasm.Grasshopper/.planning/Components/component.md',
                                'libs/csharp/Rasm.Grasshopper/.planning/Components/ports.md',
                                'libs/csharp/Rasm.Grasshopper/.planning/Components/data.md',
                                'libs/csharp/Rasm.Grasshopper/.planning/Components/attributes.md',
                                'libs/csharp/Rasm.Grasshopper/.planning/Components/objects.md',
                            ],
                            claim:
                                'Fence mass sits at 248-331 lines per page against the 400-700 band because the assay decompile rail was ' +
                                'blocked during census (catalog-verified members only); every RESEARCH row (special-object ctors, responder ' +
                                'hook delegate shapes, ModularComponent surface, broker return contracts, unresolved pin carriers, ' +
                                'LoopingIteration ctor) unlocks verified members that deepen the same owners \u2014 run the decompile census ' +
                                'and expand rows/arms in place, never new files.',
                        },
                        {
                            files: ['libs/csharp/Rasm/.planning/Parametric/projections.md', 'libs/csharp/Rasm/.planning/Domain/rails.md'],
                            claim:
                                'Confirm the kernel Op.Of caller-member-name mint signature and the Op? threading convention as composed by ' +
                                'ProcessScope, GardenData, Ports, and NativeObject entries; if the kernel spells the key mint or threading ' +
                                'differently, the call sites here follow the kernel owner, never a local variant.',
                        },
                        {
                            files: ['libs/csharp/Rasm.Grasshopper/.planning/Components/data.md'],
                            claim:
                                'Census-verified host members IDataAccess.TryTransform, Grasshopper2.Extensions.IDataAccess.CoverageOut, and ' +
                                'Grasshopper2.Extensions.ITree.WithPathPrefix are not yet absorbed into GardenData; rule absorption ' +
                                '(transform-aware write, coverage emission, path-prefix retention row) or an explicit kill after their ' +
                                'signatures verify.',
                        },
                        {
                            files: ['.claude/scratch/convert-host/grasshopper-gh-components-seams.md'],
                            claim:
                                'GhFault claims the 4600-4699 code band for the Components sub-domain; sibling units minting their own ' +
                                'host-fault families (gh-runtime, gh-document, gh-canvas) coordinate non-overlapping bands, and a folder-level ' +
                                'fault unification (one Rasm.Grasshopper fault owner) is a law-tail ruling, not raced here.',
                        },
                    ],
                    indexRows: [
                        {
                            doc: 'libs/csharp/Rasm.Grasshopper/ARCHITECTURE.md',
                            row:
                                'Components/Component.cs  # ComponentSpec one declaration: Execution input-shape union, ProcessScope/' +
                                'ProcessReceipt, SpecComponent ModularComponent adapter, PluginSpec/Catalogue registration rows',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Grasshopper/ARCHITECTURE.md',
                            row:
                                'Components/Ports.cs      # PortRow 53-row seed pin catalog, PinSide/PinAccess/PinPresence vocabularies, ' +
                                'PinTrim family, Ports declare/realize fold',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Grasshopper/ARCHITECTURE.md',
                            row:
                                'Components/Data.cs       # GhFault family + Hosted funnel, Transfer<T> topology union over Garden, scoped ' +
                                'BrokerLedger, Coerce merit-receipted conversion, HostUnits capture',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Grasshopper/ARCHITECTURE.md',
                            row:
                                'Components/Attributes.cs # ComponentChrome interaction policy: ChromeEvent union, right-biased ChromeDecision ' +
                                'merge, SnapWindow restoration capsule, ResizeSession, dual attribute-base projection',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Grasshopper/ARCHITECTURE.md',
                            row:
                                'Components/Objects.cs    # NativeKind 32-row object catalog, PersistedValue union, loop/boundary ' +
                                'vocabularies, NativeObject ops, Gh1Import boundary row',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Grasshopper/README.md',
                            row:
                                'Components router card: `.planning/Components/` \u2014 component authoring model (component, ports, data, ' +
                                'attributes, objects); one polymorphic owner per page composing kernel rails + GH2 host only',
                        },
                    ],
                    phantoms: [
                        'GH1 idioms (GH_Component, IGH_*, SolveInstance, RegisterInputParams, GH_ParamAccess, GH_Structure) written ' +
                            'nowhere; GH1 exists only as the Gh1Import boundary row over Grasshopper2.Interop.IGH_Component',
                        'census spelling Eto.Drawing.Cursor / Eto.Drawing.Cursors corrected to mining-verified Eto.Forms.Cursor / Eto.Forms.Cursors.Default',
                        'census-era namespace drift corrected per the mining seam: IDataAccess/InputAdder/OutputAdder/' +
                            'ComponentParameters written as Grasshopper2.Components, CurveBroker/SurfaceBroker as ' +
                            'Grasshopper2.Parameters.Standard, Solution/FleetingCustomData as Grasshopper2.Doc, Nomen as ' +
                            'Grasshopper2.UI.Nomen, IAttributes as Grasshopper2.Doc.IAttributes',
                        'no fence transcribes an unverified member as settled: TryTransform, PresetsWeak assignment, ResizeAction ' +
                            'ctor, SnappingConstraints.CreateFromDocument arity, LoopingIteration ctor, and the None-carrier pin types are ' +
                            'RESEARCH items, not code',
                    ],
                },
                'gh-document': {
                    files: [
                        'libs/csharp/Rasm.Grasshopper/.planning/Document/document.md',
                        'libs/csharp/Rasm.Grasshopper/.planning/Document/graph.md',
                        'libs/csharp/Rasm.Grasshopper/.planning/Document/history.md',
                        'libs/csharp/Rasm.Grasshopper/.planning/Document/solution.md',
                        '.claude/scratch/convert-host/grasshopper-gh-document-seams.md',
                    ],
                    verdict: 'authored',
                    summary:
                        'Ground-up authored all four gh-document pages (Document folder was empty on disk). document.md: DocumentScope ' +
                        'owns lifecycle (DocumentTier mint rows over New{Inert,Inactive,Active}Document), the DocumentGate lifecycle/' +
                        'keyed-state union (Close/Store/Mark/Stash/Forget over KeyedValues shelves), generic marshalled facet Read, ' +
                        'AllDocuments Roster, and THE one graph transaction gate: an 18-case GraphTransact union absorbing the entire ' +
                        'DocumentMethods verb surface (selection sweeps as SelectionSweep rows, clipboard incl. GH1 XML ingest, group/' +
                        'chain/cluster, shape-discriminated delete, drop/snippet, activity/display/colour dress, isolate, migrate, ' +
                        'dependencies), every mutating arm minting one ActionList and sealing through HistoryLedger.Seal, with the ' +
                        'census-flagged observation ABSENCE closed by capturing UiEvents document rows inside the transaction marshal ' +
                        'window as Seq<UiEvent> causal deltas on TransactReceipt (no second delta vocabulary \u2014 UiEvent is the ' +
                        'delta). graph.md: GraphScope.Ask settles a 10-case GraphProbe into a 10-case GraphAnswer over host ObjectList/' +
                        'Connectivity (FlowSide/GraphReach/GraphRoster row families); GraphScope.Mutate settles a 13-case ' +
                        'GraphMutation over Grasshopper2.Parameters.Connections (namespace-corrected), ObjectList transfer/remap/pins/' +
                        'window-select, and SplitWire with WirelessPair Shout/Listen evidence; census-era local traversal/cycle/path/' +
                        'routing is the explicit ruled kill via host absorption. history.md: HistoryLedger owns the one Seal wire both ' +
                        'mutation gates compose (mutation and undo structurally one act), Commit over a 6-case HistoryOp (seal/stride/' +
                        'branch-promote/record-replay/per-object attach/autosave; LedgerStride rows carry both tree stride and record ' +
                        'replay columns), Bank (ActionList.ToRecord), and branch reconciliation Reconcile/Crown receipts over ' +
                        'FindCommonAncestor/FindShortestPath/PromoteChild. solution.md: SolutionControl.Drive over a 7-case ' +
                        'SolutionCommand (launch bridled/unbridled by payload shape, StartWait await, halt, cooperative cancel, ' +
                        'three-verb deferred-expiry protocol), Watch leasing the six-row solution lifecycle family through UiEvents, ' +
                        'and detached evidence receipts RunPulse/RunEvidence/SolutionTrace with monotone-stamp validity. Every host ' +
                        'member is mining-report or .api-catalog verified; unverifiable spellings (VerbNoun mint, ActionList ctor, ' +
                        'Globals facet type, FindNear/SwapSources, flag semantics, phase/culmination vocabularies, path carriers) are ' +
                        'card-level RESEARCH items, never settled fence code. All gates ride Op-keyed Fin rails inside one EtoDispatch/' +
                        'GhSession marshal; receipts implement IValidityEvidence via ValidityClaim.All; unit seam ledger written.',
                    deltas: [
                        {
                            symbol: 'DocumentTier',
                            change: 'minted [SmartEnum<int>] 3 mint rows (Inert/Inactive/Active) over one Mint() delegate column onto Document.New*Document',
                        },
                        {
                            symbol: 'ValueShelf',
                            change:
                                'minted [SmartEnum<int>] keyed-value facet rows over Select(HostDocument)->KeyedValues; Custom row fenced, ' +
                                'Global row held as RESEARCH pending Globals type verification',
                        },
                        {
                            symbol: 'DocumentGate',
                            change:
                                'minted [Union][GenerateUnionOps] 5-case lifecycle command family: CloseCase, StoreCase(IWriter, ' +
                                'FileContents), MarkCase(bool), StashCase(ValueShelf, string, IStorable), ForgetCase(ValueShelf, string)',
                        },
                        {
                            symbol: 'SelectionSweep',
                            change: 'minted [SmartEnum<int>] 3 rows (All/None/Invert) over Sweep(DocumentMethods) \u2014 SelectAll/DeselectAll/InvertSelection as row data',
                        },
                        {
                            symbol: 'GraphTransact',
                            change:
                                'minted [Union][GenerateUnionOps] 18-case transaction vocabulary absorbing the DocumentMethods verb surface; ' +
                                'DeleteCase discriminates selection-vs-explicit on empty payload shape; host discriminants (ClipboardKind, ' +
                                'PasteBehaviour, Colour, OpenColor.Family) ride case payloads at the seam',
                        },
                        {
                            symbol: 'DocumentScope',
                            change:
                                'minted the document operator: Mint/Apply/Read<TOut>/Recall<T>/Roster/Transact gates plus internal ' +
                                'Resolve<TOut>(Option<HostDocument>, Op, body) marshal resolver (supplied graph -> EtoDispatch.Run; absent -> ' +
                                'GhSession.Run(ScopeTarget.DocumentHost)) composed by all sibling page operators',
                        },
                        {
                            symbol: 'TransactReceipt',
                            change:
                                'minted causal-delta receipt (Op, Verb, Sealed, Seq<UiEvent> Deltas, Latency) : IValidityEvidence \u2014 ' +
                                'deltas captured via UiEvents.Observe(GraphObjectAdded/GraphObjectRemoved/GraphSelection/DocumentModified) ' +
                                'inside the transaction marshal window',
                        },
                        {
                            symbol: 'DocumentReceipt',
                            change: 'minted lifecycle settlement receipt (Op, Verb, Latency) : IValidityEvidence',
                        },
                        {
                            symbol: 'FlowSide',
                            change:
                                'minted [SmartEnum<int>] 2 rows (Upstream/Downstream) carrying BOTH Search(ObjectList, IParameter) over ' +
                                'SearchUpstream/SearchDownstream AND Prune(pin, kept, ActionList) over DisconnectAll{Inputs,Outputs}Except',
                        },
                        {
                            symbol: 'GraphReach',
                            change:
                                'minted [SmartEnum<int>] 4 rows over Connectivity FindImmediate{Inputs,Outputs}/FindAll{Inputs,Outputs} via ' +
                                'one Find(Connectivity, ConnectiveObject) column',
                        },
                        {
                            symbol: 'GraphRoster',
                            change:
                                'minted [SmartEnum<int>] 8 projection rows (Forwards/Backwards/ActiveObjects/ExpiredObjects/Groups/AllWires/' +
                                'SelectedWires/GlobalPins) -> GraphAnswer; AttributeBounds/PivotBounds rows deferred to RESEARCH verification',
                        },
                        {
                            symbol: 'GraphProbe',
                            change:
                                'minted [Union][GenerateUnionOps] 10-case read-intent vocabulary (object/parameter identity, flow, reach, ' +
                                'edge, topology, linear, causal, relay-free, roster)',
                        },
                        {
                            symbol: 'GraphAnswer',
                            change: 'minted closed 10-case result union (Option/Seq/Connectivity/linear-witness payloads) \u2014 one answer vocabulary for every graph read',
                        },
                        {
                            symbol: 'WireEndRole',
                            change:
                                'minted [SmartEnum<int>] 2 rows (SourceEnd/TargetEnd) over Replace(anchor, retired, replacement, ActionList) ' +
                                'onto Connections.Replace{Source,Target}',
                        },
                        {
                            symbol: 'WireFreight',
                            change:
                                'minted [SmartEnum<int>] 2 rows (CopyInputs/MigrateOutputs) over Haul(from, to, ActionList) onto ' +
                                'Connections.{CopyAllInputs,MigrateAllOutputs}',
                        },
                        {
                            symbol: 'GraphMutation',
                            change:
                                'minted [Union][GenerateUnionOps] 13-case wire/membership mutation vocabulary: Link/Unlink/Prune(empty-kept = ' +
                                'full clear)/Rewire/Bypass/Transfer/Split/Adopt/Remap(None=ChangeAllIds, Some=ApplyIdMap)/Pin/Repair/Expire/' +
                                'Window',
                        },
                        {
                            symbol: 'GraphScope',
                            change:
                                'minted the graph operator: Ask(GraphProbe)->Fin<GraphAnswer> + Mutate(VerbNoun, ' +
                                'GraphMutation)->Fin<MutationReceipt>; wire writes spell Grasshopper2.Parameters.Connections ' +
                                '(namespace-corrected from census Doc placement)',
                        },
                        {
                            symbol: 'WirelessPair',
                            change:
                                'minted split evidence (Guid Shout, Guid Listen) : IValidityEvidence surfaced on MutationReceipt from ' +
                                'DocumentMethods.SplitWire out-parameters',
                        },
                        {
                            symbol: 'MutationReceipt',
                            change: 'minted sealed-mutation receipt (Op, Verb, Sealed, Option<WirelessPair>, Latency) : IValidityEvidence',
                        },
                        {
                            symbol: 'LedgerStride',
                            change:
                                'minted [SmartEnum<int>] 2 rows (Back/Forward) carrying BOTH Stride(History) over Undo/Redo AND Replay(Record, ' +
                                'HostDocument) over Record.Undo/Redo \u2014 one direction vocabulary for tree and record surfaces',
                        },
                        {
                            symbol: 'HistoryOp',
                            change:
                                'minted [Union][GenerateUnionOps] 6-case undo command family: SealCase(VerbNoun, ActionList), StrideCase, ' +
                                'BranchCase(Node.PromoteChild), ReplayCase, AttachCase(AddUndoRecord), AutoSaveCase(RequestAutoSave)',
                        },
                        {
                            symbol: 'HistoryLedger.Seal',
                            change:
                                'minted the ONE cross-page History.Do spelling: Seal(History, ActionList, VerbNoun, Op) -> Fin<LedgerReceipt>, ' +
                                'composed by DocumentScope.Transact and GraphScope.Mutate sealed arms so mutation and undo are one act ' +
                                'folder-wide',
                        },
                        {
                            symbol: 'HistoryLedger',
                            change:
                                'minted the ledger operator: Commit/Seal/Bank(ActionList.ToRecord)/Reconcile(FindCommonAncestor + ' +
                                'FindShortestPath -> BranchPath)/Crown(PrimaryChild + SecondaryChildren -> BranchCrown)',
                        },
                        {
                            symbol: 'BranchPath / BranchCrown / LedgerReceipt',
                            change: 'minted branch-reconciliation and settlement evidence receipts : IValidityEvidence over the live undo tree (no local tree mirror exists)',
                        },
                        {
                            symbol: 'SolutionCommand',
                            change:
                                'minted [Union][GenerateUnionOps] 7-case execution vocabulary: LaunchCase(SolutionMode, ' +
                                'Option<CancellationTokenSource> bridle discriminating the two Start overloads on payload presence), ' +
                                'AwaitCase(StartWait), HaltCase, CancelCase(Solution.Cancel), DeferCase(DelayedExpire), ' +
                                'FlushCase(ExpireDelayedObjects), ExpireCase(per-object Expire)',
                        },
                        {
                            symbol: 'SolutionControl',
                            change:
                                'minted the execution operator: Drive(SolutionCommand)->Fin<SolutionReceipt>, ' +
                                'Watch(publish)->Fin<Lease<UiSubscription>> composing the six UiSource.Solution* rows on ' +
                                'EventAnchor.SolutionCase, Probe(Solution)->Fin<RunPulse>, Audit(SolutionRecord)->Fin<RunEvidence>, ' +
                                'Trace(Seq<UiEvent>)->SolutionTrace',
                        },
                        {
                            symbol: 'RunPulse / RunEvidence / SolutionTrace',
                            change:
                                'minted detached run evidence: pulse (Id, Phase, InvalidCount), audit (Expired, Solved, UnitInterval Progress, ' +
                                'Culmination), and the pure phase-timeline fold whose IsValid claims monotone stamps',
                        },
                        {
                            symbol: 'HostDocument',
                            change:
                                'minted the folder-wide using alias for Grasshopper2.Doc.Document \u2014 the Rasm.Grasshopper.Document ' +
                                'namespace shadows the simple type name, so every fence in the sub-domain carries the alias',
                        },
                    ],
                    deferred: [
                        {
                            files: ['libs/csharp/Rasm.Grasshopper/.planning/Document/history.md'],
                            claim:
                                'VerbNoun mint spelling (constructor or factory) is catalog-unstated; every gate accepts an already-minted ' +
                                'label \u2014 decompile the Grasshopper2.Undo.VerbNoun construction surface and land the boundary factory on ' +
                                'history.md [02], then consider defaulting Transact/Mutate labels from the GraphTransact/GraphMutation case ' +
                                'SelfOp names',
                        },
                        {
                            files: [
                                'libs/csharp/Rasm.Grasshopper/.planning/Document/document.md',
                                'libs/csharp/Rasm.Grasshopper/.planning/Document/graph.md',
                            ],
                            claim:
                                'ActionList parameterless construction is assumed by the Bind/Sealed helpers in both mutation gates \u2014 ' +
                                're-verify at decompile and swap the mint spelling if the host supplies a factory',
                        },
                        {
                            files: ['libs/csharp/Rasm.Grasshopper/.planning/Document/document.md'],
                            claim:
                                'Document.Globals facet member shape unverified \u2014 land the Global ValueShelf row against KeyedValues (or ' +
                                'its real type) after decompile; Document.State vocabulary and AllDocuments element carrier re-verify in the ' +
                                'same pass',
                        },
                        {
                            files: ['libs/csharp/Rasm.Grasshopper/.planning/Document/graph.md'],
                            claim:
                                'Catalog-unstated return carriers asserted as enumerables in fences (SortCausally, WithoutRelays, ' +
                                'SubsetTopology, FindConnections, SearchUpstream/Downstream, roster properties) plus census-sighted but ' +
                                'unverified members (ObjectList.FindNear/FindByInlet/FindByOutlet, Connections.SwapSources, AttributeBounds/' +
                                'PivotBounds) \u2014 one decompile pass confirms or corrects; near/inlet/outlet probe cases, a swap case, and ' +
                                'the bounds roster rows land on confirmation',
                        },
                        {
                            files: ['libs/csharp/Rasm.Grasshopper/.planning/Document/solution.md'],
                            claim:
                                'Solution.Phase and SolutionRecord.Culmination vocabulary types unverified \u2014 receipts carry rendered ' +
                                'names; each becomes one typed field swap when the decompile fixes the enums',
                        },
                        {
                            files: ['libs/csharp/Rasm/.planning/Domain/identity.md'],
                            claim:
                                "Document content identity: Document.Hash's type is host-unstated; if a Rasm-side stable document fingerprint " +
                                'is ever needed beyond the host hash, it composes kernel ContentHash.Of over the Store byte stream \u2014 ' +
                                'kernel extension not raced here because the host member may already suffice',
                        },
                    ],
                    indexRows: [
                        {
                            doc: 'libs/csharp/Rasm.Grasshopper/README.md',
                            row:
                                'Document sub-domain router rows: Document/document.md (DocumentScope \u2014 lifecycle/IO/keyed state + the ' +
                                'one GraphTransact transaction gate with causal UiEvent deltas), Document/graph.md (GraphScope \u2014 ' +
                                'GraphProbe/GraphAnswer reads over ObjectList/Connectivity, GraphMutation wire/membership writes over ' +
                                'Parameters.Connections with sealed undo), Document/history.md (HistoryLedger \u2014 HistoryOp commits, the ' +
                                'one Seal wire, branch Reconcile/Crown), Document/solution.md (SolutionControl \u2014 SolutionCommand ' +
                                'execution, six-row lifecycle Watch, RunPulse/RunEvidence/SolutionTrace)',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Grasshopper/ARCHITECTURE.md',
                            row:
                                'Seams: Document/document.md \u2190 Shell/session.md # [BOUNDARY]: GhSession.Run(ScopeTarget.DocumentHost) ' +
                                'marshal resolution; Document/document.md + Document/solution.md \u2190 Shell/events.md # [WIRE]: ' +
                                'UiEvents.Observe rows feed TransactReceipt.Deltas and SolutionControl.Watch; Document/document.md + Document/' +
                                'graph.md \u2192 Document/history.md # [WIRE]: HistoryLedger.Seal(History, ActionList, VerbNoun, Op) is the ' +
                                'one mutation-undo seal',
                        },
                    ],
                    phantoms: [
                        'Census-era local graph algorithms (WireTraversal reachability walks, cycle detection, bounded path ' +
                            'enumeration, WireRouteSolver) \u2014 killed by host Connectivity/ObjectList absorption per the campaign ' +
                            'ruling, no successor shape authored',
                        'Census Grasshopper2.Doc.Connections spelling \u2014 corrected to Grasshopper2.Parameters.Connections per the ' +
                            'mining namespace seam; the Doc-namespace form never entered a fence',
                        'Census reflection bridges (WireRepositoryRail simple-name type discovery, WireDrawCache property reflection) ' +
                            '\u2014 silently dropped as phantom-class access patterns; every fenced member is mining/catalog-verified',
                        'Census DisconnectAllInputs/DisconnectAllOutputs bare forms \u2014 unverified against the catalog; covered by ' +
                            'the empty-kept PruneCase shape instead of being fenced',
                    ],
                },
                'gh-shell-chrome': {
                    files: [
                        'libs/csharp/Rasm.Grasshopper/.planning/Shell/editor.md',
                        'libs/csharp/Rasm.Grasshopper/.planning/Shell/chrome.md',
                        'libs/csharp/Rasm.Grasshopper/.planning/Shell/icons.md',
                        'libs/csharp/Rasm.Grasshopper/.planning/Platform/native.md',
                        'libs/csharp/Rasm.Grasshopper/.planning/Platform/composition.md',
                        '.claude/scratch/convert-host/grasshopper-gh-shell-chrome-seams.md',
                    ],
                    verdict: 'authored',
                    summary:
                        'Ground-up authored all five gh-shell-chrome pages against the current disk state of the landed gh-runtime ' +
                        'siblings (session/events/runtime composed as found, reveal ceded to SessionOp.RevealCase) and the kernel ' +
                        '[03]-[MOTION] rows. editor.md collapses the census EditorOp four-provider roster into EditorShell (ShellSlot ' +
                        'pane rows + ShellToggle axes + ShellState receipt + BeginRhinoGetter case; zero singleton interrogation at ' +
                        'consumers). chrome.md builds ONE Chrome.Apply intent gate over BarItemSpec/PanelControl/TooltipContent/' +
                        'FloatSpec families with the census Tooltip.Layout reflection path deleted as a phantom. icons.md closes the ' +
                        'CONSUMER gap with IconOwner admission-with-diagnostics, a frozen IconCatalog, PoseShift/IconFilter/IconRender ' +
                        'families, and PerceptualBlend-composed tint math. native.md fixes the census missing-platform-admission ' +
                        'defect: MacGate.Demand gates every AppKit row with typed Fault.Unsupported, MacAnchor extraction, leased ' +
                        'NativeMonitor/GestureBinding/WorkspaceWatch, 13-axis NativeInput evidence. composition.md resolves the macOS ' +
                        'half of the Motion blocker: CATransaction-fenced LayerNode graphs, FramePacer over CADisplayLink reusing ' +
                        'ClockBeat, MotionDrive sampling kernel Easing/CyclePlan/SpringShape/PerceptualBlend (the census 46-row easing ' +
                        'table, spring solver, and OKLab conversion have no successor), admitted GlidePlan attachment, filter/haptic/' +
                        'vibrancy/DisplayP3 rows. Every unverified host member is a card-declared RESEARCH item, never settled fence ' +
                        'code; unit seam ledger written.',
                    deltas: [
                        {
                            symbol: 'EditorShell.Apply(ShellOp, Op?) -> Fin<ShellReceipt>',
                            change:
                                'minted \u2014 the one editor-shell command gate (ToggleCase, GetterCase over Editor.BeginRhinoGetter); ' +
                                'replaces census EditorOp Show/Shell/State/BeginRhinoGetter provider cases',
                        },
                        {
                            symbol: 'EditorShell.Snapshot() -> Fin<ShellState>',
                            change:
                                'minted \u2014 one projected shell receipt (Collapsed/NotesShown/UndoHistoryShown/HasDocument/RecentCount); ' +
                                'consumers never interrogate the Editor singleton',
                        },
                        {
                            symbol: 'EditorShell.Grab<TPane, TOut>(ShellSlot, Func<TPane, Fin<TOut>>, Op?)',
                            change:
                                'minted \u2014 one typed pane-projection gate over ShellSlot [SmartEnum<int>] 8 rows (Tabs/BreadCrumbs/' +
                                'StatusBar/DefinedLayouts/InitialLayout/RecentActive/RecentLoaded/HostAnchor); pane member types held as ' +
                                'RESEARCH behind the generic bind',
                        },
                        {
                            symbol: 'ShellToggle [SmartEnum<int>]',
                            change:
                                'minted \u2014 3 dual-direction rows (Collapsed/Notes on Editor, UndoHistory on Canvas.ShowUndoHistory) over ' +
                                'one Swing(GhScope, Option<bool>, Op) column',
                        },
                        {
                            symbol: 'Chrome.Apply(ChromeIntent, Op?) -> Fin<ChromeReceipt>',
                            change:
                                'minted \u2014 ONE chrome settlement gate; ChromeIntent cases BarCase/BarPassCase/BarMutateCase/ColourBarsCase/' +
                                'PanelCase/TipCase/FloatCase pair host target with verb family',
                        },
                        {
                            symbol: 'BarItemSpec / BarMutation / BarPass',
                            change:
                                'minted \u2014 toolbar item rows (Push/Radio/Field over AddPushButton/AddRadioToggle/AddTextField), live-item ' +
                                'mutations (SetState/Toggle/SetText), bar passes (Layout/Render(Context)/ShowTooltipAt)',
                        },
                        {
                            symbol: 'PanelControl / PanelPlan / PanelOp',
                            change:
                                'minted \u2014 category-grouped input-panel rows (Label/Check/Text/Bar/Host) plus [GenerateUnionOps] verbs ' +
                                '(Build/Move/Rename/Remove/Find/Float over BeginCategory..ShowAsForm); census FormField roster killed',
                        },
                        {
                            symbol: 'TooltipContent / TooltipIntent / Painters',
                            change:
                                'minted \u2014 Plain/Items/Painter cases over the three Frame.Show overloads plus CreateShortcutPainter/' +
                                'CreateTextAndIconPainter factory rows; census Tooltip.Layout reflection path deleted as phantom',
                        },
                        {
                            symbol: 'FloatSpec / FloatAnchor / FloatMutation / FloatOp / FloatLayoutPass',
                            change:
                                'minted \u2014 Grasshopper2.UI.Flex floating-button family (namespace corrected from Canvas): Add/AddAnchored, ' +
                                'Show/Hide/Close, ModifyInfo/Icon/Colour/Anchor, FindByName/FindByPoint probe, MakeNumeric bind, PerformLayout/' +
                                'PerformAnchoredLayout/PerformOcclusionLogic pass rows',
                        },
                        {
                            symbol: 'IconOwner.Mint(IconSource, Op?) -> Fin<IconHandle>',
                            change:
                                'minted \u2014 FromResource/FromCode admission with CodeDiagnostic capture into IconDiagnostics evidence; ' +
                                'errors refuse typed, warnings ride the handle',
                        },
                        {
                            symbol: 'IconCatalog.Freeze(...(string, IconSource) rows) -> Fin<IconCatalog>',
                            change: 'minted \u2014 frozen per-plugin icon registry, total mint and unique keys at freeze; the CONSUMER-gap owner every future plugin composes',
                        },
                        {
                            symbol: 'PoseShift / IconFilter / IconRender',
                            change:
                                'minted \u2014 Jump/Glide over SetState/MoveState(Duration?, Motion?) with FindState gate; Disabled/Greyscale/' +
                                'Fading(Color,float) chain fold over IconContext; Surface/Raster render gate returning Fin<Option<Bitmap>>',
                        },
                        {
                            symbol: 'MacGate.Demand(Op?) -> Fin<Unit>',
                            change:
                                'minted \u2014 the ONE platform-admission policy (OperatingSystem.IsMacOS, refusal is typed ' +
                                'Fault.Unsupported); fixes the census-flagged ungated AppKit paths',
                        },
                        {
                            symbol: 'MacAnchor.Of(AnchorSource, Op?) -> Fin<MacAnchor>',
                            change: 'minted \u2014 Canvas.ControlObject / Control.Handler-as-IMacViewHandler NSView extraction, null-lowered, marshalled, non-escaping',
                        },
                        {
                            symbol: 'NativeSeam.Observe(MonitorPlan, Op?) -> Fin<Lease<NativeMonitor>>',
                            change:
                                'minted \u2014 NSEvent local monitors with 13-axis NativeInput evidence (phase/momentum/scroll/magnification/' +
                                'rotation/pressure/stage/modifiers) and swallow policy as plan data; the attach delegate Shell/events.md ' +
                                'native rows compose',
                        },
                        {
                            symbol: 'NativeSeam.Gesture/Pressure/Convert/Accessibility/Watch/Pace',
                            change:
                                'minted \u2014 leased GestureBinding, PressureConfiguration assignment, ConvertPointFromView projection, ' +
                                'AccessibilityPosture (reduce-motion/transparency/colour), WorkspaceWatch over ' +
                                'ObserveDidChangeScreenParameters, PaceBounds off NSScreen ceilings',
                        },
                        {
                            symbol: 'Compose.Mount/Mutate/Unmount',
                            change:
                                'minted \u2014 recursive LayerNode (Plain=CALayer, Shape=CAShapeLayer, Hosted=admitted gradient/text/' +
                                'replicator/emitter) built and mutated inside one CATransaction fence with DisableActions and CompletionBlock',
                        },
                        {
                            symbol: 'FramePacer.Of(MacAnchor, PaceWindow, Func<ClockBeat, Fin<Unit>>, Op?) -> Fin<Lease<FramePacer>>',
                            change:
                                'minted \u2014 CADisplayLink vsync pacer via GetDisplayLink + Export selector target, CAFrameRateRange clamped ' +
                                'against PaceBounds; REUSES Eto/runtime ClockBeat so UiClock and FramePacer are swappable pacers with one beat ' +
                                'contract',
                        },
                        {
                            symbol: 'MotionDrive.Step(DriveSpec, ClockBeat, AccessibilityPosture, Op?) -> Fin<bool>',
                            change:
                                'minted \u2014 kernel-sampled drive fold: EasedCase (CyclePlan.Phase -> Easing.Evaluate), SprungCase ' +
                                '(SpringShape.Evaluate closed form + settle tolerance), BlendCase (PerceptualBlend.Mix over eased cycle); ' +
                                'ReduceMotion degrades to terminal pose by policy; census 46-row Easing/SpringRunnerState/MotionVector have NO ' +
                                'successor',
                        },
                        {
                            symbol: 'Glides / Curves / Effects / WideColor',
                            change:
                                'minted \u2014 admitted GlidePlan AddAnimation/RemoveAnimation attachment, CAMediaTimingFunction.FromName ' +
                                'mint, FilterKind [SmartEnum<string>] CIFilter rows, HapticCue value over DefaultPerformer, VibrancyPane -> ' +
                                'NSVisualEffectView, NSColor.FromDisplayP3 projection of PerceptualBlend outputs',
                        },
                    ],
                    deferred: [
                        {
                            files: ['libs/csharp/Rasm.Grasshopper/.planning/Shell/events.md'],
                            claim:
                                "gh-runtime's UiSource vocabulary needs the chrome and native rows this unit's owners emit: Bar.Invalidated, " +
                                'TextField.TextChanged, FloatingButton AnchorChanged/ColourChanged/StateChanged/ValueChanged as chrome source ' +
                                'rows, plus the native-monitor row family whose attach column delegates to Platform/native.md ' +
                                "NativeSeam.Observe (the events page's own boundary law already names this registration; the rows themselves " +
                                'are not yet declared)',
                        },
                        {
                            files: ['libs/csharp/Rasm.Grasshopper/.planning/Platform/composition.md'],
                            claim:
                                'assay decompile confirmation over Microsoft.macOS for three catalog-implied members fenced as settled: ' +
                                'NSHapticFeedbackPerformer.PerformFeedback(pattern, performanceTime), NSVisualEffectView.Material/BlendingMode ' +
                                'property spellings, and CADisplayLink.PreferredFrameRateRange setter shape \u2014 each exists only as the ' +
                                "catalog's enum/struct rows imply; a divergence is a one-line fence fix",
                        },
                        {
                            files: [
                                'libs/csharp/Rasm.Grasshopper/.planning/Shell/editor.md',
                                'libs/csharp/Rasm.Grasshopper/.planning/Shell/chrome.md',
                            ],
                            claim:
                                'decompile pass to resolve the RESEARCH ledger both pages carry: Editor pane types (Tabs/BreadCrumbs/StatusBar/' +
                                'DefinedLayouts/InitialLayout), recent-row element type, ShowEditor(bool,string) semantics, file-compare ' +
                                'members, Bar/InputPanel/Frame instance acquisition, Add* return types, BarItem mint, Frame.Show trailing ' +
                                'booleans, FloatingButtonLayout acquisition, float handler-triple semantics, *Ux channel payload types \u2014 ' +
                                'each resolution is a row/field addition with zero gate changes',
                        },
                        {
                            files: ['libs/csharp/Rasm.Grasshopper/.planning/Canvas/motion.md'],
                            claim:
                                "gh-canvas's MotionAdapter and this unit's FramePacer are the two pacing tiers (GH2 Animated<T>/IFlexControl " +
                                "vs CADisplayLink); the canvas page's adapter card carries the consumer-selects-one law from its side \u2014 " +
                                'verify no second beat-evidence record gets minted there (ClockBeat is the shared contract)',
                        },
                    ],
                    indexRows: [
                        {
                            doc: 'libs/csharp/Rasm.Grasshopper/README.md',
                            row:
                                'Shell router: editor.md \u2014 EditorShell over the GH2 Editor singleton (ShellSlot panes, ShellToggle axes, ' +
                                'ShellState receipt, BeginRhinoGetter handoff); chrome.md \u2014 Chrome.Apply one intent gate over Toolbar.Bar/' +
                                'InputPanel/Tooltip.Frame/Flex floating buttons; icons.md \u2014 IconOwner + IconCatalog stateful vector-icon ' +
                                'family (FromResource/FromCode diagnostics, MoveState poses, filter chain, raster gate)',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Grasshopper/README.md',
                            row:
                                'Platform router: native.md \u2014 MacGate/MacAnchor/NativeSeam gated AppKit seam (local monitors, gestures, ' +
                                'pressure, workspace observation, pacing bounds); composition.md \u2014 Compose/FramePacer/MotionDrive/Glides/' +
                                'Effects CoreAnimation layer graphs, vsync pacing, kernel-driven motion, filters/haptics/vibrancy/P3',
                        },
                        {
                            doc: 'libs/csharp/Rasm.Grasshopper/ARCHITECTURE.md',
                            row:
                                'codemap: Shell/Editor.cs, Shell/Chrome.cs, Shell/Icons.cs, Platform/Native.cs, Platform/Composition.cs; ' +
                                'seams: Platform/Composition.cs \u2190 csharp:Rasm/Parametric/Projections # [SHAPE]: ' +
                                'Easing\u00b7CyclePlan\u00b7SpringShape\u00b7PerceptualBlend kernel motion rows sampled per beat; Shell/' +
                                'Icons.cs \u2192 Shell/Session.cs # [RECEIPT]: icon rasters keyed into SessionCache document tags',
                        },
                    ],
                    phantoms: [
                        'Grasshopper2.UI.Tooltip.Layout \u2014 census reflection target absent from the installed catalog; deleted silently, no successor surface',
                        'Grasshopper2.UI.Flex.Pacer / PacerOption / Subscription / RepaintRequest and Paint.Hook \u2014 census ' +
                            'Motion.cs members the gh2ui mining marks as an absent motion/subscription vocabulary in the installed XML ' +
                            'catalog; deleted, pacing lands on FramePacer/UiClock and repaint on IFlexControl.ScheduleRedraw per the flex ' +
                            'catalog',
                        'GH1 idioms (GH_DocumentEditor/ToolStrip chrome) \u2014 absent by construction; no page carries them',
                    ],
                },
            },
        },
    ],
    reports: [
        '.claude/scratch/convert-host/census-gh-a-report.json',
        '.claude/scratch/convert-host/census-gh-b-report.json',
        '.claude/scratch/convert-host/census-gh-c-report.json',
        '.claude/scratch/convert-host/census-gh-d-report.json',
        '.claude/scratch/convert-host/census-rhino-a-report.json',
        '.claude/scratch/convert-host/census-rhino-b-report.json',
        '.claude/scratch/convert-host/census-rhino-c-report.json',
        '.claude/scratch/convert-host/census-rhino-d-report.json',
        '.claude/scratch/convert-host/kernel-core-report.json',
        '.claude/scratch/convert-host/kernel-motion-report.json',
        '.claude/scratch/convert-host/kernel-visual-report.json',
        '.claude/scratch/convert-host/mining-gh-etogh-report.json',
        '.claude/scratch/convert-host/mining-gh-gh2-report.json',
        '.claude/scratch/convert-host/mining-gh-gh2ui-report.json',
        '.claude/scratch/convert-host/mining-rhino-eto-report.json',
        '.claude/scratch/convert-host/mining-rhino-rcdoc-report.json',
        '.claude/scratch/convert-host/mining-rhino-rcvis-report.json',
    ],
    kernelReports: [
        '.claude/scratch/convert-host/kernel-core-report.json',
        '.claude/scratch/convert-host/kernel-motion-report.json',
        '.claude/scratch/convert-host/kernel-visual-report.json',
    ],
};

// --- [INPUTS] ----------------------------------------------------------------------------

const argsIn = typeof args === 'string' && /^\s*[\[{]/.test(args) ? JSON.parse(args) : args;
const wanted = Array.isArray(argsIn)
    ? argsIn
    : argsIn && typeof argsIn === 'object' && Array.isArray(argsIn.targets)
      ? argsIn.targets
      : typeof argsIn === 'string' && argsIn.trim()
        ? [argsIn.trim()]
        : null;
const ACTIVE = wanted ? FOLDERS.filter((f) => wanted.some((w) => String(w).indexOf(f.name) >= 0 || String(w).indexOf(f.key) >= 0)) : FOLDERS;

// --- [MODELS] ----------------------------------------------------------------------------

// One anchor = one fact at one coordinate; interpretation never lives in an anchor row.
const anchorOf = (roles) => ({
    type: 'object',
    additionalProperties: false,
    required: ['path', 'line', 'role', 'note'],
    properties: {
        path: { type: 'string' },
        line: { type: 'integer' },
        role: { type: 'string', enum: roles },
        note: { type: 'string' },
    },
});
const COVERAGE = {
    type: 'object',
    additionalProperties: false,
    required: ['requested', 'read', 'skipped', 'unverified'],
    properties: {
        requested: { type: 'array', items: { type: 'string' } },
        read: { type: 'array', items: { type: 'string' } },
        skipped: { type: 'array', items: { type: 'string' } },
        unverified: { type: 'array', items: { type: 'string' } },
    },
};
// Recon/inventory product (research + kernel recon lanes): facts with anchors, never prescriptions.
const MAP = {
    type: 'object',
    additionalProperties: false,
    required: ['entries', 'coverage', 'summary'],
    properties: {
        entries: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['target', 'kind', 'files', 'info', 'anchors', 'members'],
                properties: {
                    target: { type: 'string' }, // catalog, kernel owner, capability cluster, or seam the entry grounds
                    kind: { type: 'string', enum: ['owner', 'catalog', 'capability', 'gap', 'seam'] },
                    files: { type: 'array', items: { type: 'string' } }, // files the reader must open for this entry
                    info: { type: 'string' }, // the fact: current shape, extension site, seam endpoints — prose truth, zero prescriptions
                    anchors: { type: 'array', items: anchorOf(['state', 'ruling', 'catalog', 'counterpart', 'absence']) },
                    members: { type: 'array', items: { type: 'string' } },
                },
            },
        }, // verified member spellings backing the entry
        coverage: COVERAGE,
        summary: { type: 'string' },
    },
};
// Defect-shaped product (census register + acceptance audit): inventory fields + constraint-boundary fields per entry.
const REGISTER = {
    type: 'object',
    additionalProperties: false,
    required: ['entries', 'coverage', 'summary'],
    properties: {
        entries: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: [
                    'claimKey',
                    'target',
                    'kind',
                    'files',
                    'info',
                    'members',
                    'severity',
                    'claim',
                    'mechanism',
                    'owner',
                    'reject',
                    'acceptance',
                    'anchors',
                ],
                properties: {
                    claimKey: { type: 'string' }, // <kind>|<owner>|<primary symbol or absence route> — stable across lanes, never lane wording
                    target: { type: 'string' }, // the .cs file (census), csproj (manifest), or miss label (acceptance)
                    kind: { type: 'string', enum: ['register', 'manifest', 'symbol', 'disposition', 'boundary', 'kernel', 'law'] },
                    files: { type: 'array', items: { type: 'string' } }, // files the reader must open or edit first
                    info: { type: 'string' }, // inventory prose: owned capability + host-agnostic vs host-bound split — facts only
                    members: { type: 'array', items: { type: 'string' } }, // exact host members composed, verified spellings
                    severity: { type: 'string', enum: ['blocker', 'major', 'minor'] }, // bound to consequence, never prose confidence
                    claim: { type: 'string' }, // the observed fact or defect (census: quality verdict, naivety axes named)
                    mechanism: { type: 'string' }, // WHY the current form fails — factual, zero repair verbs
                    owner: { type: 'string' }, // canonical absorber (census: the collapse-signal polymorphic owner)
                    reject: { type: 'array', items: { type: 'string' } }, // deleted forms (census: what dies as pattern)
                    acceptance: { type: 'array', items: { type: 'string' } }, // proof signals (census: what survives as concept)
                    anchors: { type: 'array', items: anchorOf(['defect', 'ruling', 'catalog', 'counterpart', 'absence']) },
                },
            },
        },
        coverage: COVERAGE,
        summary: { type: 'string' },
    },
};
// Thin wire receipt: the lane's PRODUCT stays on disk at `report`; only status + count + headline travel inline.
const RECEIPT = {
    type: 'object',
    additionalProperties: false,
    required: ['ok', 'report', 'entries', 'headline', 'failure'],
    properties: {
        ok: { type: 'boolean' },
        report: { type: 'string' },
        entries: { type: 'integer' },
        headline: { type: 'string' },
        failure: { type: 'string' },
    },
};
const UNIT = {
    type: 'object',
    additionalProperties: false,
    required: ['key', 'pages', 'owns', 'charter', 'after'],
    properties: {
        key: { type: 'string' },
        pages: { type: 'array', items: { type: 'string' } },
        owns: { type: 'string' }, // the ownership boundary: which vocabulary/owners THIS unit mints vs composes
        charter: { type: 'string' },
        after: { type: 'array', items: { type: 'string' } },
    },
}; // unit keys whose IMPLEMENT must land first (vocabulary producers)
const ARCH_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['preSwap', 'units', 'packageDeltas', 'summary'],
    properties: {
        preSwap: { type: 'string' },
        units: { type: 'array', items: UNIT },
        packageDeltas: { type: 'array', items: { type: 'string' } },
        summary: { type: 'string' },
    },
};
const DELTAS = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['symbol', 'change'],
        properties: { symbol: { type: 'string' }, change: { type: 'string' } },
    },
}; // navigation facts: what moved, as data, zero adjectives
const DEFERRED = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['files', 'claim'],
        properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } },
    },
};
const INDEXROWS = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['doc', 'row'],
        properties: { doc: { type: 'string' }, row: { type: 'string' } },
    },
};
const FIXLOG = {
    type: 'object',
    additionalProperties: false,
    required: ['files', 'verdict', 'summary', 'deltas', 'deferred', 'indexRows', 'phantoms'],
    properties: {
        files: { type: 'array', items: { type: 'string' } },
        verdict: { type: 'string', enum: ['authored', 'rebuilt', 'refined', 'clean'] },
        phantoms: { type: 'array', items: { type: 'string' } },
        summary: { type: 'string' },
        deltas: DELTAS,
        deferred: DEFERRED,
        indexRows: INDEXROWS,
    },
};
const DOCS_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['files', 'summary'],
    properties: {
        files: { type: 'array', items: { type: 'string' } },
        summary: { type: 'string' },
    },
};
// Catalog-roster ruling: disjoint write groups over the folder .api tier — kept stubs plus admitted new files.
const CAT_PLAN = {
    type: 'object',
    additionalProperties: false,
    required: ['groups', 'dropped', 'summary'],
    properties: {
        groups: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['key', 'catalogs'],
                properties: {
                    key: { type: 'string' },
                    catalogs: {
                        type: 'array',
                        items: {
                            type: 'object',
                            additionalProperties: false,
                            required: ['file', 'status', 'charter'],
                            properties: {
                                file: { type: 'string' }, // catalog filename under <folder>/.api/
                                status: { type: 'string', enum: ['stub', 'new'] },
                                charter: { type: 'string' }, // the owned capability territory, one line
                            },
                        },
                    },
                },
            },
        },
        dropped: { type: 'array', items: { type: 'string' } }, // proposed groupings rejected, each with its ruling
        summary: { type: 'string' },
    },
};

// --- [DOCTRINE] --------------------------------------------------------------------------

const CONTEXT =
    'Rasm monorepo - libs/csharp planning corpus (markdown specs of intended C# package designs). ' +
    'CLAUDE.md manifest + WORKSPACE_LAW strata govern (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; ' +
    'depend strictly upward). docs/stacks/csharp is the FLOOR, never the ceiling - every fence meets it and pushes past ' +
    'it to the strongest form the doctrine admits.';

const MANDATE =
    'CAMPAIGN LAW - THE HOST-BOUNDARY CONVERSION. Rasm.Rhino and Rasm.Grasshopper are planning folders (the ' +
    'index-doc shells, .planning/, and folder .api stub catalogs exist on disk); this campaign realizes them: each folder ' +
    'entirely captures its host API surface and builds higher-order abstractions, value, and capability so countless future ' +
    'Rhino/GH2 apps and plugins compose rows instead of re-deriving host plumbing. BINDING RULES: ' +
    '(1) Each folder references ONLY the Rasm kernel - leverage Rasm surfaces where they fit, NEVER any other sibling ' +
    'package, never a coupling to Element/Materials/Bim/Compute/Persistence, and NEVER Rasm.AppUi: Eto is THE native UI ' +
    'framework and each folder owns a full Eto sub-domain able to build any native UI element with native host integration. ' +
    '(2) KERNEL UNIFICATION: host-agnostic logic - easing/spring/interpolation math, perceptual color blending, pure ' +
    'geometry/numeric algorithms - COMPOSES the Rasm kernel, never a second in-folder derivation; where the kernel lacks a ' +
    'universal owner the concept demands, the kernel owner is EXTENDED in place in expand-form (a case, row, field, or ' +
    'operation on the existing Rasm planning page - the kernel recon map names the exact sites) with the host folder ' +
    'composing it; renaming or collapsing a kernel surface is recorded in `deferred` for the law tail, never raced. ' +
    '(3) CURRENT HOST ONLY: Rhino 9 WIP RhinoCommon, Grasshopper2 SDK, current Eto - a Rhino 6/7-era pattern or a GH1 idiom ' +
    '(GH_Component, IGH_*, SolveInstance, RegisterInputParams) presented as current is a phantom-class defect; every host ' +
    'member is verified via `uv run python -m tools.assay api` decompile over the host assemblies (assay blocked: the .api ' +
    'catalogs, Context7, and official McNeel material own the fallback - and the claim is marked catalog-verified, never ' +
    'guessed). (4) Planning-folder form per libs/.planning/README.md: design pages at <pkg>/.planning/<SubDomain>/<page>.md ' +
    '- PascalCase sub-folders, lowercase page names - one page per eventual source file, one dense polymorphic owner per ' +
    'page at 400-700+ LOC fence mass, transcription-complete fences, zero fence comments beyond canonical section dividers. ' +
    '(5) The existing source is CONSIDERABLE BUT NOT GOOD: capture every capability from the census, rebuild it ground-up ' +
    'denser/richer/parameterized, kill every naive pattern (both axes: thin COVERAGE slices, enumerated APPROACH rosters ' +
    'that one generator should generate), and extend to the full domain the host admits. Anticipate the FIVE-TIMES demand - ' +
    "model each owner for five times today's cases, fields, and consumers; every extension cites one gap source: HOST (a " +
    'verified member the concept admits that the census-era source ignored), DOMAIN (an attribute, state, relationship, or ' +
    'operation the real concept demands), or CONSUMER (a contract future Rhino/GH2 apps and plugins require). ' +
    '(6) Buildout over removal: a ' +
    'capability is dropped only as an explicit ruled kill; a phantom member is the sole silent deletion. (7) You fix what ' +
    'you notice anywhere in the repo EXCEPT the serialized surfaces: the owning-folder index docs (the docs agent writes ' +
    'them once), and the central manifests + cross-repo law docs (the law tail writes them once) - report exact rows in ' +
    '`indexRows` instead of editing those.';

const READ_FIRST =
    'READ FIRST, IN ORDER, BEFORE ANY EDIT. (1) DOCTRINE - enumerate docs/stacks/csharp/ with a real ' +
    'ls (never memory), read the README and EVERY root page it routes IN FULL in atlas order; then enumerate ' +
    'docs/stacks/csharp/domain/ through its router README and read every shard your pages touch (the Rhino/geometry/' +
    'host shards are mandatory here). This prompt does not restate doctrine; read it at source and conform every fence ' +
    'to it. (2) .API - ls BOTH catalog tiers in full: the shared substrate libs/csharp/.api/ AND the folder .api/, plus ' +
    'the kernel folder catalogs libs/csharp/Rasm/.api/ (the RhinoCommon-adjacent truths live there); layer the ' +
    'Thinktecture/LanguageExt rails onto the host surfaces, never the host set alone. (3) AUTHORING LAW - ' +
    'libs/.planning/README.md in full (doc-set, page grammar, card law, banned hedges) and docs/standards/' +
    'style-guide.md. (4) KERNEL - the Rasm package README.md + ARCHITECTURE.md so kernel leverage composes real ' +
    'surfaces, never guesses; the kernel recon report carries the exact unified-owner sites.';

const STANCE =
    'STANCE - every pass is hostile; the pages under review were authored by ANOTHER engineer and are under ' +
    'adversarial review. Hold every fence naive, shallow, or illusory until it survives a real attack; the burden of proof ' +
    'is on the code. Dense, confident, package-fluent code is the PRIME suspect. NAIVETY is a defect on two axes: COVERAGE ' +
    '(a 2-case family for a 20-case domain) and APPROACH (an enumerated roster where one parameterized generator should ' +
    'generate the space - the roster demotes to seed DATA). ILLUSORY code is the primary target: doctrine vocabulary ' +
    '([Union]/[SmartEnum<TKey>]/[ValueObject]/rails), cited hosts, confident prose, hollow body. Every collapse-signal ' +
    'list is a FLOOR. NO CHURN: an edit requires a named violated law or invariant and the concrete case that breaks it; a ' +
    'clean verdict earned by an attack that finds nothing is a first-class result, proven by adding nothing.';

const WRITE_FULLY =
    'WRITE FULLY - every fix you identify you make NOW; the fix-log reports edits already made, ' +
    'never a to-do or a hedge. A first-order cross-file ripple your edit causes is YOURS in the same pass; a second-order ' +
    'ripple (exposed by a ripple repair) or a surface a concurrent unit owns is recorded in `deferred` as {files, claim} - ' +
    'the law tail drains the backlog; nothing is silently dropped. Latest modern C# 14 on net10; apply the ' +
    'docs/stacks/csharp file-organization + section-order law; total generated Switch, no silent _ arm; prose per ' +
    'docs/standards/style-guide.md - declarative present-tense fact, every symbol backticked, zero meta framing, zero ' +
    'provenance, never fragile count-based prose.';

const CURRENT_STATE =
    'CURRENT STATE - sibling units land work concurrently with yours. Before any edit, re-read the ' +
    'CURRENT on-disk state of your pages AND every sibling page your pages compose; landed sibling work is picked up as ' +
    'found. A vocabulary owner your charter marks as composed (not minted) is read from disk as it NOW stands; a conflict ' +
    'between your design and a landed sibling resolves to the stronger form, never a revert. SEAM LEDGER: append one row ' +
    'per cross-unit event to your unit ledger `' +
    CODEX_DIR +
    '/<folder>-<unit>-seams.md` (`SEAM_CHANGED | <files> | ' +
    '<symbol fact, old -> new>` when a shared name/signature you mint moves; `RIPPLE_REPAIRED | <files> | <fact>` when you ' +
    'repair a counterpart). Before any edit outside your unit pages, ls `' +
    CODEX_DIR +
    '/` and read every sibling ' +
    '`*-seams.md` row whose files intersect yours - a RIPPLE_REPAIRED row is work you do NOT redo.';

const GIT_GROUND =
    'DELTA GROUNDING - run `git status` and `git diff --stat -- <your unit pages>` to see what this run ' +
    'changed before judging it; the diff is orientation, CURRENT disk is truth.';

const INFO_LAW =
    'You provide INFORMATION, never prescriptions: exact disk locations and anchors, the current shape at ' +
    'each surface, verified member spellings, gaps. The architect and unit writers decide how to build; an entry that ' +
    'tells them what to write instead of what is true is a defect. ENTRY FORM: `info` is prose truth; `anchors` carry one ' +
    'coordinate per row (role names what it proves; `note` is the shortest literal witness under 20 words, or empty when ' +
    'path+line suffice; an `absence` anchor names where the expected thing was searched and not found); `files` lists ' +
    'what the reader must open for the entry; `members` carry verified spellings only. An underutilized-capability entry ' +
    'is INVENTORY, never instruction: verified members, current usage anchors, the concept that admits it - the writer ' +
    'decides whether it composes. COVERAGE is part of the product: `requested` = your assigned scope, `read` = what you ' +
    'actually full-read, `skipped`/`unverified` = what you did not reach - an honest skip beats a silent one.';

const EVIDENCE_LAW =
    'ENTRY FORM - you deliver TRUTH, never an implementation: `claim` states the observed fact or ' +
    'defect; `mechanism` states WHY the current form fails as fact; `anchors` carry one coordinate per row (role names ' +
    'what the coordinate proves; `note` is the shortest literal witness - a symbol, member spelling, or fragment under ' +
    '20 words - or empty when path+line suffice; an `absence` anchor names where the expected thing was searched and not ' +
    'found); `owner` names the canonical owner that must absorb the resolution (the owning axis, row roster, or ' +
    'polymorphic owner - never a new local shape); `reject` lists the forms the rebuild must not take; `acceptance` ' +
    'lists the signals that prove resolution. NEVER write add/replace/implement/promote/delete as instruction - the ' +
    'writer owns the design; you own the constraint boundary. `claimKey` = <kind>|<owner>|<primary symbol or absence ' +
    'route>, identical for the same fact regardless of lane or wording. `severity` binds to consequence: blocker = ' +
    'campaign-blocking, major = rebuild-ruling correctness, minor = local cleanup - never prose confidence. OUTPUT ' +
    'BOUNDS: the task states your register bound - a census register is COMPLETE (every assigned file appears as one ' +
    'entry); an audit scope returns only confirmed misses, 0 only when the hostile pass comes back empty, and then ' +
    '`summary` names the probes that produced nothing; never manufacture an entry, never delete a confirmed one. ' +
    'COVERAGE is part of the product: `requested` = your assigned scope, `read` = what you actually full-read, ' +
    '`skipped`/`unverified` = what you did not reach or could not confirm - an honest skip beats a silent one.';

// --- [OPERATIONS] ------------------------------------------------------------------------

const sleep = (ms) => new Promise((res) => setTimeout(res, ms));
// Agent-level slot scheduler: CAP agents in flight across ALL folders and units, staggered launch,
// work-conserving backfill. The single governor for every agent call.
const makeSlots = (cap) => {
    let active = 0;
    let gate = Promise.resolve();
    const waiters = [];
    const stagger = () => {
        gate = gate.then(() => sleep(STAGGER_MS));
        return gate;
    };
    return async (fn) => {
        if (active >= cap) await new Promise((res) => waiters.push(res));
        active++;
        await stagger();
        try {
            return await fn();
        } finally {
            active--;
            const next = waiters.shift();
            if (next) next();
        }
    };
};

const slot = makeSlots(CAP);
// Codex dispatch: the sonnet wrapper makes one blocking Codex MCP call, writes its result verbatim, and returns mechanical orchestration data.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-');
const codexPrompt = (label, task, schema, o) => {
    const base = CODEX_DIR + '/' + fileTag(label);
    const root = '/Users/bardiasamiee/Documents/99.Github/Rasm';
    const report = root + '/' + base + '-report.json';
    const model = o.model || 'gpt-5.6-terra';
    return [
        'DISPATCH ROLE: ' +
            model +
            ' performs the complete TASK below through one blocking Codex MCP call. Follow exactly four steps; ' +
            'never perform, edit, judge, soften, summarize, or relay the task yourself.',
        '(1) Call ToolSearch with query "select:mcp__codex__codex".',
        '(2) Call the loaded mcp__codex__codex tool ONCE with model="' +
            model +
            '", sandbox=' +
            (o.writes ? '"workspace-write"' : '"read-only"') +
            ', cwd=' +
            JSON.stringify(root) +
            ', config={"model_reasoning_effort":"' +
            (o.codexEffort || 'high') +
            '"}, and prompt set to the COMPLETE task text below followed by its final-message contract. ' +
            'If the call errors, retry the identical call ONCE; if the retry errors, skip step (3) and return the error through step (4). ' +
            'TASK:\n\n' +
            task +
            '\n\nFinal message: ONLY a JSON object matching this shape (no fences, no prose): ' +
            JSON.stringify(schema),
        '(3) Write the tool result text VERBATIM with the Write tool to this absolute path: ' +
            report +
            '. Do not normalize, reformat, summarize, or extract the text before writing it.',
        '(4) Parse the tool result text only to compute entries.length and the kind tallies. Return ok=true, report=' +
            base +
            '-report.json, entries=that count, headline="<entries> entries | <kind tallies> | top: <most frequent first file or none>", and ' +
            'failure empty. On a second tool error return ok=false, entries=0, report and headline empty, and failure equal to the error text.',
    ].join('\n\n');
};
// Every codex-dispatched lane routes here: terra by default, sol where o.model says so; CODEX=false
// restores a native lane at o.nativeModel (opus default). The roster row carries `scope` from the
// ORCHESTRATOR (never the lane's self-report) so a failed lane's unmapped territory is exact even
// when the lane died before writing anything.
const recon = (task, o) =>
    (CODEX
        ? agent(codexPrompt(o.label, task, o.schema, o), {
              label: (o.model && o.model.indexOf('-sol') >= 0 ? 'sol:' : 'terra:') + o.label,
              phase: o.phase,
              model: 'sonnet',
              effort: 'low',
              schema: RECEIPT,
              stallMs: o.stallMs || STALL,
          })
        : agent(
              task +
                  '\n\nPRODUCT TO DISK: write your COMPLETE product as one JSON file matching this schema at ' +
                  CODEX_DIR +
                  '/' +
                  fileTag(o.label) +
                  '-report.json (Write tool, absolute path under the repo root): ' +
                  JSON.stringify(o.schema) +
                  ' — then return ONLY the receipt: ok, report path, entries count, one-line mechanical headline, failure empty.',
              { label: o.label, phase: o.phase, model: o.nativeModel || 'opus', effort: 'high', schema: RECEIPT, stallMs: o.stallMs || STALL },
          )
    )
        .then((r) => ({
            lane: o.label,
            scope: o.scope || [],
            ok: !!(r && r.ok && r.report),
            report: (r && r.report) || '',
            entries: (r && r.entries) || 0,
            headline: (r && r.headline) || '',
            failure: (r && r.failure) || (r ? '' : 'lane died'),
        }))
        .catch(() => ({ lane: o.label, scope: o.scope || [], ok: false, report: '', entries: 0, headline: '', failure: 'lane died' }));
// Navigation handoff: FACTS ONLY - files, symbol deltas, backlog. Never verdicts, summaries, or adjectives.
const navOf = (logs) => {
    const rows = logs.filter(Boolean);
    return {
        files: [...new Set(rows.flatMap((r) => r.files || []))],
        deltas: rows.flatMap((r) => r.deltas || []),
        deferred: rows.flatMap((r) => r.deferred || []),
    };
};

const censusPrompt = (folder, lane) =>
    [
        CONTEXT,
        MANDATE,
        EVIDENCE_LAW,
        'TASK: HOSTILE READ-ONLY SOURCE CENSUS over ' +
            folder.root +
            ' - your assigned slice: ' +
            lane.paths +
            '. ' +
            'Read every assigned .cs file IN FULL. For EACH file emit one `register` entry: `target` = the file; `info` = the ' +
            'owned capability in concept terms plus the HOST-AGNOSTIC vs HOST-BOUND split (pure math/algebra liftable to the ' +
            'Rasm kernel vs genuinely host-coupled mechanism); `members` = the exact host members it composes (' +
            folder.host +
            '); `claim` = the quality verdict with the naivety axes named; `mechanism` = why the current form fails; `owner` = ' +
            'the collapse signal (the one polymorphic owner the sibling capabilities belong inside); `reject` = what dies as ' +
            'pattern; `acceptance` = what survives as concept (the rebuild intent). Also inventory ' +
            folder.root +
            '/' +
            folder.name +
            '.csproj as one `manifest` entry: references, packages, host assembly bindings in `info`/`members`. ' +
            'The register must be COMPLETE - every assigned file appears; downstream disposition audits key off your entries.',
    ].join('\n\n');

const miningPrompt = (folder, lane) =>
    [
        CONTEXT,
        MANDATE,
        INFO_LAW,
        'TASK: CURRENT-ONLY HOST-API MINING for ' +
            folder.name +
            ' - lane ' +
            lane.key +
            ' (read-only; investigate, do NOT edit - opus catalog authors consume your report). ' +
            lane.charge +
            ' ' +
            'PRIMARY ROUTE: `uv run python -m tools.assay api` decompile over the installed host assemblies - enumerate ' +
            'namespaces, then drill the surfaces your charge names; quote verified member signatures with their assembly ' +
            'anchors. SECONDARY: existing catalogs under libs/csharp/Rasm/.api/ and libs/csharp/.api/, Context7, and current ' +
            'McNeel developer material - marked catalog-verified when assay cannot reach a surface. FORBIDDEN: any Rhino 6/7-era ' +
            'or GH1 pattern presented as current; training-data recall without verification. YOUR STUB TERRITORY at ' +
            folder.root +
            '/.api/: ' +
            lane.catalogs +
            ' - each stub declares its owned namespace scope; read each and mine its territory to operator DEPTH. ' +
            'REPORT PRODUCT: (1) one `catalog` entry per assigned stub - the verified member inventory for its declared scope: ' +
            'exact signatures in `members` (deep - the full useful surface, not a sample), integration shape per capability ' +
            'cluster in `info`, what the census-era source missed. (2) `gap` entries: host capability territory NO stub in the ' +
            'folder covers - each a candidate NEW catalog grouping sized for one focused file (never a god file, never a mini ' +
            'file): name the territory, exemplar verified members, and the concept that admits it, as fact. (3) `capability`/' +
            '`seam` entries for cross-catalog findings. Verified anchors ONLY; a fake anchor or phantom member is your defect.',
    ].join('\n\n');

const kernelPrompt = (slice) =>
    [
        CONTEXT,
        MANDATE,
        INFO_LAW,
        'TASK: READ-ONLY KERNEL MINING over libs/csharp/Rasm - the rebuilt kernel planning corpus the host folders compose. ' +
            'Your slice: the ' +
            slice.dirs +
            ' sub-domains. Read the package README.md + ARCHITECTURE.md, then EVERY design page under ' +
            'libs/csharp/Rasm/.planning/ inside your slice IN FULL, and ls libs/csharp/Rasm/.api/ (read any catalog your slice ' +
            'pages cite). PRODUCT - information, never prescriptions: (1) `owner` entries: every kernel owner in your slice the ' +
            'host-boundary layers must COMPOSE instead of re-deriving - easing/spring/interpolation/timeline math, perceptual ' +
            'color algebra, pure geometry/numeric algorithms, parametric/motion vocabulary - each with the exact page, owner ' +
            'name, and member spellings quoted with file:line anchors; mine to operator depth: the full capability an owner ' +
            'carries, not the fraction the census-era source used. (2) `gap` entries: host-agnostic capability the census-era ' +
            'host source hand-rolls (46-curve easing families, damped-spring integrators, OKLab/OKLCH blending, repeat/yoyo ' +
            'cycle arithmetic) that NO kernel owner in your slice carries yet - name the closest existing kernel owner and the ' +
            'expand-form extension site (the page + section where a case/row/field/operation would land) as fact. (3) `seam` ' +
            'entries: every kernel surface in your slice whose shape the host folders depend on, quoted. MANDATORY SELF-VERIFY: ' +
            're-open every cited anchor before returning; a guess or vague entry is deleted.',
    ].join('\n\n');

const catalogPlanPrompt = (folder, miningR) =>
    [
        CONTEXT,
        MANDATE,
        'TASK: RULE the .api catalog roster for ' +
            folder.root +
            '/.api/ - judgment only, do NOT author catalogs (your authors do; you MAY NOT write any catalog content). ' +
            'EVIDENCE: the mining reports are ON DISK as typed JSON - read every ok report IN FULL; a failed lane below means ' +
            'its stub territory is ruled from the stubs themselves plus your own targeted assay probes: ' +
            JSON.stringify(miningR.map((m) => ({ lane: m.lane, ok: m.ok, report: m.report, entries: m.entries }))) +
            '. Then ls ' +
            folder.root +
            '/.api/ (the stub roster) and ls libs/csharp/Rasm.Bim/.api/ + libs/csharp/.api/ (the mature form exemplars: many ' +
            'focused per-capability files, none god, none mini). RULE: (a) every existing stub keeps its file and scope unless ' +
            'two stubs genuinely own one concept (merge ruling) or one stub owns two (split ruling); (b) every mining `gap` ' +
            'grouping is admitted as a NEW catalog file, merged into a kept file, or rejected with a ruling in `dropped` - a ' +
            'silently ignored gap is your defect; (c) each catalog carries a one-line charter naming its owned capability ' +
            'territory, disjoint from every sibling. PARTITION the ruled roster into at most ' +
            CAT_AUTHORS +
            " write groups of related catalogs, balanced by expected depth (a group is one author's whole write scope; no " +
            'catalog appears twice). Return {groups, dropped, summary}.',
    ].join('\n\n');

const catalogAuthorPrompt = (folder, group, miningR) =>
    [
        CONTEXT,
        MANDATE,
        'TASK: AUTHOR the .api catalogs of group ' +
            group.key +
            ' for ' +
            folder.root +
            '/.api/ - EXACTLY these files, yours alone this run (no other lane writes them): ' +
            JSON.stringify(group.catalogs) +
            '. LOAD the `docgen` skill via the Skill tool BEFORE writing - the catalogs are durable agent-facing markdown and ' +
            'pass its prose gate. FORM: read two mature exemplars first - one folder-tier catalog from libs/csharp/Rasm.Bim/' +
            '.api/ and one substrate catalog from libs/csharp/.api/ - and match their structure, register, and density. ' +
            'EVIDENCE: the mining reports on disk carry the verified member inventories - read the ones covering your files IN ' +
            'FULL: ' +
            JSON.stringify(miningR.filter((m) => m.ok).map((m) => m.report)) +
            '. Every member you write beyond a report is verified yourself via `uv run python -m tools.assay api` decompile ' +
            '(assay blocked: Context7/official McNeel material, marked catalog-verified); a member you cannot verify is ' +
            'OMITTED, never guessed - a GH1 or Rhino 6/7-era idiom presented as current is a phantom-class defect. CONTENT per ' +
            "catalog: the file's charter, verified member signatures organized by capability cluster, the integration shape " +
            'per cluster, and a [STACKING] section layering the Thinktecture/LanguageExt substrate rails onto the host ' +
            'surfaces - inventory and integration FACT only, never design prescriptions, never process narration, zero ' +
            'provenance. A `new`-status file is authored whole from its charter; a `stub` file is DEEPENED in place preserving ' +
            'its scope and name. Return {files, summary}.',
    ].join('\n\n');

const architectPrompt = (folder, survey, kernelReports, unmapped, catRoster) =>
    [
        CONTEXT,
        MANDATE,
        READ_FIRST,
        STANCE,
        'TASK: RULE + SCAFFOLD the ' +
            folder.name +
            ' planning folder. EVIDENCE - the survey REPORT FILES are your ' +
            "reconnaissance. CONSUMPTION: (a) UNMAPPED scope below gets your own cold read FIRST - a failed lane's territory " +
            'is your direct census; (b) read every ok report IN FULL from disk - the kernel mining reports (' +
            (kernelReports.length ? kernelReports.join(', ') : 'ALL UNMAPPED: read libs/csharp/Rasm README + ARCHITECTURE + .planning directly') +
            ') and the ' +
            'host-API mining reports before the census slices; entries overlap across lanes, dedupe by claimKey/target as you read; ' +
            "(c) each entry's anchors are jump coordinates - spot-verify what you build on, and re-open every anchor behind an " +
            'edit; (d) `owner`/`reject`/`acceptance` on a census entry are its constraint boundary - honor them; the DESIGN is ' +
            'yours. UNMAPPED: ' +
            JSON.stringify(unmapped) +
            ' ROSTER: ' +
            JSON.stringify(survey) +
            '. The ruled .api catalog roster (opus authors land these concurrently; cite catalogs in unit charters by file): ' +
            catRoster +
            '. ' +
            'RULE the architecture: RE-DERIVE the ' +
            'sub-domain map from the census evidence alone - a sub-domain earns 3+ pages or folds; rule every split and every ' +
            'merge from the evidence, never from the inherited file layout and never from a pre-ruled example: concern-mixing ' +
            'in a source file is genuine split pressure, a cohesive concern is genuine keep-together pressure, and the landed ' +
            'map carries no god-surface and no one-page shell sub-domain; ' +
            'an Eto sub-domain is MANDATORY and owns full native UI construction as generator rows. Emit the complete page ' +
            'roster (PascalCase sub-folders, lowercase page names, one dense owner per page), the disposition of every census ' +
            'register entry (absorbed into a named page or explicitly killed with a ruling), the host-agnostic rows routed to the ' +
            'kernel per MANDATE rule (2) (each with its kernel extension site from the mining), and any packageDeltas (central ' +
            'Directory.Packages.props motions - REPORT them, never edit the central manifest; folder .csproj edits are yours). ' +
            'PARTITION the roster into at most ' +
            MAX_UNITS +
            ' build units of at most ' +
            UNIT_PAGES +
            ' pages each: units ' +
            'run CONCURRENTLY, so each unit carries `owns` (the vocabulary/owners it MINTS - no two units mint the same owner), ' +
            'a charter (collapse rulings, census-entry dispositions, host-capability targets from the research catalogs, kernel ' +
            'compose/extend rows), and `after` (the unit keys whose IMPLEMENT must land before this unit starts - ONLY true ' +
            'vocabulary dependence, typically the foundation unit alone; an empty after is the default, and after may only name ' +
            'units earlier in your emitted order). THEN EXECUTE the scaffold: record the current HEAD hash as preSwap; create ' +
            folder.root +
            '/.planning/<SubDomain>/ dirs; git rm every existing .cs source file and source sub-folder (the ' +
            'census captured them; git history recovers them; the sibling architect runs concurrently - on a git index.lock ' +
            'failure, wait briefly and retry) - keep the .csproj, packages.lock.json, the index-doc shells ' +
            '(IDEAS.md, TASKLOG.md), and the .api/ catalogs; apply ruled folder .csproj edits. Do NOT commit. Return {preSwap, ' +
            'units, packageDeltas, summary}.',
    ].join('\n\n');

const implementPrompt = (folder, unit, preSwap, reports, kernelReports, scopes) =>
    [
        CONTEXT,
        MANDATE,
        READ_FIRST,
        STANCE,
        WRITE_FULLY,
        CURRENT_STATE,
        'TASK: GROUND-UP AUTHOR unit ' +
            unit.key +
            ' of ' +
            folder.name +
            ' - build freely and ambitiously to the full ' +
            'bar; the trailing critique and red-team passes carry the attack. EXACTLY these pages: ' +
            unit.pages.join(', ') +
            '. OWNS: ' +
            unit.owns +
            '. CHARTER: ' +
            unit.charter +
            ' Evidence reports (typed JSON products on disk - read the ' +
            "ones your charter touches IN FULL; each entry's anchors are jump coordinates, re-open any anchor behind an edit): " +
            reports.join(', ') +
            '; kernel mining reports: ' +
            (kernelReports.length ? kernelReports.join(', ') : 'ALL UNMAPPED - read libs/csharp/Rasm README + ARCHITECTURE + .planning directly') +
            '; the folder .api catalogs are freshly authored and verified - compose them at operator depth. ' +
            'CONCURRENT UNIT SCOPES (a page another unit owns is composed from disk, never edited; a needed change there is a ' +
            '`deferred` row): ' +
            scopes +
            '. Old source recovers via git show ' +
            preSwap +
            ':<path> when a census entry needs ' +
            'depth beyond its register. Before authoring EACH page, restate in one line the owner it holds, the vocabulary it ' +
            'mints vs composes, and the doctrine laws that bind it - then build against that restatement. Construct in ' +
            'LIFECYCLE order: admit raw once, canonical owner by OWNER_CHOOSER, stacked rail/aspect over a thin pure core, ' +
            'projection, egress, BOTH ingress and egress parameterized; collapse parallel shapes into ONE [Union]/' +
            '[SmartEnum<TKey>]/[ValueObject<T>]/[ComplexValueObject]/source-generated case family IN THE SAME FILE; one ' +
            'polymorphic entrypoint per modality. Host-agnostic math COMPOSES or EXTENDS the kernel per MANDATE rule (2). ' +
            'Every host member verified per the mandate route before it is written. Return the fix-log - `deltas` carries every ' +
            'minted symbol/wire as data, `deferred` the backlog rows, both exact.',
    ].join('\n\n');

const critiquePrompt = (folder, unit, scopes, nav) =>
    [
        CONTEXT,
        MANDATE,
        READ_FIRST,
        STANCE,
        WRITE_FULLY,
        CURRENT_STATE,
        'NAVIGATION (facts from the pass that landed these pages - locations only, no assessments; it changes where you look ' +
            'FIRST, never what you conclude): ' +
            JSON.stringify(nav),
        GIT_GROUND,
        'TASK: CRITIQUE - your role law is libs/.planning/campaign-method.md [04] CRITIQUE, read at source and held to the ' +
            'letter: the mechanical line-by-line doctrinal-conformance and capability-completeness audit, every hit a fix made ' +
            'now, never a note; the named checklists are a FLOOR you hunt past. Your mandate is PREDICATE-POSITIVE: verify each ' +
            'required law holds and cite the clause. FORM YOUR OWN DEFECT LIST FIRST - read each page cold from CURRENT disk ' +
            'before consulting NAVIGATION. Fix EACH page of unit ' +
            unit.key +
            ' in place: ' +
            unit.pages.join(', ') +
            '. ' +
            'CONCURRENT UNIT SCOPES (foreign pages composed, never edited; changes there go to `deferred`): ' +
            scopes +
            '. ' +
            '- COLLAPSE_SCAN: run the docs/stacks/csharp README [03] table on every fence. ' +
            '- OWNER_CHOOSER (shapes.md [01]): re-derive every shape from its discriminants; kill every parallel DTO, ' +
            'one-field wrapper, and null/default ghost. ' +
            '- KNOB_TEST: delete each parameter - where the value reconstructs it, collapse to a policy value or input-shape ' +
            'discriminant. ' +
            '- ASPECTS + RAILS: audit against surfaces-and-dispatch.md and rails-and-effects.md at their owning pages. ' +
            '- HOST TRUTH: every RhinoCommon/Eto/GH2 member re-verified against the folder .api catalogs; a legacy idiom (GH1, ' +
            'Rhino 7-era) is a phantom - delete or rebuild on the current surface. ' +
            '- ULTRA-STACKING (both tiers): ls the folder .api/ AND libs/csharp/.api/ in full; an admitted capability the ' +
            "page's concept admits that no fence exploits - a host surface the folder catalog carries, or a Thinktecture/" +
            'LanguageExt/substrate rail the shared tier carries unlayered onto the host set - is a defect closed by growing ' +
            'the owner, never noted. ' +
            '- KERNEL UNIFICATION: host-agnostic math re-derived in the fence instead of composing/extending the kernel is a ' +
            'defect fixed per MANDATE rule (2). ' +
            '- BOUNDARY: the folder references ONLY the Rasm kernel; a coupling to any other sibling or to AppUi is a defect ' +
            'fixed now. ' +
            '- CAPABILITY-COMPLETENESS + ILLUSION: the body implements what names and prose promise; close every admitted host ' +
            'capability the owner omits per the charter and the .api catalogs; attack both naivety axes. ' +
            'Return the fix-log - `deltas` and `deferred` exact.',
    ].join('\n\n');

const redteamPrompt = (folder, unit, scopes, nav, crit) =>
    [
        CONTEXT,
        MANDATE,
        READ_FIRST,
        STANCE,
        WRITE_FULLY,
        CURRENT_STATE,
        'NAVIGATION (locations only, no assessments): ' + JSON.stringify(nav),
        crit && crit.ok
            ? 'PRIOR CLAIMS (UNVERIFIED): the sol critique fixlog is ON DISK at ' +
              crit.report +
              ' - read it IN FULL from disk; its edits and verdicts are refutation targets you judge against CURRENT disk, ' +
              'never a settled record. FOLD-FORWARD DUTY: its surviving `deferred` and `indexRows` rows are folded into YOUR ' +
              "return (re-verified against current disk, deduped) - your fixlog is the unit's consolidated record; a dropped " +
              'critique row is a silent loss.'
            : 'PRIOR CLAIMS: the critique lane did not land - your cold attack is the only review this unit gets; judge from ' +
              'CURRENT disk alone.',
        GIT_GROUND,
        'TASK: RED-TEAM - your role law is libs/.planning/campaign-method.md [04] RED-TEAM, read at source and held to the ' +
            'letter: the terminal and most aggressive review; every defect repaired in place, and the work ends objectively ' +
            'DENSER and MORE CAPABLE than critique left it. Your mandate is PREDICATE-NEGATIVE - a pre-mortem, not a second ' +
            'conformance audit. FORM YOUR OWN ATTACK FIRST - cold-read each page from CURRENT disk before consulting the claims. ' +
            'Fix EACH page of unit ' +
            unit.key +
            ' in place: ' +
            unit.pages.join(', ') +
            '. CONCURRENT UNIT SCOPES (foreign ' +
            'pages composed, never edited; changes there go to `deferred`): ' +
            scopes +
            '. ' +
            '(A) COUNTERFACTUAL on the core owner/algebra/dispatch - does a denser generated family, a derived table, a ' +
            'parameterized generator over the enumerated space, or a deeper LanguageExt/Thinktecture/host/kernel primitive ' +
            'collapse the whole fence? A fundamentally stronger design is built, never defended against. (B) ' +
            'ANTICIPATORY_COLLAPSE - the diff of the next feature: the next control kind, command, conduit, component family, or ' +
            'canvas widget lands as one row with every consumer untouched or loudly broken. (C) LONG-TAIL - empty/singular/' +
            'plural/stream/malformed/concurrent/cancelled/partial-failure/host-teardown; undo records, document events, and ' +
            'solution expiry handled where the host demands them. (D) BOUNDARY/STRATA - kernel-only references; host types never ' +
            'leak past the folder contract; host-agnostic math composes the kernel per MANDATE rule (2). (E) SPRAWL + PHANTOMS - ' +
            'hand-re-derived host capability, flat code below the operator depth the packages reach, a phantom or legacy member ' +
            '(delete), a thin wrapper; and the inverse: an edit that ADDED surface where the doctrine demands collapse is ' +
            'regression rebuilt denser. (F) a FULL COLD RE-REVIEW of every conformance dimension by name. Return the fix-log.',
    ].join('\n\n');

const docsPrompt = (folder, unitReports) =>
    [
        CONTEXT,
        MANDATE,
        WRITE_FULLY,
        'TASK: the index docs for ' +
            folder.root +
            ' per libs/.planning/README.md [02] - you are their ONE writer. LOAD the `docgen` skill via the Skill tool ' +
            'BEFORE writing; the docs pass its prose gate. Read ' +
            'libs/csharp/Rasm/README.md and libs/csharp/Rasm/ARCHITECTURE.md IN FULL as the sibling form exemplars, plus ' +
            'docs/standards/style-guide.md and docs/standards/formatting.md - the new docs match that structure, styling, and ' +
            'grammar exactly. README.md: the page router over the landed .planning tree + the domain-package registry (host ' +
            'assemblies + any folder additions) + the substrate section pointing at the branch registry. ARCHITECTURE.md: the ' +
            'standardized intro, the codemap tree naming every sub-domain with one-line charters (the eventual source tree, ' +
            'never the .planning scaffold), [02]-[SEAMS] carrying only genuine cross-folder rows (kernel consumption is a ' +
            'codemap fact, not a seam; the folder has no cross-language seam). IDEAS.md and TASKLOG.md exist as shells - ' +
            "PRESERVE them untouched; deferred-concept cards are the terminal law fixer's authority, never yours. Verify every " +
            'router row against disk. Unit results: ' +
            JSON.stringify(unitReports) +
            '. Return {files, summary}.',
    ].join('\n\n');

const acceptPrompt = (r, rows) =>
    [
        CONTEXT,
        EVIDENCE_LAW,
        'TASK: READ-ONLY ACCEPTANCE over the converted ' +
            r.folder +
            ' (' +
            r.root +
            ') - investigate, never edit. ' +
            '(1) `symbol` - cross-page symbol sweep: every cross-page symbol a landed fence composes resolves on a sibling ' +
            'owner with a matching signature; (2) `disposition` - FULL census-disposition audit: read EVERY register entry ' +
            'from the census reports under ' +
            CODEX_DIR +
            '/ (census-' +
            r.key +
            '-*-report.json) and confirm each landed in a page or carries an explicit architect kill - a capability that ' +
            'silently vanished is a `disposition` miss; (3) `boundary` - no reference beyond the Rasm kernel, no AppUi, no ' +
            'GH1/legacy member cited as current; (4) `kernel` - no host-agnostic math re-derived in a fence where the kernel ' +
            'owns or gained the owner; (5) `law` - the folder README router rows and ARCHITECTURE codemap resolve against the ' +
            'landed .planning tree, and no out-of-scope/no-planning/durable-source claim survives in the cross-repo law docs. ' +
            'KNOWN-FAILED PAGES (never landed; a disposition miss routed to one is still reported, marked failed-unit ' +
            'territory in `info`): ' +
            JSON.stringify(r.failedPages || []) +
            '. PENDING ROWS - the terminal law fixer applies these after you; a miss those rows already close is DROPPED, ' +
            'never reported: ' +
            JSON.stringify(rows) +
            '. Each confirmed miss is one entry of the matching kind with file-evidence anchors; zero misses is a valid ' +
            'empty result, and then `summary` names the probes that produced nothing.',
    ].join('\n\n');

const lawPrompt = (results, backlog, accepts, accUnmapped, orphans) =>
    [
        CONTEXT,
        MANDATE,
        WRITE_FULLY,
        "TASK: TERMINAL LAW FIXER - you are the run's LAST agent, nothing follows you: the ONE writer for central manifests " +
            'and cross-repo law docs, the drain for the deferred backlog, and the resolver for every acceptance finding. ' +
            "(0) ORPHANED CRITIQUE FIXLOGS (units whose red-team never landed, so these on-disk fixlogs' `deferred` and " +
            '`indexRows` rows were never folded forward - read each IN FULL from disk and drain those rows under the same law ' +
            'as (2) and (3)): ' +
            JSON.stringify(orphans) +
            '. ' +
            '(1) CENTRAL MANIFEST: apply the collected packageDeltas to Directory.Packages.props at the correct label groups, ' +
            'newest stable versions verified via the nuget MCP; reflect any new package in the owning folder README registry if ' +
            'the docs agent missed it: ' +
            JSON.stringify(results.map((r) => ({ folder: r.folder, packageDeltas: r.packageDeltas }))) +
            '. ' +
            '(2) DEFERRED BACKLOG (second-order and cross-unit ripples the writers recorded - re-verify each {files, claim} on ' +
            'current disk, fix what holds, reject what disk already resolved; kernel-surface collapse/rename rows are yours to ' +
            'apply now that no concurrent writer runs; a verified claim genuinely outside campaign scope lands as a ' +
            'fully-specified card in the owning folder IDEAS.md or TASKLOG.md, never dropped): ' +
            JSON.stringify(backlog) +
            '. ' +
            '(3) INDEX ROWS reported by writers (apply each to its owning doc exactly once, deduped): ' +
            JSON.stringify(results.flatMap((r) => r.indexRows || [])) +
            '. ' +
            '(4) ACCEPTANCE REPORTS - the acceptance products are ON DISK as JSON report files; the receipts below are ' +
            'navigation, never the product. UNMAPPED territory (a dead lane) is your direct audit queue - run those audit ' +
            'dimensions over that folder yourself. Read every ok report IN FULL from disk; each finding is a SIGNAL, not law: ' +
            're-open its anchors before editing, honor its `owner`/`reject`/`acceptance` constraint boundary, and implement ' +
            'the STRONGEST resolution that boundary admits - never a single-point patch where a denser root-level repair is ' +
            'available; a finding with a dead anchor or already resolved on disk is rejected with reason. ROSTER: ' +
            JSON.stringify(accepts.map((a) => ({ lane: a.lane, ok: a.ok, report: a.report, entries: a.entries, headline: a.headline }))) +
            ' UNMAPPED: ' +
            JSON.stringify(accUnmapped) +
            '. FAILED PAGES (reported, never landed - never author them here; correct any index or law claim that pretends ' +
            'they landed): ' +
            JSON.stringify(results.flatMap((r) => r.failedPages || [])) +
            '. ' +
            '(5) LAW DOCS - both folders are planning folders and the cross-repo docs already state it; VERIFY and fix residuals: ' +
            'libs/.planning/planning-targets.md (both in the CSHARP Planning Folders row), libs/.planning/architecture.md, ' +
            'libs/csharp/.planning/README.md (upgrade the two HOST-BOUNDARY router rows to README.md links now the files exist), ' +
            'libs/csharp/.planning/ARCHITECTURE.md. (6) SWEEP: rg for Rasm.Rhino and Rasm.Grasshopper across libs/**/*.md, ' +
            'README.md, and docs/** (excluding docs/stacks) - fix every remaining stale out-of-scope/no-planning/durable-source ' +
            'claim. PROSE LAW: docs/standards/style-guide.md - declarative present-tense fact, zero meta framing, structure named ' +
            'by members and law, never by count. Return {files, summary}.',
    ].join('\n\n');

// --- [COMPOSITION] -----------------------------------------------------------------------

log(
    'Continuation of wf_f27cf61a-18a: ' +
        STATE.folders.map((f) => f.name + ' (' + Object.keys(f.done).length + '/' + f.arch.units.length + ' units baked)').join(', ') +
        '; all lanes native',
);

phase('Build');
const results = (
    await Promise.all(
        STATE.folders
            .map(async (SF) => {
                const folder = FOLDERS.filter((f) => f.key === SF.key)[0];
                const arch = SF.arch;
                const units = arch.units;
                const scopes = JSON.stringify(units.map((u) => ({ unit: u.key, owns: u.owns, pages: u.pages })));
                const reports = STATE.reports;
                const kernelReports = STATE.kernelReports;
                const implDone = new Map();
                units.forEach((u) => {
                    let release;
                    implDone.set(u.key, {
                        p: new Promise((res) => {
                            release = res;
                        }),
                        release,
                    });
                });
                const keyIndex = new Map(units.map((u, i) => [u.key, i]));
                const unitReports = await Promise.all(
                    units.map(async (unit, i) => {
                        const deps = (unit.after || []).filter((k) => implDone.has(k) && keyIndex.get(k) < i);
                        await Promise.all(deps.map((k) => implDone.get(k).p));
                        let fix = SF.done[unit.key] || null; // baked: this unit's implement landed before the stop
                        if (!fix)
                            fix = await slot(() =>
                                agent(implementPrompt(folder, unit, arch.preSwap, reports, kernelReports, scopes), {
                                    label: 'impl:' + folder.key + ':' + unit.key,
                                    phase: 'Build',
                                    model: 'fable',
                                    effort: 'high',
                                    schema: FIXLOG,
                                    stallMs: STALL,
                                }),
                            ).catch(() => null);
                        implDone.get(unit.key).release(); // dependents launch even on failure; they compose current disk
                        if (!fix)
                            return {
                                unit: unit.key,
                                pages: unit.pages.length,
                                pageList: unit.pages,
                                verdict: 'failed',
                                deferred: [],
                                indexRows: [],
                                critReport: '',
                                rtLanded: false,
                            };
                        // Native fable critique: fixlog to disk via the recon native branch, receipt on the wire;
                        // the redteam reads the fixlog from disk and folds its rows forward, exactly as the codex lane did.
                        const crit = await slot(() =>
                            recon(critiquePrompt(folder, unit, scopes, navOf([fix])), {
                                label: 'crit:' + folder.key + ':' + unit.key,
                                phase: 'Build',
                                schema: FIXLOG,
                                nativeModel: 'fable',
                                stallMs: STALL,
                                scope: unit.pages,
                            }),
                        );
                        const critR = crit && crit.ok ? crit : null;
                        const rt = await slot(() =>
                            agent(redteamPrompt(folder, unit, scopes, navOf([fix]), critR), {
                                label: 'rt:' + folder.key + ':' + unit.key,
                                phase: 'Build',
                                model: 'fable',
                                effort: 'high',
                                schema: FIXLOG,
                                stallMs: STALL,
                            }),
                        ).catch(() => null);
                        return {
                            unit: unit.key,
                            pages: unit.pages.length,
                            verdict: (rt && rt.verdict) || (critR ? 'refined' : fix.verdict),
                            deferred: [fix, rt].filter(Boolean).flatMap((r) => r.deferred || []),
                            indexRows: [fix, rt].filter(Boolean).flatMap((r) => r.indexRows || []),
                            critReport: critR ? critR.report : '',
                            rtLanded: !!rt,
                        };
                    }),
                );
                // A folder where no unit landed has no tree to document; the run result carries the failure.
                const docs = unitReports.some((u) => u.verdict !== 'failed')
                    ? await slot(() =>
                          agent(
                              docsPrompt(
                                  folder,
                                  unitReports.map((u) => ({ unit: u.unit, pages: u.pages, verdict: u.verdict })),
                              ),
                              {
                                  label: 'docs:' + folder.key,
                                  phase: 'Build',
                                  model: 'opus',
                                  effort: 'high',
                                  schema: DOCS_SCHEMA,
                                  stallMs: STALL,
                              },
                          ),
                      )
                    : null;
                return {
                    folder: folder.name,
                    key: folder.key,
                    root: folder.root,
                    preSwap: arch.preSwap,
                    units: unitReports,
                    packageDeltas: arch.packageDeltas || [],
                    deferred: unitReports.flatMap((u) => u.deferred),
                    indexRows: unitReports.flatMap((u) => u.indexRows),
                    failedPages: unitReports.filter((u) => u.verdict === 'failed').flatMap((u) => u.pageList || []),
                    docs: docs ? docs.summary : 'dropped',
                };
            })
            .map((p) =>
                p.catch((e) => {
                    log('folder failed: ' + e.message);
                    return null;
                }),
            ),
    )
).filter(Boolean);

if (!results.length) {
    log('No folder landed - nothing to close');
    return { folders: [], law: 'skipped', acceptance: 'skipped' };
}

phase('Close');
const BACKLOG = results.flatMap((r) => r.deferred);
const ROWS = results.flatMap((r) => r.indexRows || []);
const ORPHANS = results.flatMap((r) => r.units.filter((u) => u.critReport && !u.rtLanded).map((u) => u.critReport));
log(
    'Close: ' +
        BACKLOG.length +
        ' deferred backlog row(s), ' +
        results.flatMap((r) => r.packageDeltas).length +
        ' package delta(s), ' +
        results.flatMap((r) => r.failedPages || []).length +
        ' failed page(s), ' +
        ORPHANS.length +
        ' orphaned critique fixlog(s)',
);
const accepts = await Promise.all(
    results.map((r) => slot(() => recon(acceptPrompt(r, ROWS), { label: 'accept:' + r.key, phase: 'Close', schema: REGISTER, scope: [r.root] }))),
);
const ACC_UNMAPPED = accepts.filter((a) => !a.ok).flatMap((a) => a.scope.map((sc) => ({ lane: a.lane, scope: sc })));
log(
    'Acceptance: ' +
        accepts.filter((a) => a.ok).reduce((n, a) => n + a.entries, 0) +
        ' finding(s) across ' +
        accepts.filter((a) => a.ok).length +
        '/' +
        accepts.length +
        ' lane(s)' +
        (ACC_UNMAPPED.length
            ? ' — FAILED: ' +
              accepts
                  .filter((a) => !a.ok)
                  .map((a) => a.lane)
                  .join(', ')
            : ''),
);
const law = await slot(() =>
    agent(lawPrompt(results, BACKLOG, accepts, ACC_UNMAPPED, ORPHANS), {
        label: 'law',
        phase: 'Close',
        model: 'fable',
        effort: 'high',
        schema: DOCS_SCHEMA,
        stallMs: STALL,
    }),
);

return {
    folders: results.map((r) => ({
        folder: r.folder,
        preSwap: r.preSwap,
        units: r.units.map((u) => ({ unit: u.unit, verdict: u.verdict })),
        packageDeltas: r.packageDeltas,
        failedPages: r.failedPages,
        docs: r.docs,
    })),
    backlog: BACKLOG.length,
    acceptance: accepts.map((a) => ({ lane: a.lane, ok: a.ok, entries: a.entries, headline: a.headline || a.failure })),
    law: law ? law.summary : 'dropped',
};
