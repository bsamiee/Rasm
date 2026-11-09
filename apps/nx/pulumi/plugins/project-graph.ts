import { findPackageJSON } from 'node:module';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type TargetConfiguration } from '@nx/devkit';
import { Array, Effect, FileSystem, Option, Path, Record, Result, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = '**/package.json';
const SDK = '@pulumi/pulumi';
const TECHNOLOGIES = ['pulumi'];
const RELEASE = 'release';

// --- [MODELS] --------------------------------------------------------------------------

const Dependencies = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.optionalKey(Schema.String), dependencies: Dependencies, devDependencies: Dependencies, optionalDependencies: Dependencies }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const target = (plugin: string, executor: string, description: string, programs: readonly string[], dependsOn: readonly string[]): TargetConfiguration => ({
    executor: `${plugin}:${executor}`,
    options: { programs },
    cache: false,
    dependsOn: [...dependsOn],
    metadata: { technologies: TECHNOLOGIES, description },
});

const targets = (plugin: string, programs: readonly string[], hosted: boolean): Record<string, TargetConfiguration> => ({
    deploy: target(plugin, 'deploy', `Applies the stacks ${programs.join(', ')} states for this project, refreshing live state first`, programs, []),
    ...(hosted
        ? {
              install: target(plugin, 'install', `Applies the stacks ${programs.join(', ')} states for this project's local install`, programs, ['^install']),
              'nx-release-publish': target(plugin, 'release-publish', `Applies the stacks ${programs.join(', ')} states for the released version`, programs, ['^nx-release-publish']),
          }
        : {}),
});

const nodes = Effect.fnUntraced(function* (workspaceRoot: string, files: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const plugin = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url)).pipe(
        Effect.flatMap(fs.readFileString),
        Effect.flatMap(Schema.decodeEffect(Manifest)),
        Effect.flatMap(({ name }) => Effect.fromNullishOr(name)),
    );
    const read = yield* Effect.forEach(files, (file) => Effect.result(Effect.flatMap(fs.readFileString(path.join(workspaceRoot, file)), Schema.decodeEffect(Manifest))).pipe(Effect.map((held) => [file, held] as const)), { concurrency: 'unbounded' });
    const manifests = read.flatMap(([file, held]) => (Result.isSuccess(held) ? [{ file, ...held.success }] : []));
    const programs = new Set(manifests.flatMap((held) => (SDK in held.dependencies ? Option.toArray(Option.fromUndefinedOr(held.name)) : [])));
    return {
        nodes: manifests.flatMap((held) => {
            const own = Option.toArray(Option.filter(Option.fromUndefinedOr(held.name), (named) => programs.has(named)));
            const hosting = Record.keys({ ...held.dependencies, ...held.devDependencies, ...held.optionalDependencies })
                .filter((dependency) => programs.has(dependency) && dependency !== held.name)
                .toSorted();
            return Array.match([...own, ...hosting], {
                onEmpty: () => [],
                onNonEmpty: (selected) => [[held.file, { projects: { [path.dirname(held.file)]: { targets: targets(plugin, selected, hosting.length > 0), ...(hosting.length > 0 ? { tags: [RELEASE] } : {}) } } }] as const],
            });
        }),
        failures: read.flatMap(([file, held]) => (Result.isFailure(held) ? [[file, new Error(held.failure.message)] satisfies [string, Error]] : [])),
    };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
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
