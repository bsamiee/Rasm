import { assert, type Vitest } from '@effect/vitest';
import { Catalog, Column, Relation, Rows } from '@rasm/database';
import { Effect, Exit, Fiber, Schema, Struct } from 'effect';
import { SqlClient } from 'effect/sql';
import { accounts, blobs, children, type Client, declared, events, isolated, notes, parents, refused, values, words } from '#/fixtures.ts';

// --- [MODELS] --------------------------------------------------------------------------

const titled = { renamed: { title: 'label' }, indexes: [{ columns: ['title'], unique: true }] } as const;
const renamed = Relation.make('parents', { id: Schema.String, title: Schema.String, state: parents.schema.fields.state }, titled);
const moved = { renamedFrom: 'children', references: [{ columns: ['parentId'], table: renamed, onDelete: 'CASCADE' }] } as const;
const extended = Relation.make('parents', { ...renamed.schema.fields, extra: Schema.NullOr(Schema.String) }, titled);
const retired = Relation.make('parents', renamed.schema.fields, { ...titled, retired: ['extra'] });
const revived = Relation.make('parents', { ...renamed.schema.fields, extra: Schema.NullOr(Schema.Int) }, { ...titled, retired: ['extra'] });
const kids = Relation.make('kids', children.schema.fields, moved);
const retyped = Relation.make('kids', { ...children.schema.fields, rank: Schema.Finite }, { ...moved, retype: ['rank'] });
const reopened = Relation.make('parents', { ...parents.schema.fields, state: Schema.Literals(['open', 'closed', 'archived']) }, { retype: ['state'] });
const annotated = Relation.make('parents', { ...parents.schema.fields, note: Schema.NullOr(Schema.String) });
const required = Relation.make('parents', { ...parents.schema.fields, owner: Schema.String });
const defaulted = Relation.make('parents', required.schema.fields, { defaults: { owner: 'nobody' } });
const relabeled = Relation.make('parents', { ...parents.schema.fields, label: Schema.Int });
const reserved = Relation.make('reserved', { id: Schema.String, rowid: Schema.Int });
const slugs = Relation.make('slugs', { body: Schema.fromJsonString(Schema.Struct({ slug: Schema.String })), slug: Schema.String }, { key: ['slug'], projections: { slug: { from: 'body', path: ['slug'] } } });
const textNotes = Relation.make('notes', { ...notes.schema.fields, id: Column.Uuid }, { search: words });
const ledgers = Relation.make('ledgers', { id: Column.Uuid, tenant: Schema.String, amount: Schema.Int }, { tenant: { column: 'tenant', setting: 'app.tenant' } });
const legacy = Relation.make('legacy', { id: Schema.String, count: Schema.Int, body: Schema.fromJsonString(Schema.Struct({ camelKey: Schema.Finite })) }, { retype: ['count', 'body'] });

// --- [OPERATIONS] ----------------------------------------------------------------------

const stored = (name: string) => SqlClient.SqlClient.use((sql) => sql`SELECT * FROM ${sql(name)} ORDER BY 1`);
const tables = SqlClient.SqlClient.use((sql) => sql`SELECT name FROM pragma_table_list WHERE schema = 'main' AND type = 'table' AND name NOT GLOB '_cf_*' AND name NOT GLOB 'sqlite_*' ORDER BY 1`);
const shape = SqlClient.SqlClient.use((sql) => sql`SELECT name, sql AS definition FROM sqlite_schema WHERE name NOT GLOB '_cf_*' ORDER BY name`);
const findings = (catalog: Catalog) =>
    Catalog.reconcile(catalog).pipe(
        Effect.as([]),
        Effect.catchTag('ReconcileError', ({ findings: refusals }) => Effect.succeed(refusals)),
    );
const seeded = SqlClient.SqlClient.use((sql) =>
    Effect.andThen(
        Catalog.reconcile(declared([parents, children])),
        Rows.atomic([
            sql`INSERT INTO parents ${sql.insert({ id: 'p', label: 'Parent', state: 'open' })}`,
            sql`INSERT INTO children ${sql.insert([
                { id: 'a', parentId: 'p', rank: 1 },
                { id: 'b', parentId: 'p', rank: 2 },
            ])}`,
        ]),
    ),
);
const reconciliation = <R, F>(it: Vitest.MethodsNonLive<R>, client: Client<F, NoInfer<R>>) => {
    const run = isolated(client);
    it.effect(
        'creates declared tables with enforced references and decoded rows',
        run(function* (sql) {
            yield* seeded;
            assert.deepStrictEqual(yield* tables, [{ name: 'children' }, { name: 'parents' }]);
            assert.deepStrictEqual(yield* Relation.select(children, { parentId: ['p'] }), [
                { id: 'a', parentId: 'p', rank: 1 },
                { id: 'b', parentId: 'p', rank: 2 },
            ]);
            yield* refused(sql`INSERT INTO children ${sql.insert({ id: 'c', parentId: 'missing', rank: 3 })}`, 'ConstraintError');
            yield* sql`DELETE FROM parents WHERE id = 'p'`;
            assert.deepStrictEqual(yield* stored('children'), []);
        }),
    );
    it.effect(
        'renames declared tables and columns keeping rows',
        run(function* () {
            yield* seeded;
            assert.deepStrictEqual(yield* Catalog.reconcile(declared([renamed, kids])), { tables: [], columns: [], indexes: [], references: [] });
            assert.deepStrictEqual(yield* stored('parents'), [{ id: 'p', title: 'Parent', state: 'open' }]);
            assert.lengthOf(yield* stored('kids'), 2);
            assert.deepStrictEqual(yield* tables, [{ name: 'kids' }, { name: 'parents' }]);
        }),
    );
    it.effect(
        'renames back keeping rows when older catalog runs after rename',
        run(function* (sql) {
            yield* seeded;
            yield* Catalog.reconcile(declared([renamed, kids]));
            yield* sql`INSERT INTO parents ${sql.insert({ id: 'q', title: 'Second', state: 'closed' })}`;
            yield* Catalog.reconcile(declared([parents, children]));
            assert.deepStrictEqual(yield* stored('parents'), [
                { id: 'p', label: 'Parent', state: 'open' },
                { id: 'q', label: 'Second', state: 'closed' },
            ]);
            assert.lengthOf(yield* stored('children'), 2);
            yield* Catalog.reconcile(declared([renamed, kids]));
            assert.deepStrictEqual((yield* stored('parents')).map(Struct.get('title')), ['Parent', 'Second']);
            assert.deepStrictEqual(yield* tables, [{ name: 'kids' }, { name: 'parents' }]);
        }),
    );
    it.effect(
        'keeps newer column and data when older catalog reconciles',
        run(function* (sql) {
            yield* seeded;
            yield* Catalog.reconcile(declared([extended, kids]));
            yield* sql`UPDATE parents SET ${sql.update({ extra: 'kept' })} WHERE id = 'p'`;
            assert.deepStrictEqual(yield* Catalog.reconcile(declared([renamed, kids])), { tables: [], columns: [{ table: 'parents', column: 'extra' }], indexes: [], references: [] });
            yield* sql`INSERT INTO parents ${sql.insert({ id: 'q', title: 'Second', state: 'closed' })}`;
            yield* Catalog.reconcile(declared([extended, kids]));
            assert.deepStrictEqual(yield* stored('parents'), [
                { id: 'p', title: 'Parent', state: 'open', extra: 'kept' },
                { id: 'q', title: 'Second', state: 'closed', extra: null },
            ]);
        }),
    );
    it.effect(
        'drops retired column and table and retypes column keeping rows',
        run(function* () {
            yield* seeded;
            yield* Catalog.reconcile(declared([extended, kids]));
            yield* Catalog.reconcile(declared([retired, retyped]));
            assert.deepStrictEqual(yield* stored('parents'), [{ id: 'p', title: 'Parent', state: 'open' }]);
            assert.deepStrictEqual((yield* stored('kids')).map(Struct.get('rank')), [1, 2]);
            yield* Catalog.reconcile(declared([retired], ['kids']));
            assert.deepStrictEqual(yield* tables, [{ name: 'parents' }]);
        }),
    );
    it.effect(
        'keeps re-added column with retired name when retiring catalog runs again',
        run(function* (sql) {
            yield* seeded;
            yield* Catalog.reconcile(declared([extended, kids]));
            yield* sql`UPDATE parents SET ${sql.update({ extra: 'old' })} WHERE id = 'p'`;
            yield* Catalog.reconcile(declared([revived, kids]));
            assert.deepStrictEqual(yield* stored('parents'), [{ id: 'p', title: 'Parent', state: 'open', extra: null }]);
            yield* sql`UPDATE parents SET ${sql.update({ extra: 7 })} WHERE id = 'p'`;
            assert.deepStrictEqual(yield* Catalog.reconcile(declared([retired, kids])), { tables: [], columns: [{ table: 'parents', column: 'extra' }], indexes: [], references: [] });
            assert.deepStrictEqual(yield* Catalog.reconcile(declared([extended, kids])), { tables: [], columns: [], indexes: [], references: [] });
            assert.deepStrictEqual(yield* stored('parents'), [{ id: 'p', title: 'Parent', state: 'open', extra: 7 }]);
        }),
    );
    it.effect(
        'retypes declared columns of an existing table to their stored types keeping rows',
        run(function* (sql) {
            yield* sql`CREATE TABLE legacy (id TEXT NOT NULL PRIMARY KEY, count, body)`;
            yield* sql`INSERT INTO legacy ${sql.insert({ id: 'l', count: 7, body: '{"camelKey":1}' })}`;
            yield* Catalog.reconcile(declared([legacy]));
            assert.deepStrictEqual(yield* Relation.select(legacy, { id: ['l'] }), [{ id: 'l', count: 7, body: { camelKey: 1 } }]);
            assert.deepStrictEqual(yield* sql`SELECT name, type FROM pragma_table_xinfo('legacy') WHERE name <> 'id' ORDER BY 1`, [
                { name: 'body', type: 'TEXT' },
                { name: 'count', type: 'INTEGER' },
            ]);
        }),
    );
    it.effect(
        'preserves SQLite autoincrement on empty and previously used tables during rebuild',
        run(function* (sql) {
            yield* sql`CREATE TABLE empty_legacy (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, amount INTEGER NOT NULL) STRICT`;
            yield* sql`CREATE TABLE used_legacy (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, amount INTEGER NOT NULL) STRICT`;
            yield* sql`CREATE TABLE outside (id INTEGER PRIMARY KEY AUTOINCREMENT)`;
            yield* sql`CREATE TABLE outside__rebuild (id INTEGER PRIMARY KEY AUTOINCREMENT)`;
            yield* sql`INSERT INTO used_legacy (id, amount) VALUES (7, 42), (90, 12)`;
            yield* sql`DELETE FROM used_legacy WHERE id = 90`;
            yield* sql`INSERT INTO outside (id) VALUES (70)`;
            yield* sql`INSERT INTO outside__rebuild (id) VALUES (4)`;
            const fields = { id: Schema.Int, amount: Schema.Finite };
            const changed = { retype: ['amount'] } as const;
            const catalog = declared([Relation.make('empty_legacy', fields, changed), Relation.make('used', fields, { ...changed, renamedFrom: 'used_legacy' })]);
            yield* Catalog.reconcile(catalog);
            yield* sql`INSERT INTO empty_legacy (amount) VALUES (1.5)`;
            yield* sql`DELETE FROM empty_legacy`;
            yield* sql`INSERT INTO empty_legacy (amount) VALUES (2.5)`;
            yield* sql`INSERT INTO used (amount) VALUES (3.5)`;
            const sequence = sql`SELECT seq FROM sqlite_sequence WHERE name = 'outside__rebuild' ORDER BY seq`;
            assert.deepStrictEqual(yield* Effect.all([stored('empty_legacy'), stored('used'), sequence, Effect.map(Catalog.plan(catalog), Struct.get('statements'))]), [
                [{ id: 2, amount: 2.5 }],
                [
                    { id: 7, amount: 42 },
                    { id: 91, amount: 3.5 },
                ],
                [{ seq: 4 }],
                [],
            ]);
        }),
    );
    it.effect(
        'plans nothing when store matches catalog',
        run(function* () {
            const full = declared([extended, kids, accounts, events, notes, values, blobs]);
            yield* seeded;
            yield* Catalog.reconcile(full);
            const before = yield* shape;
            const planned = yield* Effect.all([Catalog.plan(full), Catalog.plan(declared([renamed, kids]))]);
            assert.deepStrictEqual(
                planned.flatMap(({ statements, concurrent }) => [statements, concurrent]),
                [[], [], [], []],
            );
            yield* Catalog.reconcile(full);
            assert.deepStrictEqual(yield* shape, before);
        }),
    );
    it.effect(
        'finishes concurrent reconciles of one catalog after an interrupted one',
        run(function* () {
            const catalog = declared([parents, children]);
            const interrupted = yield* Effect.forkChild(Catalog.reconcile(catalog));
            yield* Effect.yieldNow;
            yield* Fiber.interrupt(interrupted);
            yield* Effect.all([Catalog.reconcile(catalog), Catalog.reconcile(catalog)], { concurrency: 'unbounded' });
            assert.deepStrictEqual(yield* Effect.map(Catalog.plan(catalog), Struct.pick(['statements', 'concurrent'])), { statements: [], concurrent: [] });
            assert.deepStrictEqual(yield* stored('children'), []);
        }),
    );
    it.effect(
        'keeps rows and columns when concurrent catalogs rebuild and extend one table',
        run(function* () {
            yield* seeded;
            const [extension, rebuild] = yield* Effect.all([Effect.exit(Catalog.reconcile(declared([annotated, children]))), Effect.exit(Catalog.reconcile(declared([reopened, children])))], { concurrency: 'unbounded' });
            assert.isTrue(Exit.isSuccess(rebuild) || Exit.isSuccess(extension));
            assert.deepStrictEqual(yield* stored('parents'), [{ id: 'p', label: 'Parent', state: 'open', ...(Exit.isSuccess(extension) ? { note: null } : {}) }]);
            assert.lengthOf(yield* stored('children'), 2);
        }),
    );
    it.effect(
        'refuses undeclared retypes, required columns without default, and reserved, projected, search, and tenant keys, filling existing rows from a declared default',
        run(function* () {
            yield* seeded;
            assert.deepStrictEqual(yield* Effect.all([findings(declared([required, children])), findings(declared([relabeled, children])), findings(declared([reserved, slugs])), findings(declared([textNotes])), findings(declared([ledgers]))]), [
                [{ table: 'parents', columns: ['owner'], reason: { _tag: 'Required' } }],
                [{ table: 'parents', columns: ['label'], reason: { _tag: 'Retype' } }],
                [
                    { table: 'slugs', columns: ['slug'], reason: { _tag: 'Unsupported', refusal: 'projected' } },
                    { table: 'reserved', columns: ['rowid'], reason: { _tag: 'Unsupported', refusal: 'reserved' } },
                ],
                [{ table: 'notes', columns: ['id'], reason: { _tag: 'Unsupported', refusal: 'search' } }],
                [{ table: 'ledgers', columns: ['tenant'], reason: { _tag: 'Unsupported', refusal: 'tenant' } }],
            ]);
            yield* Catalog.reconcile(declared([defaulted, children]));
            assert.deepStrictEqual(yield* stored('parents'), [{ id: 'p', label: 'Parent', state: 'open', owner: 'nobody' }]);
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { reconciliation };
