import { join } from 'node:path';
import { type BuildOutputWorker, DEFAULT_WORKER_DIRECTORY_NAME, readBuildOutput } from '@cloudflare/build-output-utils';
import { Queue, QueueConsumer, type types, Worker, WorkersCronTrigger, WorkersCustomDomain, WorkerVersion, Workflow } from '@pulumi/cloudflare';
import type { CustomResourceOptions, Output } from '@pulumi/pulumi';
import { Array, Cause, Duration, Effect, flow, Match, Predicate, Record, Result, String, Struct, UndefinedOr } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Deployment {
    readonly accountId: string;
    readonly root: string;
    readonly production: boolean;
    readonly named: (configured: string) => string;
    readonly bindings: (hosts: Output<readonly string[]>) => Record.ReadonlyRecord<string, Omit<types.input.WorkerVersionBinding, 'name'>>;
    readonly options: CustomResourceOptions;
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

const deployed = Effect.fnUntraced(function* ({ accountId, production, named, bindings, options }: Deployment, { config, bundleDir, assetsDir }: BuildOutputWorker, directory: string) {
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
    const declared = directory === DEFAULT_WORKER_DIRECTORY_NAME ? bindings(hosts) : {};
    const beside = Record.toEntries(Record.filter(config.env ?? {}, (_, variable) => !Record.has(declared, variable)));
    const produced = beside.flatMap(([, binding]) => (binding.type === 'queue' ? Array.fromNullishOr(binding.name) : []));
    const queues = Record.fromIterableWith(Array.dedupe([...consumers.flatMap(({ name: queue, deadLetterQueue }) => [queue, ...Array.fromNullishOr(deadLetterQueue)]), ...produced]), (queue) => [queue, new Queue(named(queue), { accountId, queueName: named(queue) }, options)]);
    const carried = yield* Effect.forEach(beside, ([variable, binding]) =>
        Match.value(binding).pipe(
            Match.when({ type: 'queue', name: Predicate.isString }, ({ type, name: queue }) => Effect.succeed({ name: variable, type, queueName: named(queue) })),
            Match.when({ type: (type: string) => UNNAMED.has(type) }, ({ type }) => Effect.succeed({ name: variable, type: String.kebabToSnake(type) })),
            Match.orElse(({ type }) => Effect.fail(new Cause.IllegalArgumentError(`Binding ${variable} of Worker ${name} is ${type}, expected a declared binding, a named queue, or one of ${[...UNNAMED].join(', ')}`))),
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
            bindings: [...Record.collect(declared, (variable, binding) => ({ ...binding, name: variable })), ...carried],
            exports: Record.filter(exported, (entry) => entry.type !== 'workflow'),
        },
        { ...options, dependsOn: Object.values(queues) },
    );
    const applied = { ...options, dependsOn: [version] };
    const cron = Array.isArrayNonEmpty(schedules) ? [new WorkersCronTrigger(name, { accountId, scriptName: worker.name, schedules: schedules.map(({ schedule }) => ({ cron: schedule })) }, applied)] : [];
    const consumed = consumers.flatMap(({ name: queue, deadLetterQueue, maxBatchSize, maxBatchTimeout, maxConcurrency, ...settings }) => {
        const tuned = { ...Struct.pick(settings, ['maxRetries', 'retryDelay', 'visibilityTimeoutMs']), maxConcurrency: maxConcurrency ?? undefined, batchSize: maxBatchSize, maxWaitTimeMs: UndefinedOr.map(maxBatchTimeout, flow(Duration.seconds, Duration.toMillis)) };
        return Array.fromNullishOr(queues[queue]).map(({ queueId }) => new QueueConsumer(`${name}-${queue}`, { accountId, queueId, scriptName: worker.name, type: 'worker', deadLetterQueue: UndefinedOr.map(deadLetterQueue, named), settings: tuned }, applied));
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
    const built = yield* Effect.tryPromise({ try: () => readBuildOutput(deployment.root), catch: (cause) => new Cause.UnknownError(cause, `${deployment.root} holds no Build Output tree`) });
    return yield* Effect.all(Record.map(built.workers, (worker, directory) => deployed(deployment, worker, directory)));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { deployWorker };
