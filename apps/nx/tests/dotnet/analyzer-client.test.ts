import { chmodSync } from 'node:fs';
import { join } from 'node:path';
import { env } from 'node:process';
import { createNodes } from '@rasm/nx-dotnet';
import { TempFs } from '@rasm/testing/temp-fs';
import { resetWorkspaceContext } from 'nx/src/utils/workspace-context';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

const project = (properties = '') => `<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n    ${properties}\n  </PropertyGroup>\n</Project>\n`;

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const [tempFs, previous] = [new TempFs('dotnet-analyzer'), process.cwd()];
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            vi.unstubAllEnvs();
            process.chdir(previous);
            resetWorkspaceContext();
            tempFs.cleanup();
        });
        await tempFs.createFiles({ 'a/a.csproj': project(), 'b/b.csproj': project() });
        return tempFs;
    })
    .extend(
        'analyzeProjects',
        ({ tempFs }) =>
            (files: readonly string[]) =>
                createNodes[1](files, undefined, { nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir }),
    )
    .extend('dotnet', ({ tempFs }) => async (script: string) => {
        await tempFs.createFile('bin/dotnet', `#!/bin/sh\nprintf '%s\\n' "$*" >> "$(dirname "$0")/calls.log"\n${script}\n`);
        chmodSync(join(tempFs.tempDir, 'bin/dotnet'), 0o755);
        vi.stubEnv('PATH', [join(tempFs.tempDir, 'bin'), env['PATH']].join(':'));
    })
    .extend('spawns', ({ tempFs }) => async () => {
        const log = await tempFs.readFile('bin/calls.log');
        return log.trim().split('\n');
    });

describe('MsbuildAnalyzer', { timeout: 30_000 }, () => {
    it('should stream the file list over stdin and parse stdout', async ({ tempFs, analyzeProjects, dotnet, spawns }) => {
        await dotnet('tee "$(dirname "$0")/stdin.log" | PATH="$(printf %s "$PATH" | cut -d: -f2-)" dotnet "$@"');

        const results = await analyzeProjects(['a/a.csproj', 'b/b.csproj']);

        expect(results.map(([file, { projects = {} }]) => [file, Object.keys(projects)])).toStrictEqual([
            ['a/a.csproj', ['a']],
            ['b/b.csproj', ['b']],
        ]);
        await expect(tempFs.readFile('bin/stdin.log')).resolves.toBe('a/a.csproj\nb/b.csproj');
        await expect(spawns()).resolves.toStrictEqual([expect.stringMatching(new RegExp(`${RegExp.escape(tempFs.tempDir)}$`, 'u'))]);
        await expect(spawns()).resolves.toStrictEqual([expect.not.stringContaining('"')]);
    });

    it('should return an error result when the analyzer exits non-zero', async ({ analyzeProjects, dotnet }) => {
        await dotnet('echo boom >&2\nexit 1');

        await expect(analyzeProjects(['a/a.csproj'])).rejects.toHaveProperty(['errors', 0, 1, 'message'], expect.stringMatching(/exits 1: boom/u));
    });

    it('should parse malformed analyzer output into an attributable error', async ({ analyzeProjects, dotnet }) => {
        await dotnet("printf 'not json at all'");

        await expect(analyzeProjects(['a/a.csproj'])).rejects.toHaveProperty(['errors', 0, 1, 'message'], expect.stringMatching(/dotnet .* printed no report: not json at all/u));
    });

    it('should not cache a run that reports a failure', async ({ tempFs, analyzeProjects, dotnet, spawns }) => {
        await tempFs.createFile('c/c.csproj', '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n');
        await dotnet('PATH="$(printf %s "$PATH" | cut -d: -f2-)" exec dotnet "$@"');

        await expect(analyzeProjects(['a/a.csproj', 'c/c.csproj'])).rejects.toHaveProperty(['errors', 0, 0], 'c/c.csproj');
        resetWorkspaceContext();
        await expect(analyzeProjects(['a/a.csproj', 'c/c.csproj'])).rejects.toHaveProperty(['errors', 0, 0], 'c/c.csproj');

        await expect(spawns()).resolves.toHaveLength(2);
    });

    describe('evaluation inputs', () => {
        it('should reuse a cached result whose evaluation inputs are unchanged', async ({ tempFs, analyzeProjects, dotnet, spawns }) => {
            await tempFs.createFiles({ 'Directory.Build.props': '<Project>\n  <Import Project="build/Common.msbuild" />\n</Project>\n', 'build/Common.msbuild': '<Project />\n' });
            await dotnet('PATH="$(printf %s "$PATH" | cut -d: -f2-)" exec dotnet "$@"');

            const first = await analyzeProjects(['Directory.Build.props', 'a/a.csproj']);
            resetWorkspaceContext();

            await expect(analyzeProjects(['Directory.Build.props', 'a/a.csproj'])).resolves.toStrictEqual(first);
            await expect(spawns()).resolves.toHaveLength(1);
        });

        it('should rerun the analyzer when an evaluation input changed', async ({ tempFs, analyzeProjects, dotnet, spawns }) => {
            await tempFs.createFiles({ 'Directory.Build.props': '<Project>\n  <Import Project="build/Common.msbuild" />\n</Project>\n', 'build/Common.msbuild': '<Project />\n' });
            await dotnet('PATH="$(printf %s "$PATH" | cut -d: -f2-)" exec dotnet "$@"');

            const first = await analyzeProjects(['Directory.Build.props', 'a/a.csproj']);
            await tempFs.createFile('build/Common.msbuild', '<Project>\n  <PropertyGroup>\n    <IsPackable>false</IsPackable>\n  </PropertyGroup>\n</Project>\n');
            resetWorkspaceContext();
            const second = await analyzeProjects(['Directory.Build.props', 'a/a.csproj']);

            await expect(spawns()).resolves.toHaveLength(2);
            expect(first[0]?.[1].projects?.['a']?.targets).toHaveProperty('pack--dotnet');
            expect(second[0]?.[1].projects?.['a']?.targets).not.toHaveProperty('pack--dotnet');
        });
    });
});
