import { posix } from 'node:path';
import { platform } from 'node:process';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { outputs } from '@rasm/testing/nx';
import { flow, Option, Schema } from 'effect';
import { describe, expect, vi } from 'vitest';
import { it, node, report } from '#/analyzer';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECT = '<Project Sdk="Microsoft.NET.Sdk" />\n';

// --- [MODELS] --------------------------------------------------------------------------

const Tested = Schema.Struct({ commands: Schema.Tuple([Schema.Struct({ command: Schema.String, forwardAllArgs: Schema.Boolean }), Schema.String]) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const tested = flow(Schema.decodeUnknownOption(Tested), Option.getOrThrow);

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('restore', () => {
    it.for([
        { kind: 'keeps a lock file', locks: ['apps/Tests/packages.lock.json'], restore: 'dotnet restore --no-dependencies --locked-mode -p:Configuration=Debug' },
        { kind: 'keeps no lock file', locks: [], restore: 'dotnet restore --no-dependencies -p:Configuration=Debug' },
    ])('restores the project alone before dotnet test, in locked mode only where it $kind', async ({ locks, restore }, { workspace }) => {
        const graph = await workspace.write({ 'apps/Tests/Tests.csproj': PROJECT, ...report([node('apps/Tests/Tests.csproj', { test: true, locks })]) }).graph({ createNodes, createDependencies });

        expect(tested(graph.nodes['Tests']?.data.targets?.['test--dotnet']?.options).commands[0]).toStrictEqual({ command: restore, forwardAllArgs: false });
    });
});

describe('results', () => {
    it('resolves its outputs to the test results folders, and to the results directory a run names', async ({ workspace }) => {
        const graph = await workspace.write({ 'apps/Tests/Tests.csproj': PROJECT, ...report([node('apps/Tests/Tests.csproj', { test: true, results: ['apps/Tests/bin/Debug/net10.0/TestResults', 'artifacts/test/Tests'] })]) }).graph({ createNodes, createDependencies });

        expect([outputs(graph, 'Tests', 'test--dotnet'), outputs(graph, 'Tests', 'test--dotnet', undefined, { 'results-directory': 'reports' })]).toStrictEqual([
            ['apps/Tests/bin/Debug/net10.0/TestResults', 'artifacts/test/Tests'],
            ['apps/Tests/bin/Debug/net10.0/TestResults', 'artifacts/test/Tests', 'apps/Tests/reports'],
        ]);
    });
});

describe('coverage', () => {
    it('writes the coverlet.MTP lcov report into coveragePath under the coverage configuration', async ({ workspace }) => {
        const graph = await workspace.write({ 'apps/Tests/Tests.csproj': PROJECT, ...report([node('apps/Tests/Tests.csproj', { test: true, referenced: ['coverlet.MTP', 'xunit.v3'] })]) }).graph({ createNodes, createDependencies }, { coveragePath: 'coverage/{projectName}' });

        const [, command] = tested(graph.nodes['Tests']?.data.targets?.['test--dotnet']?.configurations?.['coverage']).commands;

        expect([/ --coverlet --coverlet-output-format lcov /u.test(command), posix.join('apps/Tests', /--results-directory '(?<folder>[^']+)'/u.exec(command)?.groups?.['folder'] ?? ''), outputs(graph, 'Tests', 'test--dotnet', 'coverage')]).toStrictEqual([true, 'coverage/Tests', ['coverage/Tests']]);
    });

    it('gives no coverage configuration to a project that references no coverlet.MTP', async ({ workspace }) => {
        const graph = await workspace.write({ 'apps/Tests/Tests.csproj': PROJECT, ...report([node('apps/Tests/Tests.csproj', { test: true, referenced: ['coverlet.collector', 'xunit.v3'] })]) }).graph({ createNodes, createDependencies }, { coveragePath: 'coverage/{projectName}' });

        expect(graph.nodes['Tests']?.data.targets?.['test--dotnet']?.configurations).toStrictEqual({});
    });
});

describe('platforms', () => {
    it.for([
        { kind: 'a test project targeting windows', test: true, platforms: ['windows'], tags: ['os:win32'], hosts: ['win32'] },
        { kind: 'a test project targeting windows and macos', test: true, platforms: ['windows', 'macos'], tags: ['os:win32', 'os:darwin'], hosts: ['win32', 'darwin'] },
        { kind: 'a test project targeting a platform no host runs', test: true, platforms: ['windows', 'browser'], tags: [], hosts: ['win32', 'darwin', 'linux'] },
        { kind: 'a test project targeting no platform', test: true, platforms: [], tags: [], hosts: ['win32', 'darwin', 'linux'] },
        { kind: 'a project running no tests that targets windows', test: false, platforms: ['windows'], tags: [], hosts: [] },
    ])('tags $kind by the hosts that run its tests, and tests it only on those', async ({ test, platforms, tags, hosts }, { workspace }) => {
        const graph = await workspace.write({ 'apps/Tests/Tests.csproj': PROJECT, ...report([node('apps/Tests/Tests.csproj', { test, platforms })]) }).graph({ createNodes, createDependencies });
        const held = graph.nodes['Tests']?.data;

        expect([held?.root, held?.tags ?? [], 'test--dotnet' in (held?.targets ?? {})]).toStrictEqual(['apps/Tests', tags, hosts.includes(platform)]);
    });
});
