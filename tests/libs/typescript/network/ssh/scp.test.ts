import { hash } from 'node:crypto';
import { assert, it } from '@effect/vitest';
import { Effect, FileSystem, Path } from 'effect';
import { bounds, reasoned, Sshd } from '#/sshd';

// --- [CONSTANTS] -----------------------------------------------------------------------

const data = new TextEncoder().encode('scp regress data\n'.repeat(4096));
const summed = hash('sha256', data);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Sshd.layer, { timeout: bounds.started, excludeTestServices: true, concurrent: true })('scp', (methods) => {
    methods.effect(
        'copies a file to the remote file it names, and into the remote folder it names',
        Effect.fnUntraced(function* () {
            const [sshd, fs, path] = yield* Effect.all([Sshd, FileSystem.FileSystem, Path.Path]);
            const [{ ssh }, host, folder, made] = yield* Effect.all([sshd.client, sshd.hosted('files'), fs.makeTempDirectoryScoped(), sshd.scratch]);
            yield* fs.writeFile(path.join(folder, 'data'), data);
            yield* ssh.run(host, ['mkdir', `${made}/folder`]);
            yield* Effect.forEach([`${made}/copy`, `${made}/folder`], (target) => ssh.copy(host, path.join(folder, 'data'), target));
            const sums = yield* ssh.run(host, ['sha256sum', `${made}/copy`, `${made}/folder/data`]);
            assert.deepStrictEqual(sums.trim().split('\n'), [`${summed}  ${made}/copy`, `${summed}  ${made}/folder/data`]);
        }),
        bounds.test,
    );

    methods.effect(
        'copies a folder with its subfolders and the targets of its links',
        Effect.fnUntraced(function* () {
            const [sshd, fs, path] = yield* Effect.all([Sshd, FileSystem.FileSystem, Path.Path]);
            const [{ ssh }, host, folder, made] = yield* Effect.all([sshd.client, sshd.hosted('folders'), fs.makeTempDirectoryScoped(), sshd.scratch]);
            const grown = path.join(folder, 'grown');
            yield* fs.makeDirectory(path.join(grown, 'subdir'), { recursive: true });
            yield* Effect.all([fs.writeFile(path.join(grown, 'copy'), data), fs.writeFile(path.join(grown, 'subdir', 'copy'), data)]);
            yield* Effect.all([fs.symlink(path.join(grown, 'copy'), path.join(grown, 'copy-sym')), fs.symlink(path.join(grown, 'subdir'), path.join(grown, 'subdir-sym'))]);
            yield* ssh.copy(host, grown, `${made}/up`);
            const up = yield* ssh.run(host, ['sh', '-c', `cd ${made}/up && find . -type f | sort | xargs sha256sum`]);
            assert.deepStrictEqual(up.trim().split('\n'), [`${summed}  ./copy`, `${summed}  ./copy-sym`, `${summed}  ./subdir-sym/copy`, `${summed}  ./subdir/copy`]);
        }),
        bounds.test,
    );

    methods.effect(
        'fails a copy of a missing file with the exit status of scp',
        Effect.fnUntraced(function* () {
            const [sshd, fs, path] = yield* Effect.all([Sshd, FileSystem.FileSystem, Path.Path]);
            const [{ ssh }, host, folder, made] = yield* Effect.all([sshd.client, sshd.hosted('named'), fs.makeTempDirectoryScoped(), sshd.scratch]);
            assert.match(yield* reasoned(ssh.copy(host, path.join(folder, 'absent'), `${made}/absent`)), /^Exit [1-9]\d*$/u);
        }),
        bounds.test,
    );
});
