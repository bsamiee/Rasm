import { createHash } from 'node:crypto';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { all, runtime } from '@pulumi/pulumi';
import { App } from '@rasm/hosting';
import { deployApp } from '@rasm/hosting/deploy';
import { Array, ByteSize, ConfigProvider, Effect, FileSystem, flow, MutableRef, Path, Record, Schema } from 'effect';

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
            ...Record.fromIterableWith(Object.values(App.bundle.signing), (variable) => [variable, { type: 'secret' }]),
        },
    },
    'workers/default/assets/sheets.json': { sheets: [] },
};
const APP = new App({ name: 'sheets', env: {}, allowed: [], targets: [App.Target.Cloudflare()], limits: { cpus: 1, memory: ByteSize.gibibytes(1) } });
const TYPES = { database: 'cloudflare:index/d1Database:D1Database', bucket: 'cloudflare:index/r2Bucket:R2Bucket', queue: 'cloudflare:index/queue:Queue', version: 'cloudflare:index/workerVersion:WorkerVersion', token: 'cloudflare:index/accountToken:AccountToken' } as const;
const MINTED = { value: 'minted', group: 'bucket-item-write' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);

// --- [SERVICES] ------------------------------------------------------------------------

const registered = MutableRef.make<readonly { readonly type: string; readonly name: string; readonly inputs: Record.ReadonlyRecord<string, unknown> & { readonly bindings?: unknown } }[]>([]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const built = Effect.fnUntraced(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const root = yield* fs.makeTempDirectoryScoped();
    const output = path.join(root, '.cloudflare/output/v1');
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
    'creates each store a binding names with the jurisdiction or delivery delay the binding states, binds an R2 bucket in its jurisdiction with S3 access through a token scoped to it in place of the declared S3 secrets, and binds a D1 database stated by id without creating it',
    Effect.fnUntraced(function* () {
        yield* Effect.promise(() =>
            runtime.setMocks({
                newResource: (resource: (typeof registered.current)[number]) => {
                    MutableRef.update(registered, Array.append(resource));
                    return { id: `${resource.name}-id`, state: resource.type === TYPES.token ? { ...resource.inputs, value: MINTED.value } : resource.inputs };
                },
                call: ({ token, inputs }: { readonly token: string; readonly inputs: Record.ReadonlyRecord<string, unknown> }) => (token.endsWith('getAccountApiTokenPermissionGroupsList') ? { results: [{ id: MINTED.group }] } : inputs),
            }),
        );
        const { default: worker } = yield* deployApp({ app: APP, mode: 'disposable', root: yield* built() }).pipe(Effect.provide(ConfigProvider.layer(ConfigProvider.fromUnknown({ CLOUDFLARE_ACCOUNT_ID: 'account', CLOUDFLARE_API_TOKEN: 'token' }))));
        const { version } = yield* Effect.fromNullishOr(worker);
        yield* Effect.callback<string[]>((resume) => {
            all([version]).apply(flow(Effect.succeed, resume));
        });
        const made = Record.map(
            Array.groupBy(MutableRef.get(registered), ({ type }) => type),
            Record.fromIterableWith(({ name, inputs }) => [name, inputs]),
        );
        assert.deepStrictEqual(
            { databases: made[TYPES.database], buckets: made[TYPES.bucket], queues: made[TYPES.queue], tokens: made[TYPES.token], bindings: made[TYPES.version]?.['sheets-disposable']?.bindings },
            {
                databases: { 'records-disposable': { accountId: 'account', name: 'records-disposable', jurisdiction: 'eu' } },
                buckets: { 'archive-disposable': { accountId: 'account', name: 'archive-disposable', jurisdiction: 'eu' } },
                tokens: { 'sheets-disposable': { accountId: 'account', name: 'sheets-disposable', policies: [{ effect: 'allow', permissionGroups: [{ id: MINTED.group }], resources: yield* Schema.encodeUnknownEffect(Json)({ 'com.cloudflare.edge.r2.bucket.account_eu_archive-disposable': '*' }) }] } },
                queues: { 'jobs-disposable': { accountId: 'account', queueName: 'jobs-disposable', settings: { deliveryDelay: 60 } }, 'notices-disposable': { accountId: 'account', queueName: 'notices-disposable' } },
                bindings: {
                    [runtime.specialSigKey]: runtime.specialSecretSig,
                    value: [
                        { name: 'ASSETS', type: 'assets' },
                        { name: 'ARCHIVE', type: 'r2_bucket', bucketName: 'archive-disposable', jurisdiction: 'eu' },
                        { name: 'RECORDS', type: 'd1', id: 'records-disposable-id' },
                        { name: 'LEDGER', type: 'd1', id: LEDGER },
                        { name: 'JOBS', type: 'queue', queueName: 'jobs-disposable' },
                        { name: 'NOTICES', type: 'queue', queueName: 'notices-disposable' },
                        { name: 'AWS_ENDPOINT_URL_S3', type: 'plain_text', text: 'https://account.eu.r2.cloudflarestorage.com' },
                        { name: 'AWS_REGION', type: 'plain_text', text: 'auto' },
                        { name: 'AWS_ACCESS_KEY_ID', type: 'secret_text', text: 'sheets-disposable-id' },
                        { name: 'AWS_SECRET_ACCESS_KEY', type: 'secret_text', text: createHash('sha256').update(MINTED.value).digest('hex') },
                    ],
                },
            },
            'cloudflare/workers-sdk@88c774b packages/wrangler/src/__tests__/create-worker-upload-form/bindings.test.ts "should include R2 bindings with a bucket_name", "should include D1 bindings with a database_id", packages/wrangler/src/__tests__/deploy/queues.test.ts "should update queue producers on deploy", @pulumi/cloudflare R2BucketArgs.jurisdiction, D1DatabaseArgs.jurisdiction, QueueSettings.deliveryDelay, WorkerVersionBinding.jurisdiction',
        );
    }, Effect.provide(layer)),
);
