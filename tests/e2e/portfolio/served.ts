import { join } from 'node:path';
import process from 'node:process';
import { DatabaseSync } from 'node:sqlite';
import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { layer } from '@effect/platform-node/NodeServices';
import { App } from '@rasm/hosting/app';
import { Access, Enrollment } from '@rasm/hosting/identity';
import { Presigned } from '@rasm/object-storage/storage';
import { routed } from '@rasm/testing/playwright';
import { Cause, Effect, FileSystem, Function, Layer, Schedule, Schema } from 'effect';
import { HttpBody, HttpClient, HttpClientRequest, HttpClientResponse } from 'effect/http';
import { ChildProcess } from 'effect/process';
import getPort from 'get-port';
import { CorePaths, Miniflare } from 'miniflare';

// --- [TYPES] ---------------------------------------------------------------------------

interface Published {
    readonly files: readonly { readonly asset: { readonly id: string; readonly mime: string; readonly [field: string]: unknown }; readonly bytes: Uint8Array }[];
    readonly portfolio: object;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FORWARDED = { id: 'x-forwarded-user', email: 'x-forwarded-email' } as const;
const OWNER = { [FORWARDED.id]: 'owner', [FORWARDED.email]: 'owner@portfolio.test' } as const;
const GUEST = { [FORWARDED.id]: 'guest', [FORWARDED.email]: 'guest@portfolio.test' } as const;
const SAME_ORIGIN = { 'sec-fetch-site': 'same-origin' } as const;
const STORAGE = { binding: 'BUCKET', bucket: 'portfolio', accessKeyId: 'portfolio', secretAccessKey: 'portfolio' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Upload = Schema.Struct({ attempt: Schema.String, objects: Schema.Array(Schema.Struct({ key: Schema.String })) }).annotate({ description: 'The attempt and object keys POST /api/assets answers for a file the library lacks' });
const Allowed = Schema.String.pipe(Schema.Array, Schema.fromJsonString).annotate({ description: 'The JSON list of owner addresses the ALLOWED variable holds' });
const Cleared = Schema.Struct({ script: Schema.String }).annotate({ description: 'The one statement list SQLite builds to delete every row of every table the product created' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const published = Effect.fnUntraced(function* (origin: string, { files, portfolio }: Published) {
    const client = HttpClient.filterStatusOk(yield* HttpClient.HttpClient);
    const owner = HttpClient.mapRequest(client, Function.flow(HttpClientRequest.prependUrl(origin), HttpClientRequest.setHeaders({ ...OWNER, ...SAME_ORIGIN })));
    yield* Effect.forEach(
        files,
        Effect.fnUntraced(function* ({ asset, bytes }) {
            const { attempt, objects } = yield* Effect.flatMap(owner.post('/api/assets', { body: HttpBody.jsonUnsafe(asset) }), HttpClientResponse.schemaBodyJson(Upload));
            const signed = yield* Effect.forEach(objects, ({ key }) => Effect.flatMap(owner.post('/api/assets/signatures', { body: HttpBody.jsonUnsafe({ method: 'PUT', key }) }), HttpClientResponse.schemaBodyJson(Presigned)));
            yield* Effect.forEach(signed, ({ url, headers }) => client.put(url, { headers, body: HttpBody.uint8Array(bytes, asset.mime) }), { discard: true });
            yield* owner.post(`/api/assets/${asset.id}/uploads/${attempt}`);
        }),
        { discard: true },
    );
    yield* owner.put('/api/published', { headers: { 'if-match': '"0"' }, body: HttpBody.jsonUnsafe(portfolio) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const served = Effect.gen(function* () {
    const product = '@rasm/portfolio';
    const [fs, client, bundle] = yield* Effect.all([FileSystem.FileSystem, HttpClient.HttpClient, Effect.fromOption(routed('@rasm/nx-vite', product), () => new Cause.UnknownError(product, `nx.json states no Vite outputPath to run ${product} from`))]);
    const state = yield* fs.makeTempDirectoryScoped({ prefix: 'portfolio-' });
    const emulated: ConstructorParameters<typeof Miniflare>[0] = {
        host: '127.0.0.1',
        port: 0,
        resourcePersistencePath: join(state, 'storage'),
        enableDevRegistry: false,
        telemetry: { enabled: false },
        workers: [
            {
                config: {
                    name: STORAGE.bucket,
                    compatibilityDate: '2026-10-01',
                    manifest: { mainModule: 'index.js', modules: { 'index.js': { type: 'esm', contents: 'export default { fetch: () => new Response(null, { status: 404 }) }' } } },
                    env: { [STORAGE.binding]: { type: 'r2', name: STORAGE.bucket, dev: { experimentalS3Credentials: { accessKeyId: STORAGE.accessKeyId, secretAccessKey: STORAGE.secretAccessKey } } } },
                },
                dev: {},
            },
        ],
    };
    const storage = yield* Effect.acquireRelease(
        Effect.sync(() => new Miniflare(emulated)),
        (held) => Effect.promise(() => held.dispose()),
    );
    const [stored, port, access, allowed] = yield* Effect.all([Effect.promise(() => storage.ready), Effect.promise(() => getPort()), Schema.encodeEffect(Access.json)(Access.make({ source: { _tag: 'Forwarded', ...FORWARDED }, logout: '/' })), Schema.encodeEffect(Allowed)([OWNER[FORWARDED.email]])]);
    const origin = `http://127.0.0.1:${port}`;
    const database = join(state, 'portfolio.sqlite');
    const server = yield* ChildProcess.make(process.execPath, [join(bundle, App.bundle.environment, `${App.bundle.entry}.js`)], {
        env: {
            [App.bundle.portVariable]: `${port}`,
            DB: database,
            BUCKET: STORAGE.bucket,
            [App.bundle.signing.endpoint]: new URL(CorePaths.R2_S3, stored).href,
            [App.bundle.signing.region]: 'auto',
            [App.bundle.signing.key]: STORAGE.accessKeyId,
            [App.bundle.signing.secret]: STORAGE.secretAccessKey,
            [Access.variable]: access,
            [Enrollment.variable]: allowed,
        },
        extendEnv: true,
        stdout: 'ignore',
        stderr: 'ignore',
    });
    yield* Effect.raceFirst(
        Effect.retry(Effect.timeout(HttpClient.filterStatusOk(client).get(new URL(App.bundle.readiness, origin)), '1 second'), { schedule: Schedule.spaced('100 millis') }),
        Effect.flatMap(server.exitCode, (code) => new Cause.UnknownError(code, `${product} exits ${code} before it answers ${App.bundle.readiness}`)),
    );
    const reset = Effect.gen(function* () {
        const held = yield* Effect.acquireDisposable(Effect.sync(() => new DatabaseSync(database, { timeout: 5000 })));
        const { script } = yield* Schema.decodeUnknownEffect(Cleared)(held.prepare(`SELECT group_concat(format('DELETE FROM "%w"', name), ';') AS script FROM sqlite_schema WHERE type = 'table' AND name NOT GLOB 'sqlite_*'`).get());
        held.exec(`BEGIN;${script};COMMIT`);
        const bucket = yield* Effect.promise(() => storage.getR2Bucket(STORAGE.binding));
        const { objects } = yield* Effect.promise(() => bucket.list());
        const keys = objects.map(({ key }) => key);
        yield* Effect.promise(() => bucket.delete(keys));
    }).pipe(Effect.scoped);
    return { origin, reset };
});

const platform = Layer.merge(layerUndici, layer);

// --- [EXPORTS] -------------------------------------------------------------------------

export { GUEST, OWNER, type Published, platform, published, SAME_ORIGIN, served };
