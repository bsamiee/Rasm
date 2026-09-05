import { FileSystem, Path } from '@effect/platform';
import { Array, Context, Data, DateTime, Effect, Option, Order, pipe, Record, Schema, String } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type BenchmarkVerdict = 'pass' | 'noisy' | 'regression';
type BenchmarkDirectory = 'test-support/BenchmarkDirectory';

interface BenchmarkPolicy {
    readonly window: number;
    readonly tolerance: number;
    readonly noiseCap: number;
    readonly minHistory: number;
}

interface BenchmarkResult {
    readonly timestamp: string;
    readonly name: string;
    readonly hz: number;
    readonly rme: number;
}

interface BenchmarkSummary {
    readonly name: string;
    readonly verdict: BenchmarkVerdict;
    readonly baselineHz: number;
    readonly recentHz: number;
}

interface BenchmarkReport {
    readonly verdict: BenchmarkVerdict;
    readonly benchmarks: readonly BenchmarkSummary[];
}

type BenchmarkFiles = FileSystem.FileSystem | Path.Path | BenchmarkDirectory;

interface Benchmark {
    readonly policy: BenchmarkPolicy;
    readonly summarize: (rows: readonly BenchmarkResult[], policy?: BenchmarkPolicy) => BenchmarkReport;
    readonly importLatestResults: Effect.Effect<readonly BenchmarkResult[], BenchmarkError, BenchmarkFiles>;
    readonly history: Effect.Effect<readonly BenchmarkResult[], BenchmarkError, BenchmarkFiles>;
    readonly checkRegression: (policy?: BenchmarkPolicy) => Effect.Effect<BenchmarkReport, BenchmarkError, BenchmarkFiles>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _POLICY: BenchmarkPolicy = { window: 3, tolerance: 0.15, noiseCap: 10, minHistory: 5 };
const _FILES = { latest: 'latest.json', history: 'history.ndjson' } as const;
const _SEVERITY: Record<BenchmarkVerdict, number> = { pass: 0, noisy: 1, regression: 2 };

// --- [MODELS] --------------------------------------------------------------------------

const BenchmarkResult: Schema.Schema<BenchmarkResult> = Schema.Struct({
    timestamp: Schema.String,
    name: Schema.NonEmptyString,
    hz: Schema.Number,
    rme: Schema.Number,
});

// Shape Vitest writes through its outputJson benchmark option
const _Latest = Schema.Struct({
    files: Schema.Array(
        Schema.Struct({
            filepath: Schema.String,
            groups: Schema.Array(
                Schema.Struct({
                    fullName: Schema.String,
                    benchmarks: Schema.Array(Schema.Struct({ name: Schema.String, hz: Schema.Number, rme: Schema.Number })),
                }),
            ),
        }),
    ),
});

// --- [ERRORS] --------------------------------------------------------------------------

class BenchmarkError extends Data.Error<{
    readonly reason: 'regression' | 'malformed' | 'unreadable';
    readonly detail: string;
}> {
    readonly _tag = 'BenchmarkError' as const;
}

// --- [SERVICES] ------------------------------------------------------------------------

const BenchmarkDirectory: Context.Tag<BenchmarkDirectory, string> = Context.GenericTag<BenchmarkDirectory, string>('test-support/BenchmarkDirectory');

// --- [OPERATIONS] ----------------------------------------------------------------------

const _decodeLatest = Schema.decodeUnknown(Schema.parseJson(_Latest), { errors: 'all' });
const _decodeResult = Schema.decodeUnknown(Schema.parseJson(BenchmarkResult));
const _encodeResult = Schema.encode(Schema.parseJson(BenchmarkResult));
const _byTimestamp = Order.mapInput(Order.string, (row: BenchmarkResult) => row.timestamp);
const _bySeverity = Order.mapInput(Order.number, (verdict: BenchmarkVerdict) => _SEVERITY[verdict]);

const _fileError =
    (reason: 'malformed' | 'unreadable') =>
    (error: { readonly message: string }): BenchmarkError =>
        new BenchmarkError({ reason, detail: error.message });

const _median = (values: readonly number[]): number =>
    pipe(
        Array.sort(values, Order.number),
        (sorted) => Array.get(sorted, Math.floor(sorted.length / 2)),
        Option.getOrElse(() => 0),
    );

const _verdict = (sustained: boolean, noisy: boolean): BenchmarkVerdict => {
    if (sustained) {
        return 'regression';
    }
    if (noisy) {
        return 'noisy';
    }
    return 'pass';
};

const _readHistory: Effect.Effect<readonly BenchmarkResult[], BenchmarkError, BenchmarkFiles> = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const directory = yield* BenchmarkDirectory;
    const raw = yield* Effect.orElseSucceed(fs.readFileString(path.join(directory, _FILES.history)), () => '');
    const lines = Array.filter(String.split(raw, '\n'), String.isNonEmpty);
    return yield* Effect.mapError(
        Effect.forEach(lines, (line) => _decodeResult(line)),
        _fileError('malformed'),
    );
});

// The rows of one Vitest output file append to the history once, a rerun over the same file adds nothing
const _importLatestResults: Effect.Effect<readonly BenchmarkResult[], BenchmarkError, BenchmarkFiles> = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const directory = yield* BenchmarkDirectory;
    const target = path.join(directory, _FILES.latest);
    const raw = yield* Effect.mapError(fs.readFileString(target), _fileError('unreadable'));
    const info = yield* Effect.mapError(fs.stat(target), _fileError('unreadable'));
    const timestamp = yield* Option.match(info.mtime, {
        onNone: () => Effect.map(DateTime.now, DateTime.formatIso),
        onSome: (modifiedAt) => Effect.succeed(DateTime.formatIso(DateTime.unsafeFromDate(modifiedAt))),
    });
    const latest = yield* Effect.mapError(_decodeLatest(raw), _fileError('malformed'));
    const existing = yield* _readHistory;
    const rows: readonly BenchmarkResult[] = Array.flatMap(latest.files, (file) =>
        Array.flatMap(file.groups, (group) =>
            Array.map(group.benchmarks, (entry) => ({ timestamp, name: `${group.fullName}::${entry.name}`, hz: entry.hz, rme: entry.rme })),
        ),
    );
    return yield* Effect.if(
        Array.some(existing, (row) => row.timestamp === timestamp),
        {
            onTrue: () => Effect.succeed([]),
            onFalse: () =>
                Effect.gen(function* () {
                    const lines = yield* Effect.orDie(Effect.forEach(rows, (row) => _encodeResult(row)));
                    yield* Effect.orDie(fs.makeDirectory(directory, { recursive: true }));
                    yield* Effect.mapError(
                        fs.writeFileString(path.join(directory, _FILES.history), `${lines.join('\n')}\n`, { flag: 'a' }),
                        _fileError('unreadable'),
                    );
                    return rows;
                }),
        },
    );
});

const _summarize = (name: string, run: readonly BenchmarkResult[], policy: BenchmarkPolicy): BenchmarkSummary => {
    const history = Array.sort(run, _byTimestamp);
    const recent = Array.takeRight(history, policy.window);
    const baselineHz = _median(Array.map(Array.dropRight(history, policy.window), (row) => row.hz));
    const noisy = _median(Array.map(recent, (row) => row.rme)) > policy.noiseCap;
    const sustained =
        history.length >= policy.minHistory && baselineHz > 0 && Array.every(recent, (row) => row.hz < baselineHz * (1 - policy.tolerance));
    return { name, verdict: _verdict(sustained, noisy), baselineHz, recentHz: _median(Array.map(recent, (row) => row.hz)) };
};

const Benchmark: Benchmark = {
    policy: _POLICY,
    summarize: (rows, policy = _POLICY) =>
        pipe(
            Array.groupBy(rows, (row) => row.name),
            Record.collect((name, run) => _summarize(name, run, policy)),
            (benchmarks) => ({
                benchmarks,
                verdict: Array.match(benchmarks, {
                    onEmpty: (): BenchmarkVerdict => 'pass',
                    onNonEmpty: (summaries) =>
                        Array.max(
                            Array.map(summaries, (summary) => summary.verdict),
                            _bySeverity,
                        ),
                }),
            }),
        ),
    importLatestResults: _importLatestResults,
    history: _readHistory,
    checkRegression: (policy = _POLICY) =>
        Effect.gen(function* () {
            yield* _importLatestResults;
            const report = Benchmark.summarize(yield* _readHistory, policy);
            const regressions = Array.filterMap(report.benchmarks, (summary) =>
                Option.liftPredicate(summary.name, () => summary.verdict === 'regression'),
            );
            return yield* Effect.filterOrFail(
                Effect.succeed(report),
                (held) => held.verdict !== 'regression',
                () => new BenchmarkError({ reason: 'regression', detail: Array.join(regressions, ', ') }),
            );
        }),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Benchmark, BenchmarkDirectory, BenchmarkError, type BenchmarkPolicy, type BenchmarkReport, BenchmarkResult, type BenchmarkVerdict };
