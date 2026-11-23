import { createHash } from 'node:crypto';
import { assert, it } from '@effect/vitest';
import { Ssh, SshError } from '@rasm/ssh';
import { Array, Bytes, Effect, Option, pipe, Schema, Stream, Struct } from 'effect';
import type { ChildProcess } from 'effect/process';
import { bounds, reached, reasoned, Sshd } from '#/sshd';

// --- [CONSTANTS] -----------------------------------------------------------------------

const statuses = ['0', '1', '4', '5', '44'];
const digests = /^(?:[0-9a-f]{32} {2}-\n){6}$/u;

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Sshd.layer(), { timeout: bounds.started, excludeTestServices: true, concurrent: true })('commands', (methods) => {
    methods.effect(
        'runs a command with its environment and reports its exit',
        Effect.fnUntraced(function* () {
            const [ssh, sshd] = yield* Effect.all([Ssh, Sshd]);
            const host = yield* sshd.hosted('commands');
            const [sent, failed] = yield* Effect.all([ssh.run(host, ['sh', '-c', 'printf "[%s][%s]" "$GREETING" "$SECOND"'], { env: { GREETING: 'two words "quoted"', SECOND: 'second' } }), Effect.flip(ssh.run(host, ['sh', '-c', 'echo printed; echo refused >&2; exit 7']))]);
            assert.strictEqual(sent, '[two words "quoted"][second]');
            assert.deepStrictEqual(
                pipe(
                    Option.liftPredicate(failed, Schema.is(SshError)),
                    Option.map(({ reason }) => reason),
                    Option.filter(SshError.fields.reason.guards.Exit),
                    Option.map(({ exit }) => Struct.pick(exit, ['code', 'stdout', 'stderr'])),
                ),
                Option.some({ code: 7, stdout: 'printed\n', stderr: 'refused\n' }),
            );
        }),
        bounds.test,
    );

    methods.effect(
        'reports each remote exit status on shared and own connections, and after the command closes stdout and stderr early',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted }] = yield* Effect.all([Ssh, Sshd]);
            const host = yield* hosted('statuses');
            const exited = (script: string, options?: ChildProcess.CommandOptions) =>
                ssh.run(host, ['sh', '-c', script], options).pipe(
                    Effect.as('0'),
                    Effect.catchReason('SshError', 'Exit', ({ exit }) => Effect.succeed(`${exit.code}`)),
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
        'reports exit 255 of a command on a live shared connection as its exit, never as an unreachable host',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, far }] = yield* Effect.all([Ssh, Sshd]);
            const host = yield* hosted('exit-255');
            const name = yield* reached(ssh, host);
            const failed = yield* Effect.flip(ssh.run(host, ['sh', '-c', 'exit 255']));
            assert.deepStrictEqual([name, reasoned(failed), Schema.is(SshError)(failed) ? SshError.fields.reason.match(failed.reason, { Exit: ({ exit }) => exit.code, Unreachable: () => undefined, Refused: () => undefined }) : undefined], [far.name, 'Exit', 255]);
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
        'runs the next command over the shared connection after a running command is interrupted',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, far }] = yield* Effect.all([Ssh, Sshd]);
            const host = yield* hosted('interrupted');
            const cut = yield* Effect.timeoutOption(ssh.run(host, ['sleep', '600']), '2 seconds');
            assert.deepStrictEqual([Option.isNone(cut), yield* reached(ssh, host)], [true, far.name], 'Effect-TS/effect@7865dbb packages/sql/pg/test/Client.integration.test.ts "interruption"; openssh/openssh-portable regress/multiplex.sh -Ocheck after a session ends');
        }),
        bounds.test,
    );
});
