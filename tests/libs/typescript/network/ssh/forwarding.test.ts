import process from 'node:process';
import { assert, it } from '@effect/vitest';
import { Forward } from '@rasm/ssh';
import { Cause, Effect, Exit, FileSystem, Option, Path, Schema, Scope } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { RandomPortGenerator } from 'testcontainers';
import { Asked } from '#/peer';
import { bounds, heard, held, printed, reasoned, Sshd, served } from '#/sshd';

// --- [CONSTANTS] -----------------------------------------------------------------------

const banner = 'SSH-2.0-';
const socks = { asked: Uint8Array.fromHex('050100'), accepted: '\u0005\u0000' };
const sshd = `127.0.0.1:${served}`;

// --- [OPERATIONS] ----------------------------------------------------------------------

const wanted = Schema.decodeEffect(Forward);
const vacant = Effect.promise(() => new RandomPortGenerator().generatePort());
const greeted = Option.map((text: string) => text.slice(0, banner.length));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Sshd.layer, { timeout: bounds.started, excludeTestServices: true, concurrent: true })('forwarding', (methods) => {
    methods.effect(
        'forwards a local port to an address the server dials, and stops listening on it with its scope',
        Effect.fnUntraced(function* () {
            const running = yield* Sshd;
            const [{ ssh }, host, free] = yield* Effect.all([running.client, running.hosted('local'), vacant]);
            const at = { host: '127.0.0.1', port: free };
            const open = yield* Effect.scoped(Effect.andThen(ssh.forward(host, yield* wanted({ _tag: 'Local', bind: `127.0.0.1:${free}`, connect: sshd })), heard(at)));
            assert.deepStrictEqual([greeted(open), yield* heard(at)], [Option.some(banner), Option.none()]);
        }),
        bounds.test,
    );

    methods.effect(
        'serves SOCKS on a dynamic forward, and stops serving with its scope',
        Effect.fnUntraced(function* () {
            const running = yield* Sshd;
            const [{ ssh }, host, free] = yield* Effect.all([running.client, running.hosted('dynamic'), vacant]);
            const at = { host: '127.0.0.1', port: free };
            const open = yield* Effect.scoped(Effect.andThen(ssh.forward(host, yield* wanted({ _tag: 'Dynamic', bind: `127.0.0.1:${free}` })), heard(at, socks.asked)));
            assert.deepStrictEqual([open, yield* heard(at, socks.asked)], [Option.some(socks.accepted), Option.none()]);
        }),
        bounds.test,
    );

    methods.effect(
        'forwards a remote port the server allocates for port 0 to an address this machine dials, and ends it with its scope',
        Effect.fnUntraced(function* () {
            const [running, fs, path] = yield* Effect.all([Sshd, FileSystem.FileSystem, Path.Path]);
            const [{ ssh }, host, folder, ended] = yield* Effect.all([running.client, running.hosted('remote'), fs.makeTempDirectoryScoped(), Effect.acquireRelease(Scope.make(), (scope) => Scope.close(scope, Exit.void))]);
            const remote = yield* wanted({ _tag: 'Remote', bind: '127.0.0.1:0', connect: `${running.served.dial.host}:${running.served.dial.port}` });
            const { bind } = yield* Effect.flatMap(Scope.provide(ssh.forward(host, remote), ended), Schema.encodeEffect(Forward));
            const through = { path: path.join(folder, 'through') };
            yield* ssh.forward(host, yield* wanted({ _tag: 'Local', bind: through.path, connect: bind }));
            const open = yield* held(through);
            yield* Scope.close(ended, Exit.void);
            yield* open.closed;
            assert.deepStrictEqual([greeted(open.first), Option.contains(greeted(yield* heard(through)), banner)], [Option.some(banner), false]);
        }),
        bounds.test,
    );

    methods.effect(
        'refuses a local forward on a port a live forward binds',
        Effect.fnUntraced(function* () {
            const running = yield* Sshd;
            const [{ ssh }, host, free] = yield* Effect.all([running.client, running.hosted('bound'), vacant]);
            const bound = yield* ssh.forward(host, yield* wanted({ _tag: 'Local', bind: `127.0.0.1:${free}`, connect: sshd }));
            assert.strictEqual(yield* reasoned(ssh.forward(host, bound)), 'Refused');
        }),
        bounds.test,
    );

    methods.effect(
        'ends the forward of a killed process, and the connections it carried, with that process',
        Effect.fnUntraced(function* () {
            const [running, fs, path, spawner] = yield* Effect.all([Sshd, FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
            const [{ folder }, host, made] = yield* Effect.all([running.client, running.hosted('owned'), fs.makeTempDirectoryScoped()]);
            const at = { path: path.join(made, 'bound') };
            const request = yield* Schema.encodeEffect(Asked)({ folder, host, forward: Option.some(yield* wanted({ _tag: 'Local', bind: at.path, connect: sshd })), calls: 1, remote: ['true'] });
            const peer = yield* spawner.spawn(ChildProcess.make(process.execPath, [path.join(import.meta.dirname, 'peer.ts'), request], { stdin: 'ignore', stderr: 'inherit' }));
            yield* printed(peer, 'the forwarding process');
            const open = yield* held(at);
            yield* peer.kill({ killSignal: 'SIGKILL' });
            yield* Effect.timeoutOrElse(open.closed, { duration: '10 seconds', orElse: () => Effect.fail(new Cause.UnknownError(undefined, 'a connection the killed process forwarded stays open')) });
            assert.deepStrictEqual([greeted(open.first), yield* heard(at)], [Option.some(banner), Option.none()]);
        }),
        bounds.test,
    );
});
