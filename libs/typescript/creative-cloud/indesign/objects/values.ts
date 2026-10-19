import type { Document, Enumerator, InDesignDOMObject } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cache, Cause, Effect, flow, Match, Option, Predicate, Record, Schema, Struct } from 'effect';
import { Host } from '../host.ts';
import { Classes, type Member, record } from './classes.ts';
import { active, leaf, Reference, read, resolve, Specifier } from './specifiers.ts';
import { Mutation } from './transactions.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DECODING]
const member = Effect.fnUntraced(function* (kind: string, name: string) {
    const [{ indesign }, { enumeration, members, kinds }] = yield* Effect.all([Host, Classes]);
    const found = yield* Effect.fromOption(enumeration(kind), () => Unmatched.make({ subject: 'enumeration', query: kind, candidates: kinds.filter((each) => Option.isSome(enumeration(each))) }));
    return yield* Effect.fromOption(
        Option.liftPredicate(Reflect.get(found, name), (value): value is Enumerator => value instanceof indesign.Enumerator),
        () => Unmatched.make({ subject: `${kind} member`, query: name, candidates: Array.map(members(found), String) }),
    );
});
const catalogued = Effect.fn('catalogued')(function* (scope: Document, kind: string) {
    const { rows: models, element, collection, host } = yield* Classes;
    const rows = yield* Cache.get(models, 'Document').pipe(Effect.map(flow(Struct.get('fields'), Record.toEntries)));
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
const decoded = (target: unknown, declared: Option.Option<Pick<typeof Member.Type, 'type'>>): ((value: unknown) => Effect.Effect<unknown, Cause.UnknownError | Unmatched, Classes | Host>) => {
    const kind = Option.flatMap(declared, Struct.get('type'));
    const byName: (type: string, name: string) => Effect.Effect<unknown, Cause.UnknownError | Unmatched, Classes | Host> = Effect.fnUntraced(function* (type: string, name: string) {
        const [{ indesign }, { rows: models, lineage, named, owner }] = yield* Effect.all([Host, Classes]);
        const [rows, parents] = yield* Effect.all([Cache.get(models, type), lineage(type)]);
        const scope = Option.orElse(owner(target), () => Option.liftThrowable(() => indesign.app.activeDocument)());
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
        Effect.flatMap(Host, ({ indesign }) =>
            Effect.filterOrFail(
                Effect.try(() => indesign.app.fonts.itemByName(name)),
                (found) => !found.status.equals(indesign.FontStatus.NOT_AVAILABLE),
                () => Unmatched.make({ subject: 'installed font', query: name, candidates: [] }),
            ),
        );
    const typed = (text: string, type: string) =>
        Effect.flatMap(Classes, ({ enumeration, exported }) =>
            Match.value(type).pipe(
                Match.when(flow(enumeration, Option.isSome), (each) => member(each, text)),
                Match.when('Font', () => font(text)),
                Match.when(
                    (each) => exported.some(([name, value]) => name === each && Predicate.isFunction(value)),
                    (each) => byName(each, text),
                ),
                Match.orElse(() => Effect.succeed<unknown>(text)),
            ),
        );
    const decode: (value: unknown) => Effect.Effect<unknown, Cause.UnknownError | Unmatched, Classes | Host> = Match.type<unknown>().pipe(
        Match.when(Schema.is(Schema.Struct({ type: Schema.String, specifier: Schema.String })), ({ specifier }) => resolve(specifier)),
        Match.when(Schema.is(Schema.Struct({ type: Schema.String, member: Schema.String })), ({ type, member: name }) => member(type, name)),
        Match.when(Predicate.isString, (text) => Option.match(kind, { onNone: () => Effect.succeed(text), onSome: (type) => typed(text, type) })),
        Match.when(Array.isArray, (items) => Effect.forEach(items, decode)),
        Match.when(record, (properties) => Effect.all(Record.map(properties, decoded(target, Option.none())))),
        Match.orElse(Effect.succeed),
    );
    return decode;
};

// --- [WRITES]
const preserve = <T extends object>(target: T, replacements: Partial<T>) =>
    Effect.acquireRelease(
        Effect.try(() => Record.fromIterableWith(Struct.keys(replacements), (key) => [key, target[key]])),
        (saved) => Effect.orDie(Effect.try(() => Object.assign(target, saved))),
    ).pipe(Effect.tap(() => Effect.try(() => Object.assign(target, replacements))));
const write: (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>) => Effect.Effect<readonly string[], Cause.UnknownError | Unmatched, Classes | Host> = Effect.fnUntraced(function* (target, values) {
    const specifier = target.toSpecifier();
    const { rows, unavailable, host } = yield* Classes;
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
                return yield* write(inner, value);
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
const within = (specifier: string) => Effect.flatMap(Classes, ({ host }) => Effect.filterOrFail(resolve(specifier), host, () => Unmatched.make({ subject: 'object', query: specifier, candidates: [] })));
class Change extends Mutation.extend<Change>('Change')(
    {
        target: Specifier,
        values: Schema.JsonObject.annotate({ description: 'Fields to write by name. Enumeration members by name, named objects (styles, layers, parents, fonts) by name or {type, specifier}, swatches by name, measurements as points or unit strings; a record writes into a nested preference object' }),
    },
    { description: "Writes fields of one object through InDesign's own setters in one undo step, decoding each value by the field's declared type; every refused field is reported" },
) {
    static readonly Result = Mutation.Result({ ...Reference.fields, written: Schema.Array(Schema.String) });

    override get label(): string {
        return `Change ${Record.keys(this.values).join(', ')} of ${leaf(this.target)}`;
    }

    run() {
        const { target, values } = this;
        return this.commit(
            Change.Result,
            Effect.gen(function* () {
                const { reference } = yield* Classes;
                const found = yield* within(target);
                return { ...reference(found), written: yield* write(found, values) };
            }),
        );
    }
}
class Create extends Mutation.extend<Create>('Create')(
    {
        owner: Schema.OptionFromOptionalKey(Specifier.annotate({ identifier: undefined, description: 'Specifier of the object owning the collection; the active document when absent' })),
        collection: Schema.String.annotate({ description: 'Collection field of the owner (pages, paragraphStyles, rectangles, nestedGrepStyles, tabStops, swatches)' }),
        arguments: Schema.OptionFromOptionalKey(Schema.Array(Schema.Json).annotate({ description: "Positional arguments of the collection's add method, decoded by parameter type; inspect the collection class for its signature" })),
        properties: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: 'Fields of the new object, decoded as change decodes them' })),
    },
    { description: "Adds a member to a collection through its native add method in one undo step and returns the new object's reference" },
) {
    static readonly Result = Mutation.Result(Reference.fields);

    override get label(): string {
        return `Create ${this.collection} in ${Option.match(this.owner, { onNone: () => 'the active document', onSome: leaf })}`;
    }

    run() {
        const { owner: given, collection: field, arguments: positional, properties } = this;
        return this.commit(
            Create.Result,
            Effect.gen(function* () {
                const classes = yield* Classes;
                const found = yield* Option.match(given, { onNone: () => active, onSome: within });
                const { fields } = yield* Cache.get(classes.rows, found.constructorName);
                const collections = Record.keys(Record.filter(fields, classes.plural));
                const items = yield* Effect.fromOption(Option.liftPredicate(Reflect.get(found, field), classes.collection), () => Unmatched.make({ subject: `collection of ${found.toSpecifier()}`, query: field, candidates: collections }));
                const [{ methods }, held] = yield* Effect.all([Cache.get(classes.rows, items.constructor.name), classes.element(items.constructor.name)]);
                const add = yield* Effect.fromOption(Record.get(methods, 'add'), () => Unmatched.make({ subject: `method of ${items.constructor.name}`, query: 'add', candidates: Record.keys(methods) }));
                const rows = yield* Effect.transposeOption(Option.map(held, (kind) => Cache.get(classes.rows, kind))).pipe(Effect.map(Option.map(Struct.get('fields'))));
                const values = yield* Effect.transposeOption(Option.map(properties, (supplied) => Effect.all(Record.map(supplied, (value, name) => decoded(found, Option.flatMap(rows, Record.get(name)))(value)))));
                const slot = Array.findFirstIndex(add.parameters, ({ name }) => name === 'withProperties');
                const positioned = yield* Effect.forEach(Array.zip(Option.getOrElse(positional, Array.empty), add.parameters), ([value, parameter]) => decoded(found, Option.some(parameter))(value));
                const placed = Option.match(Option.all({ slot, values }), { onNone: () => positioned, onSome: ({ slot: at, values: supplied }) => Array.append(Array.pad(Array.take(positioned, at), at, undefined), supplied) });
                const refused = (cause: unknown) => new Cause.UnknownError(cause, `${items.everyItem().toSpecifier()} add`);
                const made = yield* Effect.try({ try: (): unknown => Reflect.apply(items.add, items, placed), catch: refused }).pipe(Effect.flatMap((result) => Effect.fromOption(Array.head(Array.ensure<unknown>(result).filter(classes.host)), () => refused(result))));
                yield* Option.isSome(slot) ? Effect.void : Effect.forEach(Option.toArray(properties), (supplied) => write(made, supplied), { discard: true });
                return classes.reference(made);
            }),
        );
    }
}
class Remove extends Mutation.extend<Remove>('Remove')({ target: Specifier }, { description: "Removes one object through its native remove method in one undo step; host-owned members ([None], [Paper], [Basic Paragraph]) answer with InDesign's refusal" }) {
    static readonly Result = Mutation.Result(Reference.fields);

    override get label(): string {
        return `Remove ${leaf(this.target)}`;
    }

    run() {
        const { target } = this;
        return this.commit(
            Remove.Result,
            Effect.gen(function* () {
                const { reference } = yield* Classes;
                const found = yield* within(target);
                const removed = yield* read(Remove.Result.mapFields(Struct.omit(['preflight'])), () => reference(found));
                const remover = yield* Effect.fromOption(
                    Option.flatMap(Option.liftPredicate(found, Predicate.hasProperty('remove')), (removable) => Option.liftPredicate(removable.remove, Predicate.isFunction)),
                    () => Unmatched.make({ subject: `method of ${target}`, query: 'remove', candidates: [] }),
                );
                yield* Effect.try({ try: () => Reflect.apply(remover, found, []), catch: (cause) => new Cause.UnknownError(cause, target) });
                return removed;
            }),
        );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Change, Create, preserve, Remove, write };
