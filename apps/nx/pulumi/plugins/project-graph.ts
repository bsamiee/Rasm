import { type CreateNodes, type CreateNodesContext, type CreateNodesResultArray, hashArray, type TargetConfiguration } from '@nx/devkit';
import { hashWithWorkspaceContext } from '@nx/devkit/internal';
import { aggregated, cached, identity, inferred, reported } from '@rasm/nx-devkit/inference';
import { Array, Effect, FileSystem, Option, Path, Record, Result, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Project {
    readonly file: string;
    readonly programs: Array.NonEmptyReadonlyArray<string>;
    readonly hosted: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = '**/package.json';
const TECHNOLOGIES = ['pulumi'];

// --- [MODELS] --------------------------------------------------------------------------

const Dependencies = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.optionalKey(Schema.String), dependencies: Dependencies, devDependencies: Dependencies, optionalDependencies: Dependencies }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const targets = (plugin: string, { programs, hosted }: Project) => {
    const stacks = (executor: string, description: string, dependsOn: readonly string[]): TargetConfiguration => ({ executor: `${plugin}:${executor}`, options: { programs }, dependsOn: [hosted ? 'build' : '^build', ...dependsOn], metadata: { technologies: TECHNOLOGIES, description } });
    return {
        deploy: stacks('deploy', 'Applies or destroys the stacks a program package states for a project, refreshing live state first', []),
        ...(hosted
            ? {
                  ...aggregated({ install: { pulumi: stacks('install', "Applies the stacks a program package states for a project's local install, refreshing live state first", ['^install']) } }, TECHNOLOGIES),
                  'nx-release-publish': stacks('release-publish', 'Applies the stacks a program package states for a released version, previewing them under a dry run', ['^nx-release-publish']),
              }
            : {}),
    };
};

const read = Effect.fnUntraced(function* (workspaceRoot: string, files: readonly string[], plugin: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const decoded = Array.zip(files, yield* Effect.forEach(files, (file) => Effect.result(Effect.flatMap(fs.readFileString(path.join(workspaceRoot, file)), Schema.decodeEffect(Manifest))), { concurrency: 'unbounded' }));
    const manifests = Array.filterMap(decoded, ([file, result]) => Result.map(result, (manifest) => ({ file, ...manifest })));
    const programs = new Set(Array.getSomes(manifests.map(({ name, dependencies, devDependencies, optionalDependencies }) => Option.filter(Option.fromUndefinedOr(name), () => '@pulumi/pulumi' in dependencies && plugin in { ...dependencies, ...devDependencies, ...optionalDependencies }))));
    const selected = manifests.map(({ file, name, dependencies }) => {
        const hosting = Record.keys(dependencies)
            .filter((dependency) => programs.has(dependency))
            .toSorted();
        return { file, hosted: hosting.length > 0, programs: [...Option.toArray(Option.filter(Option.fromUndefinedOr(name), (named) => programs.has(named))), ...hosting] };
    });
    return {
        projects: Array.filterMap(selected, ({ programs: named, ...project }) => (Array.isReadonlyArrayNonEmpty(named) ? Result.succeed<Project>({ ...project, programs: named }) : Result.failVoid)),
        failures: Array.filterMap(decoded, ([file, result]) => Result.map(Result.flip(result), (error) => [file, error] as const)),
    };
});

const nodes = Effect.fnUntraced(function* ({ workspaceRoot }: CreateNodesContext, files: readonly string[], options: unknown) {
    const [path, plugin] = yield* Effect.all([Path.Path, identity(import.meta.url, workspaceRoot)]);
    const key = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [FILES])), plugin.digest, ...files]);
    const cache = yield* cached<readonly Project[]>(plugin, options);
    const held = yield* cache.get(FILES, () => Effect.succeed(key));
    const { projects, failures } = yield* Option.match(held, {
        onSome: (kept) => Effect.succeed({ projects: kept, failures: Array.empty<readonly [string, Error]>() }),
        onNone: () => read(workspaceRoot, files, plugin.name),
    });
    yield* Option.isNone(held) && failures.length === 0 ? cache.set(FILES, projects, key) : Effect.void;
    return yield* reported(
        projects.map((project): CreateNodesResultArray[number] => [project.file, { projects: { [path.dirname(project.file)]: { targets: targets(plugin.name, project), ...(project.hosted ? { tags: ['release'] } : {}) } } }]),
        failures,
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [FILES, (files, options, context) => inferred(nodes(context, files, options))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
