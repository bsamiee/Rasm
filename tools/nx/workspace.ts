// Nx plugin that infers a project per language manifest or Python package marker and a packaging project per eng/native project file

// --- [IMPORTS] -------------------------------------------------------------------------

import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import {
    type CreateDependencies,
    type CreateDependenciesContext,
    type CreateNodes,
    type CreateNodesResult,
    type CreateNodesResultArray,
    createNodesFromFiles,
    DependencyType,
    type ProjectConfiguration,
    type RawProjectGraphDependency,
    type TargetConfiguration,
    workspaceRoot,
} from '@nx/devkit';
import { Array, Data, Effect, ManagedRuntime, Match, Option, ParseResult, Record, Schema, String } from 'effect';
import { XMLParser } from 'fast-xml-parser';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NATIVE_ROOT = 'eng/native/';
const _ARTIFACTS_ROOT = '.artifacts/native';
const _NATIVE_TAG = 'native';
const _LIBRARY_ROOT = 'libs/';

// --- [ERRORS] --------------------------------------------------------------------------

class WorkspaceError extends Data.TaggedError('WorkspaceError')<{ readonly message: string }> {}

// --- [MODELS] --------------------------------------------------------------------------

const _parser = new XMLParser({
    ignoreAttributes: false,
    attributeNamePrefix: '',
    parseTagValue: false,
    isArray: (name): boolean => Array.contains(['propertyGroup', 'itemGroup', 'packageReference', 'add'], name),
    transformTagName: String.uncapitalize,
    transformAttributeName: String.uncapitalize,
});

const _xml = <A, I>(schema: Schema.Schema<A, I>): Schema.Schema<A, string> =>
    Schema.transformOrFail(Schema.String, schema, {
        strict: false,
        decode: (text, _, ast) => ParseResult.try({ try: () => _parser.parse(text), catch: (cause) => new ParseResult.Type(ast, text, `${cause}`) }),
        encode: (input, _, ast) => ParseResult.fail(new ParseResult.Forbidden(ast, input, 'The plugin reads project files and writes none')),
    });

const _PropertyGroup = Schema.Struct({
    versionManifestFileName: Schema.optional(Schema.String),
    includeBuildOutput: Schema.optional(Schema.String),
    releaseGroup: Schema.optional(Schema.String),
});

const _ItemGroup = Schema.Struct({
    packageReference: Schema.optionalWith(Schema.Array(Schema.Struct({ include: Schema.optional(Schema.String) })), { default: () => [] }),
});

const _ProjectFile = _xml(
    Schema.Struct({
        project: Schema.Struct({
            propertyGroup: Schema.optionalWith(Schema.Array(_PropertyGroup), { default: () => [] }),
            itemGroup: Schema.optionalWith(Schema.Array(_ItemGroup), { default: () => [] }),
        }),
    }),
);

const _NuGetConfig = _xml(
    Schema.Struct({
        configuration: Schema.Struct({
            packageSources: Schema.Struct({
                add: Schema.optionalWith(Schema.Array(Schema.Struct({ key: Schema.String, value: Schema.String })), { default: () => [] }),
            }),
        }),
    }),
);

// Every manifest under eng/native/<library>/ pins the package version as version-string, the packaging project derives Version from it
const _VersionManifest = Schema.parseJson(Schema.Struct({ 'version-string': Schema.String }));

const _PackageJson = Schema.parseJson(Schema.Struct({ private: Schema.optional(Schema.Boolean) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const _read =
    <A>(schema: Schema.Schema<A, string>) =>
    (file: string): Effect.Effect<A, WorkspaceError, FileSystem.FileSystem | Path.Path> =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const text = yield* Effect.mapError(
                fs.readFileString(path.join(workspaceRoot, file)),
                (error) => new WorkspaceError({ message: `Reading ${file} failed, ${error.message}, check that the file exists and is readable` }),
            );
            return yield* Effect.mapError(
                Schema.decode(schema)(text),
                (error) =>
                    new WorkspaceError({
                        message: `Decoding ${file} failed, ${error.message}, repair the file so the named element holds the expected text`,
                    }),
            );
        });

// MSBuild keeps the last declaration of a property, the lookup follows that order
const _property = (project: typeof _ProjectFile.Type, name: keyof typeof _PropertyGroup.Type): Option.Option<string> =>
    Array.findLast(project.project.propertyGroup, (group) => Option.fromNullable(group[name]));

const _library = (name: string): string => String.toLowerCase(Array.lastNonEmpty(String.split(name, '.')));

const _isNative = (file: string): boolean => String.startsWith(_NATIVE_ROOT)(file) && String.endsWith('.csproj')(file);

const _localSource: Effect.Effect<string, WorkspaceError, FileSystem.FileSystem | Path.Path> = _read(_NuGetConfig)('NuGet.config').pipe(
    Effect.flatMap((config) => Array.findFirst(config.configuration.packageSources.add, (entry) => entry.key === 'local')),
    Effect.map((entry) => entry.value),
    Effect.catchTag(
        'NoSuchElementException',
        () =>
            new WorkspaceError({
                message:
                    'NuGet.config declares no package source with key local, the pack target writes the nupkg there, add the source under packageSources',
            }),
    ),
);

// A managed binding packs beside the native packaging project of the same library, a native project stages for itself
const _packagingNode = (
    localSource: Effect.Effect<string, WorkspaceError, FileSystem.FileSystem | Path.Path>,
    files: readonly string[],
    file: string,
): Effect.Effect<CreateNodesResult, WorkspaceError, FileSystem.FileSystem | Path.Path> =>
    Effect.gen(function* () {
        const source = yield* localSource;
        const path = yield* Path.Path;
        const name = path.basename(file, '.csproj');
        const root = path.dirname(file);
        const library = _library(name);
        const project = yield* _read(_ProjectFile)(file);
        const manifest = Option.getOrElse(_property(project, 'versionManifestFileName'), () => 'vcpkg.json');
        const { 'version-string': version } = yield* _read(_VersionManifest)(`${_NATIVE_ROOT}${library}/${manifest}`);
        const managed = Option.contains(_property(project, 'includeBuildOutput'), 'true');
        const sibling = Array.findFirst(
            files,
            (other) => _isNative(other) && other !== file && _library(path.basename(other, '.csproj')) === library,
        );
        const native = yield* Option.liftPredicate(name, () => !managed).pipe(
            Option.orElse(() => Option.map(sibling, (other) => path.basename(other, '.csproj'))),
            Effect.mapError(
                () =>
                    new WorkspaceError({ message: `${file} has no native packaging project for ${library}, add one beside it for the stage target` }),
            ),
        );
        const stage: TargetConfiguration = {
            command: `uv run --only-group eng python -m eng.scripts.stage ${library}`,
            cache: false,
            parallelism: false,
            dependsOn: [{ projects: ['eng'], target: 'provision' }],
            outputs: [`{workspaceRoot}/${_ARTIFACTS_ROOT}/${library}/stage`],
            metadata: { description: `Stage the ${library} files for a runtime identifier`, technologies: ['python', 'vcpkg'] },
        };
        const pack: TargetConfiguration = {
            command: `dotnet pack ${root} --configuration Release --output ${source} --nologo`,
            cache: true,
            dependsOn: [{ projects: [native], target: 'stage' }],
            inputs: [
                '{projectRoot}/**/*',
                `{workspaceRoot}/${_NATIVE_ROOT}{Directory.Build.props,Directory.Build.targets,Directory.Packages.props,_._}`,
                `{workspaceRoot}/${_NATIVE_ROOT}${library}/**/*`,
                '{workspaceRoot}/global.json',
                { dependentTasksOutputFiles: '**/*' },
            ],
            outputs: [`{workspaceRoot}/${source}/${name}.${version}.nupkg`, `{workspaceRoot}/${_ARTIFACTS_ROOT}/msbuild/{bin,obj}/${name}`],
            metadata: { description: `Pack ${name} ${version} into ${source}`, technologies: ['dotnet', 'nuget'] },
        };
        const configuration: ProjectConfiguration = {
            name,
            root,
            projectType: 'library',
            tags: [_NATIVE_TAG],
            implicitDependencies: Option.toArray(Option.liftPredicate(native, () => managed)),
            targets: { pack, ...Record.getSomes({ stage: Option.liftPredicate(stage, () => !managed) }) },
            metadata: { technologies: ['dotnet', 'nuget'] },
        };
        return { projects: { [root]: configuration } };
    });

// A .NET library names its release group in the project file, every other library joins the group of its language
const _releaseGroup = (root: string, file: string): Effect.Effect<Option.Option<string>, WorkspaceError, FileSystem.FileSystem | Path.Path> =>
    Match.value(file).pipe(
        Match.when(String.endsWith('.csproj'), () =>
            Effect.map(_read(_ProjectFile)(file), (project) =>
                Option.some(`release:${Option.getOrElse(_property(project, 'releaseGroup'), () => 'dotnet')}`),
            ),
        ),
        Match.when(String.endsWith('tsconfig.json'), () =>
            Effect.map(_read(_PackageJson)(`${root}/package.json`), (manifest) =>
                Option.liftPredicate('release:typescript', () => manifest.private !== true),
            ),
        ),
        Match.orElse(() => Effect.succeedSome('release:python')),
    );

const _languageNode = (
    file: string,
    language: 'dotnet' | 'python' | 'typescript',
): Effect.Effect<CreateNodesResult, WorkspaceError, FileSystem.FileSystem | Path.Path> =>
    Effect.gen(function* () {
        const path = yield* Path.Path;
        const root = path.dirname(file);
        const release = yield* Effect.if(String.startsWith(_LIBRARY_ROOT)(root), {
            onTrue: () => _releaseGroup(root, file),
            onFalse: () => Effect.succeedNone,
        });
        const configuration: ProjectConfiguration = {
            root,
            ...Record.getSomes({ name: Option.liftPredicate(path.basename(root), () => language === 'python') }),
            tags: [`language:${language}`, ...Option.toArray(release)],
            targets: { lint: {}, format: {}, typecheck: {}, check: {}, ...Record.getSomes({ 'nx-release-publish': Option.as(release, {}) }) },
        };
        return { projects: { [root]: configuration } };
    });

const _node = (
    localSource: Effect.Effect<string, WorkspaceError, FileSystem.FileSystem | Path.Path>,
    files: readonly string[],
    file: string,
): Effect.Effect<CreateNodesResult, WorkspaceError, FileSystem.FileSystem | Path.Path> =>
    Match.value(file).pipe(
        Match.when(_isNative, () => _packagingNode(localSource, files, file)),
        Match.when(String.endsWith('.csproj'), () => _languageNode(file, 'dotnet')),
        Match.when(String.endsWith('tsconfig.json'), () => _languageNode(file, 'typescript')),
        Match.orElse(() => _languageNode(file, 'python')),
    );

// Nx keeps the cached edges of every file outside filesToProcess and validates each edge as the graph builder adds it
const _packageReferenceEdges = (
    context: CreateDependenciesContext,
): Effect.Effect<RawProjectGraphDependency[], WorkspaceError, FileSystem.FileSystem | Path.Path> =>
    Effect.gen(function* () {
        const packaging = Record.filter(context.projects, (project) => Array.contains(project.tags ?? [], _NATIVE_TAG));
        const changed = Array.flatMap(Record.toEntries(context.filesToProcess.projectFileMap), ([source, files]) =>
            Array.filterMap(files, ({ file }) => Option.liftPredicate({ source, file }, () => String.endsWith('.csproj')(file))),
        );
        const edges = yield* Effect.forEach(
            changed,
            ({ source, file }) =>
                _read(_ProjectFile)(file).pipe(
                    Effect.map((project) => Array.flatMap(project.project.itemGroup, (group) => group.packageReference)),
                    Effect.map(
                        Array.filterMap(({ include }) =>
                            Option.filter(Option.fromNullable(include), (target) => target !== source && Record.has(packaging, target)),
                        ),
                    ),
                    Effect.map(Array.map((target): RawProjectGraphDependency => ({ source, target, type: DependencyType.static, sourceFile: file }))),
                ),
            { concurrency: 'unbounded' },
        );
        return Array.flatten(edges);
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const _runtime = ManagedRuntime.make(NodeContext.layer);

// A TypeScript project is a workspace package with a tsconfig.json, a Python package is a directory one level under libs/python or an application that holds __init__.py
const createNodes: CreateNodes = [
    '{**/*.csproj,{apps,libs,tests}/**/tsconfig.json,{libs/python,apps/*}/*/__init__.py}',
    (files, options, context): Promise<CreateNodesResultArray> => {
        // The first packaging node reads NuGet.config and every later one shares the value, the devkit collects each node's failure per file
        const localSource = _runtime.runSync(Effect.cached(_localSource));
        return createNodesFromFiles((file) => _runtime.runPromise(_node(localSource, files, file)), files, options, context);
    },
];

const createDependencies: CreateDependencies = (_options, context) => _runtime.runPromise(_packageReferenceEdges(context));

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
