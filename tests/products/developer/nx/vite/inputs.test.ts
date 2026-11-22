import { symlinkSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-vite';
import { it as nx, taskGraph } from '@rasm/testing/nx';
import { Predicate } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('vite/package.json'), import.meta.resolve('@nx/vite/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*","libs/*"]}', 'package-lock.json': '{}', 'apps/web/package.json': '{"name":"web","type":"module"}', 'apps/web/index.html': '<!doctype html>\n' });
});

describe('inputs of the Vite targets', { timeout: 60_000 }, () => {
    it('hashes the workspace files, installed packages, and workspace packages the config imports for its value, not its types', async ({ workspace }) => {
        workspace.write({
            'tools/vite/base.ts': 'export const port = 4300;\n',
            'apps/web/node_modules/dep/package.json': '{"name":"dep","type":"module","exports":"./index.js"}',
            'apps/web/node_modules/dep/index.js': 'export const plugin = { name: "dep" };\n',
            'apps/web/node_modules/typed/package.json': '{"name":"typed","type":"module","exports":"./index.js"}',
            'apps/web/node_modules/typed/index.js': 'export {};\n',
            'libs/shared/package.json': '{"name":"shared","type":"module","exports":"./index.js"}',
            'libs/shared/index.js': 'export const root = ".";\n',
            'apps/web/vite.config.ts': "import type { Shape } from 'typed';\nimport { plugin } from 'dep';\nimport { root } from 'shared';\nimport { port } from '../../tools/vite/base';\n\nexport default { root, plugins: [plugin], server: { port } };\n",
        });
        symlinkSync(join(workspace.root, 'libs/shared'), join(workspace.root, 'apps/web/node_modules/shared'), 'dir');

        const graph = await workspace.graph({ createNodes });
        const inputs = graph.nodes['web']?.data.targets?.['build--vite']?.inputs;

        expect(inputs).toStrictEqual(expect.arrayContaining(['{workspaceRoot}/tools/vite/base.ts', { input: 'default', projects: ['shared'] }, { externalDependencies: ['vite', 'dep'] }]));
    });

    it('hashes the compilerOptions of each tsconfig outside the project its extends chain reaches, leaving the root tsconfig to Nx', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'tsconfig.base.json': '{}',
                'tsconfig.shared.json': '{"extends":"./tsconfig.base.json"}',
                'apps/web/tsconfig.json': '{"extends":"../../tsconfig.shared.json"}',
                'apps/web/vite.config.ts': 'export default {};\n',
            })
            .graph({ createNodes });

        expect(graph.nodes['web']?.data.targets?.['build--vite']?.inputs?.filter((input) => Predicate.hasProperty(input, 'json'))).toStrictEqual([{ json: '{workspaceRoot}/tsconfig.shared.json', fields: ['compilerOptions'] }]);
    });

    it.for([
        { case: 'production inputs where nx.json names production', nx: '{"namedInputs":{"default":["{projectRoot}/**/*"],"production":["default"]}}', hashed: ['production', '^production'], unhashed: 'default' },
        { case: 'default inputs where nx.json names no production', nx: '{"namedInputs":{"default":["{projectRoot}/**/*"]}}', hashed: ['default', '^default'], unhashed: 'production' },
    ])('hashes the $case', async ({ nx: json, hashed, unhashed }, { workspace }) => {
        const graph = await workspace.write({ 'nx.json': json, 'apps/web/vite.config.ts': 'export default {};\n' }).graph({ createNodes });
        const inputs = graph.nodes['web']?.data.targets?.['build--vite']?.inputs;

        expect(inputs).toStrictEqual(expect.arrayContaining(hashed));
        expect(inputs).not.toContain(unhashed);
    });

    it('builds after the vite and tsc builds of each dependency and never after its storybook build, whose files build--vite then never hashes', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/web/package.json': '{"name":"web","type":"module","nx":{"implicitDependencies":["shared"]}}',
                'apps/web/vite.config.ts': 'export default {};\n',
                'libs/shared/package.json': '{"name":"shared","type":"module","nx":{"targets":{"build--vite":{"executor":"nx:noop"},"build--tsc":{"executor":"nx:noop"},"build--storybook":{"executor":"nx:noop"}}}}',
            })
            .graph({ createNodes });

        const { dependencies } = taskGraph(graph, ['web'], ['build--vite']);

        expect(dependencies['web:build--vite']?.map(({ id }) => id).toSorted()).toStrictEqual(['shared:build--tsc', 'shared:build--vite']);
    });
});
