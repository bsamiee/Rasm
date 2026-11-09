import { CreateBucketCommand, DeleteBucketCommand, DeleteObjectsCommand, ListObjectsV2Command, S3Client, type S3ClientConfig } from '@aws-sdk/client-s3';
import { layer } from '@effect/platform-node/NodeServices';
import { it } from '@effect/vitest';
import { s3Storage } from '@rasm/hosting/s3';
import { service } from '@rasm/test-support/compose';
import { Context, Effect, Layer, Record } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { cases } from '#/cases';

// --- [SERVICES] ------------------------------------------------------------------------

class Bucket extends Context.Service<Bucket, { readonly name: string; readonly client: S3ClientConfig }>()('@rasm/test-hosting/s3.test/Bucket') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const removed = Effect.fnUntraced(function* (s3: S3Client, name: string) {
    const { Contents = [] } = yield* Effect.tryPromise((signal) => s3.send(new ListObjectsV2Command({ Bucket: name }), { abortSignal: signal }));
    const keys = Contents.map(({ Key }) => ({ Key }));
    yield* Effect.when(
        Effect.tryPromise((signal) => s3.send(new DeleteObjectsCommand({ Bucket: name, Delete: { Objects: keys } }), { abortSignal: signal })),
        Effect.succeed(keys.length > 0),
    );
    yield* Effect.tryPromise((signal) => s3.send(new DeleteBucketCommand({ Bucket: name }), { abortSignal: signal }));
}, Effect.orDie);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(
    Layer.effect(
        Bucket,
        Effect.gen(function* () {
            const storage = yield* service('rasm', 'storage');
            const variable = (key: string) => Effect.fromOption(Record.get(storage.environment, key));
            const [accessKeyId, secretAccessKey, region] = yield* Effect.all([variable('RUSTFS_ACCESS_KEY'), variable('RUSTFS_SECRET_KEY'), variable('RUSTFS_REGION')]);
            const client = { endpoint: `http://${storage.address}`, region, credentials: { accessKeyId, secretAccessKey }, forcePathStyle: true };
            const name = `media-${yield* SecureRandom.SecureRandom.use(({ uuidV7 }) => uuidV7())}`;
            const s3 = yield* Effect.acquireRelease(
                Effect.sync(() => new S3Client(client)),
                (opened) => Effect.sync(() => opened.destroy()),
            );
            yield* Effect.acquireRelease(
                Effect.tryPromise((signal) => s3.send(new CreateBucketCommand({ Bucket: name }), { abortSignal: signal })),
                () => removed(s3, name),
            );
            return { name, client };
        }),
    ).pipe(Layer.provide(Layer.merge(layer, SecureRandom.layerWebCrypto))),
    { timeout: '5 minutes' },
)('s3', (methods) => cases(methods, Layer.unwrap(Bucket.useSync(({ name, client }) => s3Storage(name, client)))));
