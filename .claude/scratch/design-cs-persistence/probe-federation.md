# [PROBE_FEDERATION] — V1 hinge feasibility

VERDICT: **FEASIBLE_WITH_GAPS** — reintroduce `Query/federation.md` as ruled, but the ruled-default WIRE-FORM premise is overturned by disk evidence in the federation owner's favor: the boundary transcriber the default hand-builds is already SHIPPED and PUBLIC in `FlowtideDotNet.Substrait`, and no `Grpc.Tools` codegen is needed (the standard Substrait protobuf types are generated public inside the assembly). Transcription weight collapses to ~2 lines; the only new code is the lowering visitor. The retire criterion is NOT met: no second query engine is minted. Gaps are confined to the TABULAR-subtree execution engine's Substrait support (G3), all closeable on existing engines/rows.

Evidence base: `assay api query` ilspy decompile over the restored `FlowtideDotNet.Substrait@0.15.0` (`lib/net10.0`, owner `Rasm.Persistence.csproj`, restored) and `Apache.Arrow.Adbc@0.23.0`; manifest `Directory.Packages.props`; `lane.md`/`columnar.md` on disk; upstream DuckDB/ADBC docs (two-source law) for the one member reflection cannot answer (external-engine Substrait support).

---

## [01]-[PER_MEMBER_VERIFICATION]

Every member `[V1]` names, verified against the restored assembly (decompile-truth; `api-flowtide-substrait.md` line refs are the catalog claim being checked).

| Member | Exists | Signature (decompiled) | Visibility | Catalog claim |
|---|---|---|---|---|
| `Sql.SqlPlanBuilder.Sql(text)` | YES | `public void Sql(string sqlText)` — parses via `_parser.ParseSql(.., FlowtideDialect, RecursionLimit=100000)`, lowers via `SqlSubstraitVisitor.GetRelations`, `_planModifier.AddRootPlan` | **public** | `:87` CONFIRMED |
| `Sql.SqlPlanBuilder.GetPlan()` | YES | `public Plan GetPlan()` (runs `PlanModifier.Modify()`) | **public** | `:87,:112` CONFIRMED |
| `Relations.RelationVisitor<TReturn,TState>` | YES | abstract double-dispatch base; `Relation.Accept<TReturn,TState>(RelationVisitor<TReturn,TState>, TState)` | **public** | `:45` CONFIRMED |
| `Expressions.ExpressionVisitor<TOutput,TState>` | YES | abstract; the `Expression.Accept` fold | **public** | `:68` CONFIRMED |
| `Plan` (managed IR) | YES | `sealed class Plan { List<Relation> Relations }`, value-equatable | **public** | `:31` CONFIRMED |
| `Relations.Relation` (base) | YES | `abstract class Relation { List<int>? Emit; bool EmitSet; abstract int OutputLength; Hint Hint; abstract TReturn Accept<..>(..) }` | **public** | `:44` CONFIRMED |
| `Conversion.SubstraitToDifferentialCompute[.Visitor]` | YES | the reference `RelationVisitor` lowering (managed Plan → differential-compute); **139 LOC, 9 `Visit*Relation` overrides** | public bridge / class | `:33` CONFIRMED — and it is the LOWERING-WEIGHT REFERENCE |
| Public protobuf ingress (`Deserialize`) | **YES** | see [02] | **public** | `:151` **FALSIFIED** |

### The falsified load-bearing claim (`api-flowtide-substrait.md:151`)

The catalog asserts: *"the Substrait protobuf round-trip (`Google.Protobuf` transitive) is internal — the consumer composes the managed `Plan` model and the visitor fold, not a public `Serialize()`/`Deserialize()` call."* This is FALSE on disk. The assembly ships:

- namespace `Substrait.Protobuf` — the **standard Substrait protobuf types, generated PUBLIC** inside `FlowtideDotNet.Substrait.dll`: `public sealed class Plan : IMessage<Plan>` with `public static MessageParser<Plan> Parser`, `RepeatedField<PlanRel> Relations`, `Extensions`, `ExtensionUris`. A consumer parses wire bytes directly: `Substrait.Protobuf.Plan.Parser.ParseFrom(ReadOnlySpan<byte>)`.
- `public class SubstraitDeserializer` with three public entries (decompiled bodies confirmed):
  - `public Plan Deserialize(global::Substrait.Protobuf.Plan plan)` → `new SubstraitDeserializerImpl(plan).Convert()` → `VisitPlan` folds every `PlanRel` → managed `FlowtideDotNet.Substrait.Plan`.
  - `public Plan Deserialize(string json)` — parses Substrait protobuf-JSON (`JsonParser` + a `TypeRegistry` carrying the Flowtide `CustomProtobuf` extension descriptors) → managed Plan.
  - `public static Plan DeserializeFromJson(string json)`.
- `namespace FlowtideDotNet.Substrait.CustomProtobuf` + `CustomProtoReflection` — Flowtide's own protobuf extension-relation messages (`IterationRelation`, `TopNRelation`, `TableFunctionRelation`, …), unpacked from `Any` extension leaves during deserialize.
- `internal class SubstraitSerializer` (the REVERSE, managed Plan → `Substrait.Protobuf.Plan`) — **internal**, not public. Asymmetry is favorable: the design never re-serializes; it retains the received wire bytes for the tabular lane (see [03]) and hashes them for the plan digest.

Consequence: `SubstraitDeserializer.Deserialize` IS the `RelationVisitor`/`ExpressionVisitor` transcription fold the ruled default proposes to hand-write. It is shipped and public.

---

## [02]-[CODEGEN_VIABILITY]

The ruled default rides *"the shared-tier `Google.Protobuf`+`Grpc.Tools` substrait codegen (zero new admission)."* Manifest confirms the pins: `Google.Protobuf@3.35.1` (`PROPS:47`, "Wire Transport" group), `Grpc.Tools@2.81.1` (`PROPS:55`). Findings:

- **`Grpc.Tools` codegen is NOT needed and is actively COUNTERPRODUCTIVE for federation.** The generated `Substrait.Protobuf.*` message types already ship public in the assembly. Running independent `protoc`/`Grpc.Tools` over the upstream `substrait/*.proto` would mint a SECOND, distinct `Substrait.Protobuf.Plan` CLR type; `SubstraitDeserializer.Deserialize` binds the assembly's own type by identity and would reject the duplicate. The federation ingress must consume Flowtide's shipped types, not regenerate them. (`Grpc.Tools`@2.81.1 + `Google.Protobuf`@3.35.1 CAN compile substrait `.proto` — protoc-driven, standard — but that capability is irrelevant here; the package ships **zero** `.proto` files, only compiled types.)
- **`Google.Protobuf@3.35.1` is the only codegen-adjacent dependency actually used** — as the RUNTIME for `MessageParser<Plan>.ParseFrom`, `IMessage`, `ByteString`, `CodedInputStream`. Already admitted (shared tier), already the resolved transitive of `FlowtideDotNet.Substrait` (nuspec floor `3.34.0`; substrate pins `3.35.1` higher — the resolved version, per catalog `:16`). Zero new admission, and the specific admission the ruled default names (`Grpc.Tools`) is unnecessary.

Net: codegen viability is MOOT — the wire types are pre-generated and public; the consumer references them and the `Google.Protobuf` runtime it already has.

---

## [03]-[LOWERING_FIT_MAP]

Lowering target A = `SetExpr` (`lane.md:182-190`), a **set-of-`NodeId`-keys algebra**: `Literal(Seq<NodeId>) | Predicate(SetPredicate) | ByRule(string) | Union | Intersect | Difference | Closure(Seed,Depth)`, folded by `ElementSetAlgebra.Evaluate(SetExpr, SetResolve)` (`lane.md:232`) into an `ElementSet(UInt128 Receipt, Seq<NodeId> Keys, int Count, ReadOnlyMemory<byte> Preimage)` (`lane.md:193`). Target B = the columnar/ADBC lane (`columnar.md` — one in-process DuckDB engine; the ADBC Arrow bridge `AdbcStatement.SqlQuery → ExecuteQueryAsync → IArrowArrayStream`, `columnar.md:277-289`).

Authoritative managed op set (the deserializer's `VisitPlanRel`/`VisitRel` switch + the concrete `Relations.*`): `ReadRelation`, `FilterRelation`, `ProjectRelation`, `JoinRelation`, `MergeJoinRelation`, `AggregateRelation`, `SortRelation`, `TopNRelation`, `FetchRelation`, `SetRelation`, `WriteRelation`, `RootRelation`, `ReferenceRelation`, `ConsistentPartitionWindowRelation`, `ExchangeRelation`, `TableFunctionRelation`, `VirtualTableReadRelation`, `IterationRelation`, `IterationReferenceReadRelation`, `NormalizationRelation` (Flowtide custom).

| Substrait op | Disposition | Target |
|---|---|---|
| `ReadRelation` (ElementGraph table, `Filter` = `SetPredicate` shape) | **LOWERS** | `SetExpr.Predicate` leaf (GiST/GIN index scan via `SetResolve.Leaf`) |
| `FilterRelation` (predicate reducible to a `SetPredicate` case) | **LOWERS** | folds into the `Predicate` leaf |
| `SetRelation` (`SetOperation` union/intersect/except) | **LOWERS** | `Union` / `Intersect` / `Difference` — exact 3-for-3 |
| `VirtualTableReadRelation` (inline literal `NodeId` rows) | **LOWERS** | `SetExpr.Literal(Seq<NodeId>)` |
| `IterationRelation`/`IterationReferenceReadRelation` (bounded key-reachability) | **PARTIAL** | `SetExpr.Closure(Seed,Depth)` for bounded key-graph; general recursive → columnar/reject |
| `JoinRelation`/`MergeJoinRelation` (key-equality semijoin on `NodeId`) | **PARTIAL** | `SetExpr.Intersect` for the semijoin case; general join (columns/measures) → columnar |
| `NormalizationRelation` (dedup) | LOWERS/ROUTES | inherent in `ElementSet` distinct-sorted; or columnar `DISTINCT` |
| `ReferenceRelation` (view binding) | STRUCTURAL | transparent — resolves the referenced subtree's target |
| `RootRelation` (output names) | STRUCTURAL | plan-root wrapper — drops names for `ElementSet`, carries schema to Arrow |
| `ReadRelation` (external `NamedTable` — ADBC warehouse / signed artifact) | **ROUTES** | columnar/ADBC scan (tabular) |
| `ProjectRelation` (computed columns) | **ROUTES — no SetExpr target** | columnar `SELECT` (`ElementSet` is columnless) |
| `AggregateRelation` (group-by + measures) | **ROUTES — no SetExpr target** | columnar `GROUP BY` |
| `SortRelation`/`TopNRelation`/`FetchRelation` | **ROUTES — no SetExpr target** | columnar `ORDER BY`/`LIMIT` (`ElementSet` is unordered-distinct) |
| `ConsistentPartitionWindowRelation` (window) | **ROUTES — no SetExpr target** | columnar window function |
| `TableFunctionRelation` | ROUTES-conditional | columnar table function if supported, else REJECT |
| `ExchangeRelation` (shuffle) | **DROP** | physical distribution primitive — single-node irrelevant |
| `WriteRelation` (sink) | **REJECT** | federation is read-only (`SubstraitParseException`-class fault) |

Ops with **no `SetExpr` target** (the columnar-lane dependency, i.e. exactly why `[V1]` names "SetExpr AND the columnar/ADBC execution lane" as a two-target lowering): `ProjectRelation`, `AggregateRelation`, `SortRelation`, `TopNRelation`, `FetchRelation`, `ConsistentPartitionWindowRelation`, general `JoinRelation`/`MergeJoinRelation`, general `IterationRelation`, `TableFunctionRelation`. Each has a columnar home; **none is un-executable**. `SetExpr.ByRule` has no Substrait source op (Rasm-native leaf) — a one-directional asymmetry, not a gap.

**One-engine test (the retire criterion):** a TOTAL lowering onto `SetExpr` alone is correctly impossible — `SetExpr` is a key-set algebra with no columns/measures/order — but the two-target partition mints NO new engine: the key-selection subset rides the store's existing GiST/GIN indexes (`SetResolve`), and the tabular subset rides the EXISTING in-process DuckDB engine plus the external warehouse's own engine. The federation owner is a **router/lowerer** over two standing lanes, not a query engine. Retire criterion NOT triggered.

---

## [04]-[TRANSCRIPTION_WEIGHT_JUDGMENT]

The ruled default's retire trigger — *"a transcription heavier than the lowering itself"* — is definitively NOT met; the inequality runs the opposite way.

- **Transcription weight ≈ 2 lines.** `Substrait.Protobuf.Plan.Parser.ParseFrom(wireBytes)` then `new SubstraitDeserializer().Deserialize(protoPlan)`; or `SubstraitDeserializer.DeserializeFromJson(json)` for the JSON wire. Both public, both shipped. Runtime cost is one admitted dependency (`Google.Protobuf@3.35.1`). The full protobuf→managed fold (`SubstraitDeserializerImpl.VisitPlan` + `ExpressionDeserializerImpl` over every relation/expression/type kind) is Flowtide's code, not Persistence's.
- **Lowering weight ≈ one bounded `RelationVisitor` subclass, ~150-250 LOC.** The reference — Flowtide's own `SubstraitToDifferentialComputeVisitor` (managed Plan → its engine) — is **139 LOC for 9 relation kinds**. The Persistence lowering visitor is the direct analogue (managed Plan → `SetExpr` | columnar route), one `LoweringTarget` discriminant per the [03] table. This is the ONLY substantive new code the owner carries.

The wire form is settled: the plan arrives as **Substrait protobuf bytes** (primary) or **canonical protobuf-JSON** (bonus — Python `substrait` libs emit both; gives the `python:data` signature-lock latitude). The plan digest is the kernel `ContentHash.Of` over the received wire bytes (`[V1]`b), no Flowtide member involved.

---

## [05]-[GAPS_AND_CLOSES]

- **G1 — catalog `:151` correction (trivial, strengthens feasibility).** `api-flowtide-substrait.md:151` must flip: the protobuf round-trip is PUBLIC via `SubstraitDeserializer.Deserialize(Substrait.Protobuf.Plan)` / `Deserialize(string json)` / `DeserializeFromJson`; `Substrait.Protobuf.Plan` (+ `MessageParser Parser`) is public in-assembly; `SubstraitSerializer` is internal (managed→protobuf unavailable — retain wire bytes, never re-serialize). Re-anchor the catalog to the federation owner in the same motion.
- **G2 — the two-target lowering must be authored as a `LoweringTarget` discriminant with the [03] routing table.** Design obligation, fully specified by this probe: a mixed plan (key-selection subtree feeding a tabular parent, or vice versa) needs the boundary rule — the leaf key-selection subtrees resolve to `ElementSet` and materialize as a `VirtualTable`/`Literal` feeding the columnar parent, or the tabular result intersects an `ElementSet` at the root. No blocker; it is the owner's core fold.
- **G3 — the one real external uncertainty: tabular-subtree Substrait EXECUTION support.** `SetExpr` subset is zero-gap. The tabular subset needs a Substrait-capable engine:
  - IN-PROCESS DuckDB: the `substrait` **DuckDB community extension** (`INSTALL substrait FROM community; LOAD substrait;`) exposes `from_substrait(blob)` — executes binary Substrait against DuckDB, returns rows. MIT, actively maintained (v1.2.2, 2025-05), maintained by substrait-io. Close = ADD one `ColumnarExtension.Substrait = new("substrait", linked:false)` row (`columnar.md:48-60` growth law; not a NuGet — the DuckLake-precedent extension-row admission, `[V7]`/`[V13]`). RISKS to verify at build: (i) DuckDB-version coupling — the extension tracks DuckDB releases; confirm a build exists for the pinned `DuckDB.NET.Data.Full` v1.5.x runtime (`columnar.md:277`); (ii) partial Substrait coverage — the extension implements a growing SUBSET (type/relation gaps exist upstream, e.g. some scalar-type pushdowns); the lowering must fail-closed (typed fault) on an unsupported relation, never silently.
  - EXTERNAL ADBC warehouse: `AdbcStatement.SubstraitPlan` (`public virtual byte[]?`; base throws `NotImplemented`) is the native ingress, but the ADBC spec states *"drivers are not required to support both SQL and Substrait"* and the admitted **BigQuery C# driver is Beta over a SQL backend — Substrait ingress not guaranteed**. `FlowtideDotNet.Substrait` has NO public managed-Plan→SQL emitter (`SubstraitSerializer` is internal and emits protobuf only), so a SQL-only warehouse cannot be driven from a Substrait plan through the admitted stack. Close = a capability axis on `SourceKind` (Substrait-native vs SQL-only); scope `[V1]`'s COMMITTED tabular federation to Substrait-capable targets (in-process DuckDB via the extension; Flight-SQL/DataFusion-class ADBC endpoints that advertise Substrait), and stage SQL-only-warehouse federation as a growth row (the producer emits SQL text for that `SourceKind`, executed via the existing `AdbcStatement.SqlQuery` path). This does NOT mint a second engine.
- **G4 — retain wire bytes (trivial, once named).** The tabular lane consumes the ORIGINAL received bytes (ADBC `SubstraitPlan` / DuckDB `from_substrait(blob)`); the plan digest hashes the same bytes. `SubstraitDeserializer` is invoked only for the `SetExpr`-lowering subtrees. Never round-trip through the internal serializer.
- **G5 — external-source consistency honesty (documentation).** `[V1]`b pins ONE `TimeCut` (lane watermark head). For the local ElementGraph key-selection this is a true cut; for an external federated source the pinned cut is the LOCAL coordinate and the external read is snapshot-at-execution. State this on the receipt semantics: the `(plan digest, cut, watermark)` triple content-addresses the plan + local coordinate; external data currency is read-time, recorded as such.

---

## [06]-[VERDICT_AND_DISPOSITION]

**FEASIBLE_WITH_GAPS — REINTRODUCE `Query/federation.md`.** The SetExpr-lowerable subset (the primary `python:data` reuse-currency path — which ElementGraph elements a federated selection resolves to) is fully feasible with a ~150-250 LOC lowering visitor and a ~2-line shipped-public transcription. The tabular-federation subset is feasible on existing engines with G3's bounded closes (one `ColumnarExtension.Substrait` row + a `SourceKind` capability axis). No second engine; no `Grpc.Tools` codegen; no hand-written transcriber. The retire alternative does NOT trigger.

Roster consequence (ride `[V1]`, KEEP): `FlowtideDotNet.Substrait@0.15.0` (`PROPS:279`), `Apache.Arrow.Adbc@0.23.0` + `.Drivers.{Apache,BigQuery}` (`PROPS:251-253`), `Apache.Arrow.Flight@23.0.0` (`PROPS:255`). ADD the DuckDB `substrait` community-extension row (extension row, not NuGet). CORRECT the ruled default: drop the `Grpc.Tools` codegen assumption (unnecessary/counterproductive); `Google.Protobuf@3.35.1` is the only runtime dependency, already admitted. The three BLOCKED cards re-anchor to the owner with honest status: lowering target realized as fences (SetExpr subset zero-gap; tabular subset scoped to Substrait-capable sources), BLOCKED on the Python producer.
