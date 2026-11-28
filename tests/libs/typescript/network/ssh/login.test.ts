import { make as socketServer } from '@effect/platform-node/NodeSocketServer';
import { assert, it } from '@effect/vitest';
import { SshError } from '@rasm/ssh/host';
import { Array, Effect, Exit, FileSystem, Path, Schema, Scope, Struct } from 'effect';
import { NetAddress } from 'effect/net';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { principal, printed, reasoned, Sshd, served } from '#/sshd';

// --- [CONSTANTS] -----------------------------------------------------------------------

const listener = Array.make('sh', '-c', 'set -- $SSH_CONNECTION; echo "$4"');

// --- [OPERATIONS] ----------------------------------------------------------------------

const silent = socketServer({ host: '127.0.0.1', port: 0 }).pipe(
    Effect.map(Struct.get('address')),
    Effect.filterOrFail(NetAddress.isInetAddress),
    Effect.map(({ port }) => ({ host: '127.0.0.1', port })),
);
const ending = Effect.flatMap(Effect.scope, Scope.fork);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Sshd.layer, { excludeTestServices: true, concurrent: true })('login', (methods) => {
    methods.effect.each([
        { preferred: ['answering'], env: {}, taken: 'preferred' },
        { preferred: ['answering'], env: { SENT: 'sent' }, taken: 'preferred' },
        { preferred: ['refusing'], env: {}, taken: 'stated' },
        { preferred: ['refusing'], env: { SENT: 'sent' }, taken: 'stated' },
        { preferred: ['silent'], env: {}, taken: 'stated' },
        { preferred: ['refusing', 'silent', 'answering'], env: {}, taken: 'preferred' },
    ] as const)(
        'connects a host preferring $preferred dials, on a call sending $env, through its $taken dial',
        Effect.fnUntraced(function* ({ preferred, env, taken }) {
            const sshd = yield* Sshd;
            const [{ ssh }, answering, refusing, quiet] = yield* Effect.all([sshd.client, sshd.port, sshd.port, silent]);
            yield* sshd.listening(answering.inside);
            const dials = { answering: answering.dial, refusing: refusing.dial, silent: quiet };
            const host = yield* sshd.hosted('dialed', { route: { _tag: 'Direct', preferred: Array.map(preferred, (kind) => ({ ...dials[kind], within: 1000 })) } });
            assert.strictEqual(yield* ssh.run(host, listener, { env }), `${{ preferred: answering.inside, stated: served }[taken]}\n`);
        }),
    );

    methods.effect(
        'tries a preferred dial stating no port on the port of its host',
        Effect.fnUntraced(function* () {
            const sshd = yield* Sshd;
            const [{ ssh }, { inside, dial }] = yield* Effect.all([sshd.client, sshd.port]);
            yield* sshd.listening(inside);
            const host = yield* sshd.hosted('portless', {
                ...dial,
                route: {
                    _tag: 'Direct',
                    preferred: [
                        { host: dial.host, within: 1000 },
                        { ...sshd.served.dial, within: 1000 },
                    ],
                },
            });
            assert.strictEqual(yield* ssh.run(host, listener), `${inside}\n`);
        }),
    );

    methods.effect(
        'keeps a shared connection on its dial while it lives, and chooses the dial again for each connection that starts',
        Effect.fnUntraced(function* () {
            const sshd = yield* Sshd;
            const [{ ssh, terminated }, { inside, dial }, answering] = yield* Effect.all([sshd.client, sshd.port, ending]);
            const host = yield* sshd.hosted('chosen', { route: { _tag: 'Direct', preferred: [{ ...dial, within: 1000 }] } });
            yield* Scope.provide(sshd.listening(inside), answering);
            const first = yield* ssh.run(host, listener);
            yield* Scope.close(answering, Exit.void);
            const kept = yield* ssh.run(host, listener);
            const stated = yield* Effect.andThen(terminated(host.name), ssh.run(host, listener));
            yield* sshd.listening(inside);
            const held = yield* ssh.run(host, listener);
            const again = yield* Effect.andThen(terminated(host.name), ssh.run(host, listener));
            assert.deepStrictEqual([first, kept, stated, held, again], [`${inside}\n`, `${inside}\n`, `${served}\n`, `${served}\n`, `${inside}\n`]);
        }),
    );

    methods.effect(
        'reaches a host no dial of this machine reaches through its hop, on shared and own connections',
        Effect.fnUntraced(function* () {
            const sshd = yield* Sshd;
            const [{ ssh }, { inside }] = yield* Effect.all([sshd.client, sshd.port]);
            yield* sshd.listening(inside);
            const hop = { name: 'hop', user: 'root', ...sshd.served.dial, identity: { _tag: 'Key', file: sshd.files.client }, trust: { _tag: 'HostKeys', keys: [sshd.trusted] } };
            const host = yield* sshd.hosted('jumped', { host: sshd.name, port: inside, route: { _tag: 'Jump', hops: [hop] } });
            const shared = yield* ssh.run(host, listener);
            const own = yield* ssh.spawn(host, listener, { env: { SENT: 'sent' }, stdin: 'ignore' });
            assert.deepStrictEqual([shared, yield* printed(own, 'the own connection')], [`${inside}\n`, `${inside}`]);
        }),
    );

    methods.effect(
        'logs in with a key an agent holds',
        Effect.fnUntraced(function* () {
            const [sshd, fs, path, spawner] = yield* Effect.all([Sshd, FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
            const [{ ssh }, folder] = yield* Effect.all([sshd.client, fs.makeTempDirectoryScoped()]);
            const socket = path.join(folder, 'agent');
            yield* printed(yield* ChildProcess.make('ssh-agent', ['-D', '-a', socket], { stdin: 'ignore' }), 'ssh-agent');
            const added = yield* spawner.exitCode(ChildProcess.make('ssh-add', ['-q', sshd.files.client], { env: { SSH_AUTH_SOCK: socket }, extendEnv: true, stdin: 'ignore' }));
            const host = yield* sshd.hosted('agent', { identity: { _tag: 'Agent', socket } });
            assert.deepStrictEqual([added, yield* ssh.run(host, listener)], [0, `${served}\n`]);
        }),
    );

    methods.effect(
        'logs in with a certificate to a host its authority signed, and with that key alone reaches no login',
        Effect.fnUntraced(function* () {
            const sshd = yield* Sshd;
            const [{ ssh }, certified, bare] = yield* Effect.all([
                sshd.client,
                sshd.hosted(principal, { identity: { _tag: 'Key', file: sshd.files.certified, certificate: sshd.files.certificate }, trust: { _tag: 'Authority', keys: [sshd.signer] } }),
                sshd.hosted('bare', { identity: { _tag: 'Key', file: sshd.files.certified } }),
            ]);
            assert.deepStrictEqual([yield* ssh.run(certified, listener), yield* reasoned(ssh.run(bare, listener))], [`${served}\n`, 'Unreachable']);
        }),
    );

    methods.effect.each(['HostKeys', 'Authority'] as const)(
        'refuses a host whose key its %s trust does not hold, and reports the failed verification',
        Effect.fnUntraced(function* (_tag) {
            const sshd = yield* Sshd;
            const [{ ssh }, host] = yield* Effect.all([sshd.client, sshd.hosted(principal, { trust: { _tag, keys: ['ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIOMqqnkVzrm0SdG6UOoqKLsabgH5C9okWi0dh2l9GKJl'] } })]);
            const failed = yield* Effect.flip(ssh.run(host, listener));
            assert.deepStrictEqual([yield* failed.pipe(Effect.fail, reasoned), Schema.is(SshError)(failed) && failed.message.includes('Host key verification failed.')], ['Unreachable', true]);
        }),
    );
});
