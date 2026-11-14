import { basename, dirname, join, relative } from 'node:path';
import type { TargetConfiguration } from '@nx/devkit';
import { quoteShellArg } from '@nx/devkit/internal';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Node = typeof Node.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = '@rasm/nx-dotnet';

// --- [MODELS] --------------------------------------------------------------------------

const Mode = Schema.Struct({ configuration: Schema.String, outputs: Schema.Array(Schema.String), published: Schema.Array(Schema.String), reports: Schema.Array(Schema.String), packed: Schema.String });
const Node = Schema.Struct({
    file: Schema.String,
    name: Schema.String,
    language: Schema.String,
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
    locks: Schema.Array(Schema.String),
    restored: Schema.Array(Schema.String),
    results: Schema.Array(Schema.String),
    platforms: Schema.Array(Schema.String),
    implicit: Schema.Array(Schema.String),
    releases: Schema.NullOr(Schema.Struct({ shipped: Schema.String, unshipped: Schema.String })),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const metadata = (verb: string, description: string) => ({ description, help: { command: `dotnet ${verb} --help`, example: { args: ['--verbosity', 'detailed'] } } });

const targets = (node: Node): Record<string, TargetConfiguration> => {
    const root = dirname(node.file);
    const project = join('{projectRoot}', basename(node.file));
    const inputs: NonNullable<TargetConfiguration['inputs']> = [
        'default',
        '^default',
        ...node.inputs.map((input) => join('{workspaceRoot}', input)),
        ...node.bases.filter((base) => base.startsWith(`${root}/`)).map((base) => `!${join('{projectRoot}', relative(root, base), '**')}`),
        { env: 'NUGET_PACKAGES' },
        { externalDependencies: [] },
    ];
    const moded = <A extends object>(each: (mode: Node['evaluated']) => A) => ({ options: each(node.evaluated), configurations: Record.fromIterableWith(node.declared, (mode) => [mode.configuration.toLowerCase(), each(mode)] as const) });
    const written = (...modes: readonly { readonly outputs: readonly string[] }[]) => Array.makeBy(Math.max(0, ...modes.map(({ outputs }) => outputs.length)), (index) => `{options.outputs.${index}}`);
    const verb = node.publishable ? 'publish' : 'build';
    const built = moded(({ configuration, outputs, published, reports }) => ({ project, command: [verb, '--configuration', configuration], outputs: [...outputs, ...published, ...reports, ...node.restored] }));
    const packed = moded(({ configuration, packed: folder }) => ({ project, command: ['pack', '--no-build', '-restore', '--configuration', configuration], outputs: [join(folder, '*.nupkg')] }));
    const tested = moded(({ configuration }) => ({ commands: [{ command: line(['dotnet', 'restore', `-p:Configuration=${configuration}`]), forwardAllArgs: false }, line(['dotnet', 'test', '--no-build', '--configuration', configuration])] }));
    const cleaned = moded(({ configuration }) => ({ commands: [['dotnet', 'clean', node.file, '--configuration', configuration, '-p:BuildProjectReferences=false']] }));
    const formatted = (commands: Array.NonEmptyReadonlyArray<Array.NonEmptyReadonlyArray<string>>, description: string): TargetConfiguration => ({
        executor: `${PLUGIN}:format`,
        options: { commands, project },
        dependsOn: ['build--dotnet'],
        inputs,
        metadata: metadata(Array.headNonEmpty(Array.headNonEmpty(commands)), description),
    });
    const tools: Record<string, TargetConfiguration> = {
        'build--dotnet': {
            executor: `${PLUGIN}:build`,
            ...built,
            outputs: written(built.options, ...Object.values(built.configurations)),
            dependsOn: ['^build--dotnet'],
            cache: true,
            inputs,
            metadata: metadata(verb, `${node.publishable ? 'Builds and publishes' : 'Builds'} ${node.name} with its references in one MSBuild run, in configuration ${node.evaluated.configuration} unless the run names another the project declares`),
        },
        ...(node.packable && {
            'pack--dotnet': {
                executor: `${PLUGIN}:build`,
                ...packed,
                outputs: written(packed.options, ...Object.values(packed.configurations)),
                dependsOn: ['build--dotnet'],
                cache: true,
                inputs,
                metadata: metadata('pack', `Packs the build build--dotnet wrote for ${node.name} into its PackageOutputPath with dotnet pack --no-build, which compiles nothing`),
            },
        }),
        'check--dotnet-build': {
            executor: `${PLUGIN}:check`,
            ...moded(({ reports }) => ({ reports })),
            dependsOn: ['build--dotnet'],
            cache: true,
            inputs: [{ dependentTasksOutputFiles: '**/*.sarif', transitive: false }, { externalDependencies: [] }],
            metadata: { description: `Fails on each warning or error in the SARIF report the compile of ${node.name} wrote, printed as file(line,col): level id: text` },
        },
        'check--dotnet-format': { ...formatted([['format', 'style', '--verify-no-changes']], `Fails on any code style finding in ${node.name}, the IDE rules no build runs included, with dotnet format style --verify-no-changes`), cache: true },
        ...(node.test && {
            'test--dotnet': {
                executor: 'nx:run-commands',
                options: { parallel: false, cwd: '{projectRoot}', ...tested.options },
                configurations: tested.configurations,
                dependsOn: ['build--dotnet'],
                cache: true,
                inputs: [...inputs, { dependentTasksOutputFiles: '**/*.runtimeconfig.json' }, { runtime: 'node -p process.platform+process.arch' }],
                outputs: node.results.map((results) => join('{workspaceRoot}', results)),
                metadata: metadata('test', `Tests the build build--dotnet wrote for ${node.name} with dotnet test --no-build, after a restore that fetches only packages the machine lacks`),
            },
        }),
        'fix--dotnet-format': formatted(
            [
                ['format', 'style'],
                ['format', 'analyzers'],
            ],
            `Applies the code style and analyzer fixes of ${node.name} with dotnet format style and analyzers`,
        ),
    };
    const delivered: Record<string, TargetConfiguration> = {
        ...tools,
        'clean--dotnet': {
            executor: `${distribution.name}:clean`,
            inputs,
            options: { ...cleaned.options, targets: Record.keys(Record.filter(tools, ({ outputs = [] }) => outputs.length > 0)) },
            configurations: cleaned.configurations,
            metadata: metadata('clean', `Removes the build outputs of ${node.name} with dotnet clean`),
        },
        ...('pack--dotnet' in tools && {
            'attach--gh': {
                executor: `${distribution.name}:attach`,
                dependsOn: ['pack--dotnet'],
                inputs: [{ dependentTasksOutputFiles: '**/*', transitive: true }],
                metadata: { description: "Uploads the files pack--dotnet wrote to the GitHub release of the version the project's manifest states through gh release upload", help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
            },
            ...(node.pushed && {
                'nx-release-publish': {
                    executor: `${distribution.name}:publish`,
                    options: { publish: ['dotnet', 'nuget', 'push', '--skip-duplicate'], credential: 'NUGET_API_KEY', flag: '--api-key' },
                    dependsOn: ['pack--dotnet', '^nx-release-publish'],
                    inputs: [{ dependentTasksOutputFiles: '**/*', transitive: true }],
                    metadata: {
                        description: `Pushes the package pack--dotnet wrote for ${node.name} with dotnet nuget push, under the key NUGET_API_KEY holds, to the default push source NuGet resolves, which skips a version the source holds`,
                        help: { command: 'dotnet nuget push --help', example: { options: { dryRun: true } } },
                    },
                },
            }),
        }),
    };
    return Record.map(delivered, (tool): TargetConfiguration => ({ ...tool, metadata: { technologies: ['dotnet', node.language], ...tool.metadata } }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Node, PLUGIN, targets };
