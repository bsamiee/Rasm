import { dirname, join } from 'node:path';
import { type CreateNodes, createNodesFromFiles, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { mergeTargetConfigurations, type PackageJson } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Function, Predicate, Record, Result, Schema, UndefinedOr } from 'effect';
import { refused, run } from '#/executors/stacks';

// --- [TYPES] ---------------------------------------------------------------------------

type Tools = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = '@rasm/nx-pulumi';
const PULUMI = '@pulumi/pulumi';

// --- [MODELS] --------------------------------------------------------------------------

const Dependencies = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.optionalKey(Schema.String), dependencies: Dependencies, devDependencies: Dependencies, optionalDependencies: Dependencies }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const flattened = (tools: Tools, stated: Readonly<Record<string, TargetConfiguration>>): Record<string, TargetConfiguration> =>
    Record.fromEntries(
        Record.collect(Record.filter(tools, Predicate.not(Record.isEmptyRecord)), (verb, held): readonly (readonly [string, TargetConfiguration])[] => [
            ...Record.collect(held, (tool, target): readonly [string, TargetConfiguration] => [`${verb}--${tool}`, target]),
            [verb, { dependsOn: [`${verb}--*`], inputs: ['...', ...Array.dedupe(Record.collect(held, (tool, target) => UndefinedOr.match(stated[`${verb}--${tool}`], { onUndefined: () => target, onDefined: (row) => mergeTargetConfigurations(row, target) }).inputs ?? ['default', '^default']).flat())] }],
        ]).flat(),
    );

const inferred = (programs: readonly string[], file: string, workspaceRoot: string) => Effect.fnUntraced(function* (read: Result.Result<typeof Manifest.Type, Error>) {
    const { name, dependencies } = yield* Effect.fromResult(read);
    const hosting = Array.intersection(Record.keys(dependencies), programs).toSorted();
    const stated = [...programs.filter((program) => program === name), ...hosting];
    const applied = (operation: string, description: string): TargetConfiguration => ({
        executor: `${PLUGIN}:${operation}`,
        options: { programs: stated },
        dependsOn: [hosting.length > 0 ? 'build--*' : '^build', `^${operation}`],
        parallelism: false,
        metadata: { technologies: ['pulumi'], description },
    });
    const tools: Tools = {
        install: { pulumi: applied('install', "Applies the stacks a program package states for a project's local install, refreshing live state first") },
        deploy: {
            pulumi: {
                ...applied('deploy', 'Applies or destroys the stacks a program package states for a project in dependency order, refreshing live state first; configuration release applies the stacks of a released version'),
                configurations: { release: { production: true } },
            },
        },
    };
    return stated.length === 0 ? {} : { projects: { [dirname(file)]: { root: dirname(file), metadata: { technologies: ['pulumi'] }, targets: flattened(tools, yield* Effect.try({ try: () => readJsonFile<PackageJson>(join(workspaceRoot, file)).nx?.targets ?? {}, catch: refused(file, 'does not read as JSON') })) } } };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    '**/package.json',
    (files, options, context) =>
        run(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const manifests = yield* Effect.forEach(files, (file) => Effect.result(Effect.mapError(Effect.flatMap(fs.readFileString(join(context.workspaceRoot, file)), Schema.decodeEffect(Manifest)), refused(file, 'does not read as a package manifest'))), {
                    concurrency: 'unbounded',
                });
                const programs = Array.filterMap(manifests, (read) =>
                    Result.flatMap(Result.mapError(read, Function.constVoid), ({ name, dependencies, devDependencies, optionalDependencies }) =>
                        name !== undefined && PULUMI in { ...dependencies, ...devDependencies } && PLUGIN in { ...dependencies, ...devDependencies, ...optionalDependencies } ? Result.succeed(name) : Result.failVoid,
                    ),
                );
                return yield* Effect.promise(() => createNodesFromFiles((file, _options, _context, index) => run(Effect.flatMap(Effect.fromNullishOr(manifests[index]), inferred(programs, file, context.workspaceRoot))), files, options, context));
            }),
        ),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
