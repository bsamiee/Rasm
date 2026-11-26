import type { Link } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Effect, Exit, Formatter, Function, HashMap, Match, Option, Predicate, Record, Result, Schema, SchemaTransformation, Stream, Struct, Tuple } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';
import { Container, Control, Failure, type Model, type Observation, Pointer, Value } from '#/contract';

// --- [TYPES] ---------------------------------------------------------------------------

interface Tool {
    readonly id: string;
    readonly label: string;
    readonly help?: string;
}
interface Facet<E, R> {
    readonly roots: Effect.Effect<readonly FacetControl['locator'][], E, R>;
    readonly describe: (payload: { readonly within?: FacetControl['locator'] }) => Effect.Effect<Description, E, R>;
    readonly operate: (payload: { readonly locator: FacetControl['locator']; readonly to?: Observation['values'][string] }) => Effect.Effect<void, E, R>;
}
interface Changes<E, R> {
    readonly changes: Stream.Stream<typeof Link.Event.Type, E, R>;
}
type Source<E, R, EC, RC> = Facet<E, R> & Changes<EC, RC>;
type Part<E, R, EC, RC> = Facet<E, R> | Changes<EC, RC>;
interface Toolbar<E, R, EC, RC> {
    readonly current: Effect.Effect<Tool, E, R>;
    readonly listed?: Effect.Effect<readonly Tool[], E, R>;
    readonly select: (id: string) => Effect.Effect<unknown, E, R>;
    readonly changes: Stream.Stream<unknown, EC, RC>;
    readonly options?: {
        readonly describe: (at: FacetControl['locator']) => Effect.Effect<readonly FacetControl[], E, R>;
        readonly operate: (tool: string, path: Array.NonEmptyReadonlyArray<string>, to: Observation['values'][string] | undefined) => Effect.Effect<unknown, E, R>;
    };
    readonly pointer?: Effect.Effect<Omit<FacetPointer, 'tool' | 'help'>, E, R>;
}

// --- [MODELS] --------------------------------------------------------------------------

const Locator = Schema.fromJsonString(
    Schema.NonEmptyArray(
        Schema.Struct({ key: Schema.Struct({ identifier: Schema.Struct({ value: Schema.String }) }), role: Schema.String }).pipe(
            Schema.decodeTo(Schema.Tuple([Schema.String, Schema.String]), SchemaTransformation.transform({ decode: ({ role, key }) => [role, key.identifier.value] as const, encode: ([role, value]) => ({ key: { identifier: { value } }, role }) })),
        ),
    ),
).annotate({ description: 'Locator of a facet container, control, or option: the role and identifier of each step from the application down, encoded as the canonical locator text' });
class FacetControl extends Schema.Opaque<FacetControl, unknown>()(
    Control.mapFields((fields) => ({
        ...Struct.omit(fields, ['ref', 'positional', 'opens', 'alternate']),
        locator: Locator,
        opens: Schema.optionalKey(Locator.annotate({ description: 'Facet container the control opens' })),
        options: Schema.optionalKey(Schema.Array(Schema.Struct({ label: Schema.String, locator: Locator })).annotate({ description: 'Options of a choice, each addressed by its own locator' })),
    })),
) {}
class FacetContainer extends Schema.Opaque<FacetContainer, unknown>()(
    Container.mapFields((fields) => ({
        ...Struct.pick(fields, ['kind', 'title']),
        locator: Locator,
        parent: Schema.optionalKey(Locator.annotate({ description: 'Facet container this one sits in, a container with a parent never folding an Accessibility container' })),
        controls: Schema.Array(FacetControl),
    })),
) {}
class FacetPointer extends Schema.Opaque<FacetPointer, unknown>()(Pointer.mapFields((fields) => Struct.pick(fields, ['tool', 'help', 'holding', 'operations']))) {}

class Description extends Schema.Class<Description, { readonly brand: unique symbol }>('Description')(
    Schema.Struct({ containers: Schema.Array(FacetContainer), pointer: Schema.optionalKey(FacetPointer) }).check(
        Schema.makeFilter(({ containers }) => {
            const held = located(containers);
            return Array.dedupe(held).length === held.length || 'each container, control, and option a host source describes takes its own locator';
        }),
    ),
    { description: 'Containers and pointer a host source states for the part of an application interface it owns, each top-level container replacing the one Accessibility container of its kind' },
) {
    static readonly Control = FacetControl;
    static readonly Container = FacetContainer;
    static readonly Locator = Locator;
    static readonly tools = <E, R, EC, RC>({ current, listed, select, changes, options, pointer = Effect.succeed({}) }: Toolbar<E, R, EC, RC>) => {
        const toolbar: FacetControl['locator'] = [['toolbar' satisfies FacetContainer['kind'], 'tool' satisfies keyof FacetPointer]];
        const choice = Array.append(toolbar, ['choice' satisfies FacetControl['kind'], 'tool'] as const);
        const toolAt = (id: string) => Array.append(toolbar, ['tool', id] as const);
        const state = Effect.map(Effect.all([current, pointer]), ([tool, held]) => ({ tool, pointer: { ...held, ...Struct.pick(tool, ['help']), tool: tool.label } satisfies FacetPointer }));
        const stated = Effect.gen(function* () {
            const [{ tool, pointer: held }, tools] = yield* Effect.all([state, Effect.transposeOption(Option.fromUndefinedOr(listed))]);
            const settings = yield* Option.match(Option.fromUndefinedOr(options), { onNone: () => Effect.succeed([]), onSome: ({ describe }) => describe(toolAt(tool.id)) });
            const chosen = Description.Control.make({
                locator: choice,
                kind: 'choice',
                role: 'tool',
                label: { text: 'tool', attribute: 'tool' },
                value: { text: { value: tool.label } },
                selected: 'attribute',
                ...Option.getOrUndefined(Option.map(tools, (each) => ({ options: Array.map(each, ({ id, label }) => ({ label, locator: toolAt(id) })) }))),
            });
            return Description.make({ containers: [{ locator: toolbar, kind: 'toolbar', controls: [chosen, ...settings] }], pointer: held });
        });
        const take = Effect.fnUntraced(function* (id: string) {
            yield* select(id);
            const taken = yield* current;
            return yield* taken.id === id ? Effect.void : Unmatched.of({ subject: 'tool the host took', query: Option.some(id), candidates: [taken.id, taken.label] });
        });
        const resolve = (to: string) =>
            Option.match(Option.fromUndefinedOr(listed), {
                onNone: () => Effect.succeed(to),
                onSome: (tools) =>
                    Effect.map(
                        Effect.flatMap(
                            tools,
                            Unmatched.single('tool', Option.some(to), ({ id, label }: Tool) => [label, id]),
                        ),
                        Struct.get('id'),
                    ),
            });
        const configure = (key: string, path: Array.NonEmptyReadonlyArray<string>, to: Observation['values'][string] | undefined) =>
            current.pipe(
                Effect.filterOrFail(
                    ({ id }) => id === key,
                    ({ id }) => Unmatched.of({ subject: 'option of the current tool', query: Option.some(key), candidates: [id] }),
                ),
                Effect.andThen(Effect.fromOption(Option.fromUndefinedOr(options), () => Unmatched.of({ subject: 'option of a tool', query: Option.some(key) }))),
                Effect.flatMap((held) => held.operate(key, path, to)),
            );
        const route = ([role, key]: readonly [string, string], below: readonly string[], to: Observation['values'][string] | undefined) =>
            Match.value({ role, path: Array.isReadonlyArrayNonEmpty(below) ? below : undefined, to }).pipe(
                Match.withReturnType<Effect.Effect<unknown, E | Unmatched | Cause.IllegalArgumentError, R>>(),
                Match.when({ role: 'choice', path: Match.undefined, to: Match.string }, ({ to: named }) => Effect.flatMap(resolve(named), take)),
                Match.when({ role: 'tool', path: Match.undefined, to: Match.undefined }, () => take(key)),
                Match.when({ role: 'tool', path: Match.defined }, ({ path }) => configure(key, path, to)),
                Match.orElse(() => Effect.fail(new Cause.IllegalArgumentError(`the tool choice takes the name of a tool, a tool takes no value, and a tool option takes the value it holds, received ${typeof to} at ${role} ${key}`))),
            );
        return {
            roots: Effect.succeed([toolbar]),
            describe: ({ within }: { readonly within?: FacetControl['locator'] }) =>
                Option.match(
                    Option.filter(Option.fromUndefinedOr(within), (at) => !same(at, toolbar)),
                    { onNone: () => stated, onSome: (at) => Effect.fail(Unmatched.of({ subject: 'facet item', query: Option.some(Formatter.format(at)) })) },
                ),
            operate: Effect.fnUntraced(function* ({ locator, to }: { readonly locator: FacetControl['locator']; readonly to?: Observation['values'][string] }): Effect.fn.Return<void, E | Unmatched | Cause.IllegalArgumentError, R> {
                const [root, ...steps] = locator;
                yield* Option.match(
                    Option.filter(Array.head(steps), () => same([root], toolbar)),
                    { onNone: () => Effect.fail(Unmatched.of({ subject: 'facet control', query: Option.some(Formatter.format(locator)) })), onSome: (step) => route(step, Array.map(Array.drop(steps, 1), Tuple.get(1)), to) },
                );
            }),
            changes: changes.pipe(
                Stream.prepend([undefined]),
                Stream.mapEffect(() => Effect.result(Effect.map(state, Struct.get('pointer')))),
                Stream.changesWith(Result.makeEquivalence(Schema.toEquivalence(FacetPointer), Function.constTrue)),
                Stream.drop(1),
                Stream.filterMapEffect(
                    Result.match({
                        onFailure: (error) => Effect.as(Effect.logWarning('Pointer unread', Cause.fail(error)), Result.failVoid),
                        onSuccess: (read) => Effect.map(Effect.orDie(Schema.encodeEffect(Schema.toCodecJson(FacetPointer))(read)), (detail) => Result.succeed({ name: 'pointer' satisfies keyof Model, target: Option.none<string>(), detail: Option.some(detail) })),
                    }),
                ),
            ),
        };
    };
    static readonly merge: {
        <E2 = never, R2 = never, EC2 = never, RC2 = never>(that: Part<E2, R2, EC2, RC2>): <E1 = never, R1 = never, EC1 = never, RC1 = never>(self: Part<E1, R1, EC1, RC1>) => Source<E1 | E2 | Unmatched, R1 | R2, never, RC1 | RC2>;
        <E1 = never, R1 = never, EC1 = never, RC1 = never, E2 = never, R2 = never, EC2 = never, RC2 = never>(self: Part<E1, R1, EC1, RC1>, that: Part<E2, R2, EC2, RC2>): Source<E1 | E2 | Unmatched, R1 | R2, never, RC1 | RC2>;
    } = Function.dual(2, <E1, R1, EC1, RC1, E2, R2, EC2, RC2>(self: Part<E1, R1, EC1, RC1>, that: Part<E2, R2, EC2, RC2>): Source<E1 | E2 | Unmatched, R1 | R2, never, RC1 | RC2> => {
        const facets: readonly Facet<E1 | E2, R1 | R2>[] = Array.flatMap([self, that], (part) => ('roots' in part ? [part] : []));
        const owner = (locator: FacetControl['locator']) => {
            const root: FacetControl['locator'] = [Array.headNonEmpty(locator)];
            return Effect.forEach(facets, (facet) => Effect.map(facet.roots, (roots) => ({ facet, roots }))).pipe(
                Effect.flatMap((owned) => Effect.fromOption(Array.findFirst(owned, ({ roots }) => Array.containsWith(same)(roots, root)), () => Unmatched.of({ subject: 'facet root', query: Option.some(Formatter.format(root)), candidates: Array.flatMap(owned, ({ roots }) => Array.map(roots, (each) => Formatter.format(each))) }))),
                Effect.map(Struct.get('facet')),
            );
        };
        return {
            roots: Effect.map(
                Effect.forEach(facets, Struct.get('roots')),
                Array.flatten,
            ),
            describe: ({ within }) =>
                Option.match(Option.fromUndefinedOr(within), {
                    onSome: (at) => Effect.flatMap(owner(at), (facet) => facet.describe({ within: at })),
                    onNone: () =>
                        Effect.forEach(facets, (facet) => Effect.exit(facet.describe({}))).pipe(
                            Effect.flatMap((exits) => Effect.as(Exit.asVoidAll(exits), Array.getSomes(Array.map(exits, Exit.getSuccess)))),
                            Effect.map((described) =>
                                Description.make({
                                    containers: Array.flatMap(described, Struct.get('containers')),
                                    ...Array.reduce(described, {}, (held: Pick<Description, 'pointer'>, { pointer }) => (pointer === undefined ? held : { pointer: { ...held.pointer, ...pointer } })),
                                }),
                            ),
                        ),
                }),
            operate: (payload) => Effect.flatMap(owner(payload.locator), (facet) => facet.operate(payload)),
            changes: Stream.mergeAll(
                Array.flatMap([self, that], (part) => ('changes' in part ? [Stream.ignoreCause(part.changes, { log: 'Error' })] : [])),
                { concurrency: 'unbounded' },
            ),
        };
    });
    static readonly compose = Effect.fnUntraced(function* <E, R>(model: Model, description: Description, intern: (locators: readonly string[]) => Effect.Effect<readonly Control['ref'][], E, R>) {
        const paths = Array.dedupe([
            ...located(description.containers),
            ...Array.filterMap(description.containers, ({ parent }) => Result.fromNullishOr(parent, Function.constVoid)),
            ...Array.flatMap(description.containers, ({ controls }) => Array.filterMap(controls, ({ opens }) => Result.fromNullishOr(opens, Function.constVoid))),
        ]);
        const texts = yield* Effect.forEach(paths, (path) => Effect.orDie(Schema.encodeEffect(Locator)(path)));
        const interned = yield* intern(texts);
        const placed = HashMap.fromIterable(
            Array.zip(
                paths,
                Array.zipWith(texts, interned, (locator, ref) => ({ locator, ref })),
            ),
        );
        const at = (path: FacetControl['locator']) => Effect.fromOption(HashMap.get(placed, path), () => new Cause.NoSuchElementError(`Application server interned no reference for facet locator ${Formatter.format(path)}`));
        const byKind = Array.groupBy(model.containers, Struct.get('kind'));
        const placeControl = Effect.fnUntraced(function* ({ locator, options, opens, ...control }: FacetControl) {
            return {
                ...control,
                ...(yield* at(locator)),
                positional: false,
                ...(opens === undefined ? {} : { opens: (yield* at(opens)).ref }),
                ...(options === undefined ? {} : { options: yield* Effect.forEach(options, ({ label, locator: option }) => Effect.map(at(option), Function.flow(Struct.pick(['ref']), Struct.assign({ label })))) }),
            };
        });
        const placeContainer = Effect.fnUntraced(function* ({ locator, parent, controls, ...container }: FacetContainer) {
            const folded = Option.filter(Option.fromUndefinedOr(byKind[container.kind]), (held) => parent === undefined && Predicate.isTupleOf(held, 1)).pipe(Option.map(Array.headNonEmpty));
            return {
                folded,
                container: {
                    ...(yield* at(locator)),
                    ...container,
                    modal: false,
                    asking: false,
                    ...Option.getOrUndefined(Option.map(folded, (held) => Struct.pick(held, ['window', 'frame', 'layer', 'focused', 'modal', 'asking']))),
                    ...(parent === undefined ? {} : { parent: (yield* at(parent)).ref }),
                    text: [],
                    truncated: false,
                    controls: yield* Effect.forEach(controls, placeControl),
                },
            };
        });
        const composed = yield* Effect.forEach(description.containers, placeContainer);
        const replaced = new Set(Array.flatMap(composed, ({ folded }) => Option.toArray(folded)));
        return {
            model: {
                ...model,
                containers: [...model.containers.filter((read) => !replaced.has(read)), ...Array.map(composed, Struct.get('container'))],
                ...Option.getOrUndefined(Option.map(Option.fromUndefinedOr(description.pointer ?? model.pointer), () => ({ pointer: { ...model.pointer, ...description.pointer } }))),
            } satisfies Model,
            routes: Record.fromIterableWith(Array.zip(interned, paths), ([ref, path]) => [`${ref}`, path] as const),
        };
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const same = Schema.toEquivalence(Locator);
const located = (containers: readonly FacetContainer[]): readonly FacetControl['locator'][] => Array.flatMap(containers, ({ locator, controls }) => [locator, ...Array.flatMap(controls, ({ locator: control, options = [] }) => [control, ...Array.map(options, Struct.get('locator'))])]);

// --- [COMPOSITION] ---------------------------------------------------------------------

const SourceRpcs = RpcGroup.make(
    Rpc.make('describe', {
        payload: { within: Schema.optionalKey(Locator.annotate({ description: 'Facet item whose level is described alone, every facet at its top level when absent' })) },
        success: Description,
        error: Failure,
    }),
    Rpc.make('operate', {
        payload: { locator: Locator, to: Schema.optionalKey(Value.annotate({ description: 'Value the control takes, absent to press, pick, or select it' })) },
        success: Schema.Void.annotate({ description: 'The host took the operation, which a later describe reads back, as a host can apply it after the call returns' }),
        error: Failure,
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Description, type Source, SourceRpcs };
