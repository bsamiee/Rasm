import type { ElementChildren, ElementName, Elements, EngineInterface, RenderChildren, RenderElement, RenderSurface, ResolveInput, UiInputArgument, UiPressArgument, UiSelectArgument } from 'claude-code';
import { Array, Effect, type ManagedRuntime, Match, Option, Predicate, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Props<N extends ElementName> = Parameters<{ [S in RenderSurface]: N extends keyof Elements[S] ? Elements[S][N] : never }[RenderSurface]>[0];
type Child<R, F> = Element<R, F> | Exclude<RenderChildren, readonly RenderChildren[]>;
type Children<R, F> = Child<R, F> | readonly Child<R, F>[];
type Element<R = never, F = never> = {
    readonly [N in ElementName]: { readonly _tag: N } & {
        readonly [K in keyof Props<N>]: K extends keyof ElementChildren ? Children<R, F> : NonNullable<Props<N>[K]> extends (...args: infer A) => unknown ? (...args: A) => Effect.Effect<unknown, F, R> : Props<N>[K];
    };
}[ElementName];
type Table = Partial<Record<ElementName, { make(props: Readonly<Record<string, unknown>>): RenderElement }['make']>>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const guarded = <R, F>(e: UiPressArgument | UiInputArgument | UiSelectArgument, act: string, effect: Effect.Effect<unknown, F, R>) => Effect.catchCause(effect, (cause) => Effect.logWarning(`${e.element} ${act} failed`, cause));
const lowered = <R, E, F>(table: Table, runtime: () => ManagedRuntime.ManagedRuntime<R, E>, children: Children<R, F>): RenderChildren => Array.ensure(children).map((child) => (Predicate.hasProperty(child, '_tag') ? lower(table, runtime, child) : child));
const lower = <R, E, F>(table: Table, runtime: () => ManagedRuntime.ManagedRuntime<R, E>, node: Element<R, F>): RenderElement =>
    Option.getOrElse(
        Option.fromUndefinedOr(table[node._tag]),
        (): NonNullable<Table[ElementName]> => Fragment,
    )({
        ...Struct.omit(node, ['_tag', 'children']),
        ...Option.match(Option.fromUndefinedOr(node.children), { onNone: () => ({}), onSome: (children) => ({ children: lowered(table, runtime, children) }) }),
        ...Match.value(node).pipe(
            Match.tags({
                Button: ({ onPress }) => ({ onPress: (e: UiPressArgument) => runtime().runPromise(guarded(e, 'press', onPress(e))) }),
                Markdown: ({ onLinkPress }) => Option.match(Option.fromUndefinedOr(onLinkPress), { onNone: () => ({}), onSome: (press) => ({ onLinkPress: (link: Parameters<typeof press>[0], e: UiPressArgument) => runtime().runPromise(guarded(e, 'link press', press(link, e))) }) }),
                Input: ({ onInput, onSubmit }) => ({
                    ...Option.match(Option.fromUndefinedOr(onInput), {
                        onNone: () => ({}),
                        onSome: (input) => ({
                            onInput: (value: string, e: UiInputArgument) => {
                                runtime().runFork(guarded(e, e.kind, input(value, e)));
                            },
                        }),
                    }),
                    onSubmit: (value: string, e: UiInputArgument) => {
                        runtime().runFork(guarded(e, e.kind, onSubmit(value, e)));
                    },
                }),
                Select: ({ onSelect }) => ({
                    onSelect: (value: string, e: UiSelectArgument) => {
                        runtime().runFork(guarded(e, 'pick', onSelect(value, e)));
                    },
                }),
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
