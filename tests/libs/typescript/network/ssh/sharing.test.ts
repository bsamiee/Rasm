import process from 'node:process';
import { assert, it } from '@effect/vitest';
import { Ssh } from '@rasm/ssh/client';
import { Array, Effect, Fiber, Option, Path, Schema } from 'effect';
import { ChildProcess } from 'effect/process';
import { Asked, Reached } from '#/peer';
import { printed, reasoned, Sshd } from '#/sshd';

// --- [CONSTANTS] -----------------------------------------------------------------------

const connection = Array.make('sh', '-c', 'echo "$SSH_CONNECTION"');
const callers = Array.range(1, 8);

// --- [MODELS] --------------------------------------------------------------------------

const Master = Schema.TemplateLiteralParser(['Master running (pid=', Schema.FiniteFromString, ')']);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Sshd.layer, { excludeTestServices: true, concurrent: true })('sharing', (methods) => {
    methods.effect(
        'runs the concurrent first calls of two clients on one folder, and every call after them, over one connection',
        Effect.fnUntraced(function* () {
            const sshd = yield* Sshd;
            const [{ ssh, folder }, host] = yield* Effect.all([sshd.client, sshd.hosted('shared')]);
            const clients = [ssh, yield* Ssh.make(folder)] as const;
            const first = yield* Effect.forEach(callers, (index) => clients[index % 2 === 0 ? 0 : 1].run(host, connection), { concurrency: 'unbounded' });
            const later = yield* Effect.forEach(callers, (index) => clients[index % 2 === 0 ? 1 : 0].run(host, connection), { concurrency: 'unbounded' });
            assert.deepStrictEqual(Array.dedupe([...first, ...later]), [Array.headNonEmpty(first)]);
        }),
    );

    methods.effect(
        'runs the calls of a second process on the folder over the connection of the first',
        Effect.fnUntraced(function* () {
            const [sshd, path] = yield* Effect.all([Sshd, Path.Path]);
            const [{ ssh, folder }, host] = yield* Effect.all([sshd.client, sshd.hosted('process')]);
            const mine = yield* ssh.run(host, connection);
            const request = yield* Schema.encodeEffect(Asked)({ folder, host, forward: Option.none(), calls: 4, remote: connection });
            const peer = yield* ChildProcess.make(process.execPath, [path.join(import.meta.dirname, 'peer.ts'), request], { stdin: 'ignore', stderr: 'inherit' });
            assert.deepStrictEqual(yield* Effect.flatMap(printed(peer, 'the second process'), Schema.decodeEffect(Reached)), [mine, mine, mine, mine]);
        }),
    );

    methods.effect(
        'fails the command a killed shared connection carried, and runs the concurrent calls after it over one new connection',
        Effect.fnUntraced(function* () {
            const sshd = yield* Sshd;
            const [{ ssh, asked }, host] = yield* Effect.all([sshd.client, sshd.hosted('killed')]);
            const running = yield* ssh.spawn(host, ['sh', '-c', 'echo "$SSH_CONNECTION"; exec cat']);
            const before = yield* printed(running, 'the running command');
            const [, pid] = yield* Effect.flatMap(asked(host, 'check'), (shown) => Schema.decodeUnknownEffect(Master)(shown.trim()));
            yield* Effect.sync(() => process.kill(pid, 'SIGKILL'));
            const code = yield* running.exitCode;
            const after = yield* Effect.forEach(callers, () => ssh.run(host, connection), { concurrency: 'unbounded' });
            assert.deepStrictEqual([code, Array.dedupe(after).length, after.includes(`${before}\n`)], [255, 1, false]);
        }),
    );

    methods.effect(
        'lets a running command finish after a stop request, and runs the next call over a new connection',
        Effect.fnUntraced(function* () {
            const sshd = yield* Sshd;
            const [{ ssh, asked }, host, made] = yield* Effect.all([sshd.client, sshd.hosted('stopped'), sshd.scratch]);
            yield* ssh.run(host, ['mkfifo', `${made}/ready`, `${made}/gate`]);
            const running = yield* Effect.forkScoped(ssh.run(host, ['sh', '-c', `echo "$SSH_CONNECTION" > ${made}/ready; cat ${made}/gate; echo finished`]));
            const before = yield* ssh.run(host, ['cat', `${made}/ready`]);
            yield* asked(host, 'stop');
            const after = yield* ssh.run(host, ['sh', '-c', `echo released > ${made}/gate; echo "$SSH_CONNECTION"`]);
            assert.deepStrictEqual([yield* Fiber.join(running), after === before], ['released\nfinished\n', false]);
        }),
    );

    methods.effect(
        'reports a command whose shared connection an exit request ended as unreachable, and runs the next call over a new connection',
        Effect.fnUntraced(function* () {
            const sshd = yield* Sshd;
            const [{ ssh, asked }, host, made] = yield* Effect.all([sshd.client, sshd.hosted('exited'), sshd.scratch]);
            yield* ssh.run(host, ['mkfifo', `${made}/ready`, `${made}/gate`]);
            const running = yield* Effect.forkScoped(reasoned(ssh.run(host, ['sh', '-c', `echo "$SSH_CONNECTION" > ${made}/ready; cat ${made}/gate`])));
            const before = yield* ssh.run(host, ['cat', `${made}/ready`]);
            yield* asked(host, 'exit');
            const ended = yield* Fiber.join(running);
            const after = yield* ssh.run(host, ['sh', '-c', `: <> ${made}/gate; echo "$SSH_CONNECTION"`]);
            assert.deepStrictEqual([ended, after === before], ['Unreachable', false]);
        }),
    );
});
