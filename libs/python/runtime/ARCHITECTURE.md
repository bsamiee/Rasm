# [PY_RUNTIME_ARCHITECTURE]

The domain map of `runtime` — the host-free execution foundation the Python siblings compose. One polymorphic owner per sub-domain (`observability`, `reliability`, `transport`, `execution`, `evidence`, `clock`), minting the shared content-identity, fault rail, resilience, admission, transport, logical-time, and receipt vocabulary.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
runtime/
├── observability/      # Local evidence production: receipts, contributor port, signals, OTLP gate
│   ├── receipts.py     # Receipt union, drain taxonomy, ReceiptContributor port, and contribution fold
│   ├── metrics.py      # One MeterProvider's async duration/drain/gauge instruments; Metrics.record maps rows
│   └── telemetry.py    # Profile-gated OTLP install owner minting Resource, providers, and exemplar filter
├── reliability/        # One fault family and resilience policy every sibling returns through
│   ├── faults.py       # BoundaryFault union, latched aspect, Scope vocabulary, and exception-to-fault projector
│   └── resilience.py   # Retry stamina-backed policy table, one row per retryable class
├── transport/          # Filesystem/object-store roots, remote-AEC transports, companion server, wire vocabulary, and wire codec
│   ├── roots.py        # ResourceRoot/ResourceRef over fsspec/upath/obstore and HTTP/SSH TransportResource
│   ├── serve.py        # ServerHost grpc.aio lifecycle, health/Route roster, CredentialPolicy admit, FaultDetail invoke, daemon root drains
│   ├── shapes.py       # PROTO_VOCABULARY, descriptor drift gate, and grpcio-tools _pb2 surface
│   └── wire.py         # WireProtoCodec protobuf transcode + length-prefixed frame legs and CrdtOp union with DecompressFn-injected CrdtOpDecode
├── execution/          # Caller-owned admission of host facts, bounded structured concurrency, and local recipe execution
│   ├── admission.py    # RuntimeContext, causal frames, SettingsAdmission SECRET_LADDER — cloud tier over google-cloud-secret-manager
│   ├── lanes.py        # LanePolicy anyio task groups and StagePlan DAG
│   └── recipe.py       # RecipeExecution: queenbee-schema runs via lbt-recipes on THREAD lane, engine-gated, content-keyed, luigi-evidence
├── evidence/           # Content-addressing, cross-runtime seed-parity corpus, and external-surface/structural-parsing evidence
│   ├── identity.py     # ContentIdentity/ContentKey reproducing C# XxHash128 seed bit-identically
│   ├── reproduction.py # SeedReproduction/CorpusFixture/ParityReceipt nine-row ONE_WIRE_FIXTURE_CORPUS parity fold — 4-aspect KeyView
│   └── evidence.py     # Evidence union, ApiCatalogue MemberFact rows, and GrammarRegistry structural proof
└── clock/              # Logical/causal time: the host-minted HLC stamp and content-stable element id
    └── clock.py        # Hlc two-half NodaTime-parity stamp, ElementId, Tenant, and CausalFrame.of
```

## [02]-[SEAMS]

```text seams
evidence/reproduction    ⇄  csharp:Rasm/Spatial/reconciliation # [CONTENT_KEY]: XxHash128 digest endianness + seed parity (CANONICAL_BYTE_IDENTITY)
evidence/reproduction    ⇄  csharp:Rasm.Element/Projection     # [CONTENT_KEY]: MATERIAL_LAYER_GOLDEN parity ([H7]) — decode producer reference
clock/clock              ←  csharp:Rasm.AppHost/Runtime        # [PORT]: Hlc two-half NodaTime stamp single mint
transport/shapes         ⇄  csharp:Rasm.Compute/Runtime        # [WIRE]: PROTO_VOCABULARY 16-shape vocabulary + descriptor_pool drift gate
transport/wire           ⇄  csharp:Rasm.Compute/Runtime        # [WIRE]: WireProtoCodec transcode + length-prefixed frame legs
transport/wire           ⇄  csharp:Rasm.Persistence/Version    # [WIRE]: CrdtOp MessagePack union decode (DecompressFn seam)
transport/wire           ⇄  csharp:Rasm.Persistence/Version/ledger # [WIRE]: OpLogEntry.Payload MessagePack CRDT delta / replay-window decode
transport/serve          ←  csharp:Rasm.Compute/Runtime/transport # [WIRE]: CredentialPolicy five-row axis decode (UDS loopback admit)
transport/serve          →  csharp:Rasm.AppHost/Observability  # [WIRE]: W3C trace-context inbound extraction
transport/serve          ⇄  csharp:Rasm.AppHost/Agent          # [WIRE]: DiscoveryResult capability invoke + CommandReceipt
transport/serve          ⇄  csharp:Rasm.AppHost/Runtime        # [WIRE]: HLC two-half stamp + Tenant partition
observability/telemetry  ⇄  csharp:Rasm.AppHost/Observability  # [TRANSPORT]: trace-context + OTLP egress
transport/roots          ⇄  csharp:Rasm.AppHost/Runtime        # [TRANSPORT]: TransportResource HTTP/SSH remote-artifact acquisition
transport/serve          ←  csharp:Rasm.AppHost/Runtime        # [TRANSPORT]: gRPC ServerHost
transport/shapes         ⇄  python:geometry/mesh               # [WIRE]: Tessellation Request/Receipt registry rows bound by symbol
evidence                 ←  python:geometry/mesh               # [CONTENT_KEY]: ContentIdentity.of source bytes + geometry TessellationPolicy.spec
evidence/identity        →  python:data/tabular                # [CONTENT_KEY]: ContentIdentity content-key
observability            ←  python:artifacts/core/receipt      # [RECEIPT]: ArtifactReceipt receipt-facts contribution
execution                →  python:artifacts/core/receipt      # [RECEIPT]: reuse-fabric elision ContentKey hit/miss
observability            →  python:artifacts/core/receipt      # [RECEIPT]: MeterProvider signal stream
evidence/identity        →  python:artifacts                   # [CONTENT_KEY]: infallible ContentIdentity.of; producers bind the projected ContentKey
evidence                 ←  python:data                        # [CONTENT_KEY]: ContentIdentity over mesh point coordinates + tabular put payload
execution                ←  python:geometry/energy             # [PORT]: RecipeExecution/RecipeSpec — geometry binds the Job/RecipeInterface schema
evidence/identity        ←  csharp:Rasm.Compute/Runtime        # [WIRE]: XxHash128 seed-zero two-half [gated: hash-wasm / xxhash cp315]
evidence/identity        →  python:compute/numerics/array      # [CONTENT_KEY]: ContentIdentity under CANONICAL_POLICY + ParityReceipt layout proof
transport/roots          →  python:compute/experiments/model   # [BOUNDARY]: ResourceRef/UPath model-asset path resolution
execution                →  python:artifacts/core/plan         # [CONTENT_KEY]: Keyed session-lane elision (ContentKey, Work)
observability            ←  python:data/tabular                # [RECEIPT]: QueryReceipt.lineage_edges column-level lineage
transport/roots          →  python:data/tabular                # [PORT]: TransportResource remote connection
transport                →  python:data/tabular                # [TRANSPORT]: ResourceRef path resolution through fsspec
```

## [03]-[BOUNDARIES]

Each sub-domain charter is the codemap comment; the boundary below names what the sub-domain refuses, so a planned-but-empty sub-domain and a misplaced concern both read as gaps.

- `observability` — refuses an AppHost envelope, health status, exporter ownership beyond the one shared OTLP exporter, a second `MeterProvider` or a per-signal provider/exporter scatter outside the one install gate, a per-package parallel receipt rail, four parallel drain counters where one attribute-keyed counter folds them, a fresh-root span where the C# parent context is on the inbound carrier, and a stdlib `logging` call outside the structlog bridge.
- `reliability` — refuses a sentinel/`None`-as-failure return, a `try`/`except` outside the one `boundary` surface, a second retry owner, and blanket exception retrying.
- `execution` — refuses host discovery, lifecycle ownership, product-root derivation, an environment read after admission, a global mutable context, a re-minted causal stamp or tenant scheme beside the C#-minted inbound frame, an eager unattended keystore probe, and a secret read outside the one settings-admitted boundary; on the concurrency leg it refuses a second scheduler surface beside `StagePlan`, a second cron owner beside `apscheduler`, unbounded task creation, a process-pool tax on a subinterpreter-isolable CPU kernel, a durable lane cache, a background loop without a `DrainReceipt`, and any bare `asyncio` import.
- `evidence` — refuses a path-keyed identity, a second hashing owner per package, and a cross-setting cache hit, with the seed reproducing the C# `System.IO.Hashing.XxHash128` seed and never re-minted; and refuses a competing search owner, a guessed environment status, and a member named absent from the catalogue evidence, emitting evidence the `assay code` rail consumes.
- `clock` — refuses a Python-minted HLC stamp, a second `Hlc`/`ElementId`/`Tenant` spelling beside this owner, a wall-clock substitution for the host-minted physical half, and a path-or-name-keyed element id; the two-64-bit-half stamp reproduces the C# `AppHost/Runtime` NodaTime `Instant.ToUnixTimeTicks` mint bit-identically and is never re-minted, the wire codec and admission context consuming this owner so the clock lives in one place rather than scattered across the serve, wire, and admission pages.
