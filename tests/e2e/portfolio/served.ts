import { createServer } from 'node:net';
import { join } from 'node:path';
import process from 'node:process';
import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { layer } from '@effect/platform-node/NodeServices';
import { App } from '@rasm/hosting';
import { Access, Enrollment } from '@rasm/hosting/identity';
import { Presigned } from '@rasm/object-storage';
import { routed } from '@rasm/testing/playwright';
import { Cause, Effect, FileSystem, Function, Layer, Schedule, Schema } from 'effect';
import { HttpBody, HttpClient, HttpClientRequest, HttpClientResponse } from 'effect/http';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
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
const STORAGE = { bucket: 'portfolio', accessKeyId: 'portfolio', secretAccessKey: 'portfolio' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Upload = Schema.Struct({ attempt: Schema.String, objects: Schema.Array(Schema.Struct({ key: Schema.String })) }).annotate({ description: 'The attempt and object keys POST /api/assets answers for a file the library lacks' });
const Allowed = Schema.fromJsonString(Schema.Array(Schema.String)).annotate({ description: 'The JSON list of owner addresses the ALLOWED variable holds' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const allocated = Effect.callback<number>((resume) => {
    const probe = createServer().listen(0, '127.0.0.1', () => {
        const address = probe.address();
        probe.close(() => resume(Effect.succeed(typeof address === 'object' && address !== null ? address.port : 0)));
    });
});

const published = Effect.fnUntraced(function* (origin: string, { files, portfolio }: Published) {
    const client = HttpClient.filterStatusOk(yield* HttpClient.HttpClient);
    const owner = HttpClient.mapRequest(client, Function.flow(HttpClientRequest.prependUrl(origin), HttpClientRequest.setHeaders({ ...OWNER, ...SAME_ORIGIN })));
    yield* Effect.forEach(
        files,
        Effect.fnUntraced(function* ({ asset, bytes }) {
            const { attempt, objects } = yield* Effect.flatMap(owner.post('/api/assets', { body: HttpBody.jsonUnsafe(asset) }), HttpClientResponse.schemaBodyJson(Upload));
            yield* Effect.forEach(
                objects,
                Effect.fnUntraced(function* ({ key }) {
                    const { url, headers } = yield* Effect.flatMap(owner.post('/api/assets/signatures', { body: HttpBody.jsonUnsafe({ method: 'PUT', key }) }), HttpClientResponse.schemaBodyJson(Presigned));
                    yield* client.put(url, { headers, body: HttpBody.uint8Array(bytes, asset.mime) });
                }),
                { discard: true },
            );
            yield* owner.post(`/api/assets/${asset.id}/uploads/${attempt}`);
        }),
        { discard: true },
    );
    yield* owner.put('/api/published', { headers: { 'if-match': '"0"' }, body: HttpBody.jsonUnsafe(portfolio) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const served = Effect.gen(function* () {
    const product = '@rasm/portfolio';
    const [fs, spawner, client, bundle] = yield* Effect.all([FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner, HttpClient.HttpClient, Effect.fromOption(routed('@rasm/nx-vite', product), () => new Cause.UnknownError(product, `nx.json states no Vite outputPath to run ${product} from`))]);
    const state = yield* fs.makeTempDirectoryScoped({ prefix: 'portfolio-' });
    const storage = yield* Effect.acquireRelease(
        Effect.sync(
            () =>
                new Miniflare({
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
                                env: { BUCKET: { type: 'r2', name: STORAGE.bucket, dev: { experimentalS3Credentials: { accessKeyId: STORAGE.accessKeyId, secretAccessKey: STORAGE.secretAccessKey } } } },
                            },
                            dev: {},
                        },
                    ],
                }),
        ),
        (held) => Effect.promise(() => held.dispose()),
    );
    const [stored, port, access, allowed] = yield* Effect.all([Effect.promise(() => storage.ready), allocated, Schema.encodeEffect(Access.json)(Access.make({ source: { _tag: 'Forwarded', ...FORWARDED }, logout: '/' })), Schema.encodeEffect(Allowed)([OWNER[FORWARDED.email]])]);
    const origin = `http://127.0.0.1:${port}`;
    const server = yield* spawner.spawn(
        ChildProcess.make(process.execPath, [join(bundle, App.bundle.environment, `${App.bundle.entry}.js`)], {
            env: {
                [App.bundle.portVariable]: `${port}`,
                DB: join(state, 'portfolio.sqlite'),
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
        }),
    );
    yield* Effect.raceFirst(
        Effect.retry(Effect.timeout(HttpClient.filterStatusOk(client).get(new URL(App.bundle.readiness, origin)), '1 second'), { schedule: Schedule.spaced('100 millis') }),
        Effect.flatMap(server.exitCode, (code) => Effect.fail(new Cause.UnknownError(code, `${product} exits ${code} before it answers ${App.bundle.readiness}`))),
    );
    return { origin };
});

const platform = Layer.merge(layerUndici, layer);

// --- [EXPORTS] -------------------------------------------------------------------------

export { GUEST, OWNER, type Published, platform, published, SAME_ORIGIN, served };
