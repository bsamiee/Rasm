import { constants } from 'node:fs';
import { homedir } from 'node:os';
import { platform, stderr, stdout } from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import type { TaskResult } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Record, Result, Schema, Stream } from 'effect';
import { Ini, Zip } from 'effect/encoding';
import { ChildProcess } from 'effect/process';
import { DistributionError, UNREAL } from '#/plugins/host-packages';

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

const ran = (program: string, args: readonly string[], cwd: string) =>
    Effect.scoped(
        Effect.gen(function* () {
            const handle = yield* ChildProcess.make(program, args, { cwd, extendEnv: true, stdin: 'ignore' });
            const echoed = (stream: typeof handle.stdout, sink: NodeJS.WritableStream) => Stream.mkString(Stream.tap(Stream.decodeText(stream), (chunk) => Effect.sync(() => sink.write(chunk))));
            const [out, err, code] = yield* Effect.all([echoed(handle.stdout, stdout), echoed(handle.stderr, stderr), handle.exitCode], { concurrency: 'unbounded' });
            return code === 0 ? `${out}${err}` : yield* DistributionError.make({ subject: [program, ...args].join(' '), detail: `exited ${code}` });
        }),
    ).pipe(Effect.catchTag('PlatformError', (error) => DistributionError.make({ subject: program, detail: error.message })));

const engine = Effect.fnUntraced(function* (descriptor: string, engines: Readonly<Record<string, string>>, workspaceRoot: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const settings = path.join(homedir(), ...(SETTINGS[platform] ?? []));
    const held = yield* Effect.flatMap(fs.readFileString(descriptor), Schema.decodeEffect(Descriptor));
    const named = descriptor.endsWith('.uproject') ? (held.EngineAssociation ?? '') : (held.EngineVersion ?? '').split('.').slice(0, 2).join('.');
    const read = (at: readonly string[]) => Effect.option(fs.readFileString(path.join(settings, ...at)));
    const [registered, launched] = yield* Effect.all([read(['UnrealEngine', 'Install.ini']), read(['UnrealEngineLauncher', 'LauncherInstalled.dat'])]);
    const ancestors = Array.unfold(path.dirname(descriptor), (at) => (path.dirname(at) === at ? Option.none() : Option.some([at, path.dirname(at)] as const)));
    const native = yield* Effect.filter(ancestors, (at) => fs.exists(path.join(at, 'Engine', 'Build', 'Build.version')));
    const root = yield* Effect.fromOption(
        Option.firstSomeOf([
            Option.map(Record.get(engines, named), (at) => path.resolve(workspaceRoot, at)),
            Option.liftPredicate(path.resolve(path.dirname(descriptor), named), () => SEPARATOR.test(named)),
            Option.flatMap(
                Option.liftPredicate(named, (association) => association.length === 0),
                () => Array.head(native),
            ),
            Option.flatMap(
                Option.flatMap(registered, (text) => Result.getSuccess(Schema.decodeUnknownResult(Registered)(Ini.parse(text)))),
                ({ Installations }) => Record.get(Installations, named),
            ),
            Option.flatMap(
                Option.flatMap(launched, (text) => Result.getSuccess(Schema.decodeResult(Launcher)(text))),
                ({ InstallationList }) =>
                    Option.map(
                        Array.findFirst(InstallationList, ({ AppName }) => AppName === `UE_${named}`),
                        ({ InstallLocation }) => InstallLocation,
                    ),
            ),
        ]),
        () => DistributionError.make({ subject: descriptor, detail: `names engine "${named}", which the engines option, the engine tree around it, Install.ini, and LauncherInstalled.dat under ${settings} do not list` }),
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
    const host = yield* Effect.fromNullishOr(UNREAL[platform]).pipe(Effect.mapError(() => DistributionError.make({ subject: platform, detail: `runs no Unreal Engine build tools this plugin drives; supported: ${Object.keys(UNREAL).join(', ')}` })));
    const absolute = (at: string) => path.resolve(workspaceRoot, at);
    const located = (descriptor: string) => engine(absolute(descriptor), options.engines ?? {}, workspaceRoot);
    const script = (root: string, name: string) => path.join(root, 'Engine', 'Build', 'BatchFiles', name);
    const args = options.args ?? [];
    return yield* options.descriptor.endsWith('.uproject')
        ? Effect.flatMap(located(options.descriptor), ({ root }) => ran(script(root, 'RunUBT.sh'), ['-TargetType=Editor', host, 'Development', `-Project=${absolute(options.descriptor)}`, '-WaitMutex', ...args], workspaceRoot))
        : Effect.gen(function* () {
              const name = path.basename(options.descriptor, path.extname(options.descriptor));
              const held = yield* Effect.flatMap(fs.readFileString(absolute(options.descriptor)), Schema.decodeEffect(Descriptor));
              const engines = Array.dedupeWith(yield* Effect.forEach(options.hosts.length === 0 ? [options.descriptor] : options.hosts, located), (left, right) => left.root === right.root);
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
                      const log = yield* ran(script(root, 'RunUAT.sh'), ['BuildPlugin', `-Package=${staged}`, ...flags], workspaceRoot);
                      yield* archived(staged, path.join(folder, `${name}-${held.VersionName ?? ''}-${version}-${host}.zip`));
                      return log;
                  }, Effect.scoped),
              );
              return logs.join('');
          });
});

const decided = (context: ExecutorContext, inputs: Readonly<Record<string, unknown>>, graph: Pick<TaskGraph, 'dependencies'>) => {
    const depth = (id: string): number => 1 + Math.max(0, ...(graph.dependencies[id] ?? []).filter(({ id: held }) => held in inputs).map(({ id: held }) => depth(held)));
    return Effect.reduce(
        Record.keys(inputs).toSorted((left, right) => depth(left) - depth(right)),
        () => Record.empty<string, TaskResult>(),
        (done, id) =>
            (graph.dependencies[id] ?? []).some(({ id: held }) => done[held]?.success === false)
                ? Effect.succeed({ ...done, [id]: { success: false, status: 'skipped', terminalOutput: '' } satisfies TaskResult })
                : Schema.decodeUnknownEffect(Build)(inputs[id]).pipe(
                      Effect.flatMap((options) => run(context.root, options)),
                      Effect.match({
                          onFailure: (error): TaskResult => {
                              stderr.write(`${error.message}\n`);
                              return { success: false, terminalOutput: error.message };
                          },
                          onSuccess: (terminalOutput): TaskResult => ({ success: true, terminalOutput }),
                      }),
                      Effect.map((result) => ({ ...done, [id]: result })),
                  ),
    ).pipe(Effect.provide(layer));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) => {
    const id = `${context.projectName}:${context.targetName}`;
    return Effect.runPromise(Effect.map(decided(context, { [id]: options }, { dependencies: {} }), (results) => ({ success: Record.values(results).every(({ success }) => success) })));
};

const batch = (taskGraph: TaskGraph, inputs: Record<string, unknown>, _overrides: unknown, context: ExecutorContext) => Effect.runPromise(decided(context, inputs, taskGraph));

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, execute };
