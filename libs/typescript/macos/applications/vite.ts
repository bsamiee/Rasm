import { execFileSync } from 'node:child_process';
import { globSync, readFileSync } from 'node:fs';
import { basename, dirname, isAbsolute, join, resolve } from 'node:path';
import process from 'node:process';
import { Array, Config, Effect, Record, Schema } from 'effect';
import { build, type PlistValue, parse } from 'plist';
import { type Plugin, type Rolldown, searchForWorkspaceRoot } from 'vite';

// --- [MODELS] --------------------------------------------------------------------------

const Signing = Schema.Struct({ PRODUCT_BUNDLE_IDENTIFIER: Schema.String, CODE_SIGN_ENTITLEMENTS: Schema.String, DEVELOPMENT_TEAM: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const dictionary = (value: PlistValue) => (typeof value === 'object' && !Array.isArray(value) && !(value instanceof Date) && !(value instanceof Uint8Array) ? value : {});
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
) {
    const name = basename(dirname(entry));
    const settings = yield* Config.schema(Signing);
    const entitlements = resolve(searchForWorkspaceRoot(dirname(entry)), settings.CODE_SIGN_ENTITLEMENTS);
    const label = `${settings.PRODUCT_BUNDLE_IDENTIFIER}.${service}`;
    let outDir = '';
    return {
        name: 'helper',
        config: () => ({ define: { __dirname: 'import.meta.dirname', __filename: 'import.meta.filename' }, build: { ssr: true, rolldownOptions: { input: { [name]: entry }, output: { entryFileNames: '[name].mjs', codeSplitting: false } } }, ssr: { noExternal: true } }),
        configResolved({ root, build: options }): void {
            outDir = resolve(root, options.outDir);
        },
        generateBundle(this: Rolldown.PluginContext, _options, output): void {
            const folders = Array.dedupe(Object.values(output).flatMap((file) => (file.type === 'chunk' ? file.moduleIds.filter(isAbsolute).map(dirname) : [])));
            const assets = Record.fromIterableWith(
                Array.flatMap(folders, (folder) => globSync('*.wasm', { cwd: folder, withFileTypes: true })),
                ({ name: file, parentPath }) => [file, join(parentPath, file)],
            );
            this.emitFile({ type: 'asset', fileName: 'sea.json', source: JSON.stringify({ main: join(outDir, `${name}.mjs`), mainFormat: 'module', output: join(outDir, name), useVfs: true, disableExperimentalSEAWarning: true, assets }) });
            this.emitFile({ type: 'asset', fileName: 'sea.entitlements', source: build({ ...dictionary(parse(readFileSync(entitlements, 'utf8'))), 'com.apple.security.cs.allow-jit': true }) });
            this.emitFile({
                type: 'asset',
                fileName: join('LaunchAgents', `${label}.plist`),
                source: build({ ProcessType: 'Interactive', ...agent, Label: label, BundleProgram: `Contents/Helpers/${name}`, SpawnConstraint: { 'signing-identifier': label, 'team-identifier': settings.DEVELOPMENT_TEAM } }),
            });
        },
        closeBundle(): void {
            execFileSync(process.execPath, ['--build-sea', join(outDir, 'sea.json')], { stdio: 'inherit' });
            execFileSync('codesign', ['--force', '--sign', '-', '--identifier', label, '--options', 'runtime', '--entitlements', join(outDir, 'sea.entitlements'), join(outDir, name)], { stdio: 'inherit' });
        },
    } satisfies Plugin;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { helper };
