# [RASM_PERSISTENCE_API_FLOWTIDE_SUBSTRAIT]

`FlowtideDotNet.Substrait` owns the portable Substrait query-plan IR: `Plan` over a relational-algebra DAG, its typed expression tree, the value lattice, and the standard function-extension catalogs. `SqlPlanBuilder` lowers SQL text into it and `SubstraitDeserializer` lifts a foreign wire or JSON plan into it, so every producer meets one vendor-neutral model. Visitor double-dispatch folds that model onto a concrete backend and `SubstraitToDifferentialCompute` re-uses it for the streaming engine — a query-plan owner, never a store or transport.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FlowtideDotNet.Substrait`
- package: `FlowtideDotNet.Substrait` (Apache-2.0)
- assembly: `FlowtideDotNet.Substrait`
- namespace: `FlowtideDotNet.Substrait`, `.Relations`, `.Expressions`, `.Expressions.Literals`, `.Expressions.IfThen`, `.Type`, `.FunctionExtensions`, `.Sql`, `.Conversion`, `.Hints`, `.Exceptions`; generated wire messages under `Substrait.Protobuf`
- target: multi-target `net10.0`/`net8.0`; a `net10.0` consumer binds `lib/net10.0`
- asset: pure-managed runtime library, no native payload
- depends: `SqlParserCS` SQL AST, `Google.Protobuf` wire runtime, `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.ObjectPool`
- rail: query-plan

## [02]-[PUBLIC_TYPES]

[PLAN_TYPES]: `Plan` roots the IR; every peer composes it, decodes it from the wire, annotates it, or reports its parse fault.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]    | [CAPABILITY]                                    |
| :-----: | :------------------------------------------ | :--------------- | :---------------------------------------------- |
|  [01]   | `Plan`                                      | sealed class     | plan root; `required List<Relation> Relations`  |
|  [02]   | `PlanModifier`                              | class            | fluent view/root composer with reference re-map |
|  [03]   | `SubstraitDeserializer`                     | class            | wire or JSON plan lifted to the managed `Plan`  |
|  [04]   | `Substrait.Protobuf.Plan`                   | protobuf message | generated `IMessage` wire form; static `Parser` |
|  [05]   | `Conversion.SubstraitToDifferentialCompute` | static class     | bridge into the FlowtideDotNet streaming engine |
|  [06]   | `Hints.Hint`                                | class            | per-relation alias with its optimizer bag       |
|  [07]   | `Hints.HintOptimizations`                   | class            | string-keyed optimizer bag; unknown-key logging |
|  [08]   | `Exceptions.SubstraitParseException`        | exception        | SQL and plan lowering fault                     |

[RELATION_TYPES]: `FlowtideDotNet.Substrait.Relations` holds the relational-algebra tree, prefix hoisted. `Relation` roots it with `Emit` column projection, `OutputLength`, `Hint`, and `Accept`; every concrete relation is a sealed value-equatable class whose inputs are `required`.

| [INDEX] | [SYMBOL]                                  | [CAPABILITY]                                                   |
| :-----: | :---------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `Relation`                                | abstract base; `Emit`/`EmitSet`/`OutputLength`/`Hint`/`Accept` |
|  [02]   | `RelationVisitor<TReturn, TState>`        | per-relation `Visit*` double-dispatch base                     |
|  [03]   | `ReadRelation`                            | source; `BaseSchema`/`NamedTable`/`Filter?` with `Copy()`      |
|  [04]   | `FilterRelation`                          | `Condition` predicate over `Input`                             |
|  [05]   | `ProjectRelation`                         | computed `Expressions` columns over `Input`                    |
|  [06]   | `JoinRelation`                            | `Type`/`Left`/`Right`/`Expression?`/`PostJoinFilter?`          |
|  [07]   | `MergeJoinRelation`                       | sorted-key join; `LeftKeys`/`RightKeys`                        |
|  [08]   | `AggregateRelation`                       | `Groupings` and `Measures` over `Input`                        |
|  [09]   | `AggregateGrouping`                       | one `GroupingExpressions` set                                  |
|  [10]   | `AggregateMeasure`                        | `Measure` aggregate with optional `Filter`                     |
|  [11]   | `SortRelation`                            | `Sorts` sort fields over `Input`                               |
|  [12]   | `TopNRelation`                            | `Sorts` with `Offset`/`Count`                                  |
|  [13]   | `FetchRelation`                           | `Offset`/`Count` limit                                         |
|  [14]   | `SetRelation`                             | `Operation` over `Inputs`                                      |
|  [15]   | `WriteRelation`                           | sink; `TableSchema`/`NamedObject`/`Overwrite`                  |
|  [16]   | `RootRelation`                            | `Input` with output column `Names`                             |
|  [17]   | `ReferenceRelation`                       | cross-subplan reference by `RelationId`                        |
|  [18]   | `PlanRelation`                            | nested plan wrapping one `Root`                                |
|  [19]   | `NormalizationRelation`                   | `KeyIndex` barrier with optional `Filter`                      |
|  [20]   | `VirtualTableReadRelation`                | inline literal rows; `BaseSchema` with `Values`                |
|  [21]   | `TableFunctionRelation`                   | `TableFunction` optionally joined to `Input`                   |
|  [22]   | `ConsistentPartitionWindowRelation`       | `WindowFunctions` over `PartitionBy`/`OrderBy`                 |
|  [23]   | `IterationRelation`                       | recursion; `LoopPlan`/`SkipIterateCondition?`/`MaxIterations?` |
|  [24]   | `IterationReferenceReadRelation`          | loop-back read by `IterationName`                              |
|  [25]   | `BufferRelation`                          | buffered `Input`                                               |
|  [26]   | `ExchangeRelation`                        | `ExchangeKind` and `Targets` over `PartitionCount`             |
|  [27]   | `SubStreamRootRelation`                   | named substream root                                           |
|  [28]   | `PullExchangeReferenceRelation`           | pull-side read by `SubStreamName`/`ExchangeTargetId`           |
|  [29]   | `StandardOutputExchangeReferenceRelation` | standard-output read by `RelationId`/`TargetId`                |
|  [30]   | `ExchangeKind`                            | abstract partition-kind base                                   |
|  [31]   | `ExchangeTarget`                          | abstract exchange-target base                                  |

[EXCHANGE_KINDS]: `ScatterExchangeKind` `SingleBucketExchangeKind` `BroadcastExchangeKind`
[EXCHANGE_TARGETS]: `StandardOutputExchangeTarget` `PullBucketExchangeTarget`
[RELATION_ENUMS]: `JoinType` `SetOperation` `ExchangeKindType` `ExchangeTargetType`

[EXPRESSION_TYPES]: `FlowtideDotNet.Substrait.Expressions` holds the expression tree, prefix hoisted, literals under `.Literals` and conditionals under `.IfThen`. `Expression` roots it with `Accept`; a function node names its extension by `ExtensionUri` and `ExtensionName` over `Arguments` with an optional `Options` map.

| [INDEX] | [SYMBOL]                             | [CAPABILITY]                                   |
| :-----: | :----------------------------------- | :--------------------------------------------- |
|  [01]   | `Expression`                         | abstract base; `Accept<TOutput, TState>`       |
|  [02]   | `ExpressionVisitor<TOutput, TState>` | per-expression `Visit*` fold base              |
|  [03]   | `ScalarFunction`                     | scalar call by extension URI and name          |
|  [04]   | `AggregateFunction`                  | aggregate call by extension URI and name       |
|  [05]   | `WindowFunction`                     | windowed call with `LowerBound`/`UpperBound`   |
|  [06]   | `TableFunction`                      | table-valued call carrying its `TableSchema`   |
|  [07]   | `FieldReference`                     | abstract column-reference base                 |
|  [08]   | `DirectFieldReference`               | access through a `ReferenceSegment` chain      |
|  [09]   | `ReferenceSegment`                   | abstract segment base with a `Child` chain     |
|  [10]   | `StructReferenceSegment`             | struct field by ordinal `Field`                |
|  [11]   | `MapKeyReferenceSegment`             | map value by `Key`                             |
|  [12]   | `CastExpression`                     | `Expression` cast to a `SubstraitBaseType`     |
|  [13]   | `IfThen.IfThenExpression`            | `Ifs` clauses with an optional `Else`          |
|  [14]   | `IfThen.IfClause`                    | one `If`/`Then` pair                           |
|  [15]   | `SingularOrListExpression`           | one `Value` against an option list             |
|  [16]   | `MultiOrListExpression`              | a `Value` tuple against `OrListRecord` options |
|  [17]   | `OrListRecord`                       | one multi-column IN-list record                |
|  [18]   | `NestedExpression`                   | abstract nested-constructor base               |
|  [19]   | `ListNestedExpression`               | list constructor over `Values`                 |
|  [20]   | `MapNestedExpression`                | map constructor over `KeyValues`               |
|  [21]   | `StructExpression`                   | struct constructor over `Fields`               |
|  [22]   | `SortField`                          | `Expression` with its `SortDirection`          |
|  [23]   | `WindowBound`                        | abstract frame-bound base                      |
|  [24]   | `Literals.Literal`                   | abstract typed constant; `LiteralType Type`    |

[WINDOW_BOUNDS]: `PreceedingRowWindowBound` `PreceedingRangeWindowBound` `FollowingRowWindowBound` `FollowingRangeWindowBound` `CurrentRowWindowBound` `UnboundedWindowBound`
[LITERALS]: `BoolLiteral` `NumericLiteral` `StringLiteral` `BinaryLiteral` `ArrayLiteral` `NullLiteral`
[EXPRESSION_ENUMS]: `SortDirection` `WindowBoundType` `Literals.LiteralType`

[TYPE_SYSTEM_TYPES]: `FlowtideDotNet.Substrait.Type` holds the value lattice, prefix hoisted. `SubstraitBaseType` roots it with a `SubstraitType Type` discriminant and a `Nullable` flag every leaf sets.

| [INDEX] | [SYMBOL]            | [CAPABILITY]                                       |
| :-----: | :------------------ | :------------------------------------------------- |
|  [01]   | `SubstraitBaseType` | abstract root; `Type` discriminant with `Nullable` |
|  [02]   | `NamedStruct`       | row type; `Names` over a `Struct`                  |
|  [03]   | `Struct`            | ordered column `Types`                             |
|  [04]   | `NamedTable`        | qualified table `Names`                            |
|  [05]   | `VirtualTable`      | inline literal rows as `StructExpression` values   |
|  [06]   | `DecimalType`       | `Precision` with `Scale`                           |
|  [07]   | `ListType`          | element `ValueType`                                |
|  [08]   | `MapType`           | `KeyType` with `ValueType`                         |

[SCALAR_LEAVES]: `BoolType` `Int32Type` `Int64Type` `Fp32Type` `Fp64Type` `StringType` `BinaryType` `DateType` `TimestampType` `AnyType` `NullType`
[SUBSTRAIT_TYPE]: `String` `Int32` `Any` `Date` `Int64` `Fp32` `Fp64` `Bool` `Decimal` `Struct` `Map` `List` `Binary` `TimestampTz` `Null`

[SQL_TYPES]: `FlowtideDotNet.Substrait.Sql` holds the SQL-text front-end and its extension seams, prefix hoisted.

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]  | [CAPABILITY]                                          |
| :-----: | :--------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `SqlPlanBuilder`                         | class          | SQL text lowered into one composed `Plan`             |
|  [02]   | `ITableProvider`                         | interface      | dynamic table and table-function resolution           |
|  [03]   | `ISqlFunctionRegister`                   | interface      | custom operator lowering registration                 |
|  [04]   | `TableMetadata`                          | class          | table `Name` with its `NamedStruct Schema`            |
|  [05]   | `SqlTableFunctionArgument`               | class          | call arguments with alias and lowering state          |
|  [06]   | `TableProviderTableFunctionArguments`    | class          | call plus `ParentRelation`/`JoinType`/`JoinCondition` |
|  [07]   | `TableProviderTableFunctionResult`       | class          | result relation with its sub-relations                |
|  [08]   | `ScalarResponse`                         | record         | lowered `Expression` with its type                    |
|  [09]   | `AggregateResponse`                      | record         | lowered `AggregateFunction` with its type             |
|  [10]   | `WindowResponse`                         | record         | lowered `WindowFunction` with its type                |
|  [11]   | `BaseExpressionVisitor<TReturn, TState>` | abstract class | SQL expression lowering fold base                     |
|  [12]   | `SqlExpressionVisitor`                   | class          | bound `ExpressionData`/`EmitData` lowering fold       |
|  [13]   | `EmitData`                               | class          | column-emit binding state threaded through lowering   |
|  [14]   | `ExpressionData`                         | class          | one lowered `Expr` with its `Name` and `Type`         |

[FUNCTION_CATALOG_TYPES]: `FlowtideDotNet.Substrait.FunctionExtensions` holds one class per standard Substrait catalog, prefix hoisted. Each class anchors its catalog at the `Uri` const shown and carries one `const string` per function name, so a call node references a function by const rather than a spelled literal.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :-------------------------- | :------------ | :---------------------------------- |
|  [01]   | `FunctionsArithmetic`       | static class  | `/functions_arithmetic.yaml`        |
|  [02]   | `FunctionsComparison`       | static class  | `/functions_comparison.yaml`        |
|  [03]   | `FunctionsBoolean`          | static class  | `/functions_boolean.yaml`           |
|  [04]   | `FunctionsString`           | static class  | `/functions_string.yaml`            |
|  [05]   | `FunctionsDatetime`         | static class  | `/functions_datetime.yaml`          |
|  [06]   | `FunctionsRounding`         | static class  | `/functions_rounding.yaml`          |
|  [07]   | `FunctionsLogarithmic`      | static class  | `/functions_logarithmic.yaml`       |
|  [08]   | `FunctionsAggregateGeneric` | static class  | `/functions_aggregate_generic.yaml` |
|  [09]   | `FunctionsList`             | static class  | `/functions_list.yaml`              |
|  [10]   | `FunctionsHash`             | static class  | `/functions_hash.yaml`              |
|  [11]   | `FunctionsGuid`             | static class  | `/functions_guid.yaml`              |
|  [12]   | `FunctionsTableGeneric`     | static class  | `/functions_table_generic.yaml`     |
|  [13]   | `FunctionsStruct`           | static class  | `/struct.yaml`                      |
|  [14]   | `FunctionsCheck`            | static class  | `/check.yaml`                       |

## [03]-[ENTRYPOINTS]

[SQL_ENTRYPOINTS]: `SqlPlanBuilder` accumulates table declarations, providers, and statements, then folds them into one composed `Plan`; construction preloads the built-in function set.

| [INDEX] | [SURFACE]                                  | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :----------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `SqlPlanBuilder()`                         | ctor     | preloads the built-in function register     |
|  [02]   | `AddTableDefinition(string, NamedStruct)`  | instance | declares a queryable table and its row type |
|  [03]   | `AddTableProvider(ITableProvider)`         | instance | binds dynamic table resolution              |
|  [04]   | `AddPlanAsView(string, Plan)`              | instance | registers a prior plan as a named view      |
|  [05]   | `Sql(string)`                              | instance | parses and lowers SQL into the plan         |
|  [06]   | `GetPlan() -> Plan`                        | instance | binds references and returns the plan       |
|  [07]   | `FunctionRegister -> ISqlFunctionRegister` | property | custom-function registration seam           |

- `SqlPlanBuilder.AddPlanAsView`: throws `InvalidOperationException` when the given plan carries no `RootRelation`.

[COMPOSE_ENTRYPOINTS]: `PlanModifier` stacks root plans and named views, then re-maps relation ids into one `Plan`; every builder verb returns the modifier.

| [INDEX] | [SURFACE]                     | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :---------------------------- | :------- | :---------------------------------- |
|  [01]   | `PlanModifier()`              | ctor     | an empty composition scope          |
|  [02]   | `AddRootPlan(Plan)`           | instance | stacks one root plan                |
|  [03]   | `AddPlanAsView(string, Plan)` | instance | registers a subplan as a named view |
|  [04]   | `Modify() -> Plan`            | instance | re-maps ids into one composed plan  |

- `PlanModifier.Modify()`: throws `InvalidOperationException` when no root plan was added.

[FUNCTION_SEAM]: `ISqlFunctionRegister` admits custom operator lowering, prefix hoisted; every row is an instance member taking the SQL name and its mapping delegate.

| [INDEX] | [SURFACE]                                                                      | [CAPABILITY]                   |
| :-----: | :----------------------------------------------------------------------------- | :----------------------------- |
|  [01]   | `RegisterScalarFunction(string, Func<…, ScalarResponse>)`                      | custom scalar lowering         |
|  [02]   | `RegisterAggregateFunction(string, Func<…, AggregateResponse>)`                | custom aggregate lowering      |
|  [03]   | `RegisterTableFunction(string, Func<SqlTableFunctionArgument, TableFunction>)` | custom table-function lowering |

[TABLE_SEAM]: `ITableProvider` resolves schema dynamically, prefix hoisted; each probe takes the dotted name as `IReadOnlyList<string>`.

| [INDEX] | [SURFACE]                                                                                                | [CAPABILITY]                |
| :-----: | :------------------------------------------------------------------------------------------------------- | :-------------------------- |
|  [01]   | `TryGetTableInformation(name) -> TableMetadata?`                                                         | resolves a table's row type |
|  [02]   | `TryHandleTableFunction(name, TableProviderTableFunctionArguments) -> TableProviderTableFunctionResult?` | resolves a table function   |

- Both `ITableProvider` probes return `bool` and hand the result back through an `out` parameter.

[WIRE_ENTRYPOINTS]: `SubstraitDeserializer` admits a foreign plan, and `SubstraitToDifferentialCompute` lowers a composed plan into the streaming engine.

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `SubstraitDeserializer.Deserialize(Substrait.Protobuf.Plan) -> Plan`   | instance | lifts a decoded wire plan               |
|  [02]   | `SubstraitDeserializer.Deserialize(string) -> Plan`                    | instance | lifts a Substrait-JSON plan             |
|  [03]   | `SubstraitDeserializer.DeserializeFromJson(string) -> Plan`            | static   | lifts JSON without an instance          |
|  [04]   | `SubstraitToDifferentialCompute.Convert(Plan, bool, string, string[])` | static   | lowers a plan into differential compute |

- `SubstraitToDifferentialCompute.Convert`: trailing primary keys arrive as `params`, and its `bool` selects whether a `WriteRelation` sink wraps the named table.

[IR_ENTRYPOINTS]: `Plan` builds and folds in memory without SQL — object-initializer construction over nodes, and a visitor subclass overriding the arms it handles.

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `new ReadRelation { BaseSchema, NamedTable, Filter }`           | ctor     | a source with row type and predicate |
|  [02]   | `new Plan { Relations }`                                        | ctor     | a plan from explicit relations       |
|  [03]   | `Relation.Accept(RelationVisitor<TReturn, TState>, TState)`     | instance | relation double-dispatch             |
|  [04]   | `RelationVisitor<TReturn, TState>.Visit(Relation, TState)`      | fold     | dispatches to the per-relation arm   |
|  [05]   | `Expression.Accept(ExpressionVisitor<TOutput, TState>, TState)` | instance | expression double-dispatch           |
|  [06]   | `ExpressionVisitor<TOutput, TState>.Visit(Expression, TState)`  | fold     | dispatches to the per-expression arm |
|  [07]   | `Relation.Emit`                                                 | property | output-column projection             |
|  [08]   | `Relation.Hint`                                                 | property | alias and optimizer hint carrier     |

- `RelationVisitor<TReturn, TState>`: an unoverridden `Visit*` arm throws `NotImplementedException`; `ExpressionVisitor<TOutput, TState>` returns `default(TOutput)`, so expression pushdown reads that null as the inexpressible signal.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `SubstraitBaseType` over its `SubstraitType` discriminant types every value, and `NamedStruct` is the row type `ReadRelation.BaseSchema` and `WriteRelation.TableSchema` carry.
- Every function node resolves by extension URI and name from the `Functions*` catalogs.
- Decode is public and encode internal: `SubstraitDeserializer` lifts wire and JSON plans, `SubstraitSerializer` never leaves the assembly, so an outbound plan is the retained inbound payload.
- Every `Relation` and `Expression` folds through `Accept`; a transform overrides only the arms it handles.

[STACKING]:
- `Apache.Arrow.Adbc`(`.api/api-arrow.md`): retained inbound bytes bind `AdbcStatement.SubstraitPlan`, mutually exclusive with `SqlQuery`, and the driver answers with one `IArrowArrayStream`.
- `Apache.Arrow.Adbc.Drivers.Apache`(`.api/api-adbc-apache.md`): the Thrift warehouse drivers take that same `SubstraitPlan` payload wherever the backend implements it.
- `DuckDB.NET.Data.Full`(`.api/api-duckdb.md`): the `substrait` extension closes the loop both ways — `from_substrait(⟨blob⟩)` executes retained bytes, and `get_substrait(⟨sql⟩)` yields a plan `SubstraitDeserializer.Deserialize` lifts into this IR.
- `ClickHouse.Driver`(`.api/api-clickhouse.md`): a `RelationVisitor` subclass lowers each `ReadRelation` subtree to SQL for `ClickHouseClient.ExecuteReaderAsync`, `ReadRelation.Filter` and `Relation.Emit` becoming the pushed predicate and projection.
- `DeltaLake.Net`(`.api/api-deltalake.md`): the same visitor lowers a subtree to DataFusion SQL for `DeltaTable.QueryAsync(SelectQuery)`, which streams `RecordBatch`.
- `Apache.Arrow`(`.api/api-arrow.md`): a backend `Schema` maps to `NamedStruct` for `SqlPlanBuilder.AddTableDefinition`, so one lattice types every registered table.
- `Google.Protobuf`(`.api/api-protobuf.md`): `Substrait.Protobuf.Plan` is a generated `IMessage` — `MessageParser<Plan>.ParseFrom` decodes the wire form and `MessageExtensions.ToByteArray`/`WriteTo` re-emits the retained payload.
- within-lib: the federation rail folds text ingress and foreign ingress into one `Plan`, fans each `ReadRelation` to its lane through a single `RelationVisitor` subclass, and hands that same plan to `SubstraitToDifferentialCompute.Convert` — one IR serving the one-shot query and the maintained view alike.

[LOCAL_ADMISSION]:
- Substrait enters behind the federation rail as the cross-backend query-plan IR a lane dispatches to its backend.
- SQL text lowers through `SqlPlanBuilder` against tables `AddTableDefinition` registers or an `ITableProvider` resolves; that provider is the schema catalog.
- Custom federation operators register through `ISqlFunctionRegister` and `ITableProvider`, keeping one plan vocabulary for every operator.
- `SubstraitParseException` lifts at the `SqlPlanBuilder.Sql` edge onto the query failure rail, discriminated from a downstream backend execution fault.

[RAIL_LAW]:
- Package: `FlowtideDotNet.Substrait`
- Owns: the portable query-plan IR — plan, relations, expressions, value lattice, function-extension catalogs — with the SQL front-end, inbound wire decode, the visitor fold, plan composition, and the differential-compute bridge.
- Accept: SQL lowered through `SqlPlanBuilder` against registered tables, a foreign plan lifted by `SubstraitDeserializer`, backend dispatch and pushdown through `RelationVisitor`/`ExpressionVisitor`, function references through the `Functions*` catalogs, custom operators through `ISqlFunctionRegister`/`ITableProvider`, and streaming materialization through `SubstraitToDifferentialCompute.Convert`.
- Reject: a hand-rolled SQL parser or second plan model beside this IR, a spelled function-name literal where a `Functions*` const exists, and a partial `RelationVisitor` whose unhandled arms drop relations instead of failing.
