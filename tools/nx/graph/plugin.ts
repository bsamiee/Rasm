import { type CreateNodes, type CreateNodesResultArray, createNodesFromFiles, type TargetConfiguration } from '@nx/devkit';
import { findProjectForPath } from '@nx/devkit/internal';
import { Effect, Function, Option, Path, Predicate, Record, String, Struct } from 'effect';
import { infer, logged, markers, rootTargets } from '#/graph/projects.ts';
import { neutrality, roots } from '#/graph/root-project.ts';
import { buildSettings } from '#/sync/settings/build-settings.ts';
import { hostingBindings } from '#/sync/settings/hosting-bindings.ts';
import { openWorkspace, readPython, runPlugin, UV_LOCK } from '#/workspace.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const located = (inferred: CreateNodesResultArray) => inferred.flatMap(([file, { projects = {} }]) => Record.collect(projects, (at, project) => ({ file, at, project })));

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
            ...Option.toArray(Option.fromUndefinedOr(options)).map(({ root }) => ['nx.json', { projects: { '.': { root: '.', name, targets: root } } }] as const),
        ];
    }, runPlugin),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
