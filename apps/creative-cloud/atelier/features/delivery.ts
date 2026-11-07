import process from 'node:process';
import type { RunningInstance } from '@rasm/applications/contract';
import { hosts, registered, replaced } from '@rasm/automation/installer';
import { UxpManifest } from '@rasm/uxp/manifest';
import { Array, Effect, FileSystem, Option, Path, Predicate, Result, Schema, Stream, String, SubscriptionRef } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Adobe } from '#/adobe';

// --- [OPERATIONS] ----------------------------------------------------------------------

const contents = Effect.map(Path.Path, (path) => path.join(path.dirname(process.execPath), '..'));
const executable = Effect.gen(function* () {
    const [folder, path, spawner] = yield* Effect.all([contents, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
    const name = yield* spawner.string(ChildProcess.make('/usr/bin/plutil', ['-extract', 'CFBundleExecutable', 'raw', '-o', '-', path.join(folder, 'Info.plist')], { stdin: 'ignore', stderr: 'inherit' }));
    return path.join(folder, 'MacOS', name.trim());
});
const reconciled = Effect.fnUntraced(
    function* (archive: string) {
        const [adobe, spawner] = yield* Effect.all([Adobe, ChildProcessSpawner.ChildProcessSpawner]);
        const plugin = yield* spawner.string(ChildProcess.make('tar', ['-xOf', archive, UxpManifest.file], { stdin: 'ignore', stderr: 'inherit' })).pipe(Effect.flatMap(Schema.decodeEffect(UxpManifest.json)));
        const [held, identifiers] = yield* Effect.all([registered(plugin), hosts(plugin.host.app)]);
        const hosting = ({ identifier }: { readonly identifier: string }) => identifiers.includes(identifier);
        const installed = Stream.filter(SubscriptionRef.changes(adobe.installations), (rows) => Result.isSuccess(rows) && rows.success.some(hosting));
        const elsewhere = (row: RunningInstance) => !hosting(row) || Predicate.hasProperty(row.state, 'exited');
        const quit = Stream.filter(SubscriptionRef.changes(adobe.instances), Array.every(elsewhere));
        return yield* Option.exists(held, ({ versionString }) => versionString === plugin.version) ? Effect.void : Stream.runHead(installed).pipe(Effect.andThen(Stream.runHead(quit)), Effect.andThen(replaced(plugin, archive)));
    },
    Effect.ignoreCause({ log: 'Error' }),
);
const delivered = Effect.gen(function* () {
    const [folder, fs, path] = yield* Effect.all([contents, FileSystem.FileSystem, Path.Path]);
    const plugins = path.join(folder, 'Resources', 'Plugins');
    yield* Effect.forEach((yield* fs.readDirectory(plugins)).filter(String.endsWith('.ccx')), (file) => reconciled(path.join(plugins, file)), { concurrency: 'unbounded', discard: true });
}).pipe(Effect.ignoreCause({ log: 'Error' }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { delivered, executable };
