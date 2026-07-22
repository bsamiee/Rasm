import { FileSystem, HttpRouter, HttpServerResponse, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { describe, expect, layer } from '@effect/vitest';
import { uuid_ossp } from '@electric-sql/pglite/contrib/uuid_ossp';
import { Array, Context, Effect, Layer, Option, Schema } from 'effect';
import type { StartedTestContainer } from 'testcontainers';
import { Containers, HarnessFault, Loopback, Loopbacks, ObjectStore, ObjectStores, PgLane, PgLanes, PinsPath } from './harness.ts';

// --- [SERVICES] ------------------------------------------------------------------------

class Held extends Context.Tag('rasm-testkit-spec/Held')<Held, StartedTestContainer>() {}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _DDL = 'CREATE TABLE marks (key TEXT PRIMARY KEY, rank INTEGER NOT NULL);';
const _BYTES = Uint8Array.from([1, 2, 3, 4]);

// Images pin in tests/containers.json — the polyglot owner every language's container row resolves through Containers.pin;
// RASM_TESTKIT_CONTAINERS activates the live lanes.
const _LIVE = process.env['RASM_TESTKIT_CONTAINERS'] !== undefined;

// --- [MODELS] --------------------------------------------------------------------------

const _Mark = Schema.Struct({ key: Schema.String, rank: Schema.Int });

// --- [OPERATIONS] ----------------------------------------------------------------------

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

    it.effect('a NOTIFY lands in the listen mailbox once the subscription scope is open', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const lane = yield* PgLane;
                const box = yield* lane.listen('band');
                yield* lane.rows('SELECT pg_notify($1, $2)', ['band', '<pulse-a>']);
                expect(yield* box.take).toBe('<pulse-a>');
            }),
        ),
    );

    it.effect('a foreign channel never leaks into the subscription', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const lane = yield* PgLane;
                const box = yield* lane.listen('band');
                yield* lane.rows('SELECT pg_notify($1, $2)', ['other', '<stray>']);
                yield* lane.rows('SELECT pg_notify($1, $2)', ['band', '<pulse-b>']);
                expect(yield* box.take).toBe('<pulse-b>');
            }),
        ),
    );

    it.effect('an arm-prefixed control frame is dropped, never delivered as a payload', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const lane = yield* PgLane;
                const box = yield* lane.listen('band');
                yield* lane.rows('SELECT pg_notify($1, $2)', ['band', '<rasm-testkit-armed:foreign>']);
                yield* lane.rows('SELECT pg_notify($1, $2)', ['band', '<pulse-c>']);
                expect(yield* box.take).toBe('<pulse-c>');
            }),
        ),
    );

    it.effect('a sandboxed write rolls back — lane state never leaks across tests', () =>
        Effect.gen(function* () {
            const lane = yield* PgLane;
            yield* lane.sandbox(
                Effect.gen(function* () {
                    yield* lane.exec("INSERT INTO marks VALUES ('<key-sandboxed>', 1);");
                    expect(yield* lane.rows("SELECT key FROM marks WHERE key = '<key-sandboxed>'")).toHaveLength(1);
                }),
            );
            expect(yield* lane.rows("SELECT key FROM marks WHERE key = '<key-sandboxed>'")).toHaveLength(0);
        }),
    );

    it.effect('a failing sandbox still rolls back and surfaces its own typed failure', () =>
        Effect.gen(function* () {
            const lane = yield* PgLane;
            const fault = yield* Effect.flip(
                lane.sandbox(Effect.zipRight(lane.exec("INSERT INTO marks VALUES ('<key-doomed>', 1);"), Effect.fail('refused' as const))),
            );
            expect(fault).toBe('refused');
            expect(yield* lane.rows("SELECT key FROM marks WHERE key = '<key-doomed>'")).toHaveLength(0);
        }),
    );

    it.effect('an extension the lane never loaded refuses typed at CREATE EXTENSION', () =>
        Effect.gen(function* () {
            const lane = yield* PgLane;
            const fault = yield* Effect.flip(lane.exec('CREATE EXTENSION IF NOT EXISTS "uuid-ossp";'));
            expect(fault).toBeInstanceOf(HarnessFault);
            expect(fault.lane).toBe('pg');
        }),
    );
});

layer(PgLanes.pglite({ extensions: { uuid_ossp }, seed: 'CREATE EXTENSION IF NOT EXISTS "uuid-ossp";' }))('pg unit lane extension row', (it) => {
    it.effect('a caller-owned extension module loads and serves its surface', () =>
        Effect.gen(function* () {
            const lane = yield* PgLane;
            const rows = yield* lane.decoded(Schema.Struct({ id: Schema.UUID }))('SELECT uuid_generate_v4()::text AS id');
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

    it.effect('listing is lexicographic: the double mirrors the real S3 ordering contract', () =>
        Effect.gen(function* () {
            const store = yield* ObjectStore;
            yield* store.put('band/late', _BYTES);
            yield* store.put('band/early', _BYTES);
            expect(yield* store.list('band/')).toEqual(['band/early', 'band/late']);
        }),
    );

    it.effect('listing orders by UTF-8 bytes past the BMP, where UTF-16 code units would swap the pair', () =>
        Effect.gen(function* () {
            const store = yield* ObjectStore;
            // U+FF01 encodes EF BC 81, U+10000 encodes F0 90 80 80: UTF-8 puts U+FF01 first, JS string order puts it last.
            yield* store.put('astral/\u{10000}', _BYTES);
            yield* store.put('astral/！', _BYTES);
            expect(yield* store.list('astral/')).toEqual(['astral/！', 'astral/\u{10000}']);
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

layer(NodeContext.layer)('image pins', (it) => {
    it.effect('the polyglot manifest resolves a pinned image reference', () =>
        Effect.gen(function* () {
            const image = yield* Containers.pin('pg');
            expect(image).toMatch(/^\S+:\S+$/);
        }),
    );

    it.effect('an unpinned lane name is honest typed absence, never a vacuous default', () =>
        Effect.gen(function* () {
            const fault = yield* Effect.flip(Containers.pin('never-pinned'));
            expect(fault).toBeInstanceOf(HarnessFault);
            expect(fault.reason).toBe('unsupported');
        }),
    );

    it.effect('a malformed pin manifest is a typed engine fault, never a module-load throw', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const path = yield* Path.Path;
                const scratch = yield* fs.makeTempDirectoryScoped();
                const target = path.join(scratch, 'containers.json');
                yield* fs.writeFileString(target, '{"pg": 7}');
                const fault = yield* Effect.flip(Effect.provideService(Containers.pin('pg'), PinsPath, target));
                expect(fault.reason).toBe('engine');
            }),
        ),
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

describe.skipIf(!_LIVE)('pg container lane [RASM_TESTKIT_CONTAINERS]', () => {
    const _pgLive = Layer.provide(
        Layer.unwrapEffect(Effect.map(Containers.pin('pg'), (image) => PgLanes.container({ image, seed: _DDL }))),
        NodeContext.layer,
    );
    layer(_pgLive, { timeout: '150 seconds' })('real server', (it) => {
        it.effect('the mapped-port driver serves the same lane algebra over the container-seeded schema', () =>
            Effect.gen(function* () {
                const lane = yield* PgLane;
                yield* lane.exec("INSERT INTO marks VALUES ('<key-a>', 3);");
                const rows = yield* lane.decoded(_Mark)('SELECT key, rank FROM marks');
                expect(rows).toEqual([{ key: '<key-a>', rank: 3 }]);
            }),
        );

        it.effect('LISTEN/NOTIFY delivers through the armed real-driver subscription', () =>
            Effect.scoped(
                Effect.gen(function* () {
                    const lane = yield* PgLane;
                    const box = yield* lane.listen('band');
                    yield* lane.rows('SELECT pg_notify($1, $2)', ['band', '<pulse-live>']);
                    expect(yield* box.take).toBe('<pulse-live>');
                }),
            ),
        );

        it.effect('the pooled-driver sandbox joins one transaction connection and rolls back on exit', () =>
            Effect.gen(function* () {
                const lane = yield* PgLane;
                yield* lane.sandbox(
                    Effect.gen(function* () {
                        yield* lane.exec("INSERT INTO marks VALUES ('<key-pooled>', 9);");
                        expect(yield* lane.rows("SELECT key FROM marks WHERE key = '<key-pooled>'")).toHaveLength(1);
                    }),
                );
                expect(yield* lane.rows("SELECT key FROM marks WHERE key = '<key-pooled>'")).toHaveLength(0);
            }),
        );

        it.effect('a second concurrent listen arms without leaking its control frames into the first mailbox', () =>
            Effect.scoped(
                Effect.gen(function* () {
                    const lane = yield* PgLane;
                    const first = yield* lane.listen('band');
                    yield* lane.listen('band');
                    yield* lane.rows('SELECT pg_notify($1, $2)', ['band', '<pulse-shared>']);
                    expect(yield* first.take).toBe('<pulse-shared>');
                }),
            ),
        );
    });
});

describe.skipIf(!_LIVE)('container substrate [RASM_TESTKIT_CONTAINERS]', () => {
    const _seeded = Layer.provide(
        Layer.scoped(
            Held,
            Effect.gen(function* () {
                const image = yield* Containers.pin('pg');
                return yield* Containers.start({
                    ...Containers.pg({ image }),
                    copy: [{ content: 'SELECT 1;', target: '/probe.sql' }],
                    labels: { 'rasm.testkit': 'substrate' },
                });
            }),
        ),
        NodeContext.layer,
    );
    layer(_seeded, { timeout: '150 seconds' })('copy and exec rows', (it) => {
        it.effect('a copy row lands its content in the image and exec reads it back', () =>
            Effect.gen(function* () {
                const held = yield* Held;
                const receipt = yield* Containers.exec(held, ['cat', '/probe.sql']);
                expect(receipt.exitCode).toBe(0);
                expect(receipt.output).toContain('SELECT 1;');
            }),
        );

        it.effect('a failing in-container command reports its exit code as data, never a green pass', () =>
            Effect.gen(function* () {
                const held = yield* Held;
                const receipt = yield* Containers.exec(held, ['cat', '/absent-file']);
                expect(receipt.exitCode).not.toBe(0);
            }),
        );
    });

    const _networked = Layer.provide(
        Layer.scoped(
            Held,
            Effect.gen(function* () {
                const image = yield* Containers.pin('pg');
                const net = yield* Containers.network;
                return yield* Containers.start({
                    ...Containers.pg({ image }),
                    net: { network: net, aliases: ['kit-pg'] },
                    resources: { cpu: 1, memory: 0.5 },
                });
            }),
        ),
        NodeContext.layer,
    );
    layer(_networked, { timeout: '150 seconds' })('network and quota rows', (it) => {
        it.effect('the net row registers its alias on the network-embedded DNS', () =>
            Effect.gen(function* () {
                const held = yield* Held;
                const receipt = yield* Containers.exec(held, ['getent', 'hosts', 'kit-pg']);
                expect(receipt.exitCode).toBe(0);
                expect(receipt.output).toContain('kit-pg');
            }),
        );

        it.effect('the resources row lands as a real cgroup memory ceiling', () =>
            Effect.gen(function* () {
                const held = yield* Held;
                // cgroup v2 first, v1 fallback: 0.5 GB rides testcontainers' GB unit into 536870912 bytes.
                const receipt = yield* Containers.exec(held, [
                    'sh',
                    '-c',
                    'cat /sys/fs/cgroup/memory.max 2>/dev/null || cat /sys/fs/cgroup/memory/memory.limit_in_bytes',
                ]);
                expect(receipt.exitCode).toBe(0);
                expect(receipt.output).toContain('536870912');
            }),
        );
    });
});

describe.skipIf(!_LIVE)('object store container lane [RASM_TESTKIT_CONTAINERS]', () => {
    const _live = Layer.provide(
        Layer.unwrapScoped(
            Effect.gen(function* () {
                const image = yield* Containers.pin('store');
                const row = Containers.objectStore({ image, rootUser: 'testing', rootPassword: 'testing-secret' });
                const started = yield* Containers.start(row);
                return ObjectStores.s3({
                    // The service port is the row's own fact: the mapped-port read can never drift from the builder.
                    endpoint: `http://${started.getHost()}:${started.getMappedPort(Array.headNonEmpty(row.ports))}`,
                    bucket: 'kit',
                    accessKeyId: 'testing',
                    secretAccessKey: 'testing-secret',
                });
            }),
        ),
        NodeContext.layer,
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

        it.effect('an absent key resolves typed None through the NoSuchKey code, never a thrown escape', () =>
            Effect.gen(function* () {
                const store = yield* ObjectStore;
                expect(yield* store.get('band/never-written')).toEqual(Option.none());
            }),
        );
    });
});
