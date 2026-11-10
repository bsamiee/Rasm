import { connect, createServer, type NetConnectOpts } from 'node:net';
import process from 'node:process';
import type { Readable } from 'node:stream';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { Forward, Host, Ssh, SshError } from '@rasm/ssh';
import { Array, Cause, Context, Effect, FileSystem, Layer, Path, pipe, Record, Schedule, Schema, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { GenericContainer, getContainerRuntimeClient, Network } from 'testcontainers';
import { Asked, Reached } from '#/peer';

// --- [CONSTANTS] -----------------------------------------------------------------------

const served = 22;
const callers = 30;
const stated = { persist: 30_000, timeout: 5000, alive: { interval: 1000, count: 2 } };
const within = 1000;
const bounds = { started: '3 minutes', test: 120_000 } as const;
const greeting = { port: 7007, text: 'greeted' };
const greeted = new TextEncoder().encode(greeting.text);
const socks = { asked: Uint8Array.fromHex('050100'), accepted: Uint8Array.fromHex('0500') };

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

class Sshd extends Context.Service<Sshd>()('@rasm/test-ssh/sshd.test/Sshd', {
    make: Effect.gen(function* () {
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
        const listening = `/usr/sbin/sshd -D -e -p ${served} -h /keys/server -o HostCertificate=/keys/server-cert.pub -o AuthorizedKeysFile=/keys/authorized -o TrustedUserCAKeys=/keys/authority.pub -o StrictModes=no -o AcceptEnv=* -o AllowTcpForwarding=yes -o MaxSessions=${callers}`;
        const copied = [
            { source: files.server, target: '/keys/server', mode: 0o600 },
            { source: `${files.server}-cert.pub`, target: '/keys/server-cert.pub' },
            { source: `${files.client}.pub`, target: '/keys/authorized' },
            { source: `${files.authority}.pub`, target: '/keys/authority.pub' },
        ];
        const [network, runtime] = yield* Effect.all([Effect.acquireDisposable(Effect.promise(() => new Network().start())), Effect.promise(() => getContainerRuntimeClient())]);
        const serving = Effect.fnUntraced(function* (alias: string) {
            const defined = new GenericContainer('alpine:3.23').withNetwork(network).withNetworkAliases(alias).withCopyFilesToContainer(copied).withExposedPorts(served).withLogConsumer(logged(alias));
            const container = yield* Effect.acquireDisposable(Effect.promise(() => defined.withCommand(['sh', '-c', `apk add --no-cache openssh >/dev/null && { nc -ll -p ${greeting.port} -e echo ${greeting.text} & } && ${listening}`]).start()));
            const engine = runtime.container.getById(container.getId());
            const paused = <A, E, R>(effect: Effect.Effect<A, E, R>) =>
                Effect.andThen(
                    Effect.promise(() => engine.pause()),
                    Effect.ensuring(
                        effect,
                        Effect.promise(() => engine.unpause()),
                    ),
                );
            return { alias, name: container.getHostname(), dial: { host: container.getHost(), port: container.getMappedPort(served) }, paused };
        });
        const [far, near] = yield* Effect.all([serving('far'), serving('near')], { concurrency: 'unbounded' });
        const published = (file: string) => Effect.map(fs.readFileString(`${file}.pub`), (line) => line.trim());
        const [trusted, signer] = yield* Effect.all([published(files.server), published(files.authority)]);
        const hosted = (name: string, fields: Readonly<Record<string, unknown>> = {}) => Schema.decodeEffect(Host)({ name, user: 'root', ...far.dial, identity: { _tag: 'Key', file: files.client }, trust: { _tag: 'HostKeys', keys: [trusted] }, path: { _tag: 'Direct', preferred: [] }, ...stated, ...fields });
        const asked = Effect.fnUntraced(function* (host: Host, ...request: Array.NonEmptyReadonlyArray<string>) {
            const words = host.request(host.control(path, folder), ...request);
            const handle = yield* spawner.spawn(ChildProcess.make(Array.headNonEmpty(words), Array.tailNonEmpty(words), { stdin: 'ignore' }));
            return yield* Effect.andThen(handle.exitCode, Stream.mkString(Stream.decodeText(handle.stderr)));
        }, Effect.scoped);
        return { folder, files, hosted, asked, far, near, trusted, signer };
    }),
}) {
    static readonly layer = Layer.unwrap(Effect.map(Sshd, ({ folder }) => Ssh.layer(folder))).pipe(Layer.provideMerge(Layer.effect(this, this.make)), Layer.provideMerge(layer));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Sshd.layer, { timeout: bounds.started, excludeTestServices: true })('sshd', (methods) => {
    methods.effect(
        'runs a command with its environment and reports its exit',
        Effect.fnUntraced(function* () {
            const [ssh, sshd] = yield* Effect.all([Ssh, Sshd]);
            const host = yield* sshd.hosted('commands');
            const [sent, failed] = yield* Effect.all([ssh.run(host, ['sh', '-c', 'printf "[%s][%s]" "$GREETING" "$SECOND"'], { env: { GREETING: 'two words "quoted"', SECOND: 'second' } }), Effect.flip(ssh.run(host, ['sh', '-c', 'echo printed; echo refused >&2; exit 7']))]);
            assert.strictEqual(sent, '[two words "quoted"][second]');
            assert.deepStrictEqual(Schema.is(SshError)(failed) ? failed.reason : failed, { _tag: 'Exit', code: 7, stdout: 'printed\n', stderr: 'refused\n' });
        }),
        bounds.test,
    );

    methods.effect(
        'shares one connection across concurrent callers of two clients on one folder',
        Effect.fnUntraced(function* () {
            const [ssh, sshd] = yield* Effect.all([Ssh, Sshd]);
            const [host, second] = yield* Effect.all([sshd.hosted('shared'), Ssh.make(sshd.folder)]);
            const first = yield* Effect.forEach(Array.range(1, callers), (index) => (index % 2 === 0 ? ssh : second).run(host, connection), { concurrency: 'unbounded' });
            const later = yield* Effect.forEach(Array.range(1, callers), () => ssh.run(host, connection), { concurrency: 'unbounded' });
            assert.isAtMost(new Set(first).size, 2);
            assert.strictEqual(new Set(later).size, 1);
        }),
        bounds.test,
    );

    methods.effect(
        'takes the first preferred dial a login answers, on shared and own connections, and the stated dial when none answers',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, far, near }] = yield* Effect.all([Ssh, Sshd]);
            const dials = { answering: { ...near.dial, within }, silent: { host: '192.0.2.1', within }, refusing: { host: near.dial.host, port: 1, within }, inheriting: { host: near.dial.host, within }, later: { ...far.dial, within } };
            const hosts = yield* Effect.all({
                answered: hosted('answered', { path: { _tag: 'Direct', preferred: [dials.answering] } }),
                refused: hosted('refused', { path: { _tag: 'Direct', preferred: [dials.refusing] } }),
                silent: hosted('silent', { path: { _tag: 'Direct', preferred: [dials.silent] } }),
                second: hosted('second', { path: { _tag: 'Direct', preferred: [dials.silent, dials.answering] } }),
                inherited: hosted('inherited', { ...near.dial, path: { _tag: 'Direct', preferred: [dials.inheriting, dials.later] } }),
            });
            const shared = yield* Effect.all(
                Record.map(hosts, (host) => reached(ssh, host)),
                { concurrency: 'unbounded' },
            );
            const own = yield* Effect.all(
                Record.map(hosts, (host) => reached(ssh, host, { env: { SENT: 'sent' } })),
                { concurrency: 'unbounded' },
            );
            const expected = { answered: near.name, refused: far.name, silent: far.name, second: near.name, inherited: near.name };
            assert.deepStrictEqual(shared, expected);
            assert.deepStrictEqual(own, expected);
        }),
        bounds.test,
    );

    methods.effect(
        'keeps a shared connection on its dial while it lives, dials again when that dial stops answering, and decides again for each next one',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, asked, far, near }] = yield* Effect.all([Ssh, Sshd]);
            const host = yield* hosted('flipped', { path: { _tag: 'Direct', preferred: [{ ...near.dial, within }] } });
            const every = Effect.map(
                Effect.forEach(Array.range(1, callers), () => reached(ssh, host), { concurrency: 'unbounded' }),
                Array.dedupe,
            );
            const decided = Effect.andThen(asked(host, 'exit'), every);
            const first = yield* every;
            const stopped = yield* near.paused(Effect.all([reached(ssh, host), every]));
            const resumed = yield* every;
            const flips = yield* Effect.forEach(Array.range(1, 2), () => Effect.all([near.paused(decided), decided]));
            assert.deepStrictEqual(first, [near.name]);
            assert.deepStrictEqual(stopped, [far.name, [far.name]]);
            assert.deepStrictEqual(resumed, [far.name]);
            assert.deepStrictEqual(flips.flat(2), [far.name, near.name, far.name, near.name]);
        }),
        bounds.test,
    );

    methods.effect(
        'fails the call of a killed shared connection and opens one connection for every caller after it',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, asked, folder, near }, fs, path] = yield* Effect.all([Ssh, Sshd, FileSystem.FileSystem, Path.Path]);
            const host = yield* hosted('killed', { path: { _tag: 'Direct', preferred: [{ ...near.dial, within }] } });
            const running = yield* ssh.spawn(host, ['sh', '-c', 'echo started; exec sleep 600']);
            yield* printed(running, 'the remote command');
            const [, pid] = yield* Effect.flatMap(asked(host, 'check'), (shown) => Schema.decodeUnknownEffect(Master)(shown.trim()));
            yield* Effect.sync(() => process.kill(pid, 'SIGKILL'));
            const [code, left] = yield* Effect.all([running.exitCode, fs.exists(host.control(path, folder))]);
            const after = yield* Effect.forEach(Array.range(1, callers), () => ssh.run(host, ['sh', '-c', 'echo "$SSH_CONNECTION"; hostname']), { concurrency: 'unbounded' });
            assert.notStrictEqual(code, 0);
            assert.isTrue(left);
            assert.strictEqual(new Set(after).size, 1);
            assert.isTrue(after.every((shown) => shown.endsWith(`${near.name}\n`)));
        }, Effect.scoped),
        bounds.test,
    );

    methods.effect(
        'shares one connection with a second process on the folder and ends the forward of that process with it',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, folder }, fs, path, spawner] = yield* Effect.all([Ssh, Sshd, FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
            const [host, bind] = yield* Effect.all([hosted('processes'), Effect.map(fs.makeTempDirectoryScoped(), (made) => path.join(made, 'sshd'))]);
            const forward = yield* Schema.decodeEffect(Forward)({ _tag: 'Local', bind, connect: `127.0.0.1:${greeting.port}` });
            const request = yield* Schema.encodeEffect(Asked)({ folder, host, forward, calls: callers, remote: connection });
            const peer = yield* spawner.spawn(ChildProcess.make(process.execPath, [path.join(import.meta.dirname, 'peer.ts'), request], { stdin: 'ignore', stderr: 'inherit' }));
            const [mine, theirs] = yield* Effect.all([Effect.forEach(Array.range(1, callers), () => ssh.run(host, connection), { concurrency: 'unbounded' }), Effect.flatMap(printed(peer, 'the second process'), Schema.decodeEffect(Reached))], { concurrency: 'unbounded' });
            const held = yield* answered({ path: bind }, greeted);
            yield* peer.kill({ killSignal: 'SIGKILL' });
            const ended = yield* Effect.repeat(answered({ path: bind }, greeted), { until: (open) => !open, schedule: Schedule.spaced('250 millis') });
            assert.strictEqual(theirs.length, callers);
            assert.isAtMost(new Set([...mine, ...theirs]).size, 2);
            assert.isTrue(held);
            assert.isFalse(ended);
        }, Effect.scoped),
        bounds.test,
    );

    methods.effect(
        'forwards a local socket, a remote port the server allocates, and a SOCKS port for the life of each scope',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, far, trusted }, fs, path] = yield* Effect.all([Ssh, Sshd, FileSystem.FileSystem, Path.Path]);
            const [host, bind, port] = yield* Effect.all([hosted('forwards'), Effect.map(fs.makeTempDirectoryScoped(), (made) => path.join(made, 'sshd')), vacant]);
            const forwards = yield* Effect.all({
                local: Schema.decodeEffect(Forward)({ _tag: 'Local', bind, connect: `127.0.0.1:${greeting.port}` }),
                remote: Schema.decodeEffect(Forward)({ _tag: 'Remote', bind: '127.0.0.1:0', connect: `${far.dial.host}:${far.dial.port}` }),
                dynamic: Schema.decodeEffect(Forward)({ _tag: 'Dynamic', bind: `127.0.0.1:${port}` }),
            });
            const local = yield* Effect.scoped(Effect.andThen(ssh.forward(host, forwards.local), answered({ path: bind }, greeted)));
            const dynamic = yield* Effect.scoped(Effect.andThen(ssh.forward(host, forwards.dynamic), answered({ host: '127.0.0.1', port }, socks.accepted, socks.asked)));
            const scanned = yield* Effect.gen(function* () {
                const { bind: listening } = yield* Effect.flatMap(ssh.forward(host, forwards.remote), Schema.encodeEffect(Forward));
                const { port: allocated } = yield* Schema.decodeEffect(Schema.HostPortFromString)(listening);
                return { allocated, keys: yield* ssh.run(host, ['ssh-keyscan', '-t', 'ed25519', '-p', `${allocated}`, '127.0.0.1']) };
            }).pipe(Effect.scoped);
            assert.isTrue(local);
            assert.isTrue(dynamic);
            assert.isFalse(yield* answered({ path: bind }, greeted));
            assert.isFalse(yield* answered({ host: '127.0.0.1', port }, socks.accepted, socks.asked));
            assert.notStrictEqual(scanned.allocated, 0);
            assert.include(scanned.keys, trusted);
        }, Effect.scoped),
        bounds.test,
    );

    methods.effect(
        'reaches a host through a hop on every call',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, files, far, near, trusted }] = yield* Effect.all([Ssh, Sshd]);
            const hop = { name: 'hop', user: 'root', ...far.dial, identity: { _tag: 'Key', file: files.client }, trust: { _tag: 'HostKeys', keys: [trusted] } };
            const host = yield* hosted('jumped', { host: near.alias, port: served, path: { _tag: 'Jump', hops: [hop] } });
            const names = yield* Effect.all([reached(ssh, host), reached(ssh, host), reached(ssh, host, { env: { SENT: 'sent' } })]);
            assert.deepStrictEqual(names, [near.name, near.name, near.name]);
        }),
        bounds.test,
    );

    methods.effect(
        'authenticates through an agent, and through a certificate against a host an authority signed',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, files, far, signer }, fs, path, spawner] = yield* Effect.all([Ssh, Sshd, FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
            const socket = path.join(yield* fs.makeTempDirectoryScoped(), 'agent');
            yield* pipe(
                spawner.spawn(ChildProcess.make('ssh-agent', ['-D', '-a', socket], { stdin: 'ignore' })),
                Effect.flatMap((agent) => printed(agent, 'ssh-agent')),
            );
            const added = yield* spawner.exitCode(ChildProcess.make('ssh-add', ['-q', files.client], { env: { SSH_AUTH_SOCK: socket }, extendEnv: true, stdin: 'ignore' }));
            const hosts = yield* Effect.all({
                agent: hosted('agent', { identity: { _tag: 'Agent', socket } }),
                certified: hosted('certified', { identity: { _tag: 'Key', file: files.certified, certificate: files.certificate }, trust: { _tag: 'Authority', keys: [signer] } }),
                bare: hosted('bare', { identity: { _tag: 'Key', file: files.certified } }),
            });
            assert.strictEqual(added, 0);
            assert.strictEqual(yield* reached(ssh, hosts.agent), far.name);
            assert.strictEqual(yield* reached(ssh, hosts.certified), far.name);
            assert.strictEqual(reasoned(yield* Effect.flip(reached(ssh, hosts.bare))), 'Unreachable');
        }, Effect.scoped),
        bounds.test,
    );

    methods.effect(
        'reports a host no dial reaches as unreachable, then reaches it once a dial answers',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, far }] = yield* Effect.all([Ssh, Sshd]);
            const failed = yield* Effect.flip(reached(ssh, yield* hosted('unreachable', { port: 1 })));
            assert.strictEqual(reasoned(failed), 'Unreachable');
            assert.strictEqual(yield* reached(ssh, yield* hosted('unreachable')), far.name);
        }),
        bounds.test,
    );

    methods.effect(
        'places a file the caller names beside the host files, in a folder that did not exist',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, folder }, fs, path] = yield* Effect.all([Ssh, Sshd, FileSystem.FileSystem, Path.Path]);
            const kept = path.join(folder, 'elsewhere', 'kept');
            yield* ssh.written(yield* hosted('placed'), { [kept]: 'kept' });
            assert.strictEqual(yield* fs.readFileString(kept), 'kept');
        }),
        bounds.test,
    );
});
