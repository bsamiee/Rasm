import { it } from '@effect/vitest';
import { s3Storage } from '@rasm/object-storage/s3';
import { Store } from '@rasm/test-store';
import { failed } from '@rasm/testing/vitest';
import { Context, type Effect, Layer } from 'effect';
import { cases } from '#/cases';

// --- [SERVICES] ------------------------------------------------------------------------

const held = Store.use(({ bucket }) => bucket);

class Bucket extends Context.Service<Bucket, Effect.Success<typeof held>>()('@rasm/test-object-storage/s3.test/Bucket') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Layer.effect(Bucket, held).pipe(Layer.provide(Store.layer(undefined, failed))), { buildPerTest: true, timeout: '5 minutes' })('s3', (methods) => cases(methods, Layer.unwrap(Bucket.useSync(({ bucket, client }) => s3Storage({ bucket }, client)))));
