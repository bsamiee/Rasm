import type { ProjectConfiguration, TargetConfiguration } from '@nx/devkit';
import { isCI } from '@nx/devkit/internal';
import { Array, Option, type Path, Record } from 'effect';
import type { Cargo, Command } from '../executors/cargo/executor.ts';
import manifest from '../package.json' with { type: 'json' };
import { ancestors, MANIFEST, nearest, type Workspace } from './workspace.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Input = NonNullable<TargetConfiguration['inputs']>[number];
type Targets = Readonly<Record<string, TargetConfiguration>>;
type Crate = Workspace['metadata']['packages'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHECKS = ['advisories', 'bans', 'licenses', 'sources'];
const FORMAT = 'json-diagnostic-rendered-ansi';
const TECHNOLOGIES = ['rust'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const locked = () => (Boolean(isCI()) ? ['--locked'] : []);

const tokened = (path: Path.Path, file: string) => path.join('{workspaceRoot}', file);

const target = (help: string, description: string, options: typeof Cargo.Type, extra: Partial<TargetConfiguration>): TargetConfiguration => ({
    executor: `${manifest.name}:cargo`,
    options,
    metadata: { technologies: TECHNOLOGIES, description, help: { command: `cargo ${help} --help`, example: { args: ['--verbose'] } } },
    ...extra,
});

const aggregated = (targets: Targets): Targets => ({
    ...targets,
    ...Record.fromIterableWith(
        ['check', 'fix', 'upgrade'].filter((verb) => Record.keys(targets).some((name) => name.startsWith(`${verb}--`))),
        (verb) =>
            [
                verb,
                {
                    executor: 'nx:noop',
                    dependsOn: [`${verb}--*`],
                    inputs: [
                        '...',
                        ...Array.dedupe(Record.collect(Record.filter(targets, (_, name) => name.startsWith(`${verb}--`)), (_, held) => held.inputs ?? []).flat()),
                    ],
                    metadata: { technologies: TECHNOLOGIES, description: `Runs every ${verb}--* target` },
                } satisfies TargetConfiguration,
            ] as const,
    ),
});

const crateTargets = (path: Path.Path, found: Workspace, files: ReadonlySet<string>, named: readonly Input[], crate: Crate, root: string): Targets => {
    const selected = { cwd: found.root, package: crate.name };
    const shared: readonly Input[] = [
        ...named,
        ...(found.root === root ? [] : [tokened(path, path.join(found.root, MANIFEST))]),
        ...ancestors(path, found.root).flatMap((at) => ['.cargo/config.toml', '.cargo/config', 'rust-toolchain.toml', 'rust-toolchain'].map((name) => path.join(at, name)).filter((file) => files.has(file)).map((file) => tokened(path, file))),
        { externalDependencies: [] },
    ];
    const nextest = Option.liftPredicate(path.join(found.root, '.config/nextest.toml'), (file) => files.has(file));
    const tested = crate.targets.some(({ test }) => test);
    const documented = crate.targets.some(({ doctest }) => doctest);
    const products = crate.targets.filter(({ kind, crate_types }) => kind.join() === crate_types.join());
    const profiles = ['release', ...found.profiles];
    const compiler = Option.match(nearest(path, files, root, ['.clippy.toml', 'clippy.toml']), {
        onNone: () => ({ tool: 'cargo', check: ['check'] as const, fix: ['fix'] as const, inputs: [] }),
        onSome: (file) => ({ tool: 'clippy', check: ['clippy'] as const, fix: ['clippy', '--fix'] as const, inputs: [tokened(path, file)] }),
    });
    const libraries = (found.metadata.metadata?.dylint?.libraries ?? []).flatMap(({ path: at }) => (at === undefined ? [] : [tokened(path, path.join(found.root, at, '**/*'))]));
    const built = (profile: readonly string[]): Array.NonEmptyReadonlyArray<Command> => [{ args: ['build', ...profile, '--keep-going', '--message-format', FORMAT, ...locked()] }];
    const tests: readonly Command[] = Option.match(nextest, {
        onNone: () => (tested || documented ? [{ args: ['test', '--message-format', FORMAT, ...locked()] }] : []),
        onSome: () => [
            ...(tested ? [{ args: ['nextest', 'run', '--no-tests=warn', '--cargo-message-format', FORMAT, ...locked()] } as const] : []),
            ...(documented ? [{ args: ['test', '--doc', '--message-format', FORMAT, ...locked()] } as const] : []),
        ],
    });
    return {
        build: target('build', `Builds ${crate.name} with cargo`, { ...selected, commands: built([]) }, {
            cache: false,
            outputs: Option.toArray(found.output).flatMap((at) => ['debug', ...profiles].flatMap((profile) => products.flatMap(found.files).map((file) => tokened(path, path.join(at, profile, file))))),
            configurations: Record.fromIterableWith(profiles, (profile) => [profile, { commands: built(['--profile', profile]) }] as const),
        }),
        ...Array.match(tests, {
            onEmpty: () => ({}),
            onNonEmpty: (commands) => ({ test: target(Option.isSome(nextest) ? 'nextest run' : 'test', `Tests ${crate.name}`, { ...selected, commands }, { cache: true, inputs: [...shared, ...Option.toArray(Option.map(nextest, (file) => tokened(path, file)))] }) }),
        }),
        [`check--${compiler.tool}`]: target(compiler.check.join(' '), `Reports ${compiler.tool} findings for ${crate.name}, failing on any warning`, { ...selected, warnings: 'deny', commands: [{ args: [...compiler.check, '--all-targets', '--keep-going', '--message-format', FORMAT, ...locked()] }] }, { cache: true, inputs: [...shared, ...compiler.inputs] }),
        [`fix--${compiler.tool}`]: target(compiler.check.join(' '), `Applies ${compiler.tool} suggestions to ${crate.name}`, { ...selected, commands: [{ args: [...compiler.fix, '--allow-dirty', '--all-targets', '--keep-going', '--message-format', FORMAT] }] }, { cache: false }),
        ...(libraries.length === 0
            ? {}
            : {
                  'check--dylint': target(
                      'dylint',
                      `Reports Dylint findings for ${crate.name}, failing on any warning`,
                      { ...selected, warnings: 'deny', commands: [{ args: ['dylint', '--all', '--fail-on-no-libraries'], trailing: ['--all-targets', '--keep-going', '--message-format', FORMAT, ...locked()] }] },
                      { cache: true, inputs: [...shared, ...libraries] },
                  ),
              }),
    };
};

const workspaceTargets = (path: Path.Path, found: Workspace): Targets => {
    const manifests = Array.dedupe([...found.manifests, path.join(found.root, MANIFEST)]).map((file) => tokened(path, file));
    const lock = tokened(path, path.join(found.root, 'Cargo.lock'));
    return {
        ...Option.match(found.deny, {
            onNone: () => ({}),
            onSome: ({ file, tables }) => {
                const checks = CHECKS.filter((check) => tables.includes(check));
                return {
                    'check--cargo-deny': target('deny', 'Reports cargo-deny findings over the workspace graph, failing on any warning', { cwd: found.root, commands: [{ args: ['deny', '--workspace', ...locked(), 'check', '--deny', 'warnings', ...checks] }] }, {
                        cache: !checks.includes('advisories'),
                        inputs: [...manifests, lock, tokened(path, file), { externalDependencies: [] }],
                    }),
                };
            },
        }),
        'upgrade--cargo': target('upgrade', 'Moves every dependency requirement and the lock to the newest releases', { cwd: found.root, commands: [{ args: ['upgrade', '--incompatible', 'allow'] }, { args: ['update'] }] }, { cache: false, outputs: [...manifests, lock] }),
    };
};

const released = (crate: Crate, cwd: string): Pick<ProjectConfiguration, 'release'> & { readonly targets: Targets } =>
    crate.publish?.length === 0
        ? { targets: {} }
        : {
              release: { version: { versionActions: `${manifest.name}/release/version-actions` } },
              targets: {
                  'nx-release-publish': {
                      executor: `${manifest.name}:release-publish`,
                      dependsOn: ['^nx-release-publish'],
                      options: { cwd, package: crate.name },
                      metadata: { technologies: TECHNOLOGIES, description: `Publishes ${crate.name} with cargo publish` },
                  },
              },
          };

// --- [EXPORTS] -------------------------------------------------------------------------

export { aggregated, crateTargets, released, TECHNOLOGIES, workspaceTargets };
