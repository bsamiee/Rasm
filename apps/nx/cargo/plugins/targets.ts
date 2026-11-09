import type { ProjectConfiguration, TargetConfiguration } from '@nx/devkit';
import { isCI } from '@nx/devkit/internal';
import { Array, Option, type Path, Record } from 'effect';
import type { Cargo, Command } from '#/executors/cargo/executor';
import { ancestors, MANIFEST, nearest, type Workspace } from '#/plugins/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Input = NonNullable<TargetConfiguration['inputs']>[number];
type Targets = Readonly<Record<string, TargetConfiguration>>;
type Crate = Workspace['metadata']['packages'][number];
type Target = Crate['targets'][number];
interface Plugin {
    readonly name: string;
    readonly inputs: readonly Input[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHECKS = ['advisories', 'bans', 'licenses', 'sources'];
const SELECTIONS: Readonly<Record<string, (target: Target) => boolean>> = {
    '--lib': ({ kind, crate_types }) => kind.join() === crate_types.join() && !kind.includes('bin'),
    '--bins': ({ kind }) => kind.includes('bin'),
    '--tests': ({ test }) => test,
    '--benches': ({ kind }) => kind.includes('bench'),
};
const TECHNOLOGIES = ['rust'];
const RELEASE = 'release';

// --- [OPERATIONS] ----------------------------------------------------------------------

const locked = () => (isCI() === false ? [] : ['--locked']);

const tokened = (path: Path.Path, file: string) => path.join('{workspaceRoot}', file);

const target = (plugin: Plugin, help: string, description: string, options: typeof Cargo.Type & { readonly outputs?: readonly string[] }, extra: Partial<TargetConfiguration>): TargetConfiguration => ({
    executor: `${plugin.name}:cargo`,
    options,
    metadata: { technologies: TECHNOLOGIES, description, help: { command: `cargo ${help} --help`, example: { args: ['--verbose'] } } },
    ...extra,
    ...(extra.cache === true ? { inputs: [...(extra.inputs ?? []), ...plugin.inputs] } : {}),
});

const aggregated = (targets: Targets): Targets => ({
    ...targets,
    ...Record.map(
        Array.groupBy(
            Record.toEntries(targets).filter(([name]) => name.includes('--')),
            ([name]) => name.slice(0, name.indexOf('--')),
        ),
        (members, verb): TargetConfiguration => ({
            executor: 'nx:noop',
            dependsOn: [`${verb}--*`],
            inputs: ['...', ...Array.dedupe(members.flatMap(([, member]) => member.inputs ?? []))],
            metadata: { technologies: TECHNOLOGIES, description: `Runs every ${verb}--* target` },
        }),
    ),
});

const crateTargets = (plugin: Plugin, path: Path.Path, workspace: Workspace, files: ReadonlySet<string>, named: readonly Input[], crate: Crate, root: string): Targets => {
    const selected = { cwd: workspace.root, package: crate.name };
    const shared: readonly Input[] = [
        ...named,
        ...(workspace.root === root ? [] : [tokened(path, path.join(workspace.root, MANIFEST))]),
        ...ancestors(path, workspace.root).flatMap((at) =>
            ['.cargo/config.toml', '.cargo/config', 'rust-toolchain.toml', 'rust-toolchain']
                .map((name) => path.join(at, name))
                .filter((file) => files.has(file))
                .map((file) => tokened(path, file)),
        ),
    ];
    const nextest = Option.liftPredicate(path.join(workspace.root, '.config/nextest.toml'), (file) => files.has(file));
    const tested = crate.targets.some(({ test }) => test);
    const documented = crate.targets.some(({ doctest }) => doctest);
    const filters = Record.keys(Record.filter(SELECTIONS, (selects) => crate.targets.some(selects)));
    const products = crate.targets.filter(({ kind, crate_types }) => kind.join() === crate_types.join()).flatMap(workspace.files);
    const written = (folder: string) => Option.toArray(workspace.output).flatMap((at) => products.map((file) => tokened(path, path.join(at, folder, file))));
    const profiles = ['release', ...workspace.profiles];
    const configured = nearest(path, files, root, ['.clippy.toml', 'clippy.toml']);
    const compiler =
        Option.isSome(configured) || workspace.linted(crate).has('clippy') ? { tool: 'clippy', check: ['clippy'] as const, fix: ['clippy', '--fix'] as const, inputs: Option.toArray(Option.map(configured, (file) => tokened(path, file))) } : { tool: 'cargo', check: ['check'] as const, fix: ['fix'] as const, inputs: [] };
    const libraries = (workspace.metadata.metadata?.dylint?.libraries ?? []).flatMap(({ path: at }) => (at === undefined ? [] : [tokened(path, path.join(workspace.root, at, '**/*'))]));
    const built = (profile: readonly string[]): Array.NonEmptyReadonlyArray<Command> => [{ args: ['build', ...profile, '--keep-going', ...locked()], report: 'messages' }];
    const checked = (placed: (selection: readonly string[]) => Pick<Command, 'args' | 'trailing'>): Array.NonEmptyReadonlyArray<Command> =>
        Array.map(
            Array.prepend(
                workspace.strategies.map((profile) => ['--profile', profile]),
                [],
            ),
            (profile): Command => ({ ...placed([...filters, ...profile, '--keep-going', ...locked()]), report: 'messages' }),
        );
    const runs = (runner: Readonly<Record<'nextest' | 'test', Array.NonEmptyReadonlyArray<string>>>): readonly Command[] => {
        const [main, documentation] = Option.match(nextest, {
            onNone: () => [{ args: runner.test, report: 'status', selected: tested || documented }, false] as const,
            onSome: () => [{ args: runner.nextest, report: 'tests', selected: tested }, documented] as const,
        });
        return [...(main.selected ? [{ args: [...main.args, ...locked()], report: main.report } as const] : []), ...(documentation ? [{ args: ['test', '--doc', ...locked()], report: 'status' } as const] : [])];
    };
    const covered = Option.map(workspace.output, (at) => {
        const tool = 'llvm-cov';
        const report = tokened(path, path.join(at, tool, `${crate.name}.lcov`));
        const exported = ['--lcov', '--output-path', report];
        const subjects = workspace.members.filter((member) => crate.dependencies.some(({ name, path: to }) => to !== undefined && name === member.crate.name)).map((member) => ({ root: member.root, name: member.crate.name }));
        return { outputs: ['{options.outputs.0}'], configurations: { coverage: { commands: runs({ nextest: [tool, 'nextest', ...exported], test: [tool, 'test', ...exported] }).map((command) => ({ ...command, packages: subjects })), outputs: [report] } } };
    });
    return {
        'build--cargo': target(
            plugin,
            'build',
            `Builds ${crate.name} with cargo`,
            { ...selected, commands: built([]), outputs: written('debug') },
            {
                cache: false,
                outputs: written('debug').map((_, index) => `{options.outputs.${index}}`),
                configurations: Record.fromIterableWith(profiles, (profile) => [profile, { commands: built(['--profile', profile]), outputs: written(profile) }] as const),
            },
        ),
        ...Array.match(runs({ nextest: ['nextest', 'run'], test: ['test'] }), {
            onEmpty: () => ({}),
            onNonEmpty: (commands) => ({
                test: target(plugin, Option.isSome(nextest) ? 'nextest run' : 'test', `Tests ${crate.name}`, { ...selected, commands }, { cache: true, inputs: [...shared, ...Option.toArray(Option.map(nextest, (file) => tokened(path, file)))], ...Option.getOrElse(covered, () => ({})) }),
            }),
        }),
        [`check--${compiler.tool}`]: target(plugin, compiler.check.join(' '), `Reports ${compiler.tool} findings for ${crate.name}`, { ...selected, commands: checked((selection) => ({ args: [...compiler.check, ...selection] })) }, { cache: true, inputs: [...shared, ...compiler.inputs] }),
        [`fix--${compiler.tool}`]: target(plugin, compiler.check.join(' '), `Applies ${compiler.tool} suggestions to ${crate.name}`, { ...selected, commands: [{ args: [...compiler.fix, '--allow-dirty', ...filters, '--keep-going'], report: 'messages' }] }, { cache: false }),
        ...(libraries.length === 0
            ? {}
            : {
                  'check--dylint': target(plugin, 'dylint', `Reports Dylint findings for ${crate.name}`, { ...selected, commands: checked((selection) => ({ args: ['dylint', '--all', '--fail-on-no-libraries'], trailing: selection })) }, { cache: true, inputs: [...shared, ...libraries] }),
                  'fix--dylint': target(plugin, 'dylint', `Applies Dylint suggestions to ${crate.name}`, { ...selected, commands: [{ args: ['dylint', '--all', '--fail-on-no-libraries', '--fix'], trailing: ['--allow-dirty', ...filters, '--keep-going'], report: 'messages' }] }, { cache: false }),
              }),
    };
};

const workspaceTargets = (plugin: Plugin, path: Path.Path, workspace: Workspace): Targets => {
    const manifests = Array.dedupe([...workspace.manifests, path.join(workspace.root, MANIFEST)]).map((file) => tokened(path, file));
    const lock = tokened(path, path.join(workspace.root, 'Cargo.lock'));
    return {
        ...Option.match(workspace.deny, {
            onNone: () => ({}),
            onSome: ({ files, tables }) => {
                const checks = CHECKS.filter((check) => tables.includes(check));
                return {
                    'check--cargo-deny': target(
                        plugin,
                        'deny',
                        'Reports cargo-deny findings over the workspace graph, failing on any warning',
                        { cwd: workspace.root, commands: [{ args: ['deny', '--workspace', ...locked(), 'check', '--deny', 'warnings', ...checks], report: 'status' }] },
                        {
                            cache: !checks.includes('advisories'),
                            inputs: [...manifests, lock, ...files.map((file) => tokened(path, file))],
                        },
                    ),
                };
            },
        }),
        'upgrade--cargo': target(
            plugin,
            'upgrade',
            'Moves every dependency requirement and the lock to the newest releases',
            {
                cwd: workspace.root,
                commands: [
                    { args: ['upgrade', '--incompatible', 'allow'], report: 'status' },
                    { args: ['update'], report: 'status' },
                ],
            },
            { cache: false, outputs: [...manifests, lock] },
        ),
    };
};

const released = (plugin: Plugin, crate: Crate, cwd: string): Pick<ProjectConfiguration, 'release' | 'tags'> & { readonly targets: Targets } =>
    crate.publish?.length === 0
        ? { targets: {} }
        : {
              tags: [RELEASE],
              release: { version: { versionActions: `${plugin.name}/release/version-actions` } },
              targets: {
                  'nx-release-publish': {
                      executor: `${plugin.name}:release-publish`,
                      dependsOn: ['^nx-release-publish'],
                      options: { cwd, package: crate.name },
                      metadata: { technologies: TECHNOLOGIES, description: `Publishes ${crate.name} with cargo publish` },
                  },
              },
          };

// --- [EXPORTS] -------------------------------------------------------------------------

export { aggregated, crateTargets, released, TECHNOLOGIES, workspaceTargets };
