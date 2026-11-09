import { DependencyType, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { isCI } from '@nx/devkit/internal';
import { Effect, Option, type Path, Predicate, pipe, Record, Schema } from 'effect';
import type { Options } from '#/plugins/project-graph';
import { type Part, type Plugin, published, target } from '#/plugins/targets';

// --- [TYPES] ---------------------------------------------------------------------------

interface Package {
    readonly file: string;
    readonly root: string;
    readonly plists: ReadonlySet<string>;
    readonly manifest: typeof Described.Type;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'Package.swift';
const INFO = 'Info.plist';

// --- [MODELS] --------------------------------------------------------------------------

const Listed = Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));
const Tests = Schema.Literal('test');
const Executable = Schema.Literal('executable');
const Described = Schema.fromJsonString(
    Schema.Struct({
        name: Schema.String,
        platforms: Schema.Array(Schema.Struct({ name: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        dependencies: Schema.Array(Schema.Struct({ path: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        targets: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, path: Schema.String, product_memberships: Listed, target_dependencies: Listed, product_dependencies: Listed })),
        products: Schema.Array(Schema.Struct({ name: Schema.String, targets: Schema.Array(Schema.String) })),
    }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const locals = (manifest: typeof Described.Type, described: ReadonlyMap<string, Package>) => manifest.dependencies.flatMap((dependency) => Option.toArray(Option.flatMap(Option.fromUndefinedOr(dependency.path), (at) => Option.fromUndefinedOr(described.get(at)))));

const sources = (path: Path.Path, { root, manifest }: Package, described: ReadonlyMap<string, Package>): readonly string[] => [
    ...manifest.targets.map((module) => path.join('{workspaceRoot}', root, module.path, '**/*')),
    ...[MANIFEST, 'Package@swift-*.swift', 'Package.resolved', '.swiftpm/configuration/*.json'].map((file) => path.join('{workspaceRoot}', root, file)),
    ...locals(manifest, described)
        .flatMap((local) => sources(path, local, described))
        .filter((input) => !input.endsWith('Package.resolved')),
];

const packageParts = (path: Path.Path, options: Options, plugin: Plugin, local: Package, described: ReadonlyMap<string, Package>): readonly Part[] => {
    const { file, root, plists, manifest } = local;
    const modules = manifest.targets.filter((module) => module.product_memberships.length > 0 && module.name !== manifest.name);
    const flags = Predicate.isTruthy(isCI()) ? ['--disable-automatic-resolution'] : [];
    const suites = manifest.targets.filter(({ type }) => Schema.is(Tests)(type));
    const tested = new Set(suites.flatMap(({ target_dependencies }) => target_dependencies));
    const inputs = [...sources(path, local, described), path.join('{workspaceRoot}', '.swift-version'), ...plugin.inputs];
    const build = ['swift', 'build', '--package-path', root, ...flags] as const;
    const work: Readonly<Record<string, TargetConfiguration>> = {
        'build--swift-build': target(
            plugin,
            [build],
            { technologies: ['swift'], description: `Builds ${manifest.name} with SwiftPM, in release under the release configuration`, help: { command: 'swift build --help', example: { args: ['--product', manifest.products[0]?.name ?? manifest.name] } } },
            { inputs, configurations: { release: { runs: [[...build, '--configuration', 'release']] } } },
        ),
        'check--swift-build': { executor: 'nx:noop', dependsOn: ['build--swift-build'], inputs, metadata: { technologies: ['swift'], description: `Compiles ${manifest.name} as its type check` } },
        ...(suites.length === 0
            ? {}
            : {
                  test: target(
                      plugin,
                      [['swift', 'test', '--package-path', root, ...flags]],
                      { technologies: ['swift'], description: `Tests ${manifest.name} with SwiftPM`, help: { command: 'swift test --help', example: { args: ['--filter', suites[0]?.name ?? manifest.name] } } },
                      { cache: true, dependsOn: ['build--swift-build'], inputs },
                  ),
              }),
    };
    const executables = new Set(manifest.targets.filter(({ type }) => Schema.is(Executable)(type)).map(({ name }) => name));
    const products = new Map(manifest.products.flatMap(({ name, targets }) => targets.filter((owner) => executables.has(owner) && plists.has(owner)).map((owner) => [owner, name] as const)));
    const shipped = (product: string, at: string): Readonly<Record<string, TargetConfiguration>> => {
        const zipped = pipe(
            Option.fromUndefinedOr(options.outputPath),
            Option.map((output) => path.join(output, 'swiftpm', product)),
            Option.map((folder) => ({
                'build--ditto': {
                    executor: `${plugin.name}:zip`,
                    dependsOn: ['build--swift-build'],
                    inputs: [path.join('{workspaceRoot}', at, INFO), ...plugin.inputs],
                    options: { products: [] },
                    configurations: { release: { products: [{ package: root, product, folder, manifest: path.join(at, INFO) }] } },
                    outputs: [path.join('{workspaceRoot}', folder, `${product}-*.zip`)],
                    metadata: { technologies: ['swift'], description: `Zips the release build of ${product} with ditto for a GitHub release`, help: { command: 'man ditto', example: { args: ['--norsrc'] } } },
                },
                'nx-release-publish': published(plugin, 'swift', [folder], []),
            })),
        );
        return {
            install: {
                executor: `${plugin.name}:install`,
                options: { package: root, product },
                inputs: plugin.inputs,
                cache: false,
                metadata: { technologies: ['swift'], description: `Builds ${product} in release and replaces it in SwiftPM's install folder`, help: { command: 'swift package experimental-install --help', example: { args: ['--jobs', '4'] } } },
            },
            ...Option.getOrElse(zipped, () => ({})),
        };
    };
    const forwarded = (module: string) =>
        Record.map(
            Record.filter(work, (_, verb) => verb !== 'test' || tested.has(module)),
            (_, verb): TargetConfiguration => ({ executor: 'nx:noop', dependsOn: [{ projects: [manifest.name], target: verb }], inputs, metadata: { technologies: ['swift'], description: `Runs ${manifest.name}:${verb}` } }),
        );
    return [{ name: manifest.name, at: root, own: work }, ...modules.map((module) => ({ name: module.name, at: path.join(root, module.path), own: forwarded(module.name) }))].map(({ name, at, own }): Part => {
        const product = products.get(name);
        return product === undefined
            ? { root: at, name, file, technologies: ['swift'], targets: own }
            : { root: at, name, file, technologies: ['swift'], targets: { ...own, ...shipped(product, at) }, release: { version: { versionActions: `${plugin.name}/release/version-actions`, versionActionsOptions: { manifest: INFO } } } };
    });
};

const packageEdges = (manifest: typeof Described.Type, described: ReadonlyMap<string, Package>): readonly RawProjectGraphDependency[] => {
    const modules = new Set(manifest.targets.filter((module) => module.product_memberships.length > 0).map((module) => module.name));
    const project = (module: string) => (modules.has(module) ? module : manifest.name);
    const linked = locals(manifest, described).map((local) => local.manifest);
    const products = (product: string) => linked.flatMap((local) => local.products.filter((owned) => owned.name === product).flatMap((owned) => owned.targets));
    return [...linked.map((local) => ({ source: manifest.name, target: local.name })), ...manifest.targets.flatMap((module) => [...module.target_dependencies.map(project), ...module.product_dependencies.flatMap(products)].map((dependency) => ({ source: project(module.name), target: dependency })))].map(
        (edge): RawProjectGraphDependency => ({ ...edge, type: DependencyType.implicit }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Described, INFO, MANIFEST, type Package, packageEdges, packageParts, sources };
