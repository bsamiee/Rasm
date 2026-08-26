# [PERSISTENCE_QUERY_SERVING]

Rasm.Persistence serves every analytics backend through one plan and one transport dispatch. `BackendScope` carries the four coordinates a scan is bounded by, `BackendPlan` lowers one Substrait plan into each backend's own dialect, `BackendReach` decides the transport, and `BackendLanding` is the relational write peer of that read. Every declaration the plan addresses, every column it binds, and every cell it stages comes from `Query/backend#COLUMN_VOCABULARY` — this page reads that declaration and never re-states it.

Read SCOPE parts from read SHAPE here: tenant and window ride the scope value while the plan carries filters, projections, and folds alone, so an unbounded or cross-tenant backend scan has no shape that expresses it.

## [01]-[INDEX]

- [02]-[READ_PLAN]: `BackendWindow` and `BackendScope` bound every scan, `BackendFold` names the folds a question carries, and `BackendPlan` lowers one Substrait plan per dialect — the shipped operator catalogs beside this custodian's own backend folds.
- [03]-[SERVING_PLANE]: `BackendReach` closes the transports, `BackendRow` is the one row surface every reach yields, `BackendRead` runs the ONE open-execute-drain-result discipline, and `BackendLanding` stages the binary COPY under the same conformance gate the record-batch fold reads.
- [04]-[RESEARCH]: open verification debts and their routes.

## [02]-[READ_PLAN]

- Owner: `BackendWindow` is the half-open read window; `BackendScope` is the ONE read frame carrying backend, schema, window, and causal frame together; `BackendFold` is the closed vocabulary a caller names when it asks for a projected or grouped figure; `BackendPlan` is the ONE Substrait `RelationVisitor` lowering a logical plan per dialect and the ONE plan builder every consuming page composes instead of writing SQL.
- Cases: `BackendFold` is `Plain` (a declared column projected as itself), `Simple` (a shipped-catalog aggregate over one column), `Bucket` (a caller-stated grain), `Quantile` (a fraction under the convention the backend answers), `Weighted` (the toolkit time-weighted mean over raw chunks), `Mean` (the accessor over a materialised weight summary), and `Tail` (the accessor over a materialised sketch).
- Entry: `BackendPlan.Scan(AnalyticsSchema, Seq<(Identifier Column, string Value)> matches)` builds the filter-only plan; `Project(schema, matches, Seq<(Identifier Name, BackendFold Fold)> columns, Seq<Identifier> order)` builds the projected read; `Aggregate(schema, matches, Seq<Identifier> keys, Seq<(Identifier Name, BackendFold Fold)> folds)` builds the grouped read; `Lower(Plan, BackendScope, BackendProjection)` is the one lowering, gating the window and the backend's declared projection subset ahead of every relation.
- Auto: the plan is the query currency and the backend row supplies the tokens, so a question written once renders three ways and no second query language enters. Every arm recurses through `Visit`, never `Accept`, so the admitted-relation test runs once per node and an unadmitted kind returns the typed refusal rather than reaching a base arm that throws. Grouping keys thread DOWN into the fold, so a windowed aggregate re-buckets at the caller's grain rather than silently answering the backend's own storage grain.
- Packages: FlowtideDotNet.Substrait (`Plan`/`Relation`/`RelationVisitor<TReturn,TState>`/`ReadRelation`/`FilterRelation`/`ProjectRelation`/`AggregateRelation`/`AggregateMeasure`/`SortRelation`/`FetchRelation`/`TopNRelation`/`RootRelation`/`NamedTable.Names`/`NamedStruct.Names`/`DirectFieldReference`/`StructReferenceSegment.Field`/`ScalarFunction`/`AggregateFunction`/`SortField`/`SortDirection`/`Literals.NumericLiteral`/`Literals.StringLiteral`/`Literals.BoolLiteral`/`FunctionsComparison`/`FunctionsArithmetic`/`FunctionsAggregateGeneric`), Rasm (`Domain/stats#SCALAR_CARRIER` `QuantileRule` — the quantile convention a reader states rather than inherits), Rasm.Persistence (`Query/backend#COLUMN_VOCABULARY` `AnalyticsSchema`/`ColumnType.Plan`, `#BACKEND_FAMILY` `Backend`/`BackendProjection`/`BackendFault`, `#PROVISIONING` `SeriesBackend` — the materialised summary the toolkit folds read), NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new relation kind is one `Visit*` override beside its row in the admitted test; a new comparison or arithmetic operator is one row on `Operators` or `Postfixes`; a new shipped aggregate is one row on `Folds`; a new backend-owned fold is one row on `Residents` beside its `BackendFold` case, which breaks every builder at compile time; zero new surface — a second query language, a raw-SQL reader, a per-backend lowering, a hand-assembled relation at a consuming page, or a fold spelled at a call site is the deleted form.
- Law: the two fold CLASSES answer differently. Dialect folds — `time_bucket`, `quantile` — read the backend row's own delegate column, so every backend spells one or declines it by not publishing the projection it serves. Toolkit folds — `time_weight`, `weight_average`, `sketch_quantile` — read state only a provisioning arm that materialises the summary produces, so a backend whose arm emits no continuous aggregate refuses `Unlowerable` rather than lowering a naive `avg` that over-counts a dense burst under the caption the weighted mean earned.
- Boundary: the quantile fold carries the kernel `QuantileRule` the caller states, and every backend spelling — `percentile_cont` exactly, `quantileTDigest` and `approx_quantile` approximately — answers the INTERPOLATED convention, so a `NearestRank` request refuses at lowering rather than silently answering a definition the sample never contained. Kernel `Stat<TCarrier>` and `Distribution<TCarrier>` stay caller-side carriers and reach no arm here: a backend computes its figure in-engine off materialised state carrying no central moments, so the shared vocabulary is the RULE and never the summary. Field references arrive as ORDINALS a foreign plan carries, so resolution is fallible by construction — an ordinal past the roster refuses typed here, where an index into the column list throws straight out of the `Fin` fold. Backend folds ride this custodian's OWN extension URI: a backend resolving the shipped catalogs alone answers nothing for them, which is what keeps two solution folds from squatting on an upstream name.

```csharp
using Apache.Arrow;
using FlowtideDotNet.Substrait;
using FlowtideDotNet.Substrait.Expressions;
using FlowtideDotNet.Substrait.Expressions.Literals;
using FlowtideDotNet.Substrait.FunctionExtensions;
using FlowtideDotNet.Substrait.Relations;
using FlowtideDotNet.Substrait.Type;
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Rasm.Persistence.Element;
using System.Collections.Frozen;
using System.Globalization;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct BackendWindow(Instant From, Instant Until);

public sealed record BackendScope(Backend Backend, AnalyticsSchema Schema, BackendWindow Window, ProjectionContext Frame) {
    public Fin<string> Column(int ordinal) =>
        ordinal >= 0 && ordinal < Schema.Columns.Count
            ? Fin.Succ(Backend.Quote(Schema.Columns[ordinal].Name))
            : Fin.Fail<string>(new BackendFault.Unlowerable(Backend.Key, $"<field-ordinal:{ordinal}>"));

    public string Scope =>
        $"{Backend.Partition(Frame.Tenant)} AND {Backend.Quote(Schema.Time)} >= {Backend.Moment(Window.From)}"
        + $" AND {Backend.Quote(Schema.Time)} < {Backend.Moment(Window.Until)}";
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BackendFold {
    private BackendFold() { }
    public sealed record Plain(Identifier Column) : BackendFold;
    public sealed record Simple(string ExtensionName, Identifier Column) : BackendFold;
    public sealed record Bucket(Identifier Column, Duration Grain) : BackendFold;
    public sealed record Quantile(Identifier Column, double Fraction, QuantileRule Rule) : BackendFold;
    public sealed record Weighted(Identifier Column) : BackendFold;
    public sealed record Mean : BackendFold;
    public sealed record Tail(double Fraction, QuantileRule Rule) : BackendFold;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class BackendPlan : RelationVisitor<Fin<string>, BackendScope> {
    static readonly FrozenDictionary<string, string> Operators =
        new Dictionary<string, string>(StringComparer.Ordinal) {
            [FunctionsComparison.Equal] = "=", [FunctionsComparison.NotEqual] = "<>",
            [FunctionsComparison.GreaterThan] = ">", [FunctionsComparison.GreaterThanOrEqual] = ">=",
            [FunctionsComparison.LessThan] = "<", [FunctionsComparison.LessThanOrEqual] = "<=",
            [FunctionsArithmetic.Add] = "+", [FunctionsArithmetic.Subtract] = "-",
            [FunctionsArithmetic.Multiply] = "*", [FunctionsArithmetic.Divide] = "/",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    static readonly FrozenDictionary<string, string> Postfixes =
        new Dictionary<string, string>(StringComparer.Ordinal) {
            [FunctionsComparison.IsNull] = "IS NULL", [FunctionsComparison.IsNotNull] = "IS NOT NULL",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    static readonly FrozenDictionary<string, string> Folds =
        new Dictionary<string, string>(StringComparer.Ordinal) {
            [FunctionsArithmetic.Sum] = "sum", [FunctionsArithmetic.Min] = "min",
            [FunctionsArithmetic.Max] = "max", [FunctionsArithmetic.Average] = "avg",
            [FunctionsAggregateGeneric.Count] = "count",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    // --- [BACKEND_FOLDS]
    public const string BackendUri = "https://rasm.dev/substrait/backend.yaml";
    public const string BucketFold = "time_bucket";
    public const string QuantileFold = "quantile";
    public const string KeyFold = "key_equals";
    public const string WeightFold = "time_weight";
    public const string MeanFold = "weight_average";
    public const string TailFold = "sketch_quantile";

    static readonly FrozenDictionary<string, Func<Seq<Expression>, BackendScope, Fin<string>>> Residents =
        new Dictionary<string, Func<Seq<Expression>, BackendScope, Fin<string>>>(StringComparer.Ordinal) {
            [BucketFold] = static (arguments, state) => Grained(arguments, state)
                .Map(held => state.Backend.Bucket(held.Column, Duration.FromSeconds(held.Magnitude))),
            [QuantileFold] = static (arguments, state) => Graded(arguments, state, QuantileFold)
                .Map(held => state.Backend.Quantile(held.Column, held.Magnitude)),
            [KeyFold] = static (arguments, state) => Keyed(arguments, state)
                .Map(held => $"({held.Column} = {state.Backend.Literal(held.Hex)})"),
            [WeightFold] = static (arguments, state) => Toolkit(arguments, state, WeightFold).Bind(column =>
                Summarised(state, WeightFold).Map(_ =>
                    $"average(time_weight('linear', {state.Backend.Quote(state.Schema.Time)}, {column}))")),
            [MeanFold] = static (arguments, state) => arguments.IsEmpty
                ? Summarised(state, MeanFold).Map(_ => $"average({state.Backend.Quote(SeriesBackend.Weight)})")
                : Unarity(MeanFold, arguments.Count, 0, state),
            [TailFold] = static (arguments, state) => Sketched(arguments, state).Bind(fraction =>
                Summarised(state, TailFold).Map(_ =>
                    $"approx_percentile({fraction.ToString("0.####", CultureInfo.InvariantCulture)}, {state.Backend.Quote(SeriesBackend.Sketch)})")),
        }.ToFrozenDictionary(StringComparer.Ordinal);

    public override Fin<string> Visit(Relation relation, BackendScope state) =>
        relation is ReadRelation or FilterRelation or ProjectRelation or AggregateRelation or SortRelation or FetchRelation or TopNRelation or RootRelation
            ? base.Visit(relation, state)
            : Fin.Fail<string>(new BackendFault.Unlowerable(state.Backend.Key, relation.GetType().Name));

    public override Fin<string> VisitReadRelation(ReadRelation readRelation, BackendScope state) =>
        Named(readRelation, state).Map(relation => $"SELECT * FROM {relation} WHERE {state.Scope}");

    static Fin<string> Named(ReadRelation readRelation, BackendScope state) =>
        toSeq(readRelation.NamedTable?.Names ?? []).Last
            .ToFin(new BackendFault.Unlowerable(state.Backend.Key, "<unnamed-relation>"))
            .Bind(name => Op.Of().AcceptValidated<Identifier>(name).Map(state.Backend.Quote));

    public override Fin<string> VisitFilterRelation(FilterRelation filterRelation, BackendScope state) =>
        from inner in Visit(filterRelation.Input, state)
        from where in Predicate(filterRelation.Condition, state)
        select $"SELECT * FROM ({inner}) AS leg WHERE {where}";

    public override Fin<string> VisitProjectRelation(ProjectRelation projectRelation, BackendScope state) =>
        from inner in Visit(projectRelation.Input, state)
        from columns in toSeq(projectRelation.Expressions).TraverseM(expression => Predicate(expression, state)).As()
        select $"SELECT {Slotted(columns)} FROM ({inner}) AS leg";

    static string Slot(int ordinal) => $"c{ordinal.ToString(CultureInfo.InvariantCulture)}";
    static string Slotted(Seq<string> parts) => string.Join(", ", parts.Map(static (part, index) => $"{part} AS {Slot(index)}"));

    public override Fin<string> VisitAggregateRelation(AggregateRelation aggregateRelation, BackendScope state) =>
        from inner in Visit(aggregateRelation.Input, state)
        from keys in toSeq(aggregateRelation.Groupings ?? []).Bind(static grouping => toSeq(grouping.GroupingExpressions))
            .TraverseM(expression => Predicate(expression, state)).As()
        from folds in toSeq(aggregateRelation.Measures ?? []).TraverseM(measure => Fold(measure, state)).As()
        select keys.IsEmpty
            ? $"SELECT {Slotted(folds)} FROM ({inner}) AS leg"
            : $"SELECT {Slotted(keys + folds)} FROM ({inner}) AS leg GROUP BY {string.Join(", ", keys)}";

    public override Fin<string> VisitSortRelation(SortRelation sortRelation, BackendScope state) =>
        from inner in Visit(sortRelation.Input, state)
        from order in toSeq(sortRelation.Sorts)
            .TraverseM(field => Ordered(field, state, sortRelation.Input is ProjectRelation or AggregateRelation)).As()
        select $"SELECT * FROM ({inner}) AS leg ORDER BY {string.Join(", ", order)}";

    public override Fin<string> VisitFetchRelation(FetchRelation fetchRelation, BackendScope state) =>
        from inner in Visit(fetchRelation.Input, state)
        from bound in Bounded(fetchRelation.Count, fetchRelation.Offset, state)
        select $"SELECT * FROM ({inner}) AS leg {bound}";

    public override Fin<string> VisitTopNRelation(TopNRelation topNRelation, BackendScope state) =>
        from inner in Visit(topNRelation.Input, state)
        from order in toSeq(topNRelation.Sorts)
            .TraverseM(field => Ordered(field, state, topNRelation.Input is ProjectRelation or AggregateRelation)).As()
        from bound in Bounded(topNRelation.Count, topNRelation.Offset, state)
        select $"SELECT * FROM ({inner}) AS leg ORDER BY {string.Join(", ", order)} {bound}";

    static Fin<string> Bounded(int count, int offset, BackendScope state) =>
        count >= 0 && offset >= 0
            ? Fin.Succ($"LIMIT {count} OFFSET {offset}")
            : Fin.Fail<string>(new BackendFault.Unlowerable(state.Backend.Key, $"<fetch-bounds:{count}:{offset}>"));

    public override Fin<string> VisitRootRelation(RootRelation rootRelation, BackendScope state) =>
        from inner in Visit(rootRelation.Input, state)
        from names in toSeq(rootRelation.Names)
            .TraverseM(name => Op.Of().AcceptValidated<Identifier>(name).Map(state.Backend.Quote)).As()
        select $"SELECT {string.Join(", ", names.Map(static (name, index) => $"{Slot(index)} AS {name}"))} FROM ({inner}) AS root";

    // --- [EXPRESSIONS]
    static Fin<string> Predicate(Expression expression, BackendScope state) => expression switch {
        DirectFieldReference { ReferenceSegment: StructReferenceSegment segment } =>
            state.Column(segment.Field),
        NumericLiteral literal => Fin.Succ(literal.Value.ToString(CultureInfo.InvariantCulture)),
        StringLiteral literal => Fin.Succ($"'{literal.Value.Replace("'", "''", StringComparison.Ordinal)}'"),
        BoolLiteral literal => Fin.Succ(literal.Value ? "TRUE" : "FALSE"),
        ScalarFunction call when call.ExtensionUri == BackendUri =>
            Resident(call.ExtensionName, toSeq(call.Arguments), state),
        ScalarFunction call when Operators.TryGetValue(call.ExtensionName, out string? glyph) =>
            call.Arguments.Count == 2
                ? toSeq(call.Arguments).TraverseM(argument => Predicate(argument, state)).As()
                    .Map(parts => $"({string.Join($" {glyph} ", parts)})")
                : Unarity(call.ExtensionName, call.Arguments.Count, 2, state),
        ScalarFunction call when Postfixes.TryGetValue(call.ExtensionName, out string? postfix) =>
            call.Arguments.Count == 1
                ? Predicate(call.Arguments[0], state).Map(part => $"({part} {postfix})")
                : Unarity(call.ExtensionName, call.Arguments.Count, 1, state),
        _ => Fin.Fail<string>(new BackendFault.Unlowerable(state.Backend.Key, expression.GetType().Name)),
    };

    static Fin<string> Fold(AggregateMeasure measure, BackendScope state) =>
        measure.Measure.ExtensionUri == BackendUri
            ? Resident(measure.Measure.ExtensionName, toSeq(measure.Measure.Arguments), state)
            : Folds.TryGetValue(measure.Measure.ExtensionName, out string? verb)
                ? toSeq(measure.Measure.Arguments).TraverseM(argument => Predicate(argument, state)).As()
                    .Map(parts => $"{verb}({string.Join(", ", parts)})")
                : Fin.Fail<string>(new BackendFault.Unlowerable(state.Backend.Key, measure.Measure.ExtensionName));

    static Fin<string> Resident(string name, Seq<Expression> arguments, BackendScope state) =>
        Residents.TryGetValue(name, out Func<Seq<Expression>, BackendScope, Fin<string>>? render)
            ? render(arguments, state)
            : Fin.Fail<string>(new BackendFault.Unlowerable(state.Backend.Key, $"<backend-fold:{name}>"));

    static Fin<string> Unarity(string name, int found, int expected, BackendScope state) =>
        Fin.Fail<string>(new BackendFault.Unlowerable(state.Backend.Key, $"<arity:{name}:{found}:{expected}>"));

    static Fin<string> Ordered(SortField field, BackendScope state, bool slotted) =>
        (slotted && field.Expression is DirectFieldReference { ReferenceSegment: StructReferenceSegment segment }
            ? Fin.Succ(Slot(segment.Field))
            : Predicate(field.Expression, state))
        .Map(part => field.SortDirection switch {
            SortDirection.SortDirectionDescNullsFirst or SortDirection.SortDirectionDescNullsLast => $"{part} DESC",
            _ => $"{part} ASC",
        });

    // --- [FOLD_ARGUMENTS]
    static Fin<(string Column, double Magnitude)> Paired(Seq<Expression> arguments, BackendScope state, string fold) =>
        arguments.ToArray() is [Expression column, NumericLiteral magnitude]
            ? Predicate(column, state).Map(held => (Column: held, Magnitude: (double)magnitude.Value))
            : Fin.Fail<(string, double)>(new BackendFault.Unlowerable(state.Backend.Key, $"<backend-fold:{fold}:{arguments.Count}>"));

    static Fin<(string Column, double Magnitude)> Grained(Seq<Expression> arguments, BackendScope state) =>
        Paired(arguments, state, BucketFold).Bind(held => held.Magnitude > 0
            ? Fin.Succ(held)
            : Fin.Fail<(string, double)>(new BackendFault.Unlowerable(state.Backend.Key, $"<bucket-grain:{held.Magnitude}>")));

    static Fin<(string Column, double Magnitude)> Graded(Seq<Expression> arguments, BackendScope state, string fold) =>
        arguments.ToArray() is [Expression column, NumericLiteral literal, StringLiteral rule]
            ? Convention(rule.Value, state).Bind(_ => Fraction((double)literal.Value, state)
                .Bind(fraction => Predicate(column, state).Map(held => (Column: held, Magnitude: fraction))))
            : Fin.Fail<(string, double)>(new BackendFault.Unlowerable(state.Backend.Key, $"<backend-fold:{fold}:{arguments.Count}>"));

    static Fin<double> Sketched(Seq<Expression> arguments, BackendScope state) =>
        arguments.ToArray() is [NumericLiteral literal, StringLiteral rule]
            ? Convention(rule.Value, state).Bind(_ => Fraction((double)literal.Value, state))
            : Fin.Fail<double>(new BackendFault.Unlowerable(state.Backend.Key, $"<backend-fold:{TailFold}:{arguments.Count}>"));

    static Fin<Unit> Convention(string token, BackendScope state) =>
        Op.Of().Row<string, QuantileRule>(token)
            .MapFail(_ => new BackendFault.Unlowerable(state.Backend.Key, $"<quantile-rule:{token}>"))
            .Bind(asked => asked == QuantileRule.Interpolated
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new BackendFault.Unanswerable(state.Backend.Key, BackendProjection.Quantile.Key,
                    $"every backend quantile spelling answers {QuantileRule.Interpolated.Key} alone")));

    static Fin<double> Fraction(double fraction, BackendScope state) =>
        fraction is >= 0 and <= 1
            ? Fin.Succ(fraction)
            : Fin.Fail<double>(new BackendFault.Unlowerable(state.Backend.Key, $"<quantile-fraction:{fraction}>"));

    static Fin<string> Toolkit(Seq<Expression> arguments, BackendScope state, string fold) =>
        arguments.ToArray() is [Expression column]
            ? Predicate(column, state)
            : Fin.Fail<string>(new BackendFault.Unlowerable(state.Backend.Key, $"<backend-fold:{fold}:{arguments.Count}>"));

    static Fin<(string Column, string Hex)> Keyed(Seq<Expression> arguments, BackendScope state) =>
        arguments.ToArray() is [Expression column, StringLiteral hex]
            ? Predicate(column, state).Map(held => (Column: held, Hex: hex.Value))
            : Fin.Fail<(string, string)>(new BackendFault.Unlowerable(state.Backend.Key, $"<backend-fold:{KeyFold}:{arguments.Count}>"));

    static Fin<Unit> Summarised(BackendScope state, string fold) =>
        state.Backend.Statements == SeriesBackend.Statements
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new BackendFault.Unlowerable(state.Backend.Key, $"<unsummarised:{fold}>"));

    // --- [PLAN_BUILDERS]
    public static Fin<Plan> Scan(AnalyticsSchema schema, Seq<(Identifier Column, string Value)> matches) =>
        Project(schema, schema.Table, matches,
            schema.Columns.Map(static column => (column.Name, (BackendFold)new BackendFold.Plain(column.Name))),
            Seq<Identifier>());

    public static Fin<Plan> Project(AnalyticsSchema schema, Identifier relation, Seq<(Identifier Column, string Value)> matches,
        Seq<(Identifier Name, BackendFold Fold)> columns, Seq<Identifier> order) =>
        from shaped in columns.TraverseM(entry => Projected(schema, entry.Fold)).As()
        from plan in Build(schema, relation, matches, order, columns.Map(static entry => entry.Name),
            read => new ProjectRelation { Input = read, Expressions = [.. shaped] })
        select plan;

    public static Fin<Plan> Aggregate(AnalyticsSchema schema, Identifier relation, Seq<(Identifier Column, string Value)> matches,
        Seq<Identifier> keys, Seq<(Identifier Name, BackendFold Fold)> folds) =>
        from measures in folds.TraverseM(entry => Measured(schema, entry.Fold)).As()
        from grouping in keys.TraverseM(key => Reference(schema, key)).As()
        from plan in Build(schema, relation, matches, Seq<Identifier>(), keys + folds.Map(static entry => entry.Name),
            read => new AggregateRelation {
                Input = read,
                Groupings = [new AggregateGrouping { GroupingExpressions = [.. grouping] }],
                Measures = [.. measures],
            })
        select plan;

    static Fin<Plan> Build(AnalyticsSchema schema, Identifier relation, Seq<(Identifier Column, string Value)> matches,
        Seq<Identifier> order, Seq<Identifier> names, Func<Relation, Relation> shape) =>
        from conditions in matches.TraverseM(match => Narrowed(schema, match)).As()
        from sorts in order.TraverseM(key => names.IndexOf(key) is var slot && slot >= 0
            ? Fin.Succ<Expression>(new DirectFieldReference { ReferenceSegment = new StructReferenceSegment { Field = slot } })
            : Fin.Fail<Expression>(new BackendFault.Unprovisioned($"<plan-order:{schema.Dataset}.{(string)key}>"))).As()
        select Rooted(relation, schema, conditions, sorts, names, shape);

    static Plan Rooted(Identifier relation, AnalyticsSchema schema, Seq<Expression> conditions,
        Seq<Expression> sorts, Seq<Identifier> names, Func<Relation, Relation> shape) {
        Relation shaped = shape(conditions.Fold(
            (Relation)new ReadRelation {
                NamedTable = new NamedTable { Names = [(string)relation] },
                BaseSchema = new NamedStruct { Names = [.. schema.Columns.Map(static column => (string)column.Name)] },
            },
            static (input, condition) => new FilterRelation { Input = input, Condition = condition }));
        Relation ordered = sorts.IsEmpty
            ? shaped
            : new SortRelation {
                Input = shaped,
                Sorts = [.. sorts.Map(static sort => new SortField { Expression = sort, SortDirection = SortDirection.SortDirectionAscNullsLast })],
            };
        return new Plan { Relations = [new RootRelation { Input = ordered, Names = [.. names.Map(static name => (string)name)] }] };
    }

    static Fin<Expression> Narrowed(AnalyticsSchema schema, (Identifier Column, string Value) match) =>
        Reference(schema, match.Column).Bind(field =>
            schema.Columns[schema.Ordinal(match.Column)].Type.Plan(match.Value).Match(
                Some: literal => Fin.Succ<Expression>(new ScalarFunction {
                    ExtensionUri = FunctionsComparison.Uri,
                    ExtensionName = FunctionsComparison.Equal,
                    Arguments = [field, literal],
                }),
                None: () => schema.Columns[schema.Ordinal(match.Column)].Type is ColumnShape.Scalar scalar
                    && scalar.Type == ColumnType.KeyHex
                    ? Fin.Succ<Expression>(new ScalarFunction {
                        ExtensionUri = BackendUri,
                        ExtensionName = KeyFold,
                        Arguments = [field, new StringLiteral { Value = match.Value }],
                    })
                    : Fin.Fail<Expression>(new BackendFault.Unlowerable(
                        schema.Dataset, $"<literal:{schema.Dataset}.{(string)match.Column}>"))));

    static Fin<Expression> Reference(AnalyticsSchema schema, Identifier column) =>
        schema.Ordinal(column) is var ordinal && ordinal >= 0
            ? Fin.Succ<Expression>(new DirectFieldReference { ReferenceSegment = new StructReferenceSegment { Field = ordinal } })
            : Fin.Fail<Expression>(new BackendFault.Unprovisioned($"<schema-column:{schema.Dataset}.{(string)column}>"));

    static Fin<Either<Expression, (string Uri, string Name, Seq<Expression> Arguments)>> Called(AnalyticsSchema schema, BackendFold fold) =>
        fold.Switch(
            state:    schema,
            plain:    static (declaration, c) => Reference(declaration, c.Column).Map(Either<Expression, (string, string, Seq<Expression>)>.Left),
            simple:   static (declaration, c) => Folds.ContainsKey(c.ExtensionName)
                ? Called(declaration, c.Column, FunctionsArithmetic.Uri, c.ExtensionName, Seq<Expression>())
                : Fin.Fail<Either<Expression, (string, string, Seq<Expression>)>>(
                    new BackendFault.Unlowerable(declaration.Dataset, $"<fold:{c.ExtensionName}>")),
            bucket:   static (declaration, c) => Called(declaration, c.Column, BackendUri, BucketFold, Seq(Magnitude(c.Grain.TotalSeconds))),
            quantile: static (declaration, c) => Called(declaration, c.Column, BackendUri, QuantileFold, Seq(Magnitude(c.Fraction), Rule(c.Rule))),
            weighted: static (declaration, c) => Called(declaration, c.Column, BackendUri, WeightFold, Seq<Expression>()),
            mean:     static (_, _) => Called(BackendUri, MeanFold, Seq<Expression>()),
            tail:     static (_, c) => Called(BackendUri, TailFold, Seq(Magnitude(c.Fraction), Rule(c.Rule))));

    static Fin<Either<Expression, (string Uri, string Name, Seq<Expression> Arguments)>> Called(
        string uri, string name, Seq<Expression> arguments) =>
        Fin.Succ(Either<Expression, (string, string, Seq<Expression>)>.Right((uri, name, arguments)));

    static Fin<Either<Expression, (string Uri, string Name, Seq<Expression> Arguments)>> Called(
        AnalyticsSchema schema, Identifier column, string uri, string name, Seq<Expression> tail) =>
        Reference(schema, column).Map(field =>
            Either<Expression, (string, string, Seq<Expression>)>.Right((uri, name, Seq(field) + tail)));

    static Fin<Expression> Projected(AnalyticsSchema schema, BackendFold fold) =>
        Called(schema, fold).Map(static held => held.Match(
            Left:  static reference => reference,
            Right: static call => (Expression)new ScalarFunction {
                ExtensionUri = call.Uri, ExtensionName = call.Name, Arguments = [.. call.Arguments],
            }));

    static Fin<AggregateMeasure> Measured(AnalyticsSchema schema, BackendFold fold) =>
        Called(schema, fold).Bind(held => held.Match(
            Left:  _ => Fin.Fail<AggregateMeasure>(new BackendFault.Unlowerable(schema.Dataset, "<measure-plain-column>")),
            Right: call => Fin.Succ(new AggregateMeasure {
                Measure = new AggregateFunction { ExtensionUri = call.Uri, ExtensionName = call.Name, Arguments = [.. call.Arguments] },
            })));

    static Expression Magnitude(double value) => new NumericLiteral { Value = (decimal)value };
    static Expression Rule(QuantileRule rule) => new StringLiteral { Value = rule.Key };

    public static Fin<string> Lower(Plan plan, BackendScope scope, BackendProjection projection) =>
        scope.Window.Until <= scope.Window.From
            ? Fin.Fail<string>(new BackendFault.ReadRefused(scope.Backend.Key, new EngineFault("<read-window>", $"{scope.Window.From}..{scope.Window.Until}")))
            : !scope.Backend.Answers(projection)
                ? Fin.Fail<string>(new BackendFault.Unanswerable(scope.Backend.Key, projection.Key, scope.Backend.Degrade))
                : toSeq(plan.Relations).Last
                    .ToFin(new BackendFault.Unlowerable(scope.Backend.Key, "<empty-plan>"))
                    .Bind(root => new BackendPlan().Visit(root, scope));
}
```

| [INDEX] | [POLICY]          | [VALUE]                                     | [BINDING]                                                        |
| :-----: | :---------------- | :------------------------------------------ | :--------------------------------------------------------------- |
|  [01]   | query currency    | one Substrait `Plan` per question           | one lowering, three dialects; no second query language           |
|  [02]   | read scope        | frame tenant + `BackendWindow`              | the plan carries shape; an unbounded scan is unrepresentable     |
|  [03]   | narrowing literal | the column's own declared `ColumnType.Plan` | a text operand against a numeric column raises or coerces        |
|  [04]   | dialect fold      | `time_bucket`/`quantile` off the row        | every backend spells it or declines the projection it serves     |
|  [05]   | toolkit fold      | `time_weight`/accessors off the rollup      | gated on the arm that materialises the summary, never on a key   |
|  [06]   | quantile rule     | kernel `QuantileRule.Interpolated`          | `NearestRank` refuses; no backend spells `percentile_disc`       |
|  [07]   | relation naming   | the plan's own `NamedTable`                 | a rollup read and a raw read differ by relation, not by lowering |
|  [08]   | plan assembly     | `Scan`/`Project`/`Aggregate` builders       | no consuming page assembles a relation or spells an extension    |
|  [09]   | key narrowing     | the backend's own `Literal` spelling        | a key carries no plan literal; quoted hex matches no `bytea`     |

## [03]-[SERVING_PLANE]

- Owner: `BackendReach` closes the transports one read discriminates on; `BackendRow` is the ONE row surface every reach yields, with a `Fin` reader per physical family; `BackendResult<T>` carries the read result; `BackendHealth` is the family policy-health row; `BackendRead` is the ONE query entry and the ONE health probe; `BackendWrite` carries the staged count and `BackendLanding` is the ONE relational landing.
- Cases: `BackendReach` is `Relational(NpgsqlDataSource)` | `Fleet(ClickHouseClient)` | `Flight(FlightSqlClient)` | `Local(ColumnarSession)`; `BackendRow` is `Ado(DbDataReader)` | `Arrow(RecordBatch, int Ordinal)`.
- Entry: `BackendRead.Read<T>(BackendReach, Plan, BackendScope, BackendProjection, Func<BackendRow, Fin<T>>)` is the ONE query entry over every backend; `BackendRead.Health(BackendReach, Backend, AnalyticsSchema, ProjectionContext)` is the FAMILY policy probe every backend answers over the same reach arms; `BackendLanding.Stage(NpgsqlDataSource, AnalyticsSchema, Seq<Seq<ColumnCell>>, ProjectionContext)` is the ONE relational landing.
- Auto: every reach runs ONE ordered discipline — OPEN the lease chain, EXECUTE the lowered text, DRAIN through the caller's one shape, REPORT the scanned figure only that leg can honestly supply — so the three ADO-shaped reaches are one body and three rows and the Arrow reach differs by its drain alone. No caller-supplied SQL string has a parameter to arrive on, which is what makes writer/reader drift and ad-hoc tenant scans unrepresentable. Landing derives its column list, its tenancy lead, and every wire type from the same `AnalyticsSchema` the DDL emitter provisions from, so a landed row and the table it lands in cannot drift on order, count, or physical type.
- Packages: Npgsql (`NpgsqlDataSource.CreateCommand`/`OpenConnectionAsync`/`NpgsqlConnection.BeginBinaryImportAsync`/`NpgsqlBinaryImporter.StartRowAsync`/`WriteAsync`/`CompleteAsync`/`NpgsqlException`), ClickHouse.Driver (`ClickHouseClient.CreateConnection`/`ClickHouseCommand.ExecuteReaderAsync`/`QueryStats`/`ClickHouseServerException`), DuckDB.NET.Data.Full (`DuckDBConnection.Duplicate`/`DuckDBCommand.UseStreamingMode`/`DuckDBException`), Apache.Arrow.Flight.Sql (`FlightSqlClient.ExecuteAsync(string, Transaction)`/`DoGetAsync(FlightTicket)`/`Transaction.NoTransaction`), Apache.Arrow.Flight (`FlightInfo.Endpoints`/`TotalRecords`/`FlightEndpoint.Ticket`), Apache.Arrow (`RecordBatch`/`StringArray`/`Int64Array`/`DoubleArray`/`TimestampArray`/`ListArray.ValueOffsets`/`Values`), Rasm.Persistence (`Query/backend#COLUMN_VOCABULARY` `ColumnCell`/`ColumnShape.Admits`/`ColumnType.Cell`, `#BACKEND_FAMILY` `Backend`, `Element/graph#PROJECTION_FRAME` `ProjectionContext`), NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new transport is one `BackendReach` case breaking the read dispatch at compile time; a new ADO-shaped reach is one `AdoLeg` row carrying its lease chain and its scanned reader; a new physical family a shape reads is one `BackendRow` member answering both arms; zero new surface — a per-backend read entry, a per-reach shape delegate, a `DbDataReader`-typed shape the Arrow leg must fake, a second importer body, or a raw-SQL reader is the deleted form.
- Law: absence is a RESULT fact on both row arms — an Arrow primitive reads nullable by construction and a relational column reads `IsDBNull` ahead of its typed getter, so a shape needing a total column takes the refusal rather than an empty string, a zero, or a 1970 instant, the three sentinels a board renders indistinguishably from a measured reading. `QueryStats` is the ONLY honest scanned figure on the Fleet leg: the returned row count says nothing about the granules a predicate pruned, which is the whole reason the tenant leads the sort key.
- Boundary: conformance is ARITY, TYPE, CANONICITY, and WRITABILITY together and ACCUMULATES, all ahead of the copy — the binary importer infers nothing from a column list and a mismatch found at row n discards the n-1 rows already staged, so a producer learns every offending column from one refusal. It runs against `Supplied`, never `Payload` — against what the producer's contract obliges it to send — because counting the custodian's own stamped columns demands cells the category forbids a producer to carry and reads a correct producer as a defective one. `ColumnShape.Admits` is that gate, the same proof `ArrowLanding.Build` runs, so the batch fold and the COPY fold cannot disagree on whether a cell belongs to its column. Elapsed rides the frame's own monotonic mark: `ProjectionContext` is this package's ruled time-and-causal frame and already carries the `Mark`/`Elapsed` pair, so no leg starts a second clock. Cancellation stays a result fact and propagates — a cooperative cancel converted to a fault reads as a failed landing.

```csharp
using Apache.Arrow;
using Apache.Arrow.Arrays;
using Apache.Arrow.Flight;
using Apache.Arrow.Flight.Sql;
using ClickHouse.Driver;
using ClickHouse.Driver.ADO;
using DuckDB.NET.Data;
using FlowtideDotNet.Substrait;
using LanguageExt;
using NodaTime;
using Npgsql;
using NpgsqlTypes;
using Rasm.Domain;
using Rasm.Persistence.Element;
using System.Buffers.Binary;
using System.Data.Common;
using System.Globalization;
using System.Text.Json;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BackendReach {
    private BackendReach() { }
    public sealed record Relational(NpgsqlDataSource Source) : BackendReach;
    public sealed record Fleet(ClickHouseClient Client) : BackendReach;
    public sealed record Flight(FlightSqlClient Client) : BackendReach;
    public sealed record Local(ColumnarSession Session) : BackendReach;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BackendRow {
    private BackendRow() { }
    public sealed record Ado(DbDataReader Reader) : BackendRow;
    public sealed record Arrow(RecordBatch Batch, int Ordinal) : BackendRow;

    public Fin<string> Text(Backend backend, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column) ? Option<string>.None : Some(c.Reader.GetString(column)),
        arrow: c => Optional(((StringArray)c.Batch.Column(column))[c.Ordinal]))
        .ToFin(Missing(backend, column));

    public Fin<long> Whole(Backend backend, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column) ? Option<long>.None : Some(c.Reader.GetInt64(column)),
        arrow: c => Optional(((Int64Array)c.Batch.Column(column)).GetValue(c.Ordinal)))
        .ToFin(Missing(backend, column));

    public Fin<double> Real(Backend backend, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column) ? Option<double>.None : Some(c.Reader.GetDouble(column)),
        arrow: c => Optional(((DoubleArray)c.Batch.Column(column)).GetValue(c.Ordinal)))
        .ToFin(Missing(backend, column));

    public Fin<Instant> At(Backend backend, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column)
            ? Option<Instant>.None
            : Some(Instant.FromDateTimeUtc(DateTime.SpecifyKind(c.Reader.GetDateTime(column), DateTimeKind.Utc))),
        arrow: c => Optional(((TimestampArray)c.Batch.Column(column))[c.Ordinal]).Map(Instant.FromDateTimeOffset))
        .ToFin(Missing(backend, column));

    public Fin<UInt128> Key(Backend backend, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column) ? Option<byte[]>.None : Some(c.Reader.GetFieldValue<byte[]>(column)),
        arrow: c => Optional(((FixedSizeBinaryArray)c.Batch.Column(column)).GetBytes(c.Ordinal).ToArray()))
        .ToFin(Missing(backend, column))
        .Map(static packed => BinaryPrimitives.ReadUInt128BigEndian(packed));

    public Fin<Seq<string>> Items(Backend backend, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column) ? Option<Seq<string>>.None : Some(toSeq(c.Reader.GetFieldValue<string[]>(column))),
        arrow: c => ((ListArray)c.Batch.Column(column)) is var list && !list.IsNull(c.Ordinal)
            ? Some(toSeq(Enumerable.Range(list.ValueOffsets[c.Ordinal], list.ValueOffsets[c.Ordinal + 1] - list.ValueOffsets[c.Ordinal])
                .Select(index => ((StringArray)list.Values).GetString(index))))
            : Option<Seq<string>>.None)
        .ToFin(Missing(backend, column));

    static BackendFault Missing(Backend backend, int column) =>
        new BackendFault.ReadRefused(backend.Key,
            new EngineFault("<null-column>", column.ToString(CultureInfo.InvariantCulture)));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct BackendResult<T>(Backend Backend, string Lowered, Seq<T> Rows, long Scanned, Duration Elapsed);

public readonly record struct BackendHealth(string Backend, string Relation, Option<Instant> Oldest, Option<Instant> Newest, long Rows) {
    public bool Retained(BackendPolicy policy, Instant now) => Oldest.Map(at => at >= now - policy.Retain).IfNone(true);
    public Duration Lag(Instant now) => Newest.Map(at => now - at).IfNone(Duration.Zero);
}

public readonly record struct BackendWrite(string Dataset, long Staged, Instant At, CorrelationId Correlation);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class BackendRead {
    public static IO<Fin<BackendResult<T>>> Read<T>(
        BackendReach reach, Plan plan, BackendScope scope, BackendProjection projection, Func<BackendRow, Fin<T>> shape) =>
        BackendPlan.Lower(plan, scope, projection).Match(
            Succ: lowered => Serve(reach, scope.Backend, lowered, shape, scope.Frame),
            Fail: error => IO.pure(Fin<BackendResult<T>>.Fail(error)));

    public static IO<Fin<BackendResult<BackendHealth>>> Health(
        BackendReach reach, Backend backend, AnalyticsSchema schema, ProjectionContext frame) =>
        Serve(reach, backend, backend.Horizon(schema), row => Shape(backend, schema, row), frame);

    static Fin<BackendHealth> Shape(Backend backend, AnalyticsSchema schema, BackendRow row) =>
        row.Whole(backend, 2).Bind(rows => rows == 0
            ? Fin.Succ(new BackendHealth(backend.Key, (string)schema.Table, None, None, rows))
            : (row.At(backend, 0).ToValidation(), row.At(backend, 1).ToValidation())
                .Apply((oldest, newest) => new BackendHealth(
                    backend.Key, (string)schema.Table, Some(oldest), Some(newest), rows)).As().ToFin());

    // --- [SERVING_DISCIPLINE]
    static IO<Fin<BackendResult<T>>> Serve<T>(
        BackendReach reach, Backend backend, string lowered, Func<BackendRow, Fin<T>> shape, ProjectionContext frame) =>
        reach.Switch(
            relational: leg => Ado(Postgres(leg), backend, lowered, shape, frame),
            fleet:      leg => Ado(Clickhouse(leg), backend, lowered, shape, frame),
            local:      leg => Ado(Duck(leg), backend, lowered, shape, frame),
            flight:     leg => Arrow(leg, backend, lowered, shape, frame));

    readonly record struct AdoLeg(
        Func<string, ValueTask<(Seq<IAsyncDisposable> Leases, DbCommand Command)>> Open,
        Func<DbCommand, Seq<IAsyncDisposable>, long, Duration, (long Scanned, Duration Elapsed)> Report);

    static AdoLeg Postgres(BackendReach.Relational leg) => new(
        async lowered => (Seq<IAsyncDisposable>(), (DbCommand)leg.Source.CreateCommand(lowered)),
        static (_, _, returned, elapsed) => (returned, elapsed));

    static AdoLeg Clickhouse(BackendReach.Fleet leg) => new(
        async lowered => {
            ClickHouseConnection lane = leg.Client.CreateConnection();
            await lane.OpenAsync().ConfigureAwait(false);
            return (Seq<IAsyncDisposable>(lane), new ClickHouseCommand(lane) { CommandText = lowered });
        },
        static (command, _, returned, elapsed) => ((ClickHouseCommand)command).QueryStats is { } stats
            ? ((long)stats.ReadRows, Duration.FromNanoseconds(stats.ElapsedNs))
            : (returned, elapsed));

    static AdoLeg Duck(BackendReach.Local leg) => new(
        async lowered => {
            DuckDBConnection lane = leg.Session.Lane();
            await lane.OpenAsync().ConfigureAwait(false);
            DuckDBCommand command = lane.CreateCommand();
            (command.CommandText, command.UseStreamingMode) = (lowered, true);
            return (Seq<IAsyncDisposable>(lane), command);
        },
        static (_, _, returned, elapsed) => (returned, elapsed));

    static IO<Fin<BackendResult<T>>> Ado<T>(
        AdoLeg leg, Backend backend, string lowered, Func<BackendRow, Fin<T>> shape, ProjectionContext frame) =>
        IO.liftAsync(async () => (await Op.Of().Catch(async token => {
            long mark = frame.Mark();
            (Seq<IAsyncDisposable> leases, DbCommand command) = await leg.Open(lowered).ConfigureAwait(false);
            try {
                await using DbDataReader reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
                return (await Drain(reader, shape).ConfigureAwait(false)).Map(rows => {
                    (long scanned, Duration elapsed) = leg.Report(command, leases, rows.Count, frame.Elapsed(mark));
                    return new BackendResult<T>(backend, lowered, rows, scanned, elapsed);
                });
            }
            finally {
                await command.DisposeAsync().ConfigureAwait(false);
                foreach (IAsyncDisposable lease in leases.Rev()) { await lease.DisposeAsync().ConfigureAwait(false); }
            }
        }).ConfigureAwait(false)).MapFail(backend.ReadRefused));

    static IO<Fin<BackendResult<T>>> Arrow<T>(
        BackendReach.Flight leg, Backend backend, string lowered, Func<BackendRow, Fin<T>> shape, ProjectionContext frame) =>
        IO.liftAsync(async () => await Op.Of().Catch(async _ => {
            long mark = frame.Mark();
            FlightInfo info = await leg.Client.ExecuteAsync(lowered, Transaction.NoTransaction).ConfigureAwait(false);
            Fin<Seq<T>> rows = Fin.Succ(Seq<T>());
            foreach (FlightEndpoint endpoint in info.Endpoints) {
                await foreach (RecordBatch batch in leg.Client.DoGetAsync(endpoint.Ticket).ConfigureAwait(false)) {
                    rows = rows.Bind(held => Batched(batch, shape).Map(batched => held + batched));
                }
            }
            return rows.Map(held => new BackendResult<T>(backend, lowered, held, info.TotalRecords, frame.Elapsed(mark)));
        }).ConfigureAwait(false));

    static async ValueTask<Fin<Seq<T>>> Drain<T>(DbDataReader reader, Func<BackendRow, Fin<T>> shape) {
        List<T> rows = [];
        BackendRow row = new BackendRow.Ado(reader);
        while (await reader.ReadAsync().ConfigureAwait(false)) {
            Fin<T> shaped = shape(row);
            if (shaped.IsFail) { return shaped.Map(static _ => Seq<T>()); }
            shaped.Iter(held => rows.Add(held));
        }
        return Fin.Succ(toSeq(rows));
    }

    static Fin<Seq<T>> Batched<T>(RecordBatch batch, Func<BackendRow, Fin<T>> shape) =>
        toSeq(Range(0, batch.Length)).TraverseM(ordinal => shape(new BackendRow.Arrow(batch, ordinal))).As();
}

public static class BackendLanding {
    public static Seq<ColumnRow> Payload(AnalyticsSchema schema) => schema.Columns;

    public static Seq<ColumnRow> Supplied(AnalyticsSchema schema) =>
        schema.Spine == TimeSpine.Landing
            ? Payload(schema).Filter(column => column.Name != schema.Time)
            : Payload(schema);

    static Validation<Error, Option<(Identifier Name, NpgsqlDbType Wire)>> Landed(AnalyticsSchema schema) =>
        schema.Spine == TimeSpine.Event
            ? Success<Error, Option<(Identifier, NpgsqlDbType)>>(None)
            : schema.Columns.Find(column => column.Name == schema.Time)
                .ToValidation((Error)new BackendFault.Unprovisioned($"<schema-spine:{schema.Dataset}>"))
                .Bind(column => column.Type.Wire.ToValidation().Map(wire => Some((schema.Time, wire))));

    public static IO<Fin<BackendWrite>> Stage(
        NpgsqlDataSource store, AnalyticsSchema schema, Seq<Seq<ColumnCell>> rows, ProjectionContext frame) =>
        (Conformed(schema, Supplied(schema), rows), Landed(schema))
            .Apply(static (bound, landed) => (Bound: bound, Landed: landed)).As().ToFin().Match(
            Succ: proved => IO.liftAsync(async () => (await Op.Of().Catch(async token => {
                string columns = string.Join(", ",
                    (Seq(Backend.TenantColumn) + proved.Bound.Map(static entry => entry.Column.Name)
                        + proved.Landed.Map(static stamp => stamp.Name).ToSeq()).Map(Backend.Series.Quote));
                await using NpgsqlConnection lane = await store.OpenConnectionAsync(token).ConfigureAwait(false);
                await using NpgsqlBinaryImporter importer = await lane.BeginBinaryImportAsync(
                    $"COPY {Backend.Series.Quote(schema.Table)} ({columns}) FROM STDIN (FORMAT BINARY)", token).ConfigureAwait(false);
                byte[] tenant = ColumnCell.Packed(frame.Tenant.TenantId.Value);
                Instant landedAt = frame.Now();
                ColumnCell stamp = new ColumnCell.Moment(landedAt);
                foreach (Seq<ColumnCell> row in rows) {
                    await importer.StartRowAsync(token).ConfigureAwait(false);
                    await importer.WriteAsync(tenant, ColumnType.KeyHex.Wire, token).ConfigureAwait(false);
                    foreach ((ColumnCell Cell, (ColumnRow Column, NpgsqlDbType Wire) Bound) pair in row.Zip(proved.Bound)) {
                        await Bind(pair.Bound.Column.Type, pair.Cell, importer, pair.Bound.Wire).ConfigureAwait(false);
                    }
                    foreach ((Identifier Name, NpgsqlDbType Wire) landed in proved.Landed.ToSeq()) {
                        await ColumnType.Timestamp.Cell.Stage(stamp, importer, landed.Wire).ConfigureAwait(false);
                    }
                }
                ulong staged = await importer.CompleteAsync(token).ConfigureAwait(false);
                return Fin<BackendWrite>.Succ(new BackendWrite(schema.Dataset, (long)staged, landedAt, frame.Correlation));
            }).ConfigureAwait(false)).MapFail(Backend.Series.IngestRefused)),
            Fail: error => IO.pure(Fin<BackendWrite>.Fail(error)));

    static Validation<Error, Seq<(ColumnRow Column, NpgsqlDbType Wire)>> Conformed(
        AnalyticsSchema schema, Seq<ColumnRow> supplied, Seq<Seq<ColumnCell>> rows) =>
        rows.Exists(row => row.Count != supplied.Count)
            ? Fail<Error, Seq<(ColumnRow, NpgsqlDbType)>>(new BackendFault.IngestRefused(
                Backend.Series.Key, new EngineFault("<row-arity>", schema.Dataset)))
            : (rows.Traverse(row => row.Zip(supplied)
                    .Traverse(pair => pair.Item2.Admits(pair.Item1).ToValidation()).As()).As(),
               supplied.Traverse(column => column.Type.Wire.ToValidation().Map(wire => (column, wire))).As())
                .Apply(static (_, bound) => bound).As();

    static Task Bind(ColumnShape shape, ColumnCell cell, NpgsqlBinaryImporter importer, NpgsqlDbType wire) => shape.Switch(
        state:      (Cell: cell, Importer: importer, Wire: wire),
        scalar:     static (s, c) => s.Cell is ColumnCell.Absent
            ? s.Importer.WriteNullAsync()
            : c.Type.Cell.Stage(s.Cell, s.Importer, s.Wire),
        dictionary: static (s, c) => c.Element.Cell.Stage(s.Cell, s.Importer, s.Wire),
        list:       static (s, _) => s.Importer.WriteAsync(((ColumnCell.Items)s.Cell).Values.ToArray(), s.Wire),
        fixedList:  static (s, _) => s.Importer.WriteAsync(((ColumnCell.Items)s.Cell).Values.ToArray(), s.Wire),
        map:        static (s, _) => s.Importer.WriteAsync(
            JsonSerializer.Serialize(((ColumnCell.Tags)s.Cell).Pairs.ToDictionary(static pair => pair.Key, static pair => pair.Value)), s.Wire));
}
```

| [INDEX] | [POLICY]           | [VALUE]                                     | [BINDING]                                                    |
| :-----: | :----------------- | :------------------------------------------ | :----------------------------------------------------------- |
|  [01]   | serving discipline | one open-execute-drain-result order         | a reach is two rows; never a per-reach body                  |
|  [02]   | fleet leg          | READ row + `QueryStats` scanned figure      | the egress sink owns landing; never a second SoR             |
|  [03]   | lake reach         | Flight SQL cross-runtime, DuckDB in-process | one plane per the Tier-0 ruling; never a sidecar transport   |
|  [04]   | total column read  | `Fin` readers over both row arms            | absence is a refusal; never an empty string, a zero, or 1970 |
|  [05]   | elapsed measure    | the frame's own `Mark`/`Elapsed` pair       | one clock per read; never a second stopwatch per leg         |
|  [06]   | relational landing | one `BackendLanding.Stage` binary COPY      | `SeriesLane.Ingest` is its arm; never a second importer body |
|  [07]   | cell conformance   | `ColumnRow.Admits`, accumulating, pre-copy  | one gate with the batch fold; the importer infers nothing    |
|  [08]   | landing tenancy    | the frame's tenant and admission instant    | read once per batch; never a producer-supplied row column    |

## [04]-[RESEARCH]

(none)
