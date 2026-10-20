import { PgTypes } from '@effect/sql-pg';
import { DateTime, Function, flow, Result, Schema } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

const types = PgTypes.makeRegistry();
types.register(PgTypes.OID.int8, {
    decode: (bytes) => Result.flatMap(PgTypes.decode(bytes, PgTypes.OID.int8, 1), (value) => Result.liftPredicate(Number(value), Number.isSafeInteger, () => new PgTypes.CodecError({ message: `int8 ${value} exceeds the safe integer range, read it as text` }))),
    encode: (value: number | bigint) => PgTypes.encode(BigInt(value), PgTypes.OID.int8),
});
types.register(PgTypes.OID.timestamptz, {
    decode: flow((bytes: Uint8Array) => PgTypes.decode(bytes, PgTypes.OID.timestamptz, 1), Result.flatMap(Schema.decodeUnknownResult(Schema.DateTimeUtcFromDate)), Result.mapBoth({ onFailure: (error) => new PgTypes.CodecError({ message: error.message }), onSuccess: DateTime.formatIso })),
    encode: (value: string | Date | number) => PgTypes.encode(value, PgTypes.OID.timestamptz),
});
types.register(PgTypes.OID.jsonb, {
    decode: (bytes) => Result.succeed(new TextDecoder().decode(bytes.subarray(1))),
    encode: (value: unknown) => (typeof value === 'string' ? Result.succeed(new TextEncoder().encode(`\u0001${value}`)) : PgTypes.encode(value, PgTypes.OID.jsonb)),
});
types.register(PgTypes.OID.bool, {
    decode: (bytes) => Result.map(PgTypes.decode(bytes, PgTypes.OID.bool, 1), (value) => (value === true ? 1 : 0)),
    encode: (value: boolean | 0 | 1) => PgTypes.encode(Boolean(value), PgTypes.OID.bool),
});
const parsers = { [PgTypes.OID.bool]: (text: string) => Number(text === 't'), [PgTypes.OID.jsonb]: Function.identity<string>, [PgTypes.OID.timestamptz]: flow(DateTime.makeUnsafe<string>, DateTime.formatIso) };

// --- [EXPORTS] -------------------------------------------------------------------------

export { parsers, types };
