import { posix } from 'node:path';
import { type CreateDependencies, type CreateNodes, type CreateNodesContext, createNodesFromFiles, DependencyType, type RawProjectGraphDependency } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, findProjectForPath, getNamedInputs, globWithWorkspaceContext } from '@nx/devkit/internal';
import { type Anchor, anchored, inferring, manifested } from '@rasm/nx';
import { Array, Effect, Record, Schema } from 'effect';
import plugin from '#/package.json' with { type: 'json' };
import { memberInputs, memberTargets } from '#/targets';
import { Linked, MANIFEST, PRESETS, REGISTRY, resolved, run, type Tree } from '#/workspace';

// --- [MODELS] --------------------------------------------------------------------------

class Options extends Schema.Opaque<Options, unknown>()(
    Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), coveragePath: Schema.optionalKey(Schema.String) }).annotate({
        description:
            "outputPath: workspace folder each member writes its CPack archives under as <outputPath>/cpack, {projectName} naming the member, absent writes archives under packages in the member build folder. coveragePath: workspace folder the coverage configuration of a member's test target writes the lcov.info report of the member's test executables to, its raw and merged profiles staying in the member build folder, {projectName} naming the member, absent leaves each test its default.profraw and writes no report.",
    }),
) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = Effect.fnUntraced(function* (context: CreateNodesContext, anchor: Anchor, settings: Options, matched: ReadonlySet<string>, linked: Tree['members'], file: string) {
    const tree = yield* resolved(context.workspaceRoot, file, matched);
    linked.push(...tree.members);
    const projects = yield* Effect.forEach(
        Array.cartesian(
            Array.take(tree.workflows, 1),
            tree.members.filter((member) => member.compiled || member.registries.length > 0),
        ),
        ([head, member]) =>
            Effect.map(manifested(context.workspaceRoot, member.root), (declared) => {
                const targets = memberTargets(tree, member, settings, head, declared, anchor);
                return {
                    root: member.root,
                    name: member.name,
                    metadata: { technologies: ['cmake'] },
                    ...(member.compiled && { namedInputs: { default: memberInputs(tree, member, head, getNamedInputs(member.root, context)['default']) } }),
                    ...('pack--cpack' in targets && { tags: ['release'], release: { version: { versionActions: `${plugin.name}/release/version-actions` } } }),
                    targets,
                };
            }),
    );
    return { projects: Record.fromIterableBy(projects, ({ root }) => root) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    `**/${PRESETS}`,
    inferring(Options, run, function* (matched, settings, context) {
        const listed = new Set(yield* Effect.promise(() => globWithWorkspaceContext(context.workspaceRoot, [combineGlobPatterns([PRESETS, MANIFEST, '*.cmake', REGISTRY.manifest, REGISTRY.configuration].map((file) => `**/${file}`))])));
        const presets = matched.filter((file) => listed.has(posix.join(posix.dirname(file), MANIFEST)));
        const linked: Tree['members'] = [];
        const anchor = anchored(context.workspaceRoot);
        (yield* Linked).set(context.workspaceRoot, linked);
        return yield* Effect.promise(() => createNodesFromFiles((file) => run(inferred(context, anchor, settings, listed, linked, file)), presets, settings, context));
    }),
];

const createDependencies: CreateDependencies = (_, { projects, workspaceRoot }) =>
    run(
        Linked.useSync((links) => {
            const names = createProjectRootMappingsFromProjectConfigurations(projects);
            return (links.get(workspaceRoot) ?? [])
                .flatMap(({ root, links: held }) => Array.cartesian(Array.fromNullishOr(names.get(root)), held))
                .flatMap(([source, link]) => Array.fromNullishOr(names.get(link.root)).map((target): RawProjectGraphDependency => (findProjectForPath(link.file, names) === source ? { source, target, type: DependencyType.static, sourceFile: link.file } : { source, target, type: DependencyType.implicit })));
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
