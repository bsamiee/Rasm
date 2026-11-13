import { dirname, join } from 'node:path';
import type { CreateNodes, CreateNodesContext, TargetConfiguration } from '@nx/devkit';
import { promised } from '@rasm/nx-devkit/process';
import { collected, configured } from '@rasm/nx-devkit/project';
import { Array, Effect, FileSystem, Function, Record, Result, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = '@rasm/nx-pulumi';
const PULUMI = '@pulumi/pulumi';

// --- [MODELS] --------------------------------------------------------------------------

const Dependencies = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.optionalKey(Schema.String), dependencies: Dependencies, devDependencies: Dependencies, optionalDependencies: Dependencies }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = Effect.fnUntraced(function* (context: CreateNodesContext, programs: readonly string[], file: string, read: Result.Result<typeof Manifest.Type, Error>) {
    const { name, dependencies } = yield* Effect.fromResult(read);
    const root = dirname(file);
    const hosting = Array.intersection(Record.keys(dependencies), programs).toSorted();
    const stated = [...programs.filter((program) => program === name), ...hosting];
    const applied = (operation: string, description: string, upstream: string): TargetConfiguration => ({
        executor: `${PLUGIN}:${operation}`,
        options: { programs: stated },
        dependsOn: [hosting.length > 0 ? 'build--*' : '^build', upstream],
        parallelism: false,
        metadata: { description },
    });
    const members = {
        install: { pulumi: applied('install', "Applies the stacks a program package states for a project's local install, refreshing live state first", '^install') },
        deploy: {
            pulumi: {
                ...applied('deploy', 'Applies or destroys the stacks a program package states for a project in dependency order, refreshing live state first; configuration release applies the stacks of a released version', '^deploy'),
                configurations: { release: { production: true } },
            },
        },
    };
    return stated.length === 0 ? {} : { projects: { [root]: yield* configured(members, { root, metadata: { technologies: ['pulumi'] }, clean: 'pulumi' }, context) } };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    '**/package.json',
    (files, options, context) =>
        promised(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const manifests = yield* Effect.forEach(files, (file) => Effect.result(Effect.flatMap(fs.readFileString(join(context.workspaceRoot, file)), Schema.decodeEffect(Manifest))), { concurrency: 'unbounded' });
                const programs = Array.filterMap(manifests, (read) =>
                    Result.flatMap(Result.mapError(read, Function.constVoid), ({ name, dependencies, devDependencies, optionalDependencies }) =>
                        name !== undefined && PULUMI in { ...dependencies, ...devDependencies } && PLUGIN in { ...dependencies, ...devDependencies, ...optionalDependencies } ? Result.succeed(name) : Result.failVoid,
                    ),
                );
                const reads = new Map(Array.zip(files, manifests));
                return yield* collected((file) => promised(Effect.flatMap(Effect.fromNullishOr(reads.get(file)), (read) => inferred(context, programs, file, read))), files, options, context);
            }),
        ),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
