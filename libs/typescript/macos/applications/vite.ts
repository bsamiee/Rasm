import { execFileSync } from 'node:child_process';
import { globSync, readFileSync } from 'node:fs';
import { basename, dirname, isAbsolute, join, resolve } from 'node:path';
import process from 'node:process';
import { Array, Record } from 'effect';
import { build, type PlistValue, parse } from 'plist';
import type { Plugin, Rolldown } from 'vite';

// --- [TYPES] ---------------------------------------------------------------------------

interface Signing {
    readonly identifier: string;
    readonly entitlements: string;
    readonly team: string;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const dictionary = (value: PlistValue) => (typeof value === 'object' && !Array.isArray(value) && !(value instanceof Date) && !(value instanceof Uint8Array) ? value : {});
const helper = (
    entry: string,
    { identifier, entitlements, team }: Signing,
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
) => {
    const name = basename(dirname(entry));
    const label = `${identifier}.${service}`;
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
                source: build({ ProcessType: 'Interactive', ...agent, Label: label, BundleProgram: `Contents/Helpers/${name}`, SpawnConstraint: { 'signing-identifier': label, 'team-identifier': team } }),
            });
        },
        closeBundle(): void {
            execFileSync(process.execPath, ['--build-sea', join(outDir, 'sea.json')], { stdio: 'inherit' });
            execFileSync('codesign', ['--force', '--sign', '-', '--identifier', label, '--options', 'runtime', '--entitlements', join(outDir, 'sea.entitlements'), join(outDir, name)], { stdio: 'inherit' });
        },
    } satisfies Plugin;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { helper };
