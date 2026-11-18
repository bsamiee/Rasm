import { chmodSync } from 'node:fs';
import { join } from 'node:path';
import { env } from 'node:process';
import { createNodes } from '@rasm/nx-dotnet';
import { TempFs } from '@rasm/testing/temp-fs';
import { resetWorkspaceContext } from 'nx/src/utils/workspace-context';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));
vi.stubEnv('NX_DOTNET_PROJECT_GRAPH_TIMEOUT', '1');

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const [tempFs, previous] = [new TempFs('dotnet-timeout'), process.cwd()];
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            resetWorkspaceContext();
            tempFs.cleanup();
        });
        await tempFs.createFiles({ 'a/a.csproj': '<Project Sdk="Microsoft.NET.Sdk" />\n', 'bin/dotnet': '#!/bin/sh\nexec sleep 20\n' });
        return tempFs;
    })
    .extend('analyzeProjects', ({ tempFs }) => {
        chmodSync(join(tempFs.tempDir, 'bin/dotnet'), 0o755);
        vi.stubEnv('PATH', [join(tempFs.tempDir, 'bin'), env['PATH']].join(':'));
        return (files: readonly string[]) => createNodes[1](files, undefined, { nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir });
    });

describe('MsbuildAnalyzer timeout', { timeout: 10_000 }, () => {
    it('should stop an analyzer running past NX_DOTNET_PROJECT_GRAPH_TIMEOUT seconds and name the variable', async ({ analyzeProjects }) => {
        await expect(analyzeProjects(['a/a.csproj'])).rejects.toHaveProperty(['errors', 0, 1, 'message'], expect.stringMatching(/ran past 1 seconds, set NX_DOTNET_PROJECT_GRAPH_TIMEOUT/u));
    });
});
