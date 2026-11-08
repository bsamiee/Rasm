import { createRequire } from 'node:module';
import type { CreateNodesResult, TargetConfiguration } from '@nx/devkit';
import { decodeFile, librarySurface, Sgconfig } from '@rasm/eng';
import { Array, Cache, Effect, FileSystem, Option, Order, Path, Record, Schema, String, Struct } from 'effect';
import { quote } from 'shell-quote';
import { BLENDER_MANIFEST, folderInput, type Inference, imports, packageName, reach } from '#/graph/projects.ts';
import { glob, PYPROJECT, UV_LOCK, type Workspace } from '#/workspace.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface GrammarBuild {
    readonly library: string;
    readonly binary: string;
    readonly catalogs: readonly { readonly language: string; readonly file: string; readonly source: string }[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const GRAMMARS = 'tools/ast-grep/grammars';
const OUTLINE = `${Sgconfig.outline}/main.ts`;
const GRAMMAR = 'grammar.js';
const AST_GREP_BUILT_INS: Readonly<Record<string, readonly string[]>> = { json: ['json'] };

// --- [MODELS] --------------------------------------------------------------------------

const TreeSitterGrammar = Schema.fromJsonString(Schema.Struct({ name: Schema.String, inherits: Schema.OptionFromOptionalKey(Schema.String), externals: Schema.Array(Schema.Unknown) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INPUTS]

const extensionInput = (extensions: readonly string[]) => `{workspaceRoot}/**/*.${extensions.length > 1 ? `{${extensions.join(',')}}` : extensions.join(',')}`;
const importInputs = (graph: Effect.Success<ReturnType<typeof reach>>) => [...Object.keys(graph).map((file) => `{workspaceRoot}/${file}`), { externalDependencies: Array.dedupe(Object.values(graph).flatMap(Struct.get('specifiers')).map(packageName)) }];

// --- [TARGETS]

const pythonChecks = Effect.fnUntraced(function* ({ python, packages, configFiles }: Inference) {
    const [path, { roots, members, owners }, manifests] = yield* Effect.all([Path.Path, python, packages]);
    const [{ name }, workspace] = yield* Effect.all([Effect.fromOption(Record.get(manifests, '.')), Effect.fromOption(Record.get(roots, '.'))]);
    const restore = 'restore:python';
    const refurb = 'rasm-refurb';
    const nested = Record.remove(roots, '.');
    const folders = Record.keys(nested);
    const extensions = Object.values(members).filter(({ dir }) => configFiles.has(path.join(dir, BLENDER_MANIFEST)));
    const required = extensions.flatMap(({ root, requires }) => [root, ...Array.getSomes(requires.map((held) => Record.get(owners, held))).map(Struct.get('root'))]);
    const hosted = Array.intersection(folders, required);
    const located = Array.getSomes(Object.values(members).map(({ dir, root }) => Option.map(Record.get(nested, root), ({ release }) => ({ dir, release, environment: path.relative(dir, path.join(root, '.venv')) }))));
    const ty = { TY_UV: '1', UV_LOCKED: '1' };
    const checked = { dependsOn: [{ projects: [name], target: restore }], inputs: ['default', '^production', 'python', workspace.lock] };
    const inputs = ['pythonFiles', ...folders.map((folder) => `!${folderInput(folder)}`), '{workspaceRoot}/**/pyproject.toml', workspace.lock, workspace.interpreter, { externalDependencies: [] }];
    const tyExclusions = quote(folders.flatMap((folder) => ['--exclude', folder]));
    const mypyExclusions = quote(folders.flatMap((folder) => ['--exclude', `^${RegExp.escape(`${folder}/`)}`]));
    return {
        projects: {
            ...Record.fromIterableWith(located, ({ dir, release, environment }) => [
                dir,
                {
                    root: dir,
                    targets: {
                        'typecheck:ty': { ...checked, command: 'ty check .', options: { cwd: '{projectRoot}', env: { ...ty, VIRTUAL_ENV: environment } } },
                        'typecheck:mypy': {
                            ...checked,
                            command: `${refurb} --config-file ${path.relative(dir, PYPROJECT)} --python-version ${release} . -- --python-executable ${path.join(environment, 'bin', 'python')}`,
                            inputs: [...checked.inputs, { input: 'production', projects: [refurb] }],
                            options: { cwd: '{projectRoot}', env: { VIRTUAL_ENV: environment } },
                        },
                    },
                },
            ]),
            '.': {
                root: '.',
                targets: {
                    ...Array.match(folders, { onEmpty: () => ({}), onNonEmpty: (held) => ({ [restore]: { executor: 'nx:run-commands', options: { commands: held.map((root) => `cd ${quote([root])} && VIRTUAL_ENV=.venv uv sync --locked`) } } }) }),
                    'typecheck:ty': { command: `ty check ${tyExclusions}`, inputs, options: { env: { ...ty, VIRTUAL_ENV: '.venv' } } },
                    'typecheck:mypy': { command: `${refurb} -- ${mypyExclusions}`, inputs },
                    'upgrade:python': {
                        executor: 'nx:run-commands',
                        options: {
                            commands: ['pixi update', ...hosted.map((root) => `rasm-eng upgrade ${quote([root])}`), ...folders.map((root) => `uv lock --upgrade --directory ${quote([root])}`), 'uv lock --upgrade', 'pixi run --locked uv sync --locked'],
                            parallel: false,
                        },
                    },
                },
            },
        },
    } satisfies CreateNodesResult;
});
const upstreamScanners = Effect.fnUntraced(function* (workspaceRoot: string, file: string, specifiers: readonly string[]) {
    const path = yield* Path.Path;
    const { resolve } = createRequire(path.join(workspaceRoot, file));
    const upstreams = yield* Effect.forEach(
        specifiers.filter(String.endsWith(`/${GRAMMAR}`)).map((specifier) => path.relative(workspaceRoot, path.dirname(resolve(specifier)))),
        (dir) => Effect.map(decodeFile(TreeSitterGrammar, workspaceRoot, dir, 'src', 'grammar.json'), Struct.assign({ dir })),
        { concurrency: 'unbounded' },
    );
    const bases = new Set(Array.getSomes(upstreams.map(Struct.get('inherits'))));
    return upstreams.filter(({ name, externals }) => externals.length > 0 && !bases.has(name)).map(({ dir }) => path.join(dir, 'src', 'scanner.c'));
});
const grammarTarget = Effect.fnUntraced(function* (workspaceRoot: string, read: ReturnType<typeof imports>, scanned: readonly string[], outliner: Effect.Success<ReturnType<typeof reach>>, [stem, { library, binary, catalogs }]: readonly [string, GrammarBuild]) {
    const path = yield* Path.Path;
    const entry = path.join(GRAMMARS, stem, GRAMMAR);
    const owned = path.join(GRAMMARS, stem, 'scanner.c');
    const direct = yield* read(entry);
    const graph = yield* reach(read, direct.files, { [entry]: direct });
    const held = scanned.includes(owned) ? [owned] : yield* upstreamScanners(workspaceRoot, entry, direct.specifiers);
    const work = path.join(binary, '$NX_TASK_HASH-$NX_INVOCATION_ANCESTOR_PIDS');
    return [
        `grammar:${stem}`,
        {
            command: `trap "cmake -E rm -rf ${work}" EXIT; trap "exit 1" HUP INT TERM; cmake -DBINARY_DIR:PATH=${work} -DGRAMMAR_SOURCE:FILEPATH="${path.relative(GRAMMARS, entry)}" -DNODE_MODULES:PATH="${path.relative(GRAMMARS, path.join(path.dirname(GRAMMARS), 'node_modules'))}" -DSCANNER="${held.map((scanner) => path.relative(GRAMMARS, scanner)).join(';')}" -P ${GRAMMARS}/CMakeLists.txt${catalogs.map(({ language, file, source }) => ` && node ${OUTLINE} ${language} ${source} ${path.join(work, path.basename(file))} && cmake -E make_directory ${path.dirname(file)} && cmake -E rename ${path.join(work, path.basename(file))} ${file}`).join('')}`,
            options: { forwardAllArgs: false },
            cache: true,
            inputs: [
                ...importInputs(graph),
                ...(Array.isReadonlyArrayEmpty(catalogs) ? [] : importInputs(outliner)),
                ...[...held.filter((scanner) => scanner === owned), ...(Record.has(graph, path.join(GRAMMARS, 'compose.js')) ? [path.join(GRAMMARS, 'compose.c')] : []), ...catalogs.map(Struct.get('source'))].map((file) => `{workspaceRoot}/${file}`),
                `{workspaceRoot}/${path.join(GRAMMARS, 'CMakeLists.txt')}`,
                'nodePlatform',
                ...['tree-sitter --version', 'cmake --version', 'cc --version'].map((runtime) => ({ runtime })),
                ...['CC', 'CFLAGS', 'LDFLAGS', 'SDKROOT', 'MACOSX_DEPLOYMENT_TARGET'].map((env) => ({ env })),
            ],
            outputs: [`{workspaceRoot}/${library}`, `{workspaceRoot}/${binary}/src/{grammar.json,node-types.json}`, ...catalogs.map(({ file }) => `{workspaceRoot}/${file}`)],
        },
    ] as const satisfies readonly [string, TargetConfiguration];
});
const astGrep = Effect.fnUntraced(function* ({ workspaceRoot, crates }: Workspace) {
    const classes = 'tools/ast-grep/main.ts';
    const lock = 'pnpm-lock.yaml';
    const parsed = yield* Cache.make({ capacity: Number.POSITIVE_INFINITY, lookup: imports(workspaceRoot) });
    const read = (file: string) => Cache.get(parsed, file);
    const [path, fs, { ruleDirs, utilDirs, customLanguages, languageGlobs }, scanned, classGraph, outliner, members] = yield* Effect.all([Path.Path, FileSystem.FileSystem, Sgconfig.read(workspaceRoot), glob(workspaceRoot, [`${GRAMMARS}/*/scanner.c`]), reach(read, [classes]), reach(read, [OUTLINE]), crates], {
        concurrency: 'unbounded',
    });
    const outlines = Sgconfig.outlineCatalogs(path, customLanguages);
    const rows = Record.collect(customLanguages, (language, { libraryPath }) => ({ ...path.parse(libraryPath), library: libraryPath, catalog: Array.findFirst(outlines, (outline) => outline.language === language) }));
    const builds = Record.map(Array.groupBy(rows, Struct.get('name')), (held) => ({ library: held[0].library, binary: path.join(held[0].dir, held[0].name), catalogs: Array.getSomes(held.map(Struct.get('catalog'))) }));
    const catalogs = Object.values(builds).flatMap(Struct.get('catalogs'));
    const grammars = yield* Effect.forEach(Record.toEntries(builds), (build) => grammarTarget(workspaceRoot, read, scanned, outliner, build), { concurrency: 'unbounded' });
    const scan = ['ast-grep scan --custom-languages allow', quote(['--globs', `!${lock}`])].join(' ');
    const ruleLanguages = (yield* Effect.forEach(ruleDirs, (folder) => fs.readDirectory(path.join(workspaceRoot, folder)))).flat();
    const sources = Object.values({ ...Record.map(customLanguages, Struct.get('extensions')), ...Record.filter(AST_GREP_BUILT_INS, (_, language) => ruleLanguages.includes(language)) }).map(extensionInput);
    const rules = (directories: readonly string[]) => directories.map((dir) => `{workspaceRoot}/${dir}/**/*.{yml,yaml}`);
    const ruleInputs = rules([...ruleDirs, ...utilDirs]);
    const built = [{ projects: [path.basename(GRAMMARS)], target: 'grammar:*' }];
    const classifier = members.filter(({ manifest }) => !path.relative(path.dirname(classes), manifest).startsWith('..')).map(Struct.get('name'));
    return {
        projects: {
            [GRAMMARS]: { root: GRAMMARS, name: path.basename(GRAMMARS), targets: Object.fromEntries(grammars) },
            '.': {
                root: '.',
                targets: {
                    'lint:nx': { inputs: [`{workspaceRoot}/${Sgconfig.file}`, ...rules(ruleDirs)] },
                    'lint:ast-grep': {
                        dependsOn: built,
                        command: `${scan} --error=no-suppress-all`,
                        syncGenerators: ['@rasm/nx:injection-utils'],
                        inputs: [...sources, ...Object.values(languageGlobs).flatMap(Array.map((pattern) => `{workspaceRoot}/**/${pattern}`)), `!{workspaceRoot}/${lock}`, ...ruleInputs, 'astGrep'],
                    },
                    'lint:ast-grep:classes': {
                        dependsOn: built,
                        command: `node ${classes}`,
                        inputs: ['typescript', `{workspaceRoot}/${Sgconfig.file}`, ...ruleInputs, ...catalogs.map(({ source }) => `{workspaceRoot}/${source}`), ...importInputs(classGraph), { input: 'default', projects: classifier }, 'rust', { dependentTasksOutputFiles: '**/*.{so,json}', transitive: true }],
                    },
                    'rewrite:tree': { command: `${scan} --update-all`, dependsOn: built, parallelism: false },
                },
            },
        },
    } satisfies CreateNodesResult;
});
const neutrality = Effect.fnUntraced(function* ({ workspaceRoot, packages }: Workspace) {
    const config = 'eng/typescript/root-neutrality.config.ts';
    const [path, manifests, graph] = yield* Effect.all([Path.Path, packages, reach(imports(workspaceRoot), [config])]);
    const surfaces = Array.getSomes(Record.collect(manifests, (dir, manifest) => Option.map(librarySurface(manifest), (surface) => ({ dir, name: manifest.name, ...surface }))));
    const libraries = Array.sortWith(
        surfaces.map(({ dir, name, root, extendscript }) => ({ name, entries: [root, ...extendscript].map((entry) => path.join(dir, entry)) })),
        Struct.get('name'),
        Order.String,
    );
    return Array.match(libraries.map(Struct.get('name')), {
        onEmpty: () => ({}),
        onNonEmpty: (projects) => ({
            'lint:dependency-cruiser': {
                command: `depcruise --config ${config} --output-type err-long ${quote(libraries.flatMap(Struct.get('entries')))}`,
                inputs: [{ input: 'production', projects }, { input: 'production', projects, dependencies: true }, 'typescript', '{workspaceRoot}/pnpm-workspace.yaml', ...importInputs(graph), { externalDependencies: ['typescript'] }],
            },
        }),
    });
});
const roots = { [UV_LOCK]: pythonChecks, [Sgconfig.file]: astGrep } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { neutrality, roots };
