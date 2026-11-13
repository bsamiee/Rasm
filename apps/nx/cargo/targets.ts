import { basename, dirname, join, parse, relative } from 'node:path';
import type { TargetConfiguration } from '@nx/devkit';
import { ancestors, type Members } from '@rasm/nx-devkit/project';
import { Array, Equivalence, Option, Record, Result, String } from 'effect';
import type { Cargo, Command } from '#/executors/cargo/executor';
import { LOCK, MANIFEST, type Workspace } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Member = Workspace['members'][number];
type Target = Member['crate']['targets'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = ['.cargo/config.toml', '.cargo/config', 'rust-toolchain.toml', 'rust-toolchain'] as const;
const CLIPPY = ['clippy.toml', '.clippy.toml'] as const;
const NEXTEST = '.config/nextest.toml';
const DENIED = [
    { tool: 'cargo-deny', checks: ['bans', 'licenses', 'sources'], extra: { cache: true } },
    { tool: 'cargo-deny-advisories', checks: ['advisories'], extra: {} },
] as const;
const COMPILER = ['RUSTC', 'RUSTC_WRAPPER', 'RUSTC_WORKSPACE_WRAPPER', 'RUSTDOC', 'RUSTFLAGS', 'CARGO_ENCODED_RUSTFLAGS', 'RUSTDOCFLAGS', 'CARGO_ENCODED_RUSTDOCFLAGS', 'RUSTUP_TOOLCHAIN', 'CARGO_BUILD_TARGET'] as const;
const HOST = { runtime: 'rustc --print host-tuple' } as const;
const MESSAGES = ['--message-format', 'json-diagnostic-rendered-ansi'] as const;
const SUITES = { args: ['--cargo-message-format', 'json-diagnostic-rendered-ansi', '--message-format', 'libtest-json-plus', '--message-format-version', '0.1'], env: { NEXTEST_EXPERIMENTAL_LIBTEST_JSON: '1' } } as const;
const PLUGIN = '@rasm/nx-cargo';
const LOCKED = '--locked';
const SOURCES = ['default', '^default'] as const;

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

const enabled = (features: Member['crate']['features'], names: readonly string[]): readonly string[] => names.map((name) => Array.headNonEmpty(String.split(name, '/'))).flatMap((name) => Option.match(Record.get(features, name), { onNone: () => [], onSome: (implied) => [name, ...enabled(features, implied)] }));

const tokened = (file: string) => join('{workspaceRoot}', file);

const target = (help: string, description: string, options: typeof Cargo.Type, extra: Partial<TargetConfiguration>): TargetConfiguration => ({
    executor: `${PLUGIN}:cargo`,
    options,
    metadata: { description, help: { command: `cargo ${help} --help`, example: { args: ['--verbose'] } } },
    ...extra,
});

const crateInputs = (workspace: Workspace, files: ReadonlySet<string>, { crate, root }: Member, stated: TargetConfiguration['inputs']): NonNullable<TargetConfiguration['inputs']> => {
    const folders = Array.dedupe(crate.targets.map(({ src_path }) => relative(dirname(crate.manifest_path), dirname(src_path))));
    const placed = (at: string) => (at.startsWith('..') ? tokened(join(root, at)) : join('{projectRoot}', at));
    return [
        ...(stated ?? (folders.includes('') ? ['{projectRoot}/**/*'] : [join('{projectRoot}', MANIFEST), ...folders.map((at) => join(placed(at), '**/*')), ...Array.fromNullishOr(crate.readme).map(placed)])),
        tokened(join(workspace.root, LOCK)),
        ...(workspace.root === root ? [] : [tokened(join(workspace.root, MANIFEST))]),
        ...ancestors(workspace.root).flatMap((at) =>
            CONFIG.map((name) => join(at, name))
                .filter((file) => files.has(file))
                .map(tokened),
        ),
        HOST,
        ...COMPILER.map((env) => ({ env })),
    ];
};

const crateTargets = (workspace: Workspace, files: ReadonlySet<string>, reported: Option.Option<string>, { crate, root }: Member) => {
    const selected = { cwd: workspace.root, package: crate.name };
    const nextest = Option.liftPredicate(join(workspace.root, NEXTEST), (file) => files.has(file));
    const plain = Option.isNone(nextest);
    const tested = crate.targets.some(({ test }) => test);
    const documented = crate.targets.some(({ doctest }) => doctest);
    const filters = [
        ...Record.keys(Record.filter(SELECTIONS, (selects) => crate.targets.some(selects))),
        ...Array.match(Array.difference(Record.keys(crate.features), enabled(crate.features, ['default'])), {
            onEmpty: () => [],
            onNonEmpty: (features) => ['--features', features.map((feature) => `${crate.name}/${feature}`).join(',')],
        }),
    ];
    const products = crate.targets.filter(own).flatMap(workspace.uplifted);
    const written = (folder: string) =>
        Option.match(workspace.placed, {
            onNone: () => [],
            onSome: ({ artifacts, units }) => [join(units, folder, 'build', crate.name), ...products.flatMap((file) => [file, `${parse(file).name}.d`]).map((file) => join(artifacts, folder, file))].map(tokened),
        });
    const profiles = ['release', ...workspace.profiles];
    const linted = [...[...files].filter((file) => CLIPPY.some((name) => basename(file) === name)).map(tokened), { env: 'CLIPPY_CONF_DIR' }];
    const libraries = workspace.libraries.flatMap((folder) => {
        const [sources, others] = Array.partition(
            Option.match(
                Array.findFirst(workspace.members, (member) => member.root === folder),
                { onNone: () => [], onSome: (member) => member.crate.targets.map((owner) => [owner, join(folder, relative(dirname(member.crate.manifest_path), dirname(owner.src_path)))] as const) },
            ),
            ([owner, at]) => (library(owner) ? Result.succeed(at) : Result.fail(at)),
        );
        const found = Array.dedupe(sources);
        const included = Array.isReadonlyArrayNonEmpty(found) ? found : [folder];
        const excluded = Array.difference(Array.dedupe([...others, ...workspace.members.map((member) => member.root).filter((nested) => nested.startsWith(`${folder}/`))]), included);
        return [...included.map((at) => tokened(join(at, '**/*'))), ...excluded.map((at) => `!${tokened(join(at, '**/*'))}`)];
    });
    const built = (profile: readonly string[]): Array.NonEmptyReadonlyArray<Command> => [{ args: ['build', ...profile, '--keep-going', ...MESSAGES] }];
    const installed = (profile: readonly string[]): Array.NonEmptyReadonlyArray<Command> => [{ args: ['install', '--path', relative(workspace.root, root) || '.', ...profile, LOCKED] }];
    const checked = (placed: (selection: readonly string[]) => Command): Array.NonEmptyReadonlyArray<Command> =>
        Array.map(
            Array.prepend(
                workspace.strategies.map((profile) => ['--profile', profile]),
                [],
            ),
            (profile) => placed([...filters, ...profile, LOCKED, '--keep-going', ...MESSAGES]),
        );
    const covered = (run: (exported: readonly string[]) => Command): Partial<TargetConfiguration> =>
        Option.match(workspace.placed, {
            onNone: () => ({}),
            onSome: ({ artifacts }) => {
                const report = tokened(Option.getOrElse(reported, () => join(artifacts, 'llvm-cov', `${crate.name}.lcov`)));
                const packages = workspace.members.filter((member) => crate.dependencies.some(({ name, path: to }) => to !== undefined && name === member.crate.name)).map((member) => ({ root: member.root, name: member.crate.name }));
                return { outputs: Option.toArray(reported).map(() => report), configurations: { coverage: { commands: [{ ...run(['--lcov', '--output-path', report]), packages }], outputs: [report] } } };
            },
        });
    const suites = (run: Array.NonEmptyReadonlyArray<string>): Command => ({ args: Array.appendAll(run, [LOCKED, ...SUITES.args]), env: SUITES.env });
    const units = (run: Array.NonEmptyReadonlyArray<string>): Command => ({ args: Array.appendAll(run, [...(plain ? [] : ['--doc']), LOCKED]) });
    const dylint =
        libraries.length === 0
            ? { check: {}, fix: {} }
            : {
                  check: {
                      dylint: target('dylint', `Reports Dylint findings for ${crate.name}`, { ...selected, commands: checked((selection) => ({ args: ['dylint', '--all', '--fail-on-no-libraries'], trailing: selection })) }, { cache: true, inputs: [...SOURCES, ...libraries, { env: 'DYLINT_RUSTFLAGS' }] }),
                  },
                  fix: { dylint: target('dylint', `Applies Dylint suggestions to ${crate.name}`, { ...selected, commands: [{ args: ['dylint', '--all', '--fail-on-no-libraries', '--fix'], trailing: ['--allow-dirty', ...filters, '--keep-going', ...MESSAGES] }] }, {}) },
              };
    return {
        build: crate.targets.some(own)
            ? {
                  cargo: target(
                      'build',
                      `Builds ${crate.name} with cargo, bringing Cargo.lock in line with the manifests`,
                      { ...selected, commands: built([]), outputs: written('debug') },
                      {
                          cache: Option.isSome(workspace.placed),
                          inputs: [...SOURCES],
                          outputs: written('debug').map((_, index) => `{options.outputs.${index}}`),
                          configurations: Record.fromIterableWith(profiles, (profile) => [profile, { commands: built(['--profile', profile]), outputs: written(profile) }] as const),
                      },
                  ),
              }
            : {},
        test: {
            ...(!plain &&
                tested && {
                    nextest: {
                        ...target(
                            'nextest run',
                            `Tests ${crate.name} with cargo nextest`,
                            { ...selected, commands: [suites(['nextest', 'run'])] },
                            { cache: true, inputs: [...SOURCES, ...Option.toArray(nextest).map(tokened), { env: 'NEXTEST_PROFILE' }], ...covered((exported) => suites(['llvm-cov', 'nextest', ...exported])) },
                        ),
                        executor: `${PLUGIN}:nextest`,
                    },
                }),
            ...((plain ? tested || documented : documented) && {
                cargo: target(
                    'test',
                    plain ? `Tests ${crate.name} with cargo test` : `Runs the documentation tests of ${crate.name} with cargo test --doc`,
                    { ...selected, commands: [units(['test'])] },
                    { cache: true, inputs: [...SOURCES], ...(plain ? covered((exported) => units(['llvm-cov', 'test', ...exported])) : {}) },
                ),
            }),
        },
        check: {
            clippy: target('clippy', `Reports clippy findings for ${crate.name}`, { ...selected, commands: checked((selection) => ({ args: ['clippy', ...selection] })) }, { cache: true, inputs: [...SOURCES, ...linted] }),
            ...dylint.check,
        },
        clean: {
            cargo: {
                options: { commands: [['cargo', 'clean', '--package', crate.name, LOCKED]], cwd: workspace.root },
                metadata: { description: `Removes the build artifacts of ${crate.name} in every profile with cargo clean`, help: { command: 'cargo clean --help', example: { args: ['--release'] } } },
            },
        },
        fix: {
            clippy: target('clippy --fix', `Applies clippy suggestions to ${crate.name}`, { ...selected, commands: [{ args: ['clippy', '--fix', '--allow-dirty', ...filters, '--keep-going', ...MESSAGES] }] }, {}),
            ...dylint.fix,
        },
        install: crate.targets.some(binary)
            ? {
                  cargo: target(
                      'install',
                      `Builds the binaries of ${crate.name} and installs them into Cargo's install root, in release unless a configuration names a profile`,
                      { cwd: workspace.root, commands: installed([]) },
                      { inputs: [...SOURCES], configurations: Record.fromIterableWith(workspace.profiles, (profile) => [profile, { commands: installed(['--profile', profile]) }] as const) },
                  ),
              }
            : {},
    } satisfies Members;
};

const workspaceTargets = (workspace: Workspace) => {
    const manifests = Array.dedupe([...workspace.members.map((member) => member.manifest), join(workspace.root, MANIFEST)]).map(tokened);
    const lock = tokened(join(workspace.root, LOCK));
    return {
        check: Option.match(workspace.deny, {
            onNone: () => ({}),
            onSome: ({ files, tables }) =>
                Record.fromEntries(
                    DENIED.flatMap(({ tool, checks, extra }) =>
                        Array.match(Array.intersection(checks, tables), {
                            onEmpty: () => [],
                            onNonEmpty: (selected) => [
                                [
                                    tool,
                                    target(
                                        'deny',
                                        `Reports cargo-deny ${selected.join(', ')} findings over the workspace graph, failing on any warning`,
                                        { cwd: workspace.root, commands: [{ args: ['deny', LOCKED, '--workspace', 'check', '--deny', 'warnings', ...selected] }] },
                                        { ...extra, inputs: [...manifests, lock, ...files.map(tokened)] },
                                    ),
                                ] as const,
                            ],
                        }),
                    ),
                ),
        }),
        clean: {
            cargo: {
                options: { commands: [['cargo', 'clean']], cwd: workspace.root },
                metadata: { description: 'Removes the build folder of the workspace with cargo clean', help: { command: 'cargo clean --help', example: { args: ['--release'] } } },
            },
        },
        upgrade: {
            cargo: {
                executor: 'nx:run-commands',
                options: { commands: ['cargo upgrade --incompatible allow', 'cargo update'], parallel: false, cwd: workspace.root },
                metadata: { description: 'Moves every dependency requirement to its newest release with cargo upgrade, then the lock with cargo update', help: { command: 'cargo upgrade --help', example: { args: ['--dry-run'] } } },
            },
        },
    } satisfies Members;
};

const released = ({ crate }: Member, cwd: string) => {
    const [registry, ...others] = crate.publish ?? [];
    const named = registry === undefined || others.length > 0 || registry === 'crates-io' ? {} : { registry };
    return crate.publish?.length === 0
        ? {}
        : {
              release: { lock: join(cwd, LOCK), ...named },
              publish: { executor: `${PLUGIN}:release-publish`, options: { cwd, package: crate.name, ...named }, metadata: { description: `Publishes ${crate.name} with cargo publish` } },
          };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { CLIPPY, CONFIG, crateInputs, crateTargets, NEXTEST, PLUGIN, released, workspaceTargets };
