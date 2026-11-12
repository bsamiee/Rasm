import { DependencyType, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import type { Identity, Members } from '@rasm/nx-devkit/plugin';
import { Array, Effect, Option, type Path, pipe, Record, Schema } from 'effect';
import { GIT_STATUS, type Part, type Settings, target } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

interface Package {
    readonly file: string;
    readonly root: string;
    readonly plists: ReadonlySet<string>;
    readonly bins?: Readonly<Record<'debug' | 'release', string>>;
    readonly manifest: typeof Described.Type;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'Package.swift';
const INFO = 'Info.plist';
const RESOLVED = 'Package.resolved';

// --- [MODELS] --------------------------------------------------------------------------

const Listed = Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));
const Tests = Schema.Literal('test');
const Executable = Schema.Literal('executable');
const Described = Schema.fromJsonString(
    Schema.Struct({
        name: Schema.String,
        platforms: Schema.Array(Schema.Struct({ name: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        dependencies: Schema.Array(Schema.Struct({ path: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        targets: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, path: Schema.String, product_memberships: Listed, target_dependencies: Listed, product_dependencies: Listed, resources: Schema.Array(Schema.Unknown).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) })),
        products: Schema.Array(Schema.Struct({ name: Schema.String, targets: Schema.Array(Schema.String), type: Schema.Record(Schema.String, Schema.Unknown) })),
    }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const locals = (manifest: typeof Described.Type, described: ReadonlyMap<string, Package>) => Array.getSomes(manifest.dependencies.map((dependency) => Option.flatMap(Option.fromUndefinedOr(dependency.path), (at) => Option.fromUndefinedOr(described.get(at)))));

const sources = (path: Path.Path, { root, manifest }: Package, described: ReadonlyMap<string, Package>): readonly string[] => [
    ...manifest.targets.map((module) => path.join('{workspaceRoot}', root, module.path, '**/*')),
    ...[MANIFEST, 'Package@swift-*.swift', RESOLVED, '.swiftpm/configuration/*.json'].map((file) => path.join('{workspaceRoot}', root, file)),
    ...locals(manifest, described)
        .flatMap((local) => sources(path, local, described))
        .filter((input) => !input.endsWith(RESOLVED)),
];

const packageParts = (path: Path.Path, options: Settings, plugin: Identity, local: Package, described: ReadonlyMap<string, Package>): readonly Part[] => {
    const { file, root, plists, manifest, bins } = local;
    const modules = manifest.targets.filter((module) => module.product_memberships.length > 0 && module.name !== manifest.name);
    const suites = manifest.targets.filter(({ type }) => Schema.is(Tests)(type));
    const tested = new Set(suites.flatMap(({ target_dependencies }) => target_dependencies));
    const inputs = [...sources(path, local, described), path.join('{workspaceRoot}', '.swift-version'), ...plugin.inputs];
    const pinned = '--force-resolved-versions';
    const build = ['swift', 'build', '--package-path', root, pinned] as const;
    const written = (folder?: string) => [path.join(root, RESOLVED), ...(folder === undefined ? [] : manifest.products.map(({ name }) => path.join(folder, name)))].map((at) => path.join('{workspaceRoot}', at));
    const work = {
        build: {
            'swift-build': target(
                plugin,
                [build],
                { technologies: ['swift'], description: `Builds ${manifest.name} with SwiftPM, in release under the release configuration, cached where its products are executables alone`, help: { command: 'swift build --help', example: { args: ['--product', manifest.products[0]?.name ?? manifest.name] } } },
                {
                    ...(bins !== undefined && { cache: true }),
                    inputs,
                    options: { outputs: written(bins?.debug) },
                    outputs: written(bins?.debug).map((_, index) => `{options.outputs.${index}}`),
                    configurations: { release: { runs: [[...build, '--configuration', 'release']], outputs: written(bins?.release) } },
                },
            ),
        },
        check: { 'swift-build': { executor: 'nx:noop', dependsOn: ['build--swift-build'], inputs, metadata: { technologies: ['swift'], description: `Compiles ${manifest.name} as its type check` } } },
        test:
            suites.length === 0
                ? {}
                : {
                      swift: target(
                          plugin,
                          [['swift', 'test', '--package-path', root, pinned]],
                          { technologies: ['swift'], description: `Tests ${manifest.name} with SwiftPM`, help: { command: 'swift test --help', example: { args: ['--filter', suites[0]?.name ?? manifest.name] } } },
                          { cache: true, dependsOn: ['build--swift-build'], inputs: [...inputs, { dependentTasksOutputFiles: path.join(root, RESOLVED) }] },
                      ),
                  },
    } satisfies Members;
    const executables = new Set(manifest.targets.filter(({ type }) => Schema.is(Executable)(type)).map(({ name }) => name));
    const products = new Map(manifest.products.flatMap(({ name, targets }) => targets.filter((owner) => executables.has(owner) && plists.has(owner)).map((owner) => [owner, name] as const)));
    const shipped = (product: string, at: string) => {
        const zipped = pipe(
            options.placed('swiftpm', product),
            Option.map((folder) => ({
                ditto: {
                    executor: `${plugin.name}:package`,
                    dependsOn: ['build--swift-build'],
                    inputs: [path.join('{workspaceRoot}', at, INFO), ...plugin.inputs],
                    options: { products: [{ package: root, product, folder }] },
                    outputs: [path.join('{workspaceRoot}', folder, `${product}.zip`)],
                    metadata: { technologies: ['swift'], description: `Zips the release build of ${product} with ditto for a GitHub release`, help: { command: 'man ditto', example: { args: ['--norsrc'] } } },
                },
            })),
        );
        return {
            zipped,
            install: {
                executor: `${plugin.name}:install`,
                options: { package: root, product, env: GIT_STATUS, args: [pinned] },
                metadata: { technologies: ['swift'], description: `Builds ${product} in release and replaces it in SwiftPM's install folder`, help: { command: 'swift package experimental-install --help', example: { args: ['--jobs', '4'] } } },
            },
        };
    };
    const forwardedInputs = ['default', '^default', ...[MANIFEST, RESOLVED].map((at) => path.join('{workspaceRoot}', root, at)), ...plugin.inputs];
    const forwarded = (module: string): Members =>
        Record.map(
            Record.filter<string, Readonly<Record<string, TargetConfiguration>>>(work, (_, verb) => verb !== 'test' || tested.has(module)),
            (tools, verb) => Record.map(tools, (_, tool): TargetConfiguration => ({ executor: 'nx:noop', dependsOn: [{ projects: [manifest.name], target: `${verb}--${tool}` }], inputs: forwardedInputs, metadata: { technologies: ['swift'], description: `Runs ${manifest.name}:${verb}--${tool}` } })),
        );
    const upgrade = target(
        plugin,
        [['swift', 'package', '--package-path', root, 'update']],
        { technologies: ['swift'], description: `Moves ${manifest.name}'s remote dependencies to the newest versions its requirements allow and rewrites Package.resolved`, help: { command: 'swift package update --help', example: { args: ['--dry-run'] } } },
        { inputs },
    );
    const owned: Members = {
        ...work,
        upgrade: { 'swift-package': upgrade },
        clean: { 'swift-package': { options: { commands: [['swift', 'package', '--package-path', root, 'clean']], cwd: '.' }, metadata: { description: `Removes the build folder of ${manifest.name} with swift package clean`, help: { command: 'swift package clean --help', example: {} } } } },
    };
    return [{ name: manifest.name, at: root, own: owned }, ...modules.map((module) => ({ name: module.name, at: path.join(root, module.path), own: forwarded(module.name) }))].map(({ name, at, own }): Part => {
        const part = { root: at, name, file, technologies: ['swift'], clean: 'swift-package' };
        return Option.match(
            Option.map(Option.fromUndefinedOr(products.get(name)), (product) => shipped(product, at)),
            {
                onNone: () => ({ ...part, members: own }),
                onSome: ({ install, zipped }) => ({
                    ...part,
                    members: { ...own, pack: Option.getOrElse(zipped, () => ({})), install: { 'swift-package': install } },
                    ...Option.getOrUndefined(Option.map(zipped, () => ({ release: { manifest: INFO }, packaged: ['pack--ditto'] }))),
                }),
            },
        );
    });
};

const packageEdges = ({ file, manifest }: Package, described: ReadonlyMap<string, Package>): readonly RawProjectGraphDependency[] => {
    const modules = new Set(manifest.targets.filter((module) => module.product_memberships.length > 0).map((module) => module.name));
    const project = (module: string) => (modules.has(module) ? module : manifest.name);
    const linked = locals(manifest, described).map((local) => local.manifest);
    const products = (product: string) => linked.flatMap((local) => local.products.filter((owned) => owned.name === product).flatMap((owned) => owned.targets));
    return [
        ...linked.map((local): RawProjectGraphDependency => ({ source: manifest.name, target: local.name, type: DependencyType.static, sourceFile: file })),
        ...manifest.targets.flatMap((module) => [...module.target_dependencies.map(project), ...module.product_dependencies.flatMap(products)].map((dependency): RawProjectGraphDependency => ({ source: project(module.name), target: dependency, type: DependencyType.implicit }))),
    ];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Described, INFO, MANIFEST, type Package, packageEdges, packageParts, RESOLVED, sources };
