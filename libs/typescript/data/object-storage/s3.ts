import { S3Client, type S3ClientConfig } from '@aws-sdk/client-s3';
import { Config, Effect, Layer, Option, Redacted } from 'effect';
import { s3Store } from 'partial-content/s3';
import { ObjectStorage } from '#/storage';

// --- [CONSTANTS] -----------------------------------------------------------------------

const access = Config.all({
    endpoint: Config.String('AWS_ENDPOINT_URL_S3'),
    published: Config.option(Config.String('S3_PUBLIC_ENDPOINT_URL')),
    region: Config.String('AWS_REGION'),
    accessKeyId: Config.Redacted('AWS_ACCESS_KEY_ID'),
    secretAccessKey: Config.Redacted('AWS_SECRET_ACCESS_KEY'),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const s3Clients = Effect.fnUntraced(function* (config: Omit<S3ClientConfig, 'endpoint' | 'region' | 'credentials' | 'requestChecksumCalculation'>) {
    const { endpoint, published, region, accessKeyId, secretAccessKey } = yield* access;
    const opened = (at: string) =>
        Effect.acquireRelease(
            Effect.sync(() => new S3Client({ ...config, requestChecksumCalculation: 'WHEN_REQUIRED', endpoint: at, region, credentials: { accessKeyId: Redacted.value(accessKeyId), secretAccessKey: Redacted.value(secretAccessKey) } })),
            (client) => Effect.sync(() => client.destroy()),
        );
    const client = yield* opened(endpoint);
    return { client, signer: yield* Option.match(published, { onNone: () => Effect.succeed(client), onSome: opened }), origin: new URL(Option.getOrElse(published, () => endpoint)).origin };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const s3Storage = (bucket: string, config: Omit<S3ClientConfig, 'endpoint' | 'region' | 'credentials' | 'requestChecksumCalculation'> = {}) =>
    Layer.effect(
        ObjectStorage,
        Effect.map(s3Clients(config), (clients) => ObjectStorage.make(clients, bucket, s3Store({ client: clients.client, bucket, reservedPrefix: '' }))),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { s3Clients, s3Storage };
