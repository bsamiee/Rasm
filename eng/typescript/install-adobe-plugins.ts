#!/usr/bin/env node
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { HostError } from '@rasm/automation';
import { extension, installed, replaced, running } from '@rasm/automation/installer';
import { ExtensionManifest } from '@rasm/cep';
import { UxpManifest } from '@rasm/uxp';
import { Array, Console, Effect, FileSystem, Path, Schema } from 'effect';
import { Argument, Command } from 'effect/cli';
import { ChildProcess } from 'effect/process';
import nx from 'nx/package.json' with { type: 'json' };
import { decodeFile } from '#/manifest.ts';
import { runCommand } from '#/run-command.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Installed = Schema.fromJsonString(Schema.Struct({ installed: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const uxp = Effect.fn('uxp')(function* (built: string) {
    const manifest = yield* decodeFile(UxpManifest.json, built, UxpManifest.file);
    yield* running(manifest.host.app, ChildProcess.make('watch-apps', ['serve'], { stderr: 'inherit' })).pipe(
        Effect.filterOrFail(Array.isReadonlyArrayEmpty, (instances) => HostError.make({ reason: { _tag: 'Unavailable' }, subject: `Install of ${manifest.name} beside ${manifest.host.app} process ${instances.join(', ')}` })),
    );
    yield* replaced(manifest, `${built}.ccx`);
    return yield* installed(manifest);
});
const cep = Effect.fn('cep')(function* (built: string) {
    const [fs, path, { id }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, decodeFile(ExtensionManifest.fromXml, built, ExtensionManifest.file)]);
    const folder = yield* extension(id);
    const parent = path.dirname(folder);
    yield* fs.makeDirectory(parent, { recursive: true });
    const stage = yield* fs.makeTempDirectoryScoped({ directory: parent });
    yield* runCommand('cmake', ['-E', 'tar', 'xf', path.resolve(`${built}.zxp`)], { cwd: stage });
    yield* fs.remove(folder, { recursive: true, force: true });
    yield* fs.rename(stage, folder);
    return folder;
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const install = Command.make(
    'rasm-install-adobe-plugins',
    { built: Argument.Directory('built', { mustExist: true }) },
    Effect.fn('install')(function* ({ built }) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const folder = yield* (yield* fs.exists(path.join(built, UxpManifest.file))) ? uxp(built) : cep(built);
        yield* Effect.flatMap(Schema.encodeEffect(Installed)({ installed: folder }), Console.log);
    }),
);

if (import.meta.main) {
    Command.run(install, { version: nx.version }).pipe(Effect.provide(layer), runMain);
}
