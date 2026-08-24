# [PERSISTENCE_QUERY_FEDERATION]

Rasm.Persistence admits protobuf, Substrait JSON, or registered-table SQL into one `FederationPlan`, then routes it through one `Execute` rail. `FederationLowering` preserves verified keyed union/intersection, typed predicates, admitted literal keys, bounded closure, and key semijoins; unsupported set operations, exchanges, and engine-owned relations remain tabular. `SourceKind` carries each attestation or external binding and derives capability and live currency from its case. One-shot execution composes `Fin<KeySelection>` or `Fin<Seq<RecordBatch>>`; materialized execution converts the plan and passes the returned IR to the injected materialization port before success. `ReplayKey` frames plan, cut, watermark, source, and mode identity.

## [01]-[INDEX]

- [02]-[PLAN_INGRESS]: `PlanWire` three-door admission under the ONE `WireLimits.Plan` ceiling, the `SourceKind` capability axis, the `FederationMode` cadence union, the retained-wire-bytes round-trip law, the `ContentHash.Of(wireBytes)` plan digest, and the `FederationFault` closed band.
- [03]-[PLAN_LOWERING]: `RelationVisitor` double-dispatch lowering onto `LoweringTarget`, the seam-closure key-selection arm and the columnar/ADBC tabular arm, the ONE `Federation.Execute` entry owning the cut-shape default and the cadence dispatch, and the `FederatedResult` receipt with its replay triple.
- [04]-[FLIGHT_RESULT_PLANE]: `FederationFlight` the Arrow Flight return wire — a `ReplayKey`-ticketed producer whose `GetFlightInfo` admits-and-executes a command-descriptor plan and whose `DoGet` streams the held result's record batches zero-copy to a cross-runtime consumer, and the host binding contract that subclass satisfies.
- [05]-[PLAN_WIRE_SKEW]: extension-schema divergence across the frozen `SubstraitPlan` edge, why it parses clean in both directions, and which end refuses it.

## [02]-[PLAN_INGRESS]

- Owner: `SourceKind` is the closed source-binding family; each case carries the identity required to distinguish an attested artifact or external binding, while `AcceptsPlan` and `IsLive` derive from the case. `FederationMode` owns cadence and materialized-view identity. `PlanWire` owns the three ingress forms. `WireLimits` is this folder's decode-budget record and `Plan` its one row — the foreign-plan ceiling both plan doors read. `FederationPlan.Admit` normalizes each form and mints one digest; `PlanJson` is the page-declared `JsonParser` over the foreign Substrait descriptor.
- Cases: `SourceKind` is `DurableStore | SignedArtifact(UInt128 Attestation) | AdbcWarehouse(Identifier Binding) | SqlStaged(Identifier Binding)`; `FederationMode` is `OneShot | Materialized(Identifier View, Seq<Identifier> Keys)`; `PlanWire` is `Protobuf | Json | Sql(string Text, Seq<(Identifier Table, NamedStruct Schema)> Tables)`; `FederationFault` occupies `8420` through `8428`.
- Entry: `public static Fin<FederationPlan> Admit(PlanWire wire, SourceKind source, FederationMode mode)` admits the foreign plan ONCE — the `Protobuf` door parses `WirePlan.Parser.ParseFrom(CodedInputStream.CreateWithLimits(bytes.AsStream(), WireLimits.Plan.SizeLimit, WireLimits.Plan.RecursionLimit))` (the ONLY limits entry, the span bridge `CommunityToolkit.HighPerformance` supplies) and lifts through `new SubstraitDeserializer().Deserialize(parsed)`; the `Json` door refuses a body past `SizeLimit` before any parse, parses the Substrait-JSON through `PlanJson.Parse<WirePlan>` under the same recursion ceiling with unknown fields tolerated (Substrait-JSON IS the message's own wire-JSON), retains `ToByteArray()` — the canonical protobuf twin, so a JSON plan and its byte-identical protobuf sibling share ONE digest — and lifts through the same `Deserialize(parsed)`; the `Sql` door registers each `(Table, Schema)` through `SqlPlanBuilder.AddTableDefinition`, lowers the text through `Sql(text)`, and composes `GetPlan()` — every door normalizing to its retained wire and stamping `Digest = ContentHash.Of(wireBytes)`; a `SubstraitParseException` or a protobuf decode fault rails `FederationFault.SubstraitParse`, and a plan door against a `SqlStaged` source rails `SourceUncapable` BEFORE any parse.
- Auto: the retained bytes ARE the outbound wire — `SubstraitSerializer` is `internal`, so a managed `Plan` cannot re-lower to protobuf and the round-trip law is retention, never re-serialization (`api-flowtide-substrait#IMPLEMENTATION_LAW`); the digest composes the kernel seed-zero `ContentHash.Of` so the plan identity, the blob residence, and the reuse index share ONE identity scheme (a local `XxHash128` mint beside it is the deleted second hasher); function references inside a `Sql`-door plan resolve through the `FunctionExtensions.Functions*` URI catalogs (`FunctionsComparison.Equal`, `FunctionsArithmetic.Sum`, …) so no magic string names a Substrait function; custom federation tables and operators register through `ITableProvider`/`ISqlFunctionRegister` — the schema catalog is the table provider, never an ad-hoc string.
- Receipt: an admission rides `store.federation.admit` carrying the door, the source row, and the digest; a refused admission rides the typed `FederationFault` on the rail, never a receipt.
- Packages: FlowtideDotNet.Substrait (`Plan`/`SubstraitDeserializer`/`Substrait.Protobuf.Plan.Parser`/`SqlPlanBuilder`/`ITableProvider`/`ISqlFunctionRegister`/`FunctionExtensions`/`Exceptions.SubstraitParseException`), Google.Protobuf (`MessageParser<T>`/`IMessage`/`CodedInputStream.CreateWithLimits` — the one bounded binary door; `JsonParser` + `JsonParser.Settings.WithIgnoreUnknownFields`/`WithRecursionLimit` — the page-declared foreign-descriptor parser, since the substrait messages ship in FlowtideDotNet outside the estate's `WireAdmission.Registry`), CommunityToolkit.HighPerformance (`ReadOnlyMemory<byte>.AsStream()` — the span-to-stream bridge `CreateWithLimits` demands), Rasm (`Rasm.Domain` `ContentHash`/`Fault`), Rasm.Persistence (`Element/graph#FAULT_TABLES` `FaultBand`, `Query/columnar` `AdbcQuery`/`AdbcRequest` — the normalized statement door), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new ingress, source, cadence, or refusal is one case on its existing closed family; every source case carries its execution binding and replay identity.
- Boundary: the plan is a vendor-neutral IR, never a store connection — admission yields a value and opens nothing; both plan doors are UNTRUSTED ingress bounded by the ONE `WireLimits.Plan` row — a plan past the size or recursion ceiling refuses as `SubstraitParse` at the binary door and `InvalidPlan` at the JSON size gate, never an unbounded allocation; `SourceKind` is CAPABILITY data, so `SourceUncapable` is a structural refusal (a SQL-only warehouse never sees a plan blob) and `SourceUnreachable` is the availability negative of the LIVE rows only; the cross-runtime producer seams stay GATED — the `python:data` portable-plan half (the `ARCHITECTURE.md [02]-[SEAMS]` `Query`↔`Data` `[WIRE]: SubstraitPlan` edge, signature-locked) and the `python:artifacts` `SignedArtifact` binding are named blockers this owner declares, never silently-working stubs — while the `KeySelection` receipt currency itself stays owned by `Query/lane`; the `SignedArtifact` row resolves its binding through the attested ledger so a federated read over an externally-computed (including cloud-run) result is tamper-evident locally before it executes.

```csharp signature
using System.Collections.Generic;
using System.Globalization;
using Apache.Arrow;
using Apache.Arrow.Adbc;
using CommunityToolkit.HighPerformance;            // ReadOnlyMemory<byte>.AsStream — the bridge into CreateWithLimits
using FlowtideDotNet.Substrait;
using FlowtideDotNet.Substrait.Conversion;
using FlowtideDotNet.Substrait.Exceptions;
using FlowtideDotNet.Substrait.Expressions;
using FlowtideDotNet.Substrait.Expressions.Literals;
using FlowtideDotNet.Substrait.Relations;
using FlowtideDotNet.Substrait.Sql;
using FlowtideDotNet.Substrait.Type;
using Google.Protobuf;                             // CodedInputStream.CreateWithLimits + JsonParser — the two bounded plan doors
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Query;                          // Selection<TKey>, WalkDepth — the seam selection vocabulary
using Rasm.Persistence.Element;                    // ModelId — the stream grain the keyed projection qualifies by
using Thinktecture;
// Seam closure instantiated over this folder's store leaf family; this alias resolves the CS0104 ambiguity
// against the implicitly-imported `System.Predicate<T>` delegate.
using SetQuery = Rasm.Element.Query.Predicate<Rasm.Persistence.Query.SetPredicate>;
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

// --- [ERRORS] ---------------------------------------------------------------------------
// Direct generated union; arm probe: `error.IsType<FederationFault.TicketUnknown>()`.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FederationFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.StoreFederation;
    private FederationFault() { }
    [FaultCase(0)]
    public sealed partial record InvalidPlan(string Detail) : FederationFault();

    [FaultCase(1)]
    public sealed partial record SubstraitParse(Error Cause) : FederationFault(), ICausedFault;
    [FaultCase(2)]
    public sealed partial record UnsupportedRelation(Error Cause) : FederationFault(), ICausedFault;
    [FaultCase(3)]
    public sealed partial record SourceUnreachable(Error Cause) : FederationFault(), ICausedFault;
    [FaultCase(4)]
    public sealed partial record WriteRejected(string Table) : FederationFault();
    [FaultCase(5)]
    public sealed partial record SourceUncapable(string Source) : FederationFault();
    [FaultCase(6)]
    public sealed partial record MaterializationRejected(string Detail) : FederationFault();
    [FaultCase(7)]
    public sealed partial record TicketUnknown(UInt128 Ticket) : FederationFault();
    // `FlightTicket` carries an opaque caller-controlled token, so a wrong-width payload refuses apart from
    // an unredeemable one: the first never addressed the hold at all, and folding it onto `TicketUnknown`
    // reported a key the caller never sent.
    [FaultCase(8)]
    public sealed partial record TicketMalformed(int Width) : FederationFault();

    public override string Message => Switch(
        invalidPlan:             static c => $"<substrait-plan:{c.Detail}>",
        substraitParse:          static c => $"<substrait-parse:{c.Cause.Message}>",
        unsupportedRelation:     static c => $"<federation-unsupported-relation:{c.Cause.Message}>",
        sourceUnreachable:       static c => $"<federation-source-unreachable:{c.Cause.Message}>",
        writeRejected:           static c => $"<federation-write-rejected:{c.Table}>",
        sourceUncapable:         static c => $"<federation-source-uncapable:{c.Source}>",
        materializationRejected: static c => $"<federation-materialization-rejected:{c.Detail}>",
        ticketUnknown:           static c => string.Create(CultureInfo.InvariantCulture, $"<federation-ticket-unknown:{c.Ticket:x32}>"),
        ticketMalformed:         static c => string.Create(CultureInfo.InvariantCulture, $"<federation-ticket-width:{c.Width}!=16>"));
}

// --- [MODELS] -----------------------------------------------------------------------------
// ONE foreign-plan ceiling, the folder's own decode-budget row on the shape `Rasm.Element` `WireLimits` set. It is
// declared in THIS namespace so the bound type is the folder's row — a namespace member resolves ahead of the
// `Rasm.Element.Graph` import, so no alias is owed. `Plan` names both axes: the size ceiling bounds the whole-plan
// transfer, the recursion ceiling the nested relation/expression tree under protobuf's own default of 100.
public sealed record WireLimits(int SizeLimit, int RecursionLimit) {
    public static readonly WireLimits Plan = new(SizeLimit: 16 << 20, RecursionLimit: 64);
}

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

    // Substrait-JSON is a FOREIGN publisher descriptor — its messages ship inside FlowtideDotNet, outside the estate's
    // `WireAdmission.Registry` — so this is the ONE page-declared parser: unknown fields tolerate (the one admission
    // posture) and the recursion ceiling is the same `Plan` row the binary door reads.
    static readonly JsonParser PlanJson = new(JsonParser.Settings.Default
        .WithIgnoreUnknownFields(true)
        .WithRecursionLimit(WireLimits.Plan.RecursionLimit));

    public static Fin<FederationPlan> Admit(PlanWire wire, SourceKind source, FederationMode mode) =>
        mode is FederationMode.Materialized { Keys.IsEmpty: true }
            ? Fin.Fail<FederationPlan>(new FederationFault.MaterializationRejected("<primary-key>"))
            : !source.AcceptsPlan && wire is not PlanWire.Sql
            ? Fin.Fail<FederationPlan>(new FederationFault.SourceUncapable(source.Identity))
            // The JSON door's size gate sits BEFORE the parser, because a parser bounds depth and never length.
            : wire is PlanWire.Json { Body.Length: > WireLimits.Plan.SizeLimit } oversize
            ? Fin.Fail<FederationPlan>(new FederationFault.InvalidPlan($"<plan-size:{oversize.Body.Length}>"))
            : Op.Of().Catch(() => Fin.Succ(wire.Switch<(Plan Ir, AdbcQuery Wire, UInt128 Digest)>(
                    // Bounded binary door: the limits entry takes a Stream, so the span bridges through `AsStream`
                    // with no copy, and an over-ceiling payload throws `InvalidProtocolBufferException` into the
                    // funnel below rather than allocating past the row.
                    protobuf: static p => {
                        WirePlan parsed = WirePlan.Parser.ParseFrom(CodedInputStream.CreateWithLimits(
                            p.Bytes.AsStream(), WireLimits.Plan.SizeLimit, WireLimits.Plan.RecursionLimit));
                        return (new SubstraitDeserializer().Deserialize(parsed), new AdbcQuery.Plan(p.Bytes.ToArray()), ContentHash.Of(p.Bytes.Span));
                    },
                    json: static j => {
                        WirePlan twin = PlanJson.Parse<WirePlan>(j.Body);   // Substrait-JSON IS the message's wire-JSON; a JSON plan and its protobuf twin share ONE digest
                        byte[] wireBytes = twin.ToByteArray();
                        return (new SubstraitDeserializer().Deserialize(twin), new AdbcQuery.Plan(wireBytes), ContentHash.Of(wireBytes));
                    },
                    // SQL doors hold no wire bytes to hash, so identity IS the STRUCTURE registered — statement
                    // text beside each table's name and its declared field NAMES, streamed through the kernel
                    // `CanonicalWriter` under `Sorted` so registration order cannot fork one digest.
                    // `NamedStruct.ToString()` renders no identity at all — its null
                    // render collapsed onto the empty string and aliased two distinct schemas onto one plan key.
                    sql: static s => {
                        SqlPlanBuilder builder = new();
                        s.Tables.Iter(table => builder.AddTableDefinition((string)table.Table, table.Schema));
                        builder.Sql(s.Text);
                        return (builder.GetPlan(), new AdbcQuery.Sql(AdbcSql.Create(s.Text)),
                            ContentHash.Of(s, static (door, w) => {
                                w.String(door.Text).Sorted(door.Tables, static table => (string)table.Table, StringComparer.Ordinal,
                                    static (table, x) => { x.String((string)table.Table).Rows(toSeq(table.Schema.Names), static (name, y) => { y.String(name); }); });
                            }));
                    })))
                .MapFail(static error => error.Exception.Case is SubstraitParseException or InvalidProtocolBufferException or InvalidJsonException
                    ? (Error)new FederationFault.SubstraitParse(error)
                    : error)
                .Map(admitted => new FederationPlan(admitted.Ir, admitted.Wire, admitted.Digest, source, mode));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                            | [BINDING]                                                   |
| :-----: | :------------------ | :------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | wire ingress        | `CreateWithLimits` under `WireLimits.Plan`         | one ceiling, both doors; `SubstraitDeserializer` lifts      |
|  [02]   | round trip          | NORMALIZED retained wire (`AdbcQuery` door)        | `SubstraitSerializer` `internal`; no managed-IR re-lowering |
|  [03]   | plan digest         | `ContentHash.Of(wireBytes)`                        | the kernel seed-zero entry; never a local `XxHash128`       |
|  [04]   | source capability   | `SourceKind.AcceptsPlan`/`IsLive` derivations      | binding and currency follow the closed source case          |
|  [05]   | function references | `FunctionExtensions.Functions*` URI catalogs       | no magic-string Substrait function names                    |
|  [06]   | producers           | `python:data` + `python:artifacts` GATED           | named blockers; the wire never pretends to work             |
|  [07]   | json door           | page-declared `PlanJson` over a FOREIGN descriptor | outside `WireAdmission.Registry`; unknown fields tolerate   |

## [03]-[PLAN_LOWERING]

- Owner: `LoweringTarget` the two-arm `[Union]` the visitor folds every relation into (`Keyed(SetQuery)` the key-selection half over the Element seam closure, `Tabular(Relation)` the columnar half); `FederationLowering` the `RelationVisitor<Fin<LoweringTarget>, SetScope>` double-dispatch fold covering the FULL relation roster (the base class throws `NotImplementedException` on an unhandled kind, so a partial visitor fails LOUD and the funnel converts it to `UnsupportedRelation` — never a silent drop); `FederationPorts` the injected execution ports (the caller-declared `SetScope` beside `SetResolve` from `Query/lane`, the columnar `AdbcQuery` arm from `Query/columnar`, the watermark measure, the clock); `FederatedResult` the receipt implementing the kernel `IValidityEvidence` floor; `Federation` the static surface owning the ONE `Execute` entry — cut-shape default and cadence dispatch internalized, so no caller-orchestrated sibling exists.
- Cases: `SetRelation` lowers verified union and intersection variants when every input is keyed; every other set operation remains tabular instead of defaulting to difference. `VirtualTableReadRelation` admits every key on the `Fin` rail. `ExchangeRelation` remains tabular so partition semantics survive. `WriteRelation` rails `WriteRejected`; every engine-owned relation remains tabular.
- Entry: `Execute` resolves the optional cut, threads watermark failure, dispatches by cadence, and preserves every execution rail. `OneShot` composes the `Fin<KeySelection>` or `Fin<Seq<RecordBatch>>` result. `Materialized` passes the `Plan` returned by `SubstraitToDifferentialCompute.Convert` into `FederationPorts.Materialize`; conversion alone never counts as execution.
- Auto: the lowering is a VISITOR fold, never a switch over relation type names — `Relation.Accept` double-dispatches into the typed `Visit*` overrides so a new Substrait relation kind surfaces as the base-class throw the funnel converts to `UnsupportedRelation`; only a one-column `id` schema enters the key-selection arm, preventing a filtered multi-column relation from losing its row payload; predicate pushdown resolves a root `StructReferenceSegment.Field` through the relation's `NamedStruct.Names`, admits the result through `SetPath`, and composes comparison, range, `LIKE`, null, `AND`, and `OR` functions into the seam closure whose n-ary combinators COALESCE, so an n-way Substrait union lowers to ONE `Any` node carrying a flat operand run rather than a left-leaning binary spine; the full `RelationVisitor` roster lowers explicitly, with engine-owned plan, normalization, iteration-reference, buffer, substream, and exchange-reference relations remaining tabular; the `(plan-digest·full-cut·watermark)` replay frame includes the `Hlc.Logical` counter and optional stream version before `ContentHash.Of` mints `FederatedResult.ReplayKey`; an unreachable live endpoint lifts at the columnar/ADBC boundary into `SourceUnreachable`, structurally distinct from the `SourceUncapable` capability refusal.
- Receipt: an execution rides `store.federation.execute` carrying the digest, the cut, the watermark gap, the source row, and the arm taken (`keyed`/`tabular`/`materialized`); a replay hit rides the `Query/cache` reuse index receipts, never a second fact stream here; the `Materialized` arm's fact rides `store.federation.materialize` carrying the view table.
- Packages: FlowtideDotNet.Substrait (`RelationVisitor<TReturn,TState>`/`ExpressionVisitor<TOutput,TState>`/`Relation` roster/`SetOperation`/`Conversion.SubstraitToDifferentialCompute`), Apache.Arrow (`RecordBatch` — the owned batch currency the `Tabular` port drains inside the columnar ADBC statement window; a live `QueryResult` never crosses the port), Rasm (`Rasm.Domain` `ContentHash`/`IValidityEvidence`/`ValidityClaim`), Rasm.Element (`NodeId`), Rasm.Element (`Query/predicate#PREDICATE_ALGEBRA` `Predicate<TLeaf>`/`WalkDepth` — the ONE boolean closure this lowering targets), Rasm.Persistence (`Query/lane#ELEMENT_SET_ALGEBRA` `SetPredicate`/`SetResolve`/`SetKey`/`SetScope`/`KeySelection`/`Selections`, `Query/residence#COLUMN_VOCABULARY` `AnalyticsSchema`/`ColumnRow`/`ColumnCell`/`ArrowLanding` — the ONE record-batch fold, `Query/lane#READ_ROUTING` `StalenessWatermark`, `Query/columnar` `AdbcQuery`, `Version/timetravel#TIME_TRAVEL` `TimeCut` — frozen vocabulary), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new relation kind is one `Visit*` override lowering to an existing arm; a new execution surface is one `LoweringTarget` case and one `Execute` arm; a new pushdown predicate is one `SetPredicate` mapping row in the expression fold; zero new surface — a second engine beside the standing lanes, a thin single-door single-relation-arm lowering, a switch over relation type names beside the visitor, a `Seq<Error>`-flattened lowering failure, or a replay key minted off a second hasher is the deleted form because the owner is a router/lowerer, the visitor is total-by-throw, and the replay identity composes the kernel digest.
- Boundary: the keyed arm executes only when `SourceKind.IsLive` is false; live sources ship the retained wire to the tabular port. `WriteRelation` refuses fail-closed. `SubstraitToDifferentialCompute.Convert` mutates and returns a `Plan`, and the materialization port must execute that returned plan before a receipt succeeds; a materialized mode without a primary key rails `MaterializationRejected` at `FederationPlan.Admit`. `FederationPlan` and `FederatedResult` expose no public constructor, so admission and success stamping cannot be bypassed; an empty keyed or tabular result remains valid execution evidence. `FederatedResult` frames the complete cut, source, and mode identity into `ReplayKey`, so distinct HLC cells, stream versions, bindings, and materialized views cannot collide.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// `LoweringTarget` separates index-backed keyed selection from tabular ADBC execution.
// Visitor failures travel inside `Fin<LoweringTarget>` and abort the fold.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LoweringTarget {
    private LoweringTarget() { }
    public sealed record Keyed(SetQuery Query) : LoweringTarget;
    public sealed record Tabular(Relation Subtree) : LoweringTarget;
}

// --- [SERVICES] -----------------------------------------------------------------------------
// `FederationPorts` inject the read scope, selection, tabular, live, watermark, materialization, and clock
// boundaries. `Scope` is the caller-declared model roster the read-scope law demands — the plan carries
// filters, projections, and folds alone, never the models it may touch — and tabular execution drains owned
// record batches before its ADBC statement closes.
public sealed record FederationPorts(
    SetScope Scope,
    SetResolve Resolve,
    Func<AdbcRequest, IO<Fin<Seq<RecordBatch>>>> Tabular,
    Func<Plan, IO<Fin<Unit>>> Materialize,
    IO<Fin<StalenessWatermark>> Watermark,
    Func<Instant> Now);

// --- [OPERATIONS] ---------------------------------------------------------------------------
// `FederationLowering` implements every catalogued Substrait relation visitor override.
// Base throws for new relation kinds, and `Execute` converts the failure to `UnsupportedRelation`.
public sealed class FederationLowering : RelationVisitor<Fin<LoweringTarget>, SetScope> {
    public override Fin<LoweringTarget> VisitRootRelation(RootRelation root, SetScope state) => Visit(root.Input, state);

    // Verified union/intersection variants lower locally; every other operation preserves its full tabular semantics.
    public override Fin<LoweringTarget> VisitSetRelation(SetRelation set, SetScope state) =>
        toSeq(set.Inputs).Map(input => Visit(input, state)).TraverseM(identity).As().Map(lowered =>
            lowered.ForAll(static target => target is LoweringTarget.Keyed)
                // Seam combinators COALESCE, so an n-way union folds to ONE `Any` node carrying a flat
                // operand run rather than the left-leaning binary spine the deleted `SetExpr.Union` built —
                // one evaluator pass instead of a recursion per input, and one association-independent key.
                ? set.Operation switch {
                    SetOperation.UnionAll or SetOperation.UnionDistinct => (LoweringTarget)new LoweringTarget.Keyed(lowered.Map(static target => ((LoweringTarget.Keyed)target).Query).Reduce(static (left, right) => left.Or(right))),
                    SetOperation.IntersectionPrimary or SetOperation.IntersectionMultiset or SetOperation.IntersectionMultisetAll => new LoweringTarget.Keyed(lowered.Map(static target => ((LoweringTarget.Keyed)target).Query).Reduce(static (left, right) => left.And(right))),
                    _ => new LoweringTarget.Tabular(set),
                }
                : new LoweringTarget.Tabular(set));

    // ReadRelation: the filter pushes down through the expression fold onto a typed SetPredicate leaf where the
    // store index serves it; an inexpressible filter keeps the whole read in the tabular subtree.
    public override Fin<LoweringTarget> VisitReadRelation(ReadRelation read, SetScope state) =>
        Fin.Succ((SetLowering.IsKeyed(read) ? SetLowering.Predicate(read.Filter, read.BaseSchema.Names) : None).Match(
            Some: static expr => (LoweringTarget)new LoweringTarget.Keyed(expr),
            None: () => new LoweringTarget.Tabular(read)));

    public override Fin<LoweringTarget> VisitFilterRelation(FilterRelation filter, SetScope state) =>
        Visit(filter.Input, state).Map(inner => inner is LoweringTarget.Keyed keyed
            ? SetLowering.Schema(filter.Input).Bind(fields => SetLowering.Predicate(filter.Condition, fields)).Match(
                Some: query => (LoweringTarget)new LoweringTarget.Keyed(keyed.Query.And(query)),
                None: () => new LoweringTarget.Tabular(filter))
            : new LoweringTarget.Tabular(filter));

    public override Fin<LoweringTarget> VisitVirtualTableReadRelation(VirtualTableReadRelation literal, SetScope state) =>
        SetLowering.IsKeyed(literal)
            ? SetLowering.Keys(literal, state).Map(keys => (LoweringTarget)new LoweringTarget.Keyed(new SetQuery.Leaf(new SetPredicate.Literal(keys))))
            : Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(literal));

    // Bounded keyed iterations lower to `Closure`; unbounded or seedless iterations remain tabular.
    // `WalkDepth` admits foreign bounds before the fold.
    public override Fin<LoweringTarget> VisitIterationRelation(IterationRelation iteration, SetScope state) =>
        (Optional(iteration.MaxIterations), Optional(iteration.Input)).Apply((depth, seed) =>
            Visit(seed, state).Bind(lowered => lowered is LoweringTarget.Keyed keyed
                ? Selections.Depth(depth).Map(walk => (LoweringTarget)new LoweringTarget.Keyed(new SetQuery.Closure(keyed.Query, walk)))
                : Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(iteration))))
        .As()
        .IfNone(() => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(iteration)));

    // Conjunction lowers a key-equijoin over two Keyed sides, which is a semijoin on the shared key
    // space; every other join shape (outer, theta, projection-bearing) belongs to the engine arm.
    public override Fin<LoweringTarget> VisitJoinRelation(JoinRelation join, SetScope state) =>
        (Visit(join.Left, state), Visit(join.Right, state)).Apply((left, right) =>
            (left, right, SetLowering.KeySemijoin(join)) switch {
                (LoweringTarget.Keyed l, LoweringTarget.Keyed r, true) => (LoweringTarget)new LoweringTarget.Keyed(l.Query.And(r.Query)),
                _ => new LoweringTarget.Tabular(join),
            }).As();

    public override Fin<LoweringTarget> VisitProjectRelation(ProjectRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitAggregateRelation(AggregateRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitSortRelation(SortRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitTopNRelation(TopNRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitFetchRelation(FetchRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitConsistentPartitionWindowRelation(ConsistentPartitionWindowRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));

    public override Fin<LoweringTarget> VisitWriteRelation(WriteRelation write, SetScope state) =>
        Fin.Fail<LoweringTarget>(new FederationFault.WriteRejected(Optional(write.NamedObject?.ToString()).IfNone("<write>")));   // fail-closed: federation READS

    public override Fin<LoweringTarget> VisitExchangeRelation(ExchangeRelation exchange, SetScope state) =>
        Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(exchange));

    // Whole-plan, buffer, exchange, and table-function relations retain tabular execution.
    // Explicit overrides reserve base throws for uncatalogued relation kinds.
    public override Fin<LoweringTarget> VisitMergeJoinRelation(MergeJoinRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitReferenceRelation(ReferenceRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitTableFunctionRelation(TableFunctionRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitPlanRelation(PlanRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitNormalizationRelation(NormalizationRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitIterationReferenceReadRelation(IterationReferenceReadRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitBufferRelation(BufferRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitSubStreamRootRelation(SubStreamRootRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitPullExchangeReferenceRelation(PullExchangeReferenceRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
    public override Fin<LoweringTarget> VisitStandardOutputExchangeReferenceRelation(StandardOutputExchangeReferenceRelation rel, SetScope state) => Fin.Succ((LoweringTarget)new LoweringTarget.Tabular(rel));
}

// `SetLowering` composes admitted literal and predicate pushdown through catalogued Substrait functions.
// Only index-servable shapes return a typed seam query; every other expression preserves tabular execution.
public static class SetLowering {
    static readonly PredicateLowering Pushdown = new();

    public static Option<SetQuery> Predicate(Expression? filter, IReadOnlyList<string> fields) =>
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

    // One-column string virtual tables lower to literal key sets, qualified under the execution scope: a
    // Substrait plan spells no model, so bare ids admit only when the scope names exactly ONE model — a
    // multi-model scope over a bare-id literal is an ambiguity the lowering refuses rather than guesses.
    public static Fin<Seq<SetKey>> Keys(VirtualTableReadRelation literal, SetScope scope) =>
        scope.Models is [var model]
            ? toSeq(literal.Values.Expressions)
                .Map(row => row.Fields is [StringLiteral key, ..]
                    ? Op.Of().Catch(() => Fin.Succ(new SetKey(model, NodeId.Create(key.Value))))
                    : Fin.Fail<SetKey>(new FederationFault.InvalidPlan("<virtual-key>")))
                .TraverseM(identity)
                .As()
            : Fin.Fail<Seq<SetKey>>(new FederationFault.InvalidPlan("<literal-model-ambiguous>"));

    // Equal-key inner semijoins lower to set intersection; every other join remains engine-owned.
    public static bool KeySemijoin(JoinRelation join) =>
        join.Type == JoinType.Inner
        && join.Expression is ScalarFunction { ExtensionUri: FunctionsComparison.Uri, ExtensionName: FunctionsComparison.Equal } condition
        && condition.Arguments is [DirectFieldReference, DirectFieldReference];
}

// `PredicateLowering` covers comparisons, ranges, LIKE, null tests, and boolean composition.
// Inexpressible members return `null` and preserve the tabular subtree.
public sealed class PredicateLowering : ExpressionVisitor<SetQuery?, IReadOnlyList<string>> {
    // One pattern ladder over the function product — sequential `if (x is P v)` guards would re-declare
    // pattern locals at method scope (CS0128); the switch expression scopes each arm's bindings.
    public override SetQuery? VisitScalarFunction(ScalarFunction function, IReadOnlyList<string> fields) => function switch {
        { ExtensionUri: FunctionsBoolean.Uri, ExtensionName: FunctionsBoolean.And, Arguments: [Expression left, Expression right] } =>
            Combine(left, right, fields, static (l, r) => l.And(r)),
        { ExtensionUri: FunctionsBoolean.Uri, ExtensionName: FunctionsBoolean.Or, Arguments: [Expression left, Expression right] } =>
            Combine(left, right, fields, static (l, r) => l.Or(r)),
        { ExtensionUri: FunctionsComparison.Uri, ExtensionName: FunctionsComparison.Between, Arguments: [DirectFieldReference field, StringLiteral floor, StringLiteral ceiling] } =>
            Path(field, fields).Map(path => (SetQuery)new SetQuery.Leaf(new SetPredicate.Jsonpath(path, JsonComparison.GreaterOrEqual, Some(floor.Value)))
                    .And(new SetQuery.Leaf(new SetPredicate.Jsonpath(path, JsonComparison.LessOrEqual, Some(ceiling.Value)))))
                .Match<SetQuery?>(Some: static query => query, None: static () => null),
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

    SetQuery? Combine(Expression left, Expression right, IReadOnlyList<string> fields, Func<SetQuery, SetQuery, SetQuery> combine) =>
        (Optional(left.Accept(this, fields)), Optional(right.Accept(this, fields))).Apply(combine).As()
            .Match<SetQuery?>(Some: static query => query, None: static () => null);

    static SetQuery? Compared(DirectFieldReference field, string value, string operation, IReadOnlyList<string> fields) =>
        Comparison(operation).Bind(comparison => Path(field, fields).Map(path => (SetQuery)new SetQuery.Leaf(new SetPredicate.Jsonpath(path, comparison, Some(value)))))
            .Match<SetQuery?>(Some: static query => query, None: static () => null);

    static SetQuery? Leaf(DirectFieldReference field, JsonComparison comparison, Option<string> value, IReadOnlyList<string> fields) =>
        Path(field, fields).Map(path => (SetQuery)new SetQuery.Leaf(new SetPredicate.Jsonpath(path, comparison, value)))
            .Match<SetQuery?>(Some: static query => query, None: static () => null);

    static SetQuery? Exists(DirectFieldReference field, IReadOnlyList<string> fields) =>
        Path(field, fields).Map(path => (SetQuery)new SetQuery.Leaf(new SetPredicate.Exists(path)))
            .Match<SetQuery?>(Some: static query => query, None: static () => null);

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
    public static readonly Seq<StoreSlot> Slots = Seq(
        StoreSlot.Create("store.federation.admit"), StoreSlot.Create("store.federation.execute"), StoreSlot.Create("store.federation.materialize"),
        StoreSlot.Create("store.federation.flight.describe"), StoreSlot.Create("store.federation.flight.stream"));

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
        IO.lift(() => Op.Of().Catch(() =>
                plan.Ir.Relations is [Relation root, ..]
                    ? new FederationLowering().Visit(root, ports.Scope)
                    : Fin.Fail<LoweringTarget>(new FederationFault.InvalidPlan("<empty-plan>")))
            .MapFail(static error => error.Exception.Case is NotImplementedException
                ? (Error)new FederationFault.UnsupportedRelation(error)
                : error))
        .Bind(lowered => lowered.Match(
            Succ: target => target.Switch(
                // Live sources always ride remote wire execution; only durable and signed sources use local keys.
                keyed: k => plan.Source.IsLive
                    ? Engine(plan, cut, watermark, ports)
                    : IO.pure(Selections.Evaluate(k.Query, ports.Scope, ports.Resolve)
                        .Map(keys => Stamp(plan, cut, watermark, keys, None, ports.Now()))),
                tabular: t => Engine(plan, cut, watermark, ports)),
            Fail: fault => IO.pure(Fin<FederatedResult>.Fail(fault))));

    // `Materialized` lowers the same plan IR into the streaming differential-compute engine.
    // continuously-maintained view — one plan model for both cadences, never a second IR.
    static IO<Fin<FederatedResult>> Materialized(FederationPlan plan, FederationMode.Materialized mode, TimeCut cut, StalenessWatermark watermark, FederationPorts ports) =>
        IO.lift(() => Op.Of().Catch(() => Fin.Succ(SubstraitToDifferentialCompute.Convert(
                plan.Ir,
                addWriteRelation: true,
                (string)mode.View,
                [.. mode.Keys.Map(static key => (string)key)])))
            .MapFail(static error => error.Exception.Case is SubstraitParseException
                ? (Error)new FederationFault.SubstraitParse(error)
                : error))
        .Bind(converted => converted.Match(
            Succ: ir => ports.Materialize(ir)
                .Map(result => result.Map(_ => Stamp(plan, cut, watermark, KeySelection.Empty(ports.Scope), None, ports.Now()))),
            Fail: fault => IO.pure(Fin<FederatedResult>.Fail(fault))));

    // Engine execution uses normalized protobuf plans or staged SQL through one admitted door.
    // source row, selects the statement form; an unreachable live endpoint lifts into SourceUnreachable here.
    static IO<Fin<FederatedResult>> Engine(FederationPlan plan, TimeCut cut, StalenessWatermark watermark, FederationPorts ports) =>
        ports.Tabular(new AdbcRequest(plan.Wire, None))
            .Map(result => result
                .MapFail(error => plan.Source.IsLive && error.Exception.Case is AdbcException
                    ? (Error)new FederationFault.SourceUnreachable(error)
                    : error)
                .Map(batch => Stamp(plan, cut, watermark, KeySelection.Empty(ports.Scope), Some(batch), ports.Now())));

    static FederatedResult Stamp(FederationPlan plan, TimeCut cut, StalenessWatermark watermark, KeySelection keys, Option<Seq<RecordBatch>> batch, Instant at) =>
        FederatedResult.Of(plan.Digest, cut, watermark, plan.Source, keys, batch, plan.Mode, at);
}

// --- [MODELS] -------------------------------------------------------------------------------
// `FederatedResult` owns local keys, drained Arrow batches, watermark, complete cut, replay identity, and mode.
// `IsValid` composes one `ValidityClaim.All` fold.
public sealed class FederatedResult : IValidityEvidence {
    private FederatedResult(UInt128 planDigest, TimeCut cut, StalenessWatermark watermark, SourceKind source, KeySelection keys, Option<Seq<RecordBatch>> batch, FederationMode mode, Instant at) =>
        (PlanDigest, Cut, Watermark, Source, Keys, Batch, Mode, At) = (planDigest, cut, watermark, source, keys, batch, mode, at);

    public UInt128 PlanDigest { get; }
    public TimeCut Cut { get; }
    public StalenessWatermark Watermark { get; }
    public SourceKind Source { get; }
    public KeySelection Keys { get; }
    public Option<Seq<RecordBatch>> Batch { get; }
    public FederationMode Mode { get; }
    public Instant At { get; }

    internal static FederatedResult Of(UInt128 planDigest, TimeCut cut, StalenessWatermark watermark, SourceKind source, KeySelection keys, Option<Seq<RecordBatch>> batch, FederationMode mode, Instant at) =>
        new(planDigest, cut, watermark, source, keys, batch, mode, at);

    // Bare predicates ride the implicit `bool -> ValidityClaim` conversion the kernel declares, so `ValidityClaim.Of(`
    // is the deleted spelling corpus-wide and no call site wraps.
    public bool IsValid => ValidityClaim.All(
        PlanDigest != default,
        Watermark.ProjectedSequence <= Watermark.HeadSequence);

    // Replay identity streams the kernel `CanonicalWriter`: `String` length-frames the cut source and the two
    // identity renders, `I64` fixes the four counters, `Optional` frames the stream version's presence and its
    // value together — where the hand buffer wrote a presence BYTE beside a zero value, so an absent version and
    // a version of zero differed by one byte a reader had to know to interpret. Live-source replay remains a
    // hint because remote rows carry read-time currency.
    public UInt128 ReplayKey => ContentHash.Of(this, static (result, w) => {
        w.U128(result.PlanDigest)
            .String(result.Cut.Source.Key)
            .I64(result.Cut.At.ToUnixTimeTicks())
            .I64(unchecked((long)result.Cut.Ceiling.Logical))
            .Optional(result.Cut.StreamVersion, static (version, x) => { x.I64(version); })
            .I64(result.Watermark.HeadSequence)
            .I64(result.Watermark.ProjectedSequence)
            .String(result.Source.Identity)
            .String(result.Mode.Identity);
    });
}
```

| [INDEX] | [POLICY]          | [VALUE]                                            | [BINDING]                                                    |
| :-----: | :---------------- | :------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | one entry         | `Execute(FederationPlan, Option<TimeCut>, ports)`  | router/lowerer over standing lanes; never a second engine    |
|  [02]   | lowering form     | `RelationVisitor<Fin<LoweringTarget>, SetScope>`   | total-by-throw; unknown relation → `UnsupportedRelation`     |
|  [03]   | key-selection arm | seam `Predicate<SetPredicate>` via `lane`          | local non-live sources; unsupported set ops stay tabular     |
|  [04]   | tabular arm       | `ColumnarProfile.Federation` + `AdbcQuery` doors   | `from_substrait(blob)` local; ext `SubstraitPlan`/`SqlQuery` |
|  [05]   | write posture     | `WriteRelation` → `WriteRejected`                  | fail-closed; federation reads, the store rail writes         |
|  [06]   | default cut       | `Option<TimeCut>` resolved INSIDE `Execute`        | `HeadSequence` head under the clock `Hlc`; never ambient now |
|  [07]   | replay identity   | `(digest·cut·watermark·source·mode)` → `ReplayKey` | bindings and materialized views remain distinct              |
|  [08]   | receipt validity  | `IValidityEvidence` + `ValidityClaim.All`          | the kernel [C] floor; never a hand-rolled `&&` chain         |
|  [09]   | streaming cadence | `Mode.Materialized(View, Keys)` case dispatch      | one plan IR, one entry; never a sibling execution surface    |

## [04]-[FLIGHT_RESULT_PLANE]

- Owner: `FederationFlight` the `Apache.Arrow.Flight.Server` `FlightServer` subclass — the result half of the plan wire: a portable plan flows in through `#PLAN_INGRESS` and its batches flow back through zero-copy Arrow record streams; the ticket registry is one constructor-injected `Atom<HashMap<UInt128, FederatedResult>>` hold keyed by `ReplayKey`.
- Cases: `GetFlightInfo` takes a COMMAND descriptor whose `Command` bytes are the protobuf plan wire — it admits through `FederationPlan.Admit(new PlanWire.Protobuf(...), source, new FederationMode.OneShot())`, executes through the ONE `Federation.Execute`, holds the result under its `ReplayKey`, and answers a `FlightInfo` carrying the result schema, ONE `FlightEndpoint` whose `FlightTicket` is the big-endian `ReplayKey` bytes, the honest `TotalRecords`, and `TotalBytes` `-1` — the held batches carry no serialized-byte figure, so the Flight unknown sentinel is the honest claim, never a fabricated total; `DoGet` redeems the 16-byte ticket against the hold and streams every batch through `FlightServerRecordBatchStreamWriter.WriteAsync` (the first write auto-emits the schema message); an unknown or expired ticket rails `FederationFault.TicketUnknown`.
- Entry: `public override Task<FlightInfo> GetFlightInfo(FlightDescriptor descriptor, ServerCallContext context)` admits the command plan and mints a `ReplayKey` ticket; `public override Task DoGet(FlightTicket ticket, FlightServerRecordBatchStreamWriter responseStream, ServerCallContext context)` redeems that ticket. Every other base verb keeps its base throw because this plane is a read-only result producer.
- Auto: the ticket is the content-addressed result identity — `ReplayKey` frames `(plan-digest·full-cut·watermark·source·mode)`, so a byte-identical plan re-described at the same cut redeems the same ticket; a keyed result projects through the ONE `Query/residence#COLUMN_VOCABULARY` `ArrowLanding.Build` fold over the declared `KeyProjection` schema, so the batch, its field order, and its metadata all derive from one declaration; the hold is an idempotent `Atom` swap whose eviction rides the `Query/cache` reuse cadence; every typed refusal leaves the rail at the platform-forced verb edge through the AppHost `FaultWire.Raise(fault, context)` — the ONE producer fold packing `FaultDetail` beside the status, so a Flight consumer reads numeric identity where a status-plus-message once carried a string alone.
- Law: the keyed projection declares MODEL beside id and rides the one landing fold. NAMED LOSS: none — the single-`id` `RecordBatch.Builder` assembly it replaces reached for a `SetKey.Value` member that does not exist, so it ships bare node ids no consumer resolves back to a model, beside a second hand-built `Schema` that agrees with the batch only by inspection and carries no metadata seat at all. WITNESS: `KeyProjection` declares `(model, id, at)`, `SetKey` supplies the first two, `TimeSpine.Landing` obliges the third, and `Facts` rides the metadata the fold requires — plan digest, replay key, source, and stamp, none of which a redeemed batch previously carried.
- Receipt: a described plan rides `store.federation.flight.describe` carrying the digest and the minted ticket; a redeemed stream rides `store.federation.flight.stream` carrying the ticket, the batch count, and the drained rows.
- Packages: Apache.Arrow.Flight (`FlightServer`/`FlightDescriptor`/`FlightTicket`/`FlightInfo`/`FlightEndpoint`/`FlightServerRecordBatchStreamWriter`), Apache.Arrow.Flight.AspNetCore (`IGrpcServerBuilder.AddFlightServer<T>() where T : FlightServer`/`IEndpointRouteBuilder.MapFlightEndpoint()` — the composition-root binding pair, this package the sole holder of the server-adapter grant), Apache.Arrow (`RecordBatch` — the fold's own output; no builder crosses this page), Rasm.Persistence (`Query/residence#COLUMN_VOCABULARY` `AnalyticsSchema`/`ColumnRow`/`ColumnType`/`ColumnCell`/`TimeSpine`/`ArrowLanding.Build`, `Element/graph#STORE_RAIL` `ProjectionContext` — the frame the refusal context reads), Rasm.AppHost (`Runtime/ports#WIRE_LAW` `FaultWire.Raise`/`FaultContext` — the one producer fold), Rasm.Contracts (`Clock.Hlc` — the stamp the context carries), Rasm (`Rasm.Domain` `ContentHash.Wire`/`Admit` — the ticket byte correspondence), Google.Protobuf (`ByteString`), Grpc.Core (`ServerCallContext`/`RpcException`), LanguageExt.Core, BCL inbox.
- Growth: a new result consumer dials the host channel and redeems tickets; a new served identity axis is one `ReplayKey` preimage field; a discovery need is the `ListFlights` verb over the same hold. One held result serves every consumer through that ticket — a bespoke file drop, a second result wire, a session-keyed ticket, or a `DoPut` ingest arm is the deleted form.
- Boundary: the SERVER half is this package's and the MOUNT is AppHost's — `FederationFlight : FlightServer` is the whole Persistence contribution, bound at the composition root by `services.AddGrpc().AddFlightServer<FederationFlight>()` and served by the NON-GENERIC `app.MapFlightEndpoint()`, with the gRPC channel, TLS, and credentials AppHost's throughout. Those two calls arrive as ONE `Rasm.AppHost/Wire/companion#SERVICE_HOST` served-plane row the shell supplies, so an armed registration and an unmapped endpoint cannot drift apart; neither this package nor the spine names the other, since the shell is the only tier reaching both and an unsupplied row leaves the host serving control and health alone rather than degrading. `FlightServer` is NOT itself a gRPC service: no `[BindServiceMethod]` sits anywhere in its hierarchy, so `MapGrpcService<FederationFlight>()` resolves no binder and fails at startup, and the subclass reaches gRPC only DI-resolved AS `FlightServer` into the transport package's internal `FlightService.FlightServiceBase` adapter — reachable through the `Apache.Arrow.Flight.AspNetCore` `InternalsVisibleTo` grant alone (`api-arrow-egress#IMPLEMENTATION_LAW`). `DoGet` streams held batches, never a live `QueryResult`; `DoPut` and `DoExchange` keep their base throws because this plane is the lake's READ end and a Flight landing door forks the `Query/lakehouse#FLAT_TABLE_EGRESS` write custody; the serving window bounds memory, an evicted result re-executes, and `Authority.Admit` gates demand at the caller.
- Boundary: `FlightSqlServer` is the DECLINED base and stays declined — it dispatches Flight SQL command messages alone (`CommandStatementQuery` carrying SQL text, the eleven catalog-metadata commands, the prepared-statement pair) and its `GetCommand`/`GetFlightInfo`/`DoGet` fold matches `CommandStatementSubstraitPlan` nowhere, even though the generated protocol declares it beside `SubstraitPlan { Plan = 1, Version = 2 }`. Subclassing it therefore obligates 28 protected abstract handlers with no base implementations, every one a SQL-catalog verb this read-only plane answers with nothing, and STILL demands a `GetFlightInfo` override to reach the plan command — a plain `FlightServer` carrying a command descriptor is the same wire at a fraction of the surface, and its nine `virtual` verbs let a read-only plane override the two it serves and inherit the rest as refusals.

```csharp signature
using Apache.Arrow.Flight;
using Apache.Arrow.Flight.Server;
using Apache.Arrow.Types;
using Google.Protobuf;
using Grpc.Core;
using Rasm.AppHost.Runtime;                        // FaultWire/FaultContext — the ONE producer fault fold (ports#WIRE_LAW)
using Rasm.Domain;                                 // ContentHash.Wire/Admit — the 16-byte ticket correspondence
using Rasm.Persistence.Element;                    // ProjectionContext — correlation, tenant, and clock as kernel values

namespace Rasm.Persistence.Query;

// --- [SERVICES] -----------------------------------------------------------------------------
// result half of the federation wire: plans in through PLAN_INGRESS, batches out through DoGet.
// Composition registers this type through `AddFlightServer<FederationFlight>()` and never
// `MapGrpcService<FederationFlight>()` — `FlightServer` carries no bind attribute, so that generic map
// finds no binder and fails at startup; ctor dependencies still resolve because the internal adapter
// takes `FlightServer` by injection.
public sealed class FederationFlight(FederationPorts ports, SourceKind source, ProjectionContext frame, Atom<HashMap<UInt128, FederatedResult>> hold) : FlightServer {
    public override Task<FlightInfo> GetFlightInfo(FlightDescriptor descriptor, ServerCallContext context) =>
        Describe(new PlanWire.Protobuf(descriptor.Command.Memory), descriptor, source, ports, frame, hold);

    // ONE admit-execute-hold-describe chain owns command admission and ticket minting. The verb edge is the one
    // place a typed fault leaves the rail, because every `FlightServer` base verb returns `Task<T>` with no rail
    // to fail on — and it leaves through `FaultWire.Raise`, so the status derives from the fault's own case on the
    // AppHost producer table and a `FaultDetail` packs beside it; a hand status switch here was the second table.
    internal static async Task<FlightInfo> Describe(PlanWire wire, FlightDescriptor descriptor, SourceKind source, FederationPorts ports, ProjectionContext frame, Atom<HashMap<UInt128, FederatedResult>> hold) {
        Fin<FederatedResult> described = await FederationPlan
            .Admit(wire, source, new FederationMode.OneShot())
            .Match(
                Succ: plan => Federation.Execute(plan, None, ports),
                Fail: fault => IO.pure(Fin<FederatedResult>.Fail(fault)))
            .RunAsync().ConfigureAwait(false);
        return described.Bind(result => Batches(result).Map(batches => {
            _ = hold.Swap(held => held.AddOrUpdate(result.ReplayKey, result));
            return new FlightInfo(
                batches.Head.Match(Some: static b => b.Schema, None: () => KeyProjection.Fields(Facts(result))),
                descriptor,
                [new FlightEndpoint(new FlightTicket(ContentHash.Wire(result.ReplayKey)), [])],
                batches.Sum(static b => (long)b.Length),
                -1L);
        })).Match(Succ: static info => info, Fail: fault => throw FaultWire.Raise(fault, Context(frame)));
    }

    public override Task DoGet(FlightTicket ticket, FlightServerRecordBatchStreamWriter responseStream, ServerCallContext context) =>
        Redeem(ticket, responseStream, frame, hold);

    // ONE ticket-redemption body: `ContentHash.Admit` is the one 16-byte big-endian decode and its refusal re-keys
    // onto `TicketMalformed` carrying the width the caller sent — FlightTicket is an opaque caller-controlled token,
    // so a wrong-width payload stays distinct from an unredeemable one.
    internal static async Task Redeem(FlightTicket ticket, FlightServerRecordBatchStreamWriter responseStream, ProjectionContext frame, Atom<HashMap<UInt128, FederatedResult>> hold) {
        Fin<Seq<RecordBatch>> held =
            from key in ContentHash.Admit(ticket.Ticket.Span, Op.Of()).MapFail(_ => (Error)new FederationFault.TicketMalformed(ticket.Ticket.Length))
            from result in hold.Value.Find(key).ToFin(new FederationFault.TicketUnknown(key))
            from batches in Batches(result)
            select batches;
        foreach (RecordBatch batch in held.Match(Succ: static batches => batches, Fail: fault => throw FaultWire.Raise(fault, Context(frame)))) {
            await responseStream.WriteAsync(batch).ConfigureAwait(false);
        }
    }

    // The refusal context is the frame's own causal pair as kernel values — correlation and the partition-aware tenant
    // read — with the stamp a fresh physical advance at raise (logical zero IS the HLC law for a physical step) and an
    // empty violations run, since a Flight refusal is a typed case, never a field-level admission report.
    static FaultContext Context(ProjectionContext frame) =>
        new(frame.Correlation,
            new Rasm.Contracts.Clock.Hlc { Physical = frame.Now().ToUnixTimeTicks(), Logical = 0UL },
            frame.Tenant.Key.Map(_ => frame.Tenant.TenantId),
            Seq<Google.Rpc.BadRequest.Types.FieldViolation>());

    // `KeyProjection` is the declared shape of a keyed federation result, so the batch rides the ONE
    // `ArrowLanding.Build` fold every columnar landing takes rather than a `RecordBatch.Builder` assembly beside
    // a second hand-built `Schema` that agreed with it only by inspection. It declares MODEL beside id because a
    // federated selection spans models and the deleted single-`id` projection reached for a `SetKey.Value`
    // member that does not exist — dropping the model qualification the whole `SetKey` axis carries. `at` is the
    // landing stamp `TimeSpine.Landing` obliges, so a redeemed batch dates itself.
    static readonly AnalyticsSchema KeyProjection = new(
        Dataset: "federation.keys",
        Key: Seq(Identifier.Create("model"), Identifier.Create("id")),
        Columns: Seq(
            new ColumnRow(Identifier.Create("model"), ColumnType.Utf8, Nullable: false),
            new ColumnRow(Identifier.Create("id"), ColumnType.Utf8, Nullable: false),
            new ColumnRow(Identifier.Create("at"), ColumnType.Timestamp, Nullable: false)),
        Time: Identifier.Create("at"),
        Spine: TimeSpine.Landing,
        Measure: None);

    // Metadata is REQUIRED on the fold and never defaulted: `Schema.Builder` and `RecordBatch.Builder` expose no
    // metadata seat at all, which is why the deleted assembly could carry none — a redeemed batch reached its
    // consumer stating no plan, no cut, and no ticket. These four facts are the receipt the ticket redeems.
    static Seq<(string Key, string Value)> Facts(FederatedResult result) => Seq(
        ("plan_digest", result.PlanDigest.ToString("x32", CultureInfo.InvariantCulture)),
        ("replay_key", result.ReplayKey.ToString("x32", CultureInfo.InvariantCulture)),
        ("source", result.Source.Identity),
        ("at", result.At.ToString()));

    // `Batches` streams a tabular result as its drained batches and a keyed result as ONE declared projection.
    static Fin<Seq<RecordBatch>> Batches(FederatedResult result) =>
        result.Batch.Match(
            Some: Fin.Succ,
            None: () => ArrowLanding.Build(KeyProjection, result.Keys.Keys,
                    key => Seq<ColumnCell>(
                        new ColumnCell.Text(key.Model.Value.ToString("D", CultureInfo.InvariantCulture)),
                        new ColumnCell.Text(key.Node.Value),
                        new ColumnCell.Moment(result.At)),
                    Facts(result))
                .Map(static batch => Seq(batch)));
}

```

| [INDEX] | [POLICY]         | [VALUE]                                      | [BINDING]                                                     |
| :-----: | :--------------- | :------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | ticket identity  | `ContentHash.Wire(ReplayKey)` / `Admit`      | content-addressed; a re-described identical plan re-redeems   |
|  [02]   | verbs            | `GetFlightInfo` + `DoGet` only               | read-only result plane; `DoPut`/`DoExchange` stay base throws |
|  [03]   | keyed projection | `ArrowLanding.Build` over `KeyProjection`    | `(model, id, at)` declared once; metadata carries the receipt |
|  [04]   | hosting          | `AddFlightServer<T>` + `MapFlightEndpoint()` | AppHost mounts; `MapGrpcService<T>` fails at startup          |
|  [05]   | hold             | `Atom<HashMap<UInt128, FederatedResult>>`    | one serving window; eviction re-executes                      |
|  [06]   | refusal          | `FaultWire.Raise(fault, Context(frame))`     | AppHost producer table; `FaultDetail` packs beside the status |

## [05]-[PLAN_WIRE_SKEW]

- Owner: the frozen `SubstraitPlan` edge to `python:data`, whose two ends parse INCOMPATIBLE extension schemas at the pinned versions and agree on every other field.
- Cases: this producer's generated `Substrait.Protobuf.Plan` writes `ExtensionUris` at field 1 with each declaration back-referencing through `ExtensionUriReference` at field 1; the consumer's installed distribution reads `extension_urns` at field 8 with `extension_urn_reference` at field 4 and declares neither retired field.
- Auto: proto3 files an unknown field rather than raising at BOTH doors here — the binary parser retains unknown fields by default and `PlanJson` ignores them by declaration — so a plan minted here parses CLEAN across the edge — `relations`, `extensions`, `version`, and `advanced_extensions` all survive while the whole extension space vanishes into the unknown set, presenting as a plan declaring functions against no space at all.
- Boundary: the consumer refuses that signature on its own `RETIRED_EXTENSION_SCHEMA` row ahead of urn resolution, so the skew fails loudly at one named seam; without it the resolution check iterates an empty list, admits vacuously, and drops every function-vocabulary lineage edge the receipt was meant to carry.
- Growth: parity arrives when this package's generated protobuf carries the URN-era schema — no released version does, so the refusal row is the standing form and a bump is what retires it, never a local re-encode inventing anchors this IR never held.


## [06]-[RESEARCH]

(none)
