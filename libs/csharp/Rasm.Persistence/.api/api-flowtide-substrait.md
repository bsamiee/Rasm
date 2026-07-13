# [RASM_PERSISTENCE_API_FLOWTIDE_SUBSTRAIT]

`FlowtideDotNet.Substrait` is the Substrait portable query-plan IR for `.NET`: a cross-backend relational-algebra plan model (`Plan` over a `List<Relation>` of `ReadRelation`/`FilterRelation`/`ProjectRelation`/`JoinRelation`/`AggregateRelation`/`SortRelation`/`WriteRelation`/... with a typed `Expression` tree, a `SubstraitBaseType` system, and the standard Substrait function-extension URI catalogs), an `Sql.SqlPlanBuilder` front-end that parses SQL text into a `Plan` via the admitted `SqlParserCS` (sqlparser-rs port) under the `FlowtideDialect`, a `PlanModifier` for composing views/subplans/multi-root plans, an `ITableProvider`/`ISqlFunctionRegister` extension seam for custom tables and functions, and a `RelationVisitor<TReturn, TState>`/`ExpressionVisitor<TOutput, TState>` double-dispatch fold over the plan tree. It is the cross-backend query-plan owner backing the `Query/federation` rail — not a store or transport — and the `Conversion.SubstraitToDifferentialCompute.Convert` static bridge lowers a `Plan` into the FlowtideDotNet streaming differential-compute engine. It consumes its bundled `SqlParserCS` transitive (the sqlparser-rs port — the SQL AST it lowers) as its text front-end and emits the vendor-neutral plan that the federation rail dispatches across DuckDB (`api-duckdb`), ClickHouse (`api-clickhouse`), and Delta (`api-deltalake`) backends.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FlowtideDotNet.Substrait`
- package: `FlowtideDotNet.Substrait`
- license: Apache-2.0 (the nuspec omits an SPDX `<license>` expression; the FlowtideDotNet project is Apache-2.0 — a port of the Substrait spec + sqlparser-rs)
- assembly: `FlowtideDotNet.Substrait`
- namespace: `FlowtideDotNet.Substrait` (`Plan`, `PlanModifier`), `FlowtideDotNet.Substrait.Relations` (the relational-algebra rel tree + `RelationVisitor`), `FlowtideDotNet.Substrait.Expressions` (+ `.Literals`, `.IfThen`), `FlowtideDotNet.Substrait.Type` (the type system), `FlowtideDotNet.Substrait.FunctionExtensions` (the standard function URI catalogs), `FlowtideDotNet.Substrait.Sql` (the SQL-text front-end), `FlowtideDotNet.Substrait.Conversion`, `FlowtideDotNet.Substrait.Hints`, `FlowtideDotNet.Substrait.Exceptions`
- target: multi-target (`net10.0`, `net8.0`); the `net10.0` consumer binds `lib/net10.0` (the bound asset)
- native: pure-managed (no `runtimes/<rid>/native` payload)
- transitive: `SqlParserCS@0.6.5` (the sqlparser-rs port — the SQL AST the `SqlPlanBuilder` lowers; floor-pinned in the manifest transitive floors), `Google.Protobuf` (nuspec floor `3.34.0`; the substrate pins `Google.Protobuf@3.35.1` higher and is what resolves — the Substrait protobuf wire), `Microsoft.Extensions.Logging.Abstractions@10.0.2`, `Microsoft.Extensions.DependencyInjection.Abstractions@10.0.2`, `Microsoft.Extensions.ObjectPool@10.0.2`
- xml docs: absent (member intent is decompile-sourced)
- rail: query-plan

This is a plan-IR library: it produces and transforms a vendor-neutral `Plan` object, not a store connection or a wire client. The INBOUND wire path is PUBLIC: the generated `Substrait.Protobuf.Plan.Parser` (a `Google.Protobuf.MessageParser<Plan>`) decodes protobuf bytes, and the public `SubstraitDeserializer` lifts either that protobuf `Plan` or a Substrait-JSON string into the managed `FlowtideDotNet.Substrait.Plan` IR. The OUTBOUND `SubstraitSerializer` is `internal` — a managed `Plan` cannot be re-lowered to the protobuf wire, so the original protobuf bytes are RETAINED (`IMessage.ToByteArray()`/`WriteTo`) for round-trip, never re-serialized from the managed IR. The consumer composes the `Plan` model, the `SqlPlanBuilder`, the `SubstraitDeserializer` inbound bridge, the visitor double-dispatch, and the `SubstraitToDifferentialCompute` bridge.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plan, modifier, and conversion
- rail: query-plan

`Plan` is the top-level IR (a `List<Relation>`; sealed, value-equatable). `PlanModifier` composes plans — registers subplans as named views, stacks root plans, and re-binds relation references across them via `Modify()`. `SubstraitToDifferentialCompute` is the public static bridge into the FlowtideDotNet streaming engine. `Substrait.Protobuf.Plan.Parser` is a `MessageParser<Plan>` with `ParseFrom(byte[]/ReadOnlySpan<byte>/CodedInputStream)`; `SubstraitDeserializer.Deserialize` takes a protobuf `Plan` or JSON string, `DeserializeFromJson(string)` the static form.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]        | [RAIL]                                                           |
| :-----: | :------------------------------------------ | :------------------- | :--------------------------------------------------------------- |
|  [01]   | `Plan`                                      | plan root            | sealed; `List<Relation> Relations`; value equality               |
|  [02]   | `PlanModifier`                              | plan composer        | `AddPlanAsView`/`AddRootPlan`/`Modify()`                         |
|  [03]   | `SubstraitDeserializer`                     | wire → IR bridge     | PUBLIC; `Deserialize(protobufPlan\|json)`                        |
|  [04]   | `Substrait.Protobuf.Plan`                   | protobuf message     | generated `IMessage`; `Parser`, `WriteTo`/`ToByteArray`          |
|  [05]   | `SubstraitSerializer`                       | IR → wire (internal) | `internal`; no managed→protobuf lowering, retain inbound bytes   |
|  [06]   | `Conversion.SubstraitToDifferentialCompute` | engine bridge        | static `Convert(Plan, addWriteRelation, tableName, primaryKeys)` |
|  [07]   | `Hints.Hint` / `Hints.HintOptimizations`    | plan hint            | `Alias` + optimization flags on each `Relation`                  |
|  [08]   | `Exceptions.SubstraitParseException`        | parse failure        | thrown by the SQL/plan front-end                                 |

[PUBLIC_TYPE_SCOPE]: relations (the relational-algebra tree)
- rail: query-plan

`Relation` is the abstract base: `Emit` (column projection), `OutputLength`, `Hint`, and `Accept<TReturn, TState>(RelationVisitor<TReturn, TState>, state)` for double-dispatch. Each concrete relation is sealed and value-equatable, with `required` inputs. Every `[SYMBOL]` sits in the `Relations` namespace, shown bare.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [RAIL]                                                     |
| :-----: | :------------------------------------------------ | :------------ | :--------------------------------------------------------- |
|  [01]   | `Relation`                                        | rel base      | abstract; `Emit`/`OutputLength`/`Hint`/`Accept`            |
|  [02]   | `RelationVisitor<TReturn, TState>`                | rel visitor   | `Visit` + per-relation `Visit*` double-dispatch            |
|  [03]   | `ReadRelation`                                    | source rel    | `required BaseSchema`/`NamedTable`, `Filter?`              |
|  [04]   | `FilterRelation`                                  | filter rel    | predicate over an input                                    |
|  [05]   | `ProjectRelation`                                 | project rel   | computed expression columns                                |
|  [06]   | `JoinRelation` / `MergeJoinRelation`              | join rel      | `JoinType`, `Left`/`Right`, `Expression`, `PostJoinFilter` |
|  [07]   | `AggregateRelation`                               | aggregate rel | `AggregateGrouping`/`AggregateMeasure` groupings           |
|  [08]   | `SortRelation` / `TopNRelation` / `FetchRelation` | order rel     | sort fields, top-N, limit/offset                           |
|  [09]   | `SetRelation`                                     | set rel       | `SetOperation` (union/intersect/except)                    |
|  [10]   | `WriteRelation`                                   | sink rel      | `required Input`/`TableSchema`/`NamedObject`, `Overwrite`  |
|  [11]   | `RootRelation`                                    | root rel      | `required Input`, `Names` (output column names)            |
|  [12]   | `ReferenceRelation`                               | reference rel | cross-subplan reference (view binding)                     |
|  [13]   | `ConsistentPartitionWindowRelation`               | window rel    | partitioned window functions                               |
|  [14]   | `ExchangeRelation`                                | rel           | shuffle-partition relation                                 |
|  [15]   | `TableFunctionRelation`                           | rel           | table-function relation                                    |
|  [16]   | `VirtualTableReadRelation`                        | rel           | inline literal-table relation                              |
|  [17]   | `IterationRelation`                               | rel           | recursive iteration relation                               |
|  [18]   | `JoinType` / `SetOperation` / `ExchangeKindType`  | rel enum      | join kind, set op, exchange partitioning                   |

[PUBLIC_TYPE_SCOPE]: expressions and the type system
- rail: query-plan

`Expression` is the abstract expression base (`Accept<TOutput, TState>(ExpressionVisitor<TOutput, TState>, state)`); `SubstraitBaseType` roots the type lattice over the `SubstraitType` enum. Scalar/aggregate/window functions reference the standard function-extension URIs. Every expression `[SYMBOL]` sits in `Expressions` (literals in `Expressions.Literals`, if/then in `Expressions.IfThen`), shown bare; `WindowBound` subtypes are `Preceeding`/`Following`/`CurrentRow`/`Unbounded`; `Literal` variants are `Bool`/`Numeric`/`String`/`Binary`/`Array`/`Null`.

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY]  | [RAIL]                                           |
| :-----: | :--------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `Expression`                                         | expr base      | abstract; `Accept<TOutput, TState>`              |
|  [02]   | `ExpressionVisitor<TOutput, TState>`                 | expr visitor   | double-dispatch fold over the expression tree    |
|  [03]   | `ScalarFunction` / `AggregateFunction`               | function expr  | `ExtensionUri` + `ExtensionName` + arguments     |
|  [04]   | `WindowFunction`                                     | function expr  | windowed `ExtensionUri` + `ExtensionName`        |
|  [05]   | `FieldReference` / `DirectFieldReference`            | reference expr | column field access                              |
|  [06]   | `ReferenceSegment` / `StructReferenceSegment`        | reference expr | struct field-access segment                      |
|  [07]   | `MapKeyReferenceSegment`                             | reference expr | map field-access segment                         |
|  [08]   | `CastExpression` / `IfThen.IfThenExpression`         | control expr   | cast, if/then                                    |
|  [09]   | `SingularOrListExpression` / `MultiOrListExpression` | control expr   | IN-list                                          |
|  [10]   | `NestedExpression` / `ListNestedExpression`          | nested expr    | nested/list constructors                         |
|  [11]   | `MapNestedExpression` / `StructExpression`           | nested expr    | map/struct constructors                          |
|  [12]   | `SortField` / `SortDirection`                        | order          | sort key + direction                             |
|  [13]   | `WindowBound`                                        | window         | frame bound (subtypes in lead)                   |
|  [14]   | `Literals.Literal`                                   | literal expr   | typed constants; `LiteralType`, variants in lead |

[PUBLIC_TYPE_SCOPE]: the type system — `FlowtideDotNet.Substrait.Type`, prefix `Type.` hoisted. The `SubstraitType` enum values are `String`/`Int32`/`Fp64`/`Decimal`/`Struct`/`Map`/`List`/`Binary`/`TimestampTz`/...
- rail: query-plan

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY] | [RAIL]                                   |
| :-----: | :----------------------------------------------- | :------------ | :--------------------------------------- |
|  [01]   | `SubstraitBaseType` / `SubstraitType`            | type base     | abstract `Type`/`Nullable`; enum in lead |
|  [02]   | `NamedStruct` / `Struct`                         | row type      | `Names` + `Struct` of column types       |
|  [03]   | `NamedTable` / `VirtualTable`                    | table ref     | qualified name / inline literal rows     |
|  [04]   | `{Int32,Int64,Fp32,Fp64,Bool,String,Binary}Type` | scalar leaf   | concrete `SubstraitBaseType` leaves      |
|  [05]   | `{Date,Timestamp,Decimal,List,Map,Any,Null}Type` | compound leaf | concrete `SubstraitBaseType` leaves      |

[PUBLIC_TYPE_SCOPE]: SQL front-end and function-extension catalogs
- rail: query-plan

`SqlPlanBuilder` is the SQL-text → `Plan` front-end (parses via `SqlParserCS` under `FlowtideDialect`); `ITableProvider`/`ISqlFunctionRegister` are the extension seams. Every `[SYMBOL]` here sits in `FlowtideDotNet.Substrait.Sql`, prefix `Sql.` hoisted; the builder and register verbs are rostered in `[ENTRYPOINTS]`.

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY]       | [RAIL]                                            |
| :-----: | :------------------------------------------------------------- | :------------------ | :------------------------------------------------ |
|  [01]   | `SqlPlanBuilder`                                               | SQL front-end       | SQL→`Plan` builder; verbs in `[ENTRYPOINTS]`      |
|  [02]   | `ITableProvider`                                               | table seam          | `TryGetTableInformation`/`TryHandleTableFunction` |
|  [03]   | `ISqlFunctionRegister`                                         | function seam       | custom scalar/aggregate/table registration        |
|  [04]   | `TableMetadata`                                                | table seam data     | table name + `NamedStruct` schema                 |
|  [05]   | `TableProviderTableFunctionResult`                             | table seam data     | table-function relation result                    |
|  [06]   | `TableProviderTableFunctionArguments`                          | table seam data     | table-function call arguments                     |
|  [07]   | `SqlExpressionVisitor` / `BaseExpressionVisitor<TOut, TState>` | SQL expr visitor    | the SQL expression lowering fold                  |
|  [08]   | `ScalarResponse` / `AggregateResponse` / `WindowResponse`      | registration result | the relation/expr a registered function emits     |

[PUBLIC_TYPE_SCOPE]: function-extension URI catalogs — `FlowtideDotNet.Substrait.FunctionExtensions`, prefix `FunctionExtensions.` hoisted. Each `Functions*` class is one standard Substrait catalog: a `Uri` const plus the function-name consts.
- rail: query-plan

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [RAIL]                                                          |
| :-----: | :-------------------------------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `FunctionsArithmetic`                               | function URIs | `Add`/`Subtract`/`Multiply`/`Sum`/`Min`/`Max`/`RowNumber`/...   |
|  [02]   | `FunctionsComparison`                               | function URIs | `Equal`/`GreaterThan`/`Between`/`Coalesce`/`IsNull`/`IsNan`/... |
|  [03]   | `FunctionsString`                                   | function URIs | `Concat`/`Lower`/`Upper`/`Substring`/`Like`/`StrPos`/...        |
|  [04]   | `Functions{AggregateGeneric,Boolean,Datetime,List}` | function URIs | count/bool/datetime/list catalogs                               |
|  [05]   | `Functions{Rounding,Hash,Guid,Struct}`              | function URIs | rounding/hash/guid/struct catalogs                              |
|  [06]   | `Functions{Check,Logarithmic,TableGeneric}`         | function URIs | check/logarithmic/table-function catalogs                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build a plan from SQL
- rail: query-plan

`SqlPlanBuilder` registers table schemas/providers and accumulates one or more SQL statements, then `GetPlan()` runs the `PlanModifier` to bind references and yield a single composed `Plan`. The default function register pre-loads the built-in Substrait functions.

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :--------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `new SqlPlanBuilder()`                         | ctor           | loads `BuiltInSqlFunctions` into the register  |
|  [02]   | `AddTableDefinition(name, NamedStruct schema)` | schema decl    | declares a queryable table + its row type      |
|  [03]   | `AddTableProvider(ITableProvider)`             | table seam     | dynamic table/table-function resolution        |
|  [04]   | `AddPlanAsView(viewName, Plan)`                | view decl      | registers a prior plan as a named view         |
|  [05]   | `Sql(sqlText)`                                 | parse          | parses + lowers SQL into the accumulating plan |
|  [06]   | `GetPlan()`                                    | finalize       | runs `PlanModifier.Modify()` → composed `Plan` |
|  [07]   | `FunctionRegister` (property)                  | function seam  | `ISqlFunctionRegister` for custom functions    |

[ENTRYPOINT_SCOPE]: compose, register, and convert. Rows [02]-[04] hoist the `ISqlFunctionRegister.` prefix; [01] is plan compose, [05] the table seam, [06] the engine bridge, [07]-[08] wire/JSON → IR.
- rail: query-plan

| [INDEX] | [SURFACE]                                                                                  | [RAIL]                                   |
| :-----: | :----------------------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `new PlanModifier()` + `.AddRootPlan(Plan)` / `.AddPlanAsView(name, Plan)` / `.Modify()`   | stack roots + views, re-bind references  |
|  [02]   | `RegisterScalarFunction(name, Func<…, ScalarResponse>)`                                    | custom scalar function lowering          |
|  [03]   | `RegisterAggregateFunction(name, Func<…, AggregateResponse>)`                              | custom aggregate lowering                |
|  [04]   | `RegisterTableFunction(name, Func<SqlTableFunctionArgument, TableFunction>)`               | custom table-function lowering           |
|  [05]   | `ITableProvider.TryGetTableInformation(name, out TableMetadata)`                           | resolve a table's `NamedStruct` schema   |
|  [06]   | `SubstraitToDifferentialCompute.Convert(Plan, addWriteRelation, tableName, primaryKeys)`   | lower a `Plan` into differential-compute |
|  [07]   | `new SubstraitDeserializer().Deserialize(Substrait.Protobuf.Plan.Parser.ParseFrom(bytes))` | decode a foreign protobuf plan → `Plan`  |
|  [08]   | `SubstraitDeserializer.DeserializeFromJson(json)` / `.Deserialize(json)`                   | lift a Substrait-JSON plan → `Plan`      |

[ENTRYPOINT_SCOPE]: traverse and build the plan tree directly
- rail: query-plan

The plan IR is buildable and foldable in-memory without SQL: construct `Relation`/`Expression` nodes directly, and traverse via a `RelationVisitor<TReturn, TState>`/`ExpressionVisitor<TOutput, TState>` subclass that overrides the per-node `Visit*` methods.

| [INDEX] | [SURFACE]                                                                                  | [RAIL]                                      |
| :-----: | :----------------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `new ReadRelation { BaseSchema = …, NamedTable = …, Filter = … }`                          | a source relation with row type + predicate |
|  [02]   | `new Plan { Relations = [ rootRelation ] }`                                                | a plan from explicit relations              |
|  [03]   | `relation.Accept(visitor, state)` / `visitor.Visit(relation, state)`                       | double-dispatch over the relation tree      |
|  [04]   | `class MyVisitor : RelationVisitor<TReturn, TState> { override VisitJoinRelation(...) }`   | per-relation transform/extract fold         |
|  [05]   | `expression.Accept(exprVisitor, state)`                                                    | double-dispatch over the expression tree    |
|  [06]   | `new ScalarFunction { ExtensionUri = FunctionsArithmetic.Uri, ExtensionName = …Add, ... }` | a scalar function by standard URI           |
|  [07]   | `relation.Emit` / `relation.Hint`                                                          | output-column projection + optimizer hint   |

## [04]-[IMPLEMENTATION_LAW]

[SUBSTRAIT_TOPOLOGY]:
- this is a query-plan IR, not a store or transport: the deliverable is a vendor-neutral `Plan` (a `List<Relation>` forming a relational-algebra DAG with a typed `Expression` tree). There is no connection, no wire client.
- SQL front-end: `SqlPlanBuilder.Sql(text)` parses through the bundled `SqlParserCS` transitive (the sqlparser-rs port) under the `FlowtideDialect` with a raised recursion limit, then `SqlSubstraitVisitor` lowers the SQL AST to relations. `GetPlan()` runs `PlanModifier.Modify()` to bind cross-statement view references.
- type system: `SubstraitBaseType` over the `SubstraitType` enum is the column/value type lattice; `NamedStruct` (names + `Struct` of types) is the row type carried by `ReadRelation.BaseSchema` and `WriteRelation.TableSchema`.
- function model: functions are referenced by standard Substrait extension URI + name — the `Functions*` catalogs (`FunctionsArithmetic.Uri = "/functions_arithmetic.yaml"`, `FunctionsArithmetic.Add = "add"`, ...) are the canonical URI/name constants, so a `ScalarFunction`/`AggregateFunction` never hard-codes a magic string.
[TRAVERSAL]:
Every `Relation`/`Expression` is double-dispatch-folded via `Accept<TReturn, TState>` against a `RelationVisitor`/`ExpressionVisitor`; a transform or extraction subclasses the visitor and overrides the relevant `Visit*` methods. `RelationVisitor` throws `NotImplementedException` for unhandled relation kinds, while `ExpressionVisitor` returns `default(TOutput)` for every unhandled expression kind, so expression pushdown reads null as the graceful "inexpressible" signal (`Query/federation#PredicateLowering`), never a throw.
- composition: `PlanModifier` is the multi-plan composer — `AddPlanAsView` registers a subplan as a named view (re-bound to a `ReferenceRelation`), `AddRootPlan` stacks root plans, and `Modify` re-maps relation ids across them into one `Plan`. (`PlanModifier.WriteToTable` is `[Obsolete]` — use SQL inserts.)
- protobuf (INBOUND public, OUTBOUND internal): decompile of the `0.15.0` assembly falsifies the prior "serialization is internal" claim — `Substrait.Protobuf.Plan.Parser.ParseFrom(bytes)` decodes the wire message and the PUBLIC `SubstraitDeserializer.Deserialize(protobufPlan)` / `Deserialize(string json)` / static `DeserializeFromJson(string)` lift it to the managed `Plan`, so a foreign Substrait producer's wire plan enters the federation rail directly. Only `SubstraitSerializer` (managed `Plan` → protobuf) is `internal`, so the managed IR is NOT re-lowered to bytes — retain the inbound protobuf bytes (`Plan.ToByteArray()`/`WriteTo`) when a round-trip is needed, and traverse the managed IR via the visitor fold for dispatch.

[LOCAL_ADMISSION]:
- Substrait enters as the cross-backend query-plan IR behind the `Query/federation` rail — the vendor-neutral plan a federation rail dispatches to a concrete backend, not a store the rail connects to. A federation `Plan` lowers onto `Query/federation#FEDERATED_PLAN`, which walks it to `Query/lane#SetExpr` / the columnar lane for backend execution.
- the SQL text the rail accepts is lowered by `SqlPlanBuilder` against the federation's registered tables (`AddTableDefinition`/`AddTableProvider`), so the federation schema catalog is the table provider, not an ad-hoc string.
- custom federation operators register through `ISqlFunctionRegister`/`ITableProvider`, keeping the plan vocabulary one canonical surface rather than per-operator parsers.

[STACKING]:
- SQL → plan pipeline: a `Query/federation#FEDERATED_PLAN` definition arrives as SQL text; `SqlPlanBuilder` (riding the bundled `SqlParserCS` transitive) lowers it to a `Plan`, and the federation rail walks the `Plan` via a `RelationVisitor` to dispatch each `ReadRelation` to its backend — DuckDB (`api-duckdb`), ClickHouse (`api-clickhouse`), or an ADBC warehouse (`api-adbc-bigquery`). The SQL parser and the plan IR meet at the `SqlPlanBuilder.Sql` boundary; the plan is the federation's single intermediate form.
- foreign-plan ingress: a Substrait plan produced by ANOTHER engine (protobuf bytes or Substrait-JSON) enters the SAME federation rail without SQL — `Substrait.Protobuf.Plan.Parser.ParseFrom(bytes)` then `SubstraitDeserializer.Deserialize(protobufPlan)` (or `DeserializeFromJson(json)`) yields the managed `Plan` the federation visitor dispatches, so cross-tool interoperability is a public inbound path, not a re-parse. Because `SubstraitSerializer` is `internal`, an outbound wire plan is the RETAINED inbound bytes (`Plan.ToByteArray()`/`WriteTo`), never a managed→protobuf re-lowering.
- streaming materialization: `SubstraitToDifferentialCompute.Convert(plan, addWriteRelation: true, tableName, primaryKeys)` lowers a federation `Plan` into the FlowtideDotNet incremental differential-compute engine for a continuously-maintained materialized view — the same `Plan` IR drives both a one-shot federated query and a streaming materialization, no second plan model.
- backend pushdown: a `ReadRelation.Filter` and the `Relation.Emit` projection are walked by the federation visitor and pushed into the backend query (a ClickHouse SQL `WHERE`/column list, or a DuckDB scan projection) — the Substrait plan is the optimizer's pushdown carrier across heterogeneous backends.
- schema bridge: a backend's column schema (an Arrow `Schema` from `api-arrow`, or a DuckDB `ColumnInfo`) maps to a `NamedStruct` for `AddTableDefinition`, so the federation type system is one `SubstraitBaseType` lattice rather than per-backend type maps.
- fault rail: `SubstraitParseException` lifts at the `SqlPlanBuilder.Sql` edge into the query failure rail when SQL fails to lower, discriminated from a backend execution fault downstream.

[RAIL_LAW]:
- Package: `FlowtideDotNet.Substrait`
- Owns: the Substrait portable query-plan IR (`Plan`/`Relation`/`Expression`/type system), the SQL-text front-end (`SqlPlanBuilder` over `SqlParserCS`), the visitor double-dispatch fold, the standard function-extension URI catalogs, plan composition (`PlanModifier`), and the streaming differential-compute bridge
- Accept: `SqlPlanBuilder` lowering of federation SQL against registered tables, `SubstraitDeserializer.Deserialize`/`DeserializeFromJson` for a foreign protobuf/JSON plan (over `Substrait.Protobuf.Plan.Parser.ParseFrom`), `Plan` traversal via `RelationVisitor`/`ExpressionVisitor` for backend dispatch/pushdown, function references through the `Functions*` URI catalogs, `ISqlFunctionRegister`/`ITableProvider` for custom operators, and `SubstraitToDifferentialCompute.Convert` for streaming materialization
- Reject: treating this as a store or transport client, re-lowering a managed `Plan` to protobuf (`SubstraitSerializer` is `internal` — retain the inbound bytes), hard-coded function-name magic strings (use the `Functions*` URI catalogs), the `[Obsolete]` `PlanModifier.WriteToTable`, hand-rolled SQL parsing (the `SqlPlanBuilder` owns it), and a partial `RelationVisitor` silently dropping unhandled relation kinds (the base throws)
