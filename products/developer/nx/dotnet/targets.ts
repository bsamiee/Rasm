import { basename, dirname, join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { type RawProjectGraphDependency, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { type PackageJson, quoteShellArg } from '@nx/devkit/internal';
import { PLATFORM, runner, type Verbs } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Context, Layer, Option, Record, Ref, Schema, Stream } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Node = typeof Node.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#package'))).name;
const CREDENTIAL = 'NUGET_API_KEY';

// --- [MODELS] --------------------------------------------------------------------------

const Mode = Schema.Struct({ configuration: Schema.String, outputs: Schema.Array(Schema.String), published: Schema.Array(Schema.String), reports: Schema.Array(Schema.String), documents: Schema.Array(Schema.String), packed: Schema.String });
const Node = Schema.Struct({
    file: Schema.String,
    name: Schema.String,
    language: Schema.String,
    platform: Schema.String,
    test: Schema.Boolean,
    packable: Schema.Boolean,
    publishable: Schema.Boolean,
    pushed: Schema.Boolean,
    evaluated: Mode,
    declared: Schema.Array(Mode),
    references: Schema.Array(Schema.Struct({ root: Schema.String, declared: Schema.String })),
    inputs: Schema.Array(Schema.String),
    bases: Schema.Array(Schema.String),
    packages: Schema.Array(Schema.Struct({ id: Schema.String, version: Schema.String, hash: Schema.String, direct: Schema.Boolean, dependencies: Schema.Array(Schema.String) })),
    referenced: Schema.Array(Schema.String),
    locks: Schema.Array(Schema.String),
    restored: Schema.Array(Schema.String),
    results: Schema.Array(Schema.String),
    platforms: Schema.Array(Schema.String),
    implicit: Schema.Array(Schema.String),
    external: Schema.Array(Schema.String),
    releases: Schema.NullOr(Schema.Struct({ shipped: Schema.String, unshipped: Schema.String })),
});

// --- [SERVICES] ------------------------------------------------------------------------

class Edges extends Context.Service<Edges, Ref.Ref<readonly RawProjectGraphDependency[]>>()(`${PLUGIN}/targets/Edges`) {
    static readonly layer = Layer.effect(Edges, Ref.make<readonly RawProjectGraphDependency[]>([]));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const metadata = (verb: string, description: string) => ({ description, help: { command: `dotnet ${verb} --help`, example: { args: ['--verbosity', 'detailed'] } } });

const written = (...modes: readonly { readonly outputs: readonly string[] }[]) => Array.makeBy(Math.max(0, ...modes.map(({ outputs }) => outputs.length)), (index) => `{options.outputs.${index}}`);

const targets = (node: Node, coverage: Option.Option<string>, solution: (configuration: string) => Option.Option<Readonly<Record<string, string>>>, listed: Option.Option<string>): Verbs => {
    const root = dirname(node.file);
    const project = join('{projectRoot}', basename(node.file));
    const inputs: NonNullable<TargetConfiguration['inputs']> = [
        'default',
        '^default',
        ...node.bases.filter((base) => base.startsWith(`${root}/`)).map((base) => `!${join('{projectRoot}', relative(root, base), '**')}`),
        { env: 'NUGET_PACKAGES' },
        { externalDependencies: [] },
        ...node.inputs.map((input) => join('{workspaceRoot}', input)),
        ...Option.toArray(listed).map((at) => ({
            runtime: line(['node', '-e', 'for (const file of JSON.parse(require("node:fs").readFileSync(process.argv[1], "utf8"))) { try { console.log(require("node:crypto").createHash("sha256").update(require("node:fs").readFileSync(file)).digest("hex")); } catch { console.log("absent"); } }', at]),
        })),
    ];
    const moded = <A extends object>(each: (mode: Node['evaluated']) => A) => ({ options: each(node.evaluated), configurations: Record.fromIterableWith(node.declared, (mode) => [mode.configuration.toLowerCase(), each(mode)] as const) });
    const verb = node.publishable ? 'publish' : 'build';
    const built = moded(({ configuration, outputs, published, reports, documents }) => ({
        project,
        command: [verb, '--configuration', configuration],
        outputs: [...outputs, ...published, ...reports, ...documents, ...node.restored],
        ...Record.getSomes({ solution: solution(configuration) }),
    }));
    const packed = moded(({ configuration, packed: folder }) => ({ project, command: ['pack', '--no-build', '-restore', '--configuration', configuration], outputs: [join(folder, '*.nupkg')] }));
    const tested = ({ configuration }: Node['evaluated'], ...flags: readonly string[]) => ({ commands: [{ command: line(['dotnet', 'restore', `-p:Configuration=${configuration}`]), forwardAllArgs: false }, line(['dotnet', 'test', '--no-build', '--configuration', configuration, ...flags])] });
    const coverable = node.referenced.some((id) => id.toLowerCase() === 'coverlet.mtp');
    const covered = { ...tested(node.evaluated, '--coverlet', '--coverlet-output-format', 'lcov', ...Option.toArray(coverage).flatMap((at) => ['--results-directory', relative(root, at)])), outputs: Option.toArray(coverage).map((at) => join('{workspaceRoot}', at)) };
    const cleaned = moded(({ configuration }) => ({ commands: [['dotnet', 'clean', node.file, '--configuration', configuration, '-p:BuildProjectReferences=false']] }));
    const formatted = (commands: Array.NonEmptyReadonlyArray<Array.NonEmptyReadonlyArray<string>>, description: string): TargetConfiguration => ({
        executor: `${PLUGIN}:format`,
        options: { commands, project },
        dependsOn: ['build--dotnet'],
        inputs,
        metadata: metadata(Array.headNonEmpty(Array.headNonEmpty(commands)), description),
    });
    const tools: Verbs = {
        build: {
            dotnet: {
                executor: `${PLUGIN}:build`,
                ...built,
                outputs: written(built.options, ...Object.values(built.configurations)),
                dependsOn: ['^build--dotnet'],
                cache: true,
                inputs,
                metadata: metadata(verb, `${node.publishable ? 'Builds and publishes' : 'Builds'} ${node.name} and its references in one MSBuild run with dotnet ${verb}, in configuration ${node.evaluated.configuration} unless the run names another the project declares`),
            },
        },
        pack: {
            ...(node.packable && {
                dotnet: {
                    executor: `${PLUGIN}:build`,
                    ...packed,
                    outputs: written(packed.options, ...Object.values(packed.configurations)),
                    dependsOn: ['build--dotnet'],
                    cache: true,
                    inputs,
                    metadata: metadata('pack', `Packs the build build--dotnet wrote for ${node.name} into its PackageOutputPath with dotnet pack --no-build, which compiles nothing`),
                },
            }),
        },
        check: {
            'dotnet-build': {
                executor: `${PLUGIN}:check`,
                ...moded(({ reports }) => ({ reports })),
                dependsOn: ['build--dotnet'],
                cache: true,
                inputs: [{ dependentTasksOutputFiles: '**/*.sarif', transitive: false }, { externalDependencies: [] }],
                metadata: metadata('build', `Fails on each warning or error in the SARIF report dotnet build wrote for ${node.name}, printed as file(line,col): level id: text`),
            },
            'dotnet-format': { ...formatted([['format', 'style', '--verify-no-changes']], `Fails on any code style finding in ${node.name}, the IDE rules no build runs included, with dotnet format style --verify-no-changes`), cache: true },
        },
        test: {
            ...(node.test && {
                dotnet: {
                    executor: 'nx:run-commands',
                    options: { parallel: false, cwd: '{projectRoot}', ...tested(node.evaluated) },
                    configurations: { ...moded(tested).configurations, ...(coverable && { coverage: covered }) },
                    dependsOn: ['build--dotnet'],
                    cache: true,
                    inputs: [...inputs, { dependentTasksOutputFiles: '**/*.runtimeconfig.json' }, PLATFORM],
                    outputs: [...node.results.map((results) => (relative(root, results).startsWith('..') ? join('{workspaceRoot}', results) : join('{projectRoot}', relative(root, results)))), join('{projectRoot}', '{options.results-directory}'), ...(coverable ? written(covered) : [])],
                    metadata: metadata('test', [`Tests the build build--dotnet wrote for ${node.name} with dotnet test --no-build, after a restore that fetches only packages the machine lacks`, ...(coverable ? ['configuration coverage adds --coverlet with an lcov report from coverlet.MTP'] : [])].join(', and ')),
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
                executor: `${distribution.name}:clean`,
                inputs,
                options: { ...cleaned.options, targets: Record.collect(tools, (named, held) => Record.keys(Record.filter(held, ({ outputs = [] }) => outputs.length > 0)).map((tool) => `${named}--${tool}`)).flat() },
                configurations: cleaned.configurations,
                metadata: metadata('clean', `Removes the build outputs of ${node.name} with dotnet clean`),
            },
        },
        attach: {
            ...(node.packable && {
                gh: {
                    executor: `${distribution.name}:attach`,
                    dependsOn: ['pack--dotnet'],
                    inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                    metadata: { description: "Uploads the files pack--dotnet wrote to the GitHub release of the version the project's manifest states with gh release upload", help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
                },
            }),
        },
    };
};

const publishing = (node: Node): Readonly<Record<string, TargetConfiguration>> =>
    node.packable && node.pushed
        ? {
              'nx-release-publish': {
                  executor: `${distribution.name}:publish`,
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

const run = runner(Edges.layer);

const streamed = <A, E>(results: Stream.Stream<A, E, NodeServices>) => Stream.toReadableStream(Stream.provide(results, layer)).values();

// --- [EXPORTS] -------------------------------------------------------------------------

export { Edges, line, Node, PLUGIN, publishing, run, streamed, targets };
