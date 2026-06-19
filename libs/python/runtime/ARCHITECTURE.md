# [PY_RUNTIME_ARCHITECTURE]

The professional domain map of `runtime` — the host-free execution foundation the four Python siblings compose. One polymorphic owner per sub-domain (`observability`, `reliability`, `transport`, `execution`, `evidence`), minting the shared content-identity, fault rail, resilience, admission, transport, and receipt vocabulary.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
runtime/
├── observability/     # local evidence production: receipts, the contributor port, signals, the OTLP gate
│   ├── receipts.py    # the Receipt union, ReceiptContributor port, and structlog/OTel/psutil signals
│   ├── metrics.py     # async-observable durations/drain-counters/process-gauges over the MeterProvider
│   └── telemetry.py   # the one OTLP install owner minting the Resource + provider trio, profile-gated
├── reliability/       # the one fault family and resilience policy every sibling returns through
│   ├── faults.py      # the BoundaryFault tagged union and the one exception-to-fault boundary
│   └── resilience.py  # the Retry stamina-backed policy table, one row per retryable class
├── transport/         # filesystem/object-store roots, remote-AEC transports, and the companion server
│   ├── roots.py       # ResourceRoot/ResourceRef over fsspec/upath/obstore and HTTP/SSH TransportResource
│   └── serve.py       # the ServerHost grpc.aio lifecycle, decoded Credential axis, and cyclopts Entrypoint
├── execution/         # caller-owned admission of host facts and bounded structured concurrency
│   ├── admission.py   # RuntimeContext policy table, causal frames, and the SettingsAdmission secret boundary
│   └── lanes.py       # LanePolicy anyio task groups with DrainReceipt and the StagePlan DAG
└── evidence/          # content-addressing plus external-surface and structural-parsing evidence
    ├── identity.py    # ContentIdentity/ContentKey reproducing the C# XxHash128 seed bit-identically
    └── evidence.py    # ApiPackage/ApiMember reflection and Structural tree-sitter queries
```

## [2]-[SEAMS]

```text seams
evidence/identity        ⇄  csharp:Rasm/Geometry               # XxHash128 digest endianness + seed parity (content-key)
execution/admission      ⇄  csharp:Rasm.AppHost/Runtime        # CausalFrame Hlc two-half + Tenant (port)
execution/admission      ←  csharp:Rasm.AppHost/Runtime        # CredentialPem (wire)
observability/receipts   →  csharp:Rasm.AppHost/Observability  # W3C trace-context inbound extraction (wire)
transport/serve          ⇄  csharp:Rasm.Compute/Runtime        # PROTO_VOCABULARY (wire)
transport/serve          ⇄  csharp:Rasm.AppHost/Agent          # DiscoveryResult capability invoke + CommandReceipt (wire)
transport/serve          ⇄  csharp:Rasm.AppHost/Runtime        # HLC two-half stamp + Tenant partition (wire)
transport/serve          ⇄  csharp:Rasm.Persistence/Version    # CrdtOpWire MessagePack union decode (wire)
transport/serve          ⇄  csharp:Rasm.Persistence/Sync       # OpLogEntry.Payload MessagePack CRDT delta (wire)
observability/telemetry  ⇄  csharp:Rasm.AppHost/Observability  # trace-context + OTLP egress (transport)
transport/roots          ⇄  csharp:Rasm.AppHost/Runtime        # TransportResource HTTP/SSH remote-artifact acquisition (transport)
transport/serve          ←  csharp:Rasm.AppHost/Runtime        # gRPC ServerHost / CRDT MessagePack (transport)
evidence                 ←  python:geometry/mesh               # ContentIdentity.of keyed GLB bytes with policy seed (content-key)
evidence/identity        →  python:data/tabular                # ContentIdentity content-key (content-key)
```

## [3]-[BOUNDARIES]

Each sub-domain charter is the codemap comment; the boundary below names what the sub-domain refuses, so a planned-but-empty sub-domain and a misplaced concern both read as gaps.

- `observability` — refuses an AppHost envelope, health status, exporter ownership beyond the one shared OTLP exporter, a second `MeterProvider` or a per-signal provider/exporter scatter outside the one install gate, a per-package parallel receipt rail, four parallel drain counters where one attribute-keyed counter folds them, a fresh-root span where the C# parent context is on the inbound carrier, and a stdlib `logging` call outside the structlog bridge.
- `reliability` — refuses a sentinel/`None`-as-failure return, a `try`/`except` outside the one `boundary` surface, a second retry owner, and blanket exception retrying.
- `transport` — refuses a durable store, a default-root litter, a product store-root derivation, a hand-rolled retry around acquisition, and a second Python AEC-collaboration client; Speckle/OPC-UA/MQTT terminate C#-side per the cross-`libs/` `EXTERNAL_SYSTEMS_TERMINATE_CSHARP` boundary, reaching the companion through the canonical wire; on the serve leg it refuses a second wire vocabulary, a hand-rolled message loop, a bare exception across the wire, a key-chain server credential on the UDS loopback leg, and a public command surface, serving the existing C# `ComputeService`/`DocumentService`/`ControlService`/`ArtifactSync` contract over the UDS leg only, decoding the protobuf gRPC wire, the MessagePack op-log delta, and the capability-descriptor SDK the C# owners mint.
- `execution` — refuses host discovery, lifecycle ownership, product-root derivation, an environment read after admission, a global mutable context, a re-minted causal stamp or tenant scheme beside the C#-minted inbound frame, an eager unattended keystore probe, and a secret read outside the one settings-admitted boundary; on the concurrency leg it refuses a second scheduler surface beside `StagePlan`, a second cron owner beside `apscheduler`, unbounded task creation, a process-pool tax on a subinterpreter-isolable CPU kernel, a durable lane cache, a background loop without a `DrainReceipt`, and any bare `asyncio` import.
- `evidence` — refuses a path-keyed identity, a second hashing owner per package, and a cross-setting cache hit, with the seed reproducing the C# `System.IO.Hashing.XxHash128` seed and never re-minted; and refuses a competing search owner, a guessed environment status, and a member named absent from the catalogue evidence, emitting evidence the `assay code` rail consumes.
