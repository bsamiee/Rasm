import { assert, type Vitest } from '@effect/vitest';
import { Bytes, type Catalog, names, type Table, table, Uuid } from '@rasm/database/catalog.ts';
import { plan, reconcile } from '@rasm/database/reconcile.ts';
import { atomic, insert, latest, remove, search, select, store, tenant, upsert } from '@rasm/database/store.ts';
import { Array, DateTime, Effect, Exit, type Layer, Option, Schema, type Scope } from 'effect';
import { SqlClient, SqlSchema } from 'effect/sql';

// --- [TYPES] ---------------------------------------------------------------------------

type Client<F, Q> = (options: typeof names) => Layer.Layer<SqlClient.SqlClient, F, Q>;

// --- [MODELS] --------------------------------------------------------------------------

const parents = table('parents', { id: Schema.String, label: Schema.String, state: Schema.Literals(['open', 'closed']) }, {});
const children = table('children', { id: Schema.String, parentId: Schema.String, rank: Schema.Int }, { references: [{ columns: ['parentId'], table: parents, onDelete: 'CASCADE' }] });
const renamed = table('parents', { id: Schema.String, title: Schema.String, state: parents.fields.state }, { renamed: { title: 'label' }, indexes: [{ columns: ['title'], unique: true }] });
const extended = table('parents', { ...renamed.fields, extra: Schema.NullOr(Schema.String) }, { renamed: { title: 'label' }, indexes: [{ columns: ['title'], unique: true }] });
const retired = table('parents', renamed.fields, { renamed: { title: 'label' }, indexes: [{ columns: ['title'], unique: true }], retired: ['extra'] });
const revived = table('parents', { ...renamed.fields, extra: Schema.NullOr(Schema.Int) }, { renamed: { title: 'label' }, indexes: [{ columns: ['title'], unique: true }], retired: ['extra'] });
const kids = table('kids', children.fields, { renamedFrom: 'children', references: [{ columns: ['parentId'], table: renamed, onDelete: 'CASCADE' }] });
const retyped = table('kids', { ...children.fields, rank: Schema.Finite }, { renamedFrom: 'children', references: [{ columns: ['parentId'], table: renamed, onDelete: 'CASCADE' }], retype: ['rank'] });
const reopened = table('parents', { ...parents.fields, state: Schema.Literals(['open', 'closed', 'archived']) }, { retype: ['state'] });
const annotated = table('parents', { ...parents.fields, note: Schema.NullOr(Schema.String) }, {});
const required = table('parents', { ...parents.fields, owner: Schema.String }, {});
const values = table(
    'values',
    {
        id: Uuid,
        count: Schema.BigIntFromString,
        body: Schema.fromJsonString(Schema.Struct({ camelKey: Schema.Finite })),
        bytes: Bytes,
        at: Schema.DateTimeUtcFromString,
        flag: Schema.BooleanFromBit,
        ratio: Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 1 })),
        code: Schema.String.check(Schema.isBetweenCodePoints(1, 2)),
        note: Schema.OptionFromNullOr(Schema.String),
    },
    {},
);
const blobs = table('blobs', { digest: Bytes, size: Schema.Int }, { key: ['digest'] });
const events = table('events', { id: Schema.String, tenant: Schema.Int, at: Schema.DateTimeUtcFromString, kind: Schema.Literals(['view', 'buy']) }, { indexes: [{ columns: ['tenant', 'at'] }] });
const accounts = table(
    'accounts',
    { id: Uuid, email: Schema.String, deletedAt: Schema.NullOr(Schema.DateTimeUtcFromString), profile: Schema.fromJsonString(Schema.Struct({ plan: Schema.String })), plan: Schema.optionalKey(Schema.String) },
    {
        projections: { plan: { from: 'profile', path: ['plan'] } },
        indexes: [
            { columns: [{ casefold: 'email' }], unique: true, where: { deletedAt: null } },
            { columns: ['plan'], include: ['email'] },
            { columns: ['profile'], method: 'gin' },
        ],
    },
);
const notes = table('notes', { id: Schema.Int, title: Schema.String, body: Schema.String }, { search: { columns: ['title', 'body'] } });
const fragments = table('fragments', notes.fields, { search: { columns: ['title', 'body'], tokenizer: 'trigram' } });
const textNotes = table('notes', { ...notes.fields, id: Uuid }, { search: { columns: ['title', 'body'] } });
const ledgers = table('ledgers', { id: Uuid, tenant: Schema.String, amount: Schema.Int }, { tenant: 'tenant' });
const legacy = table('legacy', { id: Schema.String, count: Schema.Int, body: Schema.fromJsonString(Schema.Struct({ camelKey: Schema.Finite })) }, { retype: ['count', 'body'] });

// --- [OPERATIONS] ----------------------------------------------------------------------

const cases = <R, F, Q>(it: Vitest.MethodsNonLive<R>, client: Client<F, Q>, isolate: <A, E>(effect: Effect.Effect<A, E, Scope.Scope | Q>) => Effect.Effect<A, E, R | Scope.Scope>, record: (title: string, planned: Effect.Success<ReturnType<typeof plan>>) => Effect.Effect<void, never, Q>) => {
    const declared = (...tables: Array.NonEmptyReadonlyArray<Table>): Catalog => ({ tables, retired: [] });
    const rows = (name: string) => SqlClient.SqlClient.use((sql) => sql`SELECT * FROM ${sql(name)} ORDER BY 1`);
    const shape = SqlClient.SqlClient.use((sql) =>
        sql.onDialectOrElse({
            pg: () => sql`SELECT table_name, column_name, data_type, is_nullable, column_default FROM information_schema.columns WHERE table_schema = current_schema() ORDER BY 1, 2`,
            orElse: () => sql`SELECT name, sql FROM sqlite_schema WHERE name NOT GLOB '_cf_*' ORDER BY name`,
        }),
    );
    const step = (title: string, catalog: Catalog, runtime: Option.Option<string>) =>
        Effect.andThen(
            Effect.flatMap(plan(catalog, runtime), (planned) => record(title, planned)),
            reconcile(catalog, runtime),
        );
    const define = <A, E>(title: string, body: (apply: (catalog: Catalog) => ReturnType<typeof step>, grant: (catalog: Catalog, role: string) => ReturnType<typeof step>, sql: SqlClient.SqlClient) => Effect.Effect<A, E, SqlClient.SqlClient | Scope.Scope | Q>) => {
        const [apply, grant] = [(catalog: Catalog) => step(title, catalog, Option.none()), (catalog: Catalog, role: string) => step(title, catalog, Option.some(role))];
        return it.effect(title, () =>
            isolate(
                Effect.provide(
                    Effect.flatMap(SqlClient.SqlClient, (sql) => body(apply, grant, sql)),
                    client(names),
                ),
            ),
        );
    };
    const seeded = Effect.fn('seeded')(function* (apply: (catalog: Catalog) => ReturnType<typeof step>) {
        const sql = yield* SqlClient.SqlClient;
        yield* apply(declared(parents, children));
        yield* atomic([
            sql`INSERT INTO parents ${sql.insert({ id: 'p', label: 'Parent', state: 'open' })}`,
            sql`INSERT INTO children ${sql.insert([
                { id: 'a', parentId: 'p', rank: 1 },
                { id: 'b', parentId: 'p', rank: 2 },
            ])}`,
        ]);
    });
    it.effect('composes a store whose client names camelCase fields as snake_case columns in every statement form', () =>
        Effect.gen(function* () {
            const sql = yield* SqlClient.SqlClient;
            yield* atomic([sql`INSERT INTO parents ${sql.insert({ id: 'p', label: 'Parent', state: 'open' })}`, sql`INSERT INTO children ${sql.insert({ id: 'a', parentId: 'p', rank: 1 })}`]);
            yield* sql`UPDATE children SET ${sql.update({ parentId: 'p', rank: 2 })} WHERE ${sql('parentId')} = ${'p'}`;
            assert.deepStrictEqual(yield* Effect.flatMap(yield* select(children, 'parentId', ['p']), Schema.decodeUnknownEffect(Schema.Array(children))), [{ id: 'a', parentId: 'p', rank: 2 }]);
            assert.deepStrictEqual(yield* sql.withoutTransforms()`SELECT parent_id FROM children`, [{ parent_id: 'p' }]);
        }).pipe(Effect.provide(store(declared(parents, children), client)), isolate),
    );
    define('creates declared tables in an empty store, enforces references, and decodes rows', (apply, _, sql) =>
        Effect.gen(function* () {
            yield* seeded(apply);
            assert.deepStrictEqual(yield* Effect.flatMap(yield* select(children, 'parentId', ['p']), Schema.decodeUnknownEffect(Schema.Array(children))), [
                { id: 'a', parentId: 'p', rank: 1 },
                { id: 'b', parentId: 'p', rank: 2 },
            ]);
            assert.isTrue((yield* Effect.flip(sql`INSERT INTO children ${sql.insert({ id: 'c', parentId: 'missing', rank: 3 })}`))._tag === 'SqlError');
        }),
    );
    define('renames declared columns and tables and keeps every row', (apply) =>
        Effect.gen(function* () {
            yield* seeded(apply);
            assert.deepStrictEqual(yield* apply(declared(renamed, kids)), { tables: [], columns: [], indexes: [] });
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', title: 'Parent', state: 'open' }]);
            assert.deepStrictEqual((yield* rows('kids')).length, 2);
            assert.isTrue((yield* Effect.flip(rows('children')))._tag === 'SqlError');
        }),
    );
    define('renames back with every row when an older catalog runs after a rename', (apply, _, sql) =>
        Effect.gen(function* () {
            yield* seeded(apply);
            yield* apply(declared(renamed, kids));
            yield* sql`INSERT INTO parents ${sql.insert({ id: 'q', title: 'Second', state: 'closed' })}`;
            yield* apply(declared(parents, children));
            assert.deepStrictEqual(yield* rows('parents'), [
                { id: 'p', label: 'Parent', state: 'open' },
                { id: 'q', label: 'Second', state: 'closed' },
            ]);
            assert.deepStrictEqual((yield* rows('children')).length, 2);
            yield* apply(declared(renamed, kids));
            assert.deepStrictEqual(
                (yield* rows('parents')).map(({ title }) => title),
                ['Parent', 'Second'],
            );
        }),
    );
    define('keeps a newer column and its data when an older catalog reconciles after it', (apply, _, sql) =>
        Effect.gen(function* () {
            yield* seeded(apply);
            yield* apply(declared(extended, kids));
            yield* sql`UPDATE parents SET ${sql.update({ extra: 'kept' })} WHERE id = 'p'`;
            assert.deepStrictEqual(yield* apply(declared(renamed, kids)), { tables: [], columns: [{ table: 'parents', column: 'extra' }], indexes: [] });
            yield* sql`INSERT INTO parents ${sql.insert({ id: 'q', title: 'Second', state: 'closed' })}`;
            yield* apply(declared(extended, kids));
            assert.deepStrictEqual(yield* rows('parents'), [
                { id: 'p', title: 'Parent', state: 'open', extra: 'kept' },
                { id: 'q', title: 'Second', state: 'closed', extra: null },
            ]);
        }),
    );
    define('drops a declared retired column and table and retypes a declared column with its rows', (apply) =>
        Effect.gen(function* () {
            yield* seeded(apply);
            yield* apply(declared(extended, kids));
            yield* apply(declared(retired, retyped));
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', title: 'Parent', state: 'open' }]);
            assert.deepStrictEqual(
                (yield* rows('kids')).map(({ rank }) => rank),
                [1, 2],
            );
            yield* apply({ tables: [retired], retired: ['kids'] });
            assert.isTrue((yield* Effect.flip(rows('kids')))._tag === 'SqlError');
        }),
    );
    define('keeps a re-added column that reuses a retired name when the retiring catalog runs again', (apply, _, sql) =>
        Effect.gen(function* () {
            yield* seeded(apply);
            yield* apply(declared(extended, kids));
            yield* sql`UPDATE parents SET ${sql.update({ extra: 'old' })} WHERE id = 'p'`;
            yield* apply(declared(revived, kids));
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', title: 'Parent', state: 'open', extra: null }]);
            yield* sql`UPDATE parents SET ${sql.update({ extra: 7 })} WHERE id = 'p'`;
            assert.deepStrictEqual(yield* apply(declared(retired, kids)), { tables: [], columns: [{ table: 'parents', column: 'extra' }], indexes: [] });
            assert.deepStrictEqual(yield* apply(declared(extended, kids)), { tables: [], columns: [], indexes: [] });
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', title: 'Parent', state: 'open', extra: 7 }]);
        }),
    );
    define('plans nothing when the store already matches its catalog', (apply) =>
        Effect.gen(function* () {
            const full = declared(extended, kids, accounts, events, notes, values, blobs);
            yield* seeded(apply);
            yield* apply(full);
            const before = yield* shape;
            const planned = yield* Effect.all([plan(full, Option.none()), plan(declared(renamed, kids), Option.none())]);
            assert.deepStrictEqual(
                planned.flatMap(({ statements, concurrent }) => [statements, concurrent]),
                [[], [], [], []],
            );
            yield* apply(full);
            assert.deepStrictEqual(yield* shape, before);
        }),
    );
    define('settles concurrent reconciles of one catalog', () =>
        Effect.gen(function* () {
            yield* Effect.all([reconcile(declared(parents, children), Option.none()), reconcile(declared(parents, children), Option.none())], { concurrency: 'unbounded' });
            assert.deepStrictEqual(yield* rows('children'), []);
        }),
    );
    define('keeps every row and column when two catalogs rebuild and extend one table concurrently', (apply) =>
        Effect.gen(function* () {
            yield* seeded(apply);
            const [extension, rebuild] = yield* Effect.all([Effect.exit(reconcile(declared(annotated, children), Option.none())), Effect.exit(reconcile(declared(reopened, children), Option.none()))], { concurrency: 'unbounded' });
            assert.isTrue(Exit.isSuccess(rebuild) || Exit.isSuccess(extension));
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', label: 'Parent', state: 'open', ...(Exit.isSuccess(extension) ? { note: null } : {}) }]);
            assert.deepStrictEqual((yield* rows('children')).length, 2);
        }),
    );
    define('refuses a new required column without a default, and a tenant table and a search over a non-integer key outside PostgreSQL', (apply, _, sql) =>
        Effect.gen(function* () {
            yield* seeded(apply);
            const refused = (catalog: Catalog) => Effect.map(Effect.flip(reconcile(catalog, Option.none())), (error) => error._tag === 'ReconcileError' && error.findings);
            const unsupported = yield* sql.onDialectOrElse({ pg: () => Effect.succeed([]), orElse: () => refused(declared(ledgers, textNotes)) });
            assert.deepStrictEqual(yield* refused(declared(required, children)), [{ reason: 'Required', table: 'parents', subject: 'owner' }]);
            assert.deepStrictEqual(
                unsupported,
                sql.onDialectOrElse({
                    pg: () => [],
                    orElse: () => [
                        { reason: 'Unsupported', table: 'ledgers', subject: 'tenant' },
                        { reason: 'Unsupported', table: 'notes', subject: 'search' },
                    ],
                }),
            );
        }),
    );
    define('stores 64-bit integers, JSON keys, bytes, instants, booleans, options, and bounded values on every store', (apply, _, sql) =>
        Effect.gen(function* () {
            const written = Array.map(['2026-10-17T12:00:00.123Z', '1969-07-20T20:17:40.500Z'] as const, (at, index) => ({
                id: `019a0000-0000-7000-8000-00000000000${index}`,
                count: BigInt(Number.MAX_SAFE_INTEGER) + BigInt(index + 1),
                body: { camelKey: index },
                bytes: new TextEncoder().encode(at),
                at: DateTime.makeUnsafe(at),
                flag: index === 0,
                ratio: index / 2,
                code: 'ab'.slice(index),
                note: index === 0 ? Option.some(at) : Option.none(),
            }));
            const digests = Array.make({ digest: new TextEncoder().encode('first'), size: 1 }, { digest: new TextEncoder().encode('second'), size: 2 });
            yield* apply(declared(values, blobs));
            const decode = Schema.decodeUnknownEffect(Schema.Array(values));
            assert.deepStrictEqual(yield* Effect.flatMap(yield* insert(values, written), decode), written);
            assert.deepStrictEqual(yield* Effect.flatMap(yield* select(values, 'id', [Array.headNonEmpty(written).id]), decode), [Array.headNonEmpty(written)]);
            assert.deepStrictEqual(yield* Effect.flatMap(yield* select(values, 'count', [Array.lastNonEmpty(written).count]), decode), [Array.lastNonEmpty(written)]);
            yield* yield* insert(blobs, digests);
            assert.deepStrictEqual(yield* Effect.flatMap(yield* select(blobs, 'digest', [Array.lastNonEmpty(digests).digest]), Schema.decodeUnknownEffect(Schema.Array(blobs))), [Array.lastNonEmpty(digests)]);
            assert.deepStrictEqual((yield* yield* remove(blobs, 'digest', [Array.headNonEmpty(digests).digest])).length, 1);
            assert.isTrue((yield* Effect.flip(sql`UPDATE ${sql('values')} SET ratio = 2`))._tag === 'SqlError');
            assert.isTrue((yield* Effect.flip(sql`UPDATE ${sql('values')} SET code = 'abc'`))._tag === 'SqlError');
        }),
    );
    define('writes and reads collections through one parameter with upsert, delete, and latest per key', (apply) =>
        Effect.gen(function* () {
            const volume = 150;
            const tenants = [0, 1, 2] as const;
            yield* apply(declared(events));
            const written = Array.makeBy(volume, (index) => ({ id: `e${index}`, tenant: index % tenants.length, at: DateTime.add(DateTime.makeUnsafe('2026-01-01T00:00:00.000Z'), { seconds: index }), kind: 'view' as const }));
            const decode = Schema.decodeUnknownEffect(Schema.Array(events));
            assert.deepStrictEqual((yield* yield* insert(events, written)).length, volume);
            assert.deepStrictEqual(yield* yield* upsert(events, written), []);
            assert.deepStrictEqual(
                (yield* Effect.flatMap(yield* upsert(events, [{ ...written[0], kind: 'buy' }]), decode)).map(({ kind }) => kind),
                ['buy'],
            );
            assert.deepStrictEqual(
                (yield* Effect.flatMap(yield* latest(events, 'tenant', 'at', tenants), decode)).map(({ id }) => id).sort(),
                written
                    .slice(-tenants.length)
                    .map(({ id }) => id)
                    .sort(),
            );
            assert.deepStrictEqual((yield* yield* remove(events, 'tenant', [0])).length, volume / tenants.length);
            assert.deepStrictEqual((yield* yield* select(events, 'tenant', tenants)).length, volume - volume / tenants.length);
        }),
    );
    define('enforces partial case-insensitive unique keys and reads JSON projections', (apply) =>
        Effect.gen(function* () {
            yield* apply(declared(accounts));
            const account = (id: string, email: string, deleted: boolean) => ({ id, email, deletedAt: deleted ? DateTime.makeUnsafe('2026-10-17T00:00:00.000Z') : null, profile: { plan: 'pro' } });
            yield* yield* insert(accounts, [account('019a0000-0000-7000-8000-000000000001', 'A@example.com', true), account('019a0000-0000-7000-8000-000000000002', 'a@example.com', false)]);
            assert.isTrue((yield* Effect.flip(yield* insert(accounts, [account('019a0000-0000-7000-8000-000000000003', 'A@EXAMPLE.COM', false)])))._tag === 'SqlError');
            assert.deepStrictEqual(
                (yield* Effect.flatMap(yield* select(accounts, 'email', ['a@example.com']), Schema.decodeUnknownEffect(Schema.Array(accounts)))).map(({ plan: tier }) => tier),
                ['pro'],
            );
        }),
    );
    define('searches declared text columns by words and by trigrams and follows updates', (apply, _, sql) =>
        Effect.gen(function* () {
            yield* apply(declared(notes, fragments));
            const written = Array.make({ id: 1, title: 'Concrete shell', body: 'Thin vaults' }, { id: 2, title: 'Timber frame', body: 'Glulam beams' });
            yield* yield* insert(notes, written);
            yield* yield* insert(fragments, written);
            assert.deepStrictEqual(
                (yield* yield* search(notes, 'vaults')).map(({ title }) => title),
                ['Concrete shell'],
            );
            assert.deepStrictEqual(
                (yield* yield* search(fragments, 'lulam')).map(({ title }) => title),
                ['Timber frame'],
            );
            yield* sql`UPDATE notes SET body = 'Steel cables' WHERE title = 'Concrete shell'`;
            assert.deepStrictEqual([(yield* yield* search(notes, 'vaults')).length, (yield* yield* search(notes, 'steel')).length], [0, 1]);
        }),
    );
    define('scopes tenant rows through row-level security on PostgreSQL', (_, grant, sql) =>
        sql.onDialectOrElse({
            pg: () =>
                Effect.gen(function* () {
                    const [first, second] = ['019a0000-0000-7000-8000-00000000000a', '019a0000-0000-7000-8000-00000000000b'];
                    const scoped = <A, E>(id: string, effect: Effect.Effect<A, E, SqlClient.SqlClient>) => tenant(id, Effect.andThen(sql`SET LOCAL ROLE rasm_runtime`, effect));
                    yield* sql`DO $$ BEGIN CREATE ROLE rasm_runtime NOLOGIN; EXCEPTION WHEN duplicate_object THEN NULL; END $$`;
                    yield* grant(declared(ledgers), 'rasm_runtime');
                    yield* scoped(first, Effect.flatten(insert(ledgers, [{ id: '019a0000-0000-7000-8000-000000000001', tenant: first, amount: 1 }])));
                    yield* scoped(second, Effect.flatten(insert(ledgers, [{ id: '019a0000-0000-7000-8000-000000000002', tenant: second, amount: 2 }])));
                    assert.deepStrictEqual(yield* scoped(first, sql`SELECT amount FROM ledgers`), [{ amount: 1 }]);
                    assert.isTrue((yield* Effect.flip(scoped(first, Effect.flatten(insert(ledgers, [{ id: '019a0000-0000-7000-8000-000000000003', tenant: second, amount: 1 }])))))._tag === 'SqlError');
                }),
            orElse: () => Effect.void,
        }),
    );
    define('moves a declared retype of 32-bit and text JSON columns to the current types with every row', (apply, _, sql) =>
        sql.onDialectOrElse({
            pg: () =>
                Effect.gen(function* () {
                    yield* sql`CREATE TABLE ${sql('legacy')} (id text NOT NULL PRIMARY KEY, count integer NOT NULL, body text NOT NULL CHECK (body IS JSON))`;
                    yield* sql`INSERT INTO legacy ${sql.insert({ id: 'l', count: 7, body: '{"camelKey":1}' })}`;
                    yield* apply(declared(legacy));
                    assert.deepStrictEqual(yield* SqlSchema.findAll({ Request: Schema.Void, Result: legacy, execute: () => sql`SELECT * FROM legacy` })(undefined), [{ id: 'l', count: 7, body: { camelKey: 1 } }]);
                    assert.deepStrictEqual(yield* sql`SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'legacy' AND column_name <> 'id' ORDER BY 1`, [
                        { columnName: 'body', dataType: 'jsonb' },
                        { columnName: 'count', dataType: 'bigint' },
                    ]);
                }),
            orElse: () => Effect.void,
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { cases };
