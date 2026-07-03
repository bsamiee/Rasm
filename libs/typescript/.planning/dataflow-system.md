# [DATAFLOW_SYSTEM]

One data spine runs the branch: C#-minted wire bytes decode at `wire`, land in owned vocabulary, fold through `state`, persist through the `store` journal, and project back out through `edge`/`browser`/`ui` — under one content-identity regime, one clock law, and one tenancy scope. This page carries the spine and its extension recipes; the per-folder design pages carry the fences.

## [01]-[CONTENT_IDENTITY]

`XxHash128` seed-zero is the one content-identity regime, bit-identical across C#, Python, and TypeScript. `kernel/identity` owns the ONE mint (`ContentKey`, `:x32` spelling, LE→BE normalization); exactly three sites delegate to it — `wire/frame` (artifact reassembly verify), the `browser/transport` decode worker (off-thread verify), and `store/object` (`ObjectKey`). An artifact hashed by any runtime is reusable by every other. A second mint, a second content-address notion, or a non-zero seed is the named cross-language drift defect; `proof/corpus` asserts bit-parity against the frozen byte-identity corpora.

## [02]-[WIRE_PLANE]

C# owns every `*Wire` shape; TS decodes verbatim and authors none. Decode happens exactly once, at `wire`, INTO the owned vocabulary of the domain folder where one exists — `Hlc` and `TenantContext` into `kernel`, receipt/availability/progress evidence into `state` — and into `wire`-owned decoded shapes otherwise. The adopted-verbatim names exist at the decode boundary only: `ContentKey`, `Hlc`, `TenantContext`, `ReceiptEnvelope`, `FaultDetail`. No TS folder name mirrors a sibling package; app-authored `store` events never cross the C# wire.

- `wire/contract` owns the `FileDescriptorSet` drift gate (Identical/Additive/Breaking) over the C#-owned proto descriptors — schema drift surfaces as a typed verdict, never a runtime decode failure.
- Fault altitudes stay three and never merge: `wire/fault` reconstructs the C#-minted `FaultDetail` (wire-only, closed `HopReason` vocabulary, `fromConnect` total fold); every folder raises its own `Data.TaggedError` rail for local failures; `edge/problem` projects both outward as RFC 9457 problem details (outbound-only). A node rail importing `FaultDetail` for a local failure is the named defect.
- The GLB tessellation rail is consume-only: `wire/frame` reassembles, `browser/transport` verifies and pools off-thread, `ui/viewer` renders; IFC/BCF vocabulary is confined to `wire` codecs and `ui/viewer` marks.

## [03]-[EVENT_SPINE]

Persistence is an append-only journal — no migrations, by construction. Streams are keyed `(appKey, tenantId, aggregate)`; events are closed app-authored `Schema.TaggedClass` families carrying `eventVersion`; appends are OCC by expected version, with the idempotency ledger and the transactional outbox atomic in the same commit.

- Schema evolution is read-time upcasting: per-type `eventVersion` folds plus snapshot upcasters keyed by `snapshot_schema_version`, totality proven in `proof/law`. The raw log is never rewritten.
- DDL is idempotent declarative ensure, additive-only, with the split as law: `iac` applies at provision time, `store` verifies at startup, runtime never mutates schema. `PgMigrator` is banned branch-wide; `proof/gauge` asserts the ban.
- The read side has two lanes: inline projections (same-transaction, read-your-writes) and async projections (checkpointed, LISTEN/NOTIFY-woken, SKIP LOCKED). Rebuild is a lane operation over the journal, never a data migration.
- One fold algebra, two altitudes: browser apps fold wire-decoded events in memory through `state`; node apps fold journal events durably through `store/project` binding the same `state` algebra. The algebra is generic over the op vocabulary — wire-minted and app-authored journal families are instances.
- Retention and erasure ride the journal without rewriting it: retention classes fold from the `state` retention frontier; per-subject erasure is crypto-shredding — destroying the key for the `security/sign` AES-GCM envelope makes the subject's rows unreadable, and DSAR export is a portability fold over journal and object rows.
- PG 18.4 capability is a closed row vocabulary `{extension, floor, probeSql, capabilities, layer}` with fail-closed probes; extensions are deployment-image facts the `iac` CNPG image satisfies, never JS dependencies. The sqlite lanes carry the same journal/projection contracts with an explicit capability-degradation table.

## [04]-[TIME_AND_ORDER]

`Hlc` compose order is byte-identical to the C# port law: physical half first, logical half second, both little-endian. `kernel/clock` owns the vocabulary and the honest-uncertainty windows; `state/causal` owns delivery — the causal buffer, version vectors, the stability frontier (GLB meet), causal finalize, and the retention-frontier handoff to `store/journal`. Time-travel reads are the `AsOf` three-coordinate law on `state/query` with HLC event-time watermarks; convergence is CRDT merge in `state/crdt` over `wire`-decoded op-log instances and app-authored instances alike.

## [05]-[TENANCY_AND_SCOPE]

`AppIdentity` — `{app, tenant, build, host-fingerprint}` — is one kernel value every plane derives from: the telemetry OTel Resource, the browser boot, the store scope, the meter keys. `StoreHandle` is a Layer family keyed `(appKey, tenancy policy)` resolving to schema-per-app or database-per-app plus the RLS `app.current_tenant` GUC set in-transaction; isolation is a scope value, never a fork. The tenancy contract is declared in `security/authz` and enforced by `store` as RLS; usage and cost attribution fold from the `telemetry` meter stream keyed by the same identity.

## [06]-[CROSS_LANGUAGE_INVARIANTS]

Frozen law; `proof/corpus` drivers assert each:
- C# owns the wire vocabulary; TS re-mints nothing and imports zero C#/Python artifacts — alignment travels through wire bytes, frozen corpora, and the descriptor drift gate.
- One content-identity regime (seed-zero `XxHash128`), one clock law (HLC two-half), one quantity law (SI canonicalized once at C# admission; `kernel/schema` `Quantity` carries magnitude + dimension — a `{value, unit}` re-decode is the rejected form).
- The C# typed-receipt family never collapses into one erased TS receipt.
- TS owns no geometry and evaluates no IFC semantics; GLB and BCF arrive decoded, render-side only.

## [07]-[EXTENSION_RECIPES]

| [INDEX] | [CHANGE] | [OWNER_SURFACE] | [SHAPE_OF_THE_EDIT] |
| :-----: | :--- | :--- | :--- |
| [01] | new app event type | the app's `Schema.TaggedClass` family | one tagged case + its upcast fold |
| [02] | new event version | `store/journal` upcast folds | one version fold; the log is never rewritten |
| [03] | new projection | `store/project` lane rows | one inline or async lane row |
| [04] | new wire shape (C#-minted) | `wire/codec` | one codec row decoding INTO owned vocabulary |
| [05] | new retrieval lane | `store/retrieve` roster | one lane row (FTS, trigram, phonetic, fuzzy, semantic) |
| [06] | new pg capability | `store/capability` matrix + `iac/kube` image | one probe row + one image fact |
| [07] | new retention class | `state` retention frontier + `store/journal` | one class row |
| [08] | new fold consumer | `state` fold algebra instances | one op-vocabulary instance |
| [09] | new tenancy shape | `store/scope` policy values | one scope value — isolation never forks |
| [10] | new frozen corpus or parity claim | `proof/corpus` readers | one reader row + its driver |
