import type { Link } from '@rasm/mcp/link';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cause, Effect, Exit, Filter, Formatter, Function, HashMap, Match, Option, Predicate, Record, Result, Schema, SchemaTransformation, Stream, Struct, Tuple } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';
import { Act, Container, Control, Failure, HostLabel, type Frame, type Model, type Observation, Pointer, Value } from '#/contract';

// --- [TYPES] ---------------------------------------------------------------------------

interface Polled {
    readonly known: Option.Option<FacetPointer>;
    readonly failing: boolean;
}
interface Tool extends Omit<NonNullable<FacetControl['options']>[number], 'locator'> {
    readonly id: string;
}
interface Facet<E, R> {
    readonly roots: Effect.Effect<readonly FacetControl['locator'][], E, R>;
    readonly describe: (payload: { readonly within?: FacetControl['locator'] }) => Effect.Effect<Description, E, R>;
    readonly operate: (payload: typeof Operation.Type) => Effect.Effect<void | typeof Act.Route.cases.source.Type, E, R>;
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
    readonly pointer?: Effect.Effect<Omit<FacetPointer, 'tool' | 'shortcut' | 'help'>, E, R>;
    readonly canvas?: {
        readonly view: Effect.Effect<View, E, R>;
        readonly apply: (input: typeof Act.Input.Type) => Effect.Effect<Omit<typeof Act.Route.cases.source.Type, '_tag'>, E | Unmatched, R>;
    };
}
interface View {
    readonly frame: typeof Frame.Type;
    readonly transform: readonly [a: number, b: number, c: number, d: number, tx: number, ty: number];
}
interface Declaration {
    readonly tools: HashMap.HashMap<string, Partial<Omit<Tool, 'id'>>>;
    readonly toolbar: readonly (readonly string[])[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TOOLBAR: FacetControl['locator'] = [['toolbar' satisfies FacetContainer['kind'], 'tool' satisfies keyof FacetPointer]];
const CHOICE = Array.append(TOOLBAR, ['choice' satisfies FacetControl['kind'], 'tool'] as const);
const CANVAS = Array.append(TOOLBAR, ['region' satisfies FacetControl['kind'], 'canvas'] as const);

// --- [MODELS] --------------------------------------------------------------------------

const Locator = Schema.fromJsonString(
    Schema.NonEmptyArray(
        Schema.Struct({ key: Schema.Struct({ identifier: Schema.Struct({ value: Schema.String }) }), role: Schema.String }).pipe(
            Schema.decodeTo(Schema.Tuple([Schema.String, Schema.String]), SchemaTransformation.transform({ decode: ({ role, key }) => [role, key.identifier.value] as const, encode: ([role, value]) => ({ key: { identifier: { value } }, role }) })),
        ),
    ),
).annotate({ description: 'Locator of a facet container, control, or option: the role and identifier of each step from the application down, encoded as the canonical locator text' });
class FacetControl extends Schema.Opaque<FacetControl>()(
    Control.mapFields((fields) => ({
        ...Struct.omit(fields, ['ref', 'positional', 'opens', 'alternate']),
        label: Schema.optionalKey(HostLabel),
        locator: Locator,
        opens: Schema.optionalKey(Locator.annotate({ description: 'Facet container the control opens' })),
        options: Schema.optionalKey(Schema.Array(fields.options.schema.value.mapFields((option) => ({ ...Struct.omit(option, ['ref']), locator: Locator }))).annotate({ description: 'Options of a choice, each addressed by its own locator' })),
    })),
) {}
class FacetContainer extends Schema.Opaque<FacetContainer>()(
    Container.mapFields((fields) => ({
        ...Struct.pick(fields, ['kind', 'title']),
        locator: Locator,
        parent: Schema.optionalKey(Locator.annotate({ description: 'Facet container this one sits in, a container with a parent never folding an Accessibility container' })),
        controls: Schema.Array(FacetControl),
    })),
) {}
class FacetPointer extends Schema.Opaque<FacetPointer>()(Pointer.mapFields((fields) => Struct.pick(fields, ['tool', 'shortcut', 'help', 'holding', 'operations']))) {}
const Operation = Object.assign(
    Schema.Union([
        Schema.Struct({ locator: Locator, input: Act.Input.annotate({ description: 'Pointer step in screen points the host draws on the canvas the locator names, with its current tool through its own member' }) }),
        Schema.Struct({ locator: Locator, to: Schema.optionalKey(Value.annotate({ description: 'Value the control takes, absent to press, pick, or select it' })) }),
    ]),
    {
        Result: Schema.Union([
            Schema.Void.annotate({ description: 'The host took the operation, which a later describe reads back, as a host can apply it after the call returns' }),
            Act.Route.cases.source.annotate({ description: 'The host drew the input on its canvas through the member named, carrying all but what it names as approximated' }),
        ]),
    },
);

class Description extends Schema.Class<Description, { readonly brand: unique symbol }>('Description')(
    Schema.Struct({ containers: Schema.Array(FacetContainer), pointer: Schema.optionalKey(FacetPointer) }).check(
        Schema.makeFilter(({ containers }) => {
            const held = located(containers);
            return Array.dedupe(held).length === held.length || 'each container, control, and option a host source describes takes its own locator';
        }),
    ),
    { description: 'Containers and pointer a host source states for the part of an application interface it owns, each top-level container replacing the one Accessibility container of its kind' },
) {
    static readonly Locator = Locator;
    static readonly Operation = Operation;
    static readonly tools = <E, R, EC, RC>({ current, listed, select, changes, options, pointer = Effect.succeed({}), canvas }: Toolbar<E, R, EC, RC>) => {
        const state = Effect.map(Effect.all([current, pointer]), ([tool, held]) => ({ tool, pointer: { ...held, ...Struct.pick(tool, ['shortcut', 'help']), tool: tool.label } satisfies FacetPointer }));
        const stated = Effect.gen(function* () {
            const [{ tool, pointer: held }, tools, viewed] = yield* Effect.all([state, Effect.transposeOption(Option.fromUndefinedOr(listed)), Effect.transposeOption(Option.map(Option.fromUndefinedOr(canvas), ({ view }) => Effect.option(Effect.tapCause(view, (cause) => Effect.logDebug('Canvas unread', cause)))))]);
            const settings = yield* options?.describe(toolAt(tool.id)) ?? Effect.succeed([]);
            const chosen = FacetControl.make({
                locator: CHOICE,
                kind: 'choice',
                role: 'tool',
                label: { text: 'tool' },
                value: { _tag: 'text', value: Option.isSome(tools) ? tool.label : tool.id },
                selected: 'attribute',
                ...Option.getOrUndefined(Option.map(tools, (each) => ({ options: Array.map(each, ({ id, ...facts }) => ({ ...facts, ...(id === tool.id ? Struct.omit(tool, ['id']) : {}), locator: toolAt(id) })) }))),
            });
            const region = Option.map(Option.flatten(viewed), ({ frame }) => FacetControl.make({ locator: CANVAS, kind: 'region', role: 'canvas', label: { text: 'canvas' }, frame }));
            return Description.make({ containers: [{ locator: TOOLBAR, kind: 'toolbar', controls: [chosen, ...settings, ...Option.toArray(region)] }], pointer: held });
        });
        const take = Effect.fnUntraced(function* (id: string) {
            yield* select(id);
            const taken = yield* current;
            return yield* taken.id === id ? Effect.void : Unmatched.of({ subject: 'tool the host took', query: Option.some(id), candidates: [taken.id, taken.label] });
        });
        const resolve = (to: string) =>
            listed === undefined
                ? Effect.succeed(to)
                : listed.pipe(
                      Effect.flatMap(Unmatched.single('tool', Option.some(to), ({ id, label }: Tool) => [label, id])),
                      Effect.map(({ id }) => id),
                  );
        const configure = (key: string, path: Array.NonEmptyReadonlyArray<string>, to: Observation['values'][string] | undefined) =>
            options === undefined
                ? Effect.fail(Unmatched.of({ subject: 'option of a tool', query: Option.some(key) }))
                : current.pipe(
                      Effect.filterOrFail(
                          ({ id }) => id === key,
                          ({ id }) => Unmatched.of({ subject: 'option of the current tool', query: Option.some(key), candidates: [id] }),
                      ),
                      Effect.andThen(options.operate(key, path, to)),
                  );
        const route = ([role, key]: readonly [string, string], below: readonly string[], to: Observation['values'][string] | undefined) =>
            Match.value({ role, path: Array.isReadonlyArrayNonEmpty(below) ? below : undefined, to }).pipe(
                Match.withReturnType<Effect.Effect<unknown, E | Unmatched | Cause.IllegalArgumentError, R>>(),
                Match.when({ role: 'choice', path: Match.undefined, to: Match.string }, ({ to: named }) => Effect.flatMap(resolve(named), take)),
                Match.when({ role: 'tool', path: Match.undefined, to: Match.undefined }, () => take(key)),
                Match.when({ role: 'tool', path: Match.defined }, ({ path }) => configure(key, path, to)),
                Match.orElse(() => Effect.fail(new Cause.IllegalArgumentError(`the tool choice takes the name of a tool, a tool takes no value, and a tool option takes the value it holds, received ${typeof to} at ${role} ${key}`))),
            );
        const draw = ({ locator, input }: Extract<typeof Operation.Type, { readonly input: unknown }>) =>
            Option.match(
                Option.filter(Option.fromUndefinedOr(canvas), () => same(locator, CANVAS)),
                {
                    onNone: () => Effect.fail(Unmatched.of({ subject: 'canvas of the host source', query: Option.some(Formatter.format(locator)), candidates: canvas === undefined ? [] : [Formatter.format(CANVAS)] })),
                    onSome: ({ view, apply }) =>
                        (input.button ?? 'left') === 'left' && input.by === undefined && input.edge === undefined && (input.count ?? 1) === 1
                            ? Effect.map(
                                  Effect.flatMap(view, (held) => apply(plane(held, input))),
                                  (applied) => Act.Route.cases.source.make(applied),
                              )
                            : Effect.fail(new Cause.IllegalArgumentError(`a host source draws one whole left-button press or drag on its canvas, and the step is ${Formatter.format(Struct.pick(input, ['button', 'by', 'edge', 'count']))}`)),
                },
            );
        const press = Effect.fnUntraced(function* ({ locator, to }: Exclude<typeof Operation.Type, { readonly input: unknown }>): Effect.fn.Return<void, E | Unmatched | Cause.IllegalArgumentError, R> {
            const step = yield* Effect.fromOption(
                Option.filter(Array.get(locator, 1), () => same([Array.headNonEmpty(locator)], TOOLBAR)),
                () => Unmatched.of({ subject: 'facet control', query: Option.some(Formatter.format(locator)) }),
            );
            yield* route(step, Array.map(Array.drop(locator, 2), Tuple.get(1)), to);
        });
        const reading = Effect.result(Effect.map(state, ({ pointer: held }) => held));
        return {
            roots: Effect.succeed([TOOLBAR]),
            describe: ({ within }: { readonly within?: FacetControl['locator'] }) => (within === undefined || same(within, TOOLBAR) ? stated : Effect.fail(Unmatched.of({ subject: 'facet item', query: Option.some(Formatter.format(within)) }))),
            operate: (payload: typeof Operation.Type) => ('input' in payload ? draw(payload) : press(payload)),
            changes: Stream.unwrap(
                Effect.map(reading, (seed) =>
                    changes.pipe(
                        Stream.mapEffect(() => reading),
                        Stream.mapAccum(
                            (): Polled => ({ known: Result.getSuccess(seed), failing: Result.isFailure(seed) }),
                            ({ known, failing }: Polled, read: Effect.Success<typeof reading>): readonly [Polled, readonly Effect.Success<typeof reading>[]] =>
                                Result.match(read, {
                                    onFailure: (error) => [{ known, failing: true }, failing ? [] : [Result.fail(error)]],
                                    onSuccess: (held) => [{ known: Option.some(held), failing: false }, Option.exists(known, (last) => samePointer(last, held)) ? [] : [Result.succeed(held)]],
                                }),
                        ),
                        Stream.filterMapEffect(
                            Result.match({
                                onFailure: (error) => Effect.as(Effect.logWarning('Pointer unread', Cause.fail(error)), Result.failVoid),
                                onSuccess: (held) => Effect.map(Effect.orDie(Schema.encodeEffect(Schema.toCodecJson(FacetPointer))(held)), (detail) => Result.succeed({ name: 'pointer' satisfies keyof Model, target: Option.none<string>(), detail: Option.some(detail) })),
                            }),
                        ),
                    ),
                ),
            ),
        };
    };
    static readonly merge: {
        <E2 = never, R2 = never, EC2 = never, RC2 = never>(that: Part<E2, R2, EC2, RC2>): <E1 = never, R1 = never, EC1 = never, RC1 = never>(self: Part<E1, R1, EC1, RC1>) => Source<E1 | E2 | Unmatched, R1 | R2, never, RC1 | RC2>;
        <E1 = never, R1 = never, EC1 = never, RC1 = never, E2 = never, R2 = never, EC2 = never, RC2 = never>(self: Part<E1, R1, EC1, RC1>, that: Part<E2, R2, EC2, RC2>): Source<E1 | E2 | Unmatched, R1 | R2, never, RC1 | RC2>;
    } = Function.dual(2, <E1, R1, EC1, RC1, E2, R2, EC2, RC2>(self: Part<E1, R1, EC1, RC1>, that: Part<E2, R2, EC2, RC2>): Source<E1 | E2 | Unmatched, R1 | R2, never, RC1 | RC2> => {
        const facets: readonly Facet<E1 | E2, R1 | R2>[] = Array.flatMap([self, that], (part) => ('roots' in part ? [part] : []));
        const owner = Effect.fnUntraced(function* (locator: FacetControl['locator']) {
            const root: FacetControl['locator'] = [Array.headNonEmpty(locator)];
            const held = yield* Effect.forEach(facets, Struct.get('roots'));
            const candidates = Array.map(Array.flatten(held), (each) => Formatter.format(each));
            const [facet] = yield* Effect.fromOption(
                Array.findFirst(Array.zip(facets, held), ([, roots]) => Array.containsWith(same)(roots, root)),
                () => Unmatched.of({ subject: 'facet root', query: Option.some(Formatter.format(root)), candidates }),
            );
            return facet;
        });
        return {
            roots: Effect.map(Effect.forEach(facets, Struct.get('roots')), Array.flatten),
            describe: ({ within }) =>
                within === undefined
                    ? Effect.forEach(facets, (facet) => Effect.exit(facet.describe({}))).pipe(
                          Effect.flatMap((exits) => Effect.as(Exit.asVoidAll(exits), Array.filterMap(exits, Exit.filterValue))),
                          Effect.map((described) =>
                              Description.make({
                                  containers: Array.flatMap(described, Struct.get('containers')),
                                  ...Array.reduce(described, {}, (held: Pick<Description, 'pointer'>, { pointer }) => (pointer === undefined ? held : { pointer: layered(held.pointer ?? pointer, pointer) })),
                              }),
                          ),
                      )
                    : Effect.flatMap(owner(within), (facet) => facet.describe({ within })),
            operate: (payload) => Effect.flatMap(owner(payload.locator), (facet) => facet.operate(payload)),
            changes: Stream.mergeAll(
                Array.flatMap([self, that], (part) => ('changes' in part ? [Stream.ignoreCause(part.changes, { log: 'Error' })] : [])),
                { concurrency: 'unbounded' },
            ),
        };
    });
    static readonly fold = (described: Description, { tools, toolbar }: Declaration): Description => {
        const known = (id: string, stated: Partial<Omit<Tool, 'id'>>) => {
            const declared = Option.getOrElse(HashMap.get(tools, id), (): Partial<Omit<Tool, 'id'>> => ({}));
            return { ...declared, ...stated, label: stated.label === undefined || stated.label === id ? (declared.label ?? id) : stated.label };
        };
        return Option.match(
            Array.findFirst(Array.flatMap(described.containers, Struct.get('controls')), ({ locator }) => same(locator, CHOICE)),
            {
                onNone: () => described,
                onSome: ({ options = [], value }) => {
                    const keyOf = ({ locator }: (typeof options)[number]) => Array.lastNonEmpty(locator)[1];
                    const byId = Record.fromIterableWith(options, (option) => [keyOf(option), option] as const);
                    const shown = Option.fromUndefinedOr(value).pipe(Option.filterMap(Filter.tagged<NonNullable<FacetControl['value']>>()('text')), Option.map(Struct.get('value')));
                    const chosen = Option.flatMap(shown, (text) => Array.findFirst(options, ({ label }) => label === text));
                    const listed = Array.map(Array.dedupe([...Array.map(options, keyOf), ...Array.flatten(toolbar)]), (id) => ({
                        ...known(
                            id,
                            Option.getOrElse(Record.get(byId, id), () => ({})),
                        ),
                        locator: toolAt(id),
                    }));
                    const held = described.pointer ?? FacetPointer.make({});
                    const pointer = Option.map(
                        Option.orElse(Option.map(chosen, keyOf), () => shown),
                        (id) => ({ ...held, ...Struct.renameKeys(known(id, Struct.renameKeys(Struct.pick(held, ['tool', 'shortcut', 'help']), { tool: 'label' })), { label: 'tool' }) }),
                    );
                    const choice = {
                        ...Option.getOrUndefined(Option.map(chosen, (option) => ({ value: { _tag: 'text' as const, value: known(keyOf(option), option).label } }))),
                        ...(Array.isReadonlyArrayNonEmpty(listed) ? { options: listed } : {}),
                    };
                    return Description.make({
                        containers: Array.map(described.containers, ({ controls, ...container }) => ({ ...container, controls: Array.map(controls, (control) => (same(control.locator, CHOICE) ? { ...control, ...choice } : control)) })),
                        ...Option.getOrUndefined(
                            Option.map(
                                Option.orElse(pointer, () => Option.fromUndefinedOr(described.pointer)),
                                (each) => ({ pointer: each }),
                            ),
                        ),
                    });
                },
            },
        );
    };
    static readonly compose = Effect.fnUntraced(function* <E, R>(model: Model, description: Description, intern: (locators: readonly string[]) => Effect.Effect<readonly Control['ref'][], E, R>) {
        const paths = Array.dedupe([...located(description.containers), ...Array.flatMap(description.containers, ({ parent, controls }) => [...Array.fromNullishOr(parent), ...Array.flatMap(controls, ({ opens }) => Array.fromNullishOr(opens))])]);
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
                ...(options === undefined ? {} : { options: yield* Effect.forEach(options, ({ locator: option, ...stated }) => Effect.map(at(option), Function.flow(Struct.pick(['ref']), Struct.assign(stated)))) }),
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
                ...(description.pointer === undefined ? {} : { pointer: layered<NonNullable<Model['pointer']>>(model.pointer ?? description.pointer, description.pointer) }),
            } satisfies Model,
            routes: Record.fromIterableWith(Array.zip(interned, paths), ([ref, path]) => [`${ref}`, path] as const),
        };
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const same = Schema.toEquivalence(Locator);
const samePointer = Schema.toEquivalence(FacetPointer);
const layered = <A extends FacetPointer>(under: A, over: FacetPointer) => ({ ...(over.tool === undefined ? under : Struct.omit(under, ['tool', 'shortcut', 'help'])), ...over });
const toolAt = (id: string) => Array.append(TOOLBAR, ['tool', id] as const);
const plane = ({ frame: [[left, top]], transform: [a, b, c, d, tx, ty] }: View, { pointer, path, ...input }: typeof Act.Input.Type): typeof Act.Input.Type => {
    const place = ([x, y, ...rest]: (typeof Act.Input.Type)['pointer']): (typeof Act.Input.Type)['pointer'] => [a * (x - left) + c * (y - top) + tx, b * (x - left) + d * (y - top) + ty, ...rest];
    return { ...input, pointer: place(pointer), ...(path === undefined ? {} : { path: Array.map(path, place) }) };
};
const located = (containers: readonly FacetContainer[]): readonly FacetControl['locator'][] => Array.flatMap(containers, ({ locator, controls }) => [locator, ...Array.flatMap(controls, ({ locator: control, options = [] }) => [control, ...Array.map(options, Struct.get('locator'))])]);

// --- [COMPOSITION] ---------------------------------------------------------------------

const failing = <E extends Schema.Top>(error: E) =>
    RpcGroup.make(
        Rpc.make('describe', {
            payload: { within: Schema.optionalKey(Locator.annotate({ description: 'Facet item whose level is described alone, every facet at its top level when absent' })) },
            success: Description,
            error,
        }),
        Rpc.make('operate', {
            payload: Operation,
            success: Operation.Result,
            error,
        }),
    );
const SourceRpcs = Object.assign(failing(Failure), { failing });

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Declaration, Description, FacetContainer, FacetControl, FacetPointer, type Source, SourceRpcs };
