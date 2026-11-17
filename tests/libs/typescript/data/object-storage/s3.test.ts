import { CreateBucketCommand, S3Client } from '@aws-sdk/client-s3';
import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { layer } from '@effect/vitest';
import { s3Storage } from '@rasm/object-storage/s3';
import { ConfigProvider, Effect, Layer } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { GenericContainer, Wait } from 'testcontainers';
import { completed, conforms, reads, writes } from '#/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const served = { port: 9000, health: '/health/ready', region: 'us-east-1', bucket: 'objects' } as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const storage = Effect.gen(function* () {
    const [user, key] = yield* SecureRandom.SecureRandom.use((random) => Effect.all([random.uuidV7(), random.uuidV7()]));
    const container = yield* Effect.acquireDisposable(
        Effect.tryPromise(() => new GenericContainer('rustfs/rustfs:1.0.6').withEnvironment({ RUSTFS_ACCESS_KEY: user, RUSTFS_SECRET_KEY: key, RUSTFS_CONSOLE_ENABLE: 'false', RUSTFS_OBS_LOG_DIRECTORY: '' }).withExposedPorts(served.port).withWaitStrategy(Wait.forHttp(served.health, served.port)).start()),
    );
    const access = { AWS_ENDPOINT_URL_S3: `http://${container.getHost()}:${container.getMappedPort(served.port)}`, AWS_REGION: served.region, AWS_ACCESS_KEY_ID: user, AWS_SECRET_ACCESS_KEY: key };
    yield* Effect.tryPromise(() => new S3Client({ endpoint: access.AWS_ENDPOINT_URL_S3, region: served.region, credentials: { accessKeyId: user, secretAccessKey: key }, forcePathStyle: true }).send(new CreateBucketCommand({ Bucket: served.bucket })));
    return s3Storage(served.bucket, { forcePathStyle: true }).pipe(Layer.provide(ConfigProvider.layer(ConfigProvider.fromUnknown(access))));
}).pipe(Effect.provide(SecureRandom.layerWebCrypto), Layer.unwrap, Layer.provideMerge(layerUndici));

layer(storage, { timeout: '3 minutes', excludeTestServices: true })('object storage over the S3 API', (it) => {
    reads(it, completed);
    conforms(it, completed);
    writes(it);
});
