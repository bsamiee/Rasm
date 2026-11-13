import type { TargetConfiguration } from '@nx/devkit';
import { ancestors, type Identity, line, type Members, type Project, type Request } from '@rasm/nx-devkit/plugin';
import { Array, Option, type Path } from 'effect';
import type { Cmake } from '#/executors/cmake/executor';
import type { Package } from '#/executors/package/executor';
import { configured, invoked, MANIFEST, PACKAGING, PRESETS, planted, type Tree } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Member = Tree['members'][number];
type Workflow = Tree['workflows'][number];

interface Environment {
    readonly launcher: readonly string[];
    readonly locks: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = ['default', '^default'] as const;
const CHECKER = 'clang-tidy';
const FIXER = 'clang-apply-replacements';
const CONFIG = '.clang-tidy';

// --- [OPERATIONS] ----------------------------------------------------------------------

const tokened = (path: Path.Path, file: string) => path.join('{workspaceRoot}', file);

const memberInputs = (path: Path.Path, tree: Tree, member: Member, { launcher, locks }: Environment, stated: NonNullable<TargetConfiguration['inputs']>): NonNullable<TargetConfiguration['inputs']> => [
    ...stated,
    ...tree.inputs.filter((file) => !tree.members.some(({ root }) => root !== tree.root && !path.relative(root, file).startsWith('..'))).map((file) => tokened(path, file)),
    ...locks.map((file) => tokened(path, file)),
    ...(member.root === tree.root ? [] : [tokened(path, path.join(tree.root, PRESETS))]),
    ...tree.toolchains.map(({ compiler }) => ({ runtime: line(invoked(launcher, [compiler, '--version'])) })),
];

const memberTargets = (plugin: Identity, path: Path.Path, tree: Tree, member: Member, { launcher }: Environment, placed: Request<unknown>['placed'], files: ReadonlySet<string>) => {
    const [head, ...others] = tree.workflows;
    const within = (at: string) => path.relative(tree.root, at) || '.';
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
    const tidied = [...files].filter((file) => path.basename(file) === CONFIG && path.dirname(file) !== member.root && ancestors(path, member.root).includes(path.dirname(file))).map((file) => tokened(path, file));
    const waits = Option.match(targets, { onNone: () => ({}), onSome: () => ({ dependsOn: ['build--cmake'] }) });
    const planned = (workflow: Workflow, defines: Readonly<Record<string, string>> = {}) => ({
        at: workflow.tree,
        folder: path.join(workflow.tree, member.build),
        shared: { cwd: tree.root, configure: configured(launcher, '.', workflow, within(workflow.tree), defines) },
    });
    const compiled = (workflow: Workflow, at: string, names: Array.NonEmptyReadonlyArray<string>) => invoked(launcher, ['cmake', '--build', within(at), ...preset(workflow.build), '--target', ...names]);
    const varied = (each: (workflow: Workflow) => typeof Cmake.Encoded) => ({ configurations: Object.fromEntries(others.map((workflow) => [workflow.name, each(workflow)] as const)) });
    const target = (help: string, description: string, options: typeof Cmake.Encoded, extra: Partial<TargetConfiguration>): TargetConfiguration => ({
        executor: `${plugin.name}:cmake`,
        options,
        parallelism: false,
        metadata: { description, help: { command: `${help} --help`, example: { args: ['--verbose'] } } },
        ...extra,
    });
    const builds = (names: Array.NonEmptyReadonlyArray<string>) => (workflow: Workflow) => {
        const { at, shared } = planned(workflow);
        return { ...shared, commands: [compiled(workflow, at, names)], outputs: member.targets.flatMap(({ artifacts }) => artifacts.map((artifact) => tokened(path, path.join(at, artifact.path)))) } satisfies typeof Cmake.Encoded;
    };
    const tests = (workflow: Workflow) => {
        const { at, folder, shared } = planned(workflow);
        const report = path.join(planted(path, placed, tree.root, 'ctest', member.name), `${workflow.name}.xml`);
        return {
            ...shared,
            commands: [invoked(launcher, ['ctest', ...preset(workflow.test), '--test-dir', within(folder), '--output-junit', path.relative(folder, report)]), ...Array.fromNullishOr(workflow.after).map((stated) => invoked(launcher, ['cmake', '--build', within(at), '--preset', stated]))],
            outputs: [tokened(path, report)],
        } satisfies typeof Cmake.Encoded;
    };
    const installs = (workflow: Workflow) => {
        const { at, folder, shared } = planned(workflow);
        return { ...shared, commands: [...Option.toArray(Option.map(targets, (names) => compiled(workflow, at, names))), invoked(launcher, ['cmake', '--install', within(folder)])] } satisfies typeof Cmake.Encoded;
    };
    const checks = (workflow: Workflow, names: Array.NonEmptyReadonlyArray<string>) => {
        const { at, shared } = planned(workflow, Object.fromEntries(workflow.tidy.map(({ variable }) => [`${variable}_EXPORT_FIXES_DIR`, ''] as const)));
        return { ...shared, commands: [compiled(workflow, at, names)] } satisfies typeof Cmake.Encoded;
    };
    const fixes = (workflow: Workflow, names: Array.NonEmptyReadonlyArray<string>) => {
        const defines = workflow.tidy.flatMap(({ variable, launcher: stated }) => [[variable, `${stated};--warnings-as-errors=-*`] as const, [`${variable}_EXPORT_FIXES_DIR`, FIXER] as const]);
        const { at, folder, shared } = planned(workflow, Object.fromEntries(defines));
        return { ...shared, commands: [compiled(workflow, at, names), invoked(launcher, [FIXER, '--format', '--style=file', within(path.join(folder, FIXER))])] } satisfies typeof Cmake.Encoded;
    };
    const packs = (workflow: Workflow, stated: string, names: Array.NonEmptyReadonlyArray<string>) => {
        const { at, folder, shared } = planned(workflow);
        return {
            ...shared,
            build: compiled(workflow, at, names),
            pack: invoked(launcher, ['cpack', '--preset', stated]),
            config: path.join(folder, PACKAGING),
            outputPath: planted(path, placed, tree.root, 'cpack', member.name),
        } satisfies typeof Package.Encoded;
    };
    return head === undefined
        ? {}
        : ({
              build: Option.match(targets, {
                  onNone: () => ({}),
                  onSome: (names) => ({
                      cmake: target('cmake --build', `Builds the targets of ${member.name} in the tree its workflow preset configures`, builds(names)(head), {
                          inputs: [...SOURCES],
                          outputs: builds(names)(head).outputs.map((_, at) => `{options.outputs.${at}}`),
                          ...varied(builds(names)),
                      }),
                  }),
              }),
              pack: Option.match(Option.all({ packed: packing, names: targets }), {
                  onNone: () => ({}),
                  onSome: ({ packed: { workflow, stated }, names }) => ({
                      cpack: {
                          executor: `${plugin.name}:package`,
                          options: packs(workflow, stated, names),
                          parallelism: false,
                          cache: true,
                          inputs: [...SOURCES],
                          outputs: [tokened(path, packs(workflow, stated, names).outputPath)],
                          metadata: { description: `Builds ${member.name} in workflow ${workflow.name} and packs its install tree with CPack, which names the archive`, help: { command: 'cpack --help', example: { args: ['--verbose'] } } },
                      },
                  }),
              }),
              test: member.tested ? { ctest: target('ctest', `Runs the tests CTest lists under ${member.name}`, tests(head), { cache: true, ...waits, inputs: [...SOURCES], outputs: ['{options.outputs.0}'], ...varied(tests) }) } : {},
              check: Option.match(checked, {
                  onNone: () => ({}),
                  onSome: ({ names, workflow }) => ({
                      [CHECKER]: target(CHECKER, `Reports clang-tidy findings and failed header-set verification for ${member.name} in workflow ${workflow.name}, each as an error`, checks(workflow, names), { cache: true, inputs: [...SOURCES, ...tidied] }),
                  }),
              }),
              fix: Option.match(checked, {
                  onNone: () => ({}),
                  onSome: ({ names, workflow }) => ({ [CHECKER]: target(FIXER, `Applies the replacements clang-tidy exports for ${member.name} in workflow ${workflow.name}, then formats the changed lines`, fixes(workflow, names), {}) }),
              }),
              install: member.installs ? { cmake: target('cmake --install', `Builds ${member.name}, then installs it under the install prefix its configure preset states`, installs(head), varied(installs)) } : {},
          } satisfies Members);
};

const released = (tree: Tree, member: Member): Pick<Project, 'release' | 'packaged'> =>
    member.packaged && member.targets.length > 0 && tree.packages.some(({ configurePreset }) => tree.workflows.some(({ configure }) => configure === configurePreset)) ? { release: { manifest: MANIFEST }, packaged: ['pack--cpack'] } : {};

// --- [EXPORTS] -------------------------------------------------------------------------

export { CONFIG, type Environment, memberInputs, memberTargets, released };
