import { type CreateDependencies, type CreateDependenciesContext, type CreateNodes, type CreateNodesResultArray, createNodesFromFiles, DependencyType, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { Array, Effect, Equal, Function, Option, Path, Predicate, Record, Schema, String, Struct, Tuple } from 'effect';
import { decodeFile, Package, XcodeProject } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';
import { attachedSuites, infer, logged, markers, moduleRequests, owning, packageName, rootTargets, suitePath } from '#/tools/nx/graph/projects';
import { GRAMMARS, neutrality, roots, syncInputs } from '#/tools/nx/graph/root-project';
import { CARGO_MANIFEST, glob, JsonListRecord, openWorkspace, PYPROJECT, runPlugin, type Workspace } from '#/tools/nx/workspace';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [COMPONENTS]
const dependedProjects = (dependsOn: TargetConfiguration['dependsOn'] = []) => dependsOn.filter((step) => typeof step !== 'string').flatMap((step) => Array.ensure(step.projects ?? []));
const located = (inferred: CreateNodesResultArray) => inferred.flatMap(([file, { projects = {} }]) => Record.collect(projects, (at, project) => ({ file, at, project })));
const components = Effect.fnUntraced(function* (workspace: Workspace, inferred: CreateNodesResultArray) {
    const [path, manifests] = yield* Effect.all([Path.Path, workspace.packages]);
    const projects = located(inferred);
    const declared = (root: string) => Array.dedupe(projects.filter(({ at }) => at === root).flatMap(({ project }) => Object.keys(project.targets ?? {})));
    const held = new Set(projects.map(Struct.get('at')));
    const members = Record.collect(
        Record.filter(manifests, (_, at) => held.has(at)),
        (at, { name }) => ({ at, name, steps: Array.intersection(['build', 'pack', 'install'], declared(at)) }),
    );
    const applications = projects.filter(({ project: { tags = [] } }) => tags.includes('build:xcode')).map(({ file, at, project: { targets } }) => ({ file, root: at, env: targets?.pack?.options?.env, embedded: dependedProjects(targets?.pack?.dependsOn) }));
    const component = Effect.fnUntraced(function* ({ file, root, env, embedded }: (typeof applications)[number]) {
        const { owner } = yield* owning(workspace, root);
        const nested = owner === '.' ? [] : members.filter(Predicate.Struct({ at: Predicate.and(Predicate.not(Equal.equals(root)), Function.flow(path.relative.bind(null, owner), Predicate.not(String.startsWith('..')))) }));
        const after = (step: string, needed: string) =>
            declared(root).includes(step) ? Array.match(Array.difference(nested.filter(Predicate.Struct({ steps: Array.contains(needed) })).map(Struct.get('name')), embedded), { onEmpty: () => ({}), onNonEmpty: (names) => ({ [step]: { dependsOn: [{ projects: names, target: needed }] } }) }) : {};
        const targeted = (steps: readonly string[]) => Record.map(Record.fromIterableBy(steps, Function.identity), Function.constant({ options: { env } }));
        const dependencies = Record.map(Record.fromIterableBy(nested, Struct.get('at')), Function.flow(Struct.pick(['at', 'steps']), Struct.evolve({ steps: targeted }), Struct.renameKeys({ at: 'root', steps: 'targets' })));
        return [file, { projects: { ...dependencies, [root]: { root, implicitDependencies: nested.map(Struct.get('name')), targets: { ...after('install', 'install'), ...after('publish', 'pack') } } } }] as const;
    });
    return yield* Effect.forEach(applications, component);
});

// --- [DEPENDENCIES]
const manifestless = ({ projects }: CreateDependenciesContext, manifested: ReadonlySet<string>, project: string) => Option.exists(Record.get(projects, project), ({ root, tags = [] }) => tags.includes('language:typescript') && !manifested.has(root));
const importTarget = ({ projects, externalNodes = {} }: CreateDependenciesContext, specifier: string) => {
    const name = packageName(specifier);
    return Option.firstSomeOf([Option.as(Record.get(externalNodes, `npm:${name}`), `npm:${name}`), Option.as(Record.get(projects, name), name)]);
};
const sourceImports = Effect.fnUntraced(function* (context: CreateDependenciesContext, [source, sourceFile]: readonly [string, string]) {
    const requests = yield* moduleRequests(context.workspaceRoot, sourceFile);
    return Array.dedupe(Array.getSomes(requests.map((request) => importTarget(context, request)))).map((target) => ({ source, target, sourceFile, type: DependencyType.static }));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<{ readonly root: Record<string, TargetConfiguration>; readonly grammars: Record<string, TargetConfiguration> }> = [
    `{${Object.keys(roots).join(',')},*/**/{${Object.keys(markers).join(',')}}}`,
    Effect.fnUntraced(function* (files, options, context) {
        const [path, workspace, { name }, synced] = yield* Effect.all([Path.Path, openWorkspace(context), Package.read(context.workspaceRoot), syncInputs(context.workspaceRoot)]);
        const inference = { ...workspace, configFiles: files, dotnetRoots: files.filter(String.endsWith('.csproj')).map(path.dirname) };
        const inferred = yield* Effect.promise(
            createNodesFromFiles.bind(
                null,
                Effect.fnUntraced(function* (file: string) {
                    return yield* Option.match(Record.get(file)(roots), { onNone: () => infer(file, inference), onSome: (root) => root(inference) });
                }, runPlugin),
                files,
                options,
                context,
            ),
        );
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
            ['nx.json', rootTargets({ 'lint:nx': { inputs: synced }, restore: { options: logged(context.workspaceRoot, 'restore') }, ...(yield* neutrality(workspace)) })] as const,
            ...Array.match(unclaimed, { onEmpty: () => [], onNonEmpty: (suites) => [[PYPROJECT, rootTargets({ test: { command: 'pytest', options: { args: suites }, inputs: ['python', 'pythonFiles'] } })]] satisfies CreateNodesResultArray }),
            ...(yield* components(workspace, inferred)),
            ...configuredCrates.map(({ manifest, nx }) => [manifest, { projects: { [path.dirname(manifest)]: nx } }] as const),
            ...Option.toArray(Option.fromUndefinedOr(options)).map(({ root, grammars }) => ['nx.json', { projects: { '.': { root: '.', name, targets: root }, [GRAMMARS]: { root: GRAMMARS, targets: grammars } } }] as const),
        ];
    }, runPlugin),
];
const createDependencies: CreateDependencies = Effect.fnUntraced(function* (_options, context) {
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
                glob(workspaceRoot, [`*/**/${XcodeProject.file}`]),
                Effect.validate(
                    Effect.fnUntraced(function* (file: string) {
                        const { targets: rows } = yield* decodeFile(XcodeProject.schema, workspaceRoot, file);
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
    const manifested = new Set(
        Object.keys(owners)
            .filter((file) => path.basename(file) === Package.file)
            .map(path.dirname),
    );
    const imported = yield* Effect.forEach(
        Object.entries(filesToProcess.projectFileMap)
            .filter(([project]) => manifestless(context, manifested, project))
            .flatMap(([project, files]) => Array.cartesian([project], files.map(Struct.get('file')).filter(Predicate.or(String.endsWith('.ts'), String.endsWith('.tsx'))))),
        (pair) => sourceImports(context, pair),
        { concurrency: 'unbounded' },
    );
    return [...[...resolved, ...linked].map(([source, target, sourceFile]) => ({ source, target, sourceFile, type: DependencyType.static })), ...imported.flat(), ...swift];
}, runPlugin);

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
