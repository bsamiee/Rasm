import { createRequire } from 'node:module';
import type { CreateNodesResult, TargetConfiguration } from '@nx/devkit';
import { Array, Boolean, Effect, Equal, Function, Option, Order, Path, Predicate, Record, Schema, String, Struct, Tuple } from 'effect';
import { quote } from 'shell-quote';
import { decodeFile, extendScriptEntry, libraryRoot, type Package, Sgconfig } from '#/eng/typescript/manifest';
import { attachedSuites, folderInput, imports, packageName, reach, rootTargets, suiteInput, swiftProject, TESTS } from '#/tools/nx/graph/projects';
import { APPS, glob, PYPROJECT, SWIFT_MANIFEST, type Workspace, WorkspaceError } from '#/tools/nx/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

interface GrammarBuild {
    readonly library: string;
    readonly binary: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSIONS = new Intl.Collator('en', { numeric: true });
const GRAMMARS = 'tools/ast-grep/grammars';
const GRAMMAR = 'grammar.js';
const AST_GREP_BUILT_INS: Readonly<Record<string, readonly string[]>> = { json: ['json'] };

// --- [MODELS] --------------------------------------------------------------------------

const TreeSitterGrammar = Schema.fromJsonString(Schema.Struct({ name: Schema.String, inherits: Schema.OptionFromOptionalKey(Schema.String), externals: Schema.Array(Schema.Unknown) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INPUTS]
const extensionInput = (extensions: readonly string[]) => `{workspaceRoot}/**/*.${extensions.length > 1 ? `{${extensions.join(',')}}` : extensions.join(',')}`;
const reaches = (version: string) => (release: string) => VERSIONS.compare(version, release) >= 0;
const versionGroups = (scopes: Effect.Success<Workspace['pythonScopes']>) =>
    Record.collect(Array.groupBy(scopes, Struct.get('version')), (version, members) => {
        const folders = members.map(Struct.get('scope'));
        return { version, scoped: quote(folders), inputs: folders.map(folderInput) };
    });
const importInputs = (graph: Effect.Success<ReturnType<typeof reach>>) => [...Object.keys(graph).map((file) => `{workspaceRoot}/${file}`), { externalDependencies: Array.dedupe(Object.values(graph).flatMap(Struct.get('specifiers')).map(packageName)) }];
const split = (target: string, children: Readonly<Record<string, TargetConfiguration>>): Record<string, TargetConfiguration> => {
    const [lead, ...rest] = Object.keys(children).map((child) => `${target}:${child}`);
    return {
        [target]: { dependsOn: [{ target: lead, params: 'forward' }, ...rest] },
        ...Record.mapKeys(children, (child) => `${target}:${child}`),
    };
};

// --- [TARGETS]
const pythonChecks = Effect.fnUntraced(function* ({ hosts, pyproject, pythonScopes }: Workspace) {
    const [path, folders, { requirements, deptry, fawltydeps }, scopes] = yield* Effect.all([Path.Path, hosts, pyproject, pythonScopes]);
    const dependencyGroups = Record.mapKeys(requirements, String.concat.bind(null, '--no-group '));
    const groups = versionGroups(scopes).map(({ version, scoped, inputs }) => {
        const environment = `.cache/python/${version}`;
        const interpreterInput = { runtime: `VIRTUAL_ENV=${environment} uv python find ${version} --show-version` };
        return {
            version,
            scoped,
            interpreterInput,
            inputs: [...inputs, interpreterInput],
            executable: `"$(uv python find ${version})"`,
            synced: [`export VIRTUAL_ENV=${environment} && uv sync --active --locked --python ${version}`, ...Record.keys(Record.filter(dependencyGroups, Function.flow(Struct.get('minimum'), Tuple.get(1), reaches(version), Boolean.not))), '&&'].join(' '),
        };
    });
    const excluded = quote(scopes.flatMap(({ scope }) => ['--exclude', `^${RegExp.escape(`${scope}/`)}`]));
    const commands = {
        root: {
            inputs: ['pythonFiles'],
            ty: 'ty check',
            mypy: `python -m tools.refurb -- ${excluded}`,
        },
        ...Record.fromIterableWith(groups, ({ version, executable, scoped, inputs, synced }) => [
            version,
            {
                inputs,
                ty: `${synced} ty check --python-version ${version} $(fd --extension py . ${scoped})`,
                mypy: `${synced} python -m tools.refurb --python-version ${version} ${scoped} -- --python-executable ${executable}`,
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
                onSome: ({ executable, synced, interpreterInput }) => ({ command: `${synced} ${executable} -m deptry`, inputs: [interpreterInput] }),
            },
        );
        return [`lint:deptry:${project.name}`, { ...checker, command: `${checker.command} ${quote(['--config', path.join(member, PYPROJECT), ...firstParty, ...roots])}`, inputs: ['python', ...roots.map(folderInput), ...checker.inputs] }] as const;
    };
    const deptryRuns = deptry.map(deptryCheck);
    const fawltyRuns = fawltydeps.members.map(
        ({ member, project }) => [`lint:fawltydeps:${project.name}`, { command: 'fawltydeps', options: { cwd: member, args: ['--pyenv', '"$VIRTUAL_ENV"'], env: { FAWLTYDEPS_CUSTOM_MAPPING: fawltydeps.mapping } }, inputs: ['python', folderInput(member), { env: 'VIRTUAL_ENV' }] }] as const,
    );
    return rootTargets({
        ...split(
            'typecheck:ty',
            Record.map(commands, ({ inputs, ty }) => ({ command: ty, inputs: [...inputs, 'python'], options: { env: { TY_UV: '1', UV_LOCKED: '1' } } })),
        ),
        ...split(
            'typecheck:mypy',
            Record.map(commands, ({ inputs, mypy }) => ({ command: mypy, inputs: [...inputs, '{workspaceRoot}/tools/refurb/**/*.py', 'python'] })),
        ),
        ...Array.match(deptryRuns, { onEmpty: () => ({}), onNonEmpty: (checks) => ({ 'lint:deptry': { dependsOn: [{ target: 'lint:deptry:*', params: 'forward' as const }] }, ...Object.fromEntries(checks) }) }),
        ...Array.match(fawltyRuns, { onEmpty: () => ({}), onNonEmpty: (checks) => ({ 'lint:fawltydeps': { dependsOn: [{ target: 'lint:fawltydeps:*', params: 'forward' as const }] }, ...Object.fromEntries(checks) }) }),
    });
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
        const attached = attachedSuites(suiteTargets, target.name);
        const project = swiftProject(['build:swiftpm', ...Array.intersection(Array.union(platformed, segments), folders).map(String.concat.bind(null, 'host:'))], [...(installed ? ['install'] : []), ...(Record.has(attached, 'test') ? ['check'] : [])]);
        return [
            target.path,
            {
                ...project,
                ...Array.match(Object.values(attached), { onEmpty: () => ({}), onNonEmpty: (held) => suiteInput(...held.map(Struct.get('path'))) }),
                root: target.path,
                name: target.name,
                targets: { ...project.targets, ...Record.map(attached, Struct.get('target')) },
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
                    'lint:periphery': { ...excluded, inputs: ['swift', 'periphery', ...sources.map(Struct.get('path')).map(folderInput)] },
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
const grammarTarget = Effect.fnUntraced(function* (workspaceRoot: string, scanned: readonly string[], [stem, { library, binary }]: readonly [string, GrammarBuild]) {
    const path = yield* Path.Path;
    const entry = path.join(GRAMMARS, stem, GRAMMAR);
    const owned = path.join(GRAMMARS, stem, 'scanner.c');
    const direct = yield* imports(workspaceRoot, entry);
    const graph = yield* reach(workspaceRoot, direct.files, { [entry]: direct });
    const held = scanned.includes(owned) ? [owned] : yield* upstreamScanners(workspaceRoot, entry, direct.specifiers);
    const work = path.join(binary, '$NX_TASK_HASH-$NX_INVOCATION_ANCESTOR_PIDS');
    return [
        `grammar:${stem}`,
        {
            options: {
                commands: [
                    {
                        command: `trap "cmake -E rm -rf ${work}" EXIT; trap "exit 1" HUP INT TERM; cmake -S ${GRAMMARS} -B ${work} -DCMAKE_BUILD_TYPE=Release -DGRAMMAR_SOURCE:FILEPATH="${path.relative(GRAMMARS, entry)}" -DNODE_MODULES:PATH="${path.relative(GRAMMARS, 'node_modules')}" -DSCANNER="${held.map((scanner) => path.relative(GRAMMARS, scanner)).join(';')}" {args.cmakeArguments} && cmake --build ${work} --config Release`,
                        forwardAllArgs: false,
                    },
                ],
            },
            inputs: [...importInputs(graph), ...[...held.filter((scanner) => scanner === owned), ...(Record.has(graph, path.join(GRAMMARS, 'compose.js')) ? [path.join(GRAMMARS, 'compose.c')] : [])].map((file) => `{workspaceRoot}/${file}`)],
            outputs: [`{workspaceRoot}/${library}`, `{workspaceRoot}/${binary}/src/{grammar.json,node-types.json}`],
        },
    ] as const satisfies readonly [string, TargetConfiguration];
});
const astGrep = Effect.fnUntraced(function* ({ workspaceRoot, scanExclusions }: Workspace) {
    const classes = 'tools/ast-grep/main.ts';
    const [path, { ruleDirs, utilDirs, customLanguages, languageGlobs }, exclusions, scanned, classGraph] = yield* Effect.all([Path.Path, Sgconfig.read(workspaceRoot), scanExclusions, glob(workspaceRoot, [`${GRAMMARS}/*/scanner.c`]), reach(workspaceRoot, [classes], {})], { concurrency: 'unbounded' });
    const libraries = Object.values(customLanguages).map(({ libraryPath }) => ({ library: libraryPath, ...path.parse(libraryPath) }));
    const builds = Record.fromIterableWith(libraries, ({ library, dir, name }) => [name, { library, binary: path.join(dir, name) }] as const);
    const store = yield* Effect.fromOption(Array.head(libraries), () => WorkspaceError.missing(Sgconfig.file, 'customLanguages libraryPath'));
    const grammars = yield* Effect.forEach(Record.toEntries(builds), (language) => grammarTarget(workspaceRoot, scanned, language), { concurrency: 'unbounded' });
    const scan = ['ast-grep scan --custom-languages allow', quote(exclusions.flatMap((pattern) => ['--globs', `!${pattern}`]))].join(' ');
    const sources = Object.values({ ...Record.map(customLanguages, Struct.get('extensions')), ...AST_GREP_BUILT_INS }).map(extensionInput);
    const ruleInputs = [...ruleDirs, ...utilDirs].map((dir) => `{workspaceRoot}/${dir}/**/*.{yml,yaml}`);
    const skipped = exclusions.map((pattern) => `!{workspaceRoot}/${pattern}`);
    const lock = `flock ${quote([path.join(store.dir, 'rewrite.lock')])}`;
    return {
        projects: {
            [GRAMMARS]: { root: GRAMMARS, name: path.basename(GRAMMARS), targets: { grammar: { dependsOn: ['grammar:*'] }, ...Object.fromEntries(grammars) } },
            '.': {
                root: '.',
                targets: {
                    ...split('lint:ast-grep', {
                        tree: {
                            command: `${scan} --error=no-suppress-all`,
                            syncGenerators: ['injection-utils', 'outline-catalogs'].map((generator) => `./tools/nx/sync/generators.json:${generator}`),
                            inputs: [...sources, ...Object.values(languageGlobs).flatMap(Array.map((pattern) => `{workspaceRoot}/**/${pattern}`)), ...skipped, ...ruleInputs, 'astGrep'],
                        },
                        classes: {
                            command: `node ${classes}`,
                            inputs: [
                                'typescript',
                                `{workspaceRoot}/${Sgconfig.file}`,
                                ...ruleInputs,
                                ...Array.getSomes(Object.values(customLanguages).map(Struct.get('outlineRules'))).map((file) => `{workspaceRoot}/${file}`),
                                ...importInputs(classGraph),
                                '{workspaceRoot}/tools/ast-grep/regex/**/*',
                                '{workspaceRoot}/Cargo.lock',
                                { dependentTasksOutputFiles: '**/src/{grammar,node-types}.json', transitive: true },
                                { dependentTasksOutputFiles: '**/*.so', transitive: true },
                            ],
                        },
                    }),
                    'rewrite:tree': { command: `${lock} ${scan} --update-all` },
                },
            },
        },
    } satisfies CreateNodesResult;
});
const neutrality = Effect.fnUntraced(function* ({ workspaceRoot, packages }: Workspace) {
    const config = 'eng/typescript/root-neutrality.config.ts';
    const [path, manifests, graph] = yield* Effect.all([Path.Path, packages, reach(workspaceRoot, [config], {})]);
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

export { GRAMMARS, neutrality, roots };
