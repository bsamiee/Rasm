import { PgTypes } from '@effect/sql-pg';
import { DateTime, Function, flow, Result, Schema } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

const types = PgTypes.makeRegistry();
types.register(PgTypes.OID.int8, {
    decode: ({ buffer, byteOffset }) => {
        const value = new DataView(buffer, byteOffset).getBigInt64(0);
        return Result.succeed(Number.isSafeInteger(Number(value)) ? Number(value) : value);
    },
    encode: (value: number | bigint) => PgTypes.encode(value, PgTypes.OID.int8),
});
types.register(PgTypes.OID.timestamptz, {
    decode: (bytes) =>
        PgTypes.decode(bytes, PgTypes.OID.timestamptz, 1).pipe(
            Result.flatMap(Schema.decodeUnknownResult(Schema.DateTimeUtcFromDate)),
            Result.mapError((error) => new PgTypes.CodecError({ message: error.message })),
            Result.map(DateTime.formatIso),
        ),
    encode: (value: string | Date | number) => PgTypes.encode(value, PgTypes.OID.timestamptz),
});
types.register(PgTypes.OID.jsonb, {
    decode: (bytes) => Result.succeed(new TextDecoder().decode(bytes.subarray(1))),
    encode: (value: unknown) => PgTypes.encode(value, PgTypes.OID.jsonb),
});
types.register(PgTypes.OID.bool, {
    decode: (bytes) => Result.succeed<0 | 1>(bytes[0] === 1 ? 1 : 0),
    encode: (value: boolean | 0 | 1) => PgTypes.encode(value === true || value === 1, PgTypes.OID.bool),
});
const parsers = { [PgTypes.OID.bool]: (text: string) => (text === 't' ? 1 : 0), [PgTypes.OID.jsonb]: Function.identity<string>, [PgTypes.OID.timestamptz]: flow(DateTime.makeUnsafe<string>, DateTime.formatIso) };

// --- [EXPORTS] -------------------------------------------------------------------------

export { parsers, types };
