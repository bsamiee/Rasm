# [RASM_APPHOST_ARCHITECTURE]

`Rasm.AppHost` maps the APP-PLATFORM runtime spine `Compute`, `Persistence`, and `AppUi` adapt to and never reverse. One domain-folder owner per concern folds its axis with closed cases on a typed rail, cross-package facts cross only the inward port records, and the package holds no AEC-domain reference — alignment travels through the port seam, never a peer reference.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.AppHost/
├── Runtime/             # Runtime spine — lifecycle, clocks, config, ports, determinism, orchestration
│   ├── Profiles.cs      # Host-variance profile axis, lifetime adapters, power/thermal fidelity
│   ├── Lifecycle.cs     # Total lifecycle/phase/drain/cancellation spine with fault-to-capture trigger
│   ├── Time.cs          # Injected clock pair, deadline taxonomy, and one scheduler
│   ├── Resources.cs     # Bounded resource lanes: hybrid cache, object pools, drainable queues
│   ├── Modules.cs       # One composition root folding and freezing the service graph
│   ├── Config.cs        # Ranked config-source chain with fail-closed source-gen binding
│   ├── Secrets.cs       # Credential-material lifecycle behind the KMS-unwrap port
│   ├── Ports.cs         # Inward port records — the cross-package seam
│   ├── Determinism.cs   # Reproducibility kernel: pinned RNG/float-mode and hash-chained command log
│   ├── Orchestration.cs # Crash-durable workflow and persistent-job owner over the command/event/schedule ports
│   ├── LaneGuard.cs     # In-process WorkLane resilience governor: bulkhead, adaptive concurrency, load-shed, hedge
│   └── Features.cs      # Config-backed OpenFeature targeting and rollout with sticky bucketing; one FlagVerdict seam
├── Agent/               # Bidirectional agent surface over the capability registry
│   ├── Mcp.cs           # MCP-server projection of descriptors to tools, resources, and prompts
│   ├── Reasoning.cs     # In-process agent loop with model-selection and content-filter governance
│   ├── Federation.cs    # Folds external MCP servers into one registry as brokered descriptors
│   ├── Capability.cs    # Self-describing op catalog, command algebra, and fenced distributed quota
│   ├── Identity.cs      # Authentication boundary: OIDC issuer-trust, rotating token validation, claims-policy gate
│   └── Runtime.cs       # One command-dispatch front door over the command algebra, tool adoption, and receipt
├── Wire/                # Outbound and external-binding seam
│   ├── Outbound.cs      # Single outbound boundary with per-seam retry/cache and delivery fan-out
│   ├── LiveWire.cs      # Reactive bidirectional external-binding studio over the industrial-transport axis
│   ├── Companion.cs     # Multi-process modality axis and gRPC-over-UDS control-service host
│   ├── Topics.cs        # In-process event-bus topology with fan-out, join, and coalesce builders
│   ├── Outbox.cs        # Transactional outbox and dead-letter relay over the watermark dispatch sweep
│   └── Coordination.cs  # Cluster membership, election, and distributed-lock over the fenced lease
├── Sandbox/             # Capability-brokered plugin isolation, one admission gate, and the solver contract
│   ├── Admission.cs     # One supply-chain admission gate: offline Sigstore, SLSA provenance, SemVer contract
│   ├── Isolation.cs     # Capability-brokered WASM and process plugin isolation with unified call mediation
│   ├── Solver.cs        # Solver-plugin contract with canonical-representation negotiation
│   └── Provisioning.cs  # Post-fetch self-update state machine over the canary, blue-green, and linear-wave roll axis
└── Observability/       # Four-signal telemetry, health, and redacted support capture
    ├── Telemetry.cs     # Unified four-signal telemetry through minted identities and egress redaction
    ├── Health.cs        # Resource-pressure health fold and degradation/alert rails over one atomic reading cell
    └── Bundles.cs       # Bounded redacted support capture
```

Implementation collapses to one owner per axis and one entrypoint family per rail: a new feature is a row or case on a budgeted owner, and a public type outside an owner region is the named defect. Rail choice is named in the return type — `Validation<E,T>` accumulates, `Fin<T>` aborts, `IO<T>` carries effects; receipts stamp NodaTime `Instant`/`Duration`, and `TimeProvider` owns elapsed measurement.

## [02]-[SEAMS]

Cross-boundary seams split by counterpart group — cross-runtime wires to the TypeScript and Python peers, and same-branch ports to the C# platform packages. Each edge collapses one sub-domain-to-partner contract family onto its load-bearing kind, and the owning implementation pages carry the full family each edge stands for.

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    clusterBkg: "#21222C"
    clusterBorder: "#D6BCFA"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
    titleColor: "#D6BCFA"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: AppHost cross-runtime wire seams
    accDescr: AppHost sub-domain owners exchanging kinded wires, content keys, and transport with the TypeScript core, ui, and runtime packages and the Python runtime, edge rails colored by kind and nodes classed by seam direction.
    subgraph apphost[RASM.APPHOST]
        Agent[Agent surface]
        Runtime[Runtime spine]
        Wire[Wire seam]
        Observability[Observability signals]
    end
    Core([typescript:core])
    Ui([typescript:ui])
    TsRuntime([typescript:runtime])
    PyRuntime{{python:runtime}}
    Agent e1@-->|"[CONTENT_KEY]: CapabilityDescriptor"| Core
    Runtime e2@-->|"[WIRE]: ReceiptEnvelopeWire"| Core
    Observability e3@-->|"[WIRE]: DegradationLevel"| Core
    Wire e4@-->|"[WIRE]: BindingStatusWire"| Core
    Wire e5@-->|"[WIRE]: BindingStatus"| Ui
    Observability e6@-->|"[TRANSPORT]: OtelExport"| TsRuntime
    Agent e7@<-->|"[WIRE]: DiscoveryResult"| PyRuntime
    Observability e8@<-->|"[TRANSPORT]: TraceContext"| PyRuntime
    Runtime e9@<-->|"[WIRE]: HlcStamp"| PyRuntime
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    class Agent,Runtime,Wire,Observability primary
    class PyRuntime external
    class Core,Ui,TsRuntime annotation
    class e1,e2,e3,e4,e5,e7,e9 edgeData
    class e6,e8 edgeExternal
```

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    clusterBkg: "#21222C"
    clusterBorder: "#D6BCFA"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
    titleColor: "#D6BCFA"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: AppHost C# platform seams
    accDescr: AppHost sub-domain owners exchanging ports, shapes, wires, and receipts with the kernel, Element, AppUi, Persistence, and Compute packages, edge rails colored by kind and nodes classed by seam direction.
    subgraph apphost[RASM.APPHOST]
        Runtime[Runtime spine]
        Agent[Agent surface]
        Wire[Wire seam]
        Sandbox[Sandbox broker]
        Observability[Observability signals]
    end
    Kernel{{Rasm}}
    Element([Rasm.Element])
    AppUi{{Rasm.AppUi}}
    Compute{{Rasm.Compute}}
    Persistence[(Rasm.Persistence)]
    Kernel e1@-->|"[WIRE]: EncodedGeometry"| Sandbox
    Runtime e2@-->|"[CONTENT_KEY]: ContentHash"| Kernel
    Runtime e3@-->|"[PORT]: ProjectionContext"| Element
    Runtime e4@-->|"[PORT]: DeterminismContext"| AppUi
    AppUi e5@-->|"[WIRE]: LiveDelta"| Wire
    Runtime e6@<-->|"[PORT]: TenantContext"| Persistence
    Agent e7@<-->|"[PORT]: IdentityStore"| Persistence
    Wire e8@<-->|"[PORT]: OutboxEgress"| Persistence
    Observability e9@-->|"[PORT]: HealthContributorRow"| Persistence
    Runtime e10@-->|"[PORT]: ShedVerdict"| Compute
    Agent e11@-->|"[PORT]: GoverningChatClient"| Compute
    Compute e12@-->|"[RECEIPT]: HopReceipt"| Wire
    Sandbox e13@<-->|"[SHAPE]: EncodingKind"| Compute
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeSuccess stroke:#50FA7B,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Runtime,Agent,Wire,Sandbox,Observability primary
    class Kernel,AppUi,Compute external
    class Persistence data
    class Element annotation
    class e1,e2,e5 edgeData
    class e12 edgeSuccess
    class e3,e4,e6,e7,e8,e9,e10,e11,e13 edgeControl
```

## [03]-[INTERNAL]

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: AppHost boot-to-drain spine
    accDescr: Profile resolution feeds host boot, configuration, composition, and the lifecycle cell; runtime rails surround Running and surface through the runtime ports; the drain conductor folds participants into one unload receipt.
    Resolve(["ProfileSurface.Resolve"]) --> Boot["ProfileBoot.Boot"]
    Boot --> Compose["ConfigSource.Compose"]
    Compose --> Admit["PolicyBinding + OptionsAdmission"]
    Admit --> Fold["CompositionSurface.Compose"]
    Fold --> Ready["Lifecycle: Boot to Ready"]
    Ready --> Running["Running"]
    Running --> Rails["SignalGovernance / HealthSurface / SupportCapture / OutboundSurface"]
    Rails --> Ports[("Runtime ports")]
    Running --> Drain["DrainConductor.Drain"]
    Drain --> Unloaded(["DrainReceipt: Unloaded"])
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    class Resolve,Unloaded boundary
    class Boot,Compose,Admit,Fold,Ready,Running,Rails,Drain primary
    class Ports data
```

Boot resolves the one `ResolvedProfile`, folds and freezes the module graph behind validated frozen policy, and transitions the `Lifecycle` cell to Running; the telemetry, health, support, and outbound rails surround it and surface through the port records, and `DrainConductor.Drain` folds ranked participants into one `DrainReceipt`. Exact per-stage wiring lives on the owning implementation pages.

## [04]-[BOUNDARIES]

- AppHost is not a domain-service, job, DI, telemetry, UI, persistence, compute, or host-boundary package.
- AppHost owns runtime state and policy; app roots own process attachment and host events.
- Composition-root-only pins — the OTLP exporter, the Serilog bridge and sinks, gRPC-Web middleware, Kestrel public binding — stay at the app root.
- Protocol-runtime types the fences carry stay lib references, never app-root pins; the Sandbox and Wire owners hold the certified transport stack.
- Statement carve-outs are boundary capsules named per fence on the owning page; every other member stays expression-shaped on typed rails.
- Op catalog, command transaction, grant/cost broker, MCP projection, sandbox, solver, binding, and determinism are runtime-policy axes.
- Op execution stays Compute, durability stays Persistence, and the MCP protocol routes to the official SDK.
- Grant broker owns permission-shape evaluation as its own typed `PermissionShape` × `GrantScope` value-object predicate.
- Sentinels stop at the admission seam: `ClockPolicy.Admit` projects defaults to `Option<Instant>`; interiors never see provider shapes.
- AppHost owns support trigger and correlation; contributors own artifact classification and payload projection through `SupportContributorPort` rows.
- Lib level emits `ILogger` and minted `ActivitySource`/`Meter` pairs only; exporter projection belongs to composition roots.

## [05]-[PROHIBITIONS]

Deleted patterns the owner regions foreclose:
- NEVER a public type outside a sub-domain owner region; the port records own the cross-package seam.
- NEVER wrappers, rename adapters, helper or utility files, or thin forwarding surfaces over admitted packages.
- NEVER a generic receipt, ledger, or reported-value abstraction; every receipt stays its typed record.
- NEVER a second state machine, shutdown flag, or sibling phase enum beside `Lifecycle`.
- NEVER a free-floating `CancellationTokenSource` below the `CancelScope` spine.
- NEVER a raw wall-clock or stopwatch call site; `ClockPolicy` owns both clocks and projects sentinels at the admission seam.
- NEVER a bare duration literal; every bound traces to a `DeadlineClass` row or a page policy table.
- NEVER a second scheduler, a second cache owner, or a second retry owner on one seam; database retry stays at the Persistence execution strategy.
- NEVER ambient `IConfiguration` reads past bootstrap or interior `IOptions` handles; interiors read frozen policy records published at ready.
- NEVER hand-written service-descriptor spellings or closure-walking scans; the `Describe`/`DescribeKeyed` rows and `FromAssemblies` own registration.
- NEVER a process-static `Meter` or `ActivitySource` outliving its provider.
- NEVER Serilog types below composition roots, and never OTLP exporter pins below service app roots.
- NEVER a hand-written STJ converter beside the generated Thinktecture and NodaTime converters.
- NEVER an unredacted classified value at an exporter or bundle seam.
- NEVER posix traps or single-instance enforcement on plugin rows; host-attach injection drives phases there.
- NEVER a hand-rolled MCP transport or industrial-protocol client beside the certified Sandbox and Wire stack; the official SDKs own those wires.
- NEVER an unbrokered external-MCP side channel or a second tool catalog; federated capability enters only as brokered `CapabilityDescriptor` rows.
- NEVER a second tool-adoption seam in the reasoning loop; it reuses the one brokered `CommandAIFunction`.
- NEVER an opaque model call; every `IChatClient` call rides the one middleware pipeline, metered by `GrantBroker`, cached, and traced.
- NEVER a second op-metadata owner beside `CapabilityDescriptor` or a second permission-and-cost owner beside `GrantBroker`.
- NEVER an in-process third-party plugin outside the isolation boundary or a plugin-private geometry shape; plugins speak the Compute `EncodedTensor`.
- NEVER a second RNG or non-chained event log; `DeterminismContext` owns seed and float mode, `EventLog` the one hash-chained command log.
- NEVER a second notification sender, external-binding poller, alerting owner, or power monitor.
- `DeliveryFanout`, `LiveWire`, `AlertEngine`, and `FidelityScale` read the existing hop, health, and power signals, never parallel state machines.
- NEVER a second token-validation, JWKS, OAuth, or claims owner beside the `Agent/identity` authorities.
- Authentication produces one `Principal` that `GrantBroker` consumes.
- NEVER an unverified release or plugin install; `SupplyChainGate.Admit` proves signature and provenance against the pinned offline root first.
- NEVER a backing-service probe outside the one `DriverProbe` adapter or on a second connection; a driver row binds the shared pooled driver.
- NEVER an AEC-domain reference or a GeometryGym/IFC type on AppHost; it contributes only the `ProjectionContext` primitives the app root assembles.
- An ArchUnitNET rule asserts no GeometryGym edge at or below the element seam; `Rasm.Bim` is the sole owner above it.
- CSP analyzer diagnostics are architecture pressure: fix the shape, refine the rule on a false positive, never suppress.
