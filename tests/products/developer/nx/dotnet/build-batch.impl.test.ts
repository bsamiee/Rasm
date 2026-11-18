import { chmodSync } from 'node:fs';
import { join } from 'node:path';
import { env } from 'node:process';
import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import { batch, type Build } from '@rasm/nx-dotnet/executors/build';
import { TempFs } from '@rasm/testing/temp-fs';
import { Record } from 'effect';
import { describe, expect, test, vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DEBUG = ['build', '--configuration', 'Debug'] as const;
const RELEASE = ['build', '--configuration', 'Release'] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const csproj = (references: readonly string[] = []) => `<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n  <ItemGroup>\n${references.map((reference) => `    <ProjectReference Include="${reference}" />\n`).join('')}  </ItemGroup>\n</Project>\n`;

const task = (name: string) => ({ id: `${name}:build--dotnet`, target: { project: name, target: 'build--dotnet' }, outputs: [], overrides: {}, projectRoot: name, parallelism: false, cache: false });

const graphOf = (dependencies: Record.ReadonlyRecord<string, readonly string[]>): TaskGraph => ({
    tasks: Record.mapEntries(dependencies, (_, name) => [`${name}:build--dotnet`, task(name)]),
    dependencies: Record.mapEntries(dependencies, (held, name) => [`${name}:build--dotnet`, held.map((each) => ({ id: `${each}:build--dotnet` }))]),
    continuousDependencies: {},
    roots: [],
});

const inputsOf = (commands: Record.ReadonlyRecord<string, readonly [string, ...string[]]>): Record.ReadonlyRecord<string, typeof Build.Type> => Record.mapEntries(commands, (command, name) => [`${name}:build--dotnet`, { project: `${name}/${name}.csproj`, command }]);

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('dotnet-batch');
        onCleanup(() => {
            vi.unstubAllEnvs();
            tempFs.cleanup();
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): ExecutorContext => ({ root: tempFs.tempDir, cwd: tempFs.tempDir, isVerbose: false, projectsConfigurations: { version: 2, projects: {} }, nxJsonConfiguration: {}, projectGraph: { nodes: {}, dependencies: {}, externalNodes: {} } }))
    .extend('batches', ({ tempFs }) => async () => {
        await tempFs.createFile('bin/dotnet', `#!/bin/sh\ncase "$*" in\n  *" results "*) printf '{}' ;;\n  *) grep -o 'ProjectReference Include="[^"]*"' "$2" | sed 's#.*/\\([^/]*\\)\\.csproj"#\\1#' | paste -sd, - >> "$(dirname "$0")/batches.log" ;;\nesac\n`);
        chmodSync(join(tempFs.tempDir, 'bin/dotnet'), 0o755);
        vi.stubEnv('PATH', [join(tempFs.tempDir, 'bin'), env['PATH']].join(':'));
        return async () => {
            const log = await tempFs.readFile('bin/batches.log');
            return log.trim().split('\n');
        };
    });

describe('@rasm/nx-dotnet build batch', { timeout: 120_000 }, () => {
    it('should report each project from the run binary log, the failing one with its own compiler errors alone', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'lib/lib.csproj': csproj(),
            'lib/Lib.cs': 'namespace Lib;\n\npublic static class Shapes;\n',
            'app/app.csproj': csproj(['../lib/lib.csproj']),
            'app/App.cs': 'namespace App;\n\npublic static class Views;\n',
            'bad/bad.csproj': csproj(),
            'bad/Bad.cs': 'namespace Bad;\n\npublic static class Broken { public static int Value => Missing; }\n',
        });

        const ran = await Array.fromAsync(batch(graphOf({ lib: [], app: ['lib'], bad: [] }), inputsOf({ lib: DEBUG, app: DEBUG, bad: DEBUG }), {}, context));
        const results = Record.fromIterableWith(ran, ({ task: id, result }) => [id, result] as const);

        expect(ran.map(({ task: id, result }) => [id, result.success])).toStrictEqual([
            ['lib:build--dotnet', true],
            ['app:build--dotnet', true],
            ['bad:build--dotnet', false],
        ]);
        expect(results['bad:build--dotnet']?.terminalOutput).toMatch(/Bad\.cs\(3,\d+\): error CS0103: /u);
        expect(results['lib:build--dotnet']?.terminalOutput).not.toContain('CS0103');
    });

    it('should run the tasks of one batch whose dependencies another batch holds after that batch, in as many runs as the order needs', async ({ context, batches }) => {
        const read = await batches();

        await Array.fromAsync(batch(graphOf({ a: ['b'], b: ['c'], c: [] }), inputsOf({ a: DEBUG, b: RELEASE, c: DEBUG }), {}, context));

        await expect(read()).resolves.toStrictEqual(['c', 'b', 'a']);
    });

    it('should fail every task of a cycle between batches with a message naming the tasks, and run nothing', async ({ tempFs, context, batches }) => {
        await batches();

        const results = await Array.fromAsync(batch(graphOf({ a: ['b'], b: ['a'] }), inputsOf({ a: DEBUG, b: RELEASE }), {}, context));

        expect(results.map(({ task: id, result }) => [id, result.success])).toStrictEqual([
            ['a:build--dotnet', false],
            ['b:build--dotnet', false],
        ]);
        expect(results[0]?.result.terminalOutput).toMatch(/^Task cycle among a:build--dotnet, b:build--dotnet, /u);
        await expect(tempFs.readFile('bin/batches.log')).rejects.toThrow(/ENOENT/u);
    });
});
