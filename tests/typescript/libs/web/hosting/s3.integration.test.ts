import { CreateBucketCommand, S3Client, type S3ClientConfig } from '@aws-sdk/client-s3';
import { layer } from '@effect/vitest';
import { s3Storage } from '@rasm/hosting/node';
import { Context, Effect, Layer } from 'effect';
import { GenericContainer, PullPolicy, Wait } from 'testcontainers';
import { cases } from '#/cases';

// --- [SERVICES] ------------------------------------------------------------------------

class Bucket extends Context.Service<Bucket, { readonly name: string; readonly client: S3ClientConfig }>()('@rasm/test-hosting/s3.integration.test/Bucket') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(
    Layer.effect(
        Bucket,
        Effect.gen(function* () {
            const rustfs = { port: 9000, credentials: { accessKeyId: 'rasm-hosting-test', secretAccessKey: 'rasm-hosting-test-secret' } };
            const started = yield* Effect.acquireDisposable(
                Effect.tryPromise(() =>
                    new GenericContainer('rustfs/rustfs:latest')
                        .withPullPolicy(PullPolicy.alwaysPull())
                        .withEnvironment({ RUSTFS_ACCESS_KEY: rustfs.credentials.accessKeyId, RUSTFS_SECRET_KEY: rustfs.credentials.secretAccessKey, RUSTFS_CONSOLE_ENABLE: 'false' })
                        .withExposedPorts(rustfs.port)
                        .withWaitStrategy(Wait.forHttp('/health/ready', rustfs.port, { abortOnContainerExit: true }))
                        .start(),
                ),
            );
            const bucket = { name: 'media', client: { endpoint: `http://${started.getHost()}:${started.getMappedPort(rustfs.port)}`, region: 'us-east-1', credentials: rustfs.credentials, forcePathStyle: true } };
            yield* Effect.acquireUseRelease(
                Effect.sync(() => new S3Client(bucket.client)),
                (opened) => Effect.tryPromise((signal) => opened.send(new CreateBucketCommand({ Bucket: bucket.name }), { abortSignal: signal })),
                (opened) => Effect.sync(() => opened.destroy()),
            );
            return bucket;
        }),
    ),
    { timeout: '5 minutes' },
)('s3', (methods) => cases(methods, Layer.unwrap(Bucket.useSync(({ name, client }) => s3Storage(name, client)))));
