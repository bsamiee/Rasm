import { basename, dirname, posix } from 'node:path';
import { fileURLToPath } from 'node:url';
import type { RawProjectGraphDependency, TargetConfiguration } from '@nx/devkit';
import { quoteShellArg } from '@nx/devkit/internal';
import { below, EXECUTORS, PLATFORM, runner, tokened, type Verbs } from '@rasm/nx';
import { Array, Context, Layer, Option, Record, Ref, Schema } from 'effect';
import plugin from '#/package.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const CREDENTIAL = 'NUGET_API_KEY';
const ANALYZER = fileURLToPath(import.meta.resolve('#/analyzer/dist/MsbuildAnalyzer.dll'));
const ENVIRONMENT = { DOTNET_ROLL_FORWARD_TO_PRERELEASE: '1', DOTNET_DbgEnableMiniDump: '1', DOTNET_DbgMiniDumpType: '1', DOTNET_EnableCrashReport: '1' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Mode = Schema.Struct({ configuration: Schema.String, outputs: Schema.Array(Schema.String), published: Schema.Array(Schema.String), reports: Schema.Array(Schema.String), documents: Schema.Array(Schema.String), packed: Schema.String });
class Node extends Schema.Opaque<Node, unknown>()(
    Schema.Struct({
        file: Schema.String,
        name: Schema.String,
        language: Schema.String,
        platform: Schema.String,
        test: Schema.Boolean,
        packable: Schema.Boolean,
        publishable: Schema.Boolean,
        pushed: Schema.Boolean,
        versioned: Schema.Boolean,
        evaluated: Mode,
        declared: Schema.Array(Mode),
        references: Schema.Array(Schema.Struct({ root: Schema.String, declared: Schema.String })),
        inputs: Schema.Array(Schema.String),
        loaded: Schema.Array(Schema.String),
        catalogs: Schema.Array(Schema.String),
        bases: Schema.Array(Schema.String),
        packages: Schema.Array(Schema.Struct({ id: Schema.String, version: Schema.String, hash: Schema.String, direct: Schema.Boolean, dependencies: Schema.Array(Schema.String) })),
        referenced: Schema.Array(Schema.String),
        locks: Schema.Array(Schema.String),
        restored: Schema.Array(Schema.String),
        results: Schema.Array(Schema.String),
        platforms: Schema.Array(Schema.String),
        implicit: Schema.Array(Schema.String),
        tags: Schema.Array(Schema.String),
        environment: Schema.Array(Schema.String),
        external: Schema.Array(Schema.String),
        releases: Schema.NullOr(Schema.Struct({ shipped: Schema.String, unshipped: Schema.String })),
    }),
) {}

// --- [SERVICES] ------------------------------------------------------------------------

class Edges extends Context.Service<Edges, Ref.Ref<readonly RawProjectGraphDependency[]>>()(`${plugin.name}/targets/Edges`) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const metadata = (verb: string, description: string) => ({ description, help: { command: `dotnet ${verb} --help`, example: { args: ['--verbosity', 'detailed'] } } });

const written = (...modes: readonly { readonly outputs: readonly string[] }[]) => Array.makeBy(Math.max(0, ...modes.map(({ outputs }) => outputs.length)), (index) => `{options.outputs.${index}}`);

const targets = (node: Node, coverage: string | undefined, solution: (configuration: string) => Option.Option<Readonly<Record<string, string>>>, listed: Option.Option<string>): Verbs => {
    const root = dirname(node.file);
    const project = posix.join('{projectRoot}', basename(node.file));
    const hashed = (upstream: NonNullable<TargetConfiguration['inputs']>[number], files: readonly string[], ...hosts: NonNullable<TargetConfiguration['inputs']>): NonNullable<TargetConfiguration['inputs']> => [
        'default',
        upstream,
        ...node.bases.filter((base) => root !== '.' && below(root)(base)).map((base) => `!${posix.join('{projectRoot}', posix.relative(root, base), '**')}`),
        { runtime: 'dotnet --version' },
        ...node.environment.map((env) => ({ env })),
        ...hosts,
        { externalDependencies: [] },
        ...files.map((input) => posix.join('{workspaceRoot}', input)),
        ...Option.toArray(listed).map((at) => ({ runtime: `${line(['git', 'hash-object', '--no-filters', '--stdin-paths'])} < ${quoteShellArg(at)}` })),
    ];
    const [inputs, compiled, executed] = [hashed('^default', node.inputs), hashed('^default', node.inputs, { env: 'CI' }, PLATFORM), hashed({ dependentTasksOutputFiles: '**/!(*.sarif)', transitive: true }, node.loaded, { env: 'CI' }, PLATFORM)];
    const moded = <A extends object>(each: (mode: Node['evaluated']) => A) => ({ options: each(node.evaluated), configurations: Record.fromIterableWith(node.declared, (mode) => [mode.configuration.toLowerCase(), each(mode)] as const) });
    const verb = node.publishable ? 'publish' : 'build';
    const built = moded(({ configuration, outputs, published, reports, documents }) => ({
        project,
        command: [verb, '--configuration', configuration],
        outputs: [...outputs, ...published, ...reports, ...documents],
        ...Record.getSomes({ solution: solution(configuration) }),
    }));
    const packed = moded(({ configuration, packed: folder }) => ({ project, command: ['pack', '--no-build', '-restore', '--configuration', configuration], outputs: [posix.join(folder, '*.nupkg')] }));
    const tested = ({ configuration }: Node['evaluated'], ...flags: readonly string[]) => ({
        commands: [{ command: line(['dotnet', 'restore', '--no-dependencies', ...(node.locks.length > 0 ? ['--locked-mode'] : []), `-p:Configuration=${configuration}`]), forwardAllArgs: false }, line(['dotnet', 'test', '--no-build', '--configuration', configuration, ...flags])],
    });
    const coverable = node.referenced.some((id) => id.toLowerCase() === 'coverlet.mtp');
    const covered = {
        ...tested(node.declared.find(({ configuration }) => configuration.toLowerCase() === 'coverage') ?? node.evaluated, '--coverlet', '--coverlet-output-format', 'lcov', ...Array.fromNullishOr(coverage).flatMap((at) => ['--results-directory', posix.relative(root, at)])),
        outputs: Array.fromNullishOr(coverage).map((at) => posix.join('{workspaceRoot}', at)),
    };
    const formatted = (commands: Array.NonEmptyReadonlyArray<Array.NonEmptyReadonlyArray<string>>, description: string): TargetConfiguration => ({
        executor: `${plugin.name}:format`,
        options: { commands, project },
        dependsOn: ['build--dotnet'],
        inputs,
        metadata: metadata(Array.headNonEmpty(Array.headNonEmpty(commands)), description),
    });
    const tools: Verbs = {
        build: {
            dotnet: {
                executor: `${plugin.name}:build`,
                ...built,
                outputs: written(built.options, ...Object.values(built.configurations)),
                dependsOn: ['^build--dotnet'],
                cache: true,
                inputs: compiled,
                metadata: metadata(verb, `${node.publishable ? 'Builds and publishes' : 'Builds'} ${node.name} and its references in one MSBuild run with dotnet ${verb}, in configuration ${node.evaluated.configuration} unless the run names another the project declares`),
            },
        },
        pack: {
            ...(node.packable && {
                dotnet: {
                    executor: `${plugin.name}:build`,
                    ...packed,
                    outputs: written(packed.options, ...Object.values(packed.configurations)),
                    dependsOn: ['build--dotnet'],
                    cache: true,
                    inputs: compiled,
                    metadata: metadata('pack', `Packs the build build--dotnet wrote for ${node.name} into its PackageOutputPath with dotnet pack --no-build, which compiles nothing`),
                },
            }),
        },
        check: {
            'dotnet-build': {
                executor: `${plugin.name}:check`,
                ...moded(({ reports }) => ({ reports })),
                dependsOn: ['build--dotnet'],
                cache: true,
                inputs: compiled,
                metadata: metadata('build', `Fails on each warning or error in the SARIF report dotnet build wrote for ${node.name}, printed as file(line,col): level id: text, cached under the inputs of build--dotnet, as the report holds analyzer timing no input decides`),
            },
            'dotnet-format': { ...formatted([['format', 'style', '--verify-no-changes']], `Fails on each IDE code style finding in ${node.name}, those no code fix resolves included, with dotnet format style --verify-no-changes, as no build runs IDE rules`), cache: true },
        },
        test: {
            ...(node.test && {
                dotnet: {
                    executor: 'nx:run-commands',
                    options: { parallel: false, cwd: '{projectRoot}', ...tested(node.evaluated) },
                    configurations: { ...moded(tested).configurations, ...(coverable && { coverage: covered }) },
                    dependsOn: ['build--dotnet'],
                    cache: true,
                    inputs: executed,
                    outputs: [...node.results.map(tokened(root)), posix.join('{projectRoot}', '{options.results-directory}'), ...(coverable ? written(covered) : [])],
                    metadata: metadata(
                        'test',
                        [
                            `Tests the build build--dotnet wrote for ${node.name} with dotnet test --no-build, after a locked restore of the project alone that fetches only packages the machine lacks and writes no lock file, cached under the binaries the builds wrote and the files loading the project reads, as sources and compiler settings reach the tests through those binaries alone`,
                            ...(coverable ? ['configuration coverage adds --coverlet with an lcov report from coverlet.MTP'] : []),
                        ].join(', and '),
                    ),
                },
            }),
        },
        fix: {
            'dotnet-format': formatted(
                [
                    ['format', 'style'],
                    ['format', 'analyzers'],
                ],
                `Applies the code style and analyzer fixes of ${node.name} with dotnet format style and analyzers`,
            ),
        },
    };
    return {
        ...tools,
        clean: {
            dotnet: {
                executor: EXECUTORS.clean,
                inputs: compiled,
                options: { targets: Record.collect(tools, (named, held) => Record.keys(Record.filter(held, ({ outputs = [] }) => outputs.length > 0)).map((tool) => `${named}--${tool}`)).flat() },
                metadata: metadata('clean', `Removes the outputs every configuration of the build and pack targets of ${node.name} declares`),
            },
        },
        attach: {
            ...(node.packable && {
                gh: {
                    executor: EXECUTORS.attach,
                    dependsOn: ['pack--dotnet'],
                    inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                    metadata: {
                        description: "Uploads the files pack--dotnet wrote to the GitHub release of the version the project's manifest states with gh release upload, creating that release with gh release create where none exists",
                        help: { command: 'gh release upload --help', example: { options: { dryRun: true } } },
                    },
                },
            }),
        },
    };
};

const publishing = (node: Node): Readonly<Record<string, TargetConfiguration>> =>
    node.packable && node.pushed
        ? {
              'nx-release-publish': {
                  executor: EXECUTORS.publish,
                  options: { publish: ['dotnet', 'nuget', 'push', '--skip-duplicate'], credential: CREDENTIAL, flag: '--api-key' },
                  dependsOn: ['pack--dotnet', '^nx-release-publish'],
                  inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                  metadata: {
                      technologies: ['dotnet', node.language],
                      description: `Pushes the package pack--dotnet wrote for ${node.name} to the default push source NuGet resolves under the key ${CREDENTIAL} holds with dotnet nuget push --skip-duplicate, and names ${CREDENTIAL} and pushes nothing where it is unset`,
                      help: { command: 'dotnet nuget push --help', example: { options: { dryRun: true } } },
                  },
              },
          }
        : {};

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = runner(Layer.effect(Edges, Ref.make<readonly RawProjectGraphDependency[]>([])));

// --- [EXPORTS] -------------------------------------------------------------------------

export { ANALYZER, Edges, ENVIRONMENT, line, Node, publishing, run, targets };
