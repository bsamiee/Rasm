import { symlinkSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-playwright';
import { it as nx } from '@rasm/testing/nx';
import { Predicate } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('@playwright/test/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*","libs/*"]}', 'package-lock.json': '{}', 'apps/e2e/package.json': '{"name":"e2e"}' });
});

describe('inputs of test--playwright', { timeout: 60_000 }, () => {
    it('hashes the workspace files, installed packages, and workspace packages the config loads', async ({ workspace }) => {
        workspace.write({
            'tools/pw/base.js': 'module.exports = {};\n',
            'apps/e2e/node_modules/dep/package.json': '{"name":"dep","main":"index.js"}',
            'apps/e2e/node_modules/dep/index.js': 'module.exports = {};\n',
            'libs/shared/package.json': '{"name":"shared","main":"index.js"}',
            'libs/shared/index.js': 'module.exports = {};\n',
            'apps/e2e/playwright.config.js': "require('../../tools/pw/base.js');\nrequire('dep');\nrequire('shared');\nmodule.exports = {};\n",
        });
        symlinkSync(join(workspace.root, 'libs/shared'), join(workspace.root, 'apps/e2e/node_modules/shared'), 'dir');

        const graph = await workspace.graph({ createNodes });

        expect(graph.nodes['e2e']?.data.targets?.['test--playwright']?.inputs).toStrictEqual(expect.arrayContaining(['{workspaceRoot}/tools/pw/base.js', { input: 'default', projects: ['shared'] }, { externalDependencies: ['@playwright/test', 'dep'] }]));
    });

    it('hashes the compilerOptions and extends of each tsconfig outside the project its extends chain reaches, leaving the root tsconfig to Nx', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'tsconfig.base.json': '{}',
                'tsconfig.shared.json': '{"extends":"./tsconfig.base.json"}',
                'apps/e2e/tsconfig.json': '{"extends":"../../tsconfig.shared.json"}',
                'apps/e2e/playwright.config.js': 'module.exports = {};\n',
            })
            .graph({ createNodes });

        expect(graph.nodes['e2e']?.data.targets?.['test--playwright']?.inputs?.filter((input) => Predicate.hasProperty(input, 'json'))).toStrictEqual([{ json: '{workspaceRoot}/tsconfig.shared.json', fields: ['compilerOptions', 'extends'] }]);
    });

    it.for([
        { case: 'production where nx.json names it', nx: '{"namedInputs":{"default":["{projectRoot}/**/*"],"production":["default"]}}', hashed: '^production', unhashed: '^default' },
        { case: 'default where nx.json names no production', nx: '{"namedInputs":{"default":["{projectRoot}/**/*"]}}', hashed: '^default', unhashed: '^production' },
    ])('hashes the $case input of each dependency', async ({ nx: json, hashed, unhashed }, { workspace }) => {
        const graph = await workspace.write({ 'nx.json': json, 'apps/e2e/playwright.config.js': 'module.exports = {};\n' }).graph({ createNodes });
        const inputs = graph.nodes['e2e']?.data.targets?.['test--playwright']?.inputs;

        expect(inputs).toContain(hashed);
        expect(inputs).not.toContain(unhashed);
    });
});
