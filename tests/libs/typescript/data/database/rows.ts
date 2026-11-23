import { assert, type Vitest } from '@effect/vitest';
import { Catalog, Relation, Rows, Tokenizer } from '@rasm/database';
import { Array, BigDecimal, DateTime, Effect, Equal, Option, Order, Predicate, Schema, String, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { SqlClient } from 'effect/sql';
import { accounts, blobs, children, declared, events, isolated, notes, parents, refused, values, words } from '#/fixtures';

// --- [MODELS] --------------------------------------------------------------------------

const ValueRows = Schema.Array(values.schema);
const fragments = Relation.make('fragments', notes.schema.fields, { search: { ...words, tokenizer: Tokenizer.Trigram() } });

// --- [OPERATIONS] ----------------------------------------------------------------------

const uuid = SecureRandom.SecureRandom.use(({ uuidV7 }) => uuidV7());
const account = (id: string, email: string, deleted: boolean) => ({ id, email, deletedAt: deleted ? DateTime.makeUnsafe('2026-10-17T00:00:00.000Z') : null, profile: { plan: 'pro' } });
const rows = <R, F>(it: Vitest.MethodsNonLive<R>, client: Parameters<typeof isolated<F, NoInfer<R>>>[0]) => {
    const run = isolated(client);
    it.effect(
        'maps camelCase fields to snake_case columns in every statement form',
        Effect.fnUntraced(
            function* () {
                const sql = yield* SqlClient.SqlClient;
                yield* Rows.atomic([sql`INSERT INTO parents ${sql.insert({ id: 'p', label: 'Parent', state: 'open' })}`, sql`INSERT INTO children ${sql.insert({ id: 'a', parentId: 'p', rank: 1 })}`]);
                yield* sql`UPDATE children SET ${sql.update({ parentId: 'p', rank: 2 })} WHERE ${sql('parentId')} = ${'p'}`;
                assert.deepStrictEqual(yield* Relation.select(children, { parentId: ['p'] }), [{ id: 'a', parentId: 'p', rank: 2 }]);
                assert.deepStrictEqual(yield* sql.withoutTransforms()`SELECT parent_id FROM children`, [{ parent_id: 'p' }]);
            },
            Effect.provide(Catalog.layer(declared([parents, children]), client)),
        ),
    );
    it.effect(
        'stores 64-bit integers, exact decimals, JSON keys, bytes, instants, booleans, options, and bounded values',
        run(function* (sql) {
            const int64 = 9_223_372_036_854_775_808n;
            const byteMaximum = 255;
            const [first, second] = yield* Effect.all([uuid, uuid]);
            const written = Array.make(
                { id: first, count: int64 - 1n, amount: BigDecimal.fromStringUnsafe('12345678901234567890.123456789'), body: { camelKey: Math.PI }, bytes: Uint8Array.of(0, byteMaximum, 0), at: DateTime.makeUnsafe('2026-10-17T12:00:00.123Z'), flag: true, ratio: 0, code: '😀😀', note: Option.some('kept') },
                { id: second, count: -int64, amount: BigDecimal.fromStringUnsafe('-0.000000000000000001'), body: { camelKey: -Number.MIN_VALUE }, bytes: new Uint8Array(), at: DateTime.makeUnsafe('1969-07-20T20:17:40.500Z'), flag: false, ratio: 1, code: 'a', note: Option.none() },
            );
            const digests = Array.make({ digest: Uint8Array.of(byteMaximum, 0), size: 1 }, { digest: new Uint8Array(), size: 0 });
            yield* Catalog.reconcile(declared([values, blobs]));
            const encoded = Schema.encodeEffect(ValueRows);
            assert.deepStrictEqual(
                yield* Effect.all([Effect.flatMap(Relation.insert(values, written), encoded), Effect.flatMap(Relation.select(values, { id: [first] }), encoded), Effect.flatMap(Relation.select(values, { count: [-int64] }), encoded)]),
                yield* Effect.all([encoded(written), encoded([Array.headNonEmpty(written)]), encoded([Array.lastNonEmpty(written)])]),
            );
            yield* Relation.insert(blobs, digests);
            assert.deepStrictEqual(yield* Relation.select(blobs, { digest: [Array.lastNonEmpty(digests).digest] }), [Array.lastNonEmpty(digests)]);
            assert.deepStrictEqual(yield* Relation.remove(blobs, { digest: [Array.headNonEmpty(digests).digest] }), [Array.headNonEmpty(digests)]);
            assert.strictEqual((yield* Effect.flip(Relation.insert(values, [{ ...Array.headNonEmpty(written), id: yield* uuid, ratio: 2 }])))._tag, 'SchemaError');
            yield* refused(sql`UPDATE ${sql('values')} SET ${sql.update({ ratio: 2 })}`, 'ConstraintError');
            yield* refused(sql`UPDATE ${sql('values')} SET ${sql.update({ code: '😀😀😀' })}`, 'ConstraintError');
        }),
    );
    it.effect(
        'writes and reads collections as one parameter with upsert, ensure, remove, and newest rows per group',
        run(function* () {
            const volume = 3000;
            const tenants = [2, 0, 1] as const;
            const newest = 2;
            yield* Catalog.reconcile(declared([events]));
            const at = DateTime.makeUnsafe('2026-01-01T00:00:00.000Z');
            const written = Array.makeBy(volume, (index) => ({ id: `e${index}`, tenant: index % tenants.length, at: DateTime.add(at, { seconds: index }), kind: 'view' as const }));
            const [head, last] = [Array.headNonEmpty(written), Array.lastNonEmpty(written)];
            const descending = (kept: Predicate.Predicate<number>) => Array.reverse(written.filter(({ tenant }) => kept(tenant))).map(Struct.get('id'));
            assert.deepStrictEqual(Array.sortWith(yield* Relation.insert(events, written), Struct.get('id'), String.Order), Array.sortWith(written, Struct.get('id'), String.Order));
            assert.deepStrictEqual(yield* Relation.upsert(events, written), []);
            assert.deepStrictEqual(yield* Relation.upsert(events, [{ ...head, kind: 'buy', at: DateTime.add(head.at, { days: 1 }) }], 'at'), [{ row: { ...head, kind: 'buy' }, inserted: false }]);
            assert.strictEqual((yield* Effect.flip(Relation.upsert(events, [head, head])))._tag, 'SchemaError');
            assert.deepStrictEqual(
                (yield* Relation.select(events, { tenant: tenants }, { order: [{ descending: 'at' }], limit: newest, per: 'tenant' })).map(Struct.get('id')),
                Array.sort(tenants, Order.Number).flatMap((tenant) => Array.take(descending(Equal.equals(tenant)), newest)),
            );
            assert.lengthOf(yield* Relation.remove(events, { tenant: [0] }), volume / tenants.length);
            assert.deepStrictEqual((yield* Relation.select(events, { tenant: tenants }, { order: [{ descending: 'at' }] })).map(Struct.get('id')), descending(Predicate.not(Equal.equals(0))));
            assert.deepStrictEqual(yield* Relation.ensure(events, [head, { ...last, kind: 'buy' }]), [head, last]);
        }),
    );
    it.effect(
        'enforces partial case-insensitive unique keys and reads virtual columns from JSON and predicates',
        run(function* () {
            yield* Catalog.reconcile(declared([accounts]));
            yield* Relation.insert(accounts, [account(yield* uuid, 'A@example.com', true), account(yield* uuid, 'a@example.com', false)]);
            yield* refused(Relation.insert(accounts, [account(yield* uuid, 'A@EXAMPLE.COM', false)]), 'UniqueViolation');
            assert.deepStrictEqual((yield* Relation.select(accounts, { email: ['a@example.com', 'A@example.com'] }, { order: ['email'] })).map(Struct.pick(['email', 'plan', 'active'])), [
                { email: 'A@example.com', plan: 'pro', active: false },
                { email: 'a@example.com', plan: 'pro', active: true },
            ]);
        }),
    );
    it.effect(
        'searches declared text columns by word and trigram across updates and escapes query syntax',
        run(function* (sql) {
            yield* Catalog.reconcile(declared([notes, fragments]));
            const written = Array.make({ id: 1, title: 'Concrete shell', body: 'Thin vaults' }, { id: 2, title: 'Timber frame', body: 'Glulam beams' }, { id: 3, title: 'Café crème', body: 'Voûte mince' });
            const titles = Effect.map((found: readonly { readonly row: { readonly title: string } }[]) => found.map(Struct.get('row')).map(Struct.get('title')));
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
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { rows };
