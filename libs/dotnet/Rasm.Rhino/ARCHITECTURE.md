# [RASM_RHINO_ARCHITECTURE]

`Rasm.Rhino` maps the Rhino 9 host boundary over the RhinoCommon surfaces, the `Rhino.UI` shell, and the kernel `Rasm.Interaction` band, composing the `Rasm` kernel for every host-neutral and Eto-shaped concern, so the boundary declares no Eto twin. Each sub-domain folder maps to one namespace, and project references terminate at the kernel. Host owners compose same-assembly owners at their own or lower stratum. Seam map names only boundary-crossing contracts, each a frozen-name value type consumed down from the kernel, while host-internal wiring stays on the mutation spine.

## [01]-[DOMAIN_MAP]

```text
Rasm.Rhino/             # Rhino host boundary over the Rasm kernel
├── Document/           # Host-document substrate under every host surface
│   ├── Session.cs      # Capability-scoped document-session demand, unit-regime adjustment, worksession custody
│   ├── Geometry.cs     # GeometryHandle retained custody: deep-copy mutation commits, in-quarantine failed disposals
│   ├── Tables.cs       # TableKind/TableTarget vocabulary and sealed mutation programs
│   ├── Events.cs       # Event observation, the transactional DocumentStream, and the hook-point registry
│   ├── Layers.cs       # Managed layer tree with persistent visibility variants; the drafting-standards lowering seat
│   ├── Commit.cs       # One host-mutation commit envelope; every folder commit rail enters here and none seals its own
│   └── Lifetime.cs     # Package-wide lifetime primitives: claims capsule, symmetric subscription rollback, failure-release fold
├── Persistence/        # Typed serialization, settings custody, attached data, user text, saved-state presets
│   ├── Dictionary.cs   # ArchiveValue one boxed-host-value carrier; every KV payload admits through one gate
│   ├── Settings.cs     # Settings custody scopes, typed value rail, guards, and the change ledger
│   ├── AppSettings.cs  # AppSettingsFamily rows with capture, default, apply, and reset delegates; AppState snapshots
│   ├── UserData.cs     # ArchiveIo spine, TypedUserData template, roster census, custody transfer
│   ├── UserText.cs     # TextMutationBatch writes and TextQuery detached reads
│   ├── Presets.cs      # Sub-domain admission-refusal family mint; the construction-plane vocabulary presets and viewport share
│   └── Snapshots.cs    # Scripted snapshot ops and the SnapShotsClient participant
├── Objects/            # Live document-object domain over the table rail
│   ├── State.cs        # StateAsk closed read family, Touch selection mutation, TableAddress composition
│   ├── Attributes.cs   # AttributeEdit closing every writable ObjectAttributes family with verified payload carriers
│   ├── Materials.cs    # MaterialScope overload discriminant; MaterialAsk<TAnswer> self-typed reads fix their own answers
│   ├── Lights.cs       # LightKind capability rows, LightSeed one construction union, LightEdit gated edit family
│   ├── History.cs      # HistoryScript generated slots into leased HistoryRecords; Regrown replays, ReplayProgram composition
│   └── Authoring.cs    # Custom-object, grip, and render-mesh programs; ObjectsTelemetry egress, host taps, classification, instrument rows
├── Commands/           # Native command lifecycle, input acquisition, and picked-reference projection
│   ├── Command.cs      # CommandFlow<TState>.Drive bounded interpretation; RasmCommand<TSelf,TState> the one host entry
│   ├── Acquisition.cs  # Parameterized input-acquisition matrix and its outcome
│   ├── Options.cs      # OptionSet.Bind getter-window vocabulary; OptionLease native custody through deterministic release
│   └── Selection.cs    # Picks projection with terminal ObjRef windows; owned references dispose, borrowed never
├── Blocks/             # Instance-definition domain over the kernel
│   ├── Model.cs        # Live block address, LinkState carrier, closed mutation and preview policy values
│   ├── Graph.cs        # Definition-graph topology, queries, and archive closure
│   ├── Lifecycle.cs    # Preview bitmap custody, versioned grants, document-scoped invalidation, deterministic disposal
│   └── Operations.cs   # Block operation and query rail with geometry intake
├── Modeling/           # Host-fidelity native construction compute over the custody seam
│   ├── Solids.cs       # SolidOp family and the Extrusion lifecycle through Solids.Build over leased ModelGate borrows
│   ├── Lofting.cs      # LoftOp admission through the spine's ModelClaim fold; rails, profiles, constraints, developable products
│   ├── Surfaces.cs     # FreeformOp union through HostSurfaces.Build: network fits, rail revolves, grid interpolation
│   ├── Curves.cs       # CurveOp union through one build entry; pulls, projections, booleans, blend construction cases
│   ├── Meshing.cs      # HostMeshes.Build admitted creation and egress; MeshOp.QuadRemesh the sole mesh-to-SubD seam
│   ├── SubD.cs         # SubDOp raw-handle admission through ModelClaim; value-semantic edits, crease topology, Brep egress
│   ├── Deform.cs       # DeformOp driver admission through ModelClaim; generated policies own every native knob
│   └── Projection.cs   # ProjectionOp union through Projections.Build; ProjectionFrame admits the value frame once
├── Annotation/         # Drafting annotation domain over the resource tables
│   ├── Style.cs        # StyleField rows pairing exact payloads with catalogued DimensionStyle.Field entries; one patch fold
│   ├── Text.cs         # TextSeed/TextSpec/LeaderSpec one admission; RunFormat decoration edits, FieldKind evaluator space
│   ├── Dimension.cs    # DimensionSpec one construction admission; DimAdjust kind-matched refits on the drafting spine
│   ├── Hatch.cs        # PatternDef detached round trip, HatchSpec boundary-family construction, Hatches.Commit fold
│   ├── Linetype.cs     # StrokeDef authorable aggregate over SegmentRow atoms; Linetypes.Commit on the drafting spine
│   └── Typeface.cs     # FaceDecoration/FaceTrait capability rosters, FaceQuery one admission, FaceInfo detached descriptor
├── Viewport/           # Camera model, operation rail, capture spec, and motion pacing
│   ├── Camera.cs       # CameraPose kernel-vector composition; session-scoped borrows over the ViewportTarget address
│   ├── Operations.cs   # Camera-operation union applied behind the viewport lease
│   ├── Capture.cs      # Capture plan, request cardinality, leased delivery, and run-rail bench timing
│   └── Motion.cs       # MotionPump host lease over the kernel sampling algebra it re-declares nothing of
├── Display/            # Display-pipeline participation and renderer boundary
│   ├── Conduit.cs      # Conduit-pipeline algebra, display-mode participation, and the cull/draw veto hook mounts
│   ├── Draw.cs         # Marks.Paint one dispatch over four canvases; DisplayMark screen and world payload bands
│   ├── Interaction.cs  # Pointers/Gumballs/WidgetHost configure entries; host input admitted once, bounded facts out
│   ├── Render.cs       # Render-job session, realtime engine participant, and scene change-queue reader
│   └── Modes.cs        # Modes.Configure request algebra; raw host editors stay inside the fold, viewport binding by value
├── Render/             # RDK content model and document render configuration
│   ├── Content.cs      # ContentRef live RDK graph identity; scoped mutation, detached topology, replayable hash evidence
│   ├── Kinds.cs        # Material bridge, texture configuration, and environment bake
│   ├── Fields.cs       # One polymorphic field-value owner, declaration, binding, parameter routes
│   ├── Registry.cs     # ContentUuidCatalog seed data, ContentSerializer read transfer, Registry.Run registration close
│   ├── Settings.cs     # Render-settings duality, sub-owner states, sun astronomy, edit rail
│   └── Mapping.cs      # MappingSpec construction with recoverable primitive evidence; Mappings.Run one request family
├── Exchange/           # Document interchange and publication surface
│   ├── Formats.cs      # File-codec matrix: detection, filters, and dispatch
│   ├── Options.cs      # FormatDial per-codec policy; Admit proves codec-phase correspondence once, Dials.Resolve applies
│   ├── Archive.cs      # Archives.Apply File3dm admission and bounded materialization; one lease holds every handle
│   ├── Operations.cs   # Exchange-operation rail and headless convert sessions
│   ├── Sheets.cs       # Sheet plans, live selectors, and declarative detail state
│   └── Publish.cs      # Page-target dispatch and atomic content-keyed file landing
├── HostUi/             # Rhino.UI shell composed over the kernel Interaction band
│   ├── Shell.cs        # Host-thread session marshal, status, prompt, progress, runtime hosting, and notices
│   ├── Panels.cs       # Panel fact stream, placement, RUI state fold, and Rhino control rows
│   ├── Pages.cs        # HostPage realization from PagePlan rows; host base classes stay behind internal leaves
│   └── Dialogs.cs      # Capability-gated inquiry rail and preview projection
└── Plugin/             # Host plug-in binding, registry census, entitlement, and document participation
    ├── Lifecycle.cs    # RasmPlugIn the one PlugIn derivation and load root; quarantined host subclassing, fault ledger
    ├── Census.cs       # PluginCensus.Ask one polymorphic registry read; identity, descriptors, protection, installed roll
    ├── Licensing.cs    # Licenses.Ask entitlement union; RasmPlugIn.Entitlement the acquisition arm, typed verdicts
    └── Document.cs     # Participation.Cross write/read callback carrier; PluginSettings.Commit the settings-rail bridge
```

## [02]-[STRATA]

Five strata order the sub-domain folders; a folder composes its own owners and lower strata only, `Rasm` kernel namespaces underlie the whole boundary as the host-neutral floor, and two ruled counter-edges stand: Document's configured-open source takes Persistence's `ArchiveMap` as its typed open-options payload, minted before any session exists, and Modeling's projection frame takes Viewport's `CameraSnapshot`/`CameraPose` value shapes, value-only with no lease or borrow crossing. Every other consumption edge points down, so a new folder seats one stratum above its highest composed owner.

- S0 `Document` — spine under everything: `DocumentSession` demand, `Tables.Commit`, `Layers.Commit`, and the transactional `DocumentStream`.
- S1 single-seam — every S1 folder composes the spine ALONE, so any one deletes without moving a sibling; the single seam is the rank's test.
- S1 map — fence node to folder: `PickCapture` Commands, `GraphFold` Blocks, `ArchiveMap` Persistence, `ModelGate` Modeling, `ContentRef` Render.
- S1 absent edge — `Annotation` composes the spine with no discriminating import, so the fence draws it nowhere.
- S1 law — `Modeling` reaches only the geometry-custody capsule; its camera read is the ruled counter-edge, never a lease.
- S1 law — kernel `Rasm.Interaction` (`UiThread`, `ControlForge`, `IntentTable`, `Surface`) floors every shell surface, adapted by nothing here.
- S2 composite — `Objects` (`Objects`, `Attributes`, `Chronicle`) adds Commands, Blocks' `GraphProjection`, and Annotation's `LinetypeSource`.
- S2 composite — `HostUi` (`HostThread`, `PanelHost`, `HostPage`) composes the kernel `Rasm.Interaction` band directly.
- S2 composite — Plugin (`RasmPlugIn`, `PluginCensus`, `Licenses`, `Participation`) composes Persistence's settings rail and `PluginKey` identity.
- S2 law — Plugin routes page-collection callbacks onto HostUi's `PageBasket`/`PageMount.Land`, a same-stratum peer edge, and mints no page owner.
- S3 `Viewport` — the lease-custody rank: every camera read and capture crosses the leased pair, and no lower folder holds a lease.
- S3 law — every borrow crosses the `HostThread` session rail, `HostThread.Run(HostWork<T>.Session(...))`, under a `SessionNeed`.
- S3 law — the capture run rail takes Modeling's `BenchEvidence`/`BenchBand` value shapes — value-only, no lease or borrow crossing.
- S3 law — `Viewport/camera` reads Persistence's `CPlaneGrid`/`CPlanePalette` value shapes downward — value-only, one seat.
- S4 law — Display draws through the kernel `Interaction/paint` surface and publishes conduit faults through Objects' `ObjectsTelemetry` egress.
- S4 law — Display composes Render's `EnvironmentRole`, dither owner, and `FailureLedger`/`RetentionPolicy` downward; nothing composes S4.
- S4 ruled edge — every delivery leg is a CASE of `Exchange/publish`'s `[Union] Landing` built ARMS-UP; lower pages keep preparation values alone.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TB
    accTitle: Rasm.Rhino interior strata
    accDescr: Interior strata down to the document spine; dashed counter-edges carry ArchiveMap options and the camera frame values upward.
    subgraph S4["S4 TERMINAL COMPOSERS"]
        Modes[Modes]
        Exchanges[Exchanges]
    end
    subgraph S3["S3 VIEWPORT"]
        Lease[ViewportLease]
        Capture[Captures]
    end
    subgraph S2["S2 COMPOSITE"]
        Objects[Objects]
        HostThread[HostThread]
        Plugin[RasmPlugIn]
    end
    subgraph S1["S1 SINGLE-SEAM"]
        Picks[PickCapture]
        Blocks[GraphFold]
        Archive[ArchiveMap]
        Model[ModelGate]
        Registry[ContentRef]
    end
    subgraph S0["S0 DOCUMENT"]
        Session[DocumentSession]
    end
    Modes e1@-->|"[IMPORT]: ViewportLease"| Lease
    Modes e2@-->|"[IMPORT]: ContentRef"| Registry
    Exchanges e3@-->|"[IMPORT]: Captures.Stage"| Capture
    Registry e4@-->|"[IMPORT]: DocumentCommit"| Session
    Lease e5@-->|"[IMPORT]: HostThread"| HostThread
    Capture e6@-->|"[IMPORT]: SessionNeed"| Session
    Capture e7@-->|"[IMPORT]: BenchEvidence"| Model
    Objects e8@-->|"[IMPORT]: PickCapture"| Picks
    Objects e9@-->|"[IMPORT]: PartIndex"| Picks
    Objects e10@-->|"[IMPORT]: GraphFold"| Blocks
    Plugin e11@-->|"[IMPORT]: DocumentSession"| Session
    Plugin e12@-->|"[IMPORT]: ArchiveMap"| Archive
    Picks e13@-->|"[IMPORT]: DocumentSession"| Session
    Modes e14@-->|"[IMPORT]: ObjectsTelemetry"| Objects
    Session e15@-.->|"[COUNTER]: ArchiveMap"| Archive
    Model e16@-.->|"[COUNTER]: CameraSnapshot"| Lease
    Session f1@-->|"forbidden: spine upward"| S4
```

## [03]-[SEAMS]

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Rasm.Rhino kernel-boundary seams
    accDescr: Which kernel owner hands which frozen-name contracts to each Rhino sub-domain, and which wires the boundary emits to peers.
    subgraph rhino[RASM.RHINO]
        Document[Document substrate]
        Persistence[Persistence custody]
        Objects[Object rails]
        Commands[Command lifecycle]
        Blocks[Block domain]
        Modeling[Modeling gate]
        Annotation[Drafting annotation]
        Viewport[Viewport rail]
        Display[Display composers]
        Render[Render content]
        Exchange[Exchange surface]
        HostUi[HostUi shell]
        Plugin[Plugin binding]
    end
    subgraph rasm[RASM]
        Domain([Domain floor])
        Numerics([Numerics floor])
        Spatial([Spatial fields])
        Meshing([Mesh lattice])
        Parametric([Parametric producers])
        Processing([Processing rail])
        Drawing([Drawing producers])
        Analysis([Analysis entry])
        Interaction([Interaction plane])
    end
    PyData([python:data])
    PyGeometry([python:geometry])
    TsData([typescript:data])
    Domain e1@-->|"[BOUNDARY]: ContentHash + Context + Lease + ModelUnit + Requirement"| Document
    Domain e2@-->|"[PORT]: InstrumentSpec"| Document
    Domain e3@-->|"[BOUNDARY]: Lease"| Persistence
    Domain e4@-->|"[BOUNDARY]: ContentHash + Context + Lease + ModelUnit"| Objects
    Domain e5@-->|"[BOUNDARY]: HookRail + Lease + ModelUnit"| Commands
    Domain e6@-->|"[BOUNDARY]: ContentHash + Context + Lease + ModelUnit"| Blocks
    Domain e7@-->|"[BOUNDARY]: Context + Lease"| Modeling
    Domain e8@-->|"[BOUNDARY]: Context + Lease + ModelUnit + Requirement"| Annotation
    Domain e9@-->|"[BOUNDARY]: Context + Lease + ModelUnit"| Viewport
    Domain e10@-->|"[BOUNDARY]: ContentHash + Context + Lease + ModelUnit"| Display
    Domain e11@-->|"[BOUNDARY]: Lease + ModelUnit"| Render
    Domain e12@-->|"[BOUNDARY]: ContentHash + Lease + ModelUnit"| Exchange
    Domain e13@-->|"[BOUNDARY]: Lease + ModelUnit"| HostUi
    Domain e14@-->|"[BOUNDARY]: Lease"| Plugin
    Numerics e15@-->|"[BOUNDARY]: Dimension + PerceptualColor + Placement + UnitInterval"| Document
    Numerics e16@-->|"[BOUNDARY]: Dimension + PerceptualColor"| Persistence
    Numerics e17@-->|"[BOUNDARY]: PerceptualColor + UnitInterval + VectorCone"| Objects
    Numerics e18@-->|"[BOUNDARY]: Dimension + PerceptualColor"| Commands
    Numerics e19@-->|"[BOUNDARY]: Dimension"| Blocks
    Numerics e20@-->|"[BOUNDARY]: Placement"| Modeling
    Numerics e21@-->|"[BOUNDARY]: Dimension + PerceptualColor"| Annotation
    Numerics e22@-->|"[BOUNDARY]: Dimension + UnitInterval + VectorFrame"| Viewport
    Numerics e23@-->|"[BOUNDARY]: Dimension + PerceptualColor + UnitInterval"| Display
    Numerics e24@-->|"[BOUNDARY]: Dimension + PerceptualColor"| Render
    Numerics e25@-->|"[BOUNDARY]: Dimension + EpsilonPolicy + PerceptualColor + UnitInterval"| Exchange
    Numerics e26@-->|"[BOUNDARY]: Dimension + PerceptualColor + UnitInterval"| HostUi
    Numerics e27@-->|"[BOUNDARY]: Dimension"| Plugin
    Spatial e28@-->|"[CONTENT_KEY]: GeometryHash"| Display
    Meshing e29@-->|"[WIRE]: MeshSpace"| Display
    Parametric e30@-->|"[BOUNDARY]: MonotonicTimeline"| Modeling
    Parametric e31@-->|"[BOUNDARY]: MonotonicTimeline + MotionDrive"| Viewport
    Parametric e32@-->|"[BOUNDARY]: MonotonicStamp + MonotonicTimeline"| Display
    Parametric e33@-->|"[BOUNDARY]: MonotonicTimeline"| Exchange
    Parametric e34@-->|"[BOUNDARY]: MonotonicStamp + MonotonicTimeline"| HostUi
    Parametric e35@-->|"[BOUNDARY]: MonotonicTimeline"| Plugin
    Processing e36@-->|"[BOUNDARY]: VectorIntent"| Viewport
    Drawing e37@-->|"[BOUNDARY]: LayerName + LineWidth"| Document
    Drawing e38@-->|"[BOUNDARY]: LineWidth + SheetSize"| Annotation
    Drawing e39@-->|"[BOUNDARY]: LineWidth + SheetSize + ViewPose"| Viewport
    Drawing e40@-->|"[BOUNDARY]: LineWidth + SheetSize"| Exchange
    Drawing e41@-->|"[WIRE]: EncodedGeometry"| Display
    Analysis e42@-->|"[BOUNDARY]: AnalysisQuery"| Document
    Analysis e43@-->|"[BOUNDARY]: AnalysisQuery"| Commands
    Analysis e44@-->|"[BOUNDARY]: AnalysisQuery"| Display
    Interaction e45@-->|"[BOUNDARY]: UiDispatch"| Document
    Interaction e46@-->|"[BOUNDARY]: UiDispatch"| Viewport
    Interaction e47@-->|"[BOUNDARY]: Mark + PaintProgram"| Display
    Interaction e48@-->|"[BOUNDARY]: AssetOrigin + ControlSpec + IntentTable"| HostUi
    Interaction e49@-->|"[BOUNDARY]: AssetOrigin + UiDispatch"| Plugin
    Document e50@-->|"[WIRE]: Organization"| PyData
    Document e51@-->|"[WIRE]: Organization"| TsData
```

Every kernel contract is a frozen-name value type the host binds and never re-mints, so a kernel shape reached only as a case payload of an already-registered carrier rides that carrier's edge and mints none of its own. Kernel source is host-neutral and consumes nothing back, so the strata-locked dependency is source-only by construction.

Fence law is census, never roster: one edge per kernel owner, consuming sub-domain, and kind, each member DECLARED at that kernel owner's fences and SPELLED in the sub-domain's code fences, joined ` + ` alphabetically. Kernel-end edges fold this fence per owner, boundary, and kind, so a member added or retired here moves exactly one edge at each end under the branch `[04]-[STRUCTURE]` derivation row.

- `Op` is the rail key every fence takes and rides no seam edge, as on every other kernel seam; `Lease<T>` and `HookRail` cross as declared shapes.
- `Dimension` here is `Rasm.Numerics.Dimension` spelled in full; the bare host `Rhino.Geometry.Dimension` on the Annotation pages is no crossing.
- Seam `Placement` names the kernel transform builder reached through `Placement.Build`; `Blocks/model` owns its separate block-instance union.
- `InstrumentSpec` rides `[PORT]` because the boundary DECLARES rows the app root mounts; every other kernel member crosses as a bound value.
- `AnalysisQuery` rides the Document, Commands, and Display rails — `AnalysisOverlay` drives false-colour off `Analyze.In(...).Run`.
- `PerceptualColor` is the one colour crossing on every rail carrying it — `System.Drawing.Color` admits through `OfRgb` and leaves through `ToRgb`.
- `Document/layers#ORGANIZATION_PROJECTION` emits the recursive `organization.Organization` forest folded by Python and TypeScript data peers.
- Wire names state the host-free organizational concept and the layer vocabulary translates at the projection — no host `Guid` or path crosses.
- One emitter owns the whole descriptor — Objects composes the `Render/settings#SUN_ASTRONOMY` band downward, so nothing mints half a capture.
- Sun angles cross solved and the consumer grades declared fidelity; identity crosses RFC-4122 big-endian and spectra scene-linear on `SceneMap`.

## [04]-[INTERNAL]

Every host mutation walks one path: no sub-domain opens the document directly, the one carve being the worksession attach/detach rail, compensating through its declared per-verb inverse since Rhino's undo stack does not record it.

Document-session demand gates capability, `DocumentCommit.Sealed` frames the change over `UndoBracket`, the sub-domain executor runs inside it, and the sealing commit lands the typed result with redraw compensation; a denied demand and every mid-stage fault converge on the one rail that still releases the bracket.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Rasm.Rhino host-mutation spine
    accDescr: Which gate, bracket, and sealing owners one host mutation crosses, and the one releasing rail every fault converges on.
    Request([Host request]) e1@--> Session[[DocumentSession demand]]
    Session e2@--> Ready{Capability held?}
    Ready e3@-->|"capability held"| Bracket[[UndoBracket]]
    Bracket e4@--> Executor[[Sub-domain op]]
    Executor e5@--> Commit[[DocumentCommit.Sealed]]
    Commit e6@--> Redraw[Redraw compensation]
    Redraw e7@--> Result[(Typed result)]
    Result e8@--> Settle([Settle])
    Ready f1@-.->|"demand denied"| Fault[/Fault rail/]
    Session f2@-.->|"demand fault"| Fault
    Executor f3@-.->|"op fault"| Fault
    Commit f4@-.->|"commit fault"| Fault
    Fault f5@-->|"unconditional release"| Settle
```

## [05]-[BOUNDARIES]

- `Rasm.Rhino` owns the Rhino 9 host-boundary surface whole and re-owns no kernel concern; project references terminate at `Rasm`.
- Live host handles, native carriers, and `System.Drawing` screen structs stay inside the leasing sub-domain; kernel-neutral values alone cross.
- App roots alone compose the plug-in shell: `RasmPlugIn` is the load root, and no page binds hosting, DI, or telemetry providers beneath it.
- Peer packages consume this boundary's host-free value shapes through the seam registry; no peer references `Rasm.Rhino` and it references none.

## [06]-[NAMESPACES]

Namespace mirrors folder path: `.editorconfig` sets `dotnet_style_namespace_match_folder = true:error`, so every fence under `Rasm.Rhino/<Folder>/` declares `namespace Rasm.Rhino.<Folder>;` and the `[01]` codemap folders are the namespace roots verbatim.

Boundary compiles as ONE assembly, the single `Rasm.Rhino.csproj`, so internal members cross namespaces with no build edge, and the project references only `Rasm.csproj`.

Host-name resolution is one law: inside `Rasm.Rhino.*` a partial qualification re-resolves against the boundary's namespaces, so fences name host members BARE, with each `[RUNTIME_PRELUDE]` importing its host namespaces at global scope, matched by csproj usings. Unreachable host types spell `global::` in full; a simple-name collision takes one `<Using Alias>` row only where one winner serves the whole assembly, and a homonym whose winner differs per sub-domain spells fully qualified, since a project alias rebinds the other folder's bare reads.
