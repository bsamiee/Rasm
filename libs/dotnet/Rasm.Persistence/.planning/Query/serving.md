# [PERSISTENCE_QUERY_SERVING]

Rasm.Persistence serves every analytics residence through one plan and one transport dispatch. `ResidenceScope` carries the four coordinates a scan is bounded by, `ResidencePlan` lowers one Substrait plan into each residence's own dialect, `ResidenceReach` decides the transport, and `ResidenceLanding` is the relational write peer of that read. Every declaration the plan addresses, every column it binds, and every cell it stages comes from `Query/residence#COLUMN_VOCABULARY` — this page reads that declaration and never re-states it.

Read SCOPE parts from read SHAPE here: tenant and window ride the scope value while the plan carries filters, projections, and folds alone, so an unbounded or cross-tenant residence scan has no shape that expresses it.

## [01]-[INDEX]

- [02]-[READ_PLAN]: `ResidenceWindow` and `ResidenceScope` bound every scan, `ResidenceFold` names the folds a question carries, and `ResidencePlan` lowers one Substrait plan per dialect — the shipped operator catalogs beside this custodian's own residence folds.
- [03]-[SERVING_PLANE]: `ResidenceReach` closes the transports, `ResidenceRow` is the one row surface every reach yields, `ResidenceRead` runs the ONE open-execute-drain-result discipline, and `ResidenceLanding` stages the binary COPY under the same conformance gate the record-batch fold reads.
- [04]-[RESEARCH]: open verification debts and their routes.

## [02]-[READ_PLAN]

- Owner: `ResidenceWindow` is the half-open read window; `ResidenceScope` is the ONE read frame carrying residence, schema, window, and causal frame together; `ResidenceFold` is the closed vocabulary a caller names when it asks for a projected or grouped figure; `ResidencePlan` is the ONE Substrait `RelationVisitor` lowering a logical plan per dialect and the ONE plan builder every consuming page composes instead of writing SQL.
- Cases: `ResidenceFold` is `Plain` (a declared column projected as itself), `Simple` (a shipped-catalog aggregate over one column), `Bucket` (a caller-stated grain), `Quantile` (a fraction under the convention the residence answers), `Weighted` (the toolkit time-weighted mean over raw chunks), `Mean` (the accessor over a materialised weight summary), and `Tail` (the accessor over a materialised sketch).
- Entry: `ResidencePlan.Scan(AnalyticsSchema, Seq<(Identifier Column, string Value)> matches)` builds the filter-only plan; `Project(schema, matches, Seq<(Identifier Name, ResidenceFold Fold)> columns, Seq<Identifier> order)` builds the projected read; `Aggregate(schema, matches, Seq<Identifier> keys, Seq<(Identifier Name, ResidenceFold Fold)> folds)` builds the grouped read; `Lower(Plan, ResidenceScope, ResidenceProjection)` is the one lowering, gating the window and the residence's declared projection subset ahead of every relation.
- Auto: the plan is the query currency and the residence row supplies the tokens, so a question written once renders three ways and no second query language enters. Every arm recurses through `Visit`, never `Accept`, so the admitted-relation test runs once per node and an unadmitted kind returns the typed refusal rather than reaching a base arm that throws. Grouping keys thread DOWN into the fold, so a windowed aggregate re-buckets at the caller's grain rather than silently answering the residence's own storage grain.
- Receipt: lowering carries no slot of its own — the lowered text rides the `store.columnar.residence.read` receipt the serving plane lands.
- Packages: FlowtideDotNet.Substrait (`Plan`/`Relation`/`RelationVisitor<TReturn,TState>`/`ReadRelation`/`FilterRelation`/`ProjectRelation`/`AggregateRelation`/`AggregateMeasure`/`SortRelation`/`FetchRelation`/`TopNRelation`/`RootRelation`/`NamedTable.Names`/`NamedStruct.Names`/`DirectFieldReference`/`StructReferenceSegment.Field`/`ScalarFunction`/`AggregateFunction`/`SortField`/`SortDirection`/`Literals.NumericLiteral`/`Literals.StringLiteral`/`Literals.BoolLiteral`/`FunctionsComparison`/`FunctionsArithmetic`/`FunctionsAggregateGeneric`), Rasm (`Domain/stats#SCALAR_CARRIER` `QuantileRule` — the quantile convention a reader states rather than inherits), Rasm.Persistence (`Query/residence#COLUMN_VOCABULARY` `AnalyticsSchema`/`ColumnType.Plan`, `#RESIDENCE_FAMILY` `Residence`/`ResidenceProjection`/`ResidenceFault`, `#PROVISIONING` `SeriesResidence` — the materialised summary the toolkit folds read), NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new relation kind is one `Visit*` override beside its row in the admitted test; a new comparison or arithmetic operator is one row on `Operators` or `Postfixes`; a new shipped aggregate is one row on `Folds`; a new residence-owned fold is one row on `Residents` beside its `ResidenceFold` case, which breaks every builder at compile time; zero new surface — a second query language, a raw-SQL reader, a per-residence lowering, a hand-assembled relation at a consuming page, or a fold spelled at a call site is the deleted form.
- Law: the two fold CLASSES answer differently. Dialect folds — `time_bucket`, `quantile` — read the residence row's own delegate column, so every residence spells one or declines it by not publishing the projection it serves. Toolkit folds — `time_weight`, `weight_average`, `sketch_quantile` — read state only a provisioning arm that materialises the summary produces, so a residence whose arm emits no continuous aggregate refuses `Unlowerable` rather than lowering a naive `avg` that over-counts a dense burst under the caption the weighted mean earned.
- Boundary: the quantile fold carries the kernel `QuantileRule` the caller states, and every residence spelling — `percentile_cont` exactly, `quantileTDigest` and `approx_quantile` approximately — answers the INTERPOLATED convention, so a `NearestRank` request refuses at lowering rather than silently answering a definition the sample never contained. Kernel `Stat<TCarrier>` and `Distribution<TCarrier>` stay caller-side carriers and reach no arm here: a residence computes its figure in-engine off materialised state carrying no central moments, so the shared vocabulary is the RULE and never the summary. Field references arrive as ORDINALS a foreign plan carries, so resolution is fallible by construction — an ordinal past the roster refuses typed here, where an index into the column list throws straight out of the `Fin` fold. Residence folds ride this custodian's OWN extension URI: a backend resolving the shipped catalogs alone answers nothing for them, which is what keeps two estate folds from squatting on an upstream name.

```csharp signature
using Apache.Arrow;
using FlowtideDotNet.Substrait;
using FlowtideDotNet.Substrait.Expressions;
using FlowtideDotNet.Substrait.Expressions.Literals;
using FlowtideDotNet.Substrait.FunctionExtensions;
using FlowtideDotNet.Substrait.Relations;
using FlowtideDotNet.Substrait.Type;
using LanguageExt;
using NodaTime;
using Rasm.Domain;                                // QuantileRule — the quantile convention a residence answers
using Rasm.Persistence.Element;                   // ProjectionContext — the ruled time-and-causal frame
using System.Collections.Frozen;
using System.Globalization;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// Read SCOPE, distinct from read SHAPE: the tenant arrives on the frame and the window arrives here, so a Substrait
// plan carries filters, projections, and folds alone and never the two coordinates every residence prunes on.
public readonly record struct ResidenceWindow(Instant From, Instant Until);

// ONE read scope every entry on this plane takes: residence, schema, window, and frame always travel together, so a
// new coordinate widens one value rather than every signature. The lowering reads it as its visitor state and the
// read reads it as its scope, which keeps `Lower` and `Read` from drifting on what a scan is bounded by.
public sealed record ResidenceScope(Residence Residence, AnalyticsSchema Schema, ResidenceWindow Window, ProjectionContext Frame) {
    public Fin<string> Column(int ordinal) =>
        ordinal >= 0 && ordinal < Schema.Columns.Count
            ? Fin.Succ(Residence.Quote(Schema.Columns[ordinal].Name))
            : Fin.Fail<string>(new ResidenceFault.Unlowerable(Residence.Key, $"<field-ordinal:{ordinal}>"));

    // Tenant predicate beside the half-open window, both in the residence's own literal dialect: the leading sort-key
    // column prunes to one tenant's granules and the trailing time column prunes the window.
    public string Scope =>
        $"{Residence.Partition(Frame.Tenant)} AND {Residence.Quote(Schema.Time)} >= {Residence.Moment(Window.From)}"
        + $" AND {Residence.Quote(Schema.Time)} < {Residence.Moment(Window.Until)}";
}

// What a caller NAMES when it asks for a figure. The builder resolves each against the declared roster and mints the
// Substrait node, so no consuming page assembles a relation, spells an extension name, or writes a fold's text.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResidenceFold {
    private ResidenceFold() { }
    public sealed record Plain(Identifier Column) : ResidenceFold;
    public sealed record Simple(string ExtensionName, Identifier Column) : ResidenceFold;
    public sealed record Bucket(Identifier Column, Duration Grain) : ResidenceFold;
    public sealed record Quantile(Identifier Column, double Fraction, QuantileRule Rule) : ResidenceFold;
    public sealed record Weighted(Identifier Column) : ResidenceFold;
    // Two accessor folds name NO column: weight and sketch are the provisioning arm's own materialised
    // state, so they read off that owner's declaration and never enter a dataset's column roster, where a producer
    // would have to declare a physical type the neutral vocabulary carries no token for.
    public sealed record Mean : ResidenceFold;
    public sealed record Tail(double Fraction, QuantileRule Rule) : ResidenceFold;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public sealed class ResidencePlan : RelationVisitor<Fin<string>, ResidenceScope> {
    // Comparison and arithmetic extension names are consts on the shipped catalogs, so a renamed upstream function
    // breaks the build rather than silently lowering to a spelling no backend resolves.
    static readonly FrozenDictionary<string, string> Operators =
        new Dictionary<string, string>(StringComparer.Ordinal) {
            [FunctionsComparison.Equal] = "=", [FunctionsComparison.NotEqual] = "<>",
            [FunctionsComparison.GreaterThan] = ">", [FunctionsComparison.GreaterThanOrEqual] = ">=",
            [FunctionsComparison.LessThan] = "<", [FunctionsComparison.LessThanOrEqual] = "<=",
            [FunctionsArithmetic.Add] = "+", [FunctionsArithmetic.Subtract] = "-",
            [FunctionsArithmetic.Multiply] = "*", [FunctionsArithmetic.Divide] = "/",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    // Null tests are POSTFIX rows on the same table discipline the infix glyphs ride, so the two shapes are two
    // rosters and one arm each rather than a per-function arm accreting down the switch.
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

    // --- [RESIDENCE_FOLDS]
    // Residence folds name what the shipped catalogs cannot, because each carries an ARGUMENT no bare aggregate over a column
    // carries: a bucket its grain, a quantile its fraction, a weighted mean its interval rule, an accessor the
    // materialised state it reads. Each rides this custodian's own extension URI and lowers through the owner that
    // spells it, so no consuming page writes the text and no upstream name is squatted.
    public const string ResidenceUri = "https://rasm.dev/substrait/residence.yaml";
    public const string BucketFold = "time_bucket";
    public const string QuantileFold = "quantile";
    public const string KeyFold = "key_equals";
    public const string WeightFold = "time_weight";
    public const string MeanFold = "weight_average";
    public const string TailFold = "sketch_quantile";

    static readonly FrozenDictionary<string, Func<Seq<Expression>, ResidenceScope, Fin<string>>> Residents =
        new Dictionary<string, Func<Seq<Expression>, ResidenceScope, Fin<string>>>(StringComparer.Ordinal) {
            [BucketFold] = static (arguments, state) => Grained(arguments, state)
                .Map(held => state.Residence.Bucket(held.Column, Duration.FromSeconds(held.Magnitude))),
            [QuantileFold] = static (arguments, state) => Graded(arguments, state, QuantileFold)
                .Map(held => state.Residence.Quantile(held.Column, held.Magnitude)),
            // Content keys compare through the residence's own LITERAL spelling for the same reason a tenant does: its
            // physical form differs per engine and a quoted hex text against a `bytea` or a `FixedString(16)`
            // matches nothing and raises nothing, which is why the key row carries no Substrait literal at all.
            [KeyFold] = static (arguments, state) => Keyed(arguments, state)
                .Map(held => $"({held.Column} = {state.Residence.Literal(held.Hex)})"),
            // Toolkit folds read state only a provisioning arm that materialises the summary produces. `SeriesResidence`
            // is the one arm emitting a continuous aggregate, so the guard names that owner rather than a residence key
            // — and a residence gaining its own summary earns these folds by publishing that arm, not by an arm added here.
            [WeightFold] = static (arguments, state) => Toolkit(arguments, state, WeightFold).Bind(column =>
                Summarised(state, WeightFold).Map(_ =>
                    $"average(time_weight('linear', {state.Residence.Quote(state.Schema.Time)}, {column}))")),
            [MeanFold] = static (arguments, state) => arguments.IsEmpty
                ? Summarised(state, MeanFold).Map(_ => $"average({state.Residence.Quote(SeriesResidence.Weight)})")
                : Unarity(MeanFold, arguments.Count, 0, state),
            [TailFold] = static (arguments, state) => Sketched(arguments, state).Bind(fraction =>
                Summarised(state, TailFold).Map(_ =>
                    $"approx_percentile({fraction.ToString("0.####", CultureInfo.InvariantCulture)}, {state.Residence.Quote(SeriesResidence.Sketch)})")),
        }.ToFrozenDictionary(StringComparer.Ordinal);

    // Guarded entry: every arm recurses through `Visit`, never `Accept`, so the admitted-relation test runs once per
    // node and an unadmitted kind never reaches the base arm that throws.
    public override Fin<string> Visit(Relation relation, ResidenceScope state) =>
        relation is ReadRelation or FilterRelation or ProjectRelation or AggregateRelation or SortRelation or FetchRelation or TopNRelation or RootRelation
            ? base.Visit(relation, state)
            : Fin.Fail<string>(new ResidenceFault.Unlowerable(state.Residence.Key, relation.GetType().Name));

    // Reads are tenant-scoped and window-bounded: this arm takes both coordinates off the frame, so no filter is ever the
    // only thing separating tenants and a plan missing its window cannot fall through to a full-history scan.
    public override Fin<string> VisitReadRelation(ReadRelation readRelation, ResidenceScope state) =>
        Named(readRelation, state).Map(relation => $"SELECT * FROM {relation} WHERE {state.Scope}");

    // Every plan names the relation it reads on its OWN table, not the schema's: a rollup read and a raw read differ
    // by relation while sharing every other coordinate, so a builder naming one is not a second lowering.
    static Fin<string> Named(ReadRelation readRelation, ResidenceScope state) =>
        toSeq(readRelation.NamedTable?.Names ?? []).Last.Match(
            Some: name => Identifier.Validate(name, null, out Identifier admitted) is { } fault
                ? Fin.Fail<string>(fault)
                : Fin.Succ(state.Residence.Quote(admitted)),
            None: () => Fin.Fail<string>(new ResidenceFault.Unlowerable(state.Residence.Key, "<unnamed-relation>")));

    public override Fin<string> VisitFilterRelation(FilterRelation filterRelation, ResidenceScope state) =>
        from inner in Visit(filterRelation.Input, state)
        from where in Predicate(filterRelation.Condition, state)
        select $"SELECT * FROM ({inner}) AS leg WHERE {where}";

    // Emitted columns carry POSITIONAL slot aliases, because a lowered fold is an expression with no name of its own
    // and an outer relation addressing it by the declared name would name a column the inner leg never emitted. The
    // root then renames each slot to the caller's declared name, so declaration order stays the reader's ordinal order.
    public override Fin<string> VisitProjectRelation(ProjectRelation projectRelation, ResidenceScope state) =>
        from inner in Visit(projectRelation.Input, state)
        from columns in toSeq(projectRelation.Expressions).TraverseM(expression => Predicate(expression, state)).As()
        select $"SELECT {Slotted(columns)} FROM ({inner}) AS leg";

    static string Slot(int ordinal) => $"c{ordinal.ToString(CultureInfo.InvariantCulture)}";
    static string Slotted(Seq<string> parts) => string.Join(", ", parts.Map(static (part, index) => $"{part} AS {Slot(index)}"));

    // Grouping keys thread DOWN into the fold, so a windowed aggregate re-buckets at the caller's grain rather than
    // silently answering the residence's own storage grain.
    public override Fin<string> VisitAggregateRelation(AggregateRelation aggregateRelation, ResidenceScope state) =>
        from inner in Visit(aggregateRelation.Input, state)
        from keys in toSeq(aggregateRelation.Groupings ?? []).Bind(static grouping => toSeq(grouping.GroupingExpressions))
            .TraverseM(expression => Predicate(expression, state)).As()
        from folds in toSeq(aggregateRelation.Measures ?? []).TraverseM(measure => Fold(measure, state)).As()
        select keys.IsEmpty
            ? $"SELECT {Slotted(folds)} FROM ({inner}) AS leg"
            : $"SELECT {Slotted(keys + folds)} FROM ({inner}) AS leg GROUP BY {string.Join(", ", keys)}";

    // Ordering resolves against whatever its input EMITS: over a slot-aliased leg a field reference addresses the slot,
    // and over a passthrough leg it addresses the declared column — one test at the site the input is known.
    public override Fin<string> VisitSortRelation(SortRelation sortRelation, ResidenceScope state) =>
        from inner in Visit(sortRelation.Input, state)
        from order in toSeq(sortRelation.Sorts)
            .TraverseM(field => Ordered(field, state, sortRelation.Input is ProjectRelation or AggregateRelation)).As()
        select $"SELECT * FROM ({inner}) AS leg ORDER BY {string.Join(", ", order)}";

    public override Fin<string> VisitFetchRelation(FetchRelation fetchRelation, ResidenceScope state) =>
        from inner in Visit(fetchRelation.Input, state)
        from bound in Bounded(fetchRelation.Count, fetchRelation.Offset, state)
        select $"SELECT * FROM ({inner}) AS leg {bound}";

    public override Fin<string> VisitTopNRelation(TopNRelation topNRelation, ResidenceScope state) =>
        from inner in Visit(topNRelation.Input, state)
        from order in toSeq(topNRelation.Sorts)
            .TraverseM(field => Ordered(field, state, topNRelation.Input is ProjectRelation or AggregateRelation)).As()
        from bound in Bounded(topNRelation.Count, topNRelation.Offset, state)
        select $"SELECT * FROM ({inner}) AS leg ORDER BY {string.Join(", ", order)} {bound}";

    // ONE bound fragment both fetch-shaped arms render: a negative count or offset is a plan every dialect answers
    // differently — PostgreSQL and ClickHouse raise, DuckDB coerces — so it refuses typed at lowering.
    static Fin<string> Bounded(int count, int offset, ResidenceScope state) =>
        count >= 0 && offset >= 0
            ? Fin.Succ($"LIMIT {count} OFFSET {offset}")
            : Fin.Fail<string>(new ResidenceFault.Unlowerable(state.Residence.Key, $"<fetch-bounds:{count}:{offset}>"));

    // Root relations name the output columns, so the caller's shape reader binds by ordinal against a projection the
    // plan declared rather than against whatever the innermost leg happened to emit.
    public override Fin<string> VisitRootRelation(RootRelation rootRelation, ResidenceScope state) =>
        from inner in Visit(rootRelation.Input, state)
        from names in toSeq(rootRelation.Names).TraverseM(name =>
            Identifier.Validate(name, null, out Identifier admitted) is { } fault
                ? Fin.Fail<string>(fault)
                : Fin.Succ(state.Residence.Quote(admitted))).As()
        select $"SELECT {string.Join(", ", names.Map(static (name, index) => $"{Slot(index)} AS {name}"))} FROM ({inner}) AS root";

    // --- [EXPRESSIONS]
    // Field references resolve through the admitted schema by ordinal, literals render in their own invariant form,
    // and a function resolves through the operator table or this custodian's own residence roster — an unmapped
    // extension name is a typed refusal, never a spelled fallback the backend rejects at parse time.
    static Fin<string> Predicate(Expression expression, ResidenceScope state) => expression switch {
        DirectFieldReference { ReferenceSegment: StructReferenceSegment segment } =>
            state.Column(segment.Field),
        NumericLiteral literal => Fin.Succ(literal.Value.ToString(CultureInfo.InvariantCulture)),
        StringLiteral literal => Fin.Succ($"'{literal.Value.Replace("'", "''", StringComparison.Ordinal)}'"),
        BoolLiteral literal => Fin.Succ(literal.Value ? "TRUE" : "FALSE"),
        ScalarFunction call when call.ExtensionUri == ResidenceUri =>
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
        _ => Fin.Fail<string>(new ResidenceFault.Unlowerable(state.Residence.Key, expression.GetType().Name)),
    };

    // Aggregate measures read the SAME two rosters the scalar arm reads, so a fold usable as a projected accessor and the
    // same fold usable under a `GROUP BY` are one row rather than two tables that drift.
    static Fin<string> Fold(AggregateMeasure measure, ResidenceScope state) =>
        measure.Measure.ExtensionUri == ResidenceUri
            ? Resident(measure.Measure.ExtensionName, toSeq(measure.Measure.Arguments), state)
            : Folds.TryGetValue(measure.Measure.ExtensionName, out string? verb)
                ? toSeq(measure.Measure.Arguments).TraverseM(argument => Predicate(argument, state)).As()
                    .Map(parts => $"{verb}({string.Join(", ", parts)})")
                : Fin.Fail<string>(new ResidenceFault.Unlowerable(state.Residence.Key, measure.Measure.ExtensionName));

    static Fin<string> Resident(string name, Seq<Expression> arguments, ResidenceScope state) =>
        Residents.TryGetValue(name, out Func<Seq<Expression>, ResidenceScope, Fin<string>>? render)
            ? render(arguments, state)
            : Fin.Fail<string>(new ResidenceFault.Unlowerable(state.Residence.Key, $"<residence-fold:{name}>"));

    // Arity is proven BEFORE any argument lowers, because the malformed shapes are silent otherwise: a one-argument
    // comparison renders a bare operand the engine parses as a column, and a zero-argument null test indexes past its
    // own list and throws straight out of the `Fin` fold the lowering exists to carry.
    static Fin<string> Unarity(string name, int found, int expected, ResidenceScope state) =>
        Fin.Fail<string>(new ResidenceFault.Unlowerable(state.Residence.Key, $"<arity:{name}:{found}:{expected}>"));

    static Fin<string> Ordered(SortField field, ResidenceScope state, bool slotted) =>
        (slotted && field.Expression is DirectFieldReference { ReferenceSegment: StructReferenceSegment segment }
            ? Fin.Succ(Slot(segment.Field))
            : Predicate(field.Expression, state))
        .Map(part => field.SortDirection switch {
            SortDirection.SortDirectionDescNullsFirst or SortDirection.SortDirectionDescNullsLast => $"{part} DESC",
            _ => $"{part} ASC",
        });

    // --- [FOLD_ARGUMENTS]
    // Every residence fold's magnitude rides a NUMERIC literal beside its column reference, so the pair proves once here
    // and each render body stays total. Reading the magnitude off the plan rather than off a builder capture is what
    // lets a foreign plan carry the same fold.
    static Fin<(string Column, double Magnitude)> Paired(Seq<Expression> arguments, ResidenceScope state, string fold) =>
        arguments.ToArray() is [Expression column, NumericLiteral magnitude]
            ? Predicate(column, state).Map(held => (Column: held, Magnitude: (double)magnitude.Value))
            : Fin.Fail<(string, double)>(new ResidenceFault.Unlowerable(state.Residence.Key, $"<residence-fold:{fold}:{arguments.Count}>"));

    // Grain crosses as SECONDS and admits positives alone: a zero or negative interval buckets every row into one group on Postgres
    // and DuckDB and raises on ClickHouse, so the three answers become one typed refusal.
    static Fin<(string Column, double Magnitude)> Grained(Seq<Expression> arguments, ResidenceScope state) =>
        Paired(arguments, state, BucketFold).Bind(held => held.Magnitude > 0
            ? Fin.Succ(held)
            : Fin.Fail<(string, double)>(new ResidenceFault.Unlowerable(state.Residence.Key, $"<bucket-grain:{held.Magnitude}>")));

    // Fractions cross beside the kernel `QuantileRule` they were asked under, and every residence spelling answers the
    // INTERPOLATED convention — `percentile_cont` exactly, the two sketch tiers approximately. `NearestRank` answers an
    // observation the sample contains and no residence spells `percentile_disc`, so the request refuses here rather
    // than returning a neighbouring value under the caption the exact convention earned.
    static Fin<(string Column, double Magnitude)> Graded(Seq<Expression> arguments, ResidenceScope state, string fold) =>
        arguments.ToArray() is [Expression column, NumericLiteral literal, StringLiteral rule]
            ? Convention(rule.Value, state).Bind(_ => Fraction((double)literal.Value, state)
                .Bind(fraction => Predicate(column, state).Map(held => (Column: held, Magnitude: fraction))))
            : Fin.Fail<(string, double)>(new ResidenceFault.Unlowerable(state.Residence.Key, $"<residence-fold:{fold}:{arguments.Count}>"));

    static Fin<double> Sketched(Seq<Expression> arguments, ResidenceScope state) =>
        arguments.ToArray() is [NumericLiteral literal, StringLiteral rule]
            ? Convention(rule.Value, state).Bind(_ => Fraction((double)literal.Value, state))
            : Fin.Fail<double>(new ResidenceFault.Unlowerable(state.Residence.Key, $"<residence-fold:{TailFold}:{arguments.Count}>"));

    // Every residence quantile spelling answers the INTERPOLATED convention — `percentile_cont` exactly, the two
    // sketch tiers approximately. `NearestRank` answers an observation the sample CONTAINS and no residence spells
    // `percentile_disc`, so the request refuses rather than returning a neighbouring value under the exact caption.
    static Fin<Unit> Convention(string token, ResidenceScope state) =>
        QuantileRule.Validate(token, null, out QuantileRule? asked) is not null
            ? Fin.Fail<Unit>(new ResidenceFault.Unlowerable(state.Residence.Key, $"<quantile-rule:{token}>"))
            : asked == QuantileRule.Interpolated
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new ResidenceFault.Unanswerable(state.Residence.Key, ResidenceProjection.Quantile.Key,
                    $"every residence quantile spelling answers {QuantileRule.Interpolated.Key} alone"));

    static Fin<double> Fraction(double fraction, ResidenceScope state) =>
        fraction is >= 0 and <= 1
            ? Fin.Succ(fraction)
            : Fin.Fail<double>(new ResidenceFault.Unlowerable(state.Residence.Key, $"<quantile-fraction:{fraction}>"));

    static Fin<string> Toolkit(Seq<Expression> arguments, ResidenceScope state, string fold) =>
        arguments.ToArray() is [Expression column]
            ? Predicate(column, state)
            : Fin.Fail<string>(new ResidenceFault.Unlowerable(state.Residence.Key, $"<residence-fold:{fold}:{arguments.Count}>"));

    // Key narrowings carry their hex TEXT, which the residence's own literal renders; the column reference still
    // resolves through the declared roster, so an undeclared key column refuses exactly as any other does.
    static Fin<(string Column, string Hex)> Keyed(Seq<Expression> arguments, ResidenceScope state) =>
        arguments.ToArray() is [Expression column, StringLiteral hex]
            ? Predicate(column, state).Map(held => (Column: held, Hex: hex.Value))
            : Fin.Fail<(string, string)>(new ResidenceFault.Unlowerable(state.Residence.Key, $"<residence-fold:{KeyFold}:{arguments.Count}>"));

    // ONE summary gate the three toolkit folds share, naming the owner rather than a residence key: `SeriesResidence`
    // is the single provisioning arm emitting a continuous aggregate, so its rollup is the only materialised state
    // these accessors can read.
    static Fin<Unit> Summarised(ResidenceScope state, string fold) =>
        state.Residence.Statements == SeriesResidence.Statements
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ResidenceFault.Unlowerable(state.Residence.Key, $"<unsummarised:{fold}>"));

    // --- [PLAN_BUILDERS]
    // ONE plan builder every dataset shares in three shapes: callers name the narrowings, the columns, the grouping
    // keys, and the folds their question carries and take back the rooted plan, so relation assembly lives here once
    // and a page that would otherwise hand-write SQL reaches a typed shape. Scope stays off the plan — tenant and
    // window both enter at the read — so one plan serves every residence and every window.
    public static Fin<Plan> Scan(AnalyticsSchema schema, Seq<(Identifier Column, string Value)> matches) =>
        Project(schema, schema.Table, matches,
            schema.Columns.Map(static column => (column.Name, (ResidenceFold)new ResidenceFold.Plain(column.Name))),
            Seq<Identifier>());

    public static Fin<Plan> Project(AnalyticsSchema schema, Identifier relation, Seq<(Identifier Column, string Value)> matches,
        Seq<(Identifier Name, ResidenceFold Fold)> columns, Seq<Identifier> order) =>
        from shaped in columns.TraverseM(entry => Projected(schema, entry.Fold)).As()
        from plan in Build(schema, relation, matches, order, columns.Map(static entry => entry.Name),
            read => new ProjectRelation { Input = read, Expressions = [.. shaped] })
        select plan;

    public static Fin<Plan> Aggregate(AnalyticsSchema schema, Identifier relation, Seq<(Identifier Column, string Value)> matches,
        Seq<Identifier> keys, Seq<(Identifier Name, ResidenceFold Fold)> folds) =>
        from measures in folds.TraverseM(entry => Measured(schema, entry.Fold)).As()
        from grouping in keys.TraverseM(key => Reference(schema, key)).As()
        from plan in Build(schema, relation, matches, Seq<Identifier>(), keys + folds.Map(static entry => entry.Name),
            read => new AggregateRelation {
                Input = read,
                Groupings = [new AggregateGrouping { GroupingExpressions = [.. grouping] }],
                Measures = [.. measures],
            })
        select plan;

    // Assembly is FALLIBLE because every failure mode is silent otherwise: an undeclared column ordinals to -1 and
    // indexes out of the roster at lowering, and a value rendered as text against a numeric column raises on one
    // dialect and coerces on another. The three builders differ by the SHAPING relation alone, which arrives as the
    // one argument, so the read, the filter fold, the sort, and the root are assembled once.
    static Fin<Plan> Build(AnalyticsSchema schema, Identifier relation, Seq<(Identifier Column, string Value)> matches,
        Seq<Identifier> order, Seq<Identifier> names, Func<Relation, Relation> shape) =>
        from conditions in matches.TraverseM(match => Narrowed(schema, match)).As()
        // Ordering names an EMITTED column, never a base one: a sort over a fold the projection computed has no base
        // column to address, and a caller naming one that was never projected refuses here.
        from sorts in order.TraverseM(key => names.IndexOf(key) is var slot && slot >= 0
            ? Fin.Succ<Expression>(new DirectFieldReference { ReferenceSegment = new StructReferenceSegment { Field = slot } })
            : Fin.Fail<Expression>(new ResidenceFault.Unprovisioned($"<plan-order:{schema.Dataset}.{(string)key}>"))).As()
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

    // One narrowing is one admitted column ordinal beside the literal that column's own declared type renders, so a
    // temporal or key-typed match — the two Substrait carries no literal for — refuses here rather than lowering an
    // operand three engines each reject differently.
    static Fin<Expression> Narrowed(AnalyticsSchema schema, (Identifier Column, string Value) match) =>
        Reference(schema, match.Column).Bind(field =>
            schema.Columns[schema.Ordinal(match.Column)].Type.Plan(match.Value).Match(
                Some: literal => Fin.Succ<Expression>(new ScalarFunction {
                    ExtensionUri = FunctionsComparison.Uri,
                    ExtensionName = FunctionsComparison.Equal,
                    Arguments = [field, literal],
                }),
                // Key columns carry no Substrait literal by declaration, so a narrowing falls to the residence's
                // own literal spelling rather than refusing: a content-addressed read is the paradigm question this
                // family answers, and the previous form left it with no shape at all.
                None: () => schema.Columns[schema.Ordinal(match.Column)].Type is ColumnShape.Scalar scalar
                    && scalar.Type == ColumnType.KeyHex
                    ? Fin.Succ<Expression>(new ScalarFunction {
                        ExtensionUri = ResidenceUri,
                        ExtensionName = KeyFold,
                        Arguments = [field, new StringLiteral { Value = match.Value }],
                    })
                    : Fin.Fail<Expression>(new ResidenceFault.Unlowerable(
                        schema.Dataset, $"<literal:{schema.Dataset}.{(string)match.Column}>"))));

    static Fin<Expression> Reference(AnalyticsSchema schema, Identifier column) =>
        schema.Ordinal(column) is var ordinal && ordinal >= 0
            ? Fin.Succ<Expression>(new DirectFieldReference { ReferenceSegment = new StructReferenceSegment { Field = ordinal } })
            : Fin.Fail<Expression>(new ResidenceFault.Unprovisioned($"<schema-column:{schema.Dataset}.{(string)column}>"));

    // ONE resolution per fold: a plain column IS its own reference, and every other case is the (uri, name, arguments)
    // triple each builder wraps in the node its relation takes — a scalar function under a projection, an aggregate
    // function under a grouping — so the vocabulary resolves once and shapes twice.
    static Fin<Either<Expression, (string Uri, string Name, Seq<Expression> Arguments)>> Called(AnalyticsSchema schema, ResidenceFold fold) =>
        fold.Switch(
            state:    schema,
            plain:    static (declaration, c) => Reference(declaration, c.Column).Map(Either<Expression, (string, string, Seq<Expression>)>.Left),
            simple:   static (declaration, c) => Folds.ContainsKey(c.ExtensionName)
                ? Called(declaration, c.Column, FunctionsArithmetic.Uri, c.ExtensionName, Seq<Expression>())
                : Fin.Fail<Either<Expression, (string, string, Seq<Expression>)>>(
                    new ResidenceFault.Unlowerable(declaration.Dataset, $"<fold:{c.ExtensionName}>")),
            bucket:   static (declaration, c) => Called(declaration, c.Column, ResidenceUri, BucketFold, Seq(Magnitude(c.Grain.TotalSeconds))),
            quantile: static (declaration, c) => Called(declaration, c.Column, ResidenceUri, QuantileFold, Seq(Magnitude(c.Fraction), Rule(c.Rule))),
            weighted: static (declaration, c) => Called(declaration, c.Column, ResidenceUri, WeightFold, Seq<Expression>()),
            mean:     static (_, _) => Called(ResidenceUri, MeanFold, Seq<Expression>()),
            tail:     static (_, c) => Called(ResidenceUri, TailFold, Seq(Magnitude(c.Fraction), Rule(c.Rule))));

    static Fin<Either<Expression, (string Uri, string Name, Seq<Expression> Arguments)>> Called(
        string uri, string name, Seq<Expression> arguments) =>
        Fin.Succ(Either<Expression, (string, string, Seq<Expression>)>.Right((uri, name, arguments)));

    static Fin<Either<Expression, (string Uri, string Name, Seq<Expression> Arguments)>> Called(
        AnalyticsSchema schema, Identifier column, string uri, string name, Seq<Expression> tail) =>
        Reference(schema, column).Map(field =>
            Either<Expression, (string, string, Seq<Expression>)>.Right((uri, name, Seq(field) + tail)));

    static Fin<Expression> Projected(AnalyticsSchema schema, ResidenceFold fold) =>
        Called(schema, fold).Map(static held => held.Match(
            Left:  static reference => reference,
            Right: static call => (Expression)new ScalarFunction {
                ExtensionUri = call.Uri, ExtensionName = call.Name, Arguments = [.. call.Arguments],
            }));

    // Bare columns under a `GROUP BY` are grouping KEYS, never measures, so the builder that takes keys separately
    // refuses it here rather than emitting an aggregate the engine reports as an ungrouped column.
    static Fin<AggregateMeasure> Measured(AnalyticsSchema schema, ResidenceFold fold) =>
        Called(schema, fold).Bind(held => held.Match(
            Left:  _ => Fin.Fail<AggregateMeasure>(new ResidenceFault.Unlowerable(schema.Dataset, "<measure-plain-column>")),
            Right: call => Fin.Succ(new AggregateMeasure {
                Measure = new AggregateFunction { ExtensionUri = call.Uri, ExtensionName = call.Name, Arguments = [.. call.Arguments] },
            })));

    static Expression Magnitude(double value) => new NumericLiteral { Value = (decimal)value };
    static Expression Rule(QuantileRule rule) => new StringLiteral { Value = rule.Key };

    // One lowering entry: the root relation folds under the scope's own residence dialect, a plan naming a projection
    // that residence does not answer refuses carrying the row's `Degrade` clause, and an empty or inverted window
    // refuses ahead of both — a half-open window whose end precedes its start returns zero rows on every engine, which
    // a tile reads as a healthy quiet period rather than an unspellable scope.
    public static Fin<string> Lower(Plan plan, ResidenceScope scope, ResidenceProjection projection) =>
        scope.Window.Until <= scope.Window.From
            ? Fin.Fail<string>(new ResidenceFault.ReadRefused(scope.Residence.Key, new EngineFault("<read-window>", $"{scope.Window.From}..{scope.Window.Until}")))
            : !scope.Residence.Answers(projection)
                ? Fin.Fail<string>(new ResidenceFault.Unanswerable(scope.Residence.Key, projection.Key, scope.Residence.Degrade))
                : toSeq(plan.Relations).Last.Match(
                    Some: root => new ResidencePlan().Visit(root, scope),
                    None: () => Fin.Fail<string>(new ResidenceFault.Unlowerable(scope.Residence.Key, "<empty-plan>")));
}
```

| [INDEX] | [POLICY]          | [VALUE]                                     | [BINDING]                                                        |
| :-----: | :---------------- | :------------------------------------------ | :--------------------------------------------------------------- |
|  [01]   | query currency    | one Substrait `Plan` per question           | one lowering, three dialects; no second query language           |
|  [02]   | read scope        | frame tenant + `ResidenceWindow`            | the plan carries shape; an unbounded scan is unrepresentable     |
|  [03]   | narrowing literal | the column's own declared `ColumnType.Plan` | a text operand against a numeric column raises or coerces        |
|  [04]   | dialect fold      | `time_bucket`/`quantile` off the row        | every residence spells it or declines the projection it serves   |
|  [05]   | toolkit fold      | `time_weight`/accessors off the rollup      | gated on the arm that materialises the summary, never on a key   |
|  [06]   | quantile rule     | kernel `QuantileRule.Interpolated`          | `NearestRank` refuses; no residence spells `percentile_disc`     |
|  [07]   | relation naming   | the plan's own `NamedTable`                 | a rollup read and a raw read differ by relation, not by lowering |
|  [08]   | plan assembly     | `Scan`/`Project`/`Aggregate` builders       | no consuming page assembles a relation or spells an extension    |
|  [09]   | key narrowing     | the residence's own `Literal` spelling      | a key carries no plan literal; quoted hex matches no `bytea`     |

## [03]-[SERVING_PLANE]

- Owner: `ResidenceReach` closes the transports one read discriminates on; `ResidenceRow` is the ONE row surface every reach yields, with a `Fin` reader per physical family; `ResidenceReceipt`/`ResidenceResult<T>` carry the read's evidence and its payload; `ResidenceHealth` is the family policy-health row; `ResidenceRead` is the ONE query entry and the ONE health probe; `ResidenceIngestReceipt` carries the staged count and `ResidenceLanding` is the ONE relational landing.
- Cases: `ResidenceReach` is `Relational(NpgsqlDataSource)` | `Fleet(ClickHouseClient)` | `Flight(FlightSqlClient)` | `Local(ColumnarSession)`; `ResidenceRow` is `Ado(DbDataReader)` | `Arrow(RecordBatch, int Ordinal)`.
- Entry: `ResidenceRead.Read<T>(ResidenceReach, Plan, ResidenceScope, ResidenceProjection, Func<ResidenceRow, Fin<T>>)` is the ONE query entry over every residence; `ResidenceRead.Health(ResidenceReach, Residence, AnalyticsSchema, ProjectionContext)` is the FAMILY policy probe every residence answers over the same reach arms; `ResidenceLanding.Stage(NpgsqlDataSource, AnalyticsSchema, Seq<Seq<ColumnCell>>, ProjectionContext)` is the ONE relational landing.
- Auto: every reach runs ONE ordered discipline — OPEN the lease chain, EXECUTE the lowered text, DRAIN through the caller's one shape, REPORT the scanned figure only that leg can honestly supply — so the three ADO-shaped reaches are one body and three rows and the Arrow reach differs by its drain alone. No caller-supplied SQL string has a parameter to arrive on, which is what makes writer/reader drift and ad-hoc tenant scans unrepresentable. Landing derives its column list, its tenancy lead, and every wire type from the same `AnalyticsSchema` the DDL emitter provisions from, so a landed row and the table it lands in cannot drift on order, count, or physical type.
- Receipt: a residence read rides `store.columnar.residence.read` as the non-generic `ResidenceReceipt` carrying the residence key, the lowered text, the scanned rows, and the elapsed figure; an ingest rides `store.columnar.residence.ingest` as one `ResidenceIngestReceipt` naming its dataset beside the staged count.
- Packages: Npgsql (`NpgsqlDataSource.CreateCommand`/`OpenConnectionAsync`/`NpgsqlConnection.BeginBinaryImportAsync`/`NpgsqlBinaryImporter.StartRowAsync`/`WriteAsync`/`CompleteAsync`/`NpgsqlException`), ClickHouse.Driver (`ClickHouseClient.CreateConnection`/`ClickHouseCommand.ExecuteReaderAsync`/`QueryStats`/`ClickHouseServerException`), DuckDB.NET.Data.Full (`DuckDBConnection.Duplicate`/`DuckDBCommand.UseStreamingMode`/`DuckDBException`), Apache.Arrow.Flight.Sql (`FlightSqlClient.ExecuteAsync(string, Transaction)`/`DoGetAsync(FlightTicket)`/`Transaction.NoTransaction`), Apache.Arrow.Flight (`FlightInfo.Endpoints`/`TotalRecords`/`FlightEndpoint.Ticket`), Apache.Arrow (`RecordBatch`/`StringArray`/`Int64Array`/`DoubleArray`/`TimestampArray`/`ListArray.ValueOffsets`/`Values`), Rasm.Persistence (`Query/residence#COLUMN_VOCABULARY` `ColumnCell`/`ColumnShape.Admits`/`ColumnType.Cell`, `#RESIDENCE_FAMILY` `Residence`, `Element/graph#PROJECTION_FRAME` `ProjectionContext`), NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new transport is one `ResidenceReach` case breaking the read dispatch at compile time; a new ADO-shaped reach is one `AdoLeg` row carrying its lease chain and its scanned reader; a new physical family a shape reads is one `ResidenceRow` member answering both arms; zero new surface — a per-residence read entry, a per-reach shape delegate, a `DbDataReader`-typed shape the Arrow leg must fake, a second importer body, or a raw-SQL reader is the deleted form.
- Law: absence is a RAIL fact on both row arms — an Arrow primitive reads nullable by construction and a relational column reads `IsDBNull` ahead of its typed getter, so a shape needing a total column takes the refusal rather than an empty string, a zero, or a 1970 instant, the three sentinels a board renders indistinguishably from a measured reading. `QueryStats` is the ONLY honest scanned figure on the Fleet leg: the returned row count says nothing about the granules a predicate pruned, which is the whole reason the tenant leads the sort key.
- Boundary: conformance is ARITY, TYPE, CANONICITY, and WRITABILITY together and ACCUMULATES, all ahead of the copy — the binary importer infers nothing from a column list and a mismatch found at row n discards the n-1 rows already staged, so a producer learns every offending column from one refusal. It runs against `Supplied`, never `Payload` — against what the producer's contract obliges it to send — because counting the custodian's own stamped columns demands cells the category forbids a producer to carry and reads a correct producer as a defective one. `ColumnShape.Admits` is that gate, the same proof `ArrowLanding.Build` runs, so the batch fold and the COPY fold cannot disagree on whether a cell belongs to its column. Elapsed rides the frame's own monotonic mark: `ProjectionContext` is this package's ruled time-and-causal frame and already carries the `Mark`/`Elapsed` pair, so no leg starts a second clock. Cancellation stays a rail fact and propagates — a cooperative cancel converted to a fault reads as a failed landing.

```csharp signature
using Apache.Arrow;
using Apache.Arrow.Arrays;                        // FixedSizeBinaryArray — the packed key column both arms read
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
using Rasm.Domain;                                // CorrelationId — the causal key a receipt carries
using Rasm.Persistence.Element;                   // ProjectionContext — the mark/elapsed pair every leg reads
using System.Buffers.Binary;
using System.Data.Common;
using System.Globalization;
using System.Text.Json;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// Transport union: the READ discriminates on the reach VALUE's shape, never on a residence name or a mode flag, so a
// residence reachable two ways needs no second entry and a new transport breaks the dispatch at compile time.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResidenceReach {
    private ResidenceReach() { }
    public sealed record Relational(NpgsqlDataSource Source) : ResidenceReach;
    public sealed record Fleet(ClickHouseClient Client) : ResidenceReach;
    public sealed record Flight(FlightSqlClient Client) : ResidenceReach;
    public sealed record Local(ColumnarSession Session) : ResidenceReach;
}

// ONE row surface every reach yields, so the relational and Arrow legs are genuinely interchangeable — a
// `DbDataReader`-typed shape would force the Flight leg to fake a reader it cannot supply, and a per-reach shape
// delegate would fork the one entry this plane exists to hold.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResidenceRow {
    private ResidenceRow() { }
    public sealed record Ado(DbDataReader Reader) : ResidenceRow;
    public sealed record Arrow(RecordBatch Batch, int Ordinal) : ResidenceRow;

    public Fin<string> Text(Residence residence, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column) ? Option<string>.None : Some(c.Reader.GetString(column)),
        arrow: c => Optional(((StringArray)c.Batch.Column(column))[c.Ordinal]))
        .ToFin(Missing(residence, column));

    public Fin<long> Whole(Residence residence, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column) ? Option<long>.None : Some(c.Reader.GetInt64(column)),
        arrow: c => Optional(((Int64Array)c.Batch.Column(column)).GetValue(c.Ordinal)))
        .ToFin(Missing(residence, column));

    public Fin<double> Real(Residence residence, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column) ? Option<double>.None : Some(c.Reader.GetDouble(column)),
        arrow: c => Optional(((DoubleArray)c.Batch.Column(column)).GetValue(c.Ordinal)))
        .ToFin(Missing(residence, column));

    public Fin<Instant> At(Residence residence, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column)
            ? Option<Instant>.None
            : Some(Instant.FromDateTimeUtc(DateTime.SpecifyKind(c.Reader.GetDateTime(column), DateTimeKind.Utc))),
        arrow: c => Optional(((TimestampArray)c.Batch.Column(column))[c.Ordinal]).Map(Instant.FromDateTimeOffset))
        .ToFin(Missing(residence, column));

    // Key columns cross as sixteen big-endian bytes on both arms, so the read inverse of every `KeyHex` landing is one
    // member rather than a decode re-spelled at each dataset that stores an identity.
    public Fin<UInt128> Key(Residence residence, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column) ? Option<byte[]>.None : Some(c.Reader.GetFieldValue<byte[]>(column)),
        arrow: c => Optional(((FixedSizeBinaryArray)c.Batch.Column(column)).GetBytes(c.Ordinal).ToArray()))
        .ToFin(Missing(residence, column))
        .Map(static packed => BinaryPrimitives.ReadUInt128BigEndian(packed));

    // A `List(Utf8)` column reads as one ordered run on both arms: ADO hands the provider's text array whole, and the
    // Arrow arm slices the `Values` child between the row's two offsets — the only two members the catalog rows.
    public Fin<Seq<string>> Items(Residence residence, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column) ? Option<Seq<string>>.None : Some(toSeq(c.Reader.GetFieldValue<string[]>(column))),
        arrow: c => ((ListArray)c.Batch.Column(column)) is var list && !list.IsNull(c.Ordinal)
            ? Some(toSeq(Enumerable.Range(list.ValueOffsets[c.Ordinal], list.ValueOffsets[c.Ordinal + 1] - list.ValueOffsets[c.Ordinal])
                .Select(index => ((StringArray)list.Values).GetString(index))))
            : Option<Seq<string>>.None)
        .ToFin(Missing(residence, column));

    // One refusal spelling every reader shares, carrying the residence that answered and the ordinal that came back
    // empty, so a corrupt total column names itself instead of surfacing as a downstream parse failure.
    static ResidenceFault Missing(Residence residence, int column) =>
        new ResidenceFault.ReadRefused(residence.Key,
            new EngineFault("<null-column>", column.ToString(CultureInfo.InvariantCulture)));
}

// --- [MODELS] -----------------------------------------------------------------------------
// `ResidenceReceipt` is the NON-GENERIC projection the read slot carries, so the rows never cross the receipt wire and
// a consumer arrow handing back bare values loses no diagnosis.
public readonly record struct ResidenceReceipt(string Residence, string Lowered, long Scanned, Duration Elapsed);

public readonly record struct ResidenceResult<T>(Residence Residence, string Lowered, Seq<T> Rows, long Scanned, Duration Elapsed) {
    public ResidenceReceipt Receipt => new(Residence.Key, Lowered, Scanned, Elapsed);
}

// FAMILY health row: the resident time extent and cardinality of one residence's relation, the one evidence every tier
// produces because every tier partitions on time. `Retained` reads the expiry scheduler's OUTCOME — residue older than the
// declared horizon means the policy stopped firing, whichever engine owns it — and `Lag` reads the refresh's, so a
// stalled rollup surfaces as measured staleness rather than as an empty tile. An empty relation answers absence at
// both ends, because a zero instant reads as 1970 and would report every quiet stream as catastrophically stale.
public readonly record struct ResidenceHealth(string Residence, string Relation, Option<Instant> Oldest, Option<Instant> Newest, long Rows) {
    public bool Retained(ResidencePolicy policy, Instant now) => Oldest.Map(at => at >= now - policy.Retain).IfNone(true);
    public Duration Lag(Instant now) => Newest.Map(at => now - at).IfNone(Duration.Zero);
}

// Staged-count receipt: `Dataset` names the residence dataset a batch landed under, so the projection arm reads a
// DECLARED wire shape rather than a count with no subject — an ingest stream that stopped feeding is invisible when the
// receipt is a bare number.
public readonly record struct ResidenceIngestReceipt(string Dataset, long Staged, Instant At, CorrelationId Correlation);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ResidenceRead {
    // ONE query entry across every residence: the logical plan lowers once through the residence's dialect and the
    // reach value alone decides the transport.
    public static IO<Fin<ResidenceResult<T>>> Read<T>(
        ResidenceReach reach, Plan plan, ResidenceScope scope, ResidenceProjection projection, Func<ResidenceRow, Fin<T>> shape) =>
        ResidencePlan.Lower(plan, scope, projection).Match(
            Succ: lowered => Serve(reach, scope.Residence, lowered, shape, scope.Frame),
            Fail: error => IO.pure(Fin<ResidenceResult<T>>.Fail(error)));

    // Policy health rides the SAME reach arms every read takes, so the family gains its probe as one statement and one
    // row shape rather than a second transport ladder per residence.
    public static IO<Fin<ResidenceResult<ResidenceHealth>>> Health(
        ResidenceReach reach, Residence residence, AnalyticsSchema schema, ProjectionContext frame) =>
        Serve(reach, residence, residence.Horizon(schema), row => Shape(residence, schema, row), frame);

    // Row count gates the extent pair, because an empty relation answers absence at both ends and reads its extent
    // columns not at all. The pair ACCUMULATES: a relation whose two extent columns are both corrupt names both.
    static Fin<ResidenceHealth> Shape(Residence residence, AnalyticsSchema schema, ResidenceRow row) =>
        row.Whole(residence, 2).Bind(rows => rows == 0
            ? Fin.Succ(new ResidenceHealth(residence.Key, (string)schema.Table, None, None, rows))
            : (row.At(residence, 0).ToValidation<Error>(), row.At(residence, 1).ToValidation<Error>())
                .Apply((oldest, newest) => new ResidenceHealth(
                    residence.Key, (string)schema.Table, Some(oldest), Some(newest), rows)).As().ToFin());

    // --- [SERVING_DISCIPLINE]
    // ONE serving order for every reach: OPEN the lease chain, EXECUTE the lowered text, DRAIN through the caller's one
    // shape, REPORT the figure only that leg can honestly supply. The three ADO-shaped reaches differ by exactly those
    // two rows, so they are one body; the Arrow reach differs by its drain and shares the rest.
    static IO<Fin<ResidenceResult<T>>> Serve<T>(
        ResidenceReach reach, Residence residence, string lowered, Func<ResidenceRow, Fin<T>> shape, ProjectionContext frame) =>
        reach.Switch(
            relational: leg => Ado(Postgres(leg), residence, lowered, shape, frame),
            fleet:      leg => Ado(Clickhouse(leg), residence, lowered, shape, frame),
            local:      leg => Ado(Duck(leg), residence, lowered, shape, frame),
            flight:     leg => Arrow(leg, residence, lowered, shape, frame));

    // One lease chain and one scanned reader per ADO-shaped reach. `Report` is the row that matters: the two legs whose
    // driver publishes no scan figure answer with the rows they returned, and the one that does answers with it.
    readonly record struct AdoLeg(
        Func<string, ValueTask<(Seq<IAsyncDisposable> Leases, DbCommand Command)>> Open,
        Func<DbCommand, Seq<IAsyncDisposable>, long, Duration, (long Scanned, Duration Elapsed)> Report);

    static AdoLeg Postgres(ResidenceReach.Relational leg) => new(
        async lowered => (Seq<IAsyncDisposable>(), (DbCommand)leg.Source.CreateCommand(lowered)),
        static (_, _, returned, elapsed) => (returned, elapsed));

    // This leg rides the ADO mirror rather than the pooled client's own reader entry: `QueryStats` is a post-execution
    // property on the COMMAND and `DbConnection.CreateCommand` takes no text, so each command binds its connection at
    // construction and carries the lowered plan as its state.
    static AdoLeg Clickhouse(ResidenceReach.Fleet leg) => new(
        async lowered => {
            ClickHouseConnection lane = leg.Client.CreateConnection();
            await lane.OpenAsync().ConfigureAwait(false);
            return (Seq<IAsyncDisposable>(lane), new ClickHouseCommand(lane) { CommandText = lowered });
        },
        static (command, _, returned, elapsed) => ((ClickHouseCommand)command).QueryStats is { } stats
            ? ((long)stats.ReadRows, Duration.FromNanoseconds(stats.ElapsedNs))
            : (returned, elapsed));

    // Lake reads run in-process over the hive generation tree through the standing DuckDB anchor, so a report-grade
    // scan needs no Flight hop and the same lowered text serves both reaches.
    static AdoLeg Duck(ResidenceReach.Local leg) => new(
        async lowered => {
            DuckDBConnection lane = leg.Session.Lane();
            await lane.OpenAsync().ConfigureAwait(false);
            DuckDBCommand command = lane.CreateCommand();
            (command.CommandText, command.UseStreamingMode) = (lowered, true);
            return (Seq<IAsyncDisposable>(lane), command);
        },
        static (_, _, returned, elapsed) => (returned, elapsed));

    // Leases release in reverse acquisition order on every outcome — drained, refused, or cancelled — because a command
    // outliving its connection is exactly the leak a per-branch dispose leaves on the path nobody remembered.
    static IO<Fin<ResidenceResult<T>>> Ado<T>(
        AdoLeg leg, Residence residence, string lowered, Func<ResidenceRow, Fin<T>> shape, ProjectionContext frame) =>
        IO.liftAsync(async () => (await Op.Of().Catch(async token => {
            long mark = frame.Mark();
            (Seq<IAsyncDisposable> leases, DbCommand command) = await leg.Open(lowered).ConfigureAwait(false);
            try {
                await using DbDataReader reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
                return (await Drain(reader, shape).ConfigureAwait(false)).Map(rows => {
                    (long scanned, Duration elapsed) = leg.Report(command, leases, rows.Count, frame.Elapsed(mark));
                    return new ResidenceResult<T>(residence, lowered, rows, scanned, elapsed);
                });
            }
            finally {
                await command.DisposeAsync().ConfigureAwait(false);
                foreach (IAsyncDisposable lease in leases.Rev()) { await lease.DisposeAsync().ConfigureAwait(false); }
            }
        }).ConfigureAwait(false)).MapFail(residence.ReadRefused));

    // Flight is the ONE cross-runtime columnar query plane and Flight SQL the dialect layered on it: the lowered text
    // executes server-side, the returned `FlightInfo` carries one endpoint per partition, and every endpoint's ticket
    // streams Arrow batches back on the same plane — so a single-endpoint read and a partitioned read are one loop and
    // a residence that later partitions needs no arm.
    static IO<Fin<ResidenceResult<T>>> Arrow<T>(
        ResidenceReach.Flight leg, Residence residence, string lowered, Func<ResidenceRow, Fin<T>> shape, ProjectionContext frame) =>
        IO.liftAsync(async () => await Op.Of().Catch(async _ => {
            long mark = frame.Mark();
            FlightInfo info = await leg.Client.ExecuteAsync(lowered, Transaction.NoTransaction).ConfigureAwait(false);
            Fin<Seq<T>> rows = Fin.Succ(Seq<T>());
            foreach (FlightEndpoint endpoint in info.Endpoints) {
                await foreach (RecordBatch batch in leg.Client.DoGetAsync(endpoint.Ticket).ConfigureAwait(false)) {
                    rows = rows.Bind(held => Batched(batch, shape).Map(batched => held + batched));
                }
            }
            return rows.Map(held => new ResidenceResult<T>(residence, lowered, held, info.TotalRecords, frame.Elapsed(mark)));
        }).ConfigureAwait(false));

    // One drain per row family; the row wrapper is what lets the Arrow leg reuse the caller's one shape without a
    // second delegate or a fabricated reader. Both abort on the FIRST refusing row and return that fault, so a corrupt
    // column stops the read where it happened rather than yielding a partially fabricated result set.
    static async ValueTask<Fin<Seq<T>>> Drain<T>(DbDataReader reader, Func<ResidenceRow, Fin<T>> shape) {
        List<T> rows = [];
        ResidenceRow row = new ResidenceRow.Ado(reader);
        while (await reader.ReadAsync().ConfigureAwait(false)) {
            Fin<T> shaped = shape(row);
            if (shaped.IsFail) { return shaped.Map(static _ => Seq<T>()); }
            shaped.Iter(held => rows.Add(held));
        }
        return Fin.Succ(toSeq(rows));
    }

    static Fin<Seq<T>> Batched<T>(RecordBatch batch, Func<ResidenceRow, Fin<T>> shape) =>
        toSeq(Range(0, batch.Length)).TraverseM(ordinal => shape(new ResidenceRow.Arrow(batch, ordinal))).As();
}

// ONE landing across every relational residence dataset, the WRITE peer of `ResidenceRead`. The COPY column list, the
// tenancy lead, and each column's wire type all derive from the SAME `AnalyticsSchema` the DDL emitter provisions from,
// so a landed row and the table it lands in cannot drift on order, count, or physical type. This is the Series tier's
// declared landing owner: `SeriesLane.Ingest` is its hypertable-family arm and carries no second copy loop, while the
// Fleet tier lands through `Version/egress`'s sink and the Lake tier through `Query/lakehouse`'s generation, so each
// residence keeps exactly one writer. Binary COPY is the lane — `CompleteAsync` commits and disposal without it
// discards — so the retry unit is the whole batch and a refusal leaves nothing half-written.
public static class ResidenceLanding {
    // Payload roster IS the admitted roster, so the ingest column list and the CREATE column list are one derivation
    // off one declaration.
    public static Seq<ColumnRow> Payload(AnalyticsSchema schema) => schema.Columns;

    // Roster a PRODUCER fills, which is `Payload` minus every column the custodian stamps. A landing-time dataset's
    // instant is the custodian's BY CATEGORY, so no producer cell answers it: proving against `Payload` would demand a
    // cell for the very column the category forbids, and every correct landing-time producer would arrive one short.
    public static Seq<ColumnRow> Supplied(AnalyticsSchema schema) =>
        schema.Spine == TimeSpine.Landing
            ? Payload(schema).Filter(column => column.Name != schema.Time)
            : Payload(schema);

    // Custodian-stamped trailing column, resolved AHEAD of the copy exactly as every supplied column's wire is, so the
    // write loop stays total. The seam APPENDS the landing column, so `tenant` + supplied + landed reconstructs the
    // provisioned order byte-for-byte.
    static Validation<Error, Option<(Identifier Name, NpgsqlDbType Wire)>> Landed(AnalyticsSchema schema) =>
        schema.Spine == TimeSpine.Event
            ? Success<Error, Option<(Identifier, NpgsqlDbType)>>(None)
            : schema.Columns.Find(column => column.Name == schema.Time).Match(
                Some: column => column.Type.Wire.ToValidation<Error>().Map(wire => Some((schema.Time, wire))),
                None: () => Fail<Error, Option<(Identifier, NpgsqlDbType)>>(
                    new ResidenceFault.Unprovisioned($"<schema-spine:{schema.Dataset}>")));

    public static IO<Fin<ResidenceIngestReceipt>> Stage(
        NpgsqlDataSource store, AnalyticsSchema schema, Seq<Seq<ColumnCell>> rows, ProjectionContext frame) =>
        (Conformed(schema, Supplied(schema), rows), Landed(schema))
            .Apply(static (bound, landed) => (Bound: bound, Landed: landed)).As().ToFin().Match(
            Succ: proved => IO.liftAsync(async () => (await Op.Of().Catch(async token => {
                string columns = string.Join(", ",
                    (Seq(Residence.TenantColumn) + proved.Bound.Map(static entry => entry.Column.Name)
                        + proved.Landed.Map(static stamp => stamp.Name).ToSeq()).Map(Residence.Series.Quote));
                await using NpgsqlConnection lane = await store.OpenConnectionAsync(token).ConfigureAwait(false);
                // Scoped disposal releases the copy exactly once on every outcome — commit, typed refusal,
                // cancellation, or a conversion the gate could not foresee — and an uncompleted importer discards its
                // staged rows on the way out.
                await using NpgsqlBinaryImporter importer = await lane.BeginBinaryImportAsync(
                    $"COPY {Residence.Series.Quote(schema.Table)} ({columns}) FROM STDIN (FORMAT BINARY)", token).ConfigureAwait(false);
                // Tenancy is the FRAME's, never a row column: the whole batch lands under the ingesting tenant and
                // every read filters by it, so equal keys under distinct tenants never share rows.
                byte[] tenant = ColumnCell.Packed(frame.Tenant.TenantId.Value);
                // Landing instants ride the frame on the SAME terms: reading the clock once per batch rather than
                // once per row keeps a single COPY internally consistent, and the stamp binds through the same cell
                // arm every supplied cell binds through, so a spine change moves one declaration.
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
                // Receipt and rows carry the SAME instant, so evidence and residence agree on when this batch
                // landed; a second clock read here dates the receipt after the rows it accounts for.
                return Fin<ResidenceIngestReceipt>.Succ(new ResidenceIngestReceipt(schema.Dataset, (long)staged, landedAt, frame.Correlation));
            }).ConfigureAwait(false)).MapFail(Residence.Series.IngestRefused)),
            Fail: error => IO.pure(Fin<ResidenceIngestReceipt>.Fail(error)));

    // Conformance runs the DECLARED column's own gate — `ColumnRow.Admits`, absence against `Nullable` and presence
    // through the shape law — which is the same proof the record-batch fold runs, so the batch
    // and the COPY cannot disagree on whether a cell belongs to its column. Arity gates the column walk because a short
    // row shifts every later column onto a neighbour's proof; the two independent halves then accumulate, so a producer
    // reads every offending column and every unwritable shape from one refusal.
    static Validation<Error, Seq<(ColumnRow Column, NpgsqlDbType Wire)>> Conformed(
        AnalyticsSchema schema, Seq<ColumnRow> supplied, Seq<Seq<ColumnCell>> rows) =>
        rows.Exists(row => row.Count != supplied.Count)
            ? Fail<Error, Seq<(ColumnRow, NpgsqlDbType)>>(new ResidenceFault.IngestRefused(
                Residence.Series.Key, new EngineFault("<row-arity>", schema.Dataset)))
            : (rows.Traverse(row => row.Zip(supplied)
                    .Traverse(pair => pair.Item2.Admits(pair.Item1).ToValidation<Error>()).As()).As(),
               supplied.Traverse(column => column.Type.Wire.ToValidation<Error>().Map(wire => (column, wire))).As())
                .Apply(static (_, bound) => bound).As();

    // Binding folds with the SHAPE exactly as conformance does: a scalar or dictionary column defers to its row's own
    // cell law, a list writes its value run under the flagged wire type, and a map writes the ONE canonical JSON text — the
    // pairs serialize here so every landing spells one document shape, and the key roster reached this loop already
    // proven distinct. A fixed-arity run has no `NpgsqlDbType` to bind at all, so `Conformed` refused it before the
    // copy opened and its arm exists to close the fold rather than to be reached.
    static Task Bind(ColumnShape shape, ColumnCell cell, NpgsqlBinaryImporter importer, NpgsqlDbType wire) => shape.Switch(
        state:      (Cell: cell, Importer: importer, Wire: wire),
        // Absent cells reach this loop only through a column `ColumnRow.Admits` proved nullable and scalar, so the
        // stage writes SQL NULL — the importer's own spelling — and the cell law stays present-only.
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
|  [06]   | relational landing | one `ResidenceLanding.Stage` binary COPY    | `SeriesLane.Ingest` is its arm; never a second importer body |
|  [07]   | cell conformance   | `ColumnRow.Admits`, accumulating, pre-copy  | one gate with the batch fold; the importer infers nothing    |
|  [08]   | landing tenancy    | the frame's tenant and admission instant    | read once per batch; never a producer-supplied row column    |

## [04]-[RESEARCH]

(none)
