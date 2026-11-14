import { globSync, readFileSync } from 'node:fs';
import { basename, dirname, isAbsolute, join, resolve } from 'node:path';
import process from 'node:process';
import { Array, Cause, Effect, Record, Schema, Stream } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';
import { build, type PlistValue, parse } from 'plist';
import type { Plugin, Rolldown } from 'vite';

// --- [MODELS] --------------------------------------------------------------------------

const Settings = Schema.fromJsonString(Schema.NonEmptyArray(Schema.Struct({ buildSettings: Schema.Struct({ PRODUCT_BUNDLE_IDENTIFIER: Schema.String, CODE_SIGN_ENTITLEMENTS: Schema.String, DEVELOPMENT_TEAM: Schema.String, PROJECT_DIR: Schema.String }) }))).annotate({
    description: 'xcodebuild -showBuildSettings -json rows, the named target first',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const ran = Effect.fnUntraced(function* (command: string, args: readonly string[]) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore' });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? stdout : yield* new Cause.UnknownError(stderr, `${command} exited ${code}: ${stderr}`);
}, Effect.scoped);

const helper = Effect.fnUntraced(function* (
    entry: string,
    service: string,
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
    xcode: { readonly project: string; readonly target: string },
) {
    const name = basename(dirname(entry));
    const context = yield* Effect.context<ChildProcessSpawner.ChildProcessSpawner>();
    const signing = yield* Effect.cached(
        ran('xcodebuild', ['-showBuildSettings', '-json', '-project', xcode.project, '-target', xcode.target]).pipe(
            Effect.flatMap(Schema.decodeEffect(Settings)),
            Effect.map(([{ buildSettings }]) => ({ ...buildSettings, label: `${buildSettings.PRODUCT_BUNDLE_IDENTIFIER}.${service}` })),
        ),
    );
    let outDir = '';
    return {
        name: 'helper',
        config: () => ({ define: { __dirname: 'import.meta.dirname', __filename: 'import.meta.filename' }, build: { ssr: true, rolldownOptions: { input: { [name]: entry }, output: { entryFileNames: '[name].mjs', codeSplitting: false } } }, ssr: { noExternal: true } }),
        configResolved({ root, build: options }): void {
            outDir = resolve(root, options.outDir);
        },
        generateBundle(this: Rolldown.PluginContext, _options, output): Promise<void> {
            const folders = Array.dedupe(Object.values(output).flatMap((file) => (file.type === 'chunk' ? file.moduleIds.filter(isAbsolute).map(dirname) : [])));
            const assets = Record.fromIterableWith(
                Array.flatMap(folders, (folder) => globSync('*.wasm', { cwd: folder, withFileTypes: true })),
                ({ name: file, parentPath }) => [file, join(parentPath, file)],
            );
            const sea = JSON.stringify({ main: join(outDir, `${name}.mjs`), mainFormat: 'module', output: join(outDir, name), useVfs: true, useCodeCache: true, disableExperimentalSEAWarning: true, assets });
            return signing.pipe(
                Effect.map(({ CODE_SIGN_ENTITLEMENTS, DEVELOPMENT_TEAM, PROJECT_DIR, label }): readonly Rolldown.EmittedAsset[] => {
                    const entitlements = parse(readFileSync(resolve(PROJECT_DIR, CODE_SIGN_ENTITLEMENTS), 'utf8'));
                    return [
                        { type: 'asset', fileName: 'sea.json', source: sea },
                        { type: 'asset', fileName: 'sea.entitlements', source: build({ ...(typeof entitlements === 'object' && !Array.isArray(entitlements) && !(entitlements instanceof Date) && !(entitlements instanceof Uint8Array) ? entitlements : {}), 'com.apple.security.cs.allow-jit': true }) },
                        { type: 'asset', fileName: join('LaunchAgents', `${label}.plist`), source: build({ ProcessType: 'Interactive', ...agent, Label: label, BundleProgram: `Contents/Helpers/${name}`, SpawnConstraint: { 'signing-identifier': label, 'team-identifier': DEVELOPMENT_TEAM } }) },
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
                        ([command, ...args]) => ran(command, args),
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
