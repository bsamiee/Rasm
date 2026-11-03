import type { State } from '@rasm/automation/applications';
import { Array, Cause, Config, Context, DateTime, Duration, Effect, FileSystem, Filter, type LogLevel, Match, MutableRef, Option, Path, Predicate, Record, Result, Schema, Stdio, String, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HarnessError } from '#/error';
import { printed, System, sampled } from '#/instance';
import { Halted } from '#/results';
import type { Live } from '#/session';

// --- [TYPES] ---------------------------------------------------------------------------

type Phase = 'run' | 'prove' | 'compare' | 'plan' | 'launch' | 'ready' | 'open' | 'survey' | 'batch' | 'quit' | 'log';
interface Tally {
    readonly cases: { readonly ran: number; readonly failed: number; readonly notRun: number };
    readonly signatures: readonly (typeof Signed.Type)[];
    readonly instances: readonly { readonly pid: number; readonly started: string; readonly state: keyof typeof STATES; readonly reports: readonly (typeof Kind.Type)[] }[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const STATES = { Launching: 'Info', Responding: 'Info', Unresponsive: 'Info', Failed: 'Warn', Exited: 'Info' } as const satisfies { readonly [K in keyof typeof State.cases]: LogLevel.Severity };
const RECORDS = 'records.jsonl';
const STAMPED = /(?<high>[\da-f]{8})-(?<low>[\da-f]{4})-7[\da-f]{3}-[\da-f]{4}-[\da-f]{12}$/u;
const RETAINED = { runs: 200, age: Duration.weeks(1) } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Kind = Schema.Literals(['crash', 'hang', 'spin', 'shutdown', 'recovery', 'host']).annotate({
    description: "System report or host log kind an instance leaves behind: ReportCrash and vendor crash reports, hang and resource reports, spindump and force-quit log lines, protective-shutdown entries, recovery files, and the host's own session logs",
});
const Field = {
    pid: Schema.optionalKey(Schema.Int),
    started: Schema.optionalKey(Schema.String),
    version: Schema.optionalKey(Schema.String),
    proof: Schema.optionalKey(Schema.String),
    case: Schema.optionalKey(Schema.String),
    tag: Schema.optionalKey(Schema.String),
    completion: Schema.optionalKey(Schema.String),
    cases: Schema.optionalKey(Schema.Array(Schema.String)),
    kind: Schema.optionalKey(Kind),
    state: Schema.optionalKey(Schema.Literals(Struct.keys(STATES))),
    file: Schema.optionalKey(Schema.String),
    title: Schema.optionalKey(Schema.String),
    frames: Schema.optionalKey(Schema.Array(Schema.String)),
};
const scoped = Struct.pick(Field, ['pid', 'started', 'case']);
const grouped = Struct.pick(Field, ['pid', 'started', 'version', 'proof', 'case']);
const sighting = Schema.Struct({
    ...scoped,
    bundle: Schema.optionalKey(Schema.String),
    window: Schema.optionalKey(Schema.Int),
    id: Schema.optionalKey(Schema.String),
    number: Schema.optionalKey(Schema.Int),
    role: Schema.optionalKey(Schema.String),
    subrole: Schema.optionalKey(Schema.String),
    title: Field.title,
    document: Schema.optionalKey(Schema.String),
    edited: Schema.optionalKey(Schema.Boolean),
    modal: Schema.optionalKey(Schema.Boolean),
    identifier: Schema.optionalKey(Schema.String),
    requiresAnswer: Schema.optionalKey(Schema.Boolean),
    text: Schema.optionalKey(Schema.Array(Schema.String)),
    controls: Schema.optionalKey(Schema.Array(Schema.Json)),
});
const Messages = {
    'run started': Schema.Struct({ commit: Schema.String, args: Schema.Array(Schema.String), session: Schema.NullOr(Schema.String) }),
    'instance launched': Schema.Struct({ ...scoped, app: Schema.String, created: Schema.Boolean, state: Schema.Literals(Struct.keys(STATES)), hidden: Schema.optionalKey(Schema.Boolean), watch: Schema.optionalKey(Schema.Int), shared: Schema.optionalKey(Schema.NonEmptyArray(Schema.Int)) }),
    instance: Schema.Struct({
        ...scoped,
        bundle: Schema.optionalKey(Schema.String),
        state: Schema.Literals(Struct.keys(STATES)),
        ...Struct.pick(Halted.cases.Hung.fields, ['process', 'cpu', 'footprint', 'threads']),
        duration: Schema.optionalKey(Schema.Finite.annotate({ description: 'Seconds from the unresponsive line opening the spell to the line this record closes it with, the stall having begun at least one watch-apps heartbeat earlier' })),
        reason: Schema.optionalKey(Schema.String),
        status: Schema.optionalKey(Schema.Int),
        signal: Schema.optionalKey(Schema.Int),
        detail: Schema.optionalKey(Schema.Array(Schema.String)),
        statusFailure: Schema.optionalKey(Schema.String),
        report: Schema.optionalKey(Schema.String),
        exception: Schema.optionalKey(Schema.String),
        reportFailure: Schema.optionalKey(Schema.String),
    }),
    'window opened': sighting,
    'window changed': sighting,
    'window closed': sighting,
    'dialog answered': Schema.Struct({ ...scoped, title: Field.title, text: Schema.Array(Schema.String), pressed: Schema.String }),
    'system pressure': System.fields.system,
    'case started': Schema.Struct({ ...grouped, case: Schema.String, call: Schema.Struct({ _tag: Schema.String, fn: Schema.optionalKey(Schema.String), script: Schema.optionalKey(Schema.String), args: Schema.optionalKey(Schema.Array(Schema.Json)) }) }),
    'case ended': Schema.StructWithRest(Schema.Struct({ ...grouped, case: Schema.String, completion: Schema.String, tag: Field.tag, error: Schema.optionalKey(Schema.String) }), [Schema.Record(Schema.String, Schema.Unknown)]),
    'cases not run': Schema.Struct({ ...grouped, cases: Schema.Array(Schema.String), error: Schema.String }),
    'report linked': Schema.Struct({
        ...scoped,
        kind: Kind,
        file: Field.file,
        event: Schema.optionalKey(Schema.String.annotate({ description: 'Event the spindump report names: hang, cpu resource, or disk writes' })),
        exception: Schema.optionalKey(Schema.String.annotate({ description: 'Mach exception type and signal the crash report names' })),
        reason: Schema.optionalKey(Schema.String.annotate({ description: 'Jetsam reason the memory report names for the pid' })),
        frames: Field.frames,
        duration: Schema.optionalKey(Schema.Finite.annotate({ description: 'Seconds the hang report states the process was unresponsive' })),
        lines: Schema.optionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: 'Lines of the file or log the kind keeps for the instance' })),
    }),
    'quit sent': Schema.Struct(scoped),
    failed: Schema.StructWithRest(Schema.Struct({ ...grouped, tag: Schema.String, error: Schema.String }), [Schema.Record(Schema.String, Schema.Unknown)]),
    compared: Schema.Struct({ ...grouped, module: Schema.String, agreed: Schema.Int, mismatched: Schema.Array(Schema.String), unproven: Schema.Array(Schema.String), tag: Field.tag }),
    'results written': Schema.Struct({ ...grouped, file: Schema.String }),
    'run ended': Schema.Struct({}),
};
const Message = Schema.Literals(Struct.keys(Messages));
const Counted = Schema.Struct({ level: Schema.String, phase: Schema.String, ...Struct.pick(Field, ['pid', 'started', 'version', 'proof', 'case', 'tag', 'completion', 'cases', 'kind', 'state']) }).annotate({ description: 'Record fields the closing summary tallies' });
const Signed = Schema.Struct({ ...Struct.pick(Field, ['version', 'proof', 'case']), phase: Schema.String, tag: Schema.String }).annotate({ description: 'Failure signature: where a run failed and what tag it failed with' });
const signedAlike = Schema.toEquivalence(Signed);
const session = Config.option(Config.String('CLAUDE_CODE_SESSION_ID'));

// --- [SERVICES] ------------------------------------------------------------------------

class Run extends Context.Service<Run, { readonly folder: string; readonly phase: MutableRef.MutableRef<Phase> }>()('@rasm/harnesses/log/Run') {
    static readonly Instance = Context.Reference<Option.Option<Pick<Live, 'pid' | 'started'>>>('@rasm/harnesses/log/Run/Instance', { defaultValue: Option.none });
}
class Signing extends Context.Service<Signing, true>()('@rasm/harnesses/log/Signing') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RECORDS]
const record = <M extends keyof typeof Messages>(level: LogLevel.Severity, message: M, fields: (typeof Messages)[M]['Encoded']) => Effect.annotateLogs(Effect.logWithLevel(level)(message), fields);
const declared = (message: string, fields: Record.ReadonlyRecord<string, unknown>): Record.ReadonlyRecord<string, unknown> =>
    Option.match(Schema.decodeUnknownOption(Message)(message), {
        onNone: () => fields,
        onSome: (held) => {
            const schema: Schema.Decoder<Record.ReadonlyRecord<string, unknown>> = Messages[held];
            return Result.getOrThrow(Schema.decodeResult(schema)(fields));
        },
    });
const tagOf = Match.type<unknown>().pipe(
    Match.when(Schema.is(HarnessError), ({ reason }) => reason._tag),
    Match.when(Predicate.isError, ({ name }) => name),
    Match.orElse(() => 'Defect'),
);
const messageOf = Match.type<unknown>().pipe(
    Match.when(Predicate.isError, ({ message }) => message),
    Match.orElse((error) => globalThis.String(error)),
);
const failed = (label: Phase, cause: Cause.Cause<unknown>) =>
    Match.value(cause).pipe(
        Match.when(
            (held) => Option.isSome(Context.getOption(Cause.annotations(held), Signing)),
            () => Effect.void,
        ),
        Match.when(Cause.hasInterruptsOnly, () => record('Warn', 'failed', { tag: 'Interrupted', error: `${label} interrupted` })),
        Match.orElse((held) => {
            const error = Cause.squash(held);
            return record('Error', 'failed', { tag: tagOf(error), error: Cause.hasDies(held) ? Cause.pretty(held) : messageOf(error) });
        }),
    );
const signing = <E>(cause: Cause.Cause<E>) => Effect.failCause(Cause.annotate(cause, Context.make(Signing, true)));
const phase =
    (label: Phase) =>
    <A, E, R>(self: Effect.Effect<A, E, R>) =>
        Effect.flatMap(Run, ({ phase: current }) => {
            const outer = MutableRef.get(current);
            return Effect.sync(() => MutableRef.set(current, label)).pipe(
                Effect.andThen(self),
                Effect.onError((cause) => failed(label, cause)),
                Effect.catchCause(signing),
                Effect.ensuring(Effect.sync(() => MutableRef.set(current, outer))),
            );
        });
const counted =
    (entry: unknown) =>
    (tally: Tally): Tally =>
        Option.match(Schema.decodeUnknownOption(Counted)(entry), {
            onNone: () => tally,
            onSome: ({ level, phase: at, pid, started, version, proof, case: name, tag, completion, cases, kind, state }) => {
                const finished = completion !== undefined;
                const signed = Option.fromUndefinedOr(tag ?? (level === 'Info' ? undefined : completion));
                const ours = (held: Tally['instances'][number]) => held.pid === pid && held.started === started;
                const [mine, others] = Array.partition(tally.instances, Filter.fromPredicate(ours));
                const prior = Array.head(mine);
                const seen = Option.flatMap(Option.all({ pid: Option.fromUndefinedOr(pid), started: Option.fromUndefinedOr(started) }), (held) =>
                    Option.map(Option.firstSomeOf([Option.fromUndefinedOr(state), Option.map(prior, Struct.get('state'))]), (current) => ({
                        ...held,
                        state: current,
                        reports: Array.union(Option.match(prior, { onNone: () => [], onSome: Struct.get('reports') }), kind !== undefined && level === 'Warn' ? [kind] : []),
                    })),
                );
                return {
                    cases: { ran: tally.cases.ran + (finished ? 1 : 0), failed: tally.cases.failed + (finished && level === 'Error' ? 1 : 0), notRun: tally.cases.notRun + (cases ?? []).length },
                    signatures: Option.match(signed, {
                        onNone: () => tally.signatures,
                        onSome: (held) => Array.unionWith(tally.signatures, [{ ...Record.getSomes({ version: Option.fromUndefinedOr(version), proof: Option.fromUndefinedOr(proof), case: Option.fromUndefinedOr(name) }), phase: at, tag: held }], signedAlike),
                    }),
                    instances: Option.match(seen, { onNone: () => tally.instances, onSome: (held) => [...others, held] }),
                };
            },
        });
const sample = (pid: number) => Effect.flatMap(Effect.all([Run, Path.Path]), ([{ folder }, path]) => sampled(pid, path.join(folder, `${pid}.sample.txt`)));

// --- [RUNS]
const partition = (path: Path.Path, logs: string, host: string, run: string) => path.join(logs, `host=${host}`, `run=${run}`);
const begun = Effect.fn('begun')(function* (host: string, root: Config.Config<string>) {
    const [fs, path, run, logs, now] = yield* Effect.all([FileSystem.FileSystem, Path.Path, SecureRandom.SecureRandom.use((random) => random.uuidV7()), root, DateTime.now]);
    const folder = partition(path, logs, host, run);
    yield* fs.makeDirectory(folder, { recursive: true });
    const runs = yield* fs.glob(partition(path, logs, host, '*'));
    const cutoff = DateTime.subtractDuration(now, RETAINED.age);
    const born = (held: string) => Option.flatMap(Option.fromNullishOr(STAMPED.exec(held)?.groups), ({ high, low }) => DateTime.make(Number.parseInt(`${high}${low}`, 16)));
    const stale = runs.filter((held, index) => index < runs.length - RETAINED.runs || Option.exists(born(held), DateTime.isLessThan(cutoff)));
    yield* Effect.forEach(stale, (held) => fs.remove(held, { recursive: true, force: true }), { discard: true });
    return { run, folder, file: path.join(folder, RECORDS) };
});
const starting = Effect.flatMap(Effect.all([Effect.map(printed('git', ['rev-parse', 'HEAD']), String.trim), Stdio.Stdio.use((stdio) => stdio.args), session]), ([commit, args, owner]) => record('Info', 'run started', { commit, args, session: Option.getOrNull(owner) }));
const ran = <A, E, R>(self: Effect.Effect<A, E, R>) => starting.pipe(Effect.andThen(self), Effect.tap(record('Info', 'run ended', {})), phase('run'));

// --- [EXPORTS] -------------------------------------------------------------------------

export { begun, counted, declared, Field, Kind, Messages, messageOf, type Phase, partition, phase, RECORDS, Run, ran, record, Signed, STATES, sample, signing, type Tally, tagOf };
