import { Array, type Config, DateTime, Effect, LogLevel, Option, Path, Record, Struct } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { HarnessError } from '#/error';
import { starts } from '#/instance';
import { LAUNCHED, located, RECORDS, session } from '#/log';

// --- [TYPES] ---------------------------------------------------------------------------

type Clause = keyof typeof CLAUSES;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CLAUSES = {
    run: (value: string) => `run = ${value}`,
    newest: (value: string) => `run = (select max(run) from records where session is not distinct from nullif(${value}, ''))`,
    level: (value: string) => `list_contains(string_split(${value}, ','), level)`,
    case: (value: string) => `"case" = ${value}`,
    host: (value: string) => `host = ${value}`,
    live: (value: string) => `list_contains(string_split(${value}, ','), instance)`,
    launched: (value: string) => `instance in (select instance from records where message = ${value})`,
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const queried = Effect.fn('queried')(function* (root: Config.Config<string>, mode: readonly string[], statement: (where: string) => string, values: Partial<Record<Clause, Option.Option<string>>>) {
    const [path, folder] = yield* Effect.all([Path.Path, root]);
    const present = Array.getSomes(Struct.keys(values).map((key) => Option.map(values[key] ?? Option.none(), (value) => [key, `HARNESS_${key.toUpperCase()}`, value] as const)));
    const where = ['true', ...present.map(([key, variable]) => CLAUSES[key](`getenv('${variable}')`))].join(' and ');
    const selected = `with records as (
    select host, run, ordinality, json->>'session' as session, json->>'at' as "at", json->>'level' as level, json->>'message' as message, json->>'phase' as phase, json->>'state' as state,
        json->>'tag' as tag, json->>'case' as "case", json->>'version' as version, json->>'completion' as completion, json->>'commit' as commit, json->>'error' as error,
        concat_ws(' ', json->>'pid', json->>'started') as instance, json_merge_patch(json_object('host', host, 'run', run), json) as json
    from read_ndjson_objects(getenv('HARNESS_RUNS'), hive_partitioning = true, ignore_errors = true) with ordinality
    where json is not null
)`;
    const code = yield* ChildProcessSpawner.ChildProcessSpawner.use((spawner) =>
        spawner.exitCode(
            ChildProcess.make('duckdb', ['-no-agent', ...mode, '-c', `${selected} ${statement(where)}`], {
                stdin: 'ignore',
                stdout: 'inherit',
                stderr: 'inherit',
                env: { HARNESS_RUNS: path.join(located(path, folder, '*', '*'), RECORDS), ...Record.fromIterableWith(present, ([, variable, value]) => [variable, value] as const) },
                extendEnv: true,
            }),
        ),
    );
    return yield* code === 0 ? Effect.void : HarnessError.unavailable('duckdb')(`exited with code ${code}`);
});
const records = (root: Config.Config<string>, read: { readonly run: Option.Option<string>; readonly level: Option.Option<LogLevel.LogLevel>; readonly case: Option.Option<string> }) =>
    Effect.flatMap(session, (owner) =>
        queried(root, ['-list', '-noheader'], (where) => `select json from records where ${where} order by ordinality`, {
            run: read.run,
            newest: Option.match(read.run, { onNone: () => Option.some(Option.getOrElse(owner, () => '')), onSome: () => Option.none() }),
            level: Option.map(read.level, (least) => LogLevel.values.filter((held) => LogLevel.isGreaterThanOrEqualTo(held, least)).join(',')),
            case: read.case,
        }),
    );
const failures = (root: Config.Config<string>, host: Option.Option<string>) =>
    queried(
        root,
        ['-jsonlines'],
        (where) => `, started as (select run, commit from records where commit is not null),
cleared as (select host, version, "case", run, "at" from records where completion is not null and tag is null),
grouped as (
    select host, version, "case", phase, state, tag, count(*) as seen, min("at") as first, max("at") as last, arg_max(run, "at") as run, arg_max(session, "at") as session, arg_max(error, "at") as error
    from records where tag is not null and ${where} group by all
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
        { host },
    );
const instances = (root: Config.Config<string>) =>
    Effect.flatMap(starts(['-A']), (live) =>
        queried(root, ['-list', '-noheader'], (where) => `select arg_max(json, ordinality) from records where ${where} group by instance`, {
            launched: Option.some(LAUNCHED),
            live: Option.some(
                Array.fromIterable(live)
                    .map(([pid, started]) => `${pid} ${DateTime.formatIso(started)}`)
                    .join(','),
            ),
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { failures, instances, records };
