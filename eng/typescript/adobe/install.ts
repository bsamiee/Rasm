import { HostError } from '@rasm/automation';
import { extension, replaced, running } from '@rasm/automation/installer';
import { ExtensionManifest } from '@rasm/cep';
import { UxpManifest } from '@rasm/uxp';
import { Array, Effect, FileSystem, Path } from 'effect';
import { Argument, Command } from 'effect/cli';
import { ChildProcess } from 'effect/process';
import { decodeFile } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';

// --- [OPERATIONS] ----------------------------------------------------------------------

const uxp = Effect.fn('uxp')(function* (built: string) {
    const manifest = yield* decodeFile(UxpManifest.json, built, UxpManifest.file);
    yield* running(manifest.host.app, ChildProcess.make('watch-apps', ['serve'], { stderr: 'inherit' })).pipe(
        Effect.filterOrFail(Array.isReadonlyArrayEmpty, (instances) => HostError.make({ reason: { _tag: 'Unavailable' }, subject: `Install of ${manifest.name} beside ${manifest.host.app} process ${instances.join(', ')}` })),
    );
    yield* replaced(manifest, `${built}.ccx`);
});
const cep = Effect.fn('cep')(function* (built: string) {
    const [fs, path, { id }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, decodeFile(ExtensionManifest.fromXml, built, ExtensionManifest.file)]);
    const folder = yield* extension(id);
    yield* fs.remove(folder, { recursive: true, force: true });
    yield* fs.makeDirectory(folder, { recursive: true });
    yield* runCommand('cmake', ['-E', 'tar', 'xf', path.resolve(`${built}.zxp`)], { cwd: folder });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const install = Command.make(
    'install',
    { built: Argument.Directory('built', { mustExist: true }) },
    Effect.fn('install')(function* ({ built }) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        return yield* (yield* fs.exists(path.join(built, UxpManifest.file))) ? uxp(built) : cep(built);
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { install };
