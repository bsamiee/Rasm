import type { InDesignDOMObject } from 'adobe:indesign';
import type { Entry } from 'adobe:uxp';
import { single, Unmatched } from '@rasm/search';
import { Array, Cache, Effect, flow, Iterable, identity, Match, Option, Predicate, Record, Schema, Struct } from 'effect';
import { Model } from './model.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SELECTOR = /\[(?:"[^"]*"|[^\]"])*\]$/u;
const INDEXED = /\[-?\d+\]$/u;
const NAMED = /\[@name="(?<name>[^"]*)"\]$/u;
const IDENTIFIED = /(?<kind>[\w-]+)\[@id=\d+\]$/u;
const LAST_STEP = /\/+[^/]+$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Specifier = Schema.String.annotate({ description: "InDesign's path to an object, as results return it" });
class Reference extends Schema.Class<Reference, { readonly brand: unique symbol }>('Reference')({ type: Schema.String, specifier: Specifier, name: Schema.OptionFromOptionalKey(Schema.String), valid: Schema.Boolean }) {
    static readonly of = (object: InDesignDOMObject<unknown>): Reference => {
        const valid = object.isValid;
        return Reference.make({ type: object.constructorName, specifier: object.toSpecifier(), name: valid && 'name' in object ? Schema.decodeUnknownOption(Schema.String)(object.name) : Option.none(), valid });
    };

    get hostOwned(): boolean {
        return Option.exists(this.name, (name) => name.startsWith('[') && name.endsWith(']'));
    }
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
                return Match.value(Option.isOption(held) ? Option.getOrUndefined(held) : part).pipe(
                    Match.when(Match.instanceOf(Enumerator), String),
                    Match.when(collection, (items) => ({ type: items.constructor.name, specifier: items.everyItem().toSpecifier(), length: items.length })),
                    Match.when(host, Reference.of),
                    Match.when(
                        (entry: unknown): entry is Entry => !record(entry) && Predicate.hasProperty(entry, 'isEntry') && entry.isEntry === true,
                        (entry) => String(entry.url),
                    ),
                    Match.when(Predicate.isError, flow(Schema.encodeOption(Schema.Defect()), Option.getOrUndefined)),
                    Match.when(Predicate.isMap, Object.fromEntries),
                    Match.when(Predicate.isSet, (entries) => [...entries]),
                    Match.when(Predicate.isBigInt, String),
                    Match.orElse(identity),
                );
            }),
);
const read = <S extends Schema.Top>(schema: S, value: () => unknown) =>
    Effect.flatMap(encoder, (encoded) =>
        Effect.flatMap(
            Effect.try(() => encoded(value())),
            flow(Schema.decodeUnknownEffect(Schema.fromJsonString(schema)), Effect.orDie),
        ),
    );

// --- [LOCATIONS]
const ancestors = (object: unknown) => Effect.map(Model, ({ host }) => Array.fromIterable(Iterable.unfold(object, (above: unknown) => (host(above) && above.constructorName !== 'Application' ? Option.some([above, above.parent] as const) : Option.none()))));
const owner = (object: unknown) => Effect.flatMap(Model, ({ document }) => Effect.map(ancestors(object), Array.findFirst(document)));
const active = Effect.flatMap(Model, ({ app }) => Effect.try({ try: () => app.activeDocument, catch: () => Unmatched.make({ subject: 'active document', query: '', candidates: app.documents.everyItem().name }) }));
const resolve = Effect.fn('resolve')(function* (specifier: string) {
    const model = yield* Model;
    const kind = (path: string): string | undefined => IDENTIFIED.exec(path)?.groups?.kind;
    const located = (path: string): Option.Option<InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[]> => {
        const requested = kind(path);
        return Option.filter(Option.liftThrowable((): unknown => model.app.doScript('resolve(arguments[0])', model.ScriptLanguage.JAVASCRIPT, [path]))(), (found): found is InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[] =>
            Array.ensure<unknown>(found).every((each) => model.host(each) && (requested === undefined || kind(each.toSpecifier()) === requested)),
        );
    };
    const listed = (path: string): readonly InDesignDOMObject<unknown>[] => Array.flatMap(Option.toArray(located(path)), Array.ensure);
    const direct = located(specifier);
    if (Option.isSome(direct)) {
        return direct.value;
    }
    const siblings = listed(specifier.replace(SELECTOR, ''));
    const container = Array.head(listed(specifier.replace(LAST_STEP, '') || '/'));
    const fields = yield* Effect.map(Effect.transposeOption(Option.map(container, (at) => Cache.get(model.rows, at.constructorName))), Option.match({ onNone: Array.empty, onSome: ({ fields: rows }) => Record.toEntries(rows) }));
    const value = (field: string): unknown => Option.getOrUndefined(Option.map(container, (at) => Reflect.get(at, field)));
    const sibling = Option.map(Array.head(siblings), Struct.get('constructorName'));
    const related = fields.flatMap(([field, { type, array }]) => (Option.isSome(array) && Option.isSome(sibling) && Option.contains(type, sibling.value) ? Array.ensure<unknown>(value(field)) : [])).filter(model.named);
    const name = NAMED.exec(specifier)?.groups?.name;
    return yield* Effect.fromOption(
        Option.map(
            Array.findFirst(fields.flatMap(([field, row]) => (model.plural(row) ? [value(field)] : [])).filter(model.collection), (items) => items.everyItem().toSpecifier() === specifier),
            (items) => items.everyItem().getElements(),
        ),
        () =>
            INDEXED.test(specifier)
                ? Unmatched.make({ subject: Array.isReadonlyArrayNonEmpty(siblings) ? `object among indexes 0 to ${siblings.length - 1}` : 'object', query: specifier, candidates: [] })
                : Unmatched.make({ subject: 'object', query: specifier, candidates: Array.dedupe(Array.map([...siblings, ...related.filter((each) => each.name === name)], (each) => each.toSpecifier())) }),
    );
});
const font = Effect.fn('font')(function* (name: string) {
    const { app, FontStatus } = yield* Model;
    const every = app.fonts.everyItem();
    const installed = Array.map(Array.zip(Array.zip(every.name, every.postscriptName), Array.zip(every.fullName, Array.zip(every.fontFamily, every.fontStyleName))), ([[label, postscript], [full, [family, style]]], index) => ({
        label,
        names: [label, postscript, full, ...(style === 'Regular' ? [family] : [])],
        font: () => app.fonts.item(index),
    }));
    const missing = Array.flatMap(Option.toArray(yield* Effect.option(active)), (open) => {
        const used = open.fonts.everyItem();
        return Array.flatMap(Array.zip(used.name, used.status), ([label, status], index) => (FontStatus.INSTALLED.equals(status) ? [] : [{ label, names: [label], font: () => open.fonts.item(index) }]));
    });
    return (yield* single('font', name)([...installed, ...missing], { names: Struct.get('names'), label: Struct.get('label') })).font();
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { active, ancestors, encoder, font, owner, Reference, read, record, resolve, Specifier };
