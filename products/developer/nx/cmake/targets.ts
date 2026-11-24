import { posix } from 'node:path';
import type { TargetConfiguration } from '@nx/devkit';
import { quoteShellArg } from '@nx/devkit/internal';
import { aggregated, PLATFORM, type Verbs } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Option, Record, UndefinedOr } from 'effect';
import { configured, ENVIRONMENT, PACKAGING, REGISTRY, type Tree, WORKING, type Workflow } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHELL = /[\\"`$]/gu;
const COVERAGE = { configuration: 'coverage', folder: 'profiles', profile: '%m-%p.profraw', merged: 'coverage.profdata', report: 'lcov.info' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const tokened = (file: string) => posix.join('{workspaceRoot}', file);

const shelled = (word: string) =>
    word.includes('$env{')
        ? `"${word
              .split(ENVIRONMENT)
              .map((part, at) => (at % 2 === 0 ? part.replaceAll(SHELL, String.raw`\$&`) : `\${${part}}`))
              .join('')}"`
        : quoteShellArg(word);

const line = (argv: readonly string[]) => argv.map(shelled).join(' ');

const memberInputs = (tree: Tree, member: Tree['members'][number], stated: TargetConfiguration['inputs']): NonNullable<TargetConfiguration['inputs']> => [
    ...(stated ?? ['{projectRoot}/**/*']),
    ...tree.inputs.filter((file) => !tree.members.some(({ root }) => root !== tree.root && root !== member.root && !posix.relative(root, file).startsWith('..'))).map((file) => tokened(file)),
    { runtime: 'cmake --version' },
    ...(tree.inputs.some((file) => posix.basename(file) === REGISTRY.manifest) ? [{ runtime: 'vcpkg version' }] : []),
    PLATFORM,
];

const preset = (name: string | undefined) => Array.fromNullishOr(name).flatMap((stated) => ['--preset', stated]);

const flagged = (workflow: Workflow, flag: string) =>
    workflow.tidy.map(({ launcher }) =>
        Option.map(
            Array.findFirst(launcher.split(';'), (word) => word.startsWith(flag)),
            (word) => word.slice(flag.length),
        ),
    );

const memberTargets = (tree: Tree, member: Tree['members'][number], { outputPath, coveragePath }: { readonly outputPath?: string; readonly coveragePath?: string }, head: Workflow, declared: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>): Record<string, TargetConfiguration> => {
    const others = tree.workflows.filter((workflow) => workflow !== head);
    const within = (at: string) => posix.relative(tree.root, at) || '.';
    const checked = Option.filter(
        Array.findFirst(tree.workflows, ({ tidy }) => tidy.length > 0),
        () => member.compiled,
    );
    const packing = Array.findFirst(tree.packages, ({ name: stated, configurePreset }) =>
        Option.map(
            Option.filter(
                Array.findFirst(tree.workflows, ({ configure }) => configure === configurePreset),
                () => member.compiled && member.packaged,
            ),
            (workflow) => ({ workflow, stated }),
        ),
    );
    const sources = ['default', '^default', { externalDependencies: [] }];
    const looked = [
        ...Array.unfold(
            Option.some(posix.dirname(member.root)),
            Option.map((at: string) => [at, Option.liftPredicate(posix.dirname(at), () => at !== '.')] as const),
        ),
        posix.join(member.root, '**'),
    ].map((at) => posix.join(at, '.clang-tidy'));
    const tidied = (workflow: Workflow) => [
        ...Array.dedupe(
            flagged(workflow, '--config-file=')
                .flatMap(Option.match({ onNone: () => looked, onSome: (at) => [posix.normalize(at.replaceAll(WORKING, tree.root))] }))
                .map((file) => tokened(file)),
        ),
        ...Array.dedupe(workflow.tidy.flatMap(({ launcher }) => Array.take(launcher.split(';'), 1))).map((program) => ({ runtime: `${line([program])} --version` })),
    ];
    const planned = (workflow: Workflow, defines: Readonly<Record<string, string>> = {}) => {
        const shared = { cwd: tree.root, configure: configured(workflow.configure, { ...Record.fromIterableWith(workflow.tidy, ({ variable }) => [`${variable}_EXPORT_FIXES_DIR`, ''] as const), ...defines }) };
        const ran = (commands: readonly (readonly string[])[], outputs: readonly string[] = [], forwarded = commands.length - 1) => ({
            cwd: shared.cwd,
            parallel: false,
            commands: [shared.configure, ...commands].map((stated, at) => ({ command: line(stated), forwardAllArgs: at === forwarded + 1 })),
            ...(outputs.length > 0 && { outputs }),
        });
        return { at: workflow.tree, folder: posix.join(workflow.tree, member.build), shared, ran };
    };
    const placed = (workflow: Workflow, tool: string, inside: string) => UndefinedOr.match(outputPath, { onUndefined: () => posix.join(workflow.tree, member.build, inside), onDefined: (at) => posix.join(at, tool) });
    const compiled = (workflow: Workflow, at: string, named = posix.join(member.build, 'all')) => ['cmake', '--build', within(at), ...preset(workflow.build), '--target', named];
    const varied = (each: (workflow: Workflow) => object) => (others.length > 0 ? { configurations: Record.fromIterableWith(others, (workflow) => [workflow.name, each(workflow)] as const) } : {});
    const target = (help: string, description: string, options: Readonly<Record<string, unknown>>, extra: Partial<TargetConfiguration>): TargetConfiguration => ({
        executor: 'nx:run-commands',
        options,
        parallelism: false,
        inputs: sources,
        metadata: { description, help: { command: `${help} --help`, example: { args: ['--verbose'] } } },
        ...extra,
    });
    const builds = (workflow: Workflow) => {
        const { at, folder, ran } = planned(workflow);
        return ran([compiled(workflow, at)], [tokened(folder)]);
    };
    const covered = Option.all({ profiled: Option.fromUndefinedOr(coveragePath), workflow: Array.findFirst(others, ({ name }) => name === COVERAGE.configuration) });
    const tests = (workflow: Workflow) => {
        const { at, folder, ran } = planned(workflow);
        const ctest = ['ctest', ...preset(workflow.test), '--test-dir', within(folder)];
        const after = Array.fromNullishOr(workflow.after).map((stated) => ['cmake', '--build', within(at), '--preset', stated]);
        const [profiles, merged] = [within(posix.join(folder, COVERAGE.folder)), within(posix.join(folder, COVERAGE.merged))];
        const coverage = Option.toArray(
            Option.map(
                Option.filter(covered, (held) => held.workflow === workflow),
                (held) => ({ ...held, lcov: posix.join(held.profiled, COVERAGE.report) }),
            ),
        );
        const cleared = coverage.flatMap(({ profiled, lcov }) => [
            ['cmake', '-E', 'rm', '-rf', profiles, within(lcov)],
            ['cmake', '-E', 'make_directory', within(profiled)],
        ]);
        const profiling = coverage.flatMap(() => ['cmake', '-E', 'env', `LLVM_PROFILE_FILE=${WORKING}/${posix.join(profiles, COVERAGE.profile)}`]);
        const exported = coverage.map(({ lcov }) => ['cmake', `-DPROFILES=${profiles}`, `-DMERGED=${merged}`, `-DTESTS=${within(folder)}`, `-DREPORT=${within(lcov)}`, '-P', tree.exporter]);
        return ran(
            [...cleared, [...profiling, ...ctest], ...after, ...exported],
            coverage.map(({ lcov }) => tokened(lcov)),
            cleared.length,
        );
    };
    const installs = (workflow: Workflow) => {
        const { at, folder, ran } = planned(workflow);
        return ran([compiled(workflow, at), ['cmake', '--install', within(folder)]]);
    };
    const checks = (workflow: Workflow) => {
        const { at, ran } = planned(workflow);
        return ran([compiled(workflow, at)]);
    };
    const fixes = (workflow: Workflow) => {
        const fixer = 'clang-apply-replacements';
        const defines = workflow.tidy.flatMap(({ variable, launcher: stated }) => [[variable, `${stated};--warnings-as-errors=-*`] as const, [`${variable}_EXPORT_FIXES_DIR`, fixer] as const]);
        const style = Option.getOrElse(Array.head(Array.getSomes(flagged(workflow, '--format-style='))), () => 'file');
        const { at, folder, ran } = planned(workflow, Object.fromEntries(defines));
        return ran([compiled(workflow, at), [fixer, '--format', `--style=${style}`, within(posix.join(folder, fixer))]]);
    };
    const cleans = (workflow: Workflow) => {
        const { at, shared } = planned(workflow);
        return { cwd: shared.cwd, commands: [shared.configure, compiled(workflow, at, 'clean')] };
    };
    const packs = (workflow: Workflow, stated: string) => {
        const { at, folder, ran } = planned(workflow);
        const [archives, run] = [placed(workflow, 'cpack', 'packages'), (file: string) => posix.relative(at, file) || '.'];
        const packed = ['cpack', '--preset', stated, '--config', run(posix.join(folder, PACKAGING)), '-B', run(folder), '-D', `CPACK_OUTPUT_FILE_PREFIX=${run(archives)}`];
        return { archives, options: ran([compiled(workflow, at), ['cmake', '-E', 'rm', '-rf', within(archives)], packed]) };
    };
    const cpack = Option.map(packing, ({ workflow, stated }) => {
        const { archives, options } = packs(workflow, stated);
        return target('cpack', `Builds ${member.name} in workflow ${workflow.name}, empties its package folder, and packs its install tree with cpack, which names the archive`, options, {
            cache: true,
            outputs: [posix.relative(member.root, archives).startsWith('..') ? tokened(archives) : posix.join('{projectRoot}', posix.relative(member.root, archives))],
        });
    });
    const tools: Verbs = {
        build: { ...(member.compiled && { cmake: target('cmake --build', `Builds the ${posix.join(member.build, 'all')} target of the tree its workflow preset configures with cmake --build, every target ${member.name} adds`, builds(head), { cache: true, outputs: ['{options.outputs.0}'], ...varied(builds) }) }) },
        pack: Record.getSomes({ cpack }),
        check: Record.getSomes({
            'clang-tidy': Option.map(checked, (workflow) => target('clang-tidy', `Reports the findings and failed header-set verification of ${member.name} in workflow ${workflow.name} as errors with clang-tidy`, checks(workflow), { cache: true, inputs: [...sources, ...tidied(workflow)] })),
        }),
        fix: Record.getSomes({ 'clang-tidy': Option.map(checked, (workflow) => target('clang-apply-replacements', `Applies the replacements clang-tidy exports for ${member.name} in workflow ${workflow.name}, then formats the changed lines with clang-apply-replacements`, fixes(workflow), {})) }),
        test: { ...(member.compiled && { ctest: target('ctest', `Runs the tests of ${member.name} with ctest, the test preset deciding whether none is an error`, tests(head), { cache: true, dependsOn: ['build--cmake'], outputs: Option.toArray(covered).map(() => '{options.outputs.0}'), ...varied(tests) }) }) },
        install: { ...(member.compiled && { cmake: target('cmake --install', `Builds ${member.name}, then installs it under the install prefix its configure preset states with cmake --install`, installs(head), varied(installs)) }) },
        upgrade: Array.match(member.registries, {
            onEmpty: () => ({}),
            onNonEmpty: (held) => ({
                vcpkg: {
                    executor: `${distribution.name}:upgrade`,
                    options: {
                        cwd: member.root,
                        commands: held
                            .map(({ folder }) => ['vcpkg', 'x-update-baseline', `--x-manifest-root=${posix.relative(member.root, folder) || '.'}`, `--x-install-root=${posix.join(posix.relative(member.root, head.tree), 'vcpkg_installed')}`] as const)
                            .map((apply) => ({ apply, preview: [...apply, '--dry-run'] })),
                    },
                    inputs: [...held.flatMap(({ inputs }) => inputs).map((file) => tokened(file)), { externalDependencies: [] }],
                    metadata: { description: `Moves the baselines ${held.map(({ folder }) => posix.join(folder, 'vcpkg.json')).join(', ')} state to the newest commit of each registry with vcpkg x-update-baseline`, help: { command: 'vcpkg help x-update-baseline', example: { options: { dryRun: true } } } },
                },
            }),
        }),
    };
    const removed = Record.collect(tools, (verb, held) => Record.keys(Record.filter(held, ({ outputs = [] }) => outputs.length > 0)).map((tool) => `${verb}--${tool}`)).flat();
    const cleaning: TargetConfiguration<Readonly<Record<string, unknown>>> = member.compiled
        ? {
              options: cleans(head),
              parallelism: false,
              ...varied(cleans),
              metadata: { description: `Removes the outputs ${removed.join(', ')} declare and the build products of ${member.name} with cmake --build --target clean`, help: { command: 'cmake --build --help', example: { args: ['--verbose'] } } },
          }
        : { metadata: { description: `Removes the outputs ${removed.join(', ')} declare`, help: { command: 'nx run --help', example: { options: { targets: removed } } } } };
    return aggregated(
        {
            ...tools,
            clean: { ...((member.compiled || removed.length > 0) && { cmake: { executor: `${distribution.name}:clean`, inputs: sources, ...cleaning, options: { ...cleaning.options, targets: removed } } }) },
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
