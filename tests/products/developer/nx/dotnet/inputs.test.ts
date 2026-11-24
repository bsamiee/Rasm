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
    it.for([
        { target: 'build--dotnet', split: true },
        { target: 'pack--dotnet', split: true },
        { target: 'test--dotnet', split: true },
        { target: 'check--dotnet-build', split: true },
        { target: 'check--dotnet-format', split: false },
        { target: 'fix--dotnet-format', split: false },
    ])('hashes in $target the CI variable MSBuild reads as the property $(CI) only where the target reads what the compile it changes wrote', async ({ target, split }, { workspace }) => {
        const graph = await workspace.write({ 'libs/Lib/Lib.csproj': PROJECT, ...report([node('libs/Lib/Lib.csproj', { packable: true, test: true })]) }).graph({ createNodes, createDependencies });
        const hashed = hasher(workspace.root, graph, target);

        const [local, ci, again] = [await hashed({ CI: 'false' }), await hashed({ CI: 'true' }), await hashed({ CI: 'false' })];

        expect([local === ci, local === again]).toStrictEqual([!split, true]);
    });

    it.for(['build--dotnet', 'test--dotnet'])('hashes in %s each variable an NxEnvironmentInput item names, and no other', async (target, { workspace }) => {
        const graph = await workspace.write({ 'libs/Lib/Lib.csproj': PROJECT, ...report([node('libs/Lib/Lib.csproj', { test: true, environment: ['HOST_INSTALLER'] })]) }).graph({ createNodes, createDependencies });
        const hashed = hasher(workspace.root, graph, target);

        const [first, next, unnamed, again] = [await hashed({ HOST_INSTALLER: 'host_1.exe' }), await hashed({ HOST_INSTALLER: 'host_2.exe' }), await hashed({ HOST_INSTALLER: 'host_1.exe', OTHER_INSTALLER: 'other.exe' }), await hashed({ HOST_INSTALLER: 'host_1.exe' })];

        expect([first === next, first === unnamed, first === again]).toStrictEqual([false, true, true]);
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
