import { createNodes } from '@rasm/nx-vitest';
import { unportable } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';
import { it } from '#/installed';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('@rasm/nx-vitest under Windows path separators', () => {
    it('hands Nx posix roots, inputs, and outputs for listed, nested, and parent-relative paths', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'package.json': '{"name":"repo","workspaces":["packages/*","libs/*/*"]}',
                'vitest.config.ts': "export default { test: { projects: ['packages/*'] } };\n",
                'packages/a/package.json': '{"name":"a"}',
                'packages/b/package.json': '{"name":"b"}',
                'libs/deep/c/package.json': '{"name":"c"}',
                'libs/deep/c/vitest.config.ts': "export default { test: { setupFiles: ['../../../tools/vitest/setup.mts'] } };\n",
                'tools/vitest/setup.mts': '',
                'tools/vitest/tsconfig.json': '{"extends":"../../tsconfig.shared.json"}',
                'tsconfig.shared.json': '{"compilerOptions":{}}',
            })
            .graph({ createNodes }, { outputPath: '.artifacts/vitest/{projectName}' });

        expect(Object.fromEntries(Object.values(graph.nodes).flatMap(({ name, data }) => (data.targets?.['test--vitest'] === undefined ? [] : [[name, data.root]])))).toStrictEqual({ a: 'packages/a', b: 'packages/b', c: 'libs/deep/c' });
        expect(graph.nodes['c']?.data.targets?.['test--vitest']?.inputs).toContainEqual({ json: '{workspaceRoot}/tsconfig.shared.json', fields: ['compilerOptions'] });
        expect(unportable(graph)).toStrictEqual([]);
    });
});
