import { createNodes } from '@rasm/nx-vite';
import { refused } from '@rasm/testing/errors';
import { it as nx } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('vite/package.json'), import.meta.resolve('@nx/vite/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*"]}', 'package-lock.json': '{}', 'apps/web/package.json': '{"name":"web","type":"module"}' });
});

describe('loading a Vite config across graph passes', { timeout: 60_000 }, () => {
    const lib = "lib: { entry: 'index.ts', formats: ['es'] }";

    it('refuses a config that throws in build mode, naming the config', async ({ workspace }) => {
        const graphed = workspace.write({ 'apps/web/vite.config.ts': "throw new Error('config refused');\n" }).graph({ createNodes });

        await expect(graphed).rejects.toSatisfy(refused(/^apps\/web\/vite\.config\.ts does not load for vite build/u));
    });

    it('resolves an edited config again in the same process', async ({ workspace }) => {
        await workspace.write({ 'apps/web/vite.config.ts': `export default { build: { ${lib}, outDir: 'dist' } };\n` }).graph({ createNodes });

        const graph = await workspace.write({ 'apps/web/vite.config.ts': `export default { build: { ${lib}, outDir: 'out' } };\n` }).graph({ createNodes });

        expect(graph.nodes['web']?.data.targets?.['build--vite']?.outputs).toStrictEqual(['{projectRoot}/out']);
    });

    it('resolves the config again once a workspace file it imports from outside the project changes', async ({ workspace }) => {
        await workspace.write({ 'tools/vite/out.ts': "export const outDir = 'dist';\n", 'apps/web/vite.config.ts': `import { outDir } from '../../tools/vite/out';\n\nexport default { build: { ${lib}, outDir } };\n` }).graph({ createNodes });

        const graph = await workspace.write({ 'tools/vite/out.ts': "export const outDir = 'out';\n" }).graph({ createNodes });

        expect(graph.nodes['web']?.data.targets?.['build--vite']?.outputs).toStrictEqual(['{projectRoot}/out']);
    });
});
