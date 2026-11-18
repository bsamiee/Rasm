import { chmodSync, readdirSync } from 'node:fs';
import { join } from 'node:path';
import { env } from 'node:process';
import { PluginCache } from '@nx/devkit/internal';
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
        await tempFs.createFile('bin/dotnet', `#!/bin/sh\nprintf '%s\\n' "$*" >> "$(dirname "$0")/calls.log"\nprintf '%s|%s|%s\\n' "$1" "$DOTNET_ROLL_FORWARD_TO_PRERELEASE" "$DOTNET_DbgMiniDumpName" >> "$(dirname "$0")/env.log"\n${script}\n`);
        chmodSync(join(tempFs.tempDir, 'bin/dotnet'), 0o755);
        vi.stubEnv('PATH', [join(tempFs.tempDir, 'bin'), env['PATH']].join(':'));
    })
    .extend('analyses', ({ tempFs }) => async () => {
        const log = await tempFs.readFile('bin/calls.log');
        return log
            .trim()
            .split('\n')
            .filter((call) => call.includes('MsbuildAnalyzer.dll'));
    });

describe('MsbuildAnalyzer', { timeout: 30_000 }, () => {
    const forwarded = 'PATH="$(printf %s "$PATH" | cut -d: -f2-)" exec dotnet "$@"';

    it('should stream the file list over stdin and parse stdout', async ({ tempFs, analyzeProjects, dotnet, analyses }) => {
        await dotnet(`[ "$1" = --version ] && ${forwarded}\ntee "$(dirname "$0")/stdin.log" | ${forwarded}`);

        const results = await analyzeProjects(['a/a.csproj', 'b/b.csproj']);

        expect(results.map(([file, { projects = {} }]) => [file, Object.keys(projects)])).toStrictEqual([
            ['a/a.csproj', ['a']],
            ['b/b.csproj', ['b']],
        ]);
        await expect(tempFs.readFile('bin/stdin.log')).resolves.toBe('a/a.csproj\nb/b.csproj');
        await expect(analyses()).resolves.toStrictEqual([expect.stringMatching(new RegExp(`${RegExp.escape(tempFs.tempDir)}$`, 'u'))]);
        await expect(analyses()).resolves.toStrictEqual([expect.not.stringContaining('"')]);
    });

    it('should run the built analyzer assembly under the newest major runtime and compile nothing, as a compile at graph time races the build writing the same files', async ({ tempFs, analyzeProjects, dotnet, analyses }) => {
        await dotnet(forwarded);

        await analyzeProjects(['a/a.csproj']);

        await expect(analyses()).resolves.toStrictEqual([expect.stringMatching(new RegExp(`^--roll-forward LatestMajor \\S+/analyzer/dist/MsbuildAnalyzer\\.dll ${RegExp.escape(tempFs.tempDir)}$`, 'u'))]);
        await expect(tempFs.readFile('bin/calls.log')).resolves.not.toMatch(/^(?:build|publish|run) /mu);
    });

    it('should give the analyzer alone prerelease roll forward and crash dumps under .artifacts/dotnet/dumps', async ({ tempFs, analyzeProjects, dotnet }) => {
        await dotnet(forwarded);

        await analyzeProjects(['a/a.csproj']);

        const log = await tempFs.readFile('bin/env.log');
        const lines = log.trim().split('\n');
        expect(lines).toContain(`--roll-forward|1|${join(tempFs.tempDir, '.artifacts/dotnet/dumps/%e-%p.dmp')}`);
        expect(lines).toContain('--version||');
        expect(env['DOTNET_ROLL_FORWARD_TO_PRERELEASE']).toBeUndefined();
    });

    it('should return an error result when the analyzer exits non-zero', async ({ analyzeProjects, dotnet }) => {
        await dotnet('echo boom >&2\nexit 1');

        await expect(analyzeProjects(['a/a.csproj'])).rejects.toHaveProperty(['errors', 0, 1, 'message'], expect.stringMatching(/exits 1: boom/u));
    });

    it('should parse malformed analyzer output into an attributable error', async ({ analyzeProjects, dotnet }) => {
        await dotnet("printf 'not json at all'");

        await expect(analyzeProjects(['a/a.csproj'])).rejects.toHaveProperty(['errors', 0, 1, 'message'], expect.stringMatching(/dotnet .* printed a report this plugin cannot read, as \S+MsbuildAnalyzer\.dll predates the plugin, run mise deps msbuild-analyzer to rebuild it: /u));
    });

    it('should name the field a report from an analyzer older than the plugin lacks, and leave the report out of the message', async ({ analyzeProjects, dotnet }) => {
        await dotnet(`[ "$1" = --version ] && ${forwarded}\nprintf '{"nodes":[],"failures":[]}'`);

        const failure = analyzeProjects(['a/a.csproj']);

        await expect(failure).rejects.toHaveProperty(['errors', 0, 1, 'message'], expect.stringMatching(/run mise deps msbuild-analyzer .*evaluated/su));
        await expect(failure).rejects.toHaveProperty(['errors', 0, 1, 'message'], expect.not.stringContaining('"failures"'));
    });

    it('should not cache a run that reports a failure', async ({ tempFs, analyzeProjects, dotnet, analyses }) => {
        await tempFs.createFile('c/c.csproj', '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n');
        await dotnet(forwarded);

        await expect(analyzeProjects(['a/a.csproj', 'c/c.csproj'])).rejects.toHaveProperty(['errors', 0, 0], 'c/c.csproj');
        resetWorkspaceContext();
        await expect(analyzeProjects(['a/a.csproj', 'c/c.csproj'])).rejects.toHaveProperty(['errors', 0, 0], 'c/c.csproj');

        await expect(analyses()).resolves.toHaveLength(2);
    });

    describe('plugin cache', () => {
        it('should keep one cache file holding the latest report alone, whatever the file sets read before', async ({ tempFs, analyzeProjects, dotnet }) => {
            await dotnet(forwarded);

            await analyzeProjects(['a/a.csproj']);
            resetWorkspaceContext();
            await analyzeProjects(['a/a.csproj', 'b/b.csproj']);

            expect(readdirSync(join(tempFs.tempDir, 'tmp/project-graph-cache'))).toStrictEqual(['dotnet.hash']);
            expect(Object.keys(new PluginCache(join(tempFs.tempDir, 'tmp/project-graph-cache/dotnet.hash')).toSerializable().entries)).toHaveLength(1);
        });

        it('should rerun the analyzer when the SDK global.json resolves changes', async ({ tempFs, analyzeProjects, dotnet, analyses }) => {
            await tempFs.createFile('bin/sdk', '11.0.100');
            await dotnet(`[ "$1" = --version ] && exec cat "$(dirname "$0")/sdk"\n${forwarded}`);

            await analyzeProjects(['a/a.csproj']);
            resetWorkspaceContext();
            await analyzeProjects(['a/a.csproj']);
            await tempFs.createFile('bin/sdk', '11.0.200');
            resetWorkspaceContext();
            await analyzeProjects(['a/a.csproj']);

            await expect(analyses()).resolves.toHaveLength(2);
        });
    });

    describe('evaluation inputs', () => {
        it('should reuse a cached result whose evaluation inputs are unchanged', async ({ tempFs, analyzeProjects, dotnet, analyses }) => {
            await tempFs.createFiles({ 'Directory.Build.props': '<Project>\n  <Import Project="build/Common.msbuild" />\n</Project>\n', 'build/Common.msbuild': '<Project />\n' });
            await dotnet(forwarded);

            const first = await analyzeProjects(['Directory.Build.props', 'a/a.csproj']);
            resetWorkspaceContext();

            await expect(analyzeProjects(['Directory.Build.props', 'a/a.csproj'])).resolves.toStrictEqual(first);
            await expect(analyses()).resolves.toHaveLength(1);
        });

        it('should rerun the analyzer when an evaluation input changed', async ({ tempFs, analyzeProjects, dotnet, analyses }) => {
            await tempFs.createFiles({ 'Directory.Build.props': '<Project>\n  <Import Project="build/Common.msbuild" />\n</Project>\n', 'build/Common.msbuild': '<Project />\n' });
            await dotnet(forwarded);

            const first = await analyzeProjects(['Directory.Build.props', 'a/a.csproj']);
            await tempFs.createFile('build/Common.msbuild', '<Project>\n  <PropertyGroup>\n    <IsPackable>false</IsPackable>\n  </PropertyGroup>\n</Project>\n');
            resetWorkspaceContext();
            const second = await analyzeProjects(['Directory.Build.props', 'a/a.csproj']);

            await expect(analyses()).resolves.toHaveLength(2);
            expect(first[0]?.[1].projects?.['a']?.targets).toHaveProperty('pack--dotnet');
            expect(second[0]?.[1].projects?.['a']?.targets).not.toHaveProperty('pack--dotnet');
        });

        it('should rerun the analyzer when a restore rewrote the assets file evaluation read', async ({ tempFs, analyzeProjects, dotnet, analyses }) => {
            await dotnet(forwarded);

            await analyzeProjects(['a/a.csproj']);
            await tempFs.createFile('a/obj/project.assets.json', '{}');
            resetWorkspaceContext();
            await analyzeProjects(['a/a.csproj']);

            await expect(analyses()).resolves.toHaveLength(2);
        });
    });
});
