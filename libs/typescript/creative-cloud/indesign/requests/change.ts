import type { InDesignDOMObject } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cache, Cause, Effect, flow, Option, Predicate, Record, Schema, Struct } from 'effect';
import { decoder } from '#/dom/decoding';
import { unavailable } from '#/dom/dictionary';
import { record } from '#/dom/encoding';
import { resolveObject } from '#/dom/location';
import { Model } from '#/dom/model';
import { leaf, Reference, Specifier } from '#/dom/reference';
import { Mutation } from '#/dom/transactions';

// --- [MODELS] --------------------------------------------------------------------------

class Change extends Mutation.extend<Change>('Change')(
    {
        target: Specifier,
        values: Schema.JsonObject.annotate({ description: 'Fields to write by name. Enumeration members by name, named objects (styles, layers, parents, fonts) by name or {type, specifier}, swatches by name, measurements as points or unit strings; a record writes into a nested preference object' }),
    },
    { description: "Writes fields of one object through InDesign's own setters in one undo step, decoding each value by the field's declared type; every refused field is reported, a nested field as its path" },
) {
    static readonly Result = Reference.mapFields(Struct.assign({ written: Schema.Array(Schema.String) }));
    static readonly write: (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>) => Effect.Effect<readonly string[], Cause.UnknownError | Unmatched, Model> = Effect.fnUntraced(function* (target, values) {
        const specifier = target.toSpecifier();
        const { rows, host, plural } = yield* Model;
        const { fields } = yield* Cache.get(rows, target.constructorName).pipe(Effect.mapError((cause) => new Cause.UnknownError(cause, specifier)));
        return yield* Effect.validate(
            Object.entries(values),
            Effect.fnUntraced(function* ([field, value]: readonly [string, unknown]) {
                const refused = (cause: unknown) => new Cause.UnknownError(cause, `${specifier} ${field}`);
                const row = yield* Effect.fromOption(Record.get(fields, field), () => Unmatched.make({ subject: `field of ${specifier}`, query: Option.some(field), candidates: Record.keys(Record.filter(fields, Predicate.not(plural))) }));
                const nested = record(value) && !row.settable;
                yield* Effect.transposeOption(Option.map(unavailable(target.constructorName, field, Option.some(target), !nested), flow(refused, Effect.fail)));
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

    override get label(): string {
        return `Change ${Record.keys(this.values).join(', ')} of ${leaf(this.target)}`;
    }

    run() {
        const { values } = this;
        return Effect.flatMap(resolveObject(this.target), (found) =>
            this.commit(
                Effect.gen(function* () {
                    return { ...Reference.of(found), written: yield* Change.write(found, values) };
                }),
            ),
        );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Change };
