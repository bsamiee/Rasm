import { HttpRouter, HttpServerResponse } from '@effect/platform';
import { expect, layer } from '@effect/vitest';
import { uuid_ossp } from '@electric-sql/pglite/contrib/uuid_ossp';
import { Effect, Option, Schema } from 'effect';
import {
    TestResourceError,
    Loopback,
    LoopbackServers,
    ObjectStore,
    ObjectStoreDoubles,
    TestDatabase,
    TestDatabases,
} from './resources.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _DDL =
    'CREATE TABLE records (key TEXT PRIMARY KEY, priority INTEGER NOT NULL);';
const _BYTES = Uint8Array.from([1, 2, 3, 4]);

// --- [MODELS] --------------------------------------------------------------------------

const _Record = Schema.Struct({ key: Schema.String, priority: Schema.Int });

// --- [OPERATIONS] ----------------------------------------------------------------------

layer(TestDatabases.pglite(_DDL))('PGlite test database', (it) => {
    it.effect('seeded DDL round-trips through schema-decoded queries', () =>
        Effect.gen(function* () {
            const database = yield* TestDatabase;
            yield* database.exec(
                "INSERT INTO records VALUES ('<key-a>', 3), ('<key-b>', 7);",
            );
            const rows = yield* database.decoded(_Record)(
                'SELECT key, priority FROM records ORDER BY priority DESC',
            );
            expect(rows).toEqual([
                { key: '<key-b>', priority: 7 },
                { key: '<key-a>', priority: 3 },
            ]);
        }),
    );

    it.effect(
        'an invalid statement returns a typed database error without throwing',
        () =>
            Effect.gen(function* () {
                const database = yield* TestDatabase;
                const error = yield* Effect.flip(
                    database.exec('SELECT FROM nowhere ('),
                );
                expect(error).toBeInstanceOf(TestResourceError);
                expect(error.resource).toBe('database');
            }),
    );

    it.effect(
        'NOTIFY delivers one payload after the subscription scope opens',
        () =>
            Effect.scoped(
                Effect.gen(function* () {
                    const database = yield* TestDatabase;
                    const mailbox = yield* database.listen('events');
                    yield* database.rows('SELECT pg_notify($1, $2)', [
                        'events',
                        '<event-a>',
                    ]);
                    expect(yield* mailbox.take).toBe('<event-a>');
                }),
            ),
    );

    it.effect('the subscription excludes messages from other channels', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const database = yield* TestDatabase;
                const mailbox = yield* database.listen('events');
                yield* database.rows('SELECT pg_notify($1, $2)', [
                    'other',
                    '<foreign-event>',
                ]);
                yield* database.rows('SELECT pg_notify($1, $2)', [
                    'events',
                    '<event-b>',
                ]);
                expect(yield* mailbox.take).toBe('<event-b>');
            }),
        ),
    );

    it.effect(
        'a control notification is not delivered as application payload',
        () =>
            Effect.scoped(
                Effect.gen(function* () {
                    const database = yield* TestDatabase;
                    const mailbox = yield* database.listen('events');
                    yield* database.rows('SELECT pg_notify($1, $2)', [
                        'events',
                        '<rasm-test-support-control:external>',
                    ]);
                    yield* database.rows('SELECT pg_notify($1, $2)', [
                        'events',
                        '<event-c>',
                    ]);
                    expect(yield* mailbox.take).toBe('<event-c>');
                }),
            ),
    );

    it.effect(
        'a rollback-only transaction does not retain database state across tests',
        () =>
            Effect.gen(function* () {
                const database = yield* TestDatabase;
                yield* database.rollbackTransaction(
                    Effect.gen(function* () {
                        yield* database.exec(
                            "INSERT INTO records VALUES ('<key-transaction>', 1);",
                        );
                        expect(
                            yield* database.rows(
                                "SELECT key FROM records WHERE key = '<key-transaction>'",
                            ),
                        ).toHaveLength(1);
                    }),
                );
                expect(
                    yield* database.rows(
                        "SELECT key FROM records WHERE key = '<key-transaction>'",
                    ),
                ).toHaveLength(0);
            }),
    );

    it.effect(
        'a failed transaction rolls back and preserves the original error',
        () =>
            Effect.gen(function* () {
                const database = yield* TestDatabase;
                const error = yield* Effect.flip(
                    database.rollbackTransaction(
                        Effect.zipRight(
                            database.exec(
                                "INSERT INTO records VALUES ('<key-rollback>', 1);",
                            ),
                            Effect.fail('rejected' as const),
                        ),
                    ),
                );
                expect(error).toBe('rejected');
                expect(
                    yield* database.rows(
                        "SELECT key FROM records WHERE key = '<key-rollback>'",
                    ),
                ).toHaveLength(0);
            }),
    );

    it.effect('an unavailable extension returns a typed database error', () =>
        Effect.gen(function* () {
            const database = yield* TestDatabase;
            const error = yield* Effect.flip(
                database.exec('CREATE EXTENSION IF NOT EXISTS "uuid-ossp";'),
            );
            expect(error).toBeInstanceOf(TestResourceError);
            expect(error.resource).toBe('database');
        }),
    );
});

layer(
    TestDatabases.pglite({
        extensions: { uuid_ossp },
        seed: 'CREATE EXTENSION IF NOT EXISTS "uuid-ossp";',
    }),
)('PGlite extension configuration', (it) => {
    it.effect('a configured extension exposes its SQL function', () =>
        Effect.gen(function* () {
            const database = yield* TestDatabase;
            const rows = yield* database.decoded(
                Schema.Struct({ id: Schema.UUID }),
            )('SELECT uuid_generate_v4()::text AS id');
            expect(rows).toHaveLength(1);
        }),
    );
});

layer(ObjectStoreDoubles.memory)('in-memory object store', (it) => {
    it.effect(
        'put, get, list, and remove implement the object-store contract',
        () =>
            Effect.gen(function* () {
                const store = yield* ObjectStore;
                yield* store.put('objects/one', _BYTES);
                expect(yield* store.get('objects/one')).toEqual(
                    Option.some(_BYTES),
                );
                expect(yield* store.get('objects/absent')).toEqual(
                    Option.none(),
                );
                expect(yield* store.list('objects/')).toEqual(['objects/one']);
                yield* store.remove('objects/one');
                expect(yield* store.get('objects/one')).toEqual(Option.none());
            }),
    );

    it.effect(
        'listing is lexicographic: the double mirrors the real S3 ordering contract',
        () =>
            Effect.gen(function* () {
                const store = yield* ObjectStore;
                yield* store.put('objects/late', _BYTES);
                yield* store.put('objects/early', _BYTES);
                expect(yield* store.list('objects/')).toEqual([
                    'objects/early',
                    'objects/late',
                ]);
            }),
    );

    it.effect(
        'listing orders by UTF-8 bytes past the BMP, where UTF-16 code units would swap the pair',
        () =>
            Effect.gen(function* () {
                const store = yield* ObjectStore;
                yield* store.put('astral/\u{10000}', _BYTES);
                yield* store.put('astral/！', _BYTES);
                expect(yield* store.list('astral/')).toEqual([
                    'astral/！',
                    'astral/\u{10000}',
                ]);
            }),
    );

    it.effect(
        'presigning returns a typed unsupported error for the in-memory store',
        () =>
            Effect.gen(function* () {
                const store = yield* ObjectStore;
                const error = yield* Effect.flip(store.url('objects/one', 60));
                expect(error.reason).toBe('unsupported');
            }),
    );
});

layer(
    LoopbackServers.serve(
        HttpRouter.empty.pipe(
            HttpRouter.get('/ping', HttpServerResponse.text('pong')),
        ),
    ),
)('loopback HTTP server', (it) => {
    it.effect('the server exposes a live endpoint and configured client', () =>
        Effect.gen(function* () {
            const loop = yield* Loopback;
            expect(loop.url).toMatch(/^http:\/\/.+:\d+$/);
            const reply = yield* Effect.scoped(loop.client.get('/ping'));
            expect(reply.status).toBe(200);
            expect(yield* reply.text).toBe('pong');
        }),
    );

    it.effect('an unrouted path returns 404 over the loopback socket', () =>
        Effect.gen(function* () {
            const loop = yield* Loopback;
            const reply = yield* Effect.scoped(loop.client.get('/absent'));
            expect(reply.status).toBe(404);
        }),
    );
});
