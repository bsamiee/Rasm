import { HttpRouter, HttpServerResponse } from '@effect/platform';
import { describe, expect, layer } from '@effect/vitest';
import { Effect, Layer, Option, Schema } from 'effect';
import { Containers, HarnessFault, Loopback, Loopbacks, ObjectStore, ObjectStores, PgLane, PgLanes } from './harness.ts';

// --- [CONSTANTS] -------------------------------------------------------------------------

const _DDL = 'CREATE TABLE marks (key TEXT PRIMARY KEY, rank INTEGER NOT NULL);';
const _BYTES = Uint8Array.from([1, 2, 3, 4]);

// Container-lane activation: the shared images are unpinned, so the live lane is env-opted; the unit lanes gate every run.
const _PG_IMAGE = process.env['RASM_TESTKIT_PG_IMAGE'];
const _STORE_IMAGE = process.env['RASM_TESTKIT_STORE_IMAGE'];

// --- [MODELS] ----------------------------------------------------------------------------

const _Mark = Schema.Struct({ key: Schema.String, rank: Schema.Int });

// --- [OPERATIONS] ------------------------------------------------------------------------

layer(PgLanes.pglite(_DDL))('pg unit lane', (it) => {
    it.effect('seeded DDL round-trips through the decode-fused read', () =>
        Effect.gen(function* () {
            const lane = yield* PgLane;
            yield* lane.exec("INSERT INTO marks VALUES ('<key-a>', 3), ('<key-b>', 7);");
            const rows = yield* lane.decoded(_Mark)('SELECT key, rank FROM marks ORDER BY rank DESC');
            expect(rows).toEqual([
                { key: '<key-b>', rank: 7 },
                { key: '<key-a>', rank: 3 },
            ]);
        }),
    );

    it.effect('a broken statement is a typed engine fault, never a thrown escape', () =>
        Effect.gen(function* () {
            const lane = yield* PgLane;
            const fault = yield* Effect.flip(lane.exec('SELECT FROM nowhere ('));
            expect(fault).toBeInstanceOf(HarnessFault);
            expect(fault.lane).toBe('pg');
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

    it.effect('presign on the double refuses typed: the capability belongs to the real lane', () =>
        Effect.gen(function* () {
            const store = yield* ObjectStore;
            const fault = yield* Effect.flip(store.url('band/one', 60));
            expect(fault.reason).toBe('unsupported');
        }),
    );
});

layer(Loopbacks.serve(HttpRouter.empty.pipe(HttpRouter.get('/ping', HttpServerResponse.text('pong')))))('loopback capsule', (it) => {
    it.effect('the capsule yields a live endpoint and a base-wired client', () =>
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

describe.skipIf(_PG_IMAGE === undefined)('pg container lane [RASM_TESTKIT_PG_IMAGE]', () => {
    layer(PgLanes.container({ image: _PG_IMAGE ?? 'unset' }), { timeout: '150 seconds' })('real server', (it) => {
        it.effect('the mapped-port driver serves the same lane algebra', () =>
            Effect.gen(function* () {
                const lane = yield* PgLane;
                yield* lane.exec(_DDL);
                yield* lane.exec("INSERT INTO marks VALUES ('<key-a>', 3);");
                const rows = yield* lane.decoded(_Mark)('SELECT key, rank FROM marks');
                expect(rows).toEqual([{ key: '<key-a>', rank: 3 }]);
            }),
        );
    });
});

describe.skipIf(_STORE_IMAGE === undefined)('object store container lane [RASM_TESTKIT_STORE_IMAGE]', () => {
    const _live = Layer.unwrapScoped(
        Effect.map(
            Containers.start(Containers.objectStore({ image: _STORE_IMAGE ?? 'unset', rootUser: 'testing', rootPassword: 'testing-secret' })),
            (started) =>
                ObjectStores.s3({
                    endpoint: `http://${started.getHost()}:${started.getMappedPort(9000)}`,
                    bucket: 'kit',
                    accessKeyId: 'testing',
                    secretAccessKey: 'testing-secret',
                }),
        ),
    );
    layer(_live, { timeout: '150 seconds' })('real store', (it) => {
        it.effect('bytes round-trip and the presigned URL serves them', () =>
            Effect.gen(function* () {
                const store = yield* ObjectStore;
                yield* store.put('band/one', _BYTES);
                expect(yield* store.get('band/one')).toEqual(Option.some(_BYTES));
                const url = yield* store.url('band/one', 60);
                const fetched = yield* Effect.promise(() => fetch(url).then((reply) => reply.arrayBuffer()));
                expect(new Uint8Array(fetched)).toEqual(_BYTES);
            }),
        );
    });
});
