import { createNodes } from '@rasm/nx-storybook';
import { refused } from '@rasm/testing/errors';
import { it as nx } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('storybook/package.json'), import.meta.resolve('@storybook/react-vite/package.json'), import.meta.resolve('react/package.json'), import.meta.resolve('react-dom/package.json'), import.meta.resolve('vite/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*"]}', 'package-lock.json': '{}', 'nx.json': '{"namedInputs":{"default":["{projectRoot}/**/*"],"production":["default"]}}', 'apps/site/package.json': '{"name":"site"}' });
});

describe('loading a Storybook main config across graph passes', { timeout: 60_000 }, () => {
    it('refuses a main config that throws, naming the config', async ({ workspace }) => {
        const graphed = workspace.write({ 'apps/site/.storybook/main.ts': "throw new Error('main refused');\n" }).graph({ createNodes });

        await expect(graphed).rejects.toSatisfy(refused(/^apps\/site\/\.storybook\/main\.ts does not load as a Storybook main config/u));
    });

    it('reads an edited main config again in the same process', async ({ workspace }) => {
        await workspace.write({ 'apps/site/.storybook/main.ts': "export default { stories: ['../src/**/*.stories.tsx'] };\n" }).graph({ createNodes });

        const graph = await workspace.write({ 'apps/site/.storybook/main.ts': "export default { stories: ['../docs/**/*.mdx'] };\n" }).graph({ createNodes });

        expect(graph.nodes['site']?.data.targets?.['build--storybook']?.inputs).toStrictEqual(expect.arrayContaining(['{workspaceRoot}/apps/site/docs/**/*.mdx']));
        expect(graph.nodes['site']?.data.targets?.['build--storybook']?.inputs).not.toContain('{workspaceRoot}/apps/site/src/**/*.stories.tsx');
    });
});
