import type { InDesignDOMObject } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cache, Cause, Effect, Option, Record, Struct } from 'effect';
import { Call } from '#/contract';
import { Model } from '#/dom/model';
import { commit } from '#/dom/transactions';
import { write } from '#/requests/change';

// --- [OPERATIONS] ----------------------------------------------------------------------

const called = Effect.fnUntraced(function* ({ collection, method: wanted, arguments: positional, properties, label }: Call, found: InDesignDOMObject<unknown>) {
    const model = yield* Model;
    const listed = Effect.fnUntraced(function* (field: string) {
        const { fields } = yield* Cache.get(model.declarations, found.constructorName);
        const items = yield* Effect.fromOption(Option.liftPredicate(Reflect.get(found, field), model.collection), () => Unmatched.make({ subject: `collection of ${found.toSpecifier()}`, query: Option.some(field), candidates: Record.keys(Record.filter(fields, model.collectionField)) }));
        return [items, items.constructor.name] as const;
    });
    const [receiver, kind] = yield* Option.match(collection, { onNone: () => Effect.succeed([found, found.constructorName] as const), onSome: listed });
    const { methods } = yield* Cache.get(model.declarations, kind);
    const [method, { parameters, returns }] = yield* Unmatched.single(Record.toEntries(methods), `method of ${kind}`, Option.some(wanted), ([name]) => [name]);
    const required = parameters.filter(({ optional }) => !optional).length;
    yield* positional.length < required || positional.length > parameters.length
        ? new Cause.IllegalArgumentError(`${kind} ${method} takes ${required === parameters.length ? required : `${required} to ${parameters.length}`} arguments (${Array.map(parameters, ({ name, optional }) => (optional ? `${name}?` : name)).join(', ')}), given ${positional.length}`)
        : Effect.void;
    const decode = Model.decoder(found, yield* Effect.cached(Model.fonts));
    const positioned = yield* Effect.forEach(Array.zip(positional, parameters), ([value, parameter]) => decode(Option.some(parameter), Option.map(returns, Struct.get('type')))(value));
    const before = yield* Model.reference(found);
    const returned = yield* Effect.try({ try: (): unknown => Reflect.apply(Reflect.get(receiver, method), receiver, positioned), catch: HostError.refused(`${kind} ${method}`) });
    const made = Array.head(Array.ensure<unknown>(returned).filter(model.domObject));
    yield* Option.match(properties, {
        onNone: () => Effect.void,
        onSome: (supplied) =>
            Effect.flatMap(
                Effect.fromOption(made, () => new Cause.IllegalArgumentError(`${kind} ${method} returned no object for properties`)),
                (object) => write(object, supplied),
            ),
    });
    return yield* Model.read(Call.Result, label, () => ({ ...before, valid: found.isValid, result: Option.fromUndefinedOr(returned) }));
});
const call = (payload: Call) => Effect.flatMap(Model.located(payload.target), (found) => commit(payload.label, called(payload, found)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { call };
