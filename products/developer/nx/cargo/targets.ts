import { dirname, posix, relative } from 'node:path';
import { normalizePath, type TargetConfiguration } from '@nx/devkit';
import { type Anchor, aggregated, EXECUTORS, removable, type Verbs } from '@rasm/nx';
import { Array, Option, Record, Schema } from 'effect';
import type { Cargo, Command } from '#/executors/cargo/executor';
import plugin from '#/package.json' with { type: 'json' };
import { ACTIONS, LOCK, MANIFEST, type Member, Target, tokened, type Workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DENIED = [
    { tool: 'cargo-deny', checks: ['bans', 'licenses', 'sources'] },
    { tool: 'cargo-deny-advisories', checks: ['advisories'] },
] as const;
const LOCKED = '--locked';
const COMPILED = [LOCKED, '--keep-going', '--message-format', 'json-diagnostic-rendered-ansi'] as const;
const SOURCES: NonNullable<TargetConfiguration['inputs']> = ['default', '^default', { externalDependencies: [] }];

// --- [MODELS] --------------------------------------------------------------------------

class Options extends Schema.Opaque<Options, unknown>()(
    Schema.Struct({ coveragePath: Schema.optionalKey(Schema.String) }).annotate({
        description:
            'coveragePath: workspace folder the coverage configuration of a crate writes its lcov.info report to through cargo llvm-cov --output-path, {projectName} naming the project, absent leaves the report on stdout, and a Cargo workspace at the Nx workspace root names each source by its path from that root through --remap-path-prefix',
    }),
) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const version = (tool: string) => ({ runtime: `cargo ${tool} --version` });

const suites = (run: Array.NonEmptyReadonlyArray<string>): Command => ({ args: Array.appendAll(run, [LOCKED, '--cargo-message-format', 'json-diagnostic-rendered-ansi', '--message-format', 'libtest-json-plus', '--message-format-version', '0.1']), env: { NEXTEST_EXPERIMENTAL_LIBTEST_JSON: '1' } });

const cargoTarget = (help: string, description: string, options: Cargo, extra: Partial<TargetConfiguration>): TargetConfiguration => ({
    executor: `${plugin.name}:cargo`,
    options,
    inputs: SOURCES,
    metadata: { description, help: { command: `cargo ${help} --help`, example: { args: ['--verbose'] } } },
    ...extra,
});

const joined = (left: Verbs, right: Verbs): Verbs => Record.union(left, right, (held, added) => ({ ...held, ...added }));

const crateInputs = (workspace: Workspace, { crate, root }: Member, stated: TargetConfiguration['inputs']): NonNullable<TargetConfiguration['inputs']> => {
    const folders = Array.dedupe(crate.targets.map(({ src_path }) => normalizePath(relative(dirname(crate.manifest_path), dirname(src_path)))));
    return [
        ...(stated ?? (folders.includes('') ? ['{projectRoot}/**/*'] : [posix.join('{projectRoot}', MANIFEST), ...[...folders.map((at) => posix.join(at, '**/*')), ...Array.fromNullishOr(crate.readme)].map((at) => tokened(posix.join(root, at)))])),
        tokened(posix.join(workspace.root, LOCK)),
        tokened(posix.join(workspace.root, MANIFEST)),
        ...workspace.unified,
        ...workspace.configs,
        { runtime: 'rustc -vV' },
        ...workspace.compiler.map((env) => ({ env })),
    ];
};

const crateTargets = (workspace: Workspace, coverage: Option.Option<string>, { crate, root, packages, linted }: Member): Verbs => {
    const selected = { cwd: workspace.root, package: crate.name, unification: workspace.unification };
    const kinds = [...(crate.targets.some(Target.example) ? [...(crate.targets.some(Target.library) ? ['--lib'] : []), '--bins', '--tests', '--benches'] : ['--all-targets']), '--all-features'];
    const plain = Option.isNone(workspace.nextest);
    const tested = crate.targets.some(({ test }) => test);
    const documented = crate.targets.some(({ doctest }) => doctest);
    const products = crate.targets.filter(Target.own).flatMap(workspace.uplifted);
    const outputs = (profile: string) => Option.toArray(workspace.placed).flatMap((artifacts) => products.map(({ folder, file }) => tokened(posix.join(artifacts, ...folder, profile, file))));
    const debug = outputs('debug');
    const profiles = ['release', ...workspace.profiles];
    const built = (profile: readonly string[]): Array.NonEmptyReadonlyArray<Command> => [{ args: ['build', ...profile, ...COMPILED] }];
    const installed = (profile: readonly string[]): Array.NonEmptyReadonlyArray<Command> => [{ args: ['install', '--path', posix.relative(workspace.root, root) || '.', ...profile, ...COMPILED] }];
    const checked = (placed: (selection: readonly string[]) => Command) => Array.map(Array.prepend(workspace.strategies.map(Option.some), Option.none()), (profile) => placed([...kinds, ...Option.toArray(profile).flatMap((name) => ['--profile', name]), ...COMPILED]));
    const covered = (run: (exported: readonly string[]) => Command): Partial<TargetConfiguration> => {
        const report = Option.toArray(coverage);
        const direct = packages.filter(({ name }) => crate.dependencies.some((dependency) => dependency.path !== undefined && dependency.name === name));
        const files = report.map(tokened);
        return {
            outputs: files.map((_, index) => `{options.outputs.${index}}`),
            configurations: { coverage: { commands: [{ ...run(['--lcov', ...(workspace.root === '.' ? ['--remap-path-prefix'] : []), ...report.flatMap((file) => ['--output-path', posix.relative(workspace.root, file)])]), packages: direct }], outputs: files } },
        };
    };
    const libtest = (run: Array.NonEmptyReadonlyArray<string>): Command => ({ args: Array.appendAll(run, [...(plain ? [] : ['--doc']), LOCKED]) });
    const linting: Verbs =
        workspace.libraries.length > 0
            ? {
                  check: {
                      dylint: cargoTarget(
                          'dylint',
                          `Reports the Dylint findings of ${crate.name} with cargo dylint`,
                          { ...selected, commands: checked((selection) => ({ args: ['dylint', '--all', '--fail-on-no-libraries'], trailing: selection })) },
                          { cache: true, inputs: [...SOURCES, ...workspace.libraries, { env: 'DYLINT_RUSTFLAGS' }, version('dylint')] },
                      ),
                  },
                  fix: { dylint: cargoTarget('dylint', `Applies the Dylint suggestions for ${crate.name} with cargo dylint --fix`, { ...selected, commands: [{ args: ['dylint', '--all', '--fail-on-no-libraries', '--fix'], trailing: ['--allow-dirty', ...kinds, ...COMPILED] }] }, {}) },
              }
            : {};
    return joined(linting, {
        build: {
            ...(crate.targets.some(Target.own) && {
                cargo: cargoTarget(
                    'build',
                    `Builds ${crate.name} with cargo build from the committed Cargo.lock`,
                    { ...selected, commands: built([]), outputs: debug },
                    {
                        ...(Option.isSome(workspace.placed) && { cache: true }),
                        outputs: debug.map((_, index) => `{options.outputs.${index}}`),
                        configurations: Record.fromIterableWith(profiles, (profile) => [profile, { commands: built(['--profile', profile]), outputs: outputs(profile) }] as const),
                    },
                ),
            }),
        },
        test: {
            ...(!plain &&
                tested && {
                    nextest: {
                        ...cargoTarget(
                            'nextest run',
                            `Tests ${crate.name} with cargo nextest run`,
                            { ...selected, commands: [suites(['nextest', 'run'])] },
                            {
                                cache: true,
                                inputs: [...SOURCES, ...Option.toArray(workspace.nextest).map(tokened), { env: 'NEXTEST_PROFILE' }, version('nextest'), version('llvm-cov')],
                                ...covered((exported) => suites(['llvm-cov', 'nextest', ...exported])),
                            },
                        ),
                        executor: `${plugin.name}:nextest`,
                    },
                }),
            ...((plain ? tested || documented : documented) && {
                cargo: cargoTarget(
                    'test',
                    plain ? `Tests ${crate.name} with cargo test` : `Runs the documentation tests of ${crate.name} with cargo test --doc`,
                    { ...selected, commands: [libtest(['test'])] },
                    plain ? { cache: true, inputs: [...SOURCES, version('llvm-cov')], ...covered((exported) => libtest(['llvm-cov', 'test', ...exported])) } : { cache: true },
                ),
            }),
        },
        check: {
            rustdoc: cargoTarget('doc', `Reports the rustdoc lint findings of ${crate.name} with cargo doc --no-deps`, { ...selected, commands: [{ args: ['doc', '--no-deps', ...COMPILED] }] }, { cache: true }),
            clippy: cargoTarget('clippy', `Reports the clippy findings of ${crate.name} with cargo clippy`, { ...selected, commands: checked((selection) => ({ args: ['clippy', ...selection] })) }, { cache: true, inputs: [...SOURCES, ...linted, { env: 'CLIPPY_CONF_DIR' }] }),
        },
        fix: {
            clippy: cargoTarget('clippy --fix', `Applies the clippy suggestions for ${crate.name} with cargo clippy --fix`, { ...selected, commands: [{ args: ['clippy', '--fix', '--allow-dirty', ...kinds, ...COMPILED] }] }, {}),
        },
        install: {
            ...(crate.targets.some(Target.binary) && {
                cargo: cargoTarget(
                    'install',
                    `Installs the binaries of ${crate.name} into Cargo's install root with cargo install --path, building in the workspace target folder, in release unless a configuration names a profile`,
                    { cwd: workspace.root, unification: workspace.unification, commands: installed([]) },
                    Array.match(workspace.profiles, { onEmpty: () => ({}), onNonEmpty: (named) => ({ configurations: Record.fromIterableWith(named, (profile) => [profile, { commands: installed(['--profile', profile]) }] as const) }) }),
                ),
            }),
        },
    });
};

const workspaceTargets = (workspace: Workspace): Verbs => {
    const graph = [...[...Array.dedupe([...workspace.members.map((member) => member.manifest), posix.join(workspace.root, MANIFEST)]), posix.join(workspace.root, LOCK)].map(tokened), ...workspace.configs];
    const check = Record.fromIterableWith(
        workspace.deny.flatMap(({ files, tables }) => DENIED.map((row) => ({ ...row, files, selected: Array.intersection(row.checks, tables) }))).filter(({ selected }) => selected.length > 0),
        ({ tool, files, selected }): readonly [string, TargetConfiguration] => [
            tool,
            {
                cache: true,
                executor: 'nx:run-commands',
                options: { cwd: workspace.root, command: ['cargo', 'deny', LOCKED, '--workspace', 'check', '--deny', 'warnings', ...selected].join(' ') },
                inputs: [...graph, ...files.map(tokened), version('deny'), { externalDependencies: [] }],
                metadata: { description: `Reports the cargo-deny ${selected.join(', ')} findings of the workspace graph with cargo deny check, failing on any warning, outside the Cargo batch as it reads the whole graph once`, help: { command: 'cargo deny check --help', example: { args: ['--show-stats'] } } },
            },
        ],
    );
    return {
        check,
        upgrade: {
            cargo: {
                executor: `${plugin.name}:upgrade`,
                inputs: [...graph, { externalDependencies: [] }],
                options: { cwd: workspace.root },
                metadata: { description: 'Moves every dependency requirement to its newest release, prereleases above a stable line included, with cargo upgrade, then the lock with cargo update', help: { command: 'cargo upgrade --help', example: { options: { dryRun: true } } } },
            },
        },
    };
};

const projectTargets = (workspace: Workspace, coverage: Option.Option<string>, member: Option.Option<Member>, stated: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>, anchor: Anchor, published: Readonly<Record<string, TargetConfiguration>>): Record<string, TargetConfiguration> => {
    const nested = Option.filter(member, ({ root }) => root !== workspace.root);
    const shipped = Option.filter(member, ({ versioned, packable }) => versioned && packable);
    const packed = Option.zipWith(shipped, workspace.placed, ({ crate, packages }, artifacts) =>
        cargoTarget(
            'package',
            `Packs ${crate.name} into a .crate archive with cargo package beside the workspace crates it depends on, without the verification build`,
            { cwd: workspace.root, package: crate.name, unification: workspace.unification, commands: [{ args: ['package', '--no-verify', LOCKED], packages }] },
            { cache: true, outputs: [tokened(posix.join(artifacts, 'package', `${crate.name}-${crate.version}.crate`))] },
        ),
    );
    const tools = [...Option.toArray(Option.map(member, (held) => crateTargets(workspace, coverage, held))), ...(Option.isNone(nested) ? [workspaceTargets(workspace)] : []), { pack: Record.getSomes({ cargo: packed }) }].reduce(joined);
    const removed = removable(tools);
    const cleaned = Option.match(nested, {
        onNone: () => ({ commands: [['cargo', 'clean']] as const, cache: { folders: [['CARGO_HOME', 'log']] }, description: 'Removes the build folder of the workspace with cargo clean, and under configuration cache empties the build analysis logs cargo writes to $CARGO_HOME/log' }),
        onSome: ({ crate }) => {
            const profiles = ['dev', 'release', ...workspace.profiles];
            return { commands: profiles.map((profile) => ['cargo', 'clean', '--package', crate.name, '--profile', profile, LOCKED] as const), cache: {}, description: `Removes the build artifacts of ${crate.name} in profiles ${profiles.join(', ')} with cargo clean --profile` };
        },
    });
    return aggregated(
        {
            ...tools,
            attach: Record.getSomes({
                gh: Option.as(packed, {
                    executor: EXECUTORS.attach,
                    dependsOn: ['pack--cargo'],
                    inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                    metadata: {
                        description: "Uploads the file pack--cargo wrote to the GitHub release of the version the project's manifest states with gh release upload, creating that release with gh release create where none exists",
                        help: { command: 'gh release upload --help', example: { options: { dryRun: true } } },
                    },
                }),
            }),
            clean: {
                cargo: {
                    executor: EXECUTORS.clean,
                    inputs: SOURCES,
                    options: { commands: cleaned.commands, cwd: workspace.root, targets: removed },
                    configurations: { cache: cleaned.cache },
                    metadata: { description: cleaned.description, help: { command: 'cargo clean --help', example: { args: ['--dry-run'] } } },
                },
            },
        },
        stated,
        'rust',
        anchor,
        published,
    );
};

const released = ({ crate, versioned, packages }: Member, cwd: string) => {
    const [registry, ...others] = crate.publish ?? [];
    const named = registry === undefined || others.length > 0 || registry === 'crates-io' ? {} : { registry };
    const published: Record<string, TargetConfiguration> =
        crate.publish?.length === 0
            ? {}
            : {
                  'nx-release-publish': {
                      executor: `${plugin.name}:release-publish`,
                      options: { cwd, package: crate.name, packages, ...named },
                      dependsOn: ['^nx-release-publish'],
                      inputs: SOURCES,
                      metadata: { technologies: ['rust'], description: `Publishes ${crate.name} with cargo publish at the version nx release tagged at the checked-out commit, skipping a version cargo info finds in its registry`, help: { command: 'cargo publish --help', example: { options: { dryRun: true } } } },
                  },
              };
    return versioned ? { tags: ['release'], release: { version: { versionActions: ACTIONS, versionActionsOptions: { lock: posix.join(cwd, LOCK), ...named } } }, targets: published } : { targets: {} };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { crateInputs, Options, projectTargets, released };
