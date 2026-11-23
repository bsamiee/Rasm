import { readFileSync } from 'node:fs';
import { dirname, isAbsolute, join, normalize, resolve } from 'node:path';
import process from 'node:process';
import { run } from '@rasm/process';
import { Array, Effect, Record } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { build, type PlistValue, parse } from 'plist';
import type { Plugin, Rolldown } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ = /\bnew\s+URL\s*\(\s*(?<path>"[^"]+"|'[^']+'|`[^`$]+`)\s*,\s*import\.meta\.url\s*\)|\.join\s*\(\s*import\.meta\.dirname\s*,\s*(?<path>"[^"]+"|'[^']+'|`[^`$]+`)\s*\)/gu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const helper = Effect.fnUntraced(function* <E>(
    entry: string,
    name: string,
    agent: Readonly<
        Partial<
            Record<
                | 'AbandonProcessGroup'
                | 'Debug'
                | 'Disabled'
                | 'EnableGlobbing'
                | 'EnablePressuredExit'
                | 'EnableTransactions'
                | 'EnvironmentVariables'
                | 'ExitTimeOut'
                | 'HardResourceLimits'
                | 'KeepAlive'
                | 'LaunchEvents'
                | 'LaunchOnlyOnce'
                | 'LegacyTimers'
                | 'LimitLoadFromHardware'
                | 'LimitLoadToHardware'
                | 'LimitLoadToSessionType'
                | 'LowPriorityBackgroundIO'
                | 'LowPriorityIO'
                | 'MachServices'
                | 'Nice'
                | 'ProcessType'
                | 'ProgramArguments'
                | 'QueueDirectories'
                | 'RunAtLoad'
                | 'SessionCreate'
                | 'Sockets'
                | 'SoftResourceLimits'
                | 'StandardErrorPath'
                | 'StandardInPath'
                | 'StandardOutPath'
                | 'StartCalendarInterval'
                | 'StartInterval'
                | 'StartOnMount'
                | 'ThrottleInterval'
                | 'Umask'
                | 'WaitForDebugger'
                | 'WatchPaths'
                | 'WorkingDirectory',
                PlistValue
            >
        >
    >,
    app: Effect.Effect<{ readonly identifier: string; readonly team: string; readonly entitlements: string }, E>,
) {
    const context = yield* Effect.context<ChildProcessSpawner.ChildProcessSpawner>();
    const signing = yield* Effect.cached(Effect.map(app, (signed) => ({ ...signed, label: `${signed.identifier}.${name}` })));
    let outDir = '';
    return {
        name: 'helper',
        config: () => ({ define: { __dirname: 'import.meta.dirname', __filename: 'import.meta.filename' }, build: { ssr: true, emitAssets: true, rolldownOptions: { input: { [name]: entry }, output: { entryFileNames: '[name].mjs', codeSplitting: false } } }, ssr: { noExternal: true } }),
        configResolved({ root, build: options }): void {
            outDir = resolve(root, options.outDir);
        },
        generateBundle(this: Rolldown.PluginContext, _options, output): Promise<void> {
            const read = Record.fromEntries(
                Object.values(output)
                    .flatMap((file) => (file.type === 'chunk' ? Object.entries(file.modules).filter(([id]) => isAbsolute(id)) : []))
                    .flatMap(([id, { code }]) =>
                        (code ?? '')
                            .matchAll(READ)
                            .map(({ groups: { path = '' } = {} }) => path.slice(1, -1))
                            .map((path) => [normalize(path), resolve(dirname(id), path)] as const)
                            .toArray(),
                    ),
            );
            Record.toEntries(read).forEach(([fileName, file]) => {
                this.emitFile({ type: 'asset', fileName, source: readFileSync(file) });
            });
            const assets = Record.fromIterableWith([...Record.keys(read), ...Object.values(output).flatMap((file) => (file.type === 'asset' ? [file.fileName] : []))], (fileName) => [fileName, join(outDir, fileName)]);
            const sea = JSON.stringify({ main: join(outDir, `${name}.mjs`), mainFormat: 'module', output: join(outDir, name), useVfs: true, disableExperimentalSEAWarning: true, assets });
            return signing.pipe(
                Effect.map(({ entitlements: file, team, label }): readonly Rolldown.EmittedAsset[] => {
                    const entitlements = parse(readFileSync(file, 'utf-8'));
                    return [
                        { type: 'asset', fileName: 'sea.json', source: sea },
                        { type: 'asset', fileName: 'sea.entitlements', source: build({ ...(typeof entitlements === 'object' && !Array.isArray(entitlements) && !(entitlements instanceof Date) && !(entitlements instanceof Uint8Array) ? entitlements : {}), 'com.apple.security.cs.allow-jit': true }) },
                        { type: 'asset', fileName: join('LaunchAgents', `${label}.plist`), source: build({ ProcessType: 'Interactive', ...agent, Label: label, BundleProgram: `Contents/Helpers/${name}`, SpawnConstraint: { 'signing-identifier': label, 'team-identifier': team } }) },
                    ];
                }),
                Effect.flatMap(Effect.forEach((file: Rolldown.EmittedAsset) => Effect.sync(() => this.emitFile(file)))),
                Effect.asVoid,
                Effect.runPromiseWith(context),
            );
        },
        closeBundle(): Promise<void> {
            return signing.pipe(
                Effect.flatMap(({ label }) =>
                    Effect.forEach(
                        [
                            [process.execPath, '--build-sea', join(outDir, 'sea.json')],
                            ['codesign', '--force', '--sign', '-', '--identifier', label, '--options', 'runtime', '--entitlements', join(outDir, 'sea.entitlements'), join(outDir, name)],
                        ] as const,
                        (argv) => run(argv),
                        { discard: true },
                    ),
                ),
                Effect.runPromiseWith(context),
            );
        },
    } satisfies Plugin;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { helper };
