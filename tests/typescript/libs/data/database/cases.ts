import { D1Client } from '@effect/sql-d1';
import { assert, type Vitest } from '@effect/vitest';
import { Catalog } from '@rasm/database/catalog';
import { Column } from '@rasm/database/column';
import { Relation } from '@rasm/database/relation';
import { atomic } from '@rasm/database/transaction';
import { Array, BigDecimal, DateTime, Effect, Equal, Exit, Fiber, Option, Predicate, Schema, type Scope, String, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { SqlClient, SqlError } from 'effect/sql';

// --- [TYPES] ---------------------------------------------------------------------------

type Client<F, Q> = Parameters<typeof Catalog.layer<never, F, Q>>[1];

// --- [MODELS] --------------------------------------------------------------------------

const parents = Relation.make('parents', { id: Schema.String, label: Schema.String, state: Schema.Literals(['open', 'closed']) }, {});
const children = Relation.make('children', { id: Schema.String, parentId: Schema.String, rank: Schema.Int }, { references: [{ columns: ['parentId'], table: parents, onDelete: 'CASCADE' }] });
const renamed = Relation.make('parents', { id: Schema.String, title: Schema.String, state: parents.schema.fields.state }, { renamed: { title: 'label' }, indexes: [{ columns: ['title'], unique: true }] });
const extended = Relation.make('parents', { ...renamed.schema.fields, extra: Schema.NullOr(Schema.String) }, { renamed: { title: 'label' }, indexes: [{ columns: ['title'], unique: true }] });
const retired = Relation.make('parents', renamed.schema.fields, { renamed: { title: 'label' }, indexes: [{ columns: ['title'], unique: true }], retired: ['extra'] });
const revived = Relation.make('parents', { ...renamed.schema.fields, extra: Schema.NullOr(Schema.Int) }, { renamed: { title: 'label' }, indexes: [{ columns: ['title'], unique: true }], retired: ['extra'] });
const kids = Relation.make('kids', children.schema.fields, { renamedFrom: 'children', references: [{ columns: ['parentId'], table: renamed, onDelete: 'CASCADE' }] });
const retyped = Relation.make('kids', { ...children.schema.fields, rank: Schema.Finite }, { renamedFrom: 'children', references: [{ columns: ['parentId'], table: renamed, onDelete: 'CASCADE' }], retype: ['rank'] });
const reopened = Relation.make('parents', { ...parents.schema.fields, state: Schema.Literals(['open', 'closed', 'archived']) }, { retype: ['state'] });
const annotated = Relation.make('parents', { ...parents.schema.fields, note: Schema.NullOr(Schema.String) }, {});
const required = Relation.make('parents', { ...parents.schema.fields, owner: Schema.String }, {});
const defaulted = Relation.make('parents', required.schema.fields, { defaults: { owner: 'nobody' } });
const relabeled = Relation.make('parents', { ...parents.schema.fields, label: Schema.Int }, {});
const reserved = Relation.make('reserved', { id: Schema.String, rowid: Schema.Int }, {});
const slugs = Relation.make('slugs', { body: Schema.fromJsonString(Schema.Struct({ slug: Schema.String })), slug: Schema.String }, { key: ['slug'], projections: { slug: { from: 'body', path: ['slug'] } } });
const values = Relation.make(
    'values',
    {
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
    },
    {},
);
const blobs = Relation.make('blobs', { digest: Column.Bytes, size: Schema.Int }, { key: ['digest'] });
const events = Relation.make('events', { id: Schema.String, tenant: Schema.Int, at: Schema.DateTimeUtcFromString, kind: Schema.Literals(['view', 'buy']) }, { indexes: [{ columns: ['tenant', 'at'] }] });
const accounts = Relation.make(
    'accounts',
    { id: Column.Uuid, email: Schema.String, deletedAt: Schema.NullOr(Schema.DateTimeUtcFromString), profile: Schema.fromJsonString(Schema.Struct({ plan: Schema.String })), plan: Schema.String, active: Schema.BooleanFromBit },
    {
        projections: { plan: { from: 'profile', path: ['plan'] }, active: { where: { deletedAt: null } } },
        indexes: [
            { columns: [{ casefold: 'email' }], unique: true, where: { deletedAt: null } },
            { columns: ['plan'], include: ['email'] },
            { columns: ['profile'], method: 'gin' },
        ],
    },
);
const notes = Relation.make('notes', { id: Schema.Int, title: Schema.String, body: Schema.String }, { search: { columns: ['title', 'body'], tokenizer: { configuration: 'simple', filters: ['unaccent'], tokenize: 'unicode61 remove_diacritics 2' } } });
const fragments = Relation.make('fragments', notes.schema.fields, { search: { columns: ['title', 'body'], tokenizer: 'trigram' } });
const textNotes = Relation.make('notes', { ...notes.schema.fields, id: Column.Uuid }, { search: { columns: ['title', 'body'], tokenizer: { configuration: 'simple', filters: ['unaccent'], tokenize: 'unicode61 remove_diacritics 2' } } });
const ledgers = Relation.make('ledgers', { id: Column.Uuid, tenant: Schema.String, amount: Schema.Int }, { tenant: { column: 'tenant', setting: 'app.tenant' } });
const legacy = Relation.make('legacy', { id: Schema.String, count: Schema.Int, body: Schema.fromJsonString(Schema.Struct({ camelKey: Schema.Finite })) }, { retype: ['count', 'body'] });

// --- [OPERATIONS] ----------------------------------------------------------------------

const cases = <R, F, W>(it: Vitest.MethodsNonLive<R>, client: Client<F, NoInfer<R>>, record: (title: string, planned: Effect.Success<ReturnType<typeof Catalog.plan>>) => Effect.Effect<void, W, NoInfer<R>>) => {
    const declared = (relations: Catalog['tables'], catalog: Pick<Catalog, 'retired'> = {}) => new Catalog({ tables: relations, ...catalog, excluded: ['_cf_'], timeouts: { lock: '5 seconds', transaction: '2 minutes' } });
    const uuid = SecureRandom.SecureRandom.use(({ uuidV7 }) => uuidV7());
    const rows = (name: string) => SqlClient.SqlClient.use((sql) => sql`SELECT * FROM ${sql(name)} ORDER BY 1`);
    const tables = SqlClient.SqlClient.use((sql) =>
        sql.onDialectOrElse({
            pg: () => sql`SELECT table_name AS name FROM information_schema.tables WHERE table_schema = current_schema() ORDER BY 1`,
            orElse: () => sql`SELECT name FROM pragma_table_list WHERE schema = 'main' AND type = 'table' AND name NOT GLOB '_cf_*' AND name NOT GLOB 'sqlite_*' ORDER BY 1`,
        }),
    );
    const shape = SqlClient.SqlClient.use((sql) =>
        sql.onDialectOrElse({
            pg: () => sql`SELECT table_name AS name, concat_ws(' ', column_name, data_type, is_nullable, column_default) AS definition FROM information_schema.columns WHERE table_schema = current_schema()
                UNION ALL SELECT tablename, indexdef FROM pg_indexes WHERE schemaname = current_schema()
                UNION ALL SELECT conrelid::regclass::text, conname || ' ' || pg_get_constraintdef(oid) FROM pg_constraint WHERE connamespace = current_schema()::regnamespace
                UNION ALL SELECT c.relname, d.objsubid || ' ' || d.description FROM pg_description d JOIN pg_class c ON c.oid = d.objoid WHERE c.relnamespace = current_schema()::regnamespace
                ORDER BY 1, 2`,
            orElse: () => sql`SELECT name, sql AS definition FROM sqlite_schema WHERE name NOT GLOB '_cf_*' ORDER BY name`,
        }),
    );
    const findings = (catalog: Catalog) =>
        Catalog.reconcile(catalog).pipe(
            Effect.as([]),
            Effect.catchTag('ReconcileError', ({ findings: found }) => Effect.succeed(found)),
        );
    const refused = <A, E, Q>(effect: Effect.Effect<A, E, Q>, reason: SqlError.SqlErrorReason['_tag']) =>
        Effect.all([Effect.flip(effect), Effect.serviceOption(D1Client.D1Client)]).pipe(Effect.map(([error, d1]) => assert.strictEqual(SqlError.isSqlError(error) ? error.reason._tag : error, Option.match(d1, { onNone: () => reason, onSome: () => 'UnknownError' }))));
    const reconciling =
        (title: string) =>
        (catalog: Catalog, runtime: Option.Option<string> = Option.none()) =>
            Effect.andThen(
                Effect.flatMap(Catalog.plan(catalog, runtime), (planned) => record(title, planned)),
                Catalog.reconcile(catalog, runtime),
            );
    const isolated = <Eff extends Effect.Effect<unknown, unknown, SqlClient.SqlClient | SecureRandom.SecureRandom | Scope.Scope | R>, A>(title: string, body: (reconcile: ReturnType<typeof reconciling>, sql: SqlClient.SqlClient) => Generator<Eff, A, never>) =>
        Effect.provide(
            Effect.flatMap(SqlClient.SqlClient, (sql) => Effect.fnUntraced(body)(reconciling(title), sql)),
            [client(Column.transforms), SecureRandom.layerWebCrypto],
        );
    const seeded = Effect.fn('seeded')(function* (reconcile: ReturnType<typeof reconciling>) {
        const sql = yield* SqlClient.SqlClient;
        yield* reconcile(declared([parents, children]));
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
            assert.deepStrictEqual(yield* Relation.select(children, 'parentId', ['p']), [{ id: 'a', parentId: 'p', rank: 2 }]);
            assert.deepStrictEqual(yield* sql.withoutTransforms()`SELECT parent_id FROM children`, [{ parent_id: 'p' }]);
        }).pipe(Effect.provide(Catalog.layer(declared([parents, children]), client))),
    );
    it.effect('creates declared tables with enforced references and decoded rows', ({ task }) =>
        isolated(task.name, function* (reconcile, sql) {
            yield* seeded(reconcile);
            assert.deepStrictEqual(yield* tables, [{ name: 'children' }, { name: 'parents' }]);
            assert.deepStrictEqual(yield* Relation.select(children, 'parentId', ['p']), [
                { id: 'a', parentId: 'p', rank: 1 },
                { id: 'b', parentId: 'p', rank: 2 },
            ]);
            yield* refused(sql`INSERT INTO children ${sql.insert({ id: 'c', parentId: 'missing', rank: 3 })}`, 'ConstraintError');
            yield* sql`DELETE FROM parents WHERE id = 'p'`;
            assert.deepStrictEqual(yield* rows('children'), []);
        }),
    );
    it.effect('renames declared tables and columns keeping rows', ({ task }) =>
        isolated(task.name, function* (reconcile) {
            yield* seeded(reconcile);
            assert.deepStrictEqual(yield* reconcile(declared([renamed, kids])), { tables: [], columns: [], indexes: [], references: [] });
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', title: 'Parent', state: 'open' }]);
            assert.deepStrictEqual((yield* rows('kids')).length, 2);
            assert.deepStrictEqual(yield* tables, [{ name: 'kids' }, { name: 'parents' }]);
        }),
    );
    it.effect('renames back keeping rows when older catalog runs after rename', ({ task }) =>
        isolated(task.name, function* (reconcile, sql) {
            yield* seeded(reconcile);
            yield* reconcile(declared([renamed, kids]));
            yield* sql`INSERT INTO parents ${sql.insert({ id: 'q', title: 'Second', state: 'closed' })}`;
            yield* reconcile(declared([parents, children]));
            assert.deepStrictEqual(yield* rows('parents'), [
                { id: 'p', label: 'Parent', state: 'open' },
                { id: 'q', label: 'Second', state: 'closed' },
            ]);
            assert.deepStrictEqual((yield* rows('children')).length, 2);
            yield* reconcile(declared([renamed, kids]));
            assert.deepStrictEqual(
                (yield* rows('parents')).map(({ title }) => title),
                ['Parent', 'Second'],
            );
            assert.deepStrictEqual(yield* tables, [{ name: 'kids' }, { name: 'parents' }]);
        }),
    );
    it.effect('keeps newer column and data when older catalog reconciles', ({ task }) =>
        isolated(task.name, function* (reconcile, sql) {
            yield* seeded(reconcile);
            yield* reconcile(declared([extended, kids]));
            yield* sql`UPDATE parents SET ${sql.update({ extra: 'kept' })} WHERE id = 'p'`;
            assert.deepStrictEqual(yield* reconcile(declared([renamed, kids])), { tables: [], columns: [{ table: 'parents', column: 'extra' }], indexes: [], references: [] });
            yield* sql`INSERT INTO parents ${sql.insert({ id: 'q', title: 'Second', state: 'closed' })}`;
            yield* reconcile(declared([extended, kids]));
            assert.deepStrictEqual(yield* rows('parents'), [
                { id: 'p', title: 'Parent', state: 'open', extra: 'kept' },
                { id: 'q', title: 'Second', state: 'closed', extra: null },
            ]);
        }),
    );
    it.effect('drops retired column and table and retypes column keeping rows', ({ task }) =>
        isolated(task.name, function* (reconcile) {
            yield* seeded(reconcile);
            yield* reconcile(declared([extended, kids]));
            yield* reconcile(declared([retired, retyped]));
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', title: 'Parent', state: 'open' }]);
            assert.deepStrictEqual(
                (yield* rows('kids')).map(({ rank }) => rank),
                [1, 2],
            );
            yield* reconcile(declared([retired], { retired: ['kids'] }));
            assert.deepStrictEqual(yield* tables, [{ name: 'parents' }]);
        }),
    );
    it.effect('keeps re-added column with retired name when retiring catalog runs again', ({ task }) =>
        isolated(task.name, function* (reconcile, sql) {
            yield* seeded(reconcile);
            yield* reconcile(declared([extended, kids]));
            yield* sql`UPDATE parents SET ${sql.update({ extra: 'old' })} WHERE id = 'p'`;
            yield* reconcile(declared([revived, kids]));
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', title: 'Parent', state: 'open', extra: null }]);
            yield* sql`UPDATE parents SET ${sql.update({ extra: 7 })} WHERE id = 'p'`;
            assert.deepStrictEqual(yield* reconcile(declared([retired, kids])), { tables: [], columns: [{ table: 'parents', column: 'extra' }], indexes: [], references: [] });
            assert.deepStrictEqual(yield* reconcile(declared([extended, kids])), { tables: [], columns: [], indexes: [], references: [] });
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', title: 'Parent', state: 'open', extra: 7 }]);
        }),
    );
    it.effect('plans nothing when store matches catalog', ({ task }) =>
        isolated(task.name, function* (reconcile) {
            const full = declared([extended, kids, accounts, events, notes, values, blobs]);
            yield* seeded(reconcile);
            yield* reconcile(full);
            const before = yield* shape;
            const planned = yield* Effect.all([Catalog.plan(full), Catalog.plan(declared([renamed, kids]))]);
            assert.deepStrictEqual(
                planned.flatMap(({ statements, concurrent }) => [statements, concurrent]),
                [[], [], [], []],
            );
            yield* reconcile(full);
            assert.deepStrictEqual(yield* shape, before);
        }),
    );
    it.effect('finishes concurrent reconciles of one catalog after an interrupted one', ({ task }) =>
        isolated(task.name, function* () {
            const catalog = declared([parents, children]);
            const interrupted = yield* Effect.forkChild(Catalog.reconcile(catalog));
            yield* Effect.yieldNow;
            yield* Fiber.interrupt(interrupted);
            yield* Effect.all([Catalog.reconcile(catalog), Catalog.reconcile(catalog)], { concurrency: 'unbounded' });
            assert.deepStrictEqual(yield* Effect.map(Catalog.plan(catalog), Struct.pick(['statements', 'concurrent'])), { statements: [], concurrent: [] });
            assert.deepStrictEqual(yield* rows('children'), []);
        }),
    );
    it.effect('keeps rows and columns when concurrent catalogs rebuild and extend one table', ({ task }) =>
        isolated(task.name, function* (reconcile) {
            yield* seeded(reconcile);
            const [extension, rebuild] = yield* Effect.all([Effect.exit(Catalog.reconcile(declared([annotated, children]))), Effect.exit(Catalog.reconcile(declared([reopened, children])))], { concurrency: 'unbounded' });
            assert.isTrue(Exit.isSuccess(rebuild) || Exit.isSuccess(extension));
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', label: 'Parent', state: 'open', ...(Exit.isSuccess(extension) ? { note: null } : {}) }]);
            assert.deepStrictEqual((yield* rows('children')).length, 2);
        }),
    );
    it.effect('refuses undeclared retypes, reserved and projected keys and required columns without default, filling existing rows from a declared default', ({ task }) =>
        isolated(task.name, function* (reconcile, sql) {
            yield* seeded(reconcile);
            assert.deepStrictEqual(yield* Effect.all([findings(declared([required, children])), findings(declared([relabeled, children])), findings(declared([reserved, slugs])), findings(declared([textNotes]))]), [
                [{ _tag: 'Required', table: 'parents', columns: ['owner'] }],
                [{ _tag: 'Retype', table: 'parents', columns: ['label'] }],
                [
                    { _tag: 'Unsupported', table: 'reserved', reason: 'reserved', columns: ['rowid'] },
                    { _tag: 'Unsupported', table: 'slugs', reason: 'projected', columns: ['slug'] },
                ],
                sql.onDialectOrElse({ pg: () => [], orElse: () => [{ _tag: 'Unsupported', table: 'notes', reason: 'search', columns: ['id'] }] as const }),
            ]);
            yield* reconcile(declared([defaulted, children]));
            assert.deepStrictEqual(yield* rows('parents'), [{ id: 'p', label: 'Parent', state: 'open', owner: 'nobody' }]);
        }),
    );
    it.effect('stores 64-bit integers, exact decimals, JSON keys, bytes, instants, booleans, options, and bounded values', ({ task }) =>
        isolated(task.name, function* (reconcile, sql) {
            const int64 = 9_223_372_036_854_775_808n;
            const byteMaximum = 255;
            const [first, second] = yield* Effect.all([uuid, uuid]);
            const written = Array.make(
                { id: first, count: int64 - 1n, amount: BigDecimal.fromStringUnsafe('12345678901234567890.123456789'), body: { camelKey: Math.PI }, bytes: Uint8Array.of(0, byteMaximum, 0), at: DateTime.makeUnsafe('2026-10-17T12:00:00.123Z'), flag: true, ratio: 0, code: '😀😀', note: Option.some('kept') },
                { id: second, count: -int64, amount: BigDecimal.fromStringUnsafe('-0.000000000000000001'), body: { camelKey: -Number.MIN_VALUE }, bytes: new Uint8Array(), at: DateTime.makeUnsafe('1969-07-20T20:17:40.500Z'), flag: false, ratio: 1, code: 'a', note: Option.none() },
            );
            const digests = Array.make({ digest: Uint8Array.of(byteMaximum, 0), size: 1 }, { digest: new Uint8Array(), size: 0 });
            yield* reconcile(declared([values, blobs]));
            const stored = Schema.encodeEffect(Schema.Array(values.schema));
            assert.deepStrictEqual(
                yield* Effect.all([Effect.flatMap(Relation.insert(values, written), stored), Effect.flatMap(Relation.select(values, 'id', [first]), stored), Effect.flatMap(Relation.select(values, 'count', [-int64]), stored)]),
                yield* Effect.all([stored(written), stored([Array.headNonEmpty(written)]), stored([Array.lastNonEmpty(written)])]),
            );
            yield* Relation.insert(blobs, digests);
            assert.deepStrictEqual(yield* Relation.select(blobs, 'digest', [Array.lastNonEmpty(digests).digest]), [Array.lastNonEmpty(digests)]);
            assert.deepStrictEqual(yield* Relation.remove(blobs, 'digest', [Array.headNonEmpty(digests).digest]), [Array.headNonEmpty(digests)]);
            assert.strictEqual((yield* Effect.flip(Relation.insert(values, [{ ...Array.headNonEmpty(written), id: yield* uuid, ratio: 2 }])))._tag, 'SchemaError');
            yield* refused(sql`UPDATE ${sql('values')} SET ${sql.update({ ratio: 2 })}`, 'ConstraintError');
            yield* refused(sql`UPDATE ${sql('values')} SET ${sql.update({ code: '😀😀😀' })}`, 'ConstraintError');
        }),
    );
    it.effect('writes and reads collections as one parameter with upsert, ensure, remove, and latest per key', ({ task }) =>
        isolated(task.name, function* (reconcile) {
            const volume = 3000;
            const tenants = [2, 0, 1] as const;
            const newest = 2;
            yield* reconcile(declared([events]));
            const written = Array.makeBy(volume, (index) => ({ id: `e${index}`, tenant: index % tenants.length, at: DateTime.add(DateTime.makeUnsafe('2026-01-01T00:00:00.000Z'), { seconds: index }), kind: 'view' as const }));
            const [head, last] = [Array.headNonEmpty(written), Array.lastNonEmpty(written)];
            const descending = (kept: Predicate.Predicate<number>) => Array.reverse(written.filter(({ tenant }) => kept(tenant))).map(Struct.get('id'));
            assert.deepStrictEqual(Array.sortWith(yield* Relation.insert(events, written), Struct.get('id'), String.Order), Array.sortWith(written, Struct.get('id'), String.Order));
            assert.deepStrictEqual(yield* Relation.upsert(events, written), []);
            assert.deepStrictEqual(yield* Relation.upsert(events, [{ ...head, kind: 'buy', at: DateTime.add(head.at, { days: 1 }) }], 'at'), [{ ...head, kind: 'buy', inserted: false }]);
            assert.strictEqual((yield* Effect.flip(Relation.upsert(events, [head, head])))._tag, 'SchemaError');
            assert.deepStrictEqual(
                (yield* Relation.latest(events, 'tenant', ['at'], tenants, newest)).map(Struct.get('id')),
                tenants.flatMap((tenant) => Array.take(descending(Equal.equals(tenant)), newest)),
            );
            assert.deepStrictEqual((yield* Relation.remove(events, 'tenant', [0])).length, volume / tenants.length);
            assert.deepStrictEqual((yield* Relation.select(events, 'tenant', tenants, { descending: 'at' })).map(Struct.get('id')), descending(Predicate.not(Equal.equals(0))));
            assert.deepStrictEqual(yield* Relation.ensure(events, [head, { ...last, kind: 'buy' }]), [head, last]);
        }),
    );
    it.effect('enforces partial case-insensitive unique keys and reads virtual columns from JSON and predicates', ({ task }) =>
        isolated(task.name, function* (reconcile) {
            yield* reconcile(declared([accounts]));
            const account = (id: string, email: string, deleted: boolean) => ({ id, email, deletedAt: deleted ? DateTime.makeUnsafe('2026-10-17T00:00:00.000Z') : null, profile: { plan: 'pro' } });
            yield* Relation.insert(accounts, [account(yield* uuid, 'A@example.com', true), account(yield* uuid, 'a@example.com', false)]);
            yield* refused(Relation.insert(accounts, [account(yield* uuid, 'A@EXAMPLE.COM', false)]), 'UniqueViolation');
            assert.deepStrictEqual((yield* Relation.select(accounts, 'email', ['a@example.com', 'A@example.com'], 'email')).map(Struct.pick(['email', 'plan', 'active'])), [
                { email: 'A@example.com', plan: 'pro', active: false },
                { email: 'a@example.com', plan: 'pro', active: true },
            ]);
        }),
    );
    it.effect('searches declared text columns by word and trigram across updates and escapes query syntax', ({ task }) =>
        isolated(task.name, function* (reconcile, sql) {
            yield* reconcile(declared([notes, fragments]));
            const written = Array.make({ id: 1, title: 'Concrete shell', body: 'Thin vaults' }, { id: 2, title: 'Timber frame', body: 'Glulam beams' }, { id: 3, title: 'Café crème', body: 'Voûte mince' });
            const titles = Effect.map((found: readonly { readonly title: string }[]) => found.map(Struct.get('title')));
            yield* Relation.insert(notes, written);
            yield* Relation.insert(fragments, written);
            assert.deepStrictEqual(yield* Effect.all([...['vaults', '"vaults'].map((text) => titles(Relation.search(notes, text, written.length))), ...['lulam', 'am', 'é', '%', '_'].map((text) => titles(Relation.search(fragments, text, written.length)))]), [
                ['Concrete shell'],
                ['Concrete shell'],
                ['Timber frame'],
                ['Timber frame'],
                ['Café crème'],
                [],
                [],
            ]);
            assert.deepStrictEqual((yield* Relation.search(notes, 'cafe CREME', written.length, { open: '[', close: ']' })).map(Struct.get('highlight')), [Option.some({ title: '[Café] [crème]', body: 'Voûte mince' })]);
            yield* sql`UPDATE notes SET body = 'Steel cables' WHERE title = 'Concrete shell'`;
            assert.deepStrictEqual(yield* Effect.forEach(['vaults', 'steel'], (text) => titles(Relation.search(notes, text, written.length))), [[], ['Concrete shell']]);
        }),
    );
    it.effect('scopes tenant rows through row-level security on PostgreSQL and refuses tenant tables elsewhere', ({ task }) =>
        isolated(task.name, function* (reconcile, sql) {
            return yield* sql.onDialectOrElse({
                pg: () =>
                    Effect.gen(function* () {
                        const [first, second] = yield* Effect.all([uuid, uuid]);
                        const scoped = <A, E>(id: string, effect: Effect.Effect<A, E, SqlClient.SqlClient>) => Relation.tenant(ledgers, id, Effect.andThen(sql`SET LOCAL ROLE rasm_runtime`, effect));
                        yield* sql`DO $$ BEGIN CREATE ROLE rasm_runtime NOLOGIN; EXCEPTION WHEN duplicate_object THEN NULL; END $$`;
                        yield* reconcile(declared([ledgers]), Option.some('rasm_runtime'));
                        yield* scoped(first, Relation.insert(ledgers, [{ id: yield* uuid, tenant: first, amount: 1 }]));
                        yield* scoped(second, Relation.insert(ledgers, [{ id: yield* uuid, tenant: second, amount: 2 }]));
                        assert.deepStrictEqual(yield* scoped(first, sql`SELECT amount FROM ledgers`), [{ amount: 1 }]);
                        yield* refused(scoped(first, Relation.insert(ledgers, [{ id: yield* uuid, tenant: second, amount: 1 }])), 'AuthorizationError');
                    }),
                orElse: () => Effect.map(findings(declared([ledgers])), (found) => assert.deepStrictEqual(found, [{ _tag: 'Unsupported', table: 'ledgers', reason: 'tenant', columns: ['tenant'] }])),
            });
        }),
    );
    it.effect('retypes declared columns of an existing table to their stored types keeping rows', ({ task }) =>
        isolated(task.name, function* (reconcile, sql) {
            yield* sql.onDialectOrElse({
                pg: () => sql`CREATE TABLE ${sql('legacy')} (id text NOT NULL PRIMARY KEY, count integer NOT NULL, body text NOT NULL CHECK (body IS JSON))`,
                orElse: () => sql`CREATE TABLE ${sql('legacy')} (id TEXT NOT NULL PRIMARY KEY, count, body)`,
            });
            yield* sql`INSERT INTO legacy ${sql.insert({ id: 'l', count: 7, body: '{"camelKey":1}' })}`;
            yield* reconcile(declared([legacy]));
            assert.deepStrictEqual(yield* Relation.select(legacy, 'id', ['l']), [{ id: 'l', count: 7, body: { camelKey: 1 } }]);
            assert.deepStrictEqual(
                yield* sql.onDialectOrElse({
                    pg: () => sql`SELECT column_name AS name, data_type AS type FROM information_schema.columns WHERE table_name = 'legacy' AND column_name <> 'id' ORDER BY 1`,
                    orElse: () => sql`SELECT name, type FROM pragma_table_xinfo('legacy') WHERE name <> 'id' ORDER BY 1`,
                }),
                sql.onDialectOrElse({
                    pg: () => [
                        { name: 'body', type: 'jsonb' },
                        { name: 'count', type: 'bigint' },
                    ],
                    orElse: () => [
                        { name: 'body', type: 'TEXT' },
                        { name: 'count', type: 'INTEGER' },
                    ],
                }),
            );
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { cases };
