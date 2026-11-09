import { constants } from 'node:fs';
import { homedir } from 'node:os';
import { platform, stderr, stdout } from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import type { TaskResult } from '@nx/devkit/internal';
import { Array, Cause, Effect, Equivalence, Exit, FileSystem, Option, Path, Record, Schema, Stream, String, Struct, UndefinedOr } from 'effect';
import { Ini, Zip } from 'effect/encoding';
import { DistributionError, spawned, UNREAL } from '#/plugins/host-packages';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SETTINGS: Partial<Record<NodeJS.Platform, readonly string[]>> = { darwin: ['Library', 'Application Support', 'Epic'], linux: ['.config', 'Epic'] };
const SEPARATOR = /[/\\]/u;

// --- [MODELS] --------------------------------------------------------------------------

const Build = Schema.Struct({
    outputPath: Schema.String,
    descriptor: Schema.String,
    hosts: Schema.Array(Schema.String),
    dependencies: Schema.Array(Schema.String),
    engines: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)),
    args: Schema.optionalKey(Schema.Array(Schema.String)),
});
const Descriptor = Schema.fromJsonString(Schema.Struct({ EngineAssociation: Schema.optionalKey(Schema.String), EngineVersion: Schema.optionalKey(Schema.String), VersionName: Schema.optionalKey(Schema.String) }));
const Launcher = Schema.fromJsonString(Schema.Struct({ InstallationList: Schema.Array(Schema.Struct({ AppName: Schema.String, InstallLocation: Schema.String })) }));
const Registered = Schema.Struct({ Installations: Schema.Record(Schema.String, Schema.String) });
const Built = Schema.fromJsonString(Schema.Struct({ MajorVersion: Schema.Int, MinorVersion: Schema.Int }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const engine = Effect.fnUntraced(function* (descriptor: string, engines: Readonly<Record<string, string>>, workspaceRoot: string) {
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
            Option.map(
                Option.flatMap(named, (key) => Record.get(engines, key)),
                (at) => path.resolve(workspaceRoot, at),
            ),
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
            DistributionError.make({
                subject: descriptor,
                detail: Option.match(named, { onNone: () => 'names no engine, and no engine tree encloses it', onSome: (key) => `names engine "${key}", which the engines option, the engine tree around it, Install.ini, and LauncherInstalled.dat under ${settings} do not list` }),
            }),
    );
    const version = yield* Effect.flatMap(fs.readFileString(path.join(root, 'Engine', 'Build', 'Build.version')), Schema.decodeEffect(Built));
    return { root, version: `${version.MajorVersion}.${version.MinorVersion}` };
});

const archived = Effect.fnUntraced(function* (folder: string, file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const entries = yield* Effect.forEach((yield* fs.readDirectory(folder, { recursive: true })).toSorted(), (at) => Effect.map(fs.stat(path.join(folder, at)), (info) => ({ at, info })));
    yield* Stream.fromIterable(entries.filter(({ info }) => info.type === 'File')).pipe(
        Stream.map(({ at, info }) => Zip.fileStream(at.split(path.sep).join('/'), fs.stream(path.join(folder, at)), { mode: info.mode % (constants.S_IRWXU + constants.S_IRWXG + constants.S_IRWXO + 1) })),
        Stream.pipeThroughChannel(Zip.encode()),
        Stream.run(fs.sink(file)),
    );
});

const run = Effect.fnUntraced(function* (workspaceRoot: string, options: typeof Build.Type) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const host = yield* Effect.fromNullishOr(UNREAL[platform]).pipe(Effect.mapError(() => DistributionError.make({ subject: platform, detail: `is no supported Unreal Engine host; supported: ${Object.keys(UNREAL).join(', ')}` })));
    const absolute = (at: string) => path.resolve(workspaceRoot, at);
    const located = (descriptor: string) => engine(absolute(descriptor), options.engines ?? {}, workspaceRoot);
    const script = (root: string, name: string) => path.join(root, 'Engine', 'Build', 'BatchFiles', name);
    const args = options.args ?? [];
    return yield* options.descriptor.endsWith('.uproject')
        ? Effect.flatMap(located(options.descriptor), ({ root }) => spawned(script(root, 'RunUBT.sh'), ['-TargetType=Editor', host, 'Development', `-Project=${absolute(options.descriptor)}`, '-WaitMutex', ...args], workspaceRoot, { stdout, stderr }))
        : Effect.gen(function* () {
              const name = path.basename(options.descriptor, path.extname(options.descriptor));
              const declared = yield* Effect.flatMap(fs.readFileString(absolute(options.descriptor)), Schema.decodeEffect(Descriptor));
              const engines = Array.dedupeWith(yield* Effect.forEach(options.hosts.length === 0 ? [options.descriptor] : options.hosts, located), Equivalence.mapInput(Equivalence.String, Struct.get('root')));
              const flags = [`-Plugin=${absolute(options.descriptor)}`, ...options.dependencies.map((at) => `-Dependencies=${absolute(at)}`), ...args];
              const folder = absolute(options.outputPath);
              yield* fs.makeDirectory(folder, { recursive: true });
              yield* Effect.forEach(
                  (yield* fs.readDirectory(folder)).filter((file) => file.startsWith(`${name}-`) && path.extname(file) === '.zip'),
                  (file) => fs.remove(path.join(folder, file)),
              );
              const logs = yield* Effect.forEach(
                  engines,
                  Effect.fnUntraced(function* ({ root, version }) {
                      const staged = yield* fs.makeTempDirectoryScoped();
                      const log = yield* spawned(script(root, 'RunUAT.sh'), ['BuildPlugin', `-Package=${staged}`, ...flags], workspaceRoot, { stdout, stderr });
                      yield* archived(staged, path.join(folder, UndefinedOr.match(declared.VersionName, { onUndefined: () => `${name}-${version}-${host}.zip`, onDefined: (versionName) => `${name}-${versionName}-${version}-${host}.zip` })));
                      return log;
                  }, Effect.scoped),
              );
              return logs.join('');
          });
});

const decided = (context: ExecutorContext, inputs: Readonly<Record<string, unknown>>, graph: Pick<TaskGraph, 'dependencies'>) => {
    const depth = (id: string): number => 1 + Math.max(0, ...(graph.dependencies[id] ?? []).filter(({ id: dependency }) => dependency in inputs).map(({ id: dependency }) => depth(dependency)));
    return Effect.reduce(
        Record.keys(inputs).toSorted((left, right) => depth(left) - depth(right)),
        () => Record.empty<string, TaskResult>(),
        (done, id) =>
            (graph.dependencies[id] ?? []).some(({ id: dependency }) => done[dependency]?.success === false)
                ? Effect.succeed({ ...done, [id]: { success: false, status: 'skipped', terminalOutput: '' } satisfies TaskResult })
                : Schema.decodeUnknownEffect(Build)(inputs[id]).pipe(
                      Effect.flatMap((options) => run(context.root, options)),
                      Effect.tapCause((cause) => Effect.sync(() => stderr.write(`${Cause.pretty(cause)}\n`))),
                      Effect.exit,
                      Effect.map(Exit.match({ onFailure: (cause): TaskResult => ({ success: false, terminalOutput: Cause.pretty(cause) }), onSuccess: (terminalOutput): TaskResult => ({ success: true, terminalOutput }) })),
                      Effect.map((result) => ({ ...done, [id]: result })),
                  ),
    ).pipe(Effect.provide(layer));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const executed = Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
    const [project, target] = yield* Effect.all([Effect.fromNullishOr(context.projectName), Effect.fromNullishOr(context.targetName)]);
    const results = yield* decided(context, { [`${project}:${target}`]: options }, { dependencies: {} });
    return { success: Object.values(results).every(({ success }) => success) };
});

const execute = (options: unknown, context: ExecutorContext) => executed(options, context).pipe(Effect.runPromise);

const batch = (taskGraph: TaskGraph, inputs: Record<string, unknown>, _overrides: unknown, context: ExecutorContext) => Effect.runPromise(decided(context, inputs, taskGraph));

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, execute };
