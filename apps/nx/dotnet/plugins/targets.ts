import type { TargetConfiguration } from '@nx/devkit';
import { mergeTargetConfigurations } from '@nx/devkit/internal';
import { Array, Option, type Path, Record } from 'effect';
import type { Node } from '#/plugins/project-graph';

// --- [TYPES] ---------------------------------------------------------------------------

type Targets = Readonly<Record<string, TargetConfiguration>>;
type Options = Readonly<Record<string, (Partial<TargetConfiguration> & { readonly targetName?: string }) | false>>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const targets = (plugin: { readonly name: string; readonly inputs: NonNullable<TargetConfiguration['inputs']> }, path: Path.Path, node: Node, named: string): Targets => {
    const root = path.dirname(node.file);
    const project = path.join('{projectRoot}', path.basename(node.file));
    const technologies = ['dotnet', node.language];
    const inputs: NonNullable<TargetConfiguration['inputs']> = [named, `^${named}`, ...node.inputs.map((input) => path.join('{workspaceRoot}', input)), ...node.bases.filter((base) => base.startsWith(`${root}/`)).map((base) => `!${path.join('{workspaceRoot}', base, '**')}`), { externalDependencies: [] }];
    const metadata = (args: readonly string[], description: string) => ({ technologies, description, help: { command: `dotnet ${args[0]} --help`, example: { args: ['--verbosity', 'detailed'] } } });
    const modes = node.modes.map((mode) => {
        const publishing = node.publishable && mode.configuration === 'Release';
        return {
            name: [mode.configuration.toLowerCase(), ...Option.toArray(Option.fromNullOr(mode.runtime))].join('_'),
            args: ['--configuration', mode.configuration, ...(mode.runtime === null ? [] : ['--runtime', mode.runtime])],
            publishing,
            outputs: publishing ? [...mode.outputs, ...mode.published] : mode.outputs,
            primary: mode.configuration === node.configuration && mode.runtime === null,
        };
    });
    const command = (args: readonly string[], description: string, built = false): TargetConfiguration => ({
        executor: 'nx:run-commands',
        options: { command: ['dotnet', ...args].join(' '), cwd: '{projectRoot}', ...(built ? { outputs: modes.filter(({ primary }) => primary).flatMap(({ outputs }) => outputs) } : {}) },
        configurations: Record.fromIterableWith(modes, (mode) => [mode.name, { ...(built && mode.publishing ? { command: 'dotnet publish --no-restore' } : {}), args: mode.args, ...(built ? { outputs: mode.outputs } : {}) }] as const),
        metadata: metadata(args, description),
        ...(built ? { outputs: Array.dedupe(modes.flatMap(({ outputs }) => outputs.map((_, index) => `{options.outputs.${index}}`))) } : {}),
    });
    const batched = (commands: Array.NonEmptyReadonlyArray<Array.NonEmptyReadonlyArray<string>>, description: string): TargetConfiguration => ({
        executor: `${plugin.name}:dotnet`,
        options: { commands, project },
        inputs: [...inputs, ...plugin.inputs],
        metadata: metadata(Array.headNonEmpty(commands), description),
    });
    const formatted = node.inputs.some((input) => path.basename(input) === '.editorconfig');
    return {
        restore: { ...batched([['restore', '--no-dependencies']], `Restores ${node.name} with dotnet restore`), cache: false, outputs: [path.join('{workspaceRoot}', path.dirname(node.restored))] },
        'build--dotnet': {
            ...command(['build', '--no-restore', '--no-dependencies'], node.publishable ? `Builds ${node.name} with dotnet build, publishing it with dotnet publish in Release` : `Builds ${node.name} with dotnet build`, true),
            dependsOn: ['restore', '^build'],
            cache: false,
            inputs,
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
        ...(formatted
            ? {
                  'check--dotnet-format': {
                      ...batched([['format', 'style', '--no-restore', '--verify-no-changes']], `Fails on any code style finding in ${node.name}, the IDE rules no build runs included, with dotnet format style --verify-no-changes`),
                      dependsOn: ['restore'],
                      cache: true,
                  },
                  'fix--dotnet-format': {
                      ...batched(
                          [
                              ['format', 'style', '--no-restore'],
                              ['format', 'analyzers', '--no-restore'],
                          ],
                          `Applies the code style and analyzer fixes of ${node.name} with dotnet format style and analyzers`,
                      ),
                      dependsOn: ['restore'],
                      cache: false,
                  },
              }
            : {}),
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

const configured = (inferred: Targets, options: Options): Targets => {
    const named = (key: string) => {
        const option = options[key];
        return option === false ? undefined : (option?.targetName ?? key);
    };
    const renamed = (dependency: string) => {
        const upstream = dependency.startsWith('^') ? '^' : '';
        const key = dependency.slice(upstream.length);
        return Option.map(Option.fromUndefinedOr(key in inferred ? named(key) : key), (name) => `${upstream}${name}`);
    };
    return Record.fromEntries(
        Record.toEntries(inferred).flatMap(([key, target]): readonly (readonly [string, TargetConfiguration])[] => {
            const option = options[key];
            const { targetName: _, ...override } = option === undefined || option === false ? {} : option;
            const dependsOn = target.dependsOn?.flatMap((dependency): readonly NonNullable<TargetConfiguration['dependsOn']>[number][] => (typeof dependency === 'string' ? Option.toArray(renamed(dependency)) : Option.toArray(Option.map(renamed(dependency.target), (name) => ({ ...dependency, target: name })))));
            return Option.toArray(Option.map(Option.fromUndefinedOr(named(key)), (name) => [name, mergeTargetConfigurations(override, { ...target, ...(dependsOn === undefined ? {} : { dependsOn }) })] as const));
        }),
    );
};

const aggregated = (held: Targets, technologies: readonly string[]): Targets => ({
    ...held,
    ...Record.map(
        Array.groupBy(
            Record.toEntries(held).filter(([name]) => name.includes('--')),
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

export { aggregated, configured, type Options, targets };
