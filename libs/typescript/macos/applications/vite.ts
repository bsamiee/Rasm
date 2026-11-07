import { globSync } from 'node:fs';
import { basename, dirname, isAbsolute, join, resolve } from 'node:path';
import { Array, Record } from 'effect';
import { build, type PlistValue } from 'plist';
import type { Plugin, Rolldown } from 'vite';

// --- [OPERATIONS] ----------------------------------------------------------------------

const helper = (
    entry: string,
    identifier: string,
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
    return {
        name: 'helper',
        config: () => ({ define: { __dirname: 'import.meta.dirname', __filename: 'import.meta.filename' }, build: { ssr: true, rolldownOptions: { input: { [name]: entry }, output: { entryFileNames: '[name].mjs', codeSplitting: false } } }, ssr: { noExternal: true } }),
        generateBundle(this: Rolldown.PluginContext, _options, output): void {
            const { root, build: options } = this.environment.config;
            const outDir = resolve(root, options.outDir);
            const folders = Array.dedupe(Object.values(output).flatMap((file) => (file.type === 'chunk' ? file.moduleIds.filter(isAbsolute).map(dirname) : [])));
            const assets = Record.fromIterableWith(
                Array.flatMap(folders, (folder) => globSync('*.wasm', { cwd: folder, withFileTypes: true })),
                ({ name: file, parentPath }) => [file, join(parentPath, file)],
            );
            this.emitFile({ type: 'asset', fileName: 'sea.json', source: JSON.stringify({ main: join(outDir, `${name}.mjs`), mainFormat: 'module', output: join(outDir, name), useVfs: true, disableExperimentalSEAWarning: true, assets }) });
            this.emitFile({ type: 'asset', fileName: join('LaunchAgents', `${label}.plist`), source: build({ ...agent, Label: label, BundleProgram: `Contents/Helpers/${name}`, AssociatedBundleIdentifiers: [identifier] }) });
        },
    } satisfies Plugin;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { helper };
