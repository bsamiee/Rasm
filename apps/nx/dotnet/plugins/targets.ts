import type { TargetConfiguration } from '@nx/devkit';
import { Array, Option, type Path, Record, Struct } from 'effect';
import type { Node } from '#/plugins/project-graph';

// --- [TYPES] ---------------------------------------------------------------------------

type Targets = Readonly<Record<string, TargetConfiguration>>;
interface Plugin {
    readonly name: string;
    readonly inputs: NonNullable<TargetConfiguration['inputs']>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const metadata = (technologies: readonly string[], verb: string, description: string) => ({ technologies: [...technologies], description, help: { command: `dotnet ${verb} --help`, example: { args: ['--verbosity', 'detailed'] } } });

const targets = (plugin: Plugin, path: Path.Path, node: Node, named: string): Targets => {
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
        primary: mode.configuration === node.configuration && mode.runtime === null,
    }));
    const configured = <A>(each: (mode: (typeof modes)[number]) => A) => ({
        configurations: Record.fromIterableWith(modes, (mode) => [mode.name, each(mode)] as const),
        ...Record.getSomes({ defaultConfiguration: Option.map(Array.findFirst(modes, Struct.get('primary')), Struct.get('name')) }),
    });
    const command = (args: Array.NonEmptyReadonlyArray<string>, description: string): TargetConfiguration => ({
        executor: 'nx:run-commands',
        options: { command: ['dotnet', ...args].join(' '), cwd: '{projectRoot}' },
        ...configured(({ args: mode }) => ({ args: mode })),
        metadata: metadata(technologies, Array.headNonEmpty(args), description),
    });
    const formatted = (commands: Array.NonEmptyReadonlyArray<Array.NonEmptyReadonlyArray<string>>, description: string): TargetConfiguration => ({
        executor: `${plugin.name}:format`,
        options: { commands, project },
        dependsOn: ['build--dotnet'],
        inputs: Array.dedupe([...inputs, ...plugin.inputs]),
        metadata: metadata(technologies, Array.headNonEmpty(Array.headNonEmpty(commands)), description),
    });
    return {
        'build--dotnet': {
            executor: `${plugin.name}:build`,
            options: { project },
            ...configured(({ verb, args, outputs }) => ({ command: [verb, ...args], outputs })),
            outputs: Array.makeBy(Math.max(0, ...modes.map(({ outputs }) => outputs.length)), (index) => `{options.outputs.${index}}`),
            dependsOn: ['^build--dotnet'],
            cache: true,
            inputs: Array.dedupe([...inputs, ...plugin.inputs, { workingDirectory: 'absolute' as const }]),
            metadata: metadata(technologies, 'build', node.publishable ? `Builds ${node.name} with its references in one MSBuild run, publishing it in each optimized configuration` : `Builds ${node.name} with its references in one MSBuild run`),
        },
        'check--dotnet-build': {
            ...command(['build', '--no-restore', '--no-dependencies', '-p:NonExistentFile=__NonExistentSubDir__/__NonExistentFile__', '-warnaserror'], `Compiles ${node.name} again and fails on any compiler, analyzer, or MSBuild warning`),
            dependsOn: ['build--dotnet'],
            cache: true,
            inputs,
        },
        ...(node.test
            ? {
                  test: {
                      ...command(['test', '--no-build'], `Tests ${node.name} with dotnet test`),
                      dependsOn: ['build--dotnet'],
                      cache: true,
                      inputs: [...inputs, { dependentTasksOutputFiles: '**/*.runtimeconfig.json' }],
                      outputs: node.results.map((results) => path.join('{workspaceRoot}', results)),
                  },
              }
            : {}),
        'check--dotnet-format': { ...formatted([['format', 'style', '--no-restore', '--verify-no-changes']], `Fails on any code style finding in ${node.name}, the IDE rules no build runs included, with dotnet format style --verify-no-changes`), cache: true },
        'fix--dotnet-format': {
            ...formatted(
                [
                    ['format', 'style', '--no-restore'],
                    ['format', 'analyzers', '--no-restore'],
                ],
                `Applies the code style and analyzer fixes of ${node.name} with dotnet format style and analyzers`,
            ),
            cache: false,
        },
        ...(node.packable
            ? {
                  'nx-release-publish': {
                      executor: `${plugin.name}:release-publish`,
                      options: { project },
                      dependsOn: ['^nx-release-publish'],
                      metadata: { technologies, description: `Packs ${node.name} in Release and pushes the package with dotnet nuget push` },
                  },
              }
            : {}),
    };
};

const upgraded = (plugin: Plugin, path: Path.Path, central: string, consumers: readonly Node[]): Targets => ({
    'upgrade--dotnet': {
        executor: `${plugin.name}:upgrade`,
        options: { central: path.join('{projectRoot}', path.basename(central)), projects: consumers.map(({ file }) => path.join('{workspaceRoot}', file)) },
        cache: false,
        outputs: [path.join('{projectRoot}', path.basename(central)), ...consumers.flatMap(({ locks }) => locks.map((locked) => path.join('{workspaceRoot}', locked)))],
        metadata: metadata(['dotnet'], 'restore', `Moves every version ${path.basename(central)} pins to the newest listed release, then restores the projects reading it with dotnet restore --force-evaluate`),
    },
});

const aggregated = (declared: Targets, technologies: readonly string[]): Targets => ({
    ...declared,
    ...Record.map(
        Array.groupBy(
            Record.toEntries(declared).filter(([name]) => name.includes('--')),
            ([name]) => name.slice(0, name.indexOf('--')),
        ),
        (members, verb): TargetConfiguration => ({
            executor: 'nx:noop',
            dependsOn: [`${verb}--*`],
            inputs: ['...', ...Array.dedupe(members.flatMap(([, member]) => member.inputs ?? []))],
            metadata: { technologies: [...technologies], description: `Runs every ${verb}--* target` },
        }),
    ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { aggregated, targets, upgraded };
