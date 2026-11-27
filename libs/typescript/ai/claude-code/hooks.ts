import { Action, Decision } from '@rasm/policy/action';
import type { Policy } from '@rasm/policy/policy';
import type { EngineInterface, Next, On, ToolCallInput, ToolCallResult } from 'claude-code';
import { Cause, Duration, Effect, Function, type Layer, Match, MutableRef, Option, Path, Predicate, Schedule, Schema, SchemaGetter, Struct } from 'effect';
import { Notice, request, runtime, say, Session, session } from '#/platform';

// --- [TYPES] ---------------------------------------------------------------------------

type Runtime = ReturnType<typeof runtime<Layer.Success<ReturnType<typeof session>>, Layer.Error<ReturnType<typeof session>>>>;
type Live = MutableRef.MutableRef<Option.Option<Runtime>>;
type Busy = MutableRef.MutableRef<boolean>;

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
    Session.use((opened) => Effect.flatMap(opened.acting(action.agent), ({ environment, path }) => Effect.provideService(policy.decide(action, environment), Path.Path, path)).pipe(Effect.tap(({ faults }) => Effect.forEach(faults, (fault) => Effect.logWarning(fault), { discard: true }))));
const deadline = (next: Pick<Next<'tool.call'>, 'budget'>) =>
    Effect.suspend(() => Effect.sleep(Duration.millis(Math.max(0, next.budget.remainingMs - reserve)))).pipe(Effect.repeat({ until: () => next.budget.remainingMs <= reserve }), Effect.andThen(Effect.fail(new Cause.TimeoutError('its hook budget ran out before the rules settled'))));

// --- [COMPOSITION] ---------------------------------------------------------------------

function current($: EngineInterface, live: Live) {
    return Option.getOrElse(MutableRef.get(live), () => {
        const made = runtime($, session($));
        MutableRef.set(live, Option.some(made));
        made.runPromise(Effect.void).catch(() => {
            MutableRef.compareAndSet(live, Option.some(made), Option.none());
            return made.dispose();
        });
        return made;
    });
}

function watch($: EngineInterface, policy: Policy, busy: Busy, { call, agent }: Action, task: Option.Option.Value<ReturnType<typeof backgroundTask>>) {
    const tell = say($);
    const check = (resting: boolean) =>
        Effect.flatMap(decide(policy, Action.cases.Task.make({ ...task, call, agent, resting, again: Duration.infinity })), (decision) =>
            Effect.as(
                Effect.forEach(decision.notes, (text) => tell(Notice.Model({ text, agent, idle: true })), { discard: true }),
                Decision.$is('Rewrite')(decision) && Action.guards.Task(decision.action) ? decision.action.again : Duration.infinity,
            ),
        );
    return Session.use((opened) =>
        opened.watch(
            Effect.suspend(() => Option.match(agent, { onNone: () => Effect.succeedSome(!MutableRef.get(busy)), onSome: opened.resting })).pipe(
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
        ),
    );
}

function monitor($: EngineInterface, e: ToolCallInput, next: Next<'tool.call'>, command: string, notes: readonly string[]) {
    const { description, timeout } = e;
    const input = { command, description: Predicate.isString(description) ? description : command, ...(Predicate.isNumber(timeout) ? { timeout_ms: timeout } : {}) };
    return Effect.map(
        request('agent', 'call', command, () => $.agent.call({ tool: 'Monitor', input, ...(e.agentId === undefined ? {} : { agentId: e.agentId }) }, { signal: next.signal })),
        ({ deny, isError, text }): ToolCallResult =>
            Match.value({ deny, isError }).pipe(
                Match.when({ deny: Match.string }, (refused) => ({ deny: refused.deny })),
                Match.when({ isError: true }, () => annotate({ isError: true, result: text, ...(text === undefined ? {} : { text }) }, notes)),
                Match.orElse(() => annotate({ result: { stdout: text ?? '', stderr: '', interrupted: false } }, notes)),
            ),
    );
}

function answer($: EngineInterface, e: ToolCallInput, next: Next<'tool.call'>, action: Action, policy: Policy, live: Live, busy: Busy) {
    return current($, live).runPromise(
        Effect.gen(function* () {
            const decision = yield* Effect.raceFirst(decide(policy, action), deadline(next)).pipe(Effect.catchCause((cause) => Effect.as(Effect.logWarning(`policy did not judge ${action._tag} call ${action.call} and let it run`, cause), Decision.Allow({ notes: [], faults: [] }))));
            const run = (input: ToolCallInput) =>
                request('Engine', 'next', input.tool, () => next(input)).pipe(
                    Effect.tap((result) =>
                        Option.match(
                            Option.filter(backgroundTask(result), () => policy.appliesTo('Task')),
                            { onNone: () => Effect.void, onSome: (task) => watch($, policy, busy, action, task) },
                        ),
                    ),
                    Effect.map((result) => annotate(result, decision.notes)),
                );
            const dispatch = ({ command, run: mode }: typeof Action.cases.Shell.Type) =>
                Match.value({ kept: Action.guards.Shell(action) && action.run === mode, mode }).pipe(
                    Match.when({ kept: false, mode: 'background' }, () => {
                        const { timeout: _timeout, ...untimed } = e;
                        return run({ ...untimed, command, run_in_background: true });
                    }),
                    Match.when({ kept: false, mode: 'monitored' }, () => monitor($, e, next, command, decision.notes)),
                    Match.orElse(() => run({ ...e, command })),
                );
            return yield* Decision.$match(decision, {
                Deny: ({ reasons }) => Effect.succeed<ToolCallResult>({ deny: [...reasons, 'The call was refused whole, so none of it ran.'].join('\n') }),
                Allow: () => run(e),
                Rewrite: ({ action: restated }) => (Action.guards.Shell(restated) ? dispatch(restated) : run(e)),
            });
        }),
        { signal: next.signal },
    );
}

function hooks(on: On, policy: Policy) {
    const live: Live = MutableRef.make(Option.none());
    const busy: Busy = MutableRef.make(false);
    const judged = (e: ToolCallInput) => Option.filter(Schema.decodeOption(ToolCall)(e), ({ _tag }) => policy.appliesTo(_tag));
    on('session.end', (_, e, next) => {
        Effect.runSync(policy.clear());
        MutableRef.set(busy, false);
        return Promise.all([next(e), ...Option.toArray(Option.map(MutableRef.getAndSet(live, Option.none()), (ended) => ended.dispose()))]).then(([ended]) => ended);
    }).catch((_, e, next) => next(e));
    on('session.compact', (_, e, next) => next(e).then((result) => Effect.runSync(Effect.as(result.messages === undefined || e.trigger === 'precompute' ? Effect.void : policy.forget(Option.fromUndefinedOr(e.agentId)), result)))).catch((_, e, next) => next(e));
    if (policy.appliesTo('Task')) {
        on('turn.start', (_, e, next) => {
            MutableRef.update(busy, (running) => running || e.agentId === undefined);
            return next(e);
        }).catch((_, e, next) => next(e));
        on('turn.complete', (_, e, next) => {
            MutableRef.update(busy, (running) => running && e.agentId !== undefined);
            return next(e);
        }).catch((_, e, next) => next(e));
    }
    on('tool.call', ($, e, next) => Option.match(judged(e), { onNone: () => next(e), onSome: (action) => answer($, e, next, action, policy, live, busy) })).catch(($, e, next) => {
        Option.match(
            Option.filter(judged(e), () => !next.called && !next.signal.aborted),
            {
                onNone: Function.constVoid,
                onSome: (action) => $.ui.log(`policy did not judge ${action._tag} call ${action.call} and let it run: its hook ${next.error.kind === 'throw' ? 'threw' : 'ran out of time'}: ${next.error.message ?? next.error.kind}`, { to: 'debug' }),
            },
        );
        return next(e);
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { hooks };
