import { it } from '@effect/vitest';
import { s3Storage } from '@rasm/object-storage/s3';
import { Store } from '@rasm/test-support/store';
import { failed } from '@rasm/test-support/vitest';
import { Effect, Layer } from 'effect';
import { cases } from '#/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Store.layer(undefined, failed), { timeout: '5 minutes' })('s3', (methods) =>
    cases(methods, Layer.unwrap(Effect.map(Store.use(({ bucket }) => bucket), ({ bucket, client }) => s3Storage({ bucket }, client)))),
);
