# [HOST_ARCHITECTURE]

The domain map of `host` — the W1 process-runtime substrate of `libs/typescript`. Five sub-domains own the one process-runtime surface: `exec` the Node/Bun runtime rows and subprocess/worker execution, `config` the provider chain and boot-validated schema, `flag` the runtime-neutral verdict evaluation and rollout policy, `life` the lifecycle choreography and probe vocabulary, `net` the branch-wide client and channel policy rows. The folder imports `kernel` only; `security`, `telemetry`, `wire`, `work`, `store`, `ai`, `edge`, and `browser` all depend on it, so every policy row here is branch law, never an app convenience.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
host/src/ # imports kernel ONLY (W1); runtime-spanning — per-runtime subpath exports ./server, ./browser
├── exec/ # Process-runtime selection and execution
│   ├── runtime.ts   # Node | Bun runtime rows — a bun swap is a Layer selection in the app root, never a fork
│   └── process.ts   # subprocess execution, signals, WorkerRunner pools
├── config/ # The one configuration plane
│   ├── provider.ts  # ConfigProvider chain: env → doppler-run injection → file → remote
│   └── schema.ts    # typed config schema + validation-at-boot
├── flag/ # Runtime-neutral flag verdicts
│   ├── verdict.ts   # FlagVerdict evaluation over the shared OpenFeature contract (runtime-neutral; decode wire/codec/flag); live verdict/config SSE stream row (remote provider re-evaluation)
│   └── rollout.ts   # rollout/targeting policy rows + verdict cache/stickiness rows
├── life/ # Process lifecycle
│   ├── cycle.ts     # startup/shutdown/drain choreography
│   └── health.ts    # readiness/liveness probe vocabulary
└── net/ # The branch-wide transport policy plane
    ├── client.ts    # branch-wide HttpClient default-policy rows (timeout/retry/proxy) — ai providers, work runner discovery, and OTLP export compose these
    └── channel.ts   # Socket/Ndjson channel rows: framed stream transport with backpressure, selected beside client policy by the same consumers
```

`exec` is the runtime floor: the Node|Bun rows every node-plane sibling selects through the app root, and the process/worker pools `work` runner entrypoints ride. `config` and `flag` are the value planes — configuration decodes once at boot through the provider chain into the typed schema, and flag verdicts evaluate over the shared OpenFeature contract on every runtime, browser included. `net` is the policy plane siblings compose instead of minting per-folder transport policy. `life` closes the process: startup/shutdown/drain choreography and the probe vocabulary `edge` serves and `iac` targets. A new runtime is one `exec/runtime.ts` row; a new config source is one provider-chain row; a new targeting dimension is one `flag/rollout.ts` policy row.

## [02]-[SEAMS]

```text seams
flag/verdict ← csharp:Rasm.AppHost/Runtime   # [WIRE]: FlagVerdictWire over the shared OpenFeature evaluation contract (ONE_FEATURE_FLAG_PROJECTION)
flag/verdict ← typescript:wire/codec         # [WIRE]: FlagVerdictWire decode transits the wire codec/flag row into the host-owned verdict vocabulary — host owns verdict evaluation
net/client   → typescript:ai/model           # [SHAPE]: HttpClient default-policy rows (timeout/retry/proxy) the LanguageModel provider rows compose
net/client   → typescript:work/engine        # [SHAPE]: HttpClient default-policy rows runner discovery composes
net/client   → typescript:telemetry/otlp     # [TRANSPORT]: HttpClient default-policy rows OTLP export composes
net/channel  → typescript:ai/model           # [SHAPE]: Socket/Ndjson channel rows selected beside client policy for provider streaming
net/channel  → typescript:work/engine        # [SHAPE]: Socket/Ndjson channel rows selected beside client policy for runner discovery streams
net/channel  → typescript:telemetry/otlp     # [TRANSPORT]: Socket/Ndjson channel rows selected beside client policy for OTLP stream egress
```

The `flag` rows are the one C#-inbound seam: `Rasm.AppHost` mints `FlagVerdictWire`, `wire` decodes it through `codec/flag` into the host-owned verdict vocabulary, and `host` evaluates — entitlement claims stay in `security/authz`, which consumes verdicts over its legal `security → host` edge. The `net` rows run the opposite direction: `host` owns the branch-wide client and channel policy, and `ai` providers, `work` runner discovery, and `telemetry` OTLP export compose the same rows rather than authoring per-folder transport policy.
