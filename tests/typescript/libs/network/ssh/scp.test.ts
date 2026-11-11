import { createHash } from 'node:crypto';
import { assert, it } from '@effect/vitest';
import { Ssh, SshError } from '@rasm/ssh';
import { Array, Effect, FileSystem, Path, Record, Schema } from 'effect';
import { bounds, Sshd } from '#/sshd';

// --- [CONSTANTS] -----------------------------------------------------------------------

const lines = 16_384;
const data = new TextEncoder().encode('scp regress data\n'.repeat(lines));
const summed = createHash('sha256').update(data).digest('hex');
const forest = ['copy', 'copy-sym', 'subdir-sym/copy', 'subdir/copy'];

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Sshd.layer(), { timeout: bounds.started, excludeTestServices: true })('scp', (methods) => {
    methods.effect(
        'copies a file to a remote file, over a longer file, into a folder, and to a name holding glob characters',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted }, fs, path] = yield* Effect.all([Ssh, Sshd, FileSystem.FileSystem, Path.Path]);
            const [host, local] = yield* Effect.all([hosted('files'), Effect.map(fs.makeTempDirectoryScoped(), (folder) => path.join(folder, 'data'))]);
            const remote = Record.map({ copy: 'copy', clobbered: 'clobbered', dir: 'copy.dd', glob: 'copy.glob[123]' }, (name) => `/tmp/scp/${name}`);
            yield* fs.writeFile(local, data);
            yield* ssh.run(host, ['sh', '-c', `mkdir -p ${remote.dir} && head -c ${2 * data.length} /dev/zero > ${remote.clobbered}`]);
            yield* Effect.forEach(Object.values(remote), (target) => ssh.copy(host, local, target));
            const sums = yield* ssh.run(host, ['sha256sum', remote.copy, remote.clobbered, `${remote.dir}/data`, remote.glob]);
            assert.deepStrictEqual(
                Array.map(sums.trim().split('\n'), (line) => line.slice(0, summed.length)),
                Array.map(Object.values(remote), () => summed),
            );
        }, Effect.scoped),
        bounds.test,
    );

    methods.effect(
        'copies a folder with its subfolders and the targets of its links',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted }, fs, path] = yield* Effect.all([Ssh, Sshd, FileSystem.FileSystem, Path.Path]);
            const [host, grown] = yield* Effect.all([hosted('folders'), Effect.map(fs.makeTempDirectoryScoped(), (folder) => path.join(folder, 'grown'))]);
            yield* fs.makeDirectory(path.join(grown, 'subdir'), { recursive: true });
            yield* Effect.all([fs.writeFile(path.join(grown, 'copy'), data), fs.writeFile(path.join(grown, 'subdir', 'copy'), data)]);
            yield* Effect.all([fs.symlink(path.join(grown, 'copy'), path.join(grown, 'copy-sym')), fs.symlink(path.join(grown, 'subdir'), path.join(grown, 'subdir-sym'))]);
            yield* ssh.run(host, ['sh', '-c', 'rm -rf /tmp/forest && mkdir -p /tmp/forest']);
            yield* ssh.copy(host, grown, '/tmp/forest/up');
            const up = yield* ssh.run(host, ['sh', '-c', 'cd /tmp/forest/up && find -L . -type f | sort | while read -r file; do sha256sum "$file"; done']);
            assert.deepStrictEqual(
                up.trim().split('\n'),
                Array.map(forest, (file) => `${summed}  ./${file}`),
            );
        }, Effect.scoped),
        bounds.test,
    );

    methods.effect(
        'copies a file named in shell metacharacters without running it, and fails a copy of a missing file with its exit',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted }, fs, path] = yield* Effect.all([Ssh, Sshd, FileSystem.FileSystem, Path.Path]);
            const [host, folder] = yield* Effect.all([hosted('refusals'), fs.makeTempDirectoryScoped()]);
            const named = '`touch metachartest`';
            yield* fs.writeFile(path.join(folder, named), data);
            yield* ssh.run(host, ['sh', '-c', 'rm -rf /tmp/meta && mkdir -p /tmp/meta']);
            yield* ssh.copy(host, path.join(folder, named), '/tmp/meta');
            const listed = yield* ssh.run(host, ['sh', '-c', 'ls /tmp/meta; ls ~ /tmp | grep -c metachartest; true']);
            const missing = yield* Effect.flip(ssh.copy(host, path.join(folder, 'absent'), '/tmp/meta/absent'));
            assert.deepStrictEqual([listed, yield* fs.exists(path.join(folder, 'metachartest')), Schema.is(SshError)(missing) ? missing.reason._tag : missing], [`${named}\n0\n`, false, 'Exit']);
        }, Effect.scoped),
        bounds.test,
    );
});
