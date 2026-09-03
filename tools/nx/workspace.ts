// Nx plugin that tags every language manifest with its empty check targets and infers a packaging project per eng/native project file

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
import { Array, Data, Effect, HashSet, Inspectable, Layer, ManagedRuntime, Match, Option, Record, Schema, String } from 'effect';
import { XMLParser } from 'fast-xml-parser';

// --- [TYPES] ---------------------------------------------------------------------------

declare namespace Workspace {
    type Platform = FileSystem.FileSystem | Path.Path;
    type Reason = 'unreadable' | 'malformed' | 'version' | 'manifest' | 'native' | 'source' | 'dependency';
    type Language = 'dotnet' | 'python' | 'typescript';
    type Manifest =
        | { readonly kind: 'native'; readonly file: string }
        | { readonly kind: 'language'; readonly root: string; readonly language: Language }
        | { readonly kind: 'root' };
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

const _MANIFEST_GLOB = '**/{*.csproj,package.json,pyproject.toml}';
const _NATIVE_ROOT = 'eng/native/';
const _ARTIFACTS_ROOT = '.artifacts/native';
const _NATIVE_TAG = 'native';
const _NUGET_CONFIG = 'NuGet.config';
const _LOCAL_SOURCE_KEY = 'local'; // The local source in NuGet.config owns the pack output path, restore reads the nupkg from there
const _TYPESCRIPT_PROJECT_FILE = 'tsconfig.json'; // A package.json is a TypeScript project when the file its typecheck target builds sits beside it
const _REPEATED_ELEMENTS: ReadonlyArray<string> = ['PropertyGroup', 'ItemGroup', 'PackageReference', 'add'];
// The targetDefaults entries filtered by language tag fill these empty targets, a default creates no target on its own
const _EMPTY_TARGETS: Record<'lint' | 'format' | 'typecheck' | 'check', TargetConfiguration> = { lint: {}, format: {}, typecheck: {}, check: {} };

// --- [ERRORS] --------------------------------------------------------------------------

// Nx prints the message of every rejected file, each message states what happened, the cause, then the action
const _MESSAGES: Record<Workspace.Reason, (file: string, detail: string) => string> = {
    unreadable: (file, detail) => `Reading ${file} failed, ${detail}, check that the file exists and is readable`,
    malformed: (file, detail) => `Decoding ${file} failed, ${detail}, repair the XML so the named element holds the expected text`,
    version: (file) =>
        `${file} declares no Version property, the pack target names the nupkg after the package version, add a Version property to the project`,
    manifest: (file, detail) =>
        `${file} has no manifest directory at ${detail}, the stage target reads the library version there, add the directory with its manifest`,
    native: (file, detail) =>
        `${file} has no native packaging project for ${detail}, pack depends on its stage target, add the native packaging project beside it`,
    source: (file, detail) =>
        `${file} declares no package source with key ${detail}, the pack target writes the nupkg there, add the source under packageSources`,
    dependency: (file, detail) => `${file} declares a PackageReference edge Nx rejects, ${detail}, check the project names in the graph`,
};

class WorkspaceError extends Data.TaggedError('WorkspaceError')<{
    readonly reason: Workspace.Reason;
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
    (reason: Workspace.Reason, file: string) =>
    (cause: unknown): WorkspaceError =>
        new WorkspaceError({ reason, file, detail: cause instanceof Error ? cause.message : Inspectable.toStringUnknown(cause) });

const _decodeXml =
    <A, I>(schema: Schema.Schema<A, I>) =>
    (file: string, text: string): Effect.Effect<A, WorkspaceError> =>
        Effect.flatMap(Effect.try({ try: (): unknown => _parser.parse(text, true), catch: _fail('malformed', file) }), (parsed) =>
            Effect.mapError(Schema.decodeUnknown(schema)(parsed), _fail('malformed', file)),
        );

const _readXml =
    <A, I>(schema: Schema.Schema<A, I>) =>
    (workspaceRoot: string, file: string): Effect.Effect<A, WorkspaceError, Workspace.Platform> =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const text = yield* Effect.mapError(fs.readFileString(path.join(workspaceRoot, file)), _fail('unreadable', file));
            return yield* _decodeXml(schema)(file, text);
        });

const _readProjectFile = _readXml(_ProjectFile);
const _readNuGetConfig = _readXml(_NuGetConfig);

const _exists = (workspaceRoot: string, file: string): Effect.Effect<boolean, WorkspaceError, Workspace.Platform> =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        return yield* Effect.mapError(fs.exists(path.join(workspaceRoot, file)), _fail('unreadable', file));
    });

// MSBuild keeps the last declaration of a property, the lookup follows that order
const _property = (project: typeof _ProjectFile.Type, name: 'Version' | 'IncludeBuildOutput'): Option.Option<string> =>
    Array.last(Array.filterMap(project.Project.PropertyGroup, (group) => Option.fromNullable(group[name])));

const _packageReferences = (project: typeof _ProjectFile.Type): ReadonlyArray<string> =>
    Array.flatMap(project.Project.ItemGroup, (group) =>
        Array.filterMap(group.PackageReference, (reference) => Option.fromNullable(reference.Include)),
    );

// The library name is the last segment of the project name in lower case, its version manifest directory sits beside the project
const _library = (name: string): string => String.toLowerCase(Array.lastNonEmpty(String.split(name, '.')));

const _isNative = (file: string): boolean => String.startsWith(_NATIVE_ROOT)(file) && String.endsWith('.csproj')(file);

// eng/native project files are packaging projects, other manifests name a language, and root manifests belong to the root package.json nx field
const _manifest = (path: Path.Path, file: string): Workspace.Manifest => {
    const root = path.dirname(file);
    const base = path.basename(file);
    return _isNative(file)
        ? { kind: 'native', file }
        : String.endsWith('.csproj')(base)
          ? { kind: 'language', root, language: 'dotnet' }
          : root === '.'
            ? { kind: 'root' }
            : { kind: 'language', root, language: base === 'package.json' ? 'typescript' : 'python' };
};

const _project = (file: string, root: string, name: string, decoded: typeof _ProjectFile.Type): Effect.Effect<Workspace.Project, WorkspaceError> =>
    Option.match(_property(decoded, 'Version'), {
        onNone: () => Effect.fail(new WorkspaceError({ reason: 'version', file, detail: '' })),
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

const _readProject = (workspaceRoot: string, file: string): Effect.Effect<Workspace.Project, WorkspaceError, Workspace.Platform> =>
    Effect.gen(function* () {
        const path = yield* Path.Path;
        const decoded = yield* _readProjectFile(workspaceRoot, file);
        return yield* _project(file, path.dirname(file), path.basename(file, '.csproj'), decoded);
    });

const _withManifestDirectory = (
    workspaceRoot: string,
    project: Workspace.Project,
): Effect.Effect<Workspace.Project, WorkspaceError, Workspace.Platform> =>
    Effect.gen(function* () {
        const path = yield* Path.Path;
        const directory = path.join(path.dirname(project.root), project.library);
        const present = yield* _exists(workspaceRoot, directory);
        return yield* present
            ? Effect.succeed(project)
            : Effect.fail(new WorkspaceError({ reason: 'manifest', file: project.file, detail: directory }));
    });

// Managed bindings pack after the native packaging project of the same library stages its files
const _nativeProject = (
    workspaceRoot: string,
    project: Workspace.Project,
    nativeFiles: ReadonlyArray<string>,
): Effect.Effect<Workspace.Project, WorkspaceError, Workspace.Platform> =>
    project.managed
        ? Effect.gen(function* () {
              const path = yield* Path.Path;
              const siblings = Array.filter(
                  nativeFiles,
                  (file) => file !== project.file && _library(path.basename(file, '.csproj')) === project.library,
              );
              const candidates = yield* Effect.forEach(siblings, (file) => _readProject(workspaceRoot, file));
              return yield* Option.match(
                  Array.findFirst(candidates, (candidate) => !candidate.managed),
                  {
                      onNone: () => Effect.fail(new WorkspaceError({ reason: 'native', file: project.file, detail: project.library })),
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

const _packTarget = (project: Workspace.Project, native: Workspace.Project, source: string, parent: string): TargetConfiguration => ({
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

const _packagingConfiguration = (project: Workspace.Project, native: Workspace.Project, source: string, parent: string): ProjectConfiguration => ({
    name: project.name,
    root: project.root,
    projectType: 'library',
    tags: [_NATIVE_TAG],
    implicitDependencies: project.managed ? [native.name] : [],
    targets: project.managed
        ? { pack: _packTarget(project, native, source, parent) }
        : { pack: _packTarget(project, native, source, parent), stage: _stageTarget(project.library) },
    metadata: { technologies: ['dotnet', 'nuget'] },
});

// The plugin that reads the manifest names the project, a Python project has no such plugin and takes its root path as its name
const _languageConfiguration = (root: string, language: Workspace.Language): ProjectConfiguration => ({
    root,
    ...(language === 'python' ? { name: Array.join(String.split(root, '/'), '-') } : {}),
    tags: [`language:${language}`],
    targets: { ..._EMPTY_TARGETS },
});

const _localSource = (workspaceRoot: string): Effect.Effect<string, WorkspaceError, Workspace.Platform> =>
    Effect.flatMap(_readNuGetConfig(workspaceRoot, _NUGET_CONFIG), (config) =>
        Option.match(
            Array.findFirst(config.configuration.packageSources.add, (entry) => entry.key === _LOCAL_SOURCE_KEY),
            {
                onNone: () => Effect.fail(new WorkspaceError({ reason: 'source', file: _NUGET_CONFIG, detail: _LOCAL_SOURCE_KEY })),
                onSome: (entry) => Effect.succeed(entry.value),
            },
        ),
    );

const _packagingNode = (
    workspaceRoot: string,
    source: string,
    nativeFiles: ReadonlyArray<string>,
    file: string,
): Effect.Effect<CreateNodesResult, WorkspaceError, Workspace.Platform> =>
    Effect.gen(function* () {
        const path = yield* Path.Path;
        const project = yield* Effect.flatMap(_readProject(workspaceRoot, file), (read) => _withManifestDirectory(workspaceRoot, read));
        const native = yield* _nativeProject(workspaceRoot, project, nativeFiles);
        return { projects: { [project.root]: _packagingConfiguration(project, native, source, path.dirname(project.root)) } };
    });

const _languageNode = (
    workspaceRoot: string,
    root: string,
    language: Workspace.Language,
): Effect.Effect<CreateNodesResult, WorkspaceError, Workspace.Platform> =>
    Effect.map(language === 'typescript' ? _exists(workspaceRoot, `${root}/${_TYPESCRIPT_PROJECT_FILE}`) : Effect.succeed(true), (present) =>
        present ? { projects: { [root]: _languageConfiguration(root, language) } } : { projects: {} },
    );

const _node = (
    workspaceRoot: string,
    source: string,
    nativeFiles: ReadonlyArray<string>,
    file: string,
): Effect.Effect<CreateNodesResult, WorkspaceError, Workspace.Platform> =>
    Effect.flatMap(Path.Path, (path) =>
        Match.value(_manifest(path, file)).pipe(
            Match.discriminatorsExhaustive('kind')({
                native: (manifest) => _packagingNode(workspaceRoot, source, nativeFiles, manifest.file),
                language: (manifest) => _languageNode(workspaceRoot, manifest.root, manifest.language),
                root: () => Effect.succeed({ projects: {} }),
            }),
        ),
    );

// Every PackageReference to a packaging project becomes a static edge, a changed package then marks its consumers affected
const _packageReferenceEdges = (context: CreateDependenciesContext): Effect.Effect<RawProjectGraphDependency[], WorkspaceError, Workspace.Platform> =>
    Effect.gen(function* () {
        const packaging = HashSet.fromIterable(
            Array.filterMap(Record.toEntries(context.projects), ([name, project]) =>
                Option.liftPredicate(name, () => Array.contains(project.tags ?? [], _NATIVE_TAG)),
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
    _MANIFEST_GLOB,
    (configFiles, options, context) => {
        const nativeFiles = Array.filter(configFiles, _isNative);
        return _runtime
            .runPromise(_localSource(context.workspaceRoot))
            .then((source) =>
                createNodesFromFiles(
                    (file, _options, perFile) => _runtime.runPromise(_node(perFile.workspaceRoot, source, nativeFiles, file)),
                    configFiles,
                    options,
                    context,
                ),
            );
    },
];

const createDependencies: CreateDependencies = (_options, context) => _runtime.runPromise(_packageReferenceEdges(context));

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
