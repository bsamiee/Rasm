import { execFileSync } from 'node:child_process';
import { join } from 'node:path';
import type { CreateNodesContext } from '@nx/devkit';
import { createNodes } from '@rasm/nx-uv';
import { TempFs } from '@rasm/testing/temp-fs';
import { ChildProcess } from 'effect/process';
import { updateFilesInContext } from 'nx/src/utils/workspace-context';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));
vi.mock(import('effect/process'), { spy: true });

// --- [CONSTANTS] -----------------------------------------------------------------------

const analysis = ['workspace', 'metadata', '--frozen', '--preview-features', 'workspace-metadata', '--output-version', 'preview', '--quiet'];

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const previous = process.cwd();
        const tempFs = new TempFs('uv-analysis');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            tempFs.cleanup();
        });
        await tempFs.createFiles({
            'pyproject.toml': '[project]\nname = "uv-tutorial"\nversion = "0.1.0"\ndependencies = []\n\n[tool.uv.workspace]\nmembers = ["proj/*"]\n',
            'proj/library/pyproject.toml': '[project]\nname = "library"\nversion = "0.1.0"\ndependencies = []\n',
        });
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: tempFs.tempDir });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir }))
    .extend('spawn', () => vi.mocked(ChildProcess.make).mockClear());

describe('uv analysis', { timeout: 60_000 }, () => {
    it('should run uv analysis with default options', async ({ context, spawn }) => {
        await createNodes[1](['uv.lock'], undefined, context);

        expect(spawn).toHaveBeenCalledWith('uv', analysis, expect.objectContaining({ cwd: context.workspaceRoot, stdin: 'ignore', detached: false }));
    });

    it('should run uv analysis in the folder of each lock', async ({ tempFs, context, spawn }) => {
        await tempFs.createFiles({ 'nested/proj/pyproject.toml': '[project]\nname = "nested"\nversion = "0.1.0"\ndependencies = []\n' });
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: join(tempFs.tempDir, 'nested/proj') });

        await createNodes[1](['uv.lock', 'nested/proj/uv.lock'], undefined, context);

        expect(spawn).toHaveBeenCalledWith('uv', analysis, expect.objectContaining({ cwd: tempFs.tempDir }));
        expect(spawn).toHaveBeenCalledWith('uv', analysis, expect.objectContaining({ cwd: join(tempFs.tempDir, 'nested/proj') }));
    });

    it('should read the analysis of an unchanged workspace from the cache without running uv', async ({ context, spawn }) => {
        const cold = await createNodes[1](['uv.lock'], undefined, context);
        const warm = await createNodes[1](['uv.lock'], undefined, context);

        expect(warm).toStrictEqual(cold);
        expect(spawn).toHaveBeenCalledOnce();
    });

    it('should run uv analysis again once a file uv reads changes', async ({ tempFs, context, spawn }) => {
        await createNodes[1](['uv.lock'], undefined, context);
        tempFs.appendFile('proj/library/pyproject.toml', '\n[tool.uv]\npackage = false\n');
        updateFilesInContext(tempFs.tempDir, ['proj/library/pyproject.toml'], []);

        await createNodes[1](['uv.lock'], undefined, context);

        expect(spawn).toHaveBeenCalledTimes(2);
    });
    it('should handle uv process failure', async ({ tempFs, context }) => {
        tempFs.writeFile('proj/library/pyproject.toml', '[project\nname = ');

        const result = createNodes[1](['uv.lock'], undefined, context);

        await expect(result).rejects.toHaveProperty(['errors', 0, 0], 'uv.lock');
        await expect(result).rejects.toHaveProperty(['errors', 0, 1, 'message'], expect.stringMatching(/^uv workspace metadata --frozen --preview-features workspace-metadata --output-version preview --quiet exits 2: error: Failed to parse: proj\/library\/pyproject\.toml/u));
    });

    it('should handle spawn error', async ({ context, spawn }) => {
        spawn.mockReturnValueOnce(ChildProcess.make('rasm-uv-absent', analysis));

        await expect(createNodes[1](['uv.lock'], undefined, context)).rejects.toHaveProperty(['errors', 0, 1, 'message'], 'NotFound: ChildProcess.spawn (rasm-uv-absent workspace metadata --frozen --preview-features workspace-metadata --output-version preview --quiet)');
    });

    it('should handle a reply in another metadata schema', async ({ context, spawn }) => {
        spawn.mockReturnValueOnce(ChildProcess.make('echo', ['{"schema":{"version":"v1"}}']));

        await expect(createNodes[1](['uv.lock'], undefined, context)).rejects.toHaveProperty(['errors', 0, 1, 'message'], 'Expected "preview"\n  at ["schema"]["version"]');
    });

    it('should keep the projects of every other lock when one lock fails', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'broken/pyproject.toml': '[project\nname = ', 'broken/uv.lock': 'version = 1\n' });

        const result = createNodes[1](['uv.lock', 'broken/uv.lock'], undefined, context);

        await expect(result).rejects.toHaveProperty(['errors', 0, 0], 'broken/uv.lock');
        await expect(result).rejects.toHaveProperty(['errors', 0, 1, 'message'], expect.stringContaining('exits 2: error: Failed to parse: pyproject.toml'));
        await expect(result).rejects.toHaveProperty(['partialResults', 0, 1, 'projects', 'proj/library', 'name'], 'library');
    });
});
