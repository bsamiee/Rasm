import { findPackageJSON } from 'node:module';
import { layer } from '@effect/platform-node/NodeServices';
import type { CreateNodes, TargetConfiguration } from '@nx/devkit';
import { Array, Effect, FileSystem, Option, Path, Record, Schema } from 'effect';

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
    const read = yield* Effect.forEach(files, (file) => Effect.option(Effect.flatMap(fs.readFileString(path.join(workspaceRoot, file)), Schema.decodeEffect(Manifest))), { concurrency: 'unbounded' });
    const manifests = Array.getSomes(Array.zipWith(files, read, (file, held) => Option.map(held, (manifest) => ({ file, ...manifest, declared: Record.keys({ ...manifest.dependencies, ...manifest.devDependencies, ...manifest.optionalDependencies }) }))));
    const programs = new Set(Array.getSomes(manifests.map((held) => Option.filter(Option.fromUndefinedOr(held.name), () => '@pulumi/pulumi' in held.dependencies && held.declared.includes(plugin.name)))));
    return manifests.flatMap((held) => {
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
    });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = ['**/package.json', (files, _, context) => nodes(context.workspaceRoot, files).pipe(Effect.provide(layer), Effect.runPromise)];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
