import { dirname, join } from 'node:path';
import { type CreateNodes, createNodesFromFiles, type TargetConfiguration } from '@nx/devkit';
import { Array, Cause, Effect, FileSystem, Function, Record, Result, Schema } from 'effect';
import { run } from '#/executors/stacks';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = '@rasm/nx-pulumi';
const PULUMI = '@pulumi/pulumi';

// --- [MODELS] --------------------------------------------------------------------------

const Dependencies = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.optionalKey(Schema.String), dependencies: Dependencies, devDependencies: Dependencies, optionalDependencies: Dependencies }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = (programs: readonly string[], file: string) => (read: Result.Result<typeof Manifest.Type, Error>) =>
    Effect.map(Effect.fromResult(read), ({ name, dependencies }) => {
        const hosting = Array.intersection(Record.keys(dependencies), programs).toSorted();
        const stated = [...programs.filter((program) => program === name), ...hosting];
        const applied = (operation: string, description: string): TargetConfiguration => ({
            executor: `${PLUGIN}:${operation}`,
            options: { programs: stated },
            dependsOn: [hosting.length > 0 ? 'build--*' : '^build', `^${operation}`],
            parallelism: false,
            metadata: { technologies: ['pulumi'], description },
        });
        const tools: Record<string, TargetConfiguration> = {
            'install--pulumi': applied('install', "Applies the stacks a program package states for a project's local install, refreshing live state first"),
            'deploy--pulumi': {
                ...applied('deploy', 'Applies or destroys the stacks a program package states for a project in dependency order, refreshing live state first; configuration release applies the stacks of a released version'),
                configurations: { release: { production: true } },
            },
        };
        const targets = {
            ...tools,
            ...Record.map(
                Array.groupBy(Object.entries(tools), ([label]) => label.slice(0, label.indexOf('--'))),
                (members, verb): TargetConfiguration => ({ dependsOn: [`${verb}--*`], ...Array.match(Array.dedupe(members.flatMap(([, { inputs: hashed = [] }]) => hashed)), { onEmpty: () => ({}), onNonEmpty: (unique) => ({ inputs: ['...', ...unique] }) }) }),
            ),
        };
        return stated.length === 0 ? {} : { projects: { [dirname(file)]: { root: dirname(file), metadata: { technologies: ['pulumi'] }, targets } } };
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    '**/package.json',
    (files, options, context) =>
        run(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const manifests = yield* Effect.forEach(
                    files,
                    (file) => Effect.result(Effect.catchCause(Effect.flatMap(fs.readFileString(join(context.workspaceRoot, file)), Schema.decodeEffect(Manifest)), (cause) => Effect.fail(new Cause.UnknownError(cause, `${file}: does not read as a package manifest: ${Cause.pretty(cause)}`)))),
                    {
                        concurrency: 'unbounded',
                    },
                );
                const programs = Array.filterMap(manifests, (read) =>
                    Result.flatMap(Result.mapError(read, Function.constVoid), ({ name, dependencies, devDependencies, optionalDependencies }) =>
                        name !== undefined && PULUMI in { ...dependencies, ...devDependencies } && PLUGIN in { ...dependencies, ...devDependencies, ...optionalDependencies } ? Result.succeed(name) : Result.failVoid,
                    ),
                );
                return yield* Effect.promise(() => createNodesFromFiles((file, _options, _context, index) => run(Effect.flatMap(Effect.fromNullishOr(manifests[index]), inferred(programs, file))), files, options, context));
            }),
        ),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
