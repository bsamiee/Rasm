import type { ProjectConfiguration, TargetConfiguration } from '@nx/devkit';
import { isCI } from '@nx/devkit/internal';
import { Array, Equivalence, Option, type Path, Record, Result, String } from 'effect';
import type { Cargo, Command } from '#/executors/cargo/executor';
import { ancestors, CLIPPY_CONF_DIR, CONFIG, LOCK, MANIFEST, nearest, type Workspace } from '#/plugins/workspace';

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

const DENIED = [
    { target: 'check--cargo-deny', checks: ['bans', 'licenses', 'sources'], cache: true },
    { target: 'check--cargo-deny-advisories', checks: ['advisories'], cache: false },
] as const;
const ENVIRONMENT = {
    variables: ['RUSTC', 'RUSTC_WRAPPER', 'RUSTC_WORKSPACE_WRAPPER', 'RUSTDOC', 'RUSTFLAGS', 'CARGO_ENCODED_RUSTFLAGS', 'RUSTDOCFLAGS', 'CARGO_ENCODED_RUSTDOCFLAGS', 'RUSTUP_TOOLCHAIN'],
    build: ['rustc', 'rustc-wrapper', 'rustc-workspace-wrapper', 'rustdoc', 'target', 'rustflags', 'rustdocflags'],
    target: ['rustflags', 'runner', 'linker'],
    profiles: ['dev', 'release', 'test', 'bench'],
    profile: ['opt-level', 'debug-assertions', 'overflow-checks', 'panic', 'lto', 'codegen-units'],
} as const;
const SEPARATOR = /[.-]/gu;
const TECHNOLOGIES = ['rust'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const own = ({ kind, crate_types }: Target) => Array.makeEquivalence(Equivalence.String)(kind, crate_types);

const binary = ({ kind }: Target) => kind.includes('bin');

const library = (candidate: Target) => own(candidate) && !binary(candidate);

const SELECTIONS: Readonly<Record<string, (target: Target) => boolean>> = {
    '--lib': library,
    '--bins': binary,
    '--tests': ({ test }) => test,
    '--benches': ({ kind }) => kind.includes('bench'),
};

const enabled = (features: Crate['features'], names: readonly string[]): readonly string[] => names.map((name) => Array.headNonEmpty(String.split(name, '/'))).flatMap((name) => Option.match(Record.get(features, name), { onNone: () => [], onSome: (implied) => [name, ...enabled(features, implied)] }));

const locked = () => (isCI() === false ? [] : ['--locked']);

const tokened = (path: Path.Path, file: string) => path.join('{workspaceRoot}', file);

const configured = (...keys: readonly string[]) => ({ env: `CARGO_${keys.join('_').toUpperCase().replaceAll(SEPARATOR, '_')}` });

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
        tokened(path, path.join(workspace.root, LOCK)),
        ...(workspace.root === root ? [] : [tokened(path, path.join(workspace.root, MANIFEST))]),
        ...ancestors(path, workspace.root).flatMap((at) =>
            [...CONFIG, 'rust-toolchain.toml', 'rust-toolchain']
                .map((name) => path.join(at, name))
                .filter((file) => files.has(file))
                .map((file) => tokened(path, file)),
        ),
        { runtime: 'rustc --print host-tuple' },
        ...ENVIRONMENT.variables.map((env) => ({ env })),
        ...ENVIRONMENT.build.map((key) => configured('build', key)),
        ...Option.toArray(workspace.host).flatMap((host) => ENVIRONMENT.target.map((key) => configured('target', host, key))),
        ...Array.cartesianWith([...ENVIRONMENT.profiles, ...workspace.profiles], ENVIRONMENT.profile, (profile, key) => configured('profile', profile, key)),
    ];
    const nextest = Option.liftPredicate(path.join(workspace.root, '.config/nextest.toml'), (file) => files.has(file));
    const tested = crate.targets.some(({ test }) => test);
    const documented = crate.targets.some(({ doctest }) => doctest);
    const filters = [
        ...Record.keys(Record.filter(SELECTIONS, (selects) => crate.targets.some(selects))),
        ...Array.match(Array.difference(Record.keys(crate.features), enabled(crate.features, ['default'])), {
            onEmpty: () => [],
            onNonEmpty: (features) => ['--features', features.map((feature) => `${crate.name}/${feature}`).join(',')],
        }),
    ];
    const products = crate.targets.filter(own).flatMap(workspace.files);
    const written = (folder: string) => Option.toArray(workspace.output).flatMap((at) => products.map((file) => tokened(path, path.join(at, folder, file))));
    const profiles = ['release', ...workspace.profiles];
    const clippy = nearest(
        path,
        files,
        Option.getOrElse(workspace.clippy, () => root),
        ['.clippy.toml', 'clippy.toml'],
    );
    const compiler =
        Option.isSome(clippy) || workspace.linted(crate).has('clippy')
            ? { tool: 'clippy', check: ['clippy'] as const, fix: ['clippy', '--fix'] as const, inputs: [...Option.toArray(Option.map(clippy, (file) => tokened(path, file))), { env: CLIPPY_CONF_DIR }] }
            : { tool: 'cargo', check: ['check'] as const, fix: ['fix'] as const, inputs: [] };
    const libraries = Array.getSomes((workspace.metadata.metadata?.dylint?.libraries ?? []).map((declared) => Option.map(Option.fromUndefinedOr(declared.path), (relative) => path.join(workspace.root, relative)))).flatMap((folder) => {
        const [sources, others] = Array.partition(
            Option.match(
                Array.findFirst(workspace.members, (member) => member.root === folder),
                { onNone: () => [], onSome: (member) => member.crate.targets.map((owner) => [owner, path.join(folder, path.relative(path.dirname(member.crate.manifest_path), path.dirname(owner.src_path)))] as const) },
            ),
            ([owner, at]) => (library(owner) ? Result.succeed(at) : Result.fail(at)),
        );
        const found = Array.dedupe(sources);
        const included = Array.isReadonlyArrayNonEmpty(found) ? found : [folder];
        const excluded = Array.difference(Array.dedupe([...others, ...workspace.members.map((member) => member.root).filter((nested) => nested.startsWith(`${folder}/`))]), included);
        return [...included.map((at) => tokened(path, path.join(at, '**/*'))), ...excluded.map((at) => `!${tokened(path, path.join(at, '**/*'))}`)];
    });
    const built = (profile: readonly string[]): Array.NonEmptyReadonlyArray<Command> => [{ args: ['build', ...profile, '--keep-going', ...locked()], report: 'messages' }];
    const installed = (profile: readonly string[]): Array.NonEmptyReadonlyArray<Command> => [{ args: ['install', '--path', path.relative(workspace.root, root) || '.', ...profile, ...locked()], report: 'status' }];
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
                test: target(plugin, Option.isSome(nextest) ? 'nextest run' : 'test', `Tests ${crate.name}`, { ...selected, commands }, { cache: true, inputs: [...shared, ...Option.toArray(Option.map(nextest, (file) => tokened(path, file))), { env: 'NEXTEST_PROFILE' }], ...Option.getOrElse(covered, () => ({})) }),
            }),
        }),
        [`check--${compiler.tool}`]: target(plugin, compiler.check.join(' '), `Reports ${compiler.tool} findings for ${crate.name}`, { ...selected, commands: checked((selection) => ({ args: [...compiler.check, ...selection] })) }, { cache: true, inputs: [...shared, ...compiler.inputs] }),
        [`fix--${compiler.tool}`]: target(plugin, compiler.fix.join(' '), `Applies ${compiler.tool} suggestions to ${crate.name}`, { ...selected, commands: [{ args: [...compiler.fix, '--allow-dirty', ...filters, '--keep-going'], report: 'messages' }] }, { cache: false }),
        ...(crate.targets.some(binary)
            ? {
                  'install--cargo': target(
                      plugin,
                      'install',
                      `Builds the binaries of ${crate.name} and installs them into Cargo's install root, in release unless a configuration names a profile`,
                      { cwd: workspace.root, commands: installed([]) },
                      { cache: false, configurations: Record.fromIterableWith(workspace.profiles, (profile) => [profile, { commands: installed(['--profile', profile]) }] as const) },
                  ),
              }
            : {}),
        ...(libraries.length === 0
            ? {}
            : {
                  'check--dylint': target(plugin, 'dylint', `Reports Dylint findings for ${crate.name}`, { ...selected, commands: checked((selection) => ({ args: ['dylint', '--all', '--fail-on-no-libraries'], trailing: selection })) }, { cache: true, inputs: [...shared, ...libraries, { env: 'DYLINT_RUSTFLAGS' }] }),
                  'fix--dylint': target(plugin, 'dylint', `Applies Dylint suggestions to ${crate.name}`, { ...selected, commands: [{ args: ['dylint', '--all', '--fail-on-no-libraries', '--fix'], trailing: ['--allow-dirty', ...filters, '--keep-going'], report: 'messages' }] }, { cache: false }),
              }),
    };
};

const workspaceTargets = (plugin: Plugin, path: Path.Path, workspace: Workspace): Targets => {
    const manifests = Array.dedupe([...workspace.manifests, path.join(workspace.root, MANIFEST)]).map((file) => tokened(path, file));
    const lock = tokened(path, path.join(workspace.root, LOCK));
    return {
        ...Option.match(workspace.deny, {
            onNone: () => ({}),
            onSome: ({ files, tables }) =>
                Record.fromEntries(
                    DENIED.flatMap(({ target: name, checks, cache }) =>
                        Array.match(Array.intersection(checks, tables), {
                            onEmpty: () => [],
                            onNonEmpty: (selected) => [
                                [
                                    name,
                                    target(
                                        plugin,
                                        'deny',
                                        `Reports cargo-deny ${selected.join(', ')} findings over the workspace graph, failing on any warning`,
                                        { cwd: workspace.root, commands: [{ args: ['deny', '--workspace', ...locked(), 'check', '--deny', 'warnings', ...selected], report: 'status' }] },
                                        { cache, inputs: [...manifests, lock, ...files.map((file) => tokened(path, file))] },
                                    ),
                                ] as const,
                            ],
                        }),
                    ),
                ),
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

const unresolved = (plugin: Plugin, root: string): Targets => ({
    'check--cargo': target(plugin, 'check', `Reports the cargo error that keeps the manifest in ${root} from resolving`, { cwd: root, commands: [{ args: ['check'], report: 'status' }] }, { cache: false }),
});

const released = (plugin: Plugin, path: Path.Path, crate: Crate, cwd: string): Pick<ProjectConfiguration, 'release' | 'tags'> & { readonly targets: Targets } => {
    const [registry, ...others] = crate.publish ?? [];
    const named = registry === undefined || others.length > 0 || registry === 'crates-io' ? {} : { registry };
    return crate.publish?.length === 0
        ? { targets: {} }
        : {
              tags: ['release'],
              release: { version: { versionActions: `${plugin.name}/release/version-actions`, versionActionsOptions: { lock: path.join(cwd, LOCK), ...named } } },
              targets: {
                  'nx-release-publish': {
                      executor: `${plugin.name}:release-publish`,
                      dependsOn: ['^nx-release-publish'],
                      options: { cwd, package: crate.name, ...named },
                      metadata: { technologies: TECHNOLOGIES, description: `Publishes ${crate.name} with cargo publish` },
                  },
              },
          };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { aggregated, crateTargets, released, TECHNOLOGIES, unresolved, workspaceTargets };
