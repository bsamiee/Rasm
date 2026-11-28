import type { Args, Caught, EngineInterface, EventResult, Frozen, HookFailure, Next, On, RenderComponent, RenderElement, RenderInput } from 'claude-code';
import { Array, Effect, type FileSystem, HashSet, Layer, ManagedRuntime, MutableRef, Option, type Path, type PlatformError, Predicate, Record, Ref, Struct, SubscriptionRef } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { type Engine, engine } from '#/engine/interface';
import { call, services } from '#/engine/services';
import { type Draft, draft } from '#/prompt/draft';
import { draw } from '#/ui/element';
import { closing, type Panes, panes } from '#/ui/pane';
import { compacted, type Placed, placed, type Row, seen } from '#/ui/placed';
import { pointer, type Render, render, watched } from '#/ui/render';
import { heard, type Theme, theme } from '#/ui/theme';

// --- [TYPES] ---------------------------------------------------------------------------

type Event = 'session.start' | 'session.end' | 'session.compact' | 'turn.start' | 'turn.complete' | 'agent.report' | 'tool.describe' | 'tool.check' | 'tool.call' | 'prompt.fill' | 'prompt.paint' | 'prompt.suggest' | 'ui.press' | 'ui.input' | 'ui.select' | 'ui.focus' | 'ui.scroll';
type Provided = Engine | Draft | Panes | Placed | Theme | FileSystem.FileSystem | ChildProcessSpawner.ChildProcessSpawner | Path.Path;
type Proceed<N extends Event> = ((input: Frozen<Args<N>>) => Effect.Effect<EventResult<N>, PlatformError.PlatformError>) & Pick<Next<N>, 'budget' | 'signal'>;
type Hook<N extends Event, A, F> = (e: Frozen<Args<N>>, next: Proceed<N>) => Effect.Effect<EventResult<N>, F, A | Provided>;
interface Mod<A, E, F, S = never> {
    readonly layer: Layer.Layer<A, E, Provided>;
    readonly hooks: { readonly [N in Event]?: Hook<N, A, F> };
    readonly band?: { readonly press: (key: string) => Effect.Effect<unknown, F, A | Provided> };
    readonly render?: Render<S, A | Provided, F>;
}
interface Instance<A, E, F, S = never> extends Pick<Mod<A, E, F, S>, 'hooks'> {
    readonly live: MutableRef.MutableRef<Option.Option<ManagedRuntime.ManagedRuntime<A | Provided, E | PlatformError.PlatformError>>>;
    readonly turns: SubscriptionRef.SubscriptionRef<HashSet.HashSet<Option.Option<string>>>;
    readonly theme: Parameters<typeof theme>[1];
    readonly placed: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Row>>;
    readonly render: Option.Option<NonNullable<Mod<A, E, F, S>['render']> & { readonly held: Ref.Ref<Option.Option<S>> }>;
    readonly band: Option.Option<NonNullable<Mod<A, E, F, S>['band']> & { readonly rows: Ref.Ref<Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, string>>> }>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const reasons = { throw: 'threw', timeout: 'ran out of time' } as const satisfies Record<HookFailure['kind'], string>;
const components = [
    'AskUserQuestion',
    'UserMessage',
    'AssistantMessage',
    'CodeBlock',
    'ToolUse',
    'ToolResult',
    'ToolGroup',
    'ToolProgress',
    'CommandOutput',
    'Spinner',
    'TurnDuration',
    'InfoNotice',
    'Welcome',
    'SessionMode',
    'PromptHint',
    'StatusLine',
    'AbovePrompt',
    'AbovePromptList',
    'BesidePrompt',
    'BesidePromptList',
    'AgentRow',
    'ContextMenu',
    'Pane',
    'Placed',
    'Backdrop',
    'Underlay',
    'UnderlayList',
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const held: { readonly [N in Event]?: <A, E, F, S, X, Y, R>(instance: Instance<A, E, F, S>, e: Frozen<Args<N>>, answered: Effect.Effect<X, Y, R>) => Effect.Effect<X, Y, R> } = {
    'session.end': ({ live, turns, placed: rows }, _, answered) =>
        Effect.ensuring(answered, SubscriptionRef.set(turns, HashSet.empty()).pipe(Effect.andThen(SubscriptionRef.set(rows, {})), Effect.andThen(Effect.suspend(() => Option.match(MutableRef.getAndSet(live, Option.none()), { onNone: () => Effect.void, onSome: (made) => made.disposeEffect }))))),
    'session.compact': ({ placed: rows }, _, answered) => Effect.tap(answered, () => compacted(rows)),
    'turn.start': ({ turns }, { agentId }, answered) => Effect.andThen(SubscriptionRef.update(turns, HashSet.add(Option.fromUndefinedOr(agentId))), answered),
    'turn.complete': ({ turns }, { agentId }, answered) => Effect.andThen(SubscriptionRef.update(turns, HashSet.remove(Option.fromUndefinedOr(agentId))), answered),
};
const hosted = <A, E, F, S = never>(mod: Pick<Mod<A, E, F, S>, 'hooks' | 'band' | 'render'>): Instance<A, E, F, S> => ({
    hooks: mod.hooks,
    live: MutableRef.make(Option.none()),
    turns: HashSet.empty<Option.Option<string>>().pipe(SubscriptionRef.make, Effect.runSync),
    theme: Ref.makeUnsafe(Option.none()),
    placed: Effect.runSync(SubscriptionRef.make<Record.ReadonlyRecord<string, Row>>({})),
    render: Option.map(Option.fromUndefinedOr(mod.render), (table) => ({ ...table, held: Ref.makeUnsafe(Option.none()) })),
    band: Option.map(Option.fromUndefinedOr(mod.band), (pressed) => ({ ...pressed, rows: Ref.makeUnsafe({}) })),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

function start<A, E, F, S>($: EngineInterface, instance: Instance<A, E, F, S>, layer: Mod<A, E, F, S>['layer']) {
    return Option.getOrElse(MutableRef.get(instance.live), () => {
        const made = instance.render.pipe(
            watched,
            Layer.provideMerge(
                Layer.provideMerge(
                    layer,
                    Layer.provideMerge(
                        Layer.mergeAll(
                            engine($, instance.turns, Option.map(instance.band, Struct.get('rows'))),
                            draft($),
                            panes(
                                $,
                                instance.render.pipe(
                                    Option.flatMapNullishOr(Struct.get('Pane')),
                                    Option.getOrElse(() => ({})),
                                ),
                            ),
                            placed($, instance.placed),
                            theme($, instance.theme),
                        ),
                        services($),
                    ),
                ),
            ),
            ManagedRuntime.make,
        );
        MutableRef.set(instance.live, Option.some(made));
        made.runPromise(Effect.void).catch(() => {
            MutableRef.compareAndSet(instance.live, Option.some(made), Option.none());
            return made.dispose();
        });
        return made;
    });
}

function answer<A, E, F, S, N extends Event>($: EngineInterface, instance: Instance<A, E, F, S>, layer: Mod<A, E, F, S>['layer'], e: Frozen<Args<N>>, next: ((input: Frozen<Args<N>>) => Promise<EventResult<N>>) & Pick<Next<N>, 'budget' | 'event' | 'signal'>) {
    const proceed: Proceed<N> = Object.assign((input: Frozen<Args<N>>) => call(next.event, 'next', () => next(input)), { budget: next.budget, signal: next.signal });
    const hook: Hook<N, A, F | PlatformError.PlatformError> = instance.hooks[next.event] ?? ((input, onward) => onward(input));
    const answered = hook(e, proceed);
    return start($, instance, layer).runPromise(held[next.event]?.(instance, e, answered) ?? answered, { signal: next.signal });
}

function band<A, E, F, S>($: EngineInterface, instance: Instance<A, E, F, S>, layer: Mod<A, E, F, S>['layer'], e: Frozen<RenderInput<'AbovePrompt'>>, next: (e: Frozen<RenderInput<'AbovePrompt'>>) => Promise<RenderElement>) {
    return Option.match(
        Option.filter(instance.band, () => !e.props.hasSurvey),
        {
            onNone: () => next(e),
            onSome: ({ rows, press }) => {
                const shown = rows.pipe(Ref.getUnsafe, Record.values, Array.flatMap(Record.toEntries));
                return Array.isReadonlyArrayNonEmpty(shown) ? draw($, e, { _tag: 'Box', flexDirection: 'column', children: shown.map(([key, label]) => ({ _tag: 'Button', key, label, onPress: () => press(key) })) }, () => start($, instance, layer)) : next(e);
            },
        },
    );
}

function recover<E, R>($: EngineInterface, e: E, next: ((e: E) => R) & Caught & Pick<Next, 'event' | 'signal'>) {
    const subject = [next.event, ...(Predicate.hasProperty(e, 'tool') && Predicate.isString(e.tool) ? [e.tool] : [])].join(':');
    const line = [`${subject} hook failed`, reasons[next.error.kind], ...Option.toArray(Option.fromUndefinedOr(next.error.message))].join(': ');
    if (!next.signal.aborted) {
        $.ui.log(line, { to: 'debug' });
    }
    return next(e);
}

function host<A, E, F, S = never>(on: On, mod: Mod<A, E, F, S>) {
    const instance = hosted(mod);
    const { layer, hooks } = mod;
    on('session.start', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    on('session.end', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    on('turn.start', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    on('turn.complete', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    on('theme.*', ($, e, next) => heard($, instance.theme, e, next)).catch(recover);
    if (hooks['session.compact'] !== undefined || Option.exists(instance.render, (table) => table.Placed !== undefined)) {
        on('session.compact', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooks['agent.report'] !== undefined) {
        on('agent.report', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooks['tool.describe'] !== undefined) {
        on('tool.describe', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooks['tool.check'] !== undefined) {
        on('tool.check', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooks['tool.call'] !== undefined) {
        on('tool.call', ($, e, next) => (next.origin.plugin === $.plugin.name ? next(e) : answer($, instance, layer, e, next))).catch(recover);
    }
    if (hooks['prompt.fill'] !== undefined) {
        on('prompt.fill', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooks['prompt.paint'] !== undefined) {
        on('prompt.paint', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooks['prompt.suggest'] !== undefined) {
        on('prompt.suggest', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooks['ui.press'] !== undefined) {
        on('ui.press', { plugin: '*' }, ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooks['ui.input'] !== undefined) {
        on('ui.input', { plugin: '*' }, ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooks['ui.select'] !== undefined) {
        on('ui.select', { plugin: '*' }, ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooks['ui.focus'] !== undefined) {
        on('ui.focus', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooks['ui.scroll'] !== undefined) {
        on('ui.scroll', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (Option.isSome(instance.render)) {
        on('ui.render', { component: components satisfies [Exclude<RenderComponent, (typeof components)[number]>] extends [never] ? readonly RenderComponent[] : never }, ($, e, next) => {
            if (e.component === 'Placed') {
                seen($, instance.placed, e);
            }
            return render($, instance.render, e, next, () => start($, instance, layer));
        }).catch(recover);
    }
    if (Option.exists(instance.render, (table) => table.Backdrop !== undefined)) {
        on('ui.pointer', ($, e, next) => pointer($, instance.render, e, next, () => start($, instance, layer))).catch(recover);
    }
    if (Option.exists(instance.render, (table) => table.Pane !== undefined)) {
        on('ui.close', ($, e, next) =>
            closing(
                $,
                instance.render.pipe(
                    Option.flatMapNullishOr(Struct.get('Pane')),
                    Option.getOrElse(() => ({})),
                ),
                e,
                next,
                () => start($, instance, layer),
            ),
        ).catch(recover);
    }
    if (Option.isSome(instance.band)) {
        on('ui.render', { component: 'AbovePrompt' }, ($, e, next) => band($, instance, layer, e, next)).catch(recover);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { answer, host, hosted, type Proceed, recover };
