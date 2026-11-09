import { homedir } from 'node:os';
import { layer } from '@effect/platform-node/NodeServices';
import {
    AggregateCreateNodesError,
    type CreateDependencies,
    type CreateNodes,
    type CreateNodesResultArray,
    DependencyType,
    hashArray,
    type ProjectConfiguration,
    type RawProjectGraphDependency,
    type TargetConfiguration,
} from '@nx/devkit';
import { globWithWorkspaceContext, hashObject, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Record, Result, Schema, Stream } from 'effect';
import { ChildProcess, type ChildProcessSpawner, CommandLine } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

interface Options {
    readonly outputPath?: string;
    readonly trustPackages?: boolean;
    readonly lockedResolution?: boolean;
}
interface Part {
    readonly root: string;
    readonly name: string;
    readonly file: string;
    readonly technologies: readonly string[];
    readonly targets: Readonly<Record<string, TargetConfiguration>>;
}
interface Graph {
    readonly nodes: CreateNodesResultArray;
    readonly edges: readonly RawProjectGraphDependency[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const NAME = '@rasm/nx-apple';
const MANIFEST = 'Package.swift';
const FILES = `**/{${MANIFEST},Package@swift-*.swift,*.xcodeproj/project.xcproj,*.xcodeproj/project.pbxproj,*.xcworkspace/contents.xcworkspacedata,*.xcscheme,*.xctestplan,*.xcconfig,*.entitlements,*.plist}`;
const SKIPPED: ReadonlySet<string> = new Set(['.build', '.swiftpm', 'DerivedData']);
const MODULES: ReadonlySet<string> = new Set(['executable', 'library', 'macro', 'plugin']);
const CONTAINERS: ReadonlySet<string> = new Set(['.xcodeproj', '.xcworkspace']);
const ANALYZER = '.build/release/nx-apple-analyzer';
const STRIP = "unset LERNA_PACKAGE_NAME $(env | sed -n 's/^\\(NX_[A-Za-z0-9_]*\\)=.*/\\1/p');";

// --- [MODELS] --------------------------------------------------------------------------

const Strings = Schema.Array(Schema.String);
const Listed = Strings.pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));
const Described = Schema.fromJsonString(
    Schema.Struct({
        name: Schema.String,
        dependencies: Schema.Array(Schema.Struct({ path: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        targets: Schema.Array(
            Schema.Struct({
                name: Schema.String,
                type: Schema.Literals(['executable', 'library', 'test', 'macro', 'plugin', 'snippet', 'system-target', 'binary']),
                path: Schema.String,
                target_dependencies: Listed,
                product_dependencies: Listed,
            }),
        ),
        products: Schema.Array(Schema.Struct({ name: Schema.String, targets: Strings })),
    }),
);
const Container = Schema.Struct({
    path: Schema.String,
    schemes: Schema.Array(Schema.Struct({ name: Schema.String, entries: Schema.Array(Schema.Struct({ container: Schema.String })), plans: Strings, tests: Schema.Boolean })),
    inputs: Strings,
    members: Schema.Array(Schema.Struct({ product: Schema.String, packages: Strings })),
    projects: Strings,
    unresolved: Schema.Array(Schema.Struct({ target: Schema.String, setting: Schema.String, value: Schema.String })),
});
const Analyzed = Schema.fromJsonString(Schema.Struct({ containers: Schema.Array(Container), failures: Schema.Array(Schema.Struct({ path: Schema.String, reason: Schema.String })) }));
const Evaluated = Schema.fromJsonString(Schema.Array(Schema.Struct({ target: Schema.String, buildSettings: Schema.Record(Schema.String, Schema.String) })));
const Request = Schema.fromJsonString(Strings);

// --- [ERRORS] --------------------------------------------------------------------------

class AppleError extends Schema.TaggedError<AppleError, { readonly brand: unique symbol }>()('AppleError', { subject: Schema.String, detail: Schema.String }) {
    override get message() {
        return `${this.subject}: ${this.detail}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const shell = (words: readonly string[]) => Result.getOrThrow(CommandLine.join(words));

const tokened = (path: Path.Path, at: string) => (path.isAbsolute(at) || at.startsWith('..') ? [] : [path.join('{workspaceRoot}', at)]);

const run = (command: string, args: readonly string[], options: { readonly cwd: string; readonly env?: Readonly<Record<string, string>>; readonly input?: string }) =>
    Effect.scoped(
        Effect.gen(function* () {
            const handle = yield* ChildProcess.make(command, args, {
                cwd: options.cwd,
                extendEnv: true,
                ...(options.env === undefined ? {} : { env: options.env }),
                ...(options.input === undefined ? {} : { stdin: Stream.encodeText(Stream.succeed(options.input)) }),
            });
            const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
            return code === 0 ? stdout : yield* AppleError.make({ subject: [command, ...args].join(' '), detail: `exited ${code}: ${stderr.trim()}` });
        }),
    );

const commands = (lines: readonly string[], extra: Partial<TargetConfiguration> = {}): TargetConfiguration => ({ executor: 'nx:run-commands', options: { commands: lines, parallel: false }, ...extra });

const combined = (left: TargetConfiguration, right: TargetConfiguration): TargetConfiguration => ({
    ...left,
    ...right,
    options: { ...left.options, commands: [...(left.options?.commands ?? []), ...(right.options?.commands ?? [])] },
    ...(left.inputs === undefined && right.inputs === undefined ? {} : { inputs: Array.dedupe([...(left.inputs ?? []), ...(right.inputs ?? [])]) }),
    ...(left.outputs === undefined && right.outputs === undefined ? {} : { outputs: Array.dedupe([...(left.outputs ?? []), ...(right.outputs ?? [])]) }),
});

const linted = (path: Path.Path, root: string, paths: readonly string[]): Readonly<Record<string, TargetConfiguration>> => ({
    lint: commands([shell(['swift', 'format', 'lint', '--strict', '--parallel', '--recursive', ...paths])], {
        cache: true,
        inputs: [
            ...paths.flatMap((at) => (path.extname(at) === '.swift' ? [path.join('{workspaceRoot}', at)] : [path.join('{workspaceRoot}', at, '**/*.swift'), path.join('{workspaceRoot}', at, '**/.swift-format*')])),
            ...Array.unfold(Option.some(root), (at) => Option.map(at, (folder) => [folder, folder === '.' ? Option.none<string>() : Option.some(path.dirname(folder))] as const)).flatMap((folder) =>
                ['.swift-format', '.swift-format-ignore'].map((name) => path.join('{workspaceRoot}', folder, name)),
            ),
            { runtime: 'swift format --version' },
        ],
    }),
    format: commands([shell(['swift', 'format', 'format', '--in-place', '--parallel', '--recursive', ...paths])]),
});

const packageParts = (path: Path.Path, place: (kind: string, root: string) => Option.Option<string>, options: Options, file: string, versioned: readonly string[], held: typeof Described.Type): readonly Part[] => {
    const root = path.dirname(file);
    const modules = held.targets.filter(({ type }) => MODULES.has(type));
    const owner = modules.find(({ name }) => name === held.name);
    const scratch = place('swiftpm', root).pipe(Option.map((at) => `SWIFTPM_BUILD_DIR=${at}`), Option.toArray);
    const results = place('results', root);
    const locked = options.lockedResolution === true ? ['--disable-automatic-resolution'] : [];
    const suites = held.targets.filter(({ type }) => type === 'test');
    const tested = new Set(suites.flatMap(({ target_dependencies }) => target_dependencies));
    const work: Readonly<Record<string, TargetConfiguration>> = {
        build: commands([`${STRIP} ${shell(['env', ...scratch, 'swift', 'build', '--package-path', root, ...locked])}`]),
        ...(suites.length === 0
            ? {}
            : {
                  test: commands(
                      [
                          [
                              STRIP,
                              ...Option.toArray(results).map((at) => `${shell(['mkdir', '-p', at])} &&`),
                              shell(['env', ...scratch, 'swift', 'test', '--package-path', root, ...locked, ...Option.toArray(results).flatMap((at) => ['--xunit-output', path.join(at, 'xunit.xml')])]),
                          ].join(' '),
                      ],
                      {
                          cache: true,
                          dependsOn: ['build'],
                          inputs: ['default', '^default', { runtime: 'swift --version' }],
                          outputs: Option.toArray(results).flatMap((at) => tokened(path, at)),
                      },
                  ),
              }),
    };
    const lintable = [path.join(root, MANIFEST), ...versioned, ...held.targets.filter(({ type }) => !MODULES.has(type) && type !== 'binary').map((target) => path.join(root, target.path))];
    return [
        { root: owner === undefined ? root : path.join(root, owner.path), name: held.name, file, technologies: ['swift'], targets: { ...work, ...(owner === undefined ? linted(path, root, lintable) : {}) } },
        ...modules.map((module): Part => {
            const at = path.join(root, module.path);
            return {
                root: at,
                name: module.name,
                file,
                technologies: ['swift'],
                targets: {
                    ...(module === owner
                        ? {}
                        : Record.map(
                              Record.filter(work, (_, target) => target === 'build' || tested.has(module.name)),
                              (_, target): TargetConfiguration => ({ executor: 'nx:noop', dependsOn: [{ projects: [held.name], target }] }),
                          )),
                    ...linted(path, at, module === owner ? [at, ...lintable] : [at]),
                },
            };
        }),
    ];
};

const packageEdges = (held: typeof Described.Type, described: ReadonlyMap<string, typeof Described.Type>): readonly RawProjectGraphDependency[] => {
    const modules = new Set(held.targets.filter(({ type }) => MODULES.has(type)).map(({ name }) => name));
    const locals = held.dependencies.flatMap(({ path: at }) => Option.toArray(Option.flatMap(Option.fromUndefinedOr(at), (absolute) => Option.fromUndefinedOr(described.get(absolute)))));
    const products = (name: string) => locals.flatMap((local) => local.products.filter((product) => product.name === name).flatMap((product) => product.targets));
    return [
        ...[...modules].map((target) => ({ source: held.name, target })),
        ...locals.map((local) => ({ source: held.name, target: local.name })),
        ...held.targets
            .filter(({ name }) => modules.has(name))
            .flatMap(({ name, target_dependencies, product_dependencies }) => [...target_dependencies.filter((local) => modules.has(local)), ...product_dependencies.flatMap(products)].map((target) => ({ source: name, target }))),
    ].map((edge): RawProjectGraphDependency => ({ ...edge, type: DependencyType.implicit }));
};

const evaluated = Effect.fnUntraced(function* (workspaceRoot: string, container: typeof Container.Type) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const settings =
        container.unresolved.length === 0 ? [] : yield* Effect.flatMap(run('xcodebuild', ['-showBuildSettings', '-json', '-project', container.path, '-alltargets'], { cwd: workspaceRoot }), Schema.decodeEffect(Evaluated));
    const real = yield* fs.realPath(workspaceRoot);
    const [resolved, missing] = yield* Effect.partition(container.unresolved, (setting) =>
        Option.match(Option.fromUndefinedOr(settings.find(({ target }) => target === setting.target)?.buildSettings[setting.setting]), {
            onNone: () => Effect.fail(setting),
            onSome: (value) => Effect.mapBoth(fs.realPath(path.resolve(workspaceRoot, path.dirname(container.path), value)), { onFailure: () => setting, onSuccess: (at) => path.relative(real, at) }),
        }),
    );
    return missing.length === 0
        ? [...container.inputs, ...resolved]
        : yield* AppleError.make({ subject: container.path, detail: missing.map((setting) => `target ${setting.target} ${setting.setting}: ${setting.value} does not exist`).join('\n  ') });
});

const xcodeParts = (path: Path.Path, place: (kind: string, root: string) => Option.Option<string>, options: Options, file: string, container: typeof Container.Type, inputs: readonly string[], through: Option.Option<string>): Part => {
    const root = path.dirname(container.path);
    const built = Option.getOrElse(through, () => container.path);
    const workspace = path.extname(built) === '.xcworkspace';
    const lock = path.join(built, workspace ? 'xcshareddata/swiftpm/Package.resolved' : 'project.xcworkspace/xcshareddata/swiftpm/Package.resolved');
    const shared = [
        ...Option.toArray(place('xcode', path.dirname(built))).flatMap((at) => ['-derivedDataPath', at]),
        ...(options.trustPackages === true ? ['-skipMacroValidation', '-skipPackagePluginValidation'] : []),
        ...(options.lockedResolution === true ? ['-disableAutomaticPackageResolution'] : []),
    ];
    const invoke = (action: string, scheme: (typeof container.schemes)[number], extra: readonly string[]) => [
        'xcodebuild',
        action,
        workspace ? '-workspace' : '-project',
        built,
        '-scheme',
        scheme.name,
        ...(scheme.entries.every(({ container: owner }) => !CONTAINERS.has(path.extname(owner))) ? ['-destination', 'platform=macOS'] : []),
        ...shared,
        ...extra,
    ];
    const runs = container.schemes.filter(({ tests }) => tests).flatMap((scheme) => (scheme.plans.length === 0 ? [Option.none<string>()] : scheme.plans.map(Option.some)).map((plan) => ({ scheme, plan })));
    const bundles = runs.map(({ scheme, plan }) => Option.map(place('results', root), (at) => path.join(at, `${[scheme.name, ...Option.toArray(plan)].join('-')}.xcresult`)));
    return {
        root,
        name: path.basename(container.path, path.extname(container.path)),
        file,
        technologies: ['xcode'],
        targets: {
            build: commands(container.schemes.filter(({ entries }) => entries.length > 0).map((scheme) => `${STRIP} ${shell(invoke('build', scheme, []))}`)),
            ...(runs.length === 0
                ? {}
                : {
                      test: commands(
                          Array.zipWith(runs, bundles, ({ scheme, plan }, bundle) =>
                              [
                                  STRIP,
                                  ...Option.toArray(bundle).map((at) => `${shell(['rm', '-rf', at])} &&`),
                                  shell(invoke('test', scheme, [...Option.toArray(plan).flatMap((name) => ['-testPlan', name]), ...Option.toArray(bundle).flatMap((at) => ['-resultBundlePath', at])])),
                              ].join(' '),
                          ),
                          {
                              cache: true,
                              inputs: ['default', '^default', ...[...inputs, lock].flatMap((at) => tokened(path, at)), { runtime: 'xcodebuild -version' }],
                              outputs: bundles.flatMap(Option.toArray).flatMap((at) => tokened(path, at)),
                          },
                      ),
                  }),
            ...linted(path, root, [root]),
        },
    };
};

const analyze = Effect.fnUntraced(function* (workspaceRoot: string, options: Options, files: readonly string[]) {
    const path = yield* Path.Path;
    const output = Option.map(Option.fromUndefinedOr(options.outputPath), (at) => path.resolve(workspaceRoot, at.replace(/^~(?=$|\/)/, homedir())));
    const place = (kind: string, root: string) =>
        Option.map(output, (at) => {
            const placed = path.join(at, root, kind);
            return path.relative(workspaceRoot, placed).startsWith('..') ? placed : path.relative(workspaceRoot, placed);
        });
    const scanned = files.filter((file) => !file.split('/').some((segment) => SKIPPED.has(segment)) && Option.match(output, { onNone: () => true, onSome: (at) => !path.resolve(workspaceRoot, file).startsWith(`${at}/`) }));
    const manifests = scanned.filter((file) => path.basename(file) === MANIFEST);
    const containers = scanned.filter((file) => path.basename(file) === 'project.xcproj' || (path.basename(file) === 'contents.xcworkspacedata' && path.extname(path.dirname(path.dirname(file))) !== '.xcodeproj'));
    const unsupported = scanned.filter((file) => path.basename(file) === 'project.pbxproj' && !scanned.includes(path.join(path.dirname(file), 'project.xcproj')));
    const marker = (container: string) => containers.find((file) => path.dirname(file) === container) ?? container;
    const describedResults = yield* Effect.forEach(
        manifests,
        (file) =>
            Effect.result(
                Effect.flatMap(
                    run('swift', ['package', '--package-path', path.dirname(file), 'describe', '--type', 'json'], {
                        cwd: workspaceRoot,
                        env: { SWIFTPM_BUILD_DIR: path.join(Option.getOrElse(output, () => path.join(workspaceDataDirectory, 'nx-apple')), 'describe', path.dirname(file)) },
                    }),
                    Schema.decodeEffect(Described),
                ),
            ),
        { concurrency: 'unbounded' },
    );
    const packages = Array.zip(manifests, describedResults).flatMap(([file, held]) => (Result.isSuccess(held) ? [{ file, held: held.success }] : []));
    const described = new Map(packages.map(({ file, held }) => [path.resolve(workspaceRoot, path.dirname(file)), held] as const));
    const analyzed =
        containers.length === 0
            ? { containers: [], failures: [] }
            : yield* Effect.flatMap(run(path.join(import.meta.dirname, ANALYZER), [], { cwd: workspaceRoot, input: yield* Schema.encodeEffect(Request)(containers) }), Schema.decodeEffect(Analyzed));
    const evaluatedResults = yield* Effect.forEach(analyzed.containers, (container) => Effect.result(evaluated(workspaceRoot, container)), { concurrency: 'unbounded' });
    const xcode = Array.zip(analyzed.containers, evaluatedResults).flatMap(([container, inputs]) => (Result.isSuccess(inputs) ? [{ container, inputs: inputs.success }] : []));
    const workspaces = xcode.filter(({ container }) => path.extname(container.path) === '.xcworkspace').map(({ container }) => container);
    yield* Effect.forEach(packages, ({ file, held }) =>
        Effect.forEach(
            manifests.filter((other) => other !== file && held.targets.some((target) => `${path.dirname(other)}/`.startsWith(`${path.join(path.dirname(file), target.path)}/`))),
            (nested) => Effect.logWarning(`${nested} sits inside a target folder of ${file}, so SwiftPM compiles it into that target`),
        ),
    );
    const parts = [
        ...packages.flatMap(({ file, held }) =>
            packageParts(
                path,
                place,
                options,
                file,
                scanned.filter((other) => path.dirname(other) === path.dirname(file) && path.basename(other).startsWith('Package@swift-')),
                held,
            ),
        ),
        ...xcode.map(({ container, inputs }) => {
            const owners = workspaces.filter((workspace) => workspace.projects.includes(container.path)).map((workspace) => workspace.path);
            return xcodeParts(path, place, options, marker(container.path), container, inputs, owners.length === 1 ? Array.head(owners) : Option.none());
        }),
    ];
    const grouped = Array.groupBy(parts, (part) => part.root);
    const conflicts = Record.filter(grouped, (group) => new Set(group.map(({ name }) => name)).size > 1);
    const merged = Record.collect(
        Record.filter(grouped, (_, root) => !(root in conflicts)),
        (_, group) => group.reduce((left, right) => ({ ...left, technologies: Array.dedupe([...left.technologies, ...right.technologies]), targets: Record.union(left.targets, right.targets, combined) })),
    );
    const roots = new Map(merged.map((part) => [part.name, part.root] as const));
    const byRoot = new Map(merged.map((part) => [part.root, part.name] as const));
    const edges = [
        ...packages.flatMap(({ held }) => packageEdges(held, described)),
        ...xcode.flatMap(({ container }) =>
            [
                ...container.members.flatMap(({ product, packages: roots }) =>
                    roots.flatMap((root) => Option.toArray(Option.fromUndefinedOr(described.get(path.resolve(workspaceRoot, root))))).flatMap((held) => held.products.filter(({ name }) => name === product).flatMap(({ targets }) => targets)),
                ),
                ...container.projects.flatMap((at) => Option.toArray(Option.fromUndefinedOr(byRoot.get(CONTAINERS.has(path.extname(at)) ? path.dirname(at) : path.normalize(at))))),
            ].map((target): RawProjectGraphDependency => ({ source: path.basename(container.path, path.extname(container.path)), target, sourceFile: marker(container.path), type: DependencyType.static })),
        ),
    ].flatMap((edge) =>
        Option.toArray(Option.all([Option.fromUndefinedOr(roots.get(edge.source)), Option.fromUndefinedOr(roots.get(edge.target))])).flatMap(([source, target]) => (source === target ? [] : [{ ...edge, source, target }])),
    );
    return {
        graph: {
            nodes: merged.map(
                (part) =>
                    [
                        part.file,
                        {
                            projects: {
                                [part.root]: {
                                    name: part.name,
                                    metadata: { technologies: [...part.technologies] },
                                    targets: { ...part.targets, check: { executor: 'nx:noop', dependsOn: ['lint', 'build', 'test'].filter((target) => target in part.targets) } },
                                } satisfies Omit<ProjectConfiguration, 'root'>,
                            },
                        },
                    ] as const,
            ),
            edges: Array.dedupeWith(edges, (left, right) => left.source === right.source && left.target === right.target),
        } satisfies Graph,
        failures: [
            ...Array.zip(manifests, describedResults).flatMap(([file, held]) => (Result.isFailure(held) ? [[file, held.failure] as const] : [])),
            ...analyzed.failures.map(({ path: at, reason }) => [marker(at), AppleError.make({ subject: at, detail: reason })] as const),
            ...Array.zip(analyzed.containers, evaluatedResults).flatMap(([container, held]) => (Result.isFailure(held) ? [[marker(container.path), held.failure] as const] : [])),
            ...unsupported.map((file) => [file, AppleError.make({ subject: file, detail: 'has no project.xcproj beside it, and project.pbxproj has no reader' })] as const),
            ...Record.collect(conflicts, (root, group) => [group[0]?.file ?? root, AppleError.make({ subject: root, detail: `holds projects named ${group.map(({ name }) => name).join(' and ')}` })] as const),
        ],
    };
});

const graph = Effect.fnUntraced(function* (workspaceRoot: string, options: Options, matched: readonly string[]) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const files = matched.toSorted();
    const hash = hashArray([
        yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [FILES])),
        yield* fs.readFileString(import.meta.filename),
        String(Option.getOrElse(Option.map((yield* fs.stat(path.join(import.meta.dirname, ANALYZER))).mtime, (mtime) => mtime.valueOf()), () => 0)),
        hashObject(options),
        ...files,
    ]);
    const cache = new PluginCache<Graph>(path.join(workspaceDataDirectory, 'nx-apple.hash'));
    return yield* Option.match(Option.fromNullishOr(cache.get(hash)), {
        onSome: (held) => Effect.succeed({ graph: held, failures: [] }),
        onNone: () => Effect.tap(analyze(workspaceRoot, options, files), ({ graph: built, failures }) => (failures.length === 0 ? Effect.sync(() => (cache.set(hash, built), cache.writeToDisk())) : Effect.void)),
    });
});

const runGraph = <A, E>(effect: Effect.Effect<A, E, Path.Path | FileSystem.FileSystem | ChildProcessSpawner.ChildProcessSpawner>) => Effect.runPromise(Effect.provide(effect, layer));

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<Options> = [
    FILES,
    (files, options = {}, { workspaceRoot }) =>
        runGraph(
            Effect.flatMap(graph(workspaceRoot, options, files), ({ graph: built, failures }) =>
                failures.length === 0 ? Effect.succeed(built.nodes) : Effect.fail(new AggregateCreateNodesError(failures.map(([file, error]) => [file, error]), [...built.nodes])),
            ),
        ),
];

const createDependencies: CreateDependencies<Options> = (options = {}, { workspaceRoot, projects }) =>
    runGraph(
        Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [FILES])).pipe(
            Effect.flatMap((files) => graph(workspaceRoot, options, files)),
            Effect.map(({ graph: built }) => {
                const names = new Map(Object.values(projects).map((project) => [project.root, project.name] as const));
                return built.edges.flatMap((edge) =>
                    Option.toArray(Option.all([Option.fromUndefinedOr(names.get(edge.source)), Option.fromUndefinedOr(names.get(edge.target))])).map(([source, target]) => ({ ...edge, source, target })),
                );
            }),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes, NAME as name, type Options };
