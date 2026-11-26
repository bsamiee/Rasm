import { assert, layer } from '@effect/vitest';
import { Catalog } from '@rasm/database/catalog';
import { d1Engine } from '@rasm/database/d1';
import { Rows } from '@rasm/database/engine';
import { Relation } from '@rasm/database/relation';
import { reset } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { Array, Effect, Layer, Predicate, Schema } from 'effect';
import { SqlClient, SqlError } from 'effect/sql';
import { declared, pending, stored } from '#/reconciling';
import { featured } from '#/rows';
import { tables } from '#/storage.sqlite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const failed = /(?<kind>CHECK|UNIQUE) constraint failed/u;
const [parent, second] = [
    { id: 'p', label: 'Parent' },
    { id: 'q', label: 'Second' },
] as const;
const written = Array.make(parent, second);

// --- [MODELS] --------------------------------------------------------------------------

const people = Relation.make('people', { id: Schema.String, label: Schema.String });
const persons = Relation.make('persons', { id: Schema.String, title: Schema.String }, { renamedFrom: 'people', renamed: { title: 'label' } });
const ranked = Relation.make('people', { id: Schema.String, label: Schema.Literals(['Parent', 'Guardian']) }, { retype: ['label'] });

// --- [OPERATIONS] ----------------------------------------------------------------------

const fault = (error: unknown) => (SqlError.isSqlError(error) && Predicate.hasProperty(error.reason.cause, 'message') && Predicate.isString(error.reason.cause.message) ? failed.exec(error.reason.cause.message)?.groups?.['kind'] : error);
const seeded = Effect.andThen(Catalog.reconcile(declared([people])), Relation.insert(people, written));
const schema = SqlClient.SqlClient.use((sql) => sql`SELECT type, name, sql FROM sqlite_schema ORDER BY name`);

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(d1Engine({ db: env.DB }).pipe(Layer.provide(Layer.effectDiscard(Effect.promise(reset)))), { buildPerTest: true, excludeTestServices: true })('d1', (it) => {
    it.effect(
        'renames a table and its column keeping rows',
        Effect.fnUntraced(function* () {
            yield* seeded;
            yield* Catalog.reconcile(declared([persons]));
            assert.deepStrictEqual(
                { tables: yield* tables, stored: yield* stored('persons') },
                {
                    tables: ['persons'],
                    stored: [
                        { id: 'p', title: 'Parent' },
                        { id: 'q', title: 'Second' },
                    ],
                },
            );
        }),
    );
    it.effect(
        'keeps a table, its definition, and its rows when the rebuild a retype needs fails on a stored row',
        Effect.fnUntraced(function* () {
            yield* seeded;
            const before = yield* schema;
            const refused = yield* Effect.flip(Catalog.reconcile(declared([ranked])));
            assert.deepStrictEqual({ refused: fault(refused), schema: yield* schema, stored: yield* stored('people') }, { refused: 'CHECK', schema: before, stored: written });
        }),
    );
    it.effect(
        'writes none of a statement batch whose last statement fails',
        Effect.fnUntraced(function* () {
            const sql = yield* SqlClient.SqlClient;
            yield* Catalog.reconcile(declared([people]));
            const refused = yield* Effect.flip(Rows.atomic([sql`INSERT INTO people ${sql.insert(parent)}`, sql`INSERT INTO people ${sql.insert(second)}`, sql`INSERT INTO people ${sql.insert({ ...second, label: 'Repeated' })}`]));
            assert.deepStrictEqual({ refused: fault(refused), stored: yield* stored('people') }, { refused: 'UNIQUE', stored: [] });
        }),
    );
    it.effect(
        'reconciles references, partial, covering, and case-insensitive indexes, computed columns, and text search in the SQLite of workerd with nothing left to plan',
        Effect.fnUntraced(function* () {
            const catalog = declared(featured);
            yield* Catalog.reconcile(catalog);
            assert.deepStrictEqual(yield* pending(catalog), { statements: [], concurrent: [] });
        }),
    );
});
