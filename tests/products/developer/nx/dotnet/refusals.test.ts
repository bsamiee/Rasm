import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { refused } from '@rasm/testing/errors';
import { pathed, refusing } from '@rasm/testing/nx';
import { describe, expect, onTestFinished, vi } from 'vitest';
import { it, node, report, STUB } from '#/analyzer';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECT = '<Project Sdk="Microsoft.NET.Sdk" />\n';

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('analyzer process', () => {
    it.for(refusing('dotnet'))('refuses the graph naming the analyzer run on $case', async ({ scripts, refusal }, { workspace }) => {
        const folder = mkdtempSync(join(tmpdir(), 'dotnet-refusing-'));
        onTestFinished(() => rmSync(folder, { recursive: true }));
        workspace.write({ 'apps/App/App.csproj': PROJECT });
        pathed(folder, scripts);

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(refusal));
    });
});

describe('analyzer report', () => {
    it('refuses a report it cannot decode, naming the rebuild of the analyzer', async ({ workspace }) => {
        workspace.write({ 'apps/App/App.csproj': PROJECT, [STUB.report]: 'Unhandled exception. System.IO.FileNotFoundException' });

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/printed a report this plugin cannot read, as \S+MsbuildAnalyzer\.dll predates the plugin, run mise deps msbuild-analyzer to rebuild it: /u));
    });

    it('names the field a report of an older analyzer lacks, and leaves the report itself out', async ({ workspace }) => {
        workspace.write({ 'apps/App/App.csproj': PROJECT, [STUB.report]: JSON.stringify({ nodes: [], failures: [{ file: 'secret.csproj', message: 'held' }] }) });

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/run mise deps msbuild-analyzer to rebuild it: (?!.*secret\.csproj).*\["evaluated"\]/su));
    });

    it('refuses each project MSBuild failed to evaluate with its message', async ({ workspace }) => {
        workspace.write({ 'apps/App/App.csproj': PROJECT, 'apps/Bad/Bad.csproj': '<Project Sdk="Microsoft.NET.Sdk">\n', ...report([node('apps/App/App.csproj')], { failures: [{ file: 'apps/Bad/Bad.csproj', message: 'MSB4025: The project file could not be loaded. Unexpected end of file.' }] }) });

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/^MSB4025: The project file could not be loaded\. Unexpected end of file\.$/u));
    });
});
