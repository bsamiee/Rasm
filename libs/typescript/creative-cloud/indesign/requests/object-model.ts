import { ObjectModel } from '@rasm/automation/requests';
import { Uxp } from '@rasm/uxp/runtime';
import { Array, Cache, Effect, Option, Predicate, Record } from 'effect';
import { Dictionary } from '#/dom/dictionary';
import { Model } from '#/dom/model';

// --- [OPERATIONS] ----------------------------------------------------------------------

const objectModel = Effect.fn('objectModel')(function* (payload: ObjectModel) {
    const model = yield* Model;
    const { classes, enumerations, enumeration, declarations, parents, members } = model;
    const { Class, Enumeration } = ObjectModel.Result.cases;
    const declaration = Effect.fnUntraced(function* (name: string) {
        const declared = yield* Effect.catchTag(Effect.asSome(Cache.get(declarations, name)), 'Unmatched', () => Effect.succeedNone);
        const { fields, methods, statics } = Option.getOrElse(declared, () => ({ fields: {}, methods: {}, statics: {} }));
        const type: unknown = Reflect.get(model, name);
        const [live] = Predicate.isFunction(type) ? Uxp.members(type.prototype) : [{ fields: [], methods: [] }];
        return yield* Option.match(enumeration(name), {
            onSome: (found) => Effect.succeed(Enumeration.make({ name, members: Array.map(members(found), (each) => ({ name: String(each), description: Record.get(statics, String(each)) })) })),
            onNone: () =>
                Effect.map(
                    Effect.all({
                        parents: Option.isSome(declared) ? parents(name) : Effect.succeed([]),
                        fields: Effect.forEach(live.fields, (accessor) => Option.match(Record.get(fields, accessor.name), { onNone: () => Dictionary.undeclared(ObjectModel.Field, accessor), onSome: Effect.succeed })),
                        methods: Effect.forEach(live.methods, (method) => Option.match(Record.get(methods, method), { onNone: () => Dictionary.undeclared(ObjectModel.Method, { name: method }), onSome: Effect.succeed })),
                    }),
                    (described) => Class.make({ name, ...described }),
                ),
        });
    });
    return yield* payload.describe(classes, enumerations, declaration);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { objectModel };
