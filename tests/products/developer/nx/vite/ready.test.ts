import { createNodes } from '@rasm/nx-vite';
import { listen, it as nx } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('vite/package.json'), import.meta.resolve('@nx/vite/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","type":"module","workspaces":["apps/*"]}', 'package-lock.json': '{}', 'apps/web/package.json': '{"name":"web","type":"module"}', 'apps/web/index.html': '<!doctype html>\n' });
});

describe('readiness of the Vite servers', () => {
    it('waits for the ready signal of a config that loads nxReadySignal()', async ({ workspace }) => {
        const graph = await workspace.write({ 'apps/web/vite.config.ts': "import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';\n\nexport default { plugins: [nxReadySignal()] };\n" }).graph({ createNodes });

        expect([workspace.ready(graph, 'web', 'serve--vite')?.kind, workspace.ready(graph, 'web', 'preview--vite')?.kind]).toStrictEqual(['signal', 'signal']);
    });

    it.for([
        { target: 'serve--vite', block: 'server' },
        { target: 'preview--vite', block: 'preview' },
    ])('counts $target ready once the port of the $block block listens, and not before', async ({ target, block }, { workspace }) => {
        const [server, preview] = [await listen(), await listen()];
        await server.close();
        await preview.close();
        const graph = await workspace.write({ 'apps/web/vite.config.ts': `export default { server: { port: ${server.port}, strictPort: true }, preview: { port: ${preview.port}, strictPort: true } };\n` }).graph({ createNodes });
        const settled = workspace.poll(graph, 'web', target);

        const early = await settled('1500 millis');
        await listen(block === 'server' ? server.port : preview.port);

        expect([early, await settled('20 seconds')]).toStrictEqual([false, true]);
    });
});
