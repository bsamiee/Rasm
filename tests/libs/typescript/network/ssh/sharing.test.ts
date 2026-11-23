import process from 'node:process';
import { assert, it } from '@effect/vitest';
import { Forward, Ssh } from '@rasm/ssh';
import { Array, Effect, Exit, Fiber, FileSystem, Path, Schedule, Schema } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Asked, Reached } from '#/peer';
import { answered, bounds, callers, connection, greeted, greeting, Master, printed, reached, Sshd, within } from '#/sshd';

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Sshd.layer(), { timeout: bounds.started, excludeTestServices: true, concurrent: true })('sharing', (methods) => {
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
        'lets a running command finish after a stop request, refuses the stopped control socket, and opens a connection for the next caller',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, asked, far }] = yield* Effect.all([Ssh, Sshd]);
            const host = yield* hosted('stopped');
            const running = yield* Effect.forkScoped(ssh.run(host, ['sh', '-c', 'touch /tmp/stop-started; sleep 3; echo finished']));
            yield* Effect.retry(ssh.run(host, ['test', '-f', '/tmp/stop-started']), Schedule.spaced('100 millis'));
            yield* asked(host, 'stop');
            const [checked, finished] = yield* Effect.all([asked(host, 'check'), Fiber.join(running)]);
            assert.deepStrictEqual([Schema.is(Master)(checked.trim()), finished, yield* reached(ssh, host)], [false, 'finished\n', far.name]);
        }, Effect.scoped),
        bounds.test,
    );

    methods.effect(
        'ends a running command with an exit request, and opens a connection for the next caller',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, asked, far }] = yield* Effect.all([Ssh, Sshd]);
            const host = yield* hosted('exited');
            const running = yield* Effect.forkScoped(Effect.exit(ssh.run(host, ['sh', '-c', 'touch /tmp/exit-started; sleep 600'])));
            yield* Effect.retry(ssh.run(host, ['test', '-f', '/tmp/exit-started']), Schedule.spaced('100 millis'));
            yield* asked(host, 'exit');
            const [ended, checked] = yield* Effect.all([Fiber.join(running), asked(host, 'check')]);
            assert.deepStrictEqual([Exit.isFailure(ended), Schema.is(Master)(checked.trim()), yield* reached(ssh, host)], [true, false, far.name]);
        }, Effect.scoped),
        bounds.test,
    );
});
