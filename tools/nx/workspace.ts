import { matchesGlob } from 'node:path';
import { Lang, parse, type SgNode } from '@ast-grep/napi';
import { layerPosix } from '@effect/platform-node/NodePath';
import { layer } from '@effect/platform-node/NodeServices';
import { type CreateDependencies, type CreateNodes, type CreateNodesContext, type CreateNodesResultArray, createNodesFromFiles, DependencyType, type Optional, type ProjectConfiguration, parseJson, type RawProjectGraphDependency, type TargetConfiguration, type Tree } from '@nx/devkit';
import { globWithWorkspaceContext, hashWithWorkspaceContext, PluginCache, readNxJsonFromDisk, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Boolean, Cause, Effect, Equal, FileSystem, Filter, Function, Layer, ManagedRuntime, Option, Path, Predicate, pipe, Record, Result, Schema, Scope, Stream, String, Struct, Tuple } from 'effect';
import { XMLParser } from 'fast-xml-parser';
import { escape } from 'minimatch';
import { packageDirectory } from 'package-directory';
import { quote } from 'shell-quote';
import { serverFS } from 'typescript/fs';
import { API } from 'typescript/sync';
import { parseSync } from 'vite';
import { CST, Parser } from 'yaml';
import { accumulate } from '#/eng/typescript/accumulate';
import { decodeFile, Mise, Package } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';
import { TextFormat } from '#/eng/typescript/text-format';

// --- [TYPES] ---------------------------------------------------------------------------

type Workspace = Effect.Success<ReturnType<typeof openWorkspace>>;
interface Application {
    readonly root: string;
    readonly env: Readonly<Record<string, string>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CARGO_MANIFEST = 'Cargo.toml';
const SWIFT_MANIFEST = 'Package.swift';
const XCODE_PROJECT = 'project.xcproj';
const PYPROJECT = 'pyproject.toml';
const CLOUDFLARE_CONFIG = 'cloudflare.config.ts';
const TSCONFIG = 'tsconfig.json';
const NEUTRAL_TSCONFIG = 'tsconfig.neutral.json';
const BARREL = 'index.ts';
const TYPE_DECLARATIONS = ['interface_declaration', 'type_alias_declaration'];
const TESTS = 'tests';
const APPS = 'apps';
const CONTAINERS = /^(?:testcontainers|@testcontainers\/.+)$/u;
const DOTNET = '{workspaceRoot}/.artifacts/dotnet';
const VERSIONS = new Intl.Collator('en', { numeric: true });
const DEPLOYMENT_TARGET = /(?<=^MACOSX_DEPLOYMENT_TARGET = ).+$/mu;

// --- [MODELS] --------------------------------------------------------------------------

const Jsonc = TextFormat('JSON', parseJson);
const Release = Schema.String.check(Schema.isPattern(/^\d+\.\d+$/u));
const PythonRuntime = Schema.fromJsonString(Schema.Struct({ versions: Schema.NonEmptyArray(Schema.String) }).pipe(Schema.encodeKeys({ versions: 'active_versions' })));
const JsonListRecord = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.String)));
const Sgconfig = TextFormat.Yaml.pipe(Schema.decodeTo(Schema.Struct({ ruleDirs: Schema.Array(Schema.String) })));
const Rule = TextFormat.Yaml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            id: Schema.String,
            metadata: Schema.Struct({ below: Schema.OptionFromOptionalKey(Release), from: Schema.OptionFromOptionalKey(Release) }).pipe(Schema.encodeKeys({ below: 'python-below', from: 'python-from' }), Schema.withDecodingDefaultKey(Effect.succeed({}))),
        }),
    ),
);
const PythonRequirement = Schema.Struct({ minimum: Schema.TemplateLiteralParser(['>=', Schema.Trim.pipe(Schema.decodeTo(Release))]) }).pipe(Schema.encodeKeys({ minimum: 'requires-python' }));
const Host = TextFormat.Toml.pipe(Schema.decodeTo(Schema.Struct({ project: PythonRequirement })));
const Root = TextFormat.Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            tool: Schema.Struct({
                uv: Schema.Struct({ workspace: Schema.Struct({ members: Schema.Array(Schema.String) }), requirements: Schema.Record(Schema.String, PythonRequirement) }).pipe(Schema.encodeKeys({ requirements: 'dependency-groups' })),
                pytest: Schema.Struct({ pythonFiles: Schema.Array(Schema.String), testPaths: Schema.Array(Schema.String) }).pipe(Schema.encodeKeys({ pythonFiles: 'python_files', testPaths: 'testpaths' })),
                coverage: Schema.Struct({ run: Schema.Struct({ dataFile: Schema.String }).pipe(Schema.encodeKeys({ dataFile: 'data_file' })) }),
            }),
        }),
    ),
);
const Member = TextFormat.Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            project: Schema.Struct({ name: Schema.String, scripts: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) }),
            tool: Schema.Struct({
                hatch: Schema.Struct({
                    build: Schema.Struct({
                        devModeDirs: Schema.Array(Schema.String),
                        targets: Schema.optionalKey(Schema.Struct({ wheel: Schema.optionalKey(Schema.Struct({ sources: Schema.optionalKey(Schema.Union([Schema.Record(Schema.String, Schema.String), Schema.Array(Schema.String)])) })) })),
                    }).pipe(Schema.encodeKeys({ devModeDirs: 'dev-mode-dirs' })),
                }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({ build: { 'dev-mode-dirs': [] } }))),
                deptry: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.Unknown)),
                fawltydeps: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.Unknown)),
            }),
        }),
    ),
);
const Manifest = TextFormat.Toml.pipe(Schema.decodeTo(Schema.Struct({ id: Schema.String })));
const PylockExport = TextFormat.Toml.pipe(Schema.decodeTo(Schema.Struct({ packages: Schema.Array(Schema.Struct({ directory: Schema.OptionFromOptionalKey(Schema.Struct({ path: Schema.String })) })) })));
const Metadata = Schema.fromJsonString(
    Schema.Struct({
        packages: Schema.Array(
            Schema.Struct({
                name: Schema.String,
                manifest: Schema.String,
                targets: Schema.Array(Schema.Struct({ name: Schema.String, kind: Schema.NonEmptyArray(Schema.String), types: Schema.Array(Schema.String), source: Schema.String, test: Schema.Boolean }).pipe(Schema.encodeKeys({ types: 'crate_types', source: 'src_path' }))),
                dependencies: Schema.Array(Schema.Struct({ name: Schema.String, path: Schema.OptionFromOptionalKey(Schema.String) })),
                metadata: Schema.OptionFromNullOr(Schema.Struct({ nx: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.Unknown)) })),
            }).pipe(Schema.encodeKeys({ manifest: 'manifest_path' })),
        ),
    }),
);
const SwiftPackage = Schema.fromJsonString(
    Schema.Struct({
        targets: Schema.Array(
            Schema.Struct({ name: Schema.String, c99name: Schema.String, type: Schema.Literals(['executable', 'library', 'test', 'macro', 'plugin', 'snippet', 'system-target', 'binary']), path: Schema.String, locals: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }).pipe(
                Schema.encodeKeys({ locals: 'target_dependencies' }),
            ),
        ),
        products: Schema.Array(Schema.Struct({ name: Schema.String, targets: Schema.Array(Schema.String) })),
        platforms: Schema.Array(Schema.Struct({ name: Schema.String, version: Schema.String })),
    }),
);
const XcodeProject = Jsonc.pipe(
    Schema.decodeTo(
        Schema.Struct({
            files: Schema.Array(Schema.Struct({ path: Schema.OptionFromOptionalKey(Schema.String) })),
            targets: Schema.Array(
                Schema.Struct({
                    name: Schema.String,
                    members: Schema.Array(Schema.Struct({ product: Schema.String }).pipe(Schema.encodeKeys({ product: 'product-name' }))).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
                    settings: Schema.Struct({ PRODUCT_BUNDLE_IDENTIFIER: Schema.OptionFromOptionalKey(Schema.String) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
                }).pipe(Schema.encodeKeys({ members: 'package-product-members', settings: 'build-settings' })),
            ),
        }),
    ),
);
const NxReleaseConfiguration = Schema.Struct({ releaseTag: Schema.Struct({ pattern: Schema.String }), groups: Schema.Record(Schema.String, Schema.Struct({ projects: Schema.ArrayEnsure(Schema.String) })) });
const NeutralConfig = Schema.fromJsonString(Schema.Struct({ extends: Schema.String, compilerOptions: Schema.Struct({ types: Schema.Array(Schema.String) }), files: Schema.Array(Schema.String), include: Schema.Array(Schema.String) }));
const Hooks = Jsonc.pipe(Schema.decodeTo(Schema.Struct({ modules: Schema.NonEmptyArray(Schema.String) })));
const MSBuildProject = TextFormat('MSBuild XML', (text) => new XMLParser({ isArray: (name) => name === 'PropertyGroup' }).parse(text)).pipe(
    Schema.decodeTo(Schema.Struct({ Project: Schema.Struct({ PropertyGroup: Schema.Array(Schema.Struct({ TargetExt: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }) })),
);

// --- [ERRORS] --------------------------------------------------------------------------

class InferenceError extends Schema.TaggedError<InferenceError, { readonly brand: unique symbol }>()('InferenceError', {
    reason: Schema.TaggedUnion({ Missing: { value: Schema.String }, Shadowed: { folder: Schema.String } }),
    subject: Schema.String,
}) {
    static readonly missing = (subject: string, value: string) => InferenceError.make({ reason: { _tag: 'Missing', value }, subject });

    override get message(): string {
        return InferenceError.fields.reason.match(this.reason, {
            Missing: ({ value }) => `${this.subject} has no ${value}`,
            Shadowed: ({ folder }) => `${this.subject} shares its name with folder ${folder}, move it into the folder`,
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [READS]
const glob = (root: string, patterns: string[], exclude?: string[]) => Effect.tryPromise(() => globWithWorkspaceContext(root, patterns, exclude));
const owning = Effect.fnUntraced(function* (root: string, dir: string) {
    const path = yield* Path.Path;
    const directory = yield* Effect.flatMap(
        Effect.tryPromise(() => packageDirectory({ cwd: path.join(root, dir) })),
        Effect.fromNullishOr,
    );
    const owner = path.normalize(path.relative(root, directory));
    return { owner, manifest: yield* Package.read(directory) };
});

// --- [WORKSPACE]
const readPyproject = Effect.fnUntraced(function* (root: string, { packages }: typeof PylockExport.Type) {
    const [path, { tool }] = yield* Effect.all([Path.Path, decodeFile(Root, root, PYPROJECT)]);
    const directories = Array.getSomes(packages.map(Struct.get('directory'))).map(Struct.get('path'));
    const members = yield* Effect.forEach(Array.union(tool.uv.workspace.members, directories), (member) => Effect.map(decodeFile(Member, root, path.join(member, PYPROJECT)), Struct.assign({ member })));
    const providers = Array.bind(members, 'source', ({ tool: { hatch } }) => {
        const sources = hatch.build.targets?.wheel?.sources;
        return Array.isArray(sources) ? [] : Object.values(sources ?? {});
    })
        .filter(Predicate.Struct({ source: String.isNonEmpty }))
        .map(({ project, source }) => {
            const separator = source.indexOf('/');
            return { distribution: project.name, importRoot: separator < 0 ? source : source.slice(0, separator) };
        });
    const mapping = Record.map(Array.groupBy(providers, Struct.get('distribution')), Function.flow(Array.map(Struct.get('importRoot')), Array.dedupe));
    return {
        members: Record.fromIterableWith(members, ({ member, project }) => [member, project.name]),
        moduleRoots: Array.dedupe(Array.bind(members, 'folder', ({ tool: { hatch } }) => hatch.build.devModeDirs).map(({ member, folder }) => path.join(member, folder))),
        commands: members.filter(({ project }) => !Record.isEmptyRecord(project.scripts)).map(Struct.get('member')),
        deptry: members.filter(({ tool: { deptry } }) => Option.isSome(deptry)),
        fawltydeps: {
            members: members.filter(({ tool: { fawltydeps } }) => Option.isSome(fawltydeps)),
            mapping: yield* Schema.encodeEffect(JsonListRecord)(mapping).pipe(Effect.orDie),
        },
        requirements: tool.uv.requirements,
        dataFile: tool.coverage.run.dataFile,
        ...tool.pytest,
    };
});
const openWorkspace = Effect.fnUntraced(function* ({ workspaceRoot: root, nxJsonConfiguration }: CreateNodesContext) {
    const [fs, path, scope] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Effect.scope]);
    const configReads = new Set<string>();
    const cache = yield* Effect.acquireRelease(
        Effect.sync(() => new PluginCache<Record<'hash' | 'output', string>>(path.join(workspaceDataDirectory, 'workspace.hash'))),
        (held) => Effect.sync(held.writeToDisk.bind(held)),
    );
    const cached = <A>(schema: Schema.Codec<A, string>, inputs: string[], [command, ...args]: readonly [string, ...string[]]) => {
        const key = [command, ...args].join(' ');
        return Effect.tryPromise(() => hashWithWorkspaceContext(root, inputs)).pipe(
            Effect.bindTo('hash'),
            Effect.bind('text', ({ hash }) => Option.fromUndefinedOr(cache.get(key)).pipe(Option.filter(Predicate.Struct({ hash: Equal.equals(hash) })), Option.map(Struct.get('output')), Option.match({ onSome: Effect.succeed, onNone: () => runCommand(command, args, { cwd: root }) }))),
            Effect.bind('value', Function.flow(Struct.get('text'), Schema.decodeEffect(schema))),
            Effect.tap(({ hash, text }) => Effect.sync(cache.set.bind(cache, key, { hash, output: text }))),
            Effect.map(Struct.get('value')),
        );
    };
    const exported = (project: string) => cached(PylockExport, ['uv.lock', `**/${PYPROJECT}`], ['uv', 'export', '--frozen', '--format', 'pylock.toml', '--no-hashes', '--no-emit-project', '--project', project]);
    const crates = yield* Effect.cached(Effect.map(cached(Metadata, [`**/${CARGO_MANIFEST}`, '**/*.rs'], ['cargo', 'metadata', '--format-version', '1', '--no-deps']), Function.flow(Struct.get('packages'), Array.map(Struct.evolve({ manifest: path.relative.bind(null, root) })))));
    const patterns = ['pnpm-lock.yaml', 'tools/*/examples/**'];
    return {
        root,
        exported,
        crates,
        scanExclusions: Effect.map(
            crates,
            Function.flow(
                Array.flatMap(Struct.get('targets')),
                Array.filter(({ kind, test }) => kind.includes('example') || (kind.includes('test') && !test)),
                Array.map(({ source }) => path.relative(root, source)),
                Array.dedupe,
                Array.filter((source) => !patterns.some(matchesGlob.bind(null, source))),
                Array.map((source) => escape(source, { magicalBraces: true })),
                Array.prependAll(patterns),
            ),
        ),
        ...(yield* Effect.all({
            pyproject: Effect.cached(Effect.flatMap(exported('.'), (packages) => readPyproject(root, packages))),
            mise: Effect.cached(Mise.read(root)),
            hosts: Effect.cached(fs.readDirectory(path.join(root, APPS))),
            release: Effect.cached(Schema.decodeUnknownEffect(NxReleaseConfiguration)(nxJsonConfiguration.release)),
            configs: Effect.cached(
                Effect.acquireRelease(
                    Effect.sync(
                        () =>
                            new API({
                                cwd: root,
                                fs: {
                                    directoryExists: serverFS.useOS,
                                    fileExists: serverFS.useOS,
                                    getAccessibleEntries: serverFS.useOS,
                                    readFile: (file) => {
                                        configReads.add(file);
                                        return serverFS.useOS;
                                    },
                                    realpath: serverFS.useOS,
                                    stat: serverFS.useOS,
                                    writeFile: serverFS.useOS,
                                    removeFile: serverFS.useOS,
                                },
                            }),
                    ),
                    (api) => Effect.sync(api.close),
                ).pipe(
                    Scope.provide(scope),
                    Effect.map((api) => (file: string) => {
                        configReads.clear();
                        const config = api.parseConfigFile(path.join(root, file));
                        return { config, files: [...configReads].map(path.relative.bind(null, root)) };
                    }),
                ),
            ),
            python: Effect.cached(cached(PythonRuntime, [Mise.file], ['mise', 'tool', 'python', '--json'])),
            swift: Effect.cached(
                Effect.map(cached(SwiftPackage, [SWIFT_MANIFEST], ['swift', 'package', 'describe', '--type', 'json']), (described) => ({
                    ...described,
                    tests: Array.partition(described.targets, Filter.fromPredicate<(typeof described.targets)[number]>(Predicate.Struct({ type: Equal.equals('test') }))),
                })),
            ),
        })),
    };
});

// --- [BARRELS]
const rootedLibrary = ({ sideEffects, bin, exports }: typeof Package.schema.Type) => Option.contains(sideEffects, false) && Option.isNone(bin) && Record.has(exports, '.');
const readManifest = (tree: Tree, file: string, dir: string) =>
    Effect.fromNullishOr(tree.read(file, 'utf-8')).pipe(
        Effect.flatMap(Schema.decodeEffect(Package.schema)),
        Effect.map((manifest) => ({ dir, manifest })),
    );
const typed = (node: SgNode) => node.children().some((child) => child.kind() === 'type');
const exported = (type: boolean) => (name: SgNode) => ({ name: name.text(), type });
const specifiedNames = (statement: SgNode) => Array.getSomes(statement.findAll({ rule: { kind: 'export_specifier' } }).map((specifier) => Option.map(Option.fromNullOr(specifier.field('alias') ?? specifier.field('name')), exported(typed(statement) || typed(specifier)))));
const declaredNames = (declaration: SgNode) =>
    Array.getSomes((['lexical_declaration', 'variable_declaration'].some((kind) => declaration.is(kind)) ? declaration.children().filter((child) => child.kind() === 'variable_declarator') : [declaration]).map((node) => Option.fromNullOr(node.field('name')))).map(
        exported(TYPE_DECLARATIONS.some((kind) => declaration.is(kind))),
    );
const exportedNames = (file: string, text: string) =>
    parse(file.endsWith('.tsx') ? Lang.Tsx : Lang.TypeScript, text)
        .root()
        .children()
        .filter((node) => node.kind() === 'export_statement' && node.field('source') === null)
        .flatMap((statement) => [...specifiedNames(statement), ...Option.match(Option.fromNullOr(statement.field('declaration')), { onNone: Array.empty, onSome: declaredNames })])
        .filter(({ name }) => name !== 'default');
const selfImport = (imports: Readonly<Record<string, string>>, module: string) => Array.findFirst(Record.toEntries(imports), ([key, target]) => Option.map(Option.fromUndefinedOr(new RegExp(`^${target.split('*').map(RegExp.escape).join('(.+)')}$`, 'u').exec(`./${module}`)?.[1]), (stem) => key.replace('*', stem)));
const exportLine = (imports: Readonly<Record<string, string>>, relative: string, file: string, text: string) =>
    Array.match(exportedNames(file, text), {
        onEmpty: Array.empty,
        onNonEmpty: (names) => [`export { ${names.map(({ name, type }) => (type ? `type ${name}` : name)).join(', ')} } from '${Option.getOrElse(selfImport(imports, file), Function.constant(relative))}';`],
    });
const ownerFolder = (folders: readonly string[], separator: string) => (file: string) =>
    Option.getOrElse(
        Array.findFirst(folders, (folder) => folder === '.' || file.startsWith(`${folder}${separator}`)),
        Function.constant(''),
    );
const barrelOutput = Effect.fnUntraced(function* (tree: Tree, dir: string, imports: Readonly<Record<string, string>>, folder: string, files: readonly string[]) {
    const path = yield* Path.Path;
    const texts = yield* Effect.forEach(files, (file) => Effect.fromNullishOr(tree.read(path.join(dir, file), 'utf-8')));
    const lines = Array.zipWith(files, texts, (file, text) => exportLine(imports, `./${path.relative(folder, file)}`, file, text));
    return { file: path.join(dir, folder, BARREL), text: ['// --- [EXPORTS] -------------------------------------------------------------------------', '', ...lines.flat(), ''].join('\n') };
});
const neutralOutput = Effect.fnUntraced(function* (dir: string, entry: string) {
    const path = yield* Path.Path;
    const text = yield* Effect.orDie(Schema.encodeEffect(NeutralConfig)({ extends: `./${TSCONFIG}`, compilerOptions: { types: [] }, files: [path.normalize(entry)], include: [] }));
    return { file: path.join(dir, NEUTRAL_TSCONFIG), text };
});
const libraryOutputs = Effect.fnUntraced(function* (tree: Tree, dir: string, manifest: typeof Package.schema.Type, nested: readonly string[]) {
    const path = yield* Path.Path;
    const modules = (yield* glob(
        tree.root,
        [`${dir}/**/*.{ts,tsx}`],
        nested.map((inner) => `${inner}/**`),
    ))
        .map((file) => path.relative(dir, file))
        .filter((file) => !(file.endsWith('.d.ts') || file.endsWith('.config.ts') || file.split(path.sep).some(String.startsWith('.'))));
    const stem = (file: string) => path.join(path.dirname(file), path.parse(file).name);
    yield* accumulate(
        modules.filter((file) => modules.some(String.startsWith(`${stem(file)}${path.sep}`))),
        (file) => InferenceError.make({ reason: { _tag: 'Shadowed', folder: stem(file) }, subject: path.join(dir, file) }),
    );
    const targets = Object.values(manifest.exports).map(path.normalize);
    const folders = targets
        .filter((target) => path.basename(target) === BARREL)
        .map(path.dirname)
        .toSorted((left, right) => right.length - left.length);
    const owned = Array.groupBy(
        modules.filter((file) => !(targets.includes(file) || file.split(path.sep).includes('internal'))),
        ownerFolder(folders, path.sep),
    );
    const barrels = yield* Effect.forEach(folders, (folder) => barrelOutput(tree, dir, manifest.imports, folder, Option.getOrElse(Record.get(owned, folder), Array.empty)));
    const neutral = yield* pipe(
        Record.get(manifest.exports, '.'),
        Option.filter(() => tree.exists(path.join(dir, TSCONFIG))),
        Option.map((entry) => neutralOutput(dir, entry)),
        Effect.transposeOption,
    );
    return [...barrels, ...Option.toArray(neutral)];
});
const formatOutput = (cwd: string, { file, text }: { readonly file: string; readonly text: string }) => Effect.map(runCommand('biome', ['check', '--write', `--stdin-file-path=${file}`], { cwd, stdin: Stream.encodeText(Stream.make(text)) }), (next) => ({ file, next }));

// --- [PROJECTS]
const folderInput = (folder: string) => `{workspaceRoot}/${folder}/**/*`;
const tagPrefix = Effect.fnUntraced(function* ({ release }: Workspace, tags: readonly string[], name: string) {
    const { releaseTag, groups } = yield* release;
    const selectors = new Set(tags.map((tag) => `tag:${tag}`));
    const [group] = yield* Effect.fromOption(
        Record.findFirst(groups, ({ projects }) => !selectors.isDisjointFrom(new Set(projects))),
        () => InferenceError.missing(`${name} (${tags.join(', ')})`, 'nx.json release group'),
    );
    return releaseTag.pattern.replace('{releaseGroupName}', group).replace('{projectName}', name).replace('{version}', '');
});
const suitePath = (path: Path.Path, base: string, dir: string) => path.join(base, ...dir.split(path.sep).filter((segment) => segment !== path.basename(base)));
const pythonTargets = Effect.fnUntraced(function* (dir: string, { root, pyproject, hosts }: Workspace) {
    const [path, { pythonFiles, testPaths, dataFile, members }, folders] = yield* Effect.all([Path.Path, pyproject, hosts]);
    const suites = yield* Effect.filter(
        testPaths.map((testPath) => suitePath(path, testPath, dir)),
        (suite) => Effect.map(glob(root, [`${suite}/**/{${pythonFiles.join(',')}}`]), Array.isArrayNonEmpty),
    );
    const segments = dir.split(path.sep);
    const coverage = `${dataFile}.{projectName}`;
    const command =
        segments.includes('blender') && Array.isArrayNonEmpty(suites)
            ? `python -m eng.python.blender run {projectName} --package ${quote([yield* Effect.fromOption(Record.get(members, Array.headNonEmpty(suites)), () => InferenceError.missing(Array.headNonEmpty(suites), 'workspace package'))])} --groups test -- --python-exit-code 1 --python ${quote([path.join(root, 'tests/python/libs/blender/bpy/background/session.py')])} -- -n0`
            : 'pytest';
    const pytest = folders.some((host) => segments.includes(host)) ? { options: { args: suites }, outputs: [] } : { options: { args: [...suites, '--cov'], env: { COVERAGE_FILE: coverage } }, outputs: [`{workspaceRoot}/${coverage}`] };
    return Array.match(suites, { onEmpty: () => ({}), onNonEmpty: () => ({ check: {}, test: { command, inputs: suites.map(folderInput), ...pytest } }) });
});
const suiteInput = (...suites: readonly string[]) => ({ namedInputs: { default: ['{projectRoot}/**/*', ...suites.map(folderInput)] } });
const suiteInputs = Effect.fnUntraced(function* (language: string, dir: string, root: string) {
    const path = yield* Path.Path;
    const suite = suitePath(path, path.join(TESTS, language), dir);
    return Array.match(yield* glob(root, [`${suite}/**/*`]), { onEmpty: () => ({}), onNonEmpty: () => suiteInput(suite) });
});
const containerTests = Effect.fnUntraced(function* (dir: string, root: string) {
    const path = yield* Path.Path;
    const importsContainers = (file: string, text: string) => parseSync(file, text).module.staticImports.some(({ moduleRequest }) => CONTAINERS.test(moduleRequest.value));
    const suite = suitePath(path, path.join(TESTS, 'typescript'), dir);
    const tests = yield* Effect.filter(
        yield* glob(root, [`${suite}/**/*.test.{ts,tsx}`]),
        Effect.fnUntraced(function* (test) {
            return importsContainers(test, yield* decodeFile(Schema.String, root, test));
        }),
        { concurrency: 'unbounded' },
    );
    return tests.map((test) => path.relative(suite, test));
});
const attachedSuites = <Suite>(suites: Readonly<Record<string, Suite>>, module: string) => Record.getSomes(Record.map({ test: 'Tests', 'test:integration': 'IntegrationTests' }, (suffix) => Record.get(suites, `${module}${suffix}`)));
const swiftProject = (tags: readonly string[], targets: readonly string[]) => ({
    tags: ['language:swift', ...tags],
    targets: { check: {}, ...Record.fromIterableWith(['build', 'lint:periphery', ...targets], (target) => [target, {}]) },
});
const binlog = (target: string) => `${DOTNET}/binlog/{projectName}/${target.replaceAll(':', '-')}.binlog`;
const markers = {
    '*.csproj': Effect.fnUntraced(function* ({ dir }, { file, root }) {
        const [{ Project }, windows] = yield* Effect.all([decodeFile(MSBuildProject, root, file), Effect.map(glob(root, [`${dir}/**/Windows/**/*.cs`]), Array.isArrayNonEmpty)]);
        const plugin = Project.PropertyGroup.some(({ TargetExt }) => TargetExt === '.rhp');
        const outputs = (target: string, configuration: string) => [`${DOTNET}/{bin,obj}/{projectName}/${configuration}*`, binlog(target), ...(plugin ? [`${DOTNET}/{bin,obj}/*/{projectName}_${configuration}*`] : [])];
        return {
            tags: ['language:dotnet', ...(plugin ? ['output:rhp'] : []), ...(windows ? ['platform:windows'] : [])],
            targets: {
                build: { outputs: outputs('build', 'debug') },
                'build:release': { outputs: outputs('build:release', 'release'), defaultConfiguration: 'release' },
                ...(windows
                    ? {
                          'build:windows': {
                              options: { cwd: '{projectRoot}', args: ['--runtime', 'win-x64', '"-p:ArtifactsPath=$NX_WORKSPACE_ROOT/.artifacts/dotnet/windows"'] },
                              outputs: [`${DOTNET}/windows/{bin,obj}/{projectName}`, binlog('build:windows')],
                          },
                      }
                    : {}),
                check: {},
                ...(plugin ? { pack: {}, install: {}, publish: {} } : {}),
            },
        };
    }),
    '*.cs': Effect.fnUntraced(function* ({ dir, base, ext }, { file, root }) {
        yield* Effect.filterOrFail(decodeFile(Schema.String, root, file), (text) => text.startsWith('#!'));
        return {
            name: dir,
            tags: ['language:dotnet'],
            targets: {
                ...Record.fromIterableWith(['build', 'format'], (step) => [step, { executor: 'nx:noop', dependsOn: [{ target: `${step}:*${ext}`, params: 'forward' as const }] }]),
                [`restore:${base}`]: { outputs: [`${DOTNET}/obj/${base}/project.assets.json`, `${DOTNET}/obj/${base}/*.nuget.*`, binlog(`restore:${base}`)], options: { args: [file] } },
                [`build:${base}`]: { dependsOn: [`restore:${base}`], inputs: ['default'], outputs: [`${DOTNET}/bin/${base}`, `${DOTNET}/obj/${base}/*/**/*`, binlog(`build:${base}`)], options: { args: [file, '--no-restore'] } },
                [`run:${base}`]: { dependsOn: [`build:${base}`], options: { args: [file, '--no-build', '--'] } },
                [`format:${base}`]: { options: { args: [file, '--no-restore'] }, dependsOn: [`restore:${base}`] },
                check: {},
            },
        };
    }),
    '.swcrc': { tags: ['host:extendscript'], targets: { build: {} } },
    [CARGO_MANIFEST]: Effect.fnUntraced(function* ({ dir }, { file, root, crates, mise }) {
        const [path, all, { daemons }] = yield* Effect.all([Path.Path, crates, mise], { concurrency: 'unbounded' });
        const crate = yield* Effect.fromOption(Array.findFirst(all, ({ manifest }) => manifest === file));
        const sources = Array.dedupe(crate.targets.map(({ source }) => path.dirname(path.relative(root, source)))).filter((source) => path.relative(dir, source).split(path.sep, 1)[0] === '..');
        const bins = crate.targets.filter(({ kind }) => kind.includes('bin'));
        const named = new Set(bins.map(({ name }) => name));
        const commands = Record.getSomes(Record.map(daemons, (row) => (typeof row === 'string' || !row.boot ? Option.none() : row.run)));
        const launched = Record.keys(Record.filter(commands, (command) => !named.isDisjointFrom(new Set(command.split(' ')))));
        const tests = crate.targets.filter(Struct.get('test'));
        const tested = Array.isArrayNonEmpty(tests);
        const dylint = crate.dependencies.some(({ name }) => name === 'dylint_linting');
        const library = ({ kind, types }: (typeof crate.targets)[number]) => Array.intersection(Array.difference(kind, ['bin']), types).length > 0;
        const selection = tested ? { options: { args: crate.targets.some(library) ? ['--lib', '--bins', '--tests'] : ['--all-targets'] } } : {};
        const args = tests.flatMap((target) => (library(target) ? ['--lib'] : [`--${Array.headNonEmpty(target.kind)}`, target.name]));
        return {
            ...(sources.length > 0 ? suiteInput(...sources) : {}),
            name: crate.name,
            tags: ['language:rust', ...(dylint ? ['host:dylint'] : [])],
            targets: {
                'typecheck:clippy': { ...selection },
                'lint:cargo-modules': {},
                'lint:dylint': { ...selection },
                check: {},
                ...(tested ? { test: { options: { args } }, ...(dylint ? {} : { coverage: { options: { args } } }) } : {}),
                ...(bins.length > 0 ? { build: {} } : {}),
                ...(launched.length > 0 ? { install: { options: { args: launched } }, 'install:cargo': {}, 'upgrade:cargo': { options: { args: launched }, dependsOn: [{ projects: ['directory:.'], target: 'upgrade:rust' }] } } : {}),
            },
        };
    }),
    'blender_manifest.toml': Effect.fnUntraced(function* ({ dir }, workspace) {
        const [{ packages }, { id }, targets] = yield* Effect.all([workspace.exported(dir), decodeFile(Manifest, workspace.root, workspace.file), pythonTargets(dir, workspace)], { concurrency: 'unbounded' });
        const tags = ['language:python', 'output:extension'];
        return {
            name: id,
            tags,
            targets: {
                pack: {
                    inputs: Array.getSomes(packages.map(Struct.get('directory')))
                        .map(Struct.get('path'))
                        .map(folderInput),
                    options: { env: { MinVerTagPrefix: yield* tagPrefix(workspace, tags, id) } },
                },
                install: {},
                publish: {},
                ...targets,
            },
        };
    }),
    [XCODE_PROJECT]: Effect.fnUntraced(function* ({ dir }, workspace) {
        const [path, { files, targets }] = yield* Effect.all([Path.Path, decodeFile(XcodeProject, workspace.root, workspace.file)]);
        const [root, name] = [path.dirname(dir), path.parse(dir).name];
        const identifier = yield* Effect.fromOption(
            Option.flatMap(
                Array.findFirst(targets, (target) => target.name === name),
                ({ settings }) => settings.PRODUCT_BUNDLE_IDENTIFIER,
            ),
            () => InferenceError.missing(`${workspace.file} target ${name}`, 'PRODUCT_BUNDLE_IDENTIFIER'),
        );
        const updated = targets.flatMap(Struct.get('members')).some(Predicate.Struct({ product: Equal.equals('Sparkle') }));
        const owners = yield* Effect.forEach(
            Array.getSomes(files.map(Struct.get('path')))
                .map((entry) => path.relative('.artifacts', path.join(root, entry)))
                .filter((source) => !source.startsWith('..')),
            (source) => Effect.map(owning(workspace.root, path.join(path.dirname(source), path.parse(source).name)), Function.flow(Struct.get('manifest'), Struct.get('name'))),
        );
        const bundled = Array.match(Array.dedupe(owners), { onEmpty: () => [], onNonEmpty: (projects) => [{ projects, target: 'pack' }] });
        const tags = ['build:xcode', ...(updated ? ['output:appcast'] : [])];
        const xcode = swiftProject(tags, ['pack', 'install', 'lint:swiftlint', 'upgrade:xcode', ...(updated ? ['publish'] : [])]);
        const options = { env: { XCODE_DERIVED_DATA_PATH: path.join('.cache/xcode', root), MinVerTagPrefix: yield* tagPrefix(workspace, tags, name), PRODUCT_BUNDLE_IDENTIFIER: identifier } };
        yield* Effect.sync(() => workspace.applications.set(workspace.file, { root, env: options.env }));
        return { root, name, ...xcode, targets: { ...Record.map(xcode.targets, () => ({ options })), ...Record.fromIterableWith(['build', 'pack'], (step) => [step, { options, dependsOn: bundled }]) } };
    }),
    'py.typed': Effect.fnUntraced(function* ({ dir }, workspace) {
        const [path, { moduleRoots, commands }, targets] = yield* Effect.all([Path.Path, workspace.pyproject, pythonTargets(dir, workspace)]);
        const moduleRoot = yield* Effect.fromOption(Array.findFirst(moduleRoots, (held) => !path.relative(held, dir).startsWith('..')));
        const command = commands.includes(dir);
        return { name: path.relative(moduleRoot, dir).replaceAll(path.sep, '.'), tags: ['language:python', ...(command ? ['output:command'] : [])], targets: { ...targets, ...(command ? { install: {} } : {}) } };
    }),
    [TSCONFIG]: Effect.fnUntraced(function* ({ dir }, { file, root, configs, configFiles }) {
        const [path, readConfig] = yield* Effect.all([Path.Path, configs]);
        const {
            config: { options, errors },
            files,
        } = yield* Effect.try(() => readConfig(file));
        yield* Array.match(errors, { onEmpty: () => Effect.void, onNonEmpty: Effect.fail });
        const [suite, { owner, manifest }, containers, entries, hosts] = yield* Effect.all([suiteInputs('typescript', dir, root), owning(root, dir), containerTests(dir, root), glob(root, [`${dir}/*/main.ts`]), glob(root, [`${dir}/*/host.ts`])], {
            concurrency: 'unbounded',
        });
        const named = owner === dir ? {} : { name: path.join(manifest.name, path.relative(owner, dir)) };
        const configured = configFiles.map(path.dirname);
        const runs = [...Array.difference(entries.map(path.dirname), configured).map((folder) => [folder, `node ${path.join(folder, 'main.ts')}`] as const), ...hosts.map(path.dirname).map((folder) => [folder, `node ${path.join(dir, 'main.ts')} ${path.basename(folder)}`] as const)].map(
            ([folder, command]) => [path.basename(folder), { command, ...(configured.some(Predicate.and(Predicate.not(Equal.equals(folder)), String.startsWith(`${folder}${path.sep}`))) ? { dependsOn: [{ projects: [`directory:${folder}/**`], target: 'build' }] } : {}) }] as const,
        );
        const integration = Array.match(containers, { onEmpty: () => ({}), onNonEmpty: (tests) => ({ test: { options: { args: tests.map((test) => `--exclude='**/${test}'`) } }, 'test:integration': { options: { args: tests } } }) });
        return {
            ...suite,
            ...named,
            tags: ['language:typescript'],
            targets: {
                'typecheck:tsc': { inputs: [...files.map((held) => ({ fileset: `{workspaceRoot}/${held}`, includeIgnored: true })), ...(options.typeRoots ?? []).map((typeRoot) => ({ fileset: `{workspaceRoot}/${path.relative(root, typeRoot)}/**/*.d.ts`, includeIgnored: true }))] },
                ...(owner === dir && rootedLibrary(manifest) ? { 'typecheck:neutral': {} } : {}),
                check: {},
                ...Object.fromEntries(runs),
                ...integration,
                ...(owner === dir && Record.has(manifest.dependencies, '@effect/sql-pg') ? { 'lint:squawk': {} } : {}),
            },
        };
    }),
    'vite.config.ts': ({ dir }, { configFiles }) => Effect.map(Path.Path, (path) => configFiles.includes(path.join(dir, CLOUDFLARE_CONFIG))).pipe(Effect.map((hosted) => ({ tags: ['bundler:vite', ...(hosted ? ['output:cloudflare'] : [])], targets: { build: {}, serve: {}, ...(hosted ? { hosting: {} } : {}) } }))),
    ...Record.fromIterableWith(['uxp', 'cep'] as const, (host) => [`${host}.config.ts` as const, { tags: [`host:${host}`], targets: { build: {}, pack: {}, install: {} } }]),
    'script.config.ts': { tags: ['output:script'], targets: { build: {} } },
    'sea.config.ts': { tags: ['output:sea'], targets: { pack: {} } },
    [CLOUDFLARE_CONFIG]: { targets: { types: {}, 'typecheck:tsc': { dependsOn: ['types'] } } },
    '.storybook/main.ts': { targets: { storybook: {} } },
    'hooks/hooks.json': Effect.fnUntraced(function* (_, { file, root }) {
        yield* Effect.fromOption(Schema.decodeOption(Hooks)(yield* decodeFile(Schema.String, root, file)));
        return { tags: ['host:claude'] };
    }),
} as const satisfies Record<
    string,
    | Optional<ProjectConfiguration, 'root'>
    | ((file: Path.Path.Parsed, workspace: Workspace & { readonly file: string; readonly configFiles: readonly string[]; readonly applications: Map<string, Application> }) => Effect.Effect<Optional<ProjectConfiguration, 'root'>, unknown, ManagedRuntime.ManagedRuntime.Services<typeof runtime>>)
>;
const infer = Effect.fnUntraced(
    function* (file: string, workspace: Workspace & { readonly configFiles: readonly string[]; readonly applications: Map<string, Application> }) {
        const path = yield* Path.Path;
        const parsed = path.parse(file);
        const [marker, configure] = yield* Effect.fromOption(Array.findFirst(Object.entries(markers), ([key]) => [parsed.base, path.join(path.basename(parsed.dir), parsed.base), `*${parsed.ext}`].includes(key)));
        const { root: directory = marker.split('/').reduce((at) => path.dirname(at), file), tags = [], ...configuration }: Optional<ProjectConfiguration, 'root'> = Predicate.isFunction(configure) ? yield* configure(parsed, { ...workspace, file }) : configure;
        const hosted = Array.intersection(directory.split('/'), yield* workspace.hosts).map((host) => `host:${host}`);
        return { projects: { [directory]: { ...configuration, root: directory, tags: Array.union(tags, hosted) } } };
    },
    Effect.catchNoSuchElement,
    Effect.map(Option.getOrElse(() => ({}))),
);

// --- [ROOT]
const pythonVersions = Effect.fnUntraced(function* ({ root, hosts, pyproject, python, scanExclusions }: Workspace) {
    const [path, folders, { requirements, deptry, fawltydeps }, exclusions] = yield* Effect.all([Path.Path, hosts, pyproject, scanExclusions]);
    const [{ versions }, scopes, rules] = yield* Effect.all(
        [
            python,
            Effect.flatMap(
                glob(
                    root,
                    folders.map((host) => `**/${host}/**/${PYPROJECT}`),
                ),
                Effect.validate(
                    Effect.fnUntraced(function* (file: string) {
                        const { project } = yield* decodeFile(Host, root, file);
                        return { scope: path.dirname(file), version: project.minimum[1] };
                    }),
                    { concurrency: 'unbounded' },
                ),
            ),
            decodeFile(Sgconfig, root, 'sgconfig.yml').pipe(
                Effect.map(Struct.get('ruleDirs')),
                Effect.map(Array.map((dir) => `${dir}/python/**/*.yml`)),
                Effect.flatMap((patterns) => glob(root, patterns)),
                Effect.flatMap(Effect.validate((file: string) => decodeFile(Rule, root, file), { concurrency: 'unbounded' })),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const [interpreter] = versions;
    const reaches = (version: string) => (release: string) => VERSIONS.compare(version, release) >= 0;
    const runs =
        (version: string) =>
        ({ metadata: { below, from } }: (typeof rules)[number]) =>
            !Option.exists(below, reaches(version)) && Option.match(from, { onNone: () => true, onSome: reaches(version) });
    const off = Array.dedupe(Array.getSomes(Array.cartesianWith(rules, [interpreter, ...scopes.map(Struct.get('version'))], (rule, version) => Option.liftPredicate(rule, Predicate.not(runs(version))))));
    const ids = (held: typeof rules) => quote(['--filter', `^(${held.map(Struct.get('id')).map(RegExp.escape).join('|')})$`]);
    const dependencyGroups = Record.mapKeys(requirements, String.concat.bind(null, '--no-group '));
    const groups = Record.collect(
        Array.groupBy(scopes, ({ version }) => version),
        (version, members) => {
            const environment = `.cache/python/${version}`;
            const interpreterInput = { runtime: `VIRTUAL_ENV=${environment} uv python find ${version} --show-version` };
            const excludedGroups = Record.keys(Record.filter(dependencyGroups, Function.flow(Struct.get('minimum'), Tuple.get(1), reaches(version), Boolean.not)));
            return {
                version,
                environment,
                executable: `"$(uv python find ${version})"`,
                scoped: quote(members.map(Struct.get('scope'))),
                interpreterInput,
                inputs: [...members.map(Struct.get('scope')).map(folderInput), interpreterInput],
                synced: [`export VIRTUAL_ENV=${environment} && uv sync --active --locked --python ${version}`, ...excludedGroups, '&&'].join(' '),
            };
        },
    );
    const excluded = quote(scopes.flatMap(({ scope }) => ['--exclude', `^${RegExp.escape(`${scope}/`)}`]));
    const scan = (flags: readonly string[]) => ['ast-grep scan --custom-languages allow', ...flags].join(' ');
    const walk = (flags: readonly string[]) => scan([...flags, quote(exclusions.flatMap((pattern) => ['--globs', `!${pattern}`]))]);
    const offs = off.map(({ id }) => quote([`--off=${id}`]));
    const tree = walk(['--error=no-suppress-all', ...offs]);
    const hosted = Record.filterMap(
        {
            root: { version: interpreter, command: (filter: string) => walk([filter, ...scopes.map(({ scope }) => quote(['--globs', `!${scope}/**`]))]), inputs: ['pythonFiles', '{workspaceRoot}/**/*.{py3,bzl,bazel}', ...scopes.map(({ scope }) => `!${folderInput(scope)}`)] },
            ...Record.fromIterableWith(groups, ({ version, scoped, inputs }) => [version, { version, command: (filter: string) => scan([filter, scoped]), inputs }]),
        },
        ({ version, command, inputs }) => Result.map(Result.liftPredicate({ running: off.filter(runs(version)), inputs }, Predicate.Struct({ running: Array.isArrayNonEmpty }), Function.constVoid), Function.flow(Struct.evolve({ running: Function.flow(ids, command) }), Struct.renameKeys({ running: 'command' }))),
    );
    const lock = 'flock .cache/ast-grep/rewrite.lock';
    const split = (target: string, children: Readonly<Record<string, TargetConfiguration>>) => {
        const [lead, ...rest] = Object.keys(children).map((child) => `${target}:${child}`);
        return {
            [target]: { dependsOn: [{ target: lead, params: 'forward' }, ...rest] },
            ...Record.mapKeys(children, (child) => `${target}:${child}`),
        };
    };
    const commands = {
        root: {
            inputs: ['pythonFiles'],
            ty: `ty check ${quote(scopes.flatMap(({ scope }) => ['--exclude', scope]))}`,
            mypy: `python -m tools.refurb --quiet -- ${excluded}`,
            native: `mypy --num-workers auto ${excluded}`,
        },
        ...Record.fromIterableWith(groups, ({ version, executable, scoped, inputs, synced }) => [
            version,
            {
                inputs,
                ty: `${synced} ty check --python-version ${version} ${scoped}`,
                mypy: `${synced} python -m tools.refurb --quiet --python-version ${version} ${scoped} -- --python-executable ${executable}`,
                native: `${synced} mypy --num-workers auto --python-version ${version} --python-executable ${executable} ${scoped}`,
            },
        ]),
    };
    const deptryCheck = ({ member, project, tool }: (typeof deptry)[number]) => {
        const roots = [member, ...Array.intersection(member.split(path.sep), folders).map((host) => path.join(APPS, host))];
        const firstParty = tool.hatch.build.devModeDirs.flatMap((folder) => ['--known-first-party', path.relative(path.join(member, folder), member)]);
        const checker = Option.match(
            Option.flatMap(Array.findFirst(scopes, Predicate.Struct({ scope: Equal.equals(member) })), ({ version }) => Array.findFirst(groups, Predicate.Struct({ version: Equal.equals(version) }))),
            {
                onNone: () => ({ command: 'deptry', inputs: [] }),
                onSome: ({ executable, environment, interpreterInput }) => ({ command: `${executable} -m deptry`, options: { env: { VIRTUAL_ENV: environment } }, inputs: [interpreterInput] }),
            },
        );
        return [`lint:deptry:${project.name}`, { ...checker, command: `${checker.command} ${quote(['--config', path.join(member, PYPROJECT), ...firstParty, ...roots])}`, inputs: ['python', ...roots.map(folderInput), ...checker.inputs] }] as const;
    };
    const deptryRuns = deptry.map(deptryCheck);
    const fawltyRuns = fawltydeps.members.map(
        ({ member, project }) => [`lint:fawltydeps:${project.name}`, { command: 'fawltydeps', options: { cwd: member, args: ['--pyenv', '"$VIRTUAL_ENV"'], env: { FAWLTYDEPS_CUSTOM_MAPPING: fawltydeps.mapping } }, inputs: ['python', folderInput(member), { env: 'VIRTUAL_ENV' }] }] as const,
    );
    return {
        projects: {
            '.': {
                root: '.',
                targets: {
                    ...split('lint:ast-grep', { tree: { command: tree }, ...hosted }),
                    ...split(
                        'rewrite',
                        Record.map({ tree: { command: walk(offs) }, ...hosted }, ({ command, ...configuration }) => ({ ...configuration, command: `${lock} ${command} --update-all` })),
                    ),
                    ...split(
                        'typecheck:ty',
                        Record.map(commands, ({ inputs, ty }) => ({ command: ty, inputs, options: { env: { TY_UV: '1', UV_LOCKED: '1' } } })),
                    ),
                    ...split(
                        'typecheck:mypy',
                        Record.map(commands, ({ inputs, mypy, native }) => ({ command: mypy, inputs: [...inputs, '{workspaceRoot}/tools/refurb/**/*.py'], configurations: { native: { command: native } } })),
                    ),
                    'lint:refurb': { dependsOn: [{ target: 'typecheck:mypy', params: 'forward' }] },
                    ...Record.fromIterableWith(Object.keys(commands), (scope) => [`lint:refurb:${scope}`, { dependsOn: [{ target: `typecheck:mypy:${scope}`, params: 'forward' }] } satisfies TargetConfiguration]),
                    ...Array.match(deptryRuns, { onEmpty: () => ({}), onNonEmpty: (checks) => ({ 'lint:deptry': { dependsOn: [{ target: 'lint:deptry:*', params: 'forward' as const }] }, ...Object.fromEntries(checks) }) }),
                    ...Array.match(fawltyRuns, { onEmpty: () => ({}), onNonEmpty: (checks) => ({ 'lint:fawltydeps': { dependsOn: [{ target: 'lint:fawltydeps:*', params: 'forward' as const }] }, ...Object.fromEntries(checks) }) }),
                },
            },
        },
    };
});
const swiftModules = Effect.fnUntraced(function* ({ swift, hosts }: Workspace) {
    const [{ targets, platforms, tests }, folders] = yield* Effect.all([swift, hosts]);
    const [suites, sources] = tests;
    const platformed = platforms.map(Struct.get('name'));
    const suiteTargets = Record.fromIterableWith(suites, ({ name, path }) => [name, { path, target: { options: { args: ['--test-product', name] } } }]);
    const excluded = Array.match(suites, { onEmpty: () => ({}), onNonEmpty: (all) => ({ options: { args: ['--exclude-targets', ...all.map(Struct.get('c99name'))] } }) });
    const modules = sources.map((target) => {
        const segments = target.path.split('/');
        const installed = target.type === 'executable' && segments[0] !== TESTS;
        const project = swiftProject(['build:swiftpm', ...Array.intersection(Array.union(platformed, segments), folders).map(String.concat.bind(null, 'host:'))], installed ? ['install', 'upgrade:swiftpm'] : []);
        const attached = attachedSuites(suiteTargets, target.name);
        return [
            target.path,
            {
                ...project,
                ...Array.match(Object.values(attached), { onEmpty: () => ({}), onNonEmpty: (held) => suiteInput(...held.map(Struct.get('path'))) }),
                root: target.path,
                name: target.name,
                targets: { ...project.targets, 'lint:periphery': excluded, ...Record.map(attached, Struct.get('target')) },
            },
        ] as const;
    });
    return {
        projects: {
            ...Object.fromEntries(modules),
            '.': {
                root: '.',
                targets: {
                    'typecheck:swift': { inputs: ['swift', ...targets.map(Struct.get('path')).map(folderInput)] },
                },
            },
        },
    };
});
const roots = { [PYPROJECT]: pythonVersions, [SWIFT_MANIFEST]: swiftModules } as const;

// --- [COMPONENTS]
const components = Effect.fnUntraced(function* (root: string, inferred: CreateNodesResultArray, applications: ReadonlyMap<string, Application>) {
    const path = yield* Path.Path;
    const projects = inferred.flatMap(([, { projects: held = {} }]) => Object.entries(held));
    const declared = (at: string) => Array.dedupe(projects.filter(([directory]) => directory === at).flatMap(([, project]) => Object.keys(project.targets ?? {})));
    const members = yield* Effect.forEach(
        applications.size === 0 ? [] : yield* glob(root, Array.dedupe(projects.map(([at]) => path.join(at, Package.file)))),
        Effect.fnUntraced(function* (file) {
            const at = path.dirname(file);
            const { name } = yield* Package.read(path.join(root, at));
            return { at, name, steps: Array.intersection(['build', 'pack', 'install'], declared(at)) };
        }),
    );
    const component = Effect.fnUntraced(function* ([file, { root: app, env }]: readonly [string, Application]) {
        const { owner } = yield* owning(root, app);
        const nested = owner === '.' ? [] : members.filter(Predicate.Struct({ at: Predicate.and(Predicate.not(Equal.equals(app)), Function.flow(path.relative.bind(null, owner), Predicate.not(String.startsWith('..')))) }));
        const after = (step: string, needed: string) => (declared(app).includes(step) ? Array.match(nested.filter(Predicate.Struct({ steps: Array.contains(needed) })).map(Struct.get('name')), { onEmpty: () => ({}), onNonEmpty: (names) => ({ [step]: { dependsOn: [{ projects: names, target: needed }] } }) }) : {});
        const targeted = (steps: readonly string[]) => Record.map(Record.fromIterableBy(steps, Function.identity), Function.constant({ options: { env } }));
        const dependencies = Record.map(Record.fromIterableBy(nested, Struct.get('at')), Function.flow(Struct.pick(['at', 'steps']), Struct.evolve({ steps: targeted }), Struct.renameKeys({ at: 'root', steps: 'targets' })));
        return [file, { projects: { ...dependencies, [app]: { root: app, implicitDependencies: nested.map(Struct.get('name')), targets: { ...after('install', 'install'), ...after('publish', 'pack') } } } }] as const;
    });
    return yield* Effect.forEach(applications, component);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const runtime = ManagedRuntime.make(Layer.merge(layer, layerPosix));
const rendered = Effect.catchCause((cause: Cause.Cause<unknown>) => Effect.die(new Error(Cause.pretty(cause))));
const createNodes: CreateNodes<{ readonly root: Record<string, TargetConfiguration> }> = [
    `{${Object.keys(roots).join(',')},*/**/{${Object.keys(markers).join(',')}}}`,
    Effect.fnUntraced(
        function* (files, options, context) {
            const [path, workspace, { name }] = yield* Effect.all([Path.Path, openWorkspace(context), Package.read(context.workspaceRoot)]);
            const applications = new Map<string, Application>();
            const node = (file: string) => Option.match(Record.get(file)(roots), { onNone: () => infer(file, { ...workspace, configFiles: files, applications }), onSome: (root) => root(workspace) });
            const inferred = yield* Effect.promise(() => createNodesFromFiles(Function.flow(node, rendered, runtime.runPromise.bind(runtime)), files, options, context));
            const { pythonFiles, testPaths } = yield* workspace.pyproject;
            const testOwners = inferred
                .flatMap(([, { projects = {} }]) => Object.entries(projects))
                .filter(([, project]) => project.tags?.includes('language:python') && project.targets?.test)
                .map(([directory]) => directory);
            const claimed = Array.cartesianWith(testOwners, testPaths, (directory, testPath) => `${suitePath(path, testPath, directory)}/**`);
            const unclaimed = yield* glob(
                context.workspaceRoot,
                Array.cartesianWith(testPaths, pythonFiles, (testPath, pattern) => `${testPath}/**/${pattern}`),
                claimed,
            );
            const configuredCrates = Array.bind(yield* workspace.crates, 'nx', ({ metadata }) => Option.toArray(Option.flatMap(metadata, Struct.get('nx'))));
            return [
                ...inferred,
                ...Array.match(unclaimed, { onEmpty: () => [], onNonEmpty: (suites) => [[PYPROJECT, { projects: { '.': { root: '.', targets: { test: { command: 'pytest', options: { args: suites }, inputs: ['python', 'pythonFiles'] } } } } }]] satisfies CreateNodesResultArray }),
                ...(yield* components(context.workspaceRoot, inferred, applications)),
                ...configuredCrates.map(({ manifest, nx }) => [manifest, { projects: { [path.dirname(manifest)]: nx } }] as const),
                ...Option.toArray(Option.fromUndefinedOr(options)).map(({ root }) => ['nx.json', { projects: { '.': { root: '.', name, targets: root } } }] as const),
            ];
        },
        Effect.scoped,
        (effect) => runtime.runPromise(effect),
    ),
];
const createDependencies: CreateDependencies = Effect.fnUntraced(
    function* (_options, context) {
        const { fileMap, filesToProcess, workspaceRoot } = context;
        const [path, workspace] = yield* Effect.all([Path.Path, openWorkspace(context)]);
        const [sources, crates, { products, tests }, xcode] = yield* Effect.all(
            [
                Array.match(Object.values(filesToProcess.projectFileMap).flat().map(Struct.get('file')).filter(String.endsWith('.py')), {
                    onEmpty: () => Effect.succeed(Record.empty<string, readonly string[]>()),
                    onNonEmpty: (files) => Effect.flatMap(runCommand('ruff', ['analyze', 'graph', ...files], { cwd: workspaceRoot }), Schema.decodeEffect(JsonListRecord)),
                }),
                workspace.crates,
                workspace.swift,
                Effect.flatMap(
                    glob(workspaceRoot, [`*/**/${XCODE_PROJECT}`]),
                    Effect.validate(
                        Effect.fnUntraced(function* (file: string) {
                            const { targets: rows } = yield* decodeFile(XcodeProject, workspaceRoot, file);
                            return [file, rows.flatMap(Struct.get('members')).map(Struct.get('product'))] as const;
                        }),
                        { concurrency: 'unbounded' },
                    ),
                ),
            ],
            { concurrency: 'unbounded' },
        );
        const [suites, modules] = tests;
        const suiteMembers = Record.fromIterableBy(suites, Struct.get('name'));
        const owners = Object.fromEntries(Record.collect(fileMap.projectFileMap, (owner, files) => Array.cartesian(files.map(Struct.get('file')), [owner])).flat());
        const swift: RawProjectGraphDependency[] = modules.flatMap(({ name, locals }) => Array.cartesian([name], [...locals, ...Array.difference(Object.values(attachedSuites(suiteMembers, name)).flatMap(Struct.get('locals')), [name])])).map(([source, target]) => ({ source, target, type: DependencyType.implicit }));
        const linked = xcode.flatMap(([file, names]) => Array.cartesian(Option.toArray(Record.get(owners, file)), products.filter(Predicate.Struct({ name: Array.contains.bind(null, names) })).flatMap(Struct.get('targets'))).map(Tuple.appendElement(file)));
        const references = [
            ...Record.collect(sources, (file, targets) => Array.cartesian([file], targets)).flat(),
            ...crates.flatMap(({ manifest, dependencies }) => Array.cartesian([manifest], Array.getSomes(dependencies.map(Struct.get('path'))))).map(([manifest, member]) => [manifest, path.join(path.relative(workspaceRoot, member), CARGO_MANIFEST)] as const),
        ];
        const resolved = Array.getSomes(references.map(([sourceFile, file]) => Option.map(Option.all([Record.get(owners, sourceFile), Record.get(owners, file)]), Tuple.appendElement(sourceFile)))).filter(([source, target]) => source !== target);
        return [...[...resolved, ...linked].map(([source, target, sourceFile]) => ({ source, target, sourceFile, type: DependencyType.static })), ...swift];
    },
    Effect.scoped,
    rendered,
    (effect) => runtime.runPromise(effect),
);
const deploymentTarget = Effect.fnUntraced(
    function* (tree: Tree) {
        const { platforms } = yield* (yield* openWorkspace({ workspaceRoot: tree.root, nxJsonConfiguration: readNxJsonFromDisk(tree.root) })).swift;
        const { version } = yield* Effect.fromOption(Array.findFirst(platforms, ({ name }) => name === 'macos'));
        const setDeploymentTarget =
            (document: CST.Document): CST.Visitor =>
            (item, path) => {
                if (path.length === 2 && CST.resolveAsScalar(item.key)?.value === 'macOS_deployment_target' && CST.resolveAsScalar(CST.visit.itemAtPath(document, path.slice(0, -1))?.key)?.value === 'deployment_target' && CST.isScalar(item.value)) {
                    CST.setScalarValue(item.value, version);
                }
            };
        const updates = {
            'Xcode.xcconfig': (text: string) => text.replace(DEPLOYMENT_TARGET, version),
            '.swiftlint.yml': (text: string) => {
                const tokens = [...new Parser().parse(text)];
                tokens.filter((token) => token.type === 'document').forEach((document) => CST.visit(document, setDeploymentTarget(document)));
                return tokens.map(CST.stringify).join('');
            },
        };
        const rows = yield* Effect.forEach(
            Object.entries(updates),
            Effect.fnUntraced(function* ([file, update]) {
                const text = yield* Effect.fromNullishOr(tree.read(file, 'utf-8'));
                return { file, text, next: update(text) };
            }),
        );
        const drifted = rows.filter(({ text, next }) => next !== text);
        yield* Effect.forEach(drifted, ({ file, next }) => Effect.sync(tree.write.bind(tree, file, next)), { discard: true });
        return Array.match(drifted, { onEmpty: () => undefined, onNonEmpty: (changed) => ({ outOfSyncMessage: `macOS deployment target differs from ${SWIFT_MANIFEST} ${version}`, outOfSyncDetails: changed.map(Struct.get('file')) }) });
    },
    Effect.scoped,
    rendered,
    (effect) => runtime.runPromise(effect),
);

const barrels = Effect.fnUntraced(
    function* (tree: Tree) {
        const path = yield* Path.Path;
        const packages = yield* Effect.forEach(yield* glob(tree.root, [`**/${Package.file}`]), (file) => readManifest(tree, file, path.dirname(file)));
        const dirs = packages.map(Struct.get('dir'));
        const outputs = yield* Effect.forEach(
            packages.filter(({ manifest }) => rootedLibrary(manifest)),
            ({ dir, manifest }) => libraryOutputs(tree, dir, manifest, dirs.filter(String.startsWith(`${dir}${path.sep}`))),
            { concurrency: 'unbounded' },
        );
        const formatted = yield* Effect.forEach(outputs.flat(), (output) => formatOutput(tree.root, output), { concurrency: 'unbounded' });
        const drifted = formatted.filter(({ file, next }) => tree.read(file, 'utf-8') !== next);
        yield* Effect.forEach(drifted, ({ file, next }) => Effect.sync(tree.write.bind(tree, file, next)), { discard: true });
        return Array.match(drifted, { onEmpty: () => undefined, onNonEmpty: (changed) => ({ outOfSyncMessage: `Library barrels and ${NEUTRAL_TSCONFIG} differ from each library's exports and modules`, outOfSyncDetails: changed.map(Struct.get('file')) }) });
    },
    Effect.scoped,
    rendered,
    (effect) => runtime.runPromise(effect),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { barrels, createDependencies, createNodes, deploymentTarget };
