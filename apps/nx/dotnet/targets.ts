import { basename, dirname, join, relative } from 'node:path';
import type { TargetConfiguration } from '@nx/devkit';
import { quoteShellArg } from '@nx/devkit/internal';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Option, Record, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Node = typeof Node.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = '@rasm/nx-dotnet';

// --- [MODELS] --------------------------------------------------------------------------

const Node = Schema.Struct({
    file: Schema.String,
    name: Schema.String,
    language: Schema.String,
    test: Schema.Boolean,
    packable: Schema.Boolean,
    publishable: Schema.Boolean,
    configuration: Schema.String,
    modes: Schema.Array(Schema.Struct({ configuration: Schema.String, runtime: Schema.NullOr(Schema.String), outputs: Schema.Array(Schema.String), published: Schema.Array(Schema.String), reports: Schema.Array(Schema.String) })),
    references: Schema.Array(Schema.Struct({ root: Schema.String, declared: Schema.String })),
    inputs: Schema.Array(Schema.String),
    bases: Schema.Array(Schema.String),
    packages: Schema.Array(Schema.Struct({ id: Schema.String, version: Schema.String, hash: Schema.String, direct: Schema.Boolean, dependencies: Schema.Array(Schema.String) })),
    locks: Schema.Array(Schema.String),
    restored: Schema.Array(Schema.String),
    results: Schema.Array(Schema.String),
    platforms: Schema.Array(Schema.String),
    implicit: Schema.Array(Schema.String),
    packed: Schema.String,
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
    const modes = node.modes.map((mode) => {
        const runtime = mode.runtime === null ? [] : ['--runtime', mode.runtime];
        return {
            name: [mode.configuration.toLowerCase(), ...Array.fromNullishOr(mode.runtime)].join('_'),
            configuration: mode.configuration,
            args: ['--configuration', mode.configuration, ...runtime],
            restored: [...runtime, `-p:Configuration=${mode.configuration}`],
            verb: Array.isReadonlyArrayNonEmpty(mode.published) ? 'publish' : 'build',
            outputs: [...mode.outputs, ...mode.published, ...mode.reports, ...node.restored],
            reports: mode.reports,
            primary: mode.configuration === (node.publishable ? 'Release' : node.configuration) && mode.runtime === null,
        };
    });
    const moded = <A extends object>(each: (mode: (typeof modes)[number]) => A) => ({
        options: Option.getOrElse(Option.map(Array.findFirst(modes, Struct.get('primary')), each), (): Partial<A> => ({})),
        configurations: Record.fromIterableWith(modes, (mode) => [mode.name, each(mode)] as const),
    });
    const built = moded(({ verb, args, outputs }) => ({ command: [verb, ...args], outputs }));
    const tested = moded(({ args, restored }) => ({ commands: [{ command: line(['dotnet', 'restore', ...restored]), forwardAllArgs: false }, line(['dotnet', 'test', '--no-build', ...args])] }));
    const cleaned = moded(({ args }) => ({ commands: [['dotnet', 'clean', node.file, ...args, '-p:BuildProjectReferences=false']] }));
    const released = Option.filter(
        Option.orElse(
            Array.findFirst(modes, ({ configuration, name }) => configuration === 'Release' && name === 'release'),
            () => Array.findFirst(modes, Struct.get('primary')),
        ),
        () => node.packable,
    );
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
            options: { project, ...built.options },
            configurations: built.configurations,
            outputs: Array.makeBy(Math.max(0, ...modes.map(({ outputs }) => outputs.length)), (index) => `{options.outputs.${index}}`),
            dependsOn: ['^build--dotnet'],
            cache: true,
            inputs,
            metadata: metadata(node.publishable ? 'publish' : 'build', node.publishable ? `Builds ${node.name} with its references in one MSBuild run, publishing it in every configuration, Release unless a configuration is named` : `Builds ${node.name} with its references in one MSBuild run`),
        },
        ...Record.getSomes({
            'pack--dotnet': Option.map(
                released,
                ({ name, args }): TargetConfiguration => ({
                    command: line(['dotnet', 'pack', node.file, ...args]),
                    configurations: { [name]: {} },
                    defaultConfiguration: name,
                    dependsOn: ['build--dotnet'],
                    cache: true,
                    inputs,
                    outputs: [join('{workspaceRoot}', node.packed, '*.nupkg')],
                    metadata: metadata('pack', `Packs the ${name} build of ${node.name} into its PackageOutputPath with dotnet pack, which builds that configuration first`),
                }),
            ),
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
                inputs: [...inputs, { dependentTasksOutputFiles: '**/*.runtimeconfig.json' }],
                outputs: node.results.map((results) => join('{workspaceRoot}', results)),
                metadata: metadata('test', `Restores ${node.name} and tests the build with dotnet test`),
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
            options: { ...cleaned.options, targets: Record.keys(Record.filter(tools, ({ outputs = [] }) => outputs.length > 0)) },
            configurations: cleaned.configurations,
            metadata: metadata('clean', `Removes the build outputs of ${node.name} with dotnet clean`),
        },
        ...('pack--dotnet' in tools && {
            'attach--gh': {
                executor: `${distribution.name}:attach`,
                dependsOn: ['pack--dotnet'],
                metadata: { description: "Uploads the files pack--dotnet wrote to the GitHub release of the version the project's manifest states through gh release upload", help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
            },
            'nx-release-publish': {
                executor: `${distribution.name}:publish`,
                options: { publish: ['dotnet', 'nuget', 'push', '--skip-duplicate'] },
                dependsOn: ['pack--dotnet', '^nx-release-publish'],
                metadata: { description: `Pushes the package pack--dotnet wrote for ${node.name} with dotnet nuget push, which reads NUGET_API_KEY and skips a version nuget.org holds`, help: { command: 'dotnet nuget push --help', example: { options: { dryRun: true } } } },
            },
        }),
    };
    return Record.map(delivered, (tool): TargetConfiguration => ({ ...tool, metadata: { technologies: ['dotnet', node.language], ...tool.metadata } }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Node, PLUGIN, targets };
