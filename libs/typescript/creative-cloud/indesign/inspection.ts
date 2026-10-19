import type { InDesignDOMObject } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cache, Cause, Effect, flow, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import { DefinitionView } from './definitions.ts';
import { DocumentView, LayoutView } from './documents.ts';
import { FontView } from './fonts.ts';
import { Host } from './host.ts';
import { Classes, Field, Member, Method } from './objects/classes.ts';
import { Reference, read, resolve, Specifier } from './objects/specifiers.ts';
import { Session } from './session.ts';
import { ThreadView } from './text.ts';

// --- [MODELS] --------------------------------------------------------------------------

const View = Schema.Union([DocumentView, LayoutView, DefinitionView, ThreadView, FontView]);
class Inspect extends Schema.Class<Inspect, { readonly brand: unique symbol }>('Inspect')(
    {
        target: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'A specifier, or the name of a class or enumeration; the session when absent' })),
        fields: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: 'Limits object fields or class and enumeration members; collections list references unless fields are requested. Object values return specifiers to inspect further' })),
    },
    {
        description:
            "Reads InDesign's native object model without changing it. No target: the session, its documents, windows, selection, overset stories, and undo history. A specifier: a typed view of documents, pages, spreads, parents, layers, links, swatches, styles, stories, frames, text, and fonts, beside its native fields on request; other objects list fields, values, valid values, writable fields, and related collections. A collection: its members. A class name: fields and methods from the host dictionary. An enumeration name: its members",
    },
) {
    static readonly Result = Schema.Union([
        Session,
        Schema.TaggedStruct('Object', {
            ...Reference.mapFields(Struct.omit(['name'])).fields,
            view: Schema.OptionFromOptionalKey(View),
            fields: Schema.Array(Schema.TaggedUnion({ Success: { ...Member.mapFields(Struct.omit(['unavailable'])).fields, settable: Schema.Boolean, value: Schema.Json }, Failure: { ...Field.fields, unavailable: Schema.Defect() } })),
            collections: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, count: Schema.Int, specifier: Specifier })),
        }),
        Schema.TaggedStruct('Items', { specifier: Specifier, total: Schema.Int, items: Schema.Array(Reference.pipe(Schema.fieldsAssign({ values: Schema.JsonObject, unavailable: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.Defect())) }))) }),
        Schema.TaggedStruct('Class', { name: Schema.String, parents: Schema.Array(Schema.String), fields: Schema.Array(Field), methods: Schema.Array(Method) }),
        Schema.TaggedStruct('Enumeration', { name: Schema.String, members: Schema.Array(Member.mapFields(Struct.pick(['name', 'description']))).annotate({ description: 'Write a member by name; scripts use the native enumeration member' }) }),
    ]);

    run() {
        const { target, fields } = this;
        return Effect.gen(function* () {
            const { kinds, host } = yield* Classes;
            const described = Effect.fnUntraced(function* (specifier: string) {
                const found = yield* Effect.catchTag(resolve(specifier), 'Unmatched', (missed) => Effect.fail(Array.isReadonlyArrayEmpty(missed.candidates) ? Unmatched.make({ ...missed, candidates: kinds }) : missed));
                return host(found) ? yield* object(found, fields) : yield* items(specifier, found, fields);
            });
            const raw = yield* Option.match(target, {
                onNone: () => Session.current(),
                onSome: (given) =>
                    Option.match(
                        Array.findFirst(kinds, (name) => name.toLowerCase() === given.toLowerCase()),
                        { onSome: (name) => typed(name, fields), onNone: () => described(given) },
                    ),
            });
            return yield* read(Inspect.Result, () => raw);
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [FIELDS]
const getters = (found: InDesignDOMObject<unknown>) => Array.flatMap(Object.entries(Object.getOwnPropertyDescriptors(Object.getPrototypeOf(found))), ([name, { get }]) => (get === undefined || name === 'properties' ? [] : [name]));
const taken = (found: InDesignDOMObject<unknown>, wanted: readonly string[]) =>
    Effect.flatMap(Classes, ({ unavailable }) => {
        const prototype = Object.getPrototypeOf(found);
        return Effect.forEach(wanted, (name) =>
            Effect.gen(function* () {
                yield* Object.getOwnPropertyDescriptor(prototype, name)?.get === undefined ? Effect.fail(Unmatched.make({ subject: `field of ${found.toSpecifier()}`, query: name, candidates: getters(found) })) : Effect.void;
                const refused = (cause: unknown) => new Cause.UnknownError(cause, `${found.toSpecifier()} ${name}`);
                yield* Option.match(unavailable(found.constructorName, name, Option.some(found), false), { onNone: () => Effect.void, onSome: flow(refused, Effect.fail) });
                const value = yield* Effect.try({ try: (): unknown => Reflect.get(found, name), catch: refused });
                return yield* Predicate.isPromiseLike(value) ? Effect.tryPromise({ try: () => Promise.resolve(value), catch: refused }) : Effect.succeed(value);
            }).pipe(
                Effect.result,
                Effect.map((value) => [name, value] as const),
            ),
        );
    });
const bare = (name: string): typeof Field.Type => ({ name, type: Option.none(), description: Option.none(), array: Option.none(), minimum: Option.none(), maximum: Option.none(), members: Option.none(), unavailable: Option.none(), settable: false });

// --- [KINDS]
const object = Effect.fn('object')(function* (found: InDesignDOMObject<unknown>, fields: Option.Option<readonly string[]>) {
    const classes = yield* Classes;
    const { fields: own } = yield* Cache.get(classes.rows, found.constructorName);
    const matched = <T extends InDesignDOMObject<unknown>, A, E, R>(kind: { readonly guard: (model: Classes['Service']) => (value: InDesignDOMObject<unknown>) => value is T; readonly of: (found: T) => Effect.Effect<A, E, R> }) => Option.map(Option.liftPredicate(found, kind.guard(classes)), kind.of);
    const view = yield* Effect.transposeOption(Option.firstSomeOf<Effect.Effect<typeof View.Type, Cause.UnknownError | Unmatched, Classes | Host>>([matched(DocumentView), matched(LayoutView), matched(DefinitionView), matched(ThreadView), matched(FontView)]));
    const collections = Record.keys(Record.filter(own, classes.plural));
    const entries = yield* taken(
        found,
        Option.getOrElse(fields, () => (Option.isSome(view) ? collections : getters(found))),
    );
    const field = ([name, result]: (typeof entries)[number]) => {
        const row = Option.getOrElse(Record.get(own, name), () => bare(name));
        return Effect.fromResult(result).pipe(
            Effect.matchEffect({
                onFailure: (error) => Effect.map(Schema.encodeEffect(Schema.Defect())(error), (failure) => [{ ...row, _tag: 'Failure' as const, unavailable: failure }]),
                onSuccess: (value) => Effect.succeed(classes.collection(value) ? [] : [{ ...Struct.omit(row, ['unavailable']), _tag: 'Success' as const, value: value === undefined ? null : value }]),
            }),
            Effect.orDie,
        );
    };
    return {
        _tag: 'Object',
        type: found.constructorName,
        specifier: found,
        view: Option.getOrUndefined(view),
        fields: Array.flatten(yield* Effect.forEach(entries, field)),
        collections: Array.flatMap(entries, ([name, value]) => Result.match(value, { onFailure: () => [], onSuccess: (each) => (classes.collection(each) ? [{ name, type: each.constructor.name, count: each.length, specifier: each }] : []) })),
    };
});
const typed = Effect.fn('typed')(function* (name: string, selected: Option.Option<readonly string[]>) {
    const [{ indesign }, { rows, lineage, members }] = yield* Effect.all([Host, Classes]);
    const value: unknown = Reflect.get(indesign, name);
    const { fields, methods, statics } = yield* Cache.get(rows, name).pipe(Effect.catchTag('Unmatched', () => Effect.succeed({ fields: {}, methods: {}, statics: {} })));
    const parents = yield* lineage(name);
    const includes = (key: string) => Option.isNone(selected) || selected.value.includes(key);
    const live = Predicate.isFunction(value) ? Object.entries(Object.getOwnPropertyDescriptors(value.prototype)).filter(([key]) => includes(key)) : [];
    return value instanceof indesign.Enumeration
        ? {
              _tag: 'Enumeration',
              name,
              members: Array.map(
                  members(value).filter((each) => includes(String(each))),
                  (each) => ({ name: String(each), description: Record.get(statics, String(each)) }),
              ),
          }
        : {
              _tag: 'Class',
              name,
              parents,
              fields: Array.flatMap(live, ([key, { get }]) => (get === undefined ? [] : [Option.getOrElse(Record.get(fields, key), () => bare(key))])),
              methods: Array.flatMap(live, ([key, { value: method }]) => (Predicate.isFunction(method) && method !== value ? [Option.getOrElse(Record.get(methods, key), () => ({ name: key, description: Option.none(), parameters: [], returns: Option.none() }))] : [])),
          };
});
const items = Effect.fn('items')(function* (target: string, found: readonly InDesignDOMObject<unknown>[], fields: Option.Option<readonly string[]>) {
    const { named } = yield* Classes;
    const results = yield* Effect.forEach(found, (item) => taken(item, Option.getOrElse(fields, Array.empty)));
    const values = yield* Effect.forEach(
        results,
        Effect.fnUntraced(function* (entries) {
            const attributes = Record.fromEntries(entries);
            const failures = Option.liftPredicate(Record.getFailures(attributes), Predicate.not(Record.isEmptyReadonlyRecord));
            return {
                values: Record.getSuccesses(attributes),
                unavailable: yield* Effect.transposeOption(Option.map(failures, Schema.encodeEffect(Schema.Record(Schema.String, Schema.Defect())))).pipe(Effect.map(Option.getOrUndefined), Effect.orDie),
            };
        }),
    );
    return { _tag: 'Items', specifier: target, total: found.length, items: Array.zipWith(found, values, (item, attributes) => ({ type: item.constructorName, specifier: item, name: named(item) ? item.name : undefined, ...attributes })) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Inspect };
