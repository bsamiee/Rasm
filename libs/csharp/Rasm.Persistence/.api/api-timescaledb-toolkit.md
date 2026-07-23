# [RASM_PERSISTENCE_API_TIMESCALEDB_TOOLKIT]

`timescaledb_toolkit` owns the two-stage hyperfunction algebra over the Persistence series tables: an aggregate folds raw samples into a composable summary object, accessors project scalars off that summary, and `rollup` re-aggregates disjoint summaries across buckets without re-scanning a chunk. Every surface is server-side SQL a typed-SQL read composes as text, carrying no managed assembly and no EF translator. Summary state is what a continuous aggregate materialises, so accessor choice stays a read-time decision over one materialisation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `timescaledb_toolkit`
- package: `timescaledb_toolkit` (Timescale License)
- namespace: SQL — `public` aggregates, accessors, and the summary types they thread
- asset: server extension whose sole install precondition is the `timescaledb` base type
- rail: timescale-provisioning, analytical-lane

## [02]-[TIME_WEIGHT]

[TIME_WEIGHT_ENTRY_SCOPE]: sample-weighted mean and integral over irregular timesteps, carrying the aggregate/accessor/rollup discipline every family repeats

| [INDEX] | [SURFACE]                                               | [SHAPE]   | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------ | :-------- | :---------------------------------------- |
|  [01]   | `time_weight(TEXT, TIMESTAMPTZ, DOUBLE PRECISION)`      | aggregate | fold samples into a `TimeWeightSummary`   |
|  [02]   | `average(TimeWeightSummary) -> DOUBLE PRECISION`        | accessor  | mean weighted by each holding interval    |
|  [03]   | `integral(TimeWeightSummary, TEXT) -> DOUBLE PRECISION` | accessor  | curve area in the named time unit         |
|  [04]   | `first_val(TimeWeightSummary) -> DOUBLE PRECISION`      | accessor  | value at the first retained sample        |
|  [05]   | `last_val(TimeWeightSummary) -> DOUBLE PRECISION`       | accessor  | value at the last retained sample         |
|  [06]   | `first_time(TimeWeightSummary) -> TIMESTAMPTZ`          | accessor  | timestamp of the first retained sample    |
|  [07]   | `last_time(TimeWeightSummary) -> TIMESTAMPTZ`           | accessor  | timestamp of the last retained sample     |
|  [08]   | `rollup(TimeWeightSummary) -> TimeWeightSummary`        | rollup    | combine disjoint summaries across buckets |

- `method` is `'linear'` (alias `'trapezoidal'`) or `'LOCF'`, case-insensitive: `'linear'` interpolates across a gap between bounding samples, `'LOCF'` holds the last observation forward for change-triggered measurement.
- `interpolated_average` and `interpolated_integral` extend their base accessor with `(TIMESTAMPTZ, INTERVAL, prev, next)`, the neighbouring summaries a window `lag`/`lead` supplies, so a bucketed read carries no bound gap.
- `integral` defaults its unit to `second` and accepts any PostgreSQL time-unit alias down to `microsecond`.

## [03]-[STATISTICS]

[STATISTICS_ENTRY_SCOPE]: combinable moment and regression state, materialised once per bucket and read by many accessors

| [INDEX] | [SURFACE]                                                         | [SHAPE]   | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------- | :-------- | :----------------------------------------- |
|  [01]   | `stats_agg(DOUBLE PRECISION) -> StatsSummary1D`                   | aggregate | one-variable moment state                  |
|  [02]   | `stats_agg(DOUBLE PRECISION, DOUBLE PRECISION) -> StatsSummary2D` | aggregate | regression state, dependent variable first |
|  [03]   | `rolling(StatsSummary1D) -> StatsSummary1D`                       | rollup    | window-frame recombination                 |
|  [04]   | `rollup(StatsSummary1D) -> StatsSummary1D`                        | rollup    | combine summaries across buckets           |

- `stddev`, `variance`, `skewness`, `kurtosis`, and `covariance` take a second `TEXT` argument selecting `'population'` (the default) or `'sample'`.
- Every one-variable accessor has an `_x`/`_y` form over `StatsSummary2D`, so one regression aggregate answers both the fit and each axis's moments.
- `stats_agg` also admits a `BIGINT` value, folding integer counters into the same `StatsSummary1D`.

[STATS_1D_ACCESSORS]: `average` `sum` `num_vals` `stddev` `variance` `skewness` `kurtosis`
[STATS_2D_ACCESSORS]: `slope` `intercept` `x_intercept` `corr` `covariance` `determination_coeff` `num_vals`

## [04]-[PERCENTILE]

[PERCENTILE_ENTRY_SCOPE]: bounded-error percentile sketches that partial-aggregate, so a quantile read never sorts a chunk

| [INDEX] | [SURFACE]                                                             | [SHAPE]   | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------- | :-------- | :----------------------------------- |
|  [01]   | `percentile_agg(DOUBLE PRECISION) -> UddSketch`                       | aggregate | default-sized sketch over a column   |
|  [02]   | `uddsketch(INTEGER, DOUBLE PRECISION, DOUBLE PRECISION) -> UddSketch` | aggregate | bucket-count and target-error sketch |
|  [03]   | `approx_percentile(DOUBLE PRECISION, UddSketch) -> DOUBLE PRECISION`  | accessor  | value at a quantile in `[0.0, 1.0]`  |
|  [04]   | `rollup(UddSketch) -> UddSketch`                                      | rollup    | union sketches across buckets        |

- `error` reports the achieved relative error: a sketch short of buckets widens its error bound rather than narrowing its percentile range, so every read carries its own accuracy evidence.
- Target relative error binds to `[1.0e-12, 1.0)`; a value outside that range breaks the sketch.

[UDDSKETCH_ACCESSORS]: `approx_percentile` `approx_percentile_array` `approx_percentile_rank` `error` `mean` `num_vals`

## [05]-[COUNTER_AND_STATE]

[COUNTER_AND_STATE_ENTRY_SCOPE]: reset-adjusted counter algebra, discrete state durations, and heartbeat liveness over a declared window

| [INDEX] | [SURFACE]                                                      | [SHAPE]   | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------- | :-------- | :---------------------------------- |
|  [01]   | `counter_agg(TIMESTAMPTZ, DOUBLE PRECISION, TSTZRANGE)`        | aggregate | reset-adjusted `CounterSummary`     |
|  [02]   | `with_bounds(CounterSummary, TSTZRANGE) -> CounterSummary`     | mutator   | attach bounds after aggregation     |
|  [03]   | `extrapolated_delta(CounterSummary, TEXT) -> DOUBLE PRECISION` | accessor  | increase extrapolated to the bounds |
|  [04]   | `extrapolated_rate(CounterSummary, TEXT) -> DOUBLE PRECISION`  | accessor  | per-second rate over the bounds     |
|  [05]   | `state_agg(TIMESTAMPTZ, TEXT) -> StateAgg`                     | aggregate | state periods with transition times |
|  [06]   | `duration_in(StateAgg, TEXT) -> INTERVAL`                      | accessor  | time held in one state              |
|  [07]   | `state_at(StateAgg, TIMESTAMPTZ) -> TEXT`                      | accessor  | state holding at an instant         |
|  [08]   | `state_timeline(StateAgg)`                                     | setof     | `(state, start_time, end_time)`     |
|  [09]   | `heartbeat_agg(TIMESTAMPTZ, TIMESTAMPTZ, INTERVAL, INTERVAL)`  | aggregate | liveness over a declared window     |
|  [10]   | `live_ranges(HeartbeatAgg)`                                    | setof     | `(start, end)` live intervals       |
|  [11]   | `uptime(HeartbeatAgg) -> INTERVAL`                             | accessor  | summed live duration                |

- `extrapolated_delta` and `extrapolated_rate` take `'prometheus'` as the method and demand bounds, so an irregular bucket reports its increase to the bucket edge rather than to its last observed sample.
- `duration_in` also accepts a `(TIMESTAMPTZ, INTERVAL)` range pair, narrowing the accounted window without re-aggregating.
- `state_agg` over a `BIGINT` state projects through the `_int` accessor forms `state_at_int`, `state_int_timeline`, and `into_int_values`.
- `interpolated_delta`/`interpolated_rate`, `interpolated_duration_in`, and `interpolated_uptime`/`interpolated_downtime` take the bucket bound with its neighbouring aggregate, so a `time_bucket` read closes its own edges.

[COUNTER_ACCESSORS]: `delta` `rate` `slope` `intercept` `corr` `time_delta` `num_changes` `num_elements` `num_resets` `counter_zero_time` `idelta_left` `idelta_right` `irate_left` `irate_right`
[STATE_ACCESSORS]: `duration_in` `state_at` `state_periods` `state_timeline` `into_values`
[HEARTBEAT_ACCESSORS]: `uptime` `downtime` `live_at` `live_ranges` `dead_ranges` `num_gaps` `num_live_ranges` `trim_to` `interpolate`

## [06]-[DOWNSAMPLE_AND_CARDINALITY]

[DOWNSAMPLE_ENTRY_SCOPE]: server-side reduction to a graph-sized point set, and distinct-count sketches that union across buckets

| [INDEX] | [SURFACE]                                         | [SHAPE]   | [CAPABILITY]                        |
| :-----: | :------------------------------------------------ | :-------- | :---------------------------------- |
|  [01]   | `timevector(TIMESTAMPTZ, DOUBLE PRECISION)`       | aggregate | pack a series into one `Timevector` |
|  [02]   | `lttb(TIMESTAMPTZ, DOUBLE PRECISION, INT)`        | aggregate | peak-preserving triangle downsample |
|  [03]   | `asap_smooth(TIMESTAMPTZ, DOUBLE PRECISION, INT)` | aggregate | autocorrelation-chosen smoothing    |
|  [04]   | `unnest(Timevector)`                              | setof     | expand to `(time, value)` rows      |
|  [05]   | `hyperloglog(INTEGER, AnyElement) -> Hyperloglog` | aggregate | bucketed distinct-count sketch      |
|  [06]   | `distinct_count(Hyperloglog) -> BIGINT`           | accessor  | approximate cardinality             |
|  [07]   | `stderror(Hyperloglog) -> DOUBLE PRECISION`       | accessor  | relative error of the estimate      |
|  [08]   | `rollup(Hyperloglog) -> Hyperloglog`              | rollup    | union sketches across buckets       |

- `resolution` sets the approximate point count a graph consumes, so a tile read reduces server-side and ships no raw chunk.
- `hyperloglog` rounds its bucket count up to a power of two within `[16, 2^18]` and admits any type carrying an extended hash function.

## [07]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Aggregate, accessor, and `rollup` are three separable stages: a `timescaledb.continuous` view materialises the summary column alone, and each accessor stays a read-time choice over that one materialisation.
- `rollup` combines only disjoint summaries — overlapping inputs fault, so the bucket key partitions the series before re-aggregation.
- Ordered-input aggregates refuse Postgres parallelism and buffer their input; a `time_bucket` `GROUP BY` or continuous aggregate is what makes them partition-safe and memory-bounded.
- Every surface enters as read text a typed-SQL command composes: values bind as parameters and the relation name rides a closed-vocabulary row literal.

[STACKING]:
- `timescaledb`(`.api/api-timescaledb.md`): `time_bucket` groups the samples each aggregate folds, and a `timescaledb.continuous` view materialises the summary column `rollup` later re-aggregates across bucket widths.
- `npgsql`(`.api/api-npgsql.md`): `NpgsqlDataReader.GetDouble` and `GetFieldValue<Instant>` read every accessor projection; a summary type carries no managed mapping, so the accessor closes server-side before the reader sees a column.
- `duckdb`(`.api/api-duckdb.md`): `postgres_scan` joins the accessor-projected scalar columns as a columnar leg, the summary types staying inside the server process.
- `Query/columnar#SERIES_AND_SCALEOUT`: `SeriesLane.Weighted` composes `average(time_weight('linear', at, value))` over raw chunks where the pre-bucketed `SeriesLane.Bucketed` read cannot answer, and the `SeriesKind` row carries the bucket both share.
- Within-library composition folds one aggregate into many reads: a single `time_weight` continuous aggregate feeds `average`, `integral`, `first_val`, and `last_val`, and the `interpolated_*` accessor family closes each bucket bound off its `lag`/`lead` neighbour.

[LOCAL_ADMISSION]:
- `Store/provisioning#SERVER_EXTENSIONS` carries the `ServerExtension.TimescaledbToolkit` row under `ExtensionAdmission.BaseType("timescaledb")`, so the base extension resolves first and the row admits through `CREATE EXTENSION IF NOT EXISTS` inside the verification session.

[RAIL_LAW]:
- Package: `timescaledb_toolkit`
- Owns: the two-stage aggregate algebra over irregular series — time-weighted mean and integral, moment and regression state, bounded-error percentiles, reset-adjusted counters, state duration, liveness, and visual downsampling
- Accept: a summary column materialised once by a continuous aggregate, accessors projected at read time, `rollup` closing across disjoint buckets
- Reject: a naive `avg` over irregular timesteps, an exact percentile sort over a full chunk scan, a client-side moment or reset-adjusted counter fold, a managed EF translator over these functions
