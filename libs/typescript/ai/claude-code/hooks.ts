import { Action, Decision } from '@rasm/policy/action';
import type { Policy } from '@rasm/policy/policy';
import type { EngineInterface, Next, On, ToolCallInput, ToolCallResult } from 'claude-code';
import { Array, Cause, Clock, Duration, Effect, Exit, Function, Match, MutableRef, Option, Path, Predicate, Record, Schedule, Schema, SchemaGetter, Struct } from 'effect';
import { request, runtime, Session } from '#/platform';

// --- [TYPES] ---------------------------------------------------------------------------

type Runtime = ReturnType<typeof runtime>;
type Live = MutableRef.MutableRef<Option.Option<Runtime>>;
type Busy = MutableRef.MutableRef<boolean>;
type Health = MutableRef.MutableRef<Readonly<Record<string, Outage>>>;
type Failure = Effect.Error<ReturnType<typeof decide>> | Cause.TimeoutError;
interface Outage {
    readonly since: number;
    readonly reason: string;
    readonly unjudged: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const reserve = 500;
const monitorTimeout = 300_000;
const nothingRan = 'The call was refused whole, so none of it ran.';
const mcpName = /^mcp__(?<server>.+?)__(?<tool>.+)$/su;
const reserved = ['tool', 'tool_use_id', 'agentId', 'consent', 'requestMeta'] as const;
const outputLead = /^[^]*(?<=^|[\s"'`(])/u.source;
const outputFolder = /\/[^\n"'`]*\/tasks\//u.source;
const clock = new Intl.DateTimeFormat(undefined, { hour: '2-digit', minute: '2-digit', hourCycle: 'h23' });
const plural = new Intl.PluralRules('en');
const counted = new Intl.NumberFormat('en');
const unjudgedCalls: Partial<Record<Intl.LDMLPluralRule, (count: string) => string>> & { readonly other: (count: string) => string } = { one: (count) => `${count} call unjudged`, other: (count) => `${count} calls unjudged` };

// --- [MODELS] --------------------------------------------------------------------------

const caller = { tool_use_id: Schema.String, agentId: Schema.optional(Schema.String) };
const by = ({ tool_use_id: call, agentId }: { readonly tool_use_id: string; readonly agentId?: string | undefined }) => ({ call, agent: Option.fromUndefinedOr(agentId) });
const reads = <To extends Schema.Top, From extends Schema.Top>(to: To, decode: (from: NoInfer<From['Type']>) => To['Type']): ((from: From) => Schema.decodeTo<Schema.toType<To>, From>) => Schema.decodeTo(Schema.toType(to), { decode: SchemaGetter.transform(decode), encode: SchemaGetter.forbiddenEncoding });
const ToolCall = Schema.Union([
    Schema.Struct({ tool: Schema.Literal('Bash'), ...caller, command: Schema.String, run_in_background: Schema.optional(Schema.Union([Schema.Boolean, Schema.Literals(['true', 'false'])])) }).pipe(
        reads(Action.cases.Shell, ({ command, run_in_background: flag, ...from }) => Action.cases.Shell.make({ ...by(from), command, run: flag === true || flag === 'true' ? 'background' : 'foreground' })),
    ),
    Schema.Struct({ tool: Schema.Literal('Monitor'), ...caller, command: Schema.String }).pipe(reads(Action.cases.Shell, ({ command, ...from }) => Action.cases.Shell.make({ ...by(from), command, run: 'monitored' }))),
    Schema.Struct({ tool: Schema.Literal('Monitor'), ...caller, ws: Schema.Struct({ url: Schema.String }) }).pipe(reads(Action.cases.Fetch, ({ ws: { url }, ...from }) => Action.cases.Fetch.make({ ...by(from), url }))),
    Schema.Struct({ tool: Schema.Literal('Read'), ...caller, file_path: Schema.String }).pipe(reads(Action.cases.Read, ({ file_path: path, ...from }) => Action.cases.Read.make({ ...by(from), path }))),
    Schema.Struct({ tool: Schema.Literal('Write'), ...caller, file_path: Schema.String, content: Schema.String }).pipe(reads(Action.cases.Write, ({ file_path: path, content, ...from }) => Action.cases.Write.make({ ...by(from), path, content }))),
    Schema.Struct({ tool: Schema.Literal('Edit'), ...caller, file_path: Schema.String, old_string: Schema.String, new_string: Schema.String }).pipe(reads(Action.cases.Edit, ({ file_path: path, old_string, new_string, ...from }) => Action.cases.Edit.make({ ...by(from), path, old: old_string, new: new_string }))),
    Schema.Struct({ tool: Schema.Literal('WebFetch'), ...caller, url: Schema.String }).pipe(reads(Action.cases.Fetch, ({ url, ...from }) => Action.cases.Fetch.make({ ...by(from), url }))),
    Schema.Struct({ tool: Schema.Literal('Agent'), ...caller, prompt: Schema.String, model: Schema.optional(Schema.String) }).pipe(reads(Action.cases.Spawn, ({ prompt, model, ...from }) => Action.cases.Spawn.make({ ...by(from), prompt, model: Option.fromUndefinedOr(model) }))),
    Schema.StructWithRest(Schema.Struct({ tool: Schema.String.check(Schema.isPattern(mcpName)), ...caller }), [Schema.Record(Schema.String, Schema.Unknown)]).pipe(
        reads(Action.cases.Mcp, (from) => {
            const { server = '', tool = '' } = mcpName.exec(from.tool)?.groups ?? {};
            return Action.cases.Mcp.make({ ...by(from), server, tool, input: Struct.omit(from, reserved) });
        }),
    ),
]);
const Background = Schema.Struct({ result: Schema.Struct({ backgroundTaskId: Schema.String }), text: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const firstLine = (text: string) => text.split('\n', 1).join('');
const annotate = (result: ToolCallResult, notes: readonly string[]): ToolCallResult => (result.deny !== undefined || notes.length === 0 ? result : { ...result, context: [...(result.context ?? []), ...notes] });
const backgroundTask = (result: ToolCallResult) =>
    Option.flatMap(Schema.decodeUnknownOption(Background)(result), ({ result: { backgroundTaskId: id }, text }) => Option.map(Option.fromNullishOr(new RegExp(`${outputLead}(?<output>${outputFolder}${RegExp.escape(id)}\\.output)`, 'u').exec(text)?.groups), ({ output = '' }) => ({ id, output })));
const decide = (policy: Policy, action: Action) => Session.use((session) => Effect.flatMap(session.acting(action.agent), ({ environment, path }) => Effect.provideService(policy.decide(action, environment), Path.Path, path)).pipe(Effect.tap(({ faults }) => Effect.forEach(faults, session.log, { discard: true }))));
const deadline = (next: Pick<Next<'tool.call'>, 'budget'>) =>
    Effect.suspend(() => Effect.sleep(Duration.millis(Math.max(0, next.budget.remainingMs - reserve)))).pipe(Effect.repeat({ until: () => next.budget.remainingMs <= reserve }), Effect.andThen(Effect.fail(new Cause.TimeoutError('its hook budget ran out before the rules settled'))));
const reasonOf = (cause: Cause.Cause<Failure>): Option.Option<string> =>
    cause.pipe(
        Cause.findErrorOption,
        Option.getOrUndefined,
        Match.value,
        Match.when(Match.undefined, () => cause.pipe(Cause.pretty, firstLine, Option.some)),
        Match.when({ _tag: 'ShellError', reason: { _tag: 'Syntax' } }, () => Option.none()),
        Match.when({ _tag: 'PlatformError', reason: { _tag: 'BadArgument' } }, ({ reason }) => Option.some(firstLine(reason.message))),
        Match.when({ _tag: 'PlatformError' }, ({ reason: { module, method, description, _tag } }) => Option.some(`engine refused ${module}.${method}: ${firstLine(description ?? _tag)}`)),
        Match.orElse((error) => Option.some(firstLine(error.message))),
    );
const shown = (outages: Readonly<Record<string, Outage>>) =>
    Array.match(Record.toEntries(outages), {
        onEmpty: () => undefined,
        onNonEmpty: (rows) => rows.map(([kind, { since, reason, unjudged }]) => `${kind} rules off since ${clock.format(since)} · ${reason} · ${(unjudgedCalls[plural.select(unjudged)] ?? unjudgedCalls.other)(counted.format(unjudged))}`).join('; '),
    });
const watch = (policy: Policy, busy: Busy, { call, agent }: Action, task: Option.Option.Value<ReturnType<typeof backgroundTask>>) =>
    Session.use((session) => {
        const check = (resting: boolean) =>
            Effect.flatMap(decide(policy, Action.cases.Task.make({ ...task, call, agent, resting, again: Duration.infinity })), (decision) =>
                Effect.as(
                    Effect.forEach(decision.notes, (text) => session.tell(agent, text), { discard: true }),
                    Decision.$is('Rewrite')(decision) && Action.guards.Task(decision.action) ? decision.action.again : Duration.infinity,
                ),
            );
        return Effect.suspend(() => Option.match(agent, { onNone: () => Effect.succeedSome(!MutableRef.get(busy)), onSome: session.resting })).pipe(
            Effect.flatMap(Option.match({ onNone: () => Effect.succeed(Duration.infinity), onSome: check })),
            Effect.repeat({
                while: Duration.isFinite,
                schedule: Schedule.forever.pipe(
                    Schedule.setInputType<Duration.Duration>(),
                    Schedule.addDelay(({ input }) => Effect.succeed(input)),
                ),
            }),
            Effect.catchCause((cause) => session.log(`policy stopped watching task ${task.id}: ${Cause.pretty(cause)}`)),
        );
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

function current($: EngineInterface, live: Live) {
    return Option.getOrElse(MutableRef.get(live), () => {
        const made = runtime($);
        MutableRef.set(live, Option.some(made));
        made.runPromise(Effect.void).catch(() => {
            MutableRef.compareAndSet(live, Option.some(made), Option.none());
            return made.dispose();
        });
        return made;
    });
}

function settle($: EngineInterface, health: Health, kind: Action['_tag'], fault: Option.Option<string>) {
    const held = MutableRef.get(health);
    const after = Option.match(fault, {
        onNone: () => Record.remove(held, kind),
        onSome: (reason) => Record.set(held, kind, Option.match(Record.get(held, kind), { onNone: () => ({ since: Effect.runSync(Clock.currentTimeMillis), reason, unjudged: 1 }), onSome: ({ since, unjudged }) => ({ since, reason, unjudged: unjudged + 1 }) })),
    });
    MutableRef.set(health, after);
    if (Option.isSome(fault) || Record.has(held, kind)) {
        $.ui.status(shown(after));
    }
}

function judged($: EngineInterface, health: Health, action: Action, settled: Exit.Exit<Decision, Failure>) {
    return Exit.match(settled, {
        onSuccess: (decision) => {
            settle($, health, action._tag, Option.none());
            return decision;
        },
        onFailure: (cause) => {
            $.ui.log(`policy did not judge ${action._tag} call ${action.call} and let it run: ${Cause.pretty(cause)}`, { to: 'debug' });
            Option.match(reasonOf(cause), { onNone: Function.constVoid, onSome: (reason) => settle($, health, action._tag, Option.some(reason)) });
            return Decision.Allow({ notes: [], faults: [] });
        },
    });
}

function monitor($: EngineInterface, e: ToolCallInput, next: Next<'tool.call'>, command: string, notes: readonly string[]) {
    const { description, timeout } = e;
    const input = { command, description: Predicate.isString(description) ? description : command, timeout_ms: Predicate.isNumber(timeout) ? timeout : monitorTimeout };
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

function answer($: EngineInterface, e: ToolCallInput, next: Next<'tool.call'>, action: Action, policy: Policy, live: Runtime, busy: Busy, health: Health) {
    return live.runPromise(
        Effect.gen(function* () {
            const decision = judged($, health, action, yield* Effect.exit(Effect.raceFirst(decide(policy, action), deadline(next))));
            const watched = (result: ToolCallResult) => Option.filter(backgroundTask(result), () => policy.appliesTo('Task'));
            const run = (input: ToolCallInput) =>
                request('Engine', 'next', input.tool, () => next(input)).pipe(
                    Effect.tap((result) => Option.match(watched(result), { onNone: () => Effect.void, onSome: (task) => Session.use((session) => session.watch(watch(policy, busy, action, task))) })),
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
                Deny: ({ reasons }) => Effect.succeed<ToolCallResult>({ deny: [...reasons, nothingRan].join('\n') }),
                Allow: () => run(e),
                Rewrite: ({ action: restated }) => (Action.guards.Shell(restated) ? dispatch(restated) : run(e)),
            });
        }),
        { signal: next.signal },
    );
}

function hooks(on: On, policy: Policy) {
    const [live, busy, health]: readonly [Live, Busy, Health] = [MutableRef.make(Option.none()), MutableRef.make(false), MutableRef.make({})];
    on('session.start', ($, e, next) => {
        $.ui.status(undefined);
        return Option.match(MutableRef.getAndSet(live, Option.none()), {
            onNone: () => next(e),
            onSome: (stale) => Promise.all([next(e), stale.dispose()]).then(([started]) => started),
        });
    }).catch((_, e, next) => next(e));
    on('session.end', ($, e, next) => {
        MutableRef.set(busy, Effect.runSync(Effect.as(policy.clear(), false)));
        MutableRef.set(health, {});
        $.ui.status(undefined);
        return Option.match(MutableRef.get(live), {
            onNone: () => next(e),
            onSome: (kept) => Promise.all([next(e), kept.runPromise(Session.use((session) => session.end))]).then(([ended]) => ended),
        });
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
    on('tool.call', ($, e, next) =>
        Option.match(
            Option.filter(Schema.decodeOption(ToolCall)(e), ({ _tag }) => policy.appliesTo(_tag)),
            {
                onNone: () => next(e),
                onSome: (action) => answer($, e, next, action, policy, current($, live), busy, health),
            },
        ),
    ).catch((error, e, next) => {
        const failed = Option.filter(Schema.decodeOption(ToolCall)(e), ({ _tag }) => policy.appliesTo(_tag) && !next.called && !next.signal.aborted);
        const said = `its hook ${next.error.kind === 'throw' ? 'threw' : 'ran out of time'}: ${next.error.message ?? next.error.kind}`;
        Option.match(failed, {
            onNone: Function.constVoid,
            onSome: (action) => {
                error.ui.log(`policy did not judge ${action._tag} call ${action.call} and let it run: ${said}`, { to: 'debug' });
                settle(error, health, action._tag, Option.some(firstLine(said)));
            },
        });
        return next(e);
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { hooks };
