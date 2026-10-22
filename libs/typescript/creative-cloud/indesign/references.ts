import type { InDesignDOMObject } from 'adobe:indesign';
import type { Entry } from 'adobe:uxp';
import { Unmatched } from '@rasm/search';
import { Array, Cache, Cause, Effect, Equal, flow, Iterable, identity, Match, Option, Predicate, pipe, Record, Result, Schema, Struct } from 'effect';
import { Model } from './model.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SELECTOR = /\[(?:"[^"]*"|[^\]"])*\]$/u;
const INDEXED = /\[-?\d+\]$/u;
const NAMED = /\[@name="(?<name>[^"]*)"\]$/u;
const IDENTIFIED = /(?<kind>[\w-]+)\[@id=\d+\]$/u;
const LAST_STEP = /\/+[^/]+$/u;
const COLLECTION = /\/[\w-]+$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Specifier = Schema.String.annotate({ description: "InDesign's path to an object, as results return it" });
class Reference extends Schema.Class<Reference, { readonly brand: unique symbol }>('Reference')({ type: Schema.String, specifier: Specifier, name: Schema.OptionFromOptionalKey(Schema.String), valid: Schema.Boolean }) {
    static readonly of = (object: InDesignDOMObject<unknown>): Reference => {
        const valid = object.isValid;
        return Reference.make({ type: object.constructorName, specifier: object.toSpecifier(), name: valid && 'name' in object ? Option.liftPredicate(object.name, Predicate.isString) : Option.none(), valid });
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CODEC]
const record = (value: unknown): value is Record.ReadonlyRecord<string, unknown> => Predicate.isObject(value) && Object.getPrototypeOf(value) === Object.prototype;
const encoder = Effect.map(
    Model,
    ({ Enumerator, host, collection }) =>
        (value: unknown): string | undefined =>
            JSON.stringify(value, function (this: object, key: string, part: unknown) {
                const held: unknown = Reflect.get(this, key);
                return Match.value(held).pipe(
                    Match.when(Option.isOption, Option.getOrUndefined),
                    Match.when(Result.isResult, Result.match({ onSuccess: (success) => ({ _tag: 'Success', success: Option.isOption(success) ? Option.getOrNull(success) : success }), onFailure: (failure) => ({ _tag: 'Failure', failure }) })),
                    Match.orElse(() => part),
                    Match.value,
                    Match.when(Match.instanceOf(Enumerator), String),
                    Match.when(collection, (items) => ({ type: items.constructor.name, specifier: items.everyItem().toSpecifier(), length: items.length })),
                    Match.when(host, Reference.of),
                    Match.when(
                        (entry: unknown): entry is Entry => !record(entry) && Predicate.hasProperty(entry, 'isEntry') && entry.isEntry === true,
                        (entry) => String(entry.url),
                    ),
                    Match.when(Predicate.isPromise, () => {
                        throw new TypeError(`${key} holds a promise, which settles after a synchronous script answers`);
                    }),
                    Match.when(Predicate.isError, flow(Schema.encodeOption(Schema.Defect()), Option.getOrUndefined)),
                    Match.when(Predicate.isMap, Object.fromEntries),
                    Match.when(Predicate.isSet, (entries) => [...entries]),
                    Match.when(Predicate.isBigInt, String),
                    Match.orElse(identity),
                );
            }),
);
const read = <S extends Schema.Top>(schema: S, at: string, value: () => unknown) =>
    Effect.flatMap(encoder, (encoded) => Effect.flatMap(Effect.try({ try: () => encoded(value()), catch: (cause) => new Cause.UnknownError(cause, at) }), flow(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.toCodecJson(schema))), Effect.orDie)));
const overrides = Effect.fnUntraced(function* (object: InDesignDOMObject<unknown>, own: object, base: (name: string) => unknown, except: Schema.Struct.Fields) {
    const [{ rows, plural }, encoded] = yield* Effect.all([Model, encoder]);
    const { fields } = yield* Cache.get(rows, object.constructorName);
    return Record.filterMap(fields, (row, name) => (row.settable && !plural(row) && !Object.hasOwn(except, name) && encoded(Reflect.get(own, name)) !== encoded(base(name)) ? Result.succeed<unknown>(Reflect.get(own, name)) : Result.failVoid));
});

// --- [LOCATIONS]
const ancestors = (object: unknown) => Model.useSync(({ host }) => Array.fromIterable(Iterable.unfold(object, (above: unknown) => (host(above) && above.constructorName !== 'Application' ? Option.some([above, above.parent] as const) : Option.none()))));
const owner = (object: unknown) => Model.use(({ document }) => Effect.map(ancestors(object), Array.findFirst(document)));
const active = Model.use(({ app }) => Effect.try({ try: () => app.activeDocument, catch: () => Unmatched.make({ subject: 'active document', query: Option.none(), candidates: app.documents.everyItem().name }) }));
const documents = Effect.map(Model, ({ app }) => app.documents.everyItem().getElements());
const resolve = Effect.fn('resolve')(function* (specifier: string) {
    const model = yield* Model;
    const kind = (path: string): string | undefined => IDENTIFIED.exec(path)?.groups?.kind;
    const located = (path: string): Option.Option<InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[]> => {
        const requested = kind(path);
        return Option.filter(Option.liftThrowable((): unknown => model.app.doScript('resolve(arguments[0])', model.ScriptLanguage.JAVASCRIPT, [path]))(), (found): found is InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[] =>
            Array.ensure<unknown>(found).every((each) => model.host(each) && each.isValid && (requested === undefined || kind(each.toSpecifier()) === requested)),
        );
    };
    const listed = (path: string): readonly InDesignDOMObject<unknown>[] => Array.flatMap(Option.toArray(located(path)), Array.ensure);
    return yield* Option.match(located(specifier), {
        onSome: (found) => Effect.succeed(COLLECTION.test(specifier) ? Array.ensure(found) : found),
        onNone: Effect.fnUntraced(function* () {
            const siblings = listed(specifier.replace(SELECTOR, ''));
            const container = Array.head(listed(specifier.replace(LAST_STEP, '') || '/'));
            const fields = yield* Effect.map(Effect.transposeOption(Option.map(container, (at) => Cache.get(model.rows, at.constructorName))), Option.match({ onNone: Array.empty, onSome: ({ fields: rows }) => Record.toEntries(rows) }));
            const value = (field: string): unknown => Option.getOrUndefined(Option.map(container, (at) => Reflect.get(at, field)));
            const sibling = Option.map(Array.head(siblings), Struct.get('constructorName'));
            const related = fields.flatMap(([field, { type, array }]) => (Option.isSome(array) && Option.isSome(sibling) && Equal.equals(type, sibling) ? Array.ensure<unknown>(value(field)) : [])).filter(model.named);
            const collections = fields
                .flatMap(([field, row]) => (model.plural(row) ? [value(field)] : []))
                .filter(model.collection)
                .flatMap((items) => Option.toArray(Option.fromUndefinedOr(items.everyItem())));
            const name = NAMED.exec(specifier)?.groups?.name;
            const candidates = Array.dedupe(Array.map([...siblings, ...related.filter((each) => each.name === name)], (each) => each.toSpecifier()));
            return yield* Effect.fromOption(
                Option.map(
                    Array.findFirst(collections, (every) => every.toSpecifier() === specifier),
                    (every) => every.getElements(),
                ),
                () => (INDEXED.test(specifier) ? Unmatched.make({ subject: Array.isReadonlyArrayNonEmpty(siblings) ? `object among indexes 0 to ${siblings.length - 1}` : 'object', query: Option.some(specifier), candidates: [] }) : Unmatched.make({ subject: 'object', query: Option.some(specifier), candidates })),
            );
        }),
    });
});
const byName = Effect.fnUntraced(function* (type: string, name: string, scope: Option.Option<object>) {
    const model = yield* Model;
    const [{ fields }, parents] = yield* Effect.all([Cache.get(model.rows, type), model.lineage(type)]);
    const listing = Record.has(fields, 'name') && ![type, ...parents].includes('Swatch') ? yield* Cache.get(model.catalog, type) : Option.none();
    return yield* Option.match(Option.all([scope, listing]), {
        onNone: () => Effect.succeed<unknown>(name),
        onSome: ([document, field]) => {
            const listed: unknown = Reflect.get(document, field);
            return Unmatched.single(type, Option.some(name))((model.collection(listed) ? listed.everyItem().getElements() : Array.ensure(listed)).filter(model.named), (each) => [each.toSpecifier(), each.name]);
        },
    });
});
const font = Effect.fn('font')(function* (name: string) {
    const { app, FontStatus } = yield* Model;
    const every = app.fonts.everyItem();
    const installed = pipe(
        Array.zip(Array.zip(every.name, every.postscriptName), Array.zip(every.fullName, Array.zip(every.fontFamily, every.fontStyleName))),
        Array.map(([[label, postscript], [full, [family, style]]], index) => ({ postscript, names: [label, postscript, full, ...(style === 'Regular' ? [family] : [])] as const, font: () => app.fonts.item(index) })),
        Array.dedupeWith((a, b) => a.postscript === b.postscript),
    );
    const missing = Array.flatMap(Option.toArray(yield* Effect.option(active)), (open) => {
        const used = open.fonts.everyItem();
        return Array.flatMap(Array.zip(used.name, used.status), ([label, status], index) => (FontStatus.INSTALLED.equals(status) ? [] : [{ names: [label] as const, font: () => open.fonts.item(index) }]));
    });
    return (yield* Unmatched.single('font', Option.some(name))([...installed, ...missing], Struct.get('names'))).font();
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { active, ancestors, byName, documents, font, overrides, owner, Reference, read, record, resolve, Specifier };
