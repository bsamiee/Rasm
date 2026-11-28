import { Action, Decision } from '@rasm/policy/action';
import type { Policy } from '@rasm/policy/policy';
import type { Frozen, On, ToolCallInput, ToolCallResult } from 'claude-code';
import { Cause, Duration, Effect, type Layer, Match, Option, Path, Predicate, Schedule, Schema, SchemaGetter, Struct } from 'effect';
import { answer, close, hosted, open, type Proceed, recover, track } from '#/engine/host';
import { Engine, Notice } from '#/engine/interface';
import { layer, Session } from '#/policy/session';

// --- [CONSTANTS] -----------------------------------------------------------------------

const reserve = 500;

// --- [MODELS] --------------------------------------------------------------------------

const caller = { tool_use_id: Schema.String, agentId: Schema.optional(Schema.String) };
const row = <const Fields extends Schema.Struct.Fields>(fields: Fields) => {
    const from = Schema.StructWithRest(Schema.Struct({ ...caller, ...fields }), [Action.cases.Mcp.fields.input]);
    return (fold: (input: (typeof from)['Type'], by: Pick<Action, 'agent' | 'call'>) => Action) =>
        from.pipe(Schema.decodeTo(Schema.toType(Action), { decode: SchemaGetter.transform((input) => fold(input, { call: input.tool_use_id, agent: Option.fromUndefinedOr(input.agentId) })), encode: SchemaGetter.forbiddenEncoding }));
};
const ToolCall = Schema.Union([
    row({ tool: Schema.Literal('Bash'), command: Schema.String, run_in_background: Schema.optional(Schema.Literals([true, false, 'true', 'false'])) })(({ command, run_in_background: flag }, by) => Action.cases.Shell.make({ ...by, command, run: flag === true || flag === 'true' ? 'background' : 'foreground' })),
    row({ tool: Schema.Literal('Monitor'), command: Schema.String })(({ command }, by) => Action.cases.Shell.make({ ...by, command, run: 'monitored' })),
    row({ tool: Schema.Literal('Monitor'), ws: Schema.Struct({ url: Schema.String }) })(({ ws: { url } }, by) => Action.cases.Fetch.make({ ...by, url })),
    row({ tool: Schema.Literal('Read'), file_path: Schema.String })(({ file_path: path }, by) => Action.cases.Read.make({ ...by, path })),
    row({ tool: Schema.Literal('Write'), file_path: Schema.String, content: Schema.String })(({ file_path: path, content }, by) => Action.cases.Write.make({ ...by, path, content })),
    row({ tool: Schema.Literal('Edit'), file_path: Schema.String, old_string: Schema.String, new_string: Schema.String })(({ file_path: path, old_string, new_string }, by) => Action.cases.Edit.make({ ...by, path, old: old_string, new: new_string })),
    row({ tool: Schema.Literal('WebFetch'), url: Schema.String })(({ url }, by) => Action.cases.Fetch.make({ ...by, url })),
    row({ tool: Schema.Literal('Agent'), prompt: Schema.String, model: Schema.optional(Schema.String) })(({ prompt, model }, by) => Action.cases.Spawn.make({ ...by, prompt, model: Option.fromUndefinedOr(model) })),
    row({ tool: Schema.TemplateLiteralParser(['mcp__', Schema.String.check(Schema.isPattern(/^(?:(?!__)[^])+$/u)), '__', Schema.String]) })(({ tool: [, server, , tool], ...input }, by) => Action.cases.Mcp.make({ ...by, server, tool, input: Struct.omit(input, [...Struct.keys(caller), 'consent', 'requestMeta']) })),
]);
const Background = Schema.Struct({ result: Schema.Struct({ backgroundTaskId: Schema.String }), text: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const decide = (policy: Policy, action: Action) =>
    Session.use((session) => session.shell(action.agent)).pipe(
        Effect.flatMap(({ environment, path }) => Effect.provideService(policy.decide(action, environment), Path.Path, path)),
        Effect.tap(({ faults }) => Effect.forEach(faults, (fault) => Effect.logWarning(fault), { discard: true })),
    );
const watch = Effect.fnUntraced(function* (policy: Policy, { call, agent }: Action, task: Pick<typeof Action.cases.Task.Type, 'id' | 'output'>) {
    const [engine, session] = yield* Effect.all([Engine, Session]);
    yield* session.watch(
        Effect.flatMap(engine.isRunning(agent), (running) => Option.match(agent, { onNone: () => Effect.succeedSome(!running), onSome: (id) => Effect.map(engine.findAgent(id), Option.as(!running)) })).pipe(
            Effect.flatMap(
                Option.match({
                    onNone: () => Effect.succeed(Duration.infinity),
                    onSome: (resting) =>
                        decide(policy, Action.cases.Task.make({ ...task, call, agent, resting, again: Duration.infinity })).pipe(
                            Effect.tap(({ notes }) => Effect.forEach(notes, (text) => engine.deliver(Notice.Message({ text, agent })), { discard: true })),
                            Effect.map((decision) => (Decision.$is('Rewrite')(decision) && Action.guards.Task(decision.action) ? decision.action.again : Duration.infinity)),
                        ),
                }),
            ),
            Effect.repeat({
                while: Duration.isFinite,
                schedule: Schedule.forever.pipe(
                    Schedule.setInputType<Duration.Duration>(),
                    Schedule.addDelay(({ input }) => Effect.succeed(input)),
                ),
            }),
            Effect.catchCause((cause) => Effect.logWarning('policy stopped watching task', task.id, cause)),
        ),
    );
});
const monitor = (e: Frozen<ToolCallInput>, command: string) =>
    Engine.use((engine) =>
        Effect.map(engine.callTool({ tool: 'Monitor', input: { command, description: Predicate.isString(e['description']) ? e['description'] : command, ...(Predicate.isNumber(e['timeout']) ? { timeout_ms: e['timeout'] } : {}) }, ...Struct.pick(e, ['agentId']) }), (called): ToolCallResult =>
            Match.value(called).pipe(
                Match.withReturnType<ToolCallResult>(),
                Match.when({ deny: Match.string }, (refused) => refused),
                Match.when({ isError: true }, ({ text }) => ({ isError: true, result: text, ...(text === undefined ? {} : { text }) })),
                Match.orElse(({ text }) => ({ result: { stdout: text ?? '', stderr: '', interrupted: false } })),
            ),
        ),
    );
const judge = Effect.fnUntraced(function* (policy: Policy, e: Frozen<ToolCallInput>, next: Proceed<'tool.call'>, action: Action) {
    const decision = yield* Effect.raceFirst(
        decide(policy, action),
        Effect.suspend(() => Effect.sleep(Duration.millis(Math.max(0, next.budget.remainingMs - reserve)))).pipe(Effect.repeat({ until: () => next.budget.remainingMs <= reserve }), Effect.andThen(new Cause.TimeoutError('its hook budget ran out before the rules settled'))),
    ).pipe(Effect.catchCause((cause) => Effect.as(Effect.logWarning(`policy did not judge ${action._tag} call ${action.call} and let it run`, cause), Decision.Allow({ notes: [], faults: [] }))));
    const result = yield* Decision.$match(decision, {
        Deny: ({ reasons }) => Effect.succeed<ToolCallResult>({ deny: [...reasons, 'The call was refused whole, so none of it ran.'].join('\n') }),
        Allow: () => next(e),
        Rewrite: ({ action: restated }) =>
            Match.value({ from: action, to: restated }).pipe(
                Match.when({ from: { _tag: 'Shell', run: 'foreground' }, to: { _tag: 'Shell', run: 'background' } }, ({ to: { command } }) => {
                    const { timeout: _timeout, ...untimed } = e;
                    return next({ ...untimed, command, run_in_background: true });
                }),
                Match.when({ from: { _tag: 'Shell', run: Match.is('foreground', 'background') }, to: { _tag: 'Shell', run: 'monitored' } }, ({ to: { command } }) => monitor(e, command)),
                Match.when({ to: { _tag: 'Shell' } }, ({ to: { command } }) => next({ ...e, command })),
                Match.orElse(() => next(e)),
            ),
    });
    yield* Schema.decodeUnknownOption(Background)(result).pipe(
        Option.filter(() => policy.appliesTo('Task')),
        Option.flatMap(({ result: { backgroundTaskId: id }, text }) => Option.map(Option.fromNullishOr(new RegExp(String.raw`^[^]*(?<=^|[\s"'\x60(])(?<output>/[^\n"'\x60]*/tasks/${RegExp.escape(id)}\.output)`, 'u').exec(text)?.groups?.['output']), (output) => watch(policy, action, { id, output }))),
        Effect.transposeOption,
    );
    return result.deny !== undefined || decision.notes.length === 0 ? result : { ...result, context: [...(result.context ?? []), ...decision.notes] };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

function enforce(on: On, policy: Policy) {
    const instance = hosted<Layer.Success<ReturnType<typeof layer>>, Layer.Error<ReturnType<typeof layer>>>(Option.none());
    on('session.start', ($, e, next) => open($, instance, layer($), undefined, e, next)).catch(recover);
    on('session.end', ($, e, next) => close($, instance, layer($), (ended, proceed) => Effect.andThen(policy.clear(), proceed(ended)), e, next)).catch(recover);
    on('turn.start', (_, e, next) => {
        track(instance, e.agentId, true);
        return next(e);
    }).catch(recover);
    on('turn.complete', (_, e, next) => {
        track(instance, e.agentId, false);
        return next(e);
    }).catch(recover);
    on('session.compact', ($, e, next) => answer($, instance, layer($), (compacted, proceed) => Effect.tap(proceed(compacted), ({ messages }) => Effect.when(policy.forget(Option.fromUndefinedOr(compacted.agentId)), Effect.succeed(messages !== undefined && compacted.trigger !== 'precompute'))), e, next)).catch(recover);
    on('tool.call', ($, e, next) =>
        answer(
            $,
            instance,
            layer($),
            (called, proceed) =>
                Option.match(
                    Option.filter(Schema.decodeUnknownOption(ToolCall)(called), ({ _tag }) => policy.appliesTo(_tag)),
                    {
                        onNone: () => proceed(called),
                        onSome: (action) => judge(policy, called, proceed, action),
                    },
                ),
            e,
            next,
        ),
    ).catch(recover);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { enforce };
