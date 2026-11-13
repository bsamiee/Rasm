import { DnsRecord, getZoneOutput } from '@pulumi/cloudflare';
import { remote } from '@pulumi/command';
import { Container, type ContainerArgs, Network, Provider, type ProviderArgs, RemoteImage, Volume } from '@pulumi/docker';
import dockerBuild from '@pulumi/docker-build';
import { getActionsRegistrationTokenOutput } from '@pulumi/github';
import minio from '@pulumi/minio';
import { all, type CustomResourceOptions, getOrganization, getProject, type Input, interpolate, jsonStringify, type Output, output, StackReference, secret } from '@pulumi/pulumi';
import { RandomPassword } from '@pulumi/random';
import { Secret } from '@pulumiverse/doppler';
import { Access } from '@rasm/hosting';
import { Sites } from '@rasm/hosting/openai-sites';
import { Array, Data, Duration, Effect, Option, Record, Schema, Struct } from 'effect';
import type { configs } from '#/repository-stack';

// --- [TYPES] ---------------------------------------------------------------------------

type Role = Data.TaggedEnum<{
    Cache: { readonly name: string; readonly kept: Duration.Duration };
    Runner: Record.ReadonlyRecord<never, never>;
    Apps: { readonly owner: string };
}>;
interface Row {
    readonly address: string;
    readonly tags: readonly `tag:${string}`[];
    readonly role: Role;
}

// --- [TABLES] --------------------------------------------------------------------------

const hosts = {
    'lucky.parametric-arsenal.cloud': { address: '31.97.131.41', tags: ['tag:server'], role: { _tag: 'Cache', name: 'nx-cache', kept: Duration.weeks(1) } },
    'derby.parametric-arsenal.cloud': { address: '45.90.223.193', tags: ['tag:server'], role: { _tag: 'Runner' } },
} as const satisfies Record.ReadonlyRecord<string, Row>;
const local = { host: 'local.rasm.localhost', owner: 'owner@rasm.localhost' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Role = Data.taggedEnum<Role>();
const Stored = Schema.Struct(Struct.Record(['server', 'within', 'region', 'user', 'key'], Schema.String));

// --- [SERVICES] ------------------------------------------------------------------------

class Machine extends Data.Class<{
    readonly name: string;
    readonly zone: string;
    readonly provider: Provider;
    readonly builder: dockerBuild.Provider;
    readonly network: Input<string>;
    readonly remote: Option.Option<{ readonly address: string; readonly zoneId: Output<string> }>;
}> {
    // --- [CONSTRUCTION]

    static readonly on = (host: string, network: Option.Option<Input<string>>, registries: Pick<ProviderArgs, 'registryAuth'> = {}) => {
        const [name = host, ...labels] = host.split('.');
        const zone = labels.join('.');
        const reached = Option.map(Record.get<string, Row>(hosts, host), ({ address }) => ({ address, zoneId: getZoneOutput({ filter: { name: zone } }).zoneId }));
        const dialed = Option.getOrUndefined(Option.map(reached, () => ({ host: `ssh://root@${name}` })));
        const provider = new Provider(host, { ...dialed, ...registries, sshOpts: ['-o', 'StrictHostKeyChecking=no', '-o', 'UserKnownHostsFile=/dev/null'] });
        return new Machine({ name, zone, provider, builder: new dockerBuild.Provider(host, { ...dialed }), network: Option.getOrElse(network, () => new Network(host, { name: getProject() }, { provider }).name), remote: reached });
    };

    static readonly joined = (host: string) => {
        const reference = new StackReference(`${getOrganization()}/${getProject()}/${host}`);
        return { machine: Machine.on(host, Option.some(reference.requireOutput('network'))), storage: reference.requireOutput('storage').apply(Schema.decodeUnknownPromise(Stored)) };
    };

    // --- [ROUTING]

    static readonly entrypoint = 'websecure';
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

    pulled(image: string, options: CustomResourceOptions = {}) {
        return new RemoteImage(image, { name: image }, { provider: this.provider, ...options }).repoDigest;
    }

    imaged(image: string, inline: string, args: Pick<dockerBuild.ImageArgs, 'context' | 'buildArgs'> = {}) {
        return new dockerBuild.Image(image, { dockerfile: { inline }, tags: [image], load: true, push: false, ...args }, { provider: this.builder }).ref;
    }

    kept(volume: string) {
        return new Volume(volume, { name: volume }, { provider: this.provider }).name;
    }

    contained(service: string, tests: readonly string[], args: Omit<ContainerArgs, 'name' | 'networksAdvanced' | 'restart' | 'logDriver' | 'wait' | 'healthcheck'>, options: CustomResourceOptions = {}) {
        return new Container(service, { name: service, networksAdvanced: [{ name: this.network }], restart: 'unless-stopped', logDriver: 'local', wait: true, waitTimeout: 300, destroyGraceSeconds: 120, healthcheck: { tests: [...tests], interval: '10s' }, ...args }, { provider: this.provider, ...options });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const proxied = (machine: Machine) => {
    const secure = Machine.entrypoint.toUpperCase();
    const listened = { WEB: 80, [secure]: 443 };
    return machine.contained('traefik', ['CMD', 'traefik', 'healthcheck'], {
        image: machine.pulled('traefik:v3.8.0-rc.2'),
        envs: [
            ...Record.collect(listened, (name, port) => `TRAEFIK_ENTRYPOINTS_${name}_ADDRESS=:${port}`),
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
        ports: Object.values(listened).map((port) => ({ internal: port, external: port })),
        mounts: [
            { type: 'bind', source: '/var/run/docker.sock', target: '/var/run/docker.sock', readOnly: true },
            { type: 'volume', source: machine.kept('traefik'), target: '/var/lib/traefik' },
        ],
    });
};

const stored = (machine: Machine) => {
    const ports = { internal: 9000, external: 7412 };
    const { user, key } = Record.map({ user: 'storage-access', key: 'storage-secret' }, (name) => new RandomPassword(name, { length: 32, special: false }).result);
    const published = Option.match(machine.remote, { onNone: () => ({ ip: '127.0.0.1', address: '127.0.0.1' }), onSome: ({ address }) => ({ ip: '0.0.0.0', address }) });
    const serving = machine.contained('storage', ['CMD', 'curl', '--fail', '--silent', `http://127.0.0.1:${ports.internal}/health/ready`], {
        image: machine.pulled('rustfs/rustfs:1.0.5'),
        envs: [interpolate`RUSTFS_ACCESS_KEY=${user}`, interpolate`RUSTFS_SECRET_KEY=${key}`, 'RUSTFS_CONSOLE_ENABLE=false', 'RUSTFS_OBS_LOG_DIRECTORY='],
        ports: [{ ...ports, ip: published.ip }],
        ulimits: [{ name: 'nofile', soft: 1_048_576, hard: 1_048_576 }],
        mounts: [{ type: 'volume', source: machine.kept('storage'), target: '/data' }],
    });
    return output({ server: serving.id.apply(() => `${published.address}:${ports.external}`), within: `http://storage:${ports.internal}`, region: 'us-east-1', user, key });
};

const bucketed = (name: string, storage: Output<typeof Stored.Type>, buckets: Readonly<Record<string, { readonly kept: boolean; readonly expire: Readonly<Record<string, Duration.Duration>> }>>) => {
    const provider = new minio.Provider(name, { minioServer: storage.server, minioUser: storage.user, minioPassword: storage.key });
    const created = Record.collect(buckets, (bucket, { kept, expire }) => {
        const made = new minio.S3Bucket(bucket, { bucket, forceDestroy: !kept }, { provider, protect: kept });
        const rules = Record.collect(expire, (filter, age) => ({ id: `expire ${filter}*`, filter, expiration: `${Math.ceil(Duration.toDays(age))}d` }));
        return { arn: made.arn, expiry: new minio.IlmPolicy(bucket, { bucket: made.bucket, rules }, { provider }).id };
    });
    const policy = jsonStringify({ Version: '2012-10-17', Statement: [{ Effect: 'Allow', Action: ['s3:*'], Resource: created.flatMap(({ arn }) => [arn, interpolate`${arn}/*`]) }] });
    const account = new minio.IamServiceAccount(name, { targetUser: storage.user, policy }, { provider });
    return { AWS_ENDPOINT_URL_S3: storage.within, AWS_REGION: storage.region, AWS_ACCESS_KEY_ID: all([account.accessKey, ...created.map(({ expiry }) => expiry)]).apply(([id]) => id), AWS_SECRET_ACCESS_KEY: account.secretKey };
};

const cached = (machine: Machine, { name, kept }: Data.TaggedEnum.Value<Role, 'Cache'>, project: string) => {
    const port = 3000;
    const [proxy, scoped] = [proxied(machine), bucketed(name, stored(machine), { [name]: { kept: false, expire: { '': kept } } })];
    const tokens = Record.map({ NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN: 'READ_ONLY_ACCESS_TOKEN', NX_CACHE_WRITE_TOKEN: 'SERVICE_ACCESS_TOKEN' }, (variable, row) => ({ variable, value: new RandomPassword(row, { length: 48, special: false }).result }));
    const { address, named, labels } = machine.routed(name, port, { [`traefik.http.services.${name}.loadbalancer.healthcheck.path`]: '/health' });
    const image = machine.imaged(
        name,
        `# syntax=docker/dockerfile:1
FROM scratch AS amd64
ADD --checksum=sha256:18fa55033d5f0d1b18fc192b75ca35f39d2cb81938bbc559f7a9f5b686f4046f --chmod=755 https://github.com/nxcite/nx-cache-server/releases/download/v1.4.5/nx-cache-aws-v1.4.5-linux-x86_64 /nx-cache-aws
FROM scratch AS arm64
ADD --checksum=sha256:a20a6bdee0b869d7e8ade7319030f94b72c69358b47f08fbc3bd331d9dc66e5c --chmod=755 https://github.com/nxcite/nx-cache-server/releases/download/v1.4.5/nx-cache-aws-v1.4.5-linux-arm64 /nx-cache-aws
FROM \${TARGETARCH}
FROM gcr.io/distroless/cc-debian13:debug-nonroot
COPY --from=2 /nx-cache-aws /
ENTRYPOINT ["/nx-cache-aws"]
`,
    );
    const variables = { S3_BUCKET_NAME: name, S3_ENDPOINT_URL: scoped.AWS_ENDPOINT_URL_S3, ...scoped, ...Record.mapEntries(tokens, ({ variable, value }) => [variable, value]) };
    const envs = Record.collect(variables, (variable, value) => interpolate`${variable}=${value}`);
    const serving = machine.contained(name, ['CMD', '/busybox/wget', '--quiet', '--spider', `http://127.0.0.1:${port}/health`], { image, init: true, envs, labels }, { dependsOn: [proxy, ...named] });
    const held = { ...Record.map(tokens, Struct.get('value')), NX_SELF_HOSTED_REMOTE_CACHE_SERVER: serving.id.apply(() => `https://${address}`) };
    return Record.map(held, (value, row) => new Secret(row, { project, config: 'ci' satisfies (typeof configs)[number], name: row, value }).name);
};

const hosted = (machine: Machine, { owner }: Data.TaggedEnum.Args<Role, 'Apps'>) => {
    const port = 9091;
    const [{ guard, forwarded }, wildcard] = [Machine, `*.${machine.zone}`];
    const [proxy, password] = [proxied(machine), new RandomPassword('owner', { length: 32, special: false })];
    const { address, named, labels } = machine.routed(guard, port, {
        [`traefik.http.middlewares.${guard}-check.forwardauth.address`]: `http://${guard}:${port}/api/authz/forward-auth`,
        [`traefik.http.middlewares.${guard}-check.forwardauth.authResponseHeaders`]: Object.values(forwarded).join(','),
        ...Record.mapEntries(forwarded, (header) => [`traefik.http.middlewares.${guard}-strip.headers.customrequestheaders.${header}`, '']),
        [`traefik.http.middlewares.${guard}.chain.middlewares`]: `${guard}-strip,${guard}-check`,
    });
    const configuration = {
        access_control: {
            default_policy: 'one_factor',
            rules: [
                { domain: wildcard, methods: ['POST', 'PUT', 'PATCH', 'DELETE'], policy: 'one_factor' },
                { domain: wildcard, resources: [`^${RegExp.escape(Sites.identity.paths.signin)}([?].*)?$`], policy: 'one_factor' },
                { domain: wildcard, policy: 'bypass' },
            ],
        },
        authentication_backend: { file: { path: '/config/users.yml' }, password_reset: { disable: true } },
        notifier: { filesystem: { filename: '/var/lib/authelia/notification.txt' } },
        ntp: { disable_startup_check: true },
        session: { cookies: [{ domain: machine.zone, authelia_url: `https://${address}` }] },
        storage: { local: { path: '/var/lib/authelia/db.sqlite3' } },
    };
    machine.contained(
        guard,
        ['CMD', '/app/healthcheck.sh'],
        {
            image: machine.pulled('authelia/authelia:4.39.28'),
            envs: Array.map(['AUTHELIA_SESSION_SECRET', 'AUTHELIA_STORAGE_ENCRYPTION_KEY'], (row) => interpolate`${row}=${new RandomPassword(row, { length: 64, special: false }).result}`),
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
        const [{ address, role }, machine] = [hosts[host], Machine.on(host, Option.none(), { registryAuth: [{ address: 'ghcr.io', username: owner, password: registry }] })];
        const located = machine.named(host, 'A', address).map(({ content }) => content);
        const served = Role.$match(role, {
            Cache: (settings) => cached(machine, settings, project),
            Apps: (settings) => hosted(machine, settings),
            Runner: () => {
                const runner = { version: '2.338.2', sha256: 'baebf204307bac6193480852732f0aba2ae97b6c5597c244f4d5558390147c40', user: 'runner', uid: 1000 } as const;
                const folder = `/home/${runner.user}/actions-runner`;
                const registered = new remote.Command(
                    'runner',
                    {
                        connection: { host: machine.name },
                        addPreviousOutputInEnv: false,
                        create: [
                            'set -eu',
                            'token=$(cat)',
                            `id ${runner.user} >/dev/null 2>&1 || useradd --create-home --uid ${runner.uid} --shell /bin/bash --groups docker ${runner.user}`,
                            `echo '${runner.user} ALL=(ALL) NOPASSWD:ALL' >/etc/sudoers.d/${runner.user}`,
                            `install -d -o ${runner.user} -g ${runner.user} ${folder} && cd ${folder}`,
                            `curl -fsSL -o /tmp/runner.tar.gz https://github.com/actions/runner/releases/download/v${runner.version}/actions-runner-linux-x64-${runner.version}.tar.gz`,
                            `echo '${runner.sha256}  /tmp/runner.tar.gz' | sha256sum --check --quiet`,
                            `sudo -u ${runner.user} tar xzf /tmp/runner.tar.gz && rm /tmp/runner.tar.gz`,
                            './bin/installdependencies.sh',
                            `sudo -u ${runner.user} ./config.sh --unattended --replace --url https://github.com/${owner}/${repository} --name ${machine.name} --token "$token"`,
                            `./svc.sh install ${runner.user} && ./svc.sh start`,
                        ].join('\n'),
                        delete: `cd ${folder} && ./svc.sh stop; ./svc.sh uninstall; cd / && userdel --remove ${runner.user}; rm -f /etc/sudoers.d/${runner.user}`,
                        stdin: secret(getActionsRegistrationTokenOutput({ repository }).token),
                        logging: 'stderr',
                    },
                    { ignoreChanges: ['stdin'], deleteBeforeReplace: true },
                );
                return { runner: registered.id, engines: engines.map((version) => machine.pulled(`ghcr.io/epicgames/unreal-engine:dev-slim-${version}`, { customTimeouts: { create: '2h', update: '2h' } })) };
            },
        });
        return { located, ...served };
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { bucketed, hosts, installed, local, Machine, provisioned, Role };
