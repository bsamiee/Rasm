# [PY_RUNTIME_ARCHITECTURE]

The professional domain folder-map for `runtime`, the host-free execution foundation the four Python siblings compose. Each sub-domain is a real domain concept with one polymorphic owner and a one-line charter; the map fuels ideas and tasks, a planned-but-empty sub-domain reading as a visible gap. Dependency direction lives once in the branch `ARCHITECTURE.md` and is never restated here; boundaries and wires live on the tasks that build them.

## [1]-[DOMAIN_MAP]

```text codemap
runtime/
├── identity/            # content-addressing: the one XxHash128 owner reproducing the C# seed bit-identically
│   └── content-identity   ContentIdentity (one input-discriminated `of`), ContentKey, IdentityPolicy
├── reliability/         # the one fault family and resilience policy every sibling returns through
│   ├── faults             BoundaryFault tagged union, RuntimeRail carrier, the one exception-to-fault boundary
│   └── resilience         Retry stamina-backed policy table, one row per retryable class
├── context/             # caller-owned admission of host facts; no environment read after admission
│   └── admission          RuntimeContext + RuntimeProfile policy table, Correlation/Deadline/CausalFrame, SettingsAdmission + the keystore-then-secrets-file secret boundary
├── resources/           # filesystem + object-store roots and remote-AEC transports
│   └── roots              ResourceRoot/ResourceRef over fsspec/upath/obstore, TransportResource over HTTP/SSH
├── concurrency/         # bounded structured concurrency and stage orchestration
│   └── lanes              LanePolicy anyio task groups + DrainReceipt, StagePlan DAG, watchfiles/apscheduler sources
├── observability/       # local evidence production: receipts, the contributor port, signals, the one OTLP install gate
│   ├── receipts           Receipt union, ReceiptContributor port, Redaction, the structlog/OTel/psutil signals, the inbound W3C trace-context extract-and-continue
│   ├── metrics            Metrics async-observable durations/drain counters/process gauges reading the installed MeterProvider
│   └── telemetry          the one composition-root install owner minting the shared Resource + TracerProvider/MeterProvider/LoggerProvider trio over the OTLP exporter family, profile-gated by emit_otel
├── server/              # the inbound companion server-runtime and private daemon entry
│   └── serve              ServerHost grpc.aio lifecycle, decoded Credential axis (UDS insecure_loopback), WireProtoCodec, CrdtOpDecode, CapabilityInvoke, the cyclopts Entrypoint
└── evidence/            # external-surface and structural-parsing evidence
    └── evidence           ApiPackage/ApiMember reflection over importlib.metadata, Structural tree-sitter queries
```

## [2]-[BOUNDARIES]

Each sub-domain charter is the codemap comment; the boundary below names what the sub-domain refuses, so a planned-but-empty sub-domain and a misplaced concern both read as gaps.

- `identity` — refuses a path-keyed identity, a second hashing owner per package, and a cross-setting cache hit; the seed reproduces the C# `System.IO.Hashing.XxHash128` seed and is never re-minted.
- `reliability` — refuses a sentinel/`None`-as-failure return, a `try`/`except` outside the one `boundary` surface, a second retry owner, and blanket exception retrying.
- `context` — refuses host discovery, lifecycle ownership, product-root derivation, an environment read after admission, a global mutable context, a re-minted causal stamp or tenant scheme beside the C#-minted inbound frame, an eager unattended keystore probe, and a secret read outside the one settings-admitted boundary.
- `resources` — refuses a durable store, a default-root litter, a product store-root derivation, a hand-rolled retry around acquisition, and a second Python AEC-collaboration client; Speckle/OPC-UA/MQTT terminate C#-side per the cross-`libs/` `EXTERNAL_SYSTEMS_TERMINATE_CSHARP` boundary, reaching the companion through the canonical wire.
- `concurrency` — refuses a second scheduler surface beside `StagePlan`, a second cron owner beside `apscheduler`, unbounded task creation, a process-pool tax on a subinterpreter-isolable CPU kernel, a durable lane cache, a background loop without a `DrainReceipt`, and any bare `asyncio` import.
- `observability` — refuses an AppHost envelope, health status, exporter ownership beyond the one shared OTLP exporter, a second `MeterProvider` or a per-signal provider/exporter scatter outside the one install gate, a per-package parallel receipt rail, four parallel drain counters where one attribute-keyed counter folds them, a fresh-root span where the C# parent context is on the inbound carrier, and a stdlib `logging` call outside the structlog bridge.
- `server` — refuses a second wire vocabulary, a hand-rolled message loop, a bare exception across the wire, a key-chain server credential on the UDS loopback leg, and a public command surface; serves the existing C# `ComputeService`/`DocumentService`/`ControlService`/`ArtifactSync` contract over the UDS leg only, decoding the protobuf gRPC wire, the MessagePack op-log delta, and the capability-descriptor SDK the C# owners mint.
- `evidence` — refuses a competing search owner, a guessed environment status, and a member named absent from the catalogue evidence; emits evidence the `assay code` rail consumes.
