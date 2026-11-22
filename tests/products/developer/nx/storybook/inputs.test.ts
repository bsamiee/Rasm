import { symlinkSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-storybook';
import { it as nx } from '@rasm/testing/nx';
import { Predicate } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('storybook/package.json'), import.meta.resolve('@storybook/react-vite/package.json'), import.meta.resolve('react/package.json'), import.meta.resolve('react-dom/package.json'), import.meta.resolve('vite/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*","libs/*"]}', 'package-lock.json': '{}', 'apps/site/package.json': '{"name":"site"}' });
});

describe('inputs of the Storybook targets', { timeout: 60_000 }, () => {
    const production = '{"namedInputs":{"default":["{projectRoot}/**/*"],"production":["default","!{projectRoot}/**/*.spec.ts"]}}';

    it('hashes each story glob inside the workspace that the main config states, a folder by the files Storybook indexes in it', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'nx.json': production,
                'apps/site/src/button.stories.tsx': 'export default {};\n',
                'apps/site/.storybook/main.ts': "export default { stories: ['../src', '../docs/**/*.mdx', '../../../../outside/*.stories.tsx'], framework: '@storybook/react-vite' };\n",
            })
            .graph({ createNodes });

        expect(graph.nodes['site']?.data.targets?.['build--storybook']?.inputs?.filter((input) => Predicate.isString(input) && input.startsWith('{workspaceRoot}/apps/site/') && !input.includes('.storybook'))).toStrictEqual([
            '{workspaceRoot}/apps/site/src/**/*.@(mdx|stories.@(js|jsx|mjs|ts|tsx))',
            '{workspaceRoot}/apps/site/docs/**/*.mdx',
        ]);
    });

    it.for([
        { case: 'names production', nx: production },
        { case: 'names no production', nx: '{"namedInputs":{"default":["{projectRoot}/**/*"]}}' },
    ])('hashes a story glob outside the project where nx.json $case', async ({ nx: json }, { workspace }) => {
        const graph = await workspace.write({ 'nx.json': json, 'apps/site/.storybook/main.ts': "export default { stories: ['../../../libs/ui/**/*.stories.tsx'] };\n" }).graph({ createNodes });

        expect(graph.nodes['site']?.data.targets?.['build--storybook']?.inputs).toContain('{workspaceRoot}/libs/ui/**/*.stories.tsx');
    });

    it.for([
        { case: 'production inputs and the .storybook folder where nx.json names production', nx: production, hashed: ['production', '^production', '{workspaceRoot}/{projectRoot}/.storybook/**/*'], unhashed: 'default' },
        { case: 'default inputs where nx.json names no production', nx: '{"namedInputs":{"default":["{projectRoot}/**/*"]}}', hashed: ['default', '^default'], unhashed: 'production' },
    ])('hashes the $case', async ({ nx: json, hashed, unhashed }, { workspace }) => {
        const graph = await workspace.write({ 'nx.json': json, 'apps/site/.storybook/main.ts': "export default { stories: ['../src/**/*.stories.tsx'] };\n" }).graph({ createNodes });
        const inputs = graph.nodes['site']?.data.targets?.['build--storybook']?.inputs;

        expect(inputs).toStrictEqual(expect.arrayContaining(hashed));
        expect(inputs).not.toContain(unhashed);
    });

    it('hashes the workspace files, framework and addon packages, and workspace addon packages the main config loads', async ({ workspace }) => {
        workspace.write({
            'tools/storybook/shared.ts': "export const stories = ['../src/**/*.stories.tsx'];\n",
            'libs/addon/package.json': '{"name":"addon","type":"module","exports":"./preset.js"}',
            'libs/addon/preset.js': 'export {};\n',
            'apps/site/.storybook/local.ts': "export const docs = '@storybook/react-vite';\n",
            'apps/site/.storybook/main.ts': "import { stories } from '../../../tools/storybook/shared.ts';\nimport { docs } from './local.ts';\n\nexport default { stories, framework: { name: docs, options: {} }, addons: ['addon'] };\n",
        });
        symlinkSync(join(workspace.root, 'libs/addon'), join(workspace.root, 'node_modules/addon'), 'dir');

        const graph = await workspace.graph({ createNodes });
        const inputs = graph.nodes['site']?.data.targets?.['build--storybook']?.inputs;

        expect(inputs).toStrictEqual(expect.arrayContaining(['{workspaceRoot}/tools/storybook/shared.ts', { input: 'default', projects: ['addon'] }, { input: 'default', projects: ['addon'], dependencies: true }, { externalDependencies: ['storybook', '@storybook/react-vite'] }]));
        expect(inputs).not.toContain('{workspaceRoot}/apps/site/.storybook/local.ts');
    });

    it('hashes the compilerOptions of each tsconfig outside the project its extends chain reaches, leaving the root tsconfig to Nx', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'tsconfig.base.json': '{}',
                'tsconfig.shared.json': '{"extends":"./tsconfig.base.json"}',
                'apps/site/tsconfig.json': '{"extends":"../../tsconfig.shared.json"}',
                'apps/site/.storybook/main.ts': "export default { stories: ['../src/**/*.stories.tsx'] };\n",
            })
            .graph({ createNodes });

        expect(graph.nodes['site']?.data.targets?.['build--storybook']?.inputs?.filter((input) => Predicate.hasProperty(input, 'json'))).toStrictEqual([{ json: '{workspaceRoot}/tsconfig.shared.json', fields: ['compilerOptions'] }]);
    });
});
