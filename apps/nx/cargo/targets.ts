import { dirname, join, parse, relative } from 'node:path';
import type { TargetConfiguration } from '@nx/devkit';
import { mergeTargetConfigurations } from '@nx/devkit/internal';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Option, Predicate, Record, Schema, String } from 'effect';
import type { Cargo, Command } from '#/executors/cargo/executor';
import { binary, LOCK, library, MANIFEST, type Member, own, PLUGIN, type Target, tokened, type Workspace } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;
type Reports = Readonly<Record<'coverage' | 'junit', Option.Option<string>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const DENIED = [
    { tool: 'cargo-deny', checks: ['bans', 'licenses', 'sources'], extra: { cache: true } },
    { tool: 'cargo-deny-advisories', checks: ['advisories'], extra: {} },
] as const;
const HOST = { runtime: 'rustc --print host-tuple' } as const;
const MESSAGES = ['--message-format', 'json-diagnostic-rendered-ansi'] as const;
const SUITES = { args: ['--cargo-message-format', 'json-diagnostic-rendered-ansi', '--message-format', 'libtest-json-plus', '--message-format-version', '0.1'], env: { NEXTEST_EXPERIMENTAL_LIBTEST_JSON: '1' } } as const;
const LOCKED = '--locked';
const SOURCES: NonNullable<TargetConfiguration['inputs']> = ['default', '^default', { externalDependencies: [] }];

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), coveragePath: Schema.optionalKey(Schema.String) }).annotate({
    description:
        'outputPath: workspace folder test--nextest writes its junit.xml report to, {projectName} naming the project, absent leaves JUnit to the nextest profile. coveragePath: workspace folder the coverage configuration of a crate writes its lcov.info report to through cargo llvm-cov --output-path, {projectName} naming the project, absent leaves the report on stdout',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const SELECTIONS: Readonly<Record<string, (target: Target) => boolean>> = {
    '--lib': library,
    '--bins': binary,
    '--tests': ({ test }) => test,
    '--benches': ({ kind }) => kind.includes('bench'),
};

const version = (tool: string) => ({ runtime: `cargo ${tool} --version` });

const enabled = (features: Member['crate']['features'], names: readonly string[]): readonly string[] => names.map((name) => Array.headNonEmpty(String.split(name, '/'))).flatMap((name) => Option.match(Record.get(features, name), { onNone: () => [], onSome: (implied) => [name, ...enabled(features, implied)] }));

const cargoTarget = (help: string, description: string, options: typeof Cargo.Type, extra: Partial<TargetConfiguration>): TargetConfiguration => ({
    executor: `${PLUGIN}:cargo`,
    options,
    inputs: SOURCES,
    metadata: { description, help: { command: `cargo ${help} --help`, example: { args: ['--verbose'] } } },
    ...extra,
});

const joined = (left: Verbs, right: Verbs): Verbs => Record.union(left, right, (held, added) => ({ ...held, ...added }));

const aggregated = (verbs: Verbs, stated: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>, technology: string): Record<string, TargetConfiguration> =>
    Record.fromEntries(
        Record.collect(Record.filter(verbs, Predicate.not(Record.isEmptyRecord)), (verb, tools): readonly (readonly [string, TargetConfiguration])[] => [
            [
                verb,
                {
                    dependsOn: [`${verb}--*`],
                    inputs: [
                        '...',
                        ...Array.dedupe(
                            Record.collect(tools, (tool, target) => mergeTargetConfigurations(stated[`${verb}--${tool}`] ?? {}, { ...target, inputs: target.inputs ?? ['default', '^default'] }).inputs ?? [])
                                .flat()
                                .map((input) => (Predicate.isObject(input) && 'dependentTasksOutputFiles' in input ? { ...input, transitive: true } : input)),
                        ),
                    ],
                },
            ],
            ...Record.collect(tools, (tool, target): readonly [string, TargetConfiguration] => [`${verb}--${tool}`, { ...target, metadata: { technologies: [technology], ...target.metadata } }]),
        ]).flat(),
    );

const crateInputs = (workspace: Workspace, { crate, root }: Member, stated: TargetConfiguration['inputs']): NonNullable<TargetConfiguration['inputs']> => {
    const folders = Array.dedupe(crate.targets.map(({ src_path }) => relative(dirname(crate.manifest_path), dirname(src_path))));
    const placed = (at: string) => (at.startsWith('..') ? tokened(join(root, at)) : join('{projectRoot}', at));
    return [
        ...(stated ?? (folders.includes('') ? ['{projectRoot}/**/*'] : [join('{projectRoot}', MANIFEST), ...folders.map((at) => join(placed(at), '**/*')), ...Array.fromNullishOr(crate.readme).map(placed)])),
        tokened(join(workspace.root, LOCK)),
        ...(workspace.root === root ? [] : [tokened(join(workspace.root, MANIFEST))]),
        ...workspace.configs,
        HOST,
        ...workspace.compiler.map((env) => ({ env })),
    ];
};

const crateTargets = (workspace: Workspace, reports: Reports, { crate, root, packages, linted }: Member): Verbs => {
    const selected = { cwd: workspace.root, package: crate.name };
    const plain = Option.isNone(workspace.nextest);
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
    const outputs = (folder: string) =>
        Option.match(workspace.placed, {
            onNone: () => [],
            onSome: (artifacts) => products.flatMap((file) => [file, `${parse(file).name}.d`]).map((file) => tokened(join(artifacts, folder, file))),
        });
    const debug = outputs('debug');
    const profiles = ['release', ...workspace.profiles];
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
    const junit = Option.toArray(reports.junit);
    const covered = (run: (exported: readonly string[]) => Command, written: readonly string[]): Partial<TargetConfiguration> => {
        const report = Option.toArray(reports.coverage);
        const direct = packages.filter(({ name }) => crate.dependencies.some((dependency) => dependency.path !== undefined && dependency.name === name));
        const files = [...written, ...report].map(tokened);
        return {
            outputs: files.map((_, index) => `{options.outputs.${index}}`),
            configurations: { coverage: { commands: [{ ...run(['--lcov', ...report.flatMap((file) => ['--output-path', relative(workspace.root, file)])]), packages: direct }], outputs: files } },
        };
    };
    const suites = (run: Array.NonEmptyReadonlyArray<string>): Command => ({ args: Array.appendAll(run, [LOCKED, ...SUITES.args]), env: SUITES.env, ...Option.getOrUndefined(Option.map(reports.junit, (file) => ({ junit: file }))) });
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
                  fix: { dylint: cargoTarget('dylint', `Applies the Dylint suggestions for ${crate.name} with cargo dylint --fix`, { ...selected, commands: [{ args: ['dylint', '--all', '--fail-on-no-libraries', '--fix'], trailing: ['--allow-dirty', ...filters, '--keep-going', ...MESSAGES] }] }, {}) },
              }
            : {};
    return joined(linting, {
        build: {
            ...(crate.targets.some(own) && {
                cargo: cargoTarget(
                    'build',
                    `Builds ${crate.name} with cargo build, bringing Cargo.lock in line with the manifests`,
                    { ...selected, commands: built([]), outputs: debug },
                    {
                        cache: Option.isSome(workspace.placed),
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
                            { ...selected, commands: [suites(['nextest', 'run'])], outputs: junit.map(tokened) },
                            {
                                cache: true,
                                inputs: [...SOURCES, ...Option.toArray(workspace.nextest).map(tokened), { env: 'NEXTEST_PROFILE' }, version('nextest'), version('llvm-cov')],
                                ...covered((exported) => suites(['llvm-cov', 'nextest', ...exported]), junit),
                            },
                        ),
                        executor: `${PLUGIN}:nextest`,
                    },
                }),
            ...((plain ? tested || documented : documented) && {
                cargo: cargoTarget(
                    'test',
                    plain ? `Tests ${crate.name} with cargo test` : `Runs the documentation tests of ${crate.name} with cargo test --doc`,
                    { ...selected, commands: [libtest(['test'])] },
                    plain ? { cache: true, inputs: [...SOURCES, version('llvm-cov')], ...covered((exported) => libtest(['llvm-cov', 'test', ...exported]), []) } : { cache: true },
                ),
            }),
        },
        check: {
            clippy: cargoTarget('clippy', `Reports the clippy findings of ${crate.name} with cargo clippy`, { ...selected, commands: checked((selection) => ({ args: ['clippy', ...selection] })) }, { cache: true, inputs: [...SOURCES, ...linted, { env: 'CLIPPY_CONF_DIR' }] }),
        },
        fix: {
            clippy: cargoTarget('clippy --fix', `Applies the clippy suggestions for ${crate.name} with cargo clippy --fix`, { ...selected, commands: [{ args: ['clippy', '--fix', '--allow-dirty', ...filters, '--keep-going', ...MESSAGES] }] }, {}),
        },
        install: {
            ...(crate.targets.some(binary) && {
                cargo: cargoTarget(
                    'install',
                    `Installs the binaries of ${crate.name} into Cargo's install root with cargo install, in release unless a configuration names a profile`,
                    { cwd: workspace.root, commands: installed([]) },
                    { configurations: Record.fromIterableWith(workspace.profiles, (profile) => [profile, { commands: installed(['--profile', profile]) }] as const) },
                ),
            }),
        },
    });
};

const workspaceTargets = (workspace: Workspace): Verbs => {
    const manifests = Array.dedupe([...workspace.members.map((member) => member.manifest), join(workspace.root, MANIFEST)]).map(tokened);
    const check = Record.fromIterableWith(
        Option.toArray(workspace.deny)
            .flatMap(({ files, tables }) => DENIED.map((row) => ({ ...row, files, selected: Array.intersection(row.checks, tables) })))
            .filter(({ selected }) => selected.length > 0),
        ({ tool, extra, files, selected }) =>
            [
                tool,
                cargoTarget(
                    'deny',
                    `Reports the cargo-deny ${selected.join(', ')} findings of the workspace graph with cargo deny check, failing on any warning`,
                    { cwd: workspace.root, commands: [{ args: ['deny', LOCKED, '--workspace', 'check', '--deny', 'warnings', ...selected] }] },
                    { ...extra, inputs: [...manifests, tokened(join(workspace.root, LOCK)), ...files.map(tokened), version('deny'), { externalDependencies: [] }] },
                ),
            ] as const,
    );
    return {
        check,
        upgrade: {
            cargo: {
                executor: 'nx:run-commands',
                inputs: [...manifests, tokened(join(workspace.root, LOCK)), { externalDependencies: [] }],
                options: { commands: ['cargo upgrade --incompatible allow', 'cargo update'], parallel: false, cwd: workspace.root },
                metadata: { description: 'Moves every dependency requirement to its newest release with cargo upgrade, then the lock with cargo update', help: { command: 'cargo upgrade --help', example: { args: ['--dry-run'] } } },
            },
        },
    };
};

const projectTargets = (workspace: Workspace, reports: Reports, member: Option.Option<Member>, stated: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>): Record<string, TargetConfiguration> => {
    const nested = Option.filter(member, ({ root }) => root !== workspace.root);
    const packed = Option.map(Option.all({ held: Option.filter(member, ({ versioned }) => versioned), placed: workspace.placed }), ({ held: { crate, packages }, placed: artifacts }) =>
        cargoTarget(
            'package',
            `Packs ${crate.name} into a .crate archive with cargo package beside the workspace crates it depends on, without the verification build`,
            { cwd: workspace.root, package: crate.name, commands: [{ args: ['package', '--no-verify', LOCKED], packages }] },
            { cache: true, outputs: [tokened(join(artifacts, 'package', `${crate.name}-${crate.version}.crate`))] },
        ),
    );
    const tools = [...Option.toArray(Option.map(member, (held) => crateTargets(workspace, reports, held))), ...(Option.isNone(nested) ? [workspaceTargets(workspace)] : []), { pack: Option.match(packed, { onNone: (): Readonly<Record<string, TargetConfiguration>> => ({}), onSome: (cargo) => ({ cargo }) }) }].reduce(
        joined,
    );
    const removed = Record.collect(tools, (verb, held) => Record.keys(Record.filter(held, ({ outputs = [] }) => outputs.length > 0)).map((tool) => `${verb}--${tool}`)).flat();
    const cleaned = Option.match(nested, {
        onNone: () => ({ commands: [['cargo', 'clean']] as const, description: 'Removes the build folder of the workspace with cargo clean' }),
        onSome: ({ crate }) => {
            const profiles = ['dev', 'release', ...workspace.profiles];
            return { commands: profiles.map((profile) => ['cargo', 'clean', '--package', crate.name, '--profile', profile, LOCKED] as const), description: `Removes the build artifacts of ${crate.name} in profiles ${profiles.join(', ')} with cargo clean --profile` };
        },
    });
    return aggregated(
        {
            ...tools,
            attach: Option.match(packed, {
                onNone: () => ({}),
                onSome: () => ({
                    gh: {
                        executor: `${distribution.name}:attach`,
                        dependsOn: ['pack--cargo'],
                        inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                        metadata: { description: "Uploads the file pack--cargo wrote to the GitHub release of the version the project's manifest states with gh release upload", help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
                    },
                }),
            }),
            clean: {
                cargo: {
                    executor: `${distribution.name}:clean`,
                    inputs: SOURCES,
                    options: { commands: cleaned.commands, cwd: workspace.root, targets: removed },
                    metadata: { description: cleaned.description, help: { command: 'cargo clean --help', example: { args: ['--dry-run'] } } },
                },
            },
        },
        stated,
        'rust',
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
                      executor: `${PLUGIN}:release-publish`,
                      options: { cwd, package: crate.name, packages, ...named },
                      dependsOn: ['pack--cargo', '^nx-release-publish'],
                      inputs: SOURCES,
                      metadata: { technologies: ['rust'], description: `Publishes ${crate.name} with cargo publish at the version nx release tagged at the checked-out commit, skipping a version cargo info finds in its registry`, help: { command: 'cargo publish --help', example: { options: { dryRun: true } } } },
                  },
              };
    return versioned ? { tags: ['release'], release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { lock: join(cwd, LOCK), ...named } } }, targets: published } : { targets: {} };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { crateInputs, Options, projectTargets, released };
