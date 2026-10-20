import type { InDesignDOMObject } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import type { Uxp } from '@rasm/uxp/runtime';
import { Array, Cache, Cause, Effect, flow, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import { DocumentView, LayerView, PageView, SpreadView } from './documents.ts';
import { FontView } from './fonts.ts';
import { LinkView } from './links.ts';
import { Field, Member, Method, Model } from './model.ts';
import { Reference, read, resolve, Specifier } from './references.ts';
import { Session } from './session.ts';
import { StyleView } from './styles.ts';
import { SwatchView } from './swatches.ts';
import { StoryView, TextView } from './text.ts';

// --- [MODELS] --------------------------------------------------------------------------

const View = Schema.Union([DocumentView, PageView, SpreadView, LayerView, StyleView, SwatchView, LinkView, StoryView, TextView, FontView]);
const Row = Field.mapFields(Struct.omit(['unavailable']));
const Read = Schema.TaggedUnion({ Success: { ...Row.fields, value: Schema.Json }, Failure: { ...Row.fields, unavailable: Schema.Defect() } });
class Inspect extends Schema.Class<Inspect, { readonly brand: unique symbol }>('Inspect')(
    {
        target: Schema.OptionFromOptionalKey(Specifier.annotate({ description: 'Specifier of an object or collection; the session when absent' })),
        fields: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: 'Native fields to read; an object lists its collections and a collection its members unless fields are requested. Object values return specifiers to inspect further' })),
    },
    {
        description:
            "Reads InDesign's native object model without changing it. No target: the session, its documents, windows, selection, overset stories, and undo history. A specifier: a typed view of documents, pages, spreads, parents, layers, links, swatches, styles, stories, frames, text, and fonts, beside its native fields on request; other objects list fields, values, valid values, writable fields, and related collections. A collection: its members. Class and enumeration declarations are the kinds resource",
    },
) {
    static readonly Result = Schema.Union([
        Session,
        Schema.TaggedStruct('Object', {
            ...Reference.fields,
            view: Schema.OptionFromOptionalKey(View),
            fields: Schema.Array(Read),
            collections: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, count: Schema.Int, specifier: Specifier })),
        }),
        Schema.TaggedStruct('Items', { specifier: Specifier, total: Schema.Int, items: Schema.Array(Reference.mapFields(Struct.assign({ fields: Schema.Array(Read) }))) }),
    ]);

    run() {
        const { target, fields } = this;
        return Option.match(target, {
            onNone: () => Session.current(),
            onSome: Effect.fnUntraced(function* (specifier) {
                const [found, model] = yield* Effect.all([resolve(specifier), Model]);
                const raw = yield* model.host(found)
                    ? object(found, fields)
                    : Effect.map(
                          Effect.forEach(found, (item) => Effect.map(values(item, Option.getOrElse(fields, Array.empty)), (entries) => ({ ...Reference.of(item), fields: Array.map(entries, described) }))),
                          (items) => ({ _tag: 'Items', specifier, total: found.length, items }),
                      );
                return yield* read(Inspect.Result, () => raw);
            }),
        });
    }
}
class Kind extends Schema.Class<Kind, { readonly brand: unique symbol }>('Kind')(
    { name: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Class or enumeration name, in any case; absent lists every name' })) },
    { description: 'Native declarations of the host dictionary: every class and enumeration name, or one class with its lineage, fields, and methods, or one enumeration with the members change takes by name' },
) {
    static readonly Result = Schema.TaggedUnion({
        Index: { classes: Schema.Array(Schema.String), enumerations: Schema.Array(Schema.String) },
        Class: { name: Schema.String, parents: Schema.Array(Schema.String).annotate({ description: 'Superclasses, nearest first' }), fields: Schema.Array(Field), methods: Schema.Array(Method) },
        Enumeration: { name: Schema.String, members: Schema.Array(Member.mapFields(Struct.pick(['name', 'description']))) },
    });

    run() {
        const { name: given } = this;
        return Effect.gen(function* () {
            const model = yield* Model;
            const { classes, enumerations, enumeration, rows, lineage, members } = model;
            const declaration = Effect.fnUntraced(function* (query: string) {
                const name = yield* Unmatched.single('class or enumeration', Option.some(query))([...classes, ...enumerations], Array.of);
                const row = yield* Effect.catchTag(Effect.asSome(Cache.get(rows, name)), 'Unmatched', () => Effect.succeedNone);
                const { fields, methods, statics } = Option.getOrElse(row, () => ({ fields: {}, methods: {}, statics: {} }));
                const type: unknown = Reflect.get(model, name);
                const live = Predicate.isFunction(type) ? Object.entries(Object.getOwnPropertyDescriptors(type.prototype)) : [];
                return yield* Option.match(enumeration(name), {
                    onSome: (found) => Effect.succeed(Kind.Result.cases.Enumeration.make({ name, members: Array.map(members(found), (each) => ({ name: String(each), description: Record.get(statics, String(each)) })) })),
                    onNone: () =>
                        Effect.map(
                            Effect.all({
                                parents: Option.isSome(row) ? lineage(name) : Effect.succeed([]),
                                fields: Effect.forEach(
                                    live.filter(([, { get }]) => get !== undefined),
                                    ([key]) => Option.match(Record.get(fields, key), { onNone: () => bare(key), onSome: Effect.succeed }),
                                ),
                                methods: Effect.forEach(
                                    live.filter(([, { value: method }]) => Predicate.isFunction(method) && method !== type),
                                    ([key]) => Option.match(Record.get(methods, key), { onNone: () => Effect.orDie(Schema.decodeEffect(Method)({ name: key, parameters: [] })), onSome: Effect.succeed }),
                                ),
                            }),
                            (declared) => Kind.Result.cases.Class.make({ name, ...declared }),
                        ),
                });
            });
            return yield* Option.match(given, { onNone: () => Effect.succeed(Kind.Result.cases.Index.make({ classes, enumerations })), onSome: declaration });
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const getters = (found: InDesignDOMObject<unknown>) => Array.flatMap(Object.entries(Object.getOwnPropertyDescriptors(Object.getPrototypeOf(found))), ([name, { get }]) => (get === undefined || name === 'properties' ? [] : [name]));
const bare = (name: string) => Effect.orDie(Schema.decodeEffect(Field)({ name, settable: false }));
const values = Effect.fn('values')(function* (found: InDesignDOMObject<unknown>, wanted: readonly string[]) {
    const { rows, unavailable } = yield* Model;
    const { fields } = yield* Cache.get(rows, found.constructorName);
    const prototype = Object.getPrototypeOf(found);
    return yield* Effect.forEach(
        wanted,
        Effect.fnUntraced(function* (name) {
            yield* Object.getOwnPropertyDescriptor(prototype, name)?.get === undefined ? Effect.fail(Unmatched.make({ subject: `field of ${found.toSpecifier()}`, query: Option.some(name), candidates: getters(found) })) : Effect.void;
            const refused = (cause: unknown) => new Cause.UnknownError(cause, `${found.toSpecifier()} ${name}`);
            const value = yield* Effect.result(Effect.andThen(Effect.transposeOption(Option.map(unavailable(found.constructorName, name, Option.some(found), false), flow(refused, Effect.fail))), Effect.tryPromise({ try: (): Promise<unknown> => Promise.resolve(Reflect.get(found, name)), catch: refused })));
            return { row: Struct.omit(yield* Option.match(Record.get(fields, name), { onNone: () => bare(name), onSome: Effect.succeed }), ['unavailable']), value };
        }),
    );
});
const described = ({ row, value }: Effect.Success<ReturnType<typeof values>>[number]) => Result.match(value, { onFailure: (unavailable) => ({ ...row, _tag: 'Failure', unavailable }), onSuccess: (held) => ({ ...row, _tag: 'Success', value: held === undefined ? null : held }) });
const object = Effect.fn('object')(function* (found: InDesignDOMObject<unknown>, fields: Option.Option<readonly string[]>) {
    const model = yield* Model;
    const { fields: own } = yield* Cache.get(model.rows, found.constructorName);
    const view = yield* Effect.transposeOption(Option.firstSomeOf(Array.map(View.members, (kind): Option.Option<Effect.Effect<typeof View.Type, Cause.UnknownError | Unmatched, Model | Uxp>> => kind.viewed(model)(found))));
    const entries = yield* values(
        found,
        Option.getOrElse(fields, () => (Option.isSome(view) ? Record.keys(Record.filter(own, model.plural)) : getters(found))),
    );
    const [collections, scalars] = Array.partition(entries, (entry) =>
        Result.isSuccess(entry.value) && model.collection(entry.value.success) ? Result.succeed({ name: entry.row.name, type: entry.value.success.constructor.name, count: entry.value.success.length, specifier: entry.value.success.everyItem().toSpecifier() }) : Result.fail(described(entry)),
    );
    return { _tag: 'Object', ...Reference.of(found), view, fields: scalars, collections };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Inspect, Kind };
