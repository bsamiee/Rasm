import process from 'node:process';
import { assert, it } from '@effect/vitest';
import { Forward } from '@rasm/ssh';
import { Effect, Exit, FileSystem, Option, Path, Schema, Scope } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { RandomPortGenerator } from 'testcontainers';
import { Asked } from '#/peer';
import { bounds, heard, held, printed, reasoned, Sshd, served } from '#/sshd';

// --- [CONSTANTS] -----------------------------------------------------------------------

const socks = { asked: Uint8Array.fromHex('050100'), accepted: '\u0005\u0000' };
const sshd = `127.0.0.1:${served}`;

// --- [OPERATIONS] ----------------------------------------------------------------------

const wanted = Schema.decodeEffect(Forward);
const vacant = Effect.promise(() => new RandomPortGenerator().generatePort());
const ending = Effect.acquireRelease(Scope.make(), (scope) => Scope.close(scope, Exit.void));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Sshd.layer, { timeout: bounds.started, excludeTestServices: true, concurrent: true })('forwarding', (methods) => {
    const banner = 'SSH-2.0-';

    methods.effect.each(['port', 'path'] as const)(
        'forwards a local %s to an address the server dials, and stops listening on it with its scope',
        Effect.fnUntraced(function* (kind) {
            const [running, fs, path] = yield* Effect.all([Sshd, FileSystem.FileSystem, Path.Path]);
            const [{ ssh }, host, folder, free] = yield* Effect.all([running.client, running.hosted('local'), fs.makeTempDirectoryScoped(), vacant]);
            const { bind, at } = { port: { bind: `127.0.0.1:${free}`, at: { host: '127.0.0.1', port: free } }, path: { bind: path.join(folder, 'bound'), at: { path: path.join(folder, 'bound') } } }[kind];
            const local = yield* wanted({ _tag: 'Local', bind, connect: sshd });
            const open = yield* Effect.scoped(Effect.andThen(ssh.forward(host, local), heard(at)));
            assert.deepStrictEqual([open.slice(0, banner.length), yield* heard(at)], [banner, '']);
        }),
        bounds.test,
    );

    methods.effect(
        'serves SOCKS on a dynamic forward, and stops serving with its scope',
        Effect.fnUntraced(function* () {
            const running = yield* Sshd;
            const [{ ssh }, host, free] = yield* Effect.all([running.client, running.hosted('dynamic'), vacant]);
            const dynamic = yield* wanted({ _tag: 'Dynamic', bind: `127.0.0.1:${free}` });
            const open = yield* Effect.scoped(Effect.andThen(ssh.forward(host, dynamic), heard({ host: '127.0.0.1', port: free }, socks.asked)));
            assert.deepStrictEqual([open, yield* heard({ host: '127.0.0.1', port: free }, socks.asked)], [socks.accepted, '']);
        }),
        bounds.test,
    );

    methods.effect.each(['port', 'path'] as const)(
        'forwards a remote %s, on the port the server allocates for port 0, to an address this machine dials, and ends it with its scope',
        Effect.fnUntraced(function* (kind) {
            const [running, fs, path] = yield* Effect.all([Sshd, FileSystem.FileSystem, Path.Path]);
            const [{ ssh }, host, folder, made, ended] = yield* Effect.all([running.client, running.hosted('remote'), fs.makeTempDirectoryScoped(), running.scratch, ending]);
            const remote = yield* wanted({ _tag: 'Remote', bind: { port: '127.0.0.1:0', path: `${made}/bound` }[kind], connect: `${running.served.dial.host}:${running.served.dial.port}` });
            const { bind } = yield* Effect.flatMap(Scope.provide(ssh.forward(host, remote), ended), Schema.encodeEffect(Forward));
            const through = { path: path.join(folder, 'through') };
            yield* ssh.forward(host, yield* wanted({ _tag: 'Local', bind: through.path, connect: bind }));
            const open = yield* held(through);
            yield* Scope.close(ended, Exit.void);
            yield* open.closed;
            assert.deepStrictEqual([open.first.slice(0, banner.length), yield* heard(through), bind === '127.0.0.1:0'], [banner, '', false]);
        }),
        bounds.test,
    );

    methods.effect.each([
        { _tag: 'Local', bind: Effect.map(vacant, (free) => `127.0.0.1:${free}`) },
        { _tag: 'Remote', bind: Effect.succeed('127.0.0.1:0') },
    ] as const)(
        'refuses a $_tag forward on a port a live forward binds',
        Effect.fnUntraced(function* ({ _tag, bind }) {
            const running = yield* Sshd;
            const [{ ssh }, host, free] = yield* Effect.all([running.client, running.hosted('bound'), bind]);
            const bound = yield* ssh.forward(host, yield* wanted({ _tag, bind: free, connect: sshd }));
            assert.strictEqual(yield* reasoned(ssh.forward(host, bound)), 'Refused');
        }),
        bounds.test,
    );

    methods.effect.each([
        { options: ['AllowTcpForwarding=yes'], keys: [], bind: 3301, local: banner, remote: banner },
        { options: ['AllowTcpForwarding=local'], keys: [], bind: 3302, local: banner, remote: 'Refused' },
        { options: ['AllowTcpForwarding=remote'], keys: [], bind: 3303, local: '', remote: banner },
        { options: ['AllowTcpForwarding=no'], keys: [], bind: 3304, local: '', remote: 'Refused' },
        { options: ['PermitOpen=127.0.0.1:1'], keys: [], bind: 3306, local: '', remote: banner },
        { options: ['PermitListen=127.0.0.1:1'], keys: [], bind: 3308, local: banner, remote: 'Refused' },
        { options: [], keys: ['permitopen="127.0.0.1:1"'], bind: 3310, local: '', remote: banner },
        { options: [], keys: ['permitlisten="127.0.0.1:1"'], bind: 3312, local: banner, remote: 'Refused' },
        { options: [], keys: ['no-port-forwarding'], bind: 3313, local: '', remote: 'Refused' },
        { options: [], keys: ['restrict'], bind: 3314, local: '', remote: 'Refused' },
        { options: [], keys: ['restrict', 'port-forwarding'], bind: 3315, local: banner, remote: banner },
    ])(
        'on a server stating %j, hears the local and finds the remote forward it states',
        Effect.fnUntraced(function* ({ options, keys, bind, local, remote }) {
            const [running, fs, path] = yield* Effect.all([Sshd, FileSystem.FileSystem, Path.Path]);
            const [{ ssh }, { inside, dial }, folder] = yield* Effect.all([running.client, running.port, fs.makeTempDirectoryScoped()]);
            yield* running.listening(inside, options, keys);
            const [host, main, at, through] = [yield* running.hosted('ruled', dial), yield* running.hosted('main'), { path: path.join(folder, 'bound') }, { path: path.join(folder, 'through') }];
            const [opened, bound, chained] = yield* Effect.all([
                wanted({ _tag: 'Local', bind: at.path, connect: sshd }),
                wanted({ _tag: 'Remote', bind: `127.0.0.1:${bind}`, connect: `${running.served.dial.host}:${running.served.dial.port}` }),
                wanted({ _tag: 'Local', bind: through.path, connect: `127.0.0.1:${bind}` }),
            ]);
            const found = yield* Effect.all(
                {
                    local: Effect.scoped(Effect.andThen(ssh.forward(host, opened), heard(at))),
                    remote: Effect.all([ssh.forward(host, bound), ssh.forward(main, chained)]).pipe(
                        Effect.andThen(heard(through)),
                        Effect.catchReason('SshError', 'Refused', () => Effect.succeed('Refused')),
                        Effect.scoped,
                    ),
                },
                { concurrency: 'unbounded' },
            );
            assert.deepStrictEqual({ local: found.local.slice(0, banner.length), remote: found.remote.slice(0, banner.length) }, { local, remote });
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
            yield* open.closed;
            assert.deepStrictEqual([open.first.slice(0, banner.length), yield* heard(at)], [banner, '']);
        }),
        bounds.test,
    );
});
