import { execSync } from 'node:child_process';
import { mkdtempSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { environment, taskGraph } from '@rasm/testing/nx';
import { Array, Effect, Option, Predicate, Schema } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { describe, expect, onTestFinished, vi } from 'vitest';
import { it, node, report } from '#/analyzer.ts';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECT = '<Project Sdk="Microsoft.NET.Sdk" />\n';

// --- [MODELS] --------------------------------------------------------------------------

const Runtime = Schema.Struct({ runtime: Schema.String });

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('file inputs', () => {
    it('hashes the evaluation inputs of the project and of each project it references, leaving out those absent from the workspace', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/App/App.csproj': PROJECT,
                'libs/Core/Core.csproj': PROJECT,
                'Directory.Build.props': '<Project />\n',
                'build/Core.props': '<Project />\n',
                ...report([node('apps/App/App.csproj', { inputs: ['Directory.Build.props'], references: [{ root: 'libs/Core', declared: 'apps/App/App.csproj' }] }), node('libs/Core/Core.csproj', { inputs: ['build/Core.props', 'build/Deleted.props'] })]),
            })
            .graph({ createNodes, createDependencies });

        expect(graph.nodes['App']?.data.targets?.['build--dotnet']?.inputs?.filter(Predicate.isString)).toStrictEqual(['default', '^default', '{workspaceRoot}/Directory.Build.props', '{workspaceRoot}/build/Core.props']);
    });

    it('leaves the build folders inside the project out of its default inputs, and those outside it to their own projects', async ({ workspace }) => {
        const graph = await workspace.write({ 'apps/App/App.csproj': PROJECT, ...report([node('apps/App/App.csproj', { bases: ['apps/App/bin', 'apps/App/obj', 'artifacts/obj/App'] })]) }).graph({ createNodes, createDependencies });

        expect(graph.nodes['App']?.data.targets?.['build--dotnet']?.inputs?.filter(Predicate.isString)).toStrictEqual(['default', '^default', '!{projectRoot}/bin/**', '!{projectRoot}/obj/**']);
    });
});

describe('runtime inputs', () => {
    it.for([
        { target: 'build--dotnet', runtimes: ['dotnet --version', 'node -p process.platform+process.arch'] },
        { target: 'pack--dotnet', runtimes: ['dotnet --version', 'node -p process.platform+process.arch'] },
        { target: 'check--dotnet-build', runtimes: ['dotnet --version', 'node -p process.platform+process.arch'] },
        { target: 'clean--dotnet', runtimes: ['dotnet --version', 'node -p process.platform+process.arch'] },
        { target: 'test--dotnet', runtimes: ['dotnet --version', 'node -p process.platform+process.arch'] },
        { target: 'check--dotnet-format', runtimes: ['dotnet --version'] },
        { target: 'fix--dotnet-format', runtimes: ['dotnet --version'] },
    ])('hashes in $target the SDK global.json selects, and the host platform where the output is compiled', async ({ target, runtimes }, { workspace }) => {
        const graph = await workspace.write({ 'libs/Lib/Lib.csproj': PROJECT, ...report([node('libs/Lib/Lib.csproj', { packable: true, test: true })]) }).graph({ createNodes, createDependencies });

        expect(graph.nodes['Lib']?.data.targets?.[target]?.inputs?.flatMap((input) => (Predicate.hasProperty(input, 'runtime') ? [input.runtime] : []))).toStrictEqual(runtimes);
    });

    it.for(['build--dotnet', 'pack--dotnet', 'test--dotnet', 'check--dotnet-format'])('hashes in %s the CI variable MSBuild reads as the property $(CI)', async (target, { workspace }) => {
        const graph = await workspace.write({ 'libs/Lib/Lib.csproj': PROJECT, ...report([node('libs/Lib/Lib.csproj', { packable: true, test: true })]) }).graph({ createNodes, createDependencies });
        const tasks = taskGraph(graph, ['Lib'], [target]);
        const task = Option.getOrThrow(Option.fromUndefinedOr(tasks.tasks[`Lib:${target}`]));
        const hashed = (ci: string) => Effect.promise(() => createTaskHasher(graph, readNxJson(workspace.root)).hashTask(task, tasks, { ...environment(workspace.root), CI: ci }));

        const [local, ci, again] = await Effect.runPromise(Effect.all([hashed('false'), hashed('true'), hashed('false')]));

        expect([local.value === ci.value, local.value === again.value]).toStrictEqual([false, true]);
    });

    it('hashes a file the build reads outside the workspace by its content', async ({ workspace }) => {
        const outside = mkdtempSync(join(tmpdir(), 'dotnet-outside-'));
        onTestFinished(() => rmSync(outside, { recursive: true }));
        const file = join(outside, 'Host.dll');
        writeFileSync(file, 'first');
        const graph = await workspace.write({ 'apps/App/App.csproj': PROJECT, ...report([node('apps/App/App.csproj', { external: [file] })]) }).graph({ createNodes, createDependencies });
        const { runtime: hashed } = Option.getOrThrow(Array.findLast(Array.getSomes((graph.nodes['App']?.data.targets?.['build--dotnet']?.inputs ?? []).map((input) => Schema.decodeUnknownOption(Runtime)(input))), ({ runtime }) => runtime.startsWith('git ')));
        const before = execSync(hashed, { cwd: workspace.root, env: environment(workspace.root), encoding: 'utf-8' });

        writeFileSync(file, 'second');

        expect([before === execSync(hashed, { cwd: workspace.root, env: environment(workspace.root), encoding: 'utf-8' }), hashed.includes(outside)]).toStrictEqual([false, false]);
    });
});
