# [DATAFLOW_SYSTEM]

One data spine runs the branch: C#-minted wire bytes decode once through the core interchange plane, land in owned vocabulary, fold through the core state algebra, persist through the data journal, and project back out through the runtime front door and the ui surfaces — under one content-identity regime, one clock law, and one tenancy scope. This page carries the spine and its extension recipes; the per-folder design pages carry the fences.

## [01]-[CONTENT_IDENTITY]

`XxHash128` seed-zero is the one content-identity regime, bit-identical across C#, Python, and TypeScript. `core/value/contentKey` owns the ONE mint — `ContentKey`, the `:x32` spelling, 32 lowercase hex in the big-endian layout the C# `System.IO.Hashing.XxHash128` seed-zero engine produces — and exactly three sites delegate to it: `core/interchange/frame` (reassembly verify), the `runtime/browser/fetch` decode worker (off-thread verify through `Digest.mint("content")`), and `data/object/store` (the object key IS the ContentKey). An artifact hashed by any runtime is reusable by every other. A second mint, a second content-address notion, or a non-zero seed is the named cross-language drift defect; the `tests/contracts` corpus readers (in `tests/typescript/_testkit`) assert bit-parity against the frozen byte-identity corpora. Secret derivation is the security folder's boundary — the digest engine never carries a KDF surface.

## [02]-[INTERCHANGE_PLANE]

C# owns every `*Wire` shape; TS decodes verbatim and authors none. Decode happens exactly once, at the `core/interchange/codec` registry — ONE closed census of every wire family with ONE keyed landing table, so `Wire.decode(family, octets)` is the single decode entry and a per-family codec page is unspellable. Families with core owners land INTO the `value`/`state` vocabulary (`Hlc`, `TenantContext`, receipt/availability/progress evidence); families whose consumers live in later waves land wire-owned decoded shapes declared once, adopted-verbatim on the C#-minted names (`ContentKey`, `Hlc`, `TenantContext`, `ReceiptEnvelope`, `FaultDetail`, `FlagVerdict`). No TS folder name mirrors a sibling package; app-authored journal events never cross the C# wire.

- `core/interchange/contract` owns the `FileDescriptorSet` drift gate over the C#-owned proto descriptors — schema drift surfaces as a typed graded verdict, never a runtime decode failure.
- Fault altitudes stay three and never merge: `core/interchange/codec` reconstructs the C#-minted `FaultDetail` on the wire rail; every folder raises its own `Data.TaggedError` rail classed by the core `FaultClass` vocabulary for local failures; `runtime/serve/problem` projects both outward as self-rendering RFC 9457 problems (outbound-only). A local rail importing `FaultDetail` for a local failure is the named defect.
- The GLB tessellation rail is consume-only: `core/interchange/frame` reassembles, the `runtime/browser/fetch` worker verifies and pools off-thread, `ui/viewer` renders behind the `GlbViewport` port; IFC/BCF vocabulary is confined to the codec registry landings and the viewer marks.

## [03]-[JOURNAL_SPINE]

Persistence is an append-only journal — no migrations, by construction. `data/journal/append` is the ONE atomic write owner: streams keyed `(app, tenant, aggregate)`, events as closed app-authored `Schema.TaggedClass` families stamped with `eventVersion`, OCC by expected version, with the idempotency ledger and the transactional outbox in the same commit (`LEDGER_CLAIM`/`ATOMIC_PUBLISH`/`RELAY_ROWS`).

- Schema evolution is read-time upcasting at `data/journal/evolve`: total per-tag `eventVersion` step chains, snapshot as a projection over the same `Upcast.chain`, hydrate folding snapshot-plus-tail. The raw log is never rewritten.
- DDL is idempotent declarative ensure, additive-only, with the split as law: `iac` applies at provision, `data/lane/capability` verifies fail-closed at startup, the runtime never mutates schema.
- The read side is `data/read/fold` — one `Fold.Plan`-bound lane at three staleness budgets (inline same-transaction, async checkpointed, rebuild) — with `data/read/live` stamping reactivity keys at publish and `runtime/serve/live` serving them under the resume-token law. Rebuild is a lane operation over the journal, never a data migration.
- One fold algebra, two altitudes: browser apps fold wire-decoded events in memory through `core/state/fold`; server apps bind the same `Fold.Plan` durably through `data/read/fold`. The algebra is generic over the op vocabulary — wire-minted and app-authored families are instances.
- Retention and erasure ride the journal without rewriting it: `data/journal/retain` folds retention classes behind the causal frontier; per-subject erasure is crypto-shredding — destroying the `WrappedKey` for the security `Shredder` envelope makes the subject's rows unreadable; DSAR export is a portability fold over journal and object rows. Audit and metering are rows of the ONE fact family on `data/journal/fact`.
- PostgreSQL capability is the `data/lane/postgres` ruled row vocabulary with fail-closed probes; extensions are deployment-image facts the `iac` CNPG image satisfies, never JS dependencies (BM25 = `vchord_bm25` beside VectorChord; uuidv7 is PG-native; no queue extension exists — SKIP-LOCKED outbox statements own the class). The sqlite lane carries the same journal/projection contracts across five profile rows with an explicit per-profile degradation table.
- Fanout is the `runtime/net/pubsub` port with engines as rows — the in-process PubSub row and the NATS JetStream row (fsync-hardened, replay-capable, never the system of record).

## [04]-[TIME_AND_ORDER]

`Hlc` compose order is byte-identical to the C# port law: physical half first, logical half second. `core/value/clock` owns the vocabulary and the honest-uncertainty windows (`Uncertainty.around` over the grade ladder); `core/state/causal` owns delivery — the causal buffer in `TRef` cells, version vectors, the stability frontier (GLB meet), and the retention-frontier handoff the data journal consumes. Time-travel reads are the ONE `AsOf` coordinate on `core/state/fold` — every replay handle verb takes it; convergence is lawful `Merge.Instance` algebra on `core/state/merge` over wire-decoded op-log instances and app-authored instances alike, with the commit-graph anti-entropy vocabulary on `core/state/commit`.

## [05]-[TENANCY_AND_SCOPE]

`AppIdentity` — `{app, tenant, build, host-fingerprint}` — is one core value every plane derives from: the OTel Resource, the browser boot, the store scope, the meter keys. `TenantContext` carries the `(app, tenant)` pair with its derived `scope` key; the tenancy contract is declared at `security/access/tenant` and enforced by `data/lane/tenant` — `Tenancy.within` is the single write path that opens the transaction and pins the `app.current_tenant` GUC through `set_config`, with row-scoped RLS, schema-per-app, and database-per-app as tagged cases whose locus derives from the app key. Isolation is a scope value, never a fork. Usage and cost attribution fold from the `data/journal/fact` meter stream keyed by the same identity.

## [06]-[CROSS_LANGUAGE_INVARIANTS]

Frozen law; the `tests/contracts` corpus drivers (TS readers in `tests/typescript/_testkit`) assert each:

- C# owns the wire vocabulary; TS re-mints nothing and imports zero C#/Python artifacts — alignment travels through wire bytes, frozen corpora, and the descriptor drift gate.
- One content-identity regime (seed-zero `XxHash128`), one clock law (HLC two-half), one quantity law (SI canonicalized once at C# admission; `core/value/quantity` carries magnitude + dimension — a `{value, unit}` re-decode is the rejected form).
- The C# typed-receipt family never collapses into one erased TS receipt.
- TS owns no geometry and evaluates no IFC semantics; GLB and BCF arrive decoded, render-side only.

## [07]-[EXTENSION_RECIPES]

| [INDEX] | [NEW_CHANGE]    | [OWNER_SURFACE]                                | [SHAPE_OF_THE_EDIT]                                     |
| :-----: | :-------------- | :--------------------------------------------- | :------------------------------------------------------ |
|  [01]   | event type      | the app's `Schema.TaggedClass` family          | one tagged case + its upcast step                        |
|  [02]   | event version   | `data/journal/evolve` upcast chains            | one version step; the log is never rewritten             |
|  [03]   | wire family     | `core/interchange/codec` registry              | one census row + one landing row                         |
|  [04]   | projection      | `data/read/fold` lane rows                     | one lane row at its staleness budget                     |
|  [05]   | retrieval lane  | `data/read/search` roster                      | one lane row (FTS, trigram, phonetic, fuzzy, semantic)   |
|  [06]   | pg capability   | `data/lane/postgres` matrix + `iac/kube` image | one probe row + one image fact                           |
|  [07]   | retention class | `data/journal/retain` policy table             | one class row                                            |
|  [08]   | fold consumer   | `core/state/fold` plan instances               | one op-vocabulary instance                               |
|  [09]   | tenancy shape   | `data/lane/tenant` cases                       | one scope case — isolation never forks                   |
|  [10]   | fanout engine   | `runtime/net/pubsub` engine rows               | one engine row; the port stays engine-blind              |
|  [11]   | dashboard pack  | `core/observe/board` pack rows                 | one pack row realized by `iac/operate/observe`           |
