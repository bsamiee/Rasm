import { isIPv4 } from 'node:net';
import playwright from '@playwright/test/package.json' with { type: 'json' };
import aws from '@pulumi/aws';
import { DnsRecord, getZoneOutput } from '@pulumi/cloudflare';
import command, { remote } from '@pulumi/command';
import { Container, type ContainerArgs, getRegistryImageOutput, Network, Provider, type ProviderArgs, RemoteImage, Volume } from '@pulumi/docker';
import dockerBuild from '@pulumi/docker-build';
import minio from '@pulumi/minio';
import { all, type CustomResourceOptions, getOrganization, getProject, type Input, interpolate, jsonStringify, type Output, output, StackReference } from '@pulumi/pulumi';
import { RandomPassword } from '@pulumi/random';
import { getDeviceOutput } from '@pulumi/tailscale';
import { Secret } from '@pulumiverse/doppler';
import type { deploy } from '@rasm/hosting/deploy';
import { Access, Identity } from '@rasm/hosting/identity';
import { Array, Data, Duration, Effect, Function, Option, Record, Schema, Struct } from 'effect';
import type { configs } from '#/repository-stack';

// --- [TYPES] ---------------------------------------------------------------------------

type Role = Data.TaggedEnum<{
    Cache: { readonly name: string; readonly kept: Duration.Duration };
    Runner: { readonly instances: Readonly<Record<string, Readonly<Record<string, string>>>> };
    Apps: { readonly owner: string };
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
        roles: [],
    },
    'derby.parametric-arsenal.cloud': {
        address: '45.90.223.193',
        tags: ['tag:server'],
        roles: [
            { _tag: 'Cache', name: 'nx-cache', kept: Duration.weeks(1) },
            { _tag: 'Runner', instances: { host: { MemoryHigh: '18G', MemoryMax: '20G' }, container: {} } },
        ],
    },
} as const satisfies Record.ReadonlyRecord<string, Row>;
const entrypoints = { web: 80, websecure: 443 } as const;
const local = { host: 'local.rasm.localhost', owner: 'owner@rasm.localhost' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Role = Data.taggedEnum<Role>();
class Stored extends Schema.Opaque<Stored, unknown>()(Schema.Struct(Struct.Record(['server', 'within', 'origin', 'region', 'user', 'key'], Schema.String))) {}

// --- [SERVICES] ------------------------------------------------------------------------

class Machine extends Data.Class<{
    readonly name: string;
    readonly zone: string;
    readonly provider: Provider;
    readonly builder: dockerBuild.Provider;
    readonly shared: Option.Option<Input<string>>;
    readonly remote: Option.Option<{ readonly address: string; readonly zoneId: Output<string> }>;
}> {
    // --- [CONSTRUCTION]

    static readonly on = (host: string, network: Option.Option<Input<string>>, registries: Pick<ProviderArgs, 'registryAuth'> = {}) => {
        const [name = host, ...labels] = host.split('.');
        const zone = labels.join('.');
        const reached = Option.map(Record.get<string, Row>(hosts, host), ({ address }) => ({ address, zoneId: getZoneOutput({ filter: { name: zone } }).zoneId }));
        const dialed = Option.getOrUndefined(Option.map(reached, () => ({ host: `ssh://root@${name}` })));
        const provider = new Provider(host, { ...dialed, ...registries });
        const builder = new dockerBuild.Provider(host, { ...dialed });
        return new Machine({ name, zone, provider, builder, shared: network, remote: reached });
    };

    static readonly joined = (host: string) => {
        const reference = new StackReference(`${getOrganization()}/${getProject()}/${host}`);
        return { machine: Machine.on(host, Option.some(reference.requireOutput('network'))), storage: reference.requireOutput('storage').apply(Schema.decodeUnknownPromise(Stored)) };
    };

    // --- [ROUTING]

    static readonly entrypoint = 'websecure' satisfies keyof typeof entrypoints;
    static readonly guard = 'auth';
    static readonly forwarded = { id: 'Remote-User', email: 'Remote-Email' } as const;

    get access() {
        return Access.make({ source: { _tag: 'Forwarded', ...Machine.forwarded }, logout: `https://${Machine.guard}.${this.zone}/logout` });
    }

    named(name: string, type: 'A' | 'CNAME', content: string) {
        return Option.toArray(Option.map(this.remote, ({ zoneId }) => new DnsRecord(name, { zoneId, name, type, content, ttl: 1, proxied: false })));
    }

    routed(service: string, port: number, extra: Readonly<Record<string, string>> = {}) {
        const address = `${service}.${this.zone}`;
        const stated = { 'traefik.enable': 'true', [`traefik.http.routers.${service}.rule`]: `Host(\`${address}\`)`, [`traefik.http.routers.${service}.entrypoints`]: Machine.entrypoint, [`traefik.http.services.${service}.loadbalancer.server.port`]: `${port}`, ...extra };
        return { address, named: this.named(address, 'CNAME', `${this.name}.${this.zone}`), labels: Record.collect(stated, (label, value) => ({ label, value })) };
    }

    // --- [CONTAINERS]

    static readonly networked = Function.memoize((machine: Machine) => Option.getOrElse(machine.shared, () => new Network(`${machine.name}.${machine.zone}`, { name: getProject() }, { provider: machine.provider }).name));

    get network() {
        return Machine.networked(this);
    }

    pulled(image: string, options: CustomResourceOptions = {}) {
        return new RemoteImage(`${this.name}-${image}`, { name: interpolate`${image}@${getRegistryImageOutput({ name: image }, { provider: this.provider }).sha256Digest}` }, { provider: this.provider, ...options }).repoDigest;
    }

    imaged(image: string, inline: Input<string>, args: Pick<dockerBuild.ImageArgs, 'context' | 'labels' | 'registries'> = {}) {
        const stored = args.registries === undefined ? { load: true } : { exports: [{ registry: { annotations: args.labels } }] };
        return new dockerBuild.Image(`${this.name}-${image}`, { dockerfile: { inline }, context: { location: '-' }, tags: [image], push: false, ...stored, buildArgs: { BUILDKIT_DOCKERFILE_CHECK: 'error=true' }, ...args }, { provider: this.builder }).digest;
    }

    kept(volume: string) {
        return new Volume(`${this.name}-${volume}`, { name: volume }, { provider: this.provider }).name;
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

const stored = (machine: Machine) => {
    const ports = { internal: 9000, external: 7412, health: '/health/ready' } as const;
    const { user, key } = Record.map({ user: 'storage-access', key: 'storage-secret' }, (name) => new RandomPassword(`${machine.name}-${name}`, { length: 32, special: false }).result);
    const published = Option.match(machine.remote, {
        onNone: () => ({ ips: output(['127.0.0.1']), address: '127.0.0.1' }),
        onSome: () => ({ ips: getDeviceOutput({ hostname: machine.name }).addresses.apply(Array.filter(isIPv4)), address: machine.name }),
    });
    const { address, named, labels } = machine.routed('storage', ports.internal);
    const serving = machine.contained(
        'storage',
        ['CMD', 'curl', '--fail', '--silent', `http://127.0.0.1:${ports.internal}${ports.health}`],
        {
            image: machine.pulled('rustfs/rustfs:1.0.6'),
            envs: [interpolate`RUSTFS_ACCESS_KEY=${user}`, interpolate`RUSTFS_SECRET_KEY=${key}`, 'RUSTFS_CONSOLE_ENABLE=false', 'RUSTFS_OBS_LOG_DIRECTORY='],
            ports: published.ips.apply(Array.map((ip) => ({ internal: ports.internal, external: ports.external, ip }))),
            ulimits: [{ name: 'nofile', soft: 1_048_576, hard: 1_048_576 }],
            mounts: [{ type: 'volume', source: machine.kept('storage'), target: '/data' }],
            labels,
        },
        { dependsOn: [proxied(machine), ...named] },
    );
    return output({ server: serving.urn.apply(() => `${published.address}:${ports.external}`), within: `http://storage:${ports.internal}`, origin: `https://${address}`, region: 'us-east-1', user, key });
};

const bucketed = (name: string, storage: Output<Stored>, buckets: Effect.Success<ReturnType<typeof deploy.Machine>>['buckets']) => {
    const provider = new minio.Provider(name, { minioServer: storage.server, minioUser: storage.user, minioPassword: storage.key });
    const s3 = new aws.Provider(name, {
        region: storage.region,
        accessKey: storage.user,
        secretKey: storage.key,
        endpoints: [{ s3: interpolate`http://${storage.server}` }],
        s3UsePathStyle: true,
        skipCredentialsValidation: true,
        skipMetadataApiCheck: true,
        skipRegionValidation: true,
        skipRequestingAccountId: true,
    });
    const created = Record.collect(buckets, (bucket, { kept, abort, expire, cors }) => {
        const made = new minio.S3Bucket(bucket, { bucket, forceDestroy: !kept }, { provider, protect: kept });
        const rules = [{ id: 'abort', abortIncompleteMultipartUpload: { daysAfterInitiation: Math.ceil(Duration.toDays(abort)) } }, ...Record.collect(expire, (prefix, age) => ({ id: `expire ${prefix}*`, ...(prefix === '' ? {} : { filter: { prefix } }), expiration: { days: Math.ceil(Duration.toDays(age)) } }))];
        const allowed = cors.map(({ origins, methods, headers, exposed, age }) => ({ allowedOrigins: [...origins], allowedMethods: [...methods], allowedHeaders: [...headers], exposeHeaders: [...exposed], maxAgeSeconds: Duration.toSeconds(age) }));
        const configured = [new minio.S3BucketLifecycle(bucket, { bucket: made.bucket, rules }, { provider }), ...(Array.isArrayNonEmpty(allowed) ? [new aws.s3.BucketCorsConfiguration(bucket, { bucket: made.bucket, corsRules: allowed }, { provider: s3 })] : [])];
        return { arn: made.arn, configured: configured.map(({ id }) => id) };
    });
    const policy = jsonStringify({ Version: '2012-10-17', Statement: [{ Effect: 'Allow', Action: ['s3:*'], Resource: created.flatMap(({ arn }) => [arn, interpolate`${arn}/*`]) }] });
    const account = new minio.IamServiceAccount(name, { targetUser: storage.user, policy }, { provider });
    return { AWS_ENDPOINT_URL_S3: storage.within, AWS_REGION: storage.region, AWS_ACCESS_KEY_ID: all([account.accessKey, ...created.flatMap(({ configured }) => configured)]).apply(([id]) => id), AWS_SECRET_ACCESS_KEY: account.secretKey };
};

const cached = (machine: Machine, { name, kept }: Data.TaggedEnum.Value<Role, 'Cache'>, project: string) => {
    const { port, health } = { port: 3000, health: '/health' };
    const storage = stored(machine);
    const [proxy, scoped] = [proxied(machine), bucketed(name, storage, { [name]: { kept: false, abort: kept, expire: { '': kept }, cors: [] } })];
    const tokens = Record.map({ NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN: 'READ_ONLY_ACCESS_TOKEN', NX_CACHE_WRITE_TOKEN: 'SERVICE_ACCESS_TOKEN' }, (variable, row) => ({ variable, value: new RandomPassword(`${machine.name}-${row}`, { length: 48, special: false }).result }));
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
    const variables = { S3_BUCKET_NAME: name, S3_ENDPOINT_URL: scoped.AWS_ENDPOINT_URL_S3, ...scoped, ...Record.mapEntries(tokens, ({ variable, value }) => [variable, value]) };
    const envs = Record.collect(variables, (variable, value) => interpolate`${variable}=${value}`);
    const serving = machine.contained(name, ['CMD', '/busybox/wget', '--quiet', '--spider', `http://127.0.0.1:${port}${health}`], { image, init: true, envs, labels }, { dependsOn: [proxy, ...named] });
    const held = { ...Record.map(tokens, Struct.get('value')), NX_SELF_HOSTED_REMOTE_CACHE_SERVER: serving.id.apply(() => `https://${address}`) };
    const ci = Record.map(held, (value, row) => new Secret(`${machine.name}-${row}`, { project, config: 'ci' satisfies (typeof configs)[number], name: row, value }));
    const dev = Record.map(Struct.omit(ci, ['NX_CACHE_WRITE_TOKEN']), ({ config, name: secret }, row) => new Secret(`${machine.name}-dev-${row}`, { project, config: 'dev' satisfies (typeof configs)[number], name: row, value: interpolate`\${${config}.${secret}}` }).name);
    const root = Record.map({ RUSTFS_ACCESS_KEY: storage.user, RUSTFS_SECRET_KEY: storage.key }, (value, row) => new Secret(`${machine.name}-${row}`, { project, config: 'dev' satisfies (typeof configs)[number], name: row, value }).name);
    return { ...Record.map(ci, Struct.get('name')), dev, root };
};

const hosted = (machine: Machine, { owner }: Data.TaggedEnum.Args<Role, 'Apps'>) => {
    const port = 9091;
    const [{ guard, forwarded }, wildcard] = [Machine, `*.${machine.zone}`];
    const [proxy, password] = [proxied(machine), new RandomPassword(`${machine.name}-owner`, { length: 32, special: false })];
    const { address, named, labels } = machine.routed(guard, port, {
        [`traefik.http.middlewares.${guard}-check.forwardauth.address`]: `http://${guard}:${port}/api/authz/forward-auth`,
        [`traefik.http.middlewares.${guard}-check.forwardauth.authResponseHeaders`]: Object.values(forwarded).join(','),
        ...Record.mapEntries(forwarded, (header) => [`traefik.http.middlewares.${guard}-strip.headers.customrequestheaders.${header}`, '']),
        [`traefik.http.middlewares.${guard}.chain.middlewares`]: `${guard}-strip,${guard}-check`,
    });
    const configuration = {
        access_control: { default_policy: 'bypass', rules: [{ domain: wildcard, resources: [`^${RegExp.escape(Identity.paths.signin)}([?].*)?$`], policy: 'one_factor' }] },
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
        { dependsOn: [proxy, ...named] },
    );
    return { network: output(machine.network), owner: { name: 'owner', password: password.result }, storage: stored(machine) };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const installed = Effect.sync(() => hosted(Machine.on(local.host, Option.none()), local));

const provisioned = (host: keyof typeof hosts, { owner, repository, project, engines, registry }: { readonly owner: string; readonly repository: string; readonly project: string; readonly engines: readonly string[]; readonly registry: Input<string> }) =>
    Effect.sync(() => {
        const registries = [{ address: 'ghcr.io', username: owner, password: registry }];
        const [{ address, roles }, machine]: readonly [Row, Machine] = [hosts[host], Machine.on(host, Option.none(), { registryAuth: registries })];
        const located = machine.named(host, 'A', address).map(({ content }) => content);
        const served = roles.map(
            Role.$match({
                Cache: (settings): Readonly<Record<string, unknown>> => cached(machine, settings, project),
                Apps: (settings) => hosted(machine, settings),
                Runner: ({ instances }) => {
                    const runner = { version: '2.338.2', sha256: 'baebf204307bac6193480852732f0aba2ae97b6c5597c244f4d5558390147c40', user: 'runner' } as const;
                    const [, registered] = Array.mapAccum(Record.toEntries(instances), Option.none<remote.Command>(), (before, [label, limits]) => {
                        const folder = `/home/${runner.user}/actions-runner-${label}`;
                        const archive = `/tmp/actions-runner-${label}.tar.gz`;
                        const removed = `if cd ${folder} 2>/dev/null; then systemctl revert "$(cat ${folder}/.service)"; ./svc.sh stop; ./svc.sh uninstall; fi; rm -rf ${folder}`;
                        const installation = [
                            'token=$(cat)',
                            removed,
                            'set -eu',
                            `install -d -o ${runner.user} -g ${runner.user} ${folder} && cd ${folder}`,
                            `curl -fsSL -o ${archive} https://github.com/actions/runner/releases/download/v${runner.version}/actions-runner-linux-x64-${runner.version}.tar.gz`,
                            `echo '${runner.sha256}  ${archive}' | sha256sum --check --quiet`,
                            `sudo -u ${runner.user} tar xzf ${archive} && rm ${archive}`,
                            './bin/installdependencies.sh',
                            `sudo -u ${runner.user} ./config.sh --unattended --replace --url https://github.com/${owner}/${repository} --name ${machine.name}-${label} --labels ${label} --token "$token"`,
                            `./svc.sh install ${runner.user}`,
                            './svc.sh start',
                            ...Array.match(
                                Record.collect(limits, (option, value) => `${option}=${value}`),
                                { onEmpty: Array.empty, onNonEmpty: (options) => [`systemctl set-property "$(cat .service)" ${options.join(' ')}`] },
                            ),
                        ].join('\n');
                        const token = new command.local.Command(
                            `${machine.name}-runner-${label}-token`,
                            { create: `gh api --method POST repos/${owner}/${repository}/actions/runners/registration-token --jq .token`, triggers: [installation, getDeviceOutput({ hostname: machine.name }).nodeId], logging: 'none' },
                            { additionalSecretOutputs: ['stdout'] },
                        );
                        const instance = new remote.Command(`${machine.name}-runner-${label}`, { connection: { host: machine.name }, addPreviousOutputInEnv: false, logging: 'stderr', create: installation, delete: removed, stdin: token.stdout }, { dependsOn: Option.toArray(before) });
                        return [Option.some(instance), [label, instance.id] as const];
                    });
                    return {
                        runners: Record.fromEntries(registered),
                        engines: engines.map((version) => machine.pulled(`ghcr.io/epicgames/unreal-engine:dev-slim-${version}`, { customTimeouts: { create: '2h', update: '2h' } })),
                        browsers: machine.imaged(
                            `ghcr.io/${owner}/${repository}/playwright:${playwright.version}`.toLowerCase(),
                            interpolate`FROM node:slim@${getRegistryImageOutput({ name: 'node:slim' }, { provider: machine.provider }).sha256Digest}\nENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright\nRUN apt-get update && apt-get install --yes --no-install-recommends build-essential ca-certificates curl git libicu-dev && npx --yes playwright@${playwright.version} install --with-deps chromium && npx --yes playwright@${playwright.version} install-deps firefox && rm -rf /var/lib/apt/lists/* /root/.npm\nUSER node\n`,
                            { registries, labels: { 'org.opencontainers.image.source': `https://github.com/${owner}/${repository}` } },
                        ),
                    };
                },
            }),
        );
        return { located, ...Record.fromEntries(served.flatMap(Record.toEntries)) };
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { bucketed, hosts, installed, local, Machine, provisioned, Role };
