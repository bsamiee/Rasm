import { ppid } from 'node:process';
import { State } from '@rasm/automation/applications';
import { Array, Cause, Config, Context, DateTime, Duration, Effect, Exit, FileSystem, Match, Option, Order, Path, type PlatformError, Predicate, Schema, type Scope, String, type SubscriptionRef } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HarnessError } from '#/error';
import { printed, sampled, starts } from '#/instance';

// --- [TYPES] ---------------------------------------------------------------------------

type Phase = keyof typeof PHASES;
type Sighting = <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>;
interface Watched {
    readonly pid: number;
    readonly started: DateTime.Utc;
    readonly state: SubscriptionRef.SubscriptionRef<typeof State.Type>;
}
interface Retention {
    readonly age: Duration.Input;
    readonly count: number;
}
interface Dated {
    readonly file: string;
    readonly mtime: Option.Option<Date>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PHASES = { run: false, prove: false, compare: false, plan: true, launch: true, ready: true, open: true, survey: true, batch: true, quit: true } as const;
const LINES = '.jsonl';
const RUNS = 'runs';
const INSTANCES = 'instances';

// --- [MODELS] --------------------------------------------------------------------------

const session = Config.String('CLAUDE_CODE_SESSION_ID').pipe(Config.orElse(() => Config.succeed(`ppid-${ppid}`)));
const agent = Config.option(Config.String('AI_AGENT'));
const newest = Order.flip(Order.mapInput(Option.makeOrder(Order.Date), ({ mtime }: Dated) => mtime));

// --- [SERVICES] ------------------------------------------------------------------------

class Run extends Context.Service<Run, { readonly commit: string; readonly instances: string; readonly sightings: (file: string) => Effect.Effect<Sighting, PlatformError.PlatformError, FileSystem.FileSystem | Scope.Scope> }>()('@rasm/harnesses/log/Run') {
    static readonly Instance = Context.Reference<Option.Option<Watched>>('@rasm/harnesses/log/Run/Instance', { defaultValue: Option.none });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RECORDS]
const stateOf = (state: typeof State.Type) => (State.guards.Responding(state) && state.answering ? 'Dialog' : state._tag);
const keyOf = (pid: number, started: DateTime.Utc) => `${pid}-${DateTime.toEpochMillis(started)}`;
const tagOf = Match.type<unknown>().pipe(
    Match.when(Schema.is(HarnessError), ({ reason }) => reason._tag),
    Match.when(Predicate.isError, ({ name }) => name),
    Match.orElse(() => 'Defect'),
);
const failure = (signed: boolean, cause: Cause.Cause<unknown>) => {
    const squashed = Cause.squash(cause);
    return { ...(signed ? { tag: Option.match(Cause.findErrorOption(cause), { onNone: () => 'Defect', onSome: tagOf }) } : {}), error: Predicate.isError(squashed) ? squashed.message : globalThis.String(squashed) };
};
const phase =
    (label: Phase) =>
    <A, E, R>(self: Effect.Effect<A, E, R>) =>
        self.pipe(
            Effect.onExit(
                Exit.match({
                    onSuccess: () => Effect.logInfo(`${label} ended`),
                    onFailure: (cause) => (Cause.hasInterruptsOnly(cause) ? Effect.logWarning(`${label} interrupted`) : Effect.annotateLogs(Effect.logError(`${label} failed`), failure(PHASES[label], cause))),
                }),
            ),
            Effect.withLogSpan(label),
        );

// --- [INSTANCES]
const placed = Effect.fn('placed')(function* (watched: Pick<Watched, 'pid' | 'started'>, suffix: string) {
    const [{ instances }, path] = yield* Effect.all([Run, Path.Path]);
    return path.join(instances, `${keyOf(watched.pid, watched.started)}${suffix}`);
});
const sightings = (watched: Pick<Watched, 'pid' | 'started'>) => Effect.flatMap(Effect.all([Run, placed(watched, LINES)]), ([run, file]) => run.sightings(file));
const sample = (watched: Pick<Watched, 'pid' | 'started'>) => Effect.flatMap(placed(watched, '.sample.txt'), (file) => sampled(watched.pid, file));
const alive = Effect.map(starts(['-A']), (held) => new Set(Array.map(Array.fromIterable(held), ([pid, started]) => keyOf(pid, started))));

// --- [RETENTION]
const pruned = Effect.fn('pruned')(function* (root: string, { age, count }: Retention) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const now = DateTime.toEpochMillis(yield* DateTime.now);
    const live = yield* alive;
    const stated = (file: string) => Effect.map(fs.stat(path.join(root, file)), ({ mtime }): Dated => ({ file: path.join(root, file), mtime }));
    const dated = (pattern: string) => Effect.flatMap(fs.glob(pattern, { root }), (files) => Effect.forEach(files, stated));
    const [runs, instances] = yield* Effect.all([dated(`${RUNS}/*/*${LINES}`), dated(`${INSTANCES}/*/*`)]);
    const before =
        (at: number) =>
        ({ mtime }: Dated) =>
            Option.exists(mtime, (date) => date.getTime() < at);
    const kept = (held: readonly Dated[]) => Array.sort(held, newest).filter((entry, at) => at >= count || before(now - Duration.toMillis(age))(entry));
    const stale = [...Object.values(Array.groupBy(runs, ({ file }) => path.dirname(file))).flatMap(kept), ...instances.filter((entry) => !live.has(Array.headNonEmpty(String.split(path.basename(entry.file), '.'))) && before(now)(entry))];
    yield* Effect.forEach(stale, ({ file }) => fs.remove(file, { force: true }), { discard: true });
});

// --- [RUNS]
const begun = Effect.fn('begun')(function* (host: string, root: Config.Config<string>, retention: Retention) {
    const [fs, path, random, folder, owner, named] = yield* Effect.all([FileSystem.FileSystem, Path.Path, SecureRandom.SecureRandom, root, session, agent]);
    const runs = path.join(folder, RUNS, `host=${host}`);
    const instances = path.join(folder, INSTANCES, `host=${host}`);
    yield* Effect.all([fs.makeDirectory(runs, { recursive: true }), fs.makeDirectory(instances, { recursive: true })]);
    yield* pruned(folder, retention);
    const [run, commit] = yield* Effect.all([random.uuidV7(), Effect.map(printed('git', ['rev-parse', 'HEAD']), String.trim)]);
    return { commit, instances, file: path.join(runs, `${run}${LINES}`), fixed: { run, session: owner, ...Option.match(named, { onNone: () => ({}), onSome: (held) => ({ agent: held }) }), host } };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { alive, begun, INSTANCES, LINES, PHASES, phase, RUNS, Run, type Sighting, sample, session, sightings, stateOf, tagOf };
