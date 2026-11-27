import { createNodes } from '@rasm/nx-storybook';
import { it as nx } from '@rasm/testing/nx';
import { supportedExtensions } from 'storybook/internal/common';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.write({ 'package.json': '{"name":"repo","type":"module","workspaces":["apps/*"]}', 'package-lock.json': '{}' });
});

describe('projects the Storybook targets join', () => {
    it('infers build--storybook beside a main config of each extension Storybook loads one from', async ({ workspace }) => {
        const graph = await workspace
            .write(
                Object.fromEntries(
                    supportedExtensions.flatMap((extension) => [
                        [`apps/${extension.slice(1)}/project.json`, JSON.stringify({ name: extension.slice(1) })],
                        [`apps/${extension.slice(1)}/.storybook/main${extension}`, extension.startsWith('.c') ? 'module.exports = { stories: [] };\n' : 'export default { stories: [] };\n'],
                    ]),
                ),
            )
            .graph({ createNodes });

        expect(Object.fromEntries(Object.values(graph.nodes).map(({ name, data: { targets = {} } }) => [`.${name}`, 'build--storybook' in targets]))).toStrictEqual(Object.fromEntries(supportedExtensions.map((extension) => [extension, true])));
    });

    it('infers a project only where a project.json or a package.json the workspaces list sits beside .storybook', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/listed/package.json': '{"name":"listed"}',
                'apps/listed/.storybook/main.ts': 'export default { stories: [] };\n',
                'tools/marked/project.json': '{"name":"marked"}',
                'tools/marked/.storybook/main.ts': 'export default { stories: [] };\n',
                'tools/unlisted/package.json': '{"name":"unlisted"}',
                'tools/unlisted/.storybook/main.ts': 'export default { stories: [] };\n',
            })
            .graph({ createNodes });

        expect(Object.fromEntries(Object.values(graph.nodes).map(({ data: { root, targets = {} } }) => [root, 'build--storybook' in targets]))).toStrictEqual({ 'apps/listed': true, 'tools/marked': true });
    });
});
