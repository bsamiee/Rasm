import { DnsRecord, getZoneOutput } from '@pulumi/cloudflare';
import { remote } from '@pulumi/command';
import { Container, type ContainerArgs, Network, Provider, type ProviderArgs, RemoteImage, Volume } from '@pulumi/docker';
import dockerBuild from '@pulumi/docker-build';
import { getActionsRegistrationTokenOutput } from '@pulumi/github';
import minio from '@pulumi/minio';
import { all, type CustomResourceOptions, getOrganization, getProject, type Input, interpolate, jsonStringify, type Output, output, type Resource, StackReference, secret } from '@pulumi/pulumi';
import { RandomPassword } from '@pulumi/random';
import { Secret } from '@pulumiverse/doppler';
import { Access } from '@rasm/hosting';
import { Sites } from '@rasm/hosting/openai-sites';
import { Array, ByteSize, Data, Duration, Effect, Function, Option, Record, Schema, Struct } from 'effect';
import type { configs } from '#/repository-stack';

// --- [TYPES] ---------------------------------------------------------------------------

type Role = Data.TaggedEnum<{
    Cache: { readonly name: string; readonly kept: Duration.Duration };
    Runner: { readonly packages: readonly string[]; readonly instances: Readonly<Record<string, Readonly<Record<string, string>>>> };
    Apps: { readonly owner: string };
    Notify: { readonly name: string; readonly memory: ByteSize.ByteSize };
    Watch: { readonly memory: ByteSize.ByteSize };
}>;
interface Row {
    readonly address: string;
    readonly tags: readonly `tag:${string}`[];
    readonly roles: readonly Role[];
}
interface Setting {
    readonly content: Input<string>;
    readonly applied: readonly string[];
}

// --- [TABLES] --------------------------------------------------------------------------

const hosts = {
    'lucky.parametric-arsenal.cloud': {
        address: '31.97.131.41',
        tags: ['tag:server'],
        roles: [
            { _tag: 'Cache', name: 'nx-cache', kept: Duration.weeks(1) },
            { _tag: 'Notify', name: 'ntfy', memory: ByteSize.fromInputUnsafe('256 MiB') },
            { _tag: 'Watch', memory: ByteSize.fromInputUnsafe('256 MiB') },
        ],
    },
    'derby.parametric-arsenal.cloud': {
        address: '45.90.223.193',
        tags: ['tag:server'],
        roles: [
            { _tag: 'Runner', packages: ['build-essential', 'libsqlite3-dev', 'pkg-config', 'sqlite3', 'unzip', 'zip'], instances: { host: { MemoryHigh: '18G', MemoryMax: '20G' }, container: {} } },
            { _tag: 'Watch', memory: ByteSize.fromInputUnsafe('256 MiB') },
        ],
    },
} as const satisfies Record.ReadonlyRecord<string, Row>;
const tailnet = 'tailscale0';
const gated = { storage: { internal: 9000, external: 7412, health: '/health/ready' }, gatus: { internal: 8080, external: 8080, health: '/health' } } as const;
const routed = { Cache: { port: 3000, health: '/health' }, Notify: { port: 80, health: '/v1/health' } } as const;
const files = {
    '/etc/docker/daemon.json': { content: `${JSON.stringify({ 'live-restore': true, 'log-driver': 'local' })}\n`, applied: ['systemctl restart docker'] },
    '/etc/systemd/zram-generator.conf': {
        content: '[zram0]\nzram-size = ram / 4\ncompression-algorithm = zstd\n',
        applied: ['DEBIAN_FRONTEND=noninteractive apt-get install --quiet --yes systemd-zram-generator', 'systemctl daemon-reload', 'systemctl start dev-zram0.swap'],
    },
    '/etc/systemd/journald.conf.d/90-platform.conf': { content: '[Journal]\nSystemMaxUse=1G\n', applied: ['systemctl restart systemd-journald'] },
    '/etc/ssh/sshd_config.d/00-platform.conf': { content: 'PasswordAuthentication no\n', applied: ['systemctl try-reload-or-restart ssh'] },
    '/etc/apt/apt.conf.d/52unattended-upgrades-local': { content: 'Unattended-Upgrade::Origins-Pattern { "origin=Ubuntu,archive=*-updates"; "origin=Docker"; };\nUnattended-Upgrade::Automatic-Reboot "true";\n', applied: [] },
    '/etc/systemd/system/apt-daily-upgrade.timer.d/90-platform.conf': { content: '[Timer]\nOnCalendar=\nOnCalendar=*-*-* 05:00 America/Chicago\n', applied: ['systemctl daemon-reload', 'systemctl restart apt-daily-upgrade.timer'] },
    '/etc/ufw/after.rules': {
        content: [
            '*filter',
            ':ufw-after-input - [0:0]',
            ':ufw-after-output - [0:0]',
            ':ufw-after-forward - [0:0]',
            ':DOCKER-USER - [0:0]',
            ...Object.values(gated).map(({ external }) => `-A DOCKER-USER ! -i ${tailnet} -p tcp -m conntrack --ctstate DNAT --ctorigdstport ${external} --ctdir ORIGINAL -j DROP`),
            'COMMIT',
            '',
        ].join('\n'),
        applied: ['ufw reload'],
    },
} as const satisfies Record.ReadonlyRecord<string, Setting>;
const firewall = [`ufw allow in on ${tailnet}`, 'ufw default deny incoming', 'ufw default allow outgoing', 'ufw --force enable'] as const;
const local = { host: 'local.rasm.localhost', owner: 'owner@rasm.localhost' } as const;
const accounts = {
    owner: { role: 'admin', topics: {} },
    phone: { role: 'user', topics: { 'agent-*': 'ro', infra: 'ro', reply: 'wo', ops: 'wo' } },
    gatus: { role: 'user', topics: { infra: 'wo' } },
    button: { role: 'user', topics: { reply: 'wo', ops: 'wo', sessions: 'ro', inbox: 'wo' } },
    responder: { role: 'user', topics: { infra: 'rw', ops: 'ro' } },
} as const;
const jobs: Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, Option.Option<Duration.Duration>>> = {
    jobs: { maintenance: Option.some(Duration.fromInputUnsafe('170 hours')), deploy: Option.none() },
    'mac-jobs': { 'mac-maintenance': Option.some(Duration.fromInputUnsafe('170 hours')), redeploy: Option.none() },
    renders: { darkroom: Option.none(), blender: Option.none() },
};

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
    readonly settled: readonly Resource[];
}> {
    // --- [CONSTRUCTION]

    static readonly on = (host: string, network: Option.Option<Input<string>>, registries: Pick<ProviderArgs, 'registryAuth'> = {}, settings: Record.ReadonlyRecord<string, Setting> = {}) => {
        const [name = host, ...labels] = host.split('.');
        const zone = labels.join('.');
        const reached = Option.map(Record.get<string, Row>(hosts, host), ({ address }) => ({ address, zoneId: getZoneOutput({ filter: { name: zone } }).zoneId }));
        const dialed = Option.getOrUndefined(Option.map(reached, () => ({ host: `ssh://root@${name}` })));
        const settled = Record.collect(settings, (file, setting) => Machine.written(name, file, setting));
        const provider = new Provider(host, { ...dialed, ...registries, sshOpts: ['-o', 'StrictHostKeyChecking=no', '-o', 'UserKnownHostsFile=/dev/null'] }, { dependsOn: settled });
        const builder = new dockerBuild.Provider(host, { ...dialed }, { dependsOn: settled });
        return new Machine({ name, zone, provider, builder, network: Option.getOrElse(network, () => new Network(host, { name: getProject() }, { provider }).name), remote: reached, settled });
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
        return new RemoteImage(`${this.name}-${image}`, { name: image }, { provider: this.provider, ...options }).repoDigest;
    }

    imaged(image: string, inline: string, args: Pick<dockerBuild.ImageArgs, 'context' | 'buildArgs'> = {}) {
        return new dockerBuild.Image(`${this.name}-${image}`, { dockerfile: { inline }, context: { location: '-' }, tags: [image], load: true, push: false, ...args }, { provider: this.builder }).ref;
    }

    kept(volume: string) {
        return new Volume(`${this.name}-${volume}`, { name: volume }, { provider: this.provider }).name;
    }

    contained(service: string, tests: readonly string[], args: Omit<ContainerArgs, 'name' | 'networksAdvanced' | 'restart' | 'logDriver' | 'wait' | 'healthcheck'>, options: CustomResourceOptions = {}) {
        return new Container(
            `${this.name}-${service}`,
            { name: service, networksAdvanced: [{ name: this.network }], restart: 'unless-stopped', logDriver: 'local', wait: true, waitTimeout: 300, destroyGraceSeconds: 120, healthcheck: { tests: [...tests], interval: '10s' }, ...args },
            { provider: this.provider, ...options },
        );
    }

    // --- [SETTINGS]

    static ran(name: string, step: string, args: Omit<remote.CommandArgs, 'connection' | 'addPreviousOutputInEnv' | 'logging'>, options: CustomResourceOptions = {}) {
        return new remote.Command(`${name}-${step}`, { connection: { host: name }, addPreviousOutputInEnv: false, logging: 'stderr', ...args }, options);
    }

    static written(name: string, file: string, { content, applied }: Setting, options: CustomResourceOptions = {}) {
        return Machine.ran(name, file, { create: [`install -D -m 0644 /dev/stdin ${file}`, ...applied].join(' && '), delete: [`rm -f ${file}`, ...applied].join(' && '), stdin: content }, options);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const proxied = Function.memoize((machine: Machine) => {
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
});

const stored = (machine: Machine) => {
    const ports = gated.storage;
    const { user, key } = Record.map({ user: 'storage-access', key: 'storage-secret' }, (name) => new RandomPassword(`${machine.name}-${name}`, { length: 32, special: false }).result);
    const published = Option.match(machine.remote, { onNone: () => ({ ip: '127.0.0.1', address: '127.0.0.1' }), onSome: () => ({ ip: '0.0.0.0', address: machine.name }) });
    const serving = machine.contained('storage', ['CMD', 'curl', '--fail', '--silent', `http://127.0.0.1:${ports.internal}${ports.health}`], {
        image: machine.pulled('rustfs/rustfs:1.0.5'),
        envs: [interpolate`RUSTFS_ACCESS_KEY=${user}`, interpolate`RUSTFS_SECRET_KEY=${key}`, 'RUSTFS_CONSOLE_ENABLE=false', 'RUSTFS_OBS_LOG_DIRECTORY='],
        ports: [{ internal: ports.internal, external: ports.external, ip: published.ip }],
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
    const { port, health } = routed.Cache;
    const [proxy, scoped] = [proxied(machine), bucketed(name, stored(machine), { [name]: { kept: false, expire: { '': kept } } })];
    const tokens = Record.map({ NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN: 'READ_ONLY_ACCESS_TOKEN', NX_CACHE_WRITE_TOKEN: 'SERVICE_ACCESS_TOKEN' }, (variable, row) => ({ variable, value: new RandomPassword(`${machine.name}-${row}`, { length: 48, special: false }).result }));
    const { address, named, labels } = machine.routed(name, port, { [`traefik.http.services.${name}.loadbalancer.healthcheck.path`]: health });
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
    const serving = machine.contained(name, ['CMD', '/busybox/wget', '--quiet', '--spider', `http://127.0.0.1:${port}${health}`], { image, init: true, envs, labels }, { dependsOn: [proxy, ...named] });
    const held = { ...Record.map(tokens, Struct.get('value')), NX_SELF_HOSTED_REMOTE_CACHE_SERVER: serving.id.apply(() => `https://${address}`) };
    return Record.map(held, (value, row) => new Secret(`${machine.name}-${row}`, { project, config: 'ci' satisfies (typeof configs)[number], name: row, value }).name);
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

const credentialed = Function.memoize((machine: Machine) =>
    Record.map(accounts, (_, user) => ({
        password: new RandomPassword(`${machine.name}-${user}-password`, { length: 32, special: false }),
        token: new RandomPassword(`${machine.name}-${user}-token`, { length: 29, special: false, upper: false }).result.apply((drawn) => `tk_${drawn}`),
    })),
);

const notified = (machine: Machine, { name, memory }: Data.TaggedEnum.Value<Role, 'Notify'>) => {
    const { port, health } = routed.Notify;
    const [proxy, credentials, folder] = [proxied(machine), credentialed(machine), '/var/lib/ntfy'];
    const { address, named, labels } = machine.routed(name, port);
    const configuration = {
        'base-url': `https://${address}`,
        'behind-proxy': true,
        'upstream-base-url': 'https://ntfy.sh',
        'cache-file': `${folder}/cache.db`,
        'cache-duration': '24h',
        'attachment-cache-dir': `${folder}/attachments`,
        'attachment-file-size-limit': '50M',
        'attachment-expiry-duration': '24h',
        'visitor-attachment-total-size-limit': '1G',
        'auth-file': `${folder}/user.db`,
        'auth-default-access': 'deny-all',
        'auth-users': Record.collect(accounts, (user, { role }) => interpolate`${user}:${credentials[user].password.bcryptHash}:${role}`),
        'auth-access': Record.collect(accounts, (user, { topics }) => Record.collect<string, string, string>(topics, (topic, access) => [user, topic, access].join(':'))).flat(),
        'auth-tokens': Record.collect(credentials, (user, { token }) => interpolate`${user}:${token}`),
        'enable-login': true,
        'require-login': true,
    };
    const serving = machine.contained(
        name,
        ['CMD', 'wget', '--quiet', '--spider', `http://127.0.0.1:${port}${health}`],
        {
            image: machine.pulled('binwiederhier/ntfy:v2.38.0'),
            command: ['serve'],
            memory: ByteSize.toUnit(memory, 'MiB'),
            memorySwap: ByteSize.toUnit(memory, 'MiB'),
            uploads: [{ file: '/etc/ntfy/server.yml', content: jsonStringify(configuration) }],
            mounts: [{ type: 'volume', source: machine.kept(name), target: folder }],
            labels,
        },
        { dependsOn: [proxy, ...named] },
    );
    return { notify: { url: serving.id.apply(() => `https://${address}`), accounts: Record.map(credentials, ({ password, token }) => ({ password: password.result, token })) } };
};

const watched = (machine: Machine, host: keyof typeof hosts, { memory }: Data.TaggedEnum.Value<Role, 'Watch'>) => {
    const [{ internal, external, health }, service, apt] = [gated.gatus, 'gatus', { group: 'jobs', name: 'apt', every: Option.some(Duration.fromInputUnsafe('26 hours')) }];
    const push = new RandomPassword(`${machine.name}-heartbeat`, { length: 48, special: false }).result;
    const notifier = Array.findFirst<Role, Data.TaggedEnum.Value<Role, 'Notify'>>(hosts[host].roles, Role.$is('Notify'));
    const alerting = Option.match(notifier, {
        onNone: () => ({ topic: new RandomPassword(`${machine.name}-fallback`, { length: 32, special: false, upper: false }).result, priority: 5 }),
        onSome: ({ name }) => ({ topic: output('infra' satisfies keyof typeof accounts.gatus.topics), priority: 4, url: `http://${name}:${routed.Notify.port}`, token: credentialed(machine).gatus.token }),
    });
    const [answered, probed, reported] = [['[STATUS] == 200'], [{ type: 'ntfy' }], [{ type: 'ntfy', 'failure-threshold': 1, 'success-threshold': 1 }]];
    const endpoints = Record.collect(hosts, (named, { roles }: Row) => {
        const [group = named, ...labels] = named.split('.');
        const within = (name: string, served: { readonly port: number; readonly health: string }) => ({ group, name, url: `http://${name}:${served.port}${served.health}`, conditions: answered, alerts: probed });
        const storage = within('storage', { port: gated.storage.internal, health: gated.storage.health });
        const reached = (name: string, path: string) => ({ group, name, url: `https://${name}.${labels.join('.')}${path}`, conditions: [...answered, '[CERTIFICATE_EXPIRATION] > 240h'], alerts: probed });
        return roles.flatMap(
            Role.$match({
                Cache: ({ name }) => (named === host ? [within(name, routed.Cache), storage] : [reached(name, routed.Cache.health)]),
                Apps: () => (named === host ? [storage] : []),
                Notify: ({ name }) => [named === host ? within(name, routed.Notify) : reached(name, routed.Notify.health)],
                Watch: () => (named === host ? [] : [{ group, name: service, url: `http://${group}:${external}${health}`, client: { 'dns-resolver': 'udp://100.100.100.100:53' }, conditions: answered, alerts: probed }]),
                Runner: () => [],
            }),
        );
    }).flat();
    const pushed = [apt, ...(Option.isSome(notifier) ? Record.collect(jobs, (group, rows) => Record.collect(rows, (name, every) => ({ group, name, every }))).flat() : [])];
    const configuration = {
        storage: { type: 'sqlite', path: '/data/data.db' },
        alerting: { ntfy: { ...alerting, 'default-alert': { 'send-on-resolved': true } } },
        endpoints,
        'external-endpoints': pushed.map(({ every, ...row }) => ({ ...row, token: push, alerts: reported, ...Record.getSomes({ heartbeat: Option.map(every, (interval) => ({ interval: `${Duration.toHours(interval)}h` })) }) })),
    };
    const serving = machine.contained(service, ['CMD', '/busybox/wget', '--quiet', '--spider', `http://127.0.0.1:${internal}${health}`], {
        image: machine.imaged(service, 'FROM ghcr.io/twin/gatus:v5.38.0 AS gatus\nFROM gcr.io/distroless/static-debian13:debug\nCOPY --from=gatus /gatus /gatus\nENTRYPOINT ["/gatus"]\n'),
        memory: ByteSize.toUnit(memory, 'MiB'),
        memorySwap: ByteSize.toUnit(memory, 'MiB'),
        uploads: [{ file: '/config/config.yaml', content: jsonStringify(configuration) }],
        mounts: [{ type: 'volume', source: machine.kept(service), target: '/data' }],
        ports: [{ internal, external }],
    });
    Machine.written(
        machine.name,
        '/etc/systemd/system/apt-daily-upgrade.service.d/90-heartbeat.conf',
        {
            content: interpolate`[Service]\nExecStopPost=-/bin/sh -c 'curl --fail --silent --show-error --max-time 10 --request POST --header "Authorization: Bearer ${push}" "http://127.0.0.1:${external}/api/v1/endpoints/${apt.group}_${apt.name}/external?success=$$([ "$$SERVICE_RESULT" = success ] && echo true || echo false)&error=$$SERVICE_RESULT"'\n`,
            applied: ['systemctl daemon-reload'],
        },
        { dependsOn: serving },
    );
    return { watch: { url: serving.id.apply(() => `http://${machine.name}:${external}`), heartbeat: push, topic: alerting.topic } };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const installed = Effect.sync(() => hosted(Machine.on(local.host, Option.none()), local));

const provisioned = (host: keyof typeof hosts, { owner, repository, project, engines, registry }: { readonly owner: string; readonly repository: string; readonly project: string; readonly engines: readonly string[]; readonly registry: Input<string> }) =>
    Effect.sync(() => {
        const [{ address, roles }, machine]: readonly [Row, Machine] = [hosts[host], Machine.on(host, Option.none(), { registryAuth: [{ address: 'ghcr.io', username: owner, password: registry }] }, files)];
        const located = machine.named(host, 'A', address).map(({ content }) => content);
        const served = roles.map(
            Role.$match({
                Cache: (settings): Readonly<Record<string, unknown>> => cached(machine, settings, project),
                Apps: (settings) => hosted(machine, settings),
                Notify: (settings) => notified(machine, settings),
                Watch: (settings) => watched(machine, host, settings),
                Runner: ({ packages, instances }) => {
                    const runner = { version: '2.338.2', sha256: 'baebf204307bac6193480852732f0aba2ae97b6c5597c244f4d5558390147c40', user: 'runner', uid: 1000 } as const;
                    const token = secret(getActionsRegistrationTokenOutput({ repository }).token);
                    const prepared = [
                        Machine.ran(machine.name, 'packages', { create: `apt-get update --quiet && DEBIAN_FRONTEND=noninteractive apt-get install --quiet --yes ${packages.join(' ')}` }),
                        Machine.ran(machine.name, 'user', { create: `id ${runner.user} >/dev/null 2>&1 || useradd --create-home --uid ${runner.uid} --shell /bin/bash --groups docker ${runner.user}`, delete: `userdel --remove ${runner.user}` }),
                        Machine.written(machine.name, `/etc/sudoers.d/90-${runner.user}`, { content: `${runner.user} ALL=(ALL) NOPASSWD:ALL\n`, applied: [] }),
                        Machine.written(machine.name, '/etc/needrestart/conf.d/actions_runner_services.conf', { content: '$nrconf{override_rc}{qr(^actions\\.runner\\..+\\.service$)} = 0;\n', applied: [] }),
                    ];
                    const [, registered] = Array.mapAccum(Record.toEntries(instances), prepared, (before, [label, limits]) => {
                        const folder = `/home/${runner.user}/actions-runner-${label}`;
                        const archive = `/tmp/actions-runner-${label}.tar.gz`;
                        const unit = `"$(cat ${folder}/.service)"`;
                        const removed = `if cd ${folder} 2>/dev/null; then systemctl revert ${unit}; ./svc.sh stop; ./svc.sh uninstall; fi; rm -rf ${folder}`;
                        const limited = [
                            `systemctl revert ${unit}`,
                            ...Array.match(
                                Record.collect(limits, (option, value) => [option, value].join('=')),
                                { onEmpty: () => [], onNonEmpty: (options) => [`systemctl set-property ${unit} ${options.join(' ')}`] },
                            ),
                        ];
                        const instance = Machine.ran(
                            machine.name,
                            `runner-${label}`,
                            {
                                create: [
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
                                    ...limited,
                                    './svc.sh start',
                                ].join('\n'),
                                update: limited.join('\n'),
                                delete: removed,
                                stdin: token,
                            },
                            { ignoreChanges: ['stdin'], dependsOn: before },
                        );
                        return [[instance], [label, instance.id] as const];
                    });
                    return { runners: Record.fromEntries(registered), engines: engines.map((version) => machine.pulled(`ghcr.io/epicgames/unreal-engine:dev-slim-${version}`, { customTimeouts: { create: '2h', update: '2h' } })) };
                },
            }),
        );
        Machine.ran(machine.name, 'firewall', { create: firewall.join(' && '), delete: 'ufw disable' }, { dependsOn: [...machine.settled] });
        return { located, ...Record.fromEntries(served.flatMap(Record.toEntries)) };
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { bucketed, hosts, installed, local, Machine, provisioned, Role };
