import { Array, type Config, DateTime, Effect, type Option, Path, Record, Stdio, Stream, String } from 'effect';
import { printed, starts } from '#/instance';
import { LAUNCHED, partition, RECORDS } from '#/log';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CURRENT = `host = getenv('HARNESS_HOST') and run = coalesce(
    (select max(run) from records as held where held.host = getenv('HARNESS_HOST') and held.session = getenv('CLAUDE_CODE_SESSION_ID')),
    (select max(run) from records as held where held.host = getenv('HARNESS_HOST'))
)`;

// --- [OPERATIONS] ----------------------------------------------------------------------

const queried = Effect.fn('queried')(function* (root: Config.Config<string>, mode: readonly string[], statement: string, values: Record.ReadonlyRecord<string, string>) {
    const [path, folder] = yield* Effect.all([Path.Path, root]);
    const selected = `with records as (
    select host, run, ordinality, json->>'session' as session, json->>'at' as "at", json->>'level' as level, json->>'message' as message, json->>'phase' as phase, json->>'state' as state,
        json->>'tag' as tag, json->>'case' as "case", json->>'completion' as completion, json->>'version' as version, json->>'proof' as proof, json->>'app' as app, json->>'commit' as commit, json->>'error' as error,
        cast(json->>'pid' as integer) as pid, json->>'started' as started, concat_ws(' ', json->>'pid', json->>'started') as instance, json_merge_patch(json_object('host', host, 'run', run), json) as json
    from read_ndjson_objects(getenv('HARNESS_RUNS'), hive_partitioning = true, ignore_errors = true) with ordinality
    where json is not null
)`;
    return yield* printed('duckdb', ['-no-agent', ...mode, '-c', `${selected} ${statement}`], { HARNESS_RUNS: path.join(partition(path, folder, '*', '*'), RECORDS), ...Record.mapKeys(values, (key) => `HARNESS_${key.toUpperCase()}`) });
});
const shown = <E, R>(text: Effect.Effect<string, E, R>) => Effect.flatMap(Effect.all([text, Stdio.Stdio]), ([held, stdio]) => Stream.run(Stream.make(held), stdio.stdout()));
const records = (root: Config.Config<string>, host: string) => shown(queried(root, ['-list', '-noheader'], `select json from records where ${CURRENT} order by ordinality`, { host }));
const launched = Effect.fn('launched')(function* (root: Config.Config<string>, host: string) {
    const text = yield* queried(root, ['-jsonlines'], `select run, pid, started, max(app) as app, max(version) as version, max("at") as ended from records where ${CURRENT} and started is not null group by run, pid, started having bool_or(message = getenv('HARNESS_LAUNCHED'))`, { host, launched: LAUNCHED });
    return Array.fromIterable(String.linesIterator(text)).filter(String.isNonEmpty);
});
const failures = (root: Config.Config<string>, host: Option.Option<string>) =>
    shown(
        queried(
            root,
            ['-jsonlines'],
            `, started as (select run, commit, session from records where commit is not null),
cleared as (select host, version, proof, "case", run, "at" from records where completion is not null and tag is null),
grouped as (
    select host, version, proof, "case", phase, state, tag, count(*) as seen, min("at") as first, max("at") as last, arg_max(run, "at") as run, arg_max(error, "at") as error
    from records where tag is not null and (getenv('HARNESS_HOST') = '' or host = getenv('HARNESS_HOST')) group by all
)
select grouped.*, started.commit, started.session, fixed.run as fixed, fixed."at" as "fixedAt", after.commit as "fixedCommit"
from grouped
left join started on started.run = grouped.run
left join lateral (
    select run, "at" from cleared
    where cleared.host = grouped.host and cleared.version is not distinct from grouped.version and cleared.proof is not distinct from grouped.proof and (grouped."case" is null or cleared."case" = grouped."case") and cleared."at" > grouped.last
    order by cleared."at" limit 1
) as fixed on true
left join started as after on after.run = fixed.run
order by grouped.last desc`,
            Record.getSomes({ host }),
        ),
    );
const instances = (root: Config.Config<string>) =>
    Effect.flatMap(starts(['-A']), (live) =>
        shown(
            queried(root, ['-list', '-noheader'], `select arg_max(json, "at") from records where instance in (select instance from records where message = getenv('HARNESS_LAUNCHED')) and list_contains(string_split(getenv('HARNESS_LIVE'), ','), instance) group by instance`, {
                launched: LAUNCHED,
                live: Array.fromIterable(live)
                    .map(([pid, started]) => `${pid} ${DateTime.formatIso(started)}`)
                    .join(','),
            }),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { failures, instances, launched, records };
