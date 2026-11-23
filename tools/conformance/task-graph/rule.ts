import { isAbsolute, matchesGlob, normalize } from 'node:path';
import { type ConformanceViolation, createConformanceRule } from '@nx/conformance';
import type { ProjectGraph, TargetDependencyConfig } from '@nx/devkit';
import { expandDependencyConfigSyntaxSugar, findMatchingProjects, normalizeTargetDependencyWithStringProjects } from '@nx/devkit/internal';
import { Array, Predicate, Record } from 'effect';
import type { TaskGraph, TaskGraphEdge } from 'nx/src/config/task-graph';
import { createTaskGraph } from 'nx/src/tasks-runner/create-task-graph';
import { assertTaskGraphDoesNotContainInvalidTargets, findCycle } from 'nx/src/tasks-runner/task-graph-utils';
import { isGlobPattern } from 'nx/src/utils/globs';

// --- [CONSTANTS] -----------------------------------------------------------------------

const GLOBBED = /\/[^/]*[*{].*$/u;
const TOKENED = /\{(?:options|workspaceRoot|projectRoot|projectName)/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const holds = ({ nodes }: ProjectGraph, projects: readonly string[], pattern: string) => Array.isReadonlyArrayNonEmpty(projects) && projects.every((project) => Record.keys(nodes[project]?.data.targets ?? {}).some((target) => matchesGlob(target, pattern)));

const resolves = (graph: ProjectGraph, project: string, entry: string | TargetDependencyConfig) => {
    const { target, projects, dependencies = false } = normalizeTargetDependencyWithStringProjects(expandDependencyConfigSyntaxSugar(entry, graph, project));
    return dependencies || isGlobPattern(target) ? Record.keys(graph.nodes).some((owner) => holds(graph, [owner], target)) : holds(graph, projects === undefined ? [project] : findMatchingProjects(projects, graph.nodes), target);
};

const configured = ({ nodes }: ProjectGraph, { tasks }: TaskGraph, id: string) => {
    const task = tasks[id]?.target;
    return task === undefined ? undefined : nodes[task.project]?.data.targets?.[task.target];
};

const unsignalled = (graph: ProjectGraph, tasks: TaskGraph, dependent: string, { id, waitFor }: TaskGraphEdge) => Predicate.isNullish(configured(graph, tasks, id)?.readyWhen) || (waitFor !== 'ready' && configured(graph, tasks, dependent)?.executor !== 'nx:noop');

const reaching = (edges: Record.ReadonlyRecord<string, readonly string[]>, held: ReadonlySet<string>): ReadonlySet<string> => {
    const next = held.union(new Set(Record.keys(Record.filter(edges, (targets) => targets.some((target) => held.has(target))))));
    return next.size === held.size ? held : reaching(edges, next);
};

const scheduled = (graph: ProjectGraph, tasks: TaskGraph): readonly ConformanceViolation[] => [
    ...Array.fromNullishOr(findCycle(tasks)).map((cycle) => ({ workspaceViolation: true, message: `Task cycle ${cycle.join(' -> ')}, drop one dependsOn entry of it` })),
    ...Record.collect(tasks.continuousDependencies, (dependent, edges) =>
        edges.filter((edge) => unsignalled(graph, tasks, dependent, edge)).map(({ id, waitFor = 'started' }) => ({ sourceProject: tasks.tasks[dependent]?.target.project ?? dependent, message: `${dependent} waits for ${id} to be ${waitFor}, give ${id} a readyWhen signal and the edge waitFor ready` })),
    ).flat(),
];

const stored = ({ tasks }: TaskGraph, files: readonly string[], folders: readonly string[]): readonly ConformanceViolation[] => {
    const cached = Object.values(tasks)
        .filter(({ cache }) => cache)
        .flatMap(({ id, outputs, target }) => outputs.map((output) => ({ id, project: target.project, output, base: output.replace(GLOBBED, '') })));
    const held = new Set(files.flatMap((file) => Array.scan(file.split('/'), '', (folder, part) => (folder === '' ? part : `${folder}/${part}`))));
    return cached.flatMap(({ id, project, output, base }) =>
        [
            ...(TOKENED.test(output) || isAbsolute(output) || normalize(output).startsWith('..') ? [`resolves output ${output} outside the workspace or with a token left`] : []),
            ...(folders.some((folder) => matchesGlob(output, folder)) ? [] : [`writes ${output} outside ${folders.join(', ')}`]),
            ...(held.has(output) ? [output] : files.filter((file) => file.startsWith(`${base}/`) && matchesGlob(file, output))).map((file) => `writes ${output} over workspace file ${file}, which a replay overwrites`),
            ...cached.filter((inner) => inner.id !== id && `${inner.base}/`.startsWith(`${base}/`)).map((inner) => `writes ${base}, which holds ${inner.base} of ${inner.id}`),
        ].map((fault) => ({ sourceProject: project, message: `${id} ${fault}` })),
    );
};

const cleaned = ({ tasks, dependencies, continuousDependencies }: TaskGraph, clean: string): readonly ConformanceViolation[] => {
    const edges = Record.map(tasks, (_, id) => [...(dependencies[id] ?? []), ...(continuousDependencies[id] ?? [])].map((edge) => edge.id));
    const cleaning = new Set(
        Object.values(tasks)
            .filter(({ target }) => target.target === clean)
            .flatMap(({ id }) => [id, ...(edges[id] ?? [])]),
    );
    const reached = reaching(edges, cleaning);
    return Object.values(tasks)
        .filter(({ id, cache }) => cache && !cleaning.has(id) && reached.has(id))
        .map(({ id, target }) => ({ sourceProject: target.project, message: `${id} is cached and depends on a ${clean} task, which deletes outputs before every replay, drop that dependsOn entry` }));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createConformanceRule<{ readonly folders: readonly string[]; readonly clean: string }>({
    name: 'task-graph',
    category: 'reliability',
    description: 'Resolves every dependsOn entry, plans every target of every project as Nx runs it, keeps each cached output inside option `folders` apart from workspace files and other cached outputs, and gives every project with outputs a `clean` target no cached task depends on',
    implementation: ({ projectGraph, fileMapCache: { fileMap }, ruleOptions: { folders, clean } }) => {
        const declared = Object.values(projectGraph.nodes).flatMap(({ name, data }) => Record.collect(data.targets ?? {}, (target, { dependsOn = [], outputs = [] }) => ({ project: name, target, dependsOn, outputs })));
        const files = [...fileMap.nonProjectFiles, ...Object.values(fileMap.projectFileMap).flat()].map(({ file }) => file);
        const configuration = declared.flatMap(({ project, target, dependsOn, outputs }) =>
            [
                ...dependsOn.filter((entry) => !resolves(projectGraph, project, entry)).map((entry) => `dependsOn ${JSON.stringify(entry)} names no target, name an existing target or drop the entry`),
                ...(outputs.length > 0 && !(clean in (projectGraph.nodes[project]?.data.targets ?? {})) ? [`declares outputs in a project without a ${clean} target, give the project ${clean}`] : []),
            ].map((fault) => ({ sourceProject: project, message: `${project}:${target} ${fault}` })),
        );
        const tasks = createTaskGraph(projectGraph, {}, Array.dedupe(declared.map(({ project }) => project)), Array.dedupe(declared.map(({ target }) => target)), undefined, {});
        assertTaskGraphDoesNotContainInvalidTargets(tasks);
        return Promise.resolve({ severity: 'high' as const, details: { violations: [...configuration, ...scheduled(projectGraph, tasks), ...stored(tasks, files, folders), ...cleaned(tasks, clean)] } });
    },
});
