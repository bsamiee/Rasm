import { posix } from 'node:path';
import { type ExecutorContext, runExecutor } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { Array, Cause, Effect, Equivalence, FileSystem, Function, Option, Path, pipe, Record, Result, Schema, Stream, String, Struct } from 'effect';
import { applyEdits, modify } from 'jsonc-parser';
import { run, Words } from '#/executor';
import { ANALYSIS, engine, FOLDERS, Linked, zipped } from '#/host-packages';
import { spawned } from '#/inference';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MARK = /^\u{FEFF}/u;
const INSTALL = { variable: 'UNREAL_ENGINE_ROOT', argument: '--prefix' } as const;
const STORE = {
    path: 170,
    notice: /^\/\/ Copyright (?!.*Epic Games)(?=.*\b\d{4}\b)/u,
    sources: /\.(?:c|cc|cpp|cxx|h|hh|hpp|hxx|inl|ipp|m|mm|cs)$/u,
    built: /^[^/]+\/(?:Binaries|Build|Intermediate|Saved)\//u,
    shipped: /\.(?:exe|msi)$/iu,
    named: /^\w+(?:\.\w+)*$/u,
    folders: new Set(['Config', 'Content', 'Resources', 'Source']),
    party: 'Source/ThirdParty/',
    lines: /\r?\n/u,
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Unreal = Schema.Struct({
    outputPath: Schema.String.annotate({ description: `Workspace-relative folder holding the BuildPlugin packages under ${FOLDERS.build}, the source archive of each engine under ${FOLDERS.pack}, and automation reports under ${FOLDERS.test}` }),
    descriptor: Schema.String.annotate({ description: 'Project .uproject or .uplugin, relative to the workspace root' }),
    hosts: Schema.Array(Schema.String).annotate({ description: '.uproject files loading a plug-in, whose engine associations name each engine it builds, tests, and packs on' }),
    dependencies: Schema.Array(Schema.String).annotate({ description: 'Workspace plug-ins a .uplugin names, passed to BuildPlugin as -Dependencies' }),
    packages: Schema.optionalKey(Linked).annotate({
        description: `Packages installed into folders of a .uplugin before each BuildPlugin run, each target reading the engine root from ${INSTALL.variable} and taking its install folder under ${INSTALL.argument}, absent installing none`,
    }),
    users: Schema.Array(Schema.String).annotate({ description: '.uproject files enabling a plug-in that sits outside their plug-in folders, each receiving the package on install', default: [] }),
    operation: Schema.Literals(['build', 'check', 'test', 'pack', 'install']).annotate({ description: 'What runs: the build, the build under UnrealBuildTool static analysis, the automation tests named after the plug-in, the archives, or the copy into each user project', default: 'build' }),
    __unparsed__: Words,
}).annotate({
    title: 'Unreal',
    description:
        "Builds a .uproject's editor target through UnrealBuildTool, with its Clang static analyzer, unity builds off, and no precompiled headers for a check, or for a .uplugin packages it through AutomationTool's BuildPlugin with strict includes once per engine on this machine its hosts name, skipping a host whose engine is absent, after each package its descriptor maps to a folder installs there under a .gitignore holding *, runs the automation tests named after it through UnrealEditor-Cmd in each host and reads their result from the exported report, zips the tracked sources with those installed packages and the engine's EngineVersion once per engine after the Fab store checks pass, or copies the package into the Plugins folder of each user project",
});
const Report = Schema.fromJsonString(Schema.Struct({ tests: Schema.NonEmptyArray(Schema.Struct({ fullTestPath: Schema.String, state: Schema.String })) }));
const Listed = Schema.fromJsonString(Schema.Struct({ FabURL: Schema.optionalKey(Schema.String), Modules: Schema.Array(Schema.Struct({ Name: Schema.String, PlatformAllowList: Schema.optionalKey(Schema.Array(Schema.String)), PlatformDenyList: Schema.optionalKey(Schema.Array(Schema.String)) })) }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ outputPath, descriptor, hosts, dependencies, packages, users, operation, __unparsed__: words = [] }: typeof Unreal.Type, context: ExecutorContext) {
    const workspaceRoot = context.root;
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const absolute = (at: string) => path.resolve(workspaceRoot, at);
    const name = path.basename(descriptor, path.extname(descriptor));
    const editor = path.extname(descriptor) === '.uproject';
    const placed = (folder: keyof typeof FOLDERS, ...rest: readonly string[]) => path.join(absolute(outputPath), FOLDERS[folder], ...rest);
    const tool = (command: string, given: readonly string[], named: readonly (typeof operation)[]) => Effect.asVoid(spawned([command, ...given, ...(named.includes(operation) ? words : [])], { cwd: workspaceRoot }));
    const located = (at: string) => Effect.map(engine(absolute(at)), (found) => ({ ...found, project: absolute(at) }));
    const [loading, absent] = yield* Effect.partition(editor ? [descriptor] : hosts, (at) => Effect.mapError(located(at), (error) => ({ at, error })));
    yield* Effect.forEach(absent, ({ at, error }) => (editor ? Effect.fail(error) : Effect.logWarning(`Skipped host ${at}: ${error.message}`)), { discard: true });
    const engines = yield* pipe(
        Array.dedupeWith(loading, Equivalence.mapInput(Equivalence.String, Struct.get('root'))),
        Option.liftPredicate(Array.isReadonlyArrayNonEmpty),
        Effect.fromOption,
        Effect.mapError((cause) => new Cause.UnknownError(cause, `${descriptor} is loaded by no .uproject whose EngineAssociation names an engine on this machine`)),
    );
    type Loading = (typeof loading)[number];
    const edited = ({ scripts, platform, project: at }: Loading, analysis: readonly string[], named: readonly (typeof operation)[]) => tool(path.join(scripts, 'RunUBT.sh'), ['-TargetType=Editor', platform, 'Development', `-Project=${at}`, '-WaitMutex', ...analysis], named);
    const packaged = ({ scripts, version }: Loading) => tool(path.join(scripts, 'RunUAT.sh'), ['BuildPlugin', `-Plugin=${absolute(descriptor)}`, `-Package=${placed('build', version, name)}`, '-StrictIncludes', ...dependencies.map((at) => `-Dependencies=${absolute(at)}`)], ['build']);
    const linked = ({ platform, root }: Loading) =>
        Effect.forEach(
            Record.toEntries(packages ?? {}),
            Effect.fnUntraced(function* ([folder, target]) {
                const prefix = path.join(absolute(path.dirname(descriptor)), folder, platform);
                yield* fs.remove(prefix, { recursive: true, force: true });
                const results = yield* Effect.tryPromise({
                    try: () => runExecutor<{ readonly success: boolean }>(target, { args: [INSTALL.argument, prefix], env: { [INSTALL.variable]: root } }, context),
                    catch: (cause) => new Cause.UnknownError(cause, `${target.project} does not run ${target.target}`),
                });
                yield* Effect.filterOrFail(
                    Stream.runCollect(Stream.fromAsyncIterable(results, (cause) => new Cause.UnknownError(cause, `${target.project} fails ${target.target}`))),
                    Array.every(Struct.get('success')),
                    (cause) => new Cause.UnknownError(cause, `${target.project}: ${target.target} ended without success`),
                );
                yield* fs.writeFileString(path.join(prefix, '.gitignore'), '*\n');
            }),
            { discard: true },
        );
    const tested = Effect.fnUntraced(function* (each: Loading) {
        const report = placed('test', path.basename(each.project, path.extname(each.project)));
        yield* edited(each, [], []);
        yield* fs.remove(report, { recursive: true, force: true });
        const exited = yield* Effect.result(tool(path.join(each.binaries, 'UnrealEditor-Cmd'), [each.project, `-ExecCmds=Automation RunTests ^${name}.;Quit`, '-nullrhi', '-unattended', '-nosplash', `-ReportExportPath=${report}`], ['test']));
        const { tests } = yield* Effect.mapError(
            Effect.flatMap(fs.readFileString(path.join(report, 'index.json')), (text) => Schema.decodeEffect(Report)(text.replace(MARK, ''))),
            (unread) => Result.match(exited, { onFailure: Function.identity, onSuccess: () => new Cause.UnknownError(unread, `${report} holds no automation test named ${name}`) }),
        );
        yield* Array.match(
            tests.filter(({ state }) => state !== 'Success'),
            { onEmpty: () => Effect.fromResult(exited), onNonEmpty: (failed) => Effect.fail(new Cause.UnknownError(failed, `${name} automation tests ended without success: ${failed.map(({ fullTestPath, state }) => [fullTestPath, state].join(' ')).join(', ')}`)) },
        );
    });
    const unnoticed = (file: string) => Effect.map(fs.readFileString(absolute(file)), (code) => !STORE.notice.test(code));
    const stored = Effect.fnUntraced(function* (text: string, tracked: readonly string[], root: string, sources: readonly string[]) {
        const { FabURL, Modules } = yield* Effect.mapError(Schema.decodeEffect(Listed)(text), (cause) => new Cause.UnknownError(cause, `${descriptor} lists no Modules`));
        const opened = yield* Effect.filter(
            tracked.filter((file) => STORE.sources.test(file)),
            unnoticed,
        );
        const filtered = yield* Effect.map(Effect.option(fs.readFileString(path.join(root, 'Config', 'FilterPlugin.ini'))), Option.match({ onNone: Array.empty<string>, onSome: (ini) => ini.split(STORE.lines).map(String.trim) }));
        const folders = Array.dedupe(sources.flatMap((file) => file.split(path.sep).slice(1, -1).slice(0, 1)));
        const findings = [
            ...Array.match(Modules, { onEmpty: () => ['it declares no C++ module (Fab 4.3.6.1.c)'], onNonEmpty: () => [] }),
            ...Modules.filter(({ PlatformAllowList, PlatformDenyList }) => PlatformAllowList === undefined && PlatformDenyList === undefined).map(({ Name }) => `module ${Name} lists no platforms (Fab 4.3.6.b)`),
            ...(FabURL === undefined ? ['it states no FabURL (Fab 4.3.6.c)'] : []),
            ...dependencies.map((at) => `it depends on workspace plug-in ${at}, where Fab takes engine plug-ins alone (Fab 4.3.6.d)`),
            ...opened.map((file) => `${file} opens with no // Copyright line naming its publisher and a year other than Epic Games (Fab 4.3.6.1.b)`),
            ...sources.filter((file) => STORE.shipped.test(file)).map((file) => `${file} is an .exe or .msi (Fab 4.3.6.1.e)`),
            ...Array.dedupe(sources.filter((file) => !file.startsWith(path.join(name, STORE.party))).flatMap((file) => file.split(path.sep)))
                .filter((segment) => !STORE.named.test(segment))
                .map((segment) => `${segment} holds a character other than an English letter, digit, or underscore (Fab 4.3.7.1.c)`),
            ...sources.filter((file) => STORE.built.test(file)).map((file) => `${file} is build output (Fab 4.3.7.3.a)`),
            ...folders.filter((folder) => !(STORE.folders.has(folder) || filtered.includes(`/${folder}/...`))).map((folder) => `${folder} is no Config, Content, Resources, or Source folder and Config/FilterPlugin.ini lists no /${folder}/... (Fab 4.3.7.3.b)`),
            ...sources.filter((file) => file.length > STORE.path).map((file) => `${file} has path length ${file.length}, past ${STORE.path} (Fab 4.3.7.3.c)`),
            ...Record.keys(packages ?? {})
                .filter((folder) => !`${folder}/`.startsWith(STORE.party))
                .map((folder) => `installed package folder ${folder} sits outside ${STORE.party} (Fab 4.3.7.3.d)`),
        ];
        yield* Array.match(findings, { onEmpty: () => Effect.void, onNonEmpty: (failed) => Effect.fail(new Cause.UnknownError(failed, `${descriptor} fails the Fab store checks: ${failed.join('; ')}`)) });
    });
    const archived = Effect.fnUntraced(function* (text: string, tracked: readonly string[], { version, platform }: Loading) {
        const stage = yield* fs.makeTempDirectoryScoped();
        const root = path.join(stage, name);
        const owned = tracked.map((file) => path.join(root, path.relative(path.dirname(descriptor), file)));
        yield* Effect.forEach(Record.keys(packages ?? {}), (folder) => Effect.andThen(fs.copy(placed('build', version, name, folder), path.join(root, folder)), fs.remove(path.join(root, folder, platform, '.gitignore'), { force: true })), { discard: true });
        yield* Effect.forEach(Array.zip(tracked, owned), ([file, to]) => Effect.andThen(fs.makeDirectory(path.dirname(to), { recursive: true }), fs.copyFile(absolute(file), to)), { discard: true });
        yield* fs.writeFileString(path.join(root, path.basename(descriptor)), applyEdits(text, modify(text, ['EngineVersion'], `${version}.0`, {})));
        const sources = yield* fs.readDirectory(stage, { recursive: true });
        yield* stored(text, tracked, root, sources);
        yield* zipped(stage, sources, placed('pack', `${name}-${version}.zip`));
    }, Effect.scoped);
    const installed = Effect.fnUntraced(function* (at: string) {
        const { version, project } = yield* located(at);
        const target = path.join(path.dirname(project), 'Plugins', name);
        yield* fs.remove(target, { recursive: true, force: true });
        yield* fs.copy(placed('build', version, name), target);
    });
    const compiled = (analysis: readonly string[]) => (editor ? Effect.forEach(loading, (each) => edited(each, analysis, ['build', 'check']), { discard: true }) : Effect.forEach(engines, (each) => Effect.andThen(linked(each), packaged(each)), { discard: true }));
    const packed = Effect.gen(function* () {
        const [text, tracked] = yield* Effect.all([fs.readFileString(absolute(descriptor)), Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [posix.join(posix.dirname(descriptor), '**')])), fs.remove(placed('pack'), { recursive: true, force: true })]);
        yield* fs.makeDirectory(placed('pack'), { recursive: true });
        yield* Effect.forEach(engines, (each) => archived(text, tracked, each), { discard: true });
    });
    yield* {
        build: compiled([]),
        check: compiled(ANALYSIS),
        test: Effect.forEach(loading, tested, { discard: true }),
        pack: packed,
        install: Effect.forEach(users, installed, { discard: true }),
    }[operation];
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Unreal };
