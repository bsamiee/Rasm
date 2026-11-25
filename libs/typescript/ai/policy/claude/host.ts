/// <reference path="./types/claude-code/claude-code.d.ts" preserve="true" />
import type { EngineInterface, Next, On, ToolCallInput, ToolCallResult } from 'claude-code';
import { Duration, Effect, HashSet, Match, MutableRef, Option, Path } from 'effect';
import { Dialects, runtime } from '#/claude/runtime';
import { Told } from '#/judgment/rule';
import type { Policy } from '#/policy';

// --- [TYPES] ---------------------------------------------------------------------------

type Engine = ReturnType<typeof runtime>;
type Judgment = Effect.Success<ReturnType<Policy['judge']>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const reserve = 500;

// --- [OPERATIONS] ----------------------------------------------------------------------

const noted = (answer: ToolCallResult, notes: readonly string[]): ToolCallResult => ('deny' in answer || notes.length === 0 ? answer : { ...answer, context: [...(answer.context ?? []), ...notes] });

// --- [COMPOSITION] ---------------------------------------------------------------------

function called($: EngineInterface, e: ToolCallInput, next: Next<'tool.call'>, policy: Policy, engine: Engine, told: MutableRef.MutableRef<HashSet.HashSet<Told>>) {
    return engine.runPromise(
        Effect.gen(function* () {
            const cwd = yield* Effect.promise(() => $.session.cwd());
            const judgment = yield* Dialects.use(({ dialect }) => policy.judge(e, dialect, cwd)).pipe(
                Effect.updateService(Path.Path, (posix) => Path.make({ ...posix, resolve: (...segments) => posix.resolve(cwd, ...segments) })),
                Effect.timeoutOrElse({ duration: Duration.millis(Math.max(0, next.budget.remainingMs - reserve)), orElse: () => Effect.succeed<Judgment>({ _tag: 'Pass', notes: [new Told({ text: 'policy ran out of time and let the call run', once: 'call', by: '' })] }) }),
            );
            const held = MutableRef.get(told);
            const notes = judgment.notes.filter((note) => note.once === 'call' || !HashSet.has(held, note));
            MutableRef.set(told, HashSet.union(held, HashSet.fromIterable(notes)));
            const answered = (input: ToolCallInput) =>
                Effect.map(
                    Effect.promise(() => next(input)),
                    (answer) =>
                        noted(
                            answer,
                            notes.map(({ text }) => text),
                        ),
                );
            return yield* Match.valueTags(judgment, {
                Refuse: ({ reasons }) => Effect.succeed<ToolCallResult>({ deny: [...reasons, ...notes.map(({ text }) => text)].join('\n') }),
                Rewrite: ({ command, background }) => answered({ ...e, command, ...(background ? { run_in_background: true } : {}) }),
                Pass: () => answered(e),
            });
        }),
        { signal: next.signal },
    );
}

function host(on: On, policy: Policy) {
    const held = MutableRef.make(Option.none<Engine>());
    const told = MutableRef.make(HashSet.empty<Told>());
    on('session.start', ($, e, next) => {
        MutableRef.set(held, Option.some(runtime($)));
        return next(e);
    }).catch((_, e, next) => next(e));
    on('session.end', (_, e, next) =>
        Option.match(e.reason === 'clear' ? Option.none() : MutableRef.getAndSet(held, Option.none()), {
            onNone: () => next(e),
            onSome: (engine) => Promise.all([next(e), engine.dispose()]).then(([ended]) => ended),
        }),
    ).catch((_, e, next) => next(e));
    on('tool.call', ($, e, next) =>
        Option.match(
            Option.filter(MutableRef.get(held), () => policy.hears(e)),
            {
                onNone: () => next(e),
                onSome: (engine) => called($, e, next, policy, engine, told),
            },
        ),
    ).catch((_, e, next) => next(e).then((answer) => noted(answer, [`policy failed (${next.error.kind}) and let the call run`])));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { host };
