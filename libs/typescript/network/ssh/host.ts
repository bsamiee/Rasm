import { Array, Duration, flow, Option, type Path, Record, Result, Schema } from 'effect';
import { CommandLine } from 'effect/process';
import SSHConfig from 'ssh-config';

// --- [CONSTANTS] -----------------------------------------------------------------------

const dialed = 'dialed';

// --- [MODELS] --------------------------------------------------------------------------

const Line = Schema.NonEmptyString.check(Schema.isPattern(/^[^\0\n\r"'\\%]+$/u)).annotate({
    description: 'One value ssh_config, known_hosts, and sh read as written, free of quotes, backslash, percent, newline, and NUL',
});
const Seconds = Schema.DurationFromMillis.check(Schema.makeFilter(flow(Duration.toSeconds, Number.isInteger), { description: 'Whole seconds, the unit OpenSSH reads' }), Schema.makeIsGreaterThan({ order: Duration.Order })(Duration.zero));
const dial = { host: Schema.HostFromString.pipe(Schema.decodeTo(Schema.flip(Schema.HostFromString))), port: Schema.OptionFromOptionalKey(Schema.Port) };
const endpoint = {
    name: Schema.DomainNameFromString.annotate({ description: 'ssh_config Host alias, HostKeyAlias, and folder of its files' }),
    user: Line,
    identity: Schema.TaggedUnion({ Key: { file: Line, certificate: Schema.OptionFromOptionalKey(Line) }, Agent: { socket: Line } }).annotate({
        description: 'Key names a private key file and its certificate file the caller keeps, Agent the socket of an agent holding the key',
    }),
    trust: Schema.TaggedUnion({ HostKeys: { keys: Schema.NonEmptyArray(Line) }, Authority: { keys: Schema.NonEmptyArray(Line) } }),
    ...dial,
};
const Hop = Schema.Struct(endpoint);
const Preferred = Schema.Struct({ ...dial, within: Seconds }).annotate({ description: 'Dial tried before host and port, on the host port when it states none, taken when a login there answers within its time' });
class Host extends Schema.Class<Host, { readonly brand: unique symbol }>('Host')({
    ...endpoint,
    path: Schema.TaggedUnion({ Direct: { preferred: Schema.Array(Preferred) }, Jump: { hops: Schema.NonEmptyArray(Hop) } }).annotate({
        description: 'Direct connects to the first preferred dial a login answers, else to host and port, chosen by each connection that starts; Jump connects through its hops in order',
    }),
    persist: Seconds.annotate({ description: 'ControlPersist, the time an idle shared connection stays open for the next ssh process' }),
    alive: Schema.OptionFromOptionalKey(Schema.Struct({ interval: Seconds, count: Schema.Int.check(Schema.isGreaterThan(0)) })),
    timeout: Schema.OptionFromOptionalKey(Seconds),
}) {
    // --- [FILES]

    config(path: Path.Path, folder: string) {
        return path.join(folder, this.name, 'config');
    }

    control(path: Path.Path, folder: string) {
        return path.join(folder, this.name, 'control');
    }

    files(path: Path.Path, folder: string) {
        const [known, control, unshared] = [path.join(folder, this.name, 'known_hosts'), this.control(path, folder), this.unshared];
        const quiet = (words: readonly string[]) => Result.map(CommandLine.join(words), (line) => `${line} 2>/dev/null`);
        const marked = Host.fields.trust.match({ HostKeys: () => [], Authority: () => ['@cert-authority'] });
        const [dials, routed, hops] = Host.fields.path.match(this.path, {
            Direct: ({ preferred }) => [preferred, {}, []] as const,
            Jump: ({ hops: jumped }) => [[], { ProxyJump: jumped.map(({ name }) => name).join(',') }, jumped] as const,
        });
        const probes = dials.map(({ host, port, within }) => {
            const taken = { HostName: host, ...Record.getSomes({ Port: Option.map(port, String) }) };
            const stated = Record.toEntries({ ...taken, ConnectTimeout: `${Duration.toSeconds(within)}` }).flatMap(([option, value]) => ['-o', `${option}=${value}`]);
            return Result.map(quiet(['ssh', '-F', this.config(path, folder), '-P', dialed, ...stated, unshared, 'true']), (exec) => ({ Match: ['originalhost', `${this.name},${unshared}`, '!tagged', dialed, 'exec', exec], ...taken, Tag: dialed }));
        });
        const held = Array.match(probes, { onEmpty: () => [], onNonEmpty: () => [Result.map(quiet(this.request(control, 'check')), (exec) => ({ Match: ['originalhost', this.name, 'exec', exec], Tag: dialed }))] });
        const sections = [
            { Host: [this.name, unshared], ...section(this), ...routed },
            { Host: this.name, ControlMaster: 'auto', ControlPath: [control], ControlPersist: `${Duration.toSeconds(this.persist)}` },
            ...hops.map((hop) => ({ Host: hop.name, ...section(hop) })),
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
        ];
        return Result.map(Result.all([...held, ...probes]), (matched) => ({
            [known]: [this, ...hops].flatMap(({ name, trust }) => trust.keys.map((key) => `${[...marked(trust), name, key].join(' ')}\n`)).join(''),
            [this.config(path, folder)]: [...matched, ...sections].reduce((config, each) => config.append(each), new SSHConfig()).toString(),
        }));
    }

    // --- [COMMANDS]

    get unshared() {
        return `${this.name}.unshared`;
    }

    request(socket: string, ...asked: Array.NonEmptyReadonlyArray<string>) {
        return Array.make('ssh', '-F', 'none', '-S', socket, '-O', ...asked, this.name);
    }

    client(path: Path.Path, folder: string, ...options: readonly string[]) {
        return Array.make('env', '-u', 'SHELL', 'ssh', '-F', this.config(path, folder), ...options, this.name);
    }

    command(path: Path.Path, folder: string, remote: Array.NonEmptyReadonlyArray<string>, sent: readonly string[] = [], socket: Option.Option<string> = Option.none()) {
        const own = [...Option.match(socket, { onNone: () => [], onSome: (mastered) => ['-M', '-S', mastered] }), ...Array.match(sent, { onEmpty: () => [], onNonEmpty: (names) => ['-o', `SendEnv=${names.join(' ')}`] })];
        const words = Array.match(own, { onEmpty: () => this.client(path, folder), onNonEmpty: (options) => Array.make('env', '-u', 'SHELL', 'ssh', '-F', this.config(path, folder), ...options, this.unshared) });
        return Result.map(CommandLine.join(remote), (line) => Array.append(words, line));
    }

    copy(path: Path.Path, folder: string, source: string, target: string) {
        return Array.make('env', '-u', 'SHELL', 'scp', '-F', this.config(path, folder), '-r', '--', source, `${this.name}:${target}`);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const section = ({ name, user, identity, host, port }: typeof Hop.Type) => ({
    HostName: host,
    User: user,
    HostKeyAlias: name,
    ...Record.getSomes({ Port: Option.map(port, String) }),
    ...Host.fields.identity.match(identity, {
        Key: ({ file, certificate }) => ({ IdentityFile: file, ...Record.getSomes({ CertificateFile: certificate }), IdentitiesOnly: 'yes', IdentityAgent: 'none' }),
        Agent: ({ socket }) => ({ IdentityAgent: socket }),
    }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host, Line };
