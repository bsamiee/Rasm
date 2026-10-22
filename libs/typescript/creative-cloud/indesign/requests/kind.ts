import { Unmatched } from '@rasm/search/unmatched.ts';
import { Array, Cache, Effect, Option, Predicate, Record, Schema, Struct } from 'effect';
import { bare, Field, Member, Method } from '../dom/dictionary.ts';
import { Model } from '../dom/model.ts';

// --- [MODELS] --------------------------------------------------------------------------

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

// --- [EXPORTS] -------------------------------------------------------------------------

export { Kind };
