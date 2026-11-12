import { createHash } from 'node:crypto';
import { assert, it } from '@effect/vitest';
import { Forward, Ssh } from '@rasm/ssh';
import { Array, Bytes, Effect, Exit, Fiber, FileSystem, Path, Record, Schedule, Schema, Stream } from 'effect';
import type { ChildProcess } from 'effect/process';
import { answered, banner, bounds, Master, reached, Sshd, served, vacant } from '#/sshd';

// --- [CONSTANTS] -----------------------------------------------------------------------

const statuses = ['0', '1', '4', '5', '44'];
const digests = /^(?:[0-9a-f]{32} {2}-\n){6}$/u;
const listened = 3301;
const identified = new TextDecoder().decode(banner);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Sshd.layer(), { timeout: bounds.started, excludeTestServices: true })('multiplex', (methods) => {
    methods.effect(
        'reports each remote exit status on shared and own connections, and after the command closes stdout and stderr early',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted }] = yield* Effect.all([Ssh, Sshd]);
            const host = yield* hosted('statuses');
            const exited = (script: string, options?: ChildProcess.CommandOptions) =>
                ssh.run(host, ['sh', '-c', script], options).pipe(
                    Effect.as('0'),
                    Effect.catchReason('SshError', 'Exit', ({ code }) => Effect.succeed(`${code}`)),
                );
            const exits = yield* Effect.forEach(statuses, (status) => Effect.all([exited(`exit ${status}`), exited(`exit ${status}`, { env: { SENT: 'sent' } }), exited(`sleep 2; exec > /dev/null 2>&1; sleep 3; exit ${status}`)], { concurrency: 'unbounded' }), {
                concurrency: 'unbounded',
            });
            assert.deepStrictEqual(
                exits,
                Array.map(statuses, (status) => [status, status, status]),
            );
        }),
        bounds.test,
    );

    methods.effect(
        'delivers stderr a command writes after it closed stdout',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted }] = yield* Effect.all([Ssh, Sshd]);
            const host = yield* hosted('stderr');
            const written = yield* ssh.run(host, ['sh', '-c', 'for i in 1 2 3 4 5 6; do (date; echo $i) | md5sum; done | tee /tmp/stderr-after-eof']);
            const late = yield* ssh.spawn(host, ['sh', '-c', 'exec > /dev/null; sleep 2; cat /tmp/stderr-after-eof 1>&2']);
            const [code, stderr] = yield* Effect.all([late.exitCode, Stream.mkString(Stream.decodeText(late.stderr))], { concurrency: 'unbounded' });
            assert.deepStrictEqual([code, stderr], [0, written]);
            assert.match(written, digests);
        }, Effect.scoped),
        bounds.test,
    );

    methods.effect(
        'transfers a binary file over the shared connection unchanged',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted }] = yield* Effect.all([Ssh, Sshd]);
            const [host, data] = [yield* hosted('transfer'), '/bin/busybox'];
            const summed = yield* ssh.run(host, ['sha256sum', data]);
            const sent = yield* ssh.spawn(host, ['cat', data]);
            const [code, chunks] = yield* Effect.all([sent.exitCode, Stream.runCollect(sent.stdout)], { concurrency: 'unbounded' });
            assert.deepStrictEqual([code, `${createHash('sha256').update(Bytes.concat(chunks)).digest('hex')}  ${data}\n`], [0, summed]);
        }, Effect.scoped),
        bounds.test,
    );

    methods.effect(
        'answers a check request on the control socket of a live connection, and reports the address it dialed',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, asked, far }] = yield* Effect.all([Ssh, Sshd]);
            const host = yield* hosted('checked');
            yield* reached(ssh, host);
            const [, pid] = yield* Effect.flatMap(asked(host, 'check'), (shown) => Schema.decodeUnknownEffect(Master)(shown.trim()));
            assert.isAbove(pid, 0);
            assert.match(yield* asked(host, 'conninfo'), new RegExp(`-> \\S+:${far.dial.port}\\b`, 'u'));
        }),
        bounds.test,
    );

    methods.effect(
        'forwards local and remote TCP ports and socket paths over the shared connection, and stops each listener with its scope',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, far }, fs, path] = yield* Effect.all([Ssh, Sshd, FileSystem.FileSystem, Path.Path]);
            const [host, folder, port] = yield* Effect.all([hosted('forwarded'), fs.makeTempDirectoryScoped(), vacant]);
            const [near, back] = [`127.0.0.1:${served}`, `${far.dial.host}:${far.dial.port}`];
            const heard = (flags: string) => Effect.map(ssh.run(host, ['sh', '-c', `nc -w 2 ${flags} </dev/null 2>/dev/null | head -c ${banner.length}`]), (shown) => shown === identified);
            const kinds = {
                localPort: { forward: { _tag: 'Local', bind: `127.0.0.1:${port}`, connect: near }, listening: answered({ host: '127.0.0.1', port }, banner) },
                localPath: { forward: { _tag: 'Local', bind: path.join(folder, 'unix-1.fwd'), connect: near }, listening: answered({ path: path.join(folder, 'unix-1.fwd') }, banner) },
                remotePort: { forward: { _tag: 'Remote', bind: `127.0.0.1:${listened}`, connect: back }, listening: heard(`127.0.0.1 ${listened}`) },
                remotePath: { forward: { _tag: 'Remote', bind: '/tmp/unix-1.fwd', connect: back }, listening: heard('-U /tmp/unix-1.fwd') },
            } as const;
            const wanted = yield* Effect.all(Record.map(kinds, ({ forward }) => Schema.decodeEffect(Forward)(forward)));
            const held = yield* Effect.all(
                Record.map(kinds, ({ listening }, kind) => Effect.scoped(Effect.andThen(ssh.forward(host, wanted[kind]), listening))),
                { concurrency: 'unbounded' },
            );
            const cancelled = yield* Effect.all(
                Record.map(kinds, ({ listening }) => listening),
                { concurrency: 'unbounded' },
            );
            assert.deepStrictEqual([held, cancelled], [Record.map(kinds, () => true), Record.map(kinds, () => false)]);
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
