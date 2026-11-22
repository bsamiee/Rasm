import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { refused } from '@rasm/testing/errors';
import { it, pathed } from '@rasm/testing/nx';
import { expect, onTestFinished, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));
vi.stubEnv('NX_DOTNET_PROJECT_GRAPH_TIMEOUT', '1');

// --- [COMPOSITION] ---------------------------------------------------------------------

it('refuses an analyzer running past NX_DOTNET_PROJECT_GRAPH_TIMEOUT seconds, naming the variable', async ({ workspace }) => {
    const folder = mkdtempSync(join(tmpdir(), 'dotnet-hanging-'));
    onTestFinished(() => rmSync(folder, { recursive: true }));
    workspace.write({ 'apps/App/App.csproj': '<Project Sdk="Microsoft.NET.Sdk" />\n' });
    pathed(folder, { dotnet: 'exec /bin/sleep 20' });

    await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/ran past 1 seconds, set NX_DOTNET_PROJECT_GRAPH_TIMEOUT to a higher count of seconds$/u));
});
