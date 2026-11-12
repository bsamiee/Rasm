import { stderr, stdout } from 'node:process';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { executor } from '@rasm/nx-devkit/executor';
import { printed } from '@rasm/nx-devkit/process';
import { Array, Effect, Equivalence, FileSystem, Path, Schema, Struct } from 'effect';
import { applyEdits, modify } from 'jsonc-parser';
import { engine, zipped } from '#/host-packages';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ANALYSIS = ['-StaticAnalyzer=Default', '-StaticAnalyzerProjectOnly', '-DisableUnity'] as const;
const FOLDERS = { package: 'package', archive: 'archive', report: 'report' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Unreal = Schema.Struct({
    outputPath: Schema.String.annotate({ description: 'Workspace-relative folder holding the BuildPlugin packages under package, their archives under archive, and automation reports under report' }),
    descriptor: Schema.String.annotate({ description: 'Project .uproject or .uplugin, relative to the workspace root' }),
    hosts: Schema.Array(Schema.String).annotate({ description: '.uproject files loading a plug-in, whose engine associations name each engine it builds and tests on; empty for a plug-in no project loads, which takes its EngineVersion' }),
    dependencies: Schema.Array(Schema.String).annotate({ description: 'Workspace plug-ins a .uplugin names, passed to BuildPlugin as -Dependencies' }),
    users: Schema.Array(Schema.String).annotate({ description: '.uproject files enabling a plug-in that sits outside their plug-in folders, each receiving the package on install', default: [] }),
    operation: Schema.Literals(['build', 'check', 'test', 'pack', 'install']).annotate({ description: 'What runs: the build, the build under UnrealBuildTool static analysis, the automation tests named after the plug-in, the archives, or the copy into each user project', default: 'build' }),
    args: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments appended to the UnrealBuildTool, AutomationTool, or UnrealEditor-Cmd command' }),
}).annotate({
    title: 'Unreal',
    description:
        "Builds a .uproject's editor target through UnrealBuildTool, with its Clang static analyzer for a check, or for a .uplugin packages it through AutomationTool's BuildPlugin once per engine its hosts name, runs the automation tests named after it through UnrealEditor-Cmd in each host, zips each package and the tracked sources with the engine's EngineVersion, or copies the package into the Plugins folder of each user project",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Unreal,
    Effect.fnUntraced(function* ({ outputPath, descriptor, hosts, dependencies, users, operation, args = [] }, { root: workspaceRoot }) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const absolute = (at: string) => path.resolve(workspaceRoot, at);
        const name = path.basename(descriptor, path.extname(descriptor));
        const placed = (folder: keyof typeof FOLDERS, ...rest: readonly string[]) => path.join(absolute(outputPath), FOLDERS[folder], ...rest);
        const run = (command: string, given: readonly string[]) => printed(command, [...given, ...args], { cwd: workspaceRoot, echo: { stdout, stderr } });
        const located = (at: string) => Effect.map(engine(absolute(at)), (found) => ({ ...found, project: absolute(at) }));
        const loading = yield* Effect.forEach(hosts.length === 0 ? [descriptor] : hosts, located);
        const engines = Array.dedupeWith(loading, Equivalence.mapInput(Equivalence.String, Struct.get('root')));
        type Loading = (typeof loading)[number];
        const edited = ({ scripts, platform, project }: Loading, analysis: readonly string[]) => run(path.join(scripts, 'RunUBT.sh'), ['-TargetType=Editor', platform, 'Development', `-Project=${project}`, '-WaitMutex', ...analysis]);
        const packaged = ({ scripts, version }: Loading) => run(path.join(scripts, 'RunUAT.sh'), ['BuildPlugin', `-Plugin=${absolute(descriptor)}`, `-Package=${placed('package', version, name)}`, ...dependencies.map((at) => `-Dependencies=${absolute(at)}`)]);
        const tested = (each: Loading) =>
            Effect.andThen(edited(each, []), run(path.join(each.binaries, 'UnrealEditor-Cmd'), [each.project, `-ExecCmds=Automation RunTests ${name};Quit`, '-nullrhi', '-unattended', '-nosplash', `-ReportExportPath=${placed('report', path.basename(each.project, path.extname(each.project)))}`]));
        const archived = Effect.fnUntraced(function* ({ version, platform }: Loading) {
            const [stage, text, tracked] = yield* Effect.all([fs.makeTempDirectoryScoped(), fs.readFileString(absolute(descriptor)), Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [path.join(path.dirname(descriptor), '**')]))]);
            const sources = tracked.map((file) => path.join(name, path.relative(path.dirname(descriptor), file)));
            yield* Effect.forEach(Array.zip(tracked, sources), ([file, to]) => Effect.andThen(fs.makeDirectory(path.dirname(path.join(stage, to)), { recursive: true }), fs.copyFile(absolute(file), path.join(stage, to))), { discard: true });
            yield* fs.writeFileString(path.join(stage, name, path.basename(descriptor)), applyEdits(text, modify(text, ['EngineVersion'], `${version}.0`, {})));
            yield* zipped(stage, sources, placed('archive', `${name}-${version}.zip`));
            yield* zipped(placed('package', version), yield* fs.readDirectory(placed('package', version), { recursive: true }), placed('archive', `${name}-${version}-${platform}.zip`));
        }, Effect.scoped);
        const installed = Effect.fnUntraced(function* (at: string) {
            const { version, project } = yield* located(at);
            const target = path.join(path.dirname(project), 'Plugins', name);
            yield* fs.remove(target, { recursive: true, force: true });
            yield* fs.copy(placed('package', version, name), target);
        });
        const compiled = (analysis: readonly string[]) => (path.extname(descriptor) === '.uproject' ? Effect.forEach(loading, (each) => edited(each, analysis), { discard: true }) : Effect.forEach(engines, packaged, { discard: true }));
        const cleared = Effect.andThen(fs.remove(placed('archive'), { recursive: true, force: true }), fs.makeDirectory(placed('archive'), { recursive: true }));
        yield* {
            build: compiled([]),
            check: compiled(ANALYSIS),
            test: Effect.forEach(loading, tested, { discard: true }),
            pack: Effect.andThen(cleared, Effect.forEach(engines, archived, { discard: true })),
            install: Effect.forEach(users, installed, { discard: true }),
        }[operation];
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
