# [PERSISTENCE_QUERY_FEDERATION]

Rasm.Persistence accepts a FOREIGN Substrait relational-algebra plan and executes it over the standing read lanes as a router/lowerer — never a second engine: ONE `Federation.Execute(FederationPlan, TimeCut)` entrypoint admits the plan once, lowers it through a `RelationVisitor<Fin<LoweringTarget>, Unit>` double-dispatch fold, and dispatches the KEY-SELECTION half onto the sibling `Query/lane#ELEMENT_SET_ALGEBRA` `SetExpr` (executed over the store's EXISTING GiST/GIN indexes through the `SetResolve` ports for the local-row sources alone — a `Source.Live` plan rides the wire door whole — `SetRelation`→`Union`/`Intersect`/`Difference` 3-for-3, `ReadRelation`+`FilterRelation`→`Predicate`, `VirtualTableReadRelation`→`Literal`, bounded `IterationRelation`→`Closure`, key-semijoin `JoinRelation`→`Intersect`) and the TABULAR half (`ProjectRelation`/`AggregateRelation`/`SortRelation`/`TopNRelation`/`FetchRelation`/`ConsistentPartitionWindowRelation`/general joins) onto the `Query/columnar` execution arm — the DuckDB `ColumnarExtension.Substrait` `from_substrait(blob)` row under the `ColumnarProfile.Federation` posture, or the ADBC `AdbcQuery.Plan`/`AdbcQuery.Sql` statement doors for external warehouses. THREE ingress doors feed one admission: the shipped-PUBLIC protobuf wire (`Substrait.Protobuf.Plan.Parser.ParseFrom` then `new SubstraitDeserializer().Deserialize` — ~2 lines, zero `Grpc.Tools` codegen, `Google.Protobuf` the sole runtime dep), the Substrait-JSON string (`JsonParser.Default.Parse<WirePlan>` — the message's own wire-JSON, normalized to its protobuf twin at admission), and self-hosted SQL text (`SqlPlanBuilder` over registered `AddTableDefinition`/`AddTableProvider` tables). `SubstraitSerializer` is `internal`, so the managed IR is NEVER re-lowered to bytes — the admission NORMALIZES each door to its retained wire (the `Json` door transcodes to its protobuf twin through the message's own `JsonParser`, protobuf-native) and the plan digest is `ContentHash.Of(wireBytes)` over it (the kernel seed-zero entry, never a local `XxHash128`). Every plan executes against ONE `TimeCut` — a producer that supplies no cut resolves deterministically to the `Query/lane#READ_ROUTING` `StalenessWatermark.HeadSequence` head — and returns a `FederatedResult` carrying the `ElementSet` receipt, the optional Arrow batch, and the `(plan-digest·cut·watermark)` replay triple that content-addresses the result into ONE `Query/cache#ARTIFACT_BLOB_INDEX` `ArtifactKind` reuse row. Receipt honesty is structural: the pinned cut is the LOCAL coordinate, so the replay row is deterministic ONLY for the local `ElementSet` subtree — an external `SourceKind` row (`AdbcWarehouse`/`SqlStaged`) carries READ-TIME currency and is never presented as consistent-as-of the local cut. The same `Plan` IR drives a continuously-maintained materialized view through `SubstraitToDifferentialCompute.Convert` — the `FederationMode.Materialized` row, one owner for both cadences. The cross-runtime portable-plan wire is GATED on the `python:data` producer and the `SourceKind.SignedArtifact` binding on the `python:artifacts` producer through `Version/provenance#ATTESTED_LEDGER` — named blockers, never silent stubs. `SetExpr`/`SetResolve`/`ElementSet`/`StalenessWatermark` arrive from `Query/lane`; `AdbcQuery`/`ColumnarProfile.Federation`/`ColumnarExtension.Substrait` from `Query/columnar`; `TimeCut` from `Version/timetravel` (the frozen AS-OF vocabulary); `ContentHash.Of`/`Expected`/`IValidityEvidence`/`ValidityClaim` from the kernel `Rasm.Domain`; `FaultBand` from `Element/graph#FAULT_TABLES`; `Plan`/`SubstraitDeserializer`/`SqlPlanBuilder`/`RelationVisitor`/`SubstraitToDifferentialCompute` from `FlowtideDotNet.Substrait`; `ClockPolicy`/`ReceiptSinkPort` arrive as injected port values on the Persistence-owned `ProjectionContext` frame.

## [01]-[INDEX]

- [01]-[PLAN_INGRESS]: the three-door `PlanWire` admission, the `SourceKind` capability axis, the `FederationMode` cadence row, the retained-wire-bytes round-trip law, the `ContentHash.Of(wireBytes)` plan digest, and the `FederationFault` closed band.
- [02]-[PLAN_LOWERING]: the `RelationVisitor` double-dispatch lowering onto `LoweringTarget`, the `SetExpr` key-selection arm and the columnar/ADBC tabular arm, the ONE `Federation.Execute` entry with its default-cut law, the `FederatedResult` receipt and its replay triple, and the `Materialize` streaming sibling.

## [02]-[PLAN_INGRESS]

- Owner: `SourceKind` the `[SmartEnum<string>]` capability axis naming what the plan executes AGAINST — each row carrying `AcceptsPlan` (a Substrait plan is executable natively) and `Live` (the source answers at READ time, never as-of the local cut); `FederationMode` the `[SmartEnum<string>]` cadence row (`OneShot` runs once, `Materialized` lowers into the FlowtideDotNet streaming differential-compute engine); `PlanWire` the closed three-door ingress `[Union]` (`Protobuf`/`Json`/`Sql`); `FederationFault` the closed federation band deriving `FaultBand.Federation + n` off the `Element/graph#FAULT_TABLES` registry; `FederationPlan` the admitted plan record — the managed `Plan` IR, the NORMALIZED wire door (the sibling `Query/columnar` `AdbcQuery`: protobuf bytes or staged SQL text — the `Json` door transcodes to its protobuf twin through the message's own `JsonParser`, never the internal `SubstraitSerializer`), the `ContentHash.Of` digest, the source row, the mode row — with `Admit` the ONE admission factory.
- Cases: `SourceKind` is `DurableStore` (the local Marten/PG store — the `SetExpr` half rides the GiST/GIN indexes, the tabular half the DuckDB `from_substrait` lane; deterministic under the cut), `SignedArtifact` (a provenance-attested artifact set — the binding resolves through `Version/provenance#ATTESTED_LEDGER` and ALSO covers cloud-run artifacts landed through the `Store/blobstore` presigned-grant + `Query/cache#ArtifactKind.CloudRun` rows; GATED on the `python:artifacts` producer), `AdbcWarehouse` (a live external warehouse whose driver accepts `AdbcStatement.SubstraitPlan` — READ-TIME currency), `SqlStaged` (a SQL-only warehouse that CANNOT accept a Substrait plan — the capability axis's negative: a plan-door admission against it rails `SourceUncapable`, and the legal ingress is the `PlanWire.Sql` door whose text stages through `AdbcStatement.SqlQuery`); `FederationMode` is `OneShot`/`Materialized`; `PlanWire` is `Protobuf(ReadOnlyMemory<byte>)` (the wire message), `Json(string)` (the Substrait-JSON second wire form), `Sql(string, Seq<(string Table, NamedStruct Schema)>)` (the self-hosted front-end over registered tables); `FederationFault` is `SubstraitParse` (8421) · `UnsupportedRelation` (8422, the fail-loud partial-visitor funnel) · `SourceUnreachable` (8423, the live ADBC/Flight endpoint down — an availability fault distinct from capability) · `WriteRejected` (8424, the fail-closed `WriteRelation` refusal) · `SourceUncapable` (8425).
- Entry: `public static Fin<FederationPlan> Admit(PlanWire wire, SourceKind source, FederationMode mode)` admits the foreign plan ONCE — the `Protobuf` door parses `Substrait.Protobuf.Plan.Parser.ParseFrom(bytes.Span)` and lifts through `new SubstraitDeserializer().Deserialize(parsed)`; the `Json` door parses the Substrait-JSON through `JsonParser.Default.Parse<WirePlan>` (Substrait-JSON IS the message's own wire-JSON), retains `ToByteArray()` — the canonical protobuf twin, so a JSON plan and its byte-identical protobuf sibling share ONE digest — and lifts through the same `Deserialize(parsed)`; the `Sql` door registers each `(Table, Schema)` through `SqlPlanBuilder.AddTableDefinition`, lowers the text through `Sql(text)`, and composes `GetPlan()` — every door normalizing to its retained wire and stamping `Digest = ContentHash.Of(wireBytes)`; a `SubstraitParseException` or a protobuf decode fault rails `FederationFault.SubstraitParse`, and a plan door against a `SqlStaged` source rails `SourceUncapable` BEFORE any parse.
- Auto: the retained bytes ARE the outbound wire — `SubstraitSerializer` is `internal`, so a managed `Plan` cannot re-lower to protobuf and the round-trip law is retention, never re-serialization (`api-flowtide-substrait#SUBSTRAIT_TOPOLOGY`); the digest composes the kernel seed-zero `ContentHash.Of` so the plan identity, the blob residence, and the reuse index share ONE identity scheme (a local `XxHash128` mint beside it is the deleted second hasher); function references inside a `Sql`-door plan resolve through the `FunctionExtensions.Functions*` URI catalogs (`FunctionsComparison.Equal`, `FunctionsArithmetic.Sum`, …) so no magic string names a Substrait function; custom federation tables and operators register through `ITableProvider`/`ISqlFunctionRegister` — the schema catalog is the table provider, never an ad-hoc string.
- Receipt: an admission rides `store.federation.admit` carrying the door, the source row, and the digest; a refused admission rides the typed `FederationFault` on the rail, never a receipt.
- Packages: FlowtideDotNet.Substrait (`Plan`/`SubstraitDeserializer`/`Substrait.Protobuf.Plan.Parser`/`SqlPlanBuilder`/`ITableProvider`/`ISqlFunctionRegister`/`FunctionExtensions`/`Exceptions.SubstraitParseException`), Google.Protobuf (`MessageParser<T>`/`IMessage`/`JsonParser` — the sole runtime wire dep; zero `Grpc.Tools` codegen; `JsonParser.Default.Parse<T>` the Json-door protobuf-native transcode), Rasm (`Rasm.Domain` `ContentHash`/`Expected`), Rasm.Persistence (`Element/graph#FAULT_TABLES` `FaultBand`, `Query/columnar` `AdbcQuery` — the normalized wire door), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new ingress form is one `PlanWire` case; a new source class is one `SourceKind` row carrying its `AcceptsPlan`/`Live` columns; a new cadence is one `FederationMode` row; a new refusal cause is one `FederationFault` case; zero new surface — a `Grpc.Tools` `.proto` regeneration (mints a duplicate CLR `Plan` the shipped deserializer rejects by identity), a managed-IR re-serialization, a local plan hasher, a per-door entrypoint family, or a magic-string function name is the deleted form because the deserializer is public, the wire is the retained bytes, the digest is the kernel entry, and the doors are cases on one admission.
- Boundary: the plan is a vendor-neutral IR, never a store connection — admission yields a value and opens nothing; `SourceKind` is CAPABILITY data, so `SourceUncapable` is a structural refusal (a SQL-only warehouse never sees a plan blob) and `SourceUnreachable` is the availability negative of the LIVE rows only; the cross-runtime producer seams stay GATED — the `python:data` portable-plan half (ARCH:57b SPLIT, signature-locked) and the `python:artifacts` `SignedArtifact` binding are named blockers this owner declares, never silently-working stubs — while the `ElementSet` receipt currency itself stays owned by `Query/lane` (ARCH:57a); the `SignedArtifact` row resolves its binding through the attested ledger so a federated read over an externally-computed (including cloud-run) result is tamper-evident locally before it executes.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using Apache.Arrow;
using FlowtideDotNet.Substrait;
using FlowtideDotNet.Substrait.Conversion;
using FlowtideDotNet.Substrait.Exceptions;
using FlowtideDotNet.Substrait.Expressions;
using FlowtideDotNet.Substrait.Expressions.Literals;
using FlowtideDotNet.Substrait.Relations;
using FlowtideDotNet.Substrait.Sql;
using FlowtideDotNet.Substrait.Type;
using Google.Protobuf;                             // JsonParser — the Json door's protobuf-native transcode to the wire twin
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using Rasm.Element;
using Rasm.Persistence.Element;                   // FaultBand — the Element/graph#FAULT_TABLES registry the band derives from
using Thinktecture;
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — the alias wins over LanguageExt.Common.Expected for the bare name
using WirePlan = Substrait.Protobuf.Plan;          // the generated protobuf message — distinct from the managed FlowtideDotNet.Substrait.Plan IR
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// The capability axis the plan executes AGAINST: `AcceptsPlan` gates the Substrait-native doors, `Live`
// is the receipt-honesty column — a Live row answers at READ time and is never consistent-as-of the local cut.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SourceKind {
    public static readonly SourceKind DurableStore = new("durable-store", acceptsPlan: true, live: false);
    public static readonly SourceKind SignedArtifact = new("signed-artifact", acceptsPlan: true, live: false);
    public static readonly SourceKind AdbcWarehouse = new("adbc-warehouse", acceptsPlan: true, live: true);
    public static readonly SourceKind SqlStaged = new("sql-staged", acceptsPlan: false, live: true);
    public bool AcceptsPlan { get; }
    public bool Live { get; }
    private SourceKind(string key, bool acceptsPlan, bool live) : this(key) => (AcceptsPlan, Live) = (acceptsPlan, live);
}

// One plan IR, two cadences: OneShot runs once against the cut; Materialized lowers the SAME plan into the
// FlowtideDotNet streaming differential-compute engine as a continuously-maintained view (Federation.Materialize).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FederationMode {
    public static readonly FederationMode OneShot = new("one-shot");
    public static readonly FederationMode Materialized = new("materialized");
}

// --- [ERRORS] -----------------------------------------------------------------------------
// The closed federation band: a [Union] over the KERNEL `Rasm.Domain.Expected` (the same federation base every
// Persistence band realizes); band membership derives `Code => FaultBand.Federation + n` through the
// Element/graph#FAULT_TABLES registry row — never a bare integer — so a recovery routes
// `error.IsType<FederationFault.SourceUncapable>()` / `error.HasCode(8425)` / `error.Category()`, and the bare
// case lifts onto `Fin<T>` with no `.ToError()` hop. No `[GenerateUnionOps]` — the kernel generator is opt-in.
[Union]
public abstract partial record FederationFault : Expected, IValidationError<FederationFault> {
    private FederationFault() : base() { }
    public sealed record SubstraitParse(string Detail) : FederationFault;
    public sealed record UnsupportedRelation(string Relation) : FederationFault;
    public sealed record SourceUnreachable(string Endpoint) : FederationFault;
    public sealed record WriteRejected(string Table) : FederationFault;
    public sealed record SourceUncapable(string Source) : FederationFault;

    public override int Code => FaultBand.Federation + Switch(
        substraitParse:      static _ => 1,
        unsupportedRelation: static _ => 2,
        sourceUnreachable:   static _ => 3,
        writeRejected:       static _ => 4,
        sourceUncapable:     static _ => 5);

    public override string Message => Switch(
        substraitParse:      static c => $"<substrait-parse:{c.Detail}>",
        unsupportedRelation: static c => $"<federation-unsupported-relation:{c.Relation}>",
        sourceUnreachable:   static c => $"<federation-source-unreachable:{c.Endpoint}>",
        writeRejected:       static c => $"<federation-write-rejected:{c.Table}>",
        sourceUncapable:     static c => $"<federation-source-uncapable:{c.Source}>");

    public override string Category => Switch(
        substraitParse:      static _ => "Parse",
        unsupportedRelation: static _ => "Lowering",
        sourceUnreachable:   static _ => "Availability",
        writeRejected:       static _ => "Write",
        sourceUncapable:     static _ => "Capability");

    public static FederationFault Create(string message) => new SubstraitParse(message);
}

// --- [MODELS] -----------------------------------------------------------------------------
// The three ingress doors — one closed family, one admission. The Sql door carries its registered table
// schemas so the SqlPlanBuilder catalog is the table provider, never an ad-hoc string.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlanWire {
    private PlanWire() { }
    public sealed record Protobuf(ReadOnlyMemory<byte> Bytes) : PlanWire;
    public sealed record Json(string Body) : PlanWire;
    public sealed record Sql(string Text, Seq<(string Table, NamedStruct Schema)> Tables) : PlanWire;
}

// The admitted plan: the managed IR the visitor folds and the NORMALIZED wire door — the Json ingress
// transcodes to its protobuf twin through the message's own JsonParser (protobuf-native; the internal
// SubstraitSerializer never runs), so the retained wire is ALWAYS protobuf bytes (`AdbcQuery.Plan`) or
// staged SQL text (`AdbcQuery.Sql`) and a wrong statement form is unrepresentable at execution.
public sealed record FederationPlan(Plan Ir, AdbcQuery Wire, UInt128 Digest, SourceKind Source, FederationMode Mode) {
    public static Fin<FederationPlan> Admit(PlanWire wire, SourceKind source, FederationMode mode) =>
        !source.AcceptsPlan && wire is not PlanWire.Sql
            ? Fin.Fail<FederationPlan>(new FederationFault.SourceUncapable(source.Key))
            : Try.lift(() => wire.Switch<(Plan Ir, AdbcQuery Wire, byte[] Bytes)>(
                    protobuf: static p => {
                        byte[] wireBytes = p.Bytes.ToArray();
                        return (new SubstraitDeserializer().Deserialize(WirePlan.Parser.ParseFrom(wireBytes)), new AdbcQuery.Plan(wireBytes), wireBytes);
                    },
                    json: static j => {
                        WirePlan twin = JsonParser.Default.Parse<WirePlan>(j.Body);   // Substrait-JSON IS the message's wire-JSON; a JSON plan and its protobuf twin share ONE digest
                        byte[] wireBytes = twin.ToByteArray();
                        return (new SubstraitDeserializer().Deserialize(twin), new AdbcQuery.Plan(wireBytes), wireBytes);
                    },
                    sql: static s => {
                        var builder = new SqlPlanBuilder();
                        s.Tables.Iter(table => builder.AddTableDefinition(table.Table, table.Schema));
                        builder.Sql(s.Text);
                        return (builder.GetPlan(), new AdbcQuery.Sql(s.Text), Encoding.UTF8.GetBytes(s.Text));
                    }))
                .Run()
                .MapFail(static error => (Error)new FederationFault.SubstraitParse(error.Message))
                .Map(admitted => new FederationPlan(admitted.Ir, admitted.Wire, ContentHash.Of(admitted.Bytes), source, mode));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                                  | [BINDING]                                                                                              |
| :-----: | :------------------ | :------------------------------------------------------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | wire ingress        | `Plan.Parser.ParseFrom` + public `SubstraitDeserializer` | ~2 lines; zero `Grpc.Tools` codegen; `Google.Protobuf` the sole runtime dep                            |
|  [02]   | round trip          | NORMALIZED retained wire (`AdbcQuery` door)              | `SubstraitSerializer` is `internal`; `Json` transcodes protobuf-native; never a managed-IR re-lowering |
|  [03]   | plan digest         | `ContentHash.Of(wireBytes)`                              | the kernel seed-zero entry; never a local `XxHash128`                                                  |
|  [04]   | source capability   | `SourceKind.AcceptsPlan`/`Live` columns                  | plan-vs-SQL door and read-time-currency honesty are row DATA                                           |
|  [05]   | function references | `FunctionExtensions.Functions*` URI catalogs             | no magic-string Substrait function names                                                               |
|  [06]   | producers           | `python:data` + `python:artifacts` GATED                 | named blockers; the wire never pretends to work                                                        |

## [03]-[PLAN_LOWERING]

- Owner: `LoweringTarget` the two-arm `[Union]` the visitor folds every relation into (`Keyed(SetExpr)` the key-selection half, `Tabular(Relation)` the columnar half); `FederationLowering` the `RelationVisitor<Fin<LoweringTarget>, Unit>` double-dispatch fold covering the FULL relation roster (the base class throws `NotImplementedException` on an unhandled kind, so a partial visitor fails LOUD and the funnel converts it to `UnsupportedRelation` — never a silent drop); `FederationPorts` the injected execution ports (`SetResolve` from `Query/lane`, the columnar `AdbcQuery` arm from `Query/columnar`, the watermark measure, the clock); `FederatedResult` the receipt implementing the kernel `IValidityEvidence` floor; `Federation` the static surface owning the ONE `Execute` entry and the `Materialize` streaming sibling.
- Cases: on the visitor — `SetRelation` lowers `SetOperation` union/intersect/except onto `SetExpr.Union`/`Intersect`/`Difference` 3-for-3 when every input lowered `Keyed`; `ReadRelation` (+ its `Filter`) lowers to `SetExpr.Predicate` where the filter expression maps onto a typed `SetPredicate` through the `ExpressionVisitor` pushdown (function refs matched against the `Functions*` URI catalogs), else the read stays `Tabular`; `VirtualTableReadRelation` lowers literal rows onto `SetExpr.Literal`; a bounded `IterationRelation` lowers onto `SetExpr.Closure`; a key-equijoin whose both sides lowered `Keyed` collapses to `SetExpr.Intersect`, every other `JoinRelation`/`MergeJoinRelation` stays `Tabular`; `ProjectRelation`/`AggregateRelation`/`SortRelation`/`TopNRelation`/`FetchRelation`/`ConsistentPartitionWindowRelation` route `Tabular`; `WriteRelation` rails `FederationFault.WriteRejected` FAIL-CLOSED; `ExchangeRelation` DROPS the shuffle and lowers its input; `RootRelation`/`ReferenceRelation` recurse.
- Entry: `public static IO<Fin<FederatedResult>> Execute(FederationPlan plan, TimeCut cut, FederationPorts ports)` is the ONE entrypoint — it measures the watermark, folds the plan root through `FederationLowering`, executes a `Keyed` target through `ElementSetAlgebra.Evaluate(expr, ports.Resolve)` (the store's GiST/GIN indexes via the lane's `SetResolve` — no second engine, and the local-row sources ALONE: a `Source.Live` plan rides the wire door even when its relations lower `Keyed`, because its rows live remotely) and a `Tabular` target through the columnar arm carrying `plan.Wire` — the NORMALIZED admission door, so a protobuf-retained plan ships `AdbcQuery.Plan` and staged SQL text ships `AdbcQuery.Sql`, the door and never the source row selecting the statement form — and stamps the `FederatedResult`; a caller with no producer-supplied cut derives the default through `public static TimeCut CutOf(StalenessWatermark watermark, Instant head)` — the deterministic `HeadSequence`-head resolution under the head instant's `Hlc` ceiling, never an ambient now; `public static Fin<Unit> Materialize(FederationPlan plan, string table, params ReadOnlySpan<string> primaryKeys)` lowers the SAME `Plan` through `SubstraitToDifferentialCompute.Convert(plan.Ir, addWriteRelation: true, table, keys)` into the streaming differential-compute engine — the `FederationMode.Materialized` cadence on one owner, no second plan model.
- Auto: the lowering is a VISITOR fold, never a switch over relation type names — `Relation.Accept` double-dispatches into the typed `Visit*` overrides so a new Substrait relation kind surfaces as the base-class throw the funnel converts to `UnsupportedRelation`; predicate and projection pushdown ride the `ExpressionVisitor<TOutput, TState>` expression fold and the `Relation.Emit` column projection, so a `ReadRelation.Filter` the store index can serve becomes a typed `SetPredicate` leaf and everything else stays in the tabular subtree the engine executes; the `(plan-digest·cut·watermark)` replay triple is length-framed and folded through `ContentHash.Of` into `FederatedResult.ReplayKey` — the reuse identity ONE `ArtifactKind` row registers (the `cache.md` one-row growth law), deterministic for the local `ElementSet` subtree alone; an unreachable live endpoint lifts at the columnar/ADBC boundary into `SourceUnreachable`, structurally distinct from the `SourceUncapable` capability refusal.
- Receipt: an execution rides `store.federation.execute` carrying the digest, the cut, the watermark gap, the source row, and the arm taken (`keyed`/`tabular`); a replay hit rides the `Query/cache` reuse index receipts, never a second fact stream here; a materialization rides `store.federation.materialize` carrying the view table.
- Packages: FlowtideDotNet.Substrait (`RelationVisitor<TReturn,TState>`/`ExpressionVisitor<TOutput,TState>`/`Relation` roster/`SetOperation`/`Conversion.SubstraitToDifferentialCompute`), Apache.Arrow (`RecordBatch` — the owned batch currency the `Tabular` port drains inside the columnar ADBC statement window; a live `QueryResult` never crosses the port), Rasm (`Rasm.Domain` `ContentHash`/`IValidityEvidence`/`ValidityClaim`), Rasm.Element (`NodeId`), Rasm.Persistence (`Query/lane#ELEMENT_SET_ALGEBRA` `SetExpr`/`SetResolve`/`ElementSet`, `Query/lane#READ_ROUTING` `StalenessWatermark`, `Query/columnar` `AdbcQuery`, `Version/timetravel#TIME_CUT` `TimeCut` — frozen vocabulary), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new relation kind is one `Visit*` override lowering to an existing arm; a new execution surface is one `LoweringTarget` case plus one `Execute` arm; a new pushdown predicate is one `SetPredicate` mapping row in the expression fold; zero new surface — a second engine beside the standing lanes, a thin single-door single-relation-arm lowering, a switch over relation type names beside the visitor, a `Seq<Error>`-flattened lowering failure, or a replay key minted off a second hasher is the deleted form because the owner is a router/lowerer, the visitor is total-by-throw, and the replay identity composes the kernel digest.
- Boundary: the `SetExpr` arm executes over the store's EXISTING GiST/GIN indexes through `lane#SetResolve` — the key-selection half rides NO second engine, executes for the local-row sources alone (a `Source.Live` plan ships whole through the wire door; a warehouse read answered from local rows is the misrouted form), and a total lowering onto `SetExpr` alone is correctly impossible (it is a set-of-`NodeId`-keys algebra with no columns/measures/order), which is exactly why the TWO-target partition exists; the tabular arm is the `Query/columnar` `ColumnarProfile.Federation` posture whose community `substrait` extension row is FAIL-CLOSED at `Open` (`duckdb_extensions()` probe — a missing extension is the columnar owner's `ExtensionGap`, an unsupported relation THIS owner's `UnsupportedRelation`), and the ADBC door is driver-dependent (`AdbcStatement.SubstraitPlan` is NOT guaranteed on the BigQuery Beta driver), which is why plan-capability is a `SourceKind` column, never an assumption; `WriteRelation` is refused fail-closed — federation READS, the store rail writes; the pinned cut governs only the local subtree, so `FederatedResult` carries the source row beside the cut and a consumer reads the honesty structurally (`Source.Live` ⇒ read-time currency); `FederatedResult` implements the kernel `IValidityEvidence` floor with ONE `ValidityClaim.All` fold — a hand-rolled `&&` predicate chain is the deleted form; the BIM federation audit trail (BIM:91) reads the `store.federation.*` receipt stream by reference, never a second log owner here.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The two-arm lowering target: Keyed rides lane#SetResolve over the store's own indexes; Tabular rides the
// columnar/ADBC engine arm. Rejections travel the Fin rail INSIDE the fold, so the visitor return is
// Fin<LoweringTarget> and a WriteRelation refusal short-circuits exactly like any rail failure.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LoweringTarget {
    private LoweringTarget() { }
    public sealed record Keyed(SetExpr Expr) : LoweringTarget;
    public sealed record Tabular(Relation Subtree) : LoweringTarget;
}

// --- [SERVICES] -----------------------------------------------------------------------------
// The injected execution ports: the lane's SetResolve (Leaf index-lowering + Expand one-hop), the columnar
// AdbcQuery arm (Plan door = from_substrait/SubstraitPlan, Sql door = staged text), the watermark measure,
// the clock. Ports are VALUES the composition root supplies — this owner opens no connection of its own.
// `Tabular` yields OWNED `RecordBatch`es: the supplier composes `ColumnarLane.ArrowStream` and drains the
// `QueryResult.Stream` INSIDE the statement window (the stream dies with its owning `AdbcStatement`), so a
// raw `QueryResult` never crosses this port — the disposed-statement stream is unrepresentable here.
public sealed record FederationPorts(
    SetResolve Resolve,
    Func<AdbcQuery, IO<Seq<RecordBatch>>> Tabular,
    IO<StalenessWatermark> Watermark,
    ClockPolicy Clocks);

// --- [OPERATIONS] ---------------------------------------------------------------------------
// The full-roster double-dispatch fold (api-flowtide-substrait#SUBSTRAIT_TOPOLOGY): every relation kind is one
// typed override; the base throws NotImplementedException on an unhandled kind, so a NEW Substrait relation
// fails LOUD and the Execute funnel converts the throw to FederationFault.UnsupportedRelation — never a
// silent drop, never a switch over relation type names beside the visitor.
public sealed class FederationLowering : RelationVisitor<Fin<LoweringTarget>, Unit> {
    public override Fin<LoweringTarget> VisitRootRelation(RootRelation root, Unit state) => Visit(root.Input, state);

    // Union/Intersect/Difference lower 3-for-3 (decompile-verified SetOperation members); a set whose input
    // did not lower Keyed stays one tabular subtree — never a half-lowered mixed tree.
    public override Fin<LoweringTarget> VisitSetRelation(SetRelation set, Unit state) =>
        toSeq(set.Inputs).Map(input => Visit(input, state)).TraverseM(identity).As().Map(lowered =>
            lowered.ForAll(static t => t is LoweringTarget.Keyed)
                ? (LoweringTarget)new LoweringTarget.Keyed(
                      lowered.Map(static t => ((LoweringTarget.Keyed)t).Expr).Reduce((left, right) => set.Operation switch {
                          SetOperation.UnionAll or SetOperation.UnionDistinct => (SetExpr)new SetExpr.Union(left, right),
                          SetOperation.IntersectionPrimary or SetOperation.IntersectionMultiset or SetOperation.IntersectionMultisetAll => new SetExpr.Intersect(left, right),
                          _ => new SetExpr.Difference(left, right),
                      }))
                : new LoweringTarget.Tabular(set));

    // ReadRelation: the filter pushes down through the expression fold onto a typed SetPredicate leaf where the
    // store index serves it; an inexpressible filter keeps the whole read in the tabular subtree.
    public override Fin<LoweringTarget> VisitReadRelation(ReadRelation read, Unit state) =>
        Fin.Succ(SetLowering.Predicate(read.Filter).Match(
            Some: static leaf => (LoweringTarget)new LoweringTarget.Keyed(new SetExpr.Predicate(leaf)),
            None: () => new LoweringTarget.Tabular(read)));

    public override Fin<LoweringTarget> VisitFilterRelation(FilterRelation filter, Unit state) =>
        Visit(filter.Input, state).Map(inner => (inner, SetLowering.Predicate(filter.Condition)) switch {
            (LoweringTarget.Keyed keyed, { IsSome: true } leaf) =>
                (LoweringTarget)new LoweringTarget.Keyed(new SetExpr.Intersect(keyed.Expr, new SetExpr.Predicate(leaf.ValueUnsafe()))),
            _ => new LoweringTarget.Tabular(filter),
        });

    public override Fin<LoweringTarget> VisitVirtualTableReadRelation(VirtualTableReadRelation literal, Unit state) =>
        Fin.Succ((LoweringTarget)new LoweringTarget.Keyed(new SetExpr.Literal(SetLowering.Keys(literal))));

    // A BOUNDED iteration (MaxIterations set) whose seed lowered Keyed becomes the lane's Closure fold; an
    // unbounded or seedless iteration stays tabular — the generated Closure is depth-honest, never unbounded.
    public override Fin<LoweringTarget> VisitIterationRelation(IterationRelation iteration, Unit state) =>
        (Optional(iteration.MaxIterations), Optional(iteration.Input)).Apply((depth, seed) =>
            Visit(seed, state).Map(lowered => lowered is LoweringTarget.Keyed keyed
                ? (LoweringTarget)new LoweringTarget.Keyed(new SetExpr.Closure(keyed.Expr, depth))
                : new LoweringTarget.Tabular(iteration)))
        .As()
        .IfNone(() => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(iteration)));

    // A key-equijoin over two Keyed sides is a semijoin on the shared key space — SetExpr.Intersect; every
    // other join shape (outer, theta, projection-bearing) belongs to the engine arm.
    public override Fin<LoweringTarget> VisitJoinRelation(JoinRelation join, Unit state) =>
        (Visit(join.Left, state), Visit(join.Right, state)).Apply((left, right) =>
            (left, right, SetLowering.KeySemijoin(join)) switch {
                (LoweringTarget.Keyed l, LoweringTarget.Keyed r, true) => (LoweringTarget)new LoweringTarget.Keyed(new SetExpr.Intersect(l.Expr, r.Expr)),
                _ => new LoweringTarget.Tabular(join),
            }).As();

    public override Fin<LoweringTarget> VisitProjectRelation(ProjectRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitAggregateRelation(AggregateRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitSortRelation(SortRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitTopNRelation(TopNRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitFetchRelation(FetchRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitConsistentPartitionWindowRelation(ConsistentPartitionWindowRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));

    public override Fin<LoweringTarget> VisitWriteRelation(WriteRelation write, Unit state) =>
        Fin.Fail<LoweringTarget>(new FederationFault.WriteRejected(write.NamedObject.ToString() ?? "<write>"));   // fail-closed: federation READS

    public override Fin<LoweringTarget> VisitExchangeRelation(ExchangeRelation exchange, Unit state) =>
        Visit(exchange.Input, state);                                                                             // DROP the shuffle, keep the subtree
}

// The predicate/literal pushdown half the relation fold composes: function references match the Functions*
// URI catalogs (FunctionsComparison.Uri/Equal — never magic strings), and only an index-servable shape yields
// a typed SetPredicate; everything else stays in the tabular subtree. The ExpressionVisitor default returns
// null for unhandled expression kinds, so absence — not a throw — is the inexpressible-predicate signal here.
public static class SetLowering {
    static readonly PredicateLowering Pushdown = new();

    public static Option<SetPredicate> Predicate(Expression? filter) =>
        Optional(filter).Bind(condition => Optional(condition.Accept(Pushdown, unit)));

    // A virtual table of key literals: each StructExpression row's first field is the NodeId key string —
    // the SetExpr.Literal ingress; a non-string virtual table stays tabular via the caller's None fold.
    public static Seq<NodeId> Keys(VirtualTableReadRelation literal) =>
        toSeq(literal.Values.Expressions)
            .Map(static row => row.Fields is [StringLiteral key, ..] ? Optional(NodeId.Create(key.Value)) : None)
            .Somes();

    // Key-semijoin shape: an INNER join whose condition is the catalog `equal` over two field references —
    // the one join the shared-key intersection expresses; everything else is the engine's.
    public static bool KeySemijoin(JoinRelation join) =>
        join.Type == JoinType.Inner
        && join.Expression is ScalarFunction { ExtensionUri: FunctionsComparison.Uri, ExtensionName: FunctionsComparison.Equal } condition
        && condition.Arguments is [DirectFieldReference, DirectFieldReference];
}

// The expression fold: equality over (field, string-literal) lowers to the jsonb-served Jsonpath leaf; the
// unhandled default (base virtual members return null) IS the "stays tabular" signal — no throw, no _-arm.
public sealed class PredicateLowering : ExpressionVisitor<SetPredicate?, Unit> {
    public override SetPredicate? VisitScalarFunction(ScalarFunction function, Unit state) =>
        (function.ExtensionUri, function.ExtensionName, function.Arguments) switch {
            // `JsonComparison.Eq`, never `Equals` — the lane vocabulary avoids a static `Equals` item (CS0102
            // against the generated `Equals(object?)`/`Equals(JsonComparison?)`); `.Equals` binds a method group.
            (FunctionsComparison.Uri, FunctionsComparison.Equal, [DirectFieldReference field, StringLiteral literal]) =>
                new SetPredicate.Jsonpath(SetPath(field), JsonComparison.Eq, Some(literal.Value)),
            // Substrait spells not-null as not(is_null(x)) — `IsNull` is the catalog constant (`IsNotNull` does
            // not exist); the boolean-not wrapper over an is_null leaf lowers to the Exists predicate.
            (FunctionsBoolean.Uri, FunctionsBoolean.Not, [ScalarFunction { ExtensionUri: FunctionsComparison.Uri, ExtensionName: FunctionsComparison.IsNull, Arguments: [DirectFieldReference field] }]) =>
                new SetPredicate.Exists(SetPath(field)),
            _ => null,
        };

    static string SetPath(DirectFieldReference field) => field.ReferenceSegment.ToString() ?? "";
}

public static class Federation {
    // The default-cut law (lane.md StalenessWatermark): a producer-supplied-no-cut plan resolves DETERMINISTICALLY
    // to the VERSION-pinned head — the watermark's HeadSequence under the head instant's Hlc ceiling — never an
    // ambient clock read at the call site.
    public static TimeCut CutOf(StalenessWatermark watermark, Instant head) => TimeCut.AtVersion(watermark.HeadSequence, new Hlc(head, ulong.MaxValue));

    public static IO<Fin<FederatedResult>> Execute(FederationPlan plan, TimeCut cut, FederationPorts ports) =>
        from watermark in ports.Watermark
        from lowered in IO.lift(() => Try.lift(() =>
                plan.Ir.Relations is [var root, ..]
                    ? new FederationLowering().Visit(root, unit)
                    : Fin.Fail<LoweringTarget>(new FederationFault.SubstraitParse("<empty-plan>")))
            .Run()
            .MapFail(static error => (Error)new FederationFault.UnsupportedRelation(error.Message))   // the base-class NotImplementedException funnel
            .Bind(static outcome => outcome))
        from result in lowered.Match(
            Succ: target => target.Switch(
                // A Live source's rows live REMOTELY: its plan rides the wire door even when its relations lower
                // Keyed — a warehouse read answered from the local indexes is the misrouted form. Local
                // key-selection is the DurableStore/SignedArtifact half alone.
                keyed: k => plan.Source.Live
                    ? Engine(plan, cut, watermark, ports)
                    : IO.pure(Fin.Succ(Stamp(plan, cut, watermark, ElementSetAlgebra.Evaluate(k.Expr, ports.Resolve), None, ports.Clocks.Now))),
                tabular: t => Engine(plan, cut, watermark, ports)),
            Fail: fault => IO.pure(Fin<FederatedResult>.Fail(fault)))
        select result;

    // The Materialized cadence: the SAME Plan IR lowers into the streaming differential-compute engine as a
    // continuously-maintained view — one plan model for both cadences, never a second IR.
    public static Fin<Unit> Materialize(FederationPlan plan, string table, params ReadOnlySpan<string> primaryKeys) =>
        Try.lift(() => { _ = SubstraitToDifferentialCompute.Convert(plan.Ir, addWriteRelation: true, table, [.. primaryKeys]); return unit; })
            .Run()
            .MapFail(static error => (Error)new FederationFault.SubstraitParse(error.Message));

    // The engine arm through the NORMALIZED admission door: protobuf plan or staged SQL — the door, never the
    // source row, selects the statement form; an unreachable live endpoint lifts into SourceUnreachable here.
    static IO<Fin<FederatedResult>> Engine(FederationPlan plan, TimeCut cut, StalenessWatermark watermark, FederationPorts ports) =>
        ports.Tabular(plan.Wire)
            .Map(batch => Fin.Succ(Stamp(plan, cut, watermark, ElementSet.Empty, Some(batch), ports.Clocks.Now)))
        | @catch<IO, Fin<FederatedResult>>(static _ => true, static e => IO.pure(Fin<FederatedResult>.Fail(new FederationFault.SourceUnreachable(e.Message))));

    static FederatedResult Stamp(FederationPlan plan, TimeCut cut, StalenessWatermark watermark, ElementSet keys, Option<Seq<RecordBatch>> batch, Instant at) =>
        new(plan.Digest, cut, watermark, plan.Source, keys, batch, plan.Mode, at);
}

// --- [MODELS] -------------------------------------------------------------------------------
// The federated receipt: local ElementSet subtree + optional OWNED Arrow batches (drained inside the ADBC
// statement window by the Tabular port — never a live QueryResult whose stream died with its statement),
// cut-pinned and watermark-stamped. IsValid is the kernel [C] receipt-validity floor — ONE ValidityClaim.All
// fold, never a hand-rolled && chain.
public sealed record FederatedResult(
    UInt128 PlanDigest,
    TimeCut Cut,
    StalenessWatermark Watermark,
    SourceKind Source,
    ElementSet Keys,
    Option<Seq<RecordBatch>> Batch,
    FederationMode Mode,
    Instant At) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(PlanDigest != default),
        ValidityClaim.Of(Watermark.ProjectedSequence <= Watermark.HeadSequence),
        ValidityClaim.Of(Keys.Count > 0 || Batch.IsSome || Source.Live));

    // The (plan-digest·cut·watermark) replay triple — length-framed, kernel-hashed; the reuse identity ONE
    // Query/cache ArtifactKind row registers. Deterministic for the LOCAL ElementSet subtree only: a Live
    // source row carries read-time currency, so its replay entry is a hint, never a consistency claim.
    public UInt128 ReplayKey {
        get {
            var preimage = new ArrayBufferWriter<byte>();
            BinaryPrimitives.WriteUInt128BigEndian(preimage.GetSpan(16), PlanDigest);
            preimage.Advance(16);
            int kind = Encoding.UTF8.GetByteCount(Cut.Source.Key);
            BinaryPrimitives.WriteInt32LittleEndian(preimage.GetSpan(4), kind);
            preimage.Advance(4);
            preimage.Advance(Encoding.UTF8.GetBytes(Cut.Source.Key, preimage.GetSpan(kind)));
            BinaryPrimitives.WriteInt64LittleEndian(preimage.GetSpan(8), Cut.At.ToUnixTimeTicks());
            preimage.Advance(8);
            BinaryPrimitives.WriteInt64LittleEndian(preimage.GetSpan(8), Watermark.HeadSequence);
            preimage.Advance(8);
            BinaryPrimitives.WriteInt64LittleEndian(preimage.GetSpan(8), Watermark.ProjectedSequence);
            preimage.Advance(8);
            return ContentHash.Of(preimage.WrittenSpan);
        }
    }
}
```

| [INDEX] | [POLICY]          | [VALUE]                                            | [BINDING]                                                                                |
| :-----: | :---------------- | :------------------------------------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | one entry         | `Federation.Execute(FederationPlan, TimeCut)`      | router/lowerer over standing lanes; never a second engine                                |
|  [02]   | lowering form     | `RelationVisitor<Fin<LoweringTarget>, Unit>`       | total-by-throw; a new relation kind fails LOUD as `UnsupportedRelation`                  |
|  [03]   | key-selection arm | `SetExpr` via `lane#SetResolve`                    | the store's GiST/GIN indexes execute, local (`Live` false) sources only; 3-for-3 set ops |
|  [04]   | tabular arm       | `ColumnarProfile.Federation` + `AdbcQuery` doors   | `from_substrait(blob)` in-process; `SubstraitPlan`/`SqlQuery` external                   |
|  [05]   | write posture     | `WriteRelation` → `WriteRejected`                  | fail-closed; federation reads, the store rail writes                                     |
|  [06]   | default cut       | `StalenessWatermark.HeadSequence` head             | a no-cut plan resolves deterministically; never ambient now                              |
|  [07]   | replay identity   | `(digest·cut·watermark)` → `ReplayKey`             | ONE `ArtifactKind` reuse row; local-subtree-deterministic only                           |
|  [08]   | receipt validity  | `IValidityEvidence` + `ValidityClaim.All`          | the kernel [C] floor; never a hand-rolled `&&` chain                                     |
|  [09]   | streaming cadence | `Materialize` via `SubstraitToDifferentialCompute` | one plan IR for one-shot and materialized alike                                          |
