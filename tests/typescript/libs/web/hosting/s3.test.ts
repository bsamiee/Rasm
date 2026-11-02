import { CreateBucketCommand, S3Client, type S3ClientConfig } from '@aws-sdk/client-s3';
import { layer } from '@effect/vitest';
import { ObjectStorage } from '@rasm/hosting/storage';
import { Context, Effect, Layer } from 'effect';
import { GenericContainer, PullPolicy, Wait } from 'testcontainers';
import { cases } from '#/tests/typescript/libs/web/hosting/cases';

// --- [SERVICES] ------------------------------------------------------------------------

class Bucket extends Context.Service<Bucket, { readonly name: string; readonly client: S3ClientConfig }>()('rasm/tests/typescript/libs/web/hosting/s3.test/Bucket') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const rustfs = { port: 9000, credentials: { accessKeyId: 'rasm-hosting-test', secretAccessKey: 'rasm-hosting-test-secret' } };

layer(
    Layer.effect(
        Bucket,
        Effect.gen(function* () {
            const started = yield* Effect.acquireRelease(
                Effect.promise(() =>
                    new GenericContainer('rustfs/rustfs:latest')
                        .withPullPolicy(PullPolicy.alwaysPull())
                        .withEnvironment({ RUSTFS_ACCESS_KEY: rustfs.credentials.accessKeyId, RUSTFS_SECRET_KEY: rustfs.credentials.secretAccessKey, RUSTFS_CONSOLE_ENABLE: 'false' })
                        .withExposedPorts(rustfs.port)
                        .withWaitStrategy(Wait.forHttp('/health/ready', rustfs.port))
                        .start(),
                ),
                (container) => Effect.promise(() => container.stop()),
            );
            const bucket = { name: 'media', client: { endpoint: `http://${started.getHost()}:${started.getMappedPort(rustfs.port)}`, region: 'us-east-1', credentials: rustfs.credentials, forcePathStyle: true } };
            yield* Effect.acquireUseRelease(
                Effect.sync(() => new S3Client(bucket.client)),
                (client) => Effect.promise((signal) => client.send(new CreateBucketCommand({ Bucket: bucket.name }), { abortSignal: signal })),
                (client) => Effect.sync(() => client.destroy()),
            );
            return bucket;
        }),
    ),
    { timeout: '5 minutes' },
)('s3', (methods) => cases(methods, Layer.unwrap(Bucket.useSync(({ name, client }) => ObjectStorage.s3(name, client)))));
