import { assert, type Vitest } from '@effect/vitest';
import { apply, atomic, Bytes, type Catalog, plan, type Relation, store, table, tenant, transforms, Uuid } from '@rasm/database';
import { Array, DateTime, Effect, Exit, Option, Schema, type Scope } from 'effect';
import { SqlClient } from 'effect/sql';

// --- [TYPES] ---------------------------------------------------------------------------

type Client<F, Q> = Parameters<typeof store<never, F, Q>>[1];

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
    { id: Uuid, email: Schema.String, deletedAt: Schema.NullOr(Schema.DateTimeUtcFromString), profile: Schema.fromJsonString(Schema.Struct({ plan: Schema.String })), plan: Schema.String },
    {
        projections: { plan: { from: 'profile', path: ['plan'] } },
        indexes: [
            { columns: [{ casefold: 'email' }], unique: true, where: { deletedAt: null } },
            { columns: ['plan'], include: ['email'] },
            { columns: ['profile'], method: 'gin' },
        ],
    },
);
const notes = table('notes', { id: Schema.Int, title: Schema.String, body: Schema.String }, { search: { columns: ['title', 'body'], tokenizer: { configuration: 'simple', filters: ['unaccent'], tokenize: 'unicode61 remove_diacritics 2' } } });
const fragments = table('fragments', notes.fields, { search: { columns: ['title', 'body'], tokenizer: 'trigram' } });
const textNotes = table('notes', { ...notes.fields, id: Uuid }, { search: { columns: ['title', 'body'], tokenizer: { configuration: 'simple', filters: ['unaccent'], tokenize: 'unicode61 remove_diacritics 2' } } });
const ledgers = table('ledgers', { id: Uuid, tenant: Schema.String, amount: Schema.Int }, { tenant: 'tenant' });
const legacy = table('legacy', { id: Schema.String, count: Schema.Int, body: Schema.fromJsonString(Schema.Struct({ camelKey: Schema.Finite })) }, { retype: ['count', 'body'] });

// --- [OPERATIONS] ----------------------------------------------------------------------

const cases = <R, F>(it: Vitest.MethodsNonLive<R>, client: Client<F, NoInfer<R>>, record: (title: string, planned: Effect.Success<ReturnType<typeof plan>>) => Effect.Effect<void, never, NoInfer<R>>) => {
    const declared = (...tables: Array.NonEmptyReadonlyArray<Relation>): Catalog => ({ tables, retired: [] });
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
            apply(catalog, runtime),
        );
    const define = <A, E>(title: string, body: (applyCatalog: (catalog: Catalog) => ReturnType<typeof step>, grant: (catalog: Catalog, role: string) => ReturnType<typeof step>, sql: SqlClient.SqlClient) => Effect.Effect<A, E, SqlClient.SqlClient | Scope.Scope | R>) => {
        const [applyCatalog, grant] = [(catalog: Catalog) => step(title, catalog, Option.none()), (catalog: Catalog, role: string) => step(title, catalog, Option.some(role))];
        return it.effect(title, () =>
            Effect.provide(
                Effect.flatMap(SqlClient.SqlClient, (sql) => body(applyCatalog, grant, sql)),
                client(transforms),
            ),
        );
    };
    const seeded = Effect.fn('seeded')(function* (applyCatalog: (catalog: Catalog) => ReturnType<typeof step>) {
        const sql = yield* SqlClient.SqlClient;
        yield* applyCatalog(declared(parents, children));
        yield* atomic([
            sql`INSERT INTO parents ${sql.insert({ id: 'p', label: 'Parent', state: 'open' })}`,
            sql`INSERT INTO children ${sql.insert([
                { id: 'a', parentId: 'p', rank: 1 },
                { id: 'b', parentId: 'p', rank: 2 },
            ])}`,
        ]);
    });
    it.effect('maps camelCase fields to snake_case columns in every statement form', () =>
        Effect.gen(function* () {
            const sql = yield* SqlClient.SqlClient;
            yield* atomic([sql`INSERT INTO parents ${sql.insert({ id: 'p', label: 'Parent', state: 'open' })}`, sql`INSERT INTO children ${sql.insert({ id: 'a', parentId: 'p', rank: 1 })}`]);
            yield* sql`UPDATE children SET ${sql.update({ parentId: 'p', rank: 2 })} WHERE ${sql('parentId')} = ${'p'}`;
            assert.deepStrictEqual(yield* children.select('parentId', ['p']), [{ id: 'a', parentId: 'p', rank: 2 }]);
            assert.deepStrictEqual(yield* sql.withoutTransforms()`SELECT parent_id FROM children`, [{ parent_id: 'p' }]);
        }).pipe(Effect.provide(store(declared(parents, children), client))),
    );
    define('creates declared tables with enforced references and decoded rows', (applyCatalog, _, sql) =>
        Effect.gen(function* () {
            yield* seeded(applyCatalog);
            assert.deepStrictEqual(yield* children.select('parentId', ['p']), [
                { id: 'a', parentId: 'p', rank: 1 },
                { id: 'b', parentId: 'p', rank: 2 },
            ]);
            assert.isTrue((yield* Effect.flip(sql`INSERT INTO children ${sql.insert({ id: 'c', parentId: 'missing', rank: 3 })}`))._tag === 'SqlError');
        }),
    );
    define('renames declared tables and columns keeping rows', (applyCatalog) =>
        Effect.gen(function* () {
            yield* seeded(applyCatalog);
            assert.deepStrictEqual(yield* applyCatalog(declared(renamed, kids)), { tables: [], columns: [], indexes: [], references: [] });
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', title: 'Parent', state: 'open' }]);
            assert.deepStrictEqual((yield* rows('kids')).length, 2);
            assert.isTrue((yield* Effect.flip(rows('children')))._tag === 'SqlError');
        }),
    );
    define('renames back keeping rows when older catalog runs after rename', (applyCatalog, _, sql) =>
        Effect.gen(function* () {
            yield* seeded(applyCatalog);
            yield* applyCatalog(declared(renamed, kids));
            yield* sql`INSERT INTO parents ${sql.insert({ id: 'q', title: 'Second', state: 'closed' })}`;
            yield* applyCatalog(declared(parents, children));
            assert.deepStrictEqual(yield* rows('parents'), [
                { id: 'p', label: 'Parent', state: 'open' },
                { id: 'q', label: 'Second', state: 'closed' },
            ]);
            assert.deepStrictEqual((yield* rows('children')).length, 2);
            yield* applyCatalog(declared(renamed, kids));
            assert.deepStrictEqual(
                (yield* rows('parents')).map(({ title }) => title),
                ['Parent', 'Second'],
            );
        }),
    );
    define('keeps newer column and data when older catalog reconciles', (applyCatalog, _, sql) =>
        Effect.gen(function* () {
            yield* seeded(applyCatalog);
            yield* applyCatalog(declared(extended, kids));
            yield* sql`UPDATE parents SET ${sql.update({ extra: 'kept' })} WHERE id = 'p'`;
            assert.deepStrictEqual(yield* applyCatalog(declared(renamed, kids)), { tables: [], columns: [{ table: 'parents', column: 'extra' }], indexes: [], references: [] });
            yield* sql`INSERT INTO parents ${sql.insert({ id: 'q', title: 'Second', state: 'closed' })}`;
            yield* applyCatalog(declared(extended, kids));
            assert.deepStrictEqual(yield* rows('parents'), [
                { id: 'p', title: 'Parent', state: 'open', extra: 'kept' },
                { id: 'q', title: 'Second', state: 'closed', extra: null },
            ]);
        }),
    );
    define('drops retired column and table and retypes column keeping rows', (applyCatalog) =>
        Effect.gen(function* () {
            yield* seeded(applyCatalog);
            yield* applyCatalog(declared(extended, kids));
            yield* applyCatalog(declared(retired, retyped));
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', title: 'Parent', state: 'open' }]);
            assert.deepStrictEqual(
                (yield* rows('kids')).map(({ rank }) => rank),
                [1, 2],
            );
            yield* applyCatalog({ tables: [retired], retired: ['kids'] });
            assert.isTrue((yield* Effect.flip(rows('kids')))._tag === 'SqlError');
        }),
    );
    define('keeps re-added column with retired name when retiring catalog runs again', (applyCatalog, _, sql) =>
        Effect.gen(function* () {
            yield* seeded(applyCatalog);
            yield* applyCatalog(declared(extended, kids));
            yield* sql`UPDATE parents SET ${sql.update({ extra: 'old' })} WHERE id = 'p'`;
            yield* applyCatalog(declared(revived, kids));
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', title: 'Parent', state: 'open', extra: null }]);
            yield* sql`UPDATE parents SET ${sql.update({ extra: 7 })} WHERE id = 'p'`;
            assert.deepStrictEqual(yield* applyCatalog(declared(retired, kids)), { tables: [], columns: [{ table: 'parents', column: 'extra' }], indexes: [], references: [] });
            assert.deepStrictEqual(yield* applyCatalog(declared(extended, kids)), { tables: [], columns: [], indexes: [], references: [] });
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', title: 'Parent', state: 'open', extra: 7 }]);
        }),
    );
    define('plans nothing when store matches catalog', (applyCatalog) =>
        Effect.gen(function* () {
            const full = declared(extended, kids, accounts, events, notes, values, blobs);
            yield* seeded(applyCatalog);
            yield* applyCatalog(full);
            const before = yield* shape;
            const planned = yield* Effect.all([plan(full, Option.none()), plan(declared(renamed, kids), Option.none())]);
            assert.deepStrictEqual(
                planned.flatMap(({ statements, concurrent }) => [statements, concurrent]),
                [[], [], [], []],
            );
            yield* applyCatalog(full);
            assert.deepStrictEqual(yield* shape, before);
        }),
    );
    define('finishes concurrent reconciles of one catalog', () =>
        Effect.gen(function* () {
            yield* Effect.all([apply(declared(parents, children), Option.none()), apply(declared(parents, children), Option.none())], { concurrency: 'unbounded' });
            assert.deepStrictEqual(yield* rows('children'), []);
        }),
    );
    define('keeps rows and columns when concurrent catalogs rebuild and extend one table', (applyCatalog) =>
        Effect.gen(function* () {
            yield* seeded(applyCatalog);
            const [extension, rebuild] = yield* Effect.all([Effect.exit(apply(declared(annotated, children), Option.none())), Effect.exit(apply(declared(reopened, children), Option.none()))], { concurrency: 'unbounded' });
            assert.isTrue(Exit.isSuccess(rebuild) || Exit.isSuccess(extension));
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', label: 'Parent', state: 'open', ...(Exit.isSuccess(extension) ? { note: null } : {}) }]);
            assert.deepStrictEqual((yield* rows('children')).length, 2);
        }),
    );
    define('refuses added required column without default on every store, tenant tables and non-integer search keys outside PostgreSQL', (applyCatalog, _, sql) =>
        Effect.gen(function* () {
            yield* seeded(applyCatalog);
            const refused = (catalog: Catalog) => Effect.map(Effect.flip(apply(catalog, Option.none())), (error) => error._tag === 'ReconcileError' && error.findings);
            const unsupported = yield* sql.onDialectOrElse({ pg: () => Effect.succeed([]), orElse: () => refused(declared(ledgers, textNotes)) });
            assert.deepStrictEqual(yield* refused(declared(required, children)), [{ _tag: 'Required', table: 'parents', columns: ['owner'] }]);
            assert.deepStrictEqual(
                unsupported,
                sql.onDialectOrElse({
                    pg: () => [],
                    orElse: () => [
                        { _tag: 'Unsupported', table: 'ledgers', reason: 'tenant', columns: ['tenant'] },
                        { _tag: 'Unsupported', table: 'notes', reason: 'search', columns: ['id'] },
                    ],
                }),
            );
        }),
    );
    define('stores 64-bit integers, JSON keys, bytes, instants, booleans, options, and bounded values', (applyCatalog, _, sql) =>
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
            yield* applyCatalog(declared(values, blobs));
            assert.deepStrictEqual(yield* values.insert(written), written);
            assert.deepStrictEqual(yield* values.select('id', [Array.headNonEmpty(written).id]), [Array.headNonEmpty(written)]);
            assert.deepStrictEqual(yield* values.select('count', [Array.lastNonEmpty(written).count]), [Array.lastNonEmpty(written)]);
            yield* blobs.insert(digests);
            assert.deepStrictEqual(yield* blobs.select('digest', [Array.lastNonEmpty(digests).digest]), [Array.lastNonEmpty(digests)]);
            assert.deepStrictEqual((yield* blobs.remove('digest', [Array.headNonEmpty(digests).digest])).length, 1);
            assert.isTrue((yield* Effect.flip(sql`UPDATE ${sql('values')} SET ratio = 2`))._tag === 'SqlError');
            assert.isTrue((yield* Effect.flip(sql`UPDATE ${sql('values')} SET code = 'abc'`))._tag === 'SqlError');
        }),
    );
    define('writes and reads collections as one parameter with upsert, remove, and latest per key', (applyCatalog) =>
        Effect.gen(function* () {
            const volume = 150;
            const tenants = [2, 0, 1] as const;
            yield* applyCatalog(declared(events));
            const written = Array.makeBy(volume, (index) => ({ id: `e${index}`, tenant: index % tenants.length, at: DateTime.add(DateTime.makeUnsafe('2026-01-01T00:00:00.000Z'), { seconds: index }), kind: 'view' as const }));
            assert.deepStrictEqual((yield* events.insert(written)).length, volume);
            assert.deepStrictEqual(yield* events.upsert(written), []);
            assert.deepStrictEqual(
                (yield* events.upsert([{ ...written[0], kind: 'buy' }])).map(({ kind }) => kind),
                ['buy'],
            );
            assert.deepStrictEqual(
                (yield* events.latest('tenant', ['at'], tenants, 1)).map(({ id }) => id),
                ['e149', 'e147', 'e148'],
            );
            assert.deepStrictEqual((yield* events.remove('tenant', [0])).length, volume / tenants.length);
            assert.deepStrictEqual((yield* events.select('tenant', tenants)).length, volume - volume / tenants.length);
        }),
    );
    define('enforces partial case-insensitive unique keys and reads virtual columns from JSON', (applyCatalog) =>
        Effect.gen(function* () {
            yield* applyCatalog(declared(accounts));
            const account = (id: string, email: string, deleted: boolean) => ({ id, email, deletedAt: deleted ? DateTime.makeUnsafe('2026-10-17T00:00:00.000Z') : null, profile: { plan: 'pro' } });
            yield* accounts.insert([account('019a0000-0000-7000-8000-000000000001', 'A@example.com', true), account('019a0000-0000-7000-8000-000000000002', 'a@example.com', false)]);
            assert.isTrue((yield* Effect.flip(accounts.insert([account('019a0000-0000-7000-8000-000000000003', 'A@EXAMPLE.COM', false)])))._tag === 'SqlError');
            assert.deepStrictEqual(
                (yield* accounts.select('email', ['a@example.com'])).map(({ plan: tier }) => tier),
                ['pro'],
            );
        }),
    );
    define('searches declared text columns by word and trigram across updates', (applyCatalog, _, sql) =>
        Effect.gen(function* () {
            yield* applyCatalog(declared(notes, fragments));
            const written = Array.make({ id: 1, title: 'Concrete shell', body: 'Thin vaults' }, { id: 2, title: 'Timber frame', body: 'Glulam beams' }, { id: 3, title: 'Café crème', body: 'Voûte mince' });
            yield* notes.insert(written);
            yield* fragments.insert(written);
            assert.deepStrictEqual(
                (yield* notes.search('vaults', 10)).map(({ title }) => title),
                ['Concrete shell'],
            );
            assert.deepStrictEqual(
                (yield* notes.search('cafe CREME', 10, { open: '[', close: ']' })).map(({ highlight }) => highlight),
                [Option.some({ title: '[Café] [crème]', body: 'Voûte mince' })],
            );
            assert.deepStrictEqual(
                (yield* fragments.search('lulam', 10)).map(({ title }) => title),
                ['Timber frame'],
            );
            yield* sql`UPDATE notes SET body = 'Steel cables' WHERE title = 'Concrete shell'`;
            assert.deepStrictEqual([(yield* notes.search('vaults', 10)).length, (yield* notes.search('steel', 10)).length], [0, 1]);
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
                    yield* scoped(first, ledgers.insert([{ id: '019a0000-0000-7000-8000-000000000001', tenant: first, amount: 1 }]));
                    yield* scoped(second, ledgers.insert([{ id: '019a0000-0000-7000-8000-000000000002', tenant: second, amount: 2 }]));
                    assert.deepStrictEqual(yield* scoped(first, sql`SELECT amount FROM ledgers`), [{ amount: 1 }]);
                    assert.isTrue((yield* Effect.flip(scoped(first, ledgers.insert([{ id: '019a0000-0000-7000-8000-000000000003', tenant: second, amount: 1 }]))))._tag === 'SqlError');
                }),
            orElse: () => Effect.void,
        }),
    );
    define('retypes integer and text JSON columns to bigint and jsonb keeping rows', (applyCatalog, _, sql) =>
        sql.onDialectOrElse({
            pg: () =>
                Effect.gen(function* () {
                    yield* sql`CREATE TABLE ${sql('legacy')} (id text NOT NULL PRIMARY KEY, count integer NOT NULL, body text NOT NULL CHECK (body IS JSON))`;
                    yield* sql`INSERT INTO legacy ${sql.insert({ id: 'l', count: 7, body: '{"camelKey":1}' })}`;
                    yield* applyCatalog(declared(legacy));
                    assert.deepStrictEqual(yield* legacy.select('id', ['l']), [{ id: 'l', count: 7, body: { camelKey: 1 } }]);
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
