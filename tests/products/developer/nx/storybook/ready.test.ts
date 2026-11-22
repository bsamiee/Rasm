import { createNodes } from '@rasm/nx-storybook';
import { listen, it as nx } from '@rasm/testing/nx';
import { Option } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('storybook/package.json'), import.meta.resolve('@storybook/react-vite/package.json'), import.meta.resolve('react/package.json'), import.meta.resolve('react-dom/package.json'), import.meta.resolve('vite/package.json'), import.meta.resolve('@nx/storybook/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*"]}', 'package-lock.json': '{}', 'apps/site/package.json': '{"name":"site"}', 'apps/site/.storybook/main.ts': "export default { stories: ['../src/**/*.stories.tsx'] };\n" });
});

describe('readiness of storybook dev', { timeout: 60_000 }, () => {
    it('counts serve--storybook ready once the port its --port argument names listens, and not before', async ({ workspace }) => {
        const free = await listen();
        await free.close();
        const graph = await workspace.graph({ createNodes });
        const settled = workspace.poll(graph, 'site', 'serve--storybook', { port: free.port, __overrides_unparsed__: ['--port', `${free.port}`] });

        const early = await settled('1500 millis');
        await listen(free.port);

        expect([early, await settled('20 seconds')]).toStrictEqual([Option.none(), Option.some(0)]);
    });
});
