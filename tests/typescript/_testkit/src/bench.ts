import { fileURLToPath } from 'node:url';
import { FileSystem, Path } from '@effect/platform';
import { Array, Context, Data, DateTime, Effect, Option, Order, pipe, Record, Schema, String } from 'effect';

// --- [TYPES] -----------------------------------------------------------------------------

declare namespace Bench {
    type Policy = typeof _POLICY;
    type Verdict = 'pass' | 'noisy' | 'breach';
    type Receipt = { readonly name: string; readonly verdict: Verdict; readonly baselineHz: number; readonly recentHz: number };
    type Report = { readonly verdict: Verdict; readonly receipts: ReadonlyArray<Receipt> };
}

// --- [CONSTANTS] -------------------------------------------------------------------------

// Sustained-regression policy: a breach needs `window` consecutive slow runs — a single spike never trips the gate.
const _POLICY = { window: 3, tolerance: 0.15, noiseCap: 10, minHistory: 5 } as const;

const _LEDGER = { latest: 'latest.json', history: 'history.ndjson' } as const;
const _RANK = { pass: 0, noisy: 1, breach: 2 } as const satisfies Record<Bench.Verdict, number>;

// --- [MODELS] ----------------------------------------------------------------------------

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
                    benchmarks: Schema.Array(Schema.Struct({ name: Schema.String, hz: Schema.Number, rme: Schema.Number })),
                }),
            ),
        }),
    ),
});

// --- [ERRORS] ----------------------------------------------------------------------------

class BenchFault extends Data.TaggedError('BenchFault')<{
    readonly reason: 'breach' | 'malformed' | 'unreadable';
    readonly detail: string;
}> {}

// --- [SERVICES] --------------------------------------------------------------------------

class BenchHome extends Context.Reference<BenchHome>()('rasm-testkit/BenchHome', {
    defaultValue: (): string => fileURLToPath(new URL('../../../../.artifacts/typescript/bench', import.meta.url)),
}) {}

// --- [OPERATIONS] ------------------------------------------------------------------------

const _decodeLatest = Schema.decodeUnknown(Schema.parseJson(_Latest), { errors: 'all' });
const _decodeRow = Schema.decodeUnknown(Schema.parseJson(BenchRow));
const _encodeRow = Schema.encode(Schema.parseJson(BenchRow));

const _median = (values: ReadonlyArray<number>): Option.Option<number> =>
    pipe(Array.sort(values, Order.number), (sorted) => Array.get(sorted, Math.floor(sorted.length / 2)));

// A missing ledger is an empty history; a corrupted line is a typed malformed fault, never a die.
const _history: Effect.Effect<ReadonlyArray<BenchRow>, BenchFault, FileSystem.FileSystem | Path.Path> = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const home = yield* BenchHome;
    const raw = yield* Effect.orElseSucceed(fs.readFileString(path.join(home, _LEDGER.history)), () => '');
    return yield* Effect.mapError(
        Effect.forEach(Array.filter(String.split(raw, '\n'), String.isNonEmpty), (line) => _decodeRow(line)),
        (fault) => new BenchFault({ reason: 'malformed', detail: fault.message }),
    );
});

// Harvest keys on the autosave's own mtime: re-running the gate over one bench run appends nothing — a duplicated
// harvest would forge the consecutive-slow-run evidence the breach verdict reads.
const _harvest: Effect.Effect<ReadonlyArray<BenchRow>, BenchFault, FileSystem.FileSystem | Path.Path> = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const home = yield* BenchHome;
    const target = path.join(home, _LEDGER.latest);
    const raw = yield* Effect.mapError(fs.readFileString(target), (fault) => new BenchFault({ reason: 'unreadable', detail: fault.message }));
    const info = yield* Effect.mapError(fs.stat(target), (fault) => new BenchFault({ reason: 'unreadable', detail: fault.message }));
    const at = yield* Option.match(info.mtime, {
        onNone: () => Effect.map(DateTime.now, DateTime.formatIso),
        onSome: (minted) => Effect.succeed(DateTime.formatIso(DateTime.unsafeFromDate(minted))),
    });
    const latest = yield* Effect.mapError(_decodeLatest(raw), (fault) => new BenchFault({ reason: 'malformed', detail: fault.message }));
    const seen = yield* _history;
    const rows = Array.flatMap(latest.files, (file) =>
        Array.flatMap(file.groups, (group) =>
            Array.map(
                group.benchmarks,
                // The group fullName already carries the repo-relative spec path; an absolute filepath key couples the ledger to one machine.
                (entry) => new BenchRow({ at, name: `${group.fullName}::${entry.name}`, hz: entry.hz, rme: entry.rme }),
            ),
        ),
    );
    return yield* Array.some(seen, (row) => row.at === at)
        ? Effect.succeed<ReadonlyArray<BenchRow>>([])
        : Effect.gen(function* () {
              const lines = yield* Effect.orDie(Effect.forEach(rows, (row) => _encodeRow(row)));
              yield* Effect.orDie(fs.makeDirectory(home, { recursive: true }));
              yield* Effect.mapError(
                  fs.writeFileString(path.join(home, _LEDGER.history), `${lines.join('\n')}\n`, { flag: 'a' }),
                  (fault) => new BenchFault({ reason: 'unreadable', detail: fault.message }),
              );
              return rows;
          });
});

const _receipt = (name: string, rows: ReadonlyArray<BenchRow>, policy: Bench.Policy): Bench.Receipt => {
    const recent = Array.takeRight(rows, policy.window);
    const baseline = pipe(
        _median(Array.map(Array.dropRight(rows, policy.window), (row) => row.hz)),
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
    const sustained = rows.length >= policy.minHistory && baseline > 0 && Array.every(recent, (row) => row.hz < baseline * (1 - policy.tolerance));
    return { name, verdict: sustained ? 'breach' : noisy ? 'noisy' : 'pass', baselineHz: baseline, recentHz };
};

const Bench = {
    POLICY: _POLICY,
    // Pure sustained-regression fold over the accumulated ledger — the falsifiable core the gate and the specs share.
    fold: (rows: ReadonlyArray<BenchRow>, policy: Bench.Policy = _POLICY): Bench.Report =>
        pipe(
            Array.groupBy(rows, (row) => row.name),
            Record.collect((name, run) =>
                _receipt(
                    name,
                    Array.sort(
                        run,
                        Order.mapInput(Order.string, (row: BenchRow) => row.at),
                    ),
                    policy,
                ),
            ),
            (receipts) => ({
                receipts,
                verdict: Array.isNonEmptyReadonlyArray(receipts)
                    ? pipe(
                          Array.max(
                              Array.map(receipts, (receipt) => _RANK[receipt.verdict]),
                              Order.number,
                          ),
                          (rank): Bench.Verdict => (rank === _RANK.breach ? 'breach' : rank === _RANK.noisy ? 'noisy' : 'pass'),
                      )
                    : ('pass' as Bench.Verdict),
            }),
        ),
    harvest: _harvest,
    history: _history,
    // The gate verb: harvest the latest run, fold the full ledger, and FAIL typed on a sustained breach.
    gate: (policy: Bench.Policy = _POLICY): Effect.Effect<Bench.Report, BenchFault, FileSystem.FileSystem | Path.Path> =>
        Effect.gen(function* () {
            yield* _harvest;
            const report = Bench.fold(yield* _history, policy);
            return yield* report.verdict === 'breach'
                ? Effect.fail(
                      new BenchFault({
                          reason: 'breach',
                          detail: Array.join(
                              Array.filterMap(report.receipts, (receipt) =>
                                  receipt.verdict === 'breach' ? Option.some(receipt.name) : Option.none(),
                              ),
                              ', ',
                          ),
                      }),
                  )
                : Effect.succeed(report);
        }),
} as const;

// --- [EXPORTS] ---------------------------------------------------------------------------

export { Bench, BenchFault, BenchHome, BenchRow };
