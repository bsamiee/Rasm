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
│   └── admission          RuntimeContext + RuntimeProfile policy table, Correlation/Deadline, SettingsAdmission
├── resources/           # filesystem + object-store roots and remote-AEC transports
│   └── roots              ResourceRoot/ResourceRef over fsspec/upath/obstore, TransportResource over HTTP/SSH/Speckle
├── concurrency/         # bounded structured concurrency and stage orchestration
│   └── lanes              LanePolicy anyio task groups + DrainReceipt, StagePlan DAG, watchfiles/apscheduler sources
├── observability/       # local evidence production: receipts, the contributor port, signals
│   ├── receipts           Receipt union, ReceiptContributor port, Redaction, the structlog/OTel/psutil signals
│   └── metrics            [PLANNED] OTel async-observable instruments: durations, drain counters, process gauges
├── server/              # the inbound companion server-runtime and private daemon entry
│   └── serve              ServerHost grpc.aio lifecycle, Credential axis, the cyclopts companion Entrypoint
└── evidence/            # external-surface and structural-parsing evidence
    └── evidence           ApiPackage/ApiMember reflection over importlib.metadata, Structural tree-sitter queries
```

## [2]-[BOUNDARIES]

Each sub-domain charter is the codemap comment; the boundary below names what the sub-domain refuses, so a planned-but-empty sub-domain and a misplaced concern both read as gaps.

- `identity` — refuses a path-keyed identity, a second hashing owner per package, and a cross-setting cache hit; the seed reproduces the C# `System.IO.Hashing.XxHash128` seed and is never re-minted.
- `reliability` — refuses a sentinel/`None`-as-failure return, a `try`/`except` outside the one `boundary` surface, a second retry owner, and blanket exception retrying.
- `context` — refuses host discovery, lifecycle ownership, product-root derivation, an environment read after admission, and a global mutable context.
- `resources` — refuses a durable store, a default-root litter, a product store-root derivation, and a hand-rolled retry around acquisition; Speckle is transport, never a store.
- `concurrency` — refuses a second scheduler surface beside `StagePlan`, unbounded task creation, a background loop without a `DrainReceipt`, and any bare `asyncio` import.
- `observability` — refuses an AppHost envelope, health status, exporter ownership, a per-package parallel receipt rail, and a stdlib `logging` call outside the structlog bridge.
- `server` — refuses a second wire vocabulary, a hand-rolled message loop, a bare exception across the wire, an insecure port in production, and a public command surface; serves the existing C# `ComputeService`/`ArtifactSync` contract only.
- `evidence` — refuses a competing search owner, a guessed environment status, and a member named absent from the catalogue evidence; emits evidence the `assay code` rail consumes.
