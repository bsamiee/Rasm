import process from 'node:process';
import { type BuildOutput, readBuildOutput } from '@cloudflare/build-output-utils';
import { D1Database, getZeroTrustOrganizationOutput, getZoneOutput, HyperdriveConfig, R2Bucket, Worker, WorkersCustomDomain, WorkersRoute, WorkerVersion, ZeroTrustAccessApplication, ZeroTrustAccessIdentityProvider, ZeroTrustAccessPolicy } from '@pulumi/cloudflare';
import { Container, type ContainerArgs, getRegistryImageOutput, Network, RemoteImage, Volume } from '@pulumi/docker';
import { Image } from '@pulumi/docker-build';
import { IamPolicy, IamUser, IamUserPolicyAttachment, Provider, S3Bucket } from '@pulumi/minio';
import { Extension, Grant, Provider as PostgresProvider, Schema as PostgresSchema, type ProviderArgs, Role, type RoleArgs } from '@pulumi/postgresql';
import { type CustomResourceOptions, type Input, interpolate, output, secret } from '@pulumi/pulumi';
import { RandomInteger, RandomPassword } from '@pulumi/random';
import { Bindings, migrator } from '@rasm/hosting/bindings.ts';
import { contract } from '@rasm/hosting/sites.ts';
import { environment } from '@rasm/hosting/vite.ts';
import { Array, type Cause, Config, Effect, FileSystem, Function, Match, Path, Record, Redacted, Result, Schema } from 'effect';
import { resolveConfig } from 'vite';
import { stringify } from 'yaml';

// --- [TYPES] ---------------------------------------------------------------------------

interface Stack {
    readonly name: string;
    readonly hostnames: readonly string[];
    readonly protect: boolean;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const password = (resource: string) => new RandomPassword(resource, { length: 40, special: false });
const postgres = (resource: string, server: ProviderArgs & { readonly database: string }, protect: boolean, options?: CustomResourceOptions) => {
    const provider = new PostgresProvider(resource, server, options);
    const login = (role: string, settings: RoleArgs) => {
        const { result } = password(`${resource}-${role}`);
        return { name: new Role(`${resource}-${role}`, { name: `${resource}-${role}`, login: true, password: result, ...settings }, { provider }).name, password: result };
    };
    const owner = login('migrator', { idleInTransactionSessionTimeout: 60_000 });
    const schema = new PostgresSchema(resource, { name: owner.name, owner: owner.name, database: server.database }, { provider, protect });
    const runtime = login('runtime', { statementTimeout: 15_000, idleInTransactionSessionTimeout: 30_000, searchPaths: [schema.name] });
    const statistics = new Extension(`${resource}-pg-stat-statements`, { name: 'pg_stat_statements', database: server.database }, { provider });
    return {
        migrator: { user: new Grant(`${resource}-migrator`, { database: server.database, role: schema.owner, objectType: 'database', privileges: ['CREATE'] }, { provider, dependsOn: statistics }).role, password: owner.password },
        runtime: { user: new Grant(`${resource}-runtime`, { database: server.database, role: runtime.name, objectType: 'schema', schema: schema.name, privileges: ['USAGE'] }, { provider }).role, password: runtime.password },
    };
};
const targets = {
    cloudflare: Effect.fnUntraced(function* ({ rootConfig, workers }: BuildOutput, { env, triggers }: typeof Bindings.Type, { name, hostnames, protect }: Stack) {
        const { config, bundleDir, assetsDir } = workers.default;
        const [path, accountId] = yield* Effect.all([Path.Path, rootConfig.accountId === undefined ? Config.String('CLOUDFLARE_ACCOUNT_ID') : Config.succeed(rootConfig.accountId)]);
        const worker = new Worker(name, { accountId, name, logpush: config.logpush, observability: config.observability, subdomain: { enabled: config.workersDev ?? true, previewsEnabled: config.previewUrls } });
        const pin = new ZeroTrustAccessIdentityProvider(name, { accountId, name, type: 'onetimepin', config: {} });
        const access = new ZeroTrustAccessApplication(name, {
            accountId,
            name,
            type: 'self_hosted',
            destinations: worker.subdomain.apply(({ enabled, url }) => [...hostnames, ...(enabled ? [new URL(url).host] : [])].map((host) => ({ type: 'public', uri: `${host}${contract.paths.signin}` }))),
            allowedIdps: [pin.id],
            autoRedirectToIdentity: true,
            policies: [{ id: new ZeroTrustAccessPolicy(name, { accountId, name, decision: 'allow', includes: [{ everyone: {} }] }).id, precedence: 1 }],
        });
        const bindings = yield* Effect.forEach(Record.toEntries(env), ([variable, binding]) => {
            const resource = `${name}-${variable}`.toLowerCase();
            return Match.value(binding).pipe(
                Match.discriminatorsExhaustive('type')({
                    assets: () => Effect.succeed([{ name: variable, type: 'assets' }]),
                    d1: (database) => Effect.succeed([{ name: variable, type: 'd1', id: new D1Database(resource, { accountId, name: database.name ?? resource }, { protect }).uuid }]),
                    r2: (bucket) => Effect.succeed([{ name: variable, type: 'r2_bucket', bucketName: new R2Bucket(resource, { accountId, name: bucket.name ?? resource, jurisdiction: bucket.jurisdiction }, { protect }).name, jurisdiction: bucket.jurisdiction }]),
                    hyperdrive: () =>
                        Effect.map(Config.URL(variable), (url) => {
                            const [database, port] = [url.pathname.slice(1), url.port === '' ? undefined : Number(url.port)];
                            const { migrator: owner, runtime } = postgres(resource, { host: url.hostname, port, database, username: decodeURIComponent(url.username), password: secret(decodeURIComponent(url.password)) }, protect);
                            return [
                                {
                                    name: variable,
                                    type: 'hyperdrive',
                                    id: new HyperdriveConfig(resource, { accountId, name: resource, origin: { scheme: url.protocol.slice(0, -1), host: url.hostname, port, database, user: runtime.user, password: runtime.password }, caching: { disabled: true } }).id,
                                },
                                { name: migrator(variable), type: 'secret_text', text: interpolate`${url.protocol}//${owner.user}:${owner.password}@${url.host}${url.pathname}${url.search}` },
                            ];
                        }),
                    secret: () => Effect.map(Config.Redacted(variable), (value) => [{ name: variable, type: 'secret_text', text: secret(Redacted.value(value)) }]),
                    text: ({ value }) => Effect.succeed([{ name: variable, type: 'plain_text', text: value }]),
                }),
            );
        });
        const version = new WorkerVersion(name, {
            accountId,
            workerId: worker.id,
            compatibilityDate: config.compatibilityDate,
            compatibilityFlags: config.compatibilityFlags,
            limits: config.limits,
            ...(config.manifest === undefined || bundleDir === undefined
                ? {}
                : {
                      mainModule: config.manifest.mainModule,
                      modules: Record.toEntries(config.manifest.modules).map(([module, { type }]) => ({
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
            bindings: [...bindings.flat(), { name: 'ACCESS', type: 'json', json: output({ team: interpolate`https://${getZeroTrustOrganizationOutput({ accountId }).authDomain}`, aud: access.aud }).apply(JSON.stringify) }],
            deploy: true,
        });
        return {
            url: worker.subdomain.url,
            version: version.id,
            domains: hostnames.map((hostname) => new WorkersCustomDomain(`${name}-${hostname}`, { accountId, hostname, service: worker.name }, { dependsOn: [version] }).hostname),
            routes: triggers.map(({ pattern, zone }) => new WorkersRoute(`${name}-${pattern}`, { zoneId: getZoneOutput({ filter: { name: zone, account: { id: accountId } } }).zoneId, pattern, script: worker.name }, { dependsOn: [version] }).pattern),
        };
    }),
    docker: Effect.fnUntraced(function* ({ root, workers }: BuildOutput, { env }: typeof Bindings.Type, { name, hostnames, protect }: Stack) {
        const [path, assets, email, resolved] = yield* Effect.all([Path.Path, Effect.fromNullishOr(workers.default.assetsDir), Config.String('OWNER_EMAIL'), Effect.tryPromise(() => resolveConfig({ root }, 'build', 'production'))]);
        const { build } = yield* Effect.fromNullishOr(resolved.environments[environment]);
        const port = 8080;
        const sites = Array.match(hostnames, { onEmpty: () => [`${name}.localhost`], onNonEmpty: Function.identity });
        const [internal, edge] = [new Network(name, { name, internal: true }), new Network(`${name}-edge`, { name: `${name}-edge` })];
        const volume = (resource: string, containerPath: string) => ({ volumeName: new Volume(resource, { name: resource }, { protect }).name, containerPath });
        const pull = (resource: string, reference: string) => new RemoteImage(resource, { name: reference, keepLocally: true, pullTriggers: [getRegistryImageOutput({ name: reference }).sha256Digest] }).imageId;
        const container = (resource: string, image: Input<string>, args: Omit<ContainerArgs, 'name' | 'image' | 'networksAdvanced' | 'restart'>) =>
            new Container(resource, { name: resource, image, networksAdvanced: [{ name: internal.name }, ...(args.ports === undefined ? [] : [{ name: edge.name }])], restart: 'unless-stopped', ...args });
        const reachable = (resource: string, reference: string, { listen, test, ...args }: Parameters<typeof container>[2] & { readonly listen: number; readonly test: readonly string[] }) => {
            const address = { host: '127.0.0.1', port: new RandomInteger(`${resource}-port`, { min: 49_152, max: 65_535 }).result };
            return { address, server: container(resource, pull(resource, reference), { ...args, ports: [{ internal: listen, external: address.port, ip: address.host }], healthcheck: { tests: ['CMD', ...test], interval: '2s', retries: 30 }, wait: true }) };
        };
        const named = (variable: string) => `${name}-${variable}`.toLowerCase();
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
                const buckets = Record.filterMap(env, (binding, variable) => (binding.type === 'r2' ? Result.succeed(new S3Bucket(named(variable), { bucket: binding.name ?? named(variable), forceDestroy: !protect }, { provider, protect }).bucket) : Result.failVoid));
                const policy = new IamPolicy(
                    name,
                    { name, policy: output({ Version: '2012-10-17', Statement: [{ Effect: 'Allow', Action: ['s3:*'], Resource: Object.values(buckets).flatMap((bucket) => [interpolate`arn:aws:s3:::${bucket}`, interpolate`arn:aws:s3:::${bucket}/*`]) }] }).apply(JSON.stringify) },
                    { provider },
                );
                const account = new IamUser(name, { name, secret: password(`${name}-store-user`).result }, { provider });
                return { ...buckets, AWS_ENDPOINT_URL_S3: interpolate`http://${server.name}:9000`, AWS_REGION: region, AWS_ACCESS_KEY_ID: new IamUserPolicyAttachment(name, { userName: account.name, policyName: policy.name }, { provider }).userName, AWS_SECRET_ACCESS_KEY: account.secret };
            }),
        );
        const variables = yield* Effect.forEach(Record.toEntries(env), ([variable, binding]) =>
            Match.value(binding).pipe(
                Match.discriminatorsExhaustive('type')({
                    assets: () => Effect.succeed({}),
                    d1: () => Effect.succeed({ [variable]: `/data/${variable}/database.sqlite` }),
                    r2: () => store,
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
                        const urls = Record.map(postgres(database, { ...address, database: 'postgres', username: 'postgres', password: result, sslmode: 'disable' }, protect, { dependsOn: server }), ({ user, password: key }) => interpolate`postgres://${user}:${key}@${server.name}:5432/postgres`);
                        return Effect.succeed({ [variable]: urls.runtime, [migrator(variable)]: urls.migrator });
                    },
                    secret: () => Effect.map(Config.Redacted(variable), (value) => ({ [variable]: secret(Redacted.value(value)) })),
                    text: ({ value }) => Effect.succeed({ [variable]: value }),
                }),
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
                dockerfile: { inline: [`FROM node:${process.versions.node}-slim`, 'WORKDIR /app', 'COPY --from=assets . assets', 'COPY . .', `ENV PORT=${port}`, 'CMD ["node", "index.js"]'].join('\n') },
            }).digest,
            {
                envs: [...new Map(variables.flatMap(Record.toEntries))].map(([variable, value]) => interpolate`${variable}=${value}`),
                volumes: Record.toEntries(env).flatMap(([variable, { type }]) => (type === 'd1' ? [volume(named(variable), `/data/${variable}`)] : [])),
            },
        );
        const owner = password(`${name}-owner`);
        const identity = container(`${name}-identity`, pull(`${name}-identity`, 'authelia/authelia:latest'), {
            uploads: [
                {
                    file: '/config/configuration.yml',
                    content: output({
                        server: { address: 'tcp://:9091/' },
                        authentication_backend: { password_reset: { disable: true }, file: { path: '/config/users.yml' } },
                        access_control: {
                            default_policy: 'bypass',
                            rules: [
                                { domain: sites, resources: [`^${contract.paths.signin}([?].*)?$`], policy: 'one_factor' },
                                { domain: sites, methods: ['POST', 'PUT', 'PATCH', 'DELETE'], policy: 'one_factor' },
                            ],
                        },
                        session: { secret: password(`${name}-identity-session`).result, cookies: sites.map((site) => ({ domain: site, authelia_url: `https://auth.${site}`, default_redirection_url: `https://${site}/` })) },
                        storage: { encryption_key: password(`${name}-identity-storage`).result, local: { path: '/var/lib/authelia/db.sqlite3' } },
                        notifier: { filesystem: { filename: '/var/lib/authelia/notification.txt' } },
                        ntp: { disable_startup_check: true },
                    }).apply(stringify),
                },
                { file: '/config/users.yml', content: output({ users: { owner: { displayname: 'Owner', email, password: owner.bcryptHash } } }).apply(stringify) },
            ],
            volumes: [volume(`${name}-identity`, '/var/lib/authelia')],
            wait: true,
        });
        const authority = interpolate`${identity.name}:9091`;
        const authenticate = {
            handler: 'reverse_proxy',
            upstreams: [{ dial: authority }],
            rewrite: { method: 'GET', uri: '/api/authz/forward-auth' },
            headers: { request: { set: { 'X-Forwarded-Method': ['{http.request.method}'], 'X-Forwarded-Uri': ['{http.request.uri}'] } } },
            handle_response: [
                {
                    match: { status_code: [2] },
                    routes: [
                        { handle: [{ handler: 'headers', request: { delete: Object.values(contract.headers) } }] },
                        ...Record.toEntries({ 'Remote-User': contract.headers.id, 'Remote-Email': contract.headers.email }).map(([answer, header]) => ({
                            match: [{ not: [{ vars: { [`{http.reverse_proxy.header.${answer}}`]: [''] } }] }],
                            handle: [{ handler: 'headers', request: { set: { [header]: [`{http.reverse_proxy.header.${answer}}`] } } }],
                        })),
                    ],
                },
            ],
        };
        const proxy = container(`${name}-proxy`, pull(`${name}-proxy`, 'caddy:latest'), {
            command: ['caddy', 'run', '--config', '/etc/caddy/caddy.json'],
            uploads: [
                {
                    file: '/etc/caddy/caddy.json',
                    content: output({
                        admin: { disabled: true },
                        apps: {
                            http: {
                                servers: {
                                    [name]: {
                                        listen: [':443'],
                                        routes: [
                                            { handle: [{ handler: 'map', source: `{http.request.uri.query.${contract.returnTo}}`, destinations: ['{target}'], mappings: [{ input_regexp: '^(/.*)$', outputs: ['$1'] }], defaults: ['/'] }] },
                                            ...sites.flatMap((site) => [
                                                { match: [{ host: [`auth.${site}`] }], handle: [{ handler: 'reverse_proxy', upstreams: [{ dial: authority }] }] },
                                                { match: [{ host: [site] }], handle: [authenticate] },
                                                { match: [{ host: [site], path: [contract.paths.signin] }], handle: [{ handler: 'static_response', status_code: 302, headers: { Location: [`https://${site}{target}`] } }] },
                                                { match: [{ host: [site], path: [contract.paths.signout] }], handle: [{ handler: 'static_response', status_code: 302, headers: { Location: [`https://auth.${site}/logout?rd=https://${site}{target}`] } }] },
                                                { match: [{ host: [site] }], handle: [{ handler: 'reverse_proxy', upstreams: [{ dial: interpolate`${app.name}:${port}` }] }] },
                                            ]),
                                        ],
                                    },
                                },
                            },
                        },
                    }).apply(JSON.stringify),
                },
            ],
            volumes: [volume(`${name}-proxy`, '/data')],
            ports: [
                { internal: 80, external: 80 },
                { internal: 443, external: 443 },
                { internal: 443, external: 443, protocol: 'udp' },
            ],
        });
        return { urls: sites.map((site) => `https://${site}`), proxy: proxy.name, ownerPassword: owner.result };
    }),
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const hosting = Effect.fn('hosting')(function* (directory: string, target: keyof typeof targets, disposable: boolean) {
    const [fs, build, context] = yield* Effect.all([FileSystem.FileSystem, Effect.tryPromise(() => readBuildOutput(directory)), Effect.context<Path.Path>()]);
    const { config, configPath } = build.workers.default;
    const bindings = yield* fs.readFileString(configPath).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Schema.fromJsonString(Bindings))));
    const name = disposable ? `${config.name}-disposable` : config.name;
    const hostnames = bindings.domains.flatMap((domain) =>
        Match.value(domain).pipe(
            Match.when(Match.string, (hostname) => [hostname]),
            Match.when({ production: true }, ({ hostname }) => [hostname]),
            Match.orElse(() => []),
        ),
    );
    return { projectName: 'rasm-hosting', stackName: `${name}.${target}`, program: () => Effect.runPromiseWith(context)<Record<string, unknown>, Config.ConfigError | Cause.NoSuchElementError | Cause.UnknownError>(targets[target](build, bindings, { name, hostnames, protect: !disposable })) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { hosting, targets };
