import type { Args, Caught, EngineInterface, EventResult, Frozen, HookFailure, Next, On, RenderElement, RenderInput } from 'claude-code';
import { Array, Effect, type FileSystem, HashSet, Layer, ManagedRuntime, MutableRef, Option, type Path, type PlatformError, Predicate, Record, Ref, SubscriptionRef } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { type Engine, engine } from '#/engine/interface';
import { call, services } from '#/engine/services';

// --- [TYPES] ---------------------------------------------------------------------------

type Event = 'session.start' | 'session.end' | 'session.compact' | 'turn.start' | 'turn.complete' | 'agent.report' | 'tool.describe' | 'tool.check' | 'tool.call';
type Provided = Engine | FileSystem.FileSystem | ChildProcessSpawner.ChildProcessSpawner | Path.Path;
type Proceed<N extends Event> = ((input: Args<N>) => Effect.Effect<EventResult<N>, PlatformError.PlatformError>) & Pick<Next<N>, 'budget' | 'signal'>;
type Hook<N extends Event, A, F> = (e: Frozen<Args<N>>, next: Proceed<N>) => Effect.Effect<EventResult<N>, F, A | Provided>;
interface Mod<A, E, F> {
    readonly layer: Layer.Layer<A, E, Provided>;
    readonly hooks: { readonly [N in Event]?: Hook<N, A, F> };
    readonly band?: { readonly press: (key: string) => Effect.Effect<unknown, F, A | Provided> };
}
interface Instance<A, E, F> {
    readonly mod: Mod<A, E, F>;
    readonly live: MutableRef.MutableRef<Option.Option<ManagedRuntime.ManagedRuntime<A | Provided, E | PlatformError.PlatformError>>>;
    readonly turns: SubscriptionRef.SubscriptionRef<HashSet.HashSet<Option.Option<string>>>;
    readonly rows: Ref.Ref<Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, string>>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const reasons = { throw: 'threw', timeout: 'ran out of time' } as const satisfies Record<HookFailure['kind'], string>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const lift = <N extends Event>(next: ((input: Args<N>) => Promise<EventResult<N>>) & Pick<Next<N>, 'budget' | 'event' | 'signal'>): Proceed<N> => Object.assign((input: Args<N>) => call(next.event, 'next', () => next(input)), { budget: next.budget, signal: next.signal });
const track = <A, E, F>({ turns }: Instance<A, E, F>, agent: string | undefined, running: boolean) => Effect.runSync(SubscriptionRef.update(turns, running ? HashSet.add(Option.fromUndefinedOr(agent)) : HashSet.remove(Option.fromUndefinedOr(agent))));

// --- [COMPOSITION] ---------------------------------------------------------------------

function start<A, E, F>($: EngineInterface, instance: Instance<A, E, F>) {
    return Option.getOrElse(MutableRef.get(instance.live), () => {
        const made = ManagedRuntime.make(Layer.provideMerge(instance.mod.layer, Layer.provideMerge(engine($, instance.turns, Option.as(Option.fromUndefinedOr(instance.mod.band), instance.rows)), services($))));
        MutableRef.set(instance.live, Option.some(made));
        made.runPromise(Effect.void).catch(() => {
            MutableRef.compareAndSet(instance.live, Option.some(made), Option.none());
            return made.dispose();
        });
        return made;
    });
}

function answer<A, E, F, N extends Event>($: EngineInterface, instance: Instance<A, E, F>, hook: Hook<N, A, F>, e: Frozen<Args<N>>, next: Parameters<typeof lift<N>>[0]) {
    return start($, instance).runPromise(hook(e, lift(next)), { signal: next.signal });
}

function draw<A, E, F>($: EngineInterface, instance: Instance<A, E, F>, band: NonNullable<Mod<A, E, F>['band']>, e: Frozen<RenderInput<'AbovePrompt'>>, next: (e: Frozen<RenderInput<'AbovePrompt'>>) => Promise<RenderElement>) {
    const rows = instance.rows.pipe(Ref.getUnsafe, Record.values, Array.flatMap(Record.toEntries));
    const { Box, Button } = $.ui.resolve(e);
    return e.props.hasSurvey || !Array.isReadonlyArrayNonEmpty(rows) ? next(e) : Box({ flexDirection: 'column', children: rows.map(([key, label]) => Button({ key, label, onPress: () => start($, instance).runPromise(Effect.catchCause(band.press(key), (cause) => Effect.logWarning(`${label} press failed`, cause))) })) });
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
    const instance: Instance<A, E, F> = { mod, live: MutableRef.make(Option.none()), turns: HashSet.empty<Option.Option<string>>().pipe(SubscriptionRef.make, Effect.runSync), rows: Ref.makeUnsafe({}) };
    const { hooks, band } = mod;
    on('session.start', ($, e, next) => {
        const hook = hooks['session.start'];
        return hook === undefined
            ? start($, instance)
                  .runPromise(Effect.void)
                  .then(() => next(e))
            : answer($, instance, hook, e, next);
    }).catch(recover);
    on('session.end', ($, e, next) => {
        const hook = hooks['session.end'];
        return (hook === undefined ? next(e) : answer($, instance, hook, e, next)).then((ended) => {
            Effect.runSync(SubscriptionRef.set(instance.turns, HashSet.empty()));
            return Promise.all(Option.toArray(Option.map(MutableRef.getAndSet(instance.live, Option.none()), (made) => made.dispose()))).then(() => ended);
        });
    }).catch(recover);
    on('turn.start', ($, e, next) => {
        const hook = hooks['turn.start'];
        track(instance, e.agentId, true);
        return hook === undefined ? next(e) : answer($, instance, hook, e, next);
    }).catch(recover);
    on('turn.complete', ($, e, next) => {
        const hook = hooks['turn.complete'];
        track(instance, e.agentId, false);
        return hook === undefined ? next(e) : answer($, instance, hook, e, next);
    }).catch(recover);
    const { 'session.compact': compact, 'agent.report': report, 'tool.describe': describe, 'tool.check': check, 'tool.call': toolCall } = hooks;
    if (compact !== undefined) {
        on('session.compact', ($, e, next) => answer($, instance, compact, e, next)).catch(recover);
    }
    if (report !== undefined) {
        on('agent.report', ($, e, next) => answer($, instance, report, e, next)).catch(recover);
    }
    if (describe !== undefined) {
        on('tool.describe', ($, e, next) => answer($, instance, describe, e, next)).catch(recover);
    }
    if (check !== undefined) {
        on('tool.check', ($, e, next) => answer($, instance, check, e, next)).catch(recover);
    }
    if (toolCall !== undefined) {
        on('tool.call', ($, e, next) => answer($, instance, toolCall, e, next)).catch(recover);
    }
    if (band !== undefined) {
        on('ui.render', { component: 'AbovePrompt' }, ($, e, next) => draw($, instance, band, e, next)).catch(recover);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { host, type Proceed };
