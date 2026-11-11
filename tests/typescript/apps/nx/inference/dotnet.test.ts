import { assert } from '@effect/vitest';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import manifest from '@rasm/nx-dotnet/package.json' with { type: 'json' };
import { Effect } from 'effect';
import { cases, edges, inferred, roots, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const project = (items: string) => `<Project Sdk="Microsoft.NET.Sdk">\n    <PropertyGroup>\n        <TargetFramework>net11.0</TargetFramework>\n    </PropertyGroup>\n${items}</Project>\n`;

const subject: Subject = {
    plugin: { name: manifest.name, createNodes, createDependencies },
    state: ['obj'],
    files: {
        'package.json': '{ "name": "root" }\n',
        'Core/Core.csproj': project(''),
        'Core/Sheet.cs': 'namespace Core;\n\npublic sealed record Sheet(string Name);\n',
        'App/App.csproj': project('    <ItemGroup>\n        <ProjectReference Include="../Core/Core.csproj" />\n    </ItemGroup>\n'),
        'App/Program.cs': 'namespace App;\n\npublic static class Program\n{\n    public static string Name => new Core.Sheet("plan").Name;\n}\n',
    },
    broken: ['App/App.csproj', '<Project Sdk="Microsoft.NET.Sdk">\n    <PropertyGroup>\n'],
};

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('names each project file by its project name and links a ProjectReference from the project file that states it', () =>
        Effect.map(inferred(subject), (graph) => assert.deepOwnInclude({ ...roots(graph), edges: edges(graph) }, { Core: 'Core', App: 'App', edges: [{ source: 'App', target: 'Core', type: 'static' }] })),
    );
});
