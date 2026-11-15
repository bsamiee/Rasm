import { readdirSync } from 'node:fs';
import { basename, dirname, join, relative } from 'node:path';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, DependencyType, hashArray, type ProjectConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, getNamedInputs, hashFile, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Cause, Effect, Equal, Option, Order, Record, Result, Schema } from 'effect';
import { memberInputs, memberTargets } from '#/targets';
import { Linked, MANIFEST, manifested, PLUGIN, PRESETS, resolved, run, type Tree } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REGISTRY = { manifest: 'vcpkg.json', configuration: 'vcpkg-configuration.json' } as const;
const OWN = hashArray(
    readdirSync(import.meta.dirname)
        .toSorted()
        .flatMap((name) => [name, ...Array.fromNullishOr(hashFile(join(import.meta.dirname, name)))]),
);

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), coveragePath: Schema.optionalKey(Schema.String) }).annotate({
    description:
        "outputPath: workspace folder each member writes its CTest reports under as <outputPath>/ctest and its CPack archives under as <outputPath>/cpack, {projectName} naming the member, absent writes reports under Testing and archives under packages in the member build folder. coveragePath: workspace folder the coverage configuration of a member's test target writes the lcov.info report of the member's linked binaries to, its raw and merged profiles staying in the member build folder, {projectName} naming the member, absent leaves each test its default.profraw and writes no report",
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const rendered = (cause: Cause.Cause<unknown>) => Effect.fail(new Cause.UnknownError(cause, Cause.pretty(cause)));

const inferred = Effect.fnUntraced(function* (cache: PluginCache<{ readonly tree: Tree; readonly stamp: string }>, context: CreateNodesContext, settings: typeof Options.Type, matched: readonly string[], file: string) {
    const near = matched.filter((other) => !relative(dirname(file), other).startsWith('..'));
    const stamped = ({ inputs }: Tree) => Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [...inputs]));
    const key = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, near)), OWN, file]);
    const held = yield* Effect.findFirst(Option.toArray(Option.fromUndefinedOr(cache.get(key))), (stored) => Effect.map(stamped(stored.tree), Equal.equals(stored.stamp)));
    const tree = yield* Option.match(held, {
        onSome: (stored) => Effect.succeed(stored.tree),
        onNone: () =>
            resolved(
                context.workspaceRoot,
                file,
                near.filter((other) => basename(other) === MANIFEST),
            ),
    });
    yield* Option.isNone(held)
        ? Effect.map(stamped(tree), (stamp) => {
              cache.set(key, { tree, stamp });
              cache.writeToDisk();
          })
        : Effect.void;
    (yield* Linked).set(join(context.workspaceRoot, file), tree.members);
    const owner = (folder: string) =>
        Array.min(
            Array.prepend(
                tree.members.map(({ root }) => root).filter((root) => !relative(root, folder).startsWith('..')),
                tree.root,
            ),
            Order.mapInput(Order.Number, (root: string) => relative(root, folder).length),
        );
    const registries = near.filter((other) => basename(other) === REGISTRY.manifest).map((manifest) => ({ owner: owner(dirname(manifest)), folder: dirname(manifest), inputs: Array.intersection([manifest, join(dirname(manifest), REGISTRY.configuration)], matched) }));
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
        const cache = new PluginCache<{ readonly tree: Tree; readonly stamp: string }>(join(workspaceDataDirectory, 'cmake.hash'));
        const presets = matched.filter((file) => basename(file) === PRESETS && matched.includes(join(dirname(file), MANIFEST)));
        return run(
            Effect.gen(function* () {
                const settings = yield* Schema.decodeEffect(Options)(options ?? {});
                const read = Array.zip(presets, yield* Effect.forEach(presets, (file) => Effect.result(Effect.catchCause(inferred(cache, context, settings, matched, file), rendered)), { concurrency: 'unbounded' }));
                const failures = Array.filterMap(read, ([file, result]) => Result.map(Result.flip(result), (error): [string, Error] => [file, error]));
                const results = Array.filterMap(read, ([file, result]) => Result.map(result, (held) => [file, held] as const));
                return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
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
            return Array.getSomes(linked.map(({ root, link }) => Option.map(Option.all({ source: Option.fromUndefinedOr(names.get(root)), target: Option.fromUndefinedOr(names.get(link.root)) }), (named) => ({ ...named, type: DependencyType.static, sourceFile: link.file }))));
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
