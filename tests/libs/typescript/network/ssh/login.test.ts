import { assert, it } from '@effect/vitest';
import { Ssh, SshError } from '@rasm/ssh';
import { Effect, FileSystem, Path, pipe, Record, Schema } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { bounds, printed, reached, reasoned, Sshd, served, within } from '#/sshd';

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Sshd.layer(), { timeout: bounds.started, excludeTestServices: true, concurrent: true })('login', (methods) => {
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
        'refuses a host whose key differs from its trusted key and prints the failed verification',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted }] = yield* Effect.all([Ssh, Sshd]);
            const failed = yield* Effect.flip(reached(ssh, yield* hosted('mismatched', { trust: { _tag: 'HostKeys', keys: ['ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIOMqqnkVzrm0SdG6UOoqKLsabgH5C9okWi0dh2l9GKJl'] } })));
            assert.include(Schema.is(SshError)(failed) ? failed.message : String(failed), 'Host key verification failed.', 'openssh/openssh-portable regress/test-exec.sh StrictHostKeyChecking yes over one known_hosts line per alias; ssh(1) "VERIFYING HOST KEYS"');
        }),
        bounds.test,
    );
});
