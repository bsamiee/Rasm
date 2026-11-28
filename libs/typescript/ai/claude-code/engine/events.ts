import type { Args, Chunk, EngineInterface, EventName, EventResult, Frozen, HookStream, Next, StreamingEventName, StreamNext, ToolCallResult, ToolSpec, TurnStepInput, TurnStepResult } from 'claude-code';
import { Array, Cause, Channel, Context, Effect, Exit, Formatter, HashSet, Layer, type ManagedRuntime, Option, type PlatformError, Predicate, Pull, Record, Ref, Result, Schema, Scope } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { call } from '#/engine/services';

// --- [TYPES] ---------------------------------------------------------------------------

type Hosted =
    | 'session.start'
    | 'session.end'
    | 'session.compact'
    | 'session.receive'
    | 'session.stop'
    | 'plugin.register'
    | 'turn.start'
    | 'turn.complete'
    | 'agent.offer'
    | 'agent.report'
    | 'tool.describe'
    | 'tool.check'
    | 'tool.call'
    | 'command.run'
    | 'prompt.submit'
    | 'prompt.section'
    | 'prompt.context'
    | 'prompt.fill'
    | 'prompt.paint'
    | 'prompt.suggest'
    | 'ui.press'
    | 'ui.input'
    | 'ui.select'
    | 'ui.focus'
    | 'ui.scroll'
    | 'classic.Stop'
    | 'classic.SubagentStop'
    | 'classic.Notification'
    | 'classic.StopFailure';
type Picked = 'budget' | 'signal' | 'origin';
type Proceed<N extends EventName> = ((input: Frozen<Args<N>>) => Effect.Effect<EventResult<N>, PlatformError.PlatformError>) & Pick<Next<N>, Picked>;
type Onward<N extends StreamingEventName> = ((input: Frozen<Args<N>>) => Channel.Channel<Chunk<N>, Cause.UnknownError, EventResult<N>>) & Pick<StreamNext<N>, Picked>;
type Hook<N extends EventName, R, F> = (e: Frozen<Args<N>>, next: Proceed<N>) => Effect.Effect<EventResult<N>, F, R>;
type StreamHook<N extends StreamingEventName, R, F> = (e: Frozen<Args<N>>, next: Onward<N>) => Channel.Channel<Chunk<N>, F, EventResult<N>, unknown, unknown, unknown, R>;
type Hooks<R, F> = { readonly [N in Hosted]?: Hook<N, R, F> } & { readonly 'turn.step'?: StreamHook<'turn.step', R, F> };
type AgentSpec = Parameters<EngineInterface['agent']['register']>[0];
interface Tool<R, F> {
    readonly spec: Omit<ToolSpec, 'name'>;
    readonly run: (e: Frozen<Args<'tool.run'>>, next: Proceed<'tool.run'>) => Effect.Effect<ToolCallResult, F | Schema.SchemaError | PlatformError.PlatformError, R>;
}
interface Agent extends Omit<AgentSpec, 'name'> {
    readonly isOffered: boolean;
}

// --- [MODELS] --------------------------------------------------------------------------

const Tool = <S extends Schema.Decoder<unknown>, R, F>(
    input: S,
    { run, ...spec }: Omit<ToolSpec, 'name' | 'inputSchema'> & { readonly run: (input: S['Type'], e: Frozen<Args<'tool.run'>>, next: Proceed<'tool.run'>) => Effect.Effect<ToolCallResult, F | PlatformError.PlatformError, R> },
): Tool<R | S['DecodingServices'], F> => {
    const { schema, definitions } = Schema.toJsonSchemaDocument(input);
    const decode = Schema.decodeUnknownEffect(input);
    return { spec: { ...spec, inputSchema: Record.isEmptyRecord(definitions) ? schema : { ...schema, $defs: definitions } }, run: (e, next) => Effect.flatMap(decode(e), (decoded) => run(decoded, e, next)) };
};

// --- [SERVICES] ------------------------------------------------------------------------

class Plugin extends Context.Service<
    Plugin,
    {
        readonly name: string;
        readonly tools: Record.ReadonlyRecord<string, string>;
        readonly agents: Record.ReadonlyRecord<string, string>;
        readonly blocked: Ref.Ref<HashSet.HashSet<string>>;
    }
>()('@rasm/claude-code/engine/events/Plugin') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const dispatch = <N extends EventName, R, F>(hooks: { readonly [K in N]?: Hook<K, R, F> }, e: Frozen<Args<N>>, next: ((input: Frozen<Args<N>>) => Promise<EventResult<N>>) & Pick<Next<N>, Picked | 'event'>): Effect.Effect<EventResult<N>, F | PlatformError.PlatformError, R> => {
    const proceed: Proceed<N> = Object.assign((input: Frozen<Args<N>>) => call(next.event, 'next', () => next(input)), { budget: next.budget, signal: next.signal, origin: next.origin });
    return Option.match(Option.fromUndefinedOr(hooks[next.event]), { onNone: () => proceed(e), onSome: (hook) => hook(e, proceed) });
};
const reply = (e: Frozen<TurnStepInput>, { answer, toolUses }: Pick<TurnStepResult, 'answer' | 'toolUses'>) => {
    const first = answer === '' ? 0 : 1;
    const stopReason = Array.isReadonlyArrayNonEmpty(toolUses) ? 'tool_use' : 'end_turn';
    return Channel.fromIterable<Chunk<'turn.step'>, TurnStepResult>({
        *[Symbol.iterator]() {
            yield* answer === '' ? [] : [{ kind: 'text', index: 0, text: answer } as const];
            yield* toolUses.flatMap(
                ({ name, input }, at) =>
                    [
                        { kind: 'tool', index: first + at, id: `toolu_${SecureRandom.uuidV4Unsafe().replaceAll('-', '')}`, name },
                        { kind: 'input', index: first + at, json: JSON.stringify(input) },
                    ] as const,
            );
            yield { kind: 'stop', stopReason, usage: null } as const;
            return { turnId: e.turnId, index: e.index, answer, toolUses, stopReason, usage: null };
        },
    });
};
const stopping =
    <N extends 'classic.Stop' | 'classic.SubagentStop', R, F>(hook: Hook<N, R, F>): Hook<N, R | Plugin, F | PlatformError.PlatformError> =>
    (e, next) =>
        e.stop_hook_active
            ? next(e)
            : Effect.gen(function* () {
                  const [result, { blocked }] = yield* Effect.all([hook(e, next), Plugin]);
                  const fresh = yield* Option.match(
                      Option.map(Option.fromUndefinedOr(result.block), (block) => `${e.agent_id ?? 'main'}\n${block}`),
                      {
                          onNone: () => Effect.succeed(true),
                          onSome: (key) => Ref.modify(blocked, (held) => [!HashSet.has(held, key), HashSet.add(held, key)] as const),
                      },
                  );
                  return fresh ? result : yield* next(e);
              });
const declared = <R, F>(tools: Record.ReadonlyRecord<string, Tool<R, F>>, agents: Record.ReadonlyRecord<string, Agent>) => ({
    matchers: {
        'tool.run': { tool: new RegExp(`^mcp__(?:(?!__)[^])+__(?:${Record.keys(tools).map(RegExp.escape).join('|')})$`, 'u') },
        'agent.offer': { agent: new RegExp(`^[^:]+:(?:${Record.keys(agents).map(RegExp.escape).join('|')})$`, 'u') },
    },
    hooks: {
        'tool.run': (e, next) =>
            Plugin.use(({ tools: named }) =>
                Option.match(
                    Option.flatMap(
                        Record.findFirst(named, (full) => full === e.tool),
                        ([key]) => Option.map(Record.get(tools, key), (tool) => [key, tool] as const),
                    ),
                    {
                        onNone: () => next(e),
                        onSome: ([key, tool]) =>
                            Effect.catchCause(tool.run(e, next), (cause) => {
                                const squashed = Cause.squash(cause);
                                return Effect.as(Effect.logWarning(`tool ${key} failed`, cause), { deny: `${key}: ${Predicate.isError(squashed) ? squashed.message : Formatter.format(squashed)}` });
                            }),
                    },
                ),
            ),
        'agent.offer': (e, next) =>
            Plugin.use(({ name, agents: named }) =>
                Option.match(
                    Option.flatMap(
                        Record.findFirst(named, (full) => full === e.agent),
                        ([key]) => Record.get(agents, key),
                    ),
                    {
                        onNone: () => next(e),
                        onSome: ({ isOffered }) => (isOffered || next.origin.plugin === name ? next(e) : Effect.succeed({ isOffered: false })),
                    },
                ),
            ),
    } satisfies { readonly [N in 'tool.run' | 'agent.offer']: Hook<N, R | Plugin, PlatformError.PlatformError> },
});
const registered = <A>(event: 'tool.register' | 'agent.register', declarations: Record.ReadonlyRecord<string, A>, register: (name: string, declaration: A) => Promise<string>) =>
    Effect.map(
        Effect.filterMapEffect(Record.toEntries(declarations), ([key, declaration]) =>
            call(event, key, () => register(key, declaration)).pipe(
                Effect.map((full) => Result.succeed([key, full] as const)),
                Effect.catchCause((cause) => Effect.as(Effect.logWarning(`${event} ${key} failed`, cause), Result.failVoid)),
            ),
        ),
        Record.fromEntries,
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

function stream<N extends StreamingEventName, R, E, F>(e: Frozen<Args<N>>, next: ((input: Frozen<Args<N>>) => HookStream<Chunk<N>, EventResult<N>>) & Pick<StreamNext<N>, Picked | 'event'>, hook: StreamHook<N, R, F>, runtime: ManagedRuntime.ManagedRuntime<R, E>): AsyncGenerator<Chunk<N>, EventResult<N>> {
    const onward: Onward<N> = Object.assign((input: Frozen<Args<N>>) => Channel.fromAsyncIterable(next(input), (cause) => new Cause.UnknownError(cause, `${next.event} next failed`)), { budget: next.budget, signal: next.signal, origin: next.origin });
    const scope = Scope.makeUnsafe();
    const close = Scope.close(scope, Exit.void);
    const pulled = Channel.mapError(hook(e, onward), (error) => new Cause.UnknownError(error, `${next.event} hook failed`));
    const read = Channel.toPullScoped(pulled, scope).pipe(
        Effect.cached,
        Effect.runSync,
        Effect.flatten,
        Effect.map((value): IteratorResult<Chunk<N>, EventResult<N>> => ({ done: false, value })),
        Pull.catchDone((value) => Effect.succeed<IteratorResult<Chunk<N>, EventResult<N>>>({ done: true, value })),
        Effect.onExit((exit) => (Exit.isSuccess(exit) && exit.value.done !== true ? Effect.void : close)),
    );
    const generator: AsyncGenerator<Chunk<N>, EventResult<N>> = {
        next: () =>
            runtime.runPromise(read, { signal: next.signal }).catch((error: unknown) => {
                throw next.signal.aborted ? next.signal.reason : error;
            }),
        return: (value) => runtime.runPromise(Effect.andThen(close, Effect.promise(() => Promise.resolve(value)).pipe(Effect.map((settled): IteratorResult<Chunk<N>, EventResult<N>> => ({ done: true, value: settled }))))),
        throw: (error: unknown) => runtime.runPromise(Effect.andThen(close, Effect.die(error))),
        [Symbol.asyncIterator]: () => generator,
        [Symbol.asyncDispose]: () => runtime.runPromise(close),
    };
    return generator;
}

function plugin<R, F>($: EngineInterface, tools: Record.ReadonlyRecord<string, Tool<R, F>>, agents: Record.ReadonlyRecord<string, Agent>) {
    return Layer.effect(
        Plugin,
        Effect.all({
            name: Effect.sync(() => $.plugin.name),
            tools: registered('tool.register', tools, (name, { spec }) => $.tool.register({ ...spec, name }).then(({ tool }) => tool)),
            agents: registered('agent.register', agents, (name, { isOffered: _isOffered, ...spec }) => $.agent.register({ ...spec, name }).then(({ agent }) => agent)),
            blocked: Ref.make(HashSet.empty<string>()),
        }),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Agent, declared, dispatch, type Hook, type Hooks, type Hosted, plugin, Plugin, type Proceed, reply, type StreamHook, stopping, stream, Tool };
