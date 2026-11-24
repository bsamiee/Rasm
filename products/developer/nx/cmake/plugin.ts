import { posix } from 'node:path';
import { type CreateDependencies, type CreateNodes, type CreateNodesContext, createNodesFromFiles, DependencyType, type ProjectConfiguration, type RawProjectGraphDependency } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, findProjectForPath, getNamedInputs } from '@nx/devkit/internal';
import { inferring, manifested } from '@rasm/nx-distribution/inference';
import { Array, Effect, Option, Record, Schema } from 'effect';
import { memberInputs, memberTargets } from '#/targets';
import { Linked, MANIFEST, PLUGIN, PRESETS, REGISTRY, resolved, run, type Tree } from '#/workspace';

// --- [MODELS] --------------------------------------------------------------------------

class Options extends Schema.Opaque<Options, unknown>()(
    Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), coveragePath: Schema.optionalKey(Schema.String) }).annotate({
        description:
            "outputPath: workspace folder each member writes its CPack archives under as <outputPath>/cpack, {projectName} naming the member, absent writes archives under packages in the member build folder. coveragePath: workspace folder the coverage configuration of a member's test target writes the lcov.info report of the member's test executables to, its raw and merged profiles staying in the member build folder, {projectName} naming the member, absent leaves each test its default.profraw and writes no report.",
    }),
) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = Effect.fnUntraced(function* (context: CreateNodesContext, settings: Options, matched: readonly string[], linked: Map<string, Tree['members']>, file: string) {
    const tree = yield* resolved(context.workspaceRoot, file, matched);
    linked.set(file, tree.members);
    const built = Array.fromNullishOr(tree.workflows[0]).flatMap((head) => tree.members.filter((member) => member.compiled || member.registries.length > 0).map((member) => ({ member, head })));
    const manifests = yield* Effect.forEach(built, ({ member }) => manifested(context.workspaceRoot, member.root));
    const projects = Record.fromIterableWith(Array.zip(built, manifests), ([{ member, head }, declared]): [string, ProjectConfiguration] => {
        const { default: sources } = getNamedInputs(member.root, context);
        const targets = memberTargets(tree, member, settings, head, declared, context.workspaceRoot);
        const project = {
            root: member.root,
            name: member.name,
            metadata: { technologies: ['cmake'] },
            ...(member.compiled && { namedInputs: { default: memberInputs(tree, member, head, sources) } }),
            ...('pack--cpack' in targets && { tags: ['release'], release: { version: { versionActions: `${PLUGIN}/release/version-actions` } } }),
            targets,
        };
        return [member.root, project];
    });
    return { projects };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    combineGlobPatterns([PRESETS, MANIFEST, '*.cmake', REGISTRY.manifest, REGISTRY.configuration].map((file) => `**/${file}`)),
    inferring(Options, run, function* (matched, settings, context) {
        const presets = matched.filter((file) => posix.basename(file) === PRESETS && matched.includes(posix.join(posix.dirname(file), MANIFEST)));
        const linked = new Map<string, Tree['members']>();
        (yield* Linked).set(context.workspaceRoot, linked);
        return yield* Effect.promise(() => createNodesFromFiles((file) => run(inferred(context, settings, matched, linked, file)), presets, settings, context));
    }),
];

const createDependencies: CreateDependencies = (_, { projects, workspaceRoot }) =>
    run(
        Effect.map(Linked, (links) => {
            const names = createProjectRootMappingsFromProjectConfigurations(projects);
            const linked = [...(links.get(workspaceRoot)?.values() ?? [])].flat().flatMap(({ root, links: held }) => held.map((link) => ({ root, link })));
            return Array.getSomes(
                linked.map(({ root, link }) =>
                    Option.map(Option.all({ source: Option.fromUndefinedOr(names.get(root)), target: Option.fromUndefinedOr(names.get(link.root)) }), (named): RawProjectGraphDependency =>
                        findProjectForPath(link.file, names) === named.source ? { ...named, type: DependencyType.static, sourceFile: link.file } : { ...named, type: DependencyType.implicit },
                    ),
                ),
            );
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
