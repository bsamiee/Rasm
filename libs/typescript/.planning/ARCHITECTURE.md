# [TYPESCRIPT_BRANCH_ARCHITECTURE]

The branch domain map of `libs/typescript` — capability domains in dependency waves, acyclic with `core` at the base. Wire decode is the core interchange plane's boundary concern, never the branch center; deployment (`iac`) is the plane-distinct citizen outside the runtime graph; dev infrastructure lives under `tests/` (`tests/contracts/`, `tests/typescript/`), never in the branch. The data spine is `dataflow-system.md`; folder sub-domain maps and mirrored seam rows live in each folder `ARCHITECTURE.md`.

Each node is a folder root; the `.planning/` scaffold is authoring substrate, never part of the map.

## [01]-[PACKAGE_MAP]

```text codemap
libs/typescript/
├── core/       # runtime — branch law: value floor, state algebra, the keyed-decode interchange plane, observe vocabulary
├── security/   # runtime — identity and custody: authn, authz, crypto authority, leased secrets, stateless over ports
├── data/       # runtime — durable persistence: journal, guarantee lanes, object plane, typed read side
├── runtime/    # runtime — execution: proc substrate, net, OTLP wire, front door, durable work, ai, browser condition
├── ui/         # runtime — interface: component system + view plane, with viewer as the spatial second Nx project
└── iac/        # deploy  — Pulumi typed programs: StackSpec arm dispatch, k8s tiers, secrets, observe realization, policy
```

## [02]-[SEAMS]

```text seams
core     ⇄  csharp:Rasm              # [CONTENT_KEY]: XxHash128 seed-zero :x32 content-identity parity
core     ←  csharp:Rasm.AppHost      # [WIRE]: Hlc two-half parity + ReceiptEnvelope/TenantContext/livewire triple/FlagVerdict/CredentialPem + capability SDK
core     ⇄  csharp:Rasm.Compute      # [WIRE]: proto suite + FaultDetail + ProgressMark frames + QuantityWire + the descriptor drift gate
core     ←  csharp:Rasm.Persistence  # [WIRE]: OpLog/Snapshot CRDT wire + JsonPatch egress
core     ⇄  csharp:Rasm.Element      # [WIRE]: ElementGraph content-keyed wire under the drift gate
core     ←  csharp:Rasm.Bim          # [WIRE]: BcfTopic/BcfViewpoint + BimWire/DiffWire/OpLogWire/IdsAudit golden-byte parity + GeoFeature WKB
core     ←  csharp:Rasm.Materials    # [WIRE]: MaterialWire/PbrGroups appearance decode
core     ←  csharp:Rasm.AppUi        # [WIRE]: CommandPayloadWire + CommandGateWire + RenderReceiptWire + GeometryResidencyWire + ControlIntentWire/LayoutConstraintWire + the EvidenceFeed timeline landing at state/feed
runtime  ←  csharp:Rasm.AppHost      # [TRANSPORT]: OTLP export alignment at the shared collector
ui       ←  csharp:Rasm.AppHost      # [WIRE]: livewire triple materialized at the viewer panel
ui       ←  csharp:Rasm.AppUi        # [WIRE]: ControlIntentWire + LayoutConstraintWire materialized at viewer/panel
ui       ←  csharp:Rasm.AppUi        # [RECEIPT]: RenderReceiptWire / RenderReceipt paired with local render evidence at viewer/probe
ui       ←  csharp:Rasm.Bim          # [WIRE]: BCF marks + GlobalId selection sets
ui       ←  csharp:Rasm.Materials    # [WIRE]: PbrGroups appearance into the scene
```

Every C#-minted family decodes exactly once through the core interchange codec registry; the ui rows above name where the decoded landings materialize. TS consumes the GLB tessellation rail through the C#-owned wire; no TS↔Python seam exists. Folder-level seam rows live in each folder `ARCHITECTURE.md` `[02]-[SEAMS]` and mirror the csharp endpoint files verbatim.

## [03]-[DEPENDENCY_DIRECTION]

Dependency flows strictly downward through the waves — W0 `core`, W1 `security`, W2 `data`, W3 `runtime`, W4 `ui`/`iac`. The permitted-edge table is the whole import law:

| [INDEX] | [FROM]     | [MAY_IMPORT]               | [NOTES]                                                                                              |
| :-----: | :--------- | :------------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `core`     | (nothing)                  | Runs identically under node, bun, and the browser; imported by every runtime folder                  |
|  [02]   | `security` | `core`                     | State lives behind port Tags; the folder never imports `data`                                        |
|  [03]   | `data`     | `core`, `security`         | The one direct `data → security` edge: `journal/retain` Shredder + `lane/tenant` ambient TenantScope |
|  [04]   | `runtime`  | `core`, `security`, `data` | Both process planes; the browser condition is the same package, never a sibling                      |
|  [05]   | `ui`       | `core`, `runtime`          | `viewer` is a second Nx project inside the folder with the same edge set                             |
|  [06]   | `iac`      | `core`, `data`             | Type/value reads only (`DashboardModel`, `Alert`, `Slo.Objective`, `Pg`)                             |

Port satisfaction happens at app composition, never as an upward import:
- Security's `SessionStore`/`IdentityJournal`/`ClaimStore`/`RelationStore` Tags are satisfied by `data` scope-built Layers.
- Data's `Embedder`/`Reranker` Tags are satisfied by the `runtime` ai plane; the durable embed band's `Persistence.BackingPersistence` requirement is satisfied at the same root by the data key-value scope.
- The `ui` viewer `GlbViewport` Tag is satisfied by the browser composition root from `runtime` Depot verified-arrival pairs (ContentKey + whole-buffer GLB octets, byteOffset zero) and the Depot residency ledger cell.
- The `ui` atom `LIVE_BRIDGE` rows bind the `runtime` browser host planes — Router location/pending, Install stance/fresh, Guard.dirty, Vault.status — through `Atom.subscribable`/`Atom.subscriptionRef` at app composition; the wiring is the composition root's own code.
- The one value crossing back from `iac` is typed `StackOutputs.sharding` read by `runtime` `ShardingConfig.layerFromEnv` — an env fact, never an import.
