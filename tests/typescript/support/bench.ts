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

const _decodeLatest = Schema.Struct({
    name: Schema.String,
    throughput: Schema.Struct({ mean: Schema.Number, rme: Schema.Number }),
    fromStore: Schema.optional(Schema.Boolean),
}).pipe(
    (task) => Schema.Struct({ name: Schema.String, tasks: Schema.Array(task) }),
    (benchmark) => Schema.Struct({ benchmarks: Schema.Array(benchmark) }),
    (assertion) => Schema.Struct({ assertionResults: Schema.Array(assertion) }),
    (result) => Schema.Struct({ startTime: Schema.DateTimeUtcFromNumber, testResults: Schema.Array(result) }),
    (report) => Schema.decodeUnknown(Schema.parseJson(report), { errors: 'all' }),
);
const _decodeResult = Schema.decodeUnknown(Schema.parseJson(BenchmarkResult));
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

const _importLatestResults: Effect.Effect<readonly BenchmarkResult[], BenchmarkError, BenchmarkFiles> = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const directory = yield* BenchmarkDirectory;
    const target = path.join(directory, _FILES.latest);
    const raw = yield* Effect.mapError(fs.readFileString(target), _fileError('unreadable'));
    const latest = yield* Effect.mapError(_decodeLatest(raw), _fileError('malformed'));
    const timestamp = DateTime.formatIso(latest.startTime);
    const existing = yield* _readHistory;
    if (Array.some(existing, (row) => row.timestamp === timestamp)) {
        return [];
    }
    const rows: readonly BenchmarkResult[] = pipe(
        latest.testResults,
        Array.flatMap((result) => result.assertionResults),
        Array.flatMap((result) => result.benchmarks),
        Array.flatMap((benchmark) =>
            Array.filterMap(benchmark.tasks, (task) =>
                task.fromStore
                    ? Option.none()
                    : Option.some({ timestamp, name: `${benchmark.name}::${task.name}`, hz: task.throughput.mean, rme: task.throughput.rme }),
            ),
        ),
    );
    const lines = Array.map(rows, (row) => JSON.stringify(row));
    yield* Effect.mapError(
        fs.writeFileString(path.join(directory, _FILES.history), `${lines.join('\n')}\n`, { flag: 'a' }),
        _fileError('unreadable'),
    );
    return rows;
});

const Benchmark: Benchmark = {
    policy: _POLICY,
    summarize: (rows, policy = _POLICY) => {
        const benchmarks = Record.collect(
            Array.groupBy(rows, (row) => row.name),
            (name, run): BenchmarkSummary => {
                const history = Array.sort(run, _byTimestamp);
                const recent = Array.takeRight(history, policy.window);
                const baselineHz = _median(Array.map(Array.dropRight(history, policy.window), (row) => row.hz));
                const verdict: BenchmarkVerdict = _median(Array.map(recent, (row) => row.rme)) > policy.noiseCap ? 'noisy' : 'pass';
                const sustained =
                    history.length >= policy.minHistory &&
                    baselineHz > 0 &&
                    Array.every(recent, (row) => row.hz < baselineHz * (1 - policy.tolerance));
                return {
                    name,
                    verdict: sustained ? 'regression' : verdict,
                    baselineHz,
                    recentHz: _median(Array.map(recent, (row) => row.hz)),
                };
            },
        );
        return {
            benchmarks,
            verdict: Array.reduce(benchmarks, 'pass' as BenchmarkVerdict, (verdict, summary) => Order.max(_bySeverity)(verdict, summary.verdict)),
        };
    },
    importLatestResults: _importLatestResults,
    history: _readHistory,
    checkRegression: Effect.fnUntraced(function* (policy = _POLICY) {
        yield* _importLatestResults;
        const report = Benchmark.summarize(yield* _readHistory, policy);
        if (report.verdict !== 'regression') {
            return report;
        }
        const regressions = Array.filterMap(report.benchmarks, (summary) =>
            summary.verdict === 'regression' ? Option.some(summary.name) : Option.none(),
        );
        return yield* new BenchmarkError({ reason: 'regression', detail: Array.join(regressions, ', ') });
    }),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Benchmark, BenchmarkDirectory, BenchmarkError, type BenchmarkPolicy, type BenchmarkReport, BenchmarkResult, type BenchmarkVerdict };
