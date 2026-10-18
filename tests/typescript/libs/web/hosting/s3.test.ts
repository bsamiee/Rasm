import { CreateBucketCommand, S3Client } from '@aws-sdk/client-s3';
import { layer } from '@effect/vitest';
import { s3 } from '@rasm/hosting/storage.ts';
import { Context, Effect, Layer, Random } from 'effect';
import { GenericContainer, PullPolicy, type StartedTestContainer, Wait } from 'testcontainers';
import { vi } from 'vitest';
import { cases } from './cases.ts';

// --- [SERVICES] ------------------------------------------------------------------------

class RustFS extends Context.Service<RustFS, StartedTestContainer>()('rasm/tests/typescript/libs/web/hosting/s3.test/RustFS') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const rustfs = { port: 9000, accessKeyId: 'rasm-hosting-test', secretAccessKey: 'rasm-hosting-test-secret' };
const storage = Layer.unwrap(
    Effect.gen(function* () {
        const started = yield* RustFS;
        const endpoint = `http://${started.getHost()}:${started.getMappedPort(rustfs.port)}`;
        const bucket = `storage-${yield* Random.nextIntBetween(0, Number.MAX_SAFE_INTEGER)}`;
        Object.entries({ AWS_ENDPOINT_URL_S3: endpoint, AWS_REGION: 'us-east-1', AWS_ACCESS_KEY_ID: rustfs.accessKeyId, AWS_SECRET_ACCESS_KEY: rustfs.secretAccessKey }).forEach(([name, value]) => {
            vi.stubEnv(name, value);
        });
        yield* Effect.acquireUseRelease(
            Effect.sync(() => new S3Client({ forcePathStyle: true })),
            (client) => Effect.tryPromise(() => client.send(new CreateBucketCommand({ Bucket: bucket }))),
            (client) => Effect.sync(() => client.destroy()),
        );
        return s3(bucket);
    }),
);

layer(
    Layer.effect(
        RustFS,
        Effect.acquireRelease(
            Effect.tryPromise(() =>
                new GenericContainer('rustfs/rustfs:latest')
                    .withPullPolicy(PullPolicy.alwaysPull())
                    .withEnvironment({ RUSTFS_ACCESS_KEY: rustfs.accessKeyId, RUSTFS_SECRET_KEY: rustfs.secretAccessKey, RUSTFS_CONSOLE_ENABLE: 'false' })
                    .withExposedPorts(rustfs.port)
                    .withWaitStrategy(Wait.forHttp('/health/ready', rustfs.port))
                    .start(),
            ),
            (started) => Effect.promise(() => started.stop()),
        ),
    ),
    { timeout: '5 minutes' },
)('s3', (methods) => cases(methods, storage));
