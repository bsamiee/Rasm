import { Buffer } from 'node:buffer';
import process from 'node:process';
import { ExitError } from '@rasm/process';
import { Array, Duration, Effect, flow, Function, Option, type Path, Predicate, Record, Result, Schema } from 'effect';
import { Base64 } from 'effect/encoding';
import { Host as Address } from 'effect/net';
import { CommandLine } from 'effect/process';
import { SSHConfig } from 'ssh-config';

// --- [CONSTANTS] -----------------------------------------------------------------------

const sunPath: Partial<Record.ReadonlyRecord<NodeJS.Platform, number>> = { darwin: 104, linux: 108 };
const suffix = 17;
const decoder = new TextDecoder();

// --- [MODELS] --------------------------------------------------------------------------

const Line = Schema.NonEmptyString.check(Schema.isPattern(/^(?![#=~])[^\0\n\r"'\\%$\p{Cs}]+$/u)).annotate({
    description: 'One value ssh_config, known_hosts, and sh read as written: free of quotes, backslash, percent, dollar, newline, NUL, and lone surrogates, and opening with no number sign, equals sign, or tilde',
});
const ControlPath = Schema.String.check(
    Schema.makeFilter((socket) => Option.exists(Option.fromUndefinedOr(sunPath[process.platform]), (size) => Buffer.byteLength(socket) + suffix < size), {
        description: `Socket path an OpenSSH master binds with its ${suffix}-byte temporary suffix, which with its NUL fits sockaddr_un.sun_path of this platform`,
    }),
);
const Unquoted = Schema.String.check(Schema.makeFilter((text) => Result.getOrUndefined(CommandLine.quote(text)) === text, { description: 'Path sh reads as one word unquoted, as ProxyJump writes -F <config> into the command line its shell runs' }));
const PublicKey = Schema.String.check(
    Schema.isPattern(/^[\w.@-]+ [\d+/A-Za-z]+={0,2}$/u),
    Schema.makeFilter(
        (line) => {
            const words = line.split(' ');
            return Predicate.isTupleOf(words, 2) && Result.match(Base64.decode(words[1]), { onFailure: Function.constFalse, onSuccess: (bytes) => bytes.length > 4 && decoder.decode(bytes.subarray(4, 4 + new DataView(bytes.buffer, bytes.byteOffset, bytes.length).getUint32(0))) === words[0] });
        },
        { description: 'OpenSSH public key <algorithm> <base64 blob>, the blob opening with the algorithm as its first SSH string' },
    ),
);
const Seconds = Schema.DurationFromMillis.check(Schema.makeFilter(flow(Duration.toSeconds, Number.isInteger), { description: 'Whole seconds, the unit OpenSSH reads' }), Schema.makeIsGreaterThan({ order: Duration.Order })(Duration.zero));
const dial = {
    host: Schema.Union([Schema.IpAddressFromString, Schema.DomainNameFromString]).annotate({ description: 'HostName as an IP address or a domain name, never a scoped IPv6 literal, whose % ssh_config expands as a token' }),
    port: Schema.OptionFromOptionalKey(Schema.Port.check(Schema.isGreaterThan(0))),
};
const endpoint = {
    name: Schema.DomainNameFromString.annotate({ description: 'ssh_config Host alias, HostKeyAlias, and folder of its files' }),
    user: Line,
    identity: Schema.TaggedUnion({
        Key: { file: Line, certificate: Schema.OptionFromOptionalKey(Line) },
        Agent: { socket: Line, key: Schema.OptionFromOptionalKey(PublicKey), forwarded: Schema.Boolean.pipe(Schema.withDecodingDefaultKey(Effect.succeed(false))) },
        None: {},
    }).annotate({
        description: 'Key names a private key file and its certificate file the caller keeps; Agent an agent socket, offering the one key it names or every key it holds, and forwarding it when forwarded; None the none method, which Tailscale SSH accepts',
    }),
    trust: Schema.TaggedUnion({ HostKeys: { keys: Schema.NonEmptyArray(PublicKey) }, Authority: { keys: Schema.NonEmptyArray(PublicKey) } }),
    ...dial,
};
class Host extends Schema.Class<Host, { readonly brand: unique symbol }>('Host')({
    ...endpoint,
    route: Schema.TaggedUnion({
        Direct: { preferred: Schema.Array(Schema.Struct({ ...dial, within: Seconds }).annotate({ description: 'Dial a starting master tries before host and port, on the host port when it states none, with ConnectTimeout within' })) },
        Jump: { hops: Schema.NonEmptyArray(Schema.Struct(endpoint)) },
    }).annotate({ description: 'Direct starts its master on each preferred dial in order, then on host and port, and keeps the first that starts; Jump connects through its hops in order' }),
    persist: Seconds.annotate({ description: 'ControlPersist, the time an idle shared connection stays open for the next ssh process' }),
    alive: Schema.OptionFromOptionalKey(Schema.Struct({ interval: Seconds, count: Schema.Int.check(Schema.isGreaterThan(0)) })),
    timeout: Schema.OptionFromOptionalKey(Seconds),
    shell: Schema.Literals(['posix', 'powershell'])
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed('posix')))
        .annotate({ description: 'CommandLine format of the remote login shell, which reads each remote command line' }),
}) {
    // --- [FILES]

    config(path: Path.Path, folder: string) {
        return path.join(folder, this.name, 'config');
    }

    control(path: Path.Path, folder: string) {
        return path.join(folder, this.name, 'control');
    }

    files(path: Path.Path, folder: string) {
        const [config, control, known] = [this.config(path, folder), this.control(path, folder), path.join(folder, this.name, 'known_hosts')];
        const hops = Host.fields.route.match(this.route, { Direct: () => [], Jump: ({ hops: jumped }) => jumped });
        const endpoints = Array.map(Array.prepend(hops, this), (stated) => ({ stated, agent: path.join(folder, stated.name, 'agent.pub') }));
        const blocks = Array.map(endpoints, ({ stated, agent }) => ({ Host: stated.name, ...section(stated, agent) }));
        const routed = Array.match(hops, { onEmpty: () => Result.succeed({}), onNonEmpty: (jumped) => Result.map(Schema.decodeResult(Unquoted)(config), () => ({ ProxyJump: jumped.map(({ name }) => name).join(',') })) });
        const marked = Host.fields.trust.match({ HostKeys: () => [], Authority: () => ['@cert-authority'] });
        return Result.map(Result.all({ control: Schema.decodeResult(ControlPath)(control), routed }), ({ routed: jump }) => ({
            [known]: endpoints.flatMap(({ stated: { name, trust } }) => trust.keys.map((key) => `${[...marked(trust), name, key].join(' ')}\n`)).join(''),
            [config]: [
                { ...Array.headNonEmpty(blocks), ...jump, ControlMaster: 'auto', ControlPath: [control], ControlPersist: `${Duration.toSeconds(this.persist)}` },
                ...Array.tailNonEmpty(blocks),
                {
                    Host: '*',
                    UserKnownHostsFile: [known],
                    GlobalKnownHostsFile: 'none',
                    StrictHostKeyChecking: 'yes',
                    UpdateHostKeys: 'no',
                    BatchMode: 'yes',
                    ExitOnForwardFailure: 'yes',
                    StreamLocalBindUnlink: 'yes',
                    ...Record.getSomes({
                        ConnectTimeout: Option.map(this.timeout, (timeout) => `${Duration.toSeconds(timeout)}`),
                        ServerAliveInterval: Option.map(this.alive, ({ interval }) => `${Duration.toSeconds(interval)}`),
                        ServerAliveCountMax: Option.map(this.alive, ({ count }) => `${count}`),
                    }),
                },
            ]
                .reduce((written, block) => written.append(block), new SSHConfig())
                .toString(),
            ...Record.fromEntries(endpoints.flatMap(({ stated: { identity }, agent }) => Host.fields.identity.matchOrElse(identity, { Agent: ({ key }) => Option.toArray(Option.map(key, (held) => [agent, `${held}\n`] as const)) }, () => []))),
        }));
    }

    // --- [COMMANDS]

    get dials() {
        const preferred = Host.fields.route.match(this.route, { Direct: ({ preferred: stated }) => stated, Jump: () => [] });
        return Array.append(
            preferred.map(({ host, port, within }) => Record.toEntries({ HostName: Address.formatHost(host), ...Record.getSomes({ Port: Option.map(port, String) }), ConnectTimeout: `${Duration.toSeconds(within)}` }).flatMap(([option, value]) => ['-o', `${option}=${value}`])),
            [],
        );
    }

    command(path: Path.Path, folder: string, options: readonly string[], remote: readonly string[]) {
        const line = Array.isReadonlyArrayNonEmpty(remote) ? Result.map(CommandLine.join(remote, { format: this.shell }), Array.of) : Result.succeed<readonly string[]>([]);
        return Result.map(line, (words) => Array.make('ssh', '-F', this.config(path, folder), ...options, '--', this.name, ...words));
    }

    copy(path: Path.Path, folder: string, source: string, target: string) {
        return Array.make('scp', '-F', this.config(path, folder), '-r', '--', source, target);
    }

    remote(file: string) {
        return `${this.name}:${file}`;
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

class SshError extends Schema.TaggedError<SshError, { readonly brand: unique symbol }>()('SshError', {
    reason: Schema.TaggedUnion({
        Exit: { exit: ExitError },
        Signal: { signal: Schema.String },
        Unreachable: { output: Schema.OptionFromOptionalKey(Schema.String) },
        Refused: { answer: Schema.String },
        Protocol: { received: Schema.String },
    }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
    message: Schema.String,
}) {
    static readonly of = (reason: SshError['reason'], subject: string) => (cause?: unknown) =>
        SshError.make({
            reason,
            subject,
            ...(cause === undefined ? {} : { cause }),
            message: [
                subject,
                reason._tag.toLowerCase(),
                ...SshError.fields.reason.match(reason, {
                    Exit: ({ exit }) => [exit.message],
                    Signal: ({ signal }) => [signal],
                    Unreachable: ({ output }) => Option.toArray(output),
                    Refused: ({ answer }) => [answer],
                    Protocol: ({ received }) => [received],
                }),
            ].join(' '),
        });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const section = ({ name, user, identity, host, port }: Pick<Host, 'name' | 'user' | 'identity' | 'host' | 'port'>, agent: string) => ({
    HostName: Address.formatHost(host),
    User: user,
    HostKeyAlias: name,
    ...Record.getSomes({ Port: Option.map(port, String) }),
    ...Host.fields.identity.match(identity, {
        Key: ({ file, certificate }) => ({ IdentityFile: file, ...Record.getSomes({ CertificateFile: certificate }), IdentitiesOnly: 'yes', IdentityAgent: 'none' }),
        Agent: ({ socket, key, forwarded }) => ({ IdentityFile: Option.match(key, { onNone: () => 'none', onSome: () => agent }), IdentityAgent: socket, ...Record.getSomes({ IdentitiesOnly: Option.as(key, 'yes'), ForwardAgent: Option.liftPredicate(socket, () => forwarded) }) }),
        None: () => ({ IdentityFile: 'none', IdentityAgent: 'none' }),
    }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { ControlPath, Host, Line, SshError };
