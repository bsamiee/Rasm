import { type CreateDependencies, type CreateDependenciesContext, type CreateNodes, type CreateNodesResultArray, createNodesFromFiles, DependencyType, type TargetConfiguration } from '@nx/devkit';
import { findProjectForPath } from '@nx/devkit/internal';
import { Array, Effect, Function, Option, Path, Predicate, Record, Schema, String, Struct } from 'effect';
import { Package } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';
import { infer, logged, markers, moduleRequests, owning, packageName, rootTargets } from '#/tools/nx/graph/projects';
import { neutrality, roots } from '#/tools/nx/graph/root-project';
import { JsonListRecord, openWorkspace, runPlugin, type Workspace } from '#/tools/nx/workspace';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [COMPONENTS]
const dependedProjects = (dependsOn: TargetConfiguration['dependsOn'] = []) => dependsOn.filter((step) => typeof step !== 'string').flatMap((step) => Array.ensure(step.projects ?? []));
const located = (inferred: CreateNodesResultArray) => inferred.flatMap(([file, { projects = {} }]) => Record.collect(projects, (at, project) => ({ file, at, project })));
const components = Effect.fnUntraced(function* (workspace: Workspace, projects: ReturnType<typeof located>) {
    const [path, manifests] = yield* Effect.all([Path.Path, workspace.packages]);
    const grouped = Map.groupBy(projects, Struct.get('at'));
    const declared = (root: string) => (grouped.get(root) ?? []).flatMap(({ project }) => Object.keys(project.targets ?? {}));
    const applications = projects.filter(({ project: { tags = [] } }) => tags.includes('build:xcode')).map(({ file, at, project: { targets } }) => ({ file, root: at, env: targets?.pack?.options?.env, embedded: dependedProjects(targets?.pack?.dependsOn) }));
    const component = Effect.fnUntraced(function* ({ file, root, env, embedded }: (typeof applications)[number]) {
        const { owner } = yield* owning(workspace, root);
        const nested =
            owner === '.'
                ? []
                : Record.collect(
                      Record.filter(manifests, (_, at) => grouped.has(at) && at !== root && !path.relative(owner, at).startsWith('..')),
                      (at, { name }) => ({ at, name, steps: Array.intersection(['build', 'pack', 'install'], declared(at)) }),
                  );
        const prerequisites = (target: string, excluded: readonly string[]) => {
            const names = nested.filter(({ name, steps }) => steps.includes(target) && !excluded.includes(name)).map(Struct.get('name'));
            return names.length === 0 ? [] : [{ projects: names, target }];
        };
        const after = (step: string, needed: string) => {
            const dependsOn = prerequisites(needed, embedded);
            return dependsOn.length > 0 && declared(root).includes(step) ? { [step]: { dependsOn } } : {};
        };
        const targeted = (steps: readonly string[]) => Record.map(Record.fromIterableBy(steps, Function.identity), Function.constant({ options: { env } }));
        const dependencies = Record.map(Record.fromIterableBy(nested, Struct.get('at')), Function.flow(Struct.pick(['at', 'steps']), Struct.evolve({ steps: targeted }), Struct.renameKeys({ at: 'root', steps: 'targets' })));
        return [file, { projects: { ...dependencies, [root]: { root, implicitDependencies: nested.map(Struct.get('name')), targets: { ...after('check', 'build'), ...after('install', 'install'), ...after('publish', 'pack') } } } }] as const;
    });
    return yield* Effect.forEach(applications, component);
});

// --- [DEPENDENCIES]
const importTarget = ({ projects, externalNodes = {} }: CreateDependenciesContext, specifier: string) => {
    const name = packageName(specifier);
    return Option.firstSomeOf([Option.as(Record.get(externalNodes, `npm:${name}`), `npm:${name}`), Option.as(Record.get(projects, name), name)]);
};
const sourceImports = Effect.fnUntraced(function* (context: CreateDependenciesContext, [source, sourceFile]: readonly [string, string]) {
    const requests = yield* moduleRequests(context.workspaceRoot, sourceFile);
    return Array.dedupe(Array.getSomes(requests.map((request) => importTarget(context, request)))).map((target) => ({ source, target, sourceFile, type: DependencyType.static }));
});
const pythonImports = Effect.fnUntraced(function* ({ fileMap, filesToProcess, workspaceRoot }: CreateDependenciesContext) {
    const changed = Object.values(filesToProcess.projectFileMap).flatMap((files) => files.map(Struct.get('file')).filter(String.endsWith('.py')));
    if (changed.length === 0) {
        return [];
    }
    const sources = yield* runCommand('ruff', ['analyze', 'graph', ...changed], { cwd: workspaceRoot }).pipe(Effect.flatMap(Schema.decodeEffect(JsonListRecord)));
    const entries = Object.entries(sources);
    const referenced = new Set(entries.flatMap(([file, targets]) => [file, ...targets]));
    const owners = new Map(Object.entries(fileMap.projectFileMap).flatMap(([owner, files]) => files.filter(({ file }) => referenced.has(file)).map(({ file }) => [file, owner] as const)));
    return entries.flatMap(([sourceFile, files]) => {
        const source = owners.get(sourceFile);
        return source === undefined
            ? []
            : Array.dedupe(files.map((file) => owners.get(file)))
                  .filter((target): target is string => target !== undefined && target !== source)
                  .map((target) => ({ source, target, sourceFile, type: DependencyType.static }));
    });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<{ readonly root: Record<string, TargetConfiguration> }> = [
    `{${Object.keys(roots).join(',')},*/**/{${Object.keys(markers).join(',')}}}`,
    Effect.fnUntraced(function* (files, options, context) {
        const [path, workspace] = yield* Effect.all([Path.Path, openWorkspace(context)]);
        const dotnetRoots = new Map(files.filter(String.endsWith('.csproj')).map((file) => [path.dirname(file), file]));
        const windowsSources = files.filter((file) => file.endsWith('.cs') && file.includes('/Windows/'));
        const windowsProjects = new Set([...dotnetRoots.keys()].filter((root) => windowsSources.some((file) => file.startsWith(`${root}/`) && file.slice(root.length).includes('/Windows/'))));
        const inference = { ...workspace, configFiles: new Set(files), windowsProjects };
        const candidates = files.filter((file) => !file.endsWith('.cs') || findProjectForPath(file, dotnetRoots) === undefined);
        const configure = Effect.fnUntraced(function* (file: string) {
            return yield* Option.match(Record.get(file)(roots), {
                onSome: (root) => root(inference),
                onNone: () => infer(file, inference).pipe(Effect.catchNoSuchElement, Effect.map(Option.getOrElse(Function.constant({})))),
            });
        }, runPlugin);
        const inferred = yield* Effect.promise(() => createNodesFromFiles(configure, candidates, options, context));
        const projects = located(inferred);
        const dotnetScripts = projects.filter(({ file }) => file.endsWith('.cs'));
        const dotnetUpgrade: TargetConfiguration = {
            dependsOn: ['restore', ...dotnetScripts.map(({ file, at }) => ({ projects: [`directory:${at}`], target: `restore:${path.basename(file)}` }))],
            options: { args: ['Directory.Packages.props', ...dotnetScripts.map(Struct.get('file'))] },
        };
        const { name } = yield* Effect.flatMap(workspace.packages, (manifests) => Effect.fromOption(Record.get(manifests, '.')));
        const { daemons } = yield* workspace.mise;
        return [
            ...inferred,
            ['nx.json', rootTargets({ restore: { options: logged(context.workspaceRoot, 'restore') }, upgrade: { options: { args: Record.keys(Record.filter(daemons, (row) => typeof row !== 'string' && row.boot)) } }, 'upgrade:dotnet': dotnetUpgrade, ...(yield* neutrality(workspace)) })] as const,
            ...(yield* components(workspace, projects)),
            ...Option.toArray(Option.fromUndefinedOr(options)).map(({ root }) => ['nx.json', { projects: { '.': { root: '.', name, targets: root } } }] as const),
        ];
    }, runPlugin),
];
const createDependencies: CreateDependencies = Effect.fnUntraced(function* (_options, context) {
    const { fileMap, filesToProcess, projects } = context;
    const path = yield* Path.Path;
    const requests = Object.entries(filesToProcess.projectFileMap)
        .filter(([name]) => {
            const project = projects[name];
            return project?.tags?.includes('language:typescript') === true && !(fileMap.projectFileMap[name] ?? []).some(({ file }) => file === path.join(project.root, Package.file));
        })
        .flatMap(([name, files]) => Array.cartesian([name], files.map(Struct.get('file')).filter(Predicate.or(String.endsWith('.ts'), String.endsWith('.tsx')))));
    const [resolved, imported] = yield* Effect.all([pythonImports(context), Effect.forEach(requests, (pair) => sourceImports(context, pair), { concurrency: 'unbounded' })], { concurrency: 'unbounded' });
    return [...resolved, ...imported.flat()];
}, runPlugin);

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
