import { basename, dirname, join, relative } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, DependencyType, type ProjectConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, getNamedInputs, hashObject, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Context, Effect, Layer, ManagedRuntime, Option, Predicate, Record, Result, Schema } from 'effect';
import { CONFIG, memberInputs, memberTargets, PLUGIN, SHARED } from '#/targets';
import { MANIFEST, PRESETS, planted, refused, resolved, type Tree } from '#/workspace';

// --- [MODELS] --------------------------------------------------------------------------

const Placed = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({
    description: 'outputPath: workspace folder holding build trees as <outputPath>/cmake/<presets folder>/<configure preset>, test reports under ctest and packages under cpack, absent writes each under build in the presets folder',
});

// --- [SERVICES] ------------------------------------------------------------------------

class Links extends Context.Service<Links, Map<string, Tree['members']>>()('@rasm/nx-cmake/plugin/Links') {
    static readonly layer = Layer.sync(Links, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = Effect.fnUntraced(function* (cache: PluginCache<{ readonly tree: Tree; readonly digest: string }>, context: CreateNodesContext, outputPath: Option.Option<string>, matched: readonly string[], file: string) {
    const near = matched.filter((other) => !relative(dirname(file), other).startsWith('..'));
    const stamped = ({ inputs }: Tree) => Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [...near, ...inputs]));
    const held = yield* Effect.findFirst(Array.fromNullishOr(cache.get(file)), (stored) => Effect.map(stamped(stored.tree), (now) => now === stored.digest));
    const { tree, partial } = yield* Option.match(held, {
        onSome: (stored) => Effect.succeed({ tree: stored.tree, partial: true }),
        onNone: () => resolved(context.workspaceRoot, file, (preset) => planted(outputPath, dirname(file), 'cmake', preset)),
    });
    yield* partial ? Effect.void : Effect.map(stamped(tree), (digest) => cache.set(file, { tree, digest }));
    (yield* Links).set(join(context.workspaceRoot, file), tree.members);
    const built = Array.fromNullishOr(tree.workflows[0]).flatMap((head) => tree.members.filter(({ targets, installs, tested }) => targets.length > 0 || installs || tested).map((member) => ({ member, head })));
    const files = new Set(matched);
    const projects = Record.fromIterableWith(built, ({ member, head }): [string, ProjectConfiguration] => {
        const { default: sources } = getNamedInputs(member.root, context);
        const targets = memberTargets(tree, member, outputPath, files, head);
        const project = {
            root: member.root,
            name: member.name,
            metadata: { technologies: ['cmake'] },
            namedInputs: { default: memberInputs(tree, member, sources) },
            ...('pack--cpack' in targets && { tags: ['release'], release: { version: { versionActions: `${PLUGIN}/release/version-actions` } } }),
            targets,
        };
        return [member.root, project];
    });
    return { projects };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const runtime = ManagedRuntime.make(Layer.merge(Links.layer, layer));

const createNodes: CreateNodes = [
    combineGlobPatterns([PRESETS, MANIFEST, CONFIG].map((file) => `**/${file}`)),
    (matched, options, context) => {
        const cache = new PluginCache<{ readonly tree: Tree; readonly digest: string }>(join(workspaceDataDirectory, `cmake-${hashObject(options ?? {})}.hash`));
        const outputPath = Option.flatMap(Schema.decodeOption(Placed)({ ...context.nxJsonConfiguration.pluginsConfig?.[SHARED], ...(Predicate.isObject(options) ? options : {}) }), (placed) => Option.fromUndefinedOr(placed.outputPath));
        const presets = matched.filter((file) => basename(file) === PRESETS && matched.includes(join(dirname(file), MANIFEST)));
        return runtime.runPromise(
            Effect.ensuring(
                Effect.gen(function* () {
                    const read = Array.zip(presets, yield* Effect.forEach(presets, (file) => Effect.result(inferred(cache, context, outputPath, matched, file)), { concurrency: 'unbounded' }));
                    const failures = Array.filterMap(read, ([file, result]) => Result.map(Result.flip(result), (error): [string, Error] => [file, refused(file, error.message)(error)]));
                    const results = Array.filterMap(read, ([file, result]) => Result.map(result, (held) => [file, held] as const));
                    return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
                }),
                Effect.sync(() => cache.writeToDisk()),
            ),
        );
    },
];

const createDependencies: CreateDependencies = (_, { projects, workspaceRoot }) =>
    runtime.runPromise(
        Effect.map(Links, (links) => {
            const names = createProjectRootMappingsFromProjectConfigurations(projects);
            const members = [...links].flatMap(([presets, held]) => (relative(workspaceRoot, presets).startsWith('..') ? [] : held));
            const stated = members.flatMap(({ root, links: held }) => held.map((link) => ({ root, link })));
            return Array.getSomes(stated.map(({ root, link }) => Option.map(Option.all({ source: Option.fromUndefinedOr(names.get(root)), target: Option.fromUndefinedOr(names.get(link.root)) }), (named) => ({ ...named, type: DependencyType.static, sourceFile: link.file }))));
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
