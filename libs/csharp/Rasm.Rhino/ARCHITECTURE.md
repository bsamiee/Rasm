# [RASM_RHINO_ARCHITECTURE]

`Rasm.Rhino` maps the Rhino 9 host boundary over the RhinoCommon surfaces, the native Eto UI sub-domain, and the `Rhino.UI` shell, composing the `Rasm` kernel for every host-neutral computation. Each sub-domain folder maps to exactly one namespace, and project references terminate at the kernel. Host owners compose same-assembly owners at their own or lower stratum. Seam map names only boundary-crossing contracts — each a frozen-name value type consumed down from the kernel — while host-internal wiring stays on the mutation spine.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Rhino/             # Rhino host boundary over the Rasm kernel
├── Document/           # Host-document substrate under every host surface
│   ├── Session.cs      # Capability-scoped document-session demand, unit-regime adjustment, worksession custody
│   ├── Geometry.cs     # Native GeometryBase custody crossing and kernel transform
│   ├── Tables.cs       # Table mutation and redraw compensation
│   ├── Layers.cs       # Layer-tree topology, face and override programs, and the layer commit rail
│   └── Events.cs       # Event observation, the transactional DocumentStream, and the hook-point registry
├── Persistence/        # Typed serialization, settings custody, attached data, user text, saved-state presets
│   ├── Dictionary.cs   # ArchiveValue slot-registry carrier and the ArchiveMap detach/mint round trip
│   ├── Settings.cs     # Settings custody scopes, typed value rail, guards, and the change ledger
│   ├── AppSettings.cs  # Application preference families, theme and color slots, and the alias/shortcut/path registries
│   ├── UserData.cs     # ArchiveIo spine, TypedUserData template, roster census, custody transfer
│   ├── UserText.cs     # TextStore rail over document and per-object user strings
│   ├── Presets.cs      # CPlane, named-position, and layer-state preset rail under one mask vocabulary
│   └── Snapshots.cs    # Scripted snapshot ops and the SnapShotsClient participant
├── Objects/            # Live document-object domain over the table rail
│   ├── State.cs        # Live-object window: snapshot, frames, component touch, section custody, document analytics census
│   ├── Attributes.cs   # Typed attribute program feeding the table rail's Amend path
│   ├── Materials.cs    # Object materials, mappings, and mesh caches behind one commit
│   ├── Lights.cs       # Closed world light-kind family: seed, gated edits, and the table commit rail
│   ├── History.cs      # History record/replay triad, linkage topology, and governance
│   └── Authoring.cs    # Custom-object, grip, and render-mesh programs; ObjectsTelemetry egress, host taps, classification, instrument rows
├── Commands/           # Native command lifecycle, input acquisition, and picked-reference projection
│   ├── Command.cs      # Staged command algebra over one immutable model and its host adapter
│   ├── Acquisition.cs  # Parameterized input-acquisition matrix and its receipt
│   ├── Options.cs      # Command-line option vocabulary and leased native carriers
│   └── Selection.cs    # Picked-reference projection onto the selection union and re-entry
├── Blocks/             # Instance-definition domain over the kernel
│   ├── Model.cs        # Definitions.Lens resolution over the Document-owned ResourceRef and whole-state snapshot policy
│   ├── Graph.cs        # Definition-graph topology, queries, and archive closure
│   ├── Lifecycle.cs    # Definition ingress, the preview vault, deferred refresh, and eviction
│   └── Operations.cs   # Block operation and query rail, geometry intake, and receipts
├── Modeling/           # Host-fidelity native construction compute over the custody seam
│   ├── Solids.cs       # Brep boolean/fillet/offset/join rail, the ModelGate + Built spine, and the BenchBand evidence harvest
│   ├── Lofting.cs      # Sweep, loft, patch, and developable construction policies
│   ├── Surfaces.cs     # Freeform surface constructors with fit evidence
│   ├── Curves.cs       # Curve offset, refine, extend, split, and construction host ops
│   ├── Meshing.cs      # Parameter-carried meshing, remesh, booleans, and mesh edits
│   ├── SubD.cs         # SubD creation, crease authoring, and brep conversion
│   ├── Deform.cs       # Space morphs and the unroll/squish/unwrap flatteners
│   └── Projection.cs   # Make2D hidden-line, silhouette, and draft capture over the value frame
├── Annotation/         # Drafting annotation domain over the resource tables
│   ├── Style.cs        # StyleField schema, patch fold, override mint, and the DimStyle rail
│   ├── Text.cs         # Text and leader construction, run edits, field formulas, outlining
│   ├── Dimension.cs    # DimFamily row-table dimension family over one override algebra
│   ├── Hatch.cs        # Hatch construction and the pattern line-definition model
│   ├── Linetype.cs     # Stroke segment/shape/taper model and .lin interchange
│   └── Typeface.cs     # Face resolution and section-cut presentation resources
├── Viewport/           # Camera model, operation rail, capture spec, and motion pacing
│   ├── Camera.cs       # Camera-pose altitudes over the kernel vector frame
│   ├── Operations.cs   # Camera-operation union applied behind the viewport lease
│   ├── Capture.cs      # Capture plan, request cardinality, leased delivery, and run-rail bench timing
│   └── Motion.cs       # Host motion-pacing adapter over kernel timing
├── Display/            # Display-pipeline participation and renderer boundary
│   ├── Conduit.cs      # Conduit-pipeline algebra, display-mode participation, and the cull/draw veto hook mounts
│   ├── Draw.cs         # Two-backend mark union dispatched over the canvas
│   ├── Interaction.cs  # Pointer, gumball, and widget hooks folded onto fact streams
│   ├── Render.cs       # Render-job session, realtime engine participant, and scene change-queue reader
│   └── Modes.cs        # Display-mode appearance profile, mode policy, viewport assignment, and analysis attachment
├── Render/             # RDK content model and document render configuration
│   ├── Content.cs      # Content address, kind axis, change bracket, snapshot, hash, leased ingress
│   ├── Kinds.cs        # Material bridge, texture configuration, and environment bake
│   ├── Fields.cs       # One polymorphic field-value owner, declaration, binding, parameter routes
│   ├── Registry.cs     # Factory vocabulary, content operation rail, receipts, event stream
│   ├── Settings.cs     # Render-settings duality, sub-owner states, sun astronomy, edit rail
│   └── Mapping.cs      # Texture-mapping specs, evaluation, and per-object channel binding
├── Exchange/           # Document interchange and publication surface
│   ├── Formats.cs      # File-codec matrix: detection, filters, and dispatch
│   ├── Options.cs      # Per-format option dial family, shared axes, and host option minting
│   ├── Archive.cs      # Standalone archive programs over one detached File3dm lease
│   ├── Operations.cs   # Exchange-operation rail and headless convert sessions
│   ├── Sheets.cs       # Sheet plans, live selectors, and declarative detail state
│   └── Publish.cs      # Page-target dispatch and atomic content-keyed file landing
├── Eto/                # Native Eto UI framework sub-domain
│   ├── Platform.cs     # Ambient platform binding, native mount, and theme grid
│   ├── Runtime.cs      # Ambient runtime rails: dispatch, pulse, and projection
│   ├── Elements.cs     # Control tree, realize fold, layout algebra, themed editors, and fault band
│   ├── Binding.cs      # State-cell binding attachments and their receipt ledger
│   ├── Canvas.cs       # Drawable mount, paint-program seam, glyph shaping, and pixel leases
│   └── Chrome.cs       # Verb table projected into menus, windows, and dialogs
├── HostUi/             # Rhino.UI shell composed over the Eto sub-domain
│   ├── Shell.cs        # Host-thread session marshal, status, prompt, progress, runtime hosting, and notices
│   ├── Panels.cs       # Panel fact stream, placement, RUI state fold, and Rhino control rows
│   ├── Pages.cs        # Page realization, the signal spine, and kind-safe mounting
│   └── Dialogs.cs      # Capability-gated inquiry rail and preview projection
└── Plugin/             # Host plug-in binding, registry census, entitlement, and document participation
    ├── Lifecycle.cs    # Staged phase custody behind the one PlugIn derivation and its fault ledger
    ├── Census.cs       # Installed-plugin registry reads and the load and load-protection rail
    ├── Licensing.cs    # Zoo and CloudZoo entitlement rail and the plug-in acquisition arm
    └── Document.cs     # Per-plugin archive participation and the plug-in settings bridge
```

## [02]-[STRATA]

Five strata order the sub-domain folders; a folder composes its own owners and lower strata only, `Rasm` kernel namespaces underlie the whole boundary as the host-neutral floor, and two ruled counter-edges stand: Document's configured-open source takes Persistence's `ArchiveMap` as its typed open-options payload, minted before any session exists, and Modeling's projection frame takes Viewport's `CameraSnapshot`/`CameraPose` value shapes — value-only, no lease or borrow crossing. Every other consumption edge points down, so a new folder seats one stratum above its highest composed owner.

- S0 `Document` — spine under everything: the `DocumentSession` demand, `Tables.Commit`, `Layers.Commit`, the transactional `DocumentStream`.
- S0 reach — every sibling composes the spine.
- S1 single-seam — spine-alone composers: `Persistence` (`ArchiveMap`, `Settings`, `AppSettings`), `Commands` (`CommandVerdict`, `PickCapture`).
- S1 single-seam — `Blocks` (`BlockGraph`, `GraphFold`), `Modeling` (`ModelGate`, `Built<TSlot>`), `Annotation` (`StyleField`, `Styles`).
- S1 single-seam — `Render` (`ContentRef`, `Registry`, the `Settings` duality) composes the spine's commit and table rails alone.
- S1 single-seam — `Eto`: the `Element` realize fold and the `UiThread` floor.
- S1 law — Modeling reaches only the geometry-custody capsule and the ruled `CameraSnapshot`/`CameraPose` frame values.
- S1 law — Eto reaches the event-detach capsule and the `PluginKey` identity its process-global claims key on.
- S2 composite — `Objects` (`Objects`, `Attributes`, `Chronicle`) adds Commands, Blocks' `GraphProjection`, and Annotation's `LinetypeSource`.
- S2 composite — `HostUi` (`HostThread`, `PanelHost`, `HostPage`) adds the whole Eto sub-domain.
- S2 composite — Plugin (`RasmPlugIn`, `PluginCensus`, `LicenseRail`, `Participation`) composes Persistence's settings rail and `PluginKey` identity.
- S2 law — Plugin routes page-collection callbacks onto HostUi's `PageBasket`/`PageMount.Land`, a same-stratum peer edge, and mints no page owner.
- S3 `Viewport` — `ViewportLease`, `CameraPose`, `Cameras`, and `MotionPump`.
- S3 law — every borrow crosses the `HostThread` session rail, `HostThread.Run(HostWork<T>.Session(...))`, under a `SessionNeed`.
- S3 law — the capture run rail takes Modeling's `BenchEvidence`/`BenchBand` value shapes — value-only, no lease or borrow crossing.
- S4 terminal — `Display` (`Modes`, `Marks`) and `Exchange` (`Exchanges`, `Publishing`) compose Viewport's camera and capture rails.
- S4 law — Display draws through the Eto canvas and publishes conduit callback faults through Objects' `ObjectsTelemetry` egress.
- S4 law — Display composes Render's `EnvironmentRole`, dither owner, and `FailureLedger`/`RetentionPolicy` downward; nothing composes S4.
- S4 ruled edge — Display's render-window egress lands files through Exchange's `OutputPolicy`, a value-only seam with no lease or borrow crossing.

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
    accDescr: Five stacked strata from the terminal display and exchange composers through the viewport rail and the composite object and host-UI domains onto the single-seam domains and the document spine, every consumption edge downward and solid naming one sourced type, two dashed ruled counter-edges carrying the ArchiveMap open-options payload upward from the document spine to Persistence and the CameraSnapshot and CameraPose frame values upward from the Modeling gate to the viewport rail, and one forbidden upward edge.
    subgraph S4["S4 TERMINAL COMPOSERS"]
        Modes[Modes]
        Exchanges[Exchanges]
    end
    subgraph S3["S3 VIEWPORT"]
        Lease[ViewportLease]
        Capture[CaptureSink]
    end
    subgraph S2["S2 COMPOSITE"]
        Objects[Objects]
        HostThread[HostThread]
        Plugin[RasmPlugIn]
    end
    subgraph S1["S1 SINGLE-SEAM"]
        Picks[PickCapture]
        Blocks[GraphFold]
        Eto[UiThread]
        Archive[ArchiveMap]
        Model[ModelGate]
        Registry[ContentRef]
    end
    subgraph S0["S0 DOCUMENT"]
        Session[DocumentSession]
    end
    Modes e1@-->|"[IMPORT]: ViewportLease"| Lease
    Modes e13@-->|"[IMPORT]: ContentRef"| Registry
    Exchanges e2@-->|"[IMPORT]: CaptureSink"| Capture
    Registry e3@-->|"[IMPORT]: DocumentCommit"| Session
    Lease e4@-->|"[IMPORT]: HostThread"| HostThread
    Capture e5@-->|"[IMPORT]: SessionNeed"| S0
    Capture e14@-->|"[IMPORT]: BenchEvidence"| Model
    Objects e6@-->|"[IMPORT]: PickCapture"| Picks
    Objects e7@-->|"[IMPORT]: GraphFold"| Blocks
    HostThread e8@-->|"[IMPORT]: UiThread"| Eto
    Plugin e15@-->|"[IMPORT]: DocumentSession"| Session
    Plugin e16@-->|"[IMPORT]: ArchiveMap"| Archive
    Picks e9@-->|"[IMPORT]: DocumentSession"| Session
    Modes e12@-->|"[IMPORT]: ObjectsTelemetry"| Objects
    Session e10@-.->|"[COUNTER]: ArchiveMap"| Archive
    Model e11@-.->|"[COUNTER]: CameraSnapshot + CameraPose"| Lease
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
    accDescr: Rasm.Rhino host sub-domain owners consuming frozen-name value contracts down from the Rasm kernel, one boundary rail per consuming sub-domain.
    subgraph rhino[RASM.RHINO]
        Document[Document substrate]
        Commands[Command lifecycle]
        Blocks[Block domain]
        Modeling[Modeling gate]
        Annotation[Drafting annotation]
        Viewport[Viewport rail]
        Display[Display composers]
        Render[Render content]
        Exchange[Exchange surface]
        Eto[Eto UI]
        Plugin[Plugin binding]
    end
    Rasm([Rasm])
    PyData([python:data])
    TsData([typescript:data])
    Rasm e1@-->|"[BOUNDARY]: ModelUnit + Context + AnalysisQuery + Placement + Requirement + Lease + HookPoint + InstrumentSpec + Dimension + PerceptualColor"| Document
    Rasm e2@-->|"[BOUNDARY]: VectorFrame"| Viewport
    Rasm e3@-->|"[BOUNDARY]: AnalysisQuery"| Commands
    Rasm e4@-->|"[BOUNDARY]: PerceptualColor"| Eto
    Rasm e5@-->|"[BOUNDARY]: MonotonicTimeline"| Viewport
    Rasm e6@-->|"[BOUNDARY]: VectorIntent"| Viewport
    Rasm e7@-->|"[BOUNDARY]: Context"| Modeling
    Rasm e8@-->|"[BOUNDARY]: ContentHash"| Blocks
    Rasm e9@-->|"[BOUNDARY]: PerceptualColor"| Display
    Rasm e14@-->|"[BOUNDARY]: AnalysisQuery"| Display
    Rasm e10@-->|"[BOUNDARY]: PerceptualColor"| Render
    Rasm e11@-->|"[BOUNDARY]: PerceptualColor + Context + SpringShape + DecayShape + ViewPose"| Viewport
    Rasm e12@-->|"[BOUNDARY]: PerceptualColor + ModelUnit + Lease + ValidationError"| Annotation
    Rasm e13@-->|"[BOUNDARY]: Context"| Blocks
    Rasm e15@-->|"[BOUNDARY]: ModelUnit + ContentHash + Dimension + UnitInterval + PerceptualColor + EpsilonPolicy"| Exchange
    Rasm e16@-->|"[BOUNDARY]: Lease"| Plugin
    Document e17@-->|"[WIRE]: OrganizationWire"| PyData
    Document e18@-->|"[WIRE]: OrganizationWire"| TsData
```

Every kernel contract is a frozen-name value type the host binds and never re-mints — one `[BOUNDARY]` rail per consuming sub-domain, each carrying the exact member set its owner consumes. Document's rail carries the full set: `ModelUnit`, `Context`, `AnalysisQuery`/`Analyze`, `TransformSpec`/`Placement.Build`, `Requirement.ForKind`/`KindOf`, `Lease<T>`/`GeometryForm`, `HookModality`/`HookId`/`HookPoint`, `InstrumentSpec`/`TelemetryContributorPort`/`MeasureForm`, `PerceptualColor`, and `Dimension` with `AbsoluteTolerance`/`RelativeTolerance`/`AngleTolerance` — `AnalysisQuery` rides the Document, Commands, and Display rails, each end consuming it live — Display's `AnalysisOverlay` drives the registered false-colour mode off `Analyze.In(...).Run` at `Display/conduit#OVERLAYS`. `PerceptualColor` is the one colour crossing on every rail that carries it: a `System.Drawing.Color` admits at the boundary through `OfRgb` and leaves through `ToRgb`, never riding a public detached payload. Kernel source is host-neutral and consumes nothing back, so the strata-locked dependency is source-only by construction; the kernel seam registry mirrors each edge from its producing side. One wire leaves the boundary outward: `Document/layers#ORGANIZATION_PROJECTION` emits the `rasm.organization.v1` `OrganizationWire` document — `EntityWire` rows keyed by the content-addressed organizational address, `ContainmentWire` edges discriminated by target key space, `ViewOverrideWire` probe rows — which `python:data` folds at `graph/graph#TOPOLOGY` and `typescript:data` lands at `read/query#ORGANIZATION_ROWS`. Names on that wire state the host-free organizational concept and the Rhino layer vocabulary translates at the projection, so no host `Guid`, table index, or joined path crosses; `tests/contracts/MANIFEST.md` `ORGANIZATION_WIRE` owns the schema and the fact identity.

## [04]-[INTERNAL]

Every host mutation walks one path — no sub-domain opens the document directly, the one carve being the worksession attach/detach rail, whose reference-set change Rhino's undo stack does not record and which therefore compensates through its declared per-verb inverse instead of the sealed envelope. Document-session demand gates capability, the shared `DocumentCommit.Sealed` envelope frames the change over `UndoBracket`, the sub-domain executor runs inside it, and the sealing commit lands the typed receipt with redraw compensation; a denied demand and every mid-stage fault converge on the one rail that still releases the bracket. Exact per-stage wiring lives on the owning implementation pages.

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
    accDescr: The once-walked host mutation path from a request through the document-session demand and a capability gate into the UndoBracket, the sub-domain executor, and the sealing commit, with every stage fault converging on one fault rail that still releases the bracket.
    Request([Host request]) e1@--> Session[[DocumentSession demand]]
    Session e2@--> Ready{Capability held?}
    Ready e3@-->|"capability held"| Bracket[[UndoBracket]]
    Ready f1@-->|"demand denied"| Fault[/Fault rail/]
    Bracket e4@--> Executor[[Sub-domain op]]
    Executor e5@--> Commit[[DocumentCommit.Sealed]]
    Commit e6@--> Redraw[Redraw compensation]
    Redraw e7@--> Ledger[(Typed receipt)]
    Ledger e8@--> Settle([Settle])
    Session f2@-.->|"demand fault"| Fault
    Executor f3@-.->|"op fault"| Fault
    Commit f4@-.->|"commit fault"| Fault
    Fault f5@-->|"unconditional release"| Settle
```

## [05]-[NAMESPACES]

Namespace mirrors folder path — `.editorconfig` sets `dotnet_style_namespace_match_folder = true:error`, so every fence under `Rasm.Rhino/<Folder>/` declares `namespace Rasm.Rhino.<Folder>;` and the `[01]` codemap folders are the namespace roots verbatim.

Boundary compiles as ONE assembly — the single `Rasm.Rhino.csproj` — so internal members cross namespaces with no build edge, and the project references only `Rasm.csproj`. Kernel-neutral value types compose freely from the kernel, while a live host handle, a native carrier, or a `System.Drawing` screen struct never crosses out of the sub-domain that leases it.

Host-name resolution is one law: inside `Rasm.Rhino.*` a partial qualification re-resolves against the boundary's namespaces (`Rhino.UI.X` binds `Rasm.Rhino`), so fences name host members BARE — each `[RUNTIME_PRELUDE]` imports its host namespaces ahead of the file-scoped namespace declaration, resolving at global scope, and `Rasm.Rhino.csproj` carries the same rows as project-level usings. Host types the prelude cannot reach unshadowed spell `global::` in full, and a host simple-name collision resolves through one csproj `<Using Alias="..." />` row the branch rulings own ONLY when one winner serves the whole assembly; a homonym whose winner differs per sub-domain — `Dimension` (kernel measure vs host annotation base), `Color` (Eto paint vs GDI carrier) — spells fully qualified at every colliding site, because a project alias silently rebinds the other folder's bare reads.
