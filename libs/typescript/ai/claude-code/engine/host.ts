import type { Args, Caught, EngineInterface, EventResult, Frozen, HookFailure, Next, On, RenderElement, RenderInput } from 'claude-code';
import { Array, Effect, type FileSystem, HashSet, Layer, ManagedRuntime, MutableRef, Option, type Path, type PlatformError, Predicate, Record, Ref, Struct, SubscriptionRef } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { type Engine, engine } from '#/engine/interface';
import { call, services } from '#/engine/services';
import { type Draft, draft } from '#/prompt/draft';
import { draw } from '#/ui/element';
import { heard, type Theme, theme } from '#/ui/theme';

// --- [TYPES] ---------------------------------------------------------------------------

type Event = 'session.start' | 'session.end' | 'session.compact' | 'turn.start' | 'turn.complete' | 'agent.report' | 'tool.describe' | 'tool.check' | 'tool.call' | 'prompt.fill' | 'prompt.paint' | 'prompt.suggest' | 'ui.press' | 'ui.input' | 'ui.select' | 'ui.focus' | 'ui.scroll';
type Provided = Engine | Draft | Theme | FileSystem.FileSystem | ChildProcessSpawner.ChildProcessSpawner | Path.Path;
type Proceed<N extends Event> = ((input: Frozen<Args<N>>) => Effect.Effect<EventResult<N>, PlatformError.PlatformError>) & Pick<Next<N>, 'budget' | 'signal'>;
type Hook<N extends Event, A, F> = (e: Frozen<Args<N>>, next: Proceed<N>) => Effect.Effect<EventResult<N>, F, A | Provided>;
interface Mod<A, E, F> {
    readonly layer: Layer.Layer<A, E, Provided>;
    readonly hooks: { readonly [N in Event]?: Hook<N, A, F> };
    readonly band?: { readonly press: (key: string) => Effect.Effect<unknown, F, A | Provided> };
}
interface Instance<A, E, F> extends Pick<Mod<A, E, F>, 'hooks'> {
    readonly live: MutableRef.MutableRef<Option.Option<ManagedRuntime.ManagedRuntime<A | Provided, E | PlatformError.PlatformError>>>;
    readonly turns: SubscriptionRef.SubscriptionRef<HashSet.HashSet<Option.Option<string>>>;
    readonly theme: Parameters<typeof theme>[1];
    readonly band: Option.Option<NonNullable<Mod<A, E, F>['band']> & { readonly rows: Ref.Ref<Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, string>>> }>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const reasons = { throw: 'threw', timeout: 'ran out of time' } as const satisfies Record<HookFailure['kind'], string>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const held: { readonly [N in Event]?: <A, E, F, X, Y, R>(instance: Instance<A, E, F>, e: Frozen<Args<N>>, answered: Effect.Effect<X, Y, R>) => Effect.Effect<X, Y, R> } = {
    'session.end': ({ live, turns }, _, answered) => Effect.ensuring(answered, SubscriptionRef.set(turns, HashSet.empty()).pipe(Effect.andThen(Effect.suspend(() => Option.match(MutableRef.getAndSet(live, Option.none()), { onNone: () => Effect.void, onSome: (made) => made.disposeEffect }))))),
    'turn.start': ({ turns }, { agentId }, answered) => Effect.andThen(SubscriptionRef.update(turns, HashSet.add(Option.fromUndefinedOr(agentId))), answered),
    'turn.complete': ({ turns }, { agentId }, answered) => Effect.andThen(SubscriptionRef.update(turns, HashSet.remove(Option.fromUndefinedOr(agentId))), answered),
};
const hosted = <A, E, F>(mod: Pick<Mod<A, E, F>, 'hooks' | 'band'>): Instance<A, E, F> => ({
    hooks: mod.hooks,
    live: MutableRef.make(Option.none()),
    turns: HashSet.empty<Option.Option<string>>().pipe(SubscriptionRef.make, Effect.runSync),
    theme: Ref.makeUnsafe(Option.none()),
    band: Option.map(Option.fromUndefinedOr(mod.band), (pressed) => ({ ...pressed, rows: Ref.makeUnsafe({}) })),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

function start<A, E, F>($: EngineInterface, instance: Instance<A, E, F>, layer: Mod<A, E, F>['layer']) {
    return Option.getOrElse(MutableRef.get(instance.live), () => {
        const made = ManagedRuntime.make(Layer.provideMerge(layer, Layer.provideMerge(Layer.mergeAll(engine($, instance.turns, Option.map(instance.band, Struct.get('rows'))), draft($), theme($, instance.theme)), services($))));
        MutableRef.set(instance.live, Option.some(made));
        made.runPromise(Effect.void).catch(() => {
            MutableRef.compareAndSet(instance.live, Option.some(made), Option.none());
            return made.dispose();
        });
        return made;
    });
}

function answer<A, E, F, N extends Event>($: EngineInterface, instance: Instance<A, E, F>, layer: Mod<A, E, F>['layer'], e: Frozen<Args<N>>, next: ((input: Frozen<Args<N>>) => Promise<EventResult<N>>) & Pick<Next<N>, 'budget' | 'event' | 'signal'>) {
    const proceed: Proceed<N> = Object.assign((input: Frozen<Args<N>>) => call(next.event, 'next', () => next(input)), { budget: next.budget, signal: next.signal });
    const hook: Hook<N, A, F | PlatformError.PlatformError> = instance.hooks[next.event] ?? ((input, onward) => onward(input));
    const answered = hook(e, proceed);
    return start($, instance, layer).runPromise(held[next.event]?.(instance, e, answered) ?? answered, { signal: next.signal });
}

function band<A, E, F>($: EngineInterface, instance: Instance<A, E, F>, layer: Mod<A, E, F>['layer'], e: Frozen<RenderInput<'AbovePrompt'>>, next: (e: Frozen<RenderInput<'AbovePrompt'>>) => Promise<RenderElement>) {
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

function host<A, E, F>(on: On, mod: Mod<A, E, F>) {
    const instance = hosted(mod);
    const { layer, hooks } = mod;
    on('session.start', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    on('session.end', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    on('turn.start', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    on('turn.complete', ($, e, next) => answer($, instance, layer, e, next)).catch(recover);
    on('theme.*', ($, e, next) => heard($, instance.theme, e, next)).catch(recover);
    if (hooks['session.compact'] !== undefined) {
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
    if (Option.isSome(instance.band)) {
        on('ui.render', { component: 'AbovePrompt' }, ($, e, next) => band($, instance, layer, e, next)).catch(recover);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { answer, host, hosted, type Proceed, recover };
