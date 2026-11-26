import { Unmatched } from '@rasm/search';
import { Array, Cause, Effect, Formatter, Function, HashMap, Option, Predicate, Record, Result, Schema, SchemaTransformation, Struct } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';
import { Container, Control, Failure, type Model, Pointer, type Ref, Value } from '#/contract';

// --- [MODELS] --------------------------------------------------------------------------

const Locator = Schema.fromJsonString(
    Schema.NonEmptyArray(
        Schema.Struct({ key: Schema.Struct({ identifier: Schema.Struct({ value: Schema.String }) }), role: Schema.String }).pipe(
            Schema.decodeTo(Schema.Tuple([Schema.String, Schema.String]), SchemaTransformation.transform({ decode: ({ role, key }) => [role, key.identifier.value] as const, encode: ([role, value]) => ({ key: { identifier: { value } }, role }) })),
        ),
    ),
).annotate({ description: 'Locator of a facet container, control, or option: the role and identifier of each step from the application down, encoded as the canonical locator text' });
const FacetControl = Control.mapFields((fields) => ({
    ...Struct.omit(fields, ['ref', 'positional', 'opens', 'alternate']),
    locator: Locator,
    opens: Schema.optionalKey(Locator.annotate({ description: 'Facet container the control opens' })),
    options: Schema.optionalKey(Schema.Array(Schema.Struct({ label: Schema.String, locator: Locator })).annotate({ description: 'Options of a choice, each addressed by its own locator' })),
}));
const FacetContainer = Schema.Struct({
    ...Struct.pick(Container.fields, ['kind', 'title']),
    locator: Locator,
    parent: Schema.optionalKey(Locator.annotate({ description: 'Facet container this one sits in, a container with a parent never folding an Accessibility container' })),
    controls: Schema.Array(FacetControl),
});
const FacetPointer = Pointer.mapFields((fields) => Struct.pick(fields, ['tool', 'holding', 'operations']));

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
    static readonly pointer = <E, R>(read: Effect.Effect<typeof FacetPointer.Type, E, R>) => ({
        describe: ({ within }: { readonly within?: typeof Locator.Type }) =>
            Option.match(Option.fromUndefinedOr(within), { onNone: () => Effect.map(read, (pointer) => Description.make({ containers: [], pointer })), onSome: (item) => Effect.fail(Unmatched.of({ subject: 'facet item', query: Option.some(Formatter.format(item)) })) }),
        operate: ({ locator }: { readonly locator: typeof Locator.Type }) => Effect.fail(Unmatched.of({ subject: 'facet control', query: Option.some(Formatter.format(locator)) })),
    });
    static readonly compose = Effect.fnUntraced(function* <E, R>(model: typeof Model.Type, description: Description, intern: (locators: readonly string[]) => Effect.Effect<readonly (typeof Ref.Type)[], E, R>) {
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
        const at = (path: typeof Locator.Type) => Effect.fromOption(HashMap.get(placed, path), () => new Cause.NoSuchElementError(`Application server interned no reference for facet locator ${Formatter.format(path)}`));
        const byKind = Array.groupBy(model.containers, Struct.get('kind'));
        const placeControl = Effect.fnUntraced(function* ({ locator, options, opens, ...control }: typeof FacetControl.Type) {
            return {
                ...control,
                ...(yield* at(locator)),
                positional: false,
                ...(opens === undefined ? {} : { opens: (yield* at(opens)).ref }),
                ...(options === undefined ? {} : { options: yield* Effect.forEach(options, ({ label, locator: option }) => Effect.map(at(option), Function.flow(Struct.pick(['ref']), Struct.assign({ label })))) }),
            };
        });
        const placeContainer = Effect.fnUntraced(function* ({ locator, parent, controls, ...container }: typeof FacetContainer.Type) {
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
            } satisfies typeof Model.Type,
            routes: Record.fromIterableWith(Array.zip(interned, paths), ([ref, path]) => [`${ref}`, path] as const),
        };
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const located = (containers: readonly (typeof FacetContainer.Type)[]): readonly (typeof Locator.Type)[] => Array.flatMap(containers, ({ locator, controls }) => [locator, ...Array.flatMap(controls, ({ locator: control, options = [] }) => [control, ...Array.map(options, Struct.get('locator'))])]);

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

export { Description, SourceRpcs };
