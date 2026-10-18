import { env } from 'cloudflare:workers';
import { BrowserCrypto } from '@effect/platform-browser';
import { D1Client } from '@effect/sql-d1';
import { assert, it } from '@effect/vitest';
import { store } from '@rasm/database/store.ts';
import { contract } from '@rasm/hosting/sites.ts';
import { r2 } from '@rasm/hosting/storage.ts';
import { Effect, Layer, Schema } from 'effect';
import { HttpRouter } from 'effect/http';
import { app } from './app.ts';
import { catalog } from './database.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const origin = 'https://portfolio.test';
const code = { ok: 200, empty: 204, partial: 206, unchanged: 304, badRequest: 400, forbidden: 403, missing: 404, conflict: 409, failed: 412 };
const edge = { host: 'portfolio.test', 'x-forwarded-proto': 'https', origin };
const owner = { [contract.headers.id]: 'owner', [contract.headers.email]: 'seedy@sites.test', ...edge };
const bytes = new TextEncoder().encode('portfolio drawing bytes');
const asset = { id: '3f1c2a9e-6b7d-4c1e-9a2b-5d8e7f6a1b2c', name: 'drawing.png', size: bytes.length, mime: 'image/png', width: 64, height: 48 };
const media = `/api/media/${asset.id}`;
const hero = { caption: '', assetId: asset.id, kind: 'image', role: 'drawing', framing: { mode: 'contain' } };
const draft = { name: 'Studio', introduction: '', email: '', entries: [] };
const json = Schema.encodeEffect(Schema.fromJsonString(Schema.Unknown));
const client = Effect.gen(function* () {
    const { handler } = yield* Effect.acquireRelease(
        Effect.sync(() =>
            HttpRouter.toWebHandler(
                app({ fetch: globalThis.fetch }, owner[contract.headers.email]).pipe(
                    Layer.provideMerge(
                        Layer.mergeAll(
                            store(catalog, (options) => D1Client.layer({ db: env.DB, ...options })),
                            r2(env.BUCKET),
                            BrowserCrypto.layer,
                        ),
                    ),
                ),
            ),
        ),
        ({ dispose }) => Effect.promise(dispose),
    );
    const send = Effect.fn('send')(function* (path: string, init: RequestInit) {
        const response = yield* Effect.promise(() => handler(new Request(`${origin}${path}`, init)));
        return { status: response.status, headers: response.headers, text: new TextDecoder().decode(yield* Effect.promise(() => response.arrayBuffer())) };
    });
    const upload = Effect.fn('upload')(function* (metadata: typeof asset, content: Uint8Array<ArrayBuffer>) {
        const form = new FormData();
        form.append('asset', yield* json(metadata));
        form.append('file', new File([content], metadata.name, { type: metadata.mime }));
        return yield* send('/api/media', { method: 'POST', headers: owner, body: form });
    });
    const write = Effect.fn('write')(function* (method: 'PUT' | 'POST', document: object) {
        const etag = yield* send('/api/portfolio', { headers: owner }).pipe(Effect.flatMap(({ headers }) => Effect.fromNullishOr(headers.get('draft-etag'))));
        return yield* send(method === 'PUT' ? '/api/portfolio' : '/api/publish', { method, headers: { ...owner, 'content-type': 'application/json', 'if-match': etag }, body: yield* json(document) });
    });
    return { send, upload, write };
});

it.effect('serves ranges and preconditions to the owner and keeps an asset private until a published document places it', () =>
    Effect.gen(function* () {
        const { send, upload, write } = yield* client;
        const uploaded = yield* upload(asset, bytes);
        const ranged = yield* send(media, { headers: { ...owner, range: 'bytes=0-8' } });
        const etag = yield* Effect.fromNullishOr(ranged.headers.get('etag'));
        const [unchanged, mismatched, hidden] = yield* Effect.all([send(media, { headers: { ...owner, 'if-none-match': etag } }), send(media, { headers: { ...owner, 'if-match': '"other"', range: 'bytes=0-8' } }), send(media, {})]);
        const published = yield* write('POST', { ...draft, hero });
        const visible = yield* send(media, { headers: { range: 'bytes=-5' } });
        assert.deepStrictEqual(
            [uploaded, ranged, unchanged, mismatched, hidden, published, visible].map(({ status, text }) => [status, text]),
            [
                [code.ok, yield* json({ id: asset.id })],
                [code.partial, 'portfolio'],
                [code.unchanged, ''],
                [code.failed, ''],
                [code.forbidden, ''],
                [code.empty, ''],
                [code.partial, 'bytes'],
            ],
        );
        assert.strictEqual(ranged.headers.get('content-range'), `bytes 0-8/${bytes.length}`);
    }).pipe(Effect.scoped),
);
it.effect('settles overlapping uploads of one asset into one object and refuses differing metadata', () =>
    Effect.gen(function* () {
        const { send, upload } = yield* client;
        const attempts = yield* Effect.all([upload(asset, bytes), upload(asset, bytes)], { concurrency: 'unbounded' });
        const differing = yield* upload({ ...asset, name: 'renamed.png' }, bytes);
        const [pending, full] = yield* Effect.all([send('/api/media/pending', { headers: owner }), send(media, { headers: owner })]);
        assert.deepStrictEqual(
            [...attempts, differing, pending, full].map(({ status }) => status),
            [code.ok, code.ok, code.conflict, code.ok, code.ok],
        );
        assert.deepStrictEqual([pending.text, full.text], ['[]', 'portfolio drawing bytes']);
    }).pipe(Effect.scoped),
);
it.effect('refuses deleting an asset a document places and deletes it once the draft drops it', () =>
    Effect.gen(function* () {
        const { send, upload, write } = yield* client;
        yield* upload(asset, bytes);
        const placed = yield* write('PUT', { ...draft, hero });
        const refused = yield* send(media, { method: 'DELETE', headers: owner });
        const dropped = yield* write('PUT', draft);
        const deleted = yield* send(media, { method: 'DELETE', headers: owner });
        const gone = yield* send(media, { headers: owner });
        assert.deepStrictEqual(
            [placed, refused, dropped, deleted, gone].map(({ status }) => status),
            [code.empty, code.conflict, code.empty, code.empty, code.missing],
        );
    }).pipe(Effect.scoped),
);
it.effect('refuses a body shorter than its declared size, releases its attempt, and refuses visitor writes', () =>
    Effect.gen(function* () {
        const { send, upload } = yield* client;
        const interrupted = yield* upload(asset, bytes.subarray(1));
        const [pending, missing, visitor] = yield* Effect.all([send('/api/media/pending', { headers: owner }), send(media, { headers: owner }), send(media, { method: 'DELETE', headers: edge })]);
        assert.deepStrictEqual(
            [interrupted, pending, missing, visitor].map(({ status, text }) => [status, text]),
            [
                [code.badRequest, ''],
                [code.ok, '[]'],
                [code.missing, ''],
                [code.forbidden, ''],
            ],
        );
    }).pipe(Effect.scoped),
);
