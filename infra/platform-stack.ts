import { join } from 'node:path';
import { DnsRecord, getZoneOutput } from '@pulumi/cloudflare';
import { local, remote } from '@pulumi/command';
import { interpolate, secret } from '@pulumi/pulumi';
import { PrivateKey } from '@pulumi/tls';
import { Array, Cause, Clock, Effect, FileSystem, Schema, String } from 'effect';
import { run } from '#/command';

// --- [CONSTANTS] -----------------------------------------------------------------------

const settings = { COMPOSE_PROFILES: 'platform,deployed', TRAEFIK_CERTRESOLVER: 'acme', POSTGRES_SYNCHRONOUS_COMMIT: 'on', POSTGRES_ARCHIVE_MODE: 'on', POSTGRES_ARCHIVE_COMMAND: 'pgbackrest archive-push %p', POSTGRES_AUDIT_LOG: 'ddl' };

// --- [MODELS] --------------------------------------------------------------------------

const Deployed = Schema.Struct({
    PLATFORM_DOMAIN: Schema.String,
    BACKUP_ENDPOINT: Schema.URLFromString,
    DEPLOYED_SSH_KNOWN_HOSTS: Schema.TemplateLiteralParser([Schema.String, ' ', Schema.String, ' ', Schema.String]),
    HOSTINGER_API_TOKEN: Schema.NonEmptyString,
    POSTGRES_PASSWORD: Schema.NonEmptyString,
    CLICKHOUSE_PASSWORD: Schema.NonEmptyString,
    CLICKHOUSE_READER_PASSWORD: Schema.NonEmptyString,
    RUSTFS_ACCESS_KEY: Schema.NonEmptyString,
    RUSTFS_SECRET_KEY: Schema.NonEmptyString,
    NX_CACHE_WRITE_TOKEN: Schema.NonEmptyString,
    NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN: Schema.NonEmptyString,
});
const Machines = Schema.fromJsonString(Schema.Array(Schema.Struct({ id: Schema.Int, hostname: Schema.String })));

// --- [COMPOSITION] ---------------------------------------------------------------------

const platformStack = Effect.fnUntraced(function* (deployed: Readonly<Record<string, string>>) {
    const {
        PLATFORM_DOMAIN: domain,
        BACKUP_ENDPOINT: backup,
        DEPLOYED_SSH_KNOWN_HOSTS: [names, , algorithm, , key],
        HOSTINGER_API_TOKEN,
    } = yield* Schema.decodeUnknownEffect(Deployed)(deployed);
    const host = Array.headNonEmpty(String.split(names, ','));
    const environment = { ...deployed, ...settings, BACKUP_HOST: backup.host, BACKUP_BUCKET: `${domain.replaceAll('.', '-')}-backup` };
    return {
        stackName: host,
        envVars: deployed,
        program: Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const [applied, script, definition, machines] = yield* Effect.all([
                Clock.currentTimeMillis,
                fs.readFileString(join(import.meta.dirname, 'host.sh')),
                fs.readFileString(join(import.meta.dirname, 'compose.yaml')),
                Effect.flatMap(run('hostinger', ['vps', 'virtual-machines', 'list', '--no-input', '--format', 'json'], { HOSTINGER_API_TOKEN }), Schema.decodeEffect(Machines)),
            ]);
            const { id } = yield* Effect.fromOption(
                Array.findFirst(machines, ({ hostname }) => hostname === host),
                () => new Cause.NoSuchElementError(`Hostinger holds no virtual machine named ${host}`),
            );
            const deploy = new PrivateKey(host, { algorithm: 'ED25519' });
            const keys = (verb: string) => interpolate`hostinger vps ssh-keys ${verb} ${id} --keys '${deploy.publicKeyOpenssh.apply((openssh) => openssh.trim())}' --no-input`;
            const authorized = new local.Command(`${host}-key`, { create: keys('add-virtual-machine'), delete: interpolate`${keys('remove-virtual-machine')} --yes`, triggers: [deploy.publicKeyOpenssh] });
            const connection = { host, user: 'root', privateKey: deploy.privateKeyOpenssh, hostKey: `${algorithm} ${key.trim()}`, dialErrorLimit: 1 };
            const engine = new remote.Command(host, { connection, create: 'bash -s', stdin: script, addPreviousOutputInEnv: false }, { dependsOn: authorized });
            const { zoneId } = getZoneOutput({ filter: { name: domain } });
            const records = [domain, `*.${domain}`].map((name) => new DnsRecord(name, { zoneId, name, type: 'CNAME', content: host, ttl: 1, proxied: false }));
            const services = new remote.Command(
                `${host}-services`,
                {
                    connection,
                    create: `POSTGRES_SHARED_BUFFERS="$(awk '/MemTotal/ { printf "%dMB", $2 / 4096 }' /proc/meminfo)" docker compose --file - up --build --detach --wait --remove-orphans`,
                    delete: 'docker compose --file - down',
                    stdin: definition,
                    addPreviousOutputInEnv: false,
                    environment: secret({ ...environment, PLATFORM_APPLIED: `${applied}` }),
                },
                { dependsOn: [engine, ...records], deleteBeforeReplace: true },
            );
            return { host, domain, records: records.map(({ name }) => name), services: services.id };
        }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { platformStack };
