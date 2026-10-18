import process from 'node:process';
import { type BuildOutput, readBuildOutput } from '@cloudflare/build-output-utils';
import { D1Database, getZeroTrustOrganizationOutput, getZoneOutput, HyperdriveConfig, R2Bucket, Worker, WorkersCustomDomain, WorkersRoute, WorkerVersion, ZeroTrustAccessApplication, ZeroTrustAccessIdentityProvider, ZeroTrustAccessPolicy } from '@pulumi/cloudflare';
import { Container, getRegistryImageOutput, Network, RemoteImage, Volume } from '@pulumi/docker';
import { Image } from '@pulumi/docker-build';
import { IamPolicy, IamUser, IamUserPolicyAttachment, Provider, S3Bucket } from '@pulumi/minio';
import { Extension, Grant, Provider as PostgresProvider, Schema as PostgresSchema, type ProviderArgs, Role, type RoleArgs } from '@pulumi/postgresql';
import { all, type Input, interpolate, output, type Resource, secret } from '@pulumi/pulumi';
import { RandomInteger, RandomPassword } from '@pulumi/random';
import { Bindings, migrator } from '@rasm/hosting/bindings.ts';
import { contract } from '@rasm/hosting/sites.ts';
import { environment } from '@rasm/hosting/vite.ts';
import { Array, type Cause, Config, Effect, FileSystem, Function, Match, Path, Record, Redacted, Schema } from 'effect';
import { resolveConfig } from 'vite';
import { stringify } from 'yaml';

// --- [TYPES] ---------------------------------------------------------------------------

interface Stack {
    readonly name: string;
    readonly hostnames: readonly string[];
    readonly protect: boolean;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const postgres = (resource: string, server: ProviderArgs & { readonly database: string }, { protect, dependsOn }: { readonly protect: boolean; readonly dependsOn: Resource[] }) => {
    const provider = new PostgresProvider(resource, server, { dependsOn });
    const login = (role: string, settings: RoleArgs) => {
        const { result } = new RandomPassword(`${resource}-${role}`, { length: 40, special: false });
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
                            const { migrator: owner, runtime } = postgres(resource, { host: url.hostname, port, database, username: decodeURIComponent(url.username), password: secret(decodeURIComponent(url.password)) }, { protect, dependsOn: [] });
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
            ...Match.value({ manifest: config.manifest, bundle: bundleDir }).pipe(
                Match.when({ manifest: Match.defined, bundle: Match.defined }, ({ manifest, bundle }) => ({
                    mainModule: manifest.mainModule,
                    modules: Record.toEntries(manifest.modules).map(([module, { type }]) => ({
                        name: module,
                        contentFile: path.join(bundle, module),
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
                })),
                Match.orElse(() => ({})),
            ),
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
        const { assetsDir } = workers.default;
        const [path, assets, email, resolved] = yield* Effect.all([Path.Path, Effect.fromNullishOr(assetsDir), Config.String('OWNER_EMAIL'), Effect.tryPromise(() => resolveConfig({ root }, 'build', 'production'))]);
        const { build } = yield* Effect.fromNullishOr(resolved.environments[environment]);
        const port = 8080;
        const region = 'us-east-1';
        const loopback = '127.0.0.1';
        const sites = Array.match(hostnames, { onEmpty: () => [`${name}.localhost`], onNonEmpty: Function.identity });
        const image = (resource: string, reference: string) => new RemoteImage(resource, { name: reference, keepLocally: true, pullTriggers: [getRegistryImageOutput({ name: reference }).sha256Digest] }).imageId;
        const volume = (resource: string) => new Volume(resource, { name: resource }, { protect }).name;
        const password = (resource: string) => new RandomPassword(resource, { length: 40, special: false });
        const [internal, edge] = [new Network(name, { name, internal: true }), new Network(`${name}-edge`, { name: `${name}-edge` })];
        const store = yield* Effect.cached(
            Effect.sync(() => {
                const [user, key] = [password(`${name}-store-access`).result, password(`${name}-store-secret`).result];
                const published = new RandomInteger(`${name}-store-port`, { min: 49_152, max: 65_535 }).result;
                const container = new Container(`${name}-store`, {
                    name: `${name}-store`,
                    image: image(`${name}-store`, 'rustfs/rustfs:latest'),
                    envs: [interpolate`RUSTFS_ACCESS_KEY=${user}`, interpolate`RUSTFS_SECRET_KEY=${key}`, `RUSTFS_REGION=${region}`, 'RUSTFS_CONSOLE_ENABLE=false'],
                    volumes: [{ volumeName: volume(`${name}-store`), containerPath: '/data' }],
                    networksAdvanced: [{ name: internal.name }, { name: edge.name }],
                    ports: [{ internal: 9000, external: published, ip: loopback }],
                    healthcheck: { tests: ['CMD', 'curl', '-fsS', 'http://127.0.0.1:9000/health/ready'], interval: '2s', retries: 30 },
                    wait: true,
                    restart: 'unless-stopped',
                });
                return { endpoint: interpolate`http://${container.name}:9000`, provider: new Provider(`${name}-store`, { minioServer: interpolate`${loopback}:${published}`, minioUser: user, minioPassword: key, minioRegion: region }, { dependsOn: [container] }) };
            }),
        );
        const bindings = yield* Effect.forEach(Record.toEntries(env), ([variable, binding]) => {
            const resource = `${name}-${variable}`.toLowerCase();
            return Match.value(binding).pipe(
                Match.discriminatorsExhaustive('type')({
                    assets: () => Effect.succeed({ envs: [], volumes: [], buckets: [] }),
                    d1: () => Effect.succeed({ envs: [`${variable}=/data/${variable}/database.sqlite`], volumes: [{ volumeName: volume(resource), containerPath: `/data/${variable}` }], buckets: [] }),
                    r2: (bucket) =>
                        Effect.map(store, ({ provider }) => {
                            const { bucket: created } = new S3Bucket(resource, { bucket: bucket.name ?? resource, forceDestroy: !protect }, { provider, protect });
                            return { envs: [interpolate`${variable}=${created}`], volumes: [], buckets: [created] };
                        }),
                    hyperdrive: () =>
                        Effect.sync(() => {
                            const { result } = password(resource);
                            const published = new RandomInteger(`${resource}-port`, { min: 49_152, max: 65_535 }).result;
                            const server = new Container(resource, {
                                name: resource,
                                image: image(resource, 'postgres:latest'),
                                command: ['postgres', '-c', 'shared_preload_libraries=pg_stat_statements'],
                                envs: [interpolate`POSTGRES_PASSWORD=${result}`, 'POSTGRES_INITDB_ARGS=--locale-provider=builtin --builtin-locale=PG_UNICODE_FAST'],
                                volumes: [{ volumeName: volume(resource), containerPath: '/var/lib/postgresql' }],
                                networksAdvanced: [{ name: internal.name }, { name: edge.name }],
                                ports: [{ internal: 5432, external: published, ip: loopback }],
                                healthcheck: { tests: ['CMD', 'pg_isready', '-U', 'postgres'], interval: '2s', retries: 30 },
                                wait: true,
                                restart: 'unless-stopped',
                            });
                            const roles = postgres(resource, { host: loopback, port: published, database: 'postgres', username: 'postgres', password: result, sslmode: 'disable' }, { protect, dependsOn: [server] });
                            const url = (login: typeof roles.runtime) => interpolate`postgres://${login.user}:${login.password}@${server.name}:5432/postgres`;
                            return { envs: [interpolate`${variable}=${url(roles.runtime)}`, interpolate`${migrator(variable)}=${url(roles.migrator)}`], volumes: [], buckets: [] };
                        }),
                    secret: () => Effect.map(Config.Redacted(variable), (value) => ({ envs: [secret(`${variable}=${Redacted.value(value)}`)], volumes: [], buckets: [] })),
                    text: ({ value }) => Effect.succeed({ envs: [`${variable}=${value}`], volumes: [], buckets: [] }),
                }),
            );
        });
        const credentials = yield* Array.match(
            bindings.flatMap(({ buckets }) => buckets),
            {
                onEmpty: () => Effect.succeed(Array.empty<Input<string>>()),
                onNonEmpty: (buckets) =>
                    Effect.map(store, ({ endpoint, provider }) => {
                        const policy = new IamPolicy(name, { name, policy: output({ Version: '2012-10-17', Statement: [{ Effect: 'Allow', Action: ['s3:*'], Resource: buckets.flatMap((bucket) => [interpolate`arn:aws:s3:::${bucket}`, interpolate`arn:aws:s3:::${bucket}/*`]) }] }).apply(JSON.stringify) }, { provider });
                        const user = new IamUser(name, { name, secret: password(`${name}-store-user`).result }, { provider });
                        const { userName } = new IamUserPolicyAttachment(name, { userName: user.name, policyName: policy.name }, { provider });
                        return [interpolate`AWS_ENDPOINT_URL_S3=${endpoint}`, `AWS_REGION=${region}`, interpolate`AWS_ACCESS_KEY_ID=${userName}`, interpolate`AWS_SECRET_ACCESS_KEY=${user.secret}`];
                    }),
            },
        );
        const app = new Container(name, {
            name,
            image: new Image(name, {
                push: false,
                load: true,
                exec: true,
                buildOnPreview: false,
                tags: [`${name}:latest`],
                context: { location: path.resolve(resolved.root, build.outDir), named: { assets: { location: assets } } },
                dockerfile: { inline: [`FROM node:${process.versions.node}-slim`, 'WORKDIR /app', 'COPY --from=assets . assets', 'COPY . .', `ENV PORT=${port}`, 'CMD ["node", "index.js"]'].join('\n') },
            }).digest,
            envs: [...bindings.flatMap(({ envs }): Input<string>[] => envs), ...credentials],
            volumes: bindings.flatMap(({ volumes }) => volumes),
            networksAdvanced: [{ name: internal.name }],
            restart: 'unless-stopped',
        });
        const owner = password(`${name}-owner`);
        const identity = new Container(`${name}-identity`, {
            name: `${name}-identity`,
            image: image(`${name}-identity`, 'authelia/authelia:latest'),
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
            volumes: [{ volumeName: volume(`${name}-identity`), containerPath: '/var/lib/authelia' }],
            networksAdvanced: [{ name: internal.name }],
            wait: true,
            restart: 'unless-stopped',
        });
        const proxy = new Container(`${name}-proxy`, {
            name: `${name}-proxy`,
            image: image(`${name}-proxy`, 'caddy:latest'),
            uploads: [
                {
                    file: '/etc/caddy/Caddyfile',
                    content: all([identity.name, app.name]).apply(([authority, upstream]) =>
                        [
                            '{\n\tadmin off\n}',
                            ...sites.map((site) =>
                                [
                                    `auth.${site} {\n\treverse_proxy ${authority}:9091\n}`,
                                    `${site} {`,
                                    `\tforward_auth ${authority}:9091 {\n\t\turi /api/authz/forward-auth\n\t\tcopy_headers Remote-User>${contract.headers.id} Remote-Email>${contract.headers.email}\n\t}`,
                                    `\thandle ${contract.paths.signin} {\n\t\tredir https://{host}{query.${contract.returnTo}}\n\t}`,
                                    `\thandle ${contract.paths.signout} {\n\t\tredir https://auth.${site}/logout?rd=https://{host}{query.${contract.returnTo}}\n\t}`,
                                    `\thandle {\n\t\treverse_proxy ${upstream}:${port}\n\t}`,
                                    '}',
                                ].join('\n'),
                            ),
                        ].join('\n'),
                    ),
                },
            ],
            volumes: [{ volumeName: volume(`${name}-proxy`), containerPath: '/data' }],
            networksAdvanced: [{ name: internal.name }, { name: edge.name }],
            ports: [
                { internal: 80, external: 80 },
                { internal: 443, external: 443 },
                { internal: 443, external: 443, protocol: 'udp' },
            ],
            restart: 'unless-stopped',
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
