import { Action, Decision, type Outcome } from '@rasm/policy/action';
import type { Policy } from '@rasm/policy/policy';
import { Line, Report } from '@rasm/report';
import { Shell } from '@rasm/shell';
import type { Frozen, NextBudget, ToolCallInput, ToolCallResult } from 'claude-code';
import { Cause, Duration, Effect, HashMap, type Layer, Match, type PlatformError, Option, Path, Predicate, Record, Schedule, Schema, SchemaGetter, Struct, SubscriptionRef } from 'effect';
import { Bash } from '#/engine/bash';
import type { Proceed } from '#/engine/events';
import type { Mod } from '#/engine/host';
import { Engine, Notice } from '#/engine/interface';
import { Ledger } from '#/engine/ledger';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RESERVE = 500;
const ALLOW = Decision.Allow({ notes: [], faults: [] });

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

// --- [OPERATIONS] ----------------------------------------------------------------------

const line = ({ subject, reason, values, next }: Outcome) => new Line({ head: [subject], value: reason, facts: Record.collect(values, (label, value) => `${label} ${value}`), ...Option.getOrUndefined(Option.map(next, (call) => ({ call }))) });
const report = (outcome: Outcome) => Report.text(new Report({ lines: [line(outcome)] }));
const attach = (result: ToolCallResult, notes: readonly Outcome[]): ToolCallResult => (result.deny !== undefined || notes.length === 0 ? result : { ...result, context: [...(result.context ?? []), ...notes.map(report)] });
const decide = (policy: Policy, action: Action, budget: Option.Option<NextBudget>) =>
    Bash.use((bash) => bash.shell(action.agent)).pipe(
        Effect.flatMap(({ environment, path }) => Effect.provideService(policy.decide(action, environment), Path.Path, path)),
        Effect.tap(({ faults }) => Effect.forEach(faults, (fault) => Effect.logWarning('policy fault').pipe(Effect.annotateLogs({ fault })), { discard: true })),
        Effect.raceFirst(
            Option.match(budget, {
                onNone: () => Effect.never,
                onSome: (held) => Effect.suspend(() => Effect.sleep(Math.max(0, held.remainingMs - RESERVE))).pipe(Effect.repeat({ until: () => held.remainingMs <= RESERVE }), Effect.andThen(new Cause.TimeoutError('hook budget spent'))),
            }),
        ),
        Effect.catchCause((cause) => Effect.as(Effect.logWarning('judge failed', cause), ALLOW)),
        Effect.when(Effect.succeed(policy.appliesTo(action._tag))),
        Effect.map(Option.getOrElse(() => ALLOW)),
        Effect.annotateLogs({ action: action._tag, call: action.call }),
    );
const watch = Effect.fnUntraced(function* (policy: Policy, { call, agent }: Action, task: Pick<typeof Action.cases.Task.Type, 'id' | 'output'>) {
    const [engine, bash] = yield* Effect.all([Engine, Bash]);
    const listed = Option.match(agent, { onNone: () => Effect.succeed(true), onSome: (id) => Effect.map(engine.agents, Record.has(id)) });
    yield* bash.fork(
        Effect.gen(function* () {
            const [live, running] = yield* Effect.all([listed, engine.isRunning(agent)]);
            const decision = live ? yield* decide(policy, Action.cases.Task.make({ ...task, call, agent, resting: !running, again: Duration.infinity }), Option.none()) : ALLOW;
            yield* Effect.forEach(decision.notes, (note) => engine.deliver(Notice.Message({ text: report(note), agent })), { discard: true });
            return Decision.$is('Rewrite')(decision) && Action.guards.Task(decision.action) ? decision.action.again : Duration.infinity;
        }).pipe(
            Effect.repeat({
                while: Duration.isFinite,
                schedule: Schedule.forever.pipe(
                    Schedule.setInputType<Duration.Duration>(),
                    Schedule.addDelay(({ input }) => Effect.succeed(input)),
                ),
            }),
            Effect.catchCause((cause) => Effect.logWarning('task watch failed', cause).pipe(Effect.annotateLogs({ task: task.id }))),
        ),
    );
});
const monitor = Effect.fnUntraced(function* (e: Frozen<ToolCallInput>, command: string) {
    const [engine, ledger] = yield* Effect.all([Engine, Ledger]);
    const called = yield* engine.callTool({ tool: 'Monitor', input: { command, description: Predicate.isString(e['description']) ? e['description'] : command, ...(Predicate.isNumber(e['timeout']) ? { timeout_ms: e['timeout'] } : {}) }, ...Struct.pick(e, ['agentId']) });
    yield* ledger.amend(e.tool_use_id, { answered: { tool: 'Monitor', result: called } });
    return Match.value(called).pipe(
        Match.withReturnType<ToolCallResult>(),
        Match.when({ deny: Match.string }, (refused) => refused),
        Match.when({ isError: true }, ({ text }) => ({ isError: true, result: text, ...(text === undefined ? {} : { text }) })),
        Match.orElse(({ text }) => ({ result: { stdout: text ?? '', stderr: '', interrupted: false } })),
    );
});
const judge = Effect.fnUntraced(function* (policy: Policy, e: Frozen<ToolCallInput>, next: Proceed<'tool.call'>, action: Action) {
    const decision = yield* decide(policy, action, Option.some(next.budget));
    const result: ToolCallResult = yield* Decision.$match(decision, {
        Deny: ({ reasons }) => Effect.succeed<ToolCallResult>({ deny: Report.text(new Report({ lines: [new Line({ head: ['call refused'], facts: ['none of it ran'] }), ...reasons.map(line)] })) }),
        Allow: () => next(e),
        Rewrite: ({ action: restated, rules }) =>
            Effect.andThen(
                Ledger.use((ledger) => ledger.amend(action.call, { ran: Option.map(Option.liftPredicate(restated, Action.guards.Shell), Struct.pick(['command', 'run'])), rules })),
                () =>
                    Match.value({ from: action, to: restated }).pipe(
                        Match.when({ from: { _tag: 'Shell', run: 'foreground' }, to: { _tag: 'Shell', run: 'background' } }, ({ to: { command } }) => {
                            const { timeout: _timeout, ...untimed } = e;
                            return next({ ...untimed, command, run_in_background: true });
                        }),
                        Match.when({ from: { _tag: 'Shell', run: Match.is('foreground', 'background') }, to: { _tag: 'Shell', run: 'monitored' } }, ({ to: { command } }) => monitor(e, command)),
                        Match.when({ to: { _tag: 'Shell' } }, ({ to: { command } }) => next({ ...e, command })),
                        Match.orElse(() => next(e)),
                    ),
            ),
    });
    return attach(result, decision.notes);
});
const inspect = (policy: Policy, e: Frozen<ToolCallInput>, budget: NextBudget, result: ToolCallResult) =>
    Option.match(Option.fromUndefinedOr(result.text), {
        onNone: () => Effect.succeed(result),
        onSome: (text) => Effect.map(decide(policy, Action.cases.Output.make({ call: e.tool_use_id, agent: Option.fromUndefinedOr(e.agentId), tool: e.tool, text, readOnly: result.isReadOnly === true }), Option.some(budget)), ({ notes }) => attach(result, notes)),
    });
const trace = Effect.fnUntraced(function* (policy: Policy, action: typeof Action.cases.Shell.Type) {
    const [ledger, bash, shell] = yield* Effect.all([Ledger, Bash, Shell]);
    const use = HashMap.get((yield* SubscriptionRef.get(ledger.state)).uses, action.call);
    const { environment, path } = yield* bash.shell(action.agent);
    yield* Effect.forEach(
        Option.toArray(Option.flatMap(use, Struct.get('ran'))),
        ({ command }) =>
            environment.pipe(
                Effect.flatMap(({ dialect, cwd }) => shell.read(command, { dialect, cwd })),
                Effect.flatMap((script) => script.invocations),
                Effect.flatMap((invocations) => ledger.amend(action.call, { invocations: invocations.flatMap(Struct.get('leaves')) })),
                Effect.provideService(Path.Path, path),
                Effect.catchCause((cause) => Effect.logWarning('command parse failed', cause).pipe(Effect.annotateLogs({ call: action.call }))),
            ),
        { discard: true },
    );
    yield* Effect.forEach(Option.toArray(Option.flatMap(use, ({ task, output }) => Option.filter(Option.all({ id: task, output }), () => policy.appliesTo('Task')))), (task) => watch(policy, action, task), { discard: true });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

function enforce(policy: Policy): Mod<Layer.Success<typeof Bash.layer>, Layer.Error<typeof Bash.layer>, PlatformError.PlatformError> {
    return {
        layer: Bash.layer,
        hooks: {
            'session.end': (ended, proceed) => Effect.andThen(policy.clear(), proceed(ended)),
            'session.compact': (compacted, proceed) => Effect.tap(proceed(compacted), ({ messages }) => Effect.when(policy.forget(Option.fromUndefinedOr(compacted.agentId)), Effect.succeed(messages !== undefined && compacted.trigger !== 'precompute'))),
            'tool.call': Effect.fnUntraced(function* (called: Frozen<ToolCallInput>, proceed: Proceed<'tool.call'>) {
                const action = Schema.decodeUnknownOption(ToolCall)(called);
                const shell = Option.filter(action, Action.guards.Shell);
                const ran = Option.match(action, { onNone: () => proceed(called), onSome: (held) => judge(policy, called, proceed, held) }).pipe(Effect.flatMap((answered) => inspect(policy, called, proceed.budget, answered)));
                const result = yield* Ledger.use((ledger) => ledger.track(called, Option.map(shell, Struct.pick(['command', 'run'])), ran));
                yield* Effect.forEach(Option.toArray(Option.filter(shell, () => result.deny === undefined)), (held) => trace(policy, held), { discard: true });
                return result;
            }),
        },
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { enforce };
