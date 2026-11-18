import { execFileSync, execSync, spawnSync } from 'node:child_process';
import { mkdtempSync, readdirSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, relative } from 'node:path';
import { resolveNxTokensInOptions } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-dotnet';
import { TempFs } from '@rasm/testing/temp-fs';
import { Effect, Record, Schema } from 'effect';
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

const runtimes = (inputs: readonly unknown[] = []) => inputs.flatMap((input) => (typeof input === 'object' && input !== null && 'runtime' in input && typeof input.runtime === 'string' ? [input.runtime] : []));

const Build = Schema.Struct({ project: Schema.String, command: Schema.NonEmptyArray(Schema.String), outputs: Schema.Array(Schema.String), solution: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)) });

const Configured = Schema.Record(Schema.String, Build);

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
    .extend('outside', ({}, { onCleanup }) => {
        const folder = mkdtempSync(join(tmpdir(), 'dotnet-host-'));
        onCleanup(() => rmSync(folder, { recursive: true, force: true }));
        return folder;
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

        it('should hash the NuGet config files NuGet reads inside the workspace, spelled as on disk', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'NuGet.Config': '<configuration />\n', 'apps/MyProj/MyProj.csproj': project() });

            expect(projects['apps/MyProj']?.targets?.['build--dotnet']?.inputs).toContain('{workspaceRoot}/NuGet.Config');
        });
    });

    describe('toolchain inputs', () => {
        it.for(['build--dotnet', 'pack--dotnet', 'check--dotnet-build', 'test--dotnet', 'clean--dotnet'])('should hash the SDK global.json resolves and the platform in %s, as their outputs differ by both', async (target, { projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyProj/MyProj.csproj': project('<IsTestProject>true</IsTestProject>\n    <IsPackable>true</IsPackable>') });

            expect(runtimes(projects['apps/MyProj']?.targets?.[target]?.inputs)).toStrictEqual(['dotnet --version', 'node -p process.platform+process.arch']);
        });

        it.for(['check--dotnet-format', 'fix--dotnet-format'])('should hash the SDK alone in %s, as code style findings hold on every platform', async (target, { projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyProj/MyProj.csproj': project() });

            expect(runtimes(projects['apps/MyProj']?.targets?.[target]?.inputs)).toStrictEqual(['dotnet --version']);
        });

        it('should cache check--dotnet-build under the inputs of build--dotnet, as each SARIF report holds an analyzer timing no input decides', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyProj/MyProj.csproj': project() });

            expect(projects['apps/MyProj']?.targets?.['check--dotnet-build']?.inputs).toStrictEqual(projects['apps/MyProj']?.targets?.['build--dotnet']?.inputs);
        });
    });

    describe('output paths', () => {
        it('should declare the output, intermediate, and publish folders of the configuration as build outputs, as a build writes exactly those', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyLib/MyLib.csproj': project() });

            expect(projects['apps/MyLib']?.targets?.['build--dotnet']?.options).toHaveProperty('outputs', ['apps/MyLib/bin/Debug/net10.0', 'apps/MyLib/obj/Debug/net10.0', 'apps/MyLib/bin/Debug/net10.0/publish']);
            expect(projects['apps/MyLib']?.targets?.['build--dotnet']?.configurations).toHaveProperty(['release', 'outputs'], ['apps/MyLib/bin/Release/net10.0', 'apps/MyLib/obj/Release/net10.0', 'apps/MyLib/bin/Release/net10.0/publish']);
        });

        it('should declare the nupkg glob as the pack output, as obj is MSBuild state', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyLib/MyLib.csproj': project() });

            expect(projects['apps/MyLib']?.targets?.['pack--dotnet']?.options).toHaveProperty('outputs', ['apps/MyLib/bin/Debug/*.nupkg']);
        });

        it.for(['build--dotnet', 'pack--dotnet'])('should leave the restore files at the obj root out of the outputs of %s, as a replay of them carries another machine paths', async (target, { projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyLib/MyLib.csproj': project() });

            expect(projects['apps/MyLib']?.targets?.[target]?.options).toHaveProperty('outputs', expect.not.arrayContaining(['apps/MyLib/obj', expect.stringMatching(/^!/u)]));
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

    it('should write every declared output of build--dotnet and nothing outside them once the project is restored', async ({ tempFs, projectsOf }) => {
        const projects = await projectsOf({ 'apps/MyLib/MyLib.csproj': project(), 'apps/MyLib/Lib.cs': 'namespace MyLib;\n\npublic static class Lib;\n' });
        const { project: file, command, outputs } = resolveNxTokensInOptions(await Effect.runPromise(Schema.decodeUnknownEffect(Build)(projects['apps/MyLib']?.targets?.['build--dotnet']?.options)), { root: 'apps/MyLib' }, 'MyLib:build--dotnet');
        execFileSync('dotnet', ['restore', file], { cwd: tempFs.tempDir });
        const before = listed(tempFs.tempDir);

        execFileSync('dotnet', [command[0], file, ...command.slice(1)], { cwd: tempFs.tempDir });
        const written = [...listed(tempFs.tempDir).difference(before)];
        const under = new RegExp(`^(${outputs.map(RegExp.escape).join('|')})/`, 'u');

        expect(written).not.toHaveLength(0);
        written.forEach((path) => expect(path).toMatch(under));
        outputs.forEach((output) => expect(written).toContainEqual(expect.stringMatching(new RegExp(`^${RegExp.escape(output)}/`, 'u'))));
    });

    it.for([
        { shape: 'an extra declared configuration', properties: '<Configurations>Debug;Release;Interface</Configurations>', configuration: 'interface' },
        { shape: 'a default configuration the reference lacks', properties: '<Configurations>Debug;Release;Interface</Configurations>\n    <Configuration>Interface</Configuration>', configuration: undefined },
    ])('should build a reference that leaves the configuration undeclared in its own default under $shape', async ({ properties, configuration }, { tempFs, projectsOf }) => {
        const projects = await projectsOf({
            'libs/Core/Core.csproj': project(),
            'libs/Core/Core.cs': 'namespace Core;\n\npublic static class Shapes;\n',
            'apps/App/App.csproj': `<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n    ${properties}\n  </PropertyGroup>\n  <ItemGroup>\n    <ProjectReference Include="../../libs/Core/Core.csproj" />\n  </ItemGroup>\n</Project>\n`,
            'apps/App/App.cs': 'namespace App;\n\npublic static class Views;\n',
        });
        const target = projects['apps/App']?.targets?.['build--dotnet'];
        const { project: file, command, solution = {} } = resolveNxTokensInOptions(await Effect.runPromise(Schema.decodeUnknownEffect(Build)(configuration === undefined ? target?.options : target?.configurations?.[configuration])), { root: 'apps/App' }, 'App:build--dotnet');
        const assigned = Record.collect(solution, (at, held) => `<ProjectConfiguration Project='${at}' AbsolutePath='${join(tempFs.tempDir, at)}'>${held}</ProjectConfiguration>`);

        execFileSync('dotnet', [command[0], file, ...command.slice(1), `-p:CurrentSolutionConfigurationContents=<SolutionConfiguration>${assigned.join('')}</SolutionConfiguration>`], { cwd: tempFs.tempDir });

        expect(solution).toStrictEqual({ 'apps/App/App.csproj': 'Interface|AnyCPU', 'libs/Core/Core.csproj': 'Debug|AnyCPU' });
        expect(new Set([...listed(join(tempFs.tempDir, 'libs/Core/bin')), ...listed(join(tempFs.tempDir, 'apps/App/bin'))].map((path) => path.split('/')[0]))).toStrictEqual(new Set(['Debug', 'Interface']));
    });

    it('should name no solution mapping where every reference declares the configuration', async ({ projectsOf }) => {
        const projects = await projectsOf({ 'libs/Core/Core.csproj': project(), 'apps/App/App.csproj': project('</PropertyGroup>\n  <ItemGroup>\n    <ProjectReference Include="../../libs/Core/Core.csproj" />\n  </ItemGroup>\n  <PropertyGroup>') });
        const target = projects['apps/App']?.targets?.['build--dotnet'];

        const [options, configurations] = await Effect.runPromise(Effect.all([Schema.decodeUnknownEffect(Build)(target?.options), Schema.decodeUnknownEffect(Configured)(target?.configurations)]));

        expect([options, ...Object.values(configurations)].filter(({ solution }) => solution !== undefined)).toHaveLength(0);
    });

    describe('files outside the workspace', () => {
        it('should hash the content of an up-to-date check input outside the workspace as a runtime input that follows the file', async ({ projectsOf, outside }) => {
            const host = join(outside, 'Info.plist');
            writeFileSync(host, 'first');
            const projects = await projectsOf({ 'apps/MyProj.Tests/MyProj.Tests.csproj': project(`<IsTestProject>true</IsTestProject>\n  </PropertyGroup>\n  <ItemGroup>\n    <UpToDateCheckInput Include="${host}" />\n  </ItemGroup>\n  <PropertyGroup>`) });
            const hashed = runtimes(projects['apps/MyProj.Tests']?.targets?.['test--dotnet']?.inputs).at(-1) ?? '';
            const first = execSync(hashed, { encoding: 'utf-8' });
            writeFileSync(host, 'second');

            expect(runtimes(projects['apps/MyProj.Tests']?.targets?.['build--dotnet']?.inputs)).toContain(hashed);
            expect(hashed).not.toContain(outside);
            expect(execSync(hashed, { encoding: 'utf-8' })).not.toBe(first);
            rmSync(host);
            expect(spawnSync(hashed, { shell: true, encoding: 'utf-8' }).stderr).toMatch(/could not open/u);
        });

        it('should leave inputs inside the workspace to file inputs alone', async ({ projectsOf }) => {
            const projects = await projectsOf({
                'shared/Info.plist': 'held',
                'apps/MyProj.Tests/MyProj.Tests.csproj': project('<IsTestProject>true</IsTestProject>\n  </PropertyGroup>\n  <ItemGroup>\n    <UpToDateCheckInput Include="../../shared/Info.plist" />\n  </ItemGroup>\n  <PropertyGroup>'),
            });

            expect(runtimes(projects['apps/MyProj.Tests']?.targets?.['test--dotnet']?.inputs)).toStrictEqual(['dotnet --version', 'node -p process.platform+process.arch']);
        });
    });

    describe('test restore', () => {
        it('should restore the test project alone in locked mode before dotnet test, so a configuration writes no lock file', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyProj.Tests/MyProj.Tests.csproj': project('<IsTestProject>true</IsTestProject>'), 'apps/MyProj.Tests/packages.lock.json': '{\n  "version": 2,\n  "dependencies": {}\n}\n' });

            expect(projects['apps/MyProj.Tests']?.targets?.['test--dotnet']?.options).toHaveProperty(['commands', 0, 'command'], 'dotnet restore --no-dependencies --locked-mode -p:Configuration=Debug');
        });

        it('should restore without locked mode where the project keeps no lock file', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyProj.Tests/MyProj.Tests.csproj': project('<IsTestProject>true</IsTestProject>') });

            expect(projects['apps/MyProj.Tests']?.targets?.['test--dotnet']?.options).toHaveProperty(['commands', 0, 'command'], 'dotnet restore --no-dependencies -p:Configuration=Debug');
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

    describe('tags', () => {
        it('should tag a project with each NxTag item its evaluations declare', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyProj.Tests/MyProj.Tests.csproj': project('<IsTestProject>true</IsTestProject>\n  </PropertyGroup>\n  <ItemGroup>\n    <NxTag Include="host:rhino" />\n  </ItemGroup>\n  <PropertyGroup>') });

            expect(projects['apps/MyProj.Tests']?.tags).toContain('host:rhino');
        });
    });

    describe('release', () => {
        it('should give a packable project stating its Version the release tag, pack, and attach, and no publish target where NuGet resolves no default push source', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyLib/MyLib.csproj': project('<Version>1.0.0</Version>'), 'nuget.config': '<configuration />\n' });

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

        it.for([
            ['a publishable project stating its Version', '<OutputType>Exe</OutputType>\n    <IsPackable>false</IsPackable>\n    <IsPublishable>true</IsPublishable>\n    <Version>1.0.0</Version>', ['release']],
            ['a project publishing to a folder alone, with no Version of its own', '<OutputType>Exe</OutputType>\n    <IsPackable>false</IsPackable>\n    <IsPublishable>true</IsPublishable>', []],
            ['a packable project with no Version of its own, which nx release cannot version', '', []],
        ] as const)('should tag %s accordingly', async ([, properties, tags], { projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyApp/MyApp.csproj': project(properties) });

            expect(projects['apps/MyApp']?.tags ?? []).toStrictEqual(tags);
        });
    });

    describe('verb targets', () => {
        it('should run each verb through its <verb>--dotnet targets and declare their inputs', async ({ projectsOf }) => {
            const projects = await projectsOf({ 'apps/MyLib/MyLib.csproj': project() });

            expect(projects['apps/MyLib']?.targets?.['build']?.dependsOn).toStrictEqual([{ target: 'build--*', params: 'forward' }]);
            expect(projects['apps/MyLib']?.targets?.['build']?.inputs?.[0]).toBe('...');
            expect(projects['apps/MyLib']?.targets?.['build']?.inputs?.slice(1)).toStrictEqual(projects['apps/MyLib']?.targets?.['build--dotnet']?.inputs);
        });
    });
});
