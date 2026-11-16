import { execFileSync } from 'node:child_process';
import { readdirSync } from 'node:fs';
import { join, relative } from 'node:path';
import { resolveNxTokensInOptions } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-dotnet';
import { TempFs } from '@rasm/testing/temp-fs';
import { Effect, Schema } from 'effect';
import { minimatch } from 'minimatch';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

const [glob, createNodesFunction] = createNodes;

const matches = (path: string) => minimatch(path, glob);

const project = (properties = '') => `<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n    ${properties}\n  </PropertyGroup>\n</Project>\n`;

const listed = (root: string) =>
    new Set(
        readdirSync(root, { recursive: true, withFileTypes: true })
            .filter((entry) => entry.isFile())
            .map((entry) => relative(root, join(entry.parentPath, entry.name))),
    );

const Build = Schema.Struct({ project: Schema.String, command: Schema.NonEmptyArray(Schema.String), outputs: Schema.Array(Schema.String) });

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const [tempFs, previous] = [new TempFs('dotnet-plugin'), process.cwd()];
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            tempFs.cleanup();
        });
        return tempFs;
    })
    .extend('projectsOf', ({ tempFs }) => async (files: Readonly<Record<string, string>>) => {
        await tempFs.createFiles(files);
        const results = await createNodesFunction(
            Object.keys(files).filter((file) => minimatch(file, glob, { dot: true })),
            undefined,
            { nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir },
        );
        return Object.fromEntries(results.flatMap(([, { projects = {} }]) => Object.entries(projects)));
    });

describe('@rasm/nx-dotnet - createNodes', { timeout: 30_000 }, () => {
    describe('config file glob', () => {
        it.each([
            'apps/api/Api.csproj',
            'libs/core/Core.fsproj',
            'Directory.Build.props',
            'apps/Directory.Packages.props',
            'Directory.Build.rsp',
            'build/Common.Build.props',
            'eng/Versions.targets',
            'global.json',
            'nuget.config',
            'NuGet.Config',
            '.editorconfig',
            'apps/api/.editorconfig',
            'Workspace.slnx',
            'Legacy.sln',
            'eng/Analyzers.globalconfig',
        ])('should match %s', (path) => {
            expect(matches(path)).toBe(true);
        });

        it.each(['apps/api/Program.cs', 'package.json', 'apps/api/appsettings.json'])('should not match %s', (path) => {
            expect(matches(path)).toBe(false);
        });
    });

    describe('directory build inputs', () => {
        it('should append directory build inputs to build and pack', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'Directory.Build.props': '<Project />\n', 'apps/Directory.Build.targets': '<Project />\n', 'apps/MyProj/MyProj.csproj': project() });

            expect(projects['apps/MyProj']?.targets?.['build--dotnet']?.inputs).toContain('{workspaceRoot}/Directory.Build.props');
            expect(projects['apps/MyProj']?.targets?.['build--dotnet']?.inputs).toContain('{workspaceRoot}/apps/Directory.Build.targets');
            expect(projects['apps/MyProj']?.targets?.['pack--dotnet']?.inputs).toContain('{workspaceRoot}/Directory.Build.props');
            expect(projects['apps/MyProj']?.targets?.['pack--dotnet']?.inputs).toContain('{workspaceRoot}/apps/Directory.Build.targets');
        });

        it('should append directory build inputs to test', async ({ projectsOf }) => {
            const projects = await projectsOf({
                'Directory.Packages.props': '<Project />\n',
                'apps/MyProj.Tests/MyProj.Tests.csproj': project('<OutputType>Exe</OutputType>\n    <IsTestingPlatformApplication>true</IsTestingPlatformApplication>'),
            });

            expect(projects['apps/MyProj.Tests']?.targets?.['test--dotnet']?.inputs).toContain('{workspaceRoot}/Directory.Packages.props');
        });

        it('should give clean the inputs of build, as nx affected -t clean follows build changes', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'Directory.Build.props': '<Project />\n', 'apps/MyProj/MyProj.csproj': project() });

            expect(projects['apps/MyProj']?.targets?.['clean--dotnet']?.inputs).toStrictEqual(projects['apps/MyProj']?.targets?.['build--dotnet']?.inputs);
        });

        it('should leave the base inputs intact without directory build inputs', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyProj/MyProj.csproj': project() });

            const buildInputs = projects['apps/MyProj']?.targets?.['build--dotnet']?.inputs;
            expect(buildInputs).toContain('default');
            expect(buildInputs).toContain('^default');
            expect(buildInputs).not.toContain('{workspaceRoot}/.editorconfig');
            expect(buildInputs).not.toContainEqual(expect.stringContaining('Directory.'));
        });

        it('should append directory build inputs after the base inputs', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'Directory.Build.props': '<Project />\n', 'apps/MyProj/MyProj.csproj': project() });

            expect(projects['apps/MyProj']?.targets?.['build--dotnet']?.inputs?.at(-1)).toBe('{workspaceRoot}/Directory.Build.props');
        });
    });

    describe('output paths', () => {
        it('should declare the output, publish, and obj folders of the configuration as build outputs, as a build writes exactly those', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyLib/MyLib.csproj': project() });

            expect(projects['apps/MyLib']?.targets?.['build--dotnet']?.options).toHaveProperty('outputs', ['apps/MyLib/bin/Debug/net10.0', 'apps/MyLib/bin/Debug/net10.0/publish', 'apps/MyLib/obj']);
        });

        it('should declare the nupkg glob as the pack output, as obj is MSBuild state', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyLib/MyLib.csproj': project() });

            expect(projects['apps/MyLib']?.targets?.['pack--dotnet']?.options).toHaveProperty('outputs', ['apps/MyLib/bin/Debug/*.nupkg']);
        });

        it.for(['build--dotnet', 'pack--dotnet'])('should exclude no restore file from the outputs of %s, as every task reading obj restores first', async (target, { projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyLib/MyLib.csproj': project() });

            expect(projects['apps/MyLib']?.targets?.[target]?.options).toHaveProperty('outputs', expect.not.arrayContaining([expect.stringMatching(/^!/u)]));
        });
    });

    describe('OpenApiDocumentsDirectory', () => {
        it.for([
            ['inside the project', '<OpenApiDocumentsDirectory>openapi</OpenApiDocumentsDirectory>', ['apps/foo/openapi/foo.json', 'apps/foo/openapi/foo_*.json']],
            ['from MSBuildProjectDirectory', '<OpenApiDocumentsDirectory>$(MSBuildProjectDirectory)/openapi</OpenApiDocumentsDirectory>', ['apps/foo/openapi/foo.json', 'apps/foo/openapi/foo_*.json']],
            ['outside the project', '<OpenApiDocumentsDirectory>$(MSBuildProjectDirectory)/../../contracts/foo</OpenApiDocumentsDirectory>', ['contracts/foo/foo.json', 'contracts/foo/foo_*.json']],
            ['at the project root', '<OpenApiDocumentsDirectory>.</OpenApiDocumentsDirectory>', ['apps/foo/foo.json', 'apps/foo/foo_*.json']],
            ['at the absolute project root', '<OpenApiDocumentsDirectory>$(MSBuildProjectDirectory)</OpenApiDocumentsDirectory>', ['apps/foo/foo.json', 'apps/foo/foo_*.json']],
            ['relative above the project', '<OpenApiDocumentsDirectory>../contracts</OpenApiDocumentsDirectory>', ['apps/contracts/foo.json', 'apps/contracts/foo_*.json']],
            ['with options naming no file', '<OpenApiDocumentsDirectory>openapi</OpenApiDocumentsDirectory><OpenApiGenerateDocumentsOptions>--openapi-version v3.1 --document-name internal</OpenApiGenerateDocumentsOptions>', ['apps/foo/openapi/foo.json', 'apps/foo/openapi/foo_*.json']],
            ['with --file-name PublicApi', '<OpenApiDocumentsDirectory>openapi</OpenApiDocumentsDirectory><OpenApiGenerateDocumentsOptions>--file-name PublicApi</OpenApiGenerateDocumentsOptions>', ['apps/foo/openapi/PublicApi.json', 'apps/foo/openapi/PublicApi_*.json']],
            ['with --file-name=PublicApi', '<OpenApiDocumentsDirectory>openapi</OpenApiDocumentsDirectory><OpenApiGenerateDocumentsOptions>--file-name=PublicApi</OpenApiGenerateDocumentsOptions>', ['apps/foo/openapi/PublicApi.json', 'apps/foo/openapi/PublicApi_*.json']],
            ['with --file-name:PublicApi', '<OpenApiDocumentsDirectory>openapi</OpenApiDocumentsDirectory><OpenApiGenerateDocumentsOptions>--file-name:PublicApi</OpenApiGenerateDocumentsOptions>', ['apps/foo/openapi/PublicApi.json', 'apps/foo/openapi/PublicApi_*.json']],
            ['with --file-name after other options', '<OpenApiDocumentsDirectory>openapi</OpenApiDocumentsDirectory><OpenApiGenerateDocumentsOptions>--openapi-version v3.1 --file-name PublicApi</OpenApiGenerateDocumentsOptions>', ['apps/foo/openapi/PublicApi.json', 'apps/foo/openapi/PublicApi_*.json']],
            ['with a quoted --file-name', '<OpenApiDocumentsDirectory>openapi</OpenApiDocumentsDirectory><OpenApiGenerateDocumentsOptions>--file-name &quot;PublicApi&quot;</OpenApiGenerateDocumentsOptions>', ['apps/foo/openapi/PublicApi.json', 'apps/foo/openapi/PublicApi_*.json']],
            ['with --file-name Public-Api_v2', '<OpenApiDocumentsDirectory>openapi</OpenApiDocumentsDirectory><OpenApiGenerateDocumentsOptions>--file-name Public-Api_v2</OpenApiGenerateDocumentsOptions>', ['apps/foo/openapi/Public-Api_v2.json', 'apps/foo/openapi/Public-Api_v2_*.json']],
        ] as const)('should declare the documents of the directory %s as build output in every configuration', async ([, properties, documents], { projectsOf }) => {
            const projects = await projectsOf({ 'apps/foo/foo.csproj': project(properties) });

            expect(projects['apps/foo']?.targets?.['build--dotnet']?.options).toHaveProperty('outputs', expect.arrayContaining([...documents]));
            expect(projects['apps/foo']?.targets?.['build--dotnet']?.configurations).toHaveProperty(['release', 'outputs'], expect.arrayContaining([...documents]));
        });

        it.for([
            ['with document generation off at build', '<OpenApiGenerateDocumentsOnBuild>false</OpenApiGenerateDocumentsOnBuild><OpenApiDocumentsDirectory>openapi</OpenApiDocumentsDirectory>'],
            ['outside the workspace', '<OpenApiDocumentsDirectory>/tmp/outside-ws/openapi</OpenApiDocumentsDirectory>'],
            ['at the package default, as obj is already an output', '<OpenApiDocumentsDirectory>obj/</OpenApiDocumentsDirectory>'],
        ] as const)('should declare no document %s', async ([, properties], { projectsOf }) => {
            const projects = await projectsOf({ 'apps/foo/foo.csproj': project(properties) });

            expect(projects['apps/foo']?.targets?.['build--dotnet']?.options).toHaveProperty('outputs', expect.not.arrayContaining([expect.stringMatching(/\.json$/u)]));
        });

        it('should not add the documents to the outputs of other targets', async ({ projectsOf }) => {
            const projects = await projectsOf({
                'apps/foo/foo.csproj': project('<OpenApiDocumentsDirectory>openapi</OpenApiDocumentsDirectory>'),
                'apps/foo.Tests/foo.Tests.csproj': project('<OpenApiDocumentsDirectory>openapi</OpenApiDocumentsDirectory><IsTestProject>true</IsTestProject>'),
            });

            expect(projects['apps/foo']?.targets?.['pack--dotnet']?.options).toHaveProperty('outputs', expect.not.arrayContaining([expect.stringContaining('openapi')]));
            expect(projects['apps/foo.Tests']?.targets?.['test--dotnet']?.outputs).not.toContainEqual(expect.stringContaining('openapi'));
        });
    });

    it('should write every declared output of build--dotnet and nothing outside them', async ({ tempFs, projectsOf }) => {
        const projects = await projectsOf({ 'apps/MyLib/MyLib.csproj': project(), 'apps/MyLib/Lib.cs': 'namespace MyLib;\n\npublic static class Lib;\n' });
        const before = listed(tempFs.tempDir);
        const { project: file, command, outputs } = resolveNxTokensInOptions(await Effect.runPromise(Schema.decodeUnknownEffect(Build)(projects['apps/MyLib']?.targets?.['build--dotnet']?.options)), { root: 'apps/MyLib' }, 'MyLib:build--dotnet');

        execFileSync('dotnet', [command[0], file, ...command.slice(1)], { cwd: tempFs.tempDir });
        const written = [...listed(tempFs.tempDir).difference(before)];
        const under = new RegExp(`^(${outputs.map(RegExp.escape).join('|')})/`, 'u');

        expect(written).not.toHaveLength(0);
        written.forEach((path) => expect(path).toMatch(under));
        outputs.forEach((output) => expect(written).toContainEqual(expect.stringMatching(new RegExp(`^${RegExp.escape(output)}/`, 'u'))));
    });

    describe('NUGET_PACKAGES env input', () => {
        it.for(['build--dotnet', 'pack--dotnet', 'test--dotnet'])('should hash NUGET_PACKAGES in %s', async (target, { projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyProj/MyProj.csproj': project('<IsTestProject>true</IsTestProject>\n    <IsPackable>true</IsPackable>') });

            expect(projects['apps/MyProj']?.targets?.[target]?.inputs).toContainEqual({ env: 'NUGET_PACKAGES' });
        });
    });

    describe('test results output', () => {
        it('should declare the project relative results-directory option output', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyProj.Tests/MyProj.Tests.csproj': project('<IsTestProject>true</IsTestProject>') });

            expect(projects['apps/MyProj.Tests']?.targets?.['test--dotnet']?.outputs).toContain('{projectRoot}/{options.results-directory}');
        });

        it('should declare the default TestResults output under the project root', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyProj.Tests/MyProj.Tests.csproj': project('<IsTestProject>true</IsTestProject>') });

            expect(projects['apps/MyProj.Tests']?.targets?.['test--dotnet']?.outputs).toContain('{projectRoot}/TestResults');
        });
    });

    describe('release', () => {
        it('should give a packable project the release tag, pack, and attach, and no publish target where NuGet resolves no default push source', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyLib/MyLib.csproj': project(), 'nuget.config': '<configuration />\n' });

            expect(projects['apps/MyLib']?.tags).toStrictEqual(['release']);
            expect(projects['apps/MyLib']?.targets).toHaveProperty('pack--dotnet');
            expect(projects['apps/MyLib']?.targets).toHaveProperty('attach--gh');
            expect(projects['apps/MyLib']?.targets).not.toHaveProperty('nx-release-publish');
        });

        it('should give a packable project a publish target where NuGet resolves a default push source', async ({ projectsOf }) => {
            const projects = await projectsOf({
                'apps/MyLib/MyLib.csproj': project(),
                'nuget.config': '<configuration>\n  <config>\n    <add key="defaultPushSource" value="https://api.nuget.org/v3/index.json" />\n  </config>\n</configuration>\n',
            });

            expect(projects['apps/MyLib']?.targets?.['nx-release-publish']).toMatchObject({ dependsOn: ['pack--dotnet', '^nx-release-publish'] });
        });
    });

    describe('verb targets', () => {
        it('should run each verb through its <verb>--dotnet targets and declare their inputs', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyLib/MyLib.csproj': project() });

            expect(projects['apps/MyLib']?.targets?.['build']?.dependsOn).toStrictEqual(['build--*']);
            expect(projects['apps/MyLib']?.targets?.['build']?.inputs?.[0]).toBe('...');
            expect(projects['apps/MyLib']?.targets?.['build']?.inputs?.slice(1)).toStrictEqual(projects['apps/MyLib']?.targets?.['build--dotnet']?.inputs);
        });
    });
});
