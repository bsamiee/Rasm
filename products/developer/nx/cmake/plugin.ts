import { join, posix, relative } from 'node:path';
import { type CreateDependencies, type CreateNodes, type CreateNodesContext, createNodesFromFiles, DependencyType, hashArray, type ProjectConfiguration, type RawProjectGraphDependency } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, findProjectForPath, getNamedInputs, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { fingerprinted, manifested } from '@rasm/nx-distribution/inference';
import { Array, Effect, Equal, Option, Order, Record, Schema } from 'effect';
import { memberInputs, memberTargets } from '#/targets.ts';
import { Linked, MANIFEST, PLUGIN, PRESETS, resolved, run, type Tree } from '#/workspace.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REGISTRY = { manifest: 'vcpkg.json', configuration: 'vcpkg-configuration.json' } as const;
const OWN = fingerprinted(import.meta.dirname);

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), coveragePath: Schema.optionalKey(Schema.String) }).annotate({
    description:
        "outputPath: workspace folder each member writes its CPack archives under as <outputPath>/cpack, {projectName} naming the member, absent writes archives under packages in the member build folder. coveragePath: workspace folder the coverage configuration of a member's test target writes the lcov.info report of the member's linked binaries to, its raw and merged profiles staying in the member build folder, {projectName} naming the member, absent leaves each test its default.profraw and writes no report",
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = Effect.fnUntraced(function* (cache: PluginCache<{ readonly tree: Tree; readonly stamp: string; readonly digest: string }>, context: CreateNodesContext, settings: typeof Options.Type, matched: readonly string[], file: string) {
    const near = matched.filter((other) => !posix.relative(posix.dirname(file), other).startsWith('..'));
    const stamped = ({ inputs }: Tree) => Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [...inputs]));
    const digest = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, near)), yield* OWN]);
    const held = yield* Effect.findFirst(Option.toArray(Option.filter(Option.fromUndefinedOr(cache.get(file)), (stored) => stored.digest === digest)), (stored) => Effect.map(stamped(stored.tree), Equal.equals(stored.stamp)));
    const tree = yield* Option.match(held, {
        onSome: (stored) => Effect.succeed(stored.tree),
        onNone: () =>
            resolved(
                context.workspaceRoot,
                file,
                near.filter((other) => posix.basename(other) === MANIFEST),
            ),
    });
    yield* Option.isNone(held)
        ? Effect.map(stamped(tree), (stamp) => {
              cache.set(file, { tree, stamp, digest });
              cache.writeToDisk();
          })
        : Effect.void;
    (yield* Linked).set(join(context.workspaceRoot, file), tree.members);
    const owner = (folder: string) =>
        Array.min(
            Array.prepend(
                tree.members.map(({ root }) => root).filter((root) => !posix.relative(root, folder).startsWith('..')),
                tree.root,
            ),
            Order.mapInput(Order.Number, (root: string) => posix.relative(root, folder).length),
        );
    const registries = near.filter((other) => posix.basename(other) === REGISTRY.manifest).map((manifest) => ({ owner: owner(posix.dirname(manifest)), folder: posix.dirname(manifest), inputs: Array.intersection([manifest, posix.join(posix.dirname(manifest), REGISTRY.configuration)], matched) }));
    const claimed = Array.groupBy(registries, ({ owner: root }) => root);
    const built = Array.fromNullishOr(tree.workflows[0]).flatMap((head) => tree.members.map((member) => ({ member, head, compiled: member.targets.length > 0 || member.installs || member.tested, owned: claimed[member.root] ?? [] })).filter(({ compiled, owned }) => compiled || owned.length > 0));
    const manifests = yield* Effect.forEach(built, ({ member }) => manifested(context.workspaceRoot, member.root));
    const projects = Record.fromIterableWith(Array.zip(built, manifests), ([{ member, head, compiled, owned }, declared]): [string, ProjectConfiguration] => {
        const { default: sources } = getNamedInputs(member.root, context);
        const targets = memberTargets(tree, member, settings, owned, head, declared);
        const project = {
            root: member.root,
            name: member.name,
            metadata: { technologies: ['cmake'] },
            ...(compiled && { namedInputs: { default: memberInputs(tree, member, sources) } }),
            ...('pack--cpack' in targets && { tags: ['release'], release: { version: { versionActions: `${PLUGIN}/release/version-actions` } } }),
            targets,
        };
        return [member.root, project];
    });
    return { projects };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [
    combineGlobPatterns([PRESETS, MANIFEST, REGISTRY.manifest, REGISTRY.configuration].map((file) => `**/${file}`)),
    (matched, options, context) => {
        const cache = new PluginCache<{ readonly tree: Tree; readonly stamp: string; readonly digest: string }>(join(workspaceDataDirectory, 'cmake.hash'));
        const presets = matched.filter((file) => posix.basename(file) === PRESETS && matched.includes(posix.join(posix.dirname(file), MANIFEST)));
        return run(
            Effect.gen(function* () {
                const settings = yield* Schema.decodeEffect(Options)(options ?? {});
                return yield* Effect.promise(() => createNodesFromFiles((file) => run(inferred(cache, context, settings, matched, file)), presets, options, context));
            }),
        );
    },
];

const createDependencies: CreateDependencies = (_, { projects, workspaceRoot }) =>
    run(
        Effect.map(Linked, (links) => {
            const names = createProjectRootMappingsFromProjectConfigurations(projects);
            const members = [...links].flatMap(([presets, held]) => (relative(workspaceRoot, presets).startsWith('..') ? [] : held));
            const linked = members.flatMap(({ root, links: held }) => held.map((link) => ({ root, link })));
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
