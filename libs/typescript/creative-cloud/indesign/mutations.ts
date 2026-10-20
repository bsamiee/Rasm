import type { InDesignDOMObject } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cache, Cause, Effect, flow, Match, Option, Predicate, Record, Schema, Struct } from 'effect';
import { type Member, Model } from './model.ts';
import { active, font, owner, Reference, record, resolve, Specifier } from './references.ts';
import { Mutation } from './transactions.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DECODING]
const member = Effect.fnUntraced(function* (kind: string, name: string) {
    const { Enumerator, enumeration, members, enumerations } = yield* Model;
    const found = yield* Effect.fromOption(enumeration(kind), () => Unmatched.make({ subject: 'enumeration', query: Option.some(kind), candidates: enumerations }));
    return yield* Effect.fromOption(Option.liftPredicate(Reflect.get(found, name), Schema.is(Schema.instanceOf(Enumerator))), () => Unmatched.make({ subject: `${kind} member`, query: Option.some(name), candidates: Array.map(members(found), String) }));
});
const decoded = (target: unknown, row: Option.Option<Pick<typeof Member.Type, 'type' | 'description'>>): ((value: unknown) => Effect.Effect<unknown, Cause.UnknownError | Unmatched, Model>) => {
    const byName = Effect.fnUntraced(function* (type: string, name: string) {
        const model = yield* Model;
        const [{ fields }, parents] = yield* Effect.all([Cache.get(model.rows, type), model.lineage(type)]);
        const listing = Record.has(fields, 'name') && ![type, ...parents].includes('Swatch') ? yield* Cache.get(model.catalog, type) : Option.none();
        const scope = Option.orElse(yield* owner(target), () => Option.liftThrowable(() => model.app.activeDocument)());
        return yield* Option.match(Option.all([scope, listing]), {
            onNone: () => Effect.succeed<unknown>(name),
            onSome: ([document, field]) => {
                const listed: unknown = Reflect.get(document, field);
                return Unmatched.single(type, Option.some(name))((model.collection(listed) ? listed.everyItem().getElements() : Array.ensure(listed)).filter(model.named), (each) => [each.toSpecifier(), each.name]);
            },
        });
    });
    const typed = (text: string) =>
        Effect.flatMap(Model, ({ enumeration, classes, declared }) =>
            Match.value(Option.match(row, { onNone: Array.empty, onSome: declared })).pipe(
                Match.when(Schema.is(Schema.Tuple([Schema.String])), ([type]) =>
                    Match.value(type).pipe(
                        Match.when(flow(enumeration, Option.isSome), (each) => member(each, text)),
                        Match.when('Font', () => font(text)),
                        Match.when(
                            (each) => classes.includes(each),
                            (each) => byName(each, text),
                        ),
                        Match.orElse(() => Effect.succeed<unknown>(text)),
                    ),
                ),
                Match.orElse((types) => Effect.firstSuccessOf([...types.filter(flow(enumeration, Option.isSome)).map((each) => member(each, text)), Effect.succeed<unknown>(text)])),
            ),
        );
    const decode: (value: unknown) => Effect.Effect<unknown, Cause.UnknownError | Unmatched, Model> = Match.type<unknown>().pipe(
        Match.when(Schema.is(Schema.Struct({ type: Schema.String, specifier: Schema.String })), ({ specifier }) => resolve(specifier)),
        Match.when(Schema.is(Schema.Struct({ type: Schema.String, member: Schema.String })), ({ type, member: name }) => member(type, name)),
        Match.when(Predicate.isString, typed),
        Match.when(Array.isArray, (items) => Effect.forEach(items, decode)),
        Match.when(record, (properties) => Effect.all(Record.map(properties, decoded(target, Option.none())))),
        Match.orElse(Effect.succeed),
    );
    return decode;
};

// --- [WRITES]
const write: (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>) => Effect.Effect<readonly string[], Cause.UnknownError | Unmatched, Model> = Effect.fnUntraced(function* (target, values) {
    const specifier = target.toSpecifier();
    const { rows, unavailable, host } = yield* Model;
    const { fields } = yield* Cache.get(rows, target.constructorName).pipe(Effect.mapError((cause) => new Cause.UnknownError(cause, specifier)));
    return yield* Effect.validate(
        Object.entries(values),
        Effect.fnUntraced(function* ([field, value]: readonly [string, unknown]) {
            const refused = (cause: unknown) => new Cause.UnknownError(cause, `${specifier} ${field}`);
            const row = Record.get(fields, field);
            const nested = record(value) && Option.exists(row, ({ settable }) => !settable);
            yield* Effect.transposeOption(Option.map(unavailable(target.constructorName, field, Option.some(target), !nested), flow(refused, Effect.fail)));
            const inner: unknown = nested ? yield* Effect.try({ try: () => Reflect.get(target, field), catch: refused }) : undefined;
            if (record(value) && host(inner)) {
                return Array.map(yield* write(inner, value), (name) => `${field}.${name}`);
            }
            const native = yield* decoded(target, row)(value);
            yield* Effect.try({ try: () => Object.assign(target, { [field]: native }), catch: refused });
            return [field];
        }, Effect.sandbox),
    ).pipe(
        Effect.map(Array.flatten),
        Effect.catch((causes) => Effect.failCause(Cause.fromReasons(causes.flatMap(Struct.get('reasons'))))),
    );
});

// --- [VERBS]
const leaf = (specifier: string): string => specifier.slice(specifier.lastIndexOf('/') + 1);
const within = (specifier: string) => Effect.flatMap(Model, ({ host }) => Effect.filterOrFail(resolve(specifier), host, () => Unmatched.make({ subject: 'object', query: Option.some(specifier), candidates: [] })));
class Change extends Mutation.extend<Change>('Change')(
    {
        target: Specifier,
        values: Schema.JsonObject.annotate({ description: 'Fields to write by name. Enumeration members by name, named objects (styles, layers, parents, fonts) by name or {type, specifier}, swatches by name, measurements as points or unit strings; a record writes into a nested preference object' }),
    },
    { description: "Writes fields of one object through InDesign's own setters in one undo step, decoding each value by the field's declared type; every refused field is reported, a nested field as its path" },
) {
    static readonly Own = Reference.mapFields(Struct.assign({ written: Schema.Array(Schema.String) }));
    static readonly Result = Mutation.summarized(Change.Own);

    override get label(): string {
        return `Change ${Record.keys(this.values).join(', ')} of ${leaf(this.target)}`;
    }

    run() {
        const { target, values } = this;
        return this.commit(
            Change.Own,
            Effect.gen(function* () {
                const found = yield* within(target);
                const written = yield* write(found, values);
                return { ...Reference.of(found), written };
            }),
        );
    }
}
class Create extends Mutation.extend<Create>('Create')(
    {
        owner: Schema.OptionFromOptionalKey(Specifier.annotate({ description: 'Specifier of the object owning the collection; the active document when absent' })),
        collection: Schema.String.annotate({ description: 'Collection field of the owner (pages, paragraphStyles, rectangles, nestedGrepStyles, tabStops, swatches)' }),
        arguments: Schema.OptionFromOptionalKey(Schema.Array(Schema.Json).annotate({ description: "Positional arguments of the collection's add method, decoded by parameter type; inspect the collection class for its signature" })),
        properties: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: 'Fields of the new object, decoded as change decodes them' })),
    },
    { description: "Adds a member to a collection through its native add method in one undo step and returns the new object's reference" },
) {
    static readonly Own = Schema.Struct(Reference.fields);
    static readonly Result = Mutation.summarized(Create.Own);

    override get label(): string {
        return `Create ${this.collection} in ${Option.match(this.owner, { onNone: () => 'the active document', onSome: leaf })}`;
    }

    run() {
        const { owner: given, collection: field, arguments: positional, properties } = this;
        return this.commit(
            Create.Own,
            Effect.gen(function* () {
                const model = yield* Model;
                const found = yield* Option.match(given, { onNone: () => active, onSome: within });
                const { fields } = yield* Cache.get(model.rows, found.constructorName);
                const items = yield* Effect.fromOption(Option.liftPredicate(Reflect.get(found, field), model.collection), () => Unmatched.make({ subject: `collection of ${found.toSpecifier()}`, query: Option.some(field), candidates: Record.keys(Record.filter(fields, model.plural)) }));
                const { methods } = yield* Cache.get(model.rows, items.constructor.name);
                const add = yield* Effect.fromOption(Record.get(methods, 'add'), () => Unmatched.make({ subject: `method of ${items.constructor.name}`, query: Option.some('add'), candidates: Record.keys(methods) }));
                const ordered = Option.getOrElse(positional, Array.empty);
                yield* ordered.length > add.parameters.length ? new Cause.IllegalArgumentError(`${items.constructor.name} add takes ${add.parameters.length} parameters (${Array.map(add.parameters, Struct.get('name')).join(', ')}), given ${ordered.length}`) : Effect.void;
                const positioned = yield* Effect.forEach(Array.zip(ordered, add.parameters), ([value, parameter]) => decoded(found, Option.some(parameter))(value));
                const made = yield* Effect.try({ try: (): unknown => Reflect.apply(items.add, items, positioned), catch: (cause) => new Cause.UnknownError(cause, `${items.everyItem().toSpecifier()} add`) }).pipe(
                    Effect.flatMap((result) => Option.match(Array.head(Array.ensure<unknown>(result).filter(model.host)), { onNone: () => Effect.die(result), onSome: Effect.succeed })),
                );
                yield* Effect.forEach(Option.toArray(properties), (supplied) => write(made, supplied), { discard: true });
                return Reference.of(made);
            }),
        );
    }
}
class Remove extends Mutation.extend<Remove>('Remove')({ target: Specifier }, { description: "Removes one object through its native remove method in one undo step; host-owned members (the None, Registration, Paper, and Black swatches, [Basic Paragraph]) answer with InDesign's refusal" }) {
    static readonly Own = Schema.Struct(Reference.fields);
    static readonly Result = Mutation.summarized(Remove.Own);

    override get label(): string {
        return `Remove ${leaf(this.target)}`;
    }

    run() {
        const { target } = this;
        return this.commit(
            Remove.Own,
            Effect.gen(function* () {
                const found = yield* within(target);
                const { methods } = yield* Effect.flatMap(Model, ({ rows }) => Cache.get(rows, found.constructorName));
                const remove = yield* Record.get(methods, 'remove').pipe(
                    Option.flatMap(() => Schema.decodeUnknownOption(Schema.declare(Predicate.isFunction))(Reflect.get(found, 'remove'))),
                    Effect.fromOption(() => Unmatched.make({ subject: `method of ${found.constructorName}`, query: Option.some('remove'), candidates: Record.keys(methods) })),
                );
                const removed = Reference.of(found);
                yield* Effect.try({ try: () => Reflect.apply(remove, found, []), catch: (cause) => new Cause.UnknownError(cause, target) });
                return removed;
            }),
        );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Change, Create, Remove, write };
