import { execFileSync } from 'node:child_process';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { it, outputs } from '@rasm/testing/nx';
import { expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PACKAGES = join(dirname(fileURLToPath(import.meta.resolve('@rasm/nx-dotnet/package.json'))), '../../../../.cache/nuget/packages');
const FILES = {
    'global.json': JSON.stringify({ test: { runner: 'Microsoft.Testing.Platform' } }),
    'NuGet.config': `<configuration>\n  <config>\n    <add key="globalPackagesFolder" value="${PACKAGES}" />\n  </config>\n</configuration>\n`,
    'apps/Tests/Tests.csproj':
        '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n    <OutputType>Exe</OutputType>\n    <IsTestProject>true</IsTestProject>\n    <RollForward>Major</RollForward>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include="xunit.v3.mtp-v2" Version="4.0.3" />\n    <PackageReference Include="coverlet.MTP" Version="10.1.1" />\n  </ItemGroup>\n</Project>\n',
    'apps/Tests/Sums.cs': 'namespace Tests;\n\npublic sealed class Sums {\n    [Xunit.Fact]\n    public void Adds() => Xunit.Assert.Equal(3, 1 + 2);\n}\n',
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
