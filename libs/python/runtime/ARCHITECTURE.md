# [PY_RUNTIME_ARCHITECTURE]

The domain map of `runtime` — the host-free execution foundation the Python siblings compose. One polymorphic owner per sub-domain (`observability`, `reliability`, `transport`, `execution`, `evidence`, `clock`), minting the shared content-identity, fault rail, resilience, admission, transport, logical-time, and receipt vocabulary.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
runtime/
├── observability/     # Local evidence production: receipts, contributor port, signals, OTLP gate
│   ├── receipts.py    # Receipt union, ReceiptContributor port, and structlog/OTel/psutil signals
│   ├── metrics.py     # Async-observable durations/drain-counters/process-gauges over MeterProvider
│   └── telemetry.py   # One OTLP install owner minting Resource + provider trio, profile-gated
├── reliability/       # One fault family and resilience policy every sibling returns through
│   ├── faults.py      # BoundaryFault tagged union and one exception-to-fault boundary
│   └── resilience.py  # Retry stamina-backed policy table, one row per retryable class
├── transport/         # Filesystem/object-store roots, remote-AEC transports, companion server, and wire codec
│   ├── roots.py       # ResourceRoot/ResourceRef over fsspec/upath/obstore and HTTP/SSH TransportResource
│   ├── serve.py       # ServerHost grpc.aio lifecycle, decoded Credential axis, descriptor-driven CapabilityInvoke, and cyclopts Entrypoint
│   └── wire.py        # WireProtoCodec protobuf seam and CrdtOp union with DecompressFn-injected CrdtOpDecode
├── execution/         # Caller-owned admission of host facts and bounded structured concurrency
│   ├── admission.py   # RuntimeContext policy table, causal frames, and SettingsAdmission secret boundary
│   └── lanes.py       # LanePolicy anyio task groups with DrainReceipt and StagePlan DAG
├── evidence/          # Content-addressing plus external-surface and structural-parsing evidence
│   ├── identity.py    # ContentIdentity/ContentKey reproducing C# XxHash128 seed bit-identically
│   └── evidence.py    # ApiPackage/ApiMember reflection and Structural tree-sitter queries
└── clock/             # Logical/causal time: the host-minted HLC stamp and content-stable element id
    └── clock.py       # Hlc two-half NodaTime-parity stamp, ElementId, Tenant, and CausalFrame.of
```

## [02]-[SEAMS]

```text seams
evidence/identity        ⇄  csharp:Rasm/Geometry               # [CONTENT_KEY]: XxHash128 digest endianness + seed parity
clock/clock              ←  csharp:Rasm.AppHost/Runtime        # [PORT_RECORDS]: Hlc two-half NodaTime stamp single mint
transport/wire           ⇄  csharp:Rasm.Compute/Runtime        # [WIRE]: WireProtoCodec PROTO_VOCABULARY transcode
transport/wire           ⇄  csharp:Rasm.Persistence/Version    # [WIRE]: CrdtOp MessagePack union decode (DecompressFn seam)
execution/admission      ⇄  csharp:Rasm.AppHost/Runtime        # [PORT]: CausalFrame Hlc two-half + Tenant
execution/admission      ←  csharp:Rasm.AppHost/Runtime        # [WIRE]: CredentialPem
observability/receipts   →  csharp:Rasm.AppHost/Observability  # [WIRE]: W3C trace-context inbound extraction
transport/serve          ⇄  csharp:Rasm.Compute/Runtime        # [WIRE]: PROTO_VOCABULARY
transport/serve          ⇄  csharp:Rasm.AppHost/Agent          # [WIRE]: DiscoveryResult capability invoke + CommandReceipt
transport/serve          ⇄  csharp:Rasm.AppHost/Runtime        # [WIRE]: HLC two-half stamp + Tenant partition
transport/serve          ⇄  csharp:Rasm.Persistence/Version    # [WIRE]: CrdtOpWire MessagePack union decode
transport/serve          ⇄  csharp:Rasm.Persistence/Sync       # [WIRE]: OpLogEntry.Payload MessagePack CRDT delta
observability/telemetry  ⇄  csharp:Rasm.AppHost/Observability  # [TRANSPORT]: trace-context + OTLP egress
transport/roots          ⇄  csharp:Rasm.AppHost/Runtime        # [TRANSPORT]: TransportResource HTTP/SSH remote-artifact acquisition
transport/serve          ←  csharp:Rasm.AppHost/Runtime        # [TRANSPORT]: gRPC ServerHost / CRDT MessagePack
evidence                 ←  python:geometry/mesh               # [CONTENT_KEY]: ContentIdentity.of keyed GLB bytes with policy seed
evidence/identity        →  python:data/tabular                # [CONTENT_KEY]: ContentIdentity content-key
observability            ←  python:artifacts/receipt           # [RECEIPT]: ArtifactReceipt receipt-facts contribution
transport/serve          ⇄  python:compute/numerics            # [WIRE]: ContentIdentity array backend dispatch
execution                →  python:artifacts/pipeline          # [RECEIPT]: Keyed session-lane elision (ContentKey, Work)
observability            ←  python:data/tabular                # [RECEIPT]: QueryReceipt.lineage_edges column-level lineage
transport/roots          →  python:data/tabular                # [PORT]: TransportResource remote connection
transport                →  python:data/tabular                # [TRANSPORT]: ResourceRef path resolution through fsspec
```

## [03]-[BOUNDARIES]

Each sub-domain charter is the codemap comment; the boundary below names what the sub-domain refuses, so a planned-but-empty sub-domain and a misplaced concern both read as gaps.

- `observability` — refuses an AppHost envelope, health status, exporter ownership beyond the one shared OTLP exporter, a second `MeterProvider` or a per-signal provider/exporter scatter outside the one install gate, a per-package parallel receipt rail, four parallel drain counters where one attribute-keyed counter folds them, a fresh-root span where the C# parent context is on the inbound carrier, and a stdlib `logging` call outside the structlog bridge.
- `reliability` — refuses a sentinel/`None`-as-failure return, a `try`/`except` outside the one `boundary` surface, a second retry owner, and blanket exception retrying.
- `transport` — refuses a durable store, a default-root litter, a product store-root derivation, a hand-rolled retry around acquisition, and a second Python AEC-collaboration client; Speckle/OPC-UA/MQTT terminate C#-side per the cross-`libs/` `EXTERNAL_SYSTEMS_TERMINATE_CSHARP` boundary, reaching the companion through the canonical wire; on the serve leg it refuses a second wire vocabulary, a hand-rolled message loop, a bare exception across the wire, a key-chain server credential on the UDS loopback leg, and a public command surface, serving the existing C# `ComputeService`/`DocumentService`/`ControlService`/`ArtifactSync` contract over the UDS leg only; on the wire leg it refuses a hand-rolled varint/tag framing the protobuf core owns, a per-message hand client, a `Message` leaking into interior code, a protobuf framing of the MessagePack op-log, a re-minted op vocabulary, and a hardwired `lz4` import (the decompress seam is the injected `DecompressFn`, LZ4 gated `python_version<'3.15'`), decoding the protobuf gRPC wire, the MessagePack op-log delta, and the capability-descriptor SDK the C# owners mint.
- `execution` — refuses host discovery, lifecycle ownership, product-root derivation, an environment read after admission, a global mutable context, a re-minted causal stamp or tenant scheme beside the C#-minted inbound frame, an eager unattended keystore probe, and a secret read outside the one settings-admitted boundary; on the concurrency leg it refuses a second scheduler surface beside `StagePlan`, a second cron owner beside `apscheduler`, unbounded task creation, a process-pool tax on a subinterpreter-isolable CPU kernel, a durable lane cache, a background loop without a `DrainReceipt`, and any bare `asyncio` import.
- `evidence` — refuses a path-keyed identity, a second hashing owner per package, and a cross-setting cache hit, with the seed reproducing the C# `System.IO.Hashing.XxHash128` seed and never re-minted; and refuses a competing search owner, a guessed environment status, and a member named absent from the catalogue evidence, emitting evidence the `assay code` rail consumes.
- `clock` — refuses a Python-minted HLC stamp, a second `Hlc`/`ElementId`/`Tenant` spelling beside this owner, a wall-clock substitution for the host-minted physical half, and a path-or-name-keyed element id; the two-64-bit-half stamp reproduces the C# `AppHost/Runtime` NodaTime `Instant.ToUnixTimeTicks` mint bit-identically and is never re-minted, the wire codec and admission context consuming this owner so the clock lives in one place rather than scattered across the serve, wire, and admission pages.
