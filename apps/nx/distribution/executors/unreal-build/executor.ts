import { stderr, stdout } from 'node:process';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { executor } from '@rasm/nx-devkit/executor';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Equivalence, FileSystem, Function, Option, Path, pipe, Result, Schema, Struct } from 'effect';
import { applyEdits, modify } from 'jsonc-parser';
import { engine, zipped } from '#/host-packages';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ANALYSIS = ['-StaticAnalyzer=Default', '-StaticAnalyzerProjectOnly', '-DisableUnity', '-NoPCH'] as const;
const MARK = /^\u{FEFF}/u;
const FOLDERS = { package: 'package', archive: 'archive', report: 'report' } as const;
const STORE = { path: 170, notice: /^\/\/ Copyright /u, sources: /\.(?:cpp|h|cs)$/u, built: /(?:^|\/)(?:Binaries|Build|Intermediate|Saved)\//u } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Unreal = Schema.Struct({
    outputPath: Schema.String.annotate({ description: 'Workspace-relative folder holding the BuildPlugin packages under package, their archives under archive, and automation reports under report' }),
    descriptor: Schema.String.annotate({ description: 'Project .uproject or .uplugin, relative to the workspace root' }),
    hosts: Schema.Array(Schema.String).annotate({ description: '.uproject files loading a plug-in, whose engine associations name each engine it builds, tests, and packs on' }),
    dependencies: Schema.Array(Schema.String).annotate({ description: 'Workspace plug-ins a .uplugin names, passed to BuildPlugin as -Dependencies' }),
    users: Schema.Array(Schema.String).annotate({ description: '.uproject files enabling a plug-in that sits outside their plug-in folders, each receiving the package on install', default: [] }),
    operation: Schema.Literals(['build', 'check', 'test', 'pack', 'install']).annotate({ description: 'What runs: the build, the build under UnrealBuildTool static analysis, the automation tests named after the plug-in, the archives, or the copy into each user project', default: 'build' }),
    args: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments appended to the UnrealBuildTool, AutomationTool, or UnrealEditor-Cmd command' }),
}).annotate({
    title: 'Unreal',
    description:
        "Builds a .uproject's editor target through UnrealBuildTool, with its Clang static analyzer, unity builds off, and no precompiled headers for a check, or for a .uplugin packages it through AutomationTool's BuildPlugin with strict includes once per engine its hosts name, runs the automation tests named after it through UnrealEditor-Cmd in each host and reads their result from the exported report, zips each package and the tracked sources with the engine's EngineVersion after the store checks pass, or copies the package into the Plugins folder of each user project",
});
const Report = Schema.fromJsonString(Schema.Struct({ tests: Schema.NonEmptyArray(Schema.Struct({ fullTestPath: Schema.String, state: Schema.String })) }));
const Listed = Schema.fromJsonString(Schema.Struct({ Modules: Schema.Array(Schema.Struct({ Name: Schema.String, PlatformAllowList: Schema.optionalKey(Schema.Array(Schema.String)), PlatformDenyList: Schema.optionalKey(Schema.Array(Schema.String)) })) }));

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
        const loading = yield* Effect.forEach(path.extname(descriptor) === '.uproject' ? [descriptor] : hosts, located);
        const engines = yield* pipe(
            Array.dedupeWith(loading, Equivalence.mapInput(Equivalence.String, Struct.get('root'))),
            Option.liftPredicate(Array.isReadonlyArrayNonEmpty),
            Effect.fromOption,
            Effect.mapError(ToolError.of(descriptor, 'is loaded by no .uproject, whose EngineAssociation names the engine it builds on')),
        );
        type Loading = (typeof loading)[number];
        const edited = ({ scripts, platform, project }: Loading, analysis: readonly string[]) => run(path.join(scripts, 'RunUBT.sh'), ['-TargetType=Editor', platform, 'Development', `-Project=${project}`, '-WaitMutex', ...analysis]);
        const packaged = ({ scripts, version }: Loading) => run(path.join(scripts, 'RunUAT.sh'), ['BuildPlugin', `-Plugin=${absolute(descriptor)}`, `-Package=${placed('package', version, name)}`, '-StrictIncludes', ...dependencies.map((at) => `-Dependencies=${absolute(at)}`)]);
        const tested = Effect.fnUntraced(function* (each: Loading) {
            const report = placed('report', path.basename(each.project, path.extname(each.project)));
            yield* edited(each, []);
            yield* fs.remove(report, { recursive: true, force: true });
            const exited = yield* Effect.result(run(path.join(each.binaries, 'UnrealEditor-Cmd'), [each.project, `-ExecCmds=Automation RunTests ${name};Quit`, '-nullrhi', '-unattended', '-nosplash', `-ReportExportPath=${report}`]));
            const { tests } = yield* Effect.mapError(
                Effect.flatMap(fs.readFileString(path.join(report, 'index.json')), (text) => Schema.decodeEffect(Report)(text.replace(MARK, ''))),
                (unread) => Result.match(exited, { onFailure: Function.identity, onSuccess: () => ToolError.of(report, `holds no automation test named ${name}`)(unread) }),
            );
            yield* Array.match(
                tests.filter(({ state }) => state !== 'Success'),
                { onEmpty: () => Effect.fromResult(exited), onNonEmpty: (failed) => ToolError.make({ subject: name, detail: `automation tests ended without success: ${failed.map(({ fullTestPath, state }) => [fullTestPath, state].join(' ')).join(', ')}` }) },
            );
        });
        const unnoticed = (file: string) => Effect.map(fs.readFileString(absolute(file)), (code) => !STORE.notice.test(code));
        const stored = Effect.fnUntraced(function* (text: string, tracked: readonly string[], sources: readonly string[]) {
            const { Modules } = yield* Effect.mapError(Schema.decodeEffect(Listed)(text), ToolError.of(descriptor, 'lists no Modules'));
            const opened = yield* Effect.filter(
                tracked.filter((file) => STORE.sources.test(file)),
                unnoticed,
            );
            const refused = [
                ...sources.filter((file) => file.length > STORE.path).map((file) => `${file} has path length ${file.length}, past ${STORE.path}`),
                ...sources.filter((file) => STORE.built.test(file)).map((file) => `${file} is build output`),
                ...opened.map((file) => `${file} opens with no copyright notice`),
                ...Modules.filter(({ PlatformAllowList, PlatformDenyList }) => PlatformAllowList === undefined && PlatformDenyList === undefined).map(({ Name }) => `module ${Name} lists no platforms`),
            ];
            yield* Array.match(refused, { onEmpty: () => Effect.void, onNonEmpty: (findings) => ToolError.make({ subject: descriptor, detail: `fails the store checks: ${findings.join('; ')}` }) });
        });
        const archived = Effect.fnUntraced(function* ({ version, platform }: Loading) {
            const [stage, text, tracked] = yield* Effect.all([fs.makeTempDirectoryScoped(), fs.readFileString(absolute(descriptor)), Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [path.join(path.dirname(descriptor), '**')]))]);
            const sources = tracked.map((file) => path.join(name, path.relative(path.dirname(descriptor), file)));
            yield* stored(text, tracked, sources);
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
