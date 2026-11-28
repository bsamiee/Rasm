import type { Action } from '@rasm/policy/action';
import type { Shell as Parser } from '@rasm/shell';
import type { Frozen, ToolCallInput, ToolCallResult } from 'claude-code';
import { Context, Data, DateTime, Effect, Exit, HashMap, Layer, Match, Option, Schema, SubscriptionRef } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Shell = Pick<typeof Action.cases.Shell.Type, 'command' | 'run'>;
type Invocation = Effect.Success<Effect.Success<ReturnType<Parser['Service']['read']>>['invocations']>[number];
type Facts = Partial<Pick<Use, 'end' | 'ran' | 'rules' | 'task' | 'output' | 'commit' | 'programs' | 'changes'>>;
interface State {
    readonly uses: HashMap.HashMap<string, Use>;
    readonly tasks: HashMap.HashMap<string, string>;
    readonly paths: HashMap.HashMap<string, string>;
}

// --- [MODELS] --------------------------------------------------------------------------

class Program extends Data.Class<{ readonly program: string; readonly path: readonly string[]; readonly operands: readonly string[]; readonly cwd: Option.Option<string> }> {}
class Change extends Data.Class<{ readonly path: string; readonly removed: boolean }> {}
class Use extends Data.Class<{
    readonly call: string;
    readonly agent: Option.Option<string>;
    readonly tool: string;
    readonly asked: Option.Option<Shell>;
    readonly ran: Option.Option<Shell>;
    readonly rules: readonly string[];
    readonly start: DateTime.Utc;
    readonly end: Option.Option<DateTime.Utc>;
    readonly task: Option.Option<string>;
    readonly output: Option.Option<string>;
    readonly commit: Option.Option<string>;
    readonly programs: readonly Program[];
    readonly changes: readonly Change[];
}> {}
const Answer = Schema.Union([
    Schema.Struct({ tool: Schema.Literals(['Write', 'Edit']), result: Schema.Struct({ filePath: Schema.String, staged: Schema.optional(Schema.Boolean) }) }),
    Schema.Struct({ tool: Schema.Literal('NotebookEdit'), result: Schema.Struct({ notebook_path: Schema.String }) }),
    Schema.Struct({ tool: Schema.Literal('Monitor'), result: Schema.Struct({ taskId: Schema.String }) }),
    Schema.Struct({
        tool: Schema.Literal('Bash'),
        text: Schema.optional(Schema.String),
        result: Schema.Struct({ backgroundTaskId: Schema.optional(Schema.String), gitOperation: Schema.optional(Schema.Struct({ commit: Schema.optional(Schema.Struct({ sha: Schema.String })) })) }),
    }),
]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = (tool: string, answered: Option.Option<ToolCallResult>): Facts =>
    Option.match(
        Option.flatMap(answered, (result) => Schema.decodeUnknownOption(Answer)({ ...result, tool })),
        {
            onNone: () => ({}),
            onSome: (answer) =>
                Match.value(answer).pipe(
                    Match.discriminator('tool')('Write', 'Edit', ({ result: { filePath, staged } }): Facts => ({ changes: staged === true ? [] : [new Change({ path: filePath, removed: false })] })),
                    Match.discriminator('tool')('NotebookEdit', ({ result: { notebook_path: path } }): Facts => ({ changes: [new Change({ path, removed: false })] })),
                    Match.discriminator('tool')('Monitor', ({ result: { taskId } }): Facts => ({ task: Option.some(taskId) })),
                    Match.discriminator('tool')('Bash', ({ result: { backgroundTaskId, gitOperation }, text }): Facts => ({
                        ...(backgroundTaskId === undefined
                            ? {}
                            : { task: Option.some(backgroundTaskId), output: Option.flatMapNullishOr(Option.fromUndefinedOr(text), (body) => new RegExp(String.raw`^[^]*(?<=^|[\s"'\x60(])(?<output>/[^\n"'\x60]*/tasks/${RegExp.escape(backgroundTaskId)}\.output)`, 'u').exec(body)?.groups?.['output']) }),
                        ...(gitOperation?.commit === undefined ? {} : { commit: Option.some(gitOperation.commit.sha) }),
                    })),
                    Match.exhaustive,
                ),
        },
    );
const parse = (invocations: readonly Invocation[]): Facts => ({
    programs: invocations.flatMap((invocation) =>
        Option.toArray(invocation.name).map(
            (program) =>
                new Program({
                    program,
                    path: invocation.path,
                    operands: invocation.operands.flatMap(({ word }) => Option.toArray(word.text)),
                    cwd: Match.valueTags(invocation.place, { Local: ({ cwd }) => cwd.text, Remote: () => Option.none() }),
                }),
        ),
    ),
    changes: invocations
        .flatMap(({ writes, deletes }) => [
            { fields: writes, removed: false },
            { fields: deletes, removed: true },
        ])
        .flatMap(({ fields, removed }) => fields.flatMap(({ text }) => Option.toArray(text)).map((path) => new Change({ path, removed }))),
});

// --- [SERVICES] ------------------------------------------------------------------------

class Ledger extends Context.Service<
    Ledger,
    {
        readonly state: SubscriptionRef.SubscriptionRef<State>;
        readonly track: <E, R>(e: Frozen<ToolCallInput>, asked: Option.Option<Shell>, run: Effect.Effect<ToolCallResult, E, R>) => Effect.Effect<ToolCallResult, E, R>;
        readonly amend: (call: string, patch: Pick<Facts, 'ran' | 'rules'> & { readonly invocations?: readonly Invocation[]; readonly answered?: { readonly tool: string; readonly result: ToolCallResult } }) => Effect.Effect<void>;
    }
>()('@rasm/claude-code/engine/ledger') {
    static readonly layer = Layer.effect(
        Ledger,
        Effect.gen(function* () {
            const state = yield* SubscriptionRef.make<State>({ uses: HashMap.empty(), tasks: HashMap.empty(), paths: HashMap.empty() });
            const apply = (call: string, facts: Facts) =>
                SubscriptionRef.update(state, (held) =>
                    Option.match(HashMap.get(held.uses, call), {
                        onNone: () => held,
                        onSome: (was) => {
                            const use = new Use({ ...was, ...facts, changes: [...was.changes, ...(facts.changes ?? [])] });
                            return {
                                uses: HashMap.set(held.uses, call, use),
                                tasks: Option.match(use.task, { onNone: () => held.tasks, onSome: (id) => HashMap.set(held.tasks, id, call) }),
                                paths: HashMap.union(held.paths, HashMap.fromIterable((facts.changes ?? []).map(({ path }) => [path, call] as const))),
                            };
                        },
                    }),
                );
            const settle = (call: string, facts: Facts) => Effect.flatMap(DateTime.now, (end) => apply(call, { ...facts, end: Option.some(end) }));
            return {
                state,
                track: Effect.fnUntraced(function* <E, R>(e: Frozen<ToolCallInput>, asked: Option.Option<Shell>, run: Effect.Effect<ToolCallResult, E, R>) {
                    const use = new Use({ call: e.tool_use_id, agent: Option.fromUndefinedOr(e.agentId), tool: e.tool, asked, ran: asked, rules: [], start: yield* DateTime.now, end: Option.none(), task: Option.none(), output: Option.none(), commit: Option.none(), programs: [], changes: [] });
                    yield* SubscriptionRef.update(state, (held) => ({ ...held, uses: HashMap.set(held.uses, use.call, use) }));
                    return yield* Effect.onExit(run, (exit) => settle(use.call, read(e.tool, Exit.getSuccess(exit))));
                }),
                amend: (call, { invocations, answered, ...facts }) => {
                    const [answer, parsed]: readonly [Facts, Facts] = [answered === undefined ? {} : read(answered.tool, Option.some(answered.result)), invocations === undefined ? {} : parse(invocations)];
                    return apply(call, { ...facts, ...answer, ...parsed, changes: [...(answer.changes ?? []), ...(parsed.changes ?? [])] });
                },
            };
        }),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Change, Ledger, Program, Use };
