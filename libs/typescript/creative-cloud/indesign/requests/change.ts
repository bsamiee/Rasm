import type { InDesignDOMObject } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cache, Cause, Effect, Option, Predicate, Record, type Schema, Struct } from 'effect';
import type { Change } from '#/contract';
import { Dictionary } from '#/dom/dictionary';
import { Model } from '#/dom/model';
import { written } from '#/dom/transactions';
import { Value } from '#/dom/value';

// --- [OPERATIONS] ----------------------------------------------------------------------

const write: (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>) => Effect.Effect<readonly string[], HostError | Unmatched | Schema.SchemaError, Model> = Effect.fnUntraced(function* (target, values) {
    const specifier = target.toSpecifier();
    const { declarations, collectionField, domObject } = yield* Model;
    const [{ fields }, fonts] = yield* Effect.all([Cache.get(declarations, target.constructorName), Effect.cached(Model.fonts)]);
    const decode = Model.decoder(target, fonts);
    return yield* Effect.validate(
        Object.entries(values),
        Effect.fnUntraced(function* ([wanted, value]: readonly [string, unknown]) {
            const [field, row] = yield* Unmatched.single(Record.toEntries(Record.filter(fields, Predicate.not(collectionField))), `field of ${specifier}`, Option.some(wanted), ([name]) => [name]);
            const subject = `${specifier} ${field}`;
            const refused = HostError.refused(subject);
            const nested = Value.plainObject(value) && !row.settable;
            yield* Effect.transposeOption(Option.map(Dictionary.unavailable(target.constructorName, field, Option.some(target), !nested), (because) => HostError.refused(`${subject}: ${because}`)()));
            const inner: unknown = nested ? yield* Effect.try({ try: () => Reflect.get(target, field), catch: refused }) : undefined;
            if (Value.plainObject(value) && domObject(inner)) {
                return Array.map(yield* write(inner, value), (name) => `${field}.${name}`);
            }
            const native = yield* decode(Option.some(row), row.type)(value);
            yield* Effect.try({ try: () => Object.assign(target, { [field]: native }), catch: refused });
            return [field];
        }, Effect.sandbox),
    ).pipe(
        Effect.map(Array.flatten),
        Effect.catch((causes) => Effect.failCause(Cause.fromReasons(causes.flatMap(Struct.get('reasons'))))),
    );
});
const change = (payload: Change) => Effect.flatMap(Model.located(payload.target), (found) => written(payload.label, found, write(found, payload.values)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { change, write };
