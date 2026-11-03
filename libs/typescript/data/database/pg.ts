import { PgClient, PgTypes } from '@effect/sql-pg';
import { PgliteClient } from '@effect/sql-pglite';
import { DateTime, Function, flow, Record, Result, Schema } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const codec = (oid: number, parse: (text: string) => unknown, encoded: (value: unknown) => Result.Result<unknown, PgTypes.CodecError>, read = (bytes: Uint8Array) => PgTypes.decode(bytes, oid, 1)) => ({
    oid,
    text: (text: string) => Result.getOrThrow(encoded(parse(text))),
    binary: (bytes: Uint8Array) => Result.flatMap(read(bytes), encoded),
});
const codecs = [
    codec(PgTypes.OID.int8, BigInt, (value) => Result.succeed(Number.isSafeInteger(Number(value)) ? Number(value) : value)),
    codec(PgTypes.OID.timestamptz, flow(DateTime.makeUnsafe, DateTime.toDateUtc), (value) => Result.mapError(Schema.encodeUnknownResult(Schema.DateFromString)(value), (error) => new PgTypes.CodecError({ message: error.message }))),
    codec(PgTypes.OID.jsonb, Function.identity, Result.succeed, (bytes) => PgTypes.decode(bytes.subarray(1), PgTypes.OID.text, 1)),
    codec(PgTypes.OID.bool, (text) => text === 't', flow(Number, Result.succeed)),
];

// --- [COMPOSITION] ---------------------------------------------------------------------

const pool = (config: PgClient.PgPoolConfig) => {
    const types = PgTypes.makeRegistry();
    codecs.forEach(({ oid, binary }) => types.register(oid, { decode: binary, encode: (value: unknown) => PgTypes.encode(value, oid) }));
    return PgClient.layer({ ...config, types });
};
const pglite = (config: PgliteClient.PgliteClientConfig.Create) => PgliteClient.layer({ ...config, parsers: { ...config.parsers, ...Record.fromIterableWith(codecs, ({ oid, text }) => [`${oid}`, text]) } });

// --- [EXPORTS] -------------------------------------------------------------------------

export { pglite, pool };
