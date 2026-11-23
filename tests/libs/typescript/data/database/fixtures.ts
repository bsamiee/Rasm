import { D1Client } from '@effect/sql-d1';
import { assert } from '@effect/vitest';
import { Catalog, Column, Relation, type Rows, Tokenizer } from '@rasm/database';
import { Effect, type Layer, Option, Schema, type Scope } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { SqlClient, SqlError } from 'effect/sql';

// --- [MODELS] --------------------------------------------------------------------------

const parents = Relation.make('parents', { id: Schema.String, label: Schema.String, state: Schema.Literals(['open', 'closed']) });
const children = Relation.make('children', { id: Schema.String, parentId: Schema.String, rank: Schema.Int }, { references: [{ columns: ['parentId'], table: parents, onDelete: 'CASCADE' }] });
const values = Relation.make('values', {
    id: Column.Uuid,
    count: Schema.BigIntFromString,
    amount: Schema.BigDecimalFromString,
    body: Schema.fromJsonString(Schema.Struct({ camelKey: Schema.Finite })),
    bytes: Column.Bytes,
    at: Schema.DateTimeUtcFromString,
    flag: Schema.BooleanFromBit,
    ratio: Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 1 })),
    code: Schema.String.check(Schema.isBetweenCodePoints(1, 2)),
    note: Schema.OptionFromNullOr(Schema.String),
});
const blobs = Relation.make('blobs', { digest: Column.Bytes, size: Schema.Int }, { key: ['digest'] });
const events = Relation.make('events', { id: Schema.String, tenant: Schema.Int, at: Schema.DateTimeUtcFromString, kind: Schema.Literals(['view', 'buy']) }, { indexes: [{ columns: ['tenant', 'at'] }] });
const accounts = Relation.make(
    'accounts',
    { id: Column.Uuid, email: Schema.String.check(Schema.isAscii()), deletedAt: Schema.NullOr(Schema.DateTimeUtcFromString), profile: Schema.fromJsonString(Schema.Struct({ plan: Schema.String })), plan: Schema.String, active: Schema.BooleanFromBit },
    {
        projections: { plan: { from: 'profile', path: ['plan'] }, active: { where: { deletedAt: null } } },
        indexes: [
            { columns: [{ casefold: 'email' }], unique: true, where: { deletedAt: null } },
            { columns: ['plan'], include: ['email'] },
            { columns: ['profile'], method: 'gin' },
        ],
    },
);
const words = { columns: ['title', 'body'], tokenizer: Tokenizer.Words({ configuration: 'simple', filters: ['unaccent'], tokenize: 'unicode61 remove_diacritics 2' }) } as const;
const notes = Relation.make('notes', { id: Schema.Int, title: Schema.String, body: Schema.String }, { search: words });

// --- [OPERATIONS] ----------------------------------------------------------------------

const declared = (relations: Catalog['tables'], retired: readonly string[] = []) => Catalog.make({ tables: relations, retired, timeouts: { lock: '5 seconds', transaction: '2 minutes' } });
const refused = <A, E, Q>(effect: Effect.Effect<A, E, Q>, reason: SqlError.SqlErrorReason['_tag']) =>
    Effect.all([Effect.flip(effect), Effect.serviceOption(D1Client.D1Client)]).pipe(Effect.map(([error, d1]) => assert.strictEqual(SqlError.isSqlError(error) ? error.reason._tag : error, Option.match(d1, { onNone: () => reason, onSome: () => 'UnknownError' }))));
const isolated =
    <F, R>(client: Parameters<typeof Catalog.layer<Rows, F, R>>[1]) =>
    <Eff extends Effect.Effect<unknown, unknown, Layer.Success<typeof client> | SecureRandom.SecureRandom | Scope.Scope | R>, A>(body: (sql: SqlClient.SqlClient) => Generator<Eff, A, never>) => {
        const execute = Effect.fnUntraced(body);
        return () => SqlClient.SqlClient.use(execute).pipe(Effect.provide([client, SecureRandom.layerWebCrypto]));
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { accounts, blobs, children, declared, events, isolated, notes, parents, refused, values, words };
