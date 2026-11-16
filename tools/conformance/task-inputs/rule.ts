import { matchesGlob } from 'node:path';
import { createConformanceRule } from '@nx/conformance';
import { type FileData, type NxJsonConfiguration, type ProjectGraph, readNxJson, type Task } from '@nx/devkit';
import { filterUsingGlobPatterns, findAllProjectNodeDependencies } from '@nx/devkit/internal';
import { Array, Option, Record, Result } from 'effect';
import { getInputs } from 'nx/src/hasher/task-hasher';
import { createTaskGraph } from 'nx/src/tasks-runner/create-task-graph';

// --- [TYPES] ---------------------------------------------------------------------------

interface Guard {
    readonly owners: ReadonlyMap<string, string>;
    readonly reaches: Record.ReadonlyRecord<string, ReadonlySet<string>>;
    readonly files: readonly FileData[];
    readonly env: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKSPACE = /^(?<negation>!?)\{workspaceRoot\}\//u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const faults = (graph: ProjectGraph, nxJson: NxJsonConfiguration, { owners, reaches, files, env }: Guard, task: Task) => {
    const { selfInputs } = getInputs(task, graph, nxJson);
    const { project } = task.target;
    const related = (owner: string) => reaches[project]?.has(owner) === true || reaches[owner]?.has(project) === true;
    const patterns = Array.filterMap(selfInputs, (input) => ('fileset' in input && WORKSPACE.test(input.fileset) ? Result.succeed(input.fileset.replace(WORKSPACE, '$<negation>')) : Result.failVoid));
    const hashed = Array.isReadonlyArrayNonEmpty(patterns) ? filterUsingGlobPatterns('.', [...files], patterns) : [];
    return [
        ...hashed.filter(({ file }) => !Option.exists(Option.fromNullishOr(owners.get(file)), related)).map(({ file }) => `${task.id} hashes ${file}, which its tool never reads, drop it from the inputs`),
        ...Array.filterMap(selfInputs, (input) => ('env' in input && env.includes(input.env) ? Result.succeed(`${task.id} hashes credential variable ${input.env}, drop it from the inputs`) : Result.failVoid)),
    ];
};

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createConformanceRule<{ readonly files: readonly string[]; readonly env: readonly string[] }>({
    name: 'task-inputs',
    category: 'reliability',
    description: 'Hashes into no task a file option `files` matches outside its own project and the projects it depends on or that depend on it, or a variable option `env` names, as task inputs hold only the files and variables its tool reads',
    implementation: ({ tree, projectGraph, fileMapCache: { fileMap }, ruleOptions: { files, env } }) => {
        const owners = new Map(Record.collect(fileMap.projectFileMap, (project, held) => held.map(({ file }) => [file, project] as const)).flat());
        const guard = {
            owners,
            env,
            reaches: Record.map(projectGraph.nodes, (_, name) => new Set([name, ...findAllProjectNodeDependencies(name, projectGraph)])),
            files: [...Object.values(fileMap.projectFileMap).flat(), ...fileMap.nonProjectFiles].filter(({ file }) => files.some((pattern) => matchesGlob(file, pattern))),
        };
        const nxJson = readNxJson(tree) ?? {};
        const declared = Object.values(projectGraph.nodes).flatMap(({ name, data }) => Record.keys(data.targets ?? {}).map((target) => ({ project: name, target })));
        const { tasks } = createTaskGraph(projectGraph, {}, Array.dedupe(declared.map(({ project }) => project)), Array.dedupe(declared.map(({ target }) => target)), undefined, {});
        const violations = Object.values(tasks).flatMap((task) => faults(projectGraph, nxJson, guard, task).map((message) => ({ sourceProject: task.target.project, message })));
        return Promise.resolve({ severity: 'high' as const, details: { violations } });
    },
});
