import { join } from 'node:path';
import { DependencyType, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { Array, Effect, Option, Record, Schema } from 'effect';
import { ATTACH, GIT_STATUS, INPUTS, type Options, type Part, PLUGIN, SHARED, target } from '#/targets';

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

const sources = ({ root, manifest }: Package, described: ReadonlyMap<string, Package>): readonly string[] => [
    ...manifest.targets.map((module) => join('{workspaceRoot}', root, module.path, '**/*')),
    ...[MANIFEST, 'Package@swift-*.swift', RESOLVED, '.swiftpm/configuration/*.json'].map((file) => join('{workspaceRoot}', root, file)),
    ...locals(manifest, described)
        .flatMap((local) => sources(local, described))
        .filter((input) => !input.endsWith(RESOLVED)),
];

const packageParts = (options: typeof Options.Type, local: Package, described: ReadonlyMap<string, Package>): readonly Part[] => {
    const { file, root, plists, manifest, bins } = local;
    const modules = manifest.targets.filter((module) => module.product_memberships.length > 0 && module.name !== manifest.name);
    const suites = manifest.targets.filter(({ type }) => Schema.is(Tests)(type));
    const tested = new Set(suites.flatMap(({ target_dependencies }) => target_dependencies));
    const inputs = [...sources(local, described), join('{workspaceRoot}', '.swift-version'), ...INPUTS];
    const pinned = '--force-resolved-versions';
    const build = ['swift', 'build', '--package-path', root, pinned] as const;
    const written = (folder?: string) => (folder === undefined ? [] : manifest.products.map(({ name }) => join(folder, name))).map((at) => join('{workspaceRoot}', at));
    const work: Record<string, TargetConfiguration> = {
        'build--swift-build': target(
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
        'check--swift-build': { dependsOn: ['build--swift-build'], inputs, metadata: { technologies: ['swift'], description: `Compiles ${manifest.name} as its type check` } },
        ...(suites.length > 0 && {
            'test--swift': target(
                [['swift', 'test', '--package-path', root, pinned]],
                { technologies: ['swift'], description: `Tests ${manifest.name} with SwiftPM`, help: { command: 'swift test --help', example: { args: ['--filter', suites[0]?.name ?? manifest.name] } } },
                { cache: true, dependsOn: ['build--swift-build'], inputs },
            ),
        }),
    };
    const executables = new Set(manifest.targets.filter(({ type }) => Schema.is(Executable)(type)).map(({ name }) => name));
    const products = new Map(manifest.products.flatMap(({ name, targets }) => targets.filter((owner) => executables.has(owner) && plists.has(owner)).map((owner) => [owner, name] as const)));
    const forwardedInputs = ['default', '^default', ...[MANIFEST, RESOLVED].map((at) => join('{workspaceRoot}', root, at)), ...INPUTS];
    const owned: Record<string, TargetConfiguration> = {
        ...work,
        'upgrade--swift-package': target(
            [['swift', 'package', '--package-path', root, 'update']],
            { technologies: ['swift'], description: `Moves ${manifest.name}'s remote dependencies to the newest versions its requirements allow and rewrites Package.resolved`, help: { command: 'swift package update --help', example: { args: ['--dry-run'] } } },
            { inputs },
        ),
    };
    const cleaning: TargetConfiguration = {
        options: { commands: [['swift', 'package', '--package-path', root, 'clean']], cwd: '.' },
        metadata: { technologies: ['swift'], description: `Removes the build folder of ${manifest.name} with swift package clean`, help: { command: 'swift package clean --help', example: {} } },
    };
    const forwarded = (module: string) =>
        Record.map(
            Record.filter(work, (_, name) => name !== 'test--swift' || tested.has(module)),
            (_, name): TargetConfiguration => ({ dependsOn: [{ projects: [manifest.name], target: name }], inputs: forwardedInputs, metadata: { technologies: ['swift'], description: `Runs ${manifest.name}:${name}` } }),
        );
    return [{ name: manifest.name, at: root, own: owned, clean: Option.some(cleaning) }, ...modules.map((module) => ({ name: module.name, at: join(root, module.path), own: forwarded(module.name), clean: Option.none<TargetConfiguration>() }))].map(({ name, at, own, clean }): Part => {
        const product = Option.fromUndefinedOr(products.get(name));
        const zipped = Option.map(Option.all({ held: product, output: Option.fromUndefinedOr(options.outputPath) }), ({ held, output }) => ({ held, folder: join(output, 'swiftpm', held) }));
        const tools: Record<string, TargetConfiguration> = {
            ...own,
            ...Option.getOrUndefined(
                Option.map(product, (held) => ({
                    'install--swift-package': {
                        executor: `${PLUGIN}:install`,
                        options: { package: root, product: held, env: GIT_STATUS, args: [pinned] },
                        metadata: { technologies: ['swift'], description: `Builds ${held} in release and replaces it in SwiftPM's install folder`, help: { command: 'swift package experimental-install --help', example: { args: ['--jobs', '4'] } } },
                    },
                })),
            ),
            ...Option.getOrUndefined(
                Option.map(zipped, ({ held, folder }) => ({
                    'pack--ditto': {
                        executor: `${PLUGIN}:package`,
                        dependsOn: ['build--swift-build'],
                        inputs: [join('{workspaceRoot}', at, INFO), ...INPUTS],
                        options: { products: [{ package: root, product: held, folder }] },
                        outputs: [join('{workspaceRoot}', folder, `${held}.zip`)],
                        metadata: { technologies: ['swift'], description: `Zips the release build of ${held} with ditto for a GitHub release`, help: { command: 'man ditto', example: { args: ['--norsrc'] } } },
                    },
                    'attach--gh': ATTACH,
                })),
            ),
        };
        const removed = Record.keys(Record.filter(tools, ({ outputs = [] }) => outputs.length > 0));
        const cleaned = Option.orElse(clean, () => Option.liftPredicate<TargetConfiguration>({ metadata: { technologies: ['swift'], description: `Removes the outputs ${removed.join(', ')} declare` } }, () => removed.length > 0));
        return {
            root: at,
            name,
            file,
            technologies: ['swift'],
            targets: { ...tools, ...Option.getOrUndefined(Option.map(cleaned, (stated) => ({ 'clean--swift-package': { ...stated, executor: `${SHARED}:clean`, options: { ...stated.options, targets: removed } } }))) },
            ...Option.getOrUndefined(Option.map(zipped, () => ({ release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { manifest: INFO } } } }))),
        };
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
