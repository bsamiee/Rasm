import { Field, Method, ObjectModel } from '@rasm/automation/requests';
import { Uxp } from '@rasm/uxp/runtime';
import { Array, Cache, Effect, Option, Predicate, Record, Struct } from 'effect';
import { undeclared } from '#/dom/dictionary';
import { Model } from '#/dom/model';

// --- [OPERATIONS] ----------------------------------------------------------------------

const objectModel = Effect.fn('objectModel')(function* (payload: ObjectModel) {
    const model = yield* Model;
    const { classes, enumerations, enumeration, rows, lineage, members } = model;
    const { Class, Enumeration } = ObjectModel.Result.cases;
    const declaration = Effect.fnUntraced(function* (name: string) {
        const row = yield* Effect.catchTag(Effect.asSome(Cache.get(rows, name)), 'Unmatched', () => Effect.succeedNone);
        const { fields, methods, statics } = Option.getOrElse(row, () => ({ fields: {}, methods: {}, statics: {} }));
        const type: unknown = Reflect.get(model, name);
        const live = Predicate.isFunction(type) ? Uxp.accessors(type.prototype) : { getters: [], methods: [] };
        return yield* Option.match(enumeration(name), {
            onSome: (found) => Effect.succeed(Enumeration.make({ name, members: Array.map(members(found), (each) => ({ name: String(each), description: Record.get(statics, String(each)) })) })),
            onNone: () =>
                Effect.map(
                    Effect.all({
                        parents: Option.isSome(row) ? lineage(name) : Effect.succeed([]),
                        fields: Effect.forEach(Array.map(live.getters, Struct.get('name')), (key) => Option.match(Record.get(fields, key), { onNone: () => undeclared(Field, key), onSome: Effect.succeed })),
                        methods: Effect.forEach(live.methods, (key) => Option.match(Record.get(methods, key), { onNone: () => undeclared(Method, key), onSome: Effect.succeed })),
                    }),
                    (declared) => Class.make({ name, ...declared }),
                ),
        });
    });
    return yield* payload.describe(classes, enumerations, declaration);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { objectModel };
