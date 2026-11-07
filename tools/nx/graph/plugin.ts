import { type CreateDependencies, type CreateNodes, type CreateNodesResultArray, createNodesFromFiles, DependencyType, type TargetConfiguration } from '@nx/devkit';
import { findProjectForPath } from '@nx/devkit/internal';
import { runCommand } from '@rasm/eng';
import { Array, Effect, Function, Option, Path, Predicate, Record, Schema, String, Struct } from 'effect';
import { infer, logged, markers, owning, rootTargets } from '#/graph/projects.ts';
import { neutrality, roots } from '#/graph/root-project.ts';
import { JsonListRecord, openWorkspace, runPlugin, type Workspace } from '#/workspace.ts';

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

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<{ readonly root: Record<string, TargetConfiguration> }> = [
    `{${Object.keys(roots).join(',')},*/**/{${Object.keys(markers).join(',')}}}`,
    Effect.fnUntraced(function* (files, options, context) {
        const [path, workspace] = yield* Effect.all([Path.Path, openWorkspace(context)]);
        const dotnetRoots = new Map(files.filter(String.endsWith('.csproj')).map((file) => [path.dirname(file), file]));
        const inference = { ...workspace, configFiles: new Set(files) };
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
            options: { args: dotnetScripts.map(Struct.get('file')) },
        };
        const { name } = yield* Effect.flatMap(workspace.packages, (manifests) => Effect.fromOption(Record.get(manifests, '.')));
        const { daemons } = yield* workspace.mise;
        const booted = (row: (typeof daemons)[string]) => typeof row !== 'string' && row.boot;
        return [
            ...inferred,
            [
                'nx.json',
                rootTargets({
                    restore: { options: logged(context.workspaceRoot, 'restore') },
                    upgrade: { options: { args: Record.keys(Record.filter(daemons, booted)) } },
                    'upgrade:dotnet': dotnetUpgrade,
                    ...Record.map(Record.filter(daemons, Predicate.not(booted)), (_, daemon) => ({ command: `mise daemons start ${daemon}` })),
                    ...(yield* neutrality(workspace)),
                }),
            ] as const,
            ...(yield* components(workspace, projects)),
            ...Option.toArray(Option.fromUndefinedOr(options)).map(({ root }) => ['nx.json', { projects: { '.': { root: '.', name, targets: root } } }] as const),
        ];
    }, runPlugin),
];
const createDependencies: CreateDependencies = Effect.fnUntraced(function* (_options, { fileMap, filesToProcess, workspaceRoot }) {
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
}, runPlugin);

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
