import type { Entry } from 'adobe:uxp';
import { Cause, Effect, flow, identity, Match, Option, Predicate, type Record, Result, Schema } from 'effect';
import { Model } from '#/dom/model';
import { Reference } from '#/dom/reference';

// --- [OPERATIONS] ----------------------------------------------------------------------

const record = (value: unknown): value is Record.ReadonlyRecord<string, unknown> => Predicate.isObject(value) && Object.getPrototypeOf(value) === Object.prototype;
const encoder = Effect.map(
    Model,
    ({ Enumerator, host, collection }) =>
        (value: unknown): string | undefined =>
            JSON.stringify(value, function (this: object, key: string, part: unknown) {
                const held: unknown = Reflect.get(this, key);
                return Match.value(held).pipe(
                    Match.when(Option.isOption, Option.getOrUndefined),
                    Match.when(Result.isResult, Result.match({ onSuccess: (success) => ({ _tag: 'Success', success: Option.isOption(success) ? Option.getOrNull(success) : success }), onFailure: (failure) => ({ _tag: 'Failure', failure }) })),
                    Match.orElse(() => part),
                    Match.value,
                    Match.when(Match.instanceOf(Enumerator), String),
                    Match.when(collection, (items) => ({ type: items.constructor.name, specifier: items.everyItem().toSpecifier(), length: items.length })),
                    Match.when(host, Reference.of),
                    Match.when(
                        (entry: unknown): entry is Entry => !record(entry) && Predicate.hasProperty(entry, 'isEntry') && entry.isEntry === true,
                        (entry) => String(entry.url),
                    ),
                    Match.when(Predicate.isPromise, () => {
                        throw new TypeError(`${key} holds a promise, scripts answer before it settles`);
                    }),
                    Match.when(Predicate.isError, flow(Schema.encodeOption(Schema.Defect()), Option.getOrUndefined)),
                    Match.when(Predicate.isMap, Object.fromEntries),
                    Match.when(Predicate.isSet, (entries) => [...entries]),
                    Match.when(Predicate.isBigInt, String),
                    Match.orElse(identity),
                );
            }),
);
const read = <S extends Schema.Top>(schema: S, at: string, value: () => unknown) =>
    Effect.flatMap(encoder, (encoded) => Effect.flatMap(Effect.try({ try: () => encoded(value()), catch: (cause) => new Cause.UnknownError(cause, at) }), flow(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.toCodecJson(schema))), Effect.orDie)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { encoder, read, record };
