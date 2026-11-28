import type { Args, Caught, EngineInterface, EventName, EventResult, Frozen, HookFailure, Next, On, RenderElement, RenderInput } from 'claude-code';
import { Array, Effect, type FileSystem, HashSet, Layer, ManagedRuntime, MutableRef, Option, type Path, type PlatformError, Predicate, Record, Ref, Struct, SubscriptionRef } from 'effect';
import type { KeyValueStore } from 'effect/persistence';
import type { ChildProcessSpawner } from 'effect/process';
import { type Agent, type Command, route, dispatch, type Hook, type Hooks, type Hosted, plugin, type Plugin, runHook, blockOnce, stream, type Tool } from '#/engine/events';
import { type Engine, engine } from '#/engine/interface';
import { Ledger } from '#/engine/ledger';
import { services } from '#/engine/services';
import { type Draft, draft } from '#/prompt/draft';
import { draw } from '#/ui/element';
import { closePane, type Panes, panes } from '#/ui/pane';
import { prune, type Placed, placed, type Row, record } from '#/ui/placed';
import { pointer, type Render, render, follow } from '#/ui/render';
import { observe, type Theme, theme } from '#/ui/theme';

// --- [TYPES] ---------------------------------------------------------------------------

type Provided = Engine | Ledger | Draft | Panes | Placed | Plugin | Theme | FileSystem.FileSystem | KeyValueStore.KeyValueStore | ChildProcessSpawner.ChildProcessSpawner | Path.Path;
interface Part<A, E, F> {
    readonly layer: Layer.Layer<A, E, Provided>;
    readonly hooks: Omit<Hooks<A | Provided, F>, 'turn.step'>;
}
interface Mod<A, E, F, S = never> {
    readonly layer: Layer.Layer<A, E, Provided>;
    readonly hooks: Hooks<A | Provided, F>;
    readonly tools?: Record.ReadonlyRecord<string, Tool<A | Provided, F>>;
    readonly agents?: Record.ReadonlyRecord<string, Agent>;
    readonly commands?: Record.ReadonlyRecord<string, Command<A | Provided, F>>;
    readonly band?: { readonly press: (key: string) => Effect.Effect<unknown, F, A | Provided> };
    readonly render?: Render<S, A | Provided, F>;
}
interface Instance<A, E, F, S = never> {
    readonly tables: readonly Hooks<A | Provided, F | PlatformError.PlatformError>[];
    readonly tools: Record.ReadonlyRecord<string, Tool<A | Provided, F>>;
    readonly agents: Record.ReadonlyRecord<string, Agent>;
    readonly commands: Record.ReadonlyRecord<string, Command<A | Provided, F>>;
    readonly live: MutableRef.MutableRef<Option.Option<ManagedRuntime.ManagedRuntime<A | Provided, E | PlatformError.PlatformError>>>;
    readonly turns: SubscriptionRef.SubscriptionRef<HashSet.HashSet<Option.Option<string>>>;
    readonly theme: Parameters<typeof theme>[1];
    readonly placed: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Row>>;
    readonly render: Option.Option<NonNullable<Mod<A, E, F, S>['render']> & { readonly held: Ref.Ref<Option.Option<S>> }>;
    readonly band: Option.Option<NonNullable<Mod<A, E, F, S>['band']> & { readonly rows: Ref.Ref<Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, string>>> }>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const REASONS = { throw: 'threw', timeout: 'ran out of time' } as const satisfies Record<HookFailure['kind'], string>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const TRACK: { readonly [N in EventName]?: <A, E, F, S, Y, R>(instance: Instance<A, E, F, S>, e: Frozen<Args<N>>, answered: Effect.Effect<EventResult<N>, Y, R>) => Effect.Effect<EventResult<N>, Y, R> } = {
    'session.end': ({ live, turns, placed: rows }, _, answered) =>
        Effect.ensuring(answered, SubscriptionRef.set(turns, HashSet.empty()).pipe(Effect.andThen(SubscriptionRef.set(rows, {})), Effect.andThen(Effect.suspend(() => Option.match(MutableRef.getAndSet(live, Option.none()), { onNone: () => Effect.void, onSome: (made) => made.disposeEffect }))))),
    'session.compact': ({ placed: rows }, e, answered) => Effect.tap(answered, (result) => prune(rows, e, result)),
    'turn.start': ({ turns }, { agentId }, answered) => Effect.andThen(SubscriptionRef.update(turns, HashSet.add(Option.fromUndefinedOr(agentId))), answered),
    'turn.complete': ({ turns }, { agentId }, answered) => Effect.andThen(SubscriptionRef.update(turns, HashSet.remove(Option.fromUndefinedOr(agentId))), answered),
};
const instantiate = <A, E, F, S = never>(mod: Pick<Mod<A, E, F, S>, 'hooks' | 'tools' | 'agents' | 'commands' | 'band' | 'render'>, parts: readonly Pick<Part<A, E, F>, 'hooks'>[]): Instance<A, E, F, S> => ({
    tables: [mod.hooks, ...parts.map(Struct.get('hooks'))].map((table) => {
        const { 'classic.Stop': stop, 'classic.SubagentStop': subagentStop } = table;
        return { ...table, ...(stop === undefined ? {} : { 'classic.Stop': blockOnce(stop) }), ...(subagentStop === undefined ? {} : { 'classic.SubagentStop': blockOnce(subagentStop) }) };
    }),
    tools: mod.tools ?? {},
    agents: mod.agents ?? {},
    commands: mod.commands ?? {},
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
            follow,
            Layer.provideMerge(
                Layer.provideMerge(
                    layer,
                    Layer.provideMerge(
                        Layer.mergeAll(
                            engine($, instance.turns, Option.map(instance.band, Struct.get('rows'))),
                            Ledger.layer,
                            draft($),
                            panes(
                                $,
                                instance.render.pipe(
                                    Option.flatMapNullishOr(Struct.get('Pane')),
                                    Option.getOrElse(() => ({})),
                                ),
                            ),
                            placed($, instance.placed),
                            plugin($, instance.tools, instance.agents, instance.commands),
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

function answer<A, E, F, S, N extends Hosted>(
    $: EngineInterface,
    instance: Instance<A, E, F, S>,
    layer: Mod<A, E, F, S>['layer'],
    e: Frozen<Args<N>>,
    next: ((input: Frozen<Args<N>>) => Promise<EventResult<N>>) & Pick<Next<N>, 'budget' | 'event' | 'origin' | 'signal'>,
    tables: readonly { readonly [K in N]?: Hook<K, A | Provided, F | PlatformError.PlatformError> }[] = instance.tables,
) {
    const answered = dispatch(tables, e, next);
    return runHook(start($, instance, layer), TRACK[next.event]?.(instance, e, answered) ?? answered, next.signal);
}

function band<A, E, F, S>($: EngineInterface, instance: Instance<A, E, F, S>, layer: Mod<A, E, F, S>['layer'], e: Frozen<RenderInput<'AbovePrompt'>>, next: (e: Frozen<RenderInput<'AbovePrompt'>>) => Promise<RenderElement>) {
    return Option.match(
        Option.filter(instance.band, () => !e.props.hasSurvey),
        {
            onNone: () => next(e),
            onSome: ({ rows, press }) => {
                const shown = rows.pipe(Ref.getUnsafe, Record.values, Array.flatMap(Record.toEntries));
                return Array.isReadonlyArrayNonEmpty(shown)
                    ? draw($, e, { _tag: 'Box', flexDirection: 'column', children: shown.map(([key, label]) => ({ _tag: 'Button', key, label, onPress: () => Effect.sync(() => start($, instance, layer).runFork(Effect.catchCause(press(key), (cause) => Effect.logWarning(`${key} press failed`, cause)))) })) }, () =>
                          start($, instance, layer),
                      )
                    : next(e);
            },
        },
    );
}

function recover<E, R>($: EngineInterface, e: E, next: ((e: E) => R) & Caught & Pick<Next, 'event' | 'signal'>) {
    const subject = [next.event, ...(Predicate.hasProperty(e, 'tool') && Predicate.isString(e.tool) ? [e.tool] : [])].join(':');
    const line = [`${subject} hook failed`, REASONS[next.error.kind], ...Option.toArray(Option.fromUndefinedOr(next.error.message))].join(': ');
    if (!next.signal.aborted) {
        $.ui.log(line, { to: 'debug' });
    }
    return next(e);
}

function host<A, E, F, S = never, const P extends readonly Pick<Part<never, Error, F>, 'layer'>[] = []>(on: On, mod: Mod<A, E, F, S>, ...parts: P & { readonly [K in keyof P]: Part<Layer.Success<P[K]['layer']>, Layer.Error<P[K]['layer']>, F> }) {
    const instance = instantiate<A | Layer.Success<P[number]['layer']>, E | Layer.Error<P[number]['layer']>, F, S>(mod, parts);
    const layer = Layer.mergeAll(mod.layer, ...parts.map((part): P[number]['layer'] => part.layer));
    const hooked = (event: Hosted) => instance.tables.some((table) => table[event] !== undefined);
    const { matchers, hooks: bodies } = route(instance.tools, instance.agents, instance.commands);
    const { 'turn.step': step } = mod.hooks;
    on('session.start', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    on('session.end', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    on('turn.start', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    on('turn.complete', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    on('theme.*', ($, e, next) => observe($, instance.theme, e, next)).catch(recover);
    if (hooked('session.compact') || Option.exists(instance.render, (table) => table.Placed !== undefined)) {
        on('session.compact', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('session.receive')) {
        on('session.receive', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('session.stop')) {
        on('session.stop', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('plugin.register')) {
        on('plugin.register', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('agent.register')) {
        on('agent.register', ($, e, next) => (next.origin.plugin === $.plugin.name ? next(e) : answer($, instance, layer, e, next))).catch(recover);
    }
    if (hooked('agent.offer')) {
        on('agent.offer', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('agent.spawn')) {
        on('agent.spawn', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('agent.report')) {
        on('agent.report', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('tool.describe')) {
        on('tool.describe', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('tool.check')) {
        on('tool.check', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('tool.call')) {
        on('tool.call', ($, e, next) => (next.origin.plugin === $.plugin.name ? next(e) : answer($, instance, layer, e, next))).catch(recover);
    }
    if (hooked('command.run')) {
        on('command.run', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('prompt.submit')) {
        on('prompt.submit', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('prompt.section')) {
        on('prompt.section', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('prompt.context')) {
        on('prompt.context', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('prompt.attachment')) {
        on('prompt.attachment', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('skill.prompt')) {
        on('skill.prompt', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('prompt.fill')) {
        on('prompt.fill', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('prompt.paint')) {
        on('prompt.paint', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('prompt.suggest')) {
        on('prompt.suggest', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('ui.press')) {
        on('ui.press', { plugin: '*' }, ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('ui.input')) {
        on('ui.input', { plugin: '*' }, ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('ui.select')) {
        on('ui.select', { plugin: '*' }, ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('ui.focus')) {
        on('ui.focus', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('ui.scroll')) {
        on('ui.scroll', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('classic.Stop')) {
        on('classic.Stop', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('classic.SubagentStop')) {
        on('classic.SubagentStop', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('classic.Notification')) {
        on('classic.Notification', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (hooked('classic.StopFailure')) {
        on('classic.StopFailure', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    }
    if (step !== undefined) {
        on('turn.step', async function* ($, e, next) {
            return yield* stream(e, next, step, start($, instance, layer));
        });
    }
    if (!Record.isEmptyReadonlyRecord(instance.tools)) {
        on('tool.run', matchers['tool.run'], ($, e, next) => runHook(start($, instance, layer), dispatch([bodies], e, next), next.signal)).catch((_, e, next) => (next.called ? next(e) : { deny: `${e.tool} hook failed: ${REASONS[next.error.kind]}` }));
    }
    if (!Record.isEmptyReadonlyRecord(instance.agents)) {
        on('agent.offer', matchers['agent.offer'], ($, e, next) => answer($, instance, layer, e, next, [bodies])).catch((_, e, next) => (next.called ? next(e) : { isOffered: false }));
    }
    if (!Record.isEmptyReadonlyRecord(instance.commands)) {
        on('command.run', matchers['command.run'], ($, e, next) => answer($, instance, layer, e, next, [bodies])).catch(recover);
    }
    if (Option.isSome(instance.render)) {
        on('ui.render', { component: Struct.keys(Struct.omit(instance.render.value, ['state', 'held'])) }, ($, e, next) => {
            if (e.component === 'Placed') {
                record($, instance.placed, e);
            }
            return render($, instance.render, e, next, () => start($, instance, layer));
        }).catch(recover);
    }
    if (Option.exists(instance.render, (table) => table.Backdrop !== undefined)) {
        on('ui.pointer', ($, e, next) => pointer($, instance.render, e, next, () => start($, instance, layer))).catch(recover);
    }
    if (Option.exists(instance.render, (table) => table.Pane !== undefined)) {
        on('ui.close', ($, e, next) =>
            closePane(
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

export { host, type Part };
