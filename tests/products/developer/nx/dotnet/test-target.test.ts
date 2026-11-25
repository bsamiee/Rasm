import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath } from 'node:url';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { it, outputs } from '@rasm/testing/nx';
import { expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PACKAGES = join(dirname(fileURLToPath(import.meta.resolve('@rasm/nx-dotnet/package.json'))), '../../../../.cache/nuget/packages');
const TESTED = '<ItemGroup>\n    <PackageReference Include="xunit.v3.mtp-v2" Version="4.0.3" />\n    <PackageReference Include="coverlet.MTP" Version="10.1.1" />\n  </ItemGroup>';
const FILES = {
    'global.json': JSON.stringify({ test: { runner: 'Microsoft.Testing.Platform' } }),
    'NuGet.config': `<configuration>\n  <config>\n    <add key="globalPackagesFolder" value="${PACKAGES}" />\n  </config>\n</configuration>\n`,
    'apps/Tests/Tests.csproj': `<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n    <OutputType>Exe</OutputType>\n    <IsTestProject>true</IsTestProject>\n    <RollForward>Major</RollForward>\n  </PropertyGroup>\n  ${TESTED}\n</Project>\n`,
    'apps/Tests/Sums.cs': 'namespace Tests;\n\npublic sealed class Sums {\n    [Xunit.Fact]\n    public void Adds() => Xunit.Assert.Equal(3, 1 + 2);\n}\n',
};
const COVERED = {
    'global.json': FILES['global.json'],
    'NuGet.config': FILES['NuGet.config'],
    'Directory.Build.props': `<Project>\n  <PropertyGroup>\n    <ArtifactsPath>$([MSBuild]::NormalizePath('$(MSBuildThisFileDirectory)', '.artifacts', 'dotnet'))</ArtifactsPath>\n    <TargetFramework>net10.0</TargetFramework>\n    <RollForward>Major</RollForward>\n    <Configurations>Release;Debug;Coverage</Configurations>\n    <Configuration>Release</Configuration>\n  </PropertyGroup>\n  <PropertyGroup Condition="'$(Configuration)' == 'Coverage'">\n    <Optimize>true</Optimize>\n    <DeterministicSourcePaths>false</DeterministicSourcePaths>\n  </PropertyGroup>\n  <PropertyGroup Condition="'$(CI)' == 'true'">\n    <ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>\n  </PropertyGroup>\n</Project>\n`,
    'Directory.Build.targets': `<Project>\n  <PropertyGroup Condition="'$(Configuration)' == 'Coverage'">\n    <PathMap>$([MSBuild]::NormalizeDirectory('$(MSBuildProjectDirectory)'))=$([MSBuild]::MakeRelative('$(MSBuildThisFileDirectory)', '$(MSBuildProjectDirectory)').Replace('\\', '/'))/,$([MSBuild]::NormalizeDirectory('$(ArtifactsPath)'))=$([MSBuild]::MakeRelative('$(MSBuildThisFileDirectory)', '$(ArtifactsPath)').Replace('\\', '/'))/</PathMap>\n  </PropertyGroup>\n</Project>\n`,
    'libs/Kit/Kit.csproj': '<Project Sdk="Microsoft.NET.Sdk" />\n',
    'libs/Kit/Halves.cs': 'namespace Kit;\n\npublic static class Halves {\n    public static int Of(int value) => value % 2 == 0 ? value / 2 : -1;\n}\n',
    'products/Tool/Tool.csproj': '<Project Sdk="Microsoft.NET.Sdk" />\n',
    'products/Tool/Scales.cs': 'namespace Tool;\n\npublic static class Scales {\n    public static int Twice(int value) => value * 2;\n}\n',
    'tests/Kit.Tests/Kit.Tests.csproj': `<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <OutputType>Exe</OutputType>\n    <IsTestProject>true</IsTestProject>\n  </PropertyGroup>\n  ${TESTED}\n  <ItemGroup>\n    <ProjectReference Include="../../libs/Kit/Kit.csproj" />\n    <ProjectReference Include="../../products/Tool/Tool.csproj" />\n  </ItemGroup>\n</Project>\n`,
    'tests/Kit.Tests/Checks.cs': 'namespace Kit.Tests;\n\npublic sealed class Checks {\n    [Xunit.Fact]\n    public void HalvesTwice() => Xunit.Assert.Equal(3, Halves.Of(Tool.Scales.Twice(3)));\n}\n',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

it('runs the tests of a Microsoft Testing Platform project under configuration coverage, writing the coverlet lcov report into coveragePath within its declared outputs', async ({ workspace }) => {
    const graph = await workspace.write(FILES).graph({ createNodes, createDependencies }, { coveragePath: 'coverage/{projectName}' });
    execFileSync('dotnet', ['build', 'apps/Tests/Tests.csproj', '--configuration', 'Debug'], { cwd: workspace.root, stdio: 'ignore' });
    const before = workspace.files();

    const { success } = await workspace.run(graph, 'Tests', 'test--dotnet', 'coverage');

    const declared = outputs(graph, 'Tests', 'test--dotnet', 'coverage');
    const written = [...workspace.files().difference(before)];
    expect({ success, reports: written.filter((file) => /^coverage\/Tests\/coverage\.\d+\.info$/u.test(file)).length, outside: written.filter((file) => !declared.some((output) => file.startsWith(`${output}/`))) }).toStrictEqual({ success: true, reports: 1, outside: [] });
});

it.for(['false', 'true'])('runs configuration coverage under the Coverage configuration a workspace declares, its lcov report naming each source by its path from the workspace root with CI=%s', async (ci, { workspace }) => {
    const graph = await workspace.write(COVERED).graph({ createNodes, createDependencies }, { coveragePath: 'coverage/{projectName}' });
    execFileSync('dotnet', ['build', 'tests/Kit.Tests/Kit.Tests.csproj', '--configuration', 'Coverage'], { cwd: workspace.root, stdio: 'ignore', env: { ...env, CI: ci } });
    const before = workspace.files();

    const { success } = await workspace.run(graph, 'Kit.Tests', 'test--dotnet', 'coverage');

    const reports = [...workspace.files().difference(before)].filter((file) => /^coverage\/Kit\.Tests\/coverage\.\d+\.info$/u.test(file));
    const sources = reports.flatMap((file) => readFileSync(join(workspace.root, file), 'utf-8').split('\n')).filter((row) => row.startsWith('SF:'));
    expect({ success, sources: sources.toSorted() }).toStrictEqual({ success: true, sources: ['SF:libs/Kit/Halves.cs', 'SF:products/Tool/Scales.cs'] });
});
