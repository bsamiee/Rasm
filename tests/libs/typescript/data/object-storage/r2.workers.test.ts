import { layer } from '@effect/vitest';
import { ObjectStorageError } from '@rasm/object-storage';
import { r2Storage } from '@rasm/object-storage/r2';
import { env } from 'cloudflare:workers';
import { ConfigProvider, Effect, Layer } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { reads } from '#/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

const unsigned = { AWS_ENDPOINT_URL_S3: 'http://127.0.0.1', AWS_REGION: 'auto', AWS_ACCESS_KEY_ID: 'unsigned', AWS_SECRET_ACCESS_KEY: 'unsigned' };

layer(r2Storage({ bucket: env.BUCKET, name: 'objects' }).pipe(Layer.provide(ConfigProvider.layer(ConfigProvider.fromUnknown(unsigned))), Layer.merge(FetchHttpClient.layer)), { excludeTestServices: true })('object storage reads over an R2 binding', (it) => {
    reads(it, (key, bytes) => Effect.tryPromise({ try: () => env.BUCKET.put(key, bytes, { httpMetadata: { contentType: 'video/mp4' } }), catch: ObjectStorageError.of('Unavailable', key) }));
});
