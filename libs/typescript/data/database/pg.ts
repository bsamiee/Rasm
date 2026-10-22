import { PgClient, PgTypes } from '@effect/sql-pg';
import { DateTime, Function, flow, Redacted, Result, Schema } from 'effect';
import { transforms } from './lexical.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const types = PgTypes.makeRegistry();
const decoders = [
    [PgTypes.OID.int8, (bytes: Uint8Array) => Result.map(PgTypes.decode(bytes, PgTypes.OID.int8, 1), (value) => (Number.isSafeInteger(Number(value)) ? Number(value) : value))],
    [PgTypes.OID.timestamptz, (bytes: Uint8Array) => Result.mapError(Result.flatMap(PgTypes.decode(bytes, PgTypes.OID.timestamptz, 1), Schema.encodeUnknownResult(Schema.DateFromString)), (error) => new PgTypes.CodecError({ message: error.message }))],
    [PgTypes.OID.jsonb, (bytes: Uint8Array) => PgTypes.decode(bytes.subarray(1), PgTypes.OID.text, 1)],
    [PgTypes.OID.bool, (bytes: Uint8Array) => Result.map(PgTypes.decode(bytes, PgTypes.OID.bool, 1), Number)],
] as const;
decoders.forEach(([oid, decode]) => {
    types.register(oid, { decode, encode: (value: unknown) => PgTypes.encode(value, oid) });
});
const parsers = { [PgTypes.OID.bool]: (text: string) => Number(text === 't'), [PgTypes.OID.jsonb]: Function.identity<string>, [PgTypes.OID.timestamptz]: flow(DateTime.makeUnsafe<string>, DateTime.formatIso) };
const direct = (url: Redacted.Redacted) => PgClient.layer({ url, ...transforms, types });
const hyperdrive = (binding: Hyperdrive) => PgClient.layerFrom(PgClient.makeClient({ url: Redacted.make(binding.connectionString), ...transforms, types, prepare: false }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { direct, hyperdrive, parsers, types };
