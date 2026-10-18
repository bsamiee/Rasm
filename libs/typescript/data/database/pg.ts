import { PgTypes } from '@effect/sql-pg';
import { DateTime, Function, flow, Result } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const jsonb = { version: 1 };
const scale = 1000n;
const epoch = BigInt(DateTime.toEpochMillis(DateTime.makeUnsafe('2000-01-01T00:00:00Z'))) * scale;
const width = BigInt64Array.BYTES_PER_ELEMENT;

// --- [COMPOSITION] ---------------------------------------------------------------------

const types = PgTypes.makeRegistry();
const sized = Result.liftPredicate(
    ({ byteLength }: Uint8Array) => byteLength === width,
    ({ byteLength }) => new PgTypes.CodecError({ message: `8-byte value took ${byteLength} bytes` }),
);
const word = (value: bigint) => {
    const bytes = new Uint8Array(width);
    new DataView(bytes.buffer).setBigInt64(0, value);
    return bytes;
};
types.register(PgTypes.OID.int8, {
    decode: (bytes) =>
        sized(bytes).pipe(
            Result.map(({ buffer, byteOffset }) => new DataView(buffer, byteOffset, width).getBigInt64(0)),
            Result.map((value) => (Number.isSafeInteger(Number(value)) ? Number(value) : value)),
        ),
    encode: (value: number | bigint) => Result.succeed(word(BigInt(value))),
});
types.register(PgTypes.OID.timestamptz, {
    decode: (bytes) =>
        sized(bytes).pipe(
            Result.map(({ buffer, byteOffset }) => new DataView(buffer, byteOffset, width).getBigInt64(0)),
            Result.map((micros) => (micros + epoch) / scale),
            Result.flatMap((millis: bigint) => Result.fromOption(DateTime.make(Number(millis)), () => new PgTypes.CodecError({ message: 'timestamptz holds no finite instant' }))),
            Result.map(DateTime.formatIso),
        ),
    encode: (value: string) => Result.fromOption(DateTime.make(value), () => new PgTypes.CodecError({ message: `${value} names no instant` })).pipe(Result.map((instant) => word(BigInt(DateTime.toEpochMillis(instant)) * scale - epoch))),
});
types.register(PgTypes.OID.jsonb, {
    decode: (bytes) => (bytes[0] === jsonb.version ? Result.succeed(new TextDecoder().decode(bytes.subarray(1))) : Result.fail(new PgTypes.CodecError({ message: `Unsupported jsonb version byte ${bytes[0]}` }))),
    encode: (value: unknown) => {
        const text = new TextEncoder().encode(typeof value === 'string' ? value : JSON.stringify(value));
        const bytes = new Uint8Array(text.byteLength + 1);
        bytes[0] = jsonb.version;
        bytes.set(text, 1);
        return Result.succeed(bytes);
    },
});
types.register(PgTypes.OID.bool, {
    decode: (bytes) => Result.succeed<0 | 1>(bytes[0] === 1 ? 1 : 0),
    encode: (value: boolean | 0 | 1) => Result.succeed(Uint8Array.of(value === true || value === 1 ? 1 : 0)),
});
const parsers = { [PgTypes.OID.bool]: (text: string) => (text === 't' ? 1 : 0), [PgTypes.OID.jsonb]: Function.identity<string>, [PgTypes.OID.timestamptz]: flow(DateTime.makeUnsafe<string>, DateTime.formatIso) };

// --- [EXPORTS] -------------------------------------------------------------------------

export { parsers, types };
