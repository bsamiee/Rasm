import { dirname, join, normalize, relative } from 'node:path';
import type { TargetConfiguration } from '@nx/devkit';
import { mergeTargetConfigurations, quoteShellArg } from '@nx/devkit/internal';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Option, Predicate, Record, UndefinedOr } from 'effect';
import { configured, ENVIRONMENT, PACKAGING, PRESETS, type Tree, WORKING } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Member = Tree['members'][number];
type Workflow = Tree['workflows'][number];
type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHELL = /[\\"`$]/gu;
const PLATFORM = { runtime: 'node -p process.platform+process.arch' } as const;
const COVERAGE = { configuration: 'coverage', folder: 'profiles', profile: '%m-%p.profraw', merged: 'coverage.profdata', report: 'lcov.info' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const tokened = (file: string) => join('{workspaceRoot}', file);

const shelled = (word: string) =>
    word.includes('$env{')
        ? `"${word
              .split(ENVIRONMENT)
              .map((part, at) => (at % 2 === 0 ? part.replaceAll(SHELL, String.raw`\$&`) : `\${${part}}`))
              .join('')}"`
        : quoteShellArg(word);

const line = (argv: readonly string[]) => argv.map(shelled).join(' ');

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

const memberInputs = (tree: Tree, member: Member, stated: TargetConfiguration['inputs']): NonNullable<TargetConfiguration['inputs']> => [
    ...(stated ?? (member.folders.includes(member.root) ? ['{projectRoot}/**/*'] : member.folders.map((folder) => tokened(join(folder, '**/*'))))),
    ...tree.inputs.filter((file) => !tree.members.some(({ root }) => root !== tree.root && root !== member.root && !relative(root, file).startsWith('..'))).map((file) => tokened(file)),
    ...(member.root === tree.root ? [] : [tokened(join(tree.root, PRESETS))]),
    PLATFORM,
];

const memberTargets = (
    tree: Tree,
    member: Member,
    { outputPath, coveragePath }: { readonly outputPath?: string; readonly coveragePath?: string },
    registries: readonly { readonly folder: string; readonly inputs: readonly string[] }[],
    head: Workflow,
    declared: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>,
): Record<string, TargetConfiguration> => {
    const others = tree.workflows.filter((workflow) => workflow !== head);
    const within = (at: string) => relative(tree.root, at) || '.';
    const preset = (name: string | undefined) => Array.fromNullishOr(name).flatMap((stated) => ['--preset', stated]);
    const targets = Option.liftPredicate(
        member.targets.map(({ name }) => name),
        Array.isArrayNonEmpty,
    );
    const checked = Option.all({ names: targets, workflow: Array.findFirst(tree.workflows, ({ tidy }) => tidy.length > 0) });
    const packing = Array.findFirst(tree.packages, ({ name: stated, configurePreset }) =>
        Option.map(
            Option.filter(
                Array.findFirst(tree.workflows, ({ configure }) => configure === configurePreset),
                () => member.packaged,
            ),
            (workflow) => ({ workflow, stated }),
        ),
    );
    const sources = ['default', '^default', { externalDependencies: [] }];
    const flagged = (workflow: Workflow, flag: string) =>
        workflow.tidy.map(({ launcher }) =>
            Option.map(
                Array.findFirst(launcher.split(';'), (word) => word.startsWith(flag)),
                (word) => word.slice(flag.length),
            ),
        );
    const looked = [
        ...Array.unfold(
            Option.some(dirname(member.root)),
            Option.map((at: string) => [at, Option.liftPredicate(dirname(at), () => at !== '.')] as const),
        ),
        join(member.root, '**'),
    ].map((at) => join(at, '.clang-tidy'));
    const tidied = (workflow: Workflow) =>
        Array.dedupe(
            flagged(workflow, '--config-file=')
                .flatMap(Option.match({ onNone: () => looked, onSome: (at) => [normalize(at.replaceAll(WORKING, tree.root))] }))
                .map((file) => tokened(file)),
        );
    const waits = Option.match(targets, { onNone: () => ({}), onSome: () => ({ dependsOn: ['build--cmake'] }) });
    const planned = (workflow: Workflow, defines: Readonly<Record<string, string>> = {}) => {
        const shared = { cwd: tree.root, configure: configured(workflow.configure, { ...Record.fromIterableWith(workflow.tidy, ({ variable }) => [`${variable}_EXPORT_FIXES_DIR`, ''] as const), ...defines }) };
        const ran = (commands: readonly (string | readonly string[])[], outputs: readonly string[] = [], forwarded = commands.length - 1) => ({
            cwd: shared.cwd,
            parallel: false,
            commands: [shared.configure, ...commands].map((stated, at) => ({ command: Predicate.isString(stated) ? stated : line(stated), forwardAllArgs: at === forwarded + 1 })),
            ...(outputs.length > 0 && { outputs }),
        });
        return { at: workflow.tree, folder: join(workflow.tree, member.build), shared, ran };
    };
    const placed = (workflow: Workflow, tool: string, inside: string) => UndefinedOr.match(outputPath, { onUndefined: () => join(workflow.tree, member.build, inside), onDefined: (at) => join(at, tool) });
    const compiled = (workflow: Workflow, at: string, names: Array.NonEmptyReadonlyArray<string>) => Array.make('cmake', '--build', within(at), ...preset(workflow.build), '--target', ...names);
    const varied = (each: (workflow: Workflow) => object) => ({ configurations: Record.fromIterableWith(others, (workflow) => [workflow.name, each(workflow)] as const) });
    const target = (help: string, description: string, options: TargetConfiguration['options'], extra: Partial<TargetConfiguration>): TargetConfiguration => ({
        executor: 'nx:run-commands',
        options,
        parallelism: false,
        inputs: sources,
        metadata: { description, help: { command: `${help} --help`, example: { args: ['--verbose'] } } },
        ...extra,
    });
    const products = member.targets.flatMap(({ artifacts }) => artifacts);
    const builds = (names: Array.NonEmptyReadonlyArray<string>) => (workflow: Workflow) => {
        const { at, ran } = planned(workflow);
        return ran(
            [compiled(workflow, at, names)],
            products.map((artifact) => tokened(join(at, artifact.path))),
        );
    };
    const covered = Option.all({
        profiled: Option.fromUndefinedOr(coveragePath),
        objects: Option.liftPredicate(
            member.targets.flatMap(({ binary }) => Array.fromNullishOr(binary)),
            Array.isArrayNonEmpty,
        ),
        workflow: Array.findFirst(others, ({ name }) => name === COVERAGE.configuration),
    });
    const tests = (workflow: Workflow) => {
        const { at, folder, ran } = planned(workflow);
        const report = join(placed(workflow, 'ctest', 'Testing'), `${workflow.name}.xml`);
        const ctest = ['ctest', ...preset(workflow.test), '--test-dir', within(folder), ...(workflow === head ? ['--no-tests=error'] : []), '--output-junit', relative(folder, report)];
        const after = Array.fromNullishOr(workflow.after).map((stated) => ['cmake', '--build', within(at), '--preset', stated]);
        const [profiles, merged] = [within(join(folder, COVERAGE.folder)), within(join(folder, COVERAGE.merged))];
        const coverage = Option.toArray(
            Option.map(
                Option.filter(covered, (held) => held.workflow === workflow),
                (held) => ({ ...held, lcov: join(held.profiled, COVERAGE.report) }),
            ),
        );
        const cleared = coverage.flatMap(({ profiled }) => [
            ['cmake', '-E', 'rm', '-rf', profiles],
            ['cmake', '-E', 'make_directory', within(profiled)],
        ]);
        const profiling = coverage.flatMap(() => ['cmake', '-E', 'env', `LLVM_PROFILE_FILE=${WORKING}/${join(profiles, COVERAGE.profile)}`]);
        const exported = coverage.flatMap(({ objects, lcov }) => [
            ['llvm-profdata', 'merge', '--sparse', `--output=${merged}`, profiles],
            [line(['llvm-cov', 'export', '--format=lcov', `--instr-profile=${merged}`, ...objects.map((object) => `--object=${within(join(at, object))}`)]), '>', shelled(within(lcov))].join(' '),
        ]);
        return ran([...cleared, [...profiling, ...ctest], ...after, ...exported], [report, ...coverage.map(({ lcov }) => lcov)].map(tokened), cleared.length);
    };
    const installs = (workflow: Workflow) => {
        const { at, folder, ran } = planned(workflow);
        return ran([...Option.toArray(Option.map(targets, (names) => compiled(workflow, at, names))), ['cmake', '--install', within(folder)]]);
    };
    const checks = (workflow: Workflow, names: Array.NonEmptyReadonlyArray<string>) => {
        const { at, ran } = planned(workflow);
        return ran([compiled(workflow, at, names)]);
    };
    const fixes = (workflow: Workflow, names: Array.NonEmptyReadonlyArray<string>) => {
        const fixer = 'clang-apply-replacements';
        const defines = workflow.tidy.flatMap(({ variable, launcher: stated }) => [[variable, `${stated};--warnings-as-errors=-*`] as const, [`${variable}_EXPORT_FIXES_DIR`, fixer] as const]);
        const style = Option.getOrElse(Array.head(Array.getSomes(flagged(workflow, '--format-style='))), () => 'file');
        const { at, folder, ran } = planned(workflow, Object.fromEntries(defines));
        return ran([compiled(workflow, at, names), [fixer, '--format', `--style=${style}`, within(join(folder, fixer))]]);
    };
    const cleans = (workflow: Workflow) => {
        const { at, shared } = planned(workflow);
        return { cwd: shared.cwd, commands: [shared.configure, compiled(workflow, at, ['clean'])] };
    };
    const packs = (workflow: Workflow, stated: string, names: Array.NonEmptyReadonlyArray<string>) => {
        const { at, folder, ran } = planned(workflow);
        const [archives, run] = [placed(workflow, 'cpack', 'packages'), (file: string) => relative(at, file) || '.'];
        const packed = ['cpack', '--preset', stated, '--config', run(join(folder, PACKAGING)), '-B', run(folder), '-D', `CPACK_OUTPUT_FILE_PREFIX=${run(archives)}`];
        return { archives, options: ran([compiled(workflow, at, names), ['cmake', '-E', 'rm', '-rf', within(archives)], packed]) };
    };
    const cpack = Option.map(Option.all({ packed: packing, names: targets }), ({ packed: { workflow, stated }, names }) => {
        const { archives, options } = packs(workflow, stated, names);
        return target('cpack', `Builds ${member.name} in workflow ${workflow.name}, empties its package folder, and packs its install tree with cpack, which names the archive`, options, { cache: true, outputs: [tokened(archives)] });
    });
    const tools: Verbs = {
        build: Record.getSomes({
            cmake: Option.map(targets, (names) => target('cmake --build', `Builds the targets of ${member.name} in the tree its workflow preset configures with cmake --build`, builds(names)(head), { cache: true, outputs: products.map((_, at) => `{options.outputs.${at}}`), ...varied(builds(names)) })),
        }),
        pack: Record.getSomes({ cpack }),
        check: Record.getSomes({
            'clang-tidy': Option.map(checked, ({ names, workflow }) => target('clang-tidy', `Reports the findings and failed header-set verification of ${member.name} in workflow ${workflow.name} as errors with clang-tidy`, checks(workflow, names), { cache: true, inputs: [...sources, ...tidied(workflow)] })),
        }),
        fix: Record.getSomes({ 'clang-tidy': Option.map(checked, ({ names, workflow }) => target('clang-apply-replacements', `Applies the replacements clang-tidy exports for ${member.name} in workflow ${workflow.name}, then formats the changed lines with clang-apply-replacements`, fixes(workflow, names), {})) }),
        test: { ...(member.tested && { ctest: target('ctest', `Runs the tests of ${member.name} with ctest`, tests(head), { cache: true, ...waits, outputs: ['{options.outputs.0}', ...Option.toArray(covered).map(() => '{options.outputs.1}')], ...varied(tests) }) }) },
        install: { ...(member.installs && { cmake: target('cmake --install', `Builds ${member.name}, then installs it under the install prefix its configure preset states with cmake --install`, installs(head), varied(installs)) }) },
        upgrade: Array.match(registries, {
            onEmpty: () => ({}),
            onNonEmpty: (held) => ({
                vcpkg: {
                    executor: 'nx:run-commands',
                    options: { cwd: member.root, parallel: false, commands: held.map(({ folder }) => line(['vcpkg', 'x-update-baseline', `--x-manifest-root=${relative(member.root, folder) || '.'}`, `--x-install-root=${join(relative(member.root, head.tree), 'vcpkg_installed')}`])) },
                    inputs: [...held.flatMap(({ inputs }) => inputs).map((file) => tokened(file)), { externalDependencies: [] }],
                    metadata: { description: `Moves the baselines ${held.map(({ folder }) => join(folder, 'vcpkg.json')).join(', ')} state to the newest commit of each registry with vcpkg x-update-baseline`, help: { command: 'vcpkg help x-update-baseline', example: { args: ['--dry-run'] } } },
                },
            }),
        }),
    };
    const removed = Record.collect(tools, (verb, held) => Record.keys(Record.filter(held, ({ outputs = [] }) => outputs.length > 0)).map((tool) => `${verb}--${tool}`)).flat();
    const cleaning: TargetConfiguration = Option.match(targets, {
        onNone: () => ({ metadata: { description: `Removes the outputs ${removed.join(', ')} declare`, help: { command: 'nx run --help', example: { options: { targets: removed } } } } }),
        onSome: () => ({
            options: cleans(head),
            parallelism: false,
            ...varied(cleans),
            metadata: { description: `Removes the outputs ${removed.join(', ')} declare and the build products of ${member.name} with cmake --build --target clean`, help: { command: 'cmake --build --help', example: { args: ['--verbose'] } } },
        }),
    });
    return aggregated(
        {
            ...tools,
            clean: { ...((Option.isSome(targets) || removed.length > 0) && { cmake: { executor: `${distribution.name}:clean`, inputs: sources, ...cleaning, options: { ...cleaning.options, targets: removed } } }) },
            attach: {
                ...(Option.isSome(cpack) && {
                    gh: {
                        executor: `${distribution.name}:attach`,
                        dependsOn: ['pack--cpack'],
                        inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                        metadata: { description: "Uploads the files pack--cpack wrote to the GitHub release of the version the project's manifest states with gh release upload", help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
                    },
                }),
            },
        },
        declared,
        'cmake',
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { memberInputs, memberTargets };
