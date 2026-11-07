import { join } from 'node:path';
import process from 'node:process';
import { BUILD_OUTPUT_ROOT, type BuildOutput, readBuildOutput } from '@cloudflare/build-output-utils';
import { loadAndParseConfig } from '@cloudflare/config';
import { getZeroTrustOrganizationOutput, Worker, ZeroTrustAccessApplication, ZeroTrustAccessIdentityProvider, ZeroTrustAccessPolicy } from '@pulumi/cloudflare';
import { local } from '@pulumi/command';
import { Container, type ContainerArgs, getRegistryImageOutput, Network, RemoteImage, Volume } from '@pulumi/docker';
import { Image } from '@pulumi/docker-build';
import { getIamPolicyDocumentOutput, IamServiceAccount, Provider, S3Bucket } from '@pulumi/minio';
import { asset, type Input, interpolate, jsonStringify, secret } from '@pulumi/pulumi';
import { RandomPassword } from '@pulumi/random';
import { User } from '@rasm/hosting';
import { disposableName, hostingMode, ownerBinding } from '@rasm/hosting/cloudflare';
import { CloudflareAccess } from '@rasm/hosting/cloudflare-access';
import { nodeBundle } from '@rasm/hosting/node-bundle';
import { Array, Cause, Config, Effect, Function, Match, Option, Path, Predicate, Record, Redacted, String, Struct } from 'effect';
import { quote } from 'shell-quote';
import type { ResolvedConfig } from 'vite';
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
    const automatic = production
        ? []
        : declared.filter(([, binding]) => (binding.type === 'd1' && binding.id === undefined && binding.name === undefined) || (binding.type === 'r2' && binding.name === undefined) || (binding.type === 'kv' && binding.id === undefined) || (binding.type === 'queue' && binding.name === undefined));
    const discarded = yield* Effect.forEach(automatic, ([variable, { type }]) => {
        const resource = quote([provisioned(name, variable)]);
        return Match.value(type).pipe(
            Match.when('d1', () => Effect.succeed([`uuid=$(cf d1 list --name ${resource} | jq -r --arg name ${resource} '[.[] | select(.name == $name) | .uuid] | if length <= 1 then .[0] // "" else error("multiple databases with that name") end'); if [[ -n "$uuid" ]]; then cf d1 delete "$uuid" --force; fi`])),
            Match.when('r2', () => Effect.succeed([`while true; do keys=$(cf r2 objects list --bucket-name ${resource} --per-page 1000 | jq -c 'map(.key)'); [[ "$keys" != '[]' ]] || break; cf r2 objects bulk-delete --bucket-name ${resource} --body "$keys" --force; done`, `cf r2 buckets delete ${resource} --force`])),
            Match.orElse((unsupported) => refuse(`disposable binding ${variable} of type ${unsupported}`)),
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
    const deploy = quote(['cf', 'deploy', '--prebuilt', '--secrets-file', '/dev/stdin', ...Option.toArray(tag).flatMap((value) => ['--tag', value])]);
    const deletes = discarded.flat();
    const deployment = new local.Command(
        name,
        {
            dir: root,
            create: deploy,
            update: deploy,
            delete: deletes.length === 0 ? undefined : ['set -euo pipefail', ...deletes].join('\n'),
            interpreter: ['bash', '-c'],
            environment: { CLOUDFLARE_ACCOUNT_ID: accountId },
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
    const loopback = '127.0.0.1';
    const password = (resource: string) => new RandomPassword(resource, { length: 40, special: false });
    const domains = Array.match(hostnames, { onEmpty: () => [`${name}.localhost`], onNonEmpty: Function.identity });
    const [internal, edge] = [new Network(name, { name, internal: true }), new Network(`${name}-edge`, { name: `${name}-edge` })];
    const volume = (resource: string, containerPath: string) => ({ volumeName: new Volume(resource, { name: resource }, { protect: production }).name, containerPath });
    const pull = (resource: string, reference: string) => new RemoteImage(resource, { name: reference, keepLocally: true, pullTriggers: [getRegistryImageOutput({ name: reference }).sha256Digest] }).imageId;
    const container = (resource: string, image: Input<string>, { probe, ...args }: Omit<ContainerArgs, 'name' | 'image' | 'networksAdvanced' | 'restart'> & { readonly probe?: readonly string[] }) =>
        new Container(resource, { name: resource, image, networksAdvanced: [{ name: internal.name }, ...(args.ports === undefined ? [] : [{ name: edge.name }])], restart: 'unless-stopped', ...args, ...(probe === undefined ? {} : { healthcheck: { tests: ['CMD', ...probe], interval: '2s', retries: 30 }, wait: true }) });
    const reachable = (resource: string, reference: string, { listen, ...args }: Parameters<typeof container>[2] & { readonly listen: number; readonly probe: readonly string[] }) => {
        const server = container(resource, pull(resource, reference), { ...args, ports: [{ internal: listen, ip: loopback }] });
        const published = server.ports.apply((ports) => {
            const bound = Array.findFirst(ports ?? [], ({ internal: targetPort, ip, protocol, external }) => targetPort === listen && ip === loopback && protocol === 'tcp' && external > 0);
            return Effect.fromOption(bound, () => new Cause.NoSuchElementError(`Docker did not publish ${resource} on ${loopback}:${listen}`)).pipe(Effect.map(Struct.get('external')), Effect.runPromise);
        });
        return { address: { host: loopback, port: published }, endpoint: interpolate`${server.name}:${listen}` };
    };
    const declared = Record.filterMap(env, (binding, variable) =>
        Match.value(binding).pipe(
            Match.when({ type: 'r2' }, ({ name: bucket = provisioned(name, variable) }) => bucket),
            Match.result,
        ),
    );
    const store = Option.map(Option.liftPredicate(declared, Predicate.not(Record.isEmptyRecord)), (buckets) => {
        const [user, key, region] = [password(`${name}-store-access`).result, password(`${name}-store-secret`).result, 'us-east-1'];
        const listen = 9000;
        const { address, endpoint } = reachable(`${name}-store`, nodeBundle.services.store.image, {
            listen,
            probe: ['curl', '-fsS', `http://${loopback}:${listen}/health/ready`],
            envs: [interpolate`RUSTFS_ACCESS_KEY=${user}`, interpolate`RUSTFS_SECRET_KEY=${key}`, `RUSTFS_REGION=${region}`, 'RUSTFS_CONSOLE_ENABLE=false'],
            volumes: [volume(`${name}-store`, '/data')],
        });
        const provider = new Provider(`${name}-store`, { minioServer: interpolate`${address.host}:${address.port}`, minioUser: user, minioPassword: key, minioRegion: region });
        const created = Record.map(buckets, (bucket, variable) => new S3Bucket(provisioned(name, variable), { bucket, forceDestroy: !production }, { provider, protect: production }));
        const policy = getIamPolicyDocumentOutput({ statements: [{ effect: 'Allow', actions: ['s3:*'], resources: Object.values(created).flatMap(({ arn }) => [arn, interpolate`${arn}/*`]) }] }, { provider }).json;
        const account = new IamServiceAccount(name, { targetUser: user, policy }, { provider });
        return {
            envs: { ...Record.map(created, Struct.get('bucket')), AWS_ENDPOINT_URL_S3: interpolate`http://${endpoint}`, AWS_REGION: region, AWS_ACCESS_KEY_ID: account.accessKey, AWS_SECRET_ACCESS_KEY: account.secretKey },
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
                    const listen = 5432;
                    const { address, endpoint } = reachable(database, nodeBundle.services.database.image, {
                        listen,
                        probe: ['pg_isready', '-h', loopback, '-p', `${listen}`, '-U', 'postgres'],
                        command: nodeBundle.services.database.command,
                        envs: [interpolate`POSTGRES_PASSWORD=${result}`, ...Record.collect(nodeBundle.services.database.environment, (key, value) => `${key}=${value}`)],
                        volumes: [volume(database, '/var/lib/postgresql')],
                    });
                    return Effect.succeed({ envs: postgresRoles(database, variable, { ...address, password: result }, endpoint, production), volumes: [] });
                },
                json: ({ value }) => Effect.succeed({ envs: { [variable]: jsonStringify(value) }, volumes: [] }),
                secret: () => Effect.map(Config.Redacted(variable), (value) => ({ envs: { [variable]: secret(Redacted.value(value)) }, volumes: [] })),
                text: ({ value }) => Effect.succeed({ envs: { [variable]: value }, volumes: [] }),
            }),
            Match.orElse(({ type }) => refuse(`binding ${variable} of type ${type}`)),
        ),
    );
    const parts = [...services, ...Option.toArray(store)];
    const base = `node:${process.versions.node}-slim`;
    const pinned = interpolate`${base}@${getRegistryImageOutput({ name: base }).sha256Digest}`;
    const app = container(
        name,
        new Image(name, {
            push: false,
            load: true,
            buildOnPreview: false,
            tags: [`${name}:latest`],
            context: { location: path.resolve(resolved.root, build.outDir), named: { assets: { location: assets } } },
            dockerfile: { inline: pinned.apply((image) => [`FROM ${image}`, 'WORKDIR /app', `COPY --from=assets . ${nodeBundle.layout.assets}`, 'COPY . .', `ENV ${nodeBundle.layout.portVariable}=${port}`, `CMD ["node", "${nodeBundle.layout.entry}.js"]`].join('\n')) },
        }).digest,
        {
            probe: ['node', '--input-type=module', '--eval', `const response = await fetch('http://${loopback}:${port}${nodeBundle.layout.readiness}', { method: 'HEAD', redirect: 'manual', signal: AbortSignal.timeout(2000) }); if (response.status !== 204) throw new Error('HTTP ' + response.status);`],
            envs: parts.flatMap(({ envs }) => Record.collect(envs, (variable, value: Input<string>) => interpolate`${variable}=${value}`)),
            volumes: parts.flatMap(({ volumes }) => volumes),
        },
    );
    const owner = password(`${name}-owner`);
    const identity = container(`${name}-identity`, pull(`${name}-identity`, nodeBundle.services.identity.image), {
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
    const proxy = container(`${name}-proxy`, pull(`${name}-proxy`, nodeBundle.services.proxy.image), {
        command: ['caddy', 'run', '--config', config.file],
        uploads: [config],
        volumes: [volume(`${name}-proxy`, '/data')],
        ports: [{ internal: 80 }, { internal: 443 }, { internal: 443, protocol: 'udp' }].map((published) => ({ ...published, external: published.internal, ip: production ? undefined : loopback })),
    });
    return { urls: domains.map((site) => `https://${site}`), proxy: proxy.name, ownerPassword: owner.result };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const hostingTargets = { cloudflare: { modes: { production: hostingMode.edge, disposable: hostingMode.disposable }, program: Effect.fnUntraced(cloudflare) }, docker: { modes: { production: hostingMode.server, disposable: hostingMode.server }, program: Effect.fnUntraced(docker) } };
const hostingStack = Effect.fn('hostingStack')(function* (directory: string, target: keyof typeof hostingTargets, disposable: boolean, tag: Option.Option<string>) {
    const { modes, program } = hostingTargets[target];
    const mode = disposable ? modes.disposable : modes.production;
    const file = join(directory, 'cloudflare.config.ts');
    const { result } = yield* Effect.tryPromise({ try: () => loadAndParseConfig(file, { isPreview: false, mode }), catch: (cause) => new Cause.UnknownError(cause, `Cannot read ${file}`) });
    if (!result.success) {
        return yield* Effect.fail(result.error);
    }
    const configured = yield* Effect.fromNullishOr(result.data.worker).pipe(Effect.mapError(() => new Cause.NoSuchElementError(`${file} declares no Worker`)));
    const name = disposable && mode === modes.production ? disposableName(configured.name) : configured.name;
    const compiled = yield* Effect.cached(
        Effect.gen(function* () {
            const { createBuilder } = yield* Effect.tryPromise(() => import('vite'));
            const builder = yield* Effect.tryPromise(() => createBuilder({ root: directory, mode, configLoader: 'native' }));
            yield* Effect.tryPromise(() => builder.buildApp());
            const build = yield* Effect.tryPromise(() => readBuildOutput(directory));
            return { build, resolved: builder.config };
        }).pipe(Effect.mapError(({ cause }) => new Cause.UnknownError(cause, `Cannot build ${directory}`))),
    );
    return {
        projectName: 'rasm-hosting',
        stackName: `${name}.${target}`,
        program: Effect.gen(function* () {
            const { build, resolved } = yield* compiled;
            const { domains = [] } = build.workers.default.config;
            yield* Effect.forEach([...Record.keys(Record.remove(build.workers, 'default')).map((worker) => `worker ${worker}`), ...build.containers.map(({ configPath }) => `container at ${configPath}`)], refuse, { concurrency: 'unbounded' });
            const hostnames = Array.filterMap(
                domains,
                Match.type<(typeof domains)[number]>().pipe(
                    Match.when(Match.string, Function.identity<string>),
                    Match.when({ production: true }, ({ hostname }) => hostname),
                    Match.result,
                ),
            );
            return yield* program(build, { name, hostnames, production: !disposable, tag, resolved });
        }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { hostingStack, hostingTargets };
