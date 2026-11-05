import { join } from 'node:path';
import process from 'node:process';
import { BUILD_OUTPUT_ROOT, type BuildOutput, readBuildOutput } from '@cloudflare/build-output-utils';
import { getZeroTrustOrganizationOutput, Worker, ZeroTrustAccessApplication, ZeroTrustAccessIdentityProvider, ZeroTrustAccessPolicy } from '@pulumi/cloudflare';
import { local } from '@pulumi/command';
import { Container, type ContainerArgs, getRegistryImageOutput, Network, RemoteImage, Volume } from '@pulumi/docker';
import { Image } from '@pulumi/docker-build';
import { IamPolicy, IamUser, IamUserPolicyAttachment, Provider, S3Bucket } from '@pulumi/minio';
import { asset, type Input, interpolate, jsonStringify, secret } from '@pulumi/pulumi';
import { RandomInteger, RandomPassword } from '@pulumi/random';
import { User } from '@rasm/hosting';
import { disposableName, hostingMode, ownerBinding } from '@rasm/hosting/cloudflare';
import { CloudflareAccess } from '@rasm/hosting/cloudflare-access';
import { nodeBundle } from '@rasm/hosting/node-bundle';
import { Array, Cause, Config, Effect, Function, Match, Option, Path, Predicate, Record, Redacted, String, Struct } from 'effect';
import { createBuilder, type ResolvedConfig } from 'vite';
import { postgresRoles } from '#/infra/postgres-roles';

// --- [TYPES] ---------------------------------------------------------------------------

interface Stack {
    readonly name: string;
    readonly hostnames: readonly string[];
    readonly production: boolean;
    readonly tag: Option.Option<string>;
    readonly resolved: ResolvedConfig;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const refuse = (subject: string) => Effect.fail(new Cause.IllegalArgumentError(`${subject} has no hosting form`));
const provisioned = (name: string, variable: string) => `${name}-${String.snakeToKebab(variable.toLowerCase())}`;
const cloudflare = function* ({ root, rootConfig, workers }: BuildOutput, { name, hostnames, production, tag }: Stack) {
    const { workersDev, previewUrls, env = {} } = workers.default.config;
    const accountId = yield* rootConfig.accountId === undefined ? Config.String('CLOUDFLARE_ACCOUNT_ID') : Config.succeed(rootConfig.accountId);
    const declared = Record.toEntries(Record.remove(env, CloudflareAccess.binding));
    const secrets = yield* Effect.forEach(
        declared.filter(([, { type }]) => type === 'secret'),
        ([variable]) => Effect.map(Config.Redacted(variable), (value) => [variable, Redacted.value(value)] as const),
    );
    const discarded = production
        ? []
        : yield* Effect.forEach(declared, ([variable, binding]) => {
              const resource = provisioned(name, variable);
              return Match.value(binding).pipe(
                  Match.when({ type: 'd1', id: Predicate.isUndefined, name: Predicate.isUndefined }, () => Effect.succeed([`cf d1 delete "$(cf d1 list --name ${resource} | jq -er '.[0].uuid')" --force`])),
                  Match.when({ type: 'r2', name: Predicate.isUndefined }, () =>
                      Effect.succeed([`while keys=$(cf r2 objects list --bucket-name ${resource} --per-page 1000 | jq -ce 'map(.key) | select(length > 0)'); do cf r2 objects bulk-delete --bucket-name ${resource} --body "$keys" --force; done`, `cf r2 buckets delete ${resource} --force`]),
                  ),
                  Match.whenOr({ type: 'kv', id: Predicate.isUndefined }, { type: 'queue', name: Predicate.isUndefined }, ({ type }) => refuse(`disposable binding ${variable} of type ${type}`)),
                  Match.orElse(() => Effect.succeed([])),
              );
          });
    const worker = new Worker(name, { accountId, name, subdomain: { enabled: workersDev ?? true, previewsEnabled: previewUrls } });
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
    const deploy = ['cf deploy --prebuilt --secrets-file /dev/stdin', ...Option.toArray(Option.map(tag, (value) => `--tag ${value}`))].join(' ');
    const deployment = new local.Command(
        name,
        {
            dir: root,
            create: deploy,
            update: deploy,
            delete: discarded.flat().join(' && ') || undefined,
            stdin: secret(
                jsonStringify({
                    ...Object.fromEntries(secrets),
                    [CloudflareAccess.binding]: jsonStringify({ issuer, audience: access.aud, jwks: interpolate`${issuer}/cdn-cgi/access/certs`, logout: '/cdn-cgi/access/logout' } satisfies Record<keyof typeof CloudflareAccess.Encoded, Input<string>>),
                }),
            ),
            triggers: [new asset.FileArchive(join(root, BUILD_OUTPUT_ROOT))],
        },
        { dependsOn: [worker, access] },
    );
    return { url: worker.subdomain.url, deployment: deployment.stdout };
};
const docker = function* ({ root, workers }: BuildOutput, { name, hostnames, production, resolved }: Stack) {
    const [path, assets, email] = yield* Effect.all([Path.Path, Effect.fromNullishOr(workers.default.assetsDir).pipe(Effect.mapError(() => new Cause.NoSuchElementError(`worker ${name} has no assets for the Node server`))), Config.String(ownerBinding)]);
    const { build } = yield* Effect.fromNullishOr(resolved.environments[nodeBundle.layout.environment]).pipe(Effect.mapError(() => new Cause.NoSuchElementError(`Vite configuration at ${root} has no ${nodeBundle.layout.environment} environment`)));
    const env = workers.default.config.env ?? {};
    const port = 8080;
    const password = (resource: string) => new RandomPassword(resource, { length: 40, special: false });
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
            Match.when({ type: 'r2' }, ({ name: bucket = provisioned(name, variable) }) => bucket),
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
        const created = Record.map(buckets, (bucket, variable) => new S3Bucket(provisioned(name, variable), { bucket, forceDestroy: !production }, { provider, protect: production }));
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
                d1: () => Effect.succeed({ envs: { [variable]: `/data/${variable}/database.sqlite` }, volumes: [volume(provisioned(name, variable), `/data/${variable}`)] }),
                hyperdrive: () => {
                    const database = provisioned(name, variable);
                    const { result } = password(database);
                    const { address, server, endpoint } = reachable(database, 'postgres:latest', {
                        listen: 5432,
                        probe: () => ['pg_isready', '-U', 'postgres'],
                        command: ['postgres', '-c', 'shared_preload_libraries=pg_stat_statements'],
                        envs: [interpolate`POSTGRES_PASSWORD=${result}`, 'POSTGRES_INITDB_ARGS=--locale-provider=builtin --builtin-locale=PG_UNICODE_FAST'],
                        volumes: [volume(database, '/var/lib/postgresql')],
                    });
                    return Effect.succeed({ envs: postgresRoles(database, variable, { ...address, password: result }, endpoint, production, { dependsOn: server }), volumes: [] });
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
            pull: true,
            dockerfile: { inline: [`FROM node:${process.versions.node}-slim`, 'WORKDIR /app', `COPY --from=assets . ${nodeBundle.layout.assets}`, 'COPY . .', `ENV ${nodeBundle.layout.portVariable}=${port}`, `CMD ["node", "${nodeBundle.layout.entry}.js"]`].join('\n') },
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
                            { domain: domains, resources: [`^${User.sites.paths.signin}([?].*)?$`], policy: 'one_factor' },
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
        { match: [{ host: domains.map((site) => `auth.${site}`) }], handle: [authelia] },
        {
            match: [{ host: domains }],
            handle: [
                { handler: 'encode', encodings: { zstd: {}, gzip: {} }, prefer: ['zstd', 'gzip'] },
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
    return { urls: domains.map((site) => `https://${site}`), proxy: proxy.name, ownerPassword: owner.result };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const hostingTargets = { cloudflare: { modes: { production: hostingMode.edge, disposable: hostingMode.disposable }, program: Effect.fnUntraced(cloudflare) }, docker: { modes: { production: hostingMode.server, disposable: hostingMode.server }, program: Effect.fnUntraced(docker) } };
const hostingStack = Effect.fn('hostingStack')(function* (directory: string, target: keyof typeof hostingTargets, disposable: boolean, tag: Option.Option<string>) {
    const { modes, program } = hostingTargets[target];
    const mode = disposable ? modes.disposable : modes.production;
    const builder = yield* Effect.tryPromise({ try: () => createBuilder({ root: directory, mode, configLoader: 'native' }), catch: (cause) => new Cause.UnknownError(cause, `Vite configuration at ${directory} does not resolve`) });
    yield* Effect.tryPromise({ try: () => builder.buildApp(), catch: (cause) => new Cause.UnknownError(cause, `build at ${directory} failed`) });
    const build = yield* Effect.tryPromise({ try: () => readBuildOutput(directory), catch: (cause) => new Cause.UnknownError(cause, `build output at ${directory} is unreadable`) });
    yield* Effect.forEach([...Record.keys(Record.remove(build.workers, 'default')).map((worker) => `worker ${worker}`), ...build.containers.map(({ configPath }) => `container at ${configPath}`)], refuse);
    const { name: configured, domains = [] } = build.workers.default.config;
    const name = disposable && mode === modes.production ? disposableName(configured) : configured;
    const hostnames = Array.filterMap(
        domains,
        Match.type<(typeof domains)[number]>().pipe(
            Match.when(Match.string, Function.identity<string>),
            Match.when({ production: true }, ({ hostname }) => hostname),
            Match.result,
        ),
    );
    return {
        projectName: 'rasm-hosting',
        stackName: `${name}.${target}`,
        program: program(build, { name, hostnames, production: !disposable, tag, resolved: builder.config }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { hostingStack, hostingTargets };
