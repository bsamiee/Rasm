import type { TargetConfiguration } from '@nx/devkit';
import { aggregated, type Members } from '@rasm/nx-devkit/inference';
import { Array, Effect, Option, Path, Record, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Node = typeof Node.Type;
interface Plugin {
    readonly name: string;
    readonly inputs: NonNullable<TargetConfiguration['inputs']>;
    readonly cleaner: string;
}

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
    central: Schema.NullOr(Schema.String),
    releases: Schema.NullOr(Schema.Struct({ shipped: Schema.String, unshipped: Schema.String })),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyzer = Effect.flatMap(Path.Path, (path) => path.fromFileUrl(new URL(import.meta.resolve('#analyzer'))));

const metadata = (technologies: readonly string[], verb: string, description: string) => ({ technologies: [...technologies], description, help: { command: `dotnet ${verb} --help`, example: { args: ['--verbosity', 'detailed'] } } });

const targets = (plugin: Plugin, path: Path.Path, node: Node, named: string): Readonly<Record<string, TargetConfiguration>> => {
    const root = path.dirname(node.file);
    const project = path.join('{projectRoot}', path.basename(node.file));
    const technologies = ['dotnet', node.language];
    const inputs: NonNullable<TargetConfiguration['inputs']> = [
        named,
        `^${named}`,
        ...node.inputs.map((input) => path.join('{workspaceRoot}', input)),
        ...node.bases.filter((base) => base.startsWith(`${root}/`)).map((base) => `!${path.join('{workspaceRoot}', base, '**')}`),
        { env: 'NUGET_PACKAGES' },
        { externalDependencies: [] },
    ];
    const modes = node.modes.map((mode) => ({
        name: [mode.configuration.toLowerCase(), ...Option.toArray(Option.fromNullOr(mode.runtime))].join('_'),
        args: ['--configuration', mode.configuration, ...(mode.runtime === null ? [] : ['--runtime', mode.runtime])],
        verb: Array.isReadonlyArrayNonEmpty(mode.published) ? 'publish' : 'build',
        outputs: [...mode.outputs, ...mode.published],
        reports: mode.reports,
        primary: mode.configuration === node.configuration && mode.runtime === null,
    }));
    const configured = <A>(each: (mode: (typeof modes)[number]) => A) => ({
        configurations: Record.fromIterableWith(modes, (mode) => [mode.name, each(mode)] as const),
        ...Record.getSomes({ defaultConfiguration: Option.map(Array.findFirst(modes, Struct.get('primary')), Struct.get('name')) }),
    });
    const formatted = (commands: Array.NonEmptyReadonlyArray<Array.NonEmptyReadonlyArray<string>>, description: string): TargetConfiguration => ({
        executor: `${plugin.name}:format`,
        options: { commands, project },
        dependsOn: ['build--dotnet'],
        inputs: Array.dedupe([...inputs, ...plugin.inputs]),
        metadata: metadata(technologies, Array.headNonEmpty(Array.headNonEmpty(commands)), description),
    });
    const members: Members = {
        build: {
            dotnet: {
                executor: `${plugin.name}:build`,
                options: { project },
                ...configured(({ verb, args, outputs }) => ({ command: [verb, ...args], outputs })),
                outputs: Array.makeBy(Math.max(0, ...modes.map(({ outputs }) => outputs.length)), (index) => `{options.outputs.${index}}`),
                dependsOn: ['^build--dotnet'],
                cache: true,
                inputs: Array.dedupe([...inputs, ...plugin.inputs, { workingDirectory: 'absolute' as const }]),
                metadata: metadata(technologies, node.publishable ? 'publish' : 'build', node.publishable ? `Builds ${node.name} with its references in one MSBuild run, publishing it in every configuration` : `Builds ${node.name} with its references in one MSBuild run`),
            },
        },
        check: {
            'dotnet-build': {
                executor: `${plugin.name}:check`,
                ...configured(({ reports }) => ({ reports })),
                dependsOn: ['build--dotnet'],
                cache: true,
                inputs: [...plugin.inputs, { dependentTasksOutputFiles: '**/*.sarif', transitive: false }],
                metadata: { technologies, description: `Fails on each warning or error in the SARIF report the compile of ${node.name} wrote, printed as file(line,col): level id: text` },
            },
            'dotnet-format': { ...formatted([['format', 'style', '--verify-no-changes']], `Fails on any code style finding in ${node.name}, the IDE rules no build runs included, with dotnet format style --verify-no-changes`), cache: true },
        },
        test: node.test
            ? {
                  dotnet: {
                      executor: 'nx:run-commands',
                      options: { command: 'dotnet test --no-build', cwd: '{projectRoot}' },
                      ...configured(({ args }) => ({ args })),
                      dependsOn: ['build--dotnet'],
                      cache: true,
                      inputs: [...inputs, { dependentTasksOutputFiles: '**/*.runtimeconfig.json' }],
                      outputs: node.results.map((results) => path.join('{workspaceRoot}', results)),
                      metadata: metadata(technologies, 'test', `Tests ${node.name} with dotnet test`),
                  },
              }
            : {},
        clean: {
            dotnet: {
                executor: plugin.cleaner,
                options: { commands: [['dotnet', 'clean', node.file]] },
                ...configured(({ args }) => ({ commands: [['dotnet', 'clean', node.file, ...args]] })),
                metadata: metadata(technologies, 'clean', `Removes the build outputs of ${node.name} with dotnet clean`),
            },
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
        ...aggregated(members, technologies),
        ...(node.packable
            ? {
                  'nx-release-publish': {
                      executor: `${plugin.name}:release-publish`,
                      options: { project },
                      dependsOn: ['^nx-release-publish'],
                      metadata: { technologies, description: `Packs the Release build of ${node.name} and pushes the package with dotnet nuget push` },
                  },
              }
            : {}),
    };
};

const upgraded = (plugin: Plugin, path: Path.Path, central: string, consumers: readonly Node[]): Members => ({
    upgrade: {
        dotnet: {
            executor: `${plugin.name}:upgrade`,
            options: { central: path.join('{projectRoot}', path.basename(central)), projects: consumers.map(({ file }) => path.join('{workspaceRoot}', file)) },
            outputs: [path.join('{projectRoot}', path.basename(central)), ...consumers.flatMap(({ locks }) => locks.map((locked) => path.join('{workspaceRoot}', locked)))],
            metadata: { technologies: ['dotnet'], description: `Moves each plain version ${path.basename(central)} pins to the newest release its consumers' frameworks, hosts, and repository partners accept, then restores the projects reading it with dotnet restore --force-evaluate` },
        },
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { analyzer, Node, targets, upgraded };
