import { connect, type NetConnectOpts } from 'node:net';
import { layer } from '@effect/platform-node/NodeServices';
import { fromReadable } from '@effect/platform-node/NodeStream';
import { Host, Ssh, SshError } from '@rasm/ssh';
import { Array, Cause, Context, Deferred, Effect, Fiber, FileSystem, Formatter, Layer, Path, pipe, Queue, Record, Schema, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { GenericContainer, Wait } from 'testcontainers';

// --- [CONSTANTS] -----------------------------------------------------------------------

const served = 22;
const spare = Array.range(2201, 2216);
const bounds = { started: '3 minutes', test: 60_000 } as const;
const principal = 'certified';
const policy = ['-f', '/dev/null', '-h', '/keys/server', '-o', 'Subsystem=sftp internal-sftp', '-o', 'HostCertificate=/keys/server-cert.pub', '-o', 'TrustedUserCAKeys=/keys/authority.pub', '-o', 'StrictModes=no', '-o', 'AcceptEnv=*', '-o', 'MaxStartups=100', '-o', 'PerSourcePenalties=no', '-o', 'PidFile=none'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const held = Effect.fnUntraced(function* (options: NetConnectOpts, sent: Uint8Array = new Uint8Array()) {
    const first = yield* Deferred.make<string>();
    const answers = fromReadable({
        evaluate: () => {
            const socket = connect(options, () => {
                socket.write(sent);
            });
            return socket;
        },
    });
    const closed = yield* Effect.forkScoped(Effect.ensuring(Effect.ignore(Stream.runForEach(answers, (chunk) => Deferred.succeed(first, new TextDecoder('latin1').decode(chunk)))), Deferred.succeed(first, '')));
    return { first: yield* Deferred.await(first), closed: Fiber.join(closed) };
});
const heard = (options: NetConnectOpts, sent?: Uint8Array) => Effect.scoped(Effect.map(held(options, sent), ({ first }) => first));
const reasoned = <A, E, R>(effect: Effect.Effect<A, E, R>) =>
    Effect.match(effect, {
        onSuccess: () => 'reached',
        onFailure: (error) => (Schema.is(SshError)(error) ? SshError.fields.reason.match(error.reason, { Exit: ({ exit }) => `Exit ${exit.code}`, Unreachable: () => 'Unreachable', Refused: () => 'Refused' }) : Formatter.format(error)),
    });
const printed = (handle: ChildProcessSpawner.ChildProcessHandle, subject: string) =>
    pipe(handle.stdout, Stream.decodeText, Stream.splitLines, Stream.runHead, Effect.flatMap(Effect.fromOption), Effect.raceFirst(Effect.flatMap(handle.exitCode, (code) => Effect.fail(new Cause.UnknownError(code, `${subject} exited ${code} before it printed a line`)))));

// --- [SERVICES] ------------------------------------------------------------------------

class Sshd extends Context.Service<Sshd>()('@rasm/test-ssh/sshd', {
    make: Effect.gen(function* () {
        const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
        const keys = yield* fs.makeTempDirectoryScoped();
        const generated = (...args: readonly string[]) =>
            Effect.filterOrFail(
                spawner.exitCode(ChildProcess.make('ssh-keygen', ['-q', ...args], { stdin: 'ignore' })),
                (code) => code === 0,
                (code) => new Cause.UnknownError(code, `ssh-keygen ${args.join(' ')} exited ${code}`),
            );
        const files = Record.map({ client: 'client', server: 'server', authority: 'authority', certified: 'certified', certificate: 'certificate.pub' }, (name) => path.join(keys, name));
        yield* Effect.forEach([files.client, files.server, files.authority, files.certified], (file) => generated('-t', 'ed25519', '-N', '', '-C', '', '-f', file), { concurrency: 'unbounded' });
        yield* Effect.all([generated('-s', files.authority, '-I', 'client', '-n', 'root', `${files.certified}.pub`), generated('-s', files.authority, '-I', 'server', '-h', '-n', principal, `${files.server}.pub`)], { concurrency: 'unbounded' });
        yield* fs.rename(`${files.certified}-cert.pub`, files.certificate);
        const published = (file: string) => Effect.map(fs.readFileString(`${file}.pub`), (line) => line.trim());
        const [trusted, signer, authorized] = yield* Effect.all([published(files.server), published(files.authority), published(files.client)]);
        const image = yield* Effect.tryPromise({
            try: () =>
                GenericContainer.fromDockerfile(import.meta.dirname)
                    .withBuildkit()
                    .build('rasm/test-ssh', { deleteOnExit: false }),
            catch: (cause) => new Cause.UnknownError(cause, 'rasm/test-ssh image failed to build'),
        });
        const defined = image
            .withExposedPorts(served, ...spare)
            .withTmpFs({ '/tmp': 'rw' })
            .withCopyFilesToContainer([
                { source: files.server, target: '/keys/server', mode: 0o600 },
                { source: `${files.server}-cert.pub`, target: '/keys/server-cert.pub' },
                { source: `${files.authority}.pub`, target: '/keys/authority.pub' },
            ])
            .withCopyContentToContainer([{ content: `${authorized}\n`, target: `/keys/authorized.${served}` }])
            .withCommand(['/usr/sbin/sshd', '-D', '-e', '-p', `${served}`, ...policy, '-o', `AuthorizedKeysFile=/keys/authorized.${served}`])
            .withWaitStrategy(Wait.forLogMessage(`Server listening on 0.0.0.0 port ${served}.`));
        const container = yield* Effect.acquireDisposable(Effect.tryPromise({ try: () => defined.start(), catch: (cause) => new Cause.UnknownError(cause, 'sshd container failed to start') }));
        const executed = (...words: readonly string[]) =>
            Effect.tryPromise({ try: () => container.exec([...words]), catch: (cause) => new Cause.UnknownError(cause, `the container refused ${words.join(' ')}`) }).pipe(
                Effect.filterOrFail(
                    ({ exitCode }) => exitCode === 0,
                    ({ exitCode, output }) => new Cause.UnknownError(output, `${words.join(' ')} exited ${exitCode} in the container: ${output}`),
                ),
                Effect.map(({ stdout }) => stdout),
            );
        const vacant = yield* Queue.bounded<number>(spare.length);
        yield* Queue.offerAll(vacant, spare);
        const mapped = (inside: number) => ({ inside, dial: { host: container.getHost(), port: container.getMappedPort(inside) } });
        const port = Effect.map(
            Effect.acquireRelease(Queue.take(vacant), (taken) => Queue.offer(vacant, taken)),
            mapped,
        );
        const ended = (...words: readonly string[]) => Effect.promise(() => container.exec([...words]));
        const listening = Effect.fnUntraced(function* (inside: number, options: readonly string[] = [], keyOptions: readonly string[] = []) {
            const line = Array.match(keyOptions, { onEmpty: () => authorized, onNonEmpty: (given) => `${given.join(',')} ${authorized}` });
            yield* Effect.tryPromise({
                try: () => container.copyContentToContainer([{ content: `${line}\n`, target: `/keys/authorized.${inside}` }]),
                catch: (cause) => new Cause.UnknownError(cause, `the container refused the authorized keys of port ${inside}`),
            });
            const started = executed('/usr/sbin/sshd', '-p', `${inside}`, ...policy, '-o', `AuthorizedKeysFile=/keys/authorized.${inside}`, ...options.flatMap((option) => ['-o', option]));
            return yield* Effect.acquireRelease(started, () => ended('pkill', '-KILL', '-f', `^sshd: /usr/sbin/sshd -p ${inside} `));
        });
        const scratch = Effect.acquireRelease(
            Effect.map(executed('mktemp', '-d'), (made) => made.trim()),
            (made) => ended('rm', '-rf', made),
        );
        const hosted = (name: string, fields: Readonly<Record<string, unknown>> = {}) =>
            Schema.decodeEffect(Host)({ name, user: 'root', ...mapped(served).dial, identity: { _tag: 'Key', file: files.client }, trust: { _tag: 'HostKeys', keys: [trusted] }, path: { _tag: 'Direct', preferred: [] }, persist: 30_000, ...fields });
        const client = Effect.gen(function* () {
            const folder = yield* fs.makeTempDirectoryScoped();
            const ssh = yield* Ssh.make(folder);
            const exited = (name: string) => spawner.exitCode(ChildProcess.make('ssh', ['-F', 'none', '-S', path.join(folder, name, 'control'), '-O', 'exit', name], { stdin: 'ignore', stdout: 'ignore', stderr: 'ignore' }));
            yield* Effect.addFinalizer(() => Effect.ignore(Effect.flatMap(fs.readDirectory(folder), Effect.forEach(exited, { concurrency: 'unbounded' }))));
            const asked = (host: Host, ...request: Array.NonEmptyReadonlyArray<string>) => {
                const words = host.request(host.control(path, folder), ...request);
                return spawner.string(ChildProcess.make(Array.headNonEmpty(words), Array.tailNonEmpty(words), { stdin: 'ignore' }), { includeStderr: true });
            };
            return { ssh, folder, asked };
        });
        return { files, trusted, signer, name: container.getHostname(), served: mapped(served), port, listening, scratch, hosted, client };
    }),
}) {
    static readonly layer = Layer.provideMerge(Layer.effect(this, this.make), layer);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { bounds, heard, held, principal, printed, reasoned, Sshd, served };
