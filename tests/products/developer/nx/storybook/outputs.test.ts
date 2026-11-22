import { createNodes } from '@rasm/nx-storybook';
import { it as nx } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('storybook/package.json'), import.meta.resolve('@storybook/react-vite/package.json'), import.meta.resolve('react/package.json'), import.meta.resolve('react-dom/package.json'), import.meta.resolve('vite/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*"]}', 'package-lock.json': '{}', 'apps/site/package.json': '{"name":"site"}' });
});

describe('declared outputs of build--storybook', { timeout: 120_000 }, () => {
    it('hold every file storybook build writes under an outputPath outside the project', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/site/label.stories.tsx': "export default { title: 'Label', component: () => 'label' };\n\nexport const Shown = {};\n",
                'apps/site/.storybook/main.ts': "export default { stories: ['../*.stories.tsx'], framework: { name: '@storybook/react-vite', options: {} }, core: { disableTelemetry: true } };\n",
            })
            .graph({ createNodes }, { outputPath: '.artifacts/storybook/{projectName}' });
        const before = workspace.files('node_modules', 'tmp');

        const { success } = await workspace.run(graph, 'site', 'build--storybook');

        const written = workspace.files('node_modules', 'tmp').difference(before);
        expect(success).toBe(true);
        expect([...written]).toContain('.artifacts/storybook/site/index.html');
        expect([...written.difference(workspace.cached(graph, 'site', 'build--storybook'))]).toStrictEqual([]);
    });
});
