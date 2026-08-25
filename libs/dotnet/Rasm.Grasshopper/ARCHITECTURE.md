# [RASM_GRASSHOPPER_ARCHITECTURE]

`Rasm.Grasshopper` maps the Grasshopper 2, Eto, and Rhino UI host boundary on the .NET app strata: each sub-domain folder maps to exactly one namespace, and one owner closes each host concern over the live GH2 and Eto surfaces. It references the `Rasm` kernel and no sibling, so host-agnostic kernel math composes the motion and colour surfaces rather than a second in-folder derivation.

## [01]-[DOMAIN_MAP]

```text
Rasm.Grasshopper/       # Refs ../Rasm ONLY; GH2 + Eto host boundary; kernel math composed, never re-derived
├── Canvas/             # Paint, wire, layout, motion, and interaction owners over the live GH2 canvas
│   ├── Canvas.cs       # Canvas command-and-projection boundary over the live host surface
│   ├── Interaction.cs  # Responder dispatch, mount/focus/menu leases, and drag/resize capsules
│   ├── Layout.cs       # CanvasArrangement cases and typed snap capsules over the host solver surfaces
│   ├── Motion.cs       # Span/pace tween drive, animated glyphs, and the shared canvas pacer lease
│   ├── Paint.cs        # Event-scoped paint scene, frame/mark plans, stock custody, and pigment egress
│   └── Wires.cs        # Wire route geometry, custom routes, pick, marquee select, and the skin pen pass
├── Components/         # Component authoring, pin catalog, data transfer, attribute chrome, native catalog
│   ├── Attributes.cs   # Chrome event/decision policy, bounded trace, and the resizable chrome host spine
│   ├── Component.cs    # ComponentSpec consumed unchanged by construction, execution, lifecycle, and catalogue admission
│   ├── Data.cs         # One transfer policy over IDataAccess; typed ingress rows and the Garden promotion algebra
│   ├── Objects.cs      # Native-object factories, persisted read/assign, timer/cluster maps
│   └── Ports.cs        # PortRow rows owning verified carrier, semantic family, capability axes, one PortBinding each
├── Document/           # Graph transaction spine, query/wire operator, undo ledger, solution controller
│   ├── Document.cs     # Graph transaction spine over inert/inactive/active minting tiers, one gate
│   ├── Graph.cs        # Graph query-and-mutate operator, each mutation sealed into the ledger
│   ├── History.cs      # One ledger owner over the host History tree; ActionList seals under one VerbNoun
│   └── Solution.cs     # One solution owner over the host SolutionServer in every posture the server admits
├── Eto/                # Platform residue of the kernel interaction plane; nothing else seats here
│   └── Runtime.cs      # Platform-timer lease and pace producer; the timer cannot rise with its kernel-floor consumers
├── Platform/           # Composition root, CoreAnimation compositor, capture recorder, and the AppKit admission gate
│   ├── Capture.cs      # Leased ScreenCaptureKit recording — kernel-drain frames, one-shot still, paint proof
│   ├── Composition.cs  # One in-package composition seam nothing deeper composes; identity, time, FaultCell, broker registry
│   ├── Layers.cs       # CoreAnimation graph custody; every layer write rides the transaction fence, Display-P3 colour
│   └── Native.cs       # MacGate platform admission precondition; managed-to-AppKit extraction, input monitors
└── Shell/              # Session spine, UI event algebra, editor shell, chrome intent, vector icons
    ├── Chrome.cs       # Apply(ChromeIntent, Op?) outcome against Toolbar.Bar and InputPanel hosts; leased traverse
    ├── Editor.cs       # Editor shell — chrome-pane slots, toggles, state projection, Rhino getter
    ├── Events.cs       # UI fact/event evidence, anchor/source rows, transactional subscription, bounded drain
    ├── Hooks.cs        # Scoped veto/observe/replay hook rail with subscriber-fault isolation and taps
    ├── Icons.cs        # Vector-icon owner — host origins, a pose machine, filter chain, and catalog
    ├── Journal.cs      # Analytics egress folding UiEvent<GhFact> envelopes into bounded partitions
    ├── Session.cs      # Live-scope acquisition, apply/run gates, and gauged command acknowledgement
    └── Telemetry.cs    # Injected IMeterFactory minting one meter; producer-site instrument writes
```

## [02]-[STRATA]

Strata order the sub-domains; the UI-thread floor is the kernel's `UiThread` marshal and clock, with `Shell`'s `GhSession` scope gate composing it, and every cross-stratum consumption edge points down.

- S0 `Eto` + `Shell` — session, event, identity, telemetry, hook, and journal owners share same-stratum reach over the kernel floor.
- S0 `Eto` residue — the platform-timer lease and pace producer alone; the kernel boundary laws assign every other Eto concern to the kernel.
- S0 telemetry — `GhInstruments` declares the meter rows and every producing site writes through its owning member.
- S0 evidence — `PlatformRoot.Faults` enters the Shell floor whole and `SessionJournal` reads it, so no emitter mints its own custody.
- S1 `Document` + `Platform` — parallel composers over the floor, cross-blind to each other.
- S1 `Document` — the transaction spine: every graph mutation seals through the one gate, and no S1 sibling reads it.
- S1 `Platform` — the composition and native-gate half; `MacGate` is the one AppKit touch, so no upper owner names an AppKit member.
- S1 exemption — `PaintProof` (`Platform/Capture`) reads `PaintPass` and `JournalExport` as inert evidence under the model-only exemption.
- S2 `Canvas` — live host-surface owners seat at plugin load off the composition mount roster, so no canvas owner self-mounts.
- S2 reach — canvas owners compose session scope, the kernel marshal, undo seal, and the display-link drive.

`Components` stands beside the strata as an island, pure host-plus-kernel authoring with no interior edge either direction, so the fence draws it nowhere, and the kernel `MotionDrive` value arrives on the seam, never as an interior node.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TB
    accTitle: Rasm.Grasshopper interior strata
    accDescr: Which floor owners each canvas, document, and platform surface consumes, with the Components island deliberately undrawn.
    subgraph S2["S2 CANVAS"]
        Operator[CanvasOperator]
        Paint[PaintScene]
        Layout[CanvasLayout]
        Pacer[CanvasPacer]
    end
    subgraph S1["S1 DOCUMENT + PLATFORM"]
        Scope[DocumentScope]
        Ledger[HistoryLedger]
        Solution[SolutionControl]
    end
    subgraph S0["S0 SHELL FLOOR over kernel UiThread"]
        Session[GhSession]
        Dispatch["kernel UiThread"]
        Clock["kernel UiClock"]
        Events["kernel UiEvents"]
    end
    Operator e1@-->|"[IMPORT]: GhSession"| Session
    Operator e2@-->|"[IMPORT]: UiThread"| Dispatch
    Layout e3@-->|"[IMPORT]: HistoryLedger"| Ledger
    Paint e4@-->|"[IMPORT]: GhSession"| Session
    Pacer e5@-->|"[IMPORT]: UiClock"| Clock
    Scope e6@-->|"[IMPORT]: GhSession"| Session
    Scope e7@-->|"[IMPORT]: UiThread"| Dispatch
    Solution e8@-->|"[IMPORT]: UiEvents"| Events
    Dispatch f1@-->|"forbidden: floor upward"| S2
```

## [03]-[SEAMS]

Timing owners admit the kernel's `MonotonicTimeline`; colour owners admit `PerceptualColor`. `Interaction` crosses on the same rails: `UiDispatch` carries every UI-thread marshal, `MotionDrive` samples every paced drive, `Mark` spells every draw primitive the `GhMark.Kernel` case carries, `PaintProgram` batches every kernel draw run, `IntentTable` resolves every menu node, `ControlSpec` seats every chrome pane, and `AssetOrigin` names every icon. Host operations return their canonical values, and instrument writes project at the producing site.

Fence law is census, never roster: one edge per kernel owner, consuming sub-domain, and kind, each member DECLARED at that kernel owner's fences and SPELLED in the sub-domain's code fences, joined ` + ` alphabetically; kernel-end edges fold it per owner, boundary, and kind, so a member moves exactly one edge at each end under the branch `[04]-[STRUCTURE]` derivation row. `Op` is the rail key every fence takes and rides no seam edge; `InstrumentSpec` rides `[PORT]` because `GhInstruments` DECLARES rows the app root mounts, while every other member crosses as a bound value.

`GhTelemetry` admits the app root's `IMeterFactory` and `ILoggerFactory` the same way: capability in, `rasm.grasshopper.*` instrument writes out, zero provider reference inside the boundary.

Projection seams generate through Riok.Mapperly, one `[Mapper]` per seam: `CanvasMap` (`Canvas/canvas.md` state/pulse/pick projections), `InputMap` (`Canvas/interaction.md` host-args admission), `SolutionMap` (`Document/solution.md` pulse/audit evidence), `TrimMap` (`Components/ports.md` pin-trim writes), `ObjectMap` (`Components/objects.md` native-object writes and colour crossings), `NativeMap` (`Platform/native.md` NSEvent projection), `CaptureMap` (`Platform/capture.md` survey facts). Hand projection survives only under a NAMED host demand, stated at its arm.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Grasshopper host-boundary kernel seams
    accDescr: Which kernel owner hands which frozen-name contracts to each Grasshopper sub-domain, one edge per owner, sub-domain, and kind.
    subgraph grasshopper[RASM.GRASSHOPPER]
        Canvas[Canvas boundary]
        Components[Component authoring]
        Document[Document gates]
        Eto[Eto runtime]
        Platform[Platform native]
        Shell[Shell session]
    end
    subgraph rasm[RASM]
        Domain([Domain floor])
        Numerics([Numerics floor])
        Parametric([Parametric producers])
        Interaction([Interaction plane])
    end
    Domain e1@-->|"[BOUNDARY]: Context + HookRail + Lease"| Canvas
    Domain e2@-->|"[BOUNDARY]: Context + Lease"| Components
    Domain e3@-->|"[BOUNDARY]: HookRail + Lease"| Document
    Domain e4@-->|"[BOUNDARY]: Lease"| Eto
    Domain e5@-->|"[BOUNDARY]: Lease"| Platform
    Domain e6@-->|"[BOUNDARY]: HookRail + Lease"| Shell
    Domain e7@-->|"[PORT]: InstrumentSpec"| Shell
    Numerics e8@-->|"[BOUNDARY]: Dimension + PerceptualColor + UnitInterval"| Canvas
    Numerics e9@-->|"[BOUNDARY]: PerceptualColor"| Components
    Numerics e10@-->|"[BOUNDARY]: PerceptualColor"| Document
    Numerics e11@-->|"[BOUNDARY]: Dimension + PerceptualColor"| Platform
    Numerics e12@-->|"[BOUNDARY]: PerceptualColor"| Shell
    Parametric e13@-->|"[BOUNDARY]: MonotonicTimeline + MotionDrive"| Canvas
    Parametric e14@-->|"[BOUNDARY]: MonotonicTimeline"| Document
    Parametric e15@-->|"[BOUNDARY]: MonotonicTimeline"| Eto
    Parametric e16@-->|"[BOUNDARY]: MonotonicStamp + MonotonicTimeline + MotionDrive + SpringShape"| Platform
    Parametric e17@-->|"[BOUNDARY]: MonotonicStamp + MonotonicTimeline"| Shell
    Interaction e18@-->|"[BOUNDARY]: IntentTable + Mark + PaintProgram + UiDispatch"| Canvas
    Interaction e19@-->|"[BOUNDARY]: UiDispatch"| Document
    Interaction e20@-->|"[BOUNDARY]: UiDispatch"| Eto
    Interaction e21@-->|"[BOUNDARY]: UiDispatch"| Platform
    Interaction e22@-->|"[BOUNDARY]: AssetOrigin + ControlSpec + UiDispatch"| Shell
```

## [04]-[INTERNAL]

UI-thread interior composes around two floors, the `Eto/Runtime` dispatch surface and the `Shell/Session` scope gate, that every canvas, motion, event, and native owner marshals through; per-owner wiring lives on the owning implementation pages. Component authoring carries no UI-thread dependency; document gates marshal once through the session floor per command.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Grasshopper host-boundary interior wiring
    accDescr: How the UI-thread interior composes around the Eto dispatch floor and the Shell session-scope gate.
    Runtime[[Eto runtime floor]]
    Session[[Shell session gate]]
    Canvas[[Canvas boundary]]
    Interaction[[Interaction]]
    Paint[[Paint scene]]
    Wires[[Wire pass]]
    Motion[[Motion drive]]
    Events[[UI events]]
    Native[[Platform native]]
    Composition[[Layer compositor]]
    Capture[[Session capture]]
    Telemetry[[Telemetry fan]]
    Hooks[[Hook rail]]
    Journal[[Session journal]]
    Runtime e1@-->|"[BOUNDARY]: UiDispatch"| Session
    Runtime e2@-->|"[BOUNDARY]: UiDispatch"| Native
    Runtime e3@-->|"[PORT]: UiClock"| Motion
    Runtime e4@-->|"[SHAPE]: PulseBeat"| Events
    Runtime e5@-->|"[PROJECTION]: GhInstruments.Marshalled"| Telemetry
    Session e6@-->|"[BOUNDARY]: GhSession"| Interaction
    Session e7@-->|"[BOUNDARY]: GhSession"| Paint
    Session e8@-->|"[PROJECTION]: GhInstruments.Settled"| Telemetry
    Canvas e9@-->|"[SHAPE]: CanvasOp"| Interaction
    Canvas e10@-->|"[SHAPE]: PickHit"| Wires
    Wires e11@-->|"[SHAPE]: GhPlan"| Paint
    Paint e12@-->|"[PROJECTION]: GhInstruments.Painted"| Telemetry
    Paint e13@-->|"[SHAPE]: PaintPass"| Capture
    Motion e14@-->|"[PROJECTION]: GhInstruments.Windowed"| Telemetry
    Composition e15@-->|"[EVENT]: MotionDrive.Step"| Motion
    Native e16@-->|"[SHAPE]: MacAnchor"| Composition
    Native e17@-->|"[BOUNDARY]: MacGate"| Capture
    Events e18@-->|"[EVENT]: GhFact"| Journal
    Journal e19@-->|"[EVENT]: HookSignal"| Hooks
    Journal e20@-->|"[SHAPE]: JournalExport"| Capture
    Hooks f1@-->|"[FAULT]: FaultCell"| Telemetry
```

## [05]-[BOUNDARIES]

- `Rasm.Grasshopper` owns the GH2 host-boundary surface whole and re-owns no kernel concern; project references terminate at `Rasm`.
- Live host handles and native carriers stay inside the gated owners: `GhSession`, the kernel `UiThread`, and `MacGate` bound every live touch.
- App roots alone walk the mount roster through `PlatformRoot.Hold`, the one minter of identity, time, fault custody, and registries.
- Peer packages consume this boundary's host-free value shapes through the seam registry; no peer references this folder and it references none.

## [06]-[NAMESPACES]

Namespace mirrors folder path under `.editorconfig` `dotnet_style_namespace_match_folder = true:error`: every fence under `Rasm.Grasshopper/<Folder>/` declares `namespace Rasm.Grasshopper.<Folder>;`, giving each sub-domain folder its own root.

Boundary compiles as ONE assembly, the single `Rasm.Grasshopper.csproj`, so members cross the sub-domain namespaces with no build edge. `Eto.Forms`, `Eto.Drawing`, and the `Grasshopper2.*` roots arrive as project-level global usings, so fences name host members bare; kernel namespaces ride explicit `using` rows per fence.

Host-name resolution is one law:
- Inside `Rasm.Grasshopper.*` a partial qualification re-resolves against the boundary's own namespaces, so fences name host members bare.
- Host types no global using reaches spell `global::` in full.
- Simple-name collisions between host namespaces resolve through one project-level alias row in the csproj; the branch `RULINGS.md` homes the law.
- Fully-qualified `Grasshopper2.*` spellings stay valid because no boundary namespace shadows that root.
