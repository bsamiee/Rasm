import { PgClient, PgTypes } from '@effect/sql-pg';
import { Result, Schema } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

const pgClient = (config: PgClient.PgPoolConfig) => {
    const { OID, decode, encode } = PgTypes;
    const types = PgTypes.makeRegistry();
    const register = (oid: number, read: PgTypes.Codec<unknown>['decode']) => types.register(oid, { decode: read, encode: (value) => encode(value, oid) });
    register(OID.int8, (bytes) => Result.map(decode(bytes, OID.int8, 1), (value) => (Number.isSafeInteger(Number(value)) ? Number(value) : value)));
    register(OID.timestamptz, (bytes) => Result.flatMap(decode(bytes, OID.timestamptz, 1), (value) => Result.mapError(Schema.encodeUnknownResult(Schema.DateFromString)(value), (error) => new PgTypes.CodecError({ message: error.message }))));
    register(OID.jsonb, (bytes) => decode(bytes.subarray(1), OID.text, 1));
    register(OID.bool, (bytes) => Result.map(decode(bytes, OID.bool, 1), Number));
    return PgClient.layer({ ...config, types });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { pgClient };
