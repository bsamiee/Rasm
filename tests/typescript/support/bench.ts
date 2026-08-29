import { fileURLToPath } from 'node:url';
import { FileSystem, Path } from '@effect/platform';
import {
    Array,
    Context,
    Data,
    DateTime,
    Effect,
    Option,
    Order,
    pipe,
    Record,
    Schema,
    String,
} from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

declare namespace Benchmark {
    type Policy = typeof _POLICY;
    type Verdict = 'pass' | 'noisy' | 'regression';
    type Report = {
        readonly verdict: Verdict;
        readonly benchmarks: ReadonlyArray<{
            readonly name: string;
            readonly verdict: Verdict;
            readonly baselineHz: number;
            readonly recentHz: number;
        }>;
    };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _POLICY = {
    window: 3,
    tolerance: 0.15,
    noiseCap: 10,
    minHistory: 5,
} as const;

const _FILES = { latest: 'latest.json', history: 'history.ndjson' } as const;
const _SEVERITY = {
    pass: 0,
    noisy: 1,
    regression: 2,
} as const satisfies Record<Benchmark.Verdict, number>;

// --- [MODELS] --------------------------------------------------------------------------

class BenchmarkResult extends Schema.Class<BenchmarkResult>('BenchmarkResult')({
    timestamp: Schema.String,
    name: Schema.NonEmptyString,
    hz: Schema.Number,
    rme: Schema.Number,
}) {}

const _Latest = Schema.Struct({
    files: Schema.Array(
        Schema.Struct({
            filepath: Schema.String,
            groups: Schema.Array(
                Schema.Struct({
                    fullName: Schema.String,
                    benchmarks: Schema.Array(
                        Schema.Struct({
                            name: Schema.String,
                            hz: Schema.Number,
                            rme: Schema.Number,
                        }),
                    ),
                }),
            ),
        }),
    ),
});

// --- [ERRORS] --------------------------------------------------------------------------

class BenchmarkError extends Data.TaggedError('BenchmarkError')<{
    readonly reason: 'regression' | 'malformed' | 'unreadable';
    readonly detail: string;
}> {}

// --- [SERVICES] ------------------------------------------------------------------------

class BenchmarkDirectory extends Context.Reference<BenchmarkDirectory>()(
    'rasm-test-support/BenchmarkDirectory',
    {
        defaultValue: (): string =>
            fileURLToPath(
                new URL(
                    '../../../.artifacts/typescript/bench',
                    import.meta.url,
                ),
            ),
    },
) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _decodeLatest = Schema.decodeUnknown(Schema.parseJson(_Latest), {
    errors: 'all',
});
const _decodeResult = Schema.decodeUnknown(Schema.parseJson(BenchmarkResult));
const _encodeResult = Schema.encode(Schema.parseJson(BenchmarkResult));

const _median = (values: ReadonlyArray<number>): Option.Option<number> =>
    pipe(Array.sort(values, Order.number), (sorted) =>
        Array.get(sorted, Math.floor(sorted.length / 2)),
    );

const _readHistory: Effect.Effect<
    ReadonlyArray<BenchmarkResult>,
    BenchmarkError,
    FileSystem.FileSystem | Path.Path
> = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const home = yield* BenchmarkDirectory;
    const raw = yield* Effect.orElseSucceed(
        fs.readFileString(path.join(home, _FILES.history)),
        () => '',
    );
    return yield* Effect.mapError(
        Effect.forEach(
            Array.filter(String.split(raw, '\n'), String.isNonEmpty),
            (line) => _decodeResult(line),
        ),
        (error) =>
            new BenchmarkError({ reason: 'malformed', detail: error.message }),
    );
});

const _importLatestResults: Effect.Effect<
    ReadonlyArray<BenchmarkResult>,
    BenchmarkError,
    FileSystem.FileSystem | Path.Path
> = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const home = yield* BenchmarkDirectory;
    const target = path.join(home, _FILES.latest);
    const raw = yield* Effect.mapError(
        fs.readFileString(target),
        (error) =>
            new BenchmarkError({ reason: 'unreadable', detail: error.message }),
    );
    const info = yield* Effect.mapError(
        fs.stat(target),
        (error) =>
            new BenchmarkError({ reason: 'unreadable', detail: error.message }),
    );
    const timestamp = yield* Option.match(info.mtime, {
        onNone: () => Effect.map(DateTime.now, DateTime.formatIso),
        onSome: (modifiedAt) =>
            Effect.succeed(
                DateTime.formatIso(DateTime.unsafeFromDate(modifiedAt)),
            ),
    });
    const latest = yield* Effect.mapError(
        _decodeLatest(raw),
        (error) =>
            new BenchmarkError({ reason: 'malformed', detail: error.message }),
    );
    const existing = yield* _readHistory;
    const rows = Array.flatMap(latest.files, (file) =>
        Array.flatMap(file.groups, (group) =>
            Array.map(
                group.benchmarks,
                (entry) =>
                    new BenchmarkResult({
                        timestamp,
                        name: `${group.fullName}::${entry.name}`,
                        hz: entry.hz,
                        rme: entry.rme,
                    }),
            ),
        ),
    );
    return yield* Array.some(existing, (row) => row.timestamp === timestamp)
        ? Effect.succeed<ReadonlyArray<BenchmarkResult>>([])
        : Effect.gen(function* () {
              const lines = yield* Effect.orDie(
                  Effect.forEach(rows, (row) => _encodeResult(row)),
              );
              yield* Effect.orDie(fs.makeDirectory(home, { recursive: true }));
              yield* Effect.mapError(
                  fs.writeFileString(
                      path.join(home, _FILES.history),
                      `${lines.join('\n')}\n`,
                      { flag: 'a' },
                  ),
                  (error) =>
                      new BenchmarkError({
                          reason: 'unreadable',
                          detail: error.message,
                      }),
              );
              return rows;
          });
});

const Benchmark = {
    POLICY: _POLICY,
    summarize: (
        rows: ReadonlyArray<BenchmarkResult>,
        policy: Benchmark.Policy = _POLICY,
    ): Benchmark.Report =>
        pipe(
            Array.groupBy(rows, (row) => row.name),
            Record.collect((name, run) => {
                const history = Array.sort(
                    run,
                    Order.mapInput(
                        Order.string,
                        (row: BenchmarkResult) => row.timestamp,
                    ),
                );
                const recent = Array.takeRight(history, policy.window);
                const baselineHz = pipe(
                    _median(
                        Array.map(
                            Array.dropRight(history, policy.window),
                            (row) => row.hz,
                        ),
                    ),
                    Option.getOrElse(() => 0),
                );
                const recentHz = pipe(
                    _median(Array.map(recent, (row) => row.hz)),
                    Option.getOrElse(() => 0),
                );
                const noisy =
                    pipe(
                        _median(Array.map(recent, (row) => row.rme)),
                        Option.getOrElse(() => 0),
                    ) > policy.noiseCap;
                const sustained =
                    history.length >= policy.minHistory &&
                    baselineHz > 0 &&
                    Array.every(
                        recent,
                        (row) => row.hz < baselineHz * (1 - policy.tolerance),
                    );
                return {
                    name,
                    verdict: sustained
                        ? 'regression'
                        : noisy
                          ? 'noisy'
                          : 'pass',
                    baselineHz,
                    recentHz,
                } satisfies Benchmark.Report['benchmarks'][number];
            }),
            (benchmarks) => ({
                benchmarks,
                verdict: Array.isNonEmptyReadonlyArray(benchmarks)
                    ? pipe(
                          Array.max(
                              Array.map(
                                  benchmarks,
                                  (benchmark) => _SEVERITY[benchmark.verdict],
                              ),
                              Order.number,
                          ),
                          (rank): Benchmark.Verdict =>
                              rank === _SEVERITY.regression
                                  ? 'regression'
                                  : rank === _SEVERITY.noisy
                                    ? 'noisy'
                                    : 'pass',
                      )
                    : ('pass' as Benchmark.Verdict),
            }),
        ),
    importLatestResults: _importLatestResults,
    history: _readHistory,
    checkRegression: (
        policy: Benchmark.Policy = _POLICY,
    ): Effect.Effect<
        Benchmark.Report,
        BenchmarkError,
        FileSystem.FileSystem | Path.Path
    > =>
        Effect.gen(function* () {
            yield* _importLatestResults;
            const report = Benchmark.summarize(yield* _readHistory, policy);
            return yield* report.verdict === 'regression'
                ? Effect.fail(
                      new BenchmarkError({
                          reason: 'regression',
                          detail: Array.join(
                              Array.filterMap(report.benchmarks, (benchmark) =>
                                  benchmark.verdict === 'regression'
                                      ? Option.some(benchmark.name)
                                      : Option.none(),
                              ),
                              ', ',
                          ),
                      }),
                  )
                : Effect.succeed(report);
        }),
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Benchmark, BenchmarkDirectory, BenchmarkError, BenchmarkResult };
