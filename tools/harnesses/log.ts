import { State } from '@rasm/automation/applications';
import { Array, Cause, Config, Console, Context, DateTime, Duration, Effect, FileSystem, Formatter, Layer, Logger, Match, MutableRef, Option, Path, Predicate, References, Schema, Stdio, String, Struct, SubscriptionRef } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HarnessError } from '#/error';
import { Progress, printed, sampled } from '#/instance';
import type { Live } from '#/session';

// --- [TYPES] ---------------------------------------------------------------------------

type Phase = 'run' | 'prove' | 'compare' | 'plan' | 'launch' | 'ready' | 'open' | 'survey' | 'batch' | 'quit';
type Watched = Pick<Live, 'pid' | 'started' | 'passing' | 'state'>;
interface Tally {
    readonly cases: { readonly ran: number; readonly failed: number; readonly notRun: number };
    readonly signatures: readonly { readonly phase: string; readonly state: keyof typeof STATES | undefined; readonly tag: string }[];
    readonly instances: readonly { readonly pid: number; readonly started: string; readonly state: keyof typeof STATES; readonly reports: readonly (typeof Kind.Type)[] }[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const STATES = { Launching: 'Info', Responding: 'Info', Dialog: 'Warn', Unresponsive: 'Warn', Failed: 'Warn', Exited: 'Info' } as const;
const RECORDS = 'records.jsonl';
const LAUNCHED = 'instance launched';
const STAMPED = /(?<high>[\da-f]{8})-(?<low>[\da-f]{4})-7[\da-f]{3}-[\da-f]{4}-[\da-f]{12}$/u;
const RETAINED = { runs: 200, age: Duration.weeks(1) } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Kind = Schema.Literals(['crash', 'hang', 'spin', 'shutdown', 'recovery', 'host']).annotate({
    description: "System report or host log kind an instance leaves behind: ReportCrash and vendor crash reports, hang and resource reports, spindump and force-quit log lines, protective-shutdown entries, recovery files, and the host's own session logs",
});
const Counted = Schema.Struct({
    level: Schema.String,
    phase: Schema.String,
    tag: Schema.optionalKey(Schema.String),
    state: Schema.optionalKey(Schema.Literals(Struct.keys(STATES))),
    completion: Schema.optionalKey(Schema.String),
    cases: Schema.optionalKey(Schema.Array(Schema.String)),
    kind: Schema.optionalKey(Kind),
}).annotate({ description: 'Record fields the closing summary tallies' });
const session = Config.option(Config.String('CLAUDE_CODE_SESSION_ID'));

// --- [SERVICES] ------------------------------------------------------------------------

class Run extends Context.Service<Run, { readonly folder: string; readonly phase: MutableRef.MutableRef<Phase> }>()('@rasm/harnesses/log/Run') {
    static readonly Instance = Context.Reference<Option.Option<Watched>>('@rasm/harnesses/log/Run/Instance', { defaultValue: Option.none });
}
class Signed extends Context.Service<Signed, true>()('@rasm/harnesses/log/Signed') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RECORDS]
const stateOf = (state: typeof State.Type, passing: readonly Progress[]): keyof typeof STATES => (State.guards.Responding(state) && state.elements.some(Progress.requiring(passing)) ? 'Dialog' : state._tag);
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
            (held) => Option.isSome(Context.getOption(Cause.annotations(held), Signed)),
            () => Effect.void,
        ),
        Match.when(Cause.hasInterruptsOnly, () => Effect.logWarning(`${label} interrupted`)),
        Match.orElse((held) => Effect.annotateLogs(Effect.logError(`${label} failed`), { tag: tagOf(Cause.squash(held)), error: Cause.hasDies(held) ? Cause.pretty(held) : messageOf(Cause.squash(held)) })),
    );
const signing = <E>(cause: Cause.Cause<E>) => Effect.failCause(Cause.annotate(cause, Context.make(Signed, true)));
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
    (entry: unknown, instance: Option.Option<Omit<Tally['instances'][number], 'reports'>>) =>
    (tally: Tally): Tally =>
        Option.match(Schema.decodeUnknownOption(Counted)(entry), {
            onNone: () => tally,
            onSome: ({ level, phase: at, tag, state, completion, cases, kind }) => {
                const finished = completion !== undefined;
                const reports = kind !== undefined && level === 'Warn' ? [kind] : [];
                return {
                    cases: { ran: tally.cases.ran + (finished ? 1 : 0), failed: tally.cases.failed + (finished && level === 'Error' ? 1 : 0), notRun: tally.cases.notRun + (cases ?? []).length },
                    signatures: tag === undefined ? tally.signatures : Array.union(tally.signatures, [{ phase: at, state, tag }]),
                    instances: Option.match(instance, {
                        onNone: () => tally.instances,
                        onSome: (seen) => {
                            const ours = (held: Tally['instances'][number]) => held.pid === seen.pid && held.started === seen.started;
                            return [
                                ...tally.instances.filter((held) => !ours(held)),
                                {
                                    ...seen,
                                    reports: Array.union(
                                        tally.instances.filter(ours).flatMap((held) => held.reports),
                                        reports,
                                    ),
                                },
                            ];
                        },
                    }),
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
    const stale = runs.filter((held, index) => index < runs.length - RETAINED.runs || Option.exists(born(held), (at) => DateTime.isLessThan(at, cutoff)));
    yield* Effect.forEach(stale, (held) => fs.remove(held, { recursive: true, force: true }), { discard: true });
    return { run, folder, file: path.join(folder, RECORDS) };
});
const starting = Effect.flatMap(Effect.all([Effect.map(printed('git', ['rev-parse', 'HEAD']), String.trim), Stdio.Stdio.use((stdio) => stdio.args), session]), ([commit, args, owner]) =>
    Effect.annotateLogs(Effect.logInfo('run started'), { commit, args, ...Option.match(owner, { onNone: () => ({}), onSome: (held) => ({ session: held }) }) }),
);
const ran = <A, E, R>(self: Effect.Effect<A, E, R>) => starting.pipe(Effect.andThen(self), Effect.tap(Effect.logInfo('run ended')), phase('run'));

// --- [COMPOSITION] ---------------------------------------------------------------------

const logged = Effect.fnUntraced(function* (host: string, root: Config.Config<string>) {
    const { run, folder, file } = yield* begun(host, root);
    const current = MutableRef.make<Phase>('run');
    const tally = MutableRef.make<Tally>({ cases: { ran: 0, failed: 0, notRun: 0 }, signatures: [], instances: [] });
    const record = Logger.make(({ message, logLevel, fiber, date }) => {
        const instance = Option.map(fiber.getRef(Run.Instance), ({ pid, started, passing, state }) => ({ pid, started: DateTime.formatIso(started), state: stateOf(SubscriptionRef.getUnsafe(state), passing) }));
        return { instance, entry: { at: date.toISOString(), level: logLevel, message: Array.ensure(message).join(' '), phase: MutableRef.get(current), ...Option.getOrElse(instance, () => ({})), ...fiber.getRef(References.CurrentLogAnnotations) } };
    });
    const json = Logger.map(record, ({ entry }) => Formatter.formatJson(entry));
    yield* Effect.addFinalizer(() =>
        Effect.suspend(() => {
            const { cases, signatures, instances } = MutableRef.get(tally);
            return Console.log(Formatter.formatJson({ run, log: file, cases, ...(signatures.length === 0 ? {} : { signatures }), instances }));
        }),
    );
    return Layer.mergeAll(Layer.succeed(Run, { folder, phase: current }), Logger.layer([Logger.toFile(json, file), Logger.withConsoleLog(json), Logger.map(record, ({ entry, instance }) => MutableRef.update(tally, counted(entry, instance)))]));
}, Layer.unwrap);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Kind, LAUNCHED, logged, type Phase, partition, phase, RECORDS, Run, ran, STATES, sample, signing, stateOf, tagOf };
