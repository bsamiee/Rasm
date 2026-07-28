# [CORE_BOARD]

Dashboards are identity-derived data, and the pack library is the same owner's dispatch: `DashboardModel` is one `Schema.Class` carrying identity, the closed panel family, variables, annotations, range defaults, the shelf-layout fold, and the pack/suite records, while `Query` is the metric-expression algebra under one render fold per target and `Bench` is the baseline-versus-candidate regression fold the claim bridge meters. Packs project only `Convention`, `Slo`, and payload vocabulary, so every declared instrument has a board consumer and a hand-authored dashboard has no authoring surface.

`Query.Target` carries the residence a board reads: one algebra renders PromQL against a metrics store and SQL against a columnar residence, so a second query language, a second board owner, and a raw-SQL tile each have no seat. `DashboardModel.snapshot` is the in-process read twin over `Metric.snapshot`, filtering the global registry to `Convention.metric` rows for doctor consumers operating without a telemetry backend. Its module is `core/src/observe/board.ts`.

## [01]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                                                          |
| :-----: | :-------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `QUERY`   | the typed expression family, its dual-spelling operator rows, the target axis, and both folds   |
|  [02]   | `PANEL`   | the closed panel row family and the shelf-layout grid fold                                      |
|  [03]   | `MODEL`   | the `DashboardModel` owner: identity-derived uid, variables, annotations, the backend-free read |
|  [04]   | `BENCH`   | the structural claim shape and the baseline-versus-candidate regression fold                    |
|  [05]   | `PACKS`   | the pane builders, the payload map, the pack record, dispatch, and suite                        |

## [02]-[QUERY]

- Owner: the `Query` closed family — selectors carry equality and regex matcher rows, `Windowed` carries range functions, `Quantile` and `Fraction` fuse the two histogram projections, `Aggregate` carries grouping posture, `Rank` carries the arity-bearing `topk`/`bottomk` pair, `Binary` carries arithmetic, comparison, and set operators, and `Const` carries scalar literals; cases describe grammar while `_FNS`/`_OPS`/`_AGG`/`_RANK`/`_MATCH`/`_POLARITY` rows generate the operator space.
- Owner: `Query.Target` — the closed residence axis whose `Promql` row carries the store's translation strategy and histogram representation, whose `Sql` row carries the engine key, the residence schema, and the bucket a provider-interval window resolves to, and whose shared `source` names the datasource every panel it renders binds.
- Law: `Query.render(query, target)` is the one entry over two total folds — the expression data stays target-free by construction, so a third backend is a third fold over the SAME family and never a second family, a second query owner, or a raw-dialect tile.
- Law: `Query.Target` closes over the METRIC-SERIES backends alone — every leaf keys a `Convention.MetricName` against a series name, so TraceQL earns no arm: a span-selection dialect answers a different question and folding one family into both forges a selector on whichever side lacks the other's shape.
- Law: trace evidence compiles as a PANEL through the residence target, never as an escape hatch — a columnar residence relates spans as wide-event rows the `Sql` fold reads, so a trace tile is a residence tile and the tempo backend's own board reach is its store row's `degrade` to state.
- Law: `Query.breach` and `Query.indicator` are the two SLI projections this owner publishes, and every consuming plane — the burn panels below, the iac rule expression, the iac objective query — renders one of them; a plane re-deriving a breach expression, a good-over-total quotient, or a level comparison from `sli` fields is the forked-discipline defect, so the shared legs (`_complement`, `_goodShare`, `_timeShare`, `_alternate`) stay interior and the two folds differ only where the operator's observable is not the alert's complement.
- Law: both SLI projections fold identically under both targets because each returns a `Query` value and never a string — a target-specific breach expression re-imports the forked discipline this owner deletes, and the target enters once at the terminal render.
- Law: operator rows carry both spellings, so a target is a COLUMN on the vocabulary rather than a table of its own — `_FNS` carries the PromQL range function beside the SQL fold over a value column, `_OPS` the token pair with the `truth` flag marking a comparison the engine casts to a number, `_AGG` the PromQL aggregation beside the engine-fold key, `_RANK` the function beside its sort direction, and `_MATCH` the matcher token beside its predicate kind and negation.
- Law: engine grammar is a closed `_ENGINES` roster and residence schema is caller data — the roster spells the eight members the three admitted engines genuinely disagree on and a fourth engine is one row, while table names, column names, and the attribute accessor arrive as a `Residence` value because DDL is the residence owner's, so no render arm branches on an engine name and no consumer hand-writes SQL.
- Law: series names are `Convention.MetricName` rows and label keys are `Convention.Key` rows — `Query.Labels` is the closed `Convention.Attributes` stamping record widened by the `_DIALECT` pair, the histogram `le` bucket label and the frequency `key` occurrence label, both export-contract facts rather than emission-plane keys — so the algebra admits no free-string metric, no unowned label key, and no off-vocabulary bounded value, and the tenant template variable enters as an ordinary label value (`$tenant`).
- Law: a mint name is not a series name under the PromQL target — `Convention.translated` projects the store's own spelling through the target's strategy, so a suffixing receiver's unit word and type tail reach the selector and a store row translating differently renders its own names off the same query value.
- Law: quoting splits by target and each crosses one seam — `_literal` delegates PromQL scalar quoting and every control-character escape to `JSON.stringify`, and `_quoted` spells the SQL literal by doubling the single quote; metric names, label keys, equality values, and regex values all cross the seam their own target owns.
- Law: label emission is census-ordered under both targets — `_selector` and `_leaf` walk `_LABEL_KEYS` (the `Convention.keys` census with `le`) and probe the record per key through `Option.fromNullable`, so pair order is the vocabulary's declaration order, absent keys emit nothing, and two equal `Query` values render byte-identically; an `Object.keys` walk re-imports per-record insertion order and is the deleted spelling.
- Law: windows are positive `Duration` values rendered without rounding or one closed provider interval token (`$__rate_interval`) — integral seconds use `s`, subsecond values use exact `ms`, and an arbitrary dialect window string is unspellable; the token has no SQL spelling, so a `Sql` target names the bucket it resolves to as data rather than leaving the width implicit.
- Law: `Windowed` renders by operand shape under both targets — a selector operand takes the range form (`fn(selector[w])`, one grouped SELECT over the source rows) and any composed operand takes the subquery form (`fn((expr)[w:])`, a re-aggregate over the inner relation) — so time-share expressions compose from the same rows and no builder hand-writes subquery or join syntax.
- Law: the histogram representation is a target row, never a name fact — a classic store exposes `le`-bearing `_bucket` series so the quantile arm aggregates `by (le)` and the fraction arm divides two bucket rates, while a native store carries one series so the same two cases render `histogram_quantile` and `histogram_fraction` over it.
- Law: every SQL relation answers the same three columns — the bucket instant, the series identity, and the value — so a combinator wraps its operand as a subquery, the join keys are fixed by construction, and grouping keys thread DOWN from the enclosing `Aggregate` into the leaf that reads them; a constant SUBTREE broadcasts inline rather than joining — recognition is recursive because the scalar relation carries the empty series identity a join key would have to match, so a composed threshold folds exactly as a bare literal does and neither target renders a shape the other cannot.
- Law: the leaf reads its relation AND its value column off the metric's own kind, so a residence relating histograms in a bucket table names the scalar that table genuinely carries and no fold reads a column its relation never declared.
- Law: the render fold IS the dialect's codegen output — PromQL is a single-line dialect whose rendered string is byte-load-bearing (quoted UTF-8 selector identity), so a document-assembly layer (`@effect/printer` `Doc`/`encloseSep`) is rejected: layout grouping and reflow forge selector spelling, and the closed family already owns every arm.
- Law: the fn/op/agg vocabularies stay interior — `_FNS`/`_OPS` are `as const satisfies` row tables no export reaches, their unions derive as the interior `_Fn`/`_Op`/`_Agg` aliases the case fields consume, and consumers speak literals the fields already type; the `type`-plus-`const` pair is the family's whole public spelling.
- Entry: constructors ride the family (`Query.Windowed({ fn: "rate", of, window })`), `Query.promql`/`Query.sql` mint the target off the residence a tile declares, `Query.render(query, target)` at pack-build time.
- Growth: a new function or operator is one `_FNS`/`_OPS`/`_AGG` row answering both columns; a new grammar shape is one case with its arm in both folds the compiler enforces; a new SQL engine is one `_ENGINES` row; an arity-bearing aggregation (`topk`) lands in the `Rank` case because parameter arity is its distinct grammar discriminant.
- Packages: `effect` (`Array`, `Data`, `Duration`, `Match`, `Number`, `Option`, `Record`, `Schema`, `pipe`); `convention#IDENTITY_PROJECTION` (`Convention` rows, the `keys` census, the `named` index, and the translation projection).

```typescript signature
import { Array, Data, Duration, Effect, Match, Metric, MetricPair, MetricState, Number, Option, Record, RegExp as Regex, Schema, Struct, pipe } from "effect"
import type { measure as MitataMeasure } from "mitata"
import type { AppIdentity } from "../value/identity.ts"
import { Convention } from "./convention.ts"
import { Alert, type Sli, type Slo } from "./slo.ts"

// Rows carry both spellings of one semantic: the PromQL range function beside the SQL fold the same window takes over a
// value column, so a new function answers both targets at one row and neither target owns a table of its own.
const _FNS = {
  avg: { promql: "avg_over_time", sql: (value: string) => `avg(${value})` },
  delta: { promql: "delta", sql: (value: string) => `max(${value}) - min(${value})` },
  increase: { promql: "increase", sql: (value: string) => `max(${value}) - min(${value})` },
  max: { promql: "max_over_time", sql: (value: string) => `max(${value})` },
  min: { promql: "min_over_time", sql: (value: string) => `min(${value})` },
  // cumulative counters carry their total, so the window's own delta over its span IS the rate the store reports
  rate: { promql: "rate", sql: (value: string, seconds: number) => `(max(${value}) - min(${value})) / ${seconds}` },
} as const satisfies Record<string, { readonly promql: string; readonly sql: (value: string, seconds: number) => string }>

// `truth` marks the comparison rows: PromQL spells `bool` to yield 0/1 and SQL yields a predicate the engine casts, so
// both targets hand the alert fold the same numeric observable — set operators fold to `least`/`greatest` for that reason.
const _OPS = {
  add: { promql: "+", sql: (left: string, right: string) => `${left} + ${right}`, truth: false },
  and: { promql: "and", sql: (left: string, right: string) => `least(${left}, ${right})`, truth: false },
  div: { promql: "/", sql: (left: string, right: string) => `${left} / ${right}`, truth: false },
  eq: { promql: "== bool", sql: (left: string, right: string) => `${left} = ${right}`, truth: true },
  gte: { promql: ">= bool", sql: (left: string, right: string) => `${left} >= ${right}`, truth: true },
  gt: { promql: "> bool", sql: (left: string, right: string) => `${left} > ${right}`, truth: true },
  lte: { promql: "<= bool", sql: (left: string, right: string) => `${left} <= ${right}`, truth: true },
  lt: { promql: "< bool", sql: (left: string, right: string) => `${left} < ${right}`, truth: true },
  mod: { promql: "%", sql: (left: string, right: string) => `${left} % ${right}`, truth: false },
  mul: { promql: "*", sql: (left: string, right: string) => `${left} * ${right}`, truth: false },
  neq: { promql: "!= bool", sql: (left: string, right: string) => `${left} <> ${right}`, truth: true },
  or: { promql: "or", sql: (left: string, right: string) => `greatest(${left}, ${right})`, truth: false },
  pow: { promql: "^", sql: (left: string, right: string) => `power(${left}, ${right})`, truth: false },
  sub: { promql: "-", sql: (left: string, right: string) => `${left} - ${right}`, truth: false },
  unless: { promql: "unless", sql: (left: string, right: string) => `${left} * (1 - ${right})`, truth: false },
} as const satisfies Record<string, { readonly promql: string; readonly sql: (left: string, right: string) => string; readonly truth: boolean }>

// `fold` names the engine column rather than a function, because five aggregations agree across every engine while
// deviation, variance, and any-value do not — so divergence lands once on `_ENGINES`, never inside a render arm.
const _AGG = {
  avg: { fold: "mean", promql: "avg" },
  count: { fold: "count", promql: "count" },
  group: { fold: "any", promql: "group" },
  max: { fold: "max", promql: "max" },
  min: { fold: "min", promql: "min" },
  stddev: { fold: "deviation", promql: "stddev" },
  stdvar: { fold: "variance", promql: "stdvar" },
  sum: { fold: "sum", promql: "sum" },
} as const satisfies Record<string, { readonly fold: string; readonly promql: string }>

const _RANK = { bottomk: { order: "ASC", promql: "bottomk" }, topk: { order: "DESC", promql: "topk" } } as const
// `sql` names the predicate kind the engine spells and `negate` wraps it, so a fifth matcher is one row and no arm branches
const _MATCH = {
  equal: { negate: false, promql: "=", sql: "compare" },
  notRegex: { negate: true, promql: "!~", sql: "match" },
  regex: { negate: false, promql: "=~", sql: "match" },
  unequal: { negate: true, promql: "!=", sql: "compare" },
} as const satisfies Record<string, { readonly negate: boolean; readonly promql: string; readonly sql: "compare" | "match" }>
const _INTERVAL = { rate: "$__rate_interval" } as const
// Export-contract labels: the frequency occurrence axis is the owned `wire` constant both metric bridges append, while
// `le` stays a free string because a suffixing receiver mints the bucket bound itself and no data point ever carries it.
const _DIALECT = [Convention.wire.occurrence, "le"] as const
const _POLARITY = { ceiling: "gt", floor: "lt" } as const satisfies Record<Slo.Polarity, keyof typeof _OPS> // level breach side reads as one comparison row, never a branch
const _COLUMN = { at: "at", by: "by", value: "v" } as const // the three columns every rendered relation answers; ordinal GROUP BY keeps the expressions unrepeated

type _Agg = keyof typeof _AGG
type _Fn = keyof typeof _FNS
type _Fold = (typeof _AGG)[_Agg]["fold"]
type _Op = keyof typeof _OPS
type _Rank = keyof typeof _RANK
type _RankCount = typeof _RankCount.Type
type _Finite = typeof _Finite.Type
type _Quantile = typeof _Quantile.Type
type _QuerySpan = typeof _QuerySpan.Type

const _RankCount = Schema.Int.pipe(Schema.positive(), Schema.brand("RankCount"))
const _Finite = Schema.Number.pipe(Schema.finite(), Schema.brand("QueryFinite"))
const _Quantile = Schema.Number.pipe(Schema.greaterThan(0), Schema.lessThan(1), Schema.brand("QueryQuantile"))
const _QuerySpan = Schema.DurationFromSelf.pipe(
  Schema.filter((span) => Duration.toMillis(span) > 0, { identifier: "QuerySpan" }),
  Schema.brand("QuerySpan"),
)

declare namespace Query {
  type Dialect = (typeof _DIALECT)[number]
  type Labels = { readonly [K in Convention.Key]?: Convention.ValueOf<K> extends ReadonlyArray<Convention.Scalar> ? never : Convention.ValueOf<K> }
    & { readonly [K in Dialect]?: string }
  type Matcher = { readonly key: Convention.Key | Dialect; readonly op: keyof typeof _MATCH; readonly value: Convention.Scalar }
  type Finite = _Finite
  type QuantileValue = _Quantile
  type Span = _QuerySpan
  type Window = Span | (typeof _INTERVAL)[keyof typeof _INTERVAL]
  type Engine = keyof typeof _ENGINES
  type Histogram = "classic" | "native"
  type Key = Convention.Key | Dialect
  // Residences name their own DDL: table and value column per instrument kind, an attribute accessor, and a series
  // identity over a key list — a materialized column and a map entry both answer `attribute`, so no schema is assumed.
  // `value` keys by kind because a bucket relation carries no per-point value column its sum-relation sibling does.
  type Residence = {
    readonly attribute: (key: Key) => string
    readonly degrade: string
    readonly identity: (keys: ReadonlyArray<Key>) => string
    readonly name: string
    readonly table: { readonly [K in Convention.InstrumentKind]: string }
    readonly time: string
    readonly value: { readonly [K in Convention.InstrumentKind]: string }
  }
  type Target = _Target
}

// Eight members, each one a spelling the three admitted engines genuinely disagree on: a `QUALIFY` clause two of them
// take, a boolean cast one of them refuses outright, and the aggregate roster `_AGG` rows name through their fold column.
const _ENGINES = {
  clickhouse: {
    aggregate: { any: "any", count: "count", deviation: "stddevPop", max: "max", mean: "avg", min: "min", sum: "sum", variance: "varPop" },
    bucket: (column: string, seconds: number) => `toStartOfInterval(${column}, INTERVAL ${seconds} SECOND)`,
    latest: (value: string, time: string) => `argMax(${value}, ${time})`,
    match: (value: string, pattern: string) => `match(${value}, ${pattern})`,
    quantile: (value: string, quantile: number) => `quantile(${quantile})(${value})`,
    rank: (inner: string, order: string, count: number) =>
      `SELECT * FROM (${inner}) QUALIFY row_number() OVER (PARTITION BY ${_COLUMN.at} ORDER BY ${_COLUMN.value} ${order}) <= ${count}`,
    share: (value: string, upper: number) => `countIf(${value} <= ${upper}) / nullif(count(), 0)`,
    truth: (predicate: string) => `toFloat64(${predicate})`,
  },
  duckdb: {
    aggregate: { any: "any_value", count: "count", deviation: "stddev_pop", max: "max", mean: "avg", min: "min", sum: "sum", variance: "var_pop" },
    bucket: (column: string, seconds: number) => `time_bucket(INTERVAL '${seconds} seconds', ${column})`,
    latest: (value: string, time: string) => `arg_max(${value}, ${time})`,
    match: (value: string, pattern: string) => `regexp_matches(${value}, ${pattern})`,
    quantile: (value: string, quantile: number) => `quantile_cont(${value}, ${quantile})`,
    rank: (inner: string, order: string, count: number) =>
      `SELECT * FROM (${inner}) QUALIFY row_number() OVER (PARTITION BY ${_COLUMN.at} ORDER BY ${_COLUMN.value} ${order}) <= ${count}`,
    share: (value: string, upper: number) => `count(*) FILTER (WHERE ${value} <= ${upper})::DOUBLE / nullif(count(*), 0)`,
    truth: (predicate: string) => `CAST(${predicate} AS DOUBLE)`,
  },
  postgres: {
    aggregate: { any: "any_value", count: "count", deviation: "stddev_pop", max: "max", mean: "avg", min: "min", sum: "sum", variance: "var_pop" },
    bucket: (column: string, seconds: number) => `time_bucket(INTERVAL '${seconds} seconds', ${column})`,
    latest: (value: string, time: string) => `(array_agg(${value} ORDER BY ${time} DESC))[1]`,
    match: (value: string, pattern: string) => `${value} ~ ${pattern}`,
    quantile: (value: string, quantile: number) => `percentile_cont(${quantile}) WITHIN GROUP (ORDER BY ${value})`,
    // no QUALIFY: the window column materializes one level down and the outer filter reads it
    rank: (inner: string, order: string, count: number) =>
      `SELECT ${_COLUMN.at}, ${_COLUMN.by}, ${_COLUMN.value} FROM (SELECT *, row_number() OVER (PARTITION BY ${_COLUMN.at}`
      + ` ORDER BY ${_COLUMN.value} ${order}) AS rn FROM (${inner}) ranked) ordered WHERE rn <= ${count}`,
    // bigint over bigint truncates here, so the numerator casts before the division rather than after
    share: (value: string, upper: number) => `(count(*) FILTER (WHERE ${value} <= ${upper}))::double precision / nullif(count(*), 0)`,
    truth: (predicate: string) => `CASE WHEN ${predicate} THEN 1.0 ELSE 0.0 END`, // boolean casts to a number nowhere on this engine
  },
} as const satisfies Record<string, {
  readonly aggregate: { readonly [F in _Fold]: string }
  readonly bucket: (column: string, seconds: number) => string
  readonly latest: (value: string, time: string) => string
  readonly match: (value: string, pattern: string) => string
  readonly quantile: (value: string, quantile: number) => string
  readonly rank: (inner: string, order: string, count: number) => string
  readonly share: (value: string, upper: number) => string
  readonly truth: (predicate: string) => string
}>

type _Engine = (typeof _ENGINES)[Query.Engine]
type _Target = Data.TaggedEnum<{
  Promql: { readonly histogram: Query.Histogram; readonly source: string; readonly translation: Convention.Translation }
  Sql: { readonly engine: Query.Engine; readonly residence: Query.Residence; readonly resolution: Query.Span; readonly source: string }
}>
const _Target = Data.taggedEnum<_Target>()
type _Promql = Extract<_Target, { readonly _tag: "Promql" }>
type _Sql = Extract<_Target, { readonly _tag: "Sql" }>

type Query = Data.TaggedEnum<{
  Aggregate: { readonly by: ReadonlyArray<Query.Key>; readonly of: Query; readonly op: _Agg; readonly without?: boolean }
  Binary: { readonly left: Query; readonly op: _Op; readonly right: Query }
  Const: { readonly value: Query.Finite }
  Fraction: { readonly labels: Query.Labels; readonly metric: Convention.MetricName<"histogram">; readonly upper: Query.Finite; readonly window: Query.Window }
  Instant: { readonly labels: Query.Labels; readonly matchers?: ReadonlyArray<Query.Matcher>; readonly metric: Convention.MetricName }
  Quantile: { readonly labels: Query.Labels; readonly metric: Convention.MetricName<"histogram">; readonly q: Query.QuantileValue; readonly window: Query.Window }
  Rank: { readonly count: _RankCount; readonly of: Query; readonly op: _Rank }
  Windowed: { readonly fn: _Fn; readonly of: Query; readonly window: Query.Window }
}>
const _Query = Data.taggedEnum<Query>()

const _LABEL_KEYS: ReadonlyArray<Query.Key> = [...Convention.keys, ..._DIALECT]

const _literal = (value: Convention.Scalar): string => JSON.stringify(String(value)) ?? '""'

const _quoted = (value: Convention.Scalar): string => `'${String(value).replaceAll("'", "''")}'`

const _span = (window: Query.Window): string =>
  typeof window === "string"
    ? window
    : pipe(Duration.toMillis(window), (millis) => millis % 1000 === 0 ? `${millis / 1000}s` : `${millis}ms`)

// Provider-interval tokens name a width only Grafana resolves, so a SQL target answers one from its own row rather
// than guessing a bucket; every other window arrives as a positive Duration already.
const _bucketed = (window: Query.Window, resolution: Query.Span): number =>
  Duration.toMillis(typeof window === "string" ? resolution : window) / 1000

// Classic stores expose the bucket bounds as an `le`-bearing `_bucket` series; a native store carries one series and no
// bucket label at all, so the posture picks the series name and the two histogram arms pick the function reading it.
const _promSeries = (metric: Convention.MetricName, row: _Promql): string =>
  `${Convention.translated(metric, row.translation)}${Convention.named[metric].kind === "histogram" && row.histogram === "classic" ? "_bucket" : ""}`

const _selector = (metric: Convention.MetricName, row: _Promql, labels: Query.Labels, matchers: ReadonlyArray<Query.Matcher> = []): string =>
  pipe(
    [
      ...Array.filterMap(_LABEL_KEYS, (key) =>
        Option.map(Option.fromNullable(labels[key]), (value) => `${_literal(key)}=${_literal(value)}`)),
      ...Array.map(matchers, ({ key, op, value }) => `${_literal(key)}${_MATCH[op].promql}${_literal(value)}`),
    ],
    (pairs) => `{${_literal(_promSeries(metric, row))}${pairs.length === 0 ? "" : `,${Array.join(pairs, ",")}`}}`,
  )

const _promql = (query: Query, row: _Promql): string =>
  _Query.$match(query, {
    Aggregate: ({ by, of, op, without }) =>
      `${_AGG[op].promql}${by.length === 0 ? "" : ` ${without === true ? "without" : "by"} (${Array.join(Array.map(by, _literal), ",")})`} (${
        _promql(of, row)
      })`,
    Binary: ({ left, op, right }) => `(${_promql(left, row)}) ${_OPS[op].promql} (${_promql(right, row)})`,
    Const: ({ value }) => `${value}`,
    // classic bounds must name a declared bucket edge: stores match `le` byte-wise against the edge the exporter wrote
    Fraction: ({ labels, metric, upper, window }) =>
      row.histogram === "native"
        ? `histogram_fraction(0, ${upper}, rate(${_selector(metric, row, labels)}[${_span(window)}]))`
        : `sum(rate(${_selector(metric, row, { ...labels, le: `${upper}` })}[${_span(window)}])) / sum(rate(${
          _selector(metric, row, { ...labels, le: "+Inf" })
        }[${_span(window)}]))`,
    Instant: ({ labels, matchers, metric }) => _selector(metric, row, labels, matchers),
    Quantile: ({ labels, metric, q, window }) =>
      row.histogram === "native"
        ? `histogram_quantile(${q}, sum(rate(${_selector(metric, row, labels)}[${_span(window)}])))`
        : `histogram_quantile(${q}, sum by (le) (rate(${_selector(metric, row, labels)}[${_span(window)}])))`,
    Rank: ({ count, of, op }) => `${_RANK[op].promql}(${count}, ${_promql(of, row)})`,
    Windowed: ({ fn, of, window }) =>
      of._tag === "Instant"
        ? `${_FNS[fn].promql}(${_promql(of, row)}[${_span(window)}])`
        : `${_FNS[fn].promql}((${_promql(of, row)})[${_span(window)}:])`,
  })

const _predicates = (
  row: _Sql,
  source: { readonly labels: Query.Labels; readonly matchers?: ReadonlyArray<Query.Matcher>; readonly metric: Convention.MetricName },
): ReadonlyArray<string> => [
  `${row.residence.name} = ${_quoted(source.metric)}`,
  ...Array.filterMap(_LABEL_KEYS, (key) =>
    Option.map(Option.fromNullable(source.labels[key]), (value) => `${row.residence.attribute(key)} = ${_quoted(value)}`)),
  ...Array.map(source.matchers ?? [], ({ key, op, value }) =>
    pipe(
      _MATCH[op].sql === "match"
        ? _ENGINES[row.engine].match(row.residence.attribute(key), _quoted(value))
        : `${row.residence.attribute(key)} = ${_quoted(value)}`,
      (predicate) => _MATCH[op].negate ? `NOT (${predicate})` : predicate,
    )),
]

// One grouped SELECT over the residence rows: enclosing-Aggregate keys arrive here, so this leaf groups by exactly
// what its caller asked for and no intermediate relation carries an attribute map it would then have to project.
const _leaf = (
  row: _Sql,
  keys: ReadonlyArray<Query.Key>,
  source: { readonly labels: Query.Labels; readonly matchers?: ReadonlyArray<Query.Matcher>; readonly metric: Convention.MetricName },
  window: Query.Window,
  value: (column: string) => string,
): string =>
  pipe(Convention.named[source.metric].kind, (kind) =>
    `SELECT ${_ENGINES[row.engine].bucket(row.residence.time, _bucketed(window, row.resolution))} AS ${_COLUMN.at},`
    + ` ${row.residence.identity(keys)} AS ${_COLUMN.by}, ${value(row.residence.value[kind])} AS ${_COLUMN.value}`
    + ` FROM ${row.residence.table[kind]}`
    + ` WHERE ${Array.join(_predicates(row, source), " AND ")} GROUP BY 1, 2`)

const _applied = (op: _Op, left: string, right: string, engine: _Engine): string =>
  _OPS[op].truth ? engine.truth(_OPS[op].sql(left, right)) : _OPS[op].sql(left, right)

// A constant subtree carries no series identity, so recognition is RECURSIVE: a composed threshold
// (`Binary(Const, mul, Const)`) is exactly as scalar as a bare literal and the PromQL fold already renders it that
// way, while a shallow immediate-operand test hands the join arm a relation whose `_COLUMN.by` is the empty
// identity `_constant` writes — the join matches nothing and the frame empties silently, so the two targets fork
// on a shape the family admits.
const _scalar = (query: Query, engine: _Engine): Option.Option<string> =>
  _Query.$match(query, {
    Aggregate: () => Option.none(),
    Binary: ({ left, op, right }) => Option.zipWith(_scalar(left, engine), _scalar(right, engine), (l, r) => `(${_applied(op, l, r, engine)})`),
    Const: ({ value }) => Option.some(`${value}`),
    Fraction: () => Option.none(),
    Instant: () => Option.none(),
    Quantile: () => Option.none(),
    Rank: () => Option.none(),
    Windowed: () => Option.none(),
  })

// One scalar relation shape: the `Const` case and an all-constant `Binary` render identically, so the NULL instant
// and empty series identity a scalar carries live at one site rather than one per arm.
const _constant = (value: string): string => `SELECT NULL AS ${_COLUMN.at}, '' AS ${_COLUMN.by}, ${value} AS ${_COLUMN.value}`

const _broadcast = (relation: string, value: string): string => `SELECT ${_COLUMN.at}, ${_COLUMN.by}, ${value} AS ${_COLUMN.value} FROM (${relation})`

const _joined = (op: _Op, engine: _Engine, left: string, right: string): string =>
  `SELECT l.${_COLUMN.at} AS ${_COLUMN.at}, l.${_COLUMN.by} AS ${_COLUMN.by}, ${_applied(op, `l.${_COLUMN.value}`, `r.${_COLUMN.value}`, engine)}`
  + ` AS ${_COLUMN.value} FROM (${left}) l JOIN (${right}) r ON l.${_COLUMN.at} = r.${_COLUMN.at} AND l.${_COLUMN.by} = r.${_COLUMN.by}`

const _sql = (query: Query, row: _Sql, keys: ReadonlyArray<Query.Key> = []): string =>
  pipe(_ENGINES[row.engine], (engine) =>
    _Query.$match(query, {
      Aggregate: ({ by, of, op, without }) =>
        `SELECT ${_COLUMN.at}, ${_COLUMN.by}, ${engine.aggregate[_AGG[op].fold]}(${_COLUMN.value}) AS ${_COLUMN.value} FROM (${
          _sql(of, row, without === true ? Array.filter(_LABEL_KEYS, (key) => !Array.contains(by, key)) : by)
        }) GROUP BY 1, 2`,
      // a scalar operand broadcasts inline: joining it on a series identity it does not carry would empty the frame,
      // and `_scalar` reads the whole subtree so a composed constant broadcasts exactly as a bare literal does
      Binary: ({ left, op, right }) =>
        pipe([_scalar(left, engine), _scalar(right, engine)] as const, ([lhs, rhs]) =>
          Option.match(Option.zipWith(lhs, rhs, (l, r) => _applied(op, l, r, engine)), {
            onNone: () =>
              Option.match(rhs, {
                onNone: () =>
                  Option.match(lhs, {
                    onNone: () => _joined(op, engine, _sql(left, row, keys), _sql(right, row, keys)),
                    onSome: (scalar) => _broadcast(_sql(right, row, keys), _applied(op, scalar, _COLUMN.value, engine)),
                  }),
                onSome: (scalar) => _broadcast(_sql(left, row, keys), _applied(op, _COLUMN.value, scalar, engine)),
              }),
            onSome: _constant,
          })),
      Const: ({ value }) => _constant(`${value}`),
      Fraction: ({ labels, metric, upper, window }) => _leaf(row, keys, { labels, metric }, window, (column) => engine.share(column, upper)),
      Instant: (source) => _leaf(row, keys, source, row.resolution, (column) => engine.latest(column, row.residence.time)),
      Quantile: ({ labels, metric, q, window }) => _leaf(row, keys, { labels, metric }, window, (column) => engine.quantile(column, q)),
      Rank: ({ count, of, op }) => engine.rank(_sql(of, row, keys), _RANK[op].order, count),
      // composed operands re-bucket to the OUTER window: folding at the leaf's own resolution would average one sample
      // per bucket and silently answer the resolution instead of the window the caller named
      Windowed: ({ fn, of, window }) =>
        of._tag === "Instant"
          ? _leaf(row, keys, of, window, (column) => _FNS[fn].sql(column, _bucketed(window, row.resolution)))
          : `SELECT ${engine.bucket(_COLUMN.at, _bucketed(window, row.resolution))} AS ${_COLUMN.at}, ${_COLUMN.by}, ${
            _FNS[fn].sql(_COLUMN.value, _bucketed(window, row.resolution))
          } AS ${_COLUMN.value} FROM (${_sql(of, row, keys)}) GROUP BY 1, 2`,
    }))

const _render = (query: Query, target: Query.Target): string =>
  _Target.$match(target, { Promql: (row) => _promql(query, row), Sql: (row) => _sql(query, row) })

const Query: Data.TaggedEnum.Constructor<Query> & {
  readonly breach: (sli: Sli, window: Query.Window, labels?: Query.Labels) => Query
  readonly burn: (spec: Alert.Spec, labels?: Query.Labels) => Query
  readonly finite: typeof _Finite.make
  readonly indicator: (sli: Sli, window?: Query.Window, labels?: Query.Labels) => Query
  readonly interval: typeof _INTERVAL
  readonly promql: (
    row: { readonly histogram?: Query.Histogram; readonly source: string; readonly translation?: Convention.Translation },
  ) => Query.Target
  readonly quantile: typeof _Quantile.make
  readonly rankCount: typeof _RankCount.make
  readonly render: (query: Query, target: Query.Target) => string
  readonly span: typeof _QuerySpan.make
  readonly sql: (
    row: { readonly engine: Query.Engine; readonly residence: Query.Residence; readonly resolution?: Query.Span; readonly source: string },
  ) => Query.Target
} = {
  ..._Query,
  breach: (sli, window, labels = {}) => _breach(sli, window, labels),
  burn: (spec, labels = {}) => _burned(spec, labels),
  finite: _Finite.make,
  indicator: (sli, window = _INTERVAL.rate, labels = {}) => _indicator(sli, window, labels),
  interval: _INTERVAL,
  // estate defaults: Effect mints explicit-bucket histograms, so classic reads as truth until a store row says otherwise
  promql: ({ histogram = "classic", source, translation = Convention.wire.translation }) => _Target.Promql({ histogram, source, translation }),
  quantile: _Quantile.make,
  rankCount: _RankCount.make,
  render: _render,
  span: _QuerySpan.make,
  sql: ({ engine, residence, resolution = _QuerySpan.make(Duration.minutes(1)), source }) =>
    _Target.Sql({ engine, residence, resolution, source }),
}
```

## [03]-[PANEL]

- Owner: the closed panel family — `_PanelFields` is the shared emission record for axes, description, interaction, links, repeat variable, datasource key, grid span, transformations, transparency, and title; `Timeseries`, `Stat`, `Gauge`, `Heatmap`, `Logs`, `Table`, `Geomap`, and `Nodes` embed it and add only their genuinely distinct visualization payload.
- Law: panels store RENDERED expressions — `Query` is the build-time algebra, the panel is the emission-ready datum — so the model serializes whole and the query family never needs a schema twin.
- Law: `source` carries the datasource key the rendering target named, so a board mixing a metrics-store tile with a columnar-residence tile binds each panel to the backend that answers its own dialect; deriving the key from the panel tag instead binds every table to whatever backend the tag defaulted to and empties a residence tile silently.
- Law: rows are emission-complete — threshold steps, legend format, and unit are semantic panel facts declared here so `iac` maps rows to provider fields verbatim and invents nothing; the datasource binding, folder placement, and apply lifecycle stay provider facts on `iac`'s side of the seam.
- Law: every panel row maps onto one Foundation-SDK builder — the `_tag` selects its admitted builder subpath and the shared `_PanelFields` land on inherited members, so the iac compile leg is a per-tag fold over typed builders and a panel field with no cataloged builder member stays out of the settled family.
- Law: every visualization case carries the policy its name promises — interaction owns tooltip and zoom, heatmaps own color and bucket scales, logs own ordering/deduplication/wrapping, tables own sort, geomaps own coordinate/label/weight mappings, and node graphs own node/edge identity mappings; these remain fields on the case, never provider-only option bags or parallel DTOs — and a field the pinned provider SDK exposes no builder member for is deleted at this owner, never carried as an inert emission fact.
- Law: a shared field lands at every tag whose builder reads it and nowhere else — `tooltip` reaches the Timeseries tooltip member, `zoom` reaches the Geomap controls pair and the Nodes zoom mode, and a tag whose builder subpath answers neither emits neither; the alternative, carrying one shared field onto every tag, writes options no builder there reads back.
- Law: `Geomap` and `Nodes` are the spatial and relational rows the BIM/geo and dependency planes fill through later-wave payloads — a geo-features pack or an element-graph pack is one pack row over these existing panel rows, never a panel family fork.
- Law: layout derives — `DashboardModel.laid(model)` is a `mapAccum` shelf fold assigning `{ x, y, w, h }` positions across the 24-column grid from each panel's `span`, wrapping when a shelf overflows and advancing by the tallest panel on the shelf; a hand-positioned panel does not exist, and a layout change is a fold change applied to every dashboard at once.
- Growth: a new visualization kind is one tagged row with its arm in consumers' emission folds.
- Packages: `effect` (`Schema`).

```typescript signature
const _Span = Schema.Struct({
  h: Schema.Int.pipe(Schema.between(2, 24)),
  w: Schema.Int.pipe(Schema.between(2, 24)),
})

const _Threshold = Schema.Struct({ at: Schema.Number, tone: Schema.NonEmptyString })
const _Axis = Schema.Struct({
  label: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  max: Schema.optionalWith(Schema.Number, { as: "Option" }),
  min: Schema.optionalWith(Schema.Number, { as: "Option" }),
  placement: Schema.Literal("left", "right", "hidden"),
  scale: Schema.Literal("linear", "log2", "log10", "symlog"),
  unit: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
})
const _Link = Schema.Struct({ title: Schema.NonEmptyString, url: Schema.NonEmptyString })
// Two fields, both with a builder member behind them: tooltip rides the Timeseries tooltip row and zoom rides the
// Geomap controls pair and the Nodes zoom mode. A range-brush field carried a third knob no builder subpath reads.
const _Interaction = Schema.Struct({
  tooltip: Schema.optionalWith(Schema.Literal("hidden", "multi", "single"), { default: () => "multi" as const }),
  zoom: Schema.optionalWith(Schema.Boolean, { default: () => true }),
})
const _Transform = Schema.Union(
  Schema.TaggedStruct("Calculate", { alias: Schema.NonEmptyString, expression: Schema.NonEmptyString }),
  Schema.TaggedStruct("Filter", { field: Schema.NonEmptyString, op: Schema.Literal("equal", "greater", "less", "match", "notEqual"), value: Schema.Union(Schema.String, Schema.Number, Schema.Boolean) }),
  Schema.TaggedStruct("Group", { by: Schema.NonEmptyArray(Schema.NonEmptyString), reducers: Schema.NonEmptyArray(Schema.Literal("count", "first", "last", "max", "mean", "min", "sum")) }),
  Schema.TaggedStruct("Join", { how: Schema.Literal("inner", "left", "outer"), on: Schema.NonEmptyArray(Schema.NonEmptyString) }),
  Schema.TaggedStruct("Organize", { order: Schema.Array(Schema.NonEmptyString), rename: Schema.Record({ key: Schema.NonEmptyString, value: Schema.NonEmptyString }) }),
  Schema.TaggedStruct("Reduce", { fields: Schema.NonEmptyArray(Schema.NonEmptyString), reducer: Schema.Literal("count", "first", "last", "max", "mean", "min", "sum") }),
)
const _PanelFields = {
  axes: Schema.optionalWith(Schema.Array(_Axis), { default: () => [] }),
  description: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  interaction: Schema.optionalWith(_Interaction, { default: () => ({ tooltip: "multi" as const, zoom: true }) }),
  links: Schema.optionalWith(Schema.Array(_Link), { default: () => [] }),
  repeat: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  source: Schema.NonEmptyString, // the datasource key the rendering target named; the compile leg binds `{ type, uid }` from it
  span: _Span,
  title: Schema.NonEmptyString,
  transformations: Schema.optionalWith(Schema.Array(_Transform), { default: () => [] }),
  transparent: Schema.optionalWith(Schema.Boolean, { default: () => false }),
} as const

const Timeseries = Schema.TaggedStruct("Timeseries", {
  ..._PanelFields,
  exprs: Schema.NonEmptyArray(Schema.String),
  legend: Schema.optionalWith(Schema.String, { as: "Option" }),
  steps: Schema.Array(_Threshold),
  unit: Schema.optionalWith(Schema.String, { as: "Option" }),
})
const Stat = Schema.TaggedStruct("Stat", {
  ..._PanelFields,
  expr: Schema.String,
  steps: Schema.Array(_Threshold),
  unit: Schema.optionalWith(Schema.String, { as: "Option" }),
})
const Gauge = Schema.TaggedStruct("Gauge", {
  ..._PanelFields,
  ceiling: Schema.Number,
  expr: Schema.String,
  steps: Schema.Array(_Threshold),
})
const Heatmap = Schema.TaggedStruct("Heatmap", {
  ..._PanelFields,
  color: Schema.optionalWith(Schema.Literal("continuous", "diverging", "opacity", "scheme"), { default: () => "scheme" as const }),
  expr: Schema.String,
  scale: Schema.optionalWith(Schema.Literal("exponential", "linear", "symlog"), { default: () => "linear" as const }),
  unit: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
})
const Logs = Schema.TaggedStruct("Logs", {
  ..._PanelFields,
  deduplicate: Schema.optionalWith(Schema.Literal("exact", "none", "numbers", "signature"), { default: () => "none" as const }),
  filter: Schema.String,
  order: Schema.optionalWith(Schema.Literal("ascending", "descending"), { default: () => "descending" as const }),
  showTime: Schema.optionalWith(Schema.Boolean, { default: () => true }),
  wrap: Schema.optionalWith(Schema.Boolean, { default: () => true }),
})
const Table = Schema.TaggedStruct("Table", {
  ..._PanelFields,
  exprs: Schema.NonEmptyArray(Schema.String),
  legend: Schema.optionalWith(Schema.String, { as: "Option" }),
  sort: Schema.optionalWith(Schema.Struct({ descending: Schema.Boolean, field: Schema.NonEmptyString }), { as: "Option" }),
})
const Geomap = Schema.TaggedStruct("Geomap", {
  ..._PanelFields,
  expr: Schema.String,
  mapping: Schema.Struct({
    color: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
    label: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
    latitude: Schema.NonEmptyString,
    longitude: Schema.NonEmptyString,
    weight: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  }),
})
const Nodes = Schema.TaggedStruct("Nodes", {
  ..._PanelFields,
  edges: Schema.String,
  mapping: Schema.Struct({
    edgeSource: Schema.NonEmptyString,
    edgeTarget: Schema.NonEmptyString,
    nodeColor: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
    nodeId: Schema.NonEmptyString,
    nodeLabel: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
    nodeWeight: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  }),
  nodes: Schema.String,
})
const Panel: Schema.Union<[
  typeof Timeseries,
  typeof Stat,
  typeof Gauge,
  typeof Heatmap,
  typeof Logs,
  typeof Table,
  typeof Geomap,
  typeof Nodes,
]> = Schema.Union(Timeseries, Stat, Gauge, Heatmap, Logs, Table, Geomap, Nodes)
type Panel = typeof Panel.Type
```

## [04]-[MODEL]

- Owner: `DashboardModel` — uid (a slug brand derived, never supplied), `title`, the identity record (the same `Convention.identity` projection every signal stamps, so a dashboard is greppable by the attributes its panels query), `tags`, the tenant template `variables` row, `annotations` derived from `slo#ALERT_SPECS` specs (slug and tone), the `refresh` cadence and `since` range defaults (emission facts with owner-fixed defaults, so `iac` reads them off the encoded model), and the `panels` array.
- Law: `DashboardModel.of(board, page)` is the ONLY page-level constructor consumers touch — uid derives as `${identity.app}-${page.slug}` through the slug refinement, the tenant variable row is always present (a single-tenant app pins it), and the identity attributes stamp automatically — so every dashboard in existence is a total function of the board context and a per-app fork has no authoring surface.
- Law: `DashboardModel.Board` is that context — who emits (`identity`), where the metrics plane is read (`target`), where the columnar residence is read (`analytics`), and which datasource carries the log stream (`logs`) — so a plane swap is one value at the composition root and never an edit inside a pane builder.
- Law: `analytics` is `Option`-carried because a residence is a spec coordinate a stack may decline and a residence answering no metric relation mints no target at all — a pack renders its residence tiles only where the value is present, so declining the plane drops those tiles rather than binding a panel to a door nothing serves.
- Law: emission is the derived twin — `typeof DashboardModel.Encoded` and the class's own `Schema.encode` are what `iac` consumes and applies through its grafana provider; a grafana-sdk admission lands as one interior emission member behind this same encode seam, changing no consumer.
- Law: model-level fields mirror the Foundation-SDK `DashboardBuilder` members one-for-one — `uid`/`title`/`tags`/`refresh` land on the builder members of the same name, `since` on `time`, `variables` on `withVariable`, `annotations` on `annotation`, `laid` positions on each panel's `gridPos` — so the iac compile leg types every knob and dashboard identity survives from `AppIdentity` into the Grafana state unchanged.
- Law: statics carry the derivations — `DashboardModel.laid`, the panel union with every row schema riding `DashboardModel`, and the pack dispatch, so one import serves model, panels, rows, layout, and packs, and a consumer constructs rows by name, never by union position.
- Law: `DashboardModel.snapshot` is the branch's BACKEND-FREE measurement read — it folds this process's own registry through `Metric.snapshot` and composes no exporter, reader, collector, or store, because an archive is pulled exactly when the egress is what failed and a read reaching its measurements over the wire answers nothing about that failure.
- Law: reading never emits, so the plane is no second truth beside the mounts — a doctor probe, a support receipt, and a residence fill each project the same registry states, and a value written back onto an instrument from a reading is the deleted form.
- Law: a declared row the process never measured registers no hook and therefore reaches no pair, so it reads UNMEASURED by absence and a fabricated zero stays distinguishable from a dead producer; seeding a case per declared row instead reports a level nobody set.
- Law: every case carries the row's DECLARED wire form, because Effect stores a signed level as a counter state and the observed tag alone collapses `updown` onto `counter` — monotonicity, the residence point relation, and a receipt's kind column all read `declared`, and a consumer re-resolving `Convention.named` beside the signal is the recovery this column deletes.
- Entry: `DashboardModel.of(board, page)`; `DashboardModel.laid(model)` at the apply seam; `DashboardModel.snapshot` at a doctor, support-receipt, or evidence-fill consumer.
- Growth: a new dashboard-level axis is one field with its default in the field declaration, inherited by every pack through `of`.
- Growth: a new plane a tile reads is one `Board` field, so the pane builders take the context whole and never grow a parameter each.
- Growth: a wire form the vocabulary admits is one `Convention` `_kinds` entry reaching every signal through `declared`, and a `LiveMetric` case is earned only where Effect carries a registry state no standing case shapes.
- Packages: `effect` (`Array`, `Data`, `Duration`, `Effect`, `Match`, `Metric`, `MetricPair`, `MetricState`, `Number`, `Option`, `Record`, `Schema`, `Struct`); `value/identity` (`AppIdentity`); `convention#IDENTITY_PROJECTION`.

```typescript signature
const _Uid = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]*$/), Schema.maxLength(40), Schema.brand("DashboardUid"))

const _Annotation = Schema.Struct({ slug: Schema.NonEmptyString, tone: Schema.NonEmptyString })
const _Variable = Schema.Struct({ label: Schema.NonEmptyString, name: Schema.NonEmptyString })

// `declared` rides EVERY case because the observed state is a lossy view of the vocabulary: Effect stores a signed
// level as a counter state, so a reader folding on the state tag alone reports every `updown` row as a monotonic
// total and the residence fill then plants `is_monotonic` true for a level that falls. Carrying the row's own wire
// form here is what keeps monotonicity, the residence point relation, and a support receipt's kind column reading
// one discriminant, so no consumer re-resolves `Convention.named` to recover what the fold would otherwise drop.
type LiveMetric = Data.TaggedEnum<{
  Counter: { readonly declared: Convention.InstrumentKind; readonly labels: Convention.Bag; readonly name: Convention.MetricName; readonly value: number | bigint }
  Frequency: { readonly declared: Convention.InstrumentKind; readonly labels: Convention.Bag; readonly name: Convention.MetricName; readonly values: ReadonlyMap<string, number> }
  Gauge: { readonly declared: Convention.InstrumentKind; readonly labels: Convention.Bag; readonly name: Convention.MetricName; readonly value: number | bigint }
  Histogram: { readonly buckets: ReadonlyArray<readonly [number, number]>; readonly count: number; readonly declared: Convention.InstrumentKind; readonly labels: Convention.Bag; readonly max: number; readonly min: number; readonly name: Convention.MetricName; readonly sum: number }
  Summary: { readonly count: number; readonly declared: Convention.InstrumentKind; readonly error: number; readonly labels: Convention.Bag; readonly max: number; readonly min: number; readonly name: Convention.MetricName; readonly quantiles: ReadonlyArray<readonly [number, Option.Option<number>]>; readonly sum: number }
  Unknown: { readonly declared: Convention.InstrumentKind; readonly labels: Convention.Bag; readonly name: Convention.MetricName }
}>
const _LiveMetric = Data.taggedEnum<LiveMetric>()
// `Convention.named` IS the membership test: a second roster minted beside it drifts on the first row admitted, and a
// linear scan re-walks the whole vocabulary for every registry pair the snapshot carries.
const _isMetricName = (name: string): name is Convention.MetricName => name in Convention.named
const _live = (pair: MetricPair.MetricPair.Untyped): Option.Option<LiveMetric> =>
  Option.map(Option.liftPredicate(pair.metricKey.name, _isMetricName), (name) => {
    const labels: Convention.Bag = Record.fromEntries(Array.map(pair.metricKey.tags, (tag) => [tag.key, tag.value] as const))
    const declared = Convention.named[name].kind
    return Match.value(pair.metricState).pipe(
      Match.when(MetricState.isCounterState, (state) => _LiveMetric.Counter({ declared, labels, name, value: state.count })),
      Match.when(MetricState.isFrequencyState, (state) => _LiveMetric.Frequency({ declared, labels, name, values: state.occurrences })),
      Match.when(MetricState.isGaugeState, (state) => _LiveMetric.Gauge({ declared, labels, name, value: state.value })),
      Match.when(MetricState.isHistogramState, (state) =>
        _LiveMetric.Histogram({ buckets: state.buckets, count: state.count, declared, labels, max: state.max, min: state.min, name, sum: state.sum })),
      Match.when(MetricState.isSummaryState, (state) =>
        _LiveMetric.Summary({ count: state.count, declared, error: state.error, labels, max: state.max, min: state.min, name, quantiles: state.quantiles, sum: state.sum })),
      // Registry states no case shapes still name their declared row, so a consumer reports the row it could not
      // read rather than dropping it into a silence indistinguishable from an unmeasured instrument.
      Match.orElse(() => _LiveMetric.Unknown({ declared, labels, name })),
    )
  })

class DashboardModel extends Schema.Class<DashboardModel>("DashboardModel")({
  annotations: Schema.Array(_Annotation),
  identity: Schema.Record({ key: Schema.String, value: Schema.String }),
  panels: Schema.Array(Panel),
  refresh: Schema.optionalWith(Schema.DurationFromMillis, { default: () => Duration.seconds(30) }),
  since: Schema.optionalWith(Schema.DurationFromMillis, { default: () => Duration.hours(6) }),
  tags: Schema.Array(Schema.NonEmptyString),
  title: Schema.NonEmptyString,
  uid: _Uid,
  variables: Schema.Array(_Variable),
}) {
  static readonly Gauge = Gauge
  static readonly Geomap = Geomap
  static readonly Heatmap = Heatmap
  static readonly Logs = Logs
  static readonly Nodes = Nodes
  static readonly Panel = Panel
  static readonly Stat = Stat
  static readonly Table = Table
  static readonly Timeseries = Timeseries
  static readonly of = ({ identity }: DashboardModel.Board, page: DashboardModel.Page): DashboardModel =>
    new DashboardModel({
      annotations: page.annotations,
      identity: Convention.identity(identity),
      panels: page.panels,
      tags: [identity.app, ...page.tags],
      title: `${identity.app} ${page.title}`,
      uid: _Uid.make(`${identity.app}-${page.slug}`),
      variables: [{ label: "Tenant", name: "tenant" }, ...page.variables],
    })
  static readonly laid = (model: DashboardModel): ReadonlyArray<DashboardModel.Placed> =>
    Array.mapAccum(model.panels, { shelf: 0, x: 0, y: 0 }, (cursor, panel) => {
      const wraps = cursor.x + panel.span.w > 24
      const x = wraps ? 0 : cursor.x
      const y = wraps ? cursor.y + cursor.shelf : cursor.y
      return [
        { shelf: wraps ? panel.span.h : Number.max(cursor.shelf, panel.span.h), x: x + panel.span.w, y },
        { panel, position: { h: panel.span.h, w: panel.span.w, x, y } },
      ]
    })[1]
  static readonly snapshot: Effect.Effect<ReadonlyArray<LiveMetric>> = Effect.map(Metric.snapshot, (pairs) => Array.filterMap(pairs, _live))
  static readonly pack = <K extends DashboardModel.Pack>(
    kind: K,
    board: DashboardModel.Board,
    payload: DashboardModel.Payload[K],
  ): DashboardModel => _PACKS[kind](board, payload)
  static readonly suite = (board: DashboardModel.Board, payload: DashboardModel.Suite): ReadonlyArray<DashboardModel> =>
    Array.map(Struct.keys(_SUITE), (kind) => _SUITE[kind](board, payload))
}

declare namespace DashboardModel {
  type Board = {
    readonly analytics: Option.Option<Query.Target> // the columnar residence, absent where the stack installs none
    readonly identity: AppIdentity
    readonly logs: string
    readonly target: Query.Target // the metrics plane every health tile reads
  }
  type Page = {
    readonly annotations: ReadonlyArray<typeof _Annotation.Type>
    readonly panels: ReadonlyArray<Panel>
    readonly slug: string
    readonly tags: ReadonlyArray<string>
    readonly title: string
    readonly variables: ReadonlyArray<typeof _Variable.Type>
  }
  type Placed = { readonly panel: Panel; readonly position: { readonly h: number; readonly w: number; readonly x: number; readonly y: number } }
  type Wire = typeof DashboardModel.Encoded
  type Signal = LiveMetric
}
```

## [05]-[BENCH]

- Owner: the benchmark comparison algebra — `Bench`, the structural claim vocabulary (`Band`, the sample count beside the map of rungs its harness measured; `Metric`, the modality-labeled unit row; `Claim`, the suite-plus-host-print shape) and the pure baseline-versus-candidate fold `Bench.graded` yielding the `Graded`/`Refused` verdict family under one tolerance policy row.
- Law: the claim shape stays structural, never a harness or interchange import — `Metric.kind` composes `mitata`'s state-free modality while `_RUNGS` transcribes the rung roster the `tests/contracts/` `BENCHMARK_CLAIM` band fixes, and this plane types the exact contextual fields it grades (`suite`, `host.print`, `label`, `unit`), so a decoded `Claim` from any minting harness conforms by construction and a second benchmark vocabulary is unspellable.
- Law: incompatible comparison is refused, never computed — `_ADMISSION` orders suite, host print, metric kind-label-unit roster, and the one graded rung measured strictly positive on both claims as data rows, and `Refused` carries the first failed axis and both projections, so a gate never compares unrelated suites, changes modality or units, accepts duplicate rows, divides by a zero baseline, grades a zero-measurement candidate as improvement, reads a rung the minting harness never measured, or mistakes a partial join for a complete grade.
- Law: the grade is a tolerance policy row — `_TOLERANCE` names the graded rung (`p99`) and admits its slack through the finite `[0, 1)` `_Slack` constructor, the admitted per-kind-label-unit join is total over both rosters, and the three-grade vocabulary (`improved`/`steady`/`regressed`) is the closed union a gate reads; a caller wanting a different rung or slack passes one admitted row, never a second fold, and the refusal projection names that caller's rung rather than the module default.
- Law: verdicts feed the bridge, not the panels — the runtime meter bridge mints `Convention.metric.benchVerdicts` from the graded rows and the `bench` pack trends that series, so the board view and the gate view of one comparison are provably the same fold output.
- Growth: a new grade is one `_GRADES` entry every exhaustive consumer breaks on.
- Growth: a rung a harness starts measuring is one `_RUNGS` entry; a second graded field is one `Tolerance` field.
- Packages: `effect` (`Array`, `Data`, `Number`, `Option`, `Schema`); `mitata` (`stats` modality type alone, never the module-global harness).

```typescript signature
// Rung roster transcribed off the codec claim rather than imported, so this plane stays free of the
// interchange wave: a minting harness fills the rungs it measured — a sampling run the whole ladder, an
// equivalence sweep p50/p95/stdDev — so the grade reads ONE named rung through a lookup.
const _RUNGS = ["min", "max", "avg", "p25", "p50", "p75", "p95", "p99", "p999", "stdDev"] as const
const _GRADES = ["improved", "steady", "regressed"] as const
const _BandValue = Schema.Number.pipe(Schema.finite(), Schema.nonNegative())
const _isBandValue = Schema.is(_BandValue)
type _Slack = typeof _Slack.Type
const _Slack = Schema.Number.pipe(Schema.finite(), Schema.nonNegative(), Schema.lessThan(1), Schema.brand("BenchSlack"))
// `mitata` declares its stats interface unexported, so the modality union derives off the one member returning it,
// taking the `ReturnType` route every unexported shape in this corpus takes rather than a hand-copied literal union.
type _MitataStats = Awaited<ReturnType<typeof MitataMeasure>>

declare namespace Bench {
  type Rung = (typeof _RUNGS)[number]
  type Band = { readonly sampleCount: number; readonly rungs: { readonly [R in Rung]?: number } }
  type Metric = { readonly kind: _MitataStats["kind"]; readonly label: string; readonly unit: string; readonly band: Band }
  type Claim = { readonly suite: string; readonly host: { readonly print: string }; readonly metrics: ReadonlyArray<Metric> }
  type Grade = (typeof _GRADES)[number]
  type Row = { readonly kind: Metric["kind"]; readonly label: string; readonly unit: string; readonly grade: Grade; readonly ratio: number }
  type RefusalAxis = "suite" | "host" | "metrics" | "rung"
  type Verdict = Data.TaggedEnum<{
    Graded: { readonly suite: string; readonly print: string; readonly rows: ReadonlyArray<Row> }
    Refused: { readonly suite: string; readonly axis: RefusalAxis; readonly baseline: string; readonly candidate: string }
  }>
  type Tolerance = { readonly rung: Rung; readonly slack: _Slack }
}
const _Verdict = Data.taggedEnum<Bench.Verdict>()

const _TOLERANCE: Bench.Tolerance = { rung: "p99", slack: _Slack.make(0.05) }

const _measured = (metric: Bench.Metric, rung: Bench.Rung): Option.Option<number> =>
  Option.filter(Option.fromNullable(metric.band.rungs[rung]), _isBandValue)

const _sameMetric = (left: Bench.Metric, right: Bench.Metric): boolean =>
  left.kind === right.kind && left.label === right.label && left.unit === right.unit

const _roster = (claim: Bench.Claim): string =>
  Array.join(Array.map(claim.metrics, ({ kind, label, unit }) => `${kind}:${label}[${unit}]`), ",")

const _rungValues = (rung: Bench.Rung) => (claim: Bench.Claim): string =>
  Array.join(
    Array.map(claim.metrics, (metric) =>
      `${metric.kind}:${metric.label}[${metric.unit}]:${rung}=${Option.getOrElse(_measured(metric, rung), () => "unmeasured")}`),
    ",",
  )

const _aligned = (baseline: Bench.Claim, candidate: Bench.Claim): boolean =>
  baseline.metrics.length === candidate.metrics.length
  && Array.every(baseline.metrics, (metric) => Array.filter(candidate.metrics, (held) => _sameMetric(metric, held)).length === 1)
  && Array.every(candidate.metrics, (metric) => Array.filter(baseline.metrics, (held) => _sameMetric(metric, held)).length === 1)

type _Admission = {
  readonly accepts: (baseline: Bench.Claim, candidate: Bench.Claim, tolerance: Bench.Tolerance) => boolean
  readonly axis: Bench.RefusalAxis
  readonly project: (claim: Bench.Claim, tolerance: Bench.Tolerance) => string
}

const _ADMISSION: ReadonlyArray<_Admission> = [
  { axis: "suite", accepts: (baseline, candidate) => baseline.suite === candidate.suite, project: (claim) => claim.suite },
  { axis: "host", accepts: (baseline, candidate) => baseline.host.print === candidate.host.print, project: (claim) => claim.host.print },
  { axis: "metrics", accepts: _aligned, project: _roster },
  {
    // One axis over the ONE graded rung: a harness that never measured it has nothing to compare, and a
    // measured zero divides the ratio on the baseline side and grades as a phantom improvement on the
    // candidate side. Demanding every rung instead refuses a claim over rungs no grade reads.
    axis: "rung",
    accepts: (baseline, candidate, tolerance) =>
      Array.every([baseline, candidate], (claim) =>
        Array.every(claim.metrics, (metric) => Option.match(_measured(metric, tolerance.rung), { onNone: () => false, onSome: (value) => value > 0 }))),
    project: (claim, tolerance) => _rungValues(tolerance.rung)(claim),
  },
]

const _graded = (baseline: Bench.Claim, candidate: Bench.Claim, tolerance: Bench.Tolerance = _TOLERANCE): Bench.Verdict =>
  pipe(
    Array.findFirst(_ADMISSION, (row) => !row.accepts(baseline, candidate, tolerance)),
    Option.match({
      onSome: (row) => _Verdict.Refused({
        suite: candidate.suite,
        axis: row.axis,
        baseline: row.project(baseline, tolerance),
        candidate: row.project(candidate, tolerance),
      }),
      onNone: () => _Verdict.Graded({
        suite: candidate.suite,
        print: candidate.host.print,
        // Admission already proved the graded rung measured and positive on both sides, so this zip is total.
        rows: Array.filterMap(candidate.metrics, (metric) =>
          Option.flatMap(Array.findFirst(baseline.metrics, (row) => _sameMetric(metric, row)), (held) =>
            Option.zipWith(_measured(metric, tolerance.rung), _measured(held, tolerance.rung), (fresh, base) => {
              const ratio = fresh / base
              return {
                kind: metric.kind,
                label: metric.label,
                unit: metric.unit,
                ratio,
                grade: ratio > 1 + tolerance.slack
                  ? ("regressed" as const)
                  : ratio < 1 - tolerance.slack
                    ? ("improved" as const)
                    : ("steady" as const),
              }
            }))),
      }),
    }),
  )

const Bench: Data.TaggedEnum.Constructor<Bench.Verdict> & {
  readonly rungs: typeof _RUNGS
  readonly grades: typeof _GRADES
  readonly graded: (baseline: Bench.Claim, candidate: Bench.Claim, tolerance?: Bench.Tolerance) => Bench.Verdict
  readonly measured: (metric: Bench.Metric, rung: Bench.Rung) => Option.Option<number>
  readonly slack: typeof _Slack.make
} = { ..._Verdict, rungs: _RUNGS, grades: _GRADES, graded: _graded, measured: _measured, slack: _Slack.make }
```

## [06]-[PACKS]

- Owner: the interior `_pane` builders and the `_PACKS` handler record dispatched by `DashboardModel.pack` — the payload map types each pack's input, the mapped handler contract turns a missing pack into a compile error at the record, and the one generic indexed dispatch keeps the payload following the kind.
- Law: a builder never invents a name, a threshold, or a tone — series come from `Convention.metric` rows, tenancy filters from `Convention.rasm` keys against the `$tenant` template variable, vital ceilings and meter resource axes from the caller's payload rows, burn thresholds from the spec's own `factor`, threshold tones from the spec's own severity row (`Alert.severity.page.tone` where a panel gates with no spec) — so the builders are pure plumbing between vocabulary and visualization, a severity-to-tone table re-declared here is the hand-synced parallel the derivation law kills, and deleting any hardcoded literal from them leaves nothing to delete.
- Law: payloads carry the later-wave vocabulary IN — the runtime vital owner passes its budget rows as `gauges`, the meter owner its resource axis as `resources`, the app its objectives — so this floor renders domains it never imports, the dependency arrow stays strictly upward, and a vocabulary change upstream re-renders through the payload with zero edits here.
- Law: every pack routes through `DashboardModel.of` — identity-derived uid, stamped identity attributes, the always-present tenant variable — so the pack layer cannot mint an identity-free dashboard; the `slo` pack folds `Alert.of(objective)` specs into burn panels and annotation rows, making the alert and dashboard views of one objective provably the same data.
- Law: the burn panel renders the WHOLE discipline — `_breach(sli, window)` compiles the SLI's own breach predicate through one `Match.valueTags` record dispatch into an error-rate expression (`Latency` as the `Fraction` complement at the spec's `ceiling`, `Ratio` and `Partition` as the good-share complement, `Saturation` and `Freshness` as bool-comparison time shares whose operator reads the `_POLARITY` row), `_burnPair` divides it by the objective's budget for BOTH the long and the short window as two series on one panel, the row's `factor` lands as the panel's threshold step, and the derived `spend` prints in the panel title — so the panel shows exactly the two-window condition `slo#BURN_ROWS` legislates, the `Latency` `ceiling` has its render-side consumer, and the budget-share figure the operator reads is the spec's own derived field, never a re-computation.
- Law: the latency breach is one `Fraction` case and never a hand-assembled bucket pair — the case names the bound in the metric's own unit through `Convention.duration`, so the classic bucket division, the native `histogram_fraction`, and the SQL count share are three renders of one value rather than three builders spelling their own shares.
- Law: a `Partition` good half enters as one anchored regex matcher on the counter's own declared key — `_alternate` escapes every RE2 metacharacter and `=~` anchors the whole label value — so the numerator and the denominator select one series with one selector and no second counter carries the good half.
- Law: the audit pack queries the `Convention` audit family — the action-rate series grouped by `rasm.audit.action` and the actor/action table over `rasm.audit.actor.kind`, both over the `rasm.fact.drained` fact stream — so the audit signal domain has a standing board projection beside slo/vital/meter/crash.
- Law: the invoke pack is the capability plane's RED projection — outcome rates grouped by the `Exit`-fold vocabulary rows (`rasm.invoke.outcome`, `rasm.gateway.outcome`), the fault-reason frequency grouped by its `key` occurrence label, and duration quantiles on both directions, all over the `Convention` invoke/gateway rows with no tenant filter because the capability instruments are process-level — so the branch's hottest surface ships a standing dashboard the moment `interchange/invoke#CAPABILITY_BIND` and `interchange/invoke#COMMAND_GATEWAY` land their instruments, and the outcome-rate and quantile builders are one parameterized pair, never a builder per plane.
- Law: the work pack is the durable-work health board `convention#RASM_ROWS` legislates — outbox/queue depth and redelivery instants, oldest-age stat, relay-drain and parked rates by `rasm.work.channel`, the fact-landing accounting by `rasm.fact.stream`, lane checkpoints by `rasm.lane.name`, derivative pressure, and batch-window quantiles — every series the runtime meter bridge mints from journal facts and census probes, tenant-free because work-plane instruments are process-level, while every dispute settles against the journal.
- Law: landing renders BOTH halves on one pane — the drained rate beside the content-key-matched rate — because a drain rate alone claims zero redelivery and zero redelivery is indistinguishable from a wedged retry re-offering one window forever; a board carrying the accepted half alone leaves the estate's own at-least-once accounting with no reader.
- Law: the view pack is the interaction plane's board — graft arrivals and the refusal census by reason, pivot delta frames, and submit trips by outcome — so the ui branch's viewer, chart, and form owners each carry a standing consumer and no declared instrument reaches a receipt with no board behind it; the panes carry no tenant label because a browser-role process emits one session's series and `DashboardModel.of` already supplies the tenant variable.
- Law: the vital pack pairs each payload gauge with the observation stream, so the level and observation planes land on one board.
- Law: a gauge row names its own level series, because one OTLP metric name carries one descriptor unit and the level family splits per UCUM code.
- Law: the security pack projects the whole folder — the reject rate and facet table, the authorization-denial rate by reason, the rotated-refresh replay rate by surface, the rotation, cold-JWKS, quarantine, and shredded-open rates, and the JWKS-resolve and key-derivation quantiles — so every `Convention` security instrument carries a standing consumer and no custody signal reaches a receipt with no board behind it.
- Law: the replay pane filters the reject counter on its own `kind` rather than minting a second counter — the reject stream already keys the replay half, and a twin doubles the mounted series while stranding its denominator on any emission edit.
- Law: security panes carry no tenant label — the security instrument rows declare no tenant dimension, so the pack reads process-level series while `DashboardModel.of` supplies the tenant variable every dashboard already carries.
- Law: the crash pack groups the capture counter's ONE declared fan — a class table over `error.type` beside the capture-rate stat — while fingerprint and hop evidence reads off the exception log stream, so no panel groups on an axis the census proves no producer stamps.
- Law: the object pack is the content-addressed plane's health board — write outcomes grouped by `rasm.object.outcome`, the landed-bytes and resumable-upload flow pair, and the sweep-reclaim rate — every series the data object owners tap from receipts, tenant-free because the object instruments are process-level, while every dispute settles against the receipt.
- Law: the lake pack is the storage-harvest board — admission-wait and deferred-wait quantiles, harvested engine-profile quantiles, the retried rate by `rasm.olap.engine`, the cache hit-share expression grouped by `rasm.cache.name`, and the pool-lease instant by `rasm.pool.scheme` — so the lake-engine profile parity and cache/pool census the data lanes mint read on one standing board, and its residence tile carries the same retry series over the evidence horizon the store's own retention cannot reach.
- Law: the bench pack trends the claim bridge — the `rasm.bench.band` timing ladder per payload suite, one generated enrichment panel per GC/heap/hardware-counter unit family, and the verdict rate grouped by `rasm.bench.verdict` — the meter-bridged projection of `[05]`'s fold, so a regression is a threshold-visible line the same fold gates on and incompatible units never share one axis.
- Law: `DashboardModel.suite(board, payload)` folds the mapped `_SUITE` record, whose key contract is exactly `DashboardModel.Pack`; a new pack cannot compile until its suite projection lands, and the standing fleet never requires a hand-maintained array roster.
- Law: spans are the builders' only local decision — each pane declares its grid `span` so the model's shelf fold lays every pack without per-pack layout code; a reusable visualization earns a builder at two pack call sites, else it inlines.
- Law: a pane's grouping axes enter the `Query` through `_grouped` and reach the legend from that same list — a legend template naming a key its query never grouped on renders per-series under PromQL and one collapsed relation under SQL, so the axis list has one consumer and both targets answer the pane identically.
- Law: every builder takes the `Board` context whole and reads its plane at the one `Query.render` call — so the backend a tile queries is the composition root's value, a builder holds no dialect knowledge, the log panes bind `board.logs` because the log stream is a plane no query target names, and a residence pane binds `board.analytics` so one board mixes a store tile with a residence tile and each panel names its own datasource.
- Law: the `Board` planes ARRIVE from the tier that selected the backend, never from a composition root's own mint — the deploy plane projects `target` off the store row it installed and the residence coordinate its spec armed, and `data`'s residence owner completes that coordinate into the SQL target; a root minting either re-spells a translation strategy or a relation roster the realized backend never wrote, and the whole board then renders selectors matching nothing while every component reports healthy.
- Law: a pane's display unit derives from the series it renders — the metric row's UCUM code answers `Convention.grafanaUnit` for the fold the pane runs, so a builder spells no display word, a rate never wears its quantity's id, and a hand-spelled unit column has nothing left to hold.
- Boundary: provider emission — grafana JSON, folder placement, apply lifecycle — is `iac`'s seam over `typeof DashboardModel.Encoded`; delivery of alert specs is `slo#ALERT_SPECS`'s consumer law.
- Entry: `DashboardModel.pack(kind, board, payload)`; `DashboardModel.suite(board, payload)`.
- Growth: a new dashboard family is one payload row with its handler row; every consumer inherits it through the derived kind union.

```typescript signature
const _tenant = { [Convention.rasm.tenant]: "$tenant" } as const

const _WINDOW = Query.span(Duration.minutes(5))
const _DAY = Query.span(Duration.hours(24))

const _rated = (
  metric: Convention.MetricName<"counter">,
  labels: Query.Labels,
  window: Query.Window,
  matchers: ReadonlyArray<Query.Matcher> = [],
): Query => Query.Aggregate({ by: [], of: Query.Windowed({ fn: "rate", of: Query.Instant({ labels, matchers, metric }), window }), op: "sum" })

// RE2 alternation over the good value set; `=~` anchors fully, so escaping carries the whole correctness condition
// and an unescaped dotted value matches any single character in its place. `Regex.escape` covers a strict superset
// of the RE2 metacharacter set, and RE2 accepts every member of that superset escaped.
const _alternate = (values: ReadonlyArray<string>): string => Array.join(Array.map(values, Regex.escape), "|")

const _complement = (of: Query): Query => Query.Binary({ left: Query.Const({ value: Query.finite(1) }), op: "sub", right: of })

const _goodShare = (sli: Extract<Sli, { readonly _tag: "Partition" | "Ratio" }>, labels: Query.Labels, span: Query.Window): Query =>
  sli._tag === "Ratio"
    ? Query.Binary({ left: _rated(sli.good, labels, span), op: "div", right: _rated(sli.total, labels, span) })
    : Query.Binary({
      left: _rated(sli.metric, labels, span, [{ key: sli.by, op: "regex", value: _alternate(sli.good) }]),
      op: "div",
      right: _rated(sli.metric, labels, span),
    })

const _timeShare = (metric: Convention.MetricName, bound: Query.Finite, op: keyof typeof _OPS, labels: Query.Labels, span: Query.Window): Query =>
  Query.Windowed({ fn: "avg", of: Query.Binary({ left: Query.Instant({ labels, metric }), op, right: Query.Const({ value: bound }) }), window: span })

const _breach = (sli: Sli, window: Query.Window, labels: Query.Labels): Query => {
  const span = typeof window === "string" ? window : Query.span(window)
  // every temporal bound rides its metric's own unit through the duration projection: a staleness horizon, a classic
  // bucket edge, a native fraction upper, and a SQL count threshold are one number, so no render arm re-derives a
  // scale and a millisecond-coded level is never compared against a bound counted in seconds
  return Match.valueTags(sli, {
    Freshness: ({ horizon, metric }) => _timeShare(metric, Query.finite(Convention.duration(metric, horizon)), "gt", labels, span),
    Latency: ({ ceiling, metric }) =>
      _complement(Query.Fraction({ labels, metric, upper: Query.finite(Convention.duration(metric, ceiling)), window: span })),
    Partition: (row) => _complement(_goodShare(row, labels, span)),
    Ratio: (row) => _complement(_goodShare(row, labels, span)),
    Saturation: ({ bound, breach, metric }) => _timeShare(metric, Query.finite(bound), _POLARITY[breach], labels, span),
  })
}

// Operators read this observable and alerts fire on the error-rate expression beside it; the two diverge
// only where an indicator is no breach complement — latency reads its quantile while its breach reads a
// le-share complement — so both folds seat at this owner and no consumer plane spells either.
const _indicator = (sli: Sli, window: Query.Window, labels: Query.Labels): Query => {
  const span = typeof window === "string" ? window : Query.span(window)
  return Match.valueTags(sli, {
    Freshness: ({ horizon, metric }) => _timeShare(metric, Query.finite(Convention.duration(metric, horizon)), "gt", labels, span),
    Latency: ({ metric, quantile }) => Query.Quantile({ labels, metric, q: Query.quantile(quantile), window: span }),
    Partition: (row) => _goodShare(row, labels, span),
    Ratio: (row) => _goodShare(row, labels, span),
    Saturation: ({ bound, breach, metric }) => _timeShare(metric, Query.finite(bound), _POLARITY[breach], labels, span),
  })
}

const _burned = (spec: Alert.Spec, labels: Query.Labels): Query => {
  const threshold = Query.Const({ value: Query.finite(spec.factor * (1 - spec.target)) })
  const exceeds = (window: Duration.Duration): Query =>
    Query.Binary({ left: _breach(spec.sli, Query.span(window), labels), op: "gt", right: threshold })
  return Query.Binary({ left: exceeds(spec.windows.short), op: "and", right: exceeds(spec.windows.long) })
}

type _Pane = { readonly span: typeof _Span.Type; readonly title: string } // the two literals every pane row carries whatever it renders

const _legend = (axes: ReadonlyArray<Query.Key>): Option.Option<string> =>
  Array.match(axes, {
    onEmpty: Option.none,
    onNonEmpty: (keys) => Option.some(Array.join(Array.map(keys, (key) => `{{${key}}}`), " ")),
  })

// Display units DERIVE from the series a pane overlays: the metric's own row answers its UCUM code and
// `Convention.grafanaUnit` answers the renderer's id for the fold the pane runs, so no pane spells a display word, a
// pane cannot mislabel its own series, and the empty-string sentinel a hand-spelled column carried has no seat. An
// overlay is code-uniform by the one-unit-per-name law, so the head metric answers for the whole pane.
const _display = (metric: Convention.MetricName, fold: Convention.Display): Option.Option<string> =>
  Option.some(Convention.grafanaUnit[Convention.named[metric].unit][fold])

// Trend panes differ by five literals alone, so the rows carry them and one builder renders the family: a sixth trend is
// one row, the rate-versus-increase choice is the row's own `fn`, and the legend derives from the grouping axes.
const _TRENDS = {
  auditActions: { axes: [Convention.rasm.auditAction], fn: "rate", labels: _tenant, metric: Convention.metric.factDrained, span: { h: 8, w: 16 }, title: "audit actions" },
  chartFrames: { axes: [], fn: "rate", labels: {}, metric: Convention.metric.chartFrames, span: { h: 6, w: 8 }, title: "pivot delta frames" },
  formSubmits: { axes: [Convention.rasm.formOutcome], fn: "rate", labels: {}, metric: Convention.metric.formSubmit, span: { h: 8, w: 12 }, title: "submit trips by outcome" },
  gatewayOutcomes: { axes: [Convention.rasm.gatewayOutcome], fn: "rate", labels: {}, metric: Convention.metric.gatewayCommands, span: { h: 8, w: 12 }, title: "gateway outcomes" },
  invokeFaults: { axes: [Convention.wire.occurrence], fn: "rate", labels: {}, metric: Convention.metric.invokeFault, span: { h: 8, w: 12 }, title: "fault reasons" }, // the frequency export mints the reason under the owned occurrence axis
  invokeOutcomes: { axes: [Convention.rasm.invokeOutcome], fn: "rate", labels: {}, metric: Convention.metric.invokeCalls, span: { h: 8, w: 12 }, title: "invoke outcomes" },
  objectWrites: { axes: [Convention.rasm.objectOutcome], fn: "rate", labels: {}, metric: Convention.metric.objectWritten, span: { h: 8, w: 12 }, title: "writes by outcome" },
  olapRetries: { axes: [Convention.rasm.olapEngine], fn: "rate", labels: {}, metric: Convention.metric.olapRetried, span: { h: 8, w: 12 }, title: "queries retried" },
  sceneGrafts: { axes: [], fn: "rate", labels: {}, metric: Convention.metric.sceneGrafts, span: { h: 6, w: 8 }, title: "graft arrivals" },
  // refusal reasons ride the owned occurrence axis both metric bridges append, so this pane splits on the frequency export's own key
  sceneRefusals: { axes: [Convention.wire.occurrence], fn: "rate", labels: {}, metric: Convention.metric.sceneRefusals, span: { h: 8, w: 12 }, title: "graft refusals by reason" },
  securityDenials: { axes: [Convention.rasm.securityReason], fn: "rate", labels: {}, metric: Convention.metric.securityPolicyDeny, span: { h: 8, w: 12 }, title: "authorization denials" },
  securityJwksMisses: { axes: [], fn: "rate", labels: {}, metric: Convention.metric.securityJwksMiss, span: { h: 6, w: 8 }, title: "cold JWKS resolutions" },
  securityKeyQuarantines: { axes: [], fn: "rate", labels: {}, metric: Convention.metric.securityJwksQuarantined, span: { h: 6, w: 8 }, title: "keys quarantined" },
  securityRejects: { axes: [Convention.rasm.securityKind], fn: "rate", labels: {}, metric: Convention.metric.securityRejects, span: { h: 8, w: 14 }, title: "authenticity rejects" },
  // rotated-refresh reuse is a credential-theft signal a kind-grouped rate buries, so the replay half reads on its own axis
  securityReplays: { axes: [Convention.rasm.securitySurface], fn: "rate", labels: { [Convention.rasm.securityKind]: "reuse" }, metric: Convention.metric.securityRejects, span: { h: 8, w: 10 }, title: "replayed credentials" },
  securityRotations: { axes: [], fn: "rate", labels: {}, metric: Convention.metric.securitySecretRotation, span: { h: 6, w: 8 }, title: "secret rotations" },
  securityShreds: { axes: [], fn: "rate", labels: {}, metric: Convention.metric.securityShredReject, span: { h: 6, w: 8 }, title: "shredded-key opens" },
  verdicts: { axes: [Convention.rasm.benchVerdict], fn: "rate", labels: {}, metric: Convention.metric.benchVerdicts, span: { h: 8, w: 12 }, title: "regression verdicts" },
  vitalGrades: { axes: [Convention.rasm.vitalKind, Convention.rasm.vitalGrade], fn: "rate", labels: {}, metric: Convention.metric.vitalObserved, span: { h: 8, w: 24 }, title: "observations by grade" },
} as const satisfies Record<string, _Pane & { readonly axes: ReadonlyArray<Query.Key>; readonly fn: _Fn; readonly labels: Query.Labels; readonly metric: Convention.MetricName }>

// Facet tables differ by the same five literals over a day-long increase, so they ride one row family beside the trends
// rather than three builders whose only divergence is an axis list.
const _FACETS = {
  auditActors: { axes: [Convention.rasm.auditActorKind, Convention.rasm.auditAction], labels: _tenant, metric: Convention.metric.factDrained, span: { h: 8, w: 8 }, title: "actors by action" },
  // fingerprint and hop evidence rides the fatal log stream, so this table groups the capture counter's one declared fan
  crashClasses: { axes: [Convention.attr.errorType], labels: {}, metric: Convention.metric.crashCaptured, span: { h: 8, w: 18 }, title: "captures by class" },
  securityFacets: {
    axes: [Convention.rasm.securityKind, Convention.rasm.securityDialect, Convention.rasm.securitySurface, Convention.rasm.securityReason],
    labels: {},
    metric: Convention.metric.securityRejects,
    span: { h: 8, w: 10 },
    title: "rejects by facet",
  },
} as const satisfies Record<string, _Pane & { readonly axes: ReadonlyArray<Query.Key>; readonly labels: Query.Labels; readonly metric: Convention.MetricName }>

// Level panes read instants with no window fold at all, so one row carries both the metric list a single panel
// overlays and the legend axis those series split on; the overlay is non-empty by type because its head metric is what
// answers the pane's display unit.
const _LEVELS = {
  // redelivery rides the depth panel: a rising claimed-twice line against depth is the stall signature
  derivativePressure: { axes: [], metrics: [Convention.metric.derivativeActive, Convention.metric.derivativeQueued], span: { h: 6, w: 9 }, title: "derivative pressure" },
  laneProgress: { axes: [Convention.rasm.laneName], metrics: [Convention.metric.laneCheckpoint], span: { h: 6, w: 9 }, title: "lane checkpoints" },
  poolLeases: { axes: [Convention.rasm.poolScheme], metrics: [Convention.metric.poolHeld], span: { h: 6, w: 12 }, title: "pool leases held" },
  workDepth: {
    axes: [],
    metrics: [Convention.metric.outboxDepth, Convention.metric.queueDepth, Convention.metric.outboxRedelivered],
    span: { h: 8, w: 12 },
    title: "outbox and queue depth",
  },
} as const satisfies Record<string, _Pane & { readonly axes: ReadonlyArray<Query.Key>; readonly metrics: Array.NonEmptyReadonlyArray<Convention.MetricName> }>

// Flow panes overlay RATES of counters sharing one UCUM code, so the display unit is that code's throughput spelling
// and incompatible codes never share an axis. `axes` carries the grouping each overlay splits on, so a fanned
// counter reads per key rather than one line per replica and an unfanned one keeps the empty legend.
const _FLOWS = {
  // Landing accounting is TWO halves on ONE pane: a drain rate standing alone claims zero redelivery, and zero
  // redelivery is precisely what a wedged retry re-offering one window forever looks like from the drain's own
  // series. The deduped half is the only series proving at-least-once delivery is happening at all.
  factLanding: {
    axes: [Convention.rasm.factStream],
    metrics: [Convention.metric.factDrained, Convention.metric.factDeduped],
    span: { h: 8, w: 12 },
    title: "facts landed and deduped",
  },
  objectFlow: {
    axes: [],
    metrics: [Convention.metric.objectSize, Convention.metric.streamSize, Convention.metric.objectReclaimed],
    span: { h: 8, w: 12 },
    title: "landed, uploaded, reclaimed",
  },
} as const satisfies Record<string, _Pane & { readonly axes: ReadonlyArray<Query.Key>; readonly metrics: Array.NonEmptyReadonlyArray<Convention.MetricName<"counter">> }>

const _trend = (board: DashboardModel.Board, row: (typeof _TRENDS)[keyof typeof _TRENDS]): Panel =>
  Timeseries.make({
    exprs: [
      Query.render(
        Query.Aggregate({
          by: row.axes,
          of: Query.Windowed({ fn: row.fn, of: Query.Instant({ labels: row.labels, metric: row.metric }), window: _WINDOW }),
          op: "sum",
        }),
        board.target,
      ),
    ],
    legend: _legend(row.axes),
    source: board.target.source,
    span: row.span,
    steps: [],
    title: row.title,
    unit: _display(row.metric, "rate"),
  })

const _facets = (board: DashboardModel.Board, row: (typeof _FACETS)[keyof typeof _FACETS]): Panel =>
  Table.make({
    exprs: [
      Query.render(
        Query.Aggregate({
          by: row.axes,
          of: Query.Windowed({ fn: "increase", of: Query.Instant({ labels: row.labels, metric: row.metric }), window: _DAY }),
          op: "sum",
        }),
        board.target,
      ),
    ],
    legend: Option.none(),
    source: board.target.source,
    span: row.span,
    title: row.title,
  })

// Grouping axes ride the QUERY, never the legend alone: a legend template interpolates labels its own series must
// already split on, and the SQL leaf derives series identity from the keys an enclosing `Aggregate` threads down — so
// an axis stated only on the legend renders one collapsed relation under that target while the PromQL selector splits,
// and the two targets fork on a pane both were meant to answer identically. `max` is a level's identity per group; an
// axis-free row keeps the bare instant, because a keyless aggregation collapses the very overlay the pane exists to show.
const _grouped = (axes: ReadonlyArray<Query.Key>, of: Query): Query =>
  Array.match(axes, { onEmpty: () => of, onNonEmpty: (by) => Query.Aggregate({ by, of, op: "max" }) })

const _levels = (board: DashboardModel.Board, row: (typeof _LEVELS)[keyof typeof _LEVELS]): Panel =>
  Timeseries.make({
    exprs: Array.map(row.metrics, (metric) => Query.render(_grouped(row.axes, Query.Instant({ labels: {}, metric })), board.target)),
    legend: _legend(row.axes),
    source: board.target.source,
    span: row.span,
    steps: [],
    title: row.title,
    unit: _display(Array.headNonEmpty(row.metrics), "level"),
  })

const _flow = (board: DashboardModel.Board, row: (typeof _FLOWS)[keyof typeof _FLOWS]): Panel =>
  Timeseries.make({
    exprs: Array.map(row.metrics, (metric) =>
      Query.render(
        Query.Aggregate({
          by: row.axes,
          of: Query.Windowed({ fn: "rate", of: Query.Instant({ labels: {}, metric }), window: _WINDOW }),
          op: "sum",
        }),
        board.target,
      )),
    legend: _legend(row.axes),
    source: board.target.source,
    span: row.span,
    steps: [],
    title: row.title,
    unit: _display(Array.headNonEmpty(row.metrics), "rate"),
  })

const _quantile = (row: { readonly labels: Query.Labels; readonly metric: Convention.MetricName<"histogram">; readonly title: string }) =>
(board: DashboardModel.Board) =>
(quantile: Query.QuantileValue): Panel =>
  Timeseries.make({
    exprs: [Query.render(Query.Quantile({ labels: row.labels, metric: row.metric, q: quantile, window: _WINDOW }), board.target)],
    legend: Option.none(),
    source: board.target.source,
    span: { h: 8, w: 12 },
    steps: [],
    title: `${row.title} p${Number.round(quantile * 100, 0)}`,
    unit: _display(row.metric, "level"), // a rung IS the quantity, so the level column answers whatever code the histogram declares
  })

// semconv fixes its duration histogram in seconds where every rasm row here measures milliseconds, so each row's own
// code rides the display projection and a quantile can no longer be labelled three decades off its own descriptor
const _latency = _quantile({ labels: _tenant, metric: Convention.metric.httpServerDuration, title: "latency" })
const _invokeLatency = _quantile({ labels: {}, metric: Convention.metric.invokeDuration, title: "invoke" }) // the capability instruments are process-level: no tenant tag exists on their series
const _gatewayLatency = _quantile({ labels: {}, metric: Convention.metric.gatewayDuration, title: "gateway" })
const _batchLatency = _quantile({ labels: {}, metric: Convention.metric.batchDuration, title: "batch window" })
const _lakeWait = _quantile({ labels: {}, metric: Convention.metric.olapWait, title: "lake wait" })
const _lakeDeferred = _quantile({ labels: {}, metric: Convention.metric.olapDeferred, title: "deferred wait" })
const _lakeProfile = _quantile({ labels: {}, metric: Convention.metric.profileDuration, title: "engine profile" })
const _jwksLatency = _quantile({ labels: {}, metric: Convention.metric.securityJwksResolve, title: "JWKS resolve" })
const _kdfLatency = _quantile({ labels: {}, metric: Convention.metric.securityKdf, title: "key derivation" })

const _vitalGauge = (board: DashboardModel.Board) =>
(gauge: { readonly ceiling: number; readonly kind: string; readonly metric: Convention.MetricName<"gauge"> }): Panel =>
  Gauge.make({
    ceiling: gauge.ceiling,
    expr: Query.render(
      Query.Windowed({
        fn: "avg",
        // Level series split per UCUM code, so the producer's row names one and this fold picks none
        of: Query.Instant({ labels: { [Convention.rasm.vitalKind]: gauge.kind }, metric: gauge.metric }),
        window: _WINDOW,
      }),
      board.target,
    ),
    source: board.target.source,
    span: { h: 6, w: 4 },
    steps: [{ at: gauge.ceiling, tone: Alert.severity.page.tone }], // the paging tone reads slo's own severity table: no tone correspondence is re-declared here
    title: gauge.kind,
  })

const _usage = (board: DashboardModel.Board) => (resource: string): Panel =>
  Timeseries.make({
    exprs: [
      Query.render(
        Query.Aggregate({
          by: [Convention.rasm.tenant],
          of: Query.Windowed({
            fn: "increase",
            of: Query.Instant({ labels: { [Convention.rasm.meterResource]: resource, ..._tenant }, metric: Convention.metric.meterUsage }),
            window: _WINDOW,
          }),
          op: "sum",
        }),
        board.target,
      ),
    ],
    legend: _legend([Convention.rasm.tenant]),
    source: board.target.source,
    span: { h: 8, w: 12 },
    steps: [],
    title: `usage ${resource}`,
    unit: _display(Convention.metric.meterUsage, "level"), // an increase over a window counts, so it reads as the quantity rather than a rate
  })

// Log streams are a plane the query target never names, so this pane binds the board's own log datasource
const _crashes = (board: DashboardModel.Board): Panel =>
  Logs.make({
    filter: Convention.event.exception,
    source: board.logs,
    span: { h: 8, w: 24 },
    title: "exception records",
  })

const _crashRate = (board: DashboardModel.Board): Panel =>
  Stat.make({
    expr: Query.render(
      Query.Windowed({ fn: "rate", of: Query.Instant({ labels: {}, metric: Convention.metric.crashCaptured }), window: _WINDOW }),
      board.target,
    ),
    source: board.target.source,
    span: { h: 6, w: 6 },
    steps: [],
    title: "crash capture rate",
    unit: _display(Convention.metric.crashCaptured, "rate"),
  })

const _workFlow = (board: DashboardModel.Board): Panel =>
  Timeseries.make({
    exprs: [
      Query.render(
        Query.Aggregate({
          by: [Convention.rasm.workChannel],
          of: Query.Windowed({ fn: "rate", of: Query.Instant({ labels: {}, metric: Convention.metric.relayDrained }), window: _WINDOW }),
          op: "sum",
        }),
        board.target,
      ),
      Query.render(
        Query.Aggregate({
          by: [],
          of: Query.Windowed({ fn: "rate", of: Query.Instant({ labels: {}, metric: Convention.metric.queueParked }), window: _WINDOW }),
          op: "sum",
        }),
        board.target,
      ),
    ],
    legend: _legend([Convention.rasm.workChannel]),
    source: board.target.source,
    span: { h: 8, w: 12 },
    steps: [],
    title: "relay drain and parked",
    unit: _display(Convention.metric.relayDrained, "rate"), // both overlaid counters carry the deliverable code, so one answers the axis
  })

const _workAge = (board: DashboardModel.Board): Panel =>
  Stat.make({
    expr: Query.render(Query.Aggregate({ by: [], of: Query.Instant({ labels: {}, metric: Convention.metric.outboxAge }), op: "max" }), board.target),
    source: board.target.source,
    span: { h: 4, w: 6 },
    steps: [],
    title: "oldest undelivered age",
    unit: _display(Convention.metric.outboxAge, "level"),
  })

// Grouping seats on each OPERAND, never on the quotient. PromQL matches two label-identical vectors per cache
// unaided, but the SQL fold joins on a series identity built from the keys an enclosing `Aggregate` threads DOWN —
// with none, every cache collapses into one relation and the legend names a split the query never made. One
// `Query` value has to mean one thing under both renders, which is exactly what these two folds buy.
const _cacheShare = (board: DashboardModel.Board): Panel => {
  const perCache = (metric: Convention.MetricName<"gauge">): Query =>
    Query.Aggregate({ by: [Convention.rasm.cacheName], of: Query.Instant({ labels: {}, metric }), op: "sum" })
  const hits = perCache(Convention.metric.cacheHits)
  return Timeseries.make({
    exprs: [
      Query.render(
        Query.Binary({
          left: hits,
          op: "div",
          right: Query.Binary({ left: hits, op: "add", right: perCache(Convention.metric.cacheMisses) }),
        }),
        board.target,
      ),
    ],
    legend: _legend([Convention.rasm.cacheName]),
    source: board.target.source,
    span: { h: 6, w: 12 },
    steps: [],
    // a quotient of two coded series carries no code the vocabulary owns, so no display id derives and spelling one
    // here would hand-write the correspondence the projection exists to hold
    title: "cache hit share",
    unit: Option.none(),
  })
}

const _EVIDENCE = Query.span(Duration.days(30)) // the residence horizon: the window a metrics store's retention cannot hold

// Residence panes render the SAME algebra against the analytics target, so an evidence tile is one row rather than a
// second query owner, and the horizon is the residence's own — which is the whole reason the plane stands beside the
// store. A stack declining the residence renders no tile at all instead of one aimed at an absent door.
const _evidence = (board: DashboardModel.Board, row: {
  readonly axes: ReadonlyArray<Query.Key>
  readonly metric: Convention.MetricName<"counter">
  readonly span: typeof _Span.Type
  readonly title: string
}): ReadonlyArray<Panel> =>
  Option.match(board.analytics, {
    onNone: () => [],
    onSome: (target) => [
      Timeseries.make({
        exprs: [
          Query.render(
            Query.Aggregate({
              by: row.axes,
              of: Query.Windowed({ fn: "increase", of: Query.Instant({ labels: {}, metric: row.metric }), window: _EVIDENCE }),
              op: "sum",
            }),
            target,
          ),
        ],
        legend: _legend(row.axes),
        source: target.source,
        span: row.span,
        steps: [],
        title: row.title,
        unit: _display(row.metric, "level"),
      }),
    ],
  })

const _benchLadder = (board: DashboardModel.Board) => (suite: string): Panel =>
  Timeseries.make({
    exprs: [
      Query.render(
        _grouped(
          [Convention.rasm.benchBand, Convention.rasm.benchLabel],
          Query.Instant({ labels: { [Convention.rasm.benchSuite]: suite }, metric: Convention.metric.benchTime }),
        ),
        board.target,
      ),
    ],
    legend: _legend([Convention.rasm.benchLabel, Convention.rasm.benchBand]),
    source: board.target.source,
    span: { h: 8, w: 12 },
    steps: [],
    title: `${suite} timing ladder`,
    unit: _display(Convention.metric.benchTime, "level"),
  })

// One panel per enrichment band, each band its own instrument row: the code rides that row, so the three bands never
// share an axis and none re-spells a display word.
const _BENCH_ENRICHMENT = [
  { metric: Convention.metric.benchGc, title: "gc timing" },
  { metric: Convention.metric.benchHeap, title: "heap delta" },
  { metric: Convention.metric.benchCounter, title: "hardware counters" },
] as const

const _benchEnrichment = (board: DashboardModel.Board, suite: string, row: (typeof _BENCH_ENRICHMENT)[number]): Panel =>
  Timeseries.make({
    exprs: [
      Query.render(
        _grouped([Convention.rasm.benchBand, Convention.rasm.benchLabel], Query.Instant({ labels: { [Convention.rasm.benchSuite]: suite }, metric: row.metric })),
        board.target,
      ),
    ],
    legend: _legend([Convention.rasm.benchLabel, Convention.rasm.benchBand]),
    source: board.target.source,
    span: { h: 8, w: 12 },
    steps: [],
    title: `${suite} ${row.title}`,
    unit: _display(row.metric, "level"),
  })

const _burnPair = (board: DashboardModel.Board) => (spec: Alert.Spec): Panel =>
  Timeseries.make({
    exprs: Array.map([spec.windows.long, spec.windows.short], (window) =>
      Query.render(
        Query.Binary({ left: Query.breach(spec.sli, Query.span(window), _tenant), op: "div", right: Query.Const({ value: Query.finite(1 - spec.target) }) }),
        board.target,
      )),
    legend: Option.none(),
    source: board.target.source,
    span: { h: 6, w: 12 },
    steps: [{ at: spec.factor, tone: spec.severity.tone }],
    title: `${spec.slug} trips at ${spec.factor}x — ${Number.round(spec.spend * 100, 1)}% budget`, // the derived spend prints here: the human figure cannot drift from the row that fires it
    unit: Option.none(),
  })

declare namespace DashboardModel {
  type Pack = keyof Payload
  type Payload = {
    readonly audit: Record.ReadonlyRecord<never, never>
    readonly bench: { readonly suites: ReadonlyArray<string> }
    readonly crash: Record.ReadonlyRecord<never, never>
    readonly invoke: { readonly quantiles: ReadonlyArray<Query.QuantileValue> }
    readonly lake: { readonly quantiles: ReadonlyArray<Query.QuantileValue> }
    readonly meter: { readonly resources: ReadonlyArray<string> }
    readonly object: Record.ReadonlyRecord<never, never>
    readonly overview: { readonly quantiles: ReadonlyArray<Query.QuantileValue> }
    readonly security: { readonly quantiles: ReadonlyArray<Query.QuantileValue> }
    readonly slo: { readonly objectives: ReadonlyArray<Slo.Objective> }
    readonly vital: {
      readonly gauges: ReadonlyArray<{ readonly ceiling: number; readonly kind: string; readonly metric: Convention.MetricName<"gauge"> }>
    }
    readonly view: Record.ReadonlyRecord<never, never>
    readonly work: { readonly quantiles: ReadonlyArray<Query.QuantileValue> }
  }
  type Suite = Payload["bench"] & Payload["meter"] & Payload["overview"] & Payload["slo"] & Payload["vital"]
}

const _PACKS: { readonly [K in DashboardModel.Pack]: (board: DashboardModel.Board, payload: DashboardModel.Payload[K]) => DashboardModel } = {
  audit: (board) =>
    DashboardModel.of(board, {
      annotations: [],
      panels: [_trend(board, _TRENDS.auditActions), _facets(board, _FACETS.auditActors)],
      slug: "audit",
      tags: ["audit"],
      title: "audit",
      variables: [],
    }),
  bench: (board, payload) =>
    DashboardModel.of(board, {
      annotations: [],
      panels: [
        ...Array.flatMap(payload.suites, (suite) => [
          _benchLadder(board)(suite),
          ...Array.map(_BENCH_ENRICHMENT, (row) => _benchEnrichment(board, suite, row)),
        ]),
        _trend(board, _TRENDS.verdicts),
      ],
      slug: "bench",
      tags: ["bench"],
      title: "benchmarks",
      variables: [],
    }),
  crash: (board) =>
    DashboardModel.of(board, {
      annotations: [],
      panels: [_crashRate(board), _facets(board, _FACETS.crashClasses), _crashes(board)],
      slug: "crash",
      tags: ["crash"],
      title: "crash",
      variables: [],
    }),
  invoke: (board, payload) =>
    DashboardModel.of(board, {
      annotations: [],
      panels: [
        _trend(board, _TRENDS.invokeOutcomes),
        _trend(board, _TRENDS.gatewayOutcomes),
        _trend(board, _TRENDS.invokeFaults),
        ...Array.map(payload.quantiles, _invokeLatency(board)),
        ...Array.map(payload.quantiles, _gatewayLatency(board)),
      ],
      slug: "invoke",
      tags: ["invoke", "capability"],
      title: "capability plane",
      variables: [],
    }),
  lake: (board, payload) =>
    DashboardModel.of(board, {
      annotations: [],
      panels: [
        ...Array.map(payload.quantiles, _lakeWait(board)),
        ...Array.map(payload.quantiles, _lakeDeferred(board)),
        ...Array.map(payload.quantiles, _lakeProfile(board)),
        _trend(board, _TRENDS.olapRetries),
        _cacheShare(board),
        _levels(board, _LEVELS.poolLeases),
        ..._evidence(board, {
          axes: [Convention.rasm.olapEngine],
          metric: Convention.metric.olapRetried,
          span: { h: 8, w: 12 },
          title: "retries over the evidence horizon",
        }),
      ],
      slug: "lake",
      tags: ["lake", "storage"],
      title: "storage harvest",
      variables: [],
    }),
  meter: (board, payload) =>
    DashboardModel.of(board, {
      annotations: [],
      panels: Array.map(payload.resources, _usage(board)),
      slug: "meter",
      tags: ["meter", "billing"],
      title: "usage",
      variables: [],
    }),
  object: (board) =>
    DashboardModel.of(board, {
      annotations: [],
      panels: [_trend(board, _TRENDS.objectWrites), _flow(board, _FLOWS.objectFlow)],
      slug: "object",
      tags: ["object", "storage"],
      title: "object plane",
      variables: [],
    }),
  overview: (board, payload) =>
    DashboardModel.of(board, {
      annotations: [],
      panels: Array.map(payload.quantiles, _latency(board)),
      slug: "overview",
      tags: ["overview"],
      title: "service overview",
      variables: [],
    }),
  security: (board, payload) =>
    DashboardModel.of(board, {
      annotations: [],
      panels: [
        _trend(board, _TRENDS.securityRejects),
        _facets(board, _FACETS.securityFacets),
        _trend(board, _TRENDS.securityDenials),
        _trend(board, _TRENDS.securityReplays),
        _trend(board, _TRENDS.securityRotations),
        _trend(board, _TRENDS.securityJwksMisses),
        _trend(board, _TRENDS.securityKeyQuarantines),
        _trend(board, _TRENDS.securityShreds),
        ...Array.map(payload.quantiles, _jwksLatency(board)),
        ...Array.map(payload.quantiles, _kdfLatency(board)),
      ],
      slug: "security",
      tags: ["security"],
      title: "authenticity and custody",
      variables: [],
    }),
  slo: (board, payload) =>
    DashboardModel.of(board, {
      annotations: Array.flatMap(payload.objectives, (objective) =>
        Array.map(Alert.of(objective), (spec) => ({ slug: spec.slug, tone: spec.severity.tone }))),
      panels: Array.flatMap(payload.objectives, (objective) => Array.map(Alert.of(objective), _burnPair(board))),
      slug: "slo",
      tags: ["slo"],
      title: "objectives",
      variables: [],
    }),
  vital: (board, payload) =>
    DashboardModel.of(board, {
      annotations: [],
      panels: [...Array.map(payload.gauges, _vitalGauge(board)), _trend(board, _TRENDS.vitalGrades)],
      slug: "vital",
      tags: ["vital", "rum"],
      title: "web vitals",
      variables: [],
    }),
  view: (board) =>
    DashboardModel.of(board, {
      annotations: [],
      panels: [
        _trend(board, _TRENDS.sceneGrafts),
        _trend(board, _TRENDS.sceneRefusals),
        _trend(board, _TRENDS.chartFrames),
        _trend(board, _TRENDS.formSubmits),
      ],
      slug: "view",
      tags: ["view", "ui"],
      title: "view plane",
      variables: [],
    }),
  work: (board, payload) =>
    DashboardModel.of(board, {
      annotations: [],
      panels: [
        _levels(board, _LEVELS.workDepth),
        _workFlow(board),
        _flow(board, _FLOWS.factLanding),
        _workAge(board),
        _levels(board, _LEVELS.laneProgress),
        _levels(board, _LEVELS.derivativePressure),
        ...Array.map(payload.quantiles, _batchLatency(board)),
      ],
      slug: "work",
      tags: ["work", "durable"],
      title: "durable work",
      variables: [],
    }),
}

const _SUITE: { readonly [K in DashboardModel.Pack]: (board: DashboardModel.Board, payload: DashboardModel.Suite) => DashboardModel } = {
  audit: (board) => _PACKS.audit(board, {}),
  bench: (board, payload) => _PACKS.bench(board, { suites: payload.suites }),
  crash: (board) => _PACKS.crash(board, {}),
  invoke: (board, payload) => _PACKS.invoke(board, { quantiles: payload.quantiles }),
  lake: (board, payload) => _PACKS.lake(board, { quantiles: payload.quantiles }),
  meter: (board, payload) => _PACKS.meter(board, { resources: payload.resources }),
  object: (board) => _PACKS.object(board, {}),
  overview: (board, payload) => _PACKS.overview(board, { quantiles: payload.quantiles }),
  security: (board, payload) => _PACKS.security(board, { quantiles: payload.quantiles }),
  slo: (board, payload) => _PACKS.slo(board, { objectives: payload.objectives }),
  vital: (board, payload) => _PACKS.vital(board, { gauges: payload.gauges }),
  view: (board) => _PACKS.view(board, {}),
  work: (board, payload) => _PACKS.work(board, { quantiles: payload.quantiles }),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Bench, DashboardModel, Query }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
