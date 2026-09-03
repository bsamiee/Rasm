// Nx plugin that infers one project per packaging project under eng/native, with a stage target for the native files and a cached pack target

// --- [IMPORTS] -------------------------------------------------------------------------

import { FileSystem, Path } from '@effect/platform';
import { NodeFileSystem, NodePath } from '@effect/platform-node';
import {
    type CreateDependencies,
    type CreateDependenciesContext,
    type CreateNodes,
    type CreateNodesResult,
    createNodesFromFiles,
    DependencyType,
    type ProjectConfiguration,
    type RawProjectGraphDependency,
    type TargetConfiguration,
    validateDependency,
} from '@nx/devkit';
import { Array, Data, Effect, HashSet, Inspectable, Layer, ManagedRuntime, Option, Record, Schema, String } from 'effect';
import { XMLParser } from 'fast-xml-parser';

// --- [TYPES] ---------------------------------------------------------------------------

declare namespace NativePackaging {
    type Platform = FileSystem.FileSystem | Path.Path;
    type Reason = 'unreadable' | 'malformed' | 'version' | 'manifest' | 'native' | 'source' | 'dependency';
    type Project = {
        readonly file: string;
        readonly name: string;
        readonly root: string;
        readonly library: string;
        readonly version: string;
        readonly managed: boolean;
    };
}

// --- [MODELS] --------------------------------------------------------------------------

// fast-xml-parser output for the elements the plugin reads, repeated elements decode as arrays and every value stays a string
const _ProjectFile = Schema.Struct({
    Project: Schema.Struct({
        PropertyGroup: Schema.optionalWith(
            Schema.Array(Schema.Struct({ Version: Schema.optional(Schema.String), IncludeBuildOutput: Schema.optional(Schema.String) })),
            { default: () => [] },
        ),
        ItemGroup: Schema.optionalWith(
            Schema.Array(
                Schema.Struct({
                    PackageReference: Schema.optionalWith(Schema.Array(Schema.Struct({ Include: Schema.optional(Schema.String) })), {
                        default: () => [],
                    }),
                }),
            ),
            { default: () => [] },
        ),
    }),
});

const _NuGetConfig = Schema.Struct({
    configuration: Schema.Struct({
        packageSources: Schema.Struct({
            add: Schema.optionalWith(Schema.Array(Schema.Struct({ key: Schema.String, value: Schema.String })), { default: () => [] }),
        }),
    }),
});

// --- [CONSTANTS] -----------------------------------------------------------------------

const _PROJECT_FILE_GLOB = 'eng/native/*/*.csproj';
const _ARTIFACTS_ROOT = '.artifacts/native';
const _TAG = 'native';
const _NUGET_CONFIG = 'NuGet.config';
const _LOCAL_SOURCE_KEY = 'local'; // The local source in NuGet.config owns the pack output path, restore reads the nupkg from there
const _REPEATED_ELEMENTS: ReadonlyArray<string> = ['PropertyGroup', 'ItemGroup', 'PackageReference', 'add'];

// --- [ERRORS] --------------------------------------------------------------------------

// Nx prints the message of every rejected file, each message states what happened, the cause, then the action
const _MESSAGES: Record<NativePackaging.Reason, (file: string, detail: string) => string> = {
    unreadable: (file, detail) => `Reading ${file} failed, ${detail}, check that the file exists and is readable`,
    malformed: (file, detail) => `Decoding ${file} failed, ${detail}, repair the XML so the named element holds the expected text`,
    version: (file) =>
        `${file} declares no Version property, the pack target names the nupkg after the package version, add a Version property to the project`,
    manifest: (file, detail) =>
        `${file} has no version manifest directory at ${detail}, the stage target reads the library version from that directory, add the directory with its manifest`,
    native: (file, detail) =>
        `${file} has no native packaging project for ${detail}, its pack target depends on the stage target of that project, add the native packaging project beside it`,
    source: (file, detail) =>
        `${file} declares no package source with key ${detail}, the pack target writes the nupkg into that source, add the source under packageSources`,
    dependency: (file, detail) => `${file} declares a PackageReference edge Nx rejects, ${detail}, check the project names in the graph`,
};

class NativePackagingError extends Data.TaggedError('NativePackagingError')<{
    readonly reason: NativePackaging.Reason;
    readonly file: string;
    readonly detail: string;
}> {
    override get message(): string {
        return _MESSAGES[this.reason](this.file, this.detail);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _parser = new XMLParser({
    ignoreAttributes: false,
    attributeNamePrefix: '',
    parseTagValue: false,
    isArray: (name) => Array.contains(_REPEATED_ELEMENTS, name),
});

const _fail =
    (reason: NativePackaging.Reason, file: string) =>
    (cause: unknown): NativePackagingError =>
        new NativePackagingError({ reason, file, detail: cause instanceof Error ? cause.message : Inspectable.toStringUnknown(cause) });

const _decodeXml =
    <A, I>(schema: Schema.Schema<A, I>) =>
    (file: string, text: string): Effect.Effect<A, NativePackagingError> =>
        Effect.flatMap(Effect.try({ try: (): unknown => _parser.parse(text, true), catch: _fail('malformed', file) }), (parsed) =>
            Effect.mapError(Schema.decodeUnknown(schema)(parsed), _fail('malformed', file)),
        );

const _readXml =
    <A, I>(schema: Schema.Schema<A, I>) =>
    (workspaceRoot: string, file: string): Effect.Effect<A, NativePackagingError, NativePackaging.Platform> =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const text = yield* Effect.mapError(fs.readFileString(path.join(workspaceRoot, file)), _fail('unreadable', file));
            return yield* _decodeXml(schema)(file, text);
        });

const _readProjectFile = _readXml(_ProjectFile);
const _readNuGetConfig = _readXml(_NuGetConfig);

// MSBuild keeps the last declaration of a property, the lookup follows that order
const _property = (project: typeof _ProjectFile.Type, name: 'Version' | 'IncludeBuildOutput'): Option.Option<string> =>
    Array.last(Array.filterMap(project.Project.PropertyGroup, (group) => Option.fromNullable(group[name])));

const _packageReferences = (project: typeof _ProjectFile.Type): ReadonlyArray<string> =>
    Array.flatMap(project.Project.ItemGroup, (group) =>
        Array.filterMap(group.PackageReference, (reference) => Option.fromNullable(reference.Include)),
    );

// The library name is the last segment of the project name in lower case, its version manifest directory sits beside the project
const _library = (name: string): string => String.toLowerCase(Array.lastNonEmpty(String.split(name, '.')));

const _project = (
    file: string,
    root: string,
    name: string,
    decoded: typeof _ProjectFile.Type,
): Effect.Effect<NativePackaging.Project, NativePackagingError> =>
    Option.match(_property(decoded, 'Version'), {
        onNone: () => Effect.fail(new NativePackagingError({ reason: 'version', file, detail: '' })),
        onSome: (version) =>
            Effect.succeed({
                file,
                name,
                root,
                library: _library(name),
                version,
                managed: Option.contains(_property(decoded, 'IncludeBuildOutput'), 'true'),
            }),
    });

const _readProject = (workspaceRoot: string, file: string): Effect.Effect<NativePackaging.Project, NativePackagingError, NativePackaging.Platform> =>
    Effect.gen(function* () {
        const path = yield* Path.Path;
        const decoded = yield* _readProjectFile(workspaceRoot, file);
        return yield* _project(file, path.dirname(file), path.basename(file, '.csproj'), decoded);
    });

const _withManifestDirectory = (
    workspaceRoot: string,
    project: NativePackaging.Project,
): Effect.Effect<NativePackaging.Project, NativePackagingError, NativePackaging.Platform> =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const directory = path.join(path.dirname(project.root), project.library);
        const present = yield* Effect.mapError(fs.exists(path.join(workspaceRoot, directory)), _fail('unreadable', project.file));
        return yield* present
            ? Effect.succeed(project)
            : Effect.fail(new NativePackagingError({ reason: 'manifest', file: project.file, detail: directory }));
    });

// Managed bindings pack after the native packaging project of the same library stages its files
const _nativeProject = (
    workspaceRoot: string,
    project: NativePackaging.Project,
    configFiles: ReadonlyArray<string>,
): Effect.Effect<NativePackaging.Project, NativePackagingError, NativePackaging.Platform> =>
    project.managed
        ? Effect.gen(function* () {
              const path = yield* Path.Path;
              const siblings = Array.filter(
                  configFiles,
                  (file) => file !== project.file && _library(path.basename(file, '.csproj')) === project.library,
              );
              const candidates = yield* Effect.forEach(siblings, (file) => _readProject(workspaceRoot, file));
              return yield* Option.match(
                  Array.findFirst(candidates, (candidate) => !candidate.managed),
                  {
                      onNone: () => Effect.fail(new NativePackagingError({ reason: 'native', file: project.file, detail: project.library })),
                      onSome: Effect.succeed,
                  },
              );
          })
        : Effect.succeed(project);

const _stageTarget = (library: string): TargetConfiguration => ({
    command: `uv run python -m eng.scripts.stage ${library}`,
    cache: false,
    parallelism: false,
    dependsOn: [{ projects: ['eng'], target: 'provision' }],
    outputs: [`{workspaceRoot}/${_ARTIFACTS_ROOT}/${library}/stage`],
    metadata: { description: `Stage the ${library} files for a runtime identifier`, technologies: ['python', 'vcpkg'] },
});

const _packTarget = (project: NativePackaging.Project, native: NativePackaging.Project, source: string, parent: string): TargetConfiguration => ({
    command: `dotnet pack ${project.root} --configuration Release --output ${source} --nologo`,
    cache: true,
    dependsOn: [{ projects: [native.name], target: 'stage' }],
    inputs: [
        '{projectRoot}/**/*',
        `{workspaceRoot}/${parent}/Directory.Build.props`,
        `{workspaceRoot}/${parent}/Directory.Build.targets`,
        `{workspaceRoot}/${parent}/Directory.Packages.props`,
        `{workspaceRoot}/${parent}/_._`,
        `{workspaceRoot}/${parent}/${project.library}/**/*`,
        '{workspaceRoot}/global.json',
        { dependentTasksOutputFiles: '**/*' },
    ],
    outputs: [
        `{workspaceRoot}/${source}/${project.name}.${project.version}.nupkg`,
        `{workspaceRoot}/${_ARTIFACTS_ROOT}/msbuild/bin/${project.name}`,
        `{workspaceRoot}/${_ARTIFACTS_ROOT}/msbuild/obj/${project.name}`,
    ],
    metadata: { description: `Pack ${project.name} ${project.version} into ${source}`, technologies: ['dotnet', 'nuget'] },
});

const _configuration = (project: NativePackaging.Project, native: NativePackaging.Project, source: string, parent: string): ProjectConfiguration => ({
    name: project.name,
    root: project.root,
    projectType: 'library',
    tags: [_TAG],
    implicitDependencies: project.managed ? [native.name] : [],
    targets: project.managed
        ? { pack: _packTarget(project, native, source, parent) }
        : { pack: _packTarget(project, native, source, parent), stage: _stageTarget(project.library) },
    metadata: { technologies: ['dotnet', 'nuget'] },
});

const _localSource = (workspaceRoot: string): Effect.Effect<string, NativePackagingError, NativePackaging.Platform> =>
    Effect.flatMap(_readNuGetConfig(workspaceRoot, _NUGET_CONFIG), (config) =>
        Option.match(
            Array.findFirst(config.configuration.packageSources.add, (entry) => entry.key === _LOCAL_SOURCE_KEY),
            {
                onNone: () => Effect.fail(new NativePackagingError({ reason: 'source', file: _NUGET_CONFIG, detail: _LOCAL_SOURCE_KEY })),
                onSome: (entry) => Effect.succeed(entry.value),
            },
        ),
    );

const _projectNode = (
    workspaceRoot: string,
    source: string,
    configFiles: ReadonlyArray<string>,
    file: string,
): Effect.Effect<CreateNodesResult, NativePackagingError, NativePackaging.Platform> =>
    Effect.gen(function* () {
        const path = yield* Path.Path;
        const project = yield* Effect.flatMap(_readProject(workspaceRoot, file), (read) => _withManifestDirectory(workspaceRoot, read));
        const native = yield* _nativeProject(workspaceRoot, project, configFiles);
        return { projects: { [project.root]: _configuration(project, native, source, path.dirname(project.root)) } };
    });

// Every PackageReference to a packaging project becomes a static edge, a changed package then marks its consumers affected
const _packageReferenceEdges = (
    context: CreateDependenciesContext,
): Effect.Effect<RawProjectGraphDependency[], NativePackagingError, NativePackaging.Platform> =>
    Effect.gen(function* () {
        const packaging = HashSet.fromIterable(
            Array.filterMap(Record.toEntries(context.projects), ([name, project]) =>
                Option.liftPredicate(name, () => Array.contains(project.tags ?? [], _TAG)),
            ),
        );
        // Nx keeps the cached edges of every file outside filesToProcess, only changed project files are read
        const projectFiles = Array.flatMap(Record.toEntries(context.filesToProcess.projectFileMap), ([source, files]) =>
            Array.filterMap(files, (entry) => Option.liftPredicate({ source, file: entry.file }, ({ file }) => String.endsWith('.csproj')(file))),
        );
        const edges = yield* Effect.forEach(
            projectFiles,
            ({ source, file }) =>
                Effect.gen(function* () {
                    const decoded = yield* _readProjectFile(context.workspaceRoot, file);
                    const targets = Array.filter(_packageReferences(decoded), (target) => target !== source && HashSet.has(packaging, target));
                    return yield* Effect.forEach(targets, (target) => {
                        const dependency: RawProjectGraphDependency = { source, target, type: DependencyType.static, sourceFile: file };
                        return Effect.as(
                            Effect.try({ try: () => validateDependency(dependency, context), catch: _fail('dependency', file) }),
                            dependency,
                        );
                    });
                }),
            { concurrency: 'unbounded' },
        );
        return Array.flatten(edges);
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

// One platform runtime serves both entry points, Nx loads the plugin once per worker
const _runtime = ManagedRuntime.make(Layer.merge(NodeFileSystem.layer, NodePath.layer));

const createNodes: CreateNodes = [
    _PROJECT_FILE_GLOB,
    (configFiles, options, context) =>
        _runtime
            .runPromise(_localSource(context.workspaceRoot))
            .then((source) =>
                createNodesFromFiles(
                    (file, _options, perFile) => _runtime.runPromise(_projectNode(perFile.workspaceRoot, source, perFile.configFiles, file)),
                    configFiles,
                    options,
                    context,
                ),
            ),
];

const createDependencies: CreateDependencies = (_options, context) => _runtime.runPromise(_packageReferenceEdges(context));

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
