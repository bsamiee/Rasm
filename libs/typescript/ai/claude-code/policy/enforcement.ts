import { Action, Decision } from '@rasm/policy/action';
import type { Policy } from '@rasm/policy/policy';
import type { Frozen, On, ToolCallInput, ToolCallResult } from 'claude-code';
import { Cause, Duration, Effect, Match, Option, Path, Predicate, Schedule, Schema, SchemaGetter, Struct } from 'effect';
import { host, type Proceed } from '#/engine/host';
import { Engine, Notice } from '#/engine/interface';
import { Session } from '#/policy/session';

// --- [CONSTANTS] -----------------------------------------------------------------------

const reserve = 500;
const mcpName = /^mcp__(?<server>.+?)__(?<tool>.+)$/su;
const reserved = ['tool', 'tool_use_id', 'agentId', 'consent', 'requestMeta'] as const;
const outputLead = /^[^]*(?<=^|[\s"'`(])/u.source;
const outputFolder = /\/[^\n"'`]*\/tasks\//u.source;

// --- [MODELS] --------------------------------------------------------------------------

const caller = { tool_use_id: Schema.String, agentId: Schema.optional(Schema.String) };
const row = <From extends Schema.Top & { readonly Type: Schema.Struct.Type<typeof caller> }>(from: From, fold: (input: From['Type'], by: Pick<Action, 'agent' | 'call'>) => Action) =>
    from.pipe(Schema.decodeTo(Schema.toType(Action), { decode: SchemaGetter.transform((input) => fold(input, { call: input.tool_use_id, agent: Option.fromUndefinedOr(input.agentId) })), encode: SchemaGetter.forbiddenEncoding }));
const ToolCall = Schema.Union([
    row(Schema.Struct({ tool: Schema.Literal('Bash'), ...caller, command: Schema.String, run_in_background: Schema.optional(Schema.Union([Schema.Boolean, Schema.Literals(['true', 'false'])])) }), ({ command, run_in_background: flag }, by) =>
        Action.cases.Shell.make({ ...by, command, run: flag === true || flag === 'true' ? 'background' : 'foreground' }),
    ),
    row(Schema.Struct({ tool: Schema.Literal('Monitor'), ...caller, command: Schema.String }), ({ command }, by) => Action.cases.Shell.make({ ...by, command, run: 'monitored' })),
    row(Schema.Struct({ tool: Schema.Literal('Monitor'), ...caller, ws: Schema.Struct({ url: Schema.String }) }), ({ ws: { url } }, by) => Action.cases.Fetch.make({ ...by, url })),
    row(Schema.Struct({ tool: Schema.Literal('Read'), ...caller, file_path: Schema.String }), ({ file_path: path }, by) => Action.cases.Read.make({ ...by, path })),
    row(Schema.Struct({ tool: Schema.Literal('Write'), ...caller, file_path: Schema.String, content: Schema.String }), ({ file_path: path, content }, by) => Action.cases.Write.make({ ...by, path, content })),
    row(Schema.Struct({ tool: Schema.Literal('Edit'), ...caller, file_path: Schema.String, old_string: Schema.String, new_string: Schema.String }), ({ file_path: path, old_string, new_string }, by) => Action.cases.Edit.make({ ...by, path, old: old_string, new: new_string })),
    row(Schema.Struct({ tool: Schema.Literal('WebFetch'), ...caller, url: Schema.String }), ({ url }, by) => Action.cases.Fetch.make({ ...by, url })),
    row(Schema.Struct({ tool: Schema.Literal('Agent'), ...caller, prompt: Schema.String, model: Schema.optional(Schema.String) }), ({ prompt, model }, by) => Action.cases.Spawn.make({ ...by, prompt, model: Option.fromUndefinedOr(model) })),
    row(Schema.StructWithRest(Schema.Struct({ tool: Schema.String.check(Schema.isPattern(mcpName)), ...caller }), [Schema.Record(Schema.String, Schema.Unknown)]), (input, by) => {
        const { server = '', tool = '' } = mcpName.exec(input.tool)?.groups ?? {};
        return Action.cases.Mcp.make({ ...by, server, tool, input: Struct.omit(input, reserved) });
    }),
]);
const Background = Schema.Struct({ result: Schema.Struct({ backgroundTaskId: Schema.String }), text: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const annotate = (result: ToolCallResult, notes: readonly string[]): ToolCallResult => (result.deny !== undefined || notes.length === 0 ? result : { ...result, context: [...(result.context ?? []), ...notes] });
const backgroundTask = (result: ToolCallResult) =>
    Option.flatMap(Schema.decodeUnknownOption(Background)(result), ({ result: { backgroundTaskId: id }, text }) => Option.map(Option.fromNullishOr(new RegExp(`${outputLead}(?<output>${outputFolder}${RegExp.escape(id)}\\.output)`, 'u').exec(text)?.groups), ({ output = '' }) => ({ id, output })));
const decide = (policy: Policy, action: Action) =>
    Session.use((session) => Effect.flatMap(session.shell(action.agent), ({ environment, path }) => Effect.provideService(policy.decide(action, environment), Path.Path, path)).pipe(Effect.tap(({ faults }) => Effect.forEach(faults, (fault) => Effect.logWarning(fault), { discard: true }))));
const deadline = (budget: { readonly remainingMs: number }) =>
    Effect.suspend(() => Effect.sleep(Duration.millis(Math.max(0, budget.remainingMs - reserve)))).pipe(Effect.repeat({ until: () => budget.remainingMs <= reserve }), Effect.andThen(Effect.fail(new Cause.TimeoutError('its hook budget ran out before the rules settled'))));
const watch = Effect.fnUntraced(function* (policy: Policy, { call, agent }: Action, task: Option.Option.Value<ReturnType<typeof backgroundTask>>) {
    const [engine, session] = yield* Effect.all([Engine, Session]);
    const resting = Effect.flatMap(engine.isRunning(agent), (running) => Option.match(agent, { onNone: () => Effect.succeedSome(!running), onSome: (id) => Effect.map(engine.findAgent(id), Option.as(!running)) }));
    const check = (rested: boolean) =>
        Effect.flatMap(decide(policy, Action.cases.Task.make({ ...task, call, agent, resting: rested, again: Duration.infinity })), (decision) =>
            Effect.as(
                Effect.forEach(decision.notes, (text) => engine.deliver(Notice.Message({ text, agent })), { discard: true }),
                Decision.$is('Rewrite')(decision) && Action.guards.Task(decision.action) ? decision.action.again : Duration.infinity,
            ),
        );
    yield* session.watch(
        resting.pipe(
            Effect.flatMap(Option.match({ onNone: () => Effect.succeed(Duration.infinity), onSome: check })),
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
const monitor = (e: Frozen<ToolCallInput>, command: string, notes: readonly string[]) =>
    Engine.use((engine) =>
        Effect.map(
            engine.callTool({ tool: 'Monitor', input: { command, description: Predicate.isString(e['description']) ? e['description'] : command, ...(Predicate.isNumber(e['timeout']) ? { timeout_ms: e['timeout'] } : {}) }, ...(e.agentId === undefined ? {} : { agentId: e.agentId }) }),
            ({ deny, isError, text }): ToolCallResult =>
                Match.value({ deny, isError }).pipe(
                    Match.when({ deny: Match.string }, (refused) => ({ deny: refused.deny })),
                    Match.when({ isError: true }, () => annotate({ isError: true, result: text, ...(text === undefined ? {} : { text }) }, notes)),
                    Match.orElse(() => annotate({ result: { stdout: text ?? '', stderr: '', interrupted: false } }, notes)),
                ),
        ),
    );

const judge = Effect.fnUntraced(function* (policy: Policy, e: Frozen<ToolCallInput>, next: Proceed<'tool.call'>, action: Action) {
    const decision = yield* Effect.raceFirst(decide(policy, action), deadline(next.budget)).pipe(Effect.catchCause((cause) => Effect.as(Effect.logWarning(`policy did not judge ${action._tag} call ${action.call} and let it run`, cause), Decision.Allow({ notes: [], faults: [] }))));
    const run = (input: ToolCallInput) =>
        next(input).pipe(
            Effect.tap((result) => Effect.forEach(Option.toArray(Option.filter(backgroundTask(result), () => policy.appliesTo('Task'))), (task) => watch(policy, action, task), { discard: true })),
            Effect.map((result) => annotate(result, decision.notes)),
        );
    const dispatch = ({ command, run: mode }: typeof Action.cases.Shell.Type) =>
        Match.value({ kept: Action.guards.Shell(action) && action.run === mode, mode }).pipe(
            Match.when({ kept: false, mode: 'background' }, () => {
                const { timeout: _timeout, ...untimed } = e;
                return run({ ...untimed, command, run_in_background: true });
            }),
            Match.when({ kept: false, mode: 'monitored' }, () => monitor(e, command, decision.notes)),
            Match.orElse(() => run({ ...e, command })),
        );
    return yield* Decision.$match(decision, {
        Deny: ({ reasons }) => Effect.succeed<ToolCallResult>({ deny: [...reasons, 'The call was refused whole, so none of it ran.'].join('\n') }),
        Allow: () => run(e),
        Rewrite: ({ action: restated }) => (Action.guards.Shell(restated) ? dispatch(restated) : run(e)),
    });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

function enforce(on: On, policy: Policy) {
    const judged = (e: Frozen<ToolCallInput>) => Option.filter(Schema.decodeOption(ToolCall)(e), ({ _tag }) => policy.appliesTo(_tag));
    host(on, {
        layer: Session.layer,
        hooks: {
            'session.end': (e, next) => Effect.andThen(policy.clear(), next(e)),
            'session.compact': (e, next) => Effect.tap(next(e), ({ messages }) => Effect.when(policy.forget(Option.fromUndefinedOr(e.agentId)), Effect.succeed(messages !== undefined && e.trigger !== 'precompute'))),
            'tool.call': (e, next) =>
                Option.match(judged(e), {
                    onNone: () => next(e),
                    onSome: (action) => judge(policy, e, next, action),
                }),
        },
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { enforce };
