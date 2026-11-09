import type { TargetConfiguration } from '@nx/devkit';
import { mergeTargetConfigurations } from '@nx/devkit/internal';
import { Array, Option, type Path, Record } from 'effect';
import manifest from '../package.json' with { type: 'json' };
import type { Node } from './project-graph.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Targets = Readonly<Record<string, TargetConfiguration>>;
type Options = Readonly<Record<string, (Partial<TargetConfiguration> & { readonly targetName?: string }) | false>>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const configurations = (node: Node) =>
    Record.fromEntries(
        Array.cartesianWith(node.configurations, [undefined, ...node.runtimes], (configuration, runtime) => [
            [configuration.toLowerCase(), ...(runtime === undefined ? [] : [runtime])].join('_'),
            { args: ['--configuration', configuration, ...(runtime === undefined ? [] : ['--runtime', runtime])] },
        ]),
    );

const targets = (path: Path.Path, node: Node, named: string, platform: boolean): Targets => {
    const root = path.dirname(node.file);
    const project = path.join('{projectRoot}', path.basename(node.file));
    const technologies = ['dotnet', node.language];
    const inputs: NonNullable<TargetConfiguration['inputs']> = [
        named,
        `^${named}`,
        ...node.inputs.map((input) => path.join('{workspaceRoot}', input)),
        ...node.bases.filter((base) => base.startsWith(`${root}/`)).map((base) => `!${path.join('{workspaceRoot}', base, '**')}`),
        { externalDependencies: [] },
    ];
    const metadata = (args: readonly string[], description: string) => ({ technologies, description, help: { command: `dotnet ${args[0]} --help`, example: { args: ['--verbosity', 'detailed'] } } });
    const command = (args: readonly string[], description: string): TargetConfiguration => ({ command: ['dotnet', ...args].join(' '), configurations: configurations(node), metadata: metadata(args, description) });
    const batched = (args: readonly string[], description: string): TargetConfiguration => ({ executor: `${manifest.name}:dotnet`, options: { command: args, project }, metadata: metadata(args, description) });
    const formatted = node.inputs.some((input) => path.basename(input) === '.editorconfig');
    return {
        restore: { ...batched(['restore', '--no-dependencies'], `Restores ${node.name} with dotnet restore`), dependsOn: ['^restore'], cache: false, inputs, outputs: [path.join('{workspaceRoot}', path.dirname(node.restored))] },
        build: { ...command(['build', project, '--no-restore', '--no-dependencies'], `Builds ${node.name} with dotnet build`), dependsOn: ['restore', '^build'], cache: false, inputs, outputs: node.outputs.map((output) => path.join('{workspaceRoot}', output)) },
        'check--dotnet-build': {
            ...command(['build', project, '--no-restore', '--no-dependencies', '-p:NonExistentFile=__NonExistentSubDir__/__NonExistentFile__', '-warnaserror'], `Compiles ${node.name} again and fails on any compiler, analyzer, or MSBuild warning`),
            dependsOn: ['build'],
            cache: true,
            inputs,
        },
        check: { executor: 'nx:noop', dependsOn: ['...', 'check--*'], inputs: ['...', ...Array.dedupeWith(inputs, (left, right) => JSON.stringify(left) === JSON.stringify(right))], metadata: { technologies, description: 'Runs every check--* target' } },
        ...(node.test
            ? {
                  test: {
                      ...command(['test', ...(platform ? ['--project', project] : [project]), '--no-build'], `Tests ${node.name} with dotnet test`),
                      dependsOn: ['build'],
                      cache: true,
                      inputs: [...inputs, { env: 'DOTNET_ROLL_FORWARD' }, { env: 'DOTNET_ROLL_FORWARD_TO_PRERELEASE' }],
                      outputs: node.results === null ? [] : [path.join('{workspaceRoot}', node.results)],
                  },
              }
            : {}),
        ...(formatted
            ? {
                  'fix--dotnet-format': { ...batched(['format', '--no-restore'], `Applies the .editorconfig style and analyzer fixes to ${node.name} with dotnet format`), dependsOn: ['restore'], cache: false, inputs },
                  fix: { executor: 'nx:noop', dependsOn: ['...', 'fix--*'], metadata: { technologies, description: 'Runs every fix--* target' } },
              }
            : {}),
        ...(node.packable
            ? {
                  'nx-release-publish': {
                      executor: `${manifest.name}:release-publish`,
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
        const [, upstream = '', key = ''] = /^(\^?)(.*)$/.exec(dependency) ?? [];
        return Option.map(Option.fromUndefinedOr(key in inferred ? named(key) : key), (name) => `${upstream}${name}`);
    };
    return Record.fromEntries(
        Record.toEntries(inferred).flatMap(([key, target]): readonly (readonly [string, TargetConfiguration])[] => {
            const option = options[key];
            const { targetName: _, ...override } = option === undefined || option === false ? {} : option;
            const dependsOn = target.dependsOn?.flatMap((dependency): readonly NonNullable<TargetConfiguration['dependsOn']>[number][] =>
                typeof dependency === 'string' ? Option.toArray(renamed(dependency)) : Option.toArray(Option.map(renamed(dependency.target), (name) => ({ ...dependency, target: name }))),
            );
            return Option.toArray(Option.map(Option.fromUndefinedOr(named(key)), (name) => [name, mergeTargetConfigurations(override, { ...target, ...(dependsOn === undefined ? {} : { dependsOn }) })] as const));
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { configured, type Options, targets };
