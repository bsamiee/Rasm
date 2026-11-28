import type { Args, Caught, EngineInterface, EventResult, Frozen, Next, On, RenderElement, RenderInput } from 'claude-code';
import { Array, Effect, type FileSystem, HashSet, Layer, ManagedRuntime, MutableRef, Option, type Path, type PlatformError, Predicate, PubSub, Record, Ref, Struct, SubscriptionRef } from 'effect';
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
import { follow, type Held, pointer, type Render, render } from '#/ui/render';
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
    readonly layer: Layer.Layer<A, E, Provided>;
    readonly tables: readonly Hooks<A | Provided, F | PlatformError.PlatformError>[];
    readonly tools: Record.ReadonlyRecord<string, Tool<A | Provided, F>>;
    readonly agents: Record.ReadonlyRecord<string, Agent>;
    readonly commands: Record.ReadonlyRecord<string, Command<A | Provided, F>>;
    readonly live: MutableRef.MutableRef<Option.Option<ManagedRuntime.ManagedRuntime<A | Provided, E | PlatformError.PlatformError>>>;
    readonly turns: SubscriptionRef.SubscriptionRef<HashSet.HashSet<Option.Option<string>>>;
    readonly theme: Parameters<typeof theme>[1];
    readonly placed: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Row>>;
    readonly render: Held<S, A | Provided, F>;
    readonly band: Option.Option<NonNullable<Mod<A, E, F, S>['band']> & { readonly rows: Ref.Ref<Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, string>>> }>;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

function start<A, E, F, S>($: EngineInterface, instance: Instance<A, E, F, S>) {
    return Option.getOrElse(MutableRef.get(instance.live), () => {
        const made = instance.render.pipe(
            follow,
            Layer.provideMerge(instance.layer),
            Layer.provideMerge(Layer.mergeAll(engine($, instance.turns, Option.map(instance.band, Struct.get('rows'))), Ledger.layer, draft($), panes($, instance.render), placed($, instance.placed), plugin($, instance.tools, instance.agents, instance.commands), theme($, instance.theme))),
            Layer.provideMerge(services($)),
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
    e: Frozen<Args<N>>,
    next: ((input: Frozen<Args<N>>) => Promise<EventResult<N>>) & Pick<Next<N>, 'budget' | 'event' | 'origin' | 'signal'>,
    tables: readonly { readonly [K in N]?: Hook<K, A | Provided, F | PlatformError.PlatformError> }[] = instance.tables,
) {
    return runHook(start($, instance), dispatch(tables, e, next), next.signal);
}

function drawBand<A, E, F, S>($: EngineInterface, instance: Instance<A, E, F, S>, e: Frozen<RenderInput<'AbovePrompt'>>, next: (e: Frozen<RenderInput<'AbovePrompt'>>) => Promise<RenderElement>) {
    return Option.match(
        instance.band.pipe(
            Option.filter(() => !e.props.hasSurvey),
            Option.map(({ rows, press }) => rows.pipe(Ref.getUnsafe, Record.values, Array.flatMap(Record.toEntries), Array.map(([key, label]) => ({ _tag: 'Button', key, label, onPress: () => Effect.forkDetach(Effect.catchCause(press(key), (cause) => Effect.logWarning('press failed', cause).pipe(Effect.annotateLogs({ key })))) }) as const))),
            Option.filter((children) => Array.isReadonlyArrayNonEmpty(children)),
        ),
        { onNone: () => next(e), onSome: (children) => draw($, e, { _tag: 'Box', flexDirection: 'column', children }, () => start($, instance)) },
    );
}

function recover<E, R>($: EngineInterface, e: E, next: ((e: E) => R) & Caught & Pick<Next, 'event' | 'signal'>) {
    const subject = Predicate.hasProperty(e, 'tool') && Predicate.isString(e.tool) ? `${next.event}:${e.tool}` : next.event;
    if (!next.signal.aborted) {
        $.ui.log([`${subject} hook failed`, next.error.kind, next.error.message].filter(Predicate.isString).join(': '), { to: 'debug' });
    }
    return next(e);
}

function host<A, E, F, S = never, const P extends readonly Pick<Part<never, Error, F>, 'layer'>[] = []>(on: On, mod: Mod<A, E, F, S>, ...parts: P & { readonly [K in keyof P]: Part<Layer.Success<P[K]['layer']>, Layer.Error<P[K]['layer']>, F> }) {
    type Served = A | Layer.Success<P[number]['layer']>;
    const shared: readonly Pick<Part<Served, unknown, F>, 'hooks'>[] = parts;
    const instance: Instance<Served, E | Layer.Error<P[number]['layer']>, F, S> = {
        layer: Layer.mergeAll(mod.layer, ...parts.map((part): P[number]['layer'] => part.layer)),
        tables: [
            {
                'session.start': (e, next) => next(e),
                'session.end': (e, next) =>
                    Effect.ensuring(next(e), Effect.all([SubscriptionRef.set(instance.turns, HashSet.empty()), SubscriptionRef.set(instance.placed, {}), Effect.suspend(() => Option.match(MutableRef.getAndSet(instance.live, Option.none()), { onNone: () => Effect.void, onSome: (made) => made.disposeEffect }))])),
                'session.compact': (e, next) => Effect.tap(next(e), (result) => prune(instance.placed, e, result)),
                'turn.start': (e, next) => Effect.andThen(SubscriptionRef.update(instance.turns, HashSet.add(Option.fromUndefinedOr(e.agentId))), next(e)),
                'turn.complete': (e, next) => Effect.andThen(SubscriptionRef.update(instance.turns, HashSet.remove(Option.fromUndefinedOr(e.agentId))), next(e)),
            },
            ...[mod.hooks, ...shared.map(Struct.get('hooks'))].map((table) => {
                const { 'classic.Stop': stop, 'classic.SubagentStop': subagentStop } = table;
                return { ...table, ...(stop === undefined ? {} : { 'classic.Stop': blockOnce(stop) }), ...(subagentStop === undefined ? {} : { 'classic.SubagentStop': blockOnce(subagentStop) }) };
            }),
        ],
        tools: mod.tools ?? {},
        agents: mod.agents ?? {},
        commands: mod.commands ?? {},
        live: MutableRef.make(Option.none()),
        turns: HashSet.empty<Option.Option<string>>().pipe(SubscriptionRef.make, Effect.runSync),
        theme: Ref.makeUnsafe(Option.none()),
        placed: Effect.runSync(SubscriptionRef.make<Record.ReadonlyRecord<string, Row>>({})),
        render: Option.map(Option.fromUndefinedOr(mod.render), (table) => ({ ...table, held: Ref.makeUnsafe(Option.none()), drawn: Effect.runSync(PubSub.unbounded<string>()) })),
        band: Option.map(Option.fromUndefinedOr(mod.band), (pressed) => ({ ...pressed, rows: Ref.makeUnsafe({}) })),
    };
    const { matchers, hooks: bodies } = route(instance.tools, instance.agents, instance.commands);
    const { 'turn.step': step } = mod.hooks;
    const hooked = (event: Hosted) => instance.tables.some((table) => table[event] !== undefined);
    if (hooked('session.start')) {
        on('session.start', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('session.end')) {
        on('session.end', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('session.compact')) {
        on('session.compact', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('session.receive')) {
        on('session.receive', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('session.stop')) {
        on('session.stop', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('plugin.register')) {
        on('plugin.register', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('turn.start')) {
        on('turn.start', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('turn.complete')) {
        on('turn.complete', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('agent.register')) {
        on('agent.register', ($, e, next) => (next.origin.plugin === $.plugin.name ? next(e) : answer($, instance, e, next))).catch(recover);
    }
    if (hooked('agent.offer')) {
        on('agent.offer', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('agent.spawn')) {
        on('agent.spawn', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('agent.report')) {
        on('agent.report', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('tool.describe')) {
        on('tool.describe', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('tool.check')) {
        on('tool.check', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('tool.call')) {
        on('tool.call', ($, e, next) => (next.origin.plugin === $.plugin.name ? next(e) : answer($, instance, e, next))).catch(recover);
    }
    if (hooked('command.run')) {
        on('command.run', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('prompt.submit')) {
        on('prompt.submit', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('prompt.section')) {
        on('prompt.section', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('prompt.context')) {
        on('prompt.context', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('prompt.attachment')) {
        on('prompt.attachment', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('skill.prompt')) {
        on('skill.prompt', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('prompt.fill')) {
        on('prompt.fill', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('prompt.paint')) {
        on('prompt.paint', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('prompt.suggest')) {
        on('prompt.suggest', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('ui.press')) {
        on('ui.press', { plugin: '*' }, ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('ui.input')) {
        on('ui.input', { plugin: '*' }, ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('ui.select')) {
        on('ui.select', { plugin: '*' }, ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('ui.focus')) {
        on('ui.focus', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('ui.scroll')) {
        on('ui.scroll', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('classic.Stop')) {
        on('classic.Stop', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('classic.SubagentStop')) {
        on('classic.SubagentStop', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('classic.Notification')) {
        on('classic.Notification', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    if (hooked('classic.StopFailure')) {
        on('classic.StopFailure', ($, e, next) => answer($, instance, e, next)).catch(recover);
    }
    on('theme.*', ($, e, next) => observe($, instance.theme, e, next)).catch(recover);
    if (step !== undefined) {
        on('turn.step', async function* ($, e, next) {
            return yield* stream(e, next, step, start($, instance));
        });
    }
    if (!Record.isEmptyReadonlyRecord(instance.tools)) {
        on('tool.run', matchers['tool.run'], ($, e, next) => runHook(start($, instance), dispatch([bodies], e, next), next.signal)).catch(($, e, next) => (next.called ? recover($, e, next) : { deny: `${e.tool} hook failed: ${next.error.kind}` }));
    }
    if (!Record.isEmptyReadonlyRecord(instance.agents)) {
        on('agent.offer', matchers['agent.offer'], ($, e, next) => answer($, instance, e, next, [bodies])).catch(($, e, next) => (next.called ? recover($, e, next) : { isOffered: false }));
    }
    if (!Record.isEmptyReadonlyRecord(instance.commands)) {
        on('command.run', matchers['command.run'], ($, e, next) => answer($, instance, e, next, [bodies])).catch(recover);
    }
    if (Option.isSome(instance.render)) {
        on('ui.render', { component: Struct.keys(Struct.omit(instance.render.value, ['state', 'held', 'drawn'])) }, ($, e, next) => {
            if (e.component === 'Placed') {
                record($, instance.placed, e);
            }
            return render($, instance.render, e, next, () => start($, instance));
        }).catch(recover);
    }
    if (Option.exists(instance.render, (table) => table.Backdrop !== undefined)) {
        on('ui.pointer', ($, e, next) => pointer($, instance.render, e, next, () => start($, instance))).catch(recover);
    }
    const paneRows = Option.flatMapNullishOr(instance.render, Struct.get('Pane'));
    if (Option.isSome(paneRows)) {
        on('ui.close', ($, e, next) => closePane($, paneRows.value, e, next, () => start($, instance))).catch(recover);
    }
    if (Option.isSome(instance.band)) {
        on('ui.render', { component: 'AbovePrompt' }, ($, e, next) => drawBand($, instance, e, next)).catch(recover);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { host, type Part };
