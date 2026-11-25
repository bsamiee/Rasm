import { posix } from 'node:path';
import type { TargetConfiguration } from '@nx/devkit';
import { quoteShellArg } from '@nx/devkit/internal';
import { above, type Anchor, aggregated, EXECUTORS, inside, PLATFORM, removable, type Stated, tokened, type Verbs } from '@rasm/nx';
import { Array, Option, Record, UndefinedOr } from 'effect';
import { configured, PACKAGING, REGISTRY, type Tree, WORKING, type Workflow } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHELL = /\$env\{(?<name>\w+)\}|[\\"`$]/gu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const shelled = (word: string) => (word.includes('$env{') ? `"${word.replaceAll(SHELL, (held: string, name: string | undefined) => (name === undefined ? `\\${held}` : `\${${name}}`))}"` : quoteShellArg(word));

const line = (argv: readonly string[]) => argv.map(shelled).join(' ');

const memberInputs = (tree: Tree, member: Tree['members'][number], head: Workflow, stated: TargetConfiguration['inputs']) => [
    ...(stated ?? ['{projectRoot}/**/*']),
    ...tree.inputs.filter((file) => !tree.members.some(({ root }) => root !== tree.root && root !== member.root && inside(root)(file))).map(tokened('.')),
    { runtime: 'cmake --version' },
    ...Array.fromNullishOr(head.toolchain).map((file) => ({ runtime: line(['cmake', `-DTOOLCHAIN=${file}`, '-P', tree.identifier]) })),
    ...(tree.inputs.some((file) => posix.basename(file) === REGISTRY.manifest) ? [{ runtime: 'vcpkg version' }] : []),
    PLATFORM,
];

const preset = (name: string | undefined) => Array.fromNullishOr(name).flatMap((stated) => ['--preset', stated]);

const flagged = (workflow: Workflow, flag: string) => workflow.tidy.map(({ launcher }) => Array.findFirst(launcher.split(';'), (word) => (word.startsWith(flag) ? Option.some(word.slice(flag.length)) : Option.none())));

const memberTargets = (tree: Tree, member: Tree['members'][number], { outputPath }: { readonly outputPath?: string }, head: Workflow, declared: Stated, anchor: Anchor) => {
    const others = tree.workflows.filter((workflow) => workflow !== head);
    const within = (at: string) => posix.relative(tree.root, at) || '.';
    const checked = Array.findFirst(tree.workflows, ({ tidy }) => member.compiled && tidy.length > 0);
    const packing = Array.findFirst(tree.packages, ({ name: stated, configurePreset }) => Array.findFirst(tree.workflows, ({ configure }) => member.compiled && member.packaged && configure === configurePreset).pipe(Option.map((workflow) => ({ workflow, stated }))));
    const sources = ['default', '^default', { externalDependencies: [] }];
    const looked = [...above(posix.dirname(member.root)), posix.join(member.root, '**')].map((at) => posix.join(at, '.clang-tidy'));
    const tidied = (workflow: Workflow) => [
        ...Array.dedupe(
            flagged(workflow, '--config-file=')
                .flatMap(Option.match({ onNone: () => looked, onSome: (at) => [posix.normalize(at.replaceAll(WORKING, tree.root))] }))
                .map(tokened('.')),
        ),
        ...Array.dedupe(workflow.tidy.flatMap(({ launcher }) => launcher.split(';', 1))).map((program) => ({ runtime: `${line([program])} --version` })),
    ];
    const planned = (workflow: Workflow, defines: Readonly<Record<string, string>> = {}) => {
        const configure = configured(workflow.configure, { ...Record.fromIterableWith(workflow.tidy, ({ variable }) => [`${variable}_EXPORT_FIXES_DIR`, ''] as const), ...defines });
        const ran = (commands: readonly (readonly string[])[], outputs: readonly string[] = [], forwarded = commands.length - 1) => ({
            cwd: tree.root,
            parallel: false,
            commands: [configure, ...commands].map((stated, at) => ({ command: line(stated), forwardAllArgs: at === forwarded + 1 })),
            ...(outputs.length > 0 && { outputs }),
        });
        return { folder: posix.join(workflow.tree, member.build), configure, ran };
    };
    const compiled = (workflow: Workflow, named = posix.join(member.build, 'all')) => ['cmake', '--build', within(workflow.tree), ...preset(workflow.build), '--target', named];
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
        const { folder, ran } = planned(workflow);
        return ran([compiled(workflow)], [tokened('.')(folder)]);
    };
    const tests = (workflow: Workflow) => {
        const { folder, ran } = planned(workflow);
        const after = Array.fromNullishOr(workflow.after).map((stated) => ['cmake', '--build', within(workflow.tree), '--preset', stated]);
        return ran([['ctest', ...preset(workflow.test), '--test-dir', within(folder)], ...after], [], 0);
    };
    const installs = (workflow: Workflow) => {
        const { folder, ran } = planned(workflow);
        return ran([compiled(workflow), ['cmake', '--install', within(folder)]]);
    };
    const fixes = (workflow: Workflow) => {
        const fixer = 'clang-apply-replacements';
        const defines = workflow.tidy.flatMap(({ variable, launcher: stated }) => [[variable, `${stated};--warnings-as-errors=-*`] as const, [`${variable}_EXPORT_FIXES_DIR`, fixer] as const]);
        const style = Option.getOrElse(Option.firstSomeOf(flagged(workflow, '--format-style=')), () => 'file');
        const { folder, ran } = planned(workflow, Object.fromEntries(defines));
        return ran([compiled(workflow), [fixer, '--format', `--style=${style}`, within(posix.join(folder, fixer))]]);
    };
    const cleans = (workflow: Workflow) => ({ cwd: tree.root, commands: [planned(workflow).configure, compiled(workflow, 'clean')] });
    const cpack = Option.map(packing, ({ workflow, stated }) => {
        const { folder, ran } = planned(workflow);
        const archives = UndefinedOr.match(outputPath, { onUndefined: () => posix.join(folder, 'packages'), onDefined: (at) => posix.join(at, 'cpack') });
        const run = (file: string) => posix.relative(workflow.tree, file) || '.';
        const packed = ['cpack', '--preset', stated, '--config', run(posix.join(folder, PACKAGING)), '-B', run(folder), '-D', `CPACK_OUTPUT_FILE_PREFIX=${run(archives)}`];
        return target('cpack', `Builds ${member.name} in workflow ${workflow.name}, empties its package folder, and packs its install tree with cpack, which names the archive`, ran([compiled(workflow), ['cmake', '-E', 'rm', '-rf', within(archives)], packed]), {
            cache: true,
            outputs: [tokened(member.root)(archives)],
        });
    });
    const tools: Verbs = {
        build: { ...(member.compiled && { cmake: target('cmake --build', `Builds the ${posix.join(member.build, 'all')} target of the tree its workflow preset configures with cmake --build, every target ${member.name} adds`, builds(head), { cache: true, outputs: ['{options.outputs.0}'], ...varied(builds) }) }) },
        pack: Record.getSomes({ cpack }),
        check: Record.getSomes({
            'clang-tidy': Option.map(checked, (workflow) => target('clang-tidy', `Reports the findings and failed header-set verification of ${member.name} in workflow ${workflow.name} as errors with clang-tidy`, planned(workflow).ran([compiled(workflow)]), { cache: true, inputs: [...sources, ...tidied(workflow)] })),
        }),
        fix: Record.getSomes({ 'clang-tidy': Option.map(checked, (workflow) => target('clang-apply-replacements', `Applies the replacements clang-tidy exports for ${member.name} in workflow ${workflow.name}, then formats the changed lines with clang-apply-replacements`, fixes(workflow), {})) }),
        test: { ...(member.compiled && { ctest: target('ctest', `Runs the tests of ${member.name} with ctest, the test preset deciding whether none is an error`, tests(head), { cache: true, dependsOn: ['build--cmake'], ...varied(tests) }) }) },
        install: { ...(member.compiled && { cmake: target('cmake --install', `Builds ${member.name}, then installs it under the install prefix its configure preset states with cmake --install`, installs(head), varied(installs)) }) },
        upgrade: Array.match(member.registries, {
            onEmpty: () => ({}),
            onNonEmpty: (held) => ({
                vcpkg: {
                    executor: EXECUTORS.upgrade,
                    options: {
                        cwd: member.root,
                        commands: held
                            .map(({ folder }) => ['vcpkg', 'x-update-baseline', `--x-manifest-root=${posix.relative(member.root, folder) || '.'}`, `--x-install-root=${posix.join(posix.relative(member.root, head.tree), 'vcpkg_installed')}`] as const)
                            .map((apply) => ({ apply, preview: [...apply, '--dry-run'] })),
                    },
                    inputs: [...held.flatMap(({ inputs }) => inputs).map(tokened('.')), { externalDependencies: [] }],
                    metadata: { description: `Moves the baselines ${held.map(({ folder }) => posix.join(folder, 'vcpkg.json')).join(', ')} state to the newest commit of each registry with vcpkg x-update-baseline`, help: { command: 'vcpkg help x-update-baseline', example: { options: { dryRun: true } } } },
                },
            }),
        }),
    };
    const removed = removable(tools);
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
            clean: { ...((member.compiled || removed.length > 0) && { cmake: { executor: EXECUTORS.clean, inputs: sources, ...cleaning, options: { ...cleaning.options, targets: removed } } }) },
            attach: {
                ...(Option.isSome(cpack) && {
                    gh: {
                        executor: EXECUTORS.attach,
                        dependsOn: ['pack--cpack'],
                        inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                        metadata: {
                            description: "Uploads the files pack--cpack wrote to the GitHub release of the version the project's manifest states with gh release upload, creating that release with gh release create where none exists",
                            help: { command: 'gh release upload --help', example: { options: { dryRun: true } } },
                        },
                    },
                }),
            },
        },
        declared,
        'cmake',
        anchor,
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { memberInputs, memberTargets };
