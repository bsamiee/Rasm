# [RASM_PERSISTENCE_API_TIMESCALEDB_TOOLKIT]

`timescaledb_toolkit` supplies the hyperfunction and time-weighted-aggregate SQL layer over the `timescaledb` base at `api-timescaledb`, provisioned per the `Store/provisioning#SERVER_EXTENSIONS` row. It carries no managed assembly: every surface is server-side SQL a typed-SQL read composes, and the `Query/columnar#SERIES_AND_SCALEOUT` `SeriesLane.Weighted` read is the consuming rail.

## [01]-[TIME_WEIGHT]

`time_weight` is the two-stage aggregate discipline every toolkit hyperfunction follows: the aggregate stage folds samples into an intermediate summary object, an accessor then projects the scalar, and `rollup` re-aggregates summaries across groups without touching raw samples.

| [INDEX] | [SURFACE]                                              | [SEMANTICS]                                                               |
| :-----: | :----------------------------------------------------- | :------------------------------------------------------------------------ |
|  [01]   | `time_weight(method, ts, value)` → `TimeWeightSummary` | aggregate stage; folds samples into the summary object                    |
|  [02]   | `average(TimeWeightSummary)` → `double precision`      | time-weighted mean accessor; each sample weighted by its holding interval |
|  [03]   | `rollup(TimeWeightSummary)` → `TimeWeightSummary`      | re-aggregates per-group summaries before an accessor applies              |

- `method` is `'linear'` (alias `'trapezoidal'`) or `'LOCF'`, case-insensitive: `'linear'` interpolates across a gap between its bounding samples; `'LOCF'` carries the last observation forward — the method for change-triggered measurements.
- The summary object is composable state: `GROUP BY` buckets fold summaries, `rollup` joins buckets, one accessor closes — never a per-bucket raw re-scan.

## [02]-[IMPLEMENTATION_LAW]

- Package: `timescaledb_toolkit` (server-side, `BaseType("timescaledb")` admission per `Store/provisioning#SERVER_EXTENSIONS`)
- Owns: the time-weighted-aggregate algebra over irregular timesteps
- Accept: `average(time_weight('linear', at, value))` as the honest mean for irregular simulation series, `rollup` for cross-bucket re-aggregation
- Reject: a naive `avg(value)` over irregular timesteps (over-counts dense bursts), a managed EF translator for these functions, a self-provisioned `CREATE EXTENSION` outside the roster row
