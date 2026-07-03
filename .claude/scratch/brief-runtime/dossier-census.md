# RUNTIME CENSUS DOSSIER — libs/python/runtime/.planning

Lane: census (read-only). Corpus: 13 design pages (3164 LOC) + ARCHITECTURE.md + README.md + IDEAS.md + TASKLOG.md, all deep-read. Stance: hostile. All line refs are `path:line` under `libs/python/runtime/.planning/` unless prefixed `../`.

The corpus is high-craft at the PAGE level (dense ADT/AOP, per-page boundary ledgers, `.api`-anchored) but carries ONE campaign-defining structural defect (a 6-module import SCC that crashes at import time), a PHANTOM wire-vocabulary owner, a mislocated cross-`libs/` credential seam, and systematic sibling-tier coupling in the base foundation. Page polish masks graph-level illusion.

---

## [A] REAL INTRA-RUNTIME DEPENDENCY GRAPH (from every fence's `from rasm.runtime.X import`)

Module names are FLAT (`rasm.runtime.<leaf>`) in 100% of imports — NO nested `rasm.runtime.<folder>.<leaf>` exists (verified). This already contradicts ARCHITECTURE.md's nested codemap (`observability/receipts.py`, etc.). Edge = importer → imported:

```
faults            → (none — ROOT; only external substrate)
clock             → faults
content_identity  → faults                              # identity.md §2 [02]-[IDENTITY]
seed_reproduction → content_identity, faults, receipts  # identity.md §3 [03]-[SEED_REPRODUCTION]
evidence          → faults, receipts
roots             → faults, resilience
wire              → faults, resilience, clock, shapes(PHANTOM), _pb2(PHANTOM)
resilience        → faults, metrics, receipts
receipts          → faults, lanes
metrics           → faults, lanes, telemetry
telemetry         → faults, admission
admission         → faults, resilience, clock
lanes             → faults, resilience, metrics, receipts, content_identity
serve             → faults, resilience, clock, admission, receipts, wire
```

### [A.1] STRONGLY-CONNECTED IMPORT CLUSTER (hard import-time crash) — HEADLINE

SCC = **{lanes, metrics, receipts, resilience, telemetry, admission}** (6 of 13). Every edge is `from X import <name>` (NOT `import X`), so a partially-initialized module raises `ImportError: cannot import name … from partially initialized module`. Confirmed unresolvable 2-cycles + wider loops:

- `lanes ↔ metrics`: lanes.md:51 `from rasm.runtime.metrics import Metrics` ⟷ metrics.md:40 `from rasm.runtime.lanes import DRAIN_COLUMNS, DrainOutcome, DrainReceipt`. Both at module top before any definition. Whichever loads first, the re-entrant `from … import <name>` hits an undefined name (metrics needs `DRAIN_COLUMNS` defined at lanes.md:79; lanes needs `Metrics` at metrics.md:201) → CRASH regardless of order.
- `lanes ↔ receipts`: lanes.md:52 `from rasm.runtime.receipts import Receipt, Redaction, Signals` ⟷ receipts.md:53 `from rasm.runtime.lanes import DRAIN_COLUMNS, DrainReceipt`. Same unresolvable 2-cycle (`Receipt` at receipts.md:107 vs `DRAIN_COLUMNS` at lanes.md:79).
- `admission → resilience → metrics → telemetry → admission`: admission.md:195 → resilience.md:49 → metrics.md:41 (`from rasm.runtime.telemetry import latched`) → telemetry.md:74 (`from rasm.runtime.admission import PROFILE_POLICY, RuntimeProfile`).
- `resilience → metrics → lanes → resilience` and `resilience → receipts → lanes → resilience` (lanes.md:53 `from rasm.runtime.resilience import RetryClass, guard`).

Root causes (the pivots that close the SCC):
- **DrainReceipt/DrainOutcome/DRAIN_COLUMNS taxonomy lives in `lanes`** (lanes.md:59-110, 79) but is consumed UP by metrics + receipts, while lanes' `drained`/`feed` observing aspects (lanes.md:227-261) consume DOWN into Metrics+Signals — the taxonomy and its observers are co-located, forcing the mutual cycle.
- **`latched` lives in `telemetry`** (telemetry.md:228) but is a generic one-shot guard consumed by metrics (metrics.md:41,207) — this single edge closes the admission↔resilience↔metrics↔telemetry loop.
- **`RETRY_HOOKS` in resilience** (resilience.md:49-50,303) imports Metrics + Signals so the retry MECHANISM depends on the observability SINKS.

SHOULD-be DAG: extract the drain taxonomy (`DrainReceipt`/`DrainOutcome`/`DRAIN_COLUMNS`) and `latched` to the base tier (into `faults` or a tiny shared-vocab owner), and relocate the observing aspects (`drained`/`feed`, the `RETRY_HOOKS` composition, the metrics/receipts wiring) into a COMPOSITION owner above lanes/resilience. Then faults→(clock,vocab)→(resilience,receipts,metrics,telemetry,content_identity)→(admission,lanes,wire,roots,evidence)→serve is acyclic.

### [A.2] PHANTOM / UNWIRED MODULE SEAMS (wired-undeclared, no owner page)

- **`rasm.runtime.shapes`** (wire.md:116-120) imports the 16 canonical `msgspec.Struct` wire types — `ArtifactFrame, FaultDetail, GenerateRequest, GraphChunk, GraphDiffRequest, GraphDiffResponse, InferRequest, InferResponse, QueryRequest, QueryResponse, SolveRequest, SolveResponse, SubtreeFetchRequest, TokenChunk, TransactionReceipt, TransactionRequest`. NO `shapes` design page exists, NO `shapes` node in ARCHITECTURE codemap, NO ledger seam. `WIRE_REGISTRY`/`_PROTO_VOCABULARY` (wire.md:151-172) bind these phantom structs to `channels_pb2` messages. `FaultDetail`'s field set is described in PROSE (wire.md:102 `(package, code, case, message, evidence, correlation, hlc_physical, hlc_logical, tenant)`) but declared in NO fence — the entire msgspec interior half of every proto pair is ILLUSORY. This is the largest prose-vs-fence gap in the corpus.
- **`rasm.runtime._pb2.channels_pb2`** (wire.md:114) — generated protobuf module. Legitimately generated (not a design page), but NO page documents the generation seam (which C# `.proto` → `channels_pb2`, the build step, or the `grpcio-tools` compile). serve.md:33 mentions "generated stubs arrive from `grpcio-tools` compiling the C# `.proto`" but no owner page fixes the codegen contract.
- **`content_identity` vs codemap `identity.py` vs seam `evidence/identity`** — three-way name drift. The module is `content_identity` in every fence that names it (identity.md:226, lanes.md:49) but the codemap node (ARCHITECTURE.md:27) is `identity.py`, the folder is `evidence/`, and every [02]-[SEAMS] endpoint is `evidence/identity`. Additionally identity.md §3 SEED_REPRODUCTION `from rasm.runtime.content_identity import …` (identity.md:226) — if identity.md is ONE file this is a self-import (broken); if TWO files the codemap under-declares `seed_reproduction.py`. `ContentIdentity` is DEFINED at identity.md:158 and self-imported at 226.

---

## [B] csharp SEAM LEDGER DIFF ([02]-[SEAMS] vs fences, both directions)

Wired-undeclared csharp producers: NONE — every `csharp:*` reference in a fence/prose maps to a ledger row (`Rasm.AppHost/Runtime/ports`, `Rasm/Geometry/Spatial/reconciliation`, `Rasm.Element/Projection/address`, `Rasm.Persistence/Version/commits`, `Rasm.AppHost/Observability/telemetry` all covered). Declared-unwired / MISATTRIBUTED rows:

- **ROW MISFILED (declared-unwired + wired-undeclared)**: ARCHITECTURE.md:41 `execution/admission ← csharp:Rasm.AppHost/Runtime [WIRE]: CredentialPem`. admission.md carries NO `CredentialPem` — admission's `Credential` (admission.md:228) is a LOCAL `(username, SecretStr)` secret-boundary value object, unrelated to the C# `CredentialPolicy`/PEM. The real consumer is **`transport/serve`**: serve.md:70-89 `Credential` `@tagged_union` decodes the C# `CredentialPolicy` five-row axis (`insecure_loopback`/`tls`/`mtls`/`bearer`/`composed`) and names the 4 PEM/token outbound rows. So the credential-axis seam is DECLARED on the wrong owner (admission) and UNDECLARED on the true owner (serve — the serve ledger rows ARCHITECTURE.md:43-47,50 carry no credential row).
- **DOUBLE-ATTRIBUTION (redundant declared)**: ARCHITECTURE.md:40 `execution/admission ⇄ csharp:Rasm.AppHost/Runtime [PORT]: CausalFrame Hlc two-half + Tenant` duplicates ARCHITECTURE.md:37 `clock/clock ← csharp:Rasm.AppHost/Runtime [PORT_RECORDS]`. admission.md [CLOCK_CONSUMPTION] (admission.md:344) states admission imports `CausalFrame`/`Tenant` from `rasm.runtime.clock` and "re-spells NOTHING"; the decode seam is clock's single-mint. admission has NO direct AppHost wire (fence imports only `rasm.runtime.clock`, admission.md:46). One seam, two owners.
- **LAYERING RESTATEMENT (serve restates wire's seams)**: ARCHITECTURE.md:43 (`serve ⇄ Compute PROTO_VOCABULARY`), :46 (`serve ⇄ Persistence/Version CrdtOpWire`), :47 (`serve ⇄ Persistence/Sync OpLogEntry`) restate wire's rows :38 (`wire ⇄ Compute PROTO_VOCABULARY`), :39 (`wire ⇄ Persistence CrdtOp`). serve.md composes wire's codec (serve.md:64,226) and owns no CRDT fence method; the CRDT/proto cross-`libs/` seams are wire's. Defensible as "wire terminates at serve" but inflates the ledger 3 rows.

python: seams (ARCHITECTURE.md:51-58) are all inbound/outbound sibling relations runtime does not wire from its side (correct — README:3 "references no sibling"). Only `python:testing` appears in a fence (identity.md:216, the future SeedReproduction driver). The 8 python: rows have live TASKLOG ripple counterparts (`ARTIFACT_PIPELINE_KEYED_CONSUMER`, `DATA_LINEAGE_RECEIPT`, `DATA_TRANSPORT_DSN`) — consistent, not defects.

---

## [C] PER-PAGE ASSESSMENT

Verdict 1-10 = structural soundness + doctrine conformance (10 = ship-ready).

### faults.md — reliability/faults.py — VERDICT 9
Base of the graph, imports no runtime module. One `BoundaryFault` union + `RuntimeRail` + `CLASSIFY` table + `boundary`/`async_boundary`/`trapped` tri-shape + `traversed` disposition-fold + `railed` builder. CLASSIFY row-order load-bearing (faults.md:120-129, TimeoutError before OSError). Dense, total, PEP-695 inline generics.
Defects: none structural. Minor: `railed = effect.result[Any, BoundaryFault]()` (faults.md:208) erases per-bind element to `Any` (justified + documented).
Move pressure: it SHOULD host the extracted DrainReceipt taxonomy + `latched` to break the SCC (§A.1) — the base owner is the natural home for cross-cutting vocab.

### resilience.md — reliability/resilience.py — VERDICT 6
`RetryClass` StrEnum + per-member `Policy` rows + `guard`/`guarded`/`guarded_sync`/`retrying` triad + `RETRY_HOOKS` install. Dense mechanism.
Defects:
- **Sibling-tier coupling in the base foundation**: resilience.md:31-33 imports `adbc_driver_manager`, `adbc_driver_manager.dbapi`, `daft.exceptions` — DATA-tier packages ABSENT from runtime's README manifest (../README.md carries no daft/adbc/deltalake/pyiceberg). The host-free foundation imports data-analytics engines at module top just to name transient exceptions.
- **Domain over-reach in POLICY** (resilience.md:74-78, 257-289): rows `occt` (geometry/OCCT), `rpc` (geometry/compas), `lake-commit` (data), `remote-db` (data/adbc), `streaming` (data/daft) are sibling-domain retry classes in the base table. INCONSISTENT resolution: `rpc`/`lake-commit` use import-free `_named(qualname)` (resilience.md:265,273) to avoid coupling, but `remote-db`/`streaming` import the real types (`_adbc_transient`, `DaftTransientError`).
- **RETRY_HOOKS forces mechanism→sink dep** (resilience.md:49-50): imports Metrics + Signals, an SCC edge.
Charter as SHOULD: own the retry MECHANISM + runtime-native classes only (`object-store`/`http`/`ssh`/`wire`/`scan`/`secret`); sibling-domain classes either use the uniform import-free `_named` hook or are contributed by the owning sibling; move `RETRY_HOOKS` composition to the composition owner.

### clock.md — clock/clock.py — VERDICT 9
`Hlc`/`Ordering`/`ElementId`/`Tenant`/`CausalFrame` + `SLOTS` table. Single-mint law held, `convert(strict=False)` domain gate closing the `__init__`-bypasses-`Meta` gap (clock.md:33,186). Imports only faults. Sole decode site.
Defects: minor — `SLOTS: Final[dict[Slot, tuple[str,str]]]` (clock.md:165) is a mutable `dict` where every sibling table uses immutable `expression.Map`.
Move pressure: none — exemplary owner.

### content_identity (identity.md §2) — VERDICT 8 (docs), NAMING drift
`ContentIdentity`/`ContentKey`/`IdentityPolicy`/`IdentitySource` ADT + `of`/`key`/`derived`. `Buffer`-before-`Iterable` arm order load-bearing (identity.md:114). Dense.
Defects: **module-name drift** (§A.2): module `content_identity` vs codemap `identity.py` vs seam `evidence/identity` vs folder `evidence`. `ContentIdentity` defined identity.md:158, self-imported identity.md:226.
Move pressure: rename to one canonical spelling across codemap + ledger + module; decide whether SEED_REPRODUCTION is a second file.

### seed_reproduction (identity.md §3) — VERDICT 7
`SeedReproduction`/`CorpusFixture`/`ParityRow`/`ParityReceipt` corpus fold. REAL vs DESIGN_PIN fixture dispatch, `MATERIAL_LAYER_GOLDEN` [H7] first-class row. Good.
Defects: co-resides in identity.md but imports `content_identity` by name (identity.md:226) — implies a SEPARATE module the codemap (ARCHITECTURE.md:27, single `identity.py`) does not declare. Split/merge pressure: either declare `seed_reproduction.py` in the codemap or resolve the self-import.

### evidence.md — evidence/evidence.py — VERDICT 7 (largest page, 295 LOC; cohesion outlier)
`Evidence` union + `GrammarRegistry` (tree-sitter) + `ApiCatalogue` (importlib.metadata) + `EvidenceScan`. Genuinely dense (compile-once probes, `Map.filter`-total `CompiledProbe.of`, `did_exceed_match_limit` live grade).
Defects: **domain-placement outlier** — structural code-scanning (tree-sitter) + distribution reflection is a DEV-TOOLING concern feeding the external `assay code` rail, not a host-free EXECUTION-runtime concern. NO runtime module imports `evidence` (verified: no `from rasm.runtime.evidence`); its only consumer is the external assay tool. It sits beside faults/lanes/transport/clock without belonging to the execution foundation.
Move pressure: candidate to relocate out of `runtime` into a tooling/assay-owned surface, or justify explicitly. Its `tree-sitter-python`/`tree-sitter-typescript` deps are the only ones in runtime purely for tooling.

### admission.md — execution/admission.py — VERDICT 7
`RuntimeContext`/`RuntimeProfile`/`PROFILE_POLICY`/`FeatureGate`/`KILLSWITCH_FEATURE` + `SettingsAdmission`/`SecretTier`/`SECRET_LADDER`/`SecretBoundary`. Data-driven gate, `SecretShape`-overloaded resolve. Strong.
Defects:
- **`Credential` name collision** (admission.md:228 `class Credential(Struct): username, secret`) vs serve.md:70 `class Credential` (@tagged_union). Two different `Credential` types in one package — violates one-name-per-concept.
- **Hardcoded `SECRETS_MOUNT = "/run/secrets"`** (admission.md:223) — a Docker-secrets infra path baked as a constant.
- **In the SCC** (admission→resilience→…→telemetry→admission).
- `SettingsAdmission.scratch_root: DirectoryPath` required with no default (admission.md:239) — `SettingsAdmission()` raises at construction if the dir is absent, harsh for PACKAGE/TEST.
Charter: rename local `Credential` to `UserSecret`/`BasicCredential`; parameterize the secrets mount; break the SCC edge (admission→resilience is legit; the return path telemetry→admission via PROFILE_POLICY is the pivot to relocate or invert).

### lanes.md — execution/lanes.py — VERDICT 6 (SCC epicenter)
`LanePolicy.drain`/`offload` + `Admit` ADT + `ADMIT_TABLE` + `DrainReceipt` + `StagePlan` DAG + `LaneSource` feeder + `drained`/`feed` aspects. Extremely dense; the offload/trace-stitch/deadline-containment are well-reasoned.
Defects:
- **Owns the SCC pivots**: the DrainReceipt taxonomy (consumed up by metrics/receipts) AND the `drained`/`feed` observers (consuming down into Metrics/Signals) — §A.1.
- **`WORKER_BAND` DEAD + coupled** (lanes.md:89): defined but referenced NOWHERE in any runtime fence (verified single hit). Its comment couples to 8 named SIBLING folder internals (`exchange/detect`, `exchange/metadata`, `graphic/raster/io`, `measure`, `process`, `graphic/color/managed`, `media`) — a host-free-foundation constant naming downstream consumer interiors, with no ARCHITECTURE seam declaring those consumers.
- **`RetryClass.OCCT` reference** (lanes.md:170) leaks a geometry-domain retry class into the base concurrency owner.
Charter: extract DrainReceipt taxonomy + `drained`/`feed` observers (§A.1); export `WORKER_BAND` as a declared seam with ripple cards OR delete it; keep OCCT out.

### metrics.md — observability/metrics.py — VERDICT 7
One `INSTRUMENTS` table across both instrument disciplines, atomic `MetricState` swap, `latched` install, `measured`/`FAULT_OUTCOME` projection. Genuinely dense and correct on the SDK contract.
Defects:
- **In the SCC** (metrics↔lanes; metrics→telemetry for `latched`).
- **Artifacts-domain coupling** (metrics.md:137-138,177-178,187-188,251-258): `record_artifact` + `artifact.byte_volume`/`artifact.compression_ratio` histograms + `_HIST_SLOT` rows are ARTIFACTS-tier-specific instruments baked into the host-free metric spine, keyed off `ArtifactReceipt._facts` (an artifacts concept).
Charter: read the extracted DrainReceipt taxonomy from base; generalize artifact instruments to a parameterized per-domain histogram OR have artifacts contribute them.

### receipts.md — observability/receipts.py — VERDICT 8
`Receipt` union + `project`/`of` + `ReceiptContributor` port + `@receipted` aspect + `Redaction` chain-resident processor + `Signals` (structlog chain, W3C in/out). Excellent chain-placement reasoning (redact AFTER merge_contextvars).
Defects: **in the SCC** (receipts↔lanes via DrainReceipt/DRAIN_COLUMNS). Otherwise clean.
Charter: read DrainReceipt taxonomy from base (removes the receipts→lanes edge).

### telemetry.md — observability/telemetry.py — VERDICT 8
`Telemetry` composition-root install, `SIGNAL_SPECS` fold, `_batched` kernel, `SIGNAL_PROFILE`, `RUNTIME_RESOURCE`, `PARENT_SAMPLER`, `InstalledProviders`, railed drain. Strong composition-root.
Defects:
- **`latched` misplaced** (telemetry.md:228): a generic one-shot guard living in telemetry but consumed by metrics (metrics.md:41) — the edge that closes the admission↔telemetry loop.
- **In the SCC** (telemetry→admission→resilience→metrics→telemetry).
Charter: relocate `latched` to the base tier (faults/vocab); telemetry→admission (PROFILE_POLICY gate) is legit but is the return-path pivot — consider passing `emit_otel` as an install argument to sever telemetry→admission.

### wire.md — transport/wire.py — VERDICT 5 (phantom-vocabulary dependent)
`Decode` aspect + `WireProtoCodec`/`WIRE_REGISTRY` proto transcode + `CrdtOp`/`CrdtArm`/`CrdtOpDecode`. The `Decode._traced` direction-parameterized fold and the `array_like` CRDT arms with `_Stamped`/`_Identified` mixins are excellent.
Defects:
- **Depends on PHANTOM `rasm.runtime.shapes`** (wire.md:116-120) — the 16 canonical Structs it registers are defined by no owner (§A.2). The page's core capability (transcode canonical↔wire) is un-realizable without an owner for the canonical half.
- **Depends on PHANTOM `_pb2.channels_pb2`** (wire.md:114) with no codegen-seam owner.
- **Speculative dead branch**: `Framing.NESTED`/`_Nested`/`_unnest`/`_tagged`/`_stream` (wire.md:329-376) exist for a producer framing the `CRDT_OPLOG_WIRE_AMENDMENT` (which fixes keyed-FLAT emission) deprecates. ~30 LOC of decoder machinery for a wire shape the producer contract says won't be emitted; the page hedges ("never asserted ahead of it").
Charter: an owner page (or a wire.md region) MUST declare the 16 canonical wire Structs + `FaultDetail`'s fields; document the codegen seam; demote NESTED to a proven-need addition, not standing machinery.

### serve.md — transport/serve.py — VERDICT 6
`ServerHost` dispatch + `Credential` (CredentialPolicy axis) + `_FAULT_STATUS` + `CapabilityInvoke` + `Entrypoint`. The one-`dispatch` aspect and `settle` fold are strong.
Defects:
- **Carries the UNDECLARED credential seam** (serve.md:70-89) — the C# `CredentialPolicy`/PEM axis decode that the ledger misfiles onto admission (§B).
- **`Credential` name collides** with admission.md:228 (§B, admission).
- **Three concerns in one page**: inbound SERVE + outbound CAPABILITY_INVOKE + `Entrypoint` CLI (serve.md:318-343). Defensible (shared wire/descriptor) but heavy.
- Restates wire's cross-`libs/` seams in the ledger (§B).
Charter: add the credential-axis seam to the ledger under serve; rename one `Credential`; consider whether outbound `CapabilityInvoke` is a distinct owner.

### roots.md — transport/roots.py — VERDICT 8
`Transfer` acquisition aspect + `TransferPlan` + `ResourceRoot`/`ResourceRef`/`TransportResource` + `drain`. `functools.cache` store/client memo, endpoint-key credential-leak guard (roots.md:236-241), one-aspect collapse. Strong.
Defects: none structural. Imports only faults + resilience (clean out-of-SCC leaf). Consumes admission's `SecretBoundary` by caller-threaded `SecretStr` (prose only, no fence import — correct).
Charter: none — exemplary.

---

## [D] CROSS-CUTTING

### Duplication / parallel rails
- **Two `Credential` types** (admission.md:228 struct; serve.md:70 union) — same name, different concepts (§B/§C).
- **Seam double-attribution**: CausalFrame (admission+clock), CRDT/proto (serve+wire) — §B.
- **`latched` cross-import** (telemetry→metrics) — a generic aspect owned by a specific composition owner (§A.1).

### Concern mixing / placement
- **evidence.md** (tree-sitter + reflection) is a tooling surface with zero runtime consumers, mislodged in the execution foundation (§C).
- **metrics `record_artifact`** + artifact histograms — artifacts domain in the base spine (§C).
- **resilience POLICY** — geometry (occt/rpc) + data (lake-commit/remote-db/streaming) domains in the base retry table (§C).
- **serve.md** — inbound server + outbound client + CLI in one page (§C).

### Hardcoding vs generator
- `SECRETS_MOUNT="/run/secrets"` hardcoded infra path (admission.md:223).
- `WORKER_BAND` hardcoded `process_cpu_count() or 4` with sibling-folder-coupled comment, unused (lanes.md:89).
- Rosters that ARE correct DATA (not hardcoding defects): `_PROTO_VOCABULARY` 16 rows (wire.md:151), `POLICY` rows (resilience.md:244), `SIGNAL_PROFILE`/`INSTRUMENTS`/`SLOTS`/`PROFILE_POLICY` — all table-driven generators. Good.

### Dead / speculative carriers
- `WORKER_BAND` (lanes.md:89) — defined, referenced nowhere in-corpus.
- `Framing.NESTED` + `_Nested`/`_unnest`/`_tagged`/`_stream` (wire.md:329-376) — machinery for a deprecated producer framing.
- resilience POLICY sibling rows are pre-built for consumers that live in other folders (not dead, but forward-declared coupling).

### Unwired declared seams
- ledger `admission ← CredentialPem` (ARCHITECTURE.md:41) has no admission wire (§B).
- ledger `admission ⇄ CausalFrame` (ARCHITECTURE.md:40) has no direct admission wire (§B).
- codemap nested layout (ARCHITECTURE.md:9-30) vs 100%-flat imports.
- codemap `identity.py` vs module `content_identity` vs missing `seed_reproduction.py`.

### Unmined / illusory capability (catalog anchors)
- **`shapes` owner absent** — the 16 canonical wire Structs + `FaultDetail` fields exist only as a phantom import + prose (wire.md:102,116). The `.api/protobuf.md`/`.api/msgspec.md` anchors are cited but the canonical-shape owner is never authored.
- `CrdtOpDecode` LZ4 leg correctly deferred (IDEAS `CRDT_OPLOG_LZ4` UPSTREAM-BLOCKED, wire.md:7 DecompressFn seam) — genuine block, not a defect.

---

## [E] VERDICT CANDIDATES (campaign-defining, evidence-first)

1. **The runtime module graph contains a 6-module import SCC that crashes at import time.** {lanes, metrics, receipts, resilience, telemetry, admission} form unresolvable `from … import <name>` cycles (lanes↔metrics lanes.md:51/metrics.md:40; lanes↔receipts lanes.md:52/receipts.md:53; admission→resilience→metrics→telemetry→admission). Fix: extract `DrainReceipt`/`DrainOutcome`/`DRAIN_COLUMNS` + `latched` to the base tier and relocate the `drained`/`feed`/`RETRY_HOOKS` observers to a composition owner. THIS IS THE PRIMARY RULING.

2. **The canonical wire vocabulary has no owner (`rasm.runtime.shapes` is a phantom).** wire.md:116-120 imports 16 Structs defined nowhere; `FaultDetail`'s fields live only in prose (wire.md:102). The wire codec's entire interior half is illusory. Author a `shapes` (or wire-resident) owner declaring all 16 + `FaultDetail`, and fix the codegen-seam (`_pb2.channels_pb2`) ownership.

3. **The `CredentialPem` cross-`libs/` seam is misfiled to the wrong owner.** ARCHITECTURE.md:41 declares it on `execution/admission`, which has no such wire; the real decoder is serve.md:70-89 (`Credential` CredentialPolicy axis). Re-file the seam to `transport/serve`; remove the double-attributed `admission ⇄ CausalFrame` row (ARCHITECTURE.md:40) that duplicates clock←AppHost (:37).

4. **The host-free base foundation couples to sibling-tier domains and undeclared deps.** resilience.md:31-33 imports `adbc_driver_manager`/`daft` (data-tier, absent from ../README manifest); POLICY (resilience.md:257-289) carries geometry/data retry rows; lanes WORKER_BAND (lanes.md:89) names 8 sibling folder internals in a comment; metrics `record_artifact` (metrics.md:251) bakes in the artifacts domain. Ruling: base owners hold mechanism + native classes only; sibling-specific policy uses import-free `_named` or is sibling-contributed; declare or delete WORKER_BAND.

5. **`Credential` is spelled as two different types in one package.** admission.md:228 (`(username, SecretStr)` struct) vs serve.md:70 (CredentialPolicy `@tagged_union`) — violates one-canonical-name-per-concept. Rename the admission local to `UserSecret`/`BasicCredential`.

6. **The codemap describes a nested layout the fences do not use, and the identity node name is drifted three ways.** ARCHITECTURE.md:9-30 draws `observability/receipts.py` nesting; 100% of imports are flat `rasm.runtime.<leaf>`. The `identity.py` node is the `content_identity` module with a missing `seed_reproduction` split (identity.md:158,226). Decide flat-vs-nested and unify codemap + seam endpoints + module names.

7. **evidence.md is a dev-tooling surface mislodged in the execution foundation.** tree-sitter scanning + importlib reflection (evidence.md, 295 LOC) has zero runtime consumers (no `from rasm.runtime.evidence` anywhere); its consumer is the external `assay code` rail. Relocate to a tooling/assay owner or justify its presence in the host-free execution runtime.

8. **`latched` (generic one-shot install guard) lives in telemetry but is a shared cross-cutting aspect.** telemetry.md:228 owner, metrics.md:41 consumer — this single edge helps close the admission↔telemetry loop (candidate 1). It belongs at the base tier beside the other cross-cutting rails.

9. **Speculative NESTED CRDT framing is standing machinery for a deprecated wire shape.** wire.md:329-376 (`Framing.NESTED`/`_Nested`/`_unnest`/`_tagged`/`_stream`) decodes a producer framing the `CRDT_OPLOG_WIRE_AMENDMENT` keyed-flat contract deprecates. Demote to a proven-need addition; keep the FLAT path as the sole realized decoder.
