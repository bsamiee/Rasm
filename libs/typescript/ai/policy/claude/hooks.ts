/// <reference path="./types/claude-code.d.ts" preserve="true" />
import type { EngineInterface, Next, On, ToolCallInput, ToolCallResult, VcsIsolateInput } from 'claude-code';
import { Duration, Effect, Match, MutableRef, Option, Record, Schedule, Schema, SchemaTransformation } from 'effect';
import { runtime, Session } from '#/claude/runtime';
import { Action, Judgment } from '#/judgment/action';
import type { Policy } from '#/policy';

// --- [TYPES] ---------------------------------------------------------------------------

type Engine = ReturnType<typeof runtime>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const reserve = 500;
const monitored = 300_000;
const inplace = 'inplace';
const whole = 'The call was refused whole, so none of it ran.';

// --- [MODELS] --------------------------------------------------------------------------

const asked = { call: Schema.String, agent: Schema.OptionFromOptionalKey(Schema.String) };
const keys = { _tag: 'tool', call: 'tool_use_id', agent: 'agentId' } as const;
const Called = Schema.Union([
    Schema.Struct({
        _tag: Schema.Literal('Bash').transform('Shell'),
        ...asked,
        command: Schema.String,
        wait: Schema.Union([Schema.Boolean, Schema.Literal('true')]).pipe(Schema.decodeTo(Schema.Literals(['held', 'notified']), SchemaTransformation.transform({ decode: (flag) => (flag === false ? 'held' : 'notified'), encode: (wait) => wait === 'notified' })), Schema.withDecodingDefaultKey(Effect.succeed(false))),
    }).pipe(Schema.encodeKeys({ ...keys, wait: 'run_in_background' })),
    Schema.Struct({ _tag: Schema.Literal('Monitor').transform('Shell'), ...asked, command: Schema.String, wait: Schema.tagDefaultOmit('streamed') }).pipe(Schema.encodeKeys(keys)),
    Schema.Struct({ _tag: Schema.Literal('Monitor').transform('Fetch'), ...asked, url: Schema.Struct({ url: Schema.String }).pipe(Schema.decodeTo(Schema.String, SchemaTransformation.transform({ decode: ({ url }) => url, encode: (url) => ({ url }) }))) }).pipe(Schema.encodeKeys({ ...keys, url: 'ws' })),
    Schema.TaggedStruct('Read', { ...asked, path: Schema.String }).pipe(Schema.encodeKeys({ ...keys, path: 'file_path' })),
    Schema.TaggedStruct('Write', { ...asked, path: Schema.String, content: Schema.String }).pipe(Schema.encodeKeys({ ...keys, path: 'file_path' })),
    Schema.TaggedStruct('Edit', { ...asked, path: Schema.String, old: Schema.String, new: Schema.String }).pipe(Schema.encodeKeys({ ...keys, path: 'file_path', old: 'old_string', new: 'new_string' })),
    Schema.Struct({ _tag: Schema.Literal('WebFetch').transform('Fetch'), ...asked, url: Schema.String }).pipe(Schema.encodeKeys(keys)),
    Schema.Struct({ _tag: Schema.Literal('Agent').transform('Spawn'), ...asked, prompt: Schema.String, model: Schema.OptionFromOptionalKey(Schema.String) }).pipe(Schema.encodeKeys(keys)),
    Schema.StructWithRest(Schema.Struct({ tool: Schema.TemplateLiteralParser(['mcp__', Schema.NonEmptyString, '__', Schema.NonEmptyString]), tool_use_id: Schema.String, agentId: Schema.OptionFromOptionalKey(Schema.String) }), [Schema.Record(Schema.String, Schema.Unknown)]).pipe(
        Schema.decodeTo(
            Schema.toType(Schema.TaggedStruct('Mcp', { ...asked, server: Schema.String, tool: Schema.String, input: Schema.Record(Schema.String, Schema.Unknown) })),
            SchemaTransformation.transform({
                decode: ({ tool: [, server, , tool], tool_use_id: call, agentId: agent, ...input }) => ({ _tag: 'Mcp' as const, call, agent, server, tool, input }),
                encode: ({ call, agent, server, tool, input }) => ({ ...input, tool: ['mcp__', server, '__', tool] as const, tool_use_id: call, agentId: agent }),
            }),
        ),
    ),
]);
const Timed = Schema.Struct({ description: Schema.optionalKey(Schema.String), timeout: Schema.optionalKey(Schema.Finite) });
const Started = Schema.Struct({ result: Schema.Struct({ backgroundTaskId: Schema.String }), text: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const noted = (answer: ToolCallResult, notes: readonly string[]): ToolCallResult => ('deny' in answer || notes.length === 0 ? answer : { ...answer, context: [...(answer.context ?? []), ...notes] });
const tasked = (answer: ToolCallResult) => Option.flatMap(Schema.decodeUnknownOption(Started)(answer), ({ result: { backgroundTaskId: id }, text }) => Option.map(Option.fromNullishOr(new RegExp(String.raw`\S*/tasks/${RegExp.escape(id)}\.output`, 'u').exec(text)), ([output]) => ({ id, output })));
const watched = (policy: Policy, { call, agent }: typeof Called.Type, task: Option.Option.Value<ReturnType<typeof tasked>>) =>
    Session.use((session) =>
        Effect.flatMap(session.resting(agent), (resting) => policy.judge(Action.Task({ ...task, call, agent, resting, again: Duration.infinity }), session.shell)).pipe(
            Effect.tap(({ notes }) => Effect.forEach(notes, (text) => session.told(agent, text), { discard: true })),
            Effect.map((judgment) =>
                Match.value(judgment).pipe(
                    Match.when({ _tag: 'Rewrite', action: { _tag: 'Task' } }, ({ action }) => action.again),
                    Match.orElse(() => Duration.infinity),
                ),
            ),
            Effect.repeat({
                while: Duration.isFinite,
                schedule: Schedule.forever.pipe(
                    Schedule.setInputType<Duration.Duration>(),
                    Schedule.addDelay(({ input }) => Effect.succeed(input)),
                ),
            }),
        ),
    );
const isolated = (e: VcsIsolateInput, next: Next<'vcs.isolate'>, policy: Policy, engine: Engine) =>
    engine.runPromise(
        Effect.flatMap(
            Session.use((session) => policy.judge(Action.Isolate({ call: e.name, agent: Option.fromUndefinedOr(e.parentAgentId), from: e.from }), session.shell)),
            (judgment) => (Judgment.$is('Refuse')(judgment) ? Effect.succeed({ agent: { path: e.from, vcs: inplace }, session: { deny: judgment.reasons.join('\n') } }[e.for]) : Effect.promise(() => next(e))),
        ),
        { signal: next.signal },
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

function armed($: EngineInterface, e: ToolCallInput, next: Next<'tool.call'>, command: string, notes: readonly string[]) {
    return Effect.gen(function* () {
        const { description = command, timeout = monitored } = yield* Effect.orDie(Schema.decodeUnknownEffect(Timed)(e));
        const answer = yield* Effect.promise(() => $.agent.call({ tool: 'Monitor', input: { command, description, timeout_ms: timeout }, ...(e.agentId === undefined ? {} : { agentId: e.agentId }) }, { signal: next.signal }));
        return answer.deny === undefined ? noted({ result: { stdout: answer.text ?? '', stderr: '', interrupted: false }, ...(answer.isError === true ? { isError: true } : {}) }, notes) : ({ deny: answer.deny } satisfies ToolCallResult);
    });
}

function called($: EngineInterface, e: ToolCallInput, next: Next<'tool.call'>, action: typeof Called.Type, policy: Policy, engine: Engine) {
    return engine.runPromise(
        Effect.gen(function* () {
            const judgment = yield* Session.use((session) => policy.judge(action, session.shell)).pipe(Effect.timeoutOrElse({ duration: Duration.millis(Math.max(0, next.budget.remainingMs - reserve)), orElse: () => policy.unjudged(action.agent, 'its hook ran out of time', 'call') }));
            const answered = (input: ToolCallInput) =>
                Effect.promise(() => next(input)).pipe(
                    Effect.tap((answer) => Effect.forEach(policy.hears('Task') ? Option.toArray(tasked(answer)) : [], (task) => Effect.forkIn(watched(policy, action, task), engine.scope), { discard: true })),
                    Effect.map((answer) => noted(answer, judgment.notes)),
                );
            const moved: Readonly<Record<string, (command: string) => ReturnType<typeof answered>>> = {
                notified: (command) => {
                    const { timeout: _timeout, ...untimed } = e;
                    return answered({ ...untimed, command, run_in_background: true });
                },
                streamed: (command) => armed($, e, next, command, judgment.notes),
            };
            const carried = ({ command, wait }: ReturnType<typeof Action.Shell>) =>
                Option.match(
                    Option.filter(Record.get(moved, wait), () => !(Action.$is('Shell')(action) && action.wait === wait)),
                    { onNone: () => answered({ ...e, command }), onSome: (move) => move(command) },
                );
            return yield* Judgment.$match(judgment, {
                Refuse: ({ reasons, notes }) => Effect.succeed<ToolCallResult>({ deny: [...reasons, whole, ...notes].join('\n') }),
                Pass: () => answered(e),
                Rewrite: ({ action: restated }) => (Action.$is('Shell')(restated) ? carried(restated) : answered(e)),
            });
        }),
        { signal: next.signal },
    );
}

function hooks(on: On, policy: Policy) {
    const live = MutableRef.make(Option.none<Engine>());
    on('session.start', ($, e, next) =>
        Option.match(MutableRef.getAndSet(live, Option.some(runtime($))), {
            onNone: () => next(e),
            onSome: (stale) => Promise.all([next(e), stale.dispose()]).then(([started]) => started),
        }),
    ).catch((_, e, next) => next(e));
    on('session.end', (_, e, next) => {
        Effect.runSync(policy.forget(Option.none()));
        return Option.match(e.reason === 'clear' ? Option.none() : MutableRef.getAndSet(live, Option.none()), {
            onNone: () => next(e),
            onSome: (engine) => Promise.all([next(e), engine.dispose()]).then(([ended]) => ended),
        });
    }).catch((_, e, next) => next(e));
    on('session.compact', (_, e, next) => {
        Effect.runSync(policy.forget(Option.fromUndefinedOr(e.agentId)));
        return next(e);
    }).catch((_, e, next) => next(e));
    on('vcs.isolate', (_, e, next) => Option.match(MutableRef.get(live), { onNone: () => next(e), onSome: (engine) => isolated(e, next, policy, engine) })).catch((_, e, next) => next(e));
    on('vcs.status', { vcs: inplace }, () => ({ changes: [], unpublished: 0 })).catch(() => ({ changes: [], unpublished: 0 }));
    on('vcs.release', { vcs: inplace }, () => ({ isReleased: true as const })).catch(() => ({ isReleased: true as const }));
    on('tool.call', ($, e, next) =>
        Option.match(Option.all([MutableRef.get(live), Option.filter(Schema.decodeUnknownOption(Called)(e), ({ _tag }) => policy.hears(_tag))]), {
            onNone: () => next(e),
            onSome: ([engine, action]) => called($, e, next, action, policy, engine),
        }),
    ).catch(($, e, next) =>
        next(e).then((answer) => {
            const why = (next.error.message ?? next.error.kind).split('\n', 1).join('');
            $.ui.log(`policy did not judge this call and let it run: ${why}`, { to: 'debug' });
            return noted(answer, Effect.runSync(policy.unjudged(Option.fromUndefinedOr(e.agentId), why, 'agent')).notes);
        }),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { hooks };
