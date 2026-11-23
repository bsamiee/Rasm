import { writeFileSync } from 'node:fs';
import { delimiter, join } from 'node:path';
import type { ProjectGraph } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { environment, taskGraph } from '@rasm/testing/nx';
import { Option } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { describe, expect, vi } from 'vitest';
import { it, node, report } from '#/analyzer';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECT = '<Project Sdk="Microsoft.NET.Sdk" />\n';

// --- [OPERATIONS] ----------------------------------------------------------------------

const hasher = (root: string, graph: ProjectGraph, target: string) => {
    const tasks = taskGraph(graph, ['Lib'], [target]);
    const task = Option.getOrThrow(Option.fromUndefinedOr(tasks.tasks[`Lib:${target}`]));
    return async (set: Readonly<Record<string, string>>) => {
        const { value } = await createTaskHasher(graph, readNxJson(root)).hashTask(task, tasks, environment(root, set));
        return value;
    };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('task hashes', () => {
    it.for(['build--dotnet', 'pack--dotnet', 'test--dotnet', 'check--dotnet-format'])('hashes in %s the CI variable MSBuild reads as the property $(CI)', async (target, { workspace }) => {
        const graph = await workspace.write({ 'libs/Lib/Lib.csproj': PROJECT, ...report([node('libs/Lib/Lib.csproj', { packable: true, test: true })]) }).graph({ createNodes, createDependencies });
        const hashed = hasher(workspace.root, graph, target);

        const [local, ci, again] = [await hashed({ CI: 'false' }), await hashed({ CI: 'true' }), await hashed({ CI: 'false' })];

        expect([local === ci, local === again]).toStrictEqual([false, true]);
    });

    it.for(['build--dotnet', 'check--dotnet-format'])('hashes in %s the version of the SDK dotnet resolves', async (target, { workspace, analyzer }) => {
        const graph = await workspace.write({ 'libs/Lib/Lib.csproj': PROJECT, ...report([node('libs/Lib/Lib.csproj')]) }).graph({ createNodes, createDependencies });
        const hashed = hasher(workspace.root, graph, target);
        const resolving = (version: string) => {
            writeFileSync(join(analyzer.folder, 'version'), version);
            return hashed({ PATH: [analyzer.folder, environment(workspace.root).PATH].join(delimiter) });
        };

        const [first, next, again] = [await resolving('10.0.100'), await resolving('11.0.100'), await resolving('10.0.100')];

        expect([first === next, first === again]).toStrictEqual([false, true]);
    });
});
