import { Array, type Config, Console, DateTime, type Duration, Effect, FileSystem, LogLevel, Option, Path, pipe, Record, String, Struct } from 'effect';
import { ChildProcess } from 'effect/process';
import { HarnessError } from '#/error';
import { alive, INSTANCES, LINES, RUNS, session } from '#/log';

// --- [TYPES] ---------------------------------------------------------------------------

type Clause = keyof typeof CLAUSES;
interface Selection {
    readonly run: Option.Option<string>;
    readonly host: Option.Option<string>;
    readonly since: Option.Option<Duration.Duration>;
    readonly level: Option.Option<LogLevel.LogLevel>;
    readonly phase: Option.Option<string>;
    readonly case: Option.Option<string>;
}
interface Signed {
    readonly host: Option.Option<string>;
    readonly version: Option.Option<string>;
    readonly case: Option.Option<string>;
    readonly signature: Option.Option<string>;
    readonly since: Option.Option<Duration.Duration>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const RECORDS = `with records as (
    select host, json->>'run' as run, json->>'session' as session, json->>'at' as "at", json->>'level' as level, json->>'phase' as phase, json->>'case' as "case",
        json->>'version' as version, json->>'signature' as signature, json->>'completion' as completion, json->>'commit' as commit, json->>'error' as error, json
    from read_ndjson_objects(getenv('HARNESS_RUNS'), hive_partitioning = true)
)`;
const CLAUSES = {
    run: `run = getenv('HARNESS_RUN')`,
    newest: `run = (select max(run) from records where session = getenv('HARNESS_NEWEST') and host = coalesce(nullif(getenv('HARNESS_HOST'), ''), host))`,
    host: `host = getenv('HARNESS_HOST')`,
    since: `"at" >= getenv('HARNESS_SINCE')`,
    level: `list_contains(string_split(getenv('HARNESS_LEVEL'), ','), level)`,
    phase: `phase = getenv('HARNESS_PHASE')`,
    case: `"case" = getenv('HARNESS_CASE')`,
    version: `version = getenv('HARNESS_VERSION')`,
    signature: `signature = getenv('HARNESS_SIGNATURE')`,
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const cutoff = (since: Option.Option<Duration.Duration>) => Effect.map(DateTime.now, (now) => Option.map(since, (span) => DateTime.formatIso(DateTime.subtractDuration(now, span))));
const queried = Effect.fn('queried')(function* <K extends Clause>(root: Config.Config<string>, mode: readonly string[], statement: (where: string) => string, values: Record.ReadonlyRecord<K, Option.Option<string>>) {
    const [path, folder] = yield* Effect.all([Path.Path, root]);
    const present = Array.getSomes(Struct.keys(values).map((key) => Option.map(values[key], (value) => [key, value] as const)));
    const where = Array.match(
        present.map(([key]) => CLAUSES[key]),
        { onEmpty: () => 'true', onNonEmpty: (held) => held.join(' and ') },
    );
    const handle = yield* ChildProcess.make('duckdb', ['-no-agent', ...mode, '-c', statement(where)], {
        stdin: 'ignore',
        stdout: 'inherit',
        stderr: 'inherit',
        env: { HARNESS_RUNS: path.join(folder, RUNS, '*', `*${LINES}`), ...Record.fromIterableWith(present, ([key, value]) => [`HARNESS_${key.toUpperCase()}`, value] as const) },
        extendEnv: true,
    });
    const code = yield* handle.exitCode;
    return yield* code === 0 ? Effect.void : HarnessError.unavailable('duckdb')(`exited with code ${code}`);
}, Effect.scoped);
const records = Effect.fn('records')(function* (root: Config.Config<string>, selection: Selection) {
    const [owner, since] = yield* Effect.all([session, cutoff(selection.since)]);
    yield* queried(root, ['-list', '-noheader'], (where) => `${RECORDS} select json from records where ${where}`, {
        run: selection.run,
        newest: Option.liftPredicate(owner, () => Option.isNone(selection.run) && Option.isNone(selection.since)),
        host: selection.host,
        since,
        level: Option.map(selection.level, (least) => LogLevel.values.filter((held) => LogLevel.isGreaterThanOrEqualTo(held, least)).join(',')),
        phase: selection.phase,
        case: selection.case,
    });
});
const failures = Effect.fn('failures')(function* (root: Config.Config<string>, signed: Signed) {
    const since = yield* cutoff(signed.since);
    yield* queried(
        root,
        ['-jsonlines'],
        (where) => `${RECORDS},
started as (select run, commit from records where commit is not null),
cleared as (select host, version, "case", run, "at" from records where completion in ('Ran', 'Exited')),
grouped as (
    select host, version, "case", signature, count(*) as seen, min("at") as first, max("at") as last, arg_max(run, "at") as run, arg_max(session, "at") as session, arg_max(error, "at") as error
    from records where signature is not null and ${where} group by all
)
select grouped.*, started.commit, fixed.run as fixed, fixed."at" as "fixedAt", after.commit as "fixedCommit"
from grouped
left join started on started.run = grouped.run
left join lateral (
    select run, "at" from cleared
    where cleared.host = grouped.host and cleared.version is not distinct from grouped.version and (grouped."case" is null or cleared."case" = grouped."case") and cleared."at" > grouped.last
    order by cleared."at" limit 1
) as fixed on true
left join started as after on after.run = fixed.run
order by grouped.last desc`,
        { host: signed.host, version: signed.version, case: signed.case, signature: signed.signature, since },
    );
});
const instances = Effect.fn('instances')(function* (root: Config.Config<string>, host: Option.Option<string>) {
    const [fs, path, folder, live] = yield* Effect.all([FileSystem.FileSystem, Path.Path, root, alive]);
    const files = (yield* fs.glob(`${INSTANCES}/${Option.match(host, { onNone: () => '*', onSome: (held) => `host=${held}` })}/*${LINES}`, { root: folder })).filter((file) => live.has(path.basename(file, LINES)));
    const latest = (text: string) => pipe(text, String.trimEnd, String.split('\n'), Array.lastNonEmpty, Option.liftPredicate(String.isNonEmpty));
    yield* Effect.forEach(files, (file) => Effect.flatMap(fs.readFileString(path.join(folder, file)), (text) => Option.match(latest(text), { onNone: () => Effect.void, onSome: Console.log })), { discard: true });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { failures, instances, records };
