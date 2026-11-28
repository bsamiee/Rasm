import { make as listen } from '@effect/platform-node/NodeSocketServer';
import { ExitError, run, spawn } from '@rasm/process';
import { Array, Cache, Context, Duration, Effect, Exit, FileSystem, Layer, Match, Option, Path, Predicate, Record, Result, Schema, Stream, String } from 'effect';
import { Host as Address, NetAddress } from 'effect/net';
import { type ChildProcess, ChildProcessSpawner, CommandLine } from 'effect/process';
import { Socket } from 'effect/socket';
import { Channel } from '#/channel';
import { Forward } from '#/forward';
import { type Host, Line, SshError } from '#/host';
import { Mux } from '#/mux';

// --- [CONSTANTS] -----------------------------------------------------------------------

const transfer = { upload: (local: string, remote: string) => [local, remote] as const, download: (local: string, remote: string) => [remote, local] as const };
const withheld = { SHELL: undefined };

// --- [SERVICES] ------------------------------------------------------------------------

class Ssh extends Context.Service<Ssh>()('@rasm/ssh/client/Ssh', {
    make: Effect.fnUntraced(function* (folder: string) {
        const [spawner, fs, path, root] = yield* Effect.all([ChildProcessSpawner.ChildProcessSpawner, FileSystem.FileSystem, Path.Path, Schema.decodeEffect(Line)(folder)]);
        const local = Effect.provideService(ChildProcessSpawner.ChildProcessSpawner, spawner);
        const control = (host: Host) => host.control(path, root);
        const placed = yield* Cache.makeWith(
            Effect.fnUntraced(function* (host: Host) {
                const files = Record.toEntries(yield* Effect.fromResult(host.files(path, root)));
                yield* Effect.forEach(Array.dedupe(files.map(([file]) => path.dirname(file))), (home) => fs.makeDirectory(home, { recursive: true, mode: 0o700 }), { discard: true });
                yield* Effect.forEach(
                    files,
                    Effect.fnUntraced(function* ([file, text]) {
                        const staged = path.join(yield* fs.makeTempDirectoryScoped({ directory: path.dirname(file) }), path.basename(file));
                        yield* fs.writeFileString(staged, text, { mode: 0o600 });
                        yield* fs.rename(staged, file);
                    }, Effect.scoped),
                    { concurrency: 'unbounded', discard: true },
                );
            }),
            { capacity: Number.POSITIVE_INFINITY, timeToLive: (exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero) },
        );
        const shared = yield* Cache.makeWith(
            Effect.fnUntraced(function* (host: Host) {
                yield* Cache.get(placed, host);
                const starts = yield* Effect.fromResult(Result.all(host.dials.map((options) => host.command(path, root, options, ['exit']))));
                yield* Effect.firstSuccessOf(starts.map((words) => local(run(words, { within: within(host), env: withheld })))).pipe(
                    Effect.catchTags({
                        ExitError: (exit) => SshError.of({ _tag: 'Unreachable', output: Option.liftPredicate(exit.stderr.trim(), String.isNonEmpty) }, host.name)(exit),
                        TimeoutError: (timeout) => SshError.of({ _tag: 'Unreachable', output: Option.none() }, host.name)(timeout),
                    }),
                    Effect.when(Effect.map(Mux.alive(control(host), host.name, within(host)), Option.isNone)),
                );
            }),
            { capacity: Number.POSITIVE_INFINITY, timeToLive: () => Duration.zero },
        );
        const dial = Effect.fn('Ssh.dial')(function* (host: Host, end: Address.HostPort | NetAddress.UnixPathAddress) {
            yield* opened(shared, host);
            return yield* Channel.dial(control(host), host, end);
        });
        return {
            place: Effect.fn('Ssh.place')(function* (host: Host) {
                return yield* opened(placed, host);
            }),
            dial,
            run: Effect.fn('Ssh.run')(function* <E = never, R = never>(host: Host, words: Array.NonEmptyReadonlyArray<CommandLine.Word>, options?: { readonly env?: ChildProcess.CommandOptions['env']; readonly stdin?: Stream.Stream<Uint8Array, E, R> }) {
                yield* opened(shared, host);
                const { stdout, stderr, exit } = yield* Channel.exec(control(host), host, words, options);
                const [out, err, code] = yield* Effect.all([stdout.pipe(Stream.decodeText, Stream.mkString), stderr.pipe(Stream.decodeText, Stream.mkString), exit], { concurrency: 'unbounded' });
                return code === 0 ? out : yield* SshError.of({ _tag: 'Exit', exit: ExitError.of({ command: CommandLine.display(words, { format: host.shell }), code, stdout: out, stderr: err }) }, host.name)();
            }, Effect.scoped),
            spawn: Effect.fn('Ssh.spawn')(function* (host: Host, words: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) {
                const sent = Record.keys(Record.filter(options?.env ?? {}, Predicate.isNotUndefined)).flatMap((name) => ['-o', `SendEnv=${name}`]);
                const [ready, flags] = Array.isReadonlyArrayNonEmpty(sent) ? ([opened(placed, host), ['-S', 'none', ...sent]] as const) : ([opened(shared, host), []] as const);
                yield* ready;
                return yield* local(spawn(yield* Effect.fromResult(host.command(path, root, flags, words)), { ...options, env: { ...options?.env, ...withheld } }));
            }),
            copy: Effect.fn('Ssh.copy')(function* (host: Host, direction: keyof typeof transfer, from: string, to: string, deadline?: Duration.Input) {
                yield* opened(shared, host);
                const [source, target] = transfer[direction](path.resolve(from), host.remote(to));
                yield* local(run(host.copy(path, root, source, target), { within: deadline, env: withheld })).pipe(Effect.catchTag('ExitError', (exit) => SshError.of({ _tag: 'Exit', exit }, host.name)()));
            }),
            forward: Effect.fn('Ssh.forward')(function* (host: Host, wanted: typeof Forward.Type) {
                yield* opened(shared, host);
                if (!Forward.guards.Local(wanted)) {
                    yield* Mux.proxy(control(host), host.name, within(host));
                    return yield* Effect.acquireRelease(Mux.forward(control(host), host.name, wanted, within(host)), () => Effect.ignore(Mux.cancel(control(host), host.name, wanted, within(host))));
                }
                const [bound, shown] = Match.valueTags(wanted.bind, { HostPort: (at) => [{ host: Address.formatHost(at.host), port: at.port }, Address.formatHostPort(at)] as const, UnixPathAddress: (at) => [{ path: at.path }, NetAddress.formatUnixPath(at)] as const });
                const server = yield* listen({ ...bound, allowHalfOpen: true }).pipe(Effect.mapError(SshError.of({ _tag: 'Refused', answer: `bind ${shown}` }, host.name)));
                yield* Effect.forkScoped(server.run((inbound) => carry(inbound, dial(host, wanted.connect))));
                return wanted;
            }),
        };
    }),
}) {
    static readonly layer = (folder: string) => Layer.effect(this, this.make(folder));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const within = (host: Host) => Option.getOrUndefined(host.timeout);
const opened = <E>(cache: Cache.Cache<Host, void, E>, host: Host) => Effect.andThen(Effect.annotateCurrentSpan('server.address', host.name), Cache.get(cache, host));
const carry = Effect.fnUntraced(
    function* <E, R>(inbound: Socket.Socket, dialed: Effect.Effect<Socket.Socket, E, R>) {
        const up = yield* Socket.readerBytes(inbound);
        const outbound = yield* dialed;
        const down = yield* Socket.readerBytes(outbound);
        yield* Effect.forEach(
            [
                [up, outbound],
                [down, inbound],
            ] as const,
            ([pull, to]) =>
                to.writer.pipe(
                    Effect.flatMap((writer) => Effect.forever(Effect.flatMap(pull, writer.writeAll))),
                    Effect.scoped,
                    Effect.catchReason('SocketError', 'SocketCloseError', () => Effect.void),
                ),
            { concurrency: 'unbounded', discard: true },
        );
    },
    Effect.scoped,
    Effect.ignore({ log: 'Warn' }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Ssh };
