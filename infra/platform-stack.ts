import { isIPv4 } from 'node:net';
import playwright from '@playwright/test/package.json' with { type: 'json' };
import aws from '@pulumi/aws';
import { DnsRecord, getZoneOutput } from '@pulumi/cloudflare';
import { remote } from '@pulumi/command';
import { Container, type ContainerArgs, getRegistryImageOutput, Network, Provider, type ProviderArgs, RemoteImage, Tag, Volume } from '@pulumi/docker';
import dockerBuild from '@pulumi/docker-build';
import minio from '@pulumi/minio';
import { all, type CustomResourceOptions, getProject, type Input, interpolate, jsonStringify, output, secret } from '@pulumi/pulumi';
import { RandomPassword } from '@pulumi/random';
import { getDeviceOutput } from '@pulumi/tailscale';
import type { deploy } from '@rasm/hosting/deploy';
import { Identity } from '@rasm/hosting/identity';
import { Array, Data, Duration, Effect, Function, Option, Predicate, Record, String, Struct } from 'effect';
import type { identified } from '#/repository-stack';

// --- [TYPES] ---------------------------------------------------------------------------

type Role = Data.TaggedEnum<{
    Cache: { readonly name: string; readonly kept: Duration.Duration };
    Runner: { readonly instances: Readonly<Record<string, Readonly<Record<string, string>>>> };
    Apps: { readonly owner: string };
    History: { readonly name: string };
}>;
interface Row {
    readonly address: string;
    readonly tags: readonly `tag:${string}`[];
    readonly roles: readonly Role[];
}

// --- [TABLES] --------------------------------------------------------------------------

const hosts = {
    'lucky.parametric-arsenal.cloud': {
        address: '31.97.131.41',
        tags: ['tag:server'],
        roles: [{ _tag: 'History', name: 'atuin' }],
    },
    'derby.parametric-arsenal.cloud': {
        address: '45.90.223.193',
        tags: ['tag:server'],
        roles: [
            { _tag: 'Cache', name: 'nx-cache', kept: Duration.weeks(2) },
            { _tag: 'Runner', instances: { host: { MemoryHigh: '18G', MemoryMax: '20G' }, container: {} } },
        ],
    },
} as const satisfies Record.ReadonlyRecord<string, Row>;
const entrypoints = { web: 80, websecure: 443 } as const;
const local = { host: 'local.rasm.localhost', owner: 'owner@rasm.localhost' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Role = Data.taggedEnum<Role>();

// --- [SERVICES] ------------------------------------------------------------------------

class Machine extends Data.Class<{
    readonly name: string;
    readonly zone: string;
    readonly roles: readonly Role[];
    readonly provider: Provider;
    readonly builder: dockerBuild.Provider;
    readonly network: Input<string>;
    readonly remote: Option.Option<{ readonly zoneId: Input<string>; readonly target: Input<string> }>;
}> {
    // --- [CONSTRUCTION]

    static readonly on = (host: string, shared: Option.Option<Input<string>>, registryAuth?: ProviderArgs['registryAuth']) => {
        const [name, ...labels] = String.split(host, '.');
        const [zone, row] = [labels.join('.'), Record.get<string, Row>(hosts, host)];
        const roles = Option.match(row, { onNone: () => [Role.Apps({ owner: local.owner })], onSome: Struct.get('roles') });
        const dialed = Option.getOrUndefined(Option.map(row, () => ({ host: `ssh://root@${name}` })));
        const provider = new Provider(host, { ...dialed, registryAuth: roles.some(Role.$is('Runner')) ? registryAuth : undefined });
        const located = Option.map(row, ({ address }) => [address, getZoneOutput({ filter: { name: zone } }).zoneId] as const).pipe(
            Option.map(([address, zoneId]) => ({ zoneId, target: Option.isNone(shared) ? new DnsRecord(host, { zoneId, name: host, type: 'A', content: address, ttl: 1, proxied: false }).name : host })),
        );
        return new Machine({ name, zone, roles, provider, builder: new dockerBuild.Provider(host, { ...dialed }), network: Option.getOrElse(shared, () => new Network(host, { name: getProject() }, { provider }).name), remote: located });
    };

    // --- [ROUTING]

    static readonly entrypoint = 'websecure' satisfies keyof typeof entrypoints;
    static readonly guard = 'auth';
    static readonly forwarded = { id: 'Remote-User', email: 'Remote-Email' } as const;

    routed(service: string, port: number, extra: Readonly<Record<string, string>> = {}) {
        const address = `${service}.${this.zone}`;
        const stated = { 'traefik.enable': 'true', [`traefik.http.routers.${service}.rule`]: `Host(\`${address}\`)`, [`traefik.http.routers.${service}.entrypoints`]: Machine.entrypoint, [`traefik.http.services.${service}.loadbalancer.server.port`]: `${port}`, ...extra };
        const named = Option.toArray(Option.map(this.remote, ({ zoneId, target }) => new DnsRecord(address, { zoneId, name: address, type: 'CNAME', content: target, ttl: 1, proxied: false })));
        return { address, named, labels: Record.collect(stated, (label, value) => ({ label, value })) };
    }

    // --- [CONTAINERS]

    pulled(image: string, options: CustomResourceOptions = {}) {
        return new RemoteImage(`${this.name}-${image}`, { name: interpolate`${image}@${getRegistryImageOutput({ name: image }, { provider: this.provider }).sha256Digest}` }, { provider: this.provider, ...options }).repoDigest;
    }

    imaged(image: string, inline: Input<string>, { cache, ...args }: Pick<dockerBuild.ImageArgs, 'context' | 'labels' | 'registries'> & { readonly cache?: string } = {}) {
        const stored = cache === undefined ? { load: true } : { exports: [{ registry: { annotations: args.labels } }], ...Struct.Record(['cacheFrom', 'cacheTo'], [{ registry: { ref: cache } }]) };
        return new dockerBuild.Image(`${this.name}-${image}`, { dockerfile: { inline }, context: { location: '-' }, tags: [image], push: false, buildOnPreview: false, ...stored, buildArgs: { BUILDKIT_DOCKERFILE_CHECK: 'error=true' }, ...args }, { provider: this.builder }).digest;
    }

    kept(volume: string, options: CustomResourceOptions = {}) {
        return new Volume(`${this.name}-${volume}`, { name: volume }, { provider: this.provider, ...options }).name;
    }

    contained(service: string, tests: readonly string[], args: Omit<ContainerArgs, 'name' | 'networksAdvanced' | 'restart' | 'logDriver' | 'wait' | 'healthcheck'>, options: CustomResourceOptions = {}) {
        return new Container(
            `${this.name}-${service}`,
            { name: service, networksAdvanced: [{ name: this.network }], restart: 'unless-stopped', logDriver: 'local', wait: true, waitTimeout: 300, destroyGraceSeconds: 120, ...Array.match(tests, { onEmpty: () => ({}), onNonEmpty: (stated) => ({ healthcheck: { tests: [...stated], interval: '10s' } }) }), ...args },
            { provider: this.provider, ...options },
        );
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const tailnet = Function.memoize((machine: Machine) => getDeviceOutput({ hostname: machine.name }));

const proxied = Function.memoize((machine: Machine) => {
    const secure = Machine.entrypoint.toUpperCase();
    return machine.contained('traefik', ['CMD', 'traefik', 'healthcheck'], {
        image: machine.pulled('traefik:v3.8.0-rc.3'),
        envs: [
            ...Record.collect(entrypoints, (name, port) => `TRAEFIK_ENTRYPOINTS_${name.toUpperCase()}_ADDRESS=:${port}`),
            `TRAEFIK_ENTRYPOINTS_WEB_HTTP_REDIRECTIONS_ENTRYPOINT_TO=${Machine.entrypoint}`,
            `TRAEFIK_ENTRYPOINTS_${secure}_HTTP_TLS=true`,
            `TRAEFIK_ENTRYPOINTS_${secure}_TRANSPORT_RESPONDINGTIMEOUTS_READTIMEOUT=30m`,
            'TRAEFIK_PING=true',
            'TRAEFIK_PROVIDERS_DOCKER=true',
            'TRAEFIK_PROVIDERS_DOCKER_EXPOSEDBYDEFAULT=false',
            ...Option.match(machine.remote, {
                onNone: () => [],
                onSome: () => ['TRAEFIK_CERTIFICATESRESOLVERS_ACME_ACME_STORAGE=/var/lib/traefik/acme.json', 'TRAEFIK_CERTIFICATESRESOLVERS_ACME_ACME_TLSCHALLENGE=true', `TRAEFIK_ENTRYPOINTS_${secure}_HTTP_TLS_CERTRESOLVER=acme`],
            }),
        ],
        ports: Object.values(entrypoints).map((port) => ({ internal: port, external: port })),
        mounts: [
            { type: 'bind', source: '/var/run', target: '/var/run', readOnly: true },
            { type: 'volume', source: machine.kept('traefik'), target: '/var/lib/traefik' },
        ],
    });
});

const stored = (machine: Machine, { user, key }: Readonly<Record<'user' | 'key', Input<string>>>) => {
    const ports = { internal: 9000, external: 7412, health: '/health/ready' } as const;
    const published = Option.match(machine.remote, {
        onNone: () => ({ ips: output(['127.0.0.1']), address: '127.0.0.1' }),
        onSome: () => ({ ips: tailnet(machine).addresses.apply(Array.filter(isIPv4)), address: machine.name }),
    });
    const route = Option.map(
        Option.liftPredicate(machine, ({ roles }) => roles.some(Role.$is('Apps'))),
        (exposed) => exposed.routed('storage', ports.internal),
    );
    const serving = machine.contained(
        'storage',
        ['CMD', 'curl', '--fail', '--silent', `http://127.0.0.1:${ports.internal}${ports.health}`],
        {
            image: machine.pulled('rustfs/rustfs:1.0.6'),
            envs: [interpolate`RUSTFS_ACCESS_KEY=${user}`, interpolate`RUSTFS_SECRET_KEY=${key}`, 'RUSTFS_CONSOLE_ENABLE=false', 'RUSTFS_OBS_LOG_DIRECTORY='],
            ports: published.ips.apply(Array.map((ip) => ({ internal: ports.internal, external: ports.external, ip }))),
            ulimits: [{ name: 'nofile', soft: 1_048_576, hard: 1_048_576 }],
            mounts: [{ type: 'volume', source: machine.kept('storage'), target: '/data' }],
            labels: Option.match(route, { onNone: () => [{ label: 'traefik.enable', value: 'false' }], onSome: Struct.get('labels') }),
        },
        { dependsOn: Option.match(route, { onNone: Array.empty, onSome: ({ named }) => [proxied(machine), ...named] }) },
    );
    return output({ server: serving.urn.apply(() => `${published.address}:${ports.external}`), within: `http://storage:${ports.internal}`, ...Option.getOrUndefined(Option.map(route, ({ address }) => ({ origin: `https://${address}` }))), region: 'us-east-1', user, key });
};

const bucketed = (name: string, storage: ReturnType<typeof stored>, buckets: Effect.Success<ReturnType<typeof deploy.Machine>>['buckets']) => {
    const provider = new minio.Provider(name, { minioServer: storage.server, minioUser: storage.user, minioPassword: storage.key });
    const allowed = Record.filter(
        Record.map(buckets, ({ cors }) => cors.map(({ origins, methods, headers, exposed, age }) => ({ allowedOrigins: [...origins], allowedMethods: [...methods], allowedHeaders: [...headers], exposeHeaders: [...exposed], maxAgeSeconds: Duration.toSeconds(age) }))),
        Array.isArrayNonEmpty,
    );
    const s3 = Option.map(
        Option.liftPredicate(allowed, Predicate.not(Record.isEmptyRecord)),
        () =>
            new aws.Provider(name, {
                region: storage.region,
                accessKey: storage.user,
                secretKey: storage.key,
                endpoints: [{ s3: interpolate`http://${storage.server}` }],
                s3UsePathStyle: true,
                ...Struct.Record(['skipCredentialsValidation', 'skipMetadataApiCheck', 'skipRegionValidation', 'skipRequestingAccountId'], true),
            }),
    );
    const created = Record.collect(buckets, (bucket, { kept, abort, expire }) => {
        const made = new minio.S3Bucket(bucket, { bucket, forceDestroy: !kept }, { provider, protect: kept });
        const rules = [{ id: 'abort', abortIncompleteMultipartUpload: { daysAfterInitiation: Math.ceil(Duration.toDays(abort)) } }, ...Record.collect(expire, (prefix, age) => ({ id: `expire ${prefix}*`, ...(prefix === '' ? {} : { filter: { prefix } }), expiration: { days: Math.ceil(Duration.toDays(age)) } }))];
        const cors = Option.toArray(Option.zipWith(s3, Record.get(allowed, bucket), (s3Provider, corsRules) => new aws.s3.BucketCorsConfiguration(bucket, { bucket: made.bucket, corsRules }, { provider: s3Provider })));
        return { arn: made.arn, configured: [new minio.S3BucketLifecycle(bucket, { bucket: made.bucket, rules }, { provider }), ...cors].map(({ id }) => id) };
    });
    const account = new minio.IamServiceAccount(name, { targetUser: storage.user, policy: jsonStringify({ Version: '2012-10-17', Statement: [{ Effect: 'Allow', Action: ['s3:*'], Resource: created.flatMap(({ arn }) => [arn, interpolate`${arn}/*`]) }] }) }, { provider });
    return { AWS_ENDPOINT_URL_S3: storage.within, AWS_REGION: storage.region, AWS_ACCESS_KEY_ID: all([account.accessKey, ...created.flatMap(({ configured }) => configured)]).apply(([id]) => id), AWS_SECRET_ACCESS_KEY: account.secretKey };
};

const cached = (machine: Machine, { name, kept }: Data.TaggedEnum.Value<Role, 'Cache'>, storage: ReturnType<typeof stored>, token: Input<string>) => {
    const { port, health } = { port: 3000, health: '/health' };
    const scoped = bucketed(name, storage, { [name]: { kept: false, abort: kept, expire: { '': kept }, cors: [] } });
    const { address, named, labels } = machine.routed(name, port, { [`traefik.http.services.${name}.loadbalancer.healthcheck.path`]: health });
    const image = machine.imaged(
        name,
        `# syntax=docker/dockerfile:1
FROM scratch AS amd64
ADD --checksum=sha256:0cb2f024ddfea4e1e409525051f9cc26d1d15ced66e834298dd3623753370564 --chmod=755 https://github.com/nxcite/nx-cache-server/releases/download/v1.5.0-rc.1/nx-cache-aws-v1.5.0-rc.1-linux-x86_64 /nx-cache-aws
FROM scratch AS arm64
ADD --checksum=sha256:cdd6d6b152b35df607d0fd5d01968311be700286f9808b7093f90258164aed97 --chmod=755 https://github.com/nxcite/nx-cache-server/releases/download/v1.5.0-rc.1/nx-cache-aws-v1.5.0-rc.1-linux-arm64 /nx-cache-aws
FROM \${TARGETARCH}
FROM gcr.io/distroless/cc-debian13:debug-nonroot
COPY --from=2 /nx-cache-aws /
ENTRYPOINT ["/nx-cache-aws"]
`,
    );
    const envs = Record.collect({ S3_BUCKET_NAME: name, S3_ENDPOINT_URL: scoped.AWS_ENDPOINT_URL_S3, ...scoped, SERVICE_ACCESS_TOKEN: token }, (variable, value) => interpolate`${variable}=${value}`);
    const serving = machine.contained(name, ['CMD', '/busybox/wget', '--quiet', '--spider', `http://127.0.0.1:${port}${health}`], { image, init: true, envs, labels }, { dependsOn: [proxied(machine), ...named] });
    return { [name]: serving.urn.apply(() => `https://${address}`) };
};

const synced = (machine: Machine, { name }: Data.TaggedEnum.Value<Role, 'History'>, owner: string, password: Input<string>) => {
    const [port, file] = [8888, '/run/password'];
    const { address, named, labels } = machine.routed(name, port);
    const serving = machine.contained(
        name,
        [],
        {
            image: machine.pulled('ghcr.io/atuinsh/atuin:18.25.1'),
            entrypoints: ['/bin/sh', '-c'],
            command: [`(atuin-server admin user create --email "${owner}@users.noreply.github.com" "${owner}" < ${file} || atuin-server admin user set-password "${owner}" < ${file}) && exec atuin-server start`],
            envs: ['ATUIN_HOST=0.0.0.0', `ATUIN_PORT=${port}`, 'ATUIN_DB_URI=sqlite:///config/atuin.db'],
            uploads: [{ file, content: interpolate`${password}\n` }],
            mounts: [{ type: 'volume', source: machine.kept('atuin', { protect: true }), target: '/config' }],
            labels,
        },
        { dependsOn: [proxied(machine), ...named] },
    );
    return { [name]: serving.urn.apply(() => `https://${address}`) };
};

const hosted = (machine: Machine, { owner }: Data.TaggedEnum.Args<Role, 'Apps'>, storage: ReturnType<typeof stored>) => {
    const [port, { guard, forwarded }] = [9091, Machine];
    const password = new RandomPassword(`${machine.name}-owner`, { length: 32, special: false });
    const { address, named, labels } = machine.routed(guard, port, {
        [`traefik.http.middlewares.${guard}-check.forwardauth.address`]: `http://${guard}:${port}/api/authz/forward-auth`,
        [`traefik.http.middlewares.${guard}-check.forwardauth.authResponseHeaders`]: Object.values(forwarded).join(','),
        ...Record.mapEntries(forwarded, (header) => [`traefik.http.middlewares.${guard}-strip.headers.customrequestheaders.${header}`, '']),
        [`traefik.http.middlewares.${guard}.chain.middlewares`]: `${guard}-strip,${guard}-check`,
    });
    const configuration = {
        access_control: { default_policy: 'bypass', rules: [{ domain: `*.${machine.zone}`, resources: [`^${RegExp.escape(Identity.paths.signin)}([?].*)?$`], policy: 'one_factor' }] },
        authentication_backend: { file: { path: '/config/users.yml' }, password_reset: { disable: true } },
        notifier: { filesystem: { filename: '/var/lib/authelia/notification.txt' } },
        ntp: { disable_startup_check: true },
        session: { cookies: [{ domain: machine.zone, authelia_url: `https://${address}` }] },
        storage: { local: { path: '/var/lib/authelia/db.sqlite3' } },
    };
    machine.contained(
        guard,
        [],
        {
            image: machine.pulled('authelia/authelia:4.39.28'),
            envs: Array.map(['AUTHELIA_SESSION_SECRET', 'AUTHELIA_STORAGE_ENCRYPTION_KEY'], (row) => interpolate`${row}=${new RandomPassword([machine.name, row].join('-'), { length: 64, special: false }).result}`),
            uploads: [
                { file: '/config/configuration.yml', content: jsonStringify(configuration) },
                { file: '/config/users.yml', content: jsonStringify({ users: { owner: { displayname: 'Owner', email: owner, password: password.bcryptHash } } }) },
            ],
            mounts: [{ type: 'volume', source: machine.kept(guard), target: '/var/lib/authelia' }],
            labels,
        },
        { dependsOn: [proxied(machine), ...named] },
    );
    return { network: output(machine.network), owner: { name: 'owner', password: password.result }, storage };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const installed = Effect.sync(() => {
    const machine = Machine.on(local.host, Option.none());
    return hosted(
        machine,
        local,
        stored(
            machine,
            Record.map({ user: 'storage-access', key: 'storage-secret' }, (name) => new RandomPassword(`${machine.name}-${name}`, { length: 32, special: false }).result),
        ),
    );
});

const provisioned = (host: keyof typeof hosts, { owner, repository, engines }: Effect.Success<typeof identified>, rows: Readonly<Record<'EPIC_GHCR_TOKEN' | 'GITHUB_TOKEN' | 'NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN' | 'RUSTFS_ACCESS_KEY' | 'RUSTFS_SECRET_KEY' | 'ATUIN_PASSWORD', string>>) =>
    Effect.sync(() => {
        const registries = [{ address: 'ghcr.io', username: owner, password: rows.EPIC_GHCR_TOKEN }];
        const machine = Machine.on(host, Option.none(), registries);
        const storage = Function.memoize((held: Machine) => stored(held, { user: secret(rows.RUSTFS_ACCESS_KEY), key: secret(rows.RUSTFS_SECRET_KEY) }));
        const served = machine.roles.map(
            Role.$match({
                Cache: (settings): Readonly<Record<string, unknown>> => cached(machine, settings, storage(machine), secret(rows.NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN)),
                Apps: (settings) => hosted(machine, settings, storage(machine)),
                History: (settings) => synced(machine, settings, owner, secret(rows.ATUIN_PASSWORD)),
                Runner: ({ instances }) => {
                    const runner = { version: '2.338.2', sha256: 'baebf204307bac6193480852732f0aba2ae97b6c5597c244f4d5558390147c40', user: 'runner' } as const;
                    const browsers = `ghcr.io/${owner}/${repository}/playwright`.toLowerCase();
                    const [, registered] = Array.mapAccum(Record.toEntries(instances), Option.none<remote.Command>(), (before, [label, limits]) => {
                        const [folder, work] = [`/home/${runner.user}/actions-runner-${label}`, `/home/${runner.user}/actions-work-${label}`];
                        const archive = `/tmp/actions-runner-${label}.tar.gz`;
                        const removed = `token=$(cat); if cd "${folder}" 2>/dev/null; then systemctl revert "$(cat .service)"; ./svc.sh stop; ./svc.sh uninstall; sudo -u ${runner.user} ./config.sh remove --pat "$token"; fi; rm -rf "${folder}"`;
                        const installation = [
                            removed,
                            'set -eu',
                            `install -d -o ${runner.user} -g ${runner.user} "${folder}" "${work}" && cd "${folder}"`,
                            `curl -fsSL -o "${archive}" https://github.com/actions/runner/releases/download/v${runner.version}/actions-runner-linux-x64-${runner.version}.tar.gz`,
                            `echo "${runner.sha256}  ${archive}" | sha256sum --check --quiet`,
                            `sudo -u ${runner.user} tar xzf "${archive}" && rm "${archive}"`,
                            './bin/installdependencies.sh',
                            `sudo -u ${runner.user} ./config.sh --unattended --replace --url "https://github.com/${owner}/${repository}" --name "${machine.name}-${label}" --labels "${label}" --work "${work}" --pat "$token"`,
                            `./svc.sh install ${runner.user}`,
                            './svc.sh start',
                            ...Array.match(
                                Record.collect(limits, (option, value) => `"${option}=${value}"`),
                                { onEmpty: Array.empty, onNonEmpty: (options) => [`systemctl set-property "$(cat .service)" ${options.join(' ')}`] },
                            ),
                        ].join('\n');
                        const instance = new remote.Command(
                            `${machine.name}-runner-${label}`,
                            { connection: { host: machine.name }, addPreviousOutputInEnv: false, logging: 'stderr', create: installation, delete: `${removed}; rm -rf "${work}"`, stdin: secret(rows.GITHUB_TOKEN), triggers: [tailnet(machine).nodeId] },
                            { dependsOn: Option.toArray(before), deleteBeforeReplace: true },
                        );
                        return [Option.some(instance), [label, instance.id] as const];
                    });
                    return {
                        runners: Record.fromEntries(registered),
                        engines: Array.map(engines, (version) => `ghcr.io/epicgames/unreal-engine:dev-slim-${version}`).map(
                            (image) => new Tag(`${machine.name}-${image}`, { sourceImage: machine.pulled(image, { customTimeouts: { create: '2h', update: '2h' } }), targetImage: image }, { provider: machine.provider }).targetImage,
                        ),
                        browsers: machine.imaged(
                            `${browsers}:${playwright.version}`,
                            `FROM node:slim\nENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright\nRUN apt-get update && apt-get install --yes --no-install-recommends build-essential ca-certificates curl git libicu-dev && rm -rf /var/lib/apt/lists/*\nRUN npx --yes playwright@${playwright.version} install --with-deps chromium && npx --yes playwright@${playwright.version} install-deps firefox && rm -rf /var/lib/apt/lists/* /root/.npm\nUSER node\n`,
                            { registries, labels: { 'org.opencontainers.image.source': `https://github.com/${owner}/${repository}` }, cache: `${browsers}:cache` },
                        ),
                    };
                },
            }),
        );
        return Record.fromEntries(served.flatMap(Record.toEntries));
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { bucketed, hosts, installed, local, Machine, provisioned, Role };
