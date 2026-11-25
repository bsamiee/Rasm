/// <reference path="./types/claude-code/claude-code.d.ts" preserve="true" />
import type { EngineInterface, Next, On, Timer, ToolCallInput, ToolCallResult } from 'claude-code';
import { Array, Duration, Effect, Function, HashMap, HashSet, Match, MutableRef, Option, Path, Record, Schema, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Dialects, runtime } from '#/claude/runtime';
import { Told } from '#/judgment/rule';
import type { Policy } from '#/policy';

// --- [TYPES] ---------------------------------------------------------------------------

type Engine = ReturnType<typeof runtime>;
type Judgment = Effect.Success<ReturnType<Policy['judge']>>;
interface Task {
    readonly file: string;
    readonly agent: Option.Option<string>;
    readonly told: Option.Option<string>;
}
interface State {
    readonly live: MutableRef.MutableRef<Option.Option<{ readonly engine: Engine; readonly watch: Timer }>>;
    readonly told: MutableRef.MutableRef<HashMap.HashMap<string, HashSet.HashSet<Told>>>;
    readonly tasks: MutableRef.MutableRef<HashMap.HashMap<string, Task>>;
    readonly unjudged: MutableRef.MutableRef<HashMap.HashMap<string, string>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const reserve = 500;
const wait = 300_000;
const scan = 300_000;
const repeats = 5;
const inplace = 'inplace';
const Polled = Schema.Struct({ description: Schema.optionalKey(Schema.String), timeout: Schema.optionalKey(Schema.Finite), agentId: Schema.optionalKey(Schema.String) });
const Started = Schema.Struct({ backgroundTaskId: Schema.String });
const moved = 'This wait ran as a Monitor: each line it prints and its exit reach you as notices and its timeout bounds it, so keep working or end the turn.';
const detached = 'This call runs in the background and notifies you when it ends, so never poll it.';
const whole = 'The call was refused whole, so none of it ran.';
const worktree = 'A worktree is a second checkout that every build repeats and the guards of this checkout never see, so work in place.';

// --- [OPERATIONS] ----------------------------------------------------------------------

const noted = (answer: ToolCallResult, notes: readonly string[]): ToolCallResult => ('deny' in answer || notes.length === 0 ? answer : { ...answer, context: [...(answer.context ?? []), ...notes] });
const recorded = (state: State, e: ToolCallInput, answer: ToolCallResult) => {
    const started = Option.flatMap(
        Option.liftPredicate(answer.result, () => e.tool === 'Bash'),
        Schema.decodeUnknownOption(Started),
    );
    const found = Option.flatMap(started, ({ backgroundTaskId: id }) =>
        Option.map(
            Array.findFirst(
                (answer.text ?? '').split(/\s+/u).map((word) => word.replace(/\.$/u, '')),
                (word) => word.endsWith(`/tasks/${id}.output`),
            ),
            (file) => [id, file] as const,
        ),
    );
    MutableRef.update(state.tasks, (tasks) => Option.match(found, { onNone: () => tasks, onSome: ([id, file]) => HashMap.set(tasks, id, { file, agent: Option.fromUndefinedOr(e.agentId), told: Option.none() }) }));
};
const forgotten = (state: State, session: string, agent: Option.Option<string>) => {
    MutableRef.update(
        state.told,
        HashMap.modify(
            session,
            HashSet.filter(({ once, by }) => !((once === 'agent' && by === Option.getOrElse(agent, () => '')) || (once === 'session' && Option.isNone(agent)))),
        ),
    );
};

// --- [COMPOSITION] ---------------------------------------------------------------------

function watched($: EngineInterface, e: ToolCallInput, next: Next<'tool.call'>, command: string, notes: readonly string[]) {
    return Effect.gen(function* () {
        const { description = 'Wait for the condition this call polled', timeout = wait, agentId } = yield* Effect.orDie(Schema.decodeEffect(Polled)(e));
        const armed = yield* Effect.promise(() => $.agent.call({ tool: 'Monitor', input: { command, description, timeout_ms: timeout }, ...(agentId === undefined ? {} : { agentId }) }, { signal: next.signal }));
        const answer: ToolCallResult = armed.deny === undefined ? noted({ result: { stdout: armed.text ?? '', stderr: '', interrupted: false }, context: [moved], ...(armed.isError === true ? { isError: true } : {}) }, notes) : { deny: armed.deny };
        return answer;
    });
}

function scanned($: EngineInterface, state: State) {
    return Effect.gen(function* () {
        const listed = HashMap.toEntries(MutableRef.get(state.tasks));
        const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
        const printed = (argv: readonly [string, ...string[]]) => Effect.scoped(Effect.flatMap(spawner.spawn(ChildProcess.make(argv[0], argv.slice(1), { stdin: 'ignore' })), (handle) => Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' })));
        const [agents, [open]] = yield* Effect.all([Effect.promise(() => $.agent.list()), Effect.filterOrFail(printed(['lsof', '-F', 'an', '--', ...listed.map(([, { file }]) => file)]), ([, code]) => code <= 1)], { concurrency: 'unbounded' });
        const written = new Set(Array.mapAccum(open.split('\n'), '', (access, line) => [line.startsWith('a') ? line.slice(1) : access, line.startsWith('n') && /[wu]/u.test(access) ? [line.slice(1)] : []] as const)[1].flat());
        const running = new Set(agents.filter(({ status }) => status === 'running').map(({ id }) => id));
        MutableRef.update(state.tasks, HashMap.removeMany(listed.flatMap(([id, { file }]) => (written.has(file) ? [] : [id]))));
        const resting = listed.filter(([, { file }]) => written.has(file)).filter(([, { agent }]) => Option.match(agent, { onNone: Function.constTrue, onSome: (id) => !running.has(id) }));
        const tails = yield* Effect.forEach(resting, ([, { file }]) => Effect.map(printed(['tail', '-n', '10', '--', file]), ([tail, code]) => (code === 0 ? tail : '')), { concurrency: 'unbounded' });
        const stuck = Array.zip(resting, tails).flatMap(([[id, task], tail]) => {
            const lines = Array.groupBy(
                tail.split('\n').flatMap((line) => (line.trim() === '' ? [] : [line.replaceAll(/\d+/gu, '#')])),
                Function.identity,
            );
            return Option.toArray(
                Option.filter(
                    Array.findFirst(Record.toEntries(lines), ([, same]) => same.length >= repeats),
                    ([line]) => !Option.contains(task.told, line),
                ),
            ).map(([line]) => ({ id, task, line }));
        });
        MutableRef.update(state.tasks, (tasks) => stuck.reduce((held, { id, task, line }) => HashMap.set(held, id, { ...task, told: Option.some(line) }), tasks));
        yield* Effect.forEach(
            stuck,
            ({ id, task, line }) => {
                const text = `Background task ${id} repeats "${line}", so it looks stuck and its completion notice may never come (${task.file}): read its last lines, TaskStop ${id} if it hangs or loops, fix the cause, and rerun it bounded by a count or timeout.`;
                return Effect.promise((): Promise<unknown> => Option.match(task.agent, { onNone: () => $.prompt.submit({ text }), onSome: (agentId) => $.session.send({ to: { agentId }, text }) }));
            },
            { discard: true },
        );
    }).pipe(Effect.when(Effect.sync(() => HashMap.size(MutableRef.get(state.tasks)) > 0)), Effect.ignore);
}

function called($: EngineInterface, e: ToolCallInput, next: Next<'tool.call'>, policy: Policy, engine: Engine, state: State) {
    return engine.runPromise(
        Effect.gen(function* () {
            const [cwd, session] = yield* Effect.promise(() => Promise.all([$.session.cwd(), $.session.id()]));
            const judgment = yield* Dialects.use(({ dialect }) => policy.judge(e, dialect, cwd)).pipe(
                Effect.updateService(Path.Path, (posix) => Path.make({ ...posix, resolve: (...segments) => posix.resolve(cwd, ...segments) })),
                Effect.timeoutOrElse({ duration: Duration.millis(Math.max(0, next.budget.remainingMs - reserve)), orElse: () => Effect.succeed<Judgment>({ _tag: 'Pass', notes: [new Told({ text: 'policy ran out of time and let the call run', once: 'call', by: '' })] }) }),
            );
            const held = Option.getOrElse(HashMap.get(MutableRef.get(state.told), session), HashSet.empty<Told>);
            const fresh = judgment.notes.filter((note) => note.once === 'call' || !HashSet.has(held, note));
            MutableRef.update(state.told, HashMap.set(session, HashSet.union(held, HashSet.fromIterable(fresh))));
            const notes = fresh.map(({ text }) => text);
            const answered = (input: ToolCallInput, added: readonly string[]) =>
                Effect.map(
                    Effect.promise(() => next(input)),
                    (answer) => {
                        recorded(state, input, answer);
                        return noted(answer, [...added, ...notes]);
                    },
                );
            return yield* Match.valueTags(judgment, {
                Refuse: ({ reasons }) => Effect.succeed<ToolCallResult>({ deny: [...reasons, whole, ...notes].join('\n') }),
                Rewrite: ({ command, run }) =>
                    Match.value(run).pipe(
                        Match.when('monitor', () => watched($, e, next, command, notes)),
                        Match.when('background', () => {
                            const { timeout: _timeout, ...untimed } = e;
                            return answered({ ...untimed, command, run_in_background: true }, [detached]);
                        }),
                        Match.orElse(() => answered({ ...e, command }, [])),
                    ),
                Pass: () => answered(e, []),
            });
        }),
        { signal: next.signal },
    );
}

function host(on: On, policy: Policy) {
    const state: State = { live: MutableRef.make(Option.none()), told: MutableRef.make(HashMap.empty()), tasks: MutableRef.make(HashMap.empty()), unjudged: MutableRef.make(HashMap.empty()) };
    on('session.start', ($, e, next) => {
        const engine = runtime($);
        const watch = $.clock.every(scan, () => {
            engine.runFork(scanned($, state));
        });
        return Option.match(MutableRef.getAndSet(state.live, Option.some({ engine, watch })), {
            onNone: () => next(e),
            onSome: (stale) => {
                stale.watch.cancel();
                return Promise.all([next(e), stale.engine.dispose()]).then(([started]) => started);
            },
        });
    }).catch((_, e, next) => next(e));
    on('session.end', (_, e, next) =>
        Option.match(e.reason === 'clear' ? Option.none() : MutableRef.getAndSet(state.live, Option.none()), {
            onNone: () => next(e),
            onSome: ({ engine, watch }) => {
                watch.cancel();
                return Promise.all([next(e), engine.dispose()]).then(([ended]) => ended);
            },
        }),
    ).catch((_, e, next) => next(e));
    on('session.compact', ($, e, next) =>
        $.session.id().then((session) => {
            forgotten(state, session, Option.fromUndefinedOr(e.agentId));
            return next(e);
        }),
    ).catch((_, e, next) => next(e));
    on('vcs.isolate', (_, e) => (e.for === 'agent' ? { path: e.from, vcs: inplace } : { deny: worktree })).catch(() => ({ deny: worktree }));
    on('vcs.status', { vcs: inplace }, () => ({ changes: [], unpublished: 0 })).catch(() => ({ changes: [], unpublished: 0 }));
    on('vcs.release', { vcs: inplace }, () => ({ isReleased: true as const })).catch(() => ({ isReleased: true as const }));
    on('tool.call', ($, e, next) =>
        Option.match(
            Option.filter(MutableRef.get(state.live), () => policy.hears(e)),
            {
                onNone: () => next(e),
                onSome: ({ engine }) => called($, e, next, policy, engine, state),
            },
        ),
    ).catch(($, e, next) =>
        next(e).then((answer) => {
            const line = `policy did not judge this call and let it run: ${(next.error.message ?? next.error.kind).split('\n', 1).join('')}`;
            const agent = e.agentId ?? '';
            const repeated = Option.contains(HashMap.get(MutableRef.get(state.unjudged), agent), line);
            $.ui.log(line, { to: 'debug' });
            MutableRef.update(state.unjudged, HashMap.set(agent, line));
            return repeated ? answer : noted(answer, [line]);
        }),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { host };
