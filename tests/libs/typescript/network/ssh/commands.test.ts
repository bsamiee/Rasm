import { hash } from 'node:crypto';
import { assert, it } from '@effect/vitest';
import { Forward, SshError } from '@rasm/ssh';
import { Effect, FileSystem, Option, Path, pipe, Schema, Struct } from 'effect';
import { reasoned, Sshd } from '#/sshd';

// --- [CONSTANTS] -----------------------------------------------------------------------

const files = { top: new TextEncoder().encode('top file\n'.repeat(4096)), nested: new TextEncoder().encode('nested file\n'.repeat(4096)) };
const summed = { top: hash('sha256', files.top), nested: hash('sha256', files.nested) };

// --- [OPERATIONS] ----------------------------------------------------------------------

const exited = (failed: unknown) =>
    pipe(
        Option.liftPredicate(failed, Schema.is(SshError)),
        Option.map(({ reason }) => reason),
        Option.filter(SshError.fields.reason.guards.Exit),
        Option.map(({ exit }) => Struct.pick(exit, ['code', 'stdout', 'stderr'])),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Sshd.layer, { excludeTestServices: true, concurrent: true })('commands', (methods) => {
    methods.effect.each([
        { status: 254, env: {}, reported: 'Exit 254' },
        { status: 255, env: {}, reported: 'Exit 255' },
        { status: 254, env: { SENT: 'sent' }, reported: 'Exit 254' },
        { status: 255, env: { SENT: 'sent' }, reported: 'Unreachable' },
    ])(
        'reports a command exiting $status, sent with environment $env beside a live shared connection, as $reported',
        Effect.fnUntraced(function* ({ status, env, reported }) {
            const sshd = yield* Sshd;
            const [{ ssh }, host] = yield* Effect.all([sshd.client, sshd.hosted('exits')]);
            yield* ssh.run(host, ['true']);
            assert.strictEqual(yield* reasoned(ssh.run(host, ['sh', '-c', `exit ${status}`], { env })), reported);
        }),
    );

    methods.effect(
        'carries the environment of a call to its command, and fails a command with its exit status, output, and errors',
        Effect.fnUntraced(function* () {
            const sshd = yield* Sshd;
            const [{ ssh }, host] = yield* Effect.all([sshd.client, sshd.hosted('printed')]);
            const sent = yield* ssh.run(host, ['sh', '-c', 'printf "[%s][%s]" "$GREETING" "$SECOND"'], { env: { GREETING: 'two words "quoted"', SECOND: 'second' } });
            const failed = yield* Effect.flip(ssh.run(host, ['sh', '-c', 'echo printed; echo refused >&2; exit 7']));
            assert.deepStrictEqual([sent, exited(failed)], ['[two words "quoted"][second]', Option.some({ code: 7, stdout: 'printed\n', stderr: 'refused\n' })]);
        }),
    );

    methods.effect.each(['run', 'sent', 'copy', 'forward'] as const)(
        'reports a %s on a host whose dial answers no login as unreachable',
        Effect.fnUntraced(function* (call) {
            const [sshd, fs, path] = yield* Effect.all([Sshd, FileSystem.FileSystem, Path.Path]);
            const [{ ssh }, { dial }, folder] = yield* Effect.all([sshd.client, sshd.port, fs.makeTempDirectoryScoped()]);
            const [host, wanted] = yield* Effect.all([sshd.hosted('silent', dial), Schema.decodeEffect(Forward)({ _tag: 'Local', bind: path.join(folder, 'bound'), connect: '127.0.0.1:22' })]);
            yield* fs.writeFileString(path.join(folder, 'data'), 'data');
            const calls = { run: ssh.run(host, ['true']), sent: ssh.run(host, ['true'], { env: { SENT: 'sent' } }), copy: ssh.copy(host, path.join(folder, 'data'), '/tmp/data'), forward: ssh.forward(host, wanted) };
            assert.strictEqual(yield* reasoned(calls[call]), 'Unreachable');
        }),
    );

    methods.effect(
        'reaches a host that was unreachable once its dial answers a login',
        Effect.fnUntraced(function* () {
            const sshd = yield* Sshd;
            const [{ ssh }, { inside, dial }] = yield* Effect.all([sshd.client, sshd.port]);
            const host = yield* sshd.hosted('woken', dial);
            const before = yield* reasoned(ssh.run(host, ['true']));
            yield* sshd.listening(inside);
            assert.deepStrictEqual([before, yield* ssh.run(host, ['sh', '-c', 'set -- $SSH_CONNECTION; echo "$4"'])], ['Unreachable', `${inside}\n`]);
        }),
    );

    methods.effect(
        'copies a folder with its subfolders and the targets of its links',
        Effect.fnUntraced(function* () {
            const [sshd, fs, path] = yield* Effect.all([Sshd, FileSystem.FileSystem, Path.Path]);
            const [{ ssh }, host, folder, made] = yield* Effect.all([sshd.client, sshd.hosted('folders'), fs.makeTempDirectoryScoped(), sshd.scratch]);
            const grown = path.join(folder, 'grown');
            yield* fs.makeDirectory(path.join(grown, 'subdir'), { recursive: true });
            yield* Effect.all([fs.writeFile(path.join(grown, 'copy'), files.top), fs.writeFile(path.join(grown, 'subdir', 'copy'), files.nested)]);
            yield* Effect.all([fs.symlink(path.join(grown, 'copy'), path.join(grown, 'copy-sym')), fs.symlink(path.join(grown, 'subdir'), path.join(grown, 'subdir-sym'))]);
            yield* ssh.copy(host, grown, `${made}/up`);
            const up = yield* ssh.run(host, ['sh', '-c', `cd ${made}/up && find . -type f | sort | xargs sha256sum`]);
            assert.deepStrictEqual(up.trim().split('\n'), [`${summed.top}  ./copy`, `${summed.top}  ./copy-sym`, `${summed.nested}  ./subdir-sym/copy`, `${summed.nested}  ./subdir/copy`]);
        }),
    );
});
