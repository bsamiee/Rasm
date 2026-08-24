# [CORE_BOARD]

`Board` renders the estate's observability read surface from data alone: `Query` compiles one expression tree to every backend from a target parameter, `Panel` and `DashboardModel` encode dashboards as wire values the deploy plane realizes, `Bench` grades benchmark claims through an admission ladder, and the pack table gives every instrument family its standing consumer. Its module is `core/src/observe/board.ts`.

## [01]-[INDEX]

- [02]-[QUERY]: target-parameterized expression algebra — PromQL and residence SQL from one tree; `Board.Query`.
- [03]-[PANEL]: closed panel schema union every dashboard encodes; `Panel`.
- [04]-[MODEL]: dashboard identity, grid layout, live-metric snapshot, pack dispatch; `Board.DashboardModel`.
- [05]-[BENCH]: benchmark claims, mitata ingestion, admission-gated regression grading; `Board.Bench`.
- [06]-[PACKS]: standing pack and suite builders over the instrument estate; `Board.DashboardModel.pack`/`suite`.

## [02]-[QUERY]

- Owner: `Board.Query` compiles one tagged expression tree — instant, windowed, aggregate, binary, quantile, fraction, rank, const — and `render` answers each `Target` arm in that store's own dialect, so a panel authors one expression estate-wide.
- Law: counter windows fold monotonic resets through the lag-increment leg, so a restarted process reads as its true increase rather than a negative spike.
- Law: scalar-only binary arms fold to constants before any relation forms, so a threshold comparison never joins a relation against a broadcast constant.
- Law: `breach`, `indicator`, and `burn` derive their expressions from `Reliability` values alone, so an objective's board query and its alert rule read one definition.
- Growth: a rendering backend is one `_ENGINES` row; a windowed verb one `_FNS` row; an operator one `_OPS` row spelled in both dialects.
- Boundary: residence DDL and datasource realization are the data and deploy planes'; this owner emits expression strings alone.
- Packages: `effect` (`Data`, `Duration`, `Match`, `Schema`); `./convention.ts` (`Convention`); `./slo.ts` (`Reliability`).

```typescript signature
import { create, isMessage, type MessageShape } from "@bufbuild/protobuf"
import { EmptySchema, timestampFromMs, timestampMs } from "@bufbuild/protobuf/wkt"
import * as evidence from "@rasm\/contracts/rasm/contracts/benchmark/claim_pb"
import * as fingerprint from "@rasm\/contracts/rasm/contracts/benchmark/fingerprint_pb"
import { Array, Data, DateTime, Duration, Effect, Either, Match, Metric, MetricPair, MetricState, Number, Option, Order, ParseResult, Predicate, Record, RegExp as Regex, Schema, type SchemaAST, Struct, pipe } from "effect"
import type { measure as MitataMeasure } from "mitata"
import { Digest } from "../value/contentKey.ts"
import { Identity } from "../value/identity.ts"
import { Shape } from "../value/schema.ts"
import { Convention } from "./convention.ts"
import { Reliability } from "./slo.ts"

const _FNS = {
  avg: { promql: "avg_over_time", sql: (value: string) => `avg(${value})` },
  delta: { promql: "delta", sql: (value: string, _seconds: number, time: string, engine: _Engine) => `${engine.latest(value, time)} - ${engine.earliest(value, time)}` },
  increase: { promql: "increase", sql: (value: string) => `sum(${value})` },
  max: { promql: "max_over_time", sql: (value: string) => `max(${value})` },
  min: { promql: "min_over_time", sql: (value: string) => `min(${value})` },
  rate: { promql: "rate", sql: (value: string, seconds: number) => `sum(${value}) / ${seconds}` },
} as const satisfies Record<string, {
  readonly promql: string
  readonly sql: (value: string, seconds: number, time: string, engine: _Engine) => string
}>

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
const _MATCH = {
  equal: { negate: false, promql: "=", sql: "compare" },
  notRegex: { negate: true, promql: "!~", sql: "match" },
  regex: { negate: false, promql: "=~", sql: "match" },
  unequal: { negate: true, promql: "!=", sql: "compare" },
} as const satisfies Record<
  (typeof Reliability.Filter.Op.kinds)[number],
  { readonly negate: boolean; readonly promql: string; readonly sql: "compare" | "match" }
>
const _INTERVAL = { rate: "$__rate_interval" } as const
const _DIALECT = [Convention.wire.occurrence, "le", "quantile"] as const
const _POLARITY = { ceiling: "gt", floor: "lt" } as const satisfies Record<Reliability.Slo.Polarity, keyof typeof _OPS>
const _COLUMN = { at: "at", by: "by", prior: "prior", value: "v" } as const

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

declare namespace _Query {
  type Dialect = (typeof _DIALECT)[number]
  type Labels = { readonly [K in Convention.Key]?: Convention.ValueOf<K> extends ReadonlyArray<Convention.Scalar> ? never : Convention.ValueOf<K> }
    & { readonly [K in Dialect]?: string }
  type Matcher = Reliability.Filter
  type Finite = _Finite
  type QuantileValue = _Quantile
  type Span = _QuerySpan
  type Window = Span | (typeof _INTERVAL)[keyof typeof _INTERVAL]
  type Engine = keyof typeof _ENGINES
  type Histogram = "classic" | "native"
  type Key = Convention.Key | Dialect
  type Residence = {
    readonly attribute: (key: Key) => string
    readonly degrade: string
    readonly fraction: (below: number) => string
    readonly identity: (keys: ReadonlyArray<Key>) => string
    readonly name: string
    readonly quantile: (at: number) => string
    readonly table: { readonly [K in Convention.InstrumentKind]: string }
    readonly time: string
    readonly value: { readonly [K in Convention.InstrumentKind]: string }
  }
  type Target = _Target
}

const _ENGINES = {
  clickhouse: {
    aggregate: { any: "any", count: "count", deviation: "stddevPop", max: "max", mean: "avg", min: "min", sum: "sum", variance: "varPop" },
    bucket: (column: string, seconds: number) => `toStartOfInterval(${column}, INTERVAL ${seconds} SECOND)`,
    earliest: (value: string, time: string) => `argMin(${value}, ${time})`,
    latest: (value: string, time: string) => `argMax(${value}, ${time})`,
    match: (value: string, pattern: string) => `match(${value}, ${pattern})`,
    rank: (inner: string, order: string, count: number, _projection: string) =>
      `SELECT * FROM (${inner}) QUALIFY row_number() OVER (PARTITION BY ${_COLUMN.at} ORDER BY ${_COLUMN.value} ${order}) <= ${count}`,
    truth: (predicate: string) => `toFloat64(${predicate})`,
  },
  duckdb: {
    aggregate: { any: "any_value", count: "count", deviation: "stddev_pop", max: "max", mean: "avg", min: "min", sum: "sum", variance: "var_pop" },
    bucket: (column: string, seconds: number) => `time_bucket(INTERVAL '${seconds} seconds', ${column})`,
    earliest: (value: string, time: string) => `arg_min(${value}, ${time})`,
    latest: (value: string, time: string) => `arg_max(${value}, ${time})`,
    match: (value: string, pattern: string) => `regexp_matches(${value}, ${pattern})`,
    rank: (inner: string, order: string, count: number, _projection: string) =>
      `SELECT * FROM (${inner}) QUALIFY row_number() OVER (PARTITION BY ${_COLUMN.at} ORDER BY ${_COLUMN.value} ${order}) <= ${count}`,
    truth: (predicate: string) => `CAST(${predicate} AS DOUBLE)`,
  },
  postgres: {
    aggregate: { any: "any_value", count: "count", deviation: "stddev_pop", max: "max", mean: "avg", min: "min", sum: "sum", variance: "var_pop" },
    bucket: (column: string, seconds: number) => `time_bucket(INTERVAL '${seconds} seconds', ${column})`,
    earliest: (value: string, time: string) => `(array_agg(${value} ORDER BY ${time} ASC))[1]`,
    latest: (value: string, time: string) => `(array_agg(${value} ORDER BY ${time} DESC))[1]`,
    match: (value: string, pattern: string) => `${value} ~ ${pattern}`,
    rank: (inner: string, order: string, count: number, projection: string) =>
      `SELECT ${projection} FROM (SELECT *, row_number() OVER (PARTITION BY ${_COLUMN.at}`
      + ` ORDER BY ${_COLUMN.value} ${order}) AS rn FROM (${inner}) ranked) ordered WHERE rn <= ${count}`,
    truth: (predicate: string) => `CASE WHEN ${predicate} THEN 1.0 ELSE 0.0 END`, // boolean casts to a number nowhere on this engine
  },
} as const satisfies Record<string, {
  readonly aggregate: { readonly [F in _Fold]: string }
  readonly bucket: (column: string, seconds: number) => string
  readonly earliest: (value: string, time: string) => string
  readonly latest: (value: string, time: string) => string
  readonly match: (value: string, pattern: string) => string
  readonly rank: (inner: string, order: string, count: number, projection: string) => string
  readonly truth: (predicate: string) => string
}>

type _Engine = (typeof _ENGINES)[_Query.Engine]
type _Target = Data.TaggedEnum<{
  Promql: { readonly histogram: _Query.Histogram; readonly source: string; readonly translation: Convention.Translation }
  Sql: { readonly engine: _Query.Engine; readonly residence: _Query.Residence; readonly resolution: _Query.Span; readonly source: string }
}>
const _Target = Data.taggedEnum<_Target>()
type _Promql = Extract<_Target, { readonly _tag: "Promql" }>
type _Sql = Extract<_Target, { readonly _tag: "Sql" }>

type _Query = Data.TaggedEnum<{
  Aggregate: { readonly by: ReadonlyArray<_Query.Key>; readonly of: _Query; readonly op: _Agg; readonly without?: boolean }
  Binary: { readonly left: _Query; readonly op: _Op; readonly right: _Query }
  Const: { readonly value: _Query.Finite }
  Fraction: { readonly labels: _Query.Labels; readonly matchers?: ReadonlyArray<_Query.Matcher>; readonly metric: Convention.MetricName<"histogram">; readonly upper: _Query.Finite; readonly window: _Query.Window }
  Instant: { readonly labels: _Query.Labels; readonly matchers?: ReadonlyArray<_Query.Matcher>; readonly metric: Convention.MetricName }
  Quantile: { readonly labels: _Query.Labels; readonly matchers?: ReadonlyArray<_Query.Matcher>; readonly metric: Convention.MetricName<"histogram">; readonly q: _Query.QuantileValue; readonly window: _Query.Window }
  Rank: { readonly count: _RankCount; readonly of: _Query; readonly op: _Rank }
  Windowed: { readonly fn: _Fn; readonly of: _Query; readonly window: _Query.Window }
}>
const _QueryCases = Data.taggedEnum<_Query>()

const _LABEL_KEYS: ReadonlyArray<_Query.Key> = [...Convention.keys, ..._DIALECT]

const _literal = (value: Convention.Scalar): string => JSON.stringify(String(value)) ?? '""'

const _quoted = (value: Convention.Scalar): string => `'${String(value).replaceAll("'", "''")}'`

const _span = (window: _Query.Window): string =>
  typeof window === "string"
    ? window
    : pipe(Duration.toMillis(window), (millis) => millis % 1000 === 0 ? `${millis / 1000}s` : `${millis}ms`)

const _bucketed = (window: _Query.Window, resolution: _Query.Span): number =>
  Duration.toMillis(typeof window === "string" ? resolution : window) / 1000

const _promSeries = (metric: Convention.MetricName, row: _Promql): string =>
  `${Convention.translated(metric, row.translation)}${Convention.Metric.at(metric).kind === "histogram" && row.histogram === "classic" ? "_bucket" : ""}`

const _selector = (metric: Convention.MetricName, row: _Promql, labels: _Query.Labels, matchers: ReadonlyArray<_Query.Matcher> = []): string =>
  pipe(
    [
      ...Array.filterMap(_LABEL_KEYS, (key) =>
        Option.map(Option.fromNullable(labels[key]), (value) => `${_literal(key)}=${_literal(value)}`)),
      ...Array.map(matchers, ({ key, op, value }) => `${_literal(key)}${_MATCH[op].promql}${_literal(value)}`),
    ],
    (pairs) => `{${_literal(_promSeries(metric, row))}${pairs.length === 0 ? "" : `,${Array.join(pairs, ",")}`}}`,
  )

const _promql = (query: _Query, row: _Promql): string =>
  _Query.$match(query, {
    Aggregate: ({ by, of, op, without }) =>
      `${_AGG[op].promql}${by.length === 0 ? "" : ` ${without === true ? "without" : "by"} (${Array.join(Array.map(by, _literal), ",")})`} (${
        _promql(of, row)
      })`,
    Binary: ({ left, op, right }) => `(${_promql(left, row)}) ${_OPS[op].promql} (${_promql(right, row)})`,
    Const: ({ value }) => `${value}`,
    Fraction: ({ labels, matchers, metric, upper, window }) =>
      row.histogram === "native"
        ? `histogram_fraction(0, ${upper}, rate(${_selector(metric, row, labels, matchers ?? [])}[${_span(window)}]))`
        : `sum(rate(${_selector(metric, row, { ...labels, le: `${upper}` }, matchers ?? [])}[${_span(window)}])) / sum(rate(${
          _selector(metric, row, { ...labels, le: "+Inf" }, matchers ?? [])
        }[${_span(window)}]))`,
    Instant: ({ labels, matchers, metric }) => _selector(metric, row, labels, matchers),
    Quantile: ({ labels, matchers, metric, q, window }) =>
      row.histogram === "native"
        ? `histogram_quantile(${q}, sum(rate(${_selector(metric, row, labels, matchers ?? [])}[${_span(window)}])))`
        : `histogram_quantile(${q}, sum by (le) (rate(${_selector(metric, row, labels, matchers ?? [])}[${_span(window)}])))`,
    Rank: ({ count, of, op }) => `${_RANK[op].promql}(${count}, ${_promql(of, row)})`,
    Windowed: ({ fn, of, window }) =>
      of._tag === "Instant"
        ? `${_FNS[fn].promql}(${_promql(of, row)}[${_span(window)}])`
        : `${_FNS[fn].promql}((${_promql(of, row)})[${_span(window)}:])`,
  })

const _predicates = (
  row: _Sql,
  source: { readonly labels: _Query.Labels; readonly matchers?: ReadonlyArray<_Query.Matcher>; readonly metric: Convention.MetricName },
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

const _alias = (key: _Query.Key): string => `"${String(key).replaceAll('"', '""')}"`
const _keySelect = (row: _Sql, keys: ReadonlyArray<_Query.Key>): string =>
  Array.join(Array.map(keys, (key) => `, ${row.residence.attribute(key)} AS ${_alias(key)}`), "")
const _keyProject = (keys: ReadonlyArray<_Query.Key>, qualifier = ""): string =>
  Array.join(Array.map(keys, (key) => `, ${qualifier}${_alias(key)}`), "")
const _projection = (keys: ReadonlyArray<_Query.Key>, qualifier = ""): string =>
  `${qualifier}${_COLUMN.at}, ${qualifier}${_COLUMN.by}${_keyProject(keys, qualifier)}, ${qualifier}${_COLUMN.value}`
const _group = (keys: ReadonlyArray<_Query.Key>): string =>
  `GROUP BY ${Array.join(Array.range(1, 2 + keys.length), ", ")}`

const _leaf = (
  row: _Sql,
  keys: ReadonlyArray<_Query.Key>,
  source: { readonly labels: _Query.Labels; readonly matchers?: ReadonlyArray<_Query.Matcher>; readonly metric: Convention.MetricName },
  window: _Query.Window,
  value: (column: string, time: string, engine: _Engine) => string,
): string =>
  pipe(Convention.Metric.at(source.metric).kind, (kind) =>
    `SELECT ${_ENGINES[row.engine].bucket(row.residence.time, _bucketed(window, row.resolution))} AS ${_COLUMN.at},`
    + ` ${row.residence.identity(keys)} AS ${_COLUMN.by}${_keySelect(row, keys)}, ${value(row.residence.value[kind], row.residence.time, _ENGINES[row.engine])} AS ${_COLUMN.value}`
    + ` FROM ${row.residence.table[kind]}`
    + ` WHERE ${Array.join(_predicates(row, source), " AND ")} ${_group(keys)}`)

const _increment = (value: string, prior: string): string =>
  `CASE WHEN ${prior} IS NULL THEN 0 WHEN ${value} >= ${prior} THEN ${value} - ${prior} ELSE ${value} END`

const _resetLeaf = (
  row: _Sql,
  keys: ReadonlyArray<_Query.Key>,
  source: Extract<_Query, { readonly _tag: "Instant" }>,
  window: _Query.Window,
  fn: "increase" | "rate",
): string => {
  const kind = Convention.Metric.at(source.metric).kind
  const value = row.residence.value[kind]
  const seconds = _bucketed(window, row.resolution)
  const stepped = `SELECT *, lag(${value}) OVER (PARTITION BY ${row.residence.identity(_LABEL_KEYS)} ORDER BY ${row.residence.time}) AS ${_COLUMN.prior}`
    + ` FROM ${row.residence.table[kind]} WHERE ${Array.join(_predicates(row, source), " AND ")}`
  return `SELECT ${_ENGINES[row.engine].bucket(row.residence.time, seconds)} AS ${_COLUMN.at}, ${row.residence.identity(keys)} AS ${_COLUMN.by}`
    + `${_keySelect(row, keys)}, ${_FNS[fn].sql(_increment(value, _COLUMN.prior), seconds, row.residence.time, _ENGINES[row.engine])} AS ${_COLUMN.value}`
    + ` FROM (${stepped}) reset ${_group(keys)}`
}

const _applied = (op: _Op, left: string, right: string, engine: _Engine): string =>
  _OPS[op].truth ? engine.truth(_OPS[op].sql(left, right)) : _OPS[op].sql(left, right)

const _scalar = (query: _Query, engine: _Engine): Option.Option<string> =>
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

const _constant = (value: string): string => `SELECT NULL AS ${_COLUMN.at}, '' AS ${_COLUMN.by}, ${value} AS ${_COLUMN.value}`

const _broadcast = (relation: string, value: string, keys: ReadonlyArray<_Query.Key>): string =>
  `SELECT ${_COLUMN.at}, ${_COLUMN.by}${_keyProject(keys)}, ${value} AS ${_COLUMN.value} FROM (${relation})`

const _joined = (op: _Op, engine: _Engine, left: string, right: string, keys: ReadonlyArray<_Query.Key>): string =>
  `SELECT l.${_COLUMN.at} AS ${_COLUMN.at}, l.${_COLUMN.by} AS ${_COLUMN.by}${_keyProject(keys, "l.")}, ${_applied(op, `l.${_COLUMN.value}`, `r.${_COLUMN.value}`, engine)}`
  + ` AS ${_COLUMN.value} FROM (${left}) l JOIN (${right}) r ON l.${_COLUMN.at} = r.${_COLUMN.at} AND l.${_COLUMN.by} = r.${_COLUMN.by}`

const _windowedRelation = (
  row: _Sql,
  keys: ReadonlyArray<_Query.Key>,
  inner: string,
  fn: _Fn,
  window: _Query.Window,
): string => {
  const engine = _ENGINES[row.engine]
  const seconds = _bucketed(window, row.resolution)
  const reset = fn === "increase" || fn === "rate"
  const relation = reset
    ? `SELECT *, lag(${_COLUMN.value}) OVER (PARTITION BY ${_COLUMN.by} ORDER BY ${_COLUMN.at}) AS ${_COLUMN.prior} FROM (${inner}) series`
    : inner
  const value = reset ? _increment(_COLUMN.value, _COLUMN.prior) : _COLUMN.value
  return `SELECT ${engine.bucket(_COLUMN.at, seconds)} AS ${_COLUMN.at}, ${_COLUMN.by}${_keyProject(keys)}, ${
    _FNS[fn].sql(value, seconds, _COLUMN.at, engine)
  } AS ${_COLUMN.value} FROM (${relation}) windowed ${_group(keys)}`
}

const _sql = (query: _Query, row: _Sql, keys: ReadonlyArray<_Query.Key> = []): string =>
  pipe(_ENGINES[row.engine], (engine) =>
    _Query.$match(query, {
      Aggregate: ({ by, of, op, without }) =>
        pipe(without === true ? Array.filter(_LABEL_KEYS, (key) => !Array.contains(by, key)) : by, (grouped) =>
          `SELECT ${_COLUMN.at}, ${_COLUMN.by}${_keyProject(grouped)}, ${engine.aggregate[_AGG[op].fold]}(${_COLUMN.value}) AS ${_COLUMN.value} FROM (${
            _sql(of, row, grouped)
          }) ${_group(grouped)}`),
      Binary: ({ left, op, right }) =>
        pipe([_scalar(left, engine), _scalar(right, engine)] as const, ([lhs, rhs]) =>
          Option.match(Option.zipWith(lhs, rhs, (l, r) => _applied(op, l, r, engine)), {
            onNone: () =>
              Option.match(rhs, {
                onNone: () =>
                  Option.match(lhs, {
                    onNone: () => _joined(op, engine, _sql(left, row, keys), _sql(right, row, keys), keys),
                    onSome: (scalar) => _broadcast(_sql(right, row, keys), _applied(op, scalar, _COLUMN.value, engine), keys),
                  }),
                onSome: (scalar) => _broadcast(_sql(left, row, keys), _applied(op, _COLUMN.value, scalar, engine), keys),
              }),
            onSome: _constant,
          })),
      Const: ({ value }) => _constant(`${value}`),
      Fraction: ({ labels, matchers, metric, upper, window }) => _leaf(row, keys, { labels, matchers, metric }, window, () => row.residence.fraction(upper)),
      Instant: (source) => _leaf(row, keys, source, row.resolution, (column, time) => engine.latest(column, time)),
      Quantile: ({ labels, matchers, metric, q, window }) => _leaf(row, keys, { labels, matchers, metric }, window, () => row.residence.quantile(q)),
      Rank: ({ count, of, op }) => engine.rank(_sql(of, row, keys), _RANK[op].order, count, _projection(keys)),
      Windowed: ({ fn, of, window }) =>
        of._tag === "Instant"
          ? fn === "increase" || fn === "rate"
            ? _resetLeaf(row, keys, of, window, fn)
            : _leaf(row, keys, of, window, (column, time) => _FNS[fn].sql(column, _bucketed(window, row.resolution), time, engine))
          : _windowedRelation(row, keys, _sql(of, row, keys), fn, window),
    }))

const _render = (query: _Query, target: _Query.Target): string =>
  _Target.$match(target, { Promql: (row) => _promql(query, row), Sql: (row) => _sql(query, row) })

const _Query: Data.TaggedEnum.Constructor<_Query> & {
  readonly breach: (sli: Reliability.Sli, window: _Query.Window, labels?: _Query.Labels, filters?: ReadonlyArray<Reliability.Filter>) => _Query
  readonly burn: (spec: Reliability.Alert.Spec, labels?: _Query.Labels) => _Query
  readonly finite: typeof _Finite.make
  readonly indicator: (sli: Reliability.Sli, window?: _Query.Window, labels?: _Query.Labels, filters?: ReadonlyArray<Reliability.Filter>) => _Query
  readonly interval: typeof _INTERVAL
  readonly promql: (
    row: { readonly histogram?: _Query.Histogram; readonly source: string; readonly translation?: Convention.Translation },
  ) => _Query.Target
  readonly quantile: typeof _Quantile.make
  readonly rankCount: typeof _RankCount.make
  readonly render: (query: _Query, target: _Query.Target) => string
  readonly span: typeof _QuerySpan.make
  readonly sql: (
    row: { readonly engine: _Query.Engine; readonly residence: _Query.Residence; readonly resolution?: _Query.Span; readonly source: string },
  ) => _Query.Target
} = {
  ..._QueryCases,
  breach: (sli, window, labels = {}, filters = []) => _breach(sli, window, labels, filters),
  burn: (spec, labels = {}) => _burned(spec, labels),
  finite: _Finite.make,
  indicator: (sli, window = _INTERVAL.rate, labels = {}, filters = []) => _indicator(sli, window, labels, filters),
  interval: _INTERVAL,
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

- Owner: `Panel` closes the panel union — timeseries, stat, gauge, heatmap, logs, table, geomap, nodes — each a tagged schema whose encoded form is the wire the deploy compiler binds.
- Law: every variant composes the shared `_PanelFields` spine, so a new variant declares only its own render payload.
- Growth: a panel kind is one tagged schema on the union; a shared affordance is one `_PanelFields` column every variant inherits.
- Boundary: compilation to a store's dashboard JSON is `iac/operate/observe`'s; this owner freezes the encoded shape.
- Packages: `effect` (`Schema`); `../value/schema.ts` (`Shape.Record`).

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
const _Transform = Schema.Union(
  Schema.TaggedStruct("Calculate", { alias: Schema.NonEmptyString, expression: Schema.NonEmptyString }),
  Schema.TaggedStruct("Filter", { field: Schema.NonEmptyString, op: Schema.Literal("equal", "greater", "less", "match", "notEqual"), value: Schema.Union(Schema.String, Schema.Number, Schema.Boolean) }),
  Schema.TaggedStruct("Group", { by: Schema.NonEmptyArray(Schema.NonEmptyString), reducers: Schema.NonEmptyArray(Schema.Literal("count", "first", "last", "max", "mean", "min", "sum")) }),
  Schema.TaggedStruct("Join", { how: Schema.Literal("inner", "left", "outer"), on: Schema.NonEmptyArray(Schema.NonEmptyString) }),
  Schema.TaggedStruct("Organize", { order: Schema.Array(Schema.NonEmptyString), rename: Shape.Record(Schema.NonEmptyString, Schema.NonEmptyString) }),
  Schema.TaggedStruct("Reduce", { fields: Schema.NonEmptyArray(Schema.NonEmptyString), reducer: Schema.Literal("count", "first", "last", "max", "mean", "min", "sum") }),
)
const _PanelFields = {
  description: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
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
  axes: Schema.optionalWith(Schema.Array(_Axis), { default: () => [] }),
  exprs: Schema.NonEmptyArray(Schema.String),
  legend: Schema.optionalWith(Schema.String, { as: "Option" }),
  steps: Schema.Array(_Threshold),
  tooltip: Schema.optionalWith(Schema.Literal("hidden", "multi", "single"), { default: () => "multi" as const }),
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
  color: Schema.optionalWith(Schema.Literal("opacity", "scheme"), { default: () => "scheme" as const }),
  expr: Schema.String,
  scale: Schema.optionalWith(Schema.Literal("exponential", "linear"), { default: () => "linear" as const }),
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
  zoom: Schema.optionalWith(Schema.Boolean, { default: () => true }),
})
const Nodes = Schema.TaggedStruct("Nodes", {
  ..._PanelFields,
  edges: Schema.String,
  mapping: Schema.Struct({
    edgeId: Schema.NonEmptyString, // the edges frame's own REQUIRED id column — an edge row without one never renders
    edgeSource: Schema.NonEmptyString,
    edgeTarget: Schema.NonEmptyString,
    nodeColor: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
    nodeId: Schema.NonEmptyString,
    nodeLabel: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
    nodeWeight: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  }),
  nodes: Schema.String,
  zoom: Schema.optionalWith(Schema.Boolean, { default: () => true }),
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

- Owner: `Board.DashboardModel` mints dashboard identity from the app identity and a page value, lays panels onto the 24-column grid as a pure fold, snapshots live Effect metrics as typed `Signal` values, and dispatches pack payloads.
- Law: `snapshot` admits only names the Convention census carries and pairs each with its declared kind, so a foreign series never reaches a board wire.
- Growth: a board affordance — annotation, variable — is one schema column on the model.
- Boundary: serving a snapshot and persisting a wire are data-plane concerns; this owner encodes and decodes the value.
- Packages: `effect` (`Match`, `Metric`, `MetricPair`, `MetricState`, `Schema`); `../value/identity.ts` (`Identity`).

```typescript signature
const _Uid = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]*$/), Schema.maxLength(40), Schema.brand("DashboardUid"))

const _Annotation = Schema.Struct({ slug: Schema.NonEmptyString, tone: Schema.NonEmptyString })
const _Variable = Schema.Struct({ label: Schema.NonEmptyString, name: Schema.NonEmptyString })

type LiveMetric = Data.TaggedEnum<{
  Counter: { readonly declared: Convention.InstrumentKind; readonly labels: Convention.Bag; readonly name: Convention.MetricName; readonly value: number | bigint }
  Frequency: { readonly declared: Convention.InstrumentKind; readonly labels: Convention.Bag; readonly name: Convention.MetricName; readonly values: ReadonlyMap<string, number> }
  Gauge: { readonly declared: Convention.InstrumentKind; readonly labels: Convention.Bag; readonly name: Convention.MetricName; readonly value: number | bigint }
  Histogram: { readonly buckets: ReadonlyArray<readonly [number, number]>; readonly count: number; readonly declared: Convention.InstrumentKind; readonly labels: Convention.Bag; readonly max: number; readonly min: number; readonly name: Convention.MetricName; readonly sum: number }
  Summary: { readonly count: number; readonly declared: Convention.InstrumentKind; readonly error: number; readonly labels: Convention.Bag; readonly max: number; readonly min: number; readonly name: Convention.MetricName; readonly quantiles: ReadonlyArray<readonly [number, Option.Option<number>]>; readonly sum: number }
  Unknown: { readonly declared: Convention.InstrumentKind; readonly labels: Convention.Bag; readonly name: Convention.MetricName }
}>
const _LiveMetric = Data.taggedEnum<LiveMetric>()
const _isMetricName = Convention.Metric.is
const _live = (pair: MetricPair.MetricPair.Untyped): Option.Option<LiveMetric> =>
  Option.map(Option.liftPredicate(pair.metricKey.name, _isMetricName), (name) => {
    const labels: Convention.Bag = Record.fromEntries(Array.map(pair.metricKey.tags, (tag) => [tag.key, tag.value] as const))
    const declared = Convention.Metric.at(name).kind
    return Match.value(pair.metricState).pipe(
      Match.when(MetricState.isCounterState, (state) => _LiveMetric.Counter({ declared, labels, name, value: state.count })),
      Match.when(MetricState.isFrequencyState, (state) => _LiveMetric.Frequency({ declared, labels, name, values: state.occurrences })),
      Match.when(MetricState.isGaugeState, (state) => _LiveMetric.Gauge({ declared, labels, name, value: state.value })),
      Match.when(MetricState.isHistogramState, (state) =>
        _LiveMetric.Histogram({ buckets: state.buckets, count: state.count, declared, labels, max: state.max, min: state.min, name, sum: state.sum })),
      Match.when(MetricState.isSummaryState, (state) =>
        _LiveMetric.Summary({ count: state.count, declared, error: state.error, labels, max: state.max, min: state.min, name, quantiles: state.quantiles, sum: state.sum })),
      Match.orElse(() => _LiveMetric.Unknown({ declared, labels, name })),
    )
  })

class _DashboardModel extends Schema.Class<_DashboardModel>("_DashboardModel")({
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
  static readonly of = ({ identity }: _DashboardModel.Board, page: _DashboardModel.Page): _DashboardModel =>
    new _DashboardModel({
      annotations: page.annotations,
      identity: Convention.identity(identity),
      panels: page.panels,
      tags: [identity.app, ...page.tags],
      title: `${identity.app} ${page.title}`,
      uid: _Uid.make(`${identity.app}-${page.slug}`),
      variables: [{ label: "Tenant", name: "tenant" }, ...page.variables],
    })
  static readonly laid = (model: _DashboardModel): ReadonlyArray<_DashboardModel.Placed> =>
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
  static readonly pack = <K extends _DashboardModel.Pack>(
    kind: K,
    board: _DashboardModel.Board,
    payload: _DashboardModel.Payload[K],
  ): _DashboardModel => _PACKS[kind](board, payload)
  static readonly suite = (board: _DashboardModel.Board, payload: _DashboardModel.Suite): ReadonlyArray<_DashboardModel> =>
    Array.map(Struct.keys(_SUITE), (kind) => _SUITE[kind](board, payload))
}

declare namespace _DashboardModel {
  type Board = {
    readonly analytics: Option.Option<_Query.Target> // the columnar residence, absent where the stack installs none
    readonly identity: Identity.App
    readonly logs: string
    readonly target: _Query.Target // the metrics plane every health tile reads
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
  type Wire = typeof _DashboardModel.Encoded
  type Signal = LiveMetric
}
```

## [05]-[BENCH]

- Owner: `Board.Bench` decodes mitata measurements into `Claim` values and grades a candidate against a baseline through the ordered admission ladder — suite, host, duplicates, metric roster, sample floor, rung positivity — before any ratio computes.
- Law: refusals carry both sides' projection on the failed axis, so a refused grade is diagnosable evidence rather than a boolean.
- Law: polarity owns the ratio direction, so a maximize metric grades improved on growth without any consumer inverting a comparison.
- Growth: a hardware-counter leaf is one `_COUNTER_PATHS` row joining the series estate-wide; an admission axis is one `_ADMISSION` row; a grading knob is one `Tolerance` field.
- Boundary: benchmark execution and suite selection are the runtime bench owner's; this owner ingests and grades landed claims.
- Packages: `mitata` (`measure` stats shape); `../value/contentKey.ts` (`Digest`); `../value/schema.ts` (`Shape.Record`).

```typescript signature
// Rungs: the branch record keys a band by rung NAME and the wire carries `RungCell{rung: BenchRung, value}` rows, so
// ONE correspondence declares the name→member pair and both the roster and the inverse read derive from it — a rung
// the corpus adds lands as one row here and breaks every consumer that enumerates the record.
type _Defined<E extends { readonly UNSPECIFIED: 0 }> = Exclude<E[keyof E], E["UNSPECIFIED"]>
const _RUNG_WIRE = {
  min: evidence.BenchRung.MIN,
  max: evidence.BenchRung.MAX,
  avg: evidence.BenchRung.AVG,
  p25: evidence.BenchRung.P25,
  p50: evidence.BenchRung.P50,
  p75: evidence.BenchRung.P75,
  p95: evidence.BenchRung.P95,
  p99: evidence.BenchRung.P99,
  p999: evidence.BenchRung.P999,
  stdDev: evidence.BenchRung.STD_DEV,
} as const
const _RUNGS = Record.keys(_RUNG_WIRE)
type _RungsClosed<K extends _Defined<typeof evidence.BenchRung> = (typeof _RUNG_WIRE)[keyof typeof _RUNG_WIRE]> = K
type _RungsWhole<K extends (typeof _RUNG_WIRE)[keyof typeof _RUNG_WIRE] = _Defined<typeof evidence.BenchRung>> = K
const _RUNG_NAME = Record.fromEntries(
  Array.map(Record.toEntries(_RUNG_WIRE), ([name, member]) => [member, name] as const),
) as { readonly [M in _Defined<typeof evidence.BenchRung>]: Extract<keyof typeof _RUNG_WIRE, string> }
const _MITATA_RUNGS = ["min", "max", "avg", "p25", "p50", "p75", "p99", "p999"] as const
const _GRADES = ["improved", "steady", "regressed"] as const
const _Rung = Shape.vocabulary(_RUNGS, {
  min: {}, max: {}, avg: {}, p25: {}, p50: {}, p75: {}, p95: {}, p99: {}, p999: {}, stdDev: {},
})
const _MitataRung = Shape.vocabulary(_MITATA_RUNGS, {
  min: {}, max: {}, avg: {}, p25: {}, p50: {}, p75: {}, p99: {}, p999: {},
})
const _BandValue = Schema.Number.pipe(Schema.finite(), Schema.nonNegative())
const _isBandValue = Schema.is(_BandValue)
type _Slack = typeof _Slack.Type
const _Slack = Schema.Number.pipe(Schema.finite(), Schema.nonNegative(), Schema.lessThan(1), Schema.brand("BenchSlack"))
type _MinSamples = typeof _MinSamples.Type
const _MinSamples = Schema.Int.pipe(Schema.positive(), Schema.brand("BenchMinSamples"))
const _Grade = Shape.vocabulary(_GRADES, {
  improved: { accepts: (ratio: number, slack: number) => ratio < 1 - slack },
  steady: { accepts: (ratio: number, slack: number) => ratio >= 1 - slack && ratio <= 1 + slack },
  regressed: { accepts: (ratio: number, slack: number) => ratio > 1 + slack },
})
// Polarity is the branch's grading vocabulary and the wire's `BenchPolarity` enum is its spelling: ONE correspondence
// declares the pair and the inverse read derives from it, exactly as the rungs do. Modality and the payload band ARE
// corpus enums with no branch algebra over them, so their schemas take the generated members directly; mitata spells
// its own modality words, so one correspondence carries them onto the wire's members.
const _Polarity = Shape.vocabulary(["minimize", "maximize"] as const, {
  minimize: { ratio: (fresh: number, base: number) => fresh / base },
  maximize: { ratio: (fresh: number, base: number) => base / fresh },
})
const _POLARITY_WIRE = { minimize: evidence.BenchPolarity.MINIMIZE, maximize: evidence.BenchPolarity.MAXIMIZE } as const
type _PolarityClosed<K extends _Defined<typeof evidence.BenchPolarity> = (typeof _POLARITY_WIRE)[keyof typeof _POLARITY_WIRE]> = K
const _POLARITY_NAME = {
  [evidence.BenchPolarity.MINIMIZE]: "minimize",
  [evidence.BenchPolarity.MAXIMIZE]: "maximize",
} as const satisfies Record<_Defined<typeof evidence.BenchPolarity>, keyof typeof _POLARITY_WIRE>
const _polarityOf = Schema.is(Schema.Literal(evidence.BenchPolarity.MINIMIZE, evidence.BenchPolarity.MAXIMIZE))
const _modalities = [evidence.BenchModality.FN, evidence.BenchModality.ITER, evidence.BenchModality.YIELD] as const
const _MODALITY = { fn: evidence.BenchModality.FN, iter: evidence.BenchModality.ITER, yield: evidence.BenchModality.YIELD } as const
type _ModalityClosed<K extends _Defined<typeof evidence.BenchModality> = (typeof _modalities)[number]> = K
const _bands = [evidence.PayloadBand.MICRO, evidence.PayloadBand.SMALL, evidence.PayloadBand.MEDIUM, evidence.PayloadBand.LARGE] as const
type _BandClosed<K extends _Defined<typeof evidence.PayloadBand> = (typeof _bands)[number]> = K
type _MitataStats = Awaited<ReturnType<typeof MitataMeasure>>

const _BenchAggregate = Schema.Struct({ avg: _BandValue, min: _BandValue, max: _BandValue, total: _BandValue })
const _BenchCounterValue = Schema.Number.pipe(Schema.finite())
const _BenchCounters = Shape.Record(Schema.NonEmptyString, _BenchCounterValue)
// `partialWith` rebuilds the record AST and drops a node annotation, so the closed-key posture cannot ride
// `Shape.Record` here; it seats on the outermost node instead, which is the one whose excess-property check runs.
const _BenchRungs = Schema.Record({ key: _Rung.schema, value: _BandValue }).pipe(
  Schema.partialWith({ exact: true }),
  Schema.filter((rungs) => Array.some(_RUNGS, (rung) => rungs[rung] !== undefined) || "<rungless-band>", { identifier: "MeasuredRungs" }),
).annotations({ parseOptions: { onExcessProperty: "error" } })
const _BenchBand = Schema.Struct({
  sampleCount: Schema.Int.pipe(Schema.positive()),
  rungs: _BenchRungs,
  ticks: Schema.optionalWith(Schema.Int.pipe(Schema.nonNegative()), { as: "Option" }),
  samples: Schema.optionalWith(Schema.Array(_BandValue), { as: "Option" }),
  gc: Schema.optionalWith(_BenchAggregate, { as: "Option" }),
  heap: Schema.optionalWith(_BenchAggregate, { as: "Option" }),
  counters: Schema.optionalWith(_BenchCounters, { as: "Option" }),
})
// The input carries what the producer declares — rank IS `shape.length` and contiguity IS the stride order — so the
// two derived columns the prior mirror stored are read off the shape they were derived from.
const _BenchInput = Schema.Struct({
  payloadBytes: Schema.BigIntFromSelf,
  band: Schema.Literal(..._bands),
  dtype: Schema.NonEmptyString,
  shape: Schema.Array(Schema.BigIntFromSelf),
  strides: Schema.Array(Schema.BigIntFromSelf),
  batch: Schema.Int.pipe(Schema.positive()),
  density: Schema.Number.pipe(Schema.between(0, 1)),
})
const _ProfileArtifact = Schema.Union(
  Schema.TaggedStruct("chrome-trace", { content: Digest.codecs.content.bytes, startNs: Schema.BigIntFromSelf }),
  Schema.TaggedStruct("benchmark-export", { content: Digest.codecs.content.bytes, exporter: Schema.NonEmptyString }),
  Schema.TaggedStruct("ep-context", { content: Digest.codecs.content.bytes, ep: Schema.NonEmptyString }),
)
const _BenchSubject = Schema.Union(
  Schema.Struct({ subject: Schema.Literal("probe") }),
  Schema.Struct({
    subject: Schema.Literal("kernel"),
    input: _BenchInput,
    substrate: Schema.NonEmptyString,
    family: Schema.NonEmptyString,
    case: Schema.NonEmptyString,
    route: Schema.NonEmptyString,
    provider: Schema.NonEmptyString,
    corpus: Schema.optionalWith(Digest.codecs.content.bytes, { as: "Option" }),
    artifactKey: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
    equivalenceMaxDeviation: _BandValue,
    toleranceClass: Schema.NonEmptyString,
    artifacts: Schema.Array(_ProfileArtifact),
  }),
)
const _BenchMetric = Schema.Struct({
  label: Schema.NonEmptyString,
  unit: Schema.NonEmptyString,
  modality: Schema.Literal(..._modalities),
  polarity: _Polarity.schema,
  subject: _BenchSubject,
  band: _BenchBand,
  warmups: Schema.optionalWith(Schema.Int.pipe(Schema.nonNegative()), { as: "Option" }),
  allocatedBytes: Schema.optionalWith(Schema.BigIntFromSelf, { as: "Option" }),
  operations: Schema.optionalWith(Schema.BigIntFromSelf, { as: "Option" }),
})

// The host lands off the generated `HostFingerprintWire`: every scalar rule is protovalidate's at the frame, and what
// this owner adds is the one law no field rule states — `stamps` cross as label pairs and land as the closed-key
// record a print is compared against, so a duplicated label refuses rather than last-wins.
class _BenchHost extends Schema.Class<_BenchHost>("HostFingerprint")({
  print: Schema.String,
  machine: Schema.String,
  os: Schema.String,
  arch: Schema.String,
  processors: Schema.Int,
  runtime: Schema.String,
  stamps: Shape.Record(Schema.NonEmptyString, Schema.String),
}) {}

const _pairs = (stamps: ReadonlyArray<fingerprint.LabelPair>, ast: SchemaAST.AST): Either.Either<Readonly<Record<string, string>>, ParseResult.ParseIssue> =>
  Array.dedupe(Array.map(stamps, (pair) => pair.key)).length === stamps.length
    ? Either.right(Record.fromEntries(Array.map(stamps, (pair) => [pair.key, pair.value] as const)))
    : Either.left(new ParseResult.Type(ast, stamps, "<duplicate-stamp-label>"))

const _hostOf = (wire: fingerprint.HostFingerprintWire, ast: SchemaAST.AST): Either.Either<_BenchHost, ParseResult.ParseIssue> =>
  Either.flatMap(_pairs(wire.stamps, ast), (stamps) =>
    Either.mapLeft(
      Schema.decodeUnknownEither(_BenchHost)({
        print: wire.print, machine: wire.machine, os: wire.os, arch: wire.arch, processors: wire.processors, runtime: wire.runtime, stamps,
      }),
      (error) => error.issue,
    ))

const _hostWire = (host: _BenchHost): fingerprint.HostFingerprintWire =>
  create(fingerprint.HostFingerprintWireSchema, {
    print: host.print, machine: host.machine, os: host.os, arch: host.arch, processors: host.processors, runtime: host.runtime,
    stamps: Array.map(Record.toEntries(host.stamps), ([key, value]) => create(fingerprint.LabelPairSchema, { key, value })),
  })

class _Claim extends Schema.Class<_Claim>("Claim")({
  suite: Schema.NonEmptyString,
  metrics: Schema.NonEmptyArray(_BenchMetric),
  host: _BenchHost,
  minted: Schema.DateTimeUtc,
}) {
  static readonly RUNGS: typeof _RUNGS = _RUNGS
  static readonly Band: typeof _BenchBand = _BenchBand
  static readonly Subject: typeof _BenchSubject = _BenchSubject
  static readonly Host: typeof _BenchHost = _BenchHost
  static readonly FromWire: Schema.Schema<_Claim, MessageShape<typeof evidence.BenchmarkClaimWireSchema>> = Schema.suspend(() => _ClaimFromWire)
  static readonly matches = (claim: _Claim, identity: Identity.App): boolean => claim.host.print === identity.host
}

// --- [CLAIM_WIRE]

// The crossing is total both ways over the generated `BenchmarkClaimWire`: the descriptor proves every column and
// protovalidate every scalar rule at the frame, so this transform carries only the lifts no rule states — rung
// cells onto the named record, `Empty`/kernel onto the subject union, artifact oneofs onto tagged structs, sixteen
// byte keys onto the content brand, optional magnitudes onto the branch carrier, and the instant off the well-known
// stamp. Decode projects the wire onto the class's ENCODED face and runs the class once, so no domain rule is
// restated here; encode rebuilds the message through `create` and never a hand object.
const _Wire: Schema.Schema<MessageShape<typeof evidence.BenchmarkClaimWireSchema>> = Schema.declare(
  (input: unknown): input is MessageShape<typeof evidence.BenchmarkClaimWireSchema> =>
    isMessage(input, evidence.BenchmarkClaimWireSchema),
  { identifier: evidence.BenchmarkClaimWireSchema.typeName },
)
const _rungOf = Schema.is(Schema.Literal(..._RUNGS.map((name) => _RUNG_WIRE[name])))
const _bandOf = Schema.is(Schema.Literal(..._bands))
const _present = <A>(value: A | undefined): A | undefined => value
const _nonEmpty = <A>(values: ReadonlyArray<A>): ReadonlyArray<A> | undefined => (values.length === 0 ? undefined : values)

const _artifactOf = (wire: evidence.ProfileArtifactWire, ast: SchemaAST.AST): Either.Either<typeof _ProfileArtifact.Encoded, ParseResult.ParseIssue> =>
  Match.value(wire.kind).pipe(
    Match.when({ case: "chromeTrace" }, ({ value }) => Either.right({ _tag: "chrome-trace" as const, content: value.content, startNs: value.startNs })),
    Match.when({ case: "benchmarkExport" }, ({ value }) => Either.right({ _tag: "benchmark-export" as const, content: value.content, exporter: value.exporter })),
    Match.when({ case: "epContext" }, ({ value }) => Either.right({ _tag: "ep-context" as const, content: value.content, ep: value.ep })),
    Match.orElse(() => Either.left(new ParseResult.Type(ast, wire, "<artifact-unset>"))),
  )

const _subjectOf = (wire: evidence.BenchMetric, ast: SchemaAST.AST): Either.Either<typeof _BenchSubject.Encoded, ParseResult.ParseIssue> =>
  Match.value(wire.subject).pipe(
    Match.when({ case: "probe" }, () => Either.right({ subject: "probe" as const })),
    Match.when({ case: "kernel" }, ({ value }) =>
      Either.flatMap(
        Option.match(Option.fromNullable(value.input), {
          onNone: () => Either.left(new ParseResult.Type(ast, value, "<input-unset>")),
          onSome: (input) =>
            _bandOf(input.band)
              ? Either.right({ payloadBytes: input.payloadBytes, band: input.band, dtype: input.dtype, shape: input.shape, strides: input.strides, batch: input.batch, density: input.density })
              : Either.left(new ParseResult.Type(ast, input, "<band-undefined>")),
        }),
        (input) =>
          Either.map(Either.all(Array.map(value.artifacts, (artifact) => _artifactOf(artifact, ast))), (artifacts) => ({
            subject: "kernel" as const,
            input,
            substrate: value.substrate,
            family: value.family,
            case: value.case,
            route: value.route,
            provider: value.provider,
            corpus: _present(value.corpus),
            artifactKey: _present(value.artifactKey),
            equivalenceMaxDeviation: value.equivalenceMaxDeviation,
            toleranceClass: value.toleranceClass,
            artifacts,
          })),
      )),
    Match.orElse(() => Either.left(new ParseResult.Type(ast, wire, "<subject-unset>"))),
  )

const _bandWireOf = (band: evidence.BenchBandWire, ast: SchemaAST.AST): Either.Either<typeof _BenchBand.Encoded, ParseResult.ParseIssue> =>
  Either.map(
    Either.all(Array.map(band.rungs, (cell) =>
      _rungOf(cell.rung)
        ? Either.right([_RUNG_NAME[cell.rung], cell.value] as const)
        : Either.left(new ParseResult.Type(ast, cell, "<rung-undefined>")))),
    (cells) => ({
      sampleCount: band.sampleCount,
      rungs: Record.fromEntries(cells),
      ticks: _present(band.ticks),
      samples: _nonEmpty(band.samples),
      gc: _present(band.gc),
      heap: _present(band.heap),
      counters: Struct.keys(band.counters).length === 0 ? undefined : band.counters,
    }),
  )

const _metricOf = (wire: evidence.BenchMetric, ast: SchemaAST.AST): Either.Either<typeof _BenchMetric.Encoded, ParseResult.ParseIssue> =>
  Either.map(
    Either.all({
      polarity: _polarityOf(wire.polarity) ? Either.right(wire.polarity) : Either.left(new ParseResult.Type(ast, wire, "<polarity-undefined>")),
      subject: _subjectOf(wire, ast),
      band: Option.match(Option.fromNullable(wire.band), {
        onNone: () => Either.left(new ParseResult.Type(ast, wire, "<band-unset>")),
        onSome: (band) => _bandWireOf(band, ast),
      }),
    }),
    ({ polarity, subject, band }) => ({
      label: wire.label,
      unit: wire.unit,
      modality: wire.modality,
      polarity: _POLARITY_NAME[polarity],
      subject,
      band,
      warmups: _present(wire.warmups),
      allocatedBytes: _present(wire.allocatedBytes),
      operations: _present(wire.operations),
    }),
  )

const _artifactWire = (artifact: typeof _ProfileArtifact.Encoded): evidence.ProfileArtifactWire =>
  create(evidence.ProfileArtifactWireSchema, {
    kind: Match.valueTags(artifact, {
      "chrome-trace": ({ content, startNs }) => ({ case: "chromeTrace" as const, value: create(evidence.ChromeTraceWireSchema, { content, startNs }) }),
      "benchmark-export": ({ content, exporter }) => ({ case: "benchmarkExport" as const, value: create(evidence.BenchmarkExportWireSchema, { content, exporter }) }),
      "ep-context": ({ content, ep }) => ({ case: "epContext" as const, value: create(evidence.EpContextWireSchema, { content, ep }) }),
    }),
  })

const _metricWire = (metric: typeof _BenchMetric.Encoded): evidence.BenchMetric =>
  create(evidence.BenchMetricSchema, {
    label: metric.label,
    unit: metric.unit,
    modality: metric.modality,
    polarity: _POLARITY_WIRE[metric.polarity],
    subject: metric.subject.subject === "probe"
      ? { case: "probe", value: create(EmptySchema) }
      : {
        case: "kernel",
        value: create(evidence.BenchKernelWireSchema, {
          input: create(evidence.BenchInputWireSchema, { ...metric.subject.input, shape: [...metric.subject.input.shape], strides: [...metric.subject.input.strides] }),
          substrate: metric.subject.substrate,
          family: metric.subject.family,
          case: metric.subject.case,
          route: metric.subject.route,
          provider: metric.subject.provider,
          corpus: metric.subject.corpus,
          artifactKey: metric.subject.artifactKey,
          equivalenceMaxDeviation: metric.subject.equivalenceMaxDeviation,
          toleranceClass: metric.subject.toleranceClass,
          artifacts: Array.map(metric.subject.artifacts, _artifactWire),
        }),
      },
    band: create(evidence.BenchBandWireSchema, {
      sampleCount: metric.band.sampleCount,
      rungs: Array.filterMap(_RUNGS, (name) =>
        Option.map(Option.fromNullable(metric.band.rungs[name]), (value) => create(evidence.RungCellSchema, { rung: _RUNG_WIRE[name], value }))),
      ticks: metric.band.ticks,
      samples: [...(metric.band.samples ?? [])],
      gc: metric.band.gc,
      heap: metric.band.heap,
      counters: { ...(metric.band.counters ?? {}) },
    }),
    warmups: metric.warmups,
    allocatedBytes: metric.allocatedBytes,
    operations: metric.operations,
  })

const _ClaimFromWire: Schema.Schema<_Claim, MessageShape<typeof evidence.BenchmarkClaimWireSchema>> = Schema.transformOrFail(
  _Wire,
  _Claim,
  {
    strict: true,
    decode: (wire, _options, ast) =>
      Either.flatMap(
        Either.all({
          host: Option.match(Option.fromNullable(wire.host), {
            onNone: () => Either.left(new ParseResult.Type(ast, wire, "<host-unset>")),
            onSome: (host) => _hostOf(host, ast),
          }),
          minted: Option.match(Option.fromNullable(wire.minted), {
            onNone: () => Either.left(new ParseResult.Type(ast, wire, "<minted-unset>")),
            onSome: (stamp) => Either.right(DateTime.formatIso(DateTime.unsafeMake(timestampMs(stamp)))),
          }),
          metrics: Either.all(Array.map(wire.metrics, (metric) => _metricOf(metric, ast))),
        }),
        ({ host, minted, metrics }) =>
          Either.mapLeft(
            Schema.decodeUnknownEither(_Claim)({ suite: wire.suite, host: Schema.encodeSync(_BenchHost)(host), minted, metrics }),
            (error) => error.issue,
          ),
      ),
    encode: (claim) =>
      Either.map(
        Either.mapLeft(Schema.encodeEither(Schema.NonEmptyArray(_BenchMetric))(claim.metrics), (error) => error.issue),
        (metrics) =>
          create(evidence.BenchmarkClaimWireSchema, {
            suite: claim.suite,
            host: _hostWire(claim.host),
            minted: timestampFromMs(DateTime.toEpochMillis(claim.minted)),
            metrics: Array.map(metrics, _metricWire),
          }),
      ),
  },
)

// `_COUNTER_PATHS` carries BOTH platform planes — linux perf events and darwin kperf publish different leaves, with
// only `cycles` and `instructions` shared — and the filterMap keeps exactly what the measuring host answered; per-leaf
// presence is the host's own fact, and a leaf added here joins the series estate-wide.
const _COUNTER_PATHS = {
  cycles: ["cycles", "avg"],
  cyclesStalls: ["cycles", "stalls", "avg"],
  instructions: ["instructions", "avg"],
  instructionsLoadsStores: ["instructions", "loads_and_stores", "avg"],
  l1MissLoads: ["l1", "miss_loads", "avg"],
  l1MissStores: ["l1", "miss_stores", "avg"],
  cache: ["cache", "avg"],
  cacheMisses: ["cache", "misses", "avg"],
  branchMisses: ["_bmispred", "avg"],
} as const

const _nested = (input: unknown, path: ReadonlyArray<string>): Option.Option<number> =>
  pipe(
    Array.reduce(path, Option.some(input), (held, key) =>
      Option.flatMap(held, (value) => Predicate.hasProperty(value, key) ? Option.some(value[key]) : Option.none())),
    Option.flatMap(Schema.decodeUnknownOption(_BenchCounterValue)),
  )

const _mitataCounters = (stats: _MitataStats): Option.Option<typeof _BenchCounters.Type> => {
  const source = Predicate.hasProperty(stats, "counters") ? stats.counters : undefined
  const counters = Record.filterMap(_COUNTER_PATHS, (path) => _nested(source, path))
  return Struct.keys(counters).length === 0 ? Option.none() : Option.some(counters)
}

const _fromMitata = (stats: _MitataStats, mint: _Bench.Mint): _Claim =>
  new _Claim({
    suite: mint.suite,
    host: mint.host,
    minted: mint.minted,
    metrics: [{
      label: mint.label,
      unit: mint.unit,
      modality: _MODALITY[stats.kind],
      polarity: mint.polarity,
      subject: mint.subject,
      band: {
        sampleCount: stats.samples.length,
        rungs: {
          min: stats.min,
          max: stats.max,
          avg: stats.avg,
          p25: stats.p25,
          p50: stats.p50,
          p75: stats.p75,
          p99: stats.p99,
          p999: stats.p999,
        },
        ticks: Option.some(stats.ticks),
        samples: Option.some(stats.samples),
        // mitata seeds an empty aggregate with ±Infinity/NaN (zero qualifying samples leaves heap `_ === 0`), so
        // finite decode drops such a band to absence rather than admitting a fabricated point into a finite slot
        gc: Option.flatMap(Option.fromNullable(stats.gc), Schema.decodeUnknownOption(_BenchAggregate)),
        heap: Option.flatMap(Option.fromNullable(stats.heap), Schema.decodeUnknownOption(_BenchAggregate)),
        counters: _mitataCounters(stats),
      },
      warmups: mint.warmups,
      allocatedBytes: mint.allocatedBytes,
      operations: mint.operations,
    }],
  })

declare namespace _Bench {
  type Rung = (typeof _RUNGS)[number]
  type MitataRung = (typeof _MITATA_RUNGS)[number]
  type CounterLeaf = keyof typeof _COUNTER_PATHS
  type Band = typeof _BenchBand.Type
  type Metric = typeof _BenchMetric.Type
  type Polarity = (typeof _Polarity.kinds)[number]
  type Mint = {
    readonly suite: string
    readonly label: string
    readonly unit: string
    readonly polarity: Polarity
    readonly host: _BenchHost
    readonly subject: typeof _BenchSubject.Type
    readonly minted: typeof Schema.DateTimeUtc.Type
    readonly warmups: Option.Option<number>
    readonly allocatedBytes: Option.Option<bigint>
    readonly operations: Option.Option<bigint>
  }
  type Grade = (typeof _GRADES)[number]
  type Row = {
    readonly modality: Metric["modality"]
    readonly label: string
    readonly unit: string
    readonly polarity: Polarity
    readonly grade: Grade
    readonly ratio: number
  }
  type RefusalAxis = "suite" | "host" | "duplicates" | "metrics" | "samples" | "rung"
  type Verdict = Data.TaggedEnum<{
    Graded: { readonly suite: string; readonly print: string; readonly rows: ReadonlyArray<Row> }
    Refused: { readonly suite: string; readonly axis: RefusalAxis; readonly baseline: string; readonly candidate: string }
  }>
  type Tolerance = { readonly rung: Rung; readonly slack: _Slack; readonly minSamples: _MinSamples }
}
const _Verdict = Data.taggedEnum<_Bench.Verdict>()

const _TOLERANCE: _Bench.Tolerance = { rung: "p99", slack: _Slack.make(0.05), minSamples: _MinSamples.make(30) }

const _measured = (metric: _Bench.Metric, rung: _Bench.Rung): Option.Option<number> =>
  Option.filter(Option.fromNullable(metric.band.rungs[rung]), _isBandValue)

const _metricIdentity = ({ label, modality, polarity, unit }: _Bench.Metric): string =>
  `${modality}\u0000${label}\u0000${unit}\u0000${polarity}`
const _metricOrder: Order.Order<_Bench.Metric> = Order.mapInput(Order.string, _metricIdentity)
const _sameMetric = (left: _Bench.Metric, right: _Bench.Metric): boolean =>
  _metricIdentity(left) === _metricIdentity(right)

type _PreparedClaim = {
  readonly claim: _Claim
  readonly duplicate: Option.Option<string>
  readonly metrics: ReadonlyArray<_Bench.Metric>
}

const _prepared = (claim: _Claim): _PreparedClaim => {
  const metrics = Array.sort(claim.metrics, _metricOrder)
  const duplicate = pipe(
    Array.findFirst(Array.zip(metrics, Array.drop(metrics, 1)), ([left, right]) => _sameMetric(left, right)),
    Option.map(([metric]) => _metricIdentity(metric)),
  )
  return { claim, duplicate, metrics }
}

const _roster = (prepared: _PreparedClaim): string =>
  Array.join(Array.map(prepared.metrics, _metricIdentity), ",")

const _rungValues = (rung: _Bench.Rung) => (prepared: _PreparedClaim): string =>
  Array.join(
    Array.map(prepared.metrics, (metric) =>
      `${_metricIdentity(metric)}:${rung}=${Option.getOrElse(_measured(metric, rung), () => "unmeasured")}`),
    ",",
  )

const _sampleCounts = (prepared: _PreparedClaim): string =>
  Array.join(Array.map(prepared.metrics, (metric) => `${_metricIdentity(metric)}=${metric.band.sampleCount}`), ",")

const _aligned = (baseline: _PreparedClaim, candidate: _PreparedClaim): boolean =>
  baseline.metrics.length === candidate.metrics.length
  && Array.every(Array.zip(baseline.metrics, candidate.metrics), ([left, right]) => _sameMetric(left, right))

type _Admission = {
  readonly accepts: (baseline: _PreparedClaim, candidate: _PreparedClaim, tolerance: _Bench.Tolerance) => boolean
  readonly axis: _Bench.RefusalAxis
  readonly project: (claim: _PreparedClaim, tolerance: _Bench.Tolerance) => string
}

const _ADMISSION: ReadonlyArray<_Admission> = [
  {
    axis: "suite",
    accepts: (baseline, candidate) => baseline.claim.suite === candidate.claim.suite,
    project: ({ claim }) => claim.suite,
  },
  {
    axis: "host",
    accepts: (baseline, candidate) => baseline.claim.host.print === candidate.claim.host.print,
    project: ({ claim }) => claim.host.print,
  },
  {
    axis: "duplicates",
    accepts: (baseline, candidate) => Option.isNone(baseline.duplicate) && Option.isNone(candidate.duplicate),
    project: ({ duplicate }) => Option.getOrElse(duplicate, () => "none"),
  },
  { axis: "metrics", accepts: _aligned, project: _roster },
  {
    axis: "samples",
    accepts: (baseline, candidate, tolerance) =>
      Array.every([baseline, candidate], (claim) => Array.every(claim.metrics, (metric) => metric.band.sampleCount >= tolerance.minSamples)),
    project: _sampleCounts,
  },
  {
    axis: "rung",
    accepts: (baseline, candidate, tolerance) =>
      Array.every([baseline, candidate], (claim) =>
        Array.every(claim.metrics, (metric) => Option.match(_measured(metric, tolerance.rung), { onNone: () => false, onSome: (value) => value > 0 }))),
    project: (claim, tolerance) => _rungValues(tolerance.rung)(claim),
  },
]

const _graded = (baseline: _Claim, candidate: _Claim, tolerance: _Bench.Tolerance = _TOLERANCE): _Bench.Verdict => {
  const pair = { baseline: _prepared(baseline), candidate: _prepared(candidate) }
  return pipe(
    Array.findFirst(_ADMISSION, (row) => !row.accepts(pair.baseline, pair.candidate, tolerance)),
    Option.match({
      onSome: (row) => _Verdict.Refused({
        suite: candidate.suite,
        axis: row.axis,
        baseline: row.project(pair.baseline, tolerance),
        candidate: row.project(pair.candidate, tolerance),
      }),
      onNone: () => _Verdict.Graded({
        suite: candidate.suite,
        print: candidate.host.print,
        rows: Array.filterMap(Array.zip(pair.baseline.metrics, pair.candidate.metrics), ([held, metric]) =>
          Option.zipWith(_measured(metric, tolerance.rung), _measured(held, tolerance.rung), (fresh, base) => {
            const ratio = _Polarity.at(metric.polarity).ratio(fresh, base)
            return {
              modality: metric.modality,
              label: metric.label,
              unit: metric.unit,
              polarity: metric.polarity,
              ratio,
              grade: Array.findFirst(_Grade.kinds, (grade) => _Grade.at(grade).accepts(ratio, tolerance.slack)).pipe(Option.getOrThrow),
            }
          })),
      }),
    }),
  )
}

const _Bench: Data.TaggedEnum.Constructor<_Bench.Verdict> & {
  readonly Rung: typeof _Rung
  readonly MitataRung: typeof _MitataRung
  readonly Grade: typeof _Grade
  readonly Polarity: typeof _Polarity
  readonly fromMitata: (stats: _MitataStats, mint: _Bench.Mint) => _Claim
  readonly graded: (baseline: _Claim, candidate: _Claim, tolerance?: _Bench.Tolerance) => _Bench.Verdict
  readonly measured: (metric: _Bench.Metric, rung: _Bench.Rung) => Option.Option<number>
  readonly minSamples: typeof _MinSamples.make
  readonly slack: typeof _Slack.make
  // `counterLeaves` closes the leaf axis behind `benchCounterKind`: consumers type against the vocabulary the counter-path table declares
  readonly counterLeaves: ReadonlyArray<_Bench.CounterLeaf>
} = {
  ..._Verdict,
  Rung: _Rung,
  MitataRung: _MitataRung,
  Grade: _Grade,
  Polarity: _Polarity,
  counterLeaves: Struct.keys(_COUNTER_PATHS),
  fromMitata: _fromMitata,
  graded: _graded,
  measured: _measured,
  minSamples: _MinSamples.make,
  slack: _Slack.make,
}
```

## [06]-[PACKS]

- Owner: `Board.DashboardModel.pack` builds one standing dashboard per instrument family from board targets alone, and `suite` folds every pack over one payload so an app mounts its whole read surface in one call.
- Law: pack panels compose the shared pane tables — `_TRENDS`, `_FACETS`, `_LEVELS`, `_FLOWS` — so a family's read lands as one row in its pane table rather than a bespoke panel body.
- Growth: a pack is one payload row, one `_PACKS` arm, and one `_SUITE` row; a new read in an existing pack is one pane-table row.
- Boundary: realization to a running store is the deploy plane's; every pack admits only mounted Convention instruments.
- Packages: `effect` (`Array`, `Option`, `Record`, `Struct`); `./slo.ts` (`Reliability`).

```typescript signature
const _tenant = { [Convention.rasm.tenant]: "$tenant" } as const

const _WINDOW = _Query.span(Duration.minutes(5))
const _DAY = _Query.span(Duration.hours(24))

const _rated = (
  metric: Convention.MetricName<"counter">,
  labels: _Query.Labels,
  window: _Query.Window,
  matchers: ReadonlyArray<_Query.Matcher> = [],
): _Query => _Query.Aggregate({ by: [], of: _Query.Windowed({ fn: "rate", of: _Query.Instant({ labels, matchers, metric }), window }), op: "sum" })

const _alternate = (values: ReadonlyArray<string>): string => Array.join(Array.map(values, Regex.escape), "|")

const _complement = (of: _Query): _Query => _Query.Binary({ left: _Query.Const({ value: _Query.finite(1) }), op: "sub", right: of })

const _goodShare = (
  sli: Extract<Reliability.Sli, { readonly _tag: "Partition" | "Ratio" }>,
  labels: _Query.Labels,
  span: _Query.Window,
  filters: ReadonlyArray<_Query.Matcher>,
): _Query =>
  sli._tag === "Ratio"
    ? _Query.Binary({ left: _rated(sli.good, labels, span, filters), op: "div", right: _rated(sli.total, labels, span, filters) })
    : _Query.Binary({
      left: _rated(sli.metric, labels, span, [...filters, Reliability.Filter.make({ key: sli.by, op: "regex", value: _alternate(sli.good) })]),
      op: "div",
      right: _rated(sli.metric, labels, span, filters),
    })

const _timeShare = (
  metric: Convention.MetricName,
  bound: _Query.Finite,
  op: keyof typeof _OPS,
  labels: _Query.Labels,
  span: _Query.Window,
  filters: ReadonlyArray<_Query.Matcher>,
): _Query => _Query.Windowed({
  fn: "avg",
  of: _Query.Binary({ left: _Query.Instant({ labels, matchers: filters, metric }), op, right: _Query.Const({ value: bound }) }),
  window: span,
})

const _breach = (sli: Reliability.Sli, window: _Query.Window, labels: _Query.Labels, filters: ReadonlyArray<_Query.Matcher>): _Query => {
  const span = typeof window === "string" ? window : _Query.span(window)
  return Match.valueTags(sli, {
    Freshness: ({ horizon, metric }) => _timeShare(metric, _Query.finite(Convention.duration(metric, horizon)), "gt", labels, span, filters),
    Latency: ({ ceiling, metric }) =>
      _complement(_Query.Fraction({ labels, matchers: filters, metric, upper: _Query.finite(Convention.duration(metric, ceiling)), window: span })),
    Partition: (row) => _complement(_goodShare(row, labels, span, filters)),
    Ratio: (row) => _complement(_goodShare(row, labels, span, filters)),
    Saturation: ({ bound, breach, metric }) => _timeShare(metric, _Query.finite(bound), _POLARITY[breach], labels, span, filters),
  })
}

const _indicator = (sli: Reliability.Sli, window: _Query.Window, labels: _Query.Labels, filters: ReadonlyArray<_Query.Matcher>): _Query => {
  const span = typeof window === "string" ? window : _Query.span(window)
  return Match.valueTags(sli, {
    Freshness: ({ horizon, metric }) => _timeShare(metric, _Query.finite(Convention.duration(metric, horizon)), "gt", labels, span, filters),
    Latency: ({ metric, quantile }) => _Query.Quantile({ labels, matchers: filters, metric, q: _Query.quantile(quantile), window: span }),
    Partition: (row) => _goodShare(row, labels, span, filters),
    Ratio: (row) => _goodShare(row, labels, span, filters),
    Saturation: ({ bound, breach, metric }) => _timeShare(metric, _Query.finite(bound), _POLARITY[breach], labels, span, filters),
  })
}

const _burned = (spec: Reliability.Alert.Spec, labels: _Query.Labels): _Query => {
  const threshold = _Query.Const({ value: _Query.finite(spec.factor * (1 - spec.target)) })
  const exceeds = (window: Duration.Duration): _Query =>
    _Query.Binary({ left: _breach(spec.sli, _Query.span(window), labels, spec.filters), op: "gt", right: threshold })
  return _Query.Binary({ left: exceeds(spec.windows.short), op: "and", right: exceeds(spec.windows.long) })
}

type _Pane = { readonly span: typeof _Span.Type; readonly title: string } // the two literals every pane row carries whatever it renders

const _legend = (axes: ReadonlyArray<_Query.Key>): Option.Option<string> =>
  Array.match(axes, {
    onEmpty: Option.none,
    onNonEmpty: (keys) => Option.some(Array.join(Array.map(keys, (key) => `{{${key}}}`), " ")),
  })

const _display = (metric: Convention.MetricName, fold: Convention.Display): Option.Option<string> =>
  Option.some(Convention.grafanaUnit[Convention.Metric.at(metric).unit][fold])

const _TRENDS = {
  admitPassed: { axes: [Convention.rasm.admitScheme], fn: "rate", labels: {}, metric: Convention.metric.admitPassed, span: { h: 8, w: 12 }, title: "admissions by scheme" },
  admitRefused: { axes: [Convention.rasm.admitReason], fn: "rate", labels: {}, metric: Convention.metric.admitRefused, span: { h: 8, w: 12 }, title: "refusals by reason" },
  assetTransforms: { axes: [Convention.rasm.assetEngine, Convention.rasm.assetOutcome], fn: "rate", labels: {}, metric: Convention.metric.assetTransformed, span: { h: 8, w: 12 }, title: "asset transforms by engine and outcome" },
  auditActions: { axes: [Convention.rasm.auditAction], fn: "rate", labels: _tenant, metric: Convention.metric.factDrained, span: { h: 8, w: 16 }, title: "audit actions" },
  chartFrames: { axes: [], fn: "rate", labels: {}, metric: Convention.metric.chartFrames, span: { h: 6, w: 8 }, title: "pivot delta frames" },
  flagOutcomes: { axes: [Convention.wire.occurrence], fn: "rate", labels: {}, metric: Convention.metric.flagTracked, span: { h: 8, w: 12 }, title: "tracked outcomes by event" },
  formSubmits: { axes: [Convention.rasm.formOutcome], fn: "rate", labels: {}, metric: Convention.metric.formSubmit, span: { h: 8, w: 12 }, title: "submit trips by outcome" },
  gatewayOutcomes: { axes: [Convention.rasm.gatewayOutcome], fn: "rate", labels: {}, metric: Convention.metric.gatewayCommands, span: { h: 8, w: 12 }, title: "gateway outcomes" },
  idempotency: { axes: [Convention.rasm.admitDisposition], fn: "rate", labels: {}, metric: Convention.metric.idempotencyOutcome, span: { h: 8, w: 12 }, title: "idempotency dispositions" },
  invokeFaults: { axes: [Convention.wire.occurrence], fn: "rate", labels: {}, metric: Convention.metric.invokeFault, span: { h: 8, w: 12 }, title: "fault reasons" }, // the frequency export mints the reason under the owned occurrence axis
  invokeOutcomes: { axes: [Convention.rasm.invokeOutcome], fn: "rate", labels: {}, metric: Convention.metric.invokeCalls, span: { h: 8, w: 12 }, title: "invoke outcomes" },
  objectWrites: { axes: [Convention.rasm.objectOutcome], fn: "rate", labels: {}, metric: Convention.metric.objectWritten, span: { h: 8, w: 12 }, title: "writes by outcome" },
  olapRetries: { axes: [Convention.rasm.olapEngine], fn: "rate", labels: {}, metric: Convention.metric.olapRetried, span: { h: 8, w: 12 }, title: "queries retried" },
  remoteBytes: { axes: [Convention.rasm.remoteOp, Convention.rasm.remoteScheme], fn: "rate", labels: {}, metric: Convention.metric.remoteBytes, span: { h: 8, w: 12 }, title: "remote throughput" },
  remoteExecExits: { axes: [Convention.attr.errorType, Convention.rasm.remoteScheme], fn: "rate", labels: {}, metric: Convention.metric.remoteExecExits, span: { h: 6, w: 8 }, title: "remote command exits" },
  remoteOps: { axes: [Convention.attr.errorType, Convention.rasm.remoteOp, Convention.rasm.remoteScheme], fn: "rate", labels: {}, metric: Convention.metric.remoteOps, span: { h: 8, w: 14 }, title: "remote operations by verb and fault class" },
  remoteResumed: { axes: [Convention.rasm.remoteEngine], fn: "rate", labels: {}, metric: Convention.metric.remoteResumed, span: { h: 6, w: 8 }, title: "transfers resumed by engine" },
  remoteSyncActions: { axes: [Convention.rasm.remoteAction], fn: "rate", labels: {}, metric: Convention.metric.remoteSyncActions, span: { h: 8, w: 10 }, title: "reconciliation actions" },
  remoteWatchChanges: { axes: [Convention.rasm.remoteScheme, Convention.rasm.remoteWatch], fn: "rate", labels: {}, metric: Convention.metric.remoteWatchChanges, span: { h: 6, w: 8 }, title: "watch changes by strategy" },
  sceneGrafts: { axes: [], fn: "rate", labels: {}, metric: Convention.metric.sceneGrafts, span: { h: 6, w: 8 }, title: "graft arrivals" },
  sceneRefusals: { axes: [Convention.wire.occurrence], fn: "rate", labels: {}, metric: Convention.metric.sceneRefusals, span: { h: 8, w: 12 }, title: "graft refusals by reason" },
  securityAdmissions: { axes: [Convention.rasm.securityKind], fn: "rate", labels: {}, metric: Convention.metric.securityAdmitted, span: { h: 8, w: 14 }, title: "authenticity admissions" },
  securityDenials: { axes: [Convention.rasm.securityReason], fn: "rate", labels: {}, metric: Convention.metric.securityPolicyDeny, span: { h: 8, w: 12 }, title: "authorization denials" },
  securityJwksMisses: { axes: [], fn: "rate", labels: {}, metric: Convention.metric.securityJwksMiss, span: { h: 6, w: 8 }, title: "cold JWKS resolutions" },
  securityKeyQuarantines: { axes: [], fn: "rate", labels: {}, metric: Convention.metric.securityJwksQuarantined, span: { h: 6, w: 8 }, title: "keys quarantined" },
  securityRejects: { axes: [Convention.rasm.securityKind], fn: "rate", labels: {}, metric: Convention.metric.securityRejects, span: { h: 8, w: 14 }, title: "authenticity rejects" },
  securityReplays: { axes: [Convention.rasm.securitySurface], fn: "rate", labels: { [Convention.rasm.securityKind]: "reuse" }, metric: Convention.metric.securityRejects, span: { h: 8, w: 10 }, title: "replayed credentials" },
  securityRotations: { axes: [], fn: "rate", labels: {}, metric: Convention.metric.securitySecretRotation, span: { h: 6, w: 8 }, title: "secret rotations" },
  securityShreds: { axes: [], fn: "rate", labels: {}, metric: Convention.metric.securityShredReject, span: { h: 6, w: 8 }, title: "shredded-key opens" },
  verdicts: { axes: [Convention.rasm.benchVerdict], fn: "rate", labels: {}, metric: Convention.metric.benchVerdicts, span: { h: 8, w: 12 }, title: "regression verdicts" },
  vitalGrades: { axes: [Convention.rasm.vitalKind, Convention.rasm.vitalGrade], fn: "rate", labels: {}, metric: Convention.metric.vitalObserved, span: { h: 8, w: 24 }, title: "observations by grade" },
} as const satisfies Record<string, _Pane & { readonly axes: ReadonlyArray<_Query.Key>; readonly fn: _Fn; readonly labels: _Query.Labels; readonly metric: Convention.MetricName }>

const _FACETS = {
  auditActors: { axes: [Convention.rasm.auditActorKind, Convention.rasm.auditAction], labels: _tenant, metric: Convention.metric.factDrained, span: { h: 8, w: 8 }, title: "actors by action" },
  crashClasses: { axes: [Convention.attr.errorType], labels: {}, metric: Convention.metric.crashCaptured, span: { h: 8, w: 18 }, title: "captures by class" },
  flagEvents: { axes: [Convention.wire.occurrence], labels: {}, metric: Convention.metric.flagTracked, span: { h: 8, w: 12 }, title: "outcomes settled by event word" },
  securityFacets: {
    axes: [Convention.rasm.securityKind, Convention.rasm.securityDialect, Convention.rasm.securitySurface, Convention.rasm.securityReason],
    labels: {},
    metric: Convention.metric.securityRejects,
    span: { h: 8, w: 10 },
    title: "rejects by facet",
  },
} as const satisfies Record<string, _Pane & { readonly axes: ReadonlyArray<_Query.Key>; readonly labels: _Query.Labels; readonly metric: Convention.MetricName }>

const _LEVELS = {
  cacheResidency: { axes: [Convention.rasm.cacheName], metrics: [Convention.metric.cacheEntries], span: { h: 6, w: 12 }, title: "cache residency" },
  derivativePressure: { axes: [], metrics: [Convention.metric.derivativeActive, Convention.metric.derivativeQueued], span: { h: 6, w: 9 }, title: "derivative pressure" },
  laneProgress: { axes: [Convention.rasm.laneName], metrics: [Convention.metric.laneCheckpoint], span: { h: 6, w: 9 }, title: "lane checkpoints" },
  // one pool answers every scheme, remote origins included, so the full row is one series set rather than two irreconcilable held counts
  poolLeases: { axes: [Convention.rasm.poolScheme], metrics: [Convention.metric.poolHeld], span: { h: 6, w: 24 }, title: "pool leases held" },
  workDepth: {
    axes: [],
    metrics: [Convention.metric.outboxDepth, Convention.metric.queueDepth, Convention.metric.outboxRedelivered],
    span: { h: 8, w: 12 },
    title: "outbox and queue depth",
  },
} as const satisfies Record<string, _Pane & { readonly axes: ReadonlyArray<_Query.Key>; readonly metrics: Array.NonEmptyReadonlyArray<Convention.MetricName> }>

const _FLOWS = {
  factLanding: {
    axes: [Convention.rasm.factStream],
    metrics: [Convention.metric.factDrained, Convention.metric.factDeduped, Convention.metric.factRefused],
    span: { h: 8, w: 12 },
    title: "facts landed, deduped, and refused",
  },
  objectFlow: {
    axes: [],
    metrics: [Convention.metric.objectSize, Convention.metric.streamSize, Convention.metric.objectReclaimed],
    span: { h: 8, w: 12 },
    title: "landed, uploaded, reclaimed",
  },
} as const satisfies Record<string, _Pane & { readonly axes: ReadonlyArray<_Query.Key>; readonly metrics: Array.NonEmptyReadonlyArray<Convention.MetricName<"counter">> }>

const _trend = (board: _DashboardModel.Board, row: (typeof _TRENDS)[keyof typeof _TRENDS]): Panel =>
  Timeseries.make({
    exprs: [
      _Query.render(
        _Query.Aggregate({
          by: row.axes,
          of: _Query.Windowed({ fn: row.fn, of: _Query.Instant({ labels: row.labels, metric: row.metric }), window: _WINDOW }),
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

const _facets = (board: _DashboardModel.Board, row: (typeof _FACETS)[keyof typeof _FACETS]): Panel =>
  Table.make({
    exprs: [
      _Query.render(
        _Query.Aggregate({
          by: row.axes,
          of: _Query.Windowed({ fn: "increase", of: _Query.Instant({ labels: row.labels, metric: row.metric }), window: _DAY }),
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

const _grouped = (axes: ReadonlyArray<_Query.Key>, of: _Query): _Query =>
  Array.match(axes, { onEmpty: () => of, onNonEmpty: (by) => _Query.Aggregate({ by, of, op: "max" }) })

const _levels = (board: _DashboardModel.Board, row: (typeof _LEVELS)[keyof typeof _LEVELS]): Panel =>
  Timeseries.make({
    exprs: Array.map(row.metrics, (metric) => _Query.render(_grouped(row.axes, _Query.Instant({ labels: {}, metric })), board.target)),
    legend: _legend(row.axes),
    source: board.target.source,
    span: row.span,
    steps: [],
    title: row.title,
    unit: _display(Array.headNonEmpty(row.metrics), "level"),
  })

const _flow = (board: _DashboardModel.Board, row: (typeof _FLOWS)[keyof typeof _FLOWS]): Panel =>
  Timeseries.make({
    exprs: Array.map(row.metrics, (metric) =>
      _Query.render(
        _Query.Aggregate({
          by: row.axes,
          of: _Query.Windowed({ fn: "rate", of: _Query.Instant({ labels: {}, metric }), window: _WINDOW }),
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

const _isSummary = (metric: Convention.MetricName): metric is Convention.MetricName<"summary"> =>
  Convention.Metric.at(metric).kind === "summary"

const _rung = (metric: Convention.MetricName<"histogram" | "summary">, labels: _Query.Labels, quantile: _Query.QuantileValue): _Query =>
  _isSummary(metric)
    ? _Query.Instant({ labels: { ...labels, quantile: `${quantile}` }, metric })
    : _Query.Quantile({ labels, metric, q: quantile, window: _WINDOW })

const _quantile = (row: { readonly labels: _Query.Labels; readonly metric: Convention.MetricName<"histogram" | "summary">; readonly title: string }) =>
(board: _DashboardModel.Board) =>
(quantile: _Query.QuantileValue): Panel =>
  Timeseries.make({
    exprs: [_Query.render(_rung(row.metric, row.labels, quantile), board.target)],
    legend: Option.none(),
    source: board.target.source,
    span: { h: 8, w: 12 },
    steps: [],
    title: `${row.title} p${Number.round(quantile * 100, 0)}`,
    unit: _display(row.metric, "level"), // a rung IS the quantity, so the level column answers whatever code the histogram declares
  })

const _latency = _quantile({ labels: _tenant, metric: Convention.metric.httpServerDuration, title: "latency" })
const _invokeLatency = _quantile({ labels: {}, metric: Convention.metric.invokeDuration, title: "invoke" }) // the capability instruments are process-level: no tenant tag exists on their series
const _gatewayLatency = _quantile({ labels: {}, metric: Convention.metric.gatewayDuration, title: "gateway" })
const _batchLatency = _quantile({ labels: {}, metric: Convention.metric.batchDuration, title: "batch window" })
const _transcodeLatency = _quantile({ labels: {}, metric: Convention.metric.assetTranscodeDuration, title: "asset transcode" })
const _lakeWait = _quantile({ labels: {}, metric: Convention.metric.olapWait, title: "lake wait" })
const _lakeDeferred = _quantile({ labels: {}, metric: Convention.metric.olapDeferred, title: "deferred wait" })
const _lakeProfile = _quantile({ labels: {}, metric: Convention.metric.profileDuration, title: "engine profile" })
const _jwksLatency = _quantile({ labels: {}, metric: Convention.metric.securityJwksResolve, title: "JWKS resolve" })
const _kdfLatency = _quantile({ labels: {}, metric: Convention.metric.securityKdf, title: "key derivation" })
const _ceremonyLatency = _quantile({ labels: {}, metric: Convention.metric.securityCeremony, title: "credential ceremony" })
const _remoteLatency = _quantile({ labels: {}, metric: Convention.metric.remoteDuration, title: "remote operation" }) // the windowed row: the rank reads off the export's own quantile label

const _vitalGauge = (board: _DashboardModel.Board) =>
(gauge: { readonly ceiling: number; readonly kind: string; readonly metric: Convention.MetricName<"gauge"> }): Panel =>
  Gauge.make({
    ceiling: gauge.ceiling,
    expr: _Query.render(
      _Query.Windowed({
        fn: "avg",
        of: _Query.Instant({ labels: { [Convention.rasm.vitalKind]: gauge.kind }, metric: gauge.metric }),
        window: _WINDOW,
      }),
      board.target,
    ),
    source: board.target.source,
    span: { h: 6, w: 4 },
    steps: [{ at: gauge.ceiling, tone: Reliability.Alert.Severity.at("page").tone }],
    title: gauge.kind,
  })

const _usage = (board: _DashboardModel.Board) => (resource: string): Panel =>
  Timeseries.make({
    exprs: [
      _Query.render(
        _Query.Aggregate({
          by: [Convention.rasm.tenant],
          of: _Query.Windowed({
            fn: "increase",
            of: _Query.Instant({ labels: { [Convention.rasm.meterResource]: resource, ..._tenant }, metric: Convention.metric.meterUsage }),
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

const _crashes = (board: _DashboardModel.Board): Panel =>
  Logs.make({
    filter: Convention.event.exception,
    source: board.logs,
    span: { h: 8, w: 24 },
    title: "exception records",
  })

const _crashRate = (board: _DashboardModel.Board): Panel =>
  Stat.make({
    expr: _Query.render(
      _Query.Windowed({ fn: "rate", of: _Query.Instant({ labels: {}, metric: Convention.metric.crashCaptured }), window: _WINDOW }),
      board.target,
    ),
    source: board.target.source,
    span: { h: 6, w: 6 },
    steps: [],
    title: "crash capture rate",
    unit: _display(Convention.metric.crashCaptured, "rate"),
  })

const _workFlow = (board: _DashboardModel.Board): Panel =>
  Timeseries.make({
    exprs: [
      _Query.render(
        _Query.Aggregate({
          by: [Convention.rasm.workChannel],
          of: _Query.Windowed({ fn: "rate", of: _Query.Instant({ labels: {}, metric: Convention.metric.relayDrained }), window: _WINDOW }),
          op: "sum",
        }),
        board.target,
      ),
      _Query.render(
        _Query.Aggregate({
          by: [],
          of: _Query.Windowed({ fn: "rate", of: _Query.Instant({ labels: {}, metric: Convention.metric.queueParked }), window: _WINDOW }),
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

const _workAge = (board: _DashboardModel.Board): Panel =>
  Stat.make({
    expr: _Query.render(_Query.Aggregate({ by: [], of: _Query.Instant({ labels: {}, metric: Convention.metric.outboxAge }), op: "max" }), board.target),
    source: board.target.source,
    span: { h: 4, w: 6 },
    steps: [],
    title: "oldest undelivered age",
    unit: _display(Convention.metric.outboxAge, "level"),
  })

const _cacheShare = (board: _DashboardModel.Board): Panel => {
  const perCache = (metric: Convention.MetricName<"counter">): _Query =>
    _Query.Aggregate({
      by: [Convention.rasm.cacheName],
      of: _Query.Windowed({ fn: "rate", of: _Query.Instant({ labels: {}, metric }), window: _WINDOW }),
      op: "sum",
    })
  const hits = perCache(Convention.metric.cacheHits)
  return Timeseries.make({
    exprs: [
      _Query.render(
        _Query.Binary({
          left: hits,
          op: "div",
          right: _Query.Binary({ left: hits, op: "add", right: perCache(Convention.metric.cacheMisses) }),
        }),
        board.target,
      ),
    ],
    legend: _legend([Convention.rasm.cacheName]),
    source: board.target.source,
    span: { h: 6, w: 12 },
    steps: [],
    title: "cache hit share",
    unit: Option.none(),
  })
}

const _EVIDENCE = _Query.span(Duration.days(30)) // the residence horizon: the window a metrics store's retention cannot hold

const _evidence = (board: _DashboardModel.Board, row: {
  readonly axes: ReadonlyArray<_Query.Key>
  readonly metric: Convention.MetricName<"counter">
  readonly span: typeof _Span.Type
  readonly title: string
}): ReadonlyArray<Panel> =>
  Option.match(board.analytics, {
    onNone: () => [],
    onSome: (target) => [
      Timeseries.make({
        exprs: [
          _Query.render(
            _Query.Aggregate({
              by: row.axes,
              of: _Query.Windowed({ fn: "increase", of: _Query.Instant({ labels: {}, metric: row.metric }), window: _EVIDENCE }),
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

const _benchLadder = (board: _DashboardModel.Board) => (suite: string): Panel =>
  Timeseries.make({
    exprs: [
      _Query.render(
        _grouped(
          [Convention.rasm.benchBand, Convention.rasm.benchLabel],
          _Query.Instant({ labels: { [Convention.rasm.benchSuite]: suite }, metric: Convention.metric.benchTime }),
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

const _BENCH_ENRICHMENT = [
  { axes: [], metric: Convention.metric.benchGc, title: "gc timing" },
  { axes: [], metric: Convention.metric.benchHeap, title: "heap delta" },
  { axes: [Convention.rasm.benchCounterKind], metric: Convention.metric.benchCounter, title: "hardware counters" },
] as const satisfies ReadonlyArray<{ readonly axes: ReadonlyArray<_Query.Key>; readonly metric: Convention.MetricName<"gauge">; readonly title: string }>

const _benchEnrichment = (board: _DashboardModel.Board, suite: string, row: (typeof _BENCH_ENRICHMENT)[number]): Panel =>
  pipe([Convention.rasm.benchBand, Convention.rasm.benchLabel, ...row.axes], (axes) =>
  Timeseries.make({
    exprs: [
      _Query.render(
        _grouped(axes, _Query.Instant({ labels: { [Convention.rasm.benchSuite]: suite }, metric: row.metric })),
        board.target,
      ),
    ],
    legend: _legend(axes),
    source: board.target.source,
    span: { h: 8, w: 12 },
    steps: [],
    title: `${suite} ${row.title}`,
    unit: _display(row.metric, "level"),
  }))

const _burnPair = (board: _DashboardModel.Board) => (spec: Reliability.Alert.Spec): Panel =>
  Timeseries.make({
    exprs: Array.map([spec.windows.long, spec.windows.short], (window) =>
      _Query.render(
        _Query.Binary({ left: _Query.breach(spec.sli, _Query.span(window), _tenant), op: "div", right: _Query.Const({ value: _Query.finite(1 - spec.target) }) }),
        board.target,
      )),
    legend: Option.none(),
    source: board.target.source,
    span: { h: 6, w: 12 },
    steps: [{ at: spec.factor, tone: spec.severity.tone }],
    title: `${spec.slug} trips at ${spec.factor}x — ${Number.round(spec.spend * 100, 1)}% budget`, // the derived spend prints here: the human figure cannot drift from the row that fires it
    unit: Option.none(),
  })

declare namespace _DashboardModel {
  type Pack = keyof Payload
  type Payload = {
    readonly audit: Record.ReadonlyRecord<never, never>
    readonly bench: { readonly suites: ReadonlyArray<string> }
    readonly crash: Record.ReadonlyRecord<never, never>
    readonly flag: Record.ReadonlyRecord<never, never>
    readonly invoke: { readonly quantiles: ReadonlyArray<_Query.QuantileValue> }
    readonly lake: { readonly quantiles: ReadonlyArray<_Query.QuantileValue> }
    readonly meter: { readonly resources: ReadonlyArray<string> }
    readonly object: { readonly quantiles: ReadonlyArray<_Query.QuantileValue> }
    readonly overview: { readonly quantiles: ReadonlyArray<_Query.QuantileValue> }
    readonly security: { readonly quantiles: ReadonlyArray<_Query.QuantileValue> }
    readonly slo: { readonly objectives: ReadonlyArray<Reliability.Objective> }
    readonly vital: {
      readonly gauges: ReadonlyArray<{ readonly ceiling: number; readonly kind: string; readonly metric: Convention.MetricName<"gauge"> }>
    }
    readonly view: Record.ReadonlyRecord<never, never>
    readonly work: { readonly quantiles: ReadonlyArray<_Query.QuantileValue> }
  }
  type Suite = Payload["bench"] & Payload["meter"] & Payload["overview"] & Payload["slo"] & Payload["vital"]
}

const _PACKS: { readonly [K in _DashboardModel.Pack]: (board: _DashboardModel.Board, payload: _DashboardModel.Payload[K]) => _DashboardModel } = {
  audit: (board) =>
    _DashboardModel.of(board, {
      annotations: [],
      panels: [_trend(board, _TRENDS.auditActions), _facets(board, _FACETS.auditActors)],
      slug: "audit",
      tags: ["audit"],
      title: "audit",
      variables: [],
    }),
  bench: (board, payload) =>
    _DashboardModel.of(board, {
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
    _DashboardModel.of(board, {
      annotations: [],
      panels: [_crashRate(board), _facets(board, _FACETS.crashClasses), _crashes(board)],
      slug: "crash",
      tags: ["crash"],
      title: "crash",
      variables: [],
    }),
  // `flagTracked` is the metric plane's whole flag surface: evaluation and tracking evidence rides span attributes,
  // which the trace ruling renders through wide-event residence rather than a board query.
  flag: (board) =>
    _DashboardModel.of(board, {
      annotations: [],
      panels: [_trend(board, _TRENDS.flagOutcomes), _facets(board, _FACETS.flagEvents)],
      slug: "flag",
      tags: ["flag", "experiment"],
      title: "flag outcomes",
      variables: [],
    }),
  invoke: (board, payload) =>
    _DashboardModel.of(board, {
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
    _DashboardModel.of(board, {
      annotations: [],
      panels: [
        ...Array.map(payload.quantiles, _lakeWait(board)),
        ...Array.map(payload.quantiles, _lakeDeferred(board)),
        ...Array.map(payload.quantiles, _lakeProfile(board)),
        _trend(board, _TRENDS.olapRetries),
        _cacheShare(board),
        _levels(board, _LEVELS.cacheResidency),
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
    _DashboardModel.of(board, {
      annotations: [],
      panels: Array.map(payload.resources, _usage(board)),
      slug: "meter",
      tags: ["meter", "billing"],
      title: "usage",
      variables: [],
    }),
  object: (board, payload) =>
    _DashboardModel.of(board, {
      annotations: [],
      panels: [
        _trend(board, _TRENDS.objectWrites),
        _flow(board, _FLOWS.objectFlow),
        _trend(board, _TRENDS.assetTransforms),
        ...Array.map(payload.quantiles, _transcodeLatency(board)),
        _trend(board, _TRENDS.remoteOps),
        _trend(board, _TRENDS.remoteBytes),
        ...Array.map(payload.quantiles, _remoteLatency(board)),
        _levels(board, _LEVELS.poolLeases),
        _trend(board, _TRENDS.remoteResumed),
        _trend(board, _TRENDS.remoteSyncActions),
        _trend(board, _TRENDS.remoteWatchChanges),
        _trend(board, _TRENDS.remoteExecExits),
      ],
      slug: "object",
      tags: ["object", "storage"],
      title: "object plane",
      variables: [],
    }),
  overview: (board, payload) =>
    _DashboardModel.of(board, {
      annotations: [],
      panels: [
        ...Array.map(payload.quantiles, _latency(board)),
        _trend(board, _TRENDS.admitPassed),
        _trend(board, _TRENDS.admitRefused),
        _trend(board, _TRENDS.idempotency),
      ],
      slug: "overview",
      tags: ["overview"],
      title: "service overview",
      variables: [],
    }),
  security: (board, payload) =>
    _DashboardModel.of(board, {
      annotations: [],
      panels: [
        _trend(board, _TRENDS.securityAdmissions),
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
        ...Array.map(payload.quantiles, _ceremonyLatency(board)),
      ],
      slug: "security",
      tags: ["security"],
      title: "authenticity and custody",
      variables: [],
    }),
  slo: (board, payload) =>
    _DashboardModel.of(board, {
      annotations: Array.flatMap(payload.objectives, (objective) =>
        Array.map(Reliability.Alert.of(objective), (spec) => ({ slug: spec.slug, tone: spec.severity.tone }))),
      panels: Array.flatMap(payload.objectives, (objective) => Array.map(Reliability.Alert.of(objective), _burnPair(board))),
      slug: "slo",
      tags: ["slo"],
      title: "objectives",
      variables: [],
    }),
  vital: (board, payload) =>
    _DashboardModel.of(board, {
      annotations: [],
      panels: [...Array.map(payload.gauges, _vitalGauge(board)), _trend(board, _TRENDS.vitalGrades)],
      slug: "vital",
      tags: ["vital", "rum"],
      title: "web vitals",
      variables: [],
    }),
  view: (board) =>
    _DashboardModel.of(board, {
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
    _DashboardModel.of(board, {
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

const _SUITE: { readonly [K in _DashboardModel.Pack]: (board: _DashboardModel.Board, payload: _DashboardModel.Suite) => _DashboardModel } = {
  audit: (board) => _PACKS.audit(board, {}),
  bench: (board, payload) => _PACKS.bench(board, { suites: payload.suites }),
  crash: (board) => _PACKS.crash(board, {}),
  flag: (board) => _PACKS.flag(board, {}),
  invoke: (board, payload) => _PACKS.invoke(board, { quantiles: payload.quantiles }),
  lake: (board, payload) => _PACKS.lake(board, { quantiles: payload.quantiles }),
  meter: (board, payload) => _PACKS.meter(board, { resources: payload.resources }),
  object: (board, payload) => _PACKS.object(board, { quantiles: payload.quantiles }),
  overview: (board, payload) => _PACKS.overview(board, { quantiles: payload.quantiles }),
  security: (board, payload) => _PACKS.security(board, { quantiles: payload.quantiles }),
  slo: (board, payload) => _PACKS.slo(board, { objectives: payload.objectives }),
  vital: (board, payload) => _PACKS.vital(board, { gauges: payload.gauges }),
  view: (board) => _PACKS.view(board, {}),
  work: (board, payload) => _PACKS.work(board, { quantiles: payload.quantiles }),
}

declare namespace Board {
  type Claim = _Claim
  namespace Claim {
    type Band = _Bench.Band
    type Host = _BenchHost
    type Metric = _Bench.Metric
    type Subject = typeof _BenchSubject.Type
  }
  type DashboardModel = _DashboardModel
  namespace DashboardModel {
    type Board = _DashboardModel.Board
    type Page = _DashboardModel.Page
    type Placed = _DashboardModel.Placed
    type Signal = _DashboardModel.Signal
    type Wire = _DashboardModel.Wire
  }
  type Query = _Query
  namespace Query {
    type Dialect = _Query.Dialect
    type Engine = _Query.Engine
    type Finite = _Query.Finite
    type Histogram = _Query.Histogram
    type Key = _Query.Key
    type Labels = _Query.Labels
    type Matcher = _Query.Matcher
    type QuantileValue = _Query.QuantileValue
    type Residence = _Query.Residence
    type Span = _Query.Span
    type Target = _Query.Target
    type Window = _Query.Window
  }
  namespace Bench {
    type Band = _Bench.Band
    type Grade = _Bench.Grade
    type Metric = _Bench.Metric
    type Mint = _Bench.Mint
    type MitataRung = _Bench.MitataRung
    type Polarity = _Bench.Polarity
    type Row = _Bench.Row
    type Rung = _Bench.Rung
    type Tolerance = _Bench.Tolerance
    type Verdict = _Bench.Verdict
  }
}

const Board = { Bench: _Bench, Claim: _Claim, DashboardModel: _DashboardModel, Query: _Query } as const

export { Board }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
