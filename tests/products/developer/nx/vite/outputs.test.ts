import { existsSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-vite';
import { it as nx } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('vite/package.json'), import.meta.resolve('@nx/vite/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","type":"module","workspaces":["apps/*"]}', 'package-lock.json': '{}', 'apps/web/package.json': '{"name":"web","type":"module"}', 'apps/web/index.ts': 'export const value = 1;\n' });
});

describe('declared outputs of build--vite', () => {
    const lib = "lib: { entry: 'index.ts', formats: ['es'] }";

    it.for([
        { case: 'in the default outDir', options: {}, config: `export default { build: { ${lib} } };\n` },
        { case: 'in the outDir a plugin sets for the environment', options: {}, config: `export default { build: { ${lib} }, plugins: [{ name: 'routes', configEnvironment: () => ({ build: { outDir: '.output/client' } }) }] };\n` },
        { case: 'under an outputPath outside the project', options: { outputPath: '.artifacts/vite/{projectName}' }, config: `export default { build: { ${lib}, outDir: 'lib' } };\n` },
    ])('hold every file vite build writes $case', async ({ options, config }, { workspace }) => {
        const graph = await workspace.write({ 'apps/web/vite.config.ts': config }).graph({ createNodes }, options);
        const before = workspace.files('node_modules', 'tmp');

        const { success } = await workspace.run(graph, 'web', 'build--vite');

        const written = workspace.files('node_modules', 'tmp').difference(before);
        expect(success).toBe(true);
        expect([...written]).not.toStrictEqual([]);
        expect([...written.difference(workspace.cached(graph, 'web', 'build--vite'))]).toStrictEqual([]);
    });

    it('empties the outputPath folder of files an earlier build left before writing', async ({ workspace }) => {
        const graph = await workspace.write({ 'apps/web/vite.config.ts': `export default { build: { ${lib} } };\n`, '.artifacts/vite/web/stale.js': 'export {};\n' }).graph({ createNodes }, { outputPath: '.artifacts/vite/{projectName}' });

        await workspace.run(graph, 'web', 'build--vite');

        expect(existsSync(join(workspace.root, '.artifacts/vite/web/stale.js'))).toBe(false);
    });

    it('infers the outputs without running the configResolved hook a build acts in', async ({ workspace }) => {
        await workspace
            .write({
                'apps/web/.output/kept.json': '{}',
                'apps/web/vite.config.ts': `import { rmSync } from 'node:fs';\nimport { join } from 'node:path';\n\nexport default { build: { ${lib} }, plugins: [{ name: 'cleans', configResolved: ({ command, root }) => command === 'build' && rmSync(join(root, '.output'), { recursive: true }) }] };\n`,
            })
            .graph({ createNodes });

        expect(existsSync(join(workspace.root, 'apps/web/.output/kept.json'))).toBe(true);
    });
});
