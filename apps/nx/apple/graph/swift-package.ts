import { DependencyType, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { isCI } from '@nx/devkit/internal';
import { Effect, Option, type Path, Record, Schema } from 'effect';
import { type FOLDERS, linted, type Options, type Part, target, tokened } from './targets.ts';

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

const sources = (path: Path.Path, workspaceRoot: string, root: string, held: typeof Described.Type, described: ReadonlyMap<string, typeof Described.Type>): readonly string[] => [
    ...held.targets.map((module) => path.join('{workspaceRoot}', root, module.path, '**/*')),
    ...[MANIFEST, 'Package@swift-*.swift', 'Package.resolved', '.swiftpm/configuration/*.json'].map((name) => path.join('{workspaceRoot}', root, name)),
    ...held.dependencies.flatMap(({ path: at }) =>
        Option.toArray(Option.flatMap(Option.fromUndefinedOr(at), (absolute) => Option.map(Option.fromUndefinedOr(described.get(absolute)), (local) => ({ local, absolute })))).flatMap(({ local, absolute }) =>
            sources(path, workspaceRoot, path.relative(workspaceRoot, absolute) || '.', local, described).filter((input) => !input.endsWith('Package.resolved')),
        ),
    ),
];

const packageParts = (
    path: Path.Path,
    workspaceRoot: string,
    placed: (root: string) => Option.Option<Readonly<Record<keyof typeof FOLDERS, string>>>,
    options: Options,
    file: string,
    versioned: readonly string[],
    held: typeof Described.Type,
    described: ReadonlyMap<string, typeof Described.Type>,
): readonly Part[] => {
    const root = path.dirname(file);
    const folders = placed(root);
    const modules = held.targets.filter(({ name, product_memberships }) => product_memberships.length > 0 && name !== held.name);
    const env = Option.match(folders, { onNone: () => ({}), onSome: ({ scratch }) => ({ env: { SWIFTPM_BUILD_DIR: scratch } }) });
    const locked = Boolean(isCI()) ? ['--disable-automatic-resolution'] : [];
    const suites = held.targets.filter(({ type }) => Schema.is(Tests)(type));
    const tested = new Set(suites.flatMap(({ target_dependencies }) => target_dependencies));
    const results = Option.toArray(folders).flatMap(({ results: at }) => ['xunit.xml', 'xunit-swift-testing.xml'].map((name) => path.join(at, name)));
    const work: Readonly<Record<string, TargetConfiguration>> = {
        build: target([{ command: ['swift', 'build', '--package-path', root, ...locked], ...env }]),
        ...(suites.length === 0
            ? {}
            : {
                  test: target([{ command: ['swift', 'test', '--package-path', root, ...locked, ...results.slice(0, 1).flatMap((at) => ['--xunit-output', at])], ...env, results }], {
                      cache: true,
                      dependsOn: ['build'],
                      inputs: [...sources(path, workspaceRoot, root, held, described), { runtime: 'swift --version' }],
                      outputs: results.flatMap((at) => tokened(path, at)),
                  }),
              }),
    };
    const lintable = [path.join(root, MANIFEST), ...versioned, ...held.targets.filter((module) => !modules.includes(module)).map((module) => path.join(root, module.path))];
    return [
        { root, name: held.name, file, technologies: ['swift'], targets: { ...work, ...linted(path, root, lintable) } },
        ...modules.map((module): Part => {
            const at = path.join(root, module.path);
            return {
                root: at,
                name: module.name,
                file,
                technologies: ['swift'],
                targets: {
                    ...Record.map(
                        Record.filter(work, (_, name) => name === 'build' || tested.has(module.name)),
                        (_, name): TargetConfiguration => ({ executor: 'nx:noop', dependsOn: [{ projects: [held.name], target: name }] }),
                    ),
                    ...linted(path, at, [at]),
                },
            };
        }),
    ];
};

const packageEdges = (held: typeof Described.Type, described: ReadonlyMap<string, typeof Described.Type>): readonly RawProjectGraphDependency[] => {
    const modules = new Set(held.targets.filter(({ product_memberships }) => product_memberships.length > 0).map(({ name }) => name));
    const project = (module: string) => (modules.has(module) ? module : held.name);
    const locals = held.dependencies.flatMap(({ path: at }) => Option.toArray(Option.flatMap(Option.fromUndefinedOr(at), (absolute) => Option.fromUndefinedOr(described.get(absolute)))));
    const products = (name: string) => locals.flatMap((local) => local.products.filter((product) => product.name === name).flatMap((product) => product.targets));
    return [
        ...locals.map((local) => ({ source: held.name, target: local.name })),
        ...held.targets.flatMap(({ name, target_dependencies, product_dependencies }) =>
            [...target_dependencies.map(project), ...product_dependencies.flatMap(products)].map((module) => ({ source: project(name), target: module })),
        ),
    ].map((edge): RawProjectGraphDependency => ({ ...edge, type: DependencyType.implicit }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Described, MANIFEST, packageEdges, packageParts, sources };
