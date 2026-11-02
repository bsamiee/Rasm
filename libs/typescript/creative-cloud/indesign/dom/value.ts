import { Unmatched } from '@rasm/search/unmatched';
import { Uxp } from '@rasm/uxp/runtime';
import { Array, Cache, Effect, flow, identity, Match, Option, Predicate, pipe, Record, Result, Schema, Struct } from 'effect';
import type { Member } from '#/dom/dictionary';
import { Model } from '#/dom/model';
import { Reference, Specifier } from '#/dom/specifier';
import { InDesignError } from '#/error';

// --- [MODELS] --------------------------------------------------------------------------

const Value = Schema.Json.annotate({ description: 'Name for an enumerator or named object, {type, member} for an enumerator, {type, specifier} for another object, points or unit string for a measurement, record for nested preferences' });
const Values = Schema.Record(Schema.String, Value);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ENCODING]

const record = (value: unknown): value is Record.ReadonlyRecord<string, unknown> => Predicate.isObject(value) && Object.getPrototypeOf(value) === Object.prototype;
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
                    Match.when(host, Reference.of),
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
        Effect.flatMap((encoded) => Effect.try({ try: () => encoded(value()), catch: InDesignError.refused(subject) })),
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
    const [{ fields }, parents] = yield* Effect.all([Cache.get(model.rows, type), model.lineage(type)]);
    const listing = Record.has(fields, 'name') && ![type, ...parents].includes('Swatch') ? yield* Cache.get(model.catalog, type) : Option.none();
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
    const installed = pipe(
        Array.zip(Array.zip(every.name, every.postscriptName), Array.zip(every.fullName, Array.zip(every.fontFamily, every.fontStyleName))),
        Array.map(([[label, postscript], [full, [family, style]]], index) => ({ postscript, names: [label, postscript, full, ...(style === 'Regular' ? [family] : [])] as const, font: () => app.fonts.item(index) })),
        Array.dedupeWith((a, b) => a.postscript === b.postscript),
    );
    const missing = Array.flatMap(Option.toArray(yield* Effect.option(Model.active)), (open) => {
        const used = open.fonts.everyItem();
        return Array.flatMap(Array.zip(used.name, used.status), ([label, status], index) => (FontStatus.INSTALLED.equals(status) ? [] : [{ names: [label] as const, font: () => open.fonts.item(index) }]));
    });
    return (yield* Unmatched.single([...installed, ...missing], 'font', Option.some(name), Struct.get('names'))).font();
});

// --- [DECODING]

const decoder = (target: unknown, row: Option.Option<Pick<typeof Member.Type, 'types'>>): ((value: unknown) => Effect.Effect<unknown, InDesignError | Unmatched | Schema.SchemaError, Model>) => {
    const typed = (text: string) =>
        Effect.flatMap(Effect.all([Model, Effect.option(Model.active)]), ([{ enumeration, classes, owner }, open]) => {
            const scope = Option.orElse(owner(target), () => open);
            return Match.value(Option.match(row, { onNone: Array.empty, onSome: Struct.get('types') })).pipe(
                Match.when(Schema.is(Schema.Tuple([Schema.String])), ([type]) =>
                    Match.value(type).pipe(
                        Match.when(flow(enumeration, Option.isSome), (each) => member(each, text)),
                        Match.when('Font', () => fontByName(text)),
                        Match.when(
                            (each) => classes.includes(each),
                            (each) => byName(each, text, scope),
                        ),
                        Match.orElse(() => Effect.succeed<unknown>(text)),
                    ),
                ),
                Match.orElse((types) => Effect.firstSuccessOf([...types.filter(flow(enumeration, Option.isSome)).map((each) => member(each, text)), Effect.succeed<unknown>(text)])),
            );
        });
    const decode: (value: unknown) => Effect.Effect<unknown, InDesignError | Unmatched | Schema.SchemaError, Model> = Match.type<unknown>().pipe(
        Match.when(Schema.is(Schema.Struct({ type: Schema.String, specifier: Schema.String })), ({ specifier }) => Effect.flatMap(Schema.decodeUnknownEffect(Specifier.Path)(specifier), Specifier.resolve)),
        Match.when(Schema.is(Schema.Struct({ type: Schema.String, member: Schema.String })), ({ type, member: name }) => member(type, name)),
        Match.when(Predicate.isString, typed),
        Match.when(Array.isArray, (items) => Effect.forEach(items, decode)),
        Match.when(record, (properties) => Effect.all(Record.map(properties, decoder(target, Option.none())))),
        Match.orElse(Effect.succeed),
    );
    return decode;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { byName, decoder, encoder, fontByName, read, record, Value, Values };
