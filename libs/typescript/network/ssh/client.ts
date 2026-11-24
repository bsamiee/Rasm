import { ExitError, run, spawn } from '@rasm/process';
import { Array, Boolean, Cache, Context, Duration, Effect, Exit, FileSystem, Layer, Option, Path, Record, Schema, Stream } from 'effect';
import { type ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Forward } from '#/forward';
import { type Host, Line } from '#/host';

// --- [CONSTANTS] -----------------------------------------------------------------------

const errored = 255;

// --- [ERRORS] --------------------------------------------------------------------------

class SshError extends Schema.TaggedError<SshError, { readonly brand: unique symbol }>()('SshError', {
    reason: Schema.TaggedUnion({ Exit: { exit: ExitError }, Unreachable: { stderr: Schema.String }, Refused: { stderr: Schema.String } }),
    subject: Schema.String,
    message: Schema.String,
}) {
    static readonly of = (reason: SshError['reason'], subject: string) =>
        SshError.make({
            reason,
            subject,
            message: `${subject} ${SshError.fields.reason.match(reason, {
                Exit: ({ exit }) => `remote command exited ${exit.code}, read its output: ${exit.stderr}`,
                Unreachable: ({ stderr }) => `OpenSSH exited ${errored} with no shared connection (no connection, a lost connection, or a command exiting ${errored} on its own connection), check the host, its dials, and its identity: ${stderr}`,
                Refused: ({ stderr }) => `OpenSSH refused the forward, free its bind end: ${stderr}`,
            })}`,
        });
}

// --- [SERVICES] ------------------------------------------------------------------------

class Ssh extends Context.Service<Ssh>()('@rasm/ssh/client/Ssh', {
    make: Effect.fnUntraced(function* (folder: string) {
        const [spawner, fs, path, root] = yield* Effect.all([ChildProcessSpawner.ChildProcessSpawner, FileSystem.FileSystem, Path.Path, Schema.decodeEffect(Line)(folder)]);
        const started = (words: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) => Effect.provideService(spawn(words, options), ChildProcessSpawner.ChildProcessSpawner, spawner);
        const captured = (words: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) => Effect.provideService(run(words, options), ChildProcessSpawner.ChildProcessSpawner, spawner);
        const checked = (host: Host) =>
            captured(host.request(host.control(path, root), 'check')).pipe(
                Effect.as(true),
                Effect.catchTag('ExitError', () => Effect.succeed(false)),
            );
        const settled = (host: Host, lived = checked(host)) =>
            Effect.fnUntraced(function* (exit: ExitError) {
                const reached = exit.code !== errored || (yield* lived);
                return yield* SshError.of(reached ? { _tag: 'Exit', exit } : { _tag: 'Unreachable', stderr: exit.stderr }, host.name);
            });
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
                yield* Effect.fromResult(host.command(path, root, ['true'])).pipe(Effect.flatMap(captured), Effect.catchTag('ExitError', settled(host)), Effect.when(Effect.map(checked(host), Boolean.not)));
            }),
            { capacity: Number.POSITIVE_INFINITY, timeToLive: () => Duration.zero },
        );
        const called = Effect.fnUntraced(function* (host: Host, remote: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) {
            const sent = Record.keys(options?.env ?? {});
            const [opened, lived] = Array.match(sent, { onEmpty: () => [Cache.get(shared, host), checked(host)] as const, onNonEmpty: () => [Cache.get(placed, host), Effect.succeed(false)] as const });
            yield* opened;
            return { words: yield* Effect.fromResult(host.command(path, root, remote, sent)), lived };
        });
        return {
            written,
            spawn: Effect.fn('Ssh.spawn')(function* (host: Host, remote: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) {
                return yield* started((yield* called(host, remote, options)).words, options);
            }),
            run: Effect.fn('Ssh.run')(function* (host: Host, remote: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) {
                const { words, lived } = yield* called(host, remote, options);
                return yield* captured(words, options).pipe(
                    Effect.map(({ stdout }) => stdout),
                    Effect.catchTag('ExitError', settled(host, lived)),
                );
            }),
            copy: Effect.fn('Ssh.copy')(function* (host: Host, source: string, target: string) {
                yield* Cache.get(shared, host);
                return yield* captured(host.copy(path, root, source, target)).pipe(Effect.catchTag('ExitError', settled(host)), Effect.asVoid);
            }),
            forward: Effect.fn('Ssh.forward')(function* (host: Host, wanted: Parameters<typeof Forward.argv>[0]) {
                yield* Cache.get(placed, host);
                const socket = path.join(yield* fs.makeTempDirectoryScoped(), 'control');
                const holder = yield* started(yield* Effect.fromResult(host.command(path, root, ['sh', '-c', 'echo && exec cat'], [], Option.some(socket))), { stdin: 'pipe' });
                const refused = Effect.flatMap(Stream.mkString(Stream.decodeText(holder.stderr)), (printed) => SshError.of({ _tag: 'Unreachable', stderr: printed }, host.name));
                yield* Effect.filterOrElse(Stream.runHead(holder.stdout), Option.isSome, () => refused);
                yield* started(['sh', '-c', 'cat > /dev/null; exec "$0" "$@"', ...host.request(socket, 'exit')], { stdin: 'pipe', stdout: 'ignore', stderr: 'ignore' });
                const { stdout } = yield* captured(host.request(socket, 'forward', ...Forward.argv(wanted))).pipe(Effect.catchTag('ExitError', ({ stderr }) => SshError.of({ _tag: 'Refused', stderr }, host.name)));
                return yield* Forward.bound(stdout)(wanted);
            }),
        };
    }),
}) {
    static readonly layer = (folder: string) => Layer.effect(this, this.make(folder));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Ssh, SshError };
