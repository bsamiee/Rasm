import { CreateBucketCommand, S3Client, type S3ClientConfig } from '@aws-sdk/client-s3';
import { layer } from '@effect/vitest';
import { ObjectStorage } from '@rasm/hosting/server';
import { Context, Effect, Layer } from 'effect';
import { GenericContainer, PullPolicy, Wait } from 'testcontainers';
import { cases } from '#/tests/typescript/libs/web/hosting/cases';

// --- [SERVICES] ------------------------------------------------------------------------

class Bucket extends Context.Service<Bucket, { readonly name: string; readonly client: S3ClientConfig }>()('rasm/tests/typescript/libs/web/hosting/s3.test/Bucket') {
    static create({ name, client }: Context.Service.Shape<typeof Bucket>) {
        return Effect.acquireUseRelease(
            Effect.sync(() => new S3Client(client)),
            (opened) => Effect.promise((signal) => opened.send(new CreateBucketCommand({ Bucket: name }), { abortSignal: signal })),
            (opened) => Effect.sync(() => opened.destroy()),
        );
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(
    Layer.effect(
        Bucket,
        Effect.gen(function* () {
            const rustfs = { port: 9000, credentials: { accessKeyId: 'rasm-hosting-test', secretAccessKey: 'rasm-hosting-test-secret' } };
            const started = yield* Effect.acquireDisposable(
                Effect.promise(() =>
                    new GenericContainer('rustfs/rustfs:latest')
                        .withPullPolicy(PullPolicy.alwaysPull())
                        .withEnvironment({ RUSTFS_ACCESS_KEY: rustfs.credentials.accessKeyId, RUSTFS_SECRET_KEY: rustfs.credentials.secretAccessKey, RUSTFS_CONSOLE_ENABLE: 'false' })
                        .withExposedPorts(rustfs.port)
                        .withWaitStrategy(Wait.forHttp('/health/ready', rustfs.port))
                        .start(),
                ),
            );
            const bucket = { name: 'media', client: { endpoint: `http://${started.getHost()}:${started.getMappedPort(rustfs.port)}`, region: 'us-east-1', credentials: rustfs.credentials, forcePathStyle: true } };
            yield* Bucket.create(bucket);
            return bucket;
        }),
    ),
    { timeout: '5 minutes' },
)('s3', (methods) => cases(methods, Layer.unwrap(Bucket.useSync(({ name, client }) => ObjectStorage.s3(name, client)))));
