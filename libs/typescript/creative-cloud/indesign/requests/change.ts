import type { InDesignDOMObject } from 'adobe:indesign';
import { HostError, Reference } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cache, Cause, Effect, Option, Predicate, Record, Schema, Struct } from 'effect';
import { unavailable } from '#/dom/dictionary';
import { Model } from '#/dom/model';
import { reference, Specifier } from '#/dom/specifier';
import { commit } from '#/dom/transactions';
import { decoder, record, Values } from '#/dom/value';

// --- [MODELS] --------------------------------------------------------------------------

class Change extends Schema.Class<Change, { readonly brand: unique symbol }>('Change')({ target: Specifier.Path, values: Values }) {
    static readonly Result = Reference.mapFields(Struct.assign({ written: Schema.Array(Schema.String) }));
    static readonly write: (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>) => Effect.Effect<readonly string[], HostError | Unmatched | Schema.SchemaError, Model> = Effect.fnUntraced(function* (target, values) {
        const specifier = target.toSpecifier();
        const { rows, plural, host } = yield* Model;
        const { fields } = yield* Cache.get(rows, target.constructorName);
        return yield* Effect.validate(
            Object.entries(values),
            Effect.fnUntraced(function* ([field, value]: readonly [string, unknown]) {
                const subject = `${specifier} ${field}`;
                const refused = HostError.refused(subject);
                const row = yield* Effect.fromOption(Record.get(fields, field), () => Unmatched.make({ subject: `field of ${specifier}`, query: Option.some(field), candidates: Record.keys(Record.filter(fields, Predicate.not(plural))) }));
                const nested = record(value) && !row.settable;
                yield* Effect.transposeOption(Option.map(unavailable(target.constructorName, field, Option.some(target), !nested), (because) => HostError.make({ reason: { _tag: 'Unavailable', because }, subject })));
                const inner: unknown = nested ? yield* Effect.try({ try: () => Reflect.get(target, field), catch: refused }) : undefined;
                if (record(value) && host(inner)) {
                    return Array.map(yield* Change.write(inner, value), (name) => `${field}.${name}`);
                }
                const native = yield* decoder(target, Option.some(row))(value);
                yield* Effect.try({ try: () => Object.assign(target, { [field]: native }), catch: refused });
                return [field];
            }, Effect.sandbox),
        ).pipe(
            Effect.map(Array.flatten),
            Effect.catch((causes) => Effect.failCause(Cause.fromReasons(causes.flatMap(Struct.get('reasons'))))),
        );
    });

    static readonly handle = Effect.fn('Change.handle')(function* (payload: Change) {
        const found = yield* Specifier.object(payload.target);
        return yield* commit(
            payload.label,
            Effect.map(Change.write(found, payload.values), (written) => ({ ...reference(found), written })),
        );
    });

    get label(): string {
        return `Change ${Record.keys(this.values).join(', ')} of ${this.target.leaf}`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Change };
