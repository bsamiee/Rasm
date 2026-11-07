import { createRequire } from 'node:module';
import type { CreateNodesResult, TargetConfiguration } from '@nx/devkit';
import { Array, Cache, Effect, Equal, FileSystem, Option, Order, Path, Predicate, Record, Schema, String, Struct } from 'effect';
import { quote } from 'shell-quote';
import { decodeFile, extendScriptEntry, libraryRoot, type Package, Sgconfig } from '#/eng/typescript/manifest';
import { attachedSuites, folderInput, imports, packageName, reach, rootTargets, swiftProject, TESTS } from '#/tools/nx/graph/projects';
import { glob, PYPROJECT, SWIFT_MANIFEST, type Workspace } from '#/tools/nx/workspace';

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
const pythonChecks = Effect.fnUntraced(function* ({ workspaceRoot, hosts, pyproject, pythonScopes }: Workspace) {
    const [path, folders, { fawltydeps }, scopes] = yield* Effect.all([Path.Path, hosts, pyproject, pythonScopes]);
    const restore = 'restore:python';
    const groups = Record.collect(Array.groupBy(scopes, Struct.get('version')), (version, members) => {
        const directories = Array.dedupe(members.flatMap(({ scope, suites }) => [scope, ...suites]));
        const fixtures = Array.dedupe(members.flatMap(Struct.get('fixtures')));
        const scoped = quote([...directories, ...fixtures]);
        const environment = `.cache/python/${version}`;
        const interpreterInput = { runtime: `uv python find --system ${version} --show-version` };
        const selection = `--no-active --python ${version}`;
        return {
            version,
            directories,
            fixtures,
            interpreterInput,
            install: `UV_PROJECT_ENVIRONMENT=${environment} uv sync --locked ${selection} --no-default-groups --group host-checks`,
            check: {
                target: { inputs: [...directories.map(folderInput), ...fixtures.map((file) => `{workspaceRoot}/${file}`), interpreterInput], dependsOn: [restore] },
                ty: `VIRTUAL_ENV=${environment} ty check ${scoped}`,
                mypy: `python -m tools.refurb --python-version ${version} ${scoped} -- --python-executable "$(VIRTUAL_ENV=${environment} uv python find ${version})"`,
            },
            environment,
        };
    });
    const hostDirectories = Array.dedupe(groups.flatMap(Struct.get('directories')));
    const hostFixtures = Array.dedupe(groups.flatMap(Struct.get('fixtures'))).filter((file) => file.split(path.sep).some((part) => folders.includes(part)));
    const tyExclusions = quote([...hostDirectories, ...hostFixtures].flatMap((file) => ['--exclude', file]));
    const mypyExclusions = quote([...hostDirectories.map((folder) => `^${RegExp.escape(`${folder}/`)}`), ...hostFixtures.map((file) => `^${RegExp.escape(file)}$`)].flatMap((pattern) => ['--exclude', pattern]));
    const commands = [
        {
            suffix: '',
            target: { inputs: ['pythonFiles'] },
            ty: `ty check ${tyExclusions}`,
            mypy: `python -m tools.refurb -- ${mypyExclusions}`,
        },
        ...groups.map(({ version, check }) => ({ suffix: `:${version}`, ...check })),
    ];
    const fawltyRuns = fawltydeps.members.map(({ member, project }) => {
        const hosted = Option.flatMap(Array.findFirst(scopes, Predicate.Struct({ scope: Equal.equals(member) })), ({ version }) => Array.findFirst(groups, Predicate.Struct({ version: Equal.equals(version) })));
        const checker = Option.match(hosted, {
            onNone: () => ({ command: 'fawltydeps --check-unused', inputs: [] }),
            onSome: ({ environment, interpreterInput }) => ({ command: `fawltydeps --check-unused --pyenv ${path.join(workspaceRoot, environment)}`, dependsOn: [restore], inputs: [interpreterInput] }),
        });
        return [`lint:fawltydeps:${project.name}`, { ...checker, options: { cwd: member, env: { FAWLTYDEPS_CUSTOM_MAPPING: fawltydeps.mapping } }, inputs: ['python', folderInput(member), ...checker.inputs] }] as const;
    });
    return rootTargets({
        ...Array.match(groups, { onEmpty: () => ({}), onNonEmpty: (held) => ({ [restore]: { executor: 'nx:run-commands', options: { commands: held.map(Struct.get('install')) } } }) }),
        ...Record.fromIterableWith(commands, ({ suffix, target, ty }) => [`typecheck:ty${suffix}`, { ...target, command: ty, inputs: [...target.inputs, 'python'], options: { env: { TY_UV: '1', UV_LOCKED: '1' } } }]),
        ...Record.fromIterableWith(commands, ({ suffix, target, mypy }) => [`typecheck:mypy${suffix}`, { ...target, command: mypy, inputs: [...target.inputs, '{workspaceRoot}/tools/refurb/**/*.py', 'python'] }]),
        ...Object.fromEntries(fawltyRuns),
    });
});
const swiftModules = Effect.fnUntraced(function* ({ swift, hosts }: Workspace) {
    const [{ targets, platforms, tests }, folders] = yield* Effect.all([swift, hosts]);
    const [suites, sources] = tests;
    const platformed = platforms.map(Struct.get('name'));
    const suiteTargets = Record.fromIterableWith(suites, (suite) => [suite.name, { suite, files: [folderInput(suite.path), ...suites.filter(({ path }) => path.startsWith(`${suite.path}/`)).map(({ path }) => `!${folderInput(path)}`)] }]);
    const production = sources.filter(({ path }) => !path.startsWith(`${TESTS}/`));
    const excluded = Array.match(Array.difference(targets, production), { onEmpty: () => ({}), onNonEmpty: (all) => ({ options: { args: ['--exclude-targets', ...all.map(Struct.get('c99name'))] } }) });
    const modules = sources.map((target) => {
        const segments = target.path.split('/');
        const installed = target.type === 'executable' && segments[0] !== TESTS;
        const attached = attachedSuites(suiteTargets, target.name);
        const project = swiftProject(['build:swiftpm', ...Array.intersection(Array.union(platformed, segments), folders).map(String.concat.bind(null, 'host:'))], installed ? ['install'] : []);
        const testTargets = Record.map(attached, ({ suite, files }) => {
            const projects = Array.difference(suite.locals, [target.name]);
            const inputs: NonNullable<TargetConfiguration['inputs']> = [
                ...files,
                ...(projects.length === 0
                    ? []
                    : [
                          { input: 'default', projects },
                          { input: 'default', projects, dependencies: true },
                      ]),
            ];
            return { inputs, options: { args: ['--test-product', suite.name] } };
        });
        return [
            target.path,
            {
                ...project,
                root: target.path,
                name: target.name,
                implicitDependencies: [...target.locals],
                targets: {
                    ...project.targets,
                    ...testTargets,
                },
            },
        ] as const;
    });
    return {
        projects: {
            ...Object.fromEntries(modules),
            '.': {
                root: '.',
                tags: Array.intersection(platformed, folders).map(String.concat.bind(null, 'host:')),
                targets: {
                    'typecheck:swift': { inputs: ['swift', ...sources.map(Struct.get('path')).map(folderInput)] },
                    'lint:periphery': { ...excluded, inputs: ['swift', 'periphery', ...production.map(Struct.get('path')).map(folderInput)] },
                },
            },
        },
    };
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
            command: `trap "cmake -E rm -rf ${work}" EXIT; trap "exit 1" HUP INT TERM; cmake -DBINARY_DIR:PATH=${work} -DGRAMMAR_SOURCE:FILEPATH="${path.relative(GRAMMARS, entry)}" -DNODE_MODULES:PATH="${path.relative(GRAMMARS, 'node_modules')}" -DSCANNER="${held.map((scanner) => path.relative(GRAMMARS, scanner)).join(';')}" -P ${GRAMMARS}/CMakeLists.txt${catalogs.map(({ language, file, source }) => ` && node ${OUTLINE} ${language} ${source} ${path.join(work, path.basename(file))} && cmake -E make_directory ${path.dirname(file)} && cmake -E rename ${path.join(work, path.basename(file))} ${file}`).join('')}`,
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
const astGrep = Effect.fnUntraced(function* ({ workspaceRoot, scanExclusions }: Workspace) {
    const classes = 'tools/ast-grep/main.ts';
    const parsed = yield* Cache.make({ capacity: Number.POSITIVE_INFINITY, lookup: imports(workspaceRoot) });
    const read = (file: string) => Cache.get(parsed, file);
    const [path, fs, { ruleDirs, utilDirs, customLanguages, languageGlobs }, exclusions, scanned, classGraph, outliner] = yield* Effect.all(
        [Path.Path, FileSystem.FileSystem, Sgconfig.read(workspaceRoot), scanExclusions, glob(workspaceRoot, [`${GRAMMARS}/*/scanner.c`]), reach(read, [classes]), reach(read, [OUTLINE])],
        {
            concurrency: 'unbounded',
        },
    );
    const outlines = Sgconfig.outlineCatalogs(path, customLanguages);
    const rows = Record.collect(customLanguages, (language, { libraryPath }) => ({ ...path.parse(libraryPath), library: libraryPath, catalog: Array.findFirst(outlines, (outline) => outline.language === language) }));
    const builds = Record.map(Array.groupBy(rows, Struct.get('name')), (held) => ({ library: held[0].library, binary: path.join(held[0].dir, held[0].name), catalogs: Array.getSomes(held.map(Struct.get('catalog'))) }));
    const catalogs = Object.values(builds).flatMap(Struct.get('catalogs'));
    const grammars = yield* Effect.forEach(Record.toEntries(builds), (build) => grammarTarget(workspaceRoot, read, scanned, outliner, build), { concurrency: 'unbounded' });
    const scan = ['ast-grep scan --custom-languages allow', quote(exclusions.flatMap((pattern) => ['--globs', `!${pattern}`]))].join(' ');
    const ruleLanguages = (yield* Effect.forEach(ruleDirs, (folder) => fs.readDirectory(path.join(workspaceRoot, folder)))).flat();
    const sources = Object.values({ ...Record.map(customLanguages, Struct.get('extensions')), ...Record.filter(AST_GREP_BUILT_INS, (_, language) => ruleLanguages.includes(language)) }).map(extensionInput);
    const rules = (directories: readonly string[]) => directories.map((dir) => `{workspaceRoot}/${dir}/**/*.{yml,yaml}`);
    const ruleInputs = rules([...ruleDirs, ...utilDirs]);
    const skipped = exclusions.map((pattern) => `!{workspaceRoot}/${pattern}`);
    const built = [{ projects: [path.basename(GRAMMARS)], target: 'grammar:*' }];
    return {
        projects: {
            [GRAMMARS]: { root: GRAMMARS, name: path.basename(GRAMMARS), targets: Object.fromEntries(grammars) },
            '.': {
                root: '.',
                targets: {
                    'lint:nx': { inputs: rules(ruleDirs) },
                    'lint:ast-grep': {
                        dependsOn: built,
                        command: `${scan} --error=no-suppress-all`,
                        syncGenerators: ['./tools/nx/sync/generators.json:injection-utils'],
                        inputs: [...sources, ...Object.values(languageGlobs).flatMap(Array.map((pattern) => `{workspaceRoot}/**/${pattern}`)), ...skipped, ...ruleInputs, 'astGrep'],
                    },
                    'lint:ast-grep:classes': {
                        dependsOn: built,
                        command: `node ${classes}`,
                        inputs: ['typescript', `{workspaceRoot}/${Sgconfig.file}`, ...ruleInputs, ...catalogs.map(({ source }) => `{workspaceRoot}/${source}`), ...importInputs(classGraph), '{workspaceRoot}/tools/ast-grep/regex/**/*', 'rust', { dependentTasksOutputFiles: '**/*.{so,json}', transitive: true }],
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
    const entries = (dir: string, manifest: typeof Package.schema.Type) => Array.getSomes([libraryRoot(manifest), extendScriptEntry(manifest)]).map((entry) => path.join(dir, entry));
    const libraries = Array.sortWith(Array.getSomes(Record.collect(manifests, (dir, manifest) => Option.as(libraryRoot(manifest), { name: manifest.name, entries: entries(dir, manifest) }))), Struct.get('name'), Order.String);
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
const roots = { [PYPROJECT]: pythonChecks, [SWIFT_MANIFEST]: swiftModules, [Sgconfig.file]: astGrep } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { neutrality, roots };
