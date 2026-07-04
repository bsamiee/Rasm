# [DOSSIER] — Lane: sibling-ledgers (RASM-CS-PERSISTENCE design survey)

Read-only survey. Scope = the cross-package anchors the brief cites: `Rasm.AppHost/ARCHITECTURE.md`, `Rasm.Compute/ARCHITECTURE.md`, `Rasm.Bim/ARCHITECTURE.md`, the `Rasm.Element` seam hooks (`Graph/element.md`, `Graph/delta.md`, `Projection/address.md`), `RASM-CS-GEOMETRY-BRIEF.md` (V2 + E12), the archive `RASM-COMPONENT-PARADIGM-DECISION.md` (amendments + re-band gate), plus the Persistence own seam ledger and card pool (E1/E5/E6/E10/E12). Every anchor re-verified on disk. Sibling ARCHITECTURE files live at the PACKAGE ROOT (`libs/csharp/<Pkg>/ARCHITECTURE.md`), NOT under `.planning/`; Persistence pages are under the hidden `.planning/` tree (18 pages confirmed on disk).

Verdict legend: HOLD = register anchor exact on disk; DRIFT = defect real but anchor/target moved (corrected anchor given); REFUTED = disk falsifies the row.

---

## [01]-[REGISTER_RE-VERIFICATION] (assigned rows)

### E1 — Phantom realization — **HOLD** (every enumerated anchor exact; three additional un-enumerated instances)

- Phantom grep over all 18 pages `FederatedEntity|FederatedPlan|PlanNode|SourceKind|Query/federation` = **ZERO structural hits** (disk-confirmed). The only `SetExpr`/federation-token presence is the REAL `lane.md` `ELEMENT_SET_ALGEBRA` substrate (19 hits, all `SetExpr`), not the phantom owner.
- `IDEAS.md:38` (`[REUSE_WIRE]-[BLOCKED]`): "the source-agnostic `FederatedEntity`/`EntityGraph` ... — is realized" — EXACT.
- `IDEAS.md:45` (`[SUBSTRAIT_FEDERATION_SEAM]-[BLOCKED]`): "The Persistence-side lowering target (`ElementSet`/`SetExpr` algebra + `FederatedPlan`) is realized" — EXACT.
- `TASKLOG.md:24` (`[ARTIFACT_CONTENT_KEY_FEDERATION]-[BLOCKED]`): "`Query/federation#ENTITY_GRAPH` `SourceKind.SignedArtifact` ... is realized" — EXACT.
- Stale `-[COMPLETE]` cards citing deleted pages/nodes: `TASKLOG:37` (`Query/transaction.md`/`Query/Transaction.cs`), `:41` (`Store/quality.md`/`Store/Quality.cs`), `:42` (`Query/pipeline.md`/`Query/Pipeline.cs`), `:43` (`Sync/egress.md`/`Sync/Egress.cs`), `:44` (`Schema/ddl#EXTENSION_DDL` + `SchemaDdl.Sql` "landed"), `:48` (`Version/snapshots#SNAPSHOT_PROTOCOL AsOfKey`) — ALL EXACT. The `:44`↔`:45` self-contradiction confirmed on disk: `:44` claims `SchemaDdl.Sql` folded/landed while `:45` states "no `SchemaDdl.Sql` owner".
- **Additional (register did not enumerate):**
  - `IDEAS.md:57,58,59,60,61` — five CLOSED `-[COMPLETE]` cards cite the SAME deleted pages (`Query/transaction.md`, `Store/encryption.md`, `Store/quality.md`, `Query/pipeline.md`, `Sync/egress.md`). Same verify-remove failure class as the TASKLOG stale-COMPLETE set; the telos "every card claim resolves against disk" binds these too.
  - `TASKLOG.md:46` (`[ANNOTATION_RELOCATE_TO_BIM]-[COMPLETE]`) cites deleted `Sync/annotation.md` AND the phantom `Query/federation#ENTITY_GRAPH` join key (doubly stale) — the counterpart of BIM:101.
  - `TASKLOG.md:47` (`[SCHEDULE_RELOCATE_TO_BIM]-[COMPLETE]`) cites deleted `Sync/schedule.md` — the counterpart of BIM:102.
  - `TASKLOG.md:24`'s phantom `Query/federation` anchor CONTRADICTS the own live seam ledger: `ARCH:51` correctly homes the signed-artifact content-key binding on `Version/provenance` (`Version/provenance ← python:artifacts/provenance`). The card should re-anchor to `Version/provenance` (or the `[V1]` reintroduced federation owner), never the phantom.

### E5 — Coordination gap (AppHost side) — **HOLD** (four PORT rows exact; the gap is BILATERAL and the AppHost consumer contract is fully specified)

- `APPHOST:76` `Agent/capability ⇄ Rasm.Persistence` `[PORT]: fenced per-tenant Budget debit (ONE_FENCED_LEASE_STORE)` — EXACT.
- `APPHOST:77` `Runtime/orchestration ⇄ Rasm.Persistence` `[PORT]: workflow step-state CAS (ONE_FENCED_LEASE_STORE)` — EXACT.
- `APPHOST:78` `Wire/outbox ⇄ Rasm.Persistence` `[PORT]: transactional outbox same-tx (ONE_OUTBOX_EGRESS_SPINE)` — EXACT.
- `APPHOST:79` `Wire/Coordination.cs ⇄ Rasm.Persistence` `[PORT]: CAS + fenced-lease + membership backing store (ONE_FENCED_LEASE_STORE)` — EXACT.
- `APPHOST:71` `Runtime → Rasm.Persistence/Query/transaction` (deleted page) — EXACT stale.
- `APPHOST:73` `Runtime → Rasm.Persistence/Sync/egress` (deleted folder) — EXACT stale.
- **BILATERAL confirmation (Persistence side has NOTHING):** the Persistence `[02]-[SEAMS]` block (`ARCH:41-64`) carries ZERO counterpart rows for any of the four PORT seams — no fenced-lease, no CAS primitive, no named outbox, no membership row — AND no owner page exists (`Store/` = blobstore + provisioning only). The gap is a total one-sidedness, not a mere missing owner.
- **The AppHost consumer contract is CONCRETE, not vague** (raises the obligation precision): `Rasm.AppHost/.planning/Runtime/orchestration.md:257-271` declares `StepStateSeam` a "`Rasm.Persistence` ripple" with `Commit(WorkflowInstance, WorkflowStep)→Fin<Unit>` by fenced CAS, `Load`, and `InFlight: Func<TenantContext, Fin<Seq<string>>>` (:271); `:267` "CAS store + fenced-token column land at Persistence under the TenantId RLS predicate"; `CrashResume` (:282) reads the durable in-flight set. `[V2]`'s "READ cases beside guarded writes" is a hard demand: the AppHost flagship (`CrashResume`) needs `StepStateSeam.InFlight`, so a write-only coordination union strands it.
- **`[V2]` per-unit-vector Budget demand verified real:** `Rasm.AppHost/.planning/Agent/capability.md:54-69` — `CostUnit` `[SmartEnum<string>]` (`CpuMillis`/`WallMillis`/`BytesEgress`/`ModelTokens`/`Calls`), `CostVector(HashMap<CostUnit,long> Units)` (:62), `CostModel` (:69). AppHost genuinely meters a multi-unit vector, so the Persistence Budget row must carry N unit balances and the predicated `UPDATE` must check `token >= held AND balance_i >= debit_i` per requested unit — a scalar debit is falsified by the consumer.

### E6 — Seam-ledger drift — **HOLD** (all 12 sibling rows + 3 declared-unwired + 3 wired-undeclared confirmed; ONE internal target DRIFT: ARCH:55)

**Declared-unwired (own ledger):**
- `ARCH:50` `Version/timetravel ← python:data/gridded/virtual` `[CONTENT_KEY]: icechunk as-of snapshot identity` — EXACT. `AsOfKey` is a **genuine phantom**: grep of `AsOfKey` across all live Version pages = ZERO hits (TASKLOG:48 placed it on deleted `Version/snapshots#SNAPSHOT_PROTOCOL`). The row's target page (`timetravel`) is live but the capability it depends on is unrealized. `[V12]` mints `AsOfKey`=`Checkpoint.Hash` on `timetravel` — `Checkpoint.Hash` DOES exist (referenced `provenance.md:310`), `RecoveryPoint.AsCut()` DOES exist (`recovery.md:65`), so the fix has real anchors.
- `ARCH:55` `Query/topology ← Rasm/Spatial/reconciliation` `[CONTENT_KEY]: adjacency-derived GeometryHash` — anchor EXACT but **DRIFT on target**: `topology.md` has ZERO reconciliation refs (its `adjacency` mentions are QuikGraph connection-incidence, not the kernel `NamingHashOps`), so the row is one-sided; and the kernel upstream `Rasm/.planning/Spatial/reconciliation.md:3` targets `Persistence/version-control#STRUCTURAL_DIFF` (= `merge.md`, whose `GraphNode.GeometryHash` at `:69-73` is the named consumer), NOT `Query/topology`. Corrected target: `Version/merge#STRUCTURAL_DIFF`. See [04] for the deeper digest-semantics gap.
- `ARCH:57` `Query/lane ⇄ python:data/tabular/query` `[WIRE]: ElementSet receipt currency + Substrait portable plan` — EXACT. The Substrait half is the phantom-federation declaration; `[V1]` keeps it (reintroduce) or re-scopes to the ElementSet-currency half (retire).

**Wired-undeclared (composed in a fence, absent from the own ledger):**
- lane `VECTOR_CODEBOOK` ⇄ Compute — `lane.md:~388` composes `ProductCodebook`/`VectorRow` ("Compute imports it by its `Rasm.Persistence (project)` reference ... NEVER fits it"); declared by `COMPUTE:99` (`Model/embedding ⇄ Rasm.Persistence/Query/lane # EmbeddingVector.ContentKey ↔ VectorRow.ContentKey; ProductCodebook trained in Persistence #VECTOR_CODEBOOK`). No Persistence ledger row. HOLD. (Post-`[V5]`b split the row targets `Query/retrieval`.)
- `cache#L2_CONTRIBUTION` ⇄ AppHost — declared by `APPHOST:69` (`Runtime → Rasm.Persistence/Query/cache # [PORT]: TenantId RLS + cache L2 partition`). No Persistence ledger row. HOLD.
- ledger `PRESENCE` ← AppHost — `ledger.md:~466-472` `Awareness`/`AwarenessBeat`/`AwarenessLane` over `DrainSpec`/`DrainQueue`; the beat PRODUCER is `Rasm.AppHost/Wire/Companion.cs` (codemap: "Multi-process modality axis"), transport is `DrainSurface`. No Persistence ledger row. HOLD.

**Sibling-stale (twelve counterpart obligations) — ALL HOLD:**
- Compute: `:111` (→ `Rasm.Persistence/Sync`, deleted), `:115` (→ `Query/pipeline`, deleted), `:116` (→ `Store/quality`, deleted), `:119` (→ `Sync`, deleted + `Confluent.SchemaRegistry.Serdes.Protobuf` a `[V3]`/`[V7]` package).
- Bim: `:91` (→ `Query/federation`, deleted — the federation row), `:95` (→ `Store/quality`, deleted — the quality row), `:101` (→ `Sync/annotation`, deleted — Sync-era), `:102` (→ `Sync/schedule`, deleted — Sync-era), `:104` (→ `Sync`, deleted — Sync-era Speckle).
- AppHost: `:71` (→ `Query/transaction`, deleted), `:73` (→ `Sync/egress`, deleted), `:85` (`Observability/Health.cs → Rasm.Persistence/Store` `[HEALTH_PROBE]: HealthContributorRow fold over Npgsql/Redis/Kafka driver`).

### E10 — Type-rekey gap (Element side) — **HOLD** (register anchors exact; the load-bearing evidence is sharper at `element.md:295`)

- `merge.md:283` — `static Option<(string External, NodeId Id)> ExternalKey(Node node) => node is Node.Object { ExternalId: var external } obj ? external.Map(...) : None;` — EXACT. Aligns ONLY `Node.Object` bearing `ExternalId`; every non-`ExternalId` Object and every non-Object returns `None`, so a derived-`Type` re-key reads as delete-all+insert-all (the exact defect `Reconcile` exists to prevent). Corroborated by `merge.md:14/:18` ("`Reconcile` aligns on the stable `Node.Object.ExternalId` ... 1:1 IFC GlobalId").
- `ELEMENT Graph/element.md:40-48` — `NodeId.RootedType(ReadOnlySpan<byte> typeSeed)` (:48) over the "Representations-EXCLUDED canonical seed (`Node.Object.ToTypeSeedBytes`)" (:44) — EXACT.
- `Classifications` in the Object shape — constructor param `Seq<Classification> classifications = default` at `:225`, property `[property: UnorderedEquality] public Seq<Classification> Classifications` at `:236` — EXACT.
- **Load-bearing disk proof (sharper than the register's `:225`):** `element.md:294-298` `WriteObject` — the seed writer both the full hash and `ToTypeSeedBytes` share via the `includeRepresentations` toggle. `:295` writes `o.Classifications` (the plural Seq) UNCONDITIONALLY; `:296` (`if (includeRepresentations)`) is the ONLY conditional region. Therefore `ToTypeSeedBytes(includeRepresentations:false)` INCLUDES the full `Classifications` set — the derived-`Type` NodeId IS classification-dependent today, confirming the `[V8]`a demand (exclude `Classifications` from the seed as `Representations` already are).

### E12 — Mandate 1+2 clean (count-prefix + re-band) — **HOLD** (every archive/geometry anchor exact)

- Count-prefix: `DECISION:59` `### [AMENDMENTS]`; `DECISION:63` "Canonical-bytes count prefix (owner: resolve-residuals, before LEG-BIM). `Node.ToCanonicalBytes` propertySet/quantitySet arms gain `w.Ordinal(...)`" — EXACT. `DECISION:73` — the blast-radius row `| Rasm.Persistence (ElementGraph store) | ... | ZERO structural — no seam shape, hasher, or ca[nonical] ...` — EXACT (the "amendment IS the migration / planning-phase store" framing is the amendments-section law; the Persistence row is the ZERO-structural attestation).
- Composition on disk: the seam writer is composed, not re-spelled — `Element/Graph/delta.md:98-100` folds `Header.CanonicalBytes` "the SAME bytes the `Projection/address#CONTENT_ADDRESS OfGraph` snapshot header key composes, never re-spelled here"; the count-prefix law is enforced (`element.md:281` propertySet arm already carries `w.Ordinal(p.Bag.Values.Count)`, `:295` classifications `w.Ordinal(o.Classifications.Count)`).
- No node-canonical golden pinned: `Rasm/.planning/Spatial/reconciliation.md#ONE_WIRE_FIXTURE_CORPUS` (`:114`) — fixture `[01] CANONICAL_BYTE_IDENTITY` is REAL (52-byte int32-LE adjacency, `XxHash128`), fixture `[04] CRDT_OP_SET` is a DESIGN-PIN naming `Rasm.Persistence/Version/commits#CRDT_ALGEBRA` the producer (`:129`). No node-canonical (Node.ToCanonicalBytes) golden is corpus-pinned. Confirms `[SEAM_AND_RAIL_LAW]`'s "the commits leg PINS the `MvRegister`/`opMerge` op-set".
- Re-band: `DECISION:679` re-band gate "Entry check FIRST: probe that no 25xx Fabrication code crosses a persisted or wire boundary" — EXACT. Fabrication faults re-band to `FaultBand.Fabrication + 1..+10` = 2701-2710 (`DECISION:700-709`). `FaultBand` `[SmartEnum]` registry at `DECISION:141-149` (Component 2300 / Generation 2350 / Geometry 2400 pinned-mirror-of-kernel / Material 2450 / Projection 2470 / Element 2500 / Bim 2600 / Fabrication 2700) — the exact registry `[V4]` mirrors. Forward constraint `IDEAS:26-32` (`[FABRICATION_PROGRAM_DURABLE_ROWS]`) cites dead `csharp:Rasm.Persistence/Schema/ddl#IDENTITY and Store` (`:28,30`) — EXACT stale anchor; future durable rows must decode 2701-2710. Persisted-boundary probe passes: Persistence owns 5xx/771/82x-83x/54x bands, none in 25xx, so no Fabrication 25xx crosses a Persistence persisted receipt.

---

## [02]-[VERIFY-OR-DIE MEMBER PROOF] (lane seam members)

Kernel + Element seam members read off the LANDED fences (planning-scoped, no assembly to decompile). Every `[SEAM_AND_RAIL_LAW]` member in this lane resolves.

| Member | Owner (file:line) | Verdict |
|---|---|---|
| `ContentAddress.Of(ReadOnlySpan<byte>)` (raw-hash / merge's "span form") | `Element/Projection/address.md:48` | REAL |
| `ContentAddress.Of(UInt128)` (precomputed wrap — the SpineRef-resolution carrier) | `address.md:53` | REAL |
| `ContentAddress.Of(Node, double tolerance)` (id-inclusive node address — `timetravel.md:187` spelling) | `address.md:58` | REAL |
| `ContentAddress.OfGraph(ElementGraph)` (order-independent snapshot address) | `address.md:69` | REAL — name is `OfGraph`, NOT `Of(ElementGraph)` |
| `ContentAddress.Verify(Node, tolerance, Op)` / `Verify(ElementGraph, Op)` | `address.md:109` / `:125` | REAL |
| incremental `OfGraph(prior, delta)` (delta-composable) | — | ABSENT (only full-recompute `OfGraph`); consumer contract per `[V9]`, NOT phantom-claimed — consistent |
| `GraphDelta` monoid + `ToCanonicalBytes(double)` + DELTA_MONOID address law | `Graph/delta.md:57`(Merge)/`:90`(ToCanonicalBytes)/`:55`,`:253`(address law) | REAL |
| `NodeId.RootedType(ReadOnlySpan<byte>)` | `Graph/element.md:48` | REAL |
| `Node.Object.ToTypeSeedBytes(double)` (Representations-excluded, Classifications-INCLUDED) | `element.md:242`; seed comp `:294-298` | REAL |
| kernel `ContentHash.Of` seed-zero entry | `address.md:48` composes it; kernel `Rasm/ARCHITECTURE.md:77` | REAL |
| kernel `NamingHashOps.Encode(CanonicalTopology)→UInt128` / `Reconcile(NameTable, CanonicalTopology)→Fin<NamingHash>` | `Rasm/.planning/Spatial/reconciliation.md:88` / `:104` | REAL |
| AppHost `CostVector` / `CostUnit` (per-unit Budget vector) | `Agent/capability.md:62` / `:54-59` | REAL |
| AppHost `StepStateSeam.InFlight` / `CrashResume` / `FencingToken` | `Runtime/orchestration.md:271` / `:282` / `:261` | REAL |
| `AsOfKey` (icechunk cross-runtime as-of key) | — | PHANTOM (zero hits; TASKLOG:48 on deleted `Version/snapshots`); `[V12]` mints on `timetravel` |
| `FederatedEntity`/`FederatedPlan`/`PlanNode`/`SourceKind`/`Query/federation` | — | PHANTOM (E1); `[V1]` reintroduces or retires |

**Naming law resolved on the seam side:** `ContentAddress` provides `Of(span)` / `Of(UInt128)` / `Of(Node, tol)` and the separately-named `OfGraph(ElementGraph)` — there is NO `Of(ElementGraph)` overload. The `OfGraph`-vs-`Of(ElementGraph)` "naming split" the brief flags to the Element campaign is ALREADY unified on the owner; the split exists only in Persistence prose that may spell `Of(graph)`. Any Persistence page spelling `ContentAddress.Of(<ElementGraph>)` is a phantom-member reference — the real member is `OfGraph`. Both merge's `Of(span)` and timetravel's `Of(Node, tol)` are real; the "overload split" is a legitimate two-entry set, so the `[SEAM_AND_RAIL_LAW]` "verify the overload set or unify" resolves as VERIFIED (the semantic question of which overload each Persistence site should pick is a Version-lane interior concern).

---

## [03]-[PER-ANCHOR DEEP FINDINGS]

### [03.1] AppHost ledger (`libs/csharp/Rasm.AppHost/ARCHITECTURE.md`)

- Codemap confirms AppHost owns `Wire/Outbox.cs` ("Transactional outbox + dead-letter relay over the watermark-advancing dispatch sweep") and `Wire/Coordination.cs` ("Cluster membership/election/distributed-lock over the fenced lease") as CONSUMERS — they compose the Persistence backing store the four PORT rows demand. AppHost's outbox/coordination are the demanding half; Persistence owns neither the store nor the ledger rows.
- The four PORT rows (`:76-79`) each tag `ONE_FENCED_LEASE_STORE` / `ONE_OUTBOX_EGRESS_SPINE` — the singletons `[V2]`'s `Store/coordination.md` must NAME (the Marten event stream IS the outbox; the owner names it + mints the drain cursor).
- `:80` `Wire/Coordination.cs → Rasm.AppHost/Sandbox/Provisioning.cs # MembershipView.Serving roster + RoleElection conductor lease` — INTERNAL AppHost (not a Persistence seam), but it composes the fenced lease, so the membership rows `[V2]` mints are read by this in-process consumer.
- `:85` HEALTH_PROBE driver roster `Npgsql/Redis/Kafka` — STALE against the Persistence-side counterpart `ARCH:63` which names ONLY `Npgsql driver reachability`. The two sides DISAGREE; Persistence:63 is already prune-aligned (`[V7]` removes raw Redis, `[V3]` re-disposes Kafka), so AppHost:85 is the stale side and must drop Redis/Kafka. Corrected AppHost:85 target: `Store/provisioning` (the Persistence-side owner at ARCH:63), roster = Npgsql only.
- `:69` cache-L2 PORT + `:70` recovery-objective PORT (`Version/recovery`, live) + `:72` KMS-unwrap PORT (`Element/identity`, live, `[V5]`a leaves it unchanged) + `:74/:75` HLC/identity-store PORTs — all live counterparts; only `:71/:73` are stale-page.

### [03.2] Compute ledger (`libs/csharp/Rasm.Compute/ARCHITECTURE.md`)

- Four stale rows (`:111,115,116,119`) confirmed dead-page. Corrected targets (counterpart obligations): `:111` FastCDC content-key delta → `Element/codec#CONTENT_CHUNKER` (FastCDC lives there); `:115` parse-to-canonical-bytes Extract → `graph#STORE_RAIL` (per TASKLOG:37's transaction→graph fold) or `Ingest`; `:116` geometry-derived anomaly rule source → the columnar/provisioning verification surfaces (per `[V12]` quality re-anchor); `:119` Protobuf Kafka topics → retire with the `[V3]`/`[V7]` streaming disposition (the `Confluent.SchemaRegistry.Serdes.Protobuf` leg is a sink-row codec column, not a live seam).
- `COMPUTE:99` (VECTOR_CODEBOOK) is a LIVE counterpart declaration — it is the missing-from-Persistence-ledger wired-undeclared seam (E6). Post-`[V5]`b split it targets `Query/retrieval`.
- Many LIVE Compute→Persistence rows resolve cleanly (`:85,86,98,101,102,103,104,105,106,107,108,109`) — cache/blob/ledger/retention/commits consumers all target live pages; no additional stale rows beyond the four enumerated.

### [03.3] Bim ledger (`libs/csharp/Rasm.Bim/ARCHITECTURE.md`)

- Five stale rows (`:91,95,101,102,104`) confirmed dead-page. Corrected targets: `:91` federation AuditEntry mutation log → the `[V1]` reintroduced `Query/federation` OR the `ledger#CHANGEFEED` (if retire); `:95` IFC validation → columnar/provisioning quality surfaces (`[V12]`); `:101` durable annotation + CDE op-log → `ledger#CHANGEFEED` (the AppUi/Bim durable-annotation concern, `[V12]`); `:102` P6/MS-Project 4D → the NEW `[V11]` `Ingest/schedule.md` (the MPXJ codec + durable schedule rows); `:104` Speckle Base import → `Version/ledger` (Speckle `SyncTransport` is ledger's one live consumer per `[PACKAGE_PRESSURE]`).
- **13th sibling drift (un-enumerated): `BIM:81`** `Semantics/geospatial ← Rasm.Persistence/Store # [TRANSPORT]: GDAL /vsimem fsspec dataset open + OGR Arrow C-stream GeoParquet/FlatGeobuf columnar ingest` targets the `/Store` FOLDER with no specific page — a geospatial columnar-ingest seam with no clear Persistence owner (maps to `columnar.md` or the `Ingest/` axis). Not dead but unhomed; the DECISION should home or retire it.
- LIVE Bim→Persistence rows: `:71` (columnar `BimOpenSchemaProjection` — the Ara3D.BimOpenSchema live consumer, E2 keep-evidence, counterpart of `ARCH:56`), `:92/:94` (Version/commits `BimCommit`/`MergeBase`), `:93` (Element/codec SnapshotCodec), `:108` (Store/blobstore IFC/BREP, counterpart of `ARCH:60`). `:87,:88,:90` target `/Query` folder (imprecise, resolve to cache — soft anchor drift, not dead).
- **`BIM:94` naming drift:** the row spells `CommitGraph.MergeBase`; the owner is `commits.md:108` `MergeBase` (a static, no `CommitGraph` type). The counterpart obligation should align the spelling.

### [03.4] Persistence own seam ledger (`ARCHITECTURE.md:41-64`) — E6 both directions

- `ARCH:42` (kernel content-key seam-reference, `[PLACEMENT_LAW]`) and `ARCH:53` (AppHost identity KMS PORT, splits in `[V5]`a) confirmed exact and live.
- The own ledger's declared-unwired (`:50/:55/:57`) and total absence of the four coordination PORT counterparts are the core own-side E6 defects (above).
- No additional own-ledger rows point at deleted own pages beyond `:50/:55/:57`; `:52/:56/:58/:59/:60/:63/:64` are all live sibling counterparts.

### [03.5] Element seam hooks — E10/E11

- `Graph/delta.md`: `GraphDelta` monoid (`:57` `Merge`, cancellation/coalescence), `ToCanonicalBytes(tolerance)` (`:90`), the DELTA_MONOID address law (`:55` Merge comment, `:253` research card) — the `[V9]` incremental-`OfGraph` HOOK. `:90-100` `ToCanonicalBytes` folds `n.ToCanonicalBytes(tolerance)` per node — the composition point where the `[V9]`/geometry-`[V2]` parametric digest must fold (else `OfGraph` is blind to a parametric-body edit). All EXACT.
- `Projection/address.md`: the full `ContentAddress` overload set + `CanonicalWriter` codec + `OfGraph` full-recompute. The incremental `OfGraph(prior, delta)` is ABSENT (consumer contract). `OfGraph` folds `Header.CanonicalBytes` + sorted node addresses + sorted edge bytes (`:69-84`) — the exact snapshot address `[V9]`/`[V12]` `AsOfKey` and the recovery content-identity proof lean on.
- `Graph/element.md`: `NodeId.RootedType` (`:48`) / `ToTypeSeedBytes` (`:242`) / `WriteObject` seed (`:294-298`) — E10 proof that the Type seed is classification-dependent (`:295`).

### [03.6] Geometry brief (`RASM-CS-GEOMETRY-BRIEF.md`) — V2 + E12

- `V2_PARAMETRIC_TIER` (`:96`) — the parametric surface-development tier. Obligation (a) PARAMETRIC CONTENT IDENTITY at `:98` carries the EXACT Persistence-`[V9]` waterfall verbatim: "the projection is a COMPONENT of the parametric node's `ToCanonicalBytes(tolerance)` contribution the Element seam `ContentAddress.Of(Node)`/`OfGraph` folds (`Rasm.Persistence/.planning/Element/codec.md:151-152`), never a sibling SpineRef-resolution key beside it ... kernel emits the digest, Persistence mints the key". The sharpened contract IS on disk in the geometry brief with Persistence named the demanding consumer. HOLD.
- `E12` (`:165`) "Seam-ledger residue | landed spine `ARCH:73-99` vs fences not yet composing declared rows (index wire, reconciliation identity entry, pack row ...)" — the geometry-side E12 explicitly names the "reconciliation identity entry", the counterpart of Persistence `ARCH:55`. `V8_SPATIAL_CONTRACT` (`:122`) closes it: "The reconciliation→Persistence seam anchor unifies to one row (`reconciliation.md:3` / `ARCHITECTURE.md:79` / `Rasm.Persistence/ARCHITECTURE.md:55` today)". The two-sidedness the Persistence E6 asserts is confirmed from the geometry side; the geometry campaign owns the kernel-side unification (`[V8]` + `Rasm/ARCHITECTURE.md:79` re-point).

### [03.7] Archive decision (`.archive/RASM-COMPONENT-PARADIGM-DECISION.md`) — E12

- `[AMENDMENTS]` (`:59`), count-prefix amendment (`:63`), Persistence ZERO-structural blast-radius row (`:73`) — all EXACT (see E12).
- Re-band gate (`:679`) + Fabrication `+1..+10` = 2701-2710 (`:700-709`) + `FaultBand` `[SmartEnum]` registry (`:141-149`, `sealed partial class FaultBand` with `new(<band>, owner:)` rows) — all EXACT. This is the type-enforced-uniqueness precedent `[V4]` mirrors (a duplicate integer fails at type initialization).

---

## [04]-[CROSS-CUTTING FINDINGS]

1. **The coordination gap is BILATERAL and precisely specified.** AppHost declares four PORT seams (`:76-79`) AND fully specifies the Persistence-owned `StepStateSeam` ripple (`orchestration.md:257-282`: `Commit`/`Load`/`InFlight` by fenced CAS + `FencingToken` + TenantId-RLS column) AND meters a real multi-unit `CostVector` (`capability.md:62`). Persistence carries ZERO owner pages AND ZERO ledger rows for any of it. `[V2]`'s `Store/coordination.md` must mint BOTH the owner and the four counterpart ledger rows; the per-unit-vector Budget debit and the READ cases (`InFlight`, expired-lease scan) are hard consumer demands, not options.

2. **The reconciliation `GeometryHash` seam is a THREE-WAY anchor disagreement AND a digest-semantics gap.** (a) Targets disagree: kernel `reconciliation.md:3` → `Persistence/version-control#STRUCTURAL_DIFF` (merge); kernel `ARCH:79` → `Rasm.Persistence/Query` (folder, imprecise); Persistence `ARCH:55` → `Query/topology` (specific, WRONG — zero reconciliation refs). (b) Digest semantics diverge: `merge.md:69-73` `GraphNode.GeometryHash` = "the digest of the Object's WHOLE `Representations.ByIdentifier` map", a Persistence-COMPUTED Representations digest — NOT the kernel `NamingHashOps.Encode` canonical-ADJACENCY digest the kernel intends merge to consume. So the seam is not merely one-sided; the two sides compute DIFFERENT hashes under one name. The geometry-`[V8]` "one-row unification" must resolve BOTH the target (→ `Version/merge#STRUCTURAL_DIFF`) and whether merge's GeometryHash composes the kernel adjacency `Encode` or stays a distinct Representations digest. Persistence re-anchors `ARCH:55` → `Version/merge`; the geometry campaign re-points `Rasm/ARCHITECTURE.md:79` onto the same home.

3. **`[V8]`a and `[V8]`b are coupled.** `[V8]`b wants a "classification-INDEPENDENT Type natural key ... the representation-excluded content seed identity, mirroring the `ExternalKey` shape" for `merge.md` `Reconcile`. But the seed the seam provides (`ToTypeSeedBytes` / `NodeId.RootedType`) is classification-DEPENDENT today (`element.md:295` writes `Classifications` unconditionally). So `[V8]`b's Type correlation key cannot reuse `ToTypeSeedBytes` as a classification-independent key UNTIL `[V8]`a lands (exclude `Classifications` from the seed) OR Persistence authors a separate classification-excluded projection. The DECISION should record this dependency: `[V8]`b's merge-side Type-correlation row waits on the kernel-side `[V8]`a seed change (an out-of-scope Element/Materials edit) or ships its own classification-excluded seed.

4. **The `[V10]` MergeBase O(V²) defect has a NAMED sibling consumer.** `commits.md:108` `MergeBase` is composed by Bim (`BIM:94` `Review/versioning`; `commits.md:151` "`BimCommit` lands as one `CommitNode`; bases on `MergeBase`"). The near-linear re-shape `[V10]` demands protects a cross-package seam, not just an internal path — raising its priority.

5. **The `CRDT_OP_SET` parity fixture is a live cross-package DESIGN-PIN.** `Rasm/.planning/Spatial/reconciliation.md:129` names `Rasm.Persistence/Version/commits#CRDT_ALGEBRA` the producer of the `MvRegister`/`opMerge` op-set fixture. The commits leg's obligation to PIN it (turning the DESIGN-PIN REAL) is a kernel-fixture-corpus contract, not a local choice — the divergent-delivery folds must converge byte-identically under the one kernel seed.

6. **Minor register anchor drift (E8, adjacent lane — for register hygiene).** `recovery.md` count-as-minutes is at `:180` (register E8 says `:181`), segment-size-as-throughput at `:171` (register `:172`), `MeetsRpo` consumer at `:145`/`:150` (register `:151`) — all off by ~1. Real defects, anchors shifted; the Version-governance surveyor owns the fix.

---

## [05]-[COUNTERPART-OBLIGATION TABLE] (charter-as-it-should-be; sibling interiors stay out of edit scope)

The twelve+ stale sibling→Persistence rows the DECISION lists as counterpart obligations with corrected targets:

| Sibling row | Current (dead) target | Corrected target | Owning campaign |
|---|---|---|---|
| `COMPUTE:111` FastCDC content-key delta | `Rasm.Persistence/Sync` | `Element/codec#CONTENT_CHUNKER` | Compute |
| `COMPUTE:115` parse-to-canonical Extract | `Query/pipeline` | `graph#STORE_RAIL` / `Ingest` | Compute |
| `COMPUTE:116` anomaly rule source | `Store/quality` | columnar/provisioning verification surface | Compute |
| `COMPUTE:119` Protobuf Kafka topics | `Sync` | retire w/ `[V3]`/`[V7]` streaming disposition | Compute |
| `BIM:91` federation AuditEntry log | `Query/federation` | `[V1]` federation owner OR `ledger#CHANGEFEED` | Bim |
| `BIM:95` IFC validation rules | `Store/quality` | columnar/provisioning quality surface | Bim |
| `BIM:101` durable annotation + CDE | `Sync/annotation` | `ledger#CHANGEFEED` | Bim |
| `BIM:102` P6/MS-Project 4D | `Sync/schedule` | `[V11]` `Ingest/schedule.md` | Bim |
| `BIM:104` Speckle Base import | `Sync` | `Version/ledger` (Speckle SyncTransport) | Bim |
| `BIM:81` GDAL/OGR GeoParquet ingest (un-enumerated) | `/Store` (folder) | `columnar.md` / `Ingest/` axis, or retire | Bim |
| `BIM:94` `CommitGraph.MergeBase` (spelling) | — (spelling only) | align to `commits#MergeBase` | Bim |
| `APPHOST:71` drain 2PC in-doubt | `Query/transaction` | `graph#STORE_RAIL` (`[V12]` prepared-tx RETIRE) | AppHost |
| `APPHOST:73` keyed OutboundHop egress | `Sync/egress` | `[V3]` egress owner (`Version/egress`) | AppHost |
| `APPHOST:85` health-probe driver roster | Npgsql/Redis/Kafka | `Store/provisioning`, Npgsql-only (matches `ARCH:63`) | AppHost |

Own-side ledger corrections the DECISION emits (Persistence-owned, in scope):
- ADD wired-undeclared rows: `Query/{lane→retrieval} ⇄ Rasm.Compute/Model/embedding` (VECTOR_CODEBOOK); `Query/cache ⇄ Rasm.AppHost/Runtime` (L2 partition); `Version/ledger ← Rasm.AppHost/Wire/companion` (PRESENCE PeerRoster beats).
- ADD four coordination PORT counterparts on the NEW `Store/coordination.md`.
- CORRECT `ARCH:50` (mint `AsOfKey` on `timetravel`), `ARCH:55` (retarget → `Version/merge#STRUCTURAL_DIFF`, resolve the adjacency-vs-Representations digest), `ARCH:57` (`[V1]` reintroduce/retire the Substrait half).

---

## [06]-[SUMMARY]

Every assigned register row HOLDS on disk with the enumerated anchors EXACT; the ONE internal drift is `ARCH:55`'s target (`Query/topology` → should be `Version/merge#STRUCTURAL_DIFF`). All twelve stale sibling rows confirmed dead-page. All verify-or-die seam members in this lane resolve (Element `ContentAddress`/`GraphDelta`/`NodeId.RootedType`, kernel `ContentHash.Of`/`NamingHashOps`, AppHost `CostVector`/`StepStateSeam`); `AsOfKey` and the federation owner are the only genuine phantoms, both dispositioned by verdicts. The perimeter is fiction exactly as the VERDICT claims: the coordination gap is bilateral and fully-specified on the AppHost side, the reconciliation seam is a three-way anchor + digest-semantics disagreement, and the card pool carries phantom-realization AND deleted-page claims beyond the register's enumeration (IDEAS:57-61, TASKLOG:46-47, TASKLOG:24-vs-ARCH:51). No edits made outside `.claude/scratch/design-cs-persistence/`.
