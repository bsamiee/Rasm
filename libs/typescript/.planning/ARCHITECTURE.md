# [TYPESCRIPT_BRANCH_ARCHITECTURE]

The branch domain map of `libs/typescript` — fourteen folders in five dependency waves, acyclic with `kernel` at the base and the publication folders at the leaves. Wire decode is a boundary concern inside `wire`, never the branch center; deployment (`iac`) and dev infrastructure (`proof`) are plane-distinct citizens outside the runtime graph. The permitted-edge ledger, tag law, and port registry are `composition-system.md`; the data spine is `dataflow-system.md`.

Each node is a folder root; the `.planning/` scaffold is authoring substrate, never part of the map.

## [01]-[PACKAGE_MAP]

```text codemap
libs/typescript/
├── kernel/     # W0 runtime — cross-language identity, clock, schema-brand, quantity, fault values
├── proof/      # W0 dev     — frozen corpora, law combinators, arbitraries, harnesses, gauges
├── state/      # W1 runtime — host-free fold algebra: folds, CRDT merge, causality, evidence, live queries
├── host/       # W1 runtime — process runtime: exec rows, config chain, flags, net policy, lifecycle
├── security/   # W1 runtime — authn, authz, sessions, secrets, signing over stateless primitives
├── telemetry/  # W1 runtime — OTLP, conventions, RUM, audit/meter streams, SLO algebra, boards
├── wire/       # W2 runtime — all C#-minted wire decode: codecs, frames, gateway, contract, fault
├── work/       # W2 runtime — durable execution: entities, workflows, queues, schedules, egress
├── store/      # W3 runtime — event-sourced persistence: journal, projections, capability, objects
├── ai/         # W3 runtime — model providers, embeddings, tools, durable agents, MCP hosting
├── edge/       # W4 runtime — the one public front door: HttpApi, realtime, webhooks, CLI, problem
├── browser/    # W4 runtime — browser runtime: boot, PWA shell, persistence, transport, routing
├── ui/         # W4 runtime — component capability + the viewer spatial project (second Nx project)
└── iac/        # W4 deploy  — Pulumi typed programs, provider dispatch, K8s, secrets, observe, policy
```

## [02]-[SEAMS]

```text seams
kernel     ⇄  csharp:Rasm              # [CONTENT_KEY]: XxHash128 seed-zero content-identity parity
kernel     ←  csharp:Rasm.AppHost      # [WIRE]: HLC two-half compose-order parity
kernel     ⇄  csharp:Rasm.Compute      # [WIRE]: QuantityFamily SI-scalar decode + XxHash128 seed-zero two-half parity
state      ←  csharp:Rasm.AppHost      # [WIRE]: ReceiptEnvelope/HlcStamp/TenantContext + DegradationLevel/CommandAvailability evidence
state      ←  csharp:Rasm.Compute      # [WIRE]: ProgressMarkWire evidence folds
state      ←  csharp:Rasm.Persistence  # [SHAPE]: commit/branch/version-vector/Merkle shapes
state      ←  csharp:Rasm.AppUi        # [PROJECTION]: EvidenceFeed / EvidenceTimeline
host       ←  csharp:Rasm.AppHost      # [WIRE]: FlagVerdictWire over the shared OpenFeature contract
telemetry  ←  csharp:Rasm.AppHost      # [TRANSPORT]: OtelExport OTLP egress
wire       ←  csharp:Rasm.AppHost      # [WIRE]: ReceiptEnvelope/HLC/Tenant + capability SDK + CredentialPem/claim/livewire codecs + support-capture gateway verb
wire       ←  csharp:Rasm.Compute      # [WIRE]: proto suite wire + FaultDetail + descriptor drift gate
wire       ←  csharp:Rasm.Persistence  # [WIRE]: OpLog/Snapshot CRDT wire + JsonPatch egress
wire       ⇄  csharp:Rasm.Element      # [WIRE]: ElementGraph content-keyed wire under the descriptor gate
wire       ←  csharp:Rasm.Materials    # [WIRE]: MaterialWire/OpenPbrGroupsWire appearance decode
wire       ←  csharp:Rasm.Bim          # [WIRE]: BcfTopicWire/BcfViewpointWire + BimWire/DiffWire/OpLogWire/IdsAudit golden-byte parity + GeoFeature WKB
wire       ←  csharp:Rasm.AppUi        # [WIRE]: CommandPayloadWire + RenderReceiptWire + GeometryResidencyWire + ControlIntent/LayoutConstraint wire
ui         ←  csharp:Rasm.AppUi        # [RECEIPT]: ResidencyManifest content-key mesh residency
ui         ←  csharp:Rasm.Bim          # [WIRE]: BCF marks + GlobalId selection sets
```

TS consumes the GLB tessellation rail through the C#-owned wire; no TS↔Python seam exists. Folder-level seam rows live in each folder `ARCHITECTURE.md` `[02]-[SEAMS]` and mirror the csharp endpoint files verbatim.

## [03]-[DEPENDENCY_DIRECTION]

Dependency flows strictly downward through five waves — W0 `kernel`/`proof`, W1 `state`/`host`/`security`/`telemetry`, W2 `wire`/`work`, W3 `store`/`ai`, W4 `edge`/`browser`/`ui`/`iac` — with the full permitted-edge table, its forced resolutions, and the port registry owned by `composition-system.md`. The direction facts the map fixes:

- `kernel` imports nothing and is imported by every runtime folder; `proof` imports anything and is imported by nothing.
- Only `browser` and `ui` carry a `wire` edge — `ui` through the `#vocab` subpath only; every other runtime folder excludes it.
- `work` and `security` never import `store`: they compose port Tags the app root satisfies with `store` Layers. `store → security` is a direct edge, not a port: `store/journal` imports the `security/sign` AES-GCM envelope as its crypto-shredding `Shredder`.
- `iac` is depended on by nothing at runtime; its ledger edges point downward (`store` capability vocabulary, `telemetry` board functions), and the one value crossing back into the runtime graph is typed StackOutputs → `ShardingConfig` (the `work` seam).
