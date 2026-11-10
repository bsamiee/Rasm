import { Array, Cache, Context, Duration, Effect, Exit, FileSystem, Layer, Option, Path, pipe, Record, Schema, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Forward } from '#/forward';
import { type Host, Line } from '#/host';

// --- [CONSTANTS] -----------------------------------------------------------------------

const errored = 255;

// --- [ERRORS] --------------------------------------------------------------------------

class SshError extends Schema.TaggedError<SshError, { readonly brand: unique symbol }>()('SshError', {
    reason: Schema.TaggedUnion({ Exit: { code: Schema.Int, stdout: Schema.String, stderr: Schema.String }, Unreachable: { stderr: Schema.String }, Refused: { stderr: Schema.String } }),
    subject: Schema.String,
}) {
    override get message() {
        const outcome = SshError.fields.reason.match(this.reason, {
            Exit: ({ code, stderr }) => `remote command exited ${code}, read its output: ${stderr}`,
            Unreachable: ({ stderr }) => `OpenSSH exited ${errored} (no connection, a lost connection, or a remote command exiting ${errored}), check the host, its dials, and its identity: ${stderr}`,
            Refused: ({ stderr }) => `OpenSSH refused the forward, free its bind end: ${stderr}`,
        });
        return `${this.subject} ${outcome}`;
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Ssh extends Context.Service<Ssh>()('@rasm/ssh/client/Ssh', {
    make: Effect.fnUntraced(function* (folder: string) {
        const [spawner, fs, path, root] = yield* Effect.all([ChildProcessSpawner.ChildProcessSpawner, FileSystem.FileSystem, Path.Path, Schema.decodeEffect(Line)(folder)]);
        const started = (words: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) => spawner.spawn(ChildProcess.make(Array.headNonEmpty(words), Array.tailNonEmpty(words), { extendEnv: true, ...options }));
        const exchanged = Effect.fnUntraced(function* (words: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) {
            const handle = yield* started(words, { stdin: 'ignore', ...options });
            const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
            return { stdout, stderr, code };
        }, Effect.scoped);
        const settled =
            (host: Host) =>
            ({ stdout, stderr, code }: Effect.Success<ReturnType<typeof exchanged>>) =>
                code === 0 ? Effect.succeed(stdout) : Effect.fail(SshError.make({ reason: code === errored ? { _tag: 'Unreachable', stderr } : { _tag: 'Exit', code, stdout, stderr }, subject: host.name }));
        const written = Effect.fnUntraced(function* (host: Host, beside: Readonly<Record<string, string>> = {}) {
            const files = Record.toEntries({ ...(yield* Effect.fromResult(host.files(path, root))), ...beside });
            yield* Effect.forEach(Array.dedupe(files.map(([file]) => path.dirname(file))), (home) => fs.makeDirectory(home, { recursive: true, mode: 0o700 }), { discard: true });
            const staging = yield* fs.makeTempDirectoryScoped({ directory: path.join(root, host.name) });
            const moved = ([file, text]: readonly [string, string]) => Effect.andThen(fs.writeFileString(path.join(staging, path.basename(file)), text, { mode: 0o600 }), fs.rename(path.join(staging, path.basename(file)), file));
            return yield* Effect.forEach(files, moved, { concurrency: 'unbounded', discard: true });
        }, Effect.scoped);
        const placed = yield* Cache.makeWith(written, { capacity: Number.POSITIVE_INFINITY, timeToLive: (exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero) });
        const shared = yield* Cache.makeWith(
            Effect.fnUntraced(function* (host: Host) {
                yield* Cache.get(placed, host);
                const { code } = yield* exchanged(host.request(host.control(path, root), 'check'));
                return yield* code === 0 ? Effect.void : pipe(Effect.fromResult(host.command(path, root, ['true'])), Effect.flatMap(exchanged), Effect.flatMap(settled(host)), Effect.asVoid);
            }),
            { capacity: Number.POSITIVE_INFINITY, timeToLive: () => Duration.zero },
        );
        const called = Effect.fnUntraced(function* (host: Host, remote: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) {
            const sent = Record.keys(options?.env ?? {});
            yield* Array.isReadonlyArrayNonEmpty(sent) ? Cache.get(placed, host) : Cache.get(shared, host);
            return yield* Effect.fromResult(host.command(path, root, remote, sent));
        });
        return {
            written,
            spawn: Effect.fn('Ssh.spawn')(function* (host: Host, remote: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) {
                return yield* started(yield* called(host, remote, options), options);
            }),
            run: Effect.fn('Ssh.run')(function* (host: Host, remote: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) {
                return yield* Effect.flatMap(exchanged(yield* called(host, remote, options), options), settled(host));
            }),
            copy: Effect.fn('Ssh.copy')(function* (host: Host, source: string, target: string) {
                yield* Cache.get(shared, host);
                return yield* Effect.asVoid(Effect.flatMap(exchanged(host.copy(path, root, source, target)), settled(host)));
            }),
            forward: Effect.fn('Ssh.forward')(function* (host: Host, wanted: typeof Forward.Type) {
                yield* Cache.get(placed, host);
                const socket = path.join(yield* fs.makeTempDirectoryScoped(), 'control');
                const holder = yield* started(yield* Effect.fromResult(host.command(path, root, ['sh', '-c', 'echo && exec cat'], [], Option.some(socket))), { stdin: 'pipe' });
                const refused = Effect.flatMap(Stream.mkString(Stream.decodeText(holder.stderr)), (printed) => SshError.make({ reason: { _tag: 'Unreachable', stderr: printed }, subject: host.name }));
                yield* Effect.flatMap(Stream.runHead(holder.stdout), Option.match({ onSome: () => Effect.void, onNone: () => refused }));
                const { stdout, stderr, code } = yield* exchanged(host.request(socket, 'forward', ...Forward.argv(wanted)));
                return code === 0 ? yield* Forward.bound(wanted, stdout) : yield* SshError.make({ reason: { _tag: 'Refused', stderr }, subject: host.name });
            }),
        };
    }),
}) {
    static readonly layer = (folder: string) => Layer.effect(this, this.make(folder));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Ssh, SshError };
