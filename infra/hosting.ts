import process from 'node:process';
import { type BuildOutput, readBuildOutput } from '@cloudflare/build-output-utils';
import {
    D1Database,
    type GetZonesResult,
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
import { Extension, Grant, Provider as PostgresProvider, type ProviderArgs, Role, type RoleArgs, Schema } from '@pulumi/postgresql';
import { type CustomResourceOptions, type Input, interpolate, jsonStringify, secret } from '@pulumi/pulumi';
import { RandomInteger, RandomPassword } from '@pulumi/random';
import { Access } from '@rasm/hosting/cloudflare';
import { layout } from '@rasm/hosting/node/vite';
import { User } from '@rasm/hosting/user';
import { Array, Cache, Cause, Config, Duration, Effect, Function, Match, Order, Path, Predicate, Record, Redacted } from 'effect';
import { resolveConfig } from 'vite';

// --- [TYPES] ---------------------------------------------------------------------------

type Trigger = NonNullable<BuildOutput['workers']['default']['config']['triggers']>[number];
interface Stack {
    readonly name: string;
    readonly hostnames: readonly string[];
    readonly production: boolean;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const refuse = (subject: string) => Effect.fail(new Cause.IllegalArgumentError(`${subject} has no hosting form`));
const triggered = (triggers: readonly Trigger[]) =>
    Effect.map(
        Effect.forEach(triggers, (trigger) =>
            Match.value(trigger).pipe(
                Match.discriminators('type')({ fetch: Effect.succeed, queue: Effect.succeed, scheduled: Effect.succeed }),
                Match.orElse(({ type }) => refuse(`trigger of type ${type}`)),
            ),
        ),
        (supported) => ({
            routes: Array.filterMap(
                supported,
                Match.type<Trigger>().pipe(
                    Match.when({ type: 'fetch' }, ({ pattern, zone }) => ({ pattern, zone })),
                    Match.result,
                ),
            ),
            consumers: Array.filterMap(
                supported,
                Match.type<Trigger>().pipe(
                    Match.when({ type: 'queue' }, ({ name, deadLetterQueue, maxBatchSize, maxBatchTimeout, maxConcurrency, maxRetries, retryDelay, visibilityTimeoutMs }) => ({
                        queue: name,
                        deadLetterQueue,
                        settings: { batchSize: maxBatchSize, maxConcurrency: maxConcurrency ?? undefined, maxRetries, maxWaitTimeMs: maxBatchTimeout === undefined ? undefined : Duration.toMillis(Duration.seconds(maxBatchTimeout)), retryDelay, visibilityTimeoutMs },
                    })),
                    Match.result,
                ),
            ),
            schedules: Array.filterMap(
                supported,
                Match.type<Trigger>().pipe(
                    Match.when({ type: 'scheduled' }, ({ schedule }) => ({ cron: schedule })),
                    Match.result,
                ),
            ),
        }),
    );
const zoned = Effect.fnUntraced(function* (zones: Effect.Effect<GetZonesResult, Cause.UnknownError>, { pattern, zone }: Omit<Extract<Trigger, { readonly type: 'fetch' }>, 'type'>) {
    const { results } = yield* zones;
    const host = (pattern.split('/', 1)[0] ?? pattern).replaceAll('*', '');
    const owners = results.filter(({ name }) => (zone === undefined ? host === name || host.endsWith(`.${name}`) : zone === name));
    return yield* Array.match(owners, {
        onEmpty: () => refuse(`route ${pattern} outside every zone of the account`),
        onNonEmpty: (matched) =>
            Effect.succeed(
                Array.max(
                    matched,
                    Order.mapInput(Order.Number, ({ name }: { readonly name: string }) => name.length),
                ).id,
            ),
    });
});
const migrator = (variable: string) => `${variable}_MIGRATOR`;
const password = (resource: string) => new RandomPassword(resource, { length: 40, special: false });
const postgres = (resource: string, server: ProviderArgs & { readonly database: string }, protect: boolean, options?: CustomResourceOptions) => {
    const provider = new PostgresProvider(resource, server, options);
    const login = (role: string, settings: RoleArgs) => {
        const { result } = password(`${resource}-${role}`);
        return { name: new Role(`${resource}-${role}`, { name: `${resource}-${role}`, login: true, password: result, ...settings }, { provider }).name, password: result };
    };
    const owner = login('migrator', { idleInTransactionSessionTimeout: 60_000 });
    const schema = new Schema(resource, { name: owner.name, owner: owner.name, database: server.database }, { provider, protect });
    const runtime = login('runtime', { statementTimeout: 15_000, idleInTransactionSessionTimeout: 30_000, searchPaths: [schema.name] });
    const statistics = new Extension(`${resource}-pg-stat-statements`, { name: 'pg_stat_statements', database: server.database }, { provider });
    return {
        migrator: { user: new Grant(`${resource}-migrator`, { database: server.database, role: schema.owner, objectType: 'database', privileges: ['CREATE'] }, { provider, dependsOn: statistics }).role, password: owner.password },
        runtime: { user: new Grant(`${resource}-runtime`, { database: server.database, role: runtime.name, objectType: 'schema', schema: schema.name, privileges: ['USAGE'] }, { provider }).role, password: runtime.password },
    };
};
const targets = {
    cloudflare: Effect.fnUntraced(function* ({ rootConfig, workers }: BuildOutput, { name, hostnames, production }: Stack) {
        const { config, bundleDir, assetsDir } = workers.default;
        const [path, accountId] = yield* Effect.all([Path.Path, rootConfig.accountId === undefined ? Config.String('CLOUDFLARE_ACCOUNT_ID') : Config.succeed(rootConfig.accountId)]);
        const triggers = yield* triggered(config.triggers ?? []);
        const tails = yield* Effect.forEach(config.tailConsumers ?? [], (tail) => (tail.streaming === true ? refuse(`streaming tail consumer ${tail.worker}`) : Effect.succeed({ name: tail.worker })));
        const placement = yield* Match.value(config.placement).pipe(
            Match.whenOr(Predicate.isUndefined, { mode: 'off' }, () => Effect.undefined),
            Match.when({ mode: 'smart', hint: Match.string }, ({ hint }) => refuse(`placement hint ${hint}`)),
            Match.when({ mode: 'smart' }, () => Effect.succeed({ mode: 'smart' })),
            Match.orElse((targeted) => Effect.succeed({ ...targeted, mode: 'targeted' })),
        );
        const queues = yield* Cache.make({ capacity: Number.MAX_SAFE_INTEGER, lookup: (queueName: string) => Effect.sync(() => new Queue(queueName, { accountId, queueName }, { protect: production })) });
        const bindings = yield* Effect.forEach(Record.toEntries(config.env ?? {}), ([variable, binding]) => {
            const resource = `${name}-${variable}`.toLowerCase();
            return Match.value(binding).pipe(
                Match.whenOr({ type: 'ai' }, { type: 'assets' }, { type: 'browser' }, { type: 'images' }, { type: 'media' }, { type: 'version-metadata' }, ({ type }) => Effect.succeed([{ name: variable, type: type.replaceAll('-', '_') }])),
                Match.when({ type: 'worker', props: Predicate.isObject }, () => refuse(`service binding ${variable} with props`)),
                Match.when({ type: 'queue', deliveryDelay: Match.number }, ({ deliveryDelay }) => refuse(`queue binding ${variable} with delivery delay ${deliveryDelay}`)),
                Match.discriminators('type')({
                    'analytics-engine-dataset': ({ name: dataset = resource }) => Effect.succeed([{ name: variable, type: 'analytics_engine', dataset }]),
                    d1: ({ id, name: database = resource }) => Effect.succeed([{ name: variable, type: 'd1', id: id ?? new D1Database(resource, { accountId, name: database }, { protect: production }).uuid }]),
                    hyperdrive: () =>
                        Effect.map(Config.URL(variable), (url) => {
                            const [database, port] = [url.pathname.slice(1), url.port === '' ? undefined : Number(url.port)];
                            const { migrator: owner, runtime } = postgres(resource, { host: url.hostname, port, database, username: decodeURIComponent(url.username), password: secret(decodeURIComponent(url.password)) }, production);
                            return [
                                {
                                    name: variable,
                                    type: 'hyperdrive',
                                    id: new HyperdriveConfig(resource, { accountId, name: resource, origin: { scheme: url.protocol.slice(0, -1), host: url.hostname, port, database, user: runtime.user, password: runtime.password }, caching: { disabled: true } }).id,
                                },
                                { name: migrator(variable), type: 'secret_text', text: interpolate`${url.protocol}//${owner.user}:${owner.password}@${url.host}${url.pathname}${url.search}` },
                            ];
                        }),
                    json: ({ value }) => Effect.succeed([{ name: variable, type: 'json', json: jsonStringify(value) }]),
                    kv: ({ id }) => Effect.succeed([{ name: variable, type: 'kv_namespace', namespaceId: id ?? new WorkersKvNamespace(resource, { accountId, title: resource }, { protect: production }).id }]),
                    'mtls-certificate': ({ id: certificateId }) => Effect.succeed([{ name: variable, type: 'mtls_certificate', certificateId }]),
                    pipeline: ({ name: pipeline }) => Effect.succeed([{ name: variable, type: 'pipelines', pipeline }]),
                    queue: ({ name: queueName = resource }) => Effect.map(Cache.get(queues, queueName), (queue) => [{ name: variable, type: 'queue', queueName: queue.queueName }]),
                    r2: (bucket) => Effect.succeed([{ name: variable, type: 'r2_bucket', bucketName: new R2Bucket(resource, { accountId, name: bucket.name ?? resource, jurisdiction: bucket.jurisdiction }, { protect: production }).name, jurisdiction: bucket.jurisdiction }]),
                    'rate-limit': ({ namespace: namespaceId, simple }) => Effect.succeed([{ name: variable, type: 'ratelimit', namespaceId, simple }]),
                    secret: () => Effect.map(Config.Redacted(variable), (value) => [{ name: variable, type: 'secret_text', text: secret(Redacted.value(value)) }]),
                    'secrets-store-secret': ({ storeId, secretName }) => Effect.succeed([{ name: variable, type: 'secrets_store_secret', storeId, secretName }]),
                    'send-email': ({ destinationAddress, allowedDestinationAddresses, allowedSenderAddresses }) => Effect.succeed([{ name: variable, type: 'send_email', destinationAddress, allowedDestinationAddresses, allowedSenderAddresses }]),
                    text: ({ value }) => Effect.succeed([{ name: variable, type: 'plain_text', text: value }]),
                    vectorize: ({ name: indexName }) => Effect.succeed([{ name: variable, type: 'vectorize', indexName }]),
                    'vpc-network': ({ tunnelId, networkId }) => Effect.succeed([{ name: variable, type: 'vpc_network', tunnelId, networkId }]),
                    'vpc-service': ({ id: serviceId }) => Effect.succeed([{ name: variable, type: 'vpc_service', serviceId }]),
                    worker: ({ worker: service, exportName: entrypoint }) => Effect.succeed([{ name: variable, type: 'service', service, entrypoint }]),
                    workflow: ({ name: workflowName, worker: scriptName, exportName: className }) => Effect.succeed([{ name: variable, type: 'workflow', workflowName, scriptName, className }]),
                }),
                Match.orElse(({ type }) => refuse(`binding ${variable} of type ${type}`)),
            );
        });
        const worker = new Worker(name, { accountId, name, logpush: config.logpush, observability: config.observability, tailConsumers: tails, subdomain: { enabled: config.workersDev ?? true, previewsEnabled: config.previewUrls } });
        const pin = new ZeroTrustAccessIdentityProvider(name, { accountId, name, type: 'onetimepin', config: {} });
        const access = new ZeroTrustAccessApplication(name, {
            accountId,
            name,
            type: 'self_hosted',
            destinations: worker.subdomain.apply(({ enabled, url }) => [...hostnames, ...(enabled ? [new URL(url).host] : [])].map((host) => ({ type: 'public', uri: `${host}${User.sites.paths.signin}` }))),
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
            bindings: [...bindings.flat(), { name: Access.binding, type: 'json', json: jsonStringify({ issuer, audience: access.aud, keys: interpolate`${issuer}/cdn-cgi/access/certs`, logout: '/cdn-cgi/access/logout' } satisfies Record<keyof typeof Access.Encoded, Input<string>>) }],
            deploy: true,
        });
        const zones = yield* Effect.cached(Effect.tryPromise({ try: () => getZones({ account: { id: accountId } }), catch: (cause) => new Cause.UnknownError(cause, `zones of account ${accountId} do not list`) }));
        const routes = yield* Effect.forEach(production ? triggers.routes : [], (route) => Effect.map(zoned(zones, route), (zoneId) => new WorkersRoute(`${name}-${route.pattern}`, { zoneId, pattern: route.pattern, script: worker.name }, { dependsOn: [version] }).pattern));
        const consumers = yield* Effect.forEach(triggers.consumers, (consumer) =>
            Effect.map(Cache.get(queues, consumer.queue), ({ queueId }) => new QueueConsumer(`${name}-${consumer.queue}`, { accountId, queueId, scriptName: worker.name, type: 'worker', deadLetterQueue: consumer.deadLetterQueue, settings: consumer.settings }, { dependsOn: [version] }).queueName),
        );
        return {
            url: worker.subdomain.url,
            version: version.id,
            domains: hostnames.map((hostname) => new WorkersCustomDomain(`${name}-${hostname}`, { accountId, hostname, service: worker.name }, { dependsOn: [version] }).hostname),
            routes,
            consumers,
            schedules: new WorkersCronTrigger(name, { accountId, scriptName: worker.name, schedules: triggers.schedules }, { dependsOn: [version] }).schedules,
        };
    }),
    docker: Effect.fnUntraced(function* ({ root, workers }: BuildOutput, { name, hostnames, production }: Stack) {
        const [path, assets, email, resolved] = yield* Effect.all([
            Path.Path,
            Effect.fromNullishOr(workers.default.assetsDir).pipe(Effect.mapError(() => new Cause.NoSuchElementError(`worker ${workers.default.config.name} has no assets for the Node server`))),
            Config.String('OWNER_EMAIL'),
            Effect.tryPromise({ try: () => resolveConfig({ root }, 'build', 'production'), catch: (cause) => new Cause.UnknownError(cause, `Vite configuration at ${root} does not resolve`) }),
        ]);
        const { build } = yield* Effect.fromNullishOr(resolved.environments[layout.environment]).pipe(Effect.mapError(() => new Cause.NoSuchElementError(`Vite configuration at ${root} has no ${layout.environment} environment`)));
        const env = workers.default.config.env ?? {};
        const port = 8080;
        const sites = Array.match(hostnames, { onEmpty: () => [`${name}.localhost`], onNonEmpty: Function.identity });
        const [internal, edge] = [new Network(name, { name, internal: true }), new Network(`${name}-edge`, { name: `${name}-edge` })];
        const volume = (resource: string, containerPath: string) => ({ volumeName: new Volume(resource, { name: resource }, { protect: production }).name, containerPath });
        const pull = (resource: string, reference: string) => new RemoteImage(resource, { name: reference, keepLocally: true, pullTriggers: [getRegistryImageOutput({ name: reference }).sha256Digest] }).imageId;
        const container = (resource: string, image: Input<string>, args: Omit<ContainerArgs, 'name' | 'image' | 'networksAdvanced' | 'restart'>) =>
            new Container(resource, { name: resource, image, networksAdvanced: [{ name: internal.name }, ...(args.ports === undefined ? [] : [{ name: edge.name }])], restart: 'unless-stopped', ...args });
        const reachable = (resource: string, reference: string, { listen, test, ...args }: Parameters<typeof container>[2] & { readonly listen: number; readonly test: readonly string[] }) => {
            const address = { host: '127.0.0.1', port: new RandomInteger(`${resource}-port`, { min: 49_152, max: 65_535 }).result };
            return { address, server: container(resource, pull(resource, reference), { ...args, ports: [{ internal: listen, external: address.port, ip: address.host }], healthcheck: { tests: ['CMD', ...test], interval: '2s', retries: 30 }, wait: true }) };
        };
        const named = (variable: string) => `${name}-${variable}`.toLowerCase();
        const declared = Record.filterMap(env, (binding, variable) =>
            Match.value(binding).pipe(
                Match.when({ type: 'r2' }, ({ name: bucket = named(variable) }) => bucket),
                Match.result,
            ),
        );
        const store = yield* Effect.cached(
            Effect.sync(() => {
                const [user, key, region] = [password(`${name}-store-access`).result, password(`${name}-store-secret`).result, 'us-east-1'];
                const { address, server } = reachable(`${name}-store`, 'rustfs/rustfs:latest', {
                    listen: 9000,
                    test: ['curl', '-fsS', 'http://127.0.0.1:9000/health/ready'],
                    envs: [interpolate`RUSTFS_ACCESS_KEY=${user}`, interpolate`RUSTFS_SECRET_KEY=${key}`, `RUSTFS_REGION=${region}`, 'RUSTFS_CONSOLE_ENABLE=false'],
                    volumes: [volume(`${name}-store`, '/data')],
                });
                const provider = new Provider(`${name}-store`, { minioServer: interpolate`${address.host}:${address.port}`, minioUser: user, minioPassword: key, minioRegion: region }, { dependsOn: [server] });
                const buckets = Record.map(declared, (bucket, variable) => new S3Bucket(named(variable), { bucket, forceDestroy: !production }, { provider, protect: production }).bucket);
                const policy = new IamPolicy(name, { name, policy: jsonStringify({ Version: '2012-10-17', Statement: [{ Effect: 'Allow', Action: ['s3:*'], Resource: Object.values(buckets).flatMap((bucket) => [interpolate`arn:aws:s3:::${bucket}`, interpolate`arn:aws:s3:::${bucket}/*`]) }] }) }, { provider });
                const account = new IamUser(name, { name, secret: password(`${name}-store-user`).result }, { provider });
                return { envs: { ...buckets, AWS_ENDPOINT_URL_S3: interpolate`http://${server.name}:9000`, AWS_REGION: region, AWS_ACCESS_KEY_ID: new IamUserPolicyAttachment(name, { userName: account.name, policyName: policy.name }, { provider }).userName, AWS_SECRET_ACCESS_KEY: account.secret }, volumes: [] };
            }),
        );
        const services = yield* Effect.forEach(Record.toEntries(env), ([variable, binding]) =>
            Match.value(binding).pipe(
                Match.discriminators('type')({
                    assets: () => Effect.succeed({ envs: {}, volumes: [] }),
                    d1: () => Effect.succeed({ envs: { [variable]: `/data/${variable}/database.sqlite` }, volumes: [volume(named(variable), `/data/${variable}`)] }),
                    hyperdrive: () => {
                        const database = named(variable);
                        const { result } = password(database);
                        const { address, server } = reachable(database, 'postgres:latest', {
                            listen: 5432,
                            test: ['pg_isready', '-U', 'postgres'],
                            command: ['postgres', '-c', 'shared_preload_libraries=pg_stat_statements'],
                            envs: [interpolate`POSTGRES_PASSWORD=${result}`, 'POSTGRES_INITDB_ARGS=--locale-provider=builtin --builtin-locale=PG_UNICODE_FAST'],
                            volumes: [volume(database, '/var/lib/postgresql')],
                        });
                        const urls = Record.map(postgres(database, { ...address, database: 'postgres', username: 'postgres', password: result, sslmode: 'disable' }, production, { dependsOn: server }), ({ user, password: key }) => interpolate`postgres://${user}:${key}@${server.name}:5432/postgres`);
                        return Effect.succeed({ envs: { [variable]: urls.runtime, [migrator(variable)]: urls.migrator }, volumes: [] });
                    },
                    json: ({ value }) => Effect.succeed({ envs: { [variable]: jsonStringify(value) }, volumes: [] }),
                    r2: () => store,
                    secret: () => Effect.map(Config.Redacted(variable), (value) => ({ envs: { [variable]: secret(Redacted.value(value)) }, volumes: [] })),
                    text: ({ value }) => Effect.succeed({ envs: { [variable]: value }, volumes: [] }),
                }),
                Match.orElse(({ type }) => refuse(`binding ${variable} of type ${type}`)),
            ),
        );
        const app = container(
            name,
            new Image(name, {
                push: false,
                load: true,
                exec: true,
                buildOnPreview: false,
                tags: [`${name}:latest`],
                context: { location: path.resolve(resolved.root, build.outDir), named: { assets: { location: assets } } },
                dockerfile: { inline: [`FROM node:${process.versions.node}-slim`, 'WORKDIR /app', `COPY --from=assets . ${layout.assets}`, 'COPY . .', `ENV ${layout.port}=${port}`, `CMD ["node", "${layout.entry}.js"]`].join('\n') },
            }).digest,
            {
                envs: [...new Map(services.flatMap(({ envs }) => Record.toEntries<string, Input<string>>(envs)))].map(([variable, value]) => interpolate`${variable}=${value}`),
                volumes: services.flatMap(({ volumes }) => volumes),
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
                                { domain: sites, resources: [`^${User.sites.paths.signin}([?].*)?$`], policy: 'one_factor' },
                                { domain: sites, methods: ['POST', 'PUT', 'PATCH', 'DELETE'], policy: 'one_factor' },
                            ],
                        },
                        session: { secret: password(`${name}-identity-session`).result, cookies: sites.map((site) => ({ domain: site, authelia_url: `https://auth.${site}`, default_redirection_url: `https://${site}/` })) },
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
            Record.mapKeys({ 'Remote-User': User.sites.headers.id, 'Remote-Email': User.sites.headers.email }, (answer) => `{http.reverse_proxy.header.${answer}}`),
            (answer, header) => ({
                match: [{ vars_regexp: { [answer]: { pattern: '.' } } }],
                handle: [{ handler: 'headers', request: { set: { [header]: [answer] } } }],
            }),
        );
        const redirects = Record.collect(
            {
                signin: { source: `{http.request.uri.query.${User.sites.returnTo}}`, input_regexp: '^(/.*)$', location: 'https://{http.request.host}{target}' },
                signout: { source: '{http.request.uri.query}', input_regexp: `(?:^|&)${User.sites.returnTo}=((?:/|%2[Ff])[^&]*)`, location: 'https://auth.{http.request.host}/logout?rd=https://{http.request.host}{target}' },
            },
            (action, { source, input_regexp, location }) => ({
                match: [{ path: [User.sites.paths[action]] }],
                handle: [
                    { handler: 'map', source, destinations: ['{target}'], mappings: [{ input_regexp, outputs: ['$1'] }], defaults: ['/'] },
                    { handler: 'static_response', status_code: 302, headers: { Location: [location] } },
                ],
            }),
        );
        const routes = [
            { match: [{ host: sites.map((site) => `auth.${site}`) }], handle: [authelia] },
            {
                match: [{ host: sites }],
                handle: [
                    {
                        ...authelia,
                        rewrite: { method: 'GET', uri: '/api/authz/forward-auth' },
                        headers: { request: { set: { 'X-Forwarded-Method': ['{http.request.method}'], 'X-Forwarded-Uri': ['{http.request.uri}'] } } },
                        handle_response: [{ match: { status_code: [2] }, routes: [{ handle: [{ handler: 'headers', request: { delete: Object.values(User.sites.headers) } }] }, ...copies] }],
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
        return { urls: sites.map((site) => `https://${site}`), proxy: proxy.name, ownerPassword: owner.result };
    }),
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const hosting = Effect.fn('hosting')(function* (directory: string, target: keyof typeof targets, disposable: boolean) {
    const [build, context] = yield* Effect.all([Effect.tryPromise({ try: () => readBuildOutput(directory), catch: (cause) => new Cause.UnknownError(cause, `build output at ${directory} is unreadable`) }), Effect.context<Path.Path>()]);
    const { config } = build.workers.default;
    const name = disposable ? `${config.name}-disposable` : config.name;
    const hostnames = (config.domains ?? []).flatMap((domain) =>
        Match.value({ domain, disposable }).pipe(
            Match.when({ domain: Match.string, disposable: false }, ({ domain: hostname }) => [hostname]),
            Match.when({ domain: { production: true }, disposable: false }, ({ domain: { hostname } }) => [hostname]),
            Match.when({ domain: { previews: true }, disposable: true }, ({ domain: { hostname } }) => [`${name}.${hostname}`]),
            Match.orElse(() => []),
        ),
    );
    const stack = { name, hostnames, production: !disposable };
    return {
        projectName: 'rasm-hosting',
        stackName: `${name}.${target}`,
        program: () => Effect.runPromiseWith(context)<Record<string, unknown>, Effect.Error<ReturnType<(typeof targets)[typeof target]>>>(targets[target](build, stack)),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { hosting, targets };
