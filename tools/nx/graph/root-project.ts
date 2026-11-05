import { createRequire } from 'node:module';
import type { CreateNodesResult, TargetConfiguration } from '@nx/devkit';
import { Array, Boolean, Effect, Equal, Function, Option, Order, Path, Predicate, Record, Result, Schema, String, Struct, Tuple } from 'effect';
import { quote } from 'shell-quote';
import { parseAllDocuments } from 'yaml';
import { decodeFile, extendScriptEntry, libraryRoot, type Package, Sgconfig } from '#/eng/typescript/manifest';
import { TextFormat } from '#/eng/typescript/text-format';
import { attachedSuites, folderInput, imports, packageName, reach, rootTargets, suiteInput, swiftProject, TESTS } from '#/tools/nx/graph/projects';
import { APPS, glob, PYPROJECT, Release, SWIFT_MANIFEST, type Workspace, WorkspaceError } from '#/tools/nx/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

interface GrammarBuild {
    readonly library: string;
    readonly binary: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSIONS = new Intl.Collator('en', { numeric: true });
const GRAMMARS = 'tools/ast-grep/grammars';
const GRAMMAR = 'grammar.js';
const GENERATORS = 'tools/nx/sync/generators.json';
const AST_GREP_BUILT_INS: Readonly<Record<string, readonly string[]>> = { json: ['json'] };

// --- [MODELS] --------------------------------------------------------------------------

const NxGenerators = Schema.fromJsonString(Schema.Struct({ generators: Schema.Record(Schema.String, Schema.Struct({ factory: Schema.String, schema: Schema.String })) }));
const YamlStream = TextFormat('YAML stream', (text) => parseAllDocuments(text).map((document) => Result.getOrThrow(Array.match(document.errors, { onEmpty: () => Result.succeed(document.toJS()), onNonEmpty: ([error]) => Result.fail(error) }))));
const AstGrepRule = (stem: string) =>
    Schema.Struct({
        id: Schema.String.pipe(Schema.withDecodingDefaultKey(Effect.succeed(stem))),
        language: Schema.String,
        metadata: Schema.Struct({ below: Schema.OptionFromOptionalKey(Release), from: Schema.OptionFromOptionalKey(Release) }).pipe(Schema.encodeKeys({ below: 'python-below', from: 'python-from' }), Schema.withDecodingDefaultKey(Effect.succeed({}))),
    });
const AstGrepRuleSet = Schema.Array(AstGrepRule(''));
const TreeSitterGrammar = Schema.fromJsonString(Schema.Struct({ name: Schema.String, inherits: Schema.OptionFromOptionalKey(Schema.String), externals: Schema.Array(Schema.Unknown) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INPUTS]
const extensionInput = (extensions: readonly string[], within = '{workspaceRoot}') => `${within}/**/*.${extensions.length > 1 ? `{${extensions.join(',')}}` : extensions.join(',')}`;
const reaches = (version: string) => (release: string) => VERSIONS.compare(version, release) >= 0;
const versionGroups = (scopes: Effect.Success<Workspace['pythonScopes']>) =>
    Record.collect(Array.groupBy(scopes, Struct.get('version')), (version, members) => {
        const folders = members.map(Struct.get('scope'));
        return { version, folders, scoped: quote(folders), inputs: folders.map(folderInput) };
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
            environment,
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
                onSome: ({ executable, environment, interpreterInput }) => ({ command: `${executable} -m deptry`, options: { env: { VIRTUAL_ENV: environment } }, inputs: [interpreterInput] }),
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
                    `cmake -S ${GRAMMARS} -B ${work} -DCMAKE_BUILD_TYPE=Release -DGRAMMAR_SOURCE:FILEPATH="${path.relative(GRAMMARS, entry)}" -DNODE_MODULES:PATH="${path.relative(GRAMMARS, 'node_modules')}" -DSCANNER="${held.map((scanner) => path.relative(GRAMMARS, scanner)).join(';')}" {args.cmakeArguments}`,
                    { command: `cmake --build ${work} --config Release`, forwardAllArgs: false },
                    { command: `cmake -E rm -rf ${work}`, forwardAllArgs: false },
                ],
            },
            inputs: [...importInputs(graph), ...[...held.filter((scanner) => scanner === owned), ...(Record.has(graph, path.join(GRAMMARS, 'compose.js')) ? [path.join(GRAMMARS, 'compose.c')] : [])].map((file) => `{workspaceRoot}/${file}`)],
            outputs: [`{workspaceRoot}/${library}`, `{workspaceRoot}/${binary}/src/{grammar.json,node-types.json}`],
        },
    ] as const satisfies readonly [string, TargetConfiguration];
});
const astGrep = Effect.fnUntraced(function* ({ workspaceRoot, python, pythonScopes, scanExclusions, memoized }: Workspace) {
    const classes = 'tools/ast-grep/main.ts';
    const [path, { ruleDirs, utilDirs, customLanguages, languageGlobs, languageInjections }, { versions }, scopes, exclusions, scanned, classGraph] = yield* Effect.all(
        [Path.Path, Sgconfig.read(workspaceRoot), python, pythonScopes, scanExclusions, glob(workspaceRoot, [`${GRAMMARS}/*/scanner.c`]), reach(workspaceRoot, [classes], {})],
        { concurrency: 'unbounded' },
    );
    const libraries = Object.values(customLanguages).map(({ libraryPath }) => ({ library: libraryPath, ...path.parse(libraryPath) }));
    const builds = Record.fromIterableWith(libraries, ({ library, dir, name }) => [name, { library, binary: path.join(dir, name) }] as const);
    const store = yield* Effect.fromOption(Array.head(libraries), () => WorkspaceError.missing(Sgconfig.file, 'customLanguages libraryPath'));
    const ruleFiles = ruleDirs.map((dir) => `${dir}/**/*.{yml,yaml}`);
    const [grammars, rules] = yield* Effect.all(
        [
            Effect.forEach(Record.toEntries(builds), (language) => grammarTarget(workspaceRoot, scanned, language), { concurrency: 'unbounded' }),
            memoized(
                'ast-grep rules',
                AstGrepRuleSet,
                ruleFiles,
                glob(workspaceRoot, ruleFiles).pipe(Effect.flatMap(Effect.validate((file: string) => decodeFile(YamlStream.pipe(Schema.decodeTo(Schema.Array(AstGrepRule(path.basename(file, path.extname(file)))))), workspaceRoot, file), { concurrency: 'unbounded' })), Effect.map(Array.flatten)),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const [interpreter] = versions;
    const runs =
        (version: string) =>
        ({ metadata: { below, from } }: (typeof rules)[number]) =>
            !Option.exists(below, reaches(version)) && Option.match(from, { onNone: () => true, onSome: reaches(version) });
    const off = Array.dedupe(Array.getSomes(Array.cartesianWith(rules, [interpreter, ...scopes.map(Struct.get('version'))], (rule, version) => Option.liftPredicate(rule, Predicate.not(runs(version))))));
    const ids = (held: typeof rules) => quote(['--filter', `^(${held.map(Struct.get('id')).join('|')})$`]);
    const scan = (flags: readonly string[]) => ['ast-grep scan --custom-languages allow', ...flags].join(' ');
    const walk = (flags: readonly string[]) => scan([...flags, quote(exclusions.flatMap((pattern) => ['--globs', `!${pattern}`]))]);
    const offs = off.map(({ id }) => quote([`--off=${id}`]));
    const languages = Array.difference(Array.union(rules.map(Struct.get('language')), languageInjections.map(Struct.get('hostLanguage'))), Record.keys(customLanguages));
    const builtIns = Record.filter(AST_GREP_BUILT_INS, (_, language) => languages.includes(language));
    yield* Array.match(Array.difference(languages, Record.keys(builtIns)), { onEmpty: () => Effect.void, onNonEmpty: (missing) => Effect.fail(WorkspaceError.missing(missing.join(', '), 'ast-grep built-in extensions')) });
    const sources = { ...Record.map(customLanguages, Struct.get('extensions')), ...builtIns };
    const read = (held: readonly string[], within?: string) => Object.values(Record.filter(sources, (_, language) => held.includes(language))).map((extensions) => extensionInput(extensions, within));
    const ruleInputs = [...ruleDirs, ...utilDirs].map((dir) => `{workspaceRoot}/${dir}/**/*.{yml,yaml}`);
    const skipped = exclusions.map((pattern) => `!{workspaceRoot}/${pattern}`);
    const within = (folders: readonly string[]) => (held: readonly string[]) => [...folders.flatMap((folder) => read(held, `{workspaceRoot}/${folder}`)), ...ruleInputs];
    const hosted = Record.filterMap(
        {
            root: {
                version: interpreter,
                command: (filter: string) => walk([filter, ...scopes.map(({ scope }) => quote(['--globs', `!${scope}/**`]))]),
                inputs: (held: readonly string[]) => [...read(held), ...scopes.map(({ scope }) => `!${folderInput(scope)}`), ...skipped, ...ruleInputs],
            },
            ...Record.fromIterableWith(versionGroups(scopes), ({ version, folders, scoped }) => [version, { version, command: (filter: string) => scan([filter, scoped]), inputs: within(folders) }]),
        },
        ({ version, command, inputs }) => {
            const running = off.filter(runs(version));
            return Array.isArrayNonEmpty(running) ? Result.succeed({ command: command(ids(running)), inputs: [...inputs(Array.dedupe(running.map(Struct.get('language')))), 'astGrep'] }) : Result.failVoid;
        },
    );
    const lock = `flock ${quote([path.join(store.dir, 'rewrite.lock')])}`;
    return {
        projects: {
            [GRAMMARS]: { root: GRAMMARS, name: path.basename(GRAMMARS), targets: { grammar: { dependsOn: ['grammar:*'] }, ...Object.fromEntries(grammars) } },
            '.': {
                root: '.',
                targets: {
                    ...split('lint:ast-grep', {
                        tree: {
                            command: walk(['--error=no-suppress-all', ...offs]),
                            syncGenerators: [`./${GENERATORS}:injection-utils`, `./${GENERATORS}:outline-catalogs`],
                            inputs: [...read(Object.keys(sources)), ...Object.values(languageGlobs).flatMap(Array.map((pattern) => `{workspaceRoot}/**/${pattern}`)), ...skipped, ...ruleInputs, 'astGrep'],
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
                        ...hosted,
                    }),
                    ...split(
                        'rewrite',
                        Record.map({ tree: { command: walk(offs) }, ...hosted }, ({ command }) => ({ command: `${lock} ${command} --update-all` })),
                    ),
                },
            },
        },
    } satisfies CreateNodesResult;
});
const syncInputs = Effect.fnUntraced(function* (workspaceRoot: string) {
    const [path, { generators }] = yield* Effect.all([Path.Path, decodeFile(NxGenerators, workspaceRoot, GENERATORS)]);
    const beside = (file: string) => path.join(path.dirname(GENERATORS), file);
    const rows = Object.values(generators);
    const graph = yield* reach(
        workspaceRoot,
        rows.map(({ factory }) => beside(Array.headNonEmpty(String.split(factory, '#')))),
        {},
    );
    return [`{workspaceRoot}/${GENERATORS}`, ...Array.dedupe(rows.map(({ schema }) => `{workspaceRoot}/${beside(schema)}`)), ...importInputs(graph), { externalDependencies: ['@biomejs/biome'] }];
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

export { GRAMMARS, neutrality, roots, syncInputs };
