import { createHash } from 'node:crypto';
import { join } from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';
import { type BuildOutputWorker, DEFAULT_WORKER_DIRECTORY_NAME, readBuildOutput } from '@cloudflare/build-output-utils';
import {
    AccountToken,
    D1Database,
    getAccountApiTokenPermissionGroupsListOutput,
    getZeroTrustOrganizationOutput,
    HyperdriveConfig,
    Provider,
    Queue,
    QueueConsumer,
    R2Bucket,
    R2BucketCors,
    R2BucketLifecycle,
    type types,
    Worker,
    WorkersCronTrigger,
    WorkersCustomDomain,
    WorkerVersion,
    Workflow,
    ZeroTrustAccessApplication,
    ZeroTrustAccessPolicy,
} from '@pulumi/cloudflare';
import { local } from '@pulumi/command';
import { type CustomResourceOptions, jsonStringify, type Output, output, type Resource, secret } from '@pulumi/pulumi';
import { Array, Cause, Config, Duration, Effect, flow, Match, Predicate, Record, Redacted, Result, Schema, String, Struct, UndefinedOr } from 'effect';
import { App, type Binding } from '#/app/app';
import { Access, Identity } from '#/app/identity';

// --- [TYPES] ---------------------------------------------------------------------------

interface Stores {
    readonly d1: Record.ReadonlyRecord<string, D1Database>;
    readonly r2: Record.ReadonlyRecord<string, R2Bucket>;
    readonly queue: Record.ReadonlyRecord<string, Queue>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONTENT = {
    esm: 'application/javascript+module',
    cjs: 'application/javascript',
    json: 'application/json',
    text: 'text/plain',
    data: 'application/octet-stream',
    wasm: 'application/wasm',
    sourcemap: 'application/source-map',
    python: 'text/x-python',
    'python-requirement': 'text/x-python-requirement',
} as const;
const UNNAMED: ReadonlySet<string> = new Set(['assets', 'ai', 'browser', 'images', 'media', 'version-metadata']);
const MOUNT = '/data';

// --- [OPERATIONS] ----------------------------------------------------------------------

const aged = (age: Duration.Duration) => ({ condition: { type: 'Age', maxAge: Duration.toSeconds(age) } });
const pointed = <B extends { readonly name?: string | undefined }>(held: readonly B[]) => Record.fromEntries(Array.filterMap(held, (binding) => (binding.name === undefined ? Result.failVoid : Result.succeed([binding.name, binding] as const))));
const deployed = Effect.fnUntraced(function* (
    {
        accountId,
        production,
        named,
        bindings,
        options,
    }: { readonly accountId: string; readonly production: boolean; readonly named: (configured: string) => string; readonly bindings: (hosts: Output<readonly string[]>) => Record.ReadonlyRecord<string, Omit<types.input.WorkerVersionBinding, 'name'>>; readonly options: CustomResourceOptions },
    stores: Stores,
    { config, bundleDir, assetsDir }: BuildOutputWorker,
    directory: string,
) {
    const name = named(config.name);
    const [triggers, exported] = [config.triggers ?? [], config.exports ?? {}];
    const consumers = triggers.filter((trigger) => trigger.type === 'queue');
    const schedules = triggers.filter((trigger) => trigger.type === 'scheduled');
    const unapplied = [
        ...Array.dedupe(Array.filterMap(triggers, ({ type }) => (type === 'queue' || type === 'scheduled' ? Result.failVoid : Result.succeed(`triggers.${type}`)))),
        ...(Predicate.hasProperty(config.placement, 'hint') ? ['placement.hint'] : []),
        ...(config.observability?.redactQueryString === undefined ? [] : ['observability.redactQueryString']),
    ];
    yield* Array.isArrayNonEmpty(unapplied) ? Effect.fail(new Cause.IllegalArgumentError(`Worker ${name} states ${unapplied.join(', ')}, fields with no @pulumi/cloudflare resource argument`)) : Effect.void;
    const worker = new Worker(
        name,
        {
            accountId,
            name,
            subdomain: { enabled: config.workersDev, previewsEnabled: config.previewUrls },
            observability: UndefinedOr.map(config.observability, Struct.pick(['enabled', 'headSamplingRate', 'issues', 'logs', 'traces'])),
            logpush: config.logpush,
            tailConsumers: config.tailConsumers?.map(({ worker: consumer }) => ({ name: consumer })),
        },
        options,
    );
    const hostnames = (production ? (config.domains ?? []) : [])
        .map((domain) => (Predicate.isString(domain) ? { hostname: domain, production: true } : domain))
        .filter((domain) => domain.production)
        .map(({ hostname }) => hostname);
    const domains = hostnames.map((hostname) => new WorkersCustomDomain(hostname, { accountId, hostname, service: worker.name }, options));
    const address = worker.subdomain.apply(({ enabled, url }) => (enabled ? url : undefined));
    const hosts = address.apply((url) => [...hostnames, ...Array.fromNullishOr(url).map((live) => new URL(live).host)]);
    const provided = directory === DEFAULT_WORKER_DIRECTORY_NAME ? bindings(hosts) : {};
    const stated = yield* Effect.forEach(Record.toEntries(Record.filter(config.env ?? {}, (_, variable) => !Record.has(provided, variable))), ([variable, binding]) =>
        Match.value(binding).pipe(
            Match.when({ type: 'text' }, ({ value }) => Effect.succeed({ type: 'plain_text', text: value })),
            Match.when({ type: 'json' }, ({ type, value }) => Effect.succeed({ type, json: jsonStringify(value) })),
            Match.when({ type: 'secret' }, () => Effect.all({ type: Effect.succeed('secret_text'), text: Effect.map(Config.Redacted(variable), flow(Redacted.value, secret)) })),
            Match.when({ type: 'd1', id: Predicate.isString }, ({ type, id }) => Effect.succeed({ type, id })),
            Match.when({ type: 'd1', name: Predicate.isString }, ({ type, name: database }) => Effect.succeed({ type, id: stores.d1[database]?.id })),
            Match.when({ type: 'r2', name: Predicate.isString }, ({ name: bucket, jurisdiction }) => Effect.succeed({ type: 'r2_bucket', bucketName: named(bucket), jurisdiction })),
            Match.when({ type: 'queue', name: Predicate.isString }, ({ type, name: queue }) => Effect.succeed({ type, queueName: named(queue) })),
            Match.when({ type: (type: string) => UNNAMED.has(type) }, ({ type }) => Effect.succeed({ type: String.kebabToSnake(type) })),
            Match.orElse(({ type }) => Effect.fail(new Cause.IllegalArgumentError(`Binding ${variable} of Worker ${name} is ${type}, expected text, json, secret, a d1 with an id or a name, a named r2 or queue, or one of ${UNNAMED.values().join(', ')}`))),
            Effect.map((fields) => ({ ...fields, name: variable })),
        ),
    );
    const version = new WorkerVersion(
        name,
        {
            accountId,
            workerId: worker.id,
            deploy: true,
            compatibilityDate: config.compatibilityDate,
            compatibilityFlags: config.compatibilityFlags,
            limits: config.limits,
            placement: config.placement,
            cacheOptions: config.cache,
            mainModule: config.manifest?.mainModule,
            modules: Array.fromNullishOr(bundleDir).flatMap((bundle) => Record.collect(config.manifest?.modules ?? {}, (module, { type }) => ({ name: module, contentType: CONTENT[type], contentFile: join(bundle, module) }))),
            assets: UndefinedOr.map(assetsDir, (assets) => ({ directory: assets, config: config.assets })),
            bindings: [...stated, ...Record.collect(provided, (variable, binding) => ({ ...binding, name: variable }))],
            exports: Record.filter(exported, (entry) => entry.type !== 'workflow'),
        },
        { ...options, dependsOn: Record.values<keyof Stores, Stores[keyof Stores]>(stores).flatMap(Object.values<Resource>) },
    );
    const applied = { ...options, dependsOn: [version] };
    const cron = Array.isArrayNonEmpty(schedules) ? [new WorkersCronTrigger(name, { accountId, scriptName: worker.name, schedules: schedules.map(({ schedule }) => ({ cron: schedule })) }, applied)] : [];
    const consumed = consumers.flatMap(({ name: queue, deadLetterQueue, maxBatchSize, maxBatchTimeout, maxConcurrency, ...settings }) => {
        const tuned = { ...Struct.pick(settings, ['maxRetries', 'retryDelay', 'visibilityTimeoutMs']), maxConcurrency: maxConcurrency ?? undefined, batchSize: maxBatchSize, maxWaitTimeMs: UndefinedOr.map(maxBatchTimeout, flow(Duration.seconds, Duration.toMillis)) };
        return Array.fromNullishOr(stores.queue[queue]).map(({ queueId }) => new QueueConsumer(`${name}-${queue}`, { accountId, queueId, scriptName: worker.name, type: 'worker', deadLetterQueue: UndefinedOr.map(deadLetterQueue, named), settings: tuned }, applied));
    });
    const workflows = Record.filterMap(exported, (entry, className) =>
        entry.type === 'workflow'
            ? Result.succeed(new Workflow(named(entry.name), { accountId, className, scriptName: worker.name, workflowName: named(entry.name), schedules: Array.ensure(entry.schedules ?? []).map((schedule) => ({ cron: schedule })), ...Struct.pick(entry, ['limits', 'concurrency', 'defaultRetention']) }, applied))
            : Result.failVoid,
    );
    return { url: address, domains: domains.map(({ hostname }) => hostname), version: version.id, invoked: [...cron, ...consumed, ...Object.values(workflows)].map(({ urn }) => urn) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const deploy = {
    Cloudflare: Effect.fn('deploy.Cloudflare')(function* ({ app, mode, root }: { readonly app: App; readonly mode: keyof typeof App.modes; readonly root: string }) {
        const [{ accountId, token }, context] = yield* Effect.all([Config.all({ accountId: Config.String('CLOUDFLARE_ACCOUNT_ID'), token: Config.Redacted('CLOUDFLARE_API_TOKEN') }), Effect.context<never>()]);
        const [name, { kept, suffix }] = [App.resource(app, mode), App.modes[mode]];
        const named = (configured: string) => `${configured}${suffix}`;
        const options = { provider: new Provider(name, { apiToken: Redacted.value(token) }) };
        const pooled = yield* Effect.forEach(Record.keys(Record.filter(app.env, App.Binding.$is('postgres'))), (variable) =>
            Effect.map(Config.URL(variable), ({ hostname, port, pathname, username, password }) => {
                const [store, origin] = [App.resource(app, mode, variable), { scheme: 'postgres', host: hostname, ...(port !== '' && { port: Number(port) }), database: pathname.slice(1), user: decodeURIComponent(username), password: secret(decodeURIComponent(password)) }];
                return [variable, { type: 'hyperdrive', id: new HyperdriveConfig(store, { accountId, name: store, origin }, options).id }] as const;
            }),
        );
        const built = yield* Effect.tryPromise({ try: () => readBuildOutput(root), catch: (cause) => new Cause.UnknownError(cause, `${root} holds no Build Output tree`) });
        const configs = Object.values(built.workers).map(({ config }) => config);
        const bound = configs.flatMap(({ env = {} }) => Object.values(env));
        const buckets = Record.toEntries(pointed(bound.filter((binding) => binding.type === 'r2')));
        const [place, ...places] = Array.dedupe(buckets.map(([, { jurisdiction }]) => jurisdiction ?? 'default'));
        yield* Array.isArrayNonEmpty(places) ? Effect.fail(new Cause.IllegalArgumentError(`${app.name} binds buckets in jurisdictions ${[place, ...places].join(', ')}, one S3 endpoint serves one jurisdiction`)) : Effect.void;
        const signing = UndefinedOr.map(place, (jurisdiction) => {
            const writing = getAccountApiTokenPermissionGroupsListOutput({ accountId, name: encodeURIComponent('Workers R2 Storage Bucket Item Write'), scope: 'com.cloudflare.edge.r2.bucket' }, options).results;
            const resources = JSON.stringify(Record.fromIterableWith(buckets, ([bucket]) => [`com.cloudflare.edge.r2.bucket.${accountId}_${jurisdiction}_${named(bucket)}`, '*']));
            const granted = new AccountToken(name, { accountId, name, policies: [{ effect: 'allow', permissionGroups: writing.apply(Array.map(({ id }) => ({ id }))), resources }] }, options);
            return {
                [App.bundle.signing.endpoint]: { type: 'plain_text', text: `https://${accountId}${jurisdiction === 'default' ? '' : `.${jurisdiction}`}.r2.cloudflarestorage.com` },
                [App.bundle.signing.region]: { type: 'plain_text', text: 'auto' },
                [App.bundle.signing.key]: { type: 'secret_text', text: granted.id },
                [App.bundle.signing.secret]: { type: 'secret_text', text: secret(granted.value.apply((value) => createHash('sha256').update(value).digest('hex'))) },
            };
        });
        const bindings = (hosts: Output<readonly string[]>) => ({
            [App.bundle.mode]: { type: 'plain_text', text: mode },
            ...Record.fromEntries(pooled),
            ...signing,
            ...Array.match(app.allowed, {
                onEmpty: () => ({}),
                onNonEmpty: (addresses) => {
                    const policy = new ZeroTrustAccessPolicy(name, { accountId, name, decision: 'allow', includes: addresses.map((email) => ({ email: { email } })) }, options);
                    const destinations = hosts.apply(Array.map((host) => ({ type: 'public', uri: `${host}${Identity.paths.signin}` })));
                    const guarded = new ZeroTrustAccessApplication(name, { accountId, name, type: 'self_hosted', destinations, policies: [{ id: policy.id, precedence: 1 }], httpOnlyCookieAttribute: true, sameSiteCookieAttribute: 'lax' }, options);
                    const source = { _tag: 'Cloudflare', teamDomain: getZeroTrustOrganizationOutput({ accountId }, options).authDomain, audience: guarded.aud } as const;
                    return { [Access.variable]: { type: 'plain_text', text: output({ source, logout: Access.cloudflare.logout }).apply((access) => Effect.runPromiseWith(context)(Effect.flatMap(Schema.decodeEffect(Access)(access), Schema.encodeEffect(Access.json)))) } };
                },
            }),
        });
        const consumed = configs.flatMap(({ triggers = [] }) => triggers.filter((trigger) => trigger.type === 'queue').flatMap(({ name: queue, deadLetterQueue }) => [queue, ...Array.fromNullishOr(deadLetterQueue)]));
        const produced = pointed(bound.filter((binding) => binding.type === 'queue'));
        const protect = { ...options, protect: kept };
        const stores: Stores = {
            d1: Record.map(
                Record.filter(pointed(bound.filter((binding) => binding.type === 'd1')), ({ id }) => id === undefined),
                ({ jurisdiction }, database) => new D1Database(named(database), { accountId, name: named(database), jurisdiction }, protect),
            ),
            r2: Record.map(Record.fromEntries(buckets), ({ jurisdiction }, bucket) => new R2Bucket(named(bucket), { accountId, name: named(bucket), jurisdiction }, protect)),
            queue: Record.fromIterableWith(Array.dedupe([...consumed, ...Record.keys(produced)]), (queue) => [queue, new Queue(named(queue), { accountId, queueName: named(queue), settings: UndefinedOr.map(produced[queue]?.deliveryDelay, (deliveryDelay) => ({ deliveryDelay })) }, options)]),
        };
        const workers = yield* Effect.all(Record.map(built.workers, (worker, directory) => deployed({ accountId, production: kept, named, bindings, options }, stores, worker, directory)));
        yield* Effect.forEach(
            Record.toEntries(Record.filter(app.env, App.Binding.$is('bucket'))),
            Effect.fnUntraced(function* ([variable, { abort, expire, infrequentAccess, cors }]) {
                const [stated, store] = [App.resource(app, 'production', variable), App.resource(app, mode, variable)];
                const bucket = yield* Effect.fromOption(Record.get(stores.r2, stated), () => new Cause.NoSuchElementError(`Build Output of ${root} binds no bucket ${stated}, the store ${variable} of ${app.name}`));
                const rules = [
                    { id: 'abort', enabled: true, conditions: { prefix: '' }, abortMultipartUploadsTransition: aged(abort) },
                    ...Record.collect(expire[mode], (prefix, age) => ({ id: `expire ${prefix}*`, enabled: true, conditions: { prefix }, deleteObjectsTransition: aged(age) })),
                    ...Record.collect(infrequentAccess, (prefix, age) => ({ id: `infrequent access ${prefix}*`, enabled: true, conditions: { prefix }, storageClassTransitions: [{ storageClass: 'InfrequentAccess', ...aged(age) }] })),
                ];
                const allowed = cors.map(({ origins, methods, headers, exposed, age }) => ({ allowed: { origins: [...origins], methods: [...methods], headers: [...headers] }, exposeHeaders: [...exposed], maxAgeSeconds: Duration.toSeconds(age) }));
                const emptying = { delete: `node ${fileURLToPath(import.meta.resolve('#/deploy/bucket-job'))}`, environment: { CLOUDFLARE_ACCOUNT_ID: accountId, CLOUDFLARE_API_TOKEN: token.pipe(Redacted.value, (value) => secret(value)), BUCKET: bucket.name } };
                return [new R2BucketLifecycle(store, { accountId, bucketName: bucket.name, rules }, options), ...(Array.isArrayNonEmpty(allowed) ? [new R2BucketCors(store, { accountId, bucketName: bucket.name, rules: allowed }, options)] : []), ...(kept ? [] : [new local.Command(`${store}-objects`, emptying)])];
            }),
            { discard: true },
        );
        return workers;
    }),
    Machine: Effect.fn('deploy.Machine')(function* ({ app, mode, port, bundle }: { readonly app: App; readonly mode: keyof typeof App.modes; readonly port: number; readonly bundle: string }) {
        const variables = yield* Effect.all(
            Record.map(app.env, (binding, variable) =>
                Match.value<Binding>(binding).pipe(
                    Match.tag('sqlite', () => Effect.succeedSome(join(MOUNT, variable, 'database.sqlite'))),
                    Match.tag('bucket', () => Effect.succeedSome(App.resource(app, mode, variable))),
                    Match.tag('postgres', 'secret', () => Effect.asSome(Config.Redacted(variable))),
                    Match.tag('text', 'json', () => Effect.succeedNone),
                    Match.exhaustive,
                ),
            ),
        );
        const volumes = Record.collect(Record.filter(app.env, App.Binding.$is('sqlite')), (variable) => ({ name: App.resource(app, mode, variable), target: join(MOUNT, variable) }));
        return {
            image: {
                dockerfile: `# syntax=docker/dockerfile:1\nFROM node:${process.versions.node}-trixie-slim\nRUN install -d -o node -g node ${[MOUNT, ...volumes.map(({ target }) => target)].join(' ')}\nUSER node\nWORKDIR /app\nCOPY . .\nENTRYPOINT ["node"]\nCMD ["${App.bundle.entry}.js"]\n`,
                context: { location: bundle },
            },
            healthcheck: ['CMD', 'node', '--input-type=module', '--eval', `const response = await fetch('http://127.0.0.1:${port}${App.bundle.readiness}'); if (response.status !== 204) throw new Error('HTTP ' + response.status);`],
            env: { ...Record.getSomes<string, string | Redacted.Redacted>(variables), [App.bundle.portVariable]: `${port}` },
            volumes,
            buckets: Record.mapEntries(Record.filter(app.env, App.Binding.$is('bucket')), ({ abort, expire, cors }, variable) => [App.resource(app, mode, variable), { kept: App.modes[mode].kept, abort, expire: expire[mode], cors }]),
        };
    }),
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { deploy };
