import { Array, Console, DateTime, Effect, FileSystem, Formatter, Option, Order, Path, Predicate, Record, Schema, Struct } from 'effect';
import { HarnessError } from '#/harness-error';
import { printedLines } from '#/instance';
import { Field, partition, RECORDS, Signed } from '#/log';
import { KINDS, type Linked, system } from '#/report';

// --- [TYPES] ---------------------------------------------------------------------------

interface Printed {
    readonly seen: typeof Read.Type;
    readonly line: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TEXT = 'VARCHAR';
const SIGNED = Record.map(Signed.fields, () => TEXT);
const SINCE = `with records as (select * from read_ndjson(getenv('HARNESS_RUNS'), columns = {${[...Record.keys(SIGNED), 'level', 'completion'].map((key) => `'${key}': '${TEXT}'`).join(', ')}}, hive_partitioning = true, ignore_errors = true)),
signed as (select row_number() over () as signature, * from (select unnest(from_json(getenv('HARNESS_SIGNATURES'), getenv('HARNESS_STRUCTURE')), recursive := true))),
failing as (
    select signed.signature, records.run from signed join records on records.level <> 'Info' and coalesce(records.tag, records.completion) = signed.tag and records.phase = signed.phase
    and records.version is not distinct from signed.version and records.proof is not distinct from signed.proof and records."case" is not distinct from signed."case"
),
cleared as (select run, version, proof, "case" from records where completion is not null and level = 'Info'),
opened as (select signed.*, min(failing.run) as since from signed left join failing using (signature) group by all)
select opened.* exclude (signature), fixed.run as fixed from opened
left join lateral (
    select run from cleared
    where cleared.version is not distinct from opened.version and cleared.proof is not distinct from opened.proof and (opened."case" is null or cleared."case" = opened."case")
    and cleared.run > opened.since and cleared.run not in (select run from failing where failing.signature = opened.signature)
    order by run limit 1
) as fixed on true`;

// --- [MODELS] --------------------------------------------------------------------------

const Read = Schema.fromJsonString(Schema.Struct({ at: Schema.DateTimeUtcFromString, message: Schema.String, app: Schema.optionalKey(Schema.String), ...Struct.pick(Field, ['pid', 'started', 'kind', 'state', 'file']) })).annotate({
    description: 'Record fields reading a run locates its instances and their linked reports by',
});
const Dated = Schema.fromJsonString(Schema.Record(Schema.String, Schema.NullOr(Schema.String))).annotate({ description: 'Failure signature with the run it first failed in and its first later passing run' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const queried = Effect.fn('queried')(function* (runs: string, statement: string, values: Record.ReadonlyRecord<string, string>) {
    return yield* Effect.mapError(printedLines('duckdb', ['-no-agent', '-list', '-noheader', '-c', statement], { HARNESS_RUNS: runs, ...Record.mapKeys(values, (key) => `HARNESS_${key.toUpperCase()}`) }), HarnessError.refused(runs));
});
const since = Effect.fn('since')(function* (logs: string, host: string, signatures: Array.NonEmptyReadonlyArray<typeof Signed.Type>) {
    const path = yield* Path.Path;
    const runs = path.join(partition(path, logs, host, '*'), RECORDS);
    const rows = yield* queried(runs, `select to_json(dated) from (${SINCE}) as dated`, { signatures: Formatter.formatJson(signatures), structure: Formatter.formatJson([SIGNED]) });
    return yield* Effect.forEach(rows, (row) => Effect.mapBoth(Schema.decodeEffect(Dated)(row), { onFailure: HarnessError.refused(runs), onSuccess: Record.filter(Predicate.isNotNull) }));
});
const ours = (pid: number, stamp: string) => (held: Printed) => held.seen.pid === pid && held.seen.started === stamp;
const lifetimesOf = (records: readonly Printed[], now: DateTime.Utc) =>
    Array.getSomes(
        records
            .filter(({ seen }) => seen.message === 'instance launched')
            .map(({ seen }) =>
                Option.all({
                    pid: Option.fromUndefinedOr(seen.pid),
                    stamp: Option.fromUndefinedOr(seen.started),
                    started: Option.flatMap(Option.fromUndefinedOr(seen.started), DateTime.make),
                    app: Option.fromUndefinedOr(seen.app),
                }),
            ),
    ).map(({ pid, stamp, started, app }) => {
        const exited = Array.findLast(records.filter(ours(pid, stamp)), ({ seen }) => seen.message === 'instance' && seen.state === 'exited');
        return { pid, stamp, lifetime: { pid, started, ended: Option.match(exited, { onNone: () => now, onSome: ({ seen }) => seen.at }), chosen: { app } } };
    });
const unlinked =
    (records: readonly Printed[], pid: number, stamp: string) =>
    ({ kind, file }: Linked) =>
        !records.filter(ours(pid, stamp)).some(({ seen }) => seen.message === 'report linked' && seen.kind === kind && seen.file === Option.getOrUndefined(file));
const printedOf =
    (pid: number, stamp: string) =>
    ({ kind, at, file, facts }: Linked) => ({ at, line: Formatter.formatJson({ at: DateTime.formatIso(at), level: KINDS[kind].level, message: 'report linked', phase: 'log', pid, started: stamp, kind, ...Record.getSomes({ file }), ...facts }) });
const traced = Effect.fn('traced')(function* (logs: string, host: string) {
    const [fs, path, now] = yield* Effect.all([FileSystem.FileSystem, Path.Path, DateTime.now]);
    const folders = yield* Effect.mapError(fs.glob(partition(path, logs, host, '*')), HarnessError.refused(path.join(logs, `host=${host}`)));
    const absent = (folder: string) => Effect.map(fs.exists(path.join(folder, RECORDS)), (held) => !held);
    const missing = yield* Effect.filter(folders, absent);
    yield* Array.match(missing, { onEmpty: () => Effect.void, onNonEmpty: ([folder]) => HarnessError.unavailable(path.join(folder, RECORDS))('run folder holds no records file') });
    const runs = path.join(partition(path, logs, host, '*'), RECORDS);
    const lines =
        folders.length === 0
            ? []
            : yield* queried(
                  runs,
                  `with records as (select run, json, ordinality from read_ndjson_objects(getenv('HARNESS_RUNS'), hive_partitioning = true, ignore_errors = true) with ordinality where json is not null),
listed as (select regexp_extract(file, 'run=([^/]+)/[^/]+$', 1) as run from glob(getenv('HARNESS_RUNS'))),
chosen as (select coalesce((select max(run) from records where json->>'message' = 'run started' and json->>'session' = getenv('CLAUDE_CODE_SESSION_ID')), (select max(run) from listed)) as run)
select json from records where run = (select run from chosen) order by ordinality`,
                  {},
              );
    const records = yield* Effect.forEach(lines, (line) => Effect.mapBoth(Schema.decodeEffect(Read)(line), { onFailure: HarnessError.refused(runs), onSuccess: (seen) => ({ seen, line }) }));
    const lifetimes = lifetimesOf(records, now);
    const found = yield* Effect.forEach(lifetimes, ({ lifetime }) => system(lifetime));
    const late = Array.zipWith(lifetimes, found, ({ pid, stamp }, held) => held.filter(unlinked(records, pid, stamp)).map(printedOf(pid, stamp)));
    const spliced = Array.sort(
        [...records.map(({ seen, line }) => ({ at: seen.at, line })), ...late.flat()],
        Order.mapInput(DateTime.Order, ({ at }: { readonly at: DateTime.Utc }) => at),
    );
    yield* Array.match(spliced, { onEmpty: () => Effect.void, onNonEmpty: (held) => Console.log(held.map(({ line }) => line).join('\n')) });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { since, traced };
