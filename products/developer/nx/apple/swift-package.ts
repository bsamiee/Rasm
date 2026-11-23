import { dirname, join, relative } from 'node:path';
import { DependencyType, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import type { Verbs } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Effect, Option, Record, Schema } from 'effect';
import { ATTACH, INPUTS, type Options, type Part, PLUGIN, SWIFTPM, sequenced } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

interface Package {
    readonly file: string;
    readonly root: string;
    readonly plists: ReadonlyMap<string, string>;
    readonly entitled: ReadonlyMap<string, string>;
    readonly bins: Readonly<Record<'debug' | 'release' | 'codecov', string>>;
    readonly whole: boolean;
    readonly manifest: typeof Described.Type;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'Package.swift';
const INFO = 'Info.plist';
const ENTITLEMENTS = '.entitlements';
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

const sources = ({ root, manifest }: Package, described: ReadonlyMap<string, Package>): readonly string[] => [
    ...manifest.targets.map((module) => join('{workspaceRoot}', root, module.path, '**/*')),
    ...[MANIFEST, 'Package@swift-*.swift', RESOLVED, '.swiftpm/configuration/*.json'].map((file) => join('{workspaceRoot}', root, file)),
    ...locals(manifest, described)
        .flatMap((local) => sources(local, described))
        .filter((input) => !input.endsWith(RESOLVED)),
];

const packageParts = (options: typeof Options.Type, local: Package, described: ReadonlyMap<string, Package>): readonly Part[] => {
    const { file, root, plists, entitled, manifest, bins, whole } = local;
    const modules = manifest.targets.filter((module) => module.product_memberships.length > 0 && module.name !== manifest.name);
    const suites = manifest.targets.filter(({ type }) => Schema.is(Tests)(type));
    const tested = new Set(suites.flatMap(({ target_dependencies }) => target_dependencies));
    const inputs = [...sources(local, described), join('{workspaceRoot}', '.swift-version'), ...INPUTS];
    const build = ['swift', 'build', '--package-path', root, ...SWIFTPM] as const;
    const test = ['swift', 'test', '--package-path', root, ...SWIFTPM] as const;
    const lcov = Option.map(Option.fromUndefinedOr(options.coveragePath), (folder) => join(folder, 'lcov.info'));
    const covered = [bins.codecov, ...Option.toArray(lcov)];
    const written = (folder: string) => (whole ? manifest.products.map(({ name }) => join('{workspaceRoot}', folder, name)) : []);
    const report = Option.map(lcov, (at) => ({ report: { profiles: [join(dirname(bins.codecov), 'default.profdata')], objects: Array.map(suites, ({ name }) => join(bins.debug, `${name}.xctest`, 'Contents', 'MacOS', name)), file: at } }));
    const coverage = { runs: [[...test, '--enable-code-coverage']], outputs: covered.map((at) => join('{workspaceRoot}', at)), ...Option.getOrUndefined(report) };
    const testing = { cache: true, dependsOn: ['build--swift-build'], inputs, outputs: covered.map((_, index) => `{options.outputs.${index}}`), configurations: { coverage } };
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
                    outputs: written(bins.debug).map((_, index) => `{options.outputs.${index}}`),
                    configurations: { release: { runs: [[...build, '--configuration', 'release']], outputs: written(bins.release) } },
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
    const forwardedInputs = ['default', '^default', ...[MANIFEST, RESOLVED].map((at) => join('{workspaceRoot}', root, at)), ...INPUTS];
    const owned: Verbs = {
        ...work,
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
    const cleaning: TargetConfiguration<Readonly<Record<string, unknown>>> = {
        inputs,
        options: { commands: [['swift', 'package', '--package-path', root, 'clean']], cwd: '.' },
        metadata: { technologies: ['swift'], description: `Removes the build folder of ${manifest.name} with swift package clean`, help: { command: 'swift package clean --help', example: {} } },
    };
    const forwarded = (module: string): Verbs =>
        Record.map(
            Record.filter(work, (_, verb) => verb !== 'test' || tested.has(module)),
            (tools, verb) =>
                Record.map(tools, (_, tool): TargetConfiguration => ({
                    dependsOn: [{ projects: [manifest.name], target: `${verb}--${tool}` }],
                    inputs: forwardedInputs,
                    metadata: { technologies: ['swift'], description: `Runs ${verb}--${tool} of ${manifest.name} with nx run`, help: { command: `nx run ${manifest.name}:${verb}--${tool} --help`, example: { args: ['--verbose'] } } },
                })),
        );
    const relayed: TargetConfiguration<Readonly<Record<string, unknown>>> = {
        dependsOn: [{ projects: [manifest.name], target: 'clean--swift-package' }],
        inputs: forwardedInputs,
        metadata: { technologies: ['swift'], description: `Runs clean--swift-package of ${manifest.name} with nx run`, help: { command: `nx run ${manifest.name}:clean--swift-package --help`, example: { args: ['--verbose'] } } },
    };
    return [{ name: manifest.name, at: root, own: owned, clean: cleaning }, ...modules.map((module) => ({ name: module.name, at: join(root, module.path), own: forwarded(module.name), clean: relayed }))].map(({ name, at, own, clean }): Part => {
        const product = Option.fromUndefinedOr(products.get(name));
        const signing = Option.getOrUndefined(Option.map(Option.all([product, Option.fromUndefinedOr(entitled.get(name))]), ([, entitlements]) => ({ entitlements })));
        const zipped = Option.map(Option.all({ held: product, plist: Option.fromUndefinedOr(plists.get(name)) }), ({ held, plist }) => ({ held, plist, folder: options.outputPath === undefined ? bins.release : join(options.outputPath, held) }));
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
                            dependsOn: ['build--swift-build'],
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
        const removed = Record.collect(tools, (verb, held) => Record.keys(Record.filter(held, ({ outputs = [] }) => outputs.length > 0)).map((tool) => `${verb}--${tool}`)).flat();
        return {
            root: at,
            name,
            file,
            technologies: ['swift'],
            tools: { ...tools, clean: { 'swift-package': { ...clean, executor: `${distribution.name}:clean`, options: { ...clean.options, targets: removed } } } },
            ...Option.getOrUndefined(Option.map(zipped, ({ plist }) => ({ release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { manifest: relative(at, plist) } } } }))),
        };
    });
};

const packageEdges = ({ file, manifest }: Package, described: ReadonlyMap<string, Package>): readonly RawProjectGraphDependency[] => {
    const modules = new Set(manifest.targets.filter((module) => module.product_memberships.length > 0).map((module) => module.name));
    const project = (module: string) => (modules.has(module) ? module : manifest.name);
    const linked = locals(manifest, described).map((local) => local.manifest);
    const products = (product: string) => linked.flatMap((local) => local.products.filter((owned) => owned.name === product).flatMap((owned) => owned.targets));
    return Array.dedupeWith(
        [
            ...linked.map((local): RawProjectGraphDependency => ({ source: manifest.name, target: local.name, type: DependencyType.static, sourceFile: file })),
            ...manifest.targets.flatMap((module) => [...module.target_dependencies.map(project), ...module.product_dependencies.flatMap(products)].map((dependency): RawProjectGraphDependency => ({ source: project(module.name), target: dependency, type: DependencyType.implicit }))),
        ],
        (left, right) => left.source === right.source && left.target === right.target,
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Described, ENTITLEMENTS, INFO, MANIFEST, type Package, packageEdges, packageParts, RESOLVED, sources };
