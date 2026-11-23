import { createNodes } from '@rasm/nx-typescript';
import { it, typescript } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"root","workspaces":["libs/*"]}', 'package-lock.json': '' };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('eslint check inference', () => {
    it.for([
        { case: 'a root config governs a project.json project with a ts file', files: { 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'apps/app/project.json': '{"name":"app"}', 'apps/app/index.ts': 'export {};' }, linted: ['app'] },
        { case: 'a project config governs a workspace package', files: { 'libs/lib/eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/index.ts': 'export {};' }, linted: ['lib'] },
        { case: 'the config names json files a package holds', files: { 'eslint.config.mjs': "export default [{ files: ['**/executors.json'] }];\n", 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/executors.json': '{}' }, linted: ['lib'] },
        { case: 'no flat config exists', files: { 'apps/app/project.json': '{"name":"app"}', 'apps/app/index.ts': 'export {};' }, linted: [] },
        { case: 'a project holds no file the config lints', files: { 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'apps/app/project.json': '{"name":"app"}', 'apps/app/one.png': '...', 'apps/app/two.css': '...' }, linted: [] },
        { case: 'the config ignores every file of the project', files: { 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }, { ignores: ['apps/**'] }];\n", 'apps/app/project.json': '{"name":"app"}', 'apps/app/index.ts': 'export {};' }, linted: [] },
        {
            case: 'a project holds only the lintable files of a project nested in it',
            files: { 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'apps/app/project.json': '{"name":"app"}', 'apps/app/README.md': '# app', 'apps/app/nested/project.json': '{"name":"nested"}', 'apps/app/nested/index.ts': 'export {};' },
            linted: ['nested'],
        },
        { case: 'the workspace root holds a src folder', files: { 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'src/index.ts': 'export {};' }, linted: ['root'] },
        { case: 'the workspace root holds lintable files outside src and lib', files: { 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'scripts/index.ts': 'export {};' }, linted: [] },
    ])('lints the projects $linted when $case', async ({ files, linted }, { workspace }) => {
        const graph = await workspace
            .install(typescript, import.meta.resolve('eslint/package.json'))
            .write({ ...ROOT, ...files })
            .graph({ createNodes });

        expect(
            Object.values(graph.nodes)
                .filter(({ data: { targets = {} } }) => 'check--eslint' in targets)
                .map(({ name }) => name)
                .toSorted(),
        ).toStrictEqual(linted);
    });

    it('lints only the src folder of the workspace root, leaving nested projects to their own target', async ({ workspace }) => {
        workspace.install(typescript, import.meta.resolve('eslint/package.json')).write({
            ...ROOT,
            'eslint.config.mjs': "export default [{ files: ['**/*.ts'], rules: { 'no-debugger': 'error' } }];\n",
            'src/index.ts': 'export {};\n',
            'apps/app/project.json': '{"name":"app"}',
            'apps/app/index.ts': 'debugger;\n',
        });
        const graph = await workspace.graph({ createNodes });

        const [root, app] = [await workspace.run(graph, 'root', 'check--eslint'), await workspace.run(graph, 'app', 'check--eslint')];

        expect([root.success, app.success]).toStrictEqual([true, false]);
    });

    it('hashes each local module the config imports and each package any of them imports', async ({ workspace }) => {
        const graph = await workspace
            .install(typescript, import.meta.resolve('eslint/package.json'))
            .write({
                ...ROOT,
                'eslint.config.mjs': "import { defineConfig } from 'eslint/config';\nimport { files } from './tools/lint/files.mjs';\nimport 'node:path';\n\nexport default defineConfig({ files });\n",
                'tools/lint/files.mjs': "import manifest from 'typescript/package.json' with { type: 'json' };\n\nexport const files = manifest.name === 'typescript' ? ['**/*.ts'] : [];\n",
                'apps/app/project.json': '{"name":"app"}',
                'apps/app/index.ts': 'export {};',
            })
            .graph({ createNodes });

        expect(graph.nodes['app']?.data.targets?.['check--eslint']?.inputs).toStrictEqual(['default', '^default', '{workspaceRoot}/eslint.config.mjs', '{workspaceRoot}/tools/lint/files.mjs', { externalDependencies: ['eslint', 'typescript'] }]);
    });

    it('reads the imports of a module the config imports again after it changes', async ({ workspace }) => {
        workspace.install(typescript, import.meta.resolve('eslint/package.json')).write({
            ...ROOT,
            'eslint.config.mjs': "import { files } from './tools/lint/files.mjs';\n\nexport default [{ files }];\n",
            'tools/lint/files.mjs': "export const files = ['**/*.ts'];\n",
            'apps/app/project.json': '{"name":"app"}',
            'apps/app/index.ts': 'export {};',
        });
        await workspace.graph({ createNodes });

        const graph = await workspace.write({ 'tools/lint/files.mjs': "import 'typescript/package.json' with { type: 'json' };\n\nexport const files = ['**/*.ts'];\n" }).graph({ createNodes });

        expect(graph.nodes['app']?.data.targets?.['check--eslint']?.inputs).toContainEqual({ externalDependencies: ['eslint', 'typescript'] });
    });

    it('infers the target it read before once the compiler stops resolving, while the config, the modules it imports, and the project files are unchanged', async ({ workspace }) => {
        workspace.install(typescript, import.meta.resolve('eslint/package.json')).write({ ...ROOT, 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'apps/app/project.json': '{"name":"app"}', 'apps/app/index.ts': 'export {};' });
        const fresh = await workspace.graph({ createNodes });

        const cached = await workspace.remove('node_modules/typescript').graph({ createNodes });

        expect(cached.nodes['app']?.data.targets?.['check--eslint']).toStrictEqual(fresh.nodes['app']?.data.targets?.['check--eslint']);
    });

    it('hashes the files the project tsconfig extends outside the project, except the root tsconfig and installed presets', async ({ workspace }) => {
        const graph = await workspace
            .install(typescript, import.meta.resolve('eslint/package.json'))
            .write({
                ...ROOT,
                'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n",
                'tsconfig.base.json': '{"extends":"./tsconfig.root.json"}',
                'tsconfig.root.json': '{}',
                'tsconfig.shared.json': '{}',
                'node_modules/@some/preset/package.json': '{"name":"@some/preset"}',
                'node_modules/@some/preset/tsconfig.json': '{}',
                'apps/app/project.json': '{"name":"app"}',
                'apps/app/tsconfig.json': '{"extends":["../../tsconfig.shared.json","../../tsconfig.base.json","@some/preset/tsconfig.json"],"files":["index.ts"],"compilerOptions":{"noEmit":true}}',
                'apps/app/index.ts': 'export {};',
            })
            .graph({ createNodes });

        expect(graph.nodes['app']?.data.targets?.['check--eslint']?.inputs?.filter((input) => typeof input === 'string' && input.includes('tsconfig')).toSorted()).toStrictEqual(['{workspaceRoot}/tsconfig.root.json', '{workspaceRoot}/tsconfig.shared.json']);
    });
});
