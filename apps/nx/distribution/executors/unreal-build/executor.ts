import { homedir } from 'node:os';
import { platform, stderr, stdout } from 'node:process';
import { executor } from '@rasm/nx-devkit/executor';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Equivalence, FileSystem, Option, Path, Record, Schema, String, Struct, UndefinedOr } from 'effect';
import { Ini } from 'effect/encoding';
import { UNREAL, zipped } from '#/host-packages';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SETTINGS: Partial<Record<NodeJS.Platform, readonly string[]>> = { darwin: ['Library', 'Application Support', 'Epic'], linux: ['.config', 'Epic'] };
const SEPARATOR = /[/\\]/u;

// --- [MODELS] --------------------------------------------------------------------------

const Build = Schema.Struct({
    outputPath: Schema.String.annotate({ description: "Workspace-relative folder a plug-in's archives are written to" }),
    descriptor: Schema.String.annotate({ description: 'Project .uproject or .uplugin, relative to the workspace root' }),
    hosts: Schema.Array(Schema.String).annotate({ description: '.uproject files building a plug-in, whose engine associations name each engine BuildPlugin runs on; empty for a plug-in no project builds, which takes its EngineVersion' }),
    dependencies: Schema.Array(Schema.String).annotate({ description: 'Workspace plug-ins a .uplugin names, passed to BuildPlugin as -Dependencies' }),
    args: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments appended to the UnrealBuildTool or AutomationTool command' }),
}).annotate({ title: 'Unreal build', description: "Builds a .uproject's editor target through UnrealBuildTool, or packages a .uplugin through AutomationTool's BuildPlugin once per engine its hosts name and archives each package" });
const Descriptor = Schema.fromJsonString(Schema.Struct({ EngineAssociation: Schema.optionalKey(Schema.String), EngineVersion: Schema.optionalKey(Schema.String), VersionName: Schema.optionalKey(Schema.String) }));
const Launcher = Schema.fromJsonString(Schema.Struct({ InstallationList: Schema.Array(Schema.Struct({ AppName: Schema.String, InstallLocation: Schema.String })) }));
const Registered = Schema.Struct({ Installations: Schema.Record(Schema.String, Schema.String) });
const Built = Schema.fromJsonString(Schema.Struct({ MajorVersion: Schema.Int, MinorVersion: Schema.Int }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const engine = Effect.fnUntraced(function* (descriptor: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const settings = path.join(homedir(), ...(SETTINGS[platform] ?? []));
    const declared = yield* Effect.flatMap(fs.readFileString(descriptor), Schema.decodeEffect(Descriptor));
    const named = descriptor.endsWith('.uproject') ? Option.filter(Option.fromUndefinedOr(declared.EngineAssociation), String.isNonEmpty) : Option.map(Option.fromUndefinedOr(declared.EngineVersion), (engineVersion) => Array.take(String.split(engineVersion, '.'), 2).join('.'));
    const read = (at: readonly string[]) => Effect.option(fs.readFileString(path.join(settings, ...at)));
    const [registered, launched] = yield* Effect.all([read(['UnrealEngine', 'Install.ini']), read(['UnrealEngineLauncher', 'LauncherInstalled.dat'])]);
    const ancestors = Array.unfold(path.dirname(descriptor), (at) => (path.dirname(at) === at ? Option.none() : Option.some([at, path.dirname(at)] as const)));
    const native = yield* Effect.filter(ancestors, (at) => fs.exists(path.join(at, 'Engine', 'Build', 'Build.version')));
    const root = yield* Effect.fromOption(
        Option.firstSomeOf([
            Option.flatMap(named, (key) => Option.liftPredicate(path.resolve(path.dirname(descriptor), key), () => SEPARATOR.test(key))),
            Option.match(named, { onNone: () => Array.head(native), onSome: () => Option.none() }),
            Option.flatMap(Option.all([named, Option.flatMap(registered, (text) => Schema.decodeUnknownOption(Registered)(Ini.parse(text)))]), ([key, { Installations }]) => Record.get(Installations, key)),
            Option.flatMap(Option.all([named, Option.flatMap(launched, (text) => Schema.decodeOption(Launcher)(text))]), ([key, { InstallationList }]) =>
                Option.map(
                    Array.findFirst(InstallationList, ({ AppName }) => AppName === `UE_${key}`),
                    ({ InstallLocation }) => InstallLocation,
                ),
            ),
        ]),
        () =>
            ToolError.make({
                subject: descriptor,
                detail: Option.match(named, { onNone: () => 'names no engine, and no engine tree encloses it', onSome: (key) => `names engine "${key}", which the engine tree around it, Install.ini, and LauncherInstalled.dat under ${settings} do not list` }),
            }),
    );
    const version = yield* Effect.flatMap(fs.readFileString(path.join(root, 'Engine', 'Build', 'Build.version')), Schema.decodeEffect(Built));
    return { root, version: `${version.MajorVersion}.${version.MinorVersion}` };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Build,
    Effect.fnUntraced(function* (options, { root: workspaceRoot }) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const host = yield* Effect.mapError(Effect.fromNullishOr(UNREAL[platform]), ToolError.of(platform, `is no supported Unreal Engine host; supported: ${Object.keys(UNREAL).join(', ')}`));
        const absolute = (at: string) => path.resolve(workspaceRoot, at);
        const script = (at: string, file: string) => path.join(at, 'Engine', 'Build', 'BatchFiles', file);
        const run = (command: string, args: readonly string[]) => printed(command, [...args, ...(options.args ?? [])], { cwd: workspaceRoot, echo: { stdout, stderr } });
        if (options.descriptor.endsWith('.uproject')) {
            const { root } = yield* engine(absolute(options.descriptor));
            yield* run(script(root, 'RunUBT.sh'), ['-TargetType=Editor', host, 'Development', `-Project=${absolute(options.descriptor)}`, '-WaitMutex']);
            return true;
        }
        const name = path.basename(options.descriptor, path.extname(options.descriptor));
        const declared = yield* Effect.flatMap(fs.readFileString(absolute(options.descriptor)), Schema.decodeEffect(Descriptor));
        const engines = Array.dedupeWith(yield* Effect.forEach(options.hosts.length === 0 ? [options.descriptor] : options.hosts, (descriptor) => engine(absolute(descriptor))), Equivalence.mapInput(Equivalence.String, Struct.get('root')));
        const folder = absolute(options.outputPath);
        yield* fs.makeDirectory(folder, { recursive: true });
        yield* Effect.forEach(
            (yield* fs.readDirectory(folder)).filter((file) => file.startsWith(`${name}-`) && path.extname(file) === '.zip'),
            (file) => fs.remove(path.join(folder, file)),
        );
        yield* Effect.forEach(
            engines,
            Effect.fnUntraced(function* ({ root, version }) {
                const staged = yield* fs.makeTempDirectoryScoped();
                yield* run(script(root, 'RunUAT.sh'), ['BuildPlugin', `-Package=${staged}`, `-Plugin=${absolute(options.descriptor)}`, ...options.dependencies.map((at) => `-Dependencies=${absolute(at)}`)]);
                yield* zipped(staged, yield* fs.readDirectory(staged, { recursive: true }), path.join(folder, UndefinedOr.match(declared.VersionName, { onUndefined: () => `${name}-${version}-${host}.zip`, onDefined: (versionName) => `${name}-${versionName}-${version}-${host}.zip` })));
            }, Effect.scoped),
        );
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
