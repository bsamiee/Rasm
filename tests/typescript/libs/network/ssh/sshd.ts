import { connect, createServer, type NetConnectOpts } from 'node:net';
import process from 'node:process';
import type { Readable } from 'node:stream';
import { layer } from '@effect/platform-node/NodeServices';
import { Host, Ssh, SshError } from '@rasm/ssh';
import { Array, Cause, Context, Effect, FileSystem, Layer, Path, pipe, Record, Schema, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { GenericContainer, getContainerRuntimeClient, Network } from 'testcontainers';

// --- [TYPES] ---------------------------------------------------------------------------

type Listeners = Record.ReadonlyRecord<string, { readonly options: readonly string[]; readonly keyOptions: readonly string[] }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const served = 22;
const callers = 30;
const stated = { persist: 30_000, timeout: 5000, alive: { interval: 1000, count: 2 } };
const within = 1000;
const bounds = { started: '3 minutes', test: 120_000 } as const;
const greeting = { port: 7007, text: 'greeted' };
const greeted = new TextEncoder().encode(greeting.text);
const banner = new TextEncoder().encode('SSH-2.0-');

// --- [MODELS] --------------------------------------------------------------------------

const Master = Schema.TemplateLiteralParser(['Master running (pid=', Schema.FiniteFromString, ')']);

// --- [OPERATIONS] ----------------------------------------------------------------------

const connection = Array.make('sh', '-c', 'echo "$SSH_CONNECTION"');
const logged = (alias: string) => (stream: Readable) => stream.on('data', (line) => process.stderr.write(`[${alias}] ${line}`));
const answered = (options: NetConnectOpts, expected: Uint8Array, sent = new Uint8Array()) =>
    Effect.callback<boolean>((resume) => {
        const socket = connect(options, () => socket.write(sent));
        socket
            .once('data', (chunk) => resume(Effect.succeed(expected.every((byte, index) => chunk[index] === byte))))
            .once('error', () => resume(Effect.succeed(false)))
            .once('close', () => resume(Effect.succeed(false)));
        return Effect.sync(() => socket.destroy());
    });
const vacant = Effect.callback<number>((resume) => {
    const server = createServer().listen(0, '127.0.0.1', () => {
        const address = server.address();
        server.close(() => resume(typeof address === 'object' && address !== null ? Effect.succeed(address.port) : Effect.die(address)));
    });
});
const printed = (handle: ChildProcessSpawner.ChildProcessHandle, subject: string) =>
    pipe(handle.stdout, Stream.decodeText, Stream.splitLines, Stream.runHead, Effect.flatMap(Effect.fromOption), Effect.raceFirst(Effect.flatMap(handle.exitCode, (code) => Effect.fail(new Cause.UnknownError(code, `${subject} exited ${code} before it printed a line`)))));
const reasoned = (error: unknown) => (Schema.is(SshError)(error) ? error.reason._tag : error);
const reached = (ssh: Ssh['Service'], host: Host, options?: ChildProcess.CommandOptions) => Effect.map(ssh.run(host, ['hostname'], options), (name) => name.trim());

// --- [SERVICES] ------------------------------------------------------------------------

class Sshd extends Context.Service<Sshd>()('@rasm/test-ssh/sshd', {
    make: Effect.fnUntraced(function* (listeners: Listeners) {
        const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
        const [keys, folder] = yield* Effect.all([fs.makeTempDirectoryScoped(), fs.makeTempDirectoryScoped()]);
        const generated = (...args: readonly string[]) =>
            Effect.filterOrFail(
                spawner.exitCode(ChildProcess.make('ssh-keygen', ['-q', ...args], { stdin: 'ignore' })),
                (code) => code === 0,
                (code) => new Cause.UnknownError(code, `ssh-keygen ${args.join(' ')} exited ${code}`),
            );
        const files = Record.map({ client: 'client', server: 'server', authority: 'authority', certified: 'certified', certificate: 'certificate.pub' }, (name) => path.join(keys, name));
        yield* Effect.forEach([files.client, files.server, files.authority, files.certified], (file) => generated('-t', 'ed25519', '-N', '', '-C', '', '-f', file), { concurrency: 'unbounded' });
        yield* Effect.all([generated('-s', files.authority, '-I', 'client', '-n', 'root', `${files.certified}.pub`), generated('-s', files.authority, '-I', 'server', '-h', '-n', 'certified', `${files.server}.pub`)], { concurrency: 'unbounded' });
        yield* fs.rename(`${files.certified}-cert.pub`, files.certificate);
        const published = (file: string) => Effect.map(fs.readFileString(`${file}.pub`), (line) => line.trim());
        const [trusted, signer, client] = yield* Effect.all([published(files.server), published(files.authority), published(files.client)]);
        const shared = `-h /keys/server -o HostCertificate=/keys/server-cert.pub -o TrustedUserCAKeys=/keys/authority.pub -o StrictModes=no -o AcceptEnv=* -o MaxStartups=100 -o MaxSessions=${callers}`;
        const listening = `/usr/sbin/sshd -D -e -p ${served} ${shared} -o AuthorizedKeysFile=/keys/authorized -o AllowTcpForwarding=yes`;
        const restricted = Array.map(Record.toEntries(listeners), ([name, { options, keyOptions }], index) => ({
            name,
            port: served + 1 + index,
            authorized: { content: [...Array.match(keyOptions, { onEmpty: () => [], onNonEmpty: (given) => [given.join(',')] }), client, '\n'].join(' '), target: `/keys/authorized.${name}` },
            command: ['/usr/sbin/sshd', '-p', served + 1 + index, shared, '-o', `AuthorizedKeysFile=/keys/authorized.${name}`, ...options.map((option) => `-o '${option}'`)].join(' '),
        }));
        const copied = [
            { source: files.server, target: '/keys/server', mode: 0o600 },
            { source: `${files.server}-cert.pub`, target: '/keys/server-cert.pub' },
            { source: `${files.client}.pub`, target: '/keys/authorized' },
            { source: `${files.authority}.pub`, target: '/keys/authority.pub' },
        ];
        const started = [`{ busybox nc -ll -p ${greeting.port} -e echo ${greeting.text} & }`, ...restricted.map(({ command }) => command), listening].join(' && ');
        const [network, runtime] = yield* Effect.all([Effect.acquireDisposable(Effect.tryPromise(() => new Network().start())), Effect.tryPromise(() => getContainerRuntimeClient())]);
        const serving = Effect.fnUntraced(function* (alias: string) {
            const image = yield* Effect.tryPromise(() =>
                GenericContainer.fromDockerfile(import.meta.dirname)
                    .withBuildkit()
                    .build('rasm/test-ssh', { deleteOnExit: false }),
            );
            const defined = image
                .withNetwork(network)
                .withNetworkAliases(alias)
                .withCopyFilesToContainer(copied)
                .withCopyContentToContainer(restricted.map(({ authorized }) => authorized))
                .withExposedPorts(served, ...restricted.map(({ port }) => port))
                .withLogConsumer(logged(alias));
            const container = yield* Effect.acquireDisposable(Effect.tryPromise(() => defined.withCommand(['sh', '-c', started]).start()));
            const engine = runtime.container.getById(container.getId());
            const paused = <A, E, R>(effect: Effect.Effect<A, E, R>) =>
                Effect.andThen(
                    Effect.tryPromise(() => engine.pause()),
                    Effect.ensuring(
                        effect,
                        Effect.promise(() => engine.unpause()),
                    ),
                );
            const dials = Record.fromIterableWith(restricted, ({ name, port }) => [name, { host: container.getHost(), port: container.getMappedPort(port) }]);
            return { alias, name: container.getHostname(), dial: { host: container.getHost(), port: container.getMappedPort(served) }, dials, paused };
        });
        const [far, near] = yield* Effect.all([serving('far'), serving('near')], { concurrency: 'unbounded' });
        const hosted = (name: string, fields: Readonly<Record<string, unknown>> = {}) => Schema.decodeEffect(Host)({ name, user: 'root', ...far.dial, identity: { _tag: 'Key', file: files.client }, trust: { _tag: 'HostKeys', keys: [trusted] }, path: { _tag: 'Direct', preferred: [] }, ...stated, ...fields });
        const asked = Effect.fnUntraced(function* (host: Host, ...request: Array.NonEmptyReadonlyArray<string>) {
            const words = host.request(host.control(path, folder), ...request);
            const handle = yield* spawner.spawn(ChildProcess.make(Array.headNonEmpty(words), Array.tailNonEmpty(words), { stdin: 'ignore' }));
            return yield* Effect.andThen(handle.exitCode, Stream.mkString(Stream.merge(Stream.decodeText(handle.stdout), Stream.decodeText(handle.stderr))));
        }, Effect.scoped);
        return { folder, files, hosted, asked, far, near, trusted, signer };
    }),
}) {
    static readonly layer = (listeners: Listeners = {}) => Layer.unwrap(Effect.map(Sshd, ({ folder }) => Ssh.layer(folder))).pipe(Layer.provideMerge(Layer.effect(this, this.make(listeners))), Layer.provideMerge(layer));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { answered, banner, bounds, callers, connection, greeted, greeting, type Listeners, Master, printed, reached, reasoned, Sshd, served, vacant, within };
