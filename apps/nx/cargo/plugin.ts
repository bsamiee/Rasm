import { readdirSync } from 'node:fs';
import { basename, dirname, join, relative } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, DependencyType, hashArray, type ProjectConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, getNamedInputs, globWithWorkspaceContext, hashFile, hashObject, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Context, Effect, FileSystem, Formatter, Layer, ManagedRuntime, Option, Predicate, Record, Result, Schema, String } from 'effect';
import { CLIPPY, CONFIG, crateInputs, NEXTEST, projectTargets, released, SHARED } from '#/targets';
import { MANIFEST, type Member, type Reading, refused, resolved, workspaces } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = combineGlobPatterns([MANIFEST, ...CONFIG, ...CLIPPY, NEXTEST, '{,.}deny{,.exceptions}.toml'].map((file) => `**/${file}`));
const LAYOUT = ['src/{main,lib}.rs', '{src/bin,examples,tests,benches}/{*.rs,*/main.rs}', 'build.rs'] as const;
const CODE = hashArray(
    readdirSync(import.meta.dirname, { withFileTypes: true })
        .flatMap((entry) => (entry.isFile() ? Array.fromNullishOr(hashFile(join(entry.parentPath, entry.name))) : []))
        .toSorted(),
);

// --- [MODELS] --------------------------------------------------------------------------

const Placed = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({
    description: 'outputPath: workspace folder holding coverage reports as <outputPath>/llvm-cov/<project name>/lcov.info, absent writes each as llvm-cov/<crate>.lcov in the Cargo target folder',
});

// --- [SERVICES] ------------------------------------------------------------------------

class Links extends Context.Service<Links, Map<string, Member>>()('@rasm/nx-cargo/plugin/Links') {
    static readonly layer = Layer.sync(Links, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = Effect.fnUntraced(function* (context: CreateNodesContext, files: ReadonlySet<string>, reported: Option.Option<string>, reading: Reading) {
    const [fs, links] = yield* Effect.all([FileSystem.FileSystem, Links]);
    const workspace = resolved(context.workspaceRoot, reading);
    workspace.members.forEach((member) => links.set(dirname(member.crate.manifest_path), member));
    const rooted = (yield* fs.exists(join(context.workspaceRoot, workspace.root, 'package.json'))) ? [[workspace.root, Option.none()] as const] : [];
    const roots = new Map<string, Option.Option<Member>>([...rooted, ...workspace.members.map((member) => [member.root, Option.some(member)] as const)]);
    const projects = Record.fromIterableWith(roots, ([root, member]): [string, ProjectConfiguration] => {
        const targets = projectTargets(workspace, files, reported, member);
        const stated = Option.map(member, (held) => {
            const { default: sources } = getNamedInputs(root, context);
            const implicit = held.crate.metadata?.nx?.implicitDependencies;
            const delivery = released(held, workspace.root);
            return {
                name: String.kebabToSnake(held.crate.name),
                namedInputs: { default: crateInputs(workspace, files, held, sources) },
                ...(implicit !== undefined && { implicitDependencies: [...implicit] }),
                ...delivery,
                targets: { ...targets, ...delivery.targets },
            };
        });
        return [root, { root, metadata: { technologies: ['rust'] }, targets, ...Option.getOrUndefined(stated) }];
    });
    return { projects };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const runtime = ManagedRuntime.make(Layer.merge(Links.layer, layer));

const createNodes: CreateNodes = [
    FILES,
    (matched, options, context) =>
        runtime.runPromise(
            Effect.gen(function* () {
                const { workspaceRoot } = context;
                const files = new Set(matched);
                const manifests = matched.filter((file) => basename(file) === MANIFEST);
                const cache = new PluginCache<readonly Reading[]>(join(workspaceDataDirectory, `cargo-${hashObject(options ?? {})}.hash`));
                const [content, laid] = yield* Effect.promise(() => Promise.all([hashWithWorkspaceContext(workspaceRoot, [FILES]), globWithWorkspaceContext(workspaceRoot, Array.cartesianWith(manifests.map(dirname), LAYOUT, join))]));
                const key = hashArray([CODE, content, ...laid]);
                const read = yield* Option.match(Option.fromNullishOr(cache.get(key)), {
                    onNone: () => workspaces(workspaceRoot, manifests, files),
                    onSome: (held) => Effect.succeed(held.map((reading) => [join(relative(workspaceRoot, reading.metadata.workspace_root), MANIFEST), Result.succeed(reading)] as const)),
                });
                const failures = Array.filterMap(read, ([file, reading]) => Result.map(Result.flip(reading), (error): [string, Error] => [file, refused(file, Formatter.format(error))(error)]));
                yield* Effect.sync(() => (failures.length === 0 ? cache.set(key, Array.getSuccesses(read.map(([, reading]) => reading))) : undefined));
                yield* Effect.sync(() => cache.writeToDisk());
                const reported = Option.map(
                    Option.flatMap(Schema.decodeOption(Placed)({ ...context.nxJsonConfiguration.pluginsConfig?.[SHARED], ...(Predicate.isObject(options) ? options : {}) }), ({ outputPath }) => Option.fromUndefinedOr(outputPath)),
                    (at) => join(at, 'llvm-cov', '{projectName}', 'lcov.info'),
                );
                const results = yield* Effect.forEach(
                    Array.filterMap(read, ([file, reading]) => Result.map(reading, (held) => [file, held] as const)),
                    ([file, held]) => Effect.map(inferred(context, files, reported, held), (result) => [file, result] as const),
                );
                return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
            }),
        ),
];

const createDependencies: CreateDependencies = (_, { projects, workspaceRoot }) =>
    runtime.runPromise(
        Effect.map(Links, (links) => {
            const names = createProjectRootMappingsFromProjectConfigurations(projects);
            const named = ({ root }: Member) => Option.fromUndefinedOr(names.get(root));
            const members = [...links.values()].filter(({ crate }) => !relative(workspaceRoot, crate.manifest_path).startsWith('..'));
            const stated = members.flatMap((member) => Array.getSomes(member.crate.dependencies.map(({ path }) => Option.fromUndefinedOr(path))).map((at) => ({ member, to: Option.fromUndefinedOr(links.get(at)) })));
            return Array.getSomes(stated.map(({ member, to }) => Option.map(Option.all({ source: named(member), target: Option.flatMap(to, named) }), (edge) => ({ ...edge, type: DependencyType.static, sourceFile: member.manifest }))));
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
