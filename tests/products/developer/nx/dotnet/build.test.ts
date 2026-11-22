import { execFileSync } from 'node:child_process';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { Build, execute } from '@rasm/nx-dotnet/executors/build';
import { it, outputs, targetOptions } from '@rasm/testing/nx';
import { expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

it('builds a project in a configuration its reference lacks, writing within the declared outputs of both and the reference in its own default', { timeout: 240_000 }, async ({ workspace }) => {
    const graph = await workspace
        .write({
            'libs/Core/Core.csproj': '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n</Project>\n',
            'libs/Core/Shapes.cs': 'namespace Core;\n\npublic static class Shapes;\n',
            'apps/App/App.csproj':
                '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n    <Configurations>Debug;Release;Interface</Configurations>\n  </PropertyGroup>\n  <ItemGroup>\n    <ProjectReference Include="../../libs/Core/Core.csproj" />\n  </ItemGroup>\n</Project>\n',
            'apps/App/Views.cs': 'namespace App;\n\npublic static class Views;\n',
        })
        .graph({ createNodes, createDependencies });
    const declared = [...outputs(graph, 'App', 'build--dotnet', 'interface'), ...outputs(graph, 'Core', 'build--dotnet')];
    execFileSync('dotnet', ['restore', 'apps/App/App.csproj'], { cwd: workspace.root, stdio: 'ignore' });
    const before = workspace.files();

    const { success } = await execute(await targetOptions(Build, graph, 'App', 'build--dotnet', 'interface'), workspace.context(graph, 'App', 'build--dotnet', 'interface'));

    const written = [...workspace.files().difference(before)];
    expect({ success, outside: written.filter((file) => !declared.some((output) => file.startsWith(`${output}/`))), configurations: new Set(written.map((file) => file.split('/').slice(0, 4).join('/'))) }).toStrictEqual({
        success: true,
        outside: [],
        configurations: new Set(['apps/App/bin/Interface', 'apps/App/obj/Interface', 'libs/Core/bin/Debug', 'libs/Core/obj/Debug']),
    });
});
