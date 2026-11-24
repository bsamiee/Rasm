import { readFileSync } from 'node:fs';
import { createNodes } from '@rasm/nx-pulumi';
import { it, unportable } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});

// --- [CONSTANTS] -----------------------------------------------------------------------

const NESTED = {
    'package.json': '{"name":"root","workspaces":["stacks/*/*","sites/*"]}',
    'stacks/cloud/infra/package.json': JSON.stringify({ name: 'infra', bin: './bin/stacks.mjs', dependencies: { '@pulumi/pulumi': '*' }, nx: { metadata: { pulumi: { hosts: 'hosting.config.ts' } } } }),
    'stacks/cloud/infra/bin/stacks.mjs': "import { writeFileSync } from 'node:fs';\nwriteFileSync(new URL('ran.txt', import.meta.url), process.argv.slice(2).join(' '));\n",
    'sites/web/package.json': '{"name":"web"}',
    'sites/web/hosting.config.ts': 'export default {};\n',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('Windows path separators', () => {
    it('hands Nx workspace-relative posix roots, inputs, and working folders', async ({ workspace }) => {
        const graph = await workspace.write(NESTED).graph({ createNodes });

        expect(unportable(graph)).toStrictEqual([]);
    });

    it('runs a program in a parent-relative folder from a package it hosts', async ({ workspace }) => {
        const graph = await workspace.write(NESTED).graph({ createNodes });

        const { success } = await workspace.run(graph, 'web', 'deploy--pulumi');

        expect(success).toBe(true);
        expect(readFileSync(`${workspace.root}/stacks/cloud/infra/bin/ran.txt`, 'utf-8')).toBe('deploy');
    });
});
