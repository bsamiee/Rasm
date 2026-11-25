import { dirname, join, matchesGlob, relative, sep } from 'node:path';
import { DependencyType, type RawProjectGraphDependency } from '@nx/devkit';
import { EXECUTORS, removable, type Verbs } from '@rasm/nx';
import { Array, Effect, Graph, Option, Record, Schema, Struct, UndefinedOr } from 'effect';
import { ATTACH, INPUTS, type Options, type Part, PLUGIN, SWIFTPM, sequenced } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

interface Package {
    readonly file: string;
    readonly root: string;
    readonly plists: Readonly<Record<string, string>>;
    readonly entitled: Readonly<Record<string, string>>;
    readonly bins: Readonly<Record<'debug' | 'release' | 'codecov', string>>;
    readonly shipped: readonly string[];
    readonly manifest: Described;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'Package.swift';
const PACKAGED = 'package.json';
const INFO = 'Info.plist';
const ENTITLEMENTS = '.entitlements';
const RESOLVED = 'Package.resolved';

// --- [MODELS] --------------------------------------------------------------------------

const Listed = Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));
const Tests = Schema.Literal('test');
const Executable = Schema.Literal('executable');
class Described extends Schema.Opaque<Described, unknown>()(
    Schema.fromJsonString(
        Schema.Struct({
            name: Schema.String,
            dependencies: Schema.Array(Schema.Struct({ path: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
            targets: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, path: Schema.String, target_dependencies: Listed, product_dependencies: Listed, resources: Schema.Array(Schema.Unknown).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) })),
            products: Schema.Array(Schema.Struct({ name: Schema.String, targets: Schema.Array(Schema.String), type: Schema.Record(Schema.String, Schema.Unknown) })),
        }),
    ),
) {}
const Shipped = Schema.fromJsonString(Schema.Struct({ files: Listed })).annotate({ description: 'The files row of the package.json beside a Package.swift, the paths npm publishes, a release product among them making release the default configuration of its build' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const locals = (manifest: Described, described: ReadonlyMap<string, Package>) => manifest.dependencies.flatMap(({ path }) => Array.fromNullishOr(UndefinedOr.map(path, (at) => described.get(at))));

const sources = ({ root, manifest }: Package, described: ReadonlyMap<string, Package>, products: Option.Option<readonly string[]>): readonly string[] => {
    const indexed = new Map(manifest.targets.map(({ name }, index) => [name, index] as const));
    const graph = Graph.fromSnapshot({
        type: 'directed',
        nodes: manifest.targets.map((data, index) => ({ index, data })),
        edges: manifest.targets.flatMap(({ target_dependencies }, source) => target_dependencies.flatMap((name) => Array.fromNullishOr(indexed.get(name))).map((target) => ({ source, target, data: undefined }))).map((edge, index) => ({ ...edge, index })),
    });
    const linked = Option.map(products, (names) => new Set(manifest.products.filter(({ name }) => names.includes(name)).flatMap(({ targets }) => targets)));
    const reached = Array.fromIterable(Graph.values(Graph.dfs(graph, { start: Graph.findNodes(graph, ({ name }) => Option.match(linked, { onNone: () => true, onSome: (held) => held.has(name) })) })));
    const used = reached.flatMap(({ product_dependencies }) => product_dependencies);
    return [
        ...reached.map((module) => join('{workspaceRoot}', root, module.path, '**/*')),
        ...[MANIFEST, 'Package@swift-*.swift', RESOLVED, '.swiftpm/configuration/*.json'].map((file) => join('{workspaceRoot}', root, file)),
        ...locals(manifest, described)
            .flatMap((local) => sources(local, described, Option.as(products, Array.intersection(local.manifest.products.map(Struct.get('name')), used))))
            .filter((input) => !input.endsWith(RESOLVED)),
    ];
};

const packageParts = (options: Options, local: Package, described: ReadonlyMap<string, Package>, owners: ReadonlyMap<Package, Part>): readonly Part[] => {
    const { file, root, plists, entitled, manifest, bins, shipped } = local;
    const whole = Array.isReadonlyArrayNonEmpty(manifest.products) && manifest.products.every(({ type }) => 'executable' in type) && manifest.targets.every(({ resources }) => resources.length === 0);
    const suites = manifest.targets.filter(({ type }) => Schema.is(Tests)(type));
    const modules = manifest.targets.filter(({ name, type }) => !Schema.is(Tests)(type) && name !== manifest.name);
    const inputs = [...sources(local, described, Option.none()), ...INPUTS];
    const build = ['swift', 'build', '--package-path', root, ...SWIFTPM] as const;
    const test = ['swift', 'test', '--package-path', root, ...SWIFTPM] as const;
    const lcov = Option.map(Option.fromUndefinedOr(options.coveragePath), (folder) => join(folder, 'lcov.info'));
    const covered = Option.getOrElse(lcov, () => bins.codecov);
    const written = (folder: string) => (whole ? manifest.products.map(({ name }) => join('{workspaceRoot}', folder, name)) : []);
    const released = written(bins.release);
    const scratch = join(root, ...relative(root, bins.debug).split(sep, 1));
    const report = Option.map(lcov, (at) => ({ report: { profiles: [join(dirname(bins.codecov), 'default.profdata')], objects: Array.map(suites, ({ name }) => join(bins.debug, `${name}.xctest`, 'Contents', 'MacOS', name)), built: [scratch], mapping: ['-Xswiftc', '-coverage-prefix-map', '-Xswiftc'], file: at } }));
    const coverage = { runs: [[...test, '--enable-code-coverage']], outputs: [join('{workspaceRoot}', covered)], ...Option.getOrUndefined(report) };
    const testing = { cache: true, dependsOn: ['build--swift-build'], inputs, outputs: ['{options.outputs.0}'], configurations: { coverage } };
    const work: Verbs = {
        build: {
            'swift-build': sequenced(
                [build],
                {
                    technologies: ['swift'],
                    description: `Builds ${manifest.name} with swift build, in release under the release configuration, restoring its products from the cache where they are executables alone`,
                    help: { command: 'swift build --help', example: { args: ['--product', manifest.products[0]?.name ?? manifest.name] } },
                },
                {
                    cache: true,
                    inputs,
                    options: { outputs: written(bins.debug) },
                    outputs: released.map((_, index) => `{options.outputs.${index}}`),
                    configurations: { release: { runs: [[...build, '--configuration', 'release']], outputs: released } },
                    ...(manifest.products.some(({ name }) => shipped.some((entry) => matchesGlob(relative(root, join(bins.release, name)), entry))) && { defaultConfiguration: 'release' }),
                },
            ),
        },
        check: { 'swift-build': { dependsOn: ['build--swift-build'], inputs, metadata: { technologies: ['swift'], description: `Type-checks ${manifest.name} with swift build`, help: { command: 'swift build --help', example: { args: ['--configuration', 'release'] } } } } },
        test: {
            ...(Array.isReadonlyArrayNonEmpty(suites) && {
                swift: sequenced(
                    [test],
                    {
                        technologies: ['swift'],
                        description: `Tests ${manifest.name} with swift test, under the coverage configuration with --enable-code-coverage, exporting lcov with xcrun llvm-cov export where coveragePath is set`,
                        help: { command: 'swift test --help', example: { args: ['--filter', Array.headNonEmpty(suites).name] } },
                    },
                    testing,
                ),
            }),
        },
    };
    const executables = new Set(manifest.targets.filter(({ type }) => Schema.is(Executable)(type)).map(({ name }) => name));
    const products = new Map(manifest.products.flatMap(({ name, targets }) => targets.filter((owner) => executables.has(owner)).map((owner) => [owner, name] as const)));
    const scheme = Option.fromUndefinedOr(owners.get(local));
    const [compiler, home] = Option.match(scheme, { onNone: () => [manifest.name, root] as const, onSome: ({ name, root: at }) => [name, at] as const });
    const owned: Verbs = {
        ...(Option.isNone(scheme) && work),
        upgrade: {
            'swift-package': {
                executor: `${PLUGIN}:upgrade`,
                inputs,
                options: { package: root },
                metadata: {
                    technologies: ['swift'],
                    description: `Moves the lower bound of each remote from: requirement of ${manifest.name} to the newest semver tag of its repository, prereleases included, published at or after the bound's tag, then rewrites Package.resolved with swift package update`,
                    help: { command: 'swift package update --help', example: { options: { dryRun: true } } },
                },
            },
        },
    };
    return [
        { module: manifest.name, name: compiler, at: home, own: owned, swept: [['swift', 'package', '--package-path', root, 'clean']], description: `Removes the build folder of ${manifest.name} with swift package clean, then the outputs its targets declare` },
        ...modules.map((module) => ({ module: module.name, name: module.name, at: join(root, module.path), own: Record.empty<string, Verbs[string]>(), swept: [], description: `Removes the outputs the targets of ${module.name} declare` })),
    ].map(({ module, name, at, own, swept, description }): Part => {
        const product = Option.fromUndefinedOr(products.get(module));
        const signing = Option.getOrUndefined(Option.zipWith(product, Record.get(entitled, module), (_, entitlements) => ({ entitlements })));
        const zipped = Option.zipWith(product, Record.get(plists, module), (held, plist) => ({ held, plist, folder: options.outputPath === undefined ? bins.release : join(options.outputPath, held) }));
        const tools: Verbs = {
            ...own,
            ...Option.getOrUndefined(
                Option.map(product, (held) => ({
                    install: {
                        'swift-package': {
                            executor: `${PLUGIN}:install`,
                            inputs,
                            options: { package: root, product: held, ...signing },
                            metadata: { technologies: ['swift'], description: `Builds ${held} in release with swift build, signs it, and replaces it in ~/.local/bin`, help: { command: 'swift build --help', example: { args: ['--jobs', '4'] } } },
                        },
                    },
                })),
            ),
            ...Option.getOrUndefined(
                Option.map(zipped, ({ held, plist, folder }) => ({
                    pack: {
                        ditto: {
                            executor: `${PLUGIN}:package`,
                            dependsOn: Record.keys(own['build'] ?? {}).map((tool) => `build--${tool}`),
                            inputs: [join('{workspaceRoot}', plist), ...inputs],
                            options: { products: [{ package: root, product: held, folder, bin: bins.release, ...signing }] },
                            outputs: [join('{workspaceRoot}', folder, `${held}.zip`)],
                            metadata: { technologies: ['swift'], description: `Signs the release build of ${held} and zips it with ditto for a GitHub release`, help: { command: 'man ditto', example: { args: ['--norsrc'] } } },
                        },
                    },
                    attach: { gh: ATTACH },
                })),
            ),
        };
        const removed = removable(tools);
        return {
            root: at,
            name,
            file,
            technologies: ['swift'],
            tools: {
                ...tools,
                ...(swept.length + removed.length > 0 && {
                    clean: { 'swift-package': { executor: EXECUTORS.clean, inputs, options: { commands: swept, cwd: '.', targets: removed }, metadata: { technologies: ['swift'], description, help: { command: 'swift package clean --help', example: {} } } } },
                }),
            },
            ...Option.getOrUndefined(Option.map(zipped, ({ plist }) => ({ release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { manifest: relative(at, plist) } } } }))),
        };
    });
};

const packageEdges = (local: Package, described: ReadonlyMap<string, Package>, owners: ReadonlyMap<Package, Part>): readonly RawProjectGraphDependency[] => {
    const { file, manifest } = local;
    const named = (held: Package) => owners.get(held)?.name ?? held.manifest.name;
    const modules = new Set(manifest.targets.filter(({ name, type }) => !Schema.is(Tests)(type) && name !== manifest.name).map(({ name }) => name));
    const project = (module: string) => (modules.has(module) ? module : named(local));
    const linked = locals(manifest, described);
    const products = (product: string) => linked.flatMap((held) => held.manifest.products.filter((owned) => owned.name === product).flatMap((owned) => owned.targets));
    return Array.dedupeWith(
        [
            ...linked.map((held): RawProjectGraphDependency => ({ source: named(local), target: named(held), type: DependencyType.static, sourceFile: file })),
            ...manifest.targets.flatMap((module) => [...module.target_dependencies.map(project), ...module.product_dependencies.flatMap(products)].map((dependency): RawProjectGraphDependency => ({ source: project(module.name), target: dependency, type: DependencyType.implicit }))),
        ],
        (left, right) => left.source === right.source && left.target === right.target,
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Described, ENTITLEMENTS, INFO, MANIFEST, PACKAGED, type Package, packageEdges, packageParts, RESOLVED, Shipped, sources };
