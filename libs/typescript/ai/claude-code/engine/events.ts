import type { Args, Chunk, EngineInterface, EventName, EventResult, Frozen, HookStream, Next, StreamingEventName, StreamNext, ToolCallResult, ToolSpec, TurnStepInput, TurnStepResult } from 'claude-code';
import { Array, Cause, Channel, Context, Effect, Exit, Formatter, HashSet, Layer, type ManagedRuntime, Option, PlatformError, Predicate, Pull, Record, Ref, Result, Schema, Scope } from 'effect';
import { Tool as AiTool } from 'effect/ai';
import { SecureRandom } from 'effect/crypto';
import { call } from '#/engine/services';

// --- [TYPES] ---------------------------------------------------------------------------

type Picked = 'budget' | 'signal' | 'origin';
type Proceed<N extends EventName> = ((input: Frozen<Args<N>>) => Effect.Effect<EventResult<N>, PlatformError.PlatformError>) & Pick<Next<N>, Picked>;
type Onward<N extends StreamingEventName> = ((input: Frozen<Args<N>>) => Channel.Channel<Chunk<N>, Cause.UnknownError, EventResult<N>>) & Pick<StreamNext<N>, Picked>;
type Hook<N extends EventName, R, F> = (e: Frozen<Args<N>>, next: Proceed<N>) => Effect.Effect<EventResult<N>, F, R>;
type StreamHook<N extends StreamingEventName, R, F> = (e: Frozen<Args<N>>, next: Onward<N>) => Channel.Channel<Chunk<N>, F, EventResult<N>, unknown, unknown, unknown, R>;

// --- [MODELS] --------------------------------------------------------------------------

interface Tool<R, F> extends Omit<ToolSpec, 'name'> {
    readonly run: (e: Frozen<Args<'tool.run'>>, next: Proceed<'tool.run'>) => Effect.Effect<ToolCallResult, F | Schema.SchemaError | PlatformError.PlatformError, R>;
}
interface Agent extends Omit<Parameters<EngineInterface['agent']['register']>[0], 'name'> {
    readonly isOffered: boolean;
}

const Tool = <S extends Schema.Decoder<unknown>, R, F>(
    input: S,
    { run, ...spec }: Omit<ToolSpec, 'name' | 'inputSchema'> & { readonly run: (input: S['Type'], e: Frozen<Args<'tool.run'>>, next: Proceed<'tool.run'>) => Effect.Effect<ToolCallResult, F | PlatformError.PlatformError, R> },
): Tool<R | S['DecodingServices'], F> => {
    const decode = Schema.decodeUnknownEffect(input);
    return { ...spec, inputSchema: AiTool.getJsonSchemaFromSchema(input), run: (e, next) => Effect.flatMap(decode(e), (decoded) => run(decoded, e, next)) };
};

// --- [SERVICES] ------------------------------------------------------------------------

class Plugin extends Context.Service<
    Plugin,
    {
        readonly name: string;
        readonly tools: Record.ReadonlyRecord<string, string>;
        readonly agents: Record.ReadonlyRecord<string, string>;
        readonly blocked: Ref.Ref<HashSet.HashSet<readonly [string | undefined, string]>>;
    }
>()('@rasm/claude-code/engine/events/Plugin') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const carry = <G extends object>(run: G, next: Pick<Next, Picked>) => Object.defineProperty(Object.assign(run, { budget: next.budget, signal: next.signal, origin: next.origin }), 'budget', { get: () => next.budget });
const dispatch = <N extends EventName, R, F>(tables: readonly { readonly [K in N]?: Hook<K, R, F> }[], e: Frozen<Args<N>>, next: ((input: Frozen<Args<N>>) => Promise<EventResult<N>>) & Pick<Next<N>, Picked | 'event'>): Effect.Effect<EventResult<N>, PlatformError.PlatformError, R> =>
    Effect.flatMap(Effect.context<R>(), (context) => {
        const lower = (failure: F | PlatformError.PlatformError) => (PlatformError.isPlatformError(failure) ? failure : PlatformError.systemError({ _tag: 'Unknown', module: 'hook', method: next.event, description: Formatter.format(failure), cause: failure }));
        const proceed = tables.reduceRight<Proceed<N>>(
            (onward, table) =>
                Option.match(Option.fromUndefinedOr(table[next.event]), {
                    onNone: () => onward,
                    onSome: (hook) => carry((input: Frozen<Args<N>>) => hook(input, onward).pipe(Effect.provideContext(context), Effect.mapError(lower)), next),
                }),
            carry((input: Frozen<Args<N>>) => call(next.event, 'next', () => next(input)), next),
        );
        return proceed(e);
    });
const runHook = <A, E, R, X>(runtime: ManagedRuntime.ManagedRuntime<R, X>, effect: Effect.Effect<A, E, R>, signal: AbortSignal): Promise<A> =>
    runtime.runPromise(effect, { signal }).catch((error: unknown) => {
        signal.throwIfAborted();
        throw error;
    });
const reply = (e: Frozen<TurnStepInput>, { answer, toolUses }: Pick<TurnStepResult, 'answer' | 'toolUses'>) => {
    const text = answer === '' ? [] : [{ kind: 'text', index: 0, text: answer } as const];
    const stopReason = Array.isReadonlyArrayNonEmpty(toolUses) ? 'tool_use' : 'end_turn';
    return Channel.fromIterable<Chunk<'turn.step'>, TurnStepResult>({
        *[Symbol.iterator]() {
            yield* text;
            yield* toolUses.flatMap(
                ({ name, input }, at) =>
                    [
                        { kind: 'tool', index: text.length + at, id: `toolu_${SecureRandom.uuidV7Unsafe().replaceAll('-', '')}`, name },
                        { kind: 'input', index: text.length + at, json: JSON.stringify(input) },
                    ] as const,
            );
            yield { kind: 'stop', stopReason, usage: null } as const;
            return { turnId: e.turnId, index: e.index, answer, toolUses, stopReason, usage: null };
        },
    });
};
const findDeclared = <A>(named: Record.ReadonlyRecord<string, string>, declarations: Record.ReadonlyRecord<string, A>, full: string) =>
    Option.flatMap(
        Record.findFirst(named, (name) => name === full),
        ([key]) => Option.map(Record.get(declarations, key), (declaration) => [key, declaration] as const),
    );
const explain = (cause: Cause.Cause<unknown>) => {
    const squashed = Cause.squash(cause);
    return Predicate.isError(squashed) ? squashed.message : Formatter.format(squashed);
};
const blockOnce =
    <N extends 'classic.Stop' | 'classic.SubagentStop', R, F>(hook: Hook<N, R, F>): Hook<N, R | Plugin, F | PlatformError.PlatformError> =>
    (e, next) =>
        e.stop_hook_active
            ? next(e)
            : Effect.gen(function* () {
                  const [result, { blocked }] = yield* Effect.all([hook(e, next), Plugin]);
                  const { block } = result;
                  const fresh = block === undefined || (yield* Ref.modify(blocked, (held) => [!HashSet.has(held, [e.agent_id, block]), HashSet.add(held, [e.agent_id, block])] as const));
                  return fresh ? result : yield* next(e);
              });
const route = <R, F>(tools: Record.ReadonlyRecord<string, Tool<R, F>>, agents: Record.ReadonlyRecord<string, Agent>) => ({
    matchers: {
        'tool.run': { tool: new RegExp(`^mcp__(?:(?!__)[^])+__(?:${Record.keys(tools).map(RegExp.escape).join('|')})$`, 'u') },
        'agent.offer': { agent: new RegExp(`^[^:]+:(?:${Record.keys(agents).map(RegExp.escape).join('|')})$`, 'u') },
    },
    hooks: {
        'tool.run': (e, next) =>
            Plugin.use(({ tools: named }) =>
                Option.match(findDeclared(named, tools, e.tool), {
                    onNone: () => next(e),
                    onSome: ([key, tool]) => Effect.catchCause(tool.run(e, next), (cause) => Effect.as(Effect.logWarning('tool failed', cause).pipe(Effect.annotateLogs({ tool: key })), { deny: `${key}: ${explain(cause)}` })),
                }),
            ),
        'agent.offer': (e, next) =>
            Plugin.use(({ name, agents: named }) =>
                Option.match(findDeclared(named, agents, e.agent), {
                    onNone: () => next(e),
                    onSome: ([, { isOffered }]) => (isOffered || next.origin.plugin === name ? next(e) : Effect.succeed({ isOffered: false })),
                }),
            ),
    } satisfies { readonly [N in 'tool.run' | 'agent.offer']: Hook<N, R | Plugin, PlatformError.PlatformError> },
});
const registerAll = <A>(event: 'tool.register' | 'agent.register', declarations: Record.ReadonlyRecord<string, A>, register: (name: string, declaration: A) => Promise<string>) =>
    Effect.map(
        Effect.filterMapEffect(Record.toEntries(declarations), ([key, declaration]) =>
            call(event, key, () => register(key, declaration)).pipe(
                Effect.map((full) => Result.succeed([key, full] as const)),
                Effect.catchCause((cause) => Effect.as(Effect.logWarning('register failed', cause).pipe(Effect.annotateLogs({ event, name: key })), Result.failVoid)),
            ),
        ),
        Record.fromEntries,
    );
const stream = <N extends StreamingEventName, R, E, F>(e: Frozen<Args<N>>, next: ((input: Frozen<Args<N>>) => HookStream<Chunk<N>, EventResult<N>>) & Pick<StreamNext<N>, Picked | 'event'>, hook: StreamHook<N, R, F>, runtime: ManagedRuntime.ManagedRuntime<R, E>): AsyncGenerator<Chunk<N>, EventResult<N>> => {
    const onward: Onward<N> = carry((input: Frozen<Args<N>>) => Channel.fromAsyncIterable(next(input), (cause) => new Cause.UnknownError(cause, `${next.event} next failed`)), next);
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
        next: () => runHook(runtime, read, next.signal),
        return: (value) => Promise.all([runtime.runPromise(close), value]).then(([, held]) => ({ done: true, value: held })),
        throw: (error: unknown) => runtime.runPromise(Effect.andThen(close, Effect.die(error))),
        [Symbol.asyncIterator]: () => generator,
        [Symbol.asyncDispose]: () => runtime.runPromise(close),
    };
    return generator;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

function plugin<R, F>($: EngineInterface, tools: Record.ReadonlyRecord<string, Tool<R, F>>, agents: Record.ReadonlyRecord<string, Agent>) {
    return Layer.effect(
        Plugin,
        Effect.all({
            tools: registerAll('tool.register', tools, (name, { run: _run, ...spec }) => $.tool.register({ ...spec, name }).then(({ tool }) => tool)),
            agents: registerAll('agent.register', agents, (name, { isOffered: _isOffered, ...spec }) => $.agent.register({ ...spec, name }).then(({ agent }) => agent)),
            blocked: Ref.make(HashSet.empty<readonly [string | undefined, string]>()),
        }).pipe(Effect.map((registered) => ({ ...registered, name: $.plugin.name }))),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Agent, blockOnce, dispatch, type Hook, plugin, Plugin, type Proceed, reply, route, runHook, stream, type StreamHook, Tool };
