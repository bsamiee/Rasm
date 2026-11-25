import { PgClient } from '@effect/sql-pg';
import { assert, layer, type Vitest } from '@effect/vitest';
import { Catalog, Relation, Tokenizer } from '@rasm/database';
import { Pg } from '@rasm/database/pg';
import { PostgreSqlContainer, type StartedPostgreSqlContainer } from '@testcontainers/postgresql';
import { Array, Context, Deferred, Effect, Function, Layer, Number, Option, Predicate, Redacted, Ref, Schema, Struct } from 'effect';
import { SqlClient, SqlError, SqlSchema } from 'effect/sql';
import { declared, pending, reconciling, stored } from '#/reconciling';
import { type Relational, rows, values } from '#/rows';

// --- [CONSTANTS] -----------------------------------------------------------------------

const store: Relational = {
    tables: SqlClient.SqlClient.use((sql) => Effect.map(sql`SELECT c.relname AS name FROM pg_class c WHERE c.relnamespace = current_schema()::regnamespace AND c.relkind = 'r' ORDER BY c.relname`, Array.map(Struct.get('name')))),
    fault: (error) => (SqlError.isSqlError(error) && Predicate.hasProperty(error.reason.cause, 'code') ? error.reason.cause.code : error),
    check: '23514',
    unique: '23505',
    reference: '23503',
    command: Function.identity,
    isolated: 4,
};
const privileges = ['SELECT', 'INSERT', 'UPDATE', 'DELETE', 'TRUNCATE'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const measured = Relation.make('measured', { id: Schema.String, ratio: Schema.Finite, amount: Schema.BigDecimalFromString, at: Schema.DateTimeUtcFromString });
const finite = { id: 'a', ratio: 1, amount: '1', at: '2026-01-01T00:00:00.000Z' };
const unbounded: readonly { readonly field: keyof typeof finite; readonly refused: string }[] = [
    { field: 'ratio', refused: 'NaN' },
    { field: 'ratio', refused: 'Infinity' },
    { field: 'ratio', refused: '-Infinity' },
    { field: 'amount', refused: 'NaN' },
    { field: 'amount', refused: 'Infinity' },
    { field: 'at', refused: 'infinity' },
    { field: 'at', refused: '-infinity' },
];
const handles = Relation.make('handles', { id: Schema.String, handle: Schema.String }, { indexes: [{ columns: [{ casefold: 'handle' }], unique: true }] });
const indexed = Relation.make(
    'indexed',
    {
        id: Schema.String,
        email: Schema.String,
        title: Schema.String,
        deletedAt: Schema.NullOr(Schema.DateTimeUtcFromString),
        at: Schema.DateTimeUtcFromString,
        profile: Schema.fromJsonString(Schema.Struct({ plan: Schema.String })),
        plan: Schema.String,
        live: Schema.BooleanFromBit,
    },
    {
        projections: { plan: { from: 'profile', path: ['plan'] }, live: { where: { deletedAt: null } } },
        indexes: [
            { columns: ['email'], unique: true, where: { deletedAt: null } },
            { columns: ['title'], include: ['email'] },
            { columns: ['profile'], method: 'gin' },
            { columns: ['title'], method: 'gin' },
            { columns: ['email'], method: 'gist' },
            { columns: ['at'], method: 'brin' },
        ],
    },
);
const scores = Relation.make('scores', { id: Schema.String, score: Schema.Finite });
const counted = Relation.make('scores', { id: Schema.String, score: Schema.Int }, { retype: ['score'] });
const drafts = Relation.make('drafts', { id: Schema.String, note: Schema.NullOr(Schema.String) });
const finals = Relation.make('drafts', { id: Schema.String, note: Schema.String }, { retype: ['note'] });
const lookups = Relation.make('lookups', { id: Schema.String, label: Schema.String });
const sought = Relation.make('lookups', lookups.schema.fields, { indexes: [{ columns: ['label'] }] });
const widened = Relation.make('lookups', { ...lookups.schema.fields, note: Schema.NullOr(Schema.String) });
const articles = Relation.make('articles', { id: Schema.Int, title: Schema.String }, { search: { columns: ['title'], tokenizer: Tokenizer.Words({ configuration: 'simple', filters: ['unaccent'], tokenize: 'unicode61' }) } });
const ledgers = Relation.make('ledgers', { id: Schema.String, tenant: Schema.String, amount: Schema.Int }, { tenant: { column: 'tenant', setting: 'app.tenant' } });
const slugged = Relation.make('legacy', { id: Schema.String, slug: Schema.String.check(Schema.isLowercased()), state: Schema.Literals(['lower', 'upper']) }, { renamed: { slug: 'lower' } });
const restated = Relation.make('legacy', { id: Schema.String, lower: Schema.String.check(Schema.isLowercased()), state: Schema.Literals(['lower', 'text']) });

// --- [SERVICES] ------------------------------------------------------------------------

class Database extends Context.Service<Database, { readonly connected: (username: string, password: string) => Redacted.Redacted; readonly name: string }>()('@rasm/test-database/postgres.test/Database') {}
class Postgres extends Context.Service<Postgres, { readonly container: StartedPostgreSqlContainer; readonly admin: SqlClient.SqlClient; readonly created: Ref.Ref<number> }>()('@rasm/test-database/postgres.test/Postgres') {
    static readonly layer = Effect.gen(function* () {
        const container = yield* Effect.acquireDisposable(Effect.promise(() => new PostgreSqlContainer('postgres:19.0-alpine3.24').withCommand(['postgres', '-c', 'fsync=off', '-c', 'synchronous_commit=off', '-c', 'full_page_writes=off']).withTmpFs({ '/var/lib/postgresql': 'rw' }).start()));
        const admin = Context.get(yield* Layer.build(PgClient.layer({ url: Redacted.make(container.getConnectionUri()) })), SqlClient.SqlClient);
        return { container, admin, created: yield* Ref.make(0) };
    }).pipe(Layer.effect(Postgres));
    static readonly database = Layer.unwrap(
        Effect.gen(function* () {
            const { container, admin, created } = yield* Postgres;
            const name = `test_${yield* Ref.updateAndGet(created, Number.increment)}`;
            const connected = (username: string, password: string) => Redacted.make(Object.assign(new URL(container.getConnectionUri()), { pathname: name, username, password }).href);
            yield* Effect.acquireRelease(admin`CREATE DATABASE ${admin(name)}`, () => Effect.orDie(admin`DROP DATABASE ${admin(name)} WITH (FORCE)`));
            return Layer.merge(Pg.layer({ url: connected(container.getUsername(), container.getPassword()) }), Layer.succeed(Database, { name, connected }));
        }),
    );
    static readonly role = Effect.fnUntraced(function* (label: string) {
        const [sql, { name, connected }] = yield* Effect.all([SqlClient.SqlClient, Database]);
        const role = `${name}_${label}`;
        yield* Effect.acquireRelease(sql`CREATE ROLE ${sql(role)} LOGIN PASSWORD ${sql.literal(`'${role}'`)}`, () => Effect.orDie(Effect.andThen(sql`DROP OWNED BY ${sql(role)}`, sql`DROP ROLE ${sql(role)}`)));
        return { role, client: Pg.layer({ url: connected(role, role) }) };
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const columns = (table: string) =>
    SqlClient.SqlClient.use(
        (sql) => sql`SELECT a.attname AS name, format_type(a.atttypid, a.atttypmod) AS type, a.attnotnull::text AS required, a.attgenerated::text AS generated
            FROM pg_attribute a WHERE a.attrelid = ${table}::regclass AND a.attnum > 0 AND NOT a.attisdropped ORDER BY a.attnum`,
    );
const constraints = (table: string) => SqlClient.SqlClient.use((sql) => sql`SELECT n.contype::text AS kind, n.convalidated::text AS validated FROM pg_constraint n WHERE n.conrelid = ${table}::regclass AND n.contype IN ('n', 'c') ORDER BY n.conname`);
const extensions = SqlClient.SqlClient.use((sql) => sql`SELECT e.extname AS name FROM pg_extension e WHERE e.extname <> 'plpgsql' ORDER BY e.extname`);
const unique = Effect.exit(SqlClient.SqlClient.use((sql) => sql`CREATE UNIQUE INDEX CONCURRENTLY lookups_label ON lookups (label)`));
const builds: readonly { readonly label: string; readonly labels: Array.NonEmptyReadonlyArray<string>; readonly arranged: Effect.Effect<unknown, never, SqlClient.SqlClient>; readonly before: readonly object[]; readonly planned: readonly string[] }[] = [
    { label: 'builds an index added to a stored table', labels: ['one', 'two'], arranged: Effect.void, before: [], planned: ['CREATE INDEX CONCURRENTLY'] },
    { label: 'replaces a valid index of another shape', labels: ['one', 'two'], arranged: unique, before: [{ valid: 'true', unique: 'true' }], planned: ['DROP INDEX CONCURRENTLY', 'CREATE INDEX CONCURRENTLY'] },
    { label: 'replaces an invalid index a failed build left', labels: ['same', 'same'], arranged: unique, before: [{ valid: 'false', unique: 'true' }], planned: ['DROP INDEX CONCURRENTLY', 'CREATE INDEX CONCURRENTLY'] },
];
const mapped = SqlSchema.findAll({
    Request: Schema.Void,
    Result: Schema.Struct({ configuration: Schema.String, dictionary: Schema.String }),
    execute: () =>
        SqlClient.SqlClient.use(
            (sql) => sql`SELECT c.cfgname AS configuration, m.mapdict::regdictionary::text AS dictionary FROM pg_ts_config c JOIN pg_ts_config_map m ON m.mapcfg = c.oid
                JOIN ts_token_type(c.cfgparser) t ON t.tokid = m.maptokentype WHERE c.cfgnamespace = current_schema()::regnamespace AND t.alias = 'word' ORDER BY m.mapseqno`,
        ),
})(undefined);

const cases = (it: Vitest.MethodsNonLive<Layer.Success<typeof Postgres.database> | Postgres>) => {
    reconciling(it, store);
    rows(it, store);
    it.effect(
        'stores each schema kind in the PostgreSQL type that holds every value of it',
        Effect.fnUntraced(function* () {
            yield* Catalog.reconcile(declared([values]));
            assert.deepStrictEqual((yield* columns('values')).map(Struct.pick(['name', 'type', 'required'])), [
                { name: 'id', type: 'uuid', required: 'true' },
                { name: 'count', type: 'bigint', required: 'true' },
                { name: 'amount', type: 'numeric', required: 'true' },
                { name: 'body', type: 'jsonb', required: 'true' },
                { name: 'bytes', type: 'bytea', required: 'true' },
                { name: 'at', type: 'timestamp(3) with time zone', required: 'true' },
                { name: 'flag', type: 'boolean', required: 'true' },
                { name: 'ratio', type: 'double precision', required: 'true' },
                { name: 'code', type: 'text', required: 'true' },
                { name: 'note', type: 'text', required: 'false' },
            ]);
        }),
    );
    it.effect.each(unbounded)(
        'refuses $refused as $field',
        Effect.fnUntraced(function* ({ field, refused }) {
            const sql = yield* SqlClient.SqlClient;
            yield* Catalog.reconcile(declared([measured]));
            yield* sql`INSERT INTO measured ${sql.insert(finite)}`;
            const error = yield* Effect.flip(sql`INSERT INTO measured ${sql.insert({ ...finite, id: 'b', [field]: refused })}`);
            assert.deepStrictEqual({ fault: store.fault(error), stored: yield* sql`SELECT id FROM measured` }, { fault: store.check, stored: [{ id: 'a' }] });
        }),
    );
    it.effect(
        'keeps one handle per Unicode full case folding',
        Effect.fnUntraced(function* () {
            const kept = Array.make({ id: 'a', handle: 'Maße' }, { id: 'b', handle: 'masse2' });
            yield* Catalog.reconcile(declared([handles]));
            yield* Relation.insert(handles, kept);
            const folded = yield* Effect.flip(Relation.insert(handles, [{ id: 'c', handle: 'MASSE' }]));
            assert.deepStrictEqual({ fault: store.fault(folded), stored: yield* stored('handles') }, { fault: store.unique, stored: kept });
        }),
    );
    it.effect(
        'builds partial, covering, JSON path, trigram, and block range indexes with their extension, and computed columns as virtual',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            yield* Catalog.reconcile(declared([indexed]));
            assert.deepStrictEqual(
                yield* Effect.all({
                    indexes: sql`SELECT am.amname AS method, (SELECT string_agg(o.opcname, ',' ORDER BY u.position) FROM unnest(x.indclass) WITH ORDINALITY u(oid, position) JOIN pg_opclass o ON o.oid = u.oid) AS classes, x.indisunique::text AS unique, x.indisvalid::text AS valid,
                            x.indnkeyatts::int AS keys, x.indnatts::int AS width, coalesce(pg_get_expr(x.indpred, x.indrelid), '') AS predicate
                            FROM pg_index x JOIN pg_class i ON i.oid = x.indexrelid JOIN pg_am am ON am.oid = i.relam WHERE x.indrelid = 'indexed'::regclass AND NOT x.indisprimary ORDER BY 1, 2, 3`,
                    extensions,
                    generated: Effect.map(columns('indexed'), (found) => found.filter(({ generated }) => generated !== '').map(Struct.pick(['name', 'generated']))),
                }),
                {
                    indexes: [
                        { method: 'brin', classes: 'timestamptz_minmax_ops', unique: 'false', valid: 'true', keys: 1, width: 1, predicate: '' },
                        { method: 'btree', classes: 'text_ops', unique: 'false', valid: 'true', keys: 1, width: 2, predicate: '' },
                        { method: 'btree', classes: 'text_ops', unique: 'true', valid: 'true', keys: 1, width: 1, predicate: '(deleted_at IS NULL)' },
                        { method: 'gin', classes: 'gin_trgm_ops', unique: 'false', valid: 'true', keys: 1, width: 1, predicate: '' },
                        { method: 'gin', classes: 'jsonb_path_ops', unique: 'false', valid: 'true', keys: 1, width: 1, predicate: '' },
                        { method: 'gist', classes: 'gist_trgm_ops', unique: 'false', valid: 'true', keys: 1, width: 1, predicate: '' },
                    ],
                    extensions: [{ name: 'pg_trgm' }],
                    generated: [
                        { name: 'plan', generated: 'v' },
                        { name: 'live', generated: 'v' },
                    ],
                },
            );
        }),
    );
    it.effect(
        'refuses a retype that would change a stored value and applies it once every value survives',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            const retyped = declared([counted]);
            const seen = Effect.all({ types: Effect.map(columns('scores'), Array.map(Struct.get('type'))), stored: stored('scores') });
            yield* Catalog.reconcile(declared([scores]));
            yield* Relation.insert(scores, [{ id: 'a', score: 1.5 }]);
            const lossy = yield* Effect.flip(Catalog.reconcile(retyped));
            const kept = yield* seen;
            yield* sql`UPDATE scores SET score = 2`;
            yield* Catalog.reconcile(retyped);
            assert.deepStrictEqual({ fault: store.fault(lossy), kept, applied: yield* seen }, { fault: store.check, kept: { types: ['text', 'double precision'], stored: [{ id: 'a', score: 1.5 }] }, applied: { types: ['text', 'bigint'], stored: [{ id: 'a', score: 2 }] } });
        }),
    );
    it.effect(
        'leaves a tightened column unvalidated while a row violates it and validates it once none does',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            const tightened = declared([finals]);
            yield* Catalog.reconcile(declared([drafts]));
            yield* Relation.insert(drafts, [{ id: 'a', note: null }]);
            const violated = yield* Effect.flip(Catalog.reconcile(tightened));
            const unvalidated = yield* constraints('drafts');
            yield* sql`UPDATE drafts SET note = 'written'`;
            yield* Catalog.reconcile(tightened);
            assert.deepStrictEqual(
                { fault: store.fault(violated), unvalidated, validated: yield* constraints('drafts'), pending: yield* pending(tightened), stored: yield* stored('drafts') },
                {
                    fault: '23502',
                    unvalidated: [
                        { kind: 'n', validated: 'true' },
                        { kind: 'n', validated: 'false' },
                    ],
                    validated: [
                        { kind: 'n', validated: 'true' },
                        { kind: 'n', validated: 'true' },
                    ],
                    pending: { statements: [], concurrent: [] },
                    stored: [{ id: 'a', note: 'written' }],
                },
            );
        }),
    );
    it.effect.each(builds)(
        '$label once across concurrent reconciles, outside the transaction and with no deadlock',
        Effect.fnUntraced(function* ({ labels, arranged, before, planned }) {
            const sql = yield* SqlClient.SqlClient;
            const catalog = declared([sought]);
            const indexes = sql`SELECT x.indisvalid::text AS valid, x.indisunique::text AS unique FROM pg_index x WHERE x.indrelid = 'lookups'::regclass AND NOT x.indisprimary`;
            yield* Catalog.reconcile(declared([lookups]));
            const seeds = Array.map(labels, (label, index) => ({ id: `${index}`, label }));
            yield* Relation.insert(lookups, seeds);
            yield* arranged;
            const seeded = yield* indexes;
            yield* sql`CREATE TABLE built (tag text NOT NULL, identity text NOT NULL)`;
            yield* sql`CREATE FUNCTION logged() RETURNS event_trigger LANGUAGE plpgsql AS 'BEGIN INSERT INTO built (tag, identity) SELECT c.command_tag, c.object_identity FROM pg_event_trigger_ddl_commands() c WHERE c.object_type = ''index'' AND c.schema_name = current_schema(); END'`;
            yield* sql`CREATE EVENT TRIGGER logged ON ddl_command_end WHEN TAG IN ('CREATE INDEX') EXECUTE FUNCTION logged()`;
            const pended = yield* pending(catalog);
            const settled = yield* Effect.all(Array.replicate(Effect.result(Catalog.reconcile(catalog)), 4), { concurrency: 'unbounded' });
            assert.deepStrictEqual<unknown>(
                {
                    seeded,
                    planned: { statements: pended.statements, concurrent: pended.concurrent.map((text) => text.split(' ').slice(0, 3).join(' ')) },
                    raised: Array.map(Array.getFailures(settled), store.fault),
                    deadlocks: yield* sql`SELECT d.deadlocks::int AS deadlocks FROM pg_stat_database d WHERE d.datname = current_database()`,
                    built: yield* stored('built'),
                    indexes: yield* indexes,
                    pending: yield* pending(catalog),
                },
                {
                    seeded: before,
                    planned: { statements: [], concurrent: planned },
                    raised: [],
                    deadlocks: [{ deadlocks: 0 }],
                    built: [{ tag: 'CREATE INDEX', identity: 'public.lookups_label' }],
                    indexes: [{ valid: 'true', unique: 'false' }],
                    pending: { statements: [], concurrent: [] },
                },
            );
        }),
    );
    it.effect(
        'copies a text search configuration with its filter dictionary and repairs a changed mapping',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            const catalog = declared([articles]);
            const dictionaries = Effect.map(mapped, Array.map(Struct.get('dictionary')));
            yield* Catalog.reconcile(catalog);
            const copied = yield* dictionaries;
            yield* Effect.forEach(Array.dedupe((yield* mapped).map(Struct.get('configuration'))), (configuration) => sql`ALTER TEXT SEARCH CONFIGURATION ${sql(configuration)} ALTER MAPPING FOR word WITH simple`);
            yield* Relation.insert(articles, [{ id: 1, title: 'Crème' }]);
            const drifted = yield* dictionaries;
            yield* Catalog.reconcile(catalog);
            assert.deepStrictEqual(
                { copied, extensions: yield* extensions, drifted, repaired: yield* dictionaries, found: (yield* Relation.search(articles, 'creme', articles.key.length)).map(({ row }) => row.title) },
                { copied: ['unaccent', 'simple'], extensions: [{ name: 'unaccent' }], drifted: ['simple'], repaired: ['unaccent', 'simple'], found: ['Crème'] },
            );
        }),
    );
    it.effect(
        'shows a tenant its own rows alone and refuses a row written for another tenant',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            const runtime = yield* Postgres.role('runtime');
            const [north, south] = [
                { id: 'a', tenant: 'north', amount: 1 },
                { id: 'b', tenant: 'south', amount: 2 },
            ];
            yield* Catalog.reconcile(declared([ledgers]), Option.some(runtime.role));
            const observed = yield* Effect.all({
                written: Effect.forEach([north, south], (row) => Relation.withTenant(ledgers, row.tenant, Relation.insert(ledgers, [row]))),
                crossed: Effect.map(Effect.flip(Relation.withTenant(ledgers, north.tenant, Relation.insert(ledgers, [{ id: 'c', tenant: south.tenant, amount: 3 }]))), store.fault),
                north: Relation.withTenant(ledgers, north.tenant, Relation.select(ledgers, { amount: [1, 2] })),
                south: Relation.withTenant(ledgers, south.tenant, Relation.select(ledgers, { amount: [1, 2] })),
                unscoped: Relation.select(ledgers, { amount: [1, 2] }),
            }).pipe(Effect.provide(runtime.client));
            assert.deepStrictEqual(
                { observed, secured: yield* sql`SELECT c.relrowsecurity::text AS enabled, c.relforcerowsecurity::text AS forced FROM pg_class c WHERE c.oid = 'ledgers'::regclass` },
                { observed: { written: [[north], [south]], crossed: '42501', north: [north], south: [south], unscoped: [] }, secured: [{ enabled: 'true', forced: 'true' }] },
            );
        }),
    );
    it.effect(
        'grants the runtime role row reads and writes and the reader role reads when a migrator reconciles',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            const [runtime, reader] = yield* Effect.all([Postgres.role('runtime'), Postgres.role('reader')]);
            const held = (role: string) => Effect.forEach(privileges, (privilege) => sql`SELECT has_table_privilege(${role}, 'lookups', ${privilege})::text AS held`).pipe(Effect.map((found) => found.flat().map(Struct.get('held'))));
            yield* Layer.build(Catalog.layer(Catalog.make({ tables: [lookups], reader: reader.role }), runtime.client, Layer.succeed(SqlClient.SqlClient, sql)));
            assert.deepStrictEqual(yield* Effect.all({ runtime: held(runtime.role), reader: held(reader.role) }), { runtime: ['true', 'true', 'true', 'true', 'false'], reader: ['true', 'false', 'false', 'false', 'false'] });
        }),
    );
    it.effect(
        'stops with a lock timeout while another session locks a table it must change, changing nothing',
        Effect.fnUntraced(function* () {
            const [{ connected }, { container }] = yield* Effect.all([Database, Postgres]);
            const [locked, released] = yield* Effect.all([Deferred.make<void>(), Deferred.make<void>()]);
            yield* Catalog.reconcile(declared([lookups]));
            yield* SqlClient.SqlClient.use((holder) => holder.withTransaction(holder`LOCK TABLE lookups IN ACCESS EXCLUSIVE MODE`.pipe(Effect.andThen(Deferred.succeed(locked, undefined)), Effect.andThen(Deferred.await(released))))).pipe(
                Effect.provide(Pg.layer({ url: connected(container.getUsername(), container.getPassword()) })),
                Effect.forkScoped,
            );
            yield* Deferred.await(locked);
            const blocked = yield* Effect.flip(Catalog.reconcile(Catalog.make({ tables: [widened], timeouts: { lock: '100 millis', transaction: '1 minute' } })));
            yield* Deferred.succeed(released, undefined);
            assert.deepStrictEqual({ fault: store.fault(blocked), columns: (yield* columns('lookups')).map(Struct.get('name')) }, { fault: '55P03', columns: ['id', 'label'] });
        }),
    );
    it.effect(
        'adopts a hand-made table under a renamed column whose name its check also uses as a function and a literal, and refuses a changed check',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            yield* sql`CREATE TABLE legacy (id text NOT NULL PRIMARY KEY, lower text NOT NULL CHECK (lower(lower) = lower), state text NOT NULL CHECK (state IN ('lower', 'upper')))`;
            yield* sql`INSERT INTO legacy (id, lower, state) VALUES ('a', 'kept', 'lower')`;
            const changed = yield* Effect.flip(Catalog.reconcile(declared([restated])));
            yield* Catalog.reconcile(declared([slugged]));
            assert.deepStrictEqual(
                { changed: changed._tag === 'ReconcileError' ? changed.findings : changed, adopted: yield* stored('legacy'), pending: yield* pending(declared([slugged])) },
                { changed: [{ table: 'legacy', columns: ['state'], reason: { _tag: 'Retype' } }], adopted: [{ id: 'a', slug: 'kept', state: 'lower' }], pending: { statements: [], concurrent: [] } },
            );
        }),
    );
};

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(Postgres.layer, { excludeTestServices: true, concurrent: true })('postgres', (outer) => outer.layer(Postgres.database, { buildPerTest: true })(cases));
