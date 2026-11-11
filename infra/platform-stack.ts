import { dirname, join, relative } from 'node:path';
import { DnsRecord, getZoneOutput } from '@pulumi/cloudflare';
import { local } from '@pulumi/command';
import { asset, type Input, interpolate, type Output, output, type Resource, runtime, secret, unsecret } from '@pulumi/pulumi';
import { getDevicesOutput, TailnetKey, Webhook } from '@pulumi/tailscale';
import { PrivateKey } from '@pulumi/tls';
import { paths } from '@rasm/hooks/senders';
import { Host, Ssh } from '@rasm/ssh';
import { TailscaleEvent } from '@rasm/webhooks/tailscale';
import { Array, ByteSize, Cause, Clock, Config, Effect, FileSystem, flow, Option, Path, Record, Result, Schema, Struct } from 'effect';
import { Base64 } from 'effect/encoding';
import { type ChildProcessSpawner, CommandLine } from 'effect/process';
import { run } from '#/command';

// --- [TYPES] ---------------------------------------------------------------------------

interface Target {
    readonly profiles: readonly string[];
    readonly replica: (path: string) => string;
    readonly environment: Record.ReadonlyRecord<string, string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const sessions = 100;
const connections = join(dirname(import.meta.dirname), '.cache', 'ssh');
const definition = join(import.meta.dirname, 'compose.yaml');
const dialing = { alive: { interval: 5000, count: 3 }, persist: 60_000, within: 2000 };
const settings = { TRAEFIK_CERTRESOLVER: 'acme', POSTGRES_AUDIT_LOG: 'ddl' };
const profiles = ['platform', 'deployed'];
const archived = { POSTGRES_ARCHIVE_MODE: 'on', POSTGRES_ARCHIVE_TIMEOUT: '1min' };
const allocation = {
    divisors: { POSTGRES_SHARED_BUFFERS: 4, POSTGRES_EFFECTIVE_CACHE_SIZE: 2, POSTGRES_MAINTENANCE_WORK_MEM: 32 },
    server: 4,
    parts: { CLICKHOUSE_MAX_SERVER_MEMORY_USAGE_TO_RAM_RATIO: 1, CLICKHOUSE_MERGES_MUTATIONS_MEMORY_USAGE_TO_RAM_RATIO: 2, CLICKHOUSE_CACHE_SIZE_TO_RAM_MAX_RATIO: 5 },
};
const packages = ['docker-ce', 'docker-buildx-plugin', 'docker-compose-plugin', 'systemd-zram-generator'];

// --- [TABLES] --------------------------------------------------------------------------

const files = {
    '/etc/docker/daemon.json': { content: '{"live-restore": true}\n', reload: [['systemctl', 'reload', 'docker']] },
    '/etc/sysctl.d/90-platform.conf': { content: 'vm.overcommit_memory = 1\nnet.ipv4.tcp_congestion_control = bbr\n', reload: [['sysctl', '--system']] },
    '/etc/systemd/zram-generator.conf': {
        content: '[zram0]\nzram-size = ram / 4\ncompression-algorithm = zstd\n',
        reload: [
            ['systemctl', 'daemon-reload'],
            ['systemctl', 'restart', 'systemd-zram-setup@zram0.service'],
        ],
    },
    '/etc/ssh/sshd_config.d/00-platform.conf': { content: `PasswordAuthentication no\nAcceptEnv *\nMaxSessions ${sessions}\nMaxStartups ${sessions}\n`, reload: [['systemctl', 'reload', 'ssh']] },
    '/etc/systemd/journald.conf.d/90-platform.conf': { content: '[Journal]\nSystemMaxUse=1G\n', reload: [['systemctl', 'restart', 'systemd-journald']] },
    '/etc/systemd/system.conf.d/90-platform.conf': { content: '[Manager]\nDefaultTimeoutStopSec=150s\n', reload: [['systemctl', 'daemon-reexec']] },
};
const registries = { 'dhi.io': ({ DOCKERHUB_USERNAME, DOCKERHUB_TOKEN }: Record.ReadonlyRecord<'DOCKERHUB_USERNAME' | 'DOCKERHUB_TOKEN', string>) => Base64.encode(`${DOCKERHUB_USERNAME}:${DOCKERHUB_TOKEN}`) };
const machines = {
    hostinger: {
        user: 'root',
        listed: flow(
            Schema.decodeUnknownEffect(Schema.Struct({ HOSTINGER_API_TOKEN: Schema.NonEmptyString })),
            Effect.flatMap((credentials) => run('hostinger', ['vps', 'virtual-machines', 'list', '--no-input', '--format', 'json'], credentials)),
            Effect.flatMap(Schema.decodeEffect(Schema.fromJsonString(Schema.Array(Schema.Struct({ id: Schema.Int, hostname: Schema.String, memory: Schema.Int }))))),
            Effect.map(Record.fromIterableBy(Struct.get('hostname'))),
            Effect.map(
                Record.map(({ id, memory }) => ({
                    memory: ByteSize.mebibytes(memory),
                    authorized: (key: Output<string>) => ({ create: interpolate`hostinger vps ssh-keys add-virtual-machine ${id} --keys '${key}' --no-input`, delete: interpolate`hostinger vps ssh-keys remove-virtual-machine ${id} --keys '${key}' --no-input --yes` }),
                })),
            ),
        ),
    },
};
const records = {
    cloudflare: (zone: string, names: readonly string[], content: string) => {
        const { zoneId } = getZoneOutput({ filter: { name: zone } });
        return names.map((name) => new DnsRecord(name, { zoneId, name, type: 'CNAME', content, ttl: 1, proxied: false }));
    },
};
const backups = {
    s3: flow(
        Schema.decodeUnknownEffect(Schema.Struct({ BACKUP_ENDPOINT: Schema.URLFromString, BACKUP_REGION: Schema.NonEmptyString, BACKUP_ACCESS_KEY_ID: Schema.NonEmptyString, BACKUP_SECRET_ACCESS_KEY: Schema.NonEmptyString })),
        Effect.map(
            ({ BACKUP_ENDPOINT: { host, origin }, BACKUP_REGION: region, BACKUP_ACCESS_KEY_ID: id, BACKUP_SECRET_ACCESS_KEY: key }) =>
                (bucket: string): Target => ({
                    profiles: ['backup', 'backup-s3'],
                    replica: (path) => `s3://${bucket}/${path}`,
                    environment: {
                        PGBACKREST_REPO1_TYPE: 's3',
                        PGBACKREST_REPO1_S3_BUCKET: bucket,
                        PGBACKREST_REPO1_S3_ENDPOINT: host,
                        PGBACKREST_REPO1_S3_REGION: region,
                        PGBACKREST_REPO1_S3_KEY: id,
                        PGBACKREST_REPO1_S3_KEY_SECRET: key,
                        AWS_ENDPOINT: host,
                        AWS_ENDPOINT_URL: origin,
                        AWS_REGION: region,
                        AWS_S3_BUCKET_NAME: bucket,
                        AWS_ACCESS_KEY_ID: id,
                        AWS_SECRET_ACCESS_KEY: key,
                        REMOTE_STORAGE: 's3',
                        S3_BUCKET: bucket,
                        S3_ENDPOINT: origin,
                        S3_REGION: region,
                        S3_ACCESS_KEY: id,
                        S3_SECRET_KEY: key,
                        LITESTREAM_ACCESS_KEY_ID: id,
                        LITESTREAM_SECRET_ACCESS_KEY: key,
                        LITESTREAM_ENDPOINT: origin,
                        LITESTREAM_REGION: region,
                    },
                }),
        ),
    ),
    azure: flow(
        Schema.decodeUnknownEffect(Schema.Struct({ BACKUP_ENDPOINT: Schema.TemplateLiteralParser([Schema.Literals(['http', 'https']), '://', Schema.NonEmptyString, '.blob.', Schema.NonEmptyString]), BACKUP_SECRET_ACCESS_KEY: Schema.NonEmptyString })),
        Effect.map(
            ({ BACKUP_ENDPOINT: [scheme, , account, , suffix], BACKUP_SECRET_ACCESS_KEY: key }) =>
                (bucket: string): Target => ({
                    profiles: ['backup'],
                    replica: (path) => `abs://${account}@${bucket}/${path}`,
                    environment: {
                        PGBACKREST_REPO1_TYPE: 'azure',
                        PGBACKREST_REPO1_AZURE_ACCOUNT: account,
                        PGBACKREST_REPO1_AZURE_CONTAINER: bucket,
                        PGBACKREST_REPO1_AZURE_KEY: key,
                        PGBACKREST_REPO1_AZURE_ENDPOINT: `blob.${suffix}`,
                        AZURE_STORAGE_ACCOUNT_NAME: account,
                        AZURE_STORAGE_PRIMARY_ACCOUNT_KEY: key,
                        AZURE_STORAGE_CONTAINER_NAME: bucket,
                        AZURE_STORAGE_ENDPOINT: `${scheme}://${account}.blob.${suffix}/`,
                        REMOTE_STORAGE: 'azblob',
                        AZBLOB_ACCOUNT_NAME: account,
                        AZBLOB_ACCOUNT_KEY: key,
                        AZBLOB_CONTAINER: bucket,
                        AZBLOB_ENDPOINT_SUFFIX: suffix,
                        AZBLOB_ENDPOINT_SCHEMA: scheme,
                        LITESTREAM_AZURE_ACCOUNT_KEY: key,
                    },
                }),
        ),
    ),
};

// --- [MODELS] --------------------------------------------------------------------------

const dialed = { host: Schema.NonEmptyString, port: Schema.optionalKey(Schema.Port) };
const Route = Schema.TaggedUnion({ Address: dialed, Tailnet: dialed });
const Hosts = Schema.Struct({
    DEPLOYED_HOSTS: Schema.fromJsonString(
        Schema.Record(
            Schema.NonEmptyString,
            Schema.Struct({
                hostKey: Schema.NonEmptyString,
                machine: Schema.Literals(Struct.keys(machines)),
                domain: Schema.Struct({ name: Schema.NonEmptyString, dns: Schema.Literals(Struct.keys(records)) }),
                tags: Schema.Array(Schema.TemplateLiteral(['tag:', Schema.NonEmptyString])),
                routes: Schema.NonEmptyArray(Route),
                tailnet: Schema.Struct({ ssh: Schema.Boolean }),
            }),
        ),
    ),
});
const Deployed = Hosts.mapFields(
    Struct.assign({
        BACKUP_KIND: Schema.OptionFromOptionalKey(Schema.Literals(Struct.keys(backups))),
        ...Struct.Record(
            [
                'CLOUDFLARE_ACCOUNT_ID',
                'DOCKERHUB_USERNAME',
                'DOCKERHUB_TOKEN',
                'POSTGRES_PASSWORD',
                'CLICKHOUSE_PASSWORD',
                'CLICKHOUSE_READER_PASSWORD',
                'RUSTFS_ACCESS_KEY',
                'RUSTFS_SECRET_KEY',
                'AUTHELIA_SESSION_SECRET',
                'AUTHELIA_STORAGE_ENCRYPTION_KEY',
                'AUTHELIA_OWNER_PASSWORD_HASH',
                'NX_CACHE_WRITE_TOKEN',
                'NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN',
            ],
            Schema.NonEmptyString,
        ),
    }),
);
const Kind = Deployed.mapFields(Struct.pick(['BACKUP_KIND']));
const Composed = Schema.fromJsonString(Schema.Struct({ services: Schema.Record(Schema.String, Schema.Struct({ build: Schema.optionalKey(Schema.Struct({ context: Schema.String })) })) }));
const Variables = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Unknown));
const Credentials = Schema.fromJsonString(Schema.Struct({ auths: Schema.Record(Schema.String, Schema.Struct({ auth: Schema.String })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const identified = (host: string) => join(connections, host, `${host}.key`);
const backupTarget = Effect.fnUntraced(function* (deployed: Readonly<Record<string, string>>, domain: string) {
    const { BACKUP_KIND: kind } = yield* Schema.decodeEffect(Kind)(deployed);
    const bucket = `${domain.replaceAll('.', '-')}-backup`;
    return yield* Effect.transposeOption(Option.map(kind, (stated) => Effect.map(backups[stated](deployed), (row) => row(bucket))));
});
const uploaded = Effect.fnUntraced(function* (host: Host, name: string, root: string, bundle: string, dependsOn: readonly Resource[]) {
    const path = yield* Path.Path;
    const [source, folder] = [join(root, bundle), dirname(bundle)];
    const steps = yield* Effect.fromResult(Result.all([...Array.make(['rm', '-rf', bundle] as const, ['mkdir', '-p', folder] as const).map((words) => Result.flatMap(host.command(path, connections, words), CommandLine.join)), CommandLine.join(host.copy(path, connections, source, folder))]));
    return new local.Command(`${name}-${bundle}`, { create: steps.join(' && '), triggers: [new asset.FileArchive(source)], addPreviousOutputInEnv: false }, { dependsOn: [...dependsOn] });
});
const prepared = Effect.fnUntraced(function* (deployed: Readonly<Record<string, string>>) {
    const decoded = yield* Schema.decodeUnknownEffect(Deployed)(deployed);
    const listings = yield* Effect.all(Record.map(machines, (row) => Effect.cached(row.listed(deployed))));
    const enrolled = new Set(Object.values(decoded.DEPLOYED_HOSTS).flatMap(({ routes }) => routes.filter(Route.guards.Tailnet).map(Struct.get('host'))));
    return { deployed, decoded, listings, enrolled };
});

const allotted = Effect.fnUntraced(function* (deployed: Readonly<Record<string, string>>, domain: string, installed: ByteSize.ByteSize) {
    const [target, node] = yield* Effect.all([backupTarget(deployed, domain), Config.String('NODE_VERSION')]);
    const backup = Option.map(target, ({ profiles: added, environment: projected }) => ({ profiles: added, environment: { ...projected, ...archived } }));
    const sizes = Record.getSomes(Record.map(allocation.divisors, (share) => Option.map(ByteSize.divide(installed, share), (size) => `${ByteSize.toUnit(size, 'MiB')}MB`)));
    const candidates = {
        ...deployed,
        ...settings,
        ...sizes,
        ...Record.map(allocation.parts, (part) => String(1 / (allocation.server * part))),
        PLATFORM_DOMAIN: domain,
        NODE_VERSION: node,
        COMPOSE_PROFILES: [...profiles, ...Option.match(backup, { onNone: () => [], onSome: Struct.get('profiles') })].join(','),
    };
    return { backup, candidates };
});

const accessed = Effect.fnUntraced(function* (host: string, { hostKey, machine, routes }: (typeof Hosts.Type)['DEPLOYED_HOSTS'][string], deploy: PrivateKey) {
    const [path, context] = yield* Effect.all([Path.Path, Effect.context<FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner>()]);
    const address = yield* Effect.fromOption(Array.findFirst(routes, Route.guards.Address), () => new Cause.NoSuchElementError(`${host} states no Address route to enroll through`));
    const mesh = Array.findFirst(routes, Route.guards.Tailnet);
    const stated = { ...Struct.omit(dialing, ['within']), ...Struct.omit(address, ['_tag']), user: machines[machine].user, identity: { _tag: 'Key', file: identified(host) }, trust: { _tag: 'HostKeys', keys: [hostKey] } };
    const [direct, meshed] = yield* Effect.all([
        Schema.decodeUnknownEffect(Host)({ ...stated, name: host, path: { _tag: 'Direct', preferred: [] } }),
        Effect.transposeOption(Option.map(mesh, (machined) => Schema.decodeUnknownEffect(Host)({ ...stated, name: machined.host, path: { _tag: 'Direct', preferred: [{ ...Struct.omit(machined, ['_tag']), within: dialing.within }] } }))),
    ]);
    const placed = Effect.fnUntraced(
        function* (material: string) {
            const ssh = yield* Ssh;
            yield* Effect.all([ssh.written(direct, { [identified(host)]: material }), ...Option.toArray(Option.map(meshed, (each) => ssh.written(each)))], { discard: true });
            return connections;
        },
        Effect.provide(Ssh.layer(connections)),
    );
    const ready = unsecret(output(deploy.privateKeyOpenssh).apply((material) => (runtime.isDryRun() ? connections : placed(material).pipe(Effect.runPromiseWith(context)))));
    const over = (through: Host, words: Array.NonEmptyReadonlyArray<string>, sent: readonly string[] = []) => Effect.map(Effect.fromResult(Result.flatMap(through.command(path, connections, words, sent), CommandLine.join)), (line) => ready.apply(() => line));
    return { mesh, direct, reached: Option.getOrElse(meshed, () => direct), over };
});

const equipped = Effect.fnUntraced(function* (host: string, { domain, tags, tailnet }: (typeof Hosts.Type)['DEPLOYED_HOSTS'][string], { mesh, direct, over }: Effect.Success<ReturnType<typeof accessed>>, key: local.Command, credentials: string, script: string) {
    const engine = new local.Command(host, { create: yield* over(direct, ['bash', '-s', '--', ...packages, ...Option.toArray(Option.as(mesh, 'tailscale'))]), stdin: script, addPreviousOutputInEnv: false }, { dependsOn: key });
    const authenticated = new local.Command(`${host}-registries`, { create: yield* over(direct, ['install', '-D', '-m', '0600', '/dev/stdin', '.docker/config.json']), stdin: secret(credentials), addPreviousOutputInEnv: false }, { dependsOn: engine });
    const configured = yield* Effect.reduce(
        Record.toEntries(files),
        () => Array.of(engine),
        (chained, [file, { content, reload }]) =>
            Effect.fromResult(Result.all([CommandLine.join(['install', '-D', '-m', '0644', '/dev/stdin', file]), ...reload.map((words) => CommandLine.join(words))])).pipe(
                Effect.flatMap((chain) => over(direct, ['sh', '-c', chain.join(' && ')])),
                Effect.map((create) => Array.append(chained, new local.Command(`${host}-${file}`, { create, stdin: content, addPreviousOutputInEnv: false }, { dependsOn: Array.lastNonEmpty(chained) }))),
            ),
    );
    const joined = yield* Effect.transposeOption(
        Option.map(mesh, ({ host: name }) =>
            Effect.map(over(direct, ['tailscale', 'up', '--auth-key=file:/dev/stdin', `--hostname=${name}`, `--ssh=${tailnet.ssh}`]), (create) => ({
                reachable: new local.Command(`${host}-mesh`, { create, stdin: new TailnetKey(`${host}-mesh`, { tags: [...tags], preauthorized: true, recreateIfInvalid: 'always' }).key, addPreviousOutputInEnv: false }, { dependsOn: configured }),
                variables: { TAILSCALE_WEBHOOK_SECRET: new Webhook(`${host}-mesh`, { endpointUrl: `https://hooks.${domain.name}${paths.tailscale}`, subscriptions: [...TailscaleEvent.Kind.literals] }).secret },
            })),
        ),
    );
    return { authenticated, joined, reachable: Option.match(joined, { onNone: () => configured, onSome: (enrollment) => [enrollment.reachable] }) };
});

const provisioned = Effect.fnUntraced(function* ({ deployed, decoded, listings, enrolled }: Effect.Success<ReturnType<typeof prepared>>, host: string) {
    const [fs, project, root] = [yield* FileSystem.FileSystem, import.meta.dirname, dirname(import.meta.dirname)];
    const stated = yield* Effect.fromOption(Record.get(decoded.DEPLOYED_HOSTS, host));
    const { memory: installed, authorized } = yield* Effect.flatMap(listings[stated.machine], (listed) => Effect.fromOption(Record.get(listed, host), () => new Cause.NoSuchElementError(`${stated.machine} lists no machine named ${host}`)));
    const [{ backup, candidates }, domain] = [yield* allotted(deployed, stated.domain.name, installed), stated.domain];
    const [applied, credentials, script, text, variables, { services: composed }] = yield* Effect.all([
        Clock.currentTimeMillis,
        Schema.encodeEffect(Credentials)({ auths: Record.map(registries, (auth) => ({ auth: auth(decoded) })) }),
        fs.readFileString(join(project, 'host.sh')),
        fs.readFileString(definition),
        Effect.flatMap(run('docker', ['compose', '--file', definition, 'config', '--variables', '--format', 'json']), Schema.decodeEffect(Variables)),
        Effect.flatMap(run('docker', ['compose', '--file', definition, 'config', '--format', 'json'], candidates), Schema.decodeEffect(Composed)),
    ]);
    const deploy = new PrivateKey(host, { algorithm: 'ED25519' });
    const key = new local.Command(`${host}-key`, { ...authorized(deploy.publicKeyOpenssh.apply((openssh) => openssh.trim())), triggers: [deploy.publicKeyOpenssh] });
    const access = yield* accessed(host, stated, deploy);
    const { authenticated, joined, reachable } = yield* equipped(host, stated, access, key, credentials, script);
    const bundles = yield* Effect.forEach(
        Array.dedupe(Object.values(composed).flatMap(({ build }) => Array.fromNullishOr(build?.context))).filter((built) => built !== project),
        (bundle) => uploaded(access.reached, host, root, relative(root, bundle), reachable),
    );
    const named = records[domain.dns](domain.name, [domain.name, `*.${domain.name}`], host);
    const supplied: Readonly<Record<string, Input<string>>> = {
        ...Record.filter(candidates, (_, name) => Record.has(variables, name)),
        ...Option.match(backup, { onNone: () => ({}), onSome: Struct.get('environment') }),
        ...Option.match(joined, { onNone: () => ({}), onSome: Struct.get('variables') }),
        COMPOSE_PROFILES: candidates.COMPOSE_PROFILES,
        PLATFORM_APPLIED: `${applied}`,
        TAILSCALE_HOSTS: getDevicesOutput({}, { dependsOn: reachable }).apply(({ devices: found = [] }) =>
            found
                .filter(({ hostname }) => enrolled.has(hostname))
                .map(({ name }) => name)
                .join(','),
        ),
    };
    const composing = (...words: Array.NonEmptyReadonlyArray<string>) => access.over(access.reached, ['docker', 'compose', '--project-directory', relative(root, project), '--file', '-', ...words], Record.keys(supplied));
    const services = new local.Command(
        `${host}-services`,
        { create: yield* composing('up', '--build', '--detach', '--wait', '--remove-orphans'), delete: yield* composing('down'), stdin: text, addPreviousOutputInEnv: false, environment: secret(supplied) },
        { dependsOn: [...reachable, authenticated, ...named, ...bundles], deleteBeforeReplace: true, protect: true },
    );
    return { host, domain: domain.name, records: named.map(({ name }) => name), services: services.id, connection: Struct.omit(yield* Schema.encodeEffect(Host)(access.reached), ['identity']), identity: deploy.privateKeyOpenssh };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const platformStacks = flow(
    prepared,
    Effect.map((shared) => Object.keys(shared.decoded.DEPLOYED_HOSTS).map((stackName) => ({ stackName, program: provisioned(shared, stackName) }))),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { backupTarget, connections, Deployed, definition, identified, platformStacks, uploaded };
