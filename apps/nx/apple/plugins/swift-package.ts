import { DependencyType, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { isCI } from '@nx/devkit/internal';
import { Effect, Option, type Path, Predicate, Record, Schema } from 'effect';
import { type Part, TOOLCHAIN, target } from '#/plugins/targets';

// --- [TYPES] ---------------------------------------------------------------------------

interface Package {
    readonly file: string;
    readonly root: string;
    readonly held: typeof Described.Type;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'Package.swift';

// --- [MODELS] --------------------------------------------------------------------------

const Listed = Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));
const Tests = Schema.Literal('test');
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

const locals = (held: typeof Described.Type, described: ReadonlyMap<string, Package>) => held.dependencies.flatMap((dependency) => Option.toArray(Option.flatMap(Option.fromUndefinedOr(dependency.path), (at) => Option.fromUndefinedOr(described.get(at)))));

const sources = (path: Path.Path, { root, held }: Package, described: ReadonlyMap<string, Package>): readonly string[] => [
    ...held.targets.map((module) => path.join('{workspaceRoot}', root, module.path, '**/*')),
    ...[MANIFEST, 'Package@swift-*.swift', 'Package.resolved', '.swiftpm/configuration/*.json'].map((file) => path.join('{workspaceRoot}', root, file)),
    ...locals(held, described)
        .flatMap((local) => sources(path, local, described))
        .filter((input) => !input.endsWith('Package.resolved')),
];

const packageParts = (path: Path.Path, plugin: string, local: Package, described: ReadonlyMap<string, Package>): readonly Part[] => {
    const { file, root, held } = local;
    const modules = held.targets.filter((module) => module.product_memberships.length > 0 && module.name !== held.name);
    const flags = Predicate.isTruthy(isCI()) ? ['--disable-automatic-resolution'] : [];
    const suites = held.targets.filter(({ type }) => Schema.is(Tests)(type));
    const tested = new Set(suites.flatMap(({ target_dependencies }) => target_dependencies));
    const inputs = [...sources(path, local, described), path.join('{workspaceRoot}', '.swift-version'), ...TOOLCHAIN];
    const work: Readonly<Record<string, TargetConfiguration>> = {
        'build--swift-build': target(plugin, [['swift', 'build', '--package-path', root, ...flags]], { technologies: ['swift'], description: `Builds ${held.name} with SwiftPM`, help: { command: 'swift build --help', example: { args: ['--configuration', 'release'] } } }, { inputs }),
        'check--swift-build': { executor: 'nx:noop', dependsOn: ['build--swift-build'], inputs, metadata: { technologies: ['swift'], description: `Compiles ${held.name} as its type check` } },
        ...(suites.length === 0
            ? {}
            : {
                  test: target(
                      plugin,
                      [['swift', 'test', '--package-path', root, ...flags]],
                      { technologies: ['swift'], description: `Tests ${held.name} with SwiftPM`, help: { command: 'swift test --help', example: { args: ['--filter', suites[0]?.name ?? held.name] } } },
                      { cache: true, dependsOn: ['build--swift-build'], inputs },
                  ),
              }),
    };
    const forwarded = (module: string) =>
        Record.map(
            Record.filter(work, (_, verb) => verb !== 'test' || tested.has(module)),
            (_, verb): TargetConfiguration => ({ executor: 'nx:noop', dependsOn: [{ projects: [held.name], target: verb }], inputs, metadata: { technologies: ['swift'], description: `Runs ${held.name}:${verb}` } }),
        );
    return [{ root, name: held.name, file, technologies: ['swift'], targets: work }, ...modules.map((module): Part => ({ root: path.join(root, module.path), name: module.name, file, technologies: ['swift'], targets: forwarded(module.name) }))];
};

const packageEdges = (held: typeof Described.Type, described: ReadonlyMap<string, Package>): readonly RawProjectGraphDependency[] => {
    const modules = new Set(held.targets.filter((module) => module.product_memberships.length > 0).map((module) => module.name));
    const project = (module: string) => (modules.has(module) ? module : held.name);
    const linked = locals(held, described).map((local) => local.held);
    const products = (product: string) => linked.flatMap((local) => local.products.filter((owned) => owned.name === product).flatMap((owned) => owned.targets));
    return [...linked.map((local) => ({ source: held.name, target: local.name })), ...held.targets.flatMap((module) => [...module.target_dependencies.map(project), ...module.product_dependencies.flatMap(products)].map((dependency) => ({ source: project(module.name), target: dependency })))].map(
        (edge): RawProjectGraphDependency => ({ ...edge, type: DependencyType.implicit }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Described, MANIFEST, type Package, packageEdges, packageParts, sources };
