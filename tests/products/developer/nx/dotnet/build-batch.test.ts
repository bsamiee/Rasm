import { existsSync, mkdtempSync, readFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import { batch } from '@rasm/nx-dotnet/executors/build';
import { it, pathed } from '@rasm/testing/nx';
import { Record } from 'effect';
import { describe, expect, onTestFinished } from 'vitest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const project = (references: readonly string[]) => `<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n  <ItemGroup>\n${references.map((reference) => `    <ProjectReference Include="${reference}" />\n`).join('')}  </ItemGroup>\n</Project>\n`;

const graphed = (dependencies: Record.ReadonlyRecord<string, readonly string[]>): TaskGraph => ({
    tasks: Record.mapEntries(dependencies, (_, name) => [`${name}:build--dotnet`, { id: `${name}:build--dotnet`, target: { project: name, target: 'build--dotnet' }, outputs: [], overrides: {}, projectRoot: name, parallelism: false, cache: false }]),
    dependencies: Record.mapEntries(dependencies, (held, name) => [`${name}:build--dotnet`, held.map((each) => ({ id: `${each}:build--dotnet` }))]),
    continuousDependencies: {},
    roots: [],
});

const context = (root: string): ExecutorContext => ({ root, cwd: root, isVerbose: false, projectsConfigurations: { version: 2, projects: {} }, nxJsonConfiguration: {}, projectGraph: { nodes: {}, dependencies: {} } });

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('one MSBuild run per batch', () => {
    it('reports each project from the binary log of the run, a failing one with its own compiler errors alone', { timeout: 240_000 }, async ({ workspace }) => {
        workspace.write({
            'lib/lib.csproj': project([]),
            'lib/Shapes.cs': 'namespace Lib;\n\npublic static class Shapes;\n',
            'app/app.csproj': project(['../lib/lib.csproj']),
            'app/Views.cs': 'namespace App;\n\npublic static class Views;\n',
            'bad/bad.csproj': project([]),
            'bad/Broken.cs': 'namespace Bad;\n\npublic static class Broken { public static int Value => Missing; }\n',
        });
        const debug = ['build', '--configuration', 'Debug'] as const;

        const ran = await Array.fromAsync(
            batch(graphed({ lib: [], app: ['lib'], bad: [] }), { 'lib:build--dotnet': { project: 'lib/lib.csproj', command: debug }, 'app:build--dotnet': { project: 'app/app.csproj', command: debug }, 'bad:build--dotnet': { project: 'bad/bad.csproj', command: debug } }, {}, context(workspace.root)),
        );

        expect(Record.fromIterableWith(ran, ({ task, result: { success, terminalOutput } }) => [task, { success, errors: terminalOutput.match(/\w+\.cs\(\d+,\d+\): error CS\d+/gu) ?? [] }] as const)).toStrictEqual({
            'lib:build--dotnet': { success: true, errors: [] },
            'app:build--dotnet': { success: true, errors: [] },
            'bad:build--dotnet': { success: false, errors: ['Broken.cs(3,57): error CS0103'] },
        });
    });
});

describe('batch order', () => {
    it('runs a batch only after the batches holding the tasks it depends on, in as many runs as the order needs', async ({ workspace }) => {
        const folder = mkdtempSync(join(tmpdir(), 'dotnet-batches-'));
        onTestFinished(() => rmSync(folder, { recursive: true }));
        pathed(folder, { dotnet: `[ "$1" = --roll-forward ] && exec /usr/bin/printf '{}'\n/usr/bin/grep -o 'ProjectReference Include="[^"]*/[a-z]*\\.csproj"' "$2" | /usr/bin/sed 's#.*/\\([a-z]*\\)\\.csproj"#\\1#' | /usr/bin/paste -sd, - >> ${folder}/runs.log` });

        await Array.fromAsync(
            batch(
                graphed({ a: ['b'], b: ['c'], c: [] }),
                { 'a:build--dotnet': { project: 'a/a.csproj', command: ['build', '--configuration', 'Debug'] }, 'b:build--dotnet': { project: 'b/b.csproj', command: ['build', '--configuration', 'Release'] }, 'c:build--dotnet': { project: 'c/c.csproj', command: ['build', '--configuration', 'Debug'] } },
                {},
                context(workspace.root),
            ),
        );

        expect(readFileSync(join(folder, 'runs.log'), 'utf-8')).toBe('c\nb\na\n');
    });

    it('fails every task of a cycle between batches naming the tasks, and runs nothing', async ({ workspace }) => {
        const folder = mkdtempSync(join(tmpdir(), 'dotnet-batches-'));
        onTestFinished(() => rmSync(folder, { recursive: true }));
        pathed(folder, { dotnet: `printf '%s\\n' "$*" >> ${folder}/runs.log` });

        const ran = await Array.fromAsync(batch(graphed({ a: ['b'], b: ['a'] }), { 'a:build--dotnet': { project: 'a/a.csproj', command: ['build', '--configuration', 'Debug'] }, 'b:build--dotnet': { project: 'b/b.csproj', command: ['build', '--configuration', 'Release'] } }, {}, context(workspace.root)));

        expect([ran.map(({ task, result: { success, terminalOutput } }) => [task, success, terminalOutput.startsWith('Task cycle among a:build--dotnet, b:build--dotnet, ')]), existsSync(join(folder, 'runs.log'))]).toStrictEqual([
            [
                ['a:build--dotnet', false, true],
                ['b:build--dotnet', false, true],
            ],
            false,
        ]);
    });
});
