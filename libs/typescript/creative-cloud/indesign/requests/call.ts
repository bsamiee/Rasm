import type { InDesignDOMObject } from 'adobe:indesign';
import { HostError, Reference } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cache, Cause, Effect, Option, Record, Schema, Struct } from 'effect';
import { Model } from '#/dom/model';
import { reference, Specifier } from '#/dom/specifier';
import { commit } from '#/dom/transactions';
import { decoder, read, Value, Values } from '#/dom/value';
import { Change } from '#/requests/change';

// --- [MODELS] --------------------------------------------------------------------------

class Call extends Schema.Class<Call, { readonly brand: unique symbol }>('Call')({
    target: Specifier.Path.annotate({ description: 'Object, or owner of collection' }),
    collection: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Collection field of target, its method called in place of the object method' })),
    method: Schema.String.annotate({ description: 'Method name (add, remove, place, duplicate, move, save, close, exportFile)' }),
    arguments: Schema.OptionFromOptionalKey(Schema.Array(Value).annotate({ description: 'Positional arguments' })),
    properties: Schema.OptionFromOptionalKey(Values.annotate({ description: 'Fields written onto returned object' })),
}) {
    static readonly Result = Reference.mapFields(Struct.assign({ result: Schema.OptionFromOptionalKey(Schema.Json) }));
    static readonly handle = (payload: Call) => Effect.flatMap(Specifier.object(payload.target), (found) => commit(payload.label, called(payload, found)));

    get label(): string {
        return `Call ${[...Option.toArray(this.collection), this.method].join(' ')} of ${this.target.leaf}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const called = Effect.fnUntraced(function* ({ collection, method, arguments: positional, properties, label }: Call, found: InDesignDOMObject<unknown>) {
    const model = yield* Model;
    const listed = Effect.fnUntraced(function* (field: string) {
        const { fields } = yield* Cache.get(model.rows, found.constructorName);
        const items = yield* Effect.fromOption(Option.liftPredicate(Reflect.get(found, field), model.collection), () => Unmatched.make({ subject: `collection of ${found.toSpecifier()}`, query: Option.some(field), candidates: Record.keys(Record.filter(fields, model.plural)) }));
        return [items, items.constructor.name] as const;
    });
    const [receiver, kind] = yield* Option.match(collection, { onNone: () => Effect.succeed([found, found.constructorName] as const), onSome: listed });
    const { methods } = yield* Cache.get(model.rows, kind);
    const { parameters } = yield* Effect.fromOption(Record.get(methods, method), () => Unmatched.make({ subject: `method of ${kind}`, query: Option.some(method), candidates: Record.keys(methods) }));
    const ordered = Option.getOrElse(positional, Array.empty);
    yield* ordered.length > parameters.length ? new Cause.IllegalArgumentError(`${kind} ${method} takes ${parameters.length} parameters (${Array.map(parameters, Struct.get('name')).join(', ')}), given ${ordered.length}`) : Effect.void;
    const positioned = yield* Effect.forEach(Array.zip(ordered, parameters), ([value, parameter]) => decoder(found, Option.some(parameter))(value));
    const before = reference(found);
    const returned = yield* Effect.try({ try: (): unknown => Reflect.apply(Reflect.get(receiver, method), receiver, positioned), catch: HostError.refused(`${kind} ${method}`) });
    const made = Array.head(Array.ensure<unknown>(returned).filter(model.host));
    yield* Option.match(properties, {
        onNone: () => Effect.void,
        onSome: (supplied) =>
            Effect.flatMap(
                Effect.fromOption(made, () => new Cause.IllegalArgumentError(`${kind} ${method} returned no object for properties`)),
                (object) => Change.write(object, supplied),
            ),
    });
    return yield* read(Call.Result, label, () => ({ ...before, valid: found.isValid, result: Option.fromUndefinedOr(returned) }));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Call };
