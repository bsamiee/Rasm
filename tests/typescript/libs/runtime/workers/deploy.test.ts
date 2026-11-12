import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { all, runtime } from '@pulumi/pulumi';
import { deployWorker } from '@rasm/workers/deploy';
import { Array, Effect, FileSystem, MutableRef, Path, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LEDGER = '4e8a1c2d-6b7f-4d3a-9c5e-2f1b0a9d8c7e';
const FILES = {
    'config.json': { buildContext: { isPreview: false, mode: 'production' } },
    'workers/default/worker.config.json': {
        name: 'sheets',
        compatibilityDate: '2026-10-01',
        env: {
            ASSETS: { type: 'assets' },
            ARCHIVE: { type: 'r2', name: 'archive', jurisdiction: 'eu' },
            RECORDS: { type: 'd1', name: 'records', jurisdiction: 'eu' },
            LEDGER: { type: 'd1', name: 'ledger', id: LEDGER },
            JOBS: { type: 'queue', name: 'jobs', deliveryDelay: 60 },
            NOTICES: { type: 'queue', name: 'notices' },
        },
    },
    'workers/default/assets/sheets.json': { sheets: [] },
};
const TYPES = { database: 'cloudflare:index/d1Database:D1Database', bucket: 'cloudflare:index/r2Bucket:R2Bucket', queue: 'cloudflare:index/queue:Queue', version: 'cloudflare:index/workerVersion:WorkerVersion' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);

// --- [SERVICES] ------------------------------------------------------------------------

const registered = MutableRef.make<ReadonlyArray<{ readonly type: string; readonly name: string; readonly inputs: Record.ReadonlyRecord<string, unknown> & { readonly bindings?: unknown } }>>([]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const built = Effect.fnUntraced(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const root = yield* fs.makeTempDirectoryScoped();
    const output = path.join(root, '.cloudflare/output/v0');
    yield* Effect.forEach(
        Record.toEntries(FILES),
        Effect.fnUntraced(function* ([file, content]) {
            yield* fs.makeDirectory(path.join(output, path.dirname(file)), { recursive: true });
            yield* fs.writeFileString(path.join(output, file), yield* Schema.encodeUnknownEffect(Json)(content));
        }),
        { discard: true },
    );
    return root;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.effect(
    'creates each store a binding names with the jurisdiction or delivery delay the binding states, binds an R2 bucket in its jurisdiction, and binds a D1 database stated by id without creating it',
    Effect.fnUntraced(function* () {
        yield* Effect.promise(() =>
            Promise.resolve(
                runtime.setMocks({
                    newResource: ({ type, name, inputs }) => {
                        MutableRef.update(registered, Array.append({ type, name, inputs }));
                        return { id: `${name}-id`, state: inputs };
                    },
                    call: ({ inputs }) => inputs,
                }),
            ),
        );
        const { stores, workers } = yield* deployWorker({ accountId: 'account', root: yield* built(), production: false, named: (configured) => `${configured}-next`, bindings: () => ({}), options: {} });
        const { default: worker } = workers;
        const { version } = yield* Effect.fromNullishOr(worker);
        const urns = [...Object.values(stores.d1), ...Object.values(stores.r2), ...Object.values(stores.queue)].map(({ urn }) => urn);
        yield* Effect.callback<void>((resume) => {
            all([version, ...urns]).apply(() => resume(Effect.void));
        });
        const made = Record.map(
            Array.groupBy(MutableRef.get(registered), ({ type }) => type),
            Record.fromIterableWith(({ name, inputs }) => [name, inputs]),
        );
        assert.deepStrictEqual(
            { databases: made[TYPES.database], buckets: made[TYPES.bucket], queues: made[TYPES.queue], bindings: made[TYPES.version]?.['sheets-next']?.bindings },
            {
                databases: { 'records-next': { accountId: 'account', name: 'records-next', jurisdiction: 'eu' } },
                buckets: { 'archive-next': { accountId: 'account', name: 'archive-next', jurisdiction: 'eu' } },
                queues: { 'jobs-next': { accountId: 'account', queueName: 'jobs-next', settings: { deliveryDelay: 60 } }, 'notices-next': { accountId: 'account', queueName: 'notices-next' } },
                bindings: [
                    { name: 'ASSETS', type: 'assets' },
                    { name: 'ARCHIVE', type: 'r2_bucket', bucketName: 'archive-next', jurisdiction: 'eu' },
                    { name: 'RECORDS', type: 'd1', id: 'records-next-id' },
                    { name: 'LEDGER', type: 'd1', id: LEDGER },
                    { name: 'JOBS', type: 'queue', queueName: 'jobs-next' },
                    { name: 'NOTICES', type: 'queue', queueName: 'notices-next' },
                ],
            },
            'cloudflare/workers-sdk@88c774b packages/wrangler/src/__tests__/create-worker-upload-form/bindings.test.ts "should include R2 bindings with a bucket_name", "should include D1 bindings with a database_id", packages/wrangler/src/__tests__/deploy/queues.test.ts "should update queue producers on deploy", @pulumi/cloudflare R2BucketArgs.jurisdiction, D1DatabaseArgs.jurisdiction, QueueSettings.deliveryDelay, WorkerVersionBinding.jurisdiction',
        );
    }, Effect.provide(layer)),
);
