import { State } from '@rasm/automation/applications';
import { Array, Cause, Config, Context, DateTime, Duration, Effect, Exit, FileSystem, type LogLevel, Match, Option, Path, Predicate, Schema, Stdio, String, type SubscriptionRef } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HarnessError } from '#/error';
import { printed, sampled } from '#/instance';

// --- [TYPES] ---------------------------------------------------------------------------

type Phase = 'run' | 'prove' | 'compare' | 'plan' | 'launch' | 'ready' | 'open' | 'survey' | 'batch' | 'quit';
interface Watched {
    readonly pid: number;
    readonly started: DateTime.Utc;
    readonly state: SubscriptionRef.SubscriptionRef<typeof State.Type>;
}
interface Tally {
    readonly levels: Partial<Record<LogLevel.LogLevel, number>>;
    readonly signatures: readonly { readonly phase: string | undefined; readonly state: keyof typeof STATES | undefined; readonly tag: string }[];
    readonly instances: readonly { readonly pid: number; readonly started: string; readonly state: keyof typeof STATES }[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const STATES = { Launching: 'Info', Responding: 'Info', Dialog: 'Warn', Unresponsive: 'Warn', Failed: 'Warn', Exited: 'Info' } as const;
const RECORDS = 'records.jsonl';
const LAUNCHED = 'instance launched';
const STAMPED = /(?<high>[\da-f]{8})-(?<low>[\da-f]{4})-7[\da-f]{3}-[\da-f]{4}-[\da-f]{12}$/u;

// --- [MODELS] --------------------------------------------------------------------------

const session = Config.option(Config.String('CLAUDE_CODE_SESSION_ID'));

// --- [SERVICES] ------------------------------------------------------------------------

class Run extends Context.Service<Run, { readonly folder: string }>()('@rasm/harnesses/log/Run') {
    static readonly Instance = Context.Reference<Option.Option<Watched>>('@rasm/harnesses/log/Run/Instance', { defaultValue: Option.none });
}
class Signed extends Context.Service<Signed, true>()('@rasm/harnesses/log/Signed') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RECORDS]
const stateOf = (state: typeof State.Type): keyof typeof STATES => (State.guards.Responding(state) && state.answering ? 'Dialog' : state._tag);
const tagOf = Match.type<unknown>().pipe(
    Match.when(Schema.is(HarnessError), ({ reason }) => reason._tag),
    Match.when(Predicate.isError, ({ name }) => name),
    Match.orElse(() => 'Defect'),
);
const failed = (label: Phase, cause: Cause.Cause<unknown>) => {
    const error = Cause.squash(cause);
    const held = Option.isSome(Context.getOption(Cause.annotations(cause), Signed));
    return Effect.annotateLogs(Effect.logError(`${label} failed`), { ...(held ? {} : { tag: tagOf(error) }), error: Predicate.isError(error) ? error.message : globalThis.String(error) });
};
const signing = <E>(cause: Cause.Cause<E>) => Effect.failCause(Cause.annotate(cause, Context.make(Signed, true)));
const phase =
    (label: Phase) =>
    <A, E, R>(self: Effect.Effect<A, E, R>) =>
        self.pipe(
            Effect.onExit(
                Exit.match({
                    onSuccess: () => Effect.logInfo(`${label} ended`),
                    onFailure: (cause) => (Cause.hasInterruptsOnly(cause) ? Effect.logWarning(`${label} interrupted`) : failed(label, cause)),
                }),
            ),
            Effect.catchCause(signing),
            Effect.withLogSpan(label),
        );
const counted =
    ({ level, phase: at, tag, pid, started, state }: { readonly level: LogLevel.LogLevel; readonly phase?: string; readonly tag?: unknown; readonly pid?: number; readonly started?: string; readonly state?: keyof typeof STATES }) =>
    ({ levels, signatures, instances }: Tally): Tally => ({
        levels: { ...levels, [level]: (levels[level] ?? 0) + 1 },
        signatures: Predicate.isString(tag) ? Array.union(signatures, [{ phase: at, state, tag }]) : signatures,
        instances: Option.match(Option.all({ pid: Option.fromUndefinedOr(pid), started: Option.fromUndefinedOr(started), state: Option.fromUndefinedOr(state) }), {
            onNone: () => instances,
            onSome: (seen) => [...instances.filter((held) => held.pid !== seen.pid || held.started !== seen.started), seen],
        }),
    });
const sample = (pid: number) => Effect.flatMap(Effect.all([Run, Path.Path]), ([{ folder }, path]) => sampled(pid, path.join(folder, `${pid}.sample.txt`)));

// --- [RUNS]
const located = (path: Path.Path, logs: string, host: string, run: string) => path.join(logs, `host=${host}`, `run=${run}`);
const begun = Effect.fn('begun')(function* (host: string, root: Config.Config<string>) {
    const [fs, path, run, logs, owner, now] = yield* Effect.all([FileSystem.FileSystem, Path.Path, SecureRandom.SecureRandom.use((random) => random.uuidV7()), root, session, DateTime.now]);
    const folder = located(path, logs, host, run);
    yield* fs.makeDirectory(folder, { recursive: true });
    const runs = yield* fs.glob(located(path, logs, host, '*'));
    const kept = 200;
    const cutoff = DateTime.subtractDuration(now, Duration.weeks(1));
    const born = (held: string) => Option.flatMap(Option.fromNullishOr(STAMPED.exec(held)?.groups), ({ high, low }) => DateTime.make(Number.parseInt(`${high}${low}`, 16)));
    const stale = runs.filter((held, index) => index < runs.length - kept || Option.exists(born(held), (at) => DateTime.isLessThan(at, cutoff)));
    yield* Effect.forEach(stale, (held) => fs.remove(held, { recursive: true, force: true }), { discard: true });
    return { run, folder, file: path.join(folder, RECORDS), fixed: Option.match(owner, { onNone: () => ({}), onSome: (held) => ({ session: held }) }) };
});
const starting = Effect.flatMap(Effect.all([Effect.map(printed('git', ['rev-parse', 'HEAD']), String.trim), Stdio.Stdio.use((stdio) => stdio.args)]), ([commit, args]) => Effect.annotateLogs(Effect.logInfo('run started'), { commit, args }));
const ran = <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.andThen(starting, self).pipe(phase('run'));

// --- [EXPORTS] -------------------------------------------------------------------------

export { begun, counted, LAUNCHED, located, phase, RECORDS, Run, ran, STATES, sample, session, signing, stateOf, type Tally, tagOf };
