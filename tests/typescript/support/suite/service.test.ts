import { GetObjectCommand, PutObjectCommand, S3Client, type S3ClientConfig } from '@aws-sdk/client-s3';
import { assert, it } from '@effect/vitest';
import { Array, Cause, Deferred, Effect, Exit, flow, Layer, Option, Ref, Result, Schema, Scope } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { FetchHttpClient, HttpBody, HttpClient } from 'effect/http';
import { createClient, type RedisClientOptions } from 'redis';
import { Store, StoreError } from '#/store';
import { failed } from '#/vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const callers = 4;
const refusing = 20_000;

// --- [OPERATIONS] ----------------------------------------------------------------------

const opened = (options: RedisClientOptions) =>
    Effect.acquireRelease(
        Effect.tryPromise(() => {
            const client = createClient({ ...options, socket: { ...options.socket, reconnectStrategy: false } });
            return client.connect().then(() => client);
        }),
        (client) => Effect.sync(() => client.destroy()),
    );

const stored = ({ bucket, client }: { readonly bucket: string; readonly client: S3ClientConfig }) =>
    Effect.acquireUseRelease(
        Effect.sync(() => new S3Client(client)),
        (storage) =>
            Effect.promise(() =>
                storage
                    .send(new PutObjectCommand({ Bucket: bucket, Key: 'held', Body: 'stored' }))
                    .then(() => storage.send(new GetObjectCommand({ Bucket: bucket, Key: 'held' })))
                    .then(({ Body }) => Body?.transformToString()),
            ),
        (storage) => Effect.sync(() => storage.destroy()),
    );

const pinged = (options: RedisClientOptions) => Effect.scoped(Effect.flatMap(opened(options), (client) => Effect.promise(() => client.ping())));

const asked = ({ url, database, username, password }: { readonly url: string; readonly database: string; readonly username: string; readonly password: string }, query: string) =>
    HttpClient.post(url, { urlParams: { database }, headers: { 'X-ClickHouse-User': username, 'X-ClickHouse-Key': password }, body: HttpBody.text(query) }).pipe(
        Effect.flatMap((answer) => answer.text),
        Effect.map((text) => text.trim()),
        Effect.orDie,
        Effect.provide(FetchHttpClient.layer),
    );

const marked = (builds: Ref.Ref<number>) => (build: Parameters<typeof asked>[0]) =>
    Effect.andThen(
        Ref.update(builds, (built) => built + 1),
        asked(build, "create table marks engine = MergeTree order by mark as select 'held' as mark"),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const cache = Store.use((store) => store.cache);

it.layer(Layer.merge(Store.layer(undefined, failed), SecureRandom.layerWebCrypto), { excludeTestServices: true })('store service', (methods) => {
    methods.effect(
        'serves each kind through the options its catalog driver takes',
        Effect.fnUntraced(function* () {
            const held = yield* Store.use((store) => Effect.all({ bucket: store.bucket, cache: store.cache, clickhouse: store.clickhouse, postgres: store.postgres }));
            assert.deepStrictEqual([yield* stored(held.bucket), yield* pinged(held.cache.client), yield* asked(held.clickhouse, 'select currentDatabase()'), held.postgres.database.length > 0], ['stored', 'PONG', held.clickhouse.database, true]);
        }),
    );
    methods.effect(
        'drops the store of a scope that passed and keeps the store of one that failed',
        Effect.fnUntraced(function* () {
            const seen = yield* Deferred.make<RedisClientOptions>();
            const passed = yield* Effect.scoped(Effect.map(cache, ({ client }) => client));
            const refused = yield* Effect.gen(function* () {
                yield* Deferred.succeed(seen, (yield* cache).client);
                return yield* Effect.die('assertion');
            }).pipe(Effect.scoped, Effect.exit);
            assert.deepStrictEqual([Exit.isFailure(yield* Effect.exit(pinged(passed))), refused, yield* pinged(yield* Deferred.await(seen))], [true, Exit.die('assertion'), 'PONG']);
        }),
    );
    methods.effect(
        'fails a passing scope as leaked when a connection outlives its store',
        Effect.fnUntraced(function* () {
            const outliving = yield* Effect.scope;
            const exit = yield* Effect.gen(function* () {
                return yield* Scope.provide(opened((yield* cache).client), outliving);
            }).pipe(Effect.scoped, Effect.exit);
            const refusal = Option.filter(Exit.match(exit, { onSuccess: () => Option.none(), onFailure: flow(Cause.findDefect, Result.getSuccess) }), Schema.is(StoreError));
            assert.deepStrictEqual(
                Option.map(refusal, ({ reason }) => reason),
                Option.some({ _tag: 'Leaked' as const }),
            );
        }),
        refusing,
    );
    methods.effect(
        'builds a digest once for clones asked at once, each holding the built rows in a database of its own',
        Effect.fnUntraced(function* () {
            const [digest, builds] = yield* Effect.all([SecureRandom.SecureRandom.use(({ uuidV7 }) => uuidV7()), Ref.make(0)]);
            const cloned = Store.use((store) => store.templates.clickhouse(digest, marked(builds)));
            const clones = yield* Effect.all(
                Array.makeBy(callers, () => cloned),
                { concurrency: 'unbounded' },
            );
            const rows = yield* Effect.forEach(clones, (clone) => asked(clone, 'select mark from marks'));
            assert.deepStrictEqual([yield* Ref.get(builds), rows, new Set(clones.map(({ database }) => database)).size], [1, Array.makeBy(callers, () => 'held'), callers]);
        }),
    );
});
