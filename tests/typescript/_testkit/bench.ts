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

declare namespace Bench {
    type Policy = typeof _POLICY;
    type Verdict = 'pass' | 'noisy' | 'breach';
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

const _LEDGER = { latest: 'latest.json', history: 'history.ndjson' } as const;
const _RANK = { pass: 0, noisy: 1, breach: 2 } as const satisfies Record<
    Bench.Verdict,
    number
>;

// --- [MODELS] --------------------------------------------------------------------------

class BenchRow extends Schema.Class<BenchRow>('BenchRow')({
    at: Schema.String,
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

class BenchFault extends Data.TaggedError('BenchFault')<{
    readonly reason: 'breach' | 'malformed' | 'unreadable';
    readonly detail: string;
}> {}

// --- [SERVICES] ------------------------------------------------------------------------

class BenchHome extends Context.Reference<BenchHome>()(
    'rasm-testkit/BenchHome',
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
const _decodeRow = Schema.decodeUnknown(Schema.parseJson(BenchRow));
const _encodeRow = Schema.encode(Schema.parseJson(BenchRow));

const _median = (values: ReadonlyArray<number>): Option.Option<number> =>
    pipe(Array.sort(values, Order.number), (sorted) =>
        Array.get(sorted, Math.floor(sorted.length / 2)),
    );

const _history: Effect.Effect<
    ReadonlyArray<BenchRow>,
    BenchFault,
    FileSystem.FileSystem | Path.Path
> = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const home = yield* BenchHome;
    const raw = yield* Effect.orElseSucceed(
        fs.readFileString(path.join(home, _LEDGER.history)),
        () => '',
    );
    return yield* Effect.mapError(
        Effect.forEach(
            Array.filter(String.split(raw, '\n'), String.isNonEmpty),
            (line) => _decodeRow(line),
        ),
        (fault) =>
            new BenchFault({ reason: 'malformed', detail: fault.message }),
    );
});

const _harvest: Effect.Effect<
    ReadonlyArray<BenchRow>,
    BenchFault,
    FileSystem.FileSystem | Path.Path
> = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const home = yield* BenchHome;
    const target = path.join(home, _LEDGER.latest);
    const raw = yield* Effect.mapError(
        fs.readFileString(target),
        (fault) =>
            new BenchFault({ reason: 'unreadable', detail: fault.message }),
    );
    const info = yield* Effect.mapError(
        fs.stat(target),
        (fault) =>
            new BenchFault({ reason: 'unreadable', detail: fault.message }),
    );
    const at = yield* Option.match(info.mtime, {
        onNone: () => Effect.map(DateTime.now, DateTime.formatIso),
        onSome: (minted) =>
            Effect.succeed(DateTime.formatIso(DateTime.unsafeFromDate(minted))),
    });
    const latest = yield* Effect.mapError(
        _decodeLatest(raw),
        (fault) =>
            new BenchFault({ reason: 'malformed', detail: fault.message }),
    );
    const seen = yield* _history;
    const rows = Array.flatMap(latest.files, (file) =>
        Array.flatMap(file.groups, (group) =>
            Array.map(
                group.benchmarks,
                (entry) =>
                    new BenchRow({
                        at,
                        name: `${group.fullName}::${entry.name}`,
                        hz: entry.hz,
                        rme: entry.rme,
                    }),
            ),
        ),
    );
    return yield* Array.some(seen, (row) => row.at === at)
        ? Effect.succeed<ReadonlyArray<BenchRow>>([])
        : Effect.gen(function* () {
              const lines = yield* Effect.orDie(
                  Effect.forEach(rows, (row) => _encodeRow(row)),
              );
              yield* Effect.orDie(fs.makeDirectory(home, { recursive: true }));
              yield* Effect.mapError(
                  fs.writeFileString(
                      path.join(home, _LEDGER.history),
                      `${lines.join('\n')}\n`,
                      { flag: 'a' },
                  ),
                  (fault) =>
                      new BenchFault({
                          reason: 'unreadable',
                          detail: fault.message,
                      }),
              );
              return rows;
          });
});

const Bench = {
    POLICY: _POLICY,
    fold: (
        rows: ReadonlyArray<BenchRow>,
        policy: Bench.Policy = _POLICY,
    ): Bench.Report =>
        pipe(
            Array.groupBy(rows, (row) => row.name),
            Record.collect((name, run) => {
                const history = Array.sort(
                    run,
                    Order.mapInput(Order.string, (row: BenchRow) => row.at),
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
                    verdict: sustained ? 'breach' : noisy ? 'noisy' : 'pass',
                    baselineHz,
                    recentHz,
                } satisfies Bench.Report['benchmarks'][number];
            }),
            (benchmarks) => ({
                benchmarks,
                verdict: Array.isNonEmptyReadonlyArray(benchmarks)
                    ? pipe(
                          Array.max(
                              Array.map(
                                  benchmarks,
                                  (benchmark) => _RANK[benchmark.verdict],
                              ),
                              Order.number,
                          ),
                          (rank): Bench.Verdict =>
                              rank === _RANK.breach
                                  ? 'breach'
                                  : rank === _RANK.noisy
                                    ? 'noisy'
                                    : 'pass',
                      )
                    : ('pass' as Bench.Verdict),
            }),
        ),
    harvest: _harvest,
    history: _history,
    gate: (
        policy: Bench.Policy = _POLICY,
    ): Effect.Effect<
        Bench.Report,
        BenchFault,
        FileSystem.FileSystem | Path.Path
    > =>
        Effect.gen(function* () {
            yield* _harvest;
            const report = Bench.fold(yield* _history, policy);
            return yield* report.verdict === 'breach'
                ? Effect.fail(
                      new BenchFault({
                          reason: 'breach',
                          detail: Array.join(
                              Array.filterMap(report.benchmarks, (benchmark) =>
                                  benchmark.verdict === 'breach'
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

export { Bench, BenchFault, BenchHome, BenchRow };
