import { basename, dirname, join, relative } from 'node:path';
import type { TargetConfiguration } from '@nx/devkit';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Option, Record } from 'effect';
import type { Cmake } from '#/executors/cmake/executor';
import type { Package } from '#/executors/package/executor';
import { configured, PACKAGING, PRESETS, planted, type Tree } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Member = Tree['members'][number];
type Workflow = Tree['workflows'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = '.clang-tidy';
const PLUGIN = '@rasm/nx-cmake';
const SHARED = distribution.name;

// --- [OPERATIONS] ----------------------------------------------------------------------

const tokened = (file: string) => join('{workspaceRoot}', file);

const memberInputs = (tree: Tree, member: Member, stated: TargetConfiguration['inputs']): NonNullable<TargetConfiguration['inputs']> => [
    ...(stated ?? (member.folders.includes(member.root) ? ['{projectRoot}/**/*'] : member.folders.map((folder) => tokened(join(folder, '**/*'))))),
    ...tree.inputs.filter((file) => !tree.members.some(({ root }) => root !== tree.root && root !== member.root && !relative(root, file).startsWith('..'))).map((file) => tokened(file)),
    ...(member.root === tree.root ? [] : [tokened(join(tree.root, PRESETS))]),
    { runtime: 'node -p process.platform+process.arch' },
];

const memberTargets = (tree: Tree, member: Member, outputPath: Option.Option<string>, files: ReadonlySet<string>, head: Workflow): Record<string, TargetConfiguration> => {
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
    const tidied = [...files].filter((file) => basename(file) === CONFIG && !relative(dirname(file), member.root).startsWith('..')).map((file) => tokened(file));
    const waits = Option.match(targets, { onNone: () => ({}), onSome: () => ({ dependsOn: ['build--cmake'] }) });
    const planned = (workflow: Workflow, defines: Readonly<Record<string, string>> = {}) => ({
        at: workflow.tree,
        folder: join(workflow.tree, member.build),
        shared: { cwd: tree.root, configure: configured('.', workflow, within(workflow.tree), defines) },
    });
    const compiled = (workflow: Workflow, at: string, names: Array.NonEmptyReadonlyArray<string>) => Array.make('cmake', '--build', within(at), ...preset(workflow.build), '--target', ...names);
    const varied = (each: (workflow: Workflow) => object) => ({ configurations: Record.fromIterableWith(others, (workflow) => [workflow.name, each(workflow)] as const) });
    const target = (help: string, description: string, options: typeof Cmake.Type, extra: Partial<TargetConfiguration>): TargetConfiguration => ({
        executor: `${PLUGIN}:cmake`,
        options,
        parallelism: false,
        metadata: { description, help: { command: `${help} --help`, example: { args: ['--verbose'] } } },
        ...extra,
    });
    const builds = (names: Array.NonEmptyReadonlyArray<string>) => (workflow: Workflow) => {
        const { at, shared } = planned(workflow);
        return { ...shared, commands: [compiled(workflow, at, names)], outputs: member.targets.flatMap(({ artifacts }) => artifacts.map((artifact) => tokened(join(at, artifact.path)))) } satisfies typeof Cmake.Type;
    };
    const tests = (workflow: Workflow) => {
        const { at, folder, shared } = planned(workflow);
        const report = join(planted(outputPath, tree.root, 'ctest', member.name), `${workflow.name}.xml`);
        return {
            ...shared,
            commands: [Array.make('ctest', ...preset(workflow.test), '--test-dir', within(folder), '--output-junit', relative(folder, report)), ...Array.fromNullishOr(workflow.after).map((stated) => Array.make('cmake', '--build', within(at), '--preset', stated))],
            outputs: [tokened(report)],
        } satisfies typeof Cmake.Type;
    };
    const installs = (workflow: Workflow) => {
        const { at, folder, shared } = planned(workflow);
        return { ...shared, commands: [...Option.toArray(Option.map(targets, (names) => compiled(workflow, at, names))), Array.make('cmake', '--install', within(folder))] } satisfies typeof Cmake.Type;
    };
    const checks = (workflow: Workflow, names: Array.NonEmptyReadonlyArray<string>) => {
        const { at, shared } = planned(
            workflow,
            Record.fromIterableWith(workflow.tidy, ({ variable }) => [`${variable}_EXPORT_FIXES_DIR`, ''] as const),
        );
        return { ...shared, commands: [compiled(workflow, at, names)] } satisfies typeof Cmake.Type;
    };
    const fixes = (workflow: Workflow, names: Array.NonEmptyReadonlyArray<string>) => {
        const fixer = 'clang-apply-replacements';
        const defines = workflow.tidy.flatMap(({ variable, launcher: stated }) => [[variable, `${stated};--warnings-as-errors=-*`] as const, [`${variable}_EXPORT_FIXES_DIR`, fixer] as const]);
        const { at, folder, shared } = planned(workflow, Object.fromEntries(defines));
        return { ...shared, commands: [compiled(workflow, at, names), Array.make(fixer, '--format', '--style=file', within(join(folder, fixer)))] } satisfies typeof Cmake.Type;
    };
    const cleans = (workflow: Workflow) => {
        const { at, shared } = planned(workflow);
        return { cwd: shared.cwd, commands: [shared.configure, Array.make('cmake', '--build', within(at), '--target', 'clean')] };
    };
    const packs = (workflow: Workflow, stated: string, names: Array.NonEmptyReadonlyArray<string>) => {
        const { at, folder, shared } = planned(workflow);
        return {
            ...shared,
            build: compiled(workflow, at, names),
            pack: Array.make('cpack', '--preset', stated),
            config: join(folder, PACKAGING),
            outputPath: planted(outputPath, tree.root, 'cpack', member.name),
        } satisfies typeof Package.Type;
    };
    const built = Record.getSomes<string, TargetConfiguration>({
        'build--cmake': Option.map(targets, (names) =>
            target('cmake --build', `Builds the targets of ${member.name} in the tree its workflow preset configures`, builds(names)(head), { cache: true, inputs: sources, outputs: builds(names)(head).outputs.map((_, at) => `{options.outputs.${at}}`), ...varied(builds(names)) }),
        ),
        'pack--cpack': Option.map(Option.all({ packed: packing, names: targets }), ({ packed: { workflow, stated }, names }) => ({
            executor: `${PLUGIN}:package`,
            options: packs(workflow, stated, names),
            parallelism: false,
            cache: true,
            inputs: sources,
            outputs: [tokened(packs(workflow, stated, names).outputPath)],
            metadata: { description: `Builds ${member.name} in workflow ${workflow.name} and packs its install tree with CPack, which names the archive`, help: { command: 'cpack --help', example: { args: ['--verbose'] } } },
        })),

        'check--clang-tidy': Option.map(checked, ({ names, workflow }) => target('clang-tidy', `Reports clang-tidy findings and failed header-set verification for ${member.name} in workflow ${workflow.name}, each as an error`, checks(workflow, names), { cache: true, inputs: [...sources, ...tidied] })),
        'fix--clang-tidy': Option.map(checked, ({ names, workflow }) => target('clang-apply-replacements', `Applies the replacements clang-tidy exports for ${member.name} in workflow ${workflow.name}, then formats the changed lines`, fixes(workflow, names), {})),
    });
    const tools: Record<string, TargetConfiguration> = {
        ...built,
        ...(member.tested && { 'test--ctest': target('ctest', `Runs the tests CTest lists under ${member.name}`, tests(head), { cache: true, ...waits, inputs: sources, outputs: ['{options.outputs.0}'], ...varied(tests) }) }),
        ...(member.installs && { 'install--cmake': target('cmake --install', `Builds ${member.name}, then installs it under the install prefix its configure preset states`, installs(head), varied(installs)) }),
    };
    const written = Record.keys(Record.filter(tools, ({ outputs = [] }) => outputs.length > 0));
    const cleaning: TargetConfiguration = Option.match(targets, {
        onNone: () => ({ metadata: { description: `Removes the outputs ${written.join(', ')} declare` } }),
        onSome: () => ({ options: cleans(head), parallelism: false, ...varied(cleans), metadata: { description: `Removes the build products of ${member.name} with the clean target of its configured tree`, help: { command: 'cmake --build', example: { args: ['--verbose'] } } } }),
    });
    const delivered: Record<string, TargetConfiguration> = {
        ...tools,
        ...((Option.isSome(targets) || written.length > 0) && { 'clean--cmake': { executor: `${SHARED}:clean`, ...cleaning, options: { ...cleaning.options, targets: written } } }),
        ...('pack--cpack' in tools && {
            'attach--gh': {
                executor: `${SHARED}:attach`,
                dependsOn: ['pack--cpack'],
                metadata: { description: "Uploads the files pack--cpack wrote to the GitHub release of the version the project's manifest states through gh release upload", help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
            },
        }),
    };
    return {
        ...Record.map(delivered, (tool): TargetConfiguration => ({ ...tool, metadata: { technologies: ['cmake'], ...tool.metadata } })),
        ...Record.map(
            Array.groupBy(Object.entries(delivered), ([label]) => label.slice(0, label.indexOf('--'))),
            (members, verb): TargetConfiguration => ({ dependsOn: [`${verb}--*`], ...Array.match(Array.dedupe(members.flatMap(([, { inputs: hashed = [] }]) => hashed)), { onEmpty: () => ({}), onNonEmpty: (unique) => ({ inputs: ['...', ...unique] }) }) }),
        ),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { CONFIG, memberInputs, memberTargets, PLUGIN, SHARED };
