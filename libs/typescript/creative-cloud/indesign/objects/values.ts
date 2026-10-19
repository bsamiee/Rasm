import indesign, { type Document, type Enumerator, type InDesignDOMObject } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cache, Effect, flow, Match, Option, Predicate, Record, Schema, Struct } from 'effect';
import { InDesignError, Reference, Specifier } from '../models.ts';
import { Classes, element, enumeration, lineage, type Member, members, plural, unavailable } from './classes.ts';
import { active, collection, host, named, owner, read, record, reference, resolve } from './specifiers.ts';
import { Description, transacted } from './transactions.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Referenced = Schema.Struct({ type: Schema.String, specifier: Schema.String });
const Enumerated = Schema.Struct({ type: Schema.String, member: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DECODING]
const member = (kind: string, name: string) =>
    Effect.flatMap(
        Effect.fromOption(enumeration(kind), () => Unmatched.make({ subject: 'enumeration', query: kind, candidates: Array.flatMap(Object.getOwnPropertyNames(indesign), (each) => Option.toArray(Option.map(enumeration(each), () => each))) })),
        (found) =>
            Effect.fromOption(
                Option.liftPredicate(Reflect.get(found, name), (value): value is Enumerator => value instanceof indesign.Enumerator),
                () => Unmatched.make({ subject: `${kind} member`, query: name, candidates: Array.map(members(found), String) }),
            ),
    );
const catalogued = Effect.fn('catalogued')(function* (scope: Document, kind: string) {
    const classes = yield* Classes;
    const rows = yield* Cache.get(classes, 'Document').pipe(Effect.map(flow(Struct.get('fields'), Record.toEntries)));
    const arrays = Array.flatMap(rows, ([field, { array, type }]) => (Option.isSome(array) && Option.contains(type, kind) ? [field] : []));
    const collections = yield* Effect.forEach(
        rows.filter(([, { array, type }]) => Option.isNone(array) && Option.isSome(type)),
        ([field, { type }]) => Effect.map(element(Option.getOrThrow(type)), (held) => (Option.contains(held, kind) ? [field] : [])),
    );
    return Array.flatMap([...arrays, ...Array.flatten(collections)], (field) => {
        const value: unknown = Reflect.get(scope, field);
        return collection(value) ? value.everyItem().getElements() : Array.ensure<unknown>(value).filter(host);
    });
});
const decoded = (scope: Option.Option<Document>, declared: Option.Option<Pick<typeof Member.Type, 'type'>>): ((value: unknown) => Effect.Effect<unknown, InDesignError | Unmatched, Classes>) => {
    const kind = Option.flatMap(declared, Struct.get('type'));
    const byName: (type: string, name: string) => Effect.Effect<unknown, InDesignError | Unmatched, Classes> = Effect.fnUntraced(function* (type: string, name: string) {
        const classes = yield* Classes;
        const [rows, parents] = yield* Effect.all([Cache.get(classes, type), lineage(type)]);
        const within = Option.filter(scope, () => Option.isSome(Record.get(rows.fields, 'name')) && ![type, ...parents].includes('Swatch'));
        const found = yield* Effect.transposeOption(Option.map(within, (document) => catalogued(document, type)));
        return yield* Option.match(found, {
            onNone: () => Effect.succeed<unknown>(name),
            onSome: (candidates) =>
                Array.match(
                    candidates.filter(named).filter((each) => each.name === name),
                    {
                        onEmpty: () => Effect.fail(Unmatched.make({ subject: type, query: name, candidates: Array.dedupe(Array.map(candidates.filter(named), Struct.get('name'))) })),
                        onNonEmpty: ([only, ...rest]) => (rest.length === 0 ? Effect.succeed<unknown>(only) : Effect.fail(Unmatched.make({ subject: `single ${type}`, query: name, candidates: [only, ...rest].map((each) => each.toSpecifier()) }))),
                    },
                ),
        });
    });
    const font = (name: string) =>
        Effect.filterOrFail(
            Effect.try({ try: () => indesign.app.fonts.itemByName(name), catch: InDesignError.refused(Option.some(name)) }),
            (found) => !found.status.equals(indesign.FontStatus.NOT_AVAILABLE),
            () => Unmatched.make({ subject: 'installed font', query: name, candidates: [] }),
        );
    const decode: (value: unknown) => Effect.Effect<unknown, InDesignError | Unmatched, Classes> = Match.type<unknown>().pipe(
        Match.when(Schema.is(Referenced), ({ specifier }) => resolve(specifier)),
        Match.when(Schema.is(Enumerated), ({ type, member: name }) => member(type, name)),
        Match.when(Predicate.isString, (text) =>
            Option.match(kind, {
                onNone: () => Effect.succeed(text),
                onSome: (type) =>
                    Match.value(type).pipe(
                        Match.when(flow(enumeration, Option.isSome), (each) => member(each, text)),
                        Match.when('Font', () => font(text)),
                        Match.when(
                            (each) => Predicate.isFunction(Reflect.get(indesign, each)),
                            (each) => byName(each, text),
                        ),
                        Match.orElse(() => Effect.succeed(text)),
                    ),
            }),
        ),
        Match.when(Array.isArray, (items) => Effect.forEach(items, decode)),
        Match.when(record, (properties) => Effect.all(Record.map(properties, decoded(scope, Option.none())))),
        Match.orElse(Effect.succeed),
    );
    return decode;
};

// --- [WRITES]
const preserve = <T extends object>(target: T, replacements: Partial<T>) => {
    const refused = InDesignError.refused(Option.none());
    return Effect.acquireRelease(Effect.try({ try: () => Record.fromIterableWith(Struct.keys(replacements), (key) => [key, target[key]]), catch: refused }), (saved) => Effect.try({ try: () => Object.assign(target, saved), catch: refused }).pipe(Effect.orDie)).pipe(
        Effect.tap(() => Effect.try({ try: () => Object.assign(target, replacements), catch: refused })),
    );
};
const assigned: (scope: Option.Option<Document>, target: InDesignDOMObject<unknown>, properties: Record.ReadonlyRecord<string, unknown>) => Effect.Effect<readonly string[], InDesignError | Unmatched, Classes> = Effect.fnUntraced(function* (scope, target, properties) {
    const specifier = target.toSpecifier();
    const { fields } = yield* Cache.get(yield* Classes, target.constructorName).pipe(Effect.mapError(InDesignError.refused(Option.some(specifier))));
    return yield* Effect.validate(
        Object.entries(properties),
        Effect.fnUntraced(function* ([field, value]) {
            const refused = InDesignError.refused(Option.some(`${specifier} ${field}`));
            const row = Record.get(fields, field);
            const nested = record(value) && Option.exists(row, ({ settable }) => !settable);
            yield* Effect.transposeOption(Option.map(unavailable(target.constructorName, field, Option.some(target), !nested), flow(refused, Effect.fail)));
            const inner: unknown = nested ? yield* Effect.try({ try: () => Reflect.get(target, field), catch: refused }) : undefined;
            if (record(value) && host(inner)) {
                return yield* assigned(scope, inner, value);
            }
            const native = yield* decoded(scope, row)(value);
            yield* Effect.try({ try: () => Object.assign(target, { [field]: native }), catch: refused });
            return [field];
        }),
    ).pipe(
        Effect.map(Array.flatten),
        Effect.mapError((errors) => (errors.length === 1 ? Array.headNonEmpty(errors) : InDesignError.make({ reasons: Array.flatMap(errors, (error) => (Schema.is(InDesignError)(error) ? error.reasons : [{ _tag: 'Refused' as const, at: Option.some(`${error.subject} ${error.query}`), cause: error }])) }))),
    );
});
const write = (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>) =>
    assigned(
        Option.orElse(owner(target), () => Option.liftThrowable(() => indesign.app.activeDocument)()),
        target,
        values,
    );

// --- [VERBS]
const within = (specifier: string) => Effect.filterOrFail(resolve(specifier), host, () => Unmatched.make({ subject: 'object', query: specifier, candidates: [] }));
class Change extends Schema.Class<Change, { readonly brand: unique symbol }>('Change')(
    {
        description: Description,
        target: Specifier,
        values: Schema.JsonObject.annotate({ description: 'Fields to write by name. Enumeration members by name, named objects (styles, layers, parents, fonts) by name or {type, specifier}, swatches by name, measurements as points or unit strings; a record writes into a nested preference object' }),
    },
    { description: "Writes fields of one object through InDesign's own setters in one undo step, decoding each value by the field's declared type; every refused field is reported" },
) {
    static readonly Result = Reference.pipe(Schema.fieldsAssign({ written: Schema.Array(Schema.String) }));

    run() {
        const { target, values } = this;
        return transacted(
            this.description,
            Effect.gen(function* () {
                const found = yield* within(target);
                const written = yield* write(found, values);
                return yield* read(Change.Result, () => ({ ...reference(found), written }));
            }),
        );
    }
}
class Create extends Schema.Class<Create, { readonly brand: unique symbol }>('Create')(
    {
        description: Description,
        owner: Schema.OptionFromOptionalKey(Specifier.annotate({ identifier: undefined, description: 'Specifier of the object owning the collection; the active document when absent' })),
        collection: Schema.String.annotate({ description: 'Collection field of the owner (pages, paragraphStyles, rectangles, nestedGrepStyles, tabStops, swatches)' }),
        arguments: Schema.OptionFromOptionalKey(Schema.Array(Schema.Json).annotate({ description: "Positional arguments of the collection's add method, decoded by parameter type; inspect the collection class for its signature" })),
        properties: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: 'Fields of the new object, decoded as change decodes them' })),
    },
    { description: "Adds a member to a collection through its native add method in one undo step and returns the new object's reference" },
) {
    static readonly Result = Reference;

    run() {
        const { owner: given, collection: field, arguments: positional, properties } = this;
        return transacted(
            this.description,
            Effect.gen(function* () {
                const classes = yield* Classes;
                const found = yield* Option.match(given, { onNone: () => active, onSome: within });
                const { fields } = yield* Cache.get(classes, found.constructorName);
                const collections = Record.keys(Record.filter(fields, plural));
                const items = yield* Effect.fromOption(Option.liftPredicate(Reflect.get(found, field), collection), () => Unmatched.make({ subject: `collection of ${found.toSpecifier()}`, query: field, candidates: collections }));
                const scope = Option.orElse(owner(found), () => Option.liftThrowable(() => indesign.app.activeDocument)());
                const [{ methods }, held] = yield* Effect.all([Cache.get(classes, items.constructor.name), element(items.constructor.name)]);
                const add = yield* Effect.fromOption(Record.get(methods, 'add'), () => Unmatched.make({ subject: `method of ${items.constructor.name}`, query: 'add', candidates: Record.keys(methods) }));
                const rows = yield* Effect.transposeOption(Option.map(held, (kind) => Cache.get(classes, kind))).pipe(Effect.map(Option.map(Struct.get('fields'))));
                const values = yield* Effect.transposeOption(Option.map(properties, (supplied) => Effect.all(Record.map(supplied, (value, name) => decoded(scope, Option.flatMap(rows, Record.get(name)))(value)))));
                const slot = Array.findFirstIndex(add.parameters, ({ name }) => name === 'withProperties');
                const positioned = yield* Effect.forEach(Array.zip(Option.getOrElse(positional, Array.empty), add.parameters), ([value, parameter]) => decoded(scope, Option.some(parameter))(value));
                const placed = Option.match(Option.all({ slot, values }), { onNone: () => positioned, onSome: ({ slot: at, values: supplied }) => Array.replace(Array.pad(positioned, at + 1, undefined), at, supplied) });
                const refused = InDesignError.refused(Option.some(`${items.everyItem().toSpecifier()} add`));
                const made = yield* Effect.try({ try: (): unknown => Reflect.apply(items.add, items, [...placed]), catch: refused }).pipe(Effect.flatMap((result) => Effect.fromOption(Array.head(Array.ensure<unknown>(result).filter(host)), () => refused(result))));
                yield* Option.isSome(slot) ? Effect.void : Effect.forEach(Option.toArray(properties), (supplied) => write(made, supplied), { discard: true });
                return yield* read(Create.Result, () => reference(made));
            }),
        );
    }
}
class Remove extends Schema.Class<Remove, { readonly brand: unique symbol }>('Remove')({ description: Description, target: Specifier }, { description: "Removes one object through its native remove method in one undo step; host-owned members ([None], [Paper], [Basic Paragraph]) answer with InDesign's refusal" }) {
    static readonly Result = Reference;

    run() {
        const { target } = this;
        return transacted(
            this.description,
            Effect.gen(function* () {
                const found = yield* within(target);
                const removed = yield* read(Remove.Result, () => reference(found));
                const remover = yield* Effect.fromOption(
                    Option.flatMap(Option.liftPredicate(found, Predicate.hasProperty('remove')), (removable) => Option.liftPredicate(removable.remove, Predicate.isFunction)),
                    () => Unmatched.make({ subject: `method of ${target}`, query: 'remove', candidates: [] }),
                );
                yield* Effect.try({ try: () => Reflect.apply(remover, found, []), catch: InDesignError.refused(Option.some(target)) });
                return removed;
            }),
        );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Change, Create, preserve, Remove, write };
