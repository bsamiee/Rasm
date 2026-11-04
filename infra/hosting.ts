import process from 'node:process';
import { type BuildOutput, readBuildOutput } from '@cloudflare/build-output-utils';
import {
    D1Database,
    getZeroTrustOrganizationOutput,
    getZones,
    HyperdriveConfig,
    Queue,
    QueueConsumer,
    R2Bucket,
    Worker,
    WorkersCronTrigger,
    WorkersCustomDomain,
    WorkersKvNamespace,
    WorkersRoute,
    WorkerVersion,
    ZeroTrustAccessApplication,
    ZeroTrustAccessIdentityProvider,
    ZeroTrustAccessPolicy,
} from '@pulumi/cloudflare';
import { Container, type ContainerArgs, getRegistryImageOutput, Network, RemoteImage, Volume } from '@pulumi/docker';
import { Image } from '@pulumi/docker-build';
import { IamPolicy, IamUser, IamUserPolicyAttachment, Provider, S3Bucket } from '@pulumi/minio';
import postgresql from '@pulumi/postgresql';
import { type CustomResourceOptions, type Input, interpolate, jsonStringify, secret } from '@pulumi/pulumi';
import { RandomInteger, RandomPassword } from '@pulumi/random';
import { CloudflareAccess } from '@rasm/hosting';
import { vite } from '@rasm/hosting/vite';
import { sites } from '@rasm/portfolio/sites';
import { Array, Cache, Cause, Config, Duration, Effect, Function, Match, Option, Order, Path, Predicate, Record, Redacted, String, Struct } from 'effect';
import { resolveConfig } from 'vite';

// --- [TYPES] ---------------------------------------------------------------------------

type Trigger = NonNullable<BuildOutput['workers']['default']['config']['triggers']>[number];
interface Stack {
    readonly name: string;
    readonly hostnames: readonly string[];
    readonly production: boolean;
    readonly named: (variable: string) => string;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const refuse = (subject: string) => Effect.fail(new Cause.IllegalArgumentError(`${subject} has no hosting form`));
const migrator = (variable: string) => `${variable}_MIGRATOR`;
const password = (resource: string) => new RandomPassword(resource, { length: 40, special: false });
const postgres = (resource: string, server: postgresql.ProviderArgs & { readonly database: string }, protect: boolean, options?: CustomResourceOptions) => {
    const provider = new postgresql.Provider(resource, server, options);
    const login = (role: string, settings: postgresql.RoleArgs) => {
        const { result } = password(`${resource}-${role}`);
        return { name: new postgresql.Role(`${resource}-${role}`, { name: `${resource}-${role}`, login: true, password: result, ...settings }, { provider }).name, password: result };
    };
    const owner = login('migrator', { idleInTransactionSessionTimeout: 60_000 });
    const schema = new postgresql.Schema(resource, { name: owner.name, owner: owner.name, database: server.database }, { provider, protect });
    const runtime = login('runtime', { statementTimeout: 15_000, idleInTransactionSessionTimeout: 30_000, searchPaths: [schema.name] });
    const statistics = new postgresql.Extension(`${resource}-pg-stat-statements`, { name: 'pg_stat_statements', database: server.database }, { provider });
    return {
        migrator: { user: new postgresql.Grant(`${resource}-migrator`, { database: server.database, role: schema.owner, objectType: 'database', privileges: ['CREATE'] }, { provider, dependsOn: statistics }).role, password: owner.password },
        runtime: { user: new postgresql.Grant(`${resource}-runtime`, { database: server.database, role: runtime.name, objectType: 'schema', schema: schema.name, privileges: ['USAGE'] }, { provider }).role, password: runtime.password },
    };
};
const cloudflare = function* ({ rootConfig, workers }: BuildOutput, { name, hostnames, production, named }: Stack) {
    const { config, bundleDir, assetsDir } = workers.default;
    const [path, accountId] = yield* Effect.all([Path.Path, rootConfig.accountId === undefined ? Config.String('CLOUDFLARE_ACCOUNT_ID') : Config.succeed(rootConfig.accountId)]);
    const triggers = config.triggers ?? [];
    const triggered = <K extends Trigger['type']>(type: K) => triggers.filter((trigger): trigger is Extract<Trigger, { readonly type: K }> => trigger.type === type);
    yield* Effect.forEach(
        triggers,
        Match.type<Trigger>().pipe(
            Match.whenOr({ type: 'fetch' }, { type: 'queue' }, { type: 'scheduled' }, () => Effect.void),
            Match.orElse(({ type }) => refuse(`trigger of type ${type}`)),
        ),
    );
    const tails = yield* Effect.forEach(config.tailConsumers ?? [], (tail) => (tail.streaming === true ? refuse(`streaming tail consumer ${tail.worker}`) : Effect.succeed({ name: tail.worker })));
    const placement = yield* Match.value(config.placement).pipe(
        Match.whenOr(Predicate.isUndefined, { mode: 'off' }, () => Effect.undefined),
        Match.when({ mode: 'smart', hint: Match.string }, ({ hint }) => refuse(`placement hint ${hint}`)),
        Match.when({ mode: 'smart' }, () => Effect.succeed({ mode: 'smart' })),
        Match.orElse((targeted) => Effect.succeed({ ...targeted, mode: 'targeted' })),
    );
    yield* Effect.forEach(Record.toEntries(config.exports ?? {}), ([className, declared]) =>
        Match.value(declared).pipe(
            Match.when({ type: 'workflow' }, () => refuse(`workflow export ${className}`)),
            Match.when({ container: Match.string }, ({ container }) => refuse(`durable object ${className} in container ${container}`)),
            Match.orElse(() => Effect.void),
        ),
    );
    const queues = yield* Cache.make({ capacity: Number.POSITIVE_INFINITY, lookup: (queueName: string) => Effect.sync(() => new Queue(queueName, { accountId, queueName }, { protect: production })) });
    const bindings = yield* Effect.forEach(Record.toEntries(config.env ?? {}), ([variable, binding]) => {
        const resource = named(variable);
        return Match.value(binding).pipe(
            Match.whenOr({ type: 'ai' }, { type: 'assets' }, { type: 'browser' }, { type: 'images' }, { type: 'media' }, { type: 'version-metadata' }, () => Effect.succeed({})),
            Match.when({ type: 'worker', props: Predicate.isObject }, () => refuse(`service binding ${variable} with props`)),
            Match.when({ type: 'queue', deliveryDelay: Match.number }, ({ deliveryDelay }) => refuse(`queue binding ${variable} with delivery delay ${deliveryDelay}`)),
            Match.discriminators('type')({
                'ai-search': ({ name: instanceName }) => Effect.succeed({ instanceName }),
                'ai-search-namespace': ({ namespace }) => Effect.succeed({ namespace }),
                'analytics-engine-dataset': ({ name: dataset = resource }) => Effect.succeed({ type: 'analytics_engine', dataset }),
                artifacts: ({ namespace }) => Effect.succeed({ namespace }),
                d1: ({ id, name: database = resource }) => Effect.succeed({ id: id ?? new D1Database(resource, { accountId, name: database }, { protect: production }).uuid }),
                'durable-object': ({ worker: scriptName, exportName: className }) => Effect.succeed({ type: 'durable_object_namespace', className, scriptName: scriptName === config.name ? undefined : scriptName }),
                hyperdrive: () =>
                    Effect.map(Config.URL(variable), (url) => {
                        const server = { host: url.hostname, port: url.port === '' ? undefined : Number(url.port), database: url.pathname.slice(1) };
                        const { migrator: owner, runtime } = postgres(resource, { ...server, username: decodeURIComponent(url.username), password: secret(decodeURIComponent(url.password)) }, production);
                        return [
                            { id: new HyperdriveConfig(resource, { accountId, name: resource, origin: { scheme: url.protocol.slice(0, -1), ...server, ...runtime }, caching: { disabled: true } }).id },
                            { name: migrator(variable), type: 'secret_text', text: interpolate`${url.protocol}//${owner.user}:${owner.password}@${url.host}${url.pathname}${url.search}` },
                        ];
                    }),
                json: ({ value }) => Effect.succeed({ json: jsonStringify(value) }),
                k2: ({ stream }) => Effect.succeed({ stream }),
                kv: ({ id }) => Effect.succeed({ type: 'kv_namespace', namespaceId: id ?? new WorkersKvNamespace(resource, { accountId, title: resource }, { protect: production }).id }),
                'mtls-certificate': ({ id: certificateId }) => Effect.succeed({ certificateId }),
                pipeline: ({ name: pipeline }) => Effect.succeed({ type: 'pipelines', pipeline }),
                queue: ({ name: queueName = resource }) => Effect.map(Cache.get(queues, queueName), (queue) => ({ queueName: queue.queueName })),
                r2: (bucket) => Effect.succeed({ type: 'r2_bucket', bucketName: new R2Bucket(resource, { accountId, name: bucket.name ?? resource, jurisdiction: bucket.jurisdiction }, { protect: production }).name, jurisdiction: bucket.jurisdiction }),
                'rate-limit': ({ namespace: namespaceId, simple }) => Effect.succeed({ type: 'ratelimit', namespaceId, simple }),
                secret: () => Effect.map(Config.Redacted(variable), (value) => ({ type: 'secret_text', text: secret(Redacted.value(value)) })),
                'secrets-store-secret': ({ storeId, secretName }) => Effect.succeed({ storeId, secretName }),
                'send-email': ({ destinationAddress, allowedDestinationAddresses, allowedSenderAddresses }) => Effect.succeed({ destinationAddress, allowedDestinationAddresses, allowedSenderAddresses }),
                text: ({ value }) => Effect.succeed({ type: 'plain_text', text: value }),
                vectorize: ({ name: indexName }) => Effect.succeed({ indexName }),
                'vpc-network': ({ tunnelId, networkId }) => Effect.succeed({ tunnelId, networkId }),
                'vpc-service': ({ id: serviceId }) => Effect.succeed({ serviceId }),
                worker: ({ worker: service, exportName: entrypoint }) => Effect.succeed({ type: 'service', service: service === config.name ? name : service, entrypoint }),
                workflow: ({ name: workflowName, worker: scriptName, exportName: className }) => Effect.succeed({ workflowName, scriptName, className }),
            }),
            Match.orElse(({ type }) => refuse(`binding ${variable} of type ${type}`)),
            Effect.map((made) => ({ name: variable, type: String.kebabToSnake(binding.type), made: Array.ensure(made) })),
        );
    });
    const worker = new Worker(name, { accountId, name, logpush: config.logpush, observability: config.observability, tailConsumers: tails, subdomain: { enabled: config.workersDev ?? true, previewsEnabled: config.previewUrls } });
    const pin = new ZeroTrustAccessIdentityProvider(name, { accountId, name, type: 'onetimepin', config: {} });
    const access = new ZeroTrustAccessApplication(name, {
        accountId,
        name,
        type: 'self_hosted',
        destinations: worker.subdomain.apply(({ enabled, url }) => [...hostnames, ...(enabled ? [new URL(url).host] : [])].map((host) => ({ type: 'public', uri: `${host}${sites.paths.signin}` }))),
        allowedIdps: [pin.id],
        autoRedirectToIdentity: true,
        policies: [{ id: new ZeroTrustAccessPolicy(name, { accountId, name, decision: 'allow', includes: [{ everyone: {} }] }).id, precedence: 1 }],
    });
    const issuer = interpolate`https://${getZeroTrustOrganizationOutput({ accountId }).authDomain}`;
    const version = new WorkerVersion(name, {
        accountId,
        workerId: worker.id,
        compatibilityDate: config.compatibilityDate,
        compatibilityFlags: config.compatibilityFlags,
        limits: config.limits,
        cacheOptions: config.cache,
        placement,
        exports: config.exports,
        ...(config.manifest === undefined || bundleDir === undefined
            ? {}
            : {
                  mainModule: config.manifest.mainModule,
                  modules: Record.collect(config.manifest.modules, (module, { type }) => ({
                      name: module,
                      contentFile: path.join(bundleDir, module),
                      contentType: (
                          {
                              esm: 'application/javascript+module',
                              cjs: 'application/javascript',
                              json: 'application/json',
                              text: 'text/plain',
                              data: 'application/octet-stream',
                              wasm: 'application/wasm',
                              sourcemap: 'application/source-map',
                              python: 'text/x-python',
                              'python-requirement': 'text/x-python-requirement',
                          } satisfies Record<typeof type, string>
                      )[type],
                  })),
              }),
        assets: assetsDir === undefined ? undefined : { directory: assetsDir, config: config.assets },
        bindings: [
            ...bindings.flatMap(({ made, ...binding }) => made.map((fields) => ({ ...binding, ...fields }))),
            { name: CloudflareAccess.binding, type: 'json', json: jsonStringify({ issuer, audience: access.aud, jwks: interpolate`${issuer}/cdn-cgi/access/certs`, logout: '/cdn-cgi/access/logout' } satisfies Record<keyof typeof CloudflareAccess.Encoded, Input<string>>) },
        ],
        deploy: true,
    });
    const zones = yield* Effect.cached(Effect.tryPromise({ try: () => getZones({ account: { id: accountId } }), catch: (cause) => new Cause.UnknownError(cause, `zones of account ${accountId} do not list`) }));
    const routes = yield* Effect.forEach(
        production ? triggered('fetch') : [],
        Effect.fnUntraced(function* ({ pattern, zone }: Extract<Trigger, { readonly type: 'fetch' }>) {
            const host = Array.headNonEmpty(String.split(pattern, '/')).replaceAll('*', '');
            const owners = (yield* zones).results.filter((owner) => (zone === undefined ? host === owner.name || host.endsWith(`.${owner.name}`) : zone === owner.name));
            const { id } = yield* Array.match(owners, { onEmpty: () => refuse(`route ${pattern} outside every zone of the account`), onNonEmpty: Function.flow(Array.max(Order.mapInput(Order.Number, (owner: (typeof owners)[number]) => owner.name.length)), Effect.succeed) });
            return new WorkersRoute(`${name}-${pattern}`, { zoneId: id, pattern, script: worker.name }, { dependsOn: [version] }).pattern;
        }),
    );
    const consumers = yield* Effect.forEach(
        triggered('queue'),
        Effect.fnUntraced(function* ({ name: queue, deadLetterQueue, maxBatchSize, maxBatchTimeout, maxConcurrency, maxRetries, retryDelay, visibilityTimeoutMs }: Extract<Trigger, { readonly type: 'queue' }>) {
            const { queueId } = yield* Cache.get(queues, queue);
            const settings = { batchSize: maxBatchSize, maxConcurrency: maxConcurrency ?? undefined, maxRetries, maxWaitTimeMs: maxBatchTimeout === undefined ? undefined : Duration.toMillis(Duration.seconds(maxBatchTimeout)), retryDelay, visibilityTimeoutMs };
            return new QueueConsumer(`${name}-${queue}`, { accountId, queueId, scriptName: worker.name, type: 'worker', deadLetterQueue, settings }, { dependsOn: [version] }).queueName;
        }),
    );
    return {
        url: worker.subdomain.url,
        version: version.id,
        domains: hostnames.map((hostname) => new WorkersCustomDomain(`${name}-${hostname}`, { accountId, hostname, service: worker.name }, { dependsOn: [version] }).hostname),
        routes,
        consumers,
        schedules: new WorkersCronTrigger(name, { accountId, scriptName: worker.name, schedules: triggered('scheduled').map(({ schedule }) => ({ cron: schedule })) }, { dependsOn: [version] }).schedules,
    };
};
const docker = function* ({ root, workers }: BuildOutput, { name, hostnames, production, named }: Stack) {
    const [path, assets, email, resolved] = yield* Effect.all([
        Path.Path,
        Effect.fromNullishOr(workers.default.assetsDir).pipe(Effect.mapError(() => new Cause.NoSuchElementError(`worker ${workers.default.config.name} has no assets for the Node server`))),
        Config.String('OWNER_EMAIL'),
        Effect.tryPromise({ try: () => resolveConfig({ root }, 'build', 'production'), catch: (cause) => new Cause.UnknownError(cause, `Vite configuration at ${root} does not resolve`) }),
    ]);
    const { build } = yield* Effect.fromNullishOr(resolved.environments[vite.layout.environment]).pipe(Effect.mapError(() => new Cause.NoSuchElementError(`Vite configuration at ${root} has no ${vite.layout.environment} environment`)));
    const env = workers.default.config.env ?? {};
    const port = 8080;
    const domains = Array.match(hostnames, { onEmpty: () => [`${name}.localhost`], onNonEmpty: Function.identity });
    const [internal, edge] = [new Network(name, { name, internal: true }), new Network(`${name}-edge`, { name: `${name}-edge` })];
    const volume = (resource: string, containerPath: string) => ({ volumeName: new Volume(resource, { name: resource }, { protect: production }).name, containerPath });
    const pull = (resource: string, reference: string) => new RemoteImage(resource, { name: reference, keepLocally: true, pullTriggers: [getRegistryImageOutput({ name: reference }).sha256Digest] }).imageId;
    const container = (resource: string, image: Input<string>, args: Omit<ContainerArgs, 'name' | 'image' | 'networksAdvanced' | 'restart'>) =>
        new Container(resource, { name: resource, image, networksAdvanced: [{ name: internal.name }, ...(args.ports === undefined ? [] : [{ name: edge.name }])], restart: 'unless-stopped', ...args });
    const reachable = (resource: string, reference: string, { listen, probe, ...args }: Parameters<typeof container>[2] & { readonly listen: number; readonly probe: (listen: number) => readonly string[] }) => {
        const address = { host: '127.0.0.1', port: new RandomInteger(`${resource}-port`, { min: 49_152, max: 65_535 }).result };
        const server = container(resource, pull(resource, reference), { ...args, ports: [{ internal: listen, external: address.port, ip: address.host }], healthcheck: { tests: ['CMD', ...probe(listen)], interval: '2s', retries: 30 }, wait: true });
        return { address, server, endpoint: interpolate`${server.name}:${listen}` };
    };
    const declared = Record.filterMap(env, (binding, variable) =>
        Match.value(binding).pipe(
            Match.when({ type: 'r2' }, ({ name: bucket = named(variable) }) => bucket),
            Match.result,
        ),
    );
    const store = Option.map(Option.liftPredicate(declared, Predicate.not(Record.isEmptyRecord)), (buckets) => {
        const [user, key, region] = [password(`${name}-store-access`).result, password(`${name}-store-secret`).result, 'us-east-1'];
        const { address, server, endpoint } = reachable(`${name}-store`, 'rustfs/rustfs:latest', {
            listen: 9000,
            probe: (listen) => ['curl', '-fsS', `http://127.0.0.1:${listen}/health/ready`],
            envs: [interpolate`RUSTFS_ACCESS_KEY=${user}`, interpolate`RUSTFS_SECRET_KEY=${key}`, `RUSTFS_REGION=${region}`, 'RUSTFS_CONSOLE_ENABLE=false'],
            volumes: [volume(`${name}-store`, '/data')],
        });
        const provider = new Provider(`${name}-store`, { minioServer: interpolate`${address.host}:${address.port}`, minioUser: user, minioPassword: key, minioRegion: region }, { dependsOn: [server] });
        const created = Record.map(buckets, (bucket, variable) => new S3Bucket(named(variable), { bucket, forceDestroy: !production }, { provider, protect: production }));
        const policy = new IamPolicy(name, { name, policy: jsonStringify({ Version: '2012-10-17', Statement: [{ Effect: 'Allow', Action: ['s3:*'], Resource: Object.values(created).flatMap(({ arn }) => [arn, interpolate`${arn}/*`]) }] }) }, { provider });
        const account = new IamUser(name, { name, secret: password(`${name}-store-user`).result }, { provider });
        return {
            envs: { ...Record.map(created, Struct.get('bucket')), AWS_ENDPOINT_URL_S3: interpolate`http://${endpoint}`, AWS_REGION: region, AWS_ACCESS_KEY_ID: new IamUserPolicyAttachment(name, { userName: account.name, policyName: policy.name }, { provider }).userName, AWS_SECRET_ACCESS_KEY: account.secret },
            volumes: [],
        };
    });
    const services = yield* Effect.forEach(Record.toEntries(env), ([variable, binding]) =>
        Match.value(binding).pipe(
            Match.whenOr({ type: 'assets' }, { type: 'r2' }, () => Effect.succeed({ envs: {}, volumes: [] })),
            Match.discriminators('type')({
                d1: () => Effect.succeed({ envs: { [variable]: `/data/${variable}/database.sqlite` }, volumes: [volume(named(variable), `/data/${variable}`)] }),
                hyperdrive: () => {
                    const database = named(variable);
                    const { result } = password(database);
                    const { address, server, endpoint } = reachable(database, 'postgres:latest', {
                        listen: 5432,
                        probe: () => ['pg_isready', '-U', 'postgres'],
                        command: ['postgres', '-c', 'shared_preload_libraries=pg_stat_statements'],
                        envs: [interpolate`POSTGRES_PASSWORD=${result}`, 'POSTGRES_INITDB_ARGS=--locale-provider=builtin --builtin-locale=PG_UNICODE_FAST'],
                        volumes: [volume(database, '/var/lib/postgresql')],
                    });
                    const urls = Record.map(postgres(database, { ...address, database: 'postgres', username: 'postgres', password: result, sslmode: 'disable' }, production, { dependsOn: server }), ({ user, password: key }) => interpolate`postgres://${user}:${key}@${endpoint}/postgres`);
                    return Effect.succeed({ envs: { [variable]: urls.runtime, [migrator(variable)]: urls.migrator }, volumes: [] });
                },
                json: ({ value }) => Effect.succeed({ envs: { [variable]: jsonStringify(value) }, volumes: [] }),
                secret: () => Effect.map(Config.Redacted(variable), (value) => ({ envs: { [variable]: secret(Redacted.value(value)) }, volumes: [] })),
                text: ({ value }) => Effect.succeed({ envs: { [variable]: value }, volumes: [] }),
            }),
            Match.orElse(({ type }) => refuse(`binding ${variable} of type ${type}`)),
        ),
    );
    const parts = [...services, ...Option.toArray(store)];
    const app = container(
        name,
        new Image(name, {
            push: false,
            load: true,
            exec: true,
            buildOnPreview: false,
            tags: [`${name}:latest`],
            context: { location: path.resolve(resolved.root, build.outDir), named: { assets: { location: assets } } },
            dockerfile: { inline: [`FROM node:${process.versions.node}-slim`, 'WORKDIR /app', `COPY --from=assets . ${vite.layout.assets}`, 'COPY . .', `ENV ${vite.layout.portVariable}=${port}`, `CMD ["node", "${vite.layout.entry}.js"]`].join('\n') },
        }).digest,
        {
            envs: parts.flatMap(({ envs }) => Record.collect(envs, (variable, value: Input<string>) => interpolate`${variable}=${value}`)),
            volumes: parts.flatMap(({ volumes }) => volumes),
        },
    );
    const owner = password(`${name}-owner`);
    const identity = container(`${name}-identity`, pull(`${name}-identity`, 'authelia/authelia:latest'), {
        uploads: Record.collect(
            {
                '/config/configuration.yml': {
                    authentication_backend: { password_reset: { disable: true }, file: { path: '/config/users.yml' } },
                    access_control: {
                        default_policy: 'bypass',
                        rules: [
                            { domain: domains, resources: [`^${sites.paths.signin}([?].*)?$`], policy: 'one_factor' },
                            { domain: domains, methods: ['POST', 'PUT', 'PATCH', 'DELETE'], policy: 'one_factor' },
                        ],
                    },
                    session: { secret: password(`${name}-identity-session`).result, cookies: domains.map((site) => ({ domain: site, authelia_url: `https://auth.${site}`, default_redirection_url: `https://${site}/` })) },
                    storage: { encryption_key: password(`${name}-identity-storage`).result, local: { path: '/var/lib/authelia/db.sqlite3' } },
                    notifier: { filesystem: { filename: '/var/lib/authelia/notification.txt' } },
                    ntp: { disable_startup_check: true },
                },
                '/config/users.yml': { users: { owner: { displayname: 'Owner', email, password: owner.bcryptHash } } },
            },
            (file, document) => ({ file, content: jsonStringify(document) }),
        ),
        volumes: [volume(`${name}-identity`, '/var/lib/authelia')],
        wait: true,
    });
    const authelia = { handler: 'reverse_proxy', upstreams: [{ dial: interpolate`${identity.name}:9091` }] };
    const copies = Record.collect(
        Record.mapKeys({ 'Remote-User': sites.headers.id, 'Remote-Email': sites.headers.email }, (answer) => `{http.reverse_proxy.header.${answer}}`),
        (answer, header) => ({
            match: [{ vars_regexp: { [answer]: { pattern: '.' } } }],
            handle: [{ handler: 'headers', request: { set: { [header]: [answer] } } }],
        }),
    );
    const redirects = Record.collect(
        {
            signin: { source: `{http.request.uri.query.${sites.returnTo}}`, input_regexp: '^(/.*)$', location: 'https://{http.request.host}{target}' },
            signout: { source: '{http.request.uri.query}', input_regexp: `(?:^|&)${sites.returnTo}=((?:/|%2[Ff])[^&]*)`, location: 'https://auth.{http.request.host}/logout?rd=https://{http.request.host}{target}' },
        },
        (action, { source, input_regexp, location }) => ({
            match: [{ path: [sites.paths[action]] }],
            handle: [
                { handler: 'map', source, destinations: ['{target}'], mappings: [{ input_regexp, outputs: ['$1'] }], defaults: ['/'] },
                { handler: 'static_response', status_code: 302, headers: { Location: [location] } },
            ],
        }),
    );
    const routes = [
        { match: [{ host: domains.map((site) => `auth.${site}`) }], handle: [authelia] },
        {
            match: [{ host: domains }],
            handle: [
                {
                    ...authelia,
                    rewrite: { method: 'GET', uri: '/api/authz/forward-auth' },
                    headers: { request: { set: { 'X-Forwarded-Method': ['{http.request.method}'], 'X-Forwarded-Uri': ['{http.request.uri}'] } } },
                    handle_response: [{ match: { status_code: [2] }, routes: [{ handle: [{ handler: 'headers', request: { delete: Object.values(sites.headers) } }] }, ...copies] }],
                },
                { handler: 'subroute', routes: redirects },
                { handler: 'reverse_proxy', upstreams: [{ dial: interpolate`${app.name}:${port}` }] },
            ],
        },
    ];
    const config = { file: '/etc/caddy/caddy.json', content: jsonStringify({ admin: { disabled: true }, apps: { http: { servers: { [name]: { listen: [':443'], routes } } } } }) };
    const proxy = container(`${name}-proxy`, pull(`${name}-proxy`, 'caddy:latest'), {
        command: ['caddy', 'run', '--config', config.file],
        uploads: [config],
        volumes: [volume(`${name}-proxy`, '/data')],
        ports: [{ internal: 80 }, { internal: 443 }, { internal: 443, protocol: 'udp' }].map((published) => ({ ...published, external: published.internal })),
    });
    return { urls: domains.map((site) => `https://${site}`), proxy: proxy.name, ownerPassword: owner.result };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const hostingTargets = { cloudflare: Effect.fnUntraced(cloudflare), docker: Effect.fnUntraced(docker) };
const hosting = Effect.fn('hosting')(function* (directory: string, target: keyof typeof hostingTargets, disposable: boolean) {
    const build = yield* Effect.tryPromise({ try: () => readBuildOutput(directory), catch: (cause) => new Cause.UnknownError(cause, `build output at ${directory} is unreadable`) });
    yield* Effect.forEach([...Record.keys(Record.remove(build.workers, 'default')).map((worker) => `worker ${worker}`), ...build.containers.map(({ configPath }) => `container at ${configPath}`)], refuse);
    const { config } = build.workers.default;
    const name = disposable ? `${config.name}-disposable` : config.name;
    const domains = (config.domains ?? []).map((domain) => ({ domain, disposable }));
    const hostnames = Array.filterMap(
        domains,
        Match.type<(typeof domains)[number]>().pipe(
            Match.when({ domain: Match.string, disposable: false }, ({ domain: hostname }) => hostname),
            Match.when({ domain: { production: true }, disposable: false }, ({ domain: { hostname } }) => hostname),
            Match.when({ domain: { previews: true }, disposable: true }, ({ domain: { hostname } }) => `${name}.${hostname}`),
            Match.result,
        ),
    );
    return {
        projectName: 'rasm-hosting',
        stackName: `${name}.${target}`,
        program: hostingTargets[target](build, { name, hostnames, production: !disposable, named: (variable) => `${name}-${variable}`.toLowerCase() }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { hosting, hostingTargets };
