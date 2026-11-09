import { findPackageJSON } from 'node:module';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type TargetConfiguration } from '@nx/devkit';
import { Array, Effect, FileSystem, Option, Path, Record, Result, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Dependencies = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.optionalKey(Schema.String), dependencies: Dependencies, devDependencies: Dependencies, optionalDependencies: Dependencies }));
const Plugin = Schema.fromJsonString(Schema.Struct({ name: Schema.String, executors: Schema.String }));
const Executors = Schema.fromJsonString(Schema.Struct({ executors: Schema.Record(Schema.String, Schema.Struct({ description: Schema.String })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const nodes = Effect.fnUntraced(function* (workspaceRoot: string, files: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const located = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url));
    const plugin = yield* Effect.flatMap(fs.readFileString(located), Schema.decodeEffect(Plugin));
    const { executors } = yield* Effect.flatMap(fs.readFileString(path.join(path.dirname(located), plugin.executors)), Schema.decodeEffect(Executors));
    const read = yield* Effect.forEach(files, (file) => Effect.map(Effect.result(Effect.flatMap(fs.readFileString(path.join(workspaceRoot, file)), Schema.decodeEffect(Manifest))), (held) => [file, held] as const), { concurrency: 'unbounded' });
    const manifests = read.flatMap(([file, held]) => (Result.isSuccess(held) ? [{ file, ...held.success, declared: Record.keys({ ...held.success.dependencies, ...held.success.devDependencies, ...held.success.optionalDependencies }) }] : []));
    const programs = new Set(manifests.flatMap((held) => ('@pulumi/pulumi' in held.dependencies && held.declared.includes(plugin.name) ? Option.toArray(Option.fromUndefinedOr(held.name)) : [])));
    return {
        nodes: manifests.flatMap((held) => {
            const own = Option.toArray(Option.filter(Option.fromUndefinedOr(held.name), (named) => programs.has(named)));
            const hosting = held.declared.filter((dependency) => programs.has(dependency)).toSorted();
            const verbs: Record<string, readonly [string, readonly string[]]> = { deploy: ['deploy', []], ...(hosting.length > 0 ? { install: ['install', ['^install']], 'nx-release-publish': ['release-publish', ['^nx-release-publish']] } : {}) };
            return Array.match([...own, ...hosting], {
                onEmpty: () => [],
                onNonEmpty: (selected) => [
                    [
                        held.file,
                        {
                            projects: {
                                [path.dirname(held.file)]: {
                                    targets: Record.map(verbs, ([executor, dependsOn]): TargetConfiguration => ({ executor: `${plugin.name}:${executor}`, options: { programs: selected }, cache: false, dependsOn: [...dependsOn], metadata: { technologies: ['pulumi'], ...executors[executor] } })),
                                    ...(hosting.length > 0 ? { tags: ['release'] } : {}),
                                },
                            },
                        },
                    ] as const,
                ],
            });
        }),
        failures: read.flatMap(([file, held]) => (Result.isFailure(held) ? [[file, new Error(held.failure.message)] satisfies [string, Error]] : [])),
    };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    '**/package.json',
    (files, _, context) =>
        Effect.runPromise(
            Effect.flatMap(
                Effect.mapError(nodes(context.workspaceRoot, files), (error) => new AggregateCreateNodesError([[null, new Error(error.message)]], [])),
                (built) => (built.failures.length === 0 ? Effect.succeed(built.nodes) : Effect.fail(new AggregateCreateNodesError(built.failures, built.nodes))),
            ).pipe(Effect.provide(layer)),
        ),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
