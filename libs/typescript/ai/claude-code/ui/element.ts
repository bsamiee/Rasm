import type { ElementChildren, ElementName, Elements, EngineInterface, PressedLink, RenderChildren, RenderElement, RenderSurface, ResolveInput, UiInputArgument, UiPressArgument, UiSelectArgument } from 'claude-code';
import { Array, Effect, Function, type ManagedRuntime, Match, Predicate, type Record, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Props<N extends ElementName> = Parameters<{ [S in RenderSurface]: N extends keyof Elements[S] ? Elements[S][N] : never }[RenderSurface]>[0];
type Child<R, F> = Element<R, F> | Exclude<RenderChildren, readonly RenderChildren[]>;
type Element<R = never, F = never> = {
    readonly [N in ElementName]: { readonly _tag: N } & {
        readonly [K in keyof Props<N>]: K extends keyof ElementChildren ? Child<R, F> | readonly Child<R, F>[] : NonNullable<Props<N>[K]> extends (...args: infer A) => unknown ? (...args: A) => Effect.Effect<unknown, F, R> : Props<N>[K];
    };
}[ElementName];
type Make = { make(props: object): RenderElement }['make'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = <R, E, F>(runtime: () => ManagedRuntime.ManagedRuntime<R, E>, e: UiPressArgument | UiInputArgument | UiSelectArgument, handled: Effect.Effect<unknown, F, R>) =>
    runtime().runPromise(Effect.catchCause(handled, (cause) => Effect.logWarning('handler failed', cause).pipe(Effect.annotateLogs(Struct.omit(e, ['element'])))));
const lower = <R, E, F>(table: Partial<Record.ReadonlyRecord<ElementName, Make>>, runtime: () => ManagedRuntime.ManagedRuntime<R, E>, node: Element<R, F>): RenderElement =>
    (table[node._tag] ?? Function.identity<Make>(Fragment))({
        ...Struct.omit(node, ['_tag', 'children']),
        ...(node.children === undefined ? {} : { children: Array.ensure(node.children).map((child) => (Predicate.hasProperty(child, '_tag') ? lower(table, runtime, child) : child)) }),
        ...Match.value(node).pipe(
            Match.tags({
                Button: ({ onPress }) => ({ onPress: (e: UiPressArgument) => run(runtime, e, onPress(e)) }),
                Markdown: ({ onLinkPress }) => (onLinkPress === undefined ? {} : { onLinkPress: (link: PressedLink, e: UiPressArgument) => run(runtime, e, onLinkPress(link, e)) }),
                Input: ({ onInput, onSubmit }) => ({ ...(onInput === undefined ? {} : { onInput: (value: string, e: UiInputArgument) => run(runtime, e, onInput(value, e)) }), onSubmit: (value: string, e: UiInputArgument) => run(runtime, e, onSubmit(value, e)) }),
                Select: ({ onSelect }) => ({ onSelect: (value: string, e: UiSelectArgument) => run(runtime, e, onSelect(value, e)) }),
            }),
            Match.orElse(() => ({})),
        ),
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

function draw<R, E, F>($: EngineInterface, e: ResolveInput, element: Element<R, F>, runtime: () => ManagedRuntime.ManagedRuntime<R, E>) {
    return lower($.ui.resolve(e), runtime, element);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { draw, type Element };
