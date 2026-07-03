# [RASM_TS_BLUEPRINT]

`libs/typescript` is a first-class platform library: thirteen top-level folders shipping composable Effect `Layer`/`Service` families that thin app composition roots select. One npm package `@rasm/ts` with per-folder exports subpaths — never a test-infra subpath; the dev plane lives under `tests/`. Complete in isolation; aligned with the C# and Python branches only through wire bytes, the `tests/contracts` frozen corpus, and the contract drift gate. Each folder section below carries its charter, package allocation, source-tree shape (leaves map 1:1 to `.planning/<path>.md` design pages), inbound seams, growth axes, and `_tmp` salvage targets. Branch-wide law — catalog allocation, the cross-folder port contracts, the build order — closes the file. Campaign verdicts, the versioned catalog, the edge ledger, and the seam re-mirror map live in `RASM-TS-PLATFORM-DECISION.md`; this file owns intent and growth.

## [01]-[KERNEL]

The contract floor: cross-language identity, clock, schema-brand, quantity, and fault-classification VALUES. No wire shape, no transport, no sibling import — every folder types against `kernel`, so a C# wire drift never ripples through it. The one `XxHash128` seed-zero mint lives here; `wire/frame`, the `browser/transport` worker, and `store/object` delegate to it, never re-mint.

[PACKAGES]: `hash-wasm` (folder-local, the only cataloguing site). Substrate: `effect`.

```
kernel/src/
  identity/
    contentkey.ts    # XxHash128 seed-zero ContentKey brand — the ONE mint; :x32 spelling; LE→BE normalize at every delegate
    appidentity.ts   # AppKey / AppIdentity / TenantContext value vocabulary — {app, tenant, build, host-fingerprint} dimensions telemetry, browser boot, and store scopes derive from
  clock/
    hlc.ts           # Hlc brand — two-half compose (physical first, logical second, both little-endian), compare/merge folds
    uncertainty.ts   # honest clock-uncertainty window vocabulary state causality consumes
  schema/
    brand.ts         # branded primitive family (Guid-v7, OrdinalKey, JsonPointer, BCP-47 Locale, …) + INGRESS_BUDGET refinement budgets — the decode-once law's type floor
    quantity.ts      # SI Quantity: magnitude + dimension; a {value, unit} shape never exists in TS
  fault/
    classify.ts      # cross-language fault classification values + the enricher CONTRACT (telemetry consumes, wire implements)
    budget.ts        # retry/degradation budget vocabulary folder policies type against
```

[SEAMS]: `K:47` (content-key parity), `AH:53`/`AH:64` (HLC two-half), `CO:96` (Quantity SI), `CO:110` (seed-zero two-half).
[GROWTH]: a new cross-language value is a new brand or vocabulary row here — never a per-folder re-declaration; a new fault class is a `classify.ts` row every rail inherits.
[SALVAGE]: mine `_tmp/interchange` refinement (brands/budgets), the identity/HLC parity law halves, the `CLOSED_FAMILY_LINT_FENCE` idea. Discard per-site mint plumbing.

## [02]-[PROOF_DISSOLUTION]

ROUTE record — `proof` is dissolved out of `libs/typescript`: test infrastructure never lives under `libs/`, and the `@rasm/ts` exports map ships no test-infra subpath. The re-homing map: frozen corpus bytes + producer/consumer law → `tests/contracts/` (C# sole producer; the byte-identity digest, `MATERIAL_LAYER_GOLDEN`, BimWire golden bytes, and HLC two-half vectors land there, asserting the `K:47`, `AH:64`, `CO:110`, `BM:98` parity claims); TS corpus readers, law combinators (fold identity, merge commutativity, upcast totality), Schema-driven arbitraries, and harness layers (testcontainers pg-18.4-with-extensions + the S3-compatible object-store row, the pglite fast unit lane, `@effect/vitest` layer sharing — never a hand-rolled wrapper) → `@rasm/ts-testkit` at `tests/typescript/_testkit`; the gauges the exports map cannot express (edge-ledger import audit, per-runtime subpath purity, external-admission and per-sub-folder crypto-admission audits, the branch-wide migrator-import ban) → the `tests/typescript/_architecture` suite; mutation thresholds-as-data → `.config/stryker.config.json`; e2e drivers → `tests/typescript/e2e`; dev-tool `.api` catalogs → `tests/typescript/.api/`. Specs colocate beside their owning folders and import the kit through the workspace graph; a new frozen corpus is a `tests/contracts/<seam>/` asset with its kit reader, and a new branch law is a kit combinator — the `tests/typescript/README.md` law governs both.

[SALVAGE]: mine `_tmp/projection` convergence/law + window `REPLAY_LAW` proof spines and the `_tmp/interchange` parity harness half INTO the kit at TS buildout. Discard nothing — the owner moved, the capability stands.

## [03]-[STATE]

The host-free fold algebra: keyed folds, CRDT merge, causality, evidence, live queries. One algebra, two altitudes — browser apps fold wire-decoded events in memory; node apps fold journal events durably through `store/project`. `state` never imports `wire`: wire decodes INTO state vocabulary, so the algebra stays transport-free and browser-safe by construction.

[PACKAGES]: `@electric-sql/d2ts`, `@electric-sql/d2mini` (incremental-dataflow lane), `@effect/typeclass` (R24-gated: Semigroup/semilattice instances for crdt merge + the `tests/typescript/_testkit` law combinators). Substrate: `effect`.

```
state/src/
  fold/
    algebra.ts       # keyed folds — ONE algebra, two altitudes (browser in-memory, store durable)
    replay.ts        # replay law + the d2ts incremental-dataflow lane
  crdt/
    merge.ts         # CRDT op merge semantics — one algebra generic over the op vocabulary; the C#-minted wire family and app-authored journal families are instances
    converge.ts      # convergence laws; fixtures pinned in the tests/contracts corpus
  causal/
    vector.ts        # version vectors, commit/branch shapes, Merkle comparison
    order.ts         # happened-before folds + honest-uncertainty windows (kernel/clock); causal delivery buffer; stability frontier (GLB meet), causal finalize, retention-frontier handoff to store/journal/retain
  evidence/
    receipt.ts       # ReceiptEnvelope-decoded evidence vocabulary — the typed receipt family, never erased
    availability.ts  # DegradationLevel / CommandAvailability vocabulary the wire gateway gate types against
    progress.ts      # progress-mark evidence folds
    timeline.ts      # evidence feed/timeline folds
  query/
    live.ts          # Subscribable live queries + presence semantics (edge/live serves these)
    window.ts        # windowed query folds + the REPLAY_LAW spine; AsOf 3-coordinate time-travel reads + asOfDiff + HLC event-time watermarks
```

[SEAMS]: `AH:58` (availability), `AH:63` (receipt), `CO:73` (progress), `AU:61` (timeline), `PE:46` (causal vector), `PE:44` consumer (crdt merge).
[GROWTH]: a new evidence kind is an `evidence/` vocabulary row + one fold arm; a new CRDT type is a `merge.ts` case with its `converge.ts` law.
[SALVAGE]: mine `_tmp/projection` wholesale — fold, crdt/convergence, causality, query/window, evidence; re-sequence the MERKLE/CAUSAL_FINALIZE blocked gates against the `tests/contracts` corpus. Discard phantom-`testing/` citations (repoint to `tests/contracts` + `tests/typescript/_testkit`) and array re-sort fallbacks.

## [04]-[HOST]

Process-runtime substrate: runtime selection rows, the config provider chain, flag verdicts, the branch-wide net client/channel policy rows, lifecycle. The one owner that keeps config/flags/exec/net policy from forking across folders as apps multiply. `flag/verdict` is runtime-neutral so browser apps evaluate the same verdicts node services do.

[PACKAGES]: none folder-local. Substrate: `effect`, `@effect/platform`, `@effect/platform-node`, `@effect/platform-bun`, `@effect/platform-browser`.

```
host/src/
  exec/
    runtime.ts       # Node | Bun runtime rows — a bun swap is a Layer selection in the app root, never a fork
    process.ts       # subprocess execution, signals, WorkerRunner pools
  config/
    provider.ts      # ConfigProvider chain: env → doppler-run injection → file → remote
    schema.ts        # typed config schema + validation-at-boot
  flag/
    verdict.ts       # FlagVerdict evaluation over the shared OpenFeature contract (runtime-neutral; decode wire/codec/flag); live verdict/config SSE stream row (remote provider re-evaluation)
    rollout.ts       # rollout/targeting policy rows + verdict cache/stickiness rows
  life/
    cycle.ts         # startup/shutdown/drain choreography
    health.ts        # readiness/liveness probe vocabulary
  net/
    client.ts        # branch-wide HttpClient default-policy rows (timeout/retry/proxy) — ai providers, work runner discovery, and OTLP export compose these
    channel.ts       # Socket/Ndjson channel rows: framed stream transport with backpressure, selected beside client policy by the same consumers
```

[SEAMS]: `AH:83` (FlagVerdictWire), `AppHost IDEAS:39` (the OpenFeature client projection).
[GROWTH]: a new runtime is one `exec/runtime.ts` row; a new config source is one provider-chain row; a new targeting dimension is one `rollout.ts` policy row.
[SALVAGE]: mine `_tmp/platform` Runtime (bindings, lifecycle) and Config (provider chain, flags, the live flag SSE stream → `flag/verdict`). Discard private `visibilitychange` ingresses (browser owns connectivity).

## [05]-[SECURITY]

Authn, authz, sessions, secrets, and signing composed from stateless primitives into Effect-owned Layers — no framework owns our schema. Identity state is event-sourced: `security` declares `SessionStore`/`IdentityJournal` port Tags against its own models and the app root composes `store` journal Layers into them. Every crypto package has exactly one admission sub-folder, ban-enforced.

[PACKAGES]: `arctic` + `@simplewebauthn/*` + `otplib` (authn only), `jose` + `@node-rs/argon2` + `@oslojs/crypto` + `@oslojs/encoding` (sign only), `@dopplerhq/node-sdk` (secret only). Substrate: `effect`, `@effect/platform`.

```
security/src/
  authn/
    oauth.ts         # arctic OAuth/OIDC provider rows (authorization-code + PKCE)
    webauthn.ts      # passkey ceremonies — browser-safe subpath
    otp.ts           # otplib TOTP/HOTP rows + recovery/backup-code rows
    apikey.ts        # API-key machine credentials: mint, digest-at-rest, rotate/revoke, prefix-indexed byHash resolve — hashing delegates sign/crypto
  session/
    token.ts         # token/refresh vocabulary + refresh rotation/revocation law; SessionStore/IdentityJournal port Tags (store satisfies at app root)
    cookie.ts        # cookie rows (httpOnly, sameSite, path-scoped) + the CSRF law
  authz/
    policy.ts        # RBAC/ReBAC relation tuples + policy evaluation folds (verdict evaluation delegates to host/flag)
    claim.ts         # tenant/entitlement claims; the app.current_tenant contract store enforces as RLS
  secret/
    doppler.ts       # Doppler axis: TTL-leased rotation, Redacted end-to-end
    material.ts      # key-material vocabulary; CredentialPemWire terminates here — redacted carrier, never logged
  sign/
    jwt.ts           # jose JWT/JWS/JWKS + rotation — the one token-crypto owner
    crypto.ts        # argon2 hashing, HMAC webhook signing, AES-GCM envelope (the store Shredder primitive) — node-only subpath
```

[SEAMS]: `AH:55` (CredentialPemWire → `secret/material`), `AppHost IDEAS:23` (PKCE web flow).
[GROWTH]: a new OAuth provider is one `arctic` row; a new claim kind is one `claim.ts` row; a new signing scheme is one `sign/crypto.ts` arm; a KMS/envelope key-custody provider is a prepared `sign/crypto` row.
[SALVAGE]: mine `_tmp/services` security (authn, secret) and `_tmp/platform` Session auth ceremony algebra. Discard better-auth-shaped assumptions.

## [06]-[TELEMETRY]

The four-signal observability plane serving hundreds of apps because everything is parameterized by app identity: the OTel Resource derives from the same `AppIdentity` value `browser` boot and `StoreHandle` use; signal conventions are vocabulary rows; the audit and usage-metering fact streams ride the same identity spine through journal ports. Dashboards are total functions `AppIdentity -> DashboardModel` — a per-app dashboard fork is structurally impossible because dashboards are data derived from identity, never files.

[PACKAGES]: `@opentelemetry/semantic-conventions`; the `@opentelemetry` sdk/exporter block (collapses at R3). Substrate: `@effect/opentelemetry`.

```
telemetry/src/
  otlp/
    export.ts        # Otlp trace/metric/log exporters; NodeSdk/WebSdk rows; egress redaction policy rows (export-boundary PII scrub); the ./dev DevTools row (plane:dev-fenced)
    context.ts       # W3C composite extract-and-continue at every ingress (edge middleware, browser boot, work entities)
  signal/
    convention.ts    # semantic-convention vocabulary rows — names as data, never string literals
    vital.ts         # browser RUM via native PerformanceObserver budgets (zero web-vitals)
    crash.ts         # crash capture reconstructing FaultDetail through the kernel enricher contract; replay redaction-at-capture
    audit.ts         # audit fact stream: actor/action/target vocabulary + typed diff evidence + retention classes; durable via a journal port Tag (store satisfies at app root)
    meter.ts         # usage/cost metering fact stream: (app, tenant)-keyed request/compute/storage/token counters + rating policy rows; same journal port law — the billing/cost-attribution source
  slo/
    burnrate.ts      # multi-window multi-burn-rate typed policy rows — SLO is algebra, not config
    alert.ts         # alert emission vocabulary feeding board + iac/observe
  board/
    model.ts         # DashboardModel algebra: AppIdentity -> DashboardModel total functions
    library.ts       # reusable dashboard/alert pack functions over the convention vocabulary (foundation-sdk behind the facade)
```

[SEAMS]: `AH:66` (OtelExport OTLP), `AH:56` consumer (support-capture → `signal/crash`).
[GROWTH]: a new signal convention is a vocabulary row; a new SLO shape is a policy row; a new metered resource is a `meter.ts` counter row; a new dashboard family is a `library.ts` function — never a file.
[SALVAGE]: mine `_tmp/platform` Observability (metrics, budget, crash, recorder, the egress-redaction boundary) and `_tmp/services` slo + audit. Discard direct `@opentelemetry` SDK coupling pending R3.

## [07]-[WIRE]

The one boundary rail to the C# wire: ALL C#-minted `*Wire` decode, the codec dispatch machinery, frames, gateway, contract drift, `FaultDetail` reconstruction, and the capability SDK. Decodes INTO `kernel`/`state` vocabulary where a domain owner exists; owns the decoded shape otherwise. Consumers reach decoded values through the `#vocab` exports subpath or ports declared at the shared vocabulary owner — the machinery interior is physically unresolvable. C# owns every wire shape; TS decodes verbatim and authors none.

[PACKAGES]: `@bufbuild/protobuf`, `@connectrpc/connect`, `@connectrpc/connect-web`, `@msgpack/msgpack` (R9), `cbor-x` (R10), `rfc6902`. Substrate: `effect`, `@effect/platform`.

```
wire/src/
  codec/
    envelope.ts      # ReceiptEnvelope/HlcStamp/TenantContext/RenderReceipt decode — typed receipt family, never erased
    proto.ts         # protobuf suite decode + FaultDetail vocabulary hook + QuantityFamily SI-scalar decode → kernel Quantity (invariant 4)
    graph.ts         # ElementGraph/Node/Relationship content-keyed decode
    oplog.ts         # OpLog CRDT wire decode → store/journal
    snapshot.ts      # SnapshotHeader canonical-CBOR decode → store snapshots
    crdt.ts          # CrdtOpWire MessagePack union + 16-byte Hlc cell → state/crdt
    version.ts       # commit/branch/version-vector/Merkle decode → state/causal
    patch.ts         # RFC 6902 EntityEdit egress codec
    progress.ts      # ProgressStore stream proto + progress-mark decode
    credential.ts    # CredentialPemWire decode → security/secret (redacted carrier)
    claim.ts         # BenchmarkClaimWire/HostFingerprintWire decode
    livewire.ts      # BindingStatusWire/CoercedValueWire/WriteReceiptWire decode
    flag.ts          # FlagVerdictWire decode → host/flag
    control.ts       # ControlIntentWire kind-discriminated decode
    layout.ts        # LayoutConstraintWire ordered Cassowary program decode
    bcf.ts           # BcfTopicWire/BcfViewpointWire decode
    geo.ts           # GeoFeature WKB decode (parser identity R6; turf owns planar ops only, in the viewer)
    bim.ts           # BimWire/DiffWire/IdsAudit golden-byte decode
    appearance.ts    # MaterialWire/OpenPbrGroupsWire/AppearanceSummary field-for-field decode — the index-promised cluster
  frame/
    artifact.ts      # ArtifactFrameWire reassembly + content-key verify (kernel-delegating mint site)
    geometry.ts      # GeometryPayload/MeshTensor frames — the GLB rail
    residency.ts     # GeometryResidencyWire residency protocol
  gateway/
    command.ts       # CommandPayloadWire dispatch + availability gate port typed against state/evidence/availability
    support.ts       # support-capture verb → telemetry crash
  invoke/
    capability.ts    # CapabilityDescriptor decode + the capability SDK (C# SdkTarget.TypeScript generated emit)
    client.ts        # typed invocation client: retry/backoff budgets from kernel/fault; protocol axis (connect | grpc-web) + retryable-wire schedules
  contract/
    descriptor.ts    # FileDescriptorSet drift gate
    drift.ts         # drift verdict vocabulary + wire inventory
  fault/
    detail.ts        # FaultDetail reconstruction + the closed HopReason hop vocabulary + the fromConnect total fold — the wire-only fault altitude
    quarantine.ts    # poison-frame quarantine + replay
```

[SEAMS]: `BR:29-31`, `BR:34`, `AH:52`, `AH:55-56`, `AH:60`, `AH:65`, `CO:67-70`, `CO:76`, `PE:43-44`, `PE:47`, `AU:59-60`, `AU:62`, `AU:82-83`, `BM:63`, `BM:77`, `BM:98`, `EL:68`, `EL IDEAS:40`, `MA:65`.
[GROWTH]: a new C# wire family is one codec table row + one page — never a new folder; a second consumer of a decoded shape reads `#vocab`, never relocates ownership.
[SALVAGE]: mine `_tmp/interchange` wholesale — transport, gateway, codec table, patch, frame, descriptor, quarantine, fault, inventory. Discard stale import paths, the unrealized APPEARANCE index promise (authored fresh), the inbound-root posture.

## [08]-[WORK]

Durable execution: cluster entities, durable workflows, queues, schedules, and signed egress. In-process durable-actor altitude — deployment topology is `iac`; the two meet only at the StackOutputs → `ShardingConfig` seam. `work` composes the `@effect/sql` core `SqlClient` Tag and the `@effect/cluster` `MessageStorage` Tag; the app root provides the store-owned driver Layer.

[PACKAGES]: `@effect/cluster`, `@effect/workflow`, `nodemailer`, `exceljs`, `jspdf`, `jszip`, `papaparse`. Substrate: `effect`, `@effect/platform`, `@effect/experimental`.

```
work/src/
  engine/
    entity.ts        # cluster Entities, sharding, runner discovery (K8sHttpClient is discovery, never provisioning); per-tenant fenced-quota rows; runner entrypoint Layers selected via host/exec at the app root
    storage.ts       # MessageStorage composition law — SqlClient Tag satisfied at the app root (bootstrap posture R5)
  flow/
    durable.ts       # durable workflow definitions + compensation/saga folds
    activity.ts      # activity rows: retry/timeout budgets from kernel/fault
  queue/
    job.ts           # DurableQueue job families: priority + dedupe/batch keys + DLQ/replay rows
    schedule.ts      # ClusterCron + schedule vocabulary + misfire/window policy rows
  deliver/
    webhook.ts       # durable signed egress + delivery receipts (signs via security/sign)
    mail.ts          # nodemailer mail egress + locale-keyed template rows (catalogs arrive as app-passed values) + suppression/unsubscribe rows
    report.ts        # document egress rows: exceljs/jspdf/jszip/papaparse as durable report jobs
    relay.ts         # outbox relay: SKIP LOCKED + LISTEN/NOTIFY drain feeding the channel rows; one fan-out policy row; per-tenant egress quota + DeliverAt
```

[SEAMS]: none inbound from C# — durable execution is TS-native capability; the `work`/`store` boundary is execution semantics vs queue-as-data (the pgmq row verdict is the DECISION's).
[GROWTH]: a new job kind is a `queue/job.ts` family row; a new egress channel is one `deliver/` row; a new saga pattern is a `flow/durable.ts` fold.
[SALVAGE]: mine `_tmp/services` execution (cluster, saga, runner, outbox-drain → `deliver/relay`), messaging (entity, delayed delivery, quota-enforcement half), and persistence work (durable job families → `queue/job`). Discard `SloBudget` (→`telemetry`) and `FaultDetail` imports in node rails.

## [09]-[STORE]

Event-sourced durable persistence with no migrations, by construction: one append surface, read-time upcasting, typed extension-capability rows, per-app tenancy scopes, sqlite lanes, retrieval, and content-addressed objects. DDL is idempotent declarative ensure — `iac` applies it at provision time, `store` verifies at startup, runtime never mutates schema. Hundreds of apps get isolated durable state from one surface: `StoreHandle` keyed `(appKey, tenancy policy)` with `LayerMap`-cached per-tenant Layers — isolation is a scope value, never a fork.

[PACKAGES]: `@effect/sql`, `@effect/sql-pg`, `@effect/sql-sqlite-node`, `@effect/sql-sqlite-bun`, `@effect/sql-sqlite-wasm`, `@aws-sdk/client-s3`, `@aws-sdk/s3-request-presigner`, `sharp`. Substrate: `effect`, `@effect/experimental` (EventLog overlay only).

```
store/src/
  journal/
    append.ts        # the ONE append surface: streams (appKey, tenantId, aggregate); OCC by expected version; events are closed Schema.TaggedClass families with app-authored eventVersion
    outbox.ts        # transactional outbox atomic with the append + idempotency ledger (ON CONFLICT DO UPDATE RETURNING (xmax = 0))
    snapshot.ts      # snapshot store keyed snapshot_schema_version
    upcast.ts        # read-time eventVersion upcaster folds — total functions, proven via the testkit law combinators; the raw log is never rewritten
    retain.ts        # retention policy rows + crypto-shredding via the security/sign Shredder; per-subject erasure = key destruction, the log never rewritten; the per-subject DSAR export fold (portability read over journal + object rows) rides beside erasure
  project/
    inline.ts        # same-transaction read-your-writes lanes (binds state folds to durability)
    async.ts         # checkpointed, LISTEN/NOTIFY-woken, SKIP LOCKED lanes
    rebuild.ts       # pg_cron rebuild/compaction + pg_ivm in-database IVM rows
  capability/
    row.ts           # closed {extension, floor, probeSql, capabilities, layer} vocabulary; fail-closed probes
    matrix.ts        # the PG 18.4 extension matrix rows (pgvector/VectorChord, pg_search, timescaledb, pg_partman, pgmq, pg_cron, pg_ivm, pg_duckdb, pg_graphql, pg_jsonschema, pgaudit, postgis, pg_uuidv7, h3; in-core trgm + LISTEN/NOTIFY channelization + advisory-lock claims + COPY bulk lanes) [R11]
  scope/
    handle.ts        # StoreHandle Layer family keyed (appKey, tenancy policy); LayerMap-cached per-tenant Layers
    tenant.ts        # RLS app.current_tenant GUC row set in-transaction; schema-per-app / database-per-app law
  lane/
    sqlite.ts        # sqlite node/bun lane + the explicit capability-degradation table (no RLS → file-per-app; no pg_ivm → in-process folds)
    wasm.ts          # OPFS sqlite-wasm lane law — browser/persist consumes [R13]
  retrieve/
    hybrid.ts        # hybrid RRF over the FTS | trigram | phonetic | fuzzy | semantic lane roster; embedding-fingerprint keys per vector row; rerank row; facet/snippet/keyset-cursor families; Embedder port satisfied by ai/embed
    index.ts         # vector/text index rows (pgvector/VectorChord, pg_search)
  object/
    key.ts           # ObjectKey = kernel ContentKey; content-addressed store (kernel-delegating mint site); object lifecycle rows (reference-sweep GC by retention class)
    presign.ts       # presign + codec fan-out (sharp derivative rows); conditional-put content-address idempotency (If-None-Match; 412 = idempotent noop)
```

[SEAMS]: `BR:31` consumer (OpLog/Snapshot → `journal`), `PE:43` consumer (SnapshotHeader), `CO codecs:760` (`ObjectKey` peer).
[GROWTH]: a new extension is one `capability/matrix.ts` row realized by the `iac/kube` image; a new tenancy shape is one `scope` policy value; a new lane is one dialect row under the same journal/projection contracts.
[SALVAGE]: mine `_tmp/services` persistence (store, tenancy, reactive, idempotency, outbox, object) + search, and `_tmp/projection` retention. Discard the `store.md` Migrator posture and the ioredis lanes.

## [10]-[AI]

The intelligence rail as a peer folder: provider rows, embeddings, Schema-typed tools, durable agents, MCP hosting. Runtime-neutral model/tool vocabulary with node-tagged actor rows; the capability-asymmetry table replaces provider-uniformity assumptions.

[PACKAGES]: `@effect/ai`, `@effect/ai-anthropic`, `@effect/ai-openai`, `@effect/ai-google`, `@effect/ai-amazon-bedrock`, `@effect/ai-openrouter`, `@modelcontextprotocol/sdk`. Substrate: `effect`, `@effect/platform`.

```
ai/src/
  model/
    provider.ts      # LanguageModel provider rows + the capability-asymmetry table + cost/latency tier-routing policy rows + guardrail rows (input/output moderation folds, Schema-refusal admission — one gate over every provider row)
    token.ts         # tokenizer budgets (AnthropicTokenizer — the one tokenizer owner) + context-assembly rows over app-passed retrieval results (never a store import)
  embed/
    embedder.ts      # EmbeddingModel rows; satisfies the store/retrieve Embedder port
    chunk.ts         # chunking/normalization policy rows
  tool/
    toolkit.ts       # Schema-typed toolkits as data
    mcp.ts           # MCP hosting on native McpServer/McpSchema — app toolkits projected as MCP tools, selected at the app root; @modelcontextprotocol/sdk is the client lane only
  agent/
    actor.ts         # durable agents over work entities
    memory.ts        # agent memory/session state rows
```

[SEAMS]: none inbound from C# — provider capability is TS-native; the `store` `Embedder` port is the one cross-folder contract.
[GROWTH]: a new provider is one `model/provider.ts` row with its asymmetry column; a new tool is a toolkit data row; a new agent pattern is an `actor.ts` arm over `work`.
[SALVAGE]: mine `_tmp/services` agent, the search embedding lifecycle, and the execution ai-activity. Discard provider-uniformity assumptions.

## [11]-[EDGE]

The one public front door: domain folders contribute `HttpApiGroup` families as data; the APP assembles exactly one `HttpApi` from selected groups and serves it through one serve row. Entry families generalize — HTTP, terminal verbs, realtime — so the god-contract is structurally impossible: the api VALUE exists only in the app.

[PACKAGES]: `@effect/cli`, `@effect/rpc`, `@effect/printer`, `@effect/printer-ansi`. Substrate: `effect`, `@effect/platform`, `@effect/platform-node`, `@effect/platform-bun`.

```
edge/src/
  api/
    group.ts         # HttpApiGroup contribution law — the app owns the assembled HttpApi value; RpcGroup/RpcServer rows (http | websocket | worker) are the second contribution family; versioning/pagination convention rows
    middleware.ts    # auth + API-key admission (security), W3C trace continuation (telemetry), rate/quota + load-shed (concurrency caps, queue-depth 503/Retry-After), CORS + security-header, idempotency-key, locale negotiation (Accept-Language → kernel Locale FiberRef row), FiberRef request/tenant context rows
    serve.ts         # NodeHttpServer | BunHttpServer | toWebHandler serve rows + the SPA/static-asset row (immutable-asset cache headers)
    emit.ts          # OpenApi document emission + HttpApiScalar reference + HttpApiClient typed-SDK derivation from the app-assembled HttpApi value — spec, docs, and client never drift from the served contract
  problem/
    detail.ts        # RFC 9457 problem-details mapping — the outbound-only fault altitude
    policy.ts        # status/retry-after/redaction policy rows
  live/
    socket.ts        # WS/SSE endpoints over state Subscribables + SSE resume-token rows + the protocol-handler mount port (an HttpApp port Tag; the store EventLog sync server is the standing example)
    presence.ts      # presence/subscription admission
  hook/
    verify.ts        # inbound webhook signature verify (security/sign)
    admit.ts         # replay protection + quota admission (types against work fenced-quota rows via port); admitted hooks enqueue through a declared ingress port Tag (work queue or store journal satisfies)
  cli/
    verb.ts          # verb contribution families under the one assembly law — the app assembles exactly one Command root from selected families; doctor/replay/inspect ship as the lib ops family, executing over host/exec
    render.ts        # CLI output rendering rows — Doc composition through @effect/printer(-ansi)
```

[SEAMS]: none inbound from C# — public ingress is TS-native; durable signed EGRESS is `work/deliver`, inbound admission is `hook`.
[GROWTH]: a new entry family is one sub-domain row (HTTP/CLI/realtime already generalized); a new middleware concern is one `middleware.ts` row; a new problem policy is one `policy.ts` row; a new LIB runbook is one `cli/verb.ts` family row — app verbs are contributed data under the assembly law, and runbooks are code, never documents.
[SALVAGE]: mine the branch IDEAS `PUBLIC_EDGE_INGRESS` + TASKLOG cards, the `_tmp/services` quota admission half, the `_tmp/services` messaging rpc, and the fault-altitude law. Net-new folder — greenfield with the law carried.

## [12]-[BROWSER]

Browser runtime, peer of `ui`: the single-boot law, PWA shell, local persistence, the decode transport pool, Navigation-API routing, session ceremonies. `browser` never imports `ui`; where a component needs a runtime capability, `ui` declares the port and `browser` provides the Layer at app composition. The render posture is client-rendered PWA + build-time prerender rows (per-route static HTML at app build, hydrated by `boot`); a streaming-SSR react server runtime is the named non-goal.

[PACKAGES]: `idb-keyval`, `workbox-window`, `workbox-build`, `nuqs` (`[R17]` — the `route/navigate` composition). Substrate: `effect`, `@effect/platform-browser`, `@effect/experimental` (EventLog client).

```
browser/src/
  boot/
    runtime.ts       # the single BrowserRuntime.runMain law (a second boot is the named defect) + the AppSpec budget VALUE apps construct
    connect.ts       # connectivity/visibility/network state rows
  shell/
    worker.ts        # service-worker/workbox rows + background-sync replay
    install.ts       # PWA manifest/install/update rows
  persist/
    kv.ts            # idb-keyval typed KV
    opfs.ts          # OPFS sqlite-wasm local-first lane + the EventLog overlay client [R13][R19]
  transport/
    pool.ts          # decode worker pool: frame reassembly + off-thread content-key verify (kernel-delegating mint site)
    fetch.ts         # fetch/stream transport rows + backpressure
  session/
    ceremony.ts      # webauthn/oauth browser ceremonies over security runtime-neutral subpaths
    store.ts         # browser session/token storage law
  route/
    navigate.ts      # Navigation-API typed router: Schema route table/params + traversal folds (the R17 nuqs composition site); zero routing package
    guard.ts         # NavigationGuard admission/confirm folds over security/host verdicts
```

[SEAMS]: `CO:69` consumer + `AU:62` consumer (frame/residency → `transport/pool`), `AppUi pipeline:548` (the residency worker).
[GROWTH]: a new offline capability is a `persist` row; a new transport shape is a `transport` row; the five-feed Rasm budget is product guidance an app constructs, never lib law.
[SALVAGE]: mine `_tmp/platform` Shell/Transport/Session (browser half: ceremony, router, guard)/Runtime (capability, connectivity, events, composition) and the decode-worker spawner intent. Discard dangling `decode.worker.ts` references and the five-feed budget as law.

## [13]-[UI]

Component capability on the React 19 spine: `@effect-atom` bindings, design tokens, interaction, localization, headless react-aria views — and `viewer`, a second Nx project carrying the spatial/GLB/geo/BCF tier so heavy render deps are compile-time excluded from the non-spatial majority. Components are capability, boot is runtime: `ui` never imports `browser`.

[PACKAGES]: `react`, `react-dom`, `react-aria-components`, `react-aria`, `react-stately`, `@react-aria/live-announcer`, `@effect-atom/atom`, `@effect-atom/atom-react`, `babel-plugin-react-compiler`, `react-compiler-runtime`, `react-error-boundary`, `@floating-ui/*`, `@radix-ui/*`, `@tanstack/react-table`, `@tanstack/react-virtual`, `@use-gesture/react`, `cmdk`, `vaul`, `lucide-react`, `class-variance-authority`, `clsx`, `tailwindcss` family, `colorjs.io`, `isomorphic-dompurify`. Viewer project: `three`, `maplibre-gl`, `@deck.gl/*`, `@geoarrow/deck.gl-geoarrow`, `apache-arrow`, `@google/model-viewer`, `@lume/kiwi`, `@turf/turf`, `@webgpu/types`.

```
ui/src/
  atom/
    binding.ts       # @effect-atom one-binding law (ONE_FOLD_ONE_BINDING) — the one state binding; AtomHttpApi/AtomRpc direct-binding rows (members [R25])
    derive.ts        # derived atoms/selectors over state folds + undo/redo stack folds
  token/
    theme.ts         # design tokens + theming rows
    scale.ts         # spacing/typography scale vocabulary + motion token rows (tw-animate)
  act/
    transition.ts    # native View Transitions API; <ViewTransition> upgrade row [R16]
    gesture.ts       # interaction/gesture rows (react-aria)
  view/
    primitive.ts     # react-aria headless component spine + live-region announce/toast rows
    compose.ts       # composition/slot patterns + Schema→aria FormBinding/picker rows; command-palette (cmdk), table/virtual collection, floating-anchor/sheet, presence-cursor cohort rows
  intl/
    message.ts       # Schema-typed message-catalog rows keyed by the kernel Locale brand + plural/select folds over native Intl — catalogs are app data, zero i18n package
    format.ts        # number/date/list/relative-time format rows composing react-aria I18nProvider/useLocale over native Intl
ui/viewer/src/       # second Nx project — scope:viewer
  scene/
    glb.ts           # GLB_VIEWPORT scene residency + three rows; consumes the browser decode-worker port; meshopt decode gated [R23]
    appearance.ts    # OpenPBR appearance binding over wire#vocab appearance
  geo/
    layers.ts        # maplibre/deck.gl geo layers + turf planar ops (WKB decode stays in wire)
    project.ts       # projection/camera sync rows
  mark/
    bcf.ts           # BCF topic/viewpoint anchors (GlobalId)
    selection.ts     # GlobalId selection sets
  probe/
    receipt.ts       # RenderReceipt frame-hash probes
    benchmark.ts     # BenchmarkClaim/HostFingerprint probes
  panel/
    binding.ts       # livewire binding panels
    control.ts       # ControlIntent panels
    layout.ts        # @lume/kiwi Cassowary layout re-solve to identical positions
```

[SEAMS]: `AU:63` (residency manifest), `BM:64`/`BM:67`/`BM:99` (BCF/selection marks), `AH:60` consumer (benchmark probes), `AH:65` consumer (binding panels), `AU:82-83` consumers (control/layout panels), `MA:65` consumer (appearance), `AppUi controls:204` + `solver:216` (the materializing head).
[GROWTH]: a new component family is a `view` row; a new token axis is a `token` row; a new interaction pattern is an `act` row; a new locale is `intl` catalog data, never a lib edit; a new viewer overlay is a `mark` row; a new panel kind is one `panel` row over its wire vocabulary.
[SALVAGE]: mine `_tmp/ui` wholesale — binding→`atom`, theming→`token`, interaction→`act`, overlay (bcf)→`viewer/mark`, overlay (floating, presence)→`view/compose`, render→`view` (core) + `viewer` (scene/geo/probe/panel). Discard the `glb.md` code-keyed `HopFault` defect (closed `HopReason` vocabulary wins), one-file sub-folder shapes, stale blocked-notes.

## [14]-[IAC]

The deploy plane: Pulumi Automation-API typed programs with zero authored YAML anywhere. One closed polymorphic provider dispatch — self-hosted K8s first-class, Docker, and `aws`/`gcp`/`cloudflare` as prepared rows. An app finalizes a cloud row by supplying a `StackSpec` VALUE; adding a provider is one dispatch arm in the lib; finalizing one is app data — never a lib edit, never a fork.

[PACKAGES]: `@pulumi/pulumi`, `@pulumi/kubernetes`, `@pulumi/aws`, `@pulumi/awsx`, `@pulumi/gcp`, `@pulumi/cloudflare`, `@pulumi/postgresql`, `@pulumi/tls`, `@pulumi/random`, `@pulumi/command`, `@pulumi/docker`, `@pulumi/policy`, `@pulumiverse/doppler`, `@pulumiverse/grafana`. Substrate: `effect`.

```
iac/src/
  program/
    automation.ts    # Automation-API inline programs (LocalWorkspace.createOrSelectStack); the one CLI-on-PATH wrap; typed run receipts (up | preview | refresh | destroy ledger)
    spec.ts          # StackSpec vocabulary: target arm, capability profile, region/domain, Doppler project ref
  stack/
    component.ts     # ComponentResource tiers
    output.ts        # typed StackOutputs; StackOutputs -> ShardingConfig is the SOLE iac/work meeting seam
  provider/
    dispatch.ts      # closed Match.exhaustive dispatch: selfhosted-k8s | selfhosted-docker | aws | gcp | cloudflare
    surface.ts       # per-arm service surface rows + the service-equivalence map; selfhosted-k8s includes the cluster-bootstrap row (@pulumi/command over owned metal/VPS)
  kube/
    workload.ts      # typed @pulumi/kubernetes workloads
    data.ts          # CNPG operator row provisioning the PG18.4-extension image that realizes store/capability; scheduled-backup + PITR rows to the object-store row [R15]
    traffic.ts       # cert/dns/ingress rows
  secret/
    doppler.ts       # @pulumiverse/doppler project/config provisioning
    inject.ts        # doppler-run injection law; ESC as a prepared row
  observe/
    stack.ts         # LGTM + the OTel collector row via helm.v4 typed values — upstream charts as typed objects, zero authored YAML
    apply.ts         # telemetry/board dashboard/alert functions applied through @pulumiverse/grafana
  policy/
    guard.ts         # CrossGuard packs
    drift.ts         # previewRefresh drift fold over OpType
```

[SEAMS]: none inbound from C# — deployment topology is TS-native; realizes the `store/capability` matrix as image facts and applies `telemetry/board` outputs.
[GROWTH]: a new cloud is one `dispatch.ts` arm + one `surface.ts` column; a new operator is one `kube` row; a new policy pack is one `guard.ts` row.
[SALVAGE]: mine `_tmp/services` provisioning wholesale (tier, drift, policy, observability-stack). Discard the `bootstrap.sh` shell entry.

## [15]-[CATALOG_STRUCTURE]

Allocation law: the SUBSTRATE tier — `effect`, `@effect/platform`, `@effect/platform-node`, `@effect/platform-bun`, `@effect/platform-browser`, `@effect/experimental`, `@effect/opentelemetry`, `@effect/vitest` (dev) — is catalogued once at `libs/typescript/.api/`; every other runtime package is folder-local in `libs/typescript/<folder>/.api/`, never duplicated; the dev-tool tier (vitest family, fast-check, testcontainers, pglite, playwright, stryker, DOM environments, k6 types) is catalogued at `tests/typescript/.api/` — dev tooling is consumed from the tests estate, never a lib folder. A folder README names consumed substrate packages under `[SUBSTRATE_PACKAGES]`; owner groups in `pnpm-workspace.yaml` mirror the folder roster plus `dev` and `tooling` tiers. Version verdicts, drops, the REJECTED negative space, and prepared/gated rows are the DECISION `[CATALOG]`.

## [16]-[PORT_LAW]

The permitted-edge ledger, the tag vocabulary, and the external-admission union are the DECISION `[EDGE_LEDGER]`; the `@rasm/ts` exports map enforces the ledger physically, the `tests/typescript/_architecture` suite audits it, and roster growth is a reviewed row-add there. This section owns the durable port law the ledger forces.

Cross-folder needs that would violate the ledger travel as ports: `security` declares `SessionStore`/`IdentityJournal` Tags (store journal satisfies), `work` composes the `@effect/sql` core `SqlClient` Tag and the `@effect/cluster` `MessageStorage` Tag (store driver satisfies both), `store/retrieve` declares the `Embedder` Tag (`ai/embed` satisfies), `ui` declares runtime-capability ports (`browser` satisfies), `kernel/fault` declares the enricher contract (`wire` satisfies, `telemetry` consumes), `telemetry` audit + meter declare journal ports (`store` journal satisfies), `edge/hook` declares the ingress + quota ports (`work` queue or `store` journal satisfies), `edge/live` declares the protocol-handler mount port (the `store` EventLog sync server satisfies). The app root wires every port; a port exists only where the ledger forbids the import — a port minted to dodge a legal edge is the named defect. A Tag for a runtime VALUE typed against a legally imported vocabulary — the `wire` gateway availability gate over `state/evidence` — is ordinary dependency injection, outside this registry.

The `tests/typescript/_architecture` suite audits what the exports map cannot express — the edge-ledger import audit, the branch-wide migrator-import ban, per-runtime subpath purity, and the `security` per-sub-folder crypto-admission boundaries.

## [17]-[BUILD_ORDER]

Topological waves from the DECISION `[EDGE_LEDGER]`; the `tests/contracts` corpus and the `tests/typescript` kit fill continuously as fixtures land:

- W0: `kernel`
- W1: `state`, `host`, `security`, `telemetry`
- W2: `wire`, `work`
- W3: `store`, `ai`
- W4: `edge`, `browser`, `ui` (+ `viewer` project), `iac`

A folder leg closes when every archetype that selects it (per the DECISION acceptance matrix) composes it as a thin ~30-line `main.ts` with zero lib edits. The six archetypes — SaaS product, realtime dashboard, geospatial/BIM viewer, headless service, CLI tool, AI copilot — are the standing derivation record for roster and capability growth: a proposed capability that no archetype would compose is app code, not lib code.
