import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cause, Effect, Exit, Filter, Formatter, Function, HashMap, HashSet, Match, Option, Predicate, Record, Result, Schema, SchemaTransformation, Stream, Struct, Tuple } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';
import { Act } from '#/server/act';
import { Element } from '#/server/element';
import { ApplicationFailure } from '#/server/failure';

// --- [TYPES] ---------------------------------------------------------------------------

interface Tool extends Omit<NonNullable<(typeof Control.Type)['options']>[number], 'locator'> {
    readonly id: string;
}
interface Toolbar<E, R, EC, RC> {
    readonly current: Effect.Effect<Tool, E, R>;
    readonly listed?: Effect.Effect<readonly Tool[], E, R>;
    readonly select: (id: string) => Effect.Effect<unknown, E, R>;
    readonly changes: Stream.Stream<unknown, EC, RC>;
    readonly options?: {
        readonly describe: (at: typeof Locator.Type) => Effect.Effect<readonly (typeof Control.Type)[], E, R>;
        readonly operate: (tool: string, path: Array.NonEmptyReadonlyArray<string>, to: (typeof Operation.cases.press.Type)['to']) => Effect.Effect<unknown, E, R>;
    };
    readonly pointer?: Effect.Effect<Omit<typeof Pointer.Type, 'tool' | 'shortcut' | 'help'>, E, R>;
    readonly canvas?: {
        readonly view: Effect.Effect<{ readonly frame: typeof Element.Frame.Type; readonly transform: readonly [a: number, b: number, c: number, d: number, tx: number, ty: number] }, E, R>;
        readonly apply: (input: typeof Act.Input.Type) => Effect.Effect<Omit<typeof Act.Route.cases.source.Type, '_tag'>, E | Unmatched, R>;
    };
}
interface Declaration {
    readonly tools: HashMap.HashMap<string, Partial<Omit<Tool, 'id'>>>;
    readonly toolbar: readonly (readonly string[])[];
}
type Source<E = never, R = never, EP = never, RP = never> = {
    readonly [Request in RpcGroup.Rpcs<typeof Description.Rpcs> as Request['_tag']]: (payload: Rpc.Payload<Request>) => Effect.Effect<Rpc.Success<Request>, E, R>;
} & {
    readonly roots: Effect.Effect<readonly (typeof Locator.Type)[], E, R>;
    readonly pointer?: Stream.Stream<typeof Pointer.Type, EP, RP>;
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const TOOLBAR: typeof Locator.Type = [['toolbar' satisfies (typeof Container.Type)['kind'], 'tool' satisfies keyof typeof Pointer.Type]];
const CHOICE = Array.append(TOOLBAR, ['choice' satisfies (typeof Control.Type)['kind'], 'tool'] as const);
const CANVAS = Array.append(TOOLBAR, ['region' satisfies (typeof Control.Type)['kind'], 'canvas'] as const);

// --- [MODELS] --------------------------------------------------------------------------

const Locator = Schema.Struct({ key: Schema.Struct({ identifier: Schema.Struct({ value: Schema.String }) }), role: Schema.String }).pipe(
    Schema.decodeTo(Schema.Tuple([Schema.String, Schema.String]), SchemaTransformation.transform({ decode: ({ role, key }) => [role, key.identifier.value] as const, encode: ([role, value]) => ({ key: { identifier: { value } }, role }) })),
    Schema.NonEmptyArray,
    Schema.fromJsonString,
);
const Control = Element.cases.control.mapFields((fields) => ({
    ...Struct.omit(fields, ['_tag', 'ref', 'positional', 'alternate', 'disclosedBy', 'children']),
    locator: Locator,
    label: Schema.optionalKey(Schema.Struct({ text: Schema.String })),
    opens: Schema.optionalKey(Locator),
    options: Element.cases.option.mapFields((stated) => ({ ...Struct.omit(stated, ['_tag', 'ref', 'children']), locator: Locator })).pipe(Schema.Array, Schema.optionalKey),
}));
const Container = Element.cases.container.mapFields((fields) => ({ ...Struct.pick(fields, ['kind', 'title']), locator: Locator, parent: Schema.optionalKey(Locator), controls: Schema.Array(Control) }));
const Pointer = Element.Pointer.mapFields(Struct.omit(['cursor', 'draggingFrame']));
const Operation = Schema.TaggedUnion({ pointer: { locator: Locator, input: Act.Input }, press: { locator: Locator, to: Schema.optionalKey(Element.Value) } });
class Description extends Schema.Class<Description, { readonly brand: unique symbol }>('Description')(
    Schema.Struct({ containers: Schema.Array(Container), pointer: Schema.optionalKey(Pointer) }).check(
        Schema.makeFilter(
            ({ containers }) => {
                const held = locators(containers);
                return Array.dedupe(held).length === held.length;
            },
            { expected: 'a locator of its own for each container, control, and option' },
        ),
        Schema.makeFilter(({ containers }) => containers.every(({ parent }, at) => parent === undefined || !containers.slice(at).some(({ locator }) => same(locator, parent))), { expected: 'each container after the container its parent names' }),
    ),
) {
    // --- [CONTRACT]

    static readonly Locator = Locator;
    static readonly #describe = Rpc.make('describe', { payload: { within: Schema.optionalKey(Locator) }, success: Description, error: ApplicationFailure.Thrown });
    static readonly #operate = Rpc.make('operate', { payload: Operation, success: Schema.Union([Act.Route.cases.source, Schema.Void]), error: ApplicationFailure.Thrown });
    static readonly Rpcs = Object.assign(RpcGroup.make(Description.#describe, Description.#operate), {
        failing: <E extends Schema.Top>(error: E) => RpcGroup.make(Description.#describe.setError(error), Description.#operate.setError(error)),
    });

    // --- [SOURCES]

    static readonly tools = <E, R, EC, RC>({ current, listed, select, changes, options, pointer = Effect.succeed({}), canvas }: Toolbar<E, R, EC, RC>) => {
        const state = Effect.map(Effect.all([current, pointer]), ([tool, held]) => ({ tool, pointer: { ...held, ...Struct.pick(tool, ['shortcut', 'help']), tool: tool.label } satisfies typeof Pointer.Type }));
        const description = Effect.gen(function* () {
            const [now, tools, viewed] = yield* Effect.all([state, Effect.transposeOption(Option.fromUndefinedOr(listed)), Effect.transposeOption(Option.map(Option.fromUndefinedOr(canvas), ({ view }) => Effect.option(Effect.tapCause(view, (cause) => Effect.logDebug('Canvas unread', cause)))))]);
            const { tool } = now;
            const settings = yield* options?.describe(toolAt(tool.id)) ?? Effect.succeed([]);
            const choice = Control.make({
                locator: CHOICE,
                kind: 'choice',
                role: 'tool',
                label: { text: 'tool' },
                value: { _tag: 'text', value: Option.isSome(tools) ? tool.label : tool.id },
                selected: 'attribute',
                ...Option.getOrUndefined(Option.map(tools, (each) => ({ options: Array.map(each, ({ id, ...facts }) => ({ ...facts, ...(id === tool.id ? Struct.omit(tool, ['id']) : {}), locator: toolAt(id) })) }))),
            });
            const region = Option.map(Option.flatten(viewed), ({ frame }) => Control.make({ locator: CANVAS, kind: 'region', role: 'canvas', label: { text: 'canvas' }, frame }));
            return Description.make({ containers: [{ locator: TOOLBAR, kind: 'toolbar', controls: [choice, ...settings, ...Option.toArray(region)] }], pointer: now.pointer });
        });
        const take = (id: string) => Effect.flatMap(Effect.andThen(select(id), current), (taken) => (taken.id === id ? Effect.void : Unmatched.of({ subject: 'tool the host took', query: Option.some(id), candidates: [taken.id, taken.label] })));
        const resolve = (to: string) =>
            listed === undefined
                ? Effect.succeed(to)
                : listed.pipe(
                      Effect.flatMap(Unmatched.single('tool', Option.some(to), ({ id, label }: Tool) => [label, id])),
                      Effect.map(({ id }) => id),
                  );
        const configure = (key: string, path: Array.NonEmptyReadonlyArray<string>, to: (typeof Operation.cases.press.Type)['to']) =>
            options === undefined
                ? Unmatched.of({ subject: 'option of a tool', query: Option.some(key) })
                : current.pipe(
                      Effect.filterOrFail(
                          ({ id }) => id === key,
                          ({ id }) => Unmatched.of({ subject: 'option of the current tool', query: Option.some(key), candidates: [id] }),
                      ),
                      Effect.andThen(options.operate(key, path, to)),
                  );
        const draw = Effect.fnUntraced(function* ({ locator, input }: typeof Operation.cases.pointer.Type) {
            const { view, apply } = yield* Effect.fromOption(
                Option.filter(Option.fromUndefinedOr(canvas), () => same(locator, CANVAS)),
                () => Unmatched.of({ subject: 'canvas of the host source', query: Option.some(Formatter.format(locator)), candidates: canvas === undefined ? [] : [Formatter.format(CANVAS)] }),
            );
            yield* (input.button ?? 'left') === 'left' && input.by === undefined && input.edge === undefined && (input.count ?? 1) === 1
                ? Effect.void
                : new Cause.IllegalArgumentError(`a host source draws one whole left-button press or drag on its canvas, and the step is ${Formatter.format(Struct.pick(input, ['button', 'by', 'edge', 'count']))}`);
            const held = yield* view;
            const [origin] = held.frame;
            const [left, top] = origin;
            const [a, b, c, d, tx, ty] = held.transform;
            const place = ([x, y, ...rest]: (typeof Act.Input.Type)['pointer']): (typeof Act.Input.Type)['pointer'] => [a * (x - left) + c * (y - top) + tx, b * (x - left) + d * (y - top) + ty, ...rest];
            return Act.Route.cases.source.make(yield* apply({ ...input, pointer: place(input.pointer), ...(input.path === undefined ? {} : { path: Array.map(input.path, place) }) }));
        });
        const press = Effect.fnUntraced(function* ({ locator, to }: typeof Operation.cases.press.Type) {
            const [role, key] = yield* Effect.fromOption(
                Option.filter(Array.get(locator, 1), () => same([Array.headNonEmpty(locator)], TOOLBAR)),
                () => Unmatched.of({ subject: 'control of the toolbar', query: Option.some(Formatter.format(locator)) }),
            );
            const below = Array.map(Array.drop(locator, 2), Tuple.get(1));
            yield* Match.value({ role, path: Array.isReadonlyArrayNonEmpty(below) ? below : undefined, to }).pipe(
                Match.withReturnType<Effect.Effect<unknown, E | Unmatched | Cause.IllegalArgumentError, R>>(),
                Match.when({ role: 'choice', path: Match.undefined, to: Match.string }, ({ to: named }) => Effect.flatMap(resolve(named), take)),
                Match.when({ role: 'tool', path: Match.undefined, to: Match.undefined }, () => take(key)),
                Match.when({ role: 'tool', path: Match.defined }, ({ path }) => configure(key, path, to)),
                Match.orElse(() => new Cause.IllegalArgumentError(`the tool choice takes the name of a tool, a tool takes no value, and a tool option takes the value it holds, received ${typeof to} at ${role} ${key}`)),
            );
        });
        const reading = Effect.result(Effect.map(state, ({ pointer: held }) => held));
        return {
            roots: Effect.succeed([TOOLBAR]),
            describe: ({ within }: { readonly within?: typeof Locator.Type }) => (within === undefined || same(within, TOOLBAR) ? description : Unmatched.of({ subject: 'container of the toolbar', query: Option.some(Formatter.format(within)), candidates: [Formatter.format(TOOLBAR)] })),
            operate: Operation.match({ pointer: draw, press }),
            pointer: Stream.unwrap(
                Effect.map(reading, (seed) =>
                    changes.pipe(
                        Stream.mapEffect(() => reading),
                        Stream.prepend([seed]),
                        Stream.changesWith((last, next) => Result.isFailure(last) && Result.isFailure(next)),
                        Stream.tap((read) => Result.match(read, { onSuccess: () => Effect.void, onFailure: (error) => Effect.logWarning('Pointer unread', Cause.fail(error)) })),
                        Stream.filterMap(Function.identity),
                        Stream.changesWith(samePointer),
                        Stream.drop(Result.isSuccess(seed) ? 1 : 0),
                    ),
                ),
            ),
        };
    };
    static readonly merge: {
        <E2 = never, R2 = never, EP2 = never, RP2 = never>(that: Source<E2, R2, EP2, RP2>): <E1 = never, R1 = never, EP1 = never, RP1 = never>(self: Source<E1, R1, EP1, RP1>) => Required<Source<E1 | E2 | Unmatched, R1 | R2, never, RP1 | RP2>>;
        <E1 = never, R1 = never, EP1 = never, RP1 = never, E2 = never, R2 = never, EP2 = never, RP2 = never>(self: Source<E1, R1, EP1, RP1>, that: Source<E2, R2, EP2, RP2>): Required<Source<E1 | E2 | Unmatched, R1 | R2, never, RP1 | RP2>>;
    } = Function.dual(2, <E1, R1, EP1, RP1, E2, R2, EP2, RP2>(self: Source<E1, R1, EP1, RP1>, that: Source<E2, R2, EP2, RP2>): Required<Source<E1 | E2 | Unmatched, R1 | R2, never, RP1 | RP2>> => {
        const sources: readonly Source<E1 | E2, R1 | R2, EP1 | EP2, RP1 | RP2>[] = [self, that];
        const owner = Effect.fnUntraced(function* (locator: typeof Locator.Type) {
            const root: typeof Locator.Type = [Array.headNonEmpty(locator)];
            const held = yield* Effect.forEach(sources, ({ roots }) => roots);
            const candidates = Array.map(Array.flatten(held), (each) => Formatter.format(each));
            const [source] = yield* Effect.fromOption(
                Array.findFirst(Array.zip(sources, held), ([, roots]) => Array.containsWith(same)(roots, root)),
                () => Unmatched.of({ subject: 'root of a host source', query: Option.some(Formatter.format(root)), candidates }),
            );
            return source;
        });
        return {
            roots: Effect.forEach(sources, ({ roots }) => roots).pipe(Effect.map(Array.flatten)),
            describe: ({ within }) =>
                within === undefined
                    ? Effect.forEach(sources, (source) => Effect.exit(source.describe({}))).pipe(
                          Effect.flatMap((exits) => Effect.as(Exit.asVoidAll(exits), Array.filterMap(exits, Exit.filterValue))),
                          Effect.map((described) => {
                              const pointers = Array.flatMap(described, ({ pointer }) => Array.fromNullishOr(pointer));
                              return Description.make({ containers: Array.flatMap(described, Struct.get('containers')), ...(Array.isReadonlyArrayNonEmpty(pointers) ? { pointer: pointers.reduce((under, over) => overlay(under, over)) } : {}) });
                          }),
                      )
                    : Effect.flatMap(owner(within), (source) => source.describe({ within })),
            operate: (payload) => Effect.flatMap(owner(payload.locator), (source) => source.operate(payload)),
            pointer: Stream.mergeAll(
                Array.flatMap(sources, ({ pointer }) => (pointer === undefined ? [] : [Stream.ignoreCause(pointer, { log: 'Error' })])),
                { concurrency: 'unbounded' },
            ),
        };
    });

    // --- [FOLDS]

    static readonly fold = (described: Description, { tools, toolbar }: Declaration): Description => {
        const complete = (id: string, stated: Partial<Omit<Tool, 'id'>>) => {
            const declared = Option.getOrElse(HashMap.get(tools, id), (): Partial<Omit<Tool, 'id'>> => ({}));
            return { ...declared, ...stated, label: stated.label === undefined || stated.label === id ? (declared.label ?? id) : stated.label };
        };
        return Option.match(
            Array.findFirst(Array.flatMap(described.containers, Struct.get('controls')), ({ locator }) => same(locator, CHOICE)),
            {
                onNone: () => described,
                onSome: ({ options = [], value }) => {
                    const idOf = ({ locator }: (typeof options)[number]) => Array.lastNonEmpty(locator)[1];
                    const byId = Record.fromIterableWith(options, (option) => [idOf(option), option] as const);
                    const text = Option.fromUndefinedOr(value).pipe(Option.filterMap(Filter.tagged<NonNullable<(typeof Control.Type)['value']>>()('text')), Option.map(Struct.get('value')));
                    const current = Option.flatMap(text, (label) => Array.findFirst(options, (option) => option.label === label));
                    const declared = Array.map(Array.dedupe([...Array.map(options, idOf), ...Array.flatten(toolbar)]), (id) => ({ ...complete(id, byId[id] ?? {}), locator: toolAt(id) }));
                    const read: typeof Pointer.Type = described.pointer ?? {};
                    const pointer = Option.match(Option.orElse(Option.map(current, idOf), Function.constant(text)), {
                        onNone: () => described.pointer,
                        onSome: (id) => ({ ...read, ...Struct.renameKeys(complete(id, Struct.renameKeys(Struct.pick(read, ['tool', 'shortcut', 'help']), { tool: 'label' })), { label: 'tool' }) }),
                    });
                    const choice = {
                        ...Option.getOrUndefined(Option.map(current, (option) => ({ value: { _tag: 'text' as const, value: complete(idOf(option), option).label } }))),
                        ...(Array.isReadonlyArrayNonEmpty(declared) ? { options: declared } : {}),
                    };
                    return Description.make({
                        containers: Array.map(described.containers, ({ controls, ...container }) => ({ ...container, controls: Array.map(controls, (control) => (same(control.locator, CHOICE) ? { ...control, ...choice } : control)) })),
                        ...(pointer === undefined ? {} : { pointer }),
                    });
                },
            },
        );
    };
    static readonly compose = Effect.fnUntraced(function* <E, R>(model: typeof Element.Model.Type, description: Description, intern: (locators: readonly (typeof Element.Locator.Type)[]) => Effect.Effect<readonly (typeof Element.Ref.Type)[], E, R>) {
        const paths = Array.dedupe([...locators(description.containers), ...Array.flatMap(description.containers, ({ parent, controls }) => [...Array.fromNullishOr(parent), ...Array.flatMap(controls, ({ opens }) => Array.fromNullishOr(opens))])]);
        const texts = yield* Effect.forEach(paths, (path) => Effect.orDie(Schema.encodeEffect(Locator)(path)));
        const refs = yield* intern(texts);
        const interned = HashMap.fromIterable(
            Array.zip(
                paths,
                Array.zipWith(texts, refs, (locator, ref) => ({ locator, ref })),
            ),
        );
        const at = (path: typeof Locator.Type) => Effect.fromOption(HashMap.get(interned, path), () => new Cause.NoSuchElementError(`Application server interned no reference for locator ${Formatter.format(path)}`));
        const containers = (elements: (typeof Element.Model.Type)['elements']): readonly (typeof Element.cases.container.Type)[] => Array.filter(elements, Element.isAnyOf(['container'])).flatMap((container) => [container, ...containers(container.children)]);
        const paired = Record.intersection(singles(Array.filter(description.containers, ({ parent }) => parent === undefined)), singles(containers(model.elements)), ([stated], [held]) => [stated.locator, held] as const);
        const replaced = HashMap.fromIterable(Record.values(paired));
        const levels = HashSet.fromIterable(Array.map(description.containers, Struct.get('locator')));
        const placeControl = Effect.fnUntraced(function* ({ locator, opens, options = [], ...stated }: typeof Control.Type) {
            return {
                ...stated,
                ...(yield* at(locator)),
                _tag: 'control' as const,
                positional: false,
                ...(opens === undefined ? {} : { opens: (yield* at(opens)).ref }),
                children: yield* Effect.forEach(options, ({ locator: option, ...facts }) => Effect.map(at(option), Function.flow(Struct.pick(['ref']), Struct.assign({ ...facts, _tag: 'option' as const, children: [] })))),
            };
        });
        const placeContainer = Effect.fnUntraced(function* ({ locator, parent, controls, ...stated }: typeof Container.Type) {
            const swap = HashMap.get(replaced, locator);
            const under = Option.filter(Option.fromUndefinedOr(parent), (path) => HashSet.has(levels, path));
            const nest = yield* Effect.transposeOption(Option.map(under, at));
            const node = {
                ...(yield* at(locator)),
                ...stated,
                _tag: 'container' as const,
                closed: false,
                modal: false,
                asking: false,
                ...Option.getOrUndefined(Option.map(swap, (held) => Struct.pick(held, ['window', 'frame', 'layer', 'focused', 'modal', 'asking']))),
                text: [],
                truncated: false,
                unreadable: false,
                children: [...(yield* Effect.forEach(controls, placeControl)), ...Array.flatMap(Option.toArray(swap), ({ children }) => Array.filter(children, Element.isAnyOf(['container'])))],
            };
            return { node, swap, nest: Option.map(nest, ({ ref }) => `${ref}`) };
        });
        const placed = yield* Effect.forEach(description.containers, placeContainer);
        const swaps = new Map(Array.flatMap(placed, ({ node, swap }) => Option.toArray(Option.map(swap, (held) => [held, node] as const))));
        const nests = Array.groupBy(
            Array.flatMap(placed, ({ node, nest }) => Option.toArray(Option.map(nest, (under) => ({ under, node })))),
            ({ under }) => under,
        );
        const graft = (elements: (typeof Element.Model.Type)['elements']): (typeof Element.Model.Type)['elements'] =>
            Array.map(
                elements,
                Element.matchOrElse(
                    {
                        container: (held) => {
                            const now = swaps.get(held) ?? held;
                            return { ...now, children: graft([...now.children, ...(nests[`${now.ref}`] ?? []).map(({ node }) => node)]) };
                        },
                    },
                    (other) => other,
                ),
            );
        return {
            model: {
                ...model,
                elements: graft([...model.elements, ...Array.flatMap(placed, ({ node, swap, nest }) => (Option.isNone(swap) && Option.isNone(nest) ? [node] : []))]),
                ...(description.pointer === undefined ? {} : { pointer: overlay<NonNullable<(typeof Element.Model.Type)['pointer']>>(model.pointer ?? description.pointer, description.pointer) }),
            } satisfies typeof Element.Model.Type,
            routes: Record.fromIterableWith(Array.zip(refs, paths), ([ref, path]) => [`${ref}`, path] as const),
        };
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const same = Schema.toEquivalence(Locator);
const samePointer = Schema.toEquivalence(Pointer);
const overlay = <A extends typeof Pointer.Type>(under: A, over: typeof Pointer.Type) => ({ ...(over.tool === undefined ? under : Struct.omit(under, ['tool', 'shortcut', 'help'])), ...over });
const toolAt = (id: string) => Array.append(TOOLBAR, ['tool', id] as const);
const singles = <A extends { readonly kind: string }>(held: readonly A[]) =>
    Record.filter(
        Array.groupBy(held, ({ kind }) => kind),
        Predicate.isTupleOf(1),
    );
const locators = (containers: readonly (typeof Container.Type)[]): readonly (typeof Locator.Type)[] => Array.flatMap(containers, ({ locator, controls }) => [locator, ...Array.flatMap(controls, ({ locator: control, options = [] }) => [control, ...Array.map(options, Struct.get('locator'))])]);

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Declaration, Description, type Source };
