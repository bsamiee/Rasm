# [PERSISTENCE_QUERY_FEDERATION]

Rasm.Persistence admits protobuf, Substrait JSON, or registered-table SQL into one `FederationPlan`, then routes it through one `Execute` rail. `FederationLowering` preserves verified keyed union/intersection, typed predicates, admitted literal keys, bounded closure, and key semijoins; unsupported set operations, exchanges, and engine-owned relations remain tabular. `SourceKind` carries each attestation or external binding and derives capability and live currency from its case. One-shot execution composes `Fin<ElementSet>` or `Fin<Seq<RecordBatch>>`; materialized execution converts the plan and passes the returned IR to the injected materialization port before success. `ReplayKey` frames plan, cut, watermark, source, and mode identity.

## [01]-[INDEX]

- [01]-[PLAN_INGRESS]: the three-door `PlanWire` admission, the `SourceKind` capability axis, the `FederationMode` cadence union, the retained-wire-bytes round-trip law, the `ContentHash.Of(wireBytes)` plan digest, and the `FederationFault` closed band.
- [02]-[PLAN_LOWERING]: the `RelationVisitor` double-dispatch lowering onto `LoweringTarget`, the `SetExpr` key-selection arm and the columnar/ADBC tabular arm, the ONE `Federation.Execute` entry owning the cut-shape default and the cadence dispatch, and the `FederatedResult` receipt with its replay triple.
- [03]-[FLIGHT_RESULT_PLANE]: the Arrow Flight return wire — the `ReplayKey`-ticketed `FederationFlight` producer whose `GetFlightInfo` admits-and-executes a command-descriptor plan and whose `DoGet` streams the held result's record batches zero-copy to a cross-runtime consumer.

## [02]-[PLAN_INGRESS]

- Owner: `SourceKind` is the closed source-binding family; each case carries the identity required to distinguish an attested artifact or external binding, while `AcceptsPlan` and `IsLive` derive from the case. `FederationMode` owns cadence and materialized-view identity. `PlanWire` owns the three ingress forms. `FederationPlan.Admit` normalizes each form and mints one digest.
- Cases: `SourceKind` is `DurableStore | SignedArtifact(UInt128 Attestation) | AdbcWarehouse(Identifier Binding) | SqlStaged(Identifier Binding)`; `FederationMode` is `OneShot | Materialized(Identifier View, Seq<Identifier> Keys)`; `PlanWire` is `Protobuf | Json | Sql(string Text, Seq<(Identifier Table, NamedStruct Schema)> Tables)`; `FederationFault` occupies `8421` through `8427`.
- Entry: `public static Fin<FederationPlan> Admit(PlanWire wire, SourceKind source, FederationMode mode)` admits the foreign plan ONCE — the `Protobuf` door parses `Substrait.Protobuf.Plan.Parser.ParseFrom(bytes.Span)` and lifts through `new SubstraitDeserializer().Deserialize(parsed)`; the `Json` door parses the Substrait-JSON through `JsonParser.Default.Parse<WirePlan>` (Substrait-JSON IS the message's own wire-JSON), retains `ToByteArray()` — the canonical protobuf twin, so a JSON plan and its byte-identical protobuf sibling share ONE digest — and lifts through the same `Deserialize(parsed)`; the `Sql` door registers each `(Table, Schema)` through `SqlPlanBuilder.AddTableDefinition`, lowers the text through `Sql(text)`, and composes `GetPlan()` — every door normalizing to its retained wire and stamping `Digest = ContentHash.Of(wireBytes)`; a `SubstraitParseException` or a protobuf decode fault rails `FederationFault.SubstraitParse`, and a plan door against a `SqlStaged` source rails `SourceUncapable` BEFORE any parse.
- Auto: the retained bytes ARE the outbound wire — `SubstraitSerializer` is `internal`, so a managed `Plan` cannot re-lower to protobuf and the round-trip law is retention, never re-serialization (`api-flowtide-substrait#SUBSTRAIT_TOPOLOGY`); the digest composes the kernel seed-zero `ContentHash.Of` so the plan identity, the blob residence, and the reuse index share ONE identity scheme (a local `XxHash128` mint beside it is the deleted second hasher); function references inside a `Sql`-door plan resolve through the `FunctionExtensions.Functions*` URI catalogs (`FunctionsComparison.Equal`, `FunctionsArithmetic.Sum`, …) so no magic string names a Substrait function; custom federation tables and operators register through `ITableProvider`/`ISqlFunctionRegister` — the schema catalog is the table provider, never an ad-hoc string.
- Receipt: an admission rides `store.federation.admit` carrying the door, the source row, and the digest; a refused admission rides the typed `FederationFault` on the rail, never a receipt.
- Packages: FlowtideDotNet.Substrait (`Plan`/`SubstraitDeserializer`/`Substrait.Protobuf.Plan.Parser`/`SqlPlanBuilder`/`ITableProvider`/`ISqlFunctionRegister`/`FunctionExtensions`/`Exceptions.SubstraitParseException`), Google.Protobuf (`MessageParser<T>`/`IMessage`/`JsonParser` — the sole runtime wire dep; zero `Grpc.Tools` codegen; `JsonParser.Default.Parse<T>` the JSON-door protobuf-native transcode), Rasm (`Rasm.Domain` `ContentHash`/`Expected`), Rasm.Persistence (`Element/graph#FAULT_TABLES` `FaultBand`, `Query/columnar` `AdbcQuery`/`AdbcRequest` — the normalized statement door), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new ingress, source, cadence, or refusal is one case on its existing closed family; every source case carries its execution binding and replay identity.
- Boundary: the plan is a vendor-neutral IR, never a store connection — admission yields a value and opens nothing; `SourceKind` is CAPABILITY data, so `SourceUncapable` is a structural refusal (a SQL-only warehouse never sees a plan blob) and `SourceUnreachable` is the availability negative of the LIVE rows only; the cross-runtime producer seams stay GATED — the `python:data` portable-plan half (the `ARCHITECTURE.md [02]-[SEAMS]` `Query`↔`Data` `[WIRE]: SubstraitPlan` edge, signature-locked) and the `python:artifacts` `SignedArtifact` binding are named blockers this owner declares, never silently-working stubs — while the `ElementSet` receipt currency itself stays owned by `Query/lane`; the `SignedArtifact` row resolves its binding through the attested ledger so a federated read over an externally-computed (including cloud-run) result is tamper-evident locally before it executes.

```csharp signature
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
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
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Persistence.Element;                   // FaultBand — the Element/graph#FAULT_TABLES registry the band derives from
using Thinktecture;
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — the alias wins over LanguageExt.Common.Expected for the bare name
using WirePlan = Substrait.Protobuf.Plan;          // the generated protobuf message — distinct from the managed FlowtideDotNet.Substrait.Plan IR
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// `FederationCapability` governs plan admission, live execution, writeability, and snapshot support.
// Live sources answer at read time and never claim consistency at the local cut.
[Union]
public abstract partial record SourceKind {
    private SourceKind() { }
    public sealed record DurableStore : SourceKind;
    public sealed record SignedArtifact(UInt128 Attestation) : SourceKind;
    public sealed record AdbcWarehouse(Identifier Binding) : SourceKind;
    public sealed record SqlStaged(Identifier Binding) : SourceKind;

    public bool AcceptsPlan => Switch(
        durableStore:  static _ => true,
        signedArtifact: static _ => true,
        adbcWarehouse: static _ => true,
        sqlStaged:     static _ => false);

    public bool IsLive => Switch(
        durableStore:  static _ => false,
        signedArtifact: static _ => false,
        adbcWarehouse: static _ => true,
        sqlStaged:     static _ => true);

    public string Identity => Switch(
        durableStore:  static _ => "durable-store",
        signedArtifact: static source => string.Create(CultureInfo.InvariantCulture, $"signed-artifact:{source.Attestation:x32}"),
        adbcWarehouse: static source => string.Create(CultureInfo.InvariantCulture, $"adbc-warehouse:{(string)source.Binding}"),
        sqlStaged:     static source => string.Create(CultureInfo.InvariantCulture, $"sql-staged:{(string)source.Binding}"));
}

// `FederationMode` carries one-shot or materialized cadence on the plan value.
// Materialized mode retains view identity and primary keys for differential compute.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FederationMode {
    private FederationMode() { }
    public sealed record OneShot : FederationMode;
    public sealed record Materialized(Identifier View, Seq<Identifier> Keys) : FederationMode;

    public string Identity => Switch(
        oneShot: static _ => "one-shot",
        materialized: static mode => string.Create(CultureInfo.InvariantCulture, $"materialized:{(string)mode.View}:{string.Join(',', mode.Keys.Map(static key => (string)key))}"));
}

// --- [ERRORS] -----------------------------------------------------------------------------
// `FederationFault` closes `FaultBand.Federation` over `Rasm.Domain.Expected`.
// Cases lift directly onto `Fin<T>` without generated union operations.
[Union]
public abstract partial record FederationFault : Expected, IValidationError<FederationFault> {
    private FederationFault() : base() { }
    public sealed record SubstraitParse(string Detail) : FederationFault;
    public sealed record UnsupportedRelation(string Relation) : FederationFault;
    public sealed record SourceUnreachable(string Endpoint) : FederationFault;
    public sealed record WriteRejected(string Table) : FederationFault;
    public sealed record SourceUncapable(string Source) : FederationFault;
    public sealed record MaterializationRejected(string Detail) : FederationFault;
    public sealed record TicketUnknown(UInt128 Ticket) : FederationFault;

    public override int Code => FaultBand.Federation + Switch(
        substraitParse:      static _ => 1,
        unsupportedRelation: static _ => 2,
        sourceUnreachable:   static _ => 3,
        writeRejected:       static _ => 4,
        sourceUncapable:     static _ => 5,
        materializationRejected: static _ => 6,
        ticketUnknown:       static _ => 7);

    public override string Message => Switch(
        substraitParse:      static c => $"<substrait-parse:{c.Detail}>",
        unsupportedRelation: static c => $"<federation-unsupported-relation:{c.Relation}>",
        sourceUnreachable:   static c => $"<federation-source-unreachable:{c.Endpoint}>",
        writeRejected:       static c => $"<federation-write-rejected:{c.Table}>",
        sourceUncapable:     static c => $"<federation-source-uncapable:{c.Source}>",
        materializationRejected: static c => $"<federation-materialization-rejected:{c.Detail}>",
        ticketUnknown:       static c => $"<federation-ticket-unknown:{c.Ticket:x32}>");

    public override string Category => Switch(
        substraitParse:      static _ => "Parse",
        unsupportedRelation: static _ => "Lowering",
        sourceUnreachable:   static _ => "Availability",
        writeRejected:       static _ => "Write",
        sourceUncapable:     static _ => "Capability",
        materializationRejected: static _ => "Admission",
        ticketUnknown:       static _ => "Ticket");

    public static FederationFault Create(string message) => new SubstraitParse(message);
}

// --- [MODELS] -----------------------------------------------------------------------------
// `FederationSource` closes registered SQL, normalized plan, and live-source ingress under one admission path.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlanWire {
    private PlanWire() { }
    public sealed record Protobuf(ReadOnlyMemory<byte> Bytes) : PlanWire;
    public sealed record Json(string Body) : PlanWire;
    public sealed record Sql(string Text, Seq<(Identifier Table, NamedStruct Schema)> Tables) : PlanWire;
}

// `FederationPlan` owns admitted IR and normalized protobuf or staged-SQL execution forms.
// JSON ingress transcodes through protobuf parsing without invoking internal serialization.
public sealed class FederationPlan {
    private FederationPlan(Plan ir, AdbcQuery wire, UInt128 digest, SourceKind source, FederationMode mode) =>
        (Ir, Wire, Digest, Source, Mode) = (ir, wire, digest, source, mode);

    public Plan Ir { get; }
    public AdbcQuery Wire { get; }
    public UInt128 Digest { get; }
    public SourceKind Source { get; }
    public FederationMode Mode { get; }

    public static Fin<FederationPlan> Admit(PlanWire wire, SourceKind source, FederationMode mode) =>
        mode is FederationMode.Materialized { Keys.IsEmpty: true }
            ? Fin.Fail<FederationPlan>(new FederationFault.MaterializationRejected("<primary-key>"))
            : !source.AcceptsPlan && wire is not PlanWire.Sql
            ? Fin.Fail<FederationPlan>(new FederationFault.SourceUncapable(source.Identity))
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
                        SqlPlanBuilder builder = new();
                        s.Tables.Iter(table => builder.AddTableDefinition((string)table.Table, table.Schema));
                        builder.Sql(s.Text);
                        ArrayBufferWriter<byte> identity = new();
                        Frame(identity, s.Text);
                        s.Tables.OrderBy(static table => (string)table.Table).Iter(table => {
                            Frame(identity, (string)table.Table);
                            Frame(identity, table.Schema.ToString() ?? "");
                        });
                        return (builder.GetPlan(), new AdbcQuery.Sql(AdbcSql.Create(s.Text)), identity.WrittenSpan.ToArray());
                    }))
                .Run()
                .MapFail(static error => (Error)new FederationFault.SubstraitParse(error.Message))
                .Map(admitted => new FederationPlan(admitted.Ir, admitted.Wire, ContentHash.Of(admitted.Bytes), source, mode));

    static void Frame(ArrayBufferWriter<byte> identity, string value) {
        int bytes = Encoding.UTF8.GetByteCount(value);
        BinaryPrimitives.WriteInt32LittleEndian(identity.GetSpan(4), bytes);
        identity.Advance(4);
        identity.Advance(Encoding.UTF8.GetBytes(value, identity.GetSpan(bytes)));
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                           | [BINDING]                                                     |
| :-----: | :------------------ | :------------------------------------------------ | :------------------------------------------------------------ |
|  [01]   | wire ingress        | `Plan.Parser.ParseFrom` + `SubstraitDeserializer` | zero `Grpc.Tools` codegen; `Google.Protobuf` sole runtime dep |
|  [02]   | round trip          | NORMALIZED retained wire (`AdbcQuery` door)       | `SubstraitSerializer` `internal`; no managed-IR re-lowering   |
|  [03]   | plan digest         | `ContentHash.Of(wireBytes)`                       | the kernel seed-zero entry; never a local `XxHash128`         |
|  [04]   | source capability   | `SourceKind.AcceptsPlan`/`IsLive` derivations     | binding and currency follow the closed source case            |
|  [05]   | function references | `FunctionExtensions.Functions*` URI catalogs      | no magic-string Substrait function names                      |
|  [06]   | producers           | `python:data` + `python:artifacts` GATED          | named blockers; the wire never pretends to work               |

## [03]-[PLAN_LOWERING]

- Owner: `LoweringTarget` the two-arm `[Union]` the visitor folds every relation into (`Keyed(SetExpr)` the key-selection half, `Tabular(Relation)` the columnar half); `FederationLowering` the `RelationVisitor<Fin<LoweringTarget>, Unit>` double-dispatch fold covering the FULL relation roster (the base class throws `NotImplementedException` on an unhandled kind, so a partial visitor fails LOUD and the funnel converts it to `UnsupportedRelation` — never a silent drop); `FederationPorts` the injected execution ports (`SetResolve` from `Query/lane`, the columnar `AdbcQuery` arm from `Query/columnar`, the watermark measure, the clock); `FederatedResult` the receipt implementing the kernel `IValidityEvidence` floor; `Federation` the static surface owning the ONE `Execute` entry — cut-shape default and cadence dispatch internalized, so no caller-orchestrated sibling exists.
- Cases: `SetRelation` lowers verified union and intersection variants when every input is keyed; every other set operation remains tabular instead of defaulting to difference. `VirtualTableReadRelation` admits every key on the `Fin` rail. `ExchangeRelation` remains tabular so partition semantics survive. `WriteRelation` rails `WriteRejected`; every engine-owned relation remains tabular.
- Entry: `Execute` resolves the optional cut, threads watermark failure, dispatches by cadence, and preserves every execution rail. `OneShot` composes the `Fin<ElementSet>` or `Fin<Seq<RecordBatch>>` result. `Materialized` passes the `Plan` returned by `SubstraitToDifferentialCompute.Convert` into `FederationPorts.Materialize`; conversion alone never counts as execution.
- Auto: the lowering is a VISITOR fold, never a switch over relation type names — `Relation.Accept` double-dispatches into the typed `Visit*` overrides so a new Substrait relation kind surfaces as the base-class throw the funnel converts to `UnsupportedRelation`; only a one-column `id` schema enters the key-selection arm, preventing a filtered multi-column relation from losing its row payload; predicate pushdown resolves a root `StructReferenceSegment.Field` through the relation's `NamedStruct.Names`, admits the result through `SetPath`, and composes comparison, range, `LIKE`, null, `AND`, and `OR` functions into `SetExpr`; the full `RelationVisitor` roster lowers explicitly, with engine-owned plan, normalization, iteration-reference, buffer, substream, and exchange-reference relations remaining tabular; the `(plan-digest·full-cut·watermark)` replay frame includes the `Hlc.Logical` counter and optional stream version before `ContentHash.Of` mints `FederatedResult.ReplayKey`; an unreachable live endpoint lifts at the columnar/ADBC boundary into `SourceUnreachable`, structurally distinct from the `SourceUncapable` capability refusal.
- Receipt: an execution rides `store.federation.execute` carrying the digest, the cut, the watermark gap, the source row, and the arm taken (`keyed`/`tabular`/`materialized`); a replay hit rides the `Query/cache` reuse index receipts, never a second fact stream here; the `Materialized` arm's fact rides `store.federation.materialize` carrying the view table.
- Packages: FlowtideDotNet.Substrait (`RelationVisitor<TReturn,TState>`/`ExpressionVisitor<TOutput,TState>`/`Relation` roster/`SetOperation`/`Conversion.SubstraitToDifferentialCompute`), Apache.Arrow (`RecordBatch` — the owned batch currency the `Tabular` port drains inside the columnar ADBC statement window; a live `QueryResult` never crosses the port), Rasm (`Rasm.Domain` `ContentHash`/`IValidityEvidence`/`ValidityClaim`), Rasm.Element (`NodeId`), Rasm.Persistence (`Query/lane#ELEMENT_SET_ALGEBRA` `SetExpr`/`SetResolve`/`ElementSet`, `Query/lane#READ_ROUTING` `StalenessWatermark`, `Query/columnar` `AdbcQuery`, `Version/timetravel#TIME_CUT` `TimeCut` — frozen vocabulary), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new relation kind is one `Visit*` override lowering to an existing arm; a new execution surface is one `LoweringTarget` case plus one `Execute` arm; a new pushdown predicate is one `SetPredicate` mapping row in the expression fold; zero new surface — a second engine beside the standing lanes, a thin single-door single-relation-arm lowering, a switch over relation type names beside the visitor, a `Seq<Error>`-flattened lowering failure, or a replay key minted off a second hasher is the deleted form because the owner is a router/lowerer, the visitor is total-by-throw, and the replay identity composes the kernel digest.
- Boundary: `SetExpr` executes only when `SourceKind.IsLive` is false; live sources ship the retained wire to the tabular port. `WriteRelation` refuses fail-closed. `SubstraitToDifferentialCompute.Convert` mutates and returns a `Plan`, and the materialization port must execute that returned plan before a receipt succeeds; a materialized mode without a primary key rails `MaterializationRejected` at `FederationPlan.Admit`. `FederationPlan` and `FederatedResult` expose no public constructor, so admission and success stamping cannot be bypassed; an empty keyed or tabular result remains valid execution evidence. `FederatedResult` frames the complete cut, source, and mode identity into `ReplayKey`, so distinct HLC cells, stream versions, bindings, and materialized views cannot collide.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// `LoweringTarget` separates index-backed keyed selection from tabular ADBC execution.
// Visitor failures travel inside `Fin<LoweringTarget>` and abort the fold.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LoweringTarget {
    private LoweringTarget() { }
    public sealed record Keyed(SetExpr Expr) : LoweringTarget;
    public sealed record Tabular(Relation Subtree) : LoweringTarget;
}

// --- [SERVICES] -----------------------------------------------------------------------------
// `FederationPorts` inject selection, tabular, live, watermark, materialization, and clock boundaries.
// Tabular execution drains owned record batches before its ADBC statement closes.
public sealed record FederationPorts(
    SetResolve Resolve,
    Func<AdbcRequest, IO<Fin<Seq<RecordBatch>>>> Tabular,
    Func<Plan, IO<Fin<Unit>>> Materialize,
    IO<Fin<StalenessWatermark>> Watermark,
    Func<Instant> Now);

// --- [OPERATIONS] ---------------------------------------------------------------------------
// `FederationLowering` implements every catalogued Substrait relation visitor override.
// Base throws for new relation kinds, and `Execute` converts the failure to `UnsupportedRelation`.
public sealed class FederationLowering : RelationVisitor<Fin<LoweringTarget>, Unit> {
    public override Fin<LoweringTarget> VisitRootRelation(RootRelation root, Unit state) => Visit(root.Input, state);

    // Verified union/intersection variants lower locally; every other operation preserves its full tabular semantics.
    public override Fin<LoweringTarget> VisitSetRelation(SetRelation set, Unit state) =>
        toSeq(set.Inputs).Map(input => Visit(input, state)).TraverseM(identity).As().Map(lowered =>
            lowered.ForAll(static target => target is LoweringTarget.Keyed)
                ? set.Operation switch {
                    SetOperation.UnionAll or SetOperation.UnionDistinct => (LoweringTarget)new LoweringTarget.Keyed(lowered.Map(static target => ((LoweringTarget.Keyed)target).Expr).Reduce(static (left, right) => new SetExpr.Union(left, right))),
                    SetOperation.IntersectionPrimary or SetOperation.IntersectionMultiset or SetOperation.IntersectionMultisetAll => new LoweringTarget.Keyed(lowered.Map(static target => ((LoweringTarget.Keyed)target).Expr).Reduce(static (left, right) => new SetExpr.Intersect(left, right))),
                    _ => new LoweringTarget.Tabular(set),
                }
                : new LoweringTarget.Tabular(set));

    // ReadRelation: the filter pushes down through the expression fold onto a typed SetPredicate leaf where the
    // store index serves it; an inexpressible filter keeps the whole read in the tabular subtree.
    public override Fin<LoweringTarget> VisitReadRelation(ReadRelation read, Unit state) =>
        Fin.Succ((SetLowering.IsKeyed(read) ? SetLowering.Predicate(read.Filter, read.BaseSchema.Names) : None).Match(
            Some: static expr => (LoweringTarget)new LoweringTarget.Keyed(expr),
            None: () => new LoweringTarget.Tabular(read)));

    public override Fin<LoweringTarget> VisitFilterRelation(FilterRelation filter, Unit state) =>
        Visit(filter.Input, state).Map(inner => inner is LoweringTarget.Keyed keyed
            ? SetLowering.Schema(filter.Input).Bind(fields => SetLowering.Predicate(filter.Condition, fields)).Match(
                Some: expr => (LoweringTarget)new LoweringTarget.Keyed(new SetExpr.Intersect(keyed.Expr, expr)),
                None: () => new LoweringTarget.Tabular(filter))
            : new LoweringTarget.Tabular(filter));

    public override Fin<LoweringTarget> VisitVirtualTableReadRelation(VirtualTableReadRelation literal, Unit state) =>
        SetLowering.IsKeyed(literal)
            ? SetLowering.Keys(literal).Map(keys => (LoweringTarget)new LoweringTarget.Keyed(new SetExpr.Literal(keys)))
            : Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(literal));

    // Bounded keyed iterations lower to `Closure`; unbounded or seedless iterations remain tabular.
    // `WalkDepth` admits foreign bounds before the fold.
    public override Fin<LoweringTarget> VisitIterationRelation(IterationRelation iteration, Unit state) =>
        (Optional(iteration.MaxIterations), Optional(iteration.Input)).Apply((depth, seed) =>
            Visit(seed, state).Bind(lowered => lowered is LoweringTarget.Keyed keyed
                ? WalkDepth.Validate(depth, null, out WalkDepth walk) is { } fault
                    ? Fin.Fail<LoweringTarget>(fault)
                    : Fin.Succ((LoweringTarget)new LoweringTarget.Keyed(new SetExpr.Closure(keyed.Expr, walk)))
                : Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(iteration))))
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
        Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(exchange));

    // Whole-plan, buffer, exchange, and table-function relations retain tabular execution.
    // Explicit overrides reserve base throws for uncatalogued relation kinds.
    public override Fin<LoweringTarget> VisitMergeJoinRelation(MergeJoinRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitReferenceRelation(ReferenceRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitTableFunctionRelation(TableFunctionRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitPlanRelation(PlanRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitNormalizationRelation(NormalizationRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitIterationReferenceReadRelation(IterationReferenceReadRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitBufferRelation(BufferRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitSubStreamRootRelation(SubStreamRootRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitPullExchangeReferenceRelation(PullExchangeReferenceRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitStandardOutputExchangeReferenceRelation(StandardOutputExchangeReferenceRelation rel, Unit state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
}

// `SetLowering` composes admitted literal and predicate pushdown through catalogued Substrait functions.
// Only index-servable shapes return typed `SetExpr`; every other expression preserves tabular execution.
public static class SetLowering {
    static readonly PredicateLowering Pushdown = new();

    public static Option<SetExpr> Predicate(Expression? filter, IReadOnlyList<string> fields) =>
        Optional(filter).Bind(condition => Optional(condition.Accept(Pushdown, fields)));

    public static Option<IReadOnlyList<string>> Schema(Relation relation) => relation switch {
        ReadRelation read => Some<IReadOnlyList<string>>(read.BaseSchema.Names),
        FilterRelation filter => Schema(filter.Input),
        _ => None,
    };

    public static bool IsKeyed(ReadRelation relation) => KeySchema(relation.BaseSchema.Names, relation.OutputLength);
    public static bool IsKeyed(VirtualTableReadRelation relation) => KeySchema(relation.BaseSchema.Names, relation.OutputLength);

    static bool KeySchema(IReadOnlyList<string> fields, int outputLength) =>
        outputLength == 1 && fields.Count == 1 && string.Equals(fields[0], "id", StringComparison.Ordinal);

    // One-column string virtual tables lower to literal NodeId sets; other schemas remain tabular.
    public static Fin<Seq<NodeId>> Keys(VirtualTableReadRelation literal) =>
        toSeq(literal.Values.Expressions)
            .Map(static row => row.Fields is [StringLiteral key, ..]
                ? Try.lift(() => NodeId.Create(key.Value)).Run().MapFail(error => (Error)new FederationFault.SubstraitParse(error.Message))
                : Fin.Fail<NodeId>(new FederationFault.SubstraitParse("<virtual-key>")))
            .TraverseM(identity)
            .As();

    // Equal-key inner semijoins lower to set intersection; every other join remains engine-owned.
    public static bool KeySemijoin(JoinRelation join) =>
        join.Type == JoinType.Inner
        && join.Expression is ScalarFunction { ExtensionUri: FunctionsComparison.Uri, ExtensionName: FunctionsComparison.Equal } condition
        && condition.Arguments is [DirectFieldReference, DirectFieldReference];
}

// `PredicateLowering` covers comparisons, ranges, LIKE, null tests, and boolean composition.
// Inexpressible members return `null` and preserve the tabular subtree.
public sealed class PredicateLowering : ExpressionVisitor<SetExpr?, IReadOnlyList<string>> {
    // One pattern ladder over the function product — sequential `if (x is P v)` guards would re-declare
    // pattern locals at method scope (CS0128); the switch expression scopes each arm's bindings.
    public override SetExpr? VisitScalarFunction(ScalarFunction function, IReadOnlyList<string> fields) => function switch {
        { ExtensionUri: FunctionsBoolean.Uri, ExtensionName: FunctionsBoolean.And, Arguments: [Expression left, Expression right] } =>
            Combine(left, right, fields, static (l, r) => new SetExpr.Intersect(l, r)),
        { ExtensionUri: FunctionsBoolean.Uri, ExtensionName: FunctionsBoolean.Or, Arguments: [Expression left, Expression right] } =>
            Combine(left, right, fields, static (l, r) => new SetExpr.Union(l, r)),
        { ExtensionUri: FunctionsComparison.Uri, ExtensionName: FunctionsComparison.Between, Arguments: [DirectFieldReference field, StringLiteral floor, StringLiteral ceiling] } =>
            Path(field, fields).Map(path => (SetExpr)new SetExpr.Intersect(
                new SetExpr.Predicate(new SetPredicate.Jsonpath(path, JsonComparison.GreaterOrEqual, Some(floor.Value))),
                new SetExpr.Predicate(new SetPredicate.Jsonpath(path, JsonComparison.LessOrEqual, Some(ceiling.Value)))))
                .Match<SetExpr?>(Some: static expr => expr, None: static () => null),
        { ExtensionUri: FunctionsComparison.Uri, Arguments: [DirectFieldReference field, StringLiteral literal] } =>
            Compared(field, literal.Value, function.ExtensionName, fields),
        { ExtensionUri: FunctionsString.Uri, ExtensionName: FunctionsString.Like, Arguments: [DirectFieldReference field, StringLiteral literal] } =>
            Leaf(field, JsonComparison.Matches, Some(literal.Value), fields),
        { ExtensionUri: FunctionsComparison.Uri, ExtensionName: FunctionsComparison.IsNotNull, Arguments: [DirectFieldReference field] } =>
            Exists(field, fields),
        { ExtensionUri: FunctionsBoolean.Uri, ExtensionName: FunctionsBoolean.Not, Arguments: [ScalarFunction { ExtensionUri: FunctionsComparison.Uri, ExtensionName: FunctionsComparison.IsNull, Arguments: [DirectFieldReference field] }] } =>
            Exists(field, fields),
        _ => null,
    };

    SetExpr? Combine(Expression left, Expression right, IReadOnlyList<string> fields, Func<SetExpr, SetExpr, SetExpr> combine) =>
        (Optional(left.Accept(this, fields)), Optional(right.Accept(this, fields))).Apply(combine).As()
            .Match<SetExpr?>(Some: static expr => expr, None: static () => null);

    static SetExpr? Compared(DirectFieldReference field, string value, string operation, IReadOnlyList<string> fields) =>
        Comparison(operation).Bind(comparison => Path(field, fields).Map(path => (SetExpr)new SetExpr.Predicate(new SetPredicate.Jsonpath(path, comparison, Some(value)))))
            .Match<SetExpr?>(Some: static expr => expr, None: static () => null);

    static SetExpr? Leaf(DirectFieldReference field, JsonComparison comparison, Option<string> value, IReadOnlyList<string> fields) =>
        Path(field, fields).Map(path => (SetExpr)new SetExpr.Predicate(new SetPredicate.Jsonpath(path, comparison, value)))
            .Match<SetExpr?>(Some: static expr => expr, None: static () => null);

    static SetExpr? Exists(DirectFieldReference field, IReadOnlyList<string> fields) =>
        Path(field, fields).Map(path => (SetExpr)new SetExpr.Predicate(new SetPredicate.Exists(path)))
            .Match<SetExpr?>(Some: static expr => expr, None: static () => null);

    static Option<JsonComparison> Comparison(string operation) => operation switch {
        FunctionsComparison.Equal => Some(JsonComparison.Eq),
        FunctionsComparison.GreaterThan => Some(JsonComparison.GreaterThan),
        FunctionsComparison.GreaterThanOrEqual => Some(JsonComparison.GreaterOrEqual),
        FunctionsComparison.LessThan => Some(JsonComparison.LessThan),
        FunctionsComparison.LessThanOrEqual => Some(JsonComparison.LessOrEqual),
        _ => None,
    };

    static Option<SetPath> Path(DirectFieldReference field, IReadOnlyList<string> fields) =>
        field.ReferenceSegment is StructReferenceSegment { Field: >= 0, Child: null } segment
        && segment.Field < fields.Count
        && SetPath.Validate(fields[segment.Field], null, out SetPath path) is null
            ? Some(path)
            : None;
}

public static class Federation {
    // `Execute` resolves absent cuts from head sequence and injected clock, then dispatches plan cadence.
    // Callers cannot sequence cut resolution or materialization beside the plan.
    public static IO<Fin<FederatedResult>> Execute(FederationPlan plan, Option<TimeCut> cut, FederationPorts ports) =>
        ports.Watermark.Bind(measured => measured.Match(
            Succ: watermark => {
                TimeCut pinned = cut.IfNone(() => TimeCut.AtVersion(watermark.HeadSequence, new Hlc(ports.Now(), ulong.MaxValue)));
                return plan.Mode.Switch(
                    oneShot:      _ => OneShot(plan, pinned, watermark, ports),
                    materialized: mode => Materialized(plan, mode, pinned, watermark, ports));
            },
            Fail: fault => IO.pure(Fin<FederatedResult>.Fail(fault))));

    static IO<Fin<FederatedResult>> OneShot(FederationPlan plan, TimeCut cut, StalenessWatermark watermark, FederationPorts ports) =>
        IO.lift(() => Try.lift(() =>
                plan.Ir.Relations is [Relation root, ..]
                    ? new FederationLowering().Visit(root, unit)
                    : Fin.Fail<LoweringTarget>(new FederationFault.SubstraitParse("<empty-plan>")))
            .Run()
            .MapFail(static error => (Error)new FederationFault.UnsupportedRelation(error.Message))   // the base-class NotImplementedException funnel
            .Bind(static outcome => outcome))
        .Bind(lowered => lowered.Match(
            Succ: target => target.Switch(
                // Live sources always ride remote wire execution; only durable and signed sources use local keys.
                keyed: k => plan.Source.IsLive
                    ? Engine(plan, cut, watermark, ports)
                    : IO.pure(ElementSetAlgebra.Evaluate(k.Expr, ports.Resolve)
                        .Map(keys => Stamp(plan, cut, watermark, keys, None, ports.Now()))),
                tabular: t => Engine(plan, cut, watermark, ports)),
            Fail: fault => IO.pure(Fin<FederatedResult>.Fail(fault))));

    // `Materialized` lowers the same plan IR into the streaming differential-compute engine.
    // continuously-maintained view — one plan model for both cadences, never a second IR.
    static IO<Fin<FederatedResult>> Materialized(FederationPlan plan, FederationMode.Materialized mode, TimeCut cut, StalenessWatermark watermark, FederationPorts ports) =>
        IO.lift(() => Try.lift(() => SubstraitToDifferentialCompute.Convert(
                plan.Ir,
                addWriteRelation: true,
                (string)mode.View,
                [.. mode.Keys.Map(static key => (string)key)]))
            .Run()
            .MapFail(static error => (Error)new FederationFault.SubstraitParse(error.Message)))
        .Bind(converted => converted.Match(
            Succ: ir => ports.Materialize(ir)
                .Map(result => result.Map(_ => Stamp(plan, cut, watermark, ElementSet.Empty, None, ports.Now()))),
            Fail: fault => IO.pure(Fin<FederatedResult>.Fail(fault))));

    // Engine execution uses normalized protobuf plans or staged SQL through one admitted door.
    // source row, selects the statement form; an unreachable live endpoint lifts into SourceUnreachable here.
    static IO<Fin<FederatedResult>> Engine(FederationPlan plan, TimeCut cut, StalenessWatermark watermark, FederationPorts ports) =>
        ports.Tabular(new AdbcRequest(plan.Wire, None))
            .Map(result => result.Map(batch => Stamp(plan, cut, watermark, ElementSet.Empty, Some(batch), ports.Now())))
        | @catch<IO, Fin<FederatedResult>>(static error => error.IsExceptional, static e => IO.pure(Fin<FederatedResult>.Fail(new FederationFault.SourceUnreachable(e.Message))));

    static FederatedResult Stamp(FederationPlan plan, TimeCut cut, StalenessWatermark watermark, ElementSet keys, Option<Seq<RecordBatch>> batch, Instant at) =>
        FederatedResult.Of(plan.Digest, cut, watermark, plan.Source, keys, batch, plan.Mode, at);
}

// --- [MODELS] -------------------------------------------------------------------------------
// `FederatedResult` owns local keys, drained Arrow batches, watermark, complete cut, replay identity, and mode.
// `IsValid` composes one `ValidityClaim.All` fold.
public sealed class FederatedResult : IValidityEvidence {
    private FederatedResult(UInt128 planDigest, TimeCut cut, StalenessWatermark watermark, SourceKind source, ElementSet keys, Option<Seq<RecordBatch>> batch, FederationMode mode, Instant at) =>
        (PlanDigest, Cut, Watermark, Source, Keys, Batch, Mode, At) = (planDigest, cut, watermark, source, keys, batch, mode, at);

    public UInt128 PlanDigest { get; }
    public TimeCut Cut { get; }
    public StalenessWatermark Watermark { get; }
    public SourceKind Source { get; }
    public ElementSet Keys { get; }
    public Option<Seq<RecordBatch>> Batch { get; }
    public FederationMode Mode { get; }
    public Instant At { get; }

    internal static FederatedResult Of(UInt128 planDigest, TimeCut cut, StalenessWatermark watermark, SourceKind source, ElementSet keys, Option<Seq<RecordBatch>> batch, FederationMode mode, Instant at) =>
        new(planDigest, cut, watermark, source, keys, batch, mode, at);

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(PlanDigest != default),
        ValidityClaim.Of(Watermark.ProjectedSequence <= Watermark.HeadSequence));

    // Replay identity frames plan digest, complete cut, and watermark before kernel hashing.
    // Live-source replay remains a hint because remote rows carry read-time currency.
    public UInt128 ReplayKey {
        get {
            ArrayBufferWriter<byte> preimage = new();
            BinaryPrimitives.WriteUInt128BigEndian(preimage.GetSpan(16), PlanDigest);
            preimage.Advance(16);
            int kind = Encoding.UTF8.GetByteCount(Cut.Source.Key);
            BinaryPrimitives.WriteInt32LittleEndian(preimage.GetSpan(4), kind);
            preimage.Advance(4);
            preimage.Advance(Encoding.UTF8.GetBytes(Cut.Source.Key, preimage.GetSpan(kind)));
            BinaryPrimitives.WriteInt64LittleEndian(preimage.GetSpan(8), Cut.At.ToUnixTimeTicks());
            preimage.Advance(8);
            BinaryPrimitives.WriteUInt64LittleEndian(preimage.GetSpan(8), Cut.Ceiling.Logical);
            preimage.Advance(8);
            preimage.GetSpan(1)[0] = Cut.StreamVersion.IsSome ? (byte)1 : (byte)0;
            preimage.Advance(1);
            long streamVersion = Cut.StreamVersion.Match(Some: static version => version, None: static () => 0L);
            BinaryPrimitives.WriteInt64LittleEndian(preimage.GetSpan(8), streamVersion);
            preimage.Advance(8);
            BinaryPrimitives.WriteInt64LittleEndian(preimage.GetSpan(8), Watermark.HeadSequence);
            preimage.Advance(8);
            BinaryPrimitives.WriteInt64LittleEndian(preimage.GetSpan(8), Watermark.ProjectedSequence);
            preimage.Advance(8);
            Frame(preimage, Source.Identity);
            Frame(preimage, Mode.Identity);
            return ContentHash.Of(preimage.WrittenSpan);
        }
    }

    static void Frame(ArrayBufferWriter<byte> preimage, string value) {
        int bytes = Encoding.UTF8.GetByteCount(value);
        BinaryPrimitives.WriteInt32LittleEndian(preimage.GetSpan(4), bytes);
        preimage.Advance(4);
        preimage.Advance(Encoding.UTF8.GetBytes(value, preimage.GetSpan(bytes)));
    }
}
```

| [INDEX] | [POLICY]          | [VALUE]                                            | [BINDING]                                                      |
| :-----: | :---------------- | :------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | one entry         | `Execute(FederationPlan, Option<TimeCut>, ports)`  | router/lowerer over standing lanes; never a second engine      |
|  [02]   | lowering form     | `RelationVisitor<Fin<LoweringTarget>, Unit>`       | total-by-throw; unknown relation → `UnsupportedRelation`       |
|  [03]   | key-selection arm | `SetExpr` via `lane#SetResolve`                    | local non-live sources; unsupported set ops stay tabular       |
|  [04]   | tabular arm       | `ColumnarProfile.Federation` + `AdbcQuery` doors   | `from_substrait(blob)` local; ext `SubstraitPlan`/`SqlQuery`   |
|  [05]   | write posture     | `WriteRelation` → `WriteRejected`                  | fail-closed; federation reads, the store rail writes           |
|  [06]   | default cut       | `Option<TimeCut>` resolved INSIDE `Execute`        | `HeadSequence` head under the clock `Hlc`; never ambient now   |
|  [07]   | replay identity   | `(digest·cut·watermark·source·mode)` → `ReplayKey` | bindings and materialized views remain distinct                |
|  [08]   | receipt validity  | `IValidityEvidence` + `ValidityClaim.All`          | the kernel [C] floor; never a hand-rolled `&&` chain           |
|  [09]   | streaming cadence | `Mode.Materialized(View, Keys)` case dispatch      | one plan IR, one entry; never a sibling execution surface      |

## [04]-[FLIGHT_RESULT_PLANE]

- Owner: `FederationFlight` the `Apache.Arrow.Flight.Server` `FlightServer` subclass — the RESULT half of the plan wire: a portable plan flows IN through `#PLAN_INGRESS` and its batches flow BACK OUT through this producer as zero-copy Arrow record streams, so a Python or TypeScript analytics consumer never re-serializes through files or an ad-hoc wire; the ticket registry is the constructor-injected `Atom<HashMap<UInt128, FederatedResult>>` hold keyed by `ReplayKey`.
- Cases: `GetFlightInfo` takes a COMMAND descriptor whose `Command` bytes are the protobuf plan wire — it admits through `FederationPlan.Admit(new PlanWire.Protobuf(...), source, new FederationMode.OneShot())`, executes through the ONE `Federation.Execute`, holds the result under its `ReplayKey`, and answers a `FlightInfo` carrying the result schema, ONE `FlightEndpoint` whose `FlightTicket` is the big-endian `ReplayKey` bytes, and the honest `TotalRecords`/`TotalBytes`; `DoGet` redeems the 16-byte ticket against the hold and streams every batch through `FlightServerRecordBatchStreamWriter.WriteAsync` (the first write auto-emits the schema message); an unknown or expired ticket rails `FederationFault.TicketUnknown`.
- Entry: `public override async Task<FlightInfo> GetFlightInfo(FlightDescriptor descriptor, ServerCallContext context)` and `public override async Task DoGet(FlightTicket ticket, FlightServerRecordBatchStreamWriter responseStream, ServerCallContext context)` — the two overridden verbs; every other base verb keeps its base throw because this plane is a read-only result producer, never an ingest door (`DoPut`/`DoExchange` stay unimplemented by decision, not omission).
- Auto: the ticket IS the content-addressed result identity — `ReplayKey` already frames `(plan-digest·full-cut·watermark·source·mode)`, so a byte-identical plan re-described at the same cut redeems the SAME ticket and reuse is identity, never a session table; a keyed (`ElementSet`) result projects to ONE single-column `id` batch through `StringArray.Builder.AppendRange` over the set's sorted keys so keyed and tabular results stream through one verb with no result-shape fork; the hold is an `Atom` swap (idempotent CAS re-add under a race) whose eviction rides the `Query/cache` reuse index cadence — the hold is a serving window over already-executed results, never a second cache; the typed `TicketUnknown` fault converts to the gRPC `NotFound` status at the verb edge because a `FlightServer` verb has no rail return — the one platform-forced throw seam this plane carries.
- Receipt: a described plan rides `store.federation.flight.describe` carrying the digest and the minted ticket; a redeemed stream rides `store.federation.flight.stream` carrying the ticket, the batch count, and the drained rows.
- Packages: Apache.Arrow.Flight (`FlightServer`/`FlightDescriptor`/`FlightTicket`/`FlightInfo`/`FlightEndpoint`/`FlightLocation`/`FlightServerRecordBatchStreamWriter` — the served node; `api-arrow` Flight server family), Apache.Arrow (`RecordBatch`/`Schema.Builder`/`Field.Builder`/`StringArray.Builder`), Google.Protobuf (`ByteString` — the descriptor command and ticket payload carrier), Grpc.Core (`ServerCallContext`/`RpcException`/`StatusCode` — the hosting seam's per-call context), LanguageExt.Core, BCL inbox.
- Growth: a new result consumer is zero surface — it dials the host channel and redeems tickets; a new served identity axis is one `ReplayKey` preimage field (the `#PLAN_LOWERING` frame already owns it); a discovery need is the `ListFlights` verb overridden over the same hold; zero new surface — a bespoke result file drop, a second result wire beside the ticket plane, a session-keyed ticket, or a `DoPut` ingest arm on this producer is the deleted form because the ticket is the replay identity and plan ingress stays the `#PLAN_INGRESS` door.
- Boundary: AppHost owns the gRPC channel, TLS, credentials, and service binding — the `FlightServer` subclass is Persistence's contribution mapped at the host composition root exactly as the `Version/egress` `WireNative` sink rides the AppHost `OutboundHop` (the standing delivery-honesty split; a Persistence-owned listener is the strata inversion); the `python:data` consumer leg rides the `ARCHITECTURE.md [02]-[SEAMS]` Flight-ticket return edge beside the inbound `SubstraitPlan` wire; `DoGet` streams the HELD batches — a live `QueryResult` never crosses (the `#PLAN_LOWERING` `Tabular` port drained it inside the ADBC statement window), so the plane serves owned memory only; the hold's serving window bounds memory (an evicted result re-executes through `GetFlightInfo`, cost never correctness), and `Authority.Admit` gates the demand at the caller exactly as the `Store/blobstore` issuer grants are gated.

```csharp signature
using Apache.Arrow.Flight;
using Apache.Arrow.Flight.Server;
using Apache.Arrow.Types;
using Google.Protobuf;
using Grpc.Core;

namespace Rasm.Persistence.Query;

// --- [SERVICES] -----------------------------------------------------------------------------
// The result half of the federation wire: plans in through PLAN_INGRESS, batches out through DoGet.
// AppHost binds the gRPC service; this class is the Persistence contribution, constructor-injected.
public sealed class FederationFlight(FederationPorts ports, SourceKind source, Atom<HashMap<UInt128, FederatedResult>> hold) : FlightServer {
    public override async Task<FlightInfo> GetFlightInfo(FlightDescriptor descriptor, ServerCallContext context) {
        Fin<FederatedResult> executed = await FederationPlan
            .Admit(new PlanWire.Protobuf(descriptor.Command.Memory), source, new FederationMode.OneShot())
            .Match(
                Succ: plan => Federation.Execute(plan, None, ports),
                Fail: fault => IO.pure(Fin<FederatedResult>.Fail(fault)))
            .RunAsync().ConfigureAwait(false);
        return executed.Match(
            Succ: result => {
                _ = hold.Swap(held => held.AddOrUpdate(result.ReplayKey, result));
                Seq<RecordBatch> batches = Batches(result);
                return new FlightInfo(
                    batches.Head.Match(Some: static b => b.Schema, None: KeySchema),
                    descriptor,
                    [new FlightEndpoint(new FlightTicket(ByteString.CopyFrom(TicketBytes(result.ReplayKey))), [])],
                    batches.Sum(static b => (long)b.Length),
                    -1L);
            },
            Fail: fault => throw new RpcException(new Status(StatusCode.InvalidArgument, fault.Message)));   // gRPC verb edge: no rail return exists on FlightServer
    }

    public override async Task DoGet(FlightTicket ticket, FlightServerRecordBatchStreamWriter responseStream, ServerCallContext context) {
        // Exact-width admission BEFORE the fixed-width decode: FlightTicket is an opaque caller-controlled token,
        // so a non-16-byte payload terminates through the declared gRPC fault boundary, never a decode throw.
        if (ticket.Ticket.Length != 16) { throw new RpcException(new Status(StatusCode.InvalidArgument, $"<flight-ticket-width:{ticket.Ticket.Length}!=16>")); }
        UInt128 key = BinaryPrimitives.ReadUInt128BigEndian(ticket.Ticket.Span);
        await hold.Value.Find(key).Match(
            Some: async result => { foreach (RecordBatch batch in Batches(result)) { await responseStream.WriteAsync(batch).ConfigureAwait(false); } },
            None: () => throw new RpcException(new Status(StatusCode.NotFound, new FederationFault.TicketUnknown(key).Message))).ConfigureAwait(false);
    }

    // A keyed result streams as ONE single-column `id` batch; a tabular result streams its drained batches.
    static Seq<RecordBatch> Batches(FederatedResult result) => result.Batch.IfNone(() =>
        Seq(new RecordBatch.Builder().Append("id", false, new StringArray.Builder().AppendRange(result.Keys.Keys.Map(static k => k.Value)).Build()).Build()));

    static Schema KeySchema() => new Schema.Builder().Field(new Field.Builder().Name("id").DataType(StringType.Default).Nullable(false).Build()).Build();

    static byte[] TicketBytes(UInt128 key) {
        byte[] bytes = new byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(bytes, key);
        return bytes;
    }
}
```

| [INDEX] | [POLICY]         | [VALUE]                                        | [BINDING]                                                     |
| :-----: | :--------------- | :---------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | ticket identity  | `ReplayKey` big-endian 16 bytes                 | content-addressed; a re-described identical plan re-redeems   |
|  [02]   | verbs            | `GetFlightInfo` + `DoGet` only                  | read-only result plane; `DoPut`/`DoExchange` stay base throws |
|  [03]   | keyed projection | one `id` `StringArray` batch                    | keyed and tabular results share one stream verb               |
|  [04]   | hosting          | AppHost gRPC channel, Persistence `FlightServer` | the `WireNative` delivery-honesty split; no local listener    |
|  [05]   | hold             | `Atom<HashMap<UInt128, FederatedResult>>`       | serving window over executed results; eviction re-executes    |
