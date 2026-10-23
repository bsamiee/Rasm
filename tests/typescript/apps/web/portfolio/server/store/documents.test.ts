import { reset } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { D1Client } from '@effect/sql-d1';
import { assert, beforeEach, it } from '@effect/vitest';
import { store } from '@rasm/database';
import { catalog, readDocument, writeDocument } from '@rasm/portfolio/server';
import { Effect } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

beforeEach(reset);
it.effect('saves and publishes draft in one atomic batch, keeps revision on unchanged save, rejects stale revision', () =>
    Effect.gen(function* () {
        const draft = { name: 'Studio', introduction: '', email: 'studio@example.com', entries: [] };
        const empty = yield* readDocument('draft');
        const saved = yield* writeDocument(draft, true, empty.revision);
        const published = yield* readDocument('published');
        assert.deepStrictEqual([empty.revision, saved, published.data.portfolio.name, published.revision], [0, 1, 'Studio', saved]);
        assert.deepStrictEqual(yield* writeDocument(draft, false, saved), saved);
        assert.deepStrictEqual(yield* writeDocument({ ...draft, name: 'Atelier' }, false, saved), saved + 1);
        assert.deepStrictEqual((yield* Effect.flip(writeDocument(draft, false, empty.revision)))._tag, 'PreconditionFailed');
    }).pipe(Effect.provide(store(catalog, (options) => D1Client.layer({ db: env.DB, ...options })))),
);
