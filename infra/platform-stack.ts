import { isIPv4 } from 'node:net';
import aws from '@pulumi/aws';
import { DnsRecord, getZoneOutput } from '@pulumi/cloudflare';
import { remote } from '@pulumi/command';
import { Container, type ContainerArgs, getRegistryImageOutput, Network, Provider, RemoteImage, Tag, type types, Volume } from '@pulumi/docker';
import dockerBuild from '@pulumi/docker-build';
import minio from '@pulumi/minio';
import { all, type CustomResourceOptions, getProject, type Input, interpolate, jsonStringify, output, secret } from '@pulumi/pulumi';
import { getDeviceOutput } from '@pulumi/tailscale';
import { Secret } from '@pulumiverse/doppler';
import type { deploy } from '@rasm/hosting/deploy';
import { Identity } from '@rasm/hosting/identity';
import { containers } from '@rasm/testing/containers';
import { Array, Data, Duration, Effect, Function, Option, Predicate, Record, String, Struct, UndefinedOr } from 'effect';
import type { configs, identified } from '#/repository-stack';

// --- [TYPES] ---------------------------------------------------------------------------

type Role = Data.TaggedEnum<{
    Cache: { readonly name: string; readonly kept: Duration.Duration; readonly abort: Duration.Duration };
    Bucket: { readonly name: string; readonly kept: Duration.Duration; readonly abort: Duration.Duration };
    Runner: { readonly label: string };
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
            { _tag: 'Cache', name: 'nx-cache', kept: Duration.weeks(1), abort: Duration.days(1) },
            { _tag: 'Bucket', name: 'cargo', kept: Duration.days(3), abort: Duration.days(1) },
            { _tag: 'Runner', label: 'unreal' },
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

    static readonly on = (host: string, shared: Option.Option<Input<string>>, hub?: Readonly<Record<'DOCKERHUB_USERNAME' | 'DOCKERHUB_TOKEN', string>>, running: readonly types.input.ProviderRegistryAuth[] = []) => {
        const [name, ...labels] = String.split(host, '.');
        const [zone, row] = [labels.join('.'), Record.get<string, Row>(hosts, host)];
        const roles = Option.match(row, { onNone: () => [Role.Apps({ owner: local.owner })], onSome: Struct.get('roles') });
        const dialed = Option.getOrUndefined(Option.map(row, () => ({ host: `ssh://root@${name}` })));
        const pulling = UndefinedOr.map(hub, (rows) => [{ address: 'registry-1.docker.io', username: rows.DOCKERHUB_USERNAME, password: rows.DOCKERHUB_TOKEN }, ...(roles.some(Role.$is('Runner')) ? running : [])]);
        const provider = new Provider(host, { ...dialed, registryAuth: pulling });
        const located = Option.map(row, ({ address }) => [address, getZoneOutput({ filter: { name: zone } }).zoneId] as const).pipe(
            Option.map(([address, zoneId]) => ({ zoneId, target: Option.isNone(shared) ? new DnsRecord(host, { zoneId, name: host, type: 'A', content: address, ttl: 1, proxied: false }).name : host })),
        );
        return new Machine({ name, zone, roles, provider, builder: new dockerBuild.Provider(host, { ...dialed }), network: Option.getOrElse(shared, () => new Network(host, { name: getProject() }, { provider }).name), remote: located });
    };

    // --- [ROUTING]

    static readonly entrypoint = 'websecure' satisfies keyof typeof entrypoints;
    static readonly guard = 'auth';
    static readonly base = /^(?<lead>FROM\s+(?:--\S+\s+)*)(?<named>[^\s@$]+)(?=\s|$)/gmu;
    static readonly forwarded = { id: 'Remote-User', email: 'Remote-Email' } as const;

    routed(service: string, port: number, extra: Readonly<Record<string, string>> = {}) {
        const address = `${service}.${this.zone}`;
        const stated = { 'traefik.enable': 'true', [`traefik.http.routers.${service}.rule`]: `Host(\`${address}\`)`, [`traefik.http.routers.${service}.entrypoints`]: Machine.entrypoint, [`traefik.http.services.${service}.loadbalancer.server.port`]: `${port}`, ...extra };
        const named = Option.toArray(Option.map(this.remote, ({ zoneId, target }) => new DnsRecord(address, { zoneId, name: address, type: 'CNAME', content: target, ttl: 1, proxied: false })));
        return { address, named, labels: Record.collect(stated, (label, value) => ({ label, value })) };
    }

    // --- [CONTAINERS]

    digested(image: string) {
        return interpolate`${image}@${getRegistryImageOutput({ name: image }, { provider: this.provider }).sha256Digest}`;
    }

    pulled(image: string, { digest, ...options }: CustomResourceOptions & { readonly digest?: Input<string> } = {}) {
        return new RemoteImage(`${this.name}-${image}`, { name: digest === undefined ? this.digested(image) : interpolate`${image}@${digest}` }, { provider: this.provider, ...options }).repoDigest;
    }

    tagged(image: string, options: CustomResourceOptions & { readonly digest?: Input<string> } = {}) {
        return new Tag(`${this.name}-${image}`, { sourceImage: this.pulled(image, options), targetImage: image }, { provider: this.provider }).targetImage;
    }

    imaged(image: string, inline: Input<string>, { cache, ...args }: Pick<dockerBuild.ImageArgs, 'context' | 'labels' | 'registries'> & { readonly cache?: string } = {}) {
        const stored = cache === undefined ? { load: true } : { exports: [{ registry: { annotations: args.labels } }], ...Struct.Record(['cacheFrom', 'cacheTo'], [{ registry: { ref: cache } }]) };
        const based = output(inline).apply((text) => {
            const stages = new Set(
                text
                    .matchAll(/^FROM\s.*\sAS\s+(?<stage>\S+)/gimu)
                    .map(({ groups }) => groups?.['stage'])
                    .filter(Predicate.isString),
            );
            const bases = new Set(
                text
                    .matchAll(Machine.base)
                    .map(({ groups }) => groups?.['named'])
                    .filter(Predicate.isString),
            ).difference(stages.union(new Set(['scratch'])));
            return all(Record.fromIterableWith(bases, (named) => [named, this.digested(named)])).apply((pinned) => text.replaceAll(Machine.base, (line: string, lead: string, named: string) => (named in pinned ? `${lead}${pinned[named]}` : line)));
        });
        return new dockerBuild.Image(`${this.name}-${image}`, { dockerfile: { inline: based }, context: { location: '-' }, tags: [image], push: false, buildOnPreview: false, ...stored, buildArgs: { BUILDKIT_DOCKERFILE_CHECK: 'error=true' }, ...args }, { provider: this.builder }).digest;
    }

    kept(volume: string, options: CustomResourceOptions = {}) {
        return new Volume(`${this.name}-${volume}`, { name: volume }, { provider: this.provider, ...options }).name;
    }

    contained(service: string, tests: readonly string[], args: Omit<ContainerArgs, 'name' | 'networksAdvanced' | 'restart' | 'logDriver' | 'logOpts' | 'wait' | 'healthcheck'>, options: CustomResourceOptions = {}) {
        return new Container(
            `${this.name}-${service}`,
            {
                name: service,
                networksAdvanced: [{ name: this.network }],
                restart: 'unless-stopped',
                logDriver: 'journald',
                logOpts: { tag: '{{.Name}}' },
                wait: true,
                waitTimeout: 300,
                destroyGraceSeconds: 120,
                ...Array.match(tests, { onEmpty: () => ({}), onNonEmpty: (stated) => ({ healthcheck: { tests: [...stated], interval: '10s' } }) }),
                ...args,
                envs: output(args.envs).apply((envs) => Array.prepend(envs ?? [], 'NO_COLOR=1')),
            },
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
            'TRAEFIK_LOG_NOCOLOR=true',
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

const stored = (machine: Machine, rows: Readonly<Record<'RUSTFS_ACCESS_KEY' | 'RUSTFS_SECRET_KEY', string>>) => {
    const [user, key] = [secret(rows.RUSTFS_ACCESS_KEY), secret(rows.RUSTFS_SECRET_KEY)];
    const ports = { internal: 9000, external: 7412, health: '/health/ready' } as const;
    const published = Option.match(machine.remote, {
        onNone: () => ({ ips: output(['127.0.0.1']), address: '127.0.0.1' }),
        onSome: () => ({ ips: tailnet(machine).addresses.apply(Array.filter(isIPv4)), address: machine.name }),
    });
    const { address, named, labels } = machine.routed('storage', ports.internal);
    const serving = machine.contained(
        'storage',
        ['CMD', 'curl', '--fail', '--silent', `http://127.0.0.1:${ports.internal}${ports.health}`],
        {
            image: machine.pulled(containers.rustfs),
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

const bucketed = (name: string, storage: ReturnType<typeof stored>, buckets: Effect.Success<ReturnType<typeof deploy.Machine>>['buckets']) => {
    const provider = new minio.Provider(name, { minioServer: storage.server, minioUser: storage.user, minioPassword: storage.key });
    const shared = Record.map(buckets, ({ cors }) => cors.map(({ origins, methods, headers, exposed, age }) => ({ allowedOrigins: [...origins], allowedMethods: [...methods], allowedHeaders: [...headers], exposeHeaders: [...exposed], maxAgeSeconds: Duration.toSeconds(age) })));
    const allowed = Record.filter(shared, Array.isArrayNonEmpty);
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
    return { AWS_ENDPOINT_URL_S3: storage.within, AWS_REGION: storage.region, AWS_ACCESS_KEY_ID: all([account.accessKey, all(created.flatMap(({ configured }) => configured))]).apply(([id]) => id), AWS_SECRET_ACCESS_KEY: account.secretKey };
};

const cached = (machine: Machine, { name, kept, abort }: Data.TaggedEnum.Value<Role, 'Cache'>, storage: ReturnType<typeof stored>, tokens: Readonly<Record<'SERVICE_ACCESS_TOKEN' | 'READ_ONLY_ACCESS_TOKEN', Input<string>>>) => {
    const { port, health } = { port: 3000, health: '/health' };
    const scoped = bucketed(name, storage, { [name]: { kept: false, abort, expire: { '': kept }, cors: [] } });
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
    const envs = Record.collect({ S3_BUCKET_NAME: name, S3_ENDPOINT_URL: scoped.AWS_ENDPOINT_URL_S3, ...scoped, ...tokens }, (variable, value) => interpolate`${variable}=${value}`);
    const serving = machine.contained(name, ['CMD', '/busybox/wget', '--quiet', '--spider', `http://127.0.0.1:${port}${health}`], { image, init: true, envs, labels }, { dependsOn: [proxied(machine), ...named] });
    return { [name]: serving.urn.apply(() => `https://${address}`) };
};

const exposed = ({ name, kept, abort }: Data.TaggedEnum.Value<Role, 'Bucket'>, storage: ReturnType<typeof stored>, project: string) => {
    const { AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY } = bucketed(name, storage, { [name]: { kept: false, abort, expire: { '': kept }, cors: [] } });
    const rows = { RUNS_ON_S3_BUCKET_CACHE: name, RUNS_ON_S3_BUCKET_ENDPOINT: storage.origin, AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY };
    return { [name]: Record.map(rows, (value, row) => new Secret(`${name}-${row}`, { project, config: 'ci' satisfies (typeof configs)[number], name: row, value }).name) };
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

const hosted = (machine: Machine, { owner }: Data.TaggedEnum.Args<Role, 'Apps'>, storage: ReturnType<typeof stored>, rows: Readonly<Record<'AUTHELIA_SESSION_SECRET' | 'AUTHELIA_STORAGE_ENCRYPTION_KEY' | 'AUTHELIA_OWNER_PASSWORD_HASH', string>>) => {
    const [port, { guard, forwarded }] = [9091, Machine];
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
            envs: Array.map(['AUTHELIA_SESSION_SECRET', 'AUTHELIA_STORAGE_ENCRYPTION_KEY'] as const, (row) => interpolate`${row}=${secret(rows[row])}`),
            uploads: [
                { file: '/config/configuration.yml', content: jsonStringify(configuration) },
                { file: '/config/users.yml', content: jsonStringify({ users: { owner: { displayname: 'Owner', email: owner, password: secret(rows.AUTHELIA_OWNER_PASSWORD_HASH) } } }) },
            ],
            mounts: [{ type: 'volume', source: machine.kept(guard), target: '/var/lib/authelia' }],
            labels,
        },
        { dependsOn: [proxied(machine), ...named] },
    );
    return { network: output(machine.network), storage };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const installed = (rows: Parameters<typeof stored>[1] & Parameters<typeof hosted>[3] & NonNullable<Parameters<typeof Machine.on>[2]>) =>
    Effect.sync(() => {
        const machine = Machine.on(local.host, Option.none(), rows);
        return hosted(machine, local, stored(machine, rows), rows);
    });

const provisioned = (
    host: keyof typeof hosts,
    { project, owner, repository, engines }: Effect.Success<typeof identified>,
    rows: Parameters<typeof installed>[0] & Readonly<Record<'EPIC_GHCR_TOKEN' | 'GITHUB_TOKEN' | 'NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN' | 'NX_SELF_HOSTED_REMOTE_CACHE_READ_TOKEN' | 'ATUIN_PASSWORD', string>>,
) =>
    Effect.sync(() => {
        const registries = [{ address: 'ghcr.io', username: owner, password: rows.EPIC_GHCR_TOKEN }];
        const machine = Machine.on(host, Option.none(), rows, registries);
        const storage = Function.memoize((held: Machine) => stored(held, rows));
        const served = machine.roles.map(
            Role.$match({
                Cache: (settings): Readonly<Record<string, unknown>> => cached(machine, settings, storage(machine), { SERVICE_ACCESS_TOKEN: secret(rows.NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN), READ_ONLY_ACCESS_TOKEN: secret(rows.NX_SELF_HOSTED_REMOTE_CACHE_READ_TOKEN) }),
                Bucket: (settings) => exposed(settings, storage(machine), project),
                Apps: (settings) => hosted(machine, settings, storage(machine), rows),
                History: (settings) => synced(machine, settings, owner, secret(rows.ATUIN_PASSWORD)),
                Runner: ({ label }) => {
                    const runner = { version: '2.338.2', sha256: 'baebf204307bac6193480852732f0aba2ae97b6c5597c244f4d5558390147c40', user: 'runner', drained: Duration.hours(2) } as const;
                    const [folder, tools, unit, archive] = [`/home/${runner.user}/actions-runner-${label}`, `/home/${runner.user}/actions-tools-${label}`, `actions.runner.${label}.service`, `/tmp/actions-runner-${label}.tar.gz`];
                    const removed = `systemctl disable --now ${unit}; rm -rf "/etc/systemd/system/${unit}" "${folder}"; systemctl daemon-reload`;
                    const register = `#!/bin/bash
set -euo pipefail
rm -rf "${folder}/_work"
curl -fsS -H "Authorization: Bearer $(cat "${folder}/.token")" -d '{"name":"${machine.name}-${label}-'"$(date +%s)"'","runner_group_id":1,"labels":["self-hosted","${label}"]}' https://api.github.com/repos/${owner}/${repository}/actions/runners/generate-jitconfig | jq -r .encoded_jit_config | install -m 600 -o ${runner.user} -g ${runner.user} /dev/stdin "${folder}/.jit"`;
                    const installation = [
                        'token=$(cat)',
                        removed,
                        'set -eu',
                        `install -d -o ${runner.user} -g ${runner.user} "${folder}" "${tools}" && cd "${folder}"`,
                        `curl -fsSL -o "${archive}" https://github.com/actions/runner/releases/download/v${runner.version}/actions-runner-linux-x64-${runner.version}.tar.gz`,
                        `echo "${runner.sha256}  ${archive}" | sha256sum --check --quiet`,
                        `sudo -u ${runner.user} tar xzf "${archive}" && rm "${archive}"`,
                        './bin/installdependencies.sh',
                        `(umask 077 && printf '%s' "$token" > .token)`,
                        `install -m 700 /dev/stdin register <<'EOF'\n${register}\nEOF`,
                        `install -m 644 /dev/stdin "/etc/systemd/system/${unit}" <<'EOF'`,
                        '[Unit]',
                        `Description=GitHub Actions runner ${label}, registered for one job at a time`,
                        'After=docker.service network-online.target',
                        'Wants=network-online.target',
                        '[Service]',
                        `User=${runner.user}`,
                        `WorkingDirectory=${folder}`,
                        `Environment=RUNNER_TOOL_CACHE=${tools}`,
                        `ExecStartPre=+${folder}/register`,
                        `ExecStart=/bin/bash -c 'exec ./run.sh --jitconfig "$$(cat .jit)"'`,
                        `ExecStop=/bin/bash -c 'while pgrep -f "^${folder}/bin/Runner.Worker" >/dev/null; do sleep 10; done'`,
                        'Restart=always',
                        'RestartSec=5s',
                        `TimeoutStopSec=${Duration.toSeconds(runner.drained)}s`,
                        '[Install]',
                        'WantedBy=multi-user.target',
                        'EOF',
                        'systemctl daemon-reload',
                        `systemctl enable --now ${unit}`,
                    ].join('\n');
                    const connection = { host: machine.name };
                    const kept = new remote.Command(`${machine.name}-tools-${label}`, { connection, delete: `rm -rf "${tools}"` });
                    const instance = new remote.Command(
                        `${machine.name}-runner-${label}`,
                        { connection, addPreviousOutputInEnv: false, logging: 'stderr', create: installation, delete: removed, stdin: secret(rows.GITHUB_TOKEN), triggers: [tailnet(machine).nodeId] },
                        { dependsOn: [kept], deleteBeforeReplace: true, customTimeouts: Struct.Record(['create', 'update', 'delete'], `${Duration.toMinutes(Duration.sum(runner.drained, Duration.minutes(15)))}m`) },
                    );
                    return { [label]: instance.id, engines: Array.map(engines, (version) => machine.tagged(`ghcr.io/epicgames/unreal-engine:dev-slim-${version}`, { customTimeouts: { create: '2h', update: '2h' } })) };
                },
            }),
        );
        return Record.fromEntries(served.flatMap(Record.toEntries));
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { bucketed, hosts, installed, local, Machine, provisioned, Role };
