import { HttpRouter, HttpServerResponse } from '@effect/platform';
import { expect, layer } from '@effect/vitest';
import { uuid_ossp } from '@electric-sql/pglite/contrib/uuid_ossp';
import { Effect, Option, Schema } from 'effect';
import {
    HarnessFault,
    Loopback,
    Loopbacks,
    ObjectStore,
    ObjectStores,
    PgLane,
    PgLanes,
} from './harness.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _DDL =
    'CREATE TABLE marks (key TEXT PRIMARY KEY, rank INTEGER NOT NULL);';
const _BYTES = Uint8Array.from([1, 2, 3, 4]);

// --- [MODELS] --------------------------------------------------------------------------

const _Mark = Schema.Struct({ key: Schema.String, rank: Schema.Int });

// --- [OPERATIONS] ----------------------------------------------------------------------

layer(PgLanes.pglite(_DDL))('pg unit lane', (it) => {
    it.effect('seeded DDL round-trips through the decode-fused read', () =>
        Effect.gen(function* () {
            const lane = yield* PgLane;
            yield* lane.exec(
                "INSERT INTO marks VALUES ('<key-a>', 3), ('<key-b>', 7);",
            );
            const rows = yield* lane.decoded(_Mark)(
                'SELECT key, rank FROM marks ORDER BY rank DESC',
            );
            expect(rows).toEqual([
                { key: '<key-b>', rank: 7 },
                { key: '<key-a>', rank: 3 },
            ]);
        }),
    );

    it.effect(
        'a broken statement is a typed engine fault, never a thrown escape',
        () =>
            Effect.gen(function* () {
                const lane = yield* PgLane;
                const fault = yield* Effect.flip(
                    lane.exec('SELECT FROM nowhere ('),
                );
                expect(fault).toBeInstanceOf(HarnessFault);
                expect(fault.lane).toBe('pg');
            }),
    );

    it.effect(
        'a NOTIFY lands in the listen mailbox once the subscription scope is open',
        () =>
            Effect.scoped(
                Effect.gen(function* () {
                    const lane = yield* PgLane;
                    const box = yield* lane.listen('band');
                    yield* lane.rows('SELECT pg_notify($1, $2)', [
                        'band',
                        '<pulse-a>',
                    ]);
                    expect(yield* box.take).toBe('<pulse-a>');
                }),
            ),
    );

    it.effect('a foreign channel never leaks into the subscription', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const lane = yield* PgLane;
                const box = yield* lane.listen('band');
                yield* lane.rows('SELECT pg_notify($1, $2)', [
                    'other',
                    '<stray>',
                ]);
                yield* lane.rows('SELECT pg_notify($1, $2)', [
                    'band',
                    '<pulse-b>',
                ]);
                expect(yield* box.take).toBe('<pulse-b>');
            }),
        ),
    );

    it.effect(
        'an arm-prefixed control frame is dropped, never delivered as a payload',
        () =>
            Effect.scoped(
                Effect.gen(function* () {
                    const lane = yield* PgLane;
                    const box = yield* lane.listen('band');
                    yield* lane.rows('SELECT pg_notify($1, $2)', [
                        'band',
                        '<rasm-testkit-armed:foreign>',
                    ]);
                    yield* lane.rows('SELECT pg_notify($1, $2)', [
                        'band',
                        '<pulse-c>',
                    ]);
                    expect(yield* box.take).toBe('<pulse-c>');
                }),
            ),
    );

    it.effect(
        'a sandboxed write rolls back — lane state never leaks across tests',
        () =>
            Effect.gen(function* () {
                const lane = yield* PgLane;
                yield* lane.sandbox(
                    Effect.gen(function* () {
                        yield* lane.exec(
                            "INSERT INTO marks VALUES ('<key-sandboxed>', 1);",
                        );
                        expect(
                            yield* lane.rows(
                                "SELECT key FROM marks WHERE key = '<key-sandboxed>'",
                            ),
                        ).toHaveLength(1);
                    }),
                );
                expect(
                    yield* lane.rows(
                        "SELECT key FROM marks WHERE key = '<key-sandboxed>'",
                    ),
                ).toHaveLength(0);
            }),
    );

    it.effect(
        'a failing sandbox still rolls back and surfaces its own typed failure',
        () =>
            Effect.gen(function* () {
                const lane = yield* PgLane;
                const fault = yield* Effect.flip(
                    lane.sandbox(
                        Effect.zipRight(
                            lane.exec(
                                "INSERT INTO marks VALUES ('<key-doomed>', 1);",
                            ),
                            Effect.fail('refused' as const),
                        ),
                    ),
                );
                expect(fault).toBe('refused');
                expect(
                    yield* lane.rows(
                        "SELECT key FROM marks WHERE key = '<key-doomed>'",
                    ),
                ).toHaveLength(0);
            }),
    );

    it.effect(
        'an extension the lane never loaded refuses typed at CREATE EXTENSION',
        () =>
            Effect.gen(function* () {
                const lane = yield* PgLane;
                const fault = yield* Effect.flip(
                    lane.exec('CREATE EXTENSION IF NOT EXISTS "uuid-ossp";'),
                );
                expect(fault).toBeInstanceOf(HarnessFault);
                expect(fault.lane).toBe('pg');
            }),
    );
});

layer(
    PgLanes.pglite({
        extensions: { uuid_ossp },
        seed: 'CREATE EXTENSION IF NOT EXISTS "uuid-ossp";',
    }),
)('pg unit lane extension row', (it) => {
    it.effect(
        'a caller-owned extension module loads and serves its surface',
        () =>
            Effect.gen(function* () {
                const lane = yield* PgLane;
                const rows = yield* lane.decoded(
                    Schema.Struct({ id: Schema.UUID }),
                )('SELECT uuid_generate_v4()::text AS id');
                expect(rows).toHaveLength(1);
            }),
    );
});

layer(ObjectStores.memory)('object store double', (it) => {
    it.effect('the filesystem algebra holds: put, get, list, remove', () =>
        Effect.gen(function* () {
            const store = yield* ObjectStore;
            yield* store.put('band/one', _BYTES);
            expect(yield* store.get('band/one')).toEqual(Option.some(_BYTES));
            expect(yield* store.get('band/absent')).toEqual(Option.none());
            expect(yield* store.list('band/')).toEqual(['band/one']);
            yield* store.remove('band/one');
            expect(yield* store.get('band/one')).toEqual(Option.none());
        }),
    );

    it.effect(
        'listing is lexicographic: the double mirrors the real S3 ordering contract',
        () =>
            Effect.gen(function* () {
                const store = yield* ObjectStore;
                yield* store.put('band/late', _BYTES);
                yield* store.put('band/early', _BYTES);
                expect(yield* store.list('band/')).toEqual([
                    'band/early',
                    'band/late',
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
        'presign on the double refuses typed: the capability belongs to a real store',
        () =>
            Effect.gen(function* () {
                const store = yield* ObjectStore;
                const fault = yield* Effect.flip(store.url('band/one', 60));
                expect(fault.reason).toBe('unsupported');
            }),
    );
});

layer(
    Loopbacks.serve(
        HttpRouter.empty.pipe(
            HttpRouter.get('/ping', HttpServerResponse.text('pong')),
        ),
    ),
)('loopback capsule', (it) => {
    it.effect(
        'the capsule yields a live endpoint and a base-wired client',
        () =>
            Effect.gen(function* () {
                const loop = yield* Loopback;
                expect(loop.url).toMatch(/^http:\/\/.+:\d+$/);
                const reply = yield* Effect.scoped(loop.client.get('/ping'));
                expect(reply.status).toBe(200);
                expect(yield* reply.text).toBe('pong');
            }),
    );

    it.effect('an unrouted path surfaces as a real 404 across the socket', () =>
        Effect.gen(function* () {
            const loop = yield* Loopback;
            const reply = yield* Effect.scoped(loop.client.get('/absent'));
            expect(reply.status).toBe(404);
        }),
    );
});
