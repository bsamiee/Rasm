import { existsSync } from 'node:fs';
import { join } from 'node:path';
import type { CreateNodesContext } from '@nx/devkit';
import { createNodes } from '@rasm/nx-cargo';
import { TempFs } from '@rasm/testing/temp-fs';
import { ChildProcess } from 'effect/process';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));
vi.mock(import('effect/process'), { spy: true });

// --- [CONSTANTS] -----------------------------------------------------------------------

const metadata = ['metadata', '--format-version', '1', '--no-deps', '--manifest-path', 'Cargo.toml'];

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const cwd = process.cwd();
        const tempFs = new TempFs('cargo-analyzer');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            vi.unstubAllEnvs();
            process.chdir(cwd);
            tempFs.cleanup();
        });
        await tempFs.createFiles({ 'Cargo.toml': '[package]\nname = "analyzed"\nedition = "2024"\n', 'src/lib.rs': '' });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir }))
    .extend('spawn', () => vi.mocked(ChildProcess.make).mockClear());

describe('Cargo Analyzer', { timeout: 60_000 }, () => {
    it('should run cargo metadata with default options', async ({ tempFs, context, spawn }) => {
        await createNodes[1](['Cargo.toml'], undefined, context);

        expect(spawn).toHaveBeenCalledWith('cargo', metadata, expect.objectContaining({ cwd: tempFs.tempDir, stdin: 'ignore' }));
    });

    it('should handle cargo process failure', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
            'crates/sound/Cargo.toml': '[package]\nname = "sound"\nedition = "2024"\n',
            'crates/sound/src/lib.rs': '',
            'crates/broken/Cargo.toml': '[package\nname = ',
        });

        const result = createNodes[1](['Cargo.toml', 'crates/sound/Cargo.toml', 'crates/broken/Cargo.toml'], undefined, context);

        const refusal = 'exits 101: error: unclosed table, expected `]`\n --> crates/broken/Cargo.toml:1:9';
        await expect(result).rejects.toHaveProperty(['errors', 0, 0], 'Cargo.toml');
        await expect(result).rejects.toHaveProperty(['errors', 0, 1, 'message'], expect.stringContaining(`cargo metadata --format-version 1 --no-deps --manifest-path Cargo.toml ${refusal}`));
        await expect(result).rejects.toHaveProperty(['errors', 1, 1, 'message'], expect.stringContaining(`cargo metadata --format-version 1 --no-deps --manifest-path crates/broken/Cargo.toml ${refusal}`));
        await expect(result).rejects.toHaveProperty(['errors', 2, 1, 'message'], expect.stringContaining(`cargo metadata --format-version 1 --no-deps --manifest-path crates/sound/Cargo.toml ${refusal}`));
        await expect(result).rejects.toHaveProperty('partialResults', []);
    });
});

describe('command injection (NXC-4659)', { timeout: 60_000 }, () => {
    it('should pass a manifest path as a literal argument, never through a shell', async ({ tempFs, context, spawn }) => {
        const malicious = 'x; touch pwned';
        await tempFs.createFiles({
            [`${malicious}/Cargo.toml`]: '[package]\nname = "literal"\nedition = "2024"\n',
            [`${malicious}/src/lib.rs`]: '',
        });

        const results = await createNodes[1]([`${malicious}/Cargo.toml`], undefined, context);

        expect(spawn).toHaveBeenCalledWith('cargo', ['metadata', '--format-version', '1', '--no-deps', '--manifest-path', `${malicious}/Cargo.toml`], expect.objectContaining({ cwd: tempFs.tempDir }));
        expect(existsSync(join(tempFs.tempDir, 'pwned'))).toBe(false);
        expect(results[0]?.[0]).toBe(`${malicious}/Cargo.toml`);
        expect(Object.keys({ ...results[0]?.[1].projects })).toStrictEqual([malicious]);
    });
});
