// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Crypto, Effect, Encoding, identity, JsonPatch, Option, Record, Schema } from 'effect';
import sharp from 'sharp';

// --- [MODELS] --------------------------------------------------------------------------

const Observation: Schema.TaggedUnion<{
    readonly value: Schema.TaggedStruct<'value', { readonly value: Schema.Codec<Schema.Json> }>;
    readonly unsupported: Schema.TaggedStruct<'unsupported', { readonly reason: Schema.Codec<Schema.Json> }>;
    readonly unreadable: Schema.TaggedStruct<'unreadable', { readonly cause: Schema.Defect }>;
}> = Schema.TaggedUnion({ value: { value: Schema.Json }, unsupported: { reason: Schema.Json }, unreadable: { cause: Schema.Defect() } });

const Drift: Schema.Struct<{
    readonly kind: Schema.Literal<'drift'>;
    readonly rows: Schema.$Record<
        Schema.String,
        Schema.TaggedUnion<{
            readonly unchanged: Schema.TaggedStruct<'unchanged', { readonly value: Schema.Codec<Schema.Json> }>;
            readonly missing: Schema.TaggedStruct<'missing', { readonly expected: Schema.Codec<Schema.Json> }>;
            readonly extra: Schema.TaggedStruct<'extra', { readonly actual: Schema.Codec<Schema.Json> }>;
            readonly changed: Schema.TaggedStruct<
                'changed',
                {
                    readonly expected: Schema.Codec<Schema.Json>;
                    readonly actual: Schema.Codec<Schema.Json>;
                    readonly changes: Schema.NonEmptyArray<
                        Schema.Union<
                            readonly [
                                Schema.Struct<{ readonly op: Schema.Literals<readonly ['add', 'replace']>; readonly path: Schema.String; readonly value: Schema.Codec<Schema.Json> }>,
                                Schema.Struct<{ readonly op: Schema.Literal<'remove'>; readonly path: Schema.String }>,
                            ]
                        >
                    >;
                }
            >;
            readonly unsupported: (typeof Observation)['cases']['unsupported'];
            readonly unreadable: (typeof Observation)['cases']['unreadable'];
        }>
    >;
}> = Schema.Struct({
    kind: Schema.Literal('drift'),
    rows: Schema.Record(
        Schema.String,
        Schema.TaggedUnion({
            unchanged: { value: Schema.Json },
            missing: { expected: Schema.Json },
            extra: { actual: Schema.Json },
            changed: {
                expected: Schema.Json,
                actual: Schema.Json,
                changes: Schema.NonEmptyArray(
                    Schema.Union([
                        Schema.Struct({ op: Schema.Literals(['add', 'replace']), path: Schema.String, value: Schema.Json }),
                        Schema.Struct({ op: Schema.Literal('remove'), path: Schema.String }),
                    ]),
                ),
            },
            unsupported: Observation.cases.unsupported.fields,
            unreadable: Observation.cases.unreadable.fields,
        }),
    ),
});

// --- [COMPARISON] ----------------------------------------------------------------------

/** Compares keyed native readings exactly; unreadable and unsupported rows never imply absence */
const compare = (expected: Schema.JsonObject, observed: Readonly<Record<string, (typeof Observation)['Type']>>): Effect.Effect<(typeof Drift)['Type']> =>
    Effect.succeed({
        kind: 'drift',
        rows: {
            ...Record.map(expected, (value) => ({ _tag: 'missing' as const, expected: value })),
            ...Record.map(observed, (reading, key): (typeof Drift)['Type']['rows'][string] => {
                if (reading._tag !== 'value') {
                    return reading;
                }
                return Option.match(Record.get<string, Schema.Json>(expected, key), {
                    onNone: () => ({ _tag: 'extra', actual: reading.value }),
                    onSome: (value) => {
                        const changes = JsonPatch.get(value, reading.value);
                        return Array.isReadonlyArrayNonEmpty(changes) ? { _tag: 'changed', expected: value, actual: reading.value, changes } : { _tag: 'unchanged', value: reading.value };
                    },
                });
            }),
        },
    });

/** Returns decoded sample dimensions and a pixel digest, excluding container metadata from comparison */
const fingerprint = (source: Uint8Array): Effect.Effect<(typeof Observation)['Type'], never, Crypto.Crypto> =>
    Effect.gen(function* () {
        const { data, info } = yield* Effect.tryPromise({ try: () => sharp(source).raw().toUint8Array(), catch: identity });
        const hash = yield* Crypto.Crypto.use((crypto) => crypto.digest('SHA-256', data));
        return { widthPx: info.width, heightPx: info.height, channels: info.channels, sha256: Encoding.encodeHex(hash) };
    }).pipe(Effect.match({ onSuccess: (value) => ({ _tag: 'value' as const, value }), onFailure: (cause) => ({ _tag: 'unreadable' as const, cause }) }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { compare, Drift, fingerprint, Observation };
