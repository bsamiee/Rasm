import { HostError } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Uxp } from '@rasm/uxp/runtime';
import { Array, Cache, Effect, flow, identity, Match, Option, Predicate, pipe, Record, Result, Schema, Struct } from 'effect';
import { Model } from '#/dom/model';
import { reference, Specifier } from '#/dom/specifier';

// --- [TYPES] ---------------------------------------------------------------------------

type Value = typeof Enumerated.Type | typeof Located.Type | string | number | boolean | null | readonly Value[] | Values;
type Encoded = typeof Enumerated.Encoded | typeof Located.Encoded | string | number | boolean | null | readonly Encoded[] | { readonly [field: string]: Encoded };
interface Values {
    readonly [field: string]: Value;
}

// --- [MODELS] --------------------------------------------------------------------------

const Enumerated = Schema.Struct({ type: Schema.String, member: Schema.String });
const Located = Schema.Struct({ type: Schema.String, specifier: Specifier.Path });
const Value: Schema.Codec<Value, Encoded> = Schema.Union([
    Enumerated,
    Located,
    Schema.String,
    Schema.Finite,
    Schema.Boolean,
    Schema.Null,
    Schema.Array(Schema.suspend(() => Value)),
    Schema.Record(
        Schema.String,
        Schema.suspend(() => Value),
    ),
]).annotate({
    description: 'Name for an enumerator or named object, {type, member} for an enumerator, {type, specifier} for another object, points or unit string for a measurement, record for nested preferences',
});
const Values = Schema.Record(Schema.String, Value);
class Slice extends Schema.Class<Slice, { readonly brand: unique symbol }>('Slice')({
    offset: Schema.Int.check(Schema.isGreaterThanOrEqualTo(0))
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed(0)))
        .annotate({ description: 'Items skipped' }),
    limit: Schema.OptionFromOptionalKey(Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Items returned, absent returns the rest' })),
}) {
    static readonly field = Slice.pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));

    of<A>(items: readonly A[]): readonly A[] {
        return items.slice(this.offset, Option.getOrUndefined(Option.map(this.limit, (limit) => this.offset + limit)));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ENCODING]

const record = (value: unknown): value is Record.ReadonlyRecord<string, unknown> => Predicate.isObject(value) && Object.getPrototypeOf(value) === Object.prototype;
const picked =
    <F extends Schema.Struct.Fields>(fields: F) =>
    (object: object): Record.ReadonlyRecord<string, unknown> =>
        Record.fromIterableWith(
            Struct.keys(fields).filter((key) => key in object),
            (key) => [key, Reflect.get(object, key)],
        );
const encoder = Effect.map(
    Model,
    ({ enumerator, collection, host }) =>
        (value: unknown): string | undefined =>
            JSON.stringify(value, function (this: object, key: string, part: unknown) {
                const held: unknown = Reflect.get(this, key);
                return Match.value(held).pipe(
                    Match.when(Option.isOption, Option.getOrUndefined),
                    Match.when(Result.isResult, Result.match({ onSuccess: (success) => ({ _tag: 'Success', success: Option.isOption(success) ? Option.getOrNull(success) : success }), onFailure: (failure) => ({ _tag: 'Failure', failure }) })),
                    Match.orElse(() => part),
                    Match.value,
                    Match.when(enumerator, String),
                    Match.when(collection, (items) => ({ type: items.constructor.name, specifier: items.everyItem().toSpecifier(), length: items.length })),
                    Match.when(host, reference),
                    Match.when(Predicate.isPromise, () => {
                        throw new TypeError(`${key} holds a promise, scripts answer before it settles`);
                    }),
                    Match.when(record, identity),
                    Match.when(Schema.is(Uxp.Entry), ({ url }) => String(url)),
                    Match.when(Predicate.isError, flow(Schema.encodeOption(Schema.Defect()), Option.getOrUndefined)),
                    Match.when(Predicate.isMap, Object.fromEntries),
                    Match.when(Predicate.isSet, (entries) => [...entries]),
                    Match.when(Predicate.isBigInt, String),
                    Match.orElse(identity),
                );
            }),
);
const read = <S extends Schema.Top>(schema: S, subject: string, value: () => unknown) =>
    encoder.pipe(
        Effect.flatMap((encoded) => Effect.try({ try: () => encoded(value()), catch: HostError.refused(subject) })),
        Effect.flatMap(flow(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.toCodecJson(schema))), Effect.orDie)),
    );

// --- [LOOKUP]

const member = Effect.fnUntraced(function* (kind: string, name: string) {
    const { enumeration, enumerator, members, enumerations } = yield* Model;
    const found = yield* Effect.fromOption(enumeration(kind), () => Unmatched.make({ subject: 'enumeration', query: Option.some(kind), candidates: enumerations }));
    return yield* Effect.fromOption(Option.liftPredicate(Reflect.get(found, name), enumerator), () => Unmatched.make({ subject: `${kind} member`, query: Option.some(name), candidates: Array.map(members(found), String) }));
});
const byName = Effect.fnUntraced(function* (type: string, name: string, scope: Option.Option<object>) {
    const model = yield* Model;
    const { fields } = yield* Cache.get(model.rows, type);
    const listing = Record.has(fields, 'name') ? yield* Cache.get(model.catalog, type) : Option.none();
    return yield* Option.match(Option.all([scope, listing]), {
        onNone: () => Effect.succeed<unknown>(name),
        onSome: ([document, field]) => {
            const listed: unknown = Reflect.get(document, field);
            return Unmatched.single((model.collection(listed) ? listed.everyItem().getElements() : Array.ensure(listed)).filter(model.named), type, Option.some(name), (each) => [each.toSpecifier(), each.name]);
        },
    });
});
const fontByName = Effect.fn('fontByName')(function* (name: string) {
    const { app, FontStatus } = yield* Model;
    const every = app.fonts.everyItem();
    const faces = Array.map(Array.zip(Array.zip(every.name, every.postscriptName), Array.zip(every.fullName, Array.zip(every.fontFamily, every.fontStyleName))), ([[label, postscript], [full, [family, style]]], index) => ({ label, postscript, full, family, style, index }));
    const families = Record.map(
        Array.groupBy(faces, Struct.get('family')),
        (listed) =>
            Option.getOrElse(
                Array.findFirst(listed, ({ style }) => style === 'Regular'),
                () => Array.headNonEmpty(listed),
            ).index,
    );
    const installed = pipe(
        faces,
        Array.map(({ label, postscript, full, family, index }) => ({ postscript, names: [label, postscript, full, ...(Option.contains(Record.get(families, family), index) ? [family] : [])], font: () => app.fonts.item(index) })),
        Array.dedupeWith((a, b) => a.postscript === b.postscript),
    );
    const missing = Array.flatMap(app.documents.everyItem().getElements(), (open) => {
        const used = open.fonts.everyItem();
        return Array.filterMap(Array.zip(used.name, used.status), ([label, status], index) => (FontStatus.INSTALLED.equals(status) ? Result.failVoid : Result.succeed({ names: [label], font: () => open.fonts.item(index) })));
    });
    return (yield* Unmatched.single([...installed, ...missing], 'font', Option.some(name), Struct.get('names'))).font();
});

// --- [DECODING]

const decoder = (target: unknown, row: Option.Option<{ readonly types: readonly string[] }>): ((value: unknown) => Effect.Effect<unknown, HostError | Unmatched | Schema.SchemaError, Model>) => {
    const typed = (text: string) =>
        Effect.flatMap(Effect.all([Model, Effect.option(Model.document)]), ([{ enumeration, classes, owner }, open]) => {
            const types = Option.match(row, { onNone: Array.empty<string>, onSome: Struct.get('types') });
            const scope = Option.orElse(owner(target), () => open);
            const named = types.includes('String')
                ? [Effect.succeed<unknown>(text)]
                : Array.map(
                      types.filter((type) => type === 'Font' || classes.includes(type)),
                      (type) => (type === 'Font' ? fontByName(text) : byName(type, text, scope)),
                  );
            return Effect.firstSuccessOf([...Array.map(types.filter(flow(enumeration, Option.isSome)), (type) => member(type, text)), ...named, ...(Array.isReadonlyArrayNonEmpty(named) ? [] : [Effect.succeed<unknown>(text)])]);
        });
    const decode: (value: unknown) => Effect.Effect<unknown, HostError | Unmatched | Schema.SchemaError, Model> = Match.type<unknown>().pipe(
        Match.when(Schema.is(Located), ({ specifier }) => Specifier.resolve(specifier)),
        Match.when(Schema.is(Enumerated), ({ type, member: name }) => member(type, name)),
        Match.when(Predicate.isString, typed),
        Match.when(Array.isArray, (items) => Effect.forEach(items, decode)),
        Match.when(record, (properties) => Effect.all(Record.map(properties, decoder(target, Option.none())))),
        Match.orElse(Effect.succeed),
    );
    return decode;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { byName, decoder, encoder, fontByName, picked, read, record, Slice, Value, Values };
