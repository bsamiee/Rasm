import { join } from 'node:path';
import { type BuildOutputWorker, DEFAULT_WORKER_DIRECTORY_NAME, readBuildOutput } from '@cloudflare/build-output-utils';
import { D1Database, Queue, QueueConsumer, R2Bucket, type types, Worker, WorkersCronTrigger, WorkersCustomDomain, WorkerVersion, Workflow } from '@pulumi/cloudflare';
import { type CustomResourceOptions, jsonStringify, type Output, secret } from '@pulumi/pulumi';
import { Array, Cause, Config, Duration, Effect, flow, Match, Predicate, Record, Redacted, Result, String, Struct, UndefinedOr } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Deployment {
    readonly accountId: string;
    readonly root: string;
    readonly production: boolean;
    readonly named: (configured: string) => string;
    readonly bindings: (hosts: Output<readonly string[]>) => Record.ReadonlyRecord<string, Omit<types.input.WorkerVersionBinding, 'name'>>;
    readonly options: CustomResourceOptions;
}
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

// --- [OPERATIONS] ----------------------------------------------------------------------

const deployed = Effect.fnUntraced(function* ({ accountId, production, named, bindings, options }: Deployment, stores: Stores, { config, bundleDir, assetsDir }: BuildOutputWorker, directory: string) {
    const name = named(config.name);
    const [triggers, exported] = [config.triggers ?? [], config.exports ?? {}];
    const consumers = triggers.filter((trigger) => trigger.type === 'queue');
    const schedules = triggers.filter((trigger) => trigger.type === 'scheduled');
    const unapplied = [
        ...Array.dedupe(Array.filterMap(triggers, ({ type }) => (type === 'queue' || type === 'scheduled' ? Result.failVoid : Result.succeed(`triggers.${type}`)))),
        ...(Predicate.hasProperty(config.placement, 'hint') ? ['placement.hint'] : []),
        ...(config.observability?.redactQueryString === undefined ? [] : ['observability.redactQueryString']),
        ...((config.tailConsumers ?? []).some(({ streaming }) => streaming !== undefined) ? ['tailConsumers.streaming'] : []),
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
            tailConsumers: UndefinedOr.map(
                config.tailConsumers,
                Array.map(({ worker: consumer }) => ({ name: consumer })),
            ),
        },
        options,
    );
    const hostnames = (production ? (config.domains ?? []) : [])
        .map((domain) => (Predicate.isString(domain) ? { hostname: domain, production: true } : domain))
        .filter((domain) => domain.production)
        .map(({ hostname }) => hostname);
    const domains = hostnames.map((hostname) => new WorkersCustomDomain(hostname, { accountId, hostname, service: worker.name }, options));
    const address = worker.subdomain.apply(({ enabled, url }) => (enabled === true ? url : undefined));
    const hosts = address.apply((url) => [...hostnames, ...Array.fromNullishOr(url).map((live) => new URL(live).host)]);
    const stated = yield* Effect.forEach(Record.toEntries(config.env ?? {}), ([variable, binding]) =>
        Match.value(binding).pipe(
            Match.when({ type: 'text' }, ({ value }) => Effect.succeed({ type: 'plain_text', text: value })),
            Match.when({ type: 'json' }, ({ type, value }) => Effect.succeed({ type, json: jsonStringify(value) })),
            Match.when({ type: 'secret' }, () => Effect.all({ type: Effect.succeed('secret_text'), text: Effect.map(Config.Redacted(variable), flow(Redacted.value, secret)) })),
            Match.when({ type: 'd1', name: Predicate.isString }, ({ type, name: database }) => Effect.succeed({ type, id: stores.d1[database]?.id })),
            Match.when({ type: 'r2', name: Predicate.isString }, ({ name: bucket }) => Effect.succeed({ type: 'r2_bucket', bucketName: named(bucket) })),
            Match.when({ type: 'queue', name: Predicate.isString }, ({ type, name: queue }) => Effect.succeed({ type, queueName: named(queue) })),
            Match.when({ type: (type: string) => UNNAMED.has(type) }, ({ type }) => Effect.succeed({ type: String.kebabToSnake(type) })),
            Match.orElse(({ type }) => Effect.fail(new Cause.IllegalArgumentError(`Binding ${variable} of Worker ${name} is ${type}, expected text, json, secret, a named d1, r2, or queue, or one of ${[...UNNAMED].join(', ')}`))),
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
            bindings: [...stated, ...Record.collect(directory === DEFAULT_WORKER_DIRECTORY_NAME ? bindings(hosts) : {}, (variable, binding) => ({ ...binding, name: variable }))],
            exports: Record.filter(exported, (entry) => entry.type !== 'workflow'),
        },
        { ...options, dependsOn: Object.values(stores).flatMap((held) => Object.values(held)) },
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

const deployWorker = Effect.fn('deployWorker')(function* (deployment: Deployment) {
    const { accountId, production, named, options } = deployment;
    const built = yield* Effect.tryPromise({ try: () => readBuildOutput(deployment.root), catch: (cause) => new Cause.UnknownError(cause, `${deployment.root} holds no Build Output tree`) });
    const configs = Object.values(built.workers).map(({ config }) => config);
    const bound = configs.flatMap(({ env = {} }) => Object.values(env));
    const pointed = (type: keyof Stores) => Array.dedupe(bound.flatMap((binding) => (binding.type === type ? Array.fromNullishOr(binding.name) : [])));
    const consumed = configs.flatMap(({ triggers = [] }) => triggers.filter((trigger) => trigger.type === 'queue').flatMap(({ name, deadLetterQueue }) => [name, ...Array.fromNullishOr(deadLetterQueue)]));
    const kept = { ...options, protect: production };
    const stores: Stores = {
        d1: Record.fromIterableWith(pointed('d1'), (name) => [name, new D1Database(named(name), { accountId, name: named(name) }, kept)]),
        r2: Record.fromIterableWith(pointed('r2'), (name) => [name, new R2Bucket(named(name), { accountId, name: named(name) }, kept)]),
        queue: Record.fromIterableWith(Array.dedupe([...consumed, ...pointed('queue')]), (name) => [name, new Queue(named(name), { accountId, queueName: named(name) }, options)]),
    };
    return { stores, workers: yield* Effect.all(Record.map(built.workers, (worker, directory) => deployed(deployment, stores, worker, directory))) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { deployWorker };
