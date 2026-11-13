import type { TargetConfiguration } from '@nx/devkit';
import { inferred, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { Array, Effect, FileSystem, Option, Path, Record, Result, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Hosted {
    readonly file: string;
    readonly programs: Array.NonEmptyReadonlyArray<string>;
    readonly hosted: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'package.json';
const TECHNOLOGIES = ['pulumi'];

// --- [MODELS] --------------------------------------------------------------------------

const Dependencies = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.optionalKey(Schema.String), dependencies: Dependencies, devDependencies: Dependencies, optionalDependencies: Dependencies }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes } = inferred({
    module: import.meta.url,
    files: [MANIFEST],
    options: Schema.Struct({}),
    units: ({ files }: Request<unknown>) => Effect.succeed(Array.take(files, 1).map((file): Unit => ({ key: MANIFEST, file, salt: files.join('\n') }))),
    read: Effect.fnUntraced(function* (missing: Array.NonEmptyReadonlyArray<Unit>, { files, context, plugin }: Request<unknown>) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const decoded = Array.zip(files, yield* Effect.forEach(files, (file) => Effect.result(Effect.flatMap(fs.readFileString(path.join(context.workspaceRoot, file)), Schema.decodeEffect(Manifest))), { concurrency: 'unbounded' }));
        const manifests = Array.filterMap(decoded, ([file, result]) => Result.map(result, (manifest) => ({ file, ...manifest })));
        const programs = new Set(Array.getSomes(manifests.map(({ name, dependencies, devDependencies, optionalDependencies }) => Option.filter(Option.fromUndefinedOr(name), () => '@pulumi/pulumi' in dependencies && plugin.name in { ...dependencies, ...devDependencies, ...optionalDependencies }))));
        const report = Array.filterMap(manifests, ({ file, name, dependencies }) => {
            const hosting = Array.intersection(Record.keys(dependencies), [...programs]).toSorted();
            const named = [...(name !== undefined && programs.has(name) ? [name] : []), ...hosting];
            return Array.isReadonlyArrayNonEmpty(named) ? Result.succeed<Hosted>({ file, hosted: hosting.length > 0, programs: named }) : Result.failVoid;
        });
        const failures = Array.filterMap(decoded, ([file, result]) => Result.map(Result.flip(result), (error): readonly [string, Error] => [file, error]));
        return missing.map(() => Result.succeed({ report, failures }));
    }),
    sources: () => ({ tracked: [`**/${MANIFEST}`] }),
    projects: (report: readonly Hosted[], _, { path, plugin }) =>
        report.map(({ file, programs, hosted }) => {
            const stacks = (request: string, description: string, dependsOn: readonly string[]): TargetConfiguration => ({ executor: `${plugin.name}:stacks`, options: { programs, request }, dependsOn: [hosted ? 'build--*' : '^build', ...dependsOn], metadata: { description } });
            const deploy = { ...stacks('Deploy', 'Applies or destroys the stacks a program package states for a project, refreshing live state first; configuration release applies the stacks of a released version, previewing them under a dry run', ['check']), configurations: { release: { request: 'Release' } } };
            return {
                file,
                root: path.dirname(file),
                technologies: TECHNOLOGIES,
                members: { ...(hosted && { install: { pulumi: stacks('Install', "Applies the stacks a program package states for a project's local install, refreshing live state first", ['^install']) } }), deploy: { pulumi: deploy } },
            };
        }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
