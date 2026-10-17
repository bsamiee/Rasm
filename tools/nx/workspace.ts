import python from '@ast-grep/lang-python';
import { parseAsync, registerDynamicLanguage } from '@ast-grep/napi';
import { NodeChildProcessSpawner, NodeFileSystem, NodePath } from '@effect/platform-node';
import { type CreateDependencies, type CreateDependenciesContext, type CreateNodes, type CreateNodesResultArray, createNodesFromFiles, DependencyType, type Optional, type ProjectConfiguration, parseJson, type StaticDependency, type TargetConfiguration } from '@nx/devkit';
import { Array, Cause, Effect, FileSystem, Formatter, Graph, identity, Layer, ManagedRuntime, Option, Path, pipe, Record, Schema, SchemaGetter, SchemaIssue, Stream, Trie } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';
import { XMLParser } from 'fast-xml-parser';
import { globWithWorkspaceContext, hashMultiGlobWithWorkspaceContext, PluginCache, workspaceDataDirectory } from 'nx/src/devkit-internals';
import { parse as parseToml } from 'smol-toml';
import { parse as parseYaml } from 'yaml';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _CARGO_MANIFEST = 'Cargo.toml';
const _SWIFT_MANIFEST = 'Package.swift';
const _PYPROJECT = 'pyproject.toml';

// --- [MODELS] --------------------------------------------------------------------------

const _Parsed = (expected: string, decode: (text: string) => unknown): Schema.decodeTo<Schema.Unknown, Schema.String> =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.Unknown, {
            decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => decode(text), catch: () => new SchemaIssue.InvalidValue({ expected }, text, options) })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    );
const _Toml = _Parsed('TOML', parseToml);
const _Yaml = _Parsed('YAML', parseYaml);
const _Version = (pattern: RegExp, expected: string): Schema.decodeTo<Schema.String, Schema.String> =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.String, {
            decode: SchemaGetter.transformEffect((text: string, options) => Effect.fromOption(Option.fromNullishOr(pattern.exec(text)?.groups?.version), () => new SchemaIssue.InvalidValue({ expected }, text, options))),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    );
const _Mise = _Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            tools: Schema.Struct({ python: _Version(/^(?<version>\d+\.\d+)\.\d+/u, 'a <major>.<minor>.<patch> release') }),
            bootstrap: Schema.Struct({ macos: Schema.Struct({ launchd: Schema.Struct({ agents: Schema.Record(Schema.String, Schema.Struct({ args: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) })) }) }) }),
        }),
    ),
);
const _Sgconfig = _Yaml.pipe(Schema.decodeTo(Schema.Struct({ ruleDirs: Schema.Array(Schema.String) })));
const _Rule = _Yaml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            id: Schema.String,
            metadata: Schema.OptionFromOptionalKey(Schema.Struct({ below: _Version(/^(?<version>\d+\.\d+)$/u, 'a <major>.<minor> release') }).pipe(Schema.encodeKeys({ below: 'python-below' }))),
        }),
    ),
);
const _Host = _Toml.pipe(Schema.decodeTo(Schema.Struct({ project: Schema.Struct({ minimum: _Version(/^>=\s*(?<version>\d+\.\d+)$/u, 'a >=<major>.<minor> lower bound') }).pipe(Schema.encodeKeys({ minimum: 'requires-python' })) })));
const _Root = _Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            tool: Schema.Struct({
                uv: Schema.Struct({ buildBackend: Schema.Struct({ moduleRoot: Schema.String }).pipe(Schema.encodeKeys({ moduleRoot: 'module-root' })) }).pipe(Schema.encodeKeys({ buildBackend: 'build-backend' })),
                pytest: Schema.Struct({ pythonFiles: Schema.Array(Schema.String), testPaths: Schema.Array(Schema.String) }).pipe(Schema.encodeKeys({ pythonFiles: 'python_files', testPaths: 'testpaths' })),
            }),
        }),
    ),
);
const _Manifest = _Toml.pipe(Schema.decodeTo(Schema.Struct({ id: Schema.String })));
const _Metadata = Schema.fromJsonString(
    Schema.Struct({
        packages: Schema.Array(
            Schema.Struct({
                name: Schema.String,
                manifest: Schema.String,
                targets: Schema.Array(Schema.Struct({ name: Schema.String, kind: Schema.Array(Schema.String), types: Schema.Array(Schema.String), test: Schema.Boolean }).pipe(Schema.encodeKeys({ types: 'crate_types' }))),
                dependencies: Schema.Array(Schema.Struct({ name: Schema.String, path: Schema.OptionFromOptionalKey(Schema.String) })),
            }).pipe(Schema.encodeKeys({ manifest: 'manifest_path' })),
        ),
        output: Schema.String,
    }).pipe(Schema.encodeKeys({ output: 'target_directory' })),
);
const _SwiftPackage = Schema.fromJsonString(
    Schema.Struct({
        targets: Schema.Array(Schema.Struct({ type: Schema.String })),
        dependencies: Schema.Array(Schema.Struct({ fileSystem: Schema.OptionFromOptionalKey(Schema.Array(Schema.Struct({ path: Schema.String }))) })),
    }),
);
const _XcodeProject = _Parsed('JSON', parseJson).pipe(
    Schema.decodeTo(
        Schema.Struct({
            packages: Schema.Array(Schema.Union([Schema.Struct({ kind: Schema.Literal('local'), path: Schema.String }), Schema.Struct({ kind: Schema.Literal('remote') })])).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        }),
    ),
);
const _MSBuildProject = _Parsed('MSBuild XML', (text) => new XMLParser({ isArray: (name) => name === 'PropertyGroup' }).parse(text)).pipe(
    Schema.decodeTo(Schema.Struct({ Project: Schema.Struct({ PropertyGroup: Schema.Array(Schema.Struct({ TargetExt: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }) })),
);

// --- [ERRORS] --------------------------------------------------------------------------

const _Reason = Schema.TaggedUnion({
    Unglobbed: { patterns: Schema.Array(Schema.String), cause: Schema.Defect() },
    Unhashed: { patterns: Schema.Array(Schema.String), cause: Schema.Defect() },
    Unparsed: { file: Schema.String, cause: Schema.Defect() },
    Exited: { command: Schema.String, args: Schema.Array(Schema.String), code: Schema.Finite },
});
class WorkspaceError extends Schema.TaggedError<WorkspaceError, { readonly brand: unique symbol }>()('WorkspaceError', { reason: _Reason }) {
    override get message(): string {
        return _Reason.match(this.reason, {
            Unglobbed: ({ patterns, cause }) => `cannot glob ${patterns.join(' ')} in the Nx workspace context: ${Formatter.format(cause)}`,
            Unhashed: ({ patterns, cause }) => `cannot hash ${patterns.join(' ')} in the Nx workspace context: ${Formatter.format(cause)}`,
            Unparsed: ({ file, cause }) => `${file} did not parse as Python: ${Formatter.format(cause)}`,
            Exited: ({ command, args, code }) => `${[command, ...args].join(' ')} exited with code ${code}`,
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _glob = (workspace: string, patterns: string[]): Effect.Effect<string[], WorkspaceError> => Effect.tryPromise({ try: () => globWithWorkspaceContext(workspace, patterns), catch: (cause) => WorkspaceError.make({ reason: { _tag: 'Unglobbed', patterns, cause } }) });
const _hash = (workspace: string, groups: string[][]): Effect.Effect<string[], WorkspaceError> => Effect.tryPromise({ try: () => hashMultiGlobWithWorkspaceContext(workspace, groups), catch: (cause) => WorkspaceError.make({ reason: { _tag: 'Unhashed', patterns: groups.flat(), cause } }) });
const _decode = Effect.fnUntraced(function* <A>(schema: Schema.Codec<A, string>, workspace: string, file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return yield* Effect.flatMap(fs.readFileString(path.join(workspace, file)), Schema.decodeEffect(schema));
});
const _root = Effect.fnUntraced(function* (workspace: string) {
    const { tool } = yield* _decode(_Root, workspace, _PYPROJECT);
    return { moduleRoot: tool.uv.buildBackend.moduleRoot, ...tool.pytest };
});
const _python = Effect.fnUntraced(function* (dir: string, workspace: string, root: ReturnType<typeof _root>) {
    const [path, { moduleRoot, pythonFiles, testPaths }] = yield* Effect.all([Path.Path, root]);
    const mirrors = testPaths.map((testPath) => path.join(testPath, dir.startsWith(`${moduleRoot}/`) ? path.join(path.dirname(moduleRoot), path.relative(moduleRoot, dir)) : dir));
    const tests = yield* _glob(
        workspace,
        mirrors.flatMap((mirror) => pythonFiles.map((pattern) => `${mirror}/**/${pattern}`)),
    );
    const suites = mirrors.filter((mirror) => tests.some((test) => test.startsWith(`${mirror}/`)));
    return suites.length > 0 ? { check: {}, test: { command: 'pytest', options: { args: [...suites, '--cov'] }, inputs: suites.map((suite) => `{workspaceRoot}/${suite}/**/*`) } } : {};
});
const _memo = Effect.fnUntraced(function* <A, E, R>(name: string, entries: ReadonlyArray<readonly [key: string, compute: Effect.Effect<A, E, R>]>) {
    const path = yield* Path.Path;
    const file = path.join(workspaceDataDirectory, `workspace-${name}.hash`);
    const cache = yield* Effect.sync(() => new PluginCache<A>(file));
    const keys = new Set(entries.map(([key]) => key));
    const held = Object.keys(cache.toSerializable().entries);
    const values = yield* Effect.validate(entries, ([key, compute]) => Option.match(Option.fromNullishOr(cache.get(key)), { onNone: () => compute, onSome: Effect.succeed }), { concurrency: 'unbounded' });
    yield* held.length === keys.size && held.every((key) => keys.has(key)) ? Effect.void : Effect.sync(() => new PluginCache(file, Object.fromEntries(Array.zipWith(entries, values, ([key], value) => [key, value] as const))).writeToDisk());
    return values;
});
const _sources = Effect.fnUntraced(function* (workspace: string, root: ReturnType<typeof _root>) {
    const rules = 'tools/ast-grep/utils/python/python-static-imported-module.yml';
    const [path, { moduleRoot }, files, rule] = yield* Effect.all([Path.Path, root, _glob(workspace, ['**/*.py']), Effect.cached(Effect.map(_decode(Schema.String, workspace, rules), parseYaml))], { concurrency: 'unbounded' });
    const hashes = yield* _hash(
        workspace,
        files.map((file) => [file, rules, path.relative(workspace, import.meta.filename)]),
    );
    const statements = yield* _memo(
        'imports',
        Array.zipWith(hashes, files, (hash, file) => [
            hash,
            Effect.flatMap(Effect.all([_decode(Schema.String, workspace, file), rule]), ([text, query]) =>
                Effect.map(Effect.tryPromise({ try: () => parseAsync('python', text), catch: (cause) => WorkspaceError.make({ reason: { _tag: 'Unparsed', file, cause } }) }), (tree) =>
                    tree
                        .root()
                        .findAll(query)
                        .flatMap((module) => [module.text(), ...(module.getMatch('FROM')?.fieldChildren('name') ?? []).flatMap((name) => name.findAll({ rule: { kind: 'dotted_name' } })).map((member) => `${module.text()}.${member.text()}`)]),
                ),
            ),
        ]),
    );
    const sources = Array.zipWith(files, statements, (file, imports) => {
        const source = path.parse(path.relative(file.startsWith(`${moduleRoot}/`) ? moduleRoot : '.', file));
        const parts = (source.name === '__init__' && source.dir !== '' ? source.dir : path.join(source.dir, source.name)).split(path.sep);
        return { file, name: parts.join('.'), imports: [...parts.slice(0, -1).map((_, position) => parts.slice(0, position + 1).join('.')), ...imports] };
    });
    return Graph.directed<string, undefined>((mutable) => {
        const nodes = sources.map((source) => ({ ...source, index: Graph.addNode(mutable, source.file) }));
        const modules = Trie.fromIterable(nodes.map(({ name, index }) => [`${name}.`, index] as const));
        nodes
            .flatMap(({ index, imports }) => imports.flatMap((name) => Option.toArray(Option.map(Trie.longestPrefixOf(modules, `${name}.`), ([, target]) => [index, target] as const))))
            .forEach(([source, target]) => {
                Graph.addEdge(mutable, source, target, undefined);
            });
    });
});
const _run = Effect.fnUntraced(function* (workspace: string, command: string, args: string[]) {
    const handle = yield* ChildProcess.make(command, args, { cwd: workspace, stderr: 'inherit' });
    const [output, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? output : yield* WorkspaceError.make({ reason: { _tag: 'Exited', command, args, code } });
}, Effect.scoped);
const _crates = Effect.fnUntraced(function* (workspace: string) {
    const [path, hashes] = yield* Effect.all([Path.Path, _hash(workspace, [[`**/${_CARGO_MANIFEST}`, '.cargo/config.toml', '**/*.rs']])]);
    const outputs = yield* _memo(
        'cargo',
        hashes.map((hash) => [hash, _run(workspace, 'cargo', ['metadata', '--format-version', '1', '--no-deps'])] as const),
    );
    const metadata = yield* Effect.forEach(outputs, (output) => Schema.decodeEffect(_Metadata)(output));
    return metadata.flatMap(({ packages, output }) => packages.map((crate) => ({ ...crate, manifest: path.relative(workspace, crate.manifest), output: path.relative(workspace, output) })));
});
const _packages = Effect.fnUntraced(function* (workspace: string) {
    const [path, manifests] = yield* Effect.all([Path.Path, _glob(workspace, [`*/**/${_SWIFT_MANIFEST}`])]);
    const hashes = yield* _hash(
        workspace,
        manifests.map((manifest) => [manifest]),
    );
    const outputs = yield* _memo(
        'swiftpm',
        Array.zipWith(hashes, manifests, (hash, manifest) => [hash, _run(workspace, 'swift', ['package', 'dump-package', '--package-path', path.dirname(manifest), '--scratch-path', path.join('.cache/swiftpm/build', path.dirname(manifest))])]),
    );
    const described = yield* Effect.forEach(outputs, (output) => Schema.decodeEffect(_SwiftPackage)(output));
    return Array.zipWith(manifests, described, (manifest, { targets, dependencies }) => ({
        manifest,
        tested: targets.some(({ type }) => type === 'test'),
        locals: dependencies.flatMap(({ fileSystem }) => Option.getOrElse(fileSystem, () => []).map((local) => path.join(path.relative(workspace, local.path), _SWIFT_MANIFEST))),
    }));
});
const _swift = (build: string, targets: Record<string, TargetConfiguration>): Required<Pick<ProjectConfiguration, 'tags' | 'targets'>> => ({
    tags: ['language:swift', 'host:macos', `build:${build}`],
    targets: { build: {}, upgrade: {}, lint: {}, 'lint:swift-format': {}, 'lint:swiftlint': {}, check: {}, ...targets },
});

const _PROJECTS = {
    '*.csproj': Effect.fnUntraced(function* (_, { file, workspace }) {
        const { Project } = yield* _decode(_MSBuildProject, workspace, file);
        const plugin = Project.PropertyGroup.some(({ TargetExt }) => TargetExt === '.rhp');
        const outputs = (target: string, pivot: string): string[] => [`{workspaceRoot}/.artifacts/dotnet/{bin,obj}/{projectName}/${pivot}*`, `{workspaceRoot}/.artifacts/dotnet/binlog/{projectName}/${target}.binlog`, ...(plugin ? [`{workspaceRoot}/.artifacts/dotnet/{bin,obj}/*/{projectName}_${pivot}*`] : [])];
        return {
            tags: ['language:dotnet', ...(plugin ? ['output:rhp'] : [])],
            targets: {
                build: { outputs: outputs('build', 'debug') },
                'build:release': { outputs: outputs('build:release', 'release'), ...(plugin ? { defaultConfiguration: 'release' } : {}) },
                typecheck: {},
                check: {},
                ...(plugin ? { pack: {}, install: {} } : {}),
            },
        };
    }),
    '*.cs': Effect.fnUntraced(function* ({ dir, base }, { file, workspace }) {
        yield* Effect.filterOrFail(_decode(Schema.String, workspace, file), (text) => text.startsWith('#!'));
        const inputs = ['default', '{workspaceRoot}/.editorconfig'];
        const binlog = (target: string): string => `{workspaceRoot}/.artifacts/dotnet/binlog/{projectName}/${target}:${base}.binlog`;
        return {
            name: dir,
            tags: ['language:dotnet'],
            targets: {
                build: { executor: 'nx:noop', dependsOn: [{ target: 'build:*.cs', params: 'forward' }] },
                format: { executor: 'nx:noop', dependsOn: [{ target: 'format:*.cs', params: 'forward' }] },
                lint: { executor: 'nx:noop', dependsOn: [{ target: 'lint:*.cs', params: 'forward' }] },
                [`restore:${base}`]: { inputs: ['default', 'dotnet'], outputs: [`{workspaceRoot}/.artifacts/dotnet/obj/${base}/project.assets.json`, `{workspaceRoot}/.artifacts/dotnet/obj/${base}/*.nuget.*`, binlog('restore')], options: { args: [file] } },
                [`build:${base}`]: { dependsOn: [`restore:${base}`], inputs, outputs: [`{workspaceRoot}/.artifacts/dotnet/bin/${base}`, `{workspaceRoot}/.artifacts/dotnet/obj/${base}/*/**/*`, binlog('build')], options: { args: [file, '--no-restore'] } },
                [`run:${base}`]: { dependsOn: [`build:${base}`], options: { args: [file, '--no-build', '--'] } },
                [`format:${base}`]: { options: { args: [file, '--no-restore'] }, dependsOn: [`restore:${base}`] },
                [`lint:${base}`]: { options: { args: [file, '--no-restore', '--verify-no-changes'] }, dependsOn: [`restore:${base}`], cache: true, inputs: [...inputs, 'dotnet'], outputs: [binlog('lint')] },
                typecheck: {},
                check: {},
            },
        };
    }),
    '.swcrc': () => Effect.succeed({ tags: ['host:extendscript'], targets: { build: {} } }),
    [_CARGO_MANIFEST]: Effect.fnUntraced(function* (_, { file, crates, mise }) {
        const [all, { bootstrap }] = yield* Effect.all([crates, mise], { concurrency: 'unbounded' });
        const crate = yield* Effect.fromOption(Array.findFirst(all, ({ manifest }) => manifest === file));
        const bins = crate.targets.filter(({ kind }) => kind.includes('bin'));
        const binary = bins.length > 0;
        const launched = Object.values(bootstrap.macos.launchd.agents).some(({ args }) => bins.some(({ name }) => args.includes(name)));
        const tested = crate.targets.some(({ test }) => test);
        const dylint = crate.dependencies.some(({ name }) => name === 'dylint_linting');
        const selection = tested ? { options: { args: Object.keys(Record.filter({ '--lib': crate.targets.some(({ types }) => !types.includes('bin')), '--bins': binary, '--tests': true }, identity)) } } : {};
        return {
            name: crate.name,
            tags: ['language:rust', ...(dylint ? ['host:dylint'] : [])],
            targets: {
                typecheck: { ...selection },
                'lint:cargo-modules': {},
                'lint:dylint': { ...selection },
                check: {},
                ...(tested ? { test: {}, ...(dylint ? {} : { coverage: {} }) } : {}),
                ...(binary ? { build: { outputs: bins.map(({ name }) => `{workspaceRoot}/${crate.output}/debug/${name}`) } } : {}),
                ...(launched ? { install: {}, 'install:cargo': {} } : {}),
            },
        };
    }),
    'blender_manifest.toml': Effect.fnUntraced(function* ({ dir }, { file, workspace, root }) {
        const [{ moduleRoot }, { id }, targets] = yield* Effect.all([root, _decode(_Manifest, workspace, file), _python(dir, workspace, root)], { concurrency: 'unbounded' });
        const archive = '.artifacts/blender/{projectName}/extension.zip';
        return {
            name: id,
            tags: ['language:python'],
            targets: {
                pack: { command: 'python -m eng.python.fitout pack', options: { args: ['{projectRoot}', archive] }, inputs: [`{workspaceRoot}/${moduleRoot}/**/*`], outputs: [`{workspaceRoot}/${archive}`] },
                install: { command: 'python -m eng.python.fitout install', options: { args: [archive] } },
                ...targets,
            },
        };
    }),
    'project.xcproj': ({ dir }) => Effect.map(Path.Path, (path) => ({ root: path.dirname(dir), name: path.parse(dir).name, ..._swift('xcode', { install: {} }) })),
    [_SWIFT_MANIFEST]: Effect.fnUntraced(function* ({ dir }, { file, packages }) {
        const { tested } = yield* Effect.fromOption(Array.findFirst(yield* packages, ({ manifest }) => manifest === file));
        return { name: dir, ..._swift('swiftpm', tested ? { test: {} } : {}) };
    }),
    'py.typed': ({ dir }, { workspace, root }) => Effect.map(Effect.all([Path.Path, root, _python(dir, workspace, root)]), ([path, { moduleRoot }, targets]) => ({ name: path.relative(moduleRoot, dir).replaceAll(path.sep, '.'), tags: ['language:python'], targets })),
    'tsconfig.json': () => Effect.succeed({ tags: ['language:typescript'], targets: { typecheck: {}, check: {} } }),
    'vite.config.ts': () => Effect.succeed({ tags: ['bundler:vite'], targets: { build: {}, serve: {} } }),
    'uxp.config.ts': () => Effect.succeed({ tags: ['host:uxp'], targets: { build: {}, install: {} } }),
    'cloudflare.config.ts': () => Effect.succeed({ tags: ['output:cloudflare'], targets: { build: {} } }),
} as const satisfies Record<
    string,
    (
        file: Path.Path.Parsed,
        context: {
            readonly file: string;
            readonly workspace: string;
            readonly root: ReturnType<typeof _root>;
            readonly mise: ReturnType<typeof _decode<typeof _Mise.Type>>;
            readonly crates: ReturnType<typeof _crates>;
            readonly packages: ReturnType<typeof _packages>;
        },
    ) => Effect.Effect<Optional<ProjectConfiguration, 'root'>, unknown, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner>
>;

const _pythonVersions = Effect.fnUntraced(function* (workspace: string, mise: ReturnType<typeof _decode<typeof _Mise.Type>>) {
    const path = yield* Path.Path;
    const [{ tools }, hosts, gated] = yield* Effect.all(
        [
            mise,
            Effect.flatMap(_glob(workspace, [`*/**/${_PYPROJECT}`]), (files) => Effect.validate(files, (file) => Effect.map(_decode(_Host, workspace, file), ({ project }) => ({ scope: path.dirname(file), version: project.minimum })), { concurrency: 'unbounded' })),
            Effect.flatMap(_decode(_Sgconfig, workspace, 'sgconfig.yml'), ({ ruleDirs }) =>
                Effect.flatMap(
                    _glob(
                        workspace,
                        ruleDirs.map((dir) => `${dir}/python/**/*.yml`),
                    ),
                    (files) => Effect.validate(files, (file) => Effect.map(_decode(_Rule, workspace, file), ({ id, metadata }) => Option.map(metadata, ({ below }) => ({ id, below }))), { concurrency: 'unbounded' }),
                ),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const covered =
        (version: string) =>
        ({ below }: { readonly below: string }): boolean =>
            version.localeCompare(below, 'en', { numeric: true }) >= 0;
    const off = Array.getSomes(gated).filter(covered(tools.python));
    const groups = Object.entries(Array.groupBy(hosts, ({ version }) => version)).map(([version, members]) => ({ version, scoped: members.map(({ scope }) => `"${scope}"`).join(' ') }));
    const excluded = hosts.map(({ scope }) => `--exclude '^${RegExp.escape(`${scope}/`)}'`).join(' ');
    const scan = 'ast-grep scan --custom-languages allow';
    const tree = `${scan} --no-ignore hidden --globs '!pnpm-lock.yaml' --globs '!tools/*/examples/**' --error=no-suppress-all ${off.map(({ id }) => `--off=${id}`).join(' ')}`;
    const versioned = (whole: string, hosted: readonly string[]): TargetConfiguration => ({
        executor: 'nx:run-commands',
        options: { commands: [whole, ...hosted.map((command) => ({ command, forwardAllArgs: false }))], parallel: false },
    });
    return {
        projects: {
            '.': {
                root: '.',
                targets: {
                    'lint:ast-grep': versioned(
                        tree,
                        groups.flatMap(({ version, scoped }) => {
                            const below = off.filter((rule) => !covered(version)(rule));
                            return below.length > 0 ? [`${scan} --filter '^(${below.map(({ id }) => id).join('|')})$' ${scoped}`] : [];
                        }),
                    ),
                    rewrite: { command: `lockf -k .cache/ast-grep/rewrite.lock ${tree} --update-all` },
                    'typecheck:ty': versioned(
                        `ty check ${hosts.map(({ scope }) => `--exclude "${scope}"`).join(' ')}`,
                        groups.map(({ version, scoped }) => `ty check --python-version ${version} ${scoped}`),
                    ),
                    'typecheck:mypy': versioned(
                        `mypy ${excluded}`,
                        groups.map(({ version, scoped }) => `mypy --python-version ${version} ${scoped}`),
                    ),
                    'lint:refurb': versioned(
                        `python -m tools.refurb --quiet -- ${excluded}`,
                        groups.map(({ version, scoped }) => `python -m tools.refurb --quiet --python-version ${version} ${scoped}`),
                    ),
                },
            },
        },
    };
});
const _dependencies = Effect.fnUntraced(function* ({ fileMap, workspaceRoot }: CreateDependenciesContext) {
    const path = yield* Path.Path;
    const [graph, crates, swift, xcode] = yield* Effect.all(
        [
            _sources(workspaceRoot, _root(workspaceRoot)),
            _crates(workspaceRoot),
            _packages(workspaceRoot),
            Effect.flatMap(_glob(workspaceRoot, ['*/**/project.xcproj']), (files) =>
                Effect.validate(files, (file) => Effect.map(_decode(_XcodeProject, workspaceRoot, file), ({ packages }) => packages.flatMap((row) => (row.kind === 'local' ? [[file, path.join(path.dirname(path.dirname(file)), row.path, _SWIFT_MANIFEST)] as const] : []))), { concurrency: 'unbounded' }),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const owners = Object.fromEntries(Object.entries(fileMap.projectFileMap).flatMap(([project, files]) => files.map(({ file }) => [file, project] as const)));
    return [
        ...Array.getSomes([...Graph.values(Graph.edges(graph))].map(({ source, target }) => Option.product(Graph.getNode(graph, source), Graph.getNode(graph, target)))),
        ...crates.flatMap(({ manifest, dependencies }) => Array.getSomes(dependencies.map((dependency) => dependency.path)).map((member) => [manifest, path.join(path.relative(workspaceRoot, member), _CARGO_MANIFEST)] as const)),
        ...swift.flatMap(({ manifest, locals }) => locals.map((local) => [manifest, local] as const)),
        ...xcode.flat(),
    ].flatMap(([sourceFile, file]) =>
        pipe(
            Option.product(Record.get(owners, sourceFile), Record.get(owners, file)),
            Option.filter(([source, target]) => source !== target),
            Option.map(([source, target]): StaticDependency => ({ source, target, sourceFile, type: DependencyType.static })),
            Option.toArray,
        ),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

registerDynamicLanguage({ python });
const _runtime = ManagedRuntime.make(Layer.provideMerge(NodeChildProcessSpawner.layer, Layer.merge(NodeFileSystem.layer, NodePath.layerPosix)));
const _rendered = Effect.catchCause((cause: Cause.Cause<unknown>) => Effect.die(new Error(Cause.pretty(cause))));
const createNodes: CreateNodes = [
    `{${_PYPROJECT},*/**/{${Object.keys(_PROJECTS).join(',')}}}`,
    (files, options, context): Promise<CreateNodesResultArray> =>
        _runtime.runPromise(
            Effect.gen(function* () {
                const workspace = context.workspaceRoot;
                const [path, root, mise] = yield* Effect.all([Path.Path, Effect.cached(_root(workspace)), Effect.cached(_decode(_Mise, workspace, 'mise.toml'))]);
                const [hosts, crates, packages] = yield* Effect.all([Effect.cached(Effect.flatMap(FileSystem.FileSystem, (fs) => fs.readDirectory(path.join(workspace, 'apps')))), Effect.cached(_crates(workspace)), Effect.cached(_packages(workspace))]);
                const project = Effect.fnUntraced(
                    function* (file: string) {
                        const parsed = path.parse(file);
                        const [, configure] = yield* Effect.fromOption(Array.findFirst(Object.entries(_PROJECTS), ([key]) => key === parsed.base || key === `*${parsed.ext}`));
                        const configuration = { root: parsed.dir, ...(yield* configure(parsed, { file, workspace, root, mise, crates, packages })) };
                        const tags = Array.intersection(configuration.root.split('/'), yield* hosts).map((host) => `host:${host}`);
                        return { projects: { [configuration.root]: { ...configuration, tags: Array.union(configuration.tags, tags) } } };
                    },
                    Effect.catchNoSuchElement,
                    Effect.map(Option.getOrElse(() => ({}))),
                );
                return yield* Effect.promise(() => createNodesFromFiles((file) => _runtime.runPromise(_rendered(file === _PYPROJECT ? _pythonVersions(workspace, mise) : project(file))), files, options, context));
            }),
        ),
];
const createDependencies: CreateDependencies = (_options, context) => _runtime.runPromise(_rendered(_dependencies(context)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
