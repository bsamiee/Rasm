import { type CreateNodes, type CreateNodesResultArray, createNodesFromFiles, type TargetConfiguration } from '@nx/devkit';
import { findProjectForPath } from '@nx/devkit/internal';
import { Array, Effect, Function, Option, Path, Predicate, Record, String, Struct } from 'effect';
import { infer, logged, markers, owning, rootTargets } from '#/graph/projects.ts';
import { neutrality, roots } from '#/graph/root-project.ts';
import { buildSettings } from '#/sync/settings/build-settings.ts';
import { hostingBindings } from '#/sync/settings/hosting-bindings.ts';
import { openWorkspace, readPython, runPlugin, UV_LOCK, type Workspace } from '#/workspace.ts';

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
    `{${Object.keys(roots).join(',')},*/**/{${[...Object.keys(markers), UV_LOCK].join(',')}}}`,
    Effect.fnUntraced(function* (files, options, context) {
        const [path, workspace] = yield* Effect.all([Path.Path, openWorkspace(context)]);
        const dotnetRoots = new Map(files.filter(String.endsWith('.csproj')).map((file) => [path.dirname(file), file]));
        const inference = {
            ...workspace,
            configFiles: new Set(files),
            python: yield* Effect.cached(
                readPython(
                    context.workspaceRoot,
                    files.filter((file) => path.basename(file) === UV_LOCK),
                ),
            ),
        };
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
                    restore: { options: logged(path, '.', 'restore') },
                    'lint:nx': { inputs: ['...', ...[...buildSettings.files, ...hostingBindings.files].map((file) => `{workspaceRoot}/${file}`)] },
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
