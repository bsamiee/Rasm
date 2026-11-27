import { assert, type Vitest } from '@effect/vitest';
import { Catalog } from '@rasm/database/catalog';
import { Column, type Fields } from '@rasm/database/column';
import type { Engine } from '@rasm/database/engine';
import { Relation } from '@rasm/database/relation';
import { Array, DateTime, Effect, Record, Schema, Struct } from 'effect';
import { SqlClient, type SqlError } from 'effect/sql';

// --- [TYPES] ---------------------------------------------------------------------------

interface Store {
    readonly tables: Effect.Effect<readonly unknown[], SqlError.SqlError, SqlClient.SqlClient>;
    readonly fault: (error: unknown) => unknown;
    readonly check: unknown;
    readonly command: <A, E, R>(statement: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>;
    readonly isolated: number;
}
interface Step {
    readonly declared: Catalog['tables'];
    readonly retired?: readonly string[];
    readonly written?: Readonly<Record<string, Array.NonEmptyReadonlyArray<Readonly<Record<string, string | number | null>>>>>;
}

// --- [MODELS] --------------------------------------------------------------------------

const person = { id: Schema.String, label: Schema.String, rank: Schema.Int };
const people = Relation.make('people', person);
const pets = Relation.make('pets', { id: Schema.String, ownerId: Schema.String });
const persons = Relation.make('persons', { id: Schema.String, title: Schema.String, rank: Schema.Int }, { renamedFrom: 'people', renamed: { title: 'label' } });
const recounted = { ...person, note: Schema.NullOr(Schema.Int) };
const unranked = Relation.make('people', Struct.omit(person, ['rank']));
const trimmed = Relation.make('people', unranked.schema.fields, { retired: ['rank'] });
const stated = Relation.make('people', { ...person, note: Schema.NullOr(Schema.String), state: Schema.String }, { defaults: { state: 'open' } });
const owned = Relation.make('people', { ...person, owner: Schema.String });
const measured = Relation.make('people', { ...person, rank: Schema.Finite }, { retype: ['rank'] });
const noted = Relation.make('people', { ...person, note: Schema.NullOr(Schema.String) });
const renoted = Relation.make('people', recounted, { retired: ['note'] });
const denoted = Relation.make('people', person, { retired: ['note'] });
const relabeled = Relation.make('people', { ...person, label: Schema.Int });
const either = Relation.make('people', { ...person, either: Schema.Union([Schema.String, Schema.Finite]) });
const preset = Relation.make('people', person, { defaults: { rank: 1.5 } });
const slugs = Relation.make('slugs', { body: Schema.fromJsonString(Schema.Struct({ slug: Schema.String })), slug: Schema.String }, { key: ['slug'], projections: { slug: { from: 'body', path: ['slug'] } } });
const seeded = {
    declared: [people, pets],
    written: {
        people: [
            { id: 'p', label: 'Parent', rank: 1 },
            { id: 'q', label: 'Second', rank: 2 },
        ],
        pets: [{ id: 'a', ownerId: 'p' }],
    },
} as const satisfies Step;
const quiet = { tables: [], columns: [], indexes: [], references: [] };
const transitions: readonly { readonly label: string; readonly steps: Array.NonEmptyReadonlyArray<Step>; readonly tables: readonly string[]; readonly stored: Readonly<Record<string, readonly object[]>>; readonly undeclared: object }[] = [
    { label: 'creates each declared table', steps: [seeded], tables: ['people', 'pets'], stored: seeded.written, undeclared: quiet },
    {
        label: 'renames a table and a column keeping rows',
        steps: [seeded, { declared: [persons, pets] }],
        tables: ['persons', 'pets'],
        stored: {
            persons: [
                { id: 'p', title: 'Parent', rank: 1 },
                { id: 'q', title: 'Second', rank: 2 },
            ],
        },
        undeclared: quiet,
    },
    {
        label: 'renames back keeping rows when the older catalog runs after the rename',
        steps: [seeded, { declared: [persons, pets], written: { persons: [{ id: 'r', title: 'Third', rank: 3 }] } }, { declared: [people, pets] }],
        tables: ['people', 'pets'],
        stored: { people: [...seeded.written.people, { id: 'r', label: 'Third', rank: 3 }] },
        undeclared: quiet,
    },
    {
        label: 'drops a retired column and a retired table',
        steps: [seeded, { declared: [trimmed], retired: ['pets'] }],
        tables: ['people'],
        stored: {
            people: [
                { id: 'p', label: 'Parent' },
                { id: 'q', label: 'Second' },
            ],
        },
        undeclared: quiet,
    },
    {
        label: 'keeps and reports a column and a table the catalog no longer declares',
        steps: [seeded, { declared: [unranked] }],
        tables: ['people', 'pets'],
        stored: seeded.written,
        undeclared: { ...quiet, tables: ['pets'], columns: [{ table: 'people', column: 'rank' }] },
    },
    {
        label: 'adds a nullable column as null and a required column as its declared default',
        steps: [seeded, { declared: [stated, pets] }],
        tables: ['people', 'pets'],
        stored: {
            people: [
                { id: 'p', label: 'Parent', rank: 1, note: null, state: 'open' },
                { id: 'q', label: 'Second', rank: 2, note: null, state: 'open' },
            ],
        },
        undeclared: quiet,
    },
    {
        label: 'adds a required column without a default while its table holds no row',
        steps: [{ declared: [people] }, { declared: [owned], written: { people: [{ id: 'p', label: 'Parent', rank: 1, owner: 'o' }] } }],
        tables: ['people'],
        stored: { people: [{ id: 'p', label: 'Parent', rank: 1, owner: 'o' }] },
        undeclared: quiet,
    },
    {
        label: 'retypes a column declared for retype keeping its values',
        steps: [seeded, { declared: [measured, pets], written: { people: [{ id: 'r', label: 'Third', rank: 2.5 }] } }],
        tables: ['people', 'pets'],
        stored: { people: [...seeded.written.people, { id: 'r', label: 'Third', rank: 2.5 }] },
        undeclared: quiet,
    },
    {
        label: 'keeps an undeclared column and its values while it retypes another column',
        steps: [{ declared: [noted], written: { people: [{ id: 'p', label: 'Parent', rank: 1, note: 'kept' }] } }, { declared: [measured] }],
        tables: ['people'],
        stored: { people: [{ id: 'p', label: 'Parent', rank: 1, note: 'kept' }] },
        undeclared: { ...quiet, columns: [{ table: 'people', column: 'note' }] },
    },
    {
        label: 'replaces a retired column with a new column of its name, holding no old value',
        steps: [{ declared: [noted], written: { people: [{ id: 'p', label: 'Parent', rank: 1, note: 'old' }] } }, { declared: [renoted] }],
        tables: ['people'],
        stored: { people: [{ id: 'p', label: 'Parent', rank: 1, note: null }] },
        undeclared: quiet,
    },
    {
        label: 'keeps a column added under a retired name when the retiring catalog runs again',
        steps: [{ declared: [noted] }, { declared: [renoted], written: { people: [{ id: 'p', label: 'Parent', rank: 1, note: 7 }] } }, { declared: [denoted] }],
        tables: ['people'],
        stored: { people: [{ id: 'p', label: 'Parent', rank: 1, note: 7 }] },
        undeclared: { ...quiet, columns: [{ table: 'people', column: 'note' }] },
    },
];
const refusals: readonly { readonly label: string; readonly declared: Catalog['tables']; readonly findings: readonly object[] }[] = [
    { label: 'a required column without a default on a table holding rows', declared: [owned, pets], findings: [{ table: 'people', columns: ['owner'], reason: { _tag: 'Required' } }] },
    { label: 'a changed column definition not declared for retype', declared: [relabeled, pets], findings: [{ table: 'people', columns: ['label'], reason: { _tag: 'Retype' } }] },
    {
        label: 'a column whose union stores text and numbers',
        declared: [either, pets],
        findings: [{ table: 'people', columns: ['either'], reason: { _tag: 'Unsupported', refusal: 'mixed' } }],
    },
    { label: 'a default its column schema refuses', declared: [preset, pets], findings: [{ table: 'people', columns: ['rank'], reason: { _tag: 'Unsupported', refusal: 'invalid' } }] },
    {
        label: 'a key column computed from another column',
        declared: [people, slugs],
        findings: [{ table: 'slugs', columns: ['slug'], reason: { _tag: 'Unsupported', refusal: 'projected' } }],
    },
];
const kinds = {
    text: Schema.String,
    count: Schema.Int,
    ratio: Schema.Finite,
    flag: Schema.BooleanFromBit,
    at: Schema.DateTimeUtcFromString,
    uuid: Column.Uuid,
    body: Schema.fromJsonString(Schema.Struct({ camelKey: Schema.Finite })),
    amount: Schema.BigDecimalFromString,
    wide: Schema.BigIntFromString,
    bytes: Column.Bytes,
    state: Schema.Literals(['open', 'closed']),
    code: Schema.String.check(Schema.isBetweenCodePoints(1, 2)),
} as const;
class Sketch extends Schema.Opaque<Sketch>()(Schema.Struct({ columns: Schema.Array(Schema.Struct({ kind: Schema.Literals(Record.keys(kinds)), nullable: Schema.Boolean, indexed: Schema.Boolean })).check(Schema.isMaxLength(5)) })) {}
const Sketches = Schema.NonEmptyArray(Sketch).check(Schema.isMaxLength(3));
const bounded = Relation.make('bounded', {
    id: Schema.String,
    ratio: Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 1 })),
    positive: Schema.Finite.check(Schema.isGreaterThan(0)),
    code: Schema.String.check(Schema.isBetweenCodePoints(1, 2)),
    state: Schema.Literals(['open', 'closed']),
    level: Schema.Literals([1, 2]),
    step: Schema.Int.check(Schema.isMultipleOf(5)),
    slug: Schema.String.check(Schema.isLowercased()),
    shout: Schema.String.check(Schema.isUppercased()),
    count: Schema.Int,
    since: Schema.DateFromString.check(DateTime.makeUnsafe('2026-01-01T00:00:00.000Z').pipe(DateTime.toDateUtc, Schema.isGreaterThanOrEqualToDate)),
    wide: Schema.BigIntFromString.check(Schema.isBetweenBigInt({ minimum: -7n, maximum: 7n })),
});
const valid = { ratio: 0.5, positive: 1, code: 'ab', state: 'open', level: 1, step: 5, slug: 'abc', shout: 'ABC', count: 1, since: '2026-06-01T00:00:00.000Z', wide: 0 };
const bounds: readonly { readonly field: keyof typeof valid; readonly accepted: string | number; readonly refused: string | number }[] = [
    { field: 'ratio', accepted: 1, refused: 1.5 },
    { field: 'ratio', accepted: 0, refused: -0.5 },
    { field: 'positive', accepted: 0.5, refused: 0 },
    { field: 'code', accepted: '😀😀', refused: '😀😀😀' },
    { field: 'code', accepted: 'a', refused: '' },
    { field: 'state', accepted: 'closed', refused: 'archived' },
    { field: 'level', accepted: 2, refused: 3 },
    { field: 'step', accepted: 10, refused: 7 },
    { field: 'slug', accepted: 'straße', refused: 'Straße' },
    { field: 'shout', accepted: 'ÉTÉ', refused: 'Été' },
    { field: 'count', accepted: 9_007_199_254_740_991, refused: 9_007_199_254_740_992 },
    { field: 'count', accepted: -9_007_199_254_740_991, refused: -9_007_199_254_740_992 },
    { field: 'since', accepted: '2026-01-01T00:00:00.000Z', refused: '2025-12-31T23:59:59.999Z' },
    { field: 'wide', accepted: 7, refused: 8 },
    { field: 'wide', accepted: -7, refused: -8 },
];

// --- [OPERATIONS] ----------------------------------------------------------------------

const sampled = ({ isolated }: Store, runs: number) => ({ arbitrary: { runs, concurrency: isolated } });
const declared = (tables: Catalog['tables'], retired: readonly string[] = []) => Catalog.make({ tables, retired, timeouts: { lock: '5 seconds', transaction: '2 minutes' } });
const stored = (table: string) => SqlClient.SqlClient.use((sql) => sql`SELECT * FROM ${sql(table)} ORDER BY 1`);
const applied = ({ command }: Store, { declared: tables, retired, written = {} }: Step) =>
    SqlClient.SqlClient.use((sql) => Effect.tap(Catalog.reconcile(declared(tables, retired)), () => Effect.forEach(Record.toEntries(written), ([table, rows]) => command(sql`INSERT INTO ${sql(table)} ${sql.insert(rows)}`), { discard: true })));
const pending = (catalog: Catalog) => Effect.map(Catalog.plan(catalog), Struct.pick(['statements', 'concurrent']));
const generated = (shape: Array.NonEmptyReadonlyArray<Sketch>) =>
    Array.map(shape, ({ columns }, table) => {
        const fields: Fields = { id: Schema.String, ...Record.fromEntries(columns.map(({ kind, nullable }, column) => [`c${column}`, nullable ? Schema.NullOr(kinds[kind]) : kinds[kind]])) };
        return Relation.make(`t${table}`, fields, { indexes: columns.flatMap(({ indexed }, column) => (indexed ? [{ columns: [`c${column}`] as const }] : [])) });
    });
const reconciling = (it: Vitest.MethodsNonLive<Engine | SqlClient.SqlClient>, store: Store) => {
    it.effect.each(transitions)(
        '$label',
        Effect.fnUntraced(function* ({ steps, tables, stored: rows, undeclared }) {
            const reports = yield* Effect.forEach(steps, (step) => applied(store, step));
            assert.deepStrictEqual({ tables: yield* store.tables, stored: yield* Effect.all(Record.map(rows, (_, table) => stored(table))), undeclared: Array.lastNonEmpty(reports) }, { tables, stored: rows, undeclared });
        }),
    );
    it.effect.each(refusals)(
        'refuses $label and changes nothing',
        Effect.fnUntraced(function* ({ declared: tables, findings }) {
            yield* applied(store, seeded);
            const refused = yield* Effect.flip(Catalog.reconcile(declared(tables)));
            assert.deepStrictEqual({ refused: refused._tag === 'ReconcileError' ? refused.findings : refused, tables: yield* store.tables, people: yield* stored('people') }, { refused: findings, tables: ['people', 'pets'], people: seeded.written.people });
        }),
    );
    it.effect.each(bounds)(
        'stores $field $accepted and refuses $refused',
        Effect.fnUntraced(function* ({ field, accepted, refused }) {
            const sql = yield* SqlClient.SqlClient;
            yield* Catalog.reconcile(declared([bounded]));
            yield* store.command(sql`INSERT INTO bounded ${sql.insert({ ...valid, id: 'accepted', [field]: accepted })}`);
            const error = yield* Effect.flip(store.command(sql`INSERT INTO bounded ${sql.insert({ ...valid, id: 'refused', [field]: refused })}`));
            assert.deepStrictEqual({ fault: store.fault(error), stored: yield* sql`SELECT id FROM bounded ORDER BY 1` }, { fault: store.check, stored: [{ id: 'accepted' }] });
        }),
    );
    it.effect(
        'plans statements for an empty store without running one',
        Effect.fnUntraced(function* () {
            const { statements } = yield* Catalog.plan(declared(seeded.declared));
            assert.deepStrictEqual({ planned: Array.isReadonlyArrayNonEmpty(statements), tables: yield* store.tables }, { planned: true, tables: [] });
        }),
    );
    it.effect(
        'finishes concurrent reconciles of one catalog with nothing left to plan',
        Effect.fnUntraced(function* () {
            const catalog = declared(seeded.declared);
            yield* Effect.all(Array.replicate(Catalog.reconcile(catalog), 4), { concurrency: 'unbounded' });
            assert.deepStrictEqual({ tables: yield* store.tables, pending: yield* pending(catalog) }, { tables: ['people', 'pets'], pending: { statements: [], concurrent: [] } });
        }),
    );
    it.effect.prop(
        'plans nothing after reconciling any generated catalog',
        { shape: Sketches },
        Effect.fnUntraced(function* ({ shape }) {
            const catalog = declared(generated(shape));
            yield* Catalog.reconcile(catalog);
            assert.deepStrictEqual({ pending: yield* pending(catalog), undeclared: yield* Catalog.reconcile(catalog) }, { pending: { statements: [], concurrent: [] }, undeclared: quiet });
        }),
        sampled(store, 12),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { declared, pending, reconciling, sampled, type Store, stored };
