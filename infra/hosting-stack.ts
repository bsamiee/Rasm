import { join } from 'node:path';
import process from 'node:process';
import { pathToFileURL } from 'node:url';
import { Container, type ContainerArgs, getRegistryImageOutput, Network, RemoteImage, Volume } from '@pulumi/docker';
import { Image } from '@pulumi/docker-build';
import { getIamPolicyDocumentOutput, IamServiceAccount, Provider, S3Bucket } from '@pulumi/minio';
import { type Input, interpolate, jsonStringify, secret } from '@pulumi/pulumi';
import { RandomPassword } from '@pulumi/random';
import { App } from '@rasm/hosting/app';
import { nodeBundle } from '@rasm/hosting/node-bundle';
import { Sites } from '@rasm/hosting/sites';
import { Array, Cause, Config, Effect, Option, Predicate, Record, Redacted, Schema, Struct } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { postgresRoles } from '#/postgres-roles';

// --- [TYPES] ---------------------------------------------------------------------------

interface Stack {
    readonly app: App;
    readonly mode: keyof typeof App.modes;
    readonly client: string;
    readonly server: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const compose = ['compose', '--file', join(import.meta.dirname, 'compose.yaml'), '--profile', 'platform'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Composed = Schema.fromJsonString(Schema.Struct({ services: Schema.Record(Schema.String, Schema.Struct({ image: Schema.String, command: Schema.optionalKey(Schema.Array(Schema.String)), environment: Schema.optionalKey(Schema.Record(Schema.String, Schema.NullOr(Schema.String))) })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const docker = function* ({ app, mode, client, server }: Stack) {
    const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
    const { services } = yield* Effect.flatMap(spawner.string(ChildProcess.make('docker', [...compose, 'config', '--format', 'json'])), Schema.decodeEffect(Composed));
    const service = (key: 'storage' | 'authelia' | 'postgres') => Effect.fromNullishOr(services[key]).pipe(Effect.mapError(() => new Cause.NoSuchElementError(`infra/compose.yaml defines no ${key} service`)));
    const [email, storage, authelia] = yield* Effect.all([
        Record.get(app.env, 'OWNER_EMAIL').pipe(Option.filter(App.Binding.$is('text')), Effect.fromOption, Effect.mapBoth({ onFailure: () => new Cause.NoSuchElementError(`app ${app.name} declares no OWNER_EMAIL text`), onSuccess: Struct.get('value') })),
        service('storage'),
        service('authelia'),
    ]);
    const name = App.resource(app, mode);
    const production = mode === 'production';
    const port = 8080;
    const loopback = '127.0.0.1';
    const password = (resource: string) => new RandomPassword(resource, { length: 40, special: false });
    const domains = [`${name}.localhost`];
    const [internal, edge] = [new Network(name, { name, internal: true }), new Network(`${name}-edge`, { name: `${name}-edge` })];
    const volume = (resource: string, containerPath: string) => ({ volumeName: new Volume(resource, { name: resource }, { protect: production }).name, containerPath });
    const pull = (resource: string, reference: string) => new RemoteImage(resource, { name: reference, keepLocally: true, pullTriggers: [getRegistryImageOutput({ name: reference }).sha256Digest] }).imageId;
    const container = (resource: string, image: Input<string>, { probe, ...args }: Omit<ContainerArgs, 'name' | 'image' | 'networksAdvanced' | 'restart'> & { readonly probe?: readonly string[] }) =>
        new Container(resource, { name: resource, image, networksAdvanced: [{ name: internal.name }, ...(args.ports === undefined ? [] : [{ name: edge.name }])], restart: 'unless-stopped', ...args, ...(probe === undefined ? {} : { healthcheck: { tests: ['CMD', ...probe], interval: '2s', retries: 30 }, wait: true }) });
    const reachable = (resource: string, image: Input<string>, { listen, ...args }: Parameters<typeof container>[2] & { readonly listen: number; readonly probe: readonly string[] }) => {
        const listening = container(resource, image, { ...args, ports: [{ internal: listen, ip: loopback }] });
        const published = listening.ports.apply((ports) => {
            const bound = Array.findFirst(ports ?? [], ({ internal: targetPort, ip, protocol, external }) => targetPort === listen && ip === loopback && protocol === 'tcp' && external > 0);
            return Effect.fromOption(bound, () => new Cause.NoSuchElementError(`Docker did not publish ${resource} on ${loopback}:${listen}`)).pipe(Effect.map(Struct.get('external')), Effect.runPromise);
        });
        return { address: { host: loopback, port: published }, endpoint: interpolate`${listening.name}:${listen}` };
    };
    const buckets = Record.map(Record.filter(app.env, App.Binding.$is('bucket')), (_, variable) => App.resource(app, mode, variable));
    const store = Option.map(Option.liftPredicate(buckets, Predicate.not(Record.isEmptyRecord)), (named) => {
        const [user, key, region] = [password(`${name}-store-access`).result, password(`${name}-store-secret`).result, 'us-east-1'];
        const listen = 9000;
        const { address, endpoint } = reachable(`${name}-store`, pull(`${name}-store`, storage.image), {
            listen,
            probe: ['curl', '-fsS', `http://${loopback}:${listen}/health/ready`],
            envs: [interpolate`RUSTFS_ACCESS_KEY=${user}`, interpolate`RUSTFS_SECRET_KEY=${key}`, `RUSTFS_REGION=${region}`, 'RUSTFS_CONSOLE_ENABLE=false'],
            volumes: [volume(`${name}-store`, '/data')],
        });
        const provider = new Provider(`${name}-store`, { minioServer: interpolate`${address.host}:${address.port}`, minioUser: user, minioPassword: key, minioRegion: region });
        const created = Record.map(named, (bucket) => new S3Bucket(bucket, { bucket, forceDestroy: !production }, { provider, protect: production }));
        const policy = getIamPolicyDocumentOutput({ statements: [{ effect: 'Allow', actions: ['s3:*'], resources: Object.values(created).flatMap(({ arn }) => [arn, interpolate`${arn}/*`]) }] }, { provider }).json;
        const account = new IamServiceAccount(name, { targetUser: user, policy }, { provider });
        return {
            envs: { ...Record.map(created, Struct.get('bucket')), AWS_ENDPOINT_URL_S3: interpolate`http://${endpoint}`, AWS_REGION: region, AWS_ACCESS_KEY_ID: account.accessKey, AWS_SECRET_ACCESS_KEY: account.secretKey },
            volumes: [],
        };
    });
    const bound = yield* Effect.forEach(Record.toEntries(app.env), ([variable, binding]) =>
        App.Binding.$match(binding, {
            bucket: () => Effect.succeed({ envs: {}, volumes: [] }),
            text: () => Effect.succeed({ envs: {}, volumes: [] }),
            json: () => Effect.succeed({ envs: {}, volumes: [] }),
            sqlite: () => Effect.succeed({ envs: { [variable]: `/data/${variable}/database.sqlite` }, volumes: [volume(App.resource(app, mode, variable), `/data/${variable}`)] }),
            postgres: Effect.fnUntraced(function* () {
                const [{ image, command = [], environment = {} }] = yield* Effect.all([service('postgres'), spawner.exitCode(ChildProcess.make('docker', [...compose, 'build', 'postgres']))]);
                const database = App.resource(app, mode, variable);
                const { result } = password(database);
                const listen = 5432;
                const { address, endpoint } = reachable(database, image, {
                    listen,
                    probe: ['pg_isready', '-h', loopback, '-p', `${listen}`, '-U', 'postgres'],
                    command: [...command],
                    envs: [interpolate`POSTGRES_PASSWORD=${result}`, ...Record.collect(Record.getSomes(Record.map(Struct.pick(environment, ['POSTGRES_INITDB_ARGS']), Option.fromNullOr)), (key, value) => `${key}=${value}`)],
                    volumes: [volume(database, '/var/lib/postgresql')],
                });
                return { envs: postgresRoles(database, variable, { ...address, password: result }, endpoint, production), volumes: [] };
            }),
            secret: () => Effect.map(Config.Redacted(variable), (value) => ({ envs: { [variable]: secret(Redacted.value(value)) }, volumes: [] })),
        }),
    );
    const parts = [...bound, ...Option.toArray(store)];
    const base = `node:${process.versions.node}-slim`;
    const pinned = interpolate`${base}@${getRegistryImageOutput({ name: base }).sha256Digest}`;
    const hosted = container(
        name,
        new Image(name, {
            push: false,
            load: true,
            buildOnPreview: false,
            tags: [`${name}:latest`],
            context: { location: server, named: { assets: { location: client } } },
            dockerfile: { inline: pinned.apply((image) => [`FROM ${image}`, 'WORKDIR /app', `COPY --from=assets . ${nodeBundle.assets}`, 'COPY . .', `ENV ${nodeBundle.portVariable}=${port}`, `CMD ["node", "${nodeBundle.entry}.js"]`].join('\n')) },
        }).digest,
        {
            probe: ['node', '--input-type=module', '--eval', `const response = await fetch('http://${loopback}:${port}${nodeBundle.readiness}', { method: 'HEAD', redirect: 'manual', signal: AbortSignal.timeout(2000) }); if (response.status !== 204) throw new Error('HTTP ' + response.status);`],
            envs: parts.flatMap(({ envs }) => Record.collect(envs, (variable, value: Input<string>) => interpolate`${variable}=${value}`)),
            volumes: parts.flatMap(({ volumes }) => volumes),
        },
    );
    const owner = password(`${name}-owner`);
    const { headers, paths, returnTo } = Sites.identity;
    const identity = container(`${name}-identity`, pull(`${name}-identity`, authelia.image), {
        uploads: Record.collect(
            {
                '/config/configuration.yml': {
                    authentication_backend: { password_reset: { disable: true }, file: { path: '/config/users.yml' } },
                    access_control: {
                        default_policy: 'bypass',
                        rules: [
                            { domain: domains, resources: [`^${paths.signin}([?].*)?$`], policy: 'one_factor' },
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
    const forward = { handler: 'reverse_proxy', upstreams: [{ dial: interpolate`${identity.name}:9091` }] };
    const copies = Record.collect(
        Record.mapKeys({ 'Remote-User': headers.id, 'Remote-Email': headers.email }, (answer) => `{http.reverse_proxy.header.${answer}}`),
        (answer, header) => ({
            match: [{ vars_regexp: { [answer]: { pattern: '.' } } }],
            handle: [{ handler: 'headers', request: { set: { [header]: [answer] } } }],
        }),
    );
    const redirects = Record.collect(
        {
            signin: { source: `{http.request.uri.query.${returnTo}}`, input_regexp: '^(/.*)$', location: 'https://{http.request.host}{target}' },
            signout: { source: '{http.request.uri.query}', input_regexp: `(?:^|&)${returnTo}=((?:/|%2[Ff])[^&]*)`, location: 'https://auth.{http.request.host}/logout?rd=https://{http.request.host}{target}' },
        },
        (action, { source, input_regexp, location }) => ({
            match: [{ path: [paths[action]] }],
            handle: [
                { handler: 'map', source, destinations: ['{target}'], mappings: [{ input_regexp, outputs: ['$1'] }], defaults: ['/'] },
                { handler: 'static_response', status_code: 302, headers: { Location: [location] } },
            ],
        }),
    );
    const routes = [
        { match: [{ host: domains.map((site) => `auth.${site}`) }], handle: [forward] },
        {
            match: [{ host: domains }],
            handle: [
                { handler: 'encode', encodings: { zstd: {}, gzip: {} }, prefer: ['zstd', 'gzip'] },
                {
                    ...forward,
                    rewrite: { method: 'GET', uri: '/api/authz/forward-auth' },
                    headers: { request: { set: { 'X-Forwarded-Method': ['{http.request.method}'], 'X-Forwarded-Uri': ['{http.request.uri}'] } } },
                    handle_response: [{ match: { status_code: [2] }, routes: [{ handle: [{ handler: 'headers', request: { delete: Object.values(headers) } }] }, ...copies] }],
                },
                { handler: 'subroute', routes: redirects },
                { handler: 'reverse_proxy', upstreams: [{ dial: interpolate`${hosted.name}:${port}` }] },
            ],
        },
    ];
    const config = { file: '/etc/caddy/caddy.json', content: jsonStringify({ admin: { disabled: true }, apps: { http: { servers: { [name]: { listen: [':443'], routes } } } } }) };
    const proxy = container(`${name}-proxy`, pull(`${name}-proxy`, 'caddy:latest'), {
        command: ['caddy', 'run', '--config', config.file],
        uploads: [config],
        volumes: [volume(`${name}-proxy`, '/data')],
        ports: [{ internal: 80 }, { internal: 443 }, { internal: 443, protocol: 'udp' }].map((published) => ({ ...published, external: published.internal, ip: production ? undefined : loopback })),
    });
    return { urls: domains.map((site) => `https://${site}`), proxy: proxy.name, ownerPassword: owner.result };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const hostingValues = { local: Effect.fnUntraced(docker) };
const hostingStack = Effect.fn('hostingStack')(function* (directory: string, value: keyof typeof hostingValues, production: boolean) {
    const mode = production ? 'production' : 'disposable';
    const app = yield* Effect.tryPromise({ try: () => import(pathToFileURL(join(directory, 'hosting.config.ts')).href), catch: (cause) => new Cause.UnknownError(cause, `Cannot load ${directory}/hosting.config.ts`) }).pipe(
        Effect.map(Struct.get('default')),
        Effect.filterOrFail(
            (declared): declared is App => declared instanceof App,
            (declared) => new Cause.UnknownError(declared, `${directory}/hosting.config.ts exports no App by default`),
        ),
    );
    const compiled = yield* Effect.cached(
        Effect.gen(function* () {
            const { createBuilder } = yield* Effect.tryPromise(() => import('vite'));
            const builder = yield* Effect.tryPromise(() => createBuilder({ root: directory, mode: 'production', configLoader: 'native' }));
            yield* Effect.tryPromise(() => builder.buildApp());
            const [client, server] = yield* Effect.all([Effect.fromOption(Record.get(builder.environments, 'client')), Effect.fromOption(Record.get(builder.environments, nodeBundle.environment))]);
            return { client: join(builder.config.root, client.config.build.outDir), server: join(builder.config.root, server.config.build.outDir) };
        }).pipe(Effect.mapError((cause) => new Cause.UnknownError(cause, `Cannot build ${directory}`))),
    );
    return {
        projectName: 'rasm-hosting',
        stackName: `${App.resource(app, mode)}.${value}`,
        program: Effect.flatMap(compiled, (built) => hostingValues[value]({ app, mode, ...built })),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { hostingStack, hostingValues };
