/// <reference path="./types/claude-code.d.ts" preserve="true" />
import type { EngineInterface, Next, On, ToolCallInput, ToolCallResult, VcsIsolateInput } from 'claude-code';
import { Duration, Effect, Match, MutableRef, Option, Schedule, Schema, SchemaTransformation } from 'effect';
import { runtime, Session } from '#/claude/platform';
import { Action, Decision } from '#/decision/action';
import type { Policy } from '#/policy';

// --- [TYPES] ---------------------------------------------------------------------------

type Runtime = ReturnType<typeof runtime>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const reserve = 500;
const monitorTimeout = 300_000;
const vcs = 'inplace';
const nothingRan = 'The call was refused whole, so none of it ran.';

// --- [MODELS] --------------------------------------------------------------------------

const caller = { call: Schema.String, agent: Schema.OptionFromOptionalKey(Schema.String) };
const keys = { _tag: 'tool', call: 'tool_use_id', agent: 'agentId' } as const;
const ToolCall = Schema.Union([
    Schema.Struct({
        _tag: Schema.Literal('Bash').transform('Shell'),
        ...caller,
        command: Schema.String,
        run: Schema.Union([Schema.Boolean, Schema.Literal('true')]).pipe(
            Schema.decodeTo(Schema.Literals(['foreground', 'background']), SchemaTransformation.transform({ decode: (flag) => (flag === false ? 'foreground' : 'background'), encode: (run) => run === 'background' })),
            Schema.withDecodingDefaultKey(Effect.succeed(false)),
        ),
    }).pipe(Schema.encodeKeys({ ...keys, run: 'run_in_background' })),
    Schema.Struct({ _tag: Schema.Literal('Monitor').transform('Shell'), ...caller, command: Schema.String, run: Schema.tagDefaultOmit('monitored') }).pipe(Schema.encodeKeys(keys)),
    Schema.Struct({ _tag: Schema.Literal('Monitor').transform('Fetch'), ...caller, url: Schema.Struct({ url: Schema.String }).pipe(Schema.decodeTo(Schema.String, SchemaTransformation.transform({ decode: ({ url }) => url, encode: (url) => ({ url }) }))) }).pipe(Schema.encodeKeys({ ...keys, url: 'ws' })),
    Schema.TaggedStruct('Read', { ...caller, path: Schema.String }).pipe(Schema.encodeKeys({ ...keys, path: 'file_path' })),
    Schema.TaggedStruct('Write', { ...caller, path: Schema.String, content: Schema.String }).pipe(Schema.encodeKeys({ ...keys, path: 'file_path' })),
    Schema.TaggedStruct('Edit', { ...caller, path: Schema.String, old: Schema.String, new: Schema.String }).pipe(Schema.encodeKeys({ ...keys, path: 'file_path', old: 'old_string', new: 'new_string' })),
    Schema.Struct({ _tag: Schema.Literal('WebFetch').transform('Fetch'), ...caller, url: Schema.String }).pipe(Schema.encodeKeys(keys)),
    Schema.Struct({ _tag: Schema.Literal('Agent').transform('Spawn'), ...caller, prompt: Schema.String, model: Schema.OptionFromOptionalKey(Schema.String) }).pipe(Schema.encodeKeys(keys)),
    Schema.StructWithRest(Schema.Struct({ tool: Schema.TemplateLiteralParser(['mcp__', Schema.NonEmptyString, '__', Schema.NonEmptyString]), tool_use_id: Schema.String, agentId: Schema.OptionFromOptionalKey(Schema.String) }), [Schema.Record(Schema.String, Schema.Unknown)]).pipe(
        Schema.decodeTo(
            Schema.toType(Schema.TaggedStruct('Mcp', { ...caller, server: Schema.String, tool: Schema.String, input: Schema.Record(Schema.String, Schema.Unknown) })),
            SchemaTransformation.transform({
                decode: ({ tool: [, server, , tool], tool_use_id: call, agentId: agent, ...input }) => ({ _tag: 'Mcp' as const, call, agent, server, tool, input }),
                encode: ({ call, agent, server, tool, input }) => ({ ...input, tool: ['mcp__', server, '__', tool] as const, tool_use_id: call, agentId: agent }),
            }),
        ),
    ),
]);
const Inherited = Schema.Struct({ description: Schema.optionalKey(Schema.String), timeout: Schema.optionalKey(Schema.Finite) });
const Background = Schema.Struct({ result: Schema.Struct({ backgroundTaskId: Schema.String }), text: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const annotate = (result: ToolCallResult, notes: readonly string[]): ToolCallResult => ('deny' in result || notes.length === 0 ? result : { ...result, context: [...(result.context ?? []), ...notes] });
const backgroundTask = (result: ToolCallResult) => Option.flatMap(Schema.decodeUnknownOption(Background)(result), ({ result: { backgroundTaskId: id }, text }) => Option.map(Option.fromNullishOr(new RegExp(String.raw`\S*/tasks/${RegExp.escape(id)}\.output`, 'u').exec(text)), ([output]) => ({ id, output })));
const watch = (policy: Policy, { call, agent }: typeof ToolCall.Type, task: Option.Option.Value<ReturnType<typeof backgroundTask>>) =>
    Session.use((session) =>
        Effect.flatMap(session.resting(agent), (resting) => policy.decide(Action.Task({ ...task, call, agent, resting, again: Duration.infinity }), session.environment)).pipe(
            Effect.tap(({ notes }) => Effect.forEach(notes, (text) => session.tell(agent, text), { discard: true })),
            Effect.map((decision) =>
                Match.value(decision).pipe(
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
const isolate = (e: VcsIsolateInput, next: Next<'vcs.isolate'>, policy: Policy, live: Runtime) =>
    live.runPromise(
        Effect.flatMap(
            Session.use((session) => policy.decide(Action.Isolate({ call: e.name, agent: Option.fromUndefinedOr(e.parentAgentId), from: e.from }), session.environment)),
            (decision) => (Decision.$is('Deny')(decision) ? Effect.succeed({ agent: { path: e.from, vcs }, session: { deny: decision.reasons.join('\n') } }[e.for]) : Effect.promise(() => next(e))),
        ),
        { signal: next.signal },
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

function monitor($: EngineInterface, e: ToolCallInput, next: Next<'tool.call'>, command: string, notes: readonly string[]) {
    return Effect.gen(function* () {
        const { description = command, timeout = monitorTimeout } = yield* Effect.orDie(Schema.decodeUnknownEffect(Inherited)(e));
        const result = yield* Effect.promise(() => $.agent.call({ tool: 'Monitor', input: { command, description, timeout_ms: timeout }, ...(e.agentId === undefined ? {} : { agentId: e.agentId }) }, { signal: next.signal }));
        return result.deny === undefined ? annotate({ result: { stdout: result.text ?? '', stderr: '', interrupted: false }, ...(result.isError === true ? { isError: true } : {}) }, notes) : ({ deny: result.deny } satisfies ToolCallResult);
    });
}

function answer($: EngineInterface, e: ToolCallInput, next: Next<'tool.call'>, action: typeof ToolCall.Type, policy: Policy, live: Runtime) {
    return live.runPromise(
        Effect.gen(function* () {
            const decision = yield* Session.use((session) => policy.decide(action, session.environment)).pipe(Effect.timeoutOrElse({ duration: Duration.millis(Math.max(0, next.budget.remainingMs - reserve)), orElse: () => policy.undecided(action.agent, 'its hook ran out of time', 'call') }));
            const run = (input: ToolCallInput) =>
                Effect.promise(() => next(input)).pipe(
                    Effect.tap((result) => Effect.forEach(policy.appliesTo('Task') ? Option.toArray(backgroundTask(result)) : [], (task) => Effect.forkIn(watch(policy, action, task), live.scope), { discard: true })),
                    Effect.map((result) => annotate(result, decision.notes)),
                );
            const dispatch = ({ command, run: mode }: ReturnType<typeof Action.Shell>) =>
                Match.value({ kept: Action.$is('Shell')(action) && action.run === mode, mode }).pipe(
                    Match.when({ kept: false, mode: 'background' }, () => {
                        const { timeout: _timeout, ...untimed } = e;
                        return run({ ...untimed, command, run_in_background: true });
                    }),
                    Match.when({ kept: false, mode: 'monitored' }, () => monitor($, e, next, command, decision.notes)),
                    Match.orElse(() => run({ ...e, command })),
                );
            return yield* Decision.$match(decision, {
                Deny: ({ reasons, notes }) => Effect.succeed<ToolCallResult>({ deny: [...reasons, nothingRan, ...notes].join('\n') }),
                Allow: () => run(e),
                Rewrite: ({ action: restated }) => (Action.$is('Shell')(restated) ? dispatch(restated) : run(e)),
            });
        }),
        { signal: next.signal },
    );
}

function hooks(on: On, policy: Policy) {
    const live = MutableRef.make(Option.none<Runtime>());
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
            onSome: (current) => Promise.all([next(e), current.dispose()]).then(([ended]) => ended),
        });
    }).catch((_, e, next) => next(e));
    on('session.compact', (_, e, next) => {
        Effect.runSync(policy.forget(Option.fromUndefinedOr(e.agentId)));
        return next(e);
    }).catch((_, e, next) => next(e));
    on('vcs.isolate', (_, e, next) => Option.match(MutableRef.get(live), { onNone: () => next(e), onSome: (current) => isolate(e, next, policy, current) })).catch((_, e, next) => next(e));
    on('vcs.status', { vcs }, () => ({ changes: [], unpublished: 0 })).catch(() => ({ changes: [], unpublished: 0 }));
    on('vcs.release', { vcs }, () => ({ isReleased: true as const })).catch(() => ({ isReleased: true as const }));
    on('tool.call', ($, e, next) =>
        Option.match(Option.all([MutableRef.get(live), Option.filter(Schema.decodeUnknownOption(ToolCall)(e), ({ _tag }) => policy.appliesTo(_tag))]), {
            onNone: () => next(e),
            onSome: ([current, action]) => answer($, e, next, action, policy, current),
        }),
    ).catch(($, e, next) =>
        next(e).then((result) => {
            const why = (next.error.message ?? next.error.kind).split('\n', 1).join('');
            $.ui.log(`policy did not judge this call and let it run: ${why}`, { to: 'debug' });
            return annotate(result, Effect.runSync(policy.undecided(Option.fromUndefinedOr(e.agentId), why, 'agent')).notes);
        }),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { hooks };
